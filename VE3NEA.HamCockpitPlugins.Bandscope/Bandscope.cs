using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VE3NEA.HamCockpit.PluginAPI;
using System.ComponentModel.Composition;
using System.Drawing;
using VE3NEA.HamCockpit.DspFun;

namespace VE3NEA.HamCockpitPlugins.Bandscope
{
  [Export(typeof(IPlugin))]
  [Export(typeof(IVisualPlugin))]
  class Bandscope : IPlugin, IVisualPlugin
  {
    #pragma warning disable 0649
    [ImportMany(typeof(IBandpassFilter))]
    internal List<IBandpassFilter> Filters;
    #pragma warning restore 0649

    internal readonly IPluginHost host;
    internal readonly PowerSpectrum Spect = new PowerSpectrum();
    internal Settings settings = new Settings();
    private BandscopeControl panel;
    private int outDialBin;
    internal float PixPerHz;
    internal Int64 CurrentFrequency;
    internal IBandpassFilter filter;
    internal bool tunePending;

    [ImportingConstructor]
    Bandscope([Import(typeof(IPluginHost))] IPluginHost host)
    {
      this.host = host;
      host.DspPipeline.Tuner.Tuned += TunedEventhandler;
      host.DspPipeline.StatusChanged += StatusEventhandler;
      host.DspPipeline.InputSignal.SamplesAvailable += Spect.SamplesAvailableEventHandler;
      host.DspPipeline.ModeSwitch.ModeChanged += FilterChangedEventHandler;

      Spect.SpectraAvailable += SpectraAvailableEventHandler;
    }

    #region IPlugin

    public string Name => "Band Scope";
    public string Author => "VE3NEA";
    public bool Enabled { get; set; }
    public object Settings { get => settings; set => ApplySettings(value as Settings); }
    public ToolStrip ToolStrip => null;
    public ToolStripItem StatusItem => null;
    #endregion

    #region IVisualPlugin
    public bool CanCreatePanel => panel == null;
    public UserControl CreatePanel()
    {
      panel = new BandscopeControl();
      panel.plugin = this;
      panel.Name = "Band Scope";
      panel.PrepareGraphics();
      SetActive();
      return panel;
    }
    public void DestroyPanel(UserControl panel)
    {
      this.panel = null;
      SetActive();
    }
    #endregion

    #region event handlers
    private void StatusEventhandler(object sender, EventArgs e)
    {
      SetActive();
    }

    private void SpectraAvailableEventHandler(object sender, SpectraEventArgs e)
    {
      if (!IsActive() || tunePending) return;
      var spectrum = e.Spectra[e.Spectra.Count - 1];

      int len = spectrum.Length;
      int binB = 0;
      int clientWidth = panel.ClientSize.Width;
      int pixB = (int)Math.Round(clientWidth * settings.DialPosOnScreen) - outDialBin;
      int pixE = pixB + spectrum.Length;

      if (pixB < 0)
      {
        binB = -pixB;
        pixB = 0;
        len -= binB;
      }
      if (pixE > clientWidth) len -= pixE - clientWidth;

      int spectHeight = panel.ClientRectangle.Height - BandscopeControl.SCALE_HEIGHT;
      panel.points = new Point[len + 2];

      int x0 = panel.ClientRectangle.Left + pixB;
      int y0 = panel.ClientRectangle.Top + spectHeight;
      for (int i = 0; i < len; i++)
        panel.points[i] = new Point(x0 + i, y0 - 30 - (int)(1.5 * spectrum[binB + i]));
      panel.points[len] = new Point(x0 + len - 1, y0);
      panel.points[len + 1] = new Point(x0, y0);

      //todo: try Refresh()
      panel.Invalidate();
    }

    private void TunedEventhandler(object sender, EventArgs e)
    {
      tunePending = false;
      Spect.Pause = false;

      Int64 freq = host.DspPipeline.Tuner.GetDialFrequency();
      //Int64 binFrom = (Int64)Math.Round(CurrentFrequency * PixPerHz);
      //Int64 binTo = (Int64)Math.Round(freq * PixPerHz);
      int dx = (int)Math.Round((CurrentFrequency - freq) * PixPerHz);
      CurrentFrequency = freq;

      Spect.ShiftSmoothedSpectrum(dx);
      if (panel != null) panel.BackDirty = true;
    }
    #endregion


