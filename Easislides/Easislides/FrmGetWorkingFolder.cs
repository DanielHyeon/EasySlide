using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmGetWorkingFolder : Form
	{

		public FrmGetWorkingFolder()
		{
			InitializeComponent();
		}

		private void FrmGetWorkingFolder_Load(object sender, EventArgs e)
		{
			labelMsg.Text = "The EasiSlides Working Folder at " + Gf.RootEasiSlidesDir + " is missing. Please select one of the following options and click OK.";
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (OptionExit.Checked)
			{
				base.DialogResult = DialogResult.Cancel;
				Close();
				return;
			}
			if (OptionNewFolder.Checked)
			{
				if (CreateFolder(Gf.RootEasiSlidesDir))
				{
					base.DialogResult = DialogResult.OK;
					Close();
				}
				return;
			}
			if (OptionRestoreOriginalDatabase.Checked)
			{
				if (CreateFolder(Gf.RootEasiSlidesDir))
				{
					Gf.RestoreSongsDatabase = true;
					base.DialogResult = DialogResult.OK;
					Close();
				}
				return;
			}
			string text = tbLocation.Text.Trim();
			if (text == "")
			{
				MessageBox.Show("Please select a valid folder location.");
				return;
			}
			if (DataUtil.Right(text, 1) != "\\")
			{
				text += "\\";
			}
			if (Directory.Exists(text))
			{
				Gf.RootEasiSlidesDir = text;
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else if (MessageBox.Show("Folder " + text + " doesn't exist, do you want EasiSlides to create it?", "Create Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (CreateFolder(text))
				{
					Gf.RootEasiSlidesDir = text;
					base.DialogResult = DialogResult.OK;
					Close();
				}
			}
			else
			{
				MessageBox.Show("Folder NOT created as instructed - Please select another option.");
			}
		}

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.SelectedPath = "C:\\";
			folderBrowserDialog.Description = "Please select a Folder from below to be the EasiSlides Working Folder.";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				tbLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private bool CreateFolder(string NewLocation)
		{
			if (FileUtil.MakeDir(NewLocation))
			{
				return true;
			}
			MessageBox.Show("Error encountered whilst creating folder: " + Gf.RootEasiSlidesDir + ". Make sure have write access to the area and try again");
			return false;
		}

		private void tbLocation_TextChanged(object sender, EventArgs e)
		{
			OptionSelectLocation.Checked = true;
		}
	}
}
