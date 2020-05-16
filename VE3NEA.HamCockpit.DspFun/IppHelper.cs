using CSIntel.Ipp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>Includes helper functions that use the 
  /// <a href="https://software.intel.com/en-us/ipp">IPP library</a>.
  /// </summary>
  public unsafe class IppHelper
  {
    /// <exclude />
    protected static readonly float LnToDb = (float)(10 / Math.Log(10));

    /// <summary>Converts power ratios to decibels in-place.</summary>
    /// <param name="power">The values to convert.</param>
    public static void PowerToLogPower(float[] power)
    {
      IppStatus rc;
      int len = power.Length;

      fixed (float* p_power = power)
      {
        rc = sp.ippsAddC_32f(p_power, 1f, p_power, len);
        IppException.Check(rc);
        rc = sp.ippsLn_32f_I(p_power, len);
        IppException.Check(rc);
        rc = sp.ippsMulC_32f_I(LnToDb, p_power, len);
        IppException.Check(rc);
      }
    }
  }
}
