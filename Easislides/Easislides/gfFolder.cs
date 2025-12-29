//using JRO;
using Easislides.SQLite;
//using Easislides.Model.EasiSlidesDbDataSetTableAdapters;
using Easislides.Util;
//using Microsoft.Office.Interop.Access.Dao;
using Microsoft.Win32;
//using NetOffice.PowerPointApi;
using OfficeLib;
using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using System.Threading;

//using NetOffice.DAOApi;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Easislides
{
    internal unsafe partial class gf
    {

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
			if (!InString.Contains("\\") && !InString.Contains("/") && !InString.Contains(":") && !InString.Contains("*") && !InString.Contains("?") && !InString.Contains("\"") && !InString.Contains("<") && !InString.Contains(">") && !InString.Contains("|"))
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
			InString = InString.Replace("\\", "");
			InString = InString.Replace("/", "");
			InString = InString.Replace(":", "");
			InString = InString.Replace("*", "");
			InString = InString.Replace("?", "");
			InString = InString.Replace("\"", "");
			InString = InString.Replace("<", "");
			InString = InString.Replace(">", "");
			InString = InString.Replace("|", "");
			return InString;
		}

#if DAO
		public static void LoadFolderNamesArray()
		{
			ValidateDefaultFolders();
			int num = 0;
			string text = "";
			string text2 = "";
			string fullSearchString = "select * from FOLDER where FolderNo >=0 and FolderNo < " + DataUtil.ObjToString(41);
			Recordset recordSet = DbDaoController.GetRecordSet(ConnectStringMainDB, fullSearchString);
			if (recordSet?.EOF ?? true)
			{
				return;
			}
			recordSet.MoveFirst();
			while (!recordSet.EOF)
			{
				num = DataUtil.GetDataInt(recordSet, "FolderNo");
				FolderName[num] = DataUtil.GetDataString(recordSet, "name");
				FolderUse[num] = DataUtil.GetDataInt(recordSet, "Use");
				FolderGroupStyle[num] = (SortBy)DataUtil.GetDataInt(recordSet, "GroupStyle");
				FolderLyricsHeading[num, 0] = DataUtil.GetDataString(recordSet, "PreChorusHeading");
				FolderLyricsHeading[num, 1] = DataUtil.GetDataString(recordSet, "ChorusHeading");
				FolderLyricsHeading[num, 2] = DataUtil.GetDataString(recordSet, "BridgeHeading");
				FolderLyricsHeading[num, 3] = DataUtil.GetDataString(recordSet, "EndingHeading");
				FolderHeadingPercentSize[num] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "HeadingSize"));
				if ((FolderHeadingPercentSize[num] < 0) | (FolderHeadingPercentSize[num] > 150))
				{
					FolderHeadingPercentSize[num] = 100;
				}
				text2 = DataUtil.GetDataString(recordSet, "HeadingOption");
				if (text2 == "")
				{
					text2 = DataUtil.ObjToString(HeadingFormat.AsRegion1Plus);
				}
				FolderHeadingOption[num] = DataUtil.ObjToInt(text2);
				if ((FolderHeadingOption[num] < 0) | (FolderHeadingOption[num] > 2))
				{
					FolderHeadingOption[num] = DataUtil.ObjToInt(HeadingFormat.AsRegion1Plus);
				}
				text = DataUtil.GetDataString(recordSet, "BIUHeading");
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
				ShowLineSpacing[num, 0] = DataUtil.GetDataDouble(recordSet, "LineSpacing");
				if ((ShowLineSpacing[num, 0] < 0.5) | (ShowLineSpacing[num, 0] > 2.0))
				{
					ShowLineSpacing[num, 0] = 1.0;
				}
				ShowLineSpacing[num, 1] = DataUtil.GetDataDouble(recordSet, "LineSpacing2");
				if ((ShowLineSpacing[num, 1] < 0.5) | (ShowLineSpacing[num, 1] > 2.0))
				{
					ShowLineSpacing[num, 1] = 1.0;
				}
				ShowLeftMargin[num] = 2;
				ShowLeftMargin[num] = DataUtil.GetDataInt(recordSet, "LMargin");
				if ((ShowLeftMargin[num] < 0) | (ShowLeftMargin[num] > 40))
				{
					ShowLeftMargin[num] = 2;
				}
				ShowRightMargin[num] = 2;
				ShowRightMargin[num] = DataUtil.GetDataInt(recordSet, "RMargin");
				if ((ShowRightMargin[num] < 0) | (ShowRightMargin[num] > 40))
				{
					ShowRightMargin[num] = 2;
				}
				ShowBottomMargin[num] = 0;
				ShowBottomMargin[num] = DataUtil.GetDataInt(recordSet, "BMargin");
				if ((ShowBottomMargin[num] < 0) | (ShowBottomMargin[num] > 100))
				{
					ShowBottomMargin[num] = 0;
				}
				ShowFontSize[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Size0"));
				if ((ShowFontSize[num, 0] < 1) | (ShowFontSize[num, 0] > 100))
				{
					ShowFontSize[num, 0] = 37;
				}
				ShowFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 1);
				ShowFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 2);
				ShowFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 3);
				ShowFontBold[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 4);
				ShowFontItalic[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 5);
				ShowFontUnderline[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 6);
				ShowFontRTL[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 7);
				ShowFontName[num, 0] = DataUtil.GetDataString(recordSet, "FontName0");
				if (ShowFontName[num, 0] == "")
				{
					ShowFontName[num, 0] = "Microsoft Sans Serif";
				}
				ShowFontVPosition[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Vpos0"));
				if ((ShowFontVPosition[num, 0] < 0) | (ShowFontVPosition[num, 0] > 100))
				{
					ShowFontVPosition[num, 0] = 0;
				}
				ShowFontSize[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Size1"));
				if ((ShowFontSize[num, 1] < 1) | (ShowFontSize[num, 1] > 100))
				{
					ShowFontSize[num, 1] = 37;
				}
				ShowFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 1);
				ShowFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 2);
				ShowFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 3);
				ShowFontBold[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 4);
				ShowFontItalic[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 5);
				ShowFontUnderline[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 6);
				ShowFontRTL[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 7);
				ShowFontName[num, 1] = DataUtil.GetDataString(recordSet, "FontName1");
				if (ShowFontName[num, 1] == "")
				{
					ShowFontName[num, 1] = "Microsoft Sans Serif";
				}
				ShowFontVPosition[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Vpos1"));
				if ((ShowFontVPosition[num, 1] < ShowFontVPosition[num, 0]) | (ShowFontVPosition[num, 1] > 100))
				{
					ShowFontVPosition[num, 1] = ShowFontVPosition[num, 0] + (100 - ShowFontVPosition[num, 0]) / 2;
				}
				recordSet.MoveNext();
			}
		}
