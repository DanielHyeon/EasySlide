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
    partial class FrmManageItemLists
    {
		private Button AddBtn;
		private Button CloseBtn;
		private Button DelBtn;
		private Button DeletePermanentlyBtn;
		private Button EmptyTrashBtn;
		private Button RenameBtn;
		private Button RestoreBtn;
		private Button SaveAsBtn;
		private Button SaveTemplateBtn;
		private Button SaveToBtn;
		private ColumnHeader columnHeader1;
		private ColumnHeader trashColumnHeader1;
		private IContainer components = null;
		private ListView ItemList;
		private ListView TrashList;
		private SaveFileDialog saveFileDialog1;
		private TabControl MainTabControl;
		private TabPage ListsTab;
		private TabPage TrashTab;

protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code

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

        #endregion
    }
}
