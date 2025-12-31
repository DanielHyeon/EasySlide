//using JRO;
using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Util;

using DbConnection = System.Data.SQLite.SQLiteConnection;


namespace Easislides
{
    internal unsafe partial class gf
    {

		public static void LoadBibleVersions(ref TabControl InTab)
		{
			try
			{
				InTab.TabPages.Clear();
				InTab.ShowToolTips = true;
				HB_TotalVersions = 0;

				Console.WriteLine($"[LoadBibleVersions] BibleDir: {BibleDir}");
				Console.WriteLine($"[LoadBibleVersions] ConnectStringBibleDB: {ConnectStringBibleDB}");

				string fullSearchString = "select * from Biblefolder where NAME like '%' and displayorder >=0 order by displayorder, NAME";

				//Provider = Microsoft.ACE.OLEDB.12.0;
				//string SQLiteConnectStringBibleDB = ConnectStringBibleDB.Replace("Provider=Microsoft.ACE.OLEDB.12.0;", "");
				using DataTable dataTable = DbController.GetDataTable(ConnectStringBibleDB, fullSearchString);
				Console.WriteLine($"[LoadBibleVersions] DataTable rows: {dataTable.Rows.Count}");

				if (dataTable.Rows.Count>0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach(DataRow dr in dataTable.Rows)
					{
						if (HB_TotalVersions <= 250)
						{
							TabPage tabPage = new TabPage();
							InTab.Controls.Add(tabPage);
							HB_Versions[HB_TotalVersions, 1] = DataUtil.GetDataString(dr, "NAME");
							HB_Versions[HB_TotalVersions, 4] = BibleDir + DataUtil.GetDataString(dr, "FILENAME");
							HB_Versions[HB_TotalVersions, 2] = DataUtil.GetDataString(dr, "DESCRIPTION");
							HB_Versions[HB_TotalVersions, 3] = DataUtil.GetDataString(dr, "COPYRIGHT");
							HB_Versions[HB_TotalVersions, 5] = "1";
							HB_Versions[HB_TotalVersions, 5] = DataUtil.GetDataString(dr, "SONGFOLDER");
							HB_Versions[HB_TotalVersions, 6] = "80";
							HB_Versions[HB_TotalVersions, 6] = DataUtil.GetDataString(dr, "SIZE");
							tabPage.Text = HB_Versions[HB_TotalVersions, 1];
							tabPage.ToolTipText = HB_Versions[HB_TotalVersions, 2];
							if (DataUtil.StringToInt(HB_Versions[HB_TotalVersions, 5]) < 1)
							{
								HB_Versions[HB_TotalVersions, 5] = "1";
							}
							if ((DataUtil.StringToInt(HB_Versions[HB_TotalVersions, 6]) < 5) | (DataUtil.StringToInt(HB_Versions[HB_TotalVersions, 6]) > 200))
							{
								HB_Versions[HB_TotalVersions, 6] = "80";
							}
							Console.WriteLine($"[LoadBibleVersions] Added Bible version: {HB_Versions[HB_TotalVersions, 1]}");
							HB_TotalVersions++;
						}
					}
				}
				Console.WriteLine($"[LoadBibleVersions] Total versions loaded: {HB_TotalVersions}");

				if (HB_TotalVersions > 0)
				{
					InTab.Enabled = true;
				}
				else
				{
					TabPage tabPage = new TabPage();
					tabPage.Text = "No Bible";
					InTab.Controls.Add(tabPage);
					InTab.Enabled = false;
					Console.WriteLine("[LoadBibleVersions] No Bible versions found - displaying 'No Bible' tab");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LoadBibleVersions] ERROR: {ex.Message}");
				Console.WriteLine($"[LoadBibleVersions] Stack Trace: {ex.StackTrace}");

				// 에러 발생 시에도 "No Bible" 탭 표시
				try
				{
					InTab.TabPages.Clear();
					TabPage tabPage = new TabPage();
					tabPage.Text = "Error Loading Bibles";
					InTab.Controls.Add(tabPage);
					InTab.Enabled = false;
				}
				catch { }
			}
		}

		public static bool LoadBibleBooksList(TabControl InTab, ref ComboBox InChapterList, bool ShowSearchResultsLine, RichTextBox OutputTextBox)
		{
			HB_CurVersionTabIndex = InTab.SelectedIndex;
			if (LoadBibleBooksList(HB_CurVersionTabIndex, ref InChapterList, ShowAllBooksLine: false, ShowSearchResultsLine))
			{
				return true;
			}

			if (OutputTextBox != null)
			{
				OutputTextBox.Text = "";
			}
			return false;
		}

