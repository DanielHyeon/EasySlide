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
    partial class FrmSmartMerge
    {
		private Button BtnCancel;
		private Button BtnOK;
		private CheckBox cbTickAll;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private ColumnHeader columnHeader5;
		private ComboBox SongFolderA;
		private ComboBox SongFolderB;
		private ComboBox SongFolderC;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private GroupBox groupBox3;
		private GroupBox groupBox4;
		private GroupBox groupBox5;
		private GroupBox groupBox6;
		private IContainer components = null;
		private ListView SongsList;
		private ProgressBar ProgressBar1;
		private RadioButton OptionNewTitleA;
		private RadioButton OptionNewTitleAB;
		private RadioButton OptionSourceATitle1;
		private RadioButton OptionSourceATitle2;
		private RadioButton OptionSourceBTitle1;
		private RadioButton OptionSourceBTitle2;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmSmartMerge));
            groupBox1 = new GroupBox();
            groupBox5 = new GroupBox();
            OptionSourceBTitle2 = new RadioButton();
            OptionSourceBTitle1 = new RadioButton();
            SongFolderB = new ComboBox();
            groupBox3 = new GroupBox();
            OptionSourceATitle2 = new RadioButton();
            OptionSourceATitle1 = new RadioButton();
            SongFolderA = new ComboBox();
            groupBox2 = new GroupBox();
            groupBox6 = new GroupBox();
            OptionNewTitleAB = new RadioButton();
            OptionNewTitleA = new RadioButton();
            SongFolderC = new ComboBox();
            BtnCancel = new Button();
            BtnOK = new Button();
            cbTickAll = new CheckBox();
            ProgressBar1 = new ProgressBar();
            groupBox4 = new GroupBox();
            SongsList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            groupBox1.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(groupBox5);
            groupBox1.Controls.Add(groupBox3);
            groupBox1.Location = new Point(16, 18);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(443, 175);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Identify Matching Titles from Source A and Source B";
            //
            // groupBox5
            //
            groupBox5.Controls.Add(OptionSourceBTitle2);
            groupBox5.Controls.Add(OptionSourceBTitle1);
            groupBox5.Controls.Add(SongFolderB);
            groupBox5.Location = new Point(225, 29);
            groupBox5.Margin = new Padding(4, 5, 4, 5);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 5, 4, 5);
            groupBox5.Size = new Size(204, 137);
            groupBox5.TabIndex = 1;
            groupBox5.TabStop = false;
            groupBox5.Text = "Source B (Region 2)";
            //
            // OptionSourceBTitle2
            //
            OptionSourceBTitle2.AutoSize = true;
            OptionSourceBTitle2.Location = new Point(23, 94);
            OptionSourceBTitle2.Margin = new Padding(4, 5, 4, 5);
            OptionSourceBTitle2.Name = "OptionSourceBTitle2";
            OptionSourceBTitle2.Size = new Size(71, 24);
            OptionSourceBTitle2.TabIndex = 2;
            OptionSourceBTitle2.Text = "Title 2";
            //
            // OptionSourceBTitle1
            //
            OptionSourceBTitle1.AutoSize = true;
            OptionSourceBTitle1.Checked = true;
            OptionSourceBTitle1.Location = new Point(23, 66);
            OptionSourceBTitle1.Margin = new Padding(4, 5, 4, 5);
            OptionSourceBTitle1.Name = "OptionSourceBTitle1";
            OptionSourceBTitle1.Size = new Size(71, 24);
            OptionSourceBTitle1.TabIndex = 1;
            OptionSourceBTitle1.TabStop = true;
            OptionSourceBTitle1.Text = "Title 1";
            OptionSourceBTitle1.CheckedChanged += OptionSourceABTitle1_CheckedChanged;
            //
            // SongFolderB
            //
            SongFolderB.DropDownStyle = ComboBoxStyle.DropDownList;
            SongFolderB.FormattingEnabled = true;
            SongFolderB.Location = new Point(8, 29);
            SongFolderB.Margin = new Padding(4, 5, 4, 5);
            SongFolderB.MaxDropDownItems = 12;
            SongFolderB.Name = "SongFolderB";
            SongFolderB.Size = new Size(187, 28);
            SongFolderB.TabIndex = 0;
            SongFolderB.SelectedIndexChanged += OptionSourceABTitle1_CheckedChanged;
            //
            // groupBox3
            //
            groupBox3.Controls.Add(OptionSourceATitle2);
            groupBox3.Controls.Add(OptionSourceATitle1);
            groupBox3.Controls.Add(SongFolderA);
            groupBox3.Location = new Point(11, 29);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(204, 137);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            groupBox3.Text = "Source A (Region 1)";
            //
            // OptionSourceATitle2
            //
            OptionSourceATitle2.AutoSize = true;
            OptionSourceATitle2.Checked = true;
            OptionSourceATitle2.Location = new Point(23, 94);
            OptionSourceATitle2.Margin = new Padding(4, 5, 4, 5);
            OptionSourceATitle2.Name = "OptionSourceATitle2";
            OptionSourceATitle2.Size = new Size(71, 24);
            OptionSourceATitle2.TabIndex = 2;
            OptionSourceATitle2.TabStop = true;
            OptionSourceATitle2.Text = "Title 2";
            //
            // OptionSourceATitle1
            //
            OptionSourceATitle1.AutoSize = true;
            OptionSourceATitle1.Location = new Point(23, 66);
            OptionSourceATitle1.Margin = new Padding(4, 5, 4, 5);
            OptionSourceATitle1.Name = "OptionSourceATitle1";
            OptionSourceATitle1.Size = new Size(71, 24);
            OptionSourceATitle1.TabIndex = 1;
            OptionSourceATitle1.Text = "Title 1";
            OptionSourceATitle1.CheckedChanged += OptionSourceABTitle1_CheckedChanged;
            //
            // SongFolderA
            //
            SongFolderA.DropDownStyle = ComboBoxStyle.DropDownList;
            SongFolderA.FormattingEnabled = true;
            SongFolderA.Location = new Point(8, 29);
            SongFolderA.Margin = new Padding(4, 5, 4, 5);
            SongFolderA.MaxDropDownItems = 12;
            SongFolderA.Name = "SongFolderA";
            SongFolderA.Size = new Size(187, 28);
            SongFolderA.TabIndex = 0;
            SongFolderA.SelectedIndexChanged += OptionSourceABTitle1_CheckedChanged;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(groupBox6);
            groupBox2.Location = new Point(463, 18);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(225, 175);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Merge Matching Items into";
            //
            // groupBox6
            //
            groupBox6.Controls.Add(OptionNewTitleAB);
            groupBox6.Controls.Add(OptionNewTitleA);
            groupBox6.Controls.Add(SongFolderC);
            groupBox6.Location = new Point(11, 29);
            groupBox6.Margin = new Padding(4, 5, 4, 5);
            groupBox6.Name = "groupBox6";
            groupBox6.Padding = new Padding(4, 5, 4, 5);
            groupBox6.Size = new Size(204, 137);
            groupBox6.TabIndex = 0;
            groupBox6.TabStop = false;
            groupBox6.Text = "Destination Folder";
            //
            // OptionNewTitleAB
            //
            OptionNewTitleAB.AutoSize = true;
            OptionNewTitleAB.Location = new Point(23, 94);
            OptionNewTitleAB.Margin = new Padding(4, 5, 4, 5);
            OptionNewTitleAB.Name = "OptionNewTitleAB";
            OptionNewTitleAB.Size = new Size(156, 24);
            OptionNewTitleAB.TabIndex = 4;
            OptionNewTitleAB.Text = "Source A Title Only";
            //
            // OptionNewTitleA
            //
            OptionNewTitleA.AutoSize = true;
            OptionNewTitleA.Checked = true;
            OptionNewTitleA.Location = new Point(23, 66);
            OptionNewTitleA.Margin = new Padding(4, 5, 4, 5);
            OptionNewTitleA.Name = "OptionNewTitleA";
            OptionNewTitleA.Size = new Size(155, 24);
            OptionNewTitleA.TabIndex = 1;
            OptionNewTitleA.TabStop = true;
            OptionNewTitleA.Text = "Merge Titles A && B";
            OptionNewTitleA.CheckedChanged += OptionNewTitleA_CheckedChanged;
            //
            // SongFolderC
            //
            SongFolderC.DropDownStyle = ComboBoxStyle.DropDownList;
            SongFolderC.FormattingEnabled = true;
            SongFolderC.Location = new Point(8, 29);
            SongFolderC.Margin = new Padding(4, 5, 4, 5);
            SongFolderC.MaxDropDownItems = 12;
            SongFolderC.Name = "SongFolderC";
            SongFolderC.Size = new Size(187, 28);
            SongFolderC.TabIndex = 0;
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(581, 546);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 6;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(453, 546);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 5;
            BtnOK.Text = "Start Merge";
            BtnOK.Click += BtnOK_Click;
            //
            // cbTickAll
            //
            cbTickAll.AutoSize = true;
            cbTickAll.Location = new Point(28, 546);
            cbTickAll.Margin = new Padding(4, 5, 4, 5);
            cbTickAll.Name = "cbTickAll";
            cbTickAll.Size = new Size(79, 24);
            cbTickAll.TabIndex = 4;
            cbTickAll.Text = "Tick All";
            cbTickAll.ThreeState = true;
            cbTickAll.KeyUp += cbTickAll_KeyUp;
            cbTickAll.MouseUp += cbTickAll_MouseUp;
            //
            // ProgressBar1
            //
            ProgressBar1.Location = new Point(16, 502);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(672, 32);
            ProgressBar1.TabIndex = 3;
            //
            // groupBox4
            //
            groupBox4.Controls.Add(SongsList);
            groupBox4.Location = new Point(16, 195);
            groupBox4.Margin = new Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 5, 4, 5);
            groupBox4.Size = new Size(672, 305);
            groupBox4.TabIndex = 2;
            groupBox4.TabStop = false;
            groupBox4.Text = "Matching Items Found";
            //
            // SongsList
            //
            SongsList.CheckBoxes = true;
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            SongsList.Location = new Point(8, 29);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.Name = "SongsList";
            SongsList.Size = new Size(655, 266);
            SongsList.TabIndex = 0;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            SongsList.KeyUp += SongsList_KeyUp;
            SongsList.MouseUp += SongsList_MouseUp;
            //
            // columnHeader1
            //
            columnHeader1.Text = "Items";
            columnHeader1.Width = 470;
            //
            // columnHeader2
            //
            columnHeader2.Text = "Song A ID";
            columnHeader2.Width = 0;
            //
            // columnHeader3
            //
            columnHeader3.Text = "Song B ID";
            columnHeader3.Width = 0;
            //
            // columnHeader4
            //
            columnHeader4.Text = "Title B";
            columnHeader4.Width = 0;
            //
            // columnHeader5
            //
            columnHeader5.Text = "Title A";
            columnHeader5.Width = 0;
            //
            // FrmSmartMerge
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 602);
            Controls.Add(groupBox4);
            Controls.Add(ProgressBar1);
            Controls.Add(cbTickAll);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmSmartMerge";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Smart Merge";
            Load += FrmSmartMerge_Load;
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

        #endregion
    }
}
