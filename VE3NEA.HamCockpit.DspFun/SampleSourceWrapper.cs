using CSCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.DspFun
{
  // turn ISampleStream into ISampleSource
  /// <summary>A wrapper for the ISampleStream that exposes the CSCore.ISampleSource interface.</summary>
  public class SampleSourceWrapper : ISampleSource
  {
    private ISampleStream source;
    private readonly WaveFormat format;

    /// <summary>Initializes a new instance of the <see cref="SampleSourceWrapper" /> class.</summary>
    /// <param name="source">The sample source.</param>
    public SampleSourceWrapper(ISampleStream source)
    {
      this.source = source;
      format = new WaveFormat(
        source.Format.SamplingRate,
        32,
        source.Format.Channels * (source.Format.IsComplex ? 2 : 1),
        AudioEncoding.IeeeFloat);
    }

    /// <summary>Gets the format of the output data.</summary>
    /// <value>The format.</value>
    public SignalFormat Format { get; private set; }

    /// <summary>Gets the CSCore.IAudioSource.WaveFormat of the waveform-audio data.</summary>
    /// <value>The wave format.</value>
    public WaveFormat WaveFormat => format;

    /// <summary>Reads data from the device to the provided buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of the values to read.</param>
    /// <returns>The number of read values.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
      return source.Read(buffer, offset, count);
    }

    /// <summary>Returns false.</summary>
    /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
    public bool CanSeek => false;

    /// <summary>  Not implemented.</summary>
    /// <value>The position.</value>
    /// <exception cref="NotImplementedException"></exception>
    public long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <summary>  Not implemented.</summary>
    /// <value>The length.</value>
    /// <exception cref="NotImplementedException"></exception>
    public long Length => throw new NotImplementedException();

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    public void Dispose() { }
  }
}
