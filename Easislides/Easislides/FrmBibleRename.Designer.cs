using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
    partial class FrmBibleRename
    {
		private Button BtnCancel;
		private Button BtnOK;
		private IContainer components = null;
		private Label label1;
		private TextBox textBoxNewString;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmBibleRename));
            label1 = new Label();
            textBoxNewString = new TextBox();
            BtnCancel = new Button();
            BtnOK = new Button();
            SuspendLayout();
            //
            // label1
            //
            label1.Location = new Point(16, 32);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(408, 48);
            label1.TabIndex = 3;
            label1.Text = "Rename:";
            //
            // textBoxNewString
            //
            textBoxNewString.Location = new Point(17, 85);
            textBoxNewString.Margin = new Padding(4, 5, 4, 5);
            textBoxNewString.Name = "textBoxNewString";
            textBoxNewString.Size = new Size(405, 27);
            textBoxNewString.TabIndex = 0;
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(228, 142);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 2;
            BtnCancel.Text = "Cancel";
            BtnCancel.Click += BtnCancel_Click;
            //
            // BtnOK
            //
            BtnOK.Location = new Point(100, 142);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 1;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            //
            // FrmBibleRename
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(440, 197);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(textBoxNewString);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmBibleRename";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Rename Bible";
            Load += FrmRename_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
