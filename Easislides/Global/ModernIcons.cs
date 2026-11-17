// Icons for the modernised main form (openspec change modernize-main-form-ui, design D18).
//
// Source: Fluent UI System Icons, https://github.com/microsoft/fluentui-system-icons
// Commit:  41089846ad80c5749df505aef35a68712c1561b9 (IconSourceCommit; the generated data block
//          at the end of this file lists every original file name).
// Licence: MIT License, Copyright (c) 2020 Microsoft Corporation.
//          The full licence text is in THIRD-PARTY-NOTICES.md at the repository root.
//
// Exception: the icons in the "Own drawings" section at the end are original drawings made
// for EasiSlides, not Fluent icons.
//
// The app ships no image or font files for these icons. The SVG path data of the icons in use
// is copied below (generated block; the original file names are listed there) and drawn at the
// exact pixel size needed, in a palette colour. The original colour (#212121) is not used.
//
// Threading: UI thread only. The parser is pure, but the bitmap cache is not synchronised.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;

namespace Easislides
{
	internal enum ModernIconStyle
	{
		Regular,
		Filled
	}

	/// <summary>
	/// Draws Fluent UI System Icons at an exact pixel size. UI thread only. No public method throws:
	/// when an icon cannot be drawn the result is null (the caller keeps the current picture) and
	/// the reason is written once to the debug log.
	/// <para>
	/// Ownership. <see cref="Get"/> returns the cache's bitmap: the cache owns it, a caller must not
	/// dispose it, and <see cref="ClearCache"/> may only run when no control still shows it (after the
	/// controls are disposed, e.g. when the main form closes). A bitmap a caller must own (anything
	/// that outlives the cache, or that the caller disposes itself) comes from <see cref="Render"/>,
	/// which returns a new bitmap on every call and never a cached one. An ImageList does NOT copy on
	/// Add: it keeps the image and reads it when its handle is created and whenever the handle is
	/// recreated, so an image added to an ImageList must live as long as the list (Render, or the
	/// caller's own bitmap; not a cached one that ClearCache may dispose first).
	/// </para>
	/// <para>
	/// Showing the bitmap unscaled and with its alpha: for an ImageList set ColorDepth = Depth32Bit
	/// and ImageSize = the rendered size before adding images (changing either later clears the
	/// list); for a ToolStrip set ImageScalingSize = the rendered size, or set the item's
	/// ImageScaling = None. Otherwise the picture is resampled (blurred) or reduced to fewer colours.
	/// </para>
	/// </summary>
	internal static class ModernIcons
	{
		// One SVG file of the source repository: icon name, style, design size (viewBox is
		// 0 0 size size), fill rule and the path strings in document order.
		private sealed class IconSource
		{
			public readonly string Name;
			public readonly ModernIconStyle Style;
			public readonly int Size;
			public readonly FillMode Fill;
			public readonly string[] Paths;

			public IconSource(string name, ModernIconStyle style, int size, FillMode fill, params string[] paths)
			{
				Name = name;
				Style = style;
				Size = size;
				Fill = fill;
				Paths = paths;
			}
		}

		private readonly struct CacheKey : IEquatable<CacheKey>
		{
			public readonly string Name;
			public readonly ModernIconStyle Style;
			public readonly int PixelSize;
			public readonly int Argb;

			public CacheKey(string name, ModernIconStyle style, int pixelSize, int argb)
			{
				Name = name;
				Style = style;
				PixelSize = pixelSize;
				Argb = argb;
			}

			public bool Equals(CacheKey other) =>
				PixelSize == other.PixelSize && Argb == other.Argb && Style == other.Style
				&& string.Equals(Name, other.Name, StringComparison.Ordinal);

			public override bool Equals(object obj) => obj is CacheKey other && Equals(other);

			public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(Name ?? ""), Style, PixelSize, Argb);
		}

		private static Dictionary<(string, ModernIconStyle), List<IconSource>> index;
		private static readonly Dictionary<CacheKey, Bitmap> cache = new Dictionary<CacheKey, Bitmap>();
		private static readonly HashSet<string> loggedFailures = new HashSet<string>(StringComparer.Ordinal);

		/// <summary>True when path data for this icon and style is present (it may still fail to parse). Never throws.</summary>
		internal static bool Has(string name, ModernIconStyle style)
		{
			try
			{
				return name != null && GetIndex().ContainsKey((name, style));
			}
			catch (Exception ex)
			{
				LogOnce("index", "ModernIcons: icon table not usable: " + ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Returns the cache's bitmap of the icon, or null when it cannot be drawn (logged once).
		/// Never throws. See the ownership rules in the class summary: the cache owns the bitmap.
		/// A failure that cannot change (unknown icon, unusable size, bad path data) is cached as null;
		/// a GDI+ failure while drawing is not cached, so a later call tries again.
		/// </summary>
		internal static Bitmap Get(string name, ModernIconStyle style, int pixelSize, Color color)
		{
			try
			{
				var key = new CacheKey(name, style, pixelSize, color.ToArgb());
				if (cache.TryGetValue(key, out Bitmap bitmap))
				{
					return bitmap;
				}
				bitmap = RenderCore(name, style, pixelSize, color, out bool permanentFailure);
				if (bitmap != null || permanentFailure)
				{
					cache[key] = bitmap;
				}
				return bitmap;
			}
			catch (Exception ex)
			{
				LogOnce(name + "/" + style + "/" + pixelSize + "/get", "ModernIcons: icon '" + name + "' (" + style + ", " + pixelSize + " px) not drawn: " + ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Disposes every cached bitmap. Call it only when no control shows any of them any more
		/// (after the controls are disposed). Never throws.
		/// </summary>
		internal static void ClearCache()
		{
			foreach (Bitmap bitmap in cache.Values)
			{
				try
				{
					bitmap?.Dispose();
				}
				catch (Exception ex)
				{
					LogOnce("clear", "ModernIcons: disposing a cached icon failed: " + ex.Message);
				}
			}
			cache.Clear();
		}

		/// <summary>
		/// Draws the icon into a NEW 32 bpp premultiplied bitmap of pixelSize x pixelSize with a
		/// transparent background, never the cached one; the caller owns and disposes it.
		/// The source drawn is the largest design size not larger than pixelSize (the smallest one
		/// when all are larger), scaled to pixelSize.
		/// Returns null (logged once per icon, style, size and reason) when the icon cannot be drawn.
		/// Never throws.
		/// </summary>
		internal static Bitmap Render(string name, ModernIconStyle style, int pixelSize, Color color)
		{
			try
			{
				return RenderCore(name, style, pixelSize, color, out _);
			}
			catch (Exception ex)
			{
				LogOnce(name + "/" + style + "/" + pixelSize + "/render", "ModernIcons: icon '" + name + "' (" + style + ", " + pixelSize + " px) not drawn: " + ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Draws several icons on top of each other, each in its own colour (e.g. a coloured badge square and
		/// the white letter on it), into a NEW bitmap the caller owns. Null (logged once) when any layer cannot
		/// be drawn. Never throws.
		/// </summary>
		internal static Bitmap RenderComposite(ModernIconStyle style, int pixelSize, params (string Name, Color Colour)[] layers)
		{
			Bitmap result = null;
			try
			{
				result = new Bitmap(pixelSize, pixelSize, PixelFormat.Format32bppPArgb);
				using (Graphics g = Graphics.FromImage(result))
				{
					g.Clear(Color.Transparent);
					foreach (var (name, colour) in layers)
					{
						using (Bitmap layer = Render(name, style, pixelSize, colour))
						{
							if (layer == null)
							{
								result.Dispose();
								return null;
							}
							// Same size, pixel on pixel, alpha-blended over the layers below.
							g.InterpolationMode = InterpolationMode.NearestNeighbor;
							g.PixelOffsetMode = PixelOffsetMode.Half;
							g.DrawImage(layer, new Rectangle(0, 0, pixelSize, pixelSize), 0, 0, pixelSize, pixelSize, GraphicsUnit.Pixel);
						}
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				result?.Dispose();
				LogOnce("composite/" + pixelSize, "ModernIcons: composite icon (" + pixelSize + " px) not drawn: " + ex.Message);
				return null;
			}
		}

		// permanentFailure: true when the same call can never succeed (unknown icon, unusable size,
		// bad path data); false for a success or for a GDI+ failure that may not happen again.
		private static Bitmap RenderCore(string name, ModernIconStyle style, int pixelSize, Color color, out bool permanentFailure)
		{
			permanentFailure = true;
			string what = "ModernIcons: icon '" + name + "' (" + style + ", " + pixelSize + " px) ";
			string keyBase = name + "/" + style + "/" + pixelSize;
			if (pixelSize < 1 || pixelSize > 1024)
			{
				LogOnce(keyBase + "/size", what + "asked for at an unusable size");
				return null;
			}
			IconSource source = PickSource(name, style, pixelSize);
			if (source == null)
			{
				LogOnce(name + "/" + style + "/missing", "ModernIcons: no path data for icon '" + name + "' (" + style + ")");
				return null;
			}
			var paths = new List<GraphicsPath>(source.Paths.Length);
			Bitmap bitmap = null;
			try
			{
				foreach (string d in source.Paths)
				{
					GraphicsPath path = ParsePath(d, source.Fill, out string error);
					if (path == null)
					{
						LogOnce(name + "/" + style + "/source" + source.Size,
							"ModernIcons: icon '" + name + "' (" + style + ", source " + source.Size + ") not drawn: " + error);
						return null;
					}
					paths.Add(path);
				}
				permanentFailure = false;
				bitmap = new Bitmap(pixelSize, pixelSize, PixelFormat.Format32bppPArgb);
				using (Graphics g = Graphics.FromImage(bitmap))
				using (var brush = new SolidBrush(color))
				{
					g.Clear(Color.Transparent);
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.CompositingQuality = CompositingQuality.HighQuality;
					// SVG puts pixel (0,0) on the square 0..1. GDI+ does the same only with the half
					// pixel offset; without it every edge on a whole coordinate would be smeared over
					// two pixels and a 16 px source drawn at 16 px would look blurred.
					g.PixelOffsetMode = PixelOffsetMode.Half;
					float scale = pixelSize / (float)source.Size;
					g.ScaleTransform(scale, scale);
					foreach (GraphicsPath path in paths)
					{
						g.FillPath(brush, path);
					}
				}
				Bitmap result = bitmap;
				bitmap = null;
				return result;
			}
			catch (Exception ex)
			{
				// GDI+ failure (out of memory, invalid parameter, ...): not permanent.
				LogOnce(keyBase + "/draw", what + "not drawn: " + ex.GetType().Name + ": " + ex.Message);
				return null;
			}
			finally
			{
				bitmap?.Dispose();
				foreach (GraphicsPath path in paths)
				{
					path.Dispose();
				}
			}
		}

		private static IconSource PickSource(string name, ModernIconStyle style, int pixelSize)
		{
			if (name == null || !GetIndex().TryGetValue((name, style), out List<IconSource> sizes))
			{
				return null;
			}
			// sizes is sorted ascending.
			IconSource best = null;
			foreach (IconSource s in sizes)
			{
				if (s.Size <= pixelSize)
				{
					best = s;
				}
			}
			return best ?? sizes[0];
		}

		private static Dictionary<(string, ModernIconStyle), List<IconSource>> GetIndex()
		{
			if (index != null)
			{
				return index;
			}
			var map = new Dictionary<(string, ModernIconStyle), List<IconSource>>();
			foreach (IconSource s in Sources.Concat(OwnSources))
			{
				if (!map.TryGetValue((s.Name, s.Style), out List<IconSource> list))
				{
					list = new List<IconSource>();
					map.Add((s.Name, s.Style), list);
				}
				list.Add(s);
			}
			foreach (List<IconSource> list in map.Values)
			{
				list.Sort((a, b) => a.Size.CompareTo(b.Size));
			}
			index = map;
			return index;
		}

		private static void LogOnce(string key, string message)
		{
			if (loggedFailures.Add(key))
			{
				FrmMain.WriteDebugLog(message);
			}
		}

		// ---------------------------------------------------------------------------------------
		// SVG path data parser (SVG 1.1 path grammar): M L H V C S Q T A Z and the lowercase
		// relative forms, implicit repeated commands (extra pairs after M/m are L/l), compact
		// numbers ("1.5.5", "1-2", "1e-3"), compact arc flags ("a1 1 0 01 2 2").
		// Quadratic segments and arcs are converted to cubic Beziers.
		// Any error makes the whole path fail: null is returned with a reason, nothing is guessed.
		// ---------------------------------------------------------------------------------------

		internal static GraphicsPath ParsePath(string d, FillMode fill, out string error)
		{
			error = null;
			if (string.IsNullOrWhiteSpace(d))
			{
				error = "empty path data";
				return null;
			}
			try
			{
				var builder = new PathBuilder();
				var reader = new PathReader(d);
				if (!Parse(reader, builder, out error))
				{
					return null;
				}
				if (builder.Points.Count == 0)
				{
					error = "path data draws nothing";
					return null;
				}
				return new GraphicsPath(builder.Points.ToArray(), builder.Types.ToArray(), fill);
			}
			catch (Exception ex)
			{
				error = "path data rejected: " + ex.Message;
				return null;
			}
		}

		private static bool Parse(PathReader r, PathBuilder b, out string error)
		{
			error = null;
			float cx = 0, cy = 0;          // current point
			float sx = 0, sy = 0;          // start of the current subpath
			float lcx = 0, lcy = 0;        // last control point, for S/s and T/t
			char lastCommand = '\0';
			char command = '\0';
			bool first = true;

			r.SkipWhite();
			while (!r.AtEnd)
			{
				if (r.PeekCommand(out char c))
				{
					r.Advance();
					command = c;
				}
				else if (command == '\0')
				{
					error = "path data does not start with a command at position " + r.Position;
					return false;
				}
				else if (command == 'Z' || command == 'z')
				{
					error = "unexpected data after Z at position " + r.Position;
					return false;
				}
				// else: implicit repetition of the previous command.

				if (first && command != 'M' && command != 'm')
				{
					error = "path data does not start with M";
					return false;
				}
				first = false;
				bool rel = char.IsLower(command);
				char upper = char.ToUpperInvariant(command);
				switch (upper)
				{
					case 'M':
					{
						if (!r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x += cx; y += cy; }
						b.MoveTo(x, y);
						cx = sx = x; cy = sy = y;
						// Further coordinate pairs after a moveto are implicit lineto commands.
						command = rel ? 'l' : 'L';
						lastCommand = 'M';
						break;
					}
					case 'L':
					{
						if (!r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x += cx; y += cy; }
						b.LineTo(x, y, sx, sy);
						cx = x; cy = y;
						lastCommand = 'L';
						break;
					}
					case 'H':
					{
						if (!r.ReadNumber(out float x))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x += cx; }
						b.LineTo(x, cy, sx, sy);
						cx = x;
						lastCommand = 'L';
						break;
					}
					case 'V':
					{
						if (!r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { y += cy; }
						b.LineTo(cx, y, sx, sy);
						cy = y;
						lastCommand = 'L';
						break;
					}
					case 'C':
					{
						if (!r.ReadNumber(out float x1) || !r.ReadNumber(out float y1)
							|| !r.ReadNumber(out float x2) || !r.ReadNumber(out float y2)
							|| !r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x1 += cx; y1 += cy; x2 += cx; y2 += cy; x += cx; y += cy; }
						b.CubicTo(x1, y1, x2, y2, x, y, sx, sy);
						lcx = x2; lcy = y2;
						cx = x; cy = y;
						lastCommand = 'C';
						break;
					}
					case 'S':
					{
						if (!r.ReadNumber(out float x2) || !r.ReadNumber(out float y2)
							|| !r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x2 += cx; y2 += cy; x += cx; y += cy; }
						float x1 = cx, y1 = cy;
						if (lastCommand == 'C')
						{
							x1 = 2 * cx - lcx;
							y1 = 2 * cy - lcy;
						}
						b.CubicTo(x1, y1, x2, y2, x, y, sx, sy);
						lcx = x2; lcy = y2;
						cx = x; cy = y;
						lastCommand = 'C';
						break;
					}
					case 'Q':
					{
						if (!r.ReadNumber(out float qx) || !r.ReadNumber(out float qy)
							|| !r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { qx += cx; qy += cy; x += cx; y += cy; }
						b.QuadTo(cx, cy, qx, qy, x, y, sx, sy);
						lcx = qx; lcy = qy;
						cx = x; cy = y;
						lastCommand = 'Q';
						break;
					}
					case 'T':
					{
						if (!r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x += cx; y += cy; }
						float qx = cx, qy = cy;
						if (lastCommand == 'Q')
						{
							qx = 2 * cx - lcx;
							qy = 2 * cy - lcy;
						}
						b.QuadTo(cx, cy, qx, qy, x, y, sx, sy);
						lcx = qx; lcy = qy;
						cx = x; cy = y;
						lastCommand = 'Q';
						break;
					}
					case 'A':
					{
						if (!r.ReadNumber(out float rx) || !r.ReadNumber(out float ry) || !r.ReadNumber(out float rotation)
							|| !r.ReadFlag(out bool largeArc) || !r.ReadFlag(out bool sweep)
							|| !r.ReadNumber(out float x) || !r.ReadNumber(out float y))
						{
							error = Bad(r, command);
							return false;
						}
						if (rel) { x += cx; y += cy; }
						b.ArcTo(cx, cy, rx, ry, rotation, largeArc, sweep, x, y, sx, sy);
						cx = x; cy = y;
						lastCommand = 'A';
						break;
					}
					case 'Z':
					{
						b.Close();
						cx = sx; cy = sy;
						lastCommand = 'Z';
						break;
					}
					default:
						error = "unknown path command '" + command + "' at position " + (r.Position - 1);
						return false;
				}
				r.SkipWhite();
			}
			b.Finish();
			return true;
		}

		private static string Bad(PathReader r, char command)
		{
			return "missing or malformed numbers for '" + command + "' at position " + r.Position;
		}

		private sealed class PathReader
		{
			private readonly string s;
			private int pos;

			public PathReader(string text)
			{
				s = text;
			}

			// True right after a number or flag was read: only then may one comma follow.
			private bool afterNumber;

			public bool AtEnd => pos >= s.Length;

			public int Position => pos;

			// Consumes the command letter at the current position.
			public void Advance()
			{
				pos++;
				afterNumber = false;
			}

			// Between commands only white space is allowed; a comma is accepted solely between two
			// numbers (see SkipArgumentSeparator), so a stray comma is left for the caller to reject.
			public void SkipWhite()
			{
				while (pos < s.Length && char.IsWhiteSpace(s[pos]))
				{
					pos++;
				}
			}

			// White space, plus at most one comma when the previous token was a number. A comma
			// after a command letter or at the start fails (returns false); a comma that is not
			// followed by a number fails in the caller, which then finds no number.
			private bool SkipArgumentSeparator()
			{
				SkipWhite();
				if (pos < s.Length && s[pos] == ',')
				{
					if (!afterNumber)
					{
						return false;
					}
					pos++;
					SkipWhite();
				}
				return true;
			}

			// A letter at the current position. 'e'/'E' never start a command here because numbers
			// are read whole, exponent included, before the next command is looked for.
			public bool PeekCommand(out char c)
			{
				c = pos < s.Length ? s[pos] : '\0';
				return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
			}

			public bool ReadNumber(out float value)
			{
				value = 0;
				if (!SkipArgumentSeparator())
				{
					return false;
				}
				int start = pos;
				if (pos < s.Length && (s[pos] == '+' || s[pos] == '-'))
				{
					pos++;
				}
				int digits = 0;
				while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9')
				{
					pos++;
					digits++;
				}
				if (pos < s.Length && s[pos] == '.')
				{
					pos++;
					while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9')
					{
						pos++;
						digits++;
					}
				}
				if (digits == 0)
				{
					pos = start;
					return false;
				}
				if (pos < s.Length && (s[pos] == 'e' || s[pos] == 'E'))
				{
					int expStart = pos;
					pos++;
					if (pos < s.Length && (s[pos] == '+' || s[pos] == '-'))
					{
						pos++;
					}
					int expDigits = 0;
					while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9')
					{
						pos++;
						expDigits++;
					}
					if (expDigits == 0)
					{
						// "e" without digits is not an exponent; leave it for the caller to reject.
						pos = expStart;
					}
				}
				if (!double.TryParse(s.AsSpan(start, pos - start), NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
					|| double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > 1e6)
				{
					pos = start;
					return false;
				}
				value = (float)v;
				afterNumber = true;
				return true;
			}

			// Arc flags are a single '0' or '1' and may be written without separators ("a1 1 0 01 2 2").
			public bool ReadFlag(out bool value)
			{
				value = false;
				if (!SkipArgumentSeparator())
				{
					return false;
				}
				if (pos < s.Length && (s[pos] == '0' || s[pos] == '1'))
				{
					value = s[pos] == '1';
					pos++;
					afterNumber = true;
					return true;
				}
				return false;
			}
		}

		// Collects GDI+ path points and types directly, so that subpaths are exactly as in the SVG
		// (GraphicsPath.AddLine and friends would join a new figure to the previous one).
		private sealed class PathBuilder
		{
			public readonly List<PointF> Points = new List<PointF>();
			public readonly List<byte> Types = new List<byte>();
			private int figureStart = -1;
			private bool closed;

			private const byte TypeStart = 0;
			private const byte TypeLine = 1;
			private const byte TypeBezier = 3;
			private const byte TypeClose = 0x80;

			public void MoveTo(float x, float y)
			{
				EndFigure();
				figureStart = Points.Count;
				closed = false;
				Points.Add(new PointF(x, y));
				Types.Add(TypeStart);
			}

			// After Z, drawing continues from the start of the closed subpath in a new subpath.
			private void EnsureFigure(float sx, float sy)
			{
				if (figureStart < 0 || closed)
				{
					MoveTo(sx, sy);
				}
			}

			public void LineTo(float x, float y, float sx, float sy)
			{
				EnsureFigure(sx, sy);
				Points.Add(new PointF(x, y));
				Types.Add(TypeLine);
			}

			public void CubicTo(float x1, float y1, float x2, float y2, float x, float y, float sx, float sy)
			{
				EnsureFigure(sx, sy);
				Points.Add(new PointF(x1, y1));
				Types.Add(TypeBezier);
				Points.Add(new PointF(x2, y2));
				Types.Add(TypeBezier);
				Points.Add(new PointF(x, y));
				Types.Add(TypeBezier);
			}

			public void QuadTo(float x0, float y0, float qx, float qy, float x, float y, float sx, float sy)
			{
				// Exact degree elevation of a quadratic Bezier.
				CubicTo(x0 + 2f / 3f * (qx - x0), y0 + 2f / 3f * (qy - y0),
					x + 2f / 3f * (qx - x), y + 2f / 3f * (qy - y),
					x, y, sx, sy);
			}

			// SVG elliptical arc (implementation notes F.6.5 / F.6.6 of SVG 1.1), as cubic Beziers of at
			// most 90 degrees each.
			public void ArcTo(float x0, float y0, float rxIn, float ryIn, float rotationDeg, bool largeArc, bool sweep,
				float x, float y, float sx, float sy)
			{
				if (x0 == x && y0 == y)
				{
					return; // F.6.2: identical end points draw nothing.
				}
				double rx = Math.Abs(rxIn), ry = Math.Abs(ryIn);
				if (rx == 0 || ry == 0)
				{
					LineTo(x, y, sx, sy); // F.6.2: a zero radius is a straight line.
					return;
				}
				double phi = rotationDeg * Math.PI / 180.0;
				double cosPhi = Math.Cos(phi), sinPhi = Math.Sin(phi);
				double dx2 = (x0 - x) / 2.0, dy2 = (y0 - y) / 2.0;
				double x1p = cosPhi * dx2 + sinPhi * dy2;
				double y1p = -sinPhi * dx2 + cosPhi * dy2;
				double lambda = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry);
				if (lambda > 1)
				{
					double root = Math.Sqrt(lambda);
					rx *= root;
					ry *= root;
				}
				double num = rx * rx * ry * ry - rx * rx * y1p * y1p - ry * ry * x1p * x1p;
				double den = rx * rx * y1p * y1p + ry * ry * x1p * x1p;
				double coef = den == 0 ? 0 : Math.Sqrt(Math.Max(0, num / den));
				if (largeArc == sweep)
				{
					coef = -coef;
				}
				double cxp = coef * (rx * y1p / ry);
				double cyp = coef * -(ry * x1p / rx);
				double cx = cosPhi * cxp - sinPhi * cyp + (x0 + x) / 2.0;
				double cy = sinPhi * cxp + cosPhi * cyp + (y0 + y) / 2.0;
				double theta1 = Angle(1, 0, (x1p - cxp) / rx, (y1p - cyp) / ry);
				double delta = Angle((x1p - cxp) / rx, (y1p - cyp) / ry, (-x1p - cxp) / rx, (-y1p - cyp) / ry);
				if (!sweep && delta > 0)
				{
					delta -= 2 * Math.PI;
				}
				else if (sweep && delta < 0)
				{
					delta += 2 * Math.PI;
				}
				int segments = Math.Max(1, (int)Math.Ceiling(Math.Abs(delta) / (Math.PI / 2) - 1e-9));
				double step = delta / segments;
				double k = 4.0 / 3.0 * Math.Tan(step / 4);
				double t = theta1;
				for (int i = 0; i < segments; i++)
				{
					double cos1 = Math.Cos(t), sin1 = Math.Sin(t);
					double cos2 = Math.Cos(t + step), sin2 = Math.Sin(t + step);
					// Unit circle control points, then scale, rotate and move.
					double p1x = cos1 - k * sin1, p1y = sin1 + k * cos1;
					double p2x = cos2 + k * sin2, p2y = sin2 - k * cos2;
					PointF c1 = Map(p1x, p1y, rx, ry, cosPhi, sinPhi, cx, cy);
					PointF c2 = Map(p2x, p2y, rx, ry, cosPhi, sinPhi, cx, cy);
					PointF e = i == segments - 1 ? new PointF(x, y) : Map(cos2, sin2, rx, ry, cosPhi, sinPhi, cx, cy);
					CubicTo(c1.X, c1.Y, c2.X, c2.Y, e.X, e.Y, sx, sy);
					t += step;
				}
			}

			private static PointF Map(double ux, double uy, double rx, double ry, double cosPhi, double sinPhi, double cx, double cy)
			{
				double px = ux * rx, py = uy * ry;
				return new PointF((float)(cosPhi * px - sinPhi * py + cx), (float)(sinPhi * px + cosPhi * py + cy));
			}

			private static double Angle(double ux, double uy, double vx, double vy)
			{
				double a = Math.Atan2(ux * vy - uy * vx, ux * vx + uy * vy);
				return a;
			}

			public void Close()
			{
				if (figureStart >= 0 && !closed)
				{
					Types[Types.Count - 1] = (byte)(Types[Types.Count - 1] | TypeClose);
					closed = true;
				}
			}

			public void Finish()
			{
				EndFigure();
			}

			// A figure that is only a start point draws nothing and would confuse GDI+: drop it.
			private void EndFigure()
			{
				if (figureStart >= 0 && Points.Count - figureStart == 1)
				{
					Points.RemoveAt(figureStart);
					Types.RemoveAt(figureStart);
				}
				figureStart = -1;
			}
		}

		// BEGIN GENERATED ICON DATA
		// Generated from the SVG files of https://github.com/microsoft/fluentui-system-icons
		// at commit 41089846ad80c5749df505aef35a68712c1561b9 (assets/<Folder>/SVG/<file>).
		// Only these canonical files are used. Some text icons (text_bold, text_italic, text_underline, ...)
		// also have language variants under assets/<Folder>/<language>/SVG/ with the same file name and
		// other letters (e.g. Cyrillic, Hangul); those variants are not used.
		// MIT License, Copyright (c) 2020 Microsoft Corporation (full text in THIRD-PARTY-NOTICES.md).
		// Path data is copied unchanged; the fill colour of the files (#212121) is not used.
		// Original files (257):
		//   ic_fluent_document_add_16_regular.svg
		//   ic_fluent_document_add_20_regular.svg
		//   ic_fluent_document_add_24_regular.svg
		//   ic_fluent_document_add_28_regular.svg
		//   ic_fluent_edit_16_regular.svg
		//   ic_fluent_edit_20_regular.svg
		//   ic_fluent_edit_24_regular.svg
		//   ic_fluent_edit_28_regular.svg
		//   ic_fluent_edit_32_regular.svg
		//   ic_fluent_copy_16_regular.svg
		//   ic_fluent_copy_20_regular.svg
		//   ic_fluent_copy_24_regular.svg
		//   ic_fluent_copy_28_regular.svg
		//   ic_fluent_copy_32_regular.svg
		//   ic_fluent_folder_arrow_right_16_regular.svg
		//   ic_fluent_folder_arrow_right_20_regular.svg
		//   ic_fluent_folder_arrow_right_24_regular.svg
		//   ic_fluent_folder_arrow_right_28_regular.svg
		//   ic_fluent_folder_arrow_right_32_regular.svg
		//   ic_fluent_delete_16_regular.svg
		//   ic_fluent_delete_20_regular.svg
		//   ic_fluent_delete_24_regular.svg
		//   ic_fluent_delete_28_regular.svg
		//   ic_fluent_delete_32_regular.svg
		//   ic_fluent_filmstrip_play_16_regular.svg
		//   ic_fluent_filmstrip_play_20_regular.svg
		//   ic_fluent_filmstrip_play_24_regular.svg
		//   ic_fluent_filmstrip_play_32_regular.svg
		//   ic_fluent_arrow_clockwise_16_regular.svg
		//   ic_fluent_arrow_clockwise_20_regular.svg
		//   ic_fluent_arrow_clockwise_24_regular.svg
		//   ic_fluent_arrow_clockwise_28_regular.svg
		//   ic_fluent_arrow_clockwise_32_regular.svg
		//   ic_fluent_settings_16_regular.svg
		//   ic_fluent_settings_20_regular.svg
		//   ic_fluent_settings_24_regular.svg
		//   ic_fluent_settings_28_regular.svg
		//   ic_fluent_settings_32_regular.svg
		//   ic_fluent_arrow_repeat_all_off_16_regular.svg
		//   ic_fluent_arrow_repeat_all_off_20_regular.svg
		//   ic_fluent_arrow_repeat_all_off_24_regular.svg
		//   ic_fluent_alert_16_regular.svg
		//   ic_fluent_alert_20_regular.svg
		//   ic_fluent_alert_24_regular.svg
		//   ic_fluent_alert_28_regular.svg
		//   ic_fluent_alert_32_regular.svg
		//   ic_fluent_search_16_regular.svg
		//   ic_fluent_search_20_regular.svg
		//   ic_fluent_search_24_regular.svg
		//   ic_fluent_search_28_regular.svg
		//   ic_fluent_search_32_regular.svg
		//   ic_fluent_arrow_sort_down_lines_16_regular.svg
		//   ic_fluent_arrow_sort_down_lines_20_regular.svg
		//   ic_fluent_arrow_sort_down_lines_24_regular.svg
		//   ic_fluent_clipboard_task_list_ltr_20_regular.svg
		//   ic_fluent_clipboard_task_list_ltr_24_regular.svg
		//   ic_fluent_text_bullet_list_add_20_regular.svg
		//   ic_fluent_text_bullet_list_add_24_regular.svg
		//   ic_fluent_folder_open_16_regular.svg
		//   ic_fluent_folder_open_20_regular.svg
		//   ic_fluent_folder_open_24_regular.svg
		//   ic_fluent_folder_open_28_regular.svg
		//   ic_fluent_arrow_up_16_regular.svg
		//   ic_fluent_arrow_up_20_regular.svg
		//   ic_fluent_arrow_up_24_regular.svg
		//   ic_fluent_arrow_up_28_regular.svg
		//   ic_fluent_arrow_up_32_regular.svg
		//   ic_fluent_arrow_down_16_regular.svg
		//   ic_fluent_arrow_down_20_regular.svg
		//   ic_fluent_arrow_down_24_regular.svg
		//   ic_fluent_arrow_down_28_regular.svg
		//   ic_fluent_arrow_down_32_regular.svg
		//   ic_fluent_document_text_16_regular.svg
		//   ic_fluent_document_text_20_regular.svg
		//   ic_fluent_document_text_24_regular.svg
		//   ic_fluent_document_text_28_regular.svg
		//   ic_fluent_document_text_32_regular.svg
		//   ic_fluent_note_edit_20_regular.svg
		//   ic_fluent_note_edit_24_regular.svg
		//   ic_fluent_code_16_regular.svg
		//   ic_fluent_code_20_regular.svg
		//   ic_fluent_code_24_regular.svg
		//   ic_fluent_book_search_20_regular.svg
		//   ic_fluent_book_search_24_regular.svg
		//   ic_fluent_document_arrow_down_16_regular.svg
		//   ic_fluent_document_arrow_down_20_regular.svg
		//   ic_fluent_document_arrow_down_24_regular.svg
		//   ic_fluent_text_bullet_list_ltr_16_regular.svg
		//   ic_fluent_text_bullet_list_ltr_20_regular.svg
		//   ic_fluent_text_bullet_list_ltr_24_regular.svg
		//   ic_fluent_grid_16_regular.svg
		//   ic_fluent_grid_20_regular.svg
		//   ic_fluent_grid_24_regular.svg
		//   ic_fluent_grid_28_regular.svg
		//   ic_fluent_chevron_down_16_regular.svg
		//   ic_fluent_chevron_down_20_regular.svg
		//   ic_fluent_chevron_down_24_regular.svg
		//   ic_fluent_chevron_down_28_regular.svg
		//   ic_fluent_chevron_down_32_regular.svg
		//   ic_fluent_chevron_up_16_regular.svg
		//   ic_fluent_chevron_up_20_regular.svg
		//   ic_fluent_chevron_up_24_regular.svg
		//   ic_fluent_chevron_up_28_regular.svg
		//   ic_fluent_chevron_up_32_regular.svg
		//   ic_fluent_chevron_double_down_16_regular.svg
		//   ic_fluent_chevron_double_down_20_regular.svg
		//   ic_fluent_chevron_double_up_16_regular.svg
		//   ic_fluent_chevron_double_up_20_regular.svg
		//   ic_fluent_arrow_enter_16_regular.svg
		//   ic_fluent_arrow_enter_20_regular.svg
		//   ic_fluent_next_16_regular.svg
		//   ic_fluent_next_20_regular.svg
		//   ic_fluent_next_24_regular.svg
		//   ic_fluent_next_28_regular.svg
		//   ic_fluent_next_32_regular.svg
		//   ic_fluent_live_16_regular.svg
		//   ic_fluent_live_20_regular.svg
		//   ic_fluent_live_24_regular.svg
		//   ic_fluent_desktop_16_regular.svg
		//   ic_fluent_desktop_20_regular.svg
		//   ic_fluent_desktop_24_regular.svg
		//   ic_fluent_desktop_28_regular.svg
		//   ic_fluent_desktop_32_regular.svg
		//   ic_fluent_closed_caption_off_16_regular.svg
		//   ic_fluent_closed_caption_off_20_regular.svg
		//   ic_fluent_closed_caption_off_24_regular.svg
		//   ic_fluent_closed_caption_off_28_regular.svg
		//   ic_fluent_video_play_pause_20_regular.svg
		//   ic_fluent_video_play_pause_24_regular.svg
		//   ic_fluent_arrow_step_over_16_regular.svg
		//   ic_fluent_arrow_step_over_20_regular.svg
		//   ic_fluent_send_16_regular.svg
		//   ic_fluent_send_20_regular.svg
		//   ic_fluent_send_24_regular.svg
		//   ic_fluent_send_28_regular.svg
		//   ic_fluent_send_32_regular.svg
		//   ic_fluent_broom_16_regular.svg
		//   ic_fluent_broom_20_regular.svg
		//   ic_fluent_broom_24_regular.svg
		//   ic_fluent_broom_28_regular.svg
		//   ic_fluent_broom_32_regular.svg
		//   ic_fluent_note_16_regular.svg
		//   ic_fluent_note_20_regular.svg
		//   ic_fluent_note_24_regular.svg
		//   ic_fluent_note_28_regular.svg
		//   ic_fluent_music_note_2_16_regular.svg
		//   ic_fluent_music_note_2_20_regular.svg
		//   ic_fluent_music_note_2_24_regular.svg
		//   ic_fluent_slide_text_16_regular.svg
		//   ic_fluent_slide_text_20_regular.svg
		//   ic_fluent_slide_text_24_regular.svg
		//   ic_fluent_slide_text_28_regular.svg
		//   ic_fluent_slide_text_32_regular.svg
		//   ic_fluent_book_open_16_regular.svg
		//   ic_fluent_book_open_20_regular.svg
		//   ic_fluent_book_open_24_regular.svg
		//   ic_fluent_book_open_28_regular.svg
		//   ic_fluent_book_open_32_regular.svg
		//   ic_fluent_text_t_16_regular.svg
		//   ic_fluent_text_t_20_regular.svg
		//   ic_fluent_text_t_24_regular.svg
		//   ic_fluent_text_t_28_regular.svg
		//   ic_fluent_text_t_32_regular.svg
		//   ic_fluent_info_16_regular.svg
		//   ic_fluent_info_20_regular.svg
		//   ic_fluent_info_24_regular.svg
		//   ic_fluent_info_28_regular.svg
		//   ic_fluent_info_32_regular.svg
		//   ic_fluent_video_16_regular.svg
		//   ic_fluent_video_20_regular.svg
		//   ic_fluent_video_24_regular.svg
		//   ic_fluent_video_28_regular.svg
		//   ic_fluent_video_32_regular.svg
		//   ic_fluent_tab_desktop_multiple_16_regular.svg
		//   ic_fluent_tab_desktop_multiple_20_regular.svg
		//   ic_fluent_tab_desktop_multiple_24_regular.svg
		//   ic_fluent_keyboard_16_regular.svg
		//   ic_fluent_keyboard_20_regular.svg
		//   ic_fluent_keyboard_24_regular.svg
		//   ic_fluent_warning_16_regular.svg
		//   ic_fluent_warning_20_regular.svg
		//   ic_fluent_warning_24_regular.svg
		//   ic_fluent_warning_28_regular.svg
		//   ic_fluent_warning_32_regular.svg
		//   ic_fluent_text_effects_24_regular.svg
		//   ic_fluent_eye_off_24_regular.svg
		//   ic_fluent_text_bold_24_regular.svg
		//   ic_fluent_text_underline_24_regular.svg
		//   ic_fluent_text_italic_24_regular.svg
		//   ic_fluent_image_off_24_regular.svg
		//   ic_fluent_save_24_regular.svg
		//   ic_fluent_subtitles_24_regular.svg
		//   ic_fluent_text_case_title_24_regular.svg
		//   ic_fluent_number_symbol_24_regular.svg
		//   ic_fluent_number_row_24_regular.svg
		//   ic_fluent_arrow_bidirectional_left_right_24_regular.svg
		//   ic_fluent_number_circle_1_24_regular.svg
		//   ic_fluent_transparency_square_24_regular.svg
		//   ic_fluent_textbox_align_top_24_regular.svg
		//   ic_fluent_textbox_align_middle_24_regular.svg
		//   ic_fluent_textbox_align_bottom_24_regular.svg
		//   ic_fluent_text_align_left_24_regular.svg
		//   ic_fluent_text_align_center_24_regular.svg
		//   ic_fluent_text_align_right_24_regular.svg
		//   ic_fluent_image_border_24_regular.svg
		//   ic_fluent_image_24_regular.svg
		//   ic_fluent_question_circle_16_regular.svg
		//   ic_fluent_question_circle_20_regular.svg
		//   ic_fluent_question_circle_24_regular.svg
		//   ic_fluent_globe_16_regular.svg
		//   ic_fluent_globe_20_regular.svg
		//   ic_fluent_globe_24_regular.svg
		//   ic_fluent_music_note_2_16_filled.svg
		//   ic_fluent_music_note_2_20_filled.svg
		//   ic_fluent_music_note_2_24_filled.svg
		//   ic_fluent_slide_text_16_filled.svg
		//   ic_fluent_slide_text_20_filled.svg
		//   ic_fluent_slide_text_24_filled.svg
		//   ic_fluent_slide_text_28_filled.svg
		//   ic_fluent_slide_text_32_filled.svg
		//   ic_fluent_book_open_16_filled.svg
		//   ic_fluent_book_open_20_filled.svg
		//   ic_fluent_book_open_24_filled.svg
		//   ic_fluent_book_open_28_filled.svg
		//   ic_fluent_book_open_32_filled.svg
		//   ic_fluent_text_t_16_filled.svg
		//   ic_fluent_text_t_20_filled.svg
		//   ic_fluent_text_t_24_filled.svg
		//   ic_fluent_text_t_28_filled.svg
		//   ic_fluent_text_t_32_filled.svg
		//   ic_fluent_info_16_filled.svg
		//   ic_fluent_info_20_filled.svg
		//   ic_fluent_info_24_filled.svg
		//   ic_fluent_info_28_filled.svg
		//   ic_fluent_info_32_filled.svg
		//   ic_fluent_document_text_16_filled.svg
		//   ic_fluent_document_text_20_filled.svg
		//   ic_fluent_document_text_24_filled.svg
		//   ic_fluent_document_text_28_filled.svg
		//   ic_fluent_document_text_32_filled.svg
		//   ic_fluent_video_16_filled.svg
		//   ic_fluent_video_20_filled.svg
		//   ic_fluent_video_24_filled.svg
		//   ic_fluent_video_28_filled.svg
		//   ic_fluent_video_32_filled.svg
		//   ic_fluent_desktop_16_filled.svg
		//   ic_fluent_desktop_20_filled.svg
		//   ic_fluent_desktop_24_filled.svg
		//   ic_fluent_desktop_28_filled.svg
		//   ic_fluent_desktop_32_filled.svg
		//   ic_fluent_closed_caption_off_16_filled.svg
		//   ic_fluent_closed_caption_off_20_filled.svg
		//   ic_fluent_closed_caption_off_24_filled.svg
		//   ic_fluent_closed_caption_off_28_filled.svg
		//   ic_fluent_live_16_filled.svg
		//   ic_fluent_live_20_filled.svg
		//   ic_fluent_live_24_filled.svg
		internal const string IconSourceCommit = "41089846ad80c5749df505aef35a68712c1561b9";

		private static readonly IconSource[] Sources =
		{
			new IconSource("document_add", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M6 1C4.89543 1 4 1.89543 4 3V5.20703C4.32228 5.11588 4.65659 5.05337 5 5.02242V3C5 2.44772 5.44772 2 6 2H9V4.5C9 5.32843 9.67157 6 10.5 6H13V13C13 13.5523 12.5523 14 12 14H9.74284C9.42948 14.3794 9.06621 14.7161 8.66308 15H12C13.1046 15 14 14.1046 14 13V5.41421C14 5.01639 13.842 4.63486 13.5607 4.35355L10.6464 1.43934C10.3651 1.15804 9.98361 1 9.58579 1H6ZM12.7929 5H10.5C10.2239 5 10 4.77614 10 4.5V2.20711L12.7929 5ZM10 10.5C10 12.9853 7.98528 15 5.5 15C3.01472 15 1 12.9853 1 10.5C1 8.01472 3.01472 6 5.5 6C7.98528 6 10 8.01472 10 10.5ZM6 8.5C6 8.22386 5.77614 8 5.5 8C5.22386 8 5 8.22386 5 8.5V10H3.5C3.22386 10 3 10.2239 3 10.5C3 10.7761 3.22386 11 3.5 11H5L5 12.5C5 12.7761 5.22386 13 5.5 13C5.77614 13 6 12.7761 6 12.5V11H7.5C7.77614 11 8 10.7761 8 10.5C8 10.2239 7.77614 10 7.5 10H6V8.5Z"),
			new IconSource("document_add", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M6 2C4.89543 2 4 2.89543 4 4V9.20703C4.32228 9.11588 4.65659 9.05337 5 9.02242V4C5 3.44772 5.44772 3 6 3H10V6.5C10 7.32843 10.6716 8 11.5 8H15V16C15 16.5523 14.5523 17 14 17H10.4003C10.2174 17.3578 9.99647 17.6929 9.74284 18H14C15.1046 18 16 17.1046 16 16V7.41421C16 7.01639 15.842 6.63486 15.5607 6.35355L11.6464 2.43934C11.3651 2.15804 10.9836 2 10.5858 2H6ZM14.7929 7H11.5C11.2239 7 11 6.77614 11 6.5V3.20711L14.7929 7ZM10 14.5C10 16.9853 7.98528 19 5.5 19C3.01472 19 1 16.9853 1 14.5C1 12.0147 3.01472 10 5.5 10C7.98528 10 10 12.0147 10 14.5ZM6 12.5C6 12.2239 5.77614 12 5.5 12C5.22386 12 5 12.2239 5 12.5V14H3.5C3.22386 14 3 14.2239 3 14.5C3 14.7761 3.22386 15 3.5 15H5L5 16.5C5 16.7761 5.22386 17 5.5 17C5.77614 17 6 16.7761 6 16.5V15H7.5C7.77614 15 8 14.7761 8 14.5C8 14.2239 7.77614 14 7.5 14H6V12.5Z"),
			new IconSource("document_add", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M18.5 20C18.5 20.275 18.276 20.5 18 20.5H12.2678C11.9806 21.051 11.6168 21.5557 11.1904 22H18C19.104 22 20 21.104 20 20V9.828C20 9.298 19.789 8.789 19.414 8.414L13.585 2.586C13.2277 2.22867 12.6729 2 12.172 2H6C4.896 2 4 2.896 4 4V11.4982C4.47417 11.3004 4.97679 11.1572 5.5 11.0764V4C5.5 3.725 5.724 3.5 6 3.5H12V8C12 9.104 12.896 10 14 10H18.5V20ZM13.5 4.621L17.378 8.5H14C13.724 8.5 13.5 8.275 13.5 8V4.621ZM12 17.5C12 14.4624 9.53757 12 6.5 12C3.46243 12 1 14.4624 1 17.5C1 20.5376 3.46243 23 6.5 23C9.53757 23 12 20.5376 12 17.5ZM7.00065 18L7.00111 20.5035C7.00111 20.7797 6.77725 21.0035 6.50111 21.0035C6.22497 21.0035 6.00111 20.7797 6.00111 20.5035L6.00065 18H3.4956C3.21973 18 2.99609 17.7762 2.99609 17.5C2.99609 17.2239 3.21973 17 3.4956 17H6.00046L6 14.4993C6 14.2231 6.22386 13.9993 6.5 13.9993C6.77614 13.9993 7 14.2231 7 14.4993L7.00046 17H9.49659C9.77246 17 9.99609 17.2239 9.99609 17.5C9.99609 17.7762 9.77246 18 9.49659 18H7.00065Z"),
			new IconSource("document_add", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M22.45 23.6C22.45 24.097 22.047 24.5 21.55 24.5H13.8457C13.4964 25.053 13.0768 25.5572 12.5995 26H21.55C22.876 26 23.95 24.926 23.95 23.6V11.395C23.95 10.758 23.697 10.148 23.247 9.698L16.253 2.704C16.201 2.65197 16.1432 2.60789 16.0858 2.56406C16.0671 2.54977 16.0484 2.53551 16.03 2.521C15.734 2.288 15.394 2.121 15.026 2.047C14.872 2.016 14.715 2 14.555 2H6.4C5.075 2 4 3.075 4 4.401V13.8655C4.47237 13.6158 4.97463 13.415 5.5 13.27V4.401C5.5 3.904 5.904 3.5 6.4 3.5H13.95V10C13.95 11.105 14.846 12 15.95 12L22.45 11.999V23.6ZM15.45 4.023L21.928 10.499L15.95 10.5C15.77 10.5 15.651 10.409 15.596 10.354C15.542 10.299 15.45 10.18 15.45 10V4.023ZM14 20.5004C14 24.0903 11.0899 27.0004 7.5 27.0004C3.91015 27.0004 1 24.0903 1 20.5004C1 16.9106 3.91015 14.0004 7.5 14.0004C11.0899 14.0004 14 16.9106 14 20.5004ZM8 16.5004C8 16.2243 7.77614 16.0004 7.5 16.0004C7.22386 16.0004 7 16.2243 7 16.5004V20.0004H3.5C3.22386 20.0004 3 20.2243 3 20.5004C3 20.7766 3.22386 21.0004 3.5 21.0004H7L7 24.5004C7 24.7766 7.22386 25.0004 7.5 25.0004C7.77614 25.0004 8 24.7766 8 24.5004V21.0004H11.5C11.7761 21.0004 12 20.7766 12 20.5004C12 20.2243 11.7761 20.0004 11.5 20.0004H8V16.5004Z"),
			new IconSource("edit", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M14.236 1.76406C13.2123 0.74037 11.5525 0.740369 10.5289 1.76406L2.65722 9.63568C2.28304 10.0099 2.01623 10.4777 1.88467 10.9902L1.01571 14.3757C0.971767 14.5469 1.02148 14.7286 1.14646 14.8536C1.27144 14.9785 1.45312 15.0282 1.62432 14.9843L5.00978 14.1153C5.52234 13.9838 5.99015 13.717 6.36433 13.3428L14.236 5.47117C15.2596 4.44748 15.2596 2.78775 14.236 1.76406ZM11.236 2.47117C11.8691 1.838 12.8957 1.838 13.5288 2.47117C14.162 3.10433 14.162 4.13089 13.5288 4.76406L12.75 5.54288L10.4571 3.24999L11.236 2.47117ZM9.75002 3.9571L12.0429 6.24999L5.65722 12.6357C5.40969 12.8832 5.10023 13.0597 4.76117 13.1467L2.19447 13.8055L2.85327 11.2388C2.9403 10.8998 3.1168 10.5903 3.36433 10.3428L9.75002 3.9571Z"),
			new IconSource("edit", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M17.1813 2.92689C16.029 1.71505 14.1046 1.69077 12.9222 2.87317L3.54735 12.2475C3.21952 12.5754 2.99198 12.9899 2.89142 13.4424L2.0138 17.3923C1.97672 17.5592 2.02748 17.7335 2.14838 17.8544C2.26928 17.9753 2.44356 18.026 2.61044 17.9889L6.53683 17.1157C7.00426 17.0118 7.43237 16.7767 7.77096 16.4381L17.129 7.08003C18.27 5.939 18.2932 4.09631 17.1813 2.92689ZM13.6293 3.58029C14.4142 2.79538 15.6917 2.8115 16.4566 3.61596C17.1947 4.39225 17.1793 5.61548 16.4218 6.37293L15.7506 7.04418L12.958 4.25155L13.6293 3.58029ZM12.2508 4.95864L15.0435 7.7513L7.06385 15.731C6.8597 15.9352 6.60158 16.0769 6.31975 16.1396L3.16044 16.8421L3.86762 13.6593C3.92692 13.3924 4.0611 13.148 4.25444 12.9547L12.2508 4.95864Z"),
			new IconSource("edit", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M20.9519 3.0481C19.5543 1.65058 17.2885 1.65064 15.8911 3.04825L3.94103 14.9997C3.5347 15.4061 3.2491 15.9172 3.116 16.4762L2.02041 21.0777C1.96009 21.3311 2.03552 21.5976 2.21968 21.7817C2.40385 21.9659 2.67037 22.0413 2.92373 21.981L7.52498 20.8855C8.08418 20.7523 8.59546 20.4666 9.00191 20.0601L20.952 8.10861C22.3493 6.71112 22.3493 4.4455 20.9519 3.0481ZM16.9518 4.10884C17.7634 3.29709 19.0795 3.29705 19.8912 4.10876C20.7028 4.9204 20.7029 6.23632 19.8913 7.04801L19 7.93946L16.0606 5.00012L16.9518 4.10884ZM15 6.06084L17.9394 9.00018L7.94119 18.9995C7.73104 19.2097 7.46668 19.3574 7.17755 19.4263L3.76191 20.2395L4.57521 16.8237C4.64402 16.5346 4.79168 16.2704 5.00175 16.0603L15 6.06084Z"),
			new IconSource("edit", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M24.8497 3.14974C23.3141 1.61414 20.8244 1.61415 19.2889 3.14976L4.5025 17.9367C4.06211 18.3771 3.74255 18.9235 3.57462 19.5233L2.02779 25.0477C1.95474 25.3086 2.02809 25.5886 2.21968 25.7802C2.41127 25.9718 2.69131 26.0452 2.95223 25.9721L8.47647 24.4254C9.07632 24.2575 9.62281 23.9379 10.0633 23.4974L24.8497 8.71037C26.3852 7.17482 26.3852 4.68527 24.8497 3.14974ZM20.3496 4.2104C21.2993 3.2606 22.8392 3.26059 23.789 4.21039C24.7387 5.16014 24.7387 6.69997 23.789 7.64973L22.2498 9.18905L18.8104 5.74965L20.3496 4.2104ZM17.7497 6.81034L21.1891 10.2497L9.00259 22.4367C8.74429 22.6951 8.42382 22.8825 8.07205 22.981L3.83174 24.1682L5.01906 19.9277C5.11754 19.576 5.30493 19.2556 5.56318 18.9974L17.7497 6.81034Z"),
			new IconSource("edit", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M28.5645 3.43393C26.6552 1.52457 23.5595 1.52463 21.6502 3.43407L4.35655 20.729C3.85234 21.2333 3.5046 21.8725 3.35518 22.5698L2.02222 28.7903C1.95114 29.122 2.05305 29.4671 2.29291 29.7069C2.53277 29.9468 2.87787 30.0487 3.20955 29.9776L9.4299 28.6447C10.1273 28.4953 10.7666 28.1474 11.2709 27.6431L28.5647 10.348C30.4738 8.43869 30.4738 5.34319 28.5645 3.43393ZM23.0644 4.84823C24.1927 3.71989 26.022 3.71986 27.1503 4.84815C28.2785 5.97637 28.2786 7.80558 27.1504 8.93386L26.2494 9.83495L22.1636 5.74916L23.0644 4.84823ZM20.7494 7.16343L24.8352 11.2492L9.85667 26.2289C9.62498 26.4606 9.33124 26.6204 9.01084 26.6891L4.30164 27.6982L5.31079 22.9888C5.37943 22.6685 5.53918 22.3748 5.77082 22.1432L20.7494 7.16343Z"),
			new IconSource("copy", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M5 6H3.5C2.67157 6 2 6.67157 2 7.5V12.5C2 13.3284 2.67157 14 3.5 14H7.5C8.32843 14 9 13.3284 9 12.5V12H10V12.5C10 13.8807 8.88071 15 7.5 15H3.5C2.11929 15 1 13.8807 1 12.5V7.5C1 6.11929 2.11929 5 3.5 5H5V6ZM12.5 1C13.8807 1 15 2.11929 15 3.5V8.5C15 9.88071 13.8807 11 12.5 11H8.5C7.11929 11 6 9.88071 6 8.5V3.5C6 2.11929 7.11929 1 8.5 1H12.5ZM8.5 2C7.67157 2 7 2.67157 7 3.5V8.5C7 9.32843 7.67157 10 8.5 10H12.5C13.3284 10 14 9.32843 14 8.5V3.5C14 2.67157 13.3284 2 12.5 2H8.5Z"),
			new IconSource("copy", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M7 7H5C3.89543 7 3 7.89543 3 9V15C3 16.1046 3.89543 17 5 17H9C10.1046 17 11 16.1046 11 15H12C12 16.6569 10.6569 18 9 18H5C3.34315 18 2 16.6569 2 15V9C2 7.34315 3.34315 6 5 6H7V7ZM15 2C16.6569 2 18 3.34315 18 5V11C18 12.6569 16.6569 14 15 14H11C9.34315 14 8 12.6569 8 11V5C8 3.34315 9.34315 2 11 2H15ZM11 3C9.89543 3 9 3.89543 9 5V11C9 12.1046 9.89543 13 11 13H15C16.1046 13 17 12.1046 17 11V5C17 3.89543 16.1046 3 15 3H11Z"),
			new IconSource("copy", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M8.5 8.5H5.25C4.2835 8.5 3.5 9.2835 3.5 10.25V18.75C3.5 19.7165 4.2835 20.5 5.25 20.5H11.25C12.2165 20.5 13 19.7165 13 18.75V18H14.5V18.75C14.5 20.5449 13.0449 22 11.25 22H5.25C3.45507 22 2 20.5449 2 18.75V10.25C2 8.45507 3.45507 7 5.25 7H8.5V8.5ZM18.75 2C20.5449 2 22 3.45507 22 5.25V13.75C22 15.5449 20.5449 17 18.75 17H12.75C10.9551 17 9.5 15.5449 9.5 13.75V5.25C9.5 3.45507 10.9551 2 12.75 2H18.75ZM12.75 3.5C11.7835 3.5 11 4.2835 11 5.25V13.75C11 14.7165 11.7835 15.5 12.75 15.5H18.75C19.7165 15.5 20.5 14.7165 20.5 13.75V5.25C20.5 4.2835 19.7165 3.5 18.75 3.5H12.75Z"),
			new IconSource("copy", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M10 9.5H5.75C4.50736 9.5 3.5 10.5074 3.5 11.75V22.25C3.50018 23.4925 4.50747 24.5 5.75 24.5H13.25C14.4925 24.5 15.4998 23.4925 15.5 22.25V21H17V22.25C16.9998 24.3209 15.321 26 13.25 26H5.75C3.67904 26 2.00018 24.3209 2 22.25V11.75C2 9.67893 3.67893 8 5.75 8H10V9.5ZM22.25 2C24.3211 2 26 3.67893 26 5.75V16.25C25.9998 18.3209 24.321 20 22.25 20H14.75C12.679 20 11.0002 18.3209 11 16.25V5.75C11 3.67893 12.6789 2 14.75 2H22.25ZM14.75 3.5C13.5074 3.5 12.5 4.50736 12.5 5.75V16.25C12.5002 17.4925 13.5075 18.5 14.75 18.5H22.25C23.4925 18.5 24.4998 17.4925 24.5 16.25V5.75C24.5 4.50736 23.4926 3.5 22.25 3.5H14.75Z"),
			new IconSource("copy", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M10.5 11H6.5C5.11929 11 4 12.1193 4 13.5V25.5C4 26.8807 5.11929 28 6.5 28H15.5C16.8807 28 18 26.8807 18 25.5V24.5H20V25.5C20 27.9853 17.9853 30 15.5 30H6.5C4.01472 30 2 27.9853 2 25.5V13.5C2 11.0147 4.01472 9 6.5 9H10.5V11ZM25.5 2C27.9853 2 30 4.01472 30 6.5V18.5C30 20.9853 27.9853 23 25.5 23H16.5C14.0147 23 12 20.9853 12 18.5V6.5C12 4.01472 14.0147 2 16.5 2H25.5ZM16.5 4C15.1193 4 14 5.11929 14 6.5V18.5C14 19.8807 15.1193 21 16.5 21H25.5C26.8807 21 28 19.8807 28 18.5V6.5C28 5.11929 26.8807 4 25.5 4H16.5Z"),
			new IconSource("folder_arrow_right", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M2 4.5V6H5.58579C5.71839 6 5.84557 5.94732 5.93934 5.85355L7.29289 4.5L5.93934 3.14645C5.84557 3.05268 5.71839 3 5.58579 3H3.5C2.67157 3 2 3.67157 2 4.5ZM1 4.5C1 3.11929 2.11929 2 3.5 2H5.58579C5.98361 2 6.36514 2.15804 6.64645 2.43934L8.20711 4H12.5C13.8807 4 15 5.11929 15 6.5V7.25716C14.6929 7.00353 14.3578 6.78261 14 6.59971V6.5C14 5.67157 13.3284 5 12.5 5H8.20711L6.64645 6.56066C6.36514 6.84197 5.98361 7 5.58579 7H2V11.5C2 12.3284 2.67157 13 3.5 13H6.20703C6.30564 13.3486 6.43777 13.6832 6.59971 14H3.5C2.11929 14 1 12.8807 1 11.5V4.5ZM11.5 7C13.9853 7 16 9.01472 16 11.5C16 13.9853 13.9853 16 11.5 16C9.01472 16 7 13.9853 7 11.5C7 9.01472 9.01472 7 11.5 7ZM13.8532 11.854L13.8557 11.8514C13.9026 11.804 13.938 11.7495 13.9621 11.6914C13.9861 11.6333 13.9996 11.5697 14 11.503L14 11.5L14 11.497C13.9996 11.4303 13.9861 11.3667 13.9621 11.3086C13.9377 11.2496 13.9015 11.1944 13.8536 11.1464L11.8536 9.14645C11.6583 8.95118 11.3417 8.95118 11.1464 9.14645C10.9512 9.34171 10.9512 9.65829 11.1464 9.85355L12.2929 11H9.5C9.22386 11 9 11.2239 9 11.5C9 11.7761 9.22386 12 9.5 12H12.2929L11.1464 13.1464C10.9512 13.3417 10.9512 13.6583 11.1464 13.8536C11.3417 14.0488 11.6583 14.0488 11.8536 13.8536L13.8532 11.854Z"),
			new IconSource("folder_arrow_right", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4.5 3C3.11929 3 2 4.11929 2 5.5V14.5C2 15.8807 3.11929 17 4.5 17H9.59971C9.43777 16.6832 9.30564 16.3486 9.20703 16H4.5C3.67157 16 3 15.3284 3 14.5V8H7.08579C7.48361 8 7.86514 7.84196 8.14645 7.56066L9.70711 6H15.5C16.3284 6 17 6.67157 17 7.5V9.59971C17.3578 9.78261 17.6929 10.0035 18 10.2572V7.5C18 6.11929 16.8807 5 15.5 5H9.70711L8.21967 3.51256C7.89148 3.18437 7.44636 3 6.98223 3H4.5ZM3 5.5C3 4.67157 3.67157 4 4.5 4H6.98223C7.18115 4 7.37191 4.07902 7.51256 4.21967L8.79289 5.5L7.43934 6.85355C7.34557 6.94732 7.21839 7 7.08579 7H3V5.5ZM14.5 10C16.9853 10 19 12.0147 19 14.5C19 16.9853 16.9853 19 14.5 19C12.0147 19 10 16.9853 10 14.5C10 12.0147 12.0147 10 14.5 10ZM16.8532 14.854L16.8557 14.8514C16.9026 14.804 16.938 14.7495 16.9621 14.6914C16.9861 14.6333 16.9996 14.5697 17 14.503L17 14.5L17 14.497C16.9996 14.4303 16.9861 14.3667 16.9621 14.3086C16.9377 14.2496 16.9015 14.1944 16.8536 14.1464L14.8536 12.1464C14.6583 11.9512 14.3417 11.9512 14.1464 12.1464C13.9512 12.3417 13.9512 12.6583 14.1464 12.8536L15.2929 14H12.5C12.2239 14 12 14.2239 12 14.5C12 14.7761 12.2239 15 12.5 15H15.2929L14.1464 16.1464C13.9512 16.3417 13.9512 16.6583 14.1464 16.8536C14.3417 17.0488 14.6583 17.0488 14.8536 16.8536L16.8532 14.854Z"),
			new IconSource("folder_arrow_right", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.5 6.25V8H8.12868C8.32759 8 8.51836 7.92098 8.65901 7.78033L10.1893 6.25L8.65901 4.71967C8.51836 4.57902 8.32759 4.5 8.12868 4.5H5.25C4.2835 4.5 3.5 5.2835 3.5 6.25ZM2 6.25C2 4.45507 3.45507 3 5.25 3H8.12868C8.72542 3 9.29771 3.23705 9.71967 3.65901L11.5607 5.5H18.75C20.5449 5.5 22 6.95507 22 8.75V12.8096C21.5557 12.3832 21.051 12.0194 20.5 11.7322V8.75C20.5 7.7835 19.7165 7 18.75 7H11.5607L9.71967 8.84099C9.29771 9.26295 8.72542 9.5 8.12868 9.5H3.5V17.75C3.5 18.7165 4.2835 19.5 5.25 19.5H11.3135C11.4858 20.0335 11.7253 20.5368 12.0218 21H5.25C3.45507 21 2 19.5449 2 17.75V6.25ZM17.5 12C20.5376 12 23 14.4624 23 17.5C23 20.5376 20.5376 23 17.5 23C14.4624 23 12 20.5376 12 17.5C12 14.4624 14.4624 12 17.5 12ZM14.5 17C14.2239 17 14 17.2239 14 17.5C14 17.7761 14.2239 18 14.5 18H19.2929L17.6464 19.6464C17.4512 19.8417 17.4512 20.1583 17.6464 20.3536C17.8417 20.5488 18.1583 20.5488 18.3536 20.3536L20.8536 17.8536C21.0488 17.6583 21.0488 17.3417 20.8536 17.1464L18.3536 14.6464C18.1583 14.4512 17.8417 14.4512 17.6464 14.6464C17.4512 14.8417 17.4512 15.1583 17.6464 15.3536L19.2929 17H14.5Z"),
			new IconSource("folder_arrow_right", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M3.50097 6.68321C3.50291 6.70521 3.50391 6.72749 3.50391 6.75V9H9.42157C9.75309 9 10.071 8.8683 10.3055 8.63388L12.1893 6.75L10.3055 4.86612C10.071 4.6317 9.75309 4.5 9.42157 4.5H5.75C4.5297 4.5 3.5363 5.47146 3.50097 6.68321ZM2.00391 9.82647C2.00131 9.80118 2 9.77566 2 9.75V6.75C2 4.67893 3.67893 3 5.75 3H9.42157C10.1509 3 10.8504 3.28973 11.3661 3.80546L13.5607 6H22.2505C24.3216 6 26.0005 7.67893 26.0005 9.75V14.4015C25.5577 13.924 25.0536 13.5043 24.5005 13.1549V9.75C24.5005 8.50736 23.4932 7.5 22.2505 7.5H13.5607L11.3661 9.69454C10.8504 10.2103 10.1509 10.5 9.42157 10.5H3.50391V20.2501C3.50391 21.4927 4.51127 22.5001 5.75391 22.5001H13.6241C13.8581 23.0356 14.1528 23.5385 14.4995 24.0001H5.75391C3.68284 24.0001 2.00391 22.3211 2.00391 20.2501V9.82647ZM20.5 13C24.0899 13 27 15.9102 27 19.5C27 23.0899 24.0898 26 20.5 26C16.9101 26 14 23.0898 14 19.5C14 15.9101 16.9102 13 20.5 13ZM16.5 19C16.2239 19 16 19.2239 16 19.5C16 19.7761 16.2239 20 16.5 20H23.2929L21.1464 22.1464C20.9512 22.3417 20.9512 22.6583 21.1464 22.8536C21.3417 23.0488 21.6583 23.0488 21.8536 22.8536L24.8536 19.8536C24.9015 19.8056 24.9377 19.7504 24.9621 19.6914C24.9861 19.6333 24.9996 19.5697 25 19.503L25 19.5L25 19.497C24.9992 19.37 24.9504 19.2433 24.8536 19.1464L21.8536 16.1464C21.6583 15.9512 21.3417 15.9512 21.1464 16.1464C20.9512 16.3417 20.9512 16.6583 21.1464 16.8536L23.2929 19H16.5Z"),
			new IconSource("folder_arrow_right", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M4 8.5V11H10.4645C10.8623 11 11.2438 10.842 11.5251 10.5607L13.5858 8.5L11.5251 6.43934C11.2438 6.15804 10.8623 6 10.4645 6H6.5C5.11929 6 4 7.11929 4 8.5ZM2 8.5C2 6.01472 4.01472 4 6.5 4H10.4645C11.3927 4 12.283 4.36875 12.9393 5.02513L15.4142 7.5H25.5C27.9853 7.5 30 9.51472 30 12V16.2917C29.4003 15.7549 28.7281 15.2974 28 14.9355V12C28 10.6193 26.8807 9.5 25.5 9.5H15.4142L12.9393 11.9749C12.283 12.6313 11.3927 13 10.4645 13H4V23.5C4 24.8807 5.11929 26 6.5 26H15.5121C15.7638 26.7121 16.1025 27.383 16.5155 28H6.5C4.01472 28 2 25.9853 2 23.5V8.5ZM31.5 23C31.5 18.8579 28.1421 15.5 24 15.5C19.8579 15.5 16.5 18.8579 16.5 23C16.5 27.1421 19.8579 30.5 24 30.5C28.1421 30.5 31.5 27.1421 31.5 23ZM19.25 22H26.9393L24.2197 19.2803C23.9268 18.9874 23.9268 18.5126 24.2197 18.2197C24.5126 17.9268 24.9874 17.9268 25.2803 18.2197L29.2803 22.2197C29.5732 22.5126 29.5732 22.9874 29.2803 23.2803L25.2803 27.2803C24.9874 27.5732 24.5126 27.5732 24.2197 27.2803C23.9268 26.9874 23.9268 26.5126 24.2197 26.2197L26.9393 23.5H19.25C18.8358 23.5 18.5 23.1642 18.5 22.75C18.5 22.3358 18.8358 22 19.25 22Z"),
			new IconSource("delete", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M7 3H9C9 2.44772 8.55228 2 8 2C7.44772 2 7 2.44772 7 3ZM6 3C6 1.89543 6.89543 1 8 1C9.10457 1 10 1.89543 10 3H14C14.2761 3 14.5 3.22386 14.5 3.5C14.5 3.77614 14.2761 4 14 4H13.4364L12.2313 12.8378C12.0624 14.0765 11.0044 15 9.75422 15H6.24578C4.99561 15 3.93762 14.0765 3.76871 12.8378L2.56355 4H2C1.72386 4 1.5 3.77614 1.5 3.5C1.5 3.22386 1.72386 3 2 3H6ZM7 6.5C7 6.22386 6.77614 6 6.5 6C6.22386 6 6 6.22386 6 6.5V11.5C6 11.7761 6.22386 12 6.5 12C6.77614 12 7 11.7761 7 11.5V6.5ZM9.5 6C9.77614 6 10 6.22386 10 6.5V11.5C10 11.7761 9.77614 12 9.5 12C9.22386 12 9 11.7761 9 11.5V6.5C9 6.22386 9.22386 6 9.5 6ZM4.75954 12.7027C4.86089 13.4459 5.49568 14 6.24578 14H9.75422C10.5043 14 11.1391 13.4459 11.2405 12.7027L12.4272 4H3.57281L4.75954 12.7027Z"),
			new IconSource("delete", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M8.5 4H11.5C11.5 3.17157 10.8284 2.5 10 2.5C9.17157 2.5 8.5 3.17157 8.5 4ZM7.5 4C7.5 2.61929 8.61929 1.5 10 1.5C11.3807 1.5 12.5 2.61929 12.5 4H17.5C17.7761 4 18 4.22386 18 4.5C18 4.77614 17.7761 5 17.5 5H16.4456L15.2521 15.3439C15.0774 16.8576 13.7957 18 12.2719 18H7.72813C6.20431 18 4.92256 16.8576 4.7479 15.3439L3.55437 5H2.5C2.22386 5 2 4.77614 2 4.5C2 4.22386 2.22386 4 2.5 4H7.5ZM5.74131 15.2292C5.85775 16.2384 6.71225 17 7.72813 17H12.2719C13.2878 17 14.1422 16.2384 14.2587 15.2292L15.439 5H4.56101L5.74131 15.2292ZM8.5 7.5C8.77614 7.5 9 7.72386 9 8V14C9 14.2761 8.77614 14.5 8.5 14.5C8.22386 14.5 8 14.2761 8 14V8C8 7.72386 8.22386 7.5 8.5 7.5ZM12 8C12 7.72386 11.7761 7.5 11.5 7.5C11.2239 7.5 11 7.72386 11 8V14C11 14.2761 11.2239 14.5 11.5 14.5C11.7761 14.5 12 14.2761 12 14V8Z"),
			new IconSource("delete", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M10 5H14C14 3.89543 13.1046 3 12 3C10.8954 3 10 3.89543 10 5ZM8.5 5C8.5 3.067 10.067 1.5 12 1.5C13.933 1.5 15.5 3.067 15.5 5H21.25C21.6642 5 22 5.33579 22 5.75C22 6.16421 21.6642 6.5 21.25 6.5H19.9309L18.7589 18.6112C18.5729 20.5334 16.9575 22 15.0263 22H8.97369C7.04254 22 5.42715 20.5334 5.24113 18.6112L4.06908 6.5H2.75C2.33579 6.5 2 6.16421 2 5.75C2 5.33579 2.33579 5 2.75 5H8.5ZM10.5 9.75C10.5 9.33579 10.1642 9 9.75 9C9.33579 9 9 9.33579 9 9.75V17.25C9 17.6642 9.33579 18 9.75 18C10.1642 18 10.5 17.6642 10.5 17.25V9.75ZM14.25 9C14.6642 9 15 9.33579 15 9.75V17.25C15 17.6642 14.6642 18 14.25 18C13.8358 18 13.5 17.6642 13.5 17.25V9.75C13.5 9.33579 13.8358 9 14.25 9ZM6.73416 18.4667C6.84577 19.62 7.815 20.5 8.97369 20.5H15.0263C16.185 20.5 17.1542 19.62 17.2658 18.4667L18.4239 6.5H5.57608L6.73416 18.4667Z"),
			new IconSource("delete", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M11.5 6H16.5C16.5 4.61929 15.3807 3.5 14 3.5C12.6193 3.5 11.5 4.61929 11.5 6ZM10 6C10 3.79086 11.7909 2 14 2C16.2091 2 18 3.79086 18 6H24.25C24.6642 6 25 6.33579 25 6.75C25 7.16421 24.6642 7.5 24.25 7.5H22.9401L21.7233 22.1029C21.5398 24.3057 19.6984 26 17.488 26H10.5123C8.3019 26 6.46053 24.3057 6.27695 22.103L5.05991 7.5H3.75C3.33579 7.5 3 7.16421 3 6.75C3 6.33579 3.33579 6 3.75 6H10ZM7.77177 21.9784C7.89055 23.4037 9.08203 24.5 10.5123 24.5H17.488C18.9183 24.5 20.1097 23.4037 20.2285 21.9784L21.4349 7.5H6.56511L7.77177 21.9784ZM11.75 11C12.1642 11 12.5 11.3358 12.5 11.75V20.25C12.5 20.6642 12.1642 21 11.75 21C11.3358 21 11 20.6642 11 20.25V11.75C11 11.3358 11.3358 11 11.75 11ZM17 11.75C17 11.3358 16.6642 11 16.25 11C15.8358 11 15.5 11.3358 15.5 11.75V20.25C15.5 20.6642 15.8358 21 16.25 21C16.6642 21 17 20.6642 17 20.25V11.75Z"),
			new IconSource("delete", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M13.5 6.5V7H18.5V6.5C18.5 5.11929 17.3807 4 16 4C14.6193 4 13.5 5.11929 13.5 6.5ZM11.5 7V6.5C11.5 4.01472 13.5147 2 16 2C18.4853 2 20.5 4.01472 20.5 6.5V7H28C28.5523 7 29 7.44772 29 8C29 8.55228 28.5523 9 28 9H26.4922L24.5988 25.5677C24.3102 28.0931 22.1729 30 19.6311 30H12.3689C9.82714 30 7.68984 28.0931 7.40123 25.5677L5.50778 9H4C3.44772 9 3 8.55228 3 8C3 7.44772 3.44772 7 4 7H11.5ZM9.3883 25.3406C9.56146 26.8558 10.8438 28 12.3689 28H19.6311C21.1562 28 22.4385 26.8558 22.6117 25.3406L24.4792 9H7.5208L9.3883 25.3406ZM13 12.5C13.5523 12.5 14 12.9477 14 13.5V23.5C14 24.0523 13.5523 24.5 13 24.5C12.4477 24.5 12 24.0523 12 23.5V13.5C12 12.9477 12.4477 12.5 13 12.5ZM20 13.5C20 12.9477 19.5523 12.5 19 12.5C18.4477 12.5 18 12.9477 18 13.5V23.5C18 24.0523 18.4477 24.5 19 24.5C19.5523 24.5 20 24.0523 20 23.5V13.5Z"),
			new IconSource("filmstrip_play", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.5 3C2.11929 3 1 4.11929 1 5.5V10.5C1 11.8807 2.11929 13 3.5 13H12.5C13.8807 13 15 11.8807 15 10.5V5.5C15 4.11929 13.8807 3 12.5 3H3.5ZM2 5.5C2 4.67157 2.67157 4 3.5 4H12.5C13.3284 4 14 4.67157 14 5.5V10.5C14 11.3284 13.3284 12 12.5 12H3.5C2.67157 12 2 11.3284 2 10.5V5.5ZM12.5 5C12.2239 5 12 5.22386 12 5.5V6.5C12 6.77614 12.2239 7 12.5 7C12.7761 7 13 6.77614 13 6.5V5.5C13 5.22386 12.7761 5 12.5 5ZM12 9.5C12 9.22386 12.2239 9 12.5 9C12.7761 9 13 9.22386 13 9.5V10.5C13 10.7761 12.7761 11 12.5 11C12.2239 11 12 10.7761 12 10.5V9.5ZM3.5 5C3.22386 5 3 5.22386 3 5.5V6.5C3 6.77614 3.22386 7 3.5 7C3.77614 7 4 6.77614 4 6.5V5.5C4 5.22386 3.77614 5 3.5 5ZM3 9.5C3 9.22386 3.22386 9 3.5 9C3.77614 9 4 9.22386 4 9.5V10.5C4 10.7761 3.77614 11 3.5 11C3.22386 11 3 10.7761 3 10.5V9.5ZM7 6.50082C7 6.11469 7.34566 5.87425 7.62067 6.06908L9.82536 7.63107C10.0582 7.79604 10.0582 8.20398 9.82536 8.36895L7.62067 9.93092C7.34566 10.1258 7 9.88531 7 9.49918V6.50082Z"),
			new IconSource("filmstrip_play", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4.5 4C3.11929 4 2 5.11929 2 6.5V13.5C2 14.8807 3.11929 16 4.5 16H15.5C16.8807 16 18 14.8807 18 13.5V6.5C18 5.11929 16.8807 4 15.5 4H4.5ZM3 6.5C3 5.67157 3.67157 5 4.5 5H15.5C16.3284 5 17 5.67157 17 6.5V13.5C17 14.3284 16.3284 15 15.5 15H4.5C3.67157 15 3 14.3284 3 13.5V6.5ZM15.5 6C15.2239 6 15 6.22386 15 6.5V7.5C15 7.77614 15.2239 8 15.5 8C15.7761 8 16 7.77614 16 7.5V6.5C16 6.22386 15.7761 6 15.5 6ZM15 12.5C15 12.2239 15.2239 12 15.5 12C15.7761 12 16 12.2239 16 12.5V13.5C16 13.7761 15.7761 14 15.5 14C15.2239 14 15 13.7761 15 13.5V12.5ZM15.5 9C15.2239 9 15 9.22386 15 9.5V10.5C15 10.7761 15.2239 11 15.5 11C15.7761 11 16 10.7761 16 10.5V9.5C16 9.22386 15.7761 9 15.5 9ZM4 6.5C4 6.22386 4.22386 6 4.5 6C4.77614 6 5 6.22386 5 6.5V7.5C5 7.77614 4.77614 8 4.5 8C4.22386 8 4 7.77614 4 7.5V6.5ZM4.5 12C4.22386 12 4 12.2239 4 12.5V13.5C4 13.7761 4.22386 14 4.5 14C4.77614 14 5 13.7761 5 13.5V12.5C5 12.2239 4.77614 12 4.5 12ZM4 9.5C4 9.22386 4.22386 9 4.5 9C4.77614 9 5 9.22386 5 9.5V10.5C5 10.7761 4.77614 11 4.5 11C4.22386 11 4 10.7761 4 10.5V9.5ZM8 8.12603C8 7.64336 8.46089 7.34281 8.82756 7.58635L11.7671 9.53883C12.0776 9.74505 12.0776 10.255 11.7671 10.4612L8.82756 12.4136C8.46088 12.6572 8 12.3566 8 11.874V8.12603Z"),
			new IconSource("filmstrip_play", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.25 4C3.45507 4 2 5.45507 2 7.25V16.75C2 18.5449 3.45507 20 5.25 20H18.75C20.5449 20 22 18.5449 22 16.75V7.25C22 5.45507 20.5449 4 18.75 4H5.25ZM3.5 7.25C3.5 6.2835 4.2835 5.5 5.25 5.5H18.75C19.7165 5.5 20.5 6.2835 20.5 7.25V16.75C20.5 17.7165 19.7165 18.5 18.75 18.5H5.25C4.2835 18.5 3.5 17.7165 3.5 16.75V7.25ZM18.25 7C17.8358 7 17.5 7.33579 17.5 7.75V8.25C17.5 8.66421 17.8358 9 18.25 9C18.6642 9 19 8.66421 19 8.25V7.75C19 7.33579 18.6642 7 18.25 7ZM17.5 15.75C17.5 15.3358 17.8358 15 18.25 15C18.6642 15 19 15.3358 19 15.75V16.25C19 16.6642 18.6642 17 18.25 17C17.8358 17 17.5 16.6642 17.5 16.25V15.75ZM18.25 11C17.8358 11 17.5 11.3358 17.5 11.75V12.25C17.5 12.6642 17.8358 13 18.25 13C18.6642 13 19 12.6642 19 12.25V11.75C19 11.3358 18.6642 11 18.25 11ZM5 7.75C5 7.33579 5.33579 7 5.75 7C6.16421 7 6.5 7.33579 6.5 7.75V8.25C6.5 8.66421 6.16421 9 5.75 9C5.33579 9 5 8.66421 5 8.25V7.75ZM5.75 15C5.33579 15 5 15.3358 5 15.75V16.25C5 16.6642 5.33579 17 5.75 17C6.16421 17 6.5 16.6642 6.5 16.25V15.75C6.5 15.3358 6.16421 15 5.75 15ZM5 11.75C5 11.3358 5.33579 11 5.75 11C6.16421 11 6.5 11.3358 6.5 11.75V12.25C6.5 12.6642 6.16421 13 5.75 13C5.33579 13 5 12.6642 5 12.25V11.75ZM10 9.75123C10 9.17203 10.5761 8.81137 11.0345 9.10362L14.7089 11.4466C15.097 11.6941 15.097 12.306 14.7089 12.5534L11.0344 14.8964C10.5761 15.1886 10 14.828 10 14.2488V9.75123Z"),
			new IconSource("filmstrip_play", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M6.5 4C4.01472 4 2 6.01472 2 8.5V23.5C2 25.9853 4.01472 28 6.5 28H25.5C27.9853 28 30 25.9853 30 23.5V8.5C30 6.01472 27.9853 4 25.5 4H6.5ZM4 8.5C4 7.11929 5.11929 6 6.5 6H25.5C26.8807 6 28 7.11929 28 8.5V23.5C28 24.8807 26.8807 26 25.5 26H6.5C5.11929 26 4 24.8807 4 23.5V8.5ZM25 8C24.4477 8 24 8.44772 24 9V11C24 11.5523 24.4477 12 25 12C25.5523 12 26 11.5523 26 11V9C26 8.44772 25.5523 8 25 8ZM24 15C24 14.4477 24.4477 14 25 14C25.5523 14 26 14.4477 26 15V17C26 17.5523 25.5523 18 25 18C24.4477 18 24 17.5523 24 17V15ZM25 20C24.4477 20 24 20.4477 24 21V23C24 23.5523 24.4477 24 25 24C25.5523 24 26 23.5523 26 23V21C26 20.4477 25.5523 20 25 20ZM6 9C6 8.44772 6.44772 8 7 8C7.55228 8 8 8.44772 8 9V11C8 11.5523 7.55228 12 7 12C6.44772 12 6 11.5523 6 11V9ZM7 14C6.44772 14 6 14.4477 6 15V17C6 17.5523 6.44772 18 7 18C7.55228 18 8 17.5523 8 17V15C8 14.4477 7.55228 14 7 14ZM6 21C6 20.4477 6.44772 20 7 20C7.55228 20 8 20.4477 8 21V23C8 23.5523 7.55228 24 7 24C6.44772 24 6 23.5523 6 23V21ZM13 13.0492C13 12.2403 13.8778 11.7366 14.5762 12.1447L20.1752 15.417C20.7665 15.7626 20.7665 16.6172 20.1752 16.9628L14.5762 20.2351C13.8778 20.6432 13 20.1395 13 19.3306V13.0492Z"),
			new IconSource("arrow_clockwise", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3 8C3 5.23858 5.23858 3 8 3C9.63527 3 11.0878 3.78495 12.0005 5H10C9.72386 5 9.5 5.22386 9.5 5.5C9.5 5.77614 9.72386 6 10 6H12.8904C12.8973 6.00014 12.9041 6.00014 12.911 6H13C13.2761 6 13.5 5.77614 13.5 5.5V2.5C13.5 2.22386 13.2761 2 13 2C12.7239 2 12.5 2.22386 12.5 2.5V4.03138C11.4009 2.78613 9.79253 2 8 2C4.68629 2 2 4.68629 2 8C2 11.3137 4.68629 14 8 14C11.1301 14 13.6999 11.6035 13.9756 8.54488C14.0003 8.26985 13.7975 8.0268 13.5225 8.00202C13.2474 7.97723 13.0044 8.1801 12.9796 8.45512C12.75 11.003 10.6079 13 8 13C5.23858 13 3 10.7614 3 8Z"),
			new IconSource("arrow_clockwise", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4 10C4 6.68629 6.68629 4 10 4C11.7766 4 13.3732 4.77191 14.4723 6H12.5C12.2239 6 12 6.22386 12 6.5C12 6.77614 12.2239 7 12.5 7H15.5C15.7761 7 16 6.77614 16 6.5V3.5C16 3.22386 15.7761 3 15.5 3C15.2239 3 15 3.22386 15 3.5V5.10109C13.7299 3.80499 11.9591 3 10 3C6.13401 3 3 6.13401 3 10C3 13.866 6.13401 17 10 17C13.866 17 17 13.866 17 10C17 9.8191 16.9931 9.6397 16.9796 9.46207C16.9587 9.18673 16.7185 8.98049 16.4431 9.00144C16.1678 9.02239 15.9615 9.26258 15.9825 9.53793C15.9941 9.69034 16 9.84443 16 10C16 13.3137 13.3137 16 10 16C6.68629 16 4 13.3137 4 10Z"),
			new IconSource("arrow_clockwise", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12 4.5C7.85786 4.5 4.5 7.85786 4.5 12C4.5 16.1421 7.85786 19.5 12 19.5C16.1421 19.5 19.5 16.1421 19.5 12C19.5 11.6236 19.4723 11.2538 19.4188 10.8923C19.3515 10.4382 19.6839 10 20.1429 10C20.5138 10 20.839 10.2562 20.8953 10.6228C20.9642 11.0718 21 11.5317 21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C14.3051 3 16.4077 3.86656 18 5.29168V4.25C18 3.83579 18.3358 3.5 18.75 3.5C19.1642 3.5 19.5 3.83579 19.5 4.25V7.25C19.5 7.66421 19.1642 8 18.75 8H15.75C15.3358 8 15 7.66421 15 7.25C15 6.83579 15.3358 6.5 15.75 6.5H17.0991C15.7609 5.25883 13.9691 4.5 12 4.5Z"),
			new IconSource("arrow_clockwise", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M4.5 14C4.5 8.75329 8.75329 4.5 14 4.5C16.7318 4.5 19.1945 5.65254 20.9283 7.5H18.25C17.8358 7.5 17.5 7.83579 17.5 8.25C17.5 8.66421 17.8358 9 18.25 9H22.25C22.6642 9 23 8.66421 23 8.25V4.25C23 3.83579 22.6642 3.5 22.25 3.5C21.8358 3.5 21.5 3.83579 21.5 4.25V5.95329C19.5357 4.12178 16.8988 3 14 3C7.92487 3 3 7.92487 3 14C3 20.0751 7.92487 25 14 25C20.0751 25 25 20.0751 25 14C25 13.5467 24.9725 13.0994 24.9191 12.6599C24.8691 12.2487 24.4953 11.9559 24.0841 12.0059C23.6729 12.0559 23.3801 12.4297 23.4301 12.8409C23.4762 13.2205 23.5 13.6073 23.5 14C23.5 19.2467 19.2467 23.5 14 23.5C8.75329 23.5 4.5 19.2467 4.5 14Z"),
			new IconSource("arrow_clockwise", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M5 16C5 9.92487 9.92487 5 16 5C18.9227 5 21.5792 6.1392 23.55 8H21C20.4477 8 20 8.44772 20 9C20 9.55228 20.4477 10 21 10H26C26.5523 10 27 9.55228 27 9V4C27 3.44772 26.5523 3 26 3C25.4477 3 25 3.44772 25 4V6.61926C22.665 4.37864 19.4935 3 16 3C8.8203 3 3 8.8203 3 16C3 23.1797 8.8203 29 16 29C23.1797 29 29 23.1797 29 16C29 15.6358 28.985 15.2748 28.9555 14.9178C28.9101 14.3674 28.4271 13.958 27.8767 14.0034C27.3263 14.0488 26.9169 14.5318 26.9623 15.0822C26.9873 15.3846 27 15.6907 27 16C27 22.0751 22.0751 27 16 27C9.92487 27 5 22.0751 5 16Z"),
			new IconSource("settings", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M7.99994 6C6.89537 6 5.99994 6.89543 5.99994 8C5.99994 9.10457 6.89537 10 7.99994 10C9.10451 10 9.99994 9.10457 9.99994 8C9.99994 6.89543 9.10451 6 7.99994 6ZM6.99994 8C6.99994 7.44772 7.44765 7 7.99994 7C8.55222 7 8.99994 7.44772 8.99994 8C8.99994 8.55228 8.55222 9 7.99994 9C7.44765 9 6.99994 8.55228 6.99994 8ZM10.618 4.39833C10.233 4.46825 9.86392 4.21413 9.7937 3.83074L9.53397 2.41496C9.50816 2.27427 9.39961 2.16301 9.25912 2.13325C8.84818 2.04621 8.42685 2.00195 8 2.00195C7.57289 2.00195 7.1513 2.04627 6.74013 2.13341C6.5996 2.1632 6.49104 2.27452 6.46529 2.41527L6.20629 3.8308C6.1994 3.86844 6.18942 3.90551 6.17647 3.9416C6.04476 4.30859 5.6392 4.49978 5.27062 4.36863L3.91115 3.88463C3.77603 3.83652 3.62511 3.87431 3.52891 3.98033C2.96005 4.60729 2.52892 5.34708 2.2672 6.15302C2.22305 6.28899 2.26562 6.43805 2.37502 6.53053L3.47694 7.46206C3.50626 7.48685 3.53352 7.51399 3.55843 7.5432C3.81177 7.84027 3.77528 8.28558 3.47693 8.53783L2.37502 9.46935C2.26562 9.56183 2.22305 9.71089 2.2672 9.84685C2.52892 10.6528 2.96005 11.3926 3.52891 12.0196C3.62511 12.1256 3.77603 12.1634 3.91115 12.1153L5.27068 11.6312C5.30687 11.6184 5.3441 11.6084 5.38196 11.6015C5.76701 11.5316 6.13608 11.7857 6.2063 12.1691L6.46529 13.5846C6.49104 13.7254 6.5996 13.8367 6.74013 13.8665C7.1513 13.9536 7.57289 13.9979 8 13.9979C8.42685 13.9979 8.84818 13.9537 9.25912 13.8666C9.39961 13.8369 9.50816 13.7256 9.53397 13.5849L9.79368 12.1692C9.8006 12.1314 9.81058 12.0944 9.82353 12.0583C9.95524 11.6913 10.3608 11.5001 10.7294 11.6312L12.0888 12.1153C12.224 12.1634 12.3749 12.1256 12.4711 12.0196C13.04 11.3926 13.4711 10.6528 13.7328 9.84685C13.777 9.71089 13.7344 9.56183 13.625 9.46935L12.5231 8.53782C12.4937 8.51303 12.4665 8.48589 12.4416 8.45667C12.1882 8.1596 12.2247 7.71429 12.5231 7.46205L13.625 6.53053C13.7344 6.43805 13.777 6.28899 13.7328 6.15302C13.4711 5.34708 13.04 4.60729 12.4711 3.98033C12.3749 3.87431 12.224 3.83652 12.0888 3.88463L10.7293 4.36865C10.6931 4.38152 10.6559 4.39146 10.618 4.39833ZM3.99863 4.97726L4.93522 5.3107C5.82017 5.62559 6.79872 5.16815 7.11769 4.2794C7.14903 4.19207 7.17324 4.1021 7.18996 4.01078L7.36738 3.04113C7.5757 3.01512 7.78684 3.00195 8 3.00195C8.213 3.00195 8.42397 3.0151 8.63214 3.04107L8.81011 4.01117C8.98053 4.9408 9.87266 5.55003 10.7967 5.38225C10.8877 5.36572 10.9775 5.34176 11.0647 5.31073L12.0014 4.97726C12.2564 5.31084 12.4684 5.67476 12.6319 6.06064L11.8774 6.6984C11.1566 7.30787 11.0675 8.38649 11.6807 9.10555C11.7408 9.17609 11.8067 9.24166 11.8775 9.3015L12.6319 9.93924C12.4684 10.3251 12.2564 10.689 12.0014 11.0226L11.0646 10.6891C10.1797 10.3742 9.20128 10.8317 8.88231 11.7205C8.85096 11.8078 8.82677 11.8978 8.81004 11.9891L8.63214 12.9588C8.42397 12.9848 8.213 12.9979 8 12.9979C7.78684 12.9979 7.5757 12.9848 7.36738 12.9587L7.18994 11.989C7.01965 11.0592 6.12743 10.4498 5.2033 10.6176C5.11227 10.6342 5.0225 10.6581 4.93528 10.6892L3.99863 11.0226C3.74357 10.689 3.53161 10.3251 3.36814 9.93924L4.12257 9.30148C4.84343 8.69201 4.93254 7.61339 4.31933 6.89433C4.25917 6.82378 4.19332 6.75822 4.12254 6.69838L3.36814 6.06064C3.53161 5.67476 3.74357 5.31084 3.99863 4.97726Z"),
			new IconSource("settings", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M1.91099 7.38266C2.28028 6.24053 2.88863 5.19213 3.69133 4.30364C3.82707 4.15339 4.04002 4.09984 4.23069 4.16802L6.14897 4.85392C6.66905 5.03977 7.24131 4.76883 7.42716 4.24875C7.44544 4.19762 7.45952 4.14507 7.46925 4.09173L7.83471 2.08573C7.87104 1.88627 8.02422 1.7285 8.22251 1.6863C8.8027 1.5628 9.39758 1.5 10.0003 1.5C10.6026 1.5 11.1971 1.56273 11.7769 1.68607C11.9752 1.72824 12.1284 1.88591 12.1648 2.08529L12.5313 4.09165C12.6303 4.63497 13.1511 4.9951 13.6944 4.89601C13.7479 4.88627 13.8004 4.87219 13.8515 4.85395L15.7698 4.16802C15.9605 4.09984 16.1734 4.15339 16.3092 4.30364C17.1119 5.19213 17.7202 6.24053 18.0895 7.38266C18.1518 7.57534 18.0918 7.78658 17.9374 7.91764L16.3825 9.23773C15.9615 9.5952 15.9101 10.2263 16.2675 10.6473C16.3027 10.6887 16.3411 10.7271 16.3825 10.7623L17.9374 12.0824C18.0918 12.2134 18.1518 12.4247 18.0895 12.6173C17.7202 13.7595 17.1119 14.8079 16.3092 15.6964C16.1734 15.8466 15.9605 15.9002 15.7698 15.832L13.8515 15.1461C13.3315 14.9602 12.7592 15.2312 12.5733 15.7512C12.5551 15.8024 12.541 15.8549 12.5312 15.9085L12.1648 17.9147C12.1284 18.1141 11.9752 18.2718 11.7769 18.3139C11.1971 18.4373 10.6026 18.5 10.0003 18.5C9.39758 18.5 8.8027 18.4372 8.22251 18.3137C8.02422 18.2715 7.87104 18.1137 7.83471 17.9143L7.46926 15.9084C7.37018 15.365 6.8494 15.0049 6.30608 15.104C6.25265 15.1137 6.20011 15.1278 6.14906 15.1461L4.23069 15.832C4.04002 15.9002 3.82707 15.8466 3.69133 15.6964C2.88863 14.8079 2.28028 13.7595 1.91099 12.6173C1.84869 12.4247 1.90876 12.2134 2.06313 12.0824L3.61798 10.7623C4.03897 10.4048 4.09046 9.77373 3.73299 9.35274C3.69784 9.31135 3.65937 9.27288 3.618 9.23775L2.06313 7.91764C1.90876 7.78658 1.84869 7.57534 1.91099 7.38266ZM2.97154 7.37709L4.26523 8.47546C4.34803 8.54576 4.42496 8.62269 4.49526 8.70548C5.2102 9.54746 5.10721 10.8096 4.26521 11.5246L2.97154 12.6229C3.26359 13.4051 3.68504 14.1322 4.21648 14.7751L5.81246 14.2044C5.91473 14.1679 6.01982 14.1397 6.12667 14.1202C7.21332 13.922 8.25487 14.6423 8.45305 15.729L8.75702 17.3975C9.16489 17.4655 9.58024 17.5 10.0003 17.5C10.42 17.5 10.8351 17.4656 11.2427 17.3976L11.5475 15.7289C11.567 15.6221 11.5951 15.517 11.6317 15.4147C12.0034 14.3746 13.1479 13.8327 14.1881 14.2044L15.784 14.7751C16.3155 14.1322 16.7369 13.4051 17.029 12.6229L15.7353 11.5245C15.6525 11.4542 15.5756 11.3773 15.5053 11.2945C14.7903 10.4525 14.8933 9.1904 15.7353 8.47544L17.029 7.37709C16.7369 6.59486 16.3155 5.86783 15.784 5.22494L14.1881 5.79559C14.0858 5.83214 13.9807 5.8603 13.8738 5.87979C12.7872 6.07796 11.7456 5.3577 11.5475 4.27119L11.2427 2.60235C10.8351 2.53443 10.42 2.5 10.0003 2.5C9.58024 2.5 9.16489 2.53448 8.75702 2.60249L8.45304 4.27105C8.43355 4.37791 8.40539 4.48299 8.36884 4.58527C7.99714 5.62542 6.8526 6.1673 5.81237 5.79556L4.21648 5.22494C3.68504 5.86783 3.26359 6.59486 2.97154 7.37709ZM7.50026 10C7.50026 8.61929 8.61954 7.5 10.0003 7.5C11.381 7.5 12.5003 8.61929 12.5003 10C12.5003 11.3807 11.381 12.5 10.0003 12.5C8.61954 12.5 7.50026 11.3807 7.50026 10ZM8.50026 10C8.50026 10.8284 9.17183 11.5 10.0003 11.5C10.8287 11.5 11.5003 10.8284 11.5003 10C11.5003 9.17157 10.8287 8.5 10.0003 8.5C9.17183 8.5 8.50026 9.17157 8.50026 10Z"),
			new IconSource("settings", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12.0122 2.25C12.7462 2.25846 13.4773 2.34326 14.1937 2.50304C14.5064 2.57279 14.7403 2.83351 14.7758 3.15196L14.946 4.67881C15.0231 5.37986 15.615 5.91084 16.3206 5.91158C16.5103 5.91188 16.6979 5.87238 16.8732 5.79483L18.2738 5.17956C18.5651 5.05159 18.9055 5.12136 19.1229 5.35362C20.1351 6.43464 20.8889 7.73115 21.3277 9.14558C21.4223 9.45058 21.3134 9.78203 21.0564 9.9715L19.8149 10.8866C19.4607 11.1468 19.2516 11.56 19.2516 11.9995C19.2516 12.4389 19.4607 12.8521 19.8157 13.1129L21.0582 14.0283C21.3153 14.2177 21.4243 14.5492 21.3297 14.8543C20.8911 16.2685 20.1377 17.5649 19.1261 18.6461C18.9089 18.8783 18.5688 18.9483 18.2775 18.8206L16.8712 18.2045C16.4688 18.0284 16.0068 18.0542 15.6265 18.274C15.2463 18.4937 14.9933 18.8812 14.945 19.3177L14.7759 20.8444C14.741 21.1592 14.5122 21.4182 14.204 21.4915C12.7556 21.8361 11.2465 21.8361 9.79803 21.4915C9.48991 21.4182 9.26105 21.1592 9.22618 20.8444L9.05736 19.32C9.00777 18.8843 8.75434 18.498 8.37442 18.279C7.99451 18.06 7.5332 18.0343 7.1322 18.2094L5.72557 18.8256C5.43422 18.9533 5.09403 18.8833 4.87678 18.6509C3.86462 17.5685 3.11119 16.2705 2.6732 14.8548C2.57886 14.5499 2.68786 14.2186 2.94485 14.0293L4.18818 13.1133C4.54232 12.8531 4.75147 12.4399 4.75147 12.0005C4.75147 11.561 4.54232 11.1478 4.18771 10.8873L2.94516 9.97285C2.6878 9.78345 2.5787 9.45178 2.67337 9.14658C3.11212 7.73215 3.86594 6.43564 4.87813 5.35462C5.09559 5.12236 5.43594 5.05259 5.72724 5.18056L7.12762 5.79572C7.53056 5.97256 7.9938 5.94585 8.37577 5.72269C8.75609 5.50209 9.00929 5.11422 9.05817 4.67764L9.22824 3.15196C9.26376 2.83335 9.49786 2.57254 9.8108 2.50294C10.5281 2.34342 11.26 2.25865 12.0122 2.25ZM12.0124 3.7499C11.5583 3.75524 11.1056 3.79443 10.6578 3.86702L10.5489 4.84418C10.4471 5.75368 9.92003 6.56102 9.13042 7.01903C8.33597 7.48317 7.36736 7.53903 6.52458 7.16917L5.62629 6.77456C5.05436 7.46873 4.59914 8.25135 4.27852 9.09168L5.07632 9.67879C5.81513 10.2216 6.25147 11.0837 6.25147 12.0005C6.25147 12.9172 5.81513 13.7793 5.0771 14.3215L4.27805 14.9102C4.59839 15.752 5.05368 16.5361 5.626 17.2316L6.53113 16.8351C7.36923 16.4692 8.33124 16.5227 9.12353 16.9794C9.91581 17.4361 10.4443 18.2417 10.548 19.1526L10.657 20.1365C11.5466 20.2878 12.4555 20.2878 13.3451 20.1365L13.4541 19.1527C13.5549 18.2421 14.0828 17.4337 14.876 16.9753C15.6692 16.5168 16.6332 16.463 17.4728 16.8305L18.3772 17.2267C18.949 16.5323 19.4041 15.7495 19.7247 14.909L18.9267 14.3211C18.1879 13.7783 17.7516 12.9162 17.7516 11.9995C17.7516 11.0827 18.1879 10.2206 18.9258 9.67847L19.7227 9.09109C19.4021 8.25061 18.9468 7.46784 18.3748 6.77356L17.4783 7.16737C17.113 7.32901 16.7178 7.4122 16.3187 7.41158C14.849 7.41004 13.6155 6.30355 13.4551 4.84383L13.3462 3.8667C12.9007 3.7942 12.4526 3.75512 12.0124 3.7499ZM11.9997 8.24995C14.0708 8.24995 15.7497 9.92888 15.7497 12C15.7497 14.071 14.0708 15.75 11.9997 15.75C9.92863 15.75 8.2497 14.071 8.2497 12C8.2497 9.92888 9.92863 8.24995 11.9997 8.24995ZM11.9997 9.74995C10.7571 9.74995 9.7497 10.7573 9.7497 12C9.7497 13.2426 10.7571 14.25 11.9997 14.25C13.2423 14.25 14.2497 13.2426 14.2497 12C14.2497 10.7573 13.2423 9.74995 11.9997 9.74995Z"),
			new IconSource("settings", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M14 9.5C11.5147 9.5 9.5 11.5147 9.5 14C9.5 16.4853 11.5147 18.5 14 18.5C15.3488 18.5 16.559 17.9066 17.3838 16.9666C18.0787 16.1745 18.5 15.1365 18.5 14C18.5 13.5401 18.431 13.0962 18.3028 12.6783C17.7382 10.838 16.0253 9.5 14 9.5ZM11 14C11 12.3431 12.3431 11 14 11C15.6569 11 17 12.3431 17 14C17 15.6569 15.6569 17 14 17C12.3431 17 11 15.6569 11 14ZM21.7093 22.3947L19.9818 21.6364C19.4876 21.4197 18.9071 21.4514 18.44 21.7219C17.9729 21.9923 17.675 22.4692 17.6157 23.0065L17.408 24.8855C17.3651 25.2729 17.084 25.5917 16.7055 25.6819C14.9263 26.106 13.0725 26.106 11.2933 25.6819C10.9148 25.5917 10.6336 25.2729 10.5908 24.8855L10.3834 23.0093C10.3225 22.473 10.0112 21.9976 9.54452 21.728C9.07783 21.4585 8.51117 21.4269 8.01859 21.6424L6.29071 22.4009C5.93281 22.558 5.51493 22.4718 5.24806 22.1858C4.00474 20.8536 3.07924 19.2561 2.54122 17.5136C2.42533 17.1383 2.55922 16.7307 2.8749 16.4976L4.40219 15.3703C4.83721 15.05 5.09414 14.5415 5.09414 14.0006C5.09414 13.4597 4.83721 12.9512 4.40162 12.6305L2.87529 11.5051C2.55914 11.272 2.42513 10.8638 2.54142 10.4881C3.08038 8.74728 4.00637 7.15157 5.24971 5.82108C5.51684 5.53522 5.93492 5.44935 6.29276 5.60685L8.01296 6.36398C8.50793 6.58162 9.07696 6.54875 9.54617 6.27409C10.0133 6.00258 10.3244 5.52521 10.3844 4.98787L10.5933 3.11011C10.637 2.71797 10.9245 2.39697 11.3089 2.31131C12.19 2.11498 13.0891 2.01065 14.0131 2C14.9147 2.01041 15.8128 2.11478 16.6928 2.31143C17.077 2.39728 17.3643 2.71817 17.4079 3.11011L17.617 4.98931C17.7116 5.85214 18.4387 6.50566 19.3055 6.50657C19.5385 6.50694 19.769 6.45832 19.9843 6.36288L21.7048 5.60562C22.0626 5.44812 22.4807 5.53399 22.7478 5.81984C23.9912 7.15034 24.9172 8.74605 25.4561 10.4869C25.5723 10.8623 25.4386 11.2702 25.1228 11.5034L23.5978 12.6297C23.1628 12.9499 22.9 13.4585 22.9 13.9994C22.9 14.5402 23.1628 15.0488 23.5988 15.3697L25.1251 16.4964C25.441 16.7296 25.5748 17.1376 25.4586 17.513C24.9198 19.2536 23.9944 20.8491 22.7517 22.1799C22.4849 22.4657 22.0671 22.5518 21.7093 22.3947ZM16.263 22.1965C16.4982 21.4684 16.9889 20.8288 17.6884 20.4238C18.5702 19.9132 19.6536 19.8546 20.5841 20.2626L21.9281 20.8526C22.791 19.8537 23.4593 18.7013 23.8981 17.4551L22.7095 16.5777L22.7086 16.577C21.898 15.9799 21.4 15.0276 21.4 13.9994C21.4 12.9718 21.8974 12.0195 22.7073 11.4227L22.7085 11.4217L23.8957 10.545C23.4567 9.29874 22.7881 8.1463 21.9248 7.14764L20.5922 7.73419L20.5899 7.73521C20.1844 7.91457 19.7472 8.00716 19.3039 8.00657C17.6715 8.00447 16.3046 6.77425 16.1261 5.15459L16.1259 5.15285L15.9635 3.69298C15.3202 3.57322 14.6677 3.50866 14.013 3.50011C13.3389 3.50885 12.6821 3.57361 12.0377 3.69322L11.8751 5.15446C11.7625 6.16266 11.1793 7.05902 10.3019 7.5698C9.41937 8.08554 8.34453 8.14837 7.40869 7.73688L6.07273 7.14887C5.20949 8.14745 4.54092 9.29977 4.10196 10.5459L5.29181 11.4232C6.11115 12.0268 6.59414 12.9836 6.59414 14.0006C6.59414 15.0172 6.11142 15.9742 5.29237 16.5776L4.10161 17.4565C4.54002 18.7044 5.2085 19.8584 6.07205 20.8587L7.41742 20.2681C8.34745 19.8613 9.41573 19.9214 10.2947 20.4291C11.174 20.9369 11.7593 21.8319 11.8738 22.84L11.8744 22.8445L12.0362 24.3087C13.3326 24.5638 14.6662 24.5638 15.9626 24.3087L16.1247 22.8417C16.1491 22.6217 16.1955 22.4054 16.263 22.1965Z"),
			new IconSource("settings", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M16 11.0002C13.2386 11.0002 11 13.2388 11 16.0002C11 18.7616 13.2386 21.0002 16 21.0002C18.7614 21.0002 21 18.7616 21 16.0002C21 13.2388 18.7614 11.0002 16 11.0002ZM13 16.0002C13 14.3434 14.3431 13.0002 16 13.0002C17.6569 13.0002 19 14.3434 19 16.0002C19 17.6571 17.6569 19.0002 16 19.0002C14.3431 19.0002 13 17.6571 13 16.0002ZM12.8403 29.6282C13.8754 29.8755 14.936 30.0004 16.0003 30.0002C17.0633 29.9986 18.1225 29.8727 19.1563 29.6252C19.4531 29.555 19.721 29.3947 19.9231 29.1663C20.1252 28.9378 20.2518 28.6524 20.2853 28.3492L20.5202 26.2192C20.5538 25.8682 20.7123 25.541 20.9669 25.2971C21.2214 25.0531 21.5551 24.9087 21.9072 24.8902C22.1404 24.8804 22.3727 24.9242 22.5863 25.0182L24.5412 25.8752C24.728 25.9573 24.9297 25.9997 25.1337 25.9997C25.3377 25.9997 25.5395 25.9573 25.7262 25.8752C25.9104 25.7951 26.0759 25.6776 26.2123 25.5302C27.6568 23.9738 28.735 22.1141 29.3682 20.0872C29.4589 19.794 29.4561 19.4799 29.3602 19.1884C29.2644 18.8969 29.0802 18.6424 28.8333 18.4602L27.1043 17.1852C26.8196 16.978 26.618 16.6762 26.5357 16.3339C26.4535 15.9916 26.4958 15.6311 26.6553 15.3172C26.7622 15.108 26.917 14.9271 27.1073 14.7892L28.8273 13.5182C29.0757 13.3366 29.2611 13.0818 29.3574 12.7894C29.4537 12.4971 29.4561 12.182 29.3642 11.8882C28.7321 9.86098 27.6533 8.00132 26.2073 6.4462C25.9982 6.22264 25.7249 6.06942 25.4251 6.00763C25.1253 5.94584 24.8137 5.97853 24.5332 6.1012L22.5873 6.95721C22.374 7.05019 22.1418 7.09167 21.9095 7.07831C21.6772 7.06495 21.4513 6.9971 21.2501 6.88028C21.0488 6.76346 20.8779 6.60092 20.7511 6.40582C20.6244 6.21072 20.5453 5.98853 20.5202 5.7572L20.2843 3.63721C20.2513 3.33026 20.1228 3.0414 19.9171 2.81125C19.7113 2.5811 19.4386 2.42126 19.1372 2.35422C18.1104 2.12478 17.0624 2.00311 16.0102 1.99121C14.9519 2.00374 13.8976 2.12539 12.8642 2.35422C12.5623 2.42058 12.289 2.5805 12.0833 2.81122C11.8771 3.03978 11.7488 3.328 11.7172 3.63422L11.4802 5.7562C11.4543 5.98787 11.3746 6.21027 11.2473 6.40558C11.12 6.60088 10.9488 6.76366 10.7473 6.88089C10.5458 6.99812 10.3196 7.06654 10.0869 7.08066C9.85422 7.09478 9.62145 7.05422 9.40725 6.96222L7.46126 6.10519C7.27614 6.02628 7.07698 5.98563 6.87574 5.98563C6.67451 5.98563 6.47536 6.02628 6.29025 6.10519C6.1005 6.18482 5.93001 6.30417 5.79025 6.4552C4.34425 8.0122 3.26489 9.87299 2.63125 11.9012C2.54103 12.1942 2.54415 12.508 2.64018 12.7992C2.73621 13.0903 2.92041 13.3444 3.16725 13.5262L4.89225 14.7982C5.17575 15.0068 5.37612 15.3092 5.45778 15.6515C5.53943 15.9939 5.49709 16.3541 5.33825 16.6682C5.2334 16.8767 5.08054 17.0573 4.89225 17.1952L3.16725 18.4702C2.91967 18.6523 2.73507 18.9071 2.63917 19.1991C2.54327 19.4911 2.54085 19.8058 2.63224 20.0992C3.26559 22.1287 4.34496 23.9909 5.79126 25.5492C5.92796 25.6957 6.0934 25.8125 6.27725 25.8922C6.46402 25.9729 6.66532 26.0145 6.86876 26.0145C7.0722 26.0145 7.27348 25.9729 7.46025 25.8922L9.41525 25.0332C9.62848 24.9411 9.86036 24.9002 10.0922 24.914C10.3241 24.9278 10.5495 24.9957 10.7504 25.1125C10.9512 25.2292 11.1219 25.3913 11.2486 25.586C11.3754 25.7806 11.4547 26.0023 11.4802 26.2332L11.7153 28.3592C11.7496 28.6607 11.876 28.9443 12.0773 29.1714C12.2786 29.3985 12.545 29.558 12.8403 29.6282ZM18.3413 27.7622C16.7983 28.0787 15.2072 28.0787 13.6643 27.7622L13.4693 26.0222C13.374 25.171 12.9681 24.3848 12.3292 23.8142C11.8291 23.3738 11.2147 23.0839 10.5568 22.9777C9.89898 22.8715 9.22451 22.9535 8.61125 23.2142L7.00525 23.9142C5.96924 22.7279 5.17199 21.3527 4.65725 19.8642L6.08125 18.8122C6.52119 18.488 6.8788 18.065 7.12526 17.5773C7.37171 17.0895 7.50012 16.5507 7.50012 16.0042C7.50012 15.4577 7.37171 14.9189 7.12526 14.4312C6.8788 13.9434 6.52119 13.5204 6.08125 13.1962L4.65925 12.1472C5.17459 10.6602 5.9718 9.28641 7.00725 8.1012L8.60725 8.80121C9.04825 8.99595 9.52516 9.09611 10.0072 9.09521C10.8657 9.09252 11.6933 8.7744 12.3325 8.20135C12.9718 7.62831 13.3781 6.8403 13.4743 5.98721L13.6683 4.2402C14.4419 4.09065 15.2273 4.01034 16.0152 4.00021C16.7972 4.01013 17.5766 4.0901 18.3443 4.2392L18.5313 5.97922C18.6237 6.83322 19.0286 7.62285 19.6683 8.1962C20.1687 8.63916 20.7848 8.93076 21.4447 9.03699C22.1045 9.14322 22.781 9.05971 23.3952 8.7962L24.9953 8.09622C26.0318 9.28121 26.8294 10.6554 27.3442 12.1432L25.9223 13.1892C25.4801 13.5123 25.1203 13.9351 24.8724 14.4234C24.6244 14.9117 24.4951 15.4516 24.4951 15.9992C24.4951 16.5468 24.6244 17.0867 24.8724 17.575C25.1203 18.0633 25.4801 18.4861 25.9223 18.8092L27.3442 19.8582C26.8292 21.3439 26.0334 22.7168 25.0003 23.9022L23.4002 23.2022C22.8996 22.9794 22.3534 22.8782 21.8062 22.9069C21.259 22.9356 20.7264 23.0934 20.2518 23.3673C19.7773 23.6413 19.3742 24.0237 19.0757 24.4832C18.7772 24.9427 18.5917 25.4663 18.5343 26.0112L18.3413 27.7622Z"),
			new IconSource("arrow_repeat_all_off", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M2.78431 2.08834L2.85355 2.14619L13.8536 13.1462C14.0488 13.3415 14.0488 13.658 13.8536 13.8533C13.68 14.0269 13.4106 14.0462 13.2157 13.9112L13.1464 13.8533L11.1305 11.8378C10.8437 11.9221 10.5435 11.9752 10.2339 11.993L10 11.9997L6.707 11.999L7.85525 13.1479C8.02882 13.3215 8.0481 13.5909 7.91311 13.7858L7.85525 13.855C7.68169 14.0286 7.41226 14.0479 7.21739 13.9129L7.14815 13.855L5.14645 11.8533C4.97288 11.6797 4.9536 11.4103 5.08859 11.2154L5.14645 11.1462L7.14815 9.14449C7.34341 8.94923 7.65999 8.94923 7.85525 9.14449C8.02882 9.31806 8.0481 9.58748 7.91311 9.78235L7.85525 9.8516L6.707 10.999L10 10.9997C10.0941 10.9997 10.1871 10.9954 10.279 10.9869L4.62614 5.33211C3.66034 5.83052 3 6.83802 3 7.99975C3 8.8193 3.32863 9.56209 3.8613 10.1035C3.94745 10.1919 4 10.3134 4 10.4472C4 10.7234 3.77614 10.9472 3.5 10.9472C3.36244 10.9472 3.23785 10.8917 3.14745 10.8018C2.4379 10.0823 2 9.09215 2 7.99975C2 6.56381 2.75664 5.30459 3.89297 4.59904L2.14645 2.8533C1.95118 2.65804 1.95118 2.34146 2.14645 2.14619C2.32001 1.97263 2.58944 1.95334 2.78431 2.08834ZM12.5 5.04865C12.6227 5.04865 12.7351 5.09287 12.8221 5.16624L12.8935 5.23788C13.579 5.95588 14 6.92865 14 7.99975C14 9.07744 13.5738 10.0556 12.8808 10.7748L12.174 10.0671C12.6858 9.52898 13 8.80105 13 7.99975C13 7.17778 12.6694 6.43303 12.134 5.89117C12.0522 5.80305 12 5.68194 12 5.54865C12 5.2725 12.2239 5.04865 12.5 5.04865ZM8.14645 2.14449C8.32001 1.97093 8.58944 1.95164 8.78431 2.08664L8.85355 2.14449L10.8553 4.14619L10.9131 4.21544C11.0312 4.38595 11.0312 4.61354 10.9131 4.78405L10.8553 4.8533L8.907 6.80005L8.2 6.09305L9.294 4.99905H7.105L6.105 3.99905H9.294L8.14645 2.8516L8.08859 2.78235C7.9536 2.58748 7.97288 2.31806 8.14645 2.14449Z"),
			new IconSource("arrow_repeat_all_off", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M2.14645 2.14645C2.32001 1.97288 2.58944 1.9536 2.78431 2.08859L2.85355 2.14645L17.8536 17.1464L17.9114 17.2157C18.0464 17.4106 18.0271 17.68 17.8536 17.8536C17.68 18.0271 17.4106 18.0464 17.2157 17.9114L17.1464 17.8536L14.158 14.8652C13.8792 14.9313 13.5915 14.9741 13.2968 14.9913L13 15H7C6.89793 15 6.79657 14.9969 6.69601 14.9909L8.35355 16.6464C8.52712 16.82 8.5464 17.0894 8.41141 17.2843L8.35355 17.3536C8.17999 17.5271 7.91056 17.5464 7.71569 17.4114L7.64645 17.3536L5.14645 14.8536C4.97288 14.68 4.9536 14.4106 5.08859 14.2157L5.14645 14.1464L7.64645 11.6464C7.84171 11.4512 8.15829 11.4512 8.35355 11.6464C8.52712 11.82 8.5464 12.0894 8.41141 12.2843L8.35355 12.3536L6.71799 13.9902L6.85838 13.9975L7 14H13C13.0951 14 13.1895 13.9967 13.283 13.9901L5.5602 6.26696C4.06248 6.845 3 8.29842 3 10C3 10.9563 3.33557 11.8342 3.89537 12.5224C3.95954 12.606 4 12.7139 4 12.8313C4 13.1074 3.77614 13.3313 3.5 13.3313C3.333 13.3313 3.18512 13.2494 3.09432 13.1236C2.40993 12.2676 2 11.1817 2 10C2 8.02733 3.14239 6.32148 4.80169 5.50793L2.14645 2.85355L2.08859 2.78431C1.9536 2.58944 1.97288 2.32001 2.14645 2.14645ZM16.5 6.67122C16.6163 6.67122 16.7233 6.71092 16.8082 6.77751L16.8755 6.84057L16.8918 6.86057C17.5849 7.71877 18 8.81091 18 10C18 11.6082 17.2407 13.0391 16.0611 13.9537L15.3465 13.2398C16.3484 12.5129 17 11.3325 17 10C17 9.04652 16.6664 8.17096 16.1095 7.48367C16.0414 7.39911 16 7.29 16 7.17122C16 6.89508 16.2239 6.67122 16.5 6.67122ZM12.2843 2.58859L12.3536 2.64645L14.8536 5.14645C15.0271 5.32001 15.0464 5.58944 14.9114 5.78431L14.8536 5.85355L12.3536 8.35355C12.1583 8.54882 11.8417 8.54882 11.6464 8.35355C11.4729 8.17999 11.4536 7.91056 11.5886 7.71569L11.6464 7.64645L13.283 6.00986L13.1421 6.00248L13 6H8.106L7.106 5H13C13.1021 5 13.2034 5.00306 13.304 5.00909L11.6464 3.35355C11.4729 3.17999 11.4536 2.91056 11.5886 2.71569L11.6464 2.64645C11.82 2.47288 12.0894 2.4536 12.2843 2.58859Z"),
			new IconSource("arrow_repeat_all_off", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.19621 2.14705L3.28033 2.21967L21.7803 20.7197C22.0732 21.0126 22.0732 21.4874 21.7803 21.7803C21.5141 22.0466 21.0974 22.0708 20.8038 21.8529L20.7197 21.7803L17.211 18.2725C16.7749 18.3911 16.3205 18.4656 15.853 18.4906L15.5 18.5H8.564L10.4634 20.3991L10.5305 20.4759C10.7542 20.7699 10.7319 21.1913 10.4634 21.4598C10.1971 21.726 9.78044 21.7502 9.48683 21.5324L9.40271 21.4598L6.22073 18.2778L6.15361 18.201C5.95224 17.9364 5.95023 17.5686 6.14756 17.302L6.22073 17.2171L9.40271 14.0351L9.47951 13.968C9.74409 13.7667 10.1119 13.7646 10.3785 13.962L10.4634 14.0351L10.5305 14.1119C10.7319 14.3765 10.7339 14.7443 10.5365 15.0109L10.4634 15.0958L8.558 17H15.5C15.642 17 15.7825 16.9941 15.9215 16.9825L6.40042 7.46086C4.68788 8.25433 3.5 9.98844 3.5 12C3.5 13.296 3.99309 14.4769 4.80189 15.3652C4.92475 15.4972 5 15.6758 5 15.8718C5 16.286 4.66421 16.6218 4.25 16.6218C4.02803 16.6218 3.82858 16.5254 3.69126 16.3722C2.6407 15.2192 2 13.6844 2 12C2 9.57872 3.32389 7.46665 5.28729 6.34818L2.21967 3.28033C1.92678 2.98744 1.92678 2.51256 2.21967 2.21967C2.48594 1.9534 2.9026 1.9292 3.19621 2.14705ZM19.75 7.37814C19.9691 7.37814 20.1662 7.47209 20.3034 7.62189C21.3571 8.77553 22 10.3127 22 12C22 14.0566 21.0449 15.8901 19.554 17.0812L18.4851 16.0115C19.708 15.1 20.5 13.6425 20.5 12C20.5 10.6939 19.9992 9.50473 19.1791 8.61411C19.0677 8.48435 19 8.31416 19 8.12814C19 7.71393 19.3358 7.37814 19.75 7.37814ZM14.5334 2.40255L14.6102 2.46967L17.7922 5.65165C18.0606 5.92014 18.083 6.34154 17.8593 6.63551L17.7922 6.71231L14.6102 9.89429C14.3173 10.1872 13.8424 10.1872 13.5495 9.89429C13.281 9.62581 13.2587 9.20441 13.4824 8.91043L13.5495 8.83363L15.381 7H9.473L7.99365 5.51943C8.08422 5.51245 8.1753 5.50733 8.26687 5.5041L8.5 5.5L15.5211 5.50003L13.5495 3.53033C13.281 3.26184 13.2587 2.84044 13.4824 2.54647L13.5495 2.46967C13.818 2.20118 14.2394 2.17881 14.5334 2.40255Z"),
			new IconSource("alert", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M8.00001 2C5.51473 2 3.50001 4.01472 3.50001 6.5V8.9014L2.5357 11.3145C2.47415 11.4685 2.49299 11.643 2.58601 11.7804C2.67904 11.9177 2.83412 12 3 12H6C6 13.1084 6.89164 14 8 14C9.10836 14 10 13.1084 10 12H13C13.1659 12 13.321 11.9177 13.414 11.7804C13.507 11.643 13.5259 11.4685 13.4643 11.3145L12.5 8.9014V6.5C12.5 4.01472 10.4853 2 8.00001 2ZM9 12C9 12.5561 8.55607 13 8 13C7.44393 13 7 12.5561 7 12H9ZM4.50001 6.5C4.50001 4.567 6.06701 3 8.00001 3C9.933 3 11.5 4.567 11.5 6.5V8.9976C11.5 9.06116 11.5121 9.12413 11.5357 9.18315L12.2617 11H3.73826L4.46431 9.18315C4.48789 9.12413 4.50001 9.06116 4.50001 8.9976V6.5Z"),
			new IconSource("alert", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M9.99766 2C13.1466 2 15.7416 4.33488 15.9821 7.3554L15.9955 7.57762L16 7.80214L15.999 11.398L16.9244 13.6202C16.947 13.6743 16.9647 13.7302 16.9774 13.7871L16.9926 13.8733L17.0013 14.0046C17.0013 14.4526 16.7048 14.8387 16.2521 14.9677L16.1358 14.9945L16.0013 15.0046L12.4996 15.004L12.4946 15.1653C12.4095 16.469 11.3252 17.5 10 17.5C8.67453 17.5 7.58998 16.4685 7.50533 15.1644L7.49962 15.004L3.99891 15.0046C3.91096 15.0046 3.82358 14.993 3.73902 14.9702L3.61456 14.9277C3.20378 14.7567 2.96181 14.3392 3.01221 13.8757L3.0333 13.7483L3.07572 13.6202L3.99902 11.401L4.0001 7.79281L4.0044 7.56824C4.12702 4.45115 6.77104 2 9.99766 2ZM11.4996 15.004H8.49962L8.50697 15.1454C8.57552 15.8581 9.14275 16.425 9.85556 16.4931L10 16.5C10.7797 16.5 11.4205 15.9051 11.4931 15.1445L11.4996 15.004ZM9.99766 3C7.37511 3 5.22717 4.92372 5.01715 7.38498L5.00393 7.59723L5.00002 7.80214V11.5L4.96161 11.6922L3.9989 14.0046L15.9566 14.0066L16.0019 14.0045L15.0384 11.6922L15 11.5L15.0001 7.81241L14.996 7.60831C14.8909 5.0349 12.6947 3 9.99766 3Z"),
			new IconSource("alert", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12.0001 1.99609C16.05 1.99609 19.3568 5.19084 19.4959 9.24515L19.5001 9.49609V13.5931L20.8801 16.7491C20.9492 16.907 20.9848 17.0775 20.9848 17.2499C20.9848 17.9402 20.4252 18.4999 19.7348 18.4999L15.0001 18.5014C15.0001 20.1582 13.657 21.5014 12.0001 21.5014C10.4024 21.5014 9.09645 20.2524 9.0052 18.6776L8.99966 18.4991L4.27498 18.4999C4.10364 18.4999 3.93413 18.4646 3.77697 18.3964C3.14377 18.1213 2.85342 17.3851 3.12846 16.7519L4.50011 13.594V9.49599C4.50071 5.3412 7.8522 1.99609 12.0001 1.99609ZM13.4997 18.4991L10.5001 18.5014C10.5001 19.3298 11.1717 20.0014 12.0001 20.0014C12.7798 20.0014 13.4206 19.4065 13.4932 18.6458L13.4997 18.4991ZM12.0001 3.49609C8.67995 3.49609 6.00059 6.17035 6.00011 9.49609V13.9057L4.65613 16.9999H19.3526L18.0001 13.9067L18.0002 9.50895L17.9965 9.28375C17.8854 6.05027 15.2417 3.49609 12.0001 3.49609Z"),
			new IconSource("alert", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M13.9999 3C18.6097 3 22.3625 6.66899 22.4974 11.246L22.5012 11.5012L22.5009 16.113L23.915 19.757C23.9531 19.8554 23.9789 19.958 23.9919 20.0624L24.0016 20.22C24.0016 20.8827 23.4979 21.4278 22.8525 21.4934L22.7216 21.5L17.5014 21.5011C17.5014 23.4348 15.9337 25.0025 13.9999 25.0025C12.1306 25.0025 10.6034 23.5376 10.5037 21.6932L10.4984 21.499L5.27473 21.5C5.11602 21.5 4.9587 21.4705 4.81078 21.413C4.19311 21.1727 3.86764 20.5057 4.04049 19.8804L4.08177 19.7561L5.49794 16.112L5.4987 11.5012C5.4987 6.80614 9.30483 3 13.9999 3ZM15.9959 21.6504L16.0004 21.499L11.9985 21.5011C11.9985 22.6064 12.8946 23.5025 13.9999 23.5025C15.0551 23.5025 15.9195 22.686 15.9959 21.6504ZM13.9999 4.5C10.2138 4.5 7.12962 7.50532 7.00276 11.2606L6.9987 11.5012V16.2534C6.9987 16.3154 6.99101 16.377 6.9759 16.4369L6.9477 16.5252L5.59594 19.999H22.3999L21.052 16.5246C21.0296 16.467 21.0145 16.4069 21.0069 16.3457L21.0012 16.2534V11.5012C21.0012 7.63456 17.8666 4.5 13.9999 4.5Z"),
			new IconSource("alert", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M16 4C11.0295 4 7.00001 8.02944 7.00001 13V17.8028L5.07141 22.6289C4.94829 22.937 4.98599 23.2861 5.17203 23.5608C5.35807 23.8355 5.66824 24 6 24H12C12 26.2167 13.7833 28 16 28C18.2167 28 20 26.2167 20 24H26C26.3318 24 26.6419 23.8355 26.828 23.5608C27.014 23.2861 27.0517 22.937 26.9286 22.6289L25 17.8028V13C25 8.02944 20.9706 4 16 4ZM18 24C18 25.1121 17.1121 26 16 26C14.8879 26 14 25.1121 14 24H18ZM9.00001 13C9.00001 9.13401 12.134 6 16 6C19.866 6 23 9.13401 23 13V17.9952C23 18.1223 23.0242 18.2483 23.0714 18.3663L24.5235 22H7.47651L8.92861 18.3663C8.97578 18.2483 9.00001 18.1223 9.00001 17.9952V13Z"),
			new IconSource("search", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M11.0195 11.7266C10.0658 12.5217 8.83875 13 7.5 13C4.46243 13 2 10.5376 2 7.5C2 4.46243 4.46243 2 7.5 2C10.5376 2 13 4.46243 13 7.5C13 8.83875 12.5217 10.0658 11.7266 11.0195L14.8535 14.1464C15.0488 14.3417 15.0488 14.6583 14.8535 14.8536C14.6583 15.0488 14.3417 15.0488 14.1464 14.8536L11.0195 11.7266ZM12 7.5C12 5.01472 9.98528 3 7.5 3C5.01472 3 3 5.01472 3 7.5C3 9.98528 5.01472 12 7.5 12C9.98528 12 12 9.98528 12 7.5Z"),
			new IconSource("search", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M13.7291 14.4362C12.5924 15.411 11.115 16 9.5 16C5.91015 16 3 13.0899 3 9.5C3 5.91015 5.91015 3 9.5 3C13.0899 3 16 5.91015 16 9.5C16 11.115 15.411 12.5924 14.4361 13.7292L17.8535 17.1465C18.0487 17.3417 18.0487 17.6583 17.8535 17.8536C17.6799 18.0271 17.4105 18.0464 17.2156 17.9114L17.1464 17.8536L13.7291 14.4362ZM13.0196 13.7266C13.276 13.5128 13.5127 13.2761 13.7265 13.0197C14.5216 12.0659 15 10.8388 15 9.5C15 6.46243 12.5376 4 9.5 4C6.46243 4 4 6.46243 4 9.5C4 12.5376 6.46243 15 9.5 15C10.8388 15 12.0658 14.5217 13.0196 13.7266Z"),
			new IconSource("search", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M16.1017 17.1624C14.717 18.3101 12.9391 19 11 19C6.58172 19 3 15.4183 3 11C3 6.58172 6.58172 3 11 3C15.4183 3 19 6.58172 19 11C19 12.9391 18.3101 14.717 17.1624 16.1018L21.7803 20.7197C22.0732 21.0126 22.0732 21.4874 21.7803 21.7803C21.4874 22.0732 21.0125 22.0732 20.7196 21.7803L16.1017 17.1624ZM17.5 11C17.5 7.41015 14.5899 4.5 11 4.5C7.41015 4.5 4.5 7.41015 4.5 11C4.5 14.5899 7.41015 17.5 11 17.5C14.5899 17.5 17.5 14.5899 17.5 11Z"),
			new IconSource("search", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M18.6664 19.727C17.0069 21.1443 14.8534 22 12.5 22C7.25329 22 3 17.7467 3 12.5C3 7.25329 7.25329 3 12.5 3C17.7467 3 22 7.25329 22 12.5C22 14.8534 21.1443 17.0069 19.727 18.6663L25.7803 24.7197C26.0732 25.0126 26.0732 25.4874 25.7803 25.7803C25.4874 26.0732 25.0126 26.0732 24.7197 25.7803L18.6664 19.727ZM20.5 12.5C20.5 8.08172 16.9183 4.5 12.5 4.5C8.08172 4.5 4.5 8.08172 4.5 12.5C4.5 16.9183 8.08172 20.5 12.5 20.5C16.9183 20.5 20.5 16.9183 20.5 12.5Z"),
			new IconSource("search", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M21.8941 23.3082C19.895 24.9881 17.3156 26 14.5 26C8.14873 26 3 20.8513 3 14.5C3 8.14873 8.14873 3 14.5 3C20.8513 3 26 8.14873 26 14.5C26 17.3283 24.979 19.9181 23.2854 21.921C23.2931 21.912 23.3007 21.903 23.3083 21.8939L29.7073 28.2929C30.0978 28.6834 30.0978 29.3166 29.7073 29.7071C29.3167 30.0976 28.6836 30.0976 28.2931 29.7071L21.8941 23.3082ZM24 14.5C24 9.25329 19.7467 5 14.5 5C9.25329 5 5 9.25329 5 14.5C5 19.7467 9.25329 24 14.5 24C19.7467 24 24 19.7467 24 14.5Z"),
			new IconSource("arrow_sort_down_lines", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M13 1.5C13 1.22386 12.7761 1 12.5 1C12.2239 1 12 1.22386 12 1.5V13.2929L10.8536 12.1464C10.6583 11.9512 10.3417 11.9512 10.1464 12.1464C9.95118 12.3417 9.95118 12.6583 10.1464 12.8536L12.1464 14.8536C12.3417 15.0488 12.6583 15.0488 12.8536 14.8536L14.8536 12.8536C15.0488 12.6583 15.0488 12.3417 14.8536 12.1464C14.6583 11.9512 14.3417 11.9512 14.1464 12.1464L13 13.2929V1.5ZM2.5 3C2.22386 3 2 3.22386 2 3.5C2 3.77614 2.22386 4 2.5 4H9.5C9.77614 4 10 3.77614 10 3.5C10 3.22386 9.77614 3 9.5 3H2.5ZM4.5 6C4.22386 6 4 6.22386 4 6.5C4 6.77614 4.22386 7 4.5 7H9.5C9.77614 7 10 6.77614 10 6.5C10 6.22386 9.77614 6 9.5 6H4.5ZM6 9.5C6 9.22386 6.22386 9 6.5 9H9.5C9.77614 9 10 9.22386 10 9.5C10 9.77614 9.77614 10 9.5 10H6.5C6.22386 10 6 9.77614 6 9.5Z"),
			new IconSource("arrow_sort_down_lines", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M15 2.5C15 2.22386 14.7761 2 14.5 2C14.2239 2 14 2.22386 14 2.5V16.2929L11.8536 14.1464C11.6583 13.9512 11.3417 13.9512 11.1464 14.1464C10.9512 14.3417 10.9512 14.6583 11.1464 14.8536L14.1464 17.8536C14.3417 18.0488 14.6583 18.0488 14.8536 17.8536L17.8536 14.8536C18.0488 14.6583 18.0488 14.3417 17.8536 14.1464C17.6583 13.9512 17.3417 13.9512 17.1464 14.1464L15 16.2929V2.5ZM2.5 4C2.22386 4 2 4.22386 2 4.5C2 4.77614 2.22386 5 2.5 5H11.5C11.7761 5 12 4.77614 12 4.5C12 4.22386 11.7761 4 11.5 4H2.5ZM5 7.5C5 7.22386 5.22386 7 5.5 7H11.5C11.7761 7 12 7.22386 12 7.5C12 7.77614 11.7761 8 11.5 8H5.5C5.22386 8 5 7.77614 5 7.5ZM8.5 10C8.22386 10 8 10.2239 8 10.5C8 10.7761 8.22386 11 8.5 11H11.5C11.7761 11 12 10.7761 12 10.5C12 10.2239 11.7761 10 11.5 10H8.5Z"),
			new IconSource("arrow_sort_down_lines", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M18 2.75C18 2.33579 17.6642 2 17.25 2C16.8358 2 16.5 2.33579 16.5 2.75V19.4393L14.2803 17.2197C13.9874 16.9268 13.5126 16.9268 13.2197 17.2197C12.9268 17.5126 12.9268 17.9874 13.2197 18.2803L16.7197 21.7803C17.0126 22.0732 17.4874 22.0732 17.7803 21.7803L21.2803 18.2803C21.5732 17.9874 21.5732 17.5126 21.2803 17.2197C20.9874 16.9268 20.5126 16.9268 20.2197 17.2197L18 19.4393V2.75ZM2.75 4.5C2.33579 4.5 2 4.83579 2 5.25C2 5.66421 2.33579 6 2.75 6H13.25C13.6642 6 14 5.66421 14 5.25C14 4.83579 13.6642 4.5 13.25 4.5H2.75ZM6 9.25C6 8.83579 6.33579 8.5 6.75 8.5H13.25C13.6642 8.5 14 8.83579 14 9.25C14 9.66421 13.6642 10 13.25 10H6.75C6.33579 10 6 9.66421 6 9.25ZM10.75 12.5C10.3358 12.5 10 12.8358 10 13.25C10 13.6642 10.3358 14 10.75 14H13.25C13.6642 14 14 13.6642 14 13.25C14 12.8358 13.6642 12.5 13.25 12.5H10.75Z"),
			new IconSource("clipboard_task_list_ltr", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M9.35355 7.85356C9.54881 7.65831 9.54882 7.34172 9.35357 7.14646C9.15831 6.95119 8.84173 6.95118 8.64646 7.14644L7.23436 8.55847L6.8372 8.19572C6.6333 8.0095 6.31704 8.02382 6.13081 8.22772C5.94459 8.43162 5.95891 8.74788 6.16281 8.9341L6.91281 9.6191C7.11021 9.79939 7.41451 9.79251 7.60355 9.60347L9.35355 7.85356ZM9.35357 12.1465C9.54882 12.3417 9.54881 12.6583 9.35355 12.8536L7.60355 14.6035C7.41451 14.7925 7.11021 14.7994 6.91281 14.6191L6.16281 13.9341C5.95891 13.7479 5.94459 13.4316 6.13081 13.2277C6.31704 13.0238 6.6333 13.0095 6.8372 13.1957L7.23436 13.5585L8.64646 12.1464C8.84173 11.9512 9.15831 11.9512 9.35357 12.1465ZM11 12.75C10.7239 12.75 10.5 12.9739 10.5 13.25C10.5 13.5261 10.7239 13.75 11 13.75H13.5C13.7761 13.75 14 13.5261 14 13.25C14 12.9739 13.7761 12.75 13.5 12.75H11ZM10.5 8.25C10.5 7.97386 10.7239 7.75 11 7.75H13.5C13.7761 7.75 14 7.97386 14 8.25C14 8.52614 13.7761 8.75 13.5 8.75H11C10.7239 8.75 10.5 8.52614 10.5 8.25ZM8.5 2C7.84689 2 7.29127 2.4174 7.08535 3H5.5C4.67157 3 4 3.67157 4 4.5V16.5C4 17.3284 4.67157 18 5.5 18H14.5C15.3284 18 16 17.3284 16 16.5V4.5C16 3.67157 15.3284 3 14.5 3H12.9146C12.7087 2.4174 12.1531 2 11.5 2H8.5ZM8 3.5C8 3.22386 8.22386 3 8.5 3H11.5C11.7761 3 12 3.22386 12 3.5C12 3.77614 11.7761 4 11.5 4H8.5C8.22386 4 8 3.77614 8 3.5ZM5.5 4H7.08535C7.29127 4.5826 7.84689 5 8.5 5H11.5C12.1531 5 12.7087 4.5826 12.9146 4H14.5C14.7761 4 15 4.22386 15 4.5V16.5C15 16.7761 14.7761 17 14.5 17H5.5C5.22386 17 5 16.7761 5 16.5V4.5C5 4.22386 5.22386 4 5.5 4Z"),
			new IconSource("clipboard_task_list_ltr", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12.5 10.25C12.5 9.83579 12.8358 9.5 13.25 9.5H16.75C17.1642 9.5 17.5 9.83579 17.5 10.25C17.5 10.6642 17.1642 11 16.75 11H13.25C12.8358 11 12.5 10.6642 12.5 10.25ZM13.2501 15C12.8358 15 12.5001 15.3358 12.5001 15.75C12.5001 16.1642 12.8358 16.5 13.2501 16.5H16.7499C17.1642 16.5 17.4999 16.1642 17.4999 15.75C17.4999 15.3358 17.1642 15 16.7499 15H13.2501ZM10.7803 9.78033C11.0732 9.48744 11.0732 9.01256 10.7803 8.71967C10.4874 8.42678 10.0126 8.42678 9.71967 8.71967L8.25 10.1893L7.78033 9.71967C7.48744 9.42678 7.01256 9.42678 6.71967 9.71967C6.42678 10.0126 6.42678 10.4874 6.71967 10.7803L7.71967 11.7803C8.01256 12.0732 8.48744 12.0732 8.78033 11.7803L10.7803 9.78033ZM10.7803 14.2197C11.0732 14.5126 11.0732 14.9874 10.7803 15.2803L8.78033 17.2803C8.48744 17.5732 8.01256 17.5732 7.71967 17.2803L6.71967 16.2803C6.42678 15.9874 6.42678 15.5126 6.71967 15.2197C7.01256 14.9268 7.48744 14.9268 7.78033 15.2197L8.25 15.6893L9.71967 14.2197C10.0126 13.9268 10.4874 13.9268 10.7803 14.2197ZM15.994 4.08449C15.9093 2.91913 14.937 2 13.75 2H10.25C9.09205 2 8.13841 2.87472 8.01379 3.99944L6.25 4C5.00736 4 4 5.00736 4 6.25V19.75C4 20.9926 5.00736 22 6.25 22H17.75C18.9926 22 20 20.9926 20 19.75V6.25C20 5.00736 18.9926 4 17.75 4L15.9862 3.99944C15.9893 4.02764 15.9919 4.05599 15.994 4.08449ZM15.9948 4.09595L16 4.25C16 4.19822 15.9983 4.14685 15.9948 4.09595ZM10.25 6.5H13.75C14.53 6.5 15.2174 6.10307 15.621 5.50016L17.75 5.5C18.1642 5.5 18.5 5.83579 18.5 6.25V19.75C18.5 20.1642 18.1642 20.5 17.75 20.5H6.25C5.83579 20.5 5.5 20.1642 5.5 19.75V6.25C5.5 5.83579 5.83579 5.5 6.25 5.5L8.37902 5.5002C8.78267 6.1031 9.46997 6.5 10.25 6.5ZM10.25 3.5H13.75C14.1642 3.5 14.5 3.83579 14.5 4.25C14.5 4.66421 14.1642 5 13.75 5H10.25C9.83579 5 9.5 4.66421 9.5 4.25C9.5 3.83579 9.83579 3.5 10.25 3.5Z"),
			new IconSource("text_bullet_list_add", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M3 5.5C3.55228 5.5 4 5.05228 4 4.5C4 3.94772 3.55228 3.5 3 3.5C2.44772 3.5 2 3.94772 2 4.5C2 5.05228 2.44772 5.5 3 5.5ZM6 4.5C6 4.22386 6.22386 4 6.5 4H17.5C17.7761 4 18 4.22386 18 4.5C18 4.77614 17.7761 5 17.5 5H6.5C6.22386 5 6 4.77614 6 4.5ZM6 9.5C6 9.22386 6.22386 9 6.5 9H17.5C17.7761 9 18 9.22386 18 9.5C18 9.77614 17.7761 10 17.5 10H6.5C6.22386 10 6 9.77614 6 9.5ZM10.0224 15C10.0534 14.6566 10.1159 14.3223 10.207 14H6.5C6.22386 14 6 14.2239 6 14.5C6 14.7761 6.22386 15 6.5 15H10.0224ZM4 14.5C4 15.0523 3.55228 15.5 3 15.5C2.44772 15.5 2 15.0523 2 14.5C2 13.9477 2.44772 13.5 3 13.5C3.55228 13.5 4 13.9477 4 14.5ZM3 10.5C3.55228 10.5 4 10.0523 4 9.5C4 8.94772 3.55228 8.5 3 8.5C2.44772 8.5 2 8.94772 2 9.5C2 10.0523 2.44772 10.5 3 10.5ZM20 15.5C20 17.9853 17.9853 20 15.5 20C13.0147 20 11 17.9853 11 15.5C11 13.0147 13.0147 11 15.5 11C17.9853 11 20 13.0147 20 15.5ZM16 13.5C16 13.2239 15.7761 13 15.5 13C15.2239 13 15 13.2239 15 13.5V15H13.5C13.2239 15 13 15.2239 13 15.5C13 15.7761 13.2239 16 13.5 16H15V17.5C15 17.7761 15.2239 18 15.5 18C15.7761 18 16 17.7761 16 17.5V16H17.5C17.7761 16 18 15.7761 18 15.5C18 15.2239 17.7761 15 17.5 15H16V13.5Z"),
			new IconSource("text_bullet_list_add", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M4.49858 18.7493C4.49858 18.0593 3.93925 17.5 3.24929 17.5C2.55933 17.5 2 18.0593 2 18.7493C2 19.4393 2.55933 19.9986 3.24929 19.9986C3.93925 19.9986 4.49858 19.4393 4.49858 18.7493ZM12.019 18H6.75L6.64823 18.0068C6.28215 18.0565 6 18.3703 6 18.75C6 19.1642 6.33579 19.5 6.75 19.5H12.3137C12.1591 19.0218 12.0585 18.5195 12.019 18ZM13.8091 13H6.75C6.33579 13 6 12.6642 6 12.25C6 11.8703 6.28215 11.5565 6.64823 11.5068L6.75 11.5H15.9944C15.1647 11.8469 14.4222 12.3609 13.8091 13ZM4.49858 12.2493C4.49858 11.5593 3.93925 11 3.24929 11C2.55933 11 2 11.5593 2 12.2493C2 12.9393 2.55933 13.4986 3.24929 13.4986C3.93925 13.4986 4.49858 12.9393 4.49858 12.2493ZM4.49858 5.74929C4.49858 5.05933 3.93925 4.5 3.24929 4.5C2.55933 4.5 2 5.05933 2 5.74929C2 6.43925 2.55933 6.99858 3.24929 6.99858C3.93925 6.99858 4.49858 6.43925 4.49858 5.74929ZM21.25 5H6.75L6.64823 5.00685C6.28215 5.05651 6 5.3703 6 5.75C6 6.16421 6.33579 6.5 6.75 6.5H21.25L21.3518 6.49315C21.7178 6.44349 22 6.1297 22 5.75C22 5.33579 21.6642 5 21.25 5ZM24 17.4995C24 14.4619 21.5376 11.9995 18.5 11.9995C15.4624 11.9995 13 14.4619 13 17.4995C13 20.5371 15.4624 22.9995 18.5 22.9995C21.5376 22.9995 24 20.5371 24 17.4995ZM19.0006 17.9995L19.0011 20.503C19.0011 20.7792 18.7773 21.003 18.5011 21.003C18.225 21.003 18.0011 20.7792 18.0011 20.503L18.0006 17.9995H15.4956C15.2197 17.9995 14.9961 17.7757 14.9961 17.4995C14.9961 17.2234 15.2197 16.9995 15.4956 16.9995H18.0005L18 14.4988C18 14.2226 18.2239 13.9988 18.5 13.9988C18.7761 13.9988 19 14.2226 19 14.4988L19.0005 16.9995H21.4966C21.7725 16.9995 21.9961 17.2234 21.9961 17.4995C21.9961 17.7757 21.7725 17.9995 21.4966 17.9995H19.0006Z"),
			new IconSource("folder_open", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M2 4.5V9.10022L2.92389 7.5C3.45979 6.5718 4.45017 6 5.52196 6L11.9146 6C11.7087 5.4174 11.1531 5 10.5 5H7C6.86739 5 6.74021 4.94732 6.64645 4.85355L4.93934 3.14645C4.84557 3.05268 4.71839 3 4.58579 3H3.5C2.67157 3 2 3.67157 2 4.5ZM7.06895 13.9953C7.04641 13.9984 7.02339 14 7 14H3.5C2.11929 14 1 12.8807 1 11.5V4.5C1 3.11929 2.11929 2 3.5 2H4.58579C4.98361 2 5.36514 2.15804 5.64645 2.43934L7.20711 4H10.5C11.724 4 12.7426 4.87965 12.958 6.04127C14.605 6.34148 15.5443 8.22106 14.6616 9.75L13.0766 12.4953C12.5407 13.4235 11.5503 13.9953 10.4785 13.9953H7.06895ZM5.52196 7C4.80743 7 4.14718 7.3812 3.78991 8L2.20492 10.7453C1.62757 11.7453 2.34926 12.9953 3.50396 12.9953L10.4785 12.9953C11.193 12.9953 11.8533 12.6141 12.2105 11.9953L13.7955 9.25C14.3729 8.25 13.6512 7 12.4965 7L5.52196 7Z"),
			new IconSource("folder_open", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M3.00026 5.5V12.1041L4.50091 9.50488C5.03681 8.57668 6.02719 8.00488 7.09898 8.00488L15.0003 8.00488V7.5C15.0003 6.67157 14.3287 6 13.5003 6H9.50026C9.36765 6 9.24047 5.94732 9.1467 5.85355L7.4396 4.14645C7.34583 4.05268 7.21865 4 7.08604 4H4.50026C3.67183 4 3.00026 4.67157 3.00026 5.5ZM4.28361 15.9845C4.35498 15.9947 4.42844 16 4.50373 16H13.9012C14.6157 16 15.276 15.6188 15.6332 15L17.7955 11.2549C18.3728 10.2549 17.6511 9.00488 16.4964 9.00488L7.09898 9.00488C6.38445 9.00488 5.7242 9.38608 5.36694 10.0049L3.20469 13.75C2.78223 14.4817 3.0553 15.3473 3.68462 15.7591C3.86197 15.8742 4.06517 15.9529 4.28361 15.9845ZM2.00026 14.4607V5.5C2.00026 4.11929 3.11955 3 4.50026 3H7.08604C7.48387 3 7.8654 3.15804 8.1467 3.43934L9.70736 5H13.5003C14.881 5 16.0003 6.11929 16.0003 7.5V8.00488H16.4964C18.4209 8.00488 19.6237 10.0882 18.6615 11.7549L16.4992 15.5C15.9633 16.4282 14.973 17 13.9012 17H4.50026C4.4378 17 4.37588 16.9977 4.31457 16.9932C3.75344 16.9527 3.26124 16.7329 2.87665 16.4011C2.36561 15.9642 2.03283 15.3249 2.00252 14.6073C2.00031 14.5586 1.99955 14.5097 2.00026 14.4607Z"),
			new IconSource("folder_open", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.50014 6.25C3.50014 5.2835 4.28364 4.5 5.25014 4.5H8.12882C8.32773 4.5 8.5185 4.57902 8.65915 4.71967L10.7198 6.78033C10.8605 6.92098 11.0512 7 11.2501 7H16.7501C17.7166 7 18.5001 7.7835 18.5001 8.75C18.5001 8.83602 18.5146 8.91866 18.5413 8.99561L8.71867 8.99561C7.37893 8.99561 6.14095 9.71035 5.47108 10.8706L3.50014 14.2844V6.25ZM2.00036 17.7883C2.02089 19.5656 3.468 21 5.25014 21H11.0001C11.0292 21 11.058 20.9983 11.0862 20.9951H16.2812C17.6209 20.9951 18.8589 20.2804 19.5288 19.1201L22.5596 13.8706C23.7748 11.7657 22.3337 9.14964 19.9567 9.00214C19.9848 8.92334 20.0001 8.83846 20.0001 8.75C20.0001 6.95507 18.5451 5.5 16.7501 5.5H11.5608L9.71981 3.65901C9.29786 3.23705 8.72556 3 8.12882 3H5.25014C3.45522 3 2.00014 4.45507 2.00014 6.25V17.71C1.9999 17.7362 1.99997 17.7623 2.00036 17.7883ZM8.71867 10.4956L19.745 10.4956C21.0921 10.4956 21.9341 11.9539 21.2605 13.1206L18.2297 18.3701C17.8278 19.0663 17.085 19.4951 16.2812 19.4951H5.25485C3.9077 19.4951 3.06573 18.0368 3.7393 16.8701L6.77011 11.6206C7.17204 10.9245 7.91482 10.4956 8.71867 10.4956Z"),
			new IconSource("folder_open", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M5.75391 3C3.68284 3 2.00391 4.67893 2.00391 6.75V21C2.00391 21.0157 2.00439 21.0312 2.00533 21.0467C2.00438 21.0768 2.00391 21.1069 2.00391 21.1371C2.00391 22.7183 3.2857 24.0001 4.86687 24.0001H19.4724C20.8095 24.0001 22.0455 23.2881 22.7163 22.1314L26.486 15.6313C27.9359 13.1313 26.1321 10 23.2421 10H23.0005V9.75C23.0005 7.67893 21.3216 6 19.2505 6H13.6642C13.5979 6 13.5343 5.97366 13.4874 5.92678L11.3661 3.80546C10.8504 3.28973 10.1509 3 9.42157 3H5.75391ZM21.5005 10H9.3893C7.94606 10 6.63089 10.8283 6.00735 12.1298L3.50391 17.3556V6.75C3.50391 5.50736 4.51127 4.5 5.75391 4.5H9.42157C9.75309 4.5 10.071 4.6317 10.3055 4.86612L12.4268 6.98744C12.755 7.31563 13.2001 7.5 13.6642 7.5H19.2505C20.4932 7.5 21.5005 8.50736 21.5005 9.75V10ZM19.4724 22.5001H4.86687C4.11412 22.5001 3.50391 21.8898 3.50391 21.1371C3.50391 20.9333 3.54962 20.7321 3.63768 20.5483L7.36013 12.7779C7.73426 11.997 8.52336 11.5 9.3893 11.5H23.2421C24.9761 11.5 26.0584 13.3788 25.1885 14.8788L21.4187 21.3789C21.0162 22.0729 20.2746 22.5001 19.4724 22.5001Z"),
			new IconSource("arrow_up", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M7.5 13.5C7.5 13.7761 7.72386 14 8 14C8.27614 14 8.5 13.7761 8.5 13.5V3.80298L12.1283 7.83448C12.3131 8.03974 12.6292 8.05638 12.8345 7.87165C13.0397 7.68692 13.0564 7.37077 12.8716 7.16552L8.37165 2.16552C8.27683 2.06016 8.14174 2 8 2C7.85826 2 7.72317 2.06016 7.62835 2.16552L3.12836 7.16552C2.94363 7.37077 2.96027 7.68692 3.16552 7.87165C3.37078 8.05638 3.68692 8.03974 3.87165 7.83448L7.5 3.80298V13.5Z"),
			new IconSource("arrow_up", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M3.13269 9.16094C2.94651 9.36488 2.96091 9.68113 3.16485 9.86731C3.36879 10.0535 3.68504 10.0391 3.87122 9.83515L9.50195 3.6673L9.50195 17.498C9.50195 17.7742 9.72581 17.998 10.002 17.998C10.2781 17.998 10.502 17.7742 10.502 17.498L10.502 3.67019L16.13 9.83515C16.3162 10.0391 16.6325 10.0535 16.8364 9.86731C17.0404 9.68113 17.0548 9.36488 16.8686 9.16094L10.5545 2.24458C10.4268 2.10464 10.2592 2.02481 10.0861 2.0051C10.0587 2.00046 10.0306 1.99805 10.002 1.99805C9.97508 1.99805 9.9487 2.00017 9.92298 2.00425C9.74708 2.02242 9.57641 2.10253 9.44673 2.24458L3.13269 9.16094Z"),
			new IconSource("arrow_up", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M4.20889 10.7327C3.9232 11.0326 3.93475 11.5074 4.23467 11.7931C4.5346 12.0788 5.00933 12.0672 5.29502 11.7673L11.2495 5.516V20.25C11.2495 20.6642 11.5853 21 11.9995 21C12.4137 21 12.7495 20.6642 12.7495 20.25V5.51565L18.7043 11.7673C18.99 12.0672 19.4648 12.0788 19.7647 11.7931C20.0646 11.5074 20.0762 11.0326 19.7905 10.7327L12.7238 3.31379C12.5627 3.14474 12.3573 3.04477 12.1438 3.01386C12.0971 3.00477 12.0489 3 11.9995 3C11.9498 3 11.9012 3.00483 11.8543 3.01406C11.6412 3.04518 11.4363 3.14509 11.2756 3.31379L4.20889 10.7327Z"),
			new IconSource("arrow_up", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M4.21144 12.7328C3.92428 13.0313 3.93349 13.5061 4.232 13.7932C4.53052 14.0804 5.0053 14.0712 5.29246 13.7727L13.252 5.49844V24.2512C13.252 24.6655 13.5877 25.0012 14.002 25.0012C14.4162 25.0012 14.752 24.6655 14.752 24.2512V5.49942L22.7105 13.7727C22.9977 14.0712 23.4724 14.0804 23.771 13.7932C24.0695 13.5061 24.0787 13.0313 23.7915 12.7328L14.7222 3.30478C14.3287 2.8958 13.6742 2.8958 13.2808 3.30478L4.21144 12.7328Z"),
			new IconSource("arrow_up", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M16 29C15.4477 29 15 28.5523 15 28V6.41422L6.70711 14.7071C6.31658 15.0976 5.68342 15.0976 5.29289 14.7071C4.90237 14.3166 4.90237 13.6834 5.29289 13.2929L15.2929 3.29289C15.6834 2.90237 16.3166 2.90237 16.7071 3.29289L26.7071 13.2929C27.0976 13.6834 27.0976 14.3166 26.7071 14.7071C26.3166 15.0976 25.6834 15.0976 25.2929 14.7071L17 6.41421V28C17 28.5523 16.5523 29 16 29Z"),
			new IconSource("arrow_down", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M8.5 2.5C8.5 2.22386 8.27615 2 8 2C7.72386 2 7.5 2.22386 7.5 2.5V12.197L3.87165 8.16552C3.68692 7.96026 3.37078 7.94362 3.16552 8.12835C2.96027 8.31308 2.94363 8.62923 3.12836 8.83448L7.62836 13.8345C7.72318 13.9398 7.85826 14 8 14C8.14175 14 8.27683 13.9398 8.37165 13.8345L12.8717 8.83448C13.0564 8.62923 13.0397 8.31308 12.8345 8.12835C12.6292 7.94362 12.3131 7.96026 12.1284 8.16552L8.5 12.197V2.5Z"),
			new IconSource("arrow_down", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M16.8666 10.8371C17.0528 10.6332 17.0384 10.3169 16.8345 10.1307C16.6305 9.94456 16.3143 9.95895 16.1281 10.1629L10.4974 16.3307V2.5C10.4974 2.22386 10.2735 2 9.99736 2C9.72122 2 9.49736 2.22386 9.49736 2.5V16.3279L3.86927 10.1629C3.68309 9.95895 3.36684 9.94456 3.16289 10.1307C2.95895 10.3169 2.94456 10.6332 3.13074 10.8371L9.44478 17.7535C9.57254 17.8934 9.74008 17.9732 9.91323 17.993C9.94058 17.9976 9.96869 18 9.99736 18C10.0242 18 10.0506 17.9979 10.0763 17.9938C10.2522 17.9756 10.4229 17.8955 10.5526 17.7535L16.8666 10.8371Z"),
			new IconSource("arrow_down", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M19.7905 13.2673C20.0762 12.9674 20.0646 12.4926 19.7647 12.2069C19.4648 11.9212 18.99 11.9328 18.7043 12.2327L12.7498 18.484V3.75C12.7498 3.33579 12.4141 3 11.9998 3C11.5856 3 11.2498 3.33579 11.2498 3.75V18.4844L5.29502 12.2327C5.00933 11.9328 4.5346 11.9212 4.23467 12.2069C3.93475 12.4926 3.9232 12.9674 4.20889 13.2673L11.2756 20.6862C11.4366 20.8553 11.642 20.9552 11.8556 20.9861C11.9023 20.9952 11.9505 21 11.9998 21C12.0495 21 12.0981 20.9952 12.1451 20.9859C12.3582 20.9548 12.5631 20.8549 12.7238 20.6862L19.7905 13.2673Z"),
			new IconSource("arrow_down", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M23.7915 15.2665C24.0787 14.968 24.0695 14.4932 23.771 14.2061C23.4724 13.9189 22.9977 13.9281 22.7105 14.2266L14.751 22.5009L14.751 3.74805C14.751 3.33383 14.4152 2.99805 14.001 2.99805C13.5868 2.99805 13.251 3.33383 13.251 3.74805L13.251 22.4999L5.29246 14.2266C5.00531 13.9281 4.53052 13.9189 4.232 14.2061C3.93349 14.4932 3.92428 14.968 4.21144 15.2665L13.2808 24.6945C13.6742 25.1035 14.3287 25.1035 14.7222 24.6945L23.7915 15.2665Z"),
			new IconSource("arrow_down", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M16 3C16.5523 3 17 3.44772 17 4V25.5858L25.2929 17.2929C25.6834 16.9024 26.3166 16.9024 26.7071 17.2929C27.0976 17.6834 27.0976 18.3166 26.7071 18.7071L16.7071 28.7071C16.3166 29.0976 15.6834 29.0976 15.2929 28.7071L5.29289 18.7071C4.90237 18.3166 4.90237 17.6834 5.29289 17.2929C5.68342 16.9024 6.31658 16.9024 6.70711 17.2929L15 25.5858V4C15 3.44772 15.4477 3 16 3Z"),
			new IconSource("document_text", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3 3C3 1.89543 3.89543 1 5 1H8.58579C8.98361 1 9.36514 1.15804 9.64645 1.43934L12.5607 4.35355C12.842 4.63486 13 5.01639 13 5.41421V13C13 14.1046 12.1046 15 11 15H5C3.89543 15 3 14.1046 3 13V3ZM5 2C4.44772 2 4 2.44772 4 3V13C4 13.5523 4.44772 14 5 14H11C11.5523 14 12 13.5523 12 13V6H9.5C8.67157 6 8 5.32843 8 4.5V2H5ZM9.5 5H11.7929L9 2.20711V4.5C9 4.77614 9.22386 5 9.5 5ZM5.5 8C5.22386 8 5 8.22386 5 8.5C5 8.77614 5.22386 9 5.5 9H10.5C10.7761 9 11 8.77614 11 8.5C11 8.22386 10.7761 8 10.5 8H5.5ZM5 10.5C5 10.2239 5.22386 10 5.5 10H10.5C10.7761 10 11 10.2239 11 10.5C11 10.7761 10.7761 11 10.5 11H5.5C5.22386 11 5 10.7761 5 10.5ZM5.5 12C5.22386 12 5 12.2239 5 12.5C5 12.7761 5.22386 13 5.5 13H10.5C10.7761 13 11 12.7761 11 12.5C11 12.2239 10.7761 12 10.5 12H5.5Z"),
			new IconSource("document_text", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M6.5 10C6.22386 10 6 10.2239 6 10.5C6 10.7761 6.22386 11 6.5 11H13.5C13.7761 11 14 10.7761 14 10.5C14 10.2239 13.7761 10 13.5 10H6.5ZM6.5 12C6.22386 12 6 12.2239 6 12.5C6 12.7761 6.22386 13 6.5 13H13.5C13.7761 13 14 12.7761 14 12.5C14 12.2239 13.7761 12 13.5 12H6.5ZM6.5 14C6.22386 14 6 14.2239 6 14.5C6 14.7761 6.22386 15 6.5 15H13.5C13.7761 15 14 14.7761 14 14.5C14 14.2239 13.7761 14 13.5 14H6.5ZM4 4C4 2.89543 4.89543 2 6 2H10.5858C10.9836 2 11.3651 2.15804 11.6464 2.43934L15.5607 6.35355C15.842 6.63486 16 7.01639 16 7.41421V16C16 17.1046 15.1046 18 14 18H6C4.89543 18 4 17.1046 4 16V4ZM6 3C5.44772 3 5 3.44772 5 4V16C5 16.5523 5.44772 17 6 17H14C14.5523 17 15 16.5523 15 16V8H11.5C10.6716 8 10 7.32843 10 6.5V3H6ZM11.5 7H14.7929L11 3.20711V6.5C11 6.77614 11.2239 7 11.5 7Z"),
			new IconSource("document_text", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M8.75 11.5C8.33579 11.5 8 11.8358 8 12.25C8 12.6642 8.33579 13 8.75 13H15.25C15.6642 13 16 12.6642 16 12.25C16 11.8358 15.6642 11.5 15.25 11.5H8.75ZM8.75 14.25C8.33579 14.25 8 14.5858 8 15C8 15.4142 8.33579 15.75 8.75 15.75H15.25C15.6642 15.75 16 15.4142 16 15C16 14.5858 15.6642 14.25 15.25 14.25H8.75ZM8.75 17C8.33579 17 8 17.3358 8 17.75C8 18.1642 8.33579 18.5 8.75 18.5H15.25C15.6642 18.5 16 18.1642 16 17.75C16 17.3358 15.6642 17 15.25 17H8.75ZM13.585 2.586L19.414 8.414C19.789 8.789 20 9.298 20 9.828V20C20 21.104 19.104 22 18 22H6C4.896 22 4 21.104 4 20V4C4 2.896 4.896 2 6 2H12.172C12.6865 2 13.2194 2.22035 13.585 2.586ZM18 20.5C18.276 20.5 18.5 20.275 18.5 20V10H14C12.896 10 12 9.104 12 8V3.5H6C5.724 3.5 5.5 3.725 5.5 4V20C5.5 20.275 5.724 20.5 6 20.5H18ZM17.378 8.5L13.5 4.621V8C13.5 8.275 13.724 8.5 14 8.5H17.378Z"),
			new IconSource("document_text", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M9.25 14.5C8.83579 14.5 8.5 14.8358 8.5 15.25C8.5 15.6642 8.83579 16 9.25 16H18.75C19.1642 16 19.5 15.6642 19.5 15.25C19.5 14.8358 19.1642 14.5 18.75 14.5H9.25ZM8.5 18.25C8.5 17.8358 8.83579 17.5 9.25 17.5H18.75C19.1642 17.5 19.5 17.8358 19.5 18.25C19.5 18.6642 19.1642 19 18.75 19H9.25C8.83579 19 8.5 18.6642 8.5 18.25ZM9.25 20.5C8.83579 20.5 8.5 20.8358 8.5 21.25C8.5 21.6642 8.83579 22 9.25 22H18.75C19.1642 22 19.5 21.6642 19.5 21.25C19.5 20.8358 19.1642 20.5 18.75 20.5H9.25ZM4 4.4C4 3.07452 5.07452 2 6.4 2H14.5665C15.2031 2 15.8135 2.25286 16.2636 2.70294L23.1945 9.63388C23.7103 10.1496 24 10.8491 24 11.5784V23.6C24 24.9255 22.9255 26 21.6 26H6.4C5.07452 26 4 24.9255 4 23.6V4.4ZM6.4 3.5C5.90294 3.5 5.5 3.90294 5.5 4.4V23.6C5.5 24.0971 5.90294 24.5 6.4 24.5H21.6C22.0971 24.5 22.5 24.0971 22.5 23.6V12H16C14.8954 12 14 11.1046 14 10V3.5H6.4ZM16 10.5H21.9393L15.5 4.06066V10C15.5 10.2761 15.7239 10.5 16 10.5Z"),
			new IconSource("document_text", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M11 23C10.4477 23 10 23.4477 10 24C10 24.5523 10.4477 25 11 25H21C21.5523 25 22 24.5523 22 24C22 23.4477 21.5523 23 21 23H11ZM10 20C10 19.4477 10.4477 19 11 19H21C21.5523 19 22 19.4477 22 20C22 20.5523 21.5523 21 21 21H11C10.4477 21 10 20.5523 10 20ZM11 15C10.4477 15 10 15.4477 10 16C10 16.5523 10.4477 17 11 17H21C21.5523 17 22 16.5523 22 16C22 15.4477 21.5523 15 21 15H11ZM5 5C5 3.34315 6.34315 2 8 2H18.1716C18.9672 2 19.7303 2.31607 20.2929 2.87868L26.1213 8.70711C26.6839 9.26972 27 10.0328 27 10.8284V27C27 28.6569 25.6569 30 24 30H8C6.34315 30 5 28.6569 5 27V5ZM8 4C7.44772 4 7 4.44772 7 5V27C7 27.5523 7.44772 28 8 28H24C24.5523 28 25 27.5523 25 27V12H20C18.3431 12 17 10.6569 17 9V4H8ZM20 10H24.5858L19 4.41421V9C19 9.55228 19.4477 10 20 10Z"),
			new IconSource("note_edit", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M3.00509 14.1763C3.09634 15.7511 4.40232 17 6 17H8.00286C8.01015 16.8859 8.02795 16.7701 8.05716 16.6532L8.22047 16H6L5.85074 15.9945C4.81588 15.9182 4 15.0544 4 14V10H7L7.17627 9.99491C8.75108 9.90366 10 8.59768 10 7V4H14L14.1493 4.00549C15.1841 4.08183 16 4.94564 16 6V8.00299C16.3368 7.98764 16.6757 8.03128 17 8.13392V6L16.9949 5.82373C16.9037 4.24892 15.5977 3 14 3H9.62132L9.44547 3.00774C8.97949 3.04885 8.5405 3.25239 8.20711 3.58579L3.58579 8.20711L3.46691 8.33693C3.16649 8.69549 3 9.14982 3 9.62132V14L3.00509 14.1763ZM7 9L4.2175 8.99866C4.2409 8.96925 4.26606 8.94105 4.29289 8.91421L8.91421 4.29289L9 4.219V7L8.99451 7.14926C8.91817 8.18412 8.05436 9 7 9ZM9.97975 14.3772L14.8092 9.54776C15.5395 8.81741 16.7237 8.81741 17.454 9.54776C18.1843 10.2781 18.1843 11.4622 17.454 12.1926L12.6246 17.022C12.343 17.3036 11.9902 17.5033 11.6039 17.5999L10.106 17.9744C9.45456 18.1372 8.86451 17.5472 9.02737 16.8958L9.40184 15.3979C9.49842 15.0116 9.69818 14.6588 9.97975 14.3772Z"),
			new IconSource("note_edit", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3 17.75C3 19.5449 4.45507 21 6.25 21H11.1651L11.5209 19.5768C11.5273 19.5511 11.534 19.5255 11.541 19.5H6.25C5.2835 19.5 4.5 18.7165 4.5 17.75V11H7.75L7.93442 10.9949C9.64354 10.8992 11 9.48303 11 7.75V4.5H17.75C18.7165 4.5 19.5 5.2835 19.5 6.25V11.2324C19.9789 11.0419 20.4935 10.9685 21 11.0122V6.25C21 4.45507 19.5449 3 17.75 3H10.8713C10.2746 3 9.70229 3.23705 9.28033 3.65901L3.65901 9.28033C3.23705 9.70229 3 10.2746 3 10.8713V17.75ZM7.75 9.5H5.561L9.5 5.561V7.75L9.4942 7.89353C9.42119 8.79289 8.66817 9.5 7.75 9.5ZM19.0999 12.6695L13.1974 18.5719C12.8533 18.916 12.6092 19.3472 12.4911 19.8194L12.0334 21.6501C11.8344 22.4462 12.5556 23.1674 13.3517 22.9683L15.1824 22.5106C15.6545 22.3926 16.0857 22.1485 16.4299 21.8043L22.3323 15.9019C23.2249 15.0093 23.2249 13.5621 22.3323 12.6695C21.4397 11.7768 19.9925 11.7768 19.0999 12.6695Z"),
			new IconSource("code", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M10.0334 1.3201C10.1326 1.06263 10.4222 0.933948 10.6798 1.03299C10.9373 1.13219 11.0659 1.42189 10.9669 1.67948L5.96695 14.6795C5.86779 14.9371 5.57812 15.0656 5.32047 14.9666C5.06285 14.8674 4.93425 14.5778 5.03336 14.3201L10.0334 1.3201ZM3.64664 4.64628C3.84188 4.45123 4.15846 4.45118 4.35367 4.64628C4.54878 4.84148 4.54872 5.15806 4.35367 5.35331L1.70718 7.99979L4.35367 10.6463C4.54878 10.8415 4.54872 11.1581 4.35367 11.3533C4.15844 11.5485 3.84191 11.5485 3.64664 11.3533L0.646637 8.35331C0.451482 8.15804 0.451411 7.8415 0.646637 7.64628L3.64664 4.64628ZM11.6466 4.64628C11.8419 4.45123 12.1585 4.45118 12.3537 4.64628L15.3537 7.64628C15.5488 7.84148 15.5487 8.15806 15.3537 8.35331L12.3537 11.3533C12.1584 11.5485 11.8419 11.5485 11.6466 11.3533C11.4515 11.158 11.4514 10.8415 11.6466 10.6463L14.2931 7.99979L11.6466 5.35331C11.4515 5.15804 11.4514 4.8415 11.6466 4.64628Z"),
			new IconSource("code", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M12.6406 1.02073C12.9053 1.09864 13.0571 1.37606 12.9795 1.64084L7.97947 18.6408C7.90151 18.9055 7.62407 19.0573 7.35935 18.9797C7.09464 18.9018 6.94295 18.6243 7.02048 18.3596L12.0205 1.35959C12.0984 1.09481 12.3758 0.943089 12.6406 1.02073ZM5.12595 5.16819C5.30934 4.9619 5.62562 4.94297 5.83201 5.12619C6.03822 5.30959 6.05718 5.62588 5.874 5.83225L2.16892 10.0002L5.874 14.1682C6.05712 14.3746 6.03825 14.6909 5.83201 14.8742C5.62564 15.0573 5.30931 15.0384 5.12595 14.8322L1.12595 10.3322C0.95789 10.1429 0.95798 9.85755 1.12595 9.66819L5.12595 5.16819ZM14.1679 5.12619C14.3743 4.943 14.6906 4.96189 14.874 5.16819L18.874 9.66819C19.042 9.85754 19.0421 10.1429 18.874 10.3322L14.874 14.8322C14.6906 15.0384 14.3743 15.0573 14.1679 14.8742C13.9617 14.6909 13.9429 14.3746 14.126 14.1682L17.831 10.0002L14.126 5.83225C13.9428 5.6259 13.9618 5.3096 14.1679 5.12619Z"),
			new IconSource("code", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M15.0461 1.49224C15.1885 1.10338 15.6191 0.903659 16.008 1.04595C16.3967 1.18841 16.5965 1.619 16.4543 2.00786L8.95427 22.5079C8.81191 22.8967 8.38124 23.0963 7.99236 22.9542C7.60349 22.8118 7.40378 22.3812 7.54607 21.9922L15.0461 1.49224ZM6.2072 6.23247C6.4929 5.93284 6.96791 5.92152 7.26775 6.20708C7.5673 6.49276 7.57861 6.9678 7.29314 7.26763L2.78533 12.0001L7.29314 16.7325C7.57857 17.0323 7.56733 17.5074 7.26775 17.793C6.96793 18.0786 6.4929 18.0672 6.2072 17.7676L1.2072 12.5176C0.93134 12.228 0.93134 11.7721 1.2072 11.4825L6.2072 6.23247ZM16.7326 6.20708C17.0325 5.92158 17.5075 5.93279 17.7931 6.23247L22.7931 11.4825C23.0688 11.7721 23.0689 12.228 22.7931 12.5176L17.7931 17.7676C17.5075 18.0673 17.0325 18.0785 16.7326 17.793C16.4329 17.5073 16.4216 17.0323 16.7072 16.7325L21.215 12.0001L16.7072 7.26763C16.4216 6.96776 16.4329 6.49277 16.7326 6.20708Z"),
			new IconSource("book_search", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4 4V16C4 17.1046 4.89543 18 6 18H15.5C15.7761 18 16 17.7761 16 17.5C16 17.2239 15.7761 17 15.5 17H6C5.44772 17 5 16.5523 5 16H15C15.5523 16 16 15.5523 16 15V4C16 2.89543 15.1046 2 14 2H6C4.89543 2 4 2.89543 4 4ZM14 3C14.5523 3 15 3.44772 15 4V15H5V4C5 3.44772 5.44772 3 6 3H14ZM11.5858 9.8787C11.8476 9.48348 12 9.00954 12 8.5C12 7.11929 10.8807 6 9.5 6C8.11929 6 7 7.11929 7 8.5C7 9.88071 8.11929 11 9.5 11C10.0095 11 10.4835 10.8476 10.8787 10.5858L12.1464 11.8536C12.3417 12.0488 12.6583 12.0488 12.8536 11.8536C13.0488 11.6583 13.0488 11.3417 12.8536 11.1464L11.5858 9.8787ZM8 8.5C8 7.67157 8.67157 7 9.5 7C10.3284 7 11 7.67157 11 8.5C11 9.32843 10.3284 10 9.5 10C8.67157 10 8 9.32843 8 8.5Z"),
			new IconSource("book_search", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M14.8185 11.795C15.2156 11.2028 15.4474 10.49 15.4474 9.72368C15.4474 7.66715 13.7802 6 11.7237 6C9.66715 6 8 7.66715 8 9.72368C8 11.7802 9.66715 13.4474 11.7237 13.4474C12.49 13.4474 13.2028 13.2156 13.795 12.8185L16.2711 15.2945L16.3381 15.3524L16.3455 15.3579C16.629 15.5682 17.031 15.5451 17.288 15.288C17.5707 15.0054 17.5707 14.5472 17.288 14.2646L14.8185 11.795ZM11.7237 7.44737C12.9809 7.44737 14 8.46651 14 9.72368C14 10.9809 12.9809 12 11.7237 12C10.4665 12 9.44737 10.9809 9.44737 9.72368C9.44737 8.46651 10.4665 7.44737 11.7237 7.44737ZM6.5 2C5.11929 2 4 3.11929 4 4.5V19.5C4 20.8807 5.11929 22 6.5 22H19.75C20.1642 22 20.5 21.6642 20.5 21.25C20.5 20.8358 20.1642 20.5 19.75 20.5H6.5C5.94772 20.5 5.5 20.0523 5.5 19.5H19.75C20.1642 19.5 20.5 19.1642 20.5 18.75V4.5C20.5 3.11929 19.3807 2 18 2H6.5ZM19 18H5.5V4.5C5.5 3.94772 5.94772 3.5 6.5 3.5H18C18.5523 3.5 19 3.94772 19 4.5V18Z"),
			new IconSource("document_arrow_down", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M4 3.00195C4 1.89738 4.89543 1.00195 6 1.00195H9.58196C9.97969 1.00195 10.3611 1.15991 10.6424 1.4411L13.5605 4.35806C13.8419 4.63938 14 5.021 14 5.41892V12.998C14 14.1026 13.1046 14.998 12 14.998H8.66586C9.06858 14.7141 9.43145 14.3774 9.74446 13.998H12C12.5523 13.998 13 13.5503 13 12.998V6.0029H10.4995C9.67108 6.0029 8.99951 5.33133 8.99951 4.5029V2.00195H6C5.44771 2.00195 5 2.44967 5 3.00195V5.02242C4.65659 5.05337 4.32228 5.11588 4 5.20703V3.00195ZM9.99951 2.21237V4.5029C9.99951 4.77904 10.2234 5.0029 10.4995 5.0029H12.7911L9.99951 2.21237ZM10 10.5C10 12.9853 7.98528 15 5.5 15C3.01472 15 1 12.9853 1 10.5C1 8.01472 3.01472 6 5.5 6C7.98528 6 10 8.01472 10 10.5ZM5.14605 12.8532L5.14857 12.8557C5.19602 12.9026 5.25051 12.938 5.30861 12.9621C5.36669 12.9861 5.4303 12.9996 5.497 13L5.5 13L5.503 13C5.5697 12.9996 5.63331 12.9861 5.69139 12.9621C5.75036 12.9377 5.80561 12.9015 5.85355 12.8536L7.85355 10.8536C8.04882 10.6583 8.04882 10.3417 7.85355 10.1464C7.65829 9.95118 7.34171 9.95118 7.14645 10.1464L6 11.2929V8.5C6 8.22386 5.77614 8 5.5 8C5.22386 8 5 8.22386 5 8.5L5 11.2929L3.85355 10.1464C3.65829 9.95118 3.34171 9.95118 3.14645 10.1464C2.95118 10.3417 2.95118 10.6583 3.14645 10.8536L5.14605 12.8532Z"),
			new IconSource("document_arrow_down", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M6 2C4.89543 2 4 2.89543 4 4V9.20703C4.32228 9.11588 4.65659 9.05337 5 9.02242V4C5 3.44772 5.44772 3 6 3H10V6.5C10 7.32843 10.6716 8 11.5 8H15V16C15 16.5523 14.5523 17 14 17H10.4003C10.2174 17.3578 9.99647 17.6929 9.74284 18H14C15.1046 18 16 17.1046 16 16V7.41421C16 7.01639 15.842 6.63486 15.5607 6.35355L11.6464 2.43934C11.3651 2.15804 10.9836 2 10.5858 2H6ZM14.7929 7H11.5C11.2239 7 11 6.77614 11 6.5V3.20711L14.7929 7ZM5.5 19C7.98528 19 10 16.9853 10 14.5C10 12.0147 7.98528 10 5.5 10C3.01472 10 1 12.0147 1 14.5C1 16.9853 3.01472 19 5.5 19ZM3.14645 14.8536C2.95118 14.6583 2.95118 14.3417 3.14645 14.1464C3.34171 13.9512 3.65829 13.9512 3.85355 14.1464L5 15.2929L5 12.5C5 12.2239 5.22386 12 5.5 12C5.77614 12 6 12.2239 6 12.5V15.2929L7.14645 14.1464C7.34171 13.9512 7.65829 13.9512 7.85355 14.1464C8.04882 14.3417 8.04882 14.6583 7.85355 14.8536L5.85355 16.8536C5.80561 16.9015 5.75036 16.9377 5.69139 16.9621C5.63331 16.9861 5.5697 16.9996 5.503 17L5.5 17L5.497 17C5.4303 16.9996 5.36669 16.9861 5.30861 16.9621C5.25051 16.938 5.19602 16.9026 5.14857 16.8557L5.14605 16.8532L3.14645 14.8536Z"),
			new IconSource("document_arrow_down", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M6 2C4.89543 2 4 2.89543 4 4V11.4982C4.47417 11.3004 4.97679 11.1572 5.5 11.0764V4C5.5 3.72386 5.72386 3.5 6 3.5H12V8C12 9.10457 12.8954 10 14 10H18.5V20C18.5 20.2761 18.2761 20.5 18 20.5H12.2678C11.9806 21.051 11.6168 21.5557 11.1904 22H18C19.1046 22 20 21.1046 20 20V9.82777C20 9.29733 19.7893 8.78863 19.4142 8.41355L13.5864 2.58579C13.2114 2.21071 12.7027 2 12.1722 2H6ZM17.3793 8.5H14C13.7239 8.5 13.5 8.27614 13.5 8V4.62066L17.3793 8.5ZM12 17.5C12 20.5376 9.53757 23 6.5 23C3.46243 23 1 20.5376 1 17.5C1 14.4624 3.46243 12 6.5 12C9.53757 12 12 14.4624 12 17.5ZM7 14.5C7 14.2239 6.77614 14 6.5 14C6.22386 14 6 14.2239 6 14.5V19.2929L4.35355 17.6464C4.15829 17.4512 3.84171 17.4512 3.64645 17.6464C3.45118 17.8417 3.45118 18.1583 3.64645 18.3536L6.14645 20.8536C6.34171 21.0488 6.65829 21.0488 6.85355 20.8536L9.35355 18.3536C9.54882 18.1583 9.54882 17.8417 9.35355 17.6464C9.15829 17.4512 8.84171 17.4512 8.64645 17.6464L7 19.2929V14.5Z"),
			new IconSource("text_bullet_list_ltr", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M2 4.5C2.55228 4.5 3 4.05228 3 3.5C3 2.94772 2.55228 2.5 2 2.5C1.44772 2.5 1 2.94772 1 3.5C1 4.05228 1.44772 4.5 2 4.5ZM2 9C2.55228 9 3 8.55228 3 8C3 7.44772 2.55228 7 2 7C1.44772 7 1 7.44772 1 8C1 8.55228 1.44772 9 2 9ZM3 12.5C3 13.0523 2.55228 13.5 2 13.5C1.44772 13.5 1 13.0523 1 12.5C1 11.9477 1.44772 11.5 2 11.5C2.55228 11.5 3 11.9477 3 12.5ZM5.5 3C5.22386 3 5 3.22386 5 3.5C5 3.77614 5.22386 4 5.5 4H14.5C14.7761 4 15 3.77614 15 3.5C15 3.22386 14.7761 3 14.5 3H5.5ZM5 8C5 7.72386 5.22386 7.5 5.5 7.5H14.5C14.7761 7.5 15 7.72386 15 8C15 8.27614 14.7761 8.5 14.5 8.5H5.5C5.22386 8.5 5 8.27614 5 8ZM5.5 12C5.22386 12 5 12.2239 5 12.5C5 12.7761 5.22386 13 5.5 13H14.5C14.7761 13 15 12.7761 15 12.5C15 12.2239 14.7761 12 14.5 12H5.5Z"),
			new IconSource("text_bullet_list_ltr", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M3 5.5C3.55228 5.5 4 5.05228 4 4.5C4 3.94772 3.55228 3.5 3 3.5C2.44772 3.5 2 3.94772 2 4.5C2 5.05228 2.44772 5.5 3 5.5ZM6 4.5C6 4.22386 6.22386 4 6.5 4H17.5C17.7761 4 18 4.22386 18 4.5C18 4.77614 17.7761 5 17.5 5H6.5C6.22386 5 6 4.77614 6 4.5ZM6 9.5C6 9.22386 6.22386 9 6.5 9H17.5C17.7761 9 18 9.22386 18 9.5C18 9.77614 17.7761 10 17.5 10H6.5C6.22386 10 6 9.77614 6 9.5ZM6.5 14C6.22386 14 6 14.2239 6 14.5C6 14.7761 6.22386 15 6.5 15H17.5C17.7761 15 18 14.7761 18 14.5C18 14.2239 17.7761 14 17.5 14H6.5ZM4 14.5C4 15.0523 3.55228 15.5 3 15.5C2.44772 15.5 2 15.0523 2 14.5C2 13.9477 2.44772 13.5 3 13.5C3.55228 13.5 4 13.9477 4 14.5ZM3 10.5C3.55228 10.5 4 10.0523 4 9.5C4 8.94772 3.55228 8.5 3 8.5C2.44772 8.5 2 8.94772 2 9.5C2 10.0523 2.44772 10.5 3 10.5Z"),
			new IconSource("text_bullet_list_ltr", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.24929 17.5C3.93925 17.5 4.49858 18.0593 4.49858 18.7493C4.49858 19.4393 3.93925 19.9986 3.24929 19.9986C2.55933 19.9986 2 19.4393 2 18.7493C2 18.0593 2.55933 17.5 3.24929 17.5ZM6.75 18H21.25C21.6642 18 22 18.3358 22 18.75C22 19.1297 21.7178 19.4435 21.3518 19.4932L21.25 19.5H6.75C6.33579 19.5 6 19.1642 6 18.75C6 18.3703 6.28215 18.0565 6.64823 18.0068L6.75 18H21.25H6.75ZM3.24929 11C3.93925 11 4.49858 11.5593 4.49858 12.2493C4.49858 12.9393 3.93925 13.4986 3.24929 13.4986C2.55933 13.4986 2 12.9393 2 12.2493C2 11.5593 2.55933 11 3.24929 11ZM6.75 11.5H21.25C21.6642 11.5 22 11.8358 22 12.25C22 12.6297 21.7178 12.9435 21.3518 12.9932L21.25 13H6.75C6.33579 13 6 12.6642 6 12.25C6 11.8703 6.28215 11.5565 6.64823 11.5068L6.75 11.5H21.25H6.75ZM3.24929 4.5C3.93925 4.5 4.49858 5.05933 4.49858 5.74929C4.49858 6.43925 3.93925 6.99858 3.24929 6.99858C2.55933 6.99858 2 6.43925 2 5.74929C2 5.05933 2.55933 4.5 3.24929 4.5ZM6.75 5H21.25C21.6642 5 22 5.33579 22 5.75C22 6.1297 21.7178 6.44349 21.3518 6.49315L21.25 6.5H6.75C6.33579 6.5 6 6.16421 6 5.75C6 5.3703 6.28215 5.05651 6.64823 5.00685L6.75 5H21.25H6.75Z"),
			new IconSource("grid", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.5 2C2.67157 2 2 2.67157 2 3.5V5.5C2 6.32843 2.67157 7 3.5 7H5.5C6.32843 7 7 6.32843 7 5.5V3.5C7 2.67157 6.32843 2 5.5 2H3.5ZM3 3.5C3 3.22386 3.22386 3 3.5 3H5.5C5.77614 3 6 3.22386 6 3.5V5.5C6 5.77614 5.77614 6 5.5 6H3.5C3.22386 6 3 5.77614 3 5.5V3.5ZM10.5 2C9.67157 2 9 2.67157 9 3.5V5.5C9 6.32843 9.67157 7 10.5 7H12.5C13.3284 7 14 6.32843 14 5.5V3.5C14 2.67157 13.3284 2 12.5 2H10.5ZM10 3.5C10 3.22386 10.2239 3 10.5 3H12.5C12.7761 3 13 3.22386 13 3.5V5.5C13 5.77614 12.7761 6 12.5 6H10.5C10.2239 6 10 5.77614 10 5.5V3.5ZM2 10.5C2 9.67157 2.67157 9 3.5 9H5.5C6.32843 9 7 9.67157 7 10.5V12.5C7 13.3284 6.32843 14 5.5 14H3.5C2.67157 14 2 13.3284 2 12.5V10.5ZM3.5 10C3.22386 10 3 10.2239 3 10.5V12.5C3 12.7761 3.22386 13 3.5 13H5.5C5.77614 13 6 12.7761 6 12.5V10.5C6 10.2239 5.77614 10 5.5 10H3.5ZM10.5 9C9.67157 9 9 9.67157 9 10.5V12.5C9 13.3284 9.67157 14 10.5 14H12.5C13.3284 14 14 13.3284 14 12.5V10.5C14 9.67157 13.3284 9 12.5 9H10.5ZM10 10.5C10 10.2239 10.2239 10 10.5 10H12.5C12.7761 10 13 10.2239 13 10.5V12.5C13 12.7761 12.7761 13 12.5 13H10.5C10.2239 13 10 12.7761 10 12.5V10.5Z"),
			new IconSource("grid", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M7.5 11C8.32843 11 9 11.6716 9 12.5V16.5C9 17.3284 8.32843 18 7.5 18H3.5C2.67157 18 2 17.3284 2 16.5V12.5C2 11.6716 2.67157 11 3.5 11H7.5ZM16.5 11C17.3284 11 18 11.6716 18 12.5V16.5C18 17.3284 17.3284 18 16.5 18H12.5C11.6716 18 11 17.3284 11 16.5V12.5C11 11.6716 11.6716 11 12.5 11H16.5ZM7.5 12H3.5C3.22386 12 3 12.2239 3 12.5V16.5C3 16.7761 3.22386 17 3.5 17H7.5C7.77614 17 8 16.7761 8 16.5V12.5C8 12.2239 7.77614 12 7.5 12ZM16.5 12H12.5C12.2239 12 12 12.2239 12 12.5V16.5C12 16.7761 12.2239 17 12.5 17H16.5C16.7761 17 17 16.7761 17 16.5V12.5C17 12.2239 16.7761 12 16.5 12ZM7.5 2C8.32843 2 9 2.67157 9 3.5V7.5C9 8.32843 8.32843 9 7.5 9H3.5C2.67157 9 2 8.32843 2 7.5V3.5C2 2.67157 2.67157 2 3.5 2H7.5ZM16.5 2C17.3284 2 18 2.67157 18 3.5V7.5C18 8.32843 17.3284 9 16.5 9H12.5C11.6716 9 11 8.32843 11 7.5V3.5C11 2.67157 11.6716 2 12.5 2H16.5ZM7.5 3H3.5C3.22386 3 3 3.22386 3 3.5V7.5C3 7.77614 3.22386 8 3.5 8H7.5C7.77614 8 8 7.77614 8 7.5V3.5C8 3.22386 7.77614 3 7.5 3ZM16.5 3H12.5C12.2239 3 12 3.22386 12 3.5V7.5C12 7.77614 12.2239 8 12.5 8H16.5C16.7761 8 17 7.77614 17 7.5V3.5C17 3.22386 16.7761 3 16.5 3Z"),
			new IconSource("grid", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M8.75 13C9.99264 13 11 14.0074 11 15.25V18.75C11 19.9926 9.99264 21 8.75 21H5.25C4.00736 21 3 19.9926 3 18.75V15.25C3 14.0074 4.00736 13 5.25 13H8.75ZM18.75 13C19.9926 13 21 14.0074 21 15.25V18.75C21 19.9926 19.9926 21 18.75 21H15.25C14.0074 21 13 19.9926 13 18.75V15.25C13 14.0074 14.0074 13 15.25 13H18.75ZM8.75 14.5H5.25C4.83579 14.5 4.5 14.8358 4.5 15.25V18.75C4.5 19.1642 4.83579 19.5 5.25 19.5H8.75C9.16421 19.5 9.5 19.1642 9.5 18.75V15.25C9.5 14.8358 9.16421 14.5 8.75 14.5ZM18.75 14.5H15.25C14.8358 14.5 14.5 14.8358 14.5 15.25V18.75C14.5 19.1642 14.8358 19.5 15.25 19.5H18.75C19.1642 19.5 19.5 19.1642 19.5 18.75V15.25C19.5 14.8358 19.1642 14.5 18.75 14.5ZM8.75 3C9.99264 3 11 4.00736 11 5.25V8.75C11 9.99264 9.99264 11 8.75 11H5.25C4.00736 11 3 9.99264 3 8.75V5.25C3 4.00736 4.00736 3 5.25 3H8.75ZM18.75 3C19.9926 3 21 4.00736 21 5.25V8.75C21 9.99264 19.9926 11 18.75 11H15.25C14.0074 11 13 9.99264 13 8.75V5.25C13 4.00736 14.0074 3 15.25 3H18.75ZM8.75 4.5H5.25C4.83579 4.5 4.5 4.83579 4.5 5.25V8.75C4.5 9.16421 4.83579 9.5 5.25 9.5H8.75C9.16421 9.5 9.5 9.16421 9.5 8.75V5.25C9.5 4.83579 9.16421 4.5 8.75 4.5ZM18.75 4.5H15.25C14.8358 4.5 14.5 4.83579 14.5 5.25V8.75C14.5 9.16421 14.8358 9.5 15.25 9.5H18.75C19.1642 9.5 19.5 9.16421 19.5 8.75V5.25C19.5 4.83579 19.1642 4.5 18.75 4.5Z"),
			new IconSource("grid", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M10.75 15C11.9926 15 13 16.0074 13 17.25V22.75C13 23.9926 11.9926 25 10.75 25H5.25C4.00736 25 3 23.9926 3 22.75V17.25C3 16.0074 4.00736 15 5.25 15H10.75ZM22.75 15C23.9926 15 25 16.0074 25 17.25V22.75C25 23.9926 23.9926 25 22.75 25H17.25C16.0074 25 15 23.9926 15 22.75V17.25C15 16.0074 16.0074 15 17.25 15H22.75ZM10.75 16.5H5.25C4.83579 16.5 4.5 16.8358 4.5 17.25V22.75C4.5 23.1642 4.83579 23.5 5.25 23.5H10.75C11.1642 23.5 11.5 23.1642 11.5 22.75V17.25C11.5 16.8358 11.1642 16.5 10.75 16.5ZM22.75 16.5H17.25C16.8358 16.5 16.5 16.8358 16.5 17.25V22.75C16.5 23.1642 16.8358 23.5 17.25 23.5H22.75C23.1642 23.5 23.5 23.1642 23.5 22.75V17.25C23.5 16.8358 23.1642 16.5 22.75 16.5ZM10.75 3C11.9926 3 13 4.00736 13 5.25V10.75C13 11.9926 11.9926 13 10.75 13H5.25C4.00736 13 3 11.9926 3 10.75V5.25C3 4.00736 4.00736 3 5.25 3H10.75ZM22.75 3C23.9926 3 25 4.00736 25 5.25V10.75C25 11.9926 23.9926 13 22.75 13H17.25C16.0074 13 15 11.9926 15 10.75V5.25C15 4.00736 16.0074 3 17.25 3H22.75ZM10.75 4.5H5.25C4.83579 4.5 4.5 4.83579 4.5 5.25V10.75C4.5 11.1642 4.83579 11.5 5.25 11.5H10.75C11.1642 11.5 11.5 11.1642 11.5 10.75V5.25C11.5 4.83579 11.1642 4.5 10.75 4.5ZM22.75 4.5H17.25C16.8358 4.5 16.5 4.83579 16.5 5.25V10.75C16.5 11.1642 16.8358 11.5 17.25 11.5H22.75C23.1642 11.5 23.5 11.1642 23.5 10.75V5.25C23.5 4.83579 23.1642 4.5 22.75 4.5Z"),
			new IconSource("chevron_down", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.14645 5.64645C3.34171 5.45118 3.65829 5.45118 3.85355 5.64645L8 9.79289L12.1464 5.64645C12.3417 5.45118 12.6583 5.45118 12.8536 5.64645C13.0488 5.84171 13.0488 6.15829 12.8536 6.35355L8.35355 10.8536C8.15829 11.0488 7.84171 11.0488 7.64645 10.8536L3.14645 6.35355C2.95118 6.15829 2.95118 5.84171 3.14645 5.64645Z"),
			new IconSource("chevron_down", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M15.8537 7.64582C16.0493 7.84073 16.0499 8.15731 15.855 8.35292L10.39 13.8374C10.1751 14.0531 9.82574 14.0531 9.6108 13.8374L4.14582 8.35292C3.9509 8.15731 3.95147 7.84073 4.14708 7.64582C4.34269 7.4509 4.65927 7.45147 4.85418 7.64708L10.0004 12.8117L15.1466 7.64708C15.3415 7.45147 15.6581 7.4509 15.8537 7.64582Z"),
			new IconSource("chevron_down", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M4.21967 8.46967C4.51256 8.17678 4.98744 8.17678 5.28033 8.46967L12 15.1893L18.7197 8.46967C19.0126 8.17678 19.4874 8.17678 19.7803 8.46967C20.0732 8.76256 20.0732 9.23744 19.7803 9.53033L12.5303 16.7803C12.2374 17.0732 11.7626 17.0732 11.4697 16.7803L4.21967 9.53033C3.92678 9.23744 3.92678 8.76256 4.21967 8.46967Z"),
			new IconSource("chevron_down", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M4.21967 9.46967C4.51256 9.17678 4.98744 9.17678 5.28033 9.46967L14 18.1893L22.7197 9.46967C23.0126 9.17678 23.4874 9.17678 23.7803 9.46967C24.0732 9.76256 24.0732 10.2374 23.7803 10.5303L14.5303 19.7803C14.2374 20.0732 13.7626 20.0732 13.4697 19.7803L4.21967 10.5303C3.92678 10.2374 3.92678 9.76256 4.21967 9.46967Z"),
			new IconSource("chevron_down", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M5.29289 11.2929C5.68342 10.9024 6.31658 10.9024 6.70711 11.2929L16 20.5858L25.2929 11.2929C25.6834 10.9024 26.3166 10.9024 26.7071 11.2929C27.0976 11.6834 27.0976 12.3166 26.7071 12.7071L16.7071 22.7071C16.3166 23.0976 15.6834 23.0976 15.2929 22.7071L5.29289 12.7071C4.90237 12.3166 4.90237 11.6834 5.29289 11.2929Z"),
			new IconSource("chevron_up", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.14645 10.3536C3.34171 10.5488 3.65829 10.5488 3.85355 10.3536L8 6.20711L12.1464 10.3536C12.3417 10.5488 12.6583 10.5488 12.8536 10.3536C13.0488 10.1583 13.0488 9.84171 12.8536 9.64645L8.35355 5.14645C8.15829 4.95118 7.84171 4.95118 7.64645 5.14645L3.14645 9.64645C2.95118 9.84171 2.95118 10.1583 3.14645 10.3536Z"),
			new IconSource("chevron_up", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4.14708 12.3534C3.95147 12.1585 3.9509 11.8419 4.14582 11.6463L9.6108 6.16178C9.82574 5.94607 10.1751 5.94607 10.39 6.16178L15.855 11.6463C16.0499 11.8419 16.0493 12.1585 15.8537 12.3534C15.6581 12.5483 15.3415 12.5477 15.1466 12.3521L10.0004 7.18753L4.85418 12.3521C4.65927 12.5477 4.34269 12.5483 4.14708 12.3534Z"),
			new IconSource("chevron_up", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M4.21967 15.5303C4.51256 15.8232 4.98744 15.8232 5.28033 15.5303L12 8.81066L18.7197 15.5303C19.0126 15.8232 19.4874 15.8232 19.7803 15.5303C20.0732 15.2374 20.0732 14.7626 19.7803 14.4697L12.5303 7.21967C12.2374 6.92678 11.7626 6.92678 11.4697 7.21967L4.21967 14.4697C3.92678 14.7626 3.92678 15.2374 4.21967 15.5303Z"),
			new IconSource("chevron_up", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M23.7803 18.5303C23.4874 18.8232 23.0126 18.8232 22.7197 18.5303L14 9.81066L5.28033 18.5303C4.98744 18.8232 4.51256 18.8232 4.21967 18.5303C3.92678 18.2374 3.92678 17.7626 4.21967 17.4697L13.4697 8.21967C13.7626 7.92678 14.2374 7.92678 14.5303 8.21967L23.7803 17.4697C24.0732 17.7626 24.0732 18.2374 23.7803 18.5303Z"),
			new IconSource("chevron_up", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M5.29289 20.7071C5.68342 21.0976 6.31658 21.0976 6.70711 20.7071L16 11.4142L25.2929 20.7071C25.6834 21.0976 26.3166 21.0976 26.7071 20.7071C27.0976 20.3166 27.0976 19.6834 26.7071 19.2929L16.7071 9.29289C16.3166 8.90237 15.6834 8.90237 15.2929 9.29289L5.29289 19.2929C4.90237 19.6834 4.90237 20.3166 5.29289 20.7071Z"),
			new IconSource("chevron_double_down", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.14643 7.64646C3.34158 7.45131 3.65817 7.45154 3.85346 7.64646L7.99998 11.793L12.1465 7.64646C12.3172 7.47569 12.5814 7.4541 12.7754 7.582C12.803 7.60021 12.8292 7.62221 12.8535 7.64646C12.8778 7.67074 12.8997 7.69702 12.918 7.72458C13.0278 7.89092 13.0278 8.10902 12.918 8.27537C12.8997 8.30307 12.8779 8.3291 12.8535 8.35349L8.35349 12.8535C8.3291 12.8779 8.30307 12.8997 8.27537 12.918C8.10903 13.0278 7.89092 13.0278 7.72458 12.918C7.69702 12.8997 7.67074 12.8778 7.64646 12.8535L3.14643 8.35349C2.95151 8.1582 2.95128 7.8416 3.14643 7.64646ZM3.14643 3.64643C3.34158 3.45128 3.65817 3.45151 3.85346 3.64643L7.99998 7.79294L12.1465 3.64643C12.3172 3.47567 12.5814 3.45407 12.7754 3.58197C12.803 3.60018 12.8292 3.62218 12.8535 3.64643C12.8778 3.67071 12.8997 3.69699 12.918 3.72455C13.0278 3.89089 13.0278 4.109 12.918 4.27534C12.8997 4.30304 12.8779 4.32907 12.8535 4.35346L8.35349 8.8535C8.3291 8.87789 8.30307 8.89965 8.27537 8.91795C8.10903 9.02776 7.89092 9.02777 7.72458 8.91795C7.69702 8.89971 7.67074 8.87777 7.64646 8.8535L3.14643 4.35346C3.12218 4.32918 3.10018 4.3029 3.08197 4.27534C2.95407 4.08129 2.97567 3.81719 3.14643 3.64643Z"),
			new IconSource("chevron_double_down", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M15.1465 9.64648C15.3418 9.45122 15.6583 9.45122 15.8535 9.64648C16.0487 9.84175 16.0488 10.1583 15.8535 10.3535L10.3535 15.8535C10.2598 15.9472 10.1326 16 10 16C9.86745 16 9.74025 15.9472 9.64649 15.8535L4.14648 10.3535C3.95122 10.1582 3.95122 9.84174 4.14648 9.64648C4.34175 9.45122 4.65825 9.45122 4.85352 9.64648L10 14.793L15.1465 9.64648ZM15.1465 4.64648C15.3418 4.45121 15.6583 4.45121 15.8535 4.64648C16.0487 4.84175 16.0488 5.15828 15.8535 5.35351L10.3535 10.8535C10.2598 10.9472 10.1326 11 10 11C9.86745 11 9.74025 10.9472 9.64649 10.8535L4.14648 5.35351C3.95122 5.15825 3.95122 4.84174 4.14648 4.64648C4.34175 4.45121 4.65825 4.45121 4.85352 4.64648L10 9.79296L15.1465 4.64648Z"),
			new IconSource("chevron_double_up", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M7.72451 7.0828C7.89086 6.97298 8.10895 6.97299 8.2753 7.0828C8.303 7.1011 8.32903 7.12286 8.35342 7.14726L12.8534 11.6473C12.8778 11.6717 12.8996 11.6977 12.9179 11.7254C13.0277 11.8918 13.0277 12.1099 12.9179 12.2762C12.8996 12.3038 12.8777 12.33 12.8534 12.3543C12.8291 12.3786 12.8029 12.4006 12.7753 12.4188C12.5813 12.5467 12.3172 12.5251 12.1464 12.3543L7.99991 8.20781L3.85341 12.3543C3.65813 12.5492 3.34153 12.5495 3.14638 12.3543C2.97562 12.1836 2.95402 11.9195 3.08193 11.7254C3.10014 11.6979 3.12214 11.6716 3.14638 11.6473L7.64639 7.14726C7.67067 7.12298 7.69695 7.10105 7.72451 7.0828ZM7.72451 3.08278C7.89086 2.97295 8.10895 2.97296 8.2753 3.08278C8.303 3.10108 8.32903 3.12284 8.35342 3.14723L12.8534 7.64726C12.8778 7.67165 12.8996 7.69769 12.9179 7.72539C13.0277 7.89173 13.0277 8.10984 12.9179 8.27617C12.8996 8.30373 12.8777 8.33002 12.8534 8.3543C12.8291 8.37854 12.8029 8.40054 12.7753 8.41875C12.5813 8.54666 12.3172 8.52505 12.1464 8.3543L7.99991 4.20778L3.85341 8.3543C3.65813 8.54922 3.34153 8.54944 3.14638 8.3543C2.95123 8.15915 2.95146 7.84255 3.14638 7.64726L7.64639 3.14723C7.67067 3.12295 7.69695 3.10102 7.72451 3.08278Z"),
			new IconSource("chevron_double_up", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M10 9C10.1326 9.00003 10.2598 9.05275 10.3535 9.14649L15.8535 14.6465C16.0487 14.8417 16.0487 15.1583 15.8535 15.3535C15.6583 15.5488 15.3418 15.5488 15.1465 15.3535L10 10.207L4.85351 15.3535C4.65825 15.5488 4.34174 15.5488 4.14648 15.3535C3.95121 15.1583 3.95121 14.8418 4.14648 14.6465L9.64649 9.14649C9.74024 9.05278 9.86744 9 10 9ZM10 4C10.1326 4.00003 10.2598 4.05274 10.3535 4.14648L15.8535 9.64649C16.0487 9.84172 16.0487 10.1583 15.8535 10.3535C15.6583 10.5488 15.3418 10.5488 15.1465 10.3535L10 5.20703L4.85351 10.3535C4.65825 10.5488 4.34174 10.5488 4.14648 10.3535C3.95121 10.1583 3.95121 9.84175 4.14648 9.64649L9.64649 4.14648C9.74024 4.05278 9.86744 4 10 4Z"),
			new IconSource("arrow_enter", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M6.5 2C6.22386 2 6 2.22386 6 2.5C6 2.77614 6.22386 3 6.5 3H11.5C12.3284 3 13 3.67157 13 4.5V11.5C13 12.3284 12.3284 13 11.5 13H6.5C6.22386 13 6 13.2239 6 13.5C6 13.7761 6.22386 14 6.5 14H11.5C12.8807 14 14 12.8807 14 11.5V4.5C14 3.11929 12.8807 2 11.5 2H6.5ZM9.85355 7.64645L6.85355 4.64645C6.65829 4.45118 6.34171 4.45118 6.14645 4.64645C5.95118 4.84171 5.95118 5.15829 6.14645 5.35355L8.29289 7.5H1.5C1.22386 7.5 1 7.72386 1 8C1 8.27614 1.22386 8.5 1.5 8.5H8.29289L6.14645 10.6464C5.95118 10.8417 5.95118 11.1583 6.14645 11.3536C6.34171 11.5488 6.65829 11.5488 6.85355 11.3536L9.85355 8.35355C10.0488 8.15829 10.0488 7.84171 9.85355 7.64645Z"),
			new IconSource("arrow_enter", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M7.5 17C7.22386 17 7 16.7761 7 16.5C7 16.2239 7.22386 16 7.5 16H14C15.1046 16 16 15.1046 16 14V6C16 4.89543 15.1046 4 14 4H7.5C7.22386 4 7 3.77614 7 3.5C7 3.22386 7.22386 3 7.5 3H14C15.6569 3 17 4.34315 17 6V14C17 15.6569 15.6569 17 14 17H7.5ZM7.64645 6.14645C7.84171 5.95118 8.15829 5.95118 8.35355 6.14645L11.8536 9.64645C12.0488 9.84171 12.0488 10.1583 11.8536 10.3536L8.35355 13.8536C8.15829 14.0488 7.84171 14.0488 7.64645 13.8536C7.45118 13.6583 7.45118 13.3417 7.64645 13.1464L10.2929 10.5H1.5C1.22386 10.5 1 10.2761 1 10C1 9.72386 1.22386 9.5 1.5 9.5H10.2929L7.64645 6.85355C7.45118 6.65829 7.45118 6.34171 7.64645 6.14645Z"),
			new IconSource("next", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M14 2.5C14 2.22386 13.7762 2 13.5 2C13.2239 2 13 2.22386 13 2.5V13.5C13 13.7761 13.2239 14 13.5 14C13.7762 14 14 13.7761 14 13.5V2.5ZM2.00024 3.00176C2.00024 2.19 2.91639 1.71649 3.57861 2.18598L10.579 7.14909C11.1388 7.54594 11.1417 8.37575 10.5847 8.77654L3.58435 13.8142C2.92285 14.2903 2.00024 13.8176 2.00024 13.0026V3.00176ZM10.0006 7.96486L3.00024 3.00176L3.00024 13.0026L10.0006 7.96486Z"),
			new IconSource("next", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M17.0001 3.5C17.0001 3.22386 16.7762 3 16.5001 3C16.2239 3 16.0001 3.22386 16.0001 3.5V16.5C16.0001 16.7761 16.2239 17 16.5001 17C16.7762 17 17.0001 16.7761 17.0001 16.5V3.5ZM3.00024 4.25211C3.00024 3.25186 4.11666 2.65676 4.94707 3.21436L13.4463 8.92144C14.1808 9.41464 14.1846 10.4938 13.4535 10.9921L4.95424 16.785C4.12447 17.3505 3.00024 16.7562 3.00024 15.7521V4.25211ZM4.38961 4.04456C4.22353 3.93304 4.00024 4.05206 4.00024 4.25211V15.7521C4.00024 15.9529 4.22509 16.0717 4.39104 15.9586L12.8903 10.1658C13.0365 10.0661 13.0358 9.85028 12.8889 9.75165L4.38961 4.04456Z"),
			new IconSource("next", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.00012 4.753C3.00012 3.34519 4.57793 2.51363 5.73928 3.30937L16.2377 10.5028C17.2479 11.1949 17.2531 12.6839 16.2478 13.3831L5.74932 20.6847C4.58899 21.4917 3.00012 20.6613 3.00012 19.248V4.753ZM4.89143 4.54677C4.72552 4.43309 4.50012 4.55189 4.50012 4.753V19.248C4.50012 19.4499 4.72711 19.5685 4.89287 19.4532L15.3913 12.1517C15.5349 12.0518 15.5342 11.8391 15.3899 11.7402L4.89143 4.54677ZM21 3.75C21 3.33579 20.6642 3 20.25 3C19.8358 3 19.5 3.33579 19.5 3.75V20.25C19.5 20.6642 19.8358 21 20.25 21C20.6642 21 21 20.6642 21 20.25V3.75Z"),
			new IconSource("next", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M23.5 3.75C23.5 3.33579 23.8358 3 24.25 3C24.6642 3 25 3.33579 25 3.75V24.25C25 24.6642 24.6642 25 24.25 25C23.8358 25 23.5 24.6642 23.5 24.25V3.75ZM3.00024 5.25389C3.00024 3.4376 5.0413 2.36983 6.53333 3.4056L19.0374 12.086C20.3212 12.9771 20.3278 14.8734 19.0504 15.7735L6.54625 24.5844C5.05564 25.6347 3.00024 24.5686 3.00024 22.7451V5.25389ZM5.67794 4.6378C5.1806 4.29254 4.50024 4.64846 4.50024 5.25389V22.7451C4.50024 23.3529 5.18538 23.7083 5.68225 23.3582L18.1864 14.5474C18.6122 14.2473 18.61 13.6152 18.1821 13.3182L5.67794 4.6378Z"),
			new IconSource("next", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M26.002 5C26.002 4.44772 26.4497 4 27.002 4C27.5542 4 28.002 4.44772 28.002 5V27C28.002 27.5523 27.5542 28 27.002 28C26.4497 28 26.002 27.5523 26.002 27V5ZM3.99915 6.50423C3.99915 4.50211 6.23519 3.31225 7.89576 4.43072L21.8991 13.8626C23.3647 14.8498 23.3714 17.0046 21.912 18.0009L7.90866 27.5603C6.24925 28.6931 3.99915 27.5047 3.99915 25.4955V6.50423ZM6.77847 6.08953C6.44635 5.86584 5.99915 6.10381 5.99915 6.50423V25.4955C5.99915 25.8973 6.44917 26.135 6.78105 25.9084L20.7844 16.3491C21.0763 16.1498 21.0749 15.7188 20.7818 15.5214L6.77847 6.08953Z"),
			new IconSource("live", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.05132 3.04985C3.24653 2.8549 3.56315 2.85487 3.75835 3.04985C3.95353 3.24503 3.95337 3.5616 3.75835 3.75688C1.41527 6.09996 1.41541 9.89907 3.75835 12.2422C3.95361 12.4375 3.95361 12.755 3.75835 12.9502C3.56316 13.1452 3.24653 13.1451 3.05132 12.9502C0.317651 10.2166 0.317651 5.78352 3.05132 3.04985ZM12.2437 3.04985C12.4389 2.8549 12.7555 2.85487 12.9507 3.04985C15.6844 5.78352 15.6844 10.2166 12.9507 12.9502C12.7555 13.1452 12.4389 13.1451 12.2437 12.9502C12.0484 12.755 12.0484 12.4375 12.2437 12.2422C14.5866 9.89907 14.5868 6.09994 12.2437 3.75688C12.0486 3.5616 12.0485 3.24504 12.2437 3.04985ZM4.8189 4.8184C5.01416 4.62314 5.33067 4.62314 5.52593 4.8184C5.72091 5.01369 5.7211 5.33027 5.52593 5.52543C4.15916 6.89227 4.15913 9.10784 5.52593 10.4747C5.72111 10.6698 5.72095 10.9864 5.52593 11.1817C5.33067 11.3769 5.01416 11.3769 4.8189 11.1817C3.06158 9.42434 3.0616 6.57576 4.8189 4.8184ZM10.4751 4.8184C10.6704 4.62315 10.9869 4.62316 11.1822 4.8184C12.9395 6.57576 12.9395 9.42432 11.1822 11.1817C10.9869 11.3769 10.6704 11.3769 10.4751 11.1817C10.2802 10.9864 10.28 10.6698 10.4751 10.4747C11.842 9.10782 11.842 6.89227 10.4751 5.52543C10.28 5.33029 10.2802 5.01369 10.4751 4.8184ZM7.99956 7.00004C8.55185 7.00004 8.99956 7.44776 8.99956 8.00004C8.99954 8.55231 8.55184 9.00004 7.99956 9.00004C7.44749 8.9998 6.99958 8.55216 6.99956 8.00004C6.99956 7.44791 7.44748 7.00028 7.99956 7.00004Z"),
			new IconSource("live", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4.34436 4.34302C4.53961 4.14788 4.85615 4.14786 5.05139 4.34302C5.24662 4.53825 5.24655 4.85478 5.05139 5.05005C2.31776 7.78367 2.31786 12.2158 5.05139 14.9495C5.24659 15.1447 5.24647 15.4612 5.05139 15.6565C4.85613 15.8518 4.53962 15.8518 4.34436 15.6565C1.2203 12.5323 1.22021 7.46716 4.34436 4.34302ZM14.9508 4.34302C15.146 4.14783 15.4626 4.14791 15.6578 4.34302C18.782 7.46718 18.7819 12.5323 15.6578 15.6565C15.4626 15.8518 15.1461 15.8518 14.9508 15.6565C14.7558 15.4612 14.7556 15.1447 14.9508 14.9495C17.6844 12.2158 17.6844 7.78369 14.9508 5.05005C14.7557 4.85478 14.7556 4.53823 14.9508 4.34302ZM6.28674 6.28735C6.482 6.09242 6.79861 6.09226 6.99377 6.28735C7.18893 6.48251 7.18872 6.7991 6.99377 6.99439C5.33404 8.65411 5.33404 11.3454 6.99377 13.0051C7.18891 13.2003 7.18883 13.5169 6.99377 13.7122C6.79853 13.9074 6.48201 13.9074 6.28674 13.7122C4.23649 11.6619 4.23649 8.33761 6.28674 6.28735ZM13.0035 6.28735C13.1988 6.09209 13.5163 6.09209 13.7115 6.28735C15.7616 8.33751 15.7615 11.6619 13.7115 13.7122C13.5163 13.9074 13.1988 13.9074 13.0035 13.7122C12.8086 13.5169 12.8085 13.2003 13.0035 13.0051C14.6633 11.3454 14.6633 8.65411 13.0035 6.99439C12.8087 6.79911 12.8085 6.48248 13.0035 6.28735ZM9.99963 8.75024C10.69 8.75024 11.2496 9.30989 11.2496 10.0002C11.2495 10.6905 10.6899 11.2502 9.99963 11.2502C9.30953 11.25 8.74976 10.6904 8.74963 10.0002C8.74963 9.31001 9.30944 8.75045 9.99963 8.75024Z"),
			new IconSource("live", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.98959 4.92865C6.28249 5.22155 6.28249 5.69642 5.98959 5.98931C2.67014 9.30877 2.67014 14.6907 5.98959 18.0101C6.28249 18.303 6.28249 18.7779 5.98959 19.0708C5.6967 19.3637 5.22183 19.3637 4.92893 19.0708C1.02369 15.1655 1.02369 8.8339 4.92893 4.92865C5.22183 4.63576 5.6967 4.63576 5.98959 4.92865ZM19.0711 4.92865C22.9763 8.8339 22.9763 15.1655 19.0711 19.0708C18.7782 19.3637 18.3033 19.3637 18.0104 19.0708C17.7175 18.7779 17.7175 18.303 18.0104 18.0101C21.3299 14.6907 21.3299 9.30877 18.0104 5.98931C17.7175 5.69642 17.7175 5.22155 18.0104 4.92865C18.3033 4.63576 18.7782 4.63576 19.0711 4.92865ZM8.81802 7.75708C9.11091 8.04997 9.11091 8.52485 8.81802 8.81774C7.06066 10.5751 7.06066 13.4243 8.81802 15.1817C9.11091 15.4746 9.11091 15.9495 8.81802 16.2424C8.52513 16.5353 8.05025 16.5353 7.75736 16.2424C5.41421 13.8992 5.41421 10.1002 7.75736 7.75708C8.05025 7.46419 8.52513 7.46419 8.81802 7.75708ZM16.2426 7.75708C18.5858 10.1002 18.5858 13.8992 16.2426 16.2424C15.9497 16.5353 15.4749 16.5353 15.182 16.2424C14.8891 15.9495 14.8891 15.4746 15.182 15.1817C16.9393 13.4243 16.9393 10.5751 15.182 8.81774C14.8891 8.52485 14.8891 8.04997 15.182 7.75708C15.4749 7.46419 15.9497 7.46419 16.2426 7.75708ZM12 10.4997C12.8284 10.4997 13.5 11.1713 13.5 11.9997C13.5 12.8281 12.8284 13.4997 12 13.4997C11.1716 13.4997 10.5 12.8281 10.5 11.9997C10.5 11.1713 11.1716 10.4997 12 10.4997Z"),
			new IconSource("desktop", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M4 2C2.89543 2 2 2.89543 2 4V9.99729C2 11.1019 2.89543 11.9973 4 11.9973H6.005V13.008L4.50598 13.008C4.22984 13.008 4.00598 13.2319 4.00599 13.508C4.00599 13.7841 4.22985 14.008 4.50599 14.008L11.5021 14.0079C11.7783 14.0079 12.0021 13.7841 12.0021 13.5079C12.0021 13.2318 11.7783 13.0079 11.5021 13.0079L10.0029 13.0079V11.9973H12C13.1046 11.9973 14 11.1019 14 9.99729V4C14 2.89543 13.1046 2 12 2H4ZM9.00293 11.9973V13.008L7.005 13.008V11.9973H9.00293ZM3 4C3 3.44772 3.44772 3 4 3H12C12.5523 3 13 3.44772 13 4V9.99729C13 10.5496 12.5523 10.9973 12 10.9973H4C3.44772 10.9973 3 10.5496 3 9.99729V4Z"),
			new IconSource("desktop", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4 2C2.89543 2 2 2.89543 2 4V13C2 14.1046 2.89543 15 4 15H7V17H5.5C5.22386 17 5 17.2239 5 17.5C5 17.7761 5.22386 18 5.5 18H14.5C14.7761 18 15 17.7761 15 17.5C15 17.2239 14.7761 17 14.5 17H13V15H16C17.1046 15 18 14.1046 18 13V4C18 2.89543 17.1046 2 16 2H4ZM12 15V17H8V15H12ZM3 4C3 3.44772 3.44772 3 4 3H16C16.5523 3 17 3.44772 17 4V13C17 13.5523 16.5523 14 16 14H4C3.44772 14 3 13.5523 3 13V4Z"),
			new IconSource("desktop", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M6.75 22.0004C6.33579 22.0004 6 21.6647 6 21.2504C6 20.8707 6.28215 20.557 6.64823 20.5073L6.75 20.5004L8.499 20.5V18.002L4.25 18.0023C3.05914 18.0023 2.08436 17.0771 2.00519 15.9063L2 15.7523V5.25C2 4.05914 2.92516 3.08436 4.09595 3.00519L4.25 3H19.7488C20.9397 3 21.9145 3.92516 21.9936 5.09595L21.9988 5.25V15.7523C21.9988 16.9431 21.0737 17.9179 19.9029 17.9971L19.7488 18.0023L15.499 18.002V20.5L17.25 20.5004C17.6642 20.5004 18 20.8362 18 21.2504C18 21.6301 17.7178 21.9439 17.3518 21.9936L17.25 22.0004H6.75ZM13.998 18.002H9.998L9.999 20.5004H13.999L13.998 18.002ZM19.7488 4.5H4.25C3.8703 4.5 3.55651 4.78215 3.50685 5.14823L3.5 5.25V15.7523C3.5 16.132 3.78215 16.4458 4.14823 16.4954L4.25 16.5023H19.7488C20.1285 16.5023 20.4423 16.2201 20.492 15.854L20.4988 15.7523V5.25C20.4988 4.8703 20.2167 4.55651 19.8506 4.50685L19.7488 4.5Z"),
			new IconSource("desktop", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M19.75 25.0004H8.25C7.83579 25.0004 7.5 24.6647 7.5 24.2504C7.5 23.8707 7.78215 23.557 8.14823 23.5073L8.25 23.5004L10 23.5V21.002L4.25 21.0023C3.00736 21.0023 2 19.9949 2 18.7523V5.25C2 4.00736 3.00736 3 4.25 3H23.7488C24.9915 3 25.9988 4.00736 25.9988 5.25V18.7523C25.9988 19.9949 24.9915 21.0023 23.7488 21.0023L17.998 21.002V23.5L19.75 23.5004C20.1297 23.5004 20.4435 23.7826 20.4932 24.1487L20.5 24.2504C20.5 24.6301 20.2178 24.9439 19.8518 24.9936L19.75 25.0004H8.25H19.75ZM16.498 21.002H11.5V23.5004H16.4988L16.498 21.002ZM23.7488 4.5H4.25C3.83579 4.5 3.5 4.83579 3.5 5.25V18.7523C3.5 19.1665 3.83579 19.5023 4.25 19.5023L17.2488 19.5004L17.277 19.502L23.7488 19.5023C24.163 19.5023 24.4988 19.1665 24.4988 18.7523V5.25C24.4988 4.83579 24.163 4.5 23.7488 4.5Z"),
			new IconSource("desktop", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M5.25 3C3.45507 3 2 4.45507 2 6.25V21.75C2 23.5449 3.45507 25 5.25 25H12.01V27.0013L9.01196 27.0013C8.45967 27.0013 8.01196 27.4488 8.01196 28.0007C8.01197 28.5526 8.45969 29 9.01197 29L23.0043 28.9999C23.5565 28.9999 24.0042 28.5525 24.0042 28.0005C24.0042 27.4486 23.5565 27.0012 23.0042 27.0012L20.0059 27.0012V25H26.75C28.5449 25 30 23.5449 30 21.75V6.25C30 4.45507 28.5449 3 26.75 3H5.25ZM18.0059 25V27.0013L14.01 27.0013V25H18.0059ZM4 6.25C4 5.55964 4.55964 5 5.25 5H26.75C27.4404 5 28 5.55964 28 6.25V21.75C28 22.4404 27.4404 23 26.75 23H5.25C4.55964 23 4 22.4404 4 21.75V6.25Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M12.2929 13L14.1464 14.8536C14.3417 15.0488 14.6583 15.0488 14.8536 14.8536C15.0488 14.6583 15.0488 14.3417 14.8536 14.1464L1.85355 1.14645C1.65829 0.951184 1.34171 0.951184 1.14645 1.14645C0.951184 1.34171 0.951184 1.65829 1.14645 1.85355L2.50063 3.20773C1.61734 3.59337 1 4.47458 1 5.49999V10.5C1 11.8807 2.11929 13 3.5 13H12.2929ZM11.2929 12H3.5C2.67157 12 2 11.3284 2 10.5V5.49999C2 4.73751 2.5689 4.10791 3.30539 4.01249L5.0595 5.76661C4.41317 6.15017 4 6.92816 4 8.00003C4 9.13518 4.46341 9.94073 5.17639 10.2972C5.87201 10.645 6.70386 10.5033 7.25355 9.95357C7.44882 9.75831 7.44882 9.44173 7.25355 9.24647C7.05829 9.0512 6.74171 9.0512 6.54645 9.24647C6.29614 9.49677 5.92799 9.55499 5.62361 9.4028C5.33659 9.25929 5 8.86484 5 8.00003C5 7.13521 5.3366 6.74075 5.62361 6.59724C5.68748 6.56531 5.75415 6.54264 5.82195 6.52906L11.2929 12ZM8.45657 6.33522L9.18559 7.06424C9.30781 6.81777 9.47258 6.67276 9.62361 6.59724C9.92799 6.44505 10.2961 6.50326 10.5464 6.75357C10.7417 6.94883 11.0583 6.94884 11.2536 6.75358C11.4488 6.55832 11.4488 6.24173 11.2536 6.04647C10.7039 5.49677 9.87201 5.355 9.17639 5.70282C8.89232 5.84485 8.64788 6.05817 8.45657 6.33522ZM14 10.5C14 10.8833 13.8562 11.2331 13.6196 11.4983L14.3274 12.206C14.7446 11.7593 15 11.1595 15 10.5V5.49999C15 4.11927 13.8807 2.99999 12.5 2.99999H5.12134L6.12134 3.99999H12.5C13.3284 3.99999 14 4.67156 14 5.49999V10.5Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M2.85355 2.14645C2.65829 1.95118 2.34171 1.95118 2.14645 2.14645C1.95118 2.34171 1.95118 2.65829 2.14645 2.85355L3.62555 4.33266C2.66007 4.83117 2 5.83849 2 6.99999V13C2 14.6568 3.34315 16 5 16H15C15.0944 16 15.1878 15.9956 15.28 15.9871L17.1464 17.8536C17.3417 18.0488 17.6583 18.0488 17.8536 17.8536C18.0488 17.6583 18.0488 17.3417 17.8536 17.1464L16.3744 15.6673C16.3744 15.6673 16.3744 15.6673 16.3744 15.6673L15.6118 14.9047C15.6118 14.9047 15.6118 14.9047 15.6118 14.9047L12.6771 11.97C12.6771 11.97 12.6772 11.97 12.6771 11.97L11.6231 10.9159C11.623 10.9159 11.6231 10.916 11.6231 10.9159L10.5041 9.79699C10.5041 9.797 10.5041 9.79698 10.5041 9.79699L8.94169 8.23459C8.9417 8.23458 8.94169 8.23459 8.94169 8.23459L7.76697 7.05983C7.76696 7.05983 7.76699 7.05984 7.76697 7.05983L4.72002 4.01288C4.72001 4.01288 4.72003 4.01288 4.72002 4.01288L2.85355 2.14645ZM14.2929 15H5C3.89543 15 3 14.1046 3 13V6.99999C3 6.10881 3.58287 5.35377 4.38819 5.0953L6.49095 7.19806C6.4609 7.21138 6.43104 7.22546 6.40139 7.24028C5.56341 7.65929 5 8.61484 5 9.99999C5 11.3851 5.56341 12.3407 6.40139 12.7597C7.22201 13.17 8.20386 13.0032 8.85355 12.3535C9.04882 12.1583 9.04882 11.8417 8.85355 11.6464C8.65829 11.4512 8.34171 11.4512 8.14645 11.6464C7.79614 11.9967 7.27799 12.0799 6.84861 11.8653C6.43659 11.6593 6 11.1148 6 9.99999C6 8.88518 6.43659 8.34072 6.84861 8.13471C6.99564 8.06119 7.15307 8.02261 7.3106 8.01771L14.2929 15ZM17.1811 15.0598L16.4736 14.3522C16.8005 13.9962 17 13.5214 17 13V6.99999C17 5.89542 16.1046 4.99999 15 4.99999H7.12134L6.12134 3.99999H15C16.6569 3.99999 18 5.34313 18 6.99999V13C18 13.7976 17.6888 14.5224 17.1811 15.0598ZM11.5444 9.42305L10.7299 8.60851C10.9697 7.96401 11.383 7.4995 11.9014 7.24031C12.722 6.82999 13.7039 6.99676 14.3536 7.64646C14.5488 7.84173 14.5488 8.15831 14.3535 8.35357C14.1583 8.54883 13.8417 8.54883 13.6464 8.35356C13.2961 8.00325 12.778 7.92004 12.3486 8.13474C12.0151 8.30148 11.6656 8.68993 11.5444 9.42305Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.28034 2.21967C2.98745 1.92678 2.51257 1.92677 2.21968 2.21966C1.92678 2.51255 1.92677 2.98742 2.21966 3.28032L3.4698 4.53047C2.5846 5.11109 2 6.11226 2 7.24997V16.7545C2 18.5494 3.45507 20.0045 5.25 20.0045H18.75C18.8132 20.0045 18.8759 20.0027 18.9382 19.9991L20.7194 21.7803C21.0123 22.0732 21.4872 22.0732 21.7801 21.7803C22.073 21.4874 22.073 21.0126 21.7801 20.7197L3.28034 2.21967ZM17.4436 18.5045H5.25C4.2835 18.5045 3.5 17.721 3.5 16.7545V7.24997C3.5 6.52294 3.94336 5.89945 4.57445 5.63513L7.40186 8.46258C6.29288 9.04205 5.5 10.2995 5.5 11.9999C5.5 15.1432 8.21539 16.7746 10.6208 15.4065C10.9809 15.2017 11.1067 14.7438 10.9019 14.3837C10.6971 14.0237 10.2392 13.8978 9.8792 14.1026C8.48411 14.8961 7 14.0045 7 11.9999C7 10.6301 7.69021 9.7809 8.57296 9.63369L17.4436 18.5045ZM20.4295 17.2477C20.4754 17.0913 20.5 16.9258 20.5 16.7545V7.24997C20.5 6.28348 19.7165 5.49998 18.75 5.49998H8.68192L7.18195 3.99999H18.75C20.5449 3.99999 22 5.45506 22 7.24997V16.7545C22 17.3475 21.8412 17.9034 21.5638 18.382L20.4295 17.2477ZM14.5531 11.3712L13.3557 10.1739C14.1688 8.28645 16.2384 7.52423 18.1216 8.5985C18.4814 8.80374 18.6067 9.26178 18.4015 9.62157C18.1962 9.98136 17.7382 10.1066 17.3784 9.90141C16.1366 9.19303 14.8292 9.81615 14.5531 11.3712Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M3.28033 2.21967C2.98744 1.92678 2.51257 1.92678 2.21967 2.21966C1.92678 2.51255 1.92677 2.98742 2.21967 3.28032L3.75117 4.81184C2.68984 5.58453 2 6.83669 2 8.24997V19.7499C2 22.0971 3.90279 23.9999 6.25 23.9999H21.75C22.1148 23.9999 22.4688 23.9539 22.8066 23.8675L24.7194 25.7803C25.0123 26.0732 25.4872 26.0732 25.7801 25.7803C26.073 25.4874 26.073 25.0126 25.7801 24.7197L3.28033 2.21967ZM21.439 22.4999H6.25C4.73122 22.4999 3.5 21.2687 3.5 19.7499V8.24997C3.5 7.24978 4.03396 6.37431 4.83238 5.89306L8.90367 9.96439C8.27154 10.2078 7.6766 10.631 7.23486 11.282C6.76315 11.9771 6.5 12.8817 6.5 14C6.5 15.1182 6.76315 16.0228 7.23486 16.7179C7.70778 17.4148 8.35627 17.8506 9.03801 18.0844C9.71269 18.3157 10.4214 18.3508 11.0513 18.2574C11.6727 18.1654 12.2765 17.9392 12.7185 17.5856C13.042 17.3268 13.0944 16.8548 12.8357 16.5314C12.5769 16.2079 12.1049 16.1555 11.7815 16.4143C11.5985 16.5606 11.2648 16.7094 10.8315 16.7736C10.4068 16.8366 9.94356 16.8091 9.52449 16.6655C9.11248 16.5242 8.74535 16.2725 8.47607 15.8757C8.2056 15.4771 8 14.8817 8 14C8 13.1182 8.2056 12.5228 8.47608 12.1242C8.74535 11.7274 9.11248 11.4757 9.52449 11.3344C9.72418 11.266 9.93389 11.2239 10.1447 11.2054L15.9929 17.0537C16.1021 17.1794 16.2177 17.2944 16.3385 17.3993L21.439 22.4999ZM7.18191 4L8.68188 5.49999H21.75C23.2688 5.49999 24.5 6.7312 24.5 8.24997V19.7499C24.5 20.2049 24.3895 20.6341 24.1939 21.0121L25.2877 22.106C25.7377 21.4316 26 20.6214 26 19.7499V8.24997C26 5.90277 24.0972 4 21.75 4H7.18191ZM21.2185 17.5855C21.1355 17.652 21.0467 17.714 20.9531 17.7714L19.8322 16.6504C20.0245 16.5816 20.1774 16.4975 20.2815 16.4143C20.6049 16.1555 21.0769 16.2079 21.3357 16.5314C21.5944 16.8548 21.542 17.3268 21.2185 17.5855ZM15.3056 12.1237L16.5405 13.3587C16.6133 12.817 16.7771 12.4174 16.9761 12.1242C17.2454 11.7274 17.6125 11.4757 18.0245 11.3344C18.4436 11.1907 18.9068 11.1633 19.3315 11.2262C19.7648 11.2904 20.0985 11.4392 20.2815 11.5856C20.6049 11.8444 21.0769 11.7919 21.3356 11.4685C21.5944 11.145 21.542 10.6731 21.2185 10.4143C20.7765 10.0607 20.1727 9.83449 19.5513 9.74244C18.9214 9.64911 18.2127 9.68419 17.538 9.91551C16.8563 10.1492 16.2078 10.585 15.7349 11.282C15.5633 11.5348 15.4193 11.8153 15.3056 12.1237Z"),
			new IconSource("video_play_pause", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M10 6.5C10 5.67157 10.6716 5 11.5 5H12.5C13.3284 5 14 5.67157 14 6.5V13.5C14 14.3284 13.3284 15 12.5 15H11.5C10.6716 15 10 14.3284 10 13.5V6.5ZM11.5 6C11.2239 6 11 6.22386 11 6.5V13.5C11 13.7761 11.2239 14 11.5 14H12.5C12.7761 14 13 13.7761 13 13.5V6.5C13 6.22386 12.7761 6 12.5 6H11.5ZM15 6.5C15 5.67157 15.6716 5 16.5 5H17.5C18.3284 5 19 5.67157 19 6.5V13.5C19 14.3284 18.3284 15 17.5 15H16.5C15.6716 15 15 14.3284 15 13.5V6.5ZM16.5 6C16.2239 6 16 6.22386 16 6.5V13.5C16 13.7761 16.2239 14 16.5 14H17.5C17.7761 14 18 13.7761 18 13.5V6.5C18 6.22386 17.7761 6 17.5 6H16.5ZM3 6.50203C3 6.3042 3.21889 6.18475 3.38527 6.29179L8.88147 9.8279C9.03533 9.92689 9.03423 10.1522 8.87943 10.2497L3.38323 13.7111C3.21675 13.8159 3 13.6963 3 13.4995V6.50203ZM3.92633 5.45081C3.09446 4.9156 2 5.51287 2 6.50203V13.4995C2 14.4832 3.08373 15.0815 3.91613 14.5572L9.41233 11.0959C10.1864 10.6084 10.1918 9.48187 9.42253 8.98692L3.92633 5.45081Z"),
			new IconSource("video_play_pause", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12 7.75C12 6.7835 12.7835 6 13.75 6H15.25C16.2165 6 17 6.7835 17 7.75V16.25C17 17.2165 16.2165 18 15.25 18H13.75C12.7835 18 12 17.2165 12 16.25V12.1198C11.9757 12.6708 11.6956 13.2117 11.1607 13.5368L4.65892 17.4886C3.4927 18.1974 2 17.3578 2 15.9931V8.00436C2 6.63216 3.50737 5.79381 4.67313 6.51765L11.1749 10.5547C11.7012 10.8815 11.9759 11.4174 12 11.9626V7.75ZM13.75 7.5C13.6119 7.5 13.5 7.61193 13.5 7.75V16.25C13.5 16.3881 13.6119 16.5 13.75 16.5H15.25C15.3881 16.5 15.5 16.3881 15.5 16.25V7.75C15.5 7.61193 15.3881 7.5 15.25 7.5H13.75ZM18 7.75C18 6.7835 18.7835 6 19.75 6H21.25C22.2165 6 23 6.7835 23 7.75V16.25C23 17.2165 22.2165 18 21.25 18H19.75C18.7835 18 18 17.2165 18 16.25V7.75ZM19.75 7.5C19.6119 7.5 19.5 7.61193 19.5 7.75V16.25C19.5 16.3881 19.6119 16.5 19.75 16.5H21.25C21.3881 16.5 21.5 16.3881 21.5 16.25V7.75C21.5 7.61193 21.3881 7.5 21.25 7.5H19.75ZM3.5 8.00436V15.9931C3.5 16.1881 3.71324 16.308 3.87985 16.2067L10.3816 12.255C10.5409 12.1582 10.542 11.9274 10.3836 11.829L3.88188 7.79198C3.71534 7.68857 3.5 7.80834 3.5 8.00436Z"),
			new IconSource("arrow_step_over", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M10.1465 2.14645C9.9512 2.34171 9.9512 2.65829 10.1465 2.85355L12.2929 5L9.95705 5C8.48941 4.99996 7.0519 4.99993 5.73136 5.39609C4.3656 5.80582 3.14605 6.62961 2.08403 8.22265C1.93085 8.45242 1.99294 8.76285 2.2227 8.91603C2.45247 9.0692 2.7629 9.00712 2.91607 8.77735C3.85404 7.3704 4.88448 6.69419 6.01871 6.35391C7.18528 6.00394 8.48258 6 10 6H12.2929L10.1465 8.14645C9.9512 8.34171 9.9512 8.65829 10.1465 8.85355C10.3417 9.04882 10.6583 9.04882 10.8536 8.85355L13.8536 5.85355C14.0488 5.65829 14.0488 5.34171 13.8536 5.14645L10.8536 2.14645C10.6583 1.95118 10.3417 1.95118 10.1465 2.14645ZM8.00003 15C9.10459 15 10 14.1046 10 13C10 11.8954 9.10459 11 8.00003 11C6.89546 11 6.00004 11.8954 6.00004 13C6.00004 14.1046 6.89546 15 8.00003 15ZM8.00003 14C7.44774 14 7.00003 13.5523 7.00003 13C7.00003 12.4477 7.44774 12 8.00003 12C8.55231 12 9.00002 12.4477 9.00002 13C9.00002 13.5523 8.55231 14 8.00003 14Z"),
			new IconSource("arrow_step_over", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M12.149 3.14636C11.9537 3.34158 11.9536 3.65816 12.1489 3.85347L15.2939 7H10.0001C7.06756 7 4.40686 8.63986 3.06365 11.0427C2.92891 11.2838 3.01508 11.5884 3.25612 11.7231C3.49716 11.8579 3.80179 11.7717 3.93653 11.5307C5.10604 9.43851 7.43601 8 10.0001 8H15.2928L12.1489 11.1454C11.9536 11.3407 11.9537 11.6573 12.149 11.8525C12.3443 12.0478 12.6609 12.0477 12.8561 11.8524L16.8396 7.86711C16.9383 7.77577 17.0001 7.6451 17.0001 7.5V7.5C17.0002 7.37199 16.9514 7.24367 16.8537 7.14598L12.8561 3.14653C12.6609 2.95122 12.3443 2.95115 12.149 3.14636ZM12.0001 15C12.0001 13.8954 11.1047 13 10.0001 13C8.89552 13 8.00009 13.8954 8.00009 15C8.00009 16.1046 8.89552 17 10.0001 17C11.1047 17 12.0001 16.1046 12.0001 15ZM10.0001 14C10.5524 14 11.0001 14.4477 11.0001 15C11.0001 15.5523 10.5524 16 10.0001 16C9.4478 16 9.00009 15.5523 9.00009 15C9.00009 14.4477 9.4478 14 10.0001 14Z"),
			new IconSource("send", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M1.17683 1.1185C1.32953 0.989145 1.54464 0.963297 1.72363 1.05279L14.7236 7.55279C14.893 7.63748 15 7.81061 15 8C15 8.18939 14.893 8.36252 14.7236 8.44721L1.72363 14.9472C1.54464 15.0367 1.32953 15.0109 1.17683 14.8815C1.02414 14.7522 0.96328 14.5442 1.02213 14.353L2.97688 8L1.02213 1.64705C0.96328 1.45578 1.02414 1.24785 1.17683 1.1185ZM3.8693 8.5L2.32155 13.5302L13.382 8L2.32155 2.46979L3.8693 7.5H9.50001C9.77615 7.5 10 7.72386 10 8C10 8.27614 9.77615 8.5 9.50001 8.5H3.8693Z"),
			new IconSource("send", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M2.18412 2.11244C2.33657 1.98818 2.54771 1.96483 2.72363 2.05279L17.7236 9.55279C17.893 9.63749 18 9.81062 18 10C18 10.1894 17.893 10.3625 17.7236 10.4472L2.72363 17.9472C2.54771 18.0352 2.33657 18.0118 2.18412 17.8876C2.03167 17.7633 1.96623 17.5612 2.0169 17.3712L3.98255 10L2.0169 2.62884C1.96623 2.4388 2.03167 2.2367 2.18412 2.11244ZM4.88416 10.5L3.26911 16.5564L16.382 10L3.26911 3.44357L4.88416 9.5H11.5C11.7762 9.5 12 9.72386 12 10C12 10.2761 11.7762 10.5 11.5 10.5H4.88416Z"),
			new IconSource("send", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.69362 11.9997L2.29933 3.2715C2.0631 2.66403 2.65544 2.08309 3.2414 2.28959L3.33375 2.32885L21.3337 11.3288C21.852 11.588 21.8844 12.2975 21.4309 12.6129L21.3337 12.6705L3.33375 21.6705C2.75077 21.962 2.11746 21.426 2.2688 20.8234L2.29933 20.7278L5.69362 11.9997L2.29933 3.2715L5.69362 11.9997ZM4.4021 4.54007L7.01109 11.2491L13.6387 11.2497C14.0184 11.2497 14.3322 11.5318 14.3818 11.8979L14.3887 11.9997C14.3887 12.3794 14.1065 12.6932 13.7404 12.7428L13.6387 12.7497L7.01109 12.7491L4.4021 19.4593L19.3213 11.9997L4.4021 4.54007Z"),
			new IconSource("send", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M2.04346 4.07607C1.76783 3.04575 2.82618 2.16572 3.78893 2.62471L25.2865 12.8736C26.2359 13.3262 26.2359 14.6776 25.2865 15.1302L3.78893 25.3791C2.82619 25.8381 1.76783 24.9581 2.04346 23.9277L4.69877 14.0019L2.04346 4.07607ZM6.05088 14.7519L3.63254 23.7919L24.1677 14.0019L3.63254 4.2119L6.05087 13.2519H17.2496C17.6638 13.2519 17.9996 13.5877 17.9996 14.0019C17.9996 14.4161 17.6638 14.7519 17.2496 14.7519H6.05088Z"),
			new IconSource("send", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M2.07832 3.96502C1.67147 2.70015 2.98855 1.5699 4.17701 2.16404L29.1714 14.6593C30.2771 15.2121 30.2771 16.7899 29.1714 17.3427L4.17701 29.838C2.98856 30.4321 1.67147 29.3019 2.07832 28.037L5.94968 16.001L2.07832 3.96502ZM7.72894 17.001L4.34639 27.5173L27.3825 16.001L4.34639 4.48472L7.72894 15.001H19.0001C19.5524 15.001 20.0001 15.4487 20.0001 16.001C20.0001 16.5533 19.5524 17.001 19.0001 17.001H7.72894Z"),
			new IconSource("broom", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M14.8536 1.14645C14.6583 0.951184 14.3417 0.951185 14.1465 1.14645L9.47596 5.81695C8.02361 4.71419 5.96894 4.88 4.65264 6.19027L3.96853 6.82878L1.29311 8.04481C1.14382 8.11267 1.03729 8.24942 1.00803 8.41078C0.978769 8.57214 1.03049 8.73759 1.14645 8.85355L7.14645 14.8536C7.26241 14.9695 7.42787 15.0212 7.58923 14.992C7.75058 14.9627 7.88734 14.8562 7.9552 14.7069L9.16755 12.0396L9.85344 11.3537C11.1784 10.0287 11.2884 7.97035 10.1835 6.52359L14.8536 1.85355C15.0488 1.65829 15.0488 1.34171 14.8536 1.14645ZM4.14134 7.84868L8.15132 11.8587L7.34337 13.6363L2.36375 8.65664L4.14134 7.84868ZM8.74989 11.043L4.96939 7.26252L5.34104 6.91565L5.35344 6.90368C6.41819 5.83893 8.10209 5.80944 9.14633 6.85368C10.2007 7.90809 10.2007 9.59215 9.14633 10.6466L8.74989 11.043Z"),
			new IconSource("broom", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M17.9099 2.1816C18.1052 2.37687 18.1052 2.69345 17.9099 2.88871L12.5872 8.21144C14.0023 9.97829 13.8909 12.5646 12.2531 14.2024L11.5895 14.866L9.49982 18.3488C9.4206 18.4808 9.28498 18.569 9.13216 18.5878C8.97935 18.6066 8.82639 18.554 8.71752 18.4451L1.64645 11.374C1.53758 11.2651 1.48494 11.1122 1.50375 10.9594C1.52256 10.8066 1.61073 10.6709 1.74276 10.5917L5.2255 8.50207L5.88909 7.83847C7.52695 6.20062 10.1132 6.08924 11.8801 7.50433L17.2028 2.1816C17.3981 1.98634 17.7146 1.98634 17.9099 2.1816ZM6.24265 8.89913L11.1924 13.8489L11.5459 13.4953C12.9128 12.1285 12.9128 9.91242 11.5459 8.54558C10.1791 7.17875 7.96303 7.17875 6.5962 8.54558L6.24265 8.89913ZM5.45803 9.52874L2.80638 11.1197L8.9718 17.2852L10.5628 14.6335L5.45803 9.52874Z"),
			new IconSource("broom", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M22.4533 1.92279C22.7462 2.21569 22.7462 2.69056 22.4533 2.98345L15.5248 9.91202C17.2598 12.1668 17.0945 15.4134 15.0287 17.4791L14.1973 18.3106L11.4101 22.4913C11.2855 22.6783 11.0836 22.7995 10.86 22.8217C10.6364 22.8438 10.4146 22.7645 10.2557 22.6056L1.77046 14.1204C1.61157 13.9615 1.53229 13.7397 1.55444 13.5161C1.57658 13.2925 1.6978 13.0906 1.88476 12.966L6.06572 10.1787L6.89699 9.34742C8.96276 7.28165 12.2093 7.11629 14.4641 8.85136L21.3927 1.92279C21.6856 1.6299 22.1604 1.6299 22.4533 1.92279ZM7.6041 10.7616L13.6145 16.772L13.9681 16.4185C15.6278 14.7588 15.6278 12.0678 13.9681 10.4081C12.3083 8.74835 9.61738 8.74835 7.95765 10.4081L7.6041 10.7616ZM6.44786 11.7267L3.47801 13.7066L10.6695 20.8981L12.6494 17.9282L6.44786 11.7267Z"),
			new IconSource("broom", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M25.7803 3.28034C26.0732 2.98745 26.0732 2.51258 25.7803 2.21968C25.4874 1.92678 25.0125 1.92677 24.7196 2.21966L16.4631 10.4758C13.8138 8.35396 9.93625 8.52013 7.47932 10.9744L6.78556 11.6547L2.3589 14.3601C2.16275 14.4799 2.03241 14.6831 2.00526 14.9114C1.9781 15.1397 2.05713 15.3678 2.21968 15.5303L12.4697 25.7803C12.6322 25.9429 12.8603 26.0219 13.0886 25.9947C13.3169 25.9676 13.5201 25.8372 13.64 25.6411L16.3455 21.214L17.0254 20.5202C19.4795 18.0633 19.6456 14.1858 17.5238 11.5365L25.7803 3.28034ZM3.95388 15.1432L7.13687 13.1979L14.8022 20.8628L12.8568 24.0461L3.95388 15.1432ZM15.9619 19.4623L15.7447 19.684L8.31552 12.2552L8.53211 12.0429L8.53731 12.0377C10.5876 9.98745 13.9117 9.98745 15.9619 12.0377C18.0122 14.088 18.0122 17.412 15.9619 19.4623Z"),
			new IconSource("broom", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M29.7071 2.29274C29.3166 1.90222 28.6834 1.90222 28.2929 2.29274L18.9519 11.6337C16.0472 9.42823 11.9379 9.75985 9.30528 12.3804L7.93705 13.6574L2.58623 16.0895C2.28764 16.2252 2.07459 16.4987 2.01606 16.8214C1.95754 17.1441 2.06099 17.475 2.29291 17.707L14.2929 29.707C14.5248 29.9389 14.8557 30.0423 15.1785 29.9838C15.5012 29.9253 15.7747 29.7122 15.9104 29.4136L18.3351 24.079L19.7069 22.7072C22.3568 20.0573 22.5768 15.9406 20.367 13.047L29.7071 3.70695C30.0976 3.31643 30.0976 2.68327 29.7071 2.29274ZM8.28268 15.6972L16.3026 23.7172L14.6867 27.2724L4.7275 17.3131L8.28268 15.6972ZM17.4998 22.0859L9.93879 14.5249L10.6821 13.8311L10.7069 13.8072C12.8364 11.6777 16.2042 11.6187 18.2927 13.7072C20.4015 15.816 20.4015 19.1842 18.2927 21.293L17.4998 22.0859Z"),
			new IconSource("note", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M4.5 2C3.11929 2 2 3.11929 2 4.5V11.5C2 12.8807 3.11929 14 4.5 14H8.17157C8.83461 14 9.4705 13.7366 9.93934 13.2678L13.2678 9.93934C13.7366 9.4705 14 8.83461 14 8.17157V4.5C14 3.11929 12.8807 2 11.5 2H4.5ZM3 4.5C3 3.67157 3.67157 3 4.5 3H11.5C12.3284 3 13 3.67157 13 4.5V8H10.5C9.11929 8 8 9.11929 8 10.5V13H4.5C3.67157 13 3 12.3284 3 11.5V4.5ZM9 12.7505V10.5C9 9.67157 9.67157 9 10.5 9H12.7505C12.6955 9.08295 12.6321 9.16082 12.5607 9.23223L9.23223 12.5607C9.16082 12.6321 9.08295 12.6955 9 12.7505Z"),
			new IconSource("note", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M14 3C15.5977 3 16.9037 4.24892 16.9949 5.82373L17 6V10.3787C17 10.8502 16.8335 11.3045 16.5331 11.6631L16.4142 11.7929L11.7929 16.4142C11.4595 16.7476 11.0205 16.9511 10.5545 16.9923L10.3787 17H6C4.40232 17 3.09634 15.7511 3.00509 14.1763L3 14V6C3 4.40232 4.24892 3.09634 5.82373 3.00509L6 3H14ZM14 4H6C4.94564 4 4.08183 4.81588 4.00549 5.85074L4 6V14C4 15.0544 4.81588 15.9182 5.85074 15.9945L6 16H10V13C10 11.4023 11.2489 10.0963 12.8237 10.0051L13 10H16V6C16 4.94564 15.1841 4.08183 14.1493 4.00549L14 4ZM15.7825 11.0013L13 11C11.9456 11 11.0818 11.8159 11.0055 12.8507L11 13V15.781L11.0858 15.7071L15.7071 11.0858C15.7339 11.059 15.7591 11.0307 15.7825 11.0013Z"),
			new IconSource("note", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M17.75 3C19.5449 3 21 4.45507 21 6.25V13.1287C21 13.7254 20.7629 14.2977 20.341 14.7197L14.7197 20.341C14.2977 20.7629 13.7254 21 13.1287 21H6.25C4.45507 21 3 19.5449 3 17.75V6.25C3 4.45507 4.45507 3 6.25 3H17.75ZM17.75 4.5H6.25C5.2835 4.5 4.5 5.2835 4.5 6.25V17.75C4.5 18.7165 5.2835 19.5 6.25 19.5H13V16.25C13 14.517 14.3565 13.1008 16.0656 13.0051L16.25 13H19.5V6.25C19.5 5.2835 18.7165 4.5 17.75 4.5ZM18.439 14.5H16.25C15.3318 14.5 14.5788 15.2071 14.5058 16.1065L14.5 16.25V18.439L18.439 14.5Z"),
			new IconSource("note", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M6.75 3C4.67893 3 3 4.67893 3 6.75V21.25C3 23.3211 4.67893 25 6.75 25H15.2574C16.2519 25 17.2057 24.6049 17.909 23.9016L23.9017 17.909C24.6049 17.2057 25 16.2519 25 15.2574V6.75C25 4.67893 23.3211 3 21.25 3H6.75ZM4.5 6.75C4.5 5.50736 5.50736 4.5 6.75 4.5H21.25C22.4926 4.5 23.5 5.50736 23.5 6.75V15H18.75C16.6789 15 15 16.6789 15 18.75V23.5H6.75C5.50736 23.5 4.5 22.4926 4.5 21.25V6.75ZM16.5 23.1257V18.75C16.5 17.5074 17.5074 16.5 18.75 16.5H23.1257C23.0433 16.6244 22.9481 16.7412 22.841 16.8483L16.8484 22.841C16.7412 22.9481 16.6244 23.0433 16.5 23.1257Z"),
			new IconSource("music_note_2", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M12 5.20951V10.2675C11.7058 10.0974 11.3643 9.99998 11 9.99998C9.89543 9.99998 9 10.8954 9 12C9 13.1045 9.89543 14 11 14C12.1046 14 13 13.1045 13 12V1.92568C13 1.37245 12.4519 0.986213 11.9309 1.17228L5.53093 3.458C5.21254 3.57171 5 3.8733 5 4.21139V11.2675C4.70583 11.0974 4.36429 11 4 11C2.89543 11 2 11.8954 2 13C2 14.1045 2.89543 15 4 15C5.10457 15 6 14.1045 6 13V7.35237L12 5.20951ZM12 4.14765L6 6.2905V4.35233L12 2.20948V4.14765ZM11 11C11.5523 11 12 11.4477 12 12C12 12.5523 11.5523 13 11 13C10.4477 13 10 12.5523 10 12C10 11.4477 10.4477 11 11 11ZM4 12C4.55228 12 5 12.4477 5 13C5 13.5523 4.55228 14 4 14C3.44772 14 3 13.5523 3 13C3 12.4477 3.44772 12 4 12Z"),
			new IconSource("music_note_2", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M14.7017 2.2259C15.3457 2.02468 16 2.50574 16 3.18038V13.5003C16 14.881 14.8807 16.0003 13.5 16.0003C12.1193 16.0003 11 14.881 11 13.5003C11 12.1196 12.1193 11.0003 13.5 11.0003C14.0628 11.0003 14.5822 11.1863 15 11.5001V6.18038L8 8.36789V15.5003C8 16.881 6.88071 18.0003 5.5 18.0003C4.11929 18.0003 3 16.881 3 15.5003C3 14.1196 4.11929 13.0003 5.5 13.0003C6.0628 13.0003 6.58217 13.1863 7 13.5001V5.36788C7 4.9305 7.28425 4.54387 7.70173 4.4134L14.7017 2.2259ZM8 7.32019L15 5.13269V3.18038L8 5.36788V7.32019ZM5.5 14.0003C4.67157 14.0003 4 14.6719 4 15.5003C4 16.3287 4.67157 17.0003 5.5 17.0003C6.32843 17.0003 7 16.3287 7 15.5003C7 14.6719 6.32843 14.0003 5.5 14.0003ZM12 13.5003C12 14.3287 12.6716 15.0003 13.5 15.0003C14.3284 15.0003 15 14.3287 15 13.5003C15 12.6719 14.3284 12.0003 13.5 12.0003C12.6716 12.0003 12 12.6719 12 13.5003Z"),
			new IconSource("music_note_2", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M19.6977 2.14829C19.8879 2.28981 20 2.51293 20 2.75001V16.25C20 16.2767 19.9986 16.303 19.9959 16.329C19.9986 16.3856 20 16.4427 20 16.5C20 18.433 18.433 20 16.5 20C14.567 20 13 18.433 13 16.5C13 14.567 14.567 13 16.5 13C17.2436 13 17.9331 13.2319 18.5 13.6273V7.75803L10 10.308V18.25C10 18.2766 9.99861 18.303 9.99589 18.3289C9.99862 18.3856 10 18.4426 10 18.5C10 20.433 8.433 22 6.5 22C4.567 22 3 20.433 3 18.5C3 16.567 4.567 15 6.5 15C7.24362 15 7.93308 15.2319 8.5 15.6273V5.75001C8.5 5.4188 8.71725 5.12681 9.03449 5.03164L19.0345 2.03164C19.2616 1.96351 19.5075 2.00677 19.6977 2.14829ZM10 8.74198L18.5 6.19198V3.75803L10 6.30803V8.74198ZM6.5 16.5C5.39543 16.5 4.5 17.3954 4.5 18.5C4.5 19.6046 5.39543 20.5 6.5 20.5C7.60457 20.5 8.5 19.6046 8.5 18.5C8.5 17.3954 7.60457 16.5 6.5 16.5ZM14.5 16.5C14.5 17.6046 15.3954 18.5 16.5 18.5C17.6046 18.5 18.5 17.6046 18.5 16.5C18.5 15.3954 17.6046 14.5 16.5 14.5C15.3954 14.5 14.5 15.3954 14.5 16.5Z"),
			new IconSource("slide_text", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M4.5 5C4.22386 5 4 5.22386 4 5.5C4 5.77614 4.22386 6 4.5 6H7.5C7.77614 6 8 5.77614 8 5.5C8 5.22386 7.77614 5 7.5 5H4.5ZM4 7.5C4 7.22386 4.22386 7 4.5 7H10.5C10.7761 7 11 7.22386 11 7.5C11 7.77614 10.7761 8 10.5 8H4.5C4.22386 8 4 7.77614 4 7.5ZM4.5 9C4.22386 9 4 9.22386 4 9.5C4 9.77614 4.22386 10 4.5 10H8.5C8.77614 10 9 9.77614 9 9.5C9 9.22386 8.77614 9 8.5 9H4.5ZM3 3C1.89543 3 1 3.89543 1 5V11C1 12.1046 1.89543 13 3 13H13C14.1046 13 15 12.1046 15 11V5C15 3.89543 14.1046 3 13 3H3ZM2 5C2 4.44772 2.44772 4 3 4H13C13.5523 4 14 4.44772 14 5V11C14 11.5523 13.5523 12 13 12H3C2.44772 12 2 11.5523 2 11V5Z"),
			new IconSource("slide_text", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M5.5 7C5.22386 7 5 7.22386 5 7.5C5 7.77614 5.22386 8 5.5 8H9.5C9.77614 8 10 7.77614 10 7.5C10 7.22386 9.77614 7 9.5 7H5.5ZM5.5 9.5C5.22386 9.5 5 9.72386 5 10C5 10.2761 5.22386 10.5 5.5 10.5H12.5C12.7761 10.5 13 10.2761 13 10C13 9.72386 12.7761 9.5 12.5 9.5H5.5ZM5 12.5C5 12.2239 5.22386 12 5.5 12H10.5C10.7761 12 11 12.2239 11 12.5C11 12.7761 10.7761 13 10.5 13H5.5C5.22386 13 5 12.7761 5 12.5ZM4.5 4C3.11929 4 2 5.11929 2 6.5V13.5C2 14.8807 3.11929 16 4.5 16H15.5C16.8807 16 18 14.8807 18 13.5V6.5C18 5.11929 16.8807 4 15.5 4H4.5ZM3 6.5C3 5.67157 3.67157 5 4.5 5H15.5C16.3284 5 17 5.67157 17 6.5V13.5C17 14.3284 16.3284 15 15.5 15H4.5C3.67157 15 3 14.3284 3 13.5V6.5Z"),
			new IconSource("slide_text", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M6.75 8C6.33579 8 6 8.33579 6 8.75C6 9.16421 6.33579 9.5 6.75 9.5H11.25C11.6642 9.5 12 9.16421 12 8.75C12 8.33579 11.6642 8 11.25 8H6.75ZM6 11.75C6 11.3358 6.33579 11 6.75 11H15.25C15.6642 11 16 11.3358 16 11.75C16 12.1642 15.6642 12.5 15.25 12.5H6.75C6.33579 12.5 6 12.1642 6 11.75ZM6.75 14C6.33579 14 6 14.3358 6 14.75C6 15.1642 6.33579 15.5 6.75 15.5H13.25C13.6642 15.5 14 15.1642 14 14.75C14 14.3358 13.6642 14 13.25 14H6.75ZM4.75 4C3.23122 4 2 5.23122 2 6.75V17.25C2 18.7688 3.23122 20 4.75 20H19.25C20.7688 20 22 18.7688 22 17.25V6.75C22 5.23122 20.7688 4 19.25 4H4.75ZM3.5 6.75C3.5 6.05964 4.05964 5.5 4.75 5.5H19.25C19.9404 5.5 20.5 6.05964 20.5 6.75V17.25C20.5 17.9404 19.9404 18.5 19.25 18.5H4.75C4.05964 18.5 3.5 17.9404 3.5 17.25V6.75Z"),
			new IconSource("slide_text", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M7.7277 9C7.32567 9 6.99976 9.32591 6.99976 9.72794V9.77206C6.99976 10.1741 7.32567 10.5 7.7277 10.5H13.2718C13.6738 10.5 13.9998 10.1741 13.9998 9.77206V9.72794C13.9998 9.32591 13.6738 9 13.2718 9H7.7277ZM7.7277 13C7.32567 13 6.99976 13.3259 6.99976 13.7279V13.7721C6.99976 14.1741 7.32567 14.5 7.7277 14.5H18.2718C18.6738 14.5 18.9998 14.1741 18.9998 13.7721V13.7279C18.9998 13.3259 18.6738 13 18.2718 13H7.7277ZM6.99976 17.7279C6.99976 17.3259 7.32567 17 7.7277 17H15.2718C15.6738 17 15.9998 17.3259 15.9998 17.7279V17.7721C15.9998 18.1741 15.6738 18.5 15.2718 18.5H7.7277C7.32567 18.5 6.99976 18.1741 6.99976 17.7721V17.7279ZM5.75391 4C3.68284 4 2.00391 5.67893 2.00391 7.75V20.2501C2.00391 22.3211 3.68284 24.0001 5.75391 24.0001H22.2505C24.3216 24.0001 26.0005 22.3211 26.0005 20.2501V7.75C26.0005 5.67893 24.3216 4 22.2505 4H5.75391ZM3.50391 7.75C3.50391 6.50736 4.51127 5.5 5.75391 5.5H22.2505C23.4932 5.5 24.5005 6.50736 24.5005 7.75V20.2501C24.5005 21.4927 23.4932 22.5001 22.2505 22.5001H5.75391C4.51127 22.5001 3.50391 21.4927 3.50391 20.2501V7.75Z"),
			new IconSource("slide_text", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M25.5 4C27.9853 4 30 6.01472 30 8.5V23.5C30 25.9853 27.9853 28 25.5 28H6.5C4.01472 28 2 25.9853 2 23.5V8.5C2 6.01472 4.01472 4 6.5 4H25.5ZM6.5 6C5.11929 6 4 7.11929 4 8.5V23.5C4 24.8807 5.11929 26 6.5 26H25.5C26.8807 26 28 24.8807 28 23.5V8.5C28 7.11929 26.8807 6 25.5 6H6.5ZM19 20C19.5523 20 20 20.4477 20 21C20 21.5523 19.5523 22 19 22H9C8.44772 22 8 21.5523 8 21C8 20.4477 8.44772 20 9 20H19ZM22 15C22.5523 15 23 15.4477 23 16C23 16.5523 22.5523 17 22 17H9C8.44772 17 8 16.5523 8 16C8 15.4477 8.44772 15 9 15H22ZM16 10C16.5523 10 17 10.4477 17 11C17 11.5523 16.5523 12 16 12H9C8.44772 12 8 11.5523 8 11C8 10.4477 8.44772 10 9 10H16Z"),
			new IconSource("book_open", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M2.5 2C1.67157 2 1 2.67157 1 3.5V12.5C1 13.3284 1.67157 14 2.5 14H6C6.8178 14 7.54389 13.6073 8 13.0002C8.45612 13.6073 9.1822 14 10 14H13.5C14.3284 14 15 13.3284 15 12.5V3.5C15 2.67157 14.3284 2 13.5 2H10C9.1822 2 8.45612 2.39267 8 2.99976C7.54389 2.39267 6.8178 2 6 2H2.5ZM7.5 4.5V11.5C7.5 12.3284 6.82843 13 6 13H2.5C2.22386 13 2 12.7761 2 12.5V3.5C2 3.22386 2.22386 3 2.5 3H6C6.82843 3 7.5 3.67157 7.5 4.5ZM8.5 11.5V4.5C8.5 3.67157 9.17157 3 10 3H13.5C13.7761 3 14 3.22386 14 3.5V12.5C14 12.7761 13.7761 13 13.5 13H10C9.17157 13 8.5 12.3284 8.5 11.5Z"),
			new IconSource("book_open", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M10 16.0002C9.54389 16.6073 8.8178 17 8 17H3.5C2.67157 17 2 16.3284 2 15.5V4.5C2 3.67157 2.67157 3 3.5 3H8C8.8178 3 9.54389 3.39267 10 3.99976C10.4561 3.39267 11.1822 3 12 3H16.5C17.3284 3 18 3.67157 18 4.5L18 15.5C18 16.3284 17.3284 17 16.5 17L12 17C11.1822 17 10.4561 16.6073 10 16.0002ZM3 4.5V15.5C3 15.7761 3.22386 16 3.5 16H8C8.82843 16 9.5 15.3284 9.5 14.5V5.5C9.5 4.67157 8.82843 4 8 4H3.5C3.22386 4 3 4.22386 3 4.5ZM10.5 14.5C10.5 15.3284 11.1716 16 12 16L16.5 16C16.7761 16 17 15.7761 17 15.5L17 4.5C17 4.22386 16.7761 4 16.5 4L12 4C11.1716 4 10.5 4.67157 10.5 5.5V14.5Z"),
			new IconSource("book_open", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12 19.1375C11.4986 19.6686 10.788 20 10 20H3.75C2.7835 20 2 19.2165 2 18.25V5.75C2 4.7835 2.7835 4 3.75 4H10C10.788 4 11.4986 4.33145 12 4.86253C12.5014 4.33145 13.212 4 14 4H20.25C21.2165 4 22 4.7835 22 5.75V18.25C22 19.2165 21.2165 20 20.25 20H14C13.212 20 12.5014 19.6686 12 19.1375ZM3.5 5.75V18.25C3.5 18.3881 3.61193 18.5 3.75 18.5H10C10.6904 18.5 11.25 17.9404 11.25 17.25V6.75C11.25 6.05964 10.6904 5.5 10 5.5H3.75C3.61193 5.5 3.5 5.61193 3.5 5.75ZM12.75 17.25C12.75 17.9404 13.3096 18.5 14 18.5H20.25C20.3881 18.5 20.5 18.3881 20.5 18.25V5.75C20.5 5.61193 20.3881 5.5 20.25 5.5H14C13.3096 5.5 12.75 6.05964 12.75 6.75V17.25Z"),
			new IconSource("book_open", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M11.5 4C12.5052 4 13.4038 4.45637 14 5.17321C14.5962 4.45637 15.4948 4 16.5 4H24.25C25.2165 4 26 4.7835 26 5.75V22.25C26 23.2165 25.2165 24 24.25 24H16.5C15.4948 24 14.5962 23.5436 14 22.8268C13.4038 23.5436 12.5052 24 11.5 24H3.75C2.7835 24 2 23.2165 2 22.25V5.75C2 4.7835 2.7835 4 3.75 4H11.5ZM13.25 20.75V7.25C13.25 6.2835 12.4665 5.5 11.5 5.5H3.75C3.61193 5.5 3.5 5.61193 3.5 5.75V22.25C3.5 22.3881 3.61193 22.5 3.75 22.5H11.5C12.4665 22.5 13.25 21.7165 13.25 20.75ZM14.75 7.25V20.75C14.75 21.7165 15.5335 22.5 16.5 22.5H24.25C24.3881 22.5 24.5 22.3881 24.5 22.25V5.75C24.5 5.61193 24.3881 5.5 24.25 5.5H16.5C15.5335 5.5 14.75 6.2835 14.75 7.25Z"),
			new IconSource("book_open", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M16 26.3287C15.175 27.3482 13.9136 28 12.5 28H4C2.89543 28 2 27.1046 2 26V6C2 4.89543 2.89543 4 4 4H12.5C13.9136 4 15.175 4.65183 16 5.67134C16.825 4.65183 18.0864 4 19.5 4H28C29.1046 4 30 4.89543 30 6V26C30 27.1046 29.1046 28 28 28H19.5C18.0864 28 16.825 27.3482 16 26.3287ZM17 8.5V23.5C17 24.8807 18.1193 26 19.5 26H28V6L19.5 6C18.1193 6 17 7.11929 17 8.5ZM15 8.5C15 7.11929 13.8807 6 12.5 6H4V26H12.5C13.8807 26 15 24.8807 15 23.5V8.5Z"),
			new IconSource("text_t", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3 2.5C3 2.22386 3.22386 2 3.5 2H11.5C11.7761 2 12 2.22386 12 2.5V4.5C12 4.77614 11.7761 5 11.5 5C11.2238 5 11 4.77614 11 4.5V3H7.99997L7.99998 13H9C9.27614 13 9.5 13.2239 9.5 13.5C9.5 13.7761 9.27614 14 9 14H6C5.72386 14 5.5 13.7761 5.5 13.5C5.5 13.2239 5.72386 13 6 13H6.99998L6.99997 3H4V4.5C4 4.77614 3.77614 5 3.5 5C3.22386 5 3 4.77614 3 4.5V2.5Z"),
			new IconSource("text_t", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4 3.5C4 3.22386 4.22386 3 4.5 3H14.5C14.7761 3 15 3.22386 15 3.5V5.5C15 5.77614 14.7761 6 14.5 6C14.2239 6 14 5.77614 14 5.5V4H10V16H11.5C11.7761 16 12 16.2239 12 16.5C12 16.7761 11.7761 17 11.5 17H7.5C7.22386 17 7 16.7761 7 16.5C7 16.2239 7.22386 16 7.5 16H9V4H5V5.5C5 5.77614 4.77614 6 4.5 6C4.22386 6 4 5.77614 4 5.5V3.5Z"),
			new IconSource("text_t", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5 4.75C5 4.33579 5.33579 4 5.75 4H18.25C18.6642 4 19 4.33579 19 4.75V6.75C19 7.16421 18.6642 7.5 18.25 7.5C17.8358 7.5 17.5 7.16421 17.5 6.75V5.5H12.75L12.75 18.5H14.25C14.6642 18.5 15 18.8358 15 19.25C15 19.6642 14.6642 20 14.25 20H9.75C9.33579 20 9 19.6642 9 19.25C9 18.8358 9.33579 18.5 9.75 18.5H11.25L11.25 5.5H6.5V6.75C6.5 7.16421 6.16421 7.5 5.75 7.5C5.33579 7.5 5 7.16421 5 6.75V4.75Z"),
			new IconSource("text_t", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M5 4.75C5 4.33579 5.33579 4 5.75 4H22.25C22.6642 4 23 4.33579 23 4.75V8.25C23 8.66421 22.6642 9 22.25 9C21.8358 9 21.5 8.66421 21.5 8.25V5.5H14.75V22.5H16.25C16.6642 22.5 17 22.8358 17 23.25C17 23.6642 16.6642 24 16.25 24H11.75C11.3358 24 11 23.6642 11 23.25C11 22.8358 11.3358 22.5 11.75 22.5H13.25V5.5H6.5V8.25C6.5 8.66421 6.16421 9 5.75 9C5.33579 9 5 8.66421 5 8.25V4.75Z"),
			new IconSource("text_t", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M6 5C6 4.44772 6.44772 4 7 4H25C25.5523 4 26 4.44772 26 5V9C26 9.55228 25.5523 10 25 10C24.4477 10 24 9.55228 24 9V6H17V26H19C19.5523 26 20 26.4477 20 27C20 27.5523 19.5523 28 19 28H13C12.4477 28 12 27.5523 12 27C12 26.4477 12.4477 26 13 26H15V6H8V9C8 9.55228 7.55228 10 7 10C6.44772 10 6 9.55228 6 9V5Z"),
			new IconSource("info", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M8.49902 7.49998C8.49902 7.22384 8.27517 6.99998 7.99902 6.99998C7.72288 6.99998 7.49902 7.22384 7.49902 7.49998V10.5C7.49902 10.7761 7.72288 11 7.99902 11C8.27517 11 8.49902 10.7761 8.49902 10.5V7.49998ZM8.74807 5.50001C8.74807 5.91369 8.41271 6.24905 7.99903 6.24905C7.58535 6.24905 7.25 5.91369 7.25 5.50001C7.25 5.08633 7.58535 4.75098 7.99903 4.75098C8.41271 4.75098 8.74807 5.08633 8.74807 5.50001ZM8 1C4.13401 1 1 4.13401 1 8C1 11.866 4.13401 15 8 15C11.866 15 15 11.866 15 8C15 4.13401 11.866 1 8 1ZM2 8C2 4.68629 4.68629 2 8 2C11.3137 2 14 4.68629 14 8C14 11.3137 11.3137 14 8 14C4.68629 14 2 11.3137 2 8Z"),
			new IconSource("info", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M10.4921 8.91012C10.4497 8.67687 10.2456 8.49999 10.0001 8.49999C9.72397 8.49999 9.50011 8.72385 9.50011 8.99999V13.5021L9.50817 13.592C9.55051 13.8253 9.75465 14.0021 10.0001 14.0021C10.2763 14.0021 10.5001 13.7783 10.5001 13.5021V8.99999L10.4921 8.91012ZM10.7988 6.74999C10.7988 6.33578 10.463 5.99999 10.0488 5.99999C9.63461 5.99999 9.29883 6.33578 9.29883 6.74999C9.29883 7.16421 9.63461 7.49999 10.0488 7.49999C10.463 7.49999 10.7988 7.16421 10.7988 6.74999ZM18 10C18 5.58172 14.4183 2 10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18C14.4183 18 18 14.4183 18 10ZM3 10C3 6.13401 6.13401 3 10 3C13.866 3 17 6.13401 17 10C17 13.866 13.866 17 10 17C6.13401 17 3 13.866 3 10Z"),
			new IconSource("info", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12.0016 1.99902C17.5253 1.99902 22.0031 6.47687 22.0031 12.0006C22.0031 17.5243 17.5253 22.0021 12.0016 22.0021C6.47785 22.0021 2 17.5243 2 12.0006C2 6.47687 6.47785 1.99902 12.0016 1.99902ZM12.0016 3.49902C7.30627 3.49902 3.5 7.3053 3.5 12.0006C3.5 16.6959 7.30627 20.5021 12.0016 20.5021C16.6968 20.5021 20.5031 16.6959 20.5031 12.0006C20.5031 7.3053 16.6968 3.49902 12.0016 3.49902ZM12 10.5C12.4142 10.5 12.75 10.8358 12.75 11.25V16.25C12.75 16.6642 12.4142 17 12 17C11.5858 17 11.25 16.6642 11.25 16.25V11.25C11.25 10.8358 11.5858 10.5 12 10.5ZM12 9C12.5523 9 13 8.55229 13 8C13 7.44772 12.5523 7 12 7C11.4477 7 11 7.44772 11 8C11 8.55229 11.4477 9 12 9Z"),
			new IconSource("info", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M2 14C2 7.37258 7.37258 2 14 2C20.6274 2 26 7.37258 26 14C26 20.6274 20.6274 26 14 26C7.37258 26 2 20.6274 2 14ZM14 3.5C8.20101 3.5 3.5 8.20101 3.5 14C3.5 19.799 8.20101 24.5 14 24.5C19.799 24.5 24.5 19.799 24.5 14C24.5 8.20101 19.799 3.5 14 3.5ZM14 11.5C14.4142 11.5 14.75 11.8358 14.75 12.25V19.25C14.75 19.6642 14.4142 20 14 20C13.5858 20 13.25 19.6642 13.25 19.25V12.25C13.25 11.8358 13.5858 11.5 14 11.5ZM14 10C14.5523 10 15 9.55229 15 9C15 8.44771 14.5523 8 14 8C13.4477 8 13 8.44771 13 9C13 9.55229 13.4477 10 14 10Z"),
			new IconSource("info", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M16 13C16.5523 13 17 13.4477 17 14V23C17 23.5523 16.5523 24 16 24C15.4477 24 15 23.5523 15 23V14C15 13.4477 15.4477 13 16 13ZM16 11C16.8284 11 17.5 10.3284 17.5 9.5C17.5 8.67157 16.8284 8 16 8C15.1716 8 14.5 8.67157 14.5 9.5C14.5 10.3284 15.1716 11 16 11ZM2 16C2 8.26801 8.26801 2 16 2C23.732 2 30 8.26801 30 16C30 23.732 23.732 30 16 30C8.26801 30 2 23.732 2 16ZM16 4C9.37258 4 4 9.37258 4 16C4 22.6274 9.37258 28 16 28C22.6274 28 28 22.6274 28 16C28 9.37258 22.6274 4 16 4Z"),
			new IconSource("video", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3.5 3C2.11929 3 1 4.11929 1 5.5V10.5C1 11.8807 2.11929 13 3.5 13H8.5C9.88071 13 11 11.8807 11 10.5V10.3728L13.0354 11.7782C13.8646 12.3507 14.9957 11.7572 14.9957 10.7496V5.25216C14.9957 4.24453 13.8646 3.65101 13.0354 4.22353L11 5.62896V5.5C11 4.11929 9.88071 3 8.5 3H3.5ZM11 6.84418L13.6036 5.04643C13.7695 4.93193 13.9957 5.05063 13.9957 5.25216V10.7496C13.9957 10.9511 13.7695 11.0698 13.6036 10.9553L11 9.15757V6.84418ZM2 5.5C2 4.67157 2.67157 4 3.5 4H8.5C9.32843 4 10 4.67157 10 5.5V10.5C10 11.3284 9.32843 12 8.5 12H3.5C2.67157 12 2 11.3284 2 10.5V5.5Z"),
			new IconSource("video", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M5 4C3.34315 4 2 5.34315 2 7V13C2 14.6569 3.34315 16 5 16H10C11.6569 16 13 14.6569 13 13V12.6787L16.0372 14.7759C16.8664 15.3484 17.9975 14.7549 17.9975 13.7473V6.25215C17.9975 5.24453 16.8664 4.65101 16.0372 5.22353L13 7.32067V7C13 5.34315 11.6569 4 10 4H5ZM13 8.53588L16.6054 6.04643C16.7712 5.93193 16.9975 6.05063 16.9975 6.25215V13.7473C16.9975 13.9488 16.7712 14.0675 16.6054 13.953L13 11.4635V8.53588ZM3 7C3 5.89543 3.89543 5 5 5H10C11.1046 5 12 5.89543 12 7V13C12 14.1046 11.1046 15 10 15H5C3.89543 15 3 14.1046 3 13V7Z"),
			new IconSource("video", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.25 5C3.45507 5 2 6.45507 2 8.25V15.75C2 17.5449 3.45507 19 5.25 19H12.75C14.5449 19 16 17.5449 16 15.75V15.438L19.2575 17.6872C20.4183 18.4887 22.0018 17.6578 22.0018 16.2471V7.75204C22.0018 6.34137 20.4183 5.51044 19.2575 6.31197L16 8.56118V8.25C16 6.45507 14.5449 5 12.75 5H5.25ZM16 10.384L20.1098 7.54632C20.2756 7.43181 20.5018 7.55051 20.5018 7.75204V16.2471C20.5018 16.4486 20.2756 16.5673 20.1098 16.4528L16 13.6151V10.384ZM3.5 8.25C3.5 7.2835 4.2835 6.5 5.25 6.5H12.75C13.7165 6.5 14.5 7.2835 14.5 8.25V15.75C14.5 16.7165 13.7165 17.5 12.75 17.5H5.25C4.2835 17.5 3.5 16.7165 3.5 15.75V8.25Z"),
			new IconSource("video", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M5.75 6C3.67893 6 2 7.67893 2 9.75V18.25C2 20.3211 3.67893 22 5.75 22H15.25C17.3211 22 19 20.3211 19 18.25V17.7473L23.2516 20.6829C24.4124 21.4844 25.9959 20.6535 25.9959 19.2428V8.75302C25.9959 7.34235 24.4124 6.51142 23.2516 7.31295L19 10.2486V9.75C19 7.67893 17.3211 6 15.25 6H5.75ZM19 12.0714L24.1039 8.54729C24.2697 8.43279 24.4959 8.55149 24.4959 8.75302V19.2428C24.4959 19.4443 24.2697 19.563 24.1039 19.4485L19 15.9244V12.0714ZM3.5 9.75C3.5 8.50736 4.50736 7.5 5.75 7.5H15.25C16.4926 7.5 17.5 8.50736 17.5 9.75V18.25C17.5 19.4926 16.4926 20.5 15.25 20.5H5.75C4.50736 20.5 3.5 19.4926 3.5 18.25V9.75Z"),
			new IconSource("video", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M6.5 6C4.01472 6 2 8.01472 2 10.5V21.5C2 23.9853 4.01472 26 6.5 26H17.5C19.9853 26 22 23.9853 22 21.5V21.0127L26.4675 24.0974C27.96 25.1279 29.9959 24.0596 29.9959 22.2459V9.75388C29.9959 7.94016 27.96 6.87182 26.4675 7.90236L22 10.987V10.5C22 8.01472 19.9853 6 17.5 6H6.5ZM22 13.4175L27.6038 9.54815C27.7697 9.43365 27.9959 9.55236 27.9959 9.75388V22.2459C27.9959 22.4474 27.7697 22.5661 27.6038 22.4516L22 18.5823V13.4175ZM4 10.5C4 9.11929 5.11929 8 6.5 8H17.5C18.8807 8 20 9.11929 20 10.5V21.5C20 22.8807 18.8807 24 17.5 24H6.5C5.11929 24 4 22.8807 4 21.5V10.5Z"),
			new IconSource("tab_desktop_multiple", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M2 4.5C2 3.11929 3.11929 2 4.5 2H9.5C10.8807 2 12 3.11929 12 4.5V9.5C12 10.8807 10.8807 12 9.5 12H4.5C3.11929 12 2 10.8807 2 9.5V4.5ZM4.5 3C3.67157 3 3 3.67157 3 4.5V9.5C3 10.3284 3.67157 11 4.5 11H9.5C10.3284 11 11 10.3284 11 9.5V5H6.5C5.67157 5 5 4.32843 5 3.5V3H4.5ZM6 3V3.5C6 3.77614 6.22386 4 6.5 4H10.9146C10.7087 3.4174 10.1531 3 9.5 3H6ZM6.49867 14.0008C5.68049 14.0008 4.95412 13.6077 4.49805 13.0002C6.33159 13.0002 8.16512 13.0008 9.99867 13.0008C11.6555 13.0008 12.9987 11.6576 12.9987 10.0008V9.61383C12.9999 9.57609 13.0005 9.53821 13.0005 9.50019V4.50195C13.6067 4.95814 13.9987 5.68368 13.9987 6.50077V10.0008C13.9987 12.2099 12.2078 14.0008 9.99867 14.0008H6.49867Z"),
			new IconSource("tab_desktop_multiple", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M4.5 2C3.11929 2 2 3.11929 2 4.5V12.5C2 13.8807 3.11929 15 4.5 15H12.5C13.8807 15 15 13.8807 15 12.5V4.5C15 3.11929 13.8807 2 12.5 2H4.5ZM14 4.5V5H7.5C7.22386 5 7 4.77614 7 4.5V3H12.5C13.3284 3 14 3.67157 14 4.5ZM6 4.5C6 5.32843 6.67157 6 7.5 6H14V12.5C14 13.3284 13.3284 14 12.5 14H4.5C3.67157 14 3 13.3284 3 12.5V4.5C3 3.67157 3.67157 3 4.5 3H6V4.5ZM7.4998 18C6.29032 18 5.28144 17.1411 5.0498 16H6.08515C6.29107 16.5826 6.84669 17 7.4998 17H13.9998C15.6566 17 16.9998 15.6569 16.9998 14V7.5C16.9998 6.84689 16.5824 6.29127 15.9998 6.08535V5.05001C17.1409 5.28165 17.9998 6.29052 17.9998 7.5V14C17.9998 16.2091 16.2089 18 13.9998 18H7.4998Z"),
			new IconSource("tab_desktop_multiple", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M9.24834 22C7.53753 22 6.13547 20.6781 6.00781 19H7.51606C7.63737 19.8481 8.36672 20.5 9.24834 20.5H17.2483C19.0433 20.5 20.4983 19.0449 20.4983 17.25V9.25C20.4983 8.36838 19.8464 7.63903 18.9983 7.51772V6.00947C20.6765 6.13713 21.9983 7.53919 21.9983 9.25V17.25C21.9983 19.8734 19.8717 22 17.2483 22H9.24834ZM2 5.25C2 3.45507 3.45507 2 5.25 2H14.75C16.5449 2 18 3.45507 18 5.25V14.75C18 16.5449 16.5449 18 14.75 18H5.25C3.45507 18 2 16.5449 2 14.75V5.25ZM5.25 3.5C4.2835 3.5 3.5 4.2835 3.5 5.25V14.75C3.5 15.7165 4.2835 16.5 5.25 16.5H14.75C15.7165 16.5 16.5 15.7165 16.5 14.75V7.5H10.75C9.50736 7.5 8.5 6.49264 8.5 5.25V3.5H5.25ZM10 3.5V5.25C10 5.66421 10.3358 6 10.75 6H16.5V5.25C16.5 4.2835 15.7165 3.5 14.75 3.5H10Z"),
			new IconSource("keyboard", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M3 10.5C3 10.2239 3.22386 10 3.5 10H12.5C12.7761 10 13 10.2239 13 10.5C13 10.7761 12.7761 11 12.5 11H3.5C3.22386 11 3 10.7761 3 10.5ZM3.25 7C3.66421 7 4 6.66421 4 6.25C4 5.83579 3.66421 5.5 3.25 5.5C2.83579 5.5 2.5 5.83579 2.5 6.25C2.5 6.66421 2.83579 7 3.25 7ZM10 6.25C10 6.66421 9.66421 7 9.25 7C8.83579 7 8.5 6.66421 8.5 6.25C8.5 5.83579 8.83579 5.5 9.25 5.5C9.66421 5.5 10 5.83579 10 6.25ZM6.25 7C6.66421 7 7 6.66421 7 6.25C7 5.83579 6.66421 5.5 6.25 5.5C5.83579 5.5 5.5 5.83579 5.5 6.25C5.5 6.66421 5.83579 7 6.25 7ZM13 6.25C13 6.66421 12.6642 7 12.25 7C11.8358 7 11.5 6.66421 11.5 6.25C11.5 5.83579 11.8358 5.5 12.25 5.5C12.6642 5.5 13 5.83579 13 6.25ZM5.25 9C5.66421 9 6 8.66421 6 8.25C6 7.83579 5.66421 7.5 5.25 7.5C4.83579 7.5 4.5 7.83579 4.5 8.25C4.5 8.66421 4.83579 9 5.25 9ZM9 8.25C9 8.66421 8.66421 9 8.25 9C7.83579 9 7.5 8.66421 7.5 8.25C7.5 7.83579 7.83579 7.5 8.25 7.5C8.66421 7.5 9 7.83579 9 8.25ZM11.25 9C11.6642 9 12 8.66421 12 8.25C12 7.83579 11.6642 7.5 11.25 7.5C10.8358 7.5 10.5 7.83579 10.5 8.25C10.5 8.66421 10.8358 9 11.25 9ZM1 4.75C1 3.7835 1.7835 3 2.75 3H13.25C14.2165 3 15 3.7835 15 4.75V11.25C15 12.2165 14.2165 13 13.25 13H2.75C1.7835 13 1 12.2165 1 11.25V4.75ZM2.75 4C2.33579 4 2 4.33579 2 4.75V11.25C2 11.6642 2.33579 12 2.75 12H13.25C13.6642 12 14 11.6642 14 11.25V4.75C14 4.33579 13.6642 4 13.25 4H2.75Z"),
			new IconSource("keyboard", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M5 12.5C5 12.2239 5.22386 12 5.5 12H14.5C14.7761 12 15 12.2239 15 12.5C15 12.7761 14.7761 13 14.5 13H5.5C5.22386 13 5 12.7761 5 12.5ZM11.5024 8.00468C11.9179 8.00468 12.2547 7.66783 12.2547 7.25231C12.2547 6.83679 11.9179 6.49994 11.5024 6.49994C11.0868 6.49994 10.75 6.83679 10.75 7.25231C10.75 7.66783 11.0868 8.00468 11.5024 8.00468ZM15.2547 7.25231C15.2547 7.66783 14.9179 8.00468 14.5024 8.00468C14.0868 8.00468 13.75 7.66783 13.75 7.25231C13.75 6.83679 14.0868 6.49994 14.5024 6.49994C14.9179 6.49994 15.2547 6.83679 15.2547 7.25231ZM5.50237 8.00468C5.91789 8.00468 6.25474 7.66783 6.25474 7.25231C6.25474 6.83679 5.91789 6.49994 5.50237 6.49994C5.08685 6.49994 4.75 6.83679 4.75 7.25231C4.75 7.66783 5.08685 8.00468 5.50237 8.00468ZM7.74998 9.75231C7.74998 10.1678 7.41313 10.5047 6.99761 10.5047C6.58209 10.5047 6.24524 10.1678 6.24524 9.75231C6.24524 9.33679 6.58209 8.99994 6.99761 8.99994C7.41313 8.99994 7.74998 9.33679 7.74998 9.75231ZM10.0024 10.5047C10.4179 10.5047 10.7547 10.1678 10.7547 9.75231C10.7547 9.33679 10.4179 8.99994 10.0024 8.99994C9.58685 8.99994 9.25 9.33679 9.25 9.75231C9.25 10.1678 9.58685 10.5047 10.0024 10.5047ZM13.7595 9.75231C13.7595 10.1678 13.4227 10.5047 13.0071 10.5047C12.5916 10.5047 12.2548 10.1678 12.2548 9.75231C12.2548 9.33679 12.5916 8.99994 13.0071 8.99994C13.4227 8.99994 13.7595 9.33679 13.7595 9.75231ZM8.50237 8.00468C8.91789 8.00468 9.25474 7.66783 9.25474 7.25231C9.25474 6.83679 8.91789 6.49994 8.50237 6.49994C8.08685 6.49994 7.75 6.83679 7.75 7.25231C7.75 7.66783 8.08685 8.00468 8.50237 8.00468ZM2 5.5C2 4.67157 2.67157 4 3.5 4H16.5C17.3284 4 18 4.67157 18 5.5V13.5C18 14.3284 17.3284 15 16.5 15H3.5C2.67157 15 2 14.3284 2 13.5V5.5ZM3.5 5C3.22386 5 3 5.22386 3 5.5V13.5C3 13.7761 3.22386 14 3.5 14H16.5C16.7761 14 17 13.7761 17 13.5V5.5C17 5.22386 16.7761 5 16.5 5H3.5Z"),
			new IconSource("keyboard", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M19.7454 5C20.988 5 21.9954 6.00736 21.9954 7.25V16.7546C21.9954 17.9972 20.988 19.0046 19.7454 19.0046H4.25C3.00736 19.0046 2 17.9972 2 16.7546V7.25C2 6.00736 3.00736 5 4.25 5H19.7454ZM19.7454 6.5H4.25C3.83579 6.5 3.5 6.83579 3.5 7.25V16.7546C3.5 17.1688 3.83579 17.5046 4.25 17.5046H19.7454C20.1596 17.5046 20.4954 17.1688 20.4954 16.7546V7.25C20.4954 6.83579 20.1596 6.5 19.7454 6.5ZM6.75 14.5H17.25C17.6642 14.5 18 14.8358 18 15.25C18 15.6297 17.7178 15.9435 17.3518 15.9932L17.25 16H6.75C6.33579 16 6 15.6642 6 15.25C6 14.8703 6.28215 14.5565 6.64823 14.5068L6.75 14.5H17.25H6.75ZM16.5 11C17.0523 11 17.5 11.4477 17.5 12C17.5 12.5523 17.0523 13 16.5 13C15.9477 13 15.5 12.5523 15.5 12C15.5 11.4477 15.9477 11 16.5 11ZM10.5049 11C11.0572 11 11.5049 11.4477 11.5049 12C11.5049 12.5523 11.0572 13 10.5049 13C9.95259 13 9.50488 12.5523 9.50488 12C9.50488 11.4477 9.95259 11 10.5049 11ZM7.50488 11C8.05716 11 8.50488 11.4477 8.50488 12C8.50488 12.5523 8.05716 13 7.50488 13C6.95259 13 6.50488 12.5523 6.50488 12C6.50488 11.4477 6.95259 11 7.50488 11ZM13.5049 11C14.0572 11 14.5049 11.4477 14.5049 12C14.5049 12.5523 14.0572 13 13.5049 13C12.9526 13 12.5049 12.5523 12.5049 12C12.5049 11.4477 12.9526 11 13.5049 11ZM6 8C6.55228 8 7 8.44772 7 9C7 9.55228 6.55228 10 6 10C5.44772 10 5 9.55228 5 9C5 8.44772 5.44772 8 6 8ZM8.99512 8C9.54741 8 9.99512 8.44772 9.99512 9C9.99512 9.55228 9.54741 10 8.99512 10C8.44284 10 7.99512 9.55228 7.99512 9C7.99512 8.44772 8.44284 8 8.99512 8ZM11.9951 8C12.5474 8 12.9951 8.44772 12.9951 9C12.9951 9.55228 12.5474 10 11.9951 10C11.4428 10 10.9951 9.55228 10.9951 9C10.9951 8.44772 11.4428 8 11.9951 8ZM14.9951 8C15.5474 8 15.9951 8.44772 15.9951 9C15.9951 9.55228 15.5474 10 14.9951 10C14.4428 10 13.9951 9.55228 13.9951 9C13.9951 8.44772 14.4428 8 14.9951 8ZM17.9951 8C18.5474 8 18.9951 8.44772 18.9951 9C18.9951 9.55228 18.5474 10 17.9951 10C17.4428 10 16.9951 9.55228 16.9951 9C16.9951 8.44772 17.4428 8 17.9951 8Z"),
			new IconSource("warning", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M5.82032 2.27536C6.77585 0.574908 9.22432 0.574731 10.1797 2.27536L14.6758 10.2793C15.6119 11.9457 14.4074 14.0037 12.4961 14.0039H3.50293C1.59177 14.0036 0.388135 11.9456 1.32422 10.2793L5.82032 2.27536ZM9.30763 2.76462C8.7344 1.74464 7.26586 1.74491 6.69239 2.76462L2.19531 10.7695C1.63411 11.7692 2.3565 13.0036 3.50293 13.0039H12.4961C13.6428 13.0037 14.3653 11.7693 13.8037 10.7695L9.30763 2.76462ZM8.00001 9.49999C8.41414 9.50007 8.74999 9.83585 8.75001 10.25C8.75001 10.6642 8.41415 10.9999 8.00001 11C7.5858 11 7.25001 10.6642 7.25001 10.25C7.25003 9.8358 7.58581 9.49999 8.00001 9.49999ZM8.00001 4.99998C8.27607 5.00006 8.49999 5.22391 8.50001 5.49998V7.99998C8.50001 8.27608 8.27608 8.4999 8.00001 8.49998C7.72387 8.49998 7.50001 8.27613 7.50001 7.99998V5.49998C7.50003 5.22386 7.72388 4.99998 8.00001 4.99998Z"),
			new IconSource("warning", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M7.37013 3.55566C8.50949 1.48158 11.4895 1.48168 12.629 3.55566L18.1251 13.5605C19.2227 15.5597 17.7759 18.0046 15.4952 18.0049H4.5039C2.22288 18.0048 0.776902 15.5598 1.87498 13.5605L7.37013 3.55566ZM11.753 4.03711C10.9934 2.65441 9.00677 2.65456 8.24709 4.03711L2.75096 14.042C2.01908 15.3748 2.98329 17.0048 4.5039 17.0049H15.4952C17.0156 17.0046 17.9798 15.3747 17.2481 14.042L11.753 4.03711ZM10 12.75C10.4142 12.7501 10.75 13.0858 10.75 13.5C10.75 13.9141 10.4141 14.2499 10 14.25C9.58587 14.25 9.2501 13.9141 9.25003 13.5C9.25003 13.0858 9.58582 12.75 10 12.75ZM10 6.5C10.2761 6.50007 10.5 6.7239 10.5 7V11C10.5 11.276 10.2761 11.4999 10 11.5C9.72394 11.5 9.50011 11.2761 9.50003 11V7C9.50003 6.72386 9.72389 6.5 10 6.5Z"),
			new IconSource("warning", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M9.13848 3.70711C10.3664 1.43126 13.6315 1.43104 14.8592 3.70711L21.6024 16.209C22.7697 18.374 21.2016 21.0014 18.742 21.002H5.25469C2.79486 21.0017 1.22681 18.3742 2.39433 16.209L9.13848 3.70711ZM13.5389 4.41902C12.8779 3.1935 11.119 3.19371 10.4578 4.41902L3.71464 16.921C3.08599 18.0867 3.93027 19.5017 5.25469 19.502H18.742C20.0661 19.5014 20.9105 18.0866 20.2821 16.921L13.5389 4.41902ZM11.9998 15.0001C12.5521 15.0001 12.9998 15.4478 12.9998 16.0001C12.9998 16.5523 12.5521 17.0001 11.9998 17.0001C11.4477 16.9999 10.9999 16.5522 10.9998 16.0001C10.9998 15.4479 11.4476 15.0002 11.9998 15.0001ZM11.9998 7.50007C12.414 7.50007 12.7498 7.83586 12.7498 8.25007V12.7501C12.7498 13.1642 12.414 13.5001 11.9998 13.5001C11.5857 13.4999 11.2499 13.1642 11.2498 12.7501V8.25007C11.2498 7.83593 11.5857 7.5002 11.9998 7.50007Z"),
			new IconSource("warning", ModernIconStyle.Regular, 28, FillMode.Winding,
				"M10.7383 4.89647C12.176 2.36788 15.8211 2.36775 17.2588 4.89647L25.501 19.3984C26.9217 21.8983 25.1166 25.0008 22.2412 25.001H5.75585C2.88049 25.001 1.07464 21.8983 2.49511 19.3984L10.7383 4.89647ZM15.9541 5.63768C15.0914 4.12085 12.9047 4.12082 12.042 5.63768L3.7998 20.1396C2.94742 21.6396 4.0306 23.501 5.75585 23.501H22.2412C23.9663 23.5008 25.0495 21.6395 24.1973 20.1396L15.9541 5.63768ZM14 18C14.5522 18 15 18.4477 15 19C15 19.5523 14.5523 20 14 20C13.4477 20 13 19.5523 13 19C13 18.4477 13.4477 18 14 18ZM14 8.99999C14.4142 9.00003 14.75 9.33581 14.75 9.74999V15.75C14.75 16.1642 14.4142 16.5 14 16.5C13.5858 16.5 13.25 16.1642 13.25 15.75V9.74999C13.25 9.33579 13.5858 8.99999 14 8.99999Z"),
			new IconSource("warning", ModernIconStyle.Regular, 32, FillMode.Winding,
				"M12.0717 4.30556C13.7888 1.23195 18.2118 1.23217 19.9291 4.30556L29.4233 21.3036C31.0984 24.3031 28.9302 27.9979 25.4946 27.9979H6.50628C3.07097 27.9976 0.902758 24.3029 2.57756 21.3036L12.0717 4.30556ZM18.183 5.28114C17.2289 3.57374 14.7717 3.57354 13.8178 5.28114L4.32366 22.2792C3.39371 23.9454 4.59806 25.9976 6.50628 25.9979H25.4946C27.4032 25.9979 28.6075 23.9455 27.6772 22.2792L18.183 5.28114ZM16.0004 19.4999C16.8286 19.5001 17.5004 20.1716 17.5004 20.9999C17.5004 21.8282 16.8287 22.4997 16.0004 22.4999C15.172 22.4999 14.5004 21.8283 14.5004 20.9999C14.5005 20.1715 15.172 19.4999 16.0004 19.4999ZM16.0004 8.9999C16.5525 9.00011 17.0004 9.44779 17.0004 9.9999V16.4999C17.0004 17.052 16.5525 17.4997 16.0004 17.4999C15.4481 17.4999 15.0004 17.0522 15.0004 16.4999V9.9999C15.0005 9.44766 15.4482 8.9999 16.0004 8.9999Z"),
			new IconSource("text_effects", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M14.2984 4.01519C13.9045 3.09599 13.0006 2.5 12.0006 2.5C11.0005 2.5 10.0966 3.096 9.7027 4.01522L5.4388 13.9645C5.42322 13.9987 5.40839 14.0332 5.39433 14.0682L3.70277 18.0153C3.15889 19.2844 3.74678 20.754 5.01586 21.2979C6.28493 21.8418 7.75463 21.2539 8.29851 19.9848L9.3634 17.5001H14.6379L15.7028 19.9849C16.2467 21.2539 17.7164 21.8418 18.9854 21.2979C20.2545 20.754 20.8424 19.2843 20.2985 18.0153L18.6069 14.0682C18.5928 14.0332 18.578 13.9986 18.5624 13.9644L14.2984 4.01519ZM12.495 12.5001L12.0006 11.3465L11.5062 12.5001H12.495ZM12.9197 4.60608L17.1968 14.5858C17.203 14.5995 17.2089 14.6133 17.2146 14.6273L18.9198 18.6061C19.1373 19.1138 18.9022 19.7016 18.3946 19.9192C17.8869 20.1368 17.2991 19.9016 17.0815 19.394L15.6269 16.0001H8.3743L6.91979 19.394C6.70224 19.9016 6.11436 20.1368 5.60673 19.9192C5.0991 19.7017 4.86394 19.1138 5.08149 18.6061L6.78667 14.6273C6.7923 14.6133 6.79823 14.5995 6.80446 14.5858L11.0814 4.60609C11.239 4.2384 11.6005 4 12.0006 4C12.4006 4 12.7621 4.23839 12.9197 4.60608ZM14.7698 14.0001H9.23143L12.0006 7.5386L14.7698 14.0001Z"),
			new IconSource("eye_off", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M2.21967 2.21967C1.9534 2.48594 1.9292 2.9026 2.14705 3.19621L2.21967 3.28033L6.25424 7.3149C4.33225 8.66437 2.89577 10.6799 2.29888 13.0644C2.1983 13.4662 2.4425 13.8735 2.84431 13.9741C3.24613 14.0746 3.6534 13.8305 3.75399 13.4286C4.28346 11.3135 5.59112 9.53947 7.33416 8.39452L9.14379 10.2043C8.43628 10.9258 8 11.9143 8 13.0046C8 15.2138 9.79086 17.0046 12 17.0046C13.0904 17.0046 14.0788 16.5683 14.8004 15.8608L20.7197 21.7803C21.0126 22.0732 21.4874 22.0732 21.7803 21.7803C22.0466 21.5141 22.0708 21.0974 21.8529 20.8038L21.7803 20.7197L15.6668 14.6055L15.668 14.604L14.4679 13.4061L11.598 10.5368L11.6 10.536L8.71877 7.65782L8.72 7.656L7.58672 6.52549L3.28033 2.21967C2.98744 1.92678 2.51256 1.92678 2.21967 2.21967ZM10.2041 11.2655L13.7392 14.8006C13.2892 15.2364 12.6759 15.5046 12 15.5046C10.6193 15.5046 9.5 14.3853 9.5 13.0046C9.5 12.3287 9.76824 11.7154 10.2041 11.2655ZM12 5.5C10.9997 5.5 10.0291 5.64807 9.11109 5.925L10.3481 7.16119C10.8839 7.05532 11.4364 7 12 7C15.9231 7 19.3099 9.68026 20.2471 13.4332C20.3475 13.835 20.7546 14.0794 21.1565 13.9791C21.5584 13.8787 21.8028 13.4716 21.7024 13.0697C20.5994 8.65272 16.6155 5.5 12 5.5ZM12.1947 9.00928L15.996 12.81C15.8942 10.7531 14.2472 9.10764 12.1947 9.00928Z"),
			new IconSource("text_bold", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M6.93514 4.43963C7.21647 4.15815 7.59813 4 7.99609 4H12.3795C15.0161 4 16.9995 6.18211 16.9995 8.62541C16.9995 9.60205 16.6828 10.5367 16.1341 11.307C17.211 12.134 17.9995 13.4274 17.9995 15.12C17.9995 18.2315 15.301 20 13.1195 20H7.99951C7.17108 20 6.49951 19.3284 6.49951 18.5L6.49609 5.50082C6.49588 5.10285 6.65382 4.72111 6.93514 4.43963ZM9.49869 10.2499H12.3795C13.2824 10.2499 13.9995 9.49051 13.9995 8.62541C13.9995 7.76 13.2814 7 12.3795 7H9.49691L9.49869 10.2499ZM9.49951 13.2499V17H13.1195C13.9927 17 14.9995 16.2464 14.9995 15.12C14.9995 13.9901 14.0248 13.2499 13.1195 13.2499H9.49951Z"),
			new IconSource("text_underline", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M6.75001 19H17.25C17.6642 19 18 19.3358 18 19.75C18 20.1297 17.7179 20.4435 17.3518 20.4932L17.25 20.5H6.75001C6.33579 20.5 6.00001 20.1642 6.00001 19.75C6.00001 19.3703 6.28216 19.0565 6.64824 19.0068L6.75001 19H17.25H6.75001ZM17.25 4C17.6297 4 17.9435 4.28215 17.9932 4.64823L18 4.75V10.75C18 15.1437 15.9371 17.5 11.9971 17.5C8.14241 17.5 6.08619 15.2446 6.00265 11.0336L6 10.75V4.75C6 4.33579 6.33579 4 6.75001 4C7.1297 4 7.44349 4.28215 7.49315 4.64823L7.5 4.75V10.7507C7.49647 14.3568 8.93346 16 11.9971 16C14.9824 16 16.4251 14.4395 16.4972 11.0235L16.5 10.75V4.75C16.5 4.33579 16.8358 4 17.25 4Z"),
			new IconSource("text_italic", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M18.75 4C19.1642 4 19.5 4.33579 19.5 4.75C19.5 5.16421 19.1642 5.5 18.75 5.5H15.2676L10.3359 18.5H13.75C14.1642 18.5 14.5 18.8358 14.5 19.25C14.5 19.6642 14.1642 20 13.75 20H5.25C4.83579 20 4.5 19.6642 4.5 19.25C4.5 18.8358 4.83579 18.5 5.25 18.5H8.73242L13.6641 5.5H10.25C9.83579 5.5 9.5 5.16421 9.5 4.75C9.5 4.33579 9.83579 4 10.25 4H18.75Z"),
			new IconSource("image_off", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3.28034 2.21968C2.98745 1.92678 2.51257 1.92677 2.21968 2.21966C1.92678 2.51255 1.92677 2.98743 2.21966 3.28032L3.48326 4.54395C3.17684 5.03983 3 5.62426 3 6.25V17.75C3 19.5449 4.45507 21 6.25 21H17.75C18.3757 21 18.96 20.8232 19.4559 20.5169L20.7194 21.7805C21.0123 22.0734 21.4872 22.0734 21.7801 21.7805C22.073 21.4876 22.073 21.0127 21.7801 20.7198L3.28034 2.21968ZM11.1189 12.1797C10.8678 12.2864 10.6322 12.4409 10.4258 12.643L4.60326 18.3437C4.53643 18.1583 4.5 17.9584 4.5 17.75V6.25C4.5 6.0437 4.5357 5.84574 4.60125 5.66196L11.1189 12.1797ZM19.5 6.25V16.3183L20.9993 17.8176C20.9998 17.7951 21 17.7726 21 17.75V6.25C21 4.45507 19.5449 3 17.75 3H6.25C6.2275 3 6.20505 3.00023 6.18265 3.00068L7.68194 4.5H17.75C18.7165 4.5 19.5 5.2835 19.5 6.25ZM18.3305 19.4014C18.1489 19.4653 17.9535 19.5 17.75 19.5H6.25C6.04613 19.5 5.8504 19.4651 5.66845 19.4011L11.4752 13.7148L11.5588 13.644C11.8501 13.4316 12.2596 13.4553 12.5247 13.7148L18.3305 19.4014ZM15.2521 6.5C16.4959 6.5 17.5042 7.50831 17.5042 8.75212C17.5042 9.99592 16.4959 11.0042 15.2521 11.0042C14.0083 11.0042 13 9.99592 13 8.75212C13 7.50831 14.0083 6.5 15.2521 6.5ZM15.2521 8C14.8367 8 14.5 8.33673 14.5 8.75212C14.5 9.1675 14.8367 9.50423 15.2521 9.50423C15.6675 9.50423 16.0042 9.1675 16.0042 8.75212C16.0042 8.33673 15.6675 8 15.2521 8Z"),
			new IconSource("save", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M3 5.75C3 4.23122 4.23122 3 5.75 3H15.7145C16.5764 3 17.4031 3.34241 18.0126 3.9519L20.0481 5.98744C20.6576 6.59693 21 7.42358 21 8.28553V18.25C21 19.7688 19.7688 21 18.25 21H5.75C4.23122 21 3 19.7688 3 18.25V5.75ZM5.75 4.5C5.05964 4.5 4.5 5.05964 4.5 5.75V18.25C4.5 18.9404 5.05964 19.5 5.75 19.5H6V14.25C6 13.0074 7.00736 12 8.25 12H15.75C16.9926 12 18 13.0074 18 14.25V19.5H18.25C18.9404 19.5 19.5 18.9404 19.5 18.25V8.28553C19.5 7.8214 19.3156 7.37629 18.9874 7.0481L16.9519 5.01256C16.6918 4.75246 16.3582 4.58269 16 4.52344V7.25C16 8.49264 14.9926 9.5 13.75 9.5H9.25C8.00736 9.5 7 8.49264 7 7.25V4.5H5.75ZM16.5 19.5V14.25C16.5 13.8358 16.1642 13.5 15.75 13.5H8.25C7.83579 13.5 7.5 13.8358 7.5 14.25V19.5H16.5ZM8.5 4.5V7.25C8.5 7.66421 8.83579 8 9.25 8H13.75C14.1642 8 14.5 7.66421 14.5 7.25V4.5H8.5Z"),
			new IconSource("subtitles", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M2 7.25C2 5.45507 3.45507 4 5.25 4H18.75C20.5449 4 22 5.45507 22 7.25V16.75C22 18.5449 20.5449 20 18.75 20H5.25C3.45507 20 2 18.5449 2 16.75V7.25ZM5.25 5.5C4.2835 5.5 3.5 6.2835 3.5 7.25V16.75C3.5 17.7165 4.2835 18.5 5.25 18.5H18.75C19.7165 18.5 20.5 17.7165 20.5 16.75V7.25C20.5 6.2835 19.7165 5.5 18.75 5.5H5.25ZM5 13.25C5 12.8358 5.33579 12.5 5.75 12.5H13.25C13.6642 12.5 14 12.8358 14 13.25C14 13.6642 13.6642 14 13.25 14H5.75C5.33579 14 5 13.6642 5 13.25ZM15.75 12.5C15.3358 12.5 15 12.8358 15 13.25C15 13.6642 15.3358 14 15.75 14H18.25C18.6642 14 19 13.6642 19 13.25C19 12.8358 18.6642 12.5 18.25 12.5H15.75ZM5 16.25C5 15.8358 5.33579 15.5 5.75 15.5H9.25C9.66421 15.5 10 15.8358 10 16.25C10 16.6642 9.66421 17 9.25 17H5.75C5.33579 17 5 16.6642 5 16.25ZM11.75 15.5C11.3358 15.5 11 15.8358 11 16.25C11 16.6642 11.3358 17 11.75 17H18.25C18.6642 17 19 16.6642 19 16.25C19 15.8358 18.6642 15.5 18.25 15.5H11.75Z"),
			new IconSource("text_case_title", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M15.75 3C15.3358 3 15 3.33579 15 3.75V19.25C15 19.6642 15.3358 20 15.75 20C16.1642 20 16.5 19.6642 16.5 19.25V19.013C17.1848 19.6306 18.0541 20 19 20C21.2091 20 23 17.9853 23 15.5C23 13.0147 21.2091 11 19 11C18.0541 11 17.1848 11.3694 16.5 11.987V3.75C16.5 3.33579 16.1642 3 15.75 3ZM16.5 15.5C16.5 13.6722 17.7805 12.5 19 12.5C20.2195 12.5 21.5 13.6722 21.5 15.5C21.5 17.3278 20.2195 18.5 19 18.5C17.7805 18.5 16.5 17.3278 16.5 15.5ZM7.76083 3.00008C8.07443 3.00452 8.35214 3.20362 8.45702 3.49919L13.957 18.9992C14.0955 19.3896 13.8914 19.8183 13.501 19.9568C13.1106 20.0953 12.6819 19.8912 12.5434 19.5008L11.1237 15.5L4.00605 15.5L2.44963 19.5207C2.3001 19.907 1.86574 20.099 1.47946 19.9494C1.09317 19.7999 0.901247 19.3655 1.05078 18.9793L7.05078 3.47925C7.16399 3.18678 7.44724 2.99563 7.76083 3.00008ZM7.71962 5.90659L4.58669 14L10.5915 14L7.71962 5.90659Z"),
			new IconSource("number_symbol", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M10.9869 2.88953C11.0639 2.48254 10.7964 2.09019 10.3894 2.0132C9.98238 1.93622 9.59003 2.20374 9.51305 2.61074L8.49396 7.99815L3.74965 8.00037C3.33544 8.00056 2.99981 8.3365 3 8.75072C3.00019 9.16493 3.33614 9.50056 3.75035 9.50037L8.2102 9.49828L7.26445 14.498L2.74965 14.5001C2.33544 14.5003 1.99981 14.8363 2 15.2505C2.00019 15.6647 2.33614 16.0003 2.75035 16.0001L6.98068 15.9981L6.01302 21.1137C5.93603 21.5207 6.20356 21.9131 6.61055 21.99C7.01755 22.067 7.4099 21.7995 7.48688 21.3925L8.50742 15.9974L13.9814 15.9949L13.0131 21.1137C12.9361 21.5207 13.2036 21.9131 13.6106 21.99C14.0176 22.067 14.41 21.7995 14.4869 21.3925L15.5081 15.9942L20.2503 15.9919C20.6646 15.9917 21.0002 15.6558 21 15.2416C20.9998 14.8274 20.6639 14.4918 20.2496 14.4919L15.7919 14.494L16.7376 9.4943L21.2503 9.49219C21.6646 9.49199 22.0002 9.15605 22 8.74184C21.9998 8.32762 21.6639 7.99199 21.2496 7.99219L17.0214 7.99416L17.987 2.88953C18.064 2.48254 17.7964 2.09019 17.3894 2.0132C16.9824 1.93622 16.5901 2.20374 16.5131 2.61074L15.4946 7.99488L10.0207 7.99744L10.9869 2.88953ZM9.73693 9.49757L15.2109 9.49501L14.2651 14.4947L8.79118 14.4973L9.73693 9.49757Z"),
			new IconSource("number_row", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M8.75 4C10.2688 4 11.5 5.23122 11.5 6.75V17.25C11.5 18.7688 10.2688 20 8.75 20H4.75C3.23122 20 2 18.7688 2 17.25V6.75C2 5.23122 3.23122 4 4.75 4H8.75ZM8.75 5.5H4.75C4.05964 5.5 3.5 6.05964 3.5 6.75V17.25C3.5 17.9404 4.05964 18.5 4.75 18.5H8.75C9.44036 18.5 10 17.9404 10 17.25V6.75C10 6.05964 9.44036 5.5 8.75 5.5ZM19.25 4C20.7688 4 22 5.23122 22 6.75V17.25C22 18.7688 20.7688 20 19.25 20H15.25C13.7312 20 12.5 18.7688 12.5 17.25V6.75C12.5 5.23122 13.7312 4 15.25 4H19.25ZM19.25 5.5H15.25C14.5596 5.5 14 6.05964 14 6.75V17.25C14 17.9404 14.5596 18.5 15.25 18.5H19.25C19.9404 18.5 20.5 17.9404 20.5 17.25V6.75C20.5 6.05964 19.9404 5.5 19.25 5.5ZM6 9V15C6 15.4142 6.33579 15.75 6.75 15.75C7.16421 15.75 7.5 15.4142 7.5 15V9C7.5 8.58579 7.16421 8.25 6.75 8.25C6.33579 8.25 6 8.58579 6 9ZM17.5 9.75V11.25H16.25C15.8358 11.25 15.5 11.5858 15.5 12V15C15.5 15.4142 15.8358 15.75 16.25 15.75H18.25C18.6642 15.75 19 15.4142 19 15C19 14.5858 18.6642 14.25 18.25 14.25H17V12.75H18.25C18.6642 12.75 19 12.4142 19 12V9C19 8.58579 18.6642 8.25 18.25 8.25H16.25C15.8358 8.25 15.5 8.58579 15.5 9C15.5 9.41421 15.8358 9.75 16.25 9.75H17.5Z"),
			new IconSource("arrow_bidirectional_left_right", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M9.03033 6.46967C9.32322 6.76256 9.32322 7.23744 9.03033 7.53033L5.56066 11H18.4393L14.9697 7.53033C14.6768 7.23744 14.6768 6.76256 14.9697 6.46967C15.2626 6.17678 15.7374 6.17678 16.0303 6.46967L20.7803 11.2197C21.0732 11.5126 21.0732 11.9874 20.7803 12.2803L16.0303 17.0303C15.7374 17.3232 15.2626 17.3232 14.9697 17.0303C14.6768 16.7374 14.6768 16.2626 14.9697 15.9697L18.4393 12.5H5.56066L9.03033 15.9697C9.32322 16.2626 9.32322 16.7374 9.03033 17.0303C8.73744 17.3232 8.26256 17.3232 7.96967 17.0303L3.21967 12.2803C2.92678 11.9874 2.92678 11.5126 3.21967 11.2197L7.96967 6.46967C8.26256 6.17678 8.73744 6.17678 9.03033 6.46967Z"),
			new IconSource("number_circle_1", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M13.5002 7.25001C13.5002 6.86442 13.2078 6.54166 12.8241 6.50366C12.4404 6.46567 12.0904 6.72482 12.0148 7.10292C11.9191 7.58119 11.6109 8.22272 11.136 8.80936C10.6617 9.39534 10.0847 9.84794 9.51303 10.0385C9.12008 10.1695 8.90771 10.5942 9.03869 10.9872C9.16968 11.3801 9.59442 11.5925 9.98737 11.4615C10.7655 11.2022 11.452 10.6878 12.0002 10.1003V16.25C12.0002 16.6642 12.336 17 12.7502 17C13.1644 17 13.5002 16.6642 13.5002 16.25V7.25001ZM12 2C6.47715 2 2 6.47715 2 12C2 17.5228 6.47715 22 12 22C17.5228 22 22 17.5228 22 12C22 6.47715 17.5228 2 12 2ZM3.5 12C3.5 7.30558 7.30558 3.5 12 3.5C16.6944 3.5 20.5 7.30558 20.5 12C20.5 16.6944 16.6944 20.5 12 20.5C7.30558 20.5 3.5 16.6944 3.5 12Z"),
			new IconSource("transparency_square", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M6.25 3C4.45507 3 3 4.45507 3 6.25V17.75C3 19.5449 4.45507 21 6.25 21H17.75C19.5449 21 21 19.5449 21 17.75V6.25C21 4.45507 19.5449 3 17.75 3H6.25ZM4.5 6.25C4.5 5.2835 5.2835 4.5 6.25 4.5H8V8H4.5V6.25ZM4.5 12H8V8H12V4.5H16V8H19.5V12H16V16H19.5V17.75C19.5 18.7165 18.7165 19.5 17.75 19.5H16V16H12V19.5H8V16H4.5V12ZM12 12V8H16V12H12ZM12 12H8V16H12V12Z"),
			new IconSource("textbox_align_top", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.75 3C4.23122 3 3 4.23122 3 5.75V18.25C3 19.7688 4.23122 21 5.75 21H18.25C19.7688 21 21 19.7688 21 18.25V5.75C21 4.23122 19.7688 3 18.25 3H5.75ZM4.5 5.75C4.5 5.05964 5.05964 4.5 5.75 4.5H18.25C18.9404 4.5 19.5 5.05964 19.5 5.75V18.25C19.5 18.9404 18.9404 19.5 18.25 19.5H5.75C5.05964 19.5 4.5 18.9404 4.5 18.25V5.75ZM7.25 7C6.83579 7 6.5 7.33579 6.5 7.75C6.5 8.16421 6.83579 8.5 7.25 8.5H16.75C17.1642 8.5 17.5 8.16421 17.5 7.75C17.5 7.33579 17.1642 7 16.75 7H7.25ZM7.25 11C6.83579 11 6.5 11.3358 6.5 11.75C6.5 12.1642 6.83579 12.5 7.25 12.5H16.75C17.1642 12.5 17.5 12.1642 17.5 11.75C17.5 11.3358 17.1642 11 16.75 11H7.25Z"),
			new IconSource("textbox_align_middle", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.75 3C4.23122 3 3 4.23122 3 5.75V18.25C3 19.7688 4.23122 21 5.75 21H18.25C19.7688 21 21 19.7688 21 18.25V5.75C21 4.23122 19.7688 3 18.25 3H5.75ZM4.5 5.75C4.5 5.05964 5.05964 4.5 5.75 4.5H18.25C18.9404 4.5 19.5 5.05964 19.5 5.75V18.25C19.5 18.9404 18.9404 19.5 18.25 19.5H5.75C5.05964 19.5 4.5 18.9404 4.5 18.25V5.75ZM7.25 9C6.83579 9 6.5 9.33579 6.5 9.75C6.5 10.1642 6.83579 10.5 7.25 10.5H16.75C17.1642 10.5 17.5 10.1642 17.5 9.75C17.5 9.33579 17.1642 9 16.75 9H7.25ZM7.25 13C6.83579 13 6.5 13.3358 6.5 13.75C6.5 14.1642 6.83579 14.5 7.25 14.5H16.75C17.1642 14.5 17.5 14.1642 17.5 13.75C17.5 13.3358 17.1642 13 16.75 13H7.25Z"),
			new IconSource("textbox_align_bottom", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5.75 3C4.23122 3 3 4.23122 3 5.75V18.25C3 19.7688 4.23122 21 5.75 21H18.25C19.7688 21 21 19.7688 21 18.25V5.75C21 4.23122 19.7688 3 18.25 3H5.75ZM4.5 5.75C4.5 5.05964 5.05964 4.5 5.75 4.5H18.25C18.9404 4.5 19.5 5.05964 19.5 5.75V18.25C19.5 18.9404 18.9404 19.5 18.25 19.5H5.75C5.05964 19.5 4.5 18.9404 4.5 18.25V5.75ZM7.25 11.5C6.83579 11.5 6.5 11.8358 6.5 12.25C6.5 12.6642 6.83579 13 7.25 13H16.75C17.1642 13 17.5 12.6642 17.5 12.25C17.5 11.8358 17.1642 11.5 16.75 11.5H7.25ZM7.25 15.5C6.83579 15.5 6.5 15.8358 6.5 16.25C6.5 16.6642 6.83579 17 7.25 17H16.75C17.1642 17 17.5 16.6642 17.5 16.25C17.5 15.8358 17.1642 15.5 16.75 15.5H7.25Z"),
			new IconSource("text_align_left", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M2 5.75C2 5.33579 2.33579 5 2.75 5H18.25C18.6642 5 19 5.33579 19 5.75C19 6.16421 18.6642 6.5 18.25 6.5H2.75C2.33579 6.5 2 6.16421 2 5.75ZM2 18.75C2 18.3358 2.33579 18 2.75 18H13.25C13.6642 18 14 18.3358 14 18.75C14 19.1642 13.6642 19.5 13.25 19.5H2.75C2.33579 19.5 2 19.1642 2 18.75ZM2.75 11.5C2.33579 11.5 2 11.8358 2 12.25C2 12.6642 2.33579 13 2.75 13H21.25C21.6642 13 22 12.6642 22 12.25C22 11.8358 21.6642 11.5 21.25 11.5H2.75Z"),
			new IconSource("text_align_center", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M4 5.75C4 5.33579 4.33579 5 4.75 5H19.25C19.6642 5 20 5.33579 20 5.75C20 6.16421 19.6642 6.5 19.25 6.5H4.75C4.33579 6.5 4 6.16421 4 5.75ZM6 18.75C6 18.3358 6.33579 18 6.75 18H17.25C17.6642 18 18 18.3358 18 18.75C18 19.1642 17.6642 19.5 17.25 19.5H6.75C6.33579 19.5 6 19.1642 6 18.75ZM2.75 11.5C2.33579 11.5 2 11.8358 2 12.25C2 12.6642 2.33579 13 2.75 13H21.25C21.6642 13 22 12.6642 22 12.25C22 11.8358 21.6642 11.5 21.25 11.5H2.75Z"),
			new IconSource("text_align_right", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M5 5.75C5 5.33579 5.33579 5 5.75 5H21.25C21.6642 5 22 5.33579 22 5.75C22 6.16421 21.6642 6.5 21.25 6.5H5.75C5.33579 6.5 5 6.16421 5 5.75ZM10 18.75C10 18.3358 10.3358 18 10.75 18H21.25C21.6642 18 22 18.3358 22 18.75C22 19.1642 21.6642 19.5 21.25 19.5H10.75C10.3358 19.5 10 19.1642 10 18.75ZM2.75 11.5C2.33579 11.5 2 11.8358 2 12.25C2 12.6642 2.33579 13 2.75 13H21.25C21.6642 13 22 12.6642 22 12.25C22 11.8358 21.6642 11.5 21.25 11.5H2.75Z"),
			new IconSource("image_border", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M14 9C13.4477 9 13 9.44772 13 10C13 10.5523 13.4477 11 14 11C14.5523 11 15 10.5523 15 10C15 9.44772 14.5523 9 14 9ZM7.25 6.5C6.83579 6.5 6.5 6.83579 6.5 7.25V16.75C6.5 17.1642 6.83579 17.5 7.25 17.5H16.75C17.1642 17.5 17.5 17.1642 17.5 16.75V7.25C17.5 6.83579 17.1642 6.5 16.75 6.5H7.25ZM10.409 12.659L8 15.068V8H16V15.068L13.591 12.659C12.7123 11.7803 11.2877 11.7803 10.409 12.659ZM12.5303 13.7197L14.8107 16H9.18934L11.4697 13.7197C11.7626 13.4268 12.2374 13.4268 12.5303 13.7197ZM6.25 3C4.45507 3 3 4.45507 3 6.25V17.75C3 19.5449 4.45507 21 6.25 21H17.75C19.5449 21 21 19.5449 21 17.75V6.25C21 4.45507 19.5449 3 17.75 3H6.25ZM4.5 6.25C4.5 5.2835 5.2835 4.5 6.25 4.5H17.75C18.7165 4.5 19.5 5.2835 19.5 6.25V17.75C19.5 18.7165 18.7165 19.5 17.75 19.5H6.25C5.2835 19.5 4.5 18.7165 4.5 17.75V6.25Z"),
			new IconSource("image", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M17.75 3C19.5449 3 21 4.45507 21 6.25V17.75C21 19.5449 19.5449 21 17.75 21H6.25C4.45507 21 3 19.5449 3 17.75V6.25C3 4.45507 4.45507 3 6.25 3H17.75ZM18.3305 19.4014L12.5247 13.7148C12.2596 13.4553 11.8501 13.4316 11.5588 13.644L11.4752 13.7148L5.66845 19.4011C5.8504 19.4651 6.04613 19.5 6.25 19.5H17.75C17.9535 19.5 18.1489 19.4653 18.3305 19.4014L12.5247 13.7148L18.3305 19.4014ZM17.75 4.5H6.25C5.2835 4.5 4.5 5.2835 4.5 6.25V17.75C4.5 17.9584 4.53643 18.1583 4.60326 18.3437L10.4258 12.643C11.2589 11.8273 12.5675 11.7885 13.4458 12.5266L13.5742 12.6431L19.3964 18.3447C19.4634 18.159 19.5 17.9588 19.5 17.75V6.25C19.5 5.2835 18.7165 4.5 17.75 4.5ZM15.2521 6.5C16.4959 6.5 17.5042 7.50831 17.5042 8.75212C17.5042 9.99592 16.4959 11.0042 15.2521 11.0042C14.0083 11.0042 13 9.99592 13 8.75212C13 7.50831 14.0083 6.5 15.2521 6.5ZM15.2521 8C14.8367 8 14.5 8.33673 14.5 8.75212C14.5 9.1675 14.8367 9.50423 15.2521 9.50423C15.6675 9.50423 16.0042 9.1675 16.0042 8.75212C16.0042 8.33673 15.6675 8 15.2521 8Z"),
			new IconSource("question_circle", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M8 1C11.866 1 15 4.13401 15 8C15 11.866 11.866 15 8 15C4.13401 15 1 11.866 1 8C1 4.13401 4.13401 1 8 1ZM8 2C4.68629 2 2 4.68629 2 8C2 11.3137 4.68629 14 8 14C11.3137 14 14 11.3137 14 8C14 4.68629 11.3137 2 8 2ZM8 10.5C8.41421 10.5 8.75 10.8358 8.75 11.25C8.75 11.6642 8.41421 12 8 12C7.58579 12 7.25 11.6642 7.25 11.25C7.25 10.8358 7.58579 10.5 8 10.5ZM8 4C9.10457 4 10 4.89543 10 6C10 6.45796 9.87522 6.8032 9.6748 7.08887C9.49738 7.34169 9.24942 7.55239 9.09668 7.69922C8.92663 7.86268 8.78826 8.01726 8.68555 8.2334C8.58291 8.44944 8.5 8.7612 8.5 9.25C8.5 9.52614 8.27614 9.75 8 9.75C7.72386 9.75 7.5 9.52614 7.5 9.25C7.5 8.64401 7.60468 8.18045 7.7832 7.80469C7.96165 7.42917 8.19844 7.17548 8.40332 6.97852C8.62547 6.76497 8.75265 6.66255 8.85645 6.51465C8.93728 6.39943 9 6.25513 9 6C9 5.44772 8.55228 5 8 5C7.44772 5 7 5.44772 7 6C7 6.27614 6.77614 6.5 6.5 6.5C6.22386 6.5 6 6.27614 6 6C6 4.89543 6.89543 4 8 4Z"),
			new IconSource("question_circle", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M10 2C14.4183 2 18 5.58172 18 10C18 14.4183 14.4183 18 10 18C5.58172 18 2 14.4183 2 10C2 5.58172 5.58172 2 10 2ZM10 3C6.13401 3 3 6.13401 3 10C3 13.866 6.13401 17 10 17C13.866 17 17 13.866 17 10C17 6.13401 13.866 3 10 3ZM10 13.5C10.4142 13.5 10.75 13.8358 10.75 14.25C10.75 14.6642 10.4142 15 10 15C9.58579 15 9.25 14.6642 9.25 14.25C9.25 13.8358 9.58579 13.5 10 13.5ZM10 5.5C11.3807 5.5 12.5 6.61929 12.5 8C12.5 8.72959 12.1848 9.40774 11.6513 9.8771L11.4967 10.0024L11.2782 10.1655L11.1906 10.2372C11.1348 10.2851 11.0835 10.3337 11.0346 10.3859C10.6963 10.7464 10.5 11.2422 10.5 12C10.5 12.2761 10.2761 12.5 10 12.5C9.72386 12.5 9.5 12.2761 9.5 12C9.5 10.988 9.79312 10.2475 10.3054 9.70162C10.4165 9.5832 10.532 9.47988 10.6609 9.37874L10.9076 9.19439L11.0256 9.09468C11.325 8.81435 11.5 8.42206 11.5 8C11.5 7.17157 10.8284 6.5 10 6.5C9.17157 6.5 8.5 7.17157 8.5 8C8.5 8.27614 8.27614 8.5 8 8.5C7.72386 8.5 7.5 8.27614 7.5 8C7.5 6.61929 8.61929 5.5 10 5.5Z"),
			new IconSource("question_circle", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12 2C17.523 2 22 6.478 22 12C22 17.522 17.523 22 12 22C6.477 22 2 17.522 2 12C2 6.478 6.477 2 12 2ZM12 3.667C7.405 3.667 3.667 7.405 3.667 12C3.667 16.595 7.405 20.333 12 20.333C16.595 20.333 20.333 16.595 20.333 12C20.333 7.405 16.595 3.667 12 3.667ZM12 15.5C12.5523 15.5 13 15.9477 13 16.5C13 17.0523 12.5523 17.5 12 17.5C11.4477 17.5 11 17.0523 11 16.5C11 15.9477 11.4477 15.5 12 15.5ZM12 6.75C13.5188 6.75 14.75 7.98122 14.75 9.5C14.75 10.5108 14.4525 11.074 13.6989 11.8586L13.5303 12.0303C12.9084 12.6522 12.75 12.9163 12.75 13.5C12.75 13.9142 12.4142 14.25 12 14.25C11.5858 14.25 11.25 13.9142 11.25 13.5C11.25 12.4892 11.5475 11.926 12.3011 11.1414L12.4697 10.9697C13.0916 10.3478 13.25 10.0837 13.25 9.5C13.25 8.80964 12.6904 8.25 12 8.25C11.3528 8.25 10.8205 8.74187 10.7565 9.37219L10.75 9.5C10.75 9.91421 10.4142 10.25 10 10.25C9.58579 10.25 9.25 9.91421 9.25 9.5C9.25 7.98122 10.4812 6.75 12 6.75Z"),
			new IconSource("globe", ModernIconStyle.Regular, 16, FillMode.Winding,
				"M8 14C11.3137 14 14 11.3137 14 8C14 4.68629 11.3137 2 8 2C4.68629 2 2 4.68629 2 8C2 11.3137 4.68629 14 8 14ZM8 3C8.37372 3 8.87543 3.35608 9.31258 4.31781C9.4073 4.52619 9.49448 4.75446 9.57265 5H6.42735C6.50552 4.75446 6.5927 4.52619 6.68742 4.31781C7.12457 3.35608 7.62628 3 8 3ZM5.77705 3.90401C5.62614 4.23601 5.49428 4.6038 5.38411 5H3.99963C4.52341 4.30269 5.22525 3.74677 6.03766 3.39978C5.94287 3.56117 5.85596 3.7304 5.77705 3.90401ZM5.16299 6C5.05694 6.6275 5 7.30146 5 8C5 8.69854 5.05694 9.3725 5.16299 10H3.41604C3.14845 9.38754 3 8.7111 3 8C3 7.2889 3.14845 6.61246 3.41604 6H5.16299ZM5.38411 11C5.49428 11.3962 5.62614 11.764 5.77705 12.096C5.85596 12.2696 5.94287 12.4388 6.03766 12.6002C5.22525 12.2532 4.52341 11.6973 3.99963 11H5.38411ZM6.42735 11H9.57265C9.49448 11.2455 9.4073 11.4738 9.31258 11.6822C8.87543 12.6439 8.37372 13 8 13C7.62628 13 7.12457 12.6439 6.68742 11.6822C6.5927 11.4738 6.50552 11.2455 6.42735 11ZM9.82134 10H6.17866C6.06438 9.3892 6 8.71396 6 8C6 7.28604 6.06438 6.6108 6.17866 6H9.82134C9.93562 6.6108 10 7.28604 10 8C10 8.71396 9.93562 9.3892 9.82134 10ZM10.6159 11H12.0004C11.4766 11.6973 10.7747 12.2532 9.96234 12.6002C10.0571 12.4388 10.144 12.2696 10.2229 12.096C10.3739 11.764 10.5057 11.3962 10.6159 11ZM12.584 10H10.837C10.9431 9.3725 11 8.69854 11 8C11 7.30146 10.9431 6.6275 10.837 6H12.584C12.8516 6.61246 13 7.2889 13 8C13 8.7111 12.8516 9.38754 12.584 10ZM9.96234 3.39978C10.7747 3.74677 11.4766 4.30269 12.0004 5H10.6159C10.5057 4.6038 10.3739 4.23601 10.2229 3.90401C10.144 3.7304 10.0571 3.56117 9.96234 3.39978Z"),
			new IconSource("globe", ModernIconStyle.Regular, 20, FillMode.Winding,
				"M10 18C14.4183 18 18 14.4183 18 10C18 5.58172 14.4183 2 10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18ZM10 3C10.6568 3 11.4068 3.59025 12.0218 4.90814C12.2393 5.37419 12.4283 5.90978 12.5806 6.5H7.41936C7.57172 5.90978 7.76073 5.37419 7.97822 4.90814C8.59323 3.59025 9.34315 3 10 3ZM7.07203 4.48526C6.79564 5.07753 6.56498 5.75696 6.38931 6.5H3.93648C4.77295 5.05399 6.11182 3.93497 7.71442 3.38163C7.47297 3.71222 7.25828 4.08617 7.07203 4.48526ZM6.19265 7.5C6.06723 8.28832 6 9.12934 6 10C6 10.8707 6.06723 11.7117 6.19265 12.5H3.45963C3.16268 11.7236 3 10.8808 3 10C3 9.1192 3.16268 8.2764 3.45963 7.5H6.19265ZM6.38931 13.5C6.56498 14.243 6.79564 14.9225 7.07203 15.5147C7.25828 15.9138 7.47297 16.2878 7.71442 16.6184C6.11182 16.065 4.77295 14.946 3.93648 13.5H6.38931ZM7.41936 13.5H12.5806C12.4283 14.0902 12.2393 14.6258 12.0218 15.0919C11.4068 16.4097 10.6568 17 10 17C9.34315 17 8.59323 16.4097 7.97822 15.0919C7.76073 14.6258 7.57172 14.0902 7.41936 13.5ZM12.7938 12.5H7.20617C7.07345 11.7253 7 10.8833 7 10C7 9.11669 7.07345 8.27472 7.20617 7.5H12.7938C12.9266 8.27472 13 9.11669 13 10C13 10.8833 12.9266 11.7253 12.7938 12.5ZM13.6107 13.5H16.0635C15.2271 14.946 13.8882 16.065 12.2856 16.6184C12.527 16.2878 12.7417 15.9138 12.928 15.5147C13.2044 14.9225 13.435 14.243 13.6107 13.5ZM16.5404 12.5H13.8074C13.9328 11.7117 14 10.8707 14 10C14 9.12934 13.9328 8.28832 13.8074 7.5H16.5404C16.8373 8.2764 17 9.1192 17 10C17 10.8808 16.8373 11.7236 16.5404 12.5ZM12.2856 3.38163C13.8882 3.93497 15.2271 5.05399 16.0635 6.5H13.6107C13.435 5.75696 13.2044 5.07753 12.928 4.48526C12.7417 4.08617 12.527 3.71222 12.2856 3.38163Z"),
			new IconSource("globe", ModernIconStyle.Regular, 24, FillMode.Winding,
				"M12.0001 1.99805C17.5238 1.99805 22.0016 6.47589 22.0016 11.9996C22.0016 17.5233 17.5238 22.0011 12.0001 22.0011C6.47638 22.0011 1.99854 17.5233 1.99854 11.9996C1.99854 6.47589 6.47638 1.99805 12.0001 1.99805ZM14.939 16.4993H9.06118C9.71322 18.9135 10.8466 20.5011 12.0001 20.5011C13.1536 20.5011 14.2869 18.9135 14.939 16.4993ZM7.5084 16.4999L4.78591 16.4998C5.74425 18.0328 7.1777 19.2384 8.88008 19.9104C8.3578 19.0906 7.92681 18.0643 7.60981 16.8949L7.5084 16.4999ZM19.2143 16.4998L16.4918 16.4999C16.168 17.8337 15.7004 18.9995 15.119 19.9104C16.716 19.2804 18.0757 18.1814 19.0291 16.7833L19.2143 16.4998ZM7.09351 9.99895H3.7359L3.73115 10.0162C3.57906 10.6525 3.49854 11.3166 3.49854 11.9996C3.49854 13.0558 3.69112 14.0669 4.0431 14.9999L7.21626 14.9995C7.07396 14.0504 6.99854 13.0422 6.99854 11.9996C6.99854 11.3156 7.031 10.6464 7.09351 9.99895ZM15.397 9.99901H8.60316C8.53514 10.6393 8.49853 11.309 8.49853 11.9996C8.49853 13.0591 8.58468 14.0694 8.73827 14.9997H15.2619C15.4155 14.0694 15.5016 13.0591 15.5016 11.9996C15.5016 11.309 15.465 10.6393 15.397 9.99901ZM20.2647 9.99811L16.9067 9.99897C16.9692 10.6464 17.0016 11.3156 17.0016 11.9996C17.0016 13.0422 16.9262 14.0504 16.7839 14.9995L19.9571 14.9999C20.309 14.0669 20.5016 13.0558 20.5016 11.9996C20.5016 11.3102 20.4196 10.64 20.2647 9.99811ZM8.88114 4.08875L8.85823 4.09747C6.81092 4.91218 5.1549 6.49949 4.25023 8.49935L7.29835 8.49972C7.61171 6.74693 8.15855 5.221 8.88114 4.08875ZM12.0001 3.49805L11.8844 3.50335C10.619 3.6191 9.39651 5.62107 8.8288 8.4993H15.1714C14.6052 5.62914 13.388 3.63033 12.1264 3.50436L12.0001 3.49805ZM15.1201 4.08881L15.2269 4.2629C15.8961 5.37537 16.4043 6.83525 16.7018 8.49972L19.7499 8.49935C18.8853 6.58795 17.3343 5.05341 15.4113 4.21008L15.1201 4.08881Z"),
			new IconSource("music_note_2", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M12 5.20951V10.2675C11.7058 10.0974 11.3643 9.99998 11 9.99998C9.89543 9.99998 9 10.8954 9 12C9 13.1045 9.89543 14 11 14C12.1046 14 13 13.1045 13 12V1.92568C13 1.37245 12.4519 0.986213 11.9309 1.17228L5.53093 3.458C5.21254 3.57171 5 3.8733 5 4.21139V11.2675C4.70583 11.0974 4.36429 11 4 11C2.89543 11 2 11.8954 2 13C2 14.1045 2.89543 15 4 15C5.10457 15 6 14.1045 6 13V7.35237L12 5.20951Z"),
			new IconSource("music_note_2", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M15.9867 3.01576C15.8908 2.43018 15.2919 2.04148 14.7017 2.2259L7.70173 4.4134C7.28425 4.54387 7 4.9305 7 5.36788V13.5001C6.58217 13.1863 6.0628 13.0003 5.5 13.0003C4.11929 13.0003 3 14.1196 3 15.5003C3 16.881 4.11929 18.0003 5.5 18.0003C6.88071 18.0003 8 16.881 8 15.5003V8.36793L15 6.18043V11.5001C14.5822 11.1863 14.0628 11.0003 13.5 11.0003C12.1193 11.0003 11 12.1196 11 13.5003C11 14.881 12.1193 16.0003 13.5 16.0003C14.8807 16.0003 16 14.881 16 13.5003V3.18043C16 3.18044 16 3.18041 16 3.18043C16 3.12411 15.9954 3.06909 15.9867 3.01576Z"),
			new IconSource("music_note_2", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M20 2.75001C20 2.51293 19.8879 2.28981 19.6977 2.14829C19.5075 2.00677 19.2616 1.96351 19.0345 2.03164L9.03449 5.03164C8.71725 5.12681 8.5 5.4188 8.5 5.75001V15.6273C7.93308 15.2319 7.24362 15 6.5 15C4.567 15 3 16.567 3 18.5C3 20.433 4.567 22 6.5 22C8.433 22 10 20.433 10 18.5C10 18.4426 9.99862 18.3856 9.99589 18.3289C9.99861 18.303 10 18.2766 10 18.25V10.308L18.5 7.75803V13.6273C17.9331 13.2319 17.2436 13 16.5 13C14.567 13 13 14.567 13 16.5C13 18.433 14.567 20 16.5 20C18.433 20 20 18.433 20 16.5C20 16.4427 19.9986 16.3856 19.9959 16.329C19.9986 16.303 20 16.2767 20 16.25V2.75001Z"),
			new IconSource("slide_text", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M1 5C1 3.89543 1.89543 3 3 3H13C14.1046 3 15 3.89543 15 5V11C15 12.1046 14.1046 13 13 13H3C1.89543 13 1 12.1046 1 11V5ZM4.5 5C4.22386 5 4 5.22386 4 5.5C4 5.77614 4.22386 6 4.5 6H7.5C7.77614 6 8 5.77614 8 5.5C8 5.22386 7.77614 5 7.5 5H4.5ZM4 7.5C4 7.77614 4.22386 8 4.5 8H10.5C10.7761 8 11 7.77614 11 7.5C11 7.22386 10.7761 7 10.5 7H4.5C4.22386 7 4 7.22386 4 7.5ZM4.5 9C4.22386 9 4 9.22386 4 9.5C4 9.77614 4.22386 10 4.5 10H8.5C8.77614 10 9 9.77614 9 9.5C9 9.22386 8.77614 9 8.5 9H4.5Z"),
			new IconSource("slide_text", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M2 6.5C2 5.11929 3.11929 4 4.5 4H15.5C16.8807 4 18 5.11929 18 6.5V13.5C18 14.8807 16.8807 16 15.5 16H4.5C3.11929 16 2 14.8807 2 13.5V6.5ZM5.5 7C5.22386 7 5 7.22386 5 7.5C5 7.77614 5.22386 8 5.5 8H9.5C9.77614 8 10 7.77614 10 7.5C10 7.22386 9.77614 7 9.5 7H5.5ZM5 10C5 10.2761 5.22386 10.5 5.5 10.5H12.5C12.7761 10.5 13 10.2761 13 10C13 9.72386 12.7761 9.5 12.5 9.5H5.5C5.22386 9.5 5 9.72386 5 10ZM5.5 12C5.22386 12 5 12.2239 5 12.5C5 12.7761 5.22386 13 5.5 13H10.5C10.7761 13 11 12.7761 11 12.5C11 12.2239 10.7761 12 10.5 12H5.5Z"),
			new IconSource("slide_text", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M2 6.75C2 5.23122 3.23122 4 4.75 4H19.25C20.7688 4 22 5.23122 22 6.75V17.25C22 18.7688 20.7688 20 19.25 20H4.75C3.23122 20 2 18.7688 2 17.25V6.75ZM6.75 8C6.33579 8 6 8.33579 6 8.75C6 9.16421 6.33579 9.5 6.75 9.5H11.25C11.6642 9.5 12 9.16421 12 8.75C12 8.33579 11.6642 8 11.25 8H6.75ZM6.75 11C6.33579 11 6 11.3358 6 11.75C6 12.1642 6.33579 12.5 6.75 12.5H15.25C15.6642 12.5 16 12.1642 16 11.75C16 11.3358 15.6642 11 15.25 11H6.75ZM6 14.75C6 15.1642 6.33579 15.5 6.75 15.5H13.25C13.6642 15.5 14 15.1642 14 14.75C14 14.3358 13.6642 14 13.25 14H6.75C6.33579 14 6 14.3358 6 14.75Z"),
			new IconSource("slide_text", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M2.00391 7.75C2.00391 5.67893 3.68284 4 5.75391 4H22.2505C24.3216 4 26.0005 5.67893 26.0005 7.75V20.2501C26.0005 22.3211 24.3216 24.0001 22.2505 24.0001H5.75391C3.68284 24.0001 2.00391 22.3211 2.00391 20.2501V7.75ZM7.7277 9C7.32567 9 6.99976 9.32591 6.99976 9.72794V9.77206C6.99976 10.1741 7.32567 10.5 7.7277 10.5H13.2718C13.6738 10.5 13.9998 10.1741 13.9998 9.77206V9.72794C13.9998 9.32591 13.6738 9 13.2718 9H7.7277ZM6.99976 13.7279V13.7721C6.99976 14.1741 7.32567 14.5 7.7277 14.5H18.2718C18.6738 14.5 18.9998 14.1741 18.9998 13.7721V13.7279C18.9998 13.3259 18.6738 13 18.2718 13H7.7277C7.32567 13 6.99976 13.3259 6.99976 13.7279ZM7.7277 17C7.32567 17 6.99976 17.3259 6.99976 17.7279V17.7721C6.99976 18.1741 7.32567 18.5 7.7277 18.5H15.2718C15.6738 18.5 15.9998 18.1741 15.9998 17.7721V17.7279C15.9998 17.3259 15.6738 17 15.2718 17H7.7277Z"),
			new IconSource("slide_text", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M25.5 4C27.9853 4 30 6.01472 30 8.5V23.5C30 25.9853 27.9853 28 25.5 28H6.5C4.01472 28 2 25.9853 2 23.5V8.5C2 6.01472 4.01472 4 6.5 4H25.5ZM9 20C8.44772 20 8 20.4477 8 21C8 21.5523 8.44772 22 9 22H19C19.5523 22 20 21.5523 20 21C20 20.4477 19.5523 20 19 20H9ZM9 15C8.44772 15 8 15.4477 8 16C8 16.5523 8.44772 17 9 17H22C22.5523 17 23 16.5523 23 16C23 15.4477 22.5523 15 22 15H9ZM9 10C8.44772 10 8 10.4477 8 11C8 11.5523 8.44772 12 9 12H16C16.5523 12 17 11.5523 17 11C17 10.4477 16.5523 10 16 10H9Z"),
			new IconSource("book_open", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M2.75 2C1.7835 2 1 2.7835 1 3.75V12.25C1 13.2165 1.7835 14 2.75 14H6C6.78802 14 7.49862 13.6686 8 13.1375C8.50139 13.6686 9.21198 14 10 14H13.25C14.2165 14 15 13.2165 15 12.25V3.75C15 2.7835 14.2165 2 13.25 2H10C9.21199 2 8.50138 2.33144 8 2.86253C7.49862 2.33144 6.78801 2 6 2H2.75ZM7.25 4.75V11.25C7.25 11.9404 6.69036 12.5 6 12.5H2.75C2.61193 12.5 2.5 12.3881 2.5 12.25V3.75C2.5 3.61193 2.61193 3.5 2.75 3.5H6C6.69036 3.5 7.25 4.05964 7.25 4.75ZM8.75 11.25V4.75C8.75 4.05964 9.30964 3.5 10 3.5H13.25C13.3881 3.5 13.5 3.61193 13.5 3.75V12.25C13.5 12.3881 13.3881 12.5 13.25 12.5H10C9.30964 12.5 8.75 11.9404 8.75 11.25Z"),
			new IconSource("book_open", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M10 16.1375C9.49862 16.6686 8.78802 17 8 17H3.75C2.7835 17 2 16.2165 2 15.25V4.75C2 3.7835 2.7835 3 3.75 3H8C8.78802 3 9.49862 3.33145 10 3.86253C10.5014 3.33145 11.212 3 12 3H16.25C17.2165 3 18 3.7835 18 4.75L18 15.25C18 16.2165 17.2165 17 16.25 17L12 17C11.212 17 10.5014 16.6686 10 16.1375ZM3.5 4.75V15.25C3.5 15.3881 3.61193 15.5 3.75 15.5H8C8.69036 15.5 9.25 14.9404 9.25 14.25V5.75C9.25 5.05964 8.69036 4.5 8 4.5H3.75C3.61193 4.5 3.5 4.61193 3.5 4.75ZM10.75 14.25C10.75 14.9404 11.3096 15.5 12 15.5L16.25 15.5C16.3881 15.5 16.5 15.3881 16.5 15.25L16.5 4.75C16.5 4.61193 16.3881 4.5 16.25 4.5L12 4.5C11.3096 4.5 10.75 5.05964 10.75 5.75V14.25Z"),
			new IconSource("book_open", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M4 4C2.89543 4 2 4.89543 2 6V18C2 19.1046 2.89543 20 4 20H10C10.7684 20 11.4692 19.7111 12 19.2361C12.5308 19.7111 13.2316 20 14 20H20C21.1046 20 22 19.1046 22 18V6C22 4.89543 21.1046 4 20 4H14C13.2316 4 12.5308 4.28885 12 4.7639C11.4692 4.28885 10.7684 4 10 4H4ZM11 7V17C11 17.5523 10.5523 18 10 18H4V6H10C10.5523 6 11 6.44772 11 7ZM13 17V7C13 6.44772 13.4477 6 14 6H20V18H14C13.4477 18 13 17.5523 13 17Z"),
			new IconSource("book_open", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M11.5 4C12.4793 4 13.3647 4.40223 14 5.05051C14.6353 4.40223 15.5207 4 16.5 4H24C25.1046 4 26 4.89543 26 6V22C26 23.1046 25.1046 24 24 24H16.5C15.5207 24 14.6353 23.5978 14 22.9495C13.3647 23.5978 12.4793 24 11.5 24H4C2.89543 24 2 23.1046 2 22V6C2 4.89543 2.89543 4 4 4H11.5ZM13 20.5V7.5C13 6.67157 12.3284 6 11.5 6H4V22H11.5C12.3284 22 13 21.3284 13 20.5ZM15 7.5V20.5C15 21.3284 15.6716 22 16.5 22H24V6H16.5C15.6716 6 15 6.67157 15 7.5Z"),
			new IconSource("book_open", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M4.25 4C3.00736 4 2 5.00736 2 6.25V25.75C2 26.9926 3.00736 28 4.25 28H12.25C13.7743 28 15.1308 27.282 16 26.1658C16.8692 27.282 18.2257 28 19.75 28H27.75C28.9926 28 30 26.9926 30 25.75V6.25C30 5.00736 28.9926 4 27.75 4H19.75C18.2257 4 16.8692 4.71797 16 5.83417C15.1308 4.71797 13.7743 4 12.25 4H4.25ZM4.5 25.5V6.5H12.25C13.4926 6.5 14.5 7.50736 14.5 8.75V23.25C14.5 24.4926 13.4926 25.5 12.25 25.5H4.5ZM17.5 8.75C17.5 7.50736 18.5074 6.5 19.75 6.5H27.5V25.5H19.75C18.5074 25.5 17.5 24.4926 17.5 23.25V8.75Z"),
			new IconSource("text_t", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M2.5 2.75C2.5 2.33579 2.83579 2 3.25 2H12.25C12.6642 2 13 2.33579 13 2.75V4.25C13 4.66421 12.6642 5 12.25 5C11.8358 5 11.5 4.66421 11.5 4.25V3.5H8.49998V12.5H9.74998C10.1642 12.5 10.5 12.8358 10.5 13.25C10.5 13.6642 10.1642 14 9.74998 14H5.74998C5.33576 14 4.99998 13.6642 4.99998 13.25C4.99998 12.8358 5.33576 12.5 5.74998 12.5H6.99998V3.5H4V4.25C4 4.66421 3.66421 5 3.25 5C2.83579 5 2.5 4.66421 2.5 4.25V2.75Z"),
			new IconSource("text_t", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M4 3.75C4 3.33579 4.33579 3 4.75 3H14.75C15.1642 3 15.5 3.33579 15.5 3.75V5.25C15.5 5.66421 15.1642 6 14.75 6C14.3358 6 14 5.66421 14 5.25V4.5H10.5V15.5H11.75C12.1642 15.5 12.5 15.8358 12.5 16.25C12.5 16.6642 12.1642 17 11.75 17H7.75C7.33579 17 7 16.6642 7 16.25C7 15.8358 7.33579 15.5 7.75 15.5H9V4.5H5.5V5.25C5.5 5.66421 5.16421 6 4.75 6C4.33579 6 4 5.66421 4 5.25V3.75Z"),
			new IconSource("text_t", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M4.75 5C4.75 4.44772 5.19772 4 5.75 4H18.25C18.8023 4 19.25 4.44772 19.25 5V7C19.25 7.55228 18.8023 8 18.25 8C17.6977 8 17.25 7.55228 17.25 7V6H13L13 18H14C14.5523 18 15 18.4477 15 19C15 19.5523 14.5523 20 14 20H10C9.44772 20 9 19.5523 9 19C9 18.4477 9.44772 18 10 18H11L11 6H6.75V7C6.75 7.55228 6.30228 8 5.75 8C5.19772 8 4.75 7.55228 4.75 7V5Z"),
			new IconSource("text_t", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M5 5C5 4.44772 5.44772 4 6 4H22C22.5523 4 23 4.44772 23 5V8C23 8.55228 22.5523 9 22 9C21.4477 9 21 8.55228 21 8V6H15V22H16C16.5523 22 17 22.4477 17 23C17 23.5523 16.5523 24 16 24H12C11.4477 24 11 23.5523 11 23C11 22.4477 11.4477 22 12 22H13V6H7V8C7 8.55228 6.55228 9 6 9C5.44772 9 5 8.55228 5 8V5Z"),
			new IconSource("text_t", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M6 5.25C6 4.55964 6.55964 4 7.25 4L24.75 4C25.4404 4 26 4.55964 26 5.25V8.75C26 9.44036 25.4404 10 24.75 10C24.0596 10 23.5 9.44036 23.5 8.75V6.5H17.25V25.5H18.75C19.4404 25.5 20 26.0596 20 26.75C20 27.4404 19.4404 28 18.75 28H13.25C12.5596 28 12 27.4404 12 26.75C12 26.0596 12.5596 25.5 13.25 25.5H14.75V6.5H8.5V8.75C8.5 9.44036 7.94036 10 7.25 10C6.55964 10 6 9.44036 6 8.75V5.25Z"),
			new IconSource("info", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M8 1C11.866 1 15 4.13401 15 8C15 11.866 11.866 15 8 15C4.13401 15 1 11.866 1 8C1 4.13401 4.13401 1 8 1ZM8.00001 6.24907C8.41369 6.24907 8.74905 5.91371 8.74905 5.50003C8.74905 5.08635 8.41369 4.751 8.00001 4.751C7.58633 4.751 7.25098 5.08635 7.25098 5.50003C7.25098 5.91371 7.58633 6.24907 8.00001 6.24907ZM8.5 7.5C8.5 7.22386 8.27614 7 8 7C7.72386 7 7.5 7.22386 7.5 7.5V10.5C7.5 10.7761 7.72386 11 8 11C8.27614 11 8.5 10.7761 8.5 10.5V7.5Z"),
			new IconSource("info", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M18 10C18 5.58172 14.4183 2 10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18C14.4183 18 18 14.4183 18 10ZM9.50806 8.91012C9.55039 8.67687 9.75454 8.49999 10 8.49999C10.2455 8.49999 10.4496 8.67687 10.4919 8.91012L10.5 8.99999V13.5021L10.4919 13.592C10.4496 13.8253 10.2455 14.0021 10 14.0021C9.75454 14.0021 9.55039 13.8253 9.50806 13.592L9.5 13.5021V8.99999L9.50806 8.91012ZM9.25 6.74999C9.25 6.33578 9.58579 5.99999 10 5.99999C10.4142 5.99999 10.75 6.33578 10.75 6.74999C10.75 7.16421 10.4142 7.49999 10 7.49999C9.58579 7.49999 9.25 7.16421 9.25 6.74999Z"),
			new IconSource("info", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M12.0016 1.99902C17.5253 1.99902 22.0031 6.47687 22.0031 12.0006C22.0031 17.5243 17.5253 22.0021 12.0016 22.0021C6.47785 22.0021 2 17.5243 2 12.0006C2 6.47687 6.47785 1.99902 12.0016 1.99902ZM12 10.5C11.5858 10.5 11.25 10.8358 11.25 11.25V16.25C11.25 16.6642 11.5858 17 12 17C12.4142 17 12.75 16.6642 12.75 16.25V11.25C12.75 10.8358 12.4142 10.5 12 10.5ZM12 9C12.5523 9 13 8.55228 13 8C13 7.44772 12.5523 7 12 7C11.4477 7 11 7.44772 11 8C11 8.55228 11.4477 9 12 9Z"),
			new IconSource("info", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M2 14C2 20.6274 7.37258 26 14 26C20.6274 26 26 20.6274 26 14C26 7.37258 20.6274 2 14 2C7.37258 2 2 7.37258 2 14ZM14 11.5C14.4142 11.5 14.75 11.8358 14.75 12.25V19.25C14.75 19.6642 14.4142 20 14 20C13.5858 20 13.25 19.6642 13.25 19.25V12.25C13.25 11.8358 13.5858 11.5 14 11.5ZM14 10C13.4477 10 13 9.55228 13 9C13 8.44772 13.4477 8 14 8C14.5523 8 15 8.44772 15 9C15 9.55228 14.5523 10 14 10Z"),
			new IconSource("info", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M16 2C8.26801 2 2 8.26801 2 16C2 23.732 8.26801 30 16 30C23.732 30 30 23.732 30 16C30 8.26801 23.732 2 16 2ZM16 13C16.5523 13 17 13.4477 17 14V23C17 23.5523 16.5523 24 16 24C15.4477 24 15 23.5523 15 23V14C15 13.4477 15.4477 13 16 13ZM16 11C15.1716 11 14.5 10.3284 14.5 9.5C14.5 8.67157 15.1716 8 16 8C16.8284 8 17.5 8.67157 17.5 9.5C17.5 10.3284 16.8284 11 16 11Z"),
			new IconSource("document_text", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M8 4.5V1H4.5C3.67157 1 3 1.67157 3 2.5V13.5C3 14.3284 3.67157 15 4.5 15H11.5C12.3284 15 13 14.3284 13 13.5V6H9.5C8.67157 6 8 5.32843 8 4.5ZM9 4.5V1.25L12.75 5H9.5C9.22386 5 9 4.77614 9 4.5ZM5.5 8H10.5C10.7761 8 11 8.22386 11 8.5C11 8.77614 10.7761 9 10.5 9H5.5C5.22386 9 5 8.77614 5 8.5C5 8.22386 5.22386 8 5.5 8ZM5 10.5C5 10.2239 5.22386 10 5.5 10H10.5C10.7761 10 11 10.2239 11 10.5C11 10.7761 10.7761 11 10.5 11H5.5C5.22386 11 5 10.7761 5 10.5ZM5.5 12H10.5C10.7761 12 11 12.2239 11 12.5C11 12.7761 10.7761 13 10.5 13H5.5C5.22386 13 5 12.7761 5 12.5C5 12.2239 5.22386 12 5.5 12Z"),
			new IconSource("document_text", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M10 6.5V2H5.5C4.67157 2 4 2.67157 4 3.5V16.5C4 17.3284 4.67157 18 5.5 18H14.5C15.3284 18 16 17.3284 16 16.5V8H11.5C10.6716 8 10 7.32843 10 6.5ZM6.5 10H13.5C13.7761 10 14 10.2239 14 10.5C14 10.7761 13.7761 11 13.5 11H6.5C6.22386 11 6 10.7761 6 10.5C6 10.2239 6.22386 10 6.5 10ZM6.5 12H13.5C13.7761 12 14 12.2239 14 12.5C14 12.7761 13.7761 13 13.5 13H6.5C6.22386 13 6 12.7761 6 12.5C6 12.2239 6.22386 12 6.5 12ZM6.5 14H13.5C13.7761 14 14 14.2239 14 14.5C14 14.7761 13.7761 15 13.5 15H6.5C6.22386 15 6 14.7761 6 14.5C6 14.2239 6.22386 14 6.5 14ZM11 6.5V2.25L15.75 7H11.5C11.2239 7 11 6.77614 11 6.5Z"),
			new IconSource("document_text", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M12 8V2H6C4.89543 2 4 2.89543 4 4V20C4 21.1046 4.89543 22 6 22H18C19.1046 22 20 21.1046 20 20V10H14C12.8954 10 12 9.10457 12 8ZM8.25 11.5H15.75C16.1642 11.5 16.5 11.8358 16.5 12.25C16.5 12.6642 16.1642 13 15.75 13H8.25C7.83579 13 7.5 12.6642 7.5 12.25C7.5 11.8358 7.83579 11.5 8.25 11.5ZM8.25 14.25H15.75C16.1642 14.25 16.5 14.5858 16.5 15C16.5 15.4142 16.1642 15.75 15.75 15.75H8.25C7.83579 15.75 7.5 15.4142 7.5 15C7.5 14.5858 7.83579 14.25 8.25 14.25ZM8.25 17H15.75C16.1642 17 16.5 17.3358 16.5 17.75C16.5 18.1642 16.1642 18.5 15.75 18.5H8.25C7.83579 18.5 7.5 18.1642 7.5 17.75C7.5 17.3358 7.83579 17 8.25 17ZM13.5 8V2.5L19.5 8.5H14C13.7239 8.5 13.5 8.27614 13.5 8Z"),
			new IconSource("document_text", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M14 10V2H6.4C5.07452 2 4 3.07452 4 4.4V23.6C4 24.9255 5.07452 26 6.4 26H21.6C22.9255 26 24 24.9255 24 23.6V12.0784C24 12.0522 23.9996 12.0261 23.9989 12H16C14.8954 12 14 11.1046 14 10ZM9.25 14.5H18.75C19.1642 14.5 19.5 14.8358 19.5 15.25C19.5 15.6642 19.1642 16 18.75 16H9.25C8.83579 16 8.5 15.6642 8.5 15.25C8.5 14.8358 8.83579 14.5 9.25 14.5ZM8.5 18.25C8.5 17.8358 8.83579 17.5 9.25 17.5H18.75C19.1642 17.5 19.5 17.8358 19.5 18.25C19.5 18.6642 19.1642 19 18.75 19H9.25C8.83579 19 8.5 18.6642 8.5 18.25ZM9.25 20.5H18.75C19.1642 20.5 19.5 20.8358 19.5 21.25C19.5 21.6642 19.1642 22 18.75 22H9.25C8.83579 22 8.5 21.6642 8.5 21.25C8.5 20.8358 8.83579 20.5 9.25 20.5ZM15.5 10V2.47509C15.5927 2.54415 15.6809 2.62022 15.7636 2.70294L23.1945 10.1339C23.3082 10.2475 23.4109 10.3701 23.5019 10.5H16C15.7239 10.5 15.5 10.2761 15.5 10Z"),
			new IconSource("document_text", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M17 9V2H8C6.34315 2 5 3.34315 5 5V27C5 28.6569 6.34315 30 8 30H24C25.6569 30 27 28.6569 27 27V12H20C18.3431 12 17 10.6569 17 9ZM11 23H21C21.5523 23 22 23.4477 22 24C22 24.5523 21.5523 25 21 25H11C10.4477 25 10 24.5523 10 24C10 23.4477 10.4477 23 11 23ZM10 20C10 19.4477 10.4477 19 11 19H21C21.5523 19 22 19.4477 22 20C22 20.5523 21.5523 21 21 21H11C10.4477 21 10 20.5523 10 20ZM11 15H21C21.5523 15 22 15.4477 22 16C22 16.5523 21.5523 17 21 17H11C10.4477 17 10 16.5523 10 16C10 15.4477 10.4477 15 11 15ZM19 9V2.11664C19.4843 2.2558 19.9302 2.51602 20.2929 2.87868L26.1213 8.70711C26.484 9.06977 26.7442 9.51572 26.8834 10H20C19.4477 10 19 9.55229 19 9Z"),
			new IconSource("video", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M1 5.5C1 4.11929 2.11929 3 3.5 3H7.5C8.88071 3 10 4.11929 10 5.5V10.5C10 11.8807 8.88071 13 7.5 13H3.5C2.11929 13 1 11.8807 1 10.5V5.5ZM13.0355 11.7782L10.8999 10.3036C10.9129 10.2115 10.9197 10.1173 10.9197 10.0215V5.98047C10.9197 5.88463 10.9129 5.79037 10.8999 5.69813L13.0355 4.22353C13.8647 3.65101 14.9958 4.24453 14.9958 5.25216V10.7496C14.9958 11.7572 13.8647 12.3507 13.0355 11.7782Z"),
			new IconSource("video", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M2 7C2 5.34315 3.34315 4 5 4H10C11.6569 4 13 5.34315 13 7V13C13 14.6569 11.6569 16 10 16H5C3.34315 16 2 14.6569 2 13V7ZM16.0373 14.7759L14 13.3692V6.63025L16.0373 5.22353C16.8665 4.65101 17.9976 5.24453 17.9976 6.25215V13.7473C17.9976 14.7549 16.8665 15.3484 16.0373 14.7759Z"),
			new IconSource("video", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M2 8.25C2 6.45507 3.45507 5 5.25 5H11.75C13.5449 5 15 6.45507 15 8.25V15.75C15 17.5449 13.5449 19 11.75 19H5.25C3.45507 19 2 17.5449 2 15.75V8.25ZM19.2574 17.6882L16 15.439V8.56209L19.2574 6.31295C20.4182 5.51142 22.0017 6.34235 22.0017 7.75302V16.2481C22.0017 17.6588 20.4182 18.4897 19.2574 17.6882Z"),
			new IconSource("video", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M2 9.75C2 7.67893 3.67893 6 5.75 6H14.25C16.3211 6 18 7.67893 18 9.75V18.25C18 20.3211 16.3211 22 14.25 22H5.75C3.67893 22 2 20.3211 2 18.25V9.75ZM23.2516 20.6829L19 17.7472V10.2486L23.2516 7.31295C24.4125 6.51142 25.996 7.34235 25.996 8.75302V19.2428C25.996 20.6535 24.4125 21.4844 23.2516 20.6829Z"),
			new IconSource("video", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M2 10.5C2 8.01472 4.01472 6 6.5 6H16.5C18.9853 6 21 8.01472 21 10.5V21.5C21 23.9853 18.9853 26 16.5 26H6.5C4.01472 26 2 23.9853 2 21.5V10.5ZM29.9959 9.75388C29.9959 7.94016 27.96 6.87182 26.4674 7.90236L22.5 10.6418V21.358L26.4675 24.0974C27.96 25.1279 29.9959 24.0596 29.9959 22.2459V9.75388Z"),
			new IconSource("desktop", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M3.50098 2C2.67255 2 2.00098 2.67157 2.00098 3.5V10.4971C2.00098 11.3256 2.67255 11.9971 3.50098 11.9971H6.00012V13H4.49573C4.21959 13 3.99573 13.2239 3.99573 13.5C3.99573 13.7761 4.21959 14 4.49573 14H11.5001C11.7763 14 12.0001 13.7761 12.0001 13.5C12.0001 13.2239 11.7763 13 11.5001 13H10.0001V11.9971H12.501C13.3294 11.9971 14.001 11.3256 14.001 10.4971V3.5C14.001 2.67157 13.3294 2 12.501 2H3.50098ZM9.00012 11.9971V13H7.00012V11.9971H9.00012Z"),
			new IconSource("desktop", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M3.5 2C2.67157 2 2 2.67157 2 3.5V13.5C2 14.3284 2.67157 15 3.5 15H7V17H5.5C5.22386 17 5 17.2239 5 17.5C5 17.7761 5.22386 18 5.5 18H14.5C14.7761 18 15 17.7761 15 17.5C15 17.2239 14.7761 17 14.5 17H13V15H16.5C17.3284 15 18 14.3284 18 13.5V3.5C18 2.67157 17.3284 2 16.5 2H3.5ZM12 15V17H8V15H12Z"),
			new IconSource("desktop", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M6.75 22.0004C6.33579 22.0004 6 21.6647 6 21.2504C6 20.8707 6.28215 20.557 6.64823 20.5073L6.75 20.5004L8.499 20.5V18.002L4.25 18.0023C3.05914 18.0023 2.08436 17.0771 2.00519 15.9063L2 15.7523V5.25C2 4.05914 2.92516 3.08436 4.09595 3.00519L4.25 3H19.7488C20.9397 3 21.9145 3.92516 21.9936 5.09595L21.9988 5.25V15.7523C21.9988 16.9431 21.0737 17.9179 19.9029 17.9971L19.7488 18.0023L15.499 18.002V20.5L17.25 20.5004C17.6642 20.5004 18 20.8362 18 21.2504C18 21.6301 17.7178 21.9439 17.3518 21.9936L17.25 22.0004H6.75ZM13.998 18.002H9.998L9.999 20.5004H13.999L13.998 18.002Z"),
			new IconSource("desktop", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M23.7488 3C24.9915 3 25.9988 4.00736 25.9988 5.25V18.7523C25.9988 19.9949 24.9915 21.0023 23.7488 21.0023L17.998 21.002V23.5004H19.75C20.1297 23.5004 20.4435 23.7826 20.4932 24.1487L20.5 24.2504C20.5 24.6301 20.2178 24.9439 19.8518 24.9936L19.75 25.0004H8.25C7.83579 25.0004 7.5 24.6647 7.5 24.2504C7.5 23.8707 7.78215 23.557 8.14823 23.5073L8.25 23.5004H10V21.002L4.25 21.0023C3.00736 21.0023 2 19.9949 2 18.7523V5.25C2 4.00736 3.00736 3 4.25 3H23.7488ZM16.498 21.002H11.5V23.5004H16.4988L16.498 21.002Z"),
			new IconSource("desktop", ModernIconStyle.Filled, 32, FillMode.Winding,
				"M2 6.25C2 4.45507 3.45507 3 5.25 3H26.75C28.5449 3 30 4.45507 30 6.25V21.75C30 23.5449 28.5449 25 26.75 25H20.0059V27.0012L23.0042 27.0012C23.5565 27.0012 24.0042 27.4486 24.0042 28.0005C24.0042 28.5525 23.5565 28.9999 23.0043 28.9999L9.01197 29C8.45969 29 8.01197 28.5526 8.01196 28.0007C8.01196 27.4488 8.45967 27.0013 9.01196 27.0013L12.01 27.0013V25H5.25C3.45507 25 2 23.5449 2 21.75V6.25ZM14.01 25V27.0013L18.0059 27.0013V25H14.01Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M12.2929 13L14.1464 14.8536C14.3417 15.0488 14.6583 15.0488 14.8536 14.8536C15.0488 14.6583 15.0488 14.3417 14.8536 14.1464L1.85355 1.14645C1.65829 0.951184 1.34171 0.951184 1.14645 1.14645C0.951184 1.34171 0.951184 1.65829 1.14645 1.85355L2.50063 3.20773C1.61734 3.59337 1 4.47458 1 5.49999V10.5C1 11.8807 2.11929 13 3.5 13H12.2929ZM5.82195 6.52906C5.75415 6.54264 5.68748 6.56531 5.62361 6.59724C5.3366 6.74075 5 7.13521 5 8.00003C5 8.86484 5.33659 9.25929 5.62361 9.4028C5.92799 9.55499 6.29614 9.49677 6.54645 9.24647C6.74171 9.0512 7.05829 9.0512 7.25355 9.24647C7.44882 9.44173 7.44882 9.75831 7.25355 9.95357C6.70386 10.5033 5.87201 10.645 5.17639 10.2972C4.46341 9.94073 4 9.13518 4 8.00003C4 6.92816 4.41317 6.15017 5.0595 5.76661L5.82195 6.52906ZM9.62361 6.59724C9.47258 6.67276 9.30781 6.81777 9.18559 7.06424L14.3274 12.206C14.7446 11.7593 15 11.1595 15 10.5V5.49999C15 4.11927 13.8807 2.99999 12.5 2.99999H5.12134L8.45657 6.33522C8.64788 6.05817 8.89232 5.84485 9.17639 5.70282C9.87201 5.355 10.7039 5.49677 11.2536 6.04647C11.4488 6.24173 11.4488 6.55832 11.2536 6.75358C11.0583 6.94884 10.7417 6.94883 10.5464 6.75357C10.2961 6.50326 9.92799 6.44505 9.62361 6.59724Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M2.85355 2.14645C2.65829 1.95118 2.34171 1.95118 2.14645 2.14645C1.95118 2.34171 1.95118 2.65829 2.14645 2.85355L3.62555 4.33266C2.66007 4.83117 2 5.83849 2 6.99999V13C2 14.6568 3.34315 16 5 16H15C15.0944 16 15.1878 15.9956 15.28 15.9871L17.1464 17.8536C17.3417 18.0488 17.6583 18.0488 17.8536 17.8536C18.0488 17.6583 18.0488 17.3417 17.8536 17.1464L16.3744 15.6673C16.3744 15.6673 16.3744 15.6673 16.3744 15.6673L12.6771 11.97C12.6771 11.97 12.6771 11.97 12.6771 11.97L11.623 10.9159C11.623 10.9159 11.623 10.9159 11.623 10.9159L10.5041 9.79702C10.5041 9.79703 10.5041 9.79701 10.5041 9.79702L8.94172 8.23458C8.94172 8.23457 8.94171 8.23458 8.94172 8.23458L7.76697 7.05986C7.76698 7.05987 7.76696 7.05986 7.76697 7.05986L4.72002 4.01288C4.72001 4.01288 4.72003 4.01288 4.72002 4.01288L2.85355 2.14645ZM7.31063 8.01774C7.15309 8.02263 6.99564 8.06122 6.84861 8.13474C6.43659 8.34075 6 8.88521 6 10C6 11.1148 6.43659 11.6593 6.84861 11.8653C7.27799 12.08 7.79614 11.9968 8.14645 11.6465C8.34171 11.4512 8.65829 11.4512 8.85355 11.6465C9.04882 11.8417 9.04882 12.1583 8.85355 12.3536C8.20386 13.0033 7.22201 13.17 6.40139 12.7597C5.56341 12.3407 5 11.3852 5 10C5 8.61487 5.56341 7.65932 6.40139 7.24031C6.43105 7.22548 6.46092 7.21141 6.49097 7.19808L7.31063 8.01774ZM10.7299 8.60851L6.12134 3.99999H15C16.6569 3.99999 18 5.34313 18 6.99999V13C18 13.7976 17.6888 14.5224 17.1811 15.0598L11.5444 9.42305C11.6656 8.68993 12.0151 8.30148 12.3486 8.13474C12.778 7.92004 13.2961 8.00325 13.6464 8.35356C13.8417 8.54883 14.1583 8.54883 14.3535 8.35357C14.5488 8.15831 14.5488 7.84173 14.3536 7.64646C13.7039 6.99676 12.722 6.82999 11.9014 7.24031C11.383 7.4995 10.9697 7.96401 10.7299 8.60851Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M3.28034 2.21967C2.98745 1.92678 2.51257 1.92677 2.21968 2.21966C1.92678 2.51255 1.92677 2.98742 2.21966 3.28032L3.4698 4.53047C2.5846 5.11109 2 6.11226 2 7.24997V16.7545C2 18.5494 3.45507 20.0045 5.25 20.0045H18.75C18.8132 20.0045 18.8759 20.0027 18.9382 19.9991L20.7194 21.7803C21.0123 22.0732 21.4872 22.0732 21.7801 21.7803C22.073 21.4874 22.073 21.0126 21.7801 20.7197L3.28034 2.21967ZM8.57296 9.63369C7.69021 9.7809 7 10.6301 7 11.9999C7 14.0045 8.48411 14.8961 9.8792 14.1026C10.2392 13.8978 10.6971 14.0237 10.9019 14.3837C11.1067 14.7438 10.9809 15.2017 10.6208 15.4065C8.21539 16.7746 5.5 15.1432 5.5 11.9999C5.5 10.2995 6.29288 9.04205 7.40186 8.46258L8.57296 9.63369ZM17.3784 9.90141C16.1366 9.19303 14.8292 9.81615 14.5531 11.3712L21.5638 18.382C21.8412 17.9034 22 17.3475 22 16.7545V7.24997C22 5.45506 20.5449 3.99999 18.75 3.99999H7.18195L13.3557 10.1739C14.1688 8.28645 16.2384 7.52423 18.1216 8.5985C18.4814 8.80374 18.6067 9.26178 18.4015 9.62157C18.1962 9.98136 17.7382 10.1066 17.3784 9.90141Z"),
			new IconSource("closed_caption_off", ModernIconStyle.Filled, 28, FillMode.Winding,
				"M3.28033 2.21967C2.98744 1.92678 2.51257 1.92678 2.21967 2.21966C1.92678 2.51255 1.92677 2.98742 2.21967 3.28032L3.75117 4.81184C2.68984 5.58453 2 6.83669 2 8.24997V19.7499C2 22.0971 3.90279 23.9999 6.25 23.9999H21.75C22.1148 23.9999 22.4688 23.9539 22.8066 23.8675L24.7194 25.7803C25.0123 26.0732 25.4872 26.0732 25.7801 25.7803C26.073 25.4874 26.073 25.0126 25.7801 24.7197L3.28033 2.21967ZM10.1447 11.2054C9.93389 11.2239 9.72418 11.266 9.52449 11.3344C9.11248 11.4757 8.74535 11.7274 8.47608 12.1242C8.2056 12.5228 8 13.1182 8 13.9999C8 14.8817 8.2056 15.4771 8.47607 15.8757C8.74535 16.2725 9.11248 16.5242 9.52449 16.6655C9.94356 16.8091 10.4068 16.8366 10.8315 16.7736C11.2648 16.7094 11.5985 16.5606 11.7815 16.4143C12.1049 16.1555 12.5769 16.2079 12.8357 16.5314C13.0944 16.8548 13.042 17.3268 12.7185 17.5855C12.2765 17.9392 11.6727 18.1654 11.0513 18.2574C10.4214 18.3508 9.71269 18.3157 9.03801 18.0844C8.35627 17.8506 7.70778 17.4148 7.23486 16.7179C6.76315 16.0228 6.5 15.1182 6.5 13.9999C6.5 12.8817 6.76315 11.9771 7.23486 11.282C7.6766 10.631 8.27154 10.2078 8.90366 9.96438L10.1447 11.2054ZM7.18191 4L15.3056 12.1237C15.4193 11.8153 15.5633 11.5348 15.7349 11.282C16.2078 10.585 16.8563 10.1492 17.538 9.91551C18.2127 9.68419 18.9214 9.64911 19.5513 9.74244C20.1727 9.83449 20.7765 10.0607 21.2185 10.4143C21.542 10.6731 21.5944 11.145 21.3356 11.4685C21.0769 11.7919 20.6049 11.8444 20.2815 11.5856C20.0985 11.4392 19.7648 11.2904 19.3315 11.2262C18.9068 11.1633 18.4436 11.1907 18.0245 11.3344C17.6125 11.4757 17.2454 11.7274 16.9761 12.1242C16.7771 12.4174 16.6133 12.817 16.5405 13.3587L19.8322 16.6504C20.0245 16.5816 20.1774 16.4975 20.2815 16.4143C20.6049 16.1555 21.0769 16.2079 21.3357 16.5314C21.5944 16.8548 21.542 17.3268 21.2185 17.5855C21.1355 17.652 21.0467 17.714 20.9531 17.7714L25.2877 22.106C25.7377 21.4316 26 20.6214 26 19.7499V8.24997C26 5.90277 24.0972 4 21.75 4H7.18191Z"),
			new IconSource("live", ModernIconStyle.Filled, 16, FillMode.Winding,
				"M3.04937 3.04989C3.34222 2.75731 3.81709 2.75728 4.10992 3.04989C4.40281 3.34279 4.40281 3.81852 4.10992 4.11142C1.96233 6.25926 1.96229 9.74094 4.10992 11.8888C4.40273 12.1816 4.40258 12.6564 4.10992 12.9493C3.81703 13.2422 3.34226 13.2422 3.04937 12.9493C0.315877 10.2156 0.31576 5.78351 3.04937 3.04989ZM11.8882 3.04989C12.1811 2.75732 12.656 2.75726 12.9488 3.04989C15.6824 5.78348 15.6822 10.2156 12.9488 12.9493C12.6559 13.2422 12.1811 13.2422 11.8882 12.9493C11.5955 12.6564 11.5954 12.1816 11.8882 11.8888C14.0358 9.74096 14.0357 6.25924 11.8882 4.11142C11.5953 3.81852 11.5953 3.34279 11.8882 3.04989ZM5.17437 5.17196C5.46726 4.87907 5.94202 4.87907 6.23492 5.17196C6.52751 5.46488 6.52771 5.93972 6.23492 6.23251C5.25862 7.20882 5.25861 8.79136 6.23492 9.76767C6.52768 10.0605 6.52756 10.5353 6.23492 10.8282C5.94204 11.1211 5.46727 11.1211 5.17437 10.8282C3.61228 9.26612 3.61229 6.73406 5.17437 5.17196ZM9.77007 5.17196C10.063 4.87907 10.5377 4.87907 10.8306 5.17196C12.3927 6.73407 12.3927 9.26613 10.8306 10.8282C10.5377 11.1211 10.063 11.1211 9.77007 10.8282C9.47738 10.5353 9.47726 10.0605 9.77007 9.76767C10.7464 8.79137 10.7463 7.20883 9.77007 6.23251C9.47727 5.9397 9.47744 5.46488 9.77007 5.17196ZM7.99957 7.00009C8.55185 7.00009 8.99957 7.4478 8.99957 8.00009C8.99953 8.55234 8.55183 9.00009 7.99957 9.00009C7.44751 8.99985 6.9996 8.5522 6.99957 8.00009C6.99957 7.44795 7.44749 7.00033 7.99957 7.00009Z"),
			new IconSource("live", ModernIconStyle.Filled, 20, FillMode.Winding,
				"M4.34239 4.34305C4.63527 4.05028 5.11007 4.05024 5.40294 4.34305C5.69579 4.63591 5.69572 5.11069 5.40294 5.4036C2.86456 7.94197 2.86464 12.0576 5.40294 14.596C5.69577 14.8888 5.69566 15.3636 5.40294 15.6565C5.11004 15.9494 4.63528 15.9494 4.34239 15.6565C1.2183 12.5323 1.21823 7.46721 4.34239 4.34305ZM14.5953 4.34305C14.8882 4.05024 15.363 4.05028 15.6559 4.34305C18.78 7.46721 18.78 12.5323 15.6559 15.6565C15.363 15.9494 14.8882 15.9494 14.5953 15.6565C14.3026 15.3636 14.3025 14.8888 14.5953 14.596C17.1336 12.0576 17.1337 7.94197 14.5953 5.4036C14.3025 5.11069 14.3025 4.63591 14.5953 4.34305ZM6.46348 6.46414C6.75639 6.1716 7.23124 6.17141 7.52403 6.46414C7.81681 6.75693 7.81659 7.23177 7.52403 7.52469C6.1572 8.89153 6.1572 11.1081 7.52403 12.4749C7.81675 12.7677 7.81672 13.2426 7.52403 13.5354C7.23118 13.8283 6.75639 13.8282 6.46348 13.5354C4.51086 11.5828 4.51086 8.41676 6.46348 6.46414ZM12.4732 6.46414C12.766 6.17139 13.2409 6.17163 13.5338 6.46414C15.4864 8.41676 15.4864 11.5828 13.5338 13.5354C13.2409 13.8282 12.7661 13.8283 12.4732 13.5354C12.1806 13.2426 12.1806 12.7677 12.4732 12.4749C13.8401 11.1081 13.8401 8.89153 12.4732 7.52469C12.1807 7.23177 12.1805 6.75691 12.4732 6.46414ZM9.99962 8.75028C10.69 8.75028 11.2496 9.30992 11.2496 10.0003C11.2495 10.6905 10.6899 11.2503 9.99962 11.2503C9.30952 11.2501 8.74976 10.6904 8.74962 10.0003C8.74962 9.31004 9.30943 8.75048 9.99962 8.75028Z"),
			new IconSource("live", ModernIconStyle.Filled, 24, FillMode.Winding,
				"M4.93067 4.92882C5.3212 4.53837 5.95423 4.53832 6.34473 4.92882C6.73523 5.31932 6.73518 5.95235 6.34473 6.34288C3.2206 9.46701 3.22073 14.5322 6.34473 17.6564C6.73526 18.0469 6.73526 18.6809 6.34473 19.0714C5.95429 19.4616 5.32115 19.4616 4.93067 19.0714C1.02543 15.1662 1.02543 8.83406 4.93067 4.92882ZM17.6582 4.92882C18.0487 4.53836 18.6817 4.53849 19.0723 4.92882C22.9775 8.83406 22.9775 15.1662 19.0723 19.0714C18.6818 19.4615 18.0486 19.4617 17.6582 19.0714C17.2677 18.6809 17.2677 18.0469 17.6582 17.6564C20.7823 14.5322 20.7824 9.46705 17.6582 6.34288C17.2679 5.95238 17.2679 5.31929 17.6582 4.92882ZM7.7588 7.75695C8.14935 7.3668 8.78246 7.36655 9.17286 7.75695C9.56326 8.14734 9.56301 8.78045 9.17286 9.17101C7.61076 10.7331 7.61076 13.2661 9.17286 14.8282C9.56318 15.2187 9.56308 15.8518 9.17286 16.2423C8.78237 16.6328 8.14933 16.6327 7.7588 16.2423C5.41565 13.8992 5.41565 10.1001 7.7588 7.75695ZM14.8291 7.75695C15.2195 7.36655 15.8526 7.3668 16.2432 7.75695C18.5863 10.1001 18.5863 13.8992 16.2432 16.2423C15.8526 16.6327 15.2196 16.6328 14.8291 16.2423C14.4389 15.8518 14.4388 15.2187 14.8291 14.8282C16.3912 13.2661 16.3912 10.7331 14.8291 9.17101C14.439 8.78045 14.4387 8.14734 14.8291 7.75695ZM12 10.5001C12.8284 10.5001 13.5 11.1717 13.5 12.0001C13.4999 12.8284 12.8284 13.5001 12 13.5001C11.1718 13.5 10.5001 12.8284 10.5 12.0001C10.5 11.1718 11.1717 10.5002 12 10.5001Z"),
		};
		// END GENERATED ICON DATA

		// ---------------------------------------------------------------------------------------
		// Own drawings (not from Fluent UI System Icons).
		// Original drawings made for EasiSlides in the same style as the Fluent "book" family
		// (stroke 1 px at 16 and 20, 1.5 px at 24; outer edges on whole pixels), drawn on each
		// grid separately. They are not copied from any icon set, so the MIT notice above does
		// not apply to them.
		//   bible: closed book with a cross on the cover (owner's choice, 2026-11-17, design D19).
		//   verse_card: rounded card with a small cross and text lines, the database item in a "Word" folder
		//     (owner's choice, design D25).
		//   badge_square, letter_p, letter_w: the PowerPoint and Word-document list icons (D25) are two
		//     layers, a coloured square and a white plain capital letter on it, composed by the caller with
		//     RenderComposite. Plain geometric letters on a square; no Microsoft logo geometry.
		//   Format toolbars (task 9.6, 24 px): heading, heading_first, heading_off (title headings: an H over
		//     text lines, with a 1, crossed out); heading_align_left / _center / _right / _r1 / _r2; region_1,
		//     region_2, region_both (the screen with the region number); text_shadow (a T with a dotted shadow);
		//     interlace; chords (chord letters over text lines); capo_zero (an arrow into a 0); transpose_down /
		//     _up (a note and an arrow); image_tile (four small pictures); italic_off (an upright serif I);
		//     italic_chorus (an italic I with a small c); copyright (a C in a circle).
		// Every part is a simple shape or an outer/inner ring pair, so evenodd is exact.
		// Parts that overlap are separate path strings (each is filled on its own), never subpaths of one string.
		// ---------------------------------------------------------------------------------------
		private static readonly IconSource[] OwnSources =
		{
			new IconSource("bible", ModernIconStyle.Regular, 16, FillMode.Alternate,
				"M5 1H12A2 2 0 0 1 14 3V13A2 2 0 0 1 12 15H5A2 2 0 0 1 3 13V3A2 2 0 0 1 5 1ZM5 2H12A1 1 0 0 1 13 3V13A1 1 0 0 1 12 14H5A1 1 0 0 1 4 13V3A1 1 0 0 1 5 2Z",
				"M4 11H13V12H4Z",
				"M8 3H9V5H11V6H9V10H8V6H6V5H8Z"),
			new IconSource("bible", ModernIconStyle.Regular, 20, FillMode.Alternate,
				"M6 2H15A2 2 0 0 1 17 4V16A2 2 0 0 1 15 18H6A2 2 0 0 1 4 16V4A2 2 0 0 1 6 2ZM6 3H15A1 1 0 0 1 16 4V16A1 1 0 0 1 15 17H6A1 1 0 0 1 5 16V4A1 1 0 0 1 6 3Z",
				"M5 14H16V15H5Z",
				"M10 4H11V6H13V7H11V12H10V7H8V6H10Z"),
			new IconSource("bible", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M6.5 2H17.5A2.5 2.5 0 0 1 20 4.5V19.5A2.5 2.5 0 0 1 17.5 22H6.5A2.5 2.5 0 0 1 4 19.5V4.5A2.5 2.5 0 0 1 6.5 2ZM6.5 3.5H17.5A1 1 0 0 1 18.5 4.5V19.5A1 1 0 0 1 17.5 20.5H6.5A1 1 0 0 1 5.5 19.5V4.5A1 1 0 0 1 6.5 3.5Z",
				"M5.5 17H18.5V18.5H5.5Z",
				"M11 5H13V7H16V9H13V15H11V9H8V7H11Z"),
			new IconSource("bible", ModernIconStyle.Filled, 16, FillMode.Alternate,
				"M5 1H12A2 2 0 0 1 14 3V13A2 2 0 0 1 12 15H5A2 2 0 0 1 3 13V3A2 2 0 0 1 5 1ZM8 3H9V5H11V6H9V10H8V6H6V5H8ZM4 11H13V12H4Z"),
			new IconSource("bible", ModernIconStyle.Filled, 20, FillMode.Alternate,
				"M6 2H15A2 2 0 0 1 17 4V16A2 2 0 0 1 15 18H6A2 2 0 0 1 4 16V4A2 2 0 0 1 6 2ZM10 4H11V6H13V7H11V12H10V7H8V6H10ZM5 14H16V15H5Z"),
			new IconSource("bible", ModernIconStyle.Filled, 24, FillMode.Alternate,
				"M6.5 2H17.5A2.5 2.5 0 0 1 20 4.5V19.5A2.5 2.5 0 0 1 17.5 22H6.5A2.5 2.5 0 0 1 4 19.5V4.5A2.5 2.5 0 0 1 6.5 2ZM11 5H13V7H16V9H13V15H11V9H8V7H11ZM5.5 17H18.5V18.5H5.5Z"),
			new IconSource("verse_card", ModernIconStyle.Regular, 16, FillMode.Alternate,
				"M3 2H13A2 2 0 0 1 15 4V12A2 2 0 0 1 13 14H3A2 2 0 0 1 1 12V4A2 2 0 0 1 3 2ZM3 3H13A1 1 0 0 1 14 4V12A1 1 0 0 1 13 13H3A1 1 0 0 1 2 12V4A1 1 0 0 1 3 3Z",
				"M3 4H4V5H5V6H4V8H3V6H2V5H3Z",
				"M6 5H13V6H6ZM3 8H13V9H3ZM3 10H11V11H3Z"),
			new IconSource("verse_card", ModernIconStyle.Regular, 20, FillMode.Alternate,
				"M4 3H16A2 2 0 0 1 18 5V15A2 2 0 0 1 16 17H4A2 2 0 0 1 2 15V5A2 2 0 0 1 4 3ZM4 4H16A1 1 0 0 1 17 5V15A1 1 0 0 1 16 16H4A1 1 0 0 1 3 15V5A1 1 0 0 1 4 4Z",
				"M5 5H6V7H8V8H6V10H5V8H3V7H5Z",
				"M9 6H15V7H9ZM5 10H15V11H5ZM5 13H12V14H5Z"),
			new IconSource("verse_card", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M5 4H19A3 3 0 0 1 22 7V17A3 3 0 0 1 19 20H5A3 3 0 0 1 2 17V7A3 3 0 0 1 5 4ZM5 5.5H19A1.5 1.5 0 0 1 20.5 7V17A1.5 1.5 0 0 1 19 18.5H5A1.5 1.5 0 0 1 3.5 17V7A1.5 1.5 0 0 1 5 5.5Z",
				"M6 6H7V8H9V9H7V12H6V9H4V8H6Z",
				"M11 7H18V8.5H11ZM6 12H18V13.5H6ZM6 16H14V17.5H6Z"),
			new IconSource("verse_card", ModernIconStyle.Filled, 16, FillMode.Alternate,
				"M3 2H13A2 2 0 0 1 15 4V12A2 2 0 0 1 13 14H3A2 2 0 0 1 1 12V4A2 2 0 0 1 3 2ZM3 4H4V5H5V6H4V8H3V6H2V5H3ZM6 5H13V6H6ZM3 8H13V9H3ZM3 10H11V11H3Z"),
			new IconSource("verse_card", ModernIconStyle.Filled, 20, FillMode.Alternate,
				"M4 3H16A2 2 0 0 1 18 5V15A2 2 0 0 1 16 17H4A2 2 0 0 1 2 15V5A2 2 0 0 1 4 3ZM5 5H6V7H8V8H6V10H5V8H3V7H5ZM9 6H15V7H9ZM5 10H15V11H5ZM5 13H12V14H5Z"),
			new IconSource("verse_card", ModernIconStyle.Filled, 24, FillMode.Alternate,
				"M5 4H19A3 3 0 0 1 22 7V17A3 3 0 0 1 19 20H5A3 3 0 0 1 2 17V7A3 3 0 0 1 5 4ZM6 6H7V8H9V9H7V12H6V9H4V8H6ZM11 7H18V8.5H11ZM6 12H18V13.5H6ZM6 16H14V17.5H6Z"),
			new IconSource("badge_square", ModernIconStyle.Regular, 16, FillMode.Alternate,
				"M3 0.5H13A2.5 2.5 0 0 1 15.5 3V13A2.5 2.5 0 0 1 13 15.5H3A2.5 2.5 0 0 1 0.5 13V3A2.5 2.5 0 0 1 3 0.5Z"),
			new IconSource("letter_p", ModernIconStyle.Regular, 16, FillMode.Alternate,
				"M4.25 3.5H6.875V12.5H4.25Z",
				"M6.875 3.5H9.125A2.625 2.625 0 0 1 11.75 6.125V6.5A2.625 2.625 0 0 1 9.125 9.125H6.875A2.625 2.625 0 0 1 4.25 6.5V6.125A2.625 2.625 0 0 1 6.875 3.5ZM7.625 5.6H8.6A0.75 0.75 0 0 1 9.35 6.35V6.35A0.75 0.75 0 0 1 8.6 7.1H7.625A0.75 0.75 0 0 1 6.875 6.35V6.35A0.75 0.75 0 0 1 7.625 5.6Z"),
			new IconSource("letter_w", ModernIconStyle.Regular, 16, FillMode.Alternate,
				"M1.819 3.939L4.519 12.639L6.381 12.061L3.681 3.361Z",
				"M6.352 12.72L8.902 6.495L7.098 5.755L4.548 11.98Z",
				"M7.098 6.495L9.648 12.72L11.452 11.98L8.902 5.755Z",
				"M11.481 12.639L14.181 3.939L12.319 3.361L9.619 12.061Z"),
			new IconSource("badge_square", ModernIconStyle.Regular, 20, FillMode.Alternate,
				"M4 1H16A3 3 0 0 1 19 4V16A3 3 0 0 1 16 19H4A3 3 0 0 1 1 16V4A3 3 0 0 1 4 1Z"),
			new IconSource("letter_p", ModernIconStyle.Regular, 20, FillMode.Alternate,
				"M5.5 4.6H8.65V15.4H5.5Z",
				"M8.65 4.6H11.35A3.15 3.15 0 0 1 14.5 7.75V8.2A3.15 3.15 0 0 1 11.35 11.35H8.65A3.15 3.15 0 0 1 5.5 8.2V7.75A3.15 3.15 0 0 1 8.65 4.6ZM9.55 7.12H10.72A0.9 0.9 0 0 1 11.62 8.02V8.02A0.9 0.9 0 0 1 10.72 8.92H9.55A0.9 0.9 0 0 1 8.65 8.02V8.02A0.9 0.9 0 0 1 9.55 7.12Z"),
			new IconSource("letter_w", ModernIconStyle.Regular, 20, FillMode.Alternate,
				"M2.583 5.127L5.823 15.567L8.057 14.873L4.817 4.433Z",
				"M8.023 15.664L11.083 8.194L8.917 7.306L5.857 14.776Z",
				"M8.917 8.194L11.977 15.664L14.143 14.776L11.083 7.306Z",
				"M14.177 15.567L17.417 5.127L15.183 4.433L11.943 14.873Z"),
			new IconSource("badge_square", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4.5 1H19.5A3.5 3.5 0 0 1 23 4.5V19.5A3.5 3.5 0 0 1 19.5 23H4.5A3.5 3.5 0 0 1 1 19.5V4.5A3.5 3.5 0 0 1 4.5 1Z"),
			new IconSource("letter_p", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M6.5 5.4H10.35V18.6H6.5Z",
				"M10.35 5.4H13.65A3.85 3.85 0 0 1 17.5 9.25V9.8A3.85 3.85 0 0 1 13.65 13.65H10.35A3.85 3.85 0 0 1 6.5 9.8V9.25A3.85 3.85 0 0 1 10.35 5.4ZM11.45 8.48H12.88A1.1 1.1 0 0 1 13.98 9.58V9.58A1.1 1.1 0 0 1 12.88 10.68H11.45A1.1 1.1 0 0 1 10.35 9.58V9.58A1.1 1.1 0 0 1 11.45 8.48Z"),
			new IconSource("letter_w", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M2.934 6.044L6.894 18.804L9.626 17.956L5.666 5.196Z",
				"M9.583 18.922L13.323 9.792L10.677 8.708L6.937 17.838Z",
				"M10.677 9.792L14.417 18.922L17.063 17.838L13.323 8.708Z",
				"M17.106 18.804L21.066 6.044L18.334 5.196L14.374 17.956Z"),
			// Task 9.6 (format toolbars and their drop-down menus), sheet approved by the owner: 24 px only,
			// because those toolbars show their pictures at 24 px at every scale (design D20).
			new IconSource("heading", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M6 3H8V13H6Z",
				"M16 3H18V13H16Z",
				"M8 7H16V9H8Z",
				"M3 16H21V17.5H3Z",
				"M3 19.5H15V21H3Z"),
			new IconSource("heading_first", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M3 3H5V13H3Z",
				"M12 3H14V13H12Z",
				"M5 7H12V9H5Z",
				"M18.25 3H19.75V13H18.25Z",
				"M18.576 3.131L15.876 4.981L16.724 6.219L19.424 4.369Z",
				"M3 16H21V17.5H3Z",
				"M3 19.5H15V21H3Z"),
			new IconSource("heading_off", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M6 3H8V13H6Z",
				"M16 3H18V13H16Z",
				"M8 7H16V9H8Z",
				"M3 16H21V17.5H3Z",
				"M3 19.5H15V21H3Z",
				"M2.941 3L19.941 22L21.059 21L4.059 2Z"),
			new IconSource("heading_align_left", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M3 2H4.5V10H3Z",
				"M9.5 2H11V10H9.5Z",
				"M4.5 5.25H9.5V6.75H4.5Z",
				"M3 13H21V14.5H3Z",
				"M3 16.5H14V18H3Z",
				"M3 20H18V21.5H3Z"),
			new IconSource("heading_align_center", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M8 2H9.5V10H8Z",
				"M14.5 2H16V10H14.5Z",
				"M9.5 5.25H14.5V6.75H9.5Z",
				"M3 13H21V14.5H3Z",
				"M6.5 16.5H17.5V18H6.5Z",
				"M4.5 20H19.5V21.5H4.5Z"),
			new IconSource("heading_align_right", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M13 2H14.5V10H13Z",
				"M19.5 2H21V10H19.5Z",
				"M14.5 5.25H19.5V6.75H14.5Z",
				"M3 13H21V14.5H3Z",
				"M10 16.5H21V18H10Z",
				"M6 20H21V21.5H6Z"),
			new IconSource("heading_align_r1", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M8 2H9.5V10H8Z",
				"M14.5 2H16V10H14.5Z",
				"M9.5 5.25H14.5V6.75H9.5Z",
				"M5.25 12.5H6.75V21.5H5.25Z",
				"M5.576 12.631L2.876 14.481L3.724 15.719L6.424 13.869Z",
				"M10 13H21V14.5H10Z",
				"M10 16.5H21V18H10Z",
				"M10 20H18V21.5H10Z"),
			new IconSource("heading_align_r2", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M8 2H9.5V10H8Z",
				"M14.5 2H16V10H14.5Z",
				"M9.5 5.25H14.5V6.75H9.5Z",
				"M2.51 15.01A2.75 2.75 0 1 1 7.742 16.412L6.383 15.778A1.25 1.25 0 1 0 4.005 15.141Z",
				"M6.498 15.602L2.76 19.882L3.89 20.868L7.628 16.589Z",
				"M2.5 20H8V21.5H2.5Z",
				"M10 13H21V14.5H10Z",
				"M10 16.5H21V18H10Z",
				"M10 20H18V21.5H10Z"),
			new IconSource("region_1", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4.5 3.5H19.5A2.5 2.5 0 0 1 22 6V18A2.5 2.5 0 0 1 19.5 20.5H4.5A2.5 2.5 0 0 1 2 18V6A2.5 2.5 0 0 1 4.5 3.5ZM4.5 5H19.5A1 1 0 0 1 20.5 6V18A1 1 0 0 1 19.5 19H4.5A1 1 0 0 1 3.5 18V6A1 1 0 0 1 4.5 5Z",
				"M12 7.5H13.5V16.5H12Z",
				"M12.352 7.615L9.402 9.465L10.198 10.735L13.148 8.885Z"),
			new IconSource("region_2", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4.5 3.5H19.5A2.5 2.5 0 0 1 22 6V18A2.5 2.5 0 0 1 19.5 20.5H4.5A2.5 2.5 0 0 1 2 18V6A2.5 2.5 0 0 1 4.5 3.5ZM4.5 5H19.5A1 1 0 0 1 20.5 6V18A1 1 0 0 1 19.5 19H4.5A1 1 0 0 1 3.5 18V6A1 1 0 0 1 4.5 5Z",
				"M9.011 10.239A3 3 0 1 1 14.719 11.768L13.359 11.134A1.5 1.5 0 1 0 10.506 10.369Z",
				"M13.528 10.902L9.314 14.826L10.336 15.924L14.55 12Z",
				"M9 15H15V16.5H9Z"),
			new IconSource("region_both", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4.5 3.5H19.5A2.5 2.5 0 0 1 22 6V18A2.5 2.5 0 0 1 19.5 20.5H4.5A2.5 2.5 0 0 1 2 18V6A2.5 2.5 0 0 1 4.5 3.5ZM4.5 5H19.5A1 1 0 0 1 20.5 6V18A1 1 0 0 1 19.5 19H4.5A1 1 0 0 1 3.5 18V6A1 1 0 0 1 4.5 5Z",
				"M7.5 7.5H9V16.5H7.5Z",
				"M7.852 7.615L4.902 9.465L5.698 10.735L8.648 8.885Z",
				"M13.011 10.239A3 3 0 1 1 18.719 11.768L17.359 11.134A1.5 1.5 0 1 0 14.506 10.369Z",
				"M17.528 10.902L13.314 14.826L14.336 15.924L18.55 12Z",
				"M13 15H19V16.5H13Z"),
			new IconSource("text_shadow", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M3 3H17V6H3Z",
				"M9 6H12V18H9Z",
				"M13 7H14V8H13ZM15 7H16V8H15ZM17 7H18V8H17ZM19 7H20V8H19ZM14 8H15V9H14ZM16 8H17V9H16ZM18 8H19V9H18ZM18 6H19V7H18ZM7 7H8V8H7ZM6 8H7V9H6ZM13 9H14V10H13ZM14 10H15V11H14ZM13 11H14V12H13ZM14 12H15V13H14ZM13 13H14V14H13ZM14 14H15V15H14ZM13 15H14V16H13ZM14 16H15V17H14ZM13 17H14V18H13ZM14 18H15V19H14ZM13 19H14V20H13ZM12 20H13V21H12ZM14 20H15V21H14Z"),
			new IconSource("interlace", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M3 4H17V6H3Z",
				"M7 8.5H21V10.5H7Z",
				"M3 13H17V15H3Z",
				"M7 17.5H21V19.5H7Z"),
			new IconSource("chords", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M10.064 9.571A4 4 0 1 1 10.064 4.429L8.915 5.393A2.5 2.5 0 1 0 8.915 8.607Z",
				"M15.805 2.718L12.555 10.718L13.945 11.282L17.195 3.282Z",
				"M15.805 3.282L19.055 11.282L20.445 10.718L17.195 2.718Z",
				"M14.5 8H18.5V9.25H14.5Z",
				"M3 14.5H21V16H3Z",
				"M3 19H21V20.5H3Z"),
			new IconSource("capo_zero", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M2.5 12.75L11 12.75L11 11.25L2.5 11.25Z",
				"M11.73 11.47L8.03 7.77L6.97 8.83L10.67 12.53Z",
				"M10.67 11.47L6.97 15.17L8.03 16.23L11.73 12.53Z",
				"M17.25 5H17.25A3.75 3.75 0 0 1 21 8.75V15.25A3.75 3.75 0 0 1 17.25 19H17.25A3.75 3.75 0 0 1 13.5 15.25V8.75A3.75 3.75 0 0 1 17.25 5ZM17.25 6.5H17.25A2.25 2.25 0 0 1 19.5 8.75V15.25A2.25 2.25 0 0 1 17.25 17.5H17.25A2.25 2.25 0 0 1 15 15.25V8.75A2.25 2.25 0 0 1 17.25 6.5Z"),
			new IconSource("transpose_down", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4 17.5A3 3 0 1 1 10 17.5A3 3 0 1 1 4 17.5Z",
				"M8.5 4H10V17.5H8.5Z",
				"M8.709 5.12L12.459 9.02L13.541 7.98L9.791 4.08Z",
				"M17.25 4H18.75V18H17.25Z",
				"M18.53 18.47L15.13 15.07L14.07 16.13L17.47 19.53Z",
				"M18.53 19.53L21.93 16.13L20.87 15.07L17.47 18.47Z"),
			new IconSource("transpose_up", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4 17.5A3 3 0 1 1 10 17.5A3 3 0 1 1 4 17.5Z",
				"M8.5 4H10V17.5H8.5Z",
				"M8.709 5.12L12.459 9.02L13.541 7.98L9.791 4.08Z",
				"M17.25 6H18.75V20H17.25Z",
				"M17.47 4.47L14.07 7.87L15.13 8.93L18.53 5.53Z",
				"M17.47 5.53L20.87 8.93L21.93 7.87L18.53 4.47Z"),
			new IconSource("image_tile", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M4 2.5H9.5A1.5 1.5 0 0 1 11 4V9.5A1.5 1.5 0 0 1 9.5 11H4A1.5 1.5 0 0 1 2.5 9.5V4A1.5 1.5 0 0 1 4 2.5ZM4 4H9.5V9.5H4Z",
				"M4.5 9L6.75 6L9 9Z",
				"M14.5 2.5H20A1.5 1.5 0 0 1 21.5 4V9.5A1.5 1.5 0 0 1 20 11H14.5A1.5 1.5 0 0 1 13 9.5V4A1.5 1.5 0 0 1 14.5 2.5ZM14.5 4H20V9.5H14.5Z",
				"M15 9L17.25 6L19.5 9Z",
				"M4 13H9.5A1.5 1.5 0 0 1 11 14.5V20A1.5 1.5 0 0 1 9.5 21.5H4A1.5 1.5 0 0 1 2.5 20V14.5A1.5 1.5 0 0 1 4 13ZM4 14.5H9.5V20H4Z",
				"M4.5 19.5L6.75 16.5L9 19.5Z",
				"M14.5 13H20A1.5 1.5 0 0 1 21.5 14.5V20A1.5 1.5 0 0 1 20 21.5H14.5A1.5 1.5 0 0 1 13 20V14.5A1.5 1.5 0 0 1 14.5 13ZM14.5 14.5H20V20H14.5Z",
				"M15 19.5L17.25 16.5L19.5 19.5Z"),
			new IconSource("italic_off", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M7.5 3H16.5V5H7.5Z",
				"M11 5H13V19H11Z",
				"M7.5 19H16.5V21H7.5Z"),
			new IconSource("italic_chorus", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M7.287 3.697L3.287 19.697L5.713 20.303L9.713 4.303Z",
				"M5 3H12.5V5H5Z",
				"M0.5 19H8V21H0.5Z",
				"M20.564 19.071A4 4 0 1 1 20.564 13.929L19.415 14.893A2.5 2.5 0 1 0 19.415 18.107Z"),
			new IconSource("copyright", ModernIconStyle.Regular, 24, FillMode.Alternate,
				"M12 2.5H12A9.5 9.5 0 0 1 21.5 12V12A9.5 9.5 0 0 1 12 21.5H12A9.5 9.5 0 0 1 2.5 12V12A9.5 9.5 0 0 1 12 2.5ZM12 4H12A8 8 0 0 1 20 12V12A8 8 0 0 1 12 20H12A8 8 0 0 1 4 12V12A8 8 0 0 1 12 4Z",
				"M15.536 15.536A5 5 0 1 1 15.536 8.464L14.475 9.525A3.5 3.5 0 1 0 14.475 14.475Z"),
		};
	}
}
