//using NetOffice.DAOApi;
using Axicultus.Properties;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data;
using Axicultus.Localization;
using Axicultus.Util;

using Axicultus.SQLite;
using Axicultus.Module;
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

		private SongSettings ImportItem = new();

		public FrmImport()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
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
				if (Gf.Folders[i].Use > 0)
				{
					SongFolder.Items.Add(Gf.Folders[i].Name);
				}
			}
		}

		private void BuildImportFolderList()
		{
			string importPath = DataUtil.Trim(tbImportFrom.Text);
			if (File.Exists(importPath))
			{
				ImportFolderList.Items.Clear();
				switch (Path.GetExtension(importPath).ToLowerInvariant())
				{
				case ".esf":
					BuildImportFolderList_Database(importPath);
					break;
				case ".est":
					BuildImportFolderList_ESTextFile(importPath);
					break;
				case ".esn":
					BuildImportFolderList_ESTextFile(importPath);
					break;
				case ".xml":
				case ".json":
					BuildExchangeFolderList(importPath);
					break;
				}
			}
		}

		private void BuildImportFolderList_Database(string ImportFileName)
		{
			ImportFolderList.Items.Clear();

			using DbConnection connection = DbController.GetDbConnection(DbController.ConnectionStringForFile(ImportFileName, readOnly: true));

			for (int i = 0; i < Gf.MAXSONGSFOLDERS; i++)
			{
				EsfFolderNames[i] = "";
			}

			bool hasUserReferenceColumn = DbController.GetColumnNames(connection, "SONG")
				.Exists(name => name.Equals("USER_REFERENCE", StringComparison.OrdinalIgnoreCase));
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
				if (hasUserReferenceColumn)
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
				MessageBox.Show(UiText.Get(nameof(FrmImport), "ReadError"));
				return;
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
				if (TextImportFormat != "[est3.1]")
				{
					MessageBox.Show(UiText.Get(nameof(FrmImport), "ReadError"));
					return;
				}
				esfSongFolder = ">f";
				esfImportFieldSeparator = '>'.ToString();
				esfImportFormatTitle = "[>";
			}
			InString = DataUtil.Right(InString, InString.Length - 8);
			InString = InString.Replace(esfImportFormatTitle, Convert.ToString('\u0001'));
			string[] songBlocks = InString.Split('\u0001');
			ListViewItem listViewItem = new();
			if (songBlocks == null || songBlocks.GetUpperBound(0) < 0)
			{
				return;
			}
			int folderTagPos = -1;
			int separatorPos = -1;
			int headerEnd = -1;
			string header = "";
			string folderName = "";
			bool alreadyListed = false;
			for (int i = 0; i <= songBlocks.GetUpperBound(0); i++)
			{
				headerEnd = songBlocks[i].IndexOf(']');
				header = DataUtil.Left(songBlocks[i], (headerEnd > 0) ? headerEnd : 0);
				if (header.Length <= 0)
				{
					continue;
				}
				folderTagPos = header.IndexOf(esfSongFolder);
				if ((folderTagPos >= 0) && (header.Length > folderTagPos))
				{
					separatorPos = header.IndexOf(esfImportFieldSeparator, folderTagPos + 1);
					folderName = ((separatorPos <= 0) ? DataUtil.Mid(header, folderTagPos + esfSongFolder.Length) : DataUtil.Mid(header, folderTagPos + esfSongFolder.Length, separatorPos - (folderTagPos + esfSongFolder.Length)));
				}
				else
				{
					folderName = UiText.Get(nameof(FrmImport), "DefaultFolder");
				}
				FolderLookupSongsCount++;
				alreadyListed = false;
				for (int j = 0; j < ImportFolderList.Items.Count; j++)
				{
					if (ImportFolderList.Items[j].ToString() == folderName)
					{
						alreadyListed = true;
					}
				}
				if (!alreadyListed)
				{
					ImportFolderList.Items.Add(folderName);
				}
			}
		}

		private void BuildExchangeFolderList(string path)
		{
			ImportFolderList.Items.Clear();
			FolderLookupSongsCount = 0;
			try
			{
				var folders = SongExchangeFile.CountFolders(path, StringComparer.Ordinal);
				FolderLookupSongsCount = folders.Values.Sum();
				ImportFolderList.Items.AddRange(folders.Keys.ToArray());
			}
			catch (Exception ex) when (SongExchangeFile.IsReadError(ex))
			{
				MessageBox.Show(UiText.Format(nameof(FrmImport), "SongFileReadFailed", ex.Message));
			}
		}

		private HashSet<string> GetSelectedImportFolders() =>
			ImportFolderList.CheckedItems.Cast<string>().ToHashSet(StringComparer.Ordinal);

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			OpenFileDialog1.Filter = UiText.Format(nameof(FrmImport), "ImportFilter", "*.esf,*.est,*.esn,*.xml,*.json", "*.esf;*.est;*.esn;*.xml;*.json", "*.mdb");
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
			string importPath = DataUtil.Trim(tbImportFrom.Text);
			if (importPath == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "SpecifyImportFile"));
			}
			else if (!File.Exists(importPath))
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "ImportFileMissing"));
			}
			else if ((ImportFolderList.Items.Count > 0) && (ImportFolderList.CheckedItems.Count < 1))
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "SelectImportFolders"));
			}
			else if (SongFolder.SelectedItems.Count < 1)
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "SelectDestinationFolder"));
			}
			else
			{
				string strExt = Path.GetExtension(importPath).ToLower();
				switch (strExt)
				{
					case ".esf":
						Import_DatabaseFormat(importPath);
						break;
					case ".est":
						Import_TextFormat(importPath);
						break;
					case ".esn":
						Import_TextFormat(importPath);
						break;
					case ".xml":
					case ".json":
						ImportExchangeFile(importPath);
						break;
					case ".mdb":
						AccessHelper(importPath);
						break;
					default:
						MessageBox.Show(UiText.Get(nameof(FrmImport), "InvalidExtension"));
						break;
				}
			}
		}

		private void Import_DatabaseFormat(string ImportFileName)
		{
			int rowNo = 0;
			ListViewItem listViewItem = new();
			string folderFilter = "";
			SongsUpdated = 0;
			SongsNew = 0;
			Cursor = Cursors.WaitCursor;
			SongsList.Items.Clear();
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			if (ImportFolderList.CheckedItems.Count > 0)
			{
				var folders = new System.Collections.Generic.List<int>();
				foreach (object item in ImportFolderList.CheckedItems)
					folders.Add(GetImportFolderNumber(item.ToString()));
				folderFilter = $" where FolderNo in ({string.Join(",", folders)})";
			}
			int totalRows = 0;

			using DataTable dataTable = DbController.GetDataTable(DbController.ConnectionStringForFile(ImportFileName, readOnly: true), "select * from SONG " + folderFilter);

			totalRows = dataTable.Rows.Count;

			if (totalRows > 0)
			{
				dataTable.BeginInit();

				listViewItem = SongsList.Items.Add(UiText.Get(nameof(FrmImport), "Importing"));
				int progressPercent = 0;
				InFolderNo = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

				//DataTable dt = DbController.GetDataTable(connection, fullSearchString);

				// GetDbConnection opens a new connection for each call, so this method owns it.
				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
				//Database daoDb = DbDaoController.GetDaoDb(Gf.ConnectStringMainDB);

				foreach (DataRow row in dataTable.Rows)
				{
					bool songRequired = false;
					int? existingId = null;
					rowNo++;
					progressPercent = rowNo * 100 / totalRows;
					ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
					Invalidate();
					ProgressBar1.Invalidate();
					if (Gf.LoadDataIntoItem(ref ImportItem, row))
					{
						listViewItem = SongsList.Items.Add(Convert.ToString(rowNo));
						listViewItem.SubItems.Add(ImportItem.Title);

						songRequired = true;
						ImportItem.FolderNo = InFolderNo;
						existingId = SongQueries.FindImportSongId(connection, InFolderNo, ImportItem.Title);
					}
					if (existingId.HasValue)
					{
						CurSongID = existingId.Value;
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

		internal static IEnumerable<(ExchangeSong Song, string Sequence)> ReadSelectedExchangeSongs(
			FileStream input, bool json, HashSet<string> selectedFolders)
		{
			foreach (var song in SongExchangeFile.ReadSongs(input, json))
			{
				if (selectedFolders.Contains(song.SourceFolder))
					yield return (song, json ? SongExchangeSequence.ToLegacy(song) : Gf.ConvertTextStringToSequence(song.Sequence));
			}
		}

		private void ImportExchangeFile(string path)
		{
			SongsUpdated = 0;
			SongsNew = 0;
			SongsList.Items.Clear();
			try
			{
				Cursor = Cursors.WaitCursor;
				bool isJson = SongExchangeFile.IsJson(path);
				using var input = File.OpenRead(path);
				var songs = ReadSelectedExchangeSongs(input, isJson, GetSelectedImportFolders());
				// Validate the entire file before the first database write. Rewind the same read-locked file.
				int total = songs.Count();
				input.Position = 0;
				int destination = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
				using var connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
				ProgressBar1.Value = 0;
				ProgressBar1.Visible = true;
				int processed = 0;
				foreach (var (song, sequence) in songs)
				{
					processed++;
					var row = SongsList.Items.Add(processed.ToString());
					row.SubItems.Add(song.Title1);
					ProgressBar1.Value = (int)(processed * 100L / total);
					if (string.IsNullOrWhiteSpace(song.Title1))
					{
						row.SubItems.Add(UiText.Get(nameof(FrmImport), "NoTitle"));
						continue;
					}
					Gf.ApplyExchangeSong(song, ref ImportItem, sequence);
					ImportItem.FolderNo = destination;
					if (!isJson)
					{
						Gf.LoadLicAdminDisplayInfo(ref ImportItem.Show_LicAdminInfo1, ref ImportItem.Show_LicAdminInfo2);
						ImportItem.CompleteLyrics = ImportItem.CompleteLyrics.TrimStart('\n', '\r').Replace("\r\n", "\n");
					}
					int? existingId = SongQueries.FindImportSongId(connection, destination, song.Title1);
					CurSongID = OptImport1.Checked ? -1 : existingId ?? -1;
					SaveSong(!existingId.HasValue || !OptImport0.Checked, CurSongID, ImportItem, ref row, ref SongsNew, ref SongsUpdated);
				}
				Show_Import_Result();
			}
			catch (Exception ex) when (SongExchangeFile.IsReadError(ex))
			{
				MessageBox.Show(UiText.Format(nameof(FrmImport), "SongFileImportFailed", ex.Message));
			}
			finally
			{
				Cursor = Cursors.Default;
				ProgressBar1.Visible = false;
			}
		}

		private void Show_Import_Result()
		{
			ListViewItem listViewItem = new();
			string noImportMessage = "";
			string newSongsMessage = "";
			string replacedMessage = "";
			if ((SongsNew == 0) && (SongsUpdated == 0))
			{
				noImportMessage = UiText.Get(nameof(FrmImport), "NoSongsImported");
			}
			else
			{
				if (SongsNew >= 1)
				{
					newSongsMessage = ((SongsNew != 1) ? UiText.Format(nameof(FrmImport), "ImportedNewSongs", SongsNew) : UiText.Get(nameof(FrmImport), "ImportedOneNewSong"));
				}
				if (SongsUpdated >= 1)
				{
					replacedMessage = ((SongsUpdated != 1) ? UiText.Format(nameof(FrmImport), "ReplacedSongs", SongsUpdated) : UiText.Get(nameof(FrmImport), "ReplacedOneSong"));
				}
			}
			string summary = string.Join(" ", new[] { noImportMessage, newSongsMessage, replacedMessage }.Where(message => message != ""));
			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add(UiText.Get(nameof(FrmImport), "Completed"));
			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add(summary);
			SongsList.Items.Add("");
			SongsList.Items[SongsList.Items.Count - 1].EnsureVisible();
			SongsList.Update();
			MessageBox.Show(UiText.Format(nameof(FrmImport), "CompletedSummary", summary));
			ProgressBar1.Visible = false;
			string strExt = Path.GetExtension(tbImportFrom.Text).ToLower();
			if (strExt == ".mdb")
			{
				tbImportFrom.Text = "";
			}
		}

		private int GetImportFolderNumber(string InFolderName)
		{
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (EsfFolderNames[i] == InFolderName)
				{
					return i;
				}
			}
			return 0;
		}

		private static void SaveSong(bool SongRequired, int InSongID, SongSettings InItem, ref ListViewItem listItem, ref int SongsNew, ref int SongsUpdated)
		{
			if (SongRequired)
			{
				if (InSongID < 1)
				{
					if (Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, InItem) <= 0)
					{
						listItem.SubItems.Add(UiText.Get(nameof(FrmImport), "InsertFailed"));
						return;
					}
					listItem.SubItems.Add(UiText.Get(nameof(FrmImport), "ImportedAsNew"));
					SongsNew++;
				}
				else
				{
					if (!Gf.UpdateDatabaseItem(Gf.ConnectStringMainDB, InItem, InSongID))
					{
						listItem.SubItems.Add(UiText.Get(nameof(FrmImport), "UpdateFailed"));
						return;
					}
					listItem.SubItems.Add(UiText.Get(nameof(FrmImport), "ExistingReplaced"));
					SongsUpdated++;
				}
			}
			else
			{
				listItem.SubItems.Add(UiText.Get(nameof(FrmImport), "NotImported"));
			}
		}

		private void Import_TextFormat(string ImportFileName)
		{
			string InString = "";
			if (!gfFileHelpers.LoadFileContents(ImportFileName, ref InString))
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "ReadError"));
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
				if (TextImportFormat != "[est3.1]")
				{
					MessageBox.Show(UiText.Get(nameof(FrmImport), "ReadError"));
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
			int progressPercent = 0;
			ListViewItem listViewItem = new();
			string[] songBlocks = InString.Split('\u0001');
			var selectedFolders = GetSelectedImportFolders();

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

			ProgressBar1.Visible = true;
			listViewItem = SongsList.Items.Add("");
			listViewItem.SubItems.Add(UiText.Get(nameof(FrmImport), "StartingImport"));
			if (songBlocks != null && songBlocks.GetUpperBound(0) >= 0)
			{
				int itemNo = 0;
				int folderTagPos = -1;
				int separatorPos = -1;
				int headerEnd = -1;
				int matchedCount = 0;
				string header = "";
				string folderName = "";
				string lyrics = "";
				SongsUpdated = 0;
				SongsNew = 0;
				for (int i = 0; i <= songBlocks.GetUpperBound(0); i++)
				{
					headerEnd = songBlocks[i].IndexOf(']');
					header = DataUtil.Left(songBlocks[i], (headerEnd > 0) ? headerEnd : 0);
					if (FolderLookupSongsCount < 1)
					{
						FolderLookupSongsCount = 1;
					}
					if (header.Length > 0)
					{
						folderTagPos = header.IndexOf(esfSongFolder);
						if ((folderTagPos >= 0) && (header.Length > folderTagPos))
						{
							separatorPos = header.IndexOf(esfImportFieldSeparator, folderTagPos + 1);
							folderName = ((separatorPos <= 0) ? DataUtil.Mid(header, folderTagPos + esfSongFolder.Length) : DataUtil.Mid(header, folderTagPos + esfSongFolder.Length, separatorPos - (folderTagPos + esfSongFolder.Length)));
						}
						else
						{
							folderName = UiText.Get(nameof(FrmImport), "DefaultFolder");
						}
						itemNo++;
						progressPercent = itemNo * 100 / FolderLookupSongsCount;
						ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
						Invalidate();
						ProgressBar1.Invalidate();
						if (selectedFolders.Contains(folderName))
						{
							Gf.InitialiseIndividualData(ref ImportItem);
							LoadTextFileHeaderToItem(ref ImportItem, header);
							lyrics = DataUtil.Mid(songBlocks[i], (headerEnd + 1 < songBlocks[i].Length) ? (headerEnd + 1) : 0);
							if (lyrics.Contains("[~", StringComparison.CurrentCulture))
							{
								int notationsStart = lyrics.IndexOf("[~") + "[~".Length;
								int notationsEnd = lyrics.IndexOf(']', notationsStart);
								if (notationsEnd > notationsStart)
								{
									ImportItem.Notations = DataUtil.Mid(lyrics, notationsStart, notationsEnd - notationsStart);
									lyrics = DataUtil.Mid(lyrics, notationsEnd + 3);
								}
								else
								{
									lyrics = "";
								}
							}
							lyrics = lyrics.TrimStart('\n', '\r');
							matchedCount++;
							listViewItem = SongsList.Items.Add(matchedCount.ToString());
							listViewItem.SubItems.Add(ImportItem.Title);

							bool songRequired = true;

							int? existingId = SongQueries.FindImportSongId(connection, folderNumber, ImportItem.Title);
							if (existingId.HasValue)
							{
								CurSongID = existingId.Value;
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

							if (songRequired)
							{
								if (ImportItem.Title != "")
								{
									ImportItem.CompleteLyrics = lyrics.Replace("\r\n", "\n");
									ImportItem.FolderNo = folderNumber;
									SaveSong(songRequired, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
								}
								else
								{
									listViewItem.SubItems.Add(UiText.Get(nameof(FrmImport), "NoTitle"));
								}
							}
							else
							{
								listViewItem.SubItems.Add(UiText.Get(nameof(FrmImport), "SongExists"));
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
			int separatorPos = InString.IndexOf(esfImportFieldSeparator);
			string fieldValue = "";
			string fieldTag = "";
			if (separatorPos <= 0)
			{
				return;
			}
			InItem.Title = DataUtil.Left(InString, separatorPos);
			InString = DataUtil.Mid(InString, separatorPos + 1);
			while (InString.Length > 0)
			{
				separatorPos = InString.IndexOf(esfImportFieldSeparator, 1);
				if (separatorPos > 0)
				{
					fieldValue = DataUtil.Left(InString, separatorPos);
					InString = DataUtil.Mid(InString, separatorPos + 1);
				}
				else
				{
					fieldValue = InString;
					InString = "";
				}
				if (fieldValue.Length > 2)
				{
					fieldTag = esfImportFieldSeparator + fieldValue[0];
					fieldValue = DataUtil.Mid(fieldValue, 1);
				}
				else
				{
					fieldTag = "";
					fieldValue = "";
				}
				if (fieldTag != "")
				{
					if (fieldTag == esfSongTitle2)
					{
						InItem.Title2 = fieldValue;
					}
					else if (fieldTag == esfSongFolder)
					{
						InItem.FolderName = fieldValue;
					}
					else if (fieldTag == esfSongNumber)
					{
						InItem.SongNumber = DataUtil.StringToInt(fieldValue);
					}
					else if (fieldTag == esfBookReference)
					{
						InItem.Book_Reference = fieldValue;
					}
					else if (fieldTag == esfUserReference)
					{
						InItem.User_Reference = fieldValue;
					}
					else if (fieldTag == esfSongCopyright)
					{
						InItem.Copyright = fieldValue;
					}
					else if (fieldTag == esfSongWriterInfo)
					{
						InItem.Writer = fieldValue;
					}
					else if (fieldTag == esfSongKey)
					{
						InItem.MusicKey = fieldValue;
					}
					else if (fieldTag == esfSongCapo)
					{
						InItem.Capo = DataUtil.StringToInt(fieldValue);
					}
					else if (fieldTag == esfSongTiming)
					{
						InItem.Timing = fieldValue;
					}
					else if (fieldTag == esfSongAdmin1)
					{
						InItem.Show_LicAdminInfo1 = fieldValue;
					}
					else if (fieldTag == esfSongAdmin2)
					{
						InItem.Show_LicAdminInfo2 = fieldValue;
					}
					else if (fieldTag == esfSequence)
					{
						InItem.SongSequence = Gf.ConvertTextStringToSequence(fieldValue);
					}
					else if (fieldTag == esfSongFormat)
					{
						InItem.Format.FormatString = fieldValue;
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
				FrmImportAccessHelper frmImportAccessHelper = new();
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
			int rowNo = 0;
			ListViewItem listViewItem = new();
			bool songRequired = false;
			SongsUpdated = 0;
			SongsNew = 0;
			Cursor = Cursors.WaitCursor;
			SongsList.Items.Clear();
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			int totalRows = 0;

			using DataTable dataTable = DbController.GetDataTable(DbController.ConnectionStringForFile(ImportFileName, readOnly: true), "select * from " + DbController.QuoteIdentifier(Gf.Import_TableName));
			if (dataTable.Rows.Count <= 0)
			{
				return;
			}

			totalRows = dataTable.Rows.Count;

			listViewItem = SongsList.Items.Add(UiText.Get(nameof(FrmImport), "Importing"));
			int progressPercent = 0;
			int folderNumber = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);

			using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

			foreach (DataRow dr in dataTable.Rows)
			{
				songRequired = false;
				int? existingId = null;
				rowNo++;
				progressPercent = rowNo * 100 / totalRows;
				ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);

				Invalidate();

				ProgressBar1.Invalidate();

				if (LoadExternalAccessDatabaseToItem(ref ImportItem, dr))
				{
					listViewItem = SongsList.Items.Add(Convert.ToString(rowNo));
					listViewItem.SubItems.Add(ImportItem.Title);

					songRequired = true;
					ImportItem.FolderNo = folderNumber;
					existingId = SongQueries.FindImportSongId(connection, folderNumber, ImportItem.Title);
				}
				else
				{
					// The row could not be read (e.g. a mapped column is missing): skip it.
					continue;
				}

				if (existingId.HasValue)
				{
					CurSongID = existingId.Value;
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

		private bool LoadExternalAccessDatabaseToItem(ref SongSettings InItem, DataRow row)
		{
			try
			{
				InItem.Title = DataUtil.GetDataString(row, Gf.Import_SongTitleColumnName);
				InItem.Title2 = DataUtil.GetDataString(row, Gf.Import_SongTitle2ColumnName);
				InItem.SongNumber = DataUtil.GetDataInt(row, Gf.Import_SongNumberColumnName);
				InItem.CompleteLyrics = GetMergedSongLyrics(row);
				InItem.Copyright = DataUtil.GetDataString(row, Gf.Import_SongCopyrightColumnName);
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(row, Gf.Import_Admin1ColumnName);
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(row, Gf.Import_Admin2ColumnName);
				InItem.Notations = DataUtil.GetDataString(row, "msc");
				InItem.Capo = DataUtil.GetDataInt(row, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(row, "Sequence");
				InItem.Writer = DataUtil.GetDataString(row, Gf.Import_SongWriterInfoColumnName);
				InItem.Book_Reference = DataUtil.GetDataString(row, Gf.Import_BookReferenceColumnName);
				InItem.User_Reference = DataUtil.GetDataString(row, Gf.Import_UserReferenceColumnName);
				InItem.Timing = DataUtil.GetDataString(row, Gf.Import_SongTimingColumnName);
				InItem.MusicKey = DataUtil.GetDataString(row, Gf.Import_SongKeyColumnName);
				InItem.Format.FormatString = "";
				return true;
			}
			catch
			{
				return false;
			}
		}

		private string GetMergedSongLyrics(DataRow row)
		{
			string columnText = "";
			string mergedLyrics = "";
			if (sArray != null)
			{
				for (int i = 0; i <= sArray.GetUpperBound(0); i++)
				{
					columnText = "";
					columnText = DataUtil.TrimEnd(Convert.ToString(DataUtil.GetDataString(row, sArray[i])));
					if (columnText != "")
					{
						mergedLyrics = mergedLyrics + columnText + "\n\n";
					}
				}
			}
			return DataUtil.TrimEnd(mergedLyrics);
		}

		private void BtnImportHelper_Click(object sender, EventArgs e)
		{
			if (DataUtil.Trim(tbImportFrom.Text) == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "HelperNoFile"));
				return;
			}
			if (SongFolder.SelectedItems.Count < 1)
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "HelperNoFolder"));
				return;
			}
			string importPath = DataUtil.Trim(tbImportFrom.Text);
			if (!File.Exists(importPath))
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "HelperFileMissing"));
			}
			else if (string.Equals(Path.GetExtension(importPath), ".mdb", StringComparison.OrdinalIgnoreCase))
			{
				Gf.Import_AccessFileName = importPath;
				FrmImportAccessHelper frmImportAccessHelper = new();
				if (frmImportAccessHelper.ShowDialog() == DialogResult.OK)
				{
					Import_AccessDatabase(importPath, UsedHelper: true);
					Update();
				}
			}
			else
			{
				MessageBox.Show(UiText.Get(nameof(FrmImport), "HelperNotAccess"));
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
				MessageBox.Show(UiText.Get(nameof(FrmImport), "SelectSourceCdFolder"));
				return;
			}
			string sourceFolder = "D:\\Source CD\\hymns";
			if (!Directory.Exists(sourceFolder))
			{
				MessageBox.Show(UiText.Format(nameof(FrmImport), "SourceCdFolderMissing", sourceFolder));
				return;
			}
			var songTitles = new Dictionary<int, string>();
			if (ReadSourceCdTitles(@"D:\Source CD\hymnindex.htm", songTitles))
			{

				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
				try
				{
					ListViewItem listViewItem = new();
					bool songRequired = false;
					listViewItem = SongsList.Items.Add(UiText.Get(nameof(FrmImport), "Importing"));
					int fileNo = 0;
					SongsNew = 0;
					SongsUpdated = 0;
					string[] files = Directory.GetFiles(sourceFolder, "*.html");
					int fileCount = files.GetUpperBound(0) + 1;
					ProgressBar1.Visible = true;
					ProgressBar1.Value = 0;
					string remainingRefs = "";
					int progressPercent = 0;
					int folderNumber = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					foreach (string importFileName in files)
					{
						fileNo++;
						progressPercent = fileNo * 100 / fileCount;
						ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
						Invalidate();
						ProgressBar1.Invalidate();
						ExtractOneSourceCDHTMLIntoItem(ref ImportItem, importFileName, songTitles);
						listViewItem = SongsList.Items.Add(fileNo.ToString());
						listViewItem.SubItems.Add(ImportItem.Title);

						songRequired = true;

						int? existingId = SongQueries.FindImportSongId(connection, folderNumber, ImportItem.Title);
						if (existingId.HasValue)
						{
							//recordset.MoveFirst();
							CurSongID = existingId.Value;
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

						if (songRequired)
						{
							if (ImportItem.Title != "")
							{
								ImportItem.FolderNo = folderNumber;
								using DataTable bookRefTable = DbController.GetDataTable(connection,
									"select book_reference from SONG where Folderno > 0 and upper(book_reference) like @reference",
									("@reference", "%" + ImportItem.Book_Reference + "%"));
								if (bookRefTable.Rows.Count > 0)
								{
									bool bookRefFound = false;
									string bookReference = "";

									foreach (DataRow dr in bookRefTable.Rows)
									{
										if (bookRefFound) break;

										bookReference = DataUtil.GetDataString(dr, "book_reference");
										remainingRefs = bookReference;
										while (remainingRefs.Length > 0 && !bookRefFound)
										{
											if (DataUtil.ExtractOneInfo(ref remainingRefs, ',').ToUpper().TrimStart(' ') == ImportItem.Book_Reference)
											{
												ImportItem.Book_Reference = bookReference;
												bookRefFound = true;
											}
										}
									}
								}
								SaveSong(songRequired, CurSongID, ImportItem, ref listViewItem, ref SongsNew, ref SongsUpdated);
							}
							else
							{
								listViewItem.SubItems.Add(UiText.Get(nameof(FrmImport), "NoTitle"));
							}
						}
						else
						{
							listViewItem.SubItems.Add(UiText.Get(nameof(FrmImport), "SongExists"));
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

        internal static bool ReadSourceCdTitles(string indexFile, Dictionary<int, string> songTitles)
        {
            if (!File.Exists(indexFile)) return false;
            string html = gfFileHelpers.LoadTextFile(indexFile);
            var remaining = html.AsSpan();
            foreach (var range in remaining.Split("<tr>"))
            {
                if (range.Start.Value == 0) continue;
                var row = remaining[range];
                if (row.Contains("<em>", StringComparison.OrdinalIgnoreCase)) continue;
                var title = NextCellText(ref row);
                var number = NextCellText(ref row);
                if (!title.IsEmpty && int.TryParse(number, out int id) && id > 0)
                    songTitles[id] = title.ToString().Replace(",", "").Replace('\n', ' ');
            }
            return songTitles.Count > 0;
        }

        private static ReadOnlySpan<char> NextCellText(ref ReadOnlySpan<char> row)
        {
            while (!row.IsEmpty)
            {
                int start = row.IndexOf('>');
                if (start < 0) break;
                row = row[(start + 1)..];
                int end = row.IndexOf('<');
                if (end < 0) break;
                var value = row[..end].Trim();
                row = row[end..];
                if (!value.IsEmpty) return value;
            }
            return [];
        }

		private static void ExtractOneSourceCDHTMLIntoItem(ref SongSettings InItem, string ImportFileName, Dictionary<int, string> songTitles)
		{
			if (!File.Exists(ImportFileName))
			{
				return;
			}
			string html = gfFileHelpers.LoadTextFile(ImportFileName);
			int songNo = DataUtil.StringToInt(Gf.GetDisplayNameOnly(ref ImportFileName, UpdateByRef: false, KeepExt: false));
			html = html.Replace("InstanceBeginEditable name=", Convert.ToString('\u0001'));
			string[] sections = html.Split('\u0001');
			string[] sectionMarkers = new string[4];
			string commentStart = "<!--";
			sectionMarkers[0] = "\"content\" -->";
			sectionMarkers[1] = "\"Author\" -->";
			sectionMarkers[2] = "\"copyright\" -->";
			string[] sectionContents =
        [
            "",
				"",
				"",
				null
			];
			int markerPos = -1;
			int sectionEnd = -1;
			if (sections != null && sections.GetUpperBound(0) >= 0)
			{
				for (int i = 0; i <= sections.GetUpperBound(0); i++)
				{
					for (int j = 0; j < 3; j++)
					{
						markerPos = sections[i].IndexOf(sectionMarkers[j]);
						if (markerPos >= 0)
						{
							sectionEnd = sections[i].IndexOf(commentStart, markerPos + sectionMarkers[j].Length);
							sectionEnd = ((sectionEnd < markerPos + sectionMarkers[j].Length) ? sections[i].Length : sectionEnd);
							sectionContents[j] = DataUtil.Mid(sections[i], markerPos + sectionMarkers[j].Length, sectionEnd - (markerPos + sectionMarkers[j].Length));
						}
					}
				}
			}
			Gf.InitialiseIndividualData(ref InItem);
			if (songNo <= 0)
			{
				return;
			}
			InItem.Title = songTitles.TryGetValue(songNo, out var title) ? title : "";
			if (InItem.Title != "")
			{
				InItem.Book_Reference = "TS" + songNo;
				InItem.SongSequence = ConvertHTMLLines(ref sectionContents[0]);
				InItem.CompleteLyrics = sectionContents[0];
				InItem.Writer = RemoveHTMLTags(sectionContents[1].Trim());
				InItem.Copyright = RemoveHTMLTags(sectionContents[2].Trim());
				if (InItem.Copyright.ToLower().Contains("public domain", StringComparison.CurrentCulture) || InItem.Copyright.Length == 0)
				{
					InItem.Show_LicAdminInfo1 = "Public Domain";
				}
				else
				{
					InItem.Show_LicAdminInfo1 = "CCLI";
				}
			}
		}

		private static string ConvertHTMLLines(ref string InContents)
		{
			InContents = InContents.Replace("\r", "");
			InContents = InContents.Replace("\n", "");
			InContents = InContents.Replace("<p>", "<br><br>");
			InContents = InContents.Replace("<br>", Convert.ToString('\u0001'));
			StringBuilder stringBuilder = new();
			string[] lines = InContents.Split('\u0001');
			bool chorusMarked = false;
			if (lines != null && lines.GetUpperBound(0) >= 0)
			{
				bool joinWithNext = false;
				for (int i = 0; i <= lines.GetUpperBound(0); i++)
				{
					if (lines[i].Contains("<em>", StringComparison.CurrentCulture) && !chorusMarked)
					{
						chorusMarked = true;
						stringBuilder.Append(Gf.VerseSymbol[0] + "\n");
					}
					lines[i] = RemoveHTMLTags(lines[i]);
					joinWithNext = i < lines.GetUpperBound(0) && lines[i + 1].Contains("&nbsp;", StringComparison.CurrentCulture);
					stringBuilder.Append(lines[i] + (joinWithNext ? " " : "\n"));
				}
			}
			InContents = stringBuilder.ToString();
			return InsertVertIndicators(ref InContents);
		}

		private static string InsertVertIndicators(ref string InString)
		{
			string contents = InString;
			string sequence = "";
			contents = contents.TrimStart('\n', '\r');
			contents = contents.TrimEnd('\n', '\r');
			contents = contents.Replace("\n\n", Convert.ToString('\u0001'));
			string[] paragraphs = contents.Split('\u0001');
			if (paragraphs != null && paragraphs.GetUpperBound(0) >= 0)
			{
				int lastIndex = (paragraphs[paragraphs.GetUpperBound(0)] != "") ? paragraphs.GetUpperBound(0) : (paragraphs.GetUpperBound(0) - 1);
				if (lastIndex > 0)
				{
					if (paragraphs[0].Contains(Gf.VerseSymbol[0], StringComparison.CurrentCulture))
					{
						sequence += '\0';
						for (int i = 1; i <= lastIndex; i++)
						{
							sequence += (char)i;
							sequence += '\0';
							paragraphs[i] = Gf.VerseSymbol[i] + "\n" + paragraphs[i].TrimStart('\n', '\r');
						}
						if (lastIndex < 2)
						{
							sequence = "";
						}
					}
					else if (paragraphs[1].Contains(Gf.VerseSymbol[0], StringComparison.CurrentCulture) && lastIndex > 1)
					{
						sequence += '\u0001';
						sequence += '\0';
						paragraphs[0] = Gf.VerseSymbol[1] + "\n" + paragraphs[0].TrimStart('\n', '\r');
						for (int i = 2; i <= lastIndex; i++)
						{
							sequence += (char)i;
							sequence += '\0';
							paragraphs[i] = Gf.VerseSymbol[i] + "\n" + paragraphs[i].TrimStart('\n', '\r');
						}
					}
					else if (contents.IndexOf(Gf.VerseSymbol[0]) < 0)
					{
						for (int i = 0; i <= lastIndex; i++)
						{
							paragraphs[i] = Gf.VerseSymbol[i + 1] + "\n" + paragraphs[i].TrimStart('\n', '\r');
						}
					}
					InString = "";
					for (int i = 0; i <= lastIndex; i++)
					{
						InString = InString + paragraphs[i].TrimEnd('\n', '\r') + ((i < lastIndex) ? "\n" : "");
					}
					return sequence;
				}
				InString = paragraphs[0];
				return "";
			}
			return "";
		}

		internal static string RemoveHTMLTags(string InContents)
		{
			if (InContents.Length == 0)
			{
				return "";
			}
			InContents = InContents.Replace("&nbsp;", "");
			var plainText = new StringBuilder(InContents.Length);
			bool insideTag = false;
			bool previousWasSpace = false;
			for (int i = 0; i < InContents.Length; i++)
			{
				if (InContents[i] == '<')
				{
					insideTag = true;
				}
				if (!insideTag && (InContents[i] != ' ' || !previousWasSpace))
				{
					plainText.Append(InContents[i]);
				}
				if (InContents[i] == '>')
				{
					insideTag = false;
				}
				previousWasSpace = InContents[i] == ' ';
			}
			return plainText.ToString().Trim();
		}

	}
