using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OfficeLib
{
	/// <summary>
	/// First-slide pictures of the external presentations listed in the PowerPoint tab (thumbnail listing style).
	/// A picture is exported once per version of a presentation and reused by later listings.
	///
	/// - Name: {key}_{ticks}_{length}.jpg, where key hashes the export format and full path (upper-cased, UTF-8).
	///   Ticks is the presentation's LastWriteTimeUtc and length its size in bytes (hex).
	///   Only [0-9a-f_.] characters, about 70 characters long, so any file name (Korean, long paths) and two files
	///   with the same name in different folders map to distinct, safe names. An edited presentation gets a new
	///   name and is exported again.
	/// - Export: PowerPoint writes to a temporary name in the same folder ({name}.{random}.tmp.jpg); only a file
	///   that is a whole JPEG (starts with FF D8, ends with FF D9) and is no longer held open is then renamed to the
	///   final name. A failed, cancelled, truncated or still open export never appears under the final name; one
	///   that finishes late is renamed by the next attempt (AdoptPendingExport) instead of being exported again.
	/// - Last use: each listing sets LastAccessTimeUtc of its pictures to now (MarkUsed; LastWriteTimeUtc, the
	///   export time, is never changed). An entry's last use is the later of the two.
	/// - Prune (each listing): only files in the cache folder itself whose names follow these patterns are ever
	///   deleted: other versions of a listed presentation, pictures not used for MaxAgeDays, the least recently
	///   used pictures over MaxFiles (never one of the listing), and temporary files older than OrphanTempMinutes.
	/// The cache folder is not inside the application's temp folder (emptied at start-up and exit): it is kept
	/// across restarts and bounded by the prune alone.
	/// </summary>
	public static class ExternalPreviewCache
	{
		// Long enough for a folder used once a month to keep its pictures.
		public const int MaxAgeDays = 30;
		public const int MaxFiles = 2000;
		public const int OrphanTempMinutes = 10;

		private const int KeyHexLength = 32;
		private const string PreviewFormat = "slide-aspect/2";

		private static readonly Regex PreviewName = new Regex(
			"^(?<key>[0-9a-f]{32})_[0-9a-f]{1,16}_[0-9a-f]{1,16}\\.jpg$",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		private static readonly Regex TempName = new Regex(
			"^[0-9a-f]{32}_[0-9a-f]{1,16}_[0-9a-f]{1,16}\\.[0-9a-f]{8}\\.tmp\\.jpg$",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		/// <summary>
		/// The picture path for the current version of presentationPath in cacheFolder, or null when the folder is
		/// not an absolute path or the presentation cannot be read (missing, no access, invalid path).
		/// </summary>
		public static string GetPreviewPath(string cacheFolder, string presentationPath)
		{
			if (!IsUsableFolder(cacheFolder) || string.IsNullOrWhiteSpace(presentationPath))
			{
				return null;
			}
			try
			{
				string fullPath = Path.GetFullPath(presentationPath);
				FileInfo info = new FileInfo(fullPath);
				if (!info.Exists)
				{
					return null;
				}
				string name = KeyOf(PreviewFormat + "\n" + fullPath) + "_" + info.LastWriteTimeUtc.Ticks.ToString("x") + "_" + info.Length.ToString("x") + ".jpg";
				return Path.Combine(cacheFolder, name);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ExternalPreviewCache: no preview path for {presentationPath}: {ex.Message}");
				return null;
			}
		}

		/// <summary>A new temporary export name next to previewPath (same folder, so the final rename is a move within one volume).</summary>
		public static string GetTempExportPath(string previewPath)
		{
			string folder = Path.GetDirectoryName(previewPath) ?? "";
			string random = Guid.NewGuid().ToString("N").Substring(0, 8);
			return Path.Combine(folder, Path.GetFileNameWithoutExtension(previewPath) + "." + random + ".tmp.jpg");
		}

		/// <summary>
		/// Renames a finished export to its final name. Returns false when the export is missing, empty, not a JPEG
		/// or truncated (the temporary file is then deleted), or still held open by its writer or a scanner (the open or the
		/// rename fails; the file is left for AdoptPendingExport, or the prune).
		/// </summary>
		public static bool CommitExport(string tempPath, string previewPath)
		{
			try
			{
				if (!File.Exists(tempPath))
				{
					return false;
				}
				if (!IsWholeJpeg(tempPath))
				{
					Console.WriteLine($"ExternalPreviewCache: export is not a complete picture: {tempPath}");
					DiscardTemp(tempPath);
					return false;
				}
				File.Move(tempPath, previewPath, overwrite: true);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ExternalPreviewCache: could not keep export {tempPath} (yet): {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Commits an earlier export of previewPath that was not ready when it returned (the file appeared late, or
		/// was still held by a scanner). Returns true when previewPath now exists, so no new export is needed.
		/// </summary>
		public static bool AdoptPendingExport(string previewPath)
		{
			try
			{
				string folder = Path.GetDirectoryName(previewPath);
				if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
				{
					return false;
				}
				string pattern = Path.GetFileNameWithoutExtension(previewPath) + ".*.tmp.jpg";
				foreach (string tempPath in Directory.EnumerateFiles(folder, pattern, SearchOption.TopDirectoryOnly))
				{
					if (TempName.IsMatch(Path.GetFileName(tempPath)) && CommitExport(tempPath, previewPath))
					{
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ExternalPreviewCache: could not look for a pending export of {previewPath}: {ex.Message}");
			}
			return false;
		}

		/// <summary>Deletes a temporary export (a failed or cancelled one), ignoring errors; the prune removes what is left.</summary>
		public static void DiscardTemp(string tempPath)
		{
			if (string.IsNullOrEmpty(tempPath) || !TempName.IsMatch(Path.GetFileName(tempPath)))
			{
				return;
			}
			TryDelete(tempPath);
		}

		/// <summary>
		/// Deletes a cached picture that could not be loaded, so the next attempt exports it again. Does nothing
		/// unless previewPath is a cache picture name directly in cacheFolder.
		/// </summary>
		public static bool DiscardPreview(string cacheFolder, string previewPath)
		{
			if (!IsUsableFolder(cacheFolder) || string.IsNullOrEmpty(previewPath))
			{
				return false;
			}
			try
			{
				string folder = Path.GetDirectoryName(Path.GetFullPath(previewPath));
				if (!SameFolder(folder, cacheFolder) || !PreviewName.IsMatch(Path.GetFileName(previewPath)))
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
			return TryDelete(previewPath);
		}

		/// <summary>
		/// Prunes cacheFolder (top level only, cache file names only) and returns the number of files deleted.
		/// listedPreviewPaths: the pictures of the current listing, which are kept.
		/// </summary>
		public static int Prune(string cacheFolder, IEnumerable<string> listedPreviewPaths, DateTime nowUtc)
		{
			if (!IsUsableFolder(cacheFolder) || !Directory.Exists(cacheFolder))
			{
				return 0;
			}
			HashSet<string> keepNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> listedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string path in listedPreviewPaths ?? Enumerable.Empty<string>())
			{
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}
				string name = Path.GetFileName(path);
				Match match = PreviewName.Match(name);
				if (match.Success)
				{
					keepNames.Add(name);
					listedKeys.Add(match.Groups["key"].Value);
				}
			}

			int deleted = 0;
			List<FileInfo> candidates = new List<FileInfo>();
			int keptListed = 0;
			FileInfo[] files;
			try
			{
				files = new DirectoryInfo(cacheFolder).GetFiles("*", SearchOption.TopDirectoryOnly);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ExternalPreviewCache: could not list {cacheFolder}: {ex.Message}");
				return 0;
			}
			DateTime oldestKept = nowUtc.AddDays(-MaxAgeDays);
			DateTime oldestTemp = nowUtc.AddMinutes(-OrphanTempMinutes);
			foreach (FileInfo file in files)
			{
				string name = file.Name;
				if (TempName.IsMatch(name))
				{
					// An export in progress is younger than this; older ones were cancelled or failed.
					if (file.LastWriteTimeUtc < oldestTemp && TryDelete(file.FullName))
					{
						deleted++;
					}
					continue;
				}
				Match match = PreviewName.Match(name);
				if (!match.Success)
				{
					continue; // not ours: never touched
				}
				if (keepNames.Contains(name))
				{
					MarkUsed(file, nowUtc);
					keptListed++;
					continue;
				}
				if (listedKeys.Contains(match.Groups["key"].Value) || LastUseUtc(file) < oldestKept)
				{
					// Another version of a listed presentation, or not used for a long time.
					if (TryDelete(file.FullName))
					{
						deleted++;
					}
					continue;
				}
				candidates.Add(file);
			}

			int excess = keptListed + candidates.Count - MaxFiles;
			if (excess > 0)
			{
				foreach (FileInfo file in candidates.OrderBy(LastUseUtc).Take(excess))
				{
					if (TryDelete(file.FullName))
					{
						deleted++;
					}
				}
			}
			return deleted;
		}

		internal static string KeyOf(string fullPath)
		{
			byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(fullPath.ToUpperInvariant()));
			return Convert.ToHexString(hash, 0, KeyHexLength / 2).ToLowerInvariant();
		}

		/// <summary>The later of the export time (LastWriteTimeUtc) and the last listing (LastAccessTimeUtc, see MarkUsed).</summary>
		public static DateTime LastUseUtc(FileInfo file)
		{
			DateTime written = file.LastWriteTimeUtc;
			DateTime accessed = file.LastAccessTimeUtc;
			return accessed > written ? accessed : written;
		}

		// Records a use of a listed picture in its LastAccessTimeUtc. LastWriteTimeUtc is not changed: ImageCanvas
		// compares it to tell a rebuilt file from its in-memory copy.
		private static void MarkUsed(FileInfo file, DateTime nowUtc)
		{
			try
			{
				if (file.LastAccessTimeUtc < nowUtc)
				{
					File.SetLastAccessTimeUtc(file.FullName, nowUtc);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ExternalPreviewCache: could not mark {file.Name} as used: {ex.Message}");
			}
		}

		// Largest run of trailing zero bytes accepted after the end marker. JPEG encoders (PowerPoint's GDI+/WIC
		// export included) end the file with FF D9; zero padding after it is tolerated in case a writer pads to a
		// block size. A truncated export ends inside the entropy-coded data, so FF D9 does not come right before
		// its last non-zero byte.
		private const int MaxTrailingPadding = 4096;

		/// <summary>True when the file starts with the JPEG start marker (FF D8) and ends with the end marker (FF D9),
		/// followed by at most MaxTrailingPadding zero bytes.</summary>
		internal static bool IsWholeJpeg(string path)
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			long length = stream.Length;
			if (length < 4 || stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8)
			{
				return false;
			}
			int tailLength = (int)Math.Min(length - 2, MaxTrailingPadding + 2);
			byte[] tail = new byte[tailLength];
			stream.Seek(length - tailLength, SeekOrigin.Begin);
			int read = 0;
			while (read < tailLength)
			{
				int n = stream.Read(tail, read, tailLength - read);
				if (n <= 0)
				{
					return false;
				}
				read += n;
			}
			int end = tailLength - 1;
			while (end >= 0 && tail[end] == 0)
			{
				end--;
			}
			return end >= 1 && tail[end] == 0xD9 && tail[end - 1] == 0xFF;
		}

		private static bool IsUsableFolder(string folder)
		{
			return !string.IsNullOrWhiteSpace(folder) && Path.IsPathFullyQualified(folder);
		}

		private static bool SameFolder(string a, string b)
		{
			string left = Path.TrimEndingDirectorySeparator(Path.GetFullPath(a));
			string right = Path.TrimEndingDirectorySeparator(Path.GetFullPath(b));
			return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
		}

		private static bool TryDelete(string path)
		{
			try
			{
				File.Delete(path);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ExternalPreviewCache: could not delete {path}: {ex.Message}");
				return false;
			}
		}
	}
}
