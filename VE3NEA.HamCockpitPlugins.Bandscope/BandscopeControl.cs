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
using System.Drawing.Imaging;
using VE3NEA.HamCockpit.PluginAPI;

namespace VE3NEA.HamCockpitPlugins.Bandscope
{

  enum DragMode { None, Down, Pan, Width, UpperEdge, LowerEdge };

  public partial class BandscopeControl : UserControl
  {
    internal const int SCALE_HEIGHT = 27;
    private const int EAR_SIZE = 8;

    internal Bandscope plugin;
    internal Point[] points;
    private Bitmap backBitmap;
    private Brush scaleBrush, backBrush, foreBrush;
    private Rectangle spectRect, scaleRect;
    internal bool BackDirty = true;

    //filter parameters
    public bool IsUpperSideband;
    public string Mode;
    int passbandLow;
    int passbandHigh;
    bool ZeroDialOffset;
    Rectangle filterRect;

    public BandscopeControl()
    {
      InitializeComponent();
      ResizeRedraw = true;
    }

    private void BandscopeControl_Resize(object sender, EventArgs e)
    {
      plugin.SetZoom();
    }

    private void Slider_ValueChanged(object sender, EventArgs e)
    {
      plugin.SetZoom();
    }

    
    
    
    //----------------------------------------------------------------------------------------------
    //                                        paint
    //----------------------------------------------------------------------------------------------
    internal void PrepareGraphics()
    {
      backBrush = new SolidBrush(plugin.settings.BackColor);
      foreBrush = new SolidBrush(plugin.settings.ForeColor);
      scaleBrush = new SolidBrush(plugin.settings.ScaleBackColor);
      //filterBrush = new SolidBrush(ControlPaint.Dark(plugin.settings.ScaleBackColor, 50));
      BackDirty = true;
    }

    private void BandscopeControl_Paint(object sender, PaintEventArgs e)
    {
      Graphics g = e.Graphics;

      if (BackDirty) BuildBackground();
      g.DrawImageUnscaled(backBitmap, 0, 0);

      //red line
      int x = GetDialPixel();
      g.DrawLine(Pens.Red, x, 0, x, ClientSize.Height - SCALE_HEIGHT - 1 + 4);

      if (points != null) g.FillPolygon(foreBrush, points); else PrintNoSignal(g);
    }

    private void PrintNoSignal(Graphics g)
    {
      string text = "no signal";
      var textSize = g.MeasureString(text, Font);
      float x = ClientRectangle.Left + (ClientRectangle.Width - textSize.Width) / 2;
      float y = ClientRectangle.Top + (ClientRectangle.Height - SCALE_HEIGHT - textSize.Height) / 2;
      g.DrawString(text, Font, foreBrush, x, y);
    }

    private void BuildBackground()
    {
      if (backBitmap == null || backBitmap.Size != ClientSize)
        backBitmap = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);

      Graphics g = Graphics.FromImage(backBitmap);

      //background
      spectRect = new Rectangle(0, 0, backBitmap.Width, backBitmap.Height - SCALE_HEIGHT);
      scaleRect = new Rectangle(0, spectRect.Height, spectRect.Width, SCALE_HEIGHT);
      g.FillRectangle(backBrush, spectRect);
      g.FillRectangle(scaleBrush, scaleRect);

      scaleRect.Height = 1;
      g.FillRectangle(Brushes.White, scaleRect);

      if (IsFilterVisible()) DrawFilter(g);
      if (plugin.IsActive()) DrawScale(g);

      //green triangle
      int x = GetDialPixel();
      DrawTriangle(g, x, Pens.Green, Brushes.Lime);

      BackDirty = false;
    }

    private void DrawTriangle(Graphics g, int xPos, Pen pen, Brush brush)
    {
      var p = new Point(xPos, scaleRect.Top + 4);
      Point[] points = { p, new Point(p.X + 7, p.Y + 7), new Point(p.X - 7, p.Y + 7) };
      g.DrawLines(pen, points);
      g.FillPolygon(brush, points);
    }

    private static readonly float[] TickMults = { 2f, 2.5f, 2f };

