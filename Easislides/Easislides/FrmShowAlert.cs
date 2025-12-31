using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public class FrmShowAlert : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private IContainer components = null;

		private GroupBox groupBox1;

		private Label label1;

		private ComboBox cbMessageAlert;

		private Button MessageShow;

		private Button BtnStop1;

		private GroupBox groupBox2;

		private Label label3;

		private Button ParentalShow;

		private Label label2;

		private Timer TimerRestoreWindow;

		private TextBox ParentalPrefix;

		private ComboBox cbParentalAlert;

		private Panel panel2;

		private ToolStrip toolStripMessageBtns;

		private ToolStripButton Message_Scroll;

		private ToolStripButton Message_Flash;

		private Panel panel1;

		private ToolStrip toolStripParentalBtns;

		private ToolStripButton Parental_Scroll;

		private ToolStripButton Parental_Flash;

		private ToolStripButton Message_Transparent;

		private ToolStripButton Parental_Transparent;

		private Button BtnCancel;

		private Button BtnStop2;

		private Button btnClearHistoryMessage;

		private Button btnClearHistoryParental;

		private GroupBox groupBox3;

		private RichTextBox tbLyricsAlert;

		private Button btnClearLyrics;

		private Button LyricsShow;

		private string FormRegLeft = "ShowAlertLeft";

		private string FormRegTop = "ShowAlertTop";

		public event Message OnMessage;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmShowAlert));
            groupBox1 = new GroupBox();
            btnClearHistoryMessage = new Button();
            panel2 = new Panel();
            toolStripMessageBtns = new ToolStrip();
            Message_Scroll = new ToolStripButton();
            Message_Flash = new ToolStripButton();
            Message_Transparent = new ToolStripButton();
            cbMessageAlert = new ComboBox();
            label1 = new Label();
            MessageShow = new Button();
            BtnStop1 = new Button();
            groupBox2 = new GroupBox();
            btnClearHistoryParental = new Button();
            panel1 = new Panel();
            toolStripParentalBtns = new ToolStrip();
            Parental_Scroll = new ToolStripButton();
            Parental_Flash = new ToolStripButton();
            Parental_Transparent = new ToolStripButton();
            cbParentalAlert = new ComboBox();
            BtnStop2 = new Button();
            ParentalPrefix = new TextBox();
            label2 = new Label();
            label3 = new Label();
            ParentalShow = new Button();
            TimerRestoreWindow = new Timer(components);
            BtnCancel = new Button();
            groupBox3 = new GroupBox();
            LyricsShow = new Button();
            btnClearLyrics = new Button();
            tbLyricsAlert = new RichTextBox();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            toolStripMessageBtns.SuspendLayout();
            groupBox2.SuspendLayout();
            panel1.SuspendLayout();
            toolStripParentalBtns.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.Control;
            groupBox1.Controls.Add(btnClearHistoryMessage);
            groupBox1.Controls.Add(panel2);
            groupBox1.Controls.Add(cbMessageAlert);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(MessageShow);
            groupBox1.Controls.Add(BtnStop1);
            groupBox1.Location = new Point(11, 11);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(409, 142);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Message Alert";
            // 
            // btnClearHistoryMessage
            // 
            btnClearHistoryMessage.FlatStyle = FlatStyle.Flat;
            btnClearHistoryMessage.Location = new Point(161, 95);
            btnClearHistoryMessage.Margin = new Padding(4, 5, 4, 5);
            btnClearHistoryMessage.Name = "btnClearHistoryMessage";
            btnClearHistoryMessage.Size = new Size(116, 37);
            btnClearHistoryMessage.TabIndex = 9;
            btnClearHistoryMessage.Text = "Clear History";
            btnClearHistoryMessage.Click += btnClearHistoryMessage_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(toolStripMessageBtns);
            panel2.Location = new Point(15, 97);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new Size(101, 34);
            panel2.TabIndex = 8;
            // 
            // toolStripMessageBtns
            // 
            toolStripMessageBtns.AutoSize = false;
            toolStripMessageBtns.CanOverflow = false;
            toolStripMessageBtns.Dock = DockStyle.None;
            toolStripMessageBtns.GripStyle = ToolStripGripStyle.Hidden;
            toolStripMessageBtns.ImageScalingSize = new Size(20, 20);
            toolStripMessageBtns.Items.AddRange(new ToolStripItem[] { Message_Scroll, Message_Flash, Message_Transparent });
            toolStripMessageBtns.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripMessageBtns.Location = new Point(0, -2);
            toolStripMessageBtns.Name = "toolStripMessageBtns";
            toolStripMessageBtns.RenderMode = ToolStripRenderMode.System;
            toolStripMessageBtns.Size = new Size(101, 38);
            toolStripMessageBtns.TabIndex = 0;
            // 
            // Message_Scroll
            // 
            Message_Scroll.CheckOnClick = true;
            Message_Scroll.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Scroll.Image = Resources.Scroll;
            Message_Scroll.ImageTransparentColor = Color.Magenta;
            Message_Scroll.Name = "Message_Scroll";
            Message_Scroll.Size = new Size(29, 35);
            Message_Scroll.Tag = "";
            Message_Scroll.ToolTipText = "Scroll";
            Message_Scroll.Click += ScrollFlashOption_Click;
            // 
            // Message_Flash
            // 
            Message_Flash.CheckOnClick = true;
            Message_Flash.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Flash.Image = Resources.Flash;
            Message_Flash.ImageTransparentColor = Color.Magenta;
            Message_Flash.Name = "Message_Flash";
            Message_Flash.Size = new Size(29, 35);
            Message_Flash.Tag = "";
            Message_Flash.ToolTipText = "Flash";
            Message_Flash.Click += ScrollFlashOption_Click;
            // 
            // Message_Transparent
            // 
            Message_Transparent.CheckOnClick = true;
            Message_Transparent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Transparent.Image = Resources.Transparent;
            Message_Transparent.ImageTransparentColor = Color.Magenta;
            Message_Transparent.Name = "Message_Transparent";
            Message_Transparent.Size = new Size(29, 35);
            Message_Transparent.ToolTipText = "Transparent";
            Message_Transparent.Click += ScrollFlashOption_Click;
            // 
            // cbMessageAlert
            // 
            cbMessageAlert.FormattingEnabled = true;
            cbMessageAlert.Location = new Point(13, 55);
            cbMessageAlert.Margin = new Padding(4, 5, 4, 5);
            cbMessageAlert.Name = "cbMessageAlert";
            cbMessageAlert.Size = new Size(263, 28);
            cbMessageAlert.TabIndex = 0;
            cbMessageAlert.Enter += cbMessageAlert_Enter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 31);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(44, 20);
            label1.TabIndex = 1;
            label1.Text = "Alert:";
            // 
            // MessageShow
            // 
            MessageShow.DialogResult = DialogResult.Cancel;
            MessageShow.Location = new Point(285, 54);
            MessageShow.Margin = new Padding(4, 5, 4, 5);
            MessageShow.Name = "MessageShow";
            MessageShow.Size = new Size(108, 37);
            MessageShow.TabIndex = 3;
            MessageShow.Text = "Go Live";
            MessageShow.Click += MessageShow_Click;
            // 
            // BtnStop1
            // 
            BtnStop1.Location = new Point(285, 97);
            BtnStop1.Margin = new Padding(4, 5, 4, 5);
            BtnStop1.Name = "BtnStop1";
            BtnStop1.Size = new Size(108, 37);
            BtnStop1.TabIndex = 2;
            BtnStop1.Text = "Stop Alert";
            BtnStop1.Click += BtnStop_Click;
            // 
            // groupBox2
            // 
            groupBox2.BackColor = SystemColors.Control;
            groupBox2.Controls.Add(btnClearHistoryParental);
            groupBox2.Controls.Add(panel1);
            groupBox2.Controls.Add(cbParentalAlert);
            groupBox2.Controls.Add(BtnStop2);
            groupBox2.Controls.Add(ParentalPrefix);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(ParentalShow);
            groupBox2.Location = new Point(11, 162);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(409, 142);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Parental Alert";
            // 
            // btnClearHistoryParental
            // 
            btnClearHistoryParental.FlatStyle = FlatStyle.Flat;
            btnClearHistoryParental.Location = new Point(161, 95);
            btnClearHistoryParental.Margin = new Padding(4, 5, 4, 5);
            btnClearHistoryParental.Name = "btnClearHistoryParental";
            btnClearHistoryParental.Size = new Size(116, 37);
            btnClearHistoryParental.TabIndex = 14;
            btnClearHistoryParental.Text = "Clear History";
            btnClearHistoryParental.Click += btnClearHistoryParental_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(toolStripParentalBtns);
            panel1.Location = new Point(13, 100);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(103, 34);
            panel1.TabIndex = 13;
            // 
            // toolStripParentalBtns
            // 
            toolStripParentalBtns.AutoSize = false;
            toolStripParentalBtns.CanOverflow = false;
            toolStripParentalBtns.Dock = DockStyle.None;
            toolStripParentalBtns.GripStyle = ToolStripGripStyle.Hidden;
            toolStripParentalBtns.ImageScalingSize = new Size(20, 20);
            toolStripParentalBtns.Items.AddRange(new ToolStripItem[] { Parental_Scroll, Parental_Flash, Parental_Transparent });
            toolStripParentalBtns.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripParentalBtns.Location = new Point(0, -2);
            toolStripParentalBtns.Name = "toolStripParentalBtns";
            toolStripParentalBtns.RenderMode = ToolStripRenderMode.System;
            toolStripParentalBtns.Size = new Size(103, 38);
            toolStripParentalBtns.TabIndex = 1;
            // 
            // Parental_Scroll
            // 
            Parental_Scroll.CheckOnClick = true;
            Parental_Scroll.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Scroll.Image = Resources.Scroll;
            Parental_Scroll.ImageTransparentColor = Color.Magenta;
            Parental_Scroll.Name = "Parental_Scroll";
            Parental_Scroll.Size = new Size(29, 35);
            Parental_Scroll.Tag = "";
            Parental_Scroll.ToolTipText = "Scroll";
            Parental_Scroll.Click += ScrollFlashOption_Click;
            // 
            // Parental_Flash
            // 
            Parental_Flash.CheckOnClick = true;
            Parental_Flash.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Flash.Image = Resources.Flash;
            Parental_Flash.ImageTransparentColor = Color.Magenta;
            Parental_Flash.Name = "Parental_Flash";
            Parental_Flash.Size = new Size(29, 35);
            Parental_Flash.Tag = "";
            Parental_Flash.ToolTipText = "Flash";
            Parental_Flash.Click += ScrollFlashOption_Click;
            // 
            // Parental_Transparent
            // 
            Parental_Transparent.CheckOnClick = true;
            Parental_Transparent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Transparent.Image = Resources.Transparent;
            Parental_Transparent.ImageTransparentColor = Color.Magenta;
            Parental_Transparent.Name = "Parental_Transparent";
            Parental_Transparent.Size = new Size(29, 35);
            Parental_Transparent.ToolTipText = "Transparent";
            Parental_Transparent.Click += ScrollFlashOption_Click;
            // 
            // cbParentalAlert
            // 
            cbParentalAlert.FormattingEnabled = true;
            cbParentalAlert.Location = new Point(155, 55);
            cbParentalAlert.Margin = new Padding(4, 5, 4, 5);
            cbParentalAlert.Name = "cbParentalAlert";
            cbParentalAlert.Size = new Size(121, 28);
            cbParentalAlert.TabIndex = 0;
            cbParentalAlert.Enter += cbParentalAlert_Enter;
            // 
            // BtnStop2
            // 
            BtnStop2.Location = new Point(285, 98);
            BtnStop2.Margin = new Padding(4, 5, 4, 5);
            BtnStop2.Name = "BtnStop2";
            BtnStop2.Size = new Size(108, 37);
            BtnStop2.TabIndex = 5;
            BtnStop2.Text = "Stop Alert";
            BtnStop2.Click += BtnStop_Click;
            // 
            // ParentalPrefix
            // 
            ParentalPrefix.BackColor = SystemColors.Control;
            ParentalPrefix.Location = new Point(13, 57);
            ParentalPrefix.Margin = new Padding(4, 5, 4, 5);
            ParentalPrefix.Name = "ParentalPrefix";
            ParentalPrefix.ReadOnly = true;
            ParentalPrefix.Size = new Size(132, 27);
            ParentalPrefix.TabIndex = 1;
            ParentalPrefix.TextAlign = HorizontalAlignment.Right;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(215, 31);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(44, 20);
            label2.TabIndex = 1;
            label2.Text = "Alert:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(16, 31);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(49, 20);
            label3.TabIndex = 12;
            label3.Text = "Prefix:";
            // 
            // ParentalShow
            // 
            ParentalShow.DialogResult = DialogResult.Cancel;
            ParentalShow.Location = new Point(285, 54);
            ParentalShow.Margin = new Padding(4, 5, 4, 5);
            ParentalShow.Name = "ParentalShow";
            ParentalShow.Size = new Size(108, 37);
            ParentalShow.TabIndex = 4;
            ParentalShow.Text = "Go Live";
            ParentalShow.Click += ParentalShow_Click;
            // 
            // TimerRestoreWindow
            // 
            TimerRestoreWindow.Interval = 1000;
            TimerRestoreWindow.Tick += TimerRestoreWindow_Tick;
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(172, 443);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 3;
            BtnCancel.Text = "Close";
            BtnCancel.Click += BtnCancel_Click;
            // 
            // groupBox3
            // 
            groupBox3.BackColor = SystemColors.Control;
            groupBox3.Controls.Add(LyricsShow);
            groupBox3.Controls.Add(btnClearLyrics);
            groupBox3.Controls.Add(tbLyricsAlert);
            groupBox3.Location = new Point(11, 312);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(409, 118);
            groupBox3.TabIndex = 6;
            groupBox3.TabStop = false;
            groupBox3.Text = "Lyrics Monitor Alert";
            // 
            // LyricsShow
            // 
            LyricsShow.DialogResult = DialogResult.Cancel;
            LyricsShow.Image = Resources.Tick;
            LyricsShow.Location = new Point(285, 29);
            LyricsShow.Margin = new Padding(4, 5, 4, 5);
            LyricsShow.Name = "LyricsShow";
            LyricsShow.Size = new Size(108, 37);
            LyricsShow.TabIndex = 7;
            LyricsShow.Text = "Send";
            LyricsShow.TextAlign = ContentAlignment.MiddleRight;
            LyricsShow.TextImageRelation = TextImageRelation.ImageBeforeText;
            LyricsShow.Click += LyricsShow_Click;
            // 
            // btnClearLyrics
            // 
            btnClearLyrics.Image = Resources.Delete;
            btnClearLyrics.Location = new Point(285, 72);
            btnClearLyrics.Margin = new Padding(4, 5, 4, 5);
            btnClearLyrics.Name = "btnClearLyrics";
            btnClearLyrics.Size = new Size(108, 37);
            btnClearLyrics.TabIndex = 8;
            btnClearLyrics.Text = "Clear";
            btnClearLyrics.TextAlign = ContentAlignment.MiddleRight;
            btnClearLyrics.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClearLyrics.Click += btnClearLyrics_Click;
            // 
            // tbLyricsAlert
            // 
            tbLyricsAlert.Location = new Point(13, 29);
            tbLyricsAlert.Margin = new Padding(4, 5, 4, 5);
            tbLyricsAlert.Name = "tbLyricsAlert";
            tbLyricsAlert.ScrollBars = RichTextBoxScrollBars.Vertical;
            tbLyricsAlert.Size = new Size(263, 75);
            tbLyricsAlert.TabIndex = 0;
            tbLyricsAlert.Text = "";
            tbLyricsAlert.Enter += tbLyricsAlert_Enter;
            // 
            // FrmShowAlert
            // 
            AcceptButton = BtnCancel;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(436, 502);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(BtnCancel);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "FrmShowAlert";
            Text = "Show Alert";
            FormClosing += FrmShowAlert_FormClosing;
            Load += FrmShowAlert_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel2.ResumeLayout(false);
            toolStripMessageBtns.ResumeLayout(false);
            toolStripMessageBtns.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel1.ResumeLayout(false);
            toolStripParentalBtns.ResumeLayout(false);
            toolStripParentalBtns.PerformLayout();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        public FrmShowAlert()
		{
			InitializeComponent();
		}

		private void FrmShowAlert_Load(object sender, EventArgs e)
		{
			gf.AlertFormOpen = true;
			int num = RegUtil.GetRegValue("settings", FormRegLeft, 50);
			int num2 = RegUtil.GetRegValue("settings", FormRegTop, 100);
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
			LoadAlertList();
			cbParentalAlert.Text = gf.ParentalAlertDetails;
			Parental_Flash.Checked = gf.ParentalAlertFlash;
			Parental_Scroll.Checked = gf.ParentalAlertScroll;
			Parental_Transparent.Checked = gf.ParentalAlertTransparent;
			ParentalPrefix.Text = gf.ParentalAlertHeading + " ";
			cbMessageAlert.Text = gf.MessageAlertDetails;
			Message_Flash.Checked = gf.MessageAlertFlash;
			Message_Scroll.Checked = gf.MessageAlertScroll;
			Message_Transparent.Checked = gf.MessageAlertTransparent;
			tbLyricsAlert.Text = gf.LyricsAlertDetails;
			TimerRestoreWindow.Start();
		}

		private void BtnStop_Click(object sender, EventArgs e)
		{
			gf.MessageAlertLive = false;
			gf.ParentalAlertLive = false;
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void LoadAlertList()
		{
			gf.LoadComboBoxFromTextFile(ref cbMessageAlert, gf.AlertsDataFile);
			gf.LoadComboBoxFromTextFile(ref cbParentalAlert, gf.ParentalDataFile);
		}

		private void ParentalShow_Click(object sender, EventArgs e)
		{
			gf.ParentalAlertDetails = DataUtil.Trim(cbParentalAlert.Text);
			if (!(gf.ParentalAlertDetails == ""))
			{
				if (cbParentalAlert.Items.Count == 0 || cbParentalAlert.Text != cbParentalAlert.Items[0].ToString())
				{
					try
					{
						cbParentalAlert.Items.Insert(0, cbParentalAlert.Text);
						if (cbParentalAlert.Items.Count > 20)
						{
							for (int num = cbParentalAlert.Items.Count; num >= 21; num--)
							{
								cbParentalAlert.Items.RemoveAt(num);
							}
						}
						gf.SaveComboBoxToTextFile(ref cbParentalAlert, gf.ParentalDataFile);
					}
					catch
					{
					}
				}
				this.OnMessage(1, "");
			}
		}

		private void MessageShow_Click(object sender, EventArgs e)
		{
			gf.MessageAlertDetails = DataUtil.Trim(cbMessageAlert.Text);
			if (!(gf.MessageAlertDetails == ""))
			{
				cbMessageAlert.Text = DataUtil.Trim(cbMessageAlert.Text);
				if (cbMessageAlert.Items.Count == 0 || cbMessageAlert.Text != cbMessageAlert.Items[0].ToString())
				{
					try
					{
						cbMessageAlert.Items.Insert(0, cbMessageAlert.Text);
						if (cbMessageAlert.Items.Count > 20)
						{
							for (int num = cbMessageAlert.Items.Count; num >= 21; num--)
							{
								cbMessageAlert.Items.RemoveAt(num);
							}
						}
						gf.SaveComboBoxToTextFile(ref cbMessageAlert, gf.AlertsDataFile);
					}
					catch
					{
					}
				}
				this.OnMessage(0, "");
			}
		}

		private void FrmShowAlert_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveFormLocation();
			gf.SaveOptionsData();
			TimerRestoreWindow.Stop();
			gf.AlertFormOpen = false;
		}

		private void SaveFormLocation()
		{
			RegUtil.SaveRegValue("settings", FormRegLeft, base.Left);
			RegUtil.SaveRegValue("settings", FormRegTop, base.Top);
		}

		private void TimerRestoreWindow_Tick(object sender, EventArgs e)
		{
			if (gf.AlertRestoreWindow)
			{
				gf.AlertRestoreWindow = false;
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

		private void ScrollFlashOption_Click(object sender, EventArgs e)
		{
			gf.ParentalAlertScroll = Parental_Scroll.Checked;
			gf.ParentalAlertFlash = Parental_Flash.Checked;
			gf.ParentalAlertTransparent = Parental_Transparent.Checked;
			gf.MessageAlertScroll = Message_Scroll.Checked;
			gf.MessageAlertFlash = Message_Flash.Checked;
			gf.MessageAlertTransparent = Message_Transparent.Checked;
		}

		private void btnClearHistoryMessage_Click(object sender, EventArgs e)
		{
			cbMessageAlert.Items.Clear();
			cbMessageAlert.Text = "";
			gf.MessageAlertDetails = "";
			gf.SaveComboBoxToTextFile(ref cbMessageAlert, gf.AlertsDataFile);
		}

		private void btnClearHistoryParental_Click(object sender, EventArgs e)
		{
			cbParentalAlert.Items.Clear();
			cbParentalAlert.Text = "";
			gf.ParentalAlertDetails = "";
			gf.SaveComboBoxToTextFile(ref cbParentalAlert, gf.ParentalDataFile);
		}

		private void cbParentalAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = ParentalShow;
		}

		private void cbMessageAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = MessageShow;
		}

		private void tbLyricsAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = LyricsShow;
		}

		private void LyricsShow_Click(object sender, EventArgs e)
		{
			gf.LyricsAlertDetails = DataUtil.Trim(tbLyricsAlert.Text);
			if (!(gf.LyricsAlertDetails == ""))
			{
				tbLyricsAlert.Text = DataUtil.Trim(tbLyricsAlert.Text);
				this.OnMessage(2, "");
			}
		}

		private void btnClearLyrics_Click(object sender, EventArgs e)
		{
			tbLyricsAlert.Text = "";
			gf.LyricsAlertDetails = "";
			this.OnMessage(2, "");
		}
	}
}
