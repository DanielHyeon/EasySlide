//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmFind : Form
	{
		private const int WordListMax = 20;

		private bool InitFormLoad = true;

		private string[] WordList = new string[20];

		private string Reg_FormLeft = "SearchFormLeft";

		private string Reg_FormTop = "SearchFormTop";

		public FrmFind()
		{
			InitializeComponent();
		}

		/// <summary>
		/// daniel �˻�
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FrmFind_Load(object sender, EventArgs e)
		{
			Gf.FindItemsFormOpen = true;
			int num = RegUtil.GetRegValue("settings", Reg_FormLeft, 50);
			int num2 = RegUtil.GetRegValue("settings", Reg_FormTop, 100);
			if (num < 0)
			{
				num = 0;
			}
			else if (num > Screen.PrimaryScreen.Bounds.Width - base.Width)
			{
				num = Screen.PrimaryScreen.Bounds.Width - base.Width;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			else if (num2 > Screen.PrimaryScreen.Bounds.Height - base.Height)
			{
				num2 = Screen.PrimaryScreen.Bounds.Height - base.Height;
			}
			base.Top = num2;
			base.Left = num;
			FolderList.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if ((Gf.FolderName[i].ToString() != "") & (Gf.FolderUse[i] > 0))
				{
					FolderList.Items.Add(Gf.FolderName[i]);
					if (Gf.FindSongsFolder[Gf.GetFolderNumber(Gf.FolderName[i])])
					{
						FolderList.SetItemChecked(FolderList.Items.Count - 1, value: true);
					}
				}
			}
			Gf.Find_SQLString = "";
			txtName.Text = Gf.FindSearchPhrase;
			cbTitle.Checked = Gf.FindItemInTitle;
			cbContents.Checked = Gf.FindItemInContents;
			cbSongNumber.Checked = Gf.FindItemInSongNumber;
			cbBookRef.Checked = Gf.FindItemInBookRef;
			cbUserRef.Checked = Gf.FindItemInUserRef;
			cbLicAdmin.Checked = Gf.FindItemInLicAdmin;
			cbWriter.Checked = Gf.FindItemInWriter;
			cbCopyright.Checked = Gf.FindItemInCopyright;
			cbMusicOnly.Checked = Gf.FindItemMediaOnly;
			cbNotationsOnly.Checked = Gf.FindItemNotationsOnly;
			cbUseDates.Checked = Gf.FindItemUseDates;
			CalendarFrom.Value = DateTime.Parse(Gf.FindItemDateFrom.ToShortTimeString());
			CalendarTo.Value = DateTime.Parse(Gf.FindItemDateTo.ToShortTimeString());
			cbUseDatesChanged();
			PopulateKeyTiming();
			TimerRestoreWindow.Start();
			BibleLookup.Items.Clear();
			BookLookup.Items.Clear();
			if (Gf.HB_TotalVersions < 1)
			{
				TabControl1.TabPages[1].Enabled = false;
				TabControl1.SelectedIndex = 0;
				txtName.Focus();
				txtName.SelectAll();
				return;
			}
			TabControl1.TabPages[1].Enabled = true;
			Gf.HB_SQLString = "";
			PassageSearchBox.Text = Gf.FindBibleSearchPhrase;
			InitFormLoad = true;
			for (int i = 0; i <= Gf.HB_TotalVersions - 1; i++)
			{
				BibleLookup.Items.Add(Gf.HB_Versions[i, 1] + " - " + Gf.HB_Versions[i, 2]);
			}
			InitFormLoad = false;
			BibleLookup.SelectedIndex = Gf.HB_CurVersionTabIndex;
			BookLookup.SelectedIndex = Gf.FindBibleBookIndex;
			if (Gf.FindBibleVerses)
			{
				TabControl1.SelectedIndex = 1;
				PassageSearchBox.Focus();
				PassageSearchBox.SelectAll();
			}
			else
			{
				txtName.Focus();
				txtName.SelectAll();
			}
		}

		private void OldPopulateSongKeyComboBox()
		{
			SongKey.Items.Clear();
			SongKey.Items.Add("");
			SongKey.Items.Add("A");
			SongKey.Items.Add("B");
			SongKey.Items.Add("C");
			SongKey.Items.Add("D");
			SongKey.Items.Add("E");
			SongKey.Items.Add("F");
			SongKey.Items.Add("G");
			SongKey.Items.Add("Am");
			SongKey.Items.Add("Bm");
			SongKey.Items.Add("Cm");
			SongKey.Items.Add("Dm");
			SongKey.Items.Add("Em");
			SongKey.Items.Add("Fm");
			SongKey.Items.Add("Gm");
			SongKey.Items.Add("Ab");
			SongKey.Items.Add("Bb");
			SongKey.Items.Add("Db");
			SongKey.Items.Add("Eb");
			SongKey.Items.Add("Gb");
			SongKey.Items.Add("Abm");
			SongKey.Items.Add("Bbm");
			SongKey.Items.Add("Dbm");
			SongKey.Items.Add("Ebm");
			SongKey.Items.Add("Gbm");
			SongKey.Items.Add("F#");
			SongKey.Items.Add("F#m");
			SongKey.Text = Gf.FindItemWithKey;
		}

		private void PopulateKeyTiming()
		{
			SongKey.Items.Clear();
			SongKey.Items.Add("");
			string fullSearchString = "select DISTINCT Key FROM SONG ORDER BY Key";
			string text = "";
			using DataTable datatable1 = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

			if (datatable1.Rows.Count > 0)
			{
				text = "";
				foreach (DataRow dr in datatable1.Rows)
				{
					text = DataUtil.Trim(DataUtil.GetDataString(dr, "Key"));
					if (text != "")
					{
						SongKey.Items.Add(text);
					}
				}
			}

			SongKey.Text = Gf.FindItemWithKey;
			SongTiming.Items.Clear();
			SongTiming.Items.Add("");
			fullSearchString = "select DISTINCT Timing FROM SONG ORDER BY Timing";

			using DataTable datatable2 = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

			if (datatable2.Rows.Count > 0)
			{
				text = "";
				foreach (DataRow dr in datatable2.Rows)
				{
					text = DataUtil.Trim(DataUtil.GetDataString(dr, "Timing"));
					if (text != "")
					{
						SongTiming.Items.Add(text);
					}
				}
			}

			SongTiming.Text = Gf.FindItemWithTiming;
		}

		private void BibleLookup_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!InitFormLoad)
			{
				BibleLookupIndexChanged();
			}
		}

		private void BibleLookupIndexChanged()
		{
			if (BibleLookup.Items.Count >= 1)
			{
				Gf.LoadBibleBooksList(BibleLookup.SelectedIndex, ref BookLookup, ShowAllBooksLine: true, ShowSearchResultsLine: false);
			}
		}

		private void SaveFormLocation()
		{
			RegUtil.SaveRegValue("settings", Reg_FormLeft, base.Left);
			RegUtil.SaveRegValue("settings", Reg_FormTop, base.Top);
		}

		private void FrmFind_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveFormLocation();
			TimerRestoreWindow.Stop();
			Gf.FindItemsFormOpen = false;
		}

		private void TimerRestoreWindow_Tick(object sender, EventArgs e)
		{
			if (Gf.FindItemRestoreWindow)
			{
				Gf.FindItemRestoreWindow = false;
				if (base.WindowState == FormWindowState.Minimized)
				{
					base.WindowState = FormWindowState.Normal;
				}
				else
				{
					Focus();
				}
				base.TopMost = true;
				base.TopMost = false;
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (TabControl1.SelectedIndex == 0)
			{
				if (FolderList.CheckedItems.Count == 0)
				{
					base.TopMost = false;
					MessageBox.Show("You have not selected any folders to search!");
					base.TopMost = true;
					return;
				}
				txtName.Text = DataUtil.Trim(txtName.Text);
				Gf.FindSearchPhrase = txtName.Text;
				Gf.FindItemInTitle = cbTitle.Checked;
				Gf.FindItemInContents = cbContents.Checked;
				Gf.FindItemInSongNumber = cbSongNumber.Checked;
				Gf.FindItemInBookRef = cbBookRef.Checked;
				Gf.FindItemInUserRef = cbUserRef.Checked;
				Gf.FindItemInLicAdmin = cbLicAdmin.Checked;
				Gf.FindItemInWriter = cbWriter.Checked;
				Gf.FindItemInCopyright = cbCopyright.Checked;
				Gf.FindItemMediaOnly = cbMusicOnly.Checked;
				Gf.FindItemNotationsOnly = cbNotationsOnly.Checked;
				Gf.FindItemWithKey = SongKey.Text;
				Gf.FindItemWithTiming = SongTiming.Text;
				Gf.FindItemUseDates = cbUseDates.Checked;
				Gf.FindItemDateFrom = CalendarFrom.Value;
				Gf.FindItemDateTo = CalendarTo.Value;
				Gf.Find_SQLString = Gf.BuildItemSearchString(txtName.Text, Gf.FindItemInTitle, Gf.FindItemInContents, Gf.FindItemInSongNumber, Gf.FindItemInBookRef, Gf.FindItemInUserRef, Gf.FindItemInLicAdmin, Gf.FindItemInWriter, Gf.FindItemInCopyright, Gf.FindItemNotationsOnly, Gf.FindItemWithKey, Gf.FindItemWithTiming, Gf.FindItemUseDates, Gf.FindItemDateFrom, Gf.FindItemDateTo, FolderList);
				if (Gf.Find_SQLString != "")
				{
					Gf.FindFolderItems = true;
					Gf.FindItemsRequested = true;
				}
			}
			else if (TabControl1.SelectedIndex == 1 && Gf.HB_TotalVersions >= 1)
			{
				string text = Gf.BuildBibleSearchString(MatchSelected: MatchAny.Checked ? 1 : ((!MatchAll.Checked) ? 2 : 0), InSearchPassage: PassageSearchBox.Text, VersionIndex: BibleLookup.SelectedIndex, BookIndex: BookLookup.SelectedIndex);
				if (text != "")
				{
					Gf.HB_CurVersionTabIndex = BibleLookup.SelectedIndex;
					Gf.HB_SQLString = text;
					Gf.FindBibleSearchPhrase = PassageSearchBox.Text;
					Gf.FindBibleBookIndex = BookLookup.SelectedIndex;
					Gf.FindItemsRequested = true;
				}
			}
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cbUseDates_CheckedChanged(object sender, EventArgs e)
		{
			cbUseDatesChanged();
		}

		private void cbUseDatesChanged()
		{
			CalendarFrom.Enabled = cbUseDates.Checked;
			CalendarTo.Enabled = cbUseDates.Checked;
		}
	}
}
