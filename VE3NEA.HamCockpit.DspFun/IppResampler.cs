using CSIntel.Ipp;
using System;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Signal resampler based on the 
  /// <a href="https://software.intel.com/en-us/ipp">IPP library</a>.
  /// </summary>
  public unsafe class IppResampler : IDisposable
  {
    private readonly IppsResamplingPolyphaseFixed_32f* pSpec;
    private readonly int inRate, outRate;
    private readonly int readIndex0, writeIndex0;

    private float[] inData;

    /// <summary>The resampled data.</summary>
    public float[] OutData;
    private double readIndex;
    private int writeIndex;

    //filterCutoff is 0..1 (-6 dB point)

    /// <summary>Initializes a new instance of the <see cref="IppResampler" /> class.</summary>
    /// <param name="inRate">The input sampling rate.</param>
    /// <param name="outRate">The output sampling rate.</param>
    /// <param name="requestedFilterOrder">The requested filter order.</param>
    /// <param name="filterCutoff">The 6-dB cutoff frequency of the anti-aliasing filter 
    /// as a fraction of the Nyquist frequency. Must be in the range of 0...1.</param>
    /// <param name="kaiserAlpha">The alpha parameter of the Kaiser filter.</param>
    public IppResampler(int inRate, int outRate, int requestedFilterOrder, float filterCutoff, float kaiserAlpha)
    {
      this.inRate = inRate;
      this.outRate = outRate;

      //allocate memory for the resampler structure
      int specSize, filterOrder, filterHeight;
      IppStatus rc;
      rc = Ipp.ippsResamplePolyphaseFixedGetSize_32f(inRate, outRate, requestedFilterOrder, &specSize, 
        &filterOrder, &filterHeight, IppHintAlgorithm.ippAlgHintFast);
      IppException.Check(rc);

      //{!} docs suggest to use ippsMalloc_8u(specSize), but that results in memory corruption
      pSpec = (IppsResamplingPolyphaseFixed_32f*) Ipp.ippsMalloc_8u(specSize * filterHeight);

      //initialize IPP resampler
      rc = Ipp.ippsResamplePolyphaseFixedInit_32f(inRate, outRate, filterOrder, filterCutoff, 
        kaiserAlpha, pSpec, IppHintAlgorithm.ippAlgHintFast);
      IppException.Check(rc);

      //set up input buffer
      inData = new float[filterOrder];
      readIndex = readIndex0 = filterOrder / 2;
      writeIndex = writeIndex0 = filterOrder;
    }

    /// <summary>Processes the specified input data.</summary>
    /// <param name="inputData">The input data.</param>
    /// <param name="inputOffset">The offset of the first value to process.</param>
    /// <param name="inputStride">The stride in the input data.</param>
    /// <param name="count">The number of values to process.</param>
    /// <returns>The number of resampled values.</returns>
    public int Process(float[] inputData, int inputOffset, int inputStride, int count)
    {
      //resize input buffer, *preserve data*
      int requiredBufferSize = writeIndex + count;
      if (inData.Length < requiredBufferSize) Array.Resize(ref inData, requiredBufferSize);

      //de-interleave input data and write to the buffer
      Dsp.StridedToFloat(inputData, inputOffset, inputStride, inData, writeIndex, count);
      writeIndex += count;
      int unusedCount = writeIndex - writeIndex0;

      //estimate output count
      int maxOutCount = (int) (((long)unusedCount * outRate) / inRate) + 2; //+2 is from the example
      if (OutData == null || OutData.Length < maxOutCount) OutData = new float[maxOutCount];
      int outCount = 0;

      fixed (float* pSrc = inData, pDst = OutData)
      fixed (double* pTime = &readIndex)
      {
        //resample
        IppStatus rc = Ipp.ippsResamplePolyphaseFixed_32f(pSrc, unusedCount, pDst, 1f, pTime, &outCount, pSpec);
        IppException.Check(rc);
        int usedCount = (int)readIndex - readIndex0;

        //shift unused data back in the buffer
        rc = sp.ippsMove_32f(pSrc + usedCount, pSrc, writeIndex - usedCount);
        IppException.Check(rc);
        readIndex -= usedCount;
        writeIndex -= usedCount;
      }

      return outCount;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, 
    /// or resetting unmanaged resources.</summary>
    public void Dispose()
    {
      Ipp.ippsFree(pSpec);
    }
  }
}
