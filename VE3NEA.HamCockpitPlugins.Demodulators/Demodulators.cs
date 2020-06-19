using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;
using System.Windows.Forms;
using System.ComponentModel;
using VE3NEA.HamCockpit.DspFun;
using CSCore;
using MathNet.Numerics;

namespace VE3NEA.HamCockpitPlugins.Demodulators
{
  public abstract class BaseDemodulator : IPlugin, IDemodulator
  {
    protected Settings settings = new Settings();
    protected string pluginName;
    protected string modeName;
    protected SignalFormat format;
    protected readonly object lockObject = new object();
    protected ISampleStream source;
    protected Mixer mixer;
    private ComplexFirFilter filter;
    private float [] inputBuffer;
    private ISampleStream signal;
    private SidebandFlipper flipper;

    #region IPlugin
    public string Name => pluginName;
    public string Author => "VE3NEA";
    public bool Enabled { get; set; }
    public object Settings { get => settings; set => settings = value as Settings; }
    public ToolStrip ToolStrip => null;
    public ToolStripItem StatusItem => null;
    #endregion

    #region IDemodulator
    public string Mode { get => modeName; set => throw new NotImplementedException(); }
    public Sideband Sideband { get => settings.Sideband; set => SetSideband(value); }
    public event EventHandler ModeChanged;

    public void Initialize(ISampleStream source)
    {
      //for now, if there are multiple channels, process all
      //TODO: decide what to discard if multiple sync or non-sync channels

      //TODO: if source.Format.DialOffset is non-zero, mix it down to baseband

      if (!source.Format.IsComplex) throw new Exception("Input to demodulator must be I/Q");
      signal = source;

      //resample down to AUDIO_SAMPLING_RATE
      if (source.Format.SamplingRate != SignalFormat.AUDIO_SAMPLING_RATE)
      {
        Resampler resampler = new Resampler(SignalFormat.AUDIO_SAMPLING_RATE, 30);
        resampler.Initialize(signal);
        signal = resampler;
      }

      //flip sidebands if LSB
      flipper = new SidebandFlipper();
      flipper.Initialize(signal);
      flipper.Enabled = settings.Sideband == Sideband.Lower;
      signal = flipper;

      //mix spectrum center up to Pitch
      if (settings.Pitch != 0)
      {
        mixer = new Mixer(settings.Pitch);
        mixer.Initialize(signal);
        signal = mixer;
      }

      //complex 0..6 kHz filter
      float Fc = 2962f / SignalFormat.AUDIO_SAMPLING_RATE;
      var realTaps = Dsp.BlackmanSincKernel(Fc, 235);
      Complex32[] taps = Dsp.FloatToComplex32(realTaps);
      Dsp.Mix(taps, 0.25); //shift filter passband -3..3 kHz -> 0..6 kHz
      filter = new ComplexFirFilter(taps);
      filter.Initialize(signal);
      signal = filter;

      //output format
      format = new SignalFormat(signal.Format)
      {
        IsComplex = false,
        PassbandLow = 0,
        PassbandHigh = 6000,
        Sideband = settings.Sideband
        //todo: DialOffset =
      };
    }
    #endregion

    #region ISampleStream
    public SignalFormat Format => format;
    public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;
    public int Read(float[] buffer, int offset, int count)
    {
      //input I/Q, output real
      int read = count * Dsp.COMPONENTS_IN_COMPLEX;
      if (inputBuffer == null || inputBuffer.Length < read) inputBuffer = new float[read];

      //read downsampled, freq-shifted, sideband-flipped and 6kHz-filtered I/Q
      read = signal.Read(inputBuffer, 0, read);
      count = read / Dsp.COMPONENTS_IN_COMPLEX;

      //complex to real
      for (int i = 0; i < count; i++) buffer[offset + i] = inputBuffer[i * Dsp.COMPONENTS_IN_COMPLEX] * 100f;

      SamplesAvailable?.Invoke(this, new SamplesAvailableEventArgs(buffer, offset, count));
      return count;
    }
    #endregion


    public virtual void SetSideband(Sideband value)
    {
      settings.Sideband = value;
      if (Format != null) Format.Sideband = value;
      if (flipper != null) flipper.Enabled = value == Sideband.Lower;
      ModeChanged?.Invoke(this, new EventArgs());
    }
  }





  [Export(typeof(IPlugin))]
  [Export(typeof(IDemodulator))]
  public class CwDemodulator : BaseDemodulator
  {
    public CwDemodulator()
    {
      pluginName = "CW Demodulator";
      modeName = "CW";
      settings.Pitch = 600;
      settings.Sideband = Sideband.Upper;
    }
  }

  [Export(typeof(IPlugin))]
  [Export(typeof(IDemodulator))]
  public class SsbDemodulator : BaseDemodulator
  {
    public SsbDemodulator()
    {
      pluginName = "SSB Demodulator";
      modeName = "SSB";
      settings.Pitch = 0;
      settings.Sideband = Sideband.Upper;
    }
  }

  [Export(typeof(IPlugin))]
  [Export(typeof(IDemodulator))]
  public class RttyDemodulator : BaseDemodulator
  {
    public RttyDemodulator()
    {
      pluginName = "RTTY Demodulator";
      modeName = "RTTY";
      settings.Pitch = 2125;
      settings.Sideband = Sideband.Lower;
    }

    private const int shift = 170;

    public override void SetSideband(Sideband value)
    {
      if (Format != null && value != Format.Sideband)
      {
        var dF = (value == Sideband.Upper) ? shift : -shift;
        settings.Pitch += dF;
        mixer.Frequency = settings.Pitch;
      }

      base.SetSideband(value);
    }

  }


  public class Settings
  {
    public int Pitch { get; set; }

    [Browsable(false)]
    public Sideband Sideband { get; set; }
  }
}

