using System;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Applies a moving average filter to the floating point data.</summary>
  /// <remarks>
  /// <para>
  /// The classical moving average, <c>y[n] = y[n-1] + x[n] - x[n-k]</c>, fails when applied
  /// to the floating point data
  /// because <c>(a + b) - b</c> is not always equal to <c>a</c> when <c>a</c> and <c>b</c>
  /// are floating point values. For example, <c>(1E-6 + 1E6) - 1E6</c> evaluates to
  /// <c>0</c>, not to <c>1E-6</c>.
  /// The algorithm used in this class is designed to prevent such errors.
  /// </para><para>
  /// The filtered output is not scaled, it needs to be multiplied 
  /// by the <see cref="Scale"/> factor to preserve the signal amplitude. </para>
  /// </remarks>
  public class MovingAverage
  {
    private readonly float[] buffer;
    private int index;
    private float partial;

    /// <summary>Gets the scaling factor.</summary>
    /// <value>The scaling factor that the filtered values need to be multiplied by.</value>
    public float Scale { get; private set; }

    /// <summary>Initializes a new instance of the <see cref="MovingAverage" /> class.</summary>
    /// <param name="length">The length of the filter.</param>
    public MovingAverage(int length)
    {
      buffer = new float[length - 1];
      Scale = 1f / length;
    }

    /// <summary>Processes the specified value.</summary>
    /// <param name="value">The input value.</param>
    /// <returns>The filtered value.</returns>
    public float Process(float value)
    {
      partial += value;
      float result = buffer[index] + partial;
      buffer[index++] = value;

      if (index == buffer.Length)
      {
        for (index = buffer.Length - 1; index > 0; index--)
          buffer[index-1] += buffer[index];
        index = 0;
        partial = 0;
      }

      return result;
    }
  }

  /// <summary>Applies a multi-pass moving average filter to the floating point data.</summary>
  public class MultipassAverage
  {
    private readonly MovingAverage[] passes;

    /// <summary>Gets the scaling factor.</summary>
    /// <value>The scaling factor that the value returned from 
    /// <see cref="ProcessUnscaled"/> needs to be multiplied by.</value>
    public float Scale { get; private set; }

    /// <summary>Initializes a new instance of the <see cref="MultipassAverage" /> class.</summary>
    /// <param name="length">The length of the filter in each pass.</param>
    /// <param name="passCount">The number of passes.</param>
    public MultipassAverage(int length, int passCount)
    {
      passes = new MovingAverage[passCount];
      for (int i = 0; i < passCount; i++) passes[i] = new MovingAverage(length);
      Scale = (float)Math.Pow(length, -passCount);
    }

    /// <summary>Filters the specified value but does not scale the output.</summary>
    /// <param name="value">The value to filter.</param>
    /// <returns>The filtered value.</returns>
    public float ProcessUnscaled(float value)
    {
      foreach (var pass in passes) value = pass.Process(value);
      return value;
    }

    /// <summary>Filters the specified value with proper scaling.</summary>
    /// <param name="value">The value to filter.</param>
    /// <returns>The filtered value.</returns>
    public float Process(float value)
    {
      return ProcessUnscaled(value) * Scale;
    }
    }
  }
