using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Axicultus.Wpf.Captions;

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

    [RelayCommand]
    private void Show() => Session.SetVisible(true);
}
