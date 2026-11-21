using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Axicultus.Global;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

internal unsafe partial class Gf
{

		public static bool MusicFound(string MusicTitle1)
		{
			return MusicFound(MusicTitle1, "");
		}

		public static bool MusicFound(string MusicTitle1, string MusicTitle2)
		{
			return MusicFound(MusicTitle1, MusicTitle2, StoreDirPath: false);
		}

		public static bool MusicFound(string MusicTitle1, string MusicTitle2, bool StoreDirPath)
		{
			if (TotalMusicFiles < 0)
			{
				TotalMusicFiles = 0;
				// A rescan starts from an empty list: before, every rescan (each folder load) appended the
				// whole Media folder again, so the list and every MusicFound lookup grew with each load.
				MediaTitles.Clear();
				MediaByTitleAndExtension.Clear();
				MusicBuildContinue = true;
				MusicBuildStartTime = DateTime.Now;
				MusicBuildLapseTime = new TimeSpan(0L);
				BuildMusicFilesListArray(MediaDir, StoreDirPath);
			}
			if (TotalMusicFiles < 1)
			{
				return false;
			}

			return MediaTitles.Contains(MusicTitle1) || (!string.IsNullOrEmpty(MusicTitle2) && MediaTitles.Contains(MusicTitle2));
		}

		public static void LoadMusicExtArray()
		{
			MediaFileExtensions.Clear();
			LoadMusicExtArray("AudioExtensions.txt", MediaBackgroundStyle.Audio);
			LoadMusicExtArray("VideoExtensions.txt", MediaBackgroundStyle.Video);
		}

		public static void LoadMusicExtArray(string InFile, MediaBackgroundStyle InMediaType)
		{
			if (InMediaType != MediaBackgroundStyle.Video)
			{
				InMediaType = MediaBackgroundStyle.Audio;
			}
			string extensionsFilePath = RootAxicultusDir + "Admin\\Database\\" + InFile;
			if (!File.Exists(extensionsFilePath))
			{
				if (File.Exists(Application.StartupPath + "\\Sys\\" + InFile))
				{
					try
					{
						File.Copy(Application.StartupPath + "\\Sys\\" + InFile, extensionsFilePath);
					}
					catch
					{
						FileUtil.CreateNewFile(extensionsFilePath, FileUtil.FileContentsType.Ascii_Rtf);
					}
				}
				else
				{
					FileUtil.CreateNewFile(extensionsFilePath, FileUtil.FileContentsType.Ascii_Rtf);
				}
			}
			using StreamReader streamReader = File.OpenText(extensionsFilePath);
			string extensionLine = "";
			while ((extensionLine = streamReader.ReadLine()) != null)
			{
				extensionLine = DataUtil.TrimEnd(extensionLine);
				if (extensionLine != "" && MediaFileExtensions.Count < 3000 && ValidateMusicExt(ref extensionLine, ShowMessage: false))
				{
					MediaFileExtensions.Add(new MediaExtension
					{
						Extension = extensionLine.ToLower(),
						TypeName = InMediaType.ToString()
					});
				}
			}
		}

		public static void BuildMusicFilesListArray(string FolderPath, bool StoreDirPath)
		{
			if (FolderPath == "" || !MusicBuildContinue || (!Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\")))
			{
				return;
			}
			MusicBuildLapseTime = DateTime.Now.Subtract(MusicBuildStartTime);
			if (MusicBuildLapseTime.TotalSeconds > 10)
			{
				MusicBuildContinue = false;
				return;
			}
			for (int i = 0; i < MediaFileExtensions.Count; i++)
			{
				try
				{
					foreach (string mediaFilePath in Directory.EnumerateFiles(FolderPath, "*" + MediaFileExtensions[i].Extension))
					{
						string InFileName = mediaFilePath;
						string fileName = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
						string extension = MediaFileExtensions[i].Extension;
						int iLen = InFileName.Length - (fileName.Length + extension.Length);
						string dirPath = DataUtil.Left(InFileName, iLen);

						MediaTitles.Add(fileName);
						MediaByTitleAndExtension.TryAdd((fileName, extension), new MediaFileInfo
						{
							FileName = fileName,
							Extension = extension,
							DirectoryPath = dirPath
						});
						TotalMusicFiles++;
					}
				}
				catch
				{
				}
			}
			foreach (string child in GetMediaSubdirectories(FolderPath))
			{
				if (!MusicBuildContinue) break;
				BuildMusicFilesListArray(child, StoreDirPath);
			}
		}

		private static readonly EnumerationOptions MediaSubdirectoryOptions = new()
		{
			AttributesToSkip = FileAttributes.ReparsePoint,
			IgnoreInaccessible = true
		};

		private static string[] GetMediaSubdirectories(string folder)
		{
			try
			{
				var directories = Directory.GetDirectories(folder, "*", MediaSubdirectoryOptions);
				SingleArraySort(directories);
				return directories;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return []; }
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2)
		{
			string DirPath = "";
			string FileName = "";
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath)
		{
			string FileName = "";
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName, bool StoreDirPath)
		{
			if (StoreDirPath & (TotalMusicFiles < 1))
			{
				return "";
			}
			string candidateTitle = "";
			for (int i = 0; i <= 1; i++)
			{
				candidateTitle = ((i == 0) ? MusicTitle1 : MusicTitle2);
				for (int j = 0; j < MediaFileExtensions.Count; j++)
				{
					if (StoreDirPath)
					{
						if (MediaByTitleAndExtension.TryGetValue((candidateTitle, MediaFileExtensions[j].Extension), out var mediaFile))
						{
							DirPath = mediaFile.DirectoryPath;
							FileName = mediaFile.FileName + mediaFile.Extension;
							return DirPath + FileName;
						}
					}
					else
					{
						string mediaFileNameFromDir = GetMediaFileNameFromDir(MediaDir, MediaFileExtensions[j].Extension, candidateTitle, ref DirPath, ref FileName);
						if (mediaFileNameFromDir != "")
						{
							return mediaFileNameFromDir;
						}
					}
				}
			}
			return "";
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1)
		{
			string DirPath = "";
			string FileName = "";
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath)
		{
			string FileName = "";
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath, ref string FileName)
		{
			if ((FolderPath == "") | !Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\"))
			{
				return "";
			}
			if (!Path.EndsInDirectorySeparator(FolderPath)) FolderPath += Path.DirectorySeparatorChar;
			if (File.Exists(FolderPath + MusicTitle1 + MusicExtension))
			{
				DirPath = FolderPath;
				FileName = MusicTitle1 + MusicExtension;
				return DirPath + FileName;
			}
			foreach (string directory in GetMediaSubdirectories(FolderPath))
			{
				string mediaFileNameFromDir = GetMediaFileNameFromDir(directory, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
				if (mediaFileNameFromDir != "")
				{
					return mediaFileNameFromDir;
				}
			}
			return "";
		}

		public static void Play_Media(string title1, string title2)
		{
			string mediaFileName = GetMediaFileName(title1, title2);
			if (!RunProcess(mediaFileName))
			{
				MessageBox.Show("Sorry, cannot find any media file for '" + title1 + "'" + ((title2 != "") ? (" or '" + title2 + "'") : ""));
			}
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName, bool StoreDirPath)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1)
		{
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath)
		{
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath, ref string FileName)
		{
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}
}
