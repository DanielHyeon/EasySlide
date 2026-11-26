using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Axicultus.Global;
using Axicultus.Localization;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

	public partial class FrmUpdateFileName : Form
	{

		private string ListText;

		private string InputDir;

		public FrmUpdateFileName()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void frmUpdateFileName_Load(object sender, EventArgs e)
		{
			if (Gf.AxicultusMode == UsageMode.Worship)
			{
				InputDir = Gf.WorshipDir;
				ListText = UiText.Get(nameof(FrmUpdateFileName), "WorshipList");
			}
			else
			{
				InputDir = Gf.PraiseBookDir;
				ListText = UiText.Get(nameof(FrmUpdateFileName), "PraiseBookList");
			}
			if (Gf.NameChangeAction == 1)
			{
				Text = UiText.Format(nameof(FrmUpdateFileName), "AddNewTitle", ListText);
				Mess.Text = UiText.Format(nameof(FrmUpdateFileName), "EnterNewTitle", ListText);
			}
			else if (Gf.NameChangeAction == 2)
			{
				Text = UiText.Format(nameof(FrmUpdateFileName), "RenameTitle", ListText);
				Mess.Text = UiText.Format(nameof(FrmUpdateFileName), "RenameTo", Gf.SelectedListName);
				tbFileName.Text = Gf.SelectedListName;
				tbFileName.SelectAll();
			}
			else if (Gf.NameChangeAction == 3)
			{
				Text = UiText.Format(nameof(FrmUpdateFileName), "RenameFolderTitle", Gf.Folders[Gf.FolderRenameNo].RenameName);
				Mess.Text = UiText.Format(nameof(FrmUpdateFileName), "RenameTo", Gf.SelectedListName);
				tbFileName.Text = Gf.Folders[Gf.FolderRenameNo].RenameName;
				tbFileName.SelectAll();
			}
			else if (Gf.NameChangeAction == 6)
			{
				Text = UiText.Get(nameof(FrmUpdateFileName), "SaveInfoScreenTitle");
				Mess.Text = UiText.Get(nameof(FrmUpdateFileName), "SaveAs");
				tbFileName.Text = Gf.Folders[Gf.FolderRenameNo].RenameName;
				tbFileName.SelectAll();
			}
			else if (Gf.NameChangeAction == 7)
			{
				Text = UiText.Get(nameof(FrmUpdateFileName), "CopySessionTitle");
				tbFileName.Text = Gf.GetDisplayNameOnly(ref Gf.SelectedListName, UpdateByRef: false, KeepExt: false);
				tbFileName.SelectAll();
				InputDir = Gf.PraiseBookDir;
				Mess.Text = UiText.Format(nameof(FrmUpdateFileName), "CopyAs", tbFileName.Text);
			}
			else if (Gf.NameChangeAction == 8)
			{
				Text = UiText.Get(nameof(FrmUpdateFileName), "CopyPraiseBookTitle");
				tbFileName.Text = Gf.GetDisplayNameOnly(ref Gf.SelectedListName, UpdateByRef: false, KeepExt: false);
				tbFileName.SelectAll();
				InputDir = Gf.WorshipDir;
				Mess.Text = UiText.Format(nameof(FrmUpdateFileName), "CopyAs", tbFileName.Text);
			}
		}

		internal static bool TryRenameFolder(FolderSettings[] folders, int selected, string requestedName)
		{
			if (selected < 1 || selected >= folders.Length || folders[selected] is null || string.IsNullOrWhiteSpace(requestedName)) return false;
			for (int i = 1; i < folders.Length; i++)
			{
				if (i != selected && string.Equals(folders[i]?.RenameName, requestedName, StringComparison.OrdinalIgnoreCase))
					return false;
			}
			folders[selected].RenameName = requestedName;
			return true;
		}

		private bool PerformAction()
		{
			string requestedName = tbFileName.Text;
			string listExtension = "";
			if (Gf.NameChangeAction == 1)
			{
				listExtension = ".json";
				if (!Gf.ValidateDirNameFormat(requestedName, UiText.Format(nameof(FrmUpdateFileName), "ListTitleHeading", ListText)))
				{
					return false;
				}
				if (File.Exists(InputDir + requestedName + listExtension))
				{
					MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "NameExists"), "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
					MessageBox.Show(UiText.Format(nameof(FrmUpdateFileName), "CreateFailed", InputDir), "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else if (Gf.NameChangeAction == 2)
			{
				listExtension = ".json";
				if (!Gf.ValidateDirNameFormat(requestedName, UiText.Format(nameof(FrmUpdateFileName), "ListTitleHeading", ListText)))
				{
					return false;
				}
				if (File.Exists(InputDir + requestedName + listExtension))
				{
					MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "NameExists"), "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
					MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "RenameFailed"), "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else
			{
				if (Gf.NameChangeAction == 3)
				{
					return Gf.ValidateDirNameFormat(requestedName, UiText.Get(nameof(FrmUpdateFileName), "NewFolderName"))
						&& TryRenameFolder(Gf.Folders, Gf.FolderRenameNo, requestedName);
				}
				if (Gf.NameChangeAction == 7)
				{
					string praiseBookPath = Gf.PraiseBookDir + DataUtil.Trim(tbFileName.Text) + ".json";
					try
					{
						if (!File.Exists(praiseBookPath))
						{
							File.Copy(Gf.SelectedListName, praiseBookPath, overwrite: true);
							return true;
						}
						if (MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "PraiseBookExists"), UiText.Get(nameof(FrmUpdateFileName), "Overwrite"), MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							File.Copy(Gf.SelectedListName, praiseBookPath, overwrite: true);
							return true;
						}
						return false;
					}
					catch
					{
						MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "SaveFailed"));
						return false;
					}
				}
				if (Gf.NameChangeAction == 8)
				{
					string worshipListPath = Gf.WorshipDir + DataUtil.Trim(tbFileName.Text) + ".json";
					try
					{
						if (!File.Exists(worshipListPath))
						{
							File.Copy(Gf.SelectedListName, worshipListPath, overwrite: true);
							return true;
						}
						if (MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "WorshipListExists"), UiText.Get(nameof(FrmUpdateFileName), "Overwrite"), MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							File.Copy(Gf.SelectedListName, worshipListPath, overwrite: true);
							return true;
						}
						return false;
					}
					catch
					{
						MessageBox.Show(UiText.Get(nameof(FrmUpdateFileName), "SaveFailed"));
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
