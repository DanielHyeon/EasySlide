using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Axicultus.Localization;

namespace Axicultus.Util;

internal class DisplayInfo
{
    internal static MonitorPlacement Output { get; private set; }
    internal static MonitorPlacement Lyrics { get; private set; }
    internal static string ActiveOutputMonitorName => Output.DeviceName ?? MonitorLayout.None;

    internal static MonitorDevice[] GetDisplays() => [.. Screen.AllScreens
        .Where(screen => screen.Bounds.Width > 0 && screen.Bounds.Height > 0)
        .Select(screen => new MonitorDevice(screen.DeviceName, screen.Bounds, screen.Primary))];

    internal static (MonitorPlacement Output, MonitorPlacement Lyrics) ResolveLaunchDisplays()
    {
        var displays = GetDisplays();
        bool custom = Gf.DualMonitorSelectAutoOption == 1;
        bool wide = Gf.isScreenWideMode;
        Point? outputOrigin = custom ? new Point(Gf.DMOption1Left, Gf.DMOption1Top) : null;
        var output = MonitorLayout.ResolveOutput(displays, Gf.OutputMonitorName, Gf.DMAlwaysUseSecondaryMonitor, wide,
            outputOrigin, Gf.DMOption1Width, Gf.DMOption1AsSingleMonitor);
        Point? lyricsOrigin = Gf.LMSelectAutoOption == 1 ? new Point(Gf.LMOption1Left, Gf.LMOption1Top) : null;
        string operatorDisplayName = FrmMain.frmMain is { IsHandleCreated: true, IsDisposed: false } main
            ? Screen.FromHandle(main.Handle).DeviceName : null;
        var lyrics = MonitorLayout.ResolveLyrics(displays, Gf.LyricsMonitorName, Gf.LMAlwaysUseSecondaryMonitor, wide,
            output, lyricsOrigin, Gf.LMOption1Width, operatorDisplayName);
        return (output, lyrics);
    }

    public static void SizeLaunchDisplay() => ApplyLaunchDisplays(ResolveLaunchDisplays());

    internal static void ApplyLaunchDisplays((MonitorPlacement Output, MonitorPlacement Lyrics) placement)
    {
        (Output, Lyrics) = placement;
        Gf.LS_Left = Output.Bounds.Left;
        Gf.LS_Top = Output.Bounds.Top;
        Gf.LS_Width = Output.Bounds.Width;
        Gf.LS_Height = Output.Bounds.Height;
        Gf.DualMonitorMode = Output.IsDual;
        Gf.OutputMonitorText = UiText.Get(nameof(DisplayInfo), !Output.Enabled ? "NoOutputMonitor" : Gf.DualMonitorSelectAutoOption == 1 ? "CustomMonitorMode"
            : Output.IsDual ? "DualMonitorMode" : "SingleMonitorMode");

        if (Gf.LS_Height >= 768)
        {
            Gf.Buffer_LS_Height = Gf.LS_Height;
            Gf.Buffer_LS_Width = Gf.LS_Width;
        }
        else
        {
            float bufferScale = 768f / (float)Gf.LS_Height;
            Gf.Buffer_LS_Height = 768;
            Gf.Buffer_LS_Width = (int)((float)Gf.LS_Width * bufferScale);
        }
        Gf.ShowTopBorderSize = (int)((double)Gf.Buffer_LS_Height * Gf.TopBorderFactor);
        Gf.ShowBottomBorderSize = (int)((double)Gf.Buffer_LS_Height * Gf.BottomBorderFactor);
        Gf.AdjustedOutlineThreshold = Gf.OutlineFontSizeThreshold * Gf.Buffer_LS_Width / 960;

        Gf.LM_Left = Lyrics.Bounds.Left;
        Gf.LM_Top = Lyrics.Bounds.Top;
        Gf.LM_Width = Lyrics.Bounds.Width;
        Gf.LM_Height = Lyrics.Bounds.Height;
    }

    public static Screen getPrimaryDisplay()
    {
        Screen primaryScreen = null;
        Screen[] allScreens = Screen.AllScreens;
        foreach (Screen screen in allScreens)
        {
            if (screen.Primary)
            {
                primaryScreen = screen;
                break;
            }
        }

        return primaryScreen;
    }

