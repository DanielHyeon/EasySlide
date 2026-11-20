using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Easislides.Wpf.Library;

public sealed class FolderLayoutSample : FrameworkElement
{
    public FolderLayoutSample()
    {
        DataContextChanged += (_, args) =>
        {
            Listen(args.OldValue as FolderEditorViewModel, false);
            Listen(args.NewValue as FolderEditorViewModel, true);
            InvalidateVisual();
        };
    }

    private void Listen(FolderEditorViewModel? vm, bool attach)
    {
        if (vm == null) return;
        foreach (var field in vm.FormatFields)
        {
            if (attach) PropertyChangedEventManager.AddHandler(field, FieldChanged, nameof(field.Value));
            else PropertyChangedEventManager.RemoveHandler(field, FieldChanged, nameof(field.Value));
        }
    }

    private void FieldChanged(object? sender, PropertyChangedEventArgs args) => InvalidateVisual();

    protected override void OnRender(DrawingContext dc)
    {
        if (DataContext is not FolderEditorViewModel vm || ActualWidth < 1 || ActualHeight < 1) return;
        double Number(string column, double fallback) => double.TryParse(vm.FormatFields.FirstOrDefault(f => f.Field.Column == column)?.Value,
            CultureInfo.InvariantCulture, out var result) && double.IsFinite(result) ? Math.Clamp(result, 0, 100) : fallback;
        double left = ActualWidth * Number("LMargin", 5) / 100;
        double width = Math.Max(0, ActualWidth - left - ActualWidth * Number("RMargin", 5) / 100);
        double top = ActualHeight * Number("Vpos0", 10) / 100;
        double bottom = Math.Max(top, ActualHeight * (1 - Number("BMargin", 5) / 100));
        double split = Math.Clamp(ActualHeight * Number("Vpos1", 65) / 100, top, bottom);
        dc.DrawRectangle(Brushes.DimGray, null, new Rect(RenderSize));
        DrawBand(top, split, Brushes.SteelBlue, "영역 1");
        DrawBand(split, bottom, Brushes.DarkSlateBlue, "영역 2");
        void DrawBand(double y, double end, Brush brush, string label)
        {
            if (end <= y || width < 1) return;
            var rect = new Rect(left, y, width, end - y);
            dc.DrawRectangle(brush, new Pen(Brushes.White, 1), rect);
            dc.PushClip(new RectangleGeometry(rect));
            var text = new FormattedText(label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(SystemFonts.MessageFontFamily.Source), 12, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(text, new Point(left + 4, y + 2));
            dc.Pop();
        }
    }
}
