//using NetOffice.DAOApi;

using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
//using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Module;
#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;
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
    public class FrmSmartMerge : Form
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

		private IContainer components = null;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private Button BtnCancel;

		private Button BtnOK;

		private CheckBox cbTickAll;

		private GroupBox groupBox3;

		private ProgressBar ProgressBar1;

		private GroupBox groupBox4;

		private RadioButton OptionSourceATitle1;

		private ComboBox SongFolderA;

		private RadioButton OptionSourceATitle2;

		private GroupBox groupBox5;

		private RadioButton OptionSourceBTitle2;

		private RadioButton OptionSourceBTitle1;

		private ComboBox SongFolderB;

		private GroupBox groupBox6;

		private RadioButton OptionNewTitleAB;

		private RadioButton OptionNewTitleA;

		private ComboBox SongFolderC;

		private ListView SongsList;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader2;

		private ColumnHeader columnHeader3;

		private ColumnHeader columnHeader4;

		private ColumnHeader columnHeader5;

		public FrmSmartMerge()
		{
			InitializeComponent();
		}

		private void FrmSmartMerge_Load(object sender, EventArgs e)
		{
			SmartMergeItemA.Initialise();
			SmartMergeItemB.Initialise();
			gf.SetListViewColumns(LyricsAndNotationsListA, 5);
			gf.SetListViewColumns(LyricsAndNotationsListB, 5);
			gf.SetListViewColumns(TempSongsList, 6);
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
			for (int i = 1; i < 41; i++)
			{
				if (gf.FolderUse[i] > 0)
				{
					SongFolderA.Items.Add(gf.FolderName[i]);
					SongFolderB.Items.Add(gf.FolderName[i]);
					SongFolderC.Items.Add(gf.FolderName[i]);
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
			inName = "select * from SONG where Folderno=" + gf.GetFolderNumber(inName);
			text = ((!OptionSourceBTitle1.Checked) ? "Title_2" : "Title_1");
			int num = 0;
			int num2 = 0;
#if OleDb
			try
			{
				using(DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, inName))
				{
					using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB))
					{
						if (datatable.Rows.Count > 0)
						{
							//recordSet.MoveFirst();
							//while (!recordSet.EOF)
							foreach (DataRow dr in datatable.Rows)
							{
								SongID1 = DataUtil.ObjToInt(dr["SongID"]);
								string text2 = (!OptionSourceATitle1.Checked) ? DataUtil.ObjToString(dr["Title_2"]) : DataUtil.ObjToString(dr["Title_1"]);
								if (DataUtil.Trim(text2) != "")
								{
									string text3 = DataUtil.ObjToString(dr["Title_1"]);
									string inName2 = SongFolderB.Items[SongFolderB.SelectedIndex].ToString();
									inName2 = "select * from SONG where Folderno=" + gf.GetFolderNumber(inName2) + " and LCase(" + text + ") like LCase(\"" + text2 + "\") ";
									try
									{
										DataTable recordSet2 = DbOleDbController.getDataTable(daoDb, inName2);
										if (recordSet2.Rows.Count > 0)
										{
											//recordSet2.MoveFirst();
											//while (!recordSet2.EOF)
											foreach (DataRow dr1 in recordSet2.Rows)
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
												//recordSet2.MoveNext();
											}
										}
									}
									catch (Exception e)
									{
										Console.WriteLine(e.Message);
										Console.WriteLine(e.StackTrace);
									}
								}
								//recordSet.MoveNext();
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
#elif SQLite
			try
			{
				using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
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
							inName2 = "select * from SONG where Folderno=" + gf.GetFolderNumber(inName2) + " and LCase(" + text + ") like LCase(\"" + text2 + "\") ";
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
#endif
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
			string folderNum = gf.GetFolderNumber(SongFolderC.Items[SongFolderC.SelectedIndex].ToString()).ToString();
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
						gf.Merge_Songs(SmartMergeItemA, SmartMergeItemB, ref CombinedLyrics, ref CombinedNotations);
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
			if (!gf.ValidateDB(DatabaseType.Songs))
			{
				return false;
			}
			if (!gf.ValidSongID(DataUtil.StringToInt(InID)))
			{
				return false;
			}
			try
			{
				string fullSearchString = "select * from SONG where songid=" + InID;

#if OleDb
			    using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif
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
					gf.FormatDisplayLyrics(ref InItem, PrepareSlides: false, UseStoredSequence: true);
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
			int num = gf.InsertItemIntoDatabase(gf.ConnectStringMainDB, Title, Title2, 0, DataUtil.StringToInt(FolderNum), InLyrics, Sequence, Writer, Copyright, SongCapo, SongTiming, SongKey, InNotations, "", SongAdmin1, SongAdmin2, BookReference, UserReference, "", "");
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSmartMerge));
			groupBox1 = new System.Windows.Forms.GroupBox();
			groupBox5 = new System.Windows.Forms.GroupBox();
			OptionSourceBTitle2 = new System.Windows.Forms.RadioButton();
			OptionSourceBTitle1 = new System.Windows.Forms.RadioButton();
			SongFolderB = new System.Windows.Forms.ComboBox();
			groupBox3 = new System.Windows.Forms.GroupBox();
			OptionSourceATitle2 = new System.Windows.Forms.RadioButton();
			OptionSourceATitle1 = new System.Windows.Forms.RadioButton();
			SongFolderA = new System.Windows.Forms.ComboBox();
			groupBox2 = new System.Windows.Forms.GroupBox();
			groupBox6 = new System.Windows.Forms.GroupBox();
			OptionNewTitleAB = new System.Windows.Forms.RadioButton();
			OptionNewTitleA = new System.Windows.Forms.RadioButton();
			SongFolderC = new System.Windows.Forms.ComboBox();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			cbTickAll = new System.Windows.Forms.CheckBox();
			ProgressBar1 = new System.Windows.Forms.ProgressBar();
			groupBox4 = new System.Windows.Forms.GroupBox();
			SongsList = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			groupBox1.SuspendLayout();
			groupBox5.SuspendLayout();
			groupBox3.SuspendLayout();
			groupBox2.SuspendLayout();
			groupBox6.SuspendLayout();
			groupBox4.SuspendLayout();
			SuspendLayout();
			groupBox1.Controls.Add(groupBox5);
			groupBox1.Controls.Add(groupBox3);
			groupBox1.Location = new System.Drawing.Point(12, 12);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(332, 114);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Identify Matching Titles from Source A and Source B";
			groupBox5.Controls.Add(OptionSourceBTitle2);
			groupBox5.Controls.Add(OptionSourceBTitle1);
			groupBox5.Controls.Add(SongFolderB);
			groupBox5.Location = new System.Drawing.Point(169, 19);
			groupBox5.Name = "groupBox5";
			groupBox5.Size = new System.Drawing.Size(153, 89);
			groupBox5.TabIndex = 1;
			groupBox5.TabStop = false;
			groupBox5.Text = "Source B (Region 2)";
			OptionSourceBTitle2.AutoSize = true;
			OptionSourceBTitle2.Location = new System.Drawing.Point(17, 61);
			OptionSourceBTitle2.Name = "OptionSourceBTitle2";
			OptionSourceBTitle2.Size = new System.Drawing.Size(54, 17);
			OptionSourceBTitle2.TabIndex = 2;
			OptionSourceBTitle2.Text = "Title 2";
			OptionSourceBTitle1.AutoSize = true;
			OptionSourceBTitle1.Checked = true;
			OptionSourceBTitle1.Location = new System.Drawing.Point(17, 43);
			OptionSourceBTitle1.Name = "OptionSourceBTitle1";
			OptionSourceBTitle1.Size = new System.Drawing.Size(54, 17);
			OptionSourceBTitle1.TabIndex = 1;
			OptionSourceBTitle1.TabStop = true;
			OptionSourceBTitle1.Text = "Title 1";
			OptionSourceBTitle1.CheckedChanged += new System.EventHandler(OptionSourceABTitle1_CheckedChanged);
			SongFolderB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SongFolderB.FormattingEnabled = true;
			SongFolderB.Location = new System.Drawing.Point(6, 19);
			SongFolderB.MaxDropDownItems = 12;
			SongFolderB.Name = "SongFolderB";
			SongFolderB.Size = new System.Drawing.Size(141, 21);
			SongFolderB.TabIndex = 0;
			SongFolderB.SelectedIndexChanged += new System.EventHandler(OptionSourceABTitle1_CheckedChanged);
			groupBox3.Controls.Add(OptionSourceATitle2);
			groupBox3.Controls.Add(OptionSourceATitle1);
			groupBox3.Controls.Add(SongFolderA);
			groupBox3.Location = new System.Drawing.Point(8, 19);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new System.Drawing.Size(153, 89);
			groupBox3.TabIndex = 0;
			groupBox3.TabStop = false;
			groupBox3.Text = "Source A (Region 1)";
			OptionSourceATitle2.AutoSize = true;
			OptionSourceATitle2.Checked = true;
			OptionSourceATitle2.Location = new System.Drawing.Point(17, 61);
			OptionSourceATitle2.Name = "OptionSourceATitle2";
			OptionSourceATitle2.Size = new System.Drawing.Size(54, 17);
			OptionSourceATitle2.TabIndex = 2;
			OptionSourceATitle2.TabStop = true;
			OptionSourceATitle2.Text = "Title 2";
			OptionSourceATitle1.AutoSize = true;
			OptionSourceATitle1.Location = new System.Drawing.Point(17, 43);
			OptionSourceATitle1.Name = "OptionSourceATitle1";
			OptionSourceATitle1.Size = new System.Drawing.Size(54, 17);
			OptionSourceATitle1.TabIndex = 1;
			OptionSourceATitle1.Text = "Title 1";
			OptionSourceATitle1.CheckedChanged += new System.EventHandler(OptionSourceABTitle1_CheckedChanged);
			SongFolderA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SongFolderA.FormattingEnabled = true;
			SongFolderA.Location = new System.Drawing.Point(6, 19);
			SongFolderA.MaxDropDownItems = 12;
			SongFolderA.Name = "SongFolderA";
			SongFolderA.Size = new System.Drawing.Size(141, 21);
			SongFolderA.TabIndex = 0;
			SongFolderA.SelectedIndexChanged += new System.EventHandler(OptionSourceABTitle1_CheckedChanged);
			groupBox2.Controls.Add(groupBox6);
			groupBox2.Location = new System.Drawing.Point(347, 12);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new System.Drawing.Size(169, 114);
			groupBox2.TabIndex = 1;
			groupBox2.TabStop = false;
			groupBox2.Text = "Merge Matching Items into";
			groupBox6.Controls.Add(OptionNewTitleAB);
			groupBox6.Controls.Add(OptionNewTitleA);
			groupBox6.Controls.Add(SongFolderC);
			groupBox6.Location = new System.Drawing.Point(8, 19);
			groupBox6.Name = "groupBox6";
			groupBox6.Size = new System.Drawing.Size(153, 89);
			groupBox6.TabIndex = 0;
			groupBox6.TabStop = false;
			groupBox6.Text = "Destination Folder";
			OptionNewTitleAB.AutoSize = true;
			OptionNewTitleAB.Location = new System.Drawing.Point(17, 61);
			OptionNewTitleAB.Name = "OptionNewTitleAB";
			OptionNewTitleAB.Size = new System.Drawing.Size(116, 17);
			OptionNewTitleAB.TabIndex = 4;
			OptionNewTitleAB.Text = "Source A Title Only";
			OptionNewTitleA.AutoSize = true;
			OptionNewTitleA.Checked = true;
			OptionNewTitleA.Location = new System.Drawing.Point(17, 43);
			OptionNewTitleA.Name = "OptionNewTitleA";
			OptionNewTitleA.Size = new System.Drawing.Size(112, 17);
			OptionNewTitleA.TabIndex = 1;
			OptionNewTitleA.TabStop = true;
			OptionNewTitleA.Text = "Merge Titles A && B";
			OptionNewTitleA.CheckedChanged += new System.EventHandler(OptionNewTitleA_CheckedChanged);
			SongFolderC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SongFolderC.FormattingEnabled = true;
			SongFolderC.Location = new System.Drawing.Point(6, 19);
			SongFolderC.MaxDropDownItems = 12;
			SongFolderC.Name = "SongFolderC";
			SongFolderC.Size = new System.Drawing.Size(141, 21);
			SongFolderC.TabIndex = 0;
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(436, 355);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 6;
			BtnCancel.Text = "Close";
			BtnOK.Location = new System.Drawing.Point(340, 355);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 5;
			BtnOK.Text = "Start Merge";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			cbTickAll.AutoSize = true;
			cbTickAll.Location = new System.Drawing.Point(21, 355);
			cbTickAll.Name = "cbTickAll";
			cbTickAll.Size = new System.Drawing.Size(61, 17);
			cbTickAll.TabIndex = 4;
			cbTickAll.Text = "Tick All";
			cbTickAll.ThreeState = true;
			cbTickAll.KeyUp += new System.Windows.Forms.KeyEventHandler(cbTickAll_KeyUp);
			cbTickAll.MouseUp += new System.Windows.Forms.MouseEventHandler(cbTickAll_MouseUp);
			ProgressBar1.Location = new System.Drawing.Point(12, 326);
			ProgressBar1.Name = "ProgressBar1";
			ProgressBar1.Size = new System.Drawing.Size(504, 21);
			ProgressBar1.TabIndex = 3;
			groupBox4.Controls.Add(SongsList);
			groupBox4.Location = new System.Drawing.Point(12, 127);
			groupBox4.Name = "groupBox4";
			groupBox4.Size = new System.Drawing.Size(504, 198);
			groupBox4.TabIndex = 2;
			groupBox4.TabStop = false;
			groupBox4.Text = "Matching Items Found";
			SongsList.CheckBoxes = true;
			SongsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5]
			{
				columnHeader1,
				columnHeader2,
				columnHeader3,
				columnHeader4,
				columnHeader5
			});
			SongsList.FullRowSelect = true;
			SongsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			SongsList.Location = new System.Drawing.Point(6, 19);
			SongsList.Name = "SongsList";
			SongsList.Size = new System.Drawing.Size(492, 174);
			SongsList.TabIndex = 0;
			SongsList.UseCompatibleStateImageBehavior = false;
			SongsList.View = System.Windows.Forms.View.Details;
			SongsList.MouseUp += new System.Windows.Forms.MouseEventHandler(SongsList_MouseUp);
			SongsList.KeyUp += new System.Windows.Forms.KeyEventHandler(SongsList_KeyUp);
			columnHeader1.Text = "Items";
			columnHeader1.Width = 470;
			columnHeader2.Text = "Song A ID";
			columnHeader2.Width = 0;
			columnHeader3.Text = "Song B ID";
			columnHeader3.Width = 0;
			columnHeader4.Text = "Title B";
			columnHeader4.Width = 0;
			columnHeader5.Text = "Title A";
			columnHeader5.Width = 0;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(528, 391);
			base.Controls.Add(groupBox4);
			base.Controls.Add(ProgressBar1);
			base.Controls.Add(cbTickAll);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(groupBox2);
			base.Controls.Add(groupBox1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmSmartMerge";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Smart Merge";
			base.Load += new System.EventHandler(FrmSmartMerge_Load);
			groupBox1.ResumeLayout(false);
			groupBox5.ResumeLayout(false);
			groupBox5.PerformLayout();
			groupBox3.ResumeLayout(false);
			groupBox3.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox6.ResumeLayout(false);
			groupBox6.PerformLayout();
			groupBox4.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
