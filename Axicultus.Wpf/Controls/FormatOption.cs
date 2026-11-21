using System.Windows;
using System.Windows.Controls;

namespace Axicultus.Wpf.Controls;

/// <summary>A format choice with the same icon in its open menu and closed toolbar button.</summary>
public sealed class FormatOption : ComboBoxItem
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), typeof(FormatOption));
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        // A stored Fill or FollowRegion1 can look like an explicit Fit or alignment choice.
        // Committing the displayed choice must persist it even when SelectedIndex stays the same.
        if (IsSelected && ItemsControl.ItemsControlFromItemContainer(this) is ComboBox owner)
            owner.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateSource();
        base.OnMouseLeftButtonUp(e);
    }
}
