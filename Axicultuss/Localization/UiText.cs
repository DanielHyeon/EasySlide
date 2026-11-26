using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Axicultus.Localization;

/// <summary>
/// The WinForms app's UI texts: Localization/Text/&lt;Area&gt;.resx (English) and &lt;Area&gt;.ko.resx (Korean), one pair
/// per form (the area is the form's class name) or helper. Code: <c>UiText.Get("FrmOptions", "SaveFailed")</c>.
/// A form's designer texts are put in the UI language by <see cref="ApplyTo"/> right after InitializeComponent; the
/// designer keeps its English texts for design time. The main menu has its own shared MenuText.
/// </summary>
public static class UiText
{
    public static readonly UiTextCatalog Catalog = new(typeof(UiText).Assembly, "Axicultus.Localization.Text");

    public static string Get(string area, string key) => Catalog.Get(area, key);

    public static string Format(string area, string key, params object[] args) => Catalog.Format(area, key, args);

    /// <summary>
    /// Sets the form's (or user control's) designer texts from its area's resources, by the designer's field names:
    /// "$this.Text" (the title), "&lt;field&gt;.Text", "&lt;field&gt;.ToolTipText", "&lt;field&gt;.HeaderText" and
    /// "&lt;field&gt;.ToolTip" (the text a ToolTip component shows for that control). A field without an entry keeps
    /// its designer text. Nested user controls localize themselves.
    /// </summary>
    public static void ApplyTo(Control root)
    {
        string area = root.GetType().Name;
        if (Catalog.Find(area, "$this.Text") is { } title) root.Text = title;
        var tips = new List<ToolTip>();
        var controls = new List<(string Name, Control Control)>();
        for (var type = root.GetType(); type != null && type != typeof(Form) && type != typeof(UserControl); type = type.BaseType)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                object value = field.GetValue(root);
                string name = field.Name;
                string Text(string property) => Catalog.Find(area, name + "." + property);
                switch (value)
                {
                    case ToolTip tip:
                        tips.Add(tip);
                        break;
                    case ToolStripItem item:
                        if (Text("Text") is { } itemText) item.Text = itemText;
                        if (Text("ToolTipText") is { } itemTip) item.ToolTipText = itemTip;
                        break;
                    case ColumnHeader column:
                        if (Text("Text") is { } columnText) column.Text = columnText;
                        break;
                    case DataGridViewColumn gridColumn:
                        if (Text("HeaderText") is { } header) gridColumn.HeaderText = header;
                        if (Text("ToolTipText") is { } gridTip) gridColumn.ToolTipText = gridTip;
                        break;
                    case Control control when !ReferenceEquals(control, root):
                        if (Text("Text") is { } controlText) control.Text = controlText;
                        controls.Add((name, control));
                        break;
                }
            }
        }
        foreach (var (name, control) in controls)
        {
            if (Catalog.Find(area, name + ".ToolTip") is not { } text) continue;
            // The ToolTip the designer gave this control (the first one when it had none).
            var tip = tips.Find(t => t.GetToolTip(control).Length > 0) ?? (tips.Count > 0 ? tips[0] : null);
            tip?.SetToolTip(control, text);
        }
    }
}
