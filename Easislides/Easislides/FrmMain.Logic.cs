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
            openFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            openFileDialog1.Title = "Load Session Settings from a Template";
            openFileDialog1.InitialDirectory = Gf.WorshipTemplatesDir;
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
                bool flag = false;
                Gf.SongFormatData = "";
                Gf.PreviewItem.UseDefaultFormat = false;
                if (Gf.PreviewItem.Type == "I")
                {
                    WorshipListItems.Items[selectedIndex].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
                }
                if (WorshipListItems.Items[selectedIndex].SubItems[2].Text == "")
                {
                    flag = true;
                }
                Gf.SongFormatData = WorshipListItems.Items[selectedIndex].SubItems[2].Text;
                Gf.LoadIndividualFormatData(ref Gf.PreviewItem, Gf.SongFormatData);
                UpdateFormatFields();
                if (flag)
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
            openFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            openFileDialog1.Title = "Load Individual Settings from a Template";
            openFileDialog1.InitialDirectory = Gf.SettingsTemplatesDir;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = openFileDialog1.FileName;
            string text = LoadWorshipList(2, fileName);
            Gf.PreviewItem.Format.FormatString = text;
            string type = Gf.PreviewItem.Type;
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
            if (Gf.PreviewItem.Source == ItemSource.SongsList)
            {
                Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, text);
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
            Gf.LoadIndividualFormatData(ref Gf.PreviewItem, text);
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
            saveFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
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
            Gf.PictureGroups[0, 0] = "Scenery";
            Gf.PictureGroups[0, 1] = Gf.RootEasiSlidesDir + "Images\\" + Gf.PictureGroups[0, 0] + "\\";
            Gf.PictureGroups[1, 0] = "Tiles";
            Gf.PictureGroups[1, 1] = Gf.RootEasiSlidesDir + "Images\\" + Gf.PictureGroups[1, 0] + "\\";
            Gf.PictureGroups[2, 0] = "Images";
            Gf.PictureGroups[2, 1] = Gf.ImagesDir;
            Gf.PicFolderTotal = 3;
            string[] directories = Directory.GetDirectories(Gf.ImagesDir);
            if (directories.Length > 0)
            {
                Gf.SingleArraySort(directories);
            }
            string text = Gf.RootEasiSlidesDir + "Images\\Scenery";
            string text2 = Gf.RootEasiSlidesDir + "Images\\Tiles";
            Gf.BuildSubFolderList(Gf.ImagesDir, Gf.ImagesDir, ref Gf.PictureGroups, ref Gf.PicFolderTotal);
            ImagesFolder.Items.Clear();
            for (int i = 0; i < Gf.PicFolderTotal; i++)
            {
                ImagesFolder.Items.Add(Gf.PictureGroups[i, 0]);
            }
        }

        private void BuildInfoScreenFolderList()
        {
            Gf.InfoScreenGroups[0, 0] = "InfoScreen Items";
            Gf.InfoScreenGroups[0, 1] = Gf.InfoScreenDir;
            Gf.InfoScreenFolderTotal = 1;
            Gf.BuildSubFolderList(Gf.InfoScreenDir, Gf.InfoScreenDir, ref Gf.InfoScreenGroups, ref Gf.InfoScreenFolderTotal);
            InfoScreenFolder.Items.Clear();
            for (int i = 0; i < Gf.InfoScreenFolderTotal; i++)
            {
                InfoScreenFolder.Items.Add(Gf.InfoScreenGroups[i, 0]);
            }
            if (InfoScreenFolder.Items.Count > 0)
            {
                InfoScreenFolder.SelectedIndex = 0;
            }
        }

        private void BuildPowerpointFolderList()
        {
            Gf.PowerpointGroups[0, 0] = "Powerpoint Items";
            Gf.PowerpointGroups[0, 1] = Gf.PowerpointDir;
            Gf.PowerpointFolderTotal = 1;
            Gf.BuildSubFolderList(Gf.PowerpointDir, Gf.PowerpointDir, ref Gf.PowerpointGroups, ref Gf.PowerpointFolderTotal);
            PowerpointFolder.Items.Clear();
            for (int i = 0; i < Gf.PowerpointFolderTotal; i++)
            {
                PowerpointFolder.Items.Add(Gf.PowerpointGroups[i, 0]);
            }
            if (PowerpointFolder.Items.Count > 0)
            {
                PowerpointFolder.SelectedIndex = 0;
            }
        }

        private void BuildMediaFolderList()
        {
            Gf.MediaGroups[0, 0] = "Media Files";
            Gf.MediaGroups[0, 1] = Gf.MediaDir;
            Gf.MediaFolderTotal = 1;
            Gf.BuildSubFolderList(Gf.MediaDir, Gf.MediaDir, ref Gf.MediaGroups, ref Gf.MediaFolderTotal);
            MediaFolder.Items.Clear();
            for (int i = 0; i < Gf.MediaFolderTotal; i++)
            {
                MediaFolder.Items.Add(Gf.MediaGroups[i, 0]);
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
                bool findItemMediaOnly = Gf.FindItemMediaOnly;
                Gf.FindItemMediaOnly = false;
                bool findItemInContents = Gf.FindItemInContents;
                Gf.FindItemInContents = false;
                SongsList.Items.Clear();
                SetSongListColWidth();
                if (SongFolder.Items.Count >= 1)
                {
                    int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
                    CurStyle = Gf.FolderGroupStyle[folderNumber];
                    Cursor = Cursors.WaitCursor;
                    SetSortButton(CurStyle);
                    SongsList.RightToLeft = ((Gf.ShowFontRTL[folderNumber, 0] > 0) ? RightToLeft.Yes : RightToLeft.No);
                    SongsList.RightToLeftLayout = ((Gf.ShowFontRTL[folderNumber, 0] > 0) ? true : false);
                    FillList(folderNumber, "", Gf.FindItemMediaOnly);
                    Gf.FindItemMediaOnly = findItemMediaOnly;
                    Gf.FindItemInContents = findItemInContents;
                    ShowStatusBarSummary();
                    Cursor = Cursors.Default;
                }
            }
        }

        private void SetItemFontSettings(ref SongSettings InItem)
        {
            Gf.FormatText(ref InItem, Gf.PanelBackColour, Gf.PanelBackColourTransparent, Gf.PanelTextColour, Gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
        }

        private void ApplyWorshipMode()
        {
            Gf.EasiSlidesMode = UsageMode.Worship;
            ModeEnableItems(EnableWorshipMode: true);
            LoadWorshipList(1);
            WorshipListIndexChanged();
            ShowStatusBarSummary();
            DisplayLyrics(Gf.PreviewItem, 0);
        }

        private void ApplyPraisebookMode()
        {
            Cursor = Cursors.WaitCursor;
            Gf.EasiSlidesMode = UsageMode.PraiseBook;
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
                string text = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                Gf.PreviewItem.CurItemNo = selectedIndex + 1;
                Gf.PreviewItem.TotalItems = PraiseBook.Items.Count;
                Gf.PreviewItem.Source = ItemSource.PraiseBook;
                LoadItem(ref Gf.PreviewItem, text);
            }
            else
            {
                Gf.PreviewItem.Type = "";
                Gf.PreviewItem.ItemID = "";
                ClearLyrics(ref flowLayoutPreviewLyrics);
            }
        }

        public void Load32PraiseBook(int DataType)
        {
            if (!(DataUtil.Trim(PraiseBook.Text) == ""))
            {
                ListViewItem listViewItem = new ListViewItem();
                string inFileName = Gf.PraiseBookDir + Gf.CurPraiseBook + ".esp";
                gfFileHelpers.LoadFileContents(inFileName, ref InContents);
                int num = Gf.Load32HeaderData(inFileName, InContents, ref Gf.HeaderData);
                if (num < 1)
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
                InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
                int num2 = 0;
                int num3 = InContents.IndexOf(">");
                try
                {
                    DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

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
                            Gf.WorshipListIDOK = true;
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
                        //string fullSearchString = (!Gf.WorshipListIDOK) ? ("select * from SONG where lower(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + Gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
                        string fullSearchString = (!Gf.WorshipListIDOK) ? ($"select * from SONG where lower(Title_1) like \"{DisplayName1.ToLower()}\"  AND FolderNo = {Gf.GetFolderNumber(FolderName)}") : ($"select * from SONG where songid = {FNum_ID} AND FolderNo > 0 ");

                        DataRow dr = DbController.GetDataRowScalar(connection, fullSearchString);
                        if (dr != null)
                        {
                            if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && Gf.FolderUse[DataUtil.GetDataInt(dr, "FolderNo")] > 0)
                            {
                                DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                                FolderName = Gf.FolderName[DataUtil.GetDataInt(dr, "FolderNo")];
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
                    Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
                }
                listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(DisplayName1, Gf.PB_CJKGroupStyle, ref FirstCharSym));
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
            if (!Directory.Exists(Gf.WorshipDir))
            {
                FileUtil.MakeDir(Gf.WorshipDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(Gf.WorshipDir);
            FileInfo[] files = directoryInfo.GetFiles("*.esw");
            foreach (FileInfo fileInfo in files)
            {
                string InFileName = fileInfo.Name;
                InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                if (InFileName != "")
                {
                    SessionList.Items.Add(InFileName);
                    if (Gf.CurSession == InFileName)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
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
                FileUtil.CreateNewFile(Gf.WorshipDir + Gf.CurSession + ".esw");
                SessionList.Items.Add(Gf.CurSession);
                SessionList.SelectedIndex = 0;
            }
            SessionList_Change();
        }

        private void PopulatePraiseBooksList()
        {
            bool flag = false;
            PraiseBook.Items.Clear();
            if (!Directory.Exists(Gf.PraiseBookDir))
            {
                FileUtil.MakeDir(Gf.PraiseBookDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(Gf.PraiseBookDir);
            FileInfo[] files = directoryInfo.GetFiles("*.esp");
            foreach (FileInfo fileInfo in files)
            {
                string InFileName = fileInfo.Name;
                InFileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                if (InFileName != "")
                {
                    PraiseBook.Items.Add(InFileName);
                    if (Gf.CurPraiseBook == InFileName)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
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
                FileUtil.CreateNewFile(Gf.PraiseBookDir + Gf.CurPraiseBook + ".esp");
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
                num3 = Gf.GetFolderNumber(SongFolder.SelectedText);
            }
            else
            {
                num3 = 0;
            }
            SongFolder.Items.Clear();
            for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
            {
                if (Gf.FolderUse[i] > 0)
                {
                    SongFolder.Items.Add(Gf.FolderName[i]);
                    if (Gf.GetFolderNumber(Gf.FolderName[i]) == num3)
                    {
                        num = i;
                        text = Gf.FolderName[i];
                    }
                    if (num2 == 0)
                    {
                        num2 = i;
                        text2 = Gf.FolderName[i];
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
            string text8 = "";
            string text9 = "";
            int num2 = 0;
            string text10 = "%";

            Gf.TotalMusicFiles = -1;
            string text11 = "";
            if (FNumber == 0)
            {
                for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
                {
                    if (Gf.FindSongsFolder[i])
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
            //string str = (FNumber < 0) ? Gf.Find_SQLString : ("select * from SONG where (lower(Title_1) like \"" + text10.ToLower() + "\" " + text11 + ") or (lower(Title_2) like \"" + text10.ToLower() + "\" " + text11 + ")");
            string str = (FNumber < 0) ? Gf.Find_SQLString : ($"select * from SONG where (lower(Title_1) like \"{text10.ToLower()}\" {text11}) or (lower(Title_2) like \"{text10.ToLower()}\" {text11})");

            string str2 = Gf.UseSongNumbers ? " order by song_number, cjk_strokecount" : ((CurStyle != SortBy.WordCount) ? " order by cjk_strokecount" : " order by cjk_wordcount, cjk_strokecount");
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

                (connection, dataReader) = DbController.GetDataReader(Gf.ConnectStringMainDB, sMultiQuery);

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
                                bool flag2 = Gf.MusicFound(DataUtil.ObjToString(dataReader["Title_1"]), musicTitle);
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
            BackgroundCurImagePath = Gf.PictureGroups[ImagesFolder.SelectedIndex, 1];
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
            ExternalPPCurImagePath = Gf.PowerpointGroups[PowerpointFolder.SelectedIndex, 1];
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
                if (Gf.PowerpointListingStyle == 0)
                {
                    text3 = listBox.Items[i].ToString();
                    listViewItem = PowerpointList.Items.Add(Gf.GetDisplayNameOnly(ref text3, UpdateByRef: false, KeepExt: false));
                    listViewItem.SubItems.Add("P" + listBox.Items[i]);
                }
                else
                {
                    ExternalPPImagename[i] = listBox.Items[i].ToString();
                }
            }
            SetPowerpointListColWidth();
            if (Gf.PowerpointListingStyle == 1)
            {
                Gf.ExtPPrefix_Num++;
                if (!Directory.Exists(Gf.ExtPPrefix + Gf.ExtPPrefix_Num + "\\"))
                {
                    FileUtil.MakeDir(Gf.ExtPPrefix + Gf.ExtPPrefix_Num + "\\");
                }
                Gf.ExternalPPT.BuildFirstScreenDump(ExternalPPImagename, listBox.Items.Count, Gf.ExtPPrefix + Gf.ExtPPrefix_Num + "\\");
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
                    //?? ???? ?? ???????? ?? ???? ??
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
                if ((BookLookup.SelectedIndex == 66) & (Gf.HB_SQLString == ""))
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
                    for (int i = 1; i <= Gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if ((Gf.HB_VersesLocation[i, 1] == num) & (Gf.HB_VersesLocation[i, 2] == num2) & (Gf.HB_VersesLocation[i, 3] == num3))
                        {
                            BibleText.SelectionStart = Gf.HB_VersesLocation[i, 4];
                            for (int j = i; j <= Gf.HB_VersesLocation[0, 0]; j++)
                            {
                                if ((Gf.HB_VersesLocation[j, 1] == num6) & (Gf.HB_VersesLocation[j, 2] == num4) & (Gf.HB_VersesLocation[j, 3] == num5))
                                {
                                    BibleText.SelectionLength = Gf.HB_VersesLocation[j, 4] - Gf.HB_VersesLocation[i, 4] + Gf.HB_VersesLocation[j, 5] - 2;
                                    BibleText.ScrollToCaret();
                                    j = Gf.HB_VersesLocation[0, 0];
                                }
                            }
                            i = Gf.HB_VersesLocation[0, 0];
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
            if (Gf.HB_TotalVersions >= 1)
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
                string displayNameOnly = Gf.GetDisplayNameOnly(ref Gf.HB_Versions[InBibleVersion, 4], UpdateByRef: false, KeepExt: true);
                int num = 0;
                int num2 = 0;
                string text = "";
                int num3 = BibleText.SelectionStart + 2;
                int num4 = num3 + BibleText.SelectionLength;
                if (num3 >= 0)
                {
                    for (int i = 1; i <= Gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if (!((num3 >= Gf.HB_VersesLocation[i, 4]) & (num3 <= Gf.HB_VersesLocation[i, 4] + Gf.HB_VersesLocation[i, 5])))
                        {
                            continue;
                        }
                        num = i;
                        for (int j = i; j <= Gf.HB_VersesLocation[0, 0]; j++)
                        {
                            if ((num4 >= Gf.HB_VersesLocation[j, 4]) & (num4 <= Gf.HB_VersesLocation[j, 4] + Gf.HB_VersesLocation[j, 5]))
                            {
                                num2 = j;
                                j = 3002;
                            }
                        }
                        i = 3002;
                    }
                }
                int num5 = num2 - num + 1;
                if (Gf.HB_SequentialListing)
                {
                    if (num5 > Gf.HB_MaxVersesSelection)
                    {
                        num2 = num + Gf.HB_MaxVersesSelection - 1;
                    }
                }
                else if (num5 > Gf.HB_MaxAdhocVersesSelection)
                {
                    num2 = num + Gf.HB_MaxAdhocVersesSelection - 1;
                }
                if (num >= 0)
                {
                    BibleText.SelectionStart = Gf.HB_VersesLocation[num, 4];
                    if (num2 < 0)
                    {
                        num2 = num;
                    }
                    BibleText.SelectionLength = Gf.HB_VersesLocation[num2, 4] - Gf.HB_VersesLocation[num, 4] + Gf.HB_VersesLocation[num2, 5] - 2;
                }
                BibleText.ScrollToCaret();
                if (Gf.HB_SequentialListing)
                {
                    string text2 = Convert.ToString(Gf.HB_VersesLocation[num, 2]) + ":" + Convert.ToString(Gf.HB_VersesLocation[num, 3]);
                    string text3 = " - " + Convert.ToString(Gf.HB_VersesLocation[num2, 2]) + ":" + Convert.ToString(Gf.HB_VersesLocation[num2, 3]);
                    if (Gf.HB_VersesLocation[num, 2] == Gf.HB_VersesLocation[num2, 2])
                    {
                        text3 = ((Gf.HB_VersesLocation[num, 3] != Gf.HB_VersesLocation[num2, 3]) ? ("-" + Convert.ToString(Gf.HB_VersesLocation[num2, 3])) : "");
                    }
                    OutputTitle = Gf.LookUpBookName(InBibleVersion, Gf.HB_VersesLocation[num, 1]) + " " + text2 + text3 + " (" + Gf.HB_Versions[InBibleVersion, 1] + ")";
                    text = "0" + ';' + displayNameOnly + ';' + ';';
                    int i;
                    for (i = num; i <= num2; i++)
                    {
                        int num6;
                        for (num6 = i; Gf.HB_VersesLocation[i, 2] == Gf.HB_VersesLocation[num6, 2] && i <= num2; i++)
                        {
                        }
                        int j = i - 1;
                        object obj = text;
                        text = string.Concat(obj, Convert.ToString(Gf.HB_VersesLocation[num6, 1]), ';', Convert.ToString(Gf.HB_VersesLocation[num6, 2]), ';', Convert.ToString(Gf.HB_VersesLocation[num6, 3]), ';', Convert.ToString(Gf.HB_VersesLocation[j, 2]), ';', Convert.ToString(Gf.HB_VersesLocation[j, 3]), ';');
                        i = j;
                    }
                }
                else
                {
                    text = "1" + ';' + displayNameOnly + ';' + ';';
                    for (int i = num; i <= num2; i++)
                    {
                        string text4 = OutputTitle;
                        OutputTitle = text4 + DataUtil.Trim(DataUtil.Left(Gf.LookUpBookName(InBibleVersion, Gf.HB_VersesLocation[i, 1]), 4)) + " " + Convert.ToString(Gf.HB_VersesLocation[i, 2]) + ":" + Convert.ToString(Gf.HB_VersesLocation[i, 3]) + ",";
                        object obj = text;
                        text = string.Concat(obj, Convert.ToString(Gf.HB_VersesLocation[i, 1]), ';', Convert.ToString(Gf.HB_VersesLocation[i, 2]), ';', Convert.ToString(Gf.HB_VersesLocation[i, 3]), ';', Convert.ToString(Gf.HB_VersesLocation[i, 2]), ';', Convert.ToString(Gf.HB_VersesLocation[i, 3]), ';');
                    }
                    OutputTitle = DataUtil.Left(OutputTitle, OutputTitle.Length - 1);
                    if (OutputTitle.Length > 60)
                    {
                        OutputTitle = DataUtil.Left(OutputTitle, 60) + " .. ";
                    }
                    OutputTitle = OutputTitle + " (" + Gf.HB_Versions[InBibleVersion, 1] + ")";
                }
                return text;
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

        private void WriteCurSession()
        {
            RegUtil.SaveRegValue("config", "current_session", Gf.CurSession);
        }

        private void WriteCurPraiseBook()
        {
            RegUtil.SaveRegValue("config", "current_praisebook", Gf.CurPraiseBook);
        }

        private void LoadWorshipList(int DataType)
        {
            string inFileName = Gf.WorshipDir + Gf.CurSession + ".esw";
            LoadWorshipList(DataType, inFileName);
        }

        private string LoadWorshipList(int DataType, string InFileName)
        {
            Gf.StartPresAt = 0;
            int itemCountInFileBefore = -1;
            string lastWriteTime = "N/A";
            try
            {
                if (System.IO.File.Exists(InFileName))
                {
                    string fileContent = System.IO.File.ReadAllText(InFileName);
                    itemCountInFileBefore = System.Text.RegularExpressions.Regex.Matches(fileContent, "<Item>").Count;
                    lastWriteTime = System.IO.File.GetLastWriteTime(InFileName).ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
            }
            catch { }
            WriteDebugLog($"LoadWorshipList() called - DataType: {DataType}, File: {InFileName}, Items in file: {itemCountInFileBefore}, File last modified: {lastWriteTime}");
            string result = LoadIndexFile(DataType, InFileName, ref WorshipListItems, UsageMode.Worship, ref Gf.CurSessionNotes);
            WriteDebugLog($"LoadWorshipList() completed - Items loaded in ListView: {WorshipListItems.Items.Count}");
            PreviewNotes.Text = Gf.CurSessionNotes;
            return result;
        }

        private void LoadPraiseBook(int DataType)
        {
            string inFileName = Gf.PraiseBookDir + Gf.CurPraiseBook + ".esp";
            LoadIndexFile(DataType, inFileName, ref PraiseBookItems, UsageMode.PraiseBook, ref Gf.CurPraiseBookNotes);
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
                                Gf.WorshipListIDOK = false;
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "SystemID"))
                                {
                                    if (Gf.SystemID == xmlTextReader.ReadElementContentAsString())
                                    {
                                        Gf.WorshipListIDOK = true;
                                    }
                                    xmlTextReader.Read();
                                    if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "FormatData"))
                                    {
                                        text = xmlTextReader.ReadElementContentAsString();
                                        if (DataType == 2)
                                        {
                                            xmlTextReader.Close();
                                            return text;
                                        }
                                        Gf.LoadHeaderData(text, ref Gf.HeaderData, '>');
                                        if (DataType == 1)
                                        {
                                            Gf.ApplyHeaderData();
                                            if (InMode == UsageMode.Worship)
                                            {
                                                UpdateDefaultFields();
                                                UpdateDisplayPanelFields();
                                            }
                                            xmlTextReader.Close();
                                            return text;
                                        }
                                        Gf.ApplyHeaderData();
                                        InList.Items.Clear();
                                        InNotes = "";
                                        xmlTextReader.Read();
                                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "Notes"))
                                        {
                                            InNotes = xmlTextReader.ReadElementContentAsString();
                                        }
                                        DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
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
                catch
                {
                    xmlTextReader?.Close();
                }
            }
            catch
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
            Gf.TotalMusicFiles = -1;
            string inFileName = Gf.WorshipDir + Gf.CurSession + ".esw";
            gfFileHelpers.LoadFileContents(inFileName, ref InContents);
            int num = Gf.Load32HeaderData(inFileName, InContents, ref Gf.HeaderData);
            if (num < 1)
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
            InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
            int num2 = 0;
            int num3 = InContents.IndexOf(">");
            try
            {

                DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
                while (num3 >= 0)
                {
                    string text = DataUtil.Trim(DataUtil.Mid(InContents, num2, num3 - num2));
                    int num4 = 0;
                    Gf.SongFormatData = "";
                    num4 = text.IndexOf('*');
                    if (num4 >= 0)
                    {
                        Gf.SongFormatData = DataUtil.Trim(DataUtil.Right(text, text.Length - num4 - 1));
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
                        Gf.WorshipListIDOK = true;
                        DataUtil.Convertv32FormatString(ref Gf.SongFormatData, '*');

                        WriteItemtoWorshipList(connection, inSym, fNum_ID, displayName, "", Gf.SongFormatData, -1);


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
                if ((WorshipListItems.Items.Count > 0) & (Gf.PreviewItem.Source == ItemSource.WorshipList))
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
                                Gf.WorshipListIDOK = false;
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "SystemID"))
                                {
                                    if (Gf.SystemID == xmlTextReader.ReadElementContentAsString())
                                    {
                                        Gf.WorshipListIDOK = true;
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
                                        DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

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
            string musicTitle2 = Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: false);
            string text = "";
            string text2 = "";
            string text3 = "0";
            if (InSym == "D")
            {
                bool flag = false;
                try
                {
                    //SQLite LCase()  -> lower() ????UCase() -> upper()
                    //string fullSearchString = (!Gf.WorshipListIDOK) ? ("select * from SONG where LCase(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + Gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
                    string fullSearchString = (!Gf.WorshipListIDOK) ? ("select * from SONG where lower(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + Gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");

                    DataRow dr = DbController.GetDataRowScalar(connection, fullSearchString);
                    if (dr != null)
                    {
                        if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && Gf.FolderUse[DataUtil.GetDataInt(dr, "FolderNo")] > 0)
                        {
                            DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                            musicTitle = DataUtil.GetDataString(dr, "Title_2");
                            text = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
                            text2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
                            FolderName = Gf.FolderName[DataUtil.GetDataInt(dr, "FolderNo")];
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
                Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "B")
            {
                FNum_ID = "B" + FNum_ID;
            }
            else if (InSym == "T")
            {
                FNum_ID = "T" + DisplayName1;
                Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "I")
            {
                FNum_ID = "I" + DisplayName1;
                string InTitle = "";
                Gf.LoadIndividualData(ref Gf.TempItem1, FNum_ID, "", 1, ref InTitle);
                musicTitle2 = Gf.TempItem1.Title;
                musicTitle = Gf.TempItem1.Title2;
                Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "W")
            {
                FNum_ID = "W" + DisplayName1;
                Gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "M")
            {
                FNum_ID = "M" + DisplayName1;
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
            Gf.SetDefaultBackScreen(ref PreviewScreen);
            Gf.SetDefaultBackScreen(ref OutputScreen);
            if (Gf.ShowRunning)
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
            Gf.PreviewItem.Source = ItemSource.WorshipList;
            Gf.TotalWorshipListItems = WorshipListItems.Items.Count;
            int num;
            if (GetFirstItem & (Gf.TotalWorshipListItems > 0))
            {
                WorshipListItems.Items[0].Selected = true;
                num = 0;
            }
            else
            {
                num = Gf.GetSelectedIndex(WorshipListItems);
            }

            if (num >= 0)
            {
                //?? ???????? ü? ????
                string InTitle = WorshipListItems.Items[num].SubItems[0].Text;
                string text = WorshipListItems.Items[num].SubItems[1].Text;
                Gf.PreviewItem.InMainItemText = InTitle;
                Gf.PreviewItem.InSubItemItem1Text = text;
                Gf.PreviewItem.CurItemNo = num + 1;
                Gf.PreviewItem.TotalItems = WorshipListItems.Items.Count;

                string filePrefix = Gf.SetPowerpointPreviewPrefix(Gf.PreviewItem, useTitlePrefix: true);

                // ✅ 캐싱 최적화: 파일이 빌드되지 않았거나 다른 항목으로 변경된 경우에만 로드
                bool hasPptPathFromList = !string.IsNullOrEmpty(text)
                    && text[0] == 'P'
                    && text.Length > 1
                    && !string.IsNullOrWhiteSpace(text.Substring(1));
                if (!hasPptPathFromList
                    || !Gf.PreviewPPT.IsBuildedFileCheck(Gf.PreviewItem.Path, filePrefix, ref Gf.PreviewItem.TotalSlides)
                    || preSelectedItemNum != num)
                {
                    LoadItem(ref Gf.PreviewItem, text, WorshipListItems.Items[num].SubItems[2].Text, StartingSlide, ref InTitle, ScrollToCaret: true);
                    UpdateDisplayPanelFields();
                }

                preSelectedItemNum = num;
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
            // Diagnostic: Log save attempt to file
            WriteDebugLog($"SaveWorshipList() called - Items count: {WorshipListItems.Items.Count}, UpdatingFormatFields: {UpdatingFormatFields}");

            if (!UpdatingFormatFields)
            {
                Gf.CurSession = SessionList.Text;
                for (int i = 1; i <= WorshipListItems.Items.Count; i++)
                {
                    Gf.WorshipSongs[i, 2] = Gf.RemoveMusicSym(DataUtil.Trim(WorshipListItems.Items[i - 1].Text));
                    Gf.WorshipSongs[i, 0] = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[1].Text);
                    Gf.WorshipSongs[i, 1] = DataUtil.Left(Gf.WorshipSongs[i, 0], 1);
                    Gf.WorshipSongs[i, 4] = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[2].Text);
                }
                Gf.TotalWorshipListItems = WorshipListItems.Items.Count;

                string filePath = Gf.WorshipDir + Gf.CurSession + ".esw";

                // Get file size BEFORE save
                long fileSizeBefore = -1;
                try { if (System.IO.File.Exists(filePath)) fileSizeBefore = new System.IO.FileInfo(filePath).Length; } catch { }
                WriteDebugLog($"Saving to file: {filePath} (size before: {fileSizeBefore} bytes)");

                bool saveResult = gfFileHelpers.SaveIndexFile(filePath, ref WorshipListItems, UsageMode.Worship, SaveAllItems: true, "", Gf.CurSessionNotes);

                // Get file size AFTER save and count items in file
                long fileSizeAfter = -1;
                int itemCountInFile = -1;
                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        fileSizeAfter = new System.IO.FileInfo(filePath).Length;
                        string fileContent = System.IO.File.ReadAllText(filePath);
                        itemCountInFile = System.Text.RegularExpressions.Regex.Matches(fileContent, "<Item>").Count;
                    }
                }
                catch { }

                string lastWriteAfter = "N/A";
                try { if (System.IO.File.Exists(filePath)) lastWriteAfter = System.IO.File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss.fff"); } catch { }
                WriteDebugLog($"SaveIndexFile returned: {saveResult}, file size after: {fileSizeAfter} bytes, items in file: {itemCountInFile}, expected: {WorshipListItems.Items.Count}, file modified: {lastWriteAfter}");

                if (!saveResult)
                {
                    WriteDebugLog($"ERROR: SaveIndexFile returned FALSE - save FAILED!");
                }
                if (itemCountInFile != WorshipListItems.Items.Count)
                {
                    WriteDebugLog($"ERROR: Item count mismatch! File has {itemCountInFile} items but expected {WorshipListItems.Items.Count}");
                }

                if (PreloadPowerpoint)
                {
                    gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
                }
            }
            else
            {
                // Diagnostic: Log when save is skipped due to UpdatingFormatFields flag
                WriteDebugLog("WARNING: SaveWorshipList() skipped - UpdatingFormatFields is true");
            }
        }

        private const long MaxLogFileSize = 512 * 1024; // 512KB

        private void WriteDebugLog(string message)
        {
            try
            {
                string logFile = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "EasiSlides_Debug.log"
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
            int num = Gf.StartPresAt;
            if (WorshipListItems.Items.Count > 0)
            {
                int num2 = -1;
                int num3 = -1;
                if (!InItem.OutputStyleScreen)
                {
                    num = Gf.GetSelectedIndex(WorshipListItems) + 1;
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
                prevTitle = ((num2 >= 0) ? Gf.RemoveMusicSym(WorshipListItems.Items[num2].SubItems[0].Text) : "");
                nextTitle = ((num3 >= 0) ? Gf.RemoveMusicSym(WorshipListItems.Items[num3].SubItems[0].Text) : "");
            }
            string text = DataUtil.Left(InIDString, 1);
            Gf.InitialiseIndividualData(ref InItem, GapMedia.None, text);
            InItem.PrevTitle = prevTitle;
            InItem.NextTitle = nextTitle;
            MakePowerpointPreviewVisible(InItem, (text == "P") ? true : false);
            if (text == "P")
            {
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
                    ShowOutputPPThumbs(InItem.CurSlide);
                }
                else
                {
                    ShowPreviewPPThumbs(InItem.CurSlide);
                }
                InItem.Format.ShowItemTransition = 0;
                InItem.Format.ShowSlideTransition = 0;
                if (Gf.ShowRunning & InItem.OutputStyleScreen)
                {
                    InItem.TotalSlides = Gf.RunPowerpointSong(ref InItem, ref MainPPT, StartingSlide, ShowResult: true);
                    if (Gf.DualMonitorMode)
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
                    Gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
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
                        AllowIndividualFormat(AllowFormat: true, (!(InFormatString == "")) ? true : false);
                        UpdateFormatFields();
                        BuildVerseButtons(InItem);
                        DisplayLyrics(InItem, StartingSlide, ScrollToCaret);
                    }
                }
                else
                {
                    Gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
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
            text = ((InItem.Format.MediaOption == 1 || InItem.Format.MediaOption == 2) ? Gf.GetMediaLocation(InItem) : "");
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
                Gf.OutputPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

                // ✅ 중복 호출 방지: 같은 ItemID면 캐시 확인
                if (OutputItem != InItem.ItemID)
                {
                    OutputItem = InItem.ItemID;
                }
                else
                {
                    if (Gf.OutputPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
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
                Gf.PreviewPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

                // ✅ 중복 호출 방지: 같은 ItemID면 캐시 확인
                if (previwItem != InItem.ItemID)
                {
                    previwItem = InItem.ItemID;
                }
                else
                {
                    if (Gf.PreviewPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
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
                bool outBuilt = Gf.OutputPPT.BuildScreenOutDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, 9, 1000, ref InItem.SongVerses, ref InItem.Slide, Gf.SequenceSymbol);
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
                bool preBuilt = Gf.PreviewPPT.BuildScreenDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, 9, 1000, ref InItem.SongVerses, ref InItem.Slide, Gf.SequenceSymbol);
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
                if (Gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = clampedSlide;
                    ShowSlide(ref InItem, TransitionAction);
                }
                else
                {
                    Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
                }
            }
            else if (InItem.Type == "B" || InItem.Type == "T" || InItem.Type == "I" || InItem.Type == "W" || InItem.Type == "G")
            {
                if (Gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = clampedSlide;
                    ShowSlide(ref InItem, TransitionAction);
                }
            }
            else if (InItem.Type == "M" || InItem.Type == "G")
            {
                if (Gf.EasiSlidesMode == UsageMode.Worship)
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
                string text = SongsList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = SongsList.Items[selectedIndex].SubItems[0].Text;
                Gf.PreviewItem.InMainItemText = InTitle;
                Gf.PreviewItem.InSubItemItem1Text = text;
                Gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref Gf.PreviewItem, text, "", StartingSlide, ref InTitle, ScrollToCaret);
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
            if (!((Gf.GetItemTitle(InItemID) == "") | (Gf.MainEditHistoryList[1, 0] == InItemID)))
            {
                if (Gf.TotalMainEditHistory < Gf.MaxUserEditHistory)
                {
                    Gf.TotalMainEditHistory++;
                }
                else
                {
                    Gf.TotalMainEditHistory = Gf.MaxUserEditHistory;
                }
                for (int num = Gf.TotalMainEditHistory; num >= 2; num--)
                {
                    Gf.MainEditHistoryList[num, 0] = Gf.MainEditHistoryList[num - 1, 0];
                }
                Gf.MainEditHistoryList[1, 0] = InItemID;
                Gf.RemoveDuplicateEditorHistoryItems(ref Gf.MainEditHistoryList, ref Gf.TotalMainEditHistory);
                UpdateMenuEditHistory();
                Gf.SaveMainEditHistoryToRegistry();
            }
        }

        private void UpdateMenuEditHistory()
        {
            try
            {
                int num = 0;
                string text = "";
                string text2 = "";
                for (int i = 1; i <= Gf.TotalMainEditHistory; i++)
                {
                    text2 = Gf.GetItemTitle(Gf.MainEditHistoryList[i, 0]);
                    if (text2 != "" && Gf.MainEditHistoryList[num, 0] != Gf.MainEditHistoryList[i, 0])
                    {
                        num++;
                        Gf.MainEditHistoryList[num, 0] = Gf.MainEditHistoryList[i, 0];
                        Gf.MainEditHistoryList[num, 1] = text2;
                    }
                }
                Gf.TotalMainEditHistory = num;
                for (int i = Gf.TotalMainEditHistory + 1; i <= Gf.AbsoluteMaxHitoryItems; i++)
                {
                    Gf.MainEditHistoryList[i, 0] = "";
                    Gf.MainEditHistoryList[i, 1] = "";
                }
                for (int i = 1; i < Gf.AbsoluteMaxHitoryItems; i++)
                {
                    Menu_EditHistoryList.DropDownItems[i - 1].Text = i + " " + Gf.MainEditHistoryList[i, 1];
                    if (i > Gf.TotalMainEditHistory)
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Visible = false;
                    }
                    else
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Visible = true;
                        text = DataUtil.Left(Gf.MainEditHistoryList[i, 0], 1);
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
            if (directoryName == Gf.RootEasiSlidesDir + "Images\\Scenery")
            {
                backgroundImageMode = 2;
            }
            else if (directoryName == Gf.RootEasiSlidesDir + "Images\\Tiles")
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
                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
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
            else if ((InMode == 1) | (InMode == 2))
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
                            if ((DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "D") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "B") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "T") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "M") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "W"))
                            {
                                Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                                {
                                    Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                                }
                                Gf.PreviewItem.Format.FormatString = GetNewFormatString();
                                WorshipListItems.Items[i].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
                            }
                            else if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "I")
                            {
                                Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
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
                        }
                        SaveWorshipList();
                        WorshipListIndexChanged(Gf.PreviewItem.CurSlide);
                    }
                    else if (InMode == 2)
                    {
                        Gf.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
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
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, GetNewFormatString());
                        SongsListIndexChanged(Gf.PreviewItem.CurSlide);
                    }
                }
                else if (Gf.PreviewItem.Source == ItemSource.HolyBible)
                {
                    if (Gf.PreviewItem.CompleteLyrics != "")
                    {
                        Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            Gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        Gf.PreviewItem.UseDefaultFormat = false;
                        Gf.PreviewItem.Format.FormatString = GetNewFormatString();
                        HB_CurSelectedFormat = Gf.PreviewItem.Format.FormatString;
                        Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen);
                        Gf.DrawText(ref Gf.PreviewItem, ref PreviewScreen, Gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                        AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                        UpdateFormatFields();
                    }
                }
                else if (Gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen && Gf.PreviewItem.CompleteLyrics != "")
                {
                    Gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                    if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
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

        private void UpdateWorshipShowIcons()
        {
            if (ResetWorshipShowIcons())
            {
                if (Gf.OutputItem.CurItemNo > 0)
                {
                    try
                    {
                        if (WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 0 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 2 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 4 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 6 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 8 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 28 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 10)
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
                else if ((Gf.StartPresAt > 0) & (Gf.StartPresAt <= WorshipListItems.Items.Count))
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
                    Gf.WorshipSongs[0, 2] = Gf.OutputItem.InMainItemText;
                    Gf.WorshipSongs[0, 0] = Gf.OutputItem.InSubItemItem1Text;
                    Gf.WorshipSongs[0, 1] = DataUtil.Left(Gf.WorshipSongs[0, 0], 1);
                    Gf.WorshipSongs[0, 4] = Gf.PreviewItem.Format.FormatString;
                    Gf.AdHocItemPresent = true;
                }
                if (Gf.PreviewItem.Type == "P" && Gf.PreviewItem.Source != ItemSource.WorshipList)
                {
                    gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
                }
            }
            LoadThumbOutlockkey = 0;
            previousOutSelectedSlide = 1;
            LoadItem(ref Gf.OutputItem, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString, Gf.PreviewItem.CurSlide, ref Gf.PreviewItem.Title, ScrollToCaret: true);
            UpdateWorshipShowIcons();
            if (Gf.ShowRunning)
            {
                Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStoredItem;
                RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
            }
            FocusOutputArea();
        }

        private void PreViewToOutput()
        {
            if (Gf.PreviewItem.ItemID != "")
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
