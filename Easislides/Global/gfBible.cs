//using JRO;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Util;

using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;


namespace Easislides
{
    internal unsafe partial class Gf
    {

		public static void LoadBibleVersions(ref TabControl tabs)
		{
			try
			{
				ShowBibleVersions(tabs, BibleVersionCatalog.Load(ConnectStringBibleDB));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LoadBibleVersions] {ex}");
				ShowBibleVersions(tabs, BibleVersionCatalog.Empty, "Error Loading Bibles");
			}
		}

		internal static void ShowBibleVersions(TabControl tabs, BibleVersionCatalog versions, string emptyLabel = "No Bible")
		{
			BibleVersions = BibleVersionCatalog.Empty;
			while (tabs.TabPages.Count > 0)
				tabs.TabPages[tabs.TabPages.Count - 1].Dispose();

			BibleVersions = versions;
			tabs.ShowToolTips = true;
			foreach (var version in versions)
				tabs.TabPages.Add(new TabPage(version.Name) { ToolTipText = version.Description });
			tabs.Enabled = versions.Count > 0;
			if (versions.Count == 0)
				tabs.TabPages.Add(emptyLabel);
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
			string filePath = GetBibleFileName(InBibleVersion);
			int previousIndex = Math.Max(InChapterList.SelectedIndex, 0);
			InChapterList.Items.Clear();
			if (ShowAllBooksLine)
				InChapterList.Items.Add("All Books");
			if (!File.Exists(filePath))
				return false;

			int initialCount = InChapterList.Items.Count;
			InChapterList.BeginUpdate();
			try
			{
				using var connection = new DbConnection(new SQLiteConnectionStringBuilder
					{ DataSource = filePath, ReadOnly = true, FailIfMissing = true }.ToString());
				connection.Open();
				using var query = new SQLiteCommand("""
					SELECT bibletext FROM Bible WHERE book=0 AND chapter=10 AND verse BETWEEN 1 AND 66 ORDER BY verse
					""", connection);
				using var books = query.ExecuteReader();
				while (books.Read())
					InChapterList.Items.Add(books[0].ToString());
			}
			catch (SQLiteException ex)
			{
				Console.WriteLine($"[LoadBibleBooksList] {ex.Message}");
				InChapterList.Items.Clear();
				if (ShowAllBooksLine)
					InChapterList.Items.Add("All Books");
				return false;
			}
			finally
			{
				InChapterList.EndUpdate();
			}

			if (InChapterList.Items.Count == initialCount)
				return false;

			// Selection changes can open a passage, so release the book reader before setting the index.
			if (ShowSearchResultsLine)
				InChapterList.SelectedIndex = InChapterList.Items.Add("Search Results:");
			else
				InChapterList.SelectedIndex = previousIndex < InChapterList.Items.Count ? previousIndex : 0;
			return true;
		}

		public static string GetBibleFileName(int index) =>
			BibleVersions.ContainsIndex(index) ? BibleDir + BibleVersions[index].FileName : "";

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
				string connectString = ConnectStringDef + BibleDir + BibleVersions[InBiBleVersion].FileName;
				string bookName = "";
				string passageQuery = "";

