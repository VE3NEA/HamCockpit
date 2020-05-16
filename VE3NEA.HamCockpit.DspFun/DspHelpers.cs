using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>A collection of functions related to digital signal processing.</summary>
  public unsafe static class Dsp
  {
    /// <summary>The number of components in a complex value.</summary>
    /// <remarks>There are two floating point components in a complex value, 
    /// the real part and the imaginary part.</remarks>
    public const int COMPONENTS_IN_COMPLEX = 2;

    /// <summary>Generates the Blackman window.</summary>
    /// <param name="length">The window length.</param>
    /// <returns>The window coefficients.</returns>
    public static float[] BlackmanWindow(int length)
    {
      float[] result = new float[length];
      for (int i = 0; i < length; i++)
      {
        float x = (i + 1f) / (length + 1f);

        result[i] = (float)(
            0.42 
          - 0.50 * Math.Cos(2 * Math.PI * x) 
          + 0.08 * Math.Cos(4 * Math.PI * x)
          );
      }
      return result;
    }

    /// <summary>Generates the Blackman-Harris window.</summary>
    /// <param name="length">The window length.</param>
    /// <returns>The window coefficients.</returns>
    public static float[] BlackmanHarrisWindow(int length)
    {
      float[] result = new float[length];
      for (int i = 0; i < length; i++)
      {
        float x = (i + 1f) / (length + 1f);

        result[i] = (float)(
            0.35875
          - 0.48829 * Math.Cos(2 * Math.PI * x)
          + 0.14128 * Math.Cos(4 * Math.PI * x)
          - 0.01168 * Math.Cos(6 * Math.PI * x)
          );
      }
      return result;
    }

    /// <summary>Generates the Sinc filter kernel.</summary>
    /// <param name="Fc">The normalized cutoff frequency.</param>
    /// <param name="length">The kernel length.</param>
    /// <returns>The filter kernel.</returns>
    public static float[] Sinc(float Fc, int length)
    {
      int mid = (length-1) / 2;
      float[] result = new float[length];
      result[mid] = 1;
      for (int i = 1; i <= mid; i++)
      {
        float x = (float)(2f * Math.PI * Fc * i);
        result[mid - i] = result[mid + i] = (float)(Math.Sin(x) / x);
      }
      return result;
    }

    /// <summary>Generates the Blackman Sinc filter kernel.</summary>
    /// <param name="Fc">The normalized cutoff frequency.</param>
    /// <param name="length">The kernel length.</param>
    /// <returns>The filter kernel.</returns>
    public static float[] BlackmanSincKernel(float Fc, int length)
    {
      float[] result = BlackmanWindow(length);
      float[] sinc = Sinc(Fc, length);
      for (int i = 0; i < length; i++) result[i] *= sinc[i];
      Normalize(result);
      return result;
    }

    /// <summary>Normalizes the specified array of floating point data to unity sum.</summary>
    /// <param name="data">The data.</param>
    public static void Normalize(float[] data)
    {
      float sum = 0;
      foreach (float d in data) sum += d;
      for (int i = 0; i < data.Length; i++) data[i] /= sum;
    }

    /// <summary>Converts power ratio to decibels.</summary>
    /// <param name="x">The power ratio.</param>
    /// <returns>The ratio in decibels.</returns>
    public static float ToDb(float x)
    {
      return 10f * (float)Math.Log10(x);
    }

    /// <summary>Converts voltage ratio to decibels.</summary>
    /// <param name="x">The voltage ratio.</param>
    /// <returns>The ratio in decibels.</returns>
    public static float ToDb2(float x)
    {
      return 2 * ToDb(x);
    }

    /// <summary>  Converts the ratio in decibels to voltage ratio.</summary>
    /// <param name="x">The ratio in decibels.</param>
    /// <returns>The voltage ratio.</returns>
    public static float FromDb2(float x)
    {
      return (float)Math.Pow(10, x / 20f);
    }

    /// <summary>  Converts the ratio in decibels to power ratio.</summary>
    /// <param name="x">The ratio in decibels.</param>
    /// <returns>The power ratio.</returns>
    public static float FromDb(float x)
    {
      return FromDb2(2 * x);
    }

    /// <summary>Swaps the I and Q channels in the array of data.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="floatOffset">The offset to the first floating point value to convert.</param>
    /// <param name="complexCount">The number of complex values to convert.</param>    
    public static void SwapIQ(float[] buffer, int floatOffset, int complexCount)
    {
      fixed (float* pBuf = buffer)
      {
        Complex32* pDst = (Complex32*)(pBuf + floatOffset);
        for (int sample = 0; sample < complexCount; sample++, pDst++)
          *pDst = new Complex32((*pDst).Imaginary, (*pDst).Real);
      }
    }

    /// <summary>Performs in-place mixing of the specified data with a complex sinusoid.</summary>
    /// <param name="data">The data.</param>
    /// <param name="frequency">The frequency.</param>
    /// <param name="phase">The starting phase.</param>    
    public static void Mix(Complex32[] data, double frequency, double phase = 0)
    {
      Complex phasor = new Complex(Math.Cos(phase), Math.Sin(phase));
      double dPhase = 2 * Math.PI * frequency;
      Complex dPhasor = new Complex(Math.Cos(dPhase), Math.Sin(dPhase));

      for (int i=0; i<data.Length; i++)
      {
        data[i] *= new Complex32((float)phasor.Real, (float)phasor.Imaginary);
        phasor *= dPhasor;
      }
    }

    /// <summary>Unpacks complex values from the array of floats.</summary>
    /// <param name="source">The floating point values.</param>
    /// <returns>The complex values.</returns>
    public static Complex32[] FloatToComplex32(float[] source)
    {
      Complex32[] result = new Complex32[source.Length];
      for (int i = 0; i < source.Length; i++) result[i] = source[i];
      return result;
    }

    //Diophantine algorithm
    //https://www.daniweb.com/software-development/python/code/223956
    /// <summary>Finds a rational approximation
    /// of a floating point value.</summary>
    /// <param name="ratio">The floating point value.</param>
    /// <param name="maxError">The maximum error.</param>
    /// <returns>The approximating ratio, L/M.</returns>
    /// <remarks>Finds the 
    /// <a href="https://en.wikipedia.org/wiki/Diophantine_approximation">Diophantine approximation</a> 
    /// to the given floating point value. Useful for the design of low complexity 
    /// <a href="https://dspguru.com/dsp/faqs/multirate/resampling/">resamplers</a> 
    /// when the exact output rate is not required.
    /// </remarks>
    public static (int L, int M) ApproximateRatio(double ratio, double maxError)
    {
      int M, oldM, newM, L, oldL, newL;
      int quot;
      double rest;

      oldL = 1;
      L = (int)ratio;
      oldM = 0;
      M = 1;
      rest = ratio;
      quot = L;

      while (Math.Abs(L / (float)M - ratio) / ratio > maxError)
      {
        rest = 1 / (rest - quot);
        quot = (int)rest;

        newL = quot * L + oldL;
        oldL = L;
        L = newL;

        newM = quot * M + oldM;
        oldM = M;
        M = newM;
      }
    
    return (L, M);
    }

    //float[] to Complex32[]
    /// <summary>Unpacks complex values from the array of floats.</summary>
    /// <param name="source">The source array.</param>
    /// <param name="srcOffset">The offset to the first value in the source array.</param>
    /// <param name="srcStride">The stride in the source array.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="dstOffset">The offset to the first value in the destination array.</param>
    /// <param name="complexCount">The number of the complex values to output.</param>
    public static void StridedToComplex(float[] source, int srcOffset, int srcStride,
      Complex32[] destination, int dstOffset, int complexCount)
    {
      fixed (float* pSrc = source)
      fixed (Complex32* pDst = destination)
      {
        float* src = pSrc + srcOffset;
        Complex32* dst = pDst + dstOffset;
        for (int i = 0; i < complexCount; i++, src += srcStride, dst++)
          *dst = *(Complex32*)src;
      }
    }

    //float[] to float[]
    /// <summary>Convert data format.</summary>
    /// <param name="source">The source array.</param>
    /// <param name="srcOffset">The offset to the first value in the source array.</param>
    /// <param name="srcStride">The stride in the source array.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="dstOffset">The offset to the first value in the destination array.</param>
    /// <param name="complexCount">The number of the complex values to output.</param>
    public static void StridedToComplex(float[] source, int srcOffset, int srcStride,
     float[] destination, int dstOffset, int complexCount)
    {
      fixed (float* pSrc = source)
      fixed (float* pDst = destination)
      {
        float* src = pSrc + srcOffset;
        float* dst = pDst + dstOffset;
        for (int i = 0; i < complexCount; i++, src += srcStride, dst += Dsp.COMPONENTS_IN_COMPLEX)
        {
          *dst = *src;
          *(dst + 1) = *(src + 1);
        }
      }
    }

    /// <summary>Convert data format.</summary>
    /// <param name="source">The source array.</param>
    /// <param name="srcOffset">The offset to the first value in the source array.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="dstOffset">The offset to the first value in the destination array.</param>
    /// <param name="dstStride">The stride in the destination array.</param>
    /// <param name="complexCount">The number of the complex values to output.</param>
    public static void ComplexToStrided(Complex32[] source, int srcOffset,
      float[] destination, int dstOffset, int dstStride, int complexCount)
    {
      fixed (Complex32* pSrc = source)
      fixed (float* pDst = destination)
      {
        Complex32* src = pSrc + srcOffset;
        float* dst = pDst + dstOffset;
        for (int i = 0; i < complexCount; i++, src++, dst += dstStride)
          *(Complex32*)dst = *src;
      }
    }

    /// <summary>Convert data format.</summary>
    /// <param name="source">The source array.</param>
    /// <param name="srcOffset">The offset to the first value in the source array.</param>
    /// <param name="srcStride">The stride in the source array.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="dstOffset">The offset to the first value in the destination array.</param>
    /// <param name="count">The number of the values to output.</param>
    public static void StridedToFloat(float[] source, int srcOffset, int srcStride,
      float[] destination, int dstOffset, int count)
    {
      fixed (float* pSrc = source)
      fixed (float* pDst = destination)
      {
        float* src = pSrc + srcOffset;
        float* dst = pDst + dstOffset;
        for (int i = 0; i < count; i++, src += srcStride, dst++) *dst = *src;
      }
    }

    /// <summary>Convert data format.</summary>
    /// <param name="source">The source array.</param>
    /// <param name="srcOffset">The offset to the first value in the source array.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="dstOffset">The offset to the first value in the destination array.</param>
    /// <param name="dstStride">The stride in the destination array.</param>
    /// <param name="count">The number of the values to output.</param>
    public static void FloatToStrided(float[] source, int srcOffset,
      float[] destination, int dstOffset, int dstStride, int count)
    {
      fixed (float* pSrc = source)
      fixed (float* pDst = destination)
      {
        float* src = pSrc + srcOffset;
        float* dst = pDst + dstOffset;
        for (int i = 0; i < count; i++, src++, dst += dstStride) *dst = *src;
      }
    }

    /// <summary>Converts an array of floating point values to text, one value per line.</summary>
    /// <param name="array">The array of values.</param>
    /// <returns>The text.</returns>
    public static string ArrayToString(float[] array)
    {
      string s = "";
      for (int i = 0; i < array.Length; i++) s += $"{i}  {array[i]}" + Environment.NewLine;
      return s;
    }
  }
}
