using System;
using System.Linq;
using System.Windows.Input;
using Easislides.Wpf.Input;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Input;

public class CommandCatalogTests
{
    [Fact]
    public void All_HasUniqueCommandIds()
    {
        var sut = new CommandCatalog();

        sut.All.Select(command => command.Id)
            .Should()
            .OnlyHaveUniqueItems();
    }

    [Fact]
    public void All_DefaultShortcutsReferenceKnownCommands()
    {
        var sut = new CommandCatalog();
        var commandIds = sut.All.Select(command => command.Id).ToHashSet();

        sut.GetDefaultShortcuts()
            .Should()
            .OnlyContain(shortcut => commandIds.Contains(shortcut.CommandName));
    }

    [Fact]
    public void All_DefaultShortcutsDoNotCollide()
    {
        var sut = new CommandCatalog();

        sut.GetDefaultShortcuts()
            .GroupBy(shortcut => new { shortcut.Key, shortcut.Modifiers })
            .Should()
            .OnlyContain(group => group.Count() == 1);
    }

    [Theory]
    // FrmMain 라이브 운영 F-키 파리티: F12=Go Live, F11=송출 후 다음, F8=Preview→Output,
    // F7=Preview→Output+Black 해제, F10/F9=검은 화면, F3=화면 비우기, F1=도움말.
    [InlineData(Key.F12, MainCommandIds.LiveGo)]
    [InlineData(Key.F11, MainCommandIds.LiveGoAndNext)]
    [InlineData(Key.F8, MainCommandIds.LivePreviewToOutput)]
    [InlineData(Key.F7, MainCommandIds.LivePreviewToOutputClearBlack)]
    [InlineData(Key.F10, MainCommandIds.LiveBlack)]
    [InlineData(Key.F9, MainCommandIds.LiveBlack)]
    [InlineData(Key.F3, MainCommandIds.LiveClear)]
    [InlineData(Key.F1, MainCommandIds.WindowHelp)]
    public void GetDefaultShortcuts_IncludesFrmMainFunctionKeys(Key key, string commandId)
    {
        var sut = new CommandCatalog();

        sut.GetDefaultShortcuts()
            .Should()
            .Contain(shortcut => shortcut.Key == key
                && shortcut.Modifiers == ModifierKeys.None
                && shortcut.CommandName == commandId);
    }

    [Theory]
    // FrmMain 라이브 항목 이동 파리티: Space/Shift+Space 는 로컬 UI 키, F5/F4 는 라이브 운용용 글로벌 키로 유지.
    [InlineData(Key.Space, ModifierKeys.None, MainCommandIds.LiveNext, false)]
    [InlineData(Key.Space, ModifierKeys.Shift, MainCommandIds.LivePrevious, false)]
    [InlineData(Key.F5, ModifierKeys.None, MainCommandIds.LiveNext, true)]
    [InlineData(Key.F4, ModifierKeys.None, MainCommandIds.LivePrevious, true)]
    public void GetDefaultShortcuts_IncludesFrmMainNavigationKeys(
        Key key,
        ModifierKeys modifiers,
        string commandId,
        bool isGlobal)
    {
        var sut = new CommandCatalog();

        sut.GetDefaultShortcuts()
            .Should()
            .Contain(shortcut => shortcut.Key == key
                && shortcut.Modifiers == modifiers
                && shortcut.CommandName == commandId
                && shortcut.IsGlobal == isGlobal);
    }

    [Theory]
    // 화면 제어 명령 현대 단축키: Ctrl+R=처음으로(Restart), Ctrl+F5=출력 새로고침(하드 새로고침 관용). 레거시 F5 충돌 회피.
    [InlineData(Key.R, MainCommandIds.LiveRestart)]
    [InlineData(Key.F5, MainCommandIds.LiveRefresh)]
    public void GetDefaultShortcuts_IncludesModernControlShortcuts(Key key, string commandId)
    {
        var sut = new CommandCatalog();

        sut.GetDefaultShortcuts()
            .Should()
            .Contain(shortcut => shortcut.Key == key
                && shortcut.Modifiers == ModifierKeys.Control
                && shortcut.CommandName == commandId);
    }

