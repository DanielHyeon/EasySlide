using Easislides.Properties;
using Easislides.Util;
using Easislides.SQLite;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommand = System.Data.SQLite.SQLiteCommand;
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
    partial class FrmUsages
    {
		private Button BtnCancel;
		private Button BtnDelete;
		private Button BtnGenerate;
		private Button BtnReCalc;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader10;
		private ColumnHeader columnHeader11;
		private ColumnHeader columnHeader12;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private ColumnHeader columnHeader5;
		private ColumnHeader columnHeader6;
		private ColumnHeader columnHeader7;
		private ColumnHeader columnHeader8;
		private ColumnHeader columnHeader9;
		private ComboBox SessionList;
		private ContextMenuStrip CMenuUsageDetails;
		private IContainer components = null;
		private Label label1;
		private Label label2;
		private ListView SummaryDetails;
		private ListView UsageDetails;
		private MonthCalendar CalendarFrom;
		private MonthCalendar CalendarTo;
		private RadioButton BtnOccurrences;
		private RadioButton BtnUsages;
		private ToolStripMenuItem CMenuUsageDetails_Clear;
		private ToolStripMenuItem CMenuUsageDetails_Report;
		private ToolStripMenuItem CMenuUsageDetails_SelectAll;
		private ToolStripMenuItem CMenuUsageDetails_UnselectAll;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		private ToolTip toolTip1;

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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmUsages));
            SessionList = new ComboBox();
            BtnCancel = new Button();
            BtnReCalc = new Button();
            BtnGenerate = new Button();
            CalendarFrom = new MonthCalendar();
            CalendarTo = new MonthCalendar();
            label1 = new Label();
            label2 = new Label();
            BtnOccurrences = new RadioButton();
            BtnUsages = new RadioButton();
            UsageDetails = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            columnHeader8 = new ColumnHeader();
            CMenuUsageDetails = new ContextMenuStrip(components);
            CMenuUsageDetails_SelectAll = new ToolStripMenuItem();
            CMenuUsageDetails_UnselectAll = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            CMenuUsageDetails_Clear = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            CMenuUsageDetails_Report = new ToolStripMenuItem();
            SummaryDetails = new ListView();
            columnHeader9 = new ColumnHeader();
            columnHeader10 = new ColumnHeader();
            columnHeader11 = new ColumnHeader();
            columnHeader12 = new ColumnHeader();
            toolTip1 = new ToolTip(components);
            BtnDelete = new Button();
            CMenuUsageDetails.SuspendLayout();
            SuspendLayout();
            //
            // SessionList
            //
            SessionList.DropDownStyle = ComboBoxStyle.DropDownList;
            SessionList.FormattingEnabled = true;
            SessionList.Location = new Point(16, 14);
            SessionList.Margin = new Padding(4, 5, 4, 5);
            SessionList.Name = "SessionList";
            SessionList.Size = new Size(236, 28);
            SessionList.TabIndex = 0;
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(147, 54);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "Close";
            BtnCancel.Click += BtnCancel_Click;
            //
            // BtnReCalc
            //
            BtnReCalc.Location = new Point(16, 54);
            BtnReCalc.Margin = new Padding(4, 5, 4, 5);
            BtnReCalc.Name = "BtnReCalc";
            BtnReCalc.Size = new Size(107, 37);
            BtnReCalc.TabIndex = 4;
            BtnReCalc.Text = "Refresh";
            BtnReCalc.Click += BtnReCalc_Click;
            //
            // BtnGenerate
            //
            BtnGenerate.Image = Resources.document;
            BtnGenerate.Location = new Point(497, 14);
            BtnGenerate.Margin = new Padding(4, 5, 4, 5);
            BtnGenerate.Name = "BtnGenerate";
            BtnGenerate.Size = new Size(32, 37);
            BtnGenerate.TabIndex = 3;
            toolTip1.SetToolTip(BtnGenerate, "Generate Usages Report");
            BtnGenerate.Click += BtnGenerate_Click;
            //
            // CalendarFrom
            //
            CalendarFrom.Location = new Point(16, 122);
            CalendarFrom.Margin = new Padding(12, 14, 12, 14);
            CalendarFrom.Name = "CalendarFrom";
            CalendarFrom.TabIndex = 6;
            //
            // CalendarTo
            //
            CalendarTo.Location = new Point(16, 395);
            CalendarTo.Margin = new Padding(12, 14, 12, 14);
            CalendarTo.Name = "CalendarTo";
            CalendarTo.TabIndex = 7;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(19, 97);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(101, 17);
            label1.TabIndex = 13;
            label1.Text = "Period From:";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(19, 371);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(84, 17);
            label2.TabIndex = 14;
            label2.Text = "Period To:";
            //
            // BtnOccurrences
            //
            BtnOccurrences.Appearance = Appearance.Button;
            BtnOccurrences.Location = new Point(383, 14);
            BtnOccurrences.Margin = new Padding(4, 5, 4, 5);
            BtnOccurrences.Name = "BtnOccurrences";
            BtnOccurrences.Size = new Size(107, 34);
            BtnOccurrences.TabIndex = 2;
            BtnOccurrences.Text = "Occurrences";
            BtnOccurrences.TextAlign = ContentAlignment.MiddleCenter;
            //
            // BtnUsages
            //
            BtnUsages.Appearance = Appearance.Button;
            BtnUsages.Checked = true;
            BtnUsages.Location = new Point(272, 14);
            BtnUsages.Margin = new Padding(4, 5, 4, 5);
            BtnUsages.Name = "BtnUsages";
            BtnUsages.Size = new Size(107, 34);
            BtnUsages.TabIndex = 1;
            BtnUsages.TabStop = true;
            BtnUsages.Text = "Full Details";
            BtnUsages.TextAlign = ContentAlignment.MiddleCenter;
            BtnUsages.CheckedChanged += BtnUsages_CheckedChanged;
            //
            // UsageDetails
            //
            UsageDetails.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7, columnHeader8 });
            UsageDetails.ContextMenuStrip = CMenuUsageDetails;
            UsageDetails.FullRowSelect = true;
            UsageDetails.Location = new Point(272, 54);
            UsageDetails.Margin = new Padding(4, 5, 4, 5);
            UsageDetails.Name = "UsageDetails";
            UsageDetails.Size = new Size(597, 578);
            UsageDetails.TabIndex = 8;
            UsageDetails.UseCompatibleStateImageBehavior = false;
            UsageDetails.View = View.Details;
            UsageDetails.ColumnClick += UsageDetails_ColumnClick;
            //
            // columnHeader1
            //
            columnHeader1.Text = "Date";
            columnHeader1.Width = 68;
            //
            // columnHeader2
            //
            columnHeader2.Text = "Session";
            columnHeader2.Width = 118;
            //
            // columnHeader3
            //
            columnHeader3.Text = "Song Title";
            columnHeader3.Width = 143;
            //
            // columnHeader4
            //
            columnHeader4.Text = "No.";
            columnHeader4.Width = 54;
            //
            // columnHeader5
            //
            columnHeader5.Text = "Admin1";
            //
            // columnHeader6
            //
            columnHeader6.Text = "Admin2";
            columnHeader6.Width = 58;
            //
            // columnHeader7
            //
            columnHeader7.Text = "Song ID";
            columnHeader7.Width = 56;
            //
            // columnHeader8
            //
            columnHeader8.Text = "Sys ID";
            columnHeader8.Width = 53;
            //
            // CMenuUsageDetails
            //
            CMenuUsageDetails.ImageScalingSize = new Size(20, 20);
            CMenuUsageDetails.Items.AddRange(new ToolStripItem[] { CMenuUsageDetails_SelectAll, CMenuUsageDetails_UnselectAll, toolStripSeparator1, CMenuUsageDetails_Clear, toolStripSeparator2, CMenuUsageDetails_Report });
            CMenuUsageDetails.Name = "ContextMenuBibleText";
            CMenuUsageDetails.Size = new Size(286, 112);
            //
            // CMenuUsageDetails_SelectAll
            //
            CMenuUsageDetails_SelectAll.Name = "CMenuUsageDetails_SelectAll";
            CMenuUsageDetails_SelectAll.Size = new Size(285, 24);
            CMenuUsageDetails_SelectAll.Text = "Select &All";
            CMenuUsageDetails_SelectAll.Click += CMenuUsageDetails_SelectAll_Click;
            //
            // CMenuUsageDetails_UnselectAll
            //
            CMenuUsageDetails_UnselectAll.Name = "CMenuUsageDetails_UnselectAll";
            CMenuUsageDetails_UnselectAll.Size = new Size(285, 24);
            CMenuUsageDetails_UnselectAll.Text = "&Unselect All";
            CMenuUsageDetails_UnselectAll.Click += CMenuUsageDetails_UnselectAll_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(282, 6);
            //
            // CMenuUsageDetails_Clear
            //
            CMenuUsageDetails_Clear.Name = "CMenuUsageDetails_Clear";
            CMenuUsageDetails_Clear.Size = new Size(285, 24);
            CMenuUsageDetails_Clear.Text = "Delete Selected Usage Records";
            CMenuUsageDetails_Clear.Click += CMenuUsageDetails_Clear_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(282, 6);
            //
            // CMenuUsageDetails_Report
            //
            CMenuUsageDetails_Report.Name = "CMenuUsageDetails_Report";
            CMenuUsageDetails_Report.Size = new Size(285, 24);
            CMenuUsageDetails_Report.Text = "Generate Usage Report";
            CMenuUsageDetails_Report.Click += CMenuUsageDetails_Report_Click;
            //
            // SummaryDetails
            //
            SummaryDetails.Columns.AddRange(new ColumnHeader[] { columnHeader9, columnHeader10, columnHeader11, columnHeader12 });
            SummaryDetails.FullRowSelect = true;
            SummaryDetails.Location = new Point(272, 54);
            SummaryDetails.Margin = new Padding(4, 5, 4, 5);
            SummaryDetails.MultiSelect = false;
            SummaryDetails.Name = "SummaryDetails";
            SummaryDetails.Size = new Size(597, 578);
            SummaryDetails.TabIndex = 9;
            SummaryDetails.UseCompatibleStateImageBehavior = false;
            SummaryDetails.View = View.Details;
            SummaryDetails.ColumnClick += SummaryDetails_ColumnClick;
            //
            // columnHeader9
            //
            columnHeader9.Text = "Occurrence";
            columnHeader9.Width = 71;
            //
            // columnHeader10
            //
            columnHeader10.Text = "Song Title";
            columnHeader10.Width = 248;
            //
            // columnHeader11
            //
            columnHeader11.Text = "No.";
            columnHeader11.Width = 64;
            //
            // columnHeader12
            //
            columnHeader12.Text = "ID";
            //
            // BtnDelete
            //
            BtnDelete.Image = Resources.Delete;
            BtnDelete.Location = new Point(533, 14);
            BtnDelete.Margin = new Padding(4, 5, 4, 5);
            BtnDelete.Name = "BtnDelete";
            BtnDelete.Size = new Size(32, 37);
            BtnDelete.TabIndex = 15;
            toolTip1.SetToolTip(BtnDelete, "Delete Selected Usage Records");
            BtnDelete.Click += BtnDelete_Click;
            //
            // FrmUsages
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(883, 651);
            Controls.Add(BtnDelete);
            Controls.Add(BtnOccurrences);
            Controls.Add(BtnUsages);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(CalendarTo);
            Controls.Add(CalendarFrom);
            Controls.Add(BtnGenerate);
            Controls.Add(BtnReCalc);
            Controls.Add(BtnCancel);
            Controls.Add(SessionList);
            Controls.Add(UsageDetails);
            Controls.Add(SummaryDetails);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmUsages";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Usages";
            Load += FrmViewUsages_Load;
            CMenuUsageDetails.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
