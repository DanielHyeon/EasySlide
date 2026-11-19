using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Easislides.Global;
using Easislides.Module;
using Easislides.SQLite;
using Easislides.Util;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;


namespace Easislides
{
	internal unsafe partial class Gf
	{
		private const int MaxTitleLength = 100;
		private const int MaxSequenceLength = 100;
		private const int MaxWriterLength = 100;
		private const int MaxCopyrightLength = 100;
		private const int MaxTimingLength = 50;
		private const int MaxKeyLength = 20;
		private const int MaxLicAdminLength = 50;
		private const int MaxBookRefLength = 50;

		private static void TruncateSongFields(
			ref string title1, ref string title2, ref string sequence,
			ref string writer, ref string copyright, ref string timing,
			ref string key, ref string licAdmin1, ref string licAdmin2,
			ref string bookRef)
		{
			title1 = DataUtil.Left(title1, MaxTitleLength);
			title2 = DataUtil.Left(title2, MaxTitleLength);
			sequence = DataUtil.Left(sequence, MaxSequenceLength);
			writer = DataUtil.Left(writer, MaxWriterLength);
			copyright = DataUtil.Left(copyright, MaxCopyrightLength);
			timing = DataUtil.Left(timing, MaxTimingLength);
			key = DataUtil.Left(key, MaxKeyLength);
			licAdmin1 = DataUtil.Left(licAdmin1, MaxLicAdminLength);
			licAdmin2 = DataUtil.Left(licAdmin2, MaxLicAdminLength);
			bookRef = DataUtil.Left(bookRef, MaxBookRefLength);
		}

		public static void CovertItemsTov4()
		{
			WorshipDir = RootEasiSlidesDir + "Admin\\WorshipLists\\";
			RenameExtensions(WorshipDir, ".dat", ".esw");
			PraiseBookDir = RootEasiSlidesDir + "Admin\\PraiseBooks\\";
			RenameExtensions(PraiseBookDir, ".dat", ".esp");
			string name = "music_dir";
			string regValue = RegUtil.GetRegValue("config", name, RootEasiSlidesDir + "Music\\");
			if (regValue == RootEasiSlidesDir + "Music\\")
			{
				try
				{
					Directory.Move(regValue, RootEasiSlidesDir + "Media\\");
					RegUtil.SaveRegValue("config", "media_dir", RootEasiSlidesDir + "Media\\");
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to move Music directory to Media: {ex.Message}");
				}
			}
			else if (regValue != "")
			{
				RegUtil.SaveRegValue("config", "media_dir", regValue);
			}
			RegUtil.DeleteRegKey("config", name);
			string legacyMusicExtensionsPath = RootEasiSlidesDir + "Admin\\Database\\MusicExtensions.txt";
			string mediaExtensionsPath = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
			if (File.Exists(legacyMusicExtensionsPath) && !File.Exists(mediaExtensionsPath))
			{
				try
				{
					File.Move(legacyMusicExtensionsPath, mediaExtensionsPath);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to move MusicExtensions.txt to MediaExtensions.txt: {ex.Message}");
				}
			}
			string legacyImagesDirectory = RootEasiSlidesDir + "User Images";
			string imagesDirectory = RootEasiSlidesDir + "Images\\";
			if (Directory.Exists(legacyImagesDirectory) && !Directory.Exists(imagesDirectory))
			{
				try
				{
					Directory.Move(legacyImagesDirectory, imagesDirectory);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to move 'User Images' directory to 'Images': {ex.Message}");
				}
			}
		}

		public static int InsertItemIntoDatabase(string InConnectString, SongSettings InItem)
		{
			return InsertItemIntoDatabase(InConnectString, InItem.Title, InItem.Title2, InItem.SongNumber, InItem.FolderNo, InItem.CompleteLyrics, InItem.SongSequence, InItem.Writer, InItem.Copyright, InItem.Capo, InItem.Timing, InItem.MusicKey, InItem.Notations, InItem.Category, InItem.Show_LicAdminInfo1, InItem.Show_LicAdminInfo2, InItem.Book_Reference, InItem.User_Reference, InItem.Settings, InItem.Format.FormatString);
		}

		public static int InsertItemIntoDatabase(string InConnectString, object Title_1, object Title_2, object song_number, int FolderNo, object Lyrics, object Sequence, object writer, object copyright, object capo, object Timing, object Key, object msc, object CATEGORY, object LICENCE_ADMIN1, object LICENCE_ADMIN2, object BOOK_REFERENCE, object USER_REFERENCE, object Settings, object FormatData)
		{
			string title_ = Title_1.ToString();
			string title_2 = Title_2.ToString();
			int song_number2 = DataUtil.StringToInt(song_number.ToString());
			string lyrics = Lyrics.ToString();
			string sequence = Sequence.ToString();
			string writer2 = writer.ToString();
			string copyright2 = copyright.ToString();
			int capo2 = DataUtil.StringToInt(capo.ToString(), Minus1IfBlank: true);
			string timing = Timing.ToString();
			string inKey = Key.ToString();
			string msc2 = msc.ToString();
			string cATEGORY = CATEGORY.ToString();
			string lICENCE_ADMIN = LICENCE_ADMIN1.ToString();
			string lICENCE_ADMIN2 = LICENCE_ADMIN2.ToString();
			string bOOK_REFERENCE = BOOK_REFERENCE.ToString();
			string uSER_REFERENCE = USER_REFERENCE.ToString();
			string sETTINGS = Settings.ToString();
			string fORMATDATA = FormatData.ToString();
			return InsertItemIntoDatabase(InConnectString, title_, title_2, song_number2, FolderNo, lyrics, sequence, writer2, copyright2, capo2, timing, inKey, msc2, cATEGORY, lICENCE_ADMIN, lICENCE_ADMIN2, bOOK_REFERENCE, uSER_REFERENCE, sETTINGS, fORMATDATA);
		}
		public static int InsertItemIntoDatabase(string InConnectString, string Title_1, string Title_2, int song_number, int FolderNo, string Lyrics, string Sequence, string writer, string copyright, int capo, string Timing, string InKey, string msc, string CATEGORY, string LICENCE_ADMIN1, string LICENCE_ADMIN2, string BOOK_REFERENCE, string USER_REFERENCE, string SETTINGS, string FORMATDATA)
		{
			TruncateSongFields(ref Title_1, ref Title_2, ref Sequence, ref writer, ref copyright,
				ref Timing, ref InKey, ref LICENCE_ADMIN1, ref LICENCE_ADMIN2, ref BOOK_REFERENCE);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);

