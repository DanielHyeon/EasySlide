using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;

namespace Easislides
{
	public partial class FrmManageItemLists : Form
	{
		private string InputDir;

		private string ListText;

		private string ListTextTo;

		private string CurFile;

		private string Ext_ToUse = "";

		private string TrashDir;

		public FrmManageItemLists()
		{
			InitializeComponent();
		}

		private void FrmManageItemLists_Load(object sender, EventArgs e)
		{
			if (Gf.EasiSlidesMode == UsageMode.Worship)
			{
				Text = "Manage Worship Lists";
				InputDir = Gf.WorshipDir;
				TrashDir = InputDir + "Trash\\";
				CurFile = Gf.CurSession;
				ListText = "WorshipList";
				ListTextTo = "PraiseBook";
			}
			else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
			{
				Text = "Manage PraiseBooks";
				InputDir = Gf.PraiseBookDir;
				TrashDir = InputDir + "Trash\\";
				CurFile = Gf.CurPraiseBook;
				ListText = "PraiseBook";
				ListTextTo = "WorshipList";
			}
			SaveToBtn.Text = ListTextTo;
			InitializeTrash();
			UpdateLists();
			UpdateTrashList();
		}

		private void InitializeTrash()
		{
			try
			{
				if (!Directory.Exists(TrashDir))
				{
					Directory.CreateDirectory(TrashDir);
				}
			}
			catch
			{
				MessageBox.Show("Error creating trash folder. Please ensure you have write access.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void UpdateLists()
		{
			bool flag = false;
			ItemList.Items.Clear();
			Gf.ValidateDir(InputDir, CreateDir: true);
			Ext_ToUse = ((InputDir == Gf.WorshipDir) ? ".esw" : ".esp");
			string[] files = Directory.GetFiles(InputDir, "*" + Ext_ToUse);
			foreach (string text in files)
			{
				// Skip files in Trash folder (Directory.GetFiles doesn't search subdirectories, but keep this check for safety)
				if (text.StartsWith(TrashDir, StringComparison.OrdinalIgnoreCase))
					continue;

				string InFileName = text;
				InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				if (InFileName != "")
				{
					ItemList.Items.Add(InFileName);
					if (CurFile == InFileName)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				int num = 0;
				while (true)
				{
					if (num < ItemList.Items.Count)
					{
						if (CurFile == ItemList.Items[num].Text)
						{
							break;
						}
						num++;
						continue;
					}
					return;
				}
				ItemList.Items[num].Selected = true;
			}
			else if (Gf.EasiSlidesMode == UsageMode.Worship)
			{
				if (ItemList.Items.Count > 0)
				{
					Gf.CurSession = ItemList.Items[0].Text;
				}
				else
				{
					Gf.CurSession = "";
				}
			}
			else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
			{
				if (ItemList.Items.Count > 0)
				{
					Gf.CurPraiseBook = ItemList.Items[0].Text;
				}
				else
				{
					Gf.CurPraiseBook = "";
				}
			}
		}

		private void UpdateTrashList()
		{
			TrashList.Items.Clear();
			if (!Directory.Exists(TrashDir))
				return;

			Ext_ToUse = ((InputDir == Gf.WorshipDir) ? ".esw" : ".esp");
			string[] files = Directory.GetFiles(TrashDir, "*" + Ext_ToUse);
			foreach (string text in files)
			{
				string InFileName = text;
				InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				if (InFileName != "")
				{
					TrashList.Items.Add(InFileName);
				}
			}
		}

		private void AddBtn_Click(object sender, EventArgs e)
		{
			Gf.NameChangeAction = 1;
			Gf.NameChangeSucceeded = false;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				UpdateLists();
				if (Gf.EasiSlidesMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					Gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void DelBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != ListsTab)
				return;

			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to delete!");
				return;
			}

			int selectedCount = ItemList.SelectedItems.Count;
			string message = selectedCount == 1
				? "Really move '" + ItemList.SelectedItems[0].Text + "' " + ListText + " to trash?"
				: "Really move " + selectedCount + " selected " + ListText + "(s) to trash?";

			if (MessageBox.Show(message, "Move to Trash", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				int successCount = 0;
				int failCount = 0;

				foreach (ListViewItem item in ItemList.SelectedItems.Cast<ListViewItem>().ToList())
				{
					try
					{
						string fileName = item.Text;
						string sourcePath = InputDir + fileName + Ext_ToUse;
						string destPath = TrashDir + fileName + Ext_ToUse;

						// Handle duplicate files in trash
						if (File.Exists(destPath))
						{
							string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
							string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
							destPath = TrashDir + nameWithoutExt + "_" + timestamp + Ext_ToUse;
						}

						File.Move(sourcePath, destPath);
						successCount++;
					}
					catch
					{
						failCount++;
					}
				}

				if (failCount > 0)
				{
					MessageBox.Show($"Error: {failCount} file(s) could not be moved to trash. {successCount} file(s) moved successfully.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				UpdateLists();
				UpdateTrashList();

				if (Gf.EasiSlidesMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					Gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void RenameBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != ListsTab)
				return;

			Gf.NameChangeAction = 2;
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to rename!");
				return;
			}
			Gf.SelectedListName = ItemList.SelectedItems[0].Text;
			bool flag = (Gf.SelectedListName == CurFile) ? true : false;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				if (flag)
				{
					CurFile = Gf.SelectedListName;
				}
				if (Gf.EasiSlidesMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
					Gf.CurSession = CurFile;
				}
				else
				{
					Gf.PraiseBooksListChanged = true;
					Gf.CurPraiseBook = CurFile;
				}
				UpdateLists();
			}
		}

		private void SaveAsBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != ListsTab)
				return;

			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to Save As!");
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			if (SaveAsItem(InputDir + text + Ext_ToUse) != "")
			{
				if (Gf.EasiSlidesMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					Gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void SaveToBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != ListsTab)
				return;

			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to Save To " + ListTextTo);
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			Gf.SelectedListName = InputDir + text + Ext_ToUse;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			Gf.NameChangeSucceeded = false;
			if (Gf.EasiSlidesMode == UsageMode.Worship)
			{
				Gf.NameChangeAction = 7;
				if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
				{
					Gf.PraiseBooksListChanged = true;
				}
			}
			else
			{
				Gf.NameChangeAction = 8;
				if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
				{
					Gf.WorshipListsChanged = true;
				}
			}
		}

		private void SaveTemplateBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != ListsTab)
				return;

			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to Save As a Template!");
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			try
			{
				string text2 = InputDir + text + Ext_ToUse;
				string text3 = Gf.WorshipTemplatesDir + text + ".est";
				if (File.Exists(text2))
				{
					DialogResult dialogResult = DialogResult.Yes;
					if (File.Exists(text3))
					{
						dialogResult = MessageBox.Show("There is already a template with the same name. Overwrite it?", "Overwrite", MessageBoxButtons.YesNo);
					}
					if (dialogResult == DialogResult.Yes)
					{
						File.Copy(text2, text3, overwrite: true);
					}
				}
				else
				{
					MessageBox.Show("Error Saving Template - cannot find the selected " + ListText + "!");
				}
			}
			catch
			{
				MessageBox.Show("Error Saving File, please make sure you have write access and try again");
			}
		}

		private string SaveAsItem(string InFileName)
		{
			saveFileDialog1.Filter = "EasiSlides " + ListText + " File (*" + Ext_ToUse + ")|*" + Ext_ToUse;
			saveFileDialog1.InitialDirectory = Path.GetDirectoryName(InFileName);
			saveFileDialog1.FileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true);
			saveFileDialog1.OverwritePrompt = true;
			saveFileDialog1.AddExtension = true;
			saveFileDialog1.DefaultExt = Ext_ToUse;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string fileName = saveFileDialog1.FileName;
				try
				{
					if (!File.Exists(fileName))
					{
						File.Copy(InFileName, fileName, overwrite: true);
						UpdateLists();
						return fileName;
					}
				}
				catch
				{
					MessageBox.Show("Error Saving File, please make sure you have write access and try again");
				}
			}
			return "";
		}

