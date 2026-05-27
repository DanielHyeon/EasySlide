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
	public partial class FrmUsages : Form
	{

		private string SelectedSession = "";

		private string CompleteQuery;

		private int sortColumnUsages = -1;

		private int sortColumnSummary = -1;

		private bool SongNumberUsed = false;

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
			ListViewItem listViewItem = new ListViewItem();
			bool result = true;
			string text = "";
			SongNumberUsed = false;
			string text2 = "";
			try
			{
				//daniel ���� �ӵ��� ������ �ϱ�����OledataReader ���
				//OleDbConnection connection = new OleDbConnection(Gf.ConnectStringUsageDB);
				//connection.Open();
				//using (OleDbConnection daoDb = DatabaseController.GetOleDbConnection(Gf.ConnectStringUsageDB))

				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringUsageDB);
				CompleteQuery = "SELECT * FROM [USAGE] WHERE WORSHIP_DATE >= @WORSHIP_DATE_FROM and WORSHIP_DATE <= @WORSHIP_DATE_TO order by WORSHIP_DATE";

				DbCommand command = new DbCommand(CompleteQuery, connection);
				command.Parameters.AddWithValue("@WORSHIP_DATE_FROM", CalendarFrom.SelectionStart.Date);
				command.Parameters.AddWithValue("@WORSHIP_DATE_TO", CalendarTo.SelectionStart.Date);
				command.CommandText = CompleteQuery;

				using DbDataAdapter sQLiteDataAdapter = new DbDataAdapter(command);

				using DataTable dataTable = new DataTable();
				sQLiteDataAdapter.Fill(dataTable);

				using DataTableReader dataTableReader = dataTable.CreateDataReader();

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
							listViewItem.SubItems.Add(Gf.RemoveMusicSym(DataUtil.ObjToString(dataTableReader["SONG_TITLE"])));
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
			Lv.Sort(ref UsageDetails, ref sortColumnUsages, sortColumnUsages, FlipSort: false);
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
			Lv.Sort(ref SummaryDetails, ref sortColumnSummary, sortColumnSummary, FlipSort: false);
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
					if (MessageBox.Show("This will permanently delete the selected usages records. Please Yes to confirm the delete.", "Deleted Selected Usages", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						for (int num = UsageDetails.SelectedItems.Count - 1; num >= 0; num--)
						{
							using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringUsageDB);
							DbCommand command = new DbCommand("Delete * FROM [USAGE] WHERE REC_ID = " + UsageDetails.SelectedItems[num].SubItems[7].Text, connection);
							command.ExecuteNonQuery();
							UsageDetails.SelectedItems[num].Remove();
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
			string text = Gf.RootEasiSlidesDir + "Documents\\Song Usages.rtf";
			if (MessageBox.Show("This will overwrite previous report " + text + ". Press OK to proceed or Cancel to quit.", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					Cursor = Cursors.WaitCursor;
					using StreamWriter streamWriter = new StreamWriter(text, append: false, FileUtil.Utf8WithBom);
					try
					{
						streamWriter.AutoFlush = true;

						Gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
						Gf.RTFIndent[0] = "\\pard\\tx1200\\tx3500\\tx8200\\tx9000 ";
						string text2 = "";
						string value = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind1\\uc1\\pard\\f0\\fs20\\margr600\\margl1000\\margt900\\margb1000 ";
						streamWriter.Write(value);
						streamWriter.Write("\\b\\ul Usage Details: " + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write("\\b Period:\\b0  " + CalendarFrom.SelectionStart.ToString("yyyy-MM-dd") + " to " + CalendarTo.SelectionStart.ToString("yyyy-MM-dd") + " (yyyy-mm-dd)" + Gf.RTFNewLine);
						streamWriter.Write(((SelectedSession == "") ? "All Worship Lists displayed" : ("Worship List Restricted to '" + SelectedSession + "'")) + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write(Gf.RTFIndent[0] + "\\b Date\tWorship List\tSong Title" + (SongNumberUsed ? "\tNo." : "") + "\tLic Admin" + Gf.RTFNewLine);
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
							streamWriter.Write(DataUtil.UnicodeToAscii_RTF(Gf.RTFIndent[0] + UsageDetails.Items[i].SubItems[0].Text + "\t" + UsageDetails.Items[i].SubItems[1].Text + "\t" + UsageDetails.Items[i].SubItems[2].Text + (SongNumberUsed ? ("\t" + UsageDetails.Items[i].SubItems[3].Text) : "") + "\t" + text2 + Gf.RTFNewLine));
						}
						Gf.RTFIndent[0] = "\\pard\\tx1200\\tx6200 ";
						streamWriter.Write(Gf.RTFNewLine + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write("\\b\\ul Occurrences:" + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write("\\b Period:\\b0  " + CalendarFrom.SelectionStart.ToString("yyyy-MM-dd") + " to " + CalendarTo.SelectionStart.ToString("yyyy-MM-dd") + " (yyyy-mm-dd)" + Gf.RTFNewLine);
						streamWriter.Write(((SelectedSession == "") ? "All Worship Lists displayed" : ("Worship List Restricted to '" + SelectedSession + "'")) + Gf.RTFNewLine + Gf.RTFNewLine);
						streamWriter.Write(Gf.RTFIndent[0] + "\\b Occurrence\tSong Title" + (SongNumberUsed ? "\tNo." : "") + Gf.RTFNewLine);
						for (int i = 0; i <= SummaryDetails.Items.Count - 1; i++)
						{
							streamWriter.Write(DataUtil.UnicodeToAscii_RTF(Gf.RTFIndent[0] + SummaryDetails.Items[i].SubItems[0].Text + "\t" + SummaryDetails.Items[i].SubItems[1].Text + "\t" + (SongNumberUsed ? ("\t" + SummaryDetails.Items[i].SubItems[2].Text) : "") + Gf.RTFNewLine));
						}
						streamWriter.Write("}");
						//streamWriter.Flush();
						//streamWriter.Close();
						Gf.RunProcess(text);
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
