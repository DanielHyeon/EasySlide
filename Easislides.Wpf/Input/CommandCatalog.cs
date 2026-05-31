using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Input;

public sealed record CommandDescriptor(
    string Id,
    string Category,
    string DisplayName,
    string Description,
    bool IsDangerous,
    IReadOnlyList<Shortcut> DefaultShortcuts)
{
    /// <summary>
    /// 명령 팔레트(⌘K) 결과를 스크린리더가 한 호흡에 읽도록 합성한 항목 레벨 접근성 이름.
    /// 자식 TextBlock 들의 합성(비결정적)에 기대지 않고 "이름, [위험 명령,] 분류" 순서로 또렷이 읽어 준다.
    /// 위험 명령은 색 배지뿐 아니라 음성으로도 "위험"을 전달(색에 의존하지 않는 접근성).
    /// 순서는 이름 먼저 — 목록을 훑을 때 명령명이 1차 식별자라 앞에 두고, 위험·분류는 뒤따른다.
    /// </summary>
    public string AccessibleName => IsDangerous
        ? $"{DisplayName}, 위험 명령, {Category}"
        : $"{DisplayName}, {Category}";
}

public interface ICommandCatalog
{
    IReadOnlyList<CommandDescriptor> All { get; }

    CommandDescriptor? FindById(string commandId);
    IReadOnlyList<Shortcut> GetDefaultShortcuts();
}

