//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.SQLite;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmEditItem : Form
	{
		private enum ControlsBtn
		{
			PlayPausebtn,
			Stopbtn,
			FFbtn,
			FRbtn,
			Closebtn
		}

		private const int MaxRotateContainers = 1024;

		private bool InitLoad = true;

		private PopupWindowHelper popupHelper = null;

		private string PopupBtnPressed = "";

		private string wArray = "";

		private int CurSongID;

		private int[] VerseListIndex = new int[160];

		private bool VerseSymbolChanged;

		private bool[] prevVersePresent = new bool[160];

		private bool[] VersePresent = new bool[160];

		private int[] VersePresentNewScreenCount = new int[160];

//		private bool InsertAction;

//		private int LeftMargin;

//		private byte[] VerseArray;

		private string SavedWriterInfo;

		private string SavedCopyrightInfo;

		private string SavedSequence;

		private string SavedFolder;

		private string SavedUserReference;

		private string SavedBookReference;

		private string SavedSongNumber;

		private string SavedTitle2;

		private string SavedTitle;

		private string OrderListSequence;

//		private string SavedMusicNotations;

		private string SavedSongTiming;

		private string SavedSongKey;

		private string SavedCapo;

		private string SavedLicAdminInfo2;

		private string SavedLicAdminInfo1;

		private string SavedRotateString;

//		private int countc;

//		private int counta;

		private bool Title2IgnoreChange;

		private string InitSongTitle2;

		private int FormState;

//		private string IndicatorEnd = "";

		private string[] sArray;

//		private bool LoadingSong = false;

//		private int LastCurPos = 0;

		private System.Drawing.Font MainFont;

		private System.Drawing.Font NotationFont;

//		private bool CurFolderFound = false;

		private bool FormCanClose = false;

		private bool IgnoreChange = false;

		private bool InsertingPresetItem = false;

		private int StackArrayMaxPoint = 100;

		private bool SetWordWrap = true;

		private bool SetRightToLeft1 = true;

		private bool SetRightToLeft2 = true;

		private bool SetChordsMenu = true;

		private int[,] StackTrackPos = new int[3, 2];

		private int[] StackMaxRedo = new int[3];

		private int[] StackIndex = new int[3];

		private int[] StackStartPoint = new int[3];

		private string[,] sStack = new string[3, 1000];

		private int[,] iCursorPosition = new int[3, 1000];

		private string CombinedLyrics = "";

		private string CombinedNotations = "";

		private bool InitFontsLists = true;

//		private int PrevSplitterDistance = 0;

//		private bool TopPanelLocked = true;

		private RichTextBox tbWorkspace = new RichTextBox();

		private RichTextBox tbTempSpace = new RichTextBox();

		private string Lyrics1SavedNotations = "";

		private string Lyrics1SavedCopy = "";

		private string Lyrics1Only = "";

		private string Lyrics2SavedNotations = "";

		private string Lyrics2SavedCopy = "";

		private string Lyrics2Only = "";

		private string Reg_FormLeft = "EditDBLeft";

		private string Reg_FormTop = "EditDBTop";

		private string Reg_FormWidth = "EditDBWidth";

		private string Reg_FormHeight = "EditDBHeight";

		private string Reg_FormMax = "EditDBMax";

		private string Reg_FormWordWrap = "EditDB_WordWrap";

		private string Reg_FormSetChordsMenu = "EditDB_ChordsMenu";

		private string Reg_FormLyricsSplit = "EditDB_LyricsSplit";

		private string Reg_FormRegion2Tab = "EditDB_Region2Tab";

		private ListView tempSequenceCopied = new ListView();

		private ListView ListViewNotations = new ListView();

		private ListView ListViewNotationLog = new ListView();

		private ListView ScreenBreaks1 = new ListView();

		private ListView ScreenBreaks2 = new ListView();

		private bool ScreenBreak1Available = false;

		private bool ScreenBreak2Available = false;

		private string RotateString = "";

		private DateTime InitDateTime = new DateTime(2005, 1, 1, 0, 0, 0);

		private int RotateTotalScreensIndex = -1;

		private double TimeIncrement = 1.0;

		private DateTimePicker[] RotateTimePosition = new DateTimePicker[1024];

		private Label[] RotateTimeLabel = new Label[1024];

		private int AttemptConnectCount = 0;

		private int MaxAttemptConnectCount = 60;

		private double CurMediaPosition = 0.0;

		private double CurMediaLength = 0.0;

		private int[] VerseScreenCount = new int[160];

		private DShowLib DShowPlayer = new DShowLib();

		private bool PlayerOK = false;

		private int tbLyrics1MouseUpPos = 0;

		private int tbLyrics2MouseUpPos = 0;

		private bool SplitterReAdjust = false;

		public FrmEditItem()
		{
			InitializeComponent();
			popupHelper = new PopupWindowHelper();
			popupHelper.PopupClosed += popupHelper_PopupClosed;
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			popupHelper.AssignHandle(base.Handle);
		}

		private void Btn_Click(object sender, EventArgs e)
		{
			System.Drawing.Point p = new System.Drawing.Point(0, 0);
			Button button = (Button)sender;
			PopupBtnPressed = button.Name;
			switch (PopupBtnPressed)
			{
			case "Btn_Title":
				Gf.popUpText = SongTitle.Text;
				Gf.popUpTextMaxLength = SongTitle.MaxLength;
				p = new System.Drawing.Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + SongTitle.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + SongTitle.Top + SongTitle.Height);
				break;
			case "Btn_Title2":
				Gf.popUpText = SongTitle2.Text;
				Gf.popUpTextMaxLength = SongTitle2.MaxLength;
				p = new System.Drawing.Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + SongTitle2.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + SongTitle2.Top + SongTitle2.Height);
				break;
			case "Btn_Writer":
				Gf.popUpText = WriterInfo.Text;
				Gf.popUpTextMaxLength = WriterInfo.MaxLength;
				p = new System.Drawing.Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + WriterInfo.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + WriterInfo.Top + WriterInfo.Height);
				break;
			case "Btn_Copyright":
				Gf.popUpText = CopyrightInfo.Text;
				Gf.popUpTextMaxLength = CopyrightInfo.MaxLength;
				p = new System.Drawing.Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + CopyrightInfo.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + CopyrightInfo.Top + CopyrightInfo.Height);
				break;
			case "Btn_BookRef":
				Gf.popUpText = BookReference.Text;
				Gf.popUpTextMaxLength = BookReference.MaxLength;
				p = new System.Drawing.Point(splitContainerMain.Left + groupBox1.Left + panel8.Left + BookReference.Left, splitContainerMain.Top + groupBox1.Top + panel8.Top + BookReference.Top + BookReference.Height);
				break;
			case "Btn_UserRef":
				Gf.popUpText = UserReference.Text;
				Gf.popUpTextMaxLength = UserReference.MaxLength;
				p = new System.Drawing.Point(splitContainerMain.Left + groupBox1.Left + panel8.Left + UserReference.Left, splitContainerMain.Top + groupBox1.Top + panel8.Top + UserReference.Top + UserReference.Height);
				break;
			}
			FrmPopupText popup = new FrmPopupText();
			System.Drawing.Point location = PointToScreen(p);
			popupHelper.ShowPopup(this, popup, location);
		}

		private void popupHelper_PopupClosed(object sender, PopupClosedEventArgs e)
		{
			switch (PopupBtnPressed)
			{
			case "Btn_Title":
				SongTitle.Text = Gf.popUpText;
				break;
			case "Btn_Title2":
				SongTitle2.Text = Gf.popUpText;
				break;
			case "Btn_Writer":
				WriterInfo.Text = Gf.popUpText;
				break;
			case "Btn_Copyright":
				CopyrightInfo.Text = Gf.popUpText;
				break;
			case "Btn_BookRef":
				BookReference.Text = Gf.popUpText;
				break;
			case "Btn_UserRef":
				UserReference.Text = Gf.popUpText;
				break;
			}
		}

		private void FrmEdit_Load(object sender, EventArgs e)
		{
			int num = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormLeft, -1));
			int num2 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormTop, -1));
			int num3 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormWidth, 620));
			int num4 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormHeight, 450));
			FormState = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormMax, 0));
			int num5 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormLyricsSplit, 520));
			int num6 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormRegion2Tab, 0));
			if (num3 > Screen.PrimaryScreen.Bounds.Width)
			{
				num3 = Screen.PrimaryScreen.Bounds.Width - 20;
			}
			if (num4 > Screen.PrimaryScreen.Bounds.Height)
			{
				num4 = Screen.PrimaryScreen.Bounds.Height - 30;
			}
			if (num < 0)
			{
				num = (Screen.PrimaryScreen.Bounds.Width - num3) / 2;
			}
			if (num2 < 0)
			{
				num2 = (Screen.PrimaryScreen.Bounds.Height - num4) / 2;
			}
			if (num2 > 0)
			{
				num2 = num2 * 2 / 3;
			}
			if (num + num3 > Screen.PrimaryScreen.Bounds.Width)
			{
				num = (Screen.PrimaryScreen.Bounds.Width - num3) / 2;
			}
			if (num2 + num4 > Screen.PrimaryScreen.Bounds.Height)
			{
				num2 = (Screen.PrimaryScreen.Bounds.Height - num4) / 2;
			}
			base.Top = num2;
			base.Left = num;
			base.Width = num3;
			base.Height = num4;
			if (FormState > 0)
			{
				base.WindowState = FormWindowState.Maximized;
			}
			splitContainerMain.SplitterDistance = splitContainerMain.Panel1MinSize;
			num5 = ((num5 < 0 || num5 > 1000) ? 520 : num5);
			int num7 = splitContainer1.Width * num5 / 1000;
			if (num7 < splitContainer1.Panel1MinSize)
			{
				num7 = splitContainer1.Panel1MinSize;
			}
			splitContainer1.SplitterDistance = num7;
			num6 = ((num6 >= 0 && num6 <= 1) ? num6 : 0);
			tabRightPane.SelectedIndex = num6;
			BuildTempItems();
			sArray = Gf.SymbolsString.Split(',');
			InitMediaPlayer();
			BuildFolderList();
			ResetAll();
			SetWordWrap = ((RegUtil.GetRegValue("settings", Reg_FormWordWrap, 0) > 0) ? true : false);
			SetLyricsWordWrap(SetWordWrap);
			SetChordsMenu = ((RegUtil.GetRegValue("settings", Reg_FormSetChordsMenu, 0) > 0) ? true : false);
			SetMenuChordsMenu(SetChordsMenu);
			Gf.BuildFontsList(ref ComboFontName);
			Gf.BuildFontSizeList(ref ComboMainFontSize);
			Gf.BuildFontSizeList(ref ComboNotationFontSize);
			ComboFontName.Text = Gf.EditMainFontName;
			ComboMainFontSize.Text = Gf.EditMainFontSize.ToString();
			ComboNotationFontSize.Text = Gf.EditNotationFontSize.ToString();
			InitFontsLists = false;
			ApplyFonts();
			EnableFontNameSize(EnableState: true);
			LoadLists();
			if (Gf.ValidSongID(Gf.DB_CurSongID))
			{
				EnableFontNameSize(EnableState: false);
				IgnoreChange = true;
				LoadSong(Gf.DB_CurSongID);
				IgnoreChange = false;
				tbLyrics1.Focus();
			}
			else if (Gf.CurFolderName != "")
			{
				SongFolder.Text = Gf.CurFolderName;
				SavedFolder = Gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString()).ToString();
			}
			BuildEditHistoryMenuItems();
			Gf.EditorItemID = Gf.DB_CurSongID;
			EnableEditHistory();
			AddToEditHistory("D" + Convert.ToString(Gf.DB_CurSongID));
			tbLyrics1.DragEnter += tbLyrics_DragEnter;
			tbLyrics1.DragDrop += tbLyrics1_DragDrop;
			tbLyrics2.DragEnter += tbLyrics_DragEnter;
			tbLyrics2.DragDrop += tbLyrics2_DragDrop;
			TrackBarVolume.Value = (((Gf.MediaVolume >= 0) & (Gf.MediaVolume <= 100)) ? Gf.MediaVolume : 20);
			TimerTrack.Start();
			TimerEditRequest.Start();
			BeginInvoke(new MethodInvoker(focustbLyrics1));
			InitLoad = false;
		}

		private void focustbLyrics1()
		{
			tbLyrics1.Focus();
		}

		private void BuildFolderList()
		{
			SongFolder.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (Gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(Gf.FolderName[i]);
				}
			}
			if (SongFolder.Items.Count == 0)
			{
				SongFolder.Items.Add(Gf.FolderName[1]);
			}
			SongFolder.Text = SongFolder.Items[0].ToString();
		}

		private void BuildEditHistoryMenuItems()
		{
			Menu_EditHistoryList.DropDownItems.Clear();
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
			for (int i = 0; i < Gf.AbsoluteMaxHitoryItems - 1; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Name = "Menu_EditHistory" + i;
				toolStripMenuItem.Text = "";
				Menu_EditHistoryList.DropDownItems.Add(toolStripMenuItem);
				Menu_EditHistoryList.DropDownItems[i].Tag = i.ToString();
			}
			EventHandler value = new EventHandler(Menu_EditHistory_Click).Invoke;
			foreach (ToolStripMenuItem dropDownItem in Menu_EditHistoryList.DropDownItems)
			{
				dropDownItem.Click += value;
			}
		}

		private void tbLyrics_DragEnter(object sender, DragEventArgs e)
		{
			string a = DragDropItemType(e);
			if (a == DataFormats.FileDrop)
			{
				bool flag = true;
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (array == null)
				{
					flag = false;
				}
				else
				{
					string strExt = Path.GetExtension(array[0]).ToLower();
					if ((strExt != ".doc") & (strExt != ".rtf") & (strExt != ".txt"))
					{
						flag = false;
					}
				}

				e.Effect = (flag ? DragDropEffects.Copy : DragDropEffects.None);
			}
		}

		private string DragDropItemType(DragEventArgs e)
		{
			int num = e.Data.GetFormats().Length - 1;
			for (int i = 0; i <= num; i++)
			{
				if (e.Data.GetFormats()[i].Equals(DataFormats.FileDrop))
				{
					return DataFormats.FileDrop;
				}
			}
			return "";
		}

		private void tbLyrics1_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				GetExternalDocumentContents(array[0], 1);
				e.Effect = DragDropEffects.None;
			}
		}

		private void tbLyrics2_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				GetExternalDocumentContents(array[0], 2);
				e.Effect = DragDropEffects.None;
			}
		}

		private void R1Btn_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			InsertingPresetItem = true;
			Gf.InsertIndicator(ref tbLyrics1, DataUtil.ObjToInt(button.Tag));
			InsertingPresetItem = false;
			Lyrics_TextChanged(1);
		}

		private void R2Btn_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			InsertingPresetItem = true;
			Gf.InsertIndicator(ref tbLyrics2, DataUtil.ObjToInt(button.Tag));
			InsertingPresetItem = false;
			Lyrics_TextChanged(2);
		}

		private void EnableFontNameSize(bool EnableState)
		{
			ComboFontName.Enabled = EnableState;
			ComboMainFontSize.Enabled = EnableState;
			ComboNotationFontSize.Enabled = EnableState;
		}

		private void BuildTempItems()
		{
			tempSequenceCopied.Clear();
			tempSequenceCopied.Columns.Add("tempSequenceCopied1");
			tempSequenceCopied.Columns.Add("tempSequenceCopied2");
			ListViewNotations.Columns.Add("ListViewNotations1");
			ListViewNotations.Columns.Add("ListViewNotations2");
			ListViewNotations.Columns.Add("ListViewNotations3");
			ListViewNotations.Columns.Add("ListViewNotations4");
			ListViewNotations.Columns.Add("ListViewNotations5");
			ListViewNotations.View = System.Windows.Forms.View.Details;
			ListViewNotationLog.Columns.Add("ListViewNotationLog1");
			ListViewNotationLog.Columns.Add("ListViewNotationLog2");
			ListViewNotationLog.Columns.Add("ListViewNotationLog3");
			ListViewNotationLog.Columns.Add("ListViewNotationLog4");
			ListViewNotationLog.Columns.Add("ListViewNotationLog5");
			ListViewNotationLog.View = System.Windows.Forms.View.Details;
			ScreenBreaks1.Columns.Add("ScreenBreak1");
			ScreenBreaks1.Columns.Add("ScreenBreak2");
			tbWorkspace.WordWrap = false;
			tbTempSpace.WordWrap = false;
		}

		private void Menu_New_Click(object sender, EventArgs e)
		{
			NewItem();
		}

		private void Menu_Save_Click(object sender, EventArgs e)
		{
			SaveBtn_Click();
		}

		private void Menu_SaveExit_Click(object sender, EventArgs e)
		{
			SaveExitBtn_Click();
		}

		private void Menu_Import_Click(object sender, EventArgs e)
		{
			ExternalDocumentBtnPressed();
		}

		private void Menu_WordWrap_Click(object sender, EventArgs e)
		{
			SetLyricsWordWrap(Menu_WordWrap.Checked);
		}

		private void SetLyricsWordWrap(bool IsChecked)
		{
			SetWordWrap = IsChecked;
			Main_WordWrap.Checked = SetWordWrap;
			Menu_WordWrap.Checked = SetWordWrap;
			tbLyrics1.WordWrap = SetWordWrap;
			tbLyrics2.WordWrap = SetWordWrap;
		}

		private void Menu_Exit_Click(object sender, EventArgs e)
		{
			QuitEditor();
		}

		private void Menu_TransposeDown_Click(object sender, EventArgs e)
		{
			BtnChordsClick(-1);
		}

		private void Menu_TransposeUp_Click(object sender, EventArgs e)
		{
			BtnChordsClick(1);
		}

		private void Main_New_Click(object sender, EventArgs e)
		{
			NewItem();
		}

		private void Main_Save_Click(object sender, EventArgs e)
		{
			SaveBtn_Click();
		}

		private void Main_SaveExit_Click(object sender, EventArgs e)
		{
			SaveExitBtn_Click();
		}

		private void Main_Import_Click(object sender, EventArgs e)
		{
			ExternalDocumentBtnPressed();
		}

		private void Main_WordWrap_Click(object sender, EventArgs e)
		{
			SetLyricsWordWrap(Main_WordWrap.Checked);
		}

		private void Main_TransposeDown_Click(object sender, EventArgs e)
		{
			BtnChordsClick(-1);
		}

		private void Main_TransposeUp_Click(object sender, EventArgs e)
		{
			BtnChordsClick(1);
		}

		private void LoadLists()
		{
			Gf.LoadSongKeyCapoTiming(ref SongCapo, ref SongKey, ref SongTiming);
			LicAdminInfo1.Items.Clear();
			LicAdminInfo1.Items.Add("");
			LicAdminInfo1.Items.Add(Gf.LicAdmin_List[2, 0]);
			LicAdminInfo1.Items.Add(Gf.LicAdmin_List[3, 0]);
			LicAdminInfo2.Items.Clear();
			LicAdminInfo2.Items.Add("");
			LicAdminInfo2.Items.Add(Gf.LicAdmin_List[2, 0]);
			LicAdminInfo2.Items.Add(Gf.LicAdmin_List[3, 0]);
			if (Gf.LicAdmin_List[4, 0] != "")
			{
				LicAdminInfo1.Items.Add(Gf.LicAdmin_List[4, 0]);
				LicAdminInfo2.Items.Add(Gf.LicAdmin_List[4, 0]);
			}
			if (Gf.LicAdmin_List[5, 0] != "")
			{
				LicAdminInfo1.Items.Add(Gf.LicAdmin_List[5, 0]);
				LicAdminInfo2.Items.Add(Gf.LicAdmin_List[5, 0]);
			}
			if (Gf.LicAdmin_List[6, 0] != "")
			{
				LicAdminInfo1.Items.Add(Gf.LicAdmin_List[6, 0]);
				LicAdminInfo2.Items.Add(Gf.LicAdmin_List[6, 0]);
			}
		}

		private void ResetAll()
		{
			Gf.InitialiseIndividualData(ref Gf.EditItem1);
			Gf.InitialiseIndividualData(ref Gf.EditItem2);
			CurSongID = -1;
			SongTitle.Text = "";
			SongTitle2.Text = "";
			InitSongTitle2 = "";
			WriterInfo.Text = "";
			BookReference.Text = "";
			UserReference.Text = "";
			CopyrightInfo.Text = "";
			LicAdminInfo1.Text = "";
			LicAdminInfo2.Text = "";
			tbLyrics1.Text = "";
			tbLyrics2.Text = "";
			SongNumber.Text = "";
			SongCapo.Text = "";
			SongKey.Text = "";
			SongTiming.Text = "";
			Lyrics1SavedNotations = "";
			Lyrics2SavedNotations = "";
			CombinedLyrics = "";
			CombinedNotations = "";
			VersesList.Items.Clear();
			OrderList.Items.Clear();
			Rotate_VersesList.Items.Clear();
			Rotate_OrderList.Items.Clear();
			Rotate_TimeTotal.MinDate = InitDateTime;
			Rotate_TimeTotal.Value = InitDateTime;
			CurMediaLength = 0.0;
			LabelDuration.Text = "00:00";
			if (PlayerOK)
			{
				DShowPlayer.newFilename = "";
			}
			OrderListSequence = "";
			LinkTitle2Pic.Visible = false;
			Title2IgnoreChange = false;
			for (int i = 0; i < 160; i++)
			{
				prevVersePresent[i] = false;
				VersePresent[i] = false;
			}
			tempSequenceCopied.Items.Clear();
			RotateString = "";
			Rotate_tbSourceLocation.Text = "";
			if (!Rotate_Equal.Checked)
			{
				Rotate_Equal.Checked = true;
			}
			else
			{
				MediaOption_Changed();
			}
			UpdateRotateTimePositions(1, 0, 0, "", UseRotateTimings: true, ResetAll: true);
			UpdateSavedStrings();
			ClearStack(0);
		}

		private void UpdateSavedStrings()
		{
			Lyrics1SavedCopy = tbLyrics1.Text;
			Lyrics2SavedCopy = tbLyrics2.Text;
			SavedTitle = SongTitle.Text;
			SavedTitle2 = SongTitle2.Text;
			SavedSongNumber = SongNumber.Text;
			SavedBookReference = BookReference.Text;
			SavedUserReference = UserReference.Text;
			SavedWriterInfo = WriterInfo.Text;
			SavedCopyrightInfo = CopyrightInfo.Text;
			SavedLicAdminInfo1 = LicAdminInfo1.Text;
			SavedLicAdminInfo2 = LicAdminInfo2.Text;
			SavedCapo = SongCapo.Text;
			SavedSongKey = SongKey.Text;
			SavedSongTiming = SongTiming.Text;
			SavedFolder = Gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString()).ToString();
			SavedSequence = OrderListSequence;
			SavedRotateString = RotateString;
		}

		private void rtfMain_Change(int Region)
		{
			if (!IgnoreChange)
			{
				StackIndex[Region]++;
				if ((StackIndex[Region] == StackStartPoint[Region]) & (StackStartPoint[Region] > 0))
				{
					StackStartPoint[Region]++;
				}
				if (StackStartPoint[Region] > StackArrayMaxPoint)
				{
					StackStartPoint[Region] = 1;
				}
				if (StackIndex[Region] > StackArrayMaxPoint)
				{
					StackIndex[Region] = 0;
					StackStartPoint[Region] = 1;
				}
				StackMaxRedo[Region] = StackIndex[Region];
				iCursorPosition[Region, StackIndex[Region]] = ((Region == 1) ? tbLyrics1.SelectionStart : tbLyrics2.SelectionStart);
				sStack[Region, StackIndex[Region]] = ((Region == 1) ? tbLyrics1.Rtf : tbLyrics2.Rtf);
			}
		}

		private void ApplyFonts()
		{
			if (!InitFontsLists)
			{
				Gf.EditMainFontSize = DataUtil.StringToInt(ComboMainFontSize.Text);
				if ((Gf.EditMainFontSize < 6) | (Gf.EditMainFontSize > 20))
				{
					Gf.EditMainFontSize = 12;
				}
				Gf.EditNotationFontSize = DataUtil.StringToInt(ComboNotationFontSize.Text);
				if ((Gf.EditNotationFontSize < 6) | (Gf.EditNotationFontSize > 20))
				{
					Gf.EditNotationFontSize = 10;
				}
				MainFont = Gf.GetNewFont(ComboFontName.Text, Gf.EditMainFontSize, InBold: false, InItalic: false, InUnderline: false, ShowErrorMsg: false);
				Gf.EditMainFontName = MainFont.Name;
				tbLyrics1.Font = MainFont;
				tbLyrics2.Font = MainFont;
				tbWorkspace.Font = MainFont;
				tbTempSpace.Font = MainFont;
				NotationFont = Gf.GetNewFont(Gf.EditMainFontName, Gf.EditNotationFontSize, InBold: false, InItalic: false, InUnderline: false, ShowErrorMsg: false);
				RegUtil.SaveRegValue("options", "EditMainFontName", Gf.EditMainFontName);
				RegUtil.SaveRegValue("options", "EditMainFontSize", Gf.EditMainFontSize);
				RegUtil.SaveRegValue("options", "EditNotationFontSize", Gf.EditNotationFontSize);
			}
		}

		/// <summary>
		/// daniel
		/// docx 확장자 추가
		/// </summary>
		private void ExternalDocumentBtnPressed()
		{
			OpenFileDialog1.Filter = "Word Documents and Text Files (*.doc;*.docx;*.txt) | *.doc;*docx;*.txt|Word Documents (*.doc;*.docx)|*.doc;*.docx|Text Files (*.txt)|*.txt";
			OpenFileDialog1.InitialDirectory = Gf.EditOpenDocumentDir;
			OpenFileDialog1.FileName = "";
			if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Gf.EditOpenDocumentDir = Path.GetDirectoryName(OpenFileDialog1.FileName);
				RegUtil.SaveRegValue("options", "EditOpenDocumentDir", Gf.EditOpenDocumentDir);
				string fileName = OpenFileDialog1.FileName;
				GetExternalDocumentContents(fileName, 3);
			}
		}

		private void GetExternalDocumentContents(string InFileName, int RegNum)
		{
			Cursor = Cursors.WaitCursor;
			string OutText = "";
			string OutText2 = "";
			string inLyrics = "";
			string text = "";
			string text2 = "";
			bool flag = false;

			string strExt = Path.GetExtension(InFileName).ToLower();

			switch (strExt)
			{
				case ".doc":
				case "*.docx":
					inLyrics = Gf.GetOfficeDocContents(InFileName);
					flag = true;
					break;
				case ".txt":
					inLyrics = gfFileHelpers.LoadTextFile(InFileName, ShowErrorMsg: true);
					flag = true;
					break;
			}
			if (flag)
			{
				Gf.ExtractLyrics(inLyrics, "", ref OutText, ref Lyrics1SavedNotations, ref OutText2, ref Lyrics2SavedNotations);
				string text3 = "";
				string text4 = "";
				if (RegNum == 1 || RegNum == 3)
				{
					text = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
					text3 = OutText;
					text4 = OutText2;
				}
				else if (OutText2 != "")
				{
					text3 = OutText;
					text4 = OutText2;
				}
				else
				{
					text4 = OutText;
					if (SongTitle2.Text == "")
					{
						text2 = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
					}
				}
				if (RegNum == 3 || (tbLyrics1.Text != "" && text3 != "") || (tbLyrics2.Text != "" && text4 != ""))
				{
					if (!NewItem())
					{
						Cursor = Cursors.Default;
						return;
					}
					SongTitle.Text = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				}
				if (text != "")
				{
					SongTitle.Text = text;
				}
				if (text2 != "")
				{
					SongTitle2.Text = text2;
				}
				if (text3 != "")
				{
					tbLyrics1.Text = Gf.CombineLyricsAndNotations(text3, Lyrics1SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
				}
				if (text4 != "")
				{
					tbLyrics2.Text = Gf.CombineLyricsAndNotations(text4, Lyrics2SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
				}
			}
			Cursor = Cursors.Default;
		}

		private void SwitchChinese(int Region)
		{
			int num = -1;
			switch (Region)
			{
			case 1:
			{
				num = Gf.SwitchChinese(ref tbLyrics1);
				string InString = SongTitle.Text;
				Gf.SwitchChinese(ref InString, num);
				SongTitle.Text = InString;
				InString = SongTitle2.Text;
				Gf.SwitchChinese(ref InString, num);
				SongTitle2.Text = InString;
				InString = WriterInfo.Text;
				Gf.SwitchChinese(ref InString, num);
				WriterInfo.Text = InString;
				InString = CopyrightInfo.Text;
				Gf.SwitchChinese(ref InString, num);
				CopyrightInfo.Text = InString;
				break;
			}
			case 2:
				Gf.SwitchChinese(ref tbLyrics2);
				break;
			}
		}

		private void BtnChordsClick(int TransposeStep)
		{
			int flatSharpKey = -1;
			if (SongKey.Text != "")
			{
				string InKey = SongKey.Text;
				flatSharpKey = Gf.TransposeKey(ref InKey, TransposeStep);
				SongKey.Text = InKey;
			}
			if (SongCapo.Text != "")
			{
				int selectedIndex = SongCapo.SelectedIndex;
				selectedIndex -= TransposeStep;
				if (selectedIndex > 12)
				{
					selectedIndex = (selectedIndex - 1) % 12 + 1;
				}
				else if (selectedIndex < 1)
				{
					selectedIndex = 13 - (selectedIndex + 1);
				}
				SongCapo.SelectedIndex = selectedIndex;
			}
			IgnoreChange = true;
			BtnChordsClick(TransposeStep, ref tbLyrics1, ref Lyrics1SavedNotations, ref Lyrics1Only, flatSharpKey);
			BtnChordsClick(TransposeStep, ref tbLyrics2, ref Lyrics2SavedNotations, ref Lyrics2Only, flatSharpKey);
			Gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			Gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			IgnoreChange = false;
		}

		private void BtnChordsClick(int TransposeStep, ref RichTextBox InTextBox, ref string SavedNotations, ref string LyricsOnly, int FlatSharpKey)
		{
			ValidateMusicNotations(ref InTextBox, ref SavedNotations, ref LyricsOnly);
			string text = SavedNotations;
			string text2 = "";
			string text3 = "";
			for (int i = 0; i < text.Length; i++)
			{
				string text4 = DataUtil.Mid(text, i, 1);
				if ((text4 == "(") | (text4 == ")"))
				{
					text2 += text4;
				}
				else if (text4 == ';'.ToString())
				{
					text3 = Gf.TransposeChord(text3, TransposeStep, FlatSharpKey);
					text2 = text2 + text3 + text4;
					text3 = "";
				}
				else
				{
					text3 += text4;
				}
			}
			InTextBox.Text = Gf.CombineLyricsAndNotations(LyricsOnly, text2, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
		}

		private bool ValidateMusicNotations(ref RichTextBox InTextBox, ref string InSavedNotations, ref string InLyricsOnly)
		{
			InSavedNotations = "";
			InLyricsOnly = InTextBox.Text;
			if (InTextBox.Text.IndexOf("»") < 0)
			{
				return true;
			}
			int num = DataUtil.CountLf(InTextBox.Text);
			if (num < 1)
			{
				return true;
			}
			int num2 = 0;
			ListViewItem listViewItem = new ListViewItem();
			BuildListofNotationLines(ref InTextBox, ref ListViewNotations, ref InLyricsOnly, num);
			int num3 = num - 1;
			bool flag2 = false;
			for (int num4 = num - 1; num4 >= 0; num4--)
			{
				if (ListViewNotations.Items[num4].SubItems[1].Text == "" && num4 > 0)
				{
					if (ListViewNotations.Items[num4 - 1].SubItems[1].Text == "»")
					{
						ListViewNotations.Items[num4].SubItems[2].Text = Convert.ToString(num4 - 1);
					}
					else
					{
						flag2 = true;
					}
				}
				else
				{
					flag2 = true;
				}
				if (flag2)
				{
					ListViewNotations.Items[num4].Remove();
				}
				flag2 = false;
			}
			int InMin = 0;
			int InMax = 0;
			int InMin2 = 0;
			int InMax2 = 0;
			int num6 = 0;
			string MusicNotationName = "";
			System.Drawing.Point MusicNotationCoOrd = new System.Drawing.Point(0, 0);
			InSavedNotations = "";
			for (int num4 = 0; num4 <= ListViewNotations.Items.Count - 1; num4++)
			{
				ListViewNotationLog.Items.Clear();
				Gf.GetMinMaxfromTextBox(InTextBox, DataUtil.StringToInt(ListViewNotations.Items[num4].SubItems[2].Text), ref InMin, ref InMax);
				Gf.GetMinMaxfromTextBox(InTextBox, DataUtil.StringToInt(ListViewNotations.Items[num4].Text), ref InMin2, ref InMax2);
				num2 = DataUtil.StringToInt(ListViewNotations.Items[num4].SubItems[2].Text) - num4;
				num6 = 0;
				while (FindNextNotation(InTextBox, ref num6, ref MusicNotationName, ref MusicNotationCoOrd, InMin, InMax))
				{
					int associatedLyricsLineCurPos = GetAssociatedLyricsLineCurPos(ref InTextBox, MusicNotationCoOrd.X, InMin2, InMax2);
					listViewItem = ListViewNotationLog.Items.Add(MusicNotationName);
					listViewItem.SubItems.Add(Convert.ToString(associatedLyricsLineCurPos));
				}
				object obj = InSavedNotations;
				InSavedNotations = string.Concat(obj, "(", Convert.ToString(num2), ';');
				for (int i = 0; i <= ListViewNotationLog.Items.Count - 1; i++)
				{
					obj = InSavedNotations;
					InSavedNotations = string.Concat(obj, ListViewNotationLog.Items[i].Text, ';', ListViewNotationLog.Items[i].SubItems[1].Text, ';');
				}
				InSavedNotations += ")";
			}
			return true;
		}

		public void BuildListofNotationLines(ref RichTextBox InTextBox, ref ListView StoreNotationsList, ref string InLyricsOnly, int TotalLines)
		{
			ListViewItem listViewItem = new ListViewItem();
			int InMin = 0;
			int InMax = 0;
			InLyricsOnly = "";
			StoreNotationsList.Items.Clear();
			for (int i = 0; i <= TotalLines - 1; i++)
			{
				string text = InTextBox.Lines[i];
				bool flag = (text.IndexOf("»") >= 0) ? true : false;
				if (flag)
				{
					if (i < TotalLines - 1)
					{
						if (InTextBox.Lines[i + 1].IndexOf("»") >= 0)
						{
							flag = false;
						}
						else if (DataUtil.TrimEnd(InTextBox.Lines[i + 1]) == "")
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				listViewItem = StoreNotationsList.Items.Add(Convert.ToString(i));
				if (flag)
				{
					listViewItem.SubItems.Add("»");
					listViewItem.SubItems.Add("");
					continue;
				}
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add("");
				Gf.GetMinMaxfromTextBox(InTextBox, i, ref InMin, ref InMax);
				text = DataUtil.Mid(InTextBox.Text, InMin, InMax - InMin + 1);
				InLyricsOnly = InLyricsOnly + text + ((i < TotalLines - 1) ? "\n" : "");
			}
			InLyricsOnly = InLyricsOnly.Replace(" »", "");
			InLyricsOnly = InLyricsOnly.Replace(" »", "");
			InLyricsOnly = InLyricsOnly.Replace("»", "");
		}

		public void OldGetMinMaxfromTextBox(RichTextBox InBox, int InLineNumber, ref int InMin, ref int InMax)
		{
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

		public bool FindNextNotation(RichTextBox IntextBox, ref int StartMusicCurPos, ref string MusicNotationName, ref System.Drawing.Point MusicNotationCoOrd, int MusicCurPosMin, int MusicCurPosMax)
		{
			bool flag = false;
			IntextBox.SelectionLength = 0;
			MusicNotationName = "";
			if (StartMusicCurPos > MusicCurPosMax - MusicCurPosMin + 1)
			{
				return false;
			}
			for (int i = StartMusicCurPos; i <= MusicCurPosMax - MusicCurPosMin + 1; i++)
			{
				string text = DataUtil.Mid(IntextBox.Text, MusicCurPosMin + i, 1);
				if ((text != " ") & (text != "»"))
				{
					if (!flag)
					{
						flag = true;
						int index = IntextBox.SelectionStart = MusicCurPosMin + i;
						MusicNotationCoOrd = IntextBox.GetPositionFromCharIndex(index);
						int x = MusicNotationCoOrd.X;
					}
					MusicNotationName += text;
				}
				else if (flag)
				{
					StartMusicCurPos = i;
					return true;
				}
			}
			return false;
		}

		public int GetAssociatedLyricsLineCurPos(ref RichTextBox IntextBox, int MusicNotationCoOrdX, int LyricsCurPosMin, int LyricsCurPosMax)
		{
			int num = 0;
			int num2 = num;
			for (num2 = num; num2 <= LyricsCurPosMax - LyricsCurPosMin + 1; num2++)
			{
				if (IntextBox.GetPositionFromCharIndex(num2 + LyricsCurPosMin).X <= MusicNotationCoOrdX)
				{
					num = num2;
					continue;
				}
				return num;
			}
			return num2 - 1;
		}

		private string OldCombineLyricsAndNotations(string InLyrics, string InNotations)
		{
			if ((InNotations == "") | (InLyrics == ""))
			{
				return InLyrics;
			}
			StringBuilder stringBuilder = new StringBuilder();
			tbWorkspace.Text = InLyrics;
			Gf.MarkSelectedRTB(ref tbWorkspace, 0, tbWorkspace.Text.Length, 0, MainFont, NotationFont);
			int num = DataUtil.CountLf(tbWorkspace.Text);
			int InMin = 0;
			int InMax = 0;
			string text = "";
			int num2 = Gf.ListNotationData(InNotations, ref Gf.NotationsArray, num);
			for (int i = 0; i < num; i++)
			{
				if (num2 > 0 && Gf.NotationsArray[i] != "")
				{
					Gf.GetMinMaxfromTextBox(tbWorkspace, i, ref InMin, ref InMax);
					tbTempSpace.Text = "";
					string text2 = "";
					while (Gf.NotationsArray[i].Length > 0)
					{
						text = Gf.NotationsArray[i];
						string text3 = DataUtil.ExtractOneInfo(ref text, ';', RemoveExtract: true, MinusOneIfBlank: false);
						int inCurPos = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref text, ';', RemoveExtract: true, MinusOneIfBlank: false));
						Gf.NotationsArray[i] = text;
						int associatedLyricsLineCurPosX = Gf.GetAssociatedLyricsLineCurPosX(ref tbWorkspace, inCurPos, InMin, InMax);
						while (Gf.GetAssociatedLyricsLineCurPosX(ref tbTempSpace, tbTempSpace.Text.Length - 1) < associatedLyricsLineCurPosX - 1)
						{
							text2 += " ";
							tbTempSpace.Text = text2;
							Gf.MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
						}
						text2 += (((text2.Length > 1) & (DataUtil.Right(text2, 1) != " ")) ? (" " + text3) : text3);
						tbTempSpace.Text = text2;
						Gf.MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
					}
					stringBuilder.Append(tbTempSpace.Text + " »\n");
				}
				stringBuilder.Append(tbWorkspace.Lines[i] + "\n");
			}
			if (DataUtil.Right(stringBuilder.ToString(), 1) == "\n")
			{
				return DataUtil.Left(stringBuilder.ToString(), stringBuilder.Length - 1);
			}
			return stringBuilder.ToString();
		}

		private void EnableEditHistory()
		{
			Gf.LoadRegistryEditorEditHistory();
			UpdateMenu_EditHistory();
		}

		private void AddToEditHistory(string InItemID)
		{
			if (!((Gf.GetItemTitle(InItemID) == "") | (Gf.EditorEditHistoryList[1, 0] == InItemID)))
			{
				if (Gf.TotalEditorEditHistory < Gf.MaxUserEditHistory)
				{
					Gf.TotalEditorEditHistory++;
				}
				else
				{
					Gf.TotalEditorEditHistory = Gf.MaxUserEditHistory;
				}
				for (int num = Gf.TotalEditorEditHistory; num >= 2; num--)
				{
					Gf.EditorEditHistoryList[num, 0] = Gf.EditorEditHistoryList[num - 1, 0];
				}
				Gf.EditorEditHistoryList[1, 0] = InItemID;
				Gf.RemoveDuplicateEditorHistoryItems(ref Gf.EditorEditHistoryList, ref Gf.TotalEditorEditHistory);
				UpdateMenu_EditHistory();
				Gf.SaveEditorEditHistoryToRegistry();
			}
		}

		private void UpdateMenu_EditHistory()
		{
			try
			{
				int num = 0;
				string text = "";
				if ((Gf.TotalEditorEditHistory < 0) | (Gf.TotalEditorEditHistory > Gf.AbsoluteMaxHitoryItems))
				{
					Gf.TotalEditorEditHistory = Gf.AbsoluteMaxHitoryItems;
				}
				for (int i = 1; i <= Gf.TotalEditorEditHistory; i++)
				{
					text = Gf.GetItemTitle(Gf.EditorEditHistoryList[i, 0]);
					if (text != "" && Gf.EditorEditHistoryList[num, 0] != Gf.EditorEditHistoryList[i, 0])
					{
						num++;
						Gf.EditorEditHistoryList[num, 0] = Gf.EditorEditHistoryList[i, 0];
						Gf.EditorEditHistoryList[num, 1] = text;
					}
				}
				Gf.TotalEditorEditHistory = num;
				for (int i = Gf.TotalEditorEditHistory + 1; i <= Gf.AbsoluteMaxHitoryItems; i++)
				{
					Gf.EditorEditHistoryList[i, 0] = "";
					Gf.EditorEditHistoryList[i, 1] = "";
				}
				for (int i = 1; i < Gf.AbsoluteMaxHitoryItems; i++)
				{
					Menu_EditHistoryList.DropDownItems[i - 1].Text = i + " " + Gf.EditorEditHistoryList[i, 1];
					Menu_EditHistoryList.DropDownItems[i - 1].Visible = ((i <= Gf.TotalEditorEditHistory) ? true : false);
				}
			}
			catch
			{
			}
		}

		private bool NewItem()
		{
			ClearErrorMessage(0);
			if (ActionBeforeNextEvent() == DialogResult.Cancel)
			{
				return false;
			}
			ResetAll();
			return true;
		}

		private void QuitEditor()
		{
			ClearErrorMessage(0);
			Save_FormPos_To_Registry();
			if (ActionBeforeNextEvent() != DialogResult.Cancel)
			{
				FormCanClose = true;
				Close();
			}
		}

		private void ShowMessage(int Region, string MsgText)
		{
			if ((Region == 0) | (Region == 1))
			{
				LabeltbLyrics.Font = new System.Drawing.Font(LabeltbLyrics.Font, FontStyle.Bold);
				LabeltbLyrics.BackColor = Color.Yellow;
				LabeltbLyrics.Text = MsgText;
			}
			if ((Region == 0) | (Region == 2))
			{
				LabeltbLyrics2.Font = new System.Drawing.Font(LabeltbLyrics2.Font, FontStyle.Bold);
				LabeltbLyrics2.BackColor = Color.Yellow;
				LabeltbLyrics2.Text = MsgText;
			}
		}

		private void ClearErrorMessage(int Region)
		{
			if ((Region == 0) | (Region == 1))
			{
				LabeltbLyrics.Font = new System.Drawing.Font(LabeltbLyrics.Font, FontStyle.Regular);
				LabeltbLyrics.BackColor = label1.BackColor;
				LabeltbLyrics.Text = "Region 1" + (SetRightToLeft1 ? " : Right-To-Left Text On" : "");
				ScreenBreak1Available = false;
			}
			if ((Region == 0) | (Region == 2))
			{
				LabeltbLyrics2.Font = new System.Drawing.Font(LabeltbLyrics2.Font, FontStyle.Regular);
				LabeltbLyrics2.BackColor = label1.BackColor;
				LabeltbLyrics2.Text = "Region 2" + (SetRightToLeft2 ? " : Right-To-Left Text On" : "");
				ScreenBreak2Available = false;
			}
		}

		private void Save_FormPos_To_Registry()
		{
			if (base.WindowState == FormWindowState.Maximized)
			{
				RegUtil.SaveRegValue("settings", Reg_FormMax, 1);
				RegUtil.SaveRegValue("settings", Reg_FormWordWrap, tbLyrics1.WordWrap ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormSetChordsMenu, SetChordsMenu ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormRegion2Tab, tabRightPane.SelectedIndex);
			}
			else if (base.WindowState != FormWindowState.Maximized)
			{
				RegUtil.SaveRegValue("settings", Reg_FormMax, 0);
				RegUtil.SaveRegValue("settings", Reg_FormLeft, base.Left);
				RegUtil.SaveRegValue("settings", Reg_FormTop, base.Top);
				RegUtil.SaveRegValue("settings", Reg_FormWidth, base.Width);
				RegUtil.SaveRegValue("settings", Reg_FormHeight, base.Height);
				RegUtil.SaveRegValue("settings", Reg_FormWordWrap, tbLyrics1.WordWrap ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormSetChordsMenu, SetChordsMenu ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormLyricsSplit, splitContainer1.SplitterDistance * 1000 / splitContainer1.Width);
				RegUtil.SaveRegValue("settings", Reg_FormRegion2Tab, tabRightPane.SelectedIndex);
			}
		}

		private DialogResult ActionBeforeNextEvent()
		{
			if (!ChangesMade())
			{
				return DialogResult.Yes;
			}
			string text = (CurSongID >= 0) ? ("Do you wish to save the changes you have made to " + SongTitle.Text + "?") : ("Do you wish to save the new Item " + SongTitle.Text + " to the Songs Database?");
			switch (MessageBox.Show(text, "", MessageBoxButtons.YesNoCancel))
			{
			case DialogResult.Yes:
				SaveBtn_Click();
				if (!ChangesMade())
				{
					return DialogResult.Yes;
				}
				return DialogResult.Cancel;
			case DialogResult.No:
				return DialogResult.No;
			default:
				return DialogResult.Cancel;
			}
		}

		private bool ChangesMade()
		{
			if (tbLyrics1.Text != Lyrics1SavedCopy || tbLyrics2.Text != Lyrics2SavedCopy || SongTitle.Text != SavedTitle || SongTitle2.Text != SavedTitle2 || SongNumber.Text != SavedSongNumber || BookReference.Text != SavedBookReference || UserReference.Text != SavedUserReference || WriterInfo.Text != SavedWriterInfo || CopyrightInfo.Text != SavedCopyrightInfo || LicAdminInfo1.Text != SavedLicAdminInfo1 || LicAdminInfo2.Text != SavedLicAdminInfo2 || SongKey.Text != SavedSongKey || SongTiming.Text != SavedSongTiming || SongCapo.Text != SavedCapo || GenerateRotateString() != SavedRotateString || Gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString()) != DataUtil.StringToInt(SavedFolder) || OrderListSequence != SavedSequence)
			{
				return true;
			}
			return false;
		}

		private bool SaveBtn_Click()
		{
			ClearErrorMessage(0);
			Cursor = Cursors.WaitCursor;
			if (ValidateAllDetails())
			{
				SaveSong();
				Cursor = Cursors.Default;
				return true;
			}
			Cursor = Cursors.Default;
			return false;
		}

		private void SaveExitBtn_Click()
		{
			if (SaveBtn_Click())
			{
				QuitEditor();
			}
		}

		private void SaveSong()
		{
			ClearErrorMessage(0);
			if (!Gf.ValidateDB(DatabaseType.Songs))
			{
				return;
			}
			bool flag = false;
			Gf.EditorItemNew = false;
			Gf.EditorItemFolderChanged = false;
			Gf.EditorItemTitleChanged = false;
			Gf.EditorItemTitle = DataUtil.Left(SongTitle.Text, 100);
			Gf.EditorItemNewFolder = Gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString());
			Gf.EditItem1.Title = Gf.EditorItemTitle;
			Gf.EditItem1.Title2 = SongTitle2.Text;
			Gf.EditItem1.SongNumber = DataUtil.StringToInt(SongNumber.Text);
			Gf.EditItem1.FolderNo = Gf.EditorItemNewFolder;
			Gf.EditItem1.CompleteLyrics = CombinedLyrics;
			Gf.EditItem1.Notations = CombinedNotations;
			Gf.EditItem1.SongSequence = OrderListSequence;
			Gf.EditItem1.Writer = WriterInfo.Text;
			Gf.EditItem1.Copyright = CopyrightInfo.Text;
			Gf.EditItem1.Category = "";
			Gf.EditItem1.Timing = SongTiming.Text;
			Gf.EditItem1.MusicKey = SongKey.Text;
			Gf.EditItem1.Capo = SongCapo.SelectedIndex - 1;
			Gf.EditItem1.Show_LicAdminInfo1 = LicAdminInfo1.Text;
			Gf.EditItem1.Show_LicAdminInfo2 = LicAdminInfo2.Text;
			Gf.EditItem1.Book_Reference = BookReference.Text;
			Gf.EditItem1.User_Reference = UserReference.Text;
			Gf.EditItem1.RotateString = GenerateRotateString();
			Gf.EditItem1.Settings = Gf.CombineSettings(Gf.EditItem1);
			if (CurSongID < 0)
			{
				Gf.EditorItemNew = true;
				if (SongNumber.Text == "")
				{
					SongNumber.Text = "0";
				}
				int num2 = Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, Gf.EditItem1.Title, Gf.EditItem1.Title2, Gf.EditItem1.SongNumber, Gf.EditItem1.FolderNo, Gf.EditItem1.CompleteLyrics, Gf.EditItem1.SongSequence, Gf.EditItem1.Writer, Gf.EditItem1.Copyright, Gf.EditItem1.Capo, Gf.EditItem1.Timing, Gf.EditItem1.MusicKey, Gf.EditItem1.Notations, "", Gf.EditItem1.Show_LicAdminInfo1, Gf.EditItem1.Show_LicAdminInfo2, Gf.EditItem1.Book_Reference, Gf.EditItem1.User_Reference, Gf.EditItem1.Settings, Gf.EditItem1.Format.FormatString);
				if (num2 > 0)
				{
					flag = true;
					CurSongID = num2;
				}
			}
			else
			{
				Gf.LoadDBFormatString(ref Gf.EditItem1);
				flag = Gf.UpdateDatabaseItem(Gf.ConnectStringMainDB, CurSongID, Gf.EditItem1.Title, Gf.EditItem1.Title2, Gf.EditItem1.SongNumber, Gf.EditItem1.FolderNo, Gf.EditItem1.CompleteLyrics, Gf.EditItem1.SongSequence, Gf.EditItem1.Writer, Gf.EditItem1.Copyright, Gf.EditItem1.Capo, Gf.EditItem1.Timing, Gf.EditItem1.MusicKey, Gf.EditItem1.Notations, "", Gf.EditItem1.Show_LicAdminInfo1, Gf.EditItem1.Show_LicAdminInfo2, Gf.EditItem1.Book_Reference, Gf.EditItem1.User_Reference, Gf.EditItem1.Settings, Gf.EditItem1.Format.DBStoredFormat);
			}
			if (flag)
			{
				ShowMessage(1, "Song saved");
			}
			else
			{
				ShowMessage(1, "Error encountered - Item NOT saved.");
			}
			RotateString = Gf.EditItem1.RotateString;
			UpdateSavedStrings();
			Gf.EditorItemID = CurSongID;
			AddToEditHistory("D" + CurSongID);
			Gf.EditorLoadItem = true;
		}

		private void LoadSong(int InDB_CurSongID)
		{
			if (Gf.ValidateDB(DatabaseType.Songs) && Gf.ValidSongID(InDB_CurSongID))
			{
				ResetAll();
				CurSongID = InDB_CurSongID;
				string OutText = "";
				string OutText2 = "";
				Gf.InitialiseIndividualData(ref Gf.EditItem1);
				Gf.LoadIndividualData(ref Gf.EditItem1, "D" + InDB_CurSongID, "", 1);
				Gf.EditItem1.Show_LicAdminInfo1 = Gf.EditItem1.In_LicAdminInfo1;
				Gf.EditItem1.Show_LicAdminInfo2 = Gf.EditItem1.In_LicAdminInfo2;
				SongTitle.Text = Gf.EditItem1.Title;
				SongFolder.Text = Gf.FolderName[Gf.EditItem1.FolderNo];
				Title2IgnoreChange = true;
				SongTitle2.Text = Gf.EditItem1.Title2;
				SongNumber.Text = Gf.EditItem1.SongNumber.ToString();
				BookReference.Text = Gf.EditItem1.Book_Reference;
				UserReference.Text = Gf.EditItem1.User_Reference;
				InitSongTitle2 = SongTitle.Text;
				CopyrightInfo.Text = Gf.EditItem1.Copyright;
				LicAdminInfo1.Text = Gf.EditItem1.Show_LicAdminInfo1;
				LicAdminInfo2.Text = Gf.EditItem1.Show_LicAdminInfo2;
				WriterInfo.Text = Gf.EditItem1.Writer;
				OrderListSequence = Gf.EditItem1.SongSequence;
				string text = Gf.EditItem1.Capo.ToString();
				SongCapo.Text = (((text == "") | (text == "-1")) ? "" : ("Capo " + text));
				if (SongCapo.Text == "")
				{
					SongCapo.SelectedIndex = 0;
				}
				SongTiming.Text = Gf.EditItem1.Timing;
				SongKey.Text = Gf.EditItem1.MusicKey;
				Lyrics1SavedNotations = Gf.EditItem1.Notations;
				Gf.ExtractLyrics(Gf.EditItem1.CompleteLyrics, Lyrics1SavedNotations, ref OutText, ref Lyrics1SavedNotations, ref OutText2, ref Lyrics2SavedNotations);
				InitLoad = true;
				tbLyrics1.Text = Gf.CombineLyricsAndNotations(OutText, Lyrics1SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
				tbLyrics2.Text = Gf.CombineLyricsAndNotations(OutText2, Lyrics2SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
				InitLoad = false;
				UpdateVersesList();
				IgnoreChange = true;
				Gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
				Gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
				IgnoreChange = false;
				RotateString = Gf.EditItem1.RotateString;
				OrderList.Items.Clear();
				Rotate_OrderList.Items.Clear();
				Rotate_tbSourceLocation.Text = Gf.GetMediaFileName(SongTitle.Text, SongTitle2.Text);
				ListViewItem listViewItem = new ListViewItem();
				if (OrderListSequence.Length > 0)
				{
					try
					{
						for (int i = 0; i < OrderListSequence.Length; i++)
						{
							int num3 = OrderListSequence[i];
							listViewItem = OrderList.Items.Add(Gf.VerseTitle[num3]);
							listViewItem.SubItems.Add(num3.ToString());
						}
					}
					catch
					{
					}
				}
				if (Gf.EditItem1.RotateSequence.Length > 0)
				{
					try
					{
						for (int i = 0; i < Gf.EditItem1.RotateSequence.Length; i++)
						{
							int num3 = Gf.EditItem1.RotateSequence[i];
							listViewItem = Rotate_OrderList.Items.Add(Gf.VerseTitle[num3]);
							listViewItem.SubItems.Add(num3.ToString());
						}
					}
					catch
					{
					}
				}
				UpdateRotateTimePositions(Gf.EditItem1.RotateStyle, Gf.EditItem1.RotateGap, Gf.EditItem1.RotateTotal, Gf.EditItem1.RotateTimings, UseRotateTimings: true, ResetAll: false);
				UpdateSavedStrings();
				Title2IgnoreChange = false;
				SongTitle2_Change();
				tbLyrics1.Focus();
				tbLyrics1.SelectionLength = 0;
				StackTrackPos[1, 0] = 0;
				StackTrackPos[1, 1] = 0;
				StackTrackPos[2, 0] = 0;
				StackTrackPos[2, 1] = 0;
				ClearStack(0);
			}
		}

		private void SongTitle2_Change()
		{
			if (Title2IgnoreChange)
			{
				return;
			}
			if (DataUtil.Trim(SongTitle2.Text) == "")
			{
				LinkTitle2Pic.Visible = false;
			}
			else
			{
				bool flag = false;
				try
				{
					string fullSearchString = $@"select * from SONG where lower(Title_1) like \{SongTitle2.Text.ToLower()}\";
					using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

					if (datatable.Rows.Count > 0)
					{
						LinkTitle2Pic.Visible = true;
						Title2IgnoreChange = true;
						SongTitle2.Text = DataUtil.ObjToString(datatable.Rows[0]["Title_1"]);
						Title2IgnoreChange = false;
						flag = true;
					}
				}
				catch
				{
				}
				if (!flag)
				{
					LinkTitle2Pic.Visible = false;
				}
			}
			ClearErrorMessage(0);
		}

		private bool ValidateAllDetails()
		{
			if (!ValidateTitles())
			{
				return false;
			}
			if (!ValidateSequence())
			{
				return false;
			}
			int selectionStart = tbLyrics1.SelectionStart;
			int selectionStart2 = tbLyrics2.SelectionStart;
			if (!ValidateContents())
			{
				return false;
			}
			if (!ValidateMusicNotations(ref tbLyrics1, ref Lyrics1SavedNotations, ref Lyrics1Only))
			{
				return false;
			}
			if (!ValidateMusicNotations(ref tbLyrics2, ref Lyrics2SavedNotations, ref Lyrics2Only))
			{
				return false;
			}
			tbLyrics1.SelectionStart = selectionStart;
			tbLyrics1.SelectionLength = 0;
			tbLyrics1.ScrollToCaret();
			tbLyrics2.SelectionStart = selectionStart2;
			tbLyrics2.SelectionLength = 0;
			tbLyrics2.ScrollToCaret();
			Gf.EditItem1.CompleteLyrics = Lyrics1Only;
			Gf.EditItem1.Notations = Lyrics1SavedNotations;
			Gf.EditItem1.OriginalNotations = Lyrics1SavedNotations;
			Gf.EditItem2.CompleteLyrics = Lyrics2Only;
			Gf.EditItem2.Notations = Lyrics2SavedNotations;
			Gf.EditItem2.OriginalNotations = Lyrics2SavedNotations;
			Gf.Merge_Songs(Gf.EditItem1, Gf.EditItem2, ref CombinedLyrics, ref CombinedNotations);
			return true;
		}

		private bool ValidateTitles()
		{
			if (DataUtil.Trim(SongTitle.Text) == "")
			{
				ShowMessage(1, "There is no Song Title!");
				return false;
			}
			if (!Gf.ValidateTitleDetails(SongTitle.Text, "Song Title"))
			{
				return false;
			}
			if (!Gf.ValidateTitleDetails(SongTitle2.Text, "Link Title"))
			{
				return false;
			}
			if (!Gf.ValidateTitleDetails(CopyrightInfo.Text, "Copyright Info"))
			{
				return false;
			}
			if (!Gf.ValidateTitleDetails(BookReference.Text, "Book Reference Info"))
			{
				return false;
			}
			if (!Gf.ValidateTitleDetails(WriterInfo.Text, "Writer Info"))
			{
				return false;
			}
			if (SongNumber.Text != "")
			{
				int num = DataUtil.StringToInt(SongNumber.Text, Minus1IfBlank: true);
				if (num < 0)
				{
					MessageBox.Show("Song Number must be numeric and without spaces");
					return false;
				}
			}
			if ((tbLyrics1.TextLength < 1) & (tbLyrics2.TextLength < 1))
			{
				ShowMessage(1, "There are no Lyrics to save!");
				return false;
			}
			return true;
		}

		private bool ValidateSequence()
		{
			for (int i = 0; i <= OrderList.Items.Count - 1; i++)
			{
				int num = DataUtil.StringToInt(OrderList.Items[i].SubItems[1].Text);
				if (!VersePresent[num])
				{
					if (num > 0 && num < 13)
					{
						ShowMessage(1, "Sequence contains Verse " + Gf.VerseTitle[num] + " which is not in the lyrics!");
					}
					else
					{
						ShowMessage(1, "Sequence contains a " + Gf.VerseTitle[num] + " which is not in the lyrics!");
					}
					i = OrderList.Items.Count - 1;
					return false;
				}
			}
			return true;
		}

		private bool ValidateContents()
		{
			if (!ValidateContents(ref tbLyrics1, 1))
			{
				return false;
			}
			if (!ValidateContents(ref tbLyrics2, 2))
			{
				return false;
			}
			return true;
		}

		private bool ValidateContents(ref RichTextBox InTextBox, int Region)
		{
			IgnoreChange = true;
			InTextBox.Text = InTextBox.Text.Replace("\r\n", "\n");
			if (Region == 1)
			{
				Gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			}
			else
			{
				Gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			}
			IgnoreChange = false;
			bool flag = true;
			bool flag2 = false;
			int num = 0;
			while (flag)
			{
				if (VersePresent[num])
				{
					int num2 = InTextBox.Text.IndexOf(Gf.VerseSymbol[num]);
					if (num2 >= 0 && num2 != 0)
					{
						if (DataUtil.Mid(InTextBox.Text, num2 - 1, 1) != "\n")
						{
							Gf.ClipboardPasteTextBox(InTextBox, num2 - 1, "\r\n\r\n");
							num2 = InTextBox.SelectionStart + 1;
						}
						num2 += Gf.VerseSymbol[num].Length;
						if (DataUtil.Mid(InTextBox.Text, num2, 1) != "\n")
						{
							Gf.ClipboardPasteTextBox(InTextBox, num2, "\r\n\r\n");
							num2 = InTextBox.SelectionStart + 1;
						}
					}
				}
				num++;
				if (num > 99 && num < 100)
				{
					num = 100;
				}
				if (num > 150)
				{
					flag = false;
				}
			}
			tbWorkspace.Text = InTextBox.Text.Replace("\r\n", "\n");
			int num3 = tbWorkspace.Text.IndexOf(Gf.VerseSymbol[150]);
			if (num3 >= 0)
			{
				ShowMessage(Region, "REGION 2 indicator is not permitted - please remove");
				InTextBox.Focus();
				InTextBox.SelectionStart = num3;
				InTextBox.SelectionLength = Gf.VerseSymbol[150].Length;
				return false;
			}
			flag = true;
			flag2 = false;
			num = 0;
			int num4 = -1;
			int num5 = -1;
			int num6 = 30000;
			int num7 = 0;
			while (num < 160 && flag)
			{
				if (VersePresent[num])
				{
					int num2 = tbWorkspace.Text.IndexOf(Gf.VerseSymbol[num]);
					if (num2 >= 0)
					{
						if (num2 < num6)
						{
							num6 = num2;
							num5 = num;
						}
						if (num >= 0 && num <= 112)
						{
							int num8 = tbWorkspace.Text.IndexOf(Gf.VerseSymbol[num], num2 + Gf.VerseSymbol[num].Length);
							if (num8 >= 0)
							{
								flag2 = true;
							}
							if (num > 1 && num < 100)
							{
								if (!VersePresent[num - 1] && num4 < 0)
								{
									num4 = num - 1;
								}
								if (num7 < 1)
								{
									num7 = 1;
								}
							}
							else
							{
								num7 = num;
							}
						}
					}
				}
				if (flag2)
				{
					flag = false;
					continue;
				}
				num++;
				if (num > 99 && num < 100)
				{
					num = 100;
				}
			}
			if (flag2)
			{
				int num2 = -1;
				int num8 = -1;
				for (int i = 0; i <= tbWorkspace.TextLength; i++)
				{
					tbWorkspace.SelectionStart = i;
					tbWorkspace.SelectionLength = Gf.VerseSymbol[num].Length;
					if (tbWorkspace.SelectedText == Gf.VerseSymbol[num])
					{
						if (num2 < 0)
						{
							num2 = i;
							continue;
						}
						num8 = i;
						i = tbWorkspace.TextLength;
					}
				}
				if (num > 0 && num < 13)
				{
					ShowMessage(Region, "Duplicate Verse " + Gf.VerseTitle[num] + " indicator found - please amend.");
				}
				else
				{
					ShowMessage(Region, "Duplicate " + Gf.VerseTitle[num] + " indicator found - please amend.");
				}
				InTextBox.Focus();
				InTextBox.SelectionStart = num8;
				InTextBox.SelectionLength = Gf.VerseSymbol[num].Length;
				return false;
			}
			if (num4 > 0)
			{
				ShowMessage(Region, "Verse " + num4 + " indicator is missing.");
				return false;
			}
			if (num7 > 0 && num6 != 0)
			{
				if (num5 > 0 && num5 < 13)
				{
					ShowMessage(Region, "Indicator is required for the lyrics before Verse " + num5);
				}
				else
				{
					ShowMessage(Region, "Indicator is required for the lyrics before the " + Gf.VerseTitle[num5]);
				}
				return false;
			}
			ClearErrorMessage(Region);
			return true;
		}

		private void UpdateVersesList()
		{
			if (InitLoad)
			{
				return;
			}
			ListViewItem listViewItem = new ListViewItem();
			wArray = "";
			VerseSymbolChanged = false;
			bool flag = false;
			int num = -1;
			for (int i = 0; i < 160; i++)
			{
				if (!((i <= 99) | (i >= 100 && i <= 112) | (i >= 150 && i < 152)))
				{
					continue;
				}
				num = tbLyrics1.Text.IndexOf(Gf.VerseSymbol[i], 0);
				if (Gf.VerseSymbol[i] != "" && num >= 0)
				{
					VersePresent[i] = true;
					VersePresentNewScreenCount[i] = CountVerseScreens(tbLyrics1.Text, num + 1);
					flag = true;
				}
				else
				{
					num = tbLyrics2.Text.IndexOf(Gf.VerseSymbol[i], 0);
					if (Gf.VerseSymbol[i] != "" && num >= 0)
					{
						VersePresent[i] = true;
						VersePresentNewScreenCount[i] = CountVerseScreens(tbLyrics2.Text, num + 1);
						flag = true;
					}
					else
					{
						VersePresent[i] = false;
						VersePresentNewScreenCount[i] = 0;
					}
				}
				if (VersePresent[i] != prevVersePresent[i])
				{
					VerseSymbolChanged = true;
				}
				wArray += (VersePresent[i] ? ("," + Gf.VerseSymbol[i]) : "");
			}
			if (wArray != "")
			{
				wArray = DataUtil.Mid(wArray, 1);
			}
			sArray = wArray.Split(',');
			if (VerseSymbolChanged)
			{
				VersesList.Items.Clear();
				for (int i = 1; i <= 99; i++)
				{
					if (VersePresent[i])
					{
						listViewItem = VersesList.Items.Add(Gf.VerseTitle[i]);
						listViewItem.SubItems.Add(i.ToString());
						listViewItem.SubItems.Add(VersePresentNewScreenCount[i].ToString());
					}
					prevVersePresent[i] = VersePresent[i];
				}
				if (VersePresent[0])
				{
					listViewItem = VersesList.Items.Add(Gf.VerseTitle[0]);
					listViewItem.SubItems.Add(0.ToString());
					listViewItem.SubItems.Add(VersePresentNewScreenCount[0].ToString());
				}
				prevVersePresent[0] = VersePresent[0];
				for (int i = 100; i <= 112; i++)
				{
					if (VersePresent[i])
					{
						listViewItem = VersesList.Items.Add(Gf.VerseTitle[i]);
						listViewItem.SubItems.Add(i.ToString());
						listViewItem.SubItems.Add(VersePresentNewScreenCount[0].ToString());
					}
					prevVersePresent[i] = VersePresent[i];
				}
			}
			if ((VersesList.Items.Count == 0 && (tbLyrics1.Text != "" || tbLyrics2.Text != "")) || (VersesList.Items.Count > 0 && (!VersePresent[1] || tbLyrics1.Text == "" || tbLyrics1.Text[0] != '[')))
			{
				VersePresentNewScreenCount[1] = CountVerseScreens(tbLyrics1.Text, 0);
				if (VersePresentNewScreenCount[1] < 1)
				{
					VersePresentNewScreenCount[1] = 1;
				}
				VersePresent[1] = true;
				listViewItem = VersesList.Items.Add(Gf.VerseTitle[1]);
				listViewItem.SubItems.Add("1");
				listViewItem.SubItems.Add(VersePresentNewScreenCount[1].ToString());
				prevVersePresent[1] = VersePresent[1];
				flag = true;
			}
			CopyVerseListToRotateVerseList();
			if (flag || VerseSymbolChanged)
			{
				UpdateRotateTimePositions();
			}
		}

		private int CountVerseScreens(string InText, int StartIndex)
		{
			int num = InText.IndexOf('[', StartIndex);
			if (num < 0)
			{
				num = InText.Length;
			}
			int num2 = 1;
			int num3 = InText.IndexOf("\n\n", StartIndex);
			if (num3 >= 0)
			{
				while (num3 >= 0 && num3 < num)
				{
					num2++;
					num3 = ((num3 + 2 >= InText.Length) ? (-1) : InText.IndexOf("\n\n", num3 + 2));
				}
			}
			return num2;
		}

		private void tbLyrics_SelectionChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_SelectionChanged(1);
			}
		}

		private void tbLyrics2_SelectionChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_SelectionChanged(2);
			}
		}

		private void Lyrics_SelectionChanged(int Region)
		{
			StackTrackPos[Region, 0] = StackTrackPos[Region, 1];
			StackTrackPos[Region, 1] = ((Region == 1) ? tbLyrics1.SelectionStart : tbLyrics2.SelectionStart);
		}

		private void FrmEditItem_Resize(object sender, EventArgs e)
		{
			SetTopPanel();
			groupBox2.Left = base.Width - groupBox2.Width - 11;
			int num = groupBox2.Left - groupBox1.Left - 3;
			num = ((num > 0) ? num : 0);
			groupBox1.Width = num;
			if (groupBox1.Width < 443)
			{
				panel7.Width = 216;
			}
			else
			{
				panel7.Width = groupBox1.Width - 227;
			}
			panel8.Left = panel7.Left + panel7.Width;
			ResizeTitleFields();
			SetRotatePanel();
		}

		private void ResizeTitleFields()
		{
			SongTitle.Width = panel8.Left - SongTitle.Left - 7;
			WriterInfo.Width = SongTitle.Width;
			CopyrightInfo.Width = SongTitle.Width;
			SongTitle2.Width = SongTitle.Width - (panelLinkTitle2Lookup.Width + LinkTitle2Pic.Width) - 3;
			LinkTitle2Pic.Left = SongTitle.Left + SongTitle2.Width + 3;
			panelLinkTitle2Lookup.Left = LinkTitle2Pic.Left + LinkTitle2Pic.Width;
			Btn_Title.Left = SongTitle.Left + SongTitle.Width - Btn_Title.Width;
			Btn_Title2.Left = SongTitle2.Left + SongTitle2.Width - Btn_Title2.Width;
			Btn_Writer.Left = WriterInfo.Left + WriterInfo.Width - Btn_Writer.Width;
			Btn_Copyright.Left = CopyrightInfo.Left + CopyrightInfo.Width - Btn_Copyright.Width;
			UserReference.Width = (Btn_UserRef.Visible ? (162 - Btn_UserRef.Width) : 162);
			BookReference.Width = (Btn_BookRef.Visible ? (162 - Btn_BookRef.Width) : 162);
			SongTitle.Width -= (Btn_Title.Visible ? Btn_Title.Width : 0);
			SongTitle2.Width -= (Btn_Title2.Visible ? Btn_Title2.Width : 0);
			WriterInfo.Width -= (Btn_Writer.Visible ? Btn_Writer.Width : 0);
			CopyrightInfo.Width -= (Btn_Copyright.Visible ? Btn_Copyright.Width : 0);
		}

		private void ComboFonts_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyFonts();
		}

		private void tbLyrics1_TextChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_TextChanged(1);
			}
		}

		private void tbLyrics2_TextChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_TextChanged(2);
			}
		}

		private void Lyrics_TextChanged(int Region)
		{
			IgnoreChange = true;
			if ((tbLyrics1.Text != "") | (tbLyrics2.Text != ""))
			{
				if (ComboFontName.Enabled)
				{
					EnableFontNameSize(EnableState: false);
				}
			}
			else
			{
				EnableFontNameSize(EnableState: true);
			}
			ClearErrorMessage(Region);
			UpdateVersesList();
			IgnoreChange = false;
			if (!InsertingPresetItem)
			{
				rtfMain_Change(Region);
			}
			IgnoreChange = true;
			if (Region == 1)
			{
				Gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: false, StackTrackPos[Region, 1], StackTrackPos[Region, 1], sArray, MainFont, NotationFont, DoNotations: true);
				tbLyrics1.Focus();
			}
			else
			{
				Gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: false, StackTrackPos[Region, 1], StackTrackPos[Region, 1], sArray, MainFont, NotationFont, DoNotations: true);
				tbLyrics2.Focus();
			}
			IgnoreChange = false;
		}

		private void SongFolder_SelectedIndexChanged(object sender, EventArgs e)
		{
			int folderNumber = Gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString());
			try
			{
				SetRightToLeft1 = ((Gf.ShowFontRTL[folderNumber, 0] > 0) ? true : false);
				SetRightToLeft2 = ((Gf.ShowFontRTL[folderNumber, 1] > 0) ? true : false);
			}
			catch
			{
				SetRightToLeft1 = false;
				SetRightToLeft2 = false;
			}
			SetRightToLeftAtRegion(1, SetRightToLeft1);
			SetRightToLeftAtRegion(2, SetRightToLeft2);
			ClearErrorMessage(0);
		}

		private void Title_TextChanged(object sender, EventArgs e)
		{
			ClearErrorMessage(0);
		}

		private void FrmEditItem_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!FormCanClose)
			{
				ClearErrorMessage(0);
				Save_FormPos_To_Registry();
				if (ActionBeforeNextEvent() == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
				Gf.EditorFormOpen = false;
			}
			try
			{
				DShowPlayer.TidyUp();
				TimerEditRequest.Stop();
				ApplyPlayControls(ControlsBtn.Closebtn);
				TimerTrack.Stop();
				TimerFast.Stop();
			}
			catch
			{
			}
		}

		private void splitContainerMain_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!SplitterReAdjust)
			{
				SetTopPanel();
				if (splitContainerMain.SplitterDistance < 130)
				{
					SplitterReAdjust = true;
					splitContainerMain.SplitterDistance = 130;
					SplitterReAdjust = false;
				}
				groupBox2.Height = splitContainerMain.Panel1.Height - 1;
				panelVerses.Height = groupBox2.Height - 15;
				panelOrderList.Height = panelVerses.Height;
			}
		}

		private void SetTopPanel()
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				splitContainerMain.FixedPanel = FixedPanel.Panel1;
			}
			else if (base.WindowState == FormWindowState.Maximized)
			{
				splitContainerMain.FixedPanel = FixedPanel.None;
			}
		}

		private void Menu_EditHistory_Click(object sender, EventArgs e)
		{
			try
			{
				ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
				int num = DataUtil.ObjToInt(toolStripMenuItem.Tag) + 1;
				if (ActionBeforeNextEvent() != DialogResult.Cancel)
				{
					Gf.EditorItemID = DataUtil.StringToInt(DataUtil.Mid(Gf.EditorEditHistoryList[num, 0], 1));
					LoadSong(Gf.EditorItemID);
					AddToEditHistory("D" + Gf.EditorItemID);
					Gf.EditorLoadItem = true;
				}
			}
			catch
			{
			}
		}

		private void R1Chinese_Click(object sender, EventArgs e)
		{
			SwitchChinese(1);
		}

		private void R2Chinese_Click(object sender, EventArgs e)
		{
			SwitchChinese(2);
		}

		private void R1UndoRedo_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			switch (DataUtil.ObjToInt(button.Tag))
			{
			case 0:
				cmdUndo_Click(1);
				break;
			case 1:
				cmdRedo_Click(1);
				break;
			}
		}

		private void R2UndoRedo_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			switch (DataUtil.ObjToInt(button.Tag))
			{
			case 0:
				cmdUndo_Click(2);
				break;
			case 1:
				cmdRedo_Click(2);
				break;
			}
		}

		private void ClearStack(int Region)
		{
			if ((Region == 0) | (Region == 1))
			{
				StackIndex[1] = -1;
				StackMaxRedo[1] = -1;
				StackStartPoint[1] = 0;
				rtfMain_Change(1);
			}
			if ((Region == 0) | (Region == 2))
			{
				StackIndex[2] = -1;
				StackMaxRedo[2] = -1;
				StackStartPoint[2] = 0;
				rtfMain_Change(2);
			}
		}

		private void cmdUndo_Click(int Region)
		{
			if (StackIndex[Region] != StackStartPoint[Region])
			{
				IgnoreChange = true;
				StackIndex[Region]--;
				if (StackIndex[Region] >= 0)
				{
					switch (Region)
					{
					case 1:
						AssignStackToLyrics(ref tbLyrics1, Region);
						break;
					case 2:
						AssignStackToLyrics(ref tbLyrics2, Region);
						break;
					}
				}
				else if (StackStartPoint[Region] > 0)
				{
					StackIndex[Region] = StackArrayMaxPoint;
				}
				IgnoreChange = false;
			}
			if (Region == 1)
			{
				tbLyrics1.Focus();
			}
			else
			{
				tbLyrics2.Focus();
			}
		}

		private void cmdRedo_Click(int Region)
		{
			if (StackIndex[Region] != StackMaxRedo[Region])
			{
				IgnoreChange = true;
				StackIndex[Region]++;
				if (StackIndex[Region] > StackArrayMaxPoint)
				{
					StackIndex[Region] = 0;
				}
				switch (Region)
				{
				case 1:
					AssignStackToLyrics(ref tbLyrics1, Region);
					break;
				case 2:
					AssignStackToLyrics(ref tbLyrics2, Region);
					break;
				}
				IgnoreChange = false;
			}
			if (Region == 1)
			{
				tbLyrics1.Focus();
			}
			else
			{
				tbLyrics2.Focus();
			}
		}

		private void AssignStackToLyrics(ref RichTextBox InLyrics, int Region)
		{
			InLyrics.Rtf = sStack[Region, StackIndex[Region]];
			InLyrics.SelectionStart = iCursorPosition[Region, StackIndex[Region]];
			UpdateVersesList();
		}

		private void Verses_Add_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			if (toolStripButton.Name == "Verses_Add")
			{
				AddBtn_Click(ref VersesList, ref OrderList, ref OrderListSequence);
			}
			else
			{
				SmartAddBtn_Click(ref OrderList, ref OrderListSequence);
			}
		}

		private void AddBtn_Click(ref ListView InVersesList, ref ListView InOrderList, ref string InSequence)
		{
			ListViewItem listViewItem = new ListViewItem();
			ClearErrorMessage(0);
			for (int i = 0; i <= InVersesList.Items.Count - 1; i++)
			{
				if (InVersesList.Items[i].Selected)
				{
					listViewItem = InOrderList.Items.Add(InVersesList.Items[i].Text);
					listViewItem.SubItems.Add(InVersesList.Items[i].SubItems[1].Text);
				}
			}
			UpdateSequence(ref InOrderList, ref InSequence);
		}

		private void UpdateSequence(ref ListView InOrderList, ref string InSequence)
		{
			InSequence = "";
			if (InOrderList.Items.Count > 0)
			{
				for (int i = 0; i < InOrderList.Items.Count; i++)
				{
					InSequence += (char)DataUtil.StringToInt(InOrderList.Items[i].SubItems[1].Text);
				}
			}
			if (InOrderList.Name == "Rotate_OrderList")
			{
				UpdateRotateTimePositions();
			}
		}

		private void SmartAddBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ListViewItem listViewItem = new ListViewItem();
			ClearErrorMessage(0);
			InOrderList.Items.Clear();
			tbWorkspace.Text = tbLyrics1.Text;
			int num = tbWorkspace.Text.IndexOf("[");
			if (tbWorkspace.Text.IndexOf(Gf.VerseSymbol[0]) == num && num >= 0)
			{
				listViewItem = InOrderList.Items.Add(Gf.VerseTitle[0]);
				listViewItem.SubItems.Add(0.ToString());
			}
			for (int i = 1; i < 99; i++)
			{
				if (!VersePresent[i])
				{
					continue;
				}
				listViewItem = InOrderList.Items.Add(Gf.VerseTitle[i]);
				listViewItem.SubItems.Add(i.ToString());
				if (VersePresent[111])
				{
					if (!VersePresent[i + 1] & VersePresent[112])
					{
						listViewItem = InOrderList.Items.Add(Gf.VerseTitle[112]);
						listViewItem.SubItems.Add(112.ToString());
					}
					else
					{
						listViewItem = InOrderList.Items.Add(Gf.VerseTitle[111]);
						listViewItem.SubItems.Add(111.ToString());
					}
				}
				if (VersePresent[0])
				{
					if (!VersePresent[i + 1] & VersePresent[102])
					{
						listViewItem = InOrderList.Items.Add(Gf.VerseTitle[102]);
						listViewItem.SubItems.Add(102.ToString());
					}
					else
					{
						listViewItem = InOrderList.Items.Add(Gf.VerseTitle[0]);
						listViewItem.SubItems.Add(0.ToString());
					}
				}
				if ((i == 1) & VersePresent[100])
				{
					listViewItem = InOrderList.Items.Add(Gf.VerseTitle[100]);
					listViewItem.SubItems.Add(100.ToString());
				}
			}
			if (VersePresent[101])
			{
				listViewItem = InOrderList.Items.Add(Gf.VerseTitle[101]);
				listViewItem.SubItems.Add(101.ToString());
			}
			UpdateSequence(ref InOrderList, ref InSequence);
		}

		private void OrderList_Btn_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			if (toolStripButton.Name == "OrderList_Up")
			{
				MoveUPBtn_Click(ref OrderList, ref OrderListSequence);
			}
			else if (toolStripButton.Name == "OrderList_Down")
			{
				MoveDownBtn_Click(ref OrderList, ref OrderListSequence);
			}
			else
			{
				DelBtn_Click(ref OrderList, ref OrderListSequence);
			}
		}

		private void MoveUPBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ClearErrorMessage(0);
			int count = InOrderList.Items.Count;
			if (count < 1)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (InOrderList.Items[i].Selected)
				{
					if (num < 1)
					{
						num = i;
						continue;
					}
					i = count;
					num = 0;
				}
			}
			if (num >= 1)
			{
				string text = InOrderList.Items[num].Text;
				InOrderList.Items[num].Text = InOrderList.Items[num - 1].Text;
				InOrderList.Items[num - 1].Text = text;
				text = InOrderList.Items[num].SubItems[1].Text;
				InOrderList.Items[num].SubItems[1].Text = InOrderList.Items[num - 1].SubItems[1].Text;
				InOrderList.Items[num - 1].SubItems[1].Text = text;
				InOrderList.Items[num].Selected = false;
				InOrderList.Items[num - 1].Selected = true;
				UpdateSequence(ref InOrderList, ref InSequence);
			}
		}

		private void MoveDownBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ClearErrorMessage(0);
			int count = InOrderList.Items.Count;
			if (count <= 1)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i <= count - 1; i++)
			{
				if (InOrderList.Items[i].Selected)
				{
					if (num < 1)
					{
						num = i;
						continue;
					}
					i = count;
					num = -1;
				}
			}
			if (!((num < 0) | (num == count - 1)))
			{
				string text = InOrderList.Items[num].Text;
				InOrderList.Items[num].Text = InOrderList.Items[num + 1].Text;
				InOrderList.Items[num + 1].Text = text;
				text = InOrderList.Items[num].SubItems[1].Text;
				InOrderList.Items[num].SubItems[1].Text = InOrderList.Items[num + 1].SubItems[1].Text;
				InOrderList.Items[num + 1].SubItems[1].Text = text;
				InOrderList.Items[num].Selected = false;
				InOrderList.Items[num + 1].Selected = true;
				UpdateSequence(ref InOrderList, ref InSequence);
			}
		}

		private void DelBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ClearErrorMessage(0);
			if (InOrderList.Items.Count == 0)
			{
				return;
			}
			int num = 0;
			for (int num2 = InOrderList.Items.Count - 1; num2 >= 0; num2--)
			{
				if (InOrderList.Items[num2].Selected)
				{
					InOrderList.Items[num2].Remove();
					num = num2;
				}
			}
			if (num > 0)
			{
				num--;
			}
			if (InOrderList.Items.Count > 0)
			{
				InOrderList.Items[num].Selected = true;
			}
			UpdateSequence(ref InOrderList, ref InSequence);
		}

		private void OrderList_KeyUp(object sender, KeyEventArgs e)
		{
			Action_OrderList_KeyUpEvent(ref OrderList, e, ref OrderListSequence);
		}

		private void Action_OrderList_KeyUpEvent(ref ListView InOrderList, KeyEventArgs e, ref string InSequence)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				for (int i = 0; i <= InOrderList.Items.Count - 1; i++)
				{
					InOrderList.Items[i].Selected = true;
				}
			}
			else if (e.Control && e.KeyCode == Keys.C)
			{
				tempSequenceCopied.Items.Clear();
				ListViewItem listViewItem = new ListViewItem();
				for (int i = 0; i <= InOrderList.Items.Count - 1; i++)
				{
					if (InOrderList.Items[i].Selected)
					{
						listViewItem = tempSequenceCopied.Items.Add(InOrderList.Items[i].Text);
						listViewItem.SubItems.Add(InOrderList.Items[i].SubItems[1].Text);
					}
				}
			}
			else if (e.Control && e.KeyCode == Keys.V)
			{
				ListViewItem listViewItem = new ListViewItem();
				for (int i = 0; i <= tempSequenceCopied.Items.Count - 1; i++)
				{
					listViewItem = InOrderList.Items.Add(tempSequenceCopied.Items[i].Text);
					listViewItem.SubItems.Add(tempSequenceCopied.Items[i].SubItems[1].Text);
				}
				UpdateSequence(ref InOrderList, ref InSequence);
			}
			else if (e.KeyCode == Keys.Delete)
			{
				DelBtn_Click(ref InOrderList, ref InSequence);
			}
		}

		private void VersesList_DoubleClick(object sender, EventArgs e)
		{
			if (VersesList.SelectedItems.Count > 0)
			{
				AddBtn_Click(ref VersesList, ref OrderList, ref OrderListSequence);
			}
		}

		private void SongTitle2_TextChanged(object sender, EventArgs e)
		{
			SongTitle2_Change();
		}

		private void R1VerseFormat_Click(object sender, EventArgs e)
		{
			Gf.FormatPlainLyrics(ref tbLyrics1);
		}

		private void R2VerseFormat_Click(object sender, EventArgs e)
		{
			Gf.FormatPlainLyrics(ref tbLyrics2);
		}

		private void TimerEditRequest_Tick(object sender, EventArgs e)
		{
			if (Gf.Edit_RequestReceived)
			{
				Gf.Edit_RequestReceived = false;
				ClearErrorMessage(0);
				if (base.WindowState == FormWindowState.Minimized)
				{
					base.WindowState = FormWindowState.Normal;
				}
				base.TopMost = true;
				Focus();
				base.TopMost = false;
				if (Gf.DB_CurSongID > 0)
				{
					if (ActionBeforeNextEvent() == DialogResult.Cancel)
					{
						return;
					}
					LoadSong(Gf.DB_CurSongID);
					AddToEditHistory("D" + Gf.DB_CurSongID);
				}
				else
				{
					NewItem();
				}
			}
			if (Gf.Edit_HistoryMaxChanged)
			{
				Gf.SaveEditorEditHistoryToRegistry();
				UpdateMenu_EditHistory();
			}
		}

		private void Title2_LookUp_Click(object sender, EventArgs e)
		{
			if (SongTitle2.Text == "")
			{
				Gf.Lookup_NameSelected = "*";
			}
			else
			{
				Gf.Lookup_NameSelected = "*" + DataUtil.Trim(SongTitle2.Text) + "*";
			}
			FrmLookupTitles frmLookupTitles = new FrmLookupTitles();
			if (frmLookupTitles.ShowDialog() == DialogResult.OK && Gf.Lookup_NameSelected != "")
			{
				SongTitle2.Text = Gf.Lookup_NameSelected;
				if (Gf.Lookup_NameBookRef != "")
				{
					Gf.UpdateRefString(Gf.Lookup_NameBookRef, ",", ref BookReference, ",");
				}
				if (Gf.Lookup_NameUserRef != "")
				{
					Gf.UpdateRefString(Gf.Lookup_NameUserRef, ",", ref UserReference, ",");
				}
			}
		}

		private void SyncBtnUpDown_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			int direction = DataUtil.ObjToInt(button.Tag);
			ScrollBothLyrics(direction);
		}

		private void ScrollBothLyrics(int Direction)
		{
			int num = tbLyrics1.SelectionStart;
			int selectionLength = tbLyrics1.SelectionLength;
			int NewPosition = 0;
			int NewPositionLength = selectionLength;
			if (!ScreenBreak1Available)
			{
				Gf.MapLyricsBreak(ref ScreenBreaks1, ref tbLyrics1, ref ScreenBreak1Available);
			}
			if (!ScreenBreak2Available)
			{
				Gf.MapLyricsBreak(ref ScreenBreaks2, ref tbLyrics2, ref ScreenBreak2Available);
			}
			string LookupVerseSym = "";
			int LookupScreenCount = 0;
			if (num == 0 && selectionLength == 0)
			{
				num = -1;
			}
			Gf.GetBreakPosition(ScreenBreaks1, num, Direction, ref NewPosition, ref NewPositionLength, ref LookupVerseSym, ref LookupScreenCount);
			int NewPosition2 = 0;
			int NewPositionLength2 = 0;
			Gf.GetBreakPosition(ScreenBreaks2, ref NewPosition2, ref NewPositionLength2, LookupVerseSym, LookupScreenCount);
			tbLyrics1.SelectionStart = NewPosition;
			tbLyrics1.SelectionLength = ((NewPositionLength >= 0) ? NewPositionLength : (tbLyrics1.Text.Length - NewPositionLength));
			tbLyrics1.ScrollToCaret();
			tbLyrics2.SelectionStart = NewPosition2;
			tbLyrics2.SelectionLength = ((NewPositionLength2 >= 0) ? NewPositionLength2 : (tbLyrics2.Text.Length - NewPositionLength2));
			tbLyrics2.ScrollToCaret();
			tbLyrics1.Focus();
		}

		private void TextBox_Leave(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			TextBoxBtnMovement(textBox.Name);
		}

		private void SetTextBoxInvisible()
		{
			Btn_Title.Visible = false;
			Btn_Title2.Visible = false;
			Btn_Writer.Visible = false;
			Btn_Copyright.Visible = false;
			Btn_BookRef.Visible = false;
			Btn_UserRef.Visible = false;
		}

		private void TextBox_Enter(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			TextBoxBtnMovement(textBox.Name);
		}

		private void Btn_Enter(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			TextBoxBtnMovement(button.Name);
		}

		private void TextBoxBtnMovement(string InName)
		{
			SetTextBoxInvisible();
			switch (InName)
			{
			case "SongTitle":
			case "Btn_Title":
				Btn_Title.Visible = true;
				break;
			case "SongTitle2":
			case "Btn_Title2":
				Btn_Title2.Visible = true;
				break;
			case "WriterInfo":
			case "Btn_Writer":
				Btn_Writer.Visible = true;
				break;
			case "CopyrightInfo":
			case "Btn_Copyright":
				Btn_Copyright.Visible = true;
				break;
			case "BookReference":
			case "Btn_BookRef":
				Btn_BookRef.Visible = true;
				break;
			case "UserReference":
			case "Btn_UserRef":
				Btn_UserRef.Visible = true;
				break;
			}
			ResizeTitleFields();
		}

		private void tbLyrics1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.Z)
			{
				cmdUndo_Click(1);
			}
			else if (e.Control && e.KeyCode == Keys.Y)
			{
				cmdRedo_Click(1);
			}
		}

		private void tbLyrics1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F8)
			{
				InsertingPresetItem = true;
				Gf.InsertIndicator(ref tbLyrics1, 152);
				InsertingPresetItem = false;
				Lyrics_TextChanged(1);
			}
		}

		private void tbLyrics2_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.Z)
			{
				cmdUndo_Click(2);
			}
			else if (e.Control && e.KeyCode == Keys.Y)
			{
				cmdRedo_Click(2);
			}
		}

		private void tbLyrics2_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F8)
			{
				InsertingPresetItem = true;
				Gf.InsertIndicator(ref tbLyrics2, 152);
				InsertingPresetItem = false;
				Lyrics_TextChanged(2);
			}
		}

		private void splitContainerRotate_SplitterMoved(object sender, SplitterEventArgs e)
		{
			SetRotatePanel();
			if (splitContainerRotate.SplitterDistance < 112)
			{
				splitContainerRotate.SplitterDistance = 112;
			}
			groupBoxRotateVerses.Height = splitContainerRotate.Panel1.Height - 2;
			panelRotate_Verses.Height = groupBoxRotateVerses.Height - 15;
			panelRotate_OrderList.Height = panelRotate_Verses.Height;
		}

		private void SetRotatePanel()
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				splitContainerRotate.FixedPanel = FixedPanel.Panel1;
			}
			else if (base.WindowState == FormWindowState.Maximized)
			{
				splitContainerRotate.FixedPanel = FixedPanel.None;
			}
		}

		private void UpdateRotateTimePositions()
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, 0);
			UpdateRotateTimePositions(InTotalRotateTime: (int)Rotate_TimeTotal.Value.Subtract(InitDateTime).TotalSeconds, InStyle: (!Rotate_Multiple.Checked) ? 1 : 2, InGapRotateTime: (int)Rotate_SlidesGapUpDown.Value, InRotateTimings: "", UseRotateTimings: false, ResetAll: false);
		}

		private void UpdateRotateTimePositions(int InStyle, int InGapRotateTime, int InTotalRotateTime, string InRotateTimings, bool UseRotateTimings, bool ResetAll)
		{
			flowLayoutRotate.Controls.Clear();
			if (InStyle == 2)
			{
				Rotate_Multiple.Checked = true;
			}
			else
			{
				Rotate_Equal.Checked = true;
			}
			Rotate_SlidesGapUpDown.Value = InGapRotateTime;
			Rotate_TimeTotal.Value = InitDateTime.Add(new TimeSpan(0, 0, InTotalRotateTime));
			RotateTotalScreensIndex = -1;
			int num = -1;
			string text = "";
			for (int i = 0; i < Rotate_OrderList.Items.Count; i++)
			{
				num = VersePresentNewScreenCount[DataUtil.StringToInt(Rotate_OrderList.Items[i].SubItems[1].Text)];
				for (int j = 0; j < num; j++)
				{
					RotateTotalScreensIndex++;
					if (RotateTimeLabel[RotateTotalScreensIndex] == null)
					{
						RotateTimeLabel[RotateTotalScreensIndex] = new Label();
						RotateTimeLabel[RotateTotalScreensIndex].AutoSize = false;
						RotateTimeLabel[RotateTotalScreensIndex].Width = 48;
						RotateTimeLabel[RotateTotalScreensIndex].TextAlign = ContentAlignment.MiddleLeft;
					}
					if (RotateTimePosition[RotateTotalScreensIndex] == null)
					{
						RotateTimePosition[RotateTotalScreensIndex] = new DateTimePicker();
						RotateTimePosition[RotateTotalScreensIndex].Width = 52;
						RotateTimePosition[RotateTotalScreensIndex].ShowUpDown = true;
						RotateTimePosition[RotateTotalScreensIndex].MinDate = InitDateTime;
						RotateTimePosition[RotateTotalScreensIndex].CustomFormat = "mm:ss";
						RotateTimePosition[RotateTotalScreensIndex].Format = DateTimePickerFormat.Custom;
						RotateTimePosition[RotateTotalScreensIndex].Value = InitDateTime;
						if (i == 0 && j == 0)
						{
							RotateTimePosition[RotateTotalScreensIndex].Enabled = false;
						}
					}
					RotateTimeLabel[RotateTotalScreensIndex].Text = ((j < 1) ? Rotate_OrderList.Items[i].Text : " - ");
					RotateTimePosition[RotateTotalScreensIndex].Tag = ((j < 1) ? Rotate_OrderList.Items[i].SubItems[1].Text : 151.ToString());
					if (UseRotateTimings && RotateTotalScreensIndex > 0)
					{
						text = DataUtil.ExtractOneInfo(ref InRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false);
						RotateTimePosition[RotateTotalScreensIndex].Value = InitDateTime.Add(new TimeSpan(0, 0, DataUtil.StringToInt(text)));
					}
					flowLayoutRotate.Controls.Add(RotateTimeLabel[RotateTotalScreensIndex]);
					flowLayoutRotate.Controls.Add(RotateTimePosition[RotateTotalScreensIndex]);
				}
			}
			if (!ResetAll)
			{
				return;
			}
			for (int i = 0; i < RotateTimePosition.Length; i++)
			{
				if (RotateTimePosition[i] != null)
				{
					RotateTimePosition[i].Value = InitDateTime;
				}
			}
		}

		private void InitMediaPlayer()
		{
			if (Gf.WMP_Present)
			{
				try
				{
					DShowPlayer.Parent = this;
					DShowPlayer.Parent = panelRotate_Media;
					DShowPlayer.Location = new System.Drawing.Point(0, 0);
					DShowPlayer.SetDefaultSize(0, 0, panelRotate_Media.Width, panelRotate_Media.Height, (VAlign)Gf.VideoVAlign);
					DShowPlayer.ForeColorChanged += DShowPlayer_ForeColorChanged;
					PlayerOK = true;
				}
				catch
				{
					PlayerOK = false;
				}
			}
			if (PlayerOK)
			{
				DShowPlayer.Dock = DockStyle.Fill;
				DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
				panelNoPlayer.Visible = false;
				EnableMediaControls(MediaOn: true);
				DShowPlayer.Visible = true;
			}
			else
			{
				EnableMediaControls(MediaOn: false);
				panelNoPlayer.Dock = DockStyle.Fill;
				panelNoPlayer.Visible = true;
			}
		}

		private void DShowPlayer_ForeColorChanged(object sender, EventArgs e)
		{
			switch (DShowPlayer.currentState)
			{
			case PlayState.Running:
				PlayPauseBtn.Enabled = true;
				StopBtn.Enabled = true;
				PlayPauseBtn.Text = "Pause";
				LabelMediaType.Text = DShowPlayer.GetStatusText();
				Cursor = Cursors.Default;
				break;
			case PlayState.Paused:
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			case PlayState.Stopped:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			default:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				break;
			}
		}

		private void EnableMediaControls(bool MediaOn)
		{
			bool enabled = (MediaOn & PlayerOK) ? true : false;
			btnDuration.Enabled = enabled;
			btnAddPosition.Enabled = enabled;
			panelPlayBtns.Enabled = enabled;
			TrackBarVolume.Enabled = enabled;
			labelVol.Enabled = enabled;
			labelPos.Enabled = enabled;
			LabelDuration.Enabled = enabled;
			LabelPosition.Enabled = enabled;
			LabelMediaType.Enabled = enabled;
			labelMed.Enabled = enabled;
		}

		private void Rotate_LocationBtn_Click(object sender, EventArgs e)
		{
			Rotate_tbSourceLocation.Text = DataUtil.Trim(Rotate_tbSourceLocation.Text);
			string text = "";
			try
			{
				text = Path.GetDirectoryName(Rotate_tbSourceLocation.Text);
			}
			catch
			{
			}
			text = ((text != "") ? text : Gf.MediaDir);
			OpenFileDialog1.Filter = "All Files (*.*) | *.*";
			OpenFileDialog1.InitialDirectory = text;
			OpenFileDialog1.AddExtension = true;
			OpenFileDialog1.FileName = Rotate_tbSourceLocation.Text;
			bool flag = false;
			try
			{
				if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
				{
					ApplyPlayControls(ControlsBtn.Stopbtn);
					Rotate_tbSourceLocation.Text = OpenFileDialog1.FileName;
					if (PlayerOK)
					{
						DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
					}
					ApplySoundControls(ApplyMute: false);
				}
			}
			catch
			{
				flag = true;
			}
			if (flag)
			{
				try
				{
					OpenFileDialog1.FileName = "";
					if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
					{
						ApplyPlayControls(ControlsBtn.Stopbtn);
						Rotate_tbSourceLocation.Text = OpenFileDialog1.FileName;
						if (PlayerOK)
						{
							DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
						}
					}
				}
				catch
				{
				}
			}
		}

		private void ApplyPlayControls(ControlsBtn InAction)
		{
			if (!PlayerOK)
			{
				return;
			}
			TimerFast.Stop();
			switch (InAction)
			{
			case ControlsBtn.PlayPausebtn:
				if (DShowPlayer.currentState == PlayState.Running || DShowPlayer.currentState == PlayState.Paused)
				{
					DShowPlayer.PausePlayClip();
					break;
				}
				Rotate_tbSourceLocation.Text = DataUtil.Trim(Rotate_tbSourceLocation.Text);
				try
				{
					DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
					if (DShowPlayer.newFilename != "")
					{
						DShowPlayer.OpenClip();
						AttemptConnectCount = 0;
						LabelMediaType.Text = DShowPlayer.GetStatusText();
					}
					else
					{
						ResetMediaMessages();
					}
				}
				catch
				{
					DShowPlayer.newFilename = "";
					ResetMediaMessages();
				}
				return;
			case ControlsBtn.Stopbtn:
				DShowPlayer.StopClip();
				break;
			case ControlsBtn.FFbtn:
				ApplySoundControls(ApplyMute: true);
				IncrementCurrentPosition(1.0);
				TimeIncrement = 5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.FRbtn:
				ApplySoundControls(ApplyMute: true);
				IncrementCurrentPosition(-1.0);
				TimeIncrement = -5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.Closebtn:
				DShowPlayer.StopClip();
				break;
			}
			Cursor = Cursors.Default;
		}

		private void ResetMediaMessages()
		{
			if (PlayerOK)
			{
				LabelMediaType.Text = DShowPlayer.GetStatusText();
			}
			else
			{
				LabelMediaType.Text = "";
			}
			Cursor = Cursors.Default;
		}

		private void SetDurationSettings()
		{
			if (PlayerOK && DShowPlayer.GetClipDuration() > 0)
			{
				SetDurationSettings(ResetAll: false);
			}
			else
			{
				SetDurationSettings(ResetAll: true);
			}
		}

		private void SetDurationSettings(bool ResetAll)
		{
			if (ResetAll)
			{
				if (LabelMediaType.Text != "" && ((LabelMediaType.Text[0] == 'A') | (LabelMediaType.Text[0] == 'V')))
				{
					LabelDuration.Text = "Stream";
				}
				else
				{
					LabelDuration.Text = "00:00";
				}
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 0;
				TrackBarDuration.Value = 0;
			}
			else if (PlayerOK)
			{
				LabelDuration.Text = ((DShowPlayer.newFilename != "") ? DShowPlayer.GetClipDurationString() : "00:00");
				CurMediaPosition = DShowPlayer.GetCurrentPosition();
				CurMediaLength = DShowPlayer.GetClipDuration();
				LabelPosition.Text = DShowPlayer.GetCurrentPositionString();
				TrackBarDuration.Maximum = DShowPlayer.GetClipDuration();
				TrackBarDuration.Value = ((DShowPlayer.GetCurrentPosition() > TrackBarDuration.Maximum) ? TrackBarDuration.Maximum : DShowPlayer.GetCurrentPosition());
			}
			else
			{
				LabelDuration.Text = "00:00";
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 1000;
				TrackBarDuration.Value = 0;
			}
		}

		private void ApplySoundControls(bool ApplyMute)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetVolume(TrackBarVolume.Value);
				Gf.MediaVolume = TrackBarVolume.Value;
			}
		}

		private void PlayPauseBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.PlayPausebtn);
		}

		private void StopBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.Stopbtn);
		}

		private void FastReverseBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FRbtn);
		}

		private void FastReverseBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void FastForwardBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FFbtn);
		}

		private void FastForwardBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void ReturnToPreviousState()
		{
			TimerFast.Stop();
			ApplySoundControls(ApplyMute: false);
		}

		private void TimerTrack_Tick(object sender, EventArgs e)
		{
			SetDurationSettings();
		}

		private void TrackBarDuration_Scroll(object sender, EventArgs e)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition(TrackBarDuration.Value);
			}
		}

		private void TrackBarVolume_ValueChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void btnAddPosition_Click(object sender, EventArgs e)
		{
			if (RotateTotalScreensIndex <= 0)
			{
				return;
			}
			DateTime value = InitDateTime.Add(new TimeSpan(0, 0, (int)CurMediaPosition));
			int num = 1;
			while (true)
			{
				if (num < RotateTotalScreensIndex + 1)
				{
					if (RotateTimePosition[num].Value <= InitDateTime)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			RotateTimePosition[num].Value = value;
		}

		private void Rotate_Option_CheckedChanged(object sender, EventArgs e)
		{
			MediaOption_Changed();
		}

		private void MediaOption_Changed()
		{
			EnableMediaControls((Rotate_Multiple.Checked & PlayerOK) ? true : false);
			panelRotateLeftTop2.Enabled = Rotate_Multiple.Checked;
			flowLayoutRotate.Enabled = Rotate_Multiple.Checked;
			groupBoxRotateVerses.Enabled = Rotate_Multiple.Checked;
			Rotate_tbSourceLocation.Enabled = Rotate_Multiple.Checked;
			panelLoc.Enabled = Rotate_Multiple.Checked;
			if (Rotate_Multiple.Checked)
			{
				ApplyPlayControls(ControlsBtn.Closebtn);
			}
		}

		private void btnClearMediaPositions_Click(object sender, EventArgs e)
		{
			if (RotateTotalScreensIndex >= 0)
			{
				for (int i = 0; i <= RotateTotalScreensIndex; i++)
				{
					if (RotateTimePosition[i] != null)
					{
						RotateTimePosition[i].Value = InitDateTime;
					}
				}
			}
			Rotate_TimeTotal.Value = InitDateTime;
		}

		private void TimerFast_Tick(object sender, EventArgs e)
		{
			IncrementCurrentPosition(TimeIncrement);
		}

		private void IncrementCurrentPosition(double InIncrement)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition((double)DShowPlayer.GetCurrentPosition() + InIncrement);
				SetDurationSettings();
			}
		}

		private void panelRotate_Sample_Resize(object sender, EventArgs e)
		{
			Rotate_tbSourceLocation.Width = panelRotate_Sample.Width - Rotate_tbSourceLocation.Left - panelLoc.Width - 6;
			panelLoc.Left = Rotate_tbSourceLocation.Left + Rotate_tbSourceLocation.Width + 2;
		}

		private string GenerateRotateString()
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, 0);
			return GenerateRotateString(InTotalRotateTime: (int)Rotate_TimeTotal.Value.Subtract(InitDateTime).TotalSeconds, InStyle: Rotate_Equal.Checked ? 1 : 2, InGapRotateTime: (int)Rotate_SlidesGapUpDown.Value, InOrderList: Rotate_OrderList, InPosition: RotateTimePosition, InTotalScreens: RotateTotalScreensIndex + 1);
		}

		private string GenerateRotateString(int InStyle, int InGapRotateTime, int InTotalRotateTime, ListView InOrderList, DateTimePicker[] InPosition, int InTotalScreens)
		{
			if (InStyle == 1 && InGapRotateTime == 0 && InTotalRotateTime == 0 && InOrderList.Items.Count == 0)
			{
				return "";
			}
			string text = InStyle.ToString() + ';' + InGapRotateTime.ToString() + ';' + InTotalRotateTime.ToString() + ';';
			for (int i = 0; i < InOrderList.Items.Count; i++)
			{
				text = text + DataUtil.StringToInt(InOrderList.Items[i].SubItems[1].Text).ToString() + ';';
			}
			if (InTotalScreens > 0)
			{
				ListBox listBox = new ListBox();
				listBox.Sorted = false;
				TimeSpan timeSpan = new TimeSpan(0, 0, 0);
				for (int i = 0; i < InTotalScreens; i++)
				{
					if (InPosition[i] != null)
					{
						timeSpan = InPosition[i].Value.Subtract(InitDateTime);
						if ((int)timeSpan.TotalSeconds > 0)
						{
							listBox.Items.Add(timeSpan.TotalSeconds.ToString("00000"));
						}
					}
				}
				if (listBox.Items.Count > 0)
				{
					listBox.Sorted = true;
					string text3 = "";
					for (int i = 0; i < listBox.Items.Count; i++)
					{
						text3 = text3 + DataUtil.StringToInt(listBox.Items[i].ToString()).ToString() + ';';
					}
					text = text + "»" + text3;
				}
			}
			return text;
		}

		private void TimerAttemptConnect_Tick(object sender, EventArgs e)
		{
			if (PlayerOK)
			{
				if (AttemptConnectCount >= MaxAttemptConnectCount)
				{
					TimerAttemptConnect.Stop();
					LabelMediaType.Text = DShowPlayer.GetStatusText();
					DShowPlayer.StopClip();
					Cursor = Cursors.Default;
				}
				else
				{
					Cursor = Cursors.AppStarting;
					AttemptConnectCount++;
					LabelMediaType.Text = DShowPlayer.GetStatusText();
				}
			}
			else
			{
				TimerAttemptConnect.Stop();
				SetDurationSettings(ResetAll: true);
			}
		}

		private void CopyVerseListToRotateVerseList()
		{
			if (VersesList.Items.Count > 0)
			{
				ListViewItem listViewItem = new ListViewItem();
				Rotate_VersesList.Items.Clear();
				for (int i = 0; i < VersesList.Items.Count; i++)
				{
					listViewItem = (ListViewItem)VersesList.Items[i].Clone();
					Rotate_VersesList.Items.Add(listViewItem);
				}
			}
			else
			{
				Rotate_VersesList.Items.Clear();
			}
		}

		private void Rotate_Verses_Add_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string InSequence = "";
			if (toolStripButton.Name == "Rotate_Verses_Add")
			{
				AddBtn_Click(ref Rotate_VersesList, ref Rotate_OrderList, ref InSequence);
			}
			else
			{
				SmartAddBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
		}

		private void Rotate_VersesList_DoubleClick(object sender, EventArgs e)
		{
			string InSequence = "";
			if (Rotate_VersesList.SelectedItems.Count > 0)
			{
				AddBtn_Click(ref Rotate_VersesList, ref Rotate_OrderList, ref InSequence);
			}
		}

		private void Rotate_OrderList_Btn_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string InSequence = "";
			if (toolStripButton.Name == "Rotate_OrderList_Up")
			{
				MoveUPBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
			else if (toolStripButton.Name == "Rotate_OrderList_Down")
			{
				MoveDownBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
			else
			{
				DelBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
		}

		private void Rotate_OrderList_KeyUp(object sender, KeyEventArgs e)
		{
			string InSequence = "";
			Action_OrderList_KeyUpEvent(ref Rotate_OrderList, e, ref InSequence);
		}

		private void btnDuration_Click(object sender, EventArgs e)
		{
			DateTime value = InitDateTime.Add(new TimeSpan(0, 0, (int)CurMediaLength));
			Rotate_TimeTotal.Value = value;
		}

		private void CMRegion1_Copy_Click(object sender, EventArgs e)
		{
			Gf.ClipboardCopyTextBox(tbLyrics1);
		}

		private void CMRegion1_Paste_Click(object sender, EventArgs e)
		{
			Gf.ClipboardPasteTextBox(tbLyrics1, tbLyrics1.SelectionStart);
		}

		private void CMRegion2_Copy_Click(object sender, EventArgs e)
		{
			Gf.ClipboardCopyTextBox(tbLyrics2);
		}

		private void CMRegion2_Paste_Click(object sender, EventArgs e)
		{
			Gf.ClipboardPasteTextBox(tbLyrics2, tbLyrics2.SelectionStart);
		}

		private void BuildLyricsContextMenu(int InRegion)
		{
			if (InRegion == 1)
			{
				BuildContextMenuItems(ref CMRegion1, InRegion, SetChordsMenu);
			}
			else
			{
				BuildContextMenuItems(ref CMRegion2, InRegion, SetChordsMenu);
			}
		}

		private void BuildContextMenuItems(ref ContextMenuStrip InContextMenu, int InRegion, bool InSetChordsMenu)
		{
			InContextMenu.Items.Clear();
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Name = string.Concat(InContextMenu, "_Copy");
			toolStripMenuItem.Text = "Copy";
			InContextMenu.Items.Add(toolStripMenuItem);
			if (InRegion == 1)
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion1_Copy_Click).Invoke;
			}
			else
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion2_Copy_Click).Invoke;
			}
			toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Name = string.Concat(InContextMenu, "_Paste");
			toolStripMenuItem.Text = "Paste";
			InContextMenu.Items.Add(toolStripMenuItem);
			if (InRegion == 1)
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion1_Paste_Click).Invoke;
			}
			else
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion2_Paste_Click).Invoke;
			}
			if (!InSetChordsMenu)
			{
				return;
			}
			ToolStripSeparator toolStripSeparator = new ToolStripSeparator();
			toolStripSeparator = new ToolStripSeparator();
			InContextMenu.Items.Add(toolStripSeparator);
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Minor";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = Gf.MusicMinorChords[i, 0];
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = Gf.MusicMinorChords[i, 1];
					toolStripMenuItem.Tag = InRegion.ToString();
					toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Minor 7th";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = Gf.MusicMinorChords[i, 0] + "7";
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = Gf.MusicMinorChords[i, 1] + "7";
					toolStripMenuItem.Tag = InRegion.ToString();
					toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Major 7th";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = Gf.MusicMajorChords[i, 1] + "7";
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = Gf.MusicMajorChords[i, 0] + "7";
					toolStripMenuItem.Tag = InRegion.ToString();
					toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Bass";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = "/" + Gf.MusicMajorChords[i, 1];
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
			}
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = Gf.MusicMajorChords[i, 1];
				toolStripMenuItem.Tag = InRegion.ToString();
				InContextMenu.Items.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = Gf.MusicMajorChords[i, 0];
					toolStripMenuItem.Tag = InRegion.ToString();
					InContextMenu.Items.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
		}

		private void ContextMenuChords_Click(object sender, EventArgs e)
		{
			try
			{
				ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
				if (DataUtil.ObjToInt(toolStripMenuItem.Tag) == 1)
				{
					tbLyrics1.SelectionStart = tbLyrics1MouseUpPos;
					tbLyrics1.SelectionLength = 0;
					InsertingPresetItem = true;
					Gf.InsertChordAboveCurrentLine(ref tbLyrics1, toolStripMenuItem.Text);
					InsertingPresetItem = false;
					Lyrics_TextChanged(1);
				}
				else
				{
					tbLyrics2.SelectionStart = tbLyrics2MouseUpPos;
					tbLyrics2.SelectionLength = 0;
					InsertingPresetItem = true;
					Gf.InsertChordAboveCurrentLine(ref tbLyrics2, toolStripMenuItem.Text);
					InsertingPresetItem = false;
					Lyrics_TextChanged(2);
				}
			}
			catch
			{
			}
		}

		private void RightToLeft_Click(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			string name = checkBox.Name;
			if (name == "R1RightToLeft")
			{
				SetRightToLeftAtRegion(1, checkBox.Checked);
			}
			else if (name == "R2RightToLeft")
			{
				SetRightToLeftAtRegion(2, checkBox.Checked);
			}
		}

		private void SetRightToLeftAtRegion(int InRegion, bool IsTrue)
		{
			if (InRegion == 1)
			{
				SetRightToLeft1 = IsTrue;
				tbLyrics1.RightToLeft = (IsTrue ? RightToLeft.Yes : RightToLeft.No);
				ClearErrorMessage(1);
				tbLyrics1.Invalidate();
			}
			else
			{
				SetRightToLeft2 = IsTrue;
				tbLyrics2.RightToLeft = (IsTrue ? RightToLeft.Yes : RightToLeft.No);
				ClearErrorMessage(2);
				tbLyrics2.Invalidate();
			}
		}

		private void tbLyrics1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				int num = tbLyrics1MouseUpPos = tbLyrics1.GetCharIndexFromPosition(new System.Drawing.Point(e.X, e.Y));
			}
		}

		private void tbLyrics2_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				int num = tbLyrics2MouseUpPos = tbLyrics2.GetCharIndexFromPosition(new System.Drawing.Point(e.X, e.Y));
			}
		}

		private void Main_ChordsMenu_Click(object sender, EventArgs e)
		{
			SetMenuChordsMenu(Main_ChordsMenu.Checked);
		}

		private void Menu_ChordsMenu_Click(object sender, EventArgs e)
		{
			SetMenuChordsMenu(Menu_ChordsMenu.Checked);
		}

		private void SetMenuChordsMenu(bool IsChecked)
		{
			SetChordsMenu = IsChecked;
			Main_ChordsMenu.Checked = SetChordsMenu;
			Menu_ChordsMenu.Checked = SetChordsMenu;
			BuildLyricsContextMenu(1);
			BuildLyricsContextMenu(2);
		}

		            }
}
