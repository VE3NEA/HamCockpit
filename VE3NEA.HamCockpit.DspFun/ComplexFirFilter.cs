using System;
using MathNet.Numerics;
using CSIntel.Ipp;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Applies a FIR filter to complex-valued data.</summary>
  public unsafe class ComplexFirFilter : IIqProcessor, IDisposable
  {
    private Complex32[] taps;

    /// <summary>Gets or sets the filter taps.</summary>
    /// <value>The filter taps.</value>
    public Complex32[] Taps { get => Taps; set => setTaps(value); }
    private readonly IppAlgType algType;
    private IppComplexFirFilter[] filters;
    ISampleStream source;

    /// <summary>Initializes a new instance of the <see cref="ComplexFirFilter" /> class.</summary>
    /// <param name="taps">The taps.</param>
    /// <param name="algType">Type of the algorithm.</param>
    public ComplexFirFilter(Complex32[] taps, IppAlgType algType = IppAlgType.ippAlgAuto)
    {
      this.taps = taps;
      this.algType = algType;
    }

    /// <summary>Sets the taps of the FIR filter.</summary>
    /// <param name="taps">The tap coefficients.</param>
    private void setTaps(Complex32[] taps)
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
      filters = new IppComplexFirFilter[Format.Channels];
      for (int i = 0; i < filters.Length; i++)
        filters[i] = new IppComplexFirFilter(taps, algType);
    }

    //ISampleStream


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

    /// <summary>Reads the data to the provided buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset to the first value.</param>
    /// <param name="count">The number of the values to read.</param>
    /// <returns>The number of read values.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
      //read the data from source
      int read = source.Read(buffer, offset, count);

      //filter in-place each complex channel
      int stride = Dsp.COMPONENTS_IN_COMPLEX * source.Format.Channels;
      for (int i = 0; i < filters.Length; i++)
        filters[i].ProcessStrided(buffer, offset + Dsp.COMPONENTS_IN_COMPLEX * i, stride, read / stride);

      SamplesAvailable?.Invoke(this, new SamplesAvailableEventArgs(buffer, offset, read));
      return read;
    }

    //IDisposable
    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
      if (filters != null)
        foreach (var filter in filters) filter.Dispose();
    }
  }

  /// <exclude />
  unsafe class IppComplexFirFilter : IDisposable
  {
    private readonly IppAlgType algType;
    private readonly int tapCount;
    private readonly Complex32* taps;
    private readonly IppsFIRSpec_32fc* spec;
    private readonly byte* buf;
    private readonly Complex32* dly;
    private readonly object lock_object = new object();

    public Complex32[] Data;

    public IppComplexFirFilter(Complex32[] taps, IppAlgType algType = IppAlgType.ippAlgAuto)
    {
      //save taps
      tapCount = taps.Length;
      this.algType = algType;

      //allocate buffers
      int specSize, bufSize;
      IppStatus rc = Ipp.ippsFIRSRGetSize(tapCount, IppDataType.ipp32fc, &specSize, &bufSize);
      IppException.Check(rc);
      spec = (IppsFIRSpec_32fc*)Ipp.ippsMalloc_8u(specSize);
      buf = Ipp.ippsMalloc_8u(bufSize);
      dly = Ipp.ippsMalloc_32fc(tapCount - 1);
      sp.ippsZero_32fc((Ipp32fc*)dly, tapCount - 1);
      this.taps = Ipp.ippsMalloc_32fc(tapCount);
      SetTaps(taps);
    }

    public void SetTaps(Complex32[] taps)
    {
      if (taps.Length != tapCount)
        throw new Exception("Filter length cannot be changed on the fly.");

      IppStatus rc;

      lock (lock_object)
      {
        fixed (Complex32* pTaps = taps)
        {
          rc = sp.ippsMove_32fc((Ipp32fc*)pTaps, (Ipp32fc*)this.taps, tapCount);
        }
        IppException.Check(rc);
        rc = Ipp.ippsFIRSRInit_32fc(this.taps, tapCount, algType, spec);
        IppException.Check(rc);
      }
    }

    public void Process(int count)
    {
      IppStatus rc;

      fixed (Complex32* pData = Data)
      {
        rc = Ipp.ippsFIRSR_32fc(pData, pData, count, spec, dly, dly, buf);
      }
      IppException.Check(rc);
    }

    public void ProcessStrided(float[] buffer, int offset, int stride, int complexCount)
    {
      if (Data == null || Data.Length < complexCount) Data = new Complex32[complexCount];

      //todo: optimize, do not copy data if single channel
      Dsp.StridedToComplex(buffer, offset, stride, Data, 0, complexCount);
      Process(complexCount);
      Dsp.ComplexToStrided(Data, 0, buffer, offset, stride, complexCount);
    }

    public void Dispose()
    {
      Ipp.ippsFree(dly);
      Ipp.ippsFree(buf);
      Ipp.ippsFree(spec);
      Ipp.ippsFree(taps);
    }
  }
}