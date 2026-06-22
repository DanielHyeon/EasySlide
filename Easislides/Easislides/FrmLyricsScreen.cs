using Easislides.Module;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
    public partial class FrmLyricsScreen : Form
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
			Gf.LyricsItem.Initialise();
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
			//if ((Gf.LyricsMonitorNumber > 0 || Gf.LMSelectAutoOption > 0) && Gf.LMSelectAutoOption != 2)
			if ((Gf.LyricsMonitorName == DisplayInfo.getSecondryDisplayName() || Gf.LMSelectAutoOption > 0) && Gf.LMSelectAutoOption != 2)
			{
				base.Left = Gf.LM_Left;
				base.Top = Gf.LM_Top;
				base.Height = Gf.LM_Height;
				base.Width = Gf.LM_Width;
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
			WorshipListItems.BackColor = Gf.LMBackColour;
			PreviewLyrics.BackColor = Gf.LMBackColour;
			OutputLyrics.BackColor = Gf.LMBackColour;
			LyricsAlertTextBox.BackColor = Gf.LMBackColour;
			WorshipListItems.ForeColor = Gf.LMTextColour;
			PreviewLyrics.ForeColor = Gf.LMTextColour;
			OutputLyrics.ForeColor = Gf.LMTextColour;
			WorshipListItems.ForeColor = Gf.LMTextColour;
			LyricsAlertTextBox.ForeColor = Gf.LMHighlightColour;
			SetLyricsFonts();
			InitFormLoad = false;
		}

		//private void SetShowWindow()
		//{
		//	InitFormLoad = true;
		//	//if ((Gf.LyricsMonitorNumber > 0 || Gf.LMSelectAutoOption > 0) && Gf.LMSelectAutoOption != 2)
		//	if ((Gf.LyricsMonitorNumber == Gf.GetSecondryMonitorIndex() || Gf.LMSelectAutoOption > 0) && Gf.LMSelectAutoOption != 2)
		//	{
		//		base.Left = Gf.LM_Left;
		//		base.Top = Gf.LM_Top;
		//		base.Height = Gf.LM_Height;
		//		base.Width = Gf.LM_Width;
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
		//	WorshipListItems.BackColor = Gf.LMBackColour;
		//	PreviewLyrics.BackColor = Gf.LMBackColour;
		//	OutputLyrics.BackColor = Gf.LMBackColour;
		//	LyricsAlertTextBox.BackColor = Gf.LMBackColour;
		//	WorshipListItems.ForeColor = Gf.LMTextColour;
		//	PreviewLyrics.ForeColor = Gf.LMTextColour;
		//	OutputLyrics.ForeColor = Gf.LMTextColour;
		//	WorshipListItems.ForeColor = Gf.LMTextColour;
		//	LyricsAlertTextBox.ForeColor = Gf.LMHighlightColour;
		//	SetLyricsFonts();
		//	InitFormLoad = false;
		//}

		public void SetLyricsFonts()
		{
			ScreenFontSize = Gf.DisplayFontSize(Gf.LMMainFontSize, Gf.LM_Width, 1, 1);
			if (ScreenFontSize < 2)
			{
				ScreenFontSize = 2;
			}
			OutputLyrics.Font = new Font(Gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize, Gf.tbLyricsMonitorSpace.Font.Style);
			PreviewLyrics.Font = new Font(Gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize * 2 / 3, Gf.tbLyricsMonitorSpace.Font.Style);
			WorshipListItems.Font = new Font(Gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize * 2 / 3, Gf.tbLyricsMonitorSpace.Font.Style);
			LyricsAlertTextBox.Font = new Font(Gf.tbLyricsMonitorSpace.Font.Name, ScreenFontSize * 5 / 6, Gf.tbLyricsMonitorSpace.Font.Style);
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
			string InTitle = Gf.WorshipSongs[Gf.LyricsItem.CurItemNo, 2];
			LoadItem(ref Gf.LyricsItem, Gf.WorshipSongs[Gf.LyricsItem.CurItemNo, 0], Gf.WorshipSongs[Gf.LyricsItem.CurItemNo, 4], ref InTitle);
		}

		public void Remote_ItemChanged()
		{
			ItemChanged = true;
		}

		private void Apply_ItemChanged()
		{
			OutputLyrics.Text = Gf.tbLyricsMonitorSpace.Text;
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
			Gf.HighlightDisplaySlidesText(Gf.LiveItem, ref OutputLyrics, ScrollToCaret: true, Gf.LMTextColour, Gf.LMHighlightColour);
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
			LyricsAlertTextBox.Text = Gf.LyricsAlertDetails;
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
			Gf.LyricsMonitor_LyricsChanged = true;
		}

		private void UpdateWorshipList()
		{
			WorshipListItems.Items.Clear();
			if (Gf.TotalWorshipListItems <= 0)
			{
				return;
			}
			ListViewItem listViewItem = new ListViewItem();
			string text = "";
			for (int i = 0; i < Gf.TotalWorshipListItems; i++)
			{
				listViewItem = WorshipListItems.Items.Add(Gf.WorshipSongs[i + 1, 2]);
				text = Gf.WorshipSongs[i + 1, 1];
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
				listViewItem.SubItems.Add(Gf.WorshipSongs[i + 1, 0]);
				listViewItem.SubItems.Add(Gf.WorshipSongs[i + 1, 4]);
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
			if (Gf.StartPresAt > 0 && Gf.StartPresAt <= Gf.TotalWorshipListItems)
			{
				WorshipListItems.Items[Gf.StartPresAt - 1].Selected = true;
				WorshipListItems.Items[Gf.StartPresAt - 1].EnsureVisible();
			}
		}

		private void LoadNextItemLyrics()
		{
			string text = "";
			if (Gf.StartPresAt > 0)
			{
				int num = (Gf.StartPresAt < Gf.TotalWorshipListItems - 1) ? (Gf.StartPresAt + 1) : Gf.TotalWorshipListItems;
				string InTitle = Gf.WorshipSongs[num, 2];
				LoadItem(ref Gf.LyricsItem, Gf.WorshipSongs[num, 0], Gf.WorshipSongs[num, 4], ref InTitle);
				Gf.LyricsItem.CurItemNo = num;
			}
			LoadNextItem = false;
		}

		private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, ref string InTitle)
		{
			string text = DataUtil.Left(InIDString, 1);
			Gf.InitialiseIndividualData(ref InItem);
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
				Gf.LoadIndividualData(ref InItem, InIDString, "", 1, ref InTitle);
				if (InItem.Type == "I")
				{
					InFormatString = InItem.Format.FormatString;
				}
				Gf.LoadIndividualFormatData(ref InItem, InFormatString);
				Gf.FormatText(ref InItem, Gf.PanelBackColour, Gf.PanelBackColourTransparent, Gf.PanelTextColour, Gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
				Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
				Gf.DisplaySlidesFormattedLyrics(ref InItem, ref PreviewLyrics, ScrollToCaret: true, Gf.LMShowNotations);
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
			LyricsAlertTextBox.ForeColor = ((LyricsAlertTextBox.ForeColor == Gf.LMHighlightColour) ? Gf.LMTextColour : Gf.LMHighlightColour);
			LyricsAlertTextBox.BackColor = ((LyricsAlertTextBox.BackColor == Gf.LMBackColour) ? Color.Red : Gf.LMBackColour);
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
			LyricsAlertTextBox.ForeColor = Gf.LMHighlightColour;
			LyricsAlertTextBox.BackColor = Gf.LMBackColour;
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

					}
}
