using System;
using System.Data;
using System.Drawing;
using System.IO;
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

		public static bool InitEasiSlidesDir()
		{
			RootEasiSlidesDir = RegUtil.GetRegValue("config", "root_directory", "C:\\EasiSlides\\");
			if (RootEasiSlidesDir == "")
			{
				RootEasiSlidesDir = "C:\\EasiSlides\\";
			}
			string regValue = RegUtil.GetRegValue("config", "version", "none");
			string regValue2 = RegUtil.GetRegValue("config", "database", "none");
			if (regValue != "4.0.5")
			{
				RegUtil.SaveRegValue("config", "version", "4.0.5");
			}
			if (regValue2 != "4.0B")
			{
				ApplicationFirstRun = true;
				RegUtil.SaveRegValue("config", "database", "4.0B");
			}
			if (!ValidateRootFolder())
			{
				return false;
			}
			RegUtil.SaveRegValue("config", "root_directory", RootEasiSlidesDir);
			ValidateID();

			DBFileName = RootEasiSlidesDir + "Admin\\Database\\EasiSlidesDb.db";

			bool flag = File.Exists(DBFileName);

			if (ApplicationFirstRun || !flag)
			{
				CovertItemsTov4();
				if (!RestoreSongsDatabase && ApplicationFirstRun)
				{
					SplashScreenBack = true;
					if (MessageBox.Show("Would you like to Install the EasiSlides Lyrics Database supplied with EasiSlides 4.0.5? Your existing Lyrics Database, if any, will be renamed and retained for backup.  Click Yes to Install, or No to continue using existing database.", "New EasiSlides Version 4.0.5 ... Replace existing Database with Supplied Database?", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RestoreSongsDatabase = true;
					}
				}
				else if (!RestoreSongsDatabase && !flag)
				{
					SplashScreenBack = true;
					if (MessageBox.Show("Cannot find the Lyrics Database.  Would you like to Install the EasiSlides Lyrics Database supplied with EasiSlides 4.0.5?  Click Yes to Install, or No to create a new blank database.", "Loading EasiSlides 4.0.5 ... Install Supplied Database?", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RestoreSongsDatabase = true;
					}
				}
			}
			if (RestoreSongsDatabase)
			{
				string text = RestoreOriginalSongsDatabase();
				if (text != "-1")
				{
					if (ApplicationFirstRun)
					{
						if (text != "")
						{
							MessageBox.Show("Lyrics Database installed successfully. " + ((text != "") ? ("Existing database has been renamed to: " + text) : ""));
						}
					}
					else
					{
						MessageBox.Show("Lyrics Database installed successfully. " + ((text != "") ? ("Existing database has been renamed to: " + text) : ""));
					}
				}
			}
			SplashScreenFront = true;
			return true;
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
			MusicSymLen = " <#>".Length;
			MaxBackgroundStyleIndex = BackPattern.MaxStyleIndex;
			ShowScreenColour[0] = DefaultBackColour;
			ShowScreenColour[1] = DefaultBackColour;
			ShowScreenStyle = 0;
			ShowFontColour[0] = DefaultBackColour;
			ShowFontColour[1] = DefaultBackColour;
			//for (int i = 1; i < 160; i++)
			//{
			//	VerseTitle[i] = "";
			//	SequenceSymbol[i] = "";
			//}
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
			SymbolsString = "";
			for (int i = 1; i <= 99; i++)
			{
				VerseSymbol[i] = "[" + VerseTitle[i] + "]";
				SequenceSymbol[i] = VerseTitle[i];
				SymbolsString = SymbolsString + VerseSymbol[i] + ",";
			}
			//for (int i = 100; i < 160; i++)
			//{
			//	VerseSymbol[i] = "";
			//	SequenceSymbol[i] = "";
			//}
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
			SequenceSymbol[0] = "c";
			SequenceSymbol[100] = "b";
			SequenceSymbol[103] = "w";
			SequenceSymbol[101] = "e";
			SequenceSymbol[102] = "t";
			SequenceSymbol[111] = "p";
			SequenceSymbol[112] = "q";
			string symbolsString = SymbolsString;
			SymbolsString = symbolsString + VerseSymbol[0] + "," + VerseSymbol[102] + "," + VerseSymbol[100] + "," + VerseSymbol[103] + "," + VerseSymbol[111] + "," + VerseSymbol[112] + "," + VerseSymbol[101] + "," + VerseSymbol[150];
			xArray = SymbolsString.Split(',');
			ShowLMargin = Screen.PrimaryScreen.Bounds.Width / 50;
			ShowRMargin = ShowLMargin;
			ShowLyricsWidth = Screen.PrimaryScreen.Bounds.Width - ShowLMargin - ShowRMargin;
			UsageFileName = RootEasiSlidesDir + "Admin\\Database\\EsUsage.db";
			BiblesListFileName = RootEasiSlidesDir + "Admin\\Database\\EsBiblesList.db";
			tempDBFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEasiSlidesDb.db";
			tempUsageFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEsUsage.db";
			tempBiblesListFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEsBiblesList.db";
			ConnectStringMainDB = ConnectStringDef + DBFileName;
			ConnectStringUsageDB = ConnectStringDef + UsageFileName;
			ConnectStringBibleDB = ConnectStringDef + BiblesListFileName;
			UserString = DataUtil.Trim(RegUtil.GetRegValue("config", "RegistrationUser", ""));

			if (!ValidateVer_3_4_Fields())
			{
				return false;
			}

			LoadSavedData();

			GenerateMusicKeysList();
			DisplayInfo.SizeLaunchDisplay();
			ResetShowRunningSettings();

			AlertsDataFile = RootEasiSlidesDir + "Admin\\Database\\Alerts.txt";
			ParentalDataFile = RootEasiSlidesDir + "Admin\\Database\\Parental.txt";
			string startupPath = Application.StartupPath;
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
			catch
			{
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

		public static bool ValidateVer_3_4_Fields()
		{
			if (!ValidateDB(DatabaseType.Songs))
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			bool flag10 = false;
			bool flag11 = false;
			bool flag12 = false;
			bool flag13 = false;
			bool flag14 = false;
			bool flag15 = false;
			bool flag16 = false;
			bool flag17 = false;
			bool flag18 = false;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag19 = false;
			bool flag20 = false;
			bool flag21 = false;
			bool flag22 = false;

			DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
			try
			{

				using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
				{
					null,
					null,
					"Folder",
					null
				});

				foreach (DataRow row in dbSchemaTable.Rows)
				{
					string a = DataUtil.ObjToString(row["COLUMN_NAME"]).ToUpper();
					if (a != "")
					{
						if (a == "BIU0".ToUpper())
						{
							flag = true;
						}
						if (a == "BIU1".ToUpper())
						{
							flag2 = true;
						}
						if (a == "ColA".ToUpper())
						{
							flag3 = true;
						}
						if (a == "ColB".ToUpper())
						{
							flag4 = true;
						}
						if (a == "LMargin".ToUpper())
						{
							flag6 = true;
						}
						if (a == "RMargin".ToUpper())
						{
							flag7 = true;
						}
						if (a == "BMargin".ToUpper())
						{
							flag8 = true;
						}
						if (a == "BIUHeading".ToUpper())
						{
							flag9 = true;
						}
						if (a == "HeadingSize".ToUpper())
						{
							flag10 = true;
						}
						if (a == "HeadingOption".ToUpper())
						{
							flag11 = true;
						}
						if (a == "LineSpacing".ToUpper())
						{
							flag12 = true;
						}
						if (a == "LineSpacing2".ToUpper())
						{
							flag13 = true;
						}
						if (a == "PreChorusHeading".ToUpper())
						{
							flag5 = true;
						}
					}
				}

				if (!flag)
				{
					DbController.CreateField(ref connection, "Folder", "BIU0", 1);
				}
				if (!flag2)
				{
					DbController.CreateField(ref connection, "Folder", "BIU1", 1);
				}
				if (!flag3)
				{
					DbController.CreateField(ref connection, "Folder", "ColA", 0);
				}
				if (!flag4)
				{
					DbController.CreateField(ref connection, "Folder", "ColB", 0);
				}
				if (!flag5)
				{
					DbController.CreateField(ref connection, "Folder", "PreChorusHeading", 0, 30);
				}
				if (!flag6)
				{
					DbController.CreateField(ref connection, "Folder", "LMargin", 1);
				}
				if (!flag7)
				{
					DbController.CreateField(ref connection, "Folder", "RMargin", 1);
				}
				if (!flag8)
				{
					DbController.CreateField(ref connection, "Folder", "BMargin", 1);
				}
				if (!flag9)
				{
					DbController.CreateField(ref connection, "Folder", "BIUHeading", 1);
				}
				if (!flag10)
				{
					DbController.CreateField(ref connection, "Folder", "HeadingSize", 1);
				}
				if (!flag11)
				{
					DbController.CreateField(ref connection, "Folder", "HeadingOption", 1);
				}
				if (!flag12)
				{
					DbController.CreateField(ref connection, "Folder", "LineSpacing", 4);
				}
				if (!flag13)
				{
					DbController.CreateField(ref connection, "Folder", "LineSpacing2", 4);
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

			try
			{
				connection.Open();

				using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
				{
					null,
					null,
					"Song",
					null
				});

				foreach (DataRow row2 in dbSchemaTable.Rows)
				{
					string a = DataUtil.ObjToString(row2["COLUMN_NAME"]).ToUpper();
					if (a != "")
					{
						if (a == "CAPO".ToUpper())
						{
							flag14 = true;
						}
						if (a == "TIMING".ToUpper())
						{
							flag15 = true;
						}
						if (a == "SONG_NUMBER".ToUpper())
						{
							flag16 = true;
						}
						if (a == "BOOK_REFERENCE".ToUpper())
						{
							flag17 = true;
							num2 = DataUtil.ObjToInt(row2["CHARACTER_MAXIMUM_LENGTH"]);
						}
						if (a == "USER_REFERENCE".ToUpper())
						{
							flag18 = true;
							num = DataUtil.ObjToInt(row2["CHARACTER_MAXIMUM_LENGTH"]);
						}
						if (a == "LICENCE_ADMIN1".ToUpper())
						{
							flag19 = true;
						}
						if (a == "LICENCE_ADMIN2".ToUpper())
						{
							flag20 = true;
						}
						if (a == "SETTINGS".ToUpper())
						{
							flag21 = true;
						}
						if (a == "SEQUENCE".ToUpper())
						{
							num3 = DataUtil.ObjToInt(row2["CHARACTER_MAXIMUM_LENGTH"]);
						}
						if (a == "FORMATDATA".ToUpper())
						{
							flag22 = true;
						}
					}
				}

				if (num2 > 1 && num2 < 100)
				{
					try
					{

						DbCommand command = new DbCommand("ALTER TABLE Song ALTER COLUMN BOOK_REFERENCE TEXT (100) ", connection);

						command.ExecuteNonQuery();
					}
					catch { }
				}
				if (num3 > 1 && num3 < 255)
				{
					try
					{
						DbCommand command = new DbCommand("ALTER TABLE Song MODIFY SEQUENCE varchar(255)", connection);
						command.ExecuteNonQuery();
					}
					catch { }
				}
				if (!flag14)
				{
					DbController.CreateField(ref connection, "Song", "CAPO", 1);
				}
				if (!flag15)
				{
					DbController.CreateField(ref connection, "Song", "TIMING", 0);
				}
				if (!flag16)
				{
					DbController.CreateField(ref connection, "Song", "SONG_NUMBER", 1);
				}
				if (!flag17)
				{
					DbController.CreateField(ref connection, "Song", "BOOK_REFERENCE", 0);
				}
				if (!flag18)
				{
					DbController.CreateField(ref connection, "Song", "USER_REFERENCE", 5);
				}
				else if (num > 0)
				{
					try
					{
						DbCommand command = new DbCommand("ALTER TABLE Song MODIFY USER_REFERENCE varchar(255)", connection);
						command.ExecuteNonQuery();
					}
					catch { }
				}

				if (!flag19)
				{
					DbController.CreateField(ref connection, "Song", "LICENCE_ADMIN1", 0);
				}
				if (!flag20)
				{
					DbController.CreateField(ref connection, "Song", "LICENCE_ADMIN2", 0);
				}
				if (!flag21)
				{
					DbController.CreateField(ref connection, "Song", "SETTINGS", 5);
				}
				if (!flag22)
				{
					DbController.CreateField(ref connection, "Song", "FORMATDATA", 5);
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

			return true;
		}

		public static bool ValidateMusicExt(ref string InExtension, bool ShowMessage)
		{
			if (!ValidateDirNameFormat(InExtension, ShowMessage ? "Music File Extension" : ""))
			{
				return false;
			}
			InExtension = DataUtil.Trim(InExtension);
			string text = "";
			for (int i = 0; i < InExtension.Length; i++)
			{
				if (InExtension[i] != '.')
				{
					text += DataUtil.Mid(InExtension, i, 1);
				}
			}
			if (text[0] != '.')
			{
				text = "." + text;
			}
			InExtension = text;
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

		public static void InitialiseIndividualData(ref SongSettings InItem)
		{
			InitialiseIndividualData(ref InItem, GapMedia.None, "");
		}

		public static void InitialiseIndividualData(ref SongSettings InItem, GapMedia InGapMedia, string InType)
		{
			InItem.ItemID = "";
			InItem.PrevItemPP = ((InItem.Type == "P") ? true : false);
			InItem.Type = "";
			InItem.SongNumber = 0;
			InItem.FolderNo = 0;
			InItem.CompleteLyrics = "";
			InItem.SongSequence = "";
			InItem.SongBasicSequence = "";
			InItem.SongOriginalLoadedSequence = "";
			InItem.Writer = "";
			InItem.Copyright = "";
			InItem.Capo = -1;
			InItem.Timing = "";
			InItem.MusicKey = "";
			InItem.OriginalNotations = "";
			InItem.Notations = "";
			InItem.Category = "";
			InItem.Show_LicAdminInfo1 = "";
			InItem.Show_LicAdminInfo2 = "";
			InItem.In_LicAdminInfo1 = "";
			InItem.In_LicAdminInfo2 = "";
			InItem.Book_Reference = "";
			InItem.User_Reference = "";
			InItem.HBR2_FolderNo = 0;
			InItem.HBR2_FontSizeFactor = 100;
			InItem.FontSizeFactor = 100;
			InItem.CurSlide = 0;
			InItem.TotalSlides = 0;
			InItem.Path = "";
			InItem.RotateString = "";
			InItem.RotateStyle = 1;
			InItem.RotateGap = 0;
			InItem.RotateTotal = 0;
			InItem.RotateTimings = "";
			InItem.RotateSequence = "";
			InItem.Format.ImageString = "";
			InItem.Format.TempImageFileName = "";
			InItem.FirstShowing = true;
			InItem.FolderName = "";
			InItem.PrevTitle = "";
			InItem.NextTitle = "";
			InItem.Format.FormatString = "";
			InItem.Format.DBStoredFormat = "";
			if (InGapMedia == GapMedia.SessionMedia && MediaOption == 1)
			{
				InGapMedia = GapMedia.None;
			}
			switch (InGapMedia)
			{
				case GapMedia.SameAsPrevious:
					InItem.Format.MediaOption = (InItem.UseDefaultFormat ? MediaOption : InItem.Format.MediaOption);
					InItem.Format.MediaLocation = (InItem.UseDefaultFormat ? MediaLocation : InItem.Format.MediaLocation);
					InItem.Format.MediaVolume = (InItem.UseDefaultFormat ? MediaVolume : InItem.Format.MediaVolume);
					InItem.Format.MediaBalance = (InItem.UseDefaultFormat ? MediaBalance : InItem.Format.MediaBalance);
					InItem.Format.MediaCaptureDeviceNumber = (InItem.UseDefaultFormat ? MediaCaptureDeviceNumber : InItem.Format.MediaCaptureDeviceNumber);
					break;
				case GapMedia.SessionMedia:
					InItem.Format.MediaOption = MediaOption;
					InItem.Format.MediaLocation = MediaLocation;
					InItem.Format.MediaVolume = MediaVolume;
					InItem.Format.MediaBalance = MediaBalance;
					InItem.Format.MediaCaptureDeviceNumber = MediaCaptureDeviceNumber;
					break;
				default:
					InItem.Title = "";
					InItem.Title2 = "";
					InItem.Format.MediaOption = 0;
					InItem.Format.MediaLocation = "";
					InItem.Format.MediaVolume = 0;
					InItem.Format.MediaBalance = 0;
					InItem.Format.MediaCaptureDeviceNumber = 0;
					break;
			}
			InItem.UseDefaultFormat = true;
		}

		public static void ValidateSequence(ref SongSettings InItem)
		{
			if (InItem.SongSequence == null)
			{
				InItem.SongSequence = "";
			}
			string text = "";
			for (int i = 0; i < InItem.SongSequence.Length; i++)
			{
				if (InItem.VersePresent[InItem.SongSequence[i]])
				{
					text += DataUtil.Mid(InItem.SongSequence, i, 1);
				}
			}
			if (text.Length > 0)
			{
				InItem.SongSequence = text;
			}
			else
			{
				InItem.SongSequence = InItem.SongBasicSequence;
			}
		}

		public static void ValidateMainHistoryItems()
		{
			int num = 0;
			if ((TotalMainEditHistory < 0) | (TotalMainEditHistory > AbsoluteMaxHitoryItems))
			{
				TotalMainEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= TotalMainEditHistory; i++)
			{
				if (GetItemTitle(MainEditHistoryList[i, 0]) != "")
				{
					num++;
					MainEditHistoryList[num, 0] = MainEditHistoryList[i, 0];
				}
			}
			TotalMainEditHistory = num;
			RemoveDuplicateEditorHistoryItems(ref MainEditHistoryList, ref TotalMainEditHistory);
		}

		public static void ValidateEditorHistoryItems()
		{
			int num = 0;
			for (int i = 1; i <= TotalEditorEditHistory; i++)
			{
				if (GetItemTitle(EditorEditHistoryList[i, 0]) != "")
				{
					num++;
					EditorEditHistoryList[num, 0] = EditorEditHistoryList[i, 0];
				}
			}
			TotalEditorEditHistory = num;
			RemoveDuplicateEditorHistoryItems(ref EditorEditHistoryList, ref TotalEditorEditHistory);
		}

		public static void ValidateInfoScreenHistoryItems()
		{
			int num = 0;
			for (int i = 1; i <= TotalInfoScreenEditHistory; i++)
			{
				if (GetItemTitle(InfoScreenEditHistoryList[i, 0]) != "")
				{
					num++;
					InfoScreenEditHistoryList[num, 0] = InfoScreenEditHistoryList[i, 0];
				}
			}
			TotalInfoScreenEditHistory = num;
			RemoveDuplicateEditorHistoryItems(ref InfoScreenEditHistoryList, ref TotalInfoScreenEditHistory);
		}

		public static bool ValidateEasiSlidesXML(ref XmlTextReader reader)
		{
			try
			{
				reader.Read();
				while (reader.Read())
				{
					if ((reader.NodeType == XmlNodeType.Element) & (reader.Name == "EasiSlides"))
					{
						return true;
					}
				}
			}
			catch
			{
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
