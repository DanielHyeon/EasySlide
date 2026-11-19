using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmUpdateFileName : Form
	{

		private string ListText;

		private string InputDir;

		public FrmUpdateFileName()
		{
			InitializeComponent();
		}

		private void frmUpdateFileName_Load(object sender, EventArgs e)
		{
			if (Gf.EasiSlidesMode == UsageMode.Worship)
			{
				InputDir = Gf.WorshipDir;
				ListText = "Worship List";
			}
			else
			{
				InputDir = Gf.PraiseBookDir;
				ListText = "PraiseBook List";
			}
			if (Gf.NameChangeAction == 1)
			{
				Text = "Add New " + ListText;
				Mess.Text = "Please enter title of the new " + ListText;
			}
			else if (Gf.NameChangeAction == 2)
			{
				Text = "Rename " + ListText;
				Mess.Text = "Rename '" + Gf.SelectedListName + "' to:";
				tbFileName.Text = Gf.SelectedListName;
				tbFileName.SelectAll();
			}
			else if (Gf.NameChangeAction == 3)
			{
				Text = "Rename Folder '" + Gf.Folders[Gf.FolderRenameNo].RenameName + "'";
				Mess.Text = "Rename '" + Gf.SelectedListName + "' to:";
				tbFileName.Text = Gf.Folders[Gf.FolderRenameNo].RenameName;
				tbFileName.SelectAll();
			}
			else if (Gf.NameChangeAction == 6)
			{
				Text = "Save InfoScreen as...";
				Mess.Text = "Save as:";
				tbFileName.Text = Gf.Folders[Gf.FolderRenameNo].RenameName;
				tbFileName.SelectAll();
			}
			else if (Gf.NameChangeAction == 7)
			{
				Text = "Copy Session to PraiseBook";
				tbFileName.Text = Gf.GetDisplayNameOnly(ref Gf.SelectedListName, UpdateByRef: false, KeepExt: false);
				tbFileName.SelectAll();
				InputDir = Gf.PraiseBookDir;
				Mess.Text = "Copy '" + tbFileName.Text + "' as:";
			}
			else if (Gf.NameChangeAction == 8)
			{
				Text = "Copy PraiseBook to Session";
				tbFileName.Text = Gf.GetDisplayNameOnly(ref Gf.SelectedListName, UpdateByRef: false, KeepExt: false);
				tbFileName.SelectAll();
				InputDir = Gf.WorshipDir;
				Mess.Text = "Copy '" + tbFileName.Text + "' as:";
			}
		}

		private bool PerformAction()
		{
			string requestedName = tbFileName.Text;
			string listExtension = "";
			if (Gf.NameChangeAction == 1)
			{
				listExtension = ((InputDir == Gf.WorshipDir) ? ".esw" : ".esp");
				if (!Gf.ValidateDirNameFormat(requestedName, ListText + " Title"))
				{
					return false;
				}
				if (File.Exists(InputDir + requestedName + listExtension))
				{
					MessageBox.Show("There is already a list with the same name, please try a different name.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
				Gf.ValidateDir(InputDir, CreateDir: true);
				try
				{
					string inNotes = "";
                    gfFileHelpers.SaveIndexFile(InputDir + requestedName + listExtension, ref Gf.ListViewNotations, UsageMode.Worship, SaveAllItems: false, Gf.Def_FormatString, inNotes);
					Gf.SelectedListName = requestedName;
				}
				catch
				{
					MessageBox.Show("Error encountered whilst create new file - Please ensure you have write access to " + InputDir, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else if (Gf.NameChangeAction == 2)
			{
				listExtension = ((InputDir == Gf.WorshipDir) ? ".esw" : ".esp");
				if (!Gf.ValidateDirNameFormat(requestedName, ListText + " Title"))
				{
					return false;
				}
				if (File.Exists(InputDir + requestedName + listExtension))
				{
					MessageBox.Show("There is already a list with the same name, please try a different name.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
				Gf.ValidateDir(InputDir, CreateDir: true);
				try
				{
					File.Move(InputDir + Gf.SelectedListName + listExtension, InputDir + requestedName + listExtension);
					Gf.SelectedListName = requestedName;
				}
				catch
				{
					MessageBox.Show("Error encountered whilst trying to rename the file - Please ensure you have write access to it.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else
			{
				if (Gf.NameChangeAction == 3)
				{
					_ = Gf.Folders[Gf.FolderRenameNo].RenameName;
					bool folderNameExists = false;
					if (Gf.ValidateDirNameFormat(requestedName, "New folder name"))
					{
						for (int i = 1; i <= Gf.MAXSONGSFOLDERS; i++)
						{
							if (Gf.Folders[i].RenameName.ToLower() == requestedName.ToLower())
							{
								folderNameExists = true;
							}
						}
						if (!folderNameExists)
						{
							Gf.Folders[Gf.FolderRenameNo].RenameName = requestedName;
							return true;
						}
						return false;
					}
					return false;
				}
				if (Gf.NameChangeAction == 7)
				{
					string praiseBookPath = Gf.PraiseBookDir + DataUtil.Trim(tbFileName.Text) + ".esp";
					try
					{
						if (!File.Exists(praiseBookPath))
						{
							File.Copy(Gf.SelectedListName, praiseBookPath, overwrite: true);
							return true;
						}
						if (MessageBox.Show("PraiseBook already exists. Overwrite it?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							File.Copy(Gf.SelectedListName, praiseBookPath, overwrite: true);
							return true;
						}
						return false;
					}
					catch
					{
						MessageBox.Show("Error Saving File, please make sure you have write access and try again");
						return false;
					}
				}
				if (Gf.NameChangeAction == 8)
				{
					string worshipListPath = Gf.WorshipDir + DataUtil.Trim(tbFileName.Text) + ".esw";
					try
					{
						if (!File.Exists(worshipListPath))
						{
							File.Copy(Gf.SelectedListName, worshipListPath, overwrite: true);
							return true;
						}
						if (MessageBox.Show("WorshipList already exists. Overwrite it?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							File.Copy(Gf.SelectedListName, worshipListPath, overwrite: true);
							return true;
						}
						return false;
					}
					catch
					{
						MessageBox.Show("Error Saving File, please make sure you have write access and try again");
						return false;
					}
				}
			}
			return true;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (PerformAction())
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
