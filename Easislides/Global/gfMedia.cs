using System;
using System.Linq;

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

    }
}
