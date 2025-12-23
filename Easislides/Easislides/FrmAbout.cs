using Easislides.Util;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAbout));
			BtnSysInfo = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			lbleula = new System.Windows.Forms.RichTextBox();
			label2 = new System.Windows.Forms.Label();
			linkLabel1 = new System.Windows.Forms.LinkLabel();
			lblRegDetails = new System.Windows.Forms.TextBox();
			label3 = new System.Windows.Forms.Label();
			lblVersion = new System.Windows.Forms.Label();
			lblCopyright = new System.Windows.Forms.Label();
			panel1 = new System.Windows.Forms.Panel();
			label4 = new System.Windows.Forms.Label();
			SuspendLayout();
			BtnSysInfo.Location = new System.Drawing.Point(226, 293);
			BtnSysInfo.Name = "BtnSysInfo";
			BtnSysInfo.Size = new System.Drawing.Size(80, 24);
			BtnSysInfo.TabIndex = 1;
			BtnSysInfo.Text = "System Info";
			BtnSysInfo.Click += new System.EventHandler(BtnSysInfo_Click);
			BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			BtnOK.Location = new System.Drawing.Point(312, 293);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 2;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			lbleula.BackColor = System.Drawing.SystemColors.Window;
			lbleula.Location = new System.Drawing.Point(12, 93);
			lbleula.Name = "lbleula";
			lbleula.ReadOnly = true;
			lbleula.Size = new System.Drawing.Size(381, 162);
			lbleula.TabIndex = 3;
			lbleula.Text = "";
			lbleula.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(lbleula_LinkClicked);
			label2.Location = new System.Drawing.Point(15, 63);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(378, 32);
			label2.TabIndex = 6;
			label2.Text = "Use of EasiSlides is subject to your acceptance of the following End User Licence Agreement (EULA):";
			linkLabel1.AutoSize = true;
			linkLabel1.Location = new System.Drawing.Point(70, 40);
			linkLabel1.Name = "linkLabel1";
			linkLabel1.Size = new System.Drawing.Size(133, 13);
			linkLabel1.TabIndex = 7;
			((System.Windows.Forms.Label)linkLabel1).TabStop = true;
			linkLabel1.Text = "http://www.easislides.com";
			linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
			lblRegDetails.Location = new System.Drawing.Point(154, 260);
			lblRegDetails.Name = "lblRegDetails";
			lblRegDetails.Size = new System.Drawing.Size(238, 20);
			lblRegDetails.TabIndex = 0;
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(15, 263);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(136, 13);
			label3.TabIndex = 9;
			label3.Text = "Name Displayed at Startup:";
			lblVersion.AutoSize = true;
			lblVersion.Location = new System.Drawing.Point(15, 282);
			lblVersion.Name = "lblVersion";
			lblVersion.Size = new System.Drawing.Size(42, 13);
			lblVersion.TabIndex = 10;
			lblVersion.Text = "Version";
			lblCopyright.AutoSize = true;
			lblCopyright.Location = new System.Drawing.Point(15, 301);
			lblCopyright.Name = "lblCopyright";
			lblCopyright.Size = new System.Drawing.Size(51, 13);
			lblCopyright.TabIndex = 11;
			lblCopyright.Text = "Copyright";
			panel1.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel1.BackgroundImage");
			panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panel1.Location = new System.Drawing.Point(12, 12);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(46, 43);
			panel1.TabIndex = 4;
			label4.Location = new System.Drawing.Point(70, 12);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(322, 28);
			label4.TabIndex = 13;
			label4.Text = "EasiSlides provides Christian Lyrics projection and publication for Christian Praise && Worship.";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(404, 331);
			base.Controls.Add(lblRegDetails);
			base.Controls.Add(linkLabel1);
			base.Controls.Add(panel1);
			base.Controls.Add(BtnSysInfo);
			base.Controls.Add(BtnOK);
			base.Controls.Add(lbleula);
			base.Controls.Add(label2);
			base.Controls.Add(label3);
			base.Controls.Add(lblVersion);
			base.Controls.Add(lblCopyright);
			base.Controls.Add(label4);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmAbout";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "About EasiSlides";
			base.Load += new System.EventHandler(FrmAbout_Load);
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
