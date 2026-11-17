using System;
using Easislides.Wpf.Input;
using FluentAssertions;
using Xunit;
using LegacyKeys = System.Windows.Forms.Keys;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace Easislides.Wpf.Tests.Input;

public class GlobalInputServiceTests
{
    [Fact]
    public void Start_SubscribesAndStartsSourceOnce()
    {
        var source = new FakeGlobalKeySource();
        var sut = CreateSut(source);

        sut.Start().Should().BeTrue();
        sut.Start().Should().BeTrue();

        source.StartCount.Should().Be(1);
        source.SubscriptionCount.Should().Be(1);
        sut.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void KeyDown_WhenGlobalShortcutIsRegistered_RoutesThroughRegistryAndMarksHandled()
    {
        var registry = new ShortcutRegistry();
        var source = new FakeGlobalKeySource();
        var dispatcher = new RecordingGlobalInputDispatcher();
        var executed = false;
        registry.Register(new Shortcut(WpfKey.F5, WpfModifierKeys.None, "Live.Next", IsGlobal: true, "다음"));
        registry.Bind("Live.Next", () => executed = true);
        var sut = CreateSut(source, registry, dispatcher);

        sut.Start();
        var args = source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None);

        args.Handled.Should().BeTrue();
        executed.Should().BeTrue();
        dispatcher.InvokeCount.Should().Be(1);
    }

    [Fact]
    public void KeyDown_WhenShortcutIsLocalOnly_DoesNotHandle()
    {
        var registry = new ShortcutRegistry();
        var source = new FakeGlobalKeySource();
        registry.Register(new Shortcut(WpfKey.F5, WpfModifierKeys.None, "Edit.Find", IsGlobal: false, "찾기"));
        registry.Bind("Edit.Find", () => throw new InvalidOperationException("should not run"));
        var sut = CreateSut(source, registry);

        sut.Start();
        var args = source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None);

        args.Handled.Should().BeFalse();
    }

    [Fact]
    public void Stop_UnsubscribesAndStopsSource()
    {
        var source = new FakeGlobalKeySource();
        var sut = CreateSut(source);

        sut.Start();
        sut.Stop();
        var args = source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None);

        source.StopCount.Should().Be(1);
        source.SubscriptionCount.Should().Be(0);
        sut.IsRunning.Should().BeFalse();
        args.Handled.Should().BeFalse();
    }

    [Fact]
    public void Start_WhenSourceFails_CleansUpAndReturnsFalse()
    {
        var source = new FakeGlobalKeySource { ThrowOnStart = true };
        var sut = CreateSut(source);

        sut.Start().Should().BeFalse();

        sut.IsRunning.Should().BeFalse();
        sut.LastError.Should().BeOfType<InvalidOperationException>();
        source.SubscriptionCount.Should().Be(0);
    }

    private static GlobalInputService CreateSut(
        FakeGlobalKeySource source,
        ShortcutRegistry? registry = null,
        IGlobalInputDispatcher? dispatcher = null)
        => new(registry ?? new ShortcutRegistry(), source, dispatcher ?? new RecordingGlobalInputDispatcher());

    private sealed class FakeGlobalKeySource : IGlobalKeySource
    {
        private event EventHandler<GlobalKeyEventArgs>? KeyDownCore;

        public bool ThrowOnStart { get; init; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public int SubscriptionCount { get; private set; }

        public event EventHandler<GlobalKeyEventArgs>? KeyDown
        {
            add
            {
                KeyDownCore += value;
                SubscriptionCount++;
            }
            remove
            {
                KeyDownCore -= value;
                SubscriptionCount--;
            }
        }

        public void Start()
        {
            if (ThrowOnStart)
            {
                throw new InvalidOperationException("hook unavailable");
            }

            StartCount++;
        }

        public void Stop() => StopCount++;

        public GlobalKeyEventArgs RaiseKeyDown(LegacyKeys key, LegacyKeys modifiers)
        {
            var args = new GlobalKeyEventArgs(key, modifiers);
            KeyDownCore?.Invoke(this, args);
            return args;
        }
    }

    private sealed class RecordingGlobalInputDispatcher : IGlobalInputDispatcher
    {
        public int InvokeCount { get; private set; }

        public void Invoke(Action action)
        {
            InvokeCount++;
            action();
        }
    }
}
