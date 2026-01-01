using System;
using System.Collections.Generic;
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
		private const int ExecutableBufferSize = 260;
		private const int MaxPowerpointSequence = 49;
		private const int MaxSlideCount = 1000;
		private const int SongVerseInitCount = 10;
		private const int MainWindowWaitCount = 80;
		private const int MainWindowWaitDelayMs = 100;
		private const int WindowInitialSearchDelayMs = 1500;
		private const int WindowRetryDelayMs = 800;
		private const int WindowRetryCount = 5;
		private const int WindowHandleFallbackDelayMs = 1000;
		private const int WindowHandleRetryDelayMs = 500;
		private const int WindowSettleDelayMs = 300;
		private const int ForegroundDelayMs = 200;
		private const int WindowBoundsSlack = 100;
		private const int WindowTitleBufferSize = 256;
		private const float PowerpointShowWindowScale = 0.75f;

		private static readonly string[] MediaPlayerWindowKeywords =
		{
			"windows media player",
			"media player",
			"vlc media player",
			"vlc",
			"movies & tv",
			"films & tv"
		};

		private static readonly Dictionary<Keys, Keys> KeyRemapTable = new Dictionary<Keys, Keys>
		{
			{ Keys.Home, Keys.Left },
			{ Keys.Prior, Keys.Up },
			{ Keys.Next, Keys.Down },
			{ Keys.End, Keys.Right },
			{ Keys.Left, Keys.Home },
			{ Keys.Up, Keys.Prior },
			{ Keys.Down, Keys.Next },
			{ Keys.Right, Keys.End }
		};
		private static readonly Regex InvalidFileNameCharsRegex = new Regex(
			string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))),
			RegexOptions.Compiled);

		public static bool PowerpointPresent()
		{
			string text = Application.StartupPath + "\\Sys\\~temp.ppt";
			FileUtil.MakeDir(Application.StartupPath + "\\Sys");
			if (!File.Exists(text))
			{
				FileUtil.CreateNewFile(text);
			}
			text = '"' + text + '"';
			string lpResult = new string(' ', ExecutableBufferSize);
			if (FindExecutable(text, "", lpResult) > 32)
			{
				return true;
			}
			return false;
		}

		public static string SetPowerpointPreviewPrefix(SongSettings InItem, bool useTitlePrefix = false)
		{
			if (InItem.Type != "P")
			{
				return "";
			}

			if (useTitlePrefix)
			{
				string prefixItemName = InvalidFileNameCharsRegex.Replace(InItem.Title, "");

				if (InItem.OutputStyleScreen)
				{
					OUTPPFullPath = OUTPPPrefix + "$" + prefixItemName + "$";
					return OUTPPFullPath;
				}
				PREPPFullPath = PREPPPrefix + "$" + prefixItemName + "$";
				return PREPPFullPath;
			}

			if (InItem.OutputStyleScreen)
			{
				if ((OUTPPSequence < 0) | (OUTPPSequence >= MaxPowerpointSequence))
				{
					OUTPPSequence = 0;
				}
				OUTPPSequence++;
				OUTPPFullPath = OUTPPPrefix + OUTPPSequence;
				return OUTPPFullPath;
			}
			if ((PREPPSequence < 0) | (PREPPSequence >= MaxPowerpointSequence))
			{
				PREPPSequence = 0;
			}
			PREPPSequence++;
			PREPPFullPath = PREPPPrefix + PREPPSequence;
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
			for (int i = 1; i <= MaxSlideCount; i++)
			{
				InItem.Slide[i, 0] = -1;
			}
			Array.Fill(InItem.SongVerses, 0, 0, Math.Min(SongVerseInitCount, InItem.SongVerses.Length));
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
				float scalef = PowerpointShowWindowScale;


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
			if (KeyBoardOption != 1)
			{
				return;
			}

			if (KeyRemapTable.TryGetValue(InKey, out Keys mappedKey))
			{
				InKey = mappedKey;
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

				StringBuilder sb = new StringBuilder(WindowTitleBufferSize);
				GetWindowText(hWnd, sb, WindowTitleBufferSize);
				string windowTitle = sb.ToString().ToLower();


				if (!string.IsNullOrEmpty(windowTitle))
				{
					string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameLower);

					if (windowTitle.Contains(fileNameWithoutExt) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[0]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[1]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[2]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[3]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[4]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[5]))
					{
						foundWindow = hWnd;
						return false; // 李얠�??�㈃ 以묐??
					}
				}

				return true; // ?�꾩??寃??
			}, IntPtr.Zero);

			return foundWindow;
		}

		private static IntPtr FindMediaPlayerWindowWithRetries(string fileName, int initialDelayMs, int retryDelayMs, int retryCount)
		{
			if (initialDelayMs > 0)
			{
				Thread.Sleep(initialDelayMs);
			}

			IntPtr handle = FindRecentMediaPlayerWindow(fileName);
			for (int retry = 0; retry < retryCount && handle == IntPtr.Zero; retry++)
			{
				Thread.Sleep(retryDelayMs);
				handle = FindRecentMediaPlayerWindow(fileName);
			}

			return handle;
		}

		private static IntPtr WaitForMainWindowHandle(Process process, int maxWaitCount, int waitDelayMs)
		{
			int waitCount = 0;
			while (process.MainWindowHandle == IntPtr.Zero && waitCount < maxWaitCount)
			{
				Thread.Sleep(waitDelayMs);
				process.Refresh();
				waitCount++;
			}

			return process.MainWindowHandle;
		}

		public static bool RunProcessOnMonitor(string InProcessString, string monitorName)
		{
			try
			{

				Screen targetScreen = null;
				foreach (Screen screen in Screen.AllScreens)
				{
					if (screen.DeviceName == monitorName)
					{
						targetScreen = screen;
						break;
					}
				}

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
	
					return RunProcess(InProcessString);
				}


				ProcessStartInfo psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process process = Process.Start(psi);

				if (process == null)
				{
					IntPtr foundHandle = FindMediaPlayerWindowWithRetries(
						InProcessString,
						WindowInitialSearchDelayMs,
						WindowRetryDelayMs,
						WindowRetryCount);

					if (foundHandle != IntPtr.Zero)
					{

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


				IntPtr handle = WaitForMainWindowHandle(process, MainWindowWaitCount, MainWindowWaitDelayMs);


				if (handle == IntPtr.Zero)
				{
					handle = FindMediaPlayerWindowWithRetries(
						InProcessString,
						WindowHandleFallbackDelayMs,
						WindowHandleRetryDelayMs,
						WindowRetryCount);


					if (handle == IntPtr.Zero)
					{

						return true; 
					}
				}

				Rectangle bounds = targetScreen.Bounds;


				Thread.Sleep(WindowSettleDelayMs);


				SetWindowPos(handle, HWND_TOP,
					bounds.X, bounds.Y,
					bounds.Width, bounds.Height,
					SWP_SHOWWINDOW);


				SetForegroundWindow(handle);


				Thread.Sleep(ForegroundDelayMs);
				ShowWindow(handle, SW_MAXIMIZE);


				Thread.Sleep(WindowSettleDelayMs);
				RECT windowRect;
				if (GetWindowRect(handle, out windowRect))
				{

					if (windowRect.Left < bounds.Left - WindowBoundsSlack ||
						windowRect.Top < bounds.Top - WindowBoundsSlack ||
						windowRect.Left > bounds.Right + WindowBoundsSlack)
					{
						
						SetWindowPos(handle, HWND_TOP,
							bounds.X, bounds.Y,
							bounds.Width, bounds.Height,
							SWP_SHOWWINDOW);
						Thread.Sleep(MainWindowWaitDelayMs);
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
