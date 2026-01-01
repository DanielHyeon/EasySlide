using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Easislides
{
	internal unsafe partial class gf
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
			ColorDialog colorDialog = new ColorDialog();
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
			FrmBackground frmBackground = new FrmBackground();
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

		public static void BuildFontsList(ref ToolStripComboBox InTSComboBox)
		{
			if (FontsListMaxIndex < 0)
			{
				using (InstalledFontCollection installedFontCollection = new InstalledFontCollection())
				{
					FontFamily[] families = installedFontCollection.Families;
					FontsListMaxIndex = -1;
					foreach (FontFamily fontFamily in families)
					{
						if (FontsListMaxIndex + 1 >= FontsList.Length)
						{
							break;
						}
						FontsList[++FontsListMaxIndex] = fontFamily.Name;
					}
				}
			}
			if (FontsListMaxIndex >= 0)
			{
				InTSComboBox.Items.Clear();
				InTSComboBox.Sorted = false;
				for (int j = 0; j <= FontsListMaxIndex; j++)
				{
					InTSComboBox.Items.Add(FontsList[j]);
				}
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
}
