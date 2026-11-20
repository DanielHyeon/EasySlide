using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Axicultus;

	internal unsafe partial class Gf
	{
		public static Color SelectNewColour(Color CurColour)
		{
			Color InColour = CurColour;
			SelectColor(ref InColour);
			return InColour;
		}

		public static bool SelectColorFromBtn(ref Button InBtn, ref Color ColourSymbol)
		{
			Color InColour = InBtn.ForeColor;
			if (SelectColor(ref InColour))
			{
				InBtn.ForeColor = InColour;
				ColourSymbol = InColour;
				return true;
			}
			return false;
		}

		public static bool SelectColorFromBtn(ref ToolStripButton InBtn, ref Color ColourSymbol)
		{
			Color InColour = InBtn.ForeColor;
			if (SelectColor(ref InColour))
			{
				InBtn.ForeColor = InColour;
				ColourSymbol = InColour;
				return true;
			}
			return false;
		}

		public static bool SelectColor(ref Color InColour)
		{
			ColorDialog colorDialog = new();
			colorDialog.Color = InColour;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				InColour = colorDialog.Color;
				return true;
			}
			return false;
		}

		public static bool SelectBackgroundColors(ref ToolStripButton InBtn, ref Color InColour1, ref Color InColour2, ref int InStyle, bool IsDefault)
		{
			ChangedBackColour1 = InColour1;
			ChangedBackColour2 = InColour2;
			ChangedBackStyle = InStyle;
			ChangedIsDefault = IsDefault;
			FrmBackground frmBackground = new();
			if (frmBackground.ShowDialog() == DialogResult.OK)
			{
				InColour1 = ChangedBackColour1;
				InColour2 = ChangedBackColour2;
				InStyle = ChangedBackStyle;
				InBtn.ForeColor = InColour1;
				return true;
			}
			return false;
		}

		private static string[] installedFontNames = [];

		public static void BuildFontsList(ref ToolStripComboBox InTSComboBox)
		{
			if (installedFontNames.Length == 0)
			{
				using var installedFonts = new InstalledFontCollection();
				FontFamily[] families = installedFonts.Families;
				var names = new string[families.Length];
				for (int i = 0; i < families.Length; i++)
				{
					using FontFamily family = families[i];
					names[i] = family.Name;
				}
				installedFontNames = names;
			}
			if (installedFontNames.Length > 0)
			{
				InTSComboBox.Items.Clear();
				InTSComboBox.Sorted = false;
				InTSComboBox.Items.AddRange(installedFontNames);
				InTSComboBox.Sorted = true;
				InTSComboBox.SelectedIndex = 0;
			}
		}

		public static void BuildFontSizeList(ref ToolStripComboBox InCombo)
		{
			InCombo.Items.Clear();
			InCombo.Items.Add("8");
			InCombo.Items.Add("9");
			InCombo.Items.Add("10");
			InCombo.Items.Add("11");
			InCombo.Items.Add("12");
			InCombo.Items.Add("13");
			InCombo.Items.Add("14");
			InCombo.Items.Add("15");
			InCombo.Items.Add("16");
			InCombo.Items.Add("17");
			InCombo.Items.Add("18");
			InCombo.Items.Add("19");
			InCombo.Items.Add("20");
		}

		// The requests ReferenceAlertFont and Alert_UserFont were last made for (GetReusedFont).
		private static string ReferenceAlertFontKey;
		private static string AlertUserFontKey;

		/// <summary>
		/// For a font kept in one Gf field and replaced on every use (ReferenceAlertFont, Alert_UserFont):
		/// returns <paramref name="current"/> when it was made for the same request (name, size, style),
		/// otherwise a new font from GetNewFont, and disposes the one it replaces. Those fields are only read
		/// on the UI thread, for name, size and style, by calls that keep nothing (LoadRef, StartAlert), so
		/// nothing uses the replaced font afterwards. <paramref name="currentKey"/> remembers the request.
		/// </summary>
		internal static Font GetReusedFont(Font current, ref string currentKey, string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline)
		{
			string key = InFontName + "|" + InFontSize + "|" + InBold + "|" + InItalic + "|" + InUnderline;
			if (current != null && key == currentKey)
			{
				return current;
			}
			Font font = GetNewFont(InFontName, InFontSize, InBold, InItalic, InUnderline, ShowErrorMsg: false);
			currentKey = key;
			current?.Dispose();
			return font;
		}

		public static Font GetNewFont(string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline)
		{
			return GetNewFont(InFontName, InFontSize, InBold, InItalic, InUnderline, ShowErrorMsg: true);
		}

		public static Font GetNewFont(string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline, bool ShowErrorMsg)
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (InBold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (InItalic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (InUnderline)
			{
				fontStyle |= FontStyle.Underline;
			}
			try
			{
				return new Font(InFontName, InFontSize, fontStyle);
			}
			catch
			{
				if (ShowErrorMsg)
				{
				}
				return new Font("Microsoft Sans Serif", InFontSize, fontStyle);
			}
		}
	}

	/// <summary>
	/// Single source of the colours used by the modernised main form look
	/// (openspec change modernize-main-form-ui, design D1).
	/// Preview column uses the blue family, Live (output) column the orange family.
	/// Only apply these behind FrmMain.IsModernUiActive (revert switch on and no Windows
	/// high contrast theme); do not check Gf.UseModernMainUi directly.
	/// </summary>
	internal static class ModernUiPalette
	{
		public static readonly Color PreviewAccent = Color.FromArgb(0x00, 0x5F, 0xB8);
		public static readonly Color PreviewTint = Color.FromArgb(0xE5, 0xF1, 0xFB);
		public static readonly Color LiveAccent = Color.FromArgb(0xC4, 0x50, 0x0A);
		public static readonly Color LiveTint = Color.FromArgb(0xFD, 0xED, 0xE3);
		public static readonly Color NextSlide = Color.FromArgb(0x8A, 0x8A, 0x8A);
		public static readonly Color Surface = Color.FromArgb(0xFF, 0xFF, 0xFF);
		public static readonly Color SurfaceAlt = Color.FromArgb(0xF3, 0xF3, 0xF3);
		public static readonly Color TextPrimary = Color.FromArgb(0x1B, 0x1B, 0x1B);
		public static readonly Color TextSecondary = Color.FromArgb(0x5C, 0x5C, 0x5C);
		public static readonly Color OnAccent = Color.FromArgb(0xFF, 0xFF, 0xFF);
		// Text on LiveTint (current live slide that is not on the screen); 5.6:1 on LiveTint.
		public static readonly Color LiveAccentText = Color.FromArgb(0xA3, 0x3F, 0x00);
		// Badge colours of the PowerPoint and Word-document list icons (white letter on this square, design D25).
		public static readonly Color PowerPointBadge = Color.FromArgb(0xC4, 0x3E, 0x1C);
		public static readonly Color WordBadge = Color.FromArgb(0x18, 0x5A, 0xBD);
		// Flat surfaces (design D23): thin boundary lines, hovered and pressed toolbar / menu items.
		public static readonly Color Border = Color.FromArgb(0xD1, 0xD1, 0xD1);
		public static readonly Color Hover = Color.FromArgb(0xE9, 0xE9, 0xE9);
		public static readonly Color Pressed = Color.FromArgb(0xDD, 0xDD, 0xDD);
	}
