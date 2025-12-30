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

		public static void MessageOverSplashScreen(string InString)
		{
			SplashScreenBack = true;
			MessageBox.Show(InString);
			SplashScreenBack = false;
		}

		public static void RenameExtensions(string InDir, string InOldExt, string InNewExt)
		{
			try
			{
				string[] files = Directory.GetFiles(InDir, "*" + InOldExt);
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						File.Move(text, InDir + "\\" + Path.GetFileNameWithoutExtension(text) + InNewExt);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}

		public static void LoadEulaText()
		{
			EULA = "EasiSlides Software\n\nIMPORTANT: This software end user licence agreement ('EULA') is a legal agreement between you and EasiSlides. Read it carefully before completing the installation process and using the software. It provides a licence to use the software. By installing and using the software, you are confirming your acceptance of the software and agreeing to become bound by the terms of this agreement. If you do not agree with the terms of this licence you must remove EasiSlides Software files from your storage devices and cease to use the product.\n\nAll copyrights to 'EasiSlides Software', hereafter shall be referred to as 'the software', are exclusively owned by EasiSlides. Your licence confers no title or ownership in the software and should not be construed as a sale of any right in the software.\n\nYou MUST NOT use this software for purposes which are unlawful, including, but not limited to, the transmission of obscene or offensive content, or contents which may harass or cause distress to any person.\n\nYou may use this software for any length of time.\n\nYou are hereby licenced to make any number of backup copies of this software and documentation. You can give the copy of the software to anyone or distribute the software provided you abide by the following Licence restrictions:\n(a) You may not reproduce or distribute the software for the purpose of promoting other non-EasiSlides products or organisations unless specific permission to do so have been obtained from the EasiSlides Copyright holder.\n(b) You may not alter, merge, modify, adapt or translate the software, or decompile, reverse engineer, disassemble, or otherwise reduce the Software to a human-perceivable form.\n(c) You may not rent, lease, or sublicence the Software.\n(d) Where the software is placed on a network for distribution, you must place alongside the distributed software a fully functional and visible hyperlink to the official EasiSlides website at http://www.EasiSlides.com.\n(e) No fee is charged for the software.\n\nEASISLIDES SOFTWARE IS DISTRIBUTED 'AS IS'. NO WARRANTY OF ANY KIND IS EXPRESSED OR IMPLIED. YOU USE IT AT YOUR OWN RISK. EASISLIDES WILL NOT BE LIABLE FOR DATA LOSS, DAMAGES, LOSS OF PROFITS OR ANY OTHER KIND OF LOSS WHILE USING OR MISUSING THIS SOFTWARE.\n\nThis EULA  shall be governed by and construed in accordance with the laws of Northern Ireland. Any dispute arising under this EULA shall be subject to the exclusive jurisdiction of the courts of Northern Ireland.\n\nCopyright 짤 2007 EasiSlides, All rights reserved.\nInternet:  http://www.EasiSlides.com\n";
		}

		public static string GetUniqueID()
		{
			return DataUtil.Right(Guid.NewGuid().ToString(), 6);
		}

		public static void SetPatternPeriod()
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(PerformanceStartTime);
			if (timeSpan.TotalSeconds < 12.0)
			{
				PatternTimerPeriod = 40;
			}
			else if (timeSpan.TotalSeconds < 25.0)
			{
				PatternTimerPeriod = 80;
			}
			else
			{
				PatternTimerPeriod = 200;
			}
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				text = (string)InMenuItem3.Tag;
				text2 = InMenuItem3.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3, ToolStripMenuItem InMenuItem4)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				text = (string)InMenuItem3.Tag;
				text2 = InMenuItem3.Text;
			}
			else if (SelectedMenuItemName == InMenuItem4.Name)
			{
				SelectedBtn.Image = InMenuItem4.Image;
				text = (string)InMenuItem4.Tag;
				text2 = InMenuItem4.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3, ToolStripMenuItem InMenuItem4, ToolStripMenuItem InMenuItem5)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				text = (string)InMenuItem3.Tag;
				text2 = InMenuItem3.Text;
			}
			else if (SelectedMenuItemName == InMenuItem4.Name)
			{
				SelectedBtn.Image = InMenuItem4.Image;
				text = (string)InMenuItem4.Tag;
				text2 = InMenuItem4.Text;
			}
			else if (SelectedMenuItemName == InMenuItem5.Name)
			{
				SelectedBtn.Image = InMenuItem5.Image;
				text = (string)InMenuItem5.Tag;
				text2 = InMenuItem5.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static Color SelectNewColour(Color CurColour)
		{
			Color InColour = CurColour;
			SelectColor(ref InColour);
			return InColour;
		}

		public static bool SelectColorFromBtn(ref Button InBtn, ref Color ColourSymbol)
		{
			Color InColour = InBtn.ForeColor;
			if (SelectColor(ref InColour))
			{
				InBtn.ForeColor = InColour;
				ColourSymbol = InColour;
				return true;
			}
			return false;
		}

		public static bool SelectColorFromBtn(ref ToolStripButton InBtn, ref Color ColourSymbol)
		{
			Color InColour = InBtn.ForeColor;
			if (SelectColor(ref InColour))
			{
				InBtn.ForeColor = InColour;
				ColourSymbol = InColour;
				return true;
			}
			return false;
		}

		public static bool SelectColor(ref Color InColour)
		{
			ColorDialog colorDialog = new ColorDialog();
			colorDialog.Color = InColour;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				InColour = colorDialog.Color;
				return true;
			}
			return false;
		}

		public static bool SelectBackgroundColors(ref ToolStripButton InBtn, ref Color InColour1, ref Color InColour2, ref int InStyle, bool IsDefault)
		{
			ChangedBackColour1 = InColour1;
			ChangedBackColour2 = InColour2;
			ChangedBackStyle = InStyle;
			ChangedIsDefault = IsDefault;
			FrmBackground frmBackground = new FrmBackground();
			if (frmBackground.ShowDialog() == DialogResult.OK)
			{
				InColour1 = ChangedBackColour1;
				InColour2 = ChangedBackColour2;
				InStyle = ChangedBackStyle;
				InBtn.ForeColor = InColour1;
				return true;
			}
			return false;
		}

		public static void BuildFontsList(ref ToolStripComboBox InTSComboBox)
		{
			if (FontsListMaxIndex < 0)
			{
				InstalledFontCollection installedFontCollection = new InstalledFontCollection();
				FontFamily[] families = installedFontCollection.Families;
				FontsListMaxIndex = -1;
				FontFamily[] array = families;
				foreach (FontFamily fontFamily in array)
				{
					FontsList[++FontsListMaxIndex] = fontFamily.Name;
				}
			}
			if (FontsListMaxIndex >= 0)
			{
				InTSComboBox.Items.Clear();
				InTSComboBox.Sorted = false;
				for (int j = 0; j <= FontsListMaxIndex; j++)
				{
					InTSComboBox.Items.Add(FontsList[j]);
				}
				InTSComboBox.Sorted = true;
				InTSComboBox.SelectedIndex = 0;
			}
		}

		public static void BuildFontSizeList(ref ToolStripComboBox InCombo)
		{
			InCombo.Items.Clear();
			InCombo.Items.Add("8");
			InCombo.Items.Add("9");
			InCombo.Items.Add("10");
			InCombo.Items.Add("11");
			InCombo.Items.Add("12");
			InCombo.Items.Add("13");
			InCombo.Items.Add("14");
			InCombo.Items.Add("15");
			InCombo.Items.Add("16");
			InCombo.Items.Add("17");
			InCombo.Items.Add("18");
			InCombo.Items.Add("19");
			InCombo.Items.Add("20");
		}

		public static string GetDisplayNameOnly(ref string InFileName, bool UpdateByRef)
		{
			return GetDisplayNameOnly(ref InFileName, UpdateByRef, KeepExt: false);
		}

		public static string GetDisplayNameOnly(ref string InFileName, bool UpdateByRef, bool KeepExt)
		{
			if ((InFileName == null) | (InFileName == ""))
			{
				return "";
			}
			string text = "";
			try
			{
				text = ((!KeepExt) ? Path.GetFileNameWithoutExtension(InFileName) : Path.GetFileName(InFileName));
				if (UpdateByRef)
				{
					InFileName = text;
				}
			}
			catch
			{
			}
			return text;
		}

		public static bool LoadUnicodeStrokeCount1()
		{
			string InString = "";
			if (LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				int i = 1;
				for (int num = InString.Length - 2; i <= num - 2; i += 3)
				{
					int num2 = DataUtil.ObjToInt(InString.Substring(i, 2));
					if (num2 > 0)
					{
						int num3 = InString[i - 1];
						if (num3 < 0)
						{
							num3 += 65536;
						}
						if (num3 >= 0)
						{
							StrokeCount[num3] = num2;
						}
					}
				}
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		public static bool LoadUnicodeStrokeCount()
		{
			string InString = "";
			if (LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				ReadOnlySpan<char> roString = InString.AsSpan();
				int i = 1;
				for (int num = roString.Length - 2; i <= num - 2; i += 3)
				{
					//int num2 = DataUtil.ObjToInt(InString.Substring(i, 2));
					int num2 = int.Parse(roString.Slice(i, 2));
					if (num2 > 0)
					{
						int num3 = roString[i - 1];
						if (num3 < 0)
						{
							num3 += 65536;
						}
						if (num3 >= 0)
						{
							StrokeCount[num3] = num2;
						}
					}
				}
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		public static void UpdateV4RegDM()
		{
			if (DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AlwaysTryDualMonitor", -1)) >= 0)
			{
				DMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AlwaysTryDualMonitor", 1)) > 0) ? true : false);
				DualMonitorSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOption", 0));
				DMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOptionCustomLeft", 0));
				DMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOptionCustomTop", 0));
				DMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOptionCustomWidth", 1));
				RegUtil.SaveRegValue("monitors", "AlwaysTryDualMonitor", DMAlwaysUseSecondaryMonitor ? 1 : 0);
				RegUtil.SaveRegValue("monitors", "DualMonitorOption", DualMonitorSelectAutoOption);
				RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomLeft", DMOption1Left);
				RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomTop", DMOption1Top);
				RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomWidth", DMOption1Width);
				RegUtil.DeleletRegKey("options", "AlwaysTryDualMonitor");
				RegUtil.DeleletRegKey("options", "DualMonitorOption");
				RegUtil.DeleletRegKey("options", "DualMonitorOptionCustomLeft");
				RegUtil.DeleletRegKey("options", "DualMonitorOptionCustomTop");
				RegUtil.DeleletRegKey("options", "DualMonitorOptionCustomWidth");
			}
		}

		public static void LoadMusicExtArray()
		{
			TotalMediaFileExt = 0;
			LoadMusicExtArray("AudioExtensions.txt", MediaBackgroundStyle.Audio);
			LoadMusicExtArray("VideoExtensions.txt", MediaBackgroundStyle.Video);
		}

		public static void LoadMusicExtArray(string InFile, MediaBackgroundStyle InMediaType)
		{
			if (InMediaType != MediaBackgroundStyle.Video)
			{
				InMediaType = MediaBackgroundStyle.Audio;
			}
			string text = RootEasiSlidesDir + "Admin\\Database\\" + InFile;
			if (!File.Exists(text))
			{
				if (File.Exists(Application.StartupPath + "\\Sys\\" + InFile))
				{
					try
					{
						File.Copy(Application.StartupPath + "\\Sys\\" + InFile, text);
					}
					catch
					{
						FileUtil.CreateNewFile(text, FileUtil.FileContentsType.Ascii_Rtf);
					}
				}
				else
				{
					FileUtil.CreateNewFile(text, FileUtil.FileContentsType.Ascii_Rtf);
				}
			}
			using StreamReader streamReader = File.OpenText(text);
			string text2 = "";
			while ((text2 = streamReader.ReadLine()) != null)
			{
				text2 = DataUtil.TrimEnd(text2);
				if (text2 != "" && TotalMediaFileExt < 3000 && ValidateMusicExt(ref text2, ShowMessage: false))
				{
					MediaFileExtension[TotalMediaFileExt, 0] = text2.ToLower();
					MediaFileExtension[TotalMediaFileExt, 1] = InMediaType.ToString();
					TotalMediaFileExt++;
				}
			}
			//streamReader.Close();
		}

		public static void ComputeShowLineSpacing()
		{
			for (int i = 0; i < 41; i++)
			{
				MainFontSpacingFactor[i, 0] = ShowLineSpacing[i, 0] + 0.05;
				MainFontSpacingFactor[i, 1] = ShowLineSpacing[i, 1] + 0.05;
			}
		}

		public static void LoadLicAdminDetails()
		{
			LicAdminEnforceDisplay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("config", "licEnforceDisplay", 1)) > 0) ? true : false);
			LicAdminNoSymbol = RegUtil.GetRegValue("config", "licNoSym", "#");
			LicAdmin_List[0, 0] = "1";
			LicAdmin_List[1, 0] = "None";
			LicAdmin_List[2, 0] = "Public Domain";
			LicAdmin_List[3, 0] = "CCLI";
			int num = 0;
			for (num = 4; num < 9; num++)
			{
				LicAdmin_List[num, 0] = "";
			}
			string fullSearchString = "select * from LICENCE where Ref >=4 and Ref < " + DataUtil.ObjToString(9);
#if OleDb
			DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
			using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
			if (datatable.Rows.Count > 0)
			{
				//recordSet.MoveFirst();
				//while (!recordSet.EOF)
				foreach (DataRow dr in datatable.Rows)
				{
					num = DataUtil.GetDataInt(dr, "Ref");
					LicAdmin_List[num, 0] = DataUtil.Trim(DataUtil.GetDataString(dr, "ADMINISTRATOR"));
					//recordSet.MoveNext();
				}
			}
			LicAdmin_List[1, 1] = "";
			LicAdmin_List[2, 1] = "Public Domain";
			LicAdmin_List[3, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "licCCLI_no", ""));
			LicAdmin_List[4, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic4_no", ""));
			LicAdmin_List[5, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic5_no", ""));
			LicAdmin_List[6, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic6_no", ""));
			LicAdmin_List[7, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic7_no", ""));
			LicAdmin_List[8, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic8_no", ""));
			LicAdmin_List[1, 2] = "";
			LicAdmin_List[2, 2] = "Public Domain";
			for (num = 3; num < 9; num++)
			{
				if ((LicAdmin_List[num, 0] == "") | (LicAdmin_List[num, 1] == ""))
				{
					LicAdmin_List[num, 2] = "";
				}
				else
				{
					LicAdmin_List[num, 2] = LicAdmin_List[num, 0] + LicAdminNoSymbol + LicAdmin_List[num, 1];
				}
			}
			LicAdmin_List[0, 1] = LicAdmin_List[1, 1];
			LicAdmin_List[0, 2] = LicAdmin_List[1, 2];
		}

#if OleDb
		public static void SaveFoldersSettings4()
        {
            int num = 0;
            if (ValidateDefaultFolders(0))
            {
                try
                {
                    string text = "";
                    Database daoDb = DbDaoController.GetDaoDb(ConnectStringMainDB);
                    Recordset recordset = null;
					for (int i = 1; i < 41; i++)
                    {
                        num = i;
                        if (FolderName[i] != "")
                        {
                            text = "select * from Folder where FolderNo=" + i;
                            recordset = DbDaoController.GetRecordSet(daoDb, text);
                            recordset.Edit();
                            recordset.Fields["name"].Value = FolderName[i];
                            recordset.Fields["Use"].Value = FolderUse[i];
                            recordset.Fields["GroupStyle"].Value = FolderGroupStyle[i];
                            recordset.Fields["PreChorusHeading"].Value = FolderLyricsHeading[i, 0];
                            recordset.Fields["ChorusHeading"].Value = FolderLyricsHeading[i, 1];
                            recordset.Fields["BridgeHeading"].Value = FolderLyricsHeading[i, 2];
                            recordset.Fields["EndingHeading"].Value = FolderLyricsHeading[i, 3];
                            recordset.Fields["BIUHeading"].Value = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
                            recordset.Fields["HeadingSize"].Value = FolderHeadingPercentSize[i];
                            recordset.Fields["HeadingOption"].Value = FolderHeadingOption[i];
                            recordset.Fields["LineSpacing"].Value = ShowLineSpacing[i, 0];
                            recordset.Fields["LineSpacing2"].Value = ShowLineSpacing[i, 1];
                            recordset.Fields["BIU0"].Value = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
                            recordset.Fields["Size0"].Value = ShowFontSize[i, 0];
                            recordset.Fields["FontName0"].Value = ShowFontName[i, 0];
                            recordset.Fields["Vpos0"].Value = ShowFontVPosition[i, 0];
                            recordset.Fields["BIU1"].Value = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
                            recordset.Fields["Size1"].Value = ShowFontSize[i, 1];
                            recordset.Fields["FontName1"].Value = ShowFontName[i, 1];
                            recordset.Fields["Vpos1"].Value = ShowFontVPosition[i, 1];
                            recordset.Fields["LMargin"].Value = ShowLeftMargin[i];
                            recordset.Fields["RMargin"].Value = ShowRightMargin[i];
                            recordset.Fields["BMargin"].Value = ShowBottomMargin[i];
                            recordset.Update();
                            recordset.Close();
                        }
                    }
                    if (recordset != null)
                    {
                        recordset = null;
                    }
					if (daoDb != null)
					{
						daoDb.Close();
						daoDb = null;
					}

				}
                catch
                {
                    MessageBox.Show("Error: Cannot Save Folder Settings for Folder Index: " + num);
                }
            }
        }

#elif SQLite
		public static void SaveFoldersSettings4()
		{
			int num = 0;
			if (ValidateDefaultFolders(0))
			{
				try
				{
					string sQuery = "select * from Folder";

					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
					using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

					DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["FolderNo"] };
					dataTable.PrimaryKey = primarykey;

					for (int i = 1; i < 41; i++)
					{
						num = i;
						if (FolderName[i] != "")
						{
							DataRow dr = dataTable.Rows.Find($"{i}");
							if (dr != null)
							{
								dr["name"] = FolderName[i];
								dr["Use"] = FolderUse[i];
								dr["GroupStyle"] = FolderGroupStyle[i];
								dr["PreChorusHeading"] = FolderLyricsHeading[i, 0];
								dr["ChorusHeading"] = FolderLyricsHeading[i, 1];
								dr["BridgeHeading"] = FolderLyricsHeading[i, 2];
								dr["EndingHeading"] = FolderLyricsHeading[i, 3];
								dr["BIUHeading"] = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
								dr["HeadingSize"] = FolderHeadingPercentSize[i];
								dr["HeadingOption"] = FolderHeadingOption[i];
								dr["LineSpacing"] = ShowLineSpacing[i, 0];
								dr["LineSpacing2"] = ShowLineSpacing[i, 1];
								dr["BIU0"] = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
								dr["Size0"] = ShowFontSize[i, 0];
								dr["FontName0"] = ShowFontName[i, 0];
								dr["Vpos0"] = ShowFontVPosition[i, 0];
								dr["BIU1"] = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
								dr["Size1"] = ShowFontSize[i, 1];
								dr["FontName1"] = ShowFontName[i, 1];
								dr["Vpos1"] = ShowFontVPosition[i, 1];
								dr["LMargin"] = ShowLeftMargin[i];
								dr["RMargin"] = ShowRightMargin[i];
								dr["BMargin"] = ShowBottomMargin[i];
							}

						}
					}

					DbController.UpdateTable(connection, sQuery, dataTable);
				}
				catch
				{
					MessageBox.Show("Error: Cannot Save Folder Settings for Folder Index: " + num);
				}
			}
		}
#endif

#if OleDb
		public static void SaveFoldersSettings()
		{
			int num = 0;
			int rowIndex = 0;

			if (ValidateDefaultFolders(0))
			{
				try
				{
					string text = "";
					using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
					{
						OleDbDataAdapter da = null;
						DataSet ds = null;
						DataTable dt = null;

						text = "select * from Folder where FolderNo > 0 and FolderNo <= 41";

						(da, ds) = DbOleDbController.getDataAdapter(daoDb, text);
						dt = ds.Tables[0];
						if (dt.Rows.Count > 0)
						{
							for (int i = 1; i < 41; i++)
							{
								num = i;
								rowIndex = i - 1;
								if (FolderName[i] != "")
								{
									dt.Rows[rowIndex]["name"] = FolderName[i];
									dt.Rows[rowIndex]["Use"] = FolderUse[i];
									dt.Rows[rowIndex]["GroupStyle"] = FolderGroupStyle[i];
									dt.Rows[rowIndex]["PreChorusHeading"] = FolderLyricsHeading[i, 0];
									dt.Rows[rowIndex]["ChorusHeading"] = FolderLyricsHeading[i, 1];
									dt.Rows[rowIndex]["BridgeHeading"] = FolderLyricsHeading[i, 2];
									dt.Rows[rowIndex]["EndingHeading"] = FolderLyricsHeading[i, 3];
									dt.Rows[rowIndex]["BIUHeading"] = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
									dt.Rows[rowIndex]["HeadingSize"] = FolderHeadingPercentSize[i];
									dt.Rows[rowIndex]["HeadingOption"] = FolderHeadingOption[i];
									dt.Rows[rowIndex]["LineSpacing"] = ShowLineSpacing[i, 0];
									dt.Rows[rowIndex]["LineSpacing2"] = ShowLineSpacing[i, 1];
									dt.Rows[rowIndex]["BIU0"] = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
									dt.Rows[rowIndex]["Size0"] = ShowFontSize[i, 0];
									dt.Rows[rowIndex]["FontName0"] = ShowFontName[i, 0];
									dt.Rows[rowIndex]["Vpos0"] = ShowFontVPosition[i, 0];
									dt.Rows[rowIndex]["BIU1"] = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
									dt.Rows[rowIndex]["Size1"] = ShowFontSize[i, 1];
									dt.Rows[rowIndex]["FontName1"] = ShowFontName[i, 1];
									dt.Rows[rowIndex]["Vpos1"] = ShowFontVPosition[i, 1];
									dt.Rows[rowIndex]["LMargin"] = ShowLeftMargin[i];
									dt.Rows[rowIndex]["RMargin"] = ShowRightMargin[i];
									dt.Rows[rowIndex]["BMargin"] = ShowBottomMargin[i];									
								}
							}
							da.Update(dt);
							dt.Dispose();
							da.Dispose();
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}

#elif SQLite
		public static void SaveFoldersSettings()
		{
			int num = 0;
			int rowIndex = 0;

			if (ValidateDefaultFolders(0))
			{
				try
				{
					string text = "";
					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);

					DbDataAdapter sQLiteDataAdapter = null;
					DataTable dataTable = null;

					text = "select * from Folder where FolderNo > 0 and FolderNo <= 41";
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, text);

					if (dataTable.Rows.Count > 0)
					{
						for (int i = 1; i < 41; i++)
						{
							num = i;
							rowIndex = i - 1;
							if (FolderName[i] != "")
							{
								dataTable.Rows[rowIndex]["name"] = FolderName[i];
								dataTable.Rows[rowIndex]["Use"] = FolderUse[i];
								dataTable.Rows[rowIndex]["GroupStyle"] = FolderGroupStyle[i];
								dataTable.Rows[rowIndex]["PreChorusHeading"] = FolderLyricsHeading[i, 0];
								dataTable.Rows[rowIndex]["ChorusHeading"] = FolderLyricsHeading[i, 1];
								dataTable.Rows[rowIndex]["BridgeHeading"] = FolderLyricsHeading[i, 2];
								dataTable.Rows[rowIndex]["EndingHeading"] = FolderLyricsHeading[i, 3];
								dataTable.Rows[rowIndex]["BIUHeading"] = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
								dataTable.Rows[rowIndex]["HeadingSize"] = FolderHeadingPercentSize[i];
								dataTable.Rows[rowIndex]["HeadingOption"] = FolderHeadingOption[i];
								dataTable.Rows[rowIndex]["LineSpacing"] = ShowLineSpacing[i, 0];
								dataTable.Rows[rowIndex]["LineSpacing2"] = ShowLineSpacing[i, 1];
								dataTable.Rows[rowIndex]["BIU0"] = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
								dataTable.Rows[rowIndex]["Size0"] = ShowFontSize[i, 0];
								dataTable.Rows[rowIndex]["FontName0"] = ShowFontName[i, 0];
								dataTable.Rows[rowIndex]["Vpos0"] = ShowFontVPosition[i, 0];
								dataTable.Rows[rowIndex]["BIU1"] = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
								dataTable.Rows[rowIndex]["Size1"] = ShowFontSize[i, 1];
								dataTable.Rows[rowIndex]["FontName1"] = ShowFontName[i, 1];
								dataTable.Rows[rowIndex]["Vpos1"] = ShowFontVPosition[i, 1];
								dataTable.Rows[rowIndex]["LMargin"] = ShowLeftMargin[i];
								dataTable.Rows[rowIndex]["RMargin"] = ShowRightMargin[i];
								dataTable.Rows[rowIndex]["BMargin"] = ShowBottomMargin[i];
							}
						}
						DbController.UpdateTable(connection, text, dataTable);
					}

					if (dataTable != null)
						dataTable.Dispose();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}
#endif

		public static void SaveOptionsData()
		{
			RegUtil.SaveRegValue("options", "PrinterSpaces", PB_PrinterSpaces);
			RegUtil.SaveRegValue("options", "UseSongNumbers", UseSongNumbers ? 1 : 0);
			RegUtil.SaveRegValue("options", "BibleMaxSelectVerses", HB_MaxVersesSelection);
			RegUtil.SaveRegValue("options", "BibleMaxAdhocVersesSelection", HB_MaxAdhocVersesSelection);
			RegUtil.SaveRegValue("options", "BibleShowVerses", 1);
			RegUtil.SaveRegValue("options", "PowerpointMaxFiles", PP_MaxFiles);
			RegUtil.SaveRegValue("options", "AutoTextOverflow", AutoTextOverflow ? 1 : 0);
			RegUtil.SaveRegValue("options", "UseLargestFontSize", UseLargestFontSize ? 1 : 0);
			RegUtil.SaveRegValue("options", "UsePowerpointTab", UsePowerpointTab ? 1 : 0);
			RegUtil.SaveRegValue("options", "NoPowerpointPanelOverlay", NoPowerpointPanelOverlay ? 1 : 0);
			RegUtil.SaveRegValue("options", "UseMediaTab", UseMediaTab ? 1 : 0);
			RegUtil.SaveRegValue("options", "ShowLyricsMonitorAlertBox", ShowLyricsMonitorAlertBox ? 1 : 0);
			RegUtil.SaveRegValue("options", "NoMediaPanelOverlay", NoMediaPanelOverlay ? 1 : 0);
			RegUtil.SaveRegValue("options", "RotateGap", ShowRotateGap);
			RegUtil.SaveRegValue("options", "AdvanceNextItem", AdvanceNextItem ? 1 : 0);
			RegUtil.SaveRegValue("options", "LineBetweenRegions", LineBetweenRegions ? 1 : 0);
			RegUtil.SaveRegValue("options", "GapItemOption", (int)GapItemOption);
			RegUtil.SaveRegValue("options", "GapItemLogoFile", GapItemLogoFile);
			RegUtil.SaveRegValue("options", "GapItemUseFade", GapItemUseFade ? 1 : 0);
			RegUtil.SaveRegValue("options", "WordWrapLeftAlignIndent", WordWrapLeftAlignIndent ? 1 : 0);
			RegUtil.SaveRegValue("options", "WordWrapIgnoreStartSpaces", WordWrapIgnoreStartSpaces);
			RegUtil.SaveRegValue("options", "AutoRotateOn", AutoRotateOn ? 1 : 0);
			RegUtil.SaveRegValue("options", "AutoRotateStyle", AutoRotateStyle);
			RegUtil.SaveRegValue("options", "NotationFontFactor", (int)(NotationFontFactor * 100.0));
			RegUtil.SaveRegValue("options", "ExternalListing", PowerpointListingStyle);
			RegUtil.SaveRegValue("options", "KeyBoardOption", KeyBoardOption);
            // Global Hook F7, F8 (panel black) ?占쏙옙?
            RegUtil.SaveRegValue("options", "GlobalHookKey_F7", GlobalHookKey_F7 ? 1 : 0);
            RegUtil.SaveRegValue("options", "GlobalHookKey_F8", GlobalHookKey_F8 ? 1 : 0);

            // Global Hook F9, F10 (panel black) ?占쏙옙?
            RegUtil.SaveRegValue("options", "GlobalHookKey_F9", GlobalHookKey_F9 ? 1 : 0);
            RegUtil.SaveRegValue("options", "GlobalHookKey_F10", GlobalHookKey_F10 ? 1 : 0);

            // Global Hook Arrow, CtrlArrow (panel black) ?占쏙옙?
            RegUtil.SaveRegValue("options", "GlobalHookKey_Arrow", GlobalHookKey_Arrow ? 1 : 0);
            RegUtil.SaveRegValue("options", "GlobalHookKey_CtrlArrow", GlobalHookKey_CtrlArrow ? 1 : 0);

            RegUtil.SaveRegValue("options", "OutputmonitorName", OutputMonitorName);
			RegUtil.SaveRegValue("options", "LyricsMonitorName", LyricsMonitorName);

			RegUtil.SaveRegValue("options", "PreviewAreaShowNotations", PreviewArea_ShowNotations ? 1 : 0);
			RegUtil.SaveRegValue("options", "PreviewAreaLineBetweenScreens", PreviewArea_LineBetweenScreens ? 1 : 0);
			RegUtil.SaveRegValue("options", "PreviewAreaFontSize", PreviewArea_FontSize);
			RegUtil.SaveRegValue("options", "ParentalAlertHeading", ParentalAlertHeading);
			RegUtil.SaveRegValue("options", "ParentalAlertDuration", ParentalAlertDuration);
			RegUtil.SaveRegValue("options", "ParentalAlertTextColour", ParentalAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "ParentalAlertBackColour", ParentalAlertBackColour.ToArgb());
			RegUtil.SaveRegValue("options", "ParentalAlertTextAlign", ParentalAlertTextAlign);
			RegUtil.SaveRegValue("options", "ParentalAlertVerticalAlign", ParentalAlertVerticalAlign);
			int num = 0;
			num = (ParentalAlertScroll ? 1 : 0) + (ParentalAlertFlash ? 1 : 0) * 2 + (ParentalAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "ParentalAlertStyle", num);
			RegUtil.SaveRegValue("options", "ParentalAlertFontName", ParentalAlertFontName);
			RegUtil.SaveRegValue("options", "ParentalAlertFontSize", ParentalAlertFontSize);
			RegUtil.SaveRegValue("options", "ParentalAlertFontFormat", ParentalAlertFontFormat);
			RegUtil.SaveRegValue("options", "MessageAlertDuration", MessageAlertDuration);
			RegUtil.SaveRegValue("options", "MessageAlertTextAlign", MessageAlertTextAlign);
			RegUtil.SaveRegValue("options", "MessageAlertVerticalAlign", MessageAlertVerticalAlign);
			RegUtil.SaveRegValue("options", "MessageAlertTextColour", MessageAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "MessageAlertBackColour", MessageAlertBackColour.ToArgb());
			num = (MessageAlertScroll ? 1 : 0) + (MessageAlertFlash ? 1 : 0) * 2 + (MessageAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "MessageAlertStyle", num);
			RegUtil.SaveRegValue("options", "MessageAlertFontName", MessageAlertFontName);
			RegUtil.SaveRegValue("options", "MessageAlertFontSize", MessageAlertFontSize);
			RegUtil.SaveRegValue("options", "MessageAlertFontFormat", MessageAlertFontFormat);
			RegUtil.SaveRegValue("options", "ReferenceAlertDuration", ReferenceAlertDuration);
			RegUtil.SaveRegValue("options", "ReferenceAlertTextAlign", ReferenceAlertTextAlign);
			RegUtil.SaveRegValue("options", "ReferenceAlertVerticalAlign", ReferenceAlertVerticalAlign);
			RegUtil.SaveRegValue("options", "ReferenceAlertTextColour", ReferenceAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "ReferenceAlertBackColour", ReferenceAlertBackColour.ToArgb());
			num = (ReferenceAlertScroll ? 1 : 0) + (ReferenceAlertFlash ? 1 : 0) * 2 + (ReferenceAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "ReferenceAlertStyle", num);
			RegUtil.SaveRegValue("options", "ReferenceAlertFontName", ReferenceAlertFontName);
			RegUtil.SaveRegValue("options", "ReferenceAlertFontSize", ReferenceAlertFontSize);
			RegUtil.SaveRegValue("options", "ReferenceAlertFontFormat", ReferenceAlertFontFormat);
			RegUtil.SaveRegValue("options", "ReferenceAlertUsePick", ReferenceAlertUsePick ? 1 : 0);
			RegUtil.SaveRegValue("options", "ReferenceAlertBlankIfPickNotFound", ReferenceAlertBlankIfPickNotFound ? 1 : 0);
			RegUtil.SaveRegValue("options", "ReferenceAlertSource", ReferenceAlertSource);
			RegUtil.SaveRegValue("options", "ReferenceAlertPickName", ReferenceAlertPickName);
			RegUtil.SaveRegValue("options", "ReferenceAlertPickSubstitute", ReferenceAlertPickSubstitute);
			RegUtil.SaveRegValue("options", "ReferenceAlertPickSeparator", ReferenceAlertPickSeparator);
			RegUtil.SaveRegValue("options", "FocusedBackColour", FocusedTextRegionColour.ToArgb());
			RegUtil.SaveRegValue("options", "SelectedSlideTextColour", TextRegionSlideTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "SelectedSlideBackColour", TextRegionSlideBackColour.ToArgb());
			RegUtil.SaveRegValue("options", "UseFocusedBackColour", UseFocusedTextRegionColour ? 1 : 0);
			RegUtil.SaveRegValue("options", "JumpToA", JumpToA);
			RegUtil.SaveRegValue("options", "JumpToB", JumpToB);
			RegUtil.SaveRegValue("options", "JumpToC", JumpToC);
			RegUtil.SaveRegValue("options", "FindItemInTitle", FindItemInTitle ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInContents", FindItemInContents ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInSongNumber", FindItemInSongNumber ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInBookRef", FindItemInBookRef ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInUserRef", FindItemInUserRef ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInLicAdmin", FindItemInLicAdmin ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInWriter", FindItemInWriter ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInCopyright", FindItemInCopyright ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInTitle", FindItemInTitle ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemUseDates", FindItemUseDates ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemDateFrom", FindItemDateFrom.ToString());
			RegUtil.SaveRegValue("options", "FindItemDateTo", FindItemDateTo.ToString());
			RegUtil.SaveRegValue("options", "OutlineFontSizeThreshold", OutlineFontSizeThreshold);

            RegUtil.SaveRegValue("monitors", "AlwaysTryDualMonitor", DMAlwaysUseSecondaryMonitor ? 1 : 0);

            // daniel
            // 紐⑤땲??Wide ?占쏀겕占?Mode
            RegUtil.SaveRegValue("monitors", "IsMonitorWide", isScreenWideMode ? 1: 0);
			RegUtil.SaveRegValue("monitors", "DualMonitorOption", DualMonitorSelectAutoOption);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomLeft", DMOption1Left);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomTop", DMOption1Top);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomWidth", DMOption1Width);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomAsSingle", DMOption1AsSingleMonitor ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "DisableSreenSaver", DisableSreenSaver ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "VideoSize", VideoSize);
			RegUtil.SaveRegValue("monitors", "VideoVAlign", VideoVAlign);
			RegUtil.SaveRegValue("monitors", "AlwaysTrySecondaryLyricsMonitor", LMAlwaysUseSecondaryMonitor ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOption", LMSelectAutoOption);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOptionCustomLeft", LMOption1Left);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOptionCustomTop", LMOption1Top);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOptionCustomWidth", LMOption1Width);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorTextColour", LMTextColour.ToArgb());
			RegUtil.SaveRegValue("monitors", "LyricsMonitorHighlightColour", LMHighlightColour.ToArgb());
			RegUtil.SaveRegValue("monitors", "LyricsMonitorBackColour", LMBackColour.ToArgb());
			RegUtil.SaveRegValue("monitors", "LyricsMonitorShowNotations", LMShowNotations ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorFontSize", LMMainFontSize);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorNotationsFontSize", LMNotationsFontSize);
			RegUtil.SaveRegValue("options", "LyricsMonitorFontFormat", LMFontFormat);
			RegUtil.SaveRegValue("monitors", "LiveCamNumber", LiveCamNumber);
			RegUtil.SaveRegValue("monitors", "LiveCamVolume", LiveCamVolume);
			RegUtil.SaveRegValue("monitors", "LiveCamBalance", LiveCamBalance);
			RegUtil.SaveRegValue("monitors", "LiveCamWidescreen", LiveCamWidescreen ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LiveCamNoPanelOverlay", LiveCamNoPanelOverlay ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LiveCamMute", LiveCamMute ? 1 : 0);


        }

		public static void LoadSongKeyCapoTiming(ref ComboBox SongCapo, ref ComboBox SongKey, ref ComboBox SongTiming)
		{
			SongCapo.Items.Clear();
			SongCapo.Items.Add("");
			SongCapo.Items.Add("Capo 0");
			SongCapo.Items.Add("Capo 1");
			SongCapo.Items.Add("Capo 2");
			SongCapo.Items.Add("Capo 3");
			SongCapo.Items.Add("Capo 4");
			SongCapo.Items.Add("Capo 5");
			SongCapo.Items.Add("Capo 6");
			SongCapo.Items.Add("Capo 7");
			SongCapo.Items.Add("Capo 8");
			SongCapo.Items.Add("Capo 9");
			SongCapo.Items.Add("Capo 10");
			SongCapo.Items.Add("Capo 11");
			SongKey.Items.Clear();
			SongKey.Items.Add("");
			SongKey.Items.Add("A");
			SongKey.Items.Add("B");
			SongKey.Items.Add("C");
			SongKey.Items.Add("D");
			SongKey.Items.Add("E");
			SongKey.Items.Add("F");
			SongKey.Items.Add("G");
			SongKey.Items.Add("Am");
			SongKey.Items.Add("Bm");
			SongKey.Items.Add("Cm");
			SongKey.Items.Add("Dm");
			SongKey.Items.Add("Em");
			SongKey.Items.Add("Fm");
			SongKey.Items.Add("Gm");
			SongKey.Items.Add("Ab");
			SongKey.Items.Add("Bb");
			SongKey.Items.Add("Db");
			SongKey.Items.Add("Eb");
			SongKey.Items.Add("F#");
			SongKey.Items.Add("Bbm");
			SongKey.Items.Add("C#m");
			SongKey.Items.Add("D#m");
			SongKey.Items.Add("F#m");
			SongKey.Items.Add("G#m");
			SongTiming.Items.Clear();
			SongTiming.Items.Add("");
			SongTiming.Items.Add("3/4");
			SongTiming.Items.Add("4/4");
		}

		public static void GenerateMusicKeysList()
		{
			MusicMajorKeys[0] = "G";
			MusicMajorKeys[1] = "Ab";
			MusicMajorKeys[2] = "A";
			MusicMajorKeys[3] = "Bb";
			MusicMajorKeys[4] = "B";
			MusicMajorKeys[5] = "C";
			MusicMajorKeys[6] = "Db";
			MusicMajorKeys[7] = "D";
			MusicMajorKeys[8] = "Eb";
			MusicMajorKeys[9] = "E";
			MusicMajorKeys[10] = "F";
			MusicMajorKeys[11] = "F#";
			MusicMajorKeysFlatSharp[0] = 1;
			MusicMajorKeysFlatSharp[1] = 0;
			MusicMajorKeysFlatSharp[2] = 1;
			MusicMajorKeysFlatSharp[3] = 0;
			MusicMajorKeysFlatSharp[4] = 1;
			MusicMajorKeysFlatSharp[5] = 1;
			MusicMajorKeysFlatSharp[6] = 0;
			MusicMajorKeysFlatSharp[7] = 1;
			MusicMajorKeysFlatSharp[8] = 0;
			MusicMajorKeysFlatSharp[9] = 1;
			MusicMajorKeysFlatSharp[10] = 0;
			MusicMajorKeysFlatSharp[11] = 1;
			MusicMinorKeys[0] = "Gm";
			MusicMinorKeys[1] = "G#m";
			MusicMinorKeys[2] = "Am";
			MusicMinorKeys[3] = "Bbm";
			MusicMinorKeys[4] = "Bm";
			MusicMinorKeys[5] = "Cm";
			MusicMinorKeys[6] = "C#m";
			MusicMinorKeys[7] = "Dm";
			MusicMinorKeys[8] = "D#m";
			MusicMinorKeys[9] = "Em";
			MusicMinorKeys[10] = "Fm";
			MusicMinorKeys[11] = "F#m";
			MusicMinorKeysFlatSharp[0] = 0;
			MusicMinorKeysFlatSharp[1] = 1;
			MusicMinorKeysFlatSharp[2] = 1;
			MusicMinorKeysFlatSharp[3] = 0;
			MusicMinorKeysFlatSharp[4] = 1;
			MusicMinorKeysFlatSharp[5] = 0;
			MusicMinorKeysFlatSharp[6] = 1;
			MusicMinorKeysFlatSharp[7] = 0;
			MusicMinorKeysFlatSharp[8] = 1;
			MusicMinorKeysFlatSharp[9] = 1;
			MusicMinorKeysFlatSharp[10] = 0;
			MusicMinorKeysFlatSharp[11] = 1;
			MusicMajorChords[0, 0] = "G";
			MusicMajorChords[1, 0] = "Ab";
			MusicMajorChords[2, 0] = "A";
			MusicMajorChords[3, 0] = "Bb";
			MusicMajorChords[4, 0] = "B";
			MusicMajorChords[5, 0] = "C";
			MusicMajorChords[6, 0] = "Db";
			MusicMajorChords[7, 0] = "D";
			MusicMajorChords[8, 0] = "Eb";
			MusicMajorChords[9, 0] = "E";
			MusicMajorChords[10, 0] = "F";
			MusicMajorChords[11, 0] = "Gb";
			MusicMajorChords[0, 1] = "G";
			MusicMajorChords[1, 1] = "G#";
			MusicMajorChords[2, 1] = "A";
			MusicMajorChords[3, 1] = "A#";
			MusicMajorChords[4, 1] = "B";
			MusicMajorChords[5, 1] = "C";
			MusicMajorChords[6, 1] = "C#";
			MusicMajorChords[7, 1] = "D";
			MusicMajorChords[8, 1] = "D#";
			MusicMajorChords[9, 1] = "E";
			MusicMajorChords[10, 1] = "F";
			MusicMajorChords[11, 1] = "F#";
			MusicMinorChords[0, 0] = "Gm";
			MusicMinorChords[1, 0] = "Abm";
			MusicMinorChords[2, 0] = "Am";
			MusicMinorChords[3, 0] = "Bbm";
			MusicMinorChords[4, 0] = "Bm";
			MusicMinorChords[5, 0] = "Cm";
			MusicMinorChords[6, 0] = "Dbm";
			MusicMinorChords[7, 0] = "Dm";
			MusicMinorChords[8, 0] = "Ebm";
			MusicMinorChords[9, 0] = "Em";
			MusicMinorChords[10, 0] = "Fm";
			MusicMinorChords[11, 0] = "Gbm";
			MusicMinorChords[0, 1] = "Gm";
			MusicMinorChords[1, 1] = "G#m";
			MusicMinorChords[2, 1] = "Am";
			MusicMinorChords[3, 1] = "A#m";
			MusicMinorChords[4, 1] = "Bm";
			MusicMinorChords[5, 1] = "Cm";
			MusicMinorChords[6, 1] = "C#m";
			MusicMinorChords[7, 1] = "Dm";
			MusicMinorChords[8, 1] = "D#m";
			MusicMinorChords[9, 1] = "Em";
			MusicMinorChords[10, 1] = "Fm";
			MusicMinorChords[11, 1] = "F#m";
		}

		public static void OldGenerateMusicKeysList()
		{
			MusicMajorKeys[0] = "G";
			MusicMajorKeys[1] = "Ab";
			MusicMajorKeys[2] = "A";
			MusicMajorKeys[3] = "Bb";
			MusicMajorKeys[4] = "B";
			MusicMajorKeys[5] = "C";
			MusicMajorKeys[6] = "Db";
			MusicMajorKeys[7] = "D";
			MusicMajorKeys[8] = "Eb";
			MusicMajorKeys[9] = "E";
			MusicMajorKeys[10] = "F";
			MusicMajorKeys[11] = "Gb";
			MusicMinorKeys[0] = "Gm";
			MusicMinorKeys[1] = "Abm";
			MusicMinorKeys[2] = "Am";
			MusicMinorKeys[3] = "Bbm";
			MusicMinorKeys[4] = "Bm";
			MusicMinorKeys[5] = "Cm";
			MusicMinorKeys[6] = "Dbm";
			MusicMinorKeys[7] = "Dm";
			MusicMinorKeys[8] = "Ebm";
			MusicMinorKeys[9] = "Em";
			MusicMinorKeys[10] = "Fm";
			MusicMinorKeys[11] = "Gbm";
			MusicMajorChords[0, 0] = "G";
			MusicMajorChords[1, 0] = "Ab";
			MusicMajorChords[2, 0] = "A";
			MusicMajorChords[3, 0] = "Bb";
			MusicMajorChords[4, 0] = "B";
			MusicMajorChords[5, 0] = "C";
			MusicMajorChords[6, 0] = "Db";
			MusicMajorChords[7, 0] = "D";
			MusicMajorChords[8, 0] = "Eb";
			MusicMajorChords[9, 0] = "E";
			MusicMajorChords[10, 0] = "F";
			MusicMajorChords[11, 0] = "Gb";
			MusicMajorChords[0, 1] = "G";
			MusicMajorChords[1, 1] = "G#";
			MusicMajorChords[2, 1] = "A";
			MusicMajorChords[3, 1] = "A#";
			MusicMajorChords[4, 1] = "B";
			MusicMajorChords[5, 1] = "C";
			MusicMajorChords[6, 1] = "C#";
			MusicMajorChords[7, 1] = "D";
			MusicMajorChords[8, 1] = "D#";
			MusicMajorChords[9, 1] = "E";
			MusicMajorChords[10, 1] = "F";
			MusicMajorChords[11, 1] = "F#";
			MusicMinorChords[0, 0] = "Gm";
			MusicMinorChords[1, 0] = "Abm";
			MusicMinorChords[2, 0] = "Am";
			MusicMinorChords[3, 0] = "Bbm";
			MusicMinorChords[4, 0] = "Bm";
			MusicMinorChords[5, 0] = "Cm";
			MusicMinorChords[6, 0] = "Dbm";
			MusicMinorChords[7, 0] = "Dm";
			MusicMinorChords[8, 0] = "Ebm";
			MusicMinorChords[9, 0] = "Em";
			MusicMinorChords[10, 0] = "Fm";
			MusicMinorChords[11, 0] = "Gbm";
			MusicMinorChords[0, 1] = "Gm";
			MusicMinorChords[1, 1] = "G#m";
			MusicMinorChords[2, 1] = "Am";
			MusicMinorChords[3, 1] = "A#m";
			MusicMinorChords[4, 1] = "Bm";
			MusicMinorChords[5, 1] = "Cm";
			MusicMinorChords[6, 1] = "C#m";
			MusicMinorChords[7, 1] = "Dm";
			MusicMinorChords[8, 1] = "D#m";
			MusicMinorChords[9, 1] = "Em";
			MusicMinorChords[10, 1] = "Fm";
			MusicMinorChords[11, 1] = "F#m";
		}

		public static void SingleArraySort(string[] InArray)
		{
			SingleArraySort(InArray, SortAscending: true);
		}

		public static void SingleArraySort(string[] InArray, bool SortAscending)
		{
			Array.Sort(InArray);
			if (!SortAscending)
			{
				Array.Reverse(InArray);
			}
		}

		public static void BuildMusicFilesListArray(string FolderPath, bool StoreDirPath)
		{
			if (FolderPath == "" || !MusicBuildContinue || (!Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\")))
			{
				return;
			}
			MusicBuildLapseTime = DateTime.Now.Subtract(MusicBuildStartTime);
			if (MusicBuildLapseTime.Seconds > 10)
			{
				MusicBuildContinue = false;
				return;
			}
			string[] array;
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				try
				{
					string[] files = Directory.GetFiles(FolderPath, "*" + MediaFileExtension[i, 0]);
					array = files;
					foreach (string text in array)
					{
						string InFileName = text;
						MediaFilesList[TotalMusicFiles, 0] = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
						MediaFilesList[TotalMusicFiles, 1] = MediaFileExtension[i, 0];
						int iLen = InFileName.Length - (MediaFilesList[TotalMusicFiles, 0].Length + MediaFilesList[TotalMusicFiles, 1].Length);
						MediaFilesList[TotalMusicFiles, 2] = DataUtil.Left(InFileName, iLen);
						TotalMusicFiles++;
					}
				}
				catch
				{
				}
			}
			// Daniel
			// C:\EasiSlides\Media\
			if (!FolderPath.EndsWith(@"\Media\"))
			{
				gf.MediaDir = @"C:\EasiSlides\Media\";
                FolderPath = gf.MediaDir;
			}

            string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			array = directories;
			foreach (string str in array)
			{
				BuildMusicFilesListArray(str + "\\", StoreDirPath);
			}
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2)
		{
			string DirPath = "";
			string FileName = "";
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath)
		{
			string FileName = "";
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName, bool StoreDirPath)
		{
			if (StoreDirPath & (TotalMusicFiles < 1))
			{
				return "";
			}
			string text = "";
			for (int i = 0; i <= 1; i++)
			{
				text = ((i == 0) ? MusicTitle1 : MusicTitle2);
				for (int j = 0; j < TotalMediaFileExt; j++)
				{
					if (StoreDirPath)
					{
						for (int k = 0; k < TotalMusicFiles; k++)
						{
							if (MediaFilesList[k, 0] == text && MediaFilesList[k, 1] == MediaFileExtension[j, 0])
							{
								DirPath = MediaFilesList[k, 2];
								FileName = MediaFilesList[k, 0] + MediaFilesList[k, 1];
								return DirPath + FileName;
							}
						}
					}
					else
					{
						string mediaFileNameFromDir = GetMediaFileNameFromDir(MediaDir, MediaFileExtension[j, 0], text, ref DirPath, ref FileName);
						if (mediaFileNameFromDir != "")
						{
							return mediaFileNameFromDir;
						}
					}
				}
			}
			return "";
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1)
		{
			string DirPath = "";
			string FileName = "";
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath)
		{
			string FileName = "";
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath, ref string FileName)
		{
			if ((FolderPath == "") | !Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\"))
			{
				return "";
			}
			if (File.Exists(FolderPath + MusicTitle1 + MusicExtension))
			{
				DirPath = FolderPath;
				FileName = MusicTitle1 + MusicExtension;
				return DirPath + FileName;
			}
			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			string[] array = directories;
			foreach (string str in array)
			{
				string mediaFileNameFromDir = GetMediaFileNameFromDir(str + "\\", MusicExtension, MusicTitle1, ref DirPath, ref FileName);
				if (mediaFileNameFromDir != "")
				{
					return mediaFileNameFromDir;
				}
			}
			return "";
		}

		public static string LookUpBookName(int InBibleVersion, int InBookNumber)
		{
			try
			{
				string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
				string fullSearchString = "select * from Bible where book=0 and chapter=10 and verse=" + InBookNumber;
#if OleDb
				DataTable datatable = DbOleDbController.getDataTable(connectString, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(connectString, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
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

		public static int SetLyricsTopPos(int TopSetting, int ScreenHeight)
		{
			return ScreenHeight * TopSetting / 100;
		}

		public static int Load32HeaderData(string InFileName, string InContents, ref string[] ThisHeaderData)
		{
			return Load32HeaderData(InFileName, InContents, ref ThisHeaderData, '>');
		}

		public static int Load32HeaderData(string InFileName, string InContents, ref string[] ThisHeaderData, char SeparatorSym)
		{
			for (int i = 0; i < 255; i++)
			{
				ThisHeaderData[i] = "";
			}
			int num = InContents.IndexOf("[");
			int num2 = InContents.IndexOf("]", num + 1);
			if (num == 0 && num2 > 1)
			{
				num++;
				int num3 = num2 + 1;
				string InFormatString = DataUtil.Mid(InContents, num, num2 - num + 1);
				if (DataUtil.Convertv32FormatString(ref InFormatString, SeparatorSym) == 320)
				{
					LoadHeaderData(InFormatString, ref ThisHeaderData, '>');
					return num2;
				}
			}
			num = 0;
			num2 = 0;
			InContents = "";
			return num2;
		}

		public static string ExtractHeaderInfo(string InString, int Index, char Separator)
		{
			int num = InString.IndexOf(Index + "=");
			if (num >= 0)
			{
				num += Index.ToString().Length + 1;
				int num2 = InString.IndexOf(Separator, num);
				if (num2 <= num)
				{
					return "";
				}
				return DataUtil.Mid(InString, num, num2 - num);
			}
			return "";
		}

		public static int LoadHeaderData(string InFormatString, ref string[] ThisHeaderData, char SeparatorSym)
		{
			for (int i = 0; i < 255; i++)
			{
				ThisHeaderData[i] = "";
			}
			if (InFormatString == "" || InFormatString[0] == '[')
			{
				return -1;
			}
			string text = "";
			string text2 = "";
			int num = -1;
			string[] array = InFormatString.Split('>');
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				text2 = DataUtil.ExtractOneInfo(ref array[i], '=', RemoveExtract: true, MinusOneIfBlank: false);
				if (text2 != "")
				{
					num = DataUtil.StringToInt(text2);
					if (num > 0 && num < 255)
					{
						ThisHeaderData[num] = array[i];
					}
				}
			}
			return 1;
		}

		public static void ApplyHeaderData()
		{
			ApplyHeaderData(0);
		}

		public static void ApplyHeaderData(int InMode)
		{
			int num = IndexFileVersion = ExtractNumericData(HeaderData[1]);
			if (InMode != 1)
			{
				num = ExtractNumericData(HeaderData[11]);
				PanelBackColour = (((HeaderData[11] == "") | !ValidateColour(num)) ? DefaultBackColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[12]);
				PanelBackColourTransparent = (((num < 0) | (num > 1)) ? 1 : num);
				num = ExtractNumericData(HeaderData[13]);
				PanelTextColour = (((HeaderData[13] == "") | !ValidateColour(num)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[14]);
				PanelTextColourAsRegion1 = (((num < 0) | (num > 1)) ? 1 : num);
				num = ExtractNumericData(HeaderData[15]);
				num = ((num < 0) ? 31 : num);
				ShowDataDisplayMode = DataUtil.GetBitValue(num, 1);
				ShowDataDisplaySlides = DataUtil.GetBitValue(num, 2);
				ShowDataDisplaySongs = DataUtil.GetBitValue(num, 3);
				ShowDataDisplayTitle = DataUtil.GetBitValue(num, 4);
				ShowDataDisplayCopyright = DataUtil.GetBitValue(num, 5);
				ShowDataDisplayPrevNext = DataUtil.GetBitValue(num, 6);
				num = ExtractNumericData(HeaderData[16]);
				num = ((num < 6 || num > 20) ? 8 : num);
				BottomBorderFactor = (double)num / 100.0;
				ShowDataDisplayFontName = HeaderData[17];
				if (ShowDataDisplayFontName == "")
				{
					ShowDataDisplayFontName = "Microsoft Sans Serif";
				}
				num = ExtractNumericData(HeaderData[18]);
				num = ((num >= 0) ? num : 0);
				ShowDataDisplayFontBold = DataUtil.GetBitValue(num, 1);
				ShowDataDisplayFontItalic = DataUtil.GetBitValue(num, 2);
				ShowDataDisplayFontUnderline = DataUtil.GetBitValue(num, 3);
				ShowDataDisplayFontShadow = DataUtil.GetBitValue(num, 4);
				ShowDataDisplayFontOutline = DataUtil.GetBitValue(num, 5);
				num = ExtractNumericData(HeaderData[19]);
				ShowDataDisplayIndicatorsFontSize = ((num < 8 || num > 20) ? 8 : num);
				num = ExtractNumericData(HeaderData[21]);
				ShowSongHeadings = (((num < 0) | (num > 3)) ? 1 : num);
				num = ExtractNumericData(HeaderData[22]);
				num = ((num < 0) ? 2 : num);
				UseShadowFont = DataUtil.GetBitValue(num, 2);
				ShowNotations = DataUtil.GetBitValue(num, 3);
				ShowCapoZero = DataUtil.GetBitValue(num, 4);
				ShowInterlace = DataUtil.GetBitValue(num, 5);
				UseOutlineFont = DataUtil.GetBitValue(num, 6);
				num = ExtractNumericData(HeaderData[23]);
				ShowSongHeadingsAlign = ((!((num < 0) | (num > 4))) ? num : 0);
				num = ExtractNumericData(HeaderData[25]);
				ShowLyrics = (((num < 0) | (num > 2)) ? 2 : num);
				num = ExtractNumericData(HeaderData[26]);
				ShowScreenColour[0] = (((HeaderData[26] == "") | !ValidateColour(num)) ? DefaultBackColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[27]);
				ShowScreenColour[1] = (((HeaderData[27] == "") | !ValidateColour(num)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[28]);
				ShowScreenStyle = ((!((num < 0) | (num > MaxBackgroundStyleIndex))) ? num : 0);
				num = ExtractNumericData(HeaderData[29]);
				ShowFontColour[0] = (((HeaderData[29] == "") | !ValidateColour(num)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[30]);
				ShowFontColour[1] = (((HeaderData[30] == "") | !ValidateColour(num)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[31]);
				ShowFontAlign[0, 0] = (((num < 0) | (num > 3)) ? 2 : num);
				num = ExtractNumericData(HeaderData[32]);
				ShowFontAlign[0, 1] = (((num < 0) | (num > 3)) ? 2 : num);
				num = ExtractNumericData(HeaderData[50]);
				MediaOption = ((!((num < 0) | (num > 3))) ? num : 0);
				MediaLocation = HeaderData[51];
				num = ExtractNumericData(HeaderData[52]);
				MediaVolume = (((num < 0) | (num > 100)) ? 50 : num);
				num = ExtractNumericData(HeaderData[53]);
				MediaBalance = ((!((num < -100) | (num > 100))) ? num : 0);
				num = ExtractNumericData(HeaderData[54]);
				MediaMute = DataUtil.GetBitValue(num, 1);
				MediaRepeat = DataUtil.GetBitValue(num, 2);
				MediaWidescreen = DataUtil.GetBitValue(num, 3);
				num = ExtractNumericData(HeaderData[55]);
				MediaCaptureDeviceNumber = (((num < 1) | (num > 5)) ? 1 : num);
				MediaOutputMonitorName = HeaderData[56];
				BackgroundPicture = HeaderData[61];
				num = ExtractNumericData(HeaderData[62]);
				BackgroundMode = (ImageMode)(((num < 0) | (num > 2)) ? 2 : num);
				num = ExtractNumericData(HeaderData[63]);
				ShowVerticalAlign = (((num < 0) | (num > 2)) ? 1 : num);
				num = ExtractNumericData(HeaderData[64]);
				ShowLeftMargin[0] = (((num < 0) | (num > 40)) ? 2 : num);
				num = ExtractNumericData(HeaderData[65]);
				ShowRightMargin[0] = (((num < 0) | (num > 40)) ? 2 : num);
				num = ExtractNumericData(HeaderData[66]);
				ShowBottomMargin[0] = ((!((num < 0) | (num > 100))) ? num : 0);
				ShowItemTransition = GlobalImageCanvas.GetTransitionType(HeaderData[72]);
				ShowSlideTransition = GlobalImageCanvas.GetTransitionType(HeaderData[73]);
			}
			for (int i = 0; i < 8; i++)
			{
				PB_ShowWords[i] = 1;
				PB_WordsBold[i] = 0;
				PB_WordsItalic[i] = 0;
				PB_WordsUnderline[i] = 0;
				PB_WordsSize[i] = 11;
				PB_WordsColour[i] = BlackScreenColour;
			}
			PB_WordsSize[0] = 13;
			PB_WordsSize[5] = 11;
			PB_WordsSize[2] = 6;
			PB_WordsBold[0] = 1;
			PB_WordsBold[1] = 1;
			PB_WordsItalic[4] = 1;
			PB_WordsUnderline[0] = 1;
			PB_ShowHeadings[0] = 1;
			PB_ShowHeadings[1] = 1;
			PB_ShowHeadings[2] = 0;
			PB_ShowHeadings[3] = 0;
			PB_LyricsPattern = 1;
			PB_ShowSection = 2;
			PB_ShowColumns = 2;
			PB_PageSize = 0;
			PB_Spacing[0] = 0;
			PB_Spacing[1] = 2;
			PB_ShowScreenBreaks = 1;
			PB_OneSongPerPage = 0;
			PB_CJKGroupStyle = SortBy.Alpha;
			for (int i = 0; i < 8; i++)
			{
				int num2 = 101 + i * 5;
				num = ExtractNumericData(HeaderData[num2]);
				PB_ShowWords[i] = ((num < 0) ? 1 : DataUtil.GetBitValue(num, 1));
				if (num < 0)
				{
					num = 0;
				}
				if (i < 6)
				{
					PB_WordsBold[i] = DataUtil.GetBitValue(num, 2);
					PB_WordsItalic[i] = DataUtil.GetBitValue(num, 3);
					PB_WordsUnderline[i] = DataUtil.GetBitValue(num, 4);
					num = ExtractNumericData(HeaderData[num2 + 1]);
					PB_WordsSize[i] = (((num < 4) | (num > 72)) ? PB_WordsSize[i] : num);
					num = ExtractNumericData(HeaderData[num2 + 2]);
					PB_WordsColour[i] = (((HeaderData[num2 + 2] == "") | !ValidateColour(num)) ? BlackScreenColour : Color.FromArgb(Convert.ToInt32(num)));
				}
				else
				{
					PB_WordsBold[i] = PB_WordsBold[2];
					PB_WordsItalic[i] = PB_WordsItalic[2];
					PB_WordsUnderline[i] = PB_WordsUnderline[2];
					PB_WordsSize[i] = PB_WordsSize[2];
					PB_WordsColour[i] = PB_WordsColour[2];
				}
			}
			num = ExtractNumericData(HeaderData[151]);
			PB_ShowHeadings[0] = DataUtil.GetBitValue(num, 1);
			PB_ShowHeadings[1] = DataUtil.GetBitValue(num, 2);
			PB_ShowHeadings[2] = DataUtil.GetBitValue(num, 3);
			PB_ShowHeadings[3] = DataUtil.GetBitValue(num, 4);
			num = ExtractNumericData(HeaderData[153]);
			PB_LyricsPattern = (((num < 0) | (num > 1)) ? 1 : num);
			num = ExtractNumericData(HeaderData[154]);
			PB_ShowSection = (((num < 0) | (num > 2)) ? 2 : num);
			num = ExtractNumericData(HeaderData[155]);
			PB_ShowColumns = (((num < 1) | (num > 2)) ? 2 : num);
			num = ExtractNumericData(HeaderData[156]);
			PB_PageSize = ((!((num < 0) | (num > 1))) ? num : 0);
			num = ExtractNumericData(HeaderData[170]);
			PB_Spacing[0] = ((!((num < 0) | (num > 5))) ? num : 0);
			num = ExtractNumericData(HeaderData[171]);
			PB_Spacing[1] = (((num < 1) | (num > 20)) ? 2 : num);
			num = ExtractNumericData(HeaderData[172]);
			PB_ShowScreenBreaks = (((num < 0) | (num > 1)) ? 1 : num);
			num = ExtractNumericData(HeaderData[173]);
			PB_OneSongPerPage = ((!((num < 0) | (num > 1))) ? num : 0);
			num = ExtractNumericData(HeaderData[174]);
			PB_CJKGroupStyle = (SortBy)((!((num < 0) | (num > 1))) ? num : 0);
			num = ExtractNumericData(HeaderData[180]);
			PB_ShowNotations = DataUtil.GetBitValue(num, 1);
			PB_ShowTiming = DataUtil.GetBitValue(num, 2);
			PB_ShowKey = DataUtil.GetBitValue(num, 3);
			PB_ShowCapo = DataUtil.GetBitValue(num, 4);
			PB_CapoZero = ((num >= 0) ? DataUtil.GetBitValue(num, 5) : 0);
		}

		public static int ExtractNumericData(string InString)
		{
			if (string.IsNullOrEmpty(InString))
			{
				return -1;
			}

			int chknum = -1;			
			try
			{
				bool isnum = int.TryParse(InString, out chknum);
				return chknum;
			}
			catch
			{
				return -1;
			}
		}

		public static int GetSelectedIndex(ListView InListView)
		{
			string OutString = null;
			return GetSelectedIndex(InListView, ref OutString, "");
		}

		public static int GetSelectedIndex(ListView InListView, ref string OutString)
		{
			return GetSelectedIndex(InListView, ref OutString, "");
		}

		public static int GetSelectedIndex(ListView InListView, ref string OutString, string InString)
		{
			return GetSelectedIndex(InListView, ref OutString, InString, 0);
		}

		public static int GetSelectedIndex(ListView InListView, ref string OutString, string InString, int ColumnText)
		{
			if (InListView.SelectedItems.Count == 0)
			{
				if (InListView.Items.Count > 0 && InListView.Items[0].Selected)
				{
					return 0;
				}
				if (InString != "")
				{
					MessageBox.Show("Please select a song from the " + InString + " to edit");
				}
				return -1;
			}

			IEnumerator enumerator = InListView.SelectedItems.GetEnumerator();

			try
			{
				if (enumerator.MoveNext())
				{
					ListViewItem listViewItem = (ListViewItem)enumerator.Current;
					if (OutString != null)
					{
						OutString = listViewItem.SubItems[ColumnText].Text;
					}
					return InListView.Items.IndexOf(listViewItem);
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return -1;
		}

		public static string RemoveMusicSym(string InString)
		{
			if (DataUtil.Right(InString, MusicSymLen) == " <#>")
			{
				return DataUtil.Left(InString, InString.Length - MusicSymLen);
			}
			return InString;
		}		

		public static void ResetShowRunningSettings()
		{
			ShowRunning_ShowDataDisplayMode = 0;
			ShowRunning_ShowSongHeadings = 0;
			ShowRunning_ShowLyrics = 0;
			ShowRunning_UseShadowFont = 0;
			ShowRunning_UseOutlineFont = 0;
			ShowRunning_ShowNotations = 0;
			ShowRunning_ShowInterlace = 0;
			ShowRunning_ShowVerticalAlign = 0;
		}

		public static void SetListViewColumns(ListView InListView, int NumberofCol)
		{
			InListView.Clear();
			InListView.View = View.Details;
			for (int i = 0; i < NumberofCol; i++)
			{
				InListView.Columns.Add(i.ToString());
			}
		}

		public static void LoadDBFormatString(ref SongSettings InItem)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + InItem.ItemID;
#if OleDb
				DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
				{
					InItem.Format.DBStoredFormat = DataUtil.GetDataString(datatable.Rows[0], "FORMATDATA");
				}
			}
			catch
			{
			}
		}

		public static void LoadIndividualData(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide)
		{
			string InTitle = "";
			LoadIndividualData(ref InItem, InIDString, InFormatString, StartingSlide, ref InTitle);
		}

		public static void LoadIndividualData(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide, ref string InTitle)
		{
			string a = DataUtil.Left(InIDString, 1);
			InItem.ItemID = DataUtil.Mid(InIDString, 1);
			string InFileName = InItem.ItemID;
			if (a == "D")
			{
				InItem.Type = "D";
				try
				{
					string fullSearchString = "select * from SONG where songid=" + InItem.ItemID;
#if OleDb
					DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
					
					if (datatable.Rows.Count>0)
					{
						DataRow dr = datatable.Rows[0];
						InItem.Title = DataUtil.GetDataString(dr, "Title_1");
						InItem.Title2 = DataUtil.GetDataString(dr, "Title_2");
						InItem.SongNumber = DataUtil.GetDataInt(dr, "song_number");
						InItem.CompleteLyrics = DataUtil.GetDataString(dr, "Lyrics");
						InItem.FolderNo = DataUtil.GetDataInt(dr, "FolderNo");
						InItem.FontSizeFactor = 100;
						InItem.Writer = DataUtil.GetDataString(dr, "Writer");
						InItem.Copyright = DataUtil.GetDataString(dr, "Copyright");
						InItem.Format.FormatString = InFormatString;
						InItem.Format.DBStoredFormat = DataUtil.GetDataString(dr, "FORMATDATA");
						InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
						InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
						InItem.In_LicAdminInfo1 = InItem.Show_LicAdminInfo1;
						InItem.In_LicAdminInfo2 = InItem.Show_LicAdminInfo2;
						LoadLicAdminDisplayInfo(ref InItem.Show_LicAdminInfo1, ref InItem.Show_LicAdminInfo2);
						InItem.Notations = DataUtil.GetDataString(dr, "msc");
						InItem.Capo = DataUtil.GetDataInt(dr, "capo", Minus1IfBlank: true);
						InItem.CurSlide = StartingSlide;
						InItem.SongSequence = DataUtil.GetDataString(dr, "Sequence");
						InItem.SongOriginalLoadedSequence = InItem.SongSequence;
						InItem.Book_Reference = DataUtil.GetDataString(dr, "Book_Reference");
						InItem.User_Reference = DataUtil.GetDataString(dr, "User_Reference");
						InItem.Timing = DataUtil.GetDataString(dr, "timing");
						InItem.MusicKey = DataUtil.GetDataString(dr, "key");
						InItem.Category = DataUtil.GetDataString(dr, "category");
						InItem.RotateString = ExtractSettings(DataUtil.GetDataString(dr, "SETTINGS"), SettingsCategory.RotateString);
						GetRotationStyle(ref InItem);
						if ((ListViewNotations == null) | (InItem.LyricsAndNotationsList == null))
						{
							InItem.CompleteLyrics = "";
						}
					}
				}
				catch
				{
				}
			}
			else if (a == "P")
			{
				InItem.Type = "P";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "B")
			{
				string InString = InIDString;
				DataUtil.ExtractOneInfo(ref InString, ';');
				int num = LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InString, ';'));
				int num2 = LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InString, ';'));
				InItem.Type = "B";
				if (num >= 0)
				{
					InItem.Type = "B";
					InItem.FolderNo = DataUtil.StringToInt(HB_Versions[num, 5]);
					InItem.FontSizeFactor = DataUtil.StringToInt(HB_Versions[num, 6]);
					InItem.CompleteLyrics = LoadSelectedBibleVerses(InItem.ItemID);
					InItem.Copyright = HB_Versions[num, 3];
					InItem.Show_BookName = ((DataUtil.Left(InItem.ItemID, 1) == "1") ? true : false);
					int num3 = InTitle.IndexOf("(");
					if (num3 > 0)
					{
						InTitle = DataUtil.Trim(DataUtil.Left(InTitle, num3 - 1));
					}
					if (num2 < 0)
					{
						InItem.Title = InTitle + " (" + HB_Versions[num, 1] + ")";
						InItem.HBR2_FolderNo = InItem.FolderNo;
						InItem.HBR2_FontSizeFactor = InItem.FontSizeFactor;
					}
					else
					{
						InItem.Title = InTitle + " (" + HB_Versions[num, 1] + "/" + HB_Versions[num2, 1] + ")";
						InItem.HBR2_FolderNo = DataUtil.StringToInt(HB_Versions[num2, 5]);
						InItem.HBR2_FontSizeFactor = DataUtil.StringToInt(HB_Versions[num2, 6]);
						SongSettings obj2 = InItem;
						obj2.Copyright = obj2.Copyright + "/" + HB_Versions[num2, 3];
					}
					InItem.Format.FormatString = InFormatString;
					LoadLicAdminDisplayInfo(ref InItem.Show_LicAdminInfo1, ref InItem.Show_LicAdminInfo2);
					InItem.Capo = -1;
					InItem.CurSlide = StartingSlide;
					InTitle = InItem.Title;
				}
			}
			else if (a == "T")
			{
				InItem.Type = "T";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.Type = "T";
				InItem.CompleteLyrics = LoadTextFile(InFileName);
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "I")
			{
				InItem.Type = "I";
				string[] ThisHeaderData = new string[255];
				LoadInfoFile(InFileName, ref InItem, ref ThisHeaderData);
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "W")
			{
				InItem.Type = "W";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.Type = "W";
				InItem.CompleteLyrics = GetOfficeDocContents(InFileName);
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "M")
			{
				InItem.Type = "M";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.Type = "M";
				InItem.CompleteLyrics = " ";
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = 1;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "G")
			{
				InItem.Type = "G";
				InItem.Title = "";
				InItem.Type = "G";
				InItem.CompleteLyrics = " ";
				InItem.Format.FormatString = GapFormatString(ref InItem);
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
			}
			InItem.OriginalNotations = InItem.Notations;
			if (InItem.CompleteLyrics == "")
			{
				InItem.CompleteLyrics = " ";
			}
		}

		private static string GapFormatString(ref SongSettings InItem)
		{
			InItem.UseDefaultFormat = false;
			switch (GapItemOption)
			{
				case GapType.Black:
					InItem.Format.ShowScreenColour[0] = BlackScreenColour;
					InItem.Format.ShowScreenColour[1] = BlackScreenColour;
					InItem.Format.ShowScreenStyle = 11;
					InItem.Format.BackgroundPicture = "";
					break;
				case GapType.User:
					{
						InItem.Format.BackgroundPicture = GapItemLogoFile;
						string directoryName = Path.GetDirectoryName(InItem.Format.BackgroundPicture);
						if (directoryName == RootEasiSlidesDir + "Images\\Tiles")
						{
							InItem.Format.BackgroundMode = ImageMode.Tile;
						}
						else
						{
							InItem.Format.BackgroundMode = ImageMode.BestFit;
						}
						break;
					}
				case GapType.Default:
					InItem.Format.ShowScreenColour[0] = ShowScreenColour[0];
					InItem.Format.ShowScreenColour[1] = ShowScreenColour[1];
					InItem.Format.ShowScreenStyle = ShowScreenStyle;
					InItem.Format.BackgroundPicture = BackgroundPicture;
					InItem.Format.BackgroundMode = BackgroundMode;
					break;
				default:
					return "";
			}
			InItem.Format.ShowItemTransition = (GapItemUseFade ? 15 : 0);
			InItem.Format.ShowSlideTransition = InItem.Format.ShowItemTransition;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(InItem.Format.ShowScreenColour[0].ToArgb()) + '>');
			stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(InItem.Format.ShowScreenColour[1].ToArgb()) + '>');
			stringBuilder.Append(Convert.ToString(28) + "=" + InItem.Format.ShowScreenStyle.ToString() + '>');
			stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(InItem.Format.MediaOption) + '>');
			stringBuilder.Append(Convert.ToString(51) + "=" + InItem.Format.MediaLocation + '>');
			stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(InItem.Format.MediaVolume) + '>');
			stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(InItem.Format.MediaBalance) + '>');
			int num = InItem.Format.MediaMute + InItem.Format.MediaRepeat * 2 + InItem.Format.MediaWidescreen * 4;
			stringBuilder.Append(Convert.ToString(54) + "=" + num.ToString() + '>');
			stringBuilder.Append(Convert.ToString(55) + "=" + Convert.ToString(InItem.Format.MediaCaptureDeviceNumber) + '>');
			stringBuilder.Append(Convert.ToString(56) + "=" + InItem.Format.MediaOutputMonitorName + '>');
			stringBuilder.Append(Convert.ToString(61) + "=" + InItem.Format.BackgroundPicture + '>');
			stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)InItem.Format.BackgroundMode) + '>');
			stringBuilder.Append(Convert.ToString(72) + "=" + tempScreen.GetTransitionText(InItem.Format.ShowItemTransition) + '>');
			stringBuilder.Append(Convert.ToString(73) + "=" + tempScreen.GetTransitionText(InItem.Format.ShowSlideTransition) + '>');
			return stringBuilder.ToString();
		}

		public static bool IsNewR2Format(string InLyrics)
		{
			InLyrics = InLyrics.Replace("\r\n", "\n");
			string[] array = InLyrics.Split(new string[1]
			{
				VerseSymbol[150]
			}, StringSplitOptions.RemoveEmptyEntries);
			if ((InLyrics == "") | (array.GetUpperBound(0) == 0) | (array.GetUpperBound(0) > 1))
			{
				return false;
			}
			if (array[1][0] == "\n"[0])
			{
				array[1] = DataUtil.Mid(array[1], 1);
			}
			for (int i = 1; i <= 99; i++)
			{
				if (DataUtil.Left(array[1], VerseSymbol[i].Length) == VerseSymbol[i])
				{
					return true;
				}
			}
			if (DataUtil.Left(array[1], VerseSymbol[0].Length) == VerseSymbol[0])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[100].Length) == VerseSymbol[100])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[103].Length) == VerseSymbol[103])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[101].Length) == VerseSymbol[101])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[102].Length) == VerseSymbol[102])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[111].Length) == VerseSymbol[111])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[112].Length) == VerseSymbol[112])
			{
				return true;
			}
			return false;
		}

		public static void LoadLicAdminDisplayInfo(ref string AdminInfo1, ref string AdminInfo2)
		{
			string text = "";
			string text2 = "";
			if (AdminInfo1 == "")
			{
				text = LicAdmin_List[0, 2];
			}
			else
			{
				for (int i = 2; i <= 9; i++)
				{
					if (LicAdmin_List[i, 0] != "" && AdminInfo1 == LicAdmin_List[i, 0])
					{
						text = LicAdmin_List[i, 2];
						i = 9;
					}
				}
			}
			if (AdminInfo2 == "")
			{
				text2 = LicAdmin_List[0, 2];
			}
			else
			{
				for (int i = 2; i <= 9; i++)
				{
					if (LicAdmin_List[i, 0] != "" && AdminInfo2 == LicAdmin_List[i, 0])
					{
						text2 = LicAdmin_List[i, 2];
						i = 9;
					}
				}
			}
			if (text != "")
			{
				AdminInfo1 = text;
			}
			if (text != text2)
			{
				AdminInfo2 = text2;
			}
		}

		public static int IncrementChord(ref int CurPos, int TransposeTo)
		{
			int num = CurPos + TransposeTo;
			if (num >= 12)
			{
				num %= 12;
			}
			else if (num < 0)
			{
				num += 12 * (1 - num / 12);
			}
			return num;
		}

		public void LoadIndividualFormatData(ref SongSettings InItem)
		{
			LoadIndividualFormatData(ref InItem, "");
		}

        public static string LoadSelectedBibleVerses(string InFullBibleString)
        {
            try
            {
                // 占?踰덉㎏ 媛믪씠 0蹂대떎 ?占쎈㈃ flag = true
                bool flag = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InFullBibleString, ';')) > 0;

                // ?占쎄꼍 踰꾩쟾 ?占쎈낫 ?占??
                string[] bibleVersions = new string[2];
                int[] versionNumbers = new int[2];

                for (int i = 0; i < 2; i++)
                {
                    bibleVersions[i] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
                    versionNumbers[i] = LookUpBibleVersionNumber(bibleVersions[i]);
                }

                string[] verseData = { InFullBibleString, InFullBibleString };

                // 寃곌낵 臾몄옄??
                StringBuilder InTextString = new StringBuilder();

                bool hasSecondVersion = versionNumbers[1] >= 0;

                for (int i = 0; i <= (hasSecondVersion ? 1 : 0); i++)
                {
                    bool flag2 = PartialWordSearch(versionNumbers[1]);

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
                            flag, flag2, flag, ShowFormatTags: true);

                        InTextString.Append("\n");
                    }
                }

                return InTextString.ToString();
            }
            catch (Exception ex)
            {
                // ?占쎌쇅 諛쒖깮 ???占쎈쾭源낆쓣 ?占쏀빐 濡쒓렇 異쒕젰
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }

        public static string LoadSelectedBibleVerses_Old(string InFullBibleString)
        {
            try
            {
                bool flag = (DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InFullBibleString, ';')) > 0) ? true : false;
                string[] array = new string[2];
                int[] array2 = new int[2];
                string[] array3 = new string[2];
                array[0] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
                array2[0] = LookUpBibleVersionNumber(array[0]);
                array[1] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
                array2[1] = LookUpBibleVersionNumber(array[1]);
                array3[0] = InFullBibleString;
                array3[1] = InFullBibleString;
                StringBuilder InTextString = new StringBuilder();
                string text = "";
                for (int i = 0; i <= ((array2[1] >= 0) ? 1 : 0); i++)
                {
                    bool flag2 = false;
                    string text2 = ConnectStringDef + HB_Versions[array2[i], 4];
                    flag2 = PartialWordSearch(array2[1]);
                    if (i == 1)
                    {
                        InTextString.Append(VerseSymbol[150] + "\n");
                    }
                    while (array3[i] != "")
                    {
                        int inBookNumber = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int chapterStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int verseStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int chapterEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int verseEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        LoadBiblePassages(array2[i], inBookNumber, ref InTextString, InShowVerses: true, DoCompleteBook: false, TrackOutput: false, chapterStart, verseStart, chapterEnd, verseEnd, flag, flag2, flag ? true : false, ShowFormatTags: true);
                        InTextString.Append("\n");
                    }
                }
                return InTextString.ToString();
            }
            catch
            {
                return "";
            }
        }

		public static void UpdatePosUpDowns(ref NumericUpDown Reg1_UpDown, ref NumericUpDown Reg2_UpDown, ref NumericUpDown RegBottom_UpDown, ref int Reg1_Value, ref int Reg2_Value, int RegBottom_Value)
		{
			if (Reg1_Value < 0)
			{
				Reg1_Value = 0;
			}
			if (Reg1_Value > 100)
			{
				Reg1_Value = 100;
			}
			if (Reg2_Value < Reg1_Value)
			{
				Reg2_Value = Reg1_Value;
			}
			if (Reg2_Value > 100)
			{
				Reg2_Value = 100;
			}
			if (RegBottom_Value + Reg1_Value > 100)
			{
				RegBottom_Value = 100 - Reg1_Value;
				Reg2_Value = Reg1_Value;
			}
			if (RegBottom_Value + Reg2_Value > 100)
			{
				Reg2_Value = 100 - RegBottom_Value;
			}
			Reg1_UpDown.Minimum = 0m;
			Reg1_UpDown.Maximum = 100m;
			Reg1_UpDown.Value = Reg1_Value;
			Reg1_UpDown.Maximum = 100 - RegBottom_Value;
			Reg2_UpDown.Minimum = Reg1_Value;
			Reg2_UpDown.Maximum = 100m;
			Reg2_UpDown.Value = Reg2_Value;
			Reg2_UpDown.Maximum = 100 - RegBottom_Value;
			RegBottom_UpDown.Minimum = 0m;
			RegBottom_UpDown.Maximum = 100m;
			RegBottom_UpDown.Value = RegBottom_Value;
		}

		public static void SetShowBackground(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetShowBackground(InItem, ref InPic, FallBackToDefault: true);
		}

		public static void SetShowBackground(SongSettings InItem, ref ImageTransitionControl InPic, bool FallBackToDefault)
		{
			ImageMode picMode = ImageMode.Tile;
			string text = "";
			if (InItem.UseDefaultFormat)
			{
				if (File.Exists(BackgroundPicture))
				{
					text = BackgroundPicture;
				}
				picMode = BackgroundMode;
			}
			else
			{
				string text2 = (InItem.Type == "I") ? InItem.Format.TempImageFileName : InItem.Format.BackgroundPicture;
				if (File.Exists(text2))
				{
					text = text2;
					picMode = InItem.Format.BackgroundMode;
				}
				else if (InItem.Type == "I" && InItem.Format.TempImageFileName == "")
				{
					InItem.Format.ShowScreenColour[0] = ShowScreenColour[0];
					InItem.Format.ShowScreenColour[1] = ShowScreenColour[1];
					InItem.Format.ShowScreenStyle = ShowScreenStyle;
				}
			}
			if (text == "")
			{
				SetColoursFormat(InItem, ref InPic);
			}
			else
			{
				SetImageFormat(ref InPic, text, picMode, SetTransparent: false);
			}
		}

		public static void SetColoursFormat(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetColoursFormat(InItem, ref InPic, SetTransparent: false);
		}

		public static void SetColoursFormat(SongSettings InItem, ref ImageTransitionControl InPic, bool SetTransparent)
		{
			if (SetTransparent)
			{
				string text = "";
				Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				Graphics g = Graphics.FromImage(image);
				BackPattern.Clear(ref g, TransparentColour);
				InPic.NewBackgroundPicture = image;
				InPic.NewTextImage = InPic.NewBackgroundPicture;
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				//image.Dispose();
				//g.Dispose();
			}
			else if (InItem.UseDefaultFormat)
			{
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				else
				{
					InPic.CurrentBackgroundPicture = InPic.BackgroundImage;
					InPic.CurrentTextImage = InPic.BackgroundImage;
					InPic.CurrentCombinedImage = InPic.BackgroundImage;
					InPic.NewTextImage = InPic.BackgroundImage;
				}
				InPic.NewBackgroundPicture = InPic.BackgroundImage;
				if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.CurrentOnly;
				}
				InPic.BackgroundID = DefaultBackgroundID;
			}
			else
			{
				string text = "";
				Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				Graphics g = Graphics.FromImage(image);
				BackPattern.Fill(ref g, InItem.Format.ShowScreenColour[0], InItem.Format.ShowScreenColour[1], InItem.Format.ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref text);
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				else
				{
					InPic.CurrentBackgroundPicture = InPic.BackgroundImage;
					InPic.CurrentTextImage = InPic.BackgroundImage;
					InPic.CurrentCombinedImage = InPic.BackgroundImage;
					InPic.NewTextImage = InPic.BackgroundImage;
				}
				InPic.NewBackgroundPicture = image;
				if (InPic.BackgroundID == text)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				}
				else if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.NewOnly;
				}
				else if (text == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.CurrentOnly;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
				}
				InPic.BackgroundID = text;
				//image.Dispose();
				//g.Dispose();
			}
			try
			{
				if (InPic.CurrentBackgroundPicture == null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.CurrentTextImage == null)
				{
					InPic.CurrentTextImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.CurrentCombinedImage == null)
				{
					InPic.CurrentCombinedImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.NewBackgroundPicture == null)
				{
					InPic.NewBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.NewTextImage == null)
				{
					InPic.NewTextImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.NewCombinedImage == null)
				{
					InPic.NewCombinedImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
			}
			catch
			{
			}
		}

		public static void SetImageFormat(ref ImageTransitionControl InPic, string File_Name, ImageMode PicMode, bool SetTransparent)
		{
			Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
			using Graphics graphics = Graphics.FromImage(image);
			
			if (InPic.BackgroundID != "")
			{
				if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.NewOnly;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
				}
			}
			else if ((InPic.ImageFileName == File_Name) & (InPic.PicMode == (int)PicMode))
			{
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
			}
			else
			{
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
			}
			InPic.BackgroundID = "";
			
			// Load image using stream to avoid file lock
			Image image2;
			using (var stream = new FileStream(File_Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				image2 = Image.FromStream(stream);
			}
			
			try
			{
				InPic.ImageFileName = File_Name;
				InPic.PicMode = (int)PicMode;
				double num = (double)image2.Width / (double)image2.Height;
				if ((InPic.Width <= 0) | (InPic.Height <= 0))
				{
					return;
				}
				double num2 = (double)InPic.Width / (double)InPic.Height;
				switch (PicMode)
				{
					case ImageMode.Centre:
						{
							int x2 = 0;
							int y2 = 0;
							int num4 = image2.Width;
							int num3 = image2.Height;
							int x;
							int width;
							int height;
							int y;
							if (num2 < num)
							{
								x = 0;
								width = image.Width;
								height = (int)((double)width / num);
								y = (image.Height - height) / 2;
							}
							else
							{
								y = 0;
								height = image.Height;
								width = (int)((double)height * num);
								x = (image.Width - width) / 2;
							}
							graphics.DrawImage(image2, new Rectangle(x, y, width, height), new Rectangle(x2, y2, num4, num3), GraphicsUnit.Pixel);
							break;
						}
					case ImageMode.Tile:
						{
							int num4 = image2.Width;
							int num3 = image2.Height;
							for (int x2 = 0; x2 <= image.Width / num4; x2++)
							{
								for (int y2 = 0; y2 <= image.Height / num3; y2++)
								{
									int x = x2 * num4;
									int y = y2 * num3;
									graphics.DrawImage(image2, new Rectangle(x, y, num4, num3), new Rectangle(0, 0, num4, num3), GraphicsUnit.Pixel);
								}
							}
							break;
						}
					default:
						{
							int x = 0;
							int y = 0;
							int width = image.Width;
							int height = image.Height;
							int y2;
							int num3;
							int num4;
							int x2;
							if (num2 < num)
							{
								y2 = 0;
								num3 = image2.Height;
								num4 = (int)((double)num3 * num2);
								x2 = (image2.Width - num4) / 2;
							}
							else
							{
								x2 = 0;
								num4 = image2.Width;
								num3 = (int)((double)num4 / num2);
								y2 = (image2.Height - num3) / 2;
							}
							graphics.DrawImage(image2, new Rectangle(x, y, width, height), new Rectangle(x2, y2, num4, num3), GraphicsUnit.Pixel);
							break;
						}
				}
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				else
				{
					InPic.CurrentBackgroundPicture = image;
				}
				InPic.NewBackgroundPicture = image;
			}
			finally
			{
				// Clean up loaded image
				image2?.Dispose();
			}
        }

		public static string GetSlideContents(SongSettings InItem, int CurSlide, int RegionNumber, Font InFont, bool PreviewNotations)
		{
			int[,] slide = InItem.Slide;
			int num = (RegionNumber == 0) ? 1 : 3;
			int num2 = (RegionNumber == 0) ? 2 : 4;
			string text = "";
			string text2 = "";
			int num3 = 0;
			tbWorkspace.WordWrap = false;
			tbTempSpace.WordWrap = false;

			int itemCount = InItem.LyricsAndNotationsList.Items.Count;

			if (PreviewNotations)
			{
				if (slide[CurSlide, num] <= slide[CurSlide, num2])
				{
					bool flag = false;
					
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (i >= itemCount) continue;
						if (InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text != "")
						{
							flag = true;
							i = slide[CurSlide, num2] + 1;
						}
					}
					string text3 = "";
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (i >= itemCount) continue;
						text = text + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
						if (flag)
						{
							object obj = text3;
							text3 = string.Concat(obj, "(", Convert.ToInt32(num3), ';', InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text, ")");
						}
						num3++;
					}
					text = CombineLyricsAndNotations(text, text3, InFont, InFont, ref tbWorkspace, ref tbTempSpace) + "\n";
				}
			}
			else if (slide[CurSlide, num] <= slide[CurSlide, num2])
			{
				for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
				{
					if (i >= itemCount) continue;
					text2 = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
					SubstituteDashes(ref text2, 0);
					text += text2;
				}
			}
			if (InItem.Type == "B")
			{
				text = text.Replace('\u0098'.ToString(), "");
			}
			return DataUtil.TrimEnd(text) + "\n";
		}

		public static string OldGetSlideContents(SongSettings InItem, int CurSlide, int RegionNumber, Font InFont, bool PreviewNotations)
		{
			int[,] slide = InItem.Slide;
			int num = (RegionNumber == 0) ? 1 : 3;
			int num2 = (RegionNumber == 0) ? 2 : 4;
			string text = "";
			if (PreviewNotations)
			{
				if (slide[CurSlide, num] <= slide[CurSlide, num2])
				{
					bool flag = false;
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text != "")
						{
							flag = true;
							i = slide[CurSlide, num2] + 1;
						}
					}
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (flag)
						{
							text = text + CombineLyricsAndNotations(InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text, InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text, InFont, InFont, ref tbWorkspace, ref tbTempSpace) + "\n";
						}
						text = text + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
					}
				}
			}
			else if (slide[CurSlide, num] <= slide[CurSlide, num2])
			{
				for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
				{
					text = text + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
				}
			}
			if (InItem.Type == "B")
			{
				text = text.Replace('\u0098'.ToString(), "");
			}
			return text;
		}

		public static string FormatNotationString(ListView InListView, string InString, string InNotation, Font MainFont, Font NotationsFont)
		{
			Graphics graphics = InListView.CreateGraphics();
			int num = 0;
			string text = "";
			int num2 = 0;
			string text2 = "i";
			int num3 = (int)graphics.MeasureString(text2, NotationsFont, 1000, StringFormat.GenericTypographic).Width;
			string text3 = text2;
			string text4 = "";
			int num4 = 0;
			string text5 = DataUtil.ExtractOneInfo(ref InNotation, ';');
			string text6 = DataUtil.ExtractOneInfo(ref InNotation, ';');
			while ((text5 != "-1") & (text6 != "-1"))
			{
				text = DataUtil.Left(InString, Convert.ToInt32(text6));
				if (DataUtil.Right(text, 1) == " ")
				{
					text = DataUtil.Left(text, text.Length - 1) + text2;
				}
				num2 = (int)graphics.MeasureString(text, MainFont, 32000, StringFormat.GenericDefault).Width;
				while (graphics.MeasureString(text3, NotationsFont, 32000, StringFormat.GenericDefault).Width < (float)(num2 + num3))
				{
					text3 = DataUtil.Left(text3, text3.Length - 1) + " " + text2;
					num4++;
				}
				text3 = ((text3.Length - 2 < 0 || !(text3[text3.Length - 2].ToString() != " ")) ? (DataUtil.Left(text3, text3.Length - 1) + text5 + text2) : (DataUtil.Left(text3, text3.Length - 1) + " " + text5 + text2));
				if (PB_PrinterSpaces > 0)
				{
					int num5 = num4 / 12;
					for (int i = 1; i <= num4 + num5; i++)
					{
						text4 += " ";
					}
					text4 += text5;
				}
				num4 = 0;
				text5 = DataUtil.ExtractOneInfo(ref InNotation, ';');
				text6 = DataUtil.ExtractOneInfo(ref InNotation, ';');
			}
			if (DataUtil.Right(text3, 1) == text2)
			{
				text3 = DataUtil.Left(text3, text3.Length - 1);
			}
			if (text3 != "")
			{
				if (PB_PrinterSpaces > 0)
				{
					return text4;
				}
				return text3;
			}
			return " ";
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos)
		{
			return GetAssociatedLyricsLineCurPosX(ref IntextBox, InCurPos, 0);
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos, int LyricsCurPosMin)
		{
			return GetAssociatedLyricsLineCurPosX(ref IntextBox, InCurPos, LyricsCurPosMin, 64000);
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos, int LyricsCurPosMin, int LyricsCurPosMax)
		{
			if (InCurPos < 0)
			{
				InCurPos = 0;
			}
			if (InCurPos > LyricsCurPosMax - LyricsCurPosMin + 1)
			{
				Point positionFromCharIndex = IntextBox.GetPositionFromCharIndex(LyricsCurPosMax - LyricsCurPosMin + 1);
				Point positionFromCharIndex2 = IntextBox.GetPositionFromCharIndex(LyricsCurPosMax - LyricsCurPosMin);
				return positionFromCharIndex.X + (positionFromCharIndex.X - positionFromCharIndex2.X);
			}
			return IntextBox.GetPositionFromCharIndex(InCurPos + LyricsCurPosMin).X;
		}

		public static void GetMinMaxfromTextBox(RichTextBox InBox, int InLineNumber, ref int InMin, ref int InMax)
		{
			int num = 0;
			string text = InBox.Text + "\n";
			InMax = -1;
			for (int i = 0; i <= InLineNumber; i++)
			{
				InMin = InMax + 1;
				InMax = text.IndexOf("\n", InMin);
				if (InMax < 0)
				{
					i = InLineNumber;
				}
			}
			InMax--;
		}

		public static void GetMinMaxfromTextString(string InString, int InLineNumber, ref int InMin, ref int InMax)
		{
			if (InString == "")
			{
				InMin = -1;
				InMax = -1;
			}
			int num = 0;
			int num2 = 0;
			string text = InString + "\n";
			InMax = -1;
			for (num = 0; num <= InLineNumber; num++)
			{
				InMin = InMax + 1;
				InMax = text.IndexOf("\n", InMin);
				if (InMax < 0)
				{
					InMin = -1;
					InMax = -1;
					num = InLineNumber;
				}
			}
			InMax--;
		}

		public static int GetVerseIndicator(string InString)
		{
			for (int i = 0; i <= 150; i++)
			{
				if (((i < 13) | (i >= 100 && i <= 112) | (i == 150)) && InString.IndexOf(VerseSymbol[i]) >= 0)
				{
					return i;
				}
			}
			return -1;
		}

		public static string TransposeOneNotationString(string NotationString, int TransposeTo, int FlatSharpKey)
		{
			string text = "";
			while (NotationString != "")
			{
				string text2 = TransposeChord(DataUtil.ExtractOneInfo(ref NotationString, ';'), TransposeTo, FlatSharpKey);
				string text3 = DataUtil.ExtractOneInfo(ref NotationString, ';');
				object obj = text;
				text = string.Concat(obj, text2, ';', text3, ';');
			}
			return text;
		}

		public static int ExtractOneNotationsLine(ref string InString, ref string ResultString)
		{
			if ((InString == null) | (InString == ""))
			{
				ResultString = "";
				return -1;
			}
			int num = 0;
			int num2 = 0;
			string str = "";
			int num3 = -1;
			num = InString.IndexOf("(");
			if (num < 0)
			{
				return -1;
			}
			num2 = InString.IndexOf(")", num);
			if (num2 < num)
			{
				return -1;
			}
			num++;
			str += DataUtil.Mid(InString, num, num2 - num);
			InString = DataUtil.Mid(InString, num2 + ")".Length);
			num3 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref str, ';'));
			ResultString = str;
			return num3;
		}

		public static void ChangeNotationLineNumber(ref string InOneNotationLine, int InNewLineNumber)
		{
			if (InOneNotationLine.Length > 0)
			{
				string InString = InOneNotationLine;
				string text = DataUtil.ExtractOneInfo(ref InString, ';');
				int num = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, ';'));
				if (text != "-1" && num >= 0)
				{
					InOneNotationLine = text + ';' + InNewLineNumber + ';' + InString;
				}
			}
		}

		public static void BuildSlides(SongSettings InItem, ListView LyricsLists, ref string SongSequence, ref int CurSongMaxSlides, ref int[] SongVerses, ref int[] ChorusSlides, ref int[,] Slide, int InShowNotations)
		{
			int[,] Reg1SubLoc = new int[10000, 4];
			int folderNo = 1;
			for (int i = 0; i <= 9; i++)
			{
				SongVerses[i] = 0;
				ChorusSlides[i] = 0;
			}
			CurSongMaxSlides = 0;
			if (InItem != null)
			{
				InItem.Verse2Present = InItem.VersePresent[2];
				folderNo = InItem.FolderNo;
			}
			Slide[0, 3] = -1;
			foreach (int num in SongSequence)
			{
				bool lastSubScreen = false;
				if (InItem.VersePresent[num])
				{
					int verseScreenCount = GetVerseScreenCount(LyricsLists, InItem.VerseLineLoc[num, 1], InItem.VerseLineLoc[num, 2]);
					if (verseScreenCount > 0)
					{
						try
						{
							for (int j = 1; j <= verseScreenCount; j++)
							{
								int verseScreenLoc = GetVerseScreenLoc(LyricsLists, InItem.VerseLineLoc[num, 1], InItem.VerseLineLoc[num, 2], j);
								if (verseScreenLoc >= 0)
								{
									CurSongMaxSlides++;
									if (j == 1)
									{
										Slide[CurSongMaxSlides, 0] = num;
									}
									else
									{
										Slide[CurSongMaxSlides, 0] = -1;
									}
									Slide[CurSongMaxSlides, 1] = verseScreenLoc;
									Slide[CurSongMaxSlides, 2] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, InItem.VerseLineLoc[num, 2]);
									Slide[CurSongMaxSlides, 3] = -1;
									Slide[CurSongMaxSlides, 4] = -1;
									Reg1SubLoc[0, 0] = 1;
									Reg1SubLoc[1, 1] = Slide[CurSongMaxSlides, 1];
									Reg1SubLoc[1, 2] = Slide[CurSongMaxSlides, 2];
									if (InItem != null && Slide[CurSongMaxSlides, 2] >= 0 && InItem.SplitScreens && GetScreensRequired(InItem, InItem.Lyrics[0], ref LyricsLists, Slide[CurSongMaxSlides, 1], Slide[CurSongMaxSlides, 2], ref Reg1SubLoc, InItem.VersePresent[150], 0, folderNo, InShowNotations) > 1)
									{
										CurSongMaxSlides--;
										for (int k = 1; k <= Reg1SubLoc[0, 0]; k++)
										{
											CurSongMaxSlides++;
											if (j == 1 && k == 1)
											{
												Slide[CurSongMaxSlides, 0] = num;
											}
											else
											{
												Slide[CurSongMaxSlides, 0] = -1;
											}
											Slide[CurSongMaxSlides, 1] = Reg1SubLoc[k, 1];
											Slide[CurSongMaxSlides, 2] = Reg1SubLoc[k, 2];
											Slide[CurSongMaxSlides, 3] = -1;
											Slide[CurSongMaxSlides, 4] = -1;
										}
									}
									if ((InItem.VerseLineLoc[num, 3] >= 0) & (InItem.VerseLineLoc[num, 4] >= InItem.VerseLineLoc[num, 3]))
									{
										Slide[0, 3] = 1;
										if (j == verseScreenCount)
										{
											lastSubScreen = true;
										}
										BuildSlidesReg2(LyricsLists, InItem.VerseLineLoc[num, 3], InItem.VerseLineLoc[num, 4], ref Slide, ref CurSongMaxSlides, Reg1SubLoc, j, lastSubScreen);
									}
								}
							}
						}
						catch
						{
						}
					}
					else
					{
						try
						{
							Slide[0, 3] = 2;
							verseScreenCount = GetVerseScreenCount(LyricsLists, InItem.VerseLineLoc[num, 3], InItem.VerseLineLoc[num, 4]);
							for (int j = 1; j <= verseScreenCount; j++)
							{
								int verseScreenLoc = GetVerseScreenLoc(LyricsLists, InItem.VerseLineLoc[num, 3], InItem.VerseLineLoc[num, 4], j);
								if (verseScreenLoc >= 0)
								{
									CurSongMaxSlides++;
									if (j == 1)
									{
										Slide[CurSongMaxSlides, 0] = num;
									}
									else
									{
										Slide[CurSongMaxSlides, 0] = -1;
									}
									Slide[CurSongMaxSlides, 1] = -1;
									Slide[CurSongMaxSlides, 2] = -1;
									Slide[CurSongMaxSlides, 3] = verseScreenLoc;
									Slide[CurSongMaxSlides, 4] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, InItem.VerseLineLoc[num, 4]);
									Reg1SubLoc[0, 0] = 1;
									Reg1SubLoc[1, 1] = Slide[CurSongMaxSlides, 3];
									Reg1SubLoc[1, 2] = Slide[CurSongMaxSlides, 4];
									if (InItem != null && Slide[CurSongMaxSlides, 4] >= 0 && InItem.SplitScreens && GetScreensRequired(InItem, InItem.Lyrics[1], ref LyricsLists, Slide[CurSongMaxSlides, 3], Slide[CurSongMaxSlides, 4], ref Reg1SubLoc, Region2Present: true, 1, folderNo, InShowNotations) > 1)
									{
										CurSongMaxSlides--;
										for (int k = 1; k <= Reg1SubLoc[0, 0]; k++)
										{
											CurSongMaxSlides++;
											if (k == 1)
											{
												Slide[CurSongMaxSlides, 0] = num;
											}
											else
											{
												Slide[CurSongMaxSlides, 0] = -1;
											}
											Slide[CurSongMaxSlides, 1] = -1;
											Slide[CurSongMaxSlides, 2] = -1;
											Slide[CurSongMaxSlides, 3] = Reg1SubLoc[k, 1];
											Slide[CurSongMaxSlides, 4] = Reg1SubLoc[k, 2];
										}
									}
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			try
			{
				int num2 = 1;
				for (int i = CurSongMaxSlides; i >= 1; i--)
				{
					if ((Slide[i, 0] > 0) & (Slide[i, 0] <= 9))
					{
						SongVerses[Slide[i, 0]] = i;
					}
					else if (Slide[i, 0] == 0 && num2 <= 9)
					{
						SongVerses[0] = i;
						ChorusSlides[num2] = i;
						num2++;
					}
				}
			}
			catch
			{
			}
		}

		public static int GetVerseScreenCount(ListView LyricsLists, int StartLoc, int EndLoc)
		{
			if ((StartLoc > EndLoc) | (StartLoc < 0) | (EndLoc < 0))
			{
				return -1;
			}
			int num = 0;
			for (int i = StartLoc; i <= EndLoc; i++)
			{
				if ((LyricsLists.Items[i].SubItems[2].Text == "") | (i == EndLoc))
				{
					num++;
				}
			}
			return num;
		}

		public static int GetVerseScreenLoc(ListView LyricsLists, int StartLoc, int EndLoc, int InScreenNumber)
		{
			if (StartLoc > EndLoc)
			{
				return -1;
			}
			if (StartLoc + 1 <= EndLoc && LyricsLists.Items[StartLoc].SubItems[2].Text == "")
			{
				StartLoc++;
			}
			int num = 1;
			for (int i = StartLoc; i <= EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "")
				{
					num++;
				}
				if (InScreenNumber == num)
				{
					if (LyricsLists.Items[i].SubItems[2].Text != "")
					{
						return i;
					}
					if (i < EndLoc)
					{
						return i + 1;
					}
					return -1;
				}
			}
			return -1;
		}

		public static int GetVerseScreenEndLoc(ListView LyricsLists, int StartLoc, int EndLoc)
		{
			if (StartLoc > EndLoc)
			{
				return -1;
			}
			if (StartLoc == EndLoc)
			{
				return EndLoc;
			}
			if (LyricsLists.Items[StartLoc].SubItems[2].Text == "")
			{
				StartLoc++;
			}
			for (int i = StartLoc; i < EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "")
				{
					return i - 1;
				}
			}
			return EndLoc;
		}

		public static void BuildSlidesReg2(ListView LyricsLists, int StartLoc, int EndLoc, ref int[,] Slide, ref int CurSlideNumber, int[,] Reg1SubLoc, int InScreenNumber, bool LastSubScreen)
		{
			if ((StartLoc > EndLoc) | (Reg1SubLoc[0, 0] < 1))
			{
				return;
			}
			int num = -1;
			int num2 = EndLoc;
			num = GetVerseScreenLoc(LyricsLists, StartLoc, EndLoc, InScreenNumber);
			if (num < 0)
			{
				return;
			}
			num2 = GetVerseScreenEndLoc(LyricsLists, num, EndLoc);
			int num3 = num;
			for (int i = 1; i <= Reg1SubLoc[0, 0]; i++)
			{
				int num4 = CurSlideNumber - Reg1SubLoc[0, 0] + i;
				int num5 = Slide[num4, 2] - Slide[num4, 1] + 1;
				if (num5 <= 0 || num3 < 0)
				{
					continue;
				}
				Slide[num4, 3] = num3;
				if (num3 < 0)
				{
					continue;
				}
				if (num2 - num3 + 1 >= num5)
				{
					if (i == Reg1SubLoc[0, 0])
					{
						Slide[num4, 4] = num2;
						num3 = -1;
					}
					else
					{
						Slide[num4, 4] = num3 + num5 - 1;
						num3 += num5;
					}
				}
				else
				{
					Slide[num4, 4] = num2;
					num3 = -1;
				}
			}
			if (EndLoc <= num2 || !LastSubScreen)
			{
				return;
			}
			num = num2 + 1;
			for (int j = 1; j <= GetVerseScreenCount(LyricsLists, num, EndLoc); j++)
			{
				int verseScreenLoc = GetVerseScreenLoc(LyricsLists, num, EndLoc, j);
				if (verseScreenLoc >= 0)
				{
					CurSlideNumber++;
					Slide[CurSlideNumber, 0] = -1;
					Slide[CurSlideNumber, 1] = -1;
					Slide[CurSlideNumber, 2] = -1;
					Slide[CurSlideNumber, 3] = verseScreenLoc;
					Slide[CurSlideNumber, 4] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, EndLoc);
				}
			}
		}

		public static int GetScreensRequired(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, int StartLoc, int EndLoc, ref int[,] Reg1SubLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations)
		{
			Graphics g = LyricsLists.CreateGraphics();
			int num = 0;
			Reg1SubLoc[0, 0] = num;
			ListViewItem listViewItem = new ListViewItem();
			int num2 = StartLoc;
			int endline = EndLoc;
			while (GetOneScreen(InItem, InLyricsFormat, ref LyricsLists, g, num2, ref endline, EndLoc, Region2Present, RegionNumber, FolderNo, InShowNotations) > 0)
			{
				num++;
				Reg1SubLoc[num, 1] = num2;
				Reg1SubLoc[num, 2] = endline;
				Reg1SubLoc[num, 3] = endline - num2 + 1;
				Reg1SubLoc[0, 0] = num;
				num2 = endline + 1;
				endline = EndLoc;
			}
			return num;
		}

		public static int GetOneScreen(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, Graphics g, int startline, ref int endline, int EndLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations)
		{
			return GetOneScreen(InItem, InLyricsFormat, ref LyricsLists, g, startline, ref endline, EndLoc, Region2Present, RegionNumber, FolderNo, InShowNotations, FitAllIntoOneScreen: false, UseLargestFontSize: false);
		}

		public static int GetOneScreen(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, Graphics g, int startline, ref int endline, int EndLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (endline < startline)
			{
				return 0;
			}
			Font MainFont = new Font(InLyricsFormat.Font.Name, InLyricsFormat.Font.Size, InLyricsFormat.Font.Style);
			SizeF layoutArea = new SizeF(InLyricsFormat.FS_Width, 32000f);
			string text = (RegionNumber == 0) ? "\n" : "";
			double num = MainFontSpacingFactor[FolderNo, 0] + ((InShowNotations > 0) ? NotationFontFactor : 0.0);
			int num2 = (int)((double)(Region2Present ? InLyricsFormat.FS_Height_R2Bound : InLyricsFormat.FS_Height) / num);
			string text2 = "";
			for (int i = startline; i <= endline; i++)
			{
				text2 = text2 + LyricsLists.Items[i].SubItems[2].Text + "\n";
			}
			if (text2.Length > 1)
			{
				text2 = DataUtil.Left(text2, text2.Length - 1);
			}
			if ((!AutoTextOverflow || InItem.RotateStyle == 2) && InItem.Type != "B")
			{
				ReduceFontToFit(g, text2, ref MainFont, InLyricsFormat.FS_Width, num2, MultiLine: true);
			}
			int num3 = (int)g.MeasureString("A", InLyricsFormat.FS_Font, layoutArea).Height;
			int num4 = 0;
			bool flag = true;
			for (int i = startline; i <= EndLoc; i++)
			{
				if (i == startline)
				{
					ReduceFontToFit(g, LyricsLists.Items[i].SubItems[2].Text, ref MainFont, InLyricsFormat.FS_Width, num2, MultiLine: true);
				}
				num4 += GetLinesRequiredAndAddBreakPlusFont(ref LyricsLists, MainFont, g, i, layoutArea.Width) * num3;
				if ((num4 > num2) & ((AutoTextOverflow & (InItem.RotateStyle != 2)) || InItem.Type == "B"))
				{
					endline = ((i > startline) ? (i - 1) : startline);
					return 1;
				}
			}
			endline = EndLoc;
			return 1;
		}

		public static string ActionWordWrapSpacesAtStart(ref string InString)
		{
			string text = "";
			if (WordWrapIgnoreStartSpaces > 0 && InString.Length > WordWrapIgnoreStartSpaces)
			{
				string text2 = DataUtil.Left(InString, WordWrapIgnoreStartSpaces);
				for (int i = 0; i < text2.Length; i++)
				{
					if (text2[i].ToString() == " ")
					{
						text = text + i.ToString() + ';';
					}
				}
				text2 = text2.Replace(" ", "_");
				InString = text2 + DataUtil.Mid(InString, WordWrapIgnoreStartSpaces);
			}
			return text;
		}

		public static void ActionUndoWordWrapSpacesAtStart(ref string ExtractedText, ref string ReplacedLog)
		{
			int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
			string text = ExtractedText;
			while (num < ExtractedText.Length && num >= 0)
			{
				text = text.Remove(num, 1);
				text = text.Insert(num, " ");
				num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
			}
			ExtractedText = text;
			if (num > ExtractedText.Length)
			{
				ReplacedLog = num.ToString() + ';' + ReplacedLog;
			}
			int num2 = num;
			text = "";
			while (ReplacedLog != "")
			{
				num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
				text = text + Convert.ToString(num - num2) + ';';
			}
			ReplacedLog = text;
		}

		public static int GetLinesRequiredAndAddBreakPlusFont(ref ListView LyricsLists, Font MainFont, Graphics g, int LyricsIndex, float InWidth)
		{
			string InString = LyricsLists.Items[LyricsIndex].SubItems[2].Text;
			ActionWordWrapSpacesAtStart(ref InString);
			int length = InString.Length;
			if (length == 0)
			{
				return 0;
			}
			int num = 1;
			int num2 = 0;
			bool flag = false;
			int num3 = 1;
			int num4 = 0;
			LyricsLists.Items[LyricsIndex].SubItems[4].Text = "";
			for (int i = 1; i <= length; i++)
			{
				if (DataUtil.Mid(InString, i, 1) == " ")
				{
					num2 = i;
					flag = true;
				}
				else if (g.MeasureString(DataUtil.Mid(InString, num, i - num + 1), MainFont).Width > InWidth)
				{
					if (flag)
					{
						num4++;
						num = num2 + 1;
						ListViewItem.ListViewSubItem listViewSubItem = LyricsLists.Items[LyricsIndex].SubItems[4];
						listViewSubItem.Text = listViewSubItem.Text + Convert.ToString(num - num3) + '>';
						num3 = num;
						flag = false;
					}
					else
					{
						num4++;
						num = i;
						ListViewItem.ListViewSubItem listViewSubItem2 = LyricsLists.Items[LyricsIndex].SubItems[4];
						listViewSubItem2.Text = listViewSubItem2.Text + Convert.ToString(num - num3) + '>';
						num3 = num;
					}
				}
			}
			ListViewItem.ListViewSubItem listViewSubItem3 = LyricsLists.Items[LyricsIndex].SubItems[4];
			listViewSubItem3.Text = listViewSubItem3.Text + Convert.ToString(length - num3 + 1) + '>';
			LyricsLists.Items[LyricsIndex].SubItems[4].Text = Convert.ToString(num4 + 1) + '>' + Convert.ToString(MainFont.Size) + '>' + LyricsLists.Items[LyricsIndex].SubItems[4].Text;
			LyricsArray[LyricsIndex, 2] = LyricsLists.Items[LyricsIndex].SubItems[4].Text;
			return num4 + 1;
		}

		public static ImageTransitionControl.TransitionTypes ComputeTransition(SongSettings InItem, ref ImageTransitionControl InPictureBox, ImageTransitionControl.TransitionAction TransitionAction)
		{
			bool flag = false;
			if ((ShowLiveCam & InItem.AtLiveScreen) || (InItem.OutputStyleScreen && ShowLiveBlack && TransitionAction != ImageTransitionControl.TransitionAction.AsFade))
			{
				flag = true;
			}
			if (InItem.PrevItemPP || flag)
			{
				InPictureBox.TransitionType = ImageTransitionControl.TransitionTypes.None;
			}
			else if (TransitionAction != ImageTransitionControl.TransitionAction.AsStored)
			{
				if (InItem.FirstShowing)
				{
					InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowItemTransition;
				}
				else
				{
					switch (TransitionAction)
					{
						case ImageTransitionControl.TransitionAction.AsStoredItem:
							InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowItemTransition;
							break;
						case ImageTransitionControl.TransitionAction.None:
							InPictureBox.TransitionType = ImageTransitionControl.TransitionTypes.None;
							break;
						default:
							InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowSlideTransition;
							break;
					}
				}
			}
			return InPictureBox.TransitionType;
		}

		public static int GetOneRegionHeight(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, int RegNum, ListView LyricsAndNotationsList, ref Graphics g, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, bool OnlyOneRegionShown, ref Font MainFont, ref Font NotationsFont, int InterlaceOption, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (InItem.LyricsAndNotationsList.Items.Count == 0)
			{
				return 0;
			}
			int num = (int)InItem.Lyrics[RegNum].FS_Font.Size;
			int num2 = (int)((double)num * NotationFontFactor);
			string text = "";
			string text2 = "";
			int num3 = 0;
			int fS_Left = InItem.Lyrics[RegNum].FS_Left;
			int fS_Top = InItem.Lyrics[RegNum].FS_Top;
			int fS_Width = InItem.Lyrics[RegNum].FS_Width;
			int num4 = (InItem.Slide[0, 3] > 0 && !OnlyOneRegionShown) ? InItem.Lyrics[RegNum].FS_Height_R2Bound : InItem.Lyrics[RegNum].FS_Height;
			num3 = InItem.Lyrics[RegNum].FS_Top + InItem.Lyrics[RegNum].FS_Height;
			int fS_Height_R2Bound = InItem.Lyrics[1].FS_Height_R2Bound;
			SizeF layoutArea = new SizeF(fS_Width, 32000f);
			int num5 = 0;
			int num6 = 0;
			double num7 = MainFontSpacingFactor[InItem.FolderNo, (RegNum != 0) ? 1 : 0] + ((InShowNotations > 0) ? NotationFontFactor : 0.0);
			int num8;
			int num9;
			switch (RegNum)
			{
				case 0:
					num8 = InItem.Slide[InItem.CurSlide, 1];
					num9 = InItem.Slide[InItem.CurSlide, 2];
					break;
				case 1:
					num8 = InItem.Slide[InItem.CurSlide, 3];
					num9 = InItem.Slide[InItem.CurSlide, 4];
					break;
				default:
					text = InItem.Lyrics[RegNum].Text;
					MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref text);
					}
					ReduceFontToFit(g, text, ref MainFont, fS_Width, num4, MultiLine: true);
					return num4;
			}
			if ((num8 < 0) | (num9 < 0))
			{
				return 0;
			}
			if (num8 <= num9)
			{
				string text3 = "";
				for (int i = num8; i <= num9; i++)
				{
					text2 = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text;
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref text2);
					}
					text = text + text2 + "\n";
					text3 = InItem.LyricsAndNotationsList.Items[i].SubItems[4].Text;
					num5 += DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref text3, '>', RemoveExtract: false));
					num6++;
				}
				if (text.Length > 1)
				{
					text = DataUtil.Left(text, text.Length - 1);
				}
			}
			else
			{
				num6 = 1;
			}
			num4 = (int)((double)num4 / num7);
			int num10;
			if (UseLargestFontSize)
			{
				MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
				bool OnlyOneDisplayLine = false;
				num10 = IncreaseFontToLargest(g, text, ref MainFont, fS_Width, num4, ref OnlyOneDisplayLine);
				NotationsFont = new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style);
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
				return (int)((double)num10 * num7);
			}
			if (num5 > 0)
			{
				string InString = InItem.LyricsAndNotationsList.Items[num8].SubItems[4].Text;
				DataUtil.ExtractOneInfo(ref InString, '>');
				num = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				if (num < 1)
				{
					num = (int)InItem.Lyrics[RegNum].Font.Size;
				}
				MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
				NotationsFont = new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style);
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
				return InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight * num5;
			}
			MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
			ReduceFontToFit(g, text, ref MainFont, fS_Width, num4, MultiLine: true);
			num10 = num4;
			NotationsFont = new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style);
			InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
			return (int)((double)num10 * num7);
		}

		public static void SubstituteDashes(ref string ExtractedText, int InShowNotations)
		{
			if (InShowNotations < 1)
			{
				ExtractedText = ExtractedText.Replace(DashesString, DashesStringSubstitute);
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "---", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "--", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "-", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute, "");
			}
		}

		public static void SubDivideOneOutputText(string InText, Font MainFont, Font NotationsFont, Graphics g, int InWidth, int InShowNotations, string NotationsString, int TextLength, int InSetLength, int StartPos, ref int EndExtractedTextPos, ref string ExtractedText)
		{
			int num = -1;
			bool flag = false;
			int num2 = 0;
			if (InSetLength == 0)
			{
				ExtractedText = InText;
				EndExtractedTextPos = TextLength;
				return;
			}
			if (InSetLength > 0)
			{
				for (int i = StartPos; i <= StartPos + InSetLength - 1; i++)
				{
					if (DataUtil.Mid(InText, i, 1) == " ")
					{
						StartPos++;
						InSetLength--;
					}
					else
					{
						i = StartPos + InSetLength;
					}
				}
				ExtractedText = DataUtil.Mid(InText, StartPos, InSetLength);
				EndExtractedTextPos = StartPos + InSetLength - 1;
				return;
			}
			for (int i = StartPos; i <= TextLength; i++)
			{
				if (DataUtil.Mid(InText, i, 1) == " ")
				{
					num = i;
					flag = true;
					if (i == TextLength)
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, TextLength - StartPos + 1);
						EndExtractedTextPos = TextLength;
					}
				}
				else if (g.MeasureString(DataUtil.Mid(InText, StartPos, i - StartPos + 1), MainFont).Width > (float)InWidth)
				{
					if (flag)
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, num - StartPos);
						num2 = num + 1;
						EndExtractedTextPos = num2 - 1;
						i = TextLength;
					}
					else
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, i - StartPos);
						num2 = i;
						EndExtractedTextPos = num2 - 1;
						i = TextLength;
					}
				}
				else if (i == TextLength)
				{
					ExtractedText = DataUtil.Mid(InText, StartPos, TextLength - StartPos + 1);
					EndExtractedTextPos = TextLength;
				}
			}
			if (num2 <= 0)
			{
				return;
			}
			for (int j = EndExtractedTextPos + 1; j <= TextLength; j++)
			{
				if (DataUtil.Mid(InText, j, 1) == " ")
				{
					EndExtractedTextPos++;
				}
				else
				{
					j = TextLength + 1;
				}
			}
		}

		public static void ReverseString(ref string InString)
		{
			if (InString.Length != 0)
			{
				string text = "";
				for (int num = InString.Length - 1; num >= 0; num--)
				{
					text += InString[num];
				}
				InString = text;
			}
		}

		public static void LoadRegistryMainEditHistory()
		{
			MaxUserEditHistory = RegUtil.GetRegValue("options", "MaxEditHistory", 10);
			if ((MaxUserEditHistory < 0) | (MaxUserEditHistory > AbsoluteMaxHitoryItems))
			{
				MaxUserEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				MainEditHistoryList[i, 0] = "";
			}
			TotalMainEditHistory = AbsoluteMaxHitoryItems;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				MainEditHistoryList[i, 0] = RegUtil.GetRegValue("maineditlist", i.ToString(), "");
				if (MainEditHistoryList[i, 0] == "")
				{
					TotalMainEditHistory = i - 1;
				}
			}
			ValidateMainHistoryItems();
		}

		public static void LoadRegistryEditorEditHistory()
		{
			MaxUserEditHistory = RegUtil.GetRegValue("options", "MaxEditHistory", 10);
			if ((MaxUserEditHistory < 0) | (MaxUserEditHistory > AbsoluteMaxHitoryItems))
			{
				MaxUserEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				EditorEditHistoryList[i, 0] = "";
			}
			TotalEditorEditHistory = AbsoluteMaxHitoryItems;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				EditorEditHistoryList[i, 0] = RegUtil.GetRegValue("editoreditlist", i.ToString(), "");
				if (EditorEditHistoryList[i, 0] == "")
				{
					TotalEditorEditHistory = i - 1;
				}
			}
			ValidateEditorHistoryItems();
		}

		public static void LoadRegistryInfoScreenEditHistory()
		{
			MaxUserEditHistory = RegUtil.GetRegValue("options", "MaxEditHistory", 10);
			if ((MaxUserEditHistory < 0) | (MaxUserEditHistory > AbsoluteMaxHitoryItems))
			{
				MaxUserEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				InfoScreenEditHistoryList[i, 0] = "";
			}
			TotalInfoScreenEditHistory = MaxUserEditHistory;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				InfoScreenEditHistoryList[i, 0] = RegUtil.GetRegValue("infoscreeneditlist", i.ToString(), "");
				if (InfoScreenEditHistoryList[i, 0] == "")
				{
					TotalInfoScreenEditHistory = i - 1;
				}
			}
			ValidateInfoScreenHistoryItems();
		}

		public static string GetItemTitle(string InIDString)
		{
			InIDString = DataUtil.Trim(InIDString);
			if (InIDString == "")
			{
				return "";
			}
			string a = DataUtil.Left(InIDString, 1);
			string text = DataUtil.Mid(InIDString, 1);
			string InFileName = "";
			if (a == "D")
			{
				try
				{
					string fullSearchString = "select * from SONG where songid=" + text;

#if OleDb
					DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif

					if (datatable.Rows.Count>0 && DataUtil.GetDataInt(datatable.Rows[0], "FolderNo") > 0 && FolderUse[DataUtil.GetDataInt(datatable.Rows[0], "FolderNo")] > 0)
					{
						InFileName = DataUtil.GetDataString(datatable.Rows[0], "Title_1");
					}
				}
				catch
				{
				}
			}
			else if (a == "P")
			{
				InFileName = text;
			}
			else if (!(a == "B"))
			{
				if (a == "T")
				{
					InFileName = text;
				}
				else if (a == "I")
				{
					InFileName = text;
					GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				}
				else if (a == "W")
				{
					InFileName = text;
				}
				else if (a == "M")
				{
					InFileName = text;
				}
			}
			return InFileName;
		}

		public static void SaveMainEditHistoryToRegistry()
		{
			RegUtil.SaveRegValue("options", "MaxEditHistory", MaxUserEditHistory);
			if (TotalMainEditHistory > MaxUserEditHistory)
			{
				TotalMainEditHistory = MaxUserEditHistory;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				if (i <= TotalMainEditHistory)
				{
					RegUtil.SaveRegValue("maineditlist", i.ToString(), MainEditHistoryList[i, 0]);
					continue;
				}
				RegUtil.SaveRegValue("maineditlist", i.ToString(), "");
				MainEditHistoryList[i, 0] = "";
			}
		}

		public static void RemoveDuplicateEditorHistoryItems(ref string[,] InEditHistoryList, ref int InTotalEditorEditHistory)
		{
			int num = (InTotalEditorEditHistory <= AbsoluteMaxHitoryItems) ? InTotalEditorEditHistory : AbsoluteMaxHitoryItems;
			bool flag = false;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (num3 = 1; num3 <= AbsoluteMaxHitoryItems; num3++)
			{
				TempEditHistoryList[num3, 0] = "";
				TempEditHistoryList[num3, 1] = "";
			}
			for (num3 = 1; num3 <= num; num3++)
			{
				if (num3 == 1)
				{
					num2 = 1;
					TempEditHistoryList[1, 0] = InEditHistoryList[1, 0];
					TempEditHistoryList[1, 1] = InEditHistoryList[1, 1];
					continue;
				}
				flag = false;
				for (num4 = 1; num4 <= num2; num4++)
				{
					if (InEditHistoryList[num3, 0] == TempEditHistoryList[num4, 0])
					{
						flag = true;
					}
				}
				if (!flag)
				{
					num2++;
					TempEditHistoryList[num2, 0] = InEditHistoryList[num3, 0];
					TempEditHistoryList[num2, 1] = InEditHistoryList[num3, 1];
				}
			}
			for (num3 = 1; num3 <= AbsoluteMaxHitoryItems; num3++)
			{
				InEditHistoryList[num3, 0] = TempEditHistoryList[num3, 0];
				InEditHistoryList[num3, 1] = TempEditHistoryList[num3, 1];
			}
			InTotalEditorEditHistory = num2;
		}

		public static void SaveEditorEditHistoryToRegistry()
		{
			RegUtil.SaveRegValue("options", "MaxEditHistory", MaxUserEditHistory);
			if (TotalEditorEditHistory > MaxUserEditHistory)
			{
				TotalEditorEditHistory = MaxUserEditHistory;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				if (i <= TotalEditorEditHistory)
				{
					RegUtil.SaveRegValue("editoreditlist", i.ToString(), EditorEditHistoryList[i, 0]);
					continue;
				}
				RegUtil.SaveRegValue("editoreditlist", i.ToString(), "");
				EditorEditHistoryList[i, 0] = "";
			}
		}

		public static void SaveInfoScreenEditHistoryToRegistry()
		{
			RegUtil.SaveRegValue("options", "MaxEditHistory", MaxUserEditHistory);
			if (TotalInfoScreenEditHistory > MaxUserEditHistory)
			{
				TotalInfoScreenEditHistory = MaxUserEditHistory;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				if (i <= TotalInfoScreenEditHistory)
				{
					RegUtil.SaveRegValue("infoscreeneditlist", i.ToString(), InfoScreenEditHistoryList[i, 0]);
					continue;
				}
				RegUtil.SaveRegValue("infoscreeneditlist", i.ToString(), "");
				InfoScreenEditHistoryList[i, 0] = "";
			}
		}

		public static int DP_SetSlideIndicators(SongSettings InItem, ref ImageTransitionControl InPic, ref Graphics g, Font tempFont, RectangleF rect_slidesinfo)
		{
			int num = (InItem.Source == ItemSource.WorshipList) ? InItem.CurItemNo : 0;
			int totalWorshipListItems = TotalWorshipListItems;
			Color color = (PanelTextColourAsRegion1 > 0) ? InItem.Lyrics[3].ForeColour : PanelTextColour;
			StringFormat stringFormat = new StringFormat();
			string text = "";
			string text2 = "";
			FontStyle fontStyle = FontStyle.Regular;
			if (ShowDataDisplayFontBold > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ShowDataDisplayFontItalic > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			tempFont = new Font(tempFont.Name, tempFont.Size, fontStyle);
			int num2 = (int)g.MeasureString("1", tempFont, 10000).Height;
			int num3 = (int)(rect_slidesinfo.Top + rect_slidesinfo.Height / 20f);
			int num4 = (int)(rect_slidesinfo.Top + rect_slidesinfo.Height / 2f);
			int num5 = num4 - num2 / 20;
			int num6 = 0;
			if (ShowDataDisplaySlides > 0)
			{
				int num7 = DataDisplaySlides(InItem, ref g, tempFont, color, rect_slidesinfo, num3, num4, num5, 0, DisplayIndicators: false);
				num6 = (int)rect_slidesinfo.Width - num7;
				DataDisplaySlides(InItem, ref g, tempFont, color, rect_slidesinfo, num3, num4, num5, num6, DisplayIndicators: true);
				num6 -= (int)g.MeasureString("1", tempFont, 10000).Width;
			}
			else
			{
				num6 = (int)rect_slidesinfo.Width;
			}
			if (ShowDataDisplaySongs > 0)
			{
				text2 = ((num > 0) ? num.ToString() : "A");
				int num8 = (int)g.MeasureString(text2, tempFont, 10000).Width;
				text = totalWorshipListItems.ToString();
				int num9 = (int)g.MeasureString(text, tempFont, 10000).Width;
				num6 -= num9;
				OutputOneLineToScreen(InItem, text2, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, num6 + (num9 - num8) / 2, num3, num6 + num9, 0);
				OutputOneLineToScreen(InItem, text, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, num6, num4, num6 + num9, 0);
				g.DrawLine(new Pen(color), num6, num5, num6 + num9, num5);
			}
			return num6;
		}

		public static string ConvertSequenceSymbol(string InSymbol)
		{
			if (InSymbol == SequenceSymbol[103])
			{
				return SequenceSymbol[100].ToUpper();
			}
			if (InSymbol == SequenceSymbol[102])
			{
				return SequenceSymbol[0].ToUpper();
			}
			if (InSymbol == SequenceSymbol[112])
			{
				return SequenceSymbol[111].ToUpper();
			}
			return InSymbol;
		}

		public static void WriteXMLSessionHeader(ref XmlTextWriter xtw, string InFormatString, string InNotes)
		{
			int num = MediaMute + MediaRepeat * 2 + MediaWidescreen * 4;
			StringBuilder stringBuilder = new StringBuilder();
			if (InFormatString != "")
			{
				stringBuilder.Append(InFormatString);
			}
			else
			{
				stringBuilder.Append(Convert.ToString(11) + "=" + Convert.ToString(PanelBackColour.ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(12) + "=" + Convert.ToString(PanelBackColourTransparent) + '>');
				stringBuilder.Append(Convert.ToString(13) + "=" + Convert.ToString(PanelTextColour.ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(14) + "=" + Convert.ToString(PanelTextColourAsRegion1) + '>');
				stringBuilder.Append(Convert.ToString(15) + "=" + Convert.ToString(ShowDataDisplayMode + ShowDataDisplaySlides * 2 + ShowDataDisplaySongs * 4 + ShowDataDisplayTitle * 8 + ShowDataDisplayCopyright * 16 + ShowDataDisplayPrevNext * 32) + '>');
				stringBuilder.Append(Convert.ToString(16) + "=" + Convert.ToString((int)(BottomBorderFactor * 100.0)) + '>');
				stringBuilder.Append(Convert.ToString(17) + "=" + ShowDataDisplayFontName + '>');
				stringBuilder.Append(Convert.ToString(18) + "=" + Convert.ToString(ShowDataDisplayFontBold + ShowDataDisplayFontItalic * 2 + ShowDataDisplayFontUnderline * 4 + ShowDataDisplayFontShadow * 8 + ShowDataDisplayFontOutline * 16) + '>');
				stringBuilder.Append(Convert.ToString(19) + "=" + Convert.ToString(ShowDataDisplayIndicatorsFontSize) + '>');
				stringBuilder.Append(Convert.ToString(21) + "=" + Convert.ToString(ShowSongHeadings) + '>');
				stringBuilder.Append(Convert.ToString(23) + "=" + Convert.ToString(ShowSongHeadingsAlign) + '>');
				stringBuilder.Append(Convert.ToString(22) + "=" + Convert.ToString(UseShadowFont * 2 + ShowNotations * 4 + ShowCapoZero * 8 + ShowInterlace * 16 + UseOutlineFont * 32) + '>');
				stringBuilder.Append(Convert.ToString(25) + "=" + Convert.ToString(ShowLyrics) + '>');
				stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(ShowScreenColour[0].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(ShowScreenColour[1].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(28) + "=" + ShowScreenStyle.ToString() + '>');
				stringBuilder.Append(Convert.ToString(29) + "=" + Convert.ToString(ShowFontColour[0].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(30) + "=" + Convert.ToString(ShowFontColour[1].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(31) + "=" + Convert.ToString(ShowFontAlign[0, 0]) + '>');
				stringBuilder.Append(Convert.ToString(32) + "=" + Convert.ToString(ShowFontAlign[0, 1]) + '>');
				stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(MediaOption) + '>');
				stringBuilder.Append(Convert.ToString(51) + "=" + MediaLocation + '>');
				stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(MediaVolume) + '>');
				stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(MediaBalance) + '>');
				stringBuilder.Append(Convert.ToString(54) + "=" + num.ToString() + '>');
				stringBuilder.Append(Convert.ToString(55) + "=" + MediaCaptureDeviceNumber.ToString() + '>');
				stringBuilder.Append(Convert.ToString(56) + "=" + MediaOutputMonitorName + '>');
				stringBuilder.Append(Convert.ToString(61) + "=" + BackgroundPicture + '>');
				stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)BackgroundMode) + '>');
				stringBuilder.Append(Convert.ToString(63) + "=" + Convert.ToString(ShowVerticalAlign) + '>');
				stringBuilder.Append(Convert.ToString(64) + "=" + Convert.ToString(ShowLeftMargin[0]) + '>');
				stringBuilder.Append(Convert.ToString(65) + "=" + Convert.ToString(ShowRightMargin[0]) + '>');
				stringBuilder.Append(Convert.ToString(66) + "=" + Convert.ToString(ShowBottomMargin[0]) + '>');
				stringBuilder.Append(Convert.ToString(72) + "=" + GlobalImageCanvas.GetTransitionText(ShowItemTransition) + '>');
				stringBuilder.Append(Convert.ToString(73) + "=" + GlobalImageCanvas.GetTransitionText(ShowSlideTransition) + '>');
				for (int i = 0; i < 8; i++)
				{
					int num2 = 101 + i * 5;
					stringBuilder.Append(num2.ToString() + "=" + Convert.ToString(PB_ShowWords[i] + PB_WordsBold[i] * 2 + PB_WordsItalic[i] * 4 + PB_WordsUnderline[i] * 8) + '>');
					stringBuilder.Append(Convert.ToString(num2 + 1) + "=" + PB_WordsSize[i].ToString() + '>');
					stringBuilder.Append(Convert.ToString(num2 + 2) + "=" + Convert.ToString(PB_WordsColour[i].ToArgb()) + '>');
				}
				stringBuilder.Append(Convert.ToString(151) + "=" + Convert.ToString(PB_ShowHeadings[0] + PB_ShowHeadings[1] * 2 + PB_ShowHeadings[2] * 4 + PB_ShowHeadings[3] * 8) + '>');
				stringBuilder.Append(Convert.ToString(153) + "=" + Convert.ToString(PB_LyricsPattern) + '>');
				stringBuilder.Append(Convert.ToString(154) + "=" + Convert.ToString(PB_ShowSection) + '>');
				stringBuilder.Append(Convert.ToString(155) + "=" + Convert.ToString(PB_ShowColumns) + '>');
				stringBuilder.Append(Convert.ToString(156) + "=" + Convert.ToString(PB_PageSize) + '>');
				stringBuilder.Append(Convert.ToString(170) + "=" + Convert.ToString(PB_Spacing[0]) + '>');
				stringBuilder.Append(Convert.ToString(171) + "=" + Convert.ToString(PB_Spacing[1]) + '>');
				stringBuilder.Append(Convert.ToString(172) + "=" + Convert.ToString(PB_ShowScreenBreaks) + '>');
				stringBuilder.Append(Convert.ToString(173) + "=" + Convert.ToString(PB_OneSongPerPage) + '>');
				stringBuilder.Append(Convert.ToString(174) + "=" + Convert.ToString(PB_CJKGroupStyle) + '>');
				stringBuilder.Append(Convert.ToString(180) + "=" + Convert.ToString(PB_ShowNotations + PB_ShowTiming * 2 + PB_ShowKey * 4 + PB_ShowCapo * 8 + PB_CapoZero * 16) + '>');
			}
			xtw.WriteStartElement("ListHeader");
			xtw.WriteElementString("SystemID", SystemID);
			xtw.WriteElementString("FormatData", stringBuilder.ToString());
			xtw.WriteElementString("Notes", InNotes);
			xtw.WriteEndElement();
		}

		public static void ReMapKeyBoard(ref Keys InKey)
		{
			if (InKey == Keys.Home)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Left;
				}
			}
			else if (InKey == Keys.Prior)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Up;
				}
			}
			else if (InKey == Keys.Next)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Down;
				}
			}
			else if (InKey == Keys.End)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Right;
				}
			}
			else if (InKey == Keys.Left)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Home;
				}
			}
			else if (InKey == Keys.Up)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Prior;
				}
			}
			else if (InKey == Keys.Down)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Next;
				}
			}
			else if (InKey == Keys.Right && KeyBoardOption == 1)
			{
				InKey = Keys.End;
			}
		}

		public static int ImplementSlideMovement(ref int InCurSlide, int InCurMaxSlide, Keys InKey, int InSlideNo)
		{
			switch (InKey)
			{
				case Keys.Left:
					InCurSlide = 1;
					break;
				case Keys.Up:
					InCurSlide = ((InCurSlide <= 2) ? 1 : (InCurSlide - 1));
					break;
				case Keys.Down:
					InCurSlide = ((InCurSlide < InCurMaxSlide) ? (InCurSlide + 1) : InCurMaxSlide);
					break;
				case Keys.Right:
					InCurSlide = InCurMaxSlide;
					break;
				case Keys.None:
					InCurSlide = InSlideNo;
					break;
			}
			return InCurSlide;
		}

		public static Keys ReMapKeyDirectionToPowerpoint(KeyDirection InDirection)
		{
			switch (InDirection)
			{
				case KeyDirection.FirstOne:
					return Keys.Left;
				case KeyDirection.PrevOne:
					return Keys.Up;
				case KeyDirection.NextOne:
					return Keys.Down;
				case KeyDirection.LastOne:
					return Keys.Right;
                case KeyDirection.SpaceOne:
                    return Keys.Space;
                default:
					return Keys.F5;
			}
		}

		public static bool MoveToXMLItemElement(ref XmlTextReader reader)
		{
			while (reader.Read())
			{
				if ((reader.NodeType == XmlNodeType.Element) & (reader.Name == "Item"))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ExtractEasiSlidesXMLItem(ref XmlTextReader reader, ref SongSettings InItem)
		{
			string text = "";
			string text2 = "";
			bool flag = false;
			InItem.Type = "I";
			if (MoveToXMLItemElement(ref reader))
			{
				while (reader.Read())
				{
					text2 = "";
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							text = reader.Name;
							text2 = reader.ReadElementContentAsObject().ToString();
							AssignElementToItem(ref InItem, text, text2);
							flag = true;
							break;
						case XmlNodeType.EndElement:
							text = reader.Name;
							if (reader.Name == "Item")
							{
								InItem.In_LicAdminInfo1 = InItem.Show_LicAdminInfo1;
								InItem.In_LicAdminInfo2 = InItem.Show_LicAdminInfo2;
								LoadLicAdminDisplayInfo(ref InItem.Show_LicAdminInfo1, ref InItem.Show_LicAdminInfo2);
								return flag ? true : false;
							}
							break;
					}
				}
			}
			return false;
		}

		public static void GetTitle2AndFormatFromInfoFile(string InFileName, ref string Title2, ref string FormatString)
		{
			InitialiseIndividualData(ref TempItem1);
			LoadInfoFile(InFileName, ref TempItem1, ref tempHeaderData);
			Title2 = TempItem1.Title2;
			FormatString = TempItem1.Format.FormatString;
		}

		public static void AssignElementToItem(ref SongSettings InItem, string ElementName, string ElementValue)
		{
			switch (ElementName)
			{
				case "Title1":
					InItem.Title = ElementValue;
					break;
				case "Title2":
					InItem.Title2 = ElementValue;
					break;
				case "Folder":
					InItem.FolderName = ElementValue;
					InItem.FolderNo = GetFolderNumber(ElementValue);
					break;
				case "SongNumber":
					InItem.SongNumber = DataUtil.StringToInt(ElementValue);
					break;
				case "Contents":
					InItem.CompleteLyrics = ElementValue;
					break;
				case "Notations":
					InItem.Notations = ElementValue;
					break;
				case "Sequence":
					InItem.SongSequence = ConvertTextStringToSequence(ElementValue);
					InItem.SongOriginalLoadedSequence = InItem.SongSequence;
					break;
				case "Writer":
					InItem.Writer = ElementValue;
					break;
				case "Copyright":
					InItem.Copyright = ElementValue;
					break;
				case "Category":
					InItem.Category = ElementValue;
					break;
				case "Timing":
					InItem.Timing = ElementValue;
					break;
				case "MusicKey":
					InItem.MusicKey = ElementValue;
					break;
				case "Capo":
					InItem.Capo = DataUtil.StringToInt(ElementValue, Minus1IfBlank: true);
					break;
				case "LicenceAdmin1":
					InItem.Show_LicAdminInfo1 = ElementValue;
					break;
				case "LicenceAdmin2":
					InItem.Show_LicAdminInfo2 = ElementValue;
					break;
				case "BookReference":
					InItem.Book_Reference = ElementValue;
					break;
				case "UserReference":
					InItem.User_Reference = ElementValue;
					break;
				case "FormatData":
					InItem.Format.FormatString = ElementValue;
					break;
				case "Settings":
					InItem.Settings = ElementValue;
					InItem.RotateString = ExtractSettings(ElementValue, SettingsCategory.RotateString);
					GetRotationStyle(ref InItem);
					break;
				case "Image":
					InItem.Format.ImageString = ElementValue;
					break;
			}
		}

		public static string ExtractSettings(string InSettingsString, SettingsCategory settingsCategory)
		{
			if (InSettingsString == "")
			{
				return "";
			}
			string text = "";
			string text2 = "";
			int num = -1;
			string[] array = InSettingsString.Split('>');
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				text2 = DataUtil.ExtractOneInfo(ref array[i], '=', RemoveExtract: true, MinusOneIfBlank: false);
				if (text2 != "")
				{
					num = DataUtil.StringToInt(text2);
					if (num == (int)settingsCategory)
					{
						return array[i];
					}
				}
			}
			return "";
		}

		public static string CombineSettings(SongSettings InItem)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Convert.ToString(10) + "=" + InItem.RotateString + '>');
			return stringBuilder.ToString();
		}

		public static void StartElement(object strURI, string strName, string strName_3, object attributes)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public static string SetPowerpointPreviewPrefix(SongSettings InItem)
		{
			if (InItem.Type != "P")
			{
				return "";
			}
			if (InItem.OutputStyleScreen)
			{
				if ((OUTPPSequence < 0) | (OUTPPSequence >= 49))
				{
					OUTPPSequence = 0;
				}
				OUTPPSequence++;
				OUTPPFullPath = OUTPPPrefix + OUTPPSequence;
				return OUTPPFullPath;
			}
			if ((PREPPSequence < 0) | (PREPPSequence >= 49))
			{
				PREPPSequence = 0;
			}
			PREPPSequence++;
			PREPPFullPath = PREPPPrefix + PREPPSequence;
			return PREPPFullPath;
		}

		public static string SetPowerpointPreviewPrefix1(SongSettings InItem)
		{
			if (InItem.Type != "P")
			{
				return "";
			}
			Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
			string prefixItemName = regex.Replace(InItem.Title, "");

			if (InItem.OutputStyleScreen)
			{
				OUTPPFullPath = OUTPPPrefix + "$" + prefixItemName + "$";
				return OUTPPFullPath;
			}
			PREPPFullPath = PREPPPrefix + "$" + prefixItemName + "$";
			return PREPPFullPath;
		}

		public static void MinimizePowerPointWindows(ref PowerPoint InPPT)
		{
			InPPT.ResSetAllShowWindows();
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide)
		{
			return RunPowerpointSong(ref InItem, ref InPPT, StartingSlide, ShowResult: false);
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide, bool ShowResult)
		{
			for (int i = 1; i <= 1000; i++)
			{
				InItem.Slide[i, 0] = -1;
			}
			for (int i = 0; i <= 9; i++)
			{
				InItem.SongVerses[i] = 0;
			}
			InPPT.displayName = OutputMonitorName;

			string text = InPPT.Run(InItem.Path, ref PowerpointList, ref TotalPowerpointItems);
			if (StartingSlide < 2)
			{
				InPPT.First();
				InItem.CurSlide = 1;
			}
			else if (StartingSlide > InPPT.Count())
			{
				InPPT.Last();
				InItem.CurSlide = InPPT.Count();
			}
			else if (InPPT.Count() > 0)
			{
				InPPT.GotoSlide(StartingSlide);
				InItem.CurSlide = StartingSlide;
			}

			if (!ShowLiveCam && gf.DualMonitorSelectAutoOption == 1)
            {
				float scalef = 0.75f;
                // ?占쎌썙?占쎌씤???占쎌씪???占쎌떆?占쎈뒗 紐⑤땲?占쏙옙? ?占쎌젙?占쎄린 ?占쏀빐 ?占쎌젙

                if (DualMonitorMode)
                {
                    InPPT.SetShowWindow((float)LS_Left * scalef, (float)LS_Top * scalef, (float)LS_Width * scalef, (float)LS_Height * scalef);
                }
                else
                {
                    InPPT.SetShowWindow(LS_Left, LS_Top, (float)LS_Width * scalef, (float)LS_Height * scalef);
                }
            }

            InPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
			return InPPT.Count();
		}

		public static string GetOfficeDocContents(string InFileName)
		{
			try
			{
				WordDoc wordDoc = new WordDoc();
				return wordDoc.GetContents(InFileName).Replace("\v", "\n");
			}
			catch (Exception)
			{
			}
			return "";
		}

		public static bool SupportedOpenDocFormat(string InFileName)
		{
			//string a = DataUtil.Right(InFileName, 4).ToLower();
			string strExt = Path.GetExtension(InFileName).ToLower();
			if ((strExt == ".doc") | (strExt == ".docx") | (strExt == ".txt"))
			{
				return true;
			}
			return false;
		}

		public static void OldSwitchChineseLyricsNotationListView(ref ListView InLyricsAndNotationsList, int ChangeTo)
		{
			if (InLyricsAndNotationsList.Items.Count > 0)
			{
				for (int i = 0; i <= InLyricsAndNotationsList.Items.Count - 1; i++)
				{
					string InString = InLyricsAndNotationsList.Items[i].SubItems[2].Text;
					SwitchChinese(ref InString, ChangeTo);
					InLyricsAndNotationsList.Items[i].SubItems[2].Text = InString;
				}
			}
		}

		public static void SwitchChineseLyricsNotationListView(ref SongSettings InItem, int ChangeTo)
		{
			if (InItem.LyricsAndNotationsList.Items.Count > 0)
			{
				for (int i = 0; i <= InItem.LyricsAndNotationsList.Items.Count - 1; i++)
				{
					string InString = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text;
					SwitchChinese(ref InString, ChangeTo);
					InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text = InString;
					SwitchChinese(ref InItem.Title, ChangeTo);
					SwitchChinese(ref InItem.Title2, ChangeTo);
					SwitchChinese(ref InItem.Copyright, ChangeTo);
				}
			}
		}

		public static int SwitchChinese(ref RichTextBox InTextBox)
		{
			string InString = InTextBox.Text;
			int num = SwitchChinese(ref InString);
			string[] array = InString.Split("\n"[0]);
			int num2 = 0;
			int num3 = -1;
			int num4 = 0;
			string text = "";
			num2 = 0;
			SendMessage(InTextBox.Handle, 11u, 0u, 0u);
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				num3 = InString.IndexOf("\n"[0], num2);
				SwitchChinese(ref array[i], num);
				text = array[i];
				num4 = num3 - num2;
				if (num3 >= 0)
				{
					InTextBox.SelectionStart = num2;
					InTextBox.SelectionLength = num4;
					InTextBox.SelectedText = text;
					num2 = num3 + 1;
				}
				else if (num2 <= InString.Length)
				{
					InTextBox.SelectionStart = num2;
					InTextBox.SelectionLength = InString.Length - num2;
					InTextBox.SelectedText = text;
				}
			}
			SendMessage(InTextBox.Handle, 11u, 1u, 0u);
			return num;
		}

		public static int SwitchChinese(ref string InString)
		{
			return SwitchChinese(ref InString, -1);
		}

		public static int SwitchChinese(ref string InString, int SelectedType)
		{
			try
			{
				string text = "\uE000#\uE001";
				char c = '\uE002';
				string str = InString;
				//str = Strings.StrConv(str, VbStrConv.SimplifiedChinese, CultureInfo.CurrentCulture.LCID);
				
				str = getStrConv(str, "utf-8");

				switch (SelectedType)
				{
					case 1:
						if (!(str == InString))
						{
							InString = str;
						}
						return 1;
					case 0:
						if (str == InString)
						{
							str = str.Replace(c.ToString(), text);
							str = getStrConv(str, "utf-8");
							//str = Strings.StrConv(str, VbStrConv.TraditionalChinese, CultureInfo.CurrentCulture.LCID);
							str = (InString = str.Replace(text, c.ToString()));
						}
						return 0;
					default:
						if (str == InString)
						{
							str = str.Replace(c.ToString(), text);
							str = getStrConv(str, "utf-8");
							//str = Strings.StrConv(str, VbStrConv.TraditionalChinese, CultureInfo.CurrentCulture.LCID);
							str = (InString = str.Replace(text, c.ToString()));
							return 0;
						}
						InString = str;
						return 1;
				}
            }
            catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			return 1;
		}

		static string getStrConv(string sDat, string codepage)
		{
			try
			{
				//x-cp20936 以묎뎅 codepage
				System.Text.Encoding myEncoding = Encoding.GetEncoding(codepage);

				byte[] buf = myEncoding.GetBytes(sDat);

				return myEncoding.GetString(buf);
			}
			catch (Exception e)
            {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			return sDat; 
		}

		public static void ClearUpPowerpointWindows()
		{
			string text = LivePP.ClearUpPowerpointWindows(ref PowerpointList, ref TotalPowerpointItems);
			if( text == "")
            {
				LivePP.QuitPowerPointApp(LivePP.prePowerPointApp);
			}
		}

        public static void SetDefaultBackScreen(ref ImageTransitionControl InScreen)
		{
			try
			{
                switch (Buffer_LS_Width)
                {
                    case <= 0 when Buffer_LS_Height == 768:
                        Buffer_LS_Width = 1024;
                        break;
                    case <= 0 when Buffer_LS_Height == 800:
                        Buffer_LS_Width = 1280;
                        break;
                    case <= 0 when Buffer_LS_Height == 1024:
                        Buffer_LS_Width = 1280;
                        break;
                    case <= 0 when Buffer_LS_Height == 900:
                        Buffer_LS_Width = 1440;
                        break;
                    case <= 0 when Buffer_LS_Height == 1050:
                        Buffer_LS_Width = 1680;
                        break;
                    case <= 0 when Buffer_LS_Height == 1200:
                        Buffer_LS_Width = 1600;
                        break;
                    case <= 0 when Buffer_LS_Height == 1536:
                        Buffer_LS_Width = 2048;
                        break;
                    case <= 0 when Buffer_LS_Height == 1920:
                        Buffer_LS_Width = 1080;
                        break;
                    case <= 0 when Buffer_LS_Height == 2560:
                        Buffer_LS_Width = 1440;
                        break;
                    case <= 0 when Buffer_LS_Height == 3840:
                        Buffer_LS_Width = 2160;
                        break;
                    case <= 0 when Buffer_LS_Height == 7680:
                        Buffer_LS_Width = 4320;
                        break;
                }

                Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				Graphics g = Graphics.FromImage(image);
				BackPattern.Fill(ref g, ShowScreenColour[0], ShowScreenColour[1], ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref DefaultBackgroundID);
				InScreen.SetDefaultBackgroundPicture(image);
				//g.Dispose();
				//image.Dispose();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				Console.WriteLine(e.Message);
			}
		}

#if OleDb
		public static bool ClearAllFormatting()
		{
			if (MessageBox.Show("This will clear all individual formatting held in the Lyrics Database. Click Yes to proceed.", "Compact EasiSlides Databases", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					string fullSearchString = "select * from SONG where FORMATDATA <> ''";
					using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
					{
						OleDbDataAdapter da = null;
						DataSet ds = null;
						DataTable dt = null;
						(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
						dt = ds.Tables[0];
						if (dt.Rows.Count > 0)
						{
							//recordset.MoveFirst();
							//while (!recordset.EOF)
							foreach (DataRow dr in dt.Rows)
							{
								//recordset.Edit();
								dr["FORMATDATA"] = "";
								//recordset.MoveNext();
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
			}
			return false;
		}

#elif SQLite
		public static bool ClearAllFormatting()
		{
			if (MessageBox.Show("This will clear all individual formatting held in the Lyrics Database. Click Yes to proceed.", "Compact EasiSlides Databases", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					string fullSearchString = "select * from SONG where FORMATDATA <> ''";

					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);

					DbDataAdapter sQLiteDataAdapter = null;

					DataTable dataTable = null;
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					if (dataTable.Rows.Count > 0)
					{
						foreach (DataRow dr in dataTable.Rows)
						{
							dr["FORMATDATA"] = "";
						}
						DbController.UpdateTable(connection, fullSearchString, dataTable);
					}

					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
			return false;
		}
#endif

		public static bool ClearRegistrySettings()
		{
			if (MessageBox.Show("Warning: This will clear all EasiSlides Registry Settings and EasiSlides will then Close. Please note that the next time you restart EasiSlides, the EasiSlides Working Folder will be set to C:\\EasiSlides. Click Yes if you wish to proceed.", "Clear EasiSlides Registry Settings", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software", writable: true);
					registryKey.DeleteSubKeyTree("EasiSlides");
					registryKey.Close();
					return true;
				}
				catch
				{
				}
			}
			return false;
		}

#if DAO
		public static void SaveFormatStringToDatabase(string SongID, string FormatString)
		{
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				tableRecordSet.Seek("=", SongID, def, def, def, def, def, def, def, def, def, def, def, def);
				if (!tableRecordSet.NoMatch)
				{
					tableRecordSet.Edit();
					tableRecordSet.Fields["FormatData"].Value = FormatString;
					tableRecordSet.Update();
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
		}

#elif SQLite
		public static void SaveFormatStringToDatabase(string SongID, string FormatString)
		{
            int intSongID = 0;
            bool result = int.TryParse(SongID, out intSongID);

			if (!result) return;
           
			string sQuery = "select * from SONG";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

				DataRow dr = dataTable.Rows.Find($"{SongID}");
				if (dr != null)
				{
					dr["FormatData"] = FormatString;
				}
				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch
			{
			}
		}
#endif

		public static string BuildItemSearchString(string InString)
		{
			return BuildItemSearchString(InString, SearchTitle: true, SearchContents: true, SearchSongNumber: true, SearchBookRef: true, SearchUserRef: true, SearchLicAdmin: true, SearchWriter: true, SearchCopyright: true, SearchNotationsOnly: false, "", "", SearchDates: false, DateTime.Now, DateTime.Now, null);
		}

#if DAO
		public static string BuildItemSearchString(string InString, bool SearchTitle, bool SearchContents, bool SearchSongNumber, bool SearchBookRef, bool SearchUserRef, bool SearchLicAdmin, bool SearchWriter, bool SearchCopyright, bool SearchNotationsOnly, string SearchSongKey, string SearchSongTiming, bool SearchDates, DateTime DateFrom, DateTime DateTo, CheckedListBox InFolderList)
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = "";
			string text6 = "";
			string text7 = "";
			string text8 = "";
			string text9 = "";
			string text10 = "";
			string text11 = "";
			string text12 = "";
			string text13 = "";
			string text14 = "";
			int num = 0;
			string text15 = "*";
			bool flag = false;
			for (int i = 0; i <= InString.Length; i++)
			{
				text = ((!(DataUtil.Mid(InString, i, 1) != "*")) ? (text + text15) : (text + DataUtil.Mid(InString, i, 1)));
			}
			if (DataUtil.Left(text, 1) != text15)
			{
				text = text15 + text;
			}
			if (DataUtil.Right(text, 1) != text15)
			{
				text += text15;
			}
			for (int i = 1; i < 41; i++)
			{
				FindSongsFolder[i] = false;
			}
			if (InFolderList != null)
			{
				for (int i = 0; i <= InFolderList.CheckedItems.Count - 1; i++)
				{
					FindSongsFolder[GetFolderNumber(InFolderList.CheckedItems[i].ToString())] = true;
					text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())) : (" and (FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())));
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			else
			{
				for (int i = 1; i < 41; i++)
				{
					if (FolderUse[i] > 0)
					{
						FindSongsFolder[i] = true;
						text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + i) : (" and (FolderNo=" + i));
					}
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			if (SearchTitle)
			{
				text3 = " (LCase(Title_1) like \"" + text.ToLower() + "\" " + text2 + ") or (LCase(Title_2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchContents)
			{
				text7 = (flag ? " OR " : "") + " (LCase(lyrics) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			int num2 = DataUtil.StringToInt(text, Minus1IfBlank: true);
			if (SearchSongNumber)
			{
				if (num2 == 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number < 1 or song_number = NULL ) " + text2 + ")";
					flag = true;
				}
				else if (num2 < 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number >= " + num2 + "  and song_number <= " + num2 + ") " + text2 + ")";
					flag = true;
				}
			}
			if (SearchBookRef)
			{
				text5 = (flag ? " OR " : "") + " (LCase(BOOK_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchUserRef)
			{
				text6 = (flag ? " OR " : "") + " (LCase(USER_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchLicAdmin)
			{
				text10 = (flag ? " OR " : "") + " (LCase(licence_admin1) like \"" + text.ToLower() + "\" " + text2 + ") or (LCase(licence_admin2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchWriter)
			{
				text8 = (flag ? " OR " : "") + " (LCase(writer) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchCopyright)
			{
				text9 = (flag ? " OR " : "") + " (LCase(copyright) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchNotationsOnly)
			{
				text11 = (flag ? " AND " : "") + " (msc <> \"\")";
			}
			if (SearchSongKey != "")
			{
				text12 = (flag ? " AND " : "") + " (key = \"" + SearchSongKey + "\")";
			}
			if (SearchSongTiming != "")
			{
				text13 = (flag ? " AND " : "") + " (timing = \"" + SearchSongTiming + "\")";
			}
			if (SearchDates)
			{
				text14 = (flag ? " AND " : "") + " LastModified >=#" + DateFrom.ToString("MM-dd-yyyy") + "# and LastModified <=#" + DateTo.ToString("MM-dd-yyyy") + "# ";
			}
			string text16 = text3 + text7 + text4 + text5 + text6 + text10 + text8 + text9;
			if (text16 != "")
			{
				text16 = "(" + text16 + ")";
			}
			if ((text16 == "") & !FindItemNotationsOnly)
			{
				text16 = " title_1 = \"@!@~!~\"";
			}
			return "select * from SONG where " + text16 + text12 + text13 + text11 + text14;
		}
#elif SQLite
		public static string BuildItemSearchString(string InString, bool SearchTitle, bool SearchContents, bool SearchSongNumber, bool SearchBookRef, bool SearchUserRef, bool SearchLicAdmin, bool SearchWriter, bool SearchCopyright, bool SearchNotationsOnly, string SearchSongKey, string SearchSongTiming, bool SearchDates, DateTime DateFrom, DateTime DateTo, CheckedListBox InFolderList)
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = "";
			string text6 = "";
			string text7 = "";
			string text8 = "";
			string text9 = "";
			string text10 = "";
			string text11 = "";
			string text12 = "";
			string text13 = "";
			string text14 = "";
			int num = 0;
			string text15 = "%";
			bool flag = false;
			for (int i = 0; i <= InString.Length; i++)
			{
				text = ((!(DataUtil.Mid(InString, i, 1) != "%")) ? (text + text15) : (text + DataUtil.Mid(InString, i, 1)));
			}
			if (DataUtil.Left(text, 1) != text15)
			{
				text = text15 + text;
			}
			if (DataUtil.Right(text, 1) != text15)
			{
				text += text15;
			}
			for (int i = 1; i < 41; i++)
			{
				FindSongsFolder[i] = false;
			}
			if (InFolderList != null)
			{
				for (int i = 0; i <= InFolderList.CheckedItems.Count - 1; i++)
				{
					FindSongsFolder[GetFolderNumber(InFolderList.CheckedItems[i].ToString())] = true;
					text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())) : (" and (FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())));
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			else
			{
				for (int i = 1; i < 41; i++)
				{
					if (FolderUse[i] > 0)
					{
						FindSongsFolder[i] = true;
						text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + i) : (" and (FolderNo=" + i));
					}
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			if (SearchTitle)
			{
				text3 = " (lower(Title_1) like \"" + text.ToLower() + "\" " + text2 + ") or (lower(Title_2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchContents)
			{
				text7 = (flag ? " OR " : "") + " (lower(lyrics) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			int num2 = DataUtil.StringToInt(text, Minus1IfBlank: true);
			if (SearchSongNumber)
			{
				if (num2 == 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number < 1 or song_number = NULL ) " + text2 + ")";
					flag = true;
				}
				else if (num2 < 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number >= " + num2 + "  and song_number <= " + num2 + ") " + text2 + ")";
					flag = true;
				}
			}
			if (SearchBookRef)
			{
				text5 = (flag ? " OR " : "") + " (lower(BOOK_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchUserRef)
			{
				text6 = (flag ? " OR " : "") + " (lower(USER_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchLicAdmin)
			{
				text10 = (flag ? " OR " : "") + " (lower(licence_admin1) like \"" + text.ToLower() + "\" " + text2 + ") or (lower(licence_admin2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchWriter)
			{
				text8 = (flag ? " OR " : "") + " (lower(writer) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchCopyright)
			{
				text9 = (flag ? " OR " : "") + " (lower(copyright) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchNotationsOnly)
			{
				text11 = (flag ? " AND " : "") + " (msc <> \"\")";
			}
			if (SearchSongKey != "")
			{
				text12 = (flag ? " AND " : "") + " (key = \"" + SearchSongKey + "\")";
			}
			if (SearchSongTiming != "")
			{
				text13 = (flag ? " AND " : "") + " (timing = \"" + SearchSongTiming + "\")";
			}
			if (SearchDates)
			{
				text14 = (flag ? " AND " : "") + " LastModified >=#" + DateFrom.ToString("MM-dd-yyyy") + "# and LastModified <=#" + DateTo.ToString("MM-dd-yyyy") + "# ";
			}
			string text16 = text3 + text7 + text4 + text5 + text6 + text10 + text8 + text9;
			if (text16 != "")
			{
				text16 = "(" + text16 + ")";
			}
			if ((text16 == "") & !FindItemNotationsOnly)
			{
				text16 = " title_1 = \"@!@~!~\"";
			}
			return "select * from SONG where " + text16 + text12 + text13 + text11 + text14;
		}
#endif

		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex)
		{
			return BuildBibleSearchString(InSearchPassage, VersionIndex, 0, 2);
		}

#if DAO
		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex, int BookIndex, int MatchSelected)
		{
			string text = "\"*[ -/:-@]";
			string text2 = "[ -/:-@]*\"";
			if (PartialWordSearch(VersionIndex))
			{
				text = "\"*";
				text2 = "*\"";
			}
			if (DataUtil.Trim(InSearchPassage).Length > 0)
			{
				InSearchPassage = DataUtil.Trim(InSearchPassage.ToLower());
				sArray = InSearchPassage.Split(' ');
				string text3 = "";
				string text4 = "";
				string text5 = "";
				string text6 = "";
				string text7 = "";
				text3 = "select * from Bible where book";
				text3 = ((BookIndex >= 1) ? (text3 + "=" + BookIndex) : (text3 + ">0 "));
				switch (MatchSelected)
				{
					case 1:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								if (i > 0)
								{
									text4 += " or ";
									text5 += " or ";
									text6 += " or ";
									text7 += " or ";
								}
								string text8 = text4;
								text4 = text8 + " LCase(bibletext) like " + text + DataUtil.Trim(sArray[i]) + text2;
								text5 = text5 + " LCase(bibletext) like \"" + DataUtil.Trim(sArray[i]) + text2;
								text8 = text6;
								text6 = text8 + " LCase(bibletext) like " + text + DataUtil.Trim(sArray[i]) + "\"";
								text7 = text7 + " LCase(bibletext) like \"" + DataUtil.Trim(sArray[i]) + "\"";
							}
							break;
						}
					case 0:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								if (i > 0)
								{
									text4 += " and ";
									text5 += " and ";
									text6 += " and ";
									text7 += " and ";
								}
								string text8 = text4;
								text4 = text8 + " LCase(bibletext) like " + text + sArray[i] + text2;
								text5 = text5 + " LCase(bibletext) like \"" + sArray[i] + text2;
								text8 = text6;
								text6 = text8 + " LCase(bibletext) like " + text + sArray[i] + "\"";
								text7 = text7 + " LCase(bibletext) like \"" + sArray[i] + "\"";
							}
							break;
						}
					default:
						text4 = " LCase(bibletext) like " + text + DataUtil.Trim(InSearchPassage).ToLower() + text2;
						text5 = " LCase(bibletext) like \"" + DataUtil.Trim(InSearchPassage).ToLower() + text2;
						text6 = " LCase(bibletext) like " + text + DataUtil.Trim(InSearchPassage).ToLower() + "\"";
						text7 = " LCase(bibletext) like \"" + DataUtil.Trim(InSearchPassage).ToLower() + "\"";
						break;
				}
				text4 = DataUtil.Trim(text4);
				text5 = DataUtil.Trim(text5);
				text6 = DataUtil.Trim(text6);
				text7 = DataUtil.Trim(text7);
				if (text4 != "")
				{
					text3 = text3 + " AND ( (" + text4 + ")";
					if (!PartialWordSearch(VersionIndex))
					{
						string text8 = text3;
						text3 = text8 + " OR (" + text5 + ") OR (" + text6 + ") OR (" + text7 + ")";
					}
					return text3 + " ) order by Book, chapter, verse ";
				}
			}
			return "";
		}
#elif SQLite
		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex, int BookIndex, int MatchSelected)
		{
			string text = "'*[ -/:-@]";
			string text2 = "[ -/:-@]*'";
			if (PartialWordSearch(VersionIndex))
			{
				text = "'%";
				text2 = "%'";
			}
			if (DataUtil.Trim(InSearchPassage).Length > 0)
			{
				InSearchPassage = DataUtil.Trim(InSearchPassage.ToLower());
				sArray = InSearchPassage.Split(' ');
				string text3 = "";
				string text4 = "";
				string text5 = "";
				string text6 = "";
				string text7 = "";
				text3 = "select * from Bible where book";
				text3 = ((BookIndex >= 1) ? (text3 + "=" + BookIndex) : (text3 + ">0 "));
				switch (MatchSelected)
				{
					case 1:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								string term = DataUtil.Trim(sArray[i]).Replace("'", "''");
								if (i > 0)
								{
									text4 += " or ";
									text5 += " or ";
									text6 += " or ";
									text7 += " or ";
								}
								string text8 = text4;
								text4 = text8 + " lower(bibletext) like " + text + term + text2;
								text5 = text5 + " lower(bibletext) like '" + term + text2;
								text8 = text6;
								text6 = text8 + " lower(bibletext) like " + text + term + "'";
								text7 = text7 + " lower(bibletext) like '" + term + "'";
							}
							break;
						}
					case 0:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								string term = DataUtil.Trim(sArray[i]).Replace("'", "''");
								if (i > 0)
								{
									text4 += " and ";
									text5 += " and ";
									text6 += " and ";
									text7 += " and ";
								}
								string text8 = text4;
								text4 = text8 + " lower(bibletext) like " + text + term + text2;
								text5 = text5 + " lower(bibletext) like '" + term + text2;
								text8 = text6;
								text6 = text8 + " lower(bibletext) like " + text + term + "'";
								text7 = text7 + " lower(bibletext) like '" + term + "'";
							}
							break;
						}
					default:
						string passage = DataUtil.Trim(InSearchPassage).ToLower().Replace("'", "''");
						text4 = " lower(bibletext) like " + text + passage + text2;
						text5 = " lower(bibletext) like '" + passage + text2;
						text6 = " lower(bibletext) like " + text + passage + "'";
						text7 = " lower(bibletext) like '" + passage + "'";
						break;
				}
				text4 = DataUtil.Trim(text4);
				text5 = DataUtil.Trim(text5);
				text6 = DataUtil.Trim(text6);
				text7 = DataUtil.Trim(text7);
				if (text4 != "")
				{
					text3 = text3 + " AND ( (" + text4 + ")";
					if (!PartialWordSearch(VersionIndex))
					{
						string text8 = text3;
						text3 = text8 + " OR (" + text5 + ") OR (" + text6 + ") OR (" + text7 + ")";
					}
					return text3 + " ) order by Book, chapter, verse ";
				}
			}
			return "";
		}
#endif

		public static bool PartialWordSearch(int VersionIndex)
		{
			if (VersionIndex < 0) 
				return false;

			try
			{
				string connectString = ConnectStringDef + HB_Versions[VersionIndex, 4];
				string fullSearchString = "select * from Bible where book=0 and chapter=0 and verse=20";
#if OleDb
				if (DbOleDbController.GetDataTable(connectString, fullSearchString).Rows.Count > 0)
				{
					return true;
				}
#elif SQLite
				if (DbController.GetDataTable(connectString, fullSearchString).Rows.Count > 0)
				{
					return true;
				}
#endif

			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return false;
		}

#if DAO
		public static bool SearchBiblePassages(int InBibleVersion, ref ComboBox InBookList, string InSelectString, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			int num = 0;
			string text = "";
			string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
			StringBuilder stringBuilder = new StringBuilder();
			Recordset recordSet = DbDaoController.GetRecordSet(connectString, InSelectString);
			if (!(recordSet?.EOF ?? true))
			{
				recordSet.MoveFirst();
				while (!recordSet.EOF && num < 3000)
				{
					num++;
					int dataInt = DataUtil.GetDataInt(recordSet, "book");
					int dataInt2 = DataUtil.GetDataInt(recordSet, "chapter");
					int dataInt3 = DataUtil.GetDataInt(recordSet, "verse");
					text = string.Concat(InBookList.Items[dataInt - 1], " ", dataInt2, ":", dataInt3, " ");
					HB_VersesLocation[num, 0] = InBibleVersion;
					HB_VersesLocation[num, 1] = dataInt;
					HB_VersesLocation[num, 2] = dataInt2;
					HB_VersesLocation[num, 3] = dataInt3;
					HB_VersesLocation[num, 4] = stringBuilder.Length;
					stringBuilder.Append(text + (InShowVerses ? DataUtil.GetDataString(recordSet, "bibletext") : "") + "\n\n");
					HB_VersesLocation[num, 5] = stringBuilder.Length + 1 - HB_VersesLocation[num, 4];
					recordSet.MoveNext();
				}
				HB_VersesLocation[0, 0] = num;
				InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
				if (num >= 3000)
				{
					MessageBox.Show("The number of search results has been limited to " + Convert.ToString(3000) + ".");
				}
				return true;
			}
			return false;
		}

#elif SQLite
		public static bool SearchBiblePassages(int InBibleVersion, ref ComboBox InBookList, string InSelectString, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			int num = 0;
			string text = "";
			string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
			StringBuilder stringBuilder = new StringBuilder();

			DbConnection connection = null;
			DbDataReader dataReader = null;

			(connection, dataReader) = DbController.GetDataReader(connectString, InSelectString);

			//Recordset recordSet = DbDaoController.GetRecordSet(connectString, InSelectString);

			using (connection)
			{
				using (dataReader)
				{
					if (dataReader != null && dataReader.HasRows)
					{
						//recordSet.MoveFirst();
						while (dataReader.Read() && num < 3000)
						{
							num++;
							int dataInt = DataUtil.GetDataInt(dataReader, "book");
							int dataInt2 = DataUtil.GetDataInt(dataReader, "chapter");
							int dataInt3 = DataUtil.GetDataInt(dataReader, "verse");
							text = string.Concat(InBookList.Items[dataInt - 1], " ", dataInt2, ":", dataInt3, " ");
							HB_VersesLocation[num, 0] = InBibleVersion;
							HB_VersesLocation[num, 1] = dataInt;
							HB_VersesLocation[num, 2] = dataInt2;
							HB_VersesLocation[num, 3] = dataInt3;
							HB_VersesLocation[num, 4] = stringBuilder.Length;
							stringBuilder.Append(text + (InShowVerses ? DataUtil.GetDataString(dataReader, "bibletext") : "") + "\n\n");
							HB_VersesLocation[num, 5] = stringBuilder.Length + 1 - HB_VersesLocation[num, 4];
						}
						HB_VersesLocation[0, 0] = num;
						InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
						if (num >= 3000)
						{
							MessageBox.Show("The number of search results has been limited to " + Convert.ToString(3000) + ".");
						}
						return true;
					}
				}
			}
			return false;
		}
#endif

		public static bool FormInUse(string InFormName)
		{
			return (FindWindow(null, InFormName).ToInt32() > 0) ? true : false;
		}

		public static void SetMenuItem(ref ToolStripMenuItem InMenuItem, string ItemName, string DefaultText, string MergedText, bool DisableWhenBlank)
		{
			if (ItemName == "")
			{
				InMenuItem.Text = DefaultText;
				InMenuItem.Enabled = ((!DisableWhenBlank) ? true : false);
				return;
			}
			int num = 20;
			string str = (ItemName.Length <= num) ? "'" : "...'";
			InMenuItem.Text = MergedText + " '" + DataUtil.Left(ItemName, num) + str;
			InMenuItem.Enabled = true;
		}

		public static void Play_Media(string title1, string title2)
		{
			string mediaFileName = GetMediaFileName(title1, title2);
			if (!RunProcess(mediaFileName))
			{
				MessageBox.Show("Sorry, cannot find any media file for '" + title1 + "'" + ((title2 != "") ? (" or '" + title2 + "'") : ""));
			}
		}

		public static bool RunProcess(string InProcessString)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process.Start(psi);

				//.net core?占쎌꽌???占쎈옒?占?媛숈씠 ?占쎈㈃ ?占쎌옉???占쏙옙? ?占쎌쓬(?占쎌썙?占쎌씤?占쏙옙? ?占쎌닔 ?占쎌쓬)
				//Process process = Process.Start(InProcessString);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			return true;
		}

		private static IntPtr FindRecentMediaPlayerWindow(string fileName)
		{
			IntPtr foundWindow = IntPtr.Zero;
			string fileNameLower = Path.GetFileName(fileName).ToLower();

			EnumWindows((hWnd, lParam) =>
			{
				if (!IsWindowVisible(hWnd))
					return true;

				StringBuilder sb = new StringBuilder(256);
				GetWindowText(hWnd, sb, 256);
				string windowTitle = sb.ToString().ToLower();

				// 李??쒕ぉ???뚯씪 ?대쫫???ы븿?섏뼱 ?덈뒗吏 ?뺤씤
				if (!string.IsNullOrEmpty(windowTitle))
				{
					string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameLower);
					// ?뚯씪 ?대쫫 ?뺤옣???놁씠 寃???먮뒗 ?쇰컲?곸씤 誘몃뵒???뚮젅?댁뼱 ?대쫫 寃??
					if (windowTitle.Contains(fileNameWithoutExt) ||
					    windowTitle.Contains("windows media player") ||
					    windowTitle.Contains("media player") ||
					    windowTitle.Contains("vlc media player") ||
					    windowTitle.Contains("vlc") ||
					    windowTitle.Contains("movies & tv") ||
					    windowTitle.Contains("films & tv"))
					{
						foundWindow = hWnd;
						return false; // 李얠븯?쇰㈃ 以묐떒
					}
				}

				return true; // 怨꾩냽 寃??
			}, IntPtr.Zero);

			return foundWindow;
		}

		public static bool RunProcessOnMonitor(string InProcessString, string monitorName)
		{
			try
			{
				// ???紐⑤땲??李얘린
				Screen targetScreen = null;
				foreach (Screen screen in Screen.AllScreens)
				{
					if (screen.DeviceName == monitorName)
					{
						targetScreen = screen;
						break;
					}
				}

				// Primary 紐⑤땲?곕? Secondary濡?蹂寃쏀븯??濡쒖쭅 (DisplayInfo.cs? ?숈씪)
				if (monitorName == "Primary")
				{
					string secondaryName = DisplayInfo.getSecondryDisplayName();
					foreach (Screen screen in Screen.AllScreens)
					{
						if (screen.DeviceName == secondaryName)
						{
							targetScreen = screen;
							break;
						}
					}
				}

				if (targetScreen == null)
				{
					// 紐⑤땲?곕? 李얠? 紐삵븯硫?湲곕낯 ?ъ깮
					return RunProcess(InProcessString);
				}

				// ?꾨줈?몄뒪 ?쒖옉
				ProcessStartInfo psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process process = Process.Start(psi);

				// UseShellExecute = true????process媛 null?????덉쓬
				if (process == null)
				{
					// ?꾨줈?몄뒪 媛앹껜???놁?留?誘몃뵒???뚮젅?댁뼱???ㅽ뻾??
					// 李??쒕ぉ?쇰줈 寃?됲빐??李얠븘????
					Thread.Sleep(1500); // ?뚮젅?댁뼱媛 ?쒖옉???뚭퉴吏 ?湲?
					IntPtr foundHandle = FindRecentMediaPlayerWindow(InProcessString);

					// 紐?李얠쑝硫??щ윭 踰??ъ떆??(理쒕? 5踰? 珥?4珥?
					if (foundHandle == IntPtr.Zero)
					{
						for (int retry = 0; retry < 5 && foundHandle == IntPtr.Zero; retry++)
						{
							Thread.Sleep(800);
							foundHandle = FindRecentMediaPlayerWindow(InProcessString);
						}
					}

					if (foundHandle != IntPtr.Zero)
					{
						// Output Monitor濡??대룞 諛?理쒕???
						Rectangle targetBounds = targetScreen.Bounds;
						SetWindowPos(foundHandle, HWND_TOP,
							targetBounds.X, targetBounds.Y,
							targetBounds.Width, targetBounds.Height,
							SWP_SHOWWINDOW);
						SetForegroundWindow(foundHandle);
						ShowWindow(foundHandle, SW_MAXIMIZE);
						return true;
					}
					else
					{
						Console.WriteLine("RunProcessOnMonitor: process媛 null?닿퀬 李쎈룄 李얠쓣 ???놁뒿?덈떎.");
						return false;
					}
				}

				// ?꾨줈?몄뒪 李쎌씠 ?앹꽦???뚭퉴吏 ?湲?(理쒕? 8珥덈줈 利앷?)
				int maxWait = 80; // 80 * 100ms = 8珥?
				int waitCount = 0;

				while (process.MainWindowHandle == IntPtr.Zero && waitCount < maxWait)
				{
					Thread.Sleep(100);
					process.Refresh();
					waitCount++;
				}

				IntPtr handle = process.MainWindowHandle;

				// 李??몃뱾???살? 紐삵븳 寃쎌슦 李??쒕ぉ?쇰줈 ?ш???
				if (handle == IntPtr.Zero)
				{
					// 異붽? ?湲???李??쒕ぉ?쇰줈 寃???쒕룄
					Thread.Sleep(1000);
					handle = FindRecentMediaPlayerWindow(InProcessString);

					// 洹몃옒??紐?李얠쑝硫??щ윭 踰??ъ떆??
					if (handle == IntPtr.Zero)
					{
						for (int retry = 0; retry < 5 && handle == IntPtr.Zero; retry++)
						{
							Thread.Sleep(500);
							handle = FindRecentMediaPlayerWindow(InProcessString);
						}
					}

					// 理쒖쥌?곸쑝濡쒕룄 李쎌쓣 李얠? 紐삵븳 寃쎌슦
					if (handle == IntPtr.Zero)
					{
						Console.WriteLine("RunProcessOnMonitor: 李??몃뱾??李얠쓣 ???놁뒿?덈떎. ?꾩튂 ?쒖뼱 遺덇?");
						return true; // ?꾨줈?몄뒪???ㅽ뻾??
					}
				}

				// 李쎌쓣 ???紐⑤땲?곕줈 ?대룞
				Rectangle bounds = targetScreen.Bounds;

				// 李쎌씠 ?꾩쟾??濡쒕뱶???뚭퉴吏 ?좎떆 ?湲?
				Thread.Sleep(300);

				// 李??대룞 諛??ш린 議곗젙
				SetWindowPos(handle, HWND_TOP,
					bounds.X, bounds.Y,
					bounds.Width, bounds.Height,
					SWP_SHOWWINDOW);

				// 李쎌쓣 ?ш렇?쇱슫?쒕줈 媛?몄삤湲?
				SetForegroundWindow(handle);

				// ?좎떆 ?湲???理쒕???
				Thread.Sleep(200);
				ShowWindow(handle, SW_MAXIMIZE);

				// 理쒖쥌 ?뺤씤: 李쎌씠 ?쒕?濡??대룞?덈뒗吏 ?뺤씤?섍퀬 ?ъ떆??
				Thread.Sleep(300);
				RECT windowRect;
				if (GetWindowRect(handle, out windowRect))
				{
					// 李쎌씠 紐⑺몴 紐⑤땲???곸뿭???놁쑝硫??ㅼ떆 ?대룞 ?쒕룄
					if (windowRect.Left < bounds.Left - 100 ||
					    windowRect.Top < bounds.Top - 100 ||
					    windowRect.Left > bounds.Right + 100)
					{
						Console.WriteLine($"RunProcessOnMonitor: 李??꾩튂 ?ъ“??(?꾩옱: {windowRect.Left},{windowRect.Top} -> 紐⑺몴: {bounds.X},{bounds.Y})");
						SetWindowPos(handle, HWND_TOP,
							bounds.X, bounds.Y,
							bounds.Width, bounds.Height,
							SWP_SHOWWINDOW);
						Thread.Sleep(100);
						ShowWindow(handle, SW_MAXIMIZE);
					}
				}

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("RunProcessOnMonitor failed: " + e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
		}

		public static string LookupDBTitle2(int InKey)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + Convert.ToString(InKey);
#if OleDb
				DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
				{
					return DataUtil.ObjToString(datatable.Rows[0]["Title_2"]);
				}
			}
			catch
			{
			}
			return "";
		}

		public static Font GetNewFont(string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline)
		{
			return GetNewFont(InFontName, InFontSize, InBold, InItalic, InUnderline, ShowErrorMsg: true);
		}

		public static Font GetNewFont(string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline, bool ShowErrorMsg)
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (InBold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (InItalic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (InUnderline)
			{
				fontStyle |= FontStyle.Underline;
			}
			try
			{
				return new Font(InFontName, InFontSize, fontStyle);
			}
			catch
			{
				if (ShowErrorMsg)
				{
				}
				return new Font("Microsoft Sans Serif", InFontSize, fontStyle);
			}
		}

		public static string TransposeChord(string InNotation, int TransposeStep, int FlatSharpKey)
		{
			if (InNotation == "")
			{
				return "";
			}
			if (TransposeStep == 0)
			{
				return InNotation;
			}
			string[] array = InNotation.Split('/');
			string str = TransposeOneChord(array[0], TransposeStep, accidentals: false, FlatSharpKey);
			string str2 = "";
			if (array.GetUpperBound(0) > 0)
			{
				str2 = "/" + TransposeOneChord(array[1], TransposeStep, accidentals: true, FlatSharpKey);
			}
			return str + str2;
		}

		public static string TransposeOneChord(string InChord, int TransposeStep, bool accidentals, int FlatSharpKey)
		{
			int start = 0;
			bool flag = false;
			int num = -1;
			int num2 = 0;
			string text = DataUtil.Left(InChord, 1);
			if (text == "[" || text == "{")
			{
				InChord = DataUtil.Mid(InChord, 1);
			}
			else
			{
				text = "";
			}
			string text2 = DataUtil.Left(InChord, 2);
			while (text2.Length > 0 && !flag)
			{
				start = text2.Length;
				for (int i = 0; i <= 11; i++)
				{
					if ((MusicMajorChords[i, 0] == text2) | (MusicMajorChords[i, 1] == text2))
					{
						flag = true;
						num = i;
						num2 = 0;
						i = 12;
					}
				}
				if (!flag)
				{
					for (int i = 0; i <= 11; i++)
					{
						if ((MusicMinorChords[i, 0] == text2) | (MusicMinorChords[i, 1] == text2))
						{
							flag = true;
							num = i;
							num2 = 1;
							i = 12;
						}
					}
				}
				if (!flag)
				{
					text2 = DataUtil.Left(text2, text2.Length - 1);
				}
			}
			if (flag)
			{
				string text3 = "";
				if (FlatSharpKey < 0 || FlatSharpKey > 1)
				{
					FlatSharpKey = ((TransposeStep > 0) ? 1 : 0);
				}
				int num3 = num + TransposeStep;
				if (num3 < 0)
				{
					num3 = 11 + num3 + 1;
				}
				if (num3 > 11)
				{
					num3 = num3 - 11 - 1;
				}
				text3 = ((num2 != 0) ? MusicMinorChords[num3, (FlatSharpKey > 0) ? 1 : 0] : MusicMajorChords[num3, (FlatSharpKey > 0) ? 1 : 0]);
				return text + text3 + DataUtil.Mid(InChord, start);
			}
			return text + InChord;
		}

		public static int TransposeKey(ref string InKey, int TransposeStep)
		{
			if (InKey == "")
			{
				return (TransposeStep > 0) ? 1 : 0;
			}
			bool flag = false;
			int num = -1;
			int num2 = 0;
			for (int i = 0; i <= 11; i++)
			{
				if (MusicMajorKeys[i] == InKey)
				{
					flag = true;
					num = i;
					num2 = 0;
					i = 12;
				}
			}
			if (!flag)
			{
				for (int i = 0; i <= 11; i++)
				{
					if (MusicMinorKeys[i] == InKey)
					{
						flag = true;
						num = i;
						num2 = 1;
						i = 12;
					}
				}
			}
			if (flag)
			{
				string text = "";
				int num3 = -1;
				int num4 = num + TransposeStep;
				if (num4 < 0)
				{
					num4 = 11 + num4 + 1;
				}
				if (num4 > 11)
				{
					num4 = num4 - 11 - 1;
				}
				if (num2 == 0)
				{
					text = MusicMajorKeys[num4];
					num3 = MusicMajorKeysFlatSharp[num4];
				}
				else
				{
					text = MusicMinorKeys[num4];
					num3 = MusicMinorKeysFlatSharp[num4];
				}
				InKey = text;
				return num3;
			}
			return -1;
		}

		public static bool ValidSongID(int InSongID)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + InSongID;
#if OleDb
				DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
				
				if (datatable.Rows.Count > 0)
				{
					return (FolderUse[DataUtil.ObjToInt(datatable.Rows[0]["FolderNo"])] > 0) ? true : false;
				}
			}
			catch
			{
			}
			return false;
		}

		public static void GetCurPosInLine(string instring, ref int CurPos)
		{
			MoveToPosInLine(instring, ref CurPos, 0);
		}

		public static void MoveToPosInLine(string instring, ref int CurPos, int InMode)
		{
			if (instring.Length == 0)
			{
				return;
			}
			bool flag = false;
			int num = CurPos + ((InMode == 0) ? (-1) : 0);
			string text = "";
			while (num >= 0 && num < instring.Length && !flag)
			{
				text = DataUtil.Mid(instring, num, 1);
				if ((text == "\r") | (text == "\n"))
				{
					flag = true;
					CurPos = num + ((InMode == 0) ? 1 : 0);
					num = 0;
				}
				else
				{
					num += ((InMode != 0) ? 1 : (-1));
				}
			}
			if (!flag)
			{
				CurPos = ((InMode != 0) ? instring.Length : 0);
			}
		}

		public static void InsertChordAboveCurrentLine(ref RichTextBox InTextBox, string InChordString)
		{
			bool flag = (InChordString[0].ToString() == "/") ? true : false;
			int i = InTextBox.SelectionStart;
			int curPos = i;
			int num = i;
			int length = InTextBox.Text.Length;
			Point positionFromCharIndex = InTextBox.GetPositionFromCharIndex(i);
			int lineFromCharIndex = InTextBox.GetLineFromCharIndex(i);
			bool flag2 = false;
			int num2 = InTextBox.Text.IndexOf("쨩", i);
			if (InTextBox.GetLineFromCharIndex(num2) != lineFromCharIndex)
			{
				num2 = -1;
			}
			int num3 = -1;
			if (num2 >= 0)
			{
				num3 = i;
			}
			else
			{
				InChordString += " ";
				if (lineFromCharIndex < 1)
				{
					flag2 = true;
				}
				else
				{
					string textFromPreviousLine = GetTextFromPreviousLine(InTextBox.Text, curPos);
					if (textFromPreviousLine.IndexOf("쨩") < 0)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					InsertIndicator(ref InTextBox, 151);
					InTextBox.SelectionStart -= 1;
					InsertIndicator(ref InTextBox, 152);
					i = InTextBox.SelectionStart;
				}
				else
				{
					GetCurPosInLine(InTextBox.Text, ref i);
					i--;
				}
			}
			int CurPos = -1;
			MoveToPosInLine(InTextBox.Text, ref CurPos, 1);
			GetCurPosInLine(InTextBox.Text, ref i);
			int num4 = i;
			InTextBox.SelectionStart = i;
			num2 = InTextBox.Text.IndexOf("쨩", i);
			if (num2 < 0)
			{
				return;
			}
			if (num3 >= 0)
			{
				i = num3;
			}
			InTextBox.SelectionStart = num2;
			Point positionFromCharIndex2 = InTextBox.GetPositionFromCharIndex(InTextBox.SelectionStart);
			while (positionFromCharIndex2.X < positionFromCharIndex.X)
			{
				InTextBox.SelectedText = " ";
				positionFromCharIndex2 = InTextBox.GetPositionFromCharIndex(InTextBox.SelectionStart);
			}
			GetCurPosInLine(InTextBox.Text, ref i);
			for (; InTextBox.GetPositionFromCharIndex(i).X < positionFromCharIndex.X; i++)
			{
			}
			bool flag3 = false;
			while (i > num4 && i < num2 && !flag3)
			{
				if (InTextBox.Text[i - 1].ToString() == " ")
				{
					flag3 = true;
				}
				else if (InTextBox.Text[i].ToString() == " ")
				{
					i++;
					flag3 = true;
				}
				else
				{
					i++;
				}
			}
			Point point = new Point(-1, -1);
			int num5 = -1;
			for (int j = i; j <= num2; j++)
			{
				if (InTextBox.Text[j].ToString() != " ")
				{
					point = InTextBox.GetPositionFromCharIndex(j);
					num5 = j;
					j = num2 + 1;
				}
			}
			InTextBox.SelectionStart = i;
			InTextBox.SelectedText = InChordString;
			InTextBox.SelectedText = "";
			if (num5 >= 0)
			{
				num5 += InChordString.Length - 1;
				int x = InTextBox.GetPositionFromCharIndex(num5).X;
				while (x >= point.X && InTextBox.Text[num5 - 1].ToString() == " " && InTextBox.Text[num5 - 2].ToString() == " ")
				{
					InTextBox.SelectionStart = num5;
					InTextBox.SelectionLength = 1;
					InTextBox.SelectedText = "";
					num5--;
					x = InTextBox.GetPositionFromCharIndex(num5).X;
				}
			}
			InTextBox.SelectionStart = ((num3 >= 0) ? (num3 + InChordString.Length) : (num + (InTextBox.Text.Length - length)));
			InTextBox.SelectedText = "";
		}

		public static string GetTextFromPreviousLine(string InString, int CurPos)
		{
			GetCurPosInLine(InString, ref CurPos);
			if (CurPos > 0)
			{
				CurPos--;
			}
			int num = CurPos;
			GetCurPosInLine(InString, ref CurPos);
			int num2 = num - CurPos;
			string inString = "";
			if (num2 > 0)
			{
				inString = DataUtil.Mid(InString, CurPos, num2);
			}
			return DataUtil.Trim(inString);
		}

		public static void OldGetCurPosInLine(string instring, ref int CurPos, int InPos)
		{
			if (instring.Length == 0)
			{
				return;
			}
			bool flag = false;
			int num = CurPos + 1 + ((InPos == 0) ? (-1) : 0);
			while ((num > 1) & (num < instring.Length + 1))
			{
				if ((DataUtil.Mid(instring, num, 1) == "\r") | (DataUtil.Mid(instring, num, 1) == "\n"))
				{
					flag = true;
					CurPos = num + ((InPos != 0) ? (-1) : 0);
					num = 1;
				}
				else
				{
					num += ((InPos != 0) ? 1 : (-1));
				}
			}
			if (!flag)
			{
				CurPos = ((InPos != 0) ? instring.Length : 0);
			}
		}

		public static string MakeTitleValidFileName(string InString)
		{
			char[] array = new char[9]
			{
				'\\',
				'/',
				':',
				'*',
				'?',
				'"',
				'<',
				'>',
				'|'
			};
			for (int i = 0; i < array.Length; i++)
			{
				InString = InString.Replace(array[i], '_');
			}
			InString = DataUtil.Left(InString, 255);
			return InString;
		}

		public static bool ValidateTitleDetails(string InString, string Heading)
		{
			char[] anyOf = new char[6]
			{
				'[',
				']',
				'*',
				'"',
				'<',
				'>'
			};
			if (InString.IndexOfAny(anyOf) >= 0)
			{
				MessageBox.Show(Heading + " must not contain the characters: ] [ * \" < >");
				return false;
			}
			if (InString.Length > 100)
			{
				MessageBox.Show(Heading + " is too long (" + Convert.ToString(InString.Length) + "), maximum length is 100 characters including spaces");
				return false;
			}
			return true;
		}

		public static void FormatPlainLyrics(ref RichTextBox InTextBox)
		{
			string text = "";
			string text2 = InTextBox.Text;
			text = text2.Replace("\r\n", "\n");
			text = text.Replace("\r", "");
			for (int num = 99; num > 0; num--)
			{
				text = text.Replace("Verse   " + num, num.ToString());
				text = text.Replace("verse   " + num, num.ToString());
				text = text.Replace("VERSE   " + num, num.ToString());
				text = text.Replace("Verse  " + num, num.ToString());
				text = text.Replace("verse  " + num, num.ToString());
				text = text.Replace("VERSE  " + num, num.ToString());
				text = text.Replace("Verse   " + num, num.ToString());
				text = text.Replace("verse   " + num, num.ToString());
				text = text.Replace("VERSE   " + num, num.ToString());
				text = text.Replace("Verse  " + num, num.ToString());
				text = text.Replace("verse  " + num, num.ToString());
				text = text.Replace("VERSE  " + num, num.ToString());
				text = text.Replace("Verse " + num, num.ToString());
				text = text.Replace("verse " + num, num.ToString());
				text = text.Replace("VERSE " + num, num.ToString());
				text = text.Replace("Verse" + num, num.ToString());
				text = text.Replace("verse" + num, num.ToString());
				text = text.Replace("VERSE" + num, num.ToString());
				text = text.Replace("Ver   " + num, num.ToString());
				text = text.Replace("ver   " + num, num.ToString());
				text = text.Replace("VER   " + num, num.ToString());
				text = text.Replace("Ver  " + num, num.ToString());
				text = text.Replace("ver  " + num, num.ToString());
				text = text.Replace("VER  " + num, num.ToString());
				text = text.Replace("Ver " + num, num.ToString());
				text = text.Replace("ver " + num, num.ToString());
				text = text.Replace("VER " + num, num.ToString());
				text = text.Replace("Ver" + num, num.ToString());
				text = text.Replace("ver" + num, num.ToString());
				text = text.Replace("VER" + num, num.ToString());
				text = text.Replace("V   " + num, num.ToString());
				text = text.Replace("v   " + num, num.ToString());
				text = text.Replace("V  " + num, num.ToString());
				text = text.Replace("v  " + num, num.ToString());
				text = text.Replace("V " + num, num.ToString());
				text = text.Replace("v " + num, num.ToString());
				text = text.Replace("V" + num, num.ToString());
				text = text.Replace("v" + num, num.ToString());
			}
			if (text.IndexOf("1") >= 0)
			{
				if (DataUtil.Left(text, 7) == "1.     ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 8);
				}
				else if (DataUtil.Left(text, 6) == "1.    ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 7);
				}
				else if (DataUtil.Left(text, 5) == "1.   ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 6);
				}
				else if (DataUtil.Left(text, 4) == "1.  ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 5);
				}
				else if (DataUtil.Left(text, 3) == "1. ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 4);
				}
				else if (DataUtil.Left(text, 2) == "1.")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
				else if (DataUtil.Left(text, 7) == "1:     ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 8);
				}
				else if (DataUtil.Left(text, 6) == "1:    ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 7);
				}
				else if (DataUtil.Left(text, 5) == "1:   ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 6);
				}
				else if (DataUtil.Left(text, 4) == "1:  ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 5);
				}
				else if (DataUtil.Left(text, 3) == "1: ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 4);
				}
				else if (DataUtil.Left(text, 2) == "1:")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
				else if (DataUtil.Left(text, 7) == "1      ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 8);
				}
				else if (DataUtil.Left(text, 6) == "1     ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 7);
				}
				else if (DataUtil.Left(text, 5) == "1    ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 6);
				}
				else if (DataUtil.Left(text, 4) == "1   ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 5);
				}
				else if (DataUtil.Left(text, 3) == "1  ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 4);
				}
				else if (DataUtil.Left(text, 2) == "1 ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
				else if (DataUtil.Left(text, 2) == "1\n")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
			}
			for (int num = 99; num > 0; num--)
			{
				if (text.IndexOf(num.ToString()) >= 0)
				{
					text = text.Replace("\n" + num + ".     ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".    ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".   ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".  ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ". ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":     ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":    ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":   ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":  ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ": ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "     ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "    ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "   ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "  ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + " ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + " ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "\n", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "\t", "\n[" + num + "]\n");
					text = text.Replace("\n\n[" + num + "]", "\n[" + num + "]\n");
					text = text.Replace("[" + num + "]\n\n", "[" + num + "]\n");
				}
			}
			string text3 = "ChoruS";
			string text4 = "[chorus]\n";
			text = text.Replace("\nchorus", "\n" + text3);
			text = text.Replace("\nChorus", "\n" + text3);
			text = text.Replace("\nCHORUS", "\n" + text3);
			if (DataUtil.Left(text, 6) == "chorus")
			{
				text = text3 + DataUtil.Mid(text, 6);
			}
			if (DataUtil.Left(text, 6) == "Chorus")
			{
				text = text3 + DataUtil.Mid(text, 6);
			}
			if (DataUtil.Left(text, 6) == "CHORUS")
			{
				text = text3 + DataUtil.Mid(text, 6);
			}
			if (text.IndexOf(text3) >= 0)
			{
				text = text.Replace(text3 + ".     ", text4);
				text = text.Replace(text3 + ".    ", text4);
				text = text.Replace(text3 + ".   ", text4);
				text = text.Replace(text3 + ".  ", text4);
				text = text.Replace(text3 + ". ", text4);
				text = text.Replace(text3 + ".", text4);
				text = text.Replace(text3 + ":     ", text4);
				text = text.Replace(text3 + ":    ", text4);
				text = text.Replace(text3 + ":   ", text4);
				text = text.Replace(text3 + ":  ", text4);
				text = text.Replace(text3 + ": ", text4);
				text = text.Replace(text3 + ":", text4);
				text = text.Replace(text3 + "     ", text4);
				text = text.Replace(text3 + "    ", text4);
				text = text.Replace(text3 + "   ", text4);
				text = text.Replace(text3 + "  ", text4);
				text = text.Replace(text3 + " ", text4);
				text = text.Replace(text3 + "\t", text4);
				text = text.Replace(text3, text4);
				text = text.Replace("\n\n" + text4, "\n" + text4);
				text = text.Replace(text4 + "\n", text4);
			}
			if (text.IndexOf("[2]") >= 0 && text.IndexOf("[1]") < 0)
			{
				text = "[1]\n" + text;
			}
			text = text.Replace("\t\n", "\n");
			text = text.Replace("\t", "");
			text = text.Replace("\n\n\n", "\n\n");
			text = text.Replace("\n\n[", "\n[");
			text = DataUtil.TrimEnd(text);
			InTextBox.Text = text;
		}

		public static void Merge_Songs(SongSettings InItem1, SongSettings InItem2, ref string CombinedLyrics, ref string CombinedNotations)
		{
			FormatDisplayLyrics(ref InItem1, PrepareSlides: false, UseStoredSequence: true);
			FormatDisplayLyrics(ref InItem2, PrepareSlides: false, UseStoredSequence: true);
			int[] array = new int[160];
			ListViewItem listViewItem = new ListViewItem();
			TempItem1.LyricsAndNotationsList.Items.Clear();
			bool flag = false;
			for (int i = 0; i < InItem1.SongSequence.Length; i++)
			{
				int num = InItem1.SongSequence[i];
				if (i == 0 && (InItem1.CompleteLyrics.IndexOf(VerseSymbol[num]) >= 0 || InItem2.CompleteLyrics.IndexOf(VerseSymbol[num]) >= 0))
				{
					flag = true;
				}
				listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[num]);
				listViewItem.SubItems.Add("");
				for (int j = InItem1.VerseLineLoc[num, 1]; (j >= 0) & (j <= InItem1.VerseLineLoc[num, 2]); j++)
				{
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[2].Text);
					listViewItem.SubItems.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[3].Text);
				}
				if (InItem2.VersePresent[num] & (InItem2.CompleteLyrics != ""))
				{
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[150]);
					listViewItem.SubItems.Add("");
					for (int k = InItem2.VerseLineLoc[num, 1]; (k >= 0) & (k <= InItem2.VerseLineLoc[num, 2]); k++)
					{
						listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
						listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
					}
					InItem2.VersePresent[num] = false;
				}
			}
			for (int i = 0; i <= 112; i++)
			{
				if (InItem2.VersePresent[i] & (InItem2.CompleteLyrics != ""))
				{
					int num = i;
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[num]);
					listViewItem.SubItems.Add("");
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[150]);
					listViewItem.SubItems.Add("");
					for (int k = InItem2.VerseLineLoc[num, 1]; (k >= 0) & (k <= InItem2.VerseLineLoc[num, 2]); k++)
					{
						listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
						listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
					}
				}
			}
			if (!flag && TempItem1.LyricsAndNotationsList.Items.Count > 0)
			{
				TempItem1.LyricsAndNotationsList.Items[0].Remove();
			}
			CombinedLyrics = "";
			for (int i = 0; i < TempItem1.LyricsAndNotationsList.Items.Count; i++)
			{
				CombinedLyrics = CombinedLyrics + "\n" + TempItem1.LyricsAndNotationsList.Items[i].SubItems[0].Text;
			}
			CombinedLyrics = DataUtil.Mid(CombinedLyrics, 1);
			CombinedNotations = "";
			for (int i = 0; i < TempItem1.LyricsAndNotationsList.Items.Count; i++)
			{
				if (TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text != "")
				{
					object obj = CombinedNotations;
					CombinedNotations = string.Concat(obj, "(", Convert.ToString(i), ';', TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text, ")");
				}
			}
			CombinedLyrics = DataUtil.TrimEnd(CombinedLyrics);
		}

		public static bool UpdateRefString(string Instring, string InStringDelim, ref TextBox StoredTextBox, string StoredStringDelim)
		{
			string text = "";
			bool flag = false;
			bool flag2 = false;
			string text2 = "";
			string text3 = "";
			string[] array = Instring.Split(InStringDelim[0]);
			string[] array2 = StoredTextBox.Text.Split(StoredStringDelim[0]);
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				text2 = DataUtil.Trim(array[i]).ToLower();
				flag = false;
				if (!(text2 != ""))
				{
					continue;
				}
				for (int j = 0; j <= array2.GetUpperBound(0); j++)
				{
					text3 = DataUtil.Trim(array2[j]).ToLower();
					if (text2 == text3)
					{
						flag = true;
						j = array2.GetUpperBound(0) + 1;
					}
				}
				if (!flag)
				{
					text = text + ((text != "") ? (StoredStringDelim + " ") : "") + DataUtil.Trim(array[i]);
				}
			}
			if (text != "")
			{
				TextBox obj = StoredTextBox;
				obj.Text = obj.Text + ((StoredTextBox.Text != "") ? (StoredStringDelim + " ") : "") + text;
				return true;
			}
			return false;
		}

		public static ListView ExtractLyrics(string InLyrics, string InNotations)
		{
			string OutText = "";
			string OutNotationString = "";
			string OutText2 = "";
			string OutNotationString2 = "";
			return ExtractLyrics(InLyrics, InNotations, ref OutText, ref OutNotationString, ref OutText2, ref OutNotationString2);
		}

		public static ListView ExtractLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			InLyrics = InLyrics.Replace("\r\n", "\n");
			if (IsNewR2Format(InLyrics))
			{
				return ExtractNewFormatLyrics(InLyrics, InNotations, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);
			}
			return ExtractDefaultFormatLyrics(InLyrics, InNotations, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);
		}

		public static ListView ExtractNewFormatLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			string text = "";
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			string ResultString = "";
			string text2 = "";
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			string text3 = "";
			int num4 = -1;
			int num5 = 1;
			bool flag = false;
			string[] array = InLyrics.Split("\n"[0]);
			ListView listView = new ListView();
			SetListViewColumns(listView, 4);
			listView.Items.Clear();
			ListViewItem listViewItem = new ListViewItem();
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				for (int j = 0; j <= xArray.GetUpperBound(0); j++)
				{
					if (array[i] == xArray[j])
					{
						if (array[i] != VerseSymbol[150])
						{
							text3 = array[i];
							num4 = GetVerseNumeric(text3);
							flag = true;
							j = xArray.GetUpperBound(0) + 1;
						}
						else
						{
							num5 = 2;
						}
					}
				}
				if ((num < i) & (InNotations.Length > 0))
				{
					num = ExtractOneNotationsLine(ref InNotations, ref ResultString);
				}
				if (num5 == 1)
				{
					num2++;
					stringBuilder.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num2);
						stringBuilder3.Append(text2);
						text2 = ResultString;
					}
				}
				else if (array[i] == VerseSymbol[150])
				{
					flag = true;
				}
				else
				{
					num3++;
					stringBuilder2.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num3);
						stringBuilder4.Append(text2);
						text2 = ResultString;
					}
				}
				if (!flag)
				{
					if (num4 < 0)
					{
						num4 = 1;
					}
					listViewItem = listView.Items.Add(num4.ToString());
					listViewItem.SubItems.Add(num5.ToString());
					listViewItem.SubItems.Add(array[i]);
					listViewItem.SubItems.Add(text2);
				}
				flag = false;
				text2 = "";
			}
			OutText1 = DataUtil.TrimEnd(stringBuilder.ToString());
			OutText2 = DataUtil.TrimEnd(stringBuilder2.ToString());
			OutNotationString1 = stringBuilder3.ToString();
			OutNotationString2 = stringBuilder4.ToString();
			return listView;
		}

		public static ListView ExtractDefaultFormatLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			string text = "";
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			string ResultString = "";
			string text2 = "";
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			string text3 = "";
			int num4 = -1;
			int num5 = 1;
			bool flag = false;
			string[] array = InLyrics.Split("\n"[0]);
			ListView listView = new ListView();
			SetListViewColumns(listView, 4);
			listView.Items.Clear();
			ListViewItem listViewItem = new ListViewItem();
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				for (int j = 0; j <= xArray.GetUpperBound(0); j++)
				{
					if (array[i] == xArray[j])
					{
						flag = true;
						if (array[i] != VerseSymbol[150])
						{
							num5 = 1;
							text3 = array[i];
							num4 = GetVerseNumeric(text3);
							j = xArray.GetUpperBound(0) + 1;
						}
						else
						{
							num5 = 2;
						}
					}
				}
				if ((num < i) & (InNotations.Length > 0))
				{
					num = ExtractOneNotationsLine(ref InNotations, ref ResultString);
				}
				if (num5 == 1)
				{
					num2++;
					stringBuilder.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num2);
						stringBuilder3.Append(text2);
						text2 = ResultString;
					}
				}
				else if (array[i] == VerseSymbol[150])
				{
					if (text3 != "")
					{
						num3++;
						stringBuilder2.Append(text3 + "\n");
					}
				}
				else
				{
					num3++;
					stringBuilder2.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num3);
						stringBuilder4.Append(text2);
						text2 = ResultString;
					}
				}
				if (!flag)
				{
					if (num4 < 0)
					{
						num4 = 1;
					}
					listViewItem = listView.Items.Add(num4.ToString());
					listViewItem.SubItems.Add(num5.ToString());
					listViewItem.SubItems.Add(array[i]);
					listViewItem.SubItems.Add(text2);
				}
				flag = false;
				text2 = "";
			}
			OutText1 = DataUtil.TrimEnd(stringBuilder.ToString());
			OutText2 = DataUtil.TrimEnd(stringBuilder2.ToString());
			OutNotationString1 = stringBuilder3.ToString();
			OutNotationString2 = stringBuilder4.ToString();
			return listView;
		}

		private static int GetVerseNumeric(string CurVerseSymbol)
		{
			for (int i = 0; i < 160; i++)
			{
				if (CurVerseSymbol == VerseSymbol[i])
				{
					return i;
				}
			}
			return -1;
		}

		public static string AddNotationLineNumber(string InOneNotationLine, int InNewLineNumber)
		{
			if (InOneNotationLine != "")
			{
				return "(" + InNewLineNumber + ';' + InOneNotationLine + ")";
			}
			return "";
		}

		public static string RTFCheck(string InString)
		{
			InString = InString.Replace("{", "\\{");
			return InString.Replace("}", "\\}");
		}

		public static string Html_MusicDisplayName(string title1, string title2, string HTMLIndexDir)
		{
			string DirPath = "";
			string FileName = "";
			if (GetMusicFileName(title1, title2, ref DirPath, ref FileName, StoreDirPath: true) == "")
			{
				return "";
			}
			int num = 0;
			if (DirPath == HTMLIndexDir)
			{
				return FileName;
			}
			for (int i = 0; i < HTMLIndexDir.Length; i++)
			{
				if (DataUtil.Mid(HTMLIndexDir, i, 1) != DataUtil.Mid(DirPath, i, 1))
				{
					num = i - 1;
					i = HTMLIndexDir.Length;
				}
			}
			if (num < 1)
			{
				return DirPath + FileName;
			}
			string str = "";
			for (int i = num + 1; i <= HTMLIndexDir.Length; i++)
			{
				if (DataUtil.Mid(HTMLIndexDir, i, 1) == "\\")
				{
					str += "..\\";
				}
			}
			str += DataUtil.Mid(DirPath, num + 1);
			return str + FileName;
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2)
		{
			string DirPath = "";
			string FileName = "";
			return GetMusicFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath)
		{
			string FileName = "";
			return GetMusicFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName)
		{
			return GetMusicFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName, bool StoreDirPath)
		{
			if (StoreDirPath & (TotalMusicFiles < 1))
			{
				return "";
			}
			string text = "";
			for (int i = 0; i <= 1; i++)
			{
				text = ((i == 0) ? MusicTitle1 : MusicTitle2);
				for (int j = 0; j <= TotalMediaFileExt - 1; j++)
				{
					if (StoreDirPath)
					{
						for (int k = 0; k <= TotalMusicFiles - 1; k++)
						{
							if (MediaFilesList[k, 0] == text && MediaFilesList[k, 1] == MediaFileExtension[j, 0])
							{
								DirPath = MediaFilesList[k, 2];
								FileName = MediaFilesList[k, 0] + MediaFilesList[k, 1];
								return DirPath + FileName;
							}
						}
					}
					else
					{
						string musicFileNameFromDir = GetMusicFileNameFromDir(MediaDir, MediaFileExtension[j, 0], text, ref DirPath, ref FileName);
						if (musicFileNameFromDir != "")
						{
							return musicFileNameFromDir;
						}
					}
				}
			}
			return "";
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1)
		{
			string DirPath = "";
			string FileName = "";
			return GetMusicFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath)
		{
			string FileName = "";
			return GetMusicFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath, ref string FileName)
		{
			if ((FolderPath == "") | !Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 2) == ":\\System Volume Information\\"))
			{
				return "";
			}
			if (File.Exists(FolderPath + MusicTitle1 + MusicExtension))
			{
				DirPath = FolderPath;
				FileName = MusicTitle1 + MusicExtension;
				return DirPath + FileName;
			}
			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			string[] array = directories;
			foreach (string str in array)
			{
				string musicFileNameFromDir = GetMusicFileNameFromDir(str + "\\", MusicExtension, MusicTitle1, ref DirPath, ref FileName);
				if (musicFileNameFromDir != "")
				{
					return musicFileNameFromDir;
				}
			}
			return "";
		}

		public static void AlertSettings(AlertType InAlertType)
		{
			ParentalAlertLive = false;
			MessageAlertLive = false;
			AlertTimeRemaining = 0;
			Alert_FormattedMessage = "";
			Alert_MessageHeight = 0;
			Alert_MessageDisplayed = false;
			switch (InAlertType)
			{
				case AlertType.Parental:
					Alert_OriginalMessage = DataUtil.Trim(ParentalAlertHeading) + " " + ParentalAlertDetails;
					Alert_TextAlign = ParentalAlertTextAlign;
					Alert_VerticalAlign = ParentalAlertVerticalAlign;
					Alert_TextColour = ParentalAlertTextColour;
					Alert_BackColour = ParentalAlertBackColour;
					Alert_Flash = ParentalAlertFlash;
					Alert_Scroll = ParentalAlertScroll;
					Alert_Transparent = ParentalAlertTransparent;
					AlertTimeRemaining = ParentalAlertDuration;
					Alert_UserFont = GetNewFont(ParentalAlertFontName, ParentalAlertFontSize, ParentalAlertBold, ParentalAlertItalic, ParentalAlertUnderline, ShowErrorMsg: false);
					Alert_UserFontShadow = ParentalAlertShadow;
					Alert_UserFontOutline = ParentalAlertOutline;
					BuildAlertSequence();
					Alert_NewMessage = true;
					ParentalAlertLive = true;
					break;
				case AlertType.Message:
					Alert_OriginalMessage = MessageAlertDetails;
					Alert_TextAlign = MessageAlertTextAlign;
					Alert_VerticalAlign = MessageAlertVerticalAlign;
					Alert_TextColour = MessageAlertTextColour;
					Alert_BackColour = MessageAlertBackColour;
					Alert_Flash = MessageAlertFlash;
					Alert_Scroll = MessageAlertScroll;
					Alert_Transparent = MessageAlertTransparent;
					AlertTimeRemaining = MessageAlertDuration;
					Alert_UserFont = GetNewFont(MessageAlertFontName, MessageAlertFontSize, MessageAlertBold, MessageAlertItalic, MessageAlertUnderline, ShowErrorMsg: false);
					Alert_UserFontShadow = MessageAlertShadow;
					Alert_UserFontOutline = MessageAlertOutline;
					BuildAlertSequence();
					Alert_NewMessage = true;
					MessageAlertLive = true;
					break;
			}
		}

		public static void BuildAlertSequence()
		{
			string text = "";
			if (Alert_Flash)
			{
				text = "R" + '>' + "R" + '>' + "R" + '>' + "R" + '>';
			}
			Alert_FormatSequence = text;
			bool flag = Alert_Scroll;
			if ((Alert_OriginalMessage.Length + 30) * AlertGap > AlertTimeRemaining)
			{
				flag = false;
			}
			if (flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "S" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = 0;
			}
			else if (!flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = Alert_OriginalMessage.Length;
			}
			Alert_FormatSequence += text;
			for (int i = 1; i <= 30; i++)
			{
				Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
			}
			if (Alert_Scroll)
			{
				Alert_FormatSequence = Alert_FormatSequence + "C" + '>';
			}
			Alert_FormatOriginalSequence = Alert_FormatSequence;
		}

		public static void OldBuildAlertSequence()
		{
			string text = "";
			if (Alert_Flash)
			{
				text = "R" + '>' + "R" + '>' + "R" + '>' + "R" + '>';
			}
			Alert_FormatSequence = text;
			bool flag = Alert_Scroll;
			if ((Alert_OriginalMessage.Length + 30) * AlertGap > AlertTimeRemaining)
			{
				flag = false;
			}
			if (flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "S" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = 0;
			}
			else if (!flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = Alert_OriginalMessage.Length;
			}
			Alert_FormatSequence += text;
			for (int i = 1; i <= 30; i++)
			{
				Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
			}
			if (Alert_Scroll)
			{
				Alert_FormatSequence = Alert_FormatSequence + "C" + '>';
			}
			Alert_FormatOriginalSequence = Alert_FormatSequence;
		}

		public static void LoadComboBoxFromTextFile(ref ComboBox InComboBox, string InTextFile)
		{
			InComboBox.Items.Clear();
			string InString = "";
			if (LoadFileContents(InTextFile, ref InString))
			{
				InString = InString.Replace("\r\n", "\n");
				try
				{
					string[] array = InString.Split("\n"[0]);
					for (int i = 0; i <= array.GetUpperBound(0) && i < 20; i++)
					{
						if (DataUtil.Trim(array[i]) != "")
						{
							InComboBox.Items.Add(DataUtil.Trim(array[i]));
						}
					}
				}
				catch
				{
				}
			}
			InComboBox.Text = "";
		}

		public static void SaveComboBoxToTextFile(ref ComboBox InComboBox, string InTextFile)
		{
			string text = "";
			for (int i = 0; i < InComboBox.Items.Count && i < 20; i++)
			{
				if (DataUtil.Trim(InComboBox.Items[i].ToString()) != "")
				{
					text = text + DataUtil.TrimEnd(DataUtil.Trim(InComboBox.Items[i].ToString())) + "\r\n";
				}
			}
			FileUtil.CreateNewFile(InTextFile, FileUtil.FileContentsType.DoubleByte, text);
		}

		public static void LoadListViewFromTextFile(ref ListView InListView, string InTextFile)
		{
			InListView.Items.Clear();
			string InString = "";
			if (LoadFileContents(InTextFile, ref InString))
			{
				InString = InString.Replace("\r\n", "\n");
				try
				{
					string[] array = InString.Split("\n"[0]);
					for (int i = 0; i <= array.GetUpperBound(0); i++)
					{
						if (DataUtil.Trim(array[i]) != "")
						{
							InListView.Items.Add(DataUtil.Trim(array[i]));
						}
					}
				}
				catch
				{
				}
			}
			InListView.Text = "";
		}

		public static void SaveListViewToTextFile(ref ListView InListView, string InTextFile)
		{
			string text = "";
			for (int i = 0; i < InListView.Items.Count; i++)
			{
				if (DataUtil.Trim(InListView.Items[i].Text) != "")
				{
					text = text + DataUtil.TrimEnd(DataUtil.Trim(InListView.Items[i].Text)) + "\r\n";
				}
			}
			FileUtil.CreateNewFile(InTextFile, FileUtil.FileContentsType.DoubleByte, text);
		}

		public static void MapLyricsBreak(ref ListView InScreenBreak, ref RichTextBox InLyrics, ref bool ScreenBreakListDone)
		{
			int num = 0;
			int num2 = -1;
			int num3 = -1;
			int num4 = 0;
			string text = "";
			string text2 = "";
			bool flag = false;
			InLyrics.Text.Replace("\r\n", "\n");
			InLyrics.Text = DataUtil.TrimStart(InLyrics.Text);
			InScreenBreak.Items.Clear();
			InLyrics.Focus();
			while (num >= 0)
			{
				num2 = InLyrics.Text.IndexOf("[", (num < InLyrics.Text.Length) ? num : InLyrics.Text.Length);
				num3 = InLyrics.Text.IndexOf("\n\n", (num < InLyrics.Text.Length) ? num : InLyrics.Text.Length);
				if (!flag && num2 != 0 && num3 != 0)
				{
					num2 = -1;
					num3 = 0;
				}
				if (num2 >= 0 && num3 >= 0)
				{
					if (num2 < num3)
					{
						text = ValidateVerseIndicator(InLyrics.Text, num2);
						if (text != "")
						{
							if (text2 != text)
							{
								num4 = 0;
							}
							text2 = text;
							AddItemToScreenBreak(ref InScreenBreak, num2, text2, num4);
							num4++;
							num = text2.Length + num2;
						}
					}
					else
					{
						AddItemToScreenBreak(ref InScreenBreak, num3 + (flag ? 2 : 0), text2, num4);
						num4++;
						num = 2 + num3;
					}
				}
				else if (num2 >= 0)
				{
					text = ValidateVerseIndicator(InLyrics.Text, num2);
					if (text != "")
					{
						if (text2 != text)
						{
							num4 = 0;
						}
						text2 = text;
						AddItemToScreenBreak(ref InScreenBreak, num2, text2, num4);
						num4++;
						num = text2.Length + num2;
					}
				}
				else if (num3 >= 0)
				{
					AddItemToScreenBreak(ref InScreenBreak, num3 + (flag ? 2 : 0), text2, num4);
					num4++;
					num = 2 + num3;
				}
				else
				{
					num = -1;
				}
				flag = true;
			}
			ScreenBreakListDone = true;
		}

		public static string ValidateVerseIndicator(string InText, int StartPosition)
		{
			for (int i = 0; i <= 99; i++)
			{
				if (DataUtil.Mid(InText, StartPosition, VerseSymbol[i].Length) == VerseSymbol[i])
				{
					return VerseSymbol[i];
				}
			}
			for (int i = 100; i <= 112; i++)
			{
				if (DataUtil.Mid(InText, StartPosition, VerseSymbol[i].Length) == VerseSymbol[i])
				{
					return VerseSymbol[i];
				}
			}
			return "";
		}

		public static void AddItemToScreenBreak(ref ListView InListView, int NewPosition, string VerseSym, int ScreenBreakCount)
		{
			ListViewItem listViewItem = new ListViewItem();
			listViewItem = InListView.Items.Add(NewPosition.ToString());
			listViewItem.SubItems.Add(VerseSym);
			listViewItem.SubItems.Add(ScreenBreakCount.ToString());
		}

		public static void GetBreakPosition(ListView InListView, int CurPosition, int Direction, ref int NewPosition, ref int NewPositionLength, ref string LookupVerseSym, ref int LookupScreenCount)
		{
			if (InListView.Items.Count == 0)
			{
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = -1;
				LookupScreenCount = 0;
				return;
			}
			int num = -1;
			if (Direction == 0)
			{
				int num2 = InListView.Items.Count - 1;
				while (true)
				{
					if (num2 >= 0)
					{
						if (DataUtil.StringToInt(InListView.Items[num2].SubItems[0].Text) < CurPosition)
						{
							num = num2;
							break;
						}
						num2--;
						continue;
					}
					num = 0;
					break;
				}
			}
			else
			{
				int num2 = 0;
				while (true)
				{
					if (num2 < InListView.Items.Count)
					{
						if (DataUtil.StringToInt(InListView.Items[num2].SubItems[0].Text) > CurPosition)
						{
							if (NewPositionLength == 0 && num2 > 0)
							{
								num2--;
							}
							num = num2;
							break;
						}
						num2++;
						continue;
					}
					num = InListView.Items.Count - 1;
					break;
				}
			}
			NewPosition = DataUtil.StringToInt(InListView.Items[num].SubItems[0].Text);
			LookupVerseSym = InListView.Items[num].SubItems[1].Text;
			LookupScreenCount = DataUtil.StringToInt(InListView.Items[num].SubItems[2].Text);
			if (num < InListView.Items.Count - 1)
			{
				int num3 = DataUtil.StringToInt(InListView.Items[num + 1].SubItems[0].Text);
				NewPositionLength = num3 - NewPosition;
			}
			else
			{
				NewPositionLength = -1;
			}
		}

		public static void GetBreakPosition(ListView InListView, ref int NewPosition, ref int NewPositionLength, string LookupVerseSym, int LookupScreenCount)
		{
			if (InListView.Items.Count == 0)
			{
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = 0;
				LookupScreenCount = 0;
				return;
			}
			int num = -1;
			int num2 = 0;
			while (true)
			{
				if (num2 < InListView.Items.Count)
				{
					if (InListView.Items[num2].SubItems[1].Text == LookupVerseSym && DataUtil.StringToInt(InListView.Items[num2].SubItems[2].Text) == LookupScreenCount)
					{
						num = num2;
						break;
					}
					num2++;
					continue;
				}
				if (num >= 0)
				{
					break;
				}
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = 0;
				LookupScreenCount = 0;
				return;
			}
			NewPosition = DataUtil.StringToInt(InListView.Items[num].SubItems[0].Text);
			if (num < InListView.Items.Count - 1)
			{
				int num3 = DataUtil.StringToInt(InListView.Items[num + 1].SubItems[0].Text);
				NewPositionLength = num3 - NewPosition;
			}
			else
			{
				NewPositionLength = -1;
			}
		}

		public static bool RecycleBin(string FullFileName)
		{
			try
			{
				SHFILEOPSTRUCT lpFileOp = default(SHFILEOPSTRUCT);
				lpFileOp.hwnd = IntPtr.Zero;
				lpFileOp.wFunc = 3u;
				lpFileOp.fFlags = 80;
				lpFileOp.pFrom = FullFileName + '\0' + '\0';
				lpFileOp.fAnyOperationsAborted = false;
				lpFileOp.hNameMappings = IntPtr.Zero;
				SHFileOperation(ref lpFileOp);
				return !File.Exists(FullFileName);
			}
			catch
			{
				return false;
			}
		}

		public static string CopyExternalFile(string SourceFileName, string CopyToFolder)
		{
			if (File.Exists(SourceFileName))
			{
				int num = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				string text = Path.GetDirectoryName(SourceFileName) + "\\";
				if (!Directory.Exists(CopyToFolder) && !FileUtil.MakeDir(CopyToFolder))
				{
					return "";
				}
				string text2 = CopyToFolder + displayNameOnly + extension;
				while (File.Exists(text2))
				{
					num++;
					text2 = CopyToFolder + displayNameOnly + " - Copy (" + num + ")" + extension;
				}
				try
				{
					File.Copy(SourceFileName, text2);
					return text2;
				}
				catch
				{
				}
			}
			return "";
		}

		public static string MoveExternalFile(string SourceFileName, string MoveToFolder)
		{
			if (File.Exists(SourceFileName))
			{
				int num = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				string text = Path.GetDirectoryName(SourceFileName) + "\\";
				if (!Directory.Exists(MoveToFolder) && !FileUtil.MakeDir(MoveToFolder))
				{
					return "";
				}
				string text2 = MoveToFolder + displayNameOnly + extension;
				while (File.Exists(text2))
				{
					num++;
					text2 = MoveToFolder + displayNameOnly + " - Copy (" + num + ")" + extension;
				}
				try
				{
					File.Move(SourceFileName, text2);
					return text2;
				}
				catch
				{
				}
			}
			return "";
		}

		public static void ScanSelectedRTB(ref RichTextBox InTextBox, bool[] VersePresent, bool DoAll, int StartPos, int EndPos, string[] InsArray, Font MainFont, Font NotationFont, bool DoNotations)
		{
			if (InTextBox.Text == "")
			{
				return;
			}
			int selectionStart = EndPos;
			if (DoAll)
			{
				StartPos = 0;
				EndPos = InTextBox.Text.Length - 1;
			}
			else
			{
				if (StartPos > EndPos)
				{
					int num = EndPos;
					StartPos = EndPos;
					EndPos = StartPos;
				}
				MoveToPosInLine(InTextBox.Text, ref StartPos, 0);
				MoveToPosInLine(InTextBox.Text, ref EndPos, 1);
			}
			int num2 = 0;
			MarkSelectedRTB(ref InTextBox, StartPos, EndPos - StartPos + 1, 0, MainFont, NotationFont);
			for (num2 = 0; num2 <= InsArray.GetUpperBound(0); num2++)
			{
				if (StartPos < 0)
				{
					StartPos = 0;
				}
				if (InsArray[num2] == VerseSymbol[150])
				{
					int num3;
					try
					{
						num3 = InTextBox.Text.IndexOf(InsArray[num2], StartPos);
					}
					catch
					{
						num3 = -1;
					}
					while (num3 >= 0)
					{
						MarkSelectedRTB(ref InTextBox, num3, InsArray[num2].Length, 1, MainFont, NotationFont);
						num3 = InTextBox.Text.IndexOf(InsArray[num2], num3 + 1);
					}
				}
				else
				{
					int num3;
					try
					{
						num3 = InTextBox.Text.IndexOf(InsArray[num2], StartPos);
					}
					catch
					{
						num3 = -1;
					}
					if (num3 >= 0 && num3 <= EndPos)
					{
						MarkSelectedRTB(ref InTextBox, num3, InsArray[num2].Length, 1, MainFont, NotationFont);
					}
				}
			}
			if (DoNotations)
			{
				int num3;
				try
				{
					num3 = InTextBox.Text.IndexOf("쨩", StartPos);
				}
				catch
				{
					num3 = -1;
				}
				while (num3 >= 0 && num3 <= EndPos)
				{
					int CurPos = num3;
					int CurPos2 = num3;
					MoveToPosInLine(InTextBox.Text, ref CurPos, 0);
					MoveToPosInLine(InTextBox.Text, ref CurPos2, 1);
					MarkSelectedRTB(ref InTextBox, CurPos, CurPos2 - CurPos + 1, 2, MainFont, NotationFont);
					num3 = InTextBox.Text.IndexOf("쨩", num3 + 1);
				}
			}
			InTextBox.SelectionStart = selectionStart;
		}

		public static void MarkSelectedRTB(ref RichTextBox InTextBox, int SelStartPos, int SelLen, int InMode, Font MainFont, Font NotationFont)
		{
			SendMessage(InTextBox.Handle, 11u, 0u, 0u);
			InTextBox.SelectionStart = SelStartPos;
			InTextBox.SelectionLength = SelLen;
			switch (InMode)
			{
				case 0:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = MainFont;
					InTextBox.SelectionColor = NormalTextColour;
					break;
				case 1:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = MainFont;
					InTextBox.SelectionColor = SelectedTextColour;
					break;
				case 2:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = NotationFont;
					InTextBox.SelectionColor = NotationColour;
					break;
			}
			InTextBox.SelectionStart = SelStartPos + SelLen;
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionColor = NormalTextColour;
			SendMessage(InTextBox.Handle, 11u, 1u, 0u);
			InTextBox.Refresh();
		}

		public static bool SaveXMLInfoScreen(SongSettings InItem, string InFileName, string[] InHeaderData, bool ReloadImageData, bool UseOriginalNotations)
		{
            XmlTextWriter xtw = null;

            try
			{
				xtw = new XmlTextWriter(InFileName, Encoding.UTF8);
				xtw.Formatting = Formatting.Indented;
				xtw.WriteStartDocument();
				xtw.WriteStartElement("EasiSlides");
				WriteXMLOneItem(ref xtw, InItem, InHeaderData, ReloadImageData, UseOriginalNotations);
				xtw.WriteEndDocument();
				xtw.Flush();
				xtw.Dispose();
				return true;
			}
			catch
			{
				if(xtw!= null)	
					xtw.Dispose();
				return false;
			}
		}

		public static void WriteXMLOneItem(ref XmlTextWriter xtw, SongSettings InItem, string[] InHeaderData, bool ReloadImageData)
		{
			WriteXMLOneItem(ref xtw, InItem, InHeaderData, ReloadImageData, UseOriginalNotations: true);
		}

		public static void WriteXMLOneItem(ref XmlTextWriter xtw, SongSettings InItem, string[] InHeaderData, bool ReloadImageData, bool UseOriginalNotations)
		{
			string text = InItem.Format.FormatString;
			if (InHeaderData != null)
			{
				for (int i = 2; i <= 254; i++)
				{
					if (InHeaderData[i] != "" && InHeaderData[i] != null)
					{
						object obj = text;
						text = string.Concat(obj, (char)i, "=", InHeaderData[i], '>'.ToString());
					}
				}
			}
			try
			{
				xtw.WriteStartElement("Item");
				xtw.WriteElementString("Title1", InItem.Title);
				xtw.WriteElementString("Title2", InItem.Title2);
				xtw.WriteElementString("Folder", FolderName[InItem.FolderNo]);
				xtw.WriteElementString("SongNumber", InItem.SongNumber.ToString());
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\r\n", "\n");
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\n", "\r\n");
				xtw.WriteElementString("Contents", InItem.CompleteLyrics);
				xtw.WriteElementString("Notations", UseOriginalNotations ? InItem.OriginalNotations : InItem.Notations);
				xtw.WriteElementString("Sequence", ConvertSequenceToTextString(InItem.SongSequence));
				xtw.WriteElementString("Writer", InItem.Writer);
				xtw.WriteElementString("Copyright", InItem.Copyright);
				xtw.WriteElementString("Category", InItem.Category);
				xtw.WriteElementString("Timing", InItem.Timing);
				xtw.WriteElementString("MusicKey", InItem.MusicKey);
				xtw.WriteElementString("Capo", InItem.Capo.ToString());
				xtw.WriteElementString("LicenceAdmin1", InItem.Show_LicAdminInfo1);
				xtw.WriteElementString("LicenceAdmin2", InItem.Show_LicAdminInfo2);
				xtw.WriteElementString("BookReference", InItem.Book_Reference);
				xtw.WriteElementString("UserReference", InItem.User_Reference);
				xtw.WriteElementString("FormatData", text);
				xtw.WriteElementString("Settings", InItem.Settings);
				if (ReloadImageData && InItem.Format.BackgroundPicture != null && InItem.Format.BackgroundPicture != "")
				{
					Base64EncodeImageFile(ref xtw, "Image", InItem.Format.BackgroundPicture);
				}
				else if (InItem.Format.ImageString != "")
				{
					xtw.WriteElementString("Image", InItem.Format.ImageString);
				}
				xtw.WriteEndElement();
			}
			catch
			{
			}
		}

		public static void Base64EncodeImageFile(ref XmlTextWriter xtw, string InElementString, string InFileName)
		{
			FileInfo fileInfo = new FileInfo(InFileName);
			using FileStream fileStream = fileInfo.OpenRead();
			byte[] array = new byte[fileInfo.Length];
			fileStream.ReadExactly(array, 0, array.Length);
			//fileStream.Close();
			xtw.WriteStartElement(InElementString);
			xtw.WriteBase64(array, 0, array.Length);
		}

		public static string ConvertSequenceToTextString(string InSequence)
		{
			if (InSequence.Length > 0)
			{
				string text = "";
				for (int i = 0; i < InSequence.Length; i++)
				{
					int num = InSequence[i];
					text = ((num <= 0 || num >= 13) ? (text + SequenceSymbol[num]) : (text + num));
					if (i < InSequence.Length - 1)
					{
						text += ",";
					}
				}
				return text;
			}
			return "";
		}

		public static string ConvertTextStringToSequence(string InSequence)
		{
			if (InSequence.Length > 0)
			{
				InSequence = InSequence.ToLower();
				string text = "";
				int num = -1;
				string text2 = "";
				while (InSequence != "")
				{
					text = DataUtil.ExtractOneInfo(ref InSequence, ',');
					num = -1;
					if (text != "")
					{
						switch (text)
						{
							case "c":
								num = 0;
								break;
							case "b":
								num = 100;
								break;
							case "w":
								num = 103;
								break;
							case "e":
								num = 101;
								break;
							case "t":
								num = 102;
								break;
							case "p":
								num = 111;
								break;
							case "q":
								num = 112;
								break;
							default:
								num = DataUtil.StringToInt(text, Minus1IfBlank: true);
								if (num > 9 || num < 0)
								{
									num = -1;
								}
								break;
						}
					}
					if (num >= 0)
					{
						text2 += (char)num;
					}
				}
				return text2;
			}
			return "";
		}

		public static string FormatMode(int InPart)
		{
			int num = PB_WordsBold[InPart];
			int num2 = PB_WordsItalic[InPart];
			int num3 = PB_WordsUnderline[InPart];
			string str = "\\cf" + Convert.ToString(InPart + 1);
			string str2 = (num > 0) ? ((num2 > 0) ? ((num3 <= 0) ? "\\b\\i\\ulnone " : "\\b\\i\\ul ") : ((num3 <= 0) ? "\\b\\i0\\ulnone " : "\\b\\i0\\ul ")) : ((num2 > 0) ? ((num3 <= 0) ? "\\b0\\i\\ulnone " : "\\b0\\i\\ul ") : ((num3 <= 0) ? "\\b0\\i0\\ulnone " : "\\b0\\i0\\ul "));
			return "\\fs" + Convert.ToString(PB_WordsSize[InPart] * 2) + str + str2;
		}

