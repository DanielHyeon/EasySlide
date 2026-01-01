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
using Easislides.Module;
using Easislides.SQLite;
using Easislides.Util;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbConnection = System.Data.SQLite.SQLiteConnection;

namespace Easislides
{
	internal unsafe partial class gf
	{
		// Field definition structure for database schema validation
		private struct FieldDefinition
		{
			public string Name;
			public int Type;
			public int Size;
			public string TableName;

			public FieldDefinition(string name, int type, int size, string tableName)
			{
				Name = name;
				Type = type;
				Size = size;
				TableName = tableName;
			}
		}

		/// <summary>
		/// Checks and updates version information in registry
		/// </summary>
		private static void CheckAndUpdateVersion()
		{
			string version = RegUtil.GetRegValue("config", "version", "none");
			if (version != EasiSlides_Version)
			{
				RegUtil.SaveRegValue("config", "version", EasiSlides_Version);
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
				"Would you like to Install the EasiSlides Lyrics Database supplied with EasiSlides 4.0.5? Your existing Lyrics Database, if any, will be renamed and retained for backup.  Click Yes to Install, or No to continue using existing database.",
				$"New EasiSlides Version {EasiSlides_Version} ... Replace existing Database with Supplied Database?",
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
				"Cannot find the Lyrics Database.  Would you like to Install the EasiSlides Lyrics Database supplied with EasiSlides 4.0.5?  Click Yes to Install, or No to create a new blank database.",
				$"Loading EasiSlides {EasiSlides_Version} ... Install Supplied Database?",
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
					string message = "Lyrics Database installed successfully.";
					if (!string.IsNullOrEmpty(backupFileName))
					{
						message += $" Existing database has been renamed to: {backupFileName}";
					}
					MessageBox.Show(message);
				}
			}
		}

