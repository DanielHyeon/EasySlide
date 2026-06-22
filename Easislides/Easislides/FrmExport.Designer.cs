//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using Easislides.Properties;
//using Microsoft.Office.Interop.Access.Dao;
using Easislides.SQLite;
using Easislides.Util;

namespace Easislides
{
    partial class FrmExport
    {
		private Button BtnCancel;
		private Button BtnOK;
		private CheckBox cbFolderListTickAll;
		private CheckBox cbSongsListTickAll;
		private CheckedListBox FolderList;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private DateTimePicker CalendarFrom;
		private DateTimePicker CalendarTo;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private GroupBox groupBox5;
		private IContainer components = null;
		private Label label1;
		private Label label2;
		private Label tbExportTo;
		private ListView SongsList;
		private Panel panelLinkTitle2Lookup;
		private ProgressBar ProgressBar1;
		private RadioButton OptExport0;
		private RadioButton OptExport1;
		private SaveFileDialog saveFileDialog1;
		private ToolStrip toolStrip2;
		private ToolStripButton Export_FileName;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmExport));
            FolderList = new CheckedListBox();
            BtnCancel = new Button();
            BtnOK = new Button();
            cbSongsListTickAll = new CheckBox();
            tbExportTo = new Label();
            ProgressBar1 = new ProgressBar();
            groupBox5 = new GroupBox();
            CalendarTo = new DateTimePicker();
            CalendarFrom = new DateTimePicker();
            OptExport1 = new RadioButton();
            OptExport0 = new RadioButton();
            label2 = new Label();
            label1 = new Label();
            panelLinkTitle2Lookup = new Panel();
            toolStrip2 = new ToolStrip();
            Export_FileName = new ToolStripButton();
            groupBox1 = new GroupBox();
            cbFolderListTickAll = new CheckBox();
            groupBox2 = new GroupBox();
            SongsList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            saveFileDialog1 = new SaveFileDialog();
            groupBox5.SuspendLayout();
            panelLinkTitle2Lookup.SuspendLayout();
            toolStrip2.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            //
            // FolderList
            //
            FolderList.CheckOnClick = true;
            FolderList.FormattingEnabled = true;
            FolderList.Location = new Point(11, 28);
            FolderList.Margin = new Padding(4, 5, 4, 5);
            FolderList.Name = "FolderList";
            FolderList.Size = new Size(244, 158);
            FolderList.TabIndex = 0;
            FolderList.SelectedValueChanged += FolderList_SelectedValueChanged;
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(616, 522);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 6;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(488, 522);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 5;
            BtnOK.Text = "Export";
            BtnOK.Click += BtnOK_Click;
            //
            // cbSongsListTickAll
            //
            cbSongsListTickAll.AutoSize = true;
            cbSongsListTickAll.Location = new Point(19, 411);
            cbSongsListTickAll.Margin = new Padding(4, 5, 4, 5);
            cbSongsListTickAll.Name = "cbSongsListTickAll";
            cbSongsListTickAll.Size = new Size(79, 24);
            cbSongsListTickAll.TabIndex = 1;
            cbSongsListTickAll.Text = "Tick All";
            cbSongsListTickAll.ThreeState = true;
            cbSongsListTickAll.CheckedChanged += cbSongsListTickAll_CheckedChanged;
            //
            // tbExportTo
            //
            tbExportTo.BackColor = SystemColors.Control;
            tbExportTo.Location = new Point(25, 486);
            tbExportTo.Margin = new Padding(4, 0, 4, 0);
            tbExportTo.Name = "tbExportTo";
            tbExportTo.Size = new Size(640, 20);
            tbExportTo.TabIndex = 4;
            //
            // ProgressBar1
            //
            ProgressBar1.Location = new Point(17, 480);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(660, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 3;
            //
            // groupBox5
            //
            groupBox5.Controls.Add(CalendarTo);
            groupBox5.Controls.Add(CalendarFrom);
            groupBox5.Controls.Add(OptExport1);
            groupBox5.Controls.Add(OptExport0);
            groupBox5.Controls.Add(label2);
            groupBox5.Controls.Add(label1);
            groupBox5.Location = new Point(16, 280);
            groupBox5.Margin = new Padding(4, 5, 4, 5);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 5, 4, 5);
            groupBox5.Size = new Size(264, 185);
            groupBox5.TabIndex = 1;
            groupBox5.TabStop = false;
            groupBox5.Text = "List Items from Selected Folders";
            //
            // CalendarTo
            //
            CalendarTo.Location = new Point(51, 140);
            CalendarTo.Margin = new Padding(4, 5, 4, 5);
            CalendarTo.Name = "CalendarTo";
            CalendarTo.Size = new Size(207, 27);
            CalendarTo.TabIndex = 5;
            CalendarTo.ValueChanged += Calendar_ValueChanged;
            //
            // CalendarFrom
            //
            CalendarFrom.Location = new Point(51, 98);
            CalendarFrom.Margin = new Padding(4, 5, 4, 5);
            CalendarFrom.Name = "CalendarFrom";
            CalendarFrom.Size = new Size(207, 27);
            CalendarFrom.TabIndex = 3;
            CalendarFrom.ValueChanged += Calendar_ValueChanged;
            //
            // OptExport1
            //
            OptExport1.AutoSize = true;
            OptExport1.Location = new Point(11, 63);
            OptExport1.Margin = new Padding(4, 5, 4, 5);
            OptExport1.Name = "OptExport1";
            OptExport1.Size = new Size(182, 24);
            OptExport1.TabIndex = 1;
            OptExport1.Text = "Items Added/Updated:";
            //
            // OptExport0
            //
            OptExport0.AutoSize = true;
            OptExport0.Location = new Point(11, 28);
            OptExport0.Margin = new Padding(4, 5, 4, 5);
            OptExport0.Name = "OptExport0";
            OptExport0.Size = new Size(88, 24);
            OptExport0.TabIndex = 0;
            OptExport0.Text = "All Items";
            OptExport0.CheckedChanged += OptExport_CheckedChanged;
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(9, 146);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(28, 20);
            label2.TabIndex = 4;
            label2.Text = "To:";
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(8, 105);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(46, 20);
            label1.TabIndex = 2;
            label1.Text = "From:";
            //
            // panelLinkTitle2Lookup
            //
            panelLinkTitle2Lookup.Controls.Add(toolStrip2);
            panelLinkTitle2Lookup.Location = new Point(685, 478);
            panelLinkTitle2Lookup.Margin = new Padding(4, 5, 4, 5);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(29, 34);
            panelLinkTitle2Lookup.TabIndex = 48;
            //
            // toolStrip2
            //
            toolStrip2.AutoSize = false;
            toolStrip2.CanOverflow = false;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { Export_FileName });
            toolStrip2.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.RenderMode = ToolStripRenderMode.System;
            toolStrip2.Size = new Size(33, 43);
            toolStrip2.TabIndex = 0;
            //
            // Export_FileName
            //
            Export_FileName.AutoSize = false;
            Export_FileName.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Export_FileName.Image = Resources.folder;
            Export_FileName.ImageTransparentColor = Color.Magenta;
            Export_FileName.Name = "Export_FileName";
            Export_FileName.Size = new Size(22, 22);
            Export_FileName.Tag = "";
            Export_FileName.ToolTipText = "Export file name";
            Export_FileName.Click += Export_FileName_Click;
            //
            // groupBox1
            //
            groupBox1.Controls.Add(cbFolderListTickAll);
            groupBox1.Controls.Add(FolderList);
            groupBox1.Location = new Point(16, 18);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(264, 252);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Select Song Folder to Export";
            //
            // cbFolderListTickAll
            //
            cbFolderListTickAll.AutoSize = true;
            cbFolderListTickAll.Location = new Point(12, 205);
            cbFolderListTickAll.Margin = new Padding(4, 5, 4, 5);
            cbFolderListTickAll.Name = "cbFolderListTickAll";
            cbFolderListTickAll.Size = new Size(79, 24);
            cbFolderListTickAll.TabIndex = 1;
            cbFolderListTickAll.Text = "Tick All";
            cbFolderListTickAll.ThreeState = true;
            cbFolderListTickAll.CheckedChanged += cbFolderListTickAll_CheckedChanged;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(SongsList);
            groupBox2.Controls.Add(cbSongsListTickAll);
            groupBox2.Location = new Point(288, 18);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(435, 446);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Items Found:";
            //
            // SongsList
            //
            SongsList.CheckBoxes = true;
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader3, columnHeader4 });
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            SongsList.Location = new Point(8, 29);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new Size(417, 370);
            SongsList.Sorting = SortOrder.Ascending;
            SongsList.TabIndex = 0;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            SongsList.ItemChecked += SongsList_ItemChecked;
            //
            // columnHeader1
            //
            columnHeader1.Text = "Tick the items you wish to Export";
            columnHeader1.Width = 192;
            //
            // columnHeader3
            //
            columnHeader3.DisplayIndex = 2;
            columnHeader3.Text = "ID";
            columnHeader3.Width = 0;
            //
            // columnHeader4
            //
            columnHeader4.DisplayIndex = 1;
            columnHeader4.Text = "Song Folder";
            columnHeader4.Width = 96;
            //
            // FrmExport
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(731, 580);
            Controls.Add(tbExportTo);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(panelLinkTitle2Lookup);
            Controls.Add(groupBox5);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(ProgressBar1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmExport";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Export";
            Load += FrmExport_Load;
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            panelLinkTitle2Lookup.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
