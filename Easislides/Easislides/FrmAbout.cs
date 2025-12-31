using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Util;
using Microsoft.Win32;

namespace Easislides
{
	public class FrmAbout : Form
	{
		private IContainer components = null;

		private Button BtnSysInfo;

		private Button BtnOK;

		private RichTextBox lbleula;

		private Panel panel1;

		private Label label2;

		private LinkLabel linkLabel1;

		private TextBox lblRegDetails;

		private Label label3;

		private Label lblVersion;

		private Label lblCopyright;

		private Label label4;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmAbout));
            BtnSysInfo = new Button();
            BtnOK = new Button();
            lbleula = new RichTextBox();
            label2 = new Label();
            linkLabel1 = new LinkLabel();
            lblRegDetails = new TextBox();
            label3 = new Label();
            lblVersion = new Label();
            lblCopyright = new Label();
            panel1 = new Panel();
            label4 = new Label();
            SuspendLayout();
            // 
            // BtnSysInfo
            // 
            BtnSysInfo.Location = new Point(301, 451);
            BtnSysInfo.Margin = new Padding(4, 5, 4, 5);
            BtnSysInfo.Name = "BtnSysInfo";
            BtnSysInfo.Size = new Size(107, 37);
            BtnSysInfo.TabIndex = 1;
            BtnSysInfo.Text = "System Info";
            BtnSysInfo.Click += BtnSysInfo_Click;
            // 
            // BtnOK
            // 
            BtnOK.DialogResult = DialogResult.OK;
            BtnOK.Location = new Point(416, 451);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 2;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            // 
            // lbleula
            // 
            lbleula.BackColor = SystemColors.Window;
            lbleula.Location = new Point(16, 143);
            lbleula.Margin = new Padding(4, 5, 4, 5);
            lbleula.Name = "lbleula";
            lbleula.ReadOnly = true;
            lbleula.Size = new Size(507, 247);
            lbleula.TabIndex = 3;
            lbleula.Text = "";
            lbleula.LinkClicked += lbleula_LinkClicked;
            // 
            // label2
            // 
            label2.Location = new Point(20, 97);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(504, 49);
            label2.TabIndex = 6;
            label2.Text = "Use of EasiSlides is subject to your acceptance of the following End User Licence Agreement (EULA):";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(93, 62);
            linkLabel1.Margin = new Padding(4, 0, 4, 0);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(182, 20);
            linkLabel1.TabIndex = 7;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "http://www.easislides.com";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // lblRegDetails
            // 
            lblRegDetails.Location = new Point(205, 400);
            lblRegDetails.Margin = new Padding(4, 5, 4, 5);
            lblRegDetails.Name = "lblRegDetails";
            lblRegDetails.Size = new Size(316, 27);
            lblRegDetails.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 405);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(191, 20);
            label3.TabIndex = 9;
            label3.Text = "Name Displayed at Startup:";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(20, 434);
            lblVersion.Margin = new Padding(4, 0, 4, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(57, 20);
            lblVersion.TabIndex = 10;
            lblVersion.Text = "Version";
            // 
            // lblCopyright
            // 
            lblCopyright.AutoSize = true;
            lblCopyright.Location = new Point(20, 463);
            lblCopyright.Margin = new Padding(4, 0, 4, 0);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(74, 20);
            lblCopyright.TabIndex = 11;
            lblCopyright.Text = "Copyright";
            // 
            // panel1
            // 
            panel1.BackgroundImage = (Image)resources.GetObject("panel1.BackgroundImage");
            panel1.BackgroundImageLayout = ImageLayout.Stretch;
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(16, 18);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(60, 64);
            panel1.TabIndex = 4;
            // 
            // label4
            // 
            label4.Location = new Point(93, 18);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(429, 43);
            label4.TabIndex = 13;
            label4.Text = "EasiSlides provides Christian Lyrics projection and publication for Christian Praise && Worship.";
            // 
            // FrmAbout
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(539, 509);
            Controls.Add(lblRegDetails);
            Controls.Add(linkLabel1);
            Controls.Add(panel1);
            Controls.Add(BtnSysInfo);
            Controls.Add(BtnOK);
            Controls.Add(lbleula);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(lblVersion);
            Controls.Add(lblCopyright);
            Controls.Add(label4);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmAbout";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About EasiSlides";
            Load += FrmAbout_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        public FrmAbout()
		{
			InitializeComponent();
		}

		private void FrmAbout_Load(object sender, EventArgs e)
		{
			lblRegDetails.Text = RegUtil.GetRegValue("config", "RegistrationUser", "");
			lblVersion.Text = "Software Version: 5.0.0";
			lblCopyright.Text = "Copyright " + '©' + " 2019 daniel park revision";
			lbleula.Text = gf.EULA;
			lbleula.SelectionStart = 0;
			lbleula.SelectionLength = 0;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			gf.UserString = DataUtil.Trim(lblRegDetails.Text);
			RegUtil.SaveRegValue("config", "RegistrationUser", gf.UserString);
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			gf.RunProcess(linkLabel1.Text);
		}

		private void lbleula_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			gf.RunProcess(e.LinkText);
		}

		private void BtnSysInfo_Click(object sender, EventArgs e)
		{
			string text = (string)Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\MSINFO").GetValue("Path", "");
			if (File.Exists(text))
			{
				gf.RunProcess(text);
			}
		}
	}
}
