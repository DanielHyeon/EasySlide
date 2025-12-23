using Easislides.Module;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmUpdateFileName : Form
	{
		private IContainer components = null;

		private TextBox tbFileName;

		private Label Mess;

		private Button BtnCancel;

		private Button BtnOK;

		private string ListText;

		private string InputDir;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmUpdateFileName));
			tbFileName = new System.Windows.Forms.TextBox();
			Mess = new System.Windows.Forms.Label();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			SuspendLayout();
			tbFileName.Location = new System.Drawing.Point(12, 35);
			tbFileName.Name = "tbFileName";
			tbFileName.Size = new System.Drawing.Size(309, 20);
			tbFileName.TabIndex = 0;
			Mess.Location = new System.Drawing.Point(12, 9);
			Mess.Name = "Mess";
			Mess.Size = new System.Drawing.Size(316, 14);
			Mess.TabIndex = 1;
			Mess.Text = "T";
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(177, 71);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 2;
			BtnCancel.Text = "Cancel";
			BtnOK.Location = new System.Drawing.Point(81, 71);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 1;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(333, 113);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(Mess);
			base.Controls.Add(tbFileName);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmUpdateFileName";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			base.Load += new System.EventHandler(frmUpdateFileName_Load);
			ResumeLayout(false);
			PerformLayout();
		}

		public FrmUpdateFileName()
		{
			InitializeComponent();
		}

		private void frmUpdateFileName_Load(object sender, EventArgs e)
		{
			if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				InputDir = gf.WorshipDir;
				ListText = "Worship List";
			}
			else
			{
				InputDir = gf.PraiseBookDir;
				ListText = "PraiseBook List";
			}
			if (gf.NameChangeAction == 1)
			{
				Text = "Add New " + ListText;
				Mess.Text = "Please enter title of the new " + ListText;
			}
			else if (gf.NameChangeAction == 2)
			{
				Text = "Rename " + ListText;
				Mess.Text = "Rename '" + gf.SelectedListName + "' to:";
				tbFileName.Text = gf.SelectedListName;
				tbFileName.SelectAll();
			}
			else if (gf.NameChangeAction == 3)
			{
				Text = "Rename Folder '" + gf.FolderRenameName[gf.FolderRenameNo] + "'";
				Mess.Text = "Rename '" + gf.SelectedListName + "' to:";
				tbFileName.Text = gf.FolderRenameName[gf.FolderRenameNo];
				tbFileName.SelectAll();
			}
			else if (gf.NameChangeAction == 6)
			{
				Text = "Save InfoScreen as...";
				Mess.Text = "Save as:";
				tbFileName.Text = gf.FolderRenameName[gf.FolderRenameNo];
				tbFileName.SelectAll();
			}
			else if (gf.NameChangeAction == 7)
			{
				Text = "Copy Session to PraiseBook";
				tbFileName.Text = gf.GetDisplayNameOnly(ref gf.SelectedListName, UpdateByRef: false, KeepExt: false);
				tbFileName.SelectAll();
				InputDir = gf.PraiseBookDir;
				Mess.Text = "Copy '" + tbFileName.Text + "' as:";
			}
			else if (gf.NameChangeAction == 8)
			{
				Text = "Copy PraiseBook to Session";
				tbFileName.Text = gf.GetDisplayNameOnly(ref gf.SelectedListName, UpdateByRef: false, KeepExt: false);
				tbFileName.SelectAll();
				InputDir = gf.WorshipDir;
				Mess.Text = "Copy '" + tbFileName.Text + "' as:";
			}
		}

		private bool PerformAction()
		{
			string text = tbFileName.Text;
			string text2 = "";
			if (gf.NameChangeAction == 1)
			{
				text2 = ((InputDir == gf.WorshipDir) ? ".esw" : ".esp");
				if (!gf.ValidateDirNameFormat(text, ListText + " Title"))
				{
					return false;
				}
				if (File.Exists(InputDir + text + text2))
				{
					MessageBox.Show("There is already a list with the same name, please try a different name.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
				gf.ValidateDir(InputDir, CreateDir: true);
				try
				{
					string inNotes = "";
					gf.SaveIndexFile(InputDir + text + text2, ref gf.ListViewNotations, UsageMode.Worship, SaveAllItems: false, gf.Def_FormatString, inNotes);
					gf.SelectedListName = text;
				}
				catch
				{
					MessageBox.Show("Error encountered whilst create new file - Please ensure you have write access to " + InputDir, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else if (gf.NameChangeAction == 2)
			{
				text2 = ((InputDir == gf.WorshipDir) ? ".esw" : ".esp");
				if (!gf.ValidateDirNameFormat(text, ListText + " Title"))
				{
					return false;
				}
				if (File.Exists(InputDir + text + text2))
				{
					MessageBox.Show("There is already a list with the same name, please try a different name.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
				gf.ValidateDir(InputDir, CreateDir: true);
				try
				{
					File.Move(InputDir + gf.SelectedListName + text2, InputDir + text + text2);
					gf.SelectedListName = text;
				}
				catch
				{
					MessageBox.Show("Error encountered whilst trying to rename the file - Please ensure you have write access to it.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else
			{
				if (gf.NameChangeAction == 3)
				{
					string text3 = gf.FolderRenameName[gf.FolderRenameNo];
					string text4 = text;
					bool flag = false;
					if (gf.ValidateDirNameFormat(text, "New folder name"))
					{
						for (int i = 1; i <= 41; i++)
						{
							if (gf.FolderRenameName[i].ToLower() == text.ToLower())
							{
								flag = true;
							}
						}
						if (!flag)
						{
							gf.FolderRenameName[gf.FolderRenameNo] = text;
							return true;
						}
						return false;
					}
					return false;
				}
				if (gf.NameChangeAction == 7)
				{
					string text5 = gf.PraiseBookDir + DataUtil.Trim(tbFileName.Text) + ".esp";
					try
					{
						if (!File.Exists(text5))
						{
							File.Copy(gf.SelectedListName, text5, overwrite: true);
							return true;
						}
						if (MessageBox.Show("PraiseBook already exists. Overwrite it?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							File.Copy(gf.SelectedListName, text5, overwrite: true);
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
				if (gf.NameChangeAction == 8)
				{
					string text5 = gf.WorshipDir + DataUtil.Trim(tbFileName.Text) + ".esw";
					try
					{
						if (!File.Exists(text5))
						{
							File.Copy(gf.SelectedListName, text5, overwrite: true);
							return true;
						}
						if (MessageBox.Show("WorshipList already exists. Overwrite it?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							File.Copy(gf.SelectedListName, text5, overwrite: true);
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
