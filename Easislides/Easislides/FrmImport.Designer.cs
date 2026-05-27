//using NetOffice.DAOApi;
using Easislides.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Data;
using Easislides.Util;

using Easislides.SQLite;
using Easislides.Module;
#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Easislides
{
    partial class FrmImport
    {
		private Button BtnCancel;
		private Button BtnOK;
		private CheckedListBox ImportFolderList;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private GroupBox groupBox3;
		private GroupBox groupBox4;
		private IContainer components = null;
		private Label tbImportFrom;
		private ListView SongFolder;
		private ListView SongsList;
		private OpenFileDialog OpenFileDialog1;
		private Panel panelLinkTitle2Lookup;
		private ProgressBar ProgressBar1;
		private RadioButton OptImport0;
		private RadioButton OptImport1;
		private RadioButton OptImport2;
		private ToolStrip toolStrip2;
		private ToolStripButton LocationBtn;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmImport));
            groupBox1 = new GroupBox();
            panelLinkTitle2Lookup = new Panel();
            toolStrip2 = new ToolStrip();
            LocationBtn = new ToolStripButton();
            tbImportFrom = new Label();
            ProgressBar1 = new ProgressBar();
            ImportFolderList = new CheckedListBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            SongFolder = new ListView();
            columnHeader1 = new ColumnHeader();
            groupBox4 = new GroupBox();
            OptImport2 = new RadioButton();
            OptImport1 = new RadioButton();
            OptImport0 = new RadioButton();
            BtnCancel = new Button();
            BtnOK = new Button();
            SongsList = new ListView();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            OpenFileDialog1 = new OpenFileDialog();
            groupBox1.SuspendLayout();
            panelLinkTitle2Lookup.SuspendLayout();
            toolStrip2.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(panelLinkTitle2Lookup);
            groupBox1.Controls.Add(tbImportFrom);
            groupBox1.Location = new Point(16, 12);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(693, 75);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "A - Import File:";
            //
            // panelLinkTitle2Lookup
            //
            panelLinkTitle2Lookup.Controls.Add(toolStrip2);
            panelLinkTitle2Lookup.Location = new Point(656, 25);
            panelLinkTitle2Lookup.Margin = new Padding(4, 5, 4, 5);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(29, 34);
            panelLinkTitle2Lookup.TabIndex = 51;
            //
            // toolStrip2
            //
            toolStrip2.AutoSize = false;
            toolStrip2.CanOverflow = false;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { LocationBtn });
            toolStrip2.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.RenderMode = ToolStripRenderMode.System;
            toolStrip2.Size = new Size(33, 43);
            toolStrip2.TabIndex = 5;
            //
            // LocationBtn
            //
            LocationBtn.AutoSize = false;
            LocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LocationBtn.Image = Resources.folder;
            LocationBtn.ImageTransparentColor = Color.Magenta;
            LocationBtn.Name = "LocationBtn";
            LocationBtn.Size = new Size(22, 22);
            LocationBtn.Tag = "";
            LocationBtn.ToolTipText = "Export file name";
            LocationBtn.Click += LocationBtn_Click;
            //
            // tbImportFrom
            //
            tbImportFrom.BackColor = SystemColors.Control;
            tbImportFrom.BorderStyle = BorderStyle.Fixed3D;
            tbImportFrom.Location = new Point(12, 25);
            tbImportFrom.Margin = new Padding(4, 0, 4, 0);
            tbImportFrom.Name = "tbImportFrom";
            tbImportFrom.Size = new Size(640, 34);
            tbImportFrom.TabIndex = 50;
            tbImportFrom.TextAlign = ContentAlignment.MiddleLeft;
            //
            // ProgressBar1
            //
            ProgressBar1.Location = new Point(16, 474);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(693, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 5;
            //
            // ImportFolderList
            //
            ImportFolderList.CheckOnClick = true;
            ImportFolderList.FormattingEnabled = true;
            ImportFolderList.Location = new Point(12, 26);
            ImportFolderList.Margin = new Padding(4, 5, 4, 5);
            ImportFolderList.Name = "ImportFolderList";
            ImportFolderList.Size = new Size(189, 114);
            ImportFolderList.TabIndex = 50;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(ImportFolderList);
            groupBox2.Location = new Point(16, 97);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(213, 157);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "B - Import File Folders (if any)";
            //
            // groupBox3
            //
            groupBox3.Controls.Add(SongFolder);
            groupBox3.Location = new Point(237, 97);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(240, 157);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "C - Select Folder to import into";
            //
            // SongFolder
            //
            SongFolder.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            SongFolder.FullRowSelect = true;
            SongFolder.HeaderStyle = ColumnHeaderStyle.None;
            SongFolder.Location = new Point(8, 25);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.MultiSelect = false;
            SongFolder.Name = "SongFolder";
            SongFolder.ShowGroups = false;
            SongFolder.ShowItemToolTips = true;
            SongFolder.Size = new Size(223, 121);
            SongFolder.TabIndex = 56;
            SongFolder.UseCompatibleStateImageBehavior = false;
            SongFolder.View = View.Details;
            //
            // columnHeader1
            //
            columnHeader1.Text = "";
            columnHeader1.Width = 140;
            //
            // groupBox4
            //
            groupBox4.Controls.Add(OptImport2);
            groupBox4.Controls.Add(OptImport1);
            groupBox4.Controls.Add(OptImport0);
            groupBox4.Location = new Point(485, 97);
            groupBox4.Margin = new Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 5, 4, 5);
            groupBox4.Size = new Size(224, 157);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "D - If Title exists in EasiSlides";
            //
            // OptImport2
            //
            OptImport2.Location = new Point(8, 102);
            OptImport2.Margin = new Padding(4, 5, 4, 5);
            OptImport2.Name = "OptImport2";
            OptImport2.Size = new Size(200, 46);
            OptImport2.TabIndex = 2;
            OptImport2.Text = "IMPORT and REPLACE existing item.";
            //
            // OptImport1
            //
            OptImport1.Location = new Point(8, 54);
            OptImport1.Margin = new Padding(4, 5, 4, 5);
            OptImport1.Name = "OptImport1";
            OptImport1.Size = new Size(200, 51);
            OptImport1.TabIndex = 1;
            OptImport1.Text = "IMPORT but also KEEP existing item";
            //
            // OptImport0
            //
            OptImport0.Checked = true;
            OptImport0.Location = new Point(8, 18);
            OptImport0.Margin = new Padding(4, 5, 4, 5);
            OptImport0.Name = "OptImport0";
            OptImport0.Size = new Size(200, 43);
            OptImport0.TabIndex = 0;
            OptImport0.TabStop = true;
            OptImport0.Text = "DO NOT Import the item";
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(603, 520);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 7;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(475, 520);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 6;
            BtnOK.Text = "Import";
            BtnOK.Click += BtnOK_Click;
            //
            // SongsList
            //
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader2, columnHeader3, columnHeader4 });
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            SongsList.LabelWrap = false;
            SongsList.Location = new Point(16, 262);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new Size(692, 209);
            SongsList.TabIndex = 4;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            //
            // columnHeader2
            //
            columnHeader2.Text = "Item";
            columnHeader2.Width = 54;
            //
            // columnHeader3
            //
            columnHeader3.Text = "Title";
            columnHeader3.Width = 229;
            //
            // columnHeader4
            //
            columnHeader4.Text = "Status";
            columnHeader4.Width = 209;
            //
            // OpenFileDialog1
            //
            OpenFileDialog1.FileName = "openFileDialog1";
            //
            // FrmImport
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(723, 575);
            Controls.Add(SongsList);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(ProgressBar1);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmImport";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Import";
            Load += FrmImport_Load;
            KeyUp += FrmImport_KeyUp;
            groupBox1.ResumeLayout(false);
            panelLinkTitle2Lookup.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
