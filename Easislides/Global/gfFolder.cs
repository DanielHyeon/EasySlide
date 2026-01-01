using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.SQLite;
using Easislides.Util;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbParameter = System.Data.SQLite.SQLiteParameter;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;

namespace Easislides
{
	internal unsafe partial class gf
	{
		private static readonly char[] InvalidDirNameChars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

		private struct SongRefileUpdate
		{
			public int SongId;
			public int NewFolder;
			public int OldFolder;
			public ListViewItem Item;
		}

		private static bool ContainsInvalidDirNameChars(string value)
		{
			return value.IndexOfAny(InvalidDirNameChars) >= 0;
		}

		private static string RemoveInvalidDirNameCharsInternal(string value)
		{
			StringBuilder sb = new StringBuilder(value.Length);
			foreach (char ch in value)
			{
				if (Array.IndexOf(InvalidDirNameChars, ch) < 0)
				{
					sb.Append(ch);
				}
			}
			return sb.ToString();
		}

		private static string ReplaceInvalidDirNameChars(string value, char replacement)
		{
			char[] buffer = value.ToCharArray();
			bool changed = false;
			for (int i = 0; i < buffer.Length; i++)
			{
				if (Array.IndexOf(InvalidDirNameChars, buffer[i]) >= 0)
				{
					buffer[i] = replacement;
					changed = true;
				}
			}
			return changed ? new string(buffer) : value;
		}