    private void DrawScale(Graphics g)
    {
      //tick steps
      float TickStep = 200;
      float LabelStep = 1000;
      for (int i = 0; i <= 24; ++i)
      {
        if (LabelStep * plugin.PixPerHz > 60) break;
        LabelStep *= TickMults[i % 3];
        TickStep *= TickMults[(i + 1) % 3];
      }

      //label format
      string LabelFormat;
      if (LabelStep % 1000000 == 0) LabelFormat = "F0";
      else if (LabelStep % 100000 == 0) LabelFormat = "F1";
      else if (LabelStep % 10000 == 0) LabelFormat = "F2";
      else LabelFormat = "F3";

      //first label's frequency
      Int64 absDialPixel = (Int64)Math.Round(plugin.CurrentFrequency * plugin.PixPerHz);
      Int64 absLeftPixel = absDialPixel - GetDialPixel();
      Int64 freq0 = (Int64)Math.Round(absLeftPixel / plugin.PixPerHz);
      freq0 = (Int64)Math.Round(Math.Truncate(freq0 / LabelStep) * LabelStep);

      int topY = ClientRectangle.Height - SCALE_HEIGHT;

      //draw large ticks and labels
      Int64 freq = freq0;
      while (true)
      {
        Int64 absPixel = (Int64)Math.Round(freq * plugin.PixPerHz);
        int x = (int)(absPixel - absLeftPixel);
        if (x >= Width) break;
        g.FillRectangle(Brushes.Black, x, topY, 1, 12);
        g.DrawString((freq * 1e-6).ToString(LabelFormat), Font, Brushes.Black, new Point(x + 2, topY+10));
        freq += (int)LabelStep;
      }

      //draw small ticks
      freq = freq0;
      while (true)
      {
        Int64 absPixel = (Int64)Math.Round(freq * plugin.PixPerHz);
        int x = (int)(absPixel - absLeftPixel);
        if (x >= Width) break;
        g.FillRectangle(Brushes.Black, x, topY, 1, 4);
        freq += (int)TickStep;
      }
    }

    private void DrawFilter(Graphics g)
    {
      ComputeFilterRect();
      Rectangle rect = filterRect;
      //g.FillRectangle(filterBrush, rect);
      ControlPaint.DrawBorder3D(g, rect, Border3DStyle.RaisedOuter);
      rect.Inflate(-2 - EAR_SIZE, -2);
      ControlPaint.DrawBorder3D(g, rect, Border3DStyle.SunkenInner);
      rect.Inflate(-1, -1);
      g.FillRectangle(Brushes.Aqua, rect);
    }





    //----------------------------------------------------------------------------------------------
    //                                        mouse
    //----------------------------------------------------------------------------------------------
    private int mouseDownX, mouseMoveX;
    private int mouseDownValue;
    private DragMode dragMode;

    private void BandscopeControl_MouseDown(object sender, MouseEventArgs e)
    {
      if (dragMode == DragMode.Down) { dragMode = DragMode.None; return; }
      if (e.Button != MouseButtons.Left) return;

      mouseDownX = mouseMoveX = e.Location.X;

      ComputeFilterRect();

      if (IsFilterVisible() && filterRect.Contains(e.Location))
      {
        if (ZeroDialOffset) //the modes with independent hi and lo cutoffs
          if (e.Location.X < filterRect.Left + filterRect.Width / 2)
          {
            dragMode = DragMode.LowerEdge;
            mouseDownValue = IsUpperSideband ? passbandLow : passbandHigh;
            Cursor.Current = Cursors.PanWest;
          }
          else
          {
            dragMode = DragMode.UpperEdge;
            mouseDownValue = IsUpperSideband ? passbandHigh : passbandLow;
            Cursor.Current = Cursors.PanEast;
          }
        else
        {
          dragMode = DragMode.Width;
          mouseDownValue = passbandHigh - passbandLow;
          Cursor.Current = Cursors.NoMoveHoriz;
        }
      }
      else
      {
        dragMode = DragMode.Down;
        mouseDownValue = GetDialPixel();
      }

    }