				using (DbConnection connection = DbController.GetDbConnection(connectString))
				{
					
					DataTable recordset = null;
					if (AdHocListing)
					{
						passageQuery = "select * from Bible where book=0 and chapter=10 and verse=" + InBookNumber;

						recordset = DbController.GetDataTable(connection, passageQuery);

						if (recordset.Rows.Count > 0)
						{
							//recordset.MoveFirst();
							bookName = DataUtil.GetDataString(recordset.Rows[0], "bibletext");
						}
					}
					if (DoCompleteBook)
					{
						passageQuery = "select * from Bible where book=" + InBookNumber + " order by chapter, verse";
					}
					else
					{
						string chapterEndFilter = "";
						string verseEndFilter = "";
						if (ChapterEnd > 0)
						{
							chapterEndFilter = " and chapter <=" + ChapterEnd + " ";
						}
						if (VerseEnd > 0)
						{
							verseEndFilter = " and verse <=" + VerseEnd + " ";
						}
						passageQuery = "select * from Bible where book=" + InBookNumber + " and chapter >=" + ChapterStart + " " + chapterEndFilter + " and verse >=" + VerseStart + " " + verseEndFilter + " order by book, chapter, verse";
					}
					StringBuilder stringBuilder = new StringBuilder();
					string verseLabel = "";
					string verseText = "";
					char c = '\u3000';
					int previousChapter = 0;
					int verseLocationIndex = 0;
					string str = (ShowFormatTags && !AdHocListing) ? '\u0098'.ToString() : " ";

					recordset = DbController.GetDataTable(connection, passageQuery);

					if (recordset.Rows.Count > 0)
					{
						//recordset.MoveFirst();
						//while (!recordset.EOF)
						foreach (DataRow dr in recordset.Rows)
						{
							verseLocationIndex++;
							int dataInt = DataUtil.GetDataInt(dr, "chapter");
							int dataInt2 = DataUtil.GetDataInt(dr, "verse");
							verseLabel = ((ShowRepeatingChapters || dataInt != previousChapter) ? (dataInt + ":") : "") + dataInt2 + str;
							previousChapter = dataInt;
							verseText = (InShowVerses ? (DataUtil.GetDataString(dr, "bibletext") ?? "") : "");
							if (TrackOutput)
							{
								HB_VersesLocation[verseLocationIndex].Version = InBiBleVersion;
								HB_VersesLocation[verseLocationIndex].Book = InBookNumber;
								HB_VersesLocation[verseLocationIndex].Chapter = dataInt;
								HB_VersesLocation[verseLocationIndex].Verse = dataInt2;
								HB_VersesLocation[verseLocationIndex].Start = stringBuilder.Length;
								stringBuilder.Append(bookName + verseLabel + " " + verseText + "\n\n");
								HB_VersesLocation[verseLocationIndex].Length = stringBuilder.Length + 1 - HB_VersesLocation[verseLocationIndex].Start;
							}
							else if (AdHocListing)
							{
								stringBuilder.Append(verseText + " (" + bookName + verseLabel + ")\n");
							}
							else
							{
								stringBuilder.Append(verseLabel + (NoneSpacingText ? c.ToString() : " ") + verseText + "\n");
							}
							//recordset.MoveNext();
						}
					}
					if (TrackOutput)
					{
						HB_VersesLocation[0].Version = verseLocationIndex;
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
				string connectString = ConnectStringDef + BibleDir + BibleVersions[InBibleVersion].FileName;
				string verseQuery = "";

				using (DbConnection connection = DbController.GetDbConnection(connectString))
				{
					
					DataTable recordset = null;
					for (int i = 1; i <= HB_VersesLocation[0].Version; i++)
					{
						try
						{
							verseQuery = "select * from bible where book =" + HB_VersesLocation[i].Book + " and chapter=" + HB_VersesLocation[i].Chapter + " and verse=" + HB_VersesLocation[i].Verse + " order by book, chapter, verse";

							recordset = DbController.GetDataTable(connection, verseQuery);

							if (recordset.Rows.Count>0)
							{
								HB_VersesLocation[i].Start = stringBuilder.Length;
								stringBuilder.Append(string.Concat(InBookList.Items[HB_VersesLocation[i].Book - 1], " ", HB_VersesLocation[i].Chapter, ":", HB_VersesLocation[i].Verse, " ", InShowVerses ? DataUtil.GetDataString(recordset.Rows[0], "bibletext") : "", "\n\n"));
								HB_VersesLocation[i].Length = stringBuilder.Length + 1 - HB_VersesLocation[i].Start;
								recordset.Dispose();
							}
						}
						catch
						{
							HB_VersesLocation[i].Start = stringBuilder.Length;
							HB_VersesLocation[i].Length = 0;
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
			if (!BibleVersions.ContainsIndex(InBibleVersion))
			{
				return false;
			}
			for (int i = 1; i <= HB_VersesLocation[0].Version; i++)
			{
				HB_VersesLocation[i].Version = InBibleVersion;
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

		public static int LookUpBibleVersionNumber(string fileName) => BibleVersions.IndexOfFile(fileName);

		public static string LookUpBookName(int InBibleVersion, int InBookNumber)
		{
			try
			{
				string connectString = ConnectStringDef + BibleDir + BibleVersions[InBibleVersion].FileName;
				string fullSearchString = "select * from Bible where book=0 and chapter=10 and verse=" + InBookNumber;

				using DataTable datatable = DbController.GetDataTable(connectString, fullSearchString);

				if (datatable.Rows.Count > 0)
				{
					return DataUtil.GetDataString(datatable.Rows[0], "bibletext");
				}
			}
			catch
			{
				return "";
			}
			return "";
		}

		public static string LoadSelectedBibleVerses(string InFullBibleString)
		{
			try
			{
				bool isAdHocListing = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InFullBibleString, ';')) > 0;

				string[] bibleVersions = new string[2];
				int[] versionNumbers = new int[2];

				for (int i = 0; i < 2; i++)
				{
					bibleVersions[i] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
					versionNumbers[i] = LookUpBibleVersionNumber(bibleVersions[i]);
				}

				string[] verseData = { InFullBibleString, InFullBibleString };

				StringBuilder InTextString = new StringBuilder();

				bool hasSecondVersion = versionNumbers[1] >= 0;

				for (int i = 0; i <= (hasSecondVersion ? 1 : 0); i++)
				{
					bool useNonspacingText = PartialWordSearch(versionNumbers[1]);

					if (i == 1)
					{
						InTextString.Append(VerseSymbol[150]).Append("\n");
					}

					while (!string.IsNullOrEmpty(verseData[i]))
					{
						int inBookNumber = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int chapterStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int verseStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int chapterEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int verseEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));

						LoadBiblePassages(versionNumbers[i], inBookNumber, ref InTextString,
							InShowVerses: true, DoCompleteBook: false, TrackOutput: false,
							chapterStart, verseStart, chapterEnd, verseEnd,
							isAdHocListing, useNonspacingText, isAdHocListing, ShowFormatTags: true);

						InTextString.Append("\n");
					}
				}

				return InTextString.ToString();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return "";
			}
		}

		public static string LoadSelectedBibleVerses_Old(string InFullBibleString)
		{
			try
			{
				bool isAdHocListing = (DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InFullBibleString, ';')) > 0) ? true : false;
				string[] bibleVersions = new string[2];
				int[] versionNumbers = new int[2];
				string[] verseData = new string[2];
				bibleVersions[0] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
				versionNumbers[0] = LookUpBibleVersionNumber(bibleVersions[0]);
				bibleVersions[1] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
				versionNumbers[1] = LookUpBibleVersionNumber(bibleVersions[1]);
				verseData[0] = InFullBibleString;
				verseData[1] = InFullBibleString;
				StringBuilder stringBuilder = new StringBuilder();
				bool useNonspacingText = false;
				int lastVersionIndex = 0;
				lastVersionIndex = ((versionNumbers[1] >= 0) ? 1 : 0);
				for (int i = 0; i <= lastVersionIndex; i++)
				{
					useNonspacingText = PartialWordSearch(versionNumbers[1]);
					if (i == 1)
					{
						stringBuilder.Append(VerseSymbol[150] + "\n");
					}
					while (verseData[i] != "")
					{
						int inBookNumber = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int chapterStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int verseStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int chapterEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						int verseEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
						LoadBiblePassages(versionNumbers[i], inBookNumber, ref stringBuilder, InShowVerses: true, DoCompleteBook: false, TrackOutput: false, chapterStart, verseStart, chapterEnd, verseEnd, isAdHocListing, useNonspacingText, isAdHocListing, ShowFormatTags: true);
						stringBuilder.Append("\n");
					}
				}
				return stringBuilder.ToString();
			}
			catch
			{
				return "";
			}
		}

		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex)
		{
			return BuildBibleSearchString(InSearchPassage, VersionIndex, 0, 2);
		}

		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex, int BookIndex, int MatchSelected)
		{
			string passage = DataUtil.Trim(InSearchPassage).ToLower();
			if (passage.Length == 0)
				return "";

			bool partialWords = PartialWordSearch(VersionIndex);
			string prefix = partialWords ? "'%" : "'*[ -/:-@]";
			string suffix = partialWords ? "%'" : "[ -/:-@]*'";
			var query = new StringBuilder("select * from Bible where book");
			if (BookIndex >= 1)
				query.Append('=').Append(BookIndex);
			else
				query.Append(">0 ");

			query.Append(" AND ( (");
			AppendConditions(prefix, suffix);
			if (!partialWords)
			{
				query.Append(") OR (");
				AppendConditions("'", suffix);
				query.Append(") OR (");
				AppendConditions(prefix, "'");
				query.Append(") OR (");
				AppendConditions("'", "'");
			}
			return query.Append(") ) order by Book, chapter, verse ").ToString();

			void AppendConditions(string patternStart, string patternEnd)
			{
				if (MatchSelected is not (0 or 1))
				{
					AppendTerm(passage, patternStart, patternEnd);
					return;
				}

				ReadOnlySpan<char> words = passage;
				foreach (Range word in words.Split(' '))
				{
					if (word.Start.Value > 0)
						query.Append(MatchSelected == 1 ? " or  " : " and  ");
					AppendTerm(words[word].Trim(), patternStart, patternEnd);
				}
			}

			void AppendTerm(ReadOnlySpan<char> term, string patternStart, string patternEnd)
			{
				query.Append("lower(bibletext) like ").Append(patternStart);
				foreach (char c in term)
				{
					query.Append(c);
					if (c == '\'')
						query.Append('\'');
				}
				query.Append(patternEnd);
			}
		}

		public static bool PartialWordSearch(int VersionIndex)
		{
			if (!BibleVersions.ContainsIndex(VersionIndex))
				return false;

			try
			{
				string connectString = ConnectStringDef + BibleDir + BibleVersions[VersionIndex].FileName;
				string fullSearchString = "select * from Bible where book=0 and chapter=0 and verse=20";

				if (DbController.GetDataTable(connectString, fullSearchString).Rows.Count > 0)
				{
					return true;
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return false;
		}

		public static bool SearchBiblePassages(int InBibleVersion, ref ComboBox InBookList, string InSelectString, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			if (!BibleVersions.ContainsIndex(InBibleVersion))
				return false;

			int matchCount = 0;
			string verseReference = "";
			string connectString = ConnectStringDef + BibleDir + BibleVersions[InBibleVersion].FileName;
			StringBuilder stringBuilder = new StringBuilder();

			DbConnection connection = null;
			DbDataReader dataReader = null;

			(connection, dataReader) = DbController.GetDataReader(connectString, InSelectString);

			using (connection)
			{
				using (dataReader)
				{
					if (dataReader != null && dataReader.HasRows)
					{
						while (dataReader.Read() && matchCount < 3000)
						{
							matchCount++;
							int dataInt = DataUtil.GetDataInt(dataReader, "book");
							int dataInt2 = DataUtil.GetDataInt(dataReader, "chapter");
							int dataInt3 = DataUtil.GetDataInt(dataReader, "verse");
							verseReference = string.Concat(InBookList.Items[dataInt - 1], " ", dataInt2, ":", dataInt3, " ");
							HB_VersesLocation[matchCount].Version = InBibleVersion;
							HB_VersesLocation[matchCount].Book = dataInt;
							HB_VersesLocation[matchCount].Chapter = dataInt2;
							HB_VersesLocation[matchCount].Verse = dataInt3;
							HB_VersesLocation[matchCount].Start = stringBuilder.Length;
							stringBuilder.Append(verseReference + (InShowVerses ? DataUtil.GetDataString(dataReader, "bibletext") : "") + "\n\n");
							HB_VersesLocation[matchCount].Length = stringBuilder.Length + 1 - HB_VersesLocation[matchCount].Start;
						}
						HB_VersesLocation[0].Version = matchCount;
						InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
						if (matchCount >= 3000)
						{
							MessageBox.Show("The number of search results has been limited to " + Convert.ToString(3000) + ".");
						}
						return true;
					}
				}
			}
			return false;
		}

    }
}
