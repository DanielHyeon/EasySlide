using Easislides.Util;
using System.IO;
using System.Windows.Forms;

namespace Easislides
{
    internal unsafe partial class gf
    {

		public static bool PowerpointPresent()
		{
			string text = Application.StartupPath + "\\Sys\\~temp.ppt";
			FileUtil.MakeDir(Application.StartupPath + "\\Sys");
			if (!File.Exists(text))
			{
				FileUtil.CreateNewFile(text);
			}
			text = '"' + text + '"';
			string lpResult = new string(' ', 260);
			if (FindExecutable(text, "", lpResult) > 32)
			{
				return true;
			}
			return false;
		}

    }
}
