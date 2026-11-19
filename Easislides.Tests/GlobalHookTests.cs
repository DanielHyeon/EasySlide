using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xunit;

namespace Easislides.Tests;

[CollectionDefinition("Global presentation hooks", DisableParallelization = true)]
public sealed class GlobalHookCollection { }

[Collection("Global presentation hooks")]
public sealed class GlobalHookTests : IDisposable
{
    private readonly HookManager.HookApi _original = HookManager.Platform;
    private readonly FakeHookApi _api = new();
    private readonly (bool, bool, bool, bool, bool, bool) _options =
        (Gf.GlobalHookKey_F7, Gf.GlobalHookKey_F8, Gf.GlobalHookKey_F9, Gf.GlobalHookKey_F10,
         Gf.GlobalHookKey_Arrow, Gf.GlobalHookKey_CtrlArrow);

    public GlobalHookTests()
    {
        HookManager.Platform = _api;
        (Gf.GlobalHookKey_F7, Gf.GlobalHookKey_F8, Gf.GlobalHookKey_F9, Gf.GlobalHookKey_F10,
         Gf.GlobalHookKey_Arrow, Gf.GlobalHookKey_CtrlArrow) = (false, false, false, false, false, false);
    }

    public void Dispose()
    {
        HookManager.Platform = _original;
        (Gf.GlobalHookKey_F7, Gf.GlobalHookKey_F8, Gf.GlobalHookKey_F9, Gf.GlobalHookKey_F10,
         Gf.GlobalHookKey_Arrow, Gf.GlobalHookKey_CtrlArrow) = _options;
        _api.Dispose();
    }

    [Fact]
    public void KeyUpAloneCarriesCtrlSnapshotUntilQueuedArrowRuns()
    {
        Gf.GlobalHookKey_CtrlArrow = true;
        var moves = new List<(Keys, bool)>();
        var pending = new Queue<Action>();
        var router = new GlobalShortcutRouter(_ => false,
            (key, control) => pending.Enqueue(() => moves.Add((key, control))), _ => { });
        HookManager.KeyUp += router.OnArrowKey;
        try
        {
            _api.Held.Add(Keys.ControlKey);
            _api.Key(Keys.Down, down: false);
            _api.Held.Clear();
            _api.Key(Keys.Down, down: false);
            _api.Key(Keys.Left, down: false);
            Assert.Empty(moves);
            Assert.Single(pending)();
            Assert.Equal((Keys.Down, true), Assert.Single(moves));
        }
        finally { HookManager.KeyUp -= router.OnArrowKey; }
    }

    [Fact]
    public void ReleasingOneControlPreservesTheOtherAndIgnoresToggleBit()
    {
        var observed = new List<Keys>();
        KeyEventHandler capture = (_, e) => observed.Add(e.Modifiers);
        HookManager.KeyUp += capture;
        try
        {
            _api.Held.Add(Keys.RControlKey);
            _api.Key(Keys.LControlKey, down: false);
            _api.Held.Clear();
            _api.Key(Keys.RControlKey, down: false);
            Assert.Equal(new[] { Keys.Control, Keys.None }, observed);
        }
        finally { HookManager.KeyUp -= capture; }
    }

    [Fact]
    public void OnlyEnabledBlackoutKeysAreConsumedAndUnrelatedKeysQueueNothing()
    {
        Gf.GlobalHookKey_F7 = Gf.GlobalHookKey_F9 = Gf.GlobalHookKey_Arrow = true;
        var queued = new List<Keys>();
        var router = new GlobalShortcutRouter(key => { queued.Add(key); return true; },
            (key, _) => queued.Add(key), queued.Add);
        KeyEventHandler media = (_, e) => router.OnMediaKey(e.KeyCode);
        HookManager.KeyUp += router.OnArrowKey;
        HookManager.KeyDown += router.OnFunctionKey;
        HookManager.KeyDown += media;
        try
        {
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(_api.NextResult, _api.Key(Keys.F3, down: true));
                Assert.Equal(_api.NextResult, _api.Key(Keys.F3, down: false));
            }
            Assert.Empty(queued);
            Assert.Equal((nint)(-1), _api.Key(Keys.F9, down: true));
            Assert.Equal(_api.NextResult, _api.Key(Keys.F10, down: true));
            Gf.GlobalHookKey_F10 = true;
            Assert.Equal((nint)(-1), _api.Key(Keys.F10, down: true));
            Assert.Equal(_api.NextResult, _api.Key(Keys.F7, down: true));
            Assert.Equal(_api.NextResult, _api.Key(Keys.Up, down: false));
            Assert.Equal(_api.NextResult, _api.Key(Keys.Space, down: true));
            Assert.Equal(new[] { Keys.F9, Keys.F10, Keys.F7, Keys.Up, Keys.Space }, queued);
        }
        finally
        {
            HookManager.KeyDown -= media;
            HookManager.KeyDown -= router.OnFunctionKey;
            HookManager.KeyUp -= router.OnArrowKey;
        }
    }

    [Theory]
    [InlineData(0x201, 0x202, MouseButtons.Left)]
    [InlineData(0x204, 0x205, MouseButtons.Right)]
    public void ButtonPairRaisesOneCompletedClick(int down, int up, MouseButtons button)
    {
        var events = new List<string>();
        MouseEventHandler press = (_, _) => events.Add("down");
        MouseEventHandler release = (_, _) => events.Add("up");
        MouseEventHandler click = (_, e) => { Assert.Equal(button, e.Button); events.Add("click"); };
        EventHandler<MouseEventExtArgs> extended = (_, e) => { events.Add("ext"); e.Handled = true; };
        HookManager.MouseDown += press;
        HookManager.MouseUp += release;
        HookManager.MouseClick += click;
        HookManager.MouseClickExt += extended;
        try
        {
            Assert.Equal(_api.NextResult, _api.Mouse(down));
            Assert.Equal((nint)(-1), _api.Mouse(up));
            Assert.Equal(new[] { "down", "up", "click", "ext" }, events);
        }
        finally
        {
            HookManager.MouseDown -= press;
            HookManager.MouseUp -= release;
            HookManager.MouseClick -= click;
            HookManager.MouseClickExt -= extended;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReleaseFailurePreservesHandleForRetry(bool keyboard)
    {
        KeyEventHandler key = (_, _) => { };
        MouseEventHandler mouse = (_, _) => { };
        void Subscribe() { if (keyboard) HookManager.KeyUp += key; else HookManager.MouseUp += mouse; }
        void Remove() { if (keyboard) HookManager.KeyUp -= key; else HookManager.MouseUp -= mouse; }

        Subscribe();
        try
        {
            _api.ReleaseError = 5;
            Assert.Equal(5, Assert.Throws<Win32Exception>(Remove).NativeErrorCode);
            Subscribe();
            Assert.Equal(1, _api.Installs);
            _api.ReleaseError = 0;
            Remove();
            Assert.Equal(new[] { _api.Handle, _api.Handle }, _api.Releases);
            Subscribe();
            Assert.Equal(2, _api.Installs);
        }
        finally { _api.ReleaseError = 0; Remove(); }
    }

    [Fact]
    public void InvalidatedHookCanBeInstalledAgain()
    {
        KeyEventHandler key = (_, _) => { };
        HookManager.KeyDown += key;
        try
        {
            _api.ReleaseError = 1404;
            HookManager.KeyDown -= key;
            HookManager.KeyDown += key;
            Assert.Equal(2, _api.Installs);
        }
        finally { _api.ReleaseError = 0; HookManager.KeyDown -= key; }
    }

    [Theory]
    [InlineData("KeyboardHookStruct", 20, 24, 16, 16)]
    [InlineData("MouseLLHookStruct", 24, 32, 20, 24)]
    public void HookDataMatchesWindowsLayout(string name, int x86, int x64, int extra32, int extra64)
    {
        var type = typeof(HookManager).GetNestedType(name, BindingFlags.NonPublic);
        Assert.Equal(IntPtr.Size == 8 ? x64 : x86, Marshal.SizeOf(type));
        Assert.Equal(IntPtr.Size == 8 ? extra64 : extra32, Marshal.OffsetOf(type, "ExtraInfo").ToInt32());
    }

    [Fact]
    public void NativeSignaturesPreserveHandleMessageAndResultWidths()
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        var type = typeof(HookManager);
        var next = type.GetMethod("CallNextHookEx", flags);
        var callback = typeof(HookManager.HookProc).GetMethod("Invoke");
        Assert.Equal(typeof(nint), type.GetMethod("SetWindowsHookEx", flags).ReturnType);
        Assert.Equal(typeof(nint), type.GetMethod("UnhookWindowsHookEx", flags).GetParameters()[0].ParameterType);
        Assert.Equal(typeof(nint), next.ReturnType);
        Assert.Equal(typeof(nint), next.GetParameters()[0].ParameterType);
        Assert.Equal(typeof(nuint), next.GetParameters()[2].ParameterType);
        Assert.Equal(typeof(nint), callback.ReturnType);
        Assert.Equal(typeof(nuint), callback.GetParameters()[1].ParameterType);
    }

    private sealed class FakeHookApi : HookManager.HookApi, IDisposable
    {
        private const int KeyboardHook = 13, MouseHook = 14;
        private readonly Dictionary<int, HookManager.HookProc> _callbacks = new();
        private readonly nint _data = Marshal.AllocHGlobal(32);
        internal readonly HashSet<Keys> Held = new();
        internal readonly List<nint> Releases = new();
        internal readonly nint Handle = IntPtr.Size == 8 ? unchecked((nint)0x100000055L) : 85;
        internal readonly nint NextResult = IntPtr.Size == 8 ? unchecked((nint)0x100000077L) : 119;
        internal int Installs;
        internal int ReleaseError;

        internal FakeHookApi() => Marshal.Copy(new byte[32], 0, _data, 32);

        internal override nint Install(int type, HookManager.HookProc callback)
        {
            _callbacks[type] = callback;
            Installs++;
            return Handle;
        }
        internal override bool Uninstall(nint handle)
        {
            Releases.Add(handle);
            Marshal.SetLastPInvokeError(ReleaseError);
            return ReleaseError == 0;
        }
        internal override short AsyncState(Keys key) => Held.Contains(key) ? short.MinValue : (short)1;
        internal override nint Next(int code, nuint message, nint data) => NextResult;
        internal nint Key(Keys key, bool down)
        {
            Marshal.WriteInt32(_data, (int)key);
            return _callbacks[KeyboardHook](0, down ? 0x100u : 0x101u, _data);
        }
        internal nint Mouse(int message) => _callbacks[MouseHook](0, (nuint)message, _data);
        public void Dispose() => Marshal.FreeHGlobal(_data);
    }
}
