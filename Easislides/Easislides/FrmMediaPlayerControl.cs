using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmMediaPlayerControl : Form
	{
		private enum ControlsBtn
		{
			PlayPausebtn,
			Stopbtn,
			FFbtn,
			FRbtn,
			Closebtn
		}

		private IContainer components = null;

		private GroupBox groupBox2;

		private TrackBar TrackBarBalance;

		private TrackBar TrackBarVolume;

		private CheckBox cbMute;

		private Label label3;

		private Label label2;

		private Label label1;

		private Label label4;

		private Label label5;

		private Label label6;

		private TrackBar TrackBarDuration;

		private Label LabelPosition;

		private Label LabelDuration;

		private Label label8;

		private Label label7;

		private Button FastForwardBtn;

		private Button FastReverseBtn;

		private Button PlayPauseBtn;

		private Button StopBtn;

		private Button BtnCancel;

		private Button BtnOK;

		private CheckBox cbRepeat;

		private OpenFileDialog OpenFileDialog1;

		private Timer TimerFast;

		private Timer TimerTrack;

		private GroupBox groupBox1;

		private Panel panelLinkTitle2Lookup;

		private ToolStrip toolStripLocate;

		private ToolStripButton LocationBtn;

		private Panel panel1;

		private TextBox tbSourceLocation;

		private RadioButton SourceOption0;

		private RadioButton SourceOption2;

		private RadioButton SourceOption1;

		private ToolTip toolTip1;

		private Label LabelMediaType;

		private Label label10;

		private Timer TimerAttemptConnect;

		private RadioButton SourceOption3;

		private Panel panelPlayBtns;

		private Label labelNoPlayer1;

		private Label labelNoPlayer2;

		private Panel panelNoPlayer;

		private Label label9;

		private Label label14;

		private Label label13;

		private Label label12;

		private Label label11;

		private CheckBox cbWidescreen;

		private Panel panel47;

		private ToolStrip toolStripCaptureDevices;

		private ToolStripComboBox cbCaptureDevices;

		private Label label15;

		private Label LabelResolution;

		private Label labelOutputMonitor;

		private ComboBox cbOutputMonitor;

		private bool InitLoad = true;

		private double TimeIncrement = 1.0;

		private string PreviousStatus = "";

		private bool PreviousMuteState = false;

		private string Option1MediaFile = "";

		private int AttemptConnectCount = 0;

		private int MaxAttemptConnectCount = 60;

		private DShowLib DShowPlayer = new DShowLib();

		private bool PlayerOK = false;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmMediaPlayerControl));
            groupBox2 = new GroupBox();
            LabelResolution = new Label();
            label15 = new Label();
            cbOutputMonitor = new ComboBox();
            labelOutputMonitor = new Label();
            cbWidescreen = new CheckBox();
            LabelMediaType = new Label();
            label10 = new Label();
            panel1 = new Panel();
            panelNoPlayer = new Panel();
            label9 = new Label();
            labelNoPlayer2 = new Label();
            labelNoPlayer1 = new Label();
            cbRepeat = new CheckBox();
            LabelPosition = new Label();
            LabelDuration = new Label();
            label8 = new Label();
            label7 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label3 = new Label();
            label2 = new Label();
            TrackBarBalance = new TrackBar();
            TrackBarVolume = new TrackBar();
            cbMute = new CheckBox();
            panelPlayBtns = new Panel();
            TrackBarDuration = new TrackBar();
            StopBtn = new Button();
            PlayPauseBtn = new Button();
            FastReverseBtn = new Button();
            FastForwardBtn = new Button();
            label1 = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            OpenFileDialog1 = new OpenFileDialog();
            TimerFast = new Timer(components);
            TimerTrack = new Timer(components);
            groupBox1 = new GroupBox();
            panel47 = new Panel();
            toolStripCaptureDevices = new ToolStrip();
            cbCaptureDevices = new ToolStripComboBox();
            label14 = new Label();
            label13 = new Label();
            label12 = new Label();
            label11 = new Label();
            SourceOption1 = new RadioButton();
            SourceOption2 = new RadioButton();
            SourceOption0 = new RadioButton();
            panelLinkTitle2Lookup = new Panel();
            toolStripLocate = new ToolStrip();
            LocationBtn = new ToolStripButton();
            tbSourceLocation = new TextBox();
            SourceOption3 = new RadioButton();
            toolTip1 = new ToolTip(components);
            TimerAttemptConnect = new Timer(components);
            groupBox2.SuspendLayout();
            panel1.SuspendLayout();
            panelNoPlayer.SuspendLayout();
            ((ISupportInitialize)TrackBarBalance).BeginInit();
            ((ISupportInitialize)TrackBarVolume).BeginInit();
            panelPlayBtns.SuspendLayout();
            ((ISupportInitialize)TrackBarDuration).BeginInit();
            groupBox1.SuspendLayout();
            panel47.SuspendLayout();
            toolStripCaptureDevices.SuspendLayout();
            panelLinkTitle2Lookup.SuspendLayout();
            toolStripLocate.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(LabelResolution);
            groupBox2.Controls.Add(label15);
            groupBox2.Controls.Add(cbOutputMonitor);
            groupBox2.Controls.Add(labelOutputMonitor);
            groupBox2.Controls.Add(cbWidescreen);
            groupBox2.Controls.Add(LabelMediaType);
            groupBox2.Controls.Add(label10);
            groupBox2.Controls.Add(panel1);
            groupBox2.Controls.Add(cbRepeat);
            groupBox2.Controls.Add(LabelPosition);
            groupBox2.Controls.Add(LabelDuration);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(TrackBarBalance);
            groupBox2.Controls.Add(TrackBarVolume);
            groupBox2.Controls.Add(cbMute);
            groupBox2.Controls.Add(panelPlayBtns);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(12, 171);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(644, 395);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Settings";
            // 
            // LabelResolution
            // 
            LabelResolution.AutoSize = true;
            LabelResolution.Location = new Point(95, 278);
            LabelResolution.Margin = new Padding(4, 0, 4, 0);
            LabelResolution.Name = "LabelResolution";
            LabelResolution.Size = new Size(36, 20);
            LabelResolution.TabIndex = 26;
            LabelResolution.Text = "N/A";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(17, 278);
            label15.Margin = new Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new Size(82, 20);
            label15.TabIndex = 25;
            label15.Text = "Resolution:";
            // 
            // cbOutputMonitor
            // 
            cbOutputMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOutputMonitor.FormattingEnabled = true;
            cbOutputMonitor.Location = new Point(136, 346);
            cbOutputMonitor.Margin = new Padding(4, 5, 4, 5);
            cbOutputMonitor.Name = "cbOutputMonitor";
            cbOutputMonitor.Size = new Size(200, 28);
            cbOutputMonitor.TabIndex = 28;
            // 
            // labelOutputMonitor
            // 
            labelOutputMonitor.AutoSize = true;
            labelOutputMonitor.Location = new Point(17, 350);
            labelOutputMonitor.Margin = new Padding(4, 0, 4, 0);
            labelOutputMonitor.Name = "labelOutputMonitor";
            labelOutputMonitor.Size = new Size(115, 20);
            labelOutputMonitor.TabIndex = 27;
            labelOutputMonitor.Text = "Output Monitor:";
            // 
            // cbWidescreen
            // 
            cbWidescreen.AutoSize = true;
            cbWidescreen.Location = new Point(115, 320);
            cbWidescreen.Margin = new Padding(4, 5, 4, 5);
            cbWidescreen.Name = "cbWidescreen";
            cbWidescreen.Size = new Size(150, 24);
            cbWidescreen.TabIndex = 24;
            cbWidescreen.Text = "Force WideScreen";
            cbWidescreen.CheckedChanged += cbWidescreen_CheckedChanged;
            // 
            // LabelMediaType
            // 
            LabelMediaType.AutoSize = true;
            LabelMediaType.ForeColor = Color.Red;
            LabelMediaType.Location = new Point(95, 249);
            LabelMediaType.Margin = new Padding(4, 0, 4, 0);
            LabelMediaType.Name = "LabelMediaType";
            LabelMediaType.Size = new Size(45, 20);
            LabelMediaType.TabIndex = 7;
            LabelMediaType.Text = "None";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(17, 249);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(54, 20);
            label10.TabIndex = 21;
            label10.Text = "Media:";
            // 
            // panel1
            // 
            panel1.BackColor = Color.Black;
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Controls.Add(panelNoPlayer);
            panel1.Location = new Point(319, 25);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(303, 267);
            panel1.TabIndex = 0;
            // 
            // panelNoPlayer
            // 
            panelNoPlayer.BackColor = Color.MidnightBlue;
            panelNoPlayer.Controls.Add(label9);
            panelNoPlayer.Controls.Add(labelNoPlayer2);
            panelNoPlayer.Controls.Add(labelNoPlayer1);
            panelNoPlayer.ForeColor = Color.White;
            panelNoPlayer.Location = new Point(0, 0);
            panelNoPlayer.Margin = new Padding(4, 5, 4, 5);
            panelNoPlayer.Name = "panelNoPlayer";
            panelNoPlayer.Size = new Size(299, 263);
            panelNoPlayer.TabIndex = 60;
            // 
            // label9
            // 
            label9.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(19, 128);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(256, 66);
            label9.TabIndex = 25;
            label9.Text = "to view / listen to Media Backgrounds.";
            label9.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelNoPlayer2
            // 
            labelNoPlayer2.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelNoPlayer2.Location = new Point(8, 72);
            labelNoPlayer2.Margin = new Padding(4, 0, 4, 0);
            labelNoPlayer2.Name = "labelNoPlayer2";
            labelNoPlayer2.Size = new Size(287, 62);
            labelNoPlayer2.TabIndex = 24;
            labelNoPlayer2.Text = "Windows Media Player 10 or DirectX 9";
            labelNoPlayer2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelNoPlayer1
            // 
            labelNoPlayer1.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelNoPlayer1.Location = new Point(19, 43);
            labelNoPlayer1.Margin = new Padding(4, 0, 4, 0);
            labelNoPlayer1.Name = "labelNoPlayer1";
            labelNoPlayer1.Size = new Size(256, 31);
            labelNoPlayer1.TabIndex = 0;
            labelNoPlayer1.Text = "Please install";
            labelNoPlayer1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cbRepeat
            // 
            cbRepeat.AutoSize = true;
            cbRepeat.Location = new Point(19, 320);
            cbRepeat.Margin = new Padding(4, 5, 4, 5);
            cbRepeat.Name = "cbRepeat";
            cbRepeat.Size = new Size(78, 24);
            cbRepeat.TabIndex = 8;
            cbRepeat.Text = "Repeat";
            cbRepeat.CheckedChanged += cbRepeat_CheckedChanged;
            // 
            // LabelPosition
            // 
            LabelPosition.AutoSize = true;
            LabelPosition.Location = new Point(95, 220);
            LabelPosition.Margin = new Padding(4, 0, 4, 0);
            LabelPosition.Name = "LabelPosition";
            LabelPosition.Size = new Size(36, 20);
            LabelPosition.TabIndex = 6;
            LabelPosition.Text = "0:00";
            // 
            // LabelDuration
            // 
            LabelDuration.AutoSize = true;
            LabelDuration.Location = new Point(95, 192);
            LabelDuration.Margin = new Padding(4, 0, 4, 0);
            LabelDuration.Name = "LabelDuration";
            LabelDuration.Size = new Size(36, 20);
            LabelDuration.TabIndex = 5;
            LabelDuration.Text = "0:00";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(17, 220);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(64, 20);
            label8.TabIndex = 13;
            label8.Text = "Position:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(17, 192);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(70, 20);
            label7.TabIndex = 12;
            label7.Text = "Duration:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 132);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(64, 20);
            label4.TabIndex = 3;
            label4.Text = "Balance:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(275, 134);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(18, 20);
            label5.TabIndex = 9;
            label5.Text = "R";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(111, 134);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(16, 20);
            label6.TabIndex = 8;
            label6.Text = "L";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 77);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(62, 20);
            label3.TabIndex = 1;
            label3.Text = "Volume:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(275, 77);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(37, 20);
            label2.TabIndex = 6;
            label2.Text = "Max";
            // 
            // TrackBarBalance
            // 
            TrackBarBalance.AutoSize = false;
            TrackBarBalance.Location = new Point(115, 128);
            TrackBarBalance.Margin = new Padding(4, 5, 4, 5);
            TrackBarBalance.Maximum = 100;
            TrackBarBalance.Minimum = -100;
            TrackBarBalance.Name = "TrackBarBalance";
            TrackBarBalance.Size = new Size(167, 57);
            TrackBarBalance.TabIndex = 4;
            TrackBarBalance.TickFrequency = 20;
            TrackBarBalance.ValueChanged += TrackBarBalance_ValueChanged;
            // 
            // TrackBarVolume
            // 
            TrackBarVolume.AutoSize = false;
            TrackBarVolume.Location = new Point(115, 58);
            TrackBarVolume.Margin = new Padding(4, 5, 4, 5);
            TrackBarVolume.Maximum = 100;
            TrackBarVolume.Name = "TrackBarVolume";
            TrackBarVolume.Size = new Size(167, 54);
            TrackBarVolume.TabIndex = 2;
            TrackBarVolume.TickFrequency = 10;
            TrackBarVolume.TickStyle = TickStyle.Both;
            TrackBarVolume.ValueChanged += TrackBarVolume_ValueChanged;
            // 
            // cbMute
            // 
            cbMute.AutoSize = true;
            cbMute.Location = new Point(20, 32);
            cbMute.Margin = new Padding(4, 5, 4, 5);
            cbMute.Name = "cbMute";
            cbMute.Size = new Size(65, 24);
            cbMute.TabIndex = 0;
            cbMute.Text = "Mute";
            cbMute.CheckedChanged += cbMute_CheckedChanged;
            // 
            // panelPlayBtns
            // 
            panelPlayBtns.Controls.Add(TrackBarDuration);
            panelPlayBtns.Controls.Add(StopBtn);
            panelPlayBtns.Controls.Add(PlayPauseBtn);
            panelPlayBtns.Controls.Add(FastReverseBtn);
            panelPlayBtns.Controls.Add(FastForwardBtn);
            panelPlayBtns.Location = new Point(308, 292);
            panelPlayBtns.Margin = new Padding(4, 5, 4, 5);
            panelPlayBtns.Name = "panelPlayBtns";
            panelPlayBtns.Size = new Size(328, 68);
            panelPlayBtns.TabIndex = 23;
            // 
            // TrackBarDuration
            // 
            TrackBarDuration.AutoSize = false;
            TrackBarDuration.Location = new Point(0, 0);
            TrackBarDuration.Margin = new Padding(4, 5, 4, 5);
            TrackBarDuration.Maximum = 1000;
            TrackBarDuration.Name = "TrackBarDuration";
            TrackBarDuration.Size = new Size(323, 28);
            TrackBarDuration.TabIndex = 0;
            TrackBarDuration.TickFrequency = 0;
            TrackBarDuration.TickStyle = TickStyle.None;
            TrackBarDuration.Scroll += TrackBarDuration_Scroll;
            // 
            // StopBtn
            // 
            StopBtn.FlatStyle = FlatStyle.Flat;
            StopBtn.Location = new Point(216, 28);
            StopBtn.Margin = new Padding(4, 5, 4, 5);
            StopBtn.Name = "StopBtn";
            StopBtn.Size = new Size(56, 34);
            StopBtn.TabIndex = 4;
            StopBtn.Text = "Stop";
            StopBtn.Click += StopBtn_Click;
            // 
            // PlayPauseBtn
            // 
            PlayPauseBtn.FlatStyle = FlatStyle.Flat;
            PlayPauseBtn.Location = new Point(148, 28);
            PlayPauseBtn.Margin = new Padding(4, 5, 4, 5);
            PlayPauseBtn.Name = "PlayPauseBtn";
            PlayPauseBtn.Size = new Size(64, 34);
            PlayPauseBtn.TabIndex = 3;
            PlayPauseBtn.Text = "Play";
            PlayPauseBtn.Click += PlayPauseBtn_Click;
            // 
            // FastReverseBtn
            // 
            FastReverseBtn.FlatStyle = FlatStyle.Flat;
            FastReverseBtn.Location = new Point(52, 28);
            FastReverseBtn.Margin = new Padding(4, 5, 4, 5);
            FastReverseBtn.Name = "FastReverseBtn";
            FastReverseBtn.Size = new Size(43, 34);
            FastReverseBtn.TabIndex = 1;
            FastReverseBtn.Text = "<<";
            FastReverseBtn.MouseDown += FastReverseBtn_MouseDown;
            FastReverseBtn.MouseUp += FastReverseBtn_MouseUp;
            // 
            // FastForwardBtn
            // 
            FastForwardBtn.FlatStyle = FlatStyle.Flat;
            FastForwardBtn.Location = new Point(100, 28);
            FastForwardBtn.Margin = new Padding(4, 5, 4, 5);
            FastForwardBtn.Name = "FastForwardBtn";
            FastForwardBtn.Size = new Size(43, 34);
            FastForwardBtn.TabIndex = 2;
            FastForwardBtn.Text = ">>";
            FastForwardBtn.MouseDown += FastForwardBtn_MouseDown;
            FastForwardBtn.MouseUp += FastForwardBtn_MouseUp;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(87, 77);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(34, 20);
            label1.TabIndex = 5;
            label1.Text = "Min";
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(341, 585);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 3;
            BtnCancel.Text = "Cancel";
            // 
            // BtnOK
            // 
            BtnOK.DialogResult = DialogResult.OK;
            BtnOK.Location = new Point(213, 585);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 2;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            // 
            // OpenFileDialog1
            // 
            OpenFileDialog1.FileName = "openFileDialog1";
            // 
            // TimerFast
            // 
            TimerFast.Interval = 500;
            TimerFast.Tick += TimerFast_Tick;
            // 
            // TimerTrack
            // 
            TimerTrack.Interval = 1000;
            TimerTrack.Tick += TimerTrack_Tick;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel47);
            groupBox1.Controls.Add(label14);
            groupBox1.Controls.Add(label13);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(SourceOption1);
            groupBox1.Controls.Add(SourceOption2);
            groupBox1.Controls.Add(SourceOption0);
            groupBox1.Controls.Add(panelLinkTitle2Lookup);
            groupBox1.Controls.Add(tbSourceLocation);
            groupBox1.Controls.Add(SourceOption3);
            groupBox1.Location = new Point(12, 8);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(644, 158);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Source Option";
            // 
            // panel47
            // 
            panel47.Controls.Add(toolStripCaptureDevices);
            panel47.Location = new Point(171, 117);
            panel47.Margin = new Padding(4, 5, 4, 5);
            panel47.Name = "panel47";
            panel47.Size = new Size(308, 34);
            panel47.TabIndex = 71;
            // 
            // toolStripCaptureDevices
            // 
            toolStripCaptureDevices.AutoSize = false;
            toolStripCaptureDevices.CanOverflow = false;
            toolStripCaptureDevices.Dock = DockStyle.None;
            toolStripCaptureDevices.GripStyle = ToolStripGripStyle.Hidden;
            toolStripCaptureDevices.ImageScalingSize = new Size(20, 20);
            toolStripCaptureDevices.Items.AddRange(new ToolStripItem[] { cbCaptureDevices });
            toolStripCaptureDevices.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripCaptureDevices.Location = new Point(1, -2);
            toolStripCaptureDevices.Name = "toolStripCaptureDevices";
            toolStripCaptureDevices.RenderMode = ToolStripRenderMode.System;
            toolStripCaptureDevices.Size = new Size(323, 38);
            toolStripCaptureDevices.TabIndex = 5;
            // 
            // cbCaptureDevices
            // 
            cbCaptureDevices.AutoSize = false;
            cbCaptureDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCaptureDevices.Name = "cbCaptureDevices";
            cbCaptureDevices.Size = new Size(292, 28);
            cbCaptureDevices.SelectedIndexChanged += cbCaptureDevicesAndTypes_SelectedIndexChanged;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(8, 123);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(20, 20);
            label14.TabIndex = 66;
            label14.Text = "4:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(8, 91);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new Size(20, 20);
            label13.TabIndex = 65;
            label13.Text = "3:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(8, 58);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(20, 20);
            label12.TabIndex = 64;
            label12.Text = "2:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(8, 28);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(20, 20);
            label11.TabIndex = 63;
            label11.Text = "1:";
            // 
            // SourceOption1
            // 
            SourceOption1.AutoSize = true;
            SourceOption1.Location = new Point(37, 55);
            SourceOption1.Margin = new Padding(4, 5, 4, 5);
            SourceOption1.MaximumSize = new Size(617, 26);
            SourceOption1.Name = "SourceOption1";
            SourceOption1.Size = new Size(34, 24);
            SourceOption1.TabIndex = 1;
            SourceOption1.Text = " ";
            SourceOption1.TextAlign = ContentAlignment.TopLeft;
            // 
            // SourceOption2
            // 
            SourceOption2.AutoSize = true;
            SourceOption2.Location = new Point(37, 91);
            SourceOption2.Margin = new Padding(4, 5, 4, 5);
            SourceOption2.Name = "SourceOption2";
            SourceOption2.Size = new Size(17, 16);
            SourceOption2.TabIndex = 2;
            SourceOption2.CheckedChanged += SourceOption2_CheckedChanged;
            // 
            // SourceOption0
            // 
            SourceOption0.AutoSize = true;
            SourceOption0.Location = new Point(37, 25);
            SourceOption0.Margin = new Padding(4, 5, 4, 5);
            SourceOption0.Name = "SourceOption0";
            SourceOption0.Size = new Size(96, 24);
            SourceOption0.TabIndex = 0;
            SourceOption0.Text = "No Media";
            // 
            // panelLinkTitle2Lookup
            // 
            panelLinkTitle2Lookup.Controls.Add(toolStripLocate);
            panelLinkTitle2Lookup.Location = new Point(605, 83);
            panelLinkTitle2Lookup.Margin = new Padding(4, 5, 4, 5);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(31, 35);
            panelLinkTitle2Lookup.TabIndex = 3;
            // 
            // toolStripLocate
            // 
            toolStripLocate.AutoSize = false;
            toolStripLocate.CanOverflow = false;
            toolStripLocate.Dock = DockStyle.None;
            toolStripLocate.GripStyle = ToolStripGripStyle.Hidden;
            toolStripLocate.ImageScalingSize = new Size(20, 20);
            toolStripLocate.Items.AddRange(new ToolStripItem[] { LocationBtn });
            toolStripLocate.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStripLocate.Location = new Point(0, -2);
            toolStripLocate.Name = "toolStripLocate";
            toolStripLocate.RenderMode = ToolStripRenderMode.System;
            toolStripLocate.Size = new Size(33, 43);
            toolStripLocate.TabIndex = 5;
            // 
            // LocationBtn
            // 
            LocationBtn.AutoSize = false;
            LocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LocationBtn.Image = (Image)resources.GetObject("LocationBtn.Image");
            LocationBtn.ImageTransparentColor = Color.Magenta;
            LocationBtn.Name = "LocationBtn";
            LocationBtn.Size = new Size(22, 22);
            LocationBtn.Tag = "";
            LocationBtn.ToolTipText = "Media File Location";
            LocationBtn.MouseUp += LocationBtn_MouseUp;
            // 
            // tbSourceLocation
            // 
            tbSourceLocation.Location = new Point(60, 85);
            tbSourceLocation.Margin = new Padding(4, 5, 4, 5);
            tbSourceLocation.Name = "tbSourceLocation";
            tbSourceLocation.Size = new Size(537, 27);
            tbSourceLocation.TabIndex = 62;
            tbSourceLocation.TextChanged += tbSourceLocation_TextChanged;
            // 
            // SourceOption3
            // 
            SourceOption3.AutoSize = true;
            SourceOption3.Location = new Point(37, 120);
            SourceOption3.Margin = new Padding(4, 5, 4, 5);
            SourceOption3.Name = "SourceOption3";
            SourceOption3.Size = new Size(131, 24);
            SourceOption3.TabIndex = 4;
            SourceOption3.Text = "Live Feed from:";
            SourceOption3.CheckedChanged += SourceOption3_CheckedChanged;
            // 
            // TimerAttemptConnect
            // 
            TimerAttemptConnect.Interval = 500;
            TimerAttemptConnect.Tick += TimerAttemptConnect_Tick;
            // 
            // FrmMediaPlayerControl
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(668, 640);
            Controls.Add(groupBox1);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(groupBox2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmMediaPlayerControl";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Assign Media";
            FormClosing += FrmMediaPlayerControl_FormClosing;
            Load += FrmMediaPlayerControl_Load;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel1.ResumeLayout(false);
            panelNoPlayer.ResumeLayout(false);
            ((ISupportInitialize)TrackBarBalance).EndInit();
            ((ISupportInitialize)TrackBarVolume).EndInit();
            panelPlayBtns.ResumeLayout(false);
            ((ISupportInitialize)TrackBarDuration).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel47.ResumeLayout(false);
            toolStripCaptureDevices.ResumeLayout(false);
            toolStripCaptureDevices.PerformLayout();
            panelLinkTitle2Lookup.ResumeLayout(false);
            toolStripLocate.ResumeLayout(false);
            toolStripLocate.PerformLayout();
            ResumeLayout(false);
        }

        public FrmMediaPlayerControl()
		{
			InitializeComponent();
		}

		private void FrmMediaPlayerControl_Load(object sender, EventArgs e)
		{
			groupBox1.Enabled = ((!(gf.Temp_MediaItemType == "M") || gf.MPC_Type != MPCType.Individual) ? true : false);
			if (gf.MPC_Type == MPCType.Individual)
			{
				Text = "Assign Media " + ((gf.Temp_MediaTitle1 != "") ? "for " : "") + gf.Temp_MediaTitle1;
			}
			else
			{
				Text = "Assign Media - Default Settings";
			}
			SourceOption1.Text = "Play Media File based on Item Title (if any)";
			gf.LoadBlankCaptureDevices(ref cbCaptureDevices);
			InitMediaPlayer();
			LoadOutputMonitors();
			tbSourceLocation.Text = gf.Temp_MediaLocation;
			cbCaptureDevices.SelectedIndex = gf.Temp_MediaCaptureDeviceNumber - 1;
			TrackBarVolume.Value = (((gf.Temp_MediaVolume >= 0) & (gf.Temp_MediaVolume <= 100)) ? gf.Temp_MediaVolume : 50);
			TrackBarBalance.Value = (((gf.Temp_MediaBalance >= -100) & (gf.Temp_MediaBalance <= 100)) ? gf.Temp_MediaBalance : 0);
			cbMute.Checked = ((gf.Temp_MediaMute > 0) ? true : false);
			cbRepeat.Checked = ((gf.Temp_MediaRepeat > 0) ? true : false);
			cbWidescreen.Checked = ((gf.Temp_MediaWidescreen > 0) ? true : false);
			LabelMediaType.Text = "";
			LabelResolution.Text = "";
			AssignSourceOption(gf.Temp_MediaOption);
			ApplySoundControls(ApplyMute: false);
			TimerTrack.Start();
			InitLoad = false;
		}

		private void InitMediaPlayer()
		{
			if (gf.WMP_Present)
			{
				try
				{
					DShowPlayer.Parent = this;
					DShowPlayer.Parent = panel1;
					DShowPlayer.Location = new Point(0, 0);
					DShowPlayer.SetDefaultSize(0, 0, panel1.Width, panel1.Height, (VAlign)gf.VideoVAlign);
					DShowPlayer.ForeColorChanged += DShowPlayer_ForeColorChanged;
					DShowPlayer.ListCaptureDevices(ref cbCaptureDevices);
					PlayerOK = true;
				}
				catch
				{
					PlayerOK = false;
				}
			}
			if (PlayerOK)
			{
				DShowPlayer.Dock = DockStyle.Fill;
				DShowPlayer.newFilename = tbSourceLocation.Text;
				panelNoPlayer.Visible = false;
				EnableMediaControls(MediaOn: true);
				DShowPlayer.Visible = true;
			}
			else
			{
				EnableMediaControls(MediaOn: false);
			}
		}

		private void LoadOutputMonitors()
		{
			cbOutputMonitor.Items.Clear();
			cbOutputMonitor.Items.Add("Default (Output Monitor)");
			foreach (Screen screen in Screen.AllScreens)
			{
				cbOutputMonitor.Items.Add(screen.DeviceName);
			}
			SelectOutputMonitor(gf.Temp_MediaOutputMonitorName);
		}

		private void SelectOutputMonitor(string monitorName)
		{
			if (!string.IsNullOrEmpty(monitorName))
			{
				int index = cbOutputMonitor.Items.IndexOf(monitorName);
				if (index >= 0)
				{
					cbOutputMonitor.SelectedIndex = index;
					return;
				}
			}
			cbOutputMonitor.SelectedIndex = 0;
		}

		private void EnableMediaControls(bool MediaOn)
		{
			panelNoPlayer.Visible = !MediaOn;
			panelPlayBtns.Enabled = MediaOn;
		}

		private void AssignSourceOption(int InOption)
		{
			switch (InOption)
			{
			case 1:
				SourceOption1.Checked = true;
				break;
			case 2:
				SourceOption2.Checked = true;
				break;
			case 3:
				SourceOption3.Checked = true;
				break;
			default:
				SourceOption0.Checked = true;
				break;
			}
		}

		private void TrackBarVolume_ValueChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void TrackBarBalance_ValueChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void cbMute_CheckedChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void cbRepeat_CheckedChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void cbWidescreen_CheckedChanged(object sender, EventArgs e)
		{
			SetWideScreen(cbWidescreen.Checked);
		}

		private void SetWideScreen(bool InMode)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetWideScreen(InMode, ResizeWindow: true);
				LabelResolution.Text = DShowPlayer.GetVideoSize();
			}
		}

		private void ApplySoundControls(bool ApplyMute)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetVolume(TrackBarVolume.Value);
				DShowPlayer.SetBalance(TrackBarBalance.Value);
				DShowPlayer.SetMute(ApplyMute || cbMute.Checked);
				DShowPlayer.LoopClip = cbRepeat.Checked;
			}
		}

		private void PlayPauseBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.PlayPausebtn);
		}

		private void StopBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.Stopbtn);
		}

		private void FastReverseBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FRbtn);
		}

		private void FastReverseBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void FastForwardBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FFbtn);
		}

		private void FastForwardBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void StorePreviousStatus()
		{
			PreviousMuteState = cbMute.Checked;
		}

		private void ReturnToPreviousState()
		{
			TimerFast.Stop();
			ApplySoundControls(ApplyMute: false);
		}

		private void ApplyPlayControls(ControlsBtn InAction)
		{
			if (!PlayerOK)
			{
				return;
			}
			TimerFast.Stop();
			switch (InAction)
			{
			case ControlsBtn.PlayPausebtn:
			{
				if (!SourceOption3.Checked && (DShowPlayer.currentState == PlayState.Running || DShowPlayer.currentState == PlayState.Paused))
				{
					DShowPlayer.PausePlayClip();
					break;
				}
				tbSourceLocation.Text = DataUtil.Trim(tbSourceLocation.Text);
				int selectedSourceOption = GetSelectedSourceOption();
				try
				{
					switch (selectedSourceOption)
					{
					case 1:
						Option1MediaFile = gf.GetMediaFileName(gf.Temp_MediaTitle1, gf.Temp_MediaTitle2);
						if (Option1MediaFile == "")
						{
							SourceOption1.Text = "Play Media File based on Item Title (if any)";
							toolTip1.SetToolTip(SourceOption1, "");
						}
						else
						{
							SourceOption1.Text = Option1MediaFile;
							toolTip1.SetToolTip(SourceOption1, SourceOption1.Text);
						}
						DShowPlayer.newFilename = Option1MediaFile;
						break;
					case 2:
						DShowPlayer.newFilename = tbSourceLocation.Text;
						break;
					case 3:
						DShowPlayer.newFilename = "<<Capture>>";
						DShowPlayer.currentInputDevice = cbCaptureDevices.SelectedIndex + 1;
						break;
					default:
						DShowPlayer.newFilename = "";
						break;
					}
					SetWideScreen(cbWidescreen.Checked);
					if (selectedSourceOption == 3 || DShowPlayer.newFilename != "")
					{
						DShowPlayer.OpenClip();
						AttemptConnectCount = 0;
						LabelMediaType.Text = DShowPlayer.GetStatusText();
						LabelResolution.Text = DShowPlayer.GetVideoSize();
					}
					else
					{
						ResetMediaMessages();
					}
				}
				catch
				{
					DShowPlayer.newFilename = "";
					ResetMediaMessages();
				}
				return;
			}
			case ControlsBtn.Stopbtn:
				DShowPlayer.StopClip();
				break;
			case ControlsBtn.FFbtn:
				StorePreviousStatus();
				ApplySoundControls(ApplyMute: true);
				IncrementCurrentPosition(1.0);
				TimeIncrement = 5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.FRbtn:
				ApplySoundControls(ApplyMute: true);
				StorePreviousStatus();
				IncrementCurrentPosition(-1.0);
				TimeIncrement = -5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.Closebtn:
				DShowPlayer.StopClip();
				break;
			}
			Cursor = Cursors.Default;
		}

		private void ResetMediaMessages()
		{
			if (PlayerOK)
			{
				LabelMediaType.Text = DShowPlayer.GetStatusText();
				LabelResolution.Text = DShowPlayer.GetVideoSize();
			}
			else
			{
				LabelMediaType.Text = "";
				LabelResolution.Text = "";
			}
			Cursor = Cursors.Default;
		}

		private void DShowPlayer_ForeColorChanged(object sender, EventArgs e)
		{
			switch (DShowPlayer.currentState)
			{
			case PlayState.Running:
				PlayPauseBtn.Enabled = true;
				StopBtn.Enabled = true;
				PlayPauseBtn.Text = "Pause";
				LabelMediaType.Text = DShowPlayer.GetStatusText();
				LabelResolution.Text = DShowPlayer.GetVideoSize();
				Cursor = Cursors.Default;
				break;
			case PlayState.Paused:
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			case PlayState.Stopped:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			default:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				break;
			}
		}

		private void FrmMediaPlayerControl_FormClosing(object sender, FormClosingEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.Closebtn);
			DShowPlayer.TidyUp();
			TimerTrack.Stop();
			TimerFast.Stop();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			gf.Temp_MediaOption = GetSelectedSourceOption();
			gf.Temp_MediaLocation = DataUtil.Trim(tbSourceLocation.Text);
			gf.Temp_MediaCaptureDeviceNumber = cbCaptureDevices.SelectedIndex + 1;
			gf.Temp_MediaOutputMonitorName = (cbOutputMonitor.SelectedIndex > 0) ? cbOutputMonitor.SelectedItem.ToString() : "";
			gf.Temp_MediaVolume = TrackBarVolume.Value;
			gf.Temp_MediaBalance = TrackBarBalance.Value;
			gf.Temp_MediaMute = (cbMute.Checked ? 1 : 0);
			gf.Temp_MediaRepeat = (cbRepeat.Checked ? 1 : 0);
			gf.Temp_MediaWidescreen = (cbWidescreen.Checked ? 1 : 0);
		}

		private int GetSelectedSourceOption()
		{
			if (SourceOption1.Checked)
			{
				return 1;
			}
			if (SourceOption2.Checked)
			{
				return 2;
			}
			if (SourceOption3.Checked)
			{
				return 3;
			}
			return 0;
		}

		private void LocationBtn_MouseUp(object sender, MouseEventArgs e)
		{
			OpenFileDialog1.Filter = gf.GetOpenFileDialogMediaString();
			OpenFileDialog1.InitialDirectory = gf.MediaDir;
			OpenFileDialog1.AddExtension = true;
			tbSourceLocation.Text = DataUtil.Trim(tbSourceLocation.Text);
			OpenFileDialog1.FileName = tbSourceLocation.Text;
			bool flag = false;
			try
			{
				if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
				{
					ApplyPlayControls(ControlsBtn.Stopbtn);
					tbSourceLocation.Text = OpenFileDialog1.FileName;
					if (PlayerOK)
					{
						DShowPlayer.newFilename = tbSourceLocation.Text;
					}
					ApplySoundControls(ApplyMute: false);
				}
			}
			catch
			{
				flag = true;
			}
			if (flag)
			{
				try
				{
					OpenFileDialog1.FileName = "";
					if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
					{
						ApplyPlayControls(ControlsBtn.Stopbtn);
						tbSourceLocation.Text = OpenFileDialog1.FileName;
						if (PlayerOK)
						{
							DShowPlayer.newFilename = tbSourceLocation.Text;
						}
						ApplySoundControls(ApplyMute: false);
					}
				}
				catch
				{
				}
			}
		}

		private void TimerFast_Tick(object sender, EventArgs e)
		{
			IncrementCurrentPosition(TimeIncrement);
		}

		private void IncrementCurrentPosition(double InIncrement)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition((double)DShowPlayer.GetCurrentPosition() + InIncrement);
				SetDurationSettings();
			}
		}

		private void SetDurationSettings()
		{
			if (PlayerOK && DShowPlayer.GetClipDuration() > 0)
			{
				SetDurationSettings(ResetAll: false);
			}
			else
			{
				SetDurationSettings(ResetAll: true);
			}
		}

		private void SetDurationSettings(bool ResetAll)
		{
			if (ResetAll)
			{
				if (LabelMediaType.Text != "" && ((LabelMediaType.Text[0] == 'A') | (LabelMediaType.Text[0] == 'V')))
				{
					LabelDuration.Text = "Streaming Contents";
				}
				else
				{
					LabelDuration.Text = "00:00";
				}
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 0;
				TrackBarDuration.Value = 0;
			}
			else if (PlayerOK)
			{
				LabelDuration.Text = ((DShowPlayer.newFilename != "") ? DShowPlayer.GetClipDurationString() : "00:00");
				LabelPosition.Text = DShowPlayer.GetCurrentPositionString();
				TrackBarDuration.Maximum = DShowPlayer.GetClipDuration();
				TrackBarDuration.Value = ((DShowPlayer.GetCurrentPosition() > TrackBarDuration.Maximum) ? TrackBarDuration.Maximum : DShowPlayer.GetCurrentPosition());
			}
			else
			{
				LabelDuration.Text = "00:00";
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 1000;
				TrackBarDuration.Value = 0;
			}
		}

		private void TimerTrack_Tick(object sender, EventArgs e)
		{
			SetDurationSettings();
		}

		private void TrackBarDuration_Scroll(object sender, EventArgs e)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition(TrackBarDuration.Value);
			}
		}

		private void tbSourceLocation_TextChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				SourceOption2.Checked = true;
			}
		}

		private void TimerAttemptConnect_Tick(object sender, EventArgs e)
		{
		}

		private void SourceOption3_CheckedChanged(object sender, EventArgs e)
		{
			if (SourceOption3.Checked)
			{
				RestartInputDevice();
			}
		}

		private void RestartInputDevice()
		{
			ApplyPlayControls(ControlsBtn.Stopbtn);
			ApplyPlayControls(ControlsBtn.PlayPausebtn);
		}

		private void SourceOption2_CheckedChanged(object sender, EventArgs e)
		{
			if (SourceOption2.Checked)
			{
				ApplyPlayControls(ControlsBtn.Stopbtn);
			}
		}

		private void cbCaptureDevicesAndTypes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!InitLoad && SourceOption3.Checked)
			{
				RestartInputDevice();
			}
		}
	}
}
