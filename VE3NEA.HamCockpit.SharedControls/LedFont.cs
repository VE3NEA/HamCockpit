using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
//using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.SharedControls
{
  // solution from https://stackoverflow.com/questions/556147
  // font from https://www.dafont.com/alarm-clock.font

  /// <summary>Provides a 7-segment LED font.</summary>
  /// <example>
  /// This code assigns the LED font to the <c>Frequencylabel</c> control:
  /// <code>
  /// FrequencyLabel.Font = new Font(LedFont.Family, FrequencyLabel.Font.Size);
  /// </code></example>
  public class LedFont
  {
    private static readonly PrivateFontCollection collection;

    static LedFont()
    {
      collection = new PrivateFontCollection();
      LoadFont();
    }

    /// <summary>Gets the font family.</summary>
    /// <value>The font family.</value>
    public static FontFamily Family { get => collection.Families[0]; }


  [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
        IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

    private static void LoadFont()
    {
      byte[] fontBytes = Properties.Resources.LedFont;
      IntPtr fontPtr = Marshal.AllocCoTaskMem(fontBytes.Length);
      Marshal.Copy(fontBytes, 0, fontPtr, fontBytes.Length);
      collection.AddMemoryFont(fontPtr, fontBytes.Length);
      uint dummy = 0;
      AddFontMemResourceEx(fontPtr, (uint)fontBytes.Length, IntPtr.Zero, ref dummy);
      Marshal.FreeCoTaskMem(fontPtr);
    }
  }
}