#elif SQLite
		public static void LoadFolderNamesArray()
		{
			ValidateDefaultFolders();
			int num = 0;
			string text = "";
			string text2 = "";
			string fullSearchString = "select * from FOLDER where FolderNo >=0 and FolderNo < " + DataUtil.ObjToString(41);

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
#endif

		public static int GetFolderNumber(string InName)
		{
			return GetFolderNumber(InName, ZeroIfInvalid: false);
		}

		public static int GetFolderNumber(string InName, bool ZeroIfInvalid)
		{
			for (int i = 0; i < 41; i++)
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
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{
					OleDbCommand command = new OleDbCommand("Delete * from Song where FolderNo =0", daoDb);
					command.ExecuteNonQuery();
				}
#elif SQLite
				using (DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB))
				{
					DbCommand command = new DbCommand("Delete * from Song where FolderNo = 0", connection);
					command.ExecuteNonQuery();
				}
#endif

				return true;
			}
			catch
			{
			}
			return false;
		}

#if DAO
		public static int ReFileSelectedSongs(ref ListView InListView, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			string text = "";
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Selected)
					{
						string text2;
						if (CurFolder == 0)
						{
							text2 = InListView.Items[num].SubItems[3].Text;
							NewFolder = DataUtil.StringToInt(InListView.Items[num].SubItems[1].Text);
						}
						else
						{
							string text3 = InListView.Items[num].SubItems[1].Text;
							text2 = DataUtil.Right(text3, text3.Length - 1);
						}
						try
						{
							tableRecordSet.Seek("=", text2, def, def, def, def, def, def, def, def, def, def, def, def);
							if (!tableRecordSet.NoMatch)
							{
								tableRecordSet.Edit();
								tableRecordSet.Fields["OldFolder"].Value = CurFolder;
								tableRecordSet.Fields["FolderNo"].Value = NewFolder;
								if (UpdateModifiedDate)
								{
									tableRecordSet.Fields["LastModified"].Value = DateTime.Now.Date;
								}
								tableRecordSet.Update();
								InListView.Items[num].Remove();
								RefileSongs[0]++;
								RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
							}
						}
						catch
						{
						}
					}
				}
				if (tableRecordSet != null)
				{
					tableRecordSet.Close();
					tableRecordSet = null;
				}
			}
			catch
			{
			}
			return RefileSongs[0];
		}