    public static int getPrimaryDisplayIndex()
    {
        int monitorIndex = 0;

        Screen[] allScreens = Screen.AllScreens;

        for (int i = 0; i < allScreens.Length; i++)
        {
            if (allScreens[i] != null)
            {
                if (allScreens[i].Primary)
                {
                    monitorIndex = i;
                    break;
                }
            }
        }

        return monitorIndex;
    }

    public static int getSecondryMonitorIndex(int skipMonitorIndex = -1)
    {
        int monitorIndex = 0;

        Screen[] allScreens = Screen.AllScreens;

        for (int i = 0; i < allScreens.Length; i++)
        {
            if (!allScreens[i].Primary)
            {
                if (skipMonitorIndex == i)
                    continue;

                monitorIndex = i;
                break;
            }
        }

        return monitorIndex;
    }

    public static string getPrimaryDisplayName()
    {
        string primaryMonitorName = "None";

        Screen[] allScreens = Screen.AllScreens;
        foreach (Screen screen in allScreens)
        {
            if (screen.Primary)
            {
                primaryMonitorName = screen.DeviceName;
                break;
            }
        }

        return primaryMonitorName;
    }

    public static string getSecondryDisplayName()
    {
        string secondryMonitorName = "None";

        Screen[] allScreens = Screen.AllScreens;
        foreach (Screen screen in allScreens)
        {
            if (!screen.Primary)
            {
                secondryMonitorName = screen.DeviceName;
                break;
            }
        }

        return secondryMonitorName;
    }

    public static (string, Screen) GetSecondryDisplay()
    {
        string secondryMonitorName = "None";
        Screen secondryScreen = null;

        Screen[] allScreens = Screen.AllScreens;
        foreach (Screen screen in allScreens)
        {
            if (!screen.Primary)
            {
                secondryMonitorName = screen.DeviceName;
                secondryScreen = screen;
                break;
            }
        }

        return (secondryMonitorName, secondryScreen);
    }

