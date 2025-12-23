using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Easislides.Util
{
    internal class ScreenUtil
    {
        public static float GetWindowsScaling(string screenName)
        {
            Screen selectedScreen = null;
            Screen[] allScreens = Screen.AllScreens;
            foreach (Screen screen in allScreens)
            {
                if (screen.DeviceName == screenName)
                {
                    selectedScreen = screen;
                    break;
                }
            }

            return (100 * selectedScreen.WorkingArea.Width / selectedScreen.Bounds.Width);
        }
    }
}