    [Fact]
    public void CtrlF5_DoesNotCollideWithGlobalF5Next()
    {
        // Ctrl+F5(출력 새로고침)와 F5(다음 항목, 전역)는 수식 키가 달라 서로 다른 단축키다(충돌 아님).
        var sut = new CommandCatalog();
        var shortcuts = sut.GetDefaultShortcuts();

        shortcuts.Should().Contain(s => s.Key == Key.F5 && s.Modifiers == ModifierKeys.Control && s.CommandName == MainCommandIds.LiveRefresh);
        shortcuts.Should().Contain(s => s.Key == Key.F5 && s.Modifiers == ModifierKeys.None && s.CommandName == MainCommandIds.LiveNext);
    }

    [Fact]
    public void DuplicateItem_HasCtrlDShortcut_AndIsInPalette()
    {
        // 예배 순서 항목 복제 — Ctrl+D 단축키 + 명령 팔레트(⌘K) 검색·실행(모던 UX 보강).
        var sut = new CommandCatalog();

        sut.FindById(MainCommandIds.WorshipDuplicateItem).Should().NotBeNull("팔레트에서 복제 명령 실행 가능");

        sut.GetDefaultShortcuts()
            .Should()
            .Contain(shortcut => shortcut.Key == Key.D
                && shortcut.Modifiers == ModifierKeys.Control
                && shortcut.CommandName == MainCommandIds.WorshipDuplicateItem);
    }

    [Fact]
    public void WorshipReorderCommands_InCatalog_WithMoveShortcuts()
    {
        // 예배 순서 재정렬 4종이 팔레트(카탈로그)에 있고, 위/아래는 Ctrl+Shift+Up/Down 단축키를 가진다(맨위/맨아래는 팔레트 전용).
        var sut = new CommandCatalog();

        foreach (var id in new[]
                 {
                     MainCommandIds.WorshipMoveItemUp, MainCommandIds.WorshipMoveItemDown,
                     MainCommandIds.WorshipMoveItemToTop, MainCommandIds.WorshipMoveItemToBottom,
                 })
        {
            sut.FindById(id).Should().NotBeNull($"카탈로그에 {id} 가 있어야 팔레트에서 검색·실행 가능")
                .And.Match<CommandDescriptor>(c => c.Category == "예배 순서" && !c.IsDangerous);
        }

        sut.GetDefaultShortcuts().Should().Contain(s =>
            s.Key == Key.Up && s.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && s.CommandName == MainCommandIds.WorshipMoveItemUp);
        sut.GetDefaultShortcuts().Should().Contain(s =>
            s.Key == Key.Down && s.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && s.CommandName == MainCommandIds.WorshipMoveItemDown);
    }

    [Fact]
    public void WorshipQuickSave_InCatalog_WithCtrlS()
    {
        // 빠른 저장이 팔레트에 있고 표준 저장 단축키 Ctrl+S 를 가진다.
        var sut = new CommandCatalog();
        sut.FindById(MainCommandIds.WorshipQuickSave).Should().NotBeNull()
            .And.Match<CommandDescriptor>(c => c.Category == "예배 순서" && !c.IsDangerous);
        sut.GetDefaultShortcuts().Should().Contain(s =>
            s.Key == Key.S && s.Modifiers == ModifierKeys.Control && s.CommandName == MainCommandIds.WorshipQuickSave);
    }

    [Fact]
    public void WorshipSelectLiveItem_InCatalog_ForPalette()
    {
        // 현재 송출 항목 선택이 팔레트에 있다(단축키 없이 팔레트·버튼으로 실행).
        var sut = new CommandCatalog();
        sut.FindById(MainCommandIds.WorshipSelectLiveItem).Should().NotBeNull()
            .And.Match<CommandDescriptor>(c => c.Category == "예배 순서" && !c.IsDangerous);
    }

    [Fact]
    public void FindById_ReturnsDangerMetadataForLiveCommands()
    {
        var sut = new CommandCatalog();

        sut.FindById(MainCommandIds.LiveGo).Should().NotBeNull()
            .And.Match<CommandDescriptor>(command => command.IsDangerous);
        sut.FindById(MainCommandIds.LiveNext).Should().NotBeNull()
            .And.Match<CommandDescriptor>(command => !command.IsDangerous);
    }

