using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Localization;

namespace Axicultus;

	public partial class FrmCopyMoveExternal : Form
	{

		private string ActionString1 = "";

		public FrmCopyMoveExternal()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmCopyMoveExternal_Load(object sender, EventArgs e)
		{
			if (Gf.ExternalCopyFolder >= 1)
			{
				ActionString1 = "Copy";
				switch (Gf.ExternalMoveCopyType)
				{
				case "I":
					Text = UiText.Get(nameof(FrmCopyMoveExternal), "CopyInfoScreens");
					optCopyToFolder.Enabled = true;
					SongFolder.Enabled = true;
					break;
				case "P":
					Text = UiText.Get(nameof(FrmCopyMoveExternal), "CopyPowerPoints");
					optCopyToInfoScreen.Checked = true;
					optCopyToFolder.Enabled = false;
					SongFolder.Enabled = false;
					break;
				}
			}
			else
			{
				ActionString1 = "Move";
				optCopyToInfoScreen.Checked = true;
				optCopyToFolder.Enabled = false;
				SongFolder.Enabled = false;
				switch (Gf.ExternalMoveCopyType)
				{
				case "I":
					Text = UiText.Get(nameof(FrmCopyMoveExternal), "MoveInfoScreens");
					break;
				case "P":
					Text = UiText.Get(nameof(FrmCopyMoveExternal), "MovePowerPoints");
					break;
				}
			}
			Label1.Text = UiText.Format(nameof(FrmCopyMoveExternal), ActionString1 + (Gf.SelectedItemsCount > 1 ? "SelectedItems" : "SelectedItem"), Gf.SelectedItemsCount);
			ExternalFilesFolder.Items.Clear();
			switch (Gf.ExternalMoveCopyType)
			{
			case "I":
			{
				for (int i = 0; i < Gf.InfoScreenGroups.Count; i++)
				{
					ExternalFilesFolder.Items.Add(Gf.InfoScreenGroups[i].Name);
				}
				break;
			}
			case "P":
			{
				for (int i = 0; i < Gf.PowerpointGroups.Count; i++)
				{
					ExternalFilesFolder.Items.Add(Gf.PowerpointGroups[i].Name);
				}
				break;
			}
			}
			SongFolder.Items.Clear();
			for (int i = 1; i <= 40; i++)
			{
				if (Gf.Folders[i].Use > 0)
				{
					SongFolder.Items.Add(Gf.Folders[i].Name);
				}
			}
		}

		private string SelectFolderText() => UiText.Get(nameof(FrmCopyMoveExternal), ActionString1 + "SelectFolder");

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (optCopyToInfoScreen.Checked)
			{
				if (ExternalFilesFolder.SelectedItems.Count > 0)
				{
					if (Gf.ExternalCopyFolder >= 1)
					{
						Gf.ExternalCopyFolder = Gf.GetSelectedIndex(ExternalFilesFolder);
					}
					else
					{
						Gf.ExternalMoveFolder = Gf.GetSelectedIndex(ExternalFilesFolder);
					}
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show(SelectFolderText());
				}
			}
			else if (SongFolder.Items.Count > 0)
			{
				if (SongFolder.SelectedItems.Count > 0)
				{
					if (Gf.ExternalCopyFolder >= 1)
					{
						Gf.ExternalCopyFolder = -1 * Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					}
					else
					{
						Gf.ExternalMoveFolder = -1 * Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					}
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show(SelectFolderText());
				}
			}
			else
			{
				MessageBox.Show(UiText.Get(nameof(FrmCopyMoveExternal), "NoFolders"));
			}
		}

		private void SongFolder_DoubleClick(object sender, EventArgs e)
		{
			SelectOptCopyToFolder(0);
			BtnOK.PerformClick();
		}

		private void SongFolder_MouseUp(object sender, MouseEventArgs e)
		{
			SelectOptCopyToFolder(0);
		}

		private void SongFolder_KeyUp(object sender, KeyEventArgs e)
		{
			SelectOptCopyToFolder(0);
		}

		private void ExternalFilesFolder_DoubleClick(object sender, EventArgs e)
		{
			SelectOptCopyToFolder(1);
			BtnOK.PerformClick();
		}

		private void ExternalFilesFolder_KeyUp(object sender, KeyEventArgs e)
		{
			SelectOptCopyToFolder(1);
		}

		private void ExternalFilesFolder_MouseUp(object sender, MouseEventArgs e)
		{
			SelectOptCopyToFolder(1);
		}

		private void SelectOptCopyToFolder(int SelectedOpt)
		{
			if (SelectedOpt == 0)
			{
				optCopyToFolder.Checked = true;
			}
			else
			{
				optCopyToInfoScreen.Checked = true;
			}
		}
	}
