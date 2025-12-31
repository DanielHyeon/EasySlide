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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmImportAccessHelper));
            TablesList = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            TableExtracts = new ListView();
            label3 = new Label();
            groupBox2 = new GroupBox();
            panelVerses = new Panel();
            AssignedLyrics = new ListView();
            columnHeader1 = new ColumnHeader();
            panel2 = new Panel();
            label16 = new Label();
            label4 = new Label();
            panelOrderList = new Panel();
            AssignedLyricsMergeOrderList = new ListView();
            columnHeader6 = new ColumnHeader();
            panel4 = new Panel();
            label17 = new Label();
            AssignedTitle = new ComboBox();
            panelSeqSet = new Panel();
            toolStripColumnAdd = new ToolStrip();
            Column_Add = new ToolStripButton();
            panelSeqUpDown = new Panel();
            toolStripSeqUpDown = new ToolStrip();
            OrderList_Up = new ToolStripButton();
            OrderList_Down = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            OrderList_Delete = new ToolStripButton();
            groupBox1 = new GroupBox();
            AssignedAdmin2 = new ComboBox();
            AssignedAdmin1 = new ComboBox();
            AssignedTiming = new ComboBox();
            AssignedWriter = new ComboBox();
            AssignedKey = new ComboBox();
            AssignedCopyright = new ComboBox();
            AssignedUserReference = new ComboBox();
            AssignedSongNumber = new ComboBox();
            AssignedBookReference = new ComboBox();
            AssignedTitle2 = new ComboBox();
            label15 = new Label();
            label13 = new Label();
            label11 = new Label();
            label9 = new Label();
            label6 = new Label();
            label14 = new Label();
            label12 = new Label();
            label10 = new Label();
            label8 = new Label();
            label7 = new Label();
            label5 = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
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
            // 
            // TablesList
            // 
            TablesList.DropDownStyle = ComboBoxStyle.DropDownList;
            TablesList.FormattingEnabled = true;
            TablesList.Location = new System.Drawing.Point(17, 38);
            TablesList.Margin = new Padding(4, 5, 4, 5);
            TablesList.MaxDropDownItems = 12;
            TablesList.Name = "TablesList";
            TablesList.Size = new System.Drawing.Size(329, 28);
            TablesList.TabIndex = 0;
            TablesList.SelectedIndexChanged += TablesList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(16, 14);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(361, 20);
            label1.TabIndex = 4;
            label1.Text = "Step 1. Select the table which holds the songs details:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(356, 43);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(223, 20);
            label2.TabIndex = 5;
            label2.Text = "Records Found in selected table:";
            // 
            // TableExtracts
            // 
            TableExtracts.FullRowSelect = true;
            TableExtracts.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            TableExtracts.LabelWrap = false;
            TableExtracts.Location = new System.Drawing.Point(16, 80);
            TableExtracts.Margin = new Padding(4, 5, 4, 5);
            TableExtracts.Name = "TableExtracts";
            TableExtracts.ShowItemToolTips = true;
            TableExtracts.Size = new System.Drawing.Size(843, 209);
            TableExtracts.TabIndex = 1;
            TableExtracts.UseCompatibleStateImageBehavior = false;
            TableExtracts.View = View.Details;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(16, 298);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(330, 20);
            label3.TabIndex = 2;
            label3.Text = "Step 2. Assign compulsory and optional columns";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(panelVerses);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(panelOrderList);
            groupBox2.Controls.Add(AssignedTitle);
            groupBox2.Controls.Add(panelSeqSet);
            groupBox2.Controls.Add(panelSeqUpDown);
            groupBox2.Location = new System.Drawing.Point(16, 323);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(0);
            groupBox2.Size = new System.Drawing.Size(365, 252);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Mandatory Columns";
            // 
            // panelVerses
            // 
            panelVerses.BorderStyle = BorderStyle.Fixed3D;
            panelVerses.Controls.Add(AssignedLyrics);
            panelVerses.Controls.Add(panel2);
            panelVerses.Location = new System.Drawing.Point(8, 65);
            panelVerses.Margin = new Padding(4, 5, 4, 5);
            panelVerses.Name = "panelVerses";
            panelVerses.Size = new System.Drawing.Size(139, 173);
            panelVerses.TabIndex = 1;
            // 
            // AssignedLyrics
            // 
            AssignedLyrics.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            AssignedLyrics.Dock = DockStyle.Fill;
            AssignedLyrics.FullRowSelect = true;
            AssignedLyrics.HeaderStyle = ColumnHeaderStyle.None;
            AssignedLyrics.Location = new System.Drawing.Point(0, 20);
            AssignedLyrics.Margin = new Padding(1, 2, 1, 2);
            AssignedLyrics.Name = "AssignedLyrics";
            AssignedLyrics.ShowItemToolTips = true;
            AssignedLyrics.Size = new System.Drawing.Size(135, 149);
            AssignedLyrics.TabIndex = 0;
            AssignedLyrics.UseCompatibleStateImageBehavior = false;
            AssignedLyrics.View = View.Details;
            AssignedLyrics.DoubleClick += AssignedLyrics_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 74;
            // 
            // panel2
            // 
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(label16);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new System.Drawing.Point(0, 0);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(135, 20);
            panel2.TabIndex = 0;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new System.Drawing.Point(4, -2);
            label16.Margin = new Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(47, 20);
            label16.TabIndex = 0;
            label16.Text = "Lyrics:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(9, 29);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(41, 20);
            label4.TabIndex = 61;
            label4.Text = "Title:";
            // 
            // panelOrderList
            // 
            panelOrderList.BorderStyle = BorderStyle.Fixed3D;
            panelOrderList.Controls.Add(AssignedLyricsMergeOrderList);
            panelOrderList.Controls.Add(panel4);
            panelOrderList.Location = new System.Drawing.Point(187, 65);
            panelOrderList.Margin = new Padding(4, 5, 4, 5);
            panelOrderList.Name = "panelOrderList";
            panelOrderList.Size = new System.Drawing.Size(141, 173);
            panelOrderList.TabIndex = 2;
            // 
            // AssignedLyricsMergeOrderList
            // 
            AssignedLyricsMergeOrderList.Columns.AddRange(new ColumnHeader[] { columnHeader6 });
            AssignedLyricsMergeOrderList.Dock = DockStyle.Fill;
            AssignedLyricsMergeOrderList.FullRowSelect = true;
            AssignedLyricsMergeOrderList.HeaderStyle = ColumnHeaderStyle.None;
            AssignedLyricsMergeOrderList.Location = new System.Drawing.Point(0, 20);
            AssignedLyricsMergeOrderList.Margin = new Padding(4, 5, 4, 5);
            AssignedLyricsMergeOrderList.Name = "AssignedLyricsMergeOrderList";
            AssignedLyricsMergeOrderList.ShowItemToolTips = true;
            AssignedLyricsMergeOrderList.Size = new System.Drawing.Size(137, 149);
            AssignedLyricsMergeOrderList.TabIndex = 0;
            AssignedLyricsMergeOrderList.UseCompatibleStateImageBehavior = false;
            AssignedLyricsMergeOrderList.View = View.Details;
            // 
            // columnHeader6
            // 
            columnHeader6.Width = 74;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(label17);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new System.Drawing.Point(0, 0);
            panel4.Margin = new Padding(4, 5, 4, 5);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(137, 20);
            panel4.TabIndex = 0;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(4, -2);
            label17.Margin = new Padding(4, 0, 4, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(120, 20);
            label17.TabIndex = 0;
            label17.Text = "Lyrics Merge List:";
            // 
            // AssignedTitle
            // 
            AssignedTitle.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedTitle.FormattingEnabled = true;
            AssignedTitle.Location = new System.Drawing.Point(49, 25);
            AssignedTitle.Margin = new Padding(4, 5, 4, 5);
            AssignedTitle.MaxDropDownItems = 12;
            AssignedTitle.Name = "AssignedTitle";
            AssignedTitle.Size = new System.Drawing.Size(155, 28);
            AssignedTitle.TabIndex = 0;
            // 
            // panelSeqSet
            // 
            panelSeqSet.Controls.Add(toolStripColumnAdd);
            panelSeqSet.Location = new System.Drawing.Point(148, 89);
            panelSeqSet.Margin = new Padding(4, 5, 4, 5);
            panelSeqSet.Name = "panelSeqSet";
            panelSeqSet.Size = new System.Drawing.Size(33, 42);
            panelSeqSet.TabIndex = 13;
            // 
            // toolStripColumnAdd
            // 
            toolStripColumnAdd.AutoSize = false;
            toolStripColumnAdd.CanOverflow = false;
            toolStripColumnAdd.Dock = DockStyle.None;
            toolStripColumnAdd.GripStyle = ToolStripGripStyle.Hidden;
            toolStripColumnAdd.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStripColumnAdd.Items.AddRange(new ToolStripItem[] { Column_Add });
            toolStripColumnAdd.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripColumnAdd.Location = new System.Drawing.Point(0, 2);
            toolStripColumnAdd.Name = "toolStripColumnAdd";
            toolStripColumnAdd.RenderMode = ToolStripRenderMode.System;
            toolStripColumnAdd.Size = new System.Drawing.Size(33, 54);
            toolStripColumnAdd.TabIndex = 5;
            // 
            // Column_Add
            // 
            Column_Add.AutoSize = false;
            Column_Add.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Column_Add.Image = Resources.arrowR;
            Column_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
            Column_Add.Name = "Column_Add";
            Column_Add.Size = new System.Drawing.Size(22, 22);
            Column_Add.Tag = "";
            Column_Add.ToolTipText = "Add";
            Column_Add.MouseUp += Column_MouseUp;
            // 
            // panelSeqUpDown
            // 
            panelSeqUpDown.Controls.Add(toolStripSeqUpDown);
            panelSeqUpDown.Location = new System.Drawing.Point(329, 89);
            panelSeqUpDown.Margin = new Padding(4, 5, 4, 5);
            panelSeqUpDown.Name = "panelSeqUpDown";
            panelSeqUpDown.Size = new System.Drawing.Size(33, 122);
            panelSeqUpDown.TabIndex = 12;
            // 
            // toolStripSeqUpDown
            // 
            toolStripSeqUpDown.AutoSize = false;
            toolStripSeqUpDown.CanOverflow = false;
            toolStripSeqUpDown.Dock = DockStyle.None;
            toolStripSeqUpDown.GripStyle = ToolStripGripStyle.Hidden;
            toolStripSeqUpDown.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStripSeqUpDown.Items.AddRange(new ToolStripItem[] { OrderList_Up, OrderList_Down, toolStripSeparator5, OrderList_Delete });
            toolStripSeqUpDown.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripSeqUpDown.Location = new System.Drawing.Point(0, 2);
            toolStripSeqUpDown.Name = "toolStripSeqUpDown";
            toolStripSeqUpDown.RenderMode = ToolStripRenderMode.System;
            toolStripSeqUpDown.Size = new System.Drawing.Size(33, 137);
            toolStripSeqUpDown.TabIndex = 0;
            // 
            // OrderList_Up
            // 
            OrderList_Up.AutoSize = false;
            OrderList_Up.DisplayStyle = ToolStripItemDisplayStyle.Image;
            OrderList_Up.Image = Resources.handup;
            OrderList_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
            OrderList_Up.Name = "OrderList_Up";
            OrderList_Up.Size = new System.Drawing.Size(22, 22);
            OrderList_Up.Tag = "up";
            OrderList_Up.ToolTipText = "Move Item Up";
            OrderList_Up.MouseUp += OrderList_MouseUp;
            // 
            // OrderList_Down
            // 
            OrderList_Down.AutoSize = false;
            OrderList_Down.DisplayStyle = ToolStripItemDisplayStyle.Image;
            OrderList_Down.Image = Resources.handdown;
            OrderList_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
            OrderList_Down.Name = "OrderList_Down";
            OrderList_Down.Size = new System.Drawing.Size(22, 22);
            OrderList_Down.Tag = "down";
            OrderList_Down.ToolTipText = "Move Item Down";
            OrderList_Down.MouseUp += OrderList_MouseUp;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(31, 6);
            // 
            // OrderList_Delete
            // 
            OrderList_Delete.AutoSize = false;
            OrderList_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            OrderList_Delete.Image = Resources.Delete;
            OrderList_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            OrderList_Delete.Name = "OrderList_Delete";
            OrderList_Delete.Size = new System.Drawing.Size(22, 22);
            OrderList_Delete.Tag = "delete";
            OrderList_Delete.ToolTipText = "Delete";
            OrderList_Delete.MouseUp += OrderList_MouseUp;
            // 
            // groupBox1
            // 
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
            groupBox1.Location = new System.Drawing.Point(389, 325);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new System.Drawing.Size(471, 251);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Optional Columns";
            // 
            // AssignedAdmin2
            // 
            AssignedAdmin2.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedAdmin2.FormattingEnabled = true;
            AssignedAdmin2.Location = new System.Drawing.Point(304, 202);
            AssignedAdmin2.Margin = new Padding(4, 5, 4, 5);
            AssignedAdmin2.MaxDropDownItems = 12;
            AssignedAdmin2.Name = "AssignedAdmin2";
            AssignedAdmin2.Size = new System.Drawing.Size(155, 28);
            AssignedAdmin2.TabIndex = 19;
            // 
            // AssignedAdmin1
            // 
            AssignedAdmin1.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedAdmin1.FormattingEnabled = true;
            AssignedAdmin1.Location = new System.Drawing.Point(73, 202);
            AssignedAdmin1.Margin = new Padding(4, 5, 4, 5);
            AssignedAdmin1.MaxDropDownItems = 12;
            AssignedAdmin1.Name = "AssignedAdmin1";
            AssignedAdmin1.Size = new System.Drawing.Size(155, 28);
            AssignedAdmin1.TabIndex = 9;
            // 
            // AssignedTiming
            // 
            AssignedTiming.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedTiming.FormattingEnabled = true;
            AssignedTiming.Location = new System.Drawing.Point(304, 160);
            AssignedTiming.Margin = new Padding(4, 5, 4, 5);
            AssignedTiming.MaxDropDownItems = 12;
            AssignedTiming.Name = "AssignedTiming";
            AssignedTiming.Size = new System.Drawing.Size(155, 28);
            AssignedTiming.TabIndex = 17;
            // 
            // AssignedWriter
            // 
            AssignedWriter.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedWriter.FormattingEnabled = true;
            AssignedWriter.Location = new System.Drawing.Point(73, 160);
            AssignedWriter.Margin = new Padding(4, 5, 4, 5);
            AssignedWriter.MaxDropDownItems = 12;
            AssignedWriter.Name = "AssignedWriter";
            AssignedWriter.Size = new System.Drawing.Size(155, 28);
            AssignedWriter.TabIndex = 7;
            // 
            // AssignedKey
            // 
            AssignedKey.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedKey.FormattingEnabled = true;
            AssignedKey.Location = new System.Drawing.Point(304, 118);
            AssignedKey.Margin = new Padding(4, 5, 4, 5);
            AssignedKey.MaxDropDownItems = 12;
            AssignedKey.Name = "AssignedKey";
            AssignedKey.Size = new System.Drawing.Size(155, 28);
            AssignedKey.TabIndex = 15;
            // 
            // AssignedCopyright
            // 
            AssignedCopyright.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedCopyright.FormattingEnabled = true;
            AssignedCopyright.Location = new System.Drawing.Point(73, 118);
            AssignedCopyright.Margin = new Padding(4, 5, 4, 5);
            AssignedCopyright.MaxDropDownItems = 12;
            AssignedCopyright.Name = "AssignedCopyright";
            AssignedCopyright.Size = new System.Drawing.Size(155, 28);
            AssignedCopyright.TabIndex = 5;
            // 
            // AssignedUserReference
            // 
            AssignedUserReference.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedUserReference.FormattingEnabled = true;
            AssignedUserReference.Location = new System.Drawing.Point(304, 77);
            AssignedUserReference.Margin = new Padding(4, 5, 4, 5);
            AssignedUserReference.MaxDropDownItems = 12;
            AssignedUserReference.Name = "AssignedUserReference";
            AssignedUserReference.Size = new System.Drawing.Size(155, 28);
            AssignedUserReference.TabIndex = 13;
            // 
            // AssignedSongNumber
            // 
            AssignedSongNumber.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedSongNumber.FormattingEnabled = true;
            AssignedSongNumber.Location = new System.Drawing.Point(73, 77);
            AssignedSongNumber.Margin = new Padding(4, 5, 4, 5);
            AssignedSongNumber.MaxDropDownItems = 12;
            AssignedSongNumber.Name = "AssignedSongNumber";
            AssignedSongNumber.Size = new System.Drawing.Size(155, 28);
            AssignedSongNumber.TabIndex = 3;
            // 
            // AssignedBookReference
            // 
            AssignedBookReference.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedBookReference.FormattingEnabled = true;
            AssignedBookReference.Location = new System.Drawing.Point(304, 35);
            AssignedBookReference.Margin = new Padding(4, 5, 4, 5);
            AssignedBookReference.MaxDropDownItems = 12;
            AssignedBookReference.Name = "AssignedBookReference";
            AssignedBookReference.Size = new System.Drawing.Size(155, 28);
            AssignedBookReference.TabIndex = 11;
            // 
            // AssignedTitle2
            // 
            AssignedTitle2.DropDownStyle = ComboBoxStyle.DropDownList;
            AssignedTitle2.FormattingEnabled = true;
            AssignedTitle2.Location = new System.Drawing.Point(73, 35);
            AssignedTitle2.Margin = new Padding(4, 5, 4, 5);
            AssignedTitle2.MaxDropDownItems = 12;
            AssignedTitle2.Name = "AssignedTitle2";
            AssignedTitle2.Size = new System.Drawing.Size(155, 28);
            AssignedTitle2.TabIndex = 1;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(7, 206);
            label15.Margin = new Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(65, 20);
            label15.TabIndex = 8;
            label15.Text = "Admin 1";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(7, 165);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(53, 20);
            label13.TabIndex = 6;
            label13.Text = "Writer:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(7, 123);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(77, 20);
            label11.TabIndex = 4;
            label11.Text = "Copyright:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(7, 82);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(70, 20);
            label9.TabIndex = 2;
            label9.Text = "Song No.";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(7, 40);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(53, 20);
            label6.TabIndex = 0;
            label6.Text = "Title 2:";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(233, 206);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(68, 20);
            label14.TabIndex = 18;
            label14.Text = "Admin 2:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(233, 165);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(58, 20);
            label12.TabIndex = 16;
            label12.Text = "Timing:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(233, 123);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(72, 20);
            label10.TabIndex = 14;
            label10.Text = "Song key:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(233, 82);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(67, 20);
            label8.TabIndex = 12;
            label8.Text = "User Ref:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(233, 40);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(72, 20);
            label7.TabIndex = 10;
            label7.Text = "Book Ref:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(16, 585);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(172, 20);
            label5.TabIndex = 59;
            label5.Text = "Step 3. Click Import >>>";
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new System.Drawing.Point(755, 585);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new System.Drawing.Size(107, 37);
            BtnCancel.TabIndex = 6;
            BtnCancel.Text = "Cancel";
            // 
            // BtnOK
            // 
            BtnOK.Location = new System.Drawing.Point(627, 585);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new System.Drawing.Size(107, 37);
            BtnOK.TabIndex = 5;
            BtnOK.Text = "Import";
            BtnOK.Click += BtnOK_Click;
            // 
            // FrmImportAccessHelper
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new System.Drawing.Size(877, 652);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(label5);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Controls.Add(label3);
            Controls.Add(TableExtracts);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(TablesList);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmImportAccessHelper";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Access Helper";
            Load += FrmImportAccessHelper_Load;
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
