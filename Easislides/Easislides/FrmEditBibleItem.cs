using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmEditBibleItem : Form
	{
		private IContainer components = null;

		private GroupBox groupBox1;

		private Button BtnCancel;

		private Button BtnOK;

		private Label label3;

		private ComboBox BibleVersionsRegion2;

		private Label label2;

		private Label label1;

		private ComboBox BibleVersionsRegion1;

		private TextBox Title;

		private string InIDStringFirstPart;

		private string InIDString;

		private int InRegion1;

		private int InRegion2;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEditBibleItem));
			groupBox1 = new System.Windows.Forms.GroupBox();
			BibleVersionsRegion2 = new System.Windows.Forms.ComboBox();
			BibleVersionsRegion1 = new System.Windows.Forms.ComboBox();
			Title = new System.Windows.Forms.TextBox();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			groupBox1.SuspendLayout();
			SuspendLayout();
			groupBox1.Controls.Add(BibleVersionsRegion2);
			groupBox1.Controls.Add(BibleVersionsRegion1);
			groupBox1.Controls.Add(Title);
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(label1);
			groupBox1.Location = new System.Drawing.Point(12, 12);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(379, 119);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Change Bible Versions";
			BibleVersionsRegion2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			BibleVersionsRegion2.FormattingEnabled = true;
			BibleVersionsRegion2.Location = new System.Drawing.Point(100, 81);
			BibleVersionsRegion2.Name = "BibleVersionsRegion2";
			BibleVersionsRegion2.Size = new System.Drawing.Size(261, 21);
			BibleVersionsRegion2.TabIndex = 4;
			BibleVersionsRegion1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			BibleVersionsRegion1.FormattingEnabled = true;
			BibleVersionsRegion1.Location = new System.Drawing.Point(100, 54);
			BibleVersionsRegion1.Name = "BibleVersionsRegion1";
			BibleVersionsRegion1.Size = new System.Drawing.Size(261, 21);
			BibleVersionsRegion1.TabIndex = 2;
			Title.BackColor = System.Drawing.SystemColors.Window;
			Title.Location = new System.Drawing.Point(51, 28);
			Title.Name = "Title";
			Title.ReadOnly = true;
			Title.Size = new System.Drawing.Size(310, 20);
			Title.TabIndex = 0;
			Title.WordWrap = false;
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(8, 57);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(91, 13);
			label2.TabIndex = 1;
			label2.Text = "Region 1 Version:";
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(8, 84);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(91, 13);
			label3.TabIndex = 3;
			label3.Text = "Region 2 Version:";
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(8, 31);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(30, 13);
			label1.TabIndex = 5;
			label1.Text = "Title:";
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(213, 137);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 2;
			BtnCancel.Text = "Cancel";
			BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
			BtnOK.Location = new System.Drawing.Point(117, 137);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 1;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(403, 173);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(groupBox1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmEditBibleItem";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Edit Bible Item";
			base.Load += new System.EventHandler(FrmEditBibleItem_Load);
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			ResumeLayout(false);
		}

		public FrmEditBibleItem()
		{
			InitializeComponent();
		}

		private void FrmEditBibleItem_Load(object sender, EventArgs e)
		{
			Title.Text = gf.EditBible_Title;
			LoadBibleList(ref BibleVersionsRegion1);
			LoadBibleList(ref BibleVersionsRegion2);
			InIDString = gf.EditBible_IDString;
			gf.EditBible_IDString = "";
			InIDStringFirstPart = DataUtil.ExtractOneInfo(ref InIDString, ';');
			InRegion1 = gf.LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InIDString, ';')) + 1;
			InRegion2 = gf.LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InIDString, ';')) + 1;
			BibleVersionsRegion1.SelectedIndex = InRegion1;
			BibleVersionsRegion2.SelectedIndex = InRegion2;
		}

		private void LoadBibleList(ref ComboBox InBibleList)
		{
			InBibleList.Items.Clear();
			InBibleList.Items.Add("");
			for (int i = 0; i <= gf.HB_TotalVersions - 1; i++)
			{
				InBibleList.Items.Add(gf.HB_Versions[i, 1] + " - " + gf.HB_Versions[i, 2]);
			}
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (ValidateVersionOptions())
			{
				if (!((BibleVersionsRegion1.SelectedIndex == InRegion1) & (BibleVersionsRegion2.SelectedIndex == InRegion2)))
				{
					BuildNewIDString();
				}
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}

		private bool ValidateVersionOptions()
		{
			if (BibleVersionsRegion1.SelectedIndex == 0)
			{
				MessageBox.Show("You must select a Bible Version for Region 1");
				return false;
			}
			return true;
		}

		private void BuildNewIDString()
		{
			string text = "";
			string text2 = "";
			text = gf.GetDisplayNameOnly(ref gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 4], UpdateByRef: false, KeepExt: true);
			if (BibleVersionsRegion2.SelectedIndex > 0)
			{
				text2 = gf.GetDisplayNameOnly(ref gf.HB_Versions[BibleVersionsRegion2.SelectedIndex - 1, 4], UpdateByRef: false, KeepExt: true);
			}
			//gf.EditBible_IDString = InIDStringFirstPart + ';' + text + ';' + text2 + ';' + InIDString;
			gf.EditBible_IDString = $"{InIDStringFirstPart};{text};{text2};{InIDString}";
			
			int num = gf.EditBible_Title.IndexOf('(');
			if (num > 0)
			{
				gf.EditBible_Title = DataUtil.Trim(DataUtil.Left(gf.EditBible_Title, num - 1));
			}
			if (text2 == "")
			{
				gf.EditBible_Title = gf.EditBible_Title + " (" + gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 1] + ")";
				return;
			}
			string editBible_Title = gf.EditBible_Title;
			//gf.EditBible_Title = editBible_Title + " (" + gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 1] + "/" + gf.HB_Versions[BibleVersionsRegion2.SelectedIndex - 1, 1] + ")";
			gf.EditBible_Title = $"{editBible_Title} ({gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 1]}/{gf.HB_Versions[BibleVersionsRegion2.SelectedIndex - 1, 1]})";
		}

		private void BtnOK_Click()
		{
		}

		private void BtnCancel_Click()
		{
		}
	}
}
