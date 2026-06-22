//using NetOffice.DAOApi;

using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
//using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Module;
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

namespace Easislides
{
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

		private SongSettings SmartMergeItemA = new SongSettings();

		private SongSettings SmartMergeItemB = new SongSettings();

		private ListView LyricsAndNotationsListA = new ListView();

		private ListView LyricsAndNotationsListB = new ListView();

		private ListView TempSongsList = new ListView();

		public FrmSmartMerge()
		{
			InitializeComponent();
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
				if (Gf.FolderUse[i] > 0)
				{
					SongFolderA.Items.Add(Gf.FolderName[i]);
					SongFolderB.Items.Add(Gf.FolderName[i]);
					SongFolderC.Items.Add(Gf.FolderName[i]);
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
				SongsList.Columns[0].Text = "No Matching Items found";
				return 0;
			}
			int count2 = SongsList.CheckedItems.Count;
			SongsList.Columns[0].Text = " " + count + " items identified : " + Convert.ToString(count2) + " ticked for Merging";
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
				for (int num = SongsList.Items.Count - 1; num >= 0; num--)
				{
					SongsList.Items[num].Checked = true;
				}
			}
			else if (cbTickAll.CheckState == CheckState.Unchecked)
			{
				for (int num = SongsList.Items.Count - 1; num >= 0; num--)
				{
					SongsList.Items[num].Checked = false;
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
			ListViewItem listViewItem = new ListViewItem();
			SongsList.Items.Clear();
			TempSongsList.Items.Clear();
			string text;
			if (OptionSourceATitle1.Checked)
			{
				text = "Title_1";
			}
			else
			{
				text = "Title_2";
			}
			string inName = SongFolderA.Items[SongFolderA.SelectedIndex].ToString();
			inName = "select * from SONG where Folderno=" + Gf.GetFolderNumber(inName);
			text = ((!OptionSourceBTitle1.Checked) ? "Title_2" : "Title_1");
			int num = 0;
			int num2 = 0;

			try
			{
				using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(connection, inName);

				if (dataTable.Rows.Count > 0)
				{
					foreach (DataRow dr in dataTable.Rows)
					{
						SongID1 = DataUtil.ObjToInt(dr["SongID"]);
						string text2 = (!OptionSourceATitle1.Checked) ? DataUtil.ObjToString(dr["Title_2"]) : DataUtil.ObjToString(dr["Title_1"]);
						if (DataUtil.Trim(text2) != "")
						{
							string text3 = DataUtil.ObjToString(dr["Title_1"]);
							string inName2 = SongFolderB.Items[SongFolderB.SelectedIndex].ToString();
							inName2 = "select * from SONG where Folderno=" + Gf.GetFolderNumber(inName2) + " and LCase(" + text + ") like LCase(\"" + text2 + "\") ";
							try
							{
								using DataTable dataTable1 = DbController.GetDataTable(connection, inName2);
								if (dataTable1.Rows.Count > 0)
								{
									foreach (DataRow dr1 in dataTable1.Rows)
									{
										SongID2 = DataUtil.ObjToInt(dr1["SongID"]);
										num2++;
										string initTitle = GetInitTitle(DataUtil.ObjToString(dr1["Title_1"]));
										string text4 = text3 + (OptionNewTitleA.Checked ? (" (" + initTitle + ")") : "");
										listViewItem = TempSongsList.Items.Add(DataUtil.GetCJKTitle(text4, SortBy.Alpha));
										listViewItem.SubItems.Add(text4);
										listViewItem.SubItems.Add(SongID1.ToString());
										listViewItem.SubItems.Add(SongID2.ToString());
										listViewItem.SubItems.Add(text3);
										listViewItem.SubItems.Add(initTitle);
									}
								}
							}
							catch (Exception e)
							{
								Console.WriteLine(e.Message);
								Console.WriteLine(e.StackTrace);
							}
						}
					}
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
				for (num = 0; num < TempSongsList.Items.Count; num++)
				{
					listViewItem = SongsList.Items.Add(TempSongsList.Items[num].SubItems[1].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[num].SubItems[2].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[num].SubItems[3].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[num].SubItems[4].Text);
					listViewItem.SubItems.Add(TempSongsList.Items[num].SubItems[5].Text);
				}
			}
			DisplayCount();
			Cursor = Cursors.Default;
		}

		private string GetInitTitle(string InTitle)
		{
			if (DataUtil.Left(InTitle, 1) != "(")
			{
				int num = InTitle.IndexOf("(");
				if (num >= 0)
				{
					InTitle = DataUtil.Left(InTitle, num - 1);
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
			int num = DisplayCount();
			if (SongFolderA.Text == "")
			{
				MessageBox.Show("Please select a folder for Source A before proceeding");
				return;
			}
			if (SongFolderB.Text == "")
			{
				MessageBox.Show("Please select a folder for Source B before proceeding");
				return;
			}
			if (SongFolderC.Text == "")
			{
				MessageBox.Show("Please select a Destination Folder to hold the new merged items");
				return;
			}
			if (SongsList.Items.Count <= 0)
			{
				MessageBox.Show("There aren't any items listed for merging!");
				return;
			}
			if (num <= 0)
			{
				MessageBox.Show("Cannot find any items Ticked for merging!");
				return;
			}
			string CombinedLyrics = "";
			string CombinedNotations = "";
			string folderNum = Gf.GetFolderNumber(SongFolderC.Items[SongFolderC.SelectedIndex].ToString()).ToString();
			int num2 = 0;
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
						num2++;
					}
				}
				ProgressBar1.Value = num2 / num * 100;
			}
			MessageBox.Show(num2 + " newly merged items have been created in Folder '" + SongFolderC.Items[SongFolderC.SelectedIndex].ToString() + "'");
			ProgressBar1.Value = 0;
		}

		private bool LookUpSong(string InID, ref SongSettings InItem, ref string LayoutSequence, ref string Lyrics, ref string DBSequence, ref string Copyright, ref string BookReference, ref string UserReference, ref string Writer, ref string SongCapo, ref string SongKey, ref string SongTiming, ref string SongAdmin)
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
				string fullSearchString = "select * from SONG where songid=" + InID;

				using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

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

		private void SaveSong(string Title, string FolderNum, string Title2, string InLyrics, string InNotations, string Sequence, string Copyright, string BookReference, string UserReference, string Writer, string SongCapo, string SongKey, string SongTiming, string SongAdmin1, string SongAdmin2)
		{
			int num = Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, Title, Title2, 0, DataUtil.StringToInt(FolderNum), InLyrics, Sequence, Writer, Copyright, SongCapo, SongTiming, SongKey, InNotations, "", SongAdmin1, SongAdmin2, BookReference, UserReference, "", "");
		}

		            }
}
