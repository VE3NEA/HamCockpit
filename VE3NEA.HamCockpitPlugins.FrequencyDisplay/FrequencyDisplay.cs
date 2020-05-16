using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using VE3NEA.HamCockpit.PluginAPI;
using System.Windows.Forms;
using System.Drawing;
using VE3NEA.HamCockpit.SharedControls;

namespace VE3NEA.HamCockpitPlugins.FrequencyDisplay
{
  [Export(typeof(IPlugin))]
  [Export(typeof(IVisualPlugin))]
  class FrequencyDisplay : IPlugin, IVisualPlugin
  {
#pragma warning disable 0649
    [Import(typeof(IBandPlan))]
    internal IBandPlan BandPlan;
#pragma warning restore 0649

    private IPluginHost host;
    private FrequencyDisplayPanel panel;

    [ImportingConstructor]
    FrequencyDisplay([Import(typeof(IPluginHost))] IPluginHost host)
    {
      this.host = host;
      host.DspPipeline.Tuner.Tuned += TunedEventHandler;
      host.DspPipeline.StatusChanged += TunedEventHandler;
      host.DspPipeline.Transmitter.Tuned += TunedEventHandler;
      host.DspPipeline.Transmitter.SettingsChanged += TunedEventHandler;
    }

    private void TunedEventHandler(object sender, EventArgs e)
    {
      panel?.UpdateDisplayedInfo();
    }

    //IPlugin
    public string Name => "Frequency Display";
    public string Author => "VE3NEA";
    public bool Enabled { get; set; }
    public object Settings { get => null; set { } }
    public ToolStrip ToolStrip => null;
    public ToolStripItem StatusItem => null;

    //IVisualPlugin
    public bool CanCreatePanel => panel == null;

    public UserControl CreatePanel()
    {
      panel = new FrequencyDisplayPanel();
      panel.Name = "Main Receiver";
      panel.bandplan = BandPlan;
      panel.pipeline = host.DspPipeline;

      TunedEventHandler(null, null);

      return panel;
    }

    public void DestroyPanel(UserControl panel)
    {
      this.panel = null;
    }
  }
}
