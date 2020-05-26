using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VE3NEA.HamCockpitPlugins.ClockDemo
{
  public partial class Clock : UserControl
  {
    bool utcMode;
    public bool UtcMode { get => utcMode; set => SetUtcMode(value); }
    public bool Blink;

    public Clock()
    {
      InitializeComponent();
    }

    private void utcLabel_Click(object sender, EventArgs e)
    {
      SetUtcMode(!utcMode);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      ShowTime();
    }

    public void SetUtcMode(bool value)
    {
      utcMode = value;
      utcLabel.BackColor = utcMode ? Color.Aqua : Color.Teal;
      ShowTime();
    }

    private void ShowTime()
    {
      DateTime now = utcMode ? DateTime.UtcNow : DateTime.Now;
      string timeFormat = (Blink && (now.Millisecond > 500)) ? "HH' 'mm" : "HH:mm";
      timeLabel.RightToLeft = RightToLeft.No;
      timeLabel.Text = now.ToString(timeFormat);
      dateLabel.Text = now.ToString("MMM dd");      
    }
  }
}
