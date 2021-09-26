namespace VE3NEA.HamCockpitPlugins.Bandscope
{
    partial class BandscopeControl
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
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.Slider = new ColorSlider.ColorSlider();
      this.SuspendLayout();
      // 
      // Slider
      // 
      this.Slider.BackColor = System.Drawing.Color.Transparent;
      this.Slider.BarPenColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(87)))), ((int)(((byte)(94)))), ((int)(((byte)(110)))));
      this.Slider.BarPenColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(60)))), ((int)(((byte)(74)))));
      this.Slider.BorderRoundRectSize = new System.Drawing.Size(8, 8);
      this.Slider.Cursor = System.Windows.Forms.Cursors.Default;
      this.Slider.ElapsedInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
      this.Slider.ElapsedPenColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(130)))), ((int)(((byte)(208)))));
      this.Slider.ElapsedPenColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(140)))), ((int)(((byte)(180)))));
      this.Slider.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
      this.Slider.ForeColor = System.Drawing.Color.White;
      this.Slider.LargeChange = ((uint)(5u));
      this.Slider.Location = new System.Drawing.Point(5, -5);
      this.Slider.Name = "Slider";
      this.Slider.ScaleDivisions = 10;
      this.Slider.ScaleSubDivisions = 5;
      this.Slider.ShowDivisionsText = true;
      this.Slider.ShowSmallScale = false;
      this.Slider.Size = new System.Drawing.Size(93, 34);
      this.Slider.SmallChange = ((uint)(1u));
      this.Slider.TabIndex = 0;
      this.Slider.Text = "colorSlider1";
      this.Slider.ThumbInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
      this.Slider.ThumbPenColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(56)))), ((int)(((byte)(152)))));
      this.Slider.ThumbRoundRectSize = new System.Drawing.Size(16, 16);
      this.Slider.ThumbSize = new System.Drawing.Size(16, 16);
      this.Slider.TickAdd = 0F;
      this.Slider.TickColor = System.Drawing.Color.White;
      this.Slider.TickDivide = 0F;
      this.Slider.TickStyle = System.Windows.Forms.TickStyle.None;
      this.Slider.Value = 100;
      this.Slider.ValueChanged += new System.EventHandler(this.Slider_ValueChanged);
      // 
      // BandscopeControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.Slider);
      this.Cursor = System.Windows.Forms.Cursors.Cross;
      this.DoubleBuffered = true;
      this.Name = "BandscopeControl";
      this.Size = new System.Drawing.Size(544, 133);
      this.Paint += new System.Windows.Forms.PaintEventHandler(this.BandscopeControl_Paint);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BandscopeControl_MouseDown);
      this.MouseLeave += new System.EventHandler(this.BandscopeControl_MouseLeave);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BandscopeControl_MouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BandscopeControl_MouseUp);
      this.Resize += new System.EventHandler(this.BandscopeControl_Resize);
      this.ResumeLayout(false);

        }

    #endregion
    internal System.Windows.Forms.ToolTip toolTip1;
    internal ColorSlider.ColorSlider Slider;
  }
}
