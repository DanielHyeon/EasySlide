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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmUsages));
			SessionList = new System.Windows.Forms.ComboBox();
			BtnCancel = new System.Windows.Forms.Button();
			BtnReCalc = new System.Windows.Forms.Button();
			BtnGenerate = new System.Windows.Forms.Button();
			CalendarFrom = new System.Windows.Forms.MonthCalendar();
			CalendarTo = new System.Windows.Forms.MonthCalendar();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			BtnOccurrences = new System.Windows.Forms.RadioButton();
			BtnUsages = new System.Windows.Forms.RadioButton();
			UsageDetails = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			columnHeader7 = new System.Windows.Forms.ColumnHeader();
			columnHeader8 = new System.Windows.Forms.ColumnHeader();
			CMenuUsageDetails = new System.Windows.Forms.ContextMenuStrip(components);
			CMenuUsageDetails_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
			CMenuUsageDetails_UnselectAll = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			CMenuUsageDetails_Clear = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			CMenuUsageDetails_Report = new System.Windows.Forms.ToolStripMenuItem();
			SummaryDetails = new System.Windows.Forms.ListView();
			columnHeader9 = new System.Windows.Forms.ColumnHeader();
			columnHeader10 = new System.Windows.Forms.ColumnHeader();
			columnHeader11 = new System.Windows.Forms.ColumnHeader();
			columnHeader12 = new System.Windows.Forms.ColumnHeader();
			toolTip1 = new System.Windows.Forms.ToolTip(components);
			BtnDelete = new System.Windows.Forms.Button();
			CMenuUsageDetails.SuspendLayout();
			SuspendLayout();
			SessionList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SessionList.FormattingEnabled = true;
			SessionList.Location = new System.Drawing.Point(12, 9);
			SessionList.Name = "SessionList";
			SessionList.Size = new System.Drawing.Size(178, 21);
			SessionList.TabIndex = 0;
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(110, 35);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 5;
			BtnCancel.Text = "Close";
			BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
			BtnReCalc.Location = new System.Drawing.Point(12, 35);
			BtnReCalc.Name = "BtnReCalc";
			BtnReCalc.Size = new System.Drawing.Size(80, 24);
			BtnReCalc.TabIndex = 4;
			BtnReCalc.Text = "Refresh";
			BtnReCalc.Click += new System.EventHandler(BtnReCalc_Click);
			BtnGenerate.Image = Resources.document;
			BtnGenerate.Location = new System.Drawing.Point(373, 9);
			BtnGenerate.Name = "BtnGenerate";
			BtnGenerate.Size = new System.Drawing.Size(24, 24);
			BtnGenerate.TabIndex = 3;
			toolTip1.SetToolTip(BtnGenerate, "Generate Usages Report");
			BtnGenerate.Click += new System.EventHandler(BtnGenerate_Click);
			CalendarFrom.Location = new System.Drawing.Point(12, 79);
			CalendarFrom.Name = "CalendarFrom";
			CalendarFrom.TabIndex = 6;
			CalendarTo.Location = new System.Drawing.Point(12, 257);
			CalendarTo.Name = "CalendarTo";
			CalendarTo.TabIndex = 7;
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			label1.Location = new System.Drawing.Point(14, 63);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(78, 13);
			label1.TabIndex = 13;
			label1.Text = "Period From:";
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			label2.Location = new System.Drawing.Point(14, 241);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(66, 13);
			label2.TabIndex = 14;
			label2.Text = "Period To:";
			BtnOccurrences.Appearance = System.Windows.Forms.Appearance.Button;
			BtnOccurrences.Location = new System.Drawing.Point(287, 9);
			BtnOccurrences.Name = "BtnOccurrences";
			BtnOccurrences.Size = new System.Drawing.Size(80, 22);
			BtnOccurrences.TabIndex = 2;
			BtnOccurrences.Text = "Occurrences";
			BtnOccurrences.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			BtnUsages.Appearance = System.Windows.Forms.Appearance.Button;
			BtnUsages.Checked = true;
			BtnUsages.Location = new System.Drawing.Point(204, 9);
			BtnUsages.Name = "BtnUsages";
			BtnUsages.Size = new System.Drawing.Size(80, 22);
			BtnUsages.TabIndex = 1;
			BtnUsages.TabStop = true;
			BtnUsages.Text = "Full Details";
			BtnUsages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			BtnUsages.CheckedChanged += new System.EventHandler(BtnUsages_CheckedChanged);
			UsageDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[8]
			{
				columnHeader1,
				columnHeader2,
				columnHeader3,
				columnHeader4,
				columnHeader5,
				columnHeader6,
				columnHeader7,
				columnHeader8
			});
			UsageDetails.ContextMenuStrip = CMenuUsageDetails;
			UsageDetails.FullRowSelect = true;
			UsageDetails.Location = new System.Drawing.Point(204, 35);
			UsageDetails.Name = "UsageDetails";
			UsageDetails.Size = new System.Drawing.Size(449, 377);
			UsageDetails.TabIndex = 8;
			UsageDetails.UseCompatibleStateImageBehavior = false;
			UsageDetails.View = System.Windows.Forms.View.Details;
			UsageDetails.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(UsageDetails_ColumnClick);
			columnHeader1.Text = "Date";
			columnHeader1.Width = 68;
			columnHeader2.Text = "Session";
			columnHeader2.Width = 118;
			columnHeader3.Text = "Song Title";
			columnHeader3.Width = 143;
			columnHeader4.Text = "No.";
			columnHeader4.Width = 54;
			columnHeader5.Text = "Admin1";
			columnHeader6.Text = "Admin2";
			columnHeader6.Width = 58;
			columnHeader7.Text = "Song ID";
			columnHeader7.Width = 56;
			columnHeader8.Text = "Sys ID";
			columnHeader8.Width = 53;
			CMenuUsageDetails.Items.AddRange(new System.Windows.Forms.ToolStripItem[6]
			{
				CMenuUsageDetails_SelectAll,
				CMenuUsageDetails_UnselectAll,
				toolStripSeparator1,
				CMenuUsageDetails_Clear,
				toolStripSeparator2,
				CMenuUsageDetails_Report
			});
			CMenuUsageDetails.Name = "ContextMenuBibleText";
			CMenuUsageDetails.Size = new System.Drawing.Size(225, 104);
			CMenuUsageDetails_SelectAll.Name = "CMenuUsageDetails_SelectAll";
			CMenuUsageDetails_SelectAll.Size = new System.Drawing.Size(224, 22);
			CMenuUsageDetails_SelectAll.Text = "Select &All";
			CMenuUsageDetails_SelectAll.Click += new System.EventHandler(CMenuUsageDetails_SelectAll_Click);
			CMenuUsageDetails_UnselectAll.Name = "CMenuUsageDetails_UnselectAll";
			CMenuUsageDetails_UnselectAll.Size = new System.Drawing.Size(224, 22);
			CMenuUsageDetails_UnselectAll.Text = "&Unselect All";
			CMenuUsageDetails_UnselectAll.Click += new System.EventHandler(CMenuUsageDetails_UnselectAll_Click);
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(221, 6);
			CMenuUsageDetails_Clear.Name = "CMenuUsageDetails_Clear";
			CMenuUsageDetails_Clear.Size = new System.Drawing.Size(224, 22);
			CMenuUsageDetails_Clear.Text = "Delete Selected Usage Records";
			CMenuUsageDetails_Clear.Click += new System.EventHandler(CMenuUsageDetails_Clear_Click);
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(221, 6);
			CMenuUsageDetails_Report.Name = "CMenuUsageDetails_Report";
			CMenuUsageDetails_Report.Size = new System.Drawing.Size(224, 22);
			CMenuUsageDetails_Report.Text = "Generate Usage Report";
			CMenuUsageDetails_Report.Click += new System.EventHandler(CMenuUsageDetails_Report_Click);
			SummaryDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4]
			{
				columnHeader9,
				columnHeader10,
				columnHeader11,
				columnHeader12
			});
			SummaryDetails.FullRowSelect = true;
			SummaryDetails.Location = new System.Drawing.Point(204, 35);
			SummaryDetails.MultiSelect = false;
			SummaryDetails.Name = "SummaryDetails";
			SummaryDetails.Size = new System.Drawing.Size(449, 377);
			SummaryDetails.TabIndex = 9;
			SummaryDetails.UseCompatibleStateImageBehavior = false;
			SummaryDetails.View = System.Windows.Forms.View.Details;
			SummaryDetails.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(SummaryDetails_ColumnClick);
			columnHeader9.Text = "Occurrence";
			columnHeader9.Width = 71;
			columnHeader10.Text = "Song Title";
			columnHeader10.Width = 248;
			columnHeader11.Text = "No.";
			columnHeader11.Width = 64;
			columnHeader12.Text = "ID";
			BtnDelete.Image = Resources.Delete;
			BtnDelete.Location = new System.Drawing.Point(400, 9);
			BtnDelete.Name = "BtnDelete";
			BtnDelete.Size = new System.Drawing.Size(24, 24);
			BtnDelete.TabIndex = 15;
			toolTip1.SetToolTip(BtnDelete, "Delete Selected Usage Records");
			BtnDelete.Click += new System.EventHandler(BtnDelete_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(662, 423);
			base.Controls.Add(BtnDelete);
			base.Controls.Add(BtnOccurrences);
			base.Controls.Add(BtnUsages);
			base.Controls.Add(label2);
			base.Controls.Add(label1);
			base.Controls.Add(CalendarTo);
			base.Controls.Add(CalendarFrom);
			base.Controls.Add(BtnGenerate);
			base.Controls.Add(BtnReCalc);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(SessionList);
			base.Controls.Add(UsageDetails);
			base.Controls.Add(SummaryDetails);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmUsages";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Usages";
			base.Load += new System.EventHandler(FrmViewUsages_Load);
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
					using StreamWriter streamWriter = new StreamWriter(text, append: false, Encoding.Default);
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
