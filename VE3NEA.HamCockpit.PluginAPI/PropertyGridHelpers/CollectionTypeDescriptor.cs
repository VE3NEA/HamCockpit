using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.PluginHelpers
{
  //https://stackoverflow.com/questions/1928567/using-a-dictionary-in-a-propertygrid
  //https://www.codeproject.com/Articles/4448/Customized-display-of-collection-data-in-a-Propert

  //this class allows collections to be expandable in the PropertyGrid UI
  //with customized display names and values for the items

  //intended use:
  //[TypeConverter(typeof(ExpandableObjectConverter))]
  //public CollectionTypeDescriptor<ModeColor, CollectionItemPropertyDescriptor<ModeColor>> ModeColors { get; set; }
  //  = new CollectionTypeDescriptor<ModeColor, CollectionItemPropertyDescriptor<ModeColor>>(new List<ModeColor>());

  /// <exclude />
  public class CollectionTypeDescriptor<TItem, TDescriptor> : ICustomTypeDescriptor
    where TDescriptor : CollectionItemPropertyDescriptor<TItem>, new()
  {
    /// <exclude />
    public IList<TItem> collection;

    /// <exclude />
    public CollectionTypeDescriptor(IList<TItem> collection)
    {
      this.collection = collection;
    }

    /// <exclude />
    public PropertyDescriptorCollection GetProperties()
    {
      var properties = new PropertyDescriptorCollection(null);

      for (int i = 0; i < collection.Count; i++)
      {
        var prop = new TDescriptor();
        prop.collection = collection;
        prop.index = i;
        properties.Add(prop);
      }
      return properties;
    }

    #region trivial part
    /// <exclude />
    public String GetClassName()
    {
      return TypeDescriptor.GetClassName(this, true);
    }

    /// <exclude />
    public AttributeCollection GetAttributes()
    {
      return TypeDescriptor.GetAttributes(this, true);
    }

    /// <exclude />
    public String GetComponentName()
    {
      return TypeDescriptor.GetComponentName(this, true);
    }

    /// <exclude />
    public TypeConverter GetConverter()
    {
      return TypeDescriptor.GetConverter(this, true);
    }

    /// <exclude />
    public EventDescriptor GetDefaultEvent()
    {
      return TypeDescriptor.GetDefaultEvent(this, true);
    }

    /// <exclude />
    public PropertyDescriptor GetDefaultProperty()
    {
      return TypeDescriptor.GetDefaultProperty(this, true);
    }

    /// <exclude />
    public object GetEditor(Type editorBaseType)
    {
      return TypeDescriptor.GetEditor(this, editorBaseType, true);
    }

    /// <exclude />
    public EventDescriptorCollection GetEvents(Attribute[] attributes)
    {
      return TypeDescriptor.GetEvents(this, attributes, true);
    }

    /// <exclude />
    public EventDescriptorCollection GetEvents()
    {
      return TypeDescriptor.GetEvents(this, true);
    }

    /// <exclude />
    public object GetPropertyOwner(PropertyDescriptor pd)
    {
      return this;
    }

    /// <exclude />
    public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
      return GetProperties();
    }
    #endregion
  }


  /// <exclude />
  public class CollectionItemPropertyDescriptor<TItem> : PropertyDescriptor
  {
    internal IList<TItem> collection;
    internal int index;

    /// <exclude />
    public CollectionItemPropertyDescriptor() : base("", null) { }

    /// <exclude />
    public override string Name { get => GetName(); }

    /// <exclude />
    public override string DisplayName { get => GetName(); }

    /// <exclude />
    protected virtual string GetName()
    {
      return collection[index].ToString();
    }

    /// <exclude />
    public override void SetValue(object component, object value)
    { 
      collection[index] = (TItem)value; 
    }

    /// <exclude />
    public override object GetValue(object component) { return collection[index]; }

    #region trivial part
    /// <exclude />
    public override Type PropertyType { get => collection[index].GetType(); }

    /// <exclude />
    public override bool IsReadOnly { get => false; }

    /// <exclude />
    public override Type ComponentType { get => null; }

    /// <exclude />
    public override bool CanResetValue(object component) { return false; }

    /// <exclude />
    public override void ResetValue(object component) { }

    /// <exclude />
    public override bool ShouldSerializeValue(object component) { return false; }
    #endregion
  }
}
