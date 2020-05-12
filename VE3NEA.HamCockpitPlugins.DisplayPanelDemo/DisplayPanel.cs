using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VE3NEA.HamCockpitPlugins.DisplayPanelDemo
{
  public partial class DisplayPanel: UserControl
  {
    public DisplayPanel()
    {
      InitializeComponent();
    }

    public DisplayPanelSettings Settings
    {
      get
      {
        return new DisplayPanelSettings(
        toolStripButton1.Checked,
        toolStripButton2.Checked,
        toolStripButton3.Checked);
      }
      set 
      {
        toolStripButton1.Checked = value.Red;
        toolStripButton2.Checked = value.Green;
        toolStripButton3.Checked = value.Blue;
        ButtonsToColor();
      }
    }

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
      ButtonsToColor();
    }

    void ButtonsToColor()
    {
      BackColor = Color.FromArgb(
        toolStripButton1.Checked ? 255 : 180,
        toolStripButton2.Checked ? 255 : 180,
        toolStripButton3.Checked ? 255 : 180);
    }
  }


  // this class is a container for all settings of a display panel
  // that we want to be preserved between the program restarts.
  // in this demo, we just store the three components of the panel color.
  // each component is toggled with its toolbutton on the display panel.
  public class DisplayPanelSettings
  {
    public bool Red { get; set; }
    public bool Green { get; set; }
    public bool Blue { get; set; }

    public DisplayPanelSettings(bool red, bool green, bool blue)
    {
      Red = red;
      Green = green;
      Blue = blue;
    }
  }
}
