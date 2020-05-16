using System;
using CSIntel.Ipp;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Applies a FIR filter to real-valued data.</summary>
  public class RealFirFilter : IIqProcessor, IDisposable
  {
    private float[] taps;

    /// <summary>Gets or sets the filter taps.</summary>
    /// <value>The filter taps.</value>
    public float[] Taps { get => Taps; set => setTaps(value); }

    private readonly IppAlgType algType;
    private IppRealFirFilter[] filters;
    ISampleStream source;

    /// <summary>Initializes a new instance of the <see cref="RealFirFilter" /> class.</summary>
    /// <param name="taps">The taps.</param>
    /// <param name="algType">Type of the algorithm.</param>
    public RealFirFilter(float[] taps, IppAlgType algType = IppAlgType.ippAlgAuto)
    {
      this.taps = taps;
      this.algType = algType;
    }

    /// <summary>Sets the taps of the FIR filter.</summary>
    /// <param name="taps">The tap coefficients.</param>
    private void setTaps(float[] taps)
    {
      this.taps = taps;
      if (filters != null) foreach (var filter in filters) filter.SetTaps(taps);
    }

    //IIqProcessor

    /// <summary>Initializes the filter for processing data from the specified source.</summary>
    /// <param name="source">The source of the data.</param>
    public void Initialize(ISampleStream source)
    {
      this.source = source;
      Dispose();
      filters = new IppRealFirFilter[Format.Channels];
      for (int i = 0; i < filters.Length; i++)
        filters[i] = new IppRealFirFilter(taps, algType);
    }

    /// <summary>Gets the format of the output data.</summary>
    /// <value>The format of the output data.</value>
    /// <remarks>
    /// Since the filter does not know its passband (all it has is the taps array),
    /// it does not update the Format property. The calling code must change the Format settings
    /// to reflect the new bandwidth.
    /// </remarks>
    public SignalFormat Format => source.Format;

    /// <summary>Occurs when filtered samples are available.</summary>
    public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

    /// <summary>Reads processed data to the provided buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of the values to read.</param>
    /// <returns>The number of read values.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
      int read = source.Read(buffer, offset, count);

      int stride = source.Format.Channels;
      for (int i = 0; i < stride; i++)
        filters[i].ProcessStrided(buffer, offset + i, stride, read / stride);

      SamplesAvailable?.Invoke(this, new SamplesAvailableEventArgs(buffer, offset, read));
      return read;
    }

    //IDisposable
    /// <summary>Performs application-defined tasks associated with freeing, 
    /// releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
      if (filters != null)
        foreach (var filter in filters) filter.Dispose();
    }
  }

  /// <exclude />
  public unsafe class IppRealFirFilter : IDisposable
  {
    private readonly IppAlgType algType;
    private readonly int tapCount;
    private readonly float* taps;
    private readonly IppsFIRSpec_32f* spec;
    private readonly byte* buf;
    private readonly float* dly;
    private readonly object lock_object = new object();

    /// <exclude />
    public float[] Data;

    /// <exclude />
    public IppRealFirFilter(float[] taps, IppAlgType algType = IppAlgType.ippAlgAuto)
    {
      this.algType = algType;
      tapCount = taps.Length;

      //allocate buffers
      int specSize, bufSize;
      IppStatus rc = Ipp.ippsFIRSRGetSize(tapCount, IppDataType.ipp32f, &specSize, &bufSize);
      IppException.Check(rc);
      spec = (IppsFIRSpec_32f*)Ipp.ippsMalloc_8u(specSize);
      buf = Ipp.ippsMalloc_8u(bufSize);
      dly = Ipp.ippsMalloc_32f(tapCount - 1);
      sp.ippsZero_32f(dly, tapCount - 1);
      this.taps = Ipp.ippsMalloc_32f(tapCount);
      SetTaps(taps);
    }

    /// <exclude />
    public void SetTaps(float[] taps)
    {
      if (taps.Length != tapCount)
        throw new Exception("Filter length cannot be changed on the fly.");

      IppStatus rc;

      lock (lock_object)
      {
        fixed (float* pTaps = taps)
        {
          rc = sp.ippsMove_32f(pTaps, this.taps, tapCount);
        }
        IppException.Check(rc);
        rc = Ipp.ippsFIRSRInit_32f(this.taps, tapCount, algType, spec);
        IppException.Check(rc);
      }
    }

    /// <exclude />
    public void Process(int count)
    {
      IppStatus rc;

      lock (lock_object)
      {
        fixed (float* pData = Data)
        {
          rc = Ipp.ippsFIRSR_32f(pData, pData, count, spec, dly, dly, buf);
        }
        IppException.Check(rc);
      }
    }

    /// <exclude />
    public void ProcessStrided(float[] buffer, int offset, int stride, int count)
    {
      if (Data == null || Data.Length < count) Data = new float[count];

      Dsp.StridedToFloat(buffer, offset, stride, Data, 0, count);
      Process(count);
      Dsp.FloatToStrided(Data, 0, buffer, offset, stride, count);
    }

    /// <exclude />
    public void Dispose()
    {
      Ipp.ippsFree(dly);
      Ipp.ippsFree(buf);
      Ipp.ippsFree(spec);
      Ipp.ippsFree(taps);
    }
  }
}
