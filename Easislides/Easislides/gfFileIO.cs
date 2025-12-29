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

		public static bool LoadFileContents(string InFileName, ref string InString)
		{
			InString = "";
			if (File.Exists(InFileName))
			{
				using StreamReader streamReader = new StreamReader(InFileName, Encoding.Default, detectEncodingFromByteOrderMarks: true);
				InString = streamReader.ReadToEnd();
				//streamReader.Close();
				return true;
			}
			return false;
		}

		public static string GetOpenFileDialogMediaString()
		{
			string openFileDialogMediaString = GetOpenFileDialogMediaString(MediaBackgroundStyle.None);
			string openFileDialogMediaString2 = GetOpenFileDialogMediaString(MediaBackgroundStyle.Audio);
			string openFileDialogMediaString3 = GetOpenFileDialogMediaString(MediaBackgroundStyle.Video);
			string str = "All Files (*.*)|*.*";
			openFileDialogMediaString = ((openFileDialogMediaString != "") ? (openFileDialogMediaString + "|") : "");
			openFileDialogMediaString2 = ((openFileDialogMediaString2 != "") ? (openFileDialogMediaString2 + "|") : "");
			openFileDialogMediaString3 = ((openFileDialogMediaString3 != "") ? (openFileDialogMediaString3 + "|") : "");
			return openFileDialogMediaString + openFileDialogMediaString2 + openFileDialogMediaString3 + str;
		}

		public static string GetOpenFileDialogMediaString(MediaBackgroundStyle InMediaType)
		{
			if (TotalMediaFileExt == 0)
			{
				return "";
			}
			string str = "";
			string text = "";
			int num = 0;
			switch (InMediaType)
			{
				case MediaBackgroundStyle.Audio:
					str = "Audio Files (";
					break;
				case MediaBackgroundStyle.Video:
					str = "Video Files (";
					break;
			}
			bool flag = true;
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				if (InMediaType == MediaBackgroundStyle.None || MediaFileExtension[i, 1] == InMediaType.ToString())
				{
					str = str + (flag ? "" : ",") + "*" + MediaFileExtension[i, 0];
					text = text + (flag ? "" : ";") + "*" + MediaFileExtension[i, 0];
					flag = false;
				}
			}
			if (flag)
			{
				return "";
			}
			str = ((InMediaType != 0) ? (str + ")") : "Media Files (all types)");
			return str + "|" + text;
		}

		public static bool SaveIndexFile(string InFileName, ref ListView InList, UsageMode InMode, bool SaveAllItems, string InFormatString, string InNotes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			XmlTextWriter xtw = null;

            try
			{
                xtw = new XmlTextWriter(InFileName, Encoding.UTF8);

				xtw.Formatting = Formatting.Indented;
				xtw.WriteStartDocument();
				xtw.WriteStartElement("EasiSlides");
				xtw.WriteStartElement("ListItem");
				WriteXMLSessionHeader(ref xtw, InFormatString, InNotes);
				string text = "";
				string text2 = "";
				string value = "";
				string text3 = "";
				int num = SaveAllItems ? InList.Items.Count : 0;
				for (int i = 1; i <= num; i++)
				{
					text3 = "";
					switch (InMode)
					{
						case UsageMode.Worship:
							{
								text = DataUtil.Trim(InList.Items[i - 1].SubItems[1].Text);
								string text4 = DataUtil.Left(text, 1);
								switch (text4)
								{
									case "D":
										text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].Text));
										if (DataUtil.Right(text2, " <Error - Item Not Found>".Length) == " <Error - Item Not Found>")
										{
											text2 = DataUtil.Left(text2, text2.Length - " <Error - Item Not Found>".Length);
										}
										text3 = InList.Items[i - 1].SubItems[7].Text;
										break;
									case "B":
										text2 = DataUtil.Trim(InList.Items[i - 1].Text);
										text = "B" + DataUtil.Mid(text, 1);
										break;
									default:
										text2 = DataUtil.Mid(text, 1);
										text = text4 + "1";
										break;
								}
								value = DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text);
								break;
							}
						case UsageMode.PraiseBook:
							text = DataUtil.Trim(InList.Items[i - 1].SubItems[3].Text);
							text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text));
							value = InList.Items[i - 1].SubItems[5].Text;
							break;
					}
					xtw.WriteStartElement("Item");
					xtw.WriteElementString("ItemID", text);
					xtw.WriteElementString("Title1", text2);
					xtw.WriteElementString("Folder", text3);
					xtw.WriteElementString("FormatData", value);
					xtw.WriteEndElement();
				}
				xtw.WriteEndDocument();
				xtw.Flush();
                xtw.Dispose();


                return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex);

				if (xtw!= null)
                    xtw.Dispose();

                return false;
			}
		}

		public static void SaveIndexFileOld(string OutFileName, ref ListView InList, UsageMode InMode)
		{
			BinaryWriter w = null;

            try
			{
				using FileStream fileStream = new FileStream(OutFileName, FileMode.Create);
				
				w = new BinaryWriter(fileStream);
				
				w.Write(byte.MaxValue);
				w.Write((byte)254);

				string inString = "[";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0001' + "=" + Convert.ToString(320) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0002' + "=" + Convert.ToString(PanelBackColour.ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0003' + "=" + Convert.ToString(PanelBackColourTransparent) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0004' + "=" + Convert.ToString(PanelTextColour.ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0005' + "=" + Convert.ToString(PanelTextColourAsRegion1) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0006' + "=" + Convert.ToString(ShowDataDisplayMode + ShowDataDisplaySlides * 2 + ShowDataDisplaySongs * 4 + ShowDataDisplayTitle * 8 + ShowDataDisplayCopyright * 16 + ShowDataDisplayPrevNext * 32) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\a' + "=" + SystemID + ":" + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\b' + "=" + Convert.ToString(ShowSongHeadings + UseShadowFont * 2 + ShowNotations * 4 + ShowCapoZero * 8 + ShowInterlace * 16 + UseOutlineFont * 32) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\t' + "=" + Convert.ToString(ShowLyrics) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\n' + "=" + Convert.ToString(ShowScreenColour[0].ToArgb()) + ":" + Convert.ToString(ShowScreenColour[1].ToArgb()) + ":" + ShowScreenStyle.ToString() + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\f' + "=" + Convert.ToString(ShowFontColour[0].ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\r' + "=" + Convert.ToString(ShowFontColour[1].ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u000e' + "=" + Convert.ToString(ShowFontAlign[0, 0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u000f' + "=" + Convert.ToString(ShowFontAlign[0, 1]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001a' + "=" + BackgroundPicture + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001b' + "=" + Convert.ToString((int)BackgroundMode) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001c' + "=" + Convert.ToString(ShowVerticalAlign) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001d' + "=" + Convert.ToString(ShowLeftMargin[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001e' + "=" + Convert.ToString(ShowRightMargin[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001f' + "=" + Convert.ToString(ShowBottomMargin[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '!' + "=" + GlobalImageCanvas.GetTransitionText(ShowItemTransition) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '"' + "=" + GlobalImageCanvas.GetTransitionText(ShowSlideTransition) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				for (int i = 0; i <= 6; i++)
				{
					int num = 141 + i * 5;
					inString = (char)num + "=" + Convert.ToString(PB_ShowWords[i] + PB_WordsBold[i] * 2 + PB_WordsItalic[i] * 4 + PB_WordsUnderline[i] * 8) + '>';
					FileUtil.WriteStringToBinaryFile(ref w, inString);
					inString = (char)(num + 1) + "=" + Convert.ToString(PB_WordsSize[i]) + '>';
					FileUtil.WriteStringToBinaryFile(ref w, inString);
				}
				inString = 'µ' + "=" + Convert.ToString(PB_ShowHeadings[0] + PB_ShowHeadings[1] * 2 + PB_ShowHeadings[2] * 4 + PB_ShowHeadings[3] * 8) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '¶' + "=" + Convert.ToString(PB_LyricsPattern) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '·' + "=" + Convert.ToString(PB_ShowSection) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u00b8' + "=" + Convert.ToString(PB_ShowColumns) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '¹' + "=" + Convert.ToString(PB_Spacing[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = 'º' + "=" + Convert.ToString(PB_Spacing[1]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '»' + "=" + Convert.ToString(PB_ShowScreenBreaks) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = 'Ã' + "=" + Convert.ToString(PB_CJKGroupStyle) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = 'Ä' + "=" + Convert.ToString(PB_ShowNotations + PB_ShowTiming * 2 + PB_ShowKey * 4 + PB_ShowCapo * 8 + PB_CapoZero * 16) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = "]";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				string text = "";
				string text2 = "";
				string text3 = "";
				for (int i = 1; i <= InList.Items.Count; i++)
				{
					if (InMode == UsageMode.Worship)
					{
						text = DataUtil.Trim(InList.Items[i - 1].SubItems[1].Text);
						string a = DataUtil.Left(text, 1);
						if (a == "D")
						{
							text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].Text));
							if (DataUtil.Right(text2, " <Error - Item Not Found>".Length) == " <Error - Item Not Found>")
							{
								text2 = DataUtil.Left(text2, text2.Length - " <Error - Item Not Found>".Length);
							}
							text2 += ((InList.Items[i - 1].SubItems[7].Text == "") ? "" : (":" + InList.Items[i - 1].SubItems[7].Text));
						}
						else if (a == "P")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "P1";
						}
						else if (a == "B")
						{
							text2 = DataUtil.Trim(InList.Items[i - 1].Text);
							text = "B" + DataUtil.Mid(text, 1);
						}
						else if (a == "T")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "T1";
						}
						else if (a == "I")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "I1";
						}
						else if (a == "W")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "W1";
						}
						else if (a == "M")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "M1";
						}
						text3 = DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text);
					}
					else
					{
						text = DataUtil.Trim(InList.Items[i - 1].SubItems[3].Text);
						text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text));
						text3 = InList.Items[i - 1].SubItems[5].Text;
					}
					inString = text + "\\" + text2 + "\\" + '*' + text3 + '>';
					FileUtil.WriteStringToBinaryFile(ref w, inString);
				}
				w.Flush();
				w.Dispose();
				//fileStream.Close();
			}
			catch
			{
				if(w != null)
					w.Dispose();

            }
		}

		public static string LoadTextFile(string InFileName)
		{
			return LoadTextFile(InFileName, ShowErrorMsg: false);
		}

		public static string LoadTextFile(string InFileName, bool ShowErrorMsg)
		{
			try
			{
				string text = "";

      			//StreamReader streamReader = new StreamReader(InFileName, Encoding.GetEncoding(1252));
				using StreamReader streamReader = new StreamReader(InFileName, Encoding.Default, detectEncodingFromByteOrderMarks: true);
				text = streamReader.ReadToEnd();
				//streamReader.Close();
				return text.Replace("\r\n", "\n").Replace("\v", "\n");
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);

				if (ShowErrorMsg)
				{
					MessageBox.Show("Cannot open the selected text file. The text file might be corrupted");
				}
				return "";
			}
		}

		public static void Load32InfoFile(string InFileName, ref SongSettings InItem, ref string[] ThisHeaderData)
		{
			try
			{
				string text = "";
				using StreamReader streamReader = File.OpenText(InFileName);
				text = streamReader.ReadToEnd();
				//streamReader.Close();
				string InfoFolder = "";
				string InfoHeading = "";
				string InfoRotate = "";
				int num = text.IndexOf("[");
				int num2 = text.IndexOf("]", num + 1);
				if (num == 0 && num2 > 1)
				{
					num++;
					string InFormatString = DataUtil.Mid(text, num, num2 - num);
					if (DataUtil.Convertv32FormatString(ref InFormatString, '*', ref InfoHeading, ref InfoFolder, ref InfoRotate) == 320)
					{
						InItem.CompleteLyrics = DataUtil.Mid(text, num2 + 1, text.Length - num2);
						InItem.Title = InfoHeading;
						InItem.FolderNo = DataUtil.StringToInt(InfoFolder);
						if (InItem.FolderNo < 1)
						{
							InItem.FolderNo = 1;
						}
						InItem.RotateString = InfoRotate;
					}
				}
			}
			catch
			{
			}
		}

		public static void LoadInfoFile(string InFileName, ref SongSettings InItem, ref string[] ThisHeaderData)
		{
			try
			{
				XmlTextReader reader = new XmlTextReader(InFileName);
				try
				{
					bool flag = false;
					if (ValidateEasiSlidesXML(ref reader))
					{
						string itemID = InItem.ItemID;
						ExtractEasiSlidesXMLItem(ref reader, ref InItem);
						InItem.ItemID = itemID;
					}
					else
					{
						Load32InfoFile(InFileName, ref InItem, ref ThisHeaderData);
					}
				}
				catch
				{
				}
				reader?.Close();
			}
			catch
			{
			}
		}

		public static void OldSaveInfoFile(string InFullFileName, string InText, string[] ThisHeaderData)
		{
			try
			{
				using FileStream fileStream = new FileStream(InFullFileName, FileMode.Create);
				BinaryWriter w = new BinaryWriter(fileStream);
				w.Write(byte.MaxValue);
				w.Write((byte)254);
				string inString = "[";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0001' + "=" + 320.ToString() + '*'.ToString();
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				for (int i = 2; i <= 254; i++)
				{
					if (ThisHeaderData[i] != "" && ThisHeaderData[i] != null)
					{
						inString = (char)i + "=" + ThisHeaderData[i] + '*'.ToString();
						FileUtil.WriteStringToBinaryFile(ref w, inString);
					}
				}
				inString = "]";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = InText;
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				w.Close();
				//fileStream.Close();
			}
			catch
			{
			}
		}

		public static void PreLoadPowerpointFiles(ref PowerPoint InPP, ref string[,] InSongsArray)
		{
			bool flag = false;
			InPP.isLive = true;
			InPP.isEditable = false;
			for (int i = 0; i <= TotalWorshipListItems; i++)
			{
				try
				{
					if (InSongsArray[i, 1] == "P")
					{
						if (!flag)
						{
							InPP.NewApp();
							//InPP.newPowerPointSlideApp();
							flag = true;
						}
						InPP.Open(DataUtil.Right(InSongsArray[i, 0], InSongsArray[i, 0].Length - 1), ref PowerpointList, ref TotalPowerpointItems);
						// ?�렇�??�주지 ?�으�??�리?�테?�션창이 Nomal�??�다
						InPP.prePowerPointApp.Activate();
						InPP.prePowerPointApp.WindowState = NetOffice.PowerPointApi.Enums.PpWindowState.ppWindowMinimized;
						//InPP.newOpen(DataUtil.Right(InSongsArray[i, 0], InSongsArray[i, 0].Length - 1), ref PowerpointList, ref TotalPowerpointItems);
					}
				}
				catch
				{
				}
			}
		}

    }
}
