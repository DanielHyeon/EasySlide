using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Axicultus;

public static partial class HookManager
{
    internal delegate nint HookProc(int nCode, nuint wParam, nint lParam);

    // Windows can still reference a callback when unhooking fails.
    private static readonly HookProc s_MouseDelegate = MouseHookProc;
    private static readonly HookProc s_KeyboardDelegate = KeyboardHookProc;
    private static nint s_MouseHookHandle;
    private static nint s_KeyboardHookHandle;

    private static int m_OldX;
    private static int m_OldY;

    private static nint MouseHookProc(int nCode, nuint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            //Marshall the data from callback.
            MouseLLHookStruct mouseHookStruct = Marshal.PtrToStructure<MouseLLHookStruct>(lParam);

            if (wParam == WM_MOUSEMOVE && ((s_MouseMove == null && s_MouseMoveExt == null)
                || (m_OldX == mouseHookStruct.Point.X && m_OldY == mouseHookStruct.Point.Y)))
                return Platform.Next(nCode, wParam, lParam);

            //detect button clicked
            MouseButtons button = MouseButtons.None;
            short mouseDelta = 0;
            int clickCount = 0;
            bool mouseDown = false;
            bool mouseUp = false;

            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                    mouseDown = true;
                    button = MouseButtons.Left;
                    clickCount = 1;
                    break;
                case WM_LBUTTONUP:
                    mouseUp = true;
                    button = MouseButtons.Left;
                    clickCount = 1;
                    break;
                case WM_LBUTTONDBLCLK:
                    button = MouseButtons.Left;
                    clickCount = 2;
                    break;
                case WM_RBUTTONDOWN:
                    mouseDown = true;
                    button = MouseButtons.Right;
                    clickCount = 1;
                    break;
                case WM_RBUTTONUP:
                    mouseUp = true;
                    button = MouseButtons.Right;
                    clickCount = 1;
                    break;
                case WM_RBUTTONDBLCLK:
                    button = MouseButtons.Right;
                    clickCount = 2;
                    break;
                case WM_MOUSEWHEEL:
                    //If the message is WM_MOUSEWHEEL, the high-order word of MouseData member is the wheel delta.
                    //One wheel click is defined as WHEEL_DELTA, which is 120.
                    //(value >> 16) & 0xffff; retrieves the high-order word from the given 32-bit value
                    mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);

