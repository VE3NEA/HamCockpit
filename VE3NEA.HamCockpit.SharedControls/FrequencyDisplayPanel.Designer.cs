namespace VE3NEA.HamCockpit.SharedControls
{
    partial class FrequencyDisplayPanel
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrequencyDisplayPanel));
      this.centralPanel = new System.Windows.Forms.Panel();
      this.SplitModePanel = new System.Windows.Forms.Panel();
      this.SplitModeLabel = new System.Windows.Forms.Label();
      this.FrequencyLabel = new System.Windows.Forms.Label();
      this.TuneButton = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.centralPanel.SuspendLayout();
      this.SplitModePanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // centralPanel
      // 
      this.centralPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.centralPanel.BackColor = System.Drawing.Color.MidnightBlue;
      this.centralPanel.Controls.Add(this.SplitModePanel);
      this.centralPanel.Controls.Add(this.FrequencyLabel);
      this.centralPanel.Controls.Add(this.TuneButton);
      this.centralPanel.Location = new System.Drawing.Point(0, 0);
      this.centralPanel.Name = "centralPanel";
      this.centralPanel.Size = new System.Drawing.Size(196, 27);
      this.centralPanel.TabIndex = 0;
      // 
      // SplitModePanel
      // 
      this.SplitModePanel.Controls.Add(this.SplitModeLabel);
      this.SplitModePanel.Dock = System.Windows.Forms.DockStyle.Left;
      this.SplitModePanel.Location = new System.Drawing.Point(149, 0);
      this.SplitModePanel.Name = "SplitModePanel";
      this.SplitModePanel.Size = new System.Drawing.Size(47, 27);
      this.SplitModePanel.TabIndex = 2;
      // 
      // SplitModeLabel
      // 
      this.SplitModeLabel.Cursor = System.Windows.Forms.Cursors.Hand;
      this.SplitModeLabel.ForeColor = System.Drawing.Color.White;
      this.SplitModeLabel.Location = new System.Drawing.Point(0, 11);
      this.SplitModeLabel.Name = "SplitModeLabel";
      this.SplitModeLabel.Size = new System.Drawing.Size(40, 13);
      this.SplitModeLabel.TabIndex = 0;
      this.SplitModeLabel.Text = "+00.00";
      this.SplitModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.toolTip1.SetToolTip(this.SplitModeLabel, "Click to toggle split mode");
      this.SplitModeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SplitLabel_MouseDown);
      // 
      // FrequencyLabel
      // 
      this.FrequencyLabel.AutoSize = true;
      this.FrequencyLabel.Cursor = System.Windows.Forms.Cursors.Hand;
      this.FrequencyLabel.Dock = System.Windows.Forms.DockStyle.Left;
      this.FrequencyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FrequencyLabel.ForeColor = System.Drawing.Color.Teal;
      this.FrequencyLabel.Location = new System.Drawing.Point(29, 0);
      this.FrequencyLabel.Name = "FrequencyLabel";
      this.FrequencyLabel.Size = new System.Drawing.Size(120, 31);
      this.FrequencyLabel.TabIndex = 0;
      this.FrequencyLabel.Text = "0.000.00";
      this.FrequencyLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      this.toolTip1.SetToolTip(this.FrequencyLabel, "Left Click to go to the band start\r\nRight Click to enter the frequency");
      this.FrequencyLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FrequencyLabel_MouseDown);
      this.FrequencyLabel.Resize += new System.EventHandler(this.FrequencyLabel_Resize);
      // 
      // TuneButton
      // 
      this.TuneButton.Cursor = System.Windows.Forms.Cursors.Hand;
      this.TuneButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.TuneButton.Image = ((System.Drawing.Image)(resources.GetObject("TuneButton.Image")));
      this.TuneButton.Location = new System.Drawing.Point(0, 0);
      this.TuneButton.Name = "TuneButton";
      this.TuneButton.Size = new System.Drawing.Size(29, 27);
      this.TuneButton.TabIndex = 1;
      this.toolTip1.SetToolTip(this.TuneButton, "Left-Click to tune up\r\nRight-Click to tune down\r\nHold Ctrl down when clicking to " +
        "change band");
      this.TuneButton.UseVisualStyleBackColor = true;
      this.TuneButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TuneButton_MouseDown);
      // 
      // toolTip1
      // 
      this.toolTip1.AutoPopDelay = 10000;
      this.toolTip1.InitialDelay = 1000;
      this.toolTip1.ReshowDelay = 100;
      this.toolTip1.ShowAlways = true;
      this.toolTip1.ToolTipTitle = "Frequency Panel";
      // 
      // FrequencyDisplayPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.MidnightBlue;
      this.Controls.Add(this.centralPanel);
      this.Name = "FrequencyDisplayPanel";
      this.Size = new System.Drawing.Size(196, 27);
      this.Resize += new System.EventHandler(this.FrequencyDisplayPanel_Resize);
      this.centralPanel.ResumeLayout(false);
      this.centralPanel.PerformLayout();
      this.SplitModePanel.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel centralPanel;
        private System.Windows.Forms.Panel SplitModePanel;
        internal System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label FrequencyLabel;
        private System.Windows.Forms.Label SplitModeLabel;
        private System.Windows.Forms.Button TuneButton;
    }
}
