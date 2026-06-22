//using NetOffice.DAOApi;
//using NetOffice.DAOApi.Enums;

using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
//using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
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
    partial class FrmGenerateHtml
    {
		private Button BtnCancel;
		private Button BtnOK;
		private CheckBox BookRefcb;
		private CheckBox ChorusItaliccb;
		private CheckBox DoubleBytecb;
		private CheckBox MusicLinkcb;
		private CheckBox MusicNumberscb;
		private CheckBox ShowindexFilecb;
		private CheckBox UserRefcb;
		private GroupBox groupBox1;
		private IContainer components = null;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label WarningLabel;
		private NumericUpDown RTFFontSize;
		private ProgressBar ProgressBar1;
		private RadioButton OptOutputHTML;
		private RadioButton OptOutputRTF;
		private TextBox HeadingText0;
		private TextBox HeadingText1;
		private TextBox HeadingText2;
		private ToolTip ToolTip1;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmGenerateHtml));
            HeadingText0 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            HeadingText1 = new TextBox();
            label3 = new Label();
            HeadingText2 = new TextBox();
            MusicLinkcb = new CheckBox();
            MusicNumberscb = new CheckBox();
            ShowindexFilecb = new CheckBox();
            BookRefcb = new CheckBox();
            UserRefcb = new CheckBox();
            ProgressBar1 = new ProgressBar();
            WarningLabel = new Label();
            ToolTip1 = new ToolTip(components);
            OptOutputHTML = new RadioButton();
            OptOutputRTF = new RadioButton();
            BtnCancel = new Button();
            BtnOK = new Button();
            DoubleBytecb = new CheckBox();
            groupBox1 = new GroupBox();
            RTFFontSize = new NumericUpDown();
            label4 = new Label();
            ChorusItaliccb = new CheckBox();
            groupBox1.SuspendLayout();
            ((ISupportInitialize)RTFFontSize).BeginInit();
            SuspendLayout();
            //
            // HeadingText0
            //
            HeadingText0.Location = new Point(16, 51);
            HeadingText0.Margin = new Padding(4, 5, 4, 5);
            HeadingText0.Name = "HeadingText0";
            HeadingText0.Size = new Size(596, 27);
            HeadingText0.TabIndex = 0;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(15, 25);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(254, 20);
            label1.TabIndex = 1;
            label1.Text = "Main Heading - (eg. XYZ Fellowship):";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(16, 86);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(465, 20);
            label2.TabIndex = 3;
            label2.Text = "Web Address Link for Main Heading  (eg. http://www.easislides.com):";
            //
            // HeadingText1
            //
            HeadingText1.Location = new Point(17, 109);
            HeadingText1.Margin = new Padding(4, 5, 4, 5);
            HeadingText1.Name = "HeadingText1";
            HeadingText1.Size = new Size(596, 27);
            HeadingText1.TabIndex = 1;
            //
            // label3
            //
            label3.AutoSize = true;
            label3.Location = new Point(16, 146);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(319, 20);
            label3.TabIndex = 5;
            label3.Text = "Secondary Heading (eg. List of Worship Songs)";
            //
            // HeadingText2
            //
            HeadingText2.Location = new Point(17, 171);
            HeadingText2.Margin = new Padding(4, 5, 4, 5);
            HeadingText2.Name = "HeadingText2";
            HeadingText2.Size = new Size(596, 27);
            HeadingText2.TabIndex = 2;
            //
            // MusicLinkcb
            //
            MusicLinkcb.AutoSize = true;
            MusicLinkcb.Location = new Point(17, 212);
            MusicLinkcb.Margin = new Padding(4, 5, 4, 5);
            MusicLinkcb.Name = "MusicLinkcb";
            MusicLinkcb.Size = new Size(227, 24);
            MusicLinkcb.TabIndex = 3;
            MusicLinkcb.Text = "Link to Music Files if available";
            //
            // MusicNumberscb
            //
            MusicNumberscb.AutoSize = true;
            MusicNumberscb.Location = new Point(17, 265);
            MusicNumberscb.Margin = new Padding(4, 5, 4, 5);
            MusicNumberscb.Name = "MusicNumberscb";
            MusicNumberscb.Size = new Size(184, 24);
            MusicNumberscb.TabIndex = 5;
            MusicNumberscb.Text = "Show Song Numbering";
            //
            // ShowindexFilecb
            //
            ShowindexFilecb.AutoSize = true;
            ShowindexFilecb.Location = new Point(17, 291);
            ShowindexFilecb.Margin = new Padding(4, 5, 4, 5);
            ShowindexFilecb.Name = "ShowindexFilecb";
            ShowindexFilecb.Size = new Size(247, 24);
            ShowindexFilecb.TabIndex = 6;
            ShowindexFilecb.Text = "Show index.htm after generation";
            //
            // BookRefcb
            //
            BookRefcb.AutoSize = true;
            BookRefcb.Location = new Point(259, 212);
            BookRefcb.Margin = new Padding(4, 5, 4, 5);
            BookRefcb.Name = "BookRefcb";
            BookRefcb.Size = new Size(175, 24);
            BookRefcb.TabIndex = 7;
            BookRefcb.Text = "Show Book Reference";
            //
            // UserRefcb
            //
            UserRefcb.AutoSize = true;
            UserRefcb.Location = new Point(259, 238);
            UserRefcb.Margin = new Padding(4, 5, 4, 5);
            UserRefcb.Name = "UserRefcb";
            UserRefcb.Size = new Size(170, 24);
            UserRefcb.TabIndex = 8;
            UserRefcb.Text = "Show User Reference";
            //
            // ProgressBar1
            //
            ProgressBar1.Location = new Point(16, 325);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(596, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 11;
            //
            // WarningLabel
            //
            WarningLabel.Location = new Point(27, 331);
            WarningLabel.Margin = new Padding(4, 0, 4, 0);
            WarningLabel.Name = "WarningLabel";
            WarningLabel.Size = new Size(573, 20);
            WarningLabel.TabIndex = 12;
            //
            // OptOutputHTML
            //
            OptOutputHTML.AutoSize = true;
            OptOutputHTML.Checked = true;
            OptOutputHTML.Location = new Point(8, 18);
            OptOutputHTML.Margin = new Padding(4, 5, 4, 5);
            OptOutputHTML.Name = "OptOutputHTML";
            OptOutputHTML.Size = new Size(102, 24);
            OptOutputHTML.TabIndex = 0;
            OptOutputHTML.TabStop = true;
            OptOutputHTML.Text = "HTML Files";
            ToolTip1.SetToolTip(OptOutputHTML, "HTML style files");
            OptOutputHTML.UseVisualStyleBackColor = true;
            //
            // OptOutputRTF
            //
            OptOutputRTF.AutoSize = true;
            OptOutputRTF.Location = new Point(8, 51);
            OptOutputRTF.Margin = new Padding(4, 5, 4, 5);
            OptOutputRTF.Name = "OptOutputRTF";
            OptOutputRTF.Size = new Size(86, 24);
            OptOutputRTF.TabIndex = 1;
            OptOutputRTF.Text = "RTF Files";
            ToolTip1.SetToolTip(OptOutputRTF, "Rich Text Format Files with Notations (if any)");
            OptOutputRTF.UseVisualStyleBackColor = true;
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(507, 366);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 14;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(379, 366);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 13;
            BtnOK.Text = "Generate";
            BtnOK.Click += BtnOK_Click;
            //
            // DoubleBytecb
            //
            DoubleBytecb.AutoSize = true;
            DoubleBytecb.Location = new Point(259, 265);
            DoubleBytecb.Margin = new Padding(4, 5, 4, 5);
            DoubleBytecb.Name = "DoubleBytecb";
            DoubleBytecb.Size = new Size(174, 24);
            DoubleBytecb.TabIndex = 9;
            DoubleBytecb.Text = "Use Double Byte Files";
            //
            // groupBox1
            //
            groupBox1.Controls.Add(RTFFontSize);
            groupBox1.Controls.Add(OptOutputHTML);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(OptOutputRTF);
            groupBox1.Location = new Point(455, 202);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(160, 114);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            //
            // RTFFontSize
            //
            RTFFontSize.Location = new Point(99, 75);
            RTFFontSize.Margin = new Padding(4, 5, 4, 5);
            RTFFontSize.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            RTFFontSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            RTFFontSize.Name = "RTFFontSize";
            RTFFontSize.Size = new Size(53, 27);
            RTFFontSize.TabIndex = 16;
            RTFFontSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
            //
            // label4
            //
            label4.AutoSize = true;
            label4.Location = new Point(28, 82);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(72, 20);
            label4.TabIndex = 15;
            label4.Text = "Font Size:";
            //
            // ChorusItaliccb
            //
            ChorusItaliccb.AutoSize = true;
            ChorusItaliccb.Location = new Point(17, 238);
            ChorusItaliccb.Margin = new Padding(4, 5, 4, 5);
            ChorusItaliccb.Name = "ChorusItaliccb";
            ChorusItaliccb.Size = new Size(174, 24);
            ChorusItaliccb.TabIndex = 4;
            ChorusItaliccb.Text = "Show Chorus in Italics";
            //
            // FrmGenerateHtml
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(629, 429);
            Controls.Add(ChorusItaliccb);
            Controls.Add(DoubleBytecb);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(WarningLabel);
            Controls.Add(ProgressBar1);
            Controls.Add(UserRefcb);
            Controls.Add(BookRefcb);
            Controls.Add(HeadingText2);
            Controls.Add(HeadingText1);
            Controls.Add(HeadingText0);
            Controls.Add(ShowindexFilecb);
            Controls.Add(MusicNumberscb);
            Controls.Add(MusicLinkcb);
            Controls.Add(groupBox1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmGenerateHtml";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Generate HTML";
            Load += FrmGenerateHTML_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((ISupportInitialize)RTFFontSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
