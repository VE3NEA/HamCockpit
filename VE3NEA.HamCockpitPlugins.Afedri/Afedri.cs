using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VE3NEA.HamCockpit.DspFun;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpitPlugins.Afedri
{
  [Export(typeof(IPlugin))]
  [Export(typeof(ISignalSource))]
  public class Afedri : IPlugin, ISignalSource
  {
    public const int DEFAULT_SAMPLING_RATE = 96000;

    private Settings settings = new Settings();
    private AfedriDevice device = new AfedriDevice();
    private readonly RingBuffer buffer = new RingBuffer(DEFAULT_SAMPLING_RATE);

    private Thread iqThread;
    private bool stopping;
    private readonly SynchronizationContext context = SynchronizationContext.Current;


    public Afedri()
    {
    settings.SamplingRate = DEFAULT_SAMPLING_RATE;
    Format = new SignalFormat(DEFAULT_SAMPLING_RATE, true, settings.IsSync(), settings.ChannelCount(), 
      -(int)(DEFAULT_SAMPLING_RATE * 0.47), (int)(DEFAULT_SAMPLING_RATE * 0.47), 0);

      buffer.SamplesAvailable += (o, e) => SamplesAvailable?.Invoke(this, e);
    }




    //----------------------------------------------------------------------------------------------
    //                                  plugin interfaces
    //----------------------------------------------------------------------------------------------
    #region IPlugin
    public string Name => "Afedri-822x SDR";
    public string Author => "VE3NEA";
    public bool Enabled { get; set; }
    public object Settings { get => settings; set => setSettings(value as Settings); }
    public ToolStrip ToolStrip => null;
    public ToolStripItem StatusItem => null;
    #endregion

    #region ISignalSource 
    public void Initialize() { }
    public bool Active { get => device.IsActive(); set => SetActive(value); }
    public event EventHandler<StoppedEventArgs> Stopped;
    #endregion

    #region ISampleStream
    public SignalFormat Format { get; private set; }
    public int Read(float[] buffer, int offset, int count) { return this.buffer.Read(buffer, offset, count); }
    public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;
    #endregion

    #region ITuner
    public long GetDialFrequency(int channel = 0)
    {
      return settings.Frequencies[channel];
    }
    public void SetDialFrequency(long frequency, int channel = 0)
    {
      settings.Frequencies[channel] = frequency;
      if (Active)
        try
        {
          //send to radio
          device.SetFrequency(frequency, channel);
          //notify host application
          Tuned?.Invoke(this, new EventArgs());
        }
        catch (Exception e)
        {
          device.Stop();
          var exception = new Exception($"Afedri command CI_FREQUENCY failed:\n\n{e.Message}");
          Stopped?.Invoke(this, new StoppedEventArgs(exception));
        }
    }
    public event EventHandler Tuned; //never fires, this radio cannot be tuned externally
    #endregion

    private void setSettings(Settings newSettings)
    {
      //The settings are used only in SetActive(), no need to apply them here.
      //TODO: validate new settings
      settings = newSettings;
    }

    private void SetActive(bool value)
    {
      if (value == Active) return;

      if (value)
      {
        int rate = settings.SamplingRate;
        Format = new SignalFormat(rate, true, settings.IsSync(), settings.ChannelCount(),
          -(int)(rate * 0.47), (int)(rate * 0.47), 0);

        buffer.Resize(rate * settings.ChannelCount()); //0.5s worth of data
        device.Start(settings);
        Tuned?.Invoke(this, new EventArgs());

        stopping = false;
        iqThread = new Thread(new ThreadStart(IqThreadProcedure)) { IsBackground = true };
        iqThread.Start();
      }
      else
      {
        stopping = true;
        iqThread.Join();
        device.Stop();
      }
    }

    private void IqThreadProcedure()
    {
      try
      {
        while (!stopping)
        {
          var receivedBytes = device.ReadIq();
          buffer.WriteInt16(receivedBytes, receivedBytes.Length);
        }
      }
      catch (Exception e)
      {
        //exception occurred on the UDP reading thread. 
        //stop Afedri, notify the host and terminate the thread
        device.Stop();
        var exception = new Exception($"Unable to read I/Q data from Afedri SDR:\n\n{e.Message}");
        context.Post(s => Stopped?.Invoke(this, new StoppedEventArgs(exception)), null);
      }
    }
  }
}
