using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Input;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 명령 팔레트(⌘K, §7.4) — FrmMain 의 136개 메뉴를 검색 가능한 단일 진입점으로 흡수.
/// 명령 목록은 <see cref="ICommandCatalog"/> 에서, 실행은 주입된 invoke(명령 id → 성공 여부)로 위임한다
/// (MainViewModel 이 ShortcutRegistry.TryInvoke 를 연결). 키 입력 제스처는 View 가 처리하고
/// 검색 필터·선택 이동·실행 로직만 여기서 담당한다(테스트 가능 코어, 자동회전/드래그와 동일 철학).
/// </summary>
public sealed partial class CommandPaletteViewModel : ObservableObject
{
    private readonly ICommandCatalog _catalog;
    private readonly Func<string, bool> _invoke;

    [ObservableProperty] private bool _isOpen;
    [ObservableProperty] private string _query = string.Empty;
    [ObservableProperty] private CommandDescriptor? _selectedCommand;

    public CommandPaletteViewModel(ICommandCatalog catalog, Func<string, bool> invoke)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));
        OpenCommand = new RelayCommand(Open);
        CloseCommand = new RelayCommand(Close);
        ExecuteSelectedCommandCommand = new RelayCommand(ExecuteSelectedCommand);
        RebuildResults();
    }

    /// <summary>검색 결과(필터링된 명령 목록). 검색어가 비면 전체.</summary>
    public ObservableCollection<CommandDescriptor> Results { get; } = new();

    /// <summary>검색 결과가 하나도 없는지 — "결과 없음" 안내 표시에 바인딩. 검색어가 비면 전체가 보이므로(결과 있음) false.</summary>
    public bool HasNoResults => Results.Count == 0;

    public IRelayCommand OpenCommand { get; }

    public IRelayCommand CloseCommand { get; }

    public IRelayCommand ExecuteSelectedCommandCommand { get; }

    /// <summary>팔레트를 연다 — 검색어 초기화 후 전체 목록을 보이고 첫 항목을 선택.</summary>
    public void Open()
    {
        Query = string.Empty; // OnQueryChanged 가 결과를 재구성한다
        IsOpen = true;
    }

    public void Close() => IsOpen = false;

    /// <summary>현재 선택 명령을 실행하고 팔레트를 닫는다(선택이 없으면 아무 것도 안 함).</summary>
    public void ExecuteSelectedCommand()
    {
        if (SelectedCommand is not { } command)
        {
            return;
        }

        Close();
        _invoke(command.Id);
    }

    /// <summary>결과에서 다음 항목 선택(마지막이면 유지) — View 의 ↓ 키가 호출.</summary>
    public void SelectNext() => MoveSelection(+1);

    /// <summary>결과에서 이전 항목 선택(처음이면 유지) — View 의 ↑ 키가 호출.</summary>
    public void SelectPrevious() => MoveSelection(-1);

    partial void OnQueryChanged(string value) => RebuildResults();

    private void MoveSelection(int delta)
    {
        if (Results.Count == 0)
        {
            SelectedCommand = null;
            return;
        }

        var index = SelectedCommand is null ? -1 : Results.IndexOf(SelectedCommand);
        var next = Math.Clamp(index + delta, 0, Results.Count - 1);
        SelectedCommand = Results[next];
    }

    // 검색어로 명령을 필터링해 Results 를 갱신. 검색어가 비면 전체.
    // 매칭: DisplayName/Category/Description 부분일치(대소문자 무시). 선택은 결과 안에 있도록 유지(없으면 첫 항목).
    private void RebuildResults()
    {
        var matches = Filter(_catalog.All, Query);

        Results.Clear();
        foreach (var command in matches)
        {
            Results.Add(command);
        }

        // 기존 선택이 결과에 남아 있으면 유지, 아니면 첫 항목(또는 결과 없으면 null).
        if (SelectedCommand is null || !Results.Contains(SelectedCommand))
        {
            SelectedCommand = Results.Count > 0 ? Results[0] : null;
        }

        OnPropertyChanged(nameof(HasNoResults)); // 결과 개수가 바뀌면 "결과 없음" 안내 가시성 갱신.
    }

    private static IEnumerable<CommandDescriptor> Filter(IEnumerable<CommandDescriptor> commands, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return commands;
        }

        var term = query.Trim();

        bool Matches(CommandDescriptor c)
            => Contains(c.DisplayName, term) || Contains(c.Category, term) || Contains(c.Description, term);

        // DisplayName 이 검색어로 시작하는 명령을 먼저, 그다음 일반 일치 순으로 정렬(가장 관련 높은 것 위로).
        return commands
            .Where(Matches)
            .OrderByDescending(c => c.DisplayName.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static bool Contains(string? text, string term)
        => !string.IsNullOrEmpty(text) && text.Contains(term, StringComparison.OrdinalIgnoreCase);
}
