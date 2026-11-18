using System;
using System.Globalization;
using System.IO;

namespace OfficeLib
{
	/// <summary>
	/// Records which presentation version a set of exported slide images ({prefix}1.jpg .. {prefix}N.jpg) was
	/// built from, so the images are reused only when they are complete and belong to that exact file.
	///
	/// - The record ({prefix}build.txt) holds the source's full path, LastWriteTimeUtc ticks, length and the
	///   slide count. It is deleted before an export starts and written (temp file, then rename) only after every
	///   slide was exported: an interrupted export is never taken as built.
	/// - The source's time and length are read before the presentation is opened: a save during the export
	///   leaves the record older than the file, so the next check builds again.
	/// - A record is valid only for the same path, time and length, and when every slide image exists.
	/// - Numbers are written and parsed with the invariant culture (Korean and English settings alike).
	/// </summary>
	public static class SlideImageManifest
	{
		private const string RecordName = "build.txt";
		private const string FormatTag = "EasiSlidesSlideImages/2";
		private const int PrefixKeyLength = 12;

		/// <summary>A source file's identity read before it is opened; null when it cannot be read.</summary>
		public sealed class SourceVersion
		{
			public string FullPath { get; }
			public long LastWriteTicks { get; }
			public long Length { get; }

			internal SourceVersion(string fullPath, long lastWriteTicks, long length)
			{
				FullPath = fullPath;
				LastWriteTicks = lastWriteTicks;
				Length = length;
			}
		}

		/// <summary>
		/// A short, file-name-safe key of a presentation's full path (hex). Put in the image prefix, it keeps the
		/// images of two items with the same title but different files apart.
		/// </summary>
		public static string PrefixKey(string presentationPath)
		{
			if (string.IsNullOrWhiteSpace(presentationPath))
			{
				return "0";
			}
			string fullPath;
			try
			{
				fullPath = Path.GetFullPath(presentationPath);
			}
			catch (Exception)
			{
				fullPath = presentationPath;
			}
			return ExternalPreviewCache.KeyOf(fullPath).Substring(0, PrefixKeyLength);
		}

		public static SourceVersion ReadSource(string presentationPath)
		{
			try
			{
				FileInfo info = new FileInfo(presentationPath);
				if (!info.Exists)
				{
					return null;
				}
				return new SourceVersion(info.FullName, info.LastWriteTimeUtc.Ticks, info.Length);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SlideImageManifest: cannot read '{presentationPath}': {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// True when the images under prefix are a complete export of the current version of presentationPath;
		/// slideCount is then their number. Never opens PowerPoint, never throws.
		/// </summary>
		public static bool TryMatch(string prefix, string presentationPath, out int slideCount)
		{
			slideCount = 0;
			try
			{
				SourceVersion source = ReadSource(presentationPath);
				string record = RecordPath(prefix);
				if (source == null || record == null || !File.Exists(record))
				{
					return false;
				}
				string[] lines = File.ReadAllLines(record);
				if (lines.Length < 5 || lines[0] != FormatTag
					|| !string.Equals(lines[1], source.FullPath, StringComparison.OrdinalIgnoreCase)
					|| !long.TryParse(lines[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks) || ticks != source.LastWriteTicks
					|| !long.TryParse(lines[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out long length) || length != source.Length
					|| !int.TryParse(lines[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count <= 0)
				{
					return false;
				}
				for (int i = 1; i <= count; i++)
				{
					if (!File.Exists(ImagePath(prefix, i)))
					{
						return false;
					}
				}
				slideCount = count;
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SlideImageManifest: cannot check '{prefix}': {ex.Message}");
				return false;
			}
		}

		/// <summary>Removes the record (before an export starts). Returns false when it could not be removed.</summary>
		public static bool Invalidate(string prefix)
		{
			try
			{
				string record = RecordPath(prefix);
				if (record != null && File.Exists(record))
				{
					File.Delete(record);
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SlideImageManifest: cannot remove the record of '{prefix}': {ex.Message}");
				return false;
			}
		}

		/// <summary>Writes the record after every slide image under prefix was exported.</summary>
		public static void Commit(string prefix, SourceVersion source, int slideCount)
		{
			string record = RecordPath(prefix);
			if (record == null || source == null || slideCount <= 0)
			{
				return;
			}
			string temp = record + "." + Guid.NewGuid().ToString("N").Substring(0, 8) + ".tmp";
			try
			{
				File.WriteAllLines(temp, new[]
				{
					FormatTag,
					source.FullPath,
					source.LastWriteTicks.ToString(CultureInfo.InvariantCulture),
					source.Length.ToString(CultureInfo.InvariantCulture),
					slideCount.ToString(CultureInfo.InvariantCulture)
				});
				File.Move(temp, record, overwrite: true);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SlideImageManifest: cannot write the record of '{prefix}': {ex.Message}");
				try
				{
					File.Delete(temp);
				}
				catch
				{
				}
			}
		}

		public static string ImagePath(string prefix, int slideNumber)
		{
			return prefix + slideNumber.ToString(CultureInfo.InvariantCulture) + ".jpg";
		}

		private static string RecordPath(string prefix)
		{
			return string.IsNullOrEmpty(prefix) ? null : prefix + RecordName;
		}
	}
}
