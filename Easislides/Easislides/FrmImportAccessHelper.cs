//using NetOffice.DAOApi;
using Easislides.SQLite;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
//using System.Data.SQLite;
using System.Windows.Forms;
using Easislides.Properties;
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
    public class FrmImportAccessHelper : Form
	{
		private bool FormInit = true;

		private string MessRecords = "";

		private IContainer components = null;

		private ComboBox TablesList;

		private Label label1;

		private Label label2;

		private ListView TableExtracts;

		private Label label3;

		private GroupBox groupBox2;

		private Panel panelVerses;

		private ListView AssignedLyrics;

		private ColumnHeader columnHeader1;

		private Panel panel2;

		private Label label16;

		private Panel panelOrderList;

		private ListView AssignedLyricsMergeOrderList;

		private ColumnHeader columnHeader6;

		private Panel panel4;

		private Label label17;

		private Panel panelSeqSet;

		private ToolStrip toolStripColumnAdd;

		private ToolStripButton Column_Add;

		private Panel panelSeqUpDown;

		private ToolStrip toolStripSeqUpDown;

		private ToolStripButton OrderList_Up;

		private ToolStripButton OrderList_Down;

		private ToolStripSeparator toolStripSeparator5;

		private ToolStripButton OrderList_Delete;

		private ComboBox AssignedTitle;

		private Label label4;

		private GroupBox groupBox1;

		private Label label6;

		private ComboBox AssignedTitle2;

		private Label label5;

		private Label label14;

		private ComboBox AssignedAdmin2;

		private Label label15;

		private ComboBox AssignedAdmin1;

		private Label label12;

		private ComboBox AssignedTiming;

		private Label label13;

		private ComboBox AssignedWriter;

		private Label label10;

		private ComboBox AssignedKey;

		private Label label11;

		private ComboBox AssignedCopyright;

		private Label label8;

		private ComboBox AssignedUserReference;

		private Label label9;

		private ComboBox AssignedSongNumber;

		private Label label7;

		private ComboBox AssignedBookReference;

		private Button BtnCancel;

		private Button BtnOK;

		public FrmImportAccessHelper()
		{
			InitializeComponent();
		}

		private void FrmImportAccessHelper_Load(object sender, EventArgs e)
		{
			Text = "Access Helper: " + gf.Import_AccessFileName;
			gf.Import_TableName = "";
			gf.Import_SongTitleColumnName = "";
			gf.Import_SongTitle2ColumnName = "";
			gf.Import_SongNumberColumnName = "";
			gf.Import_BookReferenceColumnName = "";
			gf.Import_UserReferenceColumnName = "";
			gf.Import_SongLyricsColumnName = "";
			gf.Import_SongWriterInfoColumnName = "";
			gf.Import_SongCopyrightColumnName = "";
			gf.Import_SongKeyColumnName = "";
			gf.Import_SongTimingColumnName = "";
			gf.Import_Admin1ColumnName = "";
			gf.Import_Admin2ColumnName = "";
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
#if OleDb
			using OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringDef + gf.Import_AccessFileName);

			TablesList.Items.Clear();
			try
			{
				string[] restrictions = new string[4];
				restrictions[3] = "Table";
				DataTable userTables = daoDb.GetSchema("Tables", restrictions);

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
#elif SQLite
			using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringSQLiteDef + gf.Import_AccessFileName);

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
#endif

			if (TablesList.Items.Count > 0)
			{
				return true;
			}
			MessageBox.Show("Sorry - the Access Database File does not contain any tables. Please quit out of this Helper.");

			return false;
		}

#if OleDb
		public bool TableNameExists(OleDbConnection connection, string TableName)
#elif SQLite
		public bool TableNameExists(DbConnection connection, string TableName)
#endif
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
#if OldeDb
			using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringDef + gf.Import_AccessFileName))
			{				
				if (!TableNameExists(daoDb, text))
				{
					MessageBox.Show("Error Encountered - Cannot find the table " + text + " in the Access Database");
					return;
				}
				ClearColumns();
				AssignedTitle.Items.Add("");
				foreach (DataColumn field in daoDb.GetSchema(text).Columns)
				{
					AssignedTitle.Items.Add(field.ColumnName);
				}
			}
