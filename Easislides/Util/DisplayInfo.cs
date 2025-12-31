using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides.Util
{
    internal class DisplayInfo
    {
        public static void SizeLaunchDisplay()
        {
            if ((gf.OutputMonitorName == getPrimaryDisplayName()) && gf.DMAlwaysUseSecondaryMonitor)
            {
                gf.OutputMonitorName = getSecondryDisplayName();
            }

            GetDisplayName(ref gf.OutputMonitorName, ref gf.LS_Top, ref gf.LS_Left, ref gf.LS_Width, ref gf.LS_Height, "None");

            if (gf.DualMonitorSelectAutoOption == 0)
            {
                if (gf.OutputMonitorName != "None")
                {
                    gf.DualMonitorMode = true;
                    gf.OutputMonitorText = "Dual Monitor Mode";
                }
                else
                {
                    gf.DualMonitorMode = false;
                    gf.OutputMonitorText = "Single Monitor Mode";
                }
            }
            else
            {
                gf.LS_Top = gf.DMOption1Top;
                gf.LS_Left = gf.DMOption1Left;
                gf.LS_Width = gf.DMOption1Width;
                gf.LS_Height = gf.DMOption1Height;
                gf.DualMonitorMode = ((!gf.DMOption1AsSingleMonitor) ? true : false);
                gf.OutputMonitorText = "Custom Monitor Mode";
            }

            if (gf.LS_Height >= 768)
            {
                gf.Buffer_LS_Height = gf.LS_Height;
                gf.Buffer_LS_Width = gf.LS_Width;
            }
            else
            {
                float num = 768f / (float)gf.LS_Height;
                gf.Buffer_LS_Height = 768;
                gf.Buffer_LS_Width = (int)((float)gf.LS_Width * num);
            }
            gf.ShowTopBorderSize = (int)((double)gf.Buffer_LS_Height * gf.TopBorderFactor);
            gf.ShowBottomBorderSize = (int)((double)gf.Buffer_LS_Height * gf.BottomBorderFactor);
            gf.AdjustedOutlineThreshold = gf.OutlineFontSizeThreshold * gf.Buffer_LS_Width / 960;

            if (gf.LMAlwaysUseSecondaryMonitor && (gf.LyricsMonitorName == getPrimaryDisplayName()))
            {
                gf.LyricsMonitorName = getSecondryDisplayName();
            }

            GetDisplayName(ref gf.LyricsMonitorName, ref gf.LM_Top, ref gf.LM_Left, ref gf.LM_Width, ref gf.LM_Height, (gf.DualMonitorSelectAutoOption == 0) ? gf.OutputMonitorName : "None", gf.LMAlwaysUseSecondaryMonitor);

            if (gf.LMSelectAutoOption == 1)
            {
                gf.LM_Top = gf.LMOption1Top;
                gf.LM_Left = gf.LMOption1Left;
                gf.LM_Width = gf.LMOption1Width;
                gf.LM_Height = gf.LMOption1Height;
            }
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
                    if (gf.isScreenWideMode)
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
                    if (gf.isScreenWideMode)
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
                if (gf.isScreenWideMode)
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
                if (gf.isScreenWideMode)
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
            List<string> monitors = new List<string>();
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
            List<string> monitors = new List<string>();
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
}
