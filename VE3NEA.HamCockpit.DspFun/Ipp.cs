using CSIntel.Ipp;
using MathNet.Numerics;
using System.Runtime.InteropServices;
using System.Security;

//naming conventions of IPP differ from those of C#, ignore the warning
#pragma warning disable IDE1006

namespace VE3NEA.HamCockpit.DspFun
{
  /// <exclude />
  public struct IppsFIRSpec_32f { }

  /// <exclude />
  public struct IppsFIRSpec_32fc { }

  /// <exclude />
  public struct IppsResamplingPolyphaseFixed_32f { }

  /// <exclude />
  public enum IppAlgType
  {
    /// <exclude />
    ippAlgAuto = 0x00,
    /// <exclude />
    ippAlgDirect = 0x01,
    /// <exclude />
    ippAlgFFT = 0x02,
    /// <exclude />
    ippAlgMask = 0xFF
  }


  //

  /// <summary>
  /// The IPP PInvoke declarations missing from 
  /// <a href="https://github.com/DNRY/CSIntelPerfLibs">CSIntelPerfLibs</a>.
  /// </summary>
  /// <remarks>
  /// This class contains the IPP PInvoke declarations missing from the
  /// <a href="https://github.com/DNRY/CSIntelPerfLibs">CSIntelPerfLibs</a> library. 
  /// Please see the  
  /// <a href="https://software.intel.com/en-us/ipp/documentation">Integrated Performance 
  /// Primitives documentation</a> for the description of these functions. 
  /// </remarks>
  unsafe public class Ipp
  {
    //memory

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsMalloc_8u")]
    public static extern byte* ippsMalloc_8u(int len);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsMalloc_32f")]
    public static extern float* ippsMalloc_32f(int len);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsMalloc_32fc")]
    public static extern Complex32* ippsMalloc_32fc(int len);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFree")]
    public static extern void ippsFree(void* ptr);


    //fir filter

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRSRGetSize")]
    public static extern IppStatus ippsFIRSRGetSize(int tapsLen, IppDataType tapsType, 
      int* pSpecSize, int* pBufSize);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRSRInit_32f")]
    public static extern IppStatus ippsFIRSRInit_32f(float* pTaps, int tapsLen, 
      IppAlgType algType, IppsFIRSpec_32f* pSpec);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRSRInit_32fc")]
    public static extern IppStatus ippsFIRSRInit_32fc(Complex32* pTaps, int tapsLen, 
      IppAlgType algType, IppsFIRSpec_32fc* pSpec);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRSR_32f")]
    public static extern IppStatus ippsFIRSR_32f(float* pSrc, float* pDst, 
      int numIters, IppsFIRSpec_32f* pSpec, float* pDlySrc, float* pDlyDst, 
      byte* pBuf);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRSR_32fc")]
    public static extern IppStatus ippsFIRSR_32fc(Complex32* pSrc, Complex32* pDst, 
      int numIters, IppsFIRSpec_32fc* pSpec, Complex32* pDlySrc, Complex32* pDlyDst, 
      byte* pBuf);


    //multi-rate filter

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRMRGetStateSize_32fc")]
    public static extern IppStatus ippsFIRMRGetStateSize_32fc(int tapsLen, int upFactor, 
      int downFactor, IppDataType tapsType, int* pSpecSize, int* pBufSize);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRMRInit_32fc")]
    public static extern IppStatus ippsFIRMRInit_32fc(Complex32* pTaps, int tapsLen, 
      int upFactor, int upPhase, int downFactor, int downPhase, 
      IppsFIRSpec_32fc* pSpec);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsFIRMR_32fc")]
    public static extern IppStatus ippsFIRMR_32fc(Complex32* pSrc, Complex32* pDst, 
      int numIters, IppsFIRSpec_32fc* pSpec, Complex32* pDlySrc, Complex32* pDlyDst, 
      byte* pBuf);


    //resampler

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsResamplePolyphaseFixedGetSize_32f")]
    public static extern IppStatus ippsResamplePolyphaseFixedGetSize_32f(int inRate, int outRate, 
      int len, int* pSize, int* pLen, int* pHeight, IppHintAlgorithm hint);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsResamplePolyphaseFixedInit_32f")]
    public static extern IppStatus ippsResamplePolyphaseFixedInit_32f(int inRate, int outRate, 
      int len, float rollf, float alpha, IppsResamplingPolyphaseFixed_32f* pSpec, 
      IppHintAlgorithm hint);

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsResamplePolyphaseFixed_32f")]
    public static extern IppStatus ippsResamplePolyphaseFixed_32f(float* pSrc, int len, 
      float* pDst, float norm, double* pTime, int* pOutlen, 
      IppsResamplingPolyphaseFixed_32f* pSpec);

    //sine wave generator

    /// <exclude />
    [SuppressUnmanagedCodeSecurityAttribute()]
    [DllImport("ipps.dll", EntryPoint = "ippsTone_32fc")]
    public static extern IppStatus ippsTone_32fc(Ipp32fc* pDst, int len, float magn,
      float rFreq, float* pPhase, IppHintAlgorithm hint);
  }
}

#pragma warning restore IDE1006