                //TODO: X BUTTONS (I havent them so was unable to test)
                    //If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
                    //or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released,
                    //and the low-order word is reserved. This value can be one or more of the following values.
                    //Otherwise, MouseData is not used.
                    break;
            }

            //generate event
            MouseEventExtArgs e = new(
                                               button,
                                               clickCount,
                                               mouseHookStruct.Point.X,
                                               mouseHookStruct.Point.Y,
                                               mouseDelta);

            //Mouse up
            if (s_MouseUp!=null && mouseUp)
            {
                s_MouseUp.Invoke(null, e);
            }

            //Mouse down
            if (s_MouseDown != null && mouseDown)
            {
                s_MouseDown.Invoke(null, e);
            }

            //If someone listens to click and a click is heppened
            if (s_MouseClick != null && mouseUp)
            {
                s_MouseClick.Invoke(null, e);
            }

            //If someone listens to click and a click is heppened
            if (s_MouseClickExt != null && mouseUp)
            {
                s_MouseClickExt.Invoke(null, e);
            }

            //If someone listens to double click and a click is heppened
            if (s_MouseDoubleClick != null && clickCount == 2)
            {
                s_MouseDoubleClick.Invoke(null, e);
            }

            //Wheel was moved
            if (s_MouseWheel!=null && mouseDelta!=0)
            {
                s_MouseWheel.Invoke(null, e);
            }

            //If someone listens to move and there was a change in coordinates raise move event
            if ((s_MouseMove!=null || s_MouseMoveExt!=null) && (m_OldX != mouseHookStruct.Point.X || m_OldY != mouseHookStruct.Point.Y))
            {
                m_OldX = mouseHookStruct.Point.X;
                m_OldY = mouseHookStruct.Point.Y;
                s_MouseMove?.Invoke(null, e);

                s_MouseMoveExt?.Invoke(null, e);
            }

            if (e.Handled)
            {
                return -1;
            }
        }

        //call next hook
        return Platform.Next(nCode, wParam, lParam);
    }

    private static void EnsureSubscribedToGlobalMouseEvents() =>
        EnsureInstalled(ref s_MouseHookHandle, WH_MOUSE_LL, s_MouseDelegate);

    private static void EnsureInstalled(ref nint handle, int type, HookProc callback)
    {
        if (handle != 0) return;
        handle = Platform.Install(type, callback);
        if (handle == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    private static void ReleaseHook(ref nint handle)
    {
        if (handle == 0) return;
        if (!Platform.Uninstall(handle))
        {
            int error = Marshal.GetLastPInvokeError();
            // Windows may already have removed a timed-out hook.
            const int ErrorInvalidHookHandle = 1404;
            if (error != ErrorInvalidHookHandle) throw new Win32Exception(error);
        }
        handle = 0;
    }

    private static void TryUnsubscribeFromGlobalMouseEvents()
    {
        //if no subsribers are registered unsubsribe from hook
        if (s_MouseClick == null &&
            s_MouseDown == null &&
            s_MouseMove == null &&
            s_MouseUp == null &&
            s_MouseClickExt == null &&
            s_MouseMoveExt == null &&
            s_MouseWheel == null)
        {
            ReleaseHook(ref s_MouseHookHandle);
        }
    }

    public static bool Shift { get; private set; }
    public static bool Control { get; private set; }
    public static bool Alt { get; private set; }

    // The transitioning key still has its previous asynchronous state inside a
    // low-level hook. Use its event and consult the opposite side on release.
    private static bool ModifierDown(Keys current, bool down, Keys combined, Keys left, Keys right)
    {
        if (current == left) return down || Platform.AsyncState(right) < 0;
        if (current == right) return down || Platform.AsyncState(left) < 0;
        return Platform.AsyncState(combined) < 0;
    }

    private static Keys ReadModifiers(KeyboardHookStruct input, bool down)
    {
        bool extended = (input.Flags & 1) != 0;
        Keys current = (Keys)input.VirtualKeyCode switch
        {
            Keys.ControlKey => extended ? Keys.RControlKey : Keys.LControlKey,
            Keys.Menu => extended ? Keys.RMenu : Keys.LMenu,
            Keys.ShiftKey => input.ScanCode == 0x36 ? Keys.RShiftKey : Keys.LShiftKey,
            var key => key
        };
        Control = ModifierDown(current, down, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey);
        Alt = ModifierDown(current, down, Keys.Menu, Keys.LMenu, Keys.RMenu);
        Shift = ModifierDown(current, down, Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey);
        return (Control ? Keys.Control : Keys.None) | (Alt ? Keys.Alt : Keys.None) | (Shift ? Keys.Shift : Keys.None);
    }

    private static nint KeyboardHookProc(int nCode, nuint wParam, nint lParam)
    {
        bool handled = false;
        if (nCode >= 0)
        {
            bool down = wParam is WM_KEYDOWN or WM_SYSKEYDOWN;
            bool up = wParam is WM_KEYUP or WM_SYSKEYUP;
            var handler = down ? s_KeyDown : up ? s_KeyUp : null;
            bool keyPress = s_KeyPress != null && wParam == WM_KEYDOWN;
            if (handler == null && !keyPress)
                return Platform.Next(nCode, wParam, lParam);

            var input = Marshal.PtrToStructure<KeyboardHookStruct>(lParam);
            Keys modifiers = ReadModifiers(input, down);
            if (handler != null)
            {
                var args = new KeyEventArgs((Keys)input.VirtualKeyCode | modifiers);
                handler(null, args);
                handled = args.Handled;
            }

            // raise KeyPress
            if (keyPress)
            {
                bool isDownShift = (modifiers & Keys.Shift) != 0;

                bool isDownCapslock = (GetKeyState(VK_CAPITAL) & 1) != 0;

                byte[] keyState = new byte[256];
                byte[] inBuffer = new byte[2];
                if (GetKeyboardState(keyState) != 0 && ToAscii(input.VirtualKeyCode,
                          input.ScanCode,
                          keyState,
                          inBuffer,
                          input.Flags) == 1)
                {
                    char key = (char)inBuffer[0];
                    if ((isDownCapslock ^ isDownShift) && Char.IsLetter(key)) key = Char.ToUpper(key);
                    KeyPressEventArgs e = new(key);
                    s_KeyPress?.Invoke(null, e);
                    handled = handled || e.Handled;
                }
            }

        }

        //if event handled in application do not handoff to other listeners
        if (handled)
            return -1;

        //forward to other application
        return Platform.Next(nCode, wParam, lParam);
    }

    private static void EnsureSubscribedToGlobalKeyboardEvents() =>
        EnsureInstalled(ref s_KeyboardHookHandle, WH_KEYBOARD_LL, s_KeyboardDelegate);

    private static void TryUnsubscribeFromGlobalKeyboardEvents()
    {
        //if no subsribers are registered unsubsribe from hook
        if (s_KeyDown == null &&
            s_KeyUp == null &&
            s_KeyPress == null)
        {
            ReleaseHook(ref s_KeyboardHookHandle);
        }
    }

}
