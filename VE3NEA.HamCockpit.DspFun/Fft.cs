using CSIntel.Ipp;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;


namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>
  /// The base class that implements common functionality for the <see cref="RealFft"/> and <see cref="ComplexFft"/> classes.
  /// </summary>
  /// <remarks>
  /// This class uses the <a href="https://software.intel.com/en-us/ipp">Intel Integrated Performance Primitives library</a>
  /// to compute the fast Fourier transform.
  /// </remarks>
  /// <seealso cref="RealFft" />
  /// <seealso cref="ComplexFft" />
  public unsafe class BaseFft : IDisposable
  {
    /// <exclude />
    protected byte* specBuffer;
    /// <exclude />
    protected const int IPP_FFT_NODIV_BY_ANY = 8;
    /// <exclude />
    protected static readonly float LnToDb = (float)(10 / Math.Log(10));
    /// <exclude />
    protected readonly int order;
    /// <exclude />
    protected byte[] workBuf;

    /// <summary>The buffer for the frequency domain data.</summary>
    public Complex32[] FreqData;

    /// <summary>Initializes a new instance of the <see cref="BaseFft" /> class.</summary>
    /// <param name="size">The size of the FFT transform.</param>
    public BaseFft(int size)
    {
      order = (int)(Math.Log((double)size) / Math.Log(2d));
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (specBuffer != null) Ipp.ippsFree(specBuffer);
    }

    /// <summary>Compute the power spectrum from the complex spectrum in the <see cref="FreqData" /> array.</summary>
    /// <param name="power">The buffer for the power spectrum, or <c>null</c>.</param>
    /// <returns>The power spectrum.</returns>
    public float[] PowerSpectrum(float[] power = null)
    {
      int len = FreqData.Length;
      int half = len / 2;
      if (power == null) power = new float[len];

      fixed (Complex32* p_spectrum = FreqData)
      fixed (float* p_power = power)
      {
        IppStatus rc;

        if ((1 << order) == FreqData.Length)
        {
          //complex to power, swap the halves
          rc = sp.ippsPowerSpectr_32fc((Ipp32fc*)p_spectrum, p_power + half, half);
          IppException.Check(rc);
          rc = sp.ippsPowerSpectr_32fc((Ipp32fc*)p_spectrum + half, p_power, half);
          IppException.Check(rc);
        }
        else
        {
          rc = sp.ippsPowerSpectr_32fc((Ipp32fc*)p_spectrum, p_power, power.Length);
          IppException.Check(rc);
        }
      }

      return power;
    }

    /// <summary>Compute the <c>log</c> power spectrum from the power spectrum.</summary>
    /// <param name="power">The power spectrum.</param>
    /// <returns>The log power spectrum.</returns>
    public float[] LogPowerSpectrum(float[] power = null)
    {
      power = PowerSpectrum(power);
      IppHelper.PowerToLogPower(power);
      return power;
    }

    /// <summary>  Compute the given slice of the power spectrum.</summary>
    /// <param name="start">The start of the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>The slice of the power spectrum.</returns>
    /// <remarks>
    /// This method re-orders the two halves of the spectrum to put the negative frequencies 
    /// at the bottom, takes the slice of the spectrum and computes the log power.
    /// </remarks>
    public float[] SlicePower(int start, int length)
    {
      float[] power = new float[length];
      int inMid = FreqData.Length / 2;

      if (start < inMid)
      {
        int src = inMid + start;
        int dst = 0;
        int cnt = Math.Min(length, inMid - start);
        for (int i = 0; i < cnt; i++, src++, dst++) power[dst] = FreqData[src].MagnitudeSquared;
      }

      if ((start + length) > inMid)
      {
        int src = Math.Max(0, start - inMid);
        int cnt = (start + length) - (inMid + src);
        int dst = length - cnt;
        for (int i = 0; i < cnt; i++, src++, dst++) power[dst] = FreqData[src].MagnitudeSquared;
      }

      return power;
    }
  }

  /// <summary>Computes forward and inverse complex FFT.</summary>
  /// <remarks>
  /// This class uses the <a href="https://software.intel.com/en-us/ipp">Intel Integrated Performance Primitives library</a>
  /// to compute the fast Fourier transform.
  /// </remarks>
  public unsafe class ComplexFft : BaseFft
  {
    private readonly IppsFFTSpec_C_32fc* pFftSpec;

    /// <summary>The buffer for the time domain data.</summary>
    public readonly Complex32[] TimeData;

    /// <summary>Initializes a new instance of the <see cref="ComplexFft" /> class.</summary>
    /// <param name="size">The size of the FFT transform.</param>
    public ComplexFft(int size) : base(size)
    {
      //allocate input buffer
      TimeData = new Complex32[size];
      FreqData = new Complex32[size];

      //calculate FFT buffer sizes
      int specBufSize, initBufSize, workBufSize;
      IppStatus rc = sp.ippsFFTGetSize_C_32fc(order, IPP_FFT_NODIV_BY_ANY,
        IppHintAlgorithm.ippAlgHintNone, &specBufSize, &initBufSize, &workBufSize);
      IppException.Check(rc);

      //allocate FFT buffers
      Dispose();
      specBuffer = Ipp.ippsMalloc_8u(specBufSize);
      byte[] initBuf = new byte[initBufSize];
      workBuf = new byte[workBufSize];

      //initialize FFT
      fixed (byte* pInitBuf = initBuf)
      fixed (IppsFFTSpec_C_32fc** ppFftSpec = &pFftSpec)
      {
        rc = sp.ippsFFTInit_C_32fc(ppFftSpec, order, IPP_FFT_NODIV_BY_ANY,
              IppHintAlgorithm.ippAlgHintNone, specBuffer, pInitBuf);
      }
      IppException.Check(rc);
    }

    /// <summary>Computes the forward FFT transform.</summary>
    public void ComputeForward()
    {
      fixed (Complex32* p_timeData = TimeData, p_freqData = FreqData)
      fixed (byte* p_workBuf = workBuf)
      {
        IppStatus rc = sp.ippsFFTFwd_CToC_32fc((Ipp32fc*)p_timeData, (Ipp32fc*)p_freqData, pFftSpec, p_workBuf);
        IppException.Check(rc);
      }
    }

    /// <summary>Computes the inverse FFT transform.</summary>
    public void ComputeInverse()
    {
      fixed (Complex32* p_timeData = TimeData, p_freqData = FreqData)
      fixed (byte* p_workBuf = workBuf)
      {
        IppStatus rc = sp.ippsFFTInv_CToC_32fc((Ipp32fc*)p_freqData, (Ipp32fc*)p_timeData, pFftSpec, p_workBuf);
        IppException.Check(rc);
      }
    }
  }

  /// <summary>Computes forward and inverse real FFT.</summary> 
  /// <remarks>
  /// This class uses the <a href="https://software.intel.com/en-us/ipp">Intel Integrated Performance Primitives library</a>
  /// to compute the fast Fourier transform.
  /// </remarks>
  public unsafe class RealFft : BaseFft
  {
    private readonly IppsFFTSpec_R_32f* pFftSpec;

    /// <summary>The buffer for the time domain data.</summary>
    public readonly float[] TimeData;

    /// <summary>Initializes a new instance of the <see cref="RealFft" /> class.</summary>
    /// <param name="size">The size of the FFT transform.</param> 
    public RealFft(int size) : base(size)
    {
      TimeData = new float[size];
      FreqData = new Complex32[size / 2 + 1];

      int specBufSize, initBufSize, workBufSize;
      IppStatus rc = sp.ippsFFTGetSize_R_32f(order, IPP_FFT_NODIV_BY_ANY,
        IppHintAlgorithm.ippAlgHintNone, &specBufSize, &initBufSize, &workBufSize);
      IppException.Check(rc);

      specBuffer = Ipp.ippsMalloc_8u(specBufSize);
      byte[] initBuf = new byte[initBufSize];
      workBuf = new byte[workBufSize];

      fixed (byte* pInitBuf = initBuf)
      fixed (IppsFFTSpec_R_32f** ppFftSpec = &pFftSpec)
      {
        rc = sp.ippsFFTInit_R_32f(ppFftSpec, order, IPP_FFT_NODIV_BY_ANY,
              IppHintAlgorithm.ippAlgHintNone, specBuffer, pInitBuf);
      }
      IppException.Check(rc);
    }

    /// <summary>Computes the forward FFT transform.</summary>
    public void ComputeForward()
    {
      fixed (float* p_timeData = TimeData)
      fixed (Complex32* p_freqData = FreqData)
      fixed (byte* p_workBuf = workBuf)
      {
        IppStatus rc = sp.ippsFFTFwd_RToCCS_32f((float*)p_timeData, (float*)p_freqData, pFftSpec, p_workBuf);
        IppException.Check(rc);
      }
    }

    /// <summary>Computes the inverse FFT transform.</summary>
    public void ComputeInverse()
    {
      fixed (float* p_timeData = TimeData)
      fixed (Complex32* p_freqData = FreqData)
      fixed (byte* p_workBuf = workBuf)
      {
        IppStatus rc = sp.ippsFFTInv_CCSToR_32f((float*)p_freqData, (float*)p_timeData, pFftSpec, p_workBuf);
        IppException.Check(rc);
      }
    }
  }
}
 
 