			// A plain INSERT; SQLite assigns SONGID (the rowid). The former DataTable insert loaded the whole SONG
			// table and failed on every song ("SONGID does not allow nulls"): the table declares SONGID as
			// PRIMARY KEY("SONGID"), which the provider does not report as auto-increment.
			const string sql = "insert into SONG (Title_1, Title_2, song_number, FolderNo, Lyrics, Sequence, writer, copyright, "
				+ "CJK_WordCount, CJK_StrokeCount, capo, Timing, [Key], msc, CATEGORY, LICENCE_ADMIN1, LICENCE_ADMIN2, "
				+ "BOOK_REFERENCE, USER_REFERENCE, SETTINGS, FORMATDATA, LastModified) values (@Title_1, @Title_2, "
				+ "@song_number, @FolderNo, @Lyrics, @Sequence, @writer, @copyright, @CJK_WordCount, @CJK_StrokeCount, @capo, "
				+ "@Timing, @Key, @msc, @CATEGORY, @LICENCE_ADMIN1, @LICENCE_ADMIN2, @BOOK_REFERENCE, @USER_REFERENCE, "
				+ "@SETTINGS, @FORMATDATA, @LastModified); select last_insert_rowid();";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(InConnectString);
				using DbCommand command = new DbCommand(sql, connection);
				command.Parameters.AddWithValue("@Title_1", Title_1);
				command.Parameters.AddWithValue("@Title_2", Title_2);
				command.Parameters.AddWithValue("@song_number", song_number);
				command.Parameters.AddWithValue("@FolderNo", FolderNo);
				command.Parameters.AddWithValue("@Lyrics", Lyrics);
				command.Parameters.AddWithValue("@Sequence", Sequence);
				command.Parameters.AddWithValue("@writer", writer);
				command.Parameters.AddWithValue("@copyright", copyright);
				command.Parameters.AddWithValue("@CJK_WordCount", value);
				command.Parameters.AddWithValue("@CJK_StrokeCount", value2);
				command.Parameters.AddWithValue("@capo", capo);
				command.Parameters.AddWithValue("@Timing", Timing);
				command.Parameters.AddWithValue("@Key", InKey);
				command.Parameters.AddWithValue("@msc", msc);
				command.Parameters.AddWithValue("@CATEGORY", CATEGORY);
				command.Parameters.AddWithValue("@LICENCE_ADMIN1", LICENCE_ADMIN1);
				command.Parameters.AddWithValue("@LICENCE_ADMIN2", LICENCE_ADMIN2);
				command.Parameters.AddWithValue("@BOOK_REFERENCE", BOOK_REFERENCE);
				command.Parameters.AddWithValue("@USER_REFERENCE", USER_REFERENCE);
				command.Parameters.AddWithValue("@SETTINGS", SETTINGS);
				command.Parameters.AddWithValue("@FORMATDATA", FORMATDATA);
				command.Parameters.AddWithValue("@LastModified", DataUtil.ToStoredDate(DateTime.Now));
				int songId = DataUtil.ObjToInt(command.ExecuteScalar());
				InvalidateScriptureSongs();
				return songId;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error adding item to database: {ex.Message}");
				MessageBox.Show("Error encountered whilst adding item to database - item NOT added");
			}
			return 0;
		}

		public static bool UpdateDatabaseItem(string InConnectString, SongSettings InItem, int SongID)
		{
			return UpdateDatabaseItem(InConnectString, SongID, InItem.Title, InItem.Title2, InItem.SongNumber, InItem.FolderNo, InItem.CompleteLyrics, InItem.SongSequence, InItem.Writer, InItem.Copyright, InItem.Capo, InItem.Timing, InItem.MusicKey, InItem.Notations, InItem.Category, InItem.Show_LicAdminInfo1, InItem.Show_LicAdminInfo2, InItem.Book_Reference, InItem.User_Reference, InItem.Settings, InItem.Format.FormatString);
		}

		public static bool UpdateDatabaseItem(string InConnectString, int SongID, string Title_1, string Title_2, int song_number, int FolderNo, string Lyrics, string Sequence, string writer, string copyright, int capo, string Timing, string InKey, string msc, string CATEGORY, string LICENCE_ADMIN1, string LICENCE_ADMIN2, string BOOK_REFERENCE, string USER_REFERENCE, string SETTINGS, string FORMATDATA)
		{
			bool result = false;
			TruncateSongFields(ref Title_1, ref Title_2, ref Sequence, ref writer, ref copyright,
				ref Timing, ref InKey, ref LICENCE_ADMIN1, ref LICENCE_ADMIN2, ref BOOK_REFERENCE);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);
			try
			{

				// GetDbConnection returns an open connection; opening it again threw, so no edit was ever saved.
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				string updateSongSql = "Update SONG SET Title_1 =@Title_1,Title_2 =@Title_2,song_number =@song_number,FolderNo =@FolderNo,Lyrics =@Lyrics,Sequence =@Sequence,writer =@writer,copyright =@copyright,CJK_WordCount =@CJK_WordCount,CJK_StrokeCount =@CJK_StrokeCount,capo =@capo,Timing =@Timing,[Key] =@Key,msc =@msc,CATEGORY =@CATEGORY,LICENCE_ADMIN1 =@LICENCE_ADMIN1,LICENCE_ADMIN2 =@LICENCE_ADMIN2,BOOK_REFERENCE =@BOOK_REFERENCE,USER_REFERENCE =@USER_REFERENCE,SETTINGS =@SETTINGS,FORMATDATA =@FORMATDATA,LastModified =@LastModified where songid=" + SongID;
				using DbCommand command = new DbCommand(updateSongSql, connection);
				command.CommandText = updateSongSql;
				command.Parameters.AddWithValue("@Title_1", Title_1);
				command.Parameters.AddWithValue("@Title_2", Title_2);
				command.Parameters.AddWithValue("@song_number", song_number);
				command.Parameters.AddWithValue("@FolderNo", FolderNo);
				command.Parameters.AddWithValue("@Lyrics", Lyrics);
				command.Parameters.AddWithValue("@Sequence", Sequence);
				command.Parameters.AddWithValue("@writer", writer);
				command.Parameters.AddWithValue("@copyright", copyright);
				command.Parameters.AddWithValue("@CJK_WordCount", value);
				command.Parameters.AddWithValue("@CJK_StrokeCount", value2);
				command.Parameters.AddWithValue("@capo", capo);
				command.Parameters.AddWithValue("@Timing", Timing);
				command.Parameters.AddWithValue("@Key", InKey);
				command.Parameters.AddWithValue("@msc", msc);
				command.Parameters.AddWithValue("@CATEGORY", CATEGORY);
				command.Parameters.AddWithValue("@LICENCE_ADMIN1", LICENCE_ADMIN1);
				command.Parameters.AddWithValue("@LICENCE_ADMIN2", LICENCE_ADMIN2);
				command.Parameters.AddWithValue("@BOOK_REFERENCE", BOOK_REFERENCE);
				command.Parameters.AddWithValue("@USER_REFERENCE", USER_REFERENCE);
				command.Parameters.AddWithValue("@SETTINGS", SETTINGS);
				command.Parameters.AddWithValue("@FORMATDATA", FORMATDATA);
				command.Parameters.AddWithValue("@LastModified", DataUtil.ToStoredDate(DateTime.Now));
				command.ExecuteNonQuery();
				InvalidateScriptureSongs();
				result = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error when writing to Database: \n" + ex.Message + ex.StackTrace);
			}
			return result;
		}

		public static string RestoreOriginalSongsDatabase()
		{
			string bundledDatabasePath = $@"{Application.StartupPath}Sys\EasiSlidesDb.db";

			RestoreSongsDatabase = false;
			if (File.Exists(bundledDatabasePath))
			{
				try
				{
					FileUtil.MakeDir(RootEasiSlidesDir + "Admin\\Database\\");
					int backupNumber = 1;
					string dBFileName = DBFileName;
					if (File.Exists(dBFileName))
					{
						dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{backupNumber}{Path.GetExtension("EasiSlidesDb.db")}";
						while (File.Exists(dBFileName))
						{
							backupNumber++;
							dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{backupNumber}{Path.GetExtension("EasiSlidesDb.db")}";
						}
					}
					else
					{
						dBFileName = "";
					}
					try
					{
						if (dBFileName != "")
						{
							File.Move(DBFileName, dBFileName);
						}
						File.Copy(bundledDatabasePath, DBFileName);
						return dBFileName;
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Failed to restore database file: {ex.Message}");
					}
					MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please make sure the existing database is not in use and then try again.");
					return "-1";
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to create database directory or backup: {ex.Message}");
					MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please make sure the existing database is not in use and then try again.");
					return "-1";
				}
			}
			MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please re-install EasiSlides Software.");
			return "-1";
		}

		public static void RestoreBackgroundImages()
		{
			string bundledSceneryDirectory = $@"{Application.StartupPath}\Backgrounds\Scenery\";
			string bundledTilesDirectory = $@"{Application.StartupPath}\Backgrounds\Tiles\";
			string sceneryDirectory = $@"{RootEasiSlidesDir}Images\Scenery\";
			string tilesDirectory = $@"{RootEasiSlidesDir}Images\Tiles\";
			if (FileUtil.MakeDir($@"{RootEasiSlidesDir}Images\"))
			{
				if (Directory.Exists(bundledSceneryDirectory) && FileUtil.MakeDir(sceneryDirectory))
				{
					FileUtil.CopyFiles(bundledSceneryDirectory, sceneryDirectory);
				}
				if (Directory.Exists(bundledTilesDirectory) && FileUtil.MakeDir(tilesDirectory))
				{
					FileUtil.CopyFiles(bundledTilesDirectory, tilesDirectory);
				}
			}
		}

		public static bool ValidateDB(DatabaseType InType)
		{
			string destFileName = "";
			string dbFileName = "";
			switch (InType)
			{
				case DatabaseType.Usages:
					destFileName = UsageFileName;
					dbFileName = "EsUsage";
					break;
				case DatabaseType.Bible:
					destFileName = BiblesListFileName;
					dbFileName = "EsBiblesList";
					break;
				default:
					destFileName = DBFileName;
					dbFileName = "EasiSlidesDb";
					break;
			}
			if (File.Exists(destFileName))
			{
				return true;
			}
			string soucreFileName = $@"{Application.StartupPath}\Sys\{dbFileName}.db";

			if (File.Exists(soucreFileName))
			{
				Directory.CreateDirectory($@"{RootEasiSlidesDir}Admin\Database\");
				File.Copy(soucreFileName, destFileName, overwrite: true);
				return true;
			}
			MessageOverSplashScreen(@$"Sorry, cannot create new {dbFileName} database. Please re-install EasiSlides Software.");
			return false;
		}

		public static bool DeleteAllFolders(string InConnectString)
		{
			if (InConnectString == "" || InConnectString == ConnectStringMainDB)
			{
				return false;
			}
			try
			{
				using DbConnection connection = DbController.GetDbConnection(InConnectString);
				using var transaction = connection.BeginTransaction();
				using DbCommand command = new DbCommand("DELETE FROM Folder", connection, transaction);
				int affectedRows = command.ExecuteNonQuery();
				transaction.Commit();
				if (affectedRows == 0)
				{
					Debug.WriteLine("No folders were deleted from Folder table.");
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to delete all folders: {ex.Message}");
			}
			return false;
		}

		public static void ResetFolder(int FNumber, string InFolderName, string InConnectString)
		{

			using DbConnection connection = DbController.GetDbConnection(InConnectString);

			try
			{
				bool isFolderEnabled = false;
				string cmdText = $@"select count(*) from FOLDER where FolderNo={DataUtil.ObjToString(FNumber)}";

				DbCommand command = new DbCommand(cmdText, connection);

				if (DataUtil.ObjToInt(command.ExecuteScalar()) == 0)
				{
					string value;
					if (InFolderName == "")
					{
						if (FNumber > 0)
						{
							value = $@"Folder {DataUtil.ObjToString(FNumber)}";
							isFolderEnabled = ((FNumber < 4) ? true : false);
						}
						else
						{
							value = "Recycle Folder";
							isFolderEnabled = false;
						}
					}
					else
					{
						value = InFolderName;
						isFolderEnabled = true;
					}
					cmdText = (command.CommandText = "Insert into Folder (FolderNo,name,Use,GroupStyle,PreChorusHeading,ChorusHeading, BridgeHeading,EndingHeading,BIUHeading,HeadingSize,HeadingOption,BIU0,Size0,Bold0,Align0,FontName0,Vpos0,BIU1,Size1,Bold1,Align1,FontName1,Vpos1)  Values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
					command.Parameters.AddWithValue("@FolderNo", FNumber);
					command.Parameters.AddWithValue("@Name", value);
					command.Parameters.AddWithValue("@Use", isFolderEnabled);
					command.Parameters.AddWithValue("@GroupStyle", SortBy.Alpha);
					command.Parameters.AddWithValue("@PreChorusHeading", "");
					command.Parameters.AddWithValue("@ChorusHeading", "Chorus:");
					command.Parameters.AddWithValue("@BridgeHeading", "");
					command.Parameters.AddWithValue("@EndingHeading", "");
					command.Parameters.AddWithValue("@BIUHeading", 4);
					command.Parameters.AddWithValue("@HeadingSize", 100);
					command.Parameters.AddWithValue("@HeadingOption", HeadingFormat.AsRegion1Plus);
					command.Parameters.AddWithValue("@BIU0", 0);
					command.Parameters.AddWithValue("@Size0", 40);
					command.Parameters.AddWithValue("@Bold0", false);
					command.Parameters.AddWithValue("@Align0", 2);
					command.Parameters.AddWithValue("@FontName0", "Microsoft Sans Serif");
					command.Parameters.AddWithValue("@Vpos0", 0);
					command.Parameters.AddWithValue("@BIU1", 0);
					command.Parameters.AddWithValue("@Size1", 40);
					command.Parameters.AddWithValue("@Bold1", false);
					command.Parameters.AddWithValue("@Align1", 2);
					command.Parameters.AddWithValue("@FontName1", "Microsoft Sans Serif");
					command.Parameters.AddWithValue("@Vpos1", 50);
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to reset folder {FNumber}: {ex.Message}");
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
		}

		public static void LoadSavedData()
		{
			for (int i = 0; i < MAXSONGSFOLDERS; i++)
			{
				Folders[i].FindSongs = true;
			}
			RootEasiSlidesDir = RegUtil.GetRegValue("config", "root_directory", "C:\\EasiSlides\\");
			WorshipDir = $@"{RootEasiSlidesDir}Admin\WorshipLists\";
			if (!Directory.Exists(WorshipDir))
			{
				FileUtil.MakeDir(WorshipDir);
			}
			InfoScreenDir = $@"{RootEasiSlidesDir}InfoScreens\";
			if (!Directory.Exists(InfoScreenDir))
			{
				FileUtil.MakeDir(InfoScreenDir);
			}
			PowerpointDir = $@"{RootEasiSlidesDir}Powerpoint\";
			if (!Directory.Exists(PowerpointDir))
			{
				FileUtil.MakeDir(PowerpointDir);
			}
			BibleDir = $@"{RootEasiSlidesDir}HolyBibles\";
			if (!Directory.Exists(BibleDir))
			{
				FileUtil.MakeDir(BibleDir);
			}
			PraiseBookDir = $@"{RootEasiSlidesDir}Admin\PraiseBooks\";
			if (!Directory.Exists(PraiseBookDir))
			{
				FileUtil.MakeDir(PraiseBookDir);
			}
			WorshipTemplatesDir = $@"{RootEasiSlidesDir}Admin\Templates\WorshipListsTemplates\";
			if (!Directory.Exists(WorshipTemplatesDir))
			{
				FileUtil.MakeDir(WorshipTemplatesDir);
			}
			SettingsTemplatesDir = $@"{RootEasiSlidesDir}Admin\Templates\SettingsTemplates\";
			if (!Directory.Exists(SettingsTemplatesDir))
			{
				FileUtil.MakeDir(SettingsTemplatesDir);
			}
			EasiSlidesTempDir = $@"{Path.GetTempPath()}EasiSlides Files\";
			if (!Directory.Exists(EasiSlidesTempDir))
			{
				FileUtil.MakeDir(EasiSlidesTempDir);
			}
			MediaDir = $@"{RootEasiSlidesDir}Media\";
			if (!Directory.Exists(MediaDir))
			{
				FileUtil.MakeDir(MediaDir);
			}
			DocumentsDir = $@"{RootEasiSlidesDir}Documents\";
			if (!Directory.Exists(DocumentsDir))
			{
				FileUtil.MakeDir(DocumentsDir);
			}
			ImagesDir = $@"{RootEasiSlidesDir}Images\";
			if (!Directory.Exists(ImagesDir))
			{
				RestoreBackgroundImages();
			}
			if (!Directory.Exists($@"{ImagesDir}Tiles\") || !Directory.Exists($@"{ImagesDir}Scenery\"))
			{
				RestoreBackgroundImages();
			}
			CurSession = RegUtil.GetRegValue("config", "current_session", "");
			CurPraiseBook = RegUtil.GetRegValue("config", "current_praisebook", "");
			string configuredMediaDirectory = RegUtil.GetRegValue("config", "media_dir", MediaDir);
			if (DataUtil.Right(configuredMediaDirectory, 1) != "\\")
			{
				configuredMediaDirectory += "\\";
			}
			if (Directory.Exists(configuredMediaDirectory))
			{
				MediaDir = configuredMediaDirectory;
			}
			ExportToDir = RegUtil.GetRegValue("config", "export_dir", DocumentsDir);
			if (DataUtil.Right(ExportToDir, 1) != "\\")
			{
				ExportToDir += "\\";
			}
			ImportFromDir = RegUtil.GetRegValue("config", "import_dir", DocumentsDir);
			if (DataUtil.Right(ImportFromDir, 1) != "\\")
			{
				ImportFromDir += "\\";
			}
			PraiseOutputDir = RegUtil.GetRegValue("config", "praiseoutput_dir", DocumentsDir);
			if (DataUtil.Right(PraiseOutputDir, 1) != "\\")
			{
				PraiseOutputDir += "\\";
			}
			CurPraiseBook = RegUtil.GetRegValue("config", "current_praisebook", "");
			UseSongNumbers = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseSongNumbers", 0)) > 0) ? true : false);
			PB_PrinterSpaces = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PrinterSpaces", 0)) > 0) ? 1 : 0);
			HB_MaxVersesSelection = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleMaxSelectVerses", 500));
			if (HB_MaxVersesSelection > 1000)
			{
				HB_MaxVersesSelection = 1000;
			}
			HB_MaxAdhocVersesSelection = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleMaxAdhocVersesSelection", 200));
			if (HB_MaxAdhocVersesSelection > 500)
			{
				HB_MaxAdhocVersesSelection = 200;
			}
			HB_ShowVerses = true;
			PP_MaxFiles = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PowerpointMaxFiles", 20));
			if (PP_MaxFiles > 100)
			{
				PP_MaxFiles = 100;
			}
			ShowRotateGap = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "RotateGap", 0));
			if ((ShowRotateGap < 0) & (ShowRotateGap > 999))
			{
				ShowRotateGap = 0;
			}
			UsePowerpointTab = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UsePowerpointTab", 0)) > 0) ? true : false);
			NoPowerpointPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NoPowerpointPanelOverlay", 0)) > 0) ? true : false);
			UseMediaTab = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseMediaTab", 0)) > 0) ? true : false);
			ShowLyricsMonitorAlertBox = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ShowLyricsMonitorAlertBox", 0)) > 0) ? true : false);
			NoMediaPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NoMediaPanelOverlay", 0)) > 0) ? true : false);
			AutoTextOverflow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoTextOverflow", 1)) > 0) ? true : false);
			UseLargestFontSize = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseLargestFontSize", 0)) > 0) ? true : false);
			AdvanceNextItem = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AdvanceNextItem", 0)) > 0) ? true : false);
			LineBetweenRegions = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LineBetweenRegions", 1)) > 0) ? true : false);
			GapItemOption = (GapType)DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GapItemOption", 0));
			if ((GapItemOption < GapType.None) & (GapItemOption > GapType.User))
			{
				GapItemOption = GapType.None;
			}
			AltGapItemOption = GapType.None;
			GapItemLogoFile = RegUtil.GetRegValue("options", "GapItemLogoFile", "");
			GapItemUseFade = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GapItemUseFade", 1)) > 0) ? true : false);
			// Off unless chosen: indenting the wrapped lines made the first line look shifted to the left.
			WordWrapLeftAlignIndent = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapLeftAlignIndent", 0)) > 0;
			WordWrapIgnoreStartSpaces = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapIgnoreStartSpaces", 8));
			if (WordWrapIgnoreStartSpaces < 1 || WordWrapIgnoreStartSpaces > 15)
			{
				WordWrapIgnoreStartSpaces = 9;
			}
			AutoRotateOn = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoRotateOn", 1)) > 0) ? true : false);
			AutoRotateStyle = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoRotateStyle", 3));
			if ((AutoRotateStyle < 1) & (AutoRotateStyle > 3))
			{
				AutoRotateStyle = 3;
			}
			int notationFontFactor = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NotationFontFactor", 75));
			if (notationFontFactor < 20 && notationFontFactor > 200)
			{
				notationFontFactor = 75;
			}
			NotationFontFactor = (float)notationFontFactor / 100f;
			PowerpointListingStyle = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ExternalListing", 0));
			if ((PowerpointListingStyle < 0) & (PowerpointListingStyle > 1))
			{
				PowerpointListingStyle = 0;
			}
			KeyBoardOption = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "KeyBoardOption", 0));
			if ((KeyBoardOption < 0) & (KeyBoardOption > 1))
			{
				KeyBoardOption = 0;
			}

			GlobalHookKey_F7 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F7", 0)) > 0) ? true : false);
			GlobalHookKey_F8 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F8", 0)) > 0) ? true : false);
			GlobalHookKey_F9 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F9", 0)) > 0) ? true : false);
			GlobalHookKey_F10 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F10", 0)) > 0) ? true : false);

			GlobalHookKey_Arrow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_Arrow", 0)) > 0) ? true : false);
			GlobalHookKey_CtrlArrow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_CtrlArrow", 0)) > 0) ? true : false);

			EditMainFontName = RegUtil.GetRegValue("options", "EditMainFontName", "Microsoft Sans Serif");
			if (EditMainFontName == "")
			{
				EditMainFontName = "Microsoft Sans Serif";
			}
			EditMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "EditMainFontSize", 12));
			if ((EditMainFontSize < 8) | (EditMainFontSize > 20))
			{
				EditMainFontSize = 12;
			}
			EditNotationFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "EditNotationFontSize", 10));
			if ((EditNotationFontSize < 8) | (EditNotationFontSize > 20))
			{
				EditNotationFontSize = 10;
			}
			InfoMainFontName = RegUtil.GetRegValue("options", "InfoMainFontName", "Microsoft Sans Serif");
			if (InfoMainFontName == "")
			{
				InfoMainFontName = "Microsoft Sans Serif";
			}
			InfoMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "InfoMainFontSize", 12));
			if ((InfoMainFontSize < 8) | (InfoMainFontSize > 20))
			{
				InfoMainFontSize = 12;
			}
			EditOpenDocumentDir = RegUtil.GetRegValue("options", "EditOpenDocumentDir", DocumentsDir);
			if (EditOpenDocumentDir == "")
			{
				EditOpenDocumentDir = DocumentsDir;
			}

			OutputMonitorName = RegUtil.GetRegValue("options", "OutputmonitorName", MonitorLayout.Auto);
			LyricsMonitorName = RegUtil.GetRegValue("options", "LyricsMonitorName", "None");

			BibleText_FontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleTextFontSize", 8));
			if ((BibleText_FontSize < 8) | (BibleText_FontSize > 20))
			{
				BibleText_FontSize = 8;
			}
			PreviewArea_FontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaFontSize", 8));
			if ((PreviewArea_FontSize < 8) | (PreviewArea_FontSize > 20))
			{
				PreviewArea_FontSize = 8;
			}
			PreviewArea_ShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaShowNotations", 0)) > 0) ? true : false);
			PreviewArea_LineBetweenScreens = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaLineBetweenScreens", 0)) > 0) ? true : false);
			ParentalAlertHeading = RegUtil.GetRegValue("options", "ParentalAlertHeading", "");
			ParentalAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertDuration", 20));
			if ((ParentalAlertDuration < 1) | (ParentalAlertDuration > 60))
			{
				ParentalAlertDuration = 30;
			}
			ParentalAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertTextColour", ParentalAlertTextColour.ToArgb())));
			ParentalAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertBackColour", ParentalAlertBackColour.ToArgb())));
			ParentalAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertTextAlign", 3));
			if ((ParentalAlertTextAlign < 1) | (ParentalAlertTextAlign > 3))
			{
				ParentalAlertTextAlign = 3;
			}
			ParentalAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertVerticalAlign", 2));
			if ((ParentalAlertVerticalAlign < 0) | (ParentalAlertVerticalAlign > 2))
			{
				ParentalAlertVerticalAlign = 2;
			}
			int inValue = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertStyle", 3));
			ParentalAlertScroll = DataUtil.GetBitBoolean(inValue, 1);
			ParentalAlertFlash = DataUtil.GetBitBoolean(inValue, 2);
			ParentalAlertTransparent = DataUtil.GetBitBoolean(inValue, 3);
			ParentalAlertFontName = RegUtil.GetRegValue("options", "ParentalAlertFontName", "Microsoft Sans Serif");
			if (ParentalAlertFontName == "")
			{
				ParentalAlertFontName = "Microsoft Sans Serif";
			}
			ParentalAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertFontSize", 22));
			if ((ParentalAlertFontSize < 20) | (ParentalAlertFontSize > 50))
			{
				ParentalAlertFontSize = 25;
			}
			ParentalAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertFontFormat", 0));
			if ((ParentalAlertFontFormat < 0) | (ParentalAlertFontFormat > 100))
			{
				ParentalAlertFontFormat = 0;
			}
			ParentalAlertBold = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 1);
			ParentalAlertItalic = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 2);
			ParentalAlertUnderline = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 3);
			ParentalAlertShadow = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 4);
			ParentalAlertOutline = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 5);
			MessageAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertDuration", 20));
			if ((MessageAlertDuration < 1) | (MessageAlertDuration > 999))
			{
				MessageAlertDuration = 30;
			}
			MessageAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertTextColour", MessageAlertTextColour.ToArgb())));
			MessageAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertBackColour", MessageAlertBackColour.ToArgb())));
			MessageAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertTextAlign", 2));
			if ((MessageAlertTextAlign < 1) | (MessageAlertTextAlign > 3))
			{
				MessageAlertTextAlign = 2;
			}
			MessageAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertVerticalAlign", 2));
			if ((MessageAlertVerticalAlign < 0) | (MessageAlertVerticalAlign > 2))
			{
				MessageAlertVerticalAlign = 2;
			}
			int inValue2 = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertStyle", 3));
			MessageAlertScroll = DataUtil.GetBitBoolean(inValue2, 1);
			MessageAlertFlash = DataUtil.GetBitBoolean(inValue2, 2);
			MessageAlertTransparent = DataUtil.GetBitBoolean(inValue2, 3);
			MessageAlertFontName = RegUtil.GetRegValue("options", "MessageAlertFontName", "Microsoft Sans Serif");
			if (MessageAlertFontName == "")
			{
				MessageAlertFontName = "Microsoft Sans Serif";
			}
			MessageAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertFontSize", 22));
			if ((MessageAlertFontSize < 20) | (MessageAlertFontSize > 50))
			{
				MessageAlertFontSize = 25;
			}
			MessageAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertFontFormat", 0));
			if ((MessageAlertFontFormat < 0) | (MessageAlertFontFormat > 100))
			{
				MessageAlertFontFormat = 0;
			}
			MessageAlertBold = DataUtil.GetBitBoolean(MessageAlertFontFormat, 1);
			MessageAlertItalic = DataUtil.GetBitBoolean(MessageAlertFontFormat, 2);
			MessageAlertUnderline = DataUtil.GetBitBoolean(MessageAlertFontFormat, 3);
			MessageAlertShadow = DataUtil.GetBitBoolean(MessageAlertFontFormat, 4);
			MessageAlertOutline = DataUtil.GetBitBoolean(MessageAlertFontFormat, 5);
			ReferenceAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertDuration", 20));
			if ((ReferenceAlertDuration < 1) | (ReferenceAlertDuration > 999))
			{
				ReferenceAlertDuration = 30;
			}
			ReferenceAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertTextColour", BlackScreenColour.ToArgb())));
			ReferenceAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertBackColour", Color.White.ToArgb())));
			ReferenceAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertTextAlign", 3));
			if ((ReferenceAlertTextAlign < 1) | (ReferenceAlertTextAlign > 3))
			{
				ReferenceAlertTextAlign = 3;
			}
			ReferenceAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertVerticalAlign", 1));
			if ((ReferenceAlertVerticalAlign < 0) | (ReferenceAlertVerticalAlign > 2))
			{
				ReferenceAlertVerticalAlign = 1;
			}
			int inValue3 = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertStyle", 1));
			ReferenceAlertScroll = DataUtil.GetBitBoolean(inValue3, 1);
			ReferenceAlertFlash = DataUtil.GetBitBoolean(inValue3, 2);
			ReferenceAlertTransparent = DataUtil.GetBitBoolean(inValue3, 3);
			ReferenceAlertFontName = RegUtil.GetRegValue("options", "ReferenceAlertFontName", "Microsoft Sans Serif");
			if (ReferenceAlertFontName == "")
			{
				ReferenceAlertFontName = "Microsoft Sans Serif";
			}
			ReferenceAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertFontSize", 22));
			if ((ReferenceAlertFontSize < 20) | (ReferenceAlertFontSize > 50))
			{
				ReferenceAlertFontSize = 25;
			}
			ReferenceAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertFontFormat", 0));
			if ((ReferenceAlertFontFormat < 0) | (ReferenceAlertFontFormat > 100))
			{
				ReferenceAlertFontFormat = 0;
			}
			ReferenceAlertBold = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 1);
			ReferenceAlertItalic = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 2);
			ReferenceAlertUnderline = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 3);
			ReferenceAlertShadow = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 4);
			ReferenceAlertOutline = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 5);
			ReferenceAlertUsePick = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertUsePick", 0)) > 0) ? true : false);
			ReferenceAlertBlankIfPickNotFound = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertBlankIfPickNotFound", 0)) > 0) ? true : false);
			ReferenceAlertSource = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertSource", 0));
			ReferenceAlertPickName = RegUtil.GetRegValue("options", "ReferenceAlertPickName", "");
			ReferenceAlertPickSubstitute = RegUtil.GetRegValue("options", "ReferenceAlertPickSubstitute", "");
			ReferenceAlertPickSeparator = RegUtil.GetRegValue("options", "ReferenceAlertPickSeparator", ",");
			MonitorRegistrySettings.MigrateLegacyDualMonitorValues(RegUtil.Settings);
			DMAlwaysUseSecondaryMonitor = MonitorRegistrySettings.ReadDualMonitorValue(RegUtil.Settings, RegUtil.Reg_DMAlwaysUse, 1) > 0;
			isScreenWideMode = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "IsMonitorWide", 0)) > 0) ? true : false);

			DualMonitorSelectAutoOption = MonitorRegistrySettings.ReadDualMonitorValue(RegUtil.Settings, RegUtil.Reg_DMOption, 0);
			if ((DualMonitorSelectAutoOption < 0) | (DualMonitorSelectAutoOption > 1))
			{
				/// 모니???�택 ?�션
				/// 0??경우 ?�용?��? 리스?�에???�택?�여 ?�동?�로 PPT ?�치�??�는??
				/// 1??경우 ?�용?��? 커스?�?�로 모니???�치�??�동?�로 ?�력?�다.
				DualMonitorSelectAutoOption = 0;
			}
			DMOption1Left = MonitorRegistrySettings.ReadDualMonitorValue(RegUtil.Settings, RegUtil.Reg_DMOptionCusLeft, 0);
			if ((DMOption1Left < -9999) | (DMOption1Left > 9999))
			{
				DMOption1Left = 0;
			}
			DMOption1Top = MonitorRegistrySettings.ReadDualMonitorValue(RegUtil.Settings, RegUtil.Reg_DMOptionCusTop, 0);
			if ((DMOption1Top < -9999) | (DMOption1Top > 9999))
			{
				DMOption1Top = 0;
			}
			DMOption1Width = MonitorRegistrySettings.ReadDualMonitorValue(RegUtil.Settings, RegUtil.Reg_DMOptionCusWidth, 1);
			if ((DMOption1Width < 1) | (DMOption1Width > 9999))
			{
				DMOption1Width = 100;
			}

			DMOption1Height = MonitorLayout.HeightForWidth(DMOption1Width, isScreenWideMode);
			DMOption1AsSingleMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomAsSingle", 0)) > 0) ? true : false);
			DisableSreenSaver = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DisableSreenSaver", 1)) > 0) ? true : false);
			VideoSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "VideoSize", 100));
			if ((VideoSize < 25) | (VideoSize > 100))
			{
				VideoSize = 100;
			}
			VideoVAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "VideoVAlign", 1));
			if ((VideoVAlign < 0) | (VideoVAlign > 2))
			{
				VideoVAlign = 1;
			}
			LMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "AlwaysTrySecondaryLyricsMonitor", 1)) > 0) ? true : false);
			LMSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOption", 0));
			if ((LMSelectAutoOption < 0) | (LMSelectAutoOption > 1))
			{
				LMSelectAutoOption = 0;
			}
			LMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomLeft", 0));
			if ((LMOption1Left < -9999) | (LMOption1Left > 9999))
			{
				LMOption1Left = 0;
			}
			LMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomTop", 0));
			if ((LMOption1Top < -9999) | (LMOption1Top > 9999))
			{
				LMOption1Top = 0;
			}
			LMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomWidth", 1));
			if ((LMOption1Width < 1) | (LMOption1Width > 9999))
			{
				LMOption1Width = 100;
			}

			LMOption1Height = MonitorLayout.HeightForWidth(LMOption1Width, isScreenWideMode);
			LMTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorTextColour", LMTextColour.ToArgb())));
			LMHighlightColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorHighlightColour", LMHighlightColour.ToArgb())));
			LMBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorBackColour", LMBackColour.ToArgb())));
			LMShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorShowNotations", 1)) > 0) ? true : false);
			LMMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorFontSize", 22));
			if ((LMMainFontSize < 8) | (LMMainFontSize > 40))
			{
				LMMainFontSize = 20;
			}
			LMNotationsFontSize = MonitorRegistrySettings.ReadNotationsFontSize(RegUtil.Settings);
			LMFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorFontFormat", 0));
			if ((LMFontFormat < 0) | (LMFontFormat > 7))
			{
				LMFontFormat = 0;
			}
			LMFontBold = DataUtil.GetBitBoolean(LMFontFormat, 1);
			LMFontItalic = DataUtil.GetBitBoolean(LMFontFormat, 2);
			LMFontUnderline = DataUtil.GetBitBoolean(LMFontFormat, 3);
			AutoFocusTextRegion = false;
			UseFocusedTextRegionColour = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseFocusedBackColour", 1)) > 0) ? true : false);
			FocusedTextRegionColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FocusedBackColour", FocusedTextRegionColour.ToArgb())));
			TextRegionSlideTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "SelectedSlideTextColour", TextRegionSlideTextColour.ToArgb())));
			TextRegionSlideBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "SelectedSlideBackColour", TextRegionSlideBackColour.ToArgb())));
			// Missing value or registry read failure = modern look (GetRegValue falls back to the default 1).
			// Value 0, or a value that is not a positive number, = previous look.
			UseModernMainUi = RegUtil.GetRegValue("options", RegUtil.Reg_ModernMainUi, 1) > 0;
			MediaFileExtensions.Clear();
			MediaExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
			AudioExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\AudioExtensions.txt";
			VideoExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\VideoExtensions.txt";
			LoadMusicExtArray();
			JumpToA = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToA", 1));
			if ((JumpToA < 1) | (JumpToA > MAXSONGSFOLDERS))
			{
				JumpToA = 1;
			}
			JumpToB = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToB", 2));
			if ((JumpToB < 1) | (JumpToB > MAXSONGSFOLDERS))
			{
				JumpToB = 2;
			}
			JumpToC = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToC", 3));
			if ((JumpToC < 1) | (JumpToC > MAXSONGSFOLDERS))
			{
				JumpToC = 3;
			}
			LiveCamNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamNumber", 1));
			if ((LiveCamNumber < 1) | (LiveCamNumber > 5))
			{
				LiveCamNumber = 1;
			}
			LiveCamVolume = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamVolume", 50));
			if ((LiveCamVolume < 0) | (LiveCamVolume > 100))
			{
				LiveCamVolume = 50;
			}
			LiveCamBalance = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamBalance", 0));
			if ((LiveCamBalance < -100) | (LiveCamBalance > 100))
			{
				LiveCamBalance = 0;
			}
			LiveCamWidescreen = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamWidescreen", 0)) > 0) ? true : false);
			LiveCamMute = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamMute", 0)) > 0) ? true : false);
			LiveCamNoPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamNoPanelOverlay", 0)) > 0) ? true : false);
			FindItemInTitle = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInTitle", 1)) > 0) ? true : false);
			FindItemInContents = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInContents", 1)) > 0) ? true : false);
			FindItemInSongNumber = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInSongNumber", 1)) > 0) ? true : false);
			FindItemInBookRef = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInBookRef", 1)) > 0) ? true : false);
			FindItemInUserRef = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInUserRef", 1)) > 0) ? true : false);
			FindItemInLicAdmin = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInLicAdmin", 1)) > 0) ? true : false);
			FindItemInWriter = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInWriter", 1)) > 0) ? true : false);
			FindItemInCopyright = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInCopyright", 1)) > 0) ? true : false);
			FindItemUseDates = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemUseDates", 0)) > 0) ? true : false);
			// Saved as yyyy-MM-dd; older versions saved DateTime.ToString() of the regional format (Korean or English).
			FindItemDateFrom = DataUtil.TryParseStoredDate(RegUtil.GetRegValue("options", "FindItemDateFrom", ""), out DateTime dateFrom)
				? dateFrom.Date
				: DateTime.Today.Subtract(TimeSpan.FromDays(91.0));
			FindItemDateTo = DataUtil.TryParseStoredDate(RegUtil.GetRegValue("options", "FindItemDateTo", ""), out DateTime dateTo)
				? dateTo.Date
				: DateTime.Today;
			OutlineFontSizeThreshold = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutlineFontSizeThreshold", 55));
			if ((OutlineFontSizeThreshold < 25) | (OutlineFontSizeThreshold > 100))
			{
				OutlineFontSizeThreshold = 55;
			}
			OUTPPPrefix = EasiSlidesTempDir + "~OUTPPPreview";
			PREPPPrefix = EasiSlidesTempDir + "~PREPPPreview";
			LoadFolderNamesArray();
			ComputeShowLineSpacing();
			LoadLicAdminDetails();

			SaveConfigSettings();
		}

		public static bool ValidSongID(int InSongID)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + InSongID;

				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

				if (datatable.Rows.Count > 0)
				{
					return (Folders[DataUtil.ObjToInt(datatable.Rows[0]["FolderNo"])].Use > 0) ? true : false;
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Rewrites SONG.LastModified values that do not start with yyyy-MM-dd (written by older versions through a
		/// DataRow in the regional format, e.g. "11/18/2026 12:00:00 AM") as yyyy-MM-dd, so that the date conditions
		/// (LastModifiedBetween) find them whatever format wrote them. Only rows in another form are read and changed;
		/// values that cannot be read are left as they are. Returns the number of rows changed.
		/// </summary>
		public static int NormalizeLastModified(string connectString)
		{
			int changed = 0;
			try
			{
				using DbConnection connection = DbController.GetDbConnection(connectString);
				var updates = new List<(long SongId, string Date)>();
				using (DbCommand select = new DbCommand("select SONGID, LastModified from SONG where LastModified is not null and LastModified <> '' "
					+ "and LastModified not glob '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]*'", connection))
				using (System.Data.SQLite.SQLiteDataReader reader = select.ExecuteReader())
				{
					while (reader.Read())
					{
						if (StoredDate.TryParse(reader.GetValue(1), out DateTime date))
						{
							updates.Add((reader.GetInt64(0), StoredDate.ToText(date)));
						}
					}
				}
				if (updates.Count == 0)
				{
					return 0;
				}
				using System.Data.SQLite.SQLiteTransaction transaction = connection.BeginTransaction();
				using DbCommand update = new DbCommand("update SONG set LastModified = @date where SONGID = @id", connection, transaction);
				System.Data.SQLite.SQLiteParameter dateParam = update.Parameters.Add("@date", DbType.String);
				System.Data.SQLite.SQLiteParameter idParam = update.Parameters.Add("@id", DbType.Int64);
				foreach ((long songId, string date) in updates)
				{
					dateParam.Value = date;
					idParam.Value = songId;
					changed += update.ExecuteNonQuery();
				}
				transaction.Commit();
				Trace.WriteLine($"NormalizeLastModified: {changed} date(s) rewritten as {StoredDate.Format}");
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"NormalizeLastModified ERROR : {ex.Message}");
			}
			return changed;
		}

		// ----- Scripture songs: database items whose text is a Bible passage, shown with the Bible icon -----

		// Lyrics that end with a scripture reference in brackets (half or full width): an optional book number,
		// a book name, chapter:verse (or the Korean chapter/verse words), an optional verse letter or verse word,
		// more verses, and an optional version tag: "(Acts 9:31)", "(1 Cor 13:4)", "(John 3:16a, 18)",
		// "(Romans 8:28 NIV)", and the same with Korean book names and version names.
		private static readonly Regex ScriptureReferenceAtEnd = new Regex(
			@"[\(（]\s*(?:[1-3]\s*)?\p{L}[\p{L}\p{M}\s.]{0,24}?\s*\d{1,3}\s*(?:[:：]\s*\d{1,3}|장\s*\d{1,3})\s*(?:절|[a-c])?"
			+ @"(?:\s*[-~,]\s*\d{1,3}(?:\s*[:：]\s*\d{1,3})?\s*(?:절|[a-c])?)*(?:\s+(?:[A-Z]{2,5}|[가-힣]{2,4}))?\s*[\)）]\s*$",
			RegexOptions.CultureInvariant);

		// SONGIDs of the scripture songs; null until first needed, after any song is added or saved, and after a
		// load that failed (so the next call tries again).
		private static HashSet<int> scriptureSongIds;

		public static bool LyricsEndWithScriptureReference(string lyrics)
		{
			return !string.IsNullOrEmpty(lyrics) && ScriptureReferenceAtEnd.IsMatch(lyrics);
		}

		/// <summary>
		/// True when the song's lyrics end with a scripture reference. The ids are read in one query the first
		/// time (UI thread) and kept until a song is added or saved (InvalidateScriptureSongs).
		/// </summary>
		public static bool IsScriptureSong(int songId)
		{
			if (scriptureSongIds == null && DateTime.UtcNow - scriptureLoadFailedAt > TimeSpan.FromSeconds(10))
			{
				scriptureSongIds = LoadScriptureSongIds();
				if (scriptureSongIds == null)
				{
					// Not every row of every paint retries: the next attempt waits 10 seconds.
					scriptureLoadFailedAt = DateTime.UtcNow;
				}
			}
			return scriptureSongIds != null && scriptureSongIds.Contains(songId);
		}

		private static DateTime scriptureLoadFailedAt = DateTime.MinValue;

		public static void InvalidateScriptureSongs()
		{
			scriptureSongIds = null;
			scriptureLoadFailedAt = DateTime.MinValue;
		}

		// Null when the database cannot be read (not cached: the next call tries again).
		private static HashSet<int> LoadScriptureSongIds()
		{
			var ids = new HashSet<int>();
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				if (connection == null)
				{
					return null;
				}
				// Only lyrics with a closing bracket (half or full width) can match; the expression decides.
				using DbCommand command = new DbCommand("select SONGID, Lyrics from SONG where Lyrics like '%)%' or Lyrics like '%）%'", connection);
				using System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					if (!reader.IsDBNull(1) && LyricsEndWithScriptureReference(reader.GetString(1)))
					{
						ids.Add(Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture));
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"LoadScriptureSongIds ERROR : {ex.Message}");
				return null;
			}
			return ids;
		}

		/// <summary>The quick find search: every text column of the folders in use.</summary>
		public static string BuildItemSearchString(string searchText)
		{
			return BuildItemSearchString(searchText, new ItemSearchOptions(), folderList: null);
		}

		/// <summary>
		/// Builds the SONG query of the item search. A song is found when any chosen text column matches
		/// and every chosen filter (folders, notations, key, timing, dates) holds. Also marks the searched
		/// folders in Folders[i].FindSongs. folderList null means every folder in use.
		/// </summary>
		public static string BuildItemSearchString(string searchText, ItemSearchOptions options, CheckedListBox folderList)
		{
			string pattern = SqlText(LikePattern(searchText));

			var textMatches = new List<string>();
			void AddLike(bool enabled, params string[] columns)
			{
				if (!enabled)
				{
					return;
				}
				foreach (string column in columns)
				{
					textMatches.Add($"lower({column}) like {pattern}");
				}
			}
			AddLike(options.InTitle, "Title_1", "Title_2");
			AddLike(options.InContents, "lyrics");
			AddLike(options.InBookRef, "BOOK_REFERENCE");
			AddLike(options.InUserRef, "USER_REFERENCE");
			AddLike(options.InLicAdmin, "licence_admin1", "licence_admin2");
			AddLike(options.InWriter, "writer");
			AddLike(options.InCopyright, "copyright");
			if (options.InSongNumber && SongNumberCondition(searchText) is string numberMatch)
			{
				textMatches.Add(numberMatch);
			}

			bool anyTextColumn = options.InTitle || options.InContents || options.InSongNumber || options.InBookRef
				|| options.InUserRef || options.InLicAdmin || options.InWriter || options.InCopyright;
			var conditions = new List<string>();
			if (textMatches.Count > 0)
			{
				conditions.Add("(" + string.Join(" or ", textMatches) + ")");
			}
			else if (anyTextColumn || !options.NotationsOnly)
			{
				// Chosen columns that cannot match (song number only, text not a number), or nothing to search in.
				// Only "no text column + notations only" lists songs by the filters alone.
				conditions.Add("1 = 0");
			}
			string folderFilter = BuildFolderFilter(folderList);
			if (folderFilter != "")
			{
				conditions.Add(folderFilter);
			}
			if (options.NotationsOnly)
			{
				conditions.Add("msc <> ''");
			}
			if (!string.IsNullOrEmpty(options.Key))
			{
				conditions.Add("key = " + SqlText(options.Key));
			}
			if (!string.IsNullOrEmpty(options.Timing))
			{
				conditions.Add("timing = " + SqlText(options.Timing));
			}
			if (options.UseDates)
			{
				conditions.Add(LastModifiedBetween(options.DateFrom, options.DateTo));
			}

			return "select * from SONG where " + string.Join(" and ", conditions);
		}

		/// <summary>
		/// SQL condition: SONG.LastModified between two dates (inclusive), comparing the first 10 characters, which are
		/// yyyy-MM-dd (StoredDate.Format) for every row once NormalizeLastModified has run; date() would not do, as it
		/// cannot read a time part with the Korean AM/PM designator that older versions wrote.
		/// </summary>
		public static string LastModifiedBetween(DateTime from, DateTime to)
		{
			return $"substr(LastModified, 1, 10) between '{DataUtil.ToStoredDate(from)}' and '{DataUtil.ToStoredDate(to)}'";
		}

		// "%text%": the text matches anywhere; a '%' typed by the user stays a wildcard.
		private static string LikePattern(string text)
		{
			if (!text.StartsWith('%'))
			{
				text = "%" + text;
			}
			if (!text.EndsWith('%'))
			{
				text += "%";
			}
			return text.ToLower();
		}

		// A string literal for SQL: single quotes, with quotes inside doubled.
		private static string SqlText(string value)
		{
			return "'" + value.Replace("'", "''") + "'";
		}

		// A positive number finds that song number; any other text (also "0") adds no song number condition.
		private static string SongNumberCondition(string text)
		{
			if (!int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int number) || number <= 0)
			{
				return null;
			}
			return $"song_number = {number}";
		}

		// Marks the folders to search (Folders[i].FindSongs, read back by the Find dialog and the folder list)
		// and returns "FolderNo in (...)", or "" when there is none.
		private static string BuildFolderFilter(CheckedListBox folderList)
		{
			var numbers = new List<int>();
			if (folderList != null)
			{
				foreach (object item in folderList.CheckedItems)
				{
					numbers.Add(GetFolderNumber(item.ToString()));
				}
			}
			else
			{
				for (int i = 1; i < MAXSONGSFOLDERS; i++)
				{
					if (Folders[i].Use > 0)
					{
						numbers.Add(i);
					}
				}
			}
			for (int i = 1; i < MAXSONGSFOLDERS; i++)
			{
				Folders[i].FindSongs = numbers.Contains(i);
			}
			return (numbers.Count == 0) ? "" : "FolderNo in (" + string.Join(", ", numbers) + ")";
		}
	}
}

