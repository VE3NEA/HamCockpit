namespace VE3NEA.HamCockpit.PluginAPI
{
  /// <summary>
  /// Describes the format and characteristics of the signal samples in the data stream
  /// </summary>
  public class SignalFormat
  {
    /// <summary>The sampling rate of audio signals.</summary>
    public const int AUDIO_SAMPLING_RATE = 12000;

    private SignalFormat prototype;

    private int? samplingRate;
    private bool? isComplex;
    private bool? isSync;
    private int? channels;
    private int? passbandLow;
    private int? passbandHigh;
    private int? dialOffset;
    private Sideband? sideband;

    /// <summary>
    /// Gets or sets the sampling rate of the signal.
    /// </summary>
    /// <value>
    /// The sampling rate.
    /// </value>    
    public int SamplingRate { get => samplingRate ?? prototype.SamplingRate; set => samplingRate = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the samples are complex or real.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the samples are complex; otherwise, <c>false</c>.
    /// </value>
    public bool IsComplex { get => isComplex ?? prototype.IsComplex; set => isComplex = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the channels of the multi-channel data stream are phase-synchronized or independent.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the channels are phase-synchronized; otherwise, <c>false</c>.
    /// </value>
    public bool IsSync      { get => isSync ?? prototype.IsSync; set => isSync = value; }

    /// <summary>
    /// Gets or sets the number of channels.
    /// </summary>
    /// <value>
    /// The number of channels in the data stream.
    /// </value>
    public int Channels     { get => channels ?? prototype.Channels; set => channels = value; }

    /// <summary>
    /// Gets or sets the lower end of the signal spectrum.
    /// </summary>
    /// <value>
    /// The lower end of the signal spectrum in Hertz.
    /// </value>
    public int PassbandLow  { get => passbandLow ?? prototype.PassbandLow; set => passbandLow = value; }

    /// <summary>
    /// Gets or sets the upper end of the signal spectrum.
    /// </summary>
    /// <value>
    /// The upper end of the signal spectrum in Hertz.
    /// </value>
    public int PassbandHigh { get => passbandHigh ?? prototype.PassbandHigh; set => passbandHigh = value; }

    /// <summary>
    /// Gets or sets the dial frequency offset.
    /// </summary>
    /// <value>
    /// The offset in Hertz of the point in the sampled bandwidth that corresponds to the dial frequency
    /// </value>
    public int DialOffset { get => dialOffset ?? prototype.DialOffset; set => dialOffset = value; }

    /// <summary>
    /// Gets or sets the sideband of the signal.
    /// </summary>
    /// <value>
    /// The sideband.
    /// </value>
    /// <remarks>The sideband is <see cref="Sideband.Upper"/> if higher
    /// frequencies in the sampled data correspond to higher RF frequencies; 
    /// <see cref="Sideband.Lower"/> otherwise.</remarks>
    public Sideband Sideband { get => sideband ?? prototype.Sideband; set => sideband = value; }

    /// <summary>
    /// Gets or sets the stage gain.
    /// </summary>
    /// <value>
    /// The gain of the current signal processing stage, in dB.
    /// </value>
    public float StageGain  { get; set; }

    /// <summary>
    /// Gets the total gain.
    /// </summary>
    /// <value>
    /// The total gain, in dB, of all signal processing stages, from the signal source to the current stage.
    /// </value>
    public float TotalGain  { get => (prototype?.TotalGain ?? 0) + StageGain; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalFormat"/> class and sets all of its properties.
    /// </summary>
    /// <param name="samplingRate">The sampling rate.</param>
    /// <param name="isComplex">The IsComplex flag.</param>
    /// <param name="isSync">The IsSync flag.</param>
    /// <param name="channels">The number of channels.</param>
    /// <param name="passbandLow">The lower end of the signal spectrum.</param>
    /// <param name="passbandHigh">The upper end of the signal spectrum.</param>
    /// <param name="dialOffset">The dial frequency offset.</param>
    /// <param name="sideband">The sideband.</param>
    /// <param name="stageGain">The stage gain.</param>
    public SignalFormat(int samplingRate, bool isComplex, bool isSync, int channels, int passbandLow,
      int passbandHigh, int dialOffset, Sideband sideband = Sideband.Upper, float stageGain = 0)
    {
      SamplingRate = samplingRate;
      IsComplex = isComplex;
      IsSync = isSync;
      Channels = channels;
      PassbandLow = passbandLow;
      PassbandHigh = passbandHigh;
      DialOffset = dialOffset;
      Sideband = sideband;
      StageGain = stageGain; 
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalFormat"/> class that describes a single-channel audio signal stream.
    /// </summary>
    public SignalFormat() : this(AUDIO_SAMPLING_RATE, false, true, 1, 0, AUDIO_SAMPLING_RATE / 2, 0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalFormat"/> class.
    /// </summary>
    /// <param name="prototype">The output format of the preceding signal processing stage.</param>
    public SignalFormat(SignalFormat prototype) { this.prototype = prototype; }
  }
}
