using System;
using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Axicultus.Wpf.Captions;

/// <summary>A step of the followed deck in the console's strip: a slide, or a score row of it (Row from 0, with its picture).</summary>
internal sealed record CaptionStep(int Slide, int Row, string Label, string Text, ImageSource? Thumbnail)
{
    /// <summary>What a screen reader reads for the step: its label and first line.</summary>
    public string Spoken => Text.Length > 0 ? $"{Label} · {Text}" : Label;
}

/// <summary>A caption as the camera screen would show it (the held caption drawn dim while hidden); the names are CaptionSession's.</summary>
internal sealed record CaptionLook(string Label, ImageSource? PublicImage, string PublicText, int CaptionLines, int ScoreSize,
    Brush CaptionBackground, Brush CaptionForeground)
{
    public bool HasPublicText => PublicText.Length > 0;
    public bool HasText => HasPublicText || PublicImage != null;
}

/// <summary>True when the two values are the same object (a step and the strip's next step).</summary>
public sealed class SameItemConverter : System.Windows.Data.IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => values is { Length: 2 } && values[0] != null && ReferenceEquals(values[0], values[1]);

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}

internal sealed partial class CaptionConsoleViewModel(CaptionSession session) : ObservableObject
{
    public CaptionSession Session { get; } = session;
    public event Action? SaveRequested;

    [ObservableProperty] private string _draftText = string.Empty;
    [ObservableProperty] private bool _isInstant;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TakeCommand))]
    private bool _isComposing;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TakeCommand))]
    private bool _isRestoring;

    /// <summary>The followed deck's steps (empty while no deck is followed) and the one on air.</summary>
    [ObservableProperty] private IReadOnlyList<CaptionStep> _steps = [];
    [ObservableProperty] private CaptionStep? _currentStep;


    /// <summary>The strip's step the next button would go to (marked in the strip); null when none.</summary>
    [ObservableProperty] private CaptionStep? _nextStep;

    /// <summary>While hidden, what showing the caption would put on air (drawn dim in the preview); null on air.</summary>
    [ObservableProperty] private CaptionLook? _held;

    partial void OnDraftTextChanged(string value)
    {
        if (IsRestoring) return;
        SaveRequested?.Invoke();
        PublishInstant();
    }

    internal void CompleteComposition()
    {
        if (!IsComposing) return;
        IsComposing = false;
        PublishInstant();
    }

    // An emptied box keeps the caption on air until the next words are typed; a hidden caption stays hidden.
    private void PublishInstant()
    {
        if (IsInstant && CanTake() && !string.IsNullOrWhiteSpace(DraftText)) Session.Publish(DraftText, show: false);
    }

    /// <summary>Sends the box and empties it for the next line; the caption stays on air.</summary>
    internal void TakeAndClear()
    {
        if (!TakeCommand.CanExecute(null)) return;
        Take();
        DraftText = string.Empty;
    }

    private bool CanTake() => !IsComposing && !IsRestoring;

    internal void TakeFromShortcut(bool isRepeat)
    {
        if (!isRepeat && TakeCommand.CanExecute(null)) TakeCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(CanTake))]
    private void Take()
    {
        Session.Publish(DraftText);
        Session.RememberTake();
        SaveRequested?.Invoke();
    }

    [RelayCommand]
    private void Hide() => Session.SetVisible(false); // typing while hidden no longer reveals, so instant mode stays on

    /// <summary>The big on-air button and Space: shows a hidden caption, hides one on air.</summary>
    [RelayCommand]
    private void ToggleVisible() => Session.SetVisible(!Session.Visible);
}
