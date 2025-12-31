using System;

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
			for (int i = 0; i < TotalMusicFiles; i++)
			{
				if (MediaFilesList[i, 0].ToLower() == MusicTitle1.ToLower())
				{
					return true;
				}
				if (MusicTitle2 != "" && MediaFilesList[i, 0].ToLower() == MusicTitle2.ToLower())
				{
					return true;
				}
			}
			return false;
		}

    }
}
