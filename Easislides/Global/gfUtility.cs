using Easislides.SQLite;
using Easislides.Util;
using Microsoft.Win32;
using OfficeLib;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;


namespace Easislides
{
	internal unsafe partial class gf
	{

		public static void LoadEulaText()
		{
			EULA = "EasiSlides Software\n\nIMPORTANT: This software end user licence agreement ('EULA') is a legal agreement between you and EasiSlides. Read it carefully before completing the installation process and using the software. It provides a licence to use the software. By installing and using the software, you are confirming your acceptance of the software and agreeing to become bound by the terms of this agreement. If you do not agree with the terms of this licence you must remove EasiSlides Software files from your storage devices and cease to use the product.\n\nAll copyrights to 'EasiSlides Software', hereafter shall be referred to as 'the software', are exclusively owned by EasiSlides. Your licence confers no title or ownership in the software and should not be construed as a sale of any right in the software.\n\nYou MUST NOT use this software for purposes which are unlawful, including, but not limited to, the transmission of obscene or offensive content, or contents which may harass or cause distress to any person.\n\nYou may use this software for any length of time.\n\nYou are hereby licenced to make any number of backup copies of this software and documentation. You can give the copy of the software to anyone or distribute the software provided you abide by the following Licence restrictions:\n(a) You may not reproduce or distribute the software for the purpose of promoting other non-EasiSlides products or organisations unless specific permission to do so have been obtained from the EasiSlides Copyright holder.\n(b) You may not alter, merge, modify, adapt or translate the software, or decompile, reverse engineer, disassemble, or otherwise reduce the Software to a human-perceivable form.\n(c) You may not rent, lease, or sublicence the Software.\n(d) Where the software is placed on a network for distribution, you must place alongside the distributed software a fully functional and visible hyperlink to the official EasiSlides website at http://www.EasiSlides.com.\n(e) No fee is charged for the software.\n\nEASISLIDES SOFTWARE IS DISTRIBUTED 'AS IS'. NO WARRANTY OF ANY KIND IS EXPRESSED OR IMPLIED. YOU USE IT AT YOUR OWN RISK. EASISLIDES WILL NOT BE LIABLE FOR DATA LOSS, DAMAGES, LOSS OF PROFITS OR ANY OTHER KIND OF LOSS WHILE USING OR MISUSING THIS SOFTWARE.\n\nThis EULA  shall be governed by and construed in accordance with the laws of Northern Ireland. Any dispute arising under this EULA shall be subject to the exclusive jurisdiction of the courts of Northern Ireland.\n\nCopyright ï¿?2007 EasiSlides, All rights reserved.\nInternet:  http://www.EasiSlides.com\n";
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
			using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

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
			// Global Hook F7, F8 (panel black) ??ï¿½ì??
			RegUtil.SaveRegValue("options", "GlobalHookKey_F7", GlobalHookKey_F7 ? 1 : 0);
			RegUtil.SaveRegValue("options", "GlobalHookKey_F8", GlobalHookKey_F8 ? 1 : 0);

			// Global Hook F9, F10 (panel black) ??ï¿½ì??
			RegUtil.SaveRegValue("options", "GlobalHookKey_F9", GlobalHookKey_F9 ? 1 : 0);
			RegUtil.SaveRegValue("options", "GlobalHookKey_F10", GlobalHookKey_F10 ? 1 : 0);

			// Global Hook Arrow, CtrlArrow (panel black) ??ï¿½ì??
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
			// ï§â¤???Wide ??ï¿½ï¿½?ê²å ?Mode
			RegUtil.SaveRegValue("monitors", "IsMonitorWide", isScreenWideMode ? 1 : 0);
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

				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

				if (datatable.Rows.Count > 0)
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

					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

					if (datatable.Rows.Count > 0)
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
				InItem.Title = /* The above code is a comment in C# programming language. It is not performing any specific action in the code, but it is used to provide information or explanations about the code for other developers who may read it. In this case, the comment is indicating that the code is written in C# and the function or method being referenced is "GetDisplayNameOnly". */
				GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
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
				InItem.CompleteLyrics = gfFileHelpers.LoadTextFile(InFileName);
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "I")
			{
				InItem.Type = "I";
				string[] ThisHeaderData = new string[255];
				gfFileHelpers.LoadInfoFile(InFileName, ref InItem, ref ThisHeaderData);
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

		public void LoadIndividualFormatData(ref SongSettings InItem)
		{
			LoadIndividualFormatData(ref InItem, "");
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
					ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
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
					ListViewItem item = InItem.LyricsAndNotationsList.Items[i];
					text2 = item.SubItems[2].Text;
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref text2);
					}
					text = text + text2 + "\n";
					text3 = item.SubItems[4].Text;
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
				ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
				bool OnlyOneDisplayLine = false;
				num10 = IncreaseFontToLargest(g, text, ref MainFont, fS_Width, num4, ref OnlyOneDisplayLine);
				ReplaceFont(ref NotationsFont, new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style));
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
				ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
				ReplaceFont(ref NotationsFont, new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style));
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
				return InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight * num5;
			}
			ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
			ReduceFontToFit(g, text, ref MainFont, fS_Width, num4, MultiLine: true);
			num10 = num4;
			ReplaceFont(ref NotationsFont, new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style));
			InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
			return (int)((double)num10 * num7);
		}

		private static void ReplaceFont(ref Font target, Font replacement)
		{
			if (target != null && !ReferenceEquals(target, replacement))
			{
				target.Dispose();
			}
			target = replacement;
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

					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

					if (datatable.Rows.Count > 0 && DataUtil.GetDataInt(datatable.Rows[0], "FolderNo") > 0 && FolderUse[DataUtil.GetDataInt(datatable.Rows[0], "FolderNo")] > 0)
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
			gfFileHelpers.LoadInfoFile(InFileName, ref TempItem1, ref tempHeaderData);
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

		public static string BuildItemSearchString(string InString)
		{
			return BuildItemSearchString(InString, SearchTitle: true, SearchContents: true, SearchSongNumber: true, SearchBookRef: true, SearchUserRef: true, SearchLicAdmin: true, SearchWriter: true, SearchCopyright: true, SearchNotationsOnly: false, "", "", SearchDates: false, DateTime.Now, DateTime.Now, null);
		}

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

	



		public static string LookupDBTitle2(int InKey)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + Convert.ToString(InKey);

				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

				if (datatable.Rows.Count > 0)
				{
					return DataUtil.ObjToString(datatable.Rows[0]["Title_2"]);
				}
			}
			catch
			{
			}
			return "";
		}
}

    }
