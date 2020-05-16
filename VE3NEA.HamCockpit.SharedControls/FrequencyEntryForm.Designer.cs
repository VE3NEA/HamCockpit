namespace VE3NEA.HamCockpit.SharedControls
{
  partial class FrequencyEntryForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrequencyEntryForm));
      this.label1 = new System.Windows.Forms.Label();
      this.FrequencyComboBox = new System.Windows.Forms.ComboBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.TuneBtn = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(70, 7);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(128, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "New Dial Frequency, kHz";
      // 
      // FrequencyComboBox
      // 
      this.FrequencyComboBox.FormatString = "N2";
      this.FrequencyComboBox.FormattingEnabled = true;
      this.FrequencyComboBox.Location = new System.Drawing.Point(73, 23);
      this.FrequencyComboBox.Name = "FrequencyComboBox";
      this.FrequencyComboBox.Size = new System.Drawing.Size(125, 21);
      this.FrequencyComboBox.TabIndex = 1;
      this.FrequencyComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrequencyEdit_KeyDown);
      this.FrequencyComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBox1_KeyPress);
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(12, 12);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(32, 32);
      this.pictureBox1.TabIndex = 2;
      this.pictureBox1.TabStop = false;
      // 
      // TuneBtn
      // 
      this.TuneBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.TuneBtn.Location = new System.Drawing.Point(213, 21);
      this.TuneBtn.Name = "TuneBtn";
      this.TuneBtn.Size = new System.Drawing.Size(62, 23);
      this.TuneBtn.TabIndex = 3;
      this.TuneBtn.Text = "Tune";
      this.TuneBtn.UseVisualStyleBackColor = true;
      // 
      // FrequencyEntryForm
      // 
      this.AcceptButton = this.TuneBtn;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(285, 57);
      this.Controls.Add(this.TuneBtn);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.FrequencyComboBox);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FrequencyEntryForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Tune to Frequency";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrequencyEntryForm_FormClosing);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FrequencyComboBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        internal System.Windows.Forms.Button TuneBtn;
    }
}