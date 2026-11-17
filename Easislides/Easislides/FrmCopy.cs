using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public partial class FrmCopy : Form
	{

		public FrmCopy()
		{
			InitializeComponent();
		}

		private void FrmCopy_Load(object sender, EventArgs e)
		{
			Label1.Text = "You have selected " + Gf.SelectedItemsCount + " item" + ((Gf.SelectedItemsCount > 1) ? "s" : "") + " for Copy. Please select appropriate folder to copy the item" + ((Gf.SelectedItemsCount > 1) ? "s" : "") + " to, and then click OK.";
			Gf.CopyToFolder = -1;
			SongFolder.Items.Clear();
			for (int i = 1; i <= 40; i++)
			{
				if (Gf.Folders[i].Use > 0)
				{
                    SongFolder.Items.Add(Gf.Folders[i].Name);
				}
			}
			ExternalFilesFolder.Items.Clear();
			for (int i = 0; i < Gf.InfoScreenGroups.Count; i++)
			{
				ExternalFilesFolder.Items.Add(Gf.InfoScreenGroups[i].Name);
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (optCopyToFolder.Checked)
			{
				if (SongFolder.Items.Count > 0)
				{
					if (SongFolder.SelectedItems.Count > 0)
					{
						Gf.CopyToFolder = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
						base.DialogResult = DialogResult.OK;
						Close();
					}
					else
					{
						MessageBox.Show("Please select a folder to copy the songs to!");
					}
				}
				else
				{
					MessageBox.Show("There are no Song Folders enabled!");
				}
			}
			else if (ExternalFilesFolder.SelectedItems.Count > 0)
			{
				Gf.CopyToFolder = -1 * (1 + Gf.GetSelectedIndex(ExternalFilesFolder));
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				MessageBox.Show("Please select a folder to copy the songs to!");
			}
		}

		private void SongFolder_DoubleClick(object sender, EventArgs e)
		{
			SelectOptCopyToFolder(0);
			BtnOK.PerformClick();
		}

		private void SongFolder_KeyUp(object sender, KeyEventArgs e)
		{
			SelectOptCopyToFolder(0);
		}

		private void SongFolder_MouseUp(object sender, MouseEventArgs e)
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
