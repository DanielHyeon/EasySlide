using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Input;

public sealed record CommandDescriptor(
    string Id,
    string Category,
    string DisplayName,
    string Description,
    bool IsDangerous,
    IReadOnlyList<Shortcut> DefaultShortcuts)
{
    /// <summary>
    /// 명령 팔레트(⌘K)에 보여 줄 단축키 힌트 — 기본 단축키(첫 번째)의 표시 문자열(예 "Ctrl+L"·"F9"). 단축키가 없으면 빈 문자열.
    /// 메뉴의 InputGestureText 처럼 발견가능성을 높인다(VS Code 식 — 명령 옆에 단축키 표시).
    /// </summary>
    public string ShortcutHint => DefaultShortcuts.Count > 0 ? DefaultShortcuts[0].DisplayText : string.Empty;

    /// <summary>
    /// 명령 팔레트(⌘K) 결과를 스크린리더가 한 호흡에 읽도록 합성한 항목 레벨 접근성 이름.
    /// 자식 TextBlock 들의 합성(비결정적)에 기대지 않고 "이름, [위험 명령,] 분류[, 단축키 X]" 순서로 또렷이 읽어 준다.
    /// 위험 명령은 색 배지뿐 아니라 음성으로도 "위험"을 전달(색에 의존하지 않는 접근성). 단축키가 있으면 끝에 음성으로도 안내한다.
    /// 순서는 이름 먼저 — 목록을 훑을 때 명령명이 1차 식별자라 앞에 두고, 위험·분류·단축키는 뒤따른다.
    /// </summary>
    public string AccessibleName
    {
        get
        {
            var basePart = IsDangerous
                ? UiText.Format(nameof(CommandCatalog), "AccessibleNameDangerous", DisplayName, Category)
                : $"{DisplayName}, {Category}";
            return ShortcutHint.Length > 0 ? UiText.Format(nameof(CommandCatalog), "AccessibleNameShortcut", basePart, ShortcutHint) : basePart;
        }
    }
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
            Text("CategoryOutput"),
            "OutputOpen",
            isDangerous: false),
        Command(
            MainCommandIds.OutputClose,
            Text("CategoryOutput"),
            "OutputClose",
            isDangerous: true),
        Command(
            MainCommandIds.LiveGo,
            Text("CategoryLive"),
            "LiveGo",
            isDangerous: true,
            new Shortcut(Key.L, ModifierKeys.Control, MainCommandIds.LiveGo, IsGlobal: false, Text("LiveGo")),
            // FrmMain 파리티: F12 = Start Show/Go Live.
            new Shortcut(Key.F12, ModifierKeys.None, MainCommandIds.LiveGo, IsGlobal: false, Text("LiveGo"))),
        Command(
            MainCommandIds.LiveGoAndNext,
            Text("CategoryLive"),
            "LiveGoAndNext",
            isDangerous: true,
            // FrmMain 파리티: F11 = Go Live and Move Next.
            new Shortcut(Key.F11, ModifierKeys.None, MainCommandIds.LiveGoAndNext, IsGlobal: false, Text("LiveGoAndNext"))),
        Command(
            MainCommandIds.LivePreviewToOutputClearBlack,
            Text("CategoryLive"),
            "LivePreviewToOutputClearBlack",
            isDangerous: true,
            new Shortcut(Key.F7, ModifierKeys.None, MainCommandIds.LivePreviewToOutputClearBlack, IsGlobal: true, Text("PreviewToOutputClearBlackShortcut"))),
        Command(
            MainCommandIds.LivePreviewToOutput,
            Text("CategoryLive"),
            "LivePreviewToOutput",
            isDangerous: true,
            new Shortcut(Key.F8, ModifierKeys.None, MainCommandIds.LivePreviewToOutput, IsGlobal: true, Text("PreviewToOutputShortcut"))),
        Command(
            MainCommandIds.LiveStop,
            Text("CategoryLive"),
            "LiveStop",
            isDangerous: true),
        Command(
            MainCommandIds.LiveNext,
            Text("CategoryLive"),
            "LiveNext",
            isDangerous: false,
            new Shortcut(Key.Space, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: false, Text("LiveNext")),
            new Shortcut(Key.None, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: true, Text("LiveNext"))),
        Command(
            MainCommandIds.LivePrevious,
            Text("CategoryLive"),
            "LivePrevious",
            isDangerous: false,
            new Shortcut(Key.Space, ModifierKeys.Shift, MainCommandIds.LivePrevious, IsGlobal: false, Text("LivePrevious")),
            new Shortcut(Key.F4, ModifierKeys.None, MainCommandIds.LivePrevious, IsGlobal: true, Text("LivePrevious"))),
        Command(
            MainCommandIds.LiveHide,
            Text("CategoryLive"),
            "LiveHide",
            isDangerous: true,
            new Shortcut(Key.H, ModifierKeys.Control, MainCommandIds.LiveHide, IsGlobal: false, Text("LiveHide"))),
        Command(
            MainCommandIds.LiveBlack,
            Text("CategoryLive"),
            "LiveBlack",
            isDangerous: true,
            new Shortcut(Key.B, ModifierKeys.Control, MainCommandIds.LiveBlack, IsGlobal: false, Text("LiveBlack")),
            // FrmMain 파리티: F9 = Black Screen.
            new Shortcut(Key.F9, ModifierKeys.None, MainCommandIds.LiveBlack, IsGlobal: false, Text("LiveBlack")),
            // FrmMain 글로벌 훅 파리티: F10 도 Black Screen 토글.
            new Shortcut(Key.F10, ModifierKeys.None, MainCommandIds.LiveBlack, IsGlobal: true, Text("LiveBlack"))),
        // 화면 제어 보강(§7.3-B) — 명령 팔레트(⌘K)에서 검색·실행 + FrmMain F3 파리티.
        Command(
            MainCommandIds.LiveClear,
            Text("CategoryLive"),
            "LiveClear",
            isDangerous: true,
            // FrmMain 파리티: F3 = Clear Screen.
            new Shortcut(Key.F3, ModifierKeys.None, MainCommandIds.LiveClear, IsGlobal: false, Text("LiveClear"))),
        Command(
            MainCommandIds.LiveRestart,
            Text("CategoryLive"),
            "LiveRestart",
            isDangerous: false,
            new Shortcut(Key.F5, ModifierKeys.None, MainCommandIds.LiveRestart, IsGlobal: false, Text("LiveRestart")),
            new Shortcut(Key.R, ModifierKeys.Control, MainCommandIds.LiveRestart, IsGlobal: false, Text("LiveRestart"))),
        Command(
            MainCommandIds.LiveRefresh,
            Text("CategoryLive"),
            "LiveRefresh",
            isDangerous: false,
            // 현대 단축키 — 새로고침 관용(Ctrl+F5, "하드 새로고침"). 전역 F5(다음 항목)와 수식 키가 달라 충돌하지 않는다.
            new Shortcut(Key.F5, ModifierKeys.Control, MainCommandIds.LiveRefresh, IsGlobal: false, Text("LiveRefresh"))),
        Command(
            MainCommandIds.LiveRestore,
            Text("CategoryLive"),
            "LiveRestore",
            isDangerous: false),
        Command(
            MainCommandIds.LiveAutoRotate,
            Text("CategoryLive"),
            "LiveAutoRotate",
            isDangerous: false),
        // 예배 순서 편집 — 선택 항목 복제(모던 단축키 Ctrl+D + 명령 팔레트 검색). 텍스트박스 타이핑과 충돌하지 않는 수식 키.
        Command(
            MainCommandIds.WorshipDuplicateItem,
            Text("CategoryWorshipList"),
            "WorshipDuplicateItem",
            isDangerous: false,
            new Shortcut(Key.D, ModifierKeys.Control, MainCommandIds.WorshipDuplicateItem, IsGlobal: false, Text("WorshipDuplicateItem"))),
        // 예배 순서 재정렬(레거시 위/아래/맨위/맨아래 버튼) — 팔레트 검색 + 위/아래는 Ctrl+Shift+Up/Down 단축키.
        // 텍스트박스 편집과 충돌 않는 수식 키 조합(메인 창 텍스트박스는 단일 줄이라 Ctrl+Shift+Arrow 미사용).
        Command(
            MainCommandIds.WorshipMoveItemUp,
            Text("CategoryWorshipList"),
            "WorshipMoveItemUp",
            isDangerous: false,
            new Shortcut(Key.Up, ModifierKeys.Control | ModifierKeys.Shift, MainCommandIds.WorshipMoveItemUp, IsGlobal: false, Text("WorshipMoveItemUp"))),
        Command(
            MainCommandIds.WorshipMoveItemDown,
            Text("CategoryWorshipList"),
            "WorshipMoveItemDown",
            isDangerous: false,
            new Shortcut(Key.Down, ModifierKeys.Control | ModifierKeys.Shift, MainCommandIds.WorshipMoveItemDown, IsGlobal: false, Text("WorshipMoveItemDown"))),
        Command(
            MainCommandIds.WorshipMoveItemToTop,
            Text("CategoryWorshipList"),
            "WorshipMoveItemToTop",
            isDangerous: false),
        Command(
            MainCommandIds.WorshipMoveItemToBottom,
            Text("CategoryWorshipList"),
            "WorshipMoveItemToBottom",
            isDangerous: false),
        // 빠른 저장(Ctrl+S, 표준 저장 단축키) — 현재 세션 이름으로 덮어 저장. 미저장 변경(● 수정됨)과 짝.
        Command(
            MainCommandIds.WorshipQuickSave,
            Text("CategoryWorshipList"),
            "WorshipQuickSave",
            isDangerous: false,
            new Shortcut(Key.S, ModifierKeys.Control, MainCommandIds.WorshipQuickSave, IsGlobal: false, Text("WorshipQuickSave"))),
        // 현재 송출(LIVE) 항목 선택 — 팔레트·버튼. 송출 중이 아니면 CanExecute 가 막는다.
        Command(
            MainCommandIds.WorshipSelectLiveItem,
            Text("CategoryWorshipList"),
            "WorshipSelectLiveItem",
            isDangerous: false),
        // 창 런처(§7.4) — FrmMain 의 분리 창을 명령 팔레트로 흡수. 실행은 View(MainWindow)가 등록.
        Command(
            MainCommandIds.WindowLibrary,
            Text("CategoryWindow"),
            "WindowLibrary",
            isDangerous: false),
        Command(
            MainCommandIds.WindowBible,
            Text("CategoryWindow"),
            "WindowBible",
            isDangerous: false),
        Command(
            MainCommandIds.WindowManageBibleVersions,
            Text("CategoryWindow"),
            "WindowManageBibleVersions",
            isDangerous: false),
        Command(
            MainCommandIds.WindowSearch,
            Text("CategoryWindow"),
            "WindowSearch",
            isDangerous: false,
            new Shortcut(Key.F, ModifierKeys.Control, MainCommandIds.WindowSearch, IsGlobal: false, Text("FindShortcut"))),
        Command(
            MainCommandIds.WindowImportExport,
            Text("CategoryWindow"),
            "WindowImportExport",
            isDangerous: false),
        Command(
            MainCommandIds.WindowExternalFiles,
            Text("CategoryWindow"),
            "WindowExternalFiles",
            isDangerous: false),
        Command(
            MainCommandIds.WindowManageLists,
            Text("CategoryWindow"),
            "WindowManageLists",
            isDangerous: false),
        Command(
            MainCommandIds.WindowSettings,
            Text("CategoryWindow"),
            "WindowSettings",
            isDangerous: false),
        Command(
            MainCommandIds.WindowHelp,
            Text("CategoryWindow"),
            "WindowHelp",
            isDangerous: false,
            // FrmMain 파리티: F1 = Help Contents.
            new Shortcut(Key.F1, ModifierKeys.None, MainCommandIds.WindowHelp, IsGlobal: false, Text("HelpShortcut"))),
        Command(
            MainCommandIds.WindowRegistration,
            Text("CategoryWindow"),
            "WindowRegistration",
            isDangerous: false),
        Command(
            MainCommandIds.WindowAbout,
            Text("CategoryWindow"),
            "WindowAbout",
            isDangerous: false),
        Command(
            MainCommandIds.AddExternalFile,
            Text("CategoryWorshipList"),
            "AddExternalFile",
            isDangerous: false),
        Command(
            MainCommandIds.ImportLegacyWorshipList,
            Text("CategoryWorshipList"),
            "ImportLegacyWorshipList",
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

    // The display name is the text <key>, the description <key>Description.
    private static CommandDescriptor Command(
        string id,
        string category,
        string key,
        bool isDangerous,
        params Shortcut[] defaultShortcuts)
        => new(id, category, Text(key), Text(key + "Description"), isDangerous, Array.AsReadOnly(defaultShortcuts.ToArray()));

    private static string Text(string key) => UiText.Get(nameof(CommandCatalog), key);
}
