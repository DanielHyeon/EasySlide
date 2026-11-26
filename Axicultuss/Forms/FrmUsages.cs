using Axicultus.Localization;
using Axicultus.Properties;
using Axicultus.Util;
using Axicultus.SQLite;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Axicultus.Module;

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

namespace Axicultus;

	public partial class FrmUsages : Form
	{

		private string SelectedSession = "";


		private int sortColumnUsages = -1;

		private int sortColumnSummary = -1;

		private bool SongNumberUsed = false;

		public FrmUsages()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
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

		// The shipped EsUsage.db declares REC_ID INTEGER without a key, so the rows this program adds have no REC_ID
		// (the insert leaves it out). Such a row is known by its rowid, negated so that it cannot be taken for
		// another row's REC_ID. The WPF app reads and deletes usage rows by the same key.
		private const string UsageRecordKey = "COALESCE([USAGE].REC_ID, -[USAGE].rowid)";

		// WORSHIP_DATE is a TEXT column: "yyyy-MM-dd" in rows converted from the old database and
		// "yyyy-MM-dd HH:mm:ss" in rows written with a DateTime parameter. The date part is compared,
		// so both forms are found on the first and last day of the range.
		internal static DbCommand CreateUsageQuery(DbConnection connection, DateTime from, DateTime to)
		{
			return DbController.CreateCommand(connection,
				"SELECT WORSHIP_DATE, WORSHIP_LIST, SONG_TITLE, SONG_NUMBER, ADMIN_1, ADMIN_2, SONG_ID, " + UsageRecordKey + " AS REC_ID FROM [USAGE] "
				+ "WHERE substr(WORSHIP_DATE, 1, 10) >= @from AND substr(WORSHIP_DATE, 1, 10) <= @to ORDER BY substr(WORSHIP_DATE, 1, 10), [USAGE].REC_ID IS NULL, [USAGE].REC_ID, rowid",
				("@from", DataUtil.ToStoredDate(from)), ("@to", DataUtil.ToStoredDate(to)));
		}

		// The reader returns the stored text, not a DateTime (a cast threw, and no row was listed).
		internal static string UsageDateText(object storedDate)
		{
			return DataUtil.TryParseStoredDate(storedDate, out DateTime date) ? DataUtil.ToStoredDate(date) : DataUtil.ObjToString(storedDate);
		}

		private bool LoadExtracts()
		{
			if (!Gf.ValidateDB(DatabaseType.Usages))
			{
				return false;
			}
			SelectedSession = SessionList.Text;
			SessionList.Items.Clear();
			UsageDetails.Items.Clear();
			UsageDetails.Columns[3].Width = 0;
			SortOrder sorting = UsageDetails.Sorting;
			UsageDetails.Sorting = SortOrder.None;
			ListViewItem listViewItem = new();
			bool result = true;
			string songNumber = "";
			SongNumberUsed = false;
			string worshipListName = "";
			try
			{
				//daniel ���� �ӵ��� ������ �ϱ�����OledataReader ���
				//OleDbConnection connection = new OleDbConnection(Gf.ConnectStringUsageDB);
				//connection.Open();
				//using (OleDbConnection daoDb = DatabaseController.GetOleDbConnection(Gf.ConnectStringUsageDB))

				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringUsageDB);
				using var command = CreateUsageQuery(connection, CalendarFrom.SelectionStart, CalendarTo.SelectionStart);
				using var dataTableReader = command.ExecuteReader();

				while (dataTableReader.Read())
				{
					try
					{
						worshipListName = DataUtil.ObjToString(dataTableReader["WORSHIP_LIST"]);
						AddItemToSessionList(worshipListName);
						if (SelectedSession == "" || worshipListName == SelectedSession)
						{
							listViewItem = UsageDetails.Items.Add(UsageDateText(dataTableReader["WORSHIP_DATE"]));
							listViewItem.SubItems.Add(worshipListName);
							listViewItem.SubItems.Add(Gf.RemoveMusicSym(DataUtil.ObjToString(dataTableReader["SONG_TITLE"])));
							songNumber = DataUtil.ObjToString(dataTableReader["SONG_NUMBER"]);
							if (songNumber is not "" and not "0")
							{
								SongNumberUsed = true;
							}
							listViewItem.SubItems.Add(songNumber);
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
			Lv.Sort(ref UsageDetails, ref sortColumnUsages, sortColumnUsages, FlipSort: false);
			SessionList.Text = SelectedSession;
			return result;
		}

		private void LoadSummary()
		{
			SummaryDetails.Columns[2].Width = 0;
			ListViewItem listViewItem = new();
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
			Lv.Sort(ref SummaryDetails, ref sortColumnSummary, sortColumnSummary, FlipSort: false);
		}

		private void AddItemToRank(int InIndex)
		{
			try
			{
				string songId = UsageDetails.Items[InIndex].SubItems[6].Text;
				if (SummaryDetails.Items.Count > 0)
				{
					for (int i = 0; i <= SummaryDetails.Items.Count - 1; i++)
					{
						if (songId == SummaryDetails.Items[i].SubItems[3].Text)
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
			ListViewItem listViewItem = new();
			listViewItem = SummaryDetails.Items.Add("1");
			listViewItem.SubItems.Add(UsageDetails.Items[InIndex].SubItems[2].Text);
			listViewItem.SubItems.Add(UsageDetails.Items[InIndex].SubItems[3].Text);
			listViewItem.SubItems.Add(UsageDetails.Items[InIndex].SubItems[6].Text);
		}

		private void SortSummaryDetails()
		{
			Lv.Sort(ref SummaryDetails, ref sortColumnSummary, 0, FlipSort: true);
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
					if (MessageBox.Show(UiText.Get(nameof(FrmUsages), "DeleteConfirm"), UiText.Get(nameof(FrmUsages), "DeleteConfirmTitle"), MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						var rows = new System.Collections.Generic.List<ListViewItem>();
						var ids = new System.Collections.Generic.List<long>();
						foreach (ListViewItem row in UsageDetails.SelectedItems)
						{
							ids.Add(long.Parse(row.SubItems[7].Text));
							rows.Add(row);
						}
						using var connection = DbController.GetDbConnection(Gf.ConnectStringUsageDB);
						DeleteUsageRecords(connection, ids);
						foreach (var row in rows)
							row.Remove();
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

		internal static void DeleteUsageRecords(DbConnection connection, System.Collections.Generic.IEnumerable<long> ids)
		{
			using var transaction = connection.BeginTransaction();
			using var command = new DbCommand("DELETE FROM [USAGE] WHERE " + UsageRecordKey + "=@id", connection, transaction);
			var idParameter = command.Parameters.Add("@id", DbType.Int64);
			foreach (long id in ids)
			{
				idParameter.Value = id;
				command.ExecuteNonQuery();
			}
			transaction.Commit();
		}

		private void BtnUsages_CheckedChanged(object sender, EventArgs e)
		{
			UsageDetails.Visible = BtnUsages.Checked;
			SummaryDetails.Visible = !UsageDetails.Visible;
		}

		private void UsageDetails_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			Lv.Sort(ref UsageDetails, ref sortColumnUsages, e.Column, FlipSort: true);
		}

		private void SummaryDetails_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			Lv.Sort(ref SummaryDetails, ref sortColumnSummary, e.Column, FlipSort: true);
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
			string reportPath = Gf.RootAxicultusDir + "Documents\\Song Usages.rtf";
			if (MessageBox.Show(UiText.Format(nameof(FrmUsages), "OverwriteReport", reportPath), "", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					Cursor = Cursors.WaitCursor;
					using StreamWriter streamWriter = new(reportPath, append: false, FileUtil.Utf8WithBom);
					try
					{
						streamWriter.AutoFlush = true;

						const string detailTabs = "\\pard\\tx1200\\tx3500\\tx8200\\tx9000 ";
						string licenseAdministrators = "";
						string value = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind1\\uc1\\pard\\f0\\fs20\\margr600\\margl1000\\margt900\\margb1000 ";
						streamWriter.Write(value);
						streamWriter.Write("\\b\\ul Usage Details: " + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write("\\b Period:\\b0  " + CalendarFrom.SelectionStart.ToString("yyyy-MM-dd") + " to " + CalendarTo.SelectionStart.ToString("yyyy-MM-dd") + " (yyyy-mm-dd)" + Gf.RTFNewLine);
						streamWriter.Write(((SelectedSession == "") ? "All Worship Lists displayed" : ("Worship List Restricted to '" + SelectedSession + "'")) + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write(detailTabs + "\\b Date\tWorship List\tSong Title" + (SongNumberUsed ? "\tNo." : "") + "\tLic Admin" + Gf.RTFNewLine);
						for (int i = 0; i <= UsageDetails.Items.Count - 1; i++)
						{
							licenseAdministrators = UsageDetails.Items[i].SubItems[4].Text;
							if (licenseAdministrators == "")
							{
								licenseAdministrators = UsageDetails.Items[i].SubItems[5].Text;
							}
							else if (UsageDetails.Items[i].SubItems[5].Text != "")
							{
								licenseAdministrators = licenseAdministrators + "/" + UsageDetails.Items[i].SubItems[5].Text;
							}
							streamWriter.Write(DataUtil.UnicodeToAscii_RTF(detailTabs + UsageDetails.Items[i].SubItems[0].Text + "\t" + UsageDetails.Items[i].SubItems[1].Text + "\t" + UsageDetails.Items[i].SubItems[2].Text + (SongNumberUsed ? ("\t" + UsageDetails.Items[i].SubItems[3].Text) : "") + "\t" + licenseAdministrators + Gf.RTFNewLine));
						}
						const string summaryTabs = "\\pard\\tx1200\\tx6200 ";
						streamWriter.Write(Gf.RTFNewLine + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write("\\b\\ul Occurrences:" + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write("\\b Period:\\b0  " + CalendarFrom.SelectionStart.ToString("yyyy-MM-dd") + " to " + CalendarTo.SelectionStart.ToString("yyyy-MM-dd") + " (yyyy-mm-dd)" + Gf.RTFNewLine);
						streamWriter.Write(((SelectedSession == "") ? "All Worship Lists displayed" : ("Worship List Restricted to '" + SelectedSession + "'")) + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write(summaryTabs + "\\b Occurrence\tSong Title" + (SongNumberUsed ? "\tNo." : "") + Gf.RTFNewLine);
						for (int i = 0; i <= SummaryDetails.Items.Count - 1; i++)
						{
							streamWriter.Write(DataUtil.UnicodeToAscii_RTF(summaryTabs + SummaryDetails.Items[i].SubItems[0].Text + "\t" + SummaryDetails.Items[i].SubItems[1].Text + "\t" + (SongNumberUsed ? ("\t" + SummaryDetails.Items[i].SubItems[2].Text) : "") + Gf.RTFNewLine));
						}
						streamWriter.Write("}");
						//streamWriter.Flush();
						//streamWriter.Close();
						Gf.RunProcess(reportPath);
					}
					catch
					{
						//streamWriter.Flush();
						//streamWriter.Close();
						MessageBox.Show(UiText.Format(nameof(FrmUsages), "ReportFailed", reportPath));
					}
				}
				catch
				{
					MessageBox.Show(UiText.Format(nameof(FrmUsages), "ReportFailed", reportPath));
				}
				Cursor = Cursors.Default;
			}
		}

		private void CMenuUsageDetails_Report_Click(object sender, EventArgs e)
		{
			GenerateReport();
		}
	}
