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
    public void All_IncludesWindowLaunchersAndOperatorCommands_ForPalette()
    {
        // §7.4 명령 팔레트 흡수: 카탈로그가 분리 창 런처 + 화면 제어 보강 명령을 포함해 ⌘K 로 검색·실행되게 한다.
        var sut = new CommandCatalog();

        foreach (var id in new[]
                 {
                     MainCommandIds.WindowLibrary, MainCommandIds.WindowBible, MainCommandIds.WindowSearch,
                     MainCommandIds.WindowImportExport, MainCommandIds.WindowExternalFiles,
                     MainCommandIds.WindowManageLists, MainCommandIds.WindowSettings, MainCommandIds.WindowHelp,
                     MainCommandIds.WindowRegistration, MainCommandIds.WindowAbout, MainCommandIds.AddExternalFile,
                     MainCommandIds.LiveClear, MainCommandIds.LiveRestart, MainCommandIds.LiveRefresh,
                     MainCommandIds.LiveRestore, MainCommandIds.LiveAutoRotate,
                 })
        {
            sut.FindById(id).Should().NotBeNull($"카탈로그에 {id} 명령이 있어야 팔레트에서 실행 가능");
        }

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
