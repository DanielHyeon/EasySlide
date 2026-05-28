using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Easislides.Wpf.Input;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Platform;

public class PlatformDiagnosticsServiceTests
{
    [Fact]
    public void Capture_ReportsDisplaysInputAndCommandCatalog()
    {
        var primary = new OutputDisplay("primary", "Primary", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var secondary = new OutputDisplay("secondary", "Secondary", 1920, 0, 1920, 1080, 1);
        var display = new FakeDisplayService(new[] { primary, secondary }, primary, secondary);
        var input = new FakeGlobalInputService { IsRunning = true };
        var catalog = new FakeCommandCatalog(
            new[]
            {
                Command("Live.Next", Shortcut(Key.F5, ModifierKeys.None, "Live.Next", isGlobal: true)),
                Command("Live.Black", Shortcut(Key.B, ModifierKeys.Control, "Live.Black", isGlobal: false))
            });
        var sut = new PlatformDiagnosticsService(display, input, catalog);

        var snapshot = sut.Capture();

        snapshot.DisplayCount.Should().Be(2);
        snapshot.PrimaryDisplayId.Should().Be("primary");
        snapshot.PreferredDisplayId.Should().Be("secondary");
        snapshot.IsGlobalInputRunning.Should().BeTrue();
        snapshot.CommandCount.Should().Be(2);
        snapshot.DefaultShortcutCount.Should().Be(2);
        snapshot.GlobalShortcutCount.Should().Be(1);
        snapshot.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void Capture_WhenGlobalInputHasError_AddsWarning()
    {
        var sut = new PlatformDiagnosticsService(
            FakeDisplayService.OneDisplay(),
            new FakeGlobalInputService { LastError = new InvalidOperationException("hook unavailable") },
            new CommandCatalog());

        var snapshot = sut.Capture();

        snapshot.GlobalInputError.Should().Be("InvalidOperationException: hook unavailable");
        snapshot.Warnings.Should().Contain("Global input error: InvalidOperationException: hook unavailable");
    }

    [Fact]
    public void Capture_WhenDisplayServiceFails_AddsWarningAndContinues()
    {
        var display = new FakeDisplayService { ThrowOnRead = true };
        var sut = new PlatformDiagnosticsService(display, new FakeGlobalInputService(), new CommandCatalog());

        var snapshot = sut.Capture();

        snapshot.DisplayCount.Should().Be(0);
        snapshot.PrimaryDisplayId.Should().BeNull();
        snapshot.PreferredDisplayId.Should().BeNull();
        snapshot.Warnings.Should().Contain("Display diagnostics failed: InvalidOperationException: display unavailable");
        snapshot.CommandCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Capture_WhenCatalogHasDuplicateIdsAndShortcuts_AddsWarnings()
    {
        var duplicateShortcut = Shortcut(Key.F5, ModifierKeys.None, "Live.Next", isGlobal: true);
        var catalog = new FakeCommandCatalog(
            new[]
            {
                Command("Live.Next", duplicateShortcut),
                Command("Live.Next", duplicateShortcut)
            });
        var sut = new PlatformDiagnosticsService(FakeDisplayService.OneDisplay(), new FakeGlobalInputService(), catalog);

        var snapshot = sut.Capture();

        snapshot.Warnings.Should().Contain("Duplicate command ids: Live.Next");
        snapshot.Warnings.Should().Contain("Duplicate shortcuts: Global None+F5");
    }

    private static CommandDescriptor Command(string id, params Shortcut[] shortcuts)
        => new(id, "Test", id, id, IsDangerous: false, shortcuts);

    private static Shortcut Shortcut(Key key, ModifierKeys modifiers, string commandId, bool isGlobal)
        => new(key, modifiers, commandId, isGlobal, commandId);

    private sealed class FakeDisplayService : IDisplayService
    {
        private readonly IReadOnlyList<OutputDisplay> _displays;
        private readonly OutputDisplay _primary;
        private readonly OutputDisplay _preferred;

        public FakeDisplayService()
        {
            _primary = OutputDisplay.PrimaryFallback;
            _preferred = OutputDisplay.PrimaryFallback;
            _displays = new[] { OutputDisplay.PrimaryFallback };
        }

        public FakeDisplayService(IReadOnlyList<OutputDisplay> displays, OutputDisplay primary, OutputDisplay preferred)
        {
            _displays = displays;
            _primary = primary;
            _preferred = preferred;
        }

        public bool ThrowOnRead { get; init; }

        public static FakeDisplayService OneDisplay() => new();

        public IReadOnlyList<OutputDisplay> GetDisplays()
        {
            if (ThrowOnRead)
            {
                throw new InvalidOperationException("display unavailable");
            }

            return _displays;
        }

        public OutputDisplay GetPrimaryDisplay() => _primary;

        public OutputDisplay GetPreferredDisplay(string? preferredDisplayId = null) => _preferred;
    }

    private sealed class FakeGlobalInputService : IGlobalInputService
    {
        public bool IsRunning { get; init; }

        public Exception? LastError { get; init; }

        public bool Start() => true;

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeCommandCatalog : ICommandCatalog
    {
        public FakeCommandCatalog(IReadOnlyList<CommandDescriptor> commands) => All = commands;

        public IReadOnlyList<CommandDescriptor> All { get; }

        public CommandDescriptor? FindById(string id)
            => All.FirstOrDefault(command => string.Equals(command.Id, id, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<Shortcut> GetDefaultShortcuts()
            => All.SelectMany(command => command.DefaultShortcuts).ToArray();
    }
}
