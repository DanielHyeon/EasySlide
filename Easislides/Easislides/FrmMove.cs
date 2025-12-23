using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmMove : Form
	{
		private IContainer components = null;

		private Button BtnCancel;

		private Button BtnOK;

		private ListView SongFolder;

		private Label Label1;

		private ColumnHeader columnHeader1;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMove));
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			SongFolder = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			Label1 = new System.Windows.Forms.Label();
			SuspendLayout();
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(154, 179);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 2;
			BtnCancel.Text = "Cancel";
			BtnOK.Location = new System.Drawing.Point(58, 179);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 1;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			SongFolder.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1]
			{
				columnHeader1
			});
			SongFolder.FullRowSelect = true;
			SongFolder.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			SongFolder.Location = new System.Drawing.Point(12, 46);
			SongFolder.MultiSelect = false;
			SongFolder.Name = "SongFolder";
			SongFolder.Size = new System.Drawing.Size(277, 121);
			SongFolder.TabIndex = 0;
			SongFolder.UseCompatibleStateImageBehavior = false;
			SongFolder.View = System.Windows.Forms.View.Details;
			SongFolder.DoubleClick += new System.EventHandler(SongFolder_DoubleClick);
			columnHeader1.Width = 240;
			Label1.Location = new System.Drawing.Point(13, 9);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(276, 34);
			Label1.TabIndex = 3;
			Label1.Text = "label1";
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(301, 215);
			base.Controls.Add(Label1);
			base.Controls.Add(SongFolder);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmMove";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Move item(s) to another folder";
			base.Load += new System.EventHandler(FrmMove_Load);
			ResumeLayout(false);
		}

		public FrmMove()
		{
			InitializeComponent();
		}

		private void FrmMove_Load(object sender, EventArgs e)
		{
			Label1.Text = "You have selected " + gf.SelectedItemsCount + " item" + ((gf.SelectedItemsCount > 1) ? "s" : "") + " for Moving. Please choose a folder to move the item" + ((gf.SelectedItemsCount > 1) ? "s" : "") + " to, and then click OK.";
			gf.MoveToFolder = -1;
			SongFolder.Items.Clear();
			for (int i = 1; i <= 40; i++)
			{
				if (gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(gf.FolderName[i]);
				}
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (SongFolder.Items.Count > 0)
			{
				if (SongFolder.SelectedItems.Count > 0)
				{
					gf.MoveToFolder = gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Please select a folder to move the songs to!");
				}
			}
			else
			{
				MessageBox.Show("There are no Song Folders enabled!");
			}
		}

		private void SongFolder_DoubleClick(object sender, EventArgs e)
		{
			BtnOK.PerformClick();
		}
	}
}
