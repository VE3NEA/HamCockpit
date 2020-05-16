using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VE3NEA.HamCockpit.SharedControls
{
  /// <summary>A dialog for manual entry of the frequency.</summary>
  /// <remarks>This dialog is used in the <see cref="FrequencyDisplayPanel"/> control
  /// for manual entry of the frequency to tune to.</remarks>
  public partial class FrequencyEntryForm : Form
  {
    /// <summary>The frequency entered by the user, in Hertz.</summary>
    public Int64 EnteredFrequency;

    /// <summary>Initializes a new instance of the <see cref="FrequencyEntryForm" /> class.</summary>
    public FrequencyEntryForm()
    {
      InitializeComponent();
    }

    private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
    {
      //allow only digits, decimal point and control keys
      if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.')) e.Handled = true;
      if ((e.KeyChar == '.') && ((sender as ComboBox).Text.IndexOf('.') > -1)) e.Handled = true;
    }

    private void FrequencyEdit_KeyDown(object sender, KeyEventArgs e)
    {
      //close dialog using the Esc key
      if (e.KeyCode == Keys.Escape) this.Close();
    }

    private void FrequencyEntryForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      EnteredFrequency = 0;

      if (DialogResult == DialogResult.OK)
      {
        double result;

        if (double.TryParse(FrequencyComboBox.Text, out result))
        {
          //save entered value
          EnteredFrequency = (Int64)Math.Round(result * 1000);

          //format and add to MRU
          string formattedResult = String.Format("{0:0,0.##}", result);
          int index = FrequencyComboBox.FindStringExact(formattedResult);
          if (index >= 0) FrequencyComboBox.Items.RemoveAt(index);
          FrequencyComboBox.Items.Insert(0, formattedResult);
        }
        else
        {
          //invalid input, e.g., copy-pasted
          e.Cancel = true;
          Console.Beep();
          return;
        }
      }

      FrequencyComboBox.Text = string.Empty;
    }
  }
}