		public static bool LoadBibleBooksList(int InBibleVersion, ref ComboBox InChapterList, bool ShowAllBooksLine, bool ShowSearchResultsLine)
		{

			HBFilename = GetBibleFileName(InBibleVersion);
			int num = (InChapterList.SelectedIndex >= 0) ? InChapterList.SelectedIndex : 0;
			InChapterList.Items.Clear();
			if (ShowAllBooksLine)
			{
				InChapterList.Items.Add("All Books");
			}

			int recordSetRowsCount = 0;

			string fullSearchString = "select * from Bible where book=0 and chapter=10 and (verse >0 and verse <=" + 66 + ") order by verse";

			using (DbConnection connection = DbController.GetDbConnection(ConnectSQLiteDef + HBFilename))
			{
				DataTable datatable = DbController.GetDataTable(connection, fullSearchString);
				recordSetRowsCount = datatable.Rows.Count;
				if (recordSetRowsCount > 0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					InChapterList.BeginUpdate();
					foreach (DataRow dr in datatable.Rows)
					{
						HBVersionBookName[DataUtil.GetDataInt(dr, "verse")] = DataUtil.GetDataString(dr, "bibletext");
						InChapterList.Items.Add(DataUtil.GetDataString(dr, "bibletext"));
						//recordSet.MoveNext();
					}
					InChapterList.EndUpdate();
				}
			}

			/// daniel
			/// OleDbConnection ?�결??꼬이??것을 방�? ?�기 ?�해
			/// ?�래 로직 분리
			/// InChapterList.SelectedIndex가 ?�출 ??경우 LoadBiblePassages가 ?�출 ??
			if (recordSetRowsCount > 0)
			{
				if (ShowSearchResultsLine)
				{
					InChapterList.Items.Add("Search Results:");
					InChapterList.SelectedIndex = InChapterList.Items.Count - 1;
				}
				else
				{
					InChapterList.SelectedIndex = ((num < 66) ? num : 0);
				}
				return true;
			}

			return false;
		}

		public static string GetBibleFileName(int SelectedVersion)
		{
			string fullSearchString = "select * from Biblefolder where DISPLAYORDER = " + SelectedVersion;

			using DataTable datatable = DbController.GetDataTable(ConnectStringBibleDB, fullSearchString);

			if (datatable.Rows.Count>0)
			{
				//recordSet.MoveFirst();
				if (HB_TotalVersions <= 250)
				{
					return BibleDir + DataUtil.GetDataString(datatable.Rows[0], "filename");
				}
			}
			return "";
		}

		public static bool LoadBiblePassagesFromTabIndex(int InBiBleVersion, ComboBox InBookList, ref RichTextBox InTextBox, bool InShowVerses)
		{
			InTextBox.Clear();
			StringBuilder InTextString = new StringBuilder();
			bool result = LoadBiblePassages(InBiBleVersion, InBookList.SelectedIndex + 1, ref InTextString, InShowVerses, DoCompleteBook: true, TrackOutput: true, 1, 1, 0, 0, AdHocListing: false, NoneSpacingText: false, ShowRepeatingChapters: true, ShowFormatTags: false);
			InTextBox.Text = InTextString.ToString();
			return result;
		}

