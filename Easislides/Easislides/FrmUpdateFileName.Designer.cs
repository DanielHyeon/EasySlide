using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
    partial class FrmUpdateFileName
    {
		private Button BtnCancel;
		private Button BtnOK;
		private IContainer components = null;
		private Label Mess;
		private TextBox tbFileName;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmUpdateFileName));
            tbFileName = new TextBox();
            Mess = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            SuspendLayout();
            //
            // tbFileName
            //
            tbFileName.Location = new Point(16, 54);
            tbFileName.Margin = new Padding(4, 5, 4, 5);
            tbFileName.Name = "tbFileName";
            tbFileName.Size = new Size(411, 27);
            tbFileName.TabIndex = 0;
            //
            // Mess
            //
            Mess.Location = new Point(16, 14);
            Mess.Margin = new Padding(4, 0, 4, 0);
            Mess.Name = "Mess";
            Mess.Size = new Size(421, 22);
            Mess.TabIndex = 1;
            Mess.Text = "T";
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(236, 109);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 2;
            BtnCancel.Text = "Cancel";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(108, 109);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 1;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            //
            // FrmUpdateFileName
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(444, 174);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(Mess);
            Controls.Add(tbFileName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmUpdateFileName";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Load += frmUpdateFileName_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
