using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Text;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpit.SharedControls
{
  /// <summary>A visual control that displays and sets the frequency.</summary>
  /// <remarks>
  /// This control is used in the <c>Frequency Display</c> and <c>Waterfall Display</c> plugins
  /// to show and control the RX and TX frequencies if the radio. Other plugins may reuse it 
  /// if they need the frequency display and control functions in their visual interface.
  /// </remarks>
  public partial class FrequencyDisplayPanel : UserControl
  {
    /// <summary>A reference to the <see cref="IPluginHost.DspPipeline"/>e in the hosting application.</summary>
    /// <remarks>
    /// Assign a reference to the <see cref="IPluginHost.DspPipeline"/> to this field before using this control.
    /// </remarks>
    public IDspPipeline pipeline;
    /// <summary>A reference to the band plan object.</summary>
    /// <remarks>
    /// Assign a reference to the band plan object to this field before using this control.
    /// </remarks>
    public IBandPlan bandplan;

    private readonly FrequencyEntryForm FrequencyDialog = new FrequencyEntryForm();

    /// <summary>
    /// The page size for the Page Up and Page Down command.
    /// </summary>
    /// <remarks>The default page size is 10 kHz.</remarks>
    public int PageSizeHz = 10000;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrequencyDisplayPanel" /> class.
    /// </summary>
    public FrequencyDisplayPanel()
    {
      InitializeComponent();
      FrequencyLabel.Font = new Font(LedFont.Family, FrequencyLabel.Font.Size);
    }

    private void FrequencyDisplayPanel_Resize(object sender, EventArgs e)
    {
      centralPanel.Top = (ClientSize.Height - centralPanel.Height) / 2;
    }

    private void FrequencyLabel_Resize(object sender, EventArgs e)
    {
      Width = TuneButton.Width + FrequencyLabel.Width + SplitModePanel.Width;
    }

    private void TuneButton_MouseDown(object sender, MouseEventArgs e)
    {
      if (!pipeline.Active) return;

      string mode = pipeline.ModeSwitch.Mode;
      var qsyDirection = (e.Button == MouseButtons.Left) ? QsyDirection.Up : QsyDirection.Down;
      var freq = pipeline.Tuner.GetDialFrequency();

      if (Control.ModifierKeys.HasFlag(Keys.Control))
        freq = bandplan.GetBandStart(freq, mode, qsyDirection);
      else if (Control.ModifierKeys.HasFlag(Keys.Alt))
        freq = bandplan.GetSegment(freq, qsyDirection).DefaultFrequency;
      else
        freq += (qsyDirection == QsyDirection.Up) ? PageSizeHz : -PageSizeHz;

      pipeline.Tuner.SetDialFrequency(freq);
    }

    private void FrequencyLabel_MouseDown(object sender, MouseEventArgs e)
    {
      if (!pipeline.Active) return;

      if (e.Button == MouseButtons.Left)
      {
        //go to band start
        string mode = pipeline.ModeSwitch.Mode;
        var freq = pipeline.Tuner.GetDialFrequency();
        freq = bandplan.GetBandStart(freq, mode, QsyDirection.Here);
        pipeline.Tuner.SetDialFrequency(freq);
      }
      else
      {
        FrequencyDialog.Location = Cursor.Position;
        FrequencyDialog.ShowDialog();
        if (FrequencyDialog.EnteredFrequency > 0)
          pipeline.Tuner.SetDialFrequency(FrequencyDialog.EnteredFrequency);
      }

    }

    private void SplitLabel_MouseDown(object sender, MouseEventArgs e)
    {
      if (!pipeline.Active) return;

      //toggle Split
      if (e.Button == MouseButtons.Left)
        pipeline.Transmitter.Split = !pipeline.Transmitter.Split;

      //set TX offset to 0
      else if (pipeline.Transmitter.Split)
        pipeline.Transmitter.Frequency = pipeline.Tuner.GetDialFrequency();
    }

    /// <summary>Updates the displayed frequency and split information.</summary>
    /// <remarks>
    /// This method reads the dial frequency, split status and split offset 
    /// from the <see cref="IPluginHost.DspPipeline"/> and updates the 
    /// displayed information.
    /// </remarks>
    public void UpdateDisplayedInfo()
    {
      var transmit = pipeline.Transmitter.Transmit;
      var split = pipeline.Transmitter.Split;

      //main frequency
      var currentFrequency = pipeline.Active ? pipeline.Tuner.GetDialFrequency() : 0;
      var color = Color.Aqua;
      if (transmit) color = Color.Red; else if (currentFrequency == 0) color = Color.Teal;
      FrequencyLabel.ForeColor = color;

      var freq = (transmit && split) ? pipeline.Transmitter.Frequency : currentFrequency;
      FrequencyLabel.Text = freq.ToString("0,000,.00").Replace(",", ".");

      //split offset
      if (split)
      {
        SplitModeLabel.BackColor = Color.Red;
        var offset = (pipeline.Transmitter.Frequency - currentFrequency) / 1000f;
        if (offset > 99.99) SplitModeLabel.Text = ">>>>";
        else if (offset < -99.99) SplitModeLabel.Text = "<<<<";
        else SplitModeLabel.Text = offset.ToString("+0.00;-0.00");
      }
      else
      {
        SplitModeLabel.BackColor = Color.MidnightBlue;
        SplitModeLabel.Text = "+0.00";
      }
    }
  }
}
