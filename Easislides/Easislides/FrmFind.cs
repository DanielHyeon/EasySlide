//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Global;
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
			int windowLeft = RegUtil.GetRegValue("settings", Reg_FormLeft, 50);
			int windowTop = RegUtil.GetRegValue("settings", Reg_FormTop, 100);
			if (windowLeft < 0)
			{
				windowLeft = 0;
			}
			else if (windowLeft > Screen.PrimaryScreen.Bounds.Width - base.Width)
			{
				windowLeft = Screen.PrimaryScreen.Bounds.Width - base.Width;
			}
			if (windowTop < 0)
			{
				windowTop = 0;
			}
			else if (windowTop > Screen.PrimaryScreen.Bounds.Height - base.Height)
			{
				windowTop = Screen.PrimaryScreen.Bounds.Height - base.Height;
			}
			base.Top = windowTop;
			base.Left = windowLeft;
			FolderList.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if ((Gf.Folders[i].Name != "") & (Gf.Folders[i].Use > 0))
				{
					FolderList.Items.Add(Gf.Folders[i].Name);
					if (Gf.Folders[Gf.GetFolderNumber(Gf.Folders[i].Name)].FindSongs)
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
			// Was DateTime.Parse(date.ToShortTimeString()), which kept only the time: both pickers always showed today.
			CalendarFrom.Value = WithinPicker(CalendarFrom, Gf.FindItemDateFrom.Date);
			CalendarTo.Value = WithinPicker(CalendarTo, Gf.FindItemDateTo.Date);
			cbUseDatesChanged();
			PopulateKeyTiming();
			TimerRestoreWindow.Start();
			BibleLookup.Items.Clear();
			BookLookup.Items.Clear();
			if (Gf.BibleVersions.Count < 1)
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
			foreach (var version in Gf.BibleVersions)
			{
				BibleLookup.Items.Add(version.Name + " - " + version.Description);
			}
			InitFormLoad = false;
			BibleLookup.SelectedIndex = Gf.BibleVersions.ContainsIndex(Gf.HB_CurVersionTabIndex) ? Gf.HB_CurVersionTabIndex : 0;
			if (Gf.FindBibleBookIndex >= 0 && Gf.FindBibleBookIndex < BookLookup.Items.Count)
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
			using DataTable datatable1 = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

			if (datatable1.Rows.Count > 0)
			{
				foreach (DataRow dr in datatable1.Rows)
				{
					string songKey = DataUtil.Trim(DataUtil.GetDataString(dr, "Key"));
					if (songKey != "")
					{
						SongKey.Items.Add(songKey);
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
				foreach (DataRow dr in datatable2.Rows)
				{
					string songTiming = DataUtil.Trim(DataUtil.GetDataString(dr, "Timing"));
					if (songTiming != "")
					{
						SongTiming.Items.Add(songTiming);
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
				var options = new ItemSearchOptions
				{
					InTitle = Gf.FindItemInTitle,
					InContents = Gf.FindItemInContents,
					InSongNumber = Gf.FindItemInSongNumber,
					InBookRef = Gf.FindItemInBookRef,
					InUserRef = Gf.FindItemInUserRef,
					InLicAdmin = Gf.FindItemInLicAdmin,
					InWriter = Gf.FindItemInWriter,
					InCopyright = Gf.FindItemInCopyright,
					NotationsOnly = Gf.FindItemNotationsOnly,
					Key = Gf.FindItemWithKey,
					Timing = Gf.FindItemWithTiming,
					UseDates = Gf.FindItemUseDates,
					DateFrom = Gf.FindItemDateFrom,
					DateTo = Gf.FindItemDateTo,
				};
				Gf.Find_SQLString = Gf.BuildItemSearchString(txtName.Text, options, FolderList);
				if (Gf.Find_SQLString != "")
				{
					Gf.FindFolderItems = true;
					Gf.FindItemsRequested = true;
				}
			}
			else if (TabControl1.SelectedIndex == 1 && Gf.BibleVersions.Count >= 1)
			{
				string bibleSearchQuery = Gf.BuildBibleSearchString(MatchSelected: MatchAny.Checked ? 1 : ((!MatchAll.Checked) ? 2 : 0), InSearchPassage: PassageSearchBox.Text, VersionIndex: BibleLookup.SelectedIndex, BookIndex: BookLookup.SelectedIndex);
				if (bibleSearchQuery != "")
				{
					Gf.HB_CurVersionTabIndex = BibleLookup.SelectedIndex;
					Gf.HB_SQLString = bibleSearchQuery;
					Gf.FindBibleSearchPhrase = PassageSearchBox.Text;
					Gf.FindBibleBookIndex = BookLookup.SelectedIndex;
					Gf.FindItemsRequested = true;
				}
			}
		}

		// A DateTimePicker throws when Value is outside MinDate..MaxDate.
		private static DateTime WithinPicker(DateTimePicker picker, DateTime value)
		{
			if (value < picker.MinDate)
			{
				return picker.MinDate;
			}
			return (value > picker.MaxDate) ? picker.MaxDate : value;
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
