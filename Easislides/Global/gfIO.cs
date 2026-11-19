using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
	internal unsafe partial class Gf
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
					string baseName = Path.GetFileNameWithoutExtension(path);
					string dest = Path.Combine(InDir, baseName + InNewExt);
					int renameSuffix = 0;
					while (File.Exists(dest))
					{
						renameSuffix++;
						dest = Path.Combine(InDir, baseName + " - Renamed (" + renameSuffix + ")" + InNewExt);
					}
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
			string displayName = "";
			try
			{
				displayName = ((!KeepExt) ? Path.GetFileNameWithoutExtension(InFileName) : Path.GetFileName(InFileName));
				if (UpdateByRef)
				{
					InFileName = displayName;
				}
			}
			catch
			{
			}
			return displayName;
		}

		private static byte[] strokeCounts;

		internal static int GetStrokeCount(char character) => strokeCounts?[character] ?? 0;

		public static bool LoadUnicodeStrokeCount()
		{
			string InString = "";
			if (gfFileHelpers.LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				var counts = strokeCounts ?? new byte[char.MaxValue + 1];
				ReadStrokeCounts(InString, counts);
				strokeCounts = counts;
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		internal static void ReadStrokeCounts(ReadOnlySpan<char> data, Span<byte> counts)
		{
			// Each entry is one UTF-16 character followed by two decimal digits. The file has a two-character trailer.
			int recordsEnd = data.Length - 2;
			for (int offset = 0; offset <= recordsEnd - 3; offset += 3)
			{
				if (byte.TryParse(data.Slice(offset + 1, 2), out byte count) && count > 0)
					counts[data[offset]] = count;
			}
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
				int copySuffix = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				if (!Directory.Exists(CopyToFolder) && !FileUtil.MakeDir(CopyToFolder))
				{
					return "";
				}
				string destinationPath = Path.Combine(CopyToFolder, displayNameOnly + extension);
				while (File.Exists(destinationPath))
				{
					copySuffix++;
					destinationPath = Path.Combine(CopyToFolder, displayNameOnly + " - Copy (" + copySuffix + ")" + extension);
				}
				try
				{
					File.Copy(SourceFileName, destinationPath);
					return destinationPath;
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
				int copySuffix = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				if (!Directory.Exists(MoveToFolder) && !FileUtil.MakeDir(MoveToFolder))
				{
					return "";
				}
				string destinationPath = Path.Combine(MoveToFolder, displayNameOnly + extension);
				while (File.Exists(destinationPath))
				{
					copySuffix++;
					destinationPath = Path.Combine(MoveToFolder, displayNameOnly + " - Copy (" + copySuffix + ")" + extension);
				}
				try
				{
					File.Move(SourceFileName, destinationPath);
					return destinationPath;
				}
				catch
				{
				}
			}
			return "";
		}
	}
}
