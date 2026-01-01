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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmFind));
            TabControl1 = new TabControl();
            tabPage1 = new TabPage();
            CalendarTo = new DateTimePicker();
            CalendarFrom = new DateTimePicker();
            groupBox2 = new GroupBox();
            SongTiming = new ComboBox();
            label7 = new Label();
            cbNotationsOnly = new CheckBox();
            cbMusicOnly = new CheckBox();
            SongKey = new ComboBox();
            label5 = new Label();
            groupBox1 = new GroupBox();
            cbCopyright = new CheckBox();
            cbWriter = new CheckBox();
            cbLicAdmin = new CheckBox();
            cbUserRef = new CheckBox();
            cbBookRef = new CheckBox();
            cbSongNumber = new CheckBox();
            cbContents = new CheckBox();
            cbTitle = new CheckBox();
            FolderList = new CheckedListBox();
            txtName = new TextBox();
            label1 = new Label();
            cbUseDates = new CheckBox();
            label6 = new Label();
            tabPage2 = new TabPage();
            MatchPhrase = new RadioButton();
            MatchAny = new RadioButton();
            MatchAll = new RadioButton();
            BookLookup = new ComboBox();
            BibleLookup = new ComboBox();
            PassageSearchBox = new TextBox();
            label3 = new Label();
            label4 = new Label();
            label2 = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            TimerRestoreWindow = new Timer(components);
            TabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // TabControl1
            // 
            TabControl1.Controls.Add(tabPage1);
            TabControl1.Controls.Add(tabPage2);
            TabControl1.Location = new Point(16, 18);
            TabControl1.Margin = new Padding(4, 5, 4, 5);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new Size(520, 394);
            TabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(CalendarTo);
            tabPage1.Controls.Add(CalendarFrom);
            tabPage1.Controls.Add(groupBox2);
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Controls.Add(FolderList);
            tabPage1.Controls.Add(txtName);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(cbUseDates);
            tabPage1.Controls.Add(label6);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Margin = new Padding(4, 5, 4, 5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 5, 4, 5);
            tabPage1.Size = new Size(512, 361);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Folder Search";
            // 
            // CalendarTo
            // 
            CalendarTo.Location = new Point(296, 309);
            CalendarTo.Margin = new Padding(4, 5, 4, 5);
            CalendarTo.Name = "CalendarTo";
            CalendarTo.Size = new Size(204, 27);
            CalendarTo.TabIndex = 7;
            // 
            // CalendarFrom
            // 
            CalendarFrom.Location = new Point(295, 255);
            CalendarFrom.Margin = new Padding(4, 5, 4, 5);
            CalendarFrom.Name = "CalendarFrom";
            CalendarFrom.Size = new Size(205, 27);
            CalendarFrom.TabIndex = 5;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(SongTiming);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(cbNotationsOnly);
            groupBox2.Controls.Add(cbMusicOnly);
            groupBox2.Controls.Add(SongKey);
            groupBox2.Controls.Add(label5);
            groupBox2.Location = new Point(295, 75);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(207, 148);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Restrict To";
            // 
            // SongTiming
            // 
            SongTiming.FormattingEnabled = true;
            SongTiming.Location = new Point(69, 112);
            SongTiming.Margin = new Padding(4, 5, 4, 5);
            SongTiming.Name = "SongTiming";
            SongTiming.Size = new Size(93, 28);
            SongTiming.TabIndex = 10;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(8, 117);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(58, 20);
            label7.TabIndex = 9;
            label7.Text = "Timing:";
            // 
            // cbNotationsOnly
            // 
            cbNotationsOnly.AutoSize = true;
            cbNotationsOnly.Location = new Point(11, 52);
            cbNotationsOnly.Margin = new Padding(4, 5, 4, 5);
            cbNotationsOnly.Name = "cbNotationsOnly";
            cbNotationsOnly.Size = new Size(131, 24);
            cbNotationsOnly.TabIndex = 1;
            cbNotationsOnly.Text = "With Notations";
            // 
            // cbMusicOnly
            // 
            cbMusicOnly.AutoSize = true;
            cbMusicOnly.Location = new Point(11, 26);
            cbMusicOnly.Margin = new Padding(4, 5, 4, 5);
            cbMusicOnly.Name = "cbMusicOnly";
            cbMusicOnly.Size = new Size(131, 24);
            cbMusicOnly.TabIndex = 0;
            cbMusicOnly.Text = "With Music File";
            // 
            // SongKey
            // 
            SongKey.FormattingEnabled = true;
            SongKey.Location = new Point(69, 80);
            SongKey.Margin = new Padding(4, 5, 4, 5);
            SongKey.Name = "SongKey";
            SongKey.Size = new Size(93, 28);
            SongKey.TabIndex = 3;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 83);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(36, 20);
            label5.TabIndex = 2;
            label5.Text = "Key:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cbCopyright);
            groupBox1.Controls.Add(cbWriter);
            groupBox1.Controls.Add(cbLicAdmin);
            groupBox1.Controls.Add(cbUserRef);
            groupBox1.Controls.Add(cbBookRef);
            groupBox1.Controls.Add(cbSongNumber);
            groupBox1.Controls.Add(cbContents);
            groupBox1.Controls.Add(cbTitle);
            groupBox1.Location = new Point(167, 75);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(120, 268);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Search On";
            // 
            // cbCopyright
            // 
            cbCopyright.AutoSize = true;
            cbCopyright.Location = new Point(11, 129);
            cbCopyright.Margin = new Padding(4, 5, 4, 5);
            cbCopyright.Name = "cbCopyright";
            cbCopyright.Size = new Size(96, 24);
            cbCopyright.TabIndex = 4;
            cbCopyright.Text = "Copyright";
            // 
            // cbWriter
            // 
            cbWriter.AutoSize = true;
            cbWriter.Location = new Point(11, 103);
            cbWriter.Margin = new Padding(4, 5, 4, 5);
            cbWriter.Name = "cbWriter";
            cbWriter.Size = new Size(72, 24);
            cbWriter.TabIndex = 3;
            cbWriter.Text = "Writer";
            // 
            // cbLicAdmin
            // 
            cbLicAdmin.AutoSize = true;
            cbLicAdmin.Location = new Point(11, 206);
            cbLicAdmin.Margin = new Padding(4, 5, 4, 5);
            cbLicAdmin.Name = "cbLicAdmin";
            cbLicAdmin.Size = new Size(97, 24);
            cbLicAdmin.TabIndex = 7;
            cbLicAdmin.Text = "Lic Admin";
            // 
            // cbUserRef
            // 
            cbUserRef.AutoSize = true;
            cbUserRef.Location = new Point(11, 180);
            cbUserRef.Margin = new Padding(4, 5, 4, 5);
            cbUserRef.Name = "cbUserRef";
            cbUserRef.Size = new Size(86, 24);
            cbUserRef.TabIndex = 6;
            cbUserRef.Text = "User Ref";
            // 
            // cbBookRef
            // 
            cbBookRef.AutoSize = true;
            cbBookRef.Location = new Point(11, 154);
            cbBookRef.Margin = new Padding(4, 5, 4, 5);
            cbBookRef.Name = "cbBookRef";
            cbBookRef.Size = new Size(91, 24);
            cbBookRef.TabIndex = 5;
            cbBookRef.Text = "Book Ref";
            // 
            // cbSongNumber
            // 
            cbSongNumber.AutoSize = true;
            cbSongNumber.Location = new Point(11, 77);
            cbSongNumber.Margin = new Padding(4, 5, 4, 5);
            cbSongNumber.Name = "cbSongNumber";
            cbSongNumber.Size = new Size(92, 24);
            cbSongNumber.TabIndex = 2;
            cbSongNumber.Text = "Song No.";
            // 
            // cbContents
            // 
            cbContents.AutoSize = true;
            cbContents.Location = new Point(11, 51);
            cbContents.Margin = new Padding(4, 5, 4, 5);
            cbContents.Name = "cbContents";
            cbContents.Size = new Size(89, 24);
            cbContents.TabIndex = 1;
            cbContents.Text = "Contents";
            // 
            // cbTitle
            // 
            cbTitle.AutoSize = true;
            cbTitle.Location = new Point(11, 25);
            cbTitle.Margin = new Padding(4, 5, 4, 5);
            cbTitle.Name = "cbTitle";
            cbTitle.Size = new Size(60, 24);
            cbTitle.TabIndex = 0;
            cbTitle.Text = "Title";
            // 
            // FolderList
            // 
            FolderList.CheckOnClick = true;
            FolderList.FormattingEnabled = true;
            FolderList.Location = new Point(9, 83);
            FolderList.Margin = new Padding(4, 5, 4, 5);
            FolderList.Name = "FolderList";
            FolderList.Size = new Size(148, 246);
            FolderList.TabIndex = 1;
            // 
            // txtName
            // 
            txtName.Location = new Point(9, 43);
            txtName.Margin = new Padding(4, 5, 4, 5);
            txtName.Name = "txtName";
            txtName.Size = new Size(491, 27);
            txtName.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 18);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(140, 20);
            label1.TabIndex = 1;
            label1.Text = "Enter search phrase:";
            // 
            // cbUseDates
            // 
            cbUseDates.AutoSize = true;
            cbUseDates.Location = new Point(299, 228);
            cbUseDates.Margin = new Padding(4, 5, 4, 5);
            cbUseDates.Name = "cbUseDates";
            cbUseDates.Size = new Size(136, 24);
            cbUseDates.TabIndex = 4;
            cbUseDates.Text = "Search between";
            cbUseDates.CheckedChanged += cbUseDates_CheckedChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(295, 286);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(34, 20);
            label6.TabIndex = 6;
            label6.Text = "and";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(MatchPhrase);
            tabPage2.Controls.Add(MatchAny);
            tabPage2.Controls.Add(MatchAll);
            tabPage2.Controls.Add(BookLookup);
            tabPage2.Controls.Add(BibleLookup);
            tabPage2.Controls.Add(PassageSearchBox);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(label4);
            tabPage2.Controls.Add(label2);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Margin = new Padding(4, 5, 4, 5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4, 5, 4, 5);
            tabPage2.Size = new Size(512, 361);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Bible Search";
            // 
            // MatchPhrase
            // 
            MatchPhrase.AutoSize = true;
            MatchPhrase.Location = new Point(21, 262);
            MatchPhrase.Margin = new Padding(4, 5, 4, 5);
            MatchPhrase.Name = "MatchPhrase";
            MatchPhrase.Size = new Size(194, 24);
            MatchPhrase.TabIndex = 5;
            MatchPhrase.Text = "Match the phrase exactly";
            // 
            // MatchAny
            // 
            MatchAny.AutoSize = true;
            MatchAny.Location = new Point(21, 235);
            MatchAny.Margin = new Padding(4, 5, 4, 5);
            MatchAny.Name = "MatchAny";
            MatchAny.Size = new Size(185, 24);
            MatchAny.TabIndex = 4;
            MatchAny.Text = "Match any of the words";
            // 
            // MatchAll
            // 
            MatchAll.AutoSize = true;
            MatchAll.Checked = true;
            MatchAll.Location = new Point(21, 209);
            MatchAll.Margin = new Padding(4, 5, 4, 5);
            MatchAll.Name = "MatchAll";
            MatchAll.Size = new Size(160, 24);
            MatchAll.TabIndex = 3;
            MatchAll.TabStop = true;
            MatchAll.Text = "Match all the words";
            // 
            // BookLookup
            // 
            BookLookup.FormattingEnabled = true;
            BookLookup.Location = new Point(9, 105);
            BookLookup.Margin = new Padding(4, 5, 4, 5);
            BookLookup.Name = "BookLookup";
            BookLookup.Size = new Size(236, 28);
            BookLookup.TabIndex = 1;
            // 
            // BibleLookup
            // 
            BibleLookup.FormattingEnabled = true;
            BibleLookup.Location = new Point(9, 43);
            BibleLookup.Margin = new Padding(4, 5, 4, 5);
            BibleLookup.Name = "BibleLookup";
            BibleLookup.Size = new Size(348, 28);
            BibleLookup.TabIndex = 0;
            BibleLookup.SelectedIndexChanged += BibleLookup_SelectedIndexChanged;
            // 
            // PassageSearchBox
            // 
            PassageSearchBox.Location = new Point(9, 168);
            PassageSearchBox.Margin = new Padding(4, 5, 4, 5);
            PassageSearchBox.Name = "PassageSearchBox";
            PassageSearchBox.Size = new Size(477, 27);
            PassageSearchBox.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 18);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(98, 20);
            label3.TabIndex = 5;
            label3.Text = "Bible Version:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 80);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(46, 20);
            label4.TabIndex = 7;
            label4.Text = "Book:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 143);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(140, 20);
            label2.TabIndex = 2;
            label2.Text = "Enter search phrase:";
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(289, 425);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 1;
            BtnCancel.Text = "Close";
            BtnCancel.Click += BtnCancel_Click;
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(175, 425);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 0;
            BtnOK.Text = "Search";
            BtnOK.Click += BtnOK_Click;
            // 
            // TimerRestoreWindow
            // 
            TimerRestoreWindow.Tick += TimerRestoreWindow_Tick;
            // 
            // FrmFind
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(552, 480);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(TabControl1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "FrmFind";
            Text = "Search for EasiSlides Items";
            FormClosing += FrmFind_FormClosing;
            Load += FrmFind_Load;
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
		/// daniel �˻�
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
			for (int i = 1; i < gf.MAXSONGSFOLDERS; i++)
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
			using DataTable datatable1 = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);

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


			using DataTable datatable2 = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);

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
