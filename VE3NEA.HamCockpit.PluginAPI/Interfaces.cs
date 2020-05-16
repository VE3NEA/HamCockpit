using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VE3NEA.HamCockpit.PluginAPI
{
  /// <summary>Event arguments class for the <see cref="ISampleStream.SamplesAvailable"/> event</summary>
  public sealed class SamplesAvailableEventArgs : EventArgs
  {
    /// <summary>Gets the data array.</summary>
    /// <value>Audio or I/Q data.</value>
    public float[] Data { get; }
    /// <summary>Offset if the first available value</summary>
    /// <value>The offset if the first available value in the data array.</value>
    public int Offset { get; }
    /// <summary>Gets the number of values.</summary>
    /// <value>The number of available values in the data array.</value>
    public int Count { get; }
    /// <summary>Initializes a new instance of the <see cref="SamplesAvailableEventArgs" /> class.</summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    public SamplesAvailableEventArgs(float[] data, int offset, int count)
    { Data = data; Offset = offset; Count = count; }
  }

  /// <summary>Event arguments class for the <see cref="ISignalSource.Stopped"/> event</summary>
  public sealed class StoppedEventArgs : EventArgs
  {
    /// <summary>The exception that caused the data source stop.</summary>
    /// <value>The exception.</value>
    public readonly Exception Exception;
    /// <summary>Initializes a new instance of the <see cref="StoppedEventArgs" /> class.</summary>
    /// <param name="exception">The exception.</param>
    public StoppedEventArgs(Exception exception) { this.Exception = exception; }
  }

  /// <summary>The main interface implemented by all plugins.</summary>
  public interface IPlugin
  {
    /// <summary>Gets the plugin name.</summary>
    /// <value>The plugin name.</value>
    /// <remarks>This is the name that will appear in the Plugin Settings dialog, and in the View menu if the plugin has a visible panel.</remarks>
    string Name { get; }
    /// <summary>Gets the author's ID.</summary>
    /// <value>The author's ID.</value>
    /// <remarks>A unique identifier of the plugin author. A licensed radio amateur should use his callsign as identifier.</remarks>
    string Author { get; }
    /// <summary>Gets or sets a value indicating whether this <see cref="IPlugin" /> is enabled by the user.</summary>
    /// <value>
    ///   <c>true</c> if enabled; otherwise, <c>false</c>.</value>
    bool Enabled { get; set; }
    /// <summary>Gets or sets the settings object.</summary>
    /// <value>The plugin settings object.</value>
    /// <remarks>
    /// The plugin can use this object to store its settings. The host application saves the Settings
    /// object of each plugin on exit and reloads it on the next program start. The properties of the 
    /// object appear in the Plugin Settings dialog and are editable by the user.
    /// </remarks>
    object Settings { get; set; }
    /// <summary>Gets the toolstrip object.</summary>
    /// <value>The toolstrip that will appear on the application's toolbar.</value>
    ToolStrip ToolStrip { get; }
    /// <summary>Gets the status toolstrip item.</summary>
    /// <value>The toolstrip item that will appear on the application's status bar.</value>
    ToolStripItem StatusItem { get; }
  }

  /// <summary>The interface implemented by the plugins that show one or more visual panels.</summary>
  public interface IVisualPlugin
  {
    /// <summary>
    /// Gets a value indicating whether this instance can create a visual panel.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the plugin can create a visual panel; otherwise, <c>false</c>.
    /// </value>
    bool CanCreatePanel { get; }
    /// <summary>
    /// Creates a visual panel.
    /// </summary>
    /// <returns>The panel object</returns>
    UserControl CreatePanel();
    /// <summary>
    /// Destroys the panel.
    /// </summary>
    /// <param name="panel">The panel.</param>
    void DestroyPanel(UserControl panel);
  }

  /// <summary>Represents a window used to output debug information.</summary>
  public interface IDebugWindow
  {
    /// <summary>  Adds a message to the debug window.</summary>
    /// <param name="message">The message.</param>
    void LogMessage(string message);
    /// <summary>Sets the label text in the debug window.</summary>
    /// <value>The label text.</value>
    string LabelText { set; }
  }






  //------------------------------------------------------------------------------------------------
  //                                      DSP interfaces 
  //------------------------------------------------------------------------------------------------  
  ///<summary>The sideband of the signal.</summary>
  /// <remarks>
  /// The signal is <see cref="Sideband.Upper"/> sideband if higher frequencies in the sampled data
  /// correspond to higher RF frequencies. Examples of the <see cref="Sideband.Upper"/> 
  /// sideband signals are
  /// the audio demodulated in the USB mode, and I/Q (quadrature) signals. Examples of the
  /// <see cref="Sideband.Lower"/> sideband are the audio demodulated in the LSB mode, and
  /// I/Q signals with the I and Q channels swapped. If there is no direct correspondence between 
  /// the frequencies in the sampled data and RF frequencies, as in the demodulated FM signals,
  /// the signal sideband is <see cref="Sideband.None"/>.
  /// </remarks>
  public enum Sideband {
    /// <summary>The upper sideband.</summary>
    Upper,
    /// <summary>The lower sideband.</summary>
    Lower,
    /// <summary>The notion of sideband is not applicable.</summary>
    None
  };

  /// <summary>
  /// Represents a stream of floating point values 
  /// and information how the values are grouped into samples.
  /// </summary>
  public interface ISampleStream
  {
    /// <summary>Gets the format.</summary>
    /// <value>The format of the data in the stream.</value>
    SignalFormat Format { get; }
    /// <summary>Reads the specified buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset if the first value to read.</param>
    /// <param name="count">The number of values to read.</param>
    /// <returns>The number of values read.</returns>
    int Read(float[] buffer, int offset, int count);
    /// <summary>Occurs when the stream has data samples available.</summary>
    event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;
  }

  /// <summary>Represents a device that can be tuned to an RF frequency.</summary>
  public interface ITuner
  {
    /// <summary>Gets the dial frequency.</summary>
    /// <param name="channel">The channel id.</param>
    /// <returns>The dial frequency of the specified channel in Hertz.</returns>
    Int64 GetDialFrequency(int channel = 0);
    /// <summary>Sets the dial frequency of the specified channel.</summary>
    /// <param name="frequency">The dial frequency in Hertz.</param>
    /// <param name="channel">The channel id.</param>
    void SetDialFrequency(Int64 frequency, int channel = 0);
    /// <summary>Occurs when the dial frequency changes.</summary>
    event EventHandler Tuned;
  }

  /// <summary>Represents a device that produces a stream of samples 
  /// mapped to RF frequency</summary>
  public interface ISignalSource : ISampleStream, ITuner
  {
    /// <summary>Initializes this instance.</summary>
    void Initialize();
    /// <summary>Gets or sets a value indicating whether this 
    /// <see cref="ISignalSource" /> is active.</summary>
    /// <value>
    ///   <c>true</c> if active; otherwise, <c>false</c>.</value>
    bool Active { get; set; }
    /// <summary>Occurs when the signal source is no longer able to provide
    /// signal data.</summary>
    /// <remarks>
    /// This event should be fired when the device stops working for one reason or another,
    /// e.g., it is turned off, its cable is disconnected, etc. The application uses
    /// this event to stop the <see cref="IPluginHost.DspPipeline"/> and display an error message.
    /// </remarks>
    event EventHandler<StoppedEventArgs> Stopped;
  }

  /// <summary>Represents a sample stream that requires initialization.</summary>
  public interface IInitSampleStream : ISampleStream
  {
    /// <summary>Initializes the sample stream.</summary>
    /// <param name="source">The source sample stream.</param>
    void Initialize(ISampleStream source);
  }

  /// <summary>Represents a device or plugin that can change the modulation type.</summary>
  public interface IModeSwitch
  {
    /// <summary>Gets or sets the signal mode.</summary>
    /// <value>The mode.</value>
    string Mode { get; set; }
    /// <summary>Gets or sets the signal sideband.</summary>
    /// <value>The sideband.</value>
    Sideband Sideband { get; set; }
    /// <summary>Occurs when the signal mode or sideband changes.</summary>
    event EventHandler ModeChanged;
  }

  /// <summary>Represents a plugin that processes I/Q data.</summary>
  public interface IIqProcessor : IInitSampleStream { }

  /// <summary>Represents a plugin that demodulates I/Q data to audio signals.</summary>
  public interface IDemodulator : IInitSampleStream, IModeSwitch { }

  /// <summary>Represents a plugin that processes audio signals.</summary>
  public interface IAudioProcessor : IInitSampleStream { }

  /// <summary>Represents a device that outputs audio data.</summary>
  public interface IAudioOutput
  {
    /// <summary>Initializes the specified audio output device.</summary>
    /// <param name="audioSignal">The audio signal that will be sent to the device..</param>
    void Initialize(ISampleStream audioSignal);
    /// <summary>Gets or sets a value indicating whether this <see cref="IAudioOutput" /> is active.</summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    bool Active { get; set; }
    /// <summary>Gets or sets a value indicating whether this <see cref="IAudioOutput" /> is muted.</summary>
    /// <value><c>true</c> if muted; otherwise, <c>false</c>.</value>
    bool Mute { get; set; }
  }

  /// <summary>Represents a plugin that performs bandpass filtering of the audio signals.</summary>
  public interface IBandpassFilter
  {
    /// <summary>Sets the passband of the filter.</summary>
    /// <param name="passbandLow">The lower cutoff frequency of the filter passband in Hertz.</param>
    /// <param name="passbandHigh">The upper cutoff frequency of the filter passband in Hertz/</param>
    void SetPassband(int passbandLow, int passbandHigh);
    /// <summary>Sets the filter bandwidth.</summary>
    /// <param name="bandwidth">The bandwidth of the filter in Hertz.</param>
    void SetBandwidth(int bandwidth);
    /// <summary>Occurs when the filter passband changes.</summary>
    event EventHandler<EventArgs> PassbandChanged;
  }

  /// <summary>Represents a transmitting device.</summary>
  public interface ITransmitter : IModeSwitch
  {
    /// <summary>Gets or sets the transmitter frequency.</summary>
    /// <value>The transmitter frequency in Hertz.</value>
    Int64 Frequency { get; set; }
    /// <summary>Gets or sets a value indicating whether this <see cref="ITransmitter" /> is in the Split mode.</summary>
    /// <value>
    ///   <c>true</c> if the transmitter is in the Split mode; otherwise, <c>false</c>.</value>
    bool Split { get; set; }
    /// <summary>Gets or sets a value indicating whether this <see cref="ITransmitter" /> is in the transmit mode.</summary>
    /// <value>
    ///   <c>true</c> if the transmitter in the transmit mode; otherwise, <c>false</c>.</value>
    bool Transmit { get; set; }
    /// <summary>Determines whether the transmitter is a transceiver.</summary>
    /// <returns>
    ///   <c>true</c> if this transmitter is a transceiver; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This function must return <c>true</c> if the same radio is currently used for receiving and transmitting,
    /// and its transmit frequency follows receiver's dial frequency in the simplex mode; otherwise <c>false</c>.
    /// </remarks>
    bool IsTransceiver();
    /// <summary>Gets or sets a value indicating whether this <see cref="ITransmitter" /> is active.</summary>
    /// <value>
    ///   <c>true</c> if active; otherwise, <c>false</c>.</value>
    bool Active { get; set; }
    /// <summary>Occurs when the transmit frequency changes.</summary>
    event EventHandler Tuned;
    /// <summary>Occurs when the settings of the transmitter change.</summary>
    event EventHandler SettingsChanged;
    /// <summary>Occurs when the transmitter stops working.</summary>
    /// <remarks>
    /// This event should be fired when the radio stops working for one reason or another, 
    /// e.g., it is turned off, its cable is disconnected, etc. The application uses
    /// this event to stop the <see cref="IPluginHost.DspPipeline"/> and display an error message.
    /// </remarks>
    event EventHandler<StoppedEventArgs> Stopped;
  }




  //------------------------------------------------------------------------------------------------
  //                                    band plan
  //------------------------------------------------------------------------------------------------
  /// <summary>
  /// Direction of the frequency change.
  /// </summary>
  /// <remarks>
  /// Used in the frequency navigation commands to request a band change or a switch 
  /// to the next or previous band segment
  /// </remarks>
  public enum QsyDirection {
    /// <summary>Previous band or segment.</summary>
    Down,
    /// <summary>Current band or segment.</summary>
    Here,
    /// <summary>Next band or segment.</summary>
    Up
  }

  /// <summary>Represents a plugin that provides the band plan data.</summary>
  public interface IBandPlan
  {
    /// <summary>Gets the current or next/previous band segment given the frequency.</summary>
    /// <param name="frequency">The frequency in Hertz.</param>
    /// <param name="qsyDirection">The QSY direction.</param>
    /// <returns>The band segment.</returns>
    /// <remarks>
    /// This method may be used, in particular, to find out the band and mode of a cluster spot, 
    /// or to jump to the next or previous band segment.
    /// </remarks>
    IBandSegment GetSegment(Int64 frequency, QsyDirection qsyDirection);

    //e.g., 
    /// <summary>Gets all band segments in the given range of frequencies.</summary>
    /// <param name="startFrequency">The start frequency in Hertz.</param>
    /// <param name="endFrequency">The end frequency in Hertz.</param>
    /// <returns>The band segments.</returns>
    /// <remarks>
    /// This method may be used, in particular, to draw segments on a band map.
    /// </remarks>
    IBandSegment[] GetSegments(Int64 startFrequency, Int64 endFrequency);

    /// <summary>Gets the start frequency for the given mode on the current, next-upper or next-lower band.</summary>
    /// <param name="frequency">The frequency in Hertz.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="qsyDirection">The QSY direction.</param>
    /// <returns>The start frequency in Hertz.</returns>
    /// <remarks>
    /// This method may be used, in particular, for band switching.
    /// </remarks>
    Int64 GetBandStart(Int64 frequency, string mode, QsyDirection qsyDirection);

    /// <summary>Occurs when the user selects a different band plan.</summary>
    event EventHandler BandPlanChanged;
  }

  /// <summary>Represents a segment in the band plan</summary>
  /// <seealso cref="IBandPlan" />
  public interface IBandSegment
  {
    /// <summary>Gets the band.</summary>
    /// <value>  
    /// The band name, preferably as defined in the 
    /// <a href="http://www.adif.org">ADIF</a> format.
    /// </value>
    string Band { get; }
    /// <summary>Gets the start frequency.</summary>
    /// <value>The start frequency in Hertz.</value>
    Int64 StartFrequency { get; }
    /// <summary>Gets the end frequency.</summary>
    /// <value>The end frequency in Hertz.</value>
    Int64 EndFrequency { get; }
    /// <summary>Gets the default frequency.</summary>
    /// <value>The default frequency in Hertz.</value>
    /// <remarks>When changing the bands, the radio will be initially tuned 
    /// to this frequency. If not specified, defaults to <see cref="StartFrequency"/>.</remarks>
    Int64 DefaultFrequency { get; }
    /// <summary>Gets the primary mode.</summary>
    /// <value>The primary mode in the band segment.</value>
    /// <remarks>This property may be used, in particular, to determine the mode
    /// of a cluster spot from its frequency.
    /// </remarks>
    string PrimaryMode { get; }
    /// <summary>Gets the valid modes.</summary>
    /// <value>The modes that are valid in the segment.</value>
    /// <remarks>This property may be used, in particular, 
    /// by the skimmer plugins to determine where to decode a particular mode.
    /// If not specified, assumed to include only the primary mode.
    /// </remarks>
      string[] ValidModes { get; }
    /// <summary>Gets a value indicating whether the segment is the main segment.</summary>
    /// <value><c>true</c> if main segment; otherwise, <c>false</c>.</value>
    /// <remarks>On some bands, there is more than one segment used for a particular mode. 
    /// For example, the 14074-14078 kHz and 14095-14099 kHz segments are used for FT8 
    /// on the 20m band.One of these segments must be marked as main.
    /// </remarks>
    bool MainSegment { get; }
    /// <summary>Gets the remark text.</summary>
    /// <value>The remark text.</value>
    /// <remarks>Remark is an optional string that will be appended to the segment label 
    /// when information about the segment is displayed. For example,
    ///"F/H" is a remark in the  14095-14099 kHz segment because this segment is used 
    ///for the Fox-and-Hound style QSO. The mouse tooltip on the band map is a combination 
    ///of the band, mode and remark: "20M FT8 (F/H)".
    ///</remarks>
    string Remark { get; }
    /// <summary>Gets the color of the segment background.</summary>
    /// <value>The color of the segment background.</value>
    Color BackColor { get; }
  }




  //------------------------------------------------------------------------------------------------
  //                             interfaces implemented by the host
  //------------------------------------------------------------------------------------------------
  /// <summary>Represents the application that hosts the plugins.</summary>
  public interface IPluginHost
  {
    /// <summary>Gets the DSP pipeline.</summary>
    /// <value>The DSP pipeline.</value>
    IDspPipeline DspPipeline { get; }
    /// <summary>Gets the color of the background.</summary>
    /// <value>The color of the background.</value>
    Color BackColor { get; }
    /// <summary>Gets the user data folder.</summary>
    /// <param name="author">The author ID.</param>
    /// <returns>The folder where the program stores user-specific data.</returns>
    string GetUserDataFolder(string author); //files created by the user 
    /// <summary>Gets the reference data folder.</summary>
    /// <param name="author">The author ID.</param>
    /// <returns>The folder where the program stores reference data.</returns>
    string GetReferenceDataFolder(string author); //data that comes with the app
  }

  /// <summary>Represents the <see cref="IPluginHost.DspPipeline"/> in the host application.</summary>
  public interface IDspPipeline
  {
    /// <summary>Gets a value indicating whether this <see cref="IDspPipeline" /> is active.</summary>
    /// <value>
    ///   <c>true</c> if active; otherwise, <c>false</c>.</value>
    bool Active { get; }
    /// <summary>Occurs when the <see cref="Active"/> status changes.</summary>
    event EventHandler StatusChanged;

    /// <summary>Gets the RX frequency tuner.</summary>
    /// <value>The RX frequency tuner.</value>
    ITuner Tuner { get; }
    /// <summary>Gets the mode switch.</summary>
    /// <value>The mode switch.</value>
    IModeSwitch ModeSwitch { get; }
    /// <summary>Gets the input I/Q or audio stream.</summary>
    /// <value>The input I/Q or audio stream.</value>
    ISampleStreamTap InputSignal { get; }
    /// <summary>Gets the processed I/Q signal stream.</summary>
    /// <value>The processed I/Q signal stream.</value>
    ISampleStreamTap ProcessedSignal { get; }
    /// <summary>Gets the demodulated audio stream.</summary>
    /// <value>The demodulated audio stream.</value>
    ISampleStreamTap DemodulatedAudio { get; }
    /// <summary>Gets the processed audio stream.</summary>
    /// <value>The processed audio stream.</value>
    ISampleStreamTap ProcessedAudio { get; }
    /// <summary>Gets the transmitter.</summary>
    /// <value>The transmitter.</value>
    ITransmitter Transmitter { get; }
  }

  /// <summary>Represents a tap into the <see cref="IPluginHost.DspPipeline"/>.</summary>
  public interface ISampleStreamTap : ISampleStream
  {
    /// <summary>Gets a value indicating whether the signal at this tap is available.</summary>
    /// <value>
    ///   <c>true</c> if the signal is available; otherwise, <c>false</c>.</value>
    bool IsAvailable { get; }
  }
}
