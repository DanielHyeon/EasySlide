//using NetOffice.DAOApi;
using Easislides.SQLite;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
//using System.Data.SQLite;
using System.Windows.Forms;
using Easislides.Properties;
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
    public partial class FrmImportAccessHelper : Form
	{
		private bool FormInit = true;

		public FrmImportAccessHelper()
		{
			InitializeComponent();
		}

		private void FrmImportAccessHelper_Load(object sender, EventArgs e)
		{
			Text = "Access Helper: " + Gf.Import_AccessFileName;
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

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringSQLiteDef + Gf.Import_AccessFileName);

			TablesList.Items.Clear();
			try
			{
				string[] restrictions = new string[4];
				restrictions[3] = "Table";
				using DataTable userTables = connection.GetSchema("Tables", restrictions);

				for (int i = 0; i < userTables.Rows.Count; i++)
				{
					string tableDefName = userTables.Rows[i][2].ToString();

					if (DataUtil.Left(tableDefName, 4).ToLower() != "msys")
					{
						TablesList.Items.Add(tableDefName);
					}
				}
			}
			catch
			{
				MessageBox.Show("There was an error reading the Access Database File - please make sure its a proper Access Database File filled with data");
				return false;
			}

			if (TablesList.Items.Count > 0)
			{
				return true;
			}
			MessageBox.Show("Sorry - the Access Database File does not contain any tables. Please quit out of this Helper.");

			return false;
		}

		public bool TableNameExists(DbConnection connection, string TableName)
		{
			try
			{
				DataTable tableDef = connection.GetSchema(TableName);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void LoadColumns()
		{
			string text = TablesList.Text;
			if (text == "")
			{
				MessageBox.Show("Please select a Database Table under Step 1.");
				return;
			}

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringSQLiteDef + Gf.Import_AccessFileName);

			if (!TableNameExists(connection, text))
			{
				MessageBox.Show("Error Encountered - Cannot find the table " + text + " in the Access Database");
				return;
			}
			ClearColumns();
			AssignedTitle.Items.Add("");
			foreach (DataColumn field in connection.GetSchema(text).Columns)
			{
				AssignedTitle.Items.Add(field.ColumnName);
			}

			if (AssignedTitle.Items.Count != 0)
			{
				string text2 = "";
				for (int i = 0; i <= AssignedTitle.Items.Count - 1; i++)
				{
					text2 = AssignedTitle.Items[i].ToString();
					AssignedLyrics.Items.Add(text2);
					AssignedTitle2.Items.Add(text2);
					AssignedSongNumber.Items.Add(text2);
					AssignedBookReference.Items.Add(text2);
					AssignedUserReference.Items.Add(text2);
					AssignedCopyright.Items.Add(text2);
					AssignedWriter.Items.Add(text2);
					AssignedKey.Items.Add(text2);
					AssignedTiming.Items.Add(text2);
					AssignedAdmin1.Items.Add(text2);
					AssignedAdmin2.Items.Add(text2);
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
			ListViewItem listViewItem = new ListViewItem();
			Cursor = Cursors.WaitCursor;
			string fullSearchString = "select * from [" + TablesList.Text + "]";

			using DataTable datatable = DbController.GetDataTable(Gf.ConnectSQLiteDef + Gf.Import_AccessFileName, fullSearchString);

			int num = 0;
			if (datatable.Rows.Count > 0)
			{
				//recordSet.MoveFirst();
				int num2 = 0;
				num2 = datatable.Rows.Count;

				foreach (DataRow dr in datatable.Rows)
				{
					listViewItem = TableExtracts.Items.Add(DataUtil.ObjToString(dr[AssignedTitle.Items[0].ToString()]));
					for (int i = 1; i <= AssignedTitle.Items.Count - 1; i++)
					{
						listViewItem.SubItems.Add(Convert.ToString(DataUtil.ObjToString(dr[AssignedTitle.Items[i].ToString()])));
					}
					num++;
				}
				label2.Text = "Records Found in selected table: " + ((num < num2) ? ("(Displaying " + num + "/" + num2 + " records)") : ("(Displaying all " + num2 + " records)"));
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
				MessageBox.Show("Please assign a column to the Title");
				return false;
			}
			if (AssignedLyricsMergeOrderList.Items.Count == 0)
			{
				MessageBox.Show("'Lyrics Merge List' must have at least one column");
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
			ListViewItem listViewItem = new ListViewItem();
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
			int num = 0;
			for (int i = 0; i <= count - 1; i++)
			{
				if (AssignedLyricsMergeOrderList.Items[i].Selected)
				{
					if (num < 1)
					{
						num = i;
						continue;
					}
					i = count;
					num = 0;
				}
			}
			if (num >= 1)
			{
				string text = AssignedLyricsMergeOrderList.Items[num].Text;
				AssignedLyricsMergeOrderList.Items[num].Text = AssignedLyricsMergeOrderList.Items[num - 1].Text;
				AssignedLyricsMergeOrderList.Items[num - 1].Text = text;
				AssignedLyricsMergeOrderList.Items[num].Selected = false;
				AssignedLyricsMergeOrderList.Items[num - 1].Selected = true;
			}
		}

		private void MoveDownBtn_Click()
		{
			int count = AssignedLyricsMergeOrderList.Items.Count;
			if (count <= 1)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i <= count - 1; i++)
			{
				if (AssignedLyricsMergeOrderList.Items[i].Selected)
				{
					if (num < 1)
					{
						num = i;
						continue;
					}
					i = count;
					num = -1;
				}
			}
			if (!((num < 0) | (num == count - 1)))
			{
				string text = AssignedLyricsMergeOrderList.Items[num].Text;
				AssignedLyricsMergeOrderList.Items[num].Text = AssignedLyricsMergeOrderList.Items[num + 1].Text;
				AssignedLyricsMergeOrderList.Items[num + 1].Text = text;
				AssignedLyricsMergeOrderList.Items[num].Selected = false;
				AssignedLyricsMergeOrderList.Items[num + 1].Selected = true;
			}
		}

		private void DelBtn_Click()
		{
			if (AssignedLyricsMergeOrderList.Items.Count == 0)
			{
				return;
			}
			int num = 0;
			for (int num2 = AssignedLyricsMergeOrderList.Items.Count - 1; num2 >= 0; num2--)
			{
				if (AssignedLyricsMergeOrderList.Items[num2].Selected)
				{
					AssignedLyricsMergeOrderList.Items[num2].Remove();
					num = num2;
				}
			}
			if (num > 0)
			{
				num--;
			}
			if (AssignedLyricsMergeOrderList.Items.Count > 0)
			{
				AssignedLyricsMergeOrderList.Items[num].Selected = true;
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
				string text = "";
				Gf.Import_SongTitle2ColumnName = ((AssignedTitle2.Text != "") ? ("[" + AssignedTitle2.Text + "]") : text);
				Gf.Import_SongNumberColumnName = ((AssignedSongNumber.Text != "") ? ("[" + AssignedSongNumber.Text + "]") : text);
				Gf.Import_SongWriterInfoColumnName = ((AssignedWriter.Text != "") ? ("[" + AssignedWriter.Text + "]") : text);
				Gf.Import_BookReferenceColumnName = ((AssignedBookReference.Text != "") ? ("[" + AssignedBookReference.Text + "]") : text);
				Gf.Import_UserReferenceColumnName = ((AssignedUserReference.Text != "") ? ("[" + AssignedUserReference.Text + "]") : text);
				Gf.Import_SongCopyrightColumnName = ((AssignedCopyright.Text != "") ? ("[" + AssignedCopyright.Text + "]") : text);
				Gf.Import_SongKeyColumnName = ((AssignedKey.Text != "") ? ("[" + AssignedKey.Text + "]") : text);
				Gf.Import_SongTimingColumnName = ((AssignedTiming.Text != "") ? ("[" + AssignedTiming.Text + "]") : text);
				Gf.Import_Admin1ColumnName = ((AssignedAdmin1.Text != "") ? ("[" + AssignedAdmin1.Text + "]") : text);
				Gf.Import_Admin2ColumnName = ((AssignedAdmin2.Text != "") ? ("[" + AssignedAdmin2.Text + "]") : text);
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}

		            }
}
