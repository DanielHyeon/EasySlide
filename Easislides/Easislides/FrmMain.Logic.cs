using Easislides.Properties;
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
using System.Data;
using System.Threading.Tasks;
using Easislides.Util;
using System.Threading;
using Easislides.SQLite;
using Easislides.Module;
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

namespace Easislides
{
    public partial class FrmMain : Form
    {
        public static void Extract(string resxPath, string outputDir)
        {
            Console.WriteLine($"Processing: {resxPath}");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            using (ResXResourceReader reader = new ResXResourceReader(resxPath))
            {
                reader.UseResXDataNodes = true; // ?????????????????? ??

                foreach (DictionaryEntry entry in reader)
                {
                    string key = entry.Key.ToString();
                    ResXDataNode node = (ResXDataNode)entry.Value;

                    try
                    {
                        // ?? ?????????? ???
                        object value = node.GetValue((ITypeResolutionService)null);

                        if (value == null) continue;

                        // 1. ?? ???? (Bitmap) ó
                        if (value is Bitmap bmp)
                        {
                            SaveImage(bmp, outputDir, key, ImageFormat.Png);
                        }
                        // 2. ????(Icon) ó - $this.Icon ??
                        else if (value is Icon icon)
                        {
                            // ???? Bitmap?? ?????????? .ico??????
                            using (FileStream fs = new FileStream(Path.Combine(outputDir, $"{key}.ico"), FileMode.Create))
                            {
                                icon.Save(fs);
                            }
                            Console.WriteLine($"[Icon] Extracted: {key}.ico");
                        }
                        // 3. ???? ???????(ImageStream) ó - imageListSys.ImageStream ??
                        else if (value is ImageListStreamer streamer)
                        {
                            using (ImageList imgList = new ImageList())
                            {
                                imgList.ImageStream = streamer;
                                for (int i = 0; i < imgList.Images.Count; i++)
                                {
                                    string subName = $"{key}_{i}";
                                    SaveImage(imgList.Images[i], outputDir, subName, ImageFormat.Png);
                                }
                                Console.WriteLine($"[ImageList] Extracted {imgList.Images.Count} images from {key}");
                            }
                        }
                        // 4. ?????? (TrayLocation ??
                        else
                        {
                            // Point, Size ?? ?????? ??????????
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
            // ??? ??????? ????  ??
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
                    gf.SaveConfigSettings();
                    return;
            }
            RegUtil.SaveRegValue("settings", Splitter_FolderWidth, splitContainerMain.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_OutputWidth, splitContainer2.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_FolderHeight, splitContainer1.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_PreviewLyricsHeight, splitContainerPreview.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_outputLyricsHeight, splitContainerOutput.SplitterDistance);
            gf.SaveConfigSettings();
        }

        private void Def_SetNoImage()
        {
            gf.BackgroundPicture = "";
            ApplyDefaultData(StartAtFirstSlide: false);
            gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
            RefreshSlidesFonts(ref gf.OutputItem);
            UpdateDefaultNoImageButton();
        }

        private void LoadWorshipListTemplate()
        {
            openFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            openFileDialog1.Title = "Load Session Settings from a Template";
            openFileDialog1.InitialDirectory = gf.WorshipTemplatesDir;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                LoadWorshipList(1, fileName);
                ApplyDefaultData(StartAtFirstSlide: true);
                LoadIndexFilePostAction(UsageMode.Worship);
            }
        }

        private void SaveWorshipListTemplate(string InFileName, string InitDirectory)
        {
            saveFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            saveFileDialog1.Title = "Save Current Session Settings to a Template";
            saveFileDialog1.InitialDirectory = InitDirectory;
            saveFileDialog1.FileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true);
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
            gf.PreviewItem.Format.BackgroundPicture = "";
            gf.PreviewItem.Format.TempImageFileName = "";
            gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen, FallBackToDefault: false);
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
            if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex < 0)
                {
                    Ind_checkBox.Checked = false;
                    return;
                }
                bool flag = false;
                gf.SongFormatData = "";
                gf.PreviewItem.UseDefaultFormat = false;
                if (gf.PreviewItem.Type == "I")
                {
                    WorshipListItems.Items[selectedIndex].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                }
                if (WorshipListItems.Items[selectedIndex].SubItems[2].Text == "")
                {
                    flag = true;
                }
                gf.SongFormatData = WorshipListItems.Items[selectedIndex].SubItems[2].Text;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, gf.SongFormatData);
                UpdateFormatFields();
                if (flag)
                {
                    UpdateFormatData();
                }
            }
            else if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.PreviewItem.UseDefaultFormat = false;
            }
        }

        private void NoIndividualFormat()
        {
            Ind_checkBox.Checked = false;
            gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
            RefreshSlidesFonts(ref gf.PreviewItem, ImageTransitionControl.TransitionAction.None);
            UpdateFormatFields();
            AllowIndividualFormat(AllowFormat: true, BoxChecked: false);
            if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                gf.PreviewItem.UseDefaultFormat = true;
                bool flag = false;
                if (selectedIndex >= 0)
                {
                    if (WorshipListItems.Items[selectedIndex].SubItems[2].Text != "")
                    {
                        flag = true;
                    }
                    WorshipListItems.Items[selectedIndex].SubItems[2].Text = "";
                }
                if (flag)
                {
                    DisplayLyrics(gf.PreviewItem, 1);
                    SaveWorshipList();
                }
            }
            else if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.PreviewItem.UseDefaultFormat = true;
                DisplayLyrics(gf.PreviewItem, 1);
            }
        }

        private void LoadIndividualTemplate()
        {
            openFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            openFileDialog1.Title = "Load Individual Settings from a Template";
            openFileDialog1.InitialDirectory = gf.SettingsTemplatesDir;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = openFileDialog1.FileName;
            string text = LoadWorshipList(2, fileName);
            gf.PreviewItem.Format.FormatString = text;
            string type = gf.PreviewItem.Type;
            int num;
            switch (type)
            {
                default:
                    num = ((!(type == "M")) ? 1 : 0);
                    break;
                case "D":
                case "B":
                case "T":
                case "I":
                case "W":
                    num = 0;
                    break;
            }
            if (num != 0)
            {
                return;
            }
            if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, text);
            }
            else if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex < 0)
                {
                    return;
                }
                WorshipListItems.Items[selectedIndex].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                SaveWorshipList();
            }
            gf.LoadIndividualFormatData(ref gf.PreviewItem, text);
            if (type == "I")
            {
                gf.PreviewItem.Format.TempImageFileName = gf.PreviewItem.Format.BackgroundPicture;
                SaveInfoFilePreview(ReloadImageData: true);
            }
            SetItemFontSettings(ref gf.PreviewItem);
            gf.FormatDisplayLyrics(ref gf.PreviewItem, PrepareSlides: true, UseStoredSequence: true);
            AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
            UpdateFormatFields();
            BuildVerseButtons(gf.PreviewItem);
            DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
            DisplayItemInfo(gf.PreviewItem, ref PreviewInfo);
        }

        private void SaveIndividualTemplate(string InTitle)
        {
            InTitle = gf.MakeTitleValidFileName(InTitle);
            saveFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            saveFileDialog1.Title = "Save Current Individual Settings to a Template";
            saveFileDialog1.InitialDirectory = gf.SettingsTemplatesDir;
            saveFileDialog1.FileName = InTitle;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = ".est";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                try
                {
                    gfFileHelpers.SaveIndexFile(fileName, ref WorshipListItems, UsageMode.Worship, SaveAllItems: false, gf.PreviewItem.Format.FormatString, "");
                }
                catch
                {
                    MessageBox.Show("Error Saving File, please make sure you have write access and try again");
                }
            }
        }

        private void AssignMediaText(ref ToolStripButton InButton, int InMediaOption)
        {
            switch (InMediaOption)
            {
                case 1:
                    InButton.Text = "Media: As Title";
                    break;
                case 2:
                    InButton.Text = "Media: Specific";
                    break;
                case 3:
                    InButton.Text = "Media: Live Feed";
                    break;
                default:
                    InButton.Text = "Media: None";
                    break;
            }
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
                gf.CurPraiseBook = DataUtil.Trim(PraiseBook.Text);
                LoadPraiseBook(0);
                PraiseBookIndexChanged();
                ShowStatusBarSummary();
                DisplayLyrics(gf.PreviewItem, 0);
                Cursor = Cursors.Default;
            }
        }

        private void BuildPicturesFolderList()
        {
            gf.PictureGroups[0, 0] = "Scenery";
            gf.PictureGroups[0, 1] = gf.RootEasiSlidesDir + "Images\\" + gf.PictureGroups[0, 0] + "\\";
            gf.PictureGroups[1, 0] = "Tiles";
            gf.PictureGroups[1, 1] = gf.RootEasiSlidesDir + "Images\\" + gf.PictureGroups[1, 0] + "\\";
            gf.PictureGroups[2, 0] = "Images";
            gf.PictureGroups[2, 1] = gf.ImagesDir;
            gf.PicFolderTotal = 3;
            string[] directories = Directory.GetDirectories(gf.ImagesDir);
            if (directories.Length > 0)
            {
                gf.SingleArraySort(directories);
            }
            string text = gf.RootEasiSlidesDir + "Images\\Scenery";
            string text2 = gf.RootEasiSlidesDir + "Images\\Tiles";
            gf.BuildSubFolderList(gf.ImagesDir, gf.ImagesDir, ref gf.PictureGroups, ref gf.PicFolderTotal);
            ImagesFolder.Items.Clear();
            for (int i = 0; i < gf.PicFolderTotal; i++)
            {
                ImagesFolder.Items.Add(gf.PictureGroups[i, 0]);
            }
        }

        private void BuildInfoScreenFolderList()
        {
            gf.InfoScreenGroups[0, 0] = "InfoScreen Items";
            gf.InfoScreenGroups[0, 1] = gf.InfoScreenDir;
            gf.InfoScreenFolderTotal = 1;
            gf.BuildSubFolderList(gf.InfoScreenDir, gf.InfoScreenDir, ref gf.InfoScreenGroups, ref gf.InfoScreenFolderTotal);
            InfoScreenFolder.Items.Clear();
            for (int i = 0; i < gf.InfoScreenFolderTotal; i++)
            {
                InfoScreenFolder.Items.Add(gf.InfoScreenGroups[i, 0]);
            }
            if (InfoScreenFolder.Items.Count > 0)
            {
                InfoScreenFolder.SelectedIndex = 0;
            }
        }

        private void BuildPowerpointFolderList()
        {
            gf.PowerpointGroups[0, 0] = "Powerpoint Items";
            gf.PowerpointGroups[0, 1] = gf.PowerpointDir;
            gf.PowerpointFolderTotal = 1;
            gf.BuildSubFolderList(gf.PowerpointDir, gf.PowerpointDir, ref gf.PowerpointGroups, ref gf.PowerpointFolderTotal);
            PowerpointFolder.Items.Clear();
            for (int i = 0; i < gf.PowerpointFolderTotal; i++)
            {
                PowerpointFolder.Items.Add(gf.PowerpointGroups[i, 0]);
            }
            if (PowerpointFolder.Items.Count > 0)
            {
                PowerpointFolder.SelectedIndex = 0;
            }
        }

        private void BuildMediaFolderList()
        {
            gf.MediaGroups[0, 0] = "Media Files";
            gf.MediaGroups[0, 1] = gf.MediaDir;
            gf.MediaFolderTotal = 1;
            gf.BuildSubFolderList(gf.MediaDir, gf.MediaDir, ref gf.MediaGroups, ref gf.MediaFolderTotal);
            MediaFolder.Items.Clear();
            for (int i = 0; i < gf.MediaFolderTotal; i++)
            {
                MediaFolder.Items.Add(gf.MediaGroups[i, 0]);
            }
            if (MediaFolder.Items.Count > 0)
            {
                MediaFolder.SelectedIndex = 0;
            }
        }

        private void SongFolder_Change()
        {
            if (!(!ImplementFolderChange | (SongFolder.Text == "")))
            {
                if (SongFolder.Items[SongFolder.Items.Count - 1].ToString() == "Search Results:")
                {
                    SongFolder.Items.RemoveAt(SongFolder.Items.Count - 1);
                }
                SongFolder.ForeColor = SongsList.ForeColor;
                bool findItemMediaOnly = gf.FindItemMediaOnly;
                gf.FindItemMediaOnly = false;
                bool findItemInContents = gf.FindItemInContents;
                gf.FindItemInContents = false;
                SongsList.Items.Clear();
                SetSongListColWidth();
                if (SongFolder.Items.Count >= 1)
                {
                    int folderNumber = gf.GetFolderNumber(SongFolder.Text);
                    CurStyle = gf.FolderGroupStyle[folderNumber];
                    Cursor = Cursors.WaitCursor;
                    SetSortButton(CurStyle);
                    SongsList.RightToLeft = ((gf.ShowFontRTL[folderNumber, 0] > 0) ? RightToLeft.Yes : RightToLeft.No);
                    SongsList.RightToLeftLayout = ((gf.ShowFontRTL[folderNumber, 0] > 0) ? true : false);
                    FillList(folderNumber, "", gf.FindItemMediaOnly);
                    gf.FindItemMediaOnly = findItemMediaOnly;
                    gf.FindItemInContents = findItemInContents;
                    ShowStatusBarSummary();
                    Cursor = Cursors.Default;
                }
            }
        }

        private void SetItemFontSettings(ref SongSettings InItem)
        {
            gf.FormatText(ref InItem, gf.PanelBackColour, gf.PanelBackColourTransparent, gf.PanelTextColour, gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
        }

        private void ApplyWorshipMode()
        {
            gf.EasiSlidesMode = UsageMode.Worship;
            ModeEnableItems(EnableWorshipMode: true);
            LoadWorshipList(1);
            WorshipListIndexChanged();
            ShowStatusBarSummary();
            DisplayLyrics(gf.PreviewItem, 0);
        }

        private void ApplyPraisebookMode()
        {
            Cursor = Cursors.WaitCursor;
            gf.EasiSlidesMode = UsageMode.PraiseBook;
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
            DisplayLyrics(gf.PreviewItem, 0);
            Cursor = Cursors.Default;
        }

        private void PraiseBookIndexChanged()
        {
            int selectedIndex = gf.GetSelectedIndex(PraiseBookItems);
            if (selectedIndex >= 0)
            {
                string text = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                gf.PreviewItem.CurItemNo = selectedIndex + 1;
                gf.PreviewItem.TotalItems = PraiseBook.Items.Count;
                gf.PreviewItem.Source = ItemSource.PraiseBook;
                LoadItem(ref gf.PreviewItem, text);
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.ItemID = "";
                ClearLyrics(ref flowLayoutPreviewLyrics);
            }
        }

        public void Load32PraiseBook(int DataType)
        {
            if (!(DataUtil.Trim(PraiseBook.Text) == ""))
            {
                ListViewItem listViewItem = new ListViewItem();
                string text = "";
                string inFileName = gf.PraiseBookDir + gf.CurPraiseBook + ".esp";
                gfFileHelpers.LoadFileContents(inFileName, ref InContents);
                int num = gf.Load32HeaderData(inFileName, InContents, ref gf.HeaderData);
                if (num < 1)
                {
                    gf.ApplyHeaderData(1);
                    InContents = "";
                }
                if (DataType == 1)
                {
                    gf.ApplyHeaderData(1);
                    return;
                }
                PraiseBookItems.Items.Clear();
                gf.ApplyHeaderData(1);
                InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
                int num2 = 0;
                int num3 = InContents.IndexOf(">");
                try
                {
                    DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);

                    while (num3 >= 0)
                    {
                        string text2 = DataUtil.Trim(DataUtil.Mid(InContents, num2, num3 - num2));
                        if (text2 != "")
                        {
                            int num4 = text2.IndexOf("\\");
                            string fNum_ID = DataUtil.Mid(text2, 1, num4 - 1);
                            int num5 = text2.IndexOf("\\", num4 + 1);
                            if (num5 < 0)
                            {
                                num5 = text2.Length + 1;
                            }
                            string displayName = DataUtil.Mid(text2, num4 + 1, num5 - (num4 + 1));
                            gf.WorshipListIDOK = true;
                            WriteItemtoPraiseBook(connection, DataUtil.Left(text2, 1), fNum_ID, displayName, "");
                        }
                        text2 = "";
                        num2 = num3 + 1;
                        num3 = InContents.IndexOf(">", num2);
                    }
                }
                catch
                {
                }
                LoadIndexFilePostAction(UsageMode.PraiseBook);
            }
        }

        private void WriteItemtoPraiseBook(DbConnection connection, string InSym, string FNum_ID, string DisplayName1, string FolderName)
        {
            if (!(FNum_ID == ""))
            {
                ListViewItem listViewItem = new ListViewItem();
                //string text = "";
                //string text2 = "";
                //string text3 = "";
                string text4 = "0";
                string FirstCharSym = "";
                if (InSym == "D")
                {
                    bool flag = false;
                    try
                    {
                        //string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where lower(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
                        string fullSearchString = (!gf.WorshipListIDOK) ? ($"select * from SONG where lower(Title_1) like \"{DisplayName1.ToLower()}\"  AND FolderNo = {gf.GetFolderNumber(FolderName)}") : ($"select * from SONG where songid = {FNum_ID} AND FolderNo > 0 ");

                        DataRow dr = DbController.GetDataRowScalar(connection, fullSearchString);
                        if (dr != null)
                        {
                            if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && gf.FolderUse[DataUtil.GetDataInt(dr, "FolderNo")] > 0)
                            {
                                DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                                FolderName = gf.FolderName[DataUtil.GetDataInt(dr, "FolderNo")];
                                FNum_ID = "D" + DataUtil.GetDataInt(dr, "songid");
                                text4 = ((DataUtil.GetDataString(dr, "song_number") != "") ? DataUtil.GetDataString(dr, "song_number") : "0");
                                flag = true;
                            }
                        }

                        if (!flag)
                        {
                            FNum_ID = "D0";
                            DisplayName1 += " <Error - Item Not Found>";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        FNum_ID = "D0";
                        DisplayName1 += " <Error - Item Not Found>";
                    }
                }
                else
                {
                    FNum_ID = "D0";
                    gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
                }
                listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(DisplayName1, gf.PB_CJKGroupStyle, ref FirstCharSym));
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add(DisplayName1);
                listViewItem.SubItems.Add(FNum_ID);
                listViewItem.SubItems.Add(FirstCharSym);
                listViewItem.SubItems.Add(text4);
            }
        }

        private void PopulateWorshipList()
        {
            bool flag = false;
            SessionList.Items.Clear();
            if (!Directory.Exists(gf.WorshipDir))
            {
                FileUtil.MakeDir(gf.WorshipDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(gf.WorshipDir);
            FileInfo[] files = directoryInfo.GetFiles("*.esw");
            foreach (FileInfo fileInfo in files)
            {
                string InFileName = fileInfo.Name;
                InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                if (InFileName != "")
                {
                    SessionList.Items.Add(InFileName);
                    if (gf.CurSession == InFileName)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                SessionList.Text = gf.CurSession;
            }
            else if (SessionList.Items.Count > 0)
            {
                SessionList.SelectedIndex = 0;
                gf.CurSession = SessionList.Items[0].ToString();
            }
            else
            {
                gf.CurSession = "Worship Service";
                FileUtil.CreateNewFile(gf.WorshipDir + gf.CurSession + ".esw");
                SessionList.Items.Add(gf.CurSession);
                SessionList.SelectedIndex = 0;
            }
            SessionList_Change();
        }

        private void PopulatePraiseBooksList()
        {
            bool flag = false;
            PraiseBook.Items.Clear();
            if (!Directory.Exists(gf.PraiseBookDir))
            {
                FileUtil.MakeDir(gf.PraiseBookDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(gf.PraiseBookDir);
            FileInfo[] files = directoryInfo.GetFiles("*.esp");
            foreach (FileInfo fileInfo in files)
            {
                string InFileName = fileInfo.Name;
                InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                if (InFileName != "")
                {
                    PraiseBook.Items.Add(InFileName);
                    if (gf.CurPraiseBook == InFileName)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                PraiseBook.Text = gf.CurPraiseBook;
            }
            else if (PraiseBook.Items.Count > 0)
            {
                PraiseBook.SelectedIndex = 0;
                gf.CurPraiseBook = PraiseBook.Items[0].ToString();
            }
            else
            {
                gf.CurPraiseBook = "PraiseBook 1";
                FileUtil.CreateNewFile(gf.PraiseBookDir + gf.CurPraiseBook + ".esp");
                PraiseBook.Items.Add(gf.CurPraiseBook);
                PraiseBook.SelectedIndex = 0;
            }
            PraiseBook.Text = gf.CurPraiseBook;
        }

        private void ApplyUseSongNumbers(bool InUseSongNumbers)
        {
            gf.UseSongNumbers = InUseSongNumbers;
            Menu_UseSongNumbering.Checked = gf.UseSongNumbers;
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
            SetSortButtonPB(gf.PB_CJKGroupStyle);
        }

        private void BuildFolderList()
        {
            ImplementFolderChange = false;
            int num = 0;
            int num2 = 0;
            string text = "";
            string text2 = "";
            int num3;
            if (SongFolder.Items.Count > 0)
            {
                if (SongFolder.SelectedIndex < 0)
                {
                    SongFolder.SelectedIndex = 0;
                }
                num3 = gf.GetFolderNumber(SongFolder.SelectedText);
            }
            else
            {
                num3 = 0;
            }
            SongFolder.Items.Clear();
            for (int i = 1; i < gf.MAXSONGSFOLDERS; i++)
            {
                if (gf.FolderUse[i] > 0)
                {
                    SongFolder.Items.Add(gf.FolderName[i]);
                    if (gf.GetFolderNumber(gf.FolderName[i]) == num3)
                    {
                        num = i;
                        text = gf.FolderName[i];
                    }
                    if (num2 == 0)
                    {
                        num2 = i;
                        text2 = gf.FolderName[i];
                    }
                }
            }
            if (text == "")
            {
                text = text2;
            }
            SongFolder.Text = text;
            ImplementFolderChange = true;
            SongFolder_Change();
        }

        /// <summary>
        /// daniel find
        /// </summary>
        /// <param name="FNumber"></param>
        /// <param name="ListString"></param>
        /// <param name="InItemMusicOnly"></param>
        private void FillList(int FNumber, string ListString, bool InItemMusicOnly)
        {
            int num = 0;
            string text8 = "";
            string text9 = "";
            int num2 = 0;
            string text10 = "%";

            gf.TotalMusicFiles = -1;
            bool flag = false;
            string text11 = "";
            if (FNumber == 0)
            {
                for (int i = 1; i < gf.MAXSONGSFOLDERS; i++)
                {
                    if (gf.FindSongsFolder[i])
                    {
                        text11 = ((!(text11 == "")) ? ("{text11} or FolderNo = {Convert.ToString(i)}") : ($" and (FolderNo = {Convert.ToString(i)}"));
                    }
                }
                text11 += ")";
            }
            else
            {
                text11 = " and FolderNo=" + Convert.ToString(FNumber);
            }
            //string str = (FNumber < 0) ? gf.Find_SQLString : ("select * from SONG where (lower(Title_1) like \"" + text10.ToLower() + "\" " + text11 + ") or (lower(Title_2) like \"" + text10.ToLower() + "\" " + text11 + ")");
            string str = (FNumber < 0) ? gf.Find_SQLString : ($"select * from SONG where (lower(Title_1) like \"{text10.ToLower()}\" {text11}) or (lower(Title_2) like \"{text10.ToLower()}\" {text11})");

            string str2 = gf.UseSongNumbers ? " order by song_number, cjk_strokecount" : ((CurStyle != SortBy.WordCount) ? " order by cjk_strokecount" : " order by cjk_wordcount, cjk_strokecount");
            str += str2;

            string sQuery = str;
            string sQueryCount = str.Replace("*", "Count(*)");

            string sMultiQuery = $"{sQueryCount};{sQuery}";

            ListViewItem listViewItem = new ListViewItem();
            SongsList.BeginUpdate();
            try
            {
                DbConnection connection = null;
                DbDataReader dataReader = null;

                (connection, dataReader) = DbController.GetDataReader(gf.ConnectStringMainDB, sMultiQuery);

                using (connection)
                {
                    using (dataReader)
                    {
                        if (dataReader != null && dataReader.HasRows)
                        {
                            dataReader.Read();

                            ListViewItem[] array = new ListViewItem[DataUtil.ObjToInt(dataReader[0])];
                            int num3 = 0;
                            dataReader.NextResult();

                            while (dataReader.Read())
                            {
                                string musicTitle = DataUtil.ObjToString(dataReader["Title_2"]);
                                num2 = DataUtil.ObjToInt(dataReader["song_number"]);
                                bool flag2 = gf.MusicFound(DataUtil.ObjToString(dataReader["Title_1"]), musicTitle);
                                text8 = DataUtil.ObjToString(dataReader["LICENCE_ADMIN1"]);
                                text9 = DataUtil.ObjToString(dataReader["LICENCE_ADMIN2"]);
                                if ((InItemMusicOnly && flag2) || !InItemMusicOnly)
                                {
                                    array[num3] = new ListViewItem(new string[7]
                                    {
                                DataUtil.ObjToString(dataReader["Title_1"]) + (flag2 ? " <#>" : ""),
                                "D" + DataUtil.ObjToString(dataReader["SongID"]),
                                "",
                                "",
                                num2.ToString(),
                                text8,
                                text9
                                    });
                                    num3++;
                                }
                            }
                            SongsList.Items.AddRange(array);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            SongsList.EndUpdate();
            ShowStatusBarSummary();
        }

        private void ShowPicturesFolderThumbs()
        {
            if (InitFormLoad)
            {
                return;
            }
            BackgroundCurImagePath = "";
            BackgroundTotalImagesCount = 0;
            if (ImagesFolder.Items.Count <= 0)
            {
                return;
            }
            BackgroundCurImagePath = gf.PictureGroups[ImagesFolder.SelectedIndex, 1];
            string[] array = new string[5]
            {
                ".jpg",
                ".jpeg",
                ".bmp",
                ".gif",
                ".ico"
            };
            for (int i = 0; i < BackgroundImagename.Length; i++)
            {
                BackgroundImagename[i] = "";
            }
            ListBox listBox = new ListBox();
            listBox.Items.Clear();
            listBox.Sorted = false;
            BackgroundTotalImagesCount = 0;
            if (BackgroundCurImagePath != "")
            {
                for (int j = 0; j <= 4; j++)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(BackgroundCurImagePath, "*" + array[j]);
                        string[] array2 = files;
                        foreach (string text in array2)
                        {
                            string text2 = text;
                            listBox.Items.Add(text);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            listBox.Sorted = true;
            BackgroundTotalImagesCount = listBox.Items.Count;
            for (int i = 0; i < ((BackgroundTotalImagesCount < 1024) ? BackgroundTotalImagesCount : 1023); i++)
            {
                BackgroundImagename[i] = listBox.Items[i].ToString();
            }
            FormatBackgroundThumbContainers();
            listBox.Dispose();
        }

        private void LoadBackgroundThumbImages()
        {
            LoadThumbImages(flowLayoutImages, ref BackgroundImagesCanvas, BackgroundImagename, BackgroundTotalImagesCount, tabControlSource.Width - 15, "", 0, toolTip1, ExternalPP: false);
        }

        private void ShowPowerpointFolderContents(bool ShowThumbs)
        {
            Cursor = Cursors.WaitCursor;
            ExternalPPCurImagePath = "";
            ExternalPPTotalImagesCount = 0;
            PowerpointList.Items.Clear();
            if (PowerpointFolder.Items.Count <= 0)
            {
                return;
            }
            ExternalPPCurImagePath = gf.PowerpointGroups[PowerpointFolder.SelectedIndex, 1];
            for (int i = 0; i < ExternalPPImagename.Length; i++)
            {
                ExternalPPImagename[i] = "";
            }
            ListBox listBox = new ListBox();
            ListViewItem listViewItem = new ListViewItem();
            listBox.Items.Clear();
            listBox.Sorted = false;
            ExternalPPTotalImagesCount = 0;
            if (ExternalPPCurImagePath != "")
            {
                try
                {
                    // daniel
                    //string[] files1 = Directory.GetFiles(ExternalPPCurImagePath, "*.ppt");
                    var files = Directory.GetFiles(ExternalPPCurImagePath, "*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".ppt") || s.EndsWith(".pptx"));
                    string[] array = files.ToArray();
                    foreach (string text in array)
                    {
                        string text2 = text;
                        listBox.Items.Add(text);
                    }
                }
                catch
                {
                }
            }
            listBox.Sorted = true;
            ExternalPPTotalImagesCount = listBox.Items.Count;
            string text3 = "";
            for (int i = 0; i < ((ExternalPPTotalImagesCount < 1024) ? ExternalPPTotalImagesCount : 1023); i++)
            {
                if (gf.PowerpointListingStyle == 0)
                {
                    text3 = listBox.Items[i].ToString();
                    listViewItem = PowerpointList.Items.Add(gf.GetDisplayNameOnly(ref text3, UpdateByRef: false, KeepExt: false));
                    listViewItem.SubItems.Add("P" + listBox.Items[i]);
                }
                else
                {
                    ExternalPPImagename[i] = listBox.Items[i].ToString();
                }
            }
            SetPowerpointListColWidth();
            if (gf.PowerpointListingStyle == 1)
            {
                gf.ExtPPrefix_Num++;
                if (!Directory.Exists(gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\"))
                {
                    FileUtil.MakeDir(gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\");
                }
                gf.ExternalPPT.BuildFirstScreenDump(ExternalPPImagename, listBox.Items.Count, gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\");
                FormatExternalPowerpointThumbContainers();
            }
            SetPowerpointListColWidth();
            listBox.Dispose();
            Cursor = Cursors.Default;
        }

        private void LoadExternalPowerpointThumbImages(int GotoSlide)
        {
            LoadThumbImages(flowLayoutExternalPowerPoint, ref Powerpoint_ExternalCanvas, ExternalPPImagename, ExternalPPTotalImagesCount, tabControlSource.Width - 10, "", GotoSlide, toolTip1, ExternalPP: true);
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
                        gf.LoadBiblePassagesFromTabIndex(TabBibleVersions.SelectedIndex, BookLookup, ref BibleText, gf.HB_ShowVerses);
                        gf.HB_SequentialListing = true;
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
                    //?? ???? ?? ???????? ?? ???? ??
                    gf.LoadBiblePassagesFromTabIndex(TabBibleVersions.SelectedIndex, BookLookup, ref BibleText, gf.HB_ShowVerses);
                    gf.HB_SequentialListing = true;
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
            if (gf.LoadBibleBooksList(TabBibleVersions, ref BookLookup, HB_SearchInProgress, null))
            {
                gf.HB_CurVersionTabIndex = TabBibleVersions.SelectedIndex;
                if ((BookLookup.SelectedIndex == 66) & (gf.HB_SQLString == ""))
                {
                    Cursor = Cursors.WaitCursor;
                    gf.RefreshBiblePassages(gf.HB_CurVersionTabIndex, BookLookup, ref BibleText, gf.HB_ShowVerses);
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
            if (!((BibleText.Text == "") | (HB_CurSelectedPassages == "")))
            {
                try
                {
                    string InString = HB_CurSelectedPassages;
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num3 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num4 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num5 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num6 = num;
                    while (InString != "")
                    {
                        num6 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, ';'));
                        DataUtil.ExtractOneInfo(ref InString, ';');
                        DataUtil.ExtractOneInfo(ref InString, ';');
                        num4 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                        num5 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    }
                    BibleText.Focus();
                    for (int i = 1; i <= gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if ((gf.HB_VersesLocation[i, 1] == num) & (gf.HB_VersesLocation[i, 2] == num2) & (gf.HB_VersesLocation[i, 3] == num3))
                        {
                            BibleText.SelectionStart = gf.HB_VersesLocation[i, 4];
                            for (int j = i; j <= gf.HB_VersesLocation[0, 0]; j++)
                            {
                                if ((gf.HB_VersesLocation[j, 1] == num6) & (gf.HB_VersesLocation[j, 2] == num4) & (gf.HB_VersesLocation[j, 3] == num5))
                                {
                                    BibleText.SelectionLength = gf.HB_VersesLocation[j, 4] - gf.HB_VersesLocation[i, 4] + gf.HB_VersesLocation[j, 5] - 2;
                                    BibleText.ScrollToCaret();
                                    j = gf.HB_VersesLocation[0, 0];
                                }
                            }
                            i = gf.HB_VersesLocation[0, 0];
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
            if (gf.HB_TotalVersions >= 1)
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
                string displayNameOnly = gf.GetDisplayNameOnly(ref gf.HB_Versions[InBibleVersion, 4], UpdateByRef: false, KeepExt: true);
                int num = 0;
                int num2 = 0;
                string text = "";
                int num3 = BibleText.SelectionStart + 2;
                int num4 = num3 + BibleText.SelectionLength;
                if (num3 >= 0)
                {
                    for (int i = 1; i <= gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if (!((num3 >= gf.HB_VersesLocation[i, 4]) & (num3 <= gf.HB_VersesLocation[i, 4] + gf.HB_VersesLocation[i, 5])))
                        {
                            continue;
                        }
                        num = i;
                        for (int j = i; j <= gf.HB_VersesLocation[0, 0]; j++)
                        {
                            if ((num4 >= gf.HB_VersesLocation[j, 4]) & (num4 <= gf.HB_VersesLocation[j, 4] + gf.HB_VersesLocation[j, 5]))
                            {
                                num2 = j;
                                j = 3002;
                            }
                        }
                        i = 3002;
                    }
                }
                int num5 = num2 - num + 1;
                if (gf.HB_SequentialListing)
                {
                    if (num5 > gf.HB_MaxVersesSelection)
                    {
                        num2 = num + gf.HB_MaxVersesSelection - 1;
                    }
                }
                else if (num5 > gf.HB_MaxAdhocVersesSelection)
                {
                    num2 = num + gf.HB_MaxAdhocVersesSelection - 1;
                }
                if (num >= 0)
                {
                    BibleText.SelectionStart = gf.HB_VersesLocation[num, 4];
                    if (num2 < 0)
                    {
                        num2 = num;
                    }
                    BibleText.SelectionLength = gf.HB_VersesLocation[num2, 4] - gf.HB_VersesLocation[num, 4] + gf.HB_VersesLocation[num2, 5] - 2;
                }
                BibleText.ScrollToCaret();
                if (gf.HB_SequentialListing)
                {
                    string text2 = Convert.ToString(gf.HB_VersesLocation[num, 2]) + ":" + Convert.ToString(gf.HB_VersesLocation[num, 3]);
                    string text3 = " - " + Convert.ToString(gf.HB_VersesLocation[num2, 2]) + ":" + Convert.ToString(gf.HB_VersesLocation[num2, 3]);
                    if (gf.HB_VersesLocation[num, 2] == gf.HB_VersesLocation[num2, 2])
                    {
                        text3 = ((gf.HB_VersesLocation[num, 3] != gf.HB_VersesLocation[num2, 3]) ? ("-" + Convert.ToString(gf.HB_VersesLocation[num2, 3])) : "");
                    }
                    OutputTitle = gf.LookUpBookName(InBibleVersion, gf.HB_VersesLocation[num, 1]) + " " + text2 + text3 + " (" + gf.HB_Versions[InBibleVersion, 1] + ")";
                    text = "0" + ';' + displayNameOnly + ';' + ';';
                    int i;
                    for (i = num; i <= num2; i++)
                    {
                        int num6;
                        for (num6 = i; gf.HB_VersesLocation[i, 2] == gf.HB_VersesLocation[num6, 2] && i <= num2; i++)
                        {
                        }
                        int j = i - 1;
                        object obj = text;
                        text = string.Concat(obj, Convert.ToString(gf.HB_VersesLocation[num6, 1]), ';', Convert.ToString(gf.HB_VersesLocation[num6, 2]), ';', Convert.ToString(gf.HB_VersesLocation[num6, 3]), ';', Convert.ToString(gf.HB_VersesLocation[j, 2]), ';', Convert.ToString(gf.HB_VersesLocation[j, 3]), ';');
                        i = j;
                    }
                }
                else
                {
                    text = "1" + ';' + displayNameOnly + ';' + ';';
                    for (int i = num; i <= num2; i++)
                    {
                        string text4 = OutputTitle;
                        OutputTitle = text4 + DataUtil.Trim(DataUtil.Left(gf.LookUpBookName(InBibleVersion, gf.HB_VersesLocation[i, 1]), 4)) + " " + Convert.ToString(gf.HB_VersesLocation[i, 2]) + ":" + Convert.ToString(gf.HB_VersesLocation[i, 3]) + ",";
                        object obj = text;
                        text = string.Concat(obj, Convert.ToString(gf.HB_VersesLocation[i, 1]), ';', Convert.ToString(gf.HB_VersesLocation[i, 2]), ';', Convert.ToString(gf.HB_VersesLocation[i, 3]), ';', Convert.ToString(gf.HB_VersesLocation[i, 2]), ';', Convert.ToString(gf.HB_VersesLocation[i, 3]), ';');
                    }
                    OutputTitle = DataUtil.Left(OutputTitle, OutputTitle.Length - 1);
                    if (OutputTitle.Length > 60)
                    {
                        OutputTitle = DataUtil.Left(OutputTitle, 60) + " .. ";
                    }
                    OutputTitle = OutputTitle + " (" + gf.HB_Versions[InBibleVersion, 1] + ")";
                }
                return text;
            }
            return "";
        }

        private void SessionList_Change()
        {
            gf.CurSession = SessionList.Text;
            Cursor = Cursors.WaitCursor;
            LoadWorshipList(0);
            WriteCurSession();
            if (gf.ShowRunning)
            {
                ValidateWorshipListItems(ShowErrorMessage: false);
                gfFileHelpers.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
            }
            Cursor = Cursors.Default;
        }

        private void WriteCurSession()
        {
            RegUtil.SaveRegValue("config", "current_session", gf.CurSession);
        }

        private void WriteCurPraiseBook()
        {
            RegUtil.SaveRegValue("config", "current_praisebook", gf.CurPraiseBook);
        }

        private void LoadWorshipList(int DataType)
        {
            string inFileName = gf.WorshipDir + gf.CurSession + ".esw";
            LoadWorshipList(DataType, inFileName);
        }

        private string LoadWorshipList(int DataType, string InFileName)
        {
            gf.StartPresAt = 0;
            string result = LoadIndexFile(DataType, InFileName, ref WorshipListItems, UsageMode.Worship, ref gf.CurSessionNotes);
            PreviewNotes.Text = gf.CurSessionNotes;
            return result;
        }

        private void LoadPraiseBook(int DataType)
        {
            string inFileName = gf.PraiseBookDir + gf.CurPraiseBook + ".esp";
            LoadIndexFile(DataType, inFileName, ref PraiseBookItems, UsageMode.PraiseBook, ref gf.CurPraiseBookNotes);
        }

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="InFileName"></param>
        /// <param name="InList"></param>
        /// <param name="InMode"></param>
        /// <param name="InNotes"></param>
        /// <returns></returns>
        private string LoadIndexFile(int DataType, string InFileName, ref ListView InList, UsageMode InMode, ref string InNotes)
        {
            string text = "";
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(InFileName);
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    xmlTextReader.Read();
                    while (xmlTextReader.Read() && !flag)
                    {
                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "EasiSlides"))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        while (xmlTextReader.Read() && !flag2)
                        {
                            if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListItem"))
                            {
                                flag2 = true;
                            }
                        }
                        if (!flag2)
                        {
                            xmlTextReader?.Close();
                        }
                        else
                        {
                            while (xmlTextReader.Read() && !flag3)
                            {
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListHeader"))
                                {
                                    flag3 = true;
                                }
                            }
                            if (!flag3)
                            {
                                xmlTextReader?.Close();
                            }
                            else
                            {
                                string text2 = "";
                                string displayName = "";
                                string folderName = "";
                                string formatString = "";
                                xmlTextReader.Read();
                                gf.WorshipListIDOK = false;
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "SystemID"))
                                {
                                    if (gf.SystemID == xmlTextReader.ReadElementContentAsString())
                                    {
                                        gf.WorshipListIDOK = true;
                                    }
                                    xmlTextReader.Read();
                                    if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "FormatData"))
                                    {
                                        text = xmlTextReader.ReadElementContentAsString();
                                        if (DataType == 2)
                                        {
                                            return text;
                                        }
                                        gf.LoadHeaderData(text, ref gf.HeaderData, '>');
                                        if (DataType == 1)
                                        {
                                            gf.ApplyHeaderData();
                                            if (InMode == UsageMode.Worship)
                                            {
                                                UpdateDefaultFields();
                                                UpdateDisplayPanelFields();
                                            }
                                            return text;
                                        }
                                        gf.ApplyHeaderData();
                                        InList.Items.Clear();
                                        InNotes = "";
                                        xmlTextReader.Read();
                                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "Notes"))
                                        {
                                            InNotes = xmlTextReader.ReadElementContentAsString();
                                        }
                                        DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
                                        while (xmlTextReader.Read())
                                        {
                                            switch (xmlTextReader.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    switch (xmlTextReader.Name)
                                                    {
                                                        case "ItemID":
                                                            text2 = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Title1":
                                                            displayName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Folder":
                                                            folderName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "FormatData":
                                                            formatString = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                    }
                                                    break;
                                                case XmlNodeType.EndElement:
                                                    if (text2 != "")
                                                    {
                                                        if (InMode == UsageMode.Worship)
                                                        {
                                                            WriteItemtoWorshipList(connection, DataUtil.Left(text2, 1), DataUtil.Right(text2, text2.Length - 1), displayName, folderName, formatString, -1);
                                                        }
                                                        else
                                                        {
                                                            WriteItemtoPraiseBook(connection, DataUtil.Left(text2, 1), DataUtil.Right(text2, text2.Length - 1), displayName, folderName);
                                                        }
                                                        text2 = "";
                                                        displayName = "";
                                                        folderName = "";
                                                        formatString = "";
                                                    }
                                                    break;
                                            }
                                        }
                                        xmlTextReader?.Close();
                                        goto IL_0417;
                                    }
                                    xmlTextReader?.Close();
                                }
                                else
                                {
                                    xmlTextReader?.Close();
                                }
                            }
                        }
                        goto IL_0454;
                    }
                }
                catch (Exception e)
                {
                    xmlTextReader?.Close();
                }
            }
            catch (Exception ex)
            {
            }
            if (InMode == UsageMode.Worship)
            {
                Load32WorshipList(DataType);
            }
            else
            {
                Load32PraiseBook(DataType);
            }
            return "";
        IL_0417:
            LoadIndexFilePostAction(InMode);
            return text;
        IL_0454:
            LoadIndexFilePostAction(InMode);
            return "";
        }

        private void Load32WorshipList(int DataType)
        {
            gf.TotalMusicFiles = -1;
            string inFileName = gf.WorshipDir + gf.CurSession + ".esw";
            gfFileHelpers.LoadFileContents(inFileName, ref InContents);
            int num = gf.Load32HeaderData(inFileName, InContents, ref gf.HeaderData);
            if (num < 1)
            {
                InContents = "";
            }
            switch (DataType)
            {
                case 2:
                    return;
                case 1:
                    gf.ApplyHeaderData();
                    UpdateDefaultFields();
                    return;
            }
            gf.ApplyHeaderData();
            WorshipListItems.Items.Clear();
            InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
            int num2 = 0;
            int num3 = InContents.IndexOf(">");
            try
            {

                DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
                while (num3 >= 0)
                {
                    string text = DataUtil.Trim(DataUtil.Mid(InContents, num2, num3 - num2));
                    int num4 = 0;
                    gf.SongFormatData = "";
                    num4 = text.IndexOf('*');
                    if (num4 >= 0)
                    {
                        gf.SongFormatData = DataUtil.Trim(DataUtil.Right(text, text.Length - num4 - 1));
                        text = DataUtil.Trim(DataUtil.Left(text, num4));
                    }
                    if (text != "")
                    {
                        int num5 = text.IndexOf("\\");
                        string fNum_ID = DataUtil.Mid(text, 1, num5 - 1);
                        int num6 = text.IndexOf("\\", num5 + 1);
                        int num7 = num6;
                        while (num7 >= 0)
                        {
                            num7 = text.IndexOf("\\", num7 + 1);
                            if (num7 >= 0)
                            {
                                num6 = num7;
                            }
                        }
                        if (num6 < 0)
                        {
                            num6 = text.Length + 1;
                        }
                        string displayName = DataUtil.Mid(text, num5 + 1, num6 - (num5 + 1));
                        string inSym = DataUtil.Left(text, 1);
                        gf.WorshipListIDOK = true;
                        DataUtil.Convertv32FormatString(ref gf.SongFormatData, '*');

                        WriteItemtoWorshipList(connection, inSym, fNum_ID, displayName, "", gf.SongFormatData, -1);


                    }
                    text = "";
                    num2 = num3 + 1;
                    num3 = InContents.IndexOf(">", num2);
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
                if ((WorshipListItems.Items.Count > 0) & (gf.PreviewItem.Source == ItemSource.WorshipList))
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
                SetSortButtonPB(gf.PB_CJKGroupStyle);
                if (PraiseBookItems.Items.Count > 0)
                {
                    PraiseBookItems.Items[0].Selected = true;
                    //PraiseBookItems_SelectedIndexChanged(null, null);
                }
                SavePraiseBook();
            }
            Cursor = Cursors.Default;
        }

        private bool InsertIndexFileItems(string InFileName, ref ListView InList, int AddToLocation, ref string InNotes)
        {
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(InFileName);
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    xmlTextReader.Read();
                    while (xmlTextReader.Read() && !flag)
                    {
                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "EasiSlides"))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        while (xmlTextReader.Read() && !flag2)
                        {
                            if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListItem"))
                            {
                                flag2 = true;
                            }
                        }
                        if (!flag2)
                        {
                            xmlTextReader?.Close();
                        }
                        else
                        {
                            while (xmlTextReader.Read() && !flag3)
                            {
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListHeader"))
                                {
                                    flag3 = true;
                                }
                            }
                            if (!flag3)
                            {
                                xmlTextReader?.Close();
                            }
                            else
                            {
                                string text = "";
                                string displayName = "";
                                string folderName = "";
                                string formatString = "";
                                xmlTextReader.Read();
                                gf.WorshipListIDOK = false;
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "SystemID"))
                                {
                                    if (gf.SystemID == xmlTextReader.ReadElementContentAsString())
                                    {
                                        gf.WorshipListIDOK = true;
                                    }
                                    xmlTextReader.Read();
                                    if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "FormatData"))
                                    {
                                        string text2 = xmlTextReader.ReadElementContentAsString();
                                        xmlTextReader.Read();
                                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "Notes"))
                                        {
                                            InNotes += xmlTextReader.ReadElementContentAsString();
                                        }
                                        DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);

                                        while (xmlTextReader.Read())
                                        {
                                            switch (xmlTextReader.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    switch (xmlTextReader.Name)
                                                    {
                                                        case "ItemID":
                                                            text = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Title1":
                                                            displayName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Folder":
                                                            folderName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "FormatData":
                                                            formatString = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                    }
                                                    break;
                                                case XmlNodeType.EndElement:
                                                    if (text != "")
                                                    {

                                                        WriteItemtoWorshipList(connection, DataUtil.Left(text, 1), DataUtil.Right(text, text.Length - 1), displayName, folderName, formatString, AddToLocation);

                                                        text = "";
                                                        displayName = "";
                                                        folderName = "";
                                                        formatString = "";
                                                        AddToLocation++;
                                                    }
                                                    break;
                                            }
                                        }
                                        xmlTextReader?.Close();
                                        goto IL_0360;
                                    }
                                    xmlTextReader?.Close();
                                }
                                else
                                {
                                    xmlTextReader?.Close();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    xmlTextReader?.Close();
                }
            }
            catch
            {
            }
            return false;
        IL_0360:
            return true;
        }

        private void WriteItemtoWorshipList(DbConnection connection, string InSym, string FNum_ID, string DisplayName1, string FolderName, string FormatString, int AddToLocation)
        {
            if (FNum_ID == "")
            {
                return;
            }
            ListViewItem listViewItem = new ListViewItem();
            string musicTitle = "";
            string musicTitle2 = gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: false);
            string text = "";
            string text2 = "";
            string text3 = "0";
            if (InSym == "D")
            {
                bool flag = false;
                try
                {
                    //SQLite LCase()  -> lower() ????UCase() -> upper()
                    //string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where LCase(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
                    string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where lower(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");

                    DataRow dr = DbController.GetDataRowScalar(connection, fullSearchString);
                    if (dr != null)
                    {
                        if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && gf.FolderUse[DataUtil.GetDataInt(dr, "FolderNo")] > 0)
                        {
                            DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                            musicTitle = DataUtil.GetDataString(dr, "Title_2");
                            text = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
                            text2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
                            FolderName = gf.FolderName[DataUtil.GetDataInt(dr, "FolderNo")];
                            FNum_ID = "D" + DataUtil.GetDataInt(dr, "songid");
                            text3 = ((DataUtil.GetDataString(dr, "song_number") != "") ? DataUtil.GetDataString(dr, "song_number") : "0");
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        DisplayName1 += " <Error - Item Not Found>";
                        FNum_ID = "D0";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    FNum_ID = "D0";
                    DisplayName1 += " <Error - Item Not Found>";
                }
            }
            else if (InSym == "P")
            {
                FNum_ID = "P" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "B")
            {
                FNum_ID = "B" + FNum_ID;
            }
            else if (InSym == "T")
            {
                FNum_ID = "T" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "I")
            {
                FNum_ID = "I" + DisplayName1;
                string InTitle = "";
                gf.LoadIndividualData(ref gf.TempItem1, FNum_ID, "", 1, ref InTitle);
                musicTitle2 = gf.TempItem1.Title;
                musicTitle = gf.TempItem1.Title2;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "W")
            {
                FNum_ID = "W" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "M")
            {
                FNum_ID = "M" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            if (DisplayName1 != "")
            {
                if (gf.MusicFound(musicTitle2, musicTitle))
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
                if (InSym == "D")
                {
                    listViewItem.ImageIndex = 0;
                }
                else if (InSym == "P")
                {
                    listViewItem.ImageIndex = 2;
                }
                else if (InSym == "B")
                {
                    listViewItem.ImageIndex = 4;
                }
                else if (InSym == "T")
                {
                    listViewItem.ImageIndex = 6;
                }
                else if (InSym == "I")
                {
                    listViewItem.ImageIndex = 8;
                }
                else if (InSym == "W")
                {
                    listViewItem.ImageIndex = 10;
                }
                else if (InSym == "M")
                {
                    listViewItem.ImageIndex = 28;
                }
                listViewItem.SubItems.Add(FNum_ID);
                listViewItem.SubItems.Add(FormatString);
                listViewItem.SubItems.Add(text3);
                listViewItem.SubItems.Add(text);
                listViewItem.SubItems.Add(text2);
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add(FolderName);
            }
        }

        private void SetMainDefaultBackScreen()
        {
            gf.SetDefaultBackScreen(ref PreviewScreen);
            gf.SetDefaultBackScreen(ref OutputScreen);
            if (gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_DefaultBackgroundChanged);
            }
        }

        /// <summary>
        /// daniel
        /// </summary>
        private void WorshipListIndexChanged()
        {
            LoadThumbPreviewlockkey = 0;
            WorshipListIndexChanged(0);
        }

        private void WorshipListIndexChanged(int StartingSlide)
        {
            WorshipListIndexChanged(StartingSlide, GetFirstItem: false);
        }

        static int preSelectedItemNum = -1;

        private void WorshipListIndexChanged(int StartingSlide, bool GetFirstItem)
        {
            gf.PreviewItem.Source = ItemSource.WorshipList;
            gf.TotalWorshipListItems = WorshipListItems.Items.Count;
            int num;
            if (GetFirstItem & (gf.TotalWorshipListItems > 0))
            {
                WorshipListItems.Items[0].Selected = true;
                num = 0;
            }
            else
            {
                num = gf.GetSelectedIndex(WorshipListItems);
            }

            if (num >= 0)
            {
                //?? ???????? ü? ????
                string InTitle = WorshipListItems.Items[num].SubItems[0].Text;
                string text = WorshipListItems.Items[num].SubItems[1].Text;
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = num + 1;
                gf.PreviewItem.TotalItems = WorshipListItems.Items.Count;

                string filePrefix = gf.SetPowerpointPreviewPrefix(gf.PreviewItem, useTitlePrefix: true);

                // ✅ 캐싱 최적화: 파일이 빌드되지 않았거나 다른 항목으로 변경된 경우에만 로드
                bool hasPptPathFromList = !string.IsNullOrEmpty(text)
                    && text[0] == 'P'
                    && text.Length > 1
                    && !string.IsNullOrWhiteSpace(text.Substring(1));
                if (!hasPptPathFromList
                    || !gf.PreviewPPT.IsBuildedFileCheck(gf.PreviewItem.Path, filePrefix, ref gf.PreviewItem.TotalSlides)
                    || preSelectedItemNum != num)
                {
                    LoadItem(ref gf.PreviewItem, text, WorshipListItems.Items[num].SubItems[2].Text, StartingSlide, ref InTitle, ScrollToCaret: true);
                    UpdateDisplayPanelFields();
                }

                preSelectedItemNum = num;
            }
            else
            {
                gf.InitialiseIndividualData(ref gf.PreviewItem);
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                NoIndividualFormat();
                BuildVerseButtons(gf.PreviewItem, Reset: true);
                gf.PreviewItem.Format.ShowSlideTransition = 0;
                gf.PreviewItem.Format.ShowItemTransition = 0;

                ResetMainPictureBox(ref gf.PreviewItem);
                ClearLyrics(ref flowLayoutPreviewLyrics);
                UpdateDisplayPanelFields();
            }
        }

        private void SaveWorshipList()
        {
            SaveWorshipList(PreloadPowerpoint: false);
        }

        private void SaveWorshipList(bool PreloadPowerpoint)
        {
            if (!UpdatingFormatFields)
            {
                gf.CurSession = SessionList.Text;
                for (int i = 1; i <= WorshipListItems.Items.Count; i++)
                {
                    gf.WorshipSongs[i, 2] = gf.RemoveMusicSym(DataUtil.Trim(WorshipListItems.Items[i - 1].Text));
                    gf.WorshipSongs[i, 0] = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[1].Text);
                    gf.WorshipSongs[i, 1] = DataUtil.Left(gf.WorshipSongs[i, 0], 1);
                    gf.WorshipSongs[i, 4] = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[2].Text);
                }
                gf.TotalWorshipListItems = WorshipListItems.Items.Count;
                gfFileHelpers.SaveIndexFile(gf.WorshipDir + gf.CurSession + ".esw", ref WorshipListItems, UsageMode.Worship, SaveAllItems: true, "", gf.CurSessionNotes);
                if (PreloadPowerpoint)
                {
                    gfFileHelpers.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
                }
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
        /// daniel
        /// </summary>
        /// <param name="InItem"></param>
        /// <param name="InIDString"></param>
        /// <param name="InFormatString"></param>
        /// <param name="StartingSlide"></param>
        /// <param name="InTitle"></param>
        /// <param name="ScrollToCaret"></param>
        private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide, ref string InTitle, bool ScrollToCaret)
        {
            string prevTitle = "";
            string nextTitle = "";
            int num = gf.StartPresAt;
            if (WorshipListItems.Items.Count > 0)
            {
                int num2 = -1;
                int num3 = -1;
                if (!InItem.OutputStyleScreen)
                {
                    num = gf.GetSelectedIndex(WorshipListItems) + 1;
                }
                if (InItem.CurItemNo == 0)
                {
                    num2 = num - 1;
                    num3 = num;
                }
                else
                {
                    num2 = num - 2;
                    num3 = num;
                }
                if (num2 < 0 && InItem.CurItemNo == 0)
                {
                    num2 = 0;
                }
                if (num3 >= WorshipListItems.Items.Count)
                {
                    num3 = ((InItem.CurItemNo != 0) ? (-1) : (WorshipListItems.Items.Count - 1));
                }
                if (num2 == num3 && num2 == 0)
                {
                    num2 = -1;
                }
                prevTitle = ((num2 >= 0) ? gf.RemoveMusicSym(WorshipListItems.Items[num2].SubItems[0].Text) : "");
                nextTitle = ((num3 >= 0) ? gf.RemoveMusicSym(WorshipListItems.Items[num3].SubItems[0].Text) : "");
            }
            string text = DataUtil.Left(InIDString, 1);
            bool flag = false;
            gf.InitialiseIndividualData(ref InItem, GapMedia.None, text);
            InItem.PrevTitle = prevTitle;
            InItem.NextTitle = nextTitle;
            MakePowerpointPreviewVisible(InItem, (text == "P") ? true : false);
            if (text == "P")
            {
                gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide);
                ValidatePowerpointItem(InItem);
                gf.LoadIndividualFormatData(ref InItem, "");
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
                    ShowOutputPPThumbs(InItem.CurSlide);
                }
                else
                {
                    ShowPreviewPPThumbs(InItem.CurSlide);
                }
                InItem.Format.ShowItemTransition = 0;
                InItem.Format.ShowSlideTransition = 0;
                if (gf.ShowRunning & InItem.OutputStyleScreen)
                {
                    InItem.TotalSlides = gf.RunPowerpointSong(ref InItem, ref MainPPT, StartingSlide, ShowResult: true);
                    if (gf.DualMonitorMode)
                    {
                        ShowDualMonitorPP_Preview(ref InItem);
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
                int num4;
                switch (text)
                {
                    default:
                        num4 = ((!(text == "G")) ? 1 : 0);
                        break;
                    case "D":
                    case "B":
                    case "T":
                    case "I":
                    case "W":
                    case "M":
                        num4 = 0;
                        break;
                }
                if (num4 == 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
                    if (InItem.Source == ItemSource.SongsList)
                    {
                        if (!InItem.OutputStyleScreen)
                        {
                            InFormatString = InItem.Format.DBStoredFormat;
                        }
                    }
                    else if (text == "I" || text == "G")
                    {
                        InFormatString = InItem.Format.FormatString;
                    }
                    gf.LoadIndividualFormatData(ref InItem, InFormatString);
                    SetItemFontSettings(ref InItem);
                    gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
                    if (InItem.OutputStyleScreen)
                    {
                        BuildVerseButtons(gf.OutputItem);
                        DisplayLyrics(gf.OutputItem, StartingSlide);
                    }
                    else
                    {
                        AllowIndividualFormat(AllowFormat: true, (!(InFormatString == "")) ? true : false);
                        UpdateFormatFields();
                        BuildVerseButtons(InItem);
                        DisplayLyrics(InItem, StartingSlide, ScrollToCaret);
                    }
                }
                else
                {
                    gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
                    AllowIndividualFormat(AllowFormat: true, (!(InFormatString == "")) ? true : false);
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
            string text = "";
            if (InItem.Type == "P" || InItem.Type == "W" || InItem.Type == "T" || InItem.Type == "I")
            {
                string itemID = InItem.ItemID;
                RichTextBox obj = InTextBox;
                obj.Text = obj.Text + InItem.ItemID + "\n";
            }
            if (InItem.Title2 != "")
            {
                RichTextBox obj2 = InTextBox;
                obj2.Text = obj2.Text + "Title2: " + InItem.Title2 + "\n";
            }
            if (InItem.Format.BackgroundPicture != "" && InItem.Type != "P")
            {
                if (InTextBox.Text != "")
                {
                    InTextBox.Text += "\n";
                }
                RichTextBox obj3 = InTextBox;
                obj3.Text = obj3.Text + "(Image: " + InItem.Format.BackgroundPicture + ")\n";
            }
            text = ((InItem.Format.MediaOption == 1 || InItem.Format.MediaOption == 2) ? gf.GetMediaLocation(InItem) : "");
            if (text != "")
            {
                RichTextBox obj4 = InTextBox;
                obj4.Text = obj4.Text + "(Media: " + text + ")\n";
            }
            if (InItem.Writer != "")
            {
                RichTextBox obj5 = InTextBox;
                obj5.Text = obj5.Text + "Writer: " + InItem.Writer + "\n";
            }
            if (InItem.Copyright != "")
            {
                RichTextBox obj6 = InTextBox;
                obj6.Text = obj6.Text + "Copyright: " + InItem.Copyright + "\n";
            }
            if (InItem.Capo > 0 || InItem.MusicKey != "" || InItem.Timing != "")
            {
                KeyCapoText = ((InItem.MusicKey != "") ? ("Key: " + InItem.MusicKey + " ") : "") + ((InItem.Capo > 0) ? (" Capo " + Convert.ToString(InItem.Capo) + " ") : "") + ((InItem.Timing != "") ? (" (" + InItem.Timing + ")") : "");
                RichTextBox obj7 = InTextBox;
                obj7.Text = obj7.Text + KeyCapoText + "\n";
            }
            else
            {
                KeyCapoText = "";
            }
            if (InItem.Book_Reference != "")
            {
                RichTextBox obj8 = InTextBox;
                obj8.Text = obj8.Text + "Book Ref: " + InItem.Book_Reference + "\n";
            }
            if (InItem.User_Reference != "")
            {
                RichTextBox obj9 = InTextBox;
                obj9.Text = obj9.Text + "User Ref: " + InItem.User_Reference + "\n";
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
            MessageBox.Show("Sorry - Can't find the Powerpoint File '" + InItem.Path + "'");
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

            string filePrefix = gf.SetPowerpointPreviewPrefix(InItem, useTitlePrefix: true);
            Console.WriteLine($"[PPTPath] FilePrefix='{filePrefix}'");

            if (InItem.OutputStyleScreen)
            {
                if (gf.OutputPPT.prePowerPointApp == null)
                {
                    Console.WriteLine("[PPTInit] Output prePowerPointApp is null; initializing now.");
                    try
                    {
                        gf.OutputPPT.Init();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PPTInit] Output init failed: {ex.Message}");
                    }
                }
                gf.OutputPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

                // ✅ 중복 호출 방지: 같은 ItemID면 캐시 확인
                if (OutputItem != InItem.ItemID)
                {
                    OutputItem = InItem.ItemID;
                }
                else
                {
                    if (gf.OutputPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
                    {
                        Console.WriteLine($"[Cache Hit] Output PPT already built: {InItem.ItemID}");
                        Console.WriteLine($"[ThumbPreview] Cache hit Output: TotalSlides={InItem.TotalSlides}");
                        if (InItem.TotalSlides > 0)
                        {
                            FormatPowerPointThumbContainers(ref Powerpoint_OutputCanvas, ref flowLayoutOutputPowerPoint, InItem.TotalSlides);
                        }
                        return;
                    }
                }
            }
            else
            {
                if (gf.PreviewPPT.prePowerPointApp == null)
                {
                    Console.WriteLine("[PPTInit] Preview prePowerPointApp is null; initializing now.");
                    try
                    {
                        gf.PreviewPPT.Init();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PPTInit] Preview init failed: {ex.Message}");
                    }
                }
                gf.PreviewPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

                // ✅ 중복 호출 방지: 같은 ItemID면 캐시 확인
                if (previwItem != InItem.ItemID)
                {
                    previwItem = InItem.ItemID;
                }
                else
                {
                    if (gf.PreviewPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
                    {
                        Console.WriteLine($"[Cache Hit] Preview PPT already built: {InItem.ItemID}");
                        Console.WriteLine($"[ThumbPreview] Cache hit Preview: TotalSlides={InItem.TotalSlides}");
                        if (InItem.TotalSlides > 0)
                        {
                            FormatPowerPointThumbContainers(ref Powerpoint_PreviewCanvas, ref flowLayoutPreviewPowerPoint, InItem.TotalSlides);
                        }
                        return;
                    }
                }
            }

            if (InItem.OutputStyleScreen)
            {
                bool outBuilt = gf.OutputPPT.BuildScreenOutDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, 9, 1000, ref InItem.SongVerses, ref InItem.Slide, gf.SequenceSymbol);
                Console.WriteLine($"[PPTBuild] BuildScreenOutDumps result={outBuilt}, TotalSlides={InItem.TotalSlides}");
                if (outBuilt)
                {
                    string firstOut = filePrefix + "1.jpg";
                    Console.WriteLine($"[PPTBuild] Output first image exists: {File.Exists(firstOut)} ({firstOut})");
                    FormatPowerPointThumbContainers(ref Powerpoint_OutputCanvas, ref flowLayoutOutputPowerPoint, InItem.TotalSlides);
                }
            }
            else
            {
                // ✅ 동기 방식 유지 (PowerPoint COM 객체는 STA 스레드에서만 안전)
                // 캐싱 및 이미지 최적화로 성능 개선
                bool preBuilt = gf.PreviewPPT.BuildScreenDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, 9, 1000, ref InItem.SongVerses, ref InItem.Slide, gf.SequenceSymbol);
                Console.WriteLine($"[PPTBuild] BuildScreenDumps result={preBuilt}, TotalSlides={InItem.TotalSlides}");
                if (preBuilt)
                {
                    string firstPre = filePrefix + "1.jpg";
                    Console.WriteLine($"[PPTBuild] Preview first image exists: {File.Exists(firstPre)} ({firstPre})");
                    FormatPowerPointThumbContainers(ref Powerpoint_PreviewCanvas, ref flowLayoutPreviewPowerPoint, InItem.TotalSlides);
                }
            }
        }

        private void PP_Preview(ref SongSettings InItem)
        {
            Cursor = Cursors.WaitCursor;
            InItem.Format.BackgroundPicture = (InItem.OutputStyleScreen ? gf.OUTPPFullPath : gf.PREPPFullPath) + InItem.CurSlide + ".jpg";
            InItem.Format.BackgroundMode = ImageMode.BestFit;
            InItem.UseDefaultFormat = false;
            if (InItem.OutputStyleScreen)
            {
                gf.SetShowBackground(InItem, ref OutputScreen);
            }
            else
            {
                gf.SetShowBackground(InItem, ref PreviewScreen);
            }
            SetItemFontSettings(ref InItem);
            gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
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
            bool flag = true;
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
                if (gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = clampedSlide;
                    ShowSlide(ref InItem, TransitionAction);
                }
                else
                {
                    gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
                }
            }
            else if (InItem.Type == "B" || InItem.Type == "T" || InItem.Type == "I" || InItem.Type == "W" || InItem.Type == "G")
            {
                if (gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = clampedSlide;
                    ShowSlide(ref InItem, TransitionAction);
                }
            }
            else if (InItem.Type == "M" || InItem.Type == "G")
            {
                if (gf.EasiSlidesMode == UsageMode.Worship)
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
                    ClearLyrics(ref flowLayoutOutputLyrics);
                }
                else
                {
                    ClearLyrics(ref flowLayoutPreviewLyrics);
                }
                flag = false;
            }
            if (flag)
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
                gf.ResetPictureBox(ref InItem, ref OutputScreen, GapItemBackground, TransitionAction);
            }
            else
            {
                gf.ResetPictureBox(ref InItem, ref PreviewScreen, GapItemBackground, TransitionAction);
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
                    if (gf.ShowRunning)
                    {
                        MainPPT.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, OfficeLibKeys.None, InItem.CurSlide);
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
            if (gf.ShowDBSlide(ref InItem, ref PreviewScreen, ref OutputScreen, DoActiveIndicator, TransitionAction))
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
            gf.PreviewItem.Source = ItemSource.SongsList;
            int selectedIndex = gf.GetSelectedIndex(SongsList);
            if (selectedIndex >= 0)
            {
                string text = SongsList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = SongsList.Items[selectedIndex].SubItems[0].Text;
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref gf.PreviewItem, text, "", StartingSlide, ref InTitle, ScrollToCaret);
                UpdateDisplayPanelFields();
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.Title = "";
                gf.PreviewItem.ItemID = "";
                gf.PreviewItem.CurItemNo = 0;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(gf.PreviewItem);
                DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void EnableEditHistory()
        {
            gf.LoadRegistryMainEditHistory();
            UpdateMenuEditHistory();
        }

        private void AddToEditHistory(string InItemID)
        {
            if (!((gf.GetItemTitle(InItemID) == "") | (gf.MainEditHistoryList[1, 0] == InItemID)))
            {
                if (gf.TotalMainEditHistory < gf.MaxUserEditHistory)
                {
                    gf.TotalMainEditHistory++;
                }
                else
                {
                    gf.TotalMainEditHistory = gf.MaxUserEditHistory;
                }
                for (int num = gf.TotalMainEditHistory; num >= 2; num--)
                {
                    gf.MainEditHistoryList[num, 0] = gf.MainEditHistoryList[num - 1, 0];
                }
                gf.MainEditHistoryList[1, 0] = InItemID;
                gf.RemoveDuplicateEditorHistoryItems(ref gf.MainEditHistoryList, ref gf.TotalMainEditHistory);
                UpdateMenuEditHistory();
                gf.SaveMainEditHistoryToRegistry();
            }
        }

        private void UpdateMenuEditHistory()
        {
            try
            {
                int num = 0;
                string text = "";
                string text2 = "";
                for (int i = 1; i <= gf.TotalMainEditHistory; i++)
                {
                    text2 = gf.GetItemTitle(gf.MainEditHistoryList[i, 0]);
                    if (text2 != "" && gf.MainEditHistoryList[num, 0] != gf.MainEditHistoryList[i, 0])
                    {
                        num++;
                        gf.MainEditHistoryList[num, 0] = gf.MainEditHistoryList[i, 0];
                        gf.MainEditHistoryList[num, 1] = text2;
                    }
                }
                gf.TotalMainEditHistory = num;
                for (int i = gf.TotalMainEditHistory + 1; i <= gf.AbsoluteMaxHitoryItems; i++)
                {
                    gf.MainEditHistoryList[i, 0] = "";
                    gf.MainEditHistoryList[i, 1] = "";
                }
                for (int i = 1; i < gf.AbsoluteMaxHitoryItems; i++)
                {
                    Menu_EditHistoryList.DropDownItems[i - 1].Text = i + " " + gf.MainEditHistoryList[i, 1];
                    if (i > gf.TotalMainEditHistory)
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Visible = false;
                    }
                    else
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Visible = true;
                        text = DataUtil.Left(gf.MainEditHistoryList[i, 0], 1);
                        if (text == "D")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.ES_16;
                        }
                        else if (text == "P")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.PPImg;
                        }
                        else if (text == "B")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.Bible;
                        }
                        else if (text == "T")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.notebook;
                        }
                        else if (text == "I")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.Info_Icon;
                        }
                        else if (text == "W")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.word;
                        }
                        else
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = null;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void ApplyBackground(int ImageTag, int InMode)
        {
            string directoryName = Path.GetDirectoryName(BackgroundImagename[ImageTag]);
            int backgroundImageMode = -1;
            if (directoryName == gf.RootEasiSlidesDir + "Images\\Scenery")
            {
                backgroundImageMode = 2;
            }
            else if (directoryName == gf.RootEasiSlidesDir + "Images\\Tiles")
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
                gf.BackgroundPicture = InImageFileName;
                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                {
                    gf.BackgroundMode = (ImageMode)BackgroundImageMode;
                    gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    gf.OutputItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                }
                SaveWorshipList();
                WorshipListIndexChanged(gf.PreviewItem.CurSlide);
                gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
                DisplayLyrics(gf.OutputItem, gf.OutputItem.CurSlide);
                if (gf.ShowRunning)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                }
            }
            else if ((InMode == 1) | (InMode == 2))
            {
                if (gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                    if (selectedIndex >= 0)
                    {
                        for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                        {
                            if (!WorshipListItems.Items[i].Selected)
                            {
                                continue;
                            }
                            if ((DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "D") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "B") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "T") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "M") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "W"))
                            {
                                gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                                {
                                    gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                                }
                                gf.PreviewItem.Format.FormatString = GetNewFormatString();
                                WorshipListItems.Items[i].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                            }
                            else if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "I")
                            {
                                gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                                {
                                    gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                                }
                                gf.PreviewItem.UseDefaultFormat = false;
                                gf.PreviewItem.Format.FormatString = GetNewFormatString();
                                gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
                                gf.DrawText(ref gf.PreviewItem, ref PreviewScreen, gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                                AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                                UpdateFormatFields();
                                SaveInfoFilePreview(ReloadImageData: true);
                            }
                        }
                        SaveWorshipList();
                        WorshipListIndexChanged(gf.PreviewItem.CurSlide);
                    }
                    else if (InMode == 2)
                    {
                        gf.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            gf.BackgroundMode = (ImageMode)BackgroundImageMode;
                            gf.OutputItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        SaveWorshipList();
                        WorshipListIndexChanged(gf.PreviewItem.CurSlide);
                        gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
                        DisplayLyrics(gf.OutputItem, gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.None);
                        if (gf.ShowRunning)
                        {
                            RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                        }
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    int selectedIndex = gf.GetSelectedIndex(SongsList);
                    if (selectedIndex >= 0)
                    {
                        gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, GetNewFormatString());
                        SongsListIndexChanged(gf.PreviewItem.CurSlide);
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.HolyBible)
                {
                    if (gf.PreviewItem.CompleteLyrics != "")
                    {
                        gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        gf.PreviewItem.UseDefaultFormat = false;
                        gf.PreviewItem.Format.FormatString = GetNewFormatString();
                        HB_CurSelectedFormat = gf.PreviewItem.Format.FormatString;
                        gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
                        gf.DrawText(ref gf.PreviewItem, ref PreviewScreen, gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                        AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                        UpdateFormatFields();
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen && gf.PreviewItem.CompleteLyrics != "")
                {
                    gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                    if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                    {
                        gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    }
                    gf.PreviewItem.UseDefaultFormat = false;
                    gf.PreviewItem.Format.FormatString = GetNewFormatString();
                    gf.PreviewItem.Format.TempImageFileName = gf.PreviewItem.Format.BackgroundPicture;
                    gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
                    gf.DrawText(ref gf.PreviewItem, ref PreviewScreen, gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                    AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                    UpdateFormatFields();
                    SaveInfoFilePreview(ReloadImageData: true);
                }
            }
            Def_NoImage.Enabled = ((gf.BackgroundPicture != "") ? true : false);
            Ind_NoImage.Enabled = ((gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
            UpdateWorshipShowIcons();
            UpdateDefaultNoImageButton();
            if (gf.PreviewItem.Type == "D")
            {
                gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString);
            }
        }

        private void UpdateWorshipShowIcons()
        {
            if (ResetWorshipShowIcons())
            {
                if (gf.OutputItem.CurItemNo > 0)
                {
                    try
                    {
                        if (WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 0 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 2 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 4 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 6 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 8 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 28 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 10)
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex + 1;
                            WorshipListItems.Items[gf.StartPresAt - 1].SubItems[6].Text = "O";
                        }
                        WorshipListItems.Items[gf.StartPresAt - 1].ForeColor = Color.Red;
                        WorshipListItems.Items[gf.StartPresAt - 1].EnsureVisible();
                    }
                    catch
                    {
                    }
                }
                else if ((gf.StartPresAt > 0) & (gf.StartPresAt <= WorshipListItems.Items.Count))
                {
                    try
                    {
                        if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "D")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 0;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "P")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 2;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "B")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 4;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "T")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 6;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "I")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 8;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "W")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 10;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "M")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 28;
                        }
                        else
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].SubItems[6].Text = "";
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
                    if (WorshipListItems.Items[i].ImageIndex == 1 || WorshipListItems.Items[i].ImageIndex == 3 || WorshipListItems.Items[i].ImageIndex == 5 || WorshipListItems.Items[i].ImageIndex == 7 || WorshipListItems.Items[i].ImageIndex == 9 || WorshipListItems.Items[i].ImageIndex == 29 || WorshipListItems.Items[i].ImageIndex == 11)
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
            gf.OutputItem.InMainItemText = gf.PreviewItem.InMainItemText;
            gf.OutputItem.InSubItemItem1Text = gf.PreviewItem.InSubItemItem1Text;
            gf.OutputItem.Source = gf.PreviewItem.Source;
            gf.OutputItem.CurItemNo = gf.PreviewItem.CurItemNo;
            gf.StartPresAt = ((gf.OutputItem.CurItemNo > 0) ? gf.OutputItem.CurItemNo : gf.StartPresAt);
            gf.OutputItem.OutputStyleScreen = true;
            if (gf.ShowRunning)
            {
                if (gf.OutputItem.CurItemNo == 0)
                {
                    gf.WorshipSongs[0, 2] = gf.OutputItem.InMainItemText;
                    gf.WorshipSongs[0, 0] = gf.OutputItem.InSubItemItem1Text;
                    gf.WorshipSongs[0, 1] = DataUtil.Left(gf.WorshipSongs[0, 0], 1);
                    gf.WorshipSongs[0, 4] = gf.PreviewItem.Format.FormatString;
                    gf.AdHocItemPresent = true;
                }
                if (gf.PreviewItem.Type == "P" && gf.PreviewItem.Source != ItemSource.WorshipList)
                {
                    gfFileHelpers.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
                }
            }
            LoadThumbOutlockkey = 0;
            LoadItem(ref gf.OutputItem, gf.PreviewItem.Type + gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString, gf.PreviewItem.CurSlide, ref gf.PreviewItem.Title, ScrollToCaret: true);
            UpdateWorshipShowIcons();
            if (gf.ShowRunning)
            {
                gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStoredItem;
                RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
            }
            FocusOutputArea();
        }

        private void PreViewToOutput()
        {
            if (gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
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
                TransitionAction = (ImageTransitionControl.TransitionAction)InItem.Format.ShowItemTransition;
            }
            InItem.CurSlide = StartingSlide;
            RefreshSlidesFonts(ref InItem, TransitionAction);
        }
    }
}
