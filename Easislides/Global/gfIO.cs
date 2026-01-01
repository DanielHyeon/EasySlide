using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
	internal unsafe partial class gf
	{
		public static void RenameExtensions(string InDir, string InOldExt, string InNewExt)
		{
			if (string.IsNullOrWhiteSpace(InDir) || !Directory.Exists(InDir))
			{
				return;
			}

			foreach (string path in Directory.EnumerateFiles(InDir, "*" + InOldExt))
			{
				try
				{
					string dest = Path.Combine(InDir, Path.GetFileNameWithoutExtension(path) + InNewExt);
					File.Move(path, dest);
				}
				catch (IOException ex)
				{
					Debug.WriteLine(ex.Message);
				}
				catch (UnauthorizedAccessException ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}

		public static string GetDisplayNameOnly(ref string InFileName, bool UpdateByRef)
		{
			return GetDisplayNameOnly(ref InFileName, UpdateByRef, KeepExt: false);
		}

		public static string GetDisplayNameOnly(ref string InFileName, bool UpdateByRef, bool KeepExt)
		{
			if ((InFileName == null) | (InFileName == ""))
			{
				return "";
			}
			string text = "";
			try
			{
				text = ((!KeepExt) ? Path.GetFileNameWithoutExtension(InFileName) : Path.GetFileName(InFileName));
				if (UpdateByRef)
				{
					InFileName = text;
				}
			}
			catch
			{
			}
			return text;
		}

		public static bool LoadUnicodeStrokeCount1()
		{
			string InString = "";
			if (gfFileHelpers.LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				int i = 1;
				for (int num = InString.Length - 2; i <= num - 2; i += 3)
				{
					int num2 = DataUtil.ObjToInt(InString.Substring(i, 2));
					if (num2 > 0)
					{
						int num3 = InString[i - 1];
						if (num3 < 0)
						{
							num3 += 65536;
						}
						if (num3 >= 0)
						{
							StrokeCount[num3] = num2;
						}
					}
				}
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		public static bool LoadUnicodeStrokeCount()
		{
			string InString = "";
			if (gfFileHelpers.LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				ReadOnlySpan<char> roString = InString.AsSpan();
				int i = 1;
				for (int num = roString.Length - 2; i <= num - 2; i += 3)
				{
					if (!int.TryParse(roString.Slice(i, 2), out int num2))
					{
						continue;
					}
					if (num2 > 0)
					{
						int num3 = roString[i - 1];
						if (num3 < 0)
						{
							num3 += 65536;
						}
						if (num3 >= 0)
						{
							StrokeCount[num3] = num2;
						}
					}
				}
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		public static string ExtractDocTextContents(string InFileName)
		{
			string result = "";
			if (File.Exists(InFileName))
			{
				switch (Path.GetExtension(InFileName).ToLower())
				{
					case ".doc":
					case ".docx":
						result = GetOfficeDocContents(InFileName);
						break;
					case ".txt":
						result = gfFileHelpers.LoadTextFile(InFileName);
						break;
				}
			}
			return result;
		}

		public static string MakeTitleValidFileName(string InString)
		{
			char[] array = new char[9]
			{
				'\\',
				'/',
				':',
				'*',
				'?',
				'"',
				'<',
				'>',
				'|'
			};
			for (int i = 0; i < array.Length; i++)
			{
				InString = InString.Replace(array[i], '_');
			}
			InString = DataUtil.Left(InString, 255);
			return InString;
		}

		public static bool RecycleBin(string FullFileName)
		{
			try
			{
				SHFILEOPSTRUCT lpFileOp = default(SHFILEOPSTRUCT);
				lpFileOp.hwnd = IntPtr.Zero;
				lpFileOp.wFunc = 3u;
				lpFileOp.fFlags = 80;
				lpFileOp.pFrom = FullFileName + '\0' + '\0';
				lpFileOp.fAnyOperationsAborted = false;
				lpFileOp.hNameMappings = IntPtr.Zero;
				SHFileOperation(ref lpFileOp);
				return !File.Exists(FullFileName);
			}
			catch
			{
				return false;
			}
		}

		public static string CopyExternalFile(string SourceFileName, string CopyToFolder)
		{
			if (File.Exists(SourceFileName))
			{
				int num = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				string text = Path.GetDirectoryName(SourceFileName) + "\\";
				if (!Directory.Exists(CopyToFolder) && !FileUtil.MakeDir(CopyToFolder))
				{
					return "";
				}
				string text2 = CopyToFolder + displayNameOnly + extension;
				while (File.Exists(text2))
				{
					num++;
					text2 = CopyToFolder + displayNameOnly + " - Copy (" + num + ")" + extension;
				}
				try
				{
					File.Copy(SourceFileName, text2);
					return text2;
				}
				catch
				{
				}
			}
			return "";
		}

		public static string MoveExternalFile(string SourceFileName, string MoveToFolder)
		{
			if (File.Exists(SourceFileName))
			{
				int num = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				string text = Path.GetDirectoryName(SourceFileName) + "\\";
				if (!Directory.Exists(MoveToFolder) && !FileUtil.MakeDir(MoveToFolder))
				{
					return "";
				}
				string text2 = MoveToFolder + displayNameOnly + extension;
				while (File.Exists(text2))
				{
					num++;
					text2 = MoveToFolder + displayNameOnly + " - Copy (" + num + ")" + extension;
				}
				try
				{
					File.Move(SourceFileName, text2);
					return text2;
				}
				catch
				{
				}
			}
			return "";
		}
	}
}