		private static void DeleteFolderContents(string folder)
		{
			try
			{
				Stack<string> toProcess = new Stack<string>();
				Stack<string> toDelete = new Stack<string>();

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
			string text = DataUtil.Trim(FDir);
			if (text != "" && DataUtil.Right(text, 1) != "\\")
			{
				text += "\\";
			}
			if (Directory.Exists(text))
			{
				return true;
			}
			if (CreateDir)
			{
				return FileUtil.MakeDir(text);
			}
			return false;
		}

		public static bool ValidateDirNameFormat(string InString)
		{
			return ValidateDirNameFormat(InString, "");
		}

		public static bool ValidateDirNameFormat(string InString, string Heading)
		{
			if (!ContainsInvalidDirNameChars(InString))
			{
				return true;
			}
			if (Heading != "")
			{
				MessageBox.Show(Heading + " must not contain the characters: \\ / : * ? \" < > |");
			}
			return false;
		}

		public static string CorrectDirNameFormat(string InString)
		{
			return RemoveInvalidDirNameCharsInternal(InString);
		}

		public static void LoadFolderNamesArray()
		{
			ValidateDefaultFolders();
			int num = 0;
			string text = "";
			string text2 = "";
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
						num = DataUtil.GetDataInt(dataReader, "FolderNo");
						FolderName[num] = DataUtil.GetDataString(dataReader, "name");
						FolderUse[num] = DataUtil.GetDataInt(dataReader, "Use");
						FolderGroupStyle[num] = (SortBy)DataUtil.GetDataInt(dataReader, "GroupStyle");
						FolderLyricsHeading[num, 0] = DataUtil.GetDataString(dataReader, "PreChorusHeading");
						FolderLyricsHeading[num, 1] = DataUtil.GetDataString(dataReader, "ChorusHeading");
						FolderLyricsHeading[num, 2] = DataUtil.GetDataString(dataReader, "BridgeHeading");
						FolderLyricsHeading[num, 3] = DataUtil.GetDataString(dataReader, "EndingHeading");
						FolderHeadingPercentSize[num] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "HeadingSize"));
						if ((FolderHeadingPercentSize[num] < 0) | (FolderHeadingPercentSize[num] > 150))
						{
							FolderHeadingPercentSize[num] = 100;
						}
						text2 = DataUtil.GetDataString(dataReader, "HeadingOption");
						if (text2 == "")
						{
							text2 = DataUtil.ObjToString(HeadingFormat.AsRegion1Plus);
						}
						FolderHeadingOption[num] = DataUtil.ObjToInt(text2);
						if ((FolderHeadingOption[num] < 0) | (FolderHeadingOption[num] > 2))
						{
							FolderHeadingOption[num] = DataUtil.ObjToInt(HeadingFormat.AsRegion1Plus);
						}
						text = DataUtil.GetDataString(dataReader, "BIUHeading");
						if (text == "")
						{
							text = "4";
						}
						FolderHeadingFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 1);
						FolderHeadingFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 2);
						FolderHeadingFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 3);
						FolderHeadingFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 4);
						FolderHeadingFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 5);
						FolderHeadingFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 6);
						ShowLineSpacing[num, 0] = DataUtil.GetDataDouble(dataReader, "LineSpacing");
						if ((ShowLineSpacing[num, 0] < 0.5) | (ShowLineSpacing[num, 0] > 2.0))
						{
							ShowLineSpacing[num, 0] = 1.0;
						}
						ShowLineSpacing[num, 1] = DataUtil.GetDataDouble(dataReader, "LineSpacing2");
						if ((ShowLineSpacing[num, 1] < 0.5) | (ShowLineSpacing[num, 1] > 2.0))
						{
							ShowLineSpacing[num, 1] = 1.0;
						}
						ShowLeftMargin[num] = 2;
						ShowLeftMargin[num] = DataUtil.GetDataInt(dataReader, "LMargin");
						if ((ShowLeftMargin[num] < 0) | (ShowLeftMargin[num] > 40))
						{
							ShowLeftMargin[num] = 2;
						}
						ShowRightMargin[num] = 2;
						ShowRightMargin[num] = DataUtil.GetDataInt(dataReader, "RMargin");
						if ((ShowRightMargin[num] < 0) | (ShowRightMargin[num] > 40))
						{
							ShowRightMargin[num] = 2;
						}
						ShowBottomMargin[num] = 0;
						ShowBottomMargin[num] = DataUtil.GetDataInt(dataReader, "BMargin");
						if ((ShowBottomMargin[num] < 0) | (ShowBottomMargin[num] > 100))
						{
							ShowBottomMargin[num] = 0;
						}
						ShowFontSize[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Size0"));
						if ((ShowFontSize[num, 0] < 1) | (ShowFontSize[num, 0] > 100))
						{
							ShowFontSize[num, 0] = 37;
						}
						ShowFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 1);
						ShowFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 2);
						ShowFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 3);
						ShowFontBold[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 4);
						ShowFontItalic[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 5);
						ShowFontUnderline[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 6);
						ShowFontRTL[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 7);
						ShowFontName[num, 0] = DataUtil.GetDataString(dataReader, "FontName0");
						if (ShowFontName[num, 0] == "")
						{
							ShowFontName[num, 0] = "Microsoft Sans Serif";
						}
						ShowFontVPosition[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Vpos0"));
						if ((ShowFontVPosition[num, 0] < 0) | (ShowFontVPosition[num, 0] > 100))
						{
							ShowFontVPosition[num, 0] = 0;
						}
						ShowFontSize[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Size1"));
						if ((ShowFontSize[num, 1] < 1) | (ShowFontSize[num, 1] > 100))
						{
							ShowFontSize[num, 1] = 37;
						}
						ShowFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 1);
						ShowFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 2);
						ShowFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 3);
						ShowFontBold[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 4);
						ShowFontItalic[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 5);
						ShowFontUnderline[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 6);
						ShowFontRTL[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 7);
						ShowFontName[num, 1] = DataUtil.GetDataString(dataReader, "FontName1");
						if (ShowFontName[num, 1] == "")
						{
							ShowFontName[num, 1] = "Microsoft Sans Serif";
						}
						ShowFontVPosition[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Vpos1"));
						if ((ShowFontVPosition[num, 1] < ShowFontVPosition[num, 0]) | (ShowFontVPosition[num, 1] > 100))
						{
							ShowFontVPosition[num, 1] = ShowFontVPosition[num, 0] + (100 - ShowFontVPosition[num, 0]) / 2;
						}
					}
				}
			}
		}

		public static int GetFolderNumber(string InName)
		{
			return GetFolderNumber(InName, ZeroIfInvalid: false);
		}

		public static int GetFolderNumber(string InName, bool ZeroIfInvalid)
		{
			for (int i = 0; i < MAXSONGSFOLDERS; i++)
			{
				if (InName == FolderName[i])
				{
					return i;
				}
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
			int num = InListTitle.IndexOf(":", 0);
			if (num > 0)
			{
				string inName = DataUtil.Trim(DataUtil.Right(InListTitle, InListTitle.Length - num - 1));
				result = GetFolderNumber(inName, ZeroIfInvalid: true).ToString();
				InListTitle = DataUtil.Left(InListTitle, num);
			}
			return result;
		}

		public static bool EmptyDelFolder()
		{
			try
			{
				using (DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB))
				{
					DbCommand command = new DbCommand("Delete * from Song where FolderNo = 0", connection);
					command.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return false;
		}

		public static int ReFileSelectedSongs(ref ListView InListView, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			try
			{
				List<SongRefileUpdate> updates = new List<SongRefileUpdate>();
				HashSet<int> songIds = new HashSet<int>();

				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Selected)
					{
						string songIdText;
						int targetFolder;
						if (CurFolder == 0)
						{
							songIdText = InListView.Items[num].SubItems[3].Text;
							targetFolder = DataUtil.StringToInt(InListView.Items[num].SubItems[1].Text);
						}
						else
						{
							string text3 = InListView.Items[num].SubItems[1].Text;
							songIdText = DataUtil.Right(text3, text3.Length - 1);
							targetFolder = NewFolder;
						}

						int songId = DataUtil.StringToInt(songIdText);
						songIds.Add(songId);
						updates.Add(new SongRefileUpdate
						{
							SongId = songId,
							NewFolder = targetFolder,
							OldFolder = CurFolder,
							Item = InListView.Items[num]
						});
					}
				}

				if (updates.Count == 0)
				{
					return 0;
				}

				StringBuilder idBuilder = new StringBuilder();
				foreach (int id in songIds)
				{
					if (idBuilder.Length > 0)
					{
						idBuilder.Append(",");
					}
					idBuilder.Append(id);
				}

				string sQuery = "select * from SONG where SONGID in (" + idBuilder + ")";

				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(connection, sQuery);

				if (dataTable == null || dataTable.Rows.Count == 0)
				{
					return 0;
				}

				DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["SONGID"] };
				dataTable.PrimaryKey = primarykey;

				foreach (SongRefileUpdate update in updates)
				{
					try
					{
						DataRow dr = dataTable.Rows.Find(update.SongId);
						if (dr != null)
						{
							dr["OldFolder"] = update.OldFolder;
							dr["FolderNo"] = update.NewFolder;
							if (UpdateModifiedDate)
							{
								dr["LastModified"] = DateTime.Now.Date;
							}
							update.Item.Remove();
							RefileSongs[0]++;
							RefileSongs[RefileSongs[0]] = update.SongId;
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}

				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return RefileSongs[0];
		}

		public static int ReFileSelectedSongs(ref ListView InListView)
		{
			try
			{
				List<SongRefileUpdate> updates = new List<SongRefileUpdate>();
				HashSet<int> songIds = new HashSet<int>();

				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Checked)
					{
						int songId = DataUtil.StringToInt(InListView.Items[num].SubItems[3].Text);
						songIds.Add(songId);
						updates.Add(new SongRefileUpdate
						{
							SongId = songId,
							NewFolder = DataUtil.StringToInt(InListView.Items[num].SubItems[4].Text),
							OldFolder = 0,
							Item = InListView.Items[num]
						});
					}
				}

				if (updates.Count == 0)
				{
					return 0;
				}

				StringBuilder idBuilder = new StringBuilder();
				foreach (int id in songIds)
				{
					if (idBuilder.Length > 0)
					{
						idBuilder.Append(",");
					}
					idBuilder.Append(id);
				}

				string sQuery = "select * from SONG where SONGID in (" + idBuilder + ")";

				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(connection, sQuery);

				if (dataTable == null || dataTable.Rows.Count == 0)
				{
					return 0;
				}

				DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["SONGID"] };
				dataTable.PrimaryKey = primarykey;

				foreach (SongRefileUpdate update in updates)
				{
					try
					{
						DataRow dr = dataTable.Rows.Find(update.SongId);
						if (dr != null)
						{
							dr["OldFolder"] = update.OldFolder;
							dr["FolderNo"] = update.NewFolder;
							dr["LastModified"] = DateTime.Now.Date;
							update.Item.Remove();
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}

				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return InListView.CheckedItems.Count;
		}

		public static int ReFileSelectedSongsADO(ref ListView.SelectedListViewItemCollection SongItems, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			try
			{
				using (DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB))
				{
					using DbTransaction transaction = connection.BeginTransaction();
					string text = UpdateModifiedDate
						? "Update SONG SET OldFolder = @OldFolder, FolderNo = @FolderNo, LastModified = @LastModified where songid = @SongId"
						: "Update SONG SET OldFolder = @OldFolder, FolderNo = @FolderNo where songid = @SongId";
					using DbCommand command = new DbCommand(text, connection, transaction);
					DbParameter oldFolderParam = command.Parameters.Add("@OldFolder", DbType.Int32);
					DbParameter folderNoParam = command.Parameters.Add("@FolderNo", DbType.Int32);
					DbParameter songIdParam = command.Parameters.Add("@SongId", DbType.Int32);
					DbParameter lastModifiedParam = null;
					if (UpdateModifiedDate)
					{
						lastModifiedParam = command.Parameters.Add("@LastModified", DbType.Date);
					}
					for (int num = SongItems.Count - 1; num >= 0; num--)
					{
						try
						{
							string songIdText;
							int targetFolder;
							if (CurFolder == 0)
							{
								songIdText = SongItems[num].SubItems[3].Text;
								targetFolder = DataUtil.StringToInt(SongItems[num].SubItems[1].Text);
							}
							else
							{
								string text3 = SongItems[num].SubItems[1].Text;
								songIdText = DataUtil.Right(text3, text3.Length - 1);
								targetFolder = NewFolder;
							}

							int songId = DataUtil.StringToInt(songIdText);
							oldFolderParam.Value = CurFolder;
							folderNoParam.Value = targetFolder;
							songIdParam.Value = songId;
							if (UpdateModifiedDate && lastModifiedParam != null)
							{
								lastModifiedParam.Value = DateTime.Now.Date;
							}
							command.ExecuteNonQuery();
							SongItems[num].Remove();
							RefileSongs[0]++;
							RefileSongs[RefileSongs[0]] = songId;
						}
						catch (Exception e)
						{
							Trace.WriteLine($"ERROR : {e.Message}, {e.StackTrace}");
						}
					}
					transaction.Commit();
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return RefileSongs[0];
		}

		public static bool SwapFolderNumbers(ListView InFolderOrder)
		{
			bool flag = false;
			try
			{
				int[] array = new int[MAXSONGSFOLDERS];
				string text = "";
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
				using (DbConnection connection = new DbConnection(ConnectStringMainDB))
				{

					int num = 0;
					DbDataAdapter sQLiteDataAdapter;

					DataTable dataTable = null;
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					if (dataTable.Rows.Count > 0)
					{
						dataTable.AcceptChanges();

						DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
						sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

						foreach (DataRow dr in dataTable.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							if (num != array[num])
							{
								dr["FolderNo"] = array[num];
							}
						}
						sQLiteDataAdapter.Update(dataTable);
						dataTable.Dispose();
						sQLiteDataAdapter.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo > 0 ORDER BY FolderNo";
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					string text2 = "";
					if (dataTable.Rows.Count > 0)
					{
						dataTable.AcceptChanges();

						DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
						sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

						foreach (DataRow dr in dataTable.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							text2 = DataUtil.ObjToString(dr["Name"]);
							dr["FolderNo"] = -array[num];
						}
						sQLiteDataAdapter.Update(dataTable);
						dataTable.Dispose();
						sQLiteDataAdapter.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo < 0 ORDER BY FolderNo";
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					if (dataTable.Rows.Count > 0)
					{
						dataTable.AcceptChanges();

						DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
						sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

						foreach (DataRow dr in dataTable.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							dr["FolderNo"] = -num;
						}
						sQLiteDataAdapter.Update(dataTable);
						dataTable.Dispose();
						sQLiteDataAdapter.Dispose();
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			return false;
		}

		public static void RemoveInvalidDirNameChars(ref string InString)
		{
			InString = ReplaceInvalidDirNameChars(InString, '_');
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

		public static void DeleteFolderFiles(string InFolder)
		{
			try
			{
				if (CommonUtil.ProcessKill("POWERPNT"))
				{
					Thread.Sleep(2000);
					DeleteFolderContents(InFolder);
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
		}

		public static void BuildSubFolderList(string InDir, string RemovePrefix, ref string[,] InGroup, ref int InTotal)
		{
			string[] directories = Directory.GetDirectories(InDir);
			if (directories.Length > 0)
			{
				SingleArraySort(directories);
			}
			string[] array = directories;
			foreach (string text in array)
			{
				if ((!(RemovePrefix == ImagesDir) || !(text == RootEasiSlidesDir + "Images\\Scenery")) && !(text == RootEasiSlidesDir + "Images\\Tiles") && InTotal < 255)
				{
					InGroup[InTotal, 1] = text + "\\";
					InGroup[InTotal, 0] = "\\" + text.Replace(RemovePrefix, "");
					InTotal++;
					BuildSubFolderList(text, RemovePrefix, ref InGroup, ref InTotal);
				}
			}
		}
	}
}