#elif SQLite
		public static int ReFileSelectedSongs(ref ListView InListView, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			string sQuery = "select * from SONG";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

				DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["SONGID"] };
				dataTable.PrimaryKey = primarykey;

				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Selected)
					{
						string text2;
						if (CurFolder == 0)
						{
							text2 = InListView.Items[num].SubItems[3].Text;
							NewFolder = DataUtil.StringToInt(InListView.Items[num].SubItems[1].Text);
						}
						else
						{
							string text3 = InListView.Items[num].SubItems[1].Text;
							text2 = DataUtil.Right(text3, text3.Length - 1);
						}
						try
						{
							DataRow dr = dataTable.Rows.Find($"{text2}");
							if (dr != null)
							{
								dr["OldFolder"] = CurFolder;
								dr["FolderNo"] = NewFolder;
								if (UpdateModifiedDate)
								{
									dr["LastModified"] = DateTime.Now.Date;
								}
								InListView.Items[num].Remove();
								RefileSongs[0]++;
								RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
							}
						}
						catch
						{
						}
					}
				}

				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch
			{
			}
			return RefileSongs[0];
		}
#endif

#if DAO
		public static int ReFileSelectedSongs(ref ListView InListView)
		{
			string text = "";
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Checked)
					{
						string text2 = InListView.Items[num].SubItems[3].Text;
						try
						{
							tableRecordSet.Seek("=", text2, def, def, def, def, def, def, def, def, def, def, def, def);
							if (!tableRecordSet.NoMatch)
							{
								int num2 = Convert.ToInt32(InListView.Items[num].SubItems[4].Text);
								tableRecordSet.Edit();
								tableRecordSet.Fields["OldFolder"].Value = 0;
								tableRecordSet.Fields["FolderNo"].Value = num2;
								tableRecordSet.Fields["LastModified"].Value = DateTime.Now.Date;
								tableRecordSet.Update();
								InListView.Items[num].Remove();
							}
						}
						catch
						{
						}
					}
				}
				if (tableRecordSet != null)
				{
					tableRecordSet.Close();
					tableRecordSet = null;
				}
			}
			catch
			{
			}
			return InListView.CheckedItems.Count;
		}
#elif SQLite
		public static int ReFileSelectedSongs(ref ListView InListView)
		{
			string sQuery = "select * from SONG";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

				DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["SONGID"] };
				dataTable.PrimaryKey = primarykey;

				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Checked)
					{
						string text2 = InListView.Items[num].SubItems[3].Text;
						try
						{
							DataRow dr = dataTable.Rows.Find($"{text2}");
							if (dr != null)
							{
								int num2 = Convert.ToInt32(InListView.Items[num].SubItems[4].Text);
								dr["OldFolder"] = 0;
								dr["FolderNo"] = num2;
								dr["LastModified"] = DateTime.Now.Date;
								InListView.Items[num].Remove();
							}
						}
						catch
						{
						}
					}
				}
				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch
			{
			}
			return InListView.CheckedItems.Count;
		}
#endif

		public static int ReFileSelectedSongsADO(ref ListView.SelectedListViewItemCollection SongItems, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			try
			{
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{
					string text = "";
					for (int num = SongItems.Count - 1; num >= 0; num--)
					{
						try
						{
							string text2;
							if (CurFolder == 0)
							{
								text2 = SongItems[num].SubItems[3].Text;
								NewFolder = DataUtil.StringToInt(SongItems[num].SubItems[1].Text);
							}
							else
							{
								string text3 = SongItems[num].SubItems[1].Text;
								text2 = DataUtil.Right(text3, text3.Length - 1);
							}
							text = "Update SONG SET OldFolder = @OldFolder, FolderNo =@FolderNo" + (UpdateModifiedDate ? ", LastModified =@LastModified" : "") + " where songid=" + text2.ToString();
							OleDbCommand command = new OleDbCommand(text, daoDb);
							command.CommandText = text;
							command.Parameters.AddWithValue("@OldFolder", CurFolder);
							command.Parameters.AddWithValue("@FolderNo", NewFolder);
							command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
							command.ExecuteNonQuery();
							command.Dispose();
							SongItems[num].Remove();
							RefileSongs[0]++;
							RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
						}
						catch (Exception)
						{
						}
					}
				}
#elif SQLite
				using (DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB))
				{
					string text = "";
					for (int num = SongItems.Count - 1; num >= 0; num--)
					{
						try
						{
							string text2;
							if (CurFolder == 0)
							{
								text2 = SongItems[num].SubItems[3].Text;
								NewFolder = DataUtil.StringToInt(SongItems[num].SubItems[1].Text);
							}
							else
							{
								string text3 = SongItems[num].SubItems[1].Text;
								text2 = DataUtil.Right(text3, text3.Length - 1);
							}
							text = "Update SONG SET OldFolder = @OldFolder, FolderNo =@FolderNo" + (UpdateModifiedDate ? ", LastModified =@LastModified" : "") + " where songid=" + text2.ToString();
							using DbCommand command = new DbCommand(text, connection);
							command.CommandText = text;
							command.Parameters.AddWithValue("@OldFolder", CurFolder);
							command.Parameters.AddWithValue("@FolderNo", NewFolder);
							command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
							command.ExecuteNonQuery();
							command.Dispose();
							SongItems[num].Remove();
							RefileSongs[0]++;
							RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
							Console.WriteLine(e.StackTrace);
						}
					}
				}
#endif
				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return RefileSongs[0];
		}

