using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmBibleRename : Form
	{
		private IContainer components = null;

		private Label label1;

		private TextBox textBoxNewString;

		private Button BtnCancel;

		private Button BtnOK;

		private string InString = "";

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBibleRename));
			label1 = new System.Windows.Forms.Label();
			textBoxNewString = new System.Windows.Forms.TextBox();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			SuspendLayout();
			label1.Location = new System.Drawing.Point(12, 21);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(306, 31);
			label1.TabIndex = 3;
			label1.Text = "Rename:";
			textBoxNewString.Location = new System.Drawing.Point(13, 55);
			textBoxNewString.Name = "textBoxNewString";
			textBoxNewString.Size = new System.Drawing.Size(305, 20);
			textBoxNewString.TabIndex = 0;
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(171, 92);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 2;
			BtnCancel.Text = "Cancel";
			BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
			BtnOK.Location = new System.Drawing.Point(75, 92);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 1;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(330, 128);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(textBoxNewString);
			base.Controls.Add(label1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmBibleRename";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Rename Bible";
			base.Load += new System.EventHandler(FrmRename_Load);
			ResumeLayout(false);
			PerformLayout();
		}

		public FrmBibleRename()
		{
			InitializeComponent();
		}

		private void FrmRename_Load(object sender, EventArgs e)
		{
			InString = DataUtil.Trim(gf.Rename_String);
			label1.Text = "Rename '" + gf.Rename_String + "' to:";
			gf.Rename_ExistingString = gf.Rename_ExistingString.ToLower();
			textBoxNewString.Text = gf.Rename_String;
			textBoxNewString.SelectAll();
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			textBoxNewString.Text = DataUtil.Trim(textBoxNewString.Text);
			if (textBoxNewString.Text.Length > 0)
			{
				if (textBoxNewString.Text == gf.Rename_String)
				{
					Close();
				}
				else if (gf.Rename_ExistingString.IndexOf(textBoxNewString.Text.ToLower()) < 0)
				{
					gf.Rename_String = textBoxNewString.Text;
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Name already exists! Please try a different Bible Name.");
				}
			}
			else
			{
				MessageBox.Show("Please enter a new Bible Name.");
			}
		}
	}
}
