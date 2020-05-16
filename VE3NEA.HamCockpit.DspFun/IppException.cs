using CSIntel.Ipp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Represents errors that occur during the execution of the 
  /// <a href="https://software.intel.com/en-us/ipp">IPP</a> functions.</summary>
  public class IppException : Exception
  {
    /// <summary>Initializes a new instance of the <see cref="IppException" /> class.</summary>
    /// <param name="status">The IPP status code.</param>
    public IppException(IppStatus status) : base($"IPP error: {core.ippGetStatusString(status)}") { }

    /// <summary>Checks the IPP status code and raises an IppException if the code represents an error.</summary>
    /// <param name="status">The status code.</param>
    /// <exception cref="VE3NEA.HamCockpit.DspFun.IppException"><paramref name="status"/> represents an error.</exception>
    public static void Check(IppStatus status)
    {
      if (status < IppStatus.ippStsNoErr) throw new IppException(status);
    }
  }
}
