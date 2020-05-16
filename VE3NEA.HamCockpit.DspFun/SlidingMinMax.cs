using System;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Computes the sliding minimum of the input values.<br /></summary>
  public class SlidingMin : BaseSlidingFilter
  {
    /// <summary>Initializes a new instance of the <see cref="SlidingMin" /> class.</summary>
    /// <param name="length">The length of the sliding filter.</param>
    public SlidingMin(int length) : base(length) { }
    /// <summary>Returns the minimum of two values.</summary>
    /// <param name="x">The first value.</param>
    /// <param name="y">The second value.</param>
    /// <returns>The minimum of two values.</returns>
    /// <remarks>The child class overrides this method to return either the minimum or maximum.</remarks>
    override protected float MinOrMax(float x, float y) { return Math.Min(x, y); }
  }

  /// <summary>Computes the sliding maximum of the input values.<br /></summary>
  public class SlidingMax : BaseSlidingFilter
  {
    /// <summary>Initializes a new instance of the <see cref="SlidingMax" /> class.</summary>
    /// <param name="length">The length of the sliding filter.</param>
    public SlidingMax(int length) : base(length) { }

    /// <summary>Returns the maximum of two values.</summary>
    /// <param name="x">The first value.</param>
    /// <param name="y">The second value.</param>
    /// <returns>The maximum of two values.</returns>
    override protected float MinOrMax(float x, float y) { return Math.Max(x, y); }
  }

  /// <summary>
  /// The base class that implements common functionality for the
  /// <see cref="SlidingMin"/> and <see cref="SlidingMax"/> classes.
  /// </summary>
  public abstract class BaseSlidingFilter
  {
    private readonly int bufferLength;
    private float[] buffer;
    private int index;
    private float partial;

    /// <summary>  Returns either the minimum or maximum of two values.</summary>
    /// <param name="x">The first value.</param>
    /// <param name="y">The second value.</param>
    /// <returns>The minimum or the maximum of two values.</returns>
    /// <remarks>The child class overrides this method to return either the minimum or maximum.</remarks>
    abstract protected float MinOrMax(float x, float y);

    /// <summary>Initializes a new instance of the class.</summary>
    /// <param name="length">The length of the sliding filter.</param>
    public BaseSlidingFilter(int length)
    {
      bufferLength = length - 1;
    }

    /// <summary>Processes the specified input value.</summary>
    /// <param name="value">The output value.</param>
    /// <returns>The filtered value.</returns>
    public float Process(float value)
    {
      if (buffer == null)
      {
        buffer = new float[bufferLength];
        for (int i = 0; i < bufferLength; i++) buffer[i] = value;
        partial = value;
        return value;
      }

      partial = MinOrMax(partial, value);
      float result = MinOrMax(buffer[index], partial);
      buffer[index++] = value;

      if (index == bufferLength)
      {
        for (index = buffer.Length-1; index > 0; index--)
          buffer[index-1] = MinOrMax(buffer[index-1], buffer[index]);
        index = 0;
        partial = value;
      }

      return result;
    }

    /// <summary>Filters the array of input data in place.</summary>
    /// <param name="data">The data.</param>
    /// <exception cref="ArgumentException">Sliding Filter error: input too short</exception>
    public void FilterArrayInplace(float[] data)
    {
      int wing = bufferLength / 2;
      int len = data.Length;
      if (len <= wing) throw new ArgumentException("Sliding Filter error: input too short");

      buffer = null;
      for (int i = 0; i < wing; i++) Process(data[i]);
      for (int i = wing; i < len; i++) data[i-wing] = Process(data[i]);
      for (int i = len; i < len + wing; i++) data[i-wing] = Process(data[len-1]);
    }
  }
}
