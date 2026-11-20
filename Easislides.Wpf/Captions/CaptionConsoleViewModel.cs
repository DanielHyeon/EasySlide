using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Captions;

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

    private void PublishInstant()
    {
        if (IsInstant && CanTake()) Session.Publish(DraftText);
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
    private void Hide()
    {
        IsInstant = false;
        Session.SetVisible(false);
    }

    [RelayCommand]
    private void Show() => Session.SetVisible(true);
}
