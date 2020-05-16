using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace VE3NEA.HamCockpitPlugins.SMeter
{
  public partial class SMeterControl : UserControl
  {
    private const float Pi = (float)Math.PI;
    private const float DinR = 180 / Pi;
    private readonly Color lightBg = Color.FromArgb(247, 247, 254);
    private readonly Color darkBg = Color.FromArgb(177, 177, 203);
    private readonly int topMargin = 28;


    private float FastValue = -100;
    private float SlowValue = -100;

    public SMeterControl()
    {
      InitializeComponent();
      ResizeRedraw = true;
    }

    public void ShowValue(float value)
    {
      FastValue = value;
      //the number displayed under the scale should not change too fast
      SlowValue = 0.8f * SlowValue + 0.2f * FastValue;
      Invalidate();
    }

    private void UserControl1_Paint(object sender, PaintEventArgs e)
    {
      var g = e.Graphics;
      float phi, angle, needleLength;
      PointF p1, p2;

      //background
      var rect = ClientRectangle;
      g.SmoothingMode = SmoothingMode.HighQuality;
      using (var gradientBrush = new LinearGradientBrush(rect, darkBg, lightBg, LinearGradientMode.BackwardDiagonal))
        g.FillRectangle(gradientBrush, rect);

      //arc
      var r = rect.Width / 2;
      var needleCenter = new PointF(rect.Width / 2, topMargin + r);
      var arcCenter = new PointF(needleCenter.X, topMargin + 2 * r);
      var arcRect = new RectangleF(needleCenter.X - 2 * r, topMargin, 4 * r, 4 * r);
      angle = calcAngle(Pi / 4);
      g.DrawArc(Pens.Black, arcRect, -90 - DinR * angle, 2 * DinR * angle);

      for (int i = -25; i <= 25; i++)
      {
        //ticks
        phi = Pi / 100 * i;
        angle = calcAngle(phi);
        p1 = arcCenter + ToXY(2 * r, angle);

        var tickLength = ((i % 5) == 0) ? 10 : 6;
        needleLength = Distance(p1, needleCenter) + tickLength;
        p2 = needleCenter + ToXY(needleLength, phi);

        g.DrawLine(Pens.Black, p1, p2);

        //labels
        int labelValue = (i - 25) * 2;
        if (labelValue % 20 == 0)
          using (Font labelFont = new Font("Arial", 8))
          {
            string label = $"{labelValue}";
            var strSize = g.MeasureString(label, labelFont);
            p2.X -= strSize.Width / 2;
            p2.Y -= strSize.Height + 2;
            g.DrawString(label, labelFont, Brushes.Black, p2);
          }
      }

      //value
      string text;
      if (FastValue == -100) text = "no signal"; else text = $"{Math.Round(SlowValue)} dBFS";
      using (Font valueFont = new Font("Arial", 10))
      {
        var textSize = g.MeasureString(text, valueFont);
        var textPos = new PointF(needleCenter.X - textSize.Width / 2,
          (Math.Min(rect.Height, needleCenter.Y - 20) + topMargin - textSize.Height) / 2);
        g.DrawString(text, valueFont, Brushes.Black, textPos);
      }
 
      //needle
      phi = FastValue / 200 * Pi + Pi / 4;
      angle = calcAngle(phi);
      p1 = arcCenter + ToXY(2 * r, angle);
      needleLength = Distance(p1, needleCenter) + 6;
      p2 = needleCenter + ToXY(needleLength, phi);
      g.DrawLine(Pens.Red, needleCenter, p2);

      //disk
      g.FillEllipse(Brushes.Silver, new RectangleF(needleCenter.X - 25, needleCenter.Y - 25, 50, 50));

      //border
      rect.Inflate(-1, -1);
      ControlPaint.DrawBorder3D(g, rect, Border3DStyle.SunkenInner);
    }

    private float calcAngle(float phi)
    {
      return phi - (float)Math.Asin(Math.Sin(phi) / 2);
    }

    private SizeF ToXY(float r, float angle)
    {
      return new SizeF((float)(r * Math.Sin(angle)), -(float)(r * Math.Cos(angle)));
    }

    private float Distance(PointF p1, PointF p2)
    {
      return (float)Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
    }

  }
}
