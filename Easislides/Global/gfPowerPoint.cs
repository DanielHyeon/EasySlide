using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;
using OfficeLib;

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

		public static string SetPowerpointPreviewPrefix(SongSettings InItem)
		{
			if (InItem.Type != "P")
			{
				return "";
			}
			if (InItem.OutputStyleScreen)
			{
				if ((OUTPPSequence < 0) | (OUTPPSequence >= 49))
				{
					OUTPPSequence = 0;
				}
				OUTPPSequence++;
				OUTPPFullPath = OUTPPPrefix + OUTPPSequence;
				return OUTPPFullPath;
			}
			if ((PREPPSequence < 0) | (PREPPSequence >= 49))
			{
				PREPPSequence = 0;
			}
			PREPPSequence++;
			PREPPFullPath = PREPPPrefix + PREPPSequence;
			return PREPPFullPath;
		}

		public static string SetPowerpointPreviewPrefix1(SongSettings InItem)
		{
			if (InItem.Type != "P")
			{
				return "";
			}
			Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
			string prefixItemName = regex.Replace(InItem.Title, "");

			if (InItem.OutputStyleScreen)
			{
				OUTPPFullPath = OUTPPPrefix + "$" + prefixItemName + "$";
				return OUTPPFullPath;
			}
			PREPPFullPath = PREPPPrefix + "$" + prefixItemName + "$";
			return PREPPFullPath;
		}

		public static void MinimizePowerPointWindows(ref PowerPoint InPPT)
		{
			InPPT.ResSetAllShowWindows();
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide)
		{
			return RunPowerpointSong(ref InItem, ref InPPT, StartingSlide, ShowResult: false);
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide, bool ShowResult)
		{
			for (int i = 1; i <= 1000; i++)
			{
				InItem.Slide[i, 0] = -1;
			}
			for (int i = 0; i <= 9; i++)
			{
				InItem.SongVerses[i] = 0;
			}
			InPPT.displayName = OutputMonitorName;

			string text = InPPT.Run(InItem.Path, ref PowerpointList, ref TotalPowerpointItems);
			if (StartingSlide < 2)
			{
				InPPT.First();
				InItem.CurSlide = 1;
			}
			else if (StartingSlide > InPPT.Count())
			{
				InPPT.Last();
				InItem.CurSlide = InPPT.Count();
			}
			else if (InPPT.Count() > 0)
			{
				InPPT.GotoSlide(StartingSlide);
				InItem.CurSlide = StartingSlide;
			}

			if (!ShowLiveCam && gf.DualMonitorSelectAutoOption == 1)
			{
				float scalef = 0.75f;
				// ??�쎌???�쎌?????�쎌?????�쎌???�쎈??紐⑤???�쏙?? ??�쎌???�쎄�???��?�???�쎌??

				if (DualMonitorMode)
				{
					InPPT.SetShowWindow((float)LS_Left * scalef, (float)LS_Top * scalef, (float)LS_Width * scalef, (float)LS_Height * scalef);
				}
				else
				{
					InPPT.SetShowWindow(LS_Left, LS_Top, (float)LS_Width * scalef, (float)LS_Height * scalef);
				}
			}

			InPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
			return InPPT.Count();
		}

		public static void ClearUpPowerpointWindows()
		{
			string text = LivePP.ClearUpPowerpointWindows(ref PowerpointList, ref TotalPowerpointItems);
			if (text == "")
			{
				LivePP.QuitPowerPointApp(LivePP.prePowerPointApp);
			}
		}

		public static void ReMapKeyBoard(ref Keys InKey)
		{
			if (InKey == Keys.Home)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Left;
				}
			}
			else if (InKey == Keys.Prior)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Up;
				}
			}
			else if (InKey == Keys.Next)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Down;
				}
			}
			else if (InKey == Keys.End)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Right;
				}
			}
			else if (InKey == Keys.Left)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Home;
				}
			}
			else if (InKey == Keys.Up)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Prior;
				}
			}
			else if (InKey == Keys.Down)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Next;
				}
			}
			else if (InKey == Keys.Right && KeyBoardOption == 1)
			{
				InKey = Keys.End;
			}
		}

		public static int ImplementSlideMovement(ref int InCurSlide, int InCurMaxSlide, Keys InKey, int InSlideNo)
		{
			switch (InKey)
			{
				case Keys.Left:
					InCurSlide = 1;
					break;
				case Keys.Up:
					InCurSlide = ((InCurSlide <= 2) ? 1 : (InCurSlide - 1));
					break;
				case Keys.Down:
					InCurSlide = ((InCurSlide < InCurMaxSlide) ? (InCurSlide + 1) : InCurMaxSlide);
					break;
				case Keys.Right:
					InCurSlide = InCurMaxSlide;
					break;
				case Keys.None:
					InCurSlide = InSlideNo;
					break;
			}
			return InCurSlide;
		}

		public static Keys ReMapKeyDirectionToPowerpoint(KeyDirection InDirection)
		{
			switch (InDirection)
			{
				case KeyDirection.FirstOne:
					return Keys.Left;
				case KeyDirection.PrevOne:
					return Keys.Up;
				case KeyDirection.NextOne:
					return Keys.Down;
				case KeyDirection.LastOne:
					return Keys.Right;
				case KeyDirection.SpaceOne:
					return Keys.Space;
				default:
					return Keys.F5;
			}
		}

		public static bool RunProcess(string InProcessString)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process.Start(psi);

				//.net core??�쎌�????�쎈????媛숈????�쎈????�쎌?????�쏙?? ??�쎌????�쎌???�쎌???�쏙?? ??�쎌????�쎌??
				//Process process = Process.Start(InProcessString);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			return true;
		}

		private static IntPtr FindRecentMediaPlayerWindow(string fileName)
		{
			IntPtr foundWindow = IntPtr.Zero;
			string fileNameLower = Path.GetFileName(fileName).ToLower();

			EnumWindows((hWnd, lParam) =>
			{
				if (!IsWindowVisible(hWnd))
					return true;

				StringBuilder sb = new StringBuilder(256);
				GetWindowText(hWnd, sb, 256);
				string windowTitle = sb.ToString().ToLower();

				// �???�ぉ?????�� ??��????�??�뼱 ??�뒗吏 ?뺤씤
				if (!string.IsNullOrEmpty(windowTitle))
				{
					string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameLower);
					// ???�� ??��??뺤옣????�씠 寃???�?�� ??�컲?곸씤 誘몃�?????��??�뼱 ??��?寃??
					if (windowTitle.Contains(fileNameWithoutExt) ||
						windowTitle.Contains("windows media player") ||
						windowTitle.Contains("media player") ||
						windowTitle.Contains("vlc media player") ||
						windowTitle.Contains("vlc") ||
						windowTitle.Contains("movies & tv") ||
						windowTitle.Contains("films & tv"))
					{
						foundWindow = hWnd;
						return false; // 李얠�??�㈃ 以묐??
					}
				}

				return true; // ?�꾩??寃??
			}, IntPtr.Zero);

			return foundWindow;
		}

		public static bool RunProcessOnMonitor(string InProcessString, string monitorName)
		{
			try
			{
				// ????紐⑤???李얘�?
				Screen targetScreen = null;
				foreach (Screen screen in Screen.AllScreens)
				{
					if (screen.DeviceName == monitorName)
					{
						targetScreen = screen;
						break;
					}
				}

				// Primary 紐⑤??�? Secondary�?蹂寃�?�??濡쒖�?(DisplayInfo.cs?? ??�씪)
				if (monitorName == "Primary")
				{
					string secondaryName = DisplayInfo.getSecondryDisplayName();
					foreach (Screen screen in Screen.AllScreens)
					{
						if (screen.DeviceName == secondaryName)
						{
							targetScreen = screen;
							break;
						}
					}
				}

				if (targetScreen == null)
				{
					// 紐⑤??�? 李얠? 紐삵�?��?湲곕????�?
					return RunProcess(InProcessString);
				}

				// ?꾨줈?몄뒪 ??�옉
				ProcessStartInfo psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process process = Process.Start(psi);

				// UseShellExecute = true????process媛 null??????�쓬
				if (process == null)
				{
					// ?꾨줈?몄뒪 媛앹�?????�?誘몃�?????��??�뼱????�뻾??
					// �???�ぉ??�줈 寃??�빐??李얠�????
					Thread.Sleep(1500); // ???��??�뼱媛 ??�옉?????��吏 ??�?
					IntPtr foundHandle = FindRecentMediaPlayerWindow(InProcessString);

					// �?李얠?�硫?????�??????(理쒕? 5�? ??4??
					if (foundHandle == IntPtr.Zero)
					{
						for (int retry = 0; retry < 5 && foundHandle == IntPtr.Zero; retry++)
						{
							Thread.Sleep(800);
							foundHandle = FindRecentMediaPlayerWindow(InProcessString);
						}
					}

					if (foundHandle != IntPtr.Zero)
					{
						// Output Monitor�???��?�?理쒕???
						Rectangle targetBounds = targetScreen.Bounds;
						SetWindowPos(foundHandle, HWND_TOP,
							targetBounds.X, targetBounds.Y,
							targetBounds.Width, targetBounds.Height,
							SWP_SHOWWINDOW);
						SetForegroundWindow(foundHandle);
						ShowWindow(foundHandle, SW_MAXIMIZE);
						return true;
					}
					else
					{
						Console.WriteLine("RunProcessOnMonitor: process媛 null??��?李쎈�?李얠??????�뒿??�떎.");
						return false;
					}
				}

				// ?꾨줈?몄뒪 李쎌????�꽦?????��吏 ??�?(理쒕? 8?�덈�?利앷?)
				int maxWait = 80; // 80 * 100ms = 8??
				int waitCount = 0;

				while (process.MainWindowHandle == IntPtr.Zero && waitCount < maxWait)
				{
					Thread.Sleep(100);
					process.Refresh();
					waitCount++;
				}

				IntPtr handle = process.MainWindowHandle;

				// �??몃뱾????? 紐삵�?寃쎌??�???�ぉ??�줈 ?????
				if (handle == IntPtr.Zero)
				{
					// ?�붽? ??�???�???�ぉ??�줈 寃????�룄
					Thread.Sleep(1000);
					handle = FindRecentMediaPlayerWindow(InProcessString);

					// 洹몃???�?李얠?�硫?????�??????
					if (handle == IntPtr.Zero)
					{
						for (int retry = 0; retry < 5 && handle == IntPtr.Zero; retry++)
						{
							Thread.Sleep(500);
							handle = FindRecentMediaPlayerWindow(InProcessString);
						}
					}

					// 理쒖�?곸쑝濡쒕�?李쎌??李얠? 紐삵�?寃쎌??
					if (handle == IntPtr.Zero)
					{
						Console.WriteLine("RunProcessOnMonitor: �??몃뱾??李얠??????�뒿??�떎. ?꾩튂 ??�뼱 ?�덇?");
						return true; // ?꾨줈?몄뒪????�뻾??
					}
				}

				// 李쎌??????紐⑤??곕줈 ??��?
				Rectangle bounds = targetScreen.Bounds;

				// 李쎌???꾩쟾??濡쒕�?????��吏 ?좎떆 ??�?
				Thread.Sleep(300);

				// �???��?�???�?議곗??
				SetWindowPos(handle, HWND_TOP,
					bounds.X, bounds.Y,
					bounds.Width, bounds.Height,
					SWP_SHOWWINDOW);

				// 李쎌???????�슫??�줈 媛?몄삤�?
				SetForegroundWindow(handle);

				// ?좎떆 ??�???理쒕???
				Thread.Sleep(200);
				ShowWindow(handle, SW_MAXIMIZE);

				// 理쒖�??뺤씤: 李쎌?????�???��??�뒗吏 ?뺤씤??��??????
				Thread.Sleep(300);
				RECT windowRect;
				if (GetWindowRect(handle, out windowRect))
				{
					// 李쎌??紐⑺�?紐⑤????곸뿭????�쑝�???�떆 ??��???�룄
					if (windowRect.Left < bounds.Left - 100 ||
						windowRect.Top < bounds.Top - 100 ||
						windowRect.Left > bounds.Right + 100)
					{
						Console.WriteLine($"RunProcessOnMonitor: �??꾩튂 ?????(?꾩옱: {windowRect.Left},{windowRect.Top} -> 紐⑺�? {bounds.X},{bounds.Y})");
						SetWindowPos(handle, HWND_TOP,
							bounds.X, bounds.Y,
							bounds.Width, bounds.Height,
							SWP_SHOWWINDOW);
						Thread.Sleep(100);
						ShowWindow(handle, SW_MAXIMIZE);
					}
				}

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("RunProcessOnMonitor failed: " + e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
		}

    }
}
