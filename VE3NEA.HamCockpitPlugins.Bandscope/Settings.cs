using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpitPlugins.Bandscope
{
  class Settings
  {
    [Browsable(false)]
    public int ZoomSliderPos { get; set; } = 100;
    [Browsable(false)]
    public float DialPosOnScreen { get; set; } = 0.5f;


    [DisplayName("Spectra Per Second")]
    [Description("Band scope refresh rate")]
    [DefaultValue(20)]
    public int SpectraPerSecond { get; set; } = 20;

    [DisplayName("Scale Background Color")]
    [Description("Background color of the frequency scale")]
    [DefaultValue(typeof(Color), "ButtonFace")]
    public Color ScaleBackColor { get; set; } = SystemColors.ButtonFace;

    [DisplayName("Scale Foreground Color")]
    [Description("Foreground color of the frequency scale")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color ScaleForeColor { get; set; } = SystemColors.ControlText;

    [DisplayName("Background Color")]
    [Description("Background color of the spectrum")]
    [DefaultValue(typeof(Color), "0xF0F0FF")]
    public Color BackColor { get; set; } = SystemColors.ButtonFace;

    [DisplayName("Foreground Color")]
    [Description("Foreground color of the spectrum")]
    [DefaultValue(typeof(Color), "0x5E76BA")]
    public Color ForeColor { get; set; } = Color.Teal;
  }
}