#if DAO
		public static bool LoadDataIntoItem(ref SongSettings InItem, Recordset rs, string InID)
		{
			try
			{
				rs.Seek("=", InID, def, def, def, def, def, def, def, def, def, def, def, def);
				if (!rs.NoMatch)
				{
					return LoadDataIntoItem(ref InItem, rs);
				}

				InitialiseIndividualData(ref InItem);
			}
			catch
			{
			}
			return false;
		}
#endif

		public static bool LoadDataIntoItem(ref SongSettings InItem, DataTable dt, string InID)
		{
			try
			{
				DataRow[] arrRows = null;
				arrRows = dt.Select("SONGID='"+ InID + "'");

				if (arrRows != null && arrRows.Length > 0)
				{
					return LoadDataIntoItem(ref InItem, arrRows[0]);
				}

				InitialiseIndividualData(ref InItem);
			}
			catch
			{
			}
			return false;
		}

#if DAO
		public static bool LoadDataIntoItem(ref SongSettings InItem, Recordset rs)
		{
			InitialiseIndividualData(ref InItem);
			try
			{
				InItem.Title = DataUtil.GetDataString(rs, "Title_1");
				InItem.Title2 = DataUtil.GetDataString(rs, "Title_2");
				InItem.SongNumber = DataUtil.GetDataInt(rs, "song_number");
				InItem.CompleteLyrics = DataUtil.GetDataString(rs, "Lyrics");
				InItem.FolderNo = DataUtil.GetDataInt(rs, "FolderNo");
				InItem.Copyright = DataUtil.GetDataString(rs, "Copyright");
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(rs, "LICENCE_ADMIN1");
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(rs, "LICENCE_ADMIN2");
				InItem.Format.FormatString = DataUtil.GetDataString(rs, "FORMATDATA");
				InItem.Notations = DataUtil.GetDataString(rs, "msc");
				InItem.Capo = DataUtil.GetDataInt(rs, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(rs, "Sequence");
				InItem.Writer = DataUtil.GetDataString(rs, "Writer");
				InItem.Book_Reference = DataUtil.GetDataString(rs, "Book_Reference");
				InItem.User_Reference = DataUtil.GetDataString(rs, "User_Reference");
				InItem.Timing = DataUtil.GetDataString(rs, "timing");
				InItem.MusicKey = DataUtil.GetDataString(rs, "key");
				InItem.Category = DataUtil.GetDataString(rs, "category");
				InItem.Settings = DataUtil.GetDataString(rs, "SETTINGS");
				return true;
			}
			catch
			{
			}
			return false;
		}
#endif

		public static bool LoadDataIntoItem(ref SongSettings InItem, DataRow dr)
		{
			InitialiseIndividualData(ref InItem);
			try
			{
				InItem.Title = DataUtil.GetDataString(dr, "Title_1");
				InItem.Title2 = DataUtil.GetDataString(dr, "Title_2");
				InItem.SongNumber = DataUtil.GetDataInt(dr, "song_number");
				InItem.CompleteLyrics = DataUtil.GetDataString(dr, "Lyrics");
				InItem.FolderNo = DataUtil.GetDataInt(dr, "FolderNo");
				InItem.Copyright = DataUtil.GetDataString(dr, "Copyright");
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
				InItem.Format.FormatString = DataUtil.GetDataString(dr, "FORMATDATA");
				InItem.Notations = DataUtil.GetDataString(dr, "msc");
				InItem.Capo = DataUtil.GetDataInt(dr, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(dr, "Sequence");
				InItem.Writer = DataUtil.GetDataString(dr, "Writer");
				InItem.Book_Reference = DataUtil.GetDataString(dr, "Book_Reference");
				InItem.User_Reference = DataUtil.GetDataString(dr, "User_Reference");
				InItem.Timing = DataUtil.GetDataString(dr, "timing");
				InItem.MusicKey = DataUtil.GetDataString(dr, "key");
				InItem.Category = DataUtil.GetDataString(dr, "category");
				InItem.Settings = DataUtil.GetDataString(dr, "SETTINGS");
				return true;
			}
			catch
			{
			}
			return false;
		}

		public static string LookUpMediaString(DShowLib Player)
		{
			switch (LookUpMediaInteger(Player))
			{
				case 1:
					return "Audio Only";
				case 2:
					return "Video";
				default:
					return "No Media Playing";
			}
		}

		public static string LookUpMediaString(DShowLib Player, int InCode, int WaitCount)
		{
			switch (InCode)
			{
				case -1:
					return "Selected Source Not Playable";
				case 0:
					{
						WaitCount /= 3;
						string text = "";
						for (int i = 0; i < WaitCount; i++)
						{
							text += ".";
						}
						return "Connecting to media..." + text;
					}
				default:
					return "No Media Playing";
			}
		}

		public static int LookUpMediaInteger(DShowLib Player)
		{
			try
			{
				if (Player.newFilename != "")
				{
					if (Player.isVideo)
					{
						return 1;
					}
					return 2;
				}
				return 3;
			}
			catch
			{
				return 3;
			}
		}

		public static string GetMediaLocation(SongSettings InItem)
		{
			return GetMediaLocation(InItem.Format.MediaOption, InItem.Title, InItem.Title2, InItem.UseDefaultFormat, InItem.Type, InItem.Format.MediaLocation, InItem.Format.MediaCaptureDeviceNumber);
		}

		public static string GetMediaLocation(int InMediaOption, string InTitle1, string InTitle2, bool InUseDefaultFormat, string InType, string InMediaLocation, int InMediaCaptureDeviceNumber)
		{
			string text = "";
			InMediaOption = ((InUseDefaultFormat && InType != "M") ? MediaOption : InMediaOption);
			switch (InMediaOption)
			{
				case 1:
					return GetMediaFileName(InTitle1, InTitle2);
				case 2:
					return (InUseDefaultFormat && InType != "M") ? MediaLocation : InMediaLocation;
				case 3:
					return "<<Capture>>" + (InUseDefaultFormat ? MediaCaptureDeviceNumber.ToString() : InMediaCaptureDeviceNumber.ToString());
				default:
					return "";
			}
		}

		public static string OldGetMediaLocation(SongSettings InItem)
		{
			string text = "";
			switch (InItem.Format.MediaOption)
			{
				case 1:
					return GetMediaFileName(InItem.Title, InItem.Title2);
				case 2:
					return (InItem.UseDefaultFormat && InItem.Type != "M") ? MediaLocation : InItem.Format.MediaLocation;
				case 3:
					return "<<Capture>>" + InItem.Format.MediaCaptureDeviceNumber;
				default:
					return "";
			}
		}

		public static void GetRotationStyle(ref SongSettings InItem)
		{
			string InString = InItem.RotateString;
			string InString2 = "";
			string text = "";
			string[] array = InString.Split("쨩"[0]);
			if (array.GetUpperBound(0) >= 0)
			{
				InString = array[0];
				if (array.GetUpperBound(0) >= 1)
				{
					InString2 = array[1];
				}
			}
			int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (num < 1 || num > 2)
			{
				num = 1;
			}
			InItem.RotateStyle = num;
			int num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (num2 < 0 || num2 > 99999)
			{
				num2 = 0;
			}
			InItem.RotateGap = num2;
			int num3 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (num3 < 0 || num3 > 3599)
			{
				num3 = 0;
			}
			InItem.RotateTotal = num3;
			text = InString;
			InItem.RotateTimings = "";
			InItem.RotateSequence = "";
			while (text != "")
			{
				num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref text, ';', RemoveExtract: true, MinusOneIfBlank: true));
				if ((num2 >= 0 && num2 < 99) || (num2 >= 100 && num2 < 112))
				{
					InItem.RotateSequence += (char)num2;
				}
			}
			int num4 = num;
			if (num4 != 2)
			{
				return;
			}
			ListBox listBox = new ListBox();
			listBox.Sorted = false;
			while (InString2 != "")
			{
				num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString2, ';', RemoveExtract: true, MinusOneIfBlank: true));
				if (num2 > 0 && num2 <= 99999)
				{
					listBox.Items.Add(num2.ToString("00000"));
				}
			}
			if (listBox.Items.Count > 0)
			{
				listBox.Sorted = true;
				string text2 = "";
				for (int i = 0; i < listBox.Items.Count; i++)
				{
					text2 = DataUtil.StringToInt(listBox.Items[i].ToString()).ToString();
					SongSettings obj = InItem;
					obj.RotateTimings = obj.RotateTimings + text2 + ';';
				}
			}
		}

		internal static int GetNextNonRotateItem(bool CurrentItemIsGapItem)
		{
			int startPresAt = StartPresAt;
			int startPresAt2 = StartPresAt;
			int num = -1;
			int num2 = -1;
			if (TotalWorshipListItems > 0)
			{
				int num3 = startPresAt2 + 1;
				for (num3 = startPresAt2 + 1; num3 <= TotalWorshipListItems; num3++)
				{
					int itemRotateResult = GetItemRotateResult(TempItem2, WorshipSongs[num3, 0]);
					if (itemRotateResult < 1)
					{
						if (num < 0)
						{
							num = num3;
						}
						else if (num2 < 0)
						{
							num2 = num3;
						}
						if (num2 > 0)
						{
							num3 = TotalWorshipListItems;
						}
					}
				}
				if (num > 0)
				{
					if (GapItemOption == GapType.None)
					{
						return num;
					}
					if (!CurrentItemIsGapItem)
					{
						return num - 1;
					}
					if (num == startPresAt2 + 1)
					{
						return num;
					}
					return num - 1;
				}
			}
			return startPresAt;
		}

		internal static int GetItemRotateResult(string InIDString)
		{
			return GetItemRotateResult(TempItem1, InIDString);
		}

		internal static int GetItemRotateResult(SongSettings InItem, string InIDString)
		{
			string a = DataUtil.Left(InIDString, 1);
			if ((a == "I") | (a == "D"))
			{
				string InTitle = "";
				LoadIndividualData(ref InItem, InIDString, "", 1, ref InTitle);
				MediaBackgroundStyle mediaBackgroundType = GetMediaBackgroundType(InItem, UpdateVariables: false);
				switch (InItem.RotateStyle)
				{
					case 1:
						if (InItem.RotateGap > 0)
						{
							return (mediaBackgroundType != MediaBackgroundStyle.Video && mediaBackgroundType != MediaBackgroundStyle.SameAsPrevious) ? 1 : 2;
						}
						break;
					case 2:
						if (InItem.RotateTimings != "" || InItem.RotateTotal > 0)
						{
							return (mediaBackgroundType != MediaBackgroundStyle.Video && mediaBackgroundType != MediaBackgroundStyle.SameAsPrevious) ? 1 : 2;
						}
						break;
				}
			}
			return 0;
		}

		public static MediaBackgroundStyle GetMediaBackgroundType(SongSettings InItem, bool UpdateVariables)
		{
			string mediaLocation = GetMediaLocation(InItem);
			if (mediaLocation == "")
			{
				if (UpdateVariables)
				{
					CurrentMediaLocation = "";
				}
				return MediaBackgroundStyle.None;
			}
			if (mediaLocation == CurrentMediaLocation)
			{
				return MediaBackgroundStyle.SameAsPrevious;
			}
			MediaBackgroundStyle mediaType = GetMediaType(mediaLocation);
			if (UpdateVariables)
			{
				CurrentMediaLocation = mediaLocation;
				CurrentMediaIsVideo = ((mediaType == MediaBackgroundStyle.Video) ? true : false);
			}
			return mediaType;
		}

		public static MediaBackgroundStyle GetMediaType(string InLocation)
		{
			if (DataUtil.Left(InLocation, "<<Capture>>".Length) == "<<Capture>>")
			{
				return MediaBackgroundStyle.Video;
			}
			string text = "";
			try
			{
				text = Path.GetExtension(InLocation).ToLower();
			}
			catch
			{
				return MediaBackgroundStyle.Audio;
			}
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				if (MediaFileExtension[i, 0] == text)
				{
					if (MediaFileExtension[i, 1] == MediaBackgroundStyle.Video.ToString())
					{
						return MediaBackgroundStyle.Video;
					}
					return MediaBackgroundStyle.Audio;
				}
			}
			return MediaBackgroundStyle.Audio;
		}

		public static void SubDivideTextAndNotations(string InString, string InNotation, Font MainFont, Font NotationsFont, ref ListView TextNotationsList, int InWidth)
		{
			InWidth /= 15;
			Graphics graphics = TextNotationsList.CreateGraphics();
			int num = -1;
			ListViewItem listViewItem = new ListViewItem();
			int num2 = 0;
			TextNotationsList.Items.Clear();
			while (InString != "")
			{
				int length = InString.Length;
				for (int num3 = length; num3 >= 1; num3--)
				{
					string text = DataUtil.Left(InString, num3);
					if (((graphics.MeasureString(text, MainFont, 32000, StringFormat.GenericDefault).Width <= (float)InWidth) | (text.IndexOf(" ") < 0)) && ((DataUtil.Right(text, 1) == " ") | (num3 == length) | (text.IndexOf(" ") < 0)))
					{
						listViewItem = TextNotationsList.Items.Add(text);
						listViewItem.SubItems.Add("");
						string InString2 = InNotation;
						string text2 = "";
						while (InString2 != "")
						{
							string text3 = DataUtil.ExtractOneInfo(ref InString2, ';');
							string text4 = DataUtil.ExtractOneInfo(ref InString2, ';');
							if (((text3 != "-1") & (text4 != "-1")) && Convert.ToInt32(text4) >= num2)
							{
								if (Convert.ToInt32(text4) >= num2 + num3 && num3 < length)
								{
									InString2 = "";
									continue;
								}
								object obj = text2;
								text2 = string.Concat(obj, text3, ';', Convert.ToString(Convert.ToInt32(text4) - num2), ';');
							}
						}
						listViewItem.SubItems.Add((text2 != "") ? text2 : " ");
						listViewItem.SubItems.Add(Convert.ToString(num2));
						num2 += num3;
						InString = DataUtil.Mid(InString, num3);
						num3 = 0;
					}
				}
			}
			for (int num3 = 0; num3 < TextNotationsList.Items.Count; num3++)
			{
				TextNotationsList.Items[num3].SubItems[1].Text = FormatNotationString(TextNotationsList, TextNotationsList.Items[num3].SubItems[0].Text, TextNotationsList.Items[num3].SubItems[2].Text, MainFont, NotationsFont);
			}
		}

		public static void OldSubDivideTextAndNotations(string InString, string InNotation, Font MainFont, Font NotationsFont, ref ListView TextNotationsList, int InWidth)
		{
			InWidth /= 15;
			Graphics graphics = TextNotationsList.CreateGraphics();
			int num = -1;
			ListViewItem listViewItem = new ListViewItem();
			int num2 = 0;
			TextNotationsList.Items.Clear();
			while (InString != "")
			{
				int length = InString.Length;
				for (int num3 = length; num3 >= 1; num3--)
				{
					string text = DataUtil.Left(InString, num3);
					if (((graphics.MeasureString(text, MainFont, 32000, StringFormat.GenericDefault).Width <= (float)InWidth) | (text.IndexOf(" ") < 0)) && ((DataUtil.Right(text, 1) == " ") | (num3 == length) | (text.IndexOf(" ") < 0)))
					{
						listViewItem = TextNotationsList.Items.Add(text);
						listViewItem.SubItems.Add("");
						string InString2 = InNotation;
						string text2 = "";
						while (InString2 != "")
						{
							string text3 = DataUtil.ExtractOneInfo(ref InString2, ';');
							string text4 = DataUtil.ExtractOneInfo(ref InString2, ';');
							if (((text3 != "-1") & (text4 != "-1")) && Convert.ToInt32(text4) >= num2)
							{
								if (Convert.ToInt32(text4) >= num2 + num3 && num3 < length)
								{
									InString2 = "";
									continue;
								}
								object obj = text2;
								text2 = string.Concat(obj, text3, ';', Convert.ToString(Convert.ToInt32(text4) - num2), ';');
							}
						}
						listViewItem.SubItems.Add((text2 != "") ? text2 : " ");
						listViewItem.SubItems.Add(Convert.ToString(num2));
						num2 += num3;
						InString = DataUtil.Mid(InString, num3);
						num3 = 0;
					}
				}
			}
			for (int num3 = 0; num3 < TextNotationsList.Items.Count; num3++)
			{
			}
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

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox)
		{
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret: true);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret)
		{
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret, BlackScreenColour, Color.Red);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret, Color TextColour, Color HighlightColour)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InTextBox.Select(0, InTextBox.Text.Length);
			InTextBox.SelectionColor = TextColour;
			InTextBox.Select(InItem.Slide[InItem.CurSlide, 5], InItem.Slide[InItem.CurSlide, 6]);
			InTextBox.SelectionColor = HighlightColour;
			InTextBox.SelectionLength = 0;
			if (ScrollToCaret)
			{
				InTextBox.Select(InItem.Slide[InItem.CurSlide, 5] + InItem.Slide[InItem.CurSlide, 6] + 90, 0);
				InTextBox.ScrollToCaret();
				InTextBox.Select(InItem.Slide[InItem.CurSlide, 5], 0);
				InTextBox.ScrollToCaret();
			}
		}

		public static void DisplaySlidesFormattedLyrics(ref SongSettings InItem, ref RichTextBox PInTextBox, ref RichTextBox OInTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			if (InItem.OutputStyleScreen)
			{
				DisplaySlidesFormattedLyrics(ref InItem, ref OInTextBox, ScrollToCaret, PreviewNotations);
			}
			else
			{
				DisplaySlidesFormattedLyrics(ref InItem, ref PInTextBox, ScrollToCaret, PreviewNotations);
			}
		}

		public static void DisplaySlidesFormattedLyrics(ref SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InItem.FolderNo = ((InItem.FolderNo <= 0) ? 1 : InItem.FolderNo);
			int num = 0;
			InTextBox.Text = "";
			if (InItem.Type == "P")
			{
				return;
			}
			int num2 = 0;
			int num3 = 0;
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				num2 = InTextBox.Text.Length;
				int num4 = 0;
				try
				{
					if (InItem.Slide[i, 0] >= 0)
					{
						if (i > 1)
						{
							InTextBox.Text += "\n";
						}
						if (InItem.Slide[i, 0] == 0)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? FolderLyricsHeading[InItem.FolderNo, 1] : "Chorus:");
						}
						else if (InItem.Slide[i, 0] == 102)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? (FolderLyricsHeading[InItem.FolderNo, 1] + " (2)") : "Chorus 2:");
						}
						else if (InItem.Slide[i, 0] == 111)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? FolderLyricsHeading[InItem.FolderNo, 0] : "Prechorus:");
						}
						else if (InItem.Slide[i, 0] == 112)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? (FolderLyricsHeading[InItem.FolderNo, 0] + " (2)") : "Prechorus 2:");
						}
						else if (InItem.Slide[i, 0] == 100)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? FolderLyricsHeading[InItem.FolderNo, 2] : "Bridge:");
						}
						else if (InItem.Slide[i, 0] == 103)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? (FolderLyricsHeading[InItem.FolderNo, 2] + " (2)") : "Bridge 2:");
						}
						else if (InItem.Slide[i, 0] == 101)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 3] != "") ? FolderLyricsHeading[InItem.FolderNo, 3] : "Ending:");
						}
						else if (InItem.Verse2Present || (i > 1 && InItem.Slide[i, 0] == 1))
						{
							RichTextBox obj = InTextBox;
							obj.Text = obj.Text + VerseTitle[InItem.Slide[i, 0]] + ".";
							int num5 = InItem.Slide[i, 0];
						}
						num4 = InTextBox.Text.Length - num2;
					}
				}
				catch
				{
					MessageBox.Show("Error");
				}
				InTextBox.Text += "\n";
				if (InItem.Slide[i, 2] >= 0)
				{
					InTextBox.Text += GetSlideContents(InItem, i, 0, InTextBox.Font, PreviewNotations);
				}
				if (InItem.Slide[i, 4] >= 0)
				{
					InTextBox.Text += GetSlideContents(InItem, i, 1, InTextBox.Font, PreviewNotations);
				}
				num3 = InTextBox.Text.Length - num2 + 1;
				InItem.Slide[i, 5] = num2;
				InItem.Slide[i, 6] = num3;
				InItem.Slide[i, 7] = num4;
			}
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				if (InItem.Slide[InItem.CurSlide, 7] > 0)
				{
					InTextBox.Select(InItem.Slide[i, 5], InItem.Slide[i, 7]);
					InTextBox.SelectionFont = new Font(InTextBox.Font, FontStyle.Regular);
					InTextBox.SelectionLength = 0;
				}
			}
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret);
		}

		public static void DisplayRichTextBoxSeries(ref SongSettings InItem, ref Panel InPanel, ref RichTextBox[] InRichTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InItem.FolderNo = ((InItem.FolderNo <= 0) ? 1 : InItem.FolderNo);
			int num = 0;
			if (InRichTextBox[1] == null || InItem.Type == "P")
			{
				return;
			}
			int num2 = 0;
			int num3 = 0;
			string text = "";
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				text = "";
				num2 = text.Length;
				int num4 = 0;
				try
				{
					if (InItem.Slide[i, 0] >= 0)
					{
						if (i > 1)
						{
							text += "\n";
						}
						if (InItem.Slide[i, 0] == 0)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? FolderLyricsHeading[InItem.FolderNo, 1] : "Chorus:");
						}
						else if (InItem.Slide[i, 0] == 102)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? (FolderLyricsHeading[InItem.FolderNo, 1] + " (2)") : "Chorus 2:");
						}
						else if (InItem.Slide[i, 0] == 111)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? FolderLyricsHeading[InItem.FolderNo, 0] : "Prechorus:");
						}
						else if (InItem.Slide[i, 0] == 112)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? (FolderLyricsHeading[InItem.FolderNo, 0] + " (2)") : "Prechorus 2:");
						}
						else if (InItem.Slide[i, 0] == 100)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? FolderLyricsHeading[InItem.FolderNo, 2] : "Bridge:");
						}
						else if (InItem.Slide[i, 0] == 103)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? (FolderLyricsHeading[InItem.FolderNo, 2] + " (2)") : "Bridge 2:");
						}
						else if (InItem.Slide[i, 0] == 101)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 3] != "") ? FolderLyricsHeading[InItem.FolderNo, 3] : "Ending:");
						}
						else if (InItem.Verse2Present || (i > 1 && InItem.Slide[i, 0] == 1))
						{
							text = text + VerseTitle[InItem.Slide[i, 0]] + ".";
							int num5 = InItem.Slide[i, 0];
						}
						num4 = text.Length - num2;
					}
				}
				catch
				{
					MessageBox.Show("Error");
				}
				text += "\n";
				if (InItem.Slide[i, 2] >= 0)
				{
					text += GetSlideContents(InItem, i, 0, InRichTextBox[1].Font, PreviewNotations);
				}
				if (InItem.Slide[i, 4] >= 0)
				{
					text += GetSlideContents(InItem, i, 1, InRichTextBox[1].Font, PreviewNotations);
				}
				text = DataUtil.TrimStart(text);
				text = DataUtil.TrimEnd(text);
				if (InRichTextBox[i] != null)
				{
					InRichTextBox[i].Text = text;
					InRichTextBox[i].SelectAll();
					InRichTextBox[i].SelectionFont = new Font("Microsoft Sans Serif", InRichTextBox[i].Font.Size, InRichTextBox[i].Font.Style);
					InRichTextBox[i].SelectionStart = 0;
					InRichTextBox[i].SelectionLength = 0;
				}
				num3 = text.Length - num2 + 1;
				InItem.Slide[i, 5] = num2;
				InItem.Slide[i, 6] = num3;
				InItem.Slide[i, 7] = num4;
			}
			HighlightRichTextBox(ref InRichTextBox, ref InPanel, InItem, OnEnterPanel: false, ScrollToCaret);
		}

		public static void RefreshWindowsDesktop()
		{
			InvalidateRect(IntPtr.Zero, IntPtr.Zero, bErase: true);
		}

		public static void ClipboardCopyTextBox(RichTextBox InTextBox)
		{
			if (InTextBox.SelectedText != "")
			{
				Clipboard.SetDataObject(InTextBox.SelectedText);
			}
			else
			{
				Clipboard.SetDataObject("");
			}
		}

		public static void ClipboardPasteTextBox(RichTextBox InTextBox, int Location)
		{
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionStart = Location;
			InTextBox.Focus();
			InTextBox.Paste();
		}

		public static void ClipboardPasteTextBox(RichTextBox InTextBox, int Location, string InPasteString)
		{
			Clipboard.SetDataObject(InPasteString.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\v", "\n"));
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionStart = Location;
			InTextBox.Focus();
			InTextBox.Paste();
		}

		public static void InsertIndicator(ref RichTextBox InTextBox, int InNum)
		{
			int CurPos = InTextBox.SelectionStart;
			string selectedText = (InNum == 152) ? " 쨩" : VerseSymbol[InNum];
			string text = "";
			switch (InNum)
			{
				case 151:
					GetCurPosInLine(InTextBox.Text, ref CurPos);
					InTextBox.SelectionStart = CurPos;
					break;
				case 152:
					MoveToPosInLine(InTextBox.Text, ref CurPos, 1);
					InTextBox.SelectionStart = CurPos;
					break;
				default:
					GetCurPosInLine(InTextBox.Text, ref CurPos);
					InTextBox.SelectionStart = CurPos;
					text = (((DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\r") | (DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\n")) ? "" : "\r\n");
					break;
			}
			InTextBox.SelectedText = selectedText;
			if (text != "")
			{
				InTextBox.SelectedText = text;
			}
			if (InNum == 152)
			{
				InTextBox.SelectionStart -= 1;
			}
		}

		public static void LoadBlankCaptureDevices(ref ToolStripComboBox InComboBoxDevice, ref ToolStripComboBox InComboBoxMedium)
		{
			InComboBoxDevice.Items.Clear();
			for (int i = 1; i <= 10; i++)
			{
				InComboBoxDevice.Items.Add(i + ".");
			}
			InComboBoxMedium.Items.Clear();
			InComboBoxMedium.Items.Add("Video");
		}

		public static void LoadBlankCaptureDevices(ref ToolStripComboBox InComboBoxDevice)
		{
			InComboBoxDevice.Items.Clear();
			for (int i = 1; i <= 10; i++)
			{
				InComboBoxDevice.Items.Add(i + ".");
			}
		}

		public static void HighlightRichTextBox(ref RichTextBox[] InRichTextBox, ref Panel InPanel, SongSettings InItem, bool OnEnterPanel, bool ScrollToTop)
		{
			if (OnEnterPanel)
			{
				Control_Enter(InPanel);
			}
			else
			{
				Control_Leave(InPanel);
			}
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				if (InRichTextBox[i] == null)
				{
					continue;
				}
				if (OnEnterPanel)
				{
					Control_Enter(InRichTextBox[i]);
					if ((string)InRichTextBox[i].Tag == InItem.CurSlide.ToString() && !InItem.GapItemOnDisplay)
					{
						InRichTextBox[i].ForeColor = TextRegionSlideTextColour;
						InRichTextBox[i].BackColor = TextRegionSlideBackColour;
						int top = InRichTextBox[i].Top;
						int num = top;
						if (ScrollToTop)
						{
							bool flag = (top <= 0) ? true : false;
							while (!flag)
							{
								SendMessage(InPanel.Handle, 277u, 3u, 0u);
								top = InRichTextBox[i].Top;
								if (top < num && top > 0)
								{
									num = top;
								}
								else
								{
									flag = true;
								}
							}
						}
						InRichTextBox[i].Focus();
						InPanel.ScrollControlIntoView(InRichTextBox[i]);
						if (!ScrollToTop && i < InItem.TotalSlides)
						{
							top = InRichTextBox[i].Top;
							int num2 = 0;
							while (top > 5 && num2 < 5)
							{
								SendMessage(InPanel.Handle, 277u, 1u, 0u);
								top = InRichTextBox[i].Top;
								num2++;
							}
						}
					}
					else
					{
						InRichTextBox[i].ForeColor = NormalTextColour;
					}
				}
				else
				{
					Control_Leave(InRichTextBox[i]);
					if ((string)InRichTextBox[i].Tag == InItem.CurSlide.ToString())
					{
						InRichTextBox[i].ForeColor = TextRegionSlideTextColour;
						InRichTextBox[i].BackColor = TextRegionSlideBackColour;
					}
					else
					{
						InRichTextBox[i].ForeColor = NormalTextColour;
					}
				}
			}
		}

		public static void Control_Enter(Control InControl)
		{
			Color InBackground = InControl.BackColor;
			SetEnterColour(ref InBackground);
			InControl.BackColor = InBackground;
			if (InControl.Name == "Main_QuickFind")
			{
				((TextBox)InControl).SelectAll();
			}
		}

		public static void Control_Leave(Control InControl)
		{
			Color InBackground = InControl.BackColor;
			SetLeaveColor(ref InBackground);
			InControl.BackColor = InBackground;
		}

		public static void SetEnterColour(ref Color InBackground)
		{
			if (UseFocusedTextRegionColour)
			{
				InBackground = FocusedTextRegionColour;
			}
			else
			{
				InBackground = NormalTextRegionBackColour;
			}
		}

		public static void SetLeaveColor(ref Color InBackground)
		{
			if (InBackground != NormalTextRegionBackColour)
			{
				InBackground = NormalTextRegionBackColour;
			}
		}

		public static string ExtractDocTextContents(string InFileName)
		{
			string result = "";
			if (File.Exists(InFileName))
			{
				switch (Path.GetExtension(InFileName).ToLower())
				{
					case ".doc":
					case ".docx":
						result = GetOfficeDocContents(InFileName);
						break;
					case ".txt":
						result = LoadTextFile(InFileName);
						break;
				}
			}
			return result;
		}

		public static void LoadIndividualFormatData(ref SongSettings InItem, string InFormatString)
		{
			for (int i = 0; i < 255; i++)
			{
				InItem.Format.HeaderData[i] = ExtractHeaderInfo(InFormatString, i, '>');
			}
			int folderNo = InItem.FolderNo;
			int num = folderNo;
			int num2 = 1;
			if (InItem.Type == "B")
			{
				num = InItem.HBR2_FolderNo;
				num2 = 0;
			}
			if (folderNo >= 0)
			{
				InItem.FolderNo = folderNo;
				ShowFontBold[0, 0] = ShowFontBold[folderNo, 0];
				ShowFontBold[0, 1] = ShowFontBold[num, num2];
				ShowFontItalic[0, 0] = ShowFontItalic[folderNo, 0];
				ShowFontItalic[0, 1] = ShowFontItalic[num, num2];
				ShowFontUnderline[0, 0] = ShowFontUnderline[folderNo, 0];
				ShowFontUnderline[0, 1] = ShowFontUnderline[num, num2];
				ShowFontRTL[0, 0] = ShowFontRTL[folderNo, 0];
				ShowFontRTL[0, 1] = ShowFontRTL[num, num2];
				ShowFontBold[0, 2] = ShowFontBold[folderNo, 0];
				ShowFontBold[0, 3] = ShowFontBold[num, num2];
				ShowFontItalic[0, 2] = ShowFontItalic[folderNo, 2];
				ShowFontItalic[0, 3] = ShowFontItalic[num, 3];
				ShowFontUnderline[0, 2] = ShowFontUnderline[folderNo, 0];
				ShowFontUnderline[0, 3] = ShowFontUnderline[num, num2];
				ShowFontName[0, 0] = ShowFontName[folderNo, 0];
				ShowFontName[0, 1] = ShowFontName[num, num2];
				ShowFontSize[0, 0] = ShowFontSize[folderNo, 0] * InItem.FontSizeFactor / 100;
				ShowFontSize[0, 1] = ShowFontSize[num, num2] * ((InItem.Type == "B") ? InItem.HBR2_FontSizeFactor : InItem.FontSizeFactor) / 100;
				if (ShowFontSize[0, 0] <= 0)
				{
					ShowFontSize[0, 0] = 6;
				}
				if (ShowFontSize[0, 1] <= 0)
				{
					ShowFontSize[0, 1] = 6;
				}
				ShowBottomMargin[0] = ShowBottomMargin[folderNo];
				ShowLeftMargin[0] = ShowLeftMargin[folderNo];
				ShowRightMargin[0] = ShowRightMargin[folderNo];
				ShowFontVPosition[0, 0] = ShowFontVPosition[folderNo, 0];
				ShowFontVPosition[0, 1] = ShowFontVPosition[folderNo, 1];
				FolderHeadingFontBold[0, 0] = FolderHeadingFontBold[folderNo, 0];
				FolderHeadingFontItalic[0, 0] = FolderHeadingFontItalic[folderNo, 0];
				FolderHeadingFontUnderline[0, 0] = FolderHeadingFontUnderline[folderNo, 0];
				FolderHeadingFontBold[0, 1] = FolderHeadingFontBold[folderNo, 1];
				FolderHeadingFontItalic[0, 1] = FolderHeadingFontItalic[folderNo, 1];
				FolderHeadingFontUnderline[0, 1] = FolderHeadingFontUnderline[folderNo, 1];
				FolderHeadingPercentSize[0] = FolderHeadingPercentSize[folderNo];
				FolderHeadingOption[0] = FolderHeadingOption[folderNo];
				ShowLineSpacing[0, 0] = ShowLineSpacing[folderNo, 0];
				ShowLineSpacing[0, 1] = ShowLineSpacing[folderNo, 1];
			}
			int num3 = ExtractNumericData(InItem.Format.HeaderData[21]);
			InItem.Format.ShowSongHeadings = (((num3 < 0) | (num3 > 3)) ? ShowSongHeadings : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[22]);
			InItem.Format.UseShadowFont = ((num3 < 0) ? UseShadowFont : DataUtil.GetBitValue(num3, 2));
			InItem.Format.ShowNotations = ((num3 < 0) ? ShowNotations : DataUtil.GetBitValue(num3, 3));
			InItem.Format.ShowInterlace = ((num3 < 0) ? ShowInterlace : DataUtil.GetBitValue(num3, 5));
			InItem.Format.UseOutlineFont = ((num3 < 0) ? UseOutlineFont : DataUtil.GetBitValue(num3, 6));
			InItem.Format.HideDisplayPanel = ((num3 >= 0) ? DataUtil.GetBitValue(num3, 7) : 0);
			num3 = ExtractNumericData(InItem.Format.HeaderData[23]);
			InItem.Format.ShowSongHeadingsAlign = (((num3 < 0) | (num3 > 4)) ? ShowSongHeadingsAlign : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[25]);
			InItem.Format.ShowLyrics = (((num3 < 0) | (num3 > 2)) ? ShowLyrics : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[26]);
			InItem.Format.ShowScreenColour[0] = (((InItem.Format.HeaderData[26] == "") | !ValidateColour(num3)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[27]);
			InItem.Format.ShowScreenColour[1] = (((InItem.Format.HeaderData[27] == "") | !ValidateColour(num3)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[28]);
			InItem.Format.ShowScreenStyle = (((num3 < 0) | (num3 > MaxBackgroundStyleIndex)) ? ShowScreenStyle : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[29]);
			InItem.Format.ShowFontColour[0] = (((InItem.Format.HeaderData[29] == "") | !ValidateColour(num3)) ? ShowFontColour[0] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[30]);
			InItem.Format.ShowFontColour[1] = (((InItem.Format.HeaderData[30] == "") | !ValidateColour(num3)) ? ShowFontColour[1] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[31]);
			InItem.Format.ShowFontAlign[0] = (((num3 < 1) | (num3 > 3)) ? ShowFontAlign[0, 0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[32]);
			InItem.Format.ShowFontAlign[1] = (((num3 < 1) | (num3 > 3)) ? ShowFontAlign[0, 1] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[41]);
			InItem.Format.ShowFontBold[0] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 0] : DataUtil.GetBitValue(num3, 1));
			InItem.Format.ShowFontItalic[0] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 0] : DataUtil.GetBitValue(num3, 2));
			InItem.Format.ShowFontUnderline[0] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 0] : DataUtil.GetBitValue(num3, 3));
			InItem.Format.ShowFontBold[2] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 1] : DataUtil.GetBitValue(num3, 4));
			InItem.Format.ShowFontItalic[2] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 2] : DataUtil.GetBitValue(num3, 5));
			InItem.Format.ShowFontUnderline[2] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 1] : DataUtil.GetBitValue(num3, 6));
			num3 = ExtractNumericData(InItem.Format.HeaderData[42]);
			InItem.Format.ShowFontBold[1] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 1] : DataUtil.GetBitValue(num3, 1));
			InItem.Format.ShowFontItalic[1] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 1] : DataUtil.GetBitValue(num3, 2));
			InItem.Format.ShowFontUnderline[1] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 1] : DataUtil.GetBitValue(num3, 3));
			InItem.Format.ShowFontBold[3] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 3] : DataUtil.GetBitValue(num3, 4));
			InItem.Format.ShowFontItalic[3] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 3] : DataUtil.GetBitValue(num3, 5));
			InItem.Format.ShowFontUnderline[3] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 3] : DataUtil.GetBitValue(num3, 6));
			InItem.Format.ShowFontName[0] = ((InItem.Format.HeaderData[43] == "") ? ShowFontName[0, 0] : InItem.Format.HeaderData[43]);
			InItem.Format.ShowFontName[1] = ((InItem.Format.HeaderData[44] == "") ? ShowFontName[0, 1] : InItem.Format.HeaderData[44]);
			num3 = ExtractNumericData(InItem.Format.HeaderData[45]);
			InItem.Format.ShowFontVPosition[0] = (((num3 < 0) | (num3 > 100)) ? ShowFontVPosition[0, 0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[46]);
			InItem.Format.ShowFontVPosition[1] = (((num3 < InItem.Format.ShowFontVPosition[0]) | (num3 > 100)) ? ShowFontVPosition[0, 1] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[47]);
			InItem.Format.ShowFontSize[0] = (((num3 < 6) | (num3 > 100)) ? ShowFontSize[0, 0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[48]);
			InItem.Format.ShowFontSize[1] = (((num3 < 6) | (num3 > 100)) ? ShowFontSize[0, 1] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[71]);
			InItem.Format.TransposeOffset = ((!((num3 < 0) | (num3 > 11))) ? num3 : 0);
			InItem.Format.PreviousTransposeOffset = InItem.Format.TransposeOffset;
			InItem.Format.BackgroundPicture = InItem.Format.HeaderData[61];
			num3 = ExtractNumericData(InItem.Format.HeaderData[50]);
			InItem.Format.MediaOption = (((num3 < 0) | (num3 > 3)) ? MediaOption : num3);
			InItem.Format.MediaLocation = InItem.Format.HeaderData[51];
			num3 = ExtractNumericData(InItem.Format.HeaderData[52]);
			InItem.Format.MediaVolume = (((num3 < 0) | (num3 > 100)) ? MediaVolume : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[53]);
			InItem.Format.MediaBalance = (((num3 < -100) | (num3 > 100)) ? MediaBalance : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[54]);
			InItem.Format.MediaMute = ((num3 < 0) ? MediaMute : DataUtil.GetBitValue(num3, 1));
			InItem.Format.MediaRepeat = ((num3 < 0) ? MediaRepeat : DataUtil.GetBitValue(num3, 2));
			InItem.Format.MediaWidescreen = ((num3 < 0) ? MediaWidescreen : DataUtil.GetBitValue(num3, 3));
			if (InItem.Type == "M")
			{
				InItem.Format.ShowSongHeadings = 0;
				InItem.Format.MediaOption = 2;
				InItem.Format.MediaLocation = InItem.ItemID;
			}
			num3 = ExtractNumericData(InItem.Format.HeaderData[55]);
			InItem.Format.MediaCaptureDeviceNumber = (((num3 < 1) | (num3 > 5)) ? 1 : num3);
			InItem.Format.MediaOutputMonitorName = InItem.Format.HeaderData[56];
			num3 = ExtractNumericData(InItem.Format.HeaderData[62]);
			InItem.Format.BackgroundMode = (ImageMode)(((num3 < 0) | (num3 > 2)) ? ((int)BackgroundMode) : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[63]);
			InItem.Format.ShowVerticalAlign = (((num3 < 0) | (num3 > 2)) ? ShowVerticalAlign : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[64]);
			InItem.Format.ShowLeftMargin = (((num3 < 0) | (num3 > 40)) ? ShowLeftMargin[0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[65]);
			InItem.Format.ShowRightMargin = (((num3 < 0) | (num3 > 40)) ? ShowRightMargin[0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[66]);
			InItem.Format.ShowBottomMargin = (((num3 < 0) | (num3 > 100)) ? ShowBottomMargin[0] : num3);
			if (InItem.Format.HeaderData[72] != "")
			{
				InItem.Format.ShowItemTransition = GlobalImageCanvas.GetTransitionType(InItem.Format.HeaderData[72]);
			}
			else
			{
				InItem.Format.ShowItemTransition = ShowItemTransition;
			}
			if (InItem.Format.HeaderData[73] != "")
			{
				InItem.Format.ShowSlideTransition = GlobalImageCanvas.GetTransitionType(InItem.Format.HeaderData[73]);
			}
			else
			{
				InItem.Format.ShowSlideTransition = ShowSlideTransition;
			}
			InItem.Format.FormatString = InFormatString;
			InItem.UseDefaultFormat = ((InFormatString == "") ? true : false);
			if (InItem.Format.ImageString != "" && InItem.Format.BackgroundPicture != "")
			{
				InItem.Format.TempImageFileName = dumpImageToFile(Convert.FromBase64String(InItem.Format.ImageString), InItem.Format.BackgroundPicture);
			}
		}

    }
}