		public static bool InitEasiSlidesDir()
		{
			RootEasiSlidesDir = RegUtil.GetRegValue("config", "root_directory", DefaultEasiSlidesDir);
			if (string.IsNullOrEmpty(RootEasiSlidesDir))
			{
				RootEasiSlidesDir = DefaultEasiSlidesDir;
			}

			CheckAndUpdateVersion();
			CheckAndUpdateDatabaseVersion();

			if (!ValidateRootFolder())
			{
				return false;
			}

			RegUtil.SaveRegValue("config", "root_directory", RootEasiSlidesDir);
			ValidateID();

			DBFileName = RootEasiSlidesDir + DefaultDBDir + "EasiSlidesDb.db";
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
		/// Builds the symbols string from verse symbols
		/// </summary>
		private static void BuildSymbolsString()
		{
			var symbolsBuilder = new StringBuilder();
			
			// Add numbered verse symbols (1-99)
			for (int i = 1; i <= 99; i++)
			{
				VerseSymbol[i] = "[" + VerseTitle[i] + "]";
				SequenceSymbol[i] = VerseTitle[i];
				symbolsBuilder.Append(VerseSymbol[i]).Append(",");
			}
			
			// Append additional symbols
			symbolsBuilder.Append(VerseSymbol[0]).Append(",")
				.Append(VerseSymbol[102]).Append(",")
				.Append(VerseSymbol[100]).Append(",")
				.Append(VerseSymbol[103]).Append(",")
				.Append(VerseSymbol[111]).Append(",")
				.Append(VerseSymbol[112]).Append(",")
				.Append(VerseSymbol[101]).Append(",")
				.Append(VerseSymbol[150]);
			
			SymbolsString = symbolsBuilder.ToString();
			xArray = SymbolsString.Split(',');
		}

		/// <summary>
		/// Initializes database file paths and connection strings
		/// </summary>
		private static void InitializeDatabasePaths()
		{
			UsageFileName = RootEasiSlidesDir + DefaultDBDir + DefaultUsageFilename.Replace(".mdb", ".db");
			BiblesListFileName = RootEasiSlidesDir + DefaultDBDir + DefaultBibleDBFilename.Replace(".mdb", ".db");
			tempDBFileName = RootEasiSlidesDir + DefaultDBDir + "~tempEasiSlidesDb.db";
			tempUsageFileName = RootEasiSlidesDir + DefaultDBDir + "~tempEsUsage.db";
			tempBiblesListFileName = RootEasiSlidesDir + DefaultDBDir + "~tempEsBiblesList.db";
			ConnectStringMainDB = ConnectStringDef + DBFileName;
			ConnectStringUsageDB = ConnectStringDef + UsageFileName;
			ConnectStringBibleDB = ConnectStringDef + BiblesListFileName;
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
			HelpFile_Location = Application.StartupPath + "\\Sys\\EasiSlidesHelp.chm";
			MusicSymLen = MusicSym.Length;
			MaxBackgroundStyleIndex = BackPattern.MaxStyleIndex;
			ShowScreenColour[0] = DefaultBackColour;
			ShowScreenColour[1] = DefaultBackColour;
			ShowScreenStyle = 0;
			ShowFontColour[0] = DefaultBackColour;
			ShowFontColour[1] = DefaultBackColour;

			InitializeVerseTitles();
			InitializeVerseSymbols();
			InitializeSequenceSymbols();
			BuildSymbolsString();

			ShowLMargin = Screen.PrimaryScreen.Bounds.Width / 50;
			ShowRMargin = ShowLMargin;
			ShowLyricsWidth = Screen.PrimaryScreen.Bounds.Width - ShowLMargin - ShowRMargin;
			
			InitializeDatabasePaths();
			UserString = DataUtil.Trim(RegUtil.GetRegValue("config", "RegistrationUser", ""));

			if (!ValidateVer_3_4_Fields())
			{
				return false;
			}

			LoadSavedData();

			GenerateMusicKeysList();
			DisplayInfo.SizeLaunchDisplay();
			ResetShowRunningSettings();

			AlertsDataFile = RootEasiSlidesDir + DefaultDBDir + DefaultAlertsFilename;
			ParentalDataFile = RootEasiSlidesDir + DefaultDBDir + DefaultParentalFilename;
			
			InitializeItemInstances();
			SetListViewColumns(ListViewNotations, 5);
			
			var task1 = Task.Run(() =>
			{
				LivePP.Init();
				PreviewPPT.Init();
				OutputPPT.Init();
				ExternalPPT.Init();
			});

			SetPatternPeriod();

			return true;
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
			if (DataUtil.Right(RootEasiSlidesDir, 1) != "\\")
			{
				RootEasiSlidesDir += "\\";
			}
			if (Directory.Exists(RootEasiSlidesDir))
			{
				return true;
			}
			try
			{
				if (ApplicationFirstRun)
				{
					if (FileUtil.MakeDir(RootEasiSlidesDir))
					{
						RestoreSongsDatabase = true;
						RestoreBackgroundImages();
						return true;
					}
					MessageOverSplashScreen("Error encountered whilst creating the EasiSlides Working folder: " + RootEasiSlidesDir + ". Make sure have write access to the area and try again");
					return false;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR creating EasiSlides working folder: {ex.Message}, {ex.StackTrace}");
			}
			SplashScreenBack = true;
			FrmGetWorkingFolder frmGetWorkingFolder = new FrmGetWorkingFolder();
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
				using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
				{
					null,
					null,
					tableName,
					null
				});

				foreach (DataRow row in dbSchemaTable.Rows)
				{
					string columnName = DataUtil.ObjToString(row["COLUMN_NAME"]);
					if (!string.IsNullOrEmpty(columnName))
					{
						columns.Add(columnName.ToUpper());
					}
				}
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
		private static void ValidateAndCreateFields(DbConnection connection, string tableName, FieldDefinition[] fieldDefinitions)
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				return;
			}

			HashSet<string> existingColumns = GetTableColumns(connection, tableName);

			foreach (var fieldDef in fieldDefinitions)
			{
				string fieldNameUpper = fieldDef.Name.ToUpper();
				if (!existingColumns.Contains(fieldNameUpper))
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
					}
				}
			}
		}

		/// <summary>
		/// Validates and updates field sizes if needed
		/// </summary>
		private static void ValidateFieldSizes(DbConnection connection, string tableName, Dictionary<string, int> fieldSizes)
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				return;
			}

			try
			{
				using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
				{
					null,
					null,
					tableName,
					null
				});

				foreach (DataRow row in dbSchemaTable.Rows)
				{
					string columnName = DataUtil.ObjToString(row["COLUMN_NAME"]).ToUpper();
					if (fieldSizes.ContainsKey(columnName))
					{
						int currentSize = DataUtil.ObjToInt(row["CHARACTER_MAXIMUM_LENGTH"]);
						int requiredSize = fieldSizes[columnName];

						if (currentSize > 1 && currentSize < requiredSize)
						{
							try
							{
								string alterQuery = "";
								if (columnName == DBField_BOOK_REFERENCE.ToUpper())
								{
									alterQuery = $"ALTER TABLE {tableName} ALTER COLUMN {DBField_BOOK_REFERENCE} TEXT ({requiredSize})";
								}
								else if (columnName == DBField_SEQUENCE.ToUpper())
								{
									alterQuery = $"ALTER TABLE {tableName} MODIFY {DBField_SEQUENCE} varchar({requiredSize})";
								}
								else if (columnName == DBField_USER_REFERENCE.ToUpper())
								{
									alterQuery = $"ALTER TABLE {tableName} MODIFY {DBField_USER_REFERENCE} varchar({requiredSize})";
								}

								if (!string.IsNullOrEmpty(alterQuery))
								{
									using DbCommand command = new DbCommand(alterQuery, connection);
									command.ExecuteNonQuery();
								}
							}
							catch (Exception ex)
							{
								Trace.WriteLine($"ERROR altering field size for {columnName}: {ex.Message}, {ex.StackTrace}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR validating field sizes for table {tableName}: {ex.Message}, {ex.StackTrace}");
			}
		}

		/// <summary>
		/// Validates Folder table fields and creates missing ones
		/// </summary>
		private static bool ValidateFolderTableFields(DbConnection connection)
		{
			try
			{
				FieldDefinition[] folderFields = new FieldDefinition[]
				{
					new FieldDefinition(DBField_BIU0, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_BIU1, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_ColA, DBFieldType_Text, 0, DBTable_Folder),
					new FieldDefinition(DBField_ColB, DBFieldType_Text, 0, DBTable_Folder),
					new FieldDefinition(DBField_PreChorusHeading, DBFieldType_Text, DBFieldSize_DefaultText, DBTable_Folder),
					new FieldDefinition(DBField_LMargin, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_RMargin, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_BMargin, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_BIUHeading, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_HeadingSize, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_HeadingOption, DBFieldType_Int, 0, DBTable_Folder),
					new FieldDefinition(DBField_LineSpacing, DBFieldType_Float, 0, DBTable_Folder),
					new FieldDefinition(DBField_LineSpacing2, DBFieldType_Float, 0, DBTable_Folder)
				};

				ValidateAndCreateFields(connection, DBTable_Folder, folderFields);
				return true;
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
		private static bool ValidateSongTableFields(DbConnection connection)
		{
			try
			{
				// First, validate field sizes for existing fields
				var fieldSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
				{
					{ DBField_BOOK_REFERENCE, DBFieldSize_BookReference },
					{ DBField_SEQUENCE, DBFieldSize_Sequence },
					{ DBField_USER_REFERENCE, DBFieldSize_UserReference }
				};

				ValidateFieldSizes(connection, DBTable_Song, fieldSizes);

				// Get existing columns to check USER_REFERENCE size
				HashSet<string> existingColumns = GetTableColumns(connection, DBTable_Song);
				int userReferenceSize = 0;
				if (existingColumns.Contains(DBField_USER_REFERENCE.ToUpper()))
				{
					try
					{
						using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
						{
							null,
							null,
							DBTable_Song,
							null
						});

						foreach (DataRow row in dbSchemaTable.Rows)
						{
							string columnName = DataUtil.ObjToString(row["COLUMN_NAME"]).ToUpper();
							if (columnName == DBField_USER_REFERENCE.ToUpper())
							{
								userReferenceSize = DataUtil.ObjToInt(row["CHARACTER_MAXIMUM_LENGTH"]);
								break;
							}
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR getting USER_REFERENCE size: {ex.Message}, {ex.StackTrace}");
					}
				}

				// Validate and create missing fields
				FieldDefinition[] songFields = new FieldDefinition[]
				{
					new FieldDefinition(DBField_CAPO, DBFieldType_Int, 0, DBTable_Song),
					new FieldDefinition(DBField_TIMING, DBFieldType_Text, 0, DBTable_Song),
					new FieldDefinition(DBField_SONG_NUMBER, DBFieldType_Int, 0, DBTable_Song),
					new FieldDefinition(DBField_BOOK_REFERENCE, DBFieldType_Text, 0, DBTable_Song),
					new FieldDefinition(DBField_USER_REFERENCE, DBFieldType_TextUnlimited, 0, DBTable_Song),
					new FieldDefinition(DBField_LICENCE_ADMIN1, DBFieldType_Text, 0, DBTable_Song),
					new FieldDefinition(DBField_LICENCE_ADMIN2, DBFieldType_Text, 0, DBTable_Song),
					new FieldDefinition(DBField_SETTINGS, DBFieldType_TextUnlimited, 0, DBTable_Song),
					new FieldDefinition(DBField_FORMATDATA, DBFieldType_TextUnlimited, 0, DBTable_Song)
				};

				ValidateAndCreateFields(connection, DBTable_Song, songFields);

				// Update USER_REFERENCE size if needed
				if (userReferenceSize > 0 && userReferenceSize < DBFieldSize_UserReference)
				{
					try
					{
						using DbCommand command = new DbCommand($"ALTER TABLE {DBTable_Song} MODIFY {DBField_USER_REFERENCE} varchar({DBFieldSize_UserReference})", connection);
						command.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR updating USER_REFERENCE size: {ex.Message}, {ex.StackTrace}");
					}
				}

				return true;
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
			if (!ValidateDirNameFormat(InExtension, ShowMessage ? "Music File Extension" : ""))
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

			RegUtil.SaveRegValue("config", "root_directory", RootEasiSlidesDir);
			RegUtil.SaveRegValue("config", "current_session", CurSession);
			RegUtil.SaveRegValue("config", "current_praisebook", CurPraiseBook);
			RegUtil.SaveRegValue("config", "export_dir", ExportToDir);
			RegUtil.SaveRegValue("config", "import_dir", ImportFromDir);
			RegUtil.SaveRegValue("config", "praiseoutput_dir", PraiseOutputDir);
			RegUtil.SaveRegValue("config", "media_dir", MediaDir);

			SaveOptionsData();
			SaveLicenceConfigSettings();
			SaveFoldersSettings();
		}

		public static void SaveLicenceConfigSettings()
		{
			RegUtil.SaveRegValue("config", "licCCLI_no", LicAdmin_List[3, 1]);
			RegUtil.SaveRegValue("config", "lic4_no", LicAdmin_List[4, 1]);
			RegUtil.SaveRegValue("config", "lic5_no", LicAdmin_List[5, 1]);
			RegUtil.SaveRegValue("config", "lic6_no", LicAdmin_List[6, 1]);
			RegUtil.SaveRegValue("config", "lic7_no", LicAdmin_List[7, 1]);
			RegUtil.SaveRegValue("config", "lic8_no", LicAdmin_List[8, 1]);
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
				if (InItem.VersePresent[InItem.SongSequence[i]])
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
		private static void ValidateHistoryItems(ref string[,] historyList, ref int totalCount, int maxItems, bool validateMaxLimit)
		{
			if (validateMaxLimit && (totalCount < 0 || totalCount > maxItems))
			{
				totalCount = maxItems;
			}

			int validCount = 0;
			for (int i = 1; i <= totalCount; i++)
			{
				if (GetItemTitle(historyList[i, 0]) != "")
				{
					validCount++;
					historyList[validCount, 0] = historyList[i, 0];
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

		public static bool ValidateEasiSlidesXML(ref XmlTextReader reader)
		{
			try
			{
				reader.Read();
				while (reader.Read())
				{
					if ((reader.NodeType == XmlNodeType.Element) & (reader.Name == XMLNode_EasiSlides))
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
				Trace.WriteLine($"ERROR validating EasiSlides XML: {ex.Message}, {ex.StackTrace}");
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
}

