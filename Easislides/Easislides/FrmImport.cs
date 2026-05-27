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
    public partial class FrmImport : Form
	{

		private string TextImportFormat;

		private int CurSongID;

		private int SongsNew;

		private int SongsUpdated;

		private int InFolderNo;

		private string[] sArray;

		private string[] EsfFolderNames = new string[Gf.MAXSONGSFOLDERS];

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
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (Gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(Gf.FolderName[i]);
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

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectSQLiteDef + ImportFileName);

			for (int i = 0; i < Gf.MAXSONGSFOLDERS; i++)
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

			using DataTable dt = DbController.GetDataTable(connection, fullSearchString);

			if (dt.Rows.Count <= 0)
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
				if (Gf.ValidateEasiSlidesXML(ref reader))
				{
					while (Gf.ExtractEasiSlidesXMLItem(ref reader, ref ImportItem))
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
			OpenFileDialog1.InitialDirectory = Gf.ImportFromDir;
			OpenFileDialog1.AddExtension = true;
			OpenFileDialog1.DefaultExt = "*.xml";
			OpenFileDialog1.FileName = "";
			if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
			{
				tbImportFrom.Text = OpenFileDialog1.FileName;
				Gf.ImportFromDir = Path.GetDirectoryName(tbImportFrom.Text) + "\\";
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

			using DataTable dataTable = DbController.GetDataTable(Gf.ConnectStringDef + ImportFileName, "select * from SONG " + text);

			num2 = dataTable.Rows.Count;

			if (num2 > 0)
			{
				dataTable.BeginInit();

				listViewItem = SongsList.Items.Add("Importing...");
				int num3 = 0;
				InFolderNo = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

				//DataTable dt = DbController.GetDataTable(connection, fullSearchString);

				DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
				//Database daoDb = DbDaoController.GetDaoDb(Gf.ConnectStringMainDB);

				DataRow dr = null;

				foreach (DataRow row in dataTable.Rows)
				{
					bool songRequired = false;
					num++;
					num3 = num * 100 / num2;
					ProgressBar1.Value = ((num3 > 100) ? 100 : num3);
					Invalidate();
					ProgressBar1.Invalidate();
					if (Gf.LoadDataIntoItem(ref ImportItem, row))
					{
						listViewItem = SongsList.Items.Add(Convert.ToString(num));
						listViewItem.SubItems.Add(ImportItem.Title);
						string fullSearchString = "select * from SONG where Folderno=" + InFolderNo + " and Title_1 = \"" + EscapeSqlLiteral(ImportItem.Title) + "\"";
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

		}

		private void Import_XMLFormat(string ImportFileName)
		{
			int num = 0;
			ListViewItem listViewItem = new ListViewItem();

			SongsUpdated = 0;
			SongsNew = 0;
			SongsList.Items.Clear();

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add("Starting Import...");
			int folderNumber = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
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
				if (Gf.ValidateEasiSlidesXML(ref reader))
				{
					Cursor = Cursors.WaitCursor;
					ProgressBar1.Visible = true;
					ProgressBar1.Value = 0;
					if (FolderLookupSongsCount < 1)
					{
						FolderLookupSongsCount = 1;
					}
					while (Gf.ExtractEasiSlidesXMLItem(ref reader, ref ImportItem))
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
							string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + EscapeSqlLiteral(ImportItem.Title) + "\"";
							bool flag = true;

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
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
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
					Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, InItem);
					cItem.SubItems.Add("++ Imported as a New Item ++");
					SongsNew++;
				}
				else
				{
					Gf.UpdateDatabaseItem(Gf.ConnectStringMainDB, InItem, CurSongID);
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
			int folderNumber = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
			int num = 0;
			ListViewItem listViewItem = new ListViewItem();
			string[] array = InString.Split('\u0001');
			string text = "";
			for (int i = 0; i < ImportFolderList.CheckedItems.Count; i++)
			{
				text = text + ImportFolderList.CheckedItems[i].ToString() + ";";
			}

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

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
							Gf.InitialiseIndividualData(ref ImportItem);
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
							string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + EscapeSqlLiteral(ImportItem.Title) + "\"";
							bool flag = true;

							using DataTable dataTable = DbController.GetDataTable(connection, fullSearchString);
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
						InItem.SongSequence = Gf.ConvertTextStringToSequence(text);
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
				Gf.Import_AccessFileName = ImportFileName;
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
				Gf.Import_SongTitleColumnName = "Title_1";
				Gf.Import_SongTitle2ColumnName = "Title_2";
				Gf.Import_SongNumberColumnName = "SONG_NUMBER";
				Gf.Import_SongCopyrightColumnName = "copyright";
				Gf.Import_BookReferenceColumnName = "BOOK_REFERENCE";
				Gf.Import_UserReferenceColumnName = "USER_REFERENCE";
				Gf.Import_SongWriterInfoColumnName = "writer";
				Gf.Import_SongLyricsColumnName = "Lyrics";
				Gf.Import_SongKeyColumnName = "key";
				Gf.Import_SongTimingColumnName = "Timing";
				Gf.Import_Admin1ColumnName = "LICENCE_ADMIN1";
				Gf.Import_Admin2ColumnName = "LICENCE_ADMIN2";
			}
			sArray = Gf.Import_SongLyricsColumnName.Split('>');
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

			using DataTable dataTable = DbController.GetDataTable(Gf.ConnectSQLiteDef + ImportFileName, "select * from " + Gf.Import_TableName);
			if (dataTable.Rows.Count <= 0)
			{
				return;
			}

			num2 = dataTable.Rows.Count;

			listViewItem = SongsList.Items.Add("Importing...");
			int num3 = 0;
			int folderNumber = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
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
					string fullSearchString = "select * from SONG where Folderno=" + folderNumber + " and Title_1 = \"" + EscapeSqlLiteral(ImportItem.Title) + "\"";
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

			Cursor = Cursors.Default;
		}

		private bool LoadExternalAccessDatabaseToItem(ref SongSettings InItem, DataRow rsIm)
		{
			try
			{
				InItem.Title = DataUtil.GetDataString(rsIm, Gf.Import_SongTitleColumnName);
				InItem.Title2 = DataUtil.GetDataString(rsIm, Gf.Import_SongTitle2ColumnName);
				InItem.SongNumber = DataUtil.GetDataInt(rsIm, Gf.Import_SongNumberColumnName);
				InItem.CompleteLyrics = GetMergedSongLyrics(rsIm);
				InItem.Copyright = DataUtil.GetDataString(rsIm, Gf.Import_SongCopyrightColumnName);
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(rsIm, Gf.Import_Admin1ColumnName);
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(rsIm, Gf.Import_Admin2ColumnName);
				InItem.Notations = DataUtil.GetDataString(rsIm, "msc");
				InItem.Capo = DataUtil.GetDataInt(rsIm, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(rsIm, "Sequence");
				InItem.Writer = DataUtil.GetDataString(rsIm, Gf.Import_SongWriterInfoColumnName);
				InItem.Book_Reference = DataUtil.GetDataString(rsIm, Gf.Import_BookReferenceColumnName);
				InItem.User_Reference = DataUtil.GetDataString(rsIm, Gf.Import_UserReferenceColumnName);
				InItem.Timing = DataUtil.GetDataString(rsIm, Gf.Import_SongTimingColumnName);
				InItem.MusicKey = DataUtil.GetDataString(rsIm, Gf.Import_SongKeyColumnName);
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
				Gf.Import_AccessFileName = text;
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
				string fullSearchString2 = "select * from SONG where Folderno > 0 and upper(book_reference) like \"%" + ImportItem.Book_Reference + "%\"";
				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
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
					int folderNumber = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
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
						string fullSearchString = "select * from SONG where Folderno=" + Convert.ToString(folderNumber) + " and Title_1 = \"" + EscapeSqlLiteral(ImportItem.Title) + "\"";
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
								string fullSearchString3 = "select * from SONG where Folderno > 0 and upper(book_reference) like \"%" + ImportItem.Book_Reference + "%\"";

								using DataTable dataTable1 = DbController.GetDataTable(connection, fullSearchString3);
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
			int num = DataUtil.StringToInt(Gf.GetDisplayNameOnly(ref ImportFileName, UpdateByRef: false, KeepExt: false));
			text = text.Replace("InstanceBeginEditable name=", Convert.ToString('\u0001'));
			string[] array = text.Split('\u0001');
			string[] array2 = new string[4];
			string value = "<!--";
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
			Gf.InitialiseIndividualData(ref ImportItem);
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
						stringBuilder.Append(Gf.VerseSymbol[0] + "\n");
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
					if (array[0].IndexOf(Gf.VerseSymbol[0]) >= 0)
					{
						text2 += '\0';
						for (int i = 1; i <= num; i++)
						{
							text2 += (char)i;
							text2 += '\0';
							array[i] = Gf.VerseSymbol[i] + "\n" + array[i].TrimStart('\n', '\r');
						}
						if (num < 2)
						{
							text2 = "";
						}
					}
					else if (array[1].IndexOf(Gf.VerseSymbol[0]) >= 0 && num > 1)
					{
						text2 += '\u0001';
						text2 += '\0';
						array[0] = Gf.VerseSymbol[1] + "\n" + array[0].TrimStart('\n', '\r');
						for (int i = 2; i <= num; i++)
						{
							text2 += (char)i;
							text2 += '\0';
							array[i] = Gf.VerseSymbol[i] + "\n" + array[i].TrimStart('\n', '\r');
						}
					}
					else if (text.IndexOf(Gf.VerseSymbol[0]) < 0)
					{
						for (int i = 0; i <= num; i++)
						{
							array[i] = Gf.VerseSymbol[i + 1] + "\n" + array[i].TrimStart('\n', '\r');
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

		private static string EscapeSqlLiteral(string value)
		{
			return string.IsNullOrEmpty(value) ? "" : value.Replace("\"", "\"\"");
		}
	}
}
