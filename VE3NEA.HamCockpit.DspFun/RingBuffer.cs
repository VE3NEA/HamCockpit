using CSIntel.Ipp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>A thread-safe ring buffer for the floating point values.</summary>
  public unsafe class RingBuffer
  {
    private readonly object lockObject = new object();
    private float[] ringBuffer;
    private float[] buffer;

    /// <summary>Gets the count of the floating point values in the buffer.</summary>
    /// <value>The count.</value>
    public int Count { get; private set; }

    private int readPos, writePos;

    /// <summary>Gets or sets a value indicating whether the output is padded with zeros if there is not enough data to read.</summary>
    /// <value>
    ///   <c>true</c> if padded with zeros; otherwise, <c>false</c>.</value>
    public bool FillWithZeros { get; set; }

    /// <summary>Occurs when filtered samples are available.</summary>
    public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

    /// <summary>Initializes a new instance of the <see cref="RingBuffer" /> class.</summary>
    /// <param name="capacity">The capacity of the ring buffer.</param>
    public RingBuffer(int capacity)
    {
      Resize(capacity);
      FillWithZeros = true;
    }

    /// <summary>  Changes the capacity of the ring buffer.</summary>
    /// <param name="capacity">The capacity.</param>
    public void Resize(int capacity)
    {
      ringBuffer = new float[capacity];
      Count = readPos = writePos = 0;
    }

    /// <summary>Writes floating point values to the specified buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of values.</param>
    public void Write(float[] buffer, int offset, int count)
    {
      if (count > ringBuffer.Length)
        //throw new ArgumentException($"Too many values to write: {count}.");
        Resize(count);

      lock (lockObject)
      {
        int spaceAvailable = ringBuffer.Length - Count;
        int count1 = Math.Min(count, ringBuffer.Length - writePos);
        int count2 = count - count1;

        if (count1 > 0)
        {
          Array.Copy(buffer, offset, ringBuffer, writePos, count1);
          writePos += count1;
          if (writePos == ringBuffer.Length) writePos = 0;
        }

        if (count2 > 0)
        {
          Array.Copy(buffer, offset + count1, ringBuffer, 0, count2);
          writePos = count2;
        }

        Count += count;

        //buffer overflow
        if (count > spaceAvailable)
        {
          Count = ringBuffer.Length;
          readPos = writePos;
        }
      }

      SamplesAvailable?.Invoke(this, new SamplesAvailableEventArgs((float[])buffer.Clone(), offset, count));
    }

    //public void WriteInt16(Int16[] buffer, int wordCount)
    //{
    //  if (this.buffer == null || this.buffer.Length < wordCount)
    //    this.buffer = new float[wordCount];
    //
    //  fixed (Int16* pInBuffer = buffer)
    //  fixed (float* pOutBuffer = this.buffer)
    //    sp.ippsConvert_16s32f_Sfs(pInBuffer, pOutBuffer, wordCount, 15);
    //
    //  Write(this.buffer, 0, wordCount);
    //}

    //todo: avoid double buffering

    /// <summary>  Converts 16-bit integers to the floating point values and writes them to the buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="byteCount">The number of bytes to convert and write.</param>
    public void WriteInt16(byte[] buffer, int byteCount)
    {
      int sampleCount = byteCount / sizeof(Int16);
      if (this.buffer == null || this.buffer.Length < sampleCount)
        this.buffer = new float[sampleCount];

      fixed (byte* pInBuffer = buffer)
      fixed (float* pOutBuffer = this.buffer)
        sp.ippsConvert_16s32f_Sfs((Int16*)pInBuffer, pOutBuffer, sampleCount, 15);

      Write(this.buffer, 0, sampleCount);
    }

    /// <summary>Writes one of the channels of the strided data to the ring buffer.</summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="inFloatCount">The number of the input floating point values.</param>
    /// <param name="format">The format of the data.</param>
    public void WriteStrided(float[] data, int offset, int inFloatCount, SignalFormat format)
    {
      //single channel, just copy
      if (format.Channels == 1)
        Write(data, offset, inFloatCount);

      //multiple complex channels
      else if (format.IsComplex)
      {
        int stride = format.Channels * Dsp.COMPONENTS_IN_COMPLEX;
        int floatCount = inFloatCount / format.Channels;
        int complexCount = inFloatCount / stride;

        if (buffer == null || this.buffer.Length < floatCount) buffer = new float[floatCount];

        Dsp.StridedToComplex(data, offset, stride, buffer, 0, complexCount);
        Write(buffer, 0, floatCount);
      }

      //multiple real channels
      else
      {
        int floatCount = inFloatCount / format.Channels;

        if (buffer == null || this.buffer.Length < floatCount) buffer = new float[floatCount];

        Dsp.StridedToFloat(data, offset, format.Channels, buffer, 0, floatCount);
        Write(buffer, 0, floatCount);
      }
    }

    /// <summary>Reads the data from the ring buffer to the provided buffer.</summary>
    /// <param name="buffer">The output buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of the values to read.</param>
    /// <returns>The number of read values.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
      if (count > ringBuffer.Length)
        //throw new ArgumentException($"Too many values to read: {count}.");
        Resize(count);

      lock (lockObject)
      {
        int readCount = Math.Min(Count, count);
        int count1 = Math.Min(readCount, ringBuffer.Length - readPos);
        int count2 = readCount - count1;

        if (count1 > 0)
        {
          Array.Copy(ringBuffer, readPos, buffer, offset, count1);
          readPos += count1;
          if (readPos == ringBuffer.Length) readPos = 0;
        }

        if (count2 > 0)
        {
          Array.Copy(ringBuffer, 0, buffer, offset + count1, count2);
          readPos = count2;
        }

        Count -= readCount;

        if (readCount < count && FillWithZeros)
          Array.Clear(buffer, offset + readCount, count - readCount);

        return FillWithZeros ? count : readCount;
      }
    }

    /// <summary>Removes the values from the ring buffer.</summary>
    /// <param name="count">The number of values to remove.</param>
    /// <exception cref="ArgumentException">Too many values to dump: {count}.</exception>
    public void Dump(int count)
    {
      if (count > ringBuffer.Length)
        throw new ArgumentException($"Too many values to dump: {count}.");

      lock (lockObject)
      {
        int readCount = Math.Min(Count, count);
        int count1 = Math.Min(readCount, ringBuffer.Length - readPos);
        int count2 = readCount - count1;

        if (count1 > 0)
        {
          readPos += count1;
          if (readPos == ringBuffer.Length) readPos = 0;
        }

        if (count2 > 0) readPos = count2;

        Count -= readCount;
      }
    }
  }
}
