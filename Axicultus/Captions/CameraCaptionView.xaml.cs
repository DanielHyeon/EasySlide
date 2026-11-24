using System;
using System.Windows;
using System.Windows.Controls;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The camera lower third as the output screen shows it: 88% of the screen's width, 5% above its foot, text 5% of its
/// height, a score row up to the chosen share of it. The caption console draws its previews with the same control.
/// </summary>
public partial class CameraCaptionView : Border
{
    private Size _screen;
    private double _font = 32;

    public CameraCaptionView() => InitializeComponent();

    /// <summary>Sizes the caption for the screen (the area of the output window it sits on).</summary>
    internal void Fit(Size screen)
    {
        _screen = screen;
        Width = screen.Width * .88;
        Margin = new Thickness(0, 0, 0, screen.Height * .05);
        _font = Math.Max(12, screen.Height * .05);
        LowerThirdArea.Padding = new Thickness(_font * .55, _font * .15, _font * .55, _font * .15);
        LowerThirdImage.Margin = new Thickness(0, 0, 0, _font * .3);
        Refit();
    }

    private void Refit(object? sender, System.Windows.Data.DataTransferEventArgs e) => Refit();

    private void Refit()
    {
        // Tags carry the caption's lines and score size.
        LowerThirdImage.MaxHeight = _screen.Height * CaptionStyle.ScoreHeight(LowerThirdImage.Tag is int score ? score : 1);
        CaptionFit.Apply(LowerThirdText, LowerThirdArea, Width - LowerThirdArea.Padding.Left - LowerThirdArea.Padding.Right, _font,
            LowerThirdText.Tag is int lines ? lines : 3);
    }
}
