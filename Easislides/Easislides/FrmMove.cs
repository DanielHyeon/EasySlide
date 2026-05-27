using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public partial class FrmMove : Form
	{

		public FrmMove()
		{
			InitializeComponent();
		}

		private void FrmMove_Load(object sender, EventArgs e)
		{
			Label1.Text = "You have selected " + Gf.SelectedItemsCount + " item" + ((Gf.SelectedItemsCount > 1) ? "s" : "") + " for Moving. Please choose a folder to move the item" + ((Gf.SelectedItemsCount > 1) ? "s" : "") + " to, and then click OK.";
			Gf.MoveToFolder = -1;
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
			if (SongFolder.Items.Count > 0)
			{
				if (SongFolder.SelectedItems.Count > 0)
				{
					Gf.MoveToFolder = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Please select a folder to move the songs to!");
				}
			}
			else
			{
				MessageBox.Show("There are no Song Folders enabled!");
			}
		}

		private void SongFolder_DoubleClick(object sender, EventArgs e)
		{
			BtnOK.PerformClick();
		}
	}
}