    private void BandscopeControl_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Location.X == mouseMoveX) return;
      mouseMoveX = e.Location.X;
      int delta = 10 * (e.Location.X - mouseDownX);

      switch (dragMode)
      {
        case DragMode.None:
          ShowHint(IsFilterVisible() && filterRect.Contains(e.Location));
          return;

        case DragMode.Down:
          if (e.Button == MouseButtons.Left)
          {
            dragMode = DragMode.Pan;
            Cursor.Current = Cursors.SizeWE;
          }
          break;

        case DragMode.Pan:
          int newDialX = mouseDownValue + e.Location.X - mouseDownX;
          newDialX = Math.Max(0, Math.Min(ClientRectangle.Width - 1, newDialX));
          plugin.settings.DialPosOnScreen = newDialX / (float)ClientRectangle.Width;
          break;

        case DragMode.Width:
          plugin.filter.SetBandwidth(mouseDownValue + delta);
          break;

        case DragMode.UpperEdge:
          if (IsUpperSideband) plugin.filter.SetPassband(passbandLow, mouseDownValue + delta);
          else plugin.filter.SetPassband(mouseDownValue - delta, passbandHigh);
          break;

        case DragMode.LowerEdge:
          if (IsUpperSideband) plugin.filter.SetPassband(mouseDownValue + delta, passbandHigh);
          else plugin.filter.SetPassband(passbandLow, mouseDownValue - delta);
          break;
      }
      
      BackDirty = true;
      Invalidate();
    }

    private void BandscopeControl_MouseUp(object sender, MouseEventArgs e)
    {
      //move filter box and tune
      if (e.Button == MouseButtons.Left && dragMode == DragMode.Down)
      {
        int dx = mouseDownX - GetDialPixel();
        Int64 freq = plugin.CurrentFrequency + (Int64)(dx / plugin.PixPerHz);
        plugin.settings.DialPosOnScreen = (mouseDownValue + dx) / (float)ClientRectangle.Width;
        plugin.host.DspPipeline.Tuner.SetDialFrequency(freq);
        plugin.tunePending = true;
        plugin.Spect.Pause = true;
      }

      dragMode = DragMode.None;
      Cursor.Current = Cursors.Default;
      ShowHint(false);
    }

    private void BandscopeControl_MouseLeave(object sender, EventArgs e)
    {
      ShowHint(false);
    }




    //----------------------------------------------------------------------------------------------
    //                                       filter 
    //----------------------------------------------------------------------------------------------
    private bool IsFilterVisible()
    {
      return plugin != null && plugin.IsActive() && plugin.filter != null;
    }

    private void ComputeFilterRect()
    {
      var format = (plugin.filter as ISampleStream).Format;
      passbandLow = format.PassbandLow;
      passbandHigh = format.PassbandHigh;
      int bandwidth = passbandHigh - passbandLow;
      int filterOffset = (passbandHigh + passbandLow) / 2 - format.DialOffset;
      ZeroDialOffset = format.DialOffset == 0;

      //RTTY is the only mode in which the pitch depends on the sideband
      if (IsUpperSideband && Mode != "RTTY") filterOffset = -filterOffset;

      int filterWidth = (int)(bandwidth * plugin.PixPerHz) + 6 + 2 * EAR_SIZE;

      filterRect = new Rectangle(
        GetDialPixel() - (int)(filterOffset * plugin.PixPerHz) - filterWidth / 2,
        scaleRect.Top + 8,
        filterWidth,
        19);
    }

    private int GetDialPixel()
    {
      return (int)Math.Round(ClientSize.Width * plugin.settings.DialPosOnScreen);
    }

    private void ShowHint(bool value = true)
    {
      if (value)
      {
        string text;
        int bandwidth = passbandHigh - passbandLow;
        text = (ZeroDialOffset) ? $"{passbandLow} .. {passbandHigh} Hz" : $"{bandwidth} Hz";
        if (toolTip1.GetToolTip(this) == text) return;

        ComputeFilterRect();

        toolTip1.Show(text, this, 
          filterRect.Left + filterRect.Width / 2, 
          filterRect.Top + filterRect.Height + 2);
      }
      else
        toolTip1.Hide(this);
    }

    private readonly DragMode[] filterDragModes = new DragMode[]
      { DragMode.Width, DragMode.UpperEdge, DragMode.LowerEdge };

    internal void ShowHintIfNeeded()
    {
      ShowHint(IsFilterVisible() && filterDragModes.Contains(dragMode));
    }
  }
}