#if OleDb
		public static bool SwapFolderNumbers(ListView InFolderOrder)
		{
			bool flag = false;
			try
			{
				int[] array = new int[41];
				string text = "";
				for (int i = 1; i < 41; i++)
				{
					if (InFolderOrder.Items.Count >= 41)
					{
						break;
					}
					array[GetFolderNumber(InFolderOrder.Items[i - 1].SubItems[0].Text)] = i;
				}
				string fullSearchString = "select * from SONG WHERE FolderNo > 0 ";
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{

					int num = 0;
					OleDbDataAdapter da;
					DataSet ds = null;
					DataTable dt = null;
					(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
					dt = ds.Tables[0];

					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							if (num != array[num])
							{
								dr["FolderNo"] = array[num];
							}
						}
						da.Update(dt);
						dt.Dispose();
						da.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo > 0 ORDER BY FolderNo";
					(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
					dt = ds.Tables[0];

					string text2 = "";
					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							text2 = DataUtil.ObjToString(dr["Name"]);
							dr["FolderNo"] = -array[num];
						}
						da.Update(dt);
						dt.Dispose();
						da.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo < 0 ORDER BY FolderNo";
					(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
					dt = ds.Tables[0];

					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							dr["FolderNo"] = -num;
						}
						da.Update(dt);
						dt.Dispose();
						da.Dispose();
					}
				}
				return true;
			}
			catch
			{
			}
			return false;
		}
#elif SQLite
		public static bool SwapFolderNumbers(ListView InFolderOrder)
		{
			bool flag = false;
			try
			{
				int[] array = new int[41];
				string text = "";
				for (int i = 1; i < 41; i++)
				{
					if (InFolderOrder.Items.Count >= 41)
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
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return false;
		}
#endif

		public static void RemoveInvalidDirNameChars(ref string InString)
		{
			InString = InString.Replace("\\", "_");
			InString = InString.Replace("/", "_");
			InString = InString.Replace(":", "_");
			InString = InString.Replace("*", "_");
			InString = InString.Replace("?", "_");
			InString = InString.Replace("\"", "_");
			InString = InString.Replace("<", "_");
			InString = InString.Replace(">", "_");
			InString = InString.Replace("|", "_");
		}

        public static void InitFolderFiles(string InFolder)
        {
			try
			{
				string[] files = Directory.GetFiles(InFolder);
				string[] array = files;
				foreach (string path in array)
				{
					try
					{
						File.Delete(path);
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}
				string[] directories = Directory.GetDirectories(InFolder);
				array = directories;
				foreach (string text in array)
				{
					try
					{
                        InitFolderFiles(text);
						Directory.Delete(text);
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

        public static void DeleteFolderFiles(string InFolder)
		{
			try
			{
				if (CommonUtil.ProcessKill("POWERPNT"))
				{
					Thread.Sleep(2000);

					string[] files = Directory.GetFiles(InFolder);
					string[] array = files;
					foreach (string path in array)
					{
						try
						{
							File.Delete(path);
						}
						catch (Exception ex)
						{
							Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
						}
					}
					string[] directories = Directory.GetDirectories(InFolder);
					array = directories;
					foreach (string text in array)
					{
						try
						{
							DeleteFolderFiles(text);
							Directory.Delete(text);
						}
						catch (Exception ex)
						{
							Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
						}
					}
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
