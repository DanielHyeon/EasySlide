using Easislides.Properties;
using Easislides.Util;
using Easislides.SQLite;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Easislides.Module;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;
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
	public class FrmUsages : Form
	{
		private IContainer components = null;

		private ComboBox SessionList;

		private Button BtnCancel;

		private Button BtnReCalc;

		private Button BtnGenerate;

		private MonthCalendar CalendarFrom;

		private MonthCalendar CalendarTo;

		private Label label1;

		private Label label2;

		private RadioButton BtnOccurrences;

		private RadioButton BtnUsages;

		private ListView UsageDetails;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader2;

		private ColumnHeader columnHeader3;

		private ColumnHeader columnHeader4;

		private ColumnHeader columnHeader5;

		private ColumnHeader columnHeader6;

		private ColumnHeader columnHeader7;

		private ColumnHeader columnHeader8;

		private ListView SummaryDetails;

		private ColumnHeader columnHeader9;

		private ColumnHeader columnHeader10;

		private ColumnHeader columnHeader11;

		private ColumnHeader columnHeader12;

		private ContextMenuStrip CMenuUsageDetails;

		private ToolStripMenuItem CMenuUsageDetails_SelectAll;

		private ToolStripMenuItem CMenuUsageDetails_UnselectAll;

		private ToolStripMenuItem CMenuUsageDetails_Clear;

		private ToolStripMenuItem CMenuUsageDetails_Report;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripSeparator toolStripSeparator2;

		private ToolTip toolTip1;

		private Button BtnDelete;

		private string SelectedSession = "";

		private string CompleteQuery;

		private int sortColumnUsages = -1;

		private int sortColumnSummary = -1;

		private bool SongNumberUsed = false;

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

        public FrmUsages()
		{
			InitializeComponent();
		}

		private void FrmViewUsages_Load(object sender, EventArgs e)
		{
			CalendarFrom.SetDate(DateTime.Now.Subtract(TimeSpan.FromDays(91.0)));
			SessionList.Text = "";
			ReCalc();
		}

		private void LoadSessions()
		{
			if (UsageDetails.Items.Count > 0)
			{
				for (int i = 0; i <= UsageDetails.Items.Count - 1; i++)
				{
					AddItemToSessionList(UsageDetails.Items[i].SubItems[1].Text);
				}
			}
			SessionList.Text = SelectedSession;
		}

		private void AddItemToSessionList(string InSession)
		{
			if (SessionList.Items.Count > 0)
			{
				for (int i = 0; i <= SessionList.Items.Count - 1; i++)
				{
					if (InSession == SessionList.Items[i].ToString())
					{
						return;
					}
				}
			}
			else
			{
				SessionList.Items.Add("");
			}
			SessionList.Items.Add(InSession);
		}

		private bool LoadExtracts()
		{
			if (!gf.ValidateDB(DatabaseType.Usages))
			{
				return false;
			}
			SelectedSession = SessionList.Text;
			SessionList.Items.Clear();
			UsageDetails.Items.Clear();
			UsageDetails.Columns[3].Width = 0;
			SortOrder sorting = UsageDetails.Sorting;
			UsageDetails.Sorting = SortOrder.None;
			ListViewItem listViewItem = new ListViewItem();
			bool result = true;
			string text = "";
			SongNumberUsed = false;
			string text2 = "";
			try
			{
				//daniel 수정 속도를 빠르게 하기위해OledataReader 사용
				//OleDbConnection connection = new OleDbConnection(gf.ConnectStringUsageDB);
				//connection.Open();
				//using (OleDbConnection daoDb = DatabaseController.GetOleDbConnection(gf.ConnectStringUsageDB))
#if OleDb
				using OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringUsageDB);
				
				CompleteQuery = "SELECT * FROM [USAGE] WHERE WORSHIP_DATE >= @WORSHIP_DATE_FROM and WORSHIP_DATE <= @WORSHIP_DATE_TO order by WORSHIP_DATE";
				using OleDbCommand command = new OleDbCommand(CompleteQuery, daoDb);
				command.Parameters.AddWithValue("@WORSHIP_DATE_FROM", CalendarFrom.SelectionStart.Date);
				command.Parameters.AddWithValue("@WORSHIP_DATE_TO", CalendarTo.SelectionStart.Date);
				command.CommandText = CompleteQuery;

				using OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(command);
				DataSet dataSet = new DataSet();
				oleDbDataAdapter.Fill(dataSet);

				using DataTableReader dataTableReader = dataSet.Tables[0].CreateDataReader();
#elif SQLite
				using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringUsageDB);
				CompleteQuery = "SELECT * FROM [USAGE] WHERE WORSHIP_DATE >= @WORSHIP_DATE_FROM and WORSHIP_DATE <= @WORSHIP_DATE_TO order by WORSHIP_DATE";

				DbCommand command = new DbCommand(CompleteQuery, connection);
				command.Parameters.AddWithValue("@WORSHIP_DATE_FROM", CalendarFrom.SelectionStart.Date);
				command.Parameters.AddWithValue("@WORSHIP_DATE_TO", CalendarTo.SelectionStart.Date);
				command.CommandText = CompleteQuery;

				using DbDataAdapter sQLiteDataAdapter = new DbDataAdapter(command);

				using DataTable dataTable = new DataTable();
				sQLiteDataAdapter.Fill(dataTable);				

				using DataTableReader dataTableReader = dataTable.CreateDataReader();
#endif

				while (dataTableReader.Read())
				{
					try
					{
						text2 = DataUtil.ObjToString(dataTableReader["WORSHIP_LIST"]);
						AddItemToSessionList(text2);
						if (SelectedSession == "" || text2 == SelectedSession)
						{
							DateTime dateTime = (DateTime)dataTableReader["WORSHIP_DATE"];
							listViewItem = UsageDetails.Items.Add(dateTime.ToString("yyyy-MM-dd"));
							listViewItem.SubItems.Add(text2);
							listViewItem.SubItems.Add(gf.RemoveMusicSym(DataUtil.ObjToString(dataTableReader["SONG_TITLE"])));
							text = DataUtil.ObjToString(dataTableReader["SONG_NUMBER"]);
							if (text != "" && text != "0")
							{
								SongNumberUsed = true;
							}
							listViewItem.SubItems.Add(text);
							listViewItem.SubItems.Add(DataUtil.ObjToString(dataTableReader["ADMIN_1"]));
							listViewItem.SubItems.Add(DataUtil.ObjToString(dataTableReader["ADMIN_2"]));
							listViewItem.SubItems.Add(DataUtil.ObjToString(dataTableReader["SONG_ID"]));
							listViewItem.SubItems.Add(DataUtil.ObjToString(dataTableReader["REC_ID"]));
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
						result = false;
					}
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				result = false;
			}

			if (SongNumberUsed)
			{
				UsageDetails.Columns[3].Width = 54;
			}
			UsageDetails.Sorting = sorting;
			lv.Sort(ref UsageDetails, ref sortColumnUsages, sortColumnUsages, FlipSort: false);
			SessionList.Text = SelectedSession;
			return result;
		}

		private void LoadSummary()
		{
			SummaryDetails.Columns[2].Width = 0;
			ListViewItem listViewItem = new ListViewItem();
			SummaryDetails.Items.Clear();
			SortOrder sorting = SummaryDetails.Sorting;
			SummaryDetails.Sorting = SortOrder.None;
			if (UsageDetails.Items.Count > 0)
			{
				for (int i = 0; i <= UsageDetails.Items.Count - 1; i++)
				{
					AddItemToRank(i);
				}
			}
			if (SongNumberUsed)
			{
				SummaryDetails.Columns[2].Width = 64;
			}
			SummaryDetails.Sorting = sorting;
			lv.Sort(ref SummaryDetails, ref sortColumnSummary, sortColumnSummary, FlipSort: false);
		}

		private void AddItemToRank(int InIndex)
		{
			try
			{
				string text = UsageDetails.Items[InIndex].SubItems[6].Text;
				if (SummaryDetails.Items.Count > 0)
				{
					for (int i = 0; i <= SummaryDetails.Items.Count - 1; i++)
					{
						if (text == SummaryDetails.Items[i].SubItems[3].Text)
						{
							SummaryDetails.Items[i].SubItems[0].Text = (DataUtil.StringToInt(SummaryDetails.Items[i].SubItems[0].Text) + 1).ToString();
							return;
						}
					}
				}
			}
			catch
			{
			}
			ListViewItem listViewItem = new ListViewItem();
			listViewItem = SummaryDetails.Items.Add("1");
			listViewItem.SubItems.Add(UsageDetails.Items[InIndex].SubItems[2].Text);
			listViewItem.SubItems.Add(UsageDetails.Items[InIndex].SubItems[3].Text);
			listViewItem.SubItems.Add(UsageDetails.Items[InIndex].SubItems[6].Text);
		}

		private void SortSummaryDetails()
		{
			lv.Sort(ref SummaryDetails, ref sortColumnSummary, 0, FlipSort: true);
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BtnReCalc_Click(object sender, EventArgs e)
		{
			ReCalc();
		}

		private void ReCalc()
		{
			LoadExtracts();
			LoadSummary();
		}

		private void CMenuUsageDetails_SelectAll_Click(object sender, EventArgs e)
		{
			UsageDetails_SelectAll();
		}

		private void CMenuUsageDetails_UnselectAll_Click(object sender, EventArgs e)
		{
			UsageDetails_UnselectAll();
		}

		private void CMenuUsageDetails_Clear_Click(object sender, EventArgs e)
		{
			DeleteSelectedUsageDetails();
		}

		private void UsageDetails_SelectAll()
		{
			if (UsageDetails.Items.Count > 0)
			{
				for (int i = 0; i < UsageDetails.Items.Count; i++)
				{
					UsageDetails.Items[i].Selected = true;
				}
			}
		}

		private void UsageDetails_UnselectAll()
		{
			if (UsageDetails.Items.Count > 0)
			{
				for (int i = 0; i < UsageDetails.Items.Count; i++)
				{
					UsageDetails.Items[i].Selected = false;
				}
			}
		}

		private void DeleteSelectedUsageDetails()
		{
			if (!((UsageDetails.Items.Count == 0) | (UsageDetails.SelectedItems.Count == 0)))
			{
				try
				{
					if (MessageBox.Show("This will permanently delete the selected usages records. Please Yes to confirm the delete.", "Deleted Selected Usages", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						for (int num = UsageDetails.SelectedItems.Count - 1; num >= 0; num--)
						{
#if OleDb
							using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringUsageDB))
							{
								OleDbCommand command = new OleDbCommand("Delete * FROM [USAGE] WHERE REC_ID = " + UsageDetails.SelectedItems[num].SubItems[7].Text, daoDb);
								//connection.Open();
								command.ExecuteNonQuery();
								UsageDetails.SelectedItems[num].Remove();
								//connection.Close();
							}
#elif SQLite
							using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringUsageDB);
							DbCommand command = new DbCommand("Delete * FROM [USAGE] WHERE REC_ID = " + UsageDetails.SelectedItems[num].SubItems[7].Text, connection);
							command.ExecuteNonQuery();
							UsageDetails.SelectedItems[num].Remove();
#endif
						}
					}
				}
				catch (Exception ex)	
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
				LoadSummary();
			}
		}

		private void BtnUsages_CheckedChanged(object sender, EventArgs e)
		{
			UsageDetails.Visible = BtnUsages.Checked;
			SummaryDetails.Visible = !UsageDetails.Visible;
		}

		private void UsageDetails_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			lv.Sort(ref UsageDetails, ref sortColumnUsages, e.Column, FlipSort: true);
		}

		private void SummaryDetails_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			lv.Sort(ref SummaryDetails, ref sortColumnSummary, e.Column, FlipSort: true);
		}

		private void BtnGenerate_Click(object sender, EventArgs e)
		{
			GenerateReport();
		}

		private void BtnDelete_Click(object sender, EventArgs e)
		{
			DeleteSelectedUsageDetails();
		}

		private void GenerateReport()
		{
			string text = gf.RootEasiSlidesDir + "Documents\\Song Usages.rtf";
			if (MessageBox.Show("This will overwrite previous report " + text + ". Press OK to proceed or Cancel to quit.", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					Cursor = Cursors.WaitCursor;
					using StreamWriter streamWriter = new StreamWriter(text, append: false, FileUtil.Utf8WithBom);
					try
					{
						streamWriter.AutoFlush = true;

						gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
						gf.RTFIndent[0] = "\\pard\\tx1200\\tx3500\\tx8200\\tx9000 ";
						string text2 = "";
						string value = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind1\\uc1\\pard\\f0\\fs20\\margr600\\margl1000\\margt900\\margb1000 ";
						streamWriter.Write(value);
						streamWriter.Write("\\b\\ul Usage Details: " + gf.RTFNewLine + gf.RTFNewLine);
						streamWriter.Write("\\b Period:\\b0  " + CalendarFrom.SelectionStart.ToString("yyyy-MM-dd") + " to " + CalendarTo.SelectionStart.ToString("yyyy-MM-dd") + " (yyyy-mm-dd)" + gf.RTFNewLine);
						streamWriter.Write(((SelectedSession == "") ? "All Worship Lists displayed" : ("Worship List Restricted to '" + SelectedSession + "'")) + gf.RTFNewLine + gf.RTFNewLine);
						streamWriter.Write(gf.RTFIndent[0] + "\\b Date\tWorship List\tSong Title" + (SongNumberUsed ? "\tNo." : "") + "\tLic Admin" + gf.RTFNewLine);
						for (int i = 0; i <= UsageDetails.Items.Count - 1; i++)
						{
							text2 = UsageDetails.Items[i].SubItems[4].Text;
							if (text2 == "")
							{
								text2 = UsageDetails.Items[i].SubItems[5].Text;
							}
							else if (UsageDetails.Items[i].SubItems[5].Text != "")
							{
								text2 = text2 + "/" + UsageDetails.Items[i].SubItems[5].Text;
							}
							streamWriter.Write(DataUtil.UnicodeToAscii_RTF(gf.RTFIndent[0] + UsageDetails.Items[i].SubItems[0].Text + "\t" + UsageDetails.Items[i].SubItems[1].Text + "\t" + UsageDetails.Items[i].SubItems[2].Text + (SongNumberUsed ? ("\t" + UsageDetails.Items[i].SubItems[3].Text) : "") + "\t" + text2 + gf.RTFNewLine));
						}
						gf.RTFIndent[0] = "\\pard\\tx1200\\tx6200 ";
						streamWriter.Write(gf.RTFNewLine + gf.RTFNewLine + gf.RTFNewLine);
						streamWriter.Write("\\b\\ul Occurrences:" + gf.RTFNewLine + gf.RTFNewLine);
						streamWriter.Write("\\b Period:\\b0  " + CalendarFrom.SelectionStart.ToString("yyyy-MM-dd") + " to " + CalendarTo.SelectionStart.ToString("yyyy-MM-dd") + " (yyyy-mm-dd)" + gf.RTFNewLine);
						streamWriter.Write(((SelectedSession == "") ? "All Worship Lists displayed" : ("Worship List Restricted to '" + SelectedSession + "'")) + gf.RTFNewLine + gf.RTFNewLine);
						streamWriter.Write(gf.RTFIndent[0] + "\\b Occurrence\tSong Title" + (SongNumberUsed ? "\tNo." : "") + gf.RTFNewLine);
						for (int i = 0; i <= SummaryDetails.Items.Count - 1; i++)
						{
							streamWriter.Write(DataUtil.UnicodeToAscii_RTF(gf.RTFIndent[0] + SummaryDetails.Items[i].SubItems[0].Text + "\t" + SummaryDetails.Items[i].SubItems[1].Text + "\t" + (SongNumberUsed ? ("\t" + SummaryDetails.Items[i].SubItems[2].Text) : "") + gf.RTFNewLine));
						}
						streamWriter.Write("}");
						//streamWriter.Flush();
						//streamWriter.Close();
						gf.RunProcess(text);
					}
					catch
					{
						//streamWriter.Flush();
						//streamWriter.Close();
						MessageBox.Show("Error generating report " + text + ". The document might still be open. Please close it first and try again.");
					}
				}
				catch
				{
					MessageBox.Show("Error generating report " + text + ". The document might still be open. Please close it first and try again.");
				}
				Cursor = Cursors.Default;
			}
		}

		private void CMenuUsageDetails_Report_Click(object sender, EventArgs e)
		{
			GenerateReport();
		}
	}
}