		public static bool LoadBiblePassages(int InBiBleVersion, int InBookNumber, ref StringBuilder InTextString, bool InShowVerses, bool DoCompleteBook, bool TrackOutput, int ChapterStart, int VerseStart, int ChapterEnd, int VerseEnd, bool AdHocListing, bool NoneSpacingText, bool ShowRepeatingChapters, bool ShowFormatTags)
		{
			try
			{
				string connectString = ConnectStringDef + HB_Versions[InBiBleVersion, 4];
				string text = "";
				string text2 = "";

				using (DbConnection connection = DbController.GetDbConnection(connectString))
				{
					
					DataTable recordset = null;
					if (AdHocListing)
					{
						text2 = "select * from Bible where book=0 and chapter=10 and verse=" + InBookNumber;

						recordset = DbController.GetDataTable(connection, text2);

						if (recordset.Rows.Count > 0)
						{
							//recordset.MoveFirst();
							text = DataUtil.GetDataString(recordset.Rows[0], "bibletext");
						}
					}
					if (DoCompleteBook)
					{
						text2 = "select * from Bible where book=" + InBookNumber + " order by chapter, verse";
					}
					else
					{
						string text3 = "";
						string text4 = "";
						if (ChapterEnd > 0)
						{
							text3 = " and chapter <=" + ChapterEnd + " ";
						}
						if (VerseEnd > 0)
						{
							text4 = " and verse <=" + VerseEnd + " ";
						}
						text2 = "select * from Bible where book=" + InBookNumber + " and chapter >=" + ChapterStart + " " + text3 + " and verse >=" + VerseStart + " " + text4 + " order by book, chapter, verse";
					}
					StringBuilder stringBuilder = new StringBuilder();
					string text5 = "";
					string text6 = "";
					char c = '\u3000';
					int num = 0;
					int num2 = 0;
					string str = (ShowFormatTags && !AdHocListing) ? '\u0098'.ToString() : " ";

					recordset = DbController.GetDataTable(connection, text2);

					if (recordset.Rows.Count > 0)
					{
						//recordset.MoveFirst();
						//while (!recordset.EOF)
						foreach (DataRow dr in recordset.Rows)
						{
							num2++;
							int dataInt = DataUtil.GetDataInt(dr, "chapter");
							int dataInt2 = DataUtil.GetDataInt(dr, "verse");
							text5 = ((ShowRepeatingChapters || dataInt != num) ? (dataInt + ":") : "") + dataInt2 + str;
							num = dataInt;
							text6 = (InShowVerses ? (DataUtil.GetDataString(dr, "bibletext") ?? "") : "");
							if (TrackOutput)
							{
								HB_VersesLocation[num2, 0] = InBiBleVersion;
								HB_VersesLocation[num2, 1] = InBookNumber;
								HB_VersesLocation[num2, 2] = dataInt;
								HB_VersesLocation[num2, 3] = dataInt2;
								HB_VersesLocation[num2, 4] = stringBuilder.Length;
								stringBuilder.Append(text + text5 + " " + text6 + "\n\n");
								HB_VersesLocation[num2, 5] = stringBuilder.Length + 1 - HB_VersesLocation[num2, 4];
							}
							else if (AdHocListing)
							{
								stringBuilder.Append(text6 + " (" + text + text5 + ")\n");
							}
							else
							{
								stringBuilder.Append(text5 + (NoneSpacingText ? c.ToString() : " ") + text6 + "\n");
							}
							//recordset.MoveNext();
						}
					}
					if (TrackOutput)
					{
						HB_VersesLocation[0, 0] = num2;
					}
					InTextString.Append(DataUtil.TrimEnd(stringBuilder.ToString()));
					if (recordset != null)
					{
						recordset = null;
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool RefreshBiblePassages(int InBibleVersion, ComboBox InBookList, ref RichTextBox InTextContainer)
		{
			return RefreshBiblePassages(InBibleVersion, InBookList, ref InTextContainer, InShowVerses: true);
		}

		public static bool RefreshBiblePassages(int InBibleVersion, ComboBox InBookList, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			HBConvertVersion(InBibleVersion);
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
				string text = "";

				using (DbConnection connection = DbController.GetDbConnection(connectString))
				{
					
					DataTable recordset = null;
					for (int i = 1; i <= HB_VersesLocation[0, 0]; i++)
					{
						try
						{
							text = "select * from bible where book =" + HB_VersesLocation[i, 1] + " and chapter=" + HB_VersesLocation[i, 2] + " and verse=" + HB_VersesLocation[i, 3] + " order by book, chapter, verse";

							recordset = DbController.GetDataTable(connection, text);	

							if (recordset.Rows.Count>0)
							{
								HB_VersesLocation[i, 4] = stringBuilder.Length;
								stringBuilder.Append(string.Concat(InBookList.Items[HB_VersesLocation[i, 1] - 1], " ", HB_VersesLocation[i, 2], ":", HB_VersesLocation[i, 3], " ", InShowVerses ? DataUtil.GetDataString(recordset.Rows[0], "bibletext") : "", "\n\n"));
								HB_VersesLocation[i, 5] = stringBuilder.Length + 1 - HB_VersesLocation[i, 4];
								recordset.Dispose();
							}
						}
						catch
						{
							HB_VersesLocation[i, 4] = stringBuilder.Length;
							HB_VersesLocation[i, 5] = 0;
						}
					}
					//InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
					if (recordset != null)
					{
						recordset = null;
					}
				}
				return true;
			}
			catch
			{
				InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
				return false;
			}
		}

		public static bool HBConvertVersion(int InBibleVersion)
		{
			if (InBibleVersion < HB_TotalVersions - 1)
			{
				return false;
			}
			for (int i = 1; i <= HB_VersesLocation[0, 0]; i++)
			{
				HB_VersesLocation[i, 0] = InBibleVersion;
			}
			return true;
		}

		public static bool LookUpBibleName(string FileName, ref string Name, ref string Description, ref string Copyright, ref string Info)
		{
			Name = "";
			Description = "";
			Copyright = "";
			Info = "";
			string connectString = ConnectStringDef + FileName;
			try
			{
				DataTable dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=0");
				Description = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();

				dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=1");
				Name = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();
				dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=3");
				Copyright = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();
				dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=4");
				Info = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			if (Name == "" || Description == "")
			{
				return false;
			}
			return true;
		}

		public static int LookUpBibleVersionNumber(string InFileName)
		{
			if (HB_TotalVersions < 1)
			{
				return -1;
			}
			for (int i = 0; i < HB_TotalVersions; i++)
			{
				if (HB_Versions[i, 4] == BibleDir + InFileName)
				{
					return i;
				}
			}
			return -1;
		}

    }
}
