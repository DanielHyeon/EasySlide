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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmRegister));
            BtnCancel = new Button();
            BtnOK = new Button();
            lblRegister = new RichTextBox();
            SuspendLayout();
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(363, 306);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 2;
            BtnCancel.Text = "Close";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(235, 306);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 1;
            BtnOK.Text = "Register...";
            BtnOK.Click += BtnOK_Click;
            // 
            // lblRegister
            // 
            lblRegister.Location = new Point(16, 18);
            lblRegister.Margin = new Padding(4, 5, 4, 5);
            lblRegister.Name = "lblRegister";
            lblRegister.Size = new Size(452, 276);
            lblRegister.TabIndex = 0;
            lblRegister.Text = "";
            lblRegister.LinkClicked += lblRegister_LinkClicked;
            // 
            // FrmRegister
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(488, 360);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(lblRegister);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmRegister";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Register Use of EasiSlides";
            Load += FrmRegister_Load;
            ResumeLayout(false);
        }
    }
}
