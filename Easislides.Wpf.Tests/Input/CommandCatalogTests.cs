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