    [Fact]
    public void AccessibleName_ComposesDisplayNameCategory_AndMarksDanger()
    {
        // 팔레트 결과를 스크린리더가 한 호흡에 읽도록 항목 레벨 접근성 이름을 합성한다.
        // 위험 명령은 "위험 명령" 표시를 포함해 색에 의존하지 않고도 위험을 음성으로 전달한다.
        var danger = new CommandDescriptor("X", "Live", "라이브 중지", "설명", IsDangerous: true, Array.Empty<Shortcut>());
        var safe = new CommandDescriptor("Y", "창", "라이브러리 열기", "설명", IsDangerous: false, Array.Empty<Shortcut>());

        // 순서·구분자까지 고정 — "이름, [위험 명령,] 분류" 가 스크린리더 읽기 순서의 계약이다(단축키 없으면 끝에 안 붙는다).
        danger.AccessibleName.Should().Be("라이브 중지, 위험 명령, Live");
        safe.AccessibleName.Should().Be("라이브러리 열기, 창");
        safe.AccessibleName.Should().NotContain("위험", "안전한 명령은 위험 표시가 없어야 함");
    }

    [Fact]
    public void ShortcutHint_IsPrimaryShortcutDisplayText_OrEmpty()
    {
        // 팔레트 단축키 힌트 — 기본 단축키(첫 번째)의 표시 문자열. 단축키 없으면 빈 문자열(힌트 자동 숨김).
        var withKey = new CommandDescriptor("X", "Live", "Go Live", "설명", IsDangerous: true,
            new[] { new Shortcut(Key.L, ModifierKeys.Control, "X", IsGlobal: false, "Go Live") });
        var noKey = new CommandDescriptor("Y", "창", "라이브러리 열기", "설명", IsDangerous: false, Array.Empty<Shortcut>());

        withKey.ShortcutHint.Should().Be("Ctrl+L", "첫 단축키의 표시 문자열");
        noKey.ShortcutHint.Should().BeEmpty("단축키 없으면 빈 문자열");
    }

    [Fact]
    public void AccessibleName_AppendsShortcut_WhenPresent()
    {
        // 단축키가 있으면 스크린리더가 끝에 "단축키 X"로 안내한다(색·시각에 안 기대는 발견가능성). 없으면 안 붙음(위 테스트가 고정).
        var withKey = new CommandDescriptor("X", "Live", "검은 화면", "설명", IsDangerous: true,
            new[] { new Shortcut(Key.F9, ModifierKeys.None, "X", IsGlobal: false, "검은 화면") });

        withKey.AccessibleName.Should().Be("검은 화면, 위험 명령, Live, 단축키 F9");
    }

    [Fact]
    public void All_IncludesWindowLaunchersAndOperatorCommands_ForPalette()
    {
        // §7.4 명령 팔레트 흡수: 카탈로그가 분리 창 런처 + 화면 제어 보강 명령을 포함해 ⌘K 로 검색·실행되게 한다.
        var sut = new CommandCatalog();

        foreach (var id in new[]
                 {
                     MainCommandIds.WindowLibrary, MainCommandIds.WindowBible, MainCommandIds.WindowManageBibleVersions, MainCommandIds.WindowSearch,
                     MainCommandIds.WindowImportExport, MainCommandIds.WindowExternalFiles,
                     MainCommandIds.WindowManageLists, MainCommandIds.WindowSettings, MainCommandIds.WindowHelp,
                     MainCommandIds.WindowRegistration, MainCommandIds.WindowAbout, MainCommandIds.AddExternalFile,
                     MainCommandIds.LiveClear, MainCommandIds.LiveRestart, MainCommandIds.LiveRefresh,
                     MainCommandIds.LiveRestore, MainCommandIds.LiveAutoRotate, MainCommandIds.LiveGoAndNext,
                     MainCommandIds.LivePreviewToOutput, MainCommandIds.LivePreviewToOutputClearBlack,
                 })
        {
            sut.FindById(id).Should().NotBeNull($"카탈로그에 {id} 명령이 있어야 팔레트에서 실행 가능");
        }

        // 송출 후 다음(F11)은 위험 명령(라이브 전환)으로 표시돼 팔레트에서 경고 배지·음성 안내가 붙는다.
        sut.FindById(MainCommandIds.LiveGoAndNext).Should().NotBeNull()
            .And.Match<CommandDescriptor>(c => c.IsDangerous, "라이브 송출이라 위험 표시");

        // 창 런처는 "창" 카테고리로 묶여 팔레트에서 "창" 검색 시 함께 노출.
        sut.All.Where(c => c.Category == "창").Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void ShortcutDisplayText_UsesFriendlyPageKeyNames()
    {
        new Shortcut(Key.Prior, ModifierKeys.Control, MainCommandIds.LivePrevious, IsGlobal: false, "Previous")
            .DisplayText
            .Should()
            .Be("Ctrl+PageUp");
        new Shortcut(Key.Next, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: false, "Next")
            .DisplayText
            .Should()
            .Be("PageDown");
    }
}
