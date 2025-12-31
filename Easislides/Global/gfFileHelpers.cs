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

        public static void ReleasePowerpointApp(object pptApp)
        {
            if (pptApp == null) return;

            try
            {
                // If it's our wrapper type, use its API to quit and release the underlying COM Application
                if (pptApp is OfficeLib.PowerPoint wrapper)
                {
                    try
                    {
                        var app = wrapper.prePowerPointApp;
                        if (app != null && !app.IsDisposed)
                        {
                            try
                            {
                                app.Quit();
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"ReleasePowerpointApp - Quit failed: {ex.Message}");
                            }

                            try
                            {
                                if (Marshal.IsComObject(app))
                                {
                                    Marshal.FinalReleaseComObject(app);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"ReleasePowerpointApp - FinalReleaseComObject failed: {ex.Message}");
                            }
                            wrapper.prePowerPointApp = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"ReleasePowerpointApp - wrapper handling failed: {ex.Message}");
                    }
                }
                else
                {
                    // Fallback: caller provided a raw COM application or interop object
                    dynamic app = pptApp;
                    try
                    {
                        app.Quit();
                    }
                    catch
                    {
                        try
                        {
                            app.Close();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"ReleasePowerpointApp - Close failed: {ex.Message}");
                        }
                    }
                }
            }
            finally
            {
                // Only try to release the object itself if it is an RCW (safe to swallow errors)
                try
                {
                    if (Marshal.IsComObject(pptApp))
                    {
                        Marshal.FinalReleaseComObject(pptApp);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ReleasePowerpointApp - final release failed: {ex.Message}");
                }
            }

            // Note: Removed aggressive GC.Collect() calls for better performance.
            // COM objects will be cleaned up by the GC in its normal cycle.
            // If immediate cleanup is critical, caller should trigger GC at batch operation end.
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

        /// <summary>
        /// Release PowerPoint COM objects before deleting temp files.
        /// Call this before DeleteFolderFilesSafe if PowerPoint may be holding file locks.
        /// </summary>
        public static void ReleasePowerpointObjectsForCleanup()
        {
            try
            {
                if (gf.LivePP != null)
                {
                    ReleasePowerpointApp(gf.LivePP);
                    gf.LivePP = null;
                }
                if (gf.PreviewPPT != null)
                {
                    ReleasePowerpointApp(gf.PreviewPPT);
                    gf.PreviewPPT = null;
                }
                if (gf.OutputPPT != null)
                {
                    ReleasePowerpointApp(gf.OutputPPT);
                    gf.OutputPPT = null;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ReleasePowerpointObjectsForCleanup failed: {ex.Message}");
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
            if (gf.TotalMediaFileExt == 0)
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
            for (int i = 0; i < gf.TotalMediaFileExt; i++)
            {
                if (InMediaType == MediaBackgroundStyle.None || gf.MediaFileExtension[i, 1] == InMediaType.ToString())
                {
                    if (!isFirst)
                    {
                        displayName.Append(',');
                        extensions.Append(';');
                    }

                    string ext = "*" + gf.MediaFileExtension[i, 0];
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
                    title = gf.RemoveMusicSym(DataUtil.Trim(item.Text));
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
            title = gf.RemoveMusicSym(DataUtil.Trim(item.SubItems[2].Text));
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
                gf.WriteXMLSessionHeader(ref xtw, InFormatString, InNotes);

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
                if (gf.ValidateEasiSlidesXML(ref reader))
                {
                    string itemID = InItem.ItemID;
                    gf.ExtractEasiSlidesXMLItem(ref reader, ref InItem);
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
            InPP.isLive = true;
            InPP.isEditable = false;
            for (int i = 0; i <= gf.TotalWorshipListItems; i++)
            {
                try
                {
                    if (InSongsArray[i, 1] == "P")
                    {
                        if (!flag)
                        {
                            InPP.NewApp();
                            flag = true;
                        }
                        InPP.Open(DataUtil.Right(InSongsArray[i, 0], InSongsArray[i, 0].Length - 1), ref gf.PowerpointList, ref gf.TotalPowerpointItems);
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
        }
    }
}
