using Easislides.Module;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
    public class FrmLyricsScreen : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private bool FormFirstLoad = true;

		private int ScreenFontSize = 22;

		private int MainFontSize = 22;

		private int NotationsFontSize = 22;

		private bool LoadNextItem = false;

		private Thread timerThread;

		public bool WorshipListChanged = false;

		public bool ItemChanged = false;

		public bool LyricsChanged = false;

		public bool NotationsChanged = false;

		public bool LyricsAlertChanged = false;

		public int LyricsFlashCount = 0;

		private bool InitFormLoad = true;

		private string Reg_OutputSplit = "LyricsMonitorSplitOutput";

		private string Reg_NextItemSplit = "LyricsMonitorSplitNextItem";

		private string Reg_AlertSplit = "LyricsMonitorSplitAlert";

		private int LMSplitOutput = 500;

		private int LMSplitNextItem = 500;

		private int LMSplitAlert = 500;

		private IContainer components = null;

		private RichTextBox OutputLyrics;

		private Panel panelTop;

		private Panel panelBottom;

		private SplitContainer splitContainer1;

		private SplitContainer splitContainer2;

		private ListView WorshipListItems;

		private ColumnHeader columnHeader8;

		private ColumnHeader columnHeader9;

		private ColumnHeader columnHeader10;

		private ColumnHeader columnHeader11;

		private ColumnHeader columnHeader12;

		private ColumnHeader columnHeader13;

		private ColumnHeader columnHeader14;

		private RichTextBox PreviewLyrics;

		private Panel panelLeft;

		private Panel panelRight;

		private ImageList imageListSys;

		private RichTextBox LyricsAlertTextBox;

		private SplitContainer splitContainer3;

		private System.Windows.Forms.Timer timerLyricsAlert;

		public event Message OnMessage;

		public FrmLyricsScreen()
		{
			InitializeComponent();
		}

		private void FrmLyricsScreen_Load(object sender, EventArgs e)
		{
			InitForm();
			timerThread = new Thread(ThreadProc);
			timerThread.IsBackground = true;
			timerThread.Start();
		}

		private void InitForm()
		{
			SetShowWindow();
			gf.LyricsItem.Initialise();
			UpdateWorshipList();
		}

		public void ThreadProc()
		{
			try
			{
				MethodInvoker method = UpdateProgress;
				while (true)
				{
					BeginInvoke(method);
					Thread.Sleep(500);
				}
			}
			catch (ThreadInterruptedException)
			{
			}
			catch (Exception)
			{
			}
		}

		private void UpdateProgress()
		{
			try
			{
				if (WorshipListChanged)
				{
					WorshipListChanged = false;
					UpdateWorshipList();
				}
				else if (ItemChanged)
				{
					ItemChanged = false;
					Apply_ItemChanged();
				}
				else if (LyricsChanged)
				{
					LyricsChanged = false;
					Apply_LyricsChanged();
				}
				else if (NotationsChanged)
				{
					NotationsChanged = false;
					Apply_NotationsChanged();
				}
				else if (LyricsAlertChanged)
				{
					LyricsAlertChanged = false;
					Apply_LyricsAlertChanged();
				}
			}
			catch
			{
			}
		}

		private void WorshipListItems_Resize(object sender, EventArgs e)
		{
			SetWorshipPraiseListColWidth();
		}

		private void SetWorshipPraiseListColWidth()
		{
			if (WorshipListItems.Columns.Count > 0)
			{
				WorshipListItems.Columns[0].Width = ((WorshipListItems.Width - 25 >= 0) ? (WorshipListItems.Width - 25) : 0);
			}
		}

		private void SetShowWindow()
		{
			InitFormLoad = true;
			//if ((gf.LyricsMonitorNumber > 0 || gf.LMSelectAutoOption > 0) && gf.LMSelectAutoOption != 2)
			if ((gf.LyricsMonitorName == DisplayInfo.getSecondryDisplayName() || gf.LMSelectAutoOption > 0) && gf.LMSelectAutoOption != 2)
			{
				base.Left = gf.LM_Left;
				base.Top = gf.LM_Top;
				base.Height = gf.LM_Height;
				base.Width = gf.LM_Width;
				base.Visible = true;
			}
			else
			{
				base.Left = 0;
				base.Top = 0;
				base.Height = 1;
				base.Width = 1;
				base.Visible = false;
			}
			splitContainer1.Dock = DockStyle.Fill;
			splitContainer2.Dock = DockStyle.Fill;
			splitContainer3.Dock = DockStyle.Fill;
			PreviewLyrics.Dock = DockStyle.Fill;
			OutputLyrics.Dock = DockStyle.Fill;
			WorshipListItems.Dock = DockStyle.Fill;
			LyricsAlertTextBox.Dock = DockStyle.Fill;
			LoadPositions();
			WorshipListItems.BackColor = gf.LMBackColour;
			PreviewLyrics.BackColor = gf.LMBackColour;
			OutputLyrics.BackColor = gf.LMBackColour;
			LyricsAlertTextBox.BackColor = gf.LMBackColour;
			WorshipListItems.ForeColor = gf.LMTextColour;
			PreviewLyrics.ForeColor = gf.LMTextColour;
			OutputLyrics.ForeColor = gf.LMTextColour;
			WorshipListItems.ForeColor = gf.LMTextColour;
			LyricsAlertTextBox.ForeColor = gf.LMHighlightColour;
			SetLyricsFonts();
			InitFormLoad = false;
		}

		//private void SetShowWindow()
		//{
		//	InitFormLoad = true;
		//	//if ((gf.LyricsMonitorNumber > 0 || gf.LMSelectAutoOption > 0) && gf.LMSelectAutoOption != 2)
		//	if ((gf.LyricsMonitorNumber == gf.GetSecondryMonitorIndex() || gf.LMSelectAutoOption > 0) && gf.LMSelectAutoOption != 2)
		//	{
		//		base.Left = gf.LM_Left;
		//		base.Top = gf.LM_Top;
		//		base.Height = gf.LM_Height;
		//		base.Width = gf.LM_Width;
		//		base.Visible = true;
		//	}
		//	else
		//	{
		//		base.Left = 0;
		//		base.Top = 0;
		//		base.Height = 1;
		//		base.Width = 1;
		//		base.Visible = false;
		//	}
		//	splitContainer1.Dock = DockStyle.Fill;
		//	splitContainer2.Dock = DockStyle.Fill;
		//	splitContainer3.Dock = DockStyle.Fill;
		//	PreviewLyrics.Dock = DockStyle.Fill;
		//	OutputLyrics.Dock = DockStyle.Fill;
		//	WorshipListItems.Dock = DockStyle.Fill;
		//	LyricsAlertTextBox.Dock = DockStyle.Fill;
		//	LoadPositions();
		//	WorshipListItems.BackColor = gf.LMBackColour;
		//	PreviewLyrics.BackColor = gf.LMBackColour;
		//	OutputLyrics.BackColor = gf.LMBackColour;
		//	LyricsAlertTextBox.BackColor = gf.LMBackColour;
		//	WorshipListItems.ForeColor = gf.LMTextColour;
		//	PreviewLyrics.ForeColor = gf.LMTextColour;
		//	OutputLyrics.ForeColor = gf.LMTextColour;
		//	WorshipListItems.ForeColor = gf.LMTextColour;
		//	LyricsAlertTextBox.ForeColor = gf.LMHighlightColour;
		//	SetLyricsFonts();
		//	InitFormLoad = false;
		//}

		public void SetLyricsFonts()
		{
			ScreenFontSize = gf.DisplayFontSize(gf.LMMainFontSize, gf.LM_Width, 1, 1);
			if (ScreenFontSize < 2)
			{
				ScreenFontSize = 2;
			}
			OutputLyrics.Font = new Font(gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize, gf.tbLyricsMonitorSpace.Font.Style);
			PreviewLyrics.Font = new Font(gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize * 2 / 3, gf.tbLyricsMonitorSpace.Font.Style);
			WorshipListItems.Font = new Font(gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize * 2 / 3, gf.tbLyricsMonitorSpace.Font.Style);
			LyricsAlertTextBox.Font = new Font(gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize * 5 / 6, gf.tbLyricsMonitorSpace.Font.Style);
		}

		public void Remote_StopShow()
		{
			Hide();
		}

		public void Remote_WorshipListChanged()
		{
			WorshipListChanged = true;
		}

		public void Remote_NotationsChanged()
		{
			NotationsChanged = true;
		}

		private void Apply_NotationsChanged()
		{
			string InTitle = gf.WorshipSongs[gf.LyricsItem.CurItemNo, 2];
			LoadItem(ref gf.LyricsItem, gf.WorshipSongs[gf.LyricsItem.CurItemNo, 0], gf.WorshipSongs[gf.LyricsItem.CurItemNo, 4], ref InTitle);
		}

		public void Remote_ItemChanged()
		{
			ItemChanged = true;
		}

		private void Apply_ItemChanged()
		{
			OutputLyrics.Text = gf.tbLyricsMonitorSpace.Text;
			HighlightStartPresAtItem();
			LoadNextItem = true;
			Remote_LyricsChanged();
		}

		public void Remote_LyricsChanged()
		{
			LyricsChanged = true;
		}

		private void Apply_LyricsChanged()
		{
			gf.HighlightDisplaySlidesText(gf.LiveItem, ref OutputLyrics, ScrollToCaret: true, gf.LMTextColour, gf.LMHighlightColour);
			if (LoadNextItem)
			{
				LoadNextItemLyrics();
			}
		}

		public void Remote_LyricsAlertChanged()
		{
			LyricsAlertChanged = true;
		}

		private void Apply_LyricsAlertChanged()
		{
			LyricsAlertTextBox.Text = gf.LyricsAlertDetails;
			LyricsAlertTextBox.SelectAll();
			LyricsAlertTextBox.SelectionAlignment = HorizontalAlignment.Center;
			LyricsAlertTextBox.SelectionLength = 0;
			timerLyricsAlertReset();
			if (LyricsAlertTextBox.Text != "")
			{
				timerLyricsAlert.Start();
			}
		}

		private void EnableRemoteUpdate()
		{
			gf.LyricsMonitor_LyricsChanged = true;
		}

		private void UpdateWorshipList()
		{
			WorshipListItems.Items.Clear();
			if (gf.TotalWorshipListItems <= 0)
			{
				return;
			}
			ListViewItem listViewItem = new ListViewItem();
			string text = "";
			for (int i = 0; i < gf.TotalWorshipListItems; i++)
			{
				listViewItem = WorshipListItems.Items.Add(gf.WorshipSongs[i + 1, 2]);
				text = gf.WorshipSongs[i + 1, 1];
				if (text == "D")
				{
					listViewItem.ImageIndex = 0;
				}
				else if (text == "P")
				{
					listViewItem.ImageIndex = 2;
				}
				else if (text == "B")
				{
					listViewItem.ImageIndex = 4;
				}
				else if (text == "T")
				{
					listViewItem.ImageIndex = 6;
				}
				else if (text == "I")
				{
					listViewItem.ImageIndex = 8;
				}
				else if (text == "M")
				{
					listViewItem.ImageIndex = 28;
				}
				else if (text == "W")
				{
					listViewItem.ImageIndex = 10;
				}
				listViewItem.SubItems.Add(gf.WorshipSongs[i + 1, 0]);
				listViewItem.SubItems.Add(gf.WorshipSongs[i + 1, 4]);
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add("");
			}
			HighlightStartPresAtItem();
			SetWorshipPraiseListColWidth();
		}

		private void HighlightStartPresAtItem()
		{
			if (gf.StartPresAt > 0 && gf.StartPresAt <= gf.TotalWorshipListItems)
			{
				WorshipListItems.Items[gf.StartPresAt - 1].Selected = true;
				WorshipListItems.Items[gf.StartPresAt - 1].EnsureVisible();
			}
		}

		private void LoadNextItemLyrics()
		{
			string text = "";
			if (gf.StartPresAt > 0)
			{
				int num = (gf.StartPresAt < gf.TotalWorshipListItems - 1) ? (gf.StartPresAt + 1) : gf.TotalWorshipListItems;
				string InTitle = gf.WorshipSongs[num, 2];
				LoadItem(ref gf.LyricsItem, gf.WorshipSongs[num, 0], gf.WorshipSongs[num, 4], ref InTitle);
				gf.LyricsItem.CurItemNo = num;
			}
			LoadNextItem = false;
		}

		private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, ref string InTitle)
		{
			string text = DataUtil.Left(InIDString, 1);
			gf.InitialiseIndividualData(ref InItem);
			InItem.PrevTitle = "";
			InItem.NextTitle = "";
			PreviewLyrics.Text = "";
			int num;
			switch (text)
			{
			default:
				num = ((!(text == "W")) ? 1 : 0);
				break;
			case "D":
			case "B":
			case "T":
			case "I":
				num = 0;
				break;
			}
			if (num == 0)
			{
				gf.LoadIndividualData(ref InItem, InIDString, "", 1, ref InTitle);
				if (InItem.Type == "I")
				{
					InFormatString = InItem.Format.FormatString;
				}
				gf.LoadIndividualFormatData(ref InItem, InFormatString);
				gf.FormatText(ref InItem, gf.PanelBackColour, gf.PanelBackColourTransparent, gf.PanelTextColour, gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
				gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
				gf.DisplaySlidesFormattedLyrics(ref InItem, ref PreviewLyrics, ScrollToCaret: true, gf.LMShowNotations);
			}
		}

		private void FrmLyricsScreen_VisibleChanged(object sender, EventArgs e)
		{
			if (base.Visible)
			{
				if (FormFirstLoad)
				{
					FormFirstLoad = false;
				}
				else
				{
					InitForm();
				}
			}
		}

		private void timerLyricsAlert_Tick(object sender, EventArgs e)
		{
			LyricsAlertTextBox.ForeColor = ((LyricsAlertTextBox.ForeColor == gf.LMHighlightColour) ? gf.LMTextColour : gf.LMHighlightColour);
			LyricsAlertTextBox.BackColor = ((LyricsAlertTextBox.BackColor == gf.LMBackColour) ? Color.Red : gf.LMBackColour);
			LyricsFlashCount++;
			if (LyricsFlashCount > 3)
			{
				LyricsFlashCount = 0;
				timerLyricsAlert.Stop();
			}
		}

		private void timerLyricsAlertReset()
		{
			LyricsFlashCount = 0;
			timerLyricsAlert.Stop();
			LyricsAlertTextBox.ForeColor = gf.LMHighlightColour;
			LyricsAlertTextBox.BackColor = gf.LMBackColour;
		}

		private void FrmLyricsScreen_FormClosing(object sender, FormClosingEventArgs e)
		{
		}

		private void LoadPositions()
		{
			LMSplitOutput = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", Reg_OutputSplit, 300));
			LMSplitOutput = ((LMSplitOutput < 1 || LMSplitOutput > 1000) ? 300 : LMSplitOutput);
			LMSplitOutput = splitContainer1.Width * LMSplitOutput / 1000;
			LMSplitOutput = ((LMSplitOutput < 30) ? 30 : LMSplitOutput);
			LMSplitNextItem = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", Reg_NextItemSplit, 350));
			LMSplitNextItem = ((LMSplitNextItem < 1 || LMSplitNextItem > 1000) ? 350 : LMSplitNextItem);
			LMSplitNextItem = splitContainer2.Height * LMSplitNextItem / 1000;
			LMSplitNextItem = ((LMSplitNextItem < 30) ? 30 : LMSplitNextItem);
			LMSplitAlert = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", Reg_AlertSplit, 600));
			LMSplitAlert = ((LMSplitAlert < 1 || LMSplitAlert > 1000) ? 600 : LMSplitAlert);
			LMSplitAlert = splitContainer3.Height * LMSplitAlert / 1000;
			LMSplitAlert = ((LMSplitAlert < 30) ? 30 : LMSplitAlert);
			if (base.Width > 100)
			{
				splitContainer1.SplitterDistance = LMSplitOutput;
				splitContainer3.SplitterDistance = LMSplitAlert;
				splitContainer2.SplitterDistance = LMSplitNextItem;
			}
		}

		private void SavePositions()
		{
			if (!InitFormLoad)
			{
				int value = splitContainer1.SplitterDistance * 1000 / splitContainer1.Width;
				RegUtil.SaveRegValue("monitors", Reg_OutputSplit, value);
				value = splitContainer2.SplitterDistance * 1000 / splitContainer2.Height;
				RegUtil.SaveRegValue("monitors", Reg_NextItemSplit, value);
				value = splitContainer3.SplitterDistance * 1000 / splitContainer3.Height;
				RegUtil.SaveRegValue("monitors", Reg_AlertSplit, value);
			}
		}

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			SavePositions();
		}

		private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
		{
			SavePositions();
		}

		private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
		{
			SavePositions();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLyricsScreen));
			OutputLyrics = new System.Windows.Forms.RichTextBox();
			panelTop = new System.Windows.Forms.Panel();
			panelBottom = new System.Windows.Forms.Panel();
			splitContainer1 = new System.Windows.Forms.SplitContainer();
			splitContainer2 = new System.Windows.Forms.SplitContainer();
			WorshipListItems = new System.Windows.Forms.ListView();
			columnHeader8 = new System.Windows.Forms.ColumnHeader();
			columnHeader9 = new System.Windows.Forms.ColumnHeader();
			columnHeader10 = new System.Windows.Forms.ColumnHeader();
			columnHeader11 = new System.Windows.Forms.ColumnHeader();
			columnHeader12 = new System.Windows.Forms.ColumnHeader();
			columnHeader13 = new System.Windows.Forms.ColumnHeader();
			columnHeader14 = new System.Windows.Forms.ColumnHeader();
			imageListSys = new System.Windows.Forms.ImageList(components);
			splitContainer3 = new System.Windows.Forms.SplitContainer();
			PreviewLyrics = new System.Windows.Forms.RichTextBox();
			LyricsAlertTextBox = new System.Windows.Forms.RichTextBox();
			panelLeft = new System.Windows.Forms.Panel();
			panelRight = new System.Windows.Forms.Panel();
			timerLyricsAlert = new System.Windows.Forms.Timer(components);
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			splitContainer2.Panel1.SuspendLayout();
			splitContainer2.Panel2.SuspendLayout();
			splitContainer2.SuspendLayout();
			splitContainer3.Panel1.SuspendLayout();
			splitContainer3.Panel2.SuspendLayout();
			splitContainer3.SuspendLayout();
			SuspendLayout();
			OutputLyrics.BackColor = System.Drawing.SystemColors.Window;
			OutputLyrics.Dock = System.Windows.Forms.DockStyle.Fill;
			OutputLyrics.Location = new System.Drawing.Point(0, 0);
			OutputLyrics.Name = "OutputLyrics";
			OutputLyrics.ReadOnly = true;
			OutputLyrics.ShowSelectionMargin = true;
			OutputLyrics.Size = new System.Drawing.Size(148, 162);
			OutputLyrics.TabIndex = 1;
			OutputLyrics.Text = "";
			panelTop.Dock = System.Windows.Forms.DockStyle.Top;
			panelTop.Location = new System.Drawing.Point(0, 0);
			panelTop.Name = "panelTop";
			panelTop.Size = new System.Drawing.Size(220, 3);
			panelTop.TabIndex = 6;
			panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			panelBottom.Location = new System.Drawing.Point(0, 165);
			panelBottom.Name = "panelBottom";
			panelBottom.Size = new System.Drawing.Size(220, 3);
			panelBottom.TabIndex = 7;
			splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer1.Location = new System.Drawing.Point(3, 3);
			splitContainer1.Name = "splitContainer1";
			splitContainer1.Panel1.Controls.Add(splitContainer2);
			splitContainer1.Panel2.Controls.Add(OutputLyrics);
			splitContainer1.Size = new System.Drawing.Size(214, 162);
			splitContainer1.SplitterDistance = 63;
			splitContainer1.SplitterWidth = 3;
			splitContainer1.TabIndex = 8;
			splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer1_SplitterMoved);
			splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer2.Location = new System.Drawing.Point(0, 0);
			splitContainer2.Name = "splitContainer2";
			splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainer2.Panel1.Controls.Add(WorshipListItems);
			splitContainer2.Panel2.Controls.Add(splitContainer3);
			splitContainer2.Size = new System.Drawing.Size(63, 162);
			splitContainer2.SplitterDistance = 45;
			splitContainer2.SplitterWidth = 3;
			splitContainer2.TabIndex = 9;
			splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer2_SplitterMoved);
			WorshipListItems.AllowDrop = true;
			WorshipListItems.BackColor = System.Drawing.SystemColors.Window;
			WorshipListItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[7]
			{
				columnHeader8,
				columnHeader9,
				columnHeader10,
				columnHeader11,
				columnHeader12,
				columnHeader13,
				columnHeader14
			});
			WorshipListItems.Dock = System.Windows.Forms.DockStyle.Fill;
			WorshipListItems.FullRowSelect = true;
			WorshipListItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			WorshipListItems.HideSelection = false;
			WorshipListItems.LabelWrap = false;
			WorshipListItems.Location = new System.Drawing.Point(0, 0);
			WorshipListItems.MultiSelect = false;
			WorshipListItems.Name = "WorshipListItems";
			WorshipListItems.Size = new System.Drawing.Size(63, 45);
			WorshipListItems.SmallImageList = imageListSys;
			WorshipListItems.TabIndex = 2;
			WorshipListItems.UseCompatibleStateImageBehavior = false;
			WorshipListItems.View = System.Windows.Forms.View.Details;
			WorshipListItems.Resize += new System.EventHandler(WorshipListItems_Resize);
			columnHeader9.Width = 0;
			columnHeader10.Width = 0;
			columnHeader11.Width = 0;
			columnHeader12.Width = 0;
			columnHeader13.Width = 0;
			columnHeader14.Width = 0;
			imageListSys.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListSys.ImageStream");
			imageListSys.TransparentColor = System.Drawing.Color.Transparent;
			imageListSys.Images.SetKeyName(0, "ES Icon 32 Blue.ico");
			imageListSys.Images.SetKeyName(1, "ES Icon 32 Blue - Highlight.ico");
			imageListSys.Images.SetKeyName(2, "PPImg.gif");
			imageListSys.Images.SetKeyName(3, "PPImg - Highlight.gif");
			imageListSys.Images.SetKeyName(4, "Bible.gif");
			imageListSys.Images.SetKeyName(5, "Bible - Hightlight.gif");
			imageListSys.Images.SetKeyName(6, "notebook.gif");
			imageListSys.Images.SetKeyName(7, "notebook-highlight.gif");
			imageListSys.Images.SetKeyName(8, "Info_Sym.gif");
			imageListSys.Images.SetKeyName(9, "Info_Sym highlight.gif");
			imageListSys.Images.SetKeyName(10, "word.gif");
			imageListSys.Images.SetKeyName(11, "word-highlight.gif");
			imageListSys.Images.SetKeyName(12, "singlescreen.gif");
			imageListSys.Images.SetKeyName(13, "dualscreens.gif");
			imageListSys.Images.SetKeyName(14, "keyboard.gif");
			imageListSys.Images.SetKeyName(15, "BlackScreen-Pressed.gif");
			imageListSys.Images.SetKeyName(16, "BlackScreen-Red.gif");
			imageListSys.Images.SetKeyName(17, "BlueScreen-Pressed.gif");
			imageListSys.Images.SetKeyName(18, "BlueScreen-Red.gif");
			imageListSys.Images.SetKeyName(19, "folder.gif");
			imageListSys.Images.SetKeyName(20, "pic-bestfit.gif");
			imageListSys.Images.SetKeyName(21, "Bible.gif");
			imageListSys.Images.SetKeyName(22, "options.gif");
			imageListSys.Images.SetKeyName(23, "Info_Sym.gif");
			imageListSys.Images.SetKeyName(24, "PPImg.gif");
			imageListSys.Images.SetKeyName(25, "Tick.gif");
			imageListSys.Images.SetKeyName(26, "NumNewScreen.gif");
			imageListSys.Images.SetKeyName(27, "ques.gif");
			imageListSys.Images.SetKeyName(28, "Media.gif");
			imageListSys.Images.SetKeyName(29, "Media-highlight.gif");
			splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer3.Location = new System.Drawing.Point(0, 0);
			splitContainer3.Name = "splitContainer3";
			splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainer3.Panel1.Controls.Add(PreviewLyrics);
			splitContainer3.Panel2.Controls.Add(LyricsAlertTextBox);
			splitContainer3.Size = new System.Drawing.Size(63, 114);
			splitContainer3.SplitterDistance = 72;
			splitContainer3.SplitterWidth = 3;
			splitContainer3.TabIndex = 11;
			splitContainer3.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer3_SplitterMoved);
			PreviewLyrics.BackColor = System.Drawing.SystemColors.Window;
			PreviewLyrics.Dock = System.Windows.Forms.DockStyle.Fill;
			PreviewLyrics.Location = new System.Drawing.Point(0, 0);
			PreviewLyrics.Name = "PreviewLyrics";
			PreviewLyrics.ReadOnly = true;
			PreviewLyrics.Size = new System.Drawing.Size(63, 72);
			PreviewLyrics.TabIndex = 2;
			PreviewLyrics.Text = "";
			LyricsAlertTextBox.BackColor = System.Drawing.SystemColors.Window;
			LyricsAlertTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			LyricsAlertTextBox.Location = new System.Drawing.Point(0, 0);
			LyricsAlertTextBox.Name = "LyricsAlertTextBox";
			LyricsAlertTextBox.ReadOnly = true;
			LyricsAlertTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			LyricsAlertTextBox.ShowSelectionMargin = true;
			LyricsAlertTextBox.Size = new System.Drawing.Size(63, 39);
			LyricsAlertTextBox.TabIndex = 2;
			LyricsAlertTextBox.Text = "";
			panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
			panelLeft.Location = new System.Drawing.Point(0, 3);
			panelLeft.Name = "panelLeft";
			panelLeft.Size = new System.Drawing.Size(3, 162);
			panelLeft.TabIndex = 9;
			panelRight.Dock = System.Windows.Forms.DockStyle.Right;
			panelRight.Location = new System.Drawing.Point(217, 3);
			panelRight.Name = "panelRight";
			panelRight.Size = new System.Drawing.Size(3, 162);
			panelRight.TabIndex = 10;
			timerLyricsAlert.Interval = 500;
			timerLyricsAlert.Tick += new System.EventHandler(timerLyricsAlert_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(220, 168);
			base.ControlBox = false;
			base.Controls.Add(splitContainer1);
			base.Controls.Add(panelLeft);
			base.Controls.Add(panelRight);
			base.Controls.Add(panelBottom);
			base.Controls.Add(panelTop);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmLyricsScreen";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			Text = "Lyrics Monitor";
			base.Load += new System.EventHandler(FrmLyricsScreen_Load);
			base.VisibleChanged += new System.EventHandler(FrmLyricsScreen_VisibleChanged);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLyricsScreen_FormClosing);
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			splitContainer1.ResumeLayout(false);
			splitContainer2.Panel1.ResumeLayout(false);
			splitContainer2.Panel2.ResumeLayout(false);
			splitContainer2.ResumeLayout(false);
			splitContainer3.Panel1.ResumeLayout(false);
			splitContainer3.Panel2.ResumeLayout(false);
			splitContainer3.ResumeLayout(false);
			ResumeLayout(false);
		}
	}
}
