using CSCore;
using System;


//online docs for CSCore: https://filoe.github.io/cscore/sharpDox/1.2.0-release/#type/ChannelMask


namespace VE3NEA.HamCockpit.DspFun
{
  ///<summary>Selects one of the channels from a stereo data source.</summary>
  public class ChannelSelector : SampleAggregatorBase
  {
    private readonly WaveFormat waveFormat;
    private readonly ChannelMask channel;
    private float[] stereoBuffer;

    /// <summary>Creates a new instance of the <see cref="ChannelSelector"/> class 
    /// from an existing sample source.</summary>
    /// <param name="source">The sample source.</param>
    /// <param name="channel">The channel to select.</param>
    /// <exception cref="ArgumentNullException">source is null</exception>
    /// <exception cref="ArgumentException">The source is not stereo
    /// or Wrong channel.</exception>
    public ChannelSelector(ISampleSource source, ChannelMask channel) : base(source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof(source));
      if (source.WaveFormat.Channels != 2)
        throw new ArgumentException("The source is not stereo.", nameof(source));
      if (channel != ChannelMask.SpeakerFrontLeft && channel != ChannelMask.SpeakerFrontRight)
        throw new ArgumentException("Wrong channel.", nameof(channel));

      this.channel = channel;
      waveFormat = new WaveFormat(source.WaveFormat.SampleRate, 32, 1, AudioEncoding.IeeeFloat);
    }

    /// <summary>Reads a sequence of samples from the <see cref="T:CSCore.SampleAggregatorBase" /> and advances the position within the stream by
    /// the number of samples read.</summary>
    /// <param name="buffer">
    /// An array of floats. When this method returns, the <paramref name="buffer" /> contains the specified
    /// float array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
    /// <paramref name="count" /> - 1) replaced by the floats read from the current source.
    /// </param>
    /// <param name="offset">The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
    /// read from the current stream.</param>
    /// <param name="count">The maximum number of samples to read from the current source.</param>
    /// <returns>The total number of samples read into the buffer.</returns>
    public unsafe override int Read(float[] buffer, int offset, int count)
    {
      stereoBuffer = stereoBuffer.CheckBuffer(count * 2);
      int read = BaseSource.Read(stereoBuffer, 0, count * 2);

      fixed (float* pbuffer = buffer)
      {
        float* ppbuffer = pbuffer + offset;

        switch (channel)
        {
          case ChannelMask.SpeakerFrontLeft:
            for (int i = 0; i < read - 1; i += 2) *(ppbuffer++) = stereoBuffer[i];
            break;
          case ChannelMask.SpeakerFrontRight:
            for (int i = 0; i < read - 1; i += 2) *(ppbuffer++) = stereoBuffer[i + 1];
            break;
        }
      }

      return read / 2;
    }

    /// <summary>Gets or sets the position in samples.</summary>
    public override long Position
    {
      get => BaseSource.Position / 2;
      set
      {
        value -= (value % WaveFormat.BlockAlign);
        BaseSource.Position = value * 2;
      }
    }

    /// <summary>Gets the data length in samples.</summary>
    public override long Length => BaseSource.Length / 2;

    /// <summary>Gets the <see cref="P:CSCore.IAudioSource.WaveFormat" /> of the waveform-audio data.</summary>
    public override WaveFormat WaveFormat => waveFormat;

    /// <summary>Disposes allocated resources.</summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged
    /// resources.</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      stereoBuffer = null;
    }
  }
}
