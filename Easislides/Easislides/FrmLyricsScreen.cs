using Easislides.Module;
using Easislides.Util;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    public partial class FrmLyricsScreen : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private int ScreenFontSize = 22;

		private int MainFontSize = 22;

		private int NotationsFontSize = 22;

		private bool LoadNextItem = false;

		private readonly Timer updateTimer;
		private readonly RegistrySettings layoutSettings;

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
			: this(DisplayInfo.Lyrics)
		{
		}

		internal FrmLyricsScreen(MonitorPlacement placement, RegistrySettings settings = null)
		{
			InitializeComponent();
			layoutSettings = settings ?? RegUtil.Settings;
			StartPosition = FormStartPosition.Manual;
			if (placement.Enabled)
				Bounds = placement.Bounds;
			updateTimer = new Timer(components) { Interval = 250 };
			updateTimer.Tick += (_, _) => UpdateProgress();
		}

		protected override bool ShowWithoutActivation => true;

		internal void PrepareLayout()
		{
			SetShowWindow();
			Gf.LyricsItem.Initialise();
			UpdateWorshipList();
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
			int index = Gf.LyricsItem.CurItemNo;
			if (index < 1 || index > Gf.TotalWorshipListItems)
				return;
			var item = Gf.WorshipSongs[index];
			string title = item.Title;
			LoadItem(ref Gf.LyricsItem, item.IdString, item.FormatString, ref title);
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
			string itemTypeCode = "";
			for (int i = 0; i < Gf.TotalWorshipListItems; i++)
			{
				var item = Gf.WorshipSongs[i + 1];
				listViewItem = WorshipListItems.Items.Add(item.Title);
				itemTypeCode = item.Type;
				if (itemTypeCode == "D")
				{
					listViewItem.ImageIndex = 0;
				}
				else if (itemTypeCode == "P")
				{
					listViewItem.ImageIndex = 2;
				}
				else if (itemTypeCode == "B")
				{
					listViewItem.ImageIndex = 4;
				}
				else if (itemTypeCode == "T")
				{
					listViewItem.ImageIndex = 6;
				}
				else if (itemTypeCode == "I")
				{
					listViewItem.ImageIndex = 8;
				}
				else if (itemTypeCode == "M")
				{
					listViewItem.ImageIndex = 28;
				}
				else if (itemTypeCode == "W")
				{
					listViewItem.ImageIndex = 10;
				}
				listViewItem.SubItems.Add(item.IdString);
				listViewItem.SubItems.Add(item.FormatString);
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
			if (Gf.StartPresAt > 0 && Gf.StartPresAt <= WorshipListItems.Items.Count)
			{
				WorshipListItems.Items[Gf.StartPresAt - 1].Selected = true;
				WorshipListItems.Items[Gf.StartPresAt - 1].EnsureVisible();
			}
		}

		private void LoadNextItemLyrics()
		{
			if (Gf.StartPresAt > 0 && Gf.StartPresAt < Gf.TotalWorshipListItems)
			{
				int index = Gf.StartPresAt + 1;
				var item = Gf.WorshipSongs[index];
				string title = item.Title;
				LoadItem(ref Gf.LyricsItem, item.IdString, item.FormatString, ref title);
				Gf.LyricsItem.CurItemNo = index;
			}
			else
				PreviewLyrics.Clear();
			LoadNextItem = false;
		}

		private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, ref string InTitle)
		{
			string itemTypeCode = DataUtil.Left(InIDString, 1);
			Gf.InitialiseIndividualData(ref InItem);
			InItem.PrevTitle = "";
			InItem.NextTitle = "";
			PreviewLyrics.Text = "";
			int unsupportedItemType;
			switch (itemTypeCode)
			{
			default:
				unsupportedItemType = ((!(itemTypeCode == "W")) ? 1 : 0);
				break;
			case "D":
			case "B":
			case "T":
			case "I":
				unsupportedItemType = 0;
				break;
			}
			if (unsupportedItemType == 0)
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
			if (updateTimer != null)
				updateTimer.Enabled = Visible;
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
			updateTimer.Stop();
			timerLyricsAlert.Stop();
		}

		private void LoadPositions()
		{
			LMSplitOutput = ReadSplitterSetting(Reg_OutputSplit, 300);
			LMSplitOutput = ((LMSplitOutput < 1 || LMSplitOutput > 1000) ? 300 : LMSplitOutput);
			LMSplitOutput = splitContainer1.Width * LMSplitOutput / 1000;
			LMSplitOutput = ((LMSplitOutput < 30) ? 30 : LMSplitOutput);
			LMSplitNextItem = ReadSplitterSetting(Reg_NextItemSplit, 350);
			LMSplitNextItem = ((LMSplitNextItem < 1 || LMSplitNextItem > 1000) ? 350 : LMSplitNextItem);
			LMSplitNextItem = splitContainer2.Height * LMSplitNextItem / 1000;
			LMSplitNextItem = ((LMSplitNextItem < 30) ? 30 : LMSplitNextItem);
			LMSplitAlert = ReadSplitterSetting(Reg_AlertSplit, 600);
			LMSplitAlert = ((LMSplitAlert < 1 || LMSplitAlert > 1000) ? 600 : LMSplitAlert);
			LMSplitAlert = splitContainer3.Height * LMSplitAlert / 1000;
			LMSplitAlert = ((LMSplitAlert < 30) ? 30 : LMSplitAlert);
			SetSplitterDistance(splitContainer1, LMSplitOutput);
			SetSplitterDistance(splitContainer3, LMSplitAlert);
			SetSplitterDistance(splitContainer2, LMSplitNextItem);
		}

		private int ReadSplitterSetting(string name, int fallback)
		{
			layoutSettings.TryReadValue("monitors", name, out object value);
			return value != null ? DataUtil.ObjToInt(value) : fallback;
		}

		private static void SetSplitterDistance(SplitContainer splitter, int distance)
		{
			int size = splitter.Orientation == Orientation.Vertical ? splitter.Width : splitter.Height;
			int maximum = size - splitter.SplitterWidth - splitter.Panel2MinSize;
			if (maximum >= splitter.Panel1MinSize)
				splitter.SplitterDistance = Math.Clamp(distance, splitter.Panel1MinSize, maximum);
		}

		private void SavePositions()
		{
			if (!InitFormLoad && Visible && splitContainer1.Width > 0 && splitContainer2.Height > 0 && splitContainer3.Height > 0)
			{
				int value = splitContainer1.SplitterDistance * 1000 / splitContainer1.Width;
				layoutSettings.TryWriteValue("monitors", Reg_OutputSplit, value, RegistryValueKind.DWord);
				value = splitContainer2.SplitterDistance * 1000 / splitContainer2.Height;
				layoutSettings.TryWriteValue("monitors", Reg_NextItemSplit, value, RegistryValueKind.DWord);
				value = splitContainer3.SplitterDistance * 1000 / splitContainer3.Height;
				layoutSettings.TryWriteValue("monitors", Reg_AlertSplit, value, RegistryValueKind.DWord);
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
