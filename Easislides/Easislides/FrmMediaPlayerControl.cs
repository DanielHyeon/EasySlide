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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMediaPlayerControl));
			groupBox2 = new System.Windows.Forms.GroupBox();
			LabelResolution = new System.Windows.Forms.Label();
			label15 = new System.Windows.Forms.Label();
			cbWidescreen = new System.Windows.Forms.CheckBox();
			LabelMediaType = new System.Windows.Forms.Label();
			label10 = new System.Windows.Forms.Label();
			panel1 = new System.Windows.Forms.Panel();
			panelNoPlayer = new System.Windows.Forms.Panel();
			label9 = new System.Windows.Forms.Label();
			labelNoPlayer2 = new System.Windows.Forms.Label();
			labelNoPlayer1 = new System.Windows.Forms.Label();
			cbRepeat = new System.Windows.Forms.CheckBox();
			LabelPosition = new System.Windows.Forms.Label();
			LabelDuration = new System.Windows.Forms.Label();
			label8 = new System.Windows.Forms.Label();
			label7 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			label6 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			TrackBarBalance = new System.Windows.Forms.TrackBar();
			TrackBarVolume = new System.Windows.Forms.TrackBar();
			cbMute = new System.Windows.Forms.CheckBox();
			panelPlayBtns = new System.Windows.Forms.Panel();
			TrackBarDuration = new System.Windows.Forms.TrackBar();
			StopBtn = new System.Windows.Forms.Button();
			PlayPauseBtn = new System.Windows.Forms.Button();
			FastReverseBtn = new System.Windows.Forms.Button();
			FastForwardBtn = new System.Windows.Forms.Button();
			label1 = new System.Windows.Forms.Label();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			TimerFast = new System.Windows.Forms.Timer(components);
			TimerTrack = new System.Windows.Forms.Timer(components);
			groupBox1 = new System.Windows.Forms.GroupBox();
			panel47 = new System.Windows.Forms.Panel();
			toolStripCaptureDevices = new System.Windows.Forms.ToolStrip();
			cbCaptureDevices = new System.Windows.Forms.ToolStripComboBox();
			label14 = new System.Windows.Forms.Label();
			label13 = new System.Windows.Forms.Label();
			label12 = new System.Windows.Forms.Label();
			label11 = new System.Windows.Forms.Label();
			SourceOption1 = new System.Windows.Forms.RadioButton();
			SourceOption2 = new System.Windows.Forms.RadioButton();
			SourceOption0 = new System.Windows.Forms.RadioButton();
			panelLinkTitle2Lookup = new System.Windows.Forms.Panel();
			toolStripLocate = new System.Windows.Forms.ToolStrip();
			LocationBtn = new System.Windows.Forms.ToolStripButton();
			tbSourceLocation = new System.Windows.Forms.TextBox();
			SourceOption3 = new System.Windows.Forms.RadioButton();
			toolTip1 = new System.Windows.Forms.ToolTip(components);
			TimerAttemptConnect = new System.Windows.Forms.Timer(components);
			groupBox2.SuspendLayout();
			panel1.SuspendLayout();
			panelNoPlayer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)TrackBarBalance).BeginInit();
			((System.ComponentModel.ISupportInitialize)TrackBarVolume).BeginInit();
			panelPlayBtns.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)TrackBarDuration).BeginInit();
			groupBox1.SuspendLayout();
			panel47.SuspendLayout();
			toolStripCaptureDevices.SuspendLayout();
			panelLinkTitle2Lookup.SuspendLayout();
			toolStripLocate.SuspendLayout();
			SuspendLayout();
			groupBox2.Controls.Add(LabelResolution);
			groupBox2.Controls.Add(label15);
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
			groupBox2.Location = new System.Drawing.Point(9, 111);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new System.Drawing.Size(483, 236);
			groupBox2.TabIndex = 1;
			groupBox2.TabStop = false;
			groupBox2.Text = "Settings";
			LabelResolution.AutoSize = true;
			LabelResolution.Location = new System.Drawing.Point(71, 181);
			LabelResolution.Name = "LabelResolution";
			LabelResolution.Size = new System.Drawing.Size(27, 13);
			LabelResolution.TabIndex = 26;
			LabelResolution.Text = "N/A";
			label15.AutoSize = true;
			label15.Location = new System.Drawing.Point(13, 181);
			label15.Name = "label15";
			label15.Size = new System.Drawing.Size(60, 13);
			label15.TabIndex = 25;
			label15.Text = "Resolution:";
			cbWidescreen.AutoSize = true;
			cbWidescreen.Location = new System.Drawing.Point(86, 208);
			cbWidescreen.Name = "cbWidescreen";
			cbWidescreen.Size = new System.Drawing.Size(115, 17);
			cbWidescreen.TabIndex = 24;
			cbWidescreen.Text = "Force WideScreen";
			cbWidescreen.CheckedChanged += new System.EventHandler(cbWidescreen_CheckedChanged);
			LabelMediaType.AutoSize = true;
			LabelMediaType.ForeColor = System.Drawing.Color.Red;
			LabelMediaType.Location = new System.Drawing.Point(71, 162);
			LabelMediaType.Name = "LabelMediaType";
			LabelMediaType.Size = new System.Drawing.Size(33, 13);
			LabelMediaType.TabIndex = 7;
			LabelMediaType.Text = "None";
			label10.AutoSize = true;
			label10.Location = new System.Drawing.Point(13, 162);
			label10.Name = "label10";
			label10.Size = new System.Drawing.Size(39, 13);
			label10.TabIndex = 21;
			label10.Text = "Media:";
			panel1.BackColor = System.Drawing.Color.Black;
			panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panel1.Controls.Add(panelNoPlayer);
			panel1.Location = new System.Drawing.Point(239, 16);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(228, 175);
			panel1.TabIndex = 0;
			panelNoPlayer.BackColor = System.Drawing.Color.MidnightBlue;
			panelNoPlayer.Controls.Add(label9);
			panelNoPlayer.Controls.Add(labelNoPlayer2);
			panelNoPlayer.Controls.Add(labelNoPlayer1);
			panelNoPlayer.ForeColor = System.Drawing.Color.White;
			panelNoPlayer.Location = new System.Drawing.Point(0, 0);
			panelNoPlayer.Name = "panelNoPlayer";
			panelNoPlayer.Size = new System.Drawing.Size(224, 171);
			panelNoPlayer.TabIndex = 60;
			label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			label9.Location = new System.Drawing.Point(14, 83);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(192, 43);
			label9.TabIndex = 25;
			label9.Text = "to view / listen to Media Backgrounds.";
			label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			labelNoPlayer2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			labelNoPlayer2.Location = new System.Drawing.Point(6, 47);
			labelNoPlayer2.Name = "labelNoPlayer2";
			labelNoPlayer2.Size = new System.Drawing.Size(215, 40);
			labelNoPlayer2.TabIndex = 24;
			labelNoPlayer2.Text = "Windows Media Player 10 or DirectX 9";
			labelNoPlayer2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			labelNoPlayer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			labelNoPlayer1.Location = new System.Drawing.Point(14, 28);
			labelNoPlayer1.Name = "labelNoPlayer1";
			labelNoPlayer1.Size = new System.Drawing.Size(192, 20);
			labelNoPlayer1.TabIndex = 0;
			labelNoPlayer1.Text = "Please install";
			labelNoPlayer1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			cbRepeat.AutoSize = true;
			cbRepeat.Location = new System.Drawing.Point(14, 208);
			cbRepeat.Name = "cbRepeat";
			cbRepeat.Size = new System.Drawing.Size(61, 17);
			cbRepeat.TabIndex = 8;
			cbRepeat.Text = "Repeat";
			cbRepeat.CheckedChanged += new System.EventHandler(cbRepeat_CheckedChanged);
			LabelPosition.AutoSize = true;
			LabelPosition.Location = new System.Drawing.Point(71, 143);
			LabelPosition.Name = "LabelPosition";
			LabelPosition.Size = new System.Drawing.Size(28, 13);
			LabelPosition.TabIndex = 6;
			LabelPosition.Text = "0:00";
			LabelDuration.AutoSize = true;
			LabelDuration.Location = new System.Drawing.Point(71, 125);
			LabelDuration.Name = "LabelDuration";
			LabelDuration.Size = new System.Drawing.Size(28, 13);
			LabelDuration.TabIndex = 5;
			LabelDuration.Text = "0:00";
			label8.AutoSize = true;
			label8.Location = new System.Drawing.Point(13, 143);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(47, 13);
			label8.TabIndex = 13;
			label8.Text = "Position:";
			label7.AutoSize = true;
			label7.Location = new System.Drawing.Point(13, 125);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(50, 13);
			label7.TabIndex = 12;
			label7.Text = "Duration:";
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(13, 86);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(49, 13);
			label4.TabIndex = 3;
			label4.Text = "Balance:";
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(206, 87);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(15, 13);
			label5.TabIndex = 9;
			label5.Text = "R";
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(83, 87);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(13, 13);
			label6.TabIndex = 8;
			label6.Text = "L";
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(14, 50);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(45, 13);
			label3.TabIndex = 1;
			label3.Text = "Volume:";
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(206, 50);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(27, 13);
			label2.TabIndex = 6;
			label2.Text = "Max";
			TrackBarBalance.AutoSize = false;
			TrackBarBalance.Location = new System.Drawing.Point(86, 83);
			TrackBarBalance.Maximum = 100;
			TrackBarBalance.Minimum = -100;
			TrackBarBalance.Name = "TrackBarBalance";
			TrackBarBalance.Size = new System.Drawing.Size(125, 37);
			TrackBarBalance.TabIndex = 4;
			TrackBarBalance.TickFrequency = 20;
			TrackBarBalance.ValueChanged += new System.EventHandler(TrackBarBalance_ValueChanged);
			TrackBarVolume.AutoSize = false;
			TrackBarVolume.Location = new System.Drawing.Point(86, 38);
			TrackBarVolume.Maximum = 100;
			TrackBarVolume.Name = "TrackBarVolume";
			TrackBarVolume.Size = new System.Drawing.Size(125, 35);
			TrackBarVolume.TabIndex = 2;
			TrackBarVolume.TickFrequency = 10;
			TrackBarVolume.TickStyle = System.Windows.Forms.TickStyle.Both;
			TrackBarVolume.ValueChanged += new System.EventHandler(TrackBarVolume_ValueChanged);
			cbMute.AutoSize = true;
			cbMute.Location = new System.Drawing.Point(15, 21);
			cbMute.Name = "cbMute";
			cbMute.Size = new System.Drawing.Size(50, 17);
			cbMute.TabIndex = 0;
			cbMute.Text = "Mute";
			cbMute.CheckedChanged += new System.EventHandler(cbMute_CheckedChanged);
			panelPlayBtns.Controls.Add(TrackBarDuration);
			panelPlayBtns.Controls.Add(StopBtn);
			panelPlayBtns.Controls.Add(PlayPauseBtn);
			panelPlayBtns.Controls.Add(FastReverseBtn);
			panelPlayBtns.Controls.Add(FastForwardBtn);
			panelPlayBtns.Location = new System.Drawing.Point(231, 190);
			panelPlayBtns.Name = "panelPlayBtns";
			panelPlayBtns.Size = new System.Drawing.Size(246, 44);
			panelPlayBtns.TabIndex = 23;
			TrackBarDuration.AutoSize = false;
			TrackBarDuration.Location = new System.Drawing.Point(0, 0);
			TrackBarDuration.Maximum = 1000;
			TrackBarDuration.Name = "TrackBarDuration";
			TrackBarDuration.Size = new System.Drawing.Size(242, 18);
			TrackBarDuration.TabIndex = 0;
			TrackBarDuration.TickFrequency = 0;
			TrackBarDuration.TickStyle = System.Windows.Forms.TickStyle.None;
			TrackBarDuration.Scroll += new System.EventHandler(TrackBarDuration_Scroll);
			StopBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			StopBtn.Location = new System.Drawing.Point(162, 18);
			StopBtn.Name = "StopBtn";
			StopBtn.Size = new System.Drawing.Size(42, 22);
			StopBtn.TabIndex = 4;
			StopBtn.Text = "Stop";
			StopBtn.Click += new System.EventHandler(StopBtn_Click);
			PlayPauseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			PlayPauseBtn.Location = new System.Drawing.Point(111, 18);
			PlayPauseBtn.Name = "PlayPauseBtn";
			PlayPauseBtn.Size = new System.Drawing.Size(48, 22);
			PlayPauseBtn.TabIndex = 3;
			PlayPauseBtn.Text = "Play";
			PlayPauseBtn.Click += new System.EventHandler(PlayPauseBtn_Click);
			FastReverseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			FastReverseBtn.Location = new System.Drawing.Point(39, 18);
			FastReverseBtn.Name = "FastReverseBtn";
			FastReverseBtn.Size = new System.Drawing.Size(32, 22);
			FastReverseBtn.TabIndex = 1;
			FastReverseBtn.Text = "<<";
			FastReverseBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(FastReverseBtn_MouseDown);
			FastReverseBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(FastReverseBtn_MouseUp);
			FastForwardBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			FastForwardBtn.Location = new System.Drawing.Point(75, 18);
			FastForwardBtn.Name = "FastForwardBtn";
			FastForwardBtn.Size = new System.Drawing.Size(32, 22);
			FastForwardBtn.TabIndex = 2;
			FastForwardBtn.Text = ">>";
			FastForwardBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(FastForwardBtn_MouseDown);
			FastForwardBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(FastForwardBtn_MouseUp);
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(65, 50);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(24, 13);
			label1.TabIndex = 5;
			label1.Text = "Min";
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(256, 356);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 3;
			BtnCancel.Text = "Cancel";
			BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			BtnOK.Location = new System.Drawing.Point(160, 356);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 2;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			OpenFileDialog1.FileName = "openFileDialog1";
			TimerFast.Interval = 500;
			TimerFast.Tick += new System.EventHandler(TimerFast_Tick);
			TimerTrack.Interval = 1000;
			TimerTrack.Tick += new System.EventHandler(TimerTrack_Tick);
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
			groupBox1.Location = new System.Drawing.Point(9, 5);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(483, 103);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Source Option";
			panel47.Controls.Add(toolStripCaptureDevices);
			panel47.Location = new System.Drawing.Point(128, 76);
			panel47.Name = "panel47";
			panel47.Size = new System.Drawing.Size(231, 22);
			panel47.TabIndex = 71;
			toolStripCaptureDevices.AutoSize = false;
			toolStripCaptureDevices.CanOverflow = false;
			toolStripCaptureDevices.Dock = System.Windows.Forms.DockStyle.None;
			toolStripCaptureDevices.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripCaptureDevices.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				cbCaptureDevices
			});
			toolStripCaptureDevices.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			toolStripCaptureDevices.Location = new System.Drawing.Point(1, -1);
			toolStripCaptureDevices.Name = "toolStripCaptureDevices";
			toolStripCaptureDevices.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripCaptureDevices.Size = new System.Drawing.Size(242, 25);
			toolStripCaptureDevices.TabIndex = 5;
			cbCaptureDevices.AutoSize = false;
			cbCaptureDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			cbCaptureDevices.Name = "cbCaptureDevices";
			cbCaptureDevices.Size = new System.Drawing.Size(220, 21);
			cbCaptureDevices.SelectedIndexChanged += new System.EventHandler(cbCaptureDevicesAndTypes_SelectedIndexChanged);
			label14.AutoSize = true;
			label14.Location = new System.Drawing.Point(6, 80);
			label14.Name = "label14";
			label14.Size = new System.Drawing.Size(16, 13);
			label14.TabIndex = 66;
			label14.Text = "4:";
			label13.AutoSize = true;
			label13.Location = new System.Drawing.Point(6, 59);
			label13.Name = "label13";
			label13.Size = new System.Drawing.Size(16, 13);
			label13.TabIndex = 65;
			label13.Text = "3:";
			label12.AutoSize = true;
			label12.Location = new System.Drawing.Point(6, 38);
			label12.Name = "label12";
			label12.Size = new System.Drawing.Size(16, 13);
			label12.TabIndex = 64;
			label12.Text = "2:";
			label11.AutoSize = true;
			label11.Location = new System.Drawing.Point(6, 18);
			label11.Name = "label11";
			label11.Size = new System.Drawing.Size(16, 13);
			label11.TabIndex = 63;
			label11.Text = "1:";
			SourceOption1.AutoSize = true;
			SourceOption1.Location = new System.Drawing.Point(28, 36);
			SourceOption1.MaximumSize = new System.Drawing.Size(463, 17);
			SourceOption1.Name = "SourceOption1";
			SourceOption1.Size = new System.Drawing.Size(28, 17);
			SourceOption1.TabIndex = 1;
			SourceOption1.Text = " ";
			SourceOption1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			SourceOption2.AutoSize = true;
			SourceOption2.Location = new System.Drawing.Point(28, 59);
			SourceOption2.Name = "SourceOption2";
			SourceOption2.Size = new System.Drawing.Size(14, 13);
			SourceOption2.TabIndex = 2;
			SourceOption2.CheckedChanged += new System.EventHandler(SourceOption2_CheckedChanged);
			SourceOption0.AutoSize = true;
			SourceOption0.Location = new System.Drawing.Point(28, 16);
			SourceOption0.Name = "SourceOption0";
			SourceOption0.Size = new System.Drawing.Size(71, 17);
			SourceOption0.TabIndex = 0;
			SourceOption0.Text = "No Media";
			panelLinkTitle2Lookup.Controls.Add(toolStripLocate);
			panelLinkTitle2Lookup.Location = new System.Drawing.Point(454, 54);
			panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
			panelLinkTitle2Lookup.Size = new System.Drawing.Size(23, 23);
			panelLinkTitle2Lookup.TabIndex = 3;
			toolStripLocate.AutoSize = false;
			toolStripLocate.CanOverflow = false;
			toolStripLocate.Dock = System.Windows.Forms.DockStyle.None;
			toolStripLocate.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripLocate.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				LocationBtn
			});
			toolStripLocate.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			toolStripLocate.Location = new System.Drawing.Point(0, -1);
			toolStripLocate.Name = "toolStripLocate";
			toolStripLocate.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripLocate.Size = new System.Drawing.Size(25, 28);
			toolStripLocate.TabIndex = 5;
			LocationBtn.AutoSize = false;
			LocationBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			LocationBtn.Image = Resources.folder;
			LocationBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
			LocationBtn.Name = "LocationBtn";
			LocationBtn.Size = new System.Drawing.Size(22, 22);
			LocationBtn.Tag = "";
			LocationBtn.ToolTipText = "Media File Location";
			LocationBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(LocationBtn_MouseUp);
			tbSourceLocation.Location = new System.Drawing.Point(45, 55);
			tbSourceLocation.Name = "tbSourceLocation";
			tbSourceLocation.Size = new System.Drawing.Size(404, 20);
			tbSourceLocation.TabIndex = 62;
			tbSourceLocation.TextChanged += new System.EventHandler(tbSourceLocation_TextChanged);
			SourceOption3.AutoSize = true;
			SourceOption3.Location = new System.Drawing.Point(28, 78);
			SourceOption3.Name = "SourceOption3";
			SourceOption3.Size = new System.Drawing.Size(98, 17);
			SourceOption3.TabIndex = 4;
			SourceOption3.Text = "Live Feed from:";
			SourceOption3.CheckedChanged += new System.EventHandler(SourceOption3_CheckedChanged);
			TimerAttemptConnect.Interval = 500;
			TimerAttemptConnect.Tick += new System.EventHandler(TimerAttemptConnect_Tick);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(501, 391);
			base.Controls.Add(groupBox1);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(groupBox2);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmMediaPlayerControl";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Assign Media";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmMediaPlayerControl_FormClosing);
			base.Load += new System.EventHandler(FrmMediaPlayerControl_Load);
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			panel1.ResumeLayout(false);
			panelNoPlayer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)TrackBarBalance).EndInit();
			((System.ComponentModel.ISupportInitialize)TrackBarVolume).EndInit();
			panelPlayBtns.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)TrackBarDuration).EndInit();
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
