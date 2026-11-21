using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Axicultus.Properties;
using Axicultus.Util;

namespace Axicultus;

	public partial class FrmGetWorkingFolder : Form
	{

		public FrmGetWorkingFolder()
		{
			InitializeComponent();
		}

		private void FrmGetWorkingFolder_Load(object sender, EventArgs e)
		{
			labelMsg.Text = "The Axicultus Working Folder at " + Gf.RootAxicultusDir + " is missing. Please select one of the following options and click OK.";
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
				if (CreateFolder(Gf.RootAxicultusDir))
				{
					base.DialogResult = DialogResult.OK;
					Close();
				}
				return;
			}
			if (OptionRestoreOriginalDatabase.Checked)
			{
				if (CreateFolder(Gf.RootAxicultusDir))
				{
					Gf.RestoreSongsDatabase = true;
					base.DialogResult = DialogResult.OK;
					Close();
				}
				return;
			}
			string workingDirectory = tbLocation.Text.Trim();
			if (workingDirectory == "")
			{
				MessageBox.Show("Please select a valid folder location.");
				return;
			}
			if (DataUtil.Right(workingDirectory, 1) != "\\")
			{
				workingDirectory += "\\";
			}
			if (Directory.Exists(workingDirectory))
			{
				Gf.RootAxicultusDir = workingDirectory;
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else if (MessageBox.Show("Folder " + workingDirectory + " doesn't exist, do you want Axicultuss to create it?", "Create Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (CreateFolder(workingDirectory))
				{
					Gf.RootAxicultusDir = workingDirectory;
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
			FolderBrowserDialog folderBrowserDialog = new();
			folderBrowserDialog.SelectedPath = "C:\\";
			folderBrowserDialog.Description = "Please select a Folder from below to be the Axicultus Working Folder.";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				tbLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private static bool CreateFolder(string NewLocation)
		{
			if (FileUtil.MakeDir(NewLocation))
			{
				return true;
			}
			MessageBox.Show("Error encountered whilst creating folder: " + Gf.RootAxicultusDir + ". Make sure have write access to the area and try again");
			return false;
		}

		private void tbLocation_TextChanged(object sender, EventArgs e)
		{
			OptionSelectLocation.Checked = true;
		}
	}
