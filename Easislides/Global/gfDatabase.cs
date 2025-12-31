using Easislides.SQLite;
using Easislides.Util;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Easislides.Module;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbCommand = System.Data.SQLite.SQLiteCommand;


namespace Easislides
{
	internal unsafe partial class gf
	{

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
				catch
				{
				}
			}
			else if (regValue != "")
			{
				RegUtil.SaveRegValue("config", "media_dir", regValue);
			}
			RegUtil.DeleletRegKey("config", name);
			string text = RootEasiSlidesDir + "Admin\\Database\\MusicExtensions.txt";
			string text2 = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
			if (File.Exists(text) && !File.Exists(text2))
			{
				try
				{
					File.Move(text, text2);
				}
				catch
				{
				}
			}
			string text3 = RootEasiSlidesDir + "User Images";
			string text4 = RootEasiSlidesDir + "Images\\";
			if (Directory.Exists(text3) && !Directory.Exists(text4))
			{
				try
				{
					Directory.Move(text3, text4);
				}
				catch
				{
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
			Title_1 = DataUtil.Left(Title_1, 100);
			Title_2 = DataUtil.Left(Title_2, 100);
			Sequence = DataUtil.Left(Sequence, 100);
			writer = DataUtil.Left(writer, 100);
			copyright = DataUtil.Left(copyright, 100);
			Timing = DataUtil.Left(Timing, 50);
			InKey = DataUtil.Left(InKey, 20);
			LICENCE_ADMIN1 = DataUtil.Left(LICENCE_ADMIN1, 50);
			LICENCE_ADMIN2 = DataUtil.Left(LICENCE_ADMIN2, 50);
			BOOK_REFERENCE = DataUtil.Left(BOOK_REFERENCE, 50);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);



			(DbDataAdapter sQLiteDataAdapter, DataTable dt) = DbController.GetDataAdapter(InConnectString, "Select * from SONG");

			if (dt != null)
			{
				try
				{
					dt.AcceptChanges();

					DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
					sQLiteDataAdapter.InsertCommand = sqCB.GetInsertCommand();

					DataRow dr = dt.NewRow();

					dr["Title_1"] = Title_1;
					dr["Title_2"] = Title_2;
					dr["song_number"] = song_number;
					dr["FolderNo"] = FolderNo;
					dr["Lyrics"] = Lyrics;
					dr["Sequence"] = Sequence;
					dr["writer"] = writer;
					dr["copyright"] = copyright;
					dr["CJK_WordCount"] = value;
					dr["CJK_StrokeCount"] = value2;
					dr["capo"] = capo;
					dr["Timing"] = Timing;
					dr["Key"] = InKey;
					dr["msc"] = msc;
					dr["CATEGORY"] = CATEGORY;
					dr["LICENCE_ADMIN1"] = LICENCE_ADMIN1;
					dr["LICENCE_ADMIN2"] = LICENCE_ADMIN2;
					dr["BOOK_REFERENCE"] = BOOK_REFERENCE;
					dr["USER_REFERENCE"] = USER_REFERENCE;
					dr["SETTINGS"] = SETTINGS;
					dr["FORMATDATA"] = FORMATDATA;
					dr["LastModified"] = DateTime.Now.Date;

					dt.Rows.Add(dr);

					sQLiteDataAdapter.Update(dt);  // Its also update in database.	

					return DataUtil.ObjToInt(dr["SongID"]);
				}
				catch
				{
					MessageBox.Show("Error encountered whilst adding item to database - item NOT added");
				}
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
			Title_1 = DataUtil.Left(Title_1, 100);
			Title_2 = DataUtil.Left(Title_2, 100);
			Sequence = DataUtil.Left(Sequence, 100);
			writer = DataUtil.Left(writer, 100);
			copyright = DataUtil.Left(copyright, 100);
			Timing = DataUtil.Left(Timing, 50);
			InKey = DataUtil.Left(InKey, 20);
			LICENCE_ADMIN1 = DataUtil.Left(LICENCE_ADMIN1, 50);
			LICENCE_ADMIN2 = DataUtil.Left(LICENCE_ADMIN2, 50);
			BOOK_REFERENCE = DataUtil.Left(BOOK_REFERENCE, 50);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);
			try
			{

				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				string text = "Update SONG SET Title_1 =@Title_1,Title_2 =@Title_2,song_number =@song_number,FolderNo =@FolderNo,Lyrics =@Lyrics,Sequence =@Sequence,writer =@writer,copyright =@copyright,CJK_WordCount =@CJK_WordCount,CJK_StrokeCount =@CJK_StrokeCount,capo =@capo,Timing =@Timing,[Key] =@Key,msc =@msc,CATEGORY =@CATEGORY,LICENCE_ADMIN1 =@LICENCE_ADMIN1,LICENCE_ADMIN2 =@LICENCE_ADMIN2,BOOK_REFERENCE =@BOOK_REFERENCE,USER_REFERENCE =@USER_REFERENCE,SETTINGS =@SETTINGS,FORMATDATA =@FORMATDATA,LastModified =@LastModified where songid=" + SongID;
				using DbCommand command = new DbCommand(text, connection);
				command.CommandText = text;
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
				command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
				command.ExecuteNonQuery();
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
			string text = $@"{Application.StartupPath}Sys\EasiSlidesDb.db";

			RestoreSongsDatabase = false;
			if (File.Exists(text))
			{
				try
				{
					FileUtil.MakeDir(RootEasiSlidesDir + "Admin\\Database\\");
					int num = 1;
					string dBFileName = DBFileName;
					if (File.Exists(dBFileName))
					{
						dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{num}{Path.GetExtension("EasiSlidesDb.db")}";
						while (File.Exists(dBFileName))
						{
							num++;
							dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{num}{Path.GetExtension("EasiSlidesDb.db")}";
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
						File.Copy(text, DBFileName);
						return dBFileName;
					}
					catch
					{
					}
					MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please make sure the existing database is not in use and then try again.");
					return "-1";
				}
				catch
				{
					MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please make sure the existing database is not in use and then try again.");
					return "-1";
				}
			}
			MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please re-install EasiSlides Software.");
			return "-1";
		}

		public static void RestoreBackgroundImages()
		{
			string text = $@"{Application.StartupPath}\Backgrounds\Scenery\";
			string text2 = $@"{Application.StartupPath}\Backgrounds\Tiles\";
			string text3 = $@"{RootEasiSlidesDir}Images\Scenery\";
			string text4 = $@"{RootEasiSlidesDir}Images\Tiles\";
			if (FileUtil.MakeDir($@"{RootEasiSlidesDir}Images\"))
			{
				if (Directory.Exists(text) && FileUtil.MakeDir(text3))
				{
					FileUtil.CopyFiles(text, text3);
				}
				if (Directory.Exists(text2) && FileUtil.MakeDir(text4))
				{
					FileUtil.CopyFiles(text2, text4);
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
				using (DbConnection connection = DbController.GetDbConnection(InConnectString))
				{
					DbCommand command = new DbCommand("Delete * from Folder ", connection);
					command.ExecuteNonQuery();
				}
				return true;
			}
			catch
			{
			}
			return false;
		}

		public static void ResetFolder(int FNumber, string InFolderName, string InConnectString)
		{

			using DbConnection connection = DbController.GetDbConnection(InConnectString);

			try
			{
				bool flag = false;
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
							flag = ((FNumber < 4) ? true : false);
						}
						else
						{
							value = "Recycle Folder";
							flag = false;
						}
					}
					else
					{
						value = InFolderName;
						flag = true;
					}
					cmdText = (command.CommandText = "Insert into Folder (FolderNo,name,Use,GroupStyle,PreChorusHeading,ChorusHeading, BridgeHeading,EndingHeading,BIUHeading,HeadingSize,HeadingOption,BIU0,Size0,Bold0,Align0,FontName0,Vpos0,BIU1,Size1,Bold1,Align1,FontName1,Vpos1)  Values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
					command.Parameters.AddWithValue("@FolderNo", FNumber);
					command.Parameters.AddWithValue("@Name", value);
					command.Parameters.AddWithValue("@Use", flag);
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
			catch
			{
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
		}

		public static async Task ResetFolder1(int FNumber, string InFolderName, string InConnectString)
		{
			var task = Task.Run(() =>
			{

				using DbConnection connection = DbController.GetDbConnection(InConnectString);

				try
				{
					bool flag = false;
					string cmdText = $@"select count(*) from FOLDER where FolderNo={DataUtil.ObjToString(FNumber)}";
					connection.Open();

					DbCommand command = new DbCommand(cmdText, connection);
					if ((int)command.ExecuteScalar() == 0)
					{
						string value;
						if (InFolderName == "")
						{
							if (FNumber > 0)
							{
								value = $@"Folder {DataUtil.ObjToString(FNumber)}";
								flag = ((FNumber < 4) ? true : false);
							}
							else
							{
								value = "Recycle Folder";
								flag = false;
							}
						}
						else
						{
							value = InFolderName;
							flag = true;
						}
						cmdText = (command.CommandText = "Insert into Folder (FolderNo,name,Use,GroupStyle,PreChorusHeading,ChorusHeading, BridgeHeading,EndingHeading,BIUHeading,HeadingSize,HeadingOption,BIU0,Size0,Bold0,Align0,FontName0,Vpos0,BIU1,Size1,Bold1,Align1,FontName1,Vpos1)  Values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
						command.Parameters.AddWithValue("@FolderNo", FNumber);
						command.Parameters.AddWithValue("@Name", value);
						command.Parameters.AddWithValue("@Use", flag);
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
				catch
				{
				}
				finally
				{
					if (connection.State == ConnectionState.Open)
					{
						connection.Close();
					}
				}
			});
		}

		public static void LoadSavedData()
		{
			for (int i = 0; i < 41; i++)
			{
				FindSongsFolder[i] = true;
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
			string text = RegUtil.GetRegValue("config", "media_dir", MediaDir);
			if (DataUtil.Right(text, 1) != "\\")
			{
				text += "\\";
			}
			if (Directory.Exists(text))
			{
				MediaDir = text;
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
			WordWrapLeftAlignIndent = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapLeftAlignIndent", 1)) > 0) ? true : false);
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
			int num = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NotationFontFactor", 75));
			if (num < 20 && num > 200)
			{
				num = 75;
			}
			NotationFontFactor = (float)num / 100f;
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

			//daniel
			//Global Keyboard Hook 가?�오�?
			GlobalHookKey_F7 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F7", 0)) > 0) ? true : false);
			GlobalHookKey_F8 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F8", 0)) > 0) ? true : false);

			//daniel
			//Global Keyboard Hook 가?�오�?
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

			//daniel
			//OutputMonitorNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutputmonitorNumber", 1));
			//LyricsMonitorNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorNumber", 0));

			OutputMonitorName = RegUtil.GetRegValue("options", "OutputmonitorName", "None");
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
			UpdateV4RegDM();
			DMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "AlwaysTryDualMonitor", 1)) > 0) ? true : false);
			//daniel
			//?�크�?Mode
			isScreenWideMode = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "IsMonitorWide", 0)) > 0) ? true : false);

			DualMonitorSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOption", 0));
			if ((DualMonitorSelectAutoOption < 0) | (DualMonitorSelectAutoOption > 1))
			{
				/// 모니???�택 ?�션
				/// 0??경우 ?�용?��? 리스?�에???�택?�여 ?�동?�로 PPT ?�치�??�는??
				/// 1??경우 ?�용?��? 커스?�?�로 모니???�치�??�동?�로 ?�력?�다.
				DualMonitorSelectAutoOption = 0;
			}
			DMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomLeft", 0));
			if ((DMOption1Left < -9999) | (DMOption1Left > 9999))
			{
				DMOption1Left = 0;
			}
			DMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomTop", 0));
			if ((DMOption1Top < -9999) | (DMOption1Top > 9999))
			{
				DMOption1Top = 0;
			}
			DMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomWidth", 1));
			if ((DMOption1Width < 1) | (DMOption1Width > 9999))
			{
				DMOption1Width = 100;
			}

			//if (!gf.isScreenWideMode)
			//	DMOption1Height = DMOption1Width * 3 / 4;
			//else
			//	DMOption1Height = DMOption1Height;

			DMOption1Height = DMOption1Width * 3 / 4;
			if (DMOption1Height < 1)
			{
				DMOption1Height = 1;
			}
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

			if (!gf.isScreenWideMode)
				LMOption1Height = LMOption1Width * 3 / 4;
			else
				LMOption1Height = LMOption1Height;

			if (LMOption1Height < 1)
			{
				LMOption1Height = 1;
			}
			LMTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorTextColour", LMTextColour.ToArgb())));
			LMHighlightColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorHighlightColour", LMHighlightColour.ToArgb())));
			LMBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorBackColour", LMBackColour.ToArgb())));
			LMShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorShowNotations", 1)) > 0) ? true : false);
			LMMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorFontSize", 22));
			if ((LMMainFontSize < 8) | (LMMainFontSize > 40))
			{
				LMMainFontSize = 20;
			}
			LMNotationsFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorNotationsFontSize", 22));
			if ((LMNotationsFontSize < 8) | (LMNotationsFontSize > 40))
			{
				LMNotationsFontSize = 20;
			}
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
			string text2 = "";
			TotalMediaFileExt = 0;
			MediaExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
			AudioExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\AudioExtensions.txt";
			VideoExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\VideoExtensions.txt";
			LoadMusicExtArray();
			JumpToA = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToA", 1));
			if ((JumpToA < 1) | (JumpToA > 41))
			{
				JumpToA = 1;
			}
			JumpToB = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToB", 2));
			if ((JumpToB < 1) | (JumpToB > 41))
			{
				JumpToB = 2;
			}
			JumpToC = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToC", 3));
			if ((JumpToC < 1) | (JumpToC > 41))
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
			DateTime findItemDateFrom = DateTime.Now.Subtract(TimeSpan.FromDays(91.0));
			string s = DataUtil.ObjToString(RegUtil.GetRegValue("options", "FindItemDateFrom", findItemDateFrom.ToString()));
			try
			{
				FindItemDateFrom = DateTime.Parse(s);
			}
			catch
			{
				FindItemDateFrom = findItemDateFrom;
			}
			s = DataUtil.ObjToString(RegUtil.GetRegValue("options", "FindItemDateTo", DateTime.Now.ToString()));
			try
			{
				FindItemDateTo = DateTime.Parse(s);
			}
			catch
			{
				FindItemDateTo = DateTime.Now;
			}
			OutlineFontSizeThreshold = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutlineFontSizeThreshold", 55));
			if ((OutlineFontSizeThreshold < 25) | (OutlineFontSizeThreshold > 100))
			{
				OutlineFontSizeThreshold = 55;
			}
			OUTPPPrefix = EasiSlidesTempDir + "~OUTPPPreview";
			PREPPPrefix = EasiSlidesTempDir + "~PREPPPreview";
			ExtPPrefix = EasiSlidesTempDir + "ExtPPPreview";
			LoadFolderNamesArray();
			ComputeShowLineSpacing();
			LoadLicAdminDetails();

			SaveConfigSettings();
		}

	}
}
