using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
    partial class FrmEditBibleItem
    {
		private Button BtnCancel;
		private Button BtnOK;
		private ComboBox BibleVersionsRegion1;
		private ComboBox BibleVersionsRegion2;
		private GroupBox groupBox1;
		private IContainer components = null;
		private Label label1;
		private Label label2;
		private Label label3;
		private TextBox Title;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmEditBibleItem));
            groupBox1 = new GroupBox();
            BibleVersionsRegion2 = new ComboBox();
            BibleVersionsRegion1 = new ComboBox();
            Title = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label1 = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(BibleVersionsRegion2);
            groupBox1.Controls.Add(BibleVersionsRegion1);
            groupBox1.Controls.Add(Title);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(16, 18);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(505, 183);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Change Bible Versions";
            //
            // BibleVersionsRegion2
            //
            BibleVersionsRegion2.DropDownStyle = ComboBoxStyle.DropDownList;
            BibleVersionsRegion2.FormattingEnabled = true;
            BibleVersionsRegion2.Location = new Point(133, 125);
            BibleVersionsRegion2.Margin = new Padding(4, 5, 4, 5);
            BibleVersionsRegion2.Name = "BibleVersionsRegion2";
            BibleVersionsRegion2.Size = new Size(347, 28);
            BibleVersionsRegion2.TabIndex = 4;
            //
            // BibleVersionsRegion1
            //
            BibleVersionsRegion1.DropDownStyle = ComboBoxStyle.DropDownList;
            BibleVersionsRegion1.FormattingEnabled = true;
            BibleVersionsRegion1.Location = new Point(133, 83);
            BibleVersionsRegion1.Margin = new Padding(4, 5, 4, 5);
            BibleVersionsRegion1.Name = "BibleVersionsRegion1";
            BibleVersionsRegion1.Size = new Size(347, 28);
            BibleVersionsRegion1.TabIndex = 2;
            //
            // Title
            //
            Title.BackColor = SystemColors.Window;
            Title.Location = new Point(68, 43);
            Title.Margin = new Padding(4, 5, 4, 5);
            Title.Name = "Title";
            Title.ReadOnly = true;
            Title.Size = new Size(412, 27);
            Title.TabIndex = 0;
            Title.WordWrap = false;
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(11, 88);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(123, 20);
            label2.TabIndex = 1;
            label2.Text = "Region 1 Version:";
            //
            // label3
            //
            label3.AutoSize = true;
            label3.Location = new Point(11, 129);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(123, 20);
            label3.TabIndex = 3;
            label3.Text = "Region 2 Version:";
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(11, 48);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(41, 20);
            label1.TabIndex = 5;
            label1.Text = "Title:";
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(284, 211);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 2;
            BtnCancel.Text = "Cancel";
            BtnCancel.Click += BtnCancel_Click;
            //
            // BtnOK
            //
            BtnOK.Location = new Point(156, 211);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 1;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            //
            // FrmEditBibleItem
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(537, 266);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmEditBibleItem";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Edit Bible Item";
            Load += FrmEditBibleItem_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
