using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VE3NEA.HamCockpit.PluginHelpers
{
  //https://dotnetfacts.blogspot.com/2008/05/how-to-take-control-over-collection.html
  //https://engineeringdotnet.blogspot.com/2010/08/modifying-c-collectioneditor-for-real.html
  //https://www.codeproject.com/Articles/718006/Enhanced-CollectionEditor-Framework

  /// <summary>
  /// Provides a collection editor that can edit most types of collections 
  /// in the Plugin Settings dialog.
  /// </summary>
  /// <remarks>
  /// If a property of collection type in the Settings object of a plugin needs to be edited
  /// in the Plugin Settings dialog, use this class as an Editor attribute for that property.
  /// </remarks>
  /// <example><code>
  /// [Editor(typeof(CollectionEditorWithEvent), typeof(UITypeEditor))]
  /// public ModeColorList ModeColors { get; set; } = new ModeColorList();
  /// </code></example>
  public class CollectionEditorWithEvent : CollectionEditor
  {
    private bool changed = false;

    /// <exclude />
    public CollectionEditorWithEvent(Type type) : base(type) { }

    /// <exclude />
    public static event EventHandler CollectionChanged;

    private void PropertyValueChangedHandler(object sender, PropertyValueChangedEventArgs e)
    {
      changed = true;
    }

    private void AddRemoveButtonClickedHandler(object sender, EventArgs e)
    {
      changed = true;
    }

    private void FormClosingHandler(object sender, FormClosingEventArgs e)
    {
      if (changed) CollectionChanged?.Invoke(this, new EventArgs());
    }

    /// <exclude />
    protected override CollectionForm CreateCollectionForm()
    {
      var form = base.CreateCollectionForm();

      try
      {
        var panel1 = form.Controls["overArchingTableLayoutPanel"];
        var panel2 = panel1.Controls["addRemoveTableLayoutPanel"];
        var grid = panel1.Controls["propertyBrowser"] as PropertyGrid;
        var addBtn = panel2.Controls["addButton"] as Button;
        var removeBtn = panel2.Controls["removeButton"] as Button;

        form.FormClosing += FormClosingHandler;
        grid.PropertyValueChanged += PropertyValueChangedHandler;
        addBtn.Click += AddRemoveButtonClickedHandler;
        removeBtn.Click += AddRemoveButtonClickedHandler;

        //my personal preference
        grid.PropertySort = PropertySort.NoSort;
      }
      catch { }

      return form;
    }
  }
}