    private void SetActive()
    {
      bool active = IsActive();

      if (active)
      {
        Spect.Initialize(host.DspPipeline.InputSignal.Format, settings.SpectraPerSecond);
        panel.Slider.Value = settings.ZoomSliderPos; //calls SetZoom() in the process
        filter = Filters.Where(f => (f as IPlugin).Enabled).FirstOrDefault();
        if (filter != null) filter.PassbandChanged += FilterChangedEventHandler;
        panel.IsUpperSideband = host.DspPipeline.ModeSwitch.Sideband == Sideband.Upper;
        panel.Mode = host.DspPipeline.ModeSwitch.Mode;
      }
      else if (panel != null)
      {
        panel.points = null;
        panel.BackDirty = true;
        panel.Invalidate();
      }

      if (!active && filter != null) filter.PassbandChanged -= FilterChangedEventHandler;
      Spect.Active = active;
    }

    internal bool IsActive()
    {
      return panel != null && host.DspPipeline.Active;
    }

    internal void SetZoom()
    {
      panel.BackDirty = true;

      if (!IsActive()) return;

      //input bandwidth
      var format = host.DspPipeline.InputSignal.Format;
      float inBinsPerHz = Spect.FftSize / (float)format.SamplingRate;
      int minInBin = Spect.FftSize / 2 + (int)Math.Round(format.PassbandLow * inBinsPerHz);
      int maxInBin = Spect.FftSize / 2 + (int)Math.Round(format.PassbandHigh * inBinsPerHz);
      int maxInBinCount = maxInBin - minInBin + 1;

      //slider to BinsPerPix
      float maxBinsPerPix = maxInBinCount / (float)panel.ClientSize.Width;
      float BinsPerPix = (float)Math.Pow(maxBinsPerPix, 1 - panel.Slider.Value * 0.01);
      BinsPerPix = Math.Max(1f, Math.Min(maxBinsPerPix, BinsPerPix));
      settings.ZoomSliderPos = panel.Slider.Value;
      panel.toolTip1.SetToolTip(panel.Slider, $"Zoom {BinsPerPix:F2}");

      //output spect size,  2 * Width if possible
      int outBinCount = Math.Min(panel.ClientSize.Width * 2, (int)Math.Round(maxInBinCount / BinsPerPix));
      int inBinCount = (int)Math.Round(outBinCount * BinsPerPix);
      PixPerHz = inBinsPerHz / BinsPerPix;

      //input offset
      outDialBin = (int)Math.Round(outBinCount / 2 + format.DialOffset * PixPerHz);
      int inDialBin = (int)Math.Round(Spect.FftSize / 2 + format.DialOffset * inBinsPerHz);
      int desiredFirstInBin = inDialBin - (int)Math.Round(outDialBin * BinsPerPix);
      //ensure that input slice is within input bandwidth
      int firstInBin = Math.Max(minInBin, desiredFirstInBin);
      firstInBin = Math.Min(maxInBin + 1 - (int)Math.Round(outBinCount * BinsPerPix), firstInBin);
      //adjust dial pos if input slice moved
      outDialBin -= (int)Math.Round((firstInBin - desiredFirstInBin) / BinsPerPix);

      Spect.SetZoomParams(firstInBin, inBinCount, outBinCount);
    }

    private void FilterChangedEventHandler(object s, EventArgs e)
    {
      if (!IsActive()) return;
      panel.IsUpperSideband = host.DspPipeline.ModeSwitch.Sideband == Sideband.Upper;
      panel.Mode = host.DspPipeline.ModeSwitch.Mode;
      panel.BackDirty = true;
      panel.Refresh();
      panel.ShowHintIfNeeded();
    }

    private void ApplySettings(Settings settings)
    {
      this.settings = settings;

      if (panel != null)
      {
        panel.PrepareGraphics();
        panel.Invalidate();
      }
    }
  }
}
