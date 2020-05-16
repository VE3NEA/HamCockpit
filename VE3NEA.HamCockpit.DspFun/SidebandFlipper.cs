using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Changes the sideband of the signal.</summary>
  public unsafe class SidebandFlipper : IIqProcessor
  {
    private ISampleStream source;
    private bool enabled;

    /// <summary>Gets or sets a value indicating whether this <see cref="SidebandFlipper" /> is enabled.</summary>
    /// <value>
    ///   <c>true</c> if enabled; otherwise, <c>false</c>.</value>
    public bool Enabled { get => enabled; set => setEnabled(value); }

    private void setEnabled(bool value)
    {
      enabled = value;
      Format.DialOffset = (enabled ? -1 : 1) * source.Format.DialOffset;
    }

    //IIqProcessor

    /// <summary>Initializes the sideband flipper for processing data from the specified source.</summary>
    /// <param name="source">The source of the data.</param>
    public void Initialize(ISampleStream source)
    {
      this.source = source;
      Format = new SignalFormat(source.Format);
      setEnabled(enabled);
    }

    //ISampleStream
    /// <summary>Gets the format of the output data.</summary>
    /// <value>The format.</value>
    public SignalFormat Format { get; private set; }

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

      if (enabled)
        fixed (float* pBuf = buffer)
        {
          Complex32* pDst = (Complex32*)(pBuf + offset);
          for (int sample = 0; sample < read/2; sample++, pDst++)
            *pDst = new Complex32((*pDst).Imaginary, (*pDst).Real);
        }

      return read;
    }
  }
}
