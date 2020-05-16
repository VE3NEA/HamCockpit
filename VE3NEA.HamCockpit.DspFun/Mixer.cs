using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSIntel.Ipp;
using MathNet.Numerics;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Mixes the input complex-valued data with a complex sinusoid.</summary>
  public unsafe class Mixer : IIqProcessor
  {
    ISampleStream source;
    Int64 frequency;
    float normalized_frequency;
    float phase;
    Complex32[] sine;

    /// <summary>Initializes a new instance of the <see cref="Mixer" /> class.</summary>
    /// <param name="frequency">The frequency.</param>
    public Mixer(Int64 frequency)
    {
      Frequency = frequency;
    }

    /// <summary>Gets or sets the frequency of the complex sinusoid.</summary>
    /// <value>The frequency of the complex sinusoid.</value>
    public Int64 Frequency { get => frequency; set => SetFrequency(value); }

    //IIqProcessor
    /// <summary>Initializes the mixer to process data from the specified source.</summary>
    /// <param name="source">The source.</param>
    /// <exception cref="Exception">Input to the mixer must be complex.</exception>
    public void Initialize(ISampleStream source)
    {
      if (!source.Format.IsComplex) throw new Exception("Input to the mixer must be complex.");

      //todo: document this: never assign to Format outside of Initialize()
      this.source = source;
      Format = new SignalFormat(source.Format);

      SetUp();
    }

    //ISampleStream
    /// <summary>Gets the format of the output data.</summary>
    /// <value>The format of the output data.</value>
    public SignalFormat Format { get; private set; }

    /// <summary>Occurs when output samples are available.</summary>
    public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

    /// <summary>Reads processed data to the specified buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of values to process.</param>
    /// <returns>The number of processed values.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
      int read = source.Read(buffer, offset, count);
      Mix(buffer, offset, read/2);
      SamplesAvailable?.Invoke(this, new SamplesAvailableEventArgs(buffer, offset, read));
      return read;
    }

    private void SetFrequency(Int64 frequency)
    {
      this.frequency = frequency;
      if (source != null) SetUp();
    }

    private void SetUp()
    {
      Format.DialOffset = source.Format.DialOffset + (int)frequency;

      //0..0.5 or 0.5..1 if negative
      normalized_frequency = frequency / (float)Format.SamplingRate;
      if (normalized_frequency < 0) normalized_frequency += 1f;
    }

    private void Mix(float[] buffer, int offset, int complexCount)
    {
      if (sine == null || sine.Length < complexCount) sine = new Complex32[complexCount];

      //generate sine wave
      fixed (Complex32* pSine = sine)
      fixed (float* pPhase = &phase)
      {
        IppStatus rc = Ipp.ippsTone_32fc((Ipp32fc*)pSine, complexCount, 1f, normalized_frequency,
         pPhase, IppHintAlgorithm.ippAlgHintAccurate);
        IppException.Check(rc);
      }

      //multiply
      fixed (Complex32* pSine = sine)
      fixed (float* pBuf = buffer)
      {
        int channels = Format.Channels;
        int samples = complexCount / channels;

        Complex32* pSrc = pSine;
        Complex32* pDst = (Complex32*)(pBuf + offset);

        //all channels in the sample are multiplied by the same sine value
        for (int sample = 0; sample < samples; sample++, pSrc++)
          for (int channel = 0; channel < channels; channel++, pDst++)
             *pDst *= *pSrc;
      }
    }
  }
}
