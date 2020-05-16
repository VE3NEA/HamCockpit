using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using CSIntel.Ipp;

namespace VE3NEA.HamCockpit.DspFun
{
  unsafe class MultirateFilter  
  {
    private readonly int upFactor, downFactor;
    private readonly byte[] spec, buf;
    private readonly Complex32[] taps;
    private readonly Complex32[] dlySrc, dlyDst;
    public Complex32[] InData;
    public Complex32[] OutData { get; private set; }

    public MultirateFilter(int upFactor, int downFactor, Complex32[] taps)
    {
      this.upFactor = upFactor;
      this.downFactor = downFactor;
      //todo: ipp malloc may be required 
      this.taps = (Complex32[]) taps.Clone();

      int delayCount = (taps.Length + upFactor - 1) / upFactor;
      dlySrc = new Complex32[delayCount];
      dlyDst = new Complex32[delayCount];

      IppStatus rc;
      int specSize, bufSize;
      rc = Ipp.ippsFIRMRGetStateSize_32fc(taps.Length, upFactor, downFactor,
        IppDataType.ipp32fc, &specSize, &bufSize);
      IppException.Check(rc);

      spec = new byte[specSize];
      buf = new byte[bufSize];

      fixed (Complex32* pTaps = taps)
      fixed (byte* pSpec = spec)
      {
        rc = Ipp.ippsFIRMRInit_32fc(pTaps, taps.Length, upFactor, 0, downFactor, 0,
          (IppsFIRSpec_32fc*)pSpec);
        IppException.Check(rc);
      }
    }

    public int Process(int inCount)
    {
      int numIters = inCount / downFactor;
      int outCount = numIters * upFactor;

      if (OutData.Length < outCount) OutData = new Complex32[outCount];

      fixed (Complex32* pSrc = InData)
      fixed (Complex32* pDst = OutData)
      fixed (byte* pSpec = spec)
      fixed (Complex32* pDlySrc = dlySrc)
      fixed (Complex32* pDlyDst = dlyDst)
      fixed (byte* pBuf = buf)
      {
        IppStatus rc = Ipp.ippsFIRMR_32fc(pSrc, pDst, numIters,
          (IppsFIRSpec_32fc*)pSpec, pDlySrc, pDlyDst, pBuf);
        IppException.Check(rc);
      }

      return outCount;
    }
  }
}
