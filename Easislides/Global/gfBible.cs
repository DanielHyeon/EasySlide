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
				string connectString = DbController.ConnectionStringForFile(BibleDir + BibleVersions[InBiBleVersion].FileName, readOnly: true);
				string bookName = "";
				using (DbConnection connection = DbController.GetDbConnection(connectString))
				{
					if (AdHocListing)
					{
						using var name = DbController.CreateCommand(connection,
							"select bibletext from Bible where book=0 and chapter=10 and verse=@book limit 1", ("@book", InBookNumber));
						bookName = Convert.ToString(name.ExecuteScalar());
					}
					var passage = DoCompleteBook
						? new BoundQuery("select chapter, verse, bibletext from Bible where book=@book order by chapter, verse", ("@book", InBookNumber))
						: new BoundQuery("""
							select chapter, verse, bibletext from Bible where book=@book and chapter>=@firstChapter
							and (@lastChapter<=0 or chapter<=@lastChapter) and verse>=@firstVerse
							and (@lastVerse<=0 or verse<=@lastVerse) order by chapter, verse
							""", ("@book", InBookNumber), ("@firstChapter", ChapterStart), ("@lastChapter", ChapterEnd),
							("@firstVerse", VerseStart), ("@lastVerse", VerseEnd));
					StringBuilder stringBuilder = new StringBuilder();
					string verseLabel = "";
					string verseText = "";
					char c = '\u3000';
					int previousChapter = 0;
					int verseLocationIndex = 0;
					string str = (ShowFormatTags && !AdHocListing) ? '\u0098'.ToString() : " ";

					using var recordset = DbController.GetDataTable(connection, passage.Text, passage.Parameters);

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
				string connectString = DbController.ConnectionStringForFile(BibleDir + BibleVersions[InBibleVersion].FileName, readOnly: true);
				using var connection = DbController.GetDbConnection(connectString);
				using var verseQuery = connection.CreateCommand();
				verseQuery.CommandText = "select bibletext from Bible where book=@book and chapter=@chapter and verse=@verse limit 1";
				var book = verseQuery.Parameters.Add("@book", DbType.Int32);
				var chapter = verseQuery.Parameters.Add("@chapter", DbType.Int32);
				var verse = verseQuery.Parameters.Add("@verse", DbType.Int32);
				for (int i = 1; i <= HB_VersesLocation[0].Version; i++)
				{
					HB_VersesLocation[i].Start = stringBuilder.Length;
					HB_VersesLocation[i].Length = 0;
					try
					{
						book.Value = HB_VersesLocation[i].Book;
						chapter.Value = HB_VersesLocation[i].Chapter;
						verse.Value = HB_VersesLocation[i].Verse;
						object value = verseQuery.ExecuteScalar();
						if (value == null) continue;
						stringBuilder.Append(string.Concat(InBookList.Items[HB_VersesLocation[i].Book - 1], " ",
							chapter.Value, ":", verse.Value, " ", InShowVerses ? Convert.ToString(value) : "", "\n\n"));
						HB_VersesLocation[i].Length = stringBuilder.Length + 1 - HB_VersesLocation[i].Start;
					}
					catch (Exception ex)
					{
						System.Diagnostics.Trace.WriteLine(ex.Message);
					}
				}
				InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
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
			string connectString = DbController.ConnectionStringForFile(FileName, readOnly: true);
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
				string connectString = DbController.ConnectionStringForFile(BibleDir + BibleVersions[InBibleVersion].FileName, readOnly: true);
				const string fullSearchString = "select bibletext from Bible where book=0 and chapter=10 and verse=@book";

				using DataTable datatable = DbController.GetDataTable(connectString, fullSearchString, ("@book", InBookNumber));

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

		public static BoundQuery BuildBibleSearchQuery(string InSearchPassage, int VersionIndex)
		{
			return BuildBibleSearchQuery(InSearchPassage, VersionIndex, 0, 2);
		}

		public static BoundQuery BuildBibleSearchQuery(string InSearchPassage, int VersionIndex, int BookIndex, int MatchSelected)
		{
			string passage = DataUtil.Trim(InSearchPassage).ToLower();
			if (passage.Length == 0)
				return null;

			bool partialWords = PartialWordSearch(VersionIndex);
			string[] terms = MatchSelected is 0 or 1
				? passage.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
				: [passage];
			var parameters = new System.Collections.Generic.List<(string Name, object Value)>();
			var conditions = new string[terms.Length];
			for (int i = 0; i < terms.Length; i++)
			{
				string name = "@term" + i;
				if (partialWords)
				{
					conditions[i] = "lower(bibletext) LIKE " + name;
					parameters.Add((name, "%" + terms[i] + "%"));
				}
				else
				{
					// Padding lets the same word boundary match the beginning and end of a verse.
					conditions[i] = "(' ' || lower(bibletext) || ' ') GLOB " + name;
					parameters.Add((name, WholeWordPattern(terms[i])));
				}
			}
			string bookFilter = "book>0";
			if (BookIndex >= 1)
			{
				bookFilter = "book=@book";
				parameters.Add(("@book", BookIndex));
			}
			string matches = string.Join(MatchSelected == 1 ? " OR " : " AND ", conditions);
			return new BoundQuery($"SELECT book, chapter, verse, bibletext FROM Bible WHERE {bookFilter} AND ({matches}) ORDER BY book, chapter, verse",
				parameters.ToArray());
		}

		private static string WholeWordPattern(string term)
		{
			const string boundary = "[][\t\r\n -/:-@]";
			var pattern = new StringBuilder("*").Append(boundary);
			foreach (char c in term)
			{
				// Keep LIKE's user wildcards while treating GLOB's own metacharacters as text.
				switch (c)
				{
					case '%': pattern.Append('*'); break;
					case '_': pattern.Append('?'); break;
					case '*': pattern.Append("[*]"); break;
					case '?': pattern.Append("[?]"); break;
					case '[': pattern.Append("[[]"); break;
					default: pattern.Append(c); break;
				}
			}
			return pattern.Append(boundary).Append('*').ToString();
		}

		public static bool PartialWordSearch(int VersionIndex)
		{
			if (!BibleVersions.ContainsIndex(VersionIndex))
				return false;

			try
			{
				string connectString = DbController.ConnectionStringForFile(BibleDir + BibleVersions[VersionIndex].FileName, readOnly: true);
				using var connection = DbController.GetDbConnection(connectString);
				using var command = DbController.CreateCommand(connection, "SELECT 1 FROM Bible WHERE book=0 AND chapter=0 AND verse=20 LIMIT 1");
				return command.ExecuteScalar() != null;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return false;
		}

		public static bool SearchBiblePassages(int InBibleVersion, ref ComboBox InBookList, BoundQuery query, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			if (query == null || !BibleVersions.ContainsIndex(InBibleVersion))
				return false;

			int matchCount = 0;
			string verseReference = "";
			string connectString = DbController.ConnectionStringForFile(BibleDir + BibleVersions[InBibleVersion].FileName, readOnly: true);
			StringBuilder stringBuilder = new StringBuilder();

			DbConnection connection = null;
			DbDataReader dataReader = null;

			(connection, dataReader) = DbController.GetDataReader(connectString, query.Text, query.Parameters);

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
