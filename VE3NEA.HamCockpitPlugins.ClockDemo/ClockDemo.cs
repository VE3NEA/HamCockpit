using System.ComponentModel;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using VE3NEA.HamCockpit.PluginAPI;
using System;

namespace VE3NEA.HamCockpitPlugins.ClockDemo
{
  [Export(typeof(IPlugin))]
  class ClockDemo : IPlugin, IDisposable
  {
    private Settings settings = new Settings();
    private readonly Clock clock = new Clock();

    // IPlugin
    public string Name => "Clock Demo";
    public string Author => "VE3NEA";
    public bool Enabled { get; set; }
    public object Settings { get => GetSettings(); set => ApplySettings(value as Settings); }
    public ToolStrip ToolStrip { get; } = new ToolStrip();
    public ToolStripItem StatusItem => null;

    ClockDemo()
    {
      clock.MinimumSize = clock.Size;
      ToolStrip.Items.Add(new ToolStripControlHost(clock));
    }

    object GetSettings()
    {
      settings.UtcMode = clock.UtcMode;
      settings.DockToRight = ToolStrip.RightToLeft == RightToLeft.Yes;
      return settings;
    }

    void ApplySettings(Settings value)
    {
      settings = value;
      clock.UtcMode = settings.UtcMode;
      clock.Blink = settings.Blink;
      ToolStrip.RightToLeft = settings.DockToRight ? RightToLeft.Yes : RightToLeft.No;
    }

    public void Dispose()
    {
      clock.Dispose();
      ToolStrip.Dispose();
    }
  }

  public class Settings
  {
    //this setting is saved/restored and editable by user
    [DisplayName("Blink")]
    [Description("Time separator blinks")]
    [DefaultValue(false)]
    public bool Blink { get; set; } = false;

    //this setting is saved/restored and editable by user
    [DisplayName("Dock to Right")]
    [Description("Dock to the right side of the toolbar")]
    [DefaultValue(true)]
    public bool DockToRight { get; set; } = true;

    //this setting is saved/restored but not editable
    [Browsable(false)]
    public bool UtcMode{ get; set; } = false;
  }
}
