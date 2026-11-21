using Axicultus.Properties;
using OfficeLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Axicultus.Util;
using System.Threading;
using Axicultus.SQLite;
using Axicultus.Module;
using Axicultus.Global;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Type = System.Type;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Resources;
using System.Collections;
using System.Drawing.Imaging;
using System.ComponentModel.Design;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Axicultus;

public partial class FrmMain : Form
{
    public static void Extract(string resxPath, string outputDir)
    {
        Console.WriteLine($"Processing: {resxPath}");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        using (ResXResourceReader reader = new(resxPath))
        {
            reader.UseResXDataNodes = true; // entries come back as ResXDataNode, resolved one by one below

            foreach (DictionaryEntry entry in reader)
            {
                string key = entry.Key.ToString();
                ResXDataNode node = (ResXDataNode)entry.Value;

                try
                {
                    // Resolve the actual value
                    object value = node.GetValue((ITypeResolutionService)null);

                    if (value == null) continue;

                    // 1. Plain image (Bitmap)
                    if (value is Bitmap bmp)
                    {
                        SaveImage(bmp, outputDir, key, ImageFormat.Png);
                    }
                    // 2. Icon (e.g. $this.Icon)
                    else if (value is Icon icon)
                    {
                        // Saved as .ico rather than converted to a bitmap
                        using (FileStream fs = new(Path.Combine(outputDir, $"{key}.ico"), FileMode.Create))
                        {
                            icon.Save(fs);
                        }
                        Console.WriteLine($"[Icon] Extracted: {key}.ico");
                    }
                    // 3. Image list stream (e.g. imageListSys.ImageStream)
                    else if (value is ImageListStreamer streamer)
                    {
                        using ImageList imgList = new();
                        imgList.ImageStream = streamer;
                        for (int i = 0; i < imgList.Images.Count; i++)
                        {
                            string subName = $"{key}_{i}";
                            SaveImage(imgList.Images[i], outputDir, subName, ImageFormat.Png);
                        }
                        Console.WriteLine($"[ImageList] Extracted {imgList.Images.Count} images from {key}");
                    }
                    // 4. Anything else (e.g. TrayLocation)
                    else
                    {
                        // Point, Size and other non-image values are skipped
                        // Console.WriteLine($"[Skip] {key} is type {value.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to extract {key}: {ex.Message}");
                }
            }
        }
        Console.WriteLine("Extraction Complete.");
    }

    private static void SaveImage(Image img, string dir, string name, ImageFormat format)
    {
        // Replace the characters a file name cannot contain
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        string ext = format == ImageFormat.Png ? "png" : "jpg";
        string path = Path.Combine(dir, $"{name}.{ext}");

        img.Save(path, format);
        Console.WriteLine($"[Image] Extracted: {name}.{ext}");
    }

    private void SaveFormStateToRegistry()
    {
        // A status bar shown only for the error notice is hidden first: the splitter distances
        // saved below depend on it, and they must be those of the layout the operator chose.
        HideStatusStripShownForNotice();
        switch (base.WindowState)
        {
            case FormWindowState.Maximized:
                RegUtil.SaveRegValue("settings", "MainMax", 1);
                RegUtil.SaveRegValue("settings", "MainLeft", base.RestoreBounds.Left);
                RegUtil.SaveRegValue("settings", "MainTop", base.RestoreBounds.Top);
                RegUtil.SaveRegValue("settings", "MainWidth", base.RestoreBounds.Width);
                RegUtil.SaveRegValue("settings", "MainHeight", base.RestoreBounds.Height);
                break;
            case FormWindowState.Normal:
                RegUtil.SaveRegValue("settings", "MainMax", 0);
                RegUtil.SaveRegValue("settings", "MainLeft", base.Left);
                RegUtil.SaveRegValue("settings", "MainTop", base.Top);
                RegUtil.SaveRegValue("settings", "MainWidth", base.Width);
                RegUtil.SaveRegValue("settings", "MainHeight", base.Height);
                break;
            case FormWindowState.Minimized:
                Gf.SaveConfigSettings();
                return;
        }
        RegUtil.SaveRegValue("settings", Splitter_FolderWidth, splitContainerMain.SplitterDistance);
        RegUtil.SaveRegValue("settings", Splitter_OutputWidth, splitContainer2.SplitterDistance);
        RegUtil.SaveRegValue("settings", Splitter_FolderHeight, splitContainer1.SplitterDistance);
        RegUtil.SaveRegValue("settings", Splitter_PreviewLyricsHeight, splitContainerPreview.SplitterDistance);
        RegUtil.SaveRegValue("settings", Splitter_outputLyricsHeight, splitContainerOutput.SplitterDistance);
        Gf.SaveConfigSettings();
    }

    private void Def_SetNoImage()
    {
        Gf.BackgroundPicture = "";
        ApplyDefaultData(StartAtFirstSlide: false);
        Gf.SetShowBackground(Gf.OutputItem, ref OutputScreen);
        RefreshSlidesFonts(ref Gf.OutputItem);
        UpdateDefaultNoImageButton();
    }

    private void LoadWorshipListTemplate()
    {
        openFileDialog1.Filter = "Axicultus Template File (*.est)|*.est";
        openFileDialog1.Title = "Load Session Settings from a Template";
        openFileDialog1.InitialDirectory = Gf.WorshipTemplatesDir;
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            string fileName = openFileDialog1.FileName;
            if (LoadWorshipList(1, fileName).Length == 0) return;
            SetMainDefaultBackScreen();
            UpdateDisplayPanelData(RefreshSlides: true);
        }
    }

