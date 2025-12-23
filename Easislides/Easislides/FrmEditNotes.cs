using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmEditNotes : Form
	{
		private IContainer components = null;

		private Button BtnCancel;

		private Button BtnOK;

		private TextBox tbData;

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
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			tbData = new System.Windows.Forms.TextBox();
			SuspendLayout();
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			BtnCancel.Location = new System.Drawing.Point(156, 236);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 5;
			BtnCancel.Text = "Cancel";
			BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			BtnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			BtnOK.Location = new System.Drawing.Point(60, 236);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 4;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			tbData.Location = new System.Drawing.Point(3, 2);
			tbData.Multiline = true;
			tbData.Name = "tbData";
			tbData.Size = new System.Drawing.Size(286, 228);
			tbData.TabIndex = 3;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(292, 266);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(tbData);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmEditNotes";
			base.ShowIcon = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Edit";
			base.TopMost = true;
			base.Load += new System.EventHandler(FrmEditNotes_Load);
			ResumeLayout(false);
			PerformLayout();
		}

		public FrmEditNotes()
		{
			InitializeComponent();
		}

		private void FrmEditNotes_Load(object sender, EventArgs e)
		{
			Text = gf.EditNotesHeading;
			tbData.Text = gf.EditNotes;
			if (tbData.Text != "")
			{
				tbData.SelectionStart = 0;
				tbData.SelectionLength = 0;
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			gf.EditNotes = tbData.Text;
		}
	}
}
