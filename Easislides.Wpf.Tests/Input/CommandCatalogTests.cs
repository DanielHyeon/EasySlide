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
    // FrmMain 라이브 운영 F-키 파리티(충돌 없는 키만): F12=Go Live, F11=송출 후 다음, F9=검은 화면, F3=화면 비우기, F1=도움말.
    [InlineData(Key.F12, MainCommandIds.LiveGo)]
    [InlineData(Key.F11, MainCommandIds.LiveGoAndNext)]
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

        // 순서·구분자까지 고정 — "이름, [위험 명령,] 분류" 가 스크린리더 읽기 순서의 계약이다.
        danger.AccessibleName.Should().Be("라이브 중지, 위험 명령, Live");
        safe.AccessibleName.Should().Be("라이브러리 열기, 창");
        safe.AccessibleName.Should().NotContain("위험", "안전한 명령은 위험 표시가 없어야 함");
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
