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
	internal static partial class ModernIcons
	{
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

		private static readonly Dictionary<CacheKey, Bitmap> cache = new Dictionary<CacheKey, Bitmap>();
		private static readonly HashSet<string> loggedFailures = new HashSet<string>(StringComparer.Ordinal);

		/// <summary>True when path data for this icon and style is present (it may still fail to parse). Never throws.</summary>
		internal static bool Has(string name, ModernIconStyle style)
		{
			try
			{
				return name != null && SourceIndex.Value.ContainsKey((name, style));
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
			var paths = new List<GraphicsPath>(source.PathCount);
			Bitmap bitmap = null;
			try
			{
				for (int index = 0; index < source.PathCount; index++)
				{
					GraphicsPath path = ParsePath(source.GetPath(index), source.Fill, out string error);
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
				double centerNumerator = rx * rx * ry * ry - rx * rx * y1p * y1p - ry * ry * x1p * x1p;
				double den = rx * rx * y1p * y1p + ry * ry * x1p * x1p;
				double coef = den == 0 ? 0 : Math.Sqrt(Math.Max(0, centerNumerator / den));
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
	}
}
