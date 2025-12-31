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
	public class FrmManageItemLists : Form
	{
		private string InputDir;

		private string InputDefaultFile;

		private string ListText;

		private string ListTextTo;

		private string CurFile;

		private string StartingFolder;

		private string Ext_ToUse = "";

		private string TrashDir;

		private IContainer components = null;

		private TabControl MainTabControl;

		private TabPage ListsTab;

		private TabPage TrashTab;

		private ListView ItemList;

		private ListView TrashList;

		private Button AddBtn;

		private Button RenameBtn;

		private Button DelBtn;

		private Button CloseBtn;

		private ColumnHeader columnHeader1;

		private ColumnHeader trashColumnHeader1;

		private Button SaveAsBtn;

		private Button SaveToBtn;

		private Button SaveTemplateBtn;

		private Button RestoreBtn;

		private Button DeletePermanentlyBtn;

		private Button EmptyTrashBtn;

		private SaveFileDialog saveFileDialog1;

		public FrmManageItemLists()
		{
			InitializeComponent();
		}

		private void FrmManageItemLists_Load(object sender, EventArgs e)
		{
			if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				Text = "Manage Worship Lists";
				InputDir = gf.WorshipDir;
				TrashDir = InputDir + "Trash\\";
				InputDefaultFile = "Worship Service";
				CurFile = gf.CurSession;
				ListText = "WorshipList";
				ListTextTo = "PraiseBook";
			}
			else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
			{
				Text = "Manage PraiseBooks";
				InputDir = gf.PraiseBookDir;
				TrashDir = InputDir + "Trash\\";
				InputDefaultFile = "PraiseBook 1";
				CurFile = gf.CurPraiseBook;
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
			gf.ValidateDir(InputDir, CreateDir: true);
			Ext_ToUse = ((InputDir == gf.WorshipDir) ? ".esw" : ".esp");
			string[] files = Directory.GetFiles(InputDir, "*" + Ext_ToUse);
			foreach (string text in files)
			{
				// Skip files in Trash folder (Directory.GetFiles doesn't search subdirectories, but keep this check for safety)
				if (text.StartsWith(TrashDir, StringComparison.OrdinalIgnoreCase))
					continue;

				string InFileName = text;
				InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
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
			else if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				if (ItemList.Items.Count > 0)
				{
					gf.CurSession = ItemList.Items[0].Text;
				}
				else
				{
					gf.CurSession = "";
				}
			}
			else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
			{
				if (ItemList.Items.Count > 0)
				{
					gf.CurPraiseBook = ItemList.Items[0].Text;
				}
				else
				{
					gf.CurPraiseBook = "";
				}
			}
		}

		private void UpdateTrashList()
		{
			TrashList.Items.Clear();
			if (!Directory.Exists(TrashDir))
				return;

			Ext_ToUse = ((InputDir == gf.WorshipDir) ? ".esw" : ".esp");
			string[] files = Directory.GetFiles(TrashDir, "*" + Ext_ToUse);
			foreach (string text in files)
			{
				string InFileName = text;
				InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				if (InFileName != "")
				{
					TrashList.Items.Add(InFileName);
				}
			}
		}

		private void AddBtn_Click(object sender, EventArgs e)
		{
			gf.NameChangeAction = 1;
			gf.NameChangeSucceeded = false;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				UpdateLists();
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
				}
				else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					gf.PraiseBooksListChanged = true;
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

				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
				}
				else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void RenameBtn_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab != ListsTab)
				return;

			gf.NameChangeAction = 2;
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to rename!");
				return;
			}
			gf.SelectedListName = ItemList.SelectedItems[0].Text;
			bool flag = (gf.SelectedListName == CurFile) ? true : false;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				if (flag)
				{
					CurFile = gf.SelectedListName;
				}
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
					gf.CurSession = CurFile;
				}
				else
				{
					gf.PraiseBooksListChanged = true;
					gf.CurPraiseBook = CurFile;
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
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
				}
				else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					gf.PraiseBooksListChanged = true;
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
			gf.SelectedListName = InputDir + text + Ext_ToUse;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			gf.NameChangeSucceeded = false;
			if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				gf.NameChangeAction = 7;
				if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
				{
					gf.PraiseBooksListChanged = true;
				}
			}
			else
			{
				gf.NameChangeAction = 8;
				if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
				{
					gf.WorshipListsChanged = true;
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
				string text3 = gf.WorshipTemplatesDir + text + ".est";
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
			saveFileDialog1.FileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true);
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
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.CurSession = ItemList.SelectedItems[0].Text;
				}
				else
				{
					gf.CurPraiseBook = ItemList.SelectedItems[0].Text;
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

				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
				}
				else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					gf.PraiseBooksListChanged = true;
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmManageItemLists));
            MainTabControl = new TabControl();
            ListsTab = new TabPage();
            ItemList = new ListView();
            columnHeader1 = new ColumnHeader();
            TrashTab = new TabPage();
            TrashList = new ListView();
            trashColumnHeader1 = new ColumnHeader();
            AddBtn = new Button();
            RenameBtn = new Button();
            DelBtn = new Button();
            CloseBtn = new Button();
            SaveAsBtn = new Button();
            SaveToBtn = new Button();
            SaveTemplateBtn = new Button();
            RestoreBtn = new Button();
            DeletePermanentlyBtn = new Button();
            EmptyTrashBtn = new Button();
            saveFileDialog1 = new SaveFileDialog();
            MainTabControl.SuspendLayout();
            ListsTab.SuspendLayout();
            TrashTab.SuspendLayout();
            SuspendLayout();
            // 
            // MainTabControl
            // 
            MainTabControl.Controls.Add(ListsTab);
            MainTabControl.Controls.Add(TrashTab);
            MainTabControl.Location = new Point(16, 18);
            MainTabControl.Margin = new Padding(4, 5, 4, 5);
            MainTabControl.Name = "MainTabControl";
            MainTabControl.SelectedIndex = 0;
            MainTabControl.Size = new Size(263, 348);
            MainTabControl.TabIndex = 0;
            MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            // 
            // ListsTab
            // 
            ListsTab.Controls.Add(ItemList);
            ListsTab.Location = new Point(4, 29);
            ListsTab.Margin = new Padding(4, 5, 4, 5);
            ListsTab.Name = "ListsTab";
            ListsTab.Padding = new Padding(4, 5, 4, 5);
            ListsTab.Size = new Size(255, 315);
            ListsTab.TabIndex = 0;
            ListsTab.Text = "Lists";
            ListsTab.UseVisualStyleBackColor = true;
            // 
            // ItemList
            // 
            ItemList.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            ItemList.Dock = DockStyle.Fill;
            ItemList.FullRowSelect = true;
            ItemList.HeaderStyle = ColumnHeaderStyle.None;
            ItemList.Location = new Point(4, 5);
            ItemList.Margin = new Padding(4, 5, 4, 5);
            ItemList.Name = "ItemList";
            ItemList.ShowGroups = false;
            ItemList.ShowItemToolTips = true;
            ItemList.Size = new Size(247, 305);
            ItemList.Sorting = SortOrder.Ascending;
            ItemList.TabIndex = 0;
            ItemList.UseCompatibleStateImageBehavior = false;
            ItemList.View = View.Details;
            ItemList.DoubleClick += ItemList_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "";
            columnHeader1.Width = 165;
            // 
            // TrashTab
            // 
            TrashTab.Controls.Add(TrashList);
            TrashTab.Location = new Point(4, 29);
            TrashTab.Margin = new Padding(4, 5, 4, 5);
            TrashTab.Name = "TrashTab";
            TrashTab.Padding = new Padding(4, 5, 4, 5);
            TrashTab.Size = new Size(255, 315);
            TrashTab.TabIndex = 1;
            TrashTab.Text = "Trash";
            TrashTab.UseVisualStyleBackColor = true;
            // 
            // TrashList
            // 
            TrashList.Columns.AddRange(new ColumnHeader[] { trashColumnHeader1 });
            TrashList.Dock = DockStyle.Fill;
            TrashList.FullRowSelect = true;
            TrashList.HeaderStyle = ColumnHeaderStyle.None;
            TrashList.Location = new Point(4, 5);
            TrashList.Margin = new Padding(4, 5, 4, 5);
            TrashList.Name = "TrashList";
            TrashList.ShowGroups = false;
            TrashList.ShowItemToolTips = true;
            TrashList.Size = new Size(247, 305);
            TrashList.Sorting = SortOrder.Ascending;
            TrashList.TabIndex = 0;
            TrashList.UseCompatibleStateImageBehavior = false;
            TrashList.View = View.Details;
            // 
            // trashColumnHeader1
            // 
            trashColumnHeader1.Text = "";
            trashColumnHeader1.Width = 165;
            // 
            // AddBtn
            // 
            AddBtn.Image = Resources.New;
            AddBtn.Location = new Point(287, 18);
            AddBtn.Margin = new Padding(4, 5, 4, 5);
            AddBtn.Name = "AddBtn";
            AddBtn.Size = new Size(117, 37);
            AddBtn.TabIndex = 1;
            AddBtn.Text = "Add New";
            AddBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            AddBtn.Click += AddBtn_Click;
            // 
            // RenameBtn
            // 
            RenameBtn.Image = Resources.editsym;
            RenameBtn.Location = new Point(287, 65);
            RenameBtn.Margin = new Padding(4, 5, 4, 5);
            RenameBtn.Name = "RenameBtn";
            RenameBtn.Size = new Size(117, 37);
            RenameBtn.TabIndex = 2;
            RenameBtn.Text = "Rename";
            RenameBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            RenameBtn.Click += RenameBtn_Click;
            // 
            // DelBtn
            // 
            DelBtn.Image = Resources.Delete;
            DelBtn.Location = new Point(287, 111);
            DelBtn.Margin = new Padding(4, 5, 4, 5);
            DelBtn.Name = "DelBtn";
            DelBtn.Size = new Size(117, 37);
            DelBtn.TabIndex = 3;
            DelBtn.Text = "Delete";
            DelBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            DelBtn.Click += DelBtn_Click;
            // 
            // CloseBtn
            // 
            CloseBtn.DialogResult = DialogResult.OK;
            CloseBtn.Location = new Point(161, 375);
            CloseBtn.Margin = new Padding(4, 5, 4, 5);
            CloseBtn.Name = "CloseBtn";
            CloseBtn.Size = new Size(117, 37);
            CloseBtn.TabIndex = 4;
            CloseBtn.Text = "Close";
            CloseBtn.Click += CloseBtn_Click;
            // 
            // SaveAsBtn
            // 
            SaveAsBtn.Image = Resources.Save;
            SaveAsBtn.Location = new Point(287, 197);
            SaveAsBtn.Margin = new Padding(4, 5, 4, 5);
            SaveAsBtn.Name = "SaveAsBtn";
            SaveAsBtn.Size = new Size(117, 37);
            SaveAsBtn.TabIndex = 5;
            SaveAsBtn.Text = "Save As";
            SaveAsBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            SaveAsBtn.Click += SaveAsBtn_Click;
            // 
            // SaveToBtn
            // 
            SaveToBtn.Image = Resources.Save;
            SaveToBtn.Location = new Point(287, 289);
            SaveToBtn.Margin = new Padding(4, 5, 4, 5);
            SaveToBtn.Name = "SaveToBtn";
            SaveToBtn.Size = new Size(117, 37);
            SaveToBtn.TabIndex = 6;
            SaveToBtn.Text = "WorshipList";
            SaveToBtn.TextAlign = ContentAlignment.MiddleRight;
            SaveToBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            SaveToBtn.Click += SaveToBtn_Click;
            // 
            // SaveTemplateBtn
            // 
            SaveTemplateBtn.Image = Resources.Save;
            SaveTemplateBtn.Location = new Point(287, 243);
            SaveTemplateBtn.Margin = new Padding(4, 5, 4, 5);
            SaveTemplateBtn.Name = "SaveTemplateBtn";
            SaveTemplateBtn.Size = new Size(117, 37);
            SaveTemplateBtn.TabIndex = 7;
            SaveTemplateBtn.Text = "Template";
            SaveTemplateBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            SaveTemplateBtn.Click += SaveTemplateBtn_Click;
            // 
            // RestoreBtn
            // 
            RestoreBtn.Image = Resources.editsym;
            RestoreBtn.Location = new Point(287, 18);
            RestoreBtn.Margin = new Padding(4, 5, 4, 5);
            RestoreBtn.Name = "RestoreBtn";
            RestoreBtn.Size = new Size(117, 37);
            RestoreBtn.TabIndex = 8;
            RestoreBtn.Text = "Restore";
            RestoreBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            RestoreBtn.Visible = false;
            RestoreBtn.Click += RestoreBtn_Click;
            // 
            // DeletePermanentlyBtn
            // 
            DeletePermanentlyBtn.Image = Resources.Delete;
            DeletePermanentlyBtn.Location = new Point(287, 65);
            DeletePermanentlyBtn.Margin = new Padding(4, 5, 4, 5);
            DeletePermanentlyBtn.Name = "DeletePermanentlyBtn";
            DeletePermanentlyBtn.Size = new Size(117, 37);
            DeletePermanentlyBtn.TabIndex = 9;
            DeletePermanentlyBtn.Text = "Delete";
            DeletePermanentlyBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            DeletePermanentlyBtn.Visible = false;
            DeletePermanentlyBtn.Click += DeletePermanentlyBtn_Click;
            // 
            // EmptyTrashBtn
            // 
            EmptyTrashBtn.Location = new Point(287, 111);
            EmptyTrashBtn.Margin = new Padding(4, 5, 4, 5);
            EmptyTrashBtn.Name = "EmptyTrashBtn";
            EmptyTrashBtn.Size = new Size(117, 37);
            EmptyTrashBtn.TabIndex = 10;
            EmptyTrashBtn.Text = "Empty Trash";
            EmptyTrashBtn.Visible = false;
            EmptyTrashBtn.Click += EmptyTrashBtn_Click;
            // 
            // FrmManageItemLists
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 431);
            Controls.Add(EmptyTrashBtn);
            Controls.Add(DeletePermanentlyBtn);
            Controls.Add(RestoreBtn);
            Controls.Add(SaveTemplateBtn);
            Controls.Add(SaveToBtn);
            Controls.Add(SaveAsBtn);
            Controls.Add(CloseBtn);
            Controls.Add(DelBtn);
            Controls.Add(RenameBtn);
            Controls.Add(AddBtn);
            Controls.Add(MainTabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmManageItemLists";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Load += FrmManageItemLists_Load;
            MainTabControl.ResumeLayout(false);
            ListsTab.ResumeLayout(false);
            TrashTab.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
