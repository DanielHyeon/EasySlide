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
    partial class FrmFind
    {
		private Button BtnCancel;
		private Button BtnOK;
		private CheckBox cbBookRef;
		private CheckBox cbContents;
		private CheckBox cbCopyright;
		private CheckBox cbLicAdmin;
		private CheckBox cbMusicOnly;
		private CheckBox cbNotationsOnly;
		private CheckBox cbSongNumber;
		private CheckBox cbTitle;
		private CheckBox cbUseDates;
		private CheckBox cbUserRef;
		private CheckBox cbWriter;
		private CheckedListBox FolderList;
		private ComboBox BibleLookup;
		private ComboBox BookLookup;
		private ComboBox SongKey;
		private ComboBox SongTiming;
		private DateTimePicker CalendarFrom;
		private DateTimePicker CalendarTo;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private IContainer components = null;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private Label label6;
		private Label label7;
		private RadioButton MatchAll;
		private RadioButton MatchAny;
		private RadioButton MatchPhrase;
		private TabControl TabControl1;
		private TabPage tabPage1;
		private TabPage tabPage2;
		private TextBox PassageSearchBox;
		private TextBox txtName;
		private Timer TimerRestoreWindow;

protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code

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

        #endregion
    }
}
