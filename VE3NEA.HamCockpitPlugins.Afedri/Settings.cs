using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginHelpers;

namespace VE3NEA.HamCockpitPlugins.Afedri
{
  enum MultichannelMode : byte
  {
    [Description("Single Channel")]
    DUAL_CHANNEL_MODE_OFF = 0,

    [Description("Dual Channel, Synchronized")]
    DIVERSITY_MODE = 1,

    [Description("Dual Channel, Independent")]
    DUAL_CHANNEL_MODE = 2,

    [Description("Quad Channel, Synchronized")]
    QUAD_DIVERSITY_MODE = 4,

    [Description("Quad Channel, Independent")]
    QUAD_CHANNEL_MODE = 5
  }

  class Settings
  {
    [DisplayName("Multichannel Mode")]
    [Description("Enable 1, 2 or 4 channels, synchronized or independent")]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    [DefaultValue(MultichannelMode.DUAL_CHANNEL_MODE_OFF)]
    public MultichannelMode MultichannelMode { get; set; } = MultichannelMode.DUAL_CHANNEL_MODE_OFF;

    [DisplayName("Sampling Rate")]
    [Description("Receiver's output sampling rate")]
    [DefaultValue(Afedri.DEFAULT_SAMPLING_RATE)]
    public int SamplingRate { get; set; } = Afedri.DEFAULT_SAMPLING_RATE;

    [Browsable(false)]
    public Int64[] Frequencies { get; set; } = new Int64[] { 14000000, 14000000, 14000000, 14000000 };


    public int ChannelCount()
    {
      switch (MultichannelMode)
      {
        case MultichannelMode.DIVERSITY_MODE: return 2;
        case MultichannelMode.DUAL_CHANNEL_MODE: return 2;
        case MultichannelMode.QUAD_DIVERSITY_MODE: return 4;
        case MultichannelMode.QUAD_CHANNEL_MODE: return 4;
        default: return 1;
      }
    }

    public int FrequencyCount()
    {
      switch (MultichannelMode)
      {
        case MultichannelMode.DUAL_CHANNEL_MODE: return 2;
        case MultichannelMode.QUAD_CHANNEL_MODE: return 4;
        default: return 1;
      }
    }

    public bool IsSync()
    {
      switch (MultichannelMode)
      {
        case MultichannelMode.DIVERSITY_MODE: return true;
        case MultichannelMode.DUAL_CHANNEL_MODE: return false;
        case MultichannelMode.QUAD_DIVERSITY_MODE: return true;
        case MultichannelMode.QUAD_CHANNEL_MODE: return false;
        default: return false;
      }
    }
  }
}
