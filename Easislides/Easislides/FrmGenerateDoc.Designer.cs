using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
    partial class FrmGenerateDoc
    {
		private Button BtnCancel;
		private Button BtnIndexOnly;
		private Button BtnOK;
		private Button BtnSaveExit;
		private Button BtnTitlesRef;
		private CheckBox optCapoZero;
		private CheckBox optHeadings0;
		private CheckBox optHeadings1;
		private CheckBox optHeadings2;
		private CheckBox optHeadings3;
		private CheckBox optNewScreen;
		private CheckBox optOneSongPerPage;
		private CheckBox optPrinterSpaces;
		private CheckBox optShowCapo;
		private CheckBox optShowKey;
		private CheckBox optShowTiming;
		private CheckBox optWords0;
		private CheckBox optWords1;
		private CheckBox optWords2;
		private CheckBox optWords5;
		private CheckBox optWords6;
		private CheckBox optWords7;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private GroupBox groupBox3;
		private GroupBox groupBox4;
		private GroupBox groupBox5;
		private GroupBox groupBox6;
		private GroupBox groupBox7;
		private IContainer components = null;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private Label label6;
		private Label label7;
		private Label label8;
		private Label Mess1;
		private NumericUpDown tbFontSize0;
		private NumericUpDown tbFontSize1;
		private NumericUpDown tbFontSize2;
		private NumericUpDown tbFontSize3;
		private NumericUpDown tbFontSize4;
		private NumericUpDown tbFontSize5;
		private NumericUpDown tbSpacing0;
		private NumericUpDown tbSpacing1;
		private Panel panel1;
		private Panel panel2;
		private Panel panel3;
		private Panel panel4;
		private Panel panel5;
		private Panel panel6;
		private Panel panel7;
		private Panel panel8;
		private Panel PanelFontColour0;
		private Panel PanelFontColour1;
		private Panel PanelFontColour2;
		private Panel PanelFontColour3;
		private Panel PanelFontColour4;
		private Panel PanelFontColour5;
		private ProgressBar ProgressBar1;
		private RadioButton OptLyricsPattern0;
		private RadioButton OptLyricsPattern1;
		private RadioButton OptPageSize0;
		private RadioButton OptPageSize1;
		private RadioButton OptShowColumns1;
		private RadioButton OptShowColumns2;
		private RadioButton OptShowSection0;
		private RadioButton OptShowSection1;
		private RadioButton OptShowSection2;
		private ToolStrip toolStripFont0;
		private ToolStrip toolStripFont1;
		private ToolStrip toolStripFont2;
		private ToolStrip toolStripFont3;
		private ToolStrip toolStripFont4;
		private ToolStrip toolStripFont5;
		private ToolStripButton toolStripButton1;
		private ToolStripButton toolStripButton10;
		private ToolStripButton toolStripButton100;
		private ToolStripButton toolStripButton101;
		private ToolStripButton toolStripButton102;
		private ToolStripButton toolStripButton11;
		private ToolStripButton toolStripButton12;
		private ToolStripButton toolStripButton14;
		private ToolStripButton toolStripButton15;
		private ToolStripButton toolStripButton16;
		private ToolStripButton toolStripButton2;
		private ToolStripButton toolStripButton3;
		private ToolStripButton toolStripButton4;
		private ToolStripButton toolStripButton5;
		private ToolStripButton toolStripButton6;
		private ToolStripButton toolStripButton7;
		private ToolStripButton toolStripButton8;
		private ToolStripButton toolStripButton9;
		private ToolTip toolTip1;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmGenerateDoc));
            groupBox5 = new GroupBox();
            OptShowColumns2 = new RadioButton();
            OptShowColumns1 = new RadioButton();
            groupBox7 = new GroupBox();
            optOneSongPerPage = new CheckBox();
            optNewScreen = new CheckBox();
            panel6 = new Panel();
            tbSpacing1 = new NumericUpDown();
            label6 = new Label();
            panel7 = new Panel();
            tbSpacing0 = new NumericUpDown();
            label7 = new Label();
            groupBox6 = new GroupBox();
            OptLyricsPattern1 = new RadioButton();
            OptLyricsPattern0 = new RadioButton();
            optCapoZero = new CheckBox();
            optShowTiming = new CheckBox();
            optPrinterSpaces = new CheckBox();
            optShowCapo = new CheckBox();
            optShowKey = new CheckBox();
            optWords5 = new CheckBox();
            groupBox3 = new GroupBox();
            OptShowSection1 = new RadioButton();
            OptShowSection0 = new RadioButton();
            OptShowSection2 = new RadioButton();
            BtnSaveExit = new Button();
            BtnCancel = new Button();
            BtnOK = new Button();
            Mess1 = new Label();
            ProgressBar1 = new ProgressBar();
            groupBox2 = new GroupBox();
            optHeadings3 = new CheckBox();
            optWords7 = new CheckBox();
            optHeadings2 = new CheckBox();
            optHeadings1 = new CheckBox();
            optHeadings0 = new CheckBox();
            optWords6 = new CheckBox();
            optWords2 = new CheckBox();
            optWords1 = new CheckBox();
            optWords0 = new CheckBox();
            groupBox1 = new GroupBox();
            panel8 = new Panel();
            PanelFontColour5 = new Panel();
            tbFontSize5 = new NumericUpDown();
            toolStripFont5 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            toolStripButton9 = new ToolStripButton();
            label8 = new Label();
            panel5 = new Panel();
            PanelFontColour4 = new Panel();
            tbFontSize4 = new NumericUpDown();
            toolStripFont4 = new ToolStrip();
            toolStripButton14 = new ToolStripButton();
            toolStripButton15 = new ToolStripButton();
            toolStripButton16 = new ToolStripButton();
            label5 = new Label();
            panel4 = new Panel();
            PanelFontColour3 = new Panel();
            tbFontSize3 = new NumericUpDown();
            toolStripFont3 = new ToolStrip();
            toolStripButton10 = new ToolStripButton();
            toolStripButton11 = new ToolStripButton();
            toolStripButton12 = new ToolStripButton();
            label4 = new Label();
            panel3 = new Panel();
            PanelFontColour2 = new Panel();
            tbFontSize2 = new NumericUpDown();
            toolStripFont2 = new ToolStrip();
            toolStripButton6 = new ToolStripButton();
            toolStripButton7 = new ToolStripButton();
            toolStripButton8 = new ToolStripButton();
            label3 = new Label();
            panel2 = new Panel();
            PanelFontColour1 = new Panel();
            tbFontSize1 = new NumericUpDown();
            toolStripFont1 = new ToolStrip();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            label2 = new Label();
            panel1 = new Panel();
            PanelFontColour0 = new Panel();
            tbFontSize0 = new NumericUpDown();
            toolStripFont0 = new ToolStrip();
            toolStripButton100 = new ToolStripButton();
            toolStripButton101 = new ToolStripButton();
            toolStripButton102 = new ToolStripButton();
            label1 = new Label();
            toolTip1 = new ToolTip(components);
            BtnIndexOnly = new Button();
            BtnTitlesRef = new Button();
            groupBox4 = new GroupBox();
            OptPageSize1 = new RadioButton();
            OptPageSize0 = new RadioButton();
            groupBox5.SuspendLayout();
            groupBox7.SuspendLayout();
            panel6.SuspendLayout();
            ((ISupportInitialize)tbSpacing1).BeginInit();
            panel7.SuspendLayout();
            ((ISupportInitialize)tbSpacing0).BeginInit();
            groupBox6.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            panel8.SuspendLayout();
            ((ISupportInitialize)tbFontSize5).BeginInit();
            toolStripFont5.SuspendLayout();
            panel5.SuspendLayout();
            ((ISupportInitialize)tbFontSize4).BeginInit();
            toolStripFont4.SuspendLayout();
            panel4.SuspendLayout();
            ((ISupportInitialize)tbFontSize3).BeginInit();
            toolStripFont3.SuspendLayout();
            panel3.SuspendLayout();
            ((ISupportInitialize)tbFontSize2).BeginInit();
            toolStripFont2.SuspendLayout();
            panel2.SuspendLayout();
            ((ISupportInitialize)tbFontSize1).BeginInit();
            toolStripFont1.SuspendLayout();
            panel1.SuspendLayout();
            ((ISupportInitialize)tbFontSize0).BeginInit();
            toolStripFont0.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            //
            // groupBox5
            //
            groupBox5.Controls.Add(OptShowColumns2);
            groupBox5.Controls.Add(OptShowColumns1);
            groupBox5.Location = new Point(131, 352);
            groupBox5.Margin = new Padding(4, 5, 4, 5);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 5, 4, 5);
            groupBox5.Size = new Size(93, 98);
            groupBox5.TabIndex = 2;
            groupBox5.TabStop = false;
            groupBox5.Text = "Columns";
            //
            // OptShowColumns2
            //
            OptShowColumns2.AutoSize = true;
            OptShowColumns2.Location = new Point(8, 58);
            OptShowColumns2.Margin = new Padding(4, 5, 4, 5);
            OptShowColumns2.Name = "OptShowColumns2";
            OptShowColumns2.Size = new Size(79, 24);
            OptShowColumns2.TabIndex = 1;
            OptShowColumns2.Text = "Double";
            //
            // OptShowColumns1
            //
            OptShowColumns1.AutoSize = true;
            OptShowColumns1.Location = new Point(8, 28);
            OptShowColumns1.Margin = new Padding(4, 5, 4, 5);
            OptShowColumns1.Name = "OptShowColumns1";
            OptShowColumns1.Size = new Size(71, 24);
            OptShowColumns1.TabIndex = 0;
            OptShowColumns1.Text = "Single";
            //
            // groupBox7
            //
            groupBox7.Controls.Add(optOneSongPerPage);
            groupBox7.Controls.Add(optNewScreen);
            groupBox7.Controls.Add(panel6);
            groupBox7.Controls.Add(panel7);
            groupBox7.Location = new Point(321, 283);
            groupBox7.Margin = new Padding(4, 5, 4, 5);
            groupBox7.Name = "groupBox7";
            groupBox7.Padding = new Padding(4, 5, 4, 5);
            groupBox7.Size = new Size(296, 168);
            groupBox7.TabIndex = 5;
            groupBox7.TabStop = false;
            groupBox7.Text = "Line Spacing";
            //
            // optOneSongPerPage
            //
            optOneSongPerPage.AutoSize = true;
            optOneSongPerPage.BackColor = Color.Transparent;
            optOneSongPerPage.Location = new Point(15, 132);
            optOneSongPerPage.Margin = new Padding(4, 5, 4, 5);
            optOneSongPerPage.Name = "optOneSongPerPage";
            optOneSongPerPage.Size = new Size(158, 24);
            optOneSongPerPage.TabIndex = 1;
            optOneSongPerPage.Text = "One Song per Page";
            optOneSongPerPage.UseVisualStyleBackColor = false;
            //
            // optNewScreen
            //
            optNewScreen.AutoSize = true;
            optNewScreen.BackColor = Color.Transparent;
            optNewScreen.Location = new Point(15, 103);
            optNewScreen.Margin = new Padding(4, 5, 4, 5);
            optNewScreen.Name = "optNewScreen";
            optNewScreen.Size = new Size(273, 24);
            optNewScreen.TabIndex = 0;
            optNewScreen.Text = "One blank line for each Screen Break";
            optNewScreen.UseVisualStyleBackColor = false;
            //
            // panel6
            //
            panel6.Controls.Add(tbSpacing1);
            panel6.Controls.Add(label6);
            panel6.Location = new Point(13, 63);
            panel6.Margin = new Padding(4, 5, 4, 5);
            panel6.Name = "panel6";
            panel6.Size = new Size(272, 38);
            panel6.TabIndex = 3;
            //
            // tbSpacing1
            //
            tbSpacing1.Location = new Point(215, 5);
            tbSpacing1.Margin = new Padding(4, 5, 4, 5);
            tbSpacing1.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbSpacing1.Name = "tbSpacing1";
            tbSpacing1.Size = new Size(51, 27);
            tbSpacing1.TabIndex = 0;
            //
            // label6
            //
            label6.AutoSize = true;
            label6.Location = new Point(4, 8);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(142, 20);
            label6.TabIndex = 0;
            label6.Text = "Between each Song:";
            //
            // panel7
            //
            panel7.Controls.Add(tbSpacing0);
            panel7.Controls.Add(label7);
            panel7.Location = new Point(13, 26);
            panel7.Margin = new Padding(4, 5, 4, 5);
            panel7.Name = "panel7";
            panel7.Size = new Size(272, 38);
            panel7.TabIndex = 2;
            //
            // tbSpacing0
            //
            tbSpacing0.Location = new Point(215, 5);
            tbSpacing0.Margin = new Padding(4, 5, 4, 5);
            tbSpacing0.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbSpacing0.Name = "tbSpacing0";
            tbSpacing0.Size = new Size(51, 27);
            tbSpacing0.TabIndex = 0;
            //
            // label7
            //
            label7.AutoSize = true;
            label7.Location = new Point(4, 8);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(135, 20);
            label7.TabIndex = 0;
            label7.Text = "Between each Line:";
            //
            // groupBox6
            //
            groupBox6.Controls.Add(OptLyricsPattern1);
            groupBox6.Controls.Add(OptLyricsPattern0);
            groupBox6.Location = new Point(9, 352);
            groupBox6.Margin = new Padding(4, 5, 4, 5);
            groupBox6.Name = "groupBox6";
            groupBox6.Padding = new Padding(4, 5, 4, 5);
            groupBox6.Size = new Size(115, 98);
            groupBox6.TabIndex = 3;
            groupBox6.TabStop = false;
            groupBox6.Text = "Lyrics Pattern";
            //
            // OptLyricsPattern1
            //
            OptLyricsPattern1.AutoSize = true;
            OptLyricsPattern1.Location = new Point(8, 58);
            OptLyricsPattern1.Margin = new Padding(4, 5, 4, 5);
            OptLyricsPattern1.Name = "OptLyricsPattern1";
            OptLyricsPattern1.Size = new Size(94, 24);
            OptLyricsPattern1.TabIndex = 1;
            OptLyricsPattern1.Text = "Sequence";
            //
            // OptLyricsPattern0
            //
            OptLyricsPattern0.AutoSize = true;
            OptLyricsPattern0.Location = new Point(9, 28);
            OptLyricsPattern0.Margin = new Padding(4, 5, 4, 5);
            OptLyricsPattern0.Name = "OptLyricsPattern0";
            OptLyricsPattern0.Size = new Size(64, 24);
            OptLyricsPattern0.TabIndex = 0;
            OptLyricsPattern0.Text = "Basic";
            //
            // optCapoZero
            //
            optCapoZero.AutoSize = true;
            optCapoZero.BackColor = Color.Transparent;
            optCapoZero.Location = new Point(115, 109);
            optCapoZero.Margin = new Padding(4, 5, 4, 5);
            optCapoZero.Name = "optCapoZero";
            optCapoZero.Size = new Size(78, 24);
            optCapoZero.TabIndex = 8;
            optCapoZero.Text = "Capo 0";
            optCapoZero.UseVisualStyleBackColor = false;
            //
            // optShowTiming
            //
            optShowTiming.AutoSize = true;
            optShowTiming.BackColor = Color.Transparent;
            optShowTiming.Location = new Point(211, 108);
            optShowTiming.Margin = new Padding(4, 5, 4, 5);
            optShowTiming.Name = "optShowTiming";
            optShowTiming.Size = new Size(77, 24);
            optShowTiming.TabIndex = 11;
            optShowTiming.Text = "Timing";
            optShowTiming.UseVisualStyleBackColor = false;
            //
            // optPrinterSpaces
            //
            optPrinterSpaces.AutoSize = true;
            optPrinterSpaces.BackColor = Color.Transparent;
            optPrinterSpaces.Location = new Point(155, 218);
            optPrinterSpaces.Margin = new Padding(4, 5, 4, 5);
            optPrinterSpaces.Name = "optPrinterSpaces";
            optPrinterSpaces.Size = new Size(124, 24);
            optPrinterSpaces.TabIndex = 13;
            optPrinterSpaces.Text = "Printer Spaces";
            toolTip1.SetToolTip(optPrinterSpaces, "Add Printer Spaces");
            optPrinterSpaces.UseVisualStyleBackColor = false;
            //
            // optShowCapo
            //
            optShowCapo.AutoSize = true;
            optShowCapo.BackColor = Color.Transparent;
            optShowCapo.Location = new Point(211, 32);
            optShowCapo.Margin = new Padding(4, 5, 4, 5);
            optShowCapo.Name = "optShowCapo";
            optShowCapo.Size = new Size(66, 24);
            optShowCapo.TabIndex = 9;
            optShowCapo.Text = "Capo";
            optShowCapo.UseVisualStyleBackColor = false;
            //
            // optShowKey
            //
            optShowKey.AutoSize = true;
            optShowKey.BackColor = Color.Transparent;
            optShowKey.Location = new Point(211, 69);
            optShowKey.Margin = new Padding(4, 5, 4, 5);
            optShowKey.Name = "optShowKey";
            optShowKey.Size = new Size(55, 24);
            optShowKey.TabIndex = 10;
            optShowKey.Text = "Key";
            optShowKey.UseVisualStyleBackColor = false;
            //
            // optWords5
            //
            optWords5.AutoSize = true;
            optWords5.BackColor = Color.Transparent;
            optWords5.Location = new Point(13, 218);
            optWords5.Margin = new Padding(4, 5, 4, 5);
            optWords5.Name = "optWords5";
            optWords5.Size = new Size(96, 24);
            optWords5.TabIndex = 5;
            optWords5.Text = "Notations";
            optWords5.UseVisualStyleBackColor = false;
            //
            // groupBox3
            //
            groupBox3.Controls.Add(OptShowSection1);
            groupBox3.Controls.Add(OptShowSection0);
            groupBox3.Controls.Add(OptShowSection2);
            groupBox3.Location = new Point(9, 282);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(304, 66);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "Regions";
            //
            // OptShowSection1
            //
            OptShowSection1.AutoSize = true;
            OptShowSection1.Location = new Point(200, 29);
            OptShowSection1.Margin = new Padding(4, 5, 4, 5);
            OptShowSection1.Name = "OptShowSection1";
            OptShowSection1.Size = new Size(89, 24);
            OptShowSection1.TabIndex = 2;
            OptShowSection1.Text = "Region 2";
            //
            // OptShowSection0
            //
            OptShowSection0.AutoSize = true;
            OptShowSection0.Location = new Point(95, 29);
            OptShowSection0.Margin = new Padding(4, 5, 4, 5);
            OptShowSection0.Name = "OptShowSection0";
            OptShowSection0.Size = new Size(89, 24);
            OptShowSection0.TabIndex = 1;
            OptShowSection0.Text = "Region 1";
            //
            // OptShowSection2
            //
            OptShowSection2.AutoSize = true;
            OptShowSection2.Location = new Point(11, 29);
            OptShowSection2.Margin = new Padding(4, 5, 4, 5);
            OptShowSection2.Name = "OptShowSection2";
            OptShowSection2.Size = new Size(61, 24);
            OptShowSection2.TabIndex = 0;
            OptShowSection2.Text = "Both";
            //
            // BtnSaveExit
            //
            BtnSaveExit.Location = new Point(8, 514);
            BtnSaveExit.Margin = new Padding(4, 5, 4, 5);
            BtnSaveExit.Name = "BtnSaveExit";
            BtnSaveExit.Size = new Size(111, 37);
            BtnSaveExit.TabIndex = 6;
            BtnSaveExit.Text = "Save && Close";
            BtnSaveExit.Click += BtnSaveExit_Click;
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(511, 514);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 9;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(396, 514);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 8;
            BtnOK.Text = "Generate";
            BtnOK.Click += BtnOK_Click;
            //
            // Mess1
            //
            Mess1.BackColor = SystemColors.Control;
            Mess1.Location = new Point(16, 480);
            Mess1.Margin = new Padding(4, 0, 4, 0);
            Mess1.Name = "Mess1";
            Mess1.Size = new Size(597, 20);
            Mess1.TabIndex = 39;
            //
            // ProgressBar1
            //
            ProgressBar1.Location = new Point(8, 474);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(609, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 38;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(optHeadings3);
            groupBox2.Controls.Add(optCapoZero);
            groupBox2.Controls.Add(optWords7);
            groupBox2.Controls.Add(optShowTiming);
            groupBox2.Controls.Add(optHeadings2);
            groupBox2.Controls.Add(optPrinterSpaces);
            groupBox2.Controls.Add(optHeadings1);
            groupBox2.Controls.Add(optShowCapo);
            groupBox2.Controls.Add(optHeadings0);
            groupBox2.Controls.Add(optShowKey);
            groupBox2.Controls.Add(optWords6);
            groupBox2.Controls.Add(optWords5);
            groupBox2.Controls.Add(optWords2);
            groupBox2.Controls.Add(optWords1);
            groupBox2.Controls.Add(optWords0);
            groupBox2.Location = new Point(321, 12);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(296, 263);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Show Details";
            //
            // optHeadings3
            //
            optHeadings3.AutoSize = true;
            optHeadings3.BackColor = Color.Transparent;
            optHeadings3.Location = new Point(155, 145);
            optHeadings3.Margin = new Padding(4, 5, 4, 5);
            optHeadings3.Name = "optHeadings3";
            optHeadings3.Size = new Size(140, 24);
            optHeadings3.TabIndex = 14;
            optHeadings3.Text = "PreChorus Head.";
            optHeadings3.UseVisualStyleBackColor = false;
            //
            // optWords7
            //
            optWords7.AutoSize = true;
            optWords7.BackColor = Color.Transparent;
            optWords7.Location = new Point(115, 71);
            optWords7.Margin = new Padding(4, 5, 4, 5);
            optWords7.Name = "optWords7";
            optWords7.Size = new Size(86, 24);
            optWords7.TabIndex = 7;
            optWords7.Text = "User Ref";
            optWords7.UseVisualStyleBackColor = false;
            //
            // optHeadings2
            //
            optHeadings2.AutoSize = true;
            optHeadings2.BackColor = Color.Transparent;
            optHeadings2.Location = new Point(155, 182);
            optHeadings2.Margin = new Padding(4, 5, 4, 5);
            optHeadings2.Name = "optHeadings2";
            optHeadings2.Size = new Size(136, 24);
            optHeadings2.TabIndex = 12;
            optHeadings2.Text = "Bridge Heading";
            optHeadings2.UseVisualStyleBackColor = false;
            //
            // optHeadings1
            //
            optHeadings1.AutoSize = true;
            optHeadings1.BackColor = Color.Transparent;
            optHeadings1.Location = new Point(13, 182);
            optHeadings1.Margin = new Padding(4, 5, 4, 5);
            optHeadings1.Name = "optHeadings1";
            optHeadings1.Size = new Size(137, 24);
            optHeadings1.TabIndex = 4;
            optHeadings1.Text = "Chorus Heading";
            optHeadings1.UseVisualStyleBackColor = false;
            //
            // optHeadings0
            //
            optHeadings0.AutoSize = true;
            optHeadings0.BackColor = Color.Transparent;
            optHeadings0.Location = new Point(13, 145);
            optHeadings0.Margin = new Padding(4, 5, 4, 5);
            optHeadings0.Name = "optHeadings0";
            optHeadings0.Size = new Size(127, 24);
            optHeadings0.TabIndex = 3;
            optHeadings0.Text = "Verse Heading";
            optHeadings0.UseVisualStyleBackColor = false;
            //
            // optWords6
            //
            optWords6.AutoSize = true;
            optWords6.BackColor = Color.Transparent;
            optWords6.Location = new Point(115, 34);
            optWords6.Margin = new Padding(4, 5, 4, 5);
            optWords6.Name = "optWords6";
            optWords6.Size = new Size(91, 24);
            optWords6.TabIndex = 6;
            optWords6.Text = "Book Ref";
            optWords6.UseVisualStyleBackColor = false;
            //
            // optWords2
            //
            optWords2.AutoSize = true;
            optWords2.BackColor = Color.Transparent;
            optWords2.Location = new Point(13, 108);
            optWords2.Margin = new Padding(4, 5, 4, 5);
            optWords2.Name = "optWords2";
            optWords2.Size = new Size(96, 24);
            optWords2.TabIndex = 2;
            optWords2.Text = "Copyright";
            optWords2.UseVisualStyleBackColor = false;
            //
            // optWords1
            //
            optWords1.AutoSize = true;
            optWords1.BackColor = Color.Transparent;
            optWords1.Location = new Point(13, 71);
            optWords1.Margin = new Padding(4, 5, 4, 5);
            optWords1.Name = "optWords1";
            optWords1.Size = new Size(98, 24);
            optWords1.TabIndex = 1;
            optWords1.Text = "Song Title";
            optWords1.UseVisualStyleBackColor = false;
            //
            // optWords0
            //
            optWords0.AutoSize = true;
            optWords0.BackColor = Color.Transparent;
            optWords0.Location = new Point(13, 34);
            optWords0.Margin = new Padding(4, 5, 4, 5);
            optWords0.Name = "optWords0";
            optWords0.Size = new Size(92, 24);
            optWords0.TabIndex = 0;
            optWords0.Text = "Song No.";
            optWords0.UseVisualStyleBackColor = false;
            //
            // groupBox1
            //
            groupBox1.Controls.Add(panel8);
            groupBox1.Controls.Add(panel5);
            groupBox1.Controls.Add(panel4);
            groupBox1.Controls.Add(panel3);
            groupBox1.Controls.Add(panel2);
            groupBox1.Controls.Add(panel1);
            groupBox1.Location = new Point(9, 12);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(304, 263);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Font Size";
            //
            // panel8
            //
            panel8.Controls.Add(PanelFontColour5);
            panel8.Controls.Add(tbFontSize5);
            panel8.Controls.Add(toolStripFont5);
            panel8.Controls.Add(label8);
            panel8.Location = new Point(13, 211);
            panel8.Margin = new Padding(4, 5, 4, 5);
            panel8.Name = "panel8";
            panel8.Size = new Size(280, 38);
            panel8.TabIndex = 5;
            //
            // PanelFontColour5
            //
            PanelFontColour5.BackColor = Color.Black;
            PanelFontColour5.Cursor = Cursors.Hand;
            PanelFontColour5.Location = new Point(256, 6);
            PanelFontColour5.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour5.Name = "PanelFontColour5";
            PanelFontColour5.Size = new Size(21, 28);
            PanelFontColour5.TabIndex = 2;
            PanelFontColour5.Tag = "5";
            toolTip1.SetToolTip(PanelFontColour5, "Set Notations Colour");
            PanelFontColour5.Click += PanelFontColour_Click;
            //
            // tbFontSize5
            //
            tbFontSize5.Location = new Point(200, 5);
            tbFontSize5.Margin = new Padding(4, 5, 4, 5);
            tbFontSize5.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize5.Name = "tbFontSize5";
            tbFontSize5.Size = new Size(51, 27);
            tbFontSize5.TabIndex = 1;
            //
            // toolStripFont5
            //
            toolStripFont5.AutoSize = false;
            toolStripFont5.CanOverflow = false;
            toolStripFont5.Dock = DockStyle.None;
            toolStripFont5.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont5.ImageScalingSize = new Size(20, 20);
            toolStripFont5.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton5, toolStripButton9 });
            toolStripFont5.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont5.Location = new Point(100, 0);
            toolStripFont5.Name = "toolStripFont5";
            toolStripFont5.RenderMode = ToolStripRenderMode.System;
            toolStripFont5.Size = new Size(95, 40);
            toolStripFont5.TabIndex = 0;
            //
            // toolStripButton1
            //
            toolStripButton1.CheckOnClick = true;
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Resources.Bold;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(29, 37);
            toolStripButton1.Tag = "0";
            //
            // toolStripButton5
            //
            toolStripButton5.CheckOnClick = true;
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton5.Image = Resources.Italic;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(29, 37);
            toolStripButton5.Tag = "1";
            //
            // toolStripButton9
            //
            toolStripButton9.CheckOnClick = true;
            toolStripButton9.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton9.Image = Resources.Underline;
            toolStripButton9.ImageTransparentColor = Color.Magenta;
            toolStripButton9.Name = "toolStripButton9";
            toolStripButton9.Size = new Size(29, 37);
            toolStripButton9.Tag = "2";
            //
            // label8
            //
            label8.AutoSize = true;
            label8.Location = new Point(4, 8);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(77, 20);
            label8.TabIndex = 0;
            label8.Text = "Notations:";
            //
            // panel5
            //
            panel5.Controls.Add(PanelFontColour4);
            panel5.Controls.Add(tbFontSize4);
            panel5.Controls.Add(toolStripFont4);
            panel5.Controls.Add(label5);
            panel5.Location = new Point(13, 174);
            panel5.Margin = new Padding(4, 5, 4, 5);
            panel5.Name = "panel5";
            panel5.Size = new Size(280, 38);
            panel5.TabIndex = 4;
            //
            // PanelFontColour4
            //
            PanelFontColour4.BackColor = Color.Black;
            PanelFontColour4.Cursor = Cursors.Hand;
            PanelFontColour4.Location = new Point(256, 6);
            PanelFontColour4.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour4.Name = "PanelFontColour4";
            PanelFontColour4.Size = new Size(21, 28);
            PanelFontColour4.TabIndex = 2;
            PanelFontColour4.Tag = "4";
            toolTip1.SetToolTip(PanelFontColour4, "Set Chorus/Bridge Colour");
            PanelFontColour4.Click += PanelFontColour_Click;
            //
            // tbFontSize4
            //
            tbFontSize4.Location = new Point(200, 5);
            tbFontSize4.Margin = new Padding(4, 5, 4, 5);
            tbFontSize4.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize4.Name = "tbFontSize4";
            tbFontSize4.Size = new Size(51, 27);
            tbFontSize4.TabIndex = 1;
            //
            // toolStripFont4
            //
            toolStripFont4.AutoSize = false;
            toolStripFont4.CanOverflow = false;
            toolStripFont4.Dock = DockStyle.None;
            toolStripFont4.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont4.ImageScalingSize = new Size(20, 20);
            toolStripFont4.Items.AddRange(new ToolStripItem[] { toolStripButton14, toolStripButton15, toolStripButton16 });
            toolStripFont4.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont4.Location = new Point(100, 0);
            toolStripFont4.Name = "toolStripFont4";
            toolStripFont4.RenderMode = ToolStripRenderMode.System;
            toolStripFont4.Size = new Size(95, 40);
            toolStripFont4.TabIndex = 0;
            //
            // toolStripButton14
            //
            toolStripButton14.CheckOnClick = true;
            toolStripButton14.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton14.Image = Resources.Bold;
            toolStripButton14.ImageTransparentColor = Color.Magenta;
            toolStripButton14.Name = "toolStripButton14";
            toolStripButton14.Size = new Size(29, 37);
            toolStripButton14.Tag = "0";
            //
            // toolStripButton15
            //
            toolStripButton15.CheckOnClick = true;
            toolStripButton15.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton15.Image = Resources.Italic;
            toolStripButton15.ImageTransparentColor = Color.Magenta;
            toolStripButton15.Name = "toolStripButton15";
            toolStripButton15.Size = new Size(29, 37);
            toolStripButton15.Tag = "1";
            //
            // toolStripButton16
            //
            toolStripButton16.CheckOnClick = true;
            toolStripButton16.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton16.Image = Resources.Underline;
            toolStripButton16.ImageTransparentColor = Color.Magenta;
            toolStripButton16.Name = "toolStripButton16";
            toolStripButton16.Size = new Size(29, 37);
            toolStripButton16.Tag = "2";
            //
            // label5
            //
            label5.AutoSize = true;
            label5.Location = new Point(4, 8);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(107, 20);
            label5.TabIndex = 0;
            label5.Text = "Chorus/Bridge:";
            //
            // panel4
            //
            panel4.Controls.Add(PanelFontColour3);
            panel4.Controls.Add(tbFontSize3);
            panel4.Controls.Add(toolStripFont3);
            panel4.Controls.Add(label4);
            panel4.Location = new Point(13, 137);
            panel4.Margin = new Padding(4, 5, 4, 5);
            panel4.Name = "panel4";
            panel4.Size = new Size(280, 38);
            panel4.TabIndex = 3;
            //
            // PanelFontColour3
            //
            PanelFontColour3.BackColor = Color.Black;
            PanelFontColour3.Cursor = Cursors.Hand;
            PanelFontColour3.Location = new Point(256, 6);
            PanelFontColour3.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour3.Name = "PanelFontColour3";
            PanelFontColour3.Size = new Size(21, 28);
            PanelFontColour3.TabIndex = 2;
            PanelFontColour3.Tag = "3";
            toolTip1.SetToolTip(PanelFontColour3, "Set Verse Colour");
            PanelFontColour3.Click += PanelFontColour_Click;
            //
            // tbFontSize3
            //
            tbFontSize3.Location = new Point(200, 5);
            tbFontSize3.Margin = new Padding(4, 5, 4, 5);
            tbFontSize3.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize3.Name = "tbFontSize3";
            tbFontSize3.Size = new Size(51, 27);
            tbFontSize3.TabIndex = 1;
            //
            // toolStripFont3
            //
            toolStripFont3.AutoSize = false;
            toolStripFont3.CanOverflow = false;
            toolStripFont3.Dock = DockStyle.None;
            toolStripFont3.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont3.ImageScalingSize = new Size(20, 20);
            toolStripFont3.Items.AddRange(new ToolStripItem[] { toolStripButton10, toolStripButton11, toolStripButton12 });
            toolStripFont3.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont3.Location = new Point(100, 0);
            toolStripFont3.Name = "toolStripFont3";
            toolStripFont3.RenderMode = ToolStripRenderMode.System;
            toolStripFont3.Size = new Size(95, 40);
            toolStripFont3.TabIndex = 0;
            //
            // toolStripButton10
            //
            toolStripButton10.CheckOnClick = true;
            toolStripButton10.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton10.Image = Resources.Bold;
            toolStripButton10.ImageTransparentColor = Color.Magenta;
            toolStripButton10.Name = "toolStripButton10";
            toolStripButton10.Size = new Size(29, 37);
            toolStripButton10.Tag = "0";
            //
            // toolStripButton11
            //
            toolStripButton11.CheckOnClick = true;
            toolStripButton11.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton11.Image = Resources.Italic;
            toolStripButton11.ImageTransparentColor = Color.Magenta;
            toolStripButton11.Name = "toolStripButton11";
            toolStripButton11.Size = new Size(29, 37);
            toolStripButton11.Tag = "1";
            //
            // toolStripButton12
            //
            toolStripButton12.CheckOnClick = true;
            toolStripButton12.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton12.Image = Resources.Underline;
            toolStripButton12.ImageTransparentColor = Color.Magenta;
            toolStripButton12.Name = "toolStripButton12";
            toolStripButton12.Size = new Size(29, 37);
            toolStripButton12.Tag = "2";
            //
            // label4
            //
            label4.AutoSize = true;
            label4.Location = new Point(4, 8);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(47, 20);
            label4.TabIndex = 0;
            label4.Text = "Verse:";
            //
            // panel3
            //
            panel3.Controls.Add(PanelFontColour2);
            panel3.Controls.Add(tbFontSize2);
            panel3.Controls.Add(toolStripFont2);
            panel3.Controls.Add(label3);
            panel3.Location = new Point(13, 100);
            panel3.Margin = new Padding(4, 5, 4, 5);
            panel3.Name = "panel3";
            panel3.Size = new Size(280, 38);
            panel3.TabIndex = 2;
            //
            // PanelFontColour2
            //
            PanelFontColour2.BackColor = Color.Black;
            PanelFontColour2.Cursor = Cursors.Hand;
            PanelFontColour2.Location = new Point(256, 6);
            PanelFontColour2.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour2.Name = "PanelFontColour2";
            PanelFontColour2.Size = new Size(21, 28);
            PanelFontColour2.TabIndex = 2;
            PanelFontColour2.Tag = "2";
            toolTip1.SetToolTip(PanelFontColour2, "Set Copyright/Ref Colour");
            PanelFontColour2.Click += PanelFontColour_Click;
            //
            // tbFontSize2
            //
            tbFontSize2.Location = new Point(200, 5);
            tbFontSize2.Margin = new Padding(4, 5, 4, 5);
            tbFontSize2.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize2.Name = "tbFontSize2";
            tbFontSize2.Size = new Size(51, 27);
            tbFontSize2.TabIndex = 1;
            //
            // toolStripFont2
            //
            toolStripFont2.AutoSize = false;
            toolStripFont2.CanOverflow = false;
            toolStripFont2.Dock = DockStyle.None;
            toolStripFont2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont2.ImageScalingSize = new Size(20, 20);
            toolStripFont2.Items.AddRange(new ToolStripItem[] { toolStripButton6, toolStripButton7, toolStripButton8 });
            toolStripFont2.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont2.Location = new Point(100, 0);
            toolStripFont2.Name = "toolStripFont2";
            toolStripFont2.RenderMode = ToolStripRenderMode.System;
            toolStripFont2.Size = new Size(95, 40);
            toolStripFont2.TabIndex = 0;
            //
            // toolStripButton6
            //
            toolStripButton6.CheckOnClick = true;
            toolStripButton6.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton6.Image = Resources.Bold;
            toolStripButton6.ImageTransparentColor = Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new Size(29, 37);
            toolStripButton6.Tag = "0";
            //
            // toolStripButton7
            //
            toolStripButton7.CheckOnClick = true;
            toolStripButton7.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton7.Image = Resources.Italic;
            toolStripButton7.ImageTransparentColor = Color.Magenta;
            toolStripButton7.Name = "toolStripButton7";
            toolStripButton7.Size = new Size(29, 37);
            toolStripButton7.Tag = "1";
            //
            // toolStripButton8
            //
            toolStripButton8.CheckOnClick = true;
            toolStripButton8.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton8.Image = Resources.Underline;
            toolStripButton8.ImageTransparentColor = Color.Magenta;
            toolStripButton8.Name = "toolStripButton8";
            toolStripButton8.Size = new Size(29, 37);
            toolStripButton8.Tag = "2";
            //
            // label3
            //
            label3.AutoSize = true;
            label3.Location = new Point(4, 8);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(105, 20);
            label3.TabIndex = 0;
            label3.Text = "Copyright/Ref:";
            //
            // panel2
            //
            panel2.Controls.Add(PanelFontColour1);
            panel2.Controls.Add(tbFontSize1);
            panel2.Controls.Add(toolStripFont1);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(13, 63);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new Size(280, 38);
            panel2.TabIndex = 1;
            //
            // PanelFontColour1
            //
            PanelFontColour1.BackColor = Color.Black;
            PanelFontColour1.Cursor = Cursors.Hand;
            PanelFontColour1.Location = new Point(256, 6);
            PanelFontColour1.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour1.Name = "PanelFontColour1";
            PanelFontColour1.Size = new Size(21, 28);
            PanelFontColour1.TabIndex = 2;
            PanelFontColour1.Tag = "1";
            toolTip1.SetToolTip(PanelFontColour1, "Set Song Title Colour");
            PanelFontColour1.Click += PanelFontColour_Click;
            //
            // tbFontSize1
            //
            tbFontSize1.Location = new Point(200, 5);
            tbFontSize1.Margin = new Padding(4, 5, 4, 5);
            tbFontSize1.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize1.Name = "tbFontSize1";
            tbFontSize1.Size = new Size(51, 27);
            tbFontSize1.TabIndex = 1;
            //
            // toolStripFont1
            //
            toolStripFont1.AutoSize = false;
            toolStripFont1.CanOverflow = false;
            toolStripFont1.Dock = DockStyle.None;
            toolStripFont1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont1.ImageScalingSize = new Size(20, 20);
            toolStripFont1.Items.AddRange(new ToolStripItem[] { toolStripButton2, toolStripButton3, toolStripButton4 });
            toolStripFont1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont1.Location = new Point(100, 0);
            toolStripFont1.Name = "toolStripFont1";
            toolStripFont1.RenderMode = ToolStripRenderMode.System;
            toolStripFont1.Size = new Size(95, 40);
            toolStripFont1.TabIndex = 0;
            //
            // toolStripButton2
            //
            toolStripButton2.CheckOnClick = true;
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Resources.Bold;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(29, 37);
            toolStripButton2.Tag = "0";
            //
            // toolStripButton3
            //
            toolStripButton3.CheckOnClick = true;
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton3.Image = Resources.Italic;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(29, 37);
            toolStripButton3.Tag = "1";
            //
            // toolStripButton4
            //
            toolStripButton4.CheckOnClick = true;
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = Resources.Underline;
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(29, 37);
            toolStripButton4.Tag = "2";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(4, 8);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(79, 20);
            label2.TabIndex = 0;
            label2.Text = "Song Title:";
            //
            // panel1
            //
            panel1.Controls.Add(PanelFontColour0);
            panel1.Controls.Add(tbFontSize0);
            panel1.Controls.Add(toolStripFont0);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(13, 26);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(280, 38);
            panel1.TabIndex = 0;
            //
            // PanelFontColour0
            //
            PanelFontColour0.BackColor = Color.Black;
            PanelFontColour0.Cursor = Cursors.Hand;
            PanelFontColour0.Location = new Point(256, 6);
            PanelFontColour0.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour0.Name = "PanelFontColour0";
            PanelFontColour0.Size = new Size(21, 28);
            PanelFontColour0.TabIndex = 2;
            PanelFontColour0.Tag = "0";
            toolTip1.SetToolTip(PanelFontColour0, "Set Song Number Colour");
            PanelFontColour0.Click += PanelFontColour_Click;
            //
            // tbFontSize0
            //
            tbFontSize0.Location = new Point(200, 5);
            tbFontSize0.Margin = new Padding(4, 5, 4, 5);
            tbFontSize0.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize0.Name = "tbFontSize0";
            tbFontSize0.Size = new Size(51, 27);
            tbFontSize0.TabIndex = 1;
            //
            // toolStripFont0
            //
            toolStripFont0.AutoSize = false;
            toolStripFont0.CanOverflow = false;
            toolStripFont0.Dock = DockStyle.None;
            toolStripFont0.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont0.ImageScalingSize = new Size(20, 20);
            toolStripFont0.Items.AddRange(new ToolStripItem[] { toolStripButton100, toolStripButton101, toolStripButton102 });
            toolStripFont0.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont0.Location = new Point(100, 0);
            toolStripFont0.Name = "toolStripFont0";
            toolStripFont0.RenderMode = ToolStripRenderMode.System;
            toolStripFont0.Size = new Size(95, 40);
            toolStripFont0.TabIndex = 0;
            //
            // toolStripButton100
            //
            toolStripButton100.CheckOnClick = true;
            toolStripButton100.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton100.Image = Resources.Bold;
            toolStripButton100.ImageTransparentColor = Color.Magenta;
            toolStripButton100.Name = "toolStripButton100";
            toolStripButton100.Size = new Size(29, 37);
            toolStripButton100.Tag = "0";
            //
            // toolStripButton101
            //
            toolStripButton101.CheckOnClick = true;
            toolStripButton101.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton101.Image = Resources.Italic;
            toolStripButton101.ImageTransparentColor = Color.Magenta;
            toolStripButton101.Name = "toolStripButton101";
            toolStripButton101.Size = new Size(29, 37);
            toolStripButton101.Tag = "1";
            //
            // toolStripButton102
            //
            toolStripButton102.CheckOnClick = true;
            toolStripButton102.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton102.Image = Resources.Underline;
            toolStripButton102.ImageTransparentColor = Color.Magenta;
            toolStripButton102.Name = "toolStripButton102";
            toolStripButton102.Size = new Size(29, 37);
            toolStripButton102.Tag = "2";
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(4, 8);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(104, 20);
            label1.TabIndex = 0;
            label1.Text = "Song Number:";
            //
            // BtnIndexOnly
            //
            BtnIndexOnly.Location = new Point(167, 514);
            BtnIndexOnly.Margin = new Padding(4, 5, 4, 5);
            BtnIndexOnly.Name = "BtnIndexOnly";
            BtnIndexOnly.Size = new Size(107, 37);
            BtnIndexOnly.TabIndex = 7;
            BtnIndexOnly.Text = "Index Only";
            BtnIndexOnly.Click += BtnIndexOnly_Click;
            //
            // BtnTitlesRef
            //
            BtnTitlesRef.Location = new Point(281, 514);
            BtnTitlesRef.Margin = new Padding(4, 5, 4, 5);
            BtnTitlesRef.Name = "BtnTitlesRef";
            BtnTitlesRef.Size = new Size(107, 37);
            BtnTitlesRef.TabIndex = 40;
            BtnTitlesRef.Text = "Titles && Ref";
            BtnTitlesRef.Click += BtnTitlesRef_Click;
            //
            // groupBox4
            //
            groupBox4.Controls.Add(OptPageSize1);
            groupBox4.Controls.Add(OptPageSize0);
            groupBox4.Location = new Point(229, 352);
            groupBox4.Margin = new Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 5, 4, 5);
            groupBox4.Size = new Size(83, 98);
            groupBox4.TabIndex = 41;
            groupBox4.TabStop = false;
            groupBox4.Text = "Size";
            //
            // OptPageSize1
            //
            OptPageSize1.AutoSize = true;
            OptPageSize1.Location = new Point(7, 60);
            OptPageSize1.Margin = new Padding(4, 5, 4, 5);
            OptPageSize1.Name = "OptPageSize1";
            OptPageSize1.Size = new Size(68, 24);
            OptPageSize1.TabIndex = 1;
            OptPageSize1.Text = "Letter";
            //
            // OptPageSize0
            //
            OptPageSize0.AutoSize = true;
            OptPageSize0.Location = new Point(7, 28);
            OptPageSize0.Margin = new Padding(4, 5, 4, 5);
            OptPageSize0.Name = "OptPageSize0";
            OptPageSize0.Size = new Size(48, 24);
            OptPageSize0.TabIndex = 0;
            OptPageSize0.Text = "A4";
            //
            // FrmGenerateDoc
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(629, 569);
            Controls.Add(groupBox4);
            Controls.Add(BtnTitlesRef);
            Controls.Add(BtnIndexOnly);
            Controls.Add(Mess1);
            Controls.Add(groupBox5);
            Controls.Add(groupBox7);
            Controls.Add(groupBox6);
            Controls.Add(groupBox3);
            Controls.Add(BtnSaveExit);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(ProgressBar1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmGenerateDoc";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Generate RTF Document";
            Load += FrmFormatPraiseBookDoc_Load;
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((ISupportInitialize)tbSpacing1).EndInit();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            ((ISupportInitialize)tbSpacing0).EndInit();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            ((ISupportInitialize)tbFontSize5).EndInit();
            toolStripFont5.ResumeLayout(false);
            toolStripFont5.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((ISupportInitialize)tbFontSize4).EndInit();
            toolStripFont4.ResumeLayout(false);
            toolStripFont4.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((ISupportInitialize)tbFontSize3).EndInit();
            toolStripFont3.ResumeLayout(false);
            toolStripFont3.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((ISupportInitialize)tbFontSize2).EndInit();
            toolStripFont2.ResumeLayout(false);
            toolStripFont2.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((ISupportInitialize)tbFontSize1).EndInit();
            toolStripFont1.ResumeLayout(false);
            toolStripFont1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)tbFontSize0).EndInit();
            toolStripFont0.ResumeLayout(false);
            toolStripFont0.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
