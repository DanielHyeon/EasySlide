using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Properties;

namespace Easislides
{
	public partial class FrmRearrangeFolderPositions : Form
	{

		private int[] OriginalFolderPosition = new int[Gf.MAXSONGSFOLDERS];

		public FrmRearrangeFolderPositions()
		{
			InitializeComponent();
		}

		private void FrmRearrangeFolderPositions_Load(object sender, EventArgs e)
		{
			BuildFolderList();
		}

		private void BuildFolderList()
		{
			ListViewItem listViewItem = new ListViewItem();
			SongFolder.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				listViewItem = SongFolder.Items.Add(Gf.FolderName[i]);
				listViewItem.SubItems.Add(i.ToString());
			}
		}

		private void SF_UpDown_MouseUp(object sender, MouseEventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string name = toolStripButton.Name;
			if (name == "SF_Up")
			{
				MoveFolderUp();
			}
			else
			{
				MoveFolderDown();
			}
		}

		private void MoveFolderUp()
		{
			int count = SongFolder.Items.Count;
			if (count < 1)
			{
				return;
			}
			int selectedIndex = Gf.GetSelectedIndex(SongFolder);
			if (selectedIndex >= 1)
			{
				for (int i = 0; i <= 1; i++)
				{
					string text = SongFolder.Items[selectedIndex].SubItems[i].Text;
					SongFolder.Items[selectedIndex].SubItems[i].Text = SongFolder.Items[selectedIndex - 1].SubItems[i].Text;
					SongFolder.Items[selectedIndex - 1].SubItems[i].Text = text;
				}
				SongFolder.Items[selectedIndex].Selected = false;
				SongFolder.Items[selectedIndex - 1].Selected = true;
				SongFolder.EnsureVisible(selectedIndex - 1);
			}
		}

		private void MoveFolderDown()
		{
			int count = SongFolder.Items.Count;
			if (count <= 1)
			{
				return;
			}
			int selectedIndex = Gf.GetSelectedIndex(SongFolder);
			if (!((selectedIndex < 0) | (selectedIndex == count - 1)))
			{
				for (int i = 0; i <= 1; i++)
				{
					string text = SongFolder.Items[selectedIndex].SubItems[i].Text;
					SongFolder.Items[selectedIndex].SubItems[i].Text = SongFolder.Items[selectedIndex + 1].SubItems[i].Text;
					SongFolder.Items[selectedIndex + 1].SubItems[i].Text = text;
				}
				SongFolder.Items[selectedIndex].Selected = false;
				SongFolder.Items[selectedIndex + 1].Selected = true;
				SongFolder.EnsureVisible(selectedIndex + 1);
			}
		}

		private void OKBtn_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			string inName = Gf.FolderName[Gf.JumpToA];
			string inName2 = Gf.FolderName[Gf.JumpToB];
			string inName3 = Gf.FolderName[Gf.JumpToC];
			if (Gf.SwapFolderNumbers(SongFolder))
			{
				Gf.LoadFolderNamesArray();
				Gf.JumpToA = Gf.GetFolderNumber(inName);
				Gf.JumpToB = Gf.GetFolderNumber(inName2);
				Gf.JumpToC = Gf.GetFolderNumber(inName3);
				base.DialogResult = DialogResult.OK;
			}
			else
			{
				MessageBox.Show("Error encountered whilst re-arranging some of the folders. Please restart EasiSlides and try again.");
				base.DialogResult = DialogResult.Cancel;
			}
			Cursor = Cursors.Default;
			Close();
		}

		private void FrmRearrangeFolderPositions_FormClosing(object sender, FormClosingEventArgs e)
		{
			Cursor = Cursors.Default;
		}
	}
}
