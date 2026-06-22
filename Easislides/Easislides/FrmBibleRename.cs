using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmBibleRename : Form
	{

		private string InString = "";

		public FrmBibleRename()
		{
			InitializeComponent();
		}

		private void FrmRename_Load(object sender, EventArgs e)
		{
			InString = DataUtil.Trim(Gf.Rename_String);
			label1.Text = "Rename '" + Gf.Rename_String + "' to:";
			Gf.Rename_ExistingString = Gf.Rename_ExistingString.ToLower();
			textBoxNewString.Text = Gf.Rename_String;
			textBoxNewString.SelectAll();
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			textBoxNewString.Text = DataUtil.Trim(textBoxNewString.Text);
			if (textBoxNewString.Text.Length > 0)
			{
				if (textBoxNewString.Text == Gf.Rename_String)
				{
					Close();
				}
				else if (Gf.Rename_ExistingString.IndexOf(textBoxNewString.Text.ToLower()) < 0)
				{
					Gf.Rename_String = textBoxNewString.Text;
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Name already exists! Please try a different Bible Name.");
				}
			}
			else
			{
				MessageBox.Show("Please enter a new Bible Name.");
			}
		}
	}
}
