namespace VE3NEA.HamCockpitPlugins.ClockDemo
{
  partial class Clock
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.timeLabel = new System.Windows.Forms.Label();
      this.dateLabel = new System.Windows.Forms.Label();
      this.utcLabel = new System.Windows.Forms.Label();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // timeLabel
      // 
      this.timeLabel.AutoSize = true;
      this.timeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.timeLabel.ForeColor = System.Drawing.Color.Aqua;
      this.timeLabel.Location = new System.Drawing.Point(0, 1);
      this.timeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.timeLabel.Name = "timeLabel";
      this.timeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.timeLabel.Size = new System.Drawing.Size(66, 26);
      this.timeLabel.TabIndex = 0;
      this.timeLabel.Text = "00:00";
      this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dateLabel
      // 
      this.dateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.dateLabel.AutoSize = true;
      this.dateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.dateLabel.ForeColor = System.Drawing.Color.Aqua;
      this.dateLabel.Location = new System.Drawing.Point(62, 2);
      this.dateLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.dateLabel.Name = "dateLabel";
      this.dateLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.dateLabel.Size = new System.Drawing.Size(42, 13);
      this.dateLabel.TabIndex = 1;
      this.dateLabel.Text = "Dec 31";
      this.dateLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // utcLabel
      // 
      this.utcLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.utcLabel.AutoSize = true;
      this.utcLabel.BackColor = System.Drawing.Color.Aqua;
      this.utcLabel.Cursor = System.Windows.Forms.Cursors.Hand;
      this.utcLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.utcLabel.Location = new System.Drawing.Point(79, 17);
      this.utcLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.utcLabel.Name = "utcLabel";
      this.utcLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.utcLabel.Size = new System.Drawing.Size(21, 7);
      this.utcLabel.TabIndex = 2;
      this.utcLabel.Text = "UTC";
      this.utcLabel.Click += new System.EventHandler(this.utcLabel_Click);
      // 
      // timer1
      // 
      this.timer1.Enabled = true;
      this.timer1.Interval = 500;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // Clock
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.MidnightBlue;
      this.Controls.Add(this.utcLabel);
      this.Controls.Add(this.dateLabel);
      this.Controls.Add(this.timeLabel);
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.Name = "Clock";
      this.Size = new System.Drawing.Size(104, 28);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label timeLabel;
    private System.Windows.Forms.Label dateLabel;
    private System.Windows.Forms.Label utcLabel;
    private System.Windows.Forms.Timer timer1;
  }
}
