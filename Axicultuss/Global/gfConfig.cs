using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Axicultus.Global;
using Axicultus.Localization;
using Axicultus.Module;
using Axicultus.SQLite;
using Axicultus.Util;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbConnection = System.Data.SQLite.SQLiteConnection;

namespace Axicultus;

	internal unsafe partial class Gf
	{
		// Field definition structure for database schema validation
		private struct FieldDefinition(string name, int type, int size, string tableName)
    {
			public string Name = name;
			public int Type = type;
			public int Size = size;
			public string TableName = tableName;
    }

    /// <summary>
    /// Checks and updates version information in registry
    /// </summary>
    private static void CheckAndUpdateVersion()
		{
			string version = RegUtil.GetRegValue("config", "version", "none");
			if (version != Axicultus_Version)
			{
				RegUtil.SaveRegValue("config", "version", Axicultus_Version);
			}
		}

		/// <summary>
		/// Checks and updates database version information in registry
		/// </summary>
		private static void CheckAndUpdateDatabaseVersion()
		{
			string databaseVersion = RegUtil.GetRegValue("config", "database", "none");
			if (databaseVersion != Database_Version)
			{
				ApplicationFirstRun = true;
				RegUtil.SaveRegValue("config", "database", Database_Version);
			}
		}

		/// <summary>
		/// Prompts user to restore database on first run
		/// </summary>
		private static bool PromptRestoreDatabaseOnFirstRun()
		{
			SplashScreenBack = true;
			bool result = MessageBox.Show(
				UiText.Get("gfConfig", "ReplaceDatabase"),
				UiText.Format("gfConfig", "ReplaceDatabaseCaption", Axicultus_Version),
				MessageBoxButtons.YesNo) == DialogResult.Yes;
			SplashScreenBack = false;
			return result;
		}

		/// <summary>
		/// Prompts user to restore database when database file is missing
		/// </summary>
		private static bool PromptRestoreDatabaseOnMissing()
		{
			SplashScreenBack = true;
			bool result = MessageBox.Show(
				UiText.Get("gfConfig", "InstallDatabase"),
				UiText.Format("gfConfig", "InstallDatabaseCaption", Axicultus_Version),
				MessageBoxButtons.YesNo) == DialogResult.Yes;
			SplashScreenBack = false;
			return result;
		}

		/// <summary>
		/// Handles database restoration logic
		/// </summary>
		private static void HandleDatabaseRestoration(bool databaseExists)
		{
			if (ApplicationFirstRun || !databaseExists)
			{
				CovertItemsTov4();
				
				if (!RestoreSongsDatabase && ApplicationFirstRun)
				{
					RestoreSongsDatabase = PromptRestoreDatabaseOnFirstRun();
				}
				else if (!RestoreSongsDatabase && !databaseExists)
				{
					RestoreSongsDatabase = PromptRestoreDatabaseOnMissing();
				}
			}

			if (RestoreSongsDatabase)
			{
				string backupFileName = RestoreOriginalSongsDatabase();
				if (backupFileName != "-1")
				{
					string message = UiText.Get("gfConfig", "DatabaseInstalled");
					if (!string.IsNullOrEmpty(backupFileName))
					{
						message = UiText.Format("gfConfig", "DatabaseInstalledRenamed", backupFileName);
					}
					MessageBox.Show(message);
				}
			}
		}

		public static bool InitAxicultusDir()
		{
			RootAxicultusDir = RegUtil.GetRegValue("config", "root_directory", DefaultAxicultusDir);
			if (string.IsNullOrEmpty(RootAxicultusDir))
			{
				RootAxicultusDir = DefaultAxicultusDir;
			}

			CheckAndUpdateVersion();
			CheckAndUpdateDatabaseVersion();

			if (!ValidateRootFolder())
			{
				return false;
			}

			RegUtil.SaveRegValue("config", "root_directory", RootAxicultusDir);
			ValidateID();

			DBFileName = ProductStorage.SongDatabase(RootAxicultusDir);
			bool databaseExists = File.Exists(DBFileName);

			HandleDatabaseRestoration(databaseExists);

			SplashScreenFront = true;
			return true;
		}

		/// <summary>
		/// Initializes verse titles array
		/// </summary>
		private static void InitializeVerseTitles()
		{
			VerseTitle[0] = "chorus";
			for (int i = 1; i <= 99; i++)
			{
				VerseTitle[i] = DataUtil.ObjToString(i);
			}
			VerseTitle[100] = "bridge";
			VerseTitle[103] = VerseTitle[100] + " 2";
			VerseTitle[101] = "ending";
			VerseTitle[102] = VerseTitle[0] + " 2";
			VerseTitle[111] = "prechorus";
			VerseTitle[112] = VerseTitle[111] + " 2";
			VerseTitle[150] = "region 2";
			VerseTitle[151] = "\n";
			VerseTitle[152] = "note";
		}

		/// <summary>
		/// Initializes verse symbols and sequence symbols arrays
		/// </summary>
		private static void InitializeVerseSymbols()
		{
			VerseSymbol[0] = "[" + VerseTitle[0] + "]";
			VerseSymbol[100] = "[" + VerseTitle[100] + "]";
			VerseSymbol[103] = "[" + VerseTitle[103] + "]";
			VerseSymbol[101] = "[" + VerseTitle[101] + "]";
			VerseSymbol[102] = "[" + VerseTitle[102] + "]";
			VerseSymbol[111] = "[" + VerseTitle[111] + "]";
			VerseSymbol[112] = "[" + VerseTitle[112] + "]";
			VerseSymbol[150] = "[" + VerseTitle[150] + "]";
			VerseSymbol[151] = VerseTitle[151];
			VerseSymbol[152] = "[" + VerseTitle[152] + "]";
		}

		/// <summary>
		/// Initializes sequence symbols array
		/// </summary>
		private static void InitializeSequenceSymbols()
		{
			SequenceSymbol[0] = "c";
			SequenceSymbol[100] = "b";
			SequenceSymbol[103] = "w";
			SequenceSymbol[101] = "e";
			SequenceSymbol[102] = "t";
			SequenceSymbol[111] = "p";
			SequenceSymbol[112] = "q";
		}

		/// <summary>
		/// Initializes the verse lookup and the ordered heading list used by the editors.
		/// </summary>
		internal static void InitializeVerseHeadings()
		{
			InitializeVerseTitles();
			InitializeVerseSymbols();
			InitializeSequenceSymbols();
			var symbols = new StringBuilder();
			var numbers = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int i = 1; i <= 99; i++)
			{
				VerseSymbol[i] = "[" + VerseTitle[i] + "]";
				SequenceSymbol[i] = VerseTitle[i];
				AddHeading(i);
			}
			ReadOnlySpan<int> specialHeadings = [0, 102, 100, 103, 111, 112, 101, 150];
			foreach (int verse in specialHeadings)
				AddHeading(verse);

			SymbolsString = symbols.ToString();
			verseNumbersByHeading = numbers;

			void AddHeading(int verse)
			{
				string heading = VerseSymbol[verse];
				if (symbols.Length > 0)
					symbols.Append(',');
				symbols.Append(heading);
				numbers.Add(heading, verse);
			}
		}

		/// <summary>
		/// Initializes database file paths and connection strings
		/// </summary>
		private static void InitializeDatabasePaths()
		{
			UsageFileName = RootAxicultusDir + DefaultDBDir + DefaultUsageFilename.Replace(".mdb", ".db");
			BiblesListFileName = RootAxicultusDir + DefaultDBDir + DefaultBibleDBFilename.Replace(".mdb", ".db");
			tempDBFileName = RootAxicultusDir + DefaultDBDir + "~tempAxicultusDb.db";
			tempUsageFileName = RootAxicultusDir + DefaultDBDir + "~tempEsUsage.db";
			tempBiblesListFileName = RootAxicultusDir + DefaultDBDir + "~tempEsBiblesList.db";
			ConnectStringMainDB = DbController.ConnectionStringForFile(DBFileName);
			ConnectStringUsageDB = DbController.ConnectionStringForFile(UsageFileName);
			ConnectStringBibleDB = DbController.ConnectionStringForFile(BiblesListFileName);
		}

		/// <summary>
		/// Initializes item instances
		/// </summary>
		private static void InitializeItemInstances()
		{
			PreviewItem.Initialise();
			OutputItem.Initialise();
			LiveItem.Initialise();
			LyricsItem.Initialise();
			TempItem1.Initialise();
			EditItem1.Initialise();
			EditItem2.Initialise();
			InfoItem1.Initialise();
			InfoItem2.Initialise();
			OutputItem.OutputStyleScreen = true;
		}

		public static bool InitAppData()
		{
			PerformanceStartTime = DateTime.Now;
			LoadInitData = true;
			LoadEulaText();
			//if (!LoadUnicodeStrokeCount())
			//{
			//	return false;
			//}
			HelpFile_Location = ProductStorage.HelpFileIn(Path.Combine(Application.StartupPath, "Sys"));
			MusicSymLen = MusicSym.Length;
			MaxBackgroundStyleIndex = BackPattern.MaxStyleIndex;
			ShowScreenColour[0] = DefaultBackColour;
			ShowScreenColour[1] = DefaultBackColour;
			ShowScreenStyle = 0;
			ShowFontColour[0] = DefaultBackColour;
			ShowFontColour[1] = DefaultBackColour;

			InitializeVerseHeadings();

			ShowLMargin = Screen.PrimaryScreen.Bounds.Width / 50;
			ShowRMargin = ShowLMargin;
			ShowLyricsWidth = Screen.PrimaryScreen.Bounds.Width - ShowLMargin - ShowRMargin;
			
			InitializeDatabasePaths();
			UserString = DataUtil.Trim(RegUtil.GetRegValue("config", "RegistrationUser", ""));

			if (!ValidateVer_3_4_Fields())
			{
				return false;
			}
			// Song dates written by older versions in a regional format become yyyy-MM-dd (no change when all are).
			NormalizeLastModified(ConnectStringMainDB);

			LoadSavedData();

			DisplayInfo.SizeLaunchDisplay();
			ResetShowRunningSettings();

			AlertsDataFile = RootAxicultusDir + DefaultDBDir + DefaultAlertsFilename;
			ParentalDataFile = RootAxicultusDir + DefaultDBDir + DefaultParentalFilename;
			
			InitializeItemInstances();
			SetListViewColumns(ListViewNotations, 5);
			
			// Each wrapper attaches on its own: one failure (PowerPoint slow to start) must not leave the others
			// unattached. Wrappers that stay unattached attach lazily on the UI thread when first needed.
			_ = Task.Run(() =>
			{
				AttachAtStartup(LivePP, nameof(LivePP));
				AttachAtStartup(PreviewPPT, nameof(PreviewPPT));
				AttachAtStartup(OutputPPT, nameof(OutputPPT));
				AttachAtStartup(ExternalPPT, nameof(ExternalPPT));
			});

			SetPatternPeriod();

			return true;
		}

		private static void AttachAtStartup(OfficeLib.PowerPoint wrapper, string name)
		{
			try
			{
				wrapper.Init();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[PPTInit] Start-up attach of {name} failed: {ex.Message}");
			}
		}

		public static void ValidateID()
		{
			SystemID = RegUtil.GetRegValue("config", "ID", "");
			if (SystemID.Length < 12)
			{
				SystemID = DataUtil.Right(Guid.NewGuid().ToString(), 12);
				RegUtil.SaveRegValue("config", "ID", SystemID);
			}
			else if (SystemID.Length > 12)
			{
				SystemID = DataUtil.Left(SystemID, 12);
				RegUtil.SaveRegValue("config", "ID", SystemID);
			}
		}

		public static bool ValidateRootFolder()
		{
			if (DataUtil.Right(RootAxicultusDir, 1) != "\\")
			{
				RootAxicultusDir += "\\";
			}
			if (Directory.Exists(RootAxicultusDir))
			{
				return true;
			}
			try
			{
				if (ApplicationFirstRun)
				{
					if (FileUtil.MakeDir(RootAxicultusDir))
					{
						RestoreSongsDatabase = true;
						RestoreBackgroundImages();
						return true;
					}
					MessageOverSplashScreen(UiText.Format("gfConfig", "CreateWorkingFolderFailed", RootAxicultusDir));
					return false;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR creating Axicultus working folder: {ex.Message}, {ex.StackTrace}");
			}
			SplashScreenBack = true;
			FrmGetWorkingFolder frmGetWorkingFolder = new();
			if (frmGetWorkingFolder.ShowDialog() == DialogResult.OK)
			{
				RestoreBackgroundImages();
				SplashScreenBack = false;
				return true;
			}
			SplashScreenBack = false;
			return false;
		}

		/// <summary>
		/// Executes an action with a database connection, ensuring proper connection management
		/// </summary>
		private static T ExecuteWithConnection<T>(string connectionString, Func<DbConnection, T> action)
		{
			DbConnection connection = null;
			try
			{
				connection = DbController.GetDbConnection(connectionString);
				if (connection == null)
				{
					return default(T);
				}
				return action(connection);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR in ExecuteWithConnection: {ex.Message}, {ex.StackTrace}");
				return default(T);
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
		}

		/// <summary>
		/// Executes an action with a database connection, ensuring proper connection management (void return)
		/// </summary>
		private static void ExecuteWithConnection(string connectionString, Action<DbConnection> action)
		{
			DbConnection connection = null;
			try
			{
				connection = DbController.GetDbConnection(connectionString);
				if (connection == null)
				{
					return;
				}
				action(connection);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR in ExecuteWithConnection: {ex.Message}, {ex.StackTrace}");
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
		}

		/// <summary>
		/// Gets the column names from a database table schema
		/// </summary>
		private static HashSet<string> GetTableColumns(DbConnection connection, string tableName)
		{
			var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				columns.UnionWith(DbController.GetColumnNames(connection, tableName));
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR getting table columns for {tableName}: {ex.Message}, {ex.StackTrace}");
			}
			return columns;
		}

		/// <summary>
		/// Validates and creates missing fields for a table based on field definitions
		/// </summary>
		private static bool ValidateAndCreateFields(DbConnection connection, string tableName, FieldDefinition[] fieldDefinitions)
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				return false;
			}

			HashSet<string> existingColumns = GetTableColumns(connection, tableName);

			foreach (var fieldDef in fieldDefinitions)
			{
				if (!existingColumns.Contains(fieldDef.Name))
				{
					try
					{
						if (fieldDef.Size > 0)
						{
							DbController.CreateField(ref connection, fieldDef.TableName, fieldDef.Name, fieldDef.Type, fieldDef.Size);
						}
						else
						{
							DbController.CreateField(ref connection, fieldDef.TableName, fieldDef.Name, fieldDef.Type);
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR creating field {fieldDef.Name} in table {tableName}: {ex.Message}, {ex.StackTrace}");
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Validates Folder table fields and creates missing ones
		/// </summary>
		private static bool ValidateFolderTableFields(DbConnection connection)
		{
			try
			{
				FieldDefinition[] folderFields =
            [
                new(DBField_BIU0, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_BIU1, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_ColA, DBFieldType_Text, 0, DBTable_Folder),
					new(DBField_ColB, DBFieldType_Text, 0, DBTable_Folder),
					new(DBField_PreChorusHeading, DBFieldType_Text, DBFieldSize_DefaultText, DBTable_Folder),
					new(DBField_LMargin, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_RMargin, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_BMargin, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_BIUHeading, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_HeadingSize, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_HeadingOption, DBFieldType_Int, 0, DBTable_Folder),
					new(DBField_LineSpacing, DBFieldType_Float, 0, DBTable_Folder),
					new(DBField_LineSpacing2, DBFieldType_Float, 0, DBTable_Folder)
				];

				return ValidateAndCreateFields(connection, DBTable_Folder, folderFields);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR validating Folder table fields: {ex.Message}, {ex.StackTrace}");
				return false;
			}
		}

		/// <summary>
		/// Validates Song table fields and creates missing ones
		/// </summary>
		internal static bool ValidateSongTableFields(DbConnection connection)
		{
			try
			{
				// SQLite stores the full TEXT value irrespective of VARCHAR(n) declarations.
				// Validate and create missing fields
				FieldDefinition[] songFields =
            [
                new(DBField_CAPO, DBFieldType_Int, 0, DBTable_Song),
					new(DBField_TIMING, DBFieldType_Text, 0, DBTable_Song),
					new("MSC", DBFieldType_TextUnlimited, 0, DBTable_Song),
					new(DBField_SONG_NUMBER, DBFieldType_Int, 0, DBTable_Song),
					new(DBField_BOOK_REFERENCE, DBFieldType_Text, 0, DBTable_Song),
					new(DBField_USER_REFERENCE, DBFieldType_TextUnlimited, 0, DBTable_Song),
					new(DBField_LICENCE_ADMIN1, DBFieldType_Text, 0, DBTable_Song),
					new(DBField_LICENCE_ADMIN2, DBFieldType_Text, 0, DBTable_Song),
					new(DBField_SETTINGS, DBFieldType_TextUnlimited, 0, DBTable_Song),
					new(DBField_FORMATDATA, DBFieldType_TextUnlimited, 0, DBTable_Song)
				];

				return ValidateAndCreateFields(connection, DBTable_Song, songFields);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR validating Song table fields: {ex.Message}, {ex.StackTrace}");
				return false;
			}
		}

		/// <summary>
		/// Validates database schema fields for version 3.4 compatibility
		/// </summary>
		public static bool ValidateVer_3_4_Fields()
		{
			if (!ValidateDB(DatabaseType.Songs))
			{
				return false;
			}

			return ExecuteWithConnection(ConnectStringMainDB, connection =>
			{
				if (connection == null)
				{
					return false;
				}

				bool folderValid = ValidateFolderTableFields(connection);
				if (!folderValid)
				{
					return false;
				}

				// Ensure connection is still open for Song table validation
				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				bool songValid = ValidateSongTableFields(connection);
				return songValid;
			});
		}

		public static bool ValidateMusicExt(ref string InExtension, bool ShowMessage)
		{
			if (!ValidateDirNameFormat(InExtension, ShowMessage ? UiText.Get("gfConfig", "MusicFileExtension") : ""))
			{
				return false;
			}
			
			InExtension = DataUtil.Trim(InExtension);
			var extensionBuilder = new StringBuilder();
			
			for (int i = 0; i < InExtension.Length; i++)
			{
				if (InExtension[i] != '.')
				{
					extensionBuilder.Append(DataUtil.Mid(InExtension, i, 1));
				}
			}
			
			string cleanedExtension = extensionBuilder.ToString();
			if (cleanedExtension.Length > 0 && cleanedExtension[0] != '.')
			{
				cleanedExtension = "." + cleanedExtension;
			}
			
			InExtension = cleanedExtension;
			return true;
		}

		public static void SaveConfigSettings()
		{
			RegUtil.UpdateProfile(SaveConfigurationValues);
			SaveFoldersSettings();
		}

		private static void SaveConfigurationValues()
		{

			RegUtil.SaveRegValue("config", "root_directory", RootAxicultusDir);
			RegUtil.SaveRegValue("config", "current_session", CurSession);
			RegUtil.SaveRegValue("config", "current_praisebook", CurPraiseBook);
			RegUtil.SaveRegValue("config", "export_dir", ExportToDir);
			RegUtil.SaveRegValue("config", "import_dir", ImportFromDir);
			RegUtil.SaveRegValue("config", "praiseoutput_dir", PraiseOutputDir);
			RegUtil.SaveRegValue("config", "media_dir", MediaDir);

			SaveOptionsValues();
			SaveLicenceConfigSettings();
		}

		public static void SaveLicenceConfigSettings()
		{
			RegUtil.SaveRegValue("config", "licCCLI_no", LicenseAdministrators[3].Number);
			RegUtil.SaveRegValue("config", "lic4_no", LicenseAdministrators[4].Number);
			RegUtil.SaveRegValue("config", "lic5_no", LicenseAdministrators[5].Number);
			RegUtil.SaveRegValue("config", "lic6_no", LicenseAdministrators[6].Number);
			RegUtil.SaveRegValue("config", "lic7_no", LicenseAdministrators[7].Number);
			RegUtil.SaveRegValue("config", "lic8_no", LicenseAdministrators[8].Number);
			RegUtil.SaveRegValue("config", "licNoSym", LicAdminNoSymbol);
			RegUtil.SaveRegValue("config", "licEnforceDisplay", LicAdminEnforceDisplay ? 1 : 0);
		}

		public static bool ValidateDefaultFolders()
		{
			return ValidateDefaultFolders(0);
		}

		public static bool ValidateDefaultFolders(int CheckColumns)
		{
			if (!ValidateDB(DatabaseType.Songs))
			{
				return false;
			}
			for (int i = 0; i < MAXSONGSFOLDERS; i++)
			{
				ResetFolder(i, "", ConnectStringMainDB);
			}
			return true;
		}

		public static bool ValidateColour(int InNumber)
		{
			try
			{
				Color color = Color.FromArgb(Convert.ToInt32(InNumber));
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Resets SongSettings to default values
		/// </summary>
		private static void ResetSongSettings(ref SongSettings item)
		{
			item.ItemID = "";
			item.PrevItemPP = (item.Type == PPFileSym);
			item.Type = "";
			item.SongNumber = 0;
			item.FolderNo = 0;
			item.CompleteLyrics = "";
			item.SongSequence = "";
			item.SongBasicSequence = "";
			item.SongOriginalLoadedSequence = "";
			item.Writer = "";
			item.Copyright = "";
			item.Capo = -1;
			item.Timing = "";
			item.MusicKey = "";
			item.OriginalNotations = "";
			item.Notations = "";
			item.Category = "";
			item.Show_LicAdminInfo1 = "";
			item.Show_LicAdminInfo2 = "";
			item.In_LicAdminInfo1 = "";
			item.In_LicAdminInfo2 = "";
			item.Book_Reference = "";
			item.User_Reference = "";
			item.HBR2_FolderNo = 0;
			item.HBR2_FontSizeFactor = 100;
			item.FontSizeFactor = 100;
			item.CurSlide = 0;
			item.TotalSlides = 0;
			item.Path = "";
			item.RotateString = "";
			item.RotateStyle = 1;
			item.RotateGap = 0;
			item.RotateTotal = 0;
			item.RotateTimings = "";
			item.RotateSequence = "";
			item.FirstShowing = true;
			item.FolderName = "";
			item.PrevTitle = "";
			item.NextTitle = "";
		}

		/// <summary>
		/// Resets format-related settings
		/// </summary>
		private static void ResetFormatSettings(ref SongSettings item)
		{
			item.Format.ImageString = "";
			item.Format.TempImageFileName = "";
			item.Format.FormatString = "";
			item.Format.DBStoredFormat = "";
		}

		/// <summary>
		/// Applies gap media settings based on the specified gap media type
		/// </summary>
		private static void ApplyGapMediaSettings(ref SongSettings item, GapMedia gapMedia)
		{
			// Adjust gap media if needed
			if (gapMedia == GapMedia.SessionMedia && MediaOption == 1)
			{
				gapMedia = GapMedia.None;
			}

			switch (gapMedia)
			{
				case GapMedia.SameAsPrevious:
					item.Format.MediaOption = (item.UseDefaultFormat ? MediaOption : item.Format.MediaOption);
					item.Format.MediaLocation = (item.UseDefaultFormat ? MediaLocation : item.Format.MediaLocation);
					item.Format.MediaVolume = (item.UseDefaultFormat ? MediaVolume : item.Format.MediaVolume);
					item.Format.MediaBalance = (item.UseDefaultFormat ? MediaBalance : item.Format.MediaBalance);
					item.Format.MediaCaptureDeviceNumber = (item.UseDefaultFormat ? MediaCaptureDeviceNumber : item.Format.MediaCaptureDeviceNumber);
					break;
				case GapMedia.SessionMedia:
					item.Format.MediaOption = MediaOption;
					item.Format.MediaLocation = MediaLocation;
					item.Format.MediaVolume = MediaVolume;
					item.Format.MediaBalance = MediaBalance;
					item.Format.MediaCaptureDeviceNumber = MediaCaptureDeviceNumber;
					break;
				default:
					item.Title = "";
					item.Title2 = "";
					item.Format.MediaOption = 0;
					item.Format.MediaLocation = "";
					item.Format.MediaVolume = 0;
					item.Format.MediaBalance = 0;
					item.Format.MediaCaptureDeviceNumber = 0;
					break;
			}
			item.UseDefaultFormat = true;
		}

		public static void InitialiseIndividualData(ref SongSettings InItem)
		{
			InitialiseIndividualData(ref InItem, GapMedia.None, "");
		}

		public static void InitialiseIndividualData(ref SongSettings InItem, GapMedia InGapMedia, string InType)
		{
			ResetSongSettings(ref InItem);
			ResetFormatSettings(ref InItem);
			ApplyGapMediaSettings(ref InItem, InGapMedia);
		}

		public static void ValidateSequence(ref SongSettings InItem)
		{
			if (InItem.SongSequence == null)
			{
				InItem.SongSequence = "";
			}
			
			var sequenceBuilder = new StringBuilder();
			for (int i = 0; i < InItem.SongSequence.Length; i++)
			{
				// A sequence saved as text by another program can hold any character, not only verse codes.
				int verse = InItem.SongSequence[i];
				if (verse < InItem.VersePresent.Length && InItem.VersePresent[verse])
				{
					sequenceBuilder.Append(DataUtil.Mid(InItem.SongSequence, i, 1));
				}
			}
			
			if (sequenceBuilder.Length > 0)
			{
				InItem.SongSequence = sequenceBuilder.ToString();
			}
			else
			{
				InItem.SongSequence = InItem.SongBasicSequence;
			}
		}

		/// <summary>
		/// Validates history items by removing empty entries and ensuring count is within limits
		/// </summary>
		private static void ValidateHistoryItems(ref EditHistoryList historyList, ref int totalCount, int maxItems, bool validateMaxLimit)
		{
			if (validateMaxLimit && (totalCount < 0 || totalCount > maxItems))
			{
				totalCount = maxItems;
			}

			int validCount = 0;
			for (int i = 1; i <= totalCount; i++)
			{
				if (GetItemTitle(historyList[i].Id) != "")
				{
					validCount++;
					historyList[validCount].Id = historyList[i].Id;
				}
			}
			totalCount = validCount;
			RemoveDuplicateEditorHistoryItems(ref historyList, ref totalCount);
		}

		public static void ValidateMainHistoryItems()
		{
			ValidateHistoryItems(ref MainEditHistoryList, ref TotalMainEditHistory, AbsoluteMaxHitoryItems, validateMaxLimit: true);
		}

		public static void ValidateEditorHistoryItems()
		{
			ValidateHistoryItems(ref EditorEditHistoryList, ref TotalEditorEditHistory, AbsoluteMaxHitoryItems, validateMaxLimit: false);
		}

		public static void ValidateInfoScreenHistoryItems()
		{
			ValidateHistoryItems(ref InfoScreenEditHistoryList, ref TotalInfoScreenEditHistory, AbsoluteMaxHitoryItems, validateMaxLimit: false);
		}

		public static bool ValidateAxicultusXML(ref XmlTextReader reader)
		{
			try
			{
				while (reader.Read())
				{
					if ((reader.NodeType == XmlNodeType.Element) & ProductStorage.IsDocumentRoot(reader.Name))
					{
						return true;
					}
				}
			}
			catch (XmlException ex)
			{
				Trace.WriteLine($"ERROR parsing XML: {ex.Message}, Line: {ex.LineNumber}, Position: {ex.LinePosition}");
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR validating Axicultus XML: {ex.Message}, {ex.StackTrace}");
			}
			return false;
		}

		public static int ValidateSongMovement(ref int InCurItem, int InCurMaxItems, Keys InKey, int InItemNo)
		{
			switch (InKey)
			{
				case Keys.Home:
					InCurItem = 1;
					break;
				case Keys.End:
					InCurItem = InCurMaxItems;
					break;
				case Keys.Prior:
					InCurItem = ((InCurItem <= 2) ? 1 : (InCurItem - 1));
					break;
				case Keys.Next:
					InCurItem = ((InCurItem < InCurMaxItems) ? (InCurItem + 1) : InCurMaxItems);
					break;
				case Keys.None:
					InCurItem = ((InItemNo <= 0) ? InCurItem : ((InItemNo <= InCurMaxItems) ? InItemNo : InCurMaxItems));
					break;
			}
			return InCurItem;
		}

		public static void SetLiveShowScreenSaverSettings()
		{
			SystemParametersInfo(16, 0, ref PriorScreenSaverState, 0);
			if (PriorScreenSaverState && DisableSreenSaver)
			{
				SetScreenSaverActive(SetOn: false);
			}
		}

		public static void RestoreScreenSaverSettings()
		{
			if (PriorScreenSaverState)
			{
				SetScreenSaverActive(SetOn: true);
			}
		}

		public static void SetScreenSaverActive(bool SetOn)
		{
			SystemParametersInfo(17, SetOn ? 1 : 0, ref SetOn, 0);
		}
	}

