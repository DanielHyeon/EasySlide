using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easislides.Util
{
    class RegUtil
    {
		public const string Reg_RootEasiSlidesDir = "root_directory";

		public const string Reg_CurSession = "current_session";

		public const string Reg_CurPraiseBook = "current_praisebook";

		public const string Reg_ExportToDir = "export_dir";

		public const string Reg_ImportFromDir = "import_dir";

		public const string Reg_MediaDir = "media_dir";

		public const string Reg_PraiseOutputDir = "praiseoutput_dir";

		public const string Reg_RegKey = "RegistrationNumber";

		public const string Reg_RegUser = "RegistrationUser";

		public const string Reg_UseSongNumbers = "UseSongNumbers";

		public const string Reg_PrinterSpaces = "PrinterSpaces";

		public const string Reg_LicAdminCCLINo = "licCCLI_no";

		public const string Reg_LicAdmin4No = "lic4_no";

		public const string Reg_LicAdmin5No = "lic5_no";

		public const string Reg_LicAdmin6No = "lic6_no";

		public const string Reg_LicAdmin7No = "lic7_no";

		public const string Reg_LicAdmin8No = "lic8_no";

		public const string Reg_LicAdminNoSymbol = "licNoSym";

		public const string Reg_LicAdminEnforceDisplay = "licEnforceDisplay";

		public const string Reg_HB_MaxVersesSelection = "BibleMaxSelectVerses";

		public const string Reg_HB_MaxAdhocVersesSelection = "BibleMaxAdhocVersesSelection";

		public const string Reg_HB_ShowVerses = "BibleShowVerses";

		public const string Reg_PP_MaxFiles = "PowerpointMaxFiles";

		public const string Reg_MaxEditHistory = "MaxEditHistory";

		public const string Reg_UsePowerpointTab = "UsePowerpointTab";

		public const string Reg_NoPowerpointPanelOverlay = "NoPowerpointPanelOverlay";

		public const string Reg_UseMediaTab = "UseMediaTab";

		public const string Reg_ShowLyricsMonitorAlertBox = "ShowLyricsMonitorAlertBox";

		public const string Reg_NoMediaPanelOverlay = "NoMediaPanelOverlay";

		public const string Reg_AutoTextOverflow = "AutoTextOverflow";

		public const string Reg_UseLargestFontSize = "UseLargestFontSize";

		public const string Reg_AdvanceNextItem = "AdvanceNextItem";

		public const string Reg_LineBetweenRegions = "LineBetweenRegions";

		public const string Reg_GapItemOption = "GapItemOption";

		public const string Reg_GapItemLogoFile = "GapItemLogoFile";

		public const string Reg_GapItemUseFade = "GapItemUseFade";

		public const string Reg_KeyBoardOption = "KeyBoardOption";

		public const string Reg_OutputMonitorNo = "OutputmonitorNumber";

		public const string Reg_LyricsMonitorNo = "LyricsMonitorNumber";

		public const string Reg_PreviewAreaShowNotations = "PreviewAreaShowNotations";

		public const string Reg_PreviewAreaLineBetweenScreens = "PreviewAreaLineBetweenScreens";

		public const string Reg_PreviewAreaFontSize = "PreviewAreaFontSize";

		public const string Reg_BibleTextFontSize = "BibleTextFontSize";

		public const string Reg_EditMainFontName = "EditMainFontName";

		public const string Reg_EditMainFontSize = "EditMainFontSize";

		public const string Reg_EditNotationFontSize = "EditNotationFontSize";

		public const string Reg_InfoMainFontName = "InfoMainFontName";

		public const string Reg_InfoMainFontSize = "InfoMainFontSize";

		public const string Reg_InfoMainShowAllButtons = "InfoMainShowAllButtons";

		public const string Reg_EditOpenDocumentDir = "EditOpenDocumentDir";

		public const string Reg_ShowRotateGap = "RotateGap";

		public const string Reg_WordWrapLeftAlignIndent = "WordWrapLeftAlignIndent";

		public const string Reg_WordWrapIgnoreStartSpaces = "WordWrapIgnoreStartSpaces";

		public const string Reg_ParentalAlertHeading = "ParentalAlertHeading";

		public const string Reg_ParentalAlertDuration = "ParentalAlertDuration";

		public const string Reg_ParentalAlertTextColour = "ParentalAlertTextColour";

		public const string Reg_ParentalAlertBackColour = "ParentalAlertBackColour";

		public const string Reg_ParentalAlertTextAlign = "ParentalAlertTextAlign";

		public const string Reg_ParentalAlertVerticalAlign = "ParentalAlertVerticalAlign";

		public const string Reg_ParentalAlertStyle = "ParentalAlertStyle";

		public const string Reg_ParentalAlertFontName = "ParentalAlertFontName";

		public const string Reg_ParentalAlertFontSize = "ParentalAlertFontSize";

		public const string Reg_ParentalAlertFontFormat = "ParentalAlertFontFormat";

		public const string Reg_DMAlwaysUse = "AlwaysTryDualMonitor";

		public const string Reg_DMOption = "DualMonitorOption";

		public const string Reg_DMOptionCusLeft = "DualMonitorOptionCustomLeft";

		public const string Reg_DMOptionCusTop = "DualMonitorOptionCustomTop";

		public const string Reg_DMOptionCusWidth = "DualMonitorOptionCustomWidth";

		public const string Reg_DMOptionCusAsSingleMonitor = "DualMonitorOptionCustomAsSingle";

		public const string Reg_DisableSreenSaver = "DisableSreenSaver";

		public const string Reg_VideoSize = "VideoSize";

		public const string Reg_VideoVAlign = "VideoVAlign";

		public const string Reg_LMAlwaysUse = "AlwaysTrySecondaryLyricsMonitor";

		public const string Reg_LMOption = "LyricsMonitorOption";

		public const string Reg_LMOptionCusLeft = "LyricsMonitorOptionCustomLeft";

		public const string Reg_LMOptionCusTop = "LyricsMonitorOptionCustomTop";

		public const string Reg_LMOptionCusWidth = "LyricsMonitorOptionCustomWidth";

		public const string Reg_LMTextColour = "LyricsMonitorTextColour";

		public const string Reg_LMHighlightColour = "LyricsMonitorHighlightColour";

		public const string Reg_LMBackColour = "LyricsMonitorBackColour";

		public const string Reg_LMShowNotations = "LyricsMonitorShowNotations";

		public const string Reg_LMFontSize = "LyricsMonitorFontSize";

		public const string Reg_LMNotationsFontSize = "LyricsMonitorNotationsFontSize";

		public const string Reg_LMFontFormat = "LyricsMonitorFontFormat";

		public const string Reg_FocusedTextRegionColour = "FocusedBackColour";

		public const string Reg_TextRegionSlideTextColour = "SelectedSlideTextColour";

		public const string Reg_TextRegionSlideBackColour = "SelectedSlideBackColour";

		public const string Reg_UseFocusedTextRegionColour = "UseFocusedBackColour";

		public const string Reg_JumpToA = "JumpToA";

		public const string Reg_JumpToB = "JumpToB";

		public const string Reg_JumpToC = "JumpToC";

		public const string Reg_MessageAlertDuration = "MessageAlertDuration";

		public const string Reg_MessageAlertTextColour = "MessageAlertTextColour";

		public const string Reg_MessageAlertBackColour = "MessageAlertBackColour";

		public const string Reg_MessageAlertTextAlign = "MessageAlertTextAlign";

		public const string Reg_MessageAlertVerticalAlign = "MessageAlertVerticalAlign";

		public const string Reg_MessageAlertStyle = "MessageAlertStyle";

		public const string Reg_MessageAlertFontName = "MessageAlertFontName";

		public const string Reg_MessageAlertFontSize = "MessageAlertFontSize";

		public const string Reg_MessageAlertFontFormat = "MessageAlertFontFormat";

		public const string Reg_ReferenceAlertDuration = "ReferenceAlertDuration";

		public const string Reg_ReferenceAlertTextColour = "ReferenceAlertTextColour";

		public const string Reg_ReferenceAlertBackColour = "ReferenceAlertBackColour";

		public const string Reg_ReferenceAlertTextAlign = "ReferenceAlertTextAlign";

		public const string Reg_ReferenceAlertVerticalAlign = "ReferenceAlertVerticalAlign";

		public const string Reg_ReferenceAlertStyle = "ReferenceAlertStyle";

		public const string Reg_ReferenceAlertFontName = "ReferenceAlertFontName";

		public const string Reg_ReferenceAlertFontSize = "ReferenceAlertFontSize";

		public const string Reg_ReferenceAlertFontFormat = "ReferenceAlertFontFormat";

		public const string Reg_ReferenceAlertSource = "ReferenceAlertSource";

		public const string Reg_ReferenceAlertUsePick = "ReferenceAlertUsePick";

		public const string Reg_ReferenceAlertBlankIfPickNotFound = "ReferenceAlertBlankIfPickNotFound";

		public const string Reg_ReferenceAlertPickName = "ReferenceAlertPickName";

		public const string Reg_ReferenceAlertPickSubstitute = "ReferenceAlertPickSubstitute";

		public const string Reg_ReferenceAlertPickSeparator = "ReferenceAlertPickSeparator";

		public const string Reg_HTMLPublisher = "HTML_Publisher";

		public const string Reg_HTMLWeb = "HTML_Web";

		public const string Reg_HTMLInfo1 = "HTML_Info1";

		public const string Reg_HTMLMusicLink = "HTML_Musiclink";

		public const string Reg_HTMLShowIndex = "HTML_ShowIndex";

		public const string Reg_HTMLShowNumbers = "HTML_ShowNumbers";

		public const string Reg_HTMLShowChorusItalics = "HTML_ShowChorusItalics";

		public const string Reg_HTMLShowBookRef = "HTML_ShowBookReference";

		public const string Reg_HTMLShowUserRef = "HTML_ShowUserReference";

		public const string Reg_HTMLDoubleByte = "HTML_DoubleByte";

		public const string Reg_HTMLDocumentOption = "HTML_DocumentOption";

		public const string Reg_HTMLRTFFontSize = "HTML_RTFFontSize";

		public const string Reg_OutlineFontSizeThreshold = "OutlineFontSizeThreshold";

		public const string Reg_ExportExtension = "export_ext";

		public const string Reg_AutoRotateOn = "AutoRotateOn";

		public const string Reg_AutoRotateStyle = "AutoRotateStyle";

		public const string Reg_NotationFontFactor = "NotationFontFactor";

		public const string Reg_ExternalListing = "ExternalListing";

		public const string Reg_LiveCamNumber = "LiveCamNumber";

		public const string Reg_LiveCamMute = "LiveCamMute";

		public const string Reg_LiveCamVolume = "LiveCamVolume";

		public const string Reg_LiveCamBalance = "LiveCamBalance";

		public const string Reg_LiveCamWidescreen = "LiveCamWidescreen";

		public const string Reg_LiveCamNoPanelOverlay = "LiveCamNoPanelOverlay";

		public static void SaveRegValue(string Section, string Name, string Value)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\EasiSlides\\" + Section))
				{
					registryKey.SetValue(Name, Value, RegistryValueKind.String);
				}
			}
			catch
			{
			}
		}

		public static void SaveRegValue(string Section, string Name, int Value)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\EasiSlides\\" + Section))
				{
					registryKey.SetValue(Name, Value, RegistryValueKind.DWord);
				}
			}
			catch
			{
			}
		}

		public static void SaveRegValue(string Section, string Name, long Value)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\EasiSlides\\" + Section))
				{
					registryKey.SetValue(Name, Value, RegistryValueKind.QWord);
				}
			}
			catch
			{
			}
		}

		public static string GetRegValue(string Section, string Name, string defaultValue)
		{
			object regValue = GetRegValue(Section, Name, (object)defaultValue);
			if (regValue == null)
			{
				return defaultValue;
			}
			return DataUtil.ObjToString(regValue);
		}

		public static int GetRegValue(string Section, string Name, int defaultValue)
		{
			object regValue = GetRegValue(Section, Name, (object)defaultValue);
			if (regValue == null)
			{
				return defaultValue;
			}
			return DataUtil.ObjToInt(regValue);
		}

		public static object GetRegValue(string Section, string Name, object defaultValue)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\EasiSlides\\" + Section))
				{
					return registryKey.GetValue(Name, defaultValue);					
				}
			}
			catch
			{
				return null;
			}
		}

		public static void DeleletRegKey(string Section, string Name)
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\EasiSlides\\" + Section, writable: true);
				registryKey.DeleteValue(Name);
				registryKey.Close();
			}
			catch
			{
			}
		}
	}
}
