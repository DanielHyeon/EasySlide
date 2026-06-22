using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public partial class FrmCopyMoveExternal : Form
	{

		private string ActionString1 = "";

		private string ActionString2 = "";

		public FrmCopyMoveExternal()
		{
			InitializeComponent();
		}

		private void FrmCopyMoveExternal_Load(object sender, EventArgs e)
		{
			if (Gf.ExternalCopyFolder >= 1)
			{
				ActionString1 = "copying";
				ActionString2 = "copy";
				switch (Gf.ExternalMoveCopyType)
				{
				case "I":
					Text = "Copy InfoScreen(s)";
					optCopyToFolder.Enabled = true;
					SongFolder.Enabled = true;
					break;
				case "P":
					Text = "Copy Powerpoint File(s)";
					optCopyToInfoScreen.Checked = true;
					optCopyToFolder.Enabled = false;
					SongFolder.Enabled = false;
					break;
				}
			}
			else
			{
				ActionString1 = "moving";
				ActionString2 = "move";
				optCopyToInfoScreen.Checked = true;
				optCopyToFolder.Enabled = false;
				SongFolder.Enabled = false;
				switch (Gf.ExternalMoveCopyType)
				{
				case "I":
					Text = "Move InfoScreen(s)";
					break;
				case "P":
					Text = "Move Powerpoint File(s)";
					break;
				}
			}
			Label1.Text = "You have selected " + Gf.SelectedItemsCount + " item" + ((Gf.SelectedItemsCount > 1) ? "s" : "") + " for " + ActionString1 + ". Please choose a folder to " + ActionString2 + " the item" + ((Gf.SelectedItemsCount > 1) ? "s" : "") + " to, and then click OK.";
			ExternalFilesFolder.Items.Clear();
			switch (Gf.ExternalMoveCopyType)
			{
			case "I":
			{
				for (int i = 0; i < Gf.InfoScreenFolderTotal; i++)
				{
					ExternalFilesFolder.Items.Add(Gf.InfoScreenGroups[i, 0]);
				}
				break;
			}
			case "P":
			{
				for (int i = 0; i < Gf.PowerpointFolderTotal; i++)
				{
					ExternalFilesFolder.Items.Add(Gf.PowerpointGroups[i, 0]);
				}
				break;
			}
			}
			SongFolder.Items.Clear();
			for (int i = 1; i <= 40; i++)
			{
				if (Gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(Gf.FolderName[i]);
				}
			}
		}

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
					MessageBox.Show("Please select a folder to " + ActionString2 + " to!");
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
					MessageBox.Show("Please select a folder to " + ActionString2 + " to!");
				}
			}
			else
			{
				MessageBox.Show("There are no Song Folders enabled!");
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
}
