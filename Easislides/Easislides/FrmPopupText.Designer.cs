using System;
using System.ComponentModel;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
    partial class FrmPopupText
    {
		private Button BtnCancel;
		private Button BtnOK;
		private IContainer components = null;
		private TextBox tbData;

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
			tbData = new System.Windows.Forms.TextBox();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			SuspendLayout();
			tbData.Location = new System.Drawing.Point(3, 2);
			tbData.Multiline = true;
			tbData.Name = "tbData";
			tbData.Size = new System.Drawing.Size(258, 156);
			tbData.TabIndex = 0;
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			BtnCancel.Location = new System.Drawing.Point(139, 161);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 2;
			BtnCancel.Text = "Cancel";
			BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
			BtnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			BtnOK.Location = new System.Drawing.Point(43, 161);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 1;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(264, 188);
			base.ControlBox = false;
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(tbData);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Name = "FrmPopupText";
			base.ShowInTaskbar = false;
			base.TopMost = true;
			base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(FrmPopup_FormClosed);
			base.Load += new System.EventHandler(FrmPopup_Load);
			ResumeLayout(false);
			PerformLayout();
		}

        #endregion
    }
}
