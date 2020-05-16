using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;
using System.Windows.Forms;
using System.ComponentModel.Composition;

namespace VE3NEA.HamCockpitPlugins.DisplayPanelDemo
{
  // the plugin class. It does not do anything interesting on its own, it just
  // creates DisplayPanel's when asked, and keeps track of their settings.
  [Export(typeof(IPlugin))]
  [Export(typeof(IVisualPlugin))]
  class DisplayPanelDemo : IPlugin, IVisualPlugin
  {
    private List<DisplayPanel> panels = new List<DisplayPanel>();
    private Settings settings = new Settings();




    //----------------------------------------------------------------------------------------------
    //                                        IPlugin
    //----------------------------------------------------------------------------------------------
    public string Name => "Display Panel Demo";
    public string Author => "VE3NEA"; 
    public bool Enabled { get; set; }    
    public ToolStrip ToolStrip => null;
    public ToolStripItem StatusItem => null;

    // let the host save and load our settings, and allow the user edit those that are 
    // marked as browsable. No browsable settings in this demo though.
    public object Settings { get => BuildSettings(); set => settings = value as Settings; }

    // we will create as many panels as the host needs
    public bool CanCreatePanel => true;




    //----------------------------------------------------------------------------------------------
    //                                      IDisplayPanel
    //----------------------------------------------------------------------------------------------
    public UserControl CreatePanel()
    {
      var new_panel = new DisplayPanel();

      // on program startup the plugin receives its settings that were saved on program exit.
      // In our case, they contain the settings of all display panels that were open.
      // Now the host re-creates the panels, and we use the saved panels' settings
      // when we are asked to create a panel. After all settings are consumed, we create panels
      // with default settings.
      if (settings.panels.Count > 0)
      {
        new_panel.Settings = settings.panels[0];
        settings.panels.RemoveAt(0);
      }

      panels.Add(new_panel);
      return new_panel;
    }

    public void DestroyPanel(UserControl panel)
    {
      // we keep track of all open panels so that we can generate our settings when asked by host.
      panels.Remove(panel as DisplayPanel);
    }




    private object BuildSettings()
    {
      settings.panels.Clear();
      foreach (var panel in panels) settings.panels.Add(panel.Settings);
      return settings;
    }
  }

  // this class is a container for all settings of the plugin
  // that we want to be preserved between the program restarts.
  // in this demo, we just store the settings of each open display panel
  public class Settings
  {
    public List<DisplayPanelSettings> panels = new List<DisplayPanelSettings>();
  }
}