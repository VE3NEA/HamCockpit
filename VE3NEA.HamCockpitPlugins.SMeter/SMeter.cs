using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Threading;
using VE3NEA.HamCockpit.DspFun;

namespace VE3NEA.HamCockpitPlugins.SMeter
{
  [Export(typeof(IPlugin))]
  [Export(typeof(IVisualPlugin))]
  class SMeter : IPlugin, IVisualPlugin
  {
    private readonly IDspPipeline pipeline;
    private SMeterControl panel;
    private float value = -100;
    private int sampleCount;
    private readonly SynchronizationContext context = SynchronizationContext.Current;

    //Importing constructor allows us to get IPluginHost when the plugin is constructed
    [ImportingConstructor]
    SMeter([Import(typeof(IPluginHost))] IPluginHost host)
    {
      pipeline = host.DspPipeline;
    }

    //implementation of IPlugin:
    //this plugin does not have a settings object, toolstrip or status item
    #region IPlugin
    public string Name => "S-Meter";
    public string Author => "VE3NEA";
    public bool Enabled { get; set; }
    public object Settings { get => null; set { } }
    public ToolStrip ToolStrip => null;
    public ToolStripItem StatusItem => null;
    #endregion

    //implementation of IVisualPlugin:
    //create and destroy the S-meter visual panel
    #region IVisualPlugin

    //only one instance of the panel is allowed to be shown 
    public bool CanCreatePanel => panel == null;

    //create an s-meter panel, subscribe to the pipeline events
    public UserControl CreatePanel()
    {
      panel = new SMeterControl { Name = "S-Meter" };
      pipeline.StatusChanged += StatusEventhandler;
      pipeline.ProcessedAudio.SamplesAvailable += SamplesAvailableEventHandler;
      return panel;
    }

    //destroy the panel, unsubscribe from the events
    public void DestroyPanel(UserControl panel)
    {
      pipeline.StatusChanged -= StatusEventhandler;
      pipeline.ProcessedAudio.SamplesAvailable -= SamplesAvailableEventHandler;
      this.panel = null;
    }
    #endregion

    //show "no signal" when audio data are not available
    private void StatusEventhandler(object sender, EventArgs e)
    {
      if (!pipeline.ProcessedAudio.IsAvailable) panel?.ShowValue(-100);
    }

    //data arrived on the audio thread. 
    //no signal processing should be done on this thread
    private void SamplesAvailableEventHandler(object sender, SamplesAvailableEventArgs e)
    {
      if (!pipeline.ProcessedAudio.IsAvailable) return;

      //make a copy of the data
      //the data may come in different formats, get only what is needed
      var format = pipeline.ProcessedAudio.Format;
      int stride = format.Channels * (format.IsComplex ? 2 : 1);
      int count = e.Count / stride;
      float[] data = new float[count];
      for (int i = 0; i < count; i++) data[i] = e.Data[e.Offset + i * stride];
      
      //tell the main thread to process and display the data
      context.Post(s => ProcessData(data), null);
    }

    private void ProcessData(float[] data)
    {
      if (!pipeline.ProcessedAudio.IsAvailable) return;

      //calculate the value to display
      foreach (var v in data) value += 0.0007f * (v * v - value);
      value = Math.Max(0, value);

      //the block size is not known in advance
      //count the samples and update the control 10 times per second
      sampleCount += data.Length;
      if (sampleCount > pipeline.ProcessedAudio.Format.SamplingRate / 10)
      {
        //if previous stages apply any gain, subtract it from the reading
        float dB = Dsp.ToDb(value) - pipeline.ProcessedAudio.Format.TotalGain;
        panel?.ShowValue(Math.Max(-100, Math.Min(0, dB)));
        sampleCount = 0;
      }
    }
  }
}
