using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using Easislides.Util;
using OfficeLib;

namespace Easislides
{
    static class gfFileHelpers
    {
        // Retry configuration constants
        private const int DEFAULT_DELETE_RETRIES = 5;
        private const int DEFAULT_RETRY_DELAY_MS = 100;

        public static bool TryDeleteWithRetries(string filePath, int retries = DEFAULT_DELETE_RETRIES, int delayMs = DEFAULT_RETRY_DELAY_MS)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(delayMs);
                }
                catch (UnauthorizedAccessException)
                {
                    Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"TryDeleteWithRetries unexpected: {ex.Message}");
                    Thread.Sleep(delayMs);
                }
            }
            return false;
        }

        public static Image LoadImageNoLock(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var img = Image.FromStream(fs))
                {
                    // clone into a new Bitmap so the stream can be closed and the file unlocked
                    return new Bitmap(img);
                }
            }
        }

        public static void DeleteFolderFilesSafe(string inFolder)
        {
            if (string.IsNullOrEmpty(inFolder) || !Directory.Exists(inFolder)) return;

            try
            {
                var files = Directory.GetFiles(inFolder);
                foreach (var path in files)
                {
                    try
                    {
                        if (!TryDeleteWithRetries(path))
                        {
                            Trace.WriteLine($"ERROR : Could not delete file after retries: {path}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
                    }
                }

                var directories = Directory.GetDirectories(inFolder);
                foreach (var dir in directories)
                {
                    try
                    {
                        DeleteFolderFilesSafe(dir);
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

        public static bool LoadFileContents(string InFileName, ref string InString)
        {
            InString = LoadTextFile(InFileName, ShowErrorMsg: false);
            return !string.IsNullOrEmpty(InString);
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
            if (Gf.TotalMediaFileExt == 0)
            {
                return "";
            }

            StringBuilder displayName = new StringBuilder();
            StringBuilder extensions = new StringBuilder();

            switch (InMediaType)
            {
                case MediaBackgroundStyle.Audio:
                    displayName.Append("Audio Files (");
                    break;
                case MediaBackgroundStyle.Video:
                    displayName.Append("Video Files (");
                    break;
            }

            bool isFirst = true;
            for (int i = 0; i < Gf.TotalMediaFileExt; i++)
            {
                if (InMediaType == MediaBackgroundStyle.None || Gf.MediaFileExtension[i, 1] == InMediaType.ToString())
                {
                    if (!isFirst)
                    {
                        displayName.Append(',');
                        extensions.Append(';');
                    }

                    string ext = "*" + Gf.MediaFileExtension[i, 0];
                    displayName.Append(ext);
                    extensions.Append(ext);
                    isFirst = false;
                }
            }

            if (isFirst)
            {
                return "";
            }

            if (InMediaType != MediaBackgroundStyle.None)
            {
                displayName.Append(')');
            }
            else
            {
                displayName.Append("Media Files (all types)");
            }

            return $"{displayName}|{extensions}";
        }

        private const string ERROR_ITEM_NOT_FOUND = " <Error - Item Not Found>";

        private static void ExtractWorshipItemData(ListViewItem item, out string itemID, out string title, out string folder, out string formatData)
        {
            itemID = DataUtil.Trim(item.SubItems[1].Text);
            string itemType = DataUtil.Left(itemID, 1);

            switch (itemType)
            {
                case "D":
                    title = Gf.RemoveMusicSym(DataUtil.Trim(item.Text));
                    if (DataUtil.Right(title, ERROR_ITEM_NOT_FOUND.Length) == ERROR_ITEM_NOT_FOUND)
                    {
                        title = DataUtil.Left(title, title.Length - ERROR_ITEM_NOT_FOUND.Length);
                    }
                    folder = item.SubItems[7].Text;
                    break;
                case "B":
                    title = DataUtil.Trim(item.Text);
                    itemID = "B" + DataUtil.Mid(itemID, 1);
                    folder = "";
                    break;
                default:
                    title = DataUtil.Mid(itemID, 1);
                    itemID = itemType + "1";
                    folder = "";
                    break;
            }
            formatData = DataUtil.Trim(item.SubItems[2].Text);
        }

        private static void ExtractPraiseBookItemData(ListViewItem item, out string itemID, out string title, out string folder, out string formatData)
        {
            itemID = DataUtil.Trim(item.SubItems[3].Text);
            title = Gf.RemoveMusicSym(DataUtil.Trim(item.SubItems[2].Text));
            folder = "";
            formatData = item.SubItems[5].Text;
        }

        public static bool SaveIndexFile(string InFileName, ref ListView InList, UsageMode InMode, bool SaveAllItems, string InFormatString, string InNotes)
        {
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(InFileName, Encoding.UTF8);
                xtw.Formatting = Formatting.Indented;
                xtw.WriteStartDocument();
                xtw.WriteStartElement("EasiSlides");
                xtw.WriteStartElement("ListItem");
                Gf.WriteXMLSessionHeader(ref xtw, InFormatString, InNotes);

                int itemCount = SaveAllItems ? InList.Items.Count : 0;

                for (int i = 1; i <= itemCount; i++)
                {
                    var item = InList.Items[i - 1];
                    string itemID, title, folder, formatData;

                    switch (InMode)
                    {
                        case UsageMode.Worship:
                            ExtractWorshipItemData(item, out itemID, out title, out folder, out formatData);
                            break;
                        case UsageMode.PraiseBook:
                            ExtractPraiseBookItemData(item, out itemID, out title, out folder, out formatData);
                            break;
                        default:
                            continue;
                    }

                    xtw.WriteStartElement("Item");
                    xtw.WriteElementString("ItemID", itemID);
                    xtw.WriteElementString("Title1", title);
                    xtw.WriteElementString("Folder", folder);
                    xtw.WriteElementString("FormatData", formatData);
                    xtw.WriteEndElement();
                }
                xtw.WriteEndDocument();
                xtw.Flush();

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SaveIndexFile failed: {ex.Message}");
                Trace.WriteLine(ex.StackTrace);
                try
                {
                    string logFile = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "EasiSlides_Debug.log");
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] SaveIndexFile EXCEPTION: {ex.GetType().Name}: {ex.Message}\n    File: {InFileName}\n    StackTrace: {ex.StackTrace}{Environment.NewLine}";
                    System.IO.File.AppendAllText(logFile, logMessage);
                }
                catch { }
                return false;
            }
            finally
            {
                xtw?.Dispose();
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

                using StreamReader streamReader = new StreamReader(InFileName, Encoding.Default, detectEncodingFromByteOrderMarks: true);
                text = streamReader.ReadToEnd();
                return text.Replace("\r\n", "\n").Replace("\v", "\n");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"LoadTextFile failed: {e.Message}");
                Trace.WriteLine(e.StackTrace);

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
            catch (Exception ex)
            {
                Trace.WriteLine($"Load32InfoFile failed: {ex.Message}");
                Trace.WriteLine(ex.StackTrace);
            }
        }

        public static void LoadInfoFile(string InFileName, ref SongSettings InItem, ref string[] ThisHeaderData)
        {
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(InFileName);
                if (Gf.ValidateEasiSlidesXML(ref reader))
                {
                    string itemID = InItem.ItemID;
                    Gf.ExtractEasiSlidesXMLItem(ref reader, ref InItem);
                    InItem.ItemID = itemID;
                }
                else
                {
                    Load32InfoFile(InFileName, ref InItem, ref ThisHeaderData);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LoadInfoFile failed: {ex.Message}");
                Trace.WriteLine(ex.StackTrace);
            }
            finally
            {
                reader?.Close();
            }
        }

        public static void PreLoadPowerpointFiles(ref PowerPoint InPP, ref string[,] InSongsArray)
        {
            bool flag = false;
            int failedCount = 0;
            string firstFailed = null;
            InPP.isLive = true;
            InPP.isEditable = false;
            for (int i = 0; i <= Gf.TotalWorshipListItems; i++)
            {
                try
                {
                    if (InSongsArray[i, 1] == "P")
                    {
                        if (!flag)
                        {
                            InPP.NewApp();
                            // The main form's wrapper is made once at start-up; when PowerPoint was closed or
                            // restarted since, it must not stay on the dead process while this one attached to
                            // the new one. One cross-process call when it is fine.
                            PowerPoint mainPPT = Gf.MainFormPPT;
                            if (mainPPT != null && !ReferenceEquals(mainPPT, InPP))
                            {
                                if (mainPPT.EnsureAttached(out bool attachFailed))
                                {
                                    FrmMain.WriteDebugLog("PreLoadPowerpointFiles: the main form's PowerPoint reference no longer answered; attached it again");
                                }
                                else if (attachFailed)
                                {
                                    FrmMain.WriteDebugLog("PreLoadPowerpointFiles: the main form's PowerPoint reference no longer answered and could not be attached again");
                                    if (Gf.FirstAttachFailureReport())
                                    {
                                        FrmMain.ReportErrorNotice("PowerPoint could not be reached from the main window: its slide keys may not move the slide show on the projector");
                                    }
                                }
                            }
                            flag = true;
                        }
                        string path = DataUtil.Right(InSongsArray[i, 0], InSongsArray[i, 0].Length - 1);
                        string result = InPP.Open(path, ref Gf.PowerpointList, ref Gf.TotalPowerpointItems);
                        if (!string.IsNullOrEmpty(result))
                        {
                            // Not opened: the item cannot start its slide show during this show. Each file is
                            // reported once per show (a mid-show pre-load tries it again).
                            FrmMain.WriteDebugLog("PreLoadPowerpointFiles: '" + path + "' was not opened: " + result);
                            if (Gf.FirstPreloadFailureReport(path))
                            {
                                failedCount++;
                                firstFailed ??= path;
                            }
                        }
                        InPP.prePowerPointApp.Activate();
                        InPP.prePowerPointApp.WindowState = NetOffice.PowerPointApi.Enums.PpWindowState.ppWindowMinimized;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"PreLoadPowerpointFiles failed: {ex.Message}");
                    Trace.WriteLine(ex.StackTrace);
                }
            }
            if (failedCount > 0)
            {
                // One notice per pre-load.
                string name = Path.GetFileName(firstFailed);
                FrmMain.ReportErrorNotice(failedCount == 1
                    ? "A PowerPoint file could not be opened for the show: " + name
                    : failedCount + " PowerPoint files could not be opened for the show (first: " + name + ")");
            }
        }
    }
}
