using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 재사용 "이름 입력/변경" 다이얼로그 VM — 레거시 FrmBibleRename·FrmUpdateFileName(이름 변경)의 공통 핵심.
/// 규칙: 빈 이름 거부 / 기존 이름과 중복 거부(대소문자 무시, 자기 자신은 제외) / 변경 없음도 허용(그대로 닫기).
/// View(NameEntryWindow)는 Title·Prompt 를 보여 주고 Name 을 편집하며, CanConfirm 으로 확인 버튼을 켠다.
/// 예배 순서 이름 변경뿐 아니라 폴더·성경 버전 이름 변경에도 그대로 재사용할 수 있다.
/// </summary>
public sealed partial class NameEntryViewModel : ObservableObject
{
    private readonly string _originalName;

    // 중복 검사 대상(자기 자신 제외, 대소문자 무시).
    private readonly HashSet<string> _otherNames;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Trimmed))]
    [NotifyPropertyChangedFor(nameof(Result))]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    private string _name;

    public NameEntryViewModel(string title, string prompt, string originalName, IEnumerable<string> existingNames)
    {
        Title = title ?? "";
        Prompt = prompt ?? "";
        _originalName = (originalName ?? "").Trim();
        _name = _originalName;
        _otherNames = (existingNames ?? Array.Empty<string>())
            .Select(n => (n ?? "").Trim())
            .Where(n => n.Length > 0 && !string.Equals(n, _originalName, StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>다이얼로그 제목(예: "예배 순서 이름 변경").</summary>
    public string Title { get; }

    /// <summary>입력칸 위 안내 문구(예: "'주일오전'의 새 이름:").</summary>
    public string Prompt { get; }

    /// <summary>앞뒤 공백을 제거한 현재 이름.</summary>
    public string Trimmed => (Name ?? "").Trim();

    /// <summary>확인 시 호출자가 받을 최종 이름(공백 제거).</summary>
    public string Result => Trimmed;

    /// <summary>입력이 잘못됐을 때의 안내(문제 없으면 빈 문자열).</summary>
    public string ErrorMessage
        => Trimmed.Length == 0 ? "이름을 입력하세요."
            : _otherNames.Contains(Trimmed) ? "이미 있는 이름입니다. 다른 이름을 입력하세요."
            : "";

    public bool HasError => ErrorMessage.Length > 0;

    /// <summary>확인(저장) 가능 여부 — 빈 이름·중복이 아니면 true. 변경 없음(자기 이름 그대로)도 허용.</summary>
    public bool CanConfirm => !HasError;
}