#elif SQLite
			using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringSQLiteDef + gf.Import_AccessFileName);

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

#endif
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

#if OleDb
			using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringDef + gf.Import_AccessFileName, fullSearchString);
#elif SQLite
			using DataTable datatable = DbController.GetDataTable(gf.ConnectSQLiteDef + gf.Import_AccessFileName, fullSearchString);
#endif
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
				gf.Import_TableName = "[" + TablesList.Text + "]";
				gf.Import_SongTitleColumnName = "[" + AssignedTitle.Text + "]";
				for (int i = 0; i <= AssignedLyricsMergeOrderList.Items.Count - 1; i++)
				{
					object import_SongLyricsColumnName = gf.Import_SongLyricsColumnName;
					gf.Import_SongLyricsColumnName = string.Concat(import_SongLyricsColumnName, "[", AssignedLyricsMergeOrderList.Items[i].Text, "]", '>');
				}
				string text = "";
				gf.Import_SongTitle2ColumnName = ((AssignedTitle2.Text != "") ? ("[" + AssignedTitle2.Text + "]") : text);
				gf.Import_SongNumberColumnName = ((AssignedSongNumber.Text != "") ? ("[" + AssignedSongNumber.Text + "]") : text);
				gf.Import_SongWriterInfoColumnName = ((AssignedWriter.Text != "") ? ("[" + AssignedWriter.Text + "]") : text);
				gf.Import_BookReferenceColumnName = ((AssignedBookReference.Text != "") ? ("[" + AssignedBookReference.Text + "]") : text);
				gf.Import_UserReferenceColumnName = ((AssignedUserReference.Text != "") ? ("[" + AssignedUserReference.Text + "]") : text);
				gf.Import_SongCopyrightColumnName = ((AssignedCopyright.Text != "") ? ("[" + AssignedCopyright.Text + "]") : text);
				gf.Import_SongKeyColumnName = ((AssignedKey.Text != "") ? ("[" + AssignedKey.Text + "]") : text);
				gf.Import_SongTimingColumnName = ((AssignedTiming.Text != "") ? ("[" + AssignedTiming.Text + "]") : text);
				gf.Import_Admin1ColumnName = ((AssignedAdmin1.Text != "") ? ("[" + AssignedAdmin1.Text + "]") : text);
				gf.Import_Admin2ColumnName = ((AssignedAdmin2.Text != "") ? ("[" + AssignedAdmin2.Text + "]") : text);
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmImportAccessHelper));
			TablesList = new System.Windows.Forms.ComboBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			TableExtracts = new System.Windows.Forms.ListView();
			label3 = new System.Windows.Forms.Label();
			groupBox2 = new System.Windows.Forms.GroupBox();
			panelVerses = new System.Windows.Forms.Panel();
			AssignedLyrics = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			panel2 = new System.Windows.Forms.Panel();
			label16 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			panelOrderList = new System.Windows.Forms.Panel();
			AssignedLyricsMergeOrderList = new System.Windows.Forms.ListView();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			panel4 = new System.Windows.Forms.Panel();
			label17 = new System.Windows.Forms.Label();
			AssignedTitle = new System.Windows.Forms.ComboBox();
			panelSeqSet = new System.Windows.Forms.Panel();
			toolStripColumnAdd = new System.Windows.Forms.ToolStrip();
			Column_Add = new System.Windows.Forms.ToolStripButton();
			panelSeqUpDown = new System.Windows.Forms.Panel();
			toolStripSeqUpDown = new System.Windows.Forms.ToolStrip();
			OrderList_Up = new System.Windows.Forms.ToolStripButton();
			OrderList_Down = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			OrderList_Delete = new System.Windows.Forms.ToolStripButton();
			groupBox1 = new System.Windows.Forms.GroupBox();
			AssignedAdmin2 = new System.Windows.Forms.ComboBox();
			AssignedAdmin1 = new System.Windows.Forms.ComboBox();
			AssignedTiming = new System.Windows.Forms.ComboBox();
			AssignedWriter = new System.Windows.Forms.ComboBox();
			AssignedKey = new System.Windows.Forms.ComboBox();
			AssignedCopyright = new System.Windows.Forms.ComboBox();
			AssignedUserReference = new System.Windows.Forms.ComboBox();
			AssignedSongNumber = new System.Windows.Forms.ComboBox();
			AssignedBookReference = new System.Windows.Forms.ComboBox();
			AssignedTitle2 = new System.Windows.Forms.ComboBox();
			label15 = new System.Windows.Forms.Label();
			label13 = new System.Windows.Forms.Label();
			label11 = new System.Windows.Forms.Label();
			label9 = new System.Windows.Forms.Label();
			label6 = new System.Windows.Forms.Label();
			label14 = new System.Windows.Forms.Label();
			label12 = new System.Windows.Forms.Label();
			label10 = new System.Windows.Forms.Label();
			label8 = new System.Windows.Forms.Label();
			label7 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			groupBox2.SuspendLayout();
			panelVerses.SuspendLayout();
			panel2.SuspendLayout();
			panelOrderList.SuspendLayout();
			panel4.SuspendLayout();
			panelSeqSet.SuspendLayout();
			toolStripColumnAdd.SuspendLayout();
			panelSeqUpDown.SuspendLayout();
			toolStripSeqUpDown.SuspendLayout();
			groupBox1.SuspendLayout();
			SuspendLayout();
			TablesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			TablesList.FormattingEnabled = true;
			TablesList.Location = new System.Drawing.Point(13, 25);
			TablesList.MaxDropDownItems = 12;
			TablesList.Name = "TablesList";
			TablesList.Size = new System.Drawing.Size(248, 21);
			TablesList.TabIndex = 0;
			TablesList.SelectedIndexChanged += new System.EventHandler(TablesList_SelectedIndexChanged);
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(262, 13);
			label1.TabIndex = 4;
			label1.Text = "Step 1. Select the table which holds the songs details:";
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(267, 28);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(163, 13);
			label2.TabIndex = 5;
			label2.Text = "Records Found in selected table:";
			TableExtracts.FullRowSelect = true;
			TableExtracts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			TableExtracts.HideSelection = false;
			TableExtracts.LabelWrap = false;
			TableExtracts.Location = new System.Drawing.Point(12, 52);
			TableExtracts.Name = "TableExtracts";
			TableExtracts.ShowItemToolTips = true;
			TableExtracts.Size = new System.Drawing.Size(633, 137);
			TableExtracts.TabIndex = 1;
			TableExtracts.UseCompatibleStateImageBehavior = false;
			TableExtracts.View = System.Windows.Forms.View.Details;
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(12, 194);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(234, 13);
			label3.TabIndex = 2;
			label3.Text = "Step 2. Assign compulsory and optional columns";
			groupBox2.Controls.Add(panelVerses);
			groupBox2.Controls.Add(label4);
			groupBox2.Controls.Add(panelOrderList);
			groupBox2.Controls.Add(AssignedTitle);
			groupBox2.Controls.Add(panelSeqSet);
			groupBox2.Controls.Add(panelSeqUpDown);
			groupBox2.Location = new System.Drawing.Point(12, 210);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new System.Windows.Forms.Padding(0);
			groupBox2.Size = new System.Drawing.Size(274, 164);
			groupBox2.TabIndex = 3;
			groupBox2.TabStop = false;
			groupBox2.Text = "Mandatory Columns";
			panelVerses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelVerses.Controls.Add(AssignedLyrics);
			panelVerses.Controls.Add(panel2);
			panelVerses.Location = new System.Drawing.Point(6, 42);
			panelVerses.Name = "panelVerses";
			panelVerses.Size = new System.Drawing.Size(105, 114);
			panelVerses.TabIndex = 1;
			AssignedLyrics.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1]
			{
				columnHeader1
			});
			AssignedLyrics.Dock = System.Windows.Forms.DockStyle.Fill;
			AssignedLyrics.FullRowSelect = true;
			AssignedLyrics.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			AssignedLyrics.Location = new System.Drawing.Point(0, 14);
			AssignedLyrics.Margin = new System.Windows.Forms.Padding(1);
			AssignedLyrics.Name = "AssignedLyrics";
			AssignedLyrics.ShowItemToolTips = true;
			AssignedLyrics.Size = new System.Drawing.Size(101, 96);
			AssignedLyrics.TabIndex = 0;
			AssignedLyrics.UseCompatibleStateImageBehavior = false;
			AssignedLyrics.View = System.Windows.Forms.View.Details;
			AssignedLyrics.DoubleClick += new System.EventHandler(AssignedLyrics_DoubleClick);
			columnHeader1.Width = 74;
			panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel2.Controls.Add(label16);
			panel2.Dock = System.Windows.Forms.DockStyle.Top;
			panel2.Location = new System.Drawing.Point(0, 0);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(101, 14);
			panel2.TabIndex = 0;
			label16.AutoSize = true;
			label16.Location = new System.Drawing.Point(3, -1);
			label16.Name = "label16";
			label16.Size = new System.Drawing.Size(37, 13);
			label16.TabIndex = 0;
			label16.Text = "Lyrics:";
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(7, 19);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(30, 13);
			label4.TabIndex = 61;
			label4.Text = "Title:";
			panelOrderList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelOrderList.Controls.Add(AssignedLyricsMergeOrderList);
			panelOrderList.Controls.Add(panel4);
			panelOrderList.Location = new System.Drawing.Point(140, 42);
			panelOrderList.Name = "panelOrderList";
			panelOrderList.Size = new System.Drawing.Size(107, 114);
			panelOrderList.TabIndex = 2;
			AssignedLyricsMergeOrderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1]
			{
				columnHeader6
			});
			AssignedLyricsMergeOrderList.Dock = System.Windows.Forms.DockStyle.Fill;
			AssignedLyricsMergeOrderList.FullRowSelect = true;
			AssignedLyricsMergeOrderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			AssignedLyricsMergeOrderList.Location = new System.Drawing.Point(0, 14);
			AssignedLyricsMergeOrderList.Name = "AssignedLyricsMergeOrderList";
			AssignedLyricsMergeOrderList.ShowItemToolTips = true;
			AssignedLyricsMergeOrderList.Size = new System.Drawing.Size(103, 96);
			AssignedLyricsMergeOrderList.TabIndex = 0;
			AssignedLyricsMergeOrderList.UseCompatibleStateImageBehavior = false;
			AssignedLyricsMergeOrderList.View = System.Windows.Forms.View.Details;
			columnHeader6.Width = 74;
			panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel4.Controls.Add(label17);
			panel4.Dock = System.Windows.Forms.DockStyle.Top;
			panel4.Location = new System.Drawing.Point(0, 0);
			panel4.Name = "panel4";
			panel4.Size = new System.Drawing.Size(103, 14);
			panel4.TabIndex = 0;
			label17.AutoSize = true;
			label17.Location = new System.Drawing.Point(3, -1);
			label17.Name = "label17";
			label17.Size = new System.Drawing.Size(89, 13);
			label17.TabIndex = 0;
			label17.Text = "Lyrics Merge List:";
			AssignedTitle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedTitle.FormattingEnabled = true;
			AssignedTitle.Location = new System.Drawing.Point(37, 16);
			AssignedTitle.MaxDropDownItems = 12;
			AssignedTitle.Name = "AssignedTitle";
			AssignedTitle.Size = new System.Drawing.Size(117, 21);
			AssignedTitle.TabIndex = 0;
			panelSeqSet.Controls.Add(toolStripColumnAdd);
			panelSeqSet.Location = new System.Drawing.Point(111, 58);
			panelSeqSet.Name = "panelSeqSet";
			panelSeqSet.Size = new System.Drawing.Size(25, 27);
			panelSeqSet.TabIndex = 13;
			toolStripColumnAdd.AutoSize = false;
			toolStripColumnAdd.CanOverflow = false;
			toolStripColumnAdd.Dock = System.Windows.Forms.DockStyle.None;
			toolStripColumnAdd.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripColumnAdd.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				Column_Add
			});
			toolStripColumnAdd.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripColumnAdd.Location = new System.Drawing.Point(0, 1);
			toolStripColumnAdd.Name = "toolStripColumnAdd";
			toolStripColumnAdd.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripColumnAdd.Size = new System.Drawing.Size(25, 35);
			toolStripColumnAdd.TabIndex = 5;
			Column_Add.AutoSize = false;
			Column_Add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Column_Add.Image = Resources.arrowR;
			Column_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
			Column_Add.Name = "Column_Add";
			Column_Add.Size = new System.Drawing.Size(22, 22);
			Column_Add.Tag = "";
			Column_Add.ToolTipText = "Add";
			Column_Add.MouseUp += new System.Windows.Forms.MouseEventHandler(Column_MouseUp);
			panelSeqUpDown.Controls.Add(toolStripSeqUpDown);
			panelSeqUpDown.Location = new System.Drawing.Point(247, 58);
			panelSeqUpDown.Name = "panelSeqUpDown";
			panelSeqUpDown.Size = new System.Drawing.Size(25, 79);
			panelSeqUpDown.TabIndex = 12;
			toolStripSeqUpDown.AutoSize = false;
			toolStripSeqUpDown.CanOverflow = false;
			toolStripSeqUpDown.Dock = System.Windows.Forms.DockStyle.None;
			toolStripSeqUpDown.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripSeqUpDown.Items.AddRange(new System.Windows.Forms.ToolStripItem[4]
			{
				OrderList_Up,
				OrderList_Down,
				toolStripSeparator5,
				OrderList_Delete
			});
			toolStripSeqUpDown.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripSeqUpDown.Location = new System.Drawing.Point(0, 1);
			toolStripSeqUpDown.Name = "toolStripSeqUpDown";
			toolStripSeqUpDown.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripSeqUpDown.Size = new System.Drawing.Size(25, 89);
			toolStripSeqUpDown.TabIndex = 0;
			OrderList_Up.AutoSize = false;
			OrderList_Up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			OrderList_Up.Image = Resources.handup;
			OrderList_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
			OrderList_Up.Name = "OrderList_Up";
			OrderList_Up.Size = new System.Drawing.Size(22, 22);
			OrderList_Up.Tag = "up";
			OrderList_Up.ToolTipText = "Move Item Up";
			OrderList_Up.MouseUp += new System.Windows.Forms.MouseEventHandler(OrderList_MouseUp);
			OrderList_Down.AutoSize = false;
			OrderList_Down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			OrderList_Down.Image = Resources.handdown;
			OrderList_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
			OrderList_Down.Name = "OrderList_Down";
			OrderList_Down.Size = new System.Drawing.Size(22, 22);
			OrderList_Down.Tag = "down";
			OrderList_Down.ToolTipText = "Move Item Down";
			OrderList_Down.MouseUp += new System.Windows.Forms.MouseEventHandler(OrderList_MouseUp);
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new System.Drawing.Size(23, 6);
			OrderList_Delete.AutoSize = false;
			OrderList_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			OrderList_Delete.Image = Resources.Delete;
			OrderList_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			OrderList_Delete.Name = "OrderList_Delete";
			OrderList_Delete.Size = new System.Drawing.Size(22, 22);
			OrderList_Delete.Tag = "delete";
			OrderList_Delete.ToolTipText = "Delete";
			OrderList_Delete.MouseUp += new System.Windows.Forms.MouseEventHandler(OrderList_MouseUp);
			groupBox1.Controls.Add(AssignedAdmin2);
			groupBox1.Controls.Add(AssignedAdmin1);
			groupBox1.Controls.Add(AssignedTiming);
			groupBox1.Controls.Add(AssignedWriter);
			groupBox1.Controls.Add(AssignedKey);
			groupBox1.Controls.Add(AssignedCopyright);
			groupBox1.Controls.Add(AssignedUserReference);
			groupBox1.Controls.Add(AssignedSongNumber);
			groupBox1.Controls.Add(AssignedBookReference);
			groupBox1.Controls.Add(AssignedTitle2);
			groupBox1.Controls.Add(label15);
			groupBox1.Controls.Add(label13);
			groupBox1.Controls.Add(label11);
			groupBox1.Controls.Add(label9);
			groupBox1.Controls.Add(label6);
			groupBox1.Controls.Add(label14);
			groupBox1.Controls.Add(label12);
			groupBox1.Controls.Add(label10);
			groupBox1.Controls.Add(label8);
			groupBox1.Controls.Add(label7);
			groupBox1.Location = new System.Drawing.Point(292, 211);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(353, 163);
			groupBox1.TabIndex = 4;
			groupBox1.TabStop = false;
			groupBox1.Text = "Optional Columns";
			AssignedAdmin2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedAdmin2.FormattingEnabled = true;
			AssignedAdmin2.Location = new System.Drawing.Point(228, 131);
			AssignedAdmin2.MaxDropDownItems = 12;
			AssignedAdmin2.Name = "AssignedAdmin2";
			AssignedAdmin2.Size = new System.Drawing.Size(117, 21);
			AssignedAdmin2.TabIndex = 19;
			AssignedAdmin1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedAdmin1.FormattingEnabled = true;
			AssignedAdmin1.Location = new System.Drawing.Point(55, 131);
			AssignedAdmin1.MaxDropDownItems = 12;
			AssignedAdmin1.Name = "AssignedAdmin1";
			AssignedAdmin1.Size = new System.Drawing.Size(117, 21);
			AssignedAdmin1.TabIndex = 9;
			AssignedTiming.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedTiming.FormattingEnabled = true;
			AssignedTiming.Location = new System.Drawing.Point(228, 104);
			AssignedTiming.MaxDropDownItems = 12;
			AssignedTiming.Name = "AssignedTiming";
			AssignedTiming.Size = new System.Drawing.Size(117, 21);
			AssignedTiming.TabIndex = 17;
			AssignedWriter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedWriter.FormattingEnabled = true;
			AssignedWriter.Location = new System.Drawing.Point(55, 104);
			AssignedWriter.MaxDropDownItems = 12;
			AssignedWriter.Name = "AssignedWriter";
			AssignedWriter.Size = new System.Drawing.Size(117, 21);
			AssignedWriter.TabIndex = 7;
			AssignedKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedKey.FormattingEnabled = true;
			AssignedKey.Location = new System.Drawing.Point(228, 77);
			AssignedKey.MaxDropDownItems = 12;
			AssignedKey.Name = "AssignedKey";
			AssignedKey.Size = new System.Drawing.Size(117, 21);
			AssignedKey.TabIndex = 15;
			AssignedCopyright.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedCopyright.FormattingEnabled = true;
			AssignedCopyright.Location = new System.Drawing.Point(55, 77);
			AssignedCopyright.MaxDropDownItems = 12;
			AssignedCopyright.Name = "AssignedCopyright";
			AssignedCopyright.Size = new System.Drawing.Size(117, 21);
			AssignedCopyright.TabIndex = 5;
			AssignedUserReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedUserReference.FormattingEnabled = true;
			AssignedUserReference.Location = new System.Drawing.Point(228, 50);
			AssignedUserReference.MaxDropDownItems = 12;
			AssignedUserReference.Name = "AssignedUserReference";
			AssignedUserReference.Size = new System.Drawing.Size(117, 21);
			AssignedUserReference.TabIndex = 13;
			AssignedSongNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedSongNumber.FormattingEnabled = true;
			AssignedSongNumber.Location = new System.Drawing.Point(55, 50);
			AssignedSongNumber.MaxDropDownItems = 12;
			AssignedSongNumber.Name = "AssignedSongNumber";
			AssignedSongNumber.Size = new System.Drawing.Size(117, 21);
			AssignedSongNumber.TabIndex = 3;
			AssignedBookReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedBookReference.FormattingEnabled = true;
			AssignedBookReference.Location = new System.Drawing.Point(228, 23);
			AssignedBookReference.MaxDropDownItems = 12;
			AssignedBookReference.Name = "AssignedBookReference";
			AssignedBookReference.Size = new System.Drawing.Size(117, 21);
			AssignedBookReference.TabIndex = 11;
			AssignedTitle2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			AssignedTitle2.FormattingEnabled = true;
			AssignedTitle2.Location = new System.Drawing.Point(55, 23);
			AssignedTitle2.MaxDropDownItems = 12;
			AssignedTitle2.Name = "AssignedTitle2";
			AssignedTitle2.Size = new System.Drawing.Size(117, 21);
			AssignedTitle2.TabIndex = 1;
			label15.AutoSize = true;
			label15.Location = new System.Drawing.Point(5, 134);
			label15.Name = "label15";
			label15.Size = new System.Drawing.Size(45, 13);
			label15.TabIndex = 8;
			label15.Text = "Admin 1";
			label13.AutoSize = true;
			label13.Location = new System.Drawing.Point(5, 107);
			label13.Name = "label13";
			label13.Size = new System.Drawing.Size(38, 13);
			label13.TabIndex = 6;
			label13.Text = "Writer:";
			label11.AutoSize = true;
			label11.Location = new System.Drawing.Point(5, 80);
			label11.Name = "label11";
			label11.Size = new System.Drawing.Size(54, 13);
			label11.TabIndex = 4;
			label11.Text = "Copyright:";
			label9.AutoSize = true;
			label9.Location = new System.Drawing.Point(5, 53);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(52, 13);
			label9.TabIndex = 2;
			label9.Text = "Song No.";
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(5, 26);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(39, 13);
			label6.TabIndex = 0;
			label6.Text = "Title 2:";
			label14.AutoSize = true;
			label14.Location = new System.Drawing.Point(175, 134);
			label14.Name = "label14";
			label14.Size = new System.Drawing.Size(48, 13);
			label14.TabIndex = 18;
			label14.Text = "Admin 2:";
			label12.AutoSize = true;
			label12.Location = new System.Drawing.Point(175, 107);
			label12.Name = "label12";
			label12.Size = new System.Drawing.Size(41, 13);
			label12.TabIndex = 16;
			label12.Text = "Timing:";
			label10.AutoSize = true;
			label10.Location = new System.Drawing.Point(175, 80);
			label10.Name = "label10";
			label10.Size = new System.Drawing.Size(55, 13);
			label10.TabIndex = 14;
			label10.Text = "Song key:";
			label8.AutoSize = true;
			label8.Location = new System.Drawing.Point(175, 53);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(52, 13);
			label8.TabIndex = 12;
			label8.Text = "User Ref:";
			label7.AutoSize = true;
			label7.Location = new System.Drawing.Point(175, 26);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(55, 13);
			label7.TabIndex = 10;
			label7.Text = "Book Ref:";
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(12, 380);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(120, 13);
			label5.TabIndex = 59;
			label5.Text = "Step 3. Click Import >>>";
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(566, 380);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 6;
			BtnCancel.Text = "Cancel";
			BtnOK.Location = new System.Drawing.Point(470, 380);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 5;
			BtnOK.Text = "Import";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(658, 424);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(label5);
			base.Controls.Add(groupBox1);
			base.Controls.Add(groupBox2);
			base.Controls.Add(label3);
			base.Controls.Add(TableExtracts);
			base.Controls.Add(label2);
			base.Controls.Add(label1);
			base.Controls.Add(TablesList);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmImportAccessHelper";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Access Helper";
			base.Load += new System.EventHandler(FrmImportAccessHelper_Load);
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			panelVerses.ResumeLayout(false);
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			panelOrderList.ResumeLayout(false);
			panel4.ResumeLayout(false);
			panel4.PerformLayout();
			panelSeqSet.ResumeLayout(false);
			toolStripColumnAdd.ResumeLayout(false);
			toolStripColumnAdd.PerformLayout();
			panelSeqUpDown.ResumeLayout(false);
			toolStripSeqUpDown.ResumeLayout(false);
			toolStripSeqUpDown.PerformLayout();
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
