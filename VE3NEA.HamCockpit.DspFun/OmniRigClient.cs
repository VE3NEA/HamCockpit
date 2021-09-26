using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading;
using OmniRig;
using VE3NEA.HamCockpit.PluginAPI;

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

    private int pendingRxFrequency, rxFrequency;
    private int pendingTxFrequency, txFrequency;
    private RigParamX pendingMode, mode;
    private RigParamX pendingSplit, split;
    private RigParamX pendingTransmit, transmit;

    /// <summary>Occurs when the dial frequency changes.</summary>
    public event EventHandler Tuned;

    /// <summary>Occurs when the mode changes.</summary>
    public event EventHandler ModeChanged;

    /// <summary>Occurs when the Transmit mode changes.</summary>
    public event EventHandler TransmitChanged;

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
    public int RxFrequency { get => GetRxFrequency(); set => SetRxFrequency(value); }

    /// <summary>Gets or sets the TX frequency.</summary>
    /// <value>The TX frequency.</value>
    public int TxFrequency { get => GetTxFrequency(); set => SetTxFrequency(value); }

    /// <summary>Gets or sets the mode.</summary>
    /// <value>The mode selected in the radio.</value>
    public string Mode { get => GetMode(); set => SetMode(value); }

    /// <summary>Gets the status of the CAT connection.</summary>
    /// <value>The status of the CAT connection.</value>
    public RigStatus Status { get; private set; }

    /// <summary>Gets or sets the Split mode.</summary>
    /// <value>The Split mode selected in the radio.</value>
    public bool Split { get => GetSplit(); set => SetSplit(value); }

    /// <summary>Gets or sets the Transmit mode.</summary>
    /// <value>The Transmit mode selected in the radio.</value>
    public bool Transmit { get => GetTransmit(); set => SetTransmit(value); }

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
        ClearPending();

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
      ClearPending();

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

    private void ClearPending()
    {
      pendingRxFrequency = pendingTxFrequency = 0;
      pendingMode = pendingSplit = pendingTransmit = RigParamX.PM_UNKNOWN;
  }

  //out-of-process com objects fire events on a worker thread
  //make sure that event handlers are called on the main thread
  private void StatusChangedEventHandler(int rigNo)
    {
      if (rig.Status != RigStatusX.ST_ONLINE) ClearPending();
      context.Post(s => OnStatusChanged(rigNo), null);
    }

    private void ParamsChangedEventHandler(int rigNo, int paramsChanged)
    {
      context.Post(s => OnParamsChanged(rigNo, paramsChanged), null);
    }

    private const int ALL_PARAMS = -1;
    private const int FREQ_PARAMS = (int)RigParamX.PM_FREQ | (int)RigParamX.PM_FREQA | (int)RigParamX.PM_FREQB;

    private void OnStatusChanged(int rigNo)
    {
      if (rigNo != RigNo) return;

      Status = (rig == null) ? RigStatus.Disabled : (RigStatus)rig.Status;
      StatusChanged?.Invoke(this, new EventArgs());

      OnParamsChanged(rigNo, ALL_PARAMS);
    }
    
    private void OnParamsChanged(int rigNo, int paramsChanged)
    {
      if (rigNo != RigNo) return;
      if (rig == null) return;

      //rx frequency
      int newFrequency = rig.GetRxFrequency();
      if (newFrequency != rxFrequency)
      {
        rxFrequency = newFrequency;
        pendingRxFrequency = 0;
        Tuned?.Invoke(this, new EventArgs());
      }

      //tx frequency
      newFrequency = rig.GetTxFrequency();
      if (newFrequency != txFrequency)
      {
        txFrequency = newFrequency;
        pendingTxFrequency = 0;
        Tuned?.Invoke(this, new EventArgs());
      }

      //mode
      if (rig.Mode != mode)
      {
        mode = rig.Mode;
        pendingMode = RigParamX.PM_UNKNOWN;
        ModeChanged?.Invoke(this, new EventArgs());
      }

      //split and transmit
      bool txChanged = rig.Split != split || rig.Tx != transmit;

      if (rig.Split != split)
      {
        split = rig.Split;
        pendingSplit = RigParamX.PM_UNKNOWN;
      }
      if (rig.Tx != transmit)
      {
        transmit = rig.Tx;
        pendingTransmit = RigParamX.PM_UNKNOWN;
      }
      if (txChanged) TransmitChanged?.Invoke(this, new EventArgs());
    }

    private int GetRxFrequency()
    {
      return pendingRxFrequency != 0 ? pendingRxFrequency : rxFrequency;
    }


    private int GetTxFrequency()
    {
      return pendingTxFrequency != 0 ? pendingTxFrequency : txFrequency;
    }


    private string GetMode()
    {
      var modeCode = pendingMode == RigParamX.PM_UNKNOWN ? mode : pendingMode;
      return modeCode.ToString();
    }

    private bool GetSplit()
    {
      var splitCode = pendingSplit == RigParamX.PM_UNKNOWN ? this.split : pendingSplit;
      return splitCode == RigParamX.PM_SPLITON;
    }
    private bool GetTransmit()
    {
      var transmitCode = pendingTransmit == RigParamX.PM_UNKNOWN ? transmit : pendingTransmit;
      return transmitCode == RigParamX.PM_TX;
    }


    private void SetRxFrequency(int value)
    {
      if (value == rxFrequency) return;
      pendingRxFrequency = value;

      if (rig == null) return;
      
      if (rig.Vfo == RigParamX.PM_VFOB || rig.Vfo == RigParamX.PM_VFOBB || rig.Vfo == RigParamX.PM_VFOBA)
        rig.FreqB = value;
      else if ((rig.WriteableParams & (int)RigParamX.PM_FREQA) != 0) 
        rig.FreqA = value;
      else
        rig.Freq = value;
    }

    private void SetTxFrequency(int value)
    {
      if (value == txFrequency) return;
      pendingTxFrequency = value;

      if (rig == null) return;

      (new Thread(DoSetTxFrequency)).Start();
    }


    private void DoSetTxFrequency()
    {
      if (rig.Vfo == RigParamX.PM_VFOB || rig.Vfo == RigParamX.PM_VFOAB || rig.Vfo == RigParamX.PM_VFOBB)
        rig.FreqB = pendingTxFrequency;
      else if ((rig.WriteableParams & (int)RigParamX.PM_FREQA) != 0)
        rig.FreqA = pendingTxFrequency;
      else
        rig.Freq = pendingTxFrequency;
    }

    private void SetMode(string value)
    {
      var newMode = (RigParamX)Enum.Parse(typeof(RigParamX), value);
      if (newMode == mode) return;
      pendingMode = newMode;

      if (rig != null) rig.Mode = newMode;
    }

    private void SetSplit(bool value)
    {
      var newSplit = value ? RigParamX.PM_SPLITON : RigParamX.PM_SPLITOFF;
      if (newSplit == split) return;
      pendingSplit = newSplit;

      if (rig != null) rig.Split = newSplit;
    }

    private void SetTransmit(bool value)
    {
      var newTransmit = value ? RigParamX.PM_TX : RigParamX.PM_RX;
      if (newTransmit == transmit) return;
      pendingTransmit = newTransmit;

      if (rig != null) rig.Tx = newTransmit;
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