    public static void GetDisplayName(ref string inMonitorName, string skipMonitor, bool isAllwaysSecondryMonitor = false)
    {
        Screen[] allScreens = Screen.AllScreens;

        if (allScreens.Length == 1)
        {
            inMonitorName = allScreens[0].DeviceName;
            return;
        }

        try
        {
            foreach (Screen screen in allScreens)
            {
                if (screen.DeviceName != skipMonitor && !screen.Primary && isAllwaysSecondryMonitor)
                {
                    inMonitorName = screen.DeviceName;
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    public static void GetDisplayIndex(ref int inMonitor, int skipMonitor)
    {
        Screen[] allScreens = Screen.AllScreens;

        try
        {
            if (allScreens[inMonitor] == null)
            {
                //None ?�로 ?�정
                inMonitor = getPrimaryDisplayIndex();
                return;
            }

            if (skipMonitor != -1)
            {
                inMonitor = getSecondryMonitorIndex(skipMonitor);
            }
        }
        catch (Exception e)
        {
            inMonitor = 0;
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    public static void GetDisplayInfo(string inMonitorName, ref int t, ref int l, ref int w, ref int h)
    {

        Screen[] allScreens = Screen.AllScreens;

        Screen selectScreen = null;

        foreach (Screen screen in allScreens)
        {
            if (screen.DeviceName == inMonitorName)
            {
                selectScreen = screen;
                break;
            }
        }

        try
        {
            if (selectScreen != null)
            {
                Size screenSize = selectScreen.Bounds.Size;

                t = selectScreen.Bounds.Y;
                l = selectScreen.Bounds.X;
                h = screenSize.Height;
                w = screenSize.Width;
            }
            else
            {
                Screen primaryScreen = getPrimaryDisplay();
                Size screenSize = primaryScreen.Bounds.Size;

                t = primaryScreen.Bounds.Y;
                l = primaryScreen.Bounds.X;
                h = screenSize.Height;
                w = screenSize.Width;
            }
        }
        catch (Exception e)
        {
            Screen primaryScreen = getPrimaryDisplay();
            Size screenSize = primaryScreen.Bounds.Size;

            t = primaryScreen.Bounds.Y;
            l = primaryScreen.Bounds.X;
            h = screenSize.Height;
            w = screenSize.Width;

            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    public static void GetDisplayName(ref string inMonitorName, ref int t, ref int l, ref int w, ref int h, string skipMonitor, bool isAllwaysSecondryMonitor = false)
    {

        Screen[] allScreens = Screen.AllScreens;

        Screen selectScreen = null;

        foreach (Screen screen in allScreens)
        {
            if (screen.DeviceName == inMonitorName && screen.DeviceName != skipMonitor)
            {
                selectScreen = screen;
                break;
            }
        }

        int numberOfScreens = allScreens.Length;

        if (selectScreen == null)
        {
            switch (numberOfScreens)
            {
                case 0:
                    inMonitorName = "None";
                    break;
                case 1:
                    selectScreen = allScreens[0];
                    inMonitorName = selectScreen.DeviceName;
                    break;
                default:
                    inMonitorName = getSecondryDisplayName();
                    break;
            }
        }

        try
        {
            if ((skipMonitor != "None") && (skipMonitor == getPrimaryDisplayName()) && isAllwaysSecondryMonitor)
            {
                (inMonitorName, selectScreen) = GetSecondryDisplay();
            }

            if (selectScreen != null)
            {

                Size screenSize = selectScreen.Bounds.Size;

                t = selectScreen.Bounds.Y;
                l = selectScreen.Bounds.X;
                h = screenSize.Height;
                // daniel
                // ?�리?�테?�션 ?�이�??�?�드�?변�?
                // //w = h * 4 / 3;
                if (Gf.isScreenWideMode)
                    w = screenSize.Width;
                else
                    w = h * 4 / 3;
            }
        }
        catch (Exception e)
        {
            Screen primaryScreen = getPrimaryDisplay();
            if (primaryScreen != null)
            {
                Size screenSize = primaryScreen.Bounds.Size;

                t = primaryScreen.Bounds.Y;
                l = primaryScreen.Bounds.X;
                h = screenSize.Height;
                // daniel
                // ?�리?�테?�션 ?�이�??�?�드�?변�?
                // //w = h * 4 / 3;
                if (Gf.isScreenWideMode)
                    w = screenSize.Width;
                else
                    w = h * 4 / 3;
            }

            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    public static void GetScreenNumber(ref int inMonitor, ref int t, ref int l, ref int w, ref int h, int skipMonitor)
    {

        Screen[] allScreens = Screen.AllScreens;

        int monitorIndex = inMonitor;

        try
        {
            if ((skipMonitor != -1) && (skipMonitor == getPrimaryDisplayIndex()))
            {
                monitorIndex = getSecondryMonitorIndex();
            }

            Screen selectScreen = Screen.AllScreens[monitorIndex];

            Size screenSize = selectScreen.Bounds.Size;

            t = selectScreen.Bounds.X;
            l = selectScreen.Bounds.Y;
            h = screenSize.Height;
            // daniel
            // ?�리?�테?�션 ?�이�??�?�드�?변�?
            // //w = h * 4 / 3;
            if (Gf.isScreenWideMode)
                w = screenSize.Width;
            else
                w = h * 4 / 3;
        }
        catch (Exception e)
        {
            Screen primaryScreen = getPrimaryDisplay();
            Size screenSize = primaryScreen.Bounds.Size;

            t = primaryScreen.Bounds.X;
            l = primaryScreen.Bounds.Y;
            h = screenSize.Height;
            // daniel
            // ?�리?�테?�션 ?�이�??�?�드�?변�?
            // //w = h * 4 / 3;
            if (Gf.isScreenWideMode)
                w = screenSize.Width;
            else
                w = h * 4 / 3;

            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    /// <summary>
    /// 모든 모니터의 DeviceName 리스트를 반환합니다.
    /// </summary>
    public static List<string> GetAllMonitorsList()
    {
        List<string> monitors = [];
        Screen[] allScreens = Screen.AllScreens;

        foreach (Screen screen in allScreens)
        {
            if (screen != null)
            {
                monitors.Add(screen.DeviceName);
            }
        }

        return monitors;
    }

    /// <summary>
    /// Primary가 아닌 모니터들의 DeviceName 리스트를 반환합니다.
    /// 마지막에 "None"을 추가합니다.
    /// </summary>
    public static List<string> GetNonPrimaryMonitorsList()
    {
        List<string> monitors = [];
        Screen[] allScreens = Screen.AllScreens;

        foreach (Screen screen in allScreens)
        {
            if (screen != null && !screen.Primary)
            {
                monitors.Add(screen.DeviceName);
            }
        }

        monitors.Add("None");
        return monitors;
    }
}
