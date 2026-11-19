using System;
using System.Collections.Generic;
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
        executed.Should().BeFalse();
        dispatcher.Drain();
        executed.Should().BeTrue();
        dispatcher.PostCount.Should().Be(1);
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

    [Fact]
    public void UnrelatedKeysDoNotPostAndStoppedSessionDropsPendingCommands()
    {
        var registry = new ShortcutRegistry();
        var source = new FakeGlobalKeySource();
        var dispatcher = new RecordingGlobalInputDispatcher();
        int executed = 0;
        registry.Register(new Shortcut(WpfKey.F5, WpfModifierKeys.None, "Live.Next", true, "다음"));
        registry.Bind("Live.Next", () => executed++);
        var service = CreateSut(source, registry, dispatcher);
        service.Start();

        for (int i = 0; i < 1000; i++)
            source.RaiseKeyDown(LegacyKeys.F3, LegacyKeys.None).Handled.Should().BeFalse();
        dispatcher.PostCount.Should().Be(0);
        source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None).Handled.Should().BeTrue();
        service.Stop();
        service.Start();
        dispatcher.Drain();
        executed.Should().Be(0);
        source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None);
        dispatcher.Drain();
        executed.Should().Be(1);
    }

    [Fact]
    public void FailedPostPassesTheKeyThroughAndQueuedCommandErrorIsReported()
    {
        var registry = new ShortcutRegistry();
        var source = new FakeGlobalKeySource();
        var dispatcher = new RecordingGlobalInputDispatcher { RejectPosts = true };
        registry.Register(new Shortcut(WpfKey.F5, WpfModifierKeys.None, "Live.Next", true, "다음"));
        registry.Bind("Live.Next", () => throw new InvalidOperationException("command failed"));
        var service = CreateSut(source, registry, dispatcher);
        service.Start();

        source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None).Handled.Should().BeFalse();
        service.LastError!.Message.Should().Be("queue closed");
        dispatcher.RejectPosts = false;
        source.RaiseKeyDown(LegacyKeys.F5, LegacyKeys.None).Handled.Should().BeTrue();
        dispatcher.Drain();
        service.LastError!.Message.Should().Be("command failed");
    }

    [Fact]
    public void WpfDispatcherDefersCommandsEvenOnItsOwningThread()
    {
        StaHelper.RunOnSta(() =>
        {
            var dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            bool executed = false;
            new WpfGlobalInputDispatcher(dispatcher).Post(() => executed = true);
            executed.Should().BeFalse();
            dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => { }));
            executed.Should().BeTrue();
            dispatcher.InvokeShutdown();
        });
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
        private readonly Queue<Action> _pending = new();
        public int PostCount { get; private set; }
        public bool RejectPosts { get; set; }

        public void Drain()
        {
            while (_pending.TryDequeue(out var action)) action();
        }

        public void Post(Action action)
        {
            if (RejectPosts) throw new InvalidOperationException("queue closed");
            PostCount++;
            _pending.Enqueue(action);
        }
    }
}
