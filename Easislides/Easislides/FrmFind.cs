//using NetOffice.DAOApi;
using Easislides.SQLite;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmFind : Form
	{
		private const int WordListMax = 20;

		private IContainer components = null;

		private TabControl TabControl1;

		private TabPage tabPage1;

		private TabPage tabPage2;

		private Button BtnCancel;

		private Button BtnOK;

		private CheckedListBox FolderList;

		private TextBox txtName;

		private Label label1;

		private GroupBox groupBox2;

		private GroupBox groupBox1;

		private TextBox PassageSearchBox;

		private Label label2;

		private ComboBox BibleLookup;

		private Label label3;

		private Label label4;

		private ComboBox BookLookup;

		private RadioButton MatchPhrase;

		private RadioButton MatchAny;

		private RadioButton MatchAll;

		private ComboBox SongKey;

		private CheckBox cbLicAdmin;

		private CheckBox cbUserRef;

		private CheckBox cbBookRef;

		private CheckBox cbSongNumber;

		private CheckBox cbContents;

		private CheckBox cbTitle;

		private CheckBox cbCopyright;

		private CheckBox cbWriter;

		private CheckBox cbNotationsOnly;

		private CheckBox cbMusicOnly;

		private Label label5;

		private Timer TimerRestoreWindow;

		private DateTimePicker CalendarTo;

		private DateTimePicker CalendarFrom;

		private CheckBox cbUseDates;

		private Label label6;

		private Label label7;

		private ComboBox SongTiming;

		private bool InitFormLoad = true;

		private string[] WordList = new string[20];

		private string Reg_FormLeft = "SearchFormLeft";

		private string Reg_FormTop = "SearchFormTop";

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFind));
			TabControl1 = new System.Windows.Forms.TabControl();
			tabPage1 = new System.Windows.Forms.TabPage();
			CalendarTo = new System.Windows.Forms.DateTimePicker();
			CalendarFrom = new System.Windows.Forms.DateTimePicker();
			groupBox2 = new System.Windows.Forms.GroupBox();
			SongTiming = new System.Windows.Forms.ComboBox();
			label7 = new System.Windows.Forms.Label();
			cbNotationsOnly = new System.Windows.Forms.CheckBox();
			cbMusicOnly = new System.Windows.Forms.CheckBox();
			SongKey = new System.Windows.Forms.ComboBox();
			label5 = new System.Windows.Forms.Label();
			groupBox1 = new System.Windows.Forms.GroupBox();
			cbCopyright = new System.Windows.Forms.CheckBox();
			cbWriter = new System.Windows.Forms.CheckBox();
			cbLicAdmin = new System.Windows.Forms.CheckBox();
			cbUserRef = new System.Windows.Forms.CheckBox();
			cbBookRef = new System.Windows.Forms.CheckBox();
			cbSongNumber = new System.Windows.Forms.CheckBox();
			cbContents = new System.Windows.Forms.CheckBox();
			cbTitle = new System.Windows.Forms.CheckBox();
			FolderList = new System.Windows.Forms.CheckedListBox();
			txtName = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			cbUseDates = new System.Windows.Forms.CheckBox();
			label6 = new System.Windows.Forms.Label();
			tabPage2 = new System.Windows.Forms.TabPage();
			MatchPhrase = new System.Windows.Forms.RadioButton();
			MatchAny = new System.Windows.Forms.RadioButton();
			MatchAll = new System.Windows.Forms.RadioButton();
			BookLookup = new System.Windows.Forms.ComboBox();
			BibleLookup = new System.Windows.Forms.ComboBox();
			PassageSearchBox = new System.Windows.Forms.TextBox();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			TimerRestoreWindow = new System.Windows.Forms.Timer(components);
			TabControl1.SuspendLayout();
			tabPage1.SuspendLayout();
			groupBox2.SuspendLayout();
			groupBox1.SuspendLayout();
			tabPage2.SuspendLayout();
			SuspendLayout();
			TabControl1.Controls.Add(tabPage1);
			TabControl1.Controls.Add(tabPage2);
			TabControl1.Location = new System.Drawing.Point(12, 12);
			TabControl1.Name = "TabControl1";
			TabControl1.SelectedIndex = 0;
			TabControl1.Size = new System.Drawing.Size(390, 256);
			TabControl1.TabIndex = 0;
			tabPage1.Controls.Add(CalendarTo);
			tabPage1.Controls.Add(CalendarFrom);
			tabPage1.Controls.Add(groupBox2);
			tabPage1.Controls.Add(groupBox1);
			tabPage1.Controls.Add(FolderList);
			tabPage1.Controls.Add(txtName);
			tabPage1.Controls.Add(label1);
			tabPage1.Controls.Add(cbUseDates);
			tabPage1.Controls.Add(label6);
			tabPage1.Location = new System.Drawing.Point(4, 22);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(382, 230);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "Folder Search";
			CalendarTo.Location = new System.Drawing.Point(222, 201);
			CalendarTo.Name = "CalendarTo";
			CalendarTo.Size = new System.Drawing.Size(154, 20);
			CalendarTo.TabIndex = 7;
			CalendarFrom.Location = new System.Drawing.Point(221, 166);
			CalendarFrom.Name = "CalendarFrom";
			CalendarFrom.Size = new System.Drawing.Size(155, 20);
			CalendarFrom.TabIndex = 5;
			groupBox2.Controls.Add(SongTiming);
			groupBox2.Controls.Add(label7);
			groupBox2.Controls.Add(cbNotationsOnly);
			groupBox2.Controls.Add(cbMusicOnly);
			groupBox2.Controls.Add(SongKey);
			groupBox2.Controls.Add(label5);
			groupBox2.Location = new System.Drawing.Point(221, 49);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new System.Drawing.Size(155, 96);
			groupBox2.TabIndex = 3;
			groupBox2.TabStop = false;
			groupBox2.Text = "Restrict To";
			SongTiming.FormattingEnabled = true;
			SongTiming.Location = new System.Drawing.Point(52, 73);
			SongTiming.Name = "SongTiming";
			SongTiming.Size = new System.Drawing.Size(71, 21);
			SongTiming.TabIndex = 10;
			label7.AutoSize = true;
			label7.Location = new System.Drawing.Point(6, 76);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(41, 13);
			label7.TabIndex = 9;
			label7.Text = "Timing:";
			cbNotationsOnly.AutoSize = true;
			cbNotationsOnly.Location = new System.Drawing.Point(8, 34);
			cbNotationsOnly.Name = "cbNotationsOnly";
			cbNotationsOnly.Size = new System.Drawing.Size(96, 17);
			cbNotationsOnly.TabIndex = 1;
			cbNotationsOnly.Text = "With Notations";
			cbMusicOnly.AutoSize = true;
			cbMusicOnly.Location = new System.Drawing.Point(8, 17);
			cbMusicOnly.Name = "cbMusicOnly";
			cbMusicOnly.Size = new System.Drawing.Size(98, 17);
			cbMusicOnly.TabIndex = 0;
			cbMusicOnly.Text = "With Music File";
			SongKey.FormattingEnabled = true;
			SongKey.Location = new System.Drawing.Point(52, 52);
			SongKey.Name = "SongKey";
			SongKey.Size = new System.Drawing.Size(71, 21);
			SongKey.TabIndex = 3;
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(6, 54);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(28, 13);
			label5.TabIndex = 2;
			label5.Text = "Key:";
			groupBox1.Controls.Add(cbCopyright);
			groupBox1.Controls.Add(cbWriter);
			groupBox1.Controls.Add(cbLicAdmin);
			groupBox1.Controls.Add(cbUserRef);
			groupBox1.Controls.Add(cbBookRef);
			groupBox1.Controls.Add(cbSongNumber);
			groupBox1.Controls.Add(cbContents);
			groupBox1.Controls.Add(cbTitle);
			groupBox1.Location = new System.Drawing.Point(125, 49);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(90, 174);
			groupBox1.TabIndex = 2;
			groupBox1.TabStop = false;
			groupBox1.Text = "Search On";
			cbCopyright.AutoSize = true;
			cbCopyright.Location = new System.Drawing.Point(8, 84);
			cbCopyright.Name = "cbCopyright";
			cbCopyright.Size = new System.Drawing.Size(70, 17);
			cbCopyright.TabIndex = 4;
			cbCopyright.Text = "Copyright";
			cbWriter.AutoSize = true;
			cbWriter.Location = new System.Drawing.Point(8, 67);
			cbWriter.Name = "cbWriter";
			cbWriter.Size = new System.Drawing.Size(54, 17);
			cbWriter.TabIndex = 3;
			cbWriter.Text = "Writer";
			cbLicAdmin.AutoSize = true;
			cbLicAdmin.Location = new System.Drawing.Point(8, 134);
			cbLicAdmin.Name = "cbLicAdmin";
			cbLicAdmin.Size = new System.Drawing.Size(72, 17);
			cbLicAdmin.TabIndex = 7;
			cbLicAdmin.Text = "Lic Admin";
			cbUserRef.AutoSize = true;
			cbUserRef.Location = new System.Drawing.Point(8, 117);
			cbUserRef.Name = "cbUserRef";
			cbUserRef.Size = new System.Drawing.Size(68, 17);
			cbUserRef.TabIndex = 6;
			cbUserRef.Text = "User Ref";
			cbBookRef.AutoSize = true;
			cbBookRef.Location = new System.Drawing.Point(8, 100);
			cbBookRef.Name = "cbBookRef";
			cbBookRef.Size = new System.Drawing.Size(71, 17);
			cbBookRef.TabIndex = 5;
			cbBookRef.Text = "Book Ref";
			cbSongNumber.AutoSize = true;
			cbSongNumber.Location = new System.Drawing.Point(8, 50);
			cbSongNumber.Name = "cbSongNumber";
			cbSongNumber.Size = new System.Drawing.Size(71, 17);
			cbSongNumber.TabIndex = 2;
			cbSongNumber.Text = "Song No.";
			cbContents.AutoSize = true;
			cbContents.Location = new System.Drawing.Point(8, 33);
			cbContents.Name = "cbContents";
			cbContents.Size = new System.Drawing.Size(68, 17);
			cbContents.TabIndex = 1;
			cbContents.Text = "Contents";
			cbTitle.AutoSize = true;
			cbTitle.Location = new System.Drawing.Point(8, 16);
			cbTitle.Name = "cbTitle";
			cbTitle.Size = new System.Drawing.Size(46, 17);
			cbTitle.TabIndex = 0;
			cbTitle.Text = "Title";
			FolderList.CheckOnClick = true;
			FolderList.FormattingEnabled = true;
			FolderList.Location = new System.Drawing.Point(7, 54);
			FolderList.Name = "FolderList";
			FolderList.Size = new System.Drawing.Size(112, 169);
			FolderList.TabIndex = 1;
			txtName.Location = new System.Drawing.Point(7, 28);
			txtName.Name = "txtName";
			txtName.Size = new System.Drawing.Size(369, 20);
			txtName.TabIndex = 0;
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(9, 12);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(105, 13);
			label1.TabIndex = 1;
			label1.Text = "Enter search phrase:";
			cbUseDates.AutoSize = true;
			cbUseDates.Location = new System.Drawing.Point(224, 148);
			cbUseDates.Name = "cbUseDates";
			cbUseDates.Size = new System.Drawing.Size(104, 17);
			cbUseDates.TabIndex = 4;
			cbUseDates.Text = "Search between";
			cbUseDates.CheckedChanged += new System.EventHandler(cbUseDates_CheckedChanged);
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(221, 186);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(25, 13);
			label6.TabIndex = 6;
			label6.Text = "and";
			tabPage2.Controls.Add(MatchPhrase);
			tabPage2.Controls.Add(MatchAny);
			tabPage2.Controls.Add(MatchAll);
			tabPage2.Controls.Add(BookLookup);
			tabPage2.Controls.Add(BibleLookup);
			tabPage2.Controls.Add(PassageSearchBox);
			tabPage2.Controls.Add(label3);
			tabPage2.Controls.Add(label4);
			tabPage2.Controls.Add(label2);
			tabPage2.Location = new System.Drawing.Point(4, 22);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(382, 230);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Bible Search";
			MatchPhrase.AutoSize = true;
			MatchPhrase.Location = new System.Drawing.Point(16, 170);
			MatchPhrase.Name = "MatchPhrase";
			MatchPhrase.Size = new System.Drawing.Size(144, 17);
			MatchPhrase.TabIndex = 5;
			MatchPhrase.Text = "Match the phrase exactly";
			MatchAny.AutoSize = true;
			MatchAny.Location = new System.Drawing.Point(16, 153);
			MatchAny.Name = "MatchAny";
			MatchAny.Size = new System.Drawing.Size(136, 17);
			MatchAny.TabIndex = 4;
			MatchAny.Text = "Match any of the words";
			MatchAll.AutoSize = true;
			MatchAll.Checked = true;
			MatchAll.Location = new System.Drawing.Point(16, 136);
			MatchAll.Name = "MatchAll";
			MatchAll.Size = new System.Drawing.Size(117, 17);
			MatchAll.TabIndex = 3;
			MatchAll.TabStop = true;
			MatchAll.Text = "Match all the words";
			BookLookup.FormattingEnabled = true;
			BookLookup.Location = new System.Drawing.Point(7, 68);
			BookLookup.Name = "BookLookup";
			BookLookup.Size = new System.Drawing.Size(178, 21);
			BookLookup.TabIndex = 1;
			BibleLookup.FormattingEnabled = true;
			BibleLookup.Location = new System.Drawing.Point(7, 28);
			BibleLookup.Name = "BibleLookup";
			BibleLookup.Size = new System.Drawing.Size(262, 21);
			BibleLookup.TabIndex = 0;
			BibleLookup.SelectedIndexChanged += new System.EventHandler(BibleLookup_SelectedIndexChanged);
			PassageSearchBox.Location = new System.Drawing.Point(7, 109);
			PassageSearchBox.Name = "PassageSearchBox";
			PassageSearchBox.Size = new System.Drawing.Size(359, 20);
			PassageSearchBox.TabIndex = 2;
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(9, 12);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(71, 13);
			label3.TabIndex = 5;
			label3.Text = "Bible Version:";
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(9, 52);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(35, 13);
			label4.TabIndex = 7;
			label4.Text = "Book:";
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(9, 93);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(105, 13);
			label2.TabIndex = 2;
			label2.Text = "Enter search phrase:";
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(217, 276);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 1;
			BtnCancel.Text = "Close";
			BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
			BtnOK.Location = new System.Drawing.Point(131, 276);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 0;
			BtnOK.Text = "Search";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			TimerRestoreWindow.Tick += new System.EventHandler(TimerRestoreWindow_Tick);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(414, 312);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(TabControl1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.Name = "FrmFind";
			Text = "Search for EasiSlides Items";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmFind_FormClosing);
			base.Load += new System.EventHandler(FrmFind_Load);
			TabControl1.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			tabPage2.ResumeLayout(false);
			tabPage2.PerformLayout();
			ResumeLayout(false);
		}

		public FrmFind()
		{
			InitializeComponent();
		}

		/// <summary>
		/// daniel °Ë»ö
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FrmFind_Load(object sender, EventArgs e)
		{
			gf.FindItemsFormOpen = true;
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
			for (int i = 1; i < 41; i++)
			{
				if ((gf.FolderName[i].ToString() != "") & (gf.FolderUse[i] > 0))
				{
					FolderList.Items.Add(gf.FolderName[i]);
					if (gf.FindSongsFolder[gf.GetFolderNumber(gf.FolderName[i])])
					{
						FolderList.SetItemChecked(FolderList.Items.Count - 1, value: true);
					}
				}
			}
			gf.Find_SQLString = "";
			txtName.Text = gf.FindSearchPhrase;
			cbTitle.Checked = gf.FindItemInTitle;
			cbContents.Checked = gf.FindItemInContents;
			cbSongNumber.Checked = gf.FindItemInSongNumber;
			cbBookRef.Checked = gf.FindItemInBookRef;
			cbUserRef.Checked = gf.FindItemInUserRef;
			cbLicAdmin.Checked = gf.FindItemInLicAdmin;
			cbWriter.Checked = gf.FindItemInWriter;
			cbCopyright.Checked = gf.FindItemInCopyright;
			cbMusicOnly.Checked = gf.FindItemMediaOnly;
			cbNotationsOnly.Checked = gf.FindItemNotationsOnly;
			cbUseDates.Checked = gf.FindItemUseDates;
			CalendarFrom.Value = DateTime.Parse(gf.FindItemDateFrom.ToShortTimeString());
			CalendarTo.Value = DateTime.Parse(gf.FindItemDateTo.ToShortTimeString());
			cbUseDatesChanged();
			PopulateKeyTiming();
			TimerRestoreWindow.Start();
			BibleLookup.Items.Clear();
			BookLookup.Items.Clear();
			if (gf.HB_TotalVersions < 1)
			{
				TabControl1.TabPages[1].Enabled = false;
				TabControl1.SelectedIndex = 0;
				txtName.Focus();
				txtName.SelectAll();
				return;
			}
			TabControl1.TabPages[1].Enabled = true;
			gf.HB_SQLString = "";
			PassageSearchBox.Text = gf.FindBibleSearchPhrase;
			InitFormLoad = true;
			for (int i = 0; i <= gf.HB_TotalVersions - 1; i++)
			{
				BibleLookup.Items.Add(gf.HB_Versions[i, 1] + " - " + gf.HB_Versions[i, 2]);
			}
			InitFormLoad = false;
			BibleLookup.SelectedIndex = gf.HB_CurVersionTabIndex;
			BookLookup.SelectedIndex = gf.FindBibleBookIndex;
			if (gf.FindBibleVerses)
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
			SongKey.Text = gf.FindItemWithKey;
		}

		private void PopulateKeyTiming()
		{
			SongKey.Items.Clear();
			SongKey.Items.Add("");
			string fullSearchString = "select DISTINCT Key FROM SONG ORDER BY Key";
			string text = "";
#if OleDb
			using DataTable datatable1 = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
			using DataTable datatable1 = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif
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

			SongKey.Text = gf.FindItemWithKey;
			SongTiming.Items.Clear();
			SongTiming.Items.Add("");
			fullSearchString = "select DISTINCT Timing FROM SONG ORDER BY Timing";

#if OleDb
			using DataTable datatable2 = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
			using DataTable datatable2 = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif
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
			
			SongTiming.Text = gf.FindItemWithTiming;
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
				gf.LoadBibleBooksList(BibleLookup.SelectedIndex, ref BookLookup, ShowAllBooksLine: true, ShowSearchResultsLine: false);
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
			gf.FindItemsFormOpen = false;
		}

		private void TimerRestoreWindow_Tick(object sender, EventArgs e)
		{
			if (gf.FindItemRestoreWindow)
			{
				gf.FindItemRestoreWindow = false;
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
				gf.FindSearchPhrase = txtName.Text;
				gf.FindItemInTitle = cbTitle.Checked;
				gf.FindItemInContents = cbContents.Checked;
				gf.FindItemInSongNumber = cbSongNumber.Checked;
				gf.FindItemInBookRef = cbBookRef.Checked;
				gf.FindItemInUserRef = cbUserRef.Checked;
				gf.FindItemInLicAdmin = cbLicAdmin.Checked;
				gf.FindItemInWriter = cbWriter.Checked;
				gf.FindItemInCopyright = cbCopyright.Checked;
				gf.FindItemMediaOnly = cbMusicOnly.Checked;
				gf.FindItemNotationsOnly = cbNotationsOnly.Checked;
				gf.FindItemWithKey = SongKey.Text;
				gf.FindItemWithTiming = SongTiming.Text;
				gf.FindItemUseDates = cbUseDates.Checked;
				gf.FindItemDateFrom = CalendarFrom.Value;
				gf.FindItemDateTo = CalendarTo.Value;
				gf.Find_SQLString = gf.BuildItemSearchString(txtName.Text, gf.FindItemInTitle, gf.FindItemInContents, gf.FindItemInSongNumber, gf.FindItemInBookRef, gf.FindItemInUserRef, gf.FindItemInLicAdmin, gf.FindItemInWriter, gf.FindItemInCopyright, gf.FindItemNotationsOnly, gf.FindItemWithKey, gf.FindItemWithTiming, gf.FindItemUseDates, gf.FindItemDateFrom, gf.FindItemDateTo, FolderList);
				if (gf.Find_SQLString != "")
				{
					gf.FindFolderItems = true;
					gf.FindItemsRequested = true;
				}
			}
			else if (TabControl1.SelectedIndex == 1 && gf.HB_TotalVersions >= 1)
			{
				string text = gf.BuildBibleSearchString(MatchSelected: MatchAny.Checked ? 1 : ((!MatchAll.Checked) ? 2 : 0), InSearchPassage: PassageSearchBox.Text, VersionIndex: BibleLookup.SelectedIndex, BookIndex: BookLookup.SelectedIndex);
				if (text != "")
				{
					gf.HB_CurVersionTabIndex = BibleLookup.SelectedIndex;
					gf.HB_SQLString = text;
					gf.FindBibleSearchPhrase = PassageSearchBox.Text;
					gf.FindBibleBookIndex = BookLookup.SelectedIndex;
					gf.FindItemsRequested = true;
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
