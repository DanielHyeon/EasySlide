using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Axicultus.Global;
using Axicultus.Localization;
using Axicultus.Module;
using Axicultus.SQLite;
using Axicultus.Util;
using OfficeLib;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;


namespace Axicultus;

	internal unsafe partial class Gf
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
			string selectedItemTag = "";
			string selectedToolTip = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				selectedItemTag = (string)InMenuItem2.Tag;
				selectedToolTip = InMenuItem2.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				selectedItemTag = (string)InMenuItem1.Tag;
				selectedToolTip = InMenuItem1.Text;
			}
			SelectedBtn.Tag = selectedItemTag;
			SelectedBtn.ToolTipText = selectedToolTip;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3)
		{
			string selectedItemTag = "";
			string selectedToolTip = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				selectedItemTag = (string)InMenuItem2.Tag;
				selectedToolTip = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				selectedItemTag = (string)InMenuItem3.Tag;
				selectedToolTip = InMenuItem3.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				selectedItemTag = (string)InMenuItem1.Tag;
				selectedToolTip = InMenuItem1.Text;
			}
			SelectedBtn.Tag = selectedItemTag;
			SelectedBtn.ToolTipText = selectedToolTip;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3, ToolStripMenuItem InMenuItem4)
		{
			string selectedItemTag = "";
			string selectedToolTip = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				selectedItemTag = (string)InMenuItem2.Tag;
				selectedToolTip = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				selectedItemTag = (string)InMenuItem3.Tag;
				selectedToolTip = InMenuItem3.Text;
			}
			else if (SelectedMenuItemName == InMenuItem4.Name)
			{
				SelectedBtn.Image = InMenuItem4.Image;
				selectedItemTag = (string)InMenuItem4.Tag;
				selectedToolTip = InMenuItem4.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				selectedItemTag = (string)InMenuItem1.Tag;
				selectedToolTip = InMenuItem1.Text;
			}
			SelectedBtn.Tag = selectedItemTag;
			SelectedBtn.ToolTipText = selectedToolTip;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3, ToolStripMenuItem InMenuItem4, ToolStripMenuItem InMenuItem5)
		{
			string selectedItemTag = "";
			string selectedToolTip = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				selectedItemTag = (string)InMenuItem2.Tag;
				selectedToolTip = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				selectedItemTag = (string)InMenuItem3.Tag;
				selectedToolTip = InMenuItem3.Text;
			}
			else if (SelectedMenuItemName == InMenuItem4.Name)
			{
				SelectedBtn.Image = InMenuItem4.Image;
				selectedItemTag = (string)InMenuItem4.Tag;
				selectedToolTip = InMenuItem4.Text;
			}
			else if (SelectedMenuItemName == InMenuItem5.Name)
			{
				SelectedBtn.Image = InMenuItem5.Image;
				selectedItemTag = (string)InMenuItem5.Tag;
				selectedToolTip = InMenuItem5.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				selectedItemTag = (string)InMenuItem1.Tag;
				selectedToolTip = InMenuItem1.Text;
			}
			SelectedBtn.Tag = selectedItemTag;
			SelectedBtn.ToolTipText = selectedToolTip;
		}

	
		public static void LoadLicAdminDetails()
		{
			LicAdminEnforceDisplay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("config", "licEnforceDisplay", 1)) > 0) ? true : false);
			LicAdminNoSymbol = RegUtil.GetRegValue("config", "licNoSym", "#");
			LicenseAdministrators[0].Name = "1";
			LicenseAdministrators[1].Name = "None";
			LicenseAdministrators[2].Name = "Public Domain";
			LicenseAdministrators[3].Name = "CCLI";
			for (int administratorIndex = 4; administratorIndex < LicenseAdministrators.Length; administratorIndex++)
			{
				LicenseAdministrators[administratorIndex].Name = "";
			}
			string fullSearchString = $"select * from LICENCE where Ref >=4 and Ref < {LicenseAdministrators.Length}";
			using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);

			foreach (DataRow row in datatable.Rows)
			{
				int administratorIndex = DataUtil.GetDataInt(row, "Ref");
				LicenseAdministrators[administratorIndex].Name = DataUtil.Trim(DataUtil.GetDataString(row, "ADMINISTRATOR"));
			}
			LicenseAdministrators[1].Number = "";
			LicenseAdministrators[2].Number = "Public Domain";
			LicenseAdministrators[3].Number = DataUtil.Trim(RegUtil.GetRegValue("config", "licCCLI_no", ""));
			LicenseAdministrators[4].Number = DataUtil.Trim(RegUtil.GetRegValue("config", "lic4_no", ""));
			LicenseAdministrators[5].Number = DataUtil.Trim(RegUtil.GetRegValue("config", "lic5_no", ""));
			LicenseAdministrators[6].Number = DataUtil.Trim(RegUtil.GetRegValue("config", "lic6_no", ""));
			LicenseAdministrators[7].Number = DataUtil.Trim(RegUtil.GetRegValue("config", "lic7_no", ""));
			LicenseAdministrators[8].Number = DataUtil.Trim(RegUtil.GetRegValue("config", "lic8_no", ""));
			LicenseAdministrators[1].DisplayText = "";
			LicenseAdministrators[2].DisplayText = "Public Domain";
			for (int administratorIndex = 3; administratorIndex < LicenseAdministrators.Length; administratorIndex++)
			{
				ref var administrator = ref LicenseAdministrators[administratorIndex];
				administrator.DisplayText = string.IsNullOrEmpty(administrator.Name) || string.IsNullOrEmpty(administrator.Number)
					? "" : administrator.Name + LicAdminNoSymbol + administrator.Number;
			}
			LicenseAdministrators[0].Number = LicenseAdministrators[1].Number;
			LicenseAdministrators[0].DisplayText = LicenseAdministrators[1].DisplayText;
		}

		public static void SaveFoldersSettings4()
		{
			int currentFolderIndex = 0;
			if (ValidateDefaultFolders(0))
			{
				try
				{
					string sQuery = "select * from Folder";

					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
					using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

					DataColumn[] primarykey = [dataTable.Columns["FolderNo"]];
					dataTable.PrimaryKey = primarykey;

					for (int i = 1; i < MAXSONGSFOLDERS; i++)
					{
						currentFolderIndex = i;
						if (Folders[i].Name != "")
						{
							DataRow dr = dataTable.Rows.Find($"{i}");
							if (dr != null)
							{
								dr["name"] = Folders[i].Name;
								dr["Use"] = Folders[i].Use;
								dr["GroupStyle"] = Folders[i].GroupStyle;
								dr["PreChorusHeading"] = Folders[i].LyricsHeading[0];
								dr["ChorusHeading"] = Folders[i].LyricsHeading[1];
								dr["BridgeHeading"] = Folders[i].LyricsHeading[2];
								dr["EndingHeading"] = Folders[i].LyricsHeading[3];
								dr["BIUHeading"] = Folders[i].HeadingFontBold[0] + Folders[i].HeadingFontItalic[0] * 2 + Folders[i].HeadingFontUnderline[0] * 4 + Folders[i].HeadingFontBold[1] * 8 + Folders[i].HeadingFontItalic[1] * 16 + Folders[i].HeadingFontUnderline[1] * 32;
								dr["HeadingSize"] = Folders[i].HeadingPercentSize;
								dr["HeadingOption"] = Folders[i].HeadingOption;
								dr["LineSpacing"] = Folders[i].LineSpacing[0];
								dr["LineSpacing2"] = Folders[i].LineSpacing[1];
								dr["BIU0"] = Folders[i].FontBold[0] + Folders[i].FontItalic[0] * 2 + Folders[i].FontUnderline[0] * 4 + Folders[i].FontBold[2] * 8 + Folders[i].FontItalic[2] * 16 + Folders[i].FontUnderline[2] * 32 + Folders[i].FontRTL[0] * 64;
								dr["Size0"] = Folders[i].FontSize[0];
								dr["FontName0"] = Folders[i].FontName[0];
								dr["Vpos0"] = Folders[i].FontVPosition[0];
								dr["BIU1"] = Folders[i].FontBold[1] + Folders[i].FontItalic[1] * 2 + Folders[i].FontUnderline[1] * 4 + Folders[i].FontBold[3] * 8 + Folders[i].FontItalic[3] * 16 + Folders[i].FontUnderline[3] * 32 + Folders[i].FontRTL[1] * 64;
								dr["Size1"] = Folders[i].FontSize[1];
								dr["FontName1"] = Folders[i].FontName[1];
								dr["Vpos1"] = Folders[i].FontVPosition[1];
								dr["LMargin"] = Folders[i].LeftMargin;
								dr["RMargin"] = Folders[i].RightMargin;
								dr["BMargin"] = Folders[i].BottomMargin;
							}

						}
					}

					DbController.UpdateTable(connection, sQuery, dataTable);
				}
				catch
				{
					MessageBox.Show(UiText.Format("gfUtility", "SaveFolderSettingsFailed", currentFolderIndex));
				}
			}
		}

		public static void SaveFoldersSettings()
		{
			int currentFolderIndex = 0;
			int rowIndex = 0;

			if (ValidateDefaultFolders(0))
			{
				try
				{
					string folderQuery = "";
					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);

					DbDataAdapter sQLiteDataAdapter = null;
					DataTable dataTable = null;

					folderQuery = "select * from Folder where FolderNo > 0 and FolderNo <= " + MAXSONGSFOLDERS;
					(sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, folderQuery);

					if (dataTable.Rows.Count > 0)
					{
						for (int i = 1; i < MAXSONGSFOLDERS; i++)
						{
							currentFolderIndex = i;
							rowIndex = i - 1;
							if (Folders[i].Name != "")
							{
								dataTable.Rows[rowIndex]["name"] = Folders[i].Name;
								dataTable.Rows[rowIndex]["Use"] = Folders[i].Use;
								dataTable.Rows[rowIndex]["GroupStyle"] = Folders[i].GroupStyle;
								dataTable.Rows[rowIndex]["PreChorusHeading"] = Folders[i].LyricsHeading[0];
								dataTable.Rows[rowIndex]["ChorusHeading"] = Folders[i].LyricsHeading[1];
								dataTable.Rows[rowIndex]["BridgeHeading"] = Folders[i].LyricsHeading[2];
								dataTable.Rows[rowIndex]["EndingHeading"] = Folders[i].LyricsHeading[3];
								dataTable.Rows[rowIndex]["BIUHeading"] = Folders[i].HeadingFontBold[0] + Folders[i].HeadingFontItalic[0] * 2 + Folders[i].HeadingFontUnderline[0] * 4 + Folders[i].HeadingFontBold[1] * 8 + Folders[i].HeadingFontItalic[1] * 16 + Folders[i].HeadingFontUnderline[1] * 32;
								dataTable.Rows[rowIndex]["HeadingSize"] = Folders[i].HeadingPercentSize;
								dataTable.Rows[rowIndex]["HeadingOption"] = Folders[i].HeadingOption;
								dataTable.Rows[rowIndex]["LineSpacing"] = Folders[i].LineSpacing[0];
								dataTable.Rows[rowIndex]["LineSpacing2"] = Folders[i].LineSpacing[1];
								dataTable.Rows[rowIndex]["BIU0"] = Folders[i].FontBold[0] + Folders[i].FontItalic[0] * 2 + Folders[i].FontUnderline[0] * 4 + Folders[i].FontBold[2] * 8 + Folders[i].FontItalic[2] * 16 + Folders[i].FontUnderline[2] * 32 + Folders[i].FontRTL[0] * 64;
								dataTable.Rows[rowIndex]["Size0"] = Folders[i].FontSize[0];
								dataTable.Rows[rowIndex]["FontName0"] = Folders[i].FontName[0];
								dataTable.Rows[rowIndex]["Vpos0"] = Folders[i].FontVPosition[0];
								dataTable.Rows[rowIndex]["BIU1"] = Folders[i].FontBold[1] + Folders[i].FontItalic[1] * 2 + Folders[i].FontUnderline[1] * 4 + Folders[i].FontBold[3] * 8 + Folders[i].FontItalic[3] * 16 + Folders[i].FontUnderline[3] * 32 + Folders[i].FontRTL[1] * 64;
								dataTable.Rows[rowIndex]["Size1"] = Folders[i].FontSize[1];
								dataTable.Rows[rowIndex]["FontName1"] = Folders[i].FontName[1];
								dataTable.Rows[rowIndex]["Vpos1"] = Folders[i].FontVPosition[1];
								dataTable.Rows[rowIndex]["LMargin"] = Folders[i].LeftMargin;
								dataTable.Rows[rowIndex]["RMargin"] = Folders[i].RightMargin;
								dataTable.Rows[rowIndex]["BMargin"] = Folders[i].BottomMargin;
							}
						}
						DbController.UpdateTable(connection, folderQuery, dataTable);
					}

					dataTable?.Dispose();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		public static void SaveOptionsData() => RegUtil.UpdateProfile(SaveOptionsValues);

		private static void SaveOptionsValues()
		{
			RegUtil.SaveRegValue("options", "PrinterSpaces", PB_PrinterSpaces);
			RegUtil.SaveRegValue("options", "UseSongNumbers", UseSongNumbers ? 1 : 0);
			RegUtil.SaveRegValue("options", "BibleMaxSelectVerses", HB_MaxVersesSelection);
			RegUtil.SaveRegValue("options", "BibleMaxAdhocVersesSelection", HB_MaxAdhocVersesSelection);
			RegUtil.SaveRegValue("options", "BibleShowVerses", 1);
			RegUtil.SaveRegValue("options", "PowerpointMaxFiles", PP_MaxFiles);
			RegUtil.SaveRegValue("options", "AutoTextOverflow", AutoTextOverflow ? 1 : 0);
			RegUtil.SaveRegValue("options", RegUtil.Reg_FitTextFileParagraphs, FitTextFileParagraphs ? 1 : 0);
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
			RegUtil.SaveRegValue("options", "PowerPointSortByTitle", PowerPointSourceOrder == PowerPointSortOrder.Title ? 1 : 0);
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
			int alertStyleFlags = 0;
			alertStyleFlags = (ParentalAlertScroll ? 1 : 0) + (ParentalAlertFlash ? 1 : 0) * 2 + (ParentalAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "ParentalAlertStyle", alertStyleFlags);
			RegUtil.SaveRegValue("options", "ParentalAlertFontName", ParentalAlertFontName);
			RegUtil.SaveRegValue("options", "ParentalAlertFontSize", ParentalAlertFontSize);
			RegUtil.SaveRegValue("options", "ParentalAlertFontFormat", ParentalAlertFontFormat);
			RegUtil.SaveRegValue("options", "MessageAlertDuration", MessageAlertDuration);
			RegUtil.SaveRegValue("options", "MessageAlertTextAlign", MessageAlertTextAlign);
			RegUtil.SaveRegValue("options", "MessageAlertVerticalAlign", MessageAlertVerticalAlign);
			RegUtil.SaveRegValue("options", "MessageAlertTextColour", MessageAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "MessageAlertBackColour", MessageAlertBackColour.ToArgb());
			alertStyleFlags = (MessageAlertScroll ? 1 : 0) + (MessageAlertFlash ? 1 : 0) * 2 + (MessageAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "MessageAlertStyle", alertStyleFlags);
			RegUtil.SaveRegValue("options", "MessageAlertFontName", MessageAlertFontName);
			RegUtil.SaveRegValue("options", "MessageAlertFontSize", MessageAlertFontSize);
			RegUtil.SaveRegValue("options", "MessageAlertFontFormat", MessageAlertFontFormat);
			RegUtil.SaveRegValue("options", "ReferenceAlertDuration", ReferenceAlertDuration);
			RegUtil.SaveRegValue("options", "ReferenceAlertTextAlign", ReferenceAlertTextAlign);
			RegUtil.SaveRegValue("options", "ReferenceAlertVerticalAlign", ReferenceAlertVerticalAlign);
			RegUtil.SaveRegValue("options", "ReferenceAlertTextColour", ReferenceAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "ReferenceAlertBackColour", ReferenceAlertBackColour.ToArgb());
			alertStyleFlags = (ReferenceAlertScroll ? 1 : 0) + (ReferenceAlertFlash ? 1 : 0) * 2 + (ReferenceAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "ReferenceAlertStyle", alertStyleFlags);
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
			RegUtil.SaveRegValue("options", "FindItemDateFrom", DataUtil.ToStoredDate(FindItemDateFrom));
			RegUtil.SaveRegValue("options", "FindItemDateTo", DataUtil.ToStoredDate(FindItemDateTo));
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
			MonitorRegistrySettings.SaveNotationsFontSize(RegUtil.Settings, LMNotationsFontSize);
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
		for (int i = 0; i <= 11; i++)
		{
			SongCapo.Items.Add($"Capo {i}");
		}

		string[] keys = ["", "A", "B", "C", "D", "E", "F", "G", "Am", "Bm", "Cm", "Dm", "Em", "Fm", "Gm", "Ab", "Bb", "Db", "Eb", "F#", "Bbm", "C#m", "D#m", "F#m", "G#m"];
		SongKey.Items.Clear();
		SongKey.Items.AddRange(keys);

		string[] timings = ["", "3/4", "4/4"];
		SongTiming.Items.Clear();
		SongTiming.Items.AddRange(timings);
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
			catch (Exception ex)
			{
				Console.WriteLine($"ExtractNumericData Exception: {ex.Message}");
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
					MessageBox.Show(UiText.Format("gfUtility", "SelectSongToEdit", InString));
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
				disposable?.Dispose();
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
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, "select FORMATDATA from SONG where songid=@id", ("@id", InItem.ItemID));

				if (datatable.Rows.Count > 0)
				{
					InItem.Format.DBStoredFormat = DataUtil.GetDataString(datatable.Rows[0], "FORMATDATA");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"LoadDBFormatString Exception: {ex.Message}");
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
					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, "select * from SONG where songid=@id", ("@id", InItem.ItemID));

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
				catch (Exception ex)
				{
					Console.WriteLine($"LoadIndividualData Type D Exception: {ex.Message}");
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
				int primaryVersionIndex = LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InString, ';'));
				int secondaryVersionIndex = LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InString, ';'));
				InItem.Type = "B";
				if (primaryVersionIndex >= 0)
				{
					InItem.Type = "B";
					InItem.FolderNo = BibleVersions[primaryVersionIndex].SongFolder;
					InItem.FontSizeFactor = BibleVersions[primaryVersionIndex].FontSize;
					InItem.CompleteLyrics = LoadSelectedBibleVerses(InItem.ItemID);
					InItem.Copyright = BibleVersions[primaryVersionIndex].Copyright;
					InItem.Show_BookName = ((DataUtil.Left(InItem.ItemID, 1) == "1") ? true : false);
					int versionSuffixStart = InTitle.IndexOf('(');
					if (versionSuffixStart > 0)
					{
						InTitle = DataUtil.Trim(DataUtil.Left(InTitle, versionSuffixStart - 1));
					}
					if (secondaryVersionIndex < 0)
					{
						InItem.Title = InTitle + " (" + BibleVersions[primaryVersionIndex].Name + ")";
						InItem.HBR2_FolderNo = InItem.FolderNo;
						InItem.HBR2_FontSizeFactor = InItem.FontSizeFactor;
					}
					else
					{
						InItem.Title = InTitle + " (" + BibleVersions[primaryVersionIndex].Name + "/" + BibleVersions[secondaryVersionIndex].Name + ")";
						InItem.HBR2_FolderNo = BibleVersions[secondaryVersionIndex].SongFolder;
						InItem.HBR2_FontSizeFactor = BibleVersions[secondaryVersionIndex].FontSize;
						SongSettings obj2 = InItem;
						obj2.Copyright = obj2.Copyright + "/" + BibleVersions[secondaryVersionIndex].Copyright;
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
				gfFileHelpers.LoadInfoFile(InFileName, ref InItem);
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

		internal static void PopulateLicenseAdministratorChoices(ComboBox primary, ComboBox secondary)
		{
			primary.Items.Clear();
			secondary.Items.Clear();
			primary.Items.Add("");
			secondary.Items.Add("");
			for (int index = 2; index < LicenseAdministrators.Length; index++)
			{
				string name = LicenseAdministrators[index].Name;
				if (!string.IsNullOrEmpty(name))
				{
					primary.Items.Add(name);
					secondary.Items.Add(name);
				}
			}
		}

		public static void LoadLicAdminDisplayInfo(ref string AdminInfo1, ref string AdminInfo2)
		{
			string primary = FindLicenseDisplayText(AdminInfo1);
			string secondary = FindLicenseDisplayText(AdminInfo2);
			if (!string.IsNullOrEmpty(primary))
			{
				AdminInfo1 = primary;
			}
			if (primary != secondary)
			{
				AdminInfo2 = secondary;
			}
		}

		private static string FindLicenseDisplayText(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return LicenseAdministrators[0].DisplayText ?? "";
			}
			foreach (ref readonly var administrator in LicenseAdministrators.AsSpan(2))
			{
				if (administrator.Name == name)
				{
					return administrator.DisplayText;
				}
			}
			return name;
		}

		public static void LoadIndividualFormatData(ref SongSettings InItem)
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
			string backgroundImagePath = "";
			if (InItem.UseDefaultFormat)
			{
				if (File.Exists(BackgroundPicture))
				{
					backgroundImagePath = BackgroundPicture;
				}
				picMode = BackgroundMode;
			}
			else
			{
				string itemImagePath = (InItem.Type == "I") ? InItem.Format.TempImageFileName : InItem.Format.BackgroundPicture;
				if (File.Exists(itemImagePath))
				{
					backgroundImagePath = itemImagePath;
					picMode = InItem.Format.BackgroundMode;
				}
				else if (InItem.Type == "I" && InItem.Format.TempImageFileName == "")
				{
					InItem.Format.ShowScreenColour[0] = ShowScreenColour[0].ToArgb();
					InItem.Format.ShowScreenColour[1] = ShowScreenColour[1].ToArgb();
					InItem.Format.ShowScreenStyle = ShowScreenStyle;
				}
			}
			if (backgroundImagePath == "")
			{
				SetColoursFormat(InItem, ref InPic);
			}
			else
			{
				SetImageFormat(ref InPic, backgroundImagePath, picMode, SetTransparent: false);
			}
		}

		public static void SetColoursFormat(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetColoursFormat(InItem, ref InPic, SetTransparent: false);
		}

		/// <summary>
		/// A new background picture of the output size: transparent, or the item's colour pattern (its
		/// background ID in <paramref name="patternID"/>). If drawing it throws, the picture and its Graphics
		/// are disposed and the exception goes on to the caller, as before.
		/// </summary>
		private static Image NewBackgroundBitmap(SongSettings InItem, bool Transparent, out string patternID)
		{
			patternID = "";
			Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
			try
			{
				Graphics g = Graphics.FromImage(image);
				try
				{
					if (Transparent)
					{
						Patterns.Clear(ref g, TransparentColour);
					}
					else
					{
						// SongFormat colours are ARGB ints (main); BackPattern.Fill takes Color.
					BackPattern.Fill(ref g, Color.FromArgb(InItem.Format.ShowScreenColour[0]), Color.FromArgb(InItem.Format.ShowScreenColour[1]), InItem.Format.ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref patternID);
					}
				}
				finally
				{
					g.Dispose();
				}
			}
			catch
			{
				image.Dispose();
				throw;
			}
			return image;
		}

		public static void SetColoursFormat(SongSettings InItem, ref ImageTransitionControl InPic, bool SetTransparent)
		{
			// Background pictures made here are made for InPic alone and become its own when they are stored
			// (AdoptImage): the screen disposes each once none of its fields refers to it. No picture is copied:
			// nothing draws into a background picture after it is made, so the previous NewBackgroundPicture
			// itself becomes CurrentBackgroundPicture (it was copied each time, and the copies were left to the
			// finalizer).
			if (SetTransparent)
			{
				Image image = NewBackgroundBitmap(InItem, Transparent: true, out _);
				InPic.AdoptImage(image);
				InPic.NewBackgroundPicture = image;
				InPic.NewTextImage = InPic.NewBackgroundPicture;
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				InPic.CurrentBackgroundPicture = InPic.NewBackgroundPicture;

			}
			else if (InItem.UseDefaultFormat)
			{
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = InPic.NewBackgroundPicture;
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
				Image image = NewBackgroundBitmap(InItem, Transparent: false, out string backgroundId);
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = InPic.NewBackgroundPicture;
				}
				else
				{
					InPic.CurrentBackgroundPicture = InPic.BackgroundImage;
					InPic.CurrentTextImage = InPic.BackgroundImage;
					InPic.CurrentCombinedImage = InPic.BackgroundImage;
					InPic.NewTextImage = InPic.BackgroundImage;
				}
				InPic.AdoptImage(image);
				InPic.NewBackgroundPicture = image;
				if (InPic.BackgroundID == backgroundId)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				}
				else if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.NewOnly;
				}
				else if (backgroundId == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.CurrentOnly;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
				}
				InPic.BackgroundID = backgroundId;
			}
			try
			{
				if (InPic.CurrentBackgroundPicture == null)
				{
					InPic.CurrentBackgroundPicture = InPic.AdoptImage((Image)InPic.NewBackgroundPicture.Clone());
				}
				if (InPic.CurrentTextImage == null)
				{
					InPic.CurrentTextImage = InPic.AdoptImage((Image)InPic.NewBackgroundPicture.Clone());
				}
				if (InPic.CurrentCombinedImage == null)
				{
					InPic.CurrentCombinedImage = InPic.AdoptImage((Image)InPic.NewBackgroundPicture.Clone());
				}
				if (InPic.NewTextImage == null)
				{
					InPic.NewTextImage = InPic.AdoptImage((Image)InPic.NewBackgroundPicture.Clone());
				}
				if (InPic.NewCombinedImage == null)
				{
					InPic.NewCombinedImage = InPic.AdoptImage((Image)InPic.NewBackgroundPicture.Clone());
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SetColoursFormat Exception: {ex.Message}");
			}
		}

		public static void SetImageFormat(ref ImageTransitionControl InPic, string File_Name, ImageMode PicMode, bool SetTransparent)
		{
			if (InPic.Width <= 0 || InPic.Height <= 0)
				return;

			var file = new FileInfo(File_Name);
			var size = new Size(Buffer_LS_Width, Buffer_LS_Height);
			var key = new ImageTransitionControl.BackgroundFileKey(file.FullName, file.LastWriteTimeUtc,
				file.Length, PicMode, size, InPic.Size);
			if (InPic.RenderedBackground == key && InPic.BackgroundID == "")
			{
				InPic.CurrentBackgroundPicture = InPic.NewBackgroundPicture;
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				return;
			}

			Image image = new Bitmap(size.Width, size.Height);
			using var imageGuard = new UnstoredImageGuard(image);
			using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			using (var decoded = Image.FromStream(stream))
			using (var source = new Bitmap(decoded))
			using (var graphics = Graphics.FromImage(image))
				DrawFileBackground(graphics, source, size, InPic.Size, PicMode);

			InPic.TransitBackPictureAction = InPic.BackgroundID != "" && InPic.BackgroundID == DefaultBackgroundID
				? ImageTransitionControl.BackPicturesTransition.NewOnly
				: ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
			InPic.BackgroundID = "";
			InPic.ImageFileName = File_Name;
			InPic.PicMode = (int)PicMode;
			InPic.CurrentBackgroundPicture = InPic.NewBackgroundPicture ?? image;
			InPic.NewBackgroundPicture = InPic.AdoptImage(image);
			imageGuard.Stored = true;
			InPic.RenderedBackground = key;
		}

		private static void DrawFileBackground(Graphics graphics, Image source, Size size, Size viewport, ImageMode mode)
		{
			var destination = new Rectangle(Point.Empty, size);
			if (mode == ImageMode.Tile)
			{
				using var tiles = new TextureBrush(source, System.Drawing.Drawing2D.WrapMode.Tile);
				graphics.FillRectangle(tiles, destination);
				return;
			}

			double sourceRatio = (double)source.Width / source.Height;
			double viewportRatio = (double)viewport.Width / viewport.Height;
			var crop = new Rectangle(Point.Empty, source.Size);
			if (mode == ImageMode.Centre)
			{
				if (viewportRatio < sourceRatio)
					destination.Height = (int)(size.Width / sourceRatio);
				else
					destination.Width = (int)(size.Height * sourceRatio);
				destination.Offset((size.Width - destination.Width) / 2, (size.Height - destination.Height) / 2);
			}
			else
			{
				if (viewportRatio < sourceRatio)
					crop.Width = (int)(source.Height * viewportRatio);
				else
					crop.Height = (int)(source.Width / viewportRatio);
				crop.Offset((source.Width - crop.Width) / 2, (source.Height - crop.Height) / 2);
			}
			graphics.DrawImage(source, destination, crop, GraphicsUnit.Pixel);
		}

		/// <summary>
		/// Disposes a picture that was made for a screen but never handed to it (the method returned early
		/// or threw). Declared before the Graphics drawing on the picture, so a using declaration disposes
		/// that Graphics first.
		/// </summary>
		private sealed class UnstoredImageGuard(Image image) : IDisposable
		{
			private readonly Image image = image;

			public bool Stored;

        public void Dispose()
			{
				if (!Stored)
				{
					image.Dispose();
				}
			}
		}

		public static int GetOneRegionHeight(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, int RegNum, ListView LyricsAndNotationsList, ref Graphics g, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, bool OnlyOneRegionShown, ref Font MainFont, ref Font NotationsFont, int InterlaceOption, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (InItem.LyricsAndNotationsList.Items.Count == 0)
			{
				return 0;
			}
			int lyricFontSize = (int)InItem.Lyrics[RegNum].FS_Font.Size;
			int notationFontSize = (int)((double)lyricFontSize * NotationFontFactor);
			string regionText = "";
			string lineText = "";
			int regionBottom = 0;
			int fS_Left = InItem.Lyrics[RegNum].FS_Left;
			int fS_Top = InItem.Lyrics[RegNum].FS_Top;
			int fS_Width = InItem.Lyrics[RegNum].FS_Width;
			int availableHeight = (InItem.Slide[0, 3] > 0 && !OnlyOneRegionShown) ? InItem.Lyrics[RegNum].FS_Height_R2Bound : InItem.Lyrics[RegNum].FS_Height;
			regionBottom = InItem.Lyrics[RegNum].FS_Top + InItem.Lyrics[RegNum].FS_Height;
			int fS_Height_R2Bound = InItem.Lyrics[1].FS_Height_R2Bound;
			SizeF layoutArea = new(fS_Width, 32000f);
			int wrappedLineCount = 0;
			int lyricLineCount = 0;
			double lineSpacingFactor = Folders[InItem.FolderNo].FontSpacingFactor[(RegNum != 0) ? 1 : 0] + ((InShowNotations > 0) ? NotationFontFactor : 0.0);
			int firstLineIndex;
			int lastLineIndex;
			switch (RegNum)
			{
				case 0:
					firstLineIndex = InItem.Slide[InItem.CurSlide, 1];
					lastLineIndex = InItem.Slide[InItem.CurSlide, 2];
					break;
				case 1:
					firstLineIndex = InItem.Slide[InItem.CurSlide, 3];
					lastLineIndex = InItem.Slide[InItem.CurSlide, 4];
					break;
				default:
					regionText = InItem.Lyrics[RegNum].Text;
					ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, lyricFontSize, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref regionText);
					}
					ReduceFontToFit(g, regionText, ref MainFont, fS_Width, availableHeight, MultiLine: true);
					return availableHeight;
			}
			if ((firstLineIndex < 0) | (lastLineIndex < 0))
			{
				return 0;
			}
			if (firstLineIndex <= lastLineIndex)
			{
				string lineLayoutData = "";
				for (int i = firstLineIndex; i <= lastLineIndex; i++)
				{
					ListViewItem item = InItem.LyricsAndNotationsList.Items[i];
					lineText = item.SubItems[2].Text;
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref lineText);
					}
					regionText = regionText + lineText + "\n";
					lineLayoutData = item.SubItems[4].Text;
					wrappedLineCount += DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref lineLayoutData, '>', RemoveExtract: false));
					lyricLineCount++;
				}
				if (regionText.Length > 1)
				{
					regionText = DataUtil.Left(regionText, regionText.Length - 1);
				}
			}
			else
			{
				lyricLineCount = 1;
			}
			availableHeight = (int)((double)availableHeight / lineSpacingFactor);
			int measuredHeight;
			if (UseLargestFontSize)
			{
				ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, lyricFontSize, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
				bool OnlyOneDisplayLine = false;
				measuredHeight = IncreaseFontToLargest(g, regionText, ref MainFont, fS_Width, availableHeight, ref OnlyOneDisplayLine);
				ReplaceFont(ref NotationsFont, new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style));
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * lineSpacingFactor);
				return (int)((double)measuredHeight * lineSpacingFactor);
			}
			if (wrappedLineCount > 0)
			{
				string InString = InItem.LyricsAndNotationsList.Items[firstLineIndex].SubItems[4].Text;
				DataUtil.ExtractOneInfo(ref InString, '>');
				lyricFontSize = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				if (lyricFontSize < 1)
				{
					lyricFontSize = (int)InItem.Lyrics[RegNum].Font.Size;
				}
				ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, lyricFontSize, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
				ReplaceFont(ref NotationsFont, new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style));
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * lineSpacingFactor);
				return InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight * wrappedLineCount;
			}
			ReplaceFont(ref MainFont, new Font(InItem.Lyrics[RegNum].Font.Name, lyricFontSize, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style));
			ReduceFontToFit(g, regionText, ref MainFont, fS_Width, availableHeight, MultiLine: true);
			measuredHeight = availableHeight;
			ReplaceFont(ref NotationsFont, new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style));
			InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * lineSpacingFactor);
			return (int)((double)measuredHeight * lineSpacingFactor);
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
				string reversedText = "";
				for (int characterIndex = InString.Length - 1; characterIndex >= 0; characterIndex--)
				{
					reversedText += InString[characterIndex];
				}
				InString = reversedText;
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
				MainEditHistoryList[i].Id = "";
			}
			TotalMainEditHistory = AbsoluteMaxHitoryItems;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				MainEditHistoryList[i].Id = RegUtil.GetRegValue("maineditlist", i.ToString(), "");
				if (MainEditHistoryList[i].Id == "")
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
				EditorEditHistoryList[i].Id = "";
			}
			TotalEditorEditHistory = AbsoluteMaxHitoryItems;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				EditorEditHistoryList[i].Id = RegUtil.GetRegValue("editoreditlist", i.ToString(), "");
				if (EditorEditHistoryList[i].Id == "")
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
				InfoScreenEditHistoryList[i].Id = "";
			}
			TotalInfoScreenEditHistory = MaxUserEditHistory;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				InfoScreenEditHistoryList[i].Id = RegUtil.GetRegValue("infoscreeneditlist", i.ToString(), "");
				if (InfoScreenEditHistoryList[i].Id == "")
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
			string itemIdOrPath = DataUtil.Mid(InIDString, 1);
			string InFileName = "";
			if (a == "D")
			{
				try
				{
					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, "select * from SONG where songid=@id", ("@id", itemIdOrPath));

					if (datatable.Rows.Count > 0 && DataUtil.GetDataInt(datatable.Rows[0], "FolderNo") > 0 && Folders[DataUtil.GetDataInt(datatable.Rows[0], "FolderNo")].Use > 0)
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
				InFileName = itemIdOrPath;
			}
			else if (!(a == "B"))
			{
				if (a == "T")
				{
					InFileName = itemIdOrPath;
				}
				else if (a == "I")
				{
					InFileName = itemIdOrPath;
					GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				}
				else if (a == "W")
				{
					InFileName = itemIdOrPath;
				}
				else if (a == "M")
				{
					InFileName = itemIdOrPath;
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
					RegUtil.SaveRegValue("maineditlist", i.ToString(), MainEditHistoryList[i].Id);
					continue;
				}
				RegUtil.SaveRegValue("maineditlist", i.ToString(), "");
				MainEditHistoryList[i].Id = "";
			}
		}

		public static void RemoveDuplicateEditorHistoryItems(ref EditHistoryList InEditHistoryList, ref int InTotalEditorEditHistory)
		{
			int historyCount = (InTotalEditorEditHistory <= AbsoluteMaxHitoryItems) ? InTotalEditorEditHistory : AbsoluteMaxHitoryItems;
			bool isDuplicate = false;
			int uniqueCount = 0;
			int historyIndex = 0;
			int uniqueIndex = 0;
			for (historyIndex = 1; historyIndex <= AbsoluteMaxHitoryItems; historyIndex++)
			{
				TempEditHistoryList[historyIndex].Id = "";
				TempEditHistoryList[historyIndex].Title = "";
			}
			for (historyIndex = 1; historyIndex <= historyCount; historyIndex++)
			{
				if (historyIndex == 1)
				{
					uniqueCount = 1;
					TempEditHistoryList[1].Id = InEditHistoryList[1].Id;
					TempEditHistoryList[1].Title = InEditHistoryList[1].Title;
					continue;
				}
				isDuplicate = false;
				for (uniqueIndex = 1; uniqueIndex <= uniqueCount; uniqueIndex++)
				{
					if (InEditHistoryList[historyIndex].Id == TempEditHistoryList[uniqueIndex].Id)
					{
						isDuplicate = true;
					}
				}
				if (!isDuplicate)
				{
					uniqueCount++;
					TempEditHistoryList[uniqueCount].Id = InEditHistoryList[historyIndex].Id;
					TempEditHistoryList[uniqueCount].Title = InEditHistoryList[historyIndex].Title;
				}
			}
			for (historyIndex = 1; historyIndex <= AbsoluteMaxHitoryItems; historyIndex++)
			{
				InEditHistoryList[historyIndex].Id = TempEditHistoryList[historyIndex].Id;
				InEditHistoryList[historyIndex].Title = TempEditHistoryList[historyIndex].Title;
			}
			InTotalEditorEditHistory = uniqueCount;
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
					RegUtil.SaveRegValue("editoreditlist", i.ToString(), EditorEditHistoryList[i].Id);
					continue;
				}
				RegUtil.SaveRegValue("editoreditlist", i.ToString(), "");
				EditorEditHistoryList[i].Id = "";
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
					RegUtil.SaveRegValue("infoscreeneditlist", i.ToString(), InfoScreenEditHistoryList[i].Id);
					continue;
				}
				RegUtil.SaveRegValue("infoscreeneditlist", i.ToString(), "");
				InfoScreenEditHistoryList[i].Id = "";
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
			xtw.WriteStartElement("ListHeader");
			xtw.WriteElementString("SystemID", SystemID);
			xtw.WriteElementString("FormatData", GetListFormat(InFormatString));
			xtw.WriteElementString("Notes", InNotes);
			xtw.WriteEndElement();
		}

		public static string GetListFormat(string InFormatString)
		{
			int mediaFlags = MediaMute + MediaRepeat * 2 + MediaWidescreen * 4;
			StringBuilder stringBuilder = new();
			if (InFormatString != "")
			{
				stringBuilder.Append(InFormatString);
			}
			else
			{
				stringBuilder.AppendFormat("{0}={1}>", 11, Convert.ToString(PanelBackColour.ToArgb()));
				stringBuilder.AppendFormat("{0}={1}>", 12, PanelBackColourTransparent);
				stringBuilder.AppendFormat("{0}={1}>", 13, Convert.ToString(PanelTextColour.ToArgb()));
				stringBuilder.AppendFormat("{0}={1}>", 14, PanelTextColourAsRegion1);
				stringBuilder.AppendFormat("{0}={1}>", 15, ShowDataDisplayMode + ShowDataDisplaySlides * 2 + ShowDataDisplaySongs * 4 + ShowDataDisplayTitle * 8 + ShowDataDisplayCopyright * 16 + ShowDataDisplayPrevNext * 32);
				stringBuilder.AppendFormat("{0}={1}>", 16, Convert.ToString((int)(BottomBorderFactor * 100.0)));
				stringBuilder.AppendFormat("{0}={1}>", 17, ShowDataDisplayFontName);
				stringBuilder.AppendFormat("{0}={1}>", 18, ShowDataDisplayFontBold + ShowDataDisplayFontItalic * 2 + ShowDataDisplayFontUnderline * 4 + ShowDataDisplayFontShadow * 8 + ShowDataDisplayFontOutline * 16);
				stringBuilder.AppendFormat("{0}={1}>", 19, ShowDataDisplayIndicatorsFontSize);
				stringBuilder.AppendFormat("{0}={1}>", 21, ShowSongHeadings);
				stringBuilder.AppendFormat("{0}={1}>", 23, ShowSongHeadingsAlign);
				stringBuilder.AppendFormat("{0}={1}>", 22, UseShadowFont * 2 + ShowNotations * 4 + ShowCapoZero * 8 + ShowInterlace * 16 + UseOutlineFont * 32);
				stringBuilder.AppendFormat("{0}={1}>", 25, ShowLyrics);
				stringBuilder.AppendFormat("{0}={1}>", 26, Convert.ToString(ShowScreenColour[0].ToArgb()));
				stringBuilder.AppendFormat("{0}={1}>", 27, Convert.ToString(ShowScreenColour[1].ToArgb()));
				stringBuilder.AppendFormat("{0}={1}>", 28, ShowScreenStyle.ToString());
				stringBuilder.AppendFormat("{0}={1}>", 29, Convert.ToString(ShowFontColour[0].ToArgb()));
				stringBuilder.AppendFormat("{0}={1}>", 30, Convert.ToString(ShowFontColour[1].ToArgb()));
				stringBuilder.AppendFormat("{0}={1}>", 31, Folders[0].FontAlign[0]);
				stringBuilder.AppendFormat("{0}={1}>", 32, Folders[0].FontAlign[1]);
				stringBuilder.AppendFormat("{0}={1}>", 50, MediaOption);
				stringBuilder.AppendFormat("{0}={1}>", 51, MediaLocation);
				stringBuilder.AppendFormat("{0}={1}>", 52, MediaVolume);
				stringBuilder.AppendFormat("{0}={1}>", 53, MediaBalance);
				stringBuilder.AppendFormat("{0}={1}>", 54, mediaFlags.ToString());
				stringBuilder.AppendFormat("{0}={1}>", 55, MediaCaptureDeviceNumber.ToString());
				stringBuilder.AppendFormat("{0}={1}>", 56, MediaOutputMonitorName);
				stringBuilder.AppendFormat("{0}={1}>", 61, BackgroundPicture);
				stringBuilder.AppendFormat("{0}={1}>", 62, Convert.ToString((int)BackgroundMode));
				stringBuilder.AppendFormat("{0}={1}>", 63, ShowVerticalAlign);
				stringBuilder.AppendFormat("{0}={1}>", 64, Folders[0].LeftMargin);
				stringBuilder.AppendFormat("{0}={1}>", 65, Folders[0].RightMargin);
				stringBuilder.AppendFormat("{0}={1}>", 66, Folders[0].BottomMargin);
				stringBuilder.AppendFormat("{0}={1}>", 72, GlobalImageCanvas.GetTransitionText(ShowItemTransition));
				stringBuilder.AppendFormat("{0}={1}>", 73, GlobalImageCanvas.GetTransitionText(ShowSlideTransition));
				for (int part = 0; part < PraiseBookTextStyles.Length; part++)
				{
					PraiseBookTextStyles[part].AppendToHeader(stringBuilder, part);
				}
				stringBuilder.AppendFormat("{0}={1}>", 151, PB_ShowHeadings[0] + PB_ShowHeadings[1] * 2 + PB_ShowHeadings[2] * 4 + PB_ShowHeadings[3] * 8);
				stringBuilder.AppendFormat("{0}={1}>", 153, PB_LyricsPattern);
				stringBuilder.AppendFormat("{0}={1}>", 154, PB_ShowSection);
				stringBuilder.AppendFormat("{0}={1}>", 155, PB_ShowColumns);
				stringBuilder.AppendFormat("{0}={1}>", 156, PB_PageSize);
				stringBuilder.AppendFormat("{0}={1}>", 170, PB_Spacing[0]);
				stringBuilder.AppendFormat("{0}={1}>", 171, PB_Spacing[1]);
				stringBuilder.AppendFormat("{0}={1}>", 172, PB_ShowScreenBreaks);
				stringBuilder.AppendFormat("{0}={1}>", 173, PB_OneSongPerPage);
				stringBuilder.AppendFormat("{0}={1}>", 174, PB_CJKGroupStyle);
				stringBuilder.AppendFormat("{0}={1}>", 180, PB_ShowNotations + PB_ShowTiming * 2 + PB_ShowKey * 4 + PB_ShowCapo * 8 + PB_CapoZero * 16);
			}
			return stringBuilder.ToString();
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

		public static bool ExtractAxicultusXMLItem(ref XmlTextReader reader, ref SongSettings item)
		{
			while (MoveToXMLItemElement(ref reader))
			{
				using var subtree = reader.ReadSubtree();
				var song = ExchangeSong.ReadXmlItem(subtree);
				if (song == null) continue;
				ApplyExchangeSong(song, ref item, ConvertTextStringToSequence(song.Sequence));
				LoadLicAdminDisplayInfo(ref item.Show_LicAdminInfo1, ref item.Show_LicAdminInfo2);
				return true;
			}
			return false;
		}

		internal static void ApplyExchangeSong(ExchangeSong song, ref SongSettings item, string decodedSequence)
		{
			item.Type = "I";
			item.FolderName = song.Folder;
			item.FolderNo = GetFolderNumber(song.Folder);
			item.SongNumber = song.SongNumber;
			item.Capo = song.Capo;
			item.Title = song.Title1;
			item.Title2 = song.Title2;
			item.CompleteLyrics = song.Contents;
			item.Notations = song.Notations;
			item.OriginalNotations = song.Notations;
			item.Writer = song.Writer;
			item.Copyright = song.Copyright;
			item.Category = song.Category;
			item.Timing = song.Timing;
			item.MusicKey = song.MusicKey;
			item.Show_LicAdminInfo1 = song.LicenceAdmin1;
			item.Show_LicAdminInfo2 = song.LicenceAdmin2;
			item.Book_Reference = song.BookReference;
			item.User_Reference = song.UserReference;
			item.Format.FormatString = song.FormatData;
			item.Settings = song.Settings;
			item.SongSequence = decodedSequence;
			item.SongOriginalLoadedSequence = item.SongSequence;
			item.In_LicAdminInfo1 = song.LicenceAdmin1;
			item.In_LicAdminInfo2 = song.LicenceAdmin2;
			item.Format.ImageString = song.Image ?? "";
			item.RotateString = ExtractSettings(song.Settings, SettingsCategory.RotateString);
			GetRotationStyle(ref item);
		}

		internal static ExchangeSong ToExchangeSong(SongSettings item, string folder) =>
			new()
        {
				Folder = folder, SongNumber = item.SongNumber, Capo = item.Capo,
				Sequence = SongExchangeSequence.FromLegacy(item.SongSequence),
				Title1 = item.Title,
				Title2 = item.Title2,
				Contents = item.CompleteLyrics,
				Notations = item.Notations,
				Writer = item.Writer,
				Copyright = item.Copyright,
				Category = item.Category,
				Timing = item.Timing,
				MusicKey = item.MusicKey,
				LicenceAdmin1 = item.Show_LicAdminInfo1,
				LicenceAdmin2 = item.Show_LicAdminInfo2,
				BookReference = item.Book_Reference,
				UserReference = item.User_Reference,
				FormatData = item.Format.FormatString,
				Settings = item.Settings,
			};

		public static void GetTitle2AndFormatFromInfoFile(string InFileName, ref string Title2, ref string FormatString)
		{
			InitialiseIndividualData(ref TempItem1);
			gfFileHelpers.LoadInfoFile(InFileName, ref TempItem1);
			Title2 = TempItem1.Title2;
			FormatString = TempItem1.Format.FormatString;
		}

		public static string ExtractSettings(string InSettingsString, SettingsCategory settingsCategory)
		{
			if (InSettingsString == "")
			{
				return "";
			}
			string categoryKey = "";
			int categoryIndex = -1;
			string[] array = InSettingsString.Split('>');
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				categoryKey = DataUtil.ExtractOneInfo(ref array[i], '=', RemoveExtract: true, MinusOneIfBlank: false);
				if (categoryKey != "")
				{
					categoryIndex = DataUtil.StringToInt(categoryKey);
					if (categoryIndex == (int)settingsCategory)
					{
						return array[i];
					}
				}
			}
			return "";
		}

		public static string CombineSettings(SongSettings InItem)
		{
			StringBuilder stringBuilder = new();
			stringBuilder.AppendFormat("{0}={1}>", 10, InItem.RotateString);
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
				WordDoc wordDoc = new();
				return wordDoc.GetContents(InFileName).Replace("\v", "\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception in SetColoursFormat: {ex.Message}");
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
			if (MessageBox.Show(UiText.Get("gfUtility", "ClearFormattingConfirm"), UiText.Get("gfUtility", "ClearFormattingCaption"), MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					string fullSearchString = "select * from SONG where FORMATDATA <> ''";

					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);

					DbDataAdapter sQLiteDataAdapter = null;

					DataTable dataTable = null;
					(sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, fullSearchString);

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
			if (MessageBox.Show(UiText.Format("gfUtility", "ClearSettingsConfirm", DefaultAxicultusDir), UiText.Get("gfUtility", "ClearSettingsCaption"), MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					RegUtil.Profile.Reset();
					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Reset settings failed: {ex.Message}");
				}
			}
			return false;
		}

		public static void SaveFormatStringToDatabase(string SongID, string FormatString)
		{
			int intSongID = 0;
			bool result = int.TryParse(SongID, out intSongID);

			if (!result) return;

			try
			{
				using var connection = DbController.GetDbConnection(ConnectStringMainDB);
				using var command = DbController.CreateCommand(connection, "UPDATE SONG SET FormatData=@format WHERE SongID=@id",
					("@format", FormatString), ("@id", intSongID));
				command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SaveFormatStringToDatabase Exception: {ex.Message}");
			}
		}

		public static bool FormInUse(string InFormName)
		{
			return !Windows.Win32.PInvoke.FindWindow(null, InFormName).IsNull;
		}

		public static void SetMenuItem(ref ToolStripMenuItem InMenuItem, string ItemName, string DefaultText, string MergedText, bool DisableWhenBlank)
		{
			if (ItemName == "")
			{
				InMenuItem.Text = DefaultText;
				InMenuItem.Enabled = ((!DisableWhenBlank) ? true : false);
				return;
			}
			int maxTitleLength = 20;
			string str = (ItemName.Length <= maxTitleLength) ? "'" : "...'";
			InMenuItem.Text = MergedText + " '" + DataUtil.Left(ItemName, maxTitleLength) + str;
			InMenuItem.Enabled = true;
		}

	



		public static string LookupDBTitle2(int InKey)
		{
			try
			{
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, "select Title_2 from SONG where songid=@id", ("@id", Convert.ToString(InKey)));

				if (datatable.Rows.Count > 0)
				{
					return DataUtil.ObjToString(datatable.Rows[0]["Title_2"]);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"LookupDBTitle2 Exception: {ex.Message}");
			}
			return "";
		}
}
