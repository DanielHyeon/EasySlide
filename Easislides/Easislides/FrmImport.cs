//using NetOffice.DAOApi;
using Easislides.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Data;
using Easislides.Util;

using Easislides.SQLite;
using Easislides.Module;
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
    public class FrmImport : Form
	{
		private IContainer components = null;

		private GroupBox groupBox1;

		private Panel panelLinkTitle2Lookup;

		private ToolStrip toolStrip2;

		private ToolStripButton LocationBtn;

		private Label tbImportFrom;

		private ProgressBar ProgressBar1;

		private CheckedListBox ImportFolderList;

		private GroupBox groupBox2;

		private GroupBox groupBox3;

		private GroupBox groupBox4;

		private Button BtnCancel;

		private Button BtnOK;

		private ListView SongFolder;

		private ColumnHeader columnHeader1;

		private RadioButton OptImport2;

		private RadioButton OptImport1;

		private RadioButton OptImport0;

		private ListView SongsList;

		private ColumnHeader columnHeader2;

		private ColumnHeader columnHeader3;

		private ColumnHeader columnHeader4;

		private OpenFileDialog OpenFileDialog1;

		private string InSeq;

		private string InTxt;

		private string OneSong;

		private string InFileName;

		private string MsgText;

		private int InTotal;

		private string SongNumber;

		private string MusicNotations;

		private string SongTiming;

		private string SongCapo;

		private string SongKey;

		private string SongHeader;

		private string tempSequence;

		private string SongSequence;

		private string UserReference;

		private string BookReference;

		private string SongCopyright;

		private string SongWriterInfo;

		private string SongLyrics;

		private string SongTitle2;

		private string SongTitle;

		private string Lic_Admin2;

		private string Lic_Admin1;

		private string TextImportFormat;

		private string CJK_WCount;

		private string CJK_SCount;

		private int CurSongID;

		private int SongsNew;

		private int SongsUpdated;

		private int DataProcessedSoFar;

		private int FileDataSize;

		private int ImportFileSize;

		private int Filelength;

		private int InFolderNo;

		private string[] sArray;

		private string[] EsfFolderNames = new string[41];

		private string esf1SongTitle = "[#";

		private string esf1SongTitle2 = "##";

		private string esf1SongFolder = "#f";

		private string esf1SongCopyright = "#c";

		private string esf1BookReference = "#r";

		private string esf1UserReference = "#u";

		private string esf1SongWriterInfo = "#w";

		private string esf1SongKey = "#k";

		private string esf1SongTiming = "#t";

		private string esf1SongCapo = "#0";

		private string esf1SongNumber = "#n";

		private string esf1SongAdmin1 = "#a";

		private string esf1SongAdmin2 = "#b";

		private string esf1Sequence = "#@";

		private string esf1SongFormat = "#q";

		private string esfImportFieldSeparator;

		private string esfImportFormatTitle;

		private string esfSongTitle;

		private string esfSongTitle2;

		private string esfSongFolder;

		private string esfBookReference;

		private string esfUserReference;

		private string esfSongCopyright;

		private string esfSongWriterInfo;

		private string esfSongKey;

		private string esfSongTiming;

		private string esfSongCapo;

		private string esfSongNumber;

		private string esfSongAdmin1;

		private string esfSongAdmin2;

		private string esfSequence;

		private string esfSongFormat;

		private int FolderLookupSongsCount = 0;

		private SongSettings ImportItem = new SongSettings();

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmImport));
            groupBox1 = new GroupBox();
            panelLinkTitle2Lookup = new Panel();
            toolStrip2 = new ToolStrip();
            LocationBtn = new ToolStripButton();
            tbImportFrom = new Label();
            ProgressBar1 = new ProgressBar();
            ImportFolderList = new CheckedListBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            SongFolder = new ListView();
            columnHeader1 = new ColumnHeader();
            groupBox4 = new GroupBox();
            OptImport2 = new RadioButton();
            OptImport1 = new RadioButton();
            OptImport0 = new RadioButton();
            BtnCancel = new Button();
            BtnOK = new Button();
            SongsList = new ListView();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            OpenFileDialog1 = new OpenFileDialog();
            groupBox1.SuspendLayout();
            panelLinkTitle2Lookup.SuspendLayout();
            toolStrip2.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panelLinkTitle2Lookup);
            groupBox1.Controls.Add(tbImportFrom);
            groupBox1.Location = new Point(16, 12);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(693, 75);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "A - Import File:";
            // 
            // panelLinkTitle2Lookup
            // 
            panelLinkTitle2Lookup.Controls.Add(toolStrip2);
            panelLinkTitle2Lookup.Location = new Point(656, 25);
            panelLinkTitle2Lookup.Margin = new Padding(4, 5, 4, 5);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(29, 34);
            panelLinkTitle2Lookup.TabIndex = 51;
            // 
            // toolStrip2
            // 
            toolStrip2.AutoSize = false;
            toolStrip2.CanOverflow = false;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { LocationBtn });
            toolStrip2.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.RenderMode = ToolStripRenderMode.System;
            toolStrip2.Size = new Size(33, 43);
            toolStrip2.TabIndex = 5;
            // 
            // LocationBtn
            // 
            LocationBtn.AutoSize = false;
            LocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LocationBtn.Image = Resources.folder;
            LocationBtn.ImageTransparentColor = Color.Magenta;
            LocationBtn.Name = "LocationBtn";
            LocationBtn.Size = new Size(22, 22);
            LocationBtn.Tag = "";
            LocationBtn.ToolTipText = "Export file name";
            LocationBtn.Click += LocationBtn_Click;
            // 
            // tbImportFrom
            // 
            tbImportFrom.BackColor = SystemColors.Control;
            tbImportFrom.BorderStyle = BorderStyle.Fixed3D;
            tbImportFrom.Location = new Point(12, 25);
            tbImportFrom.Margin = new Padding(4, 0, 4, 0);
            tbImportFrom.Name = "tbImportFrom";
            tbImportFrom.Size = new Size(640, 34);
            tbImportFrom.TabIndex = 50;
            tbImportFrom.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProgressBar1
            // 
            ProgressBar1.Location = new Point(16, 474);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(693, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 5;
            // 
            // ImportFolderList
            // 
            ImportFolderList.CheckOnClick = true;
            ImportFolderList.FormattingEnabled = true;
            ImportFolderList.Location = new Point(12, 26);
            ImportFolderList.Margin = new Padding(4, 5, 4, 5);
            ImportFolderList.Name = "ImportFolderList";
            ImportFolderList.Size = new Size(189, 114);
            ImportFolderList.TabIndex = 50;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(ImportFolderList);
            groupBox2.Location = new Point(16, 97);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(213, 157);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "B - Import File Folders (if any)";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(SongFolder);
            groupBox3.Location = new Point(237, 97);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(240, 157);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "C - Select Folder to import into";
            // 
            // SongFolder
            // 
            SongFolder.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            SongFolder.FullRowSelect = true;
            SongFolder.HeaderStyle = ColumnHeaderStyle.None;
            SongFolder.Location = new Point(8, 25);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.MultiSelect = false;
            SongFolder.Name = "SongFolder";
            SongFolder.ShowGroups = false;
            SongFolder.ShowItemToolTips = true;
            SongFolder.Size = new Size(223, 121);
            SongFolder.TabIndex = 56;
            SongFolder.UseCompatibleStateImageBehavior = false;
            SongFolder.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "";
            columnHeader1.Width = 140;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(OptImport2);
            groupBox4.Controls.Add(OptImport1);
            groupBox4.Controls.Add(OptImport0);
            groupBox4.Location = new Point(485, 97);
            groupBox4.Margin = new Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 5, 4, 5);
            groupBox4.Size = new Size(224, 157);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "D - If Title exists in EasiSlides";
            // 
            // OptImport2
            // 
            OptImport2.Location = new Point(8, 102);
            OptImport2.Margin = new Padding(4, 5, 4, 5);
            OptImport2.Name = "OptImport2";
            OptImport2.Size = new Size(200, 46);
            OptImport2.TabIndex = 2;
            OptImport2.Text = "IMPORT and REPLACE existing item.";
            // 
            // OptImport1
            // 
            OptImport1.Location = new Point(8, 54);
            OptImport1.Margin = new Padding(4, 5, 4, 5);
            OptImport1.Name = "OptImport1";
            OptImport1.Size = new Size(200, 51);
            OptImport1.TabIndex = 1;
            OptImport1.Text = "IMPORT but also KEEP existing item";
            // 
            // OptImport0
            // 
            OptImport0.Checked = true;
            OptImport0.Location = new Point(8, 18);
            OptImport0.Margin = new Padding(4, 5, 4, 5);
            OptImport0.Name = "OptImport0";
            OptImport0.Size = new Size(200, 43);
            OptImport0.TabIndex = 0;
            OptImport0.TabStop = true;
            OptImport0.Text = "DO NOT Import the item";
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(603, 520);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 7;
            BtnCancel.Text = "Close";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(475, 520);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 6;
            BtnOK.Text = "Import";
            BtnOK.Click += BtnOK_Click;
            // 
            // SongsList
            // 
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader2, columnHeader3, columnHeader4 });
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            SongsList.LabelWrap = false;
            SongsList.Location = new Point(16, 262);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new Size(692, 209);
            SongsList.TabIndex = 4;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Item";
            columnHeader2.Width = 54;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Title";
            columnHeader3.Width = 229;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Status";
            columnHeader4.Width = 209;
            // 
            // OpenFileDialog1
            // 
            OpenFileDialog1.FileName = "openFileDialog1";
            // 
            // FrmImport
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(723, 575);
            Controls.Add(SongsList);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(ProgressBar1);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmImport";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Import";
            Load += FrmImport_Load;
            KeyUp += FrmImport_KeyUp;
            groupBox1.ResumeLayout(false);
            panelLinkTitle2Lookup.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            ResumeLayout(false);
        }

        public FrmImport()
		{
			InitializeComponent();
		}

		private void FrmImport_Load(object sender, EventArgs e)
		{
			ProgressBar1.Visible = false;
			OptImport1.Checked = true;
			BuildFolderList();
			ImportItem.Initialise();
		}

		private void BuildFolderList()
		{
			SongFolder.Items.Clear();
			for (int i = 1; i < 41; i++)
			{
				if (gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(gf.FolderName[i]);
				}
			}
		}

		private void BuildImportFolderList()
		{
			string text = DataUtil.Trim(tbImportFrom.Text);
			if (File.Exists(text))
			{
				ImportFolderList.Items.Clear();
				switch (DataUtil.Right(text, 4).ToLower())
				{
				case ".esf":
					BuildImportFolderList_Database(text);
					break;
				case ".est":
					BuildImportFolderList_ESTextFile(text);
					break;
				case ".esn":
					BuildImportFolderList_ESTextFile(text);
					break;
				case ".xml":
					BuildImportFolderList_ESXML(text);
					break;
				}
			}
		}

		private void BuildImportFolderList_Database(string ImportFileName)
		{
			ImportFolderList.Items.Clear();

#if OleDb
			using OleDbSQLiteConnection connection = DbConnectionController.GetOleDbConnection(gf.ConnectStringDef + ImportFileName);
#elif SQLite
			using DbConnection connection = DbController.GetDbConnection(gf.ConnectSQLiteDef + ImportFileName);
#endif

			for (int i = 0; i < 41; i++)
			{
				EsfFolderNames[i] = "";
			}

			bool flag = false;
			
			foreach (DataColumn field in connection.GetSchema("Song").Columns)
			{
				if (field.ColumnName == "USER_REFERENCE")
				{
					flag = true;
				}
			}
			string fullSearchString = "select * from Folder where FolderNo > 0 order by folderno ";

#if OleDb
			using DataTable dt = DbOleDbController.getDataTable(connection, fullSearchString);
#elif SQLite
			using DataTable dt = DbController.GetDataTable(connection, fullSearchString);
#endif

			if (dt.Rows.Count > 0)
			{
				return;
			}
			//recordset.MoveFirst();
			//while (!recordset.EOF)
			foreach (DataRow dr in dt.Rows)
			{
				EsfFolderNames[DataUtil.GetDataInt(dr, "FolderNo")] = DataUtil.GetDataString(dr, "name");
				if (flag)
				{
					ImportFolderList.Items.Add(DataUtil.GetDataString(dr, "name"));
				}
				else if (DataUtil.GetDataInt(dr, "FolderNo") == 1)
				{
					ImportFolderList.Items.Add(DataUtil.GetDataString(dr, "name"));
				}
				//recordset.MoveNext();
			}
		}

		private void BuildImportFolderList_ESTextFile(string ImportFileName)
		{
			ImportFolderList.Items.Clear();
			FolderLookupSongsCount = 0;
			string InString = "";
			if (!gfFileHelpers.LoadFileContents(ImportFileName, ref InString))
			{
				goto IL_02ca;
			}
			TextImportFormat = DataUtil.Left(InString, 8).ToLower();
			if (TextImportFormat == "[esf1.0]")
			{
				esfSongFolder = esf1SongFolder;
				esfImportFieldSeparator = "#";
				esfImportFormatTitle = "[" + esfImportFieldSeparator;
			}
			else
			{
				if (!(TextImportFormat == "[est3.1]"))
				{
					goto IL_02ca;
				}
				esfSongFolder = ">f";
				esfImportFieldSeparator = '>'.ToString();
				esfImportFormatTitle = "[>";
			}
			InString = DataUtil.Right(InString, InString.Length - 8);
			InString = InString.Replace(esfImportFormatTitle, Convert.ToString('\u0001'));
			string[] array = InString.Split('\u0001');
			ListViewItem listViewItem = new ListViewItem();
			if (array == null || array.GetUpperBound(0) < 0)
			{
				return;
			}
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			string text = "";
			string text2 = "";
			bool flag = false;
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				num3 = array[i].IndexOf("]");
				text = DataUtil.Left(array[i], (num3 > 0) ? num3 : 0);
				if (text.Length <= 0)
				{
					continue;
				}
				num = text.IndexOf(esfSongFolder);
				if ((num >= 0) & (text.Length > num))
				{
					num2 = text.IndexOf(esfImportFieldSeparator, num + 1);
					text2 = ((num2 <= 0) ? DataUtil.Mid(text, num + esfSongFolder.Length) : DataUtil.Mid(text, num + esfSongFolder.Length, num2 - (num + esfSongFolder.Length)));
				}
				else
				{
					text2 = "Default Folder";
				}
				FolderLookupSongsCount++;
				flag = false;
				for (int j = 0; j < ImportFolderList.Items.Count; j++)
				{
					if (ImportFolderList.Items[j].ToString() == text2)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					ImportFolderList.Items.Add(text2);
				}
			}
			return;
			IL_02ca:
			MessageBox.Show("There was an error reading the Import File - the file might not be a valid EasiSlides File");
		}

		private void BuildImportFolderList_ESXML(string ImportFileName)
		{
			bool flag = false;
			FolderLookupSongsCount = 0;
			ImportFolderList.Items.Clear();
			string text = "";
			ListViewItem listViewItem = new ListViewItem();
			try
			{
				XmlTextReader reader = new XmlTextReader(ImportFileName);
				if (gf.ValidateEasiSlidesXML(ref reader))
				{
					while (gf.ExtractEasiSlidesXMLItem(ref reader, ref ImportItem))
					{
						text = ((ImportItem.FolderName == "") ? "Default Folder" : ImportItem.FolderName);
						FolderLookupSongsCount++;
						flag = false;
						for (int i = 0; i < ImportFolderList.Items.Count; i++)
						{
							if (ImportFolderList.Items[i].ToString() == text)
							{
								flag = true;
							}
						}
						if (!flag)
						{
							ImportFolderList.Items.Add(text);
						}
					}
				}
				reader?.Close();
			}
			catch
			{
			}
		}

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			OpenFileDialog1.Filter = "EasiSlides/XML Files (*.esf,*.esn,*.xml)|*.esf;*.est;*.esn;*.xml|Access Database (*.mdb)|*.mdb";
			OpenFileDialog1.InitialDirectory = gf.ImportFromDir;
			OpenFileDialog1.AddExtension = true;
			OpenFileDialog1.DefaultExt = "*.xml";
			OpenFileDialog1.FileName = "";
			if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
			{
				tbImportFrom.Text = OpenFileDialog1.FileName;
				gf.ImportFromDir = Path.GetDirectoryName(tbImportFrom.Text) + "\\";
				BuildImportFolderList();
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Start_Import();
		}

		private void Start_Import()
		{
			string text = DataUtil.Trim(tbImportFrom.Text);
			if (text == "")
			{
				MessageBox.Show("Please specify an import file (A).");
			}
			else if (!File.Exists(text))
			{
				MessageBox.Show("The selected import file doesn't exist! Please re-select the import file (A).");
			}
			else if ((ImportFolderList.Items.Count > 0) & (ImportFolderList.CheckedItems.Count < 1))
			{
				MessageBox.Show("Please select Folder(s) from the list of Import File Folders (B).");
			}
			else if (SongFolder.SelectedItems.Count < 1)
			{
				MessageBox.Show("Please select a Folder to Import your items into (C).");
			}
			else
			{
				string strExt = Path.GetExtension(text).ToLower();
				switch (strExt)
				{
					case ".esf":
						Import_DatabaseFormat(text);
						break;
					case ".est":
						Import_TextFormat(text);
						break;
					case ".esn":
						Import_TextFormat(text);
						break;
					case ".xml":
						Import_XMLFormat(text);
						break;
					case ".mdb":
						AccessHelper(text);
						break;
					default:
						MessageBox.Show("Sorry - The Import File you have selected in (A) does not have a valid EasiSlides file extension.");
						break;
				}
			}
		}

		private void Import_DatabaseFormat(string ImportFileName)
		{
			int num = 0;
			ListViewItem listViewItem = new ListViewItem();
			string text = "";
			SongsUpdated = 0;
			SongsNew = 0;
			Cursor = Cursors.WaitCursor;
			SongsList.Items.Clear();
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			if (ImportFolderList.CheckedItems.Count > 0)
			{
				for (int i = 0; i < ImportFolderList.CheckedItems.Count; i++)
				{
					text = ((!(text == "")) ? (text + " or FolderNo=" + GetImportFolderNumber(ImportFolderList.CheckedItems[i].ToString())) : (" where (FolderNo=" + GetImportFolderNumber(ImportFolderList.CheckedItems[i].ToString())));
				}
				text += ") ";
			}
			int num2 = 0;
#if DAO
			Recordset recordSet = DbDaoController.GetRecordSet(gf.ConnectStringDef + ImportFileName, "select * from SONG " + text);
			if (!(recordSet?.EOF ?? true))
			{
				recordSet.MoveFirst();
				while (!recordSet.EOF)
				{
					num2++;
					recordSet.MoveNext();
				}
				recordSet.MoveFirst();
				listViewItem = SongsList.Items.Add("Importing...");
				int num3 = 0;
				InFolderNo = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
				Database daoDb = DbDaoController.GetDaoDb(gf.ConnectStringMainDB);
				Recordset recordset = null;
				int num4 = 0;
				while (!recordSet.EOF)
				{
					bool songRequired = false;
					num++;
					num3 = num * 100 / num2;
					ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
					Invalidate();
					ProgressBar1.Invalidate();
					if (gf.LoadDataIntoItem(ref ImportItem, recordSet))
					{
						listViewItem = SongsList.Items.Add(Convert.ToString(num));
						listViewItem.SubItems.Add(ImportItem.Title);
						string fullSearchString = "select * from SONG where Folderno=" + InFolderNo + " and Title_1 = \"" + ImportItem.Title + "\"";
						songRequired = true;
						ImportItem.FolderNo = InFolderNo;
						recordset = DbDaoController.GetRecordSet(daoDb, fullSearchString);
					}
					if (!(recordset?.EOF ?? true))
					{
						recordset.MoveFirst();
						CurSongID = DataUtil.GetDataInt(recordset, "SongID");
						if (OptImport0.Checked)
						{
							songRequired = false;
						}
						else if (OptImport1.Checked)
						{
							CurSongID = -1;
						}
					}
					else
					{
						CurSongID = -1;
					}
					recordset?.Close();
					num4 = 0;
					SaveSong(songRequired, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
					recordSet.MoveNext();
					SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
					SongsList.Update();
				}
				recordSet.Close();
				ProgressBar1.Value = 100;
				Show_Import_Result();
				if (recordset != null)
				{
					recordset = null;
				}
				if (recordSet != null)
				{
					datatable = null;
				}
				Cursor = Cursors.Default;
			}
			else
			{
				Cursor = Cursors.Default;
			}
#elif SQLite
			using DataTable dataTable = DbController.GetDataTable(gf.ConnectStringDef + ImportFileName, "select * from SONG " + text);

			num2 = dataTable.Rows.Count;

			if (num2 > 0)
			{
				dataTable.BeginInit();

				listViewItem = SongsList.Items.Add("Importing...");
				int num3 = 0;
				InFolderNo = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

				//DataTable dt = DbController.GetDataTable(connection, fullSearchString);

				DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
				//Database daoDb = DbDaoController.GetDaoDb(gf.ConnectStringMainDB);
				
				DataRow dr = null;
				int num4 = 0;

				foreach (DataRow row in dataTable.Rows)
				{
					bool songRequired = false;
					num++;
					num3 = num * 100 / num2;
					ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
					Invalidate();
					ProgressBar1.Invalidate();
					if (gf.LoadDataIntoItem(ref ImportItem, row))
					{
						listViewItem = SongsList.Items.Add(Convert.ToString(num));
						listViewItem.SubItems.Add(ImportItem.Title);
						string fullSearchString = "select * from SONG where Folderno=" + InFolderNo + " and Title_1 = \"" + ImportItem.Title + "\"";
						songRequired = true;
						ImportItem.FolderNo = InFolderNo;
						dr = DbController.GetDataRowScalar(connection, fullSearchString);
					}
					if (dr != null)
					{
						CurSongID = DataUtil.GetDataInt(dr, "SongID");
						if (OptImport0.Checked)
						{
							songRequired = false;
						}
						else if (OptImport1.Checked)
						{
							CurSongID = -1;
						}
					}
					else
					{
						CurSongID = -1;
					}
					num4 = 0;
					SaveSong(songRequired, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
					SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
					SongsList.Update();
				}
				ProgressBar1.Value = 100;
				Show_Import_Result();

				Cursor = Cursors.Default;
			}
			else
			{
				Cursor = Cursors.Default;
			}
#endif
		}

		private void Import_XMLFormat(string ImportFileName)
		{
			int num = 0;
			ListViewItem listViewItem = new ListViewItem();
			
			SongsUpdated = 0;
			SongsNew = 0;
			SongsList.Items.Clear();
#if OleDb
			using OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB);
#elif SQLite
			using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
#endif

			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add("Starting Import...");
			int folderNumber = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
			int num2 = 0;
			int num3 = 0;
			string text2 = "";
			string text3 = "";
			for (int i = 0; i < ImportFolderList.CheckedItems.Count; i++)
			{
				text3 = text3 + ImportFolderList.CheckedItems[i].ToString() + ";";
			}
			try
			{
				XmlTextReader reader = new XmlTextReader(ImportFileName);
				if (gf.ValidateEasiSlidesXML(ref reader))
				{
					Cursor = Cursors.WaitCursor;
					ProgressBar1.Visible = true;
					ProgressBar1.Value = 0;
					if (FolderLookupSongsCount < 1)
					{
						FolderLookupSongsCount = 1;
					}
					while (gf.ExtractEasiSlidesXMLItem(ref reader, ref ImportItem))
					{
						text2 = ((ImportItem.FolderName == "") ? "Default Folder" : ImportItem.FolderName);
						num++;
						num2 = num * 100 / FolderLookupSongsCount;
						ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
						Invalidate();
						ProgressBar1.Invalidate();
						if (text3.IndexOf(text2 + ";") >= 0)
						{
							num3++;
							listViewItem = SongsList.Items.Add(num3.ToString());
							listViewItem.SubItems.Add(ImportItem.Title);
							string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + ImportItem.Title + "\"";
							bool flag = true;

#if OleDb
			                using DataTable dataTable = DbOleDbController.getDataTable(daoDb, fullSearchString);
#elif SQLite
							using DataTable dataTable = DbController.GetDataTable(connection, fullSearchString);
#endif

							if (dataTable.Rows.Count > 0)
							{
								//recordset.MoveFirst();
								CurSongID = DataUtil.GetDataInt(dataTable.Rows[0], "SongID");
								if (OptImport0.Checked)
								{
									flag = false;
								}
								else if (OptImport1.Checked)
								{
									CurSongID = -1;
								}
							}
							else
							{
								CurSongID = -1;
							}


							if (flag)
							{
								if (ImportItem.Title != "")
								{
									ImportItem.CompleteLyrics = ImportItem.CompleteLyrics.TrimStart('\n', '\r');
									ImportItem.CompleteLyrics = ImportItem.CompleteLyrics.Replace("\r\n", "\n");
									ImportItem.FolderNo = folderNumber;
									SaveSong(flag, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
								}
								else
								{
									listViewItem.SubItems.Add("Item has No Title - Not Imported");
								}
							}
							else
							{
								listViewItem.SubItems.Add("Song exists in Database - NOT Imported");
							}
						}
						SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
						SongsList.Update();
					}
					Cursor = Cursors.Default;
					ProgressBar1.Value = 100;
					Show_Import_Result();
				}
				else
				{
					MessageBox.Show("Selected XML File is not formatted correctly for EasiSlides use. Import NOT Done");
				}
				reader?.Close();
			}
			catch
			{
			}
			Cursor = Cursors.Default;
		}

		private void Show_Import_Result()
		{
			ListViewItem listViewItem = new ListViewItem();
			string text = "";
			string text2 = "";
			string text3 = "";
			if ((SongsNew == 0) & (SongsUpdated == 0))
			{
				text = "No Songs were imported. ";
			}
			else
			{
				if (SongsNew >= 1)
				{
					text2 = ((SongsNew != 1) ? ("Imported " + Convert.ToString(SongsNew) + " new songs. ") : "Imported one new song. ");
				}
				if (SongsUpdated >= 1)
				{
					text3 = ((SongsUpdated != 1) ? ("Replaced " + Convert.ToString(SongsUpdated) + " existing songs. ") : "Replaced one existing song. ");
				}
			}
			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add("Completed.");
			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add(text + text2 + text3);
			SongsList.Items.Add("");
			SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
			SongsList.Update();
			MessageBox.Show("Completed. " + text + text2 + text3);
			ProgressBar1.Visible = false;
			string strExt = Path.GetExtension(tbImportFrom.Text).ToLower();
			if (strExt == ".mdb")
			{
				tbImportFrom.Text = "";
			}
		}

		private string GetImportFolderNumber(string InFolderName)
		{
			for (int i = 1; i < 41; i++)
			{
				if (EsfFolderNames[i] == InFolderName)
				{
					return i.ToString();
				}
			}
			return "0";
		}

		private void SaveSong(bool SongRequired, int InSongID, SongSettings InItem, ref ListViewItem cItem, ref int SongsNew, ref int SongsUpdated)
		{
			if (SongRequired)
			{
				if (CurSongID < 1)
				{
					gf.InsertItemIntoDatabase(gf.ConnectStringMainDB, InItem);
					cItem.SubItems.Add("++ Imported as a New Item ++");
					SongsNew++;
				}
				else
				{
					gf.UpdateDatabaseItem(gf.ConnectStringMainDB, InItem, CurSongID);
					cItem.SubItems.Add("** Existing Item in Database Replaced **");
					SongsUpdated++;
				}
			}
			else
			{
				cItem.SubItems.Add("Not Imported");
			}
		}

		private void Import_TextFormat(string ImportFileName)
		{
			string InString = "";
			if (!gfFileHelpers.LoadFileContents(ImportFileName, ref InString))
			{
				MessageBox.Show("There was an error reading the Import File - the file might not be a valid EasiSlides File");
				ProgressBar1.Visible = false;
				return;
			}

			TextImportFormat = DataUtil.Left(InString, 8).ToLower();
			if (TextImportFormat == "[esf1.0]")
			{
				esfSongTitle = esf1SongTitle;
				esfSongTitle2 = esf1SongTitle2;
				esfSongNumber = esf1SongNumber;
				esfSongFolder = esf1SongFolder;
				esfBookReference = esf1BookReference;
				esfUserReference = esf1UserReference;
				esfSongCopyright = esf1SongCopyright;
				esfSongWriterInfo = esf1SongWriterInfo;
				esfSongKey = esf1SongKey;
				esfSongTiming = esf1SongTiming;
				esfSongCapo = esf1SongCapo;
				esfSongAdmin1 = esf1SongAdmin1;
				esfSongAdmin2 = esf1SongAdmin2;
				esfSequence = esf1Sequence;
				esfSongFormat = esf1SongFormat;
				esfImportFieldSeparator = "#";
				esfImportFormatTitle = "[" + esfImportFieldSeparator;
			}
			else
			{
				if (!(TextImportFormat == "[est3.1]"))
				{
					MessageBox.Show("There was an error reading the Import File - the file might not be a valid EasiSlides File");
					ProgressBar1.Visible = false;
					return;
				}
				esfSongTitle = "[>";
				esfSongTitle2 = ">>";
				esfSongNumber = ">n";
				esfSongFolder = ">f";
				esfBookReference = ">r";
				esfUserReference = ">u";
				esfSongCopyright = ">c";
				esfSongWriterInfo = ">w";
				esfSongKey = ">k";
				esfSongTiming = ">t";
				esfSongCapo = ">0";
				esfSongAdmin1 = ">a";
				esfSongAdmin2 = ">b";
				esfSequence = ">@";
				esfSongFormat = ">q";
				esfImportFieldSeparator = '>'.ToString();
				esfImportFormatTitle = "[>";
			}
			InString = DataUtil.Right(InString, InString.Length - 8);
			InString = InString.Replace(esfImportFormatTitle, Convert.ToString('\u0001'));
			Cursor = Cursors.WaitCursor;
			int folderNumber = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
			int num = 0;
			ListViewItem listViewItem = new ListViewItem();
			string[] array = InString.Split('\u0001');
			string text = "";
			for (int i = 0; i < ImportFolderList.CheckedItems.Count; i++)
			{
				text = text + ImportFolderList.CheckedItems[i].ToString() + ";";
			}
#if OleDb
			using OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB);
#elif SQLite
			using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
#endif

			ProgressBar1.Visible = true;
			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add("Starting Import...");
			if (array != null && array.GetUpperBound(0) >= 0)
			{
				int num2 = 0;
				int num3 = -1;
				int num4 = -1;
				int num5 = -1;
				int num6 = 0;
				string text2 = "";
				string text3 = "";
				string text4 = "";
				SongsUpdated = 0;
				SongsNew = 0;
				for (int i = 0; i <= array.GetUpperBound(0); i++)
				{
					num5 = array[i].IndexOf("]");
					text2 = DataUtil.Left(array[i], (num5 > 0) ? num5 : 0);
					if (FolderLookupSongsCount < 1)
					{
						FolderLookupSongsCount = 1;
					}
					if (text2.Length > 0)
					{
						num3 = text2.IndexOf(esfSongFolder);
						if ((num3 >= 0) & (text2.Length > num3))
						{
							num4 = text2.IndexOf(esfImportFieldSeparator, num3 + 1);
							text3 = ((num4 <= 0) ? DataUtil.Mid(text2, num3 + esfSongFolder.Length) : DataUtil.Mid(text2, num3 + esfSongFolder.Length, num4 - (num3 + esfSongFolder.Length)));
						}
						else
						{
							text3 = "Default Folder";
						}
						num2++;
						num = num2 * 100 / FolderLookupSongsCount;
						ProgressBar1.Value = ((num > 100) ? 100 : num);
						Invalidate();
						ProgressBar1.Invalidate();
						if (text.IndexOf(text3 + ";") >= 0)
						{
							gf.InitialiseIndividualData(ref ImportItem);
							LoadTextFileHeaderToItem(ref ImportItem, text2);
							text4 = DataUtil.Mid(array[i], (num5 + 1 < array[i].Length) ? (num5 + 1) : 0);
							if (text4.IndexOf("[~") >= 0)
							{
								int num7 = text4.IndexOf("[~") + "[~".Length;
								int num8 = text4.IndexOf("]", num7);
								if (num8 > num7)
								{
									ImportItem.Notations = DataUtil.Mid(text4, num7, num8 - num7);
									text4 = DataUtil.Mid(text4, num8 + 3);
								}
								else
								{
									text4 = "";
								}
							}
							text4 = text4.TrimStart('\n', '\r');
							num6++;
							listViewItem = SongsList.Items.Add(num6.ToString());
							listViewItem.SubItems.Add(ImportItem.Title);
							string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + ImportItem.Title + "\"";
							bool flag = true;

#if OleDb
			                using DataTable dataTable = DbOleDbController.getDataTable(daoDb, fullSearchString);
#elif SQLite
							using DataTable dataTable = DbController.GetDataTable(connection, fullSearchString);
#endif
							if (dataTable.Rows.Count > 0)
							{
								CurSongID = DataUtil.GetDataInt(dataTable.Rows[0], "SongID");
								if (OptImport0.Checked)
								{
									flag = false;
								}
								else if (OptImport1.Checked)
								{
									CurSongID = -1;
								}
							}
							else
							{
								CurSongID = -1;
							}

							if (flag)
							{
								if (ImportItem.Title != "")
								{
									ImportItem.CompleteLyrics = text4.Replace("\r\n", "\n");
									ImportItem.FolderNo = folderNumber;
									SaveSong(flag, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
								}
								else
								{
									listViewItem.SubItems.Add("Item has No Title - Not Imported");
								}
							}
							else
							{
								listViewItem.SubItems.Add("Song exists in Database - NOT Imported");
							}
						}
					}

					SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
					SongsList.Update();
				}
			}

			ProgressBar1.Value = 100;
			Cursor = Cursors.Default;
			Show_Import_Result();
			return;
		}

		private void LoadTextFileHeaderToItem(ref SongSettings InItem, string InString)
		{
			int num = InString.IndexOf(esfImportFieldSeparator);
			string text = "";
			string text2 = "";
			if (num <= 0)
			{
				return;
			}
			InItem.Title = DataUtil.Left(InString, num);
			InString = DataUtil.Mid(InString, num + 1);
			while (InString.Length > 0)
			{
				num = InString.IndexOf(esfImportFieldSeparator, 1);
				if (num > 0)
				{
					text = DataUtil.Left(InString, num);
					InString = DataUtil.Mid(InString, num + 1);
				}
				else
				{
					text = InString;
					InString = "";
				}
				if (text.Length > 2)
				{
					text2 = esfImportFieldSeparator + text[0];
					text = DataUtil.Mid(text, 1);
				}
				else
				{
					text2 = "";
					text = "";
				}
				if (text2 != "")
				{
					if (text2 == esfSongTitle2)
					{
						InItem.Title2 = text;
					}
					else if (text2 == esfSongFolder)
					{
						InItem.FolderName = text;
					}
					else if (text2 == esfSongNumber)
					{
						InItem.SongNumber = DataUtil.StringToInt(text);
					}
					else if (text2 == esfBookReference)
					{
						InItem.Book_Reference = text;
					}
					else if (text2 == esfUserReference)
					{
						InItem.User_Reference = text;
					}
					else if (text2 == esfSongCopyright)
					{
						InItem.Copyright = text;
					}
					else if (text2 == esfSongWriterInfo)
					{
						InItem.Writer = text;
					}
					else if (text2 == esfSongKey)
					{
						InItem.MusicKey = text;
					}
					else if (text2 == esfSongCapo)
					{
						InItem.Capo = DataUtil.StringToInt(text);
					}
					else if (text2 == esfSongTiming)
					{
						InItem.Timing = text;
					}
					else if (text2 == esfSongAdmin1)
					{
						InItem.Show_LicAdminInfo1 = text;
					}
					else if (text2 == esfSongAdmin2)
					{
						InItem.Show_LicAdminInfo2 = text;
					}
					else if (text2 == esfSequence)
					{
						InItem.SongSequence = gf.ConvertTextStringToSequence(text);
					}
					else if (text2 == esfSongFormat)
					{
						InItem.Format.FormatString = text;
					}
				}
			}
		}

		private void AccessHelper(string ImportFileName)
		{
			string strExt = Path.GetExtension(ImportFileName).ToLower();
			if (strExt == ".mdb")
			{
				gf.Import_AccessFileName = ImportFileName;
				FrmImportAccessHelper frmImportAccessHelper = new FrmImportAccessHelper();
				if (frmImportAccessHelper.ShowDialog() == DialogResult.OK)
				{
					Update();
					Import_AccessDatabase(ImportFileName, UsedHelper: true);
					Update();
				}
			}
		}

		private void Import_AccessDatabase(string ImportFileName, bool UsedHelper)
		{
			if (!UsedHelper)
			{
				gf.Import_SongTitleColumnName = "Title_1";
				gf.Import_SongTitle2ColumnName = "Title_2";
				gf.Import_SongNumberColumnName = "SONG_NUMBER";
				gf.Import_SongCopyrightColumnName = "copyright";
				gf.Import_BookReferenceColumnName = "BOOK_REFERENCE";
				gf.Import_UserReferenceColumnName = "USER_REFERENCE";
				gf.Import_SongWriterInfoColumnName = "writer";
				gf.Import_SongLyricsColumnName = "Lyrics";
				gf.Import_SongKeyColumnName = "key";
				gf.Import_SongTimingColumnName = "Timing";
				gf.Import_Admin1ColumnName = "LICENCE_ADMIN1";
				gf.Import_Admin2ColumnName = "LICENCE_ADMIN2";
			}
			sArray = gf.Import_SongLyricsColumnName.Split('>');
			if (sArray != null)
			{
				for (int i = 0; i <= sArray.GetUpperBound(0); i++)
				{
					sArray[i] = DataUtil.Trim(sArray[i]);
				}
			}
			int num = 0;
			ListViewItem listViewItem = new ListViewItem();
			bool flag = false;
			SongsUpdated = 0;
			SongsNew = 0;
			Cursor = Cursors.WaitCursor;
			SongsList.Items.Clear();
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			int num2 = 0;
#if OleDb

			using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringDef + ImportFileName, "select * from " + gf.Import_TableName);
			if (datatable.Rows.Count == 0)
			{
				return;
			}
			//recordSet.MoveFirst();
			//while (!recordSet.EOF)
			//{
			//	num2++;
			//	recordSet.MoveNext();
			//}
			num2 = datatable.Rows.Count;

			//recordSet.MoveFirst();
			listViewItem = SongsList.Items.Add("Importing...");
			int num3 = 0;
			int folderNumber = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

			using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB))
			{
				DataTable dt = null;
				int num4 = 0;
				//while (!recordSet.EOF)
				foreach (DataRow dr in datatable.Rows)
				{
					flag = false;
					num++;
					num3 = num * 100 / num2;
					ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
					Invalidate();
					ProgressBar1.Invalidate();
					if (LoadExternalAccessDatabaseToItem(ref ImportItem, dr))
					{
						listViewItem = SongsList.Items.Add(Convert.ToString(num));
						listViewItem.SubItems.Add(ImportItem.Title);
						string fullSearchString = "select * from SONG where Folderno=" + folderNumber + " and Title_1 = \"" + ImportItem.Title + "\"";
						flag = true;
						ImportItem.FolderNo = folderNumber;
						dt = DbOleDbController.getDataTable(daoDb, fullSearchString);
					}
					if (dt.Rows.Count > 0)
					{
						//recordset.MoveFirst();
						CurSongID = DataUtil.GetDataInt(dt.Rows[0], "SongID");
						if (OptImport0.Checked)
						{
							flag = false;
						}
						else if (OptImport1.Checked)
						{
							CurSongID = -1;
						}
					}
					else
					{
						CurSongID = -1;
					}
					dt.Dispose();
					num4 = 0;
					SaveSong(flag, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
					//recordSet.MoveNext();
					SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
					SongsList.Update();
				}
				datatable.Dispose();
				ProgressBar1.Value = 100;
				Show_Import_Result();
				if (dt != null)
				{
					dt = null;
				}
			}
#elif SQLite
			using DataTable dataTable = DbController.GetDataTable(gf.ConnectSQLiteDef + ImportFileName, "select * from " + gf.Import_TableName);
			if (dataTable.Rows.Count <= 0)
			{
				return;
			}

			num2 = dataTable.Rows.Count;

			listViewItem = SongsList.Items.Add("Importing...");
			int num3 = 0;
			int folderNumber = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

			using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
			DataTable dt = null;
			
			foreach (DataRow dr in dataTable.Rows)
			{
				flag = false;
				num++;
				num3 = num * 100 / num2;
				ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
				
				Invalidate();

				ProgressBar1.Invalidate();

				if (LoadExternalAccessDatabaseToItem(ref ImportItem, dr))
				{
					listViewItem = SongsList.Items.Add(Convert.ToString(num));
					listViewItem.SubItems.Add(ImportItem.Title);
					string fullSearchString = "select * from SONG where Folderno=" + folderNumber + " and Title_1 = \"" + ImportItem.Title + "\"";
					flag = true;
					ImportItem.FolderNo = folderNumber;
					dt = DbController.GetDataTable(connection, fullSearchString);
				}
				
				if (dt.Rows.Count > 0)
				{
					CurSongID = DataUtil.GetDataInt(dt.Rows[0], "SongID");
					if (OptImport0.Checked)
					{
						flag = false;
					}
					else if (OptImport1.Checked)
					{
						CurSongID = -1;
					}
				}
				else
				{
					CurSongID = -1;
				}

				if (dt != null)
					dt.Dispose();

				SaveSong(flag, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);

				SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
				SongsList.Update();
			}

			ProgressBar1.Value = 100;
			Show_Import_Result();			
#endif
			Cursor = Cursors.Default;
		}

		private bool LoadExternalAccessDatabaseToItem(ref SongSettings InItem, DataRow rsIm)
		{
			try
			{
				InItem.Title = DataUtil.GetDataString(rsIm, gf.Import_SongTitleColumnName);
				InItem.Title2 = DataUtil.GetDataString(rsIm, gf.Import_SongTitle2ColumnName);
				InItem.SongNumber = DataUtil.GetDataInt(rsIm, gf.Import_SongNumberColumnName);
				InItem.CompleteLyrics = GetMergedSongLyrics(rsIm);
				InItem.Copyright = DataUtil.GetDataString(rsIm, gf.Import_SongCopyrightColumnName);
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(rsIm, gf.Import_Admin1ColumnName);
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(rsIm, gf.Import_Admin2ColumnName);
				InItem.Notations = DataUtil.GetDataString(rsIm, "msc");
				InItem.Capo = DataUtil.GetDataInt(rsIm, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(rsIm, "Sequence");
				InItem.Writer = DataUtil.GetDataString(rsIm, gf.Import_SongWriterInfoColumnName);
				InItem.Book_Reference = DataUtil.GetDataString(rsIm, gf.Import_BookReferenceColumnName);
				InItem.User_Reference = DataUtil.GetDataString(rsIm, gf.Import_UserReferenceColumnName);
				InItem.Timing = DataUtil.GetDataString(rsIm, gf.Import_SongTimingColumnName);
				InItem.MusicKey = DataUtil.GetDataString(rsIm, gf.Import_SongKeyColumnName);
				InItem.Format.FormatString = "";
				return true;
			}
			catch
			{
				return false;
			}
		}

		private string GetMergedSongLyrics(DataRow rs)
		{
			string text = "";
			string text2 = "";
			if (sArray != null)
			{
				for (int i = 0; i <= sArray.GetUpperBound(0); i++)
				{
					text = "";
					text = DataUtil.TrimEnd(Convert.ToString(DataUtil.GetDataString(rs, sArray[i])));
					if (text != "")
					{
						text2 = text2 + text + "\n\n";
					}
				}
			}
			return DataUtil.TrimEnd(text2);
		}

		private void BtnImportHelper_Click(object sender, EventArgs e)
		{
			if (DataUtil.Trim(tbImportFrom.Text) == "")
			{
				MessageBox.Show("Cannot start Helper - Please specify an Access Database file.");
				return;
			}
			if (SongFolder.SelectedItems.Count < 1)
			{
				MessageBox.Show("Cannot start Helper - Please select a Song Folder to Import Access database items into.");
				return;
			}
			string text = DataUtil.Trim(tbImportFrom.Text);
			if (!File.Exists(text))
			{
				MessageBox.Show("Cannot start Helper - The Access Database file specified does not exist - please select a valid import file!");
			}
			else if (Path.GetExtension(text).ToLower() == ".mdb")
			{
				gf.Import_AccessFileName = text;
				FrmImportAccessHelper frmImportAccessHelper = new FrmImportAccessHelper();
				if (frmImportAccessHelper.ShowDialog() == DialogResult.OK)
				{
					Import_AccessDatabase(text, UsedHelper: true);
					Update();
				}
			}
			else
			{
				MessageBox.Show("Cannot start Helper - The specified file is not an Access Database file");
			}
		}

		private void FrmImport_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.T)
			{
				DoSourceCDExtract();
			}
		}

		private void DoSourceCDExtract()
		{
			if (SongFolder.SelectedItems.Count < 1)
			{
				MessageBox.Show("Please select a Folder to Import the Source materials into (C).");
				return;
			}
			string text = "D:\\Source CD\\hymns";
			if (!Directory.Exists(text))
			{
				MessageBox.Show("The source CD Folder '" + text + "' does not exists!");
				return;
			}
			string[] SourceCDSongTitle = new string[2600];
			if (DoSourceCDIndexExtract(ref SourceCDSongTitle))
			{
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB))
				{
					DataTable dt = null;
					try
					{
						ListViewItem listViewItem = new ListViewItem();
						bool flag = false;
						listViewItem = SongsList.Items.Add("Importing...");
						int num = 0;
						SongsNew = 0;
						SongsUpdated = 0;
						string[] files = Directory.GetFiles(text, "*.html");
						int num2 = files.GetUpperBound(0) + 1;
						ProgressBar1.Visible = true;
						ProgressBar1.Value = 0;
						string text2 = "";
						int num3 = 0;
						int folderNumber = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
						string[] array = files;
						foreach (string importFileName in array)
						{
							num++;
							num3 = num * 100 / num2;
							ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
							Invalidate();
							ProgressBar1.Invalidate();
							ExtractOneSourceCDHTMLIntoItem(ref ImportItem, importFileName, ref SourceCDSongTitle);
							listViewItem = SongsList.Items.Add(num.ToString());
							listViewItem.SubItems.Add(ImportItem.Title);
							string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + ImportItem.Title + "\"";
							flag = true;
							dt = DbOleDbController.getDataTable(daoDb, fullSearchString);
							if (dt.Rows.Count>0)
							{
								//recordset.MoveFirst();
								CurSongID = DataUtil.GetDataInt(dt.Rows[0], "SongID");
								if (OptImport0.Checked)
								{
									flag = false;
								}
								else if (OptImport1.Checked)
								{
									CurSongID = -1;
								}
							}
							else
							{
								CurSongID = -1;
							}
							dt.Dispose();

							if (flag)
							{
								if (ImportItem.Title != "")
								{
									ImportItem.FolderNo = folderNumber;
#if DAO
									string fullSearchString2 = "select * from SONG where Folderno > 0 and UCase(book_reference) like \"*" + ImportItem.Book_Reference + "*\"";
#elif SQLite
									string fullSearchString2 = "select * from SONG where Folderno > 0 and upper(book_reference) like \"%" + ImportItem.Book_Reference + "%\"";
#endif
									dt = DbOleDbController.getDataTable(daoDb, fullSearchString2);
									if (dt.Rows.Count>0)
									{
										//recordset.MoveFirst();
										bool flag2 = false;
										string text3 = "";
										//while (!recordset.EOF && !flag2)
										//
										foreach(DataRow dr in dt.Rows)
										{
											if (flag2) break;

											text3 = DataUtil.GetDataString(dr, "book_reference");
											text2 = text3;
											while (text2.Length > 0 && !flag2)
											{
												if (DataUtil.ExtractOneInfo(ref text2, ',').ToUpper().TrimStart(' ') == ImportItem.Book_Reference)
												{
													ImportItem.Book_Reference = text3;
													flag2 = true;
												}
											}
											//recordset.MoveNext();
										}
									}
									SaveSong(flag, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
								}
								else
								{
									listViewItem.SubItems.Add("Item has No Title - Not Imported");
								}
							}
							else
							{
								listViewItem.SubItems.Add("Song exists in Database - NOT Imported");
							}
							SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
							SongsList.Update();
						}
					}
					catch
					{
					}
					ProgressBar1.Value = 100;
					Cursor = Cursors.Default;
					Show_Import_Result();
					if (dt != null)
					{
						dt = null;
					}
				}
#elif SQLite
				using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);

				try
				{
					ListViewItem listViewItem = new ListViewItem();
					bool flag = false;
					listViewItem = SongsList.Items.Add("Importing...");
					int num = 0;
					SongsNew = 0;
					SongsUpdated = 0;
					string[] files = Directory.GetFiles(text, "*.html");
					int num2 = files.GetUpperBound(0) + 1;
					ProgressBar1.Visible = true;
					ProgressBar1.Value = 0;
					string text2 = "";
					int num3 = 0;
					int folderNumber = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					string[] array = files;
					foreach (string importFileName in array)
					{
						num++;
						num3 = num * 100 / num2;
						ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
						Invalidate();
						ProgressBar1.Invalidate();
						ExtractOneSourceCDHTMLIntoItem(ref ImportItem, importFileName, ref SourceCDSongTitle);
						listViewItem = SongsList.Items.Add(num.ToString());
						listViewItem.SubItems.Add(ImportItem.Title);
						string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + ImportItem.Title + "\"";
						flag = true;

						using DataTable dataTable = DbController.GetDataTable(connection, fullSearchString);
						if (dataTable.Rows.Count > 0)
						{
							//recordset.MoveFirst();
							CurSongID = DataUtil.GetDataInt(dataTable.Rows[0], "SongID");
							if (OptImport0.Checked)
							{
								flag = false;
							}
							else if (OptImport1.Checked)
							{
								CurSongID = -1;
							}
						}
						else
						{
							CurSongID = -1;
						}
						

						if (flag)
						{
							if (ImportItem.Title != "")
							{
								ImportItem.FolderNo = folderNumber;
								string fullSearchString2 = "select * from SONG where Folderno > 0 and upper(book_reference) like \"%" + ImportItem.Book_Reference + "%\"";

								using DataTable dataTable1 = DbController.GetDataTable(connection, fullSearchString2);
								if (dataTable1.Rows.Count > 0)
								{
									bool flag2 = false;
									string text3 = "";

									foreach (DataRow dr in dataTable1.Rows)
									{
										if (flag2) break;

										text3 = DataUtil.GetDataString(dr, "book_reference");
										text2 = text3;
										while (text2.Length > 0 && !flag2)
										{
											if (DataUtil.ExtractOneInfo(ref text2, ',').ToUpper().TrimStart(' ') == ImportItem.Book_Reference)
											{
												ImportItem.Book_Reference = text3;
												flag2 = true;
											}
										}
									}
								}
								SaveSong(flag, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
							}
							else
							{
								listViewItem.SubItems.Add("Item has No Title - Not Imported");
							}
						}
						else
						{
							listViewItem.SubItems.Add("Song exists in Database - NOT Imported");
						}
						SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
						SongsList.Update();
					}
				}
				catch (Exception ex)	
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
				ProgressBar1.Value = 100;
				Cursor = Cursors.Default;
				Show_Import_Result();
#endif
			}
		}

		private bool DoSourceCDIndexExtract(ref string[] SourceCDSongTitle)
		{
			for (int i = 0; i < 2600; i++)
			{
				SourceCDSongTitle[i] = "";
			}
			string text = "D:\\Source CD\\hymnindex.htm";
			if (!File.Exists(text))
			{
				return false;
			}
			string text2 = gfFileHelpers.LoadTextFile(text);
			text2 = text2.Replace("<tr>", Convert.ToString('\u0001'));
			string[] array = text2.Split('\u0001');
			string text3 = "";
			string text4 = "";
			try
			{
				for (int i = 4; i <= 2532; i++)
				{
					array[i] = array[i].Replace('\n', ' ');
					if (array[i].IndexOf("<em>") < 0)
					{
						text3 = ExtractOneHTMLStream(ref array[i]).Replace(",", "");
						text4 = ExtractOneHTMLStream(ref array[i]);
						if (text3.Length > 0 && DataUtil.StringToInt(text4) > 0)
						{
							SourceCDSongTitle[DataUtil.StringToInt(text4)] = text3;
						}
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		private string ExtractOneHTMLStream(ref string InString)
		{
			string text = "";
			int num = InString.IndexOf('>');
			int num2 = InString.IndexOf('<', num + 1);
			while ((num >= 0 && num2 >= 0) & (num2 > num))
			{
				text = DataUtil.Trim(DataUtil.Mid(InString, num + 1, num2 - (num + 1)));
				if (text.Length > 0)
				{
					InString = DataUtil.Mid(InString, num2 + 1);
					return text;
				}
				num = InString.IndexOf('>', num2);
				num2 = InString.IndexOf('<', (num >= 0) ? num : (InString.Length - 1));
			}
			InString = "";
			return "";
		}

		private void ExtractOneSourceCDHTMLIntoItem(ref SongSettings InItem, string ImportFileName, ref string[] SourceCDSongTitle)
		{
			if (!File.Exists(ImportFileName))
			{
				return;
			}
			string text = gfFileHelpers.LoadTextFile(ImportFileName);
			int num = DataUtil.StringToInt(gf.GetDisplayNameOnly(ref ImportFileName, UpdateByRef: false, KeepExt: false));
			text = text.Replace("InstanceBeginEditable name=", Convert.ToString('\u0001'));
			string[] array = text.Split('\u0001');
			string[] array2 = new string[4];
			string value = "<!--";
			string text2 = "";
			array2[0] = "\"content\" -->";
			array2[1] = "\"Author\" -->";
			array2[2] = "\"copyright\" -->";
			string[] array3 = new string[4]
			{
				"",
				"",
				"",
				null
			};
			int num2 = -1;
			int num3 = -1;
			if (array != null && array.GetUpperBound(0) >= 0)
			{
				for (int i = 0; i <= array.GetUpperBound(0); i++)
				{
					for (int j = 0; j < 3; j++)
					{
						num2 = array[i].IndexOf(array2[j]);
						if (num2 >= 0)
						{
							num3 = array[i].IndexOf(value, num2 + array2[j].Length);
							num3 = ((num3 < num2 + array2[j].Length) ? array[i].Length : num3);
							array3[j] = DataUtil.Mid(array[i], num2 + array2[j].Length, num3 - (num2 + array2[j].Length));
						}
					}
				}
			}
			gf.InitialiseIndividualData(ref ImportItem);
			if (num <= 0)
			{
				return;
			}
			InItem.Title = SourceCDSongTitle[num];
			if (InItem.Title != "")
			{
				InItem.Book_Reference = "TS" + num;
				InItem.SongSequence = ConvertHTMLLines(ref array3[0]);
				InItem.CompleteLyrics = array3[0];
				InItem.Writer = RemoveHTMLTags(array3[1].Trim());
				InItem.Copyright = RemoveHTMLTags(array3[2].Trim());
				if (InItem.Copyright.ToLower().IndexOf("public domain") >= 0 || InItem.Copyright.Length == 0)
				{
					InItem.Show_LicAdminInfo1 = "Public Domain";
				}
				else
				{
					InItem.Show_LicAdminInfo1 = "CCLI";
				}
			}
		}

		private string ConvertHTMLLines(ref string InContents)
		{
			InContents = InContents.Replace("\r", "");
			InContents = InContents.Replace("\n", "");
			InContents = InContents.Replace("<p>", "<br><br>");
			InContents = InContents.Replace("<br>", Convert.ToString('\u0001'));
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = InContents.Split('\u0001');
			bool flag = false;
			if (array != null && array.GetUpperBound(0) >= 0)
			{
				bool flag2 = false;
				for (int i = 0; i <= array.GetUpperBound(0); i++)
				{
					if (array[i].IndexOf("<em>") >= 0 && !flag)
					{
						flag = true;
						stringBuilder.Append(gf.VerseSymbol[0] + "\n");
					}
					array[i] = RemoveHTMLTags(array[i]);
					flag2 = ((i < array.GetUpperBound(0) && array[i + 1].IndexOf("&nbsp;") >= 0) ? true : false);
					stringBuilder.Append(array[i] + (flag2 ? " " : "\n"));
				}
			}
			InContents = stringBuilder.ToString();
			return InsertVertIndicators(ref InContents);
		}

		private string InsertVertIndicators(ref string InString)
		{
			string text = InString;
			string text2 = "";
			text = text.TrimStart('\n', '\r');
			text = text.TrimEnd('\n', '\r');
			text = text.Replace("\n\n", Convert.ToString('\u0001'));
			string[] array = text.Split('\u0001');
			if (array != null && array.GetUpperBound(0) >= 0)
			{
				int num = (array[array.GetUpperBound(0)] != "") ? array.GetUpperBound(0) : (array.GetUpperBound(0) - 1);
				if (num > 0)
				{
					if (array[0].IndexOf(gf.VerseSymbol[0]) >= 0)
					{
						text2 += '\0';
						for (int i = 1; i <= num; i++)
						{
							text2 += (char)i;
							text2 += '\0';
							array[i] = gf.VerseSymbol[i] + "\n" + array[i].TrimStart('\n', '\r');
						}
						if (num < 2)
						{
							text2 = "";
						}
					}
					else if (array[1].IndexOf(gf.VerseSymbol[0]) >= 0 && num > 1)
					{
						text2 += '\u0001';
						text2 += '\0';
						array[0] = gf.VerseSymbol[1] + "\n" + array[0].TrimStart('\n', '\r');
						for (int i = 2; i <= num; i++)
						{
							text2 += (char)i;
							text2 += '\0';
							array[i] = gf.VerseSymbol[i] + "\n" + array[i].TrimStart('\n', '\r');
						}
					}
					else if (text.IndexOf(gf.VerseSymbol[0]) < 0)
					{
						for (int i = 0; i <= num; i++)
						{
							array[i] = gf.VerseSymbol[i + 1] + "\n" + array[i].TrimStart('\n', '\r');
						}
					}
					InString = "";
					for (int i = 0; i <= num; i++)
					{
						InString = InString + array[i].TrimEnd('\n', '\r') + ((i < num) ? "\n" : "");
					}
					return text2;
				}
				InString = array[0];
				return "";
			}
			return "";
		}

		private string RemoveHTMLTags(string InContents)
		{
			if (InContents.Length == 0)
			{
				return "";
			}
			InContents = InContents.Replace("&nbsp;", "");
			string text = "";
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < InContents.Length; i++)
			{
				if (InContents[i] == '<')
				{
					flag = true;
				}
				if (!flag && (InContents[i] != ' ' || !flag2))
				{
					text += InContents[i];
				}
				if (InContents[i] == '>')
				{
					flag = false;
				}
				flag2 = ((InContents[i] == ' ') ? true : false);
			}
			return text.Trim();
		}
	}
}
