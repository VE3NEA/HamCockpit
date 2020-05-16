using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.PluginHelpers
{
  internal struct SoundcardEntry
  {
    public string Id;
    public string Name;
    public SoundcardEntry(string id, string name) { Id = id; Name = name; }
  }

  /// <exclude />
  public abstract class SoundcardNameConverter : StringConverter
  {
    internal SoundcardEntry[] soundcards;

    /// <exclude />
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    /// <exclude />
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      ListDevices();
      return new StandardValuesCollection(soundcards.Select(s => s.Id).ToArray());
    }

    /// <exclude />
    protected abstract DataFlow Direction();

    /// <exclude />
    protected void ListDevices()
    {
      using (var deviceEnumerator = new MMDeviceEnumerator())
      using (var deviceCollection = deviceEnumerator.EnumAudioEndpoints(Direction(), DeviceState.Active))
        soundcards = deviceCollection.Select(s => new SoundcardEntry(s.DeviceID, s.FriendlyName)).ToArray();
    }

    /// <exclude />
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      if (soundcards == null) ListDevices();
      return soundcards.Where(s => s.Name == value as string)?.Select(s => s.Id)?.First();
    }

    /// <exclude />
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if (soundcards == null) ListDevices();

      try
      {
        return soundcards.Where(s => s.Id == value as string).Select(s => s.Name).First();
      }
      catch
      {
        return "<please select>";
      }
    }
  }

  /// <summary>
  /// Provides a type converter that allows editing the input soundcard property
  /// in the Plugin Settings dialog.
  /// </summary>
  /// <remarks>
  /// If an input soundcard selection property in the <c>Settings</c> object of a plugin needs to be edited
  /// in the Plugin Settings dialog, use this class as a <c>TypeConverter</c> attribute for that property.
  /// The class populates the dropdown list with the names of available input soundcards and assigns
  /// <c>DeviceID</c> of the selected soundcard to the property.
  /// </remarks>
  /// <example><code>
  ///[TypeConverter(typeof(InputSoundcardNameConverter))]
  ///public string Soundcard { get; set; }
  /// </code></example>
  public class InputSoundcardNameConverter : SoundcardNameConverter
  {
    /// <exclude />
    protected override DataFlow Direction() { return DataFlow.Capture; }
  }


  /// <summary>
  /// Provides a type converter that allows editing the output soundcard property
  /// in the Plugin Settings dialog.
  /// </summary>
  /// <remarks>
  /// If an output soundcard selection property in the <c>Settings</c> object of a plugin needs to be edited
  /// in the Plugin Settings dialog, use this class as a <c>TypeConverter</c> attribute for that property.
  /// The class populates the dropdown list with the names of available output soundcards and assigns
  /// <c>DeviceID</c> of the selected soundcard to the property.
  /// </remarks>
  /// <example><code>
  ///[TypeConverter(typeof(InputSoundcardNameConverter))]
  ///public string Soundcard { get; set; }
  /// </code></example>
  public class OutputSoundcardNameConverter : SoundcardNameConverter
  {
    /// <exclude />
    protected override DataFlow Direction() { return DataFlow.Render; }
  }
}
