using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;
using CSCore;
using CSCore.DSP;

//DMO resampler as IIqProcessor 

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>A wrapper for the CSCore.DSP.DmoResampler class.</summary>
  public class Resampler : IIqProcessor, IDisposable
  {
    private readonly int outputSamplingRate;
    private ISampleSource resampler;
    private DmoResampler dmo;
    private int Quality { get; set; }
    private readonly object lock_object = new object(); //todo

    /// <summary>Initializes a new instance of the <see cref="Resampler" /> class.</summary>
    /// <param name="outputSamplingRate">The output sampling rate.</param>
    public Resampler(int outputSamplingRate) : this(outputSamplingRate, 30) {}

    /// <summary>Initializes a new instance of the <see cref="Resampler" /> class.</summary>
    /// <param name="outputSamplingRate">The output sampling rate.</param>
    /// <param name="quality">The quality factor.</param>
    public Resampler(int outputSamplingRate, int quality)
    {
      this.outputSamplingRate = outputSamplingRate;
      Quality = quality;
    }

    //IIqProcessor
    /// <summary>Initializes the resampler for processing data from the specified source.</summary>
    /// <param name="source">The data source.</param>
    public void Initialize(ISampleStream source)
    {
      Format = new SignalFormat(source.Format);
      Format.SamplingRate = outputSamplingRate;

      var wrapper = new SampleSourceWrapper(source).ToWaveSource();
      dmo = new DmoResampler(wrapper, outputSamplingRate);
      dmo.Quality = Quality;
      resampler = dmo.ToSampleSource();
    }

    //ISampleStream
    /// <summary>Gets the format of the resampled data.</summary>
    /// <value>The format.</value>
    public SignalFormat Format {get; private set;}

    /// <summary>Occurs when resampled values are available.</summary>
    public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

    /// <summary>Reads processed data to the provided buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of the values to read.</param>
    /// <returns>The number of read values.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
      int read = resampler.Read(buffer, offset, count);
      SamplesAvailable?.Invoke(this, new SamplesAvailableEventArgs(buffer, offset, read));
      return read;
    }

    //IDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
      dmo.Dispose();
    }
  }
}
