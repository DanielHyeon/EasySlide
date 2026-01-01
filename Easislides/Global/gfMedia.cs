using Easislides.Module;
using Easislides.Util;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Easislides
{
    internal unsafe partial class gf
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
				MusicBuildContinue = true;
				MusicBuildStartTime = DateTime.Now;
				MusicBuildLapseTime = new TimeSpan(0L);
				BuildMusicFilesListArray(MediaDir, StoreDirPath);
			}
			if (TotalMusicFiles < 1)
			{
				return false;
			}

			return MediaFilesList.Any(f =>
				f.FileName.Equals(MusicTitle1, StringComparison.OrdinalIgnoreCase) ||
				(!string.IsNullOrEmpty(MusicTitle2) && f.FileName.Equals(MusicTitle2, StringComparison.OrdinalIgnoreCase)));
		}

		public static void LoadMusicExtArray()
		{
			TotalMediaFileExt = 0;
			LoadMusicExtArray("AudioExtensions.txt", MediaBackgroundStyle.Audio);
			LoadMusicExtArray("VideoExtensions.txt", MediaBackgroundStyle.Video);
		}

		public static void LoadMusicExtArray(string InFile, MediaBackgroundStyle InMediaType)
		{
			if (InMediaType != MediaBackgroundStyle.Video)
			{
				InMediaType = MediaBackgroundStyle.Audio;
			}
			string text = RootEasiSlidesDir + "Admin\\Database\\" + InFile;
			if (!File.Exists(text))
			{
				if (File.Exists(Application.StartupPath + "\\Sys\\" + InFile))
				{
					try
					{
						File.Copy(Application.StartupPath + "\\Sys\\" + InFile, text);
					}
					catch
					{
						FileUtil.CreateNewFile(text, FileUtil.FileContentsType.Ascii_Rtf);
					}
				}
				else
				{
					FileUtil.CreateNewFile(text, FileUtil.FileContentsType.Ascii_Rtf);
				}
			}
			using StreamReader streamReader = File.OpenText(text);
			string text2 = "";
			while ((text2 = streamReader.ReadLine()) != null)
			{
				text2 = DataUtil.TrimEnd(text2);
				if (text2 != "" && TotalMediaFileExt < 3000 && ValidateMusicExt(ref text2, ShowMessage: false))
				{
					MediaFileExtension[TotalMediaFileExt, 0] = text2.ToLower();
					MediaFileExtension[TotalMediaFileExt, 1] = InMediaType.ToString();
					TotalMediaFileExt++;
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
			if (MusicBuildLapseTime.Seconds > 10)
			{
				MusicBuildContinue = false;
				return;
			}
			string[] array;
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				try
				{
					string[] files = Directory.GetFiles(FolderPath, "*" + MediaFileExtension[i, 0]);
					array = files;
					foreach (string text in array)
					{
						string InFileName = text;
						string fileName = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
						string extension = MediaFileExtension[i, 0];
						int iLen = InFileName.Length - (fileName.Length + extension.Length);
						string dirPath = DataUtil.Left(InFileName, iLen);

						MediaFilesList.Add(new MediaFileInfo
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
			if (!FolderPath.EndsWith(@"\Media\"))
			{
				gf.MediaDir = @"C:\EasiSlides\Media\";
				FolderPath = gf.MediaDir;
			}

			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			array = directories;
			foreach (string str in array)
			{
				BuildMusicFilesListArray(str + "\\", StoreDirPath);
			}
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
			string text = "";
			for (int i = 0; i <= 1; i++)
			{
				text = ((i == 0) ? MusicTitle1 : MusicTitle2);
				for (int j = 0; j < TotalMediaFileExt; j++)
				{
					if (StoreDirPath)
					{
						var mediaFile = MediaFilesList.FirstOrDefault(f =>
							f.FileName == text && f.Extension == MediaFileExtension[j, 0]);

						if (mediaFile != null)
						{
							DirPath = mediaFile.DirectoryPath;
							FileName = mediaFile.FileName + mediaFile.Extension;
							return DirPath + FileName;
						}
					}
					else
					{
						string mediaFileNameFromDir = GetMediaFileNameFromDir(MediaDir, MediaFileExtension[j, 0], text, ref DirPath, ref FileName);
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
			if (File.Exists(FolderPath + MusicTitle1 + MusicExtension))
			{
				DirPath = FolderPath;
				FileName = MusicTitle1 + MusicExtension;
				return DirPath + FileName;
			}
			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			string[] array = directories;
			foreach (string str in array)
			{
				string mediaFileNameFromDir = GetMediaFileNameFromDir(str + "\\", MusicExtension, MusicTitle1, ref DirPath, ref FileName);
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
}
