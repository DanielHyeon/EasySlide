using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmBackground : Form
	{

		private Color InColour1;

		private Color InColour2;

		private int SelectedStyle;

		private int PicWidth = 80;

		private int PicHeight = 60;

		public FrmBackground()
		{
			InitializeComponent();
			LoadData();
		}

		private void LoadData()
		{
			Text = (Gf.ChangedIsDefault ? "Select Default Background Colours and Pattern" : "Background and Pattern for selected item");
			InColour1 = Gf.ChangedBackColour1;
			InColour2 = Gf.ChangedBackColour2;
			SelectedStyle = Gf.ChangedBackStyle;
			PicWidth = pictureBox0.Width;
			PicHeight = pictureBox0.Height;
			Colour1Btn.ForeColor = InColour1;
			Colour2Btn.ForeColor = InColour2;
			checkBoxColour2.Checked = ((!(InColour1 == InColour2)) ? true : false);
			Colour2Btn.Enabled = checkBoxColour2.Checked;
			SwapBtn.Enabled = Colour2Btn.Enabled;
			DrawAllPatterns();
		}

		private void DrawAllPatterns()
		{
			DrawPattern(pictureBox0, panel0);
			DrawPattern(pictureBox1, panel1);
			DrawPattern(pictureBox2, panel2);
			DrawPattern(pictureBox3, panel3);
			DrawPattern(pictureBox4, panel4);
			DrawPattern(pictureBox5, panel5);
			DrawPattern(pictureBox6, panel6);
			DrawPattern(pictureBox7, panel7);
			DrawPattern(pictureBox8, panel8);
			DrawPattern(pictureBox9, panel9);
			DrawPattern(pictureBox10, panel10);
			DrawPattern(pictureBox11, panel11);
			SetBorder(SelectedStyle);
		}

		private void DrawPattern(PictureBox InPictureBox, Panel InPanel)
		{
			string BackgroundID = "";
			int inStyle = DataUtil.StringToInt((string)InPictureBox.Tag);
			Image image = new Bitmap(PicWidth, PicHeight);
			Graphics g = Graphics.FromImage(image);
			Gf.BackPattern.Fill(ref g, InColour1, InColour2, inStyle, PicWidth, PicHeight, ref BackgroundID);
			InPictureBox.BackgroundImage = image;
			InPanel.BackColor = BackColor;
			//image.Dispose();
			//g.Dispose();
		}

		private void SetBorder(int InStyle)
		{
			panel0.BackColor = ((DataUtil.StringToInt((string)pictureBox0.Tag) == InStyle) ? Color.Red : BackColor);
			panel1.BackColor = ((DataUtil.StringToInt((string)pictureBox1.Tag) == InStyle) ? Color.Red : BackColor);
			panel2.BackColor = ((DataUtil.StringToInt((string)pictureBox2.Tag) == InStyle) ? Color.Red : BackColor);
			panel3.BackColor = ((DataUtil.StringToInt((string)pictureBox3.Tag) == InStyle) ? Color.Red : BackColor);
			panel4.BackColor = ((DataUtil.StringToInt((string)pictureBox4.Tag) == InStyle) ? Color.Red : BackColor);
			panel5.BackColor = ((DataUtil.StringToInt((string)pictureBox5.Tag) == InStyle) ? Color.Red : BackColor);
			panel6.BackColor = ((DataUtil.StringToInt((string)pictureBox6.Tag) == InStyle) ? Color.Red : BackColor);
			panel7.BackColor = ((DataUtil.StringToInt((string)pictureBox7.Tag) == InStyle) ? Color.Red : BackColor);
			panel8.BackColor = ((DataUtil.StringToInt((string)pictureBox8.Tag) == InStyle) ? Color.Red : BackColor);
			panel9.BackColor = ((DataUtil.StringToInt((string)pictureBox9.Tag) == InStyle) ? Color.Red : BackColor);
			panel10.BackColor = ((DataUtil.StringToInt((string)pictureBox10.Tag) == InStyle) ? Color.Red : BackColor);
			panel11.BackColor = ((DataUtil.StringToInt((string)pictureBox11.Tag) == InStyle) ? Color.Red : BackColor);
		}

		private void Colour1_MouseUp(object sender, MouseEventArgs e)
		{
			if (Gf.SelectColorFromBtn(ref Colour1Btn, ref InColour1))
			{
				if (!checkBoxColour2.Checked)
				{
					SetColour2As1();
				}
				DrawAllPatterns();
			}
		}

		private void Colour2_MouseUp(object sender, MouseEventArgs e)
		{
			if (Gf.SelectColorFromBtn(ref Colour2Btn, ref InColour2))
			{
				DrawAllPatterns();
			}
		}

		private void pictureBoxAll_MouseDown(object sender, MouseEventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			SelectedStyle = DataUtil.StringToInt((string)pictureBox.Tag);
			SetBorder(SelectedStyle);
		}

		private void checkBoxColour2_Click(object sender, EventArgs e)
		{
			SetColour2As1();
			DrawAllPatterns();
		}

		private void SetColour2As1()
		{
			Colour2Btn.Enabled = checkBoxColour2.Checked;
			Colour2Btn.ForeColor = Colour1Btn.ForeColor;
			InColour2 = Colour2Btn.ForeColor;
			SwapBtn.Enabled = Colour2Btn.Enabled;
		}

		private void pictureBoxAll_DoubleClick(object sender, EventArgs e)
		{
			UseNewSettingsAndExit();
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			UseNewSettingsAndExit();
		}

		private void UseNewSettingsAndExit()
		{
			Gf.ChangedBackColour1 = InColour1;
			Gf.ChangedBackColour2 = InColour2;
			Gf.ChangedBackStyle = SelectedStyle;
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void SwapBtn_MouseUp(object sender, MouseEventArgs e)
		{
			if (Colour2Btn.Enabled)
			{
				InColour1 = Colour2Btn.ForeColor;
				InColour2 = Colour1Btn.ForeColor;
				Colour1Btn.ForeColor = InColour1;
				Colour2Btn.ForeColor = InColour2;
				DrawAllPatterns();
			}
		}
	}
}
