using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Global;
using Axicultus.Module;
using Axicultus.SQLite;
using Axicultus.Util;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;

namespace Axicultus;

	internal unsafe partial class Gf
	{
		private static readonly SearchValues<char> InvalidDirNameChars = SearchValues.Create("\\/:*?\"<>|");
		private static Dictionary<string, int> FolderNameIndex;

		private static void RebuildFolderNameIndex()
		{
			Dictionary<string, int> index = new(MAXSONGSFOLDERS, StringComparer.Ordinal);
			for (int i = 0; i < MAXSONGSFOLDERS; i++)
			{
				string name = Folders[i].Name;
				if (!string.IsNullOrEmpty(name) && !index.ContainsKey(name))
				{
					index[name] = i;
				}
			}
			FolderNameIndex = index;
		}

		private static void DeleteFolderContents(string folder)
		{
			try
			{
				Stack<string> toProcess = new();
				Stack<string> toDelete = new();

				toProcess.Push(folder);
				while (toProcess.Count > 0)
				{
					string current = toProcess.Pop();
					toDelete.Push(current);

					try
					{
						foreach (string file in Directory.EnumerateFiles(current))
						{
							try
							{
								File.Delete(file);
							}
							catch (Exception ex)
							{
								Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
							}
						}

						foreach (string dir in Directory.EnumerateDirectories(current))
						{
							toProcess.Push(dir);
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}

				while (toDelete.Count > 0)
				{
					string dir = toDelete.Pop();
					if (dir == folder)
					{
						continue;
					}
					try
					{
						Directory.Delete(dir);
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
		}

		public static bool ValidateDir(string FDir, bool CreateDir)
		{
			string directoryPath = DataUtil.Trim(FDir);
			if (directoryPath != "" && DataUtil.Right(directoryPath, 1) != "\\")
			{
				directoryPath += "\\";
			}
			if (Directory.Exists(directoryPath))
			{
				return true;
			}
			if (CreateDir)
			{
				return FileUtil.MakeDir(directoryPath);
			}
			return false;
		}

		public static bool ValidateDirNameFormat(string InString)
		{
			return ValidateDirNameFormat(InString, "");
		}

		public static bool ValidateDirNameFormat(string InString, string Heading)
		{
			ArgumentNullException.ThrowIfNull(InString);
			if (!InString.AsSpan().ContainsAny(InvalidDirNameChars))
			{
				return true;
			}
			if (Heading != "")
			{
				MessageBox.Show(Heading + " must not contain the characters: \\ / : * ? \" < > |");
			}
			return false;
		}

		public static string CorrectDirNameFormat(string folderName)
		{
			ArgumentNullException.ThrowIfNull(folderName);
			int invalidCount = folderName.AsSpan().CountAny(InvalidDirNameChars);
			if (invalidCount == 0)
				return folderName;
			return string.Create(folderName.Length - invalidCount, folderName, static (validName, source) =>
			{
				int written = 0;
				foreach (char character in source)
					if (!InvalidDirNameChars.Contains(character))
						validName[written++] = character;
			});
		}

		public static void LoadFolderNamesArray()
		{
			ValidateDefaultFolders();
			int folderNumber = 0;
			string headingFontStyleBits = "";
			string headingOptionValue = "";
			string fullSearchString = "select * from FOLDER where FolderNo >=0 and FolderNo < " + DataUtil.ObjToString(MAXSONGSFOLDERS);

			DbConnection connection = null;
			DbDataReader dataReader = null;

			(connection, dataReader) = DbController.GetDataReader(ConnectStringMainDB, fullSearchString);

			using (connection)
			{
				using (dataReader)
				{
					if (dataReader == null || !dataReader.HasRows)
					{
						return;
					}

					while (dataReader.Read())
					{
						folderNumber = DataUtil.GetDataInt(dataReader, "FolderNo");
						Folders[folderNumber].Name = DataUtil.GetDataString(dataReader, "name");
						Folders[folderNumber].Use = DataUtil.GetDataInt(dataReader, "Use");
						Folders[folderNumber].GroupStyle = ToFolderSortBy(DataUtil.GetDataInt(dataReader, "GroupStyle"));
						Folders[folderNumber].LyricsHeading[0] = DataUtil.GetDataString(dataReader, "PreChorusHeading");
						Folders[folderNumber].LyricsHeading[1] = DataUtil.GetDataString(dataReader, "ChorusHeading");
						Folders[folderNumber].LyricsHeading[2] = DataUtil.GetDataString(dataReader, "BridgeHeading");
						Folders[folderNumber].LyricsHeading[3] = DataUtil.GetDataString(dataReader, "EndingHeading");
						Folders[folderNumber].HeadingPercentSize = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "HeadingSize"));
						if ((Folders[folderNumber].HeadingPercentSize < 0) | (Folders[folderNumber].HeadingPercentSize > 150))
						{
							Folders[folderNumber].HeadingPercentSize = 100;
						}
						headingOptionValue = DataUtil.GetDataString(dataReader, "HeadingOption");
						if (headingOptionValue == "")
						{
							headingOptionValue = DataUtil.ObjToString(HeadingFormat.AsRegion1Plus);
						}
						Folders[folderNumber].HeadingOption = DataUtil.ObjToInt(headingOptionValue);
						if ((Folders[folderNumber].HeadingOption < 0) | (Folders[folderNumber].HeadingOption > 2))
						{
							Folders[folderNumber].HeadingOption = DataUtil.ObjToInt(HeadingFormat.AsRegion1Plus);
						}
						headingFontStyleBits = DataUtil.GetDataString(dataReader, "BIUHeading");
						if (headingFontStyleBits == "")
						{
							headingFontStyleBits = "4";
						}
						Folders[folderNumber].HeadingFontBold[0] = DataUtil.GetBitValue(DataUtil.ObjToInt(headingFontStyleBits), 1);
						Folders[folderNumber].HeadingFontItalic[0] = DataUtil.GetBitValue(DataUtil.ObjToInt(headingFontStyleBits), 2);
						Folders[folderNumber].HeadingFontUnderline[0] = DataUtil.GetBitValue(DataUtil.ObjToInt(headingFontStyleBits), 3);
						Folders[folderNumber].HeadingFontBold[1] = DataUtil.GetBitValue(DataUtil.ObjToInt(headingFontStyleBits), 4);
						Folders[folderNumber].HeadingFontItalic[1] = DataUtil.GetBitValue(DataUtil.ObjToInt(headingFontStyleBits), 5);
						Folders[folderNumber].HeadingFontUnderline[1] = DataUtil.GetBitValue(DataUtil.ObjToInt(headingFontStyleBits), 6);
						Folders[folderNumber].LineSpacing[0] = DataUtil.GetDataDouble(dataReader, "LineSpacing");
						if ((Folders[folderNumber].LineSpacing[0] < 0.5) | (Folders[folderNumber].LineSpacing[0] > 2.0))
						{
							Folders[folderNumber].LineSpacing[0] = 1.0;
						}
						Folders[folderNumber].LineSpacing[1] = DataUtil.GetDataDouble(dataReader, "LineSpacing2");
						if ((Folders[folderNumber].LineSpacing[1] < 0.5) | (Folders[folderNumber].LineSpacing[1] > 2.0))
						{
							Folders[folderNumber].LineSpacing[1] = 1.0;
						}
						Folders[folderNumber].LeftMargin = 2;
						Folders[folderNumber].LeftMargin = DataUtil.GetDataInt(dataReader, "LMargin");
						if ((Folders[folderNumber].LeftMargin < 0) | (Folders[folderNumber].LeftMargin > 40))
						{
							Folders[folderNumber].LeftMargin = 2;
						}
						Folders[folderNumber].RightMargin = 2;
						Folders[folderNumber].RightMargin = DataUtil.GetDataInt(dataReader, "RMargin");
						if ((Folders[folderNumber].RightMargin < 0) | (Folders[folderNumber].RightMargin > 40))
						{
							Folders[folderNumber].RightMargin = 2;
						}
						Folders[folderNumber].BottomMargin = 0;
						Folders[folderNumber].BottomMargin = DataUtil.GetDataInt(dataReader, "BMargin");
						if ((Folders[folderNumber].BottomMargin < 0) | (Folders[folderNumber].BottomMargin > 100))
						{
							Folders[folderNumber].BottomMargin = 0;
						}
						Folders[folderNumber].FontSize[0] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Size0"));
						if ((Folders[folderNumber].FontSize[0] < 1) | (Folders[folderNumber].FontSize[0] > 100))
						{
							Folders[folderNumber].FontSize[0] = 37;
						}
						Folders[folderNumber].FontBold[0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 1);
						Folders[folderNumber].FontItalic[0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 2);
						Folders[folderNumber].FontUnderline[0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 3);
						Folders[folderNumber].FontBold[2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 4);
						Folders[folderNumber].FontItalic[2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 5);
						Folders[folderNumber].FontUnderline[2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 6);
						Folders[folderNumber].FontRTL[0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 7);
						Folders[folderNumber].FontName[0] = DataUtil.GetDataString(dataReader, "FontName0");
						if (Folders[folderNumber].FontName[0] == "")
						{
							Folders[folderNumber].FontName[0] = "Microsoft Sans Serif";
						}
						Folders[folderNumber].FontVPosition[0] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Vpos0"));
						if ((Folders[folderNumber].FontVPosition[0] < 0) | (Folders[folderNumber].FontVPosition[0] > 100))
						{
							Folders[folderNumber].FontVPosition[0] = 0;
						}
						Folders[folderNumber].FontSize[1] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Size1"));
						if ((Folders[folderNumber].FontSize[1] < 1) | (Folders[folderNumber].FontSize[1] > 100))
						{
							Folders[folderNumber].FontSize[1] = 37;
						}
						Folders[folderNumber].FontBold[1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 1);
						Folders[folderNumber].FontItalic[1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 2);
						Folders[folderNumber].FontUnderline[1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 3);
						Folders[folderNumber].FontBold[3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 4);
						Folders[folderNumber].FontItalic[3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 5);
						Folders[folderNumber].FontUnderline[3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 6);
						Folders[folderNumber].FontRTL[1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 7);
						Folders[folderNumber].FontName[1] = DataUtil.GetDataString(dataReader, "FontName1");
						if (Folders[folderNumber].FontName[1] == "")
						{
							Folders[folderNumber].FontName[1] = "Microsoft Sans Serif";
						}
						Folders[folderNumber].FontVPosition[1] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Vpos1"));
						if ((Folders[folderNumber].FontVPosition[1] < Folders[folderNumber].FontVPosition[0]) | (Folders[folderNumber].FontVPosition[1] > 100))
						{
							Folders[folderNumber].FontVPosition[1] = Folders[folderNumber].FontVPosition[0] + (100 - Folders[folderNumber].FontVPosition[0]) / 2;
						}
					}
				}
			}
			RebuildFolderNameIndex();
		}

		/// <summary>
		/// FOLDER.GroupStyle as a folder sort order: 0 title, 1 CJK word count, 2 song number; any other
		/// value is title order (openspec change add-song-number-sort, N4).
		/// </summary>
		public static SortBy ToFolderSortBy(int groupStyle)
		{
        return groupStyle switch
        {
            (int)SortBy.WordCount => SortBy.WordCount,
            (int)SortBy.Number => SortBy.Number,
            _ => SortBy.Alpha,
        };
    }

		public static int GetFolderNumber(string InName)
		{
			return GetFolderNumber(InName, ZeroIfInvalid: false);
		}

		public static int GetFolderNumber(string InName, bool ZeroIfInvalid)
		{
			if (FolderNameIndex == null)
			{
				RebuildFolderNameIndex();
			}
			if (FolderNameIndex.TryGetValue(InName, out int folderNo))
			{
				return folderNo;
			}
			if (ZeroIfInvalid)
			{
				return 0;
			}
			return 1;
		}

		public static string ExtractFolderFromListTitle(ref string InListTitle)
		{
			string result = "1";
			int separatorIndex = InListTitle.IndexOf(':', 0);
			if (separatorIndex > 0)
			{
				string inName = DataUtil.Trim(DataUtil.Right(InListTitle, InListTitle.Length - separatorIndex - 1));
				result = GetFolderNumber(inName, ZeroIfInvalid: true).ToString();
				InListTitle = DataUtil.Left(InListTitle, separatorIndex);
			}
			return result;
		}

		public static bool EmptyDelFolder()
		{
			try
			{
            using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
            using DbCommand command = new("DELETE FROM Song WHERE FolderNo = 0", connection);
            command.ExecuteNonQuery();
            return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return false;
		}

		public static HashSet<int> ReFileSelectedSongs(ListView songs, int currentFolder, int newFolder, bool updateModifiedDate, string connectionString)
		{
			HashSet<int> movedIds = [];
			if (songs.SelectedItems.Count == 0)
				return movedIds;

			List<ListViewItem> movedRows = [];
			try
			{
				using DbConnection connection = DbController.GetDbConnection(connectionString);
				using DbTransaction transaction = connection.BeginTransaction();
				string sql = updateModifiedDate
					? "UPDATE SONG SET OldFolder = @old, FolderNo = @folder, LastModified = @date WHERE SongID = @id"
					: "UPDATE SONG SET OldFolder = @old, FolderNo = @folder WHERE SongID = @id";
				using DbCommand command = new(sql, connection, transaction);
				command.Parameters.Add("@old", DbType.Int32).Value = currentFolder;
				var folder = command.Parameters.Add("@folder", DbType.Int32);
				var id = command.Parameters.Add("@id", DbType.Int32);
				if (updateModifiedDate)
					command.Parameters.Add("@date", DbType.String).Value = DataUtil.ToStoredDate(DateTime.Now);

				foreach (ListViewItem item in songs.SelectedItems)
				{
					string key = item.SubItems[1].Text;
					if (key.Length == 0)
						continue;
					ReadOnlySpan<char> idText = currentFolder == 0 ? item.SubItems[3].Text : key.AsSpan(1);
					if (!int.TryParse(idText, out int songId))
						continue;

					folder.Value = currentFolder == 0 ? DataUtil.StringToInt(key) : newFolder;
					id.Value = songId;
					if (command.ExecuteNonQuery() > 0)
					{
						movedRows.Add(item);
						movedIds.Add(songId);
					}
				}
				transaction.Commit();
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ReFileSelectedSongs failed: {ex}");
				return [];
			}

			// Keep the source selection intact if the transaction rolls back.
			for (int i = movedRows.Count - 1; i >= 0; i--)
				movedRows[i].Remove();
			return movedIds;
		}

		public static int ReFileSelectedSongs(ref ListView InListView)
		{
			var recovered = new List<ListViewItem>();
			try
			{
				using var connection = DbController.GetDbConnection(ConnectStringMainDB);
				using var transaction = connection.BeginTransaction();
				using var command = new DbCommand("""
					UPDATE SONG SET OldFolder=0, FolderNo=@folder, LastModified=@date
					WHERE SongID=@id AND FolderNo=0
					""", connection, transaction);
				var folder = command.Parameters.Add("@folder", DbType.Int32);
				var id = command.Parameters.Add("@id", DbType.Int32);
				command.Parameters.AddWithValue("@date", DataUtil.ToStoredDate(DateTime.Now));
				foreach (ListViewItem item in InListView.CheckedItems)
				{
					if (!int.TryParse(item.SubItems[3].Text, out int songId)
						|| !int.TryParse(item.SubItems[4].Text, out int originalFolder))
						continue;
					id.Value = songId;
					folder.Value = originalFolder;
					if (command.ExecuteNonQuery() > 0)
						recovered.Add(item);
				}
				transaction.Commit();
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Song recovery failed: {ex}");
				return InListView.CheckedItems.Count;
			}
			foreach (var item in recovered)
				item.Remove();
			return InListView.CheckedItems.Count;
		}

		public static bool SwapFolderNumbers(ListView InFolderOrder)
		{
			try
			{
				int[] array = new int[MAXSONGSFOLDERS];
				for (int i = 1; i < MAXSONGSFOLDERS; i++)
				{
					//daniel if (InFolderOrder.Items.Count >= 41) break;
					if (InFolderOrder.Items.Count < MAXSONGSFOLDERS)
					{
						break;
					}
					array[GetFolderNumber(InFolderOrder.Items[i - 1].SubItems[0].Text)] = i;
				}
				string fullSearchString = "select * from SONG WHERE FolderNo > 0 ";
            using DbConnection connection = new(ConnectStringMainDB);

            int originalFolderNumber = 0;
            DbDataAdapter sQLiteDataAdapter;

            DataTable dataTable = null;
            (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, fullSearchString);

            if (dataTable.Rows.Count > 0)
            {
                dataTable.AcceptChanges();

                DbCommandBuilder sqCB = new(sQLiteDataAdapter);
                sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

                foreach (DataRow dr in dataTable.Rows)
                {
                    originalFolderNumber = DataUtil.ObjToInt(dr["FolderNo"]);
                    if (originalFolderNumber != array[originalFolderNumber])
                    {
                        dr["FolderNo"] = array[originalFolderNumber];
                    }
                }
                sQLiteDataAdapter.Update(dataTable);
                dataTable.Dispose();
                sQLiteDataAdapter.Dispose();
            }

            fullSearchString = "select * from FOLDER  where FolderNo > 0 ORDER BY FolderNo";
            (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, fullSearchString);

            string folderName = "";
            if (dataTable.Rows.Count > 0)
            {
                dataTable.AcceptChanges();

                DbCommandBuilder sqCB = new(sQLiteDataAdapter);
                sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

                foreach (DataRow dr in dataTable.Rows)
                {
                    originalFolderNumber = DataUtil.ObjToInt(dr["FolderNo"]);
                    folderName = DataUtil.ObjToString(dr["Name"]);
                    dr["FolderNo"] = -array[originalFolderNumber];
                }
                sQLiteDataAdapter.Update(dataTable);
                dataTable.Dispose();
                sQLiteDataAdapter.Dispose();
            }

            fullSearchString = "select * from FOLDER  where FolderNo < 0 ORDER BY FolderNo";
            (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, fullSearchString);

            if (dataTable.Rows.Count > 0)
            {
                dataTable.AcceptChanges();

                DbCommandBuilder sqCB = new(sQLiteDataAdapter);
                sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

                foreach (DataRow dr in dataTable.Rows)
                {
                    originalFolderNumber = DataUtil.ObjToInt(dr["FolderNo"]);
                    dr["FolderNo"] = -originalFolderNumber;
                }
                sQLiteDataAdapter.Update(dataTable);
                dataTable.Dispose();
                sQLiteDataAdapter.Dispose();
            }
            return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return false;
		}

		public static void RemoveInvalidDirNameChars(ref string folderName)
		{
			ArgumentNullException.ThrowIfNull(folderName);
			if (folderName.AsSpan().ContainsAny(InvalidDirNameChars))
				folderName = string.Create(folderName.Length, folderName, static (destination, source) =>
					source.AsSpan().ReplaceAny(destination, InvalidDirNameChars, '_'));
		}

		public static void InitFolderFiles(string InFolder)
		{
			try
			{
				DeleteFolderContents(InFolder);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
		}

		public static void BuildSubFolderList(string InDir, string RemovePrefix, List<FolderGroup> InGroup)
		{
			Stack<string> pending = new();
			pending.Push(InDir);

			while (pending.Count > 0)
			{
				string current = pending.Pop();
				string[] directories = Directory.GetDirectories(current);
				if (directories.Length > 0)
				{
					SingleArraySort(directories);
				}
				for (int i = directories.Length - 1; i >= 0; i--)
				{
					string subdirectoryPath = directories[i];
					if ((!(RemovePrefix == ImagesDir) || !(subdirectoryPath == RootAxicultusDir + "Images\\Scenery")) && !(subdirectoryPath == RootAxicultusDir + "Images\\Tiles") && InGroup.Count < 255)
					{
						InGroup.Add(new FolderGroup
						{
							Path = subdirectoryPath + "\\",
							Name = "\\" + subdirectoryPath.Replace(RemovePrefix, "")
						});
						pending.Push(subdirectoryPath);
					}
				}
			}
		}
	}