		private void CloseBtn_Click(object sender, EventArgs e)
		{
			MakeSelectedItemCurrent();
		}

		private void ItemList_DoubleClick(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab == ListsTab)
			{
				MakeSelectedItemCurrent();
				Close();
			}
		}

		private void MakeSelectedItemCurrent()
		{
			if (ItemList.SelectedItems.Count > 0)
			{
				if (Gf.EasiSlidesMode == UsageMode.Worship)
				{
					Gf.CurSession = ItemList.SelectedItems[0].Text;
				}
				else
				{
					Gf.CurPraiseBook = ItemList.SelectedItems[0].Text;
				}
			}
		}

		private void RestoreBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != TrashTab)
				return;

			if (TrashList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to restore!");
				return;
			}

			int selectedCount = TrashList.SelectedItems.Count;
			string message = selectedCount == 1
				? "Really restore '" + TrashList.SelectedItems[0].Text + "' " + ListText + "?"
				: "Really restore " + selectedCount + " selected " + ListText + "(s)?";

			if (MessageBox.Show(message, "Restore", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				int successCount = 0;
				int failCount = 0;

				foreach (ListViewItem item in TrashList.SelectedItems.Cast<ListViewItem>().ToList())
				{
					try
					{
						string fileName = item.Text;
						string sourcePath = TrashDir + fileName + Ext_ToUse;
						string destPath = InputDir + fileName + Ext_ToUse;

						// Handle duplicate files - add timestamp if exists
						if (File.Exists(destPath))
						{
							string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
							string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
							destPath = InputDir + nameWithoutExt + "_" + timestamp + Ext_ToUse;
						}

						File.Move(sourcePath, destPath);
						successCount++;
					}
					catch
					{
						failCount++;
					}
				}

				if (failCount > 0)
				{
					MessageBox.Show($"Error: {failCount} file(s) could not be restored. {successCount} file(s) restored successfully.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				UpdateTrashList();
				UpdateLists();

				if (Gf.EasiSlidesMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					Gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void DeletePermanentlyBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != TrashTab)
				return;

			if (TrashList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to delete permanently!");
				return;
			}

			int selectedCount = TrashList.SelectedItems.Count;
			string message = selectedCount == 1
				? "Really permanently delete '" + TrashList.SelectedItems[0].Text + "' " + ListText + "? This action cannot be undone!"
				: "Really permanently delete " + selectedCount + " selected " + ListText + "(s)? This action cannot be undone!";

			if (MessageBox.Show(message, "Permanently Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				int successCount = 0;
				int failCount = 0;

				foreach (ListViewItem item in TrashList.SelectedItems.Cast<ListViewItem>().ToList())
				{
					try
					{
						string fileName = item.Text;
						string filePath = TrashDir + fileName + Ext_ToUse;
						File.Delete(filePath);
						successCount++;
					}
					catch
					{
						failCount++;
					}
				}

				if (failCount > 0)
				{
					MessageBox.Show($"Error: {failCount} file(s) could not be deleted. {successCount} file(s) deleted successfully.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				UpdateTrashList();
			}
		}

		private void EmptyTrashBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != TrashTab)
				return;

			if (TrashList.Items.Count == 0)
			{
				MessageBox.Show("Trash is already empty.");
				return;
			}

			if (MessageBox.Show("Really empty the trash? This will permanently delete all items in trash. This action cannot be undone!", "Empty Trash", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				int successCount = 0;
				int failCount = 0;

				foreach (ListViewItem item in TrashList.Items.Cast<ListViewItem>().ToList())
				{
					try
					{
						string fileName = item.Text;
						string filePath = TrashDir + fileName + Ext_ToUse;
						File.Delete(filePath);
						successCount++;
					}
					catch
					{
						failCount++;
					}
				}

				if (failCount > 0)
				{
					MessageBox.Show($"Error: {failCount} file(s) could not be deleted. {successCount} file(s) deleted successfully.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				UpdateTrashList();
			}
		}

		private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab == ListsTab)
			{
				UpdateLists();
				// Show Lists buttons
				AddBtn.Visible = true;
				RenameBtn.Visible = true;
				DelBtn.Visible = true;
				SaveAsBtn.Visible = true;
				SaveToBtn.Visible = true;
				SaveTemplateBtn.Visible = true;
				// Hide Trash buttons
				RestoreBtn.Visible = false;
				DeletePermanentlyBtn.Visible = false;
				EmptyTrashBtn.Visible = false;
			}
			else if (MainTabControl.SelectedTab == TrashTab)
			{
				UpdateTrashList();
				// Hide Lists buttons
				AddBtn.Visible = false;
				RenameBtn.Visible = false;
				DelBtn.Visible = false;
				SaveAsBtn.Visible = false;
				SaveToBtn.Visible = false;
				SaveTemplateBtn.Visible = false;
				// Show Trash buttons
				RestoreBtn.Visible = true;
				DeletePermanentlyBtn.Visible = true;
				EmptyTrashBtn.Visible = true;
			}
		}

		            }
}
