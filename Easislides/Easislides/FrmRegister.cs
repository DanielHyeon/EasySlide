using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmRegister : Form
	{
		private IContainer components = null;

		private Button BtnCancel;

		private Button BtnOK;

		private RichTextBox lblRegister;

		public FrmRegister()
		{
			InitializeComponent();
		}

		private void FrmRegister_Load(object sender, EventArgs e)
		{
			lblRegister.Text = "EasiSlides Version 4.0.5 is provided free of charge and for your indefinite use provided you abide by the End User Licence Agreement (EULA).  \r\n\r\nIf you intend to use this software on an on-going basis, you are invited to register your use of the software by clicking on the 'Register' button below which will take you to the EasiSlides Registration Page at http://www.easislides.com/register \r\n\r\nRegistration is voluntary and is free of charge.  The registration information you provide will help us to monitor the spread of use of EasiSlides around the world.";
			lblRegister.SelectionStart = 0;
			lblRegister.SelectionLength = 0;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			gf.RunProcess("http://www.easislides.com/register");
		}

		private void lblRegister_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			gf.RunProcess(e.LinkText);
		}

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRegister));
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			lblRegister = new System.Windows.Forms.RichTextBox();
			SuspendLayout();
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(272, 199);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 2;
			BtnCancel.Text = "Close";
			BtnOK.Location = new System.Drawing.Point(176, 199);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 1;
			BtnOK.Text = "Register...";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			lblRegister.Location = new System.Drawing.Point(12, 12);
			lblRegister.Name = "lblRegister";
			lblRegister.Size = new System.Drawing.Size(340, 181);
			lblRegister.TabIndex = 0;
			lblRegister.Text = "";
			lblRegister.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(lblRegister_LinkClicked);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(366, 234);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(lblRegister);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmRegister";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Register Use of EasiSlides";
			base.Load += new System.EventHandler(FrmRegister_Load);
			ResumeLayout(false);
		}
	}
}
