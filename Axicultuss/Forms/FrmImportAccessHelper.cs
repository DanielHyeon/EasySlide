//using NetOffice.DAOApi;
using Axicultus.SQLite;
using Axicultus.Util;
using System;
using System.ComponentModel;
using System.Data;
//using System.Data.SQLite;
using System.Windows.Forms;
using Axicultus.Properties;
using Axicultus.Localization;
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

namespace Axicultus;

public partial class FrmImportAccessHelper : Form
	{
		private bool FormInit = true;

		public FrmImportAccessHelper()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmImportAccessHelper_Load(object sender, EventArgs e)
		{
			Text = UiText.Format(nameof(FrmImportAccessHelper), "TitleWithFile", Gf.Import_AccessFileName);
			Gf.Import_TableName = "";
			Gf.Import_SongTitleColumnName = "";
			Gf.Import_SongTitle2ColumnName = "";
			Gf.Import_SongNumberColumnName = "";
			Gf.Import_BookReferenceColumnName = "";
			Gf.Import_UserReferenceColumnName = "";
			Gf.Import_SongLyricsColumnName = "";
			Gf.Import_SongWriterInfoColumnName = "";
			Gf.Import_SongCopyrightColumnName = "";
			Gf.Import_SongKeyColumnName = "";
			Gf.Import_SongTimingColumnName = "";
			Gf.Import_Admin1ColumnName = "";
			Gf.Import_Admin2ColumnName = "";
			ClearColumns();
			LoadTables();
			FormInit = false;
			if (TablesList.Items.Count > 0)
			{
				TablesList.SelectedIndex = 0;
			}
		}

		private bool LoadTables()
		{

			using DbConnection connection = DbController.GetDbConnection(DbController.ConnectionStringForFile(Gf.Import_AccessFileName, readOnly: true));

			TablesList.Items.Clear();
			try
			{
				string[] restrictions = new string[4];
				restrictions[3] = "Table";
				using DataTable userTables = connection.GetSchema("Tables", restrictions);

				for (int i = 0; i < userTables.Rows.Count; i++)
				{
					string tableDefName = userTables.Rows[i][2].ToString();

					if (!tableDefName.StartsWith("msys", StringComparison.OrdinalIgnoreCase))
					{
						TablesList.Items.Add(tableDefName);
					}
				}
			}
			catch
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportAccessHelper), "ReadError"));
				return false;
			}

			if (TablesList.Items.Count > 0)
			{
				return true;
			}
			MessageBox.Show(UiText.Get(nameof(FrmImportAccessHelper), "NoTables"));

			return false;
		}

		private void LoadColumns()
		{
			string selectedTable = TablesList.Text;
			if (selectedTable == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportAccessHelper), "SelectTable"));
				return;
			}

			using DbConnection connection = DbController.GetDbConnection(DbController.ConnectionStringForFile(Gf.Import_AccessFileName, readOnly: true));

			System.Collections.Generic.List<string> columns;
			try
			{
				columns = DbController.GetColumnNames(connection, selectedTable);
			}
			catch (Exception ex)
			{
				MessageBox.Show(UiText.Format(nameof(FrmImportAccessHelper), "TableReadFailed", ex.Message));
				return;
			}
			if (columns.Count == 0)
			{
				MessageBox.Show(UiText.Format(nameof(FrmImportAccessHelper), "TableNotFound", selectedTable));
				return;
			}
			ClearColumns();
			AssignedTitle.Items.Add("");
			foreach (string name in columns)
			{
				AssignedTitle.Items.Add(name);
			}

			if (AssignedTitle.Items.Count != 0)
			{
				string columnName = "";
				for (int i = 0; i <= AssignedTitle.Items.Count - 1; i++)
				{
					columnName = AssignedTitle.Items[i].ToString();
					AssignedLyrics.Items.Add(columnName);
					AssignedTitle2.Items.Add(columnName);
					AssignedSongNumber.Items.Add(columnName);
					AssignedBookReference.Items.Add(columnName);
					AssignedUserReference.Items.Add(columnName);
					AssignedCopyright.Items.Add(columnName);
					AssignedWriter.Items.Add(columnName);
					AssignedKey.Items.Add(columnName);
					AssignedTiming.Items.Add(columnName);
					AssignedAdmin1.Items.Add(columnName);
					AssignedAdmin2.Items.Add(columnName);
				}
				AssignedTitle.Items.RemoveAt(0);
				AssignedLyrics.Items.RemoveAt(0);
			}
		}

		private void ClearColumns()
		{
			AssignedTitle.Items.Clear();
			AssignedLyrics.Items.Clear();
			AssignedTitle2.Items.Clear();
			AssignedSongNumber.Items.Clear();
			AssignedBookReference.Items.Clear();
			AssignedUserReference.Items.Clear();
			AssignedCopyright.Items.Clear();
			AssignedWriter.Items.Clear();
			AssignedKey.Items.Clear();
			AssignedTiming.Items.Clear();
			AssignedAdmin1.Items.Clear();
			AssignedAdmin2.Items.Clear();
		}

		private bool LoadExtracts()
		{
			if ((AssignedTitle.Items.Count == 0) | (TablesList.SelectedIndex < 0))
			{
				return false;
			}
			TableExtracts.Clear();
			for (int i = 0; i <= AssignedTitle.Items.Count - 1; i++)
			{
				TableExtracts.Columns.Add(AssignedTitle.Items[i].ToString(), 60, HorizontalAlignment.Left);
			}
			for (int i = 0; i <= AssignedTitle.Items.Count - 1; i++)
			{
			}
			ListViewItem listViewItem = new();
			Cursor = Cursors.WaitCursor;
			string fullSearchString = "select * from " + DbController.QuoteIdentifier(TablesList.Text);

			using DataTable datatable = DbController.GetDataTable(DbController.ConnectionStringForFile(Gf.Import_AccessFileName, readOnly: true), fullSearchString);

			int displayedRowCount = 0;
			if (datatable.Rows.Count > 0)
			{
				//recordSet.MoveFirst();
				int totalRowCount = 0;
				totalRowCount = datatable.Rows.Count;

				foreach (DataRow dr in datatable.Rows)
				{
					listViewItem = TableExtracts.Items.Add(DataUtil.ObjToString(dr[AssignedTitle.Items[0].ToString()]));
					for (int i = 1; i <= AssignedTitle.Items.Count - 1; i++)
					{
						listViewItem.SubItems.Add(Convert.ToString(DataUtil.ObjToString(dr[AssignedTitle.Items[i].ToString()])));
					}
					displayedRowCount++;
				}
				label2.Text = (displayedRowCount < totalRowCount) ? UiText.Format(nameof(FrmImportAccessHelper), "RecordsFoundSome", displayedRowCount, totalRowCount) : UiText.Format(nameof(FrmImportAccessHelper), "RecordsFoundAll", totalRowCount);
			}

			Cursor = Cursors.Default;
			return true;
		}

		private void TablesList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!FormInit)
			{
				TablesListIndexChanged();
			}
		}

		private void TablesListIndexChanged()
		{
			if (!((TablesList.Items.Count == 0) | (TablesList.SelectedIndex < 0)))
			{
				LoadColumns();
				LoadExtracts();
			}
		}

		private bool ValidateColumns()
		{
			if (AssignedTitle.Text == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportAccessHelper), "AssignTitle"));
				return false;
			}
			if (AssignedLyricsMergeOrderList.Items.Count == 0)
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportAccessHelper), "MergeListEmpty"));
				return false;
			}
			return true;
		}

		private void Column_MouseUp(object sender, MouseEventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string name = toolStripButton.Name;
			if (name == "Column_Add")
			{
				AddBtn_Click();
			}
		}

		private void AddBtn_Click()
		{
			ListViewItem listViewItem = new();
			for (int i = 0; i <= AssignedLyrics.Items.Count - 1; i++)
			{
				if (AssignedLyrics.Items[i].Selected)
				{
					listViewItem = AssignedLyricsMergeOrderList.Items.Add(AssignedLyrics.Items[i].Text);
				}
			}
		}

		private void OrderList_MouseUp(object sender, MouseEventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string name = toolStripButton.Name;
			if (name == "OrderList_Up")
			{
				MoveUPBtn_Click();
			}
			else if (name == "OrderList_Down")
			{
				MoveDownBtn_Click();
			}
			else if (name == "OrderList_Delete")
			{
				DelBtn_Click();
			}
		}

		private void MoveUPBtn_Click()
		{
			int count = AssignedLyricsMergeOrderList.Items.Count;
			if (count < 1)
			{
				return;
			}
			int selectedIndex = 0;
			for (int i = 0; i <= count - 1; i++)
			{
				if (AssignedLyricsMergeOrderList.Items[i].Selected)
				{
					if (selectedIndex < 1)
					{
						selectedIndex = i;
						continue;
					}
					i = count;
					selectedIndex = 0;
				}
			}
			if (selectedIndex >= 1)
			{
            (AssignedLyricsMergeOrderList.Items[selectedIndex - 1].Text, AssignedLyricsMergeOrderList.Items[selectedIndex].Text) = (AssignedLyricsMergeOrderList.Items[selectedIndex].Text, AssignedLyricsMergeOrderList.Items[selectedIndex - 1].Text);
            AssignedLyricsMergeOrderList.Items[selectedIndex].Selected = false;
				AssignedLyricsMergeOrderList.Items[selectedIndex - 1].Selected = true;
			}
		}

		private void MoveDownBtn_Click()
		{
			int count = AssignedLyricsMergeOrderList.Items.Count;
			if (count <= 1)
			{
				return;
			}
			int selectedIndex = 0;
			for (int i = 0; i <= count - 1; i++)
			{
				if (AssignedLyricsMergeOrderList.Items[i].Selected)
				{
					if (selectedIndex < 1)
					{
						selectedIndex = i;
						continue;
					}
					i = count;
					selectedIndex = -1;
				}
			}
			if (!((selectedIndex < 0) | (selectedIndex == count - 1)))
			{
            (AssignedLyricsMergeOrderList.Items[selectedIndex + 1].Text, AssignedLyricsMergeOrderList.Items[selectedIndex].Text) = (AssignedLyricsMergeOrderList.Items[selectedIndex].Text, AssignedLyricsMergeOrderList.Items[selectedIndex + 1].Text);
            AssignedLyricsMergeOrderList.Items[selectedIndex].Selected = false;
				AssignedLyricsMergeOrderList.Items[selectedIndex + 1].Selected = true;
			}
		}

		private void DelBtn_Click()
		{
			if (AssignedLyricsMergeOrderList.Items.Count == 0)
			{
				return;
			}
			int nextSelectionIndex = 0;
			for (int columnIndex = AssignedLyricsMergeOrderList.Items.Count - 1; columnIndex >= 0; columnIndex--)
			{
				if (AssignedLyricsMergeOrderList.Items[columnIndex].Selected)
				{
					AssignedLyricsMergeOrderList.Items[columnIndex].Remove();
					nextSelectionIndex = columnIndex;
				}
			}
			if (nextSelectionIndex > 0)
			{
				nextSelectionIndex--;
			}
			if (AssignedLyricsMergeOrderList.Items.Count > 0)
			{
				AssignedLyricsMergeOrderList.Items[nextSelectionIndex].Selected = true;
			}
		}

		private void AssignedLyrics_DoubleClick(object sender, EventArgs e)
		{
			if (AssignedLyrics.SelectedItems.Count > 0)
			{
				AddBtn_Click();
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (ValidateColumns())
			{
				Gf.Import_TableName = "[" + TablesList.Text + "]";
				Gf.Import_SongTitleColumnName = "[" + AssignedTitle.Text + "]";
				for (int i = 0; i <= AssignedLyricsMergeOrderList.Items.Count - 1; i++)
				{
					object import_SongLyricsColumnName = Gf.Import_SongLyricsColumnName;
					Gf.Import_SongLyricsColumnName = string.Concat(import_SongLyricsColumnName, "[", AssignedLyricsMergeOrderList.Items[i].Text, "]", '>');
				}
				string unassignedColumn = "";
				Gf.Import_SongTitle2ColumnName = ((AssignedTitle2.Text != "") ? ("[" + AssignedTitle2.Text + "]") : unassignedColumn);
				Gf.Import_SongNumberColumnName = ((AssignedSongNumber.Text != "") ? ("[" + AssignedSongNumber.Text + "]") : unassignedColumn);
				Gf.Import_SongWriterInfoColumnName = ((AssignedWriter.Text != "") ? ("[" + AssignedWriter.Text + "]") : unassignedColumn);
				Gf.Import_BookReferenceColumnName = ((AssignedBookReference.Text != "") ? ("[" + AssignedBookReference.Text + "]") : unassignedColumn);
				Gf.Import_UserReferenceColumnName = ((AssignedUserReference.Text != "") ? ("[" + AssignedUserReference.Text + "]") : unassignedColumn);
				Gf.Import_SongCopyrightColumnName = ((AssignedCopyright.Text != "") ? ("[" + AssignedCopyright.Text + "]") : unassignedColumn);
				Gf.Import_SongKeyColumnName = ((AssignedKey.Text != "") ? ("[" + AssignedKey.Text + "]") : unassignedColumn);
				Gf.Import_SongTimingColumnName = ((AssignedTiming.Text != "") ? ("[" + AssignedTiming.Text + "]") : unassignedColumn);
				Gf.Import_Admin1ColumnName = ((AssignedAdmin1.Text != "") ? ("[" + AssignedAdmin1.Text + "]") : unassignedColumn);
				Gf.Import_Admin2ColumnName = ((AssignedAdmin2.Text != "") ? ("[" + AssignedAdmin2.Text + "]") : unassignedColumn);
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}

		            }