    private void SaveWorshipListTemplate(string InFileName, string InitDirectory)
    {
        saveFileDialog1.Filter = "Axicultus Template File (*.est)|*.est";
        saveFileDialog1.Title = "Save Current Session Settings to a Template";
        saveFileDialog1.InitialDirectory = InitDirectory;
        saveFileDialog1.FileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true);
        saveFileDialog1.OverwritePrompt = true;
        saveFileDialog1.AddExtension = true;
        saveFileDialog1.DefaultExt = ".est";
        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
        {
            string fileName = saveFileDialog1.FileName;
            try
            {
                gfFileHelpers.SaveIndexFile(fileName, ref WorshipListItems, UsageMode.Worship, SaveAllItems: false, "", "");
            }
            catch
            {
                MessageBox.Show("Error Saving File, please make sure you have write access and try again");
            }
        }
    }

    private void ClearFormatPicture()
    {
        Gf.PreviewItem.Format.BackgroundPicture = "";
        Gf.PreviewItem.Format.TempImageFileName = "";
        Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen, FallBackToDefault: false);
        UpdateFormatData(StartAtFirstSlide: false);
        Ind_NoImage.Enabled = false;
    }

    private void ApplyIndividualFormat(ref SongSettings InItem)
    {
        ApplyIndividualFormat(ref InItem, "D");
    }

    private void ApplyIndividualFormat(ref SongSettings InItem, string InItemSym)
    {
        ApplyIndividualFormat(ref InItem, InItemSym, 0);
    }

    private void ApplyIndividualFormat(ref SongSettings InItem, string InItemSym, int FNum)
    {
        Ind_checkBox.Checked = true;
        if (Gf.PreviewItem.Source == ItemSource.WorshipList)
        {
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            if (selectedIndex < 0)
            {
                Ind_checkBox.Checked = false;
                return;
            }
            bool formatWasEmpty = false;
            Gf.SongFormatData = "";
            Gf.PreviewItem.UseDefaultFormat = false;
            if (Gf.PreviewItem.Type == "I")
            {
                WorshipListItems.Items[selectedIndex].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
            }
            if (WorshipListItems.Items[selectedIndex].SubItems[2].Text == "")
            {
                formatWasEmpty = true;
            }
            Gf.SongFormatData = WorshipListItems.Items[selectedIndex].SubItems[2].Text;
            Gf.LoadIndividualFormatData(ref Gf.PreviewItem, Gf.SongFormatData);
            UpdateFormatFields();
            if (formatWasEmpty)
            {
                UpdateFormatData();
            }
        }
        else if (Gf.PreviewItem.Source == ItemSource.SongsList)
        {
            Gf.PreviewItem.UseDefaultFormat = false;
        }
    }

    private void NoIndividualFormat()
    {
        Ind_checkBox.Checked = false;
        Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
        RefreshSlidesFonts(ref Gf.PreviewItem, ImageTransitionControl.TransitionAction.None);
        UpdateFormatFields();
        AllowIndividualFormat(AllowFormat: true, BoxChecked: false);
        if (Gf.PreviewItem.Source == ItemSource.WorshipList)
        {
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            Gf.PreviewItem.UseDefaultFormat = true;
            bool hadFormat = false;
            if (selectedIndex >= 0)
            {
                if (WorshipListItems.Items[selectedIndex].SubItems[2].Text != "")
                {
                    hadFormat = true;
                }
                WorshipListItems.Items[selectedIndex].SubItems[2].Text = "";
            }
            if (hadFormat)
            {
                DisplayLyrics(Gf.PreviewItem, 1);
                SaveWorshipList();
            }
        }
        else if (Gf.PreviewItem.Source == ItemSource.SongsList)
        {
            Gf.PreviewItem.UseDefaultFormat = true;
            DisplayLyrics(Gf.PreviewItem, 1);
        }
    }

    private void LoadIndividualTemplate()
    {
        openFileDialog1.Filter = "Axicultus Template File (*.est)|*.est";
        openFileDialog1.Title = "Load Individual Settings from a Template";
        openFileDialog1.InitialDirectory = Gf.SettingsTemplatesDir;
        if (openFileDialog1.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        string fileName = openFileDialog1.FileName;
        string templateFormat = LoadWorshipList(2, fileName);
        Gf.PreviewItem.Format.FormatString = templateFormat;
        string type = Gf.PreviewItem.Type;
        var takesTemplate = type switch
        {
            "D" or "B" or "T" or "I" or "W" => true,
            _ => type == "M",
        };
        if (!takesTemplate)
        {
            return;
        }
        if (Gf.PreviewItem.Source == ItemSource.SongsList)
        {
            Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, templateFormat);
        }
        else if (Gf.PreviewItem.Source == ItemSource.WorshipList)
        {
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            if (selectedIndex < 0)
            {
                return;
            }
            WorshipListItems.Items[selectedIndex].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
            SaveWorshipList();
        }
        Gf.LoadIndividualFormatData(ref Gf.PreviewItem, templateFormat);
        if (type == "I")
        {
            Gf.PreviewItem.Format.TempImageFileName = Gf.PreviewItem.Format.BackgroundPicture;
            SaveInfoFilePreview(ReloadImageData: true);
        }
        SetItemFontSettings(ref Gf.PreviewItem);
        Gf.FormatDisplayLyrics(ref Gf.PreviewItem, PrepareSlides: true, UseStoredSequence: true);
        AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
        UpdateFormatFields();
        BuildVerseButtons(Gf.PreviewItem);
        DisplayLyrics(Gf.PreviewItem, 0, ScrollToCaret: true);
        DisplayItemInfo(Gf.PreviewItem, ref PreviewInfo);
    }

    private void SaveIndividualTemplate(string InTitle)
    {
        InTitle = Gf.MakeTitleValidFileName(InTitle);
        saveFileDialog1.Filter = "Axicultus Template File (*.est)|*.est";
        saveFileDialog1.Title = "Save Current Individual Settings to a Template";
        saveFileDialog1.InitialDirectory = Gf.SettingsTemplatesDir;
        saveFileDialog1.FileName = InTitle;
        saveFileDialog1.OverwritePrompt = true;
        saveFileDialog1.AddExtension = true;
        saveFileDialog1.DefaultExt = ".est";
        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
        {
            string fileName = saveFileDialog1.FileName;
            try
            {
                gfFileHelpers.SaveIndexFile(fileName, ref WorshipListItems, UsageMode.Worship, SaveAllItems: false, Gf.PreviewItem.Format.FormatString, "");
            }
            catch
            {
                MessageBox.Show("Error Saving File, please make sure you have write access and try again");
            }
        }
    }

    private static void AssignMediaText(ref ToolStripButton InButton, int InMediaOption)
    {
        InButton.Text = InMediaOption switch
        {
            1 => "Media: As Title",
            2 => "Media: Specific",
            3 => "Media: Live Feed",
            _ => "Media: None",
        };
    }

    private void PraiseBookList_Change()
    {
        if (DataUtil.Trim(PraiseBook.Text) == "")
        {
            toolStripPraiseBook1.Items[1].Enabled = false;
            toolStripPraiseBook1.Items[2].Enabled = false;
            toolStripPraiseBook2.Items[0].Enabled = false;
            toolStripPraiseBook2.Items[2].Enabled = false;
            toolStripPraiseBook2.Items[3].Enabled = false;
            PraiseBook.Items.Clear();
            return;
        }
        toolStripPraiseBook1.Items[1].Enabled = IsSelectedTab(tabControlSource, "tabFolders");
        toolStripPraiseBook1.Items[2].Enabled = true;
        toolStripPraiseBook2.Items[0].Enabled = true;
        toolStripPraiseBook2.Items[2].Enabled = true;
        toolStripPraiseBook2.Items[3].Enabled = true;
        if (!InitFormLoad)
        {
            Cursor = Cursors.WaitCursor;
            Gf.CurPraiseBook = DataUtil.Trim(PraiseBook.Text);
            LoadPraiseBook(0);
            PraiseBookIndexChanged();
            ShowStatusBarSummary();
            DisplayLyrics(Gf.PreviewItem, 0);
            Cursor = Cursors.Default;
        }
    }

    private void BuildPicturesFolderList()
    {
        Gf.PictureGroups.Clear();
        Gf.PictureGroups.Add(new FolderGroup { Name = "Scenery", Path = Gf.RootAxicultusDir + "Images\\" + "Scenery" + "\\" });
        Gf.PictureGroups.Add(new FolderGroup { Name = "Tiles", Path = Gf.RootAxicultusDir + "Images\\" + "Tiles" + "\\" });
        Gf.PictureGroups.Add(new FolderGroup { Name = "Images", Path = Gf.ImagesDir });
        string[] directories = Directory.GetDirectories(Gf.ImagesDir);
        if (directories.Length > 0)
        {
            Gf.SingleArraySort(directories);
        }
        string sceneryDir = Gf.RootAxicultusDir + "Images\\Scenery";
        string tilesDir = Gf.RootAxicultusDir + "Images\\Tiles";
        Gf.BuildSubFolderList(Gf.ImagesDir, Gf.ImagesDir, Gf.PictureGroups);
        ImagesFolder.Items.Clear();
        for (int i = 0; i < Gf.PictureGroups.Count; i++)
        {
            ImagesFolder.Items.Add(Gf.PictureGroups[i].Name);
        }
    }

    private void BuildInfoScreenFolderList()
    {
        Gf.InfoScreenGroups.Clear();
        Gf.InfoScreenGroups.Add(new FolderGroup { Name = "InfoScreen Items", Path = Gf.InfoScreenDir });
        Gf.BuildSubFolderList(Gf.InfoScreenDir, Gf.InfoScreenDir, Gf.InfoScreenGroups);
        InfoScreenFolder.Items.Clear();
        for (int i = 0; i < Gf.InfoScreenGroups.Count; i++)
        {
            InfoScreenFolder.Items.Add(Gf.InfoScreenGroups[i].Name);
        }
        if (InfoScreenFolder.Items.Count > 0)
        {
            InfoScreenFolder.SelectedIndex = 0;
        }
    }

    private void BuildPowerpointFolderList()
    {
        Gf.PowerpointGroups.Clear();
        Gf.PowerpointGroups.Add(new FolderGroup { Name = "Powerpoint Items", Path = Gf.PowerpointDir });
        foreach (var path in PowerPointSourceFiles.ListSubfolders(Gf.PowerpointDir))
            Gf.PowerpointGroups.Add(new FolderGroup
            {
                Name = "\\" + Path.GetRelativePath(Gf.PowerpointDir, path),
                Path = path + Path.DirectorySeparatorChar
            });
        var selected = SourceFileFolder.IncludeSelection(Gf.PowerpointGroups,
            RegUtil.GetRegValue("options", "PowerPointSourceFolder", ""), group => group.Path,
            path => new FolderGroup { Name = path, Path = Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar });
        PowerpointFolder.Items.Clear();
        for (int i = 0; i < Gf.PowerpointGroups.Count; i++)
        {
            PowerpointFolder.Items.Add(Gf.PowerpointGroups[i].Name);
        }
        if (PowerpointFolder.Items.Count > 0)
        {
            PowerpointFolder.SelectedIndex = selected;
        }
    }

    private void BuildMediaFolderList()
    {
        Gf.MediaGroups.Clear();
        Gf.MediaGroups.Add(new FolderGroup { Name = "Media Files", Path = Gf.MediaDir });
        Gf.BuildSubFolderList(Gf.MediaDir, Gf.MediaDir, Gf.MediaGroups);
        var selected = SourceFileFolder.IncludeSelection(Gf.MediaGroups,
            RegUtil.GetRegValue("options", "MediaSourceFolder", ""), group => group.Path,
            path => new FolderGroup { Name = path, Path = Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar });
        MediaFolder.Items.Clear();
        for (int i = 0; i < Gf.MediaGroups.Count; i++)
        {
            MediaFolder.Items.Add(Gf.MediaGroups[i].Name);
        }
        if (MediaFolder.Items.Count > 0)
        {
            MediaFolder.SelectedIndex = selected;
        }
    }

    private void SongFolder_Change()
    {
        if (ImplementFolderChange && SongFolder.Text != "")
        {
            if (SongFolder.Items[SongFolder.Items.Count - 1].ToString() == "Search Results:")
            {
                SongFolder.Items.RemoveAt(SongFolder.Items.Count - 1);
            }
            SongFolder.ForeColor = SongsList.ForeColor;
            bool findItemMediaOnly = Gf.FindItemMediaOnly;
            Gf.FindItemMediaOnly = false;
            bool findItemInContents = Gf.FindItemInContents;
            Gf.FindItemInContents = false;
            // A full folder load shows every song: the find box is emptied first (design D26).
            ResetSongFindForFolderLoad();
            SongsList.Items.Clear();
            SetSongListColWidth();
            if (SongFolder.Items.Count >= 1)
            {
                int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
                CurStyle = Gf.Folders[folderNumber].GroupStyle;
                Cursor = Cursors.WaitCursor;
                SetSortButton(CurStyle);
                SongsList.RightToLeft = ((Gf.Folders[folderNumber].FontRTL[0] > 0) ? RightToLeft.Yes : RightToLeft.No);
                SongsList.RightToLeftLayout = ((Gf.Folders[folderNumber].FontRTL[0] > 0) ? true : false);
                FillList(folderNumber, "", Gf.FindItemMediaOnly);
                Gf.FindItemMediaOnly = findItemMediaOnly;
                Gf.FindItemInContents = findItemInContents;
                ShowStatusBarSummary();
                Cursor = Cursors.Default;
            }
        }
    }

    private static void SetItemFontSettings(ref SongSettings InItem)
    {
        Gf.FormatText(ref InItem, Gf.PanelBackColour, Gf.PanelBackColourTransparent, Gf.PanelTextColour, Gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
    }

    private void ApplyWorshipMode()
    {
        Gf.AxicultusMode = UsageMode.Worship;
        ModeEnableItems(EnableWorshipMode: true);
        LoadWorshipList(1);
        WorshipListIndexChanged();
        ShowStatusBarSummary();
        DisplayLyrics(Gf.PreviewItem, 0);
    }

    private void ApplyPraisebookMode()
    {
        Cursor = Cursors.WaitCursor;
        Gf.AxicultusMode = UsageMode.PraiseBook;
        ModeEnableItems(EnableWorshipMode: false);
        if (PraiseBook.Text == "")
        {
            if (PraiseBook.Items.Count > 0)
            {
                PraiseBook.SelectedIndex = 0;
            }
        }
        else
        {
            LoadPraiseBook((PraiseBookItems.Items.Count != 0) ? 1 : 0);
        }
        PraiseBookIndexChanged();
        ShowStatusBarSummary();
        DisplayLyrics(Gf.PreviewItem, 0);
        Cursor = Cursors.Default;
    }

    private void PraiseBookIndexChanged()
    {
        int selectedIndex = Gf.GetSelectedIndex(PraiseBookItems);
        if (selectedIndex >= 0)
        {
            string itemId = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
            Gf.PreviewItem.CurItemNo = selectedIndex + 1;
            Gf.PreviewItem.TotalItems = PraiseBook.Items.Count;
            Gf.PreviewItem.Source = ItemSource.PraiseBook;
            LoadItem(ref Gf.PreviewItem, itemId);
        }
        else
        {
            Gf.PreviewItem.Type = "";
            Gf.PreviewItem.ItemID = "";
            ClearLyrics(flowLayoutPreviewLyrics);
        }
    }

    public void Load32PraiseBook(int DataType, string original = null)
    {
        if (DataUtil.Trim(PraiseBook.Text) != "")
        {
            ListViewItem listViewItem = new();
            string inFileName = Gf.PraiseBookDir + Gf.CurPraiseBook + ".json";
            if (original == null) gfFileHelpers.LoadFileContents(inFileName, ref InContents);
            else InContents = original;
            int headerLength = Gf.Load32HeaderData(inFileName, InContents, ref Gf.HeaderData);
            if (headerLength < 1)
            {
                Gf.ApplyHeaderData(1);
                InContents = "";
            }
            if (DataType == 1)
            {
                Gf.ApplyHeaderData(1);
                return;
            }
            PraiseBookItems.Items.Clear();
            Gf.ApplyHeaderData(1);
            InContents = DataUtil.Mid(InContents, headerLength + 1, InContents.Length - headerLength);
            int entryStart = 0;
            int entryEnd = InContents.IndexOf('>');
            try
            {
                // GetDbConnection opens a new connection for each call: it is closed when this block ends.
                using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

                while (entryEnd >= 0)
                {
                    string entry = DataUtil.Trim(DataUtil.Mid(InContents, entryStart, entryEnd - entryStart));
                    if (entry != "")
                    {
                        int idEnd = entry.IndexOf('\\');
                        string fNum_ID = DataUtil.Mid(entry, 1, idEnd - 1);
                        int nameEnd = entry.IndexOf('\\', idEnd + 1);
                        if (nameEnd < 0)
                        {
                            nameEnd = entry.Length + 1;
                        }
                        string displayName = DataUtil.Mid(entry, idEnd + 1, nameEnd - (idEnd + 1));
                        Gf.WorshipListIDOK = true;
                        WriteItemtoPraiseBook(connection, LegacyListReference.FromFields(DataUtil.Left(entry, 1), fNum_ID, displayName), displayName, "");
                    }
                    entry = "";
                    entryStart = entryEnd + 1;
                    entryEnd = InContents.IndexOf('>', entryStart);
                }
            }
            catch
            {
            }
            LoadIndexFilePostAction(UsageMode.PraiseBook);
        }
    }

    private void WriteItemtoPraiseBook(DbConnection connection, LegacyListReference reference, string DisplayName1, string FolderName, string storedNumber = null)
    {
        string listItemId = reference.Id;
        if (listItemId != "")
        {
            ListViewItem listViewItem = new();
            string songNumber = "0";
            string FirstCharSym = "";
            if (reference.Kind == LegacyListItemKind.DatabaseSong)
            {
                bool found = false;
                try
                {
                    DataRow dr = SongQueries.FindListEntry(connection, Gf.WorshipListIDOK, listItemId, DisplayName1, Gf.WorshipListIDOK ? 0 : Gf.GetFolderNumber(FolderName));
                    if (dr != null)
                    {
                        if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && Gf.Folders[DataUtil.GetDataInt(dr, "FolderNo")].Use > 0)
                        {
                            DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                            FolderName = Gf.Folders[DataUtil.GetDataInt(dr, "FolderNo")].Name;
                            listItemId = "D" + DataUtil.GetDataInt(dr, "songid");
                            songNumber = ((DataUtil.GetDataString(dr, "song_number") != "") ? DataUtil.GetDataString(dr, "song_number") : "0");
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        listItemId = "D0";
                        DisplayName1 += " <Error - Item Not Found>";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    listItemId = "D0";
                    DisplayName1 += " <Error - Item Not Found>";
                }
            }
            else
            {
                listItemId = "D0";
                Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(DisplayName1, Gf.PB_CJKGroupStyle, ref FirstCharSym));
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add(DisplayName1);
            listViewItem.SubItems.Add(listItemId);
            listViewItem.SubItems.Add(FirstCharSym);
            listViewItem.SubItems.Add(string.IsNullOrEmpty(storedNumber) ? songNumber : storedNumber);
        }
    }

    private void PopulateWorshipList()
    {
        bool currentSessionListed = false;
        SessionList.Items.Clear();
        if (!Directory.Exists(Gf.WorshipDir))
        {
            FileUtil.MakeDir(Gf.WorshipDir);
        }
        DirectoryInfo directoryInfo = new(Gf.WorshipDir);
        FileInfo[] files = directoryInfo.GetFiles("*.json");
        foreach (FileInfo fileInfo in files)
        {
            string InFileName = fileInfo.Name;
            InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
            if (InFileName != "")
            {
                SessionList.Items.Add(InFileName);
                if (Gf.CurSession == InFileName)
                {
                    currentSessionListed = true;
                }
            }
        }
        if (currentSessionListed)
        {
            SessionList.Text = Gf.CurSession;
        }
        else if (SessionList.Items.Count > 0)
        {
            SessionList.SelectedIndex = 0;
            Gf.CurSession = SessionList.Items[0].ToString();
        }
        else
        {
            Gf.CurSession = "Worship Service";
            new ServiceListDocument(ServiceListDocument.CurrentVersion, Gf.SystemID, Gf.GetListFormat(""), "", []).Save(Gf.WorshipDir + Gf.CurSession + ".json");
            SessionList.Items.Add(Gf.CurSession);
            SessionList.SelectedIndex = 0;
        }
        SessionList_Change();
    }

    private void PopulatePraiseBooksList()
    {
        bool currentPraiseBookListed = false;
        PraiseBook.Items.Clear();
        if (!Directory.Exists(Gf.PraiseBookDir))
        {
            FileUtil.MakeDir(Gf.PraiseBookDir);
        }
        DirectoryInfo directoryInfo = new(Gf.PraiseBookDir);
        FileInfo[] files = directoryInfo.GetFiles("*.json");
        foreach (FileInfo fileInfo in files)
        {
            string InFileName = fileInfo.Name;
            InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
            if (InFileName != "")
            {
                PraiseBook.Items.Add(InFileName);
                if (Gf.CurPraiseBook == InFileName)
                {
                    currentPraiseBookListed = true;
                }
            }
        }
        if (currentPraiseBookListed)
        {
            PraiseBook.Text = Gf.CurPraiseBook;
        }
        else if (PraiseBook.Items.Count > 0)
        {
            PraiseBook.SelectedIndex = 0;
            Gf.CurPraiseBook = PraiseBook.Items[0].ToString();
        }
        else
        {
            Gf.CurPraiseBook = "PraiseBook 1";
            new ServiceListDocument(ServiceListDocument.CurrentVersion, Gf.SystemID, Gf.GetListFormat(""), "", []).Save(Gf.PraiseBookDir + Gf.CurPraiseBook + ".json");
            PraiseBook.Items.Add(Gf.CurPraiseBook);
            PraiseBook.SelectedIndex = 0;
        }
        PraiseBook.Text = Gf.CurPraiseBook;
    }

    private void ApplyUseSongNumbers(bool InUseSongNumbers)
    {
        Gf.UseSongNumbers = InUseSongNumbers;
        Menu_UseSongNumbering.Checked = Gf.UseSongNumbers;
        if (InUseSongNumbers)
        {
            SongsList.Columns[4].Width = 60;
            Folders_WordCount.Enabled = false;
            PB_WordCount.Enabled = false;
        }
        else
        {
            SongsList.Columns[4].Width = 0;
            Folders_WordCount.Enabled = true;
            PB_WordCount.Enabled = true;
        }
        SetSortButtonPB(Gf.PB_CJKGroupStyle);
    }

    private void BuildFolderList()
    {
        ImplementFolderChange = false;
        int firstFolderIndex = 0;
        string selectedFolderName = "";
        string firstFolderName = "";
        int previousFolderNumber;
        if (SongFolder.Items.Count > 0)
        {
            if (SongFolder.SelectedIndex < 0)
            {
                SongFolder.SelectedIndex = 0;
            }
            // The selected folder's name (SelectedText is the highlighted edit text, always "" in a DropDownList).
            previousFolderNumber = Gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString());
        }
        else
        {
            previousFolderNumber = 0;
        }
        SongFolder.Items.Clear();
        for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
        {
            if (Gf.Folders[i].Use > 0)
            {
                SongFolder.Items.Add(Gf.Folders[i].Name);
                if (Gf.GetFolderNumber(Gf.Folders[i].Name) == previousFolderNumber)
                {
                    selectedFolderName = Gf.Folders[i].Name;
                }
                if (firstFolderIndex == 0)
                {
                    firstFolderIndex = i;
                    firstFolderName = Gf.Folders[i].Name;
                }
            }
        }
        if (selectedFolderName == "")
        {
            selectedFolderName = firstFolderName;
        }
        SongFolder.Text = selectedFolderName;
        ImplementFolderChange = true;
        SongFolder_Change();
    }

    /// <summary>
    /// Fills the songs list from folder FNumber (0: all folders searched by Find, below 0: the last Find
    /// query), rescanning the Media folder.
    /// </summary>
    private void FillList(int FNumber, string ListString, bool InItemMusicOnly)
    {
        FillList(FNumber, ListString, InItemMusicOnly, rescanMedia: true);
    }

    /// <param name="ListString">With a folder (FNumber &gt; 0): when not empty, only the songs whose title
    /// (Title_1) contains this text (find in this folder, design D26). Bound as a parameter, never part of the SQL.</param>
    /// <param name="rescanMedia">False keeps the Media folder scan of the last fill (the find box reloads the
    /// same folder on every keystroke burst; the media marker does not change in between).</param>
    private void FillList(int FNumber, string ListString, bool InItemMusicOnly, bool rescanMedia)
    {
        if (rescanMedia)
        {
            Gf.TotalMusicFiles = -1;
        }
        BoundQuery songs;
        if (FNumber < 0)
        {
            songs = Gf.FindSongQuery;
            if (songs == null) return;
        }
        else if (FNumber == 0)
        {
            var folders = Enumerable.Range(1, Gf.MAXSONGSFOLDERS - 1).Where(i => Gf.Folders[i].FindSongs);
            songs = new BoundQuery($"{Gf.SongListSelect} where FolderNo in ({string.Join(",", folders)}) and (Title_1 is not null or Title_2 is not null)");
        }
        else if (!string.IsNullOrEmpty(ListString))
        {
            songs = new BoundQuery($"{Gf.SongListSelect} where FolderNo=@folder and {SongFindBox.TitleCondition}",
                ("@folder", FNumber), ("@title", SongFindBox.LikePattern(ListString)));
        }
        else
        {
            songs = new BoundQuery(Gf.SongListSelect + " where FolderNo=@folder and (Title_1 is not null or Title_2 is not null)", ("@folder", FNumber));
        }

        SongsList.BeginUpdate();
        try
        {
            var (connection, reader) = DbController.GetDataReader(Gf.ConnectStringMainDB,
                songs.Text + SongListOrderClause(Gf.UseSongNumbers, CurStyle), songs.Parameters);
            using (connection)
            using (reader)
            {
                var rows = new List<ListViewItem>();
                while (reader != null && reader.Read())
                {
                    string title = DataUtil.ObjToString(reader["Title_1"]);
                    bool hasMusic = Gf.MusicFound(title, DataUtil.ObjToString(reader["Title_2"]));
                    if (InItemMusicOnly && !hasMusic) continue;
                    rows.Add(new ListViewItem(
                    [
                        title + (hasMusic ? " <#>" : ""), "D" + DataUtil.ObjToString(reader["SongID"]), "", "",
                        DataUtil.ObjToInt(reader["song_number"]).ToString(),
                        DataUtil.ObjToString(reader["LICENCE_ADMIN1"]), DataUtil.ObjToString(reader["LICENCE_ADMIN2"])
                    ]));
                }
                SongsList.Items.AddRange([.. rows]);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex);
        }
        finally
        {
            SongsList.EndUpdate();
        }
        ShowStatusBarSummary();
    }

    /// <summary>
    /// Sort order shared by folder listings and saved searches.
    /// Number (openspec change add-song-number-sort, N2): songs with a number (1 or more) by number,
    /// then the songs without one (0 or NULL; NULL &gt;= 1 is not true); ties and the unnumbered songs
    /// in title order. The global "Use Song Numbering" option keeps its own order.
    /// </summary>
    private static string SongListOrderClause(bool useSongNumbers, SortBy style)
    {
        if (useSongNumbers)
        {
            return " order by song_number, cjk_strokecount";
        }
        return style switch
        {
            SortBy.WordCount => " order by cjk_wordcount, cjk_strokecount",
            SortBy.Number => " order by (case when song_number >= 1 then 0 else 1 end), (case when song_number >= 1 then song_number else 0 end), cjk_strokecount",
            _ => " order by cjk_strokecount",
        };
    }

    private void ShowPicturesFolderThumbs()
    {
        if (InitFormLoad) return;
        BackgroundCurImagePath = ImagesFolder.SelectedIndex >= 0 ? Gf.PictureGroups[ImagesFolder.SelectedIndex].Path : "";
        BackgroundImagename = ListImageFiles(BackgroundCurImagePath);
        FormatBackgroundThumbContainers();
    }

    private static string[] ListImageFiles(string directory)
    {
        if (string.IsNullOrEmpty(directory)) return [];
        var options = new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = 0 };
        try
        {
            return [.. Directory.EnumerateFiles(directory, "*", options)
                .Where(path => Path.GetExtension(path).ToLowerInvariant() is ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".ico")
                .Order(StringComparer.CurrentCulture)];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return [];
        }
    }

    private void LoadBackgroundThumbImages()
        => flowLayoutImages.Display(BackgroundImagename, 0, toolTip1, ThumbImage_MouseUp, ThumbImage_MouseDoubleClick, false);

    private void ShowPowerpointFolderContents(bool ShowThumbs)
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            ExternalPPCurImagePath = PowerpointFolder.SelectedIndex >= 0 ? Gf.PowerpointGroups[PowerpointFolder.SelectedIndex].Path : "";
            var files = PowerPointSourceFiles.ListFiles(ExternalPPCurImagePath, includeSubfolders: true, Gf.PowerPointSourceOrder);
            ExternalPPImagename = Gf.PowerpointListingStyle == 1 ? files : [];
            PowerpointCurPreview = "";
            PowerpointList.BeginUpdate();
            try
            {
                PowerpointList.Items.Clear();
                if (Gf.PowerpointListingStyle == 0)
                    foreach (var path in files)
                        PowerpointList.Items.Add(Path.GetFileNameWithoutExtension(path)).SubItems.Add("P" + path);
            }
            finally { PowerpointList.EndUpdate(); }
            if (Gf.PowerpointListingStyle == 1)
                Gf.ExternalPPT.BuildFirstScreenDump(files, files.Length, Gf.ExternalPreviewCacheDir);
            FormatExternalPowerpointThumbContainers();
            SetPowerpointListColWidth();
        }
        finally { Cursor = Cursors.Default; }
    }

    private void LoadExternalPowerpointThumbImages(int GotoSlide)
        => flowLayoutExternalPowerPoint.Display(ExternalPPImagename, GotoSlide, toolTip1, ExternalFilesPP_MouseUp, ExternalFilesPP_DoubleClick, true);

    private void SortPowerPointFolderContents()
    {
        if (Gf.PowerpointListingStyle == 0)
        {
            SortPowerPointList(PowerpointList, Gf.PowerPointSourceOrder);
            return;
        }

        ExternalPPImagename = PowerPointSourceFiles.Sort(ExternalPPImagename, path => path, Gf.PowerPointSourceOrder).ToArray();
        Gf.ExternalPPT.BuildFirstScreenDump(ExternalPPImagename, ExternalPPImagename.Length, Gf.ExternalPreviewCacheDir);
        int selected = PowerpointCurPreview.Length > 1 ? Array.IndexOf(ExternalPPImagename, PowerpointCurPreview[1..]) : -1;
        LoadExternalPowerpointThumbImages(selected + 1);
    }

    internal static void SortPowerPointList(ListView list, PowerPointSortOrder order)
    {
        var sorted = PowerPointSourceFiles.Sort(list.Items.Cast<ListViewItem>(), item => item.SubItems[1].Text[1..], order).ToArray();
        var selected = list.SelectedItems.Cast<ListViewItem>().ToArray();
        var focused = list.FocusedItem;
        list.BeginUpdate();
        try
        {
            list.Items.Clear();
            list.Items.AddRange(sorted);
            foreach (var item in selected) item.Selected = true;
            if (focused != null) focused.Focused = true;
        }
        finally { list.EndUpdate(); }
        focused?.EnsureVisible();
    }

    private void BookLookupChanged()
    {
        if (InvokeRequired)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                if (!HB_SearchInProgress)
                {
                    Cursor = Cursors.WaitCursor;
                    if (BookLookup.Items.Count > 66)
                    {
                        BookLookup.Items.RemoveAt(66);
                    }
                    BibleText.Text = "";
                    BibleUserLookup.Text = "";
                    Gf.LoadBiblePassagesFromTabIndex(TabBibleVersions.SelectedIndex, BookLookup, ref BibleText, Gf.HB_ShowVerses);
                    Gf.HB_SequentialListing = true;
                    ShowStatusBarSummary();
                    Cursor = Cursors.Default;
                }

            }));
        }
        else
        {
            if (!HB_SearchInProgress)
            {
                Cursor = Cursors.WaitCursor;
                if (BookLookup.Items.Count > 66)
                {
                    BookLookup.Items.RemoveAt(66);
                }
                BibleText.Text = "";
                BibleUserLookup.Text = "";
                Gf.LoadBiblePassagesFromTabIndex(TabBibleVersions.SelectedIndex, BookLookup, ref BibleText, Gf.HB_ShowVerses);
                Gf.HB_SequentialListing = true;
                ShowStatusBarSummary();
                Cursor = Cursors.Default;
            }
        }
    }

    private void TabBibleVersionsChanged()
    {
        if (!TabBibleVersions.Enabled)
        {
            return;
        }
        if (BookLookup.SelectedIndex == 66)
        {
            HB_SearchInProgress = true;
        }
        if (Gf.LoadBibleBooksList(TabBibleVersions, ref BookLookup, HB_SearchInProgress, null))
        {
            Gf.HB_CurVersionTabIndex = TabBibleVersions.SelectedIndex;
            if (BookLookup.SelectedIndex == 66 && Gf.BibleSearchQuery == null)
            {
                Cursor = Cursors.WaitCursor;
                Gf.RefreshBiblePassages(Gf.HB_CurVersionTabIndex, BookLookup, ref BibleText, Gf.HB_ShowVerses);
                Cursor = Cursors.Default;
            }
            HB_ReselectSame();
        }
        else
        {
            BibleText.Text = "";
        }
        HB_SearchInProgress = false;
        ShowStatusBarSummary();
    }

    private void HB_ReselectSame()
    {
        if (BibleText.Text != "" && HB_CurSelectedPassages != "")
        {
            try
            {
                string InString = HB_CurSelectedPassages;
                DataUtil.ExtractOneInfo(ref InString, ';');
                DataUtil.ExtractOneInfo(ref InString, ';');
                DataUtil.ExtractOneInfo(ref InString, ';');
                int startBook = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                int startChapter = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                int startVerse = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                int endChapter = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                int endVerse = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                int endBook = startBook;
                while (InString != "")
                {
                    endBook = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, ';'));
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    endChapter = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    endVerse = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                }
                BibleText.Focus();
                for (int i = 1; i <= Gf.HB_VersesLocation[0].Version; i++)
                {
                    if (Gf.HB_VersesLocation[i].Book == startBook && Gf.HB_VersesLocation[i].Chapter == startChapter && Gf.HB_VersesLocation[i].Verse == startVerse)
                    {
                        BibleText.SelectionStart = Gf.HB_VersesLocation[i].Start;
                        for (int j = i; j <= Gf.HB_VersesLocation[0].Version; j++)
                        {
                            if (Gf.HB_VersesLocation[j].Book == endBook && Gf.HB_VersesLocation[j].Chapter == endChapter && Gf.HB_VersesLocation[j].Verse == endVerse)
                            {
                                BibleText.SelectionLength = Gf.HB_VersesLocation[j].Start - Gf.HB_VersesLocation[i].Start + Gf.HB_VersesLocation[j].Length - 2;
                                BibleText.ScrollToCaret();
                                j = Gf.HB_VersesLocation[0].Version;
                            }
                        }
                        i = Gf.HB_VersesLocation[0].Version;
                    }
                }
                HB_StartBuildStringProcess();
            }
            catch
            {
            }
        }
    }

    private bool HB_StartBuildStringProcess()
    {
        if (Gf.BibleVersions.ContainsIndex(TabBibleVersions.SelectedIndex))
        {
            HB_CurSelectedTitle = "";
            HB_CurSelectedPassages = HB_BuildSelectionString(TabBibleVersions.SelectedIndex, ref HB_CurSelectedTitle);
            HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
            HB_CurSelectedFormat = "";
            BibleText.Focus();
            return true;
        }
        HB_CurSelectedPassages = "";
        HB_CurSelectedTitle = "";
        BibleText.Focus();
        return false;
    }

    private string HB_BuildSelectionString(int InBibleVersion, ref string OutputTitle)
    {
        if (BibleText.Text != "")
        {
            string versionFileName = Gf.BibleVersions[InBibleVersion].FileName;
            int firstVerse = 0;
            int lastVerse = 0;
            string selection = "";
            int selectionStart = BibleText.SelectionStart + 2;
            int selectionEnd = selectionStart + BibleText.SelectionLength;
            if (selectionStart >= 0)
            {
                for (int i = 1; i <= Gf.HB_VersesLocation[0].Version; i++)
                {
                    if (!(selectionStart >= Gf.HB_VersesLocation[i].Start && selectionStart <= Gf.HB_VersesLocation[i].Start + Gf.HB_VersesLocation[i].Length))
                    {
                        continue;
                    }
                    firstVerse = i;
                    for (int j = i; j <= Gf.HB_VersesLocation[0].Version; j++)
                    {
                        if (selectionEnd >= Gf.HB_VersesLocation[j].Start && selectionEnd <= Gf.HB_VersesLocation[j].Start + Gf.HB_VersesLocation[j].Length)
                        {
                            lastVerse = j;
                            j = 3002;
                        }
                    }
                    i = 3002;
                }
            }
            int verseCount = lastVerse - firstVerse + 1;
            if (Gf.HB_SequentialListing)
            {
                if (verseCount > Gf.HB_MaxVersesSelection)
                {
                    lastVerse = firstVerse + Gf.HB_MaxVersesSelection - 1;
                }
            }
            else if (verseCount > Gf.HB_MaxAdhocVersesSelection)
            {
                lastVerse = firstVerse + Gf.HB_MaxAdhocVersesSelection - 1;
            }
            if (firstVerse >= 0)
            {
                BibleText.SelectionStart = Gf.HB_VersesLocation[firstVerse].Start;
                if (lastVerse < 0)
                {
                    lastVerse = firstVerse;
                }
                BibleText.SelectionLength = Gf.HB_VersesLocation[lastVerse].Start - Gf.HB_VersesLocation[firstVerse].Start + Gf.HB_VersesLocation[lastVerse].Length - 2;
            }
            BibleText.ScrollToCaret();
            if (Gf.HB_SequentialListing)
            {
                string startReference = Convert.ToString(Gf.HB_VersesLocation[firstVerse].Chapter) + ":" + Convert.ToString(Gf.HB_VersesLocation[firstVerse].Verse);
                string endReference = " - " + Convert.ToString(Gf.HB_VersesLocation[lastVerse].Chapter) + ":" + Convert.ToString(Gf.HB_VersesLocation[lastVerse].Verse);
                if (Gf.HB_VersesLocation[firstVerse].Chapter == Gf.HB_VersesLocation[lastVerse].Chapter)
                {
                    endReference = ((Gf.HB_VersesLocation[firstVerse].Verse != Gf.HB_VersesLocation[lastVerse].Verse) ? ("-" + Convert.ToString(Gf.HB_VersesLocation[lastVerse].Verse)) : "");
                }
                OutputTitle = Gf.LookUpBookName(InBibleVersion, Gf.HB_VersesLocation[firstVerse].Book) + " " + startReference + endReference + " (" + Gf.BibleVersions[InBibleVersion].Name + ")";
                selection = "0" + ';' + versionFileName + ';' + ';';
                int i;
                for (i = firstVerse; i <= lastVerse; i++)
                {
                    int runStart;
                    for (runStart = i; Gf.HB_VersesLocation[i].Chapter == Gf.HB_VersesLocation[runStart].Chapter && i <= lastVerse; i++)
                    {
                    }
                    int j = i - 1;
                    selection = string.Concat(selection, Convert.ToString(Gf.HB_VersesLocation[runStart].Book), ';', Convert.ToString(Gf.HB_VersesLocation[runStart].Chapter), ';', Convert.ToString(Gf.HB_VersesLocation[runStart].Verse), ';', Convert.ToString(Gf.HB_VersesLocation[j].Chapter), ';', Convert.ToString(Gf.HB_VersesLocation[j].Verse), ';');
                    i = j;
                }
            }
            else
            {
                selection = "1" + ';' + versionFileName + ';' + ';';
                for (int i = firstVerse; i <= lastVerse; i++)
                {
                    OutputTitle = OutputTitle + DataUtil.Trim(DataUtil.Left(Gf.LookUpBookName(InBibleVersion, Gf.HB_VersesLocation[i].Book), 4)) + " " + Convert.ToString(Gf.HB_VersesLocation[i].Chapter) + ":" + Convert.ToString(Gf.HB_VersesLocation[i].Verse) + ",";
                    selection = string.Concat(selection, Convert.ToString(Gf.HB_VersesLocation[i].Book), ';', Convert.ToString(Gf.HB_VersesLocation[i].Chapter), ';', Convert.ToString(Gf.HB_VersesLocation[i].Verse), ';', Convert.ToString(Gf.HB_VersesLocation[i].Chapter), ';', Convert.ToString(Gf.HB_VersesLocation[i].Verse), ';');
                }
                OutputTitle = DataUtil.Left(OutputTitle, OutputTitle.Length - 1);
                if (OutputTitle.Length > 60)
                {
                    OutputTitle = DataUtil.Left(OutputTitle, 60) + " .. ";
                }
                OutputTitle = OutputTitle + " (" + Gf.BibleVersions[InBibleVersion].Name + ")";
            }
            return selection;
        }
        return "";
    }

    private void SessionList_Change()
    {
        Gf.CurSession = SessionList.Text;
        Cursor = Cursors.WaitCursor;
        LoadWorshipList(0);
        WriteCurSession();
        if (Gf.ShowRunning)
        {
            ValidateWorshipListItems(ShowErrorMessage: false);
            gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
        }
        Cursor = Cursors.Default;
    }

    private static void WriteCurSession()
    {
        RegUtil.SaveRegValue("config", "current_session", Gf.CurSession);
    }

    private static void WriteCurPraiseBook()
    {
        RegUtil.SaveRegValue("config", "current_praisebook", Gf.CurPraiseBook);
    }

    private void LoadWorshipList(int DataType)
    {
        string inFileName = Gf.WorshipDir + Gf.CurSession + ".json";
        LoadWorshipList(DataType, inFileName);
    }

    private string LoadWorshipList(int DataType, string InFileName)
    {
        Gf.StartPresAt = 0;
        string result = LoadIndexFile(DataType, InFileName, ref WorshipListItems, UsageMode.Worship, ref Gf.CurSessionNotes);
        WriteDebugLog($"LoadWorshipList() completed - Items loaded in ListView: {WorshipListItems.Items.Count}");
        PreviewNotes.Text = Gf.CurSessionNotes;
        return result;
    }

    private void LoadPraiseBook(int DataType)
    {
        string inFileName = Gf.PraiseBookDir + Gf.CurPraiseBook + ".json";
        LoadIndexFile(DataType, inFileName, ref PraiseBookItems, UsageMode.PraiseBook, ref Gf.CurPraiseBookNotes);
    }

    /// <summary>
    /// Loads a worship list or praise book file (DataType 0: header and items, 1: header only,
    /// 2: only returns the header format data). Falls back to the v3.2 text format when the file is
    /// not an Axicultus XML list.
    /// </summary>
    private string LoadIndexFile(int DataType, string InFileName, ref ListView InList, UsageMode InMode, ref string InNotes)
    {
        try
        {
            if (!File.Exists(InFileName)) return "";
            string text = File.ReadAllText(InFileName);
            var start = text.AsSpan().TrimStart();
            if (!start.StartsWith("<") && !start.StartsWith("{"))
            {
                if (InMode == UsageMode.Worship) Load32WorshipList(DataType, text);
                else Load32PraiseBook(DataType, text);
                return "";
            }
            var document = ServiceListDocument.Read(text);
            if (document.LegacyText != null)
            {
                if (InMode == UsageMode.Worship) Load32WorshipList(DataType, document.LegacyText);
                else Load32PraiseBook(DataType, document.LegacyText);
                return "";
            }
            string format = document.FormatData ?? "";
            Gf.WorshipListIDOK = Gf.SystemID == document.SystemId;
            if (DataType == 2) return format;
            Gf.LoadHeaderData(format, ref Gf.HeaderData, '>');
            Gf.ApplyHeaderData();
            if (DataType == 1)
            {
                if (InMode == UsageMode.Worship) { UpdateDefaultFields(); UpdateDisplayPanelFields(); }
                return format;
            }
            using var connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
            InList.Items.Clear();
            InNotes = document.Notes ?? "";
            foreach (var entry in document.Items)
            {
                var reference = LegacyListReference.Parse(entry.ItemId, entry.Title);
                if (InMode == UsageMode.Worship)
                    WriteItemtoWorshipList(connection, reference, reference.ContentPath ?? entry.Title, entry.Folder, entry.FormatData, -1);
                else
                    WriteItemtoPraiseBook(connection, reference, entry.Title, entry.Folder, entry.FormatData);
            }
            LoadIndexFilePostAction(InMode);
            return format;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or XmlException or System.Text.Json.JsonException)
        {
            WriteDebugLog($"Cannot read list {InFileName}: {ex.Message}");
            return "";
        }
    }

    private void Load32WorshipList(int DataType, string original = null)
    {
        Gf.TotalMusicFiles = -1;
        string inFileName = Gf.WorshipDir + Gf.CurSession + ".json";
        if (original == null) gfFileHelpers.LoadFileContents(inFileName, ref InContents);
        else InContents = original;
        int headerLength = Gf.Load32HeaderData(inFileName, InContents, ref Gf.HeaderData);
        if (headerLength < 1)
        {
            InContents = "";
        }
        switch (DataType)
        {
            case 2:
                return;
            case 1:
                Gf.ApplyHeaderData();
                UpdateDefaultFields();
                return;
        }
        Gf.ApplyHeaderData();
        WorshipListItems.Items.Clear();
        InContents = DataUtil.Mid(InContents, headerLength + 1, InContents.Length - headerLength);
        int entryStart = 0;
        int entryEnd = InContents.IndexOf('>');
        try
        {

            // GetDbConnection opens a new connection for each call: it is closed when this block ends.
            using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
            while (entryEnd >= 0)
            {
                string entry = DataUtil.Trim(DataUtil.Mid(InContents, entryStart, entryEnd - entryStart));
                int formatStart = 0;
                Gf.SongFormatData = "";
                formatStart = entry.IndexOf('*');
                if (formatStart >= 0)
                {
                    Gf.SongFormatData = DataUtil.Trim(DataUtil.Right(entry, entry.Length - formatStart - 1));
                    entry = DataUtil.Trim(DataUtil.Left(entry, formatStart));
                }
                if (entry != "")
                {
                    int idEnd = entry.IndexOf('\\');
                    string fNum_ID = DataUtil.Mid(entry, 1, idEnd - 1);
                    int nameEnd = entry.IndexOf('\\', idEnd + 1);
                    int nextBackslash = nameEnd;
                    while (nextBackslash >= 0)
                    {
                        nextBackslash = entry.IndexOf('\\', nextBackslash + 1);
                        if (nextBackslash >= 0)
                        {
                            nameEnd = nextBackslash;
                        }
                    }
                    if (nameEnd < 0)
                    {
                        nameEnd = entry.Length + 1;
                    }
                    string displayName = DataUtil.Mid(entry, idEnd + 1, nameEnd - (idEnd + 1));
                    string inSym = DataUtil.Left(entry, 1);
                    Gf.WorshipListIDOK = true;
                    DataUtil.Convertv32FormatString(ref Gf.SongFormatData, '*');

                    WriteItemtoWorshipList(connection, LegacyListReference.FromFields(inSym, fNum_ID, displayName), displayName, "", Gf.SongFormatData, -1);


                }
                entry = "";
                entryStart = entryEnd + 1;
                entryEnd = InContents.IndexOf('>', entryStart);
            }

        }
        catch
        {
        }
        LoadIndexFilePostAction(UsageMode.Worship);
    }

    private void LoadIndexFilePostAction(UsageMode InMode)
    {
        if (InMode == UsageMode.Worship)
        {
            SetMainDefaultBackScreen();
            SetWorshipPraiseListColWidth();
            UpdateDefaultFields();
            UpdateDisplayPanelFields();
            UpdateDisplayPanelData(RefreshSlides: true);
            if (WorshipListItems.Items.Count > 0 && Gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                WorshipListItems.Items[0].Selected = true;
                WorshipListIndexChanged();
            }
            else
            {
                WorshipListIndexChanged();
            }
        }
        else
        {
            SetSortButtonPB(Gf.PB_CJKGroupStyle);
            if (PraiseBookItems.Items.Count > 0)
            {
                PraiseBookItems.Items[0].Selected = true;
                //PraiseBookItems_SelectedIndexChanged(null, null);
            }
            SavePraiseBook(persist: false);
        }
        Cursor = Cursors.Default;
    }

    private bool InsertIndexFileItems(string InFileName, ref ListView InList, int AddToLocation, ref string InNotes)
    {
        try
        {
            var document = ServiceListDocument.Read(File.ReadAllText(InFileName));
            using var connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
            Gf.WorshipListIDOK = Gf.SystemID == document.SystemId;
            InNotes += document.Notes;
            foreach (var entry in document.Items)
            {
                var reference = LegacyListReference.Parse(entry.ItemId, entry.Title);
                WriteItemtoWorshipList(connection, reference, reference.ContentPath ?? entry.Title, entry.Folder, entry.FormatData, AddToLocation++);
            }
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or XmlException or System.Text.Json.JsonException)
        {
            WriteDebugLog($"Cannot insert list {InFileName}: {ex.Message}");
            return false;
        }
    }

    private void WriteItemtoWorshipList(DbConnection connection, LegacyListReference reference, string DisplayName1, string FolderName, string FormatString, int AddToLocation)
    {
        string listItemId = reference.Id;
        if (listItemId == "")
        {
            return;
        }
        ListViewItem listViewItem = new();
        string musicTitle = "";
        string musicTitle2 = Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: false);
        string licenceAdmin1 = "";
        string licenceAdmin2 = "";
        string songNumber = "0";
        if (reference.Kind == LegacyListItemKind.DatabaseSong)
        {
            bool found = false;
            try
            {
                DataRow dr = SongQueries.FindListEntry(connection, Gf.WorshipListIDOK, listItemId, DisplayName1, Gf.WorshipListIDOK ? 0 : Gf.GetFolderNumber(FolderName));
                if (dr != null)
                {
                    if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && Gf.Folders[DataUtil.GetDataInt(dr, "FolderNo")].Use > 0)
                    {
                        DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                        musicTitle = DataUtil.GetDataString(dr, "Title_2");
                        licenceAdmin1 = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
                        licenceAdmin2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
                        FolderName = Gf.Folders[DataUtil.GetDataInt(dr, "FolderNo")].Name;
                        listItemId = "D" + DataUtil.GetDataInt(dr, "songid");
                        songNumber = ((DataUtil.GetDataString(dr, "song_number") != "") ? DataUtil.GetDataString(dr, "song_number") : "0");
                        found = true;
                    }
                }

                if (!found)
                {
                    DisplayName1 += " <Error - Item Not Found>";
                    listItemId = "D0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                listItemId = "D0";
                DisplayName1 += " <Error - Item Not Found>";
            }
        }
        else if (reference.Kind == LegacyListItemKind.BiblePassage)
        {
            listItemId = "B" + listItemId;
        }
        else if (reference.ContentPath is not null)
        {
            listItemId = reference.Code.ToUpperInvariant() + DisplayName1;
            if (reference.Kind == LegacyListItemKind.InfoScreen)
            {
                string infoTitle = "";
                Gf.LoadIndividualData(ref Gf.TempItem1, listItemId, "", 1, ref infoTitle);
                musicTitle2 = Gf.TempItem1.Title;
                musicTitle = Gf.TempItem1.Title2;
            }
            Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
        }
        if (DisplayName1 != "")
        {
            if (Gf.MusicFound(musicTitle2, musicTitle))
            {
                DisplayName1 += " <#>";
            }
            if (AddToLocation < 0)
            {
                listViewItem = WorshipListItems.Items.Add(DisplayName1);
            }
            else
            {
                try
                {
                    listViewItem = WorshipListItems.Items.Insert(AddToLocation, DisplayName1);
                }
                catch
                {
                    listViewItem = WorshipListItems.Items.Add(DisplayName1);
                }
            }
            listViewItem.ImageIndex = reference.Kind switch
            {
                LegacyListItemKind.DatabaseSong => 0,
                LegacyListItemKind.PowerPoint => 2,
                LegacyListItemKind.BiblePassage => 4,
                LegacyListItemKind.TextFile => 6,
                LegacyListItemKind.InfoScreen => 8,
                LegacyListItemKind.WordDocument => 10,
                LegacyListItemKind.Media => 28,
                _ => -1
            };
            listViewItem.SubItems.Add(listItemId);
            listViewItem.SubItems.Add(FormatString);
            listViewItem.SubItems.Add(songNumber);
            listViewItem.SubItems.Add(licenceAdmin1);
            listViewItem.SubItems.Add(licenceAdmin2);
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add(FolderName);
        }
    }

    private void SetMainDefaultBackScreen()
    {
        Gf.SetDefaultBackScreen(ref PreviewScreen);
        Gf.SetDefaultBackScreen(ref OutputScreen);
        if (Gf.ShowRunning)
        {
            RemoteControlLiveShow(LiveShowAction.Remote_DefaultBackgroundChanged);
        }
    }

    /// <summary>
    /// Loads the selected worship list item into the preview, rebuilding all its thumbnails.
    /// </summary>
    private void WorshipListIndexChanged()
    {
        flowLayoutPreviewPowerPoint.ResetSelection();
        WorshipListIndexChanged(0);
    }

    private void WorshipListIndexChanged(int StartingSlide)
    {
        WorshipListIndexChanged(StartingSlide, GetFirstItem: false);
    }

    static int preSelectedItemNum = -1;

    private void WorshipListIndexChanged(int StartingSlide, bool GetFirstItem)
    {
        Gf.PreviewItem.Source = ItemSource.WorshipList;
        Gf.TotalWorshipListItems = WorshipListItems.Items.Count;
        int selectedIndex;
        if (GetFirstItem && Gf.TotalWorshipListItems > 0)
        {
            WorshipListItems.Items[0].Selected = true;
            selectedIndex = 0;
        }
        else
        {
            selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
        }

        if (selectedIndex >= 0)
        {
            string InTitle = WorshipListItems.Items[selectedIndex].SubItems[0].Text;
            string itemId = WorshipListItems.Items[selectedIndex].SubItems[1].Text;
            Gf.PreviewItem.InMainItemText = InTitle;
            Gf.PreviewItem.InSubItemItem1Text = itemId;
            Gf.PreviewItem.CurItemNo = selectedIndex + 1;
            Gf.PreviewItem.TotalItems = WorshipListItems.Items.Count;

            string filePrefix = Gf.SetPowerpointPreviewPrefix(Gf.PreviewItem, useTitlePrefix: true);

            // Caching: load only when the files are not built yet or the row now holds another item
            bool hasPptPathFromList = !string.IsNullOrEmpty(itemId)
                && itemId[0] == 'P'
                && itemId.Length > 1
                && !string.IsNullOrWhiteSpace(itemId.Substring(1));
            // Skip only when the row still holds the item on the preview (Type + ID): after a reorder,
            // a delete or a list switch another item can sit at the same row number.
            if (!hasPptPathFromList
                || preSelectedItemNum != selectedIndex
                || Gf.PreviewItem.Type + Gf.PreviewItem.ItemID != itemId
                || !Gf.PreviewPPT.IsBuildedFileCheck(Gf.PreviewItem.Path, filePrefix, ref Gf.PreviewItem.TotalSlides, ref Gf.PreviewItem.Slide))
            {
                LoadItem(ref Gf.PreviewItem, itemId, WorshipListItems.Items[selectedIndex].SubItems[2].Text, StartingSlide, ref InTitle, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }

            preSelectedItemNum = selectedIndex;
        }
        else
        {
            Gf.InitialiseIndividualData(ref Gf.PreviewItem);
            Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
            AllowIndividualFormat(AllowFormat: false);
            NoIndividualFormat();
            BuildVerseButtons(Gf.PreviewItem, Reset: true);
            Gf.PreviewItem.Format.ShowSlideTransition = 0;
            Gf.PreviewItem.Format.ShowItemTransition = 0;

            ResetMainPictureBox(ref Gf.PreviewItem);
            ClearLyrics(flowLayoutPreviewLyrics);
            UpdateDisplayPanelFields();
        }
        RefreshWorshipListMarks();
    }

    private void SaveWorshipList()
    {
        SaveWorshipList(PreloadPowerpoint: false);
    }

    private bool SaveWorshipList(bool PreloadPowerpoint)
    {
        // Diagnostic: Log save attempt to file
        WriteDebugLog($"SaveWorshipList() called - Items count: {WorshipListItems.Items.Count}, UpdatingFormatFields: {UpdatingFormatFields}");

        if (!UpdatingFormatFields)
        {
            Gf.CurSession = SessionList.Text;
            for (int i = 1; i <= WorshipListItems.Items.Count; i++)
            {
                Gf.WorshipSongs[i].Title = Gf.RemoveMusicSym(DataUtil.Trim(WorshipListItems.Items[i - 1].Text));
                Gf.WorshipSongs[i].IdString = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[1].Text);
                Gf.WorshipSongs[i].Type = DataUtil.Left(Gf.WorshipSongs[i].IdString, 1);
                Gf.WorshipSongs[i].FormatString = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[2].Text);
            }
            Gf.TotalWorshipListItems = WorshipListItems.Items.Count;

            string filePath = Gf.WorshipDir + Gf.CurSession + ".json";

            // Get file size BEFORE save
            long fileSizeBefore = -1;
            try { if (System.IO.File.Exists(filePath)) fileSizeBefore = new System.IO.FileInfo(filePath).Length; } catch { }
            WriteDebugLog($"Saving to file: {filePath} (size before: {fileSizeBefore} bytes)");

            bool saveResult = gfFileHelpers.SaveIndexFile(filePath, ref WorshipListItems, UsageMode.Worship, SaveAllItems: true, "", Gf.CurSessionNotes);

            // Get file size AFTER save and count items in file
            WriteDebugLog($"Saved {WorshipListItems.Items.Count} list entries to {filePath}: {saveResult}");

            if (PreloadPowerpoint)
            {
                gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
            }
            return saveResult;
        }
        else
        {
            // Diagnostic: Log when save is skipped due to UpdatingFormatFields flag
            WriteDebugLog("WARNING: SaveWorshipList() skipped - UpdatingFormatFields is true");
            return false;
        }
    }

    private const long MaxLogFileSize = 512 * 1024; // 512KB

    private static readonly System.Collections.Generic.Dictionary<string, int> repeatedErrorCounts = [];
    private static readonly Lock repeatedErrorLock = new();

    /// <summary>
    /// Logs an exception that may repeat (timers, paint): the first occurrence of a given
    /// place + type + message + top frame + first frame of our own code in full, later ones only
    /// as a count line every 100th time, so the log rotation cannot push the first report out.
    /// Returns how often this exception has been seen. Never throws.
    /// </summary>
    internal static int WriteDebugLogThrottled(string where, Exception ex)
    {
        try
        {
            string[] frames = (ex?.StackTrace ?? "").Split('\n');
            string topFrame = frames[0].Trim();
            // GDI+ failures all share the same top frame (inside System.Drawing); the first frame in
            // our own code tells them apart.
            string ownFrame = "";
            foreach (string frame in frames)
            {
                // "at Namespace.Type.Method(...) in file:line N"; the leading word may be localized.
                string trimmed = frame.Trim();
                int space = trimmed.IndexOf(' ');
                string method = (space >= 0) ? trimmed.Substring(space + 1) : trimmed;
                if (method.StartsWith("Axicultus.", StringComparison.Ordinal) || method.StartsWith("OfficeLib.", StringComparison.Ordinal))
                {
                    ownFrame = trimmed;
                    break;
                }
            }
            string key = where + " | " + ex?.GetType().FullName + ": " + ex?.Message + " | " + topFrame + " | " + ownFrame;
            int count;
            lock (repeatedErrorLock)
            {
                if (repeatedErrorCounts.Count > 1000)
                {
                    repeatedErrorCounts.Clear();
                }
                repeatedErrorCounts.TryGetValue(key, out count);
                count++;
                repeatedErrorCounts[key] = count;
            }
            if (count == 1)
            {
                string report = where + " failed: " + (ex == null ? "(no exception)" : ex.ToString());
                System.Diagnostics.Trace.WriteLine(report);
                WriteDebugLog(report);
            }
            else if (count % 100 == 0)
            {
                WriteDebugLog("(repeated " + count + " times so far) " + key);
            }
            return count;
        }
        catch
        {
            return 0;
        }
    }

    internal static void WriteDebugLog(string message)
    {
        try
        {
            string logFile = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "Axicultus_Debug.log"
            );
            if (System.IO.File.Exists(logFile) && new System.IO.FileInfo(logFile).Length > MaxLogFileSize)
            {
                string backupFile = logFile + ".bak";
                if (System.IO.File.Exists(backupFile)) System.IO.File.Delete(backupFile);
                System.IO.File.Move(logFile, backupFile);
            }
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
            System.IO.File.AppendAllText(logFile, logMessage);
        }
        catch
        {
        }
    }

    private void LoadItem(ref SongSettings InItem, string InIDString)
    {
        LoadItem(ref InItem, InIDString, "", 1);
    }

    private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide)
    {
        string InTitle = "";
        LoadItem(ref InItem, InIDString, InFormatString, StartingSlide, ref InTitle, ScrollToCaret: true);
    }

    /// <summary>
    /// Loads item InIDString (type letter + id) into InItem (preview or output) and shows it from StartingSlide.
    /// </summary>
    private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide, ref string InTitle, bool ScrollToCaret)
    {
        string prevTitle = "";
        string nextTitle = "";
        int itemPosition = Gf.StartPresAt;
        if (WorshipListItems.Items.Count > 0)
        {
            int prevIndex = -1;
            int nextIndex = -1;
            if (!InItem.OutputStyleScreen)
            {
                itemPosition = Gf.GetSelectedIndex(WorshipListItems) + 1;
            }
            if (InItem.CurItemNo == 0)
            {
                prevIndex = itemPosition - 1;
                nextIndex = itemPosition;
            }
            else
            {
                prevIndex = itemPosition - 2;
                nextIndex = itemPosition;
            }
            if (prevIndex < 0 && InItem.CurItemNo == 0)
            {
                prevIndex = 0;
            }
            if (nextIndex >= WorshipListItems.Items.Count)
            {
                nextIndex = ((InItem.CurItemNo != 0) ? (-1) : (WorshipListItems.Items.Count - 1));
            }
            if (prevIndex == nextIndex && prevIndex == 0)
            {
                prevIndex = -1;
            }
            prevTitle = ((prevIndex >= 0) ? Gf.RemoveMusicSym(WorshipListItems.Items[prevIndex].SubItems[0].Text) : "");
            nextTitle = ((nextIndex >= 0) ? Gf.RemoveMusicSym(WorshipListItems.Items[nextIndex].SubItems[0].Text) : "");
        }
        string itemType = DataUtil.Left(InIDString, 1);
        Gf.InitialiseIndividualData(ref InItem, GapMedia.None, itemType);
        InItem.PrevTitle = prevTitle;
        InItem.NextTitle = nextTitle;
        MakePowerpointPreviewVisible(InItem, itemType == "P");
        if (itemType == "P")
        {
            if (Gf.ShowRunning && InItem.OutputStyleScreen)
            {
                // Before the PowerPoint work below (it can pump messages): a lowering of the slide shows still
                // waiting in the show window after a non-PowerPoint item must not run any more.
                Gf.PowerPointItemLoading();
            }
            Gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide);
            ValidatePowerpointItem(InItem);
            Gf.LoadIndividualFormatData(ref InItem, "");
            AllowIndividualFormat(AllowFormat: false);
            SetItemFontSettings(ref InItem);
            if (!InItem.OutputStyleScreen)
            {
                UpdateFormatFields();
            }
            BuildAllPowerpointScreenDumps(ref InItem);
            InItem.CurSlide = StartingSlide;
            InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
            if (InItem.OutputStyleScreen)
            {
                DisplayOutputPowerPointThumbnails(InItem.CurSlide);
            }
            else
            {
                DisplayPreviewPowerPointThumbnails(InItem.CurSlide);
            }
            InItem.Format.ShowItemTransition = 0;
            InItem.Format.ShowSlideTransition = 0;
            if (Gf.ShowRunning && InItem.OutputStyleScreen)
            {
                InItem.TotalSlides = Gf.RunPowerpointSong(ref InItem, ref MainPPT, StartingSlide, ShowResult: true);
                mainPptRunItemPath = MainPPT.LastRunStarted ? InItem.Path : null;
                outputPowerpointLoadSerial++;
                // The run above made the slide show topmost; the show window must stay above it.
                LiveShow?.KeepAboveSlideShow();
                if (Gf.DualMonitorMode)
                {
                    if (MainPPT.LastRunStarted)
                    {
                        ShowDualMonitorPP_Preview(ref InItem);
                    }
                    else
                    {
                        // The slide show did not start: the output pane must not keep showing the previous
                        // item's slide as if it were live (the show window shows its default background).
                        Gf.ResetPictureBox(ref InItem, ref OutputScreen, GapType.Default, ImageTransitionControl.TransitionAction.None);
                    }
                    // When the show window ran this item first (it changed the item itself), its result is
                    // known now: the output side follows it. Only a show-window load the main form has not
                    // seen yet counts: in the usual order (main form first) the show window's state still
                    // belongs to the previous live item, which may be the same file (listed twice, reload).
                    if (Gf.LiveLoadSerial != liveLoadSerialSeen)
                    {
                        ReconcileOutputWithLivePowerPoint();
                        liveLoadSerialSeen = Gf.LiveLoadSerial;
                    }
                }
            }
            else
            {
                PP_Preview(ref InItem);
            }
            ShowStatusBarSummary();
            BuildVerseButtons(InItem);
            DisplaySettingsLabel(InItem);
        }
        else
        {
            var isListedType = itemType switch
            {
                "D" or "B" or "T" or "I" or "W" or "M" => true,
                _ => itemType == "G",
            };
            if (isListedType)
            {
                Cursor.Current = Cursors.WaitCursor;
                Gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
                if (InItem.Source == ItemSource.SongsList)
                {
                    if (!InItem.OutputStyleScreen)
                    {
                        InFormatString = InItem.Format.DBStoredFormat;
                    }
                }
                else if (itemType is "I" or "G")
                {
                    InFormatString = InItem.Format.FormatString;
                }
                Gf.LoadIndividualFormatData(ref InItem, InFormatString);
                SetItemFontSettings(ref InItem);
                Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
                if (InItem.OutputStyleScreen)
                {
                    BuildVerseButtons(Gf.OutputItem);
                    DisplayLyrics(Gf.OutputItem, StartingSlide);
                }
                else
                {
                    AllowIndividualFormat(AllowFormat: true, InFormatString != "");
                    UpdateFormatFields();
                    BuildVerseButtons(InItem);
                    DisplayLyrics(InItem, StartingSlide, ScrollToCaret);
                }
            }
            else
            {
                Gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
                AllowIndividualFormat(AllowFormat: true, InFormatString != "");
                if (InFormatString != "")
                {
                    ApplyIndividualFormat(ref InItem);
                }
                UpdateFormatFields();
                BuildVerseButtons(InItem);
                DisplayLyrics(InItem, StartingSlide);
            }
        }
        if (InItem.OutputStyleScreen)
        {
            PostitionBlackClearGapLabels();
            return;
        }
        DisplayItemInfo(InItem, ref PreviewInfo);
        ShowStatusBarSummary();
    }

    private void DisplayItemInfo(SongSettings InItem, ref RichTextBox InTextBox)
    {
        InTextBox.Text = "";
        InTextBox.SelectionStart = 0;
        string mediaLocation = "";
        if (InItem.Type is "P" or "W" or "T" or "I")
        {
            string itemID = InItem.ItemID;
            InTextBox.Text = InTextBox.Text + InItem.ItemID + "\n";
        }
        if (InItem.Title2 != "")
        {
            InTextBox.Text = InTextBox.Text + "Title2: " + InItem.Title2 + "\n";
        }
        if (InItem.Format.BackgroundPicture != "" && InItem.Type != "P")
        {
            if (InTextBox.Text != "")
            {
                InTextBox.Text += "\n";
            }
            InTextBox.Text = InTextBox.Text + "(Image: " + InItem.Format.BackgroundPicture + ")\n";
        }
        mediaLocation = ((InItem.Format.MediaOption is 1 or 2) ? Gf.GetMediaLocation(InItem) : "");
        if (mediaLocation != "")
        {
            InTextBox.Text = InTextBox.Text + "(Media: " + mediaLocation + ")\n";
        }
        if (InItem.Writer != "")
        {
            InTextBox.Text = InTextBox.Text + "Writer: " + InItem.Writer + "\n";
        }
        if (InItem.Copyright != "")
        {
            InTextBox.Text = InTextBox.Text + "Copyright: " + InItem.Copyright + "\n";
        }
        if (InItem.Capo > 0 || InItem.MusicKey != "" || InItem.Timing != "")
        {
            KeyCapoText = ((InItem.MusicKey != "") ? ("Key: " + InItem.MusicKey + " ") : "") + ((InItem.Capo > 0) ? (" Capo " + Convert.ToString(InItem.Capo) + " ") : "") + ((InItem.Timing != "") ? (" (" + InItem.Timing + ")") : "");
            InTextBox.Text = InTextBox.Text + KeyCapoText + "\n";
        }
        else
        {
            KeyCapoText = "";
        }
        if (InItem.Book_Reference != "")
        {
            InTextBox.Text = InTextBox.Text + "Book Ref: " + InItem.Book_Reference + "\n";
        }
        if (InItem.User_Reference != "")
        {
            InTextBox.Text = InTextBox.Text + "User Ref: " + InItem.User_Reference + "\n";
        }
    }

    private bool ValidatePowerpointItem(SongSettings InItem)
    {
        if (string.IsNullOrWhiteSpace(InItem.Path))
        {
            Console.WriteLine($"[PPTPath] ValidatePowerpointItem: empty Path. ItemID={InItem.ItemID}");
        }
        if (File.Exists(InItem.Path))
        {
            return true;
        }
        if (Gf.ShowRunning)
        {
            // During a show no modal box: it stops the operator, and in single monitor mode it is hidden
            // behind the show window, so the app looks frozen.
            string originalPath = InItem.Path;
            string message = "The PowerPoint file was not found: " + InItem.Path;
            if (InItem.OutputStyleScreen)
            {
                // The path stays: the copy opened at show start may still be open, then the item is shown
                // normally; if it is not, the run fails and is reported like any item that did not start.
                message += " (the copy opened at show start is used if it is still open)";
            }
            else
            {
                InItem.Path = "";
            }
            WriteDebugLog("ValidatePowerpointItem: " + message);
            // Once per item in this show (preview load, output load and a failed run of it are one problem).
            if (Gf.FirstMissingFileReport(originalPath))
            {
                ReportErrorNotice(message);
            }
            return false;
        }
        MessageBox.Show(this, "Sorry - Can't find the Powerpoint File '" + InItem.Path + "'");
        InItem.Path = "";
        return false;
    }

    static string previwItem = "";
    static string OutputItem = "";

    private void BuildAllPowerpointScreenDumps(ref SongSettings InItem)
    {
        Console.WriteLine($"[ThumbPreview] BuildAllPowerpointScreenDumps start: ItemID={InItem.ItemID}, OutputStyleScreen={InItem.OutputStyleScreen}, Path={InItem.Path}");
        Console.WriteLine($"[PPTPath] BuildAllPowerpointScreenDumps: ItemID={InItem.ItemID}, OutputStyleScreen={InItem.OutputStyleScreen}, Path='{InItem.Path}'");
        if (string.IsNullOrWhiteSpace(InItem.Path))
        {
            Console.WriteLine($"[PPTPath] Skip: PowerPoint path is empty. ItemID={InItem.ItemID}");
            return;
        }

        string filePrefix = Gf.SetPowerpointPreviewPrefix(InItem, useTitlePrefix: true);
        Console.WriteLine($"[PPTPath] FilePrefix='{filePrefix}'");

        if (InItem.OutputStyleScreen)
        {
            if (Gf.OutputPPT.prePowerPointApp == null)
            {
                Console.WriteLine("[PPTInit] Output prePowerPointApp is null; initializing now.");
                try
                {
                    Gf.OutputPPT.Init();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PPTInit] Output init failed: {ex.Message}");
                }
            }

            // Avoid rebuilding: for the same ItemID check the cache first
            if (OutputItem != InItem.ItemID)
            {
                OutputItem = InItem.ItemID;
            }
            else
            {
                if (Gf.OutputPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides, ref InItem.Slide))
                {
                    Console.WriteLine($"[Cache Hit] Output PPT already built: {InItem.ItemID}");
                    Console.WriteLine($"[ThumbPreview] Cache hit Output: TotalSlides={InItem.TotalSlides}");
                    // Images are reused, but the verse data on InItem may belong to another item: reload it
                    // (from the session cache, else from the notes). While the show runs, LoadItem calls
                    // Gf.RunPowerpointSong right after this, which resets and re-reads the verses anyway,
                    // so the file is not opened here in that case.
                    Gf.OutputPPT.LoadVerses(InItem.Path, Gf.MAX_VERSES, Gf.DB_MAXSLIDES, ref InItem.SongVerses, ref InItem.Slide, Gf.SequenceSymbol, allowOpen: !Gf.ShowRunning);
                    if (InItem.TotalSlides > 0)
                    {
                        FormatPowerPointThumbContainers(flowLayoutOutputPowerPoint, InItem.TotalSlides);
                    }
                    return;
                }
            }
        }
        else
        {
            if (Gf.PreviewPPT.prePowerPointApp == null)
            {
                Console.WriteLine("[PPTInit] Preview prePowerPointApp is null; initializing now.");
                try
                {
                    Gf.PreviewPPT.Init();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PPTInit] Preview init failed: {ex.Message}");
                }
            }

            // Avoid rebuilding: for the same ItemID check the cache first
            if (previwItem != InItem.ItemID)
            {
                previwItem = InItem.ItemID;
            }
            else
            {
                if (Gf.PreviewPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides, ref InItem.Slide))
                {
                    Console.WriteLine($"[Cache Hit] Preview PPT already built: {InItem.ItemID}");
                    Console.WriteLine($"[ThumbPreview] Cache hit Preview: TotalSlides={InItem.TotalSlides}");
                    // Images are reused, but the verse data on InItem may belong to another item: reload it
                    // (from the session cache, else from the notes).
                    Gf.PreviewPPT.LoadVerses(InItem.Path, Gf.MAX_VERSES, Gf.DB_MAXSLIDES, ref InItem.SongVerses, ref InItem.Slide, Gf.SequenceSymbol);
                    if (InItem.TotalSlides > 0)
                    {
                        FormatPowerPointThumbContainers(flowLayoutPreviewPowerPoint, InItem.TotalSlides);
                    }
                    return;
                }
            }
        }

        if (InItem.OutputStyleScreen)
        {
            bool outBuilt = Gf.OutputPPT.BuildScreenOutDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, Gf.MAX_VERSES, Gf.DB_MAXSLIDES, ref InItem.SongVerses, ref InItem.Slide, Gf.SequenceSymbol);
            Console.WriteLine($"[PPTBuild] BuildScreenOutDumps result={outBuilt}, TotalSlides={InItem.TotalSlides}");
            if (outBuilt)
            {
                string firstOut = filePrefix + "1.jpg";
                Console.WriteLine($"[PPTBuild] Output first image exists: {File.Exists(firstOut)} ({firstOut})");
                FormatPowerPointThumbContainers(flowLayoutOutputPowerPoint, InItem.TotalSlides);
            }
        }
        else
        {
            // Kept synchronous (PowerPoint COM objects are only safe on the STA thread);
            // caching and image optimisation keep it fast
            bool preBuilt = Gf.PreviewPPT.BuildScreenDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, Gf.MAX_VERSES, Gf.DB_MAXSLIDES, ref InItem.SongVerses, ref InItem.Slide, Gf.SequenceSymbol);
            Console.WriteLine($"[PPTBuild] BuildScreenDumps result={preBuilt}, TotalSlides={InItem.TotalSlides}");
            if (preBuilt)
            {
                string firstPre = filePrefix + "1.jpg";
                Console.WriteLine($"[PPTBuild] Preview first image exists: {File.Exists(firstPre)} ({firstPre})");
                FormatPowerPointThumbContainers(flowLayoutPreviewPowerPoint, InItem.TotalSlides);
            }
        }
    }

    private void PP_Preview(ref SongSettings InItem)
    {
        Cursor = Cursors.WaitCursor;
        InItem.Format.BackgroundPicture = (InItem.OutputStyleScreen ? Gf.OUTPPFullPath : Gf.PREPPFullPath) + InItem.CurSlide + ".jpg";
        InItem.Format.BackgroundMode = ImageMode.BestFit;
        InItem.UseDefaultFormat = false;
        if (InItem.OutputStyleScreen)
        {
            Gf.SetShowBackground(InItem, ref OutputScreen);
        }
        else
        {
            Gf.SetShowBackground(InItem, ref PreviewScreen);
        }
        SetItemFontSettings(ref InItem);
        Gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
        ShowStatusBarSummary();
        DisplaySettingsLabel(InItem);
        Cursor = Cursors.Default;
    }

    private void DisplayLyrics(SongSettings InItem, int StartingSlide)
    {
        DisplayLyrics(InItem, StartingSlide, ImageTransitionControl.TransitionAction.AsStored);
    }

    private void DisplayLyrics(SongSettings InItem, int StartingSlide, ImageTransitionControl.TransitionAction TransitionAction)
    {
        DisplayLyrics(InItem, StartingSlide, ScrollToCaret: true, 2, TransitionAction);
    }

    private void DisplayLyrics(SongSettings InItem, int StartingSlide, bool ScrollToCaret)
    {
        DisplayLyrics(InItem, StartingSlide, ScrollToCaret, 2, ImageTransitionControl.TransitionAction.AsStored);
    }

    private void DisplayLyrics(SongSettings InItem, int StartingSlide, bool ScrollToCaret, int GapItemBackground)
    {
        DisplayLyrics(InItem, StartingSlide, ScrollToCaret, GapItemBackground, ImageTransitionControl.TransitionAction.AsStored);
    }

    private void DisplayLyrics(SongSettings InItem, int StartingSlide, bool ScrollToCaret, int GapItemBackground, ImageTransitionControl.TransitionAction TransitionAction)
    {
        if (InitFormLoad)
        {
            return;
        }
        bool showLyrics = true;
        int clampedSlide = StartingSlide;
        if (clampedSlide < 1)
        {
            clampedSlide = 1;
        }
        else if (InItem.TotalSlides > 0 && clampedSlide > InItem.TotalSlides)
        {
            clampedSlide = InItem.TotalSlides;
        }
        DisplaySettingsLabel(InItem);
        if (StartingSlide > 0)
        {
            TransitionAction = ImageTransitionControl.TransitionAction.None;
        }
        if (InItem.Type == "P")
        {
            ResetMainPictureBox(ref InItem);
        }
        else if (InItem.Type == "D")
        {
            if (Gf.AxicultusMode == UsageMode.Worship)
            {
                InItem.CurSlide = clampedSlide;
                ShowSlide(ref InItem, TransitionAction);
            }
            else
            {
                Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
            }
        }
        else if (InItem.Type is "B" or "T" or "I" or "W" or "G")
        {
            if (Gf.AxicultusMode == UsageMode.Worship)
            {
                InItem.CurSlide = clampedSlide;
                ShowSlide(ref InItem, TransitionAction);
            }
        }
        else if (InItem.Type is "M" or "G")
        {
            if (Gf.AxicultusMode == UsageMode.Worship)
            {
                InItem.CurSlide = 1;
                ShowSlide(ref InItem, TransitionAction);
            }
        }
        else
        {
            ResetMainPictureBox(ref InItem, GapType.Default, ImageTransitionControl.TransitionAction.None);
            if (InItem.OutputStyleScreen)
            {
                ClearLyrics(flowLayoutOutputLyrics);
            }
            else
            {
                ClearLyrics(flowLayoutPreviewLyrics);
            }
            showLyrics = false;
        }
        if (showLyrics)
        {
            FormatLyricsContainers(InItem);
        }
        ShowStatusBarSummary();
    }

    private void ResetMainPictureBox(ref SongSettings InItem)
    {
        GapType gapItemBackground = GapType.Default;
        ResetMainPictureBox(ref InItem, gapItemBackground, ImageTransitionControl.TransitionAction.None);
    }

    private void ResetMainPictureBox(ref SongSettings InItem, GapType GapItemBackground, ImageTransitionControl.TransitionAction TransitionAction)
    {
        if (InItem.OutputStyleScreen)
        {
            Gf.ResetPictureBox(ref InItem, ref OutputScreen, GapItemBackground, TransitionAction);
        }
        else
        {
            Gf.ResetPictureBox(ref InItem, ref PreviewScreen, GapItemBackground, TransitionAction);
        }
        DisplaySettingsLabel(InItem);
        ShowStatusBarSummary();
    }

    private bool ShowSlide(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
    {
        return ShowSlide(ref InItem, TransitionAction, DoActiveIndicator: false);
    }

    private bool ShowSlide(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction, bool DoActiveIndicator)
    {
        if (InItem.Type == "P")
        {
            if (InItem.OutputStyleScreen)
            {
                if (Gf.ShowRunning)
                {
                    MainPptFor(InItem).ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, OfficeLibKeys.None, InItem.CurSlide);
                    Gf.ReportPowerpointNavigation(MainPPT, InItem);
                    ShowDualMonitorPP_Preview(ref InItem);
                    if (InItem.CurSlide < 1)
                    {
                        InItem.CurSlide = InItem.TotalSlides;
                    }
                    ShowStatusBarSummary();
                }
                else
                {
                    PP_Preview(ref InItem);
                }
            }
            else
            {
                PP_Preview(ref InItem);
            }
            return true;
        }
        InItem.TotalItems = WorshipListItems.Items.Count;
        if (TransitionAction == ImageTransitionControl.TransitionAction.AsStored)
        {
        }
        if (Gf.ShowDBSlide(ref InItem, ref PreviewScreen, ref OutputScreen, DoActiveIndicator, TransitionAction))
        {
            return true;
        }
        ResetMainPictureBox(ref InItem);
        return false;
    }

    private void SongsListIndexChanged()
    {
        SongsListIndexChanged(1);
    }

    private void SongsListIndexChanged(int StartingSlide)
    {
        SongsListIndexChanged(StartingSlide, ScrollToCaret: true);
    }

    private void SongsListIndexChanged(int StartingSlide, bool ScrollToCaret)
    {
        if (LastSelectedSongsListItem[0] == null)
        {
            LastSelectedSongsListItem[0] = "";
        }
        if (LastSelectedSongsListItem[1] == null)
        {
            LastSelectedSongsListItem[1] = "";
        }
        Gf.PreviewItem.Source = ItemSource.SongsList;
        int selectedIndex = Gf.GetSelectedIndex(SongsList);
        if (selectedIndex >= 0)
        {
            string itemId = SongsList.Items[selectedIndex].SubItems[1].Text;
            string InTitle = SongsList.Items[selectedIndex].SubItems[0].Text;
            Gf.PreviewItem.InMainItemText = InTitle;
            Gf.PreviewItem.InSubItemItem1Text = itemId;
            Gf.PreviewItem.CurItemNo = 0;
            LoadItem(ref Gf.PreviewItem, itemId, "", StartingSlide, ref InTitle, ScrollToCaret);
            UpdateDisplayPanelFields();
        }
        else
        {
            Gf.PreviewItem.Type = "";
            Gf.PreviewItem.Title = "";
            Gf.PreviewItem.ItemID = "";
            Gf.PreviewItem.CurItemNo = 0;
            Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
            AllowIndividualFormat(AllowFormat: false);
            UpdateFormatFields();
            BuildVerseButtons(Gf.PreviewItem);
            DisplayLyrics(Gf.PreviewItem, 0, ScrollToCaret: true);
            UpdateDisplayPanelFields();
        }
    }

    private void EnableEditHistory()
    {
        Gf.LoadRegistryMainEditHistory();
        UpdateMenuEditHistory();
    }

    private void AddToEditHistory(string InItemID)
    {
        if (Gf.GetItemTitle(InItemID) != "" && Gf.MainEditHistoryList[1].Id != InItemID)
        {
            if (Gf.TotalMainEditHistory < Gf.MaxUserEditHistory)
            {
                Gf.TotalMainEditHistory++;
            }
            else
            {
                Gf.TotalMainEditHistory = Gf.MaxUserEditHistory;
            }
            for (int historyIndex = Gf.TotalMainEditHistory; historyIndex >= 2; historyIndex--)
            {
                Gf.MainEditHistoryList[historyIndex].Id = Gf.MainEditHistoryList[historyIndex - 1].Id;
            }
            Gf.MainEditHistoryList[1].Id = InItemID;
            Gf.RemoveDuplicateEditorHistoryItems(ref Gf.MainEditHistoryList, ref Gf.TotalMainEditHistory);
            UpdateMenuEditHistory();
            Gf.SaveMainEditHistoryToRegistry();
        }
    }

    private void UpdateMenuEditHistory()
    {
        try
        {
            int keptCount = 0;
            string itemType = "";
            string itemTitle = "";
            for (int i = 1; i <= Gf.TotalMainEditHistory; i++)
            {
                itemTitle = Gf.GetItemTitle(Gf.MainEditHistoryList[i].Id);
                if (itemTitle != "" && Gf.MainEditHistoryList[keptCount].Id != Gf.MainEditHistoryList[i].Id)
                {
                    keptCount++;
                    Gf.MainEditHistoryList[keptCount].Id = Gf.MainEditHistoryList[i].Id;
                    Gf.MainEditHistoryList[keptCount].Title = itemTitle;
                }
            }
            Gf.TotalMainEditHistory = keptCount;
            for (int i = Gf.TotalMainEditHistory + 1; i <= Gf.AbsoluteMaxHitoryItems; i++)
            {
                Gf.MainEditHistoryList[i].Id = "";
                Gf.MainEditHistoryList[i].Title = "";
            }
            for (int i = 1; i < Gf.AbsoluteMaxHitoryItems; i++)
            {
                Menu_EditHistoryList.DropDownItems[i - 1].Text = i + " " + Gf.MainEditHistoryList[i].Title;
                if (i > Gf.TotalMainEditHistory)
                {
                    Menu_EditHistoryList.DropDownItems[i - 1].Visible = false;
                }
                else
                {
                    Menu_EditHistoryList.DropDownItems[i - 1].Visible = true;
                    itemType = DataUtil.Left(Gf.MainEditHistoryList[i].Id, 1);
                    if (itemType == "D")
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.ES_16;
                    }
                    else if (itemType == "P")
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.PPImg;
                    }
                    else if (itemType == "B")
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.Bible;
                    }
                    else if (itemType == "T")
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.notebook;
                    }
                    else if (itemType == "I")
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.Info_Icon;
                    }
                    else if (itemType == "W")
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.word;
                    }
                    else
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Image = null;
                    }
                }
            }
            // New look only: the list type icons in place of the pictures above (task 9.6, design D25).
            ApplyEditHistoryIcons();
        }
        catch
        {
        }
    }

    private void ApplyBackground(int ImageTag, int InMode)
    {
        if ((uint)ImageTag >= (uint)BackgroundImagename.Length) return;
        string directoryName = Path.GetDirectoryName(BackgroundImagename[ImageTag]);
        int backgroundImageMode = -1;
        if (directoryName == Gf.RootAxicultusDir + "Images\\Scenery")
        {
            backgroundImageMode = 2;
        }
        else if (directoryName == Gf.RootAxicultusDir + "Images\\Tiles")
        {
            backgroundImageMode = 0;
        }
        ApplyBackground(BackgroundImagename[ImageTag], InMode, backgroundImageMode);
        UpdateBackgroundImageButtons(InMode, backgroundImageMode);
    }

    private void ApplyBackground(string InImageFileName)
    {
        ApplyBackground(InImageFileName, 2, -1);
    }

    private void ApplyBackground(string InImageFileName, int InMode, int BackgroundImageMode)
    {
        if (InMode == 0)
        {
            Gf.BackgroundPicture = InImageFileName;
            if (BackgroundImageMode is >= 0 and < 3)
            {
                Gf.BackgroundMode = (ImageMode)BackgroundImageMode;
                Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                Gf.OutputItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
            }
            SaveWorshipList();
            WorshipListIndexChanged(Gf.PreviewItem.CurSlide);
            Gf.SetShowBackground(Gf.OutputItem, ref OutputScreen);
            DisplayLyrics(Gf.OutputItem, Gf.OutputItem.CurSlide);
            if (Gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
            }
        }
        else if (InMode is 1 or 2)
        {
            if (Gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex >= 0)
                {
                    for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                    {
                        if (!WorshipListItems.Items[i].Selected)
                        {
                            continue;
                        }
                        if (WorshipListItems.Items[i].SubItems[1].Text is ['D' or 'B' or 'T' or 'M' or 'W', ..])
                        {
                            Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                            if (BackgroundImageMode is >= 0 and < 3)
                            {
                                Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                            }
                            Gf.PreviewItem.Format.FormatString = GetNewFormatString();
                            WorshipListItems.Items[i].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "I")
                        {
                            Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                            if (BackgroundImageMode is >= 0 and < 3)
                            {
                                Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                            }
                            Gf.PreviewItem.UseDefaultFormat = false;
                            Gf.PreviewItem.Format.FormatString = GetNewFormatString();
                            Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen);
                            Gf.DrawText(ref Gf.PreviewItem, ref PreviewScreen, Gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                            AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                            UpdateFormatFields();
                            SaveInfoFilePreview(ReloadImageData: true);
                        }
                        else
                        {
                            continue;
                        }
                        RefreshGalleryBackgroundOnOutput(ItemSource.WorshipList, i + 1, WorshipListItems.Items[i].SubItems[1].Text,
                            InImageFileName, BackgroundImageMode, Gf.PreviewItem.Format.FormatString);
                    }
                    SaveWorshipList();
                    WorshipListIndexChanged(Gf.PreviewItem.CurSlide);
                }
                else if (InMode == 2)
                {
                    Gf.BackgroundPicture = InImageFileName;
                    if (BackgroundImageMode is >= 0 and < 3)
                    {
                        Gf.BackgroundMode = (ImageMode)BackgroundImageMode;
                        Gf.OutputItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    }
                    SaveWorshipList();
                    WorshipListIndexChanged(Gf.PreviewItem.CurSlide);
                    Gf.SetShowBackground(Gf.OutputItem, ref OutputScreen);
                    DisplayLyrics(Gf.OutputItem, Gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.None);
                    if (Gf.ShowRunning)
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                    }
                }
            }
            else if (Gf.PreviewItem.Source == ItemSource.SongsList)
            {
                int selectedIndex = Gf.GetSelectedIndex(SongsList);
                if (selectedIndex >= 0)
                {
                    Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                    if (BackgroundImageMode is >= 0 and < 3)
                    {
                        Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    }
                    Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, GetNewFormatString());
                    SongsListIndexChanged(Gf.PreviewItem.CurSlide);
                    RefreshGalleryBackgroundOnOutput(Gf.PreviewItem.Source, 0, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID,
                        InImageFileName, BackgroundImageMode, Gf.PreviewItem.Format.FormatString);
                }
            }
            else if (Gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                if (Gf.PreviewItem.CompleteLyrics != "")
                {
                    Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                    if (BackgroundImageMode is >= 0 and < 3)
                    {
                        Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    }
                    Gf.PreviewItem.UseDefaultFormat = false;
                    Gf.PreviewItem.Format.FormatString = GetNewFormatString();
                    HB_CurSelectedFormat = Gf.PreviewItem.Format.FormatString;
                    RefreshGalleryBackgroundOnOutput(ItemSource.HolyBible, 0, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID,
                        InImageFileName, BackgroundImageMode, HB_CurSelectedFormat);
                    Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen);
                    Gf.DrawText(ref Gf.PreviewItem, ref PreviewScreen, Gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                    AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                    UpdateFormatFields();
                }
            }
            else if (Gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen && Gf.PreviewItem.CompleteLyrics != "")
            {
                Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                if (BackgroundImageMode is >= 0 and < 3)
                {
                    Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                }
                Gf.PreviewItem.UseDefaultFormat = false;
                Gf.PreviewItem.Format.FormatString = GetNewFormatString();
                Gf.PreviewItem.Format.TempImageFileName = Gf.PreviewItem.Format.BackgroundPicture;
                Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen);
                Gf.DrawText(ref Gf.PreviewItem, ref PreviewScreen, Gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                UpdateFormatFields();
                SaveInfoFilePreview(ReloadImageData: true);
                RefreshGalleryBackgroundOnOutput(ItemSource.ExternalFileInfoScreen, 0, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID,
                    InImageFileName, BackgroundImageMode, Gf.PreviewItem.Format.FormatString);
            }
        }
        Def_NoImage.Enabled = ((Gf.BackgroundPicture != "") ? true : false);
        Ind_NoImage.Enabled = ((Gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
        UpdateWorshipShowIcons();
        UpdateDefaultNoImageButton();
        if (Gf.PreviewItem.Type == "D")
        {
            Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString);
        }
    }

    private void RefreshGalleryBackgroundOnOutput(ItemSource source, int row, string itemKey, string path, int mode, string format)
    {
        if (UpdateGalleryBackground(Gf.OutputItem, source, row, itemKey, path, mode, format))
        {
            Gf.SetShowBackground(Gf.OutputItem, ref OutputScreen);
            DisplayLyrics(Gf.OutputItem, Gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.None);
        }
        if (Gf.ShowRunning && UpdateGalleryBackground(Gf.LiveItem, source, row, itemKey, path, mode, format))
            RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
    }

    internal static bool UpdateGalleryBackground(SongSettings item, ItemSource source, int row, string itemKey, string path, int mode, string format)
    {
        if (item.Source != source || item.CurItemNo != row || string.IsNullOrEmpty(item.ItemID)
            || !string.Equals(item.Type + item.ItemID, itemKey, StringComparison.OrdinalIgnoreCase))
            return false;

        item.UseDefaultFormat = false;
        item.Format.FormatString = format;
        item.Format.BackgroundPicture = path;
        if (mode is >= 0 and < 3) item.Format.BackgroundMode = (ImageMode)mode;
        if (item.Type == "I") item.Format.TempImageFileName = path;
        return true;
    }

    private void UpdateWorshipShowIcons()
    {
        if (ResetWorshipShowIcons())
        {
            if (Gf.OutputItem.CurItemNo > 0)
            {
                try
                {
                    if (WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex is 0 or 2 or 4 or 6 or 8 or 28 or 10)
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex + 1;
                        WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[6].Text = "O";
                    }
                    WorshipListItems.Items[Gf.StartPresAt - 1].ForeColor = Color.Red;
                    WorshipListItems.Items[Gf.StartPresAt - 1].EnsureVisible();
                }
                catch
                {
                }
            }
            else if (Gf.StartPresAt > 0 && Gf.StartPresAt <= WorshipListItems.Items.Count)
            {
                try
                {
                    if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "D")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 0;
                    }
                    else if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "P")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 2;
                    }
                    else if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "B")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 4;
                    }
                    else if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "T")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 6;
                    }
                    else if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "I")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 8;
                    }
                    else if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "W")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 10;
                    }
                    else if (DataUtil.Left(WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[1].Text, 1) == "M")
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = 28;
                    }
                    else
                    {
                        WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[6].Text = "";
                    }
                }
                catch
                {
                }
            }
        }
    }

    private bool ResetWorshipShowIcons()
    {
        if (WorshipListItems.Items.Count > 0)
        {
            for (int i = 0; i < WorshipListItems.Items.Count; i++)
            {
                if (WorshipListItems.Items[i].ImageIndex is 1 or 3 or 5 or 7 or 9 or 29 or 11)
                {
                    WorshipListItems.Items[i].ImageIndex = WorshipListItems.Items[i].ImageIndex - 1;
                }
                WorshipListItems.Items[i].SubItems[6].Text = "";
                if (WorshipListItems.Items[i].ForeColor != SongsList.ForeColor)
                {
                    WorshipListItems.Items[i].ForeColor = SongsList.ForeColor;
                }
            }
            return true;
        }
        return false;
    }

    internal void CopyPreviewToOutput()
    {
        Gf.OutputItem.InMainItemText = Gf.PreviewItem.InMainItemText;
        Gf.OutputItem.InSubItemItem1Text = Gf.PreviewItem.InSubItemItem1Text;
        Gf.OutputItem.Source = Gf.PreviewItem.Source;
        Gf.OutputItem.CurItemNo = Gf.PreviewItem.CurItemNo;
        Gf.StartPresAt = ((Gf.OutputItem.CurItemNo > 0) ? Gf.OutputItem.CurItemNo : Gf.StartPresAt);
        Gf.OutputItem.OutputStyleScreen = true;
        if (Gf.ShowRunning)
        {
            if (Gf.OutputItem.CurItemNo == 0)
            {
                Gf.WorshipSongs[0].Title = Gf.OutputItem.InMainItemText;
                Gf.WorshipSongs[0].IdString = Gf.OutputItem.InSubItemItem1Text;
                Gf.WorshipSongs[0].Type = DataUtil.Left(Gf.WorshipSongs[0].IdString, 1);
                Gf.WorshipSongs[0].FormatString = Gf.PreviewItem.Format.FormatString;
                Gf.AdHocItemPresent = true;
            }
            if (Gf.PreviewItem.Type == "P" && Gf.PreviewItem.Source != ItemSource.WorshipList)
            {
                gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
            }
        }
        flowLayoutOutputPowerPoint.ResetSelection();
        LoadItem(ref Gf.OutputItem, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString, Gf.PreviewItem.CurSlide, ref Gf.PreviewItem.Title, ScrollToCaret: true);
        UpdateWorshipShowIcons();
        if (Gf.ShowRunning)
        {
            Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStoredItem;
            RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
        }
        FocusOutputArea();
    }

    private void ShowSong(ref SongSettings InItem)
    {
        ShowSong(ref InItem, 1);
    }

    private void ShowSong(ref SongSettings InItem, int StartingSlide)
    {
        ShowSong(ref InItem, StartingSlide, ImageTransitionControl.TransitionAction.AsStored);
    }

    private void ShowSong(ref SongSettings InItem, int StartingSlide, ImageTransitionControl.TransitionAction TransitionAction)
    {
        if (TransitionAction == ImageTransitionControl.TransitionAction.AsStored)
        {
            // The stored value is a TransitionTypes index, not a TransitionAction (see Gf.StoredItemTransitionAction).
            TransitionAction = Gf.StoredItemTransitionAction(InItem);
        }
        InItem.CurSlide = StartingSlide;
        RefreshSlidesFonts(ref InItem, TransitionAction);
    }
}
