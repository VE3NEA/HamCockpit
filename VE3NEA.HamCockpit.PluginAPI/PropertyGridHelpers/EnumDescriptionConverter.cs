//source: https://www.codeproject.com/Articles/22717/Using-PropertyGrid


using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;


namespace VE3NEA.HamCockpit.PluginHelpers
{
  /// <summary>
  /// Provides a type converter that allows editing properties of <c>enum</c> type
  /// in the Plugin Settings dialog.
  /// </summary>
  /// <remarks>
  /// If a property of <c>enum</c> type in the <c>Settings</c> object of a plugin needs to be edited
  /// in the Plugin Settings dialog, use this class as a <c>TypeConverter</c> attribute for that property.
  /// With this attribute, the editor shows the descriptions of <c>enum</c> values instead of the values themselves.
  /// </remarks>
  /// <example>
  /// Enum values with descriptions:
  /// <code>
  ///   enum InputType {
  ///    [Description("Left Channel")]
  ///    LeftChannel,
  ///
  ///    [Description("Right Channel")]
  ///    RightChannel,
  ///
  ///    [Description("Diversity (stereo)")]
  ///    DiversityStereo
  ///}
  /// </code></example>
  /// <example>
  /// TypeConverter attribute:
  /// <code>
  /// [TypeConverter(typeof(EnumDescriptionConverter))]
  /// public InputType InputType { get; set; }
  /// </code></example>
  public class EnumDescriptionConverter : EnumConverter
  {
    private Type _enumType;
    /// <exclude />
    public EnumDescriptionConverter(Type type) : base(type)
    {
      _enumType = type;
    }

    /// <exclude />
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
    {
      return destType == typeof(string);
    }

    /// <exclude />
    public override object ConvertTo(ITypeDescriptorContext context,
        CultureInfo culture, object value, Type destType)
    {
      FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
      DescriptionAttribute dna =
          (DescriptionAttribute)Attribute.GetCustomAttribute(
          fi, typeof(DescriptionAttribute));

      if (dna != null)
        return dna.Description;
      else
        return value.ToString();
    }

    /// <exclude />
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
    {
      return srcType == typeof(string);
    }

    /// <exclude />
    public override object ConvertFrom(ITypeDescriptorContext context,
        CultureInfo culture, object value)
    {
      foreach (FieldInfo fi in _enumType.GetFields())
      {
        DescriptionAttribute dna =
          (DescriptionAttribute)Attribute.GetCustomAttribute(
          fi, typeof(DescriptionAttribute));

        if ((dna != null) && ((string)value == dna.Description))
          return Enum.Parse(_enumType, fi.Name);
      }
      return Enum.Parse(_enumType, (string)value);
    }
  }
}
