using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 세션 메모 편집 뷰모델(레거시 FrmMain Session Notes) — 현재 예배 세션(예배 순서 이름)의 메모를
/// 불러와 편집하고 저장한다. 저장/불러오기는 IWorshipSessionNotes 에 위임한다.
/// </summary>
public sealed partial class WorshipSessionNotesViewModel : ObservableObject
{
    private readonly IWorshipSessionNotes _notes;
    private readonly string _sessionKey;

    [ObservableProperty]
    private string _notesText = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public WorshipSessionNotesViewModel(IWorshipSessionNotes notes, string sessionKey)
    {
        _notes = notes ?? throw new ArgumentNullException(nameof(notes));
        _sessionKey = string.IsNullOrWhiteSpace(sessionKey) ? "일반" : sessionKey.Trim();

        NotesText = _notes.GetNotes(_sessionKey);
        SaveCommand = new RelayCommand(Save);
    }

    /// <summary>메모가 속한 세션 이름(창 제목·안내에 표시).</summary>
    public string SessionKey => _sessionKey;

    public IRelayCommand SaveCommand { get; }

    private void Save()
    {
        var ok = _notes.SetNotes(_sessionKey, NotesText ?? string.Empty);
        StatusText = !ok
            ? "메모 저장에 실패했습니다(디스크 권한·공간을 확인하세요)."
            : string.IsNullOrWhiteSpace(NotesText)
                ? "메모를 비웠습니다."
                : "메모를 저장했습니다.";
    }
}
