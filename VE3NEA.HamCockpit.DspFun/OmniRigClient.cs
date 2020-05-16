using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using OmniRig;

namespace VE3NEA.HamCockpit.DspFun
{
  /// <summary>The status of the CAT connection to the radio.</summary>
  public enum RigStatus
  {
    /// <summary>OmniRig is not installed.</summary>
    NotInstalled = -1,
    /// <summary>OmniRig is not configured.</summary>
    NotConfigured = 0,  
    /// <summary>The CAT interface is disabled.</summary>
    Disabled = 1,       
    /// <summary>  The COM port is used by another program.</summary>
    PortBusy = 2,
    /// <summary>The radio is not responding.</summary>
    NotResponding = 3,
    /// <summary>CAT interface OK.</summary>
    Online = 4
  }

  /// <summary>A wrapper around the OmniRig COM object.</summary>
  public class OmniRigClient
  {
    private IOmniRigX omniRig = null;
    private IOmniRigXEvents_Event events;
    private IRigX rig = null;

    private SynchronizationContext context = SynchronizationContext.Current;
    private int rxFrequency;
    private RigParamX mode;

    /// <summary>Occurs when the dial frequency changes.</summary>
    public event EventHandler Tuned;

    /// <summary>Occurs when the mode changes.</summary>
    public event EventHandler ModeChanged;

    /// <summary>Occurs when the CAT connection status changes.</summary>
    public event EventHandler StatusChanged;

    /// <summary>Gets or sets a value indicating whether this <see cref="OmniRigClient" /> is active.</summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    public bool Active { get => omniRig != null; set => SetActive(value); }

    /// <summary>Gets or sets the rig number.</summary>
    /// <value>The number of the rig to use, 1 or 2.</value>
    public int RigNo { get; set; }

    /// <summary>Gets or sets the RX frequency.</summary>
    /// <value>The RX frequency.</value>
    public int RxFrequency { get => rxFrequency; set => SetRxFrequency(value); }

    /// <summary>Gets or sets the mode.</summary>
    /// <value>The mode selected in the radio.</value>
    public string Mode { get => mode.ToString(); set => SetMode(value); }

    /// <summary>Gets the status of the CAT connection.</summary>
    /// <value>The status of the CAT connection.</value>
    public RigStatus Status { get; private set; }

    private void SetActive(bool value)
    {
      if (value == Active) return;
      else if (RigNo == 0) Status = RigStatus.Disabled;
      else if (value) Start();
      else Stop();

      OnStatusChanged(RigNo);
    }

    private void Start()
    {
      try
      {
        omniRig = new OmniRigX();

        rig = (RigNo == 1) ? omniRig.Rig1 : omniRig.Rig2;

        events = omniRig as IOmniRigXEvents_Event;
        events.ParamsChange += ParamsChangedEventHandler;
        events.StatusChange += StatusChangedEventHandler;
      }
      catch
      {
        Stop();
        Status = RigStatus.NotInstalled;
        return;
      }
    }

    //release all com interfaces so that OmniRig can shut down
    private void Stop()
    {
      if (events != null)
      {
        events.ParamsChange -= ParamsChangedEventHandler;
        events.StatusChange -= StatusChangedEventHandler;
        Marshal.ReleaseComObject(events);
        events = null;
      }

      if (rig != null) Marshal.ReleaseComObject(rig);
      rig = null;

      if (omniRig != null) Marshal.ReleaseComObject(omniRig);
      omniRig = null;

    }

    //out-of-process com objects fire events on a worker thread
    //make sure that event handlers are called on the main thread
    private void StatusChangedEventHandler(int rigNo)
    {
      context.Post(s => OnStatusChanged(rigNo), null);
    }

    private void ParamsChangedEventHandler(int rigNo, int paramsChanged)
    {
      context.Post(s => OnParamsChanged(rigNo), null);
    }

    private void OnStatusChanged(int rigNo)
    {
      if (rigNo != RigNo) return;

      Status = (rig == null) ? RigStatus.Disabled : (RigStatus)rig.Status;
      StatusChanged?.Invoke(this, new EventArgs());

      //todo: call OnParamsChanged instead?
      ParamsChangedEventHandler(rigNo, 0);
    }
    
    private void OnParamsChanged(int rigNo)
    {
      if (rigNo != RigNo) return;
      if (rig == null) return;

      int newFrequency = (rig.Status == RigStatusX.ST_ONLINE) ? rig.GetRxFrequency() : 0;
      if (newFrequency != RxFrequency)
      {
        rxFrequency = newFrequency;
        Tuned?.Invoke(this, new EventArgs());
      }

      var newMode = rig.Mode;
      if (newMode != mode)
      {
        mode = newMode;
        ModeChanged?.Invoke(this, new EventArgs());
      }
    }

    private void SetRxFrequency(int value)
    {
      if (rig == null) return;
      if (rig.Vfo != RigParamX.PM_VFOAB) rig.Vfo = RigParamX.PM_VFOAB;
      if ((rig.WriteableParams & (int)RigParamX.PM_FREQA) != 0) rig.FreqA = value; else rig.Freq = value;
    
      //throw new Exception("Bang! Simulated exception.");
    }

    private void SetMode(string value)
    {
      var mode = (RigParamX)Enum.Parse(typeof(RigParamX), value);
      if (rig == null) return;
      rig.Mode = mode;
    }

    /// <summary>Gets the text that describes the current status of OmniRig.</summary>
    /// <returns>The status text.</returns>
    public string GetStatusText()
    {
      switch (Status)
      {
        case RigStatus.NotInstalled:  return "OmniRig is not installed";
        case RigStatus.NotConfigured: return "OmniRig not configured";
        case RigStatus.Disabled:      return "CAT interface is disabled";
        case RigStatus.PortBusy:      return "COM port is not available";
        case RigStatus.NotResponding: return "Radio is not responding";
        case RigStatus.Online:        return "'CAT interface OK";
        default: return "";
      }
    }

    /// <summary>Shows the OmniRig configuration dialog.</summary>
    public void ShowDialog()
    {
      if (omniRig != null) omniRig.DialogVisible = true;
    }
  }
}