public sealed class CommandCatalog : ICommandCatalog
{
    private static readonly IReadOnlyList<CommandDescriptor> DefaultCommands = new[]
    {
        Command(
            MainCommandIds.OutputOpen,
            "Output",
            "출력 창 열기",
            "선택한 모니터에 WPF 출력 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.OutputClose,
            "Output",
            "출력 창 닫기",
            "출력 창을 닫습니다. 라이브 중이면 확인이 필요합니다.",
            isDangerous: true),
        Command(
            MainCommandIds.LiveGo,
            "Live",
            "Go Live",
            "선택한 항목을 라이브 출력으로 전환합니다.",
            isDangerous: true,
            new Shortcut(Key.L, ModifierKeys.Control, MainCommandIds.LiveGo, IsGlobal: false, "Go Live")),
        Command(
            MainCommandIds.LiveStop,
            "Live",
            "라이브 중지",
            "현재 라이브 송출을 중지합니다.",
            isDangerous: true),
        Command(
            MainCommandIds.LiveNext,
            "Live",
            "다음 항목",
            "다음 큐 항목으로 이동합니다.",
            isDangerous: false,
            new Shortcut(Key.Space, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: false, "다음 항목"),
            new Shortcut(Key.F5, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: true, "다음 항목")),
        Command(
            MainCommandIds.LivePrevious,
            "Live",
            "이전 항목",
            "이전 큐 항목으로 이동합니다.",
            isDangerous: false,
            new Shortcut(Key.Space, ModifierKeys.Shift, MainCommandIds.LivePrevious, IsGlobal: false, "이전 항목"),
            new Shortcut(Key.F4, ModifierKeys.None, MainCommandIds.LivePrevious, IsGlobal: true, "이전 항목")),
        Command(
            MainCommandIds.LiveHide,
            "Live",
            "출력 숨김",
            "현재 라이브 출력을 숨김 상태로 전환합니다.",
            isDangerous: true,
            new Shortcut(Key.H, ModifierKeys.Control, MainCommandIds.LiveHide, IsGlobal: false, "출력 숨김")),
        Command(
            MainCommandIds.LiveBlack,
            "Live",
            "검은 화면",
            "현재 라이브 출력을 검은 화면으로 전환합니다.",
            isDangerous: true,
            new Shortcut(Key.B, ModifierKeys.Control, MainCommandIds.LiveBlack, IsGlobal: false, "검은 화면")),
        // 화면 제어 보강(§7.3-B) — 명령 팔레트(⌘K)에서 검색·실행. 단축키는 미배정(필요 시 설정에서 부여).
        Command(
            MainCommandIds.LiveClear,
            "Live",
            "화면 비우기",
            "가사/콘텐츠를 비우고 배경만 송출합니다(검은 화면과 구별).",
            isDangerous: true),
        Command(
            MainCommandIds.LiveRestart,
            "Live",
            "처음으로",
            "현재 라이브 항목을 처음(첫 절/첫 슬라이드)으로 되돌려 재송출합니다.",
            isDangerous: false),
        Command(
            MainCommandIds.LiveRefresh,
            "Live",
            "출력 새로고침",
            "출력 화면을 다시 그립니다(디스플레이 글리치 복구).",
            isDangerous: false),
        Command(
            MainCommandIds.LiveRestore,
            "Live",
            "출력 복귀",
            "숨김/검은 화면/비우기에서 직전 송출로 복귀합니다.",
            isDangerous: false),
        Command(
            MainCommandIds.LiveAutoRotate,
            "Live",
            "자동 회전 토글",
            "라이브 중 절/슬라이드를 일정 간격으로 자동 전환합니다(§7.3-B).",
            isDangerous: false),
        // 창 런처(§7.4) — FrmMain 의 분리 창을 명령 팔레트로 흡수. 실행은 View(MainWindow)가 등록.
        Command(
            MainCommandIds.WindowLibrary,
            "창",
            "라이브러리 열기",
            "곡 라이브러리 창을 엽니다(곡 검색·복사·이동·삭제).",
            isDangerous: false),
        Command(
            MainCommandIds.WindowBible,
            "창",
            "성경 열기",
            "성경 창을 엽니다(버전·구절 검색).",
            isDangerous: false),
        Command(
            MainCommandIds.WindowManageBibleVersions,
            "창",
            "성경 버전 관리",
            "성경 버전 이름을 바꿉니다(본문은 그대로).",
            isDangerous: false),
        Command(
            MainCommandIds.WindowSearch,
            "창",
            "검색·사용처",
            "곡 검색 및 사용처 보고 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.WindowImportExport,
            "창",
            "가져오기·내보내기",
            "Import/Export 창을 엽니다(텍스트/XML/DB/RTF/HTML).",
            isDangerous: false),
        Command(
            MainCommandIds.WindowExternalFiles,
            "창",
            "외부 파일 복사·이동",
            "외부 파일 복사/이동 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.WindowManageLists,
            "창",
            "예배 순서 관리",
            "예배 순서를 저장/불러오는 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.WindowSettings,
            "창",
            "설정 열기",
            "설정 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.WindowHelp,
            "창",
            "도움말 열기",
            "도움말 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.WindowRegistration,
            "창",
            "등록 안내",
            "등록 페이지 안내 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.WindowAbout,
            "창",
            "정보",
            "EasiSlides 정보 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.AddExternalFile,
            "예배 순서",
            "파일 추가",
            "PowerPoint/미디어 파일을 예배 순서에 추가합니다.",
            isDangerous: false),
    };

    public CommandCatalog()
        : this(DefaultCommands)
    {
    }

    // 명시적 명령 집합으로 카탈로그를 만든다(명령 팔레트 테스트·커스텀 명령 세트용). 기본 8개 외 주입 가능.
    public CommandCatalog(IEnumerable<CommandDescriptor> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        All = commands.ToList().AsReadOnly();
    }

    public IReadOnlyList<CommandDescriptor> All { get; }

    public CommandDescriptor? FindById(string commandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        return All.FirstOrDefault(command => string.Equals(command.Id, commandId, StringComparison.Ordinal));
    }

    public IReadOnlyList<Shortcut> GetDefaultShortcuts()
        => All.SelectMany(command => command.DefaultShortcuts).ToList().AsReadOnly();

    private static CommandDescriptor Command(
        string id,
        string category,
        string displayName,
        string description,
        bool isDangerous,
        params Shortcut[] defaultShortcuts)
        => new(id, category, displayName, description, isDangerous, Array.AsReadOnly(defaultShortcuts.ToArray()));
}
