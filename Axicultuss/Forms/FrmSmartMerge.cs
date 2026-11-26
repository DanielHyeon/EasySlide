//using NetOffice.DAOApi;

using Axicultus.Localization;
using Axicultus.Util;
using System;
using System.ComponentModel;
using System.Data;
//using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.SQLite;
using Axicultus.Module;
#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Axicultus;

public partial class FrmSmartMerge : Form
	{
		private string SongLyricsB;

		private string SongLyricsA;

		private string SongSequenceB;

		private string SongSequenceA;

		private string SongCopyrightB;

		private string SongCopyrightA;

		private string BookReferenceB;

		private string BookReferenceA;

		private string UserReferenceB;

		private string UserReferenceA;

		private string SongWriterInfoB;

		private string SongWriterInfoA;

		private string SongLayoutSequenceB;

		private string SongLayoutSequenceA;

		private string SongTimingA;

		private string SongKeyA;

		private string SongCapoA;

		private string SongTimingB;

		private string SongKeyB;

		private string SongCapoB;

		private string SongAdminB;

		private string SongAdminA;

		private int SongID2;

		private int SongID1;

		private bool InitLoad;

		private SongSettings SmartMergeItemA = new();

		private SongSettings SmartMergeItemB = new();

		private ListView LyricsAndNotationsListA = new();

		private ListView LyricsAndNotationsListB = new();

		private ListView TempSongsList = new();

		public FrmSmartMerge()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmSmartMerge_Load(object sender, EventArgs e)
		{
			SmartMergeItemA.Initialise();
			SmartMergeItemB.Initialise();
			Gf.SetListViewColumns(LyricsAndNotationsListA, 5);
			Gf.SetListViewColumns(LyricsAndNotationsListB, 5);
			Gf.SetListViewColumns(TempSongsList, 6);
			InitLoad = true;
			BuildFolderList();
			UnselFolders();
			InitLoad = false;
			ProgressBar1.Value = 0;
		}

		private void BuildFolderList()
		{
			SongFolderA.Items.Clear();
			SongFolderB.Items.Clear();
			SongFolderC.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (Gf.Folders[i].Use > 0)
				{
					SongFolderA.Items.Add(Gf.Folders[i].Name);
					SongFolderB.Items.Add(Gf.Folders[i].Name);
					SongFolderC.Items.Add(Gf.Folders[i].Name);
				}
			}
		}

		private void UnselFolders()
		{
			SongFolderA.Text = "";
			SongFolderB.Text = "";
			SongFolderC.Text = "";
		}

		private int DisplayCount()
		{
			int count = SongsList.Items.Count;
			if (count < 1)
			{
				SongsList.Columns[0].Text = UiText.Get(nameof(FrmSmartMerge), "NoMatchingItems");
				return 0;
			}
			int count2 = SongsList.CheckedItems.Count;
			SongsList.Columns[0].Text = " " + UiText.Format(nameof(FrmSmartMerge), "ItemsIdentified", count, count2);
			return count2;
		}

		private void SongsList_KeyUp(object sender, KeyEventArgs e)
		{
			SongsListItemTicked();
		}

		private void SongsList_MouseUp(object sender, MouseEventArgs e)
		{
			SongsListItemTicked();
		}

		private void SongsListItemTicked()
		{
			if (SongsList.Items.Count == SongsList.CheckedItems.Count)
			{
				cbTickAll.CheckState = CheckState.Checked;
			}
			else if (SongsList.CheckedItems.Count == 0)
			{
				cbTickAll.CheckState = CheckState.Unchecked;
			}
			else
			{
				cbTickAll.CheckState = CheckState.Indeterminate;
			}
			SetCheckBoxes();
		}

		private void SetCheckBoxes()
		{
			if (SongsList.Items.Count == 0)
			{
				return;
			}
			if (cbTickAll.CheckState == CheckState.Checked)
			{
				for (int songIndex = SongsList.Items.Count - 1; songIndex >= 0; songIndex--)
				{
					SongsList.Items[songIndex].Checked = true;
				}
			}
			else if (cbTickAll.CheckState == CheckState.Unchecked)
			{
				for (int songIndex = SongsList.Items.Count - 1; songIndex >= 0; songIndex--)
				{
					SongsList.Items[songIndex].Checked = false;
				}
			}
			BtnOK.Enabled = ((SongsList.CheckedItems.Count > 0) ? true : false);
			DisplayCount();
		}

		private void cbTickAll_MouseUp(object sender, MouseEventArgs e)
		{
			SetCheckBoxes();
		}

		private void cbTickAll_KeyUp(object sender, KeyEventArgs e)
		{
			SetCheckBoxes();
		}

		private void OptionSourceABTitle1_CheckedChanged(object sender, EventArgs e)
		{
			BuildMatchingList();
		}

		private void OptionNewTitleA_CheckedChanged(object sender, EventArgs e)
		{
			if (SongsList.Items.Count > 0)
			{
				for (int i = 0; i < SongsList.Items.Count; i++)
				{
					SongsList.Items[i].SubItems[0].Text = SongsList.Items[i].SubItems[3].Text + (OptionNewTitleA.Checked ? (" (" + SongsList.Items[i].SubItems[4].Text + ")") : "");
				}
			}
		}

		private void BuildMatchingList()
		{
			if (InitLoad | (SongFolderA.Text == "") | (SongFolderB.Text == ""))
			{
				return;
			}
			Cursor = Cursors.WaitCursor;
			ListViewItem listViewItem = new();
			SongsList.Items.Clear();
			TempSongsList.Items.Clear();
			int sourceFolder = Gf.GetFolderNumber(SongFolderA.Text);
			int targetFolder = Gf.GetFolderNumber(SongFolderB.Text);

			try
			{
				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
				var pairs = SongQueries.FindMergePairs(connection, sourceFolder, OptionSourceATitle1.Checked,
					targetFolder, OptionSourceBTitle1.Checked);
				foreach (var pair in pairs)
				{
					string sourceTitle = pair.SourceTitle;
					string targetTitle = GetInitTitle(pair.TargetTitle);
					string mergedTitle = sourceTitle + (OptionNewTitleA.Checked ? (" (" + targetTitle + ")") : "");
					listViewItem = TempSongsList.Items.Add(DataUtil.GetCJKTitle(mergedTitle, SortBy.Alpha));
					listViewItem.SubItems.Add(mergedTitle);
					listViewItem.SubItems.Add(pair.SourceId.ToString());
					listViewItem.SubItems.Add(pair.TargetId.ToString());
					listViewItem.SubItems.Add(sourceTitle);
					listViewItem.SubItems.Add(targetTitle);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			TempSongsList.Sorting = SortOrder.Ascending;
			TempSongsList.Sort();
			if (TempSongsList.Items.Count > 0)
			{
				for (int matchIndex = 0; matchIndex < TempSongsList.Items.Count; matchIndex++)
				{
					listViewItem = SongsList.Items.Add(TempSongsList.Items[matchIndex].SubItems[1].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[matchIndex].SubItems[2].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[matchIndex].SubItems[3].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[matchIndex].SubItems[4].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[matchIndex].SubItems[5].Text);
				}
			}
			DisplayCount();
			Cursor = Cursors.Default;
		}

		private static string GetInitTitle(string InTitle)
		{
			if (DataUtil.Left(InTitle, 1) != "(")
			{
				int parenthesisIndex = InTitle.IndexOf('(');
				if (parenthesisIndex >= 0)
				{
					InTitle = DataUtil.Left(InTitle, parenthesisIndex - 1);
				}
			}
			return DataUtil.Trim(InTitle);
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Start_Merge();
		}

		private void Start_Merge()
		{
			int selectedSongCount = DisplayCount();
			if (SongFolderA.Text == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmSmartMerge), "SelectSourceA"));
				return;
			}
			if (SongFolderB.Text == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmSmartMerge), "SelectSourceB"));
				return;
			}
			if (SongFolderC.Text == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmSmartMerge), "SelectDestination"));
				return;
			}
			if (SongsList.Items.Count <= 0)
			{
				MessageBox.Show(UiText.Get(nameof(FrmSmartMerge), "NothingListed"));
				return;
			}
			if (selectedSongCount <= 0)
			{
				MessageBox.Show(UiText.Get(nameof(FrmSmartMerge), "NothingTicked"));
				return;
			}
			string CombinedLyrics = "";
			string CombinedNotations = "";
			string folderNum = Gf.GetFolderNumber(SongFolderC.Items[SongFolderC.SelectedIndex].ToString()).ToString();
			int processedSongCount = 0;
			for (int i = 0; i < SongsList.Items.Count; i++)
			{
				if (!SongsList.Items[i].Checked)
				{
					continue;
				}
				SongID1 = DataUtil.StringToInt(SongsList.Items[i].SubItems[1].Text);
				if (LookUpSong(SongID1.ToString(), ref SmartMergeItemA, ref SongLayoutSequenceA, ref SongLyricsA, ref SongSequenceA, ref SongCopyrightA, ref BookReferenceA, ref UserReferenceA, ref SongWriterInfoA, ref SongCapoA, ref SongKeyA, ref SongTimingA, ref SongAdminA))
				{
					SongID2 = DataUtil.StringToInt(SongsList.Items[i].SubItems[2].Text);
					if (LookUpSong(SongID2.ToString(), ref SmartMergeItemB, ref SongLayoutSequenceB, ref SongLyricsB, ref SongSequenceB, ref SongCopyrightB, ref BookReferenceB, ref UserReferenceB, ref SongWriterInfoB, ref SongCapoB, ref SongKeyB, ref SongTimingB, ref SongAdminB))
					{
						Gf.Merge_Songs(SmartMergeItemA, SmartMergeItemB, ref CombinedLyrics, ref CombinedNotations);
						SaveSong(SongsList.Items[i].SubItems[0].Text, folderNum, SongsList.Items[i].SubItems[3].Text, CombinedLyrics, CombinedNotations, SongSequenceA, (SongCopyrightA != "") ? SongCopyrightA : SongCopyrightB, BookReferenceA + (((BookReferenceA != "") & (BookReferenceB != "")) ? "," : "") + BookReferenceB, UserReferenceA + (((UserReferenceA != "") & (UserReferenceB != "")) ? "," : "") + UserReferenceB, (SongWriterInfoA != "") ? SongWriterInfoA : SongWriterInfoB, (SongCapoA != "-1") ? SongCapoA : SongCapoB, (SongKeyA != "") ? SongKeyA : SongKeyB, (SongTimingA != "") ? SongTimingA : SongTimingB, SongAdminA, SongAdminB);
						processedSongCount++;
					}
				}
				ProgressBar1.Value = processedSongCount / selectedSongCount * 100;
			}
			MessageBox.Show(UiText.Format(nameof(FrmSmartMerge), "MergeDone", processedSongCount, SongFolderC.Items[SongFolderC.SelectedIndex].ToString()));
			ProgressBar1.Value = 0;
		}

		private static bool LookUpSong(string InID, ref SongSettings InItem, ref string LayoutSequence, ref string Lyrics, ref string DBSequence, ref string Copyright, ref string BookReference, ref string UserReference, ref string Writer, ref string SongCapo, ref string SongKey, ref string SongTiming, ref string SongAdmin)
		{
			if (!Gf.ValidateDB(DatabaseType.Songs))
			{
				return false;
			}
			if (!Gf.ValidSongID(DataUtil.StringToInt(InID)))
			{
				return false;
			}
			try
			{
				const string fullSearchString = "select * from SONG where songid=@id";

				using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString, ("@id", InID));

				if (datatable.Rows.Count>0)
				{
					//recordSet.MoveFirst();
					DataRow dr = datatable.Rows[0];
					BookReference = DataUtil.ObjToString(dr["Book_Reference"]);
					UserReference = DataUtil.ObjToString(dr["User_Reference"]);
					Copyright = DataUtil.ObjToString(dr["Copyright"]);
					Writer = DataUtil.ObjToString(dr["Writer"]);
					SongCapo = DataUtil.ObjToInt(dr["capo"], Minus1IfBlank: true).ToString();
					SongKey = DataUtil.ObjToString(dr["Key"]);
					SongTiming = DataUtil.ObjToString(dr["Timing"]);
					SongAdmin = DataUtil.ObjToString(dr["LICENCE_ADMIN1"]);
					if (SongAdmin == "")
					{
						SongAdmin = DataUtil.ObjToString(dr["LICENCE_ADMIN2"]);
					}
					InItem.SongSequence = DataUtil.ObjToString(dr["Sequence"]);
					InItem.CompleteLyrics = DataUtil.ObjToString(dr["Lyrics"]);
					InItem.Notations = DataUtil.ObjToString(dr["msc"]);
					Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: false, UseStoredSequence: true);
					DBSequence = InItem.SongSequence;
				}
			}
			catch
			{
			}
			return true;
		}

		private static void SaveSong(string Title, string FolderNum, string Title2, string InLyrics, string InNotations, string Sequence, string Copyright, string BookReference, string UserReference, string Writer, string SongCapo, string SongKey, string SongTiming, string SongAdmin1, string SongAdmin2)
		{
			Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, Title, Title2, 0, DataUtil.StringToInt(FolderNum), InLyrics, Sequence, Writer, Copyright, SongCapo, SongTiming, SongKey, InNotations, "", SongAdmin1, SongAdmin2, BookReference, UserReference, "", "");
		}

		            }
