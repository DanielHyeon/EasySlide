using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    partial class FrmRegister
    {
		private Button BtnCancel;
		private Button BtnOK;
		private IContainer components = null;
		private RichTextBox lblRegister;

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

        #endregion
    }
}
