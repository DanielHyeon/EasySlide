using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Axicultus.Localization;
using Axicultus.Module;
using Axicultus.Properties;

namespace Axicultus;

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
			UiText.ApplyTo(this);
		}

		private void FrmManageItemLists_Load(object sender, EventArgs e)
		{
			if (Gf.AxicultusMode == UsageMode.Worship)
			{
				Text = UiText.Get(nameof(FrmManageItemLists), "TitleWorshipLists");
				InputDir = Gf.WorshipDir;
				TrashDir = InputDir + "Trash\\";
				CurFile = Gf.CurSession;
				ListText = UiText.Get(nameof(FrmManageItemLists), "WorshipList");
				ListTextTo = UiText.Get(nameof(FrmManageItemLists), "PraiseBook");
			}
			else if (Gf.AxicultusMode == UsageMode.PraiseBook)
			{
				Text = UiText.Get(nameof(FrmManageItemLists), "TitlePraiseBooks");
				InputDir = Gf.PraiseBookDir;
				TrashDir = InputDir + "Trash\\";
				CurFile = Gf.CurPraiseBook;
				ListText = UiText.Get(nameof(FrmManageItemLists), "PraiseBook");
				ListTextTo = UiText.Get(nameof(FrmManageItemLists), "WorshipList");
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
				string text = UiText.Get(nameof(FrmManageItemLists), "TrashFolderFailed");
				string caption = UiText.Get(nameof(FrmManageItemLists), "ErrorCaption");
				MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void UpdateLists()
		{
			bool foundCurrentList = false;
			ItemList.Items.Clear();
			Gf.ValidateDir(InputDir, CreateDir: true);
			Ext_ToUse = ".json";
			string[] files = Directory.GetFiles(InputDir, "*" + Ext_ToUse);
			foreach (string listPath in files)
			{
				// Skip files in Trash folder (Directory.GetFiles doesn't search subdirectories, but keep this check for safety)
				if (listPath.StartsWith(TrashDir, StringComparison.OrdinalIgnoreCase))
					continue;

				string InFileName = listPath;
				InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				if (InFileName != "")
				{
					ItemList.Items.Add(InFileName);
					if (CurFile == InFileName)
					{
						foundCurrentList = true;
					}
				}
			}
			if (foundCurrentList)
			{
				int currentListIndex = 0;
				while (true)
				{
					if (currentListIndex < ItemList.Items.Count)
					{
						if (CurFile == ItemList.Items[currentListIndex].Text)
						{
							break;
						}
						currentListIndex++;
						continue;
					}
					return;
				}
				ItemList.Items[currentListIndex].Selected = true;
			}
			else if (Gf.AxicultusMode == UsageMode.Worship)
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
			else if (Gf.AxicultusMode == UsageMode.PraiseBook)
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

			Ext_ToUse = ".json";
			string[] files = Directory.GetFiles(TrashDir, "*" + Ext_ToUse);
			foreach (string deletedListPath in files)
			{
				string InFileName = deletedListPath;
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
			FrmUpdateFileName frmUpdateFileName = new();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				UpdateLists();
				if (Gf.AxicultusMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.AxicultusMode == UsageMode.PraiseBook)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToDelete", ListText);
				MessageBox.Show(text);
				return;
			}

			int selectedCount = ItemList.SelectedItems.Count;
			string message = selectedCount == 1
				? UiText.Format(nameof(FrmManageItemLists), "MoveOneToTrash", ItemList.SelectedItems[0].Text, ListText)
				: UiText.Format(nameof(FrmManageItemLists), "MoveManyToTrash", selectedCount, ListText);

			string moveCaption = UiText.Get(nameof(FrmManageItemLists), "MoveToTrashCaption");
			if (MessageBox.Show(message, moveCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
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
					string text = UiText.Format(nameof(FrmManageItemLists), "MoveToTrashFailed", failCount, successCount);
					string caption = UiText.Get(nameof(FrmManageItemLists), "ErrorCaption");
					MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				UpdateLists();
				UpdateTrashList();

				if (Gf.AxicultusMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.AxicultusMode == UsageMode.PraiseBook)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToRename", ListText);
				MessageBox.Show(text);
				return;
			}
			Gf.SelectedListName = ItemList.SelectedItems[0].Text;
			bool renamingSelectedList = (Gf.SelectedListName == CurFile) ? true : false;
			FrmUpdateFileName frmUpdateFileName = new();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				if (renamingSelectedList)
				{
					CurFile = Gf.SelectedListName;
				}
				if (Gf.AxicultusMode == UsageMode.Worship)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToSaveAs", ListText);
				MessageBox.Show(text);
				return;
			}
			string selectedListName = ItemList.SelectedItems[0].Text;
			if (SaveAsItem(InputDir + selectedListName + Ext_ToUse) != "")
			{
				if (Gf.AxicultusMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.AxicultusMode == UsageMode.PraiseBook)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToSaveTo", ListText, ListTextTo);
				MessageBox.Show(text);
				return;
			}
			string selectedListName = ItemList.SelectedItems[0].Text;
			Gf.SelectedListName = InputDir + selectedListName + Ext_ToUse;
			FrmUpdateFileName frmUpdateFileName = new();
			Gf.NameChangeSucceeded = false;
			if (Gf.AxicultusMode == UsageMode.Worship)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToSaveTemplate", ListText);
				MessageBox.Show(text);
				return;
			}
			string selectedListName = ItemList.SelectedItems[0].Text;
			try
			{
				string sourceListPath = InputDir + selectedListName + Ext_ToUse;
				string templatePath = Gf.WorshipTemplatesDir + selectedListName + ".est";
				if (File.Exists(sourceListPath))
				{
					DialogResult dialogResult = DialogResult.Yes;
					if (File.Exists(templatePath))
					{
						string text = UiText.Get(nameof(FrmManageItemLists), "TemplateExists");
						string caption = UiText.Get(nameof(FrmManageItemLists), "OverwriteCaption");
						dialogResult = MessageBox.Show(text, caption, MessageBoxButtons.YesNo);
					}
					if (dialogResult == DialogResult.Yes)
					{
						File.Copy(sourceListPath, templatePath, overwrite: true);
					}
				}
				else
				{
					string text = UiText.Format(nameof(FrmManageItemLists), "TemplateSourceMissing", ListText);
					MessageBox.Show(text);
				}
			}
			catch
			{
				string text = UiText.Get(nameof(FrmManageItemLists), "SaveFailed");
				MessageBox.Show(text);
			}
		}

		private string SaveAsItem(string InFileName)
		{
			saveFileDialog1.Filter = UiText.Format(nameof(FrmManageItemLists), "SaveFilter", ListText, Ext_ToUse);
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
					string text = UiText.Get(nameof(FrmManageItemLists), "SaveFailed");
					MessageBox.Show(text);
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
				if (Gf.AxicultusMode == UsageMode.Worship)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToRestore", ListText);
				MessageBox.Show(text);
				return;
			}

			int selectedCount = TrashList.SelectedItems.Count;
			string message = selectedCount == 1
				? UiText.Format(nameof(FrmManageItemLists), "RestoreOne", TrashList.SelectedItems[0].Text, ListText)
				: UiText.Format(nameof(FrmManageItemLists), "RestoreMany", selectedCount, ListText);

			string restoreCaption = UiText.Get(nameof(FrmManageItemLists), "RestoreCaption");
			if (MessageBox.Show(message, restoreCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
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
					string text = UiText.Format(nameof(FrmManageItemLists), "RestoreFailed", failCount, successCount);
					string caption = UiText.Get(nameof(FrmManageItemLists), "ErrorCaption");
					MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				UpdateTrashList();
				UpdateLists();

				if (Gf.AxicultusMode == UsageMode.Worship)
				{
					Gf.WorshipListsChanged = true;
				}
				else if (Gf.AxicultusMode == UsageMode.PraiseBook)
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
				string text = UiText.Format(nameof(FrmManageItemLists), "SelectToDeletePermanently", ListText);
				MessageBox.Show(text);
				return;
			}

			int selectedCount = TrashList.SelectedItems.Count;
			string message = selectedCount == 1
				? UiText.Format(nameof(FrmManageItemLists), "DeleteOnePermanently", TrashList.SelectedItems[0].Text, ListText)
				: UiText.Format(nameof(FrmManageItemLists), "DeleteManyPermanently", selectedCount, ListText);

			string deleteCaption = UiText.Get(nameof(FrmManageItemLists), "DeletePermanentlyCaption");
			if (MessageBox.Show(message, deleteCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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
					string text = UiText.Format(nameof(FrmManageItemLists), "DeleteFailed", failCount, successCount);
					string caption = UiText.Get(nameof(FrmManageItemLists), "ErrorCaption");
					MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
				string text = UiText.Get(nameof(FrmManageItemLists), "TrashEmpty");
				MessageBox.Show(text);
				return;
			}

			string confirm = UiText.Get(nameof(FrmManageItemLists), "EmptyTrashConfirm");
			string emptyCaption = UiText.Get(nameof(FrmManageItemLists), "EmptyTrashCaption");
			if (MessageBox.Show(confirm, emptyCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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
					string text = UiText.Format(nameof(FrmManageItemLists), "DeleteFailed", failCount, successCount);
					string caption = UiText.Get(nameof(FrmManageItemLists), "ErrorCaption");
					MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
