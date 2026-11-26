using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Axicultus.Localization;
using Axicultus.Properties;
using Axicultus.Util;

namespace Axicultus;

	public partial class FrmGetWorkingFolder : Form
	{

		public FrmGetWorkingFolder()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmGetWorkingFolder_Load(object sender, EventArgs e)
		{
			labelMsg.Text = UiText.Format(nameof(FrmGetWorkingFolder), "FolderMissing", Gf.RootAxicultusDir);
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
				MessageBox.Show(UiText.Get(nameof(FrmGetWorkingFolder), "SelectValidFolder"));
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
			else if (MessageBox.Show(UiText.Format(nameof(FrmGetWorkingFolder), "CreateFolderConfirm", workingDirectory), UiText.Get(nameof(FrmGetWorkingFolder), "CreateFolder"), MessageBoxButtons.YesNo) == DialogResult.Yes)
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
				MessageBox.Show(UiText.Get(nameof(FrmGetWorkingFolder), "FolderNotCreated"));
			}
		}

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new();
			folderBrowserDialog.SelectedPath = "C:\\";
			folderBrowserDialog.Description = UiText.Get(nameof(FrmGetWorkingFolder), "BrowseDescription");
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
			MessageBox.Show(UiText.Format(nameof(FrmGetWorkingFolder), "CreateFolderFailed", Gf.RootAxicultusDir));
			return false;
		}

		private void tbLocation_TextChanged(object sender, EventArgs e)
		{
			OptionSelectLocation.Checked = true;
		}
	}
