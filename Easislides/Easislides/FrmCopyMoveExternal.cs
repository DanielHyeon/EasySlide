using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmCopyMoveExternal : Form
	{
		private IContainer components = null;

		private Button BtnCancel;

		private Button BtnOK;

		private Label Label1;

		private RadioButton optCopyToInfoScreen;

		private RadioButton optCopyToFolder;

		private ListView ExternalFilesFolder;

		private ColumnHeader columnHeader2;

		private ListView SongFolder;

		private ColumnHeader columnHeader1;

		private string ActionString1 = "";

		private string ActionString2 = "";

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCopyMoveExternal));
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			Label1 = new System.Windows.Forms.Label();
			optCopyToInfoScreen = new System.Windows.Forms.RadioButton();
			optCopyToFolder = new System.Windows.Forms.RadioButton();
			ExternalFilesFolder = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			SongFolder = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			SuspendLayout();
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(218, 190);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 6;
			BtnCancel.Text = "Cancel";
			BtnOK.Location = new System.Drawing.Point(122, 190);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 5;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			Label1.Location = new System.Drawing.Point(13, 8);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(395, 34);
			Label1.TabIndex = 0;
			Label1.Text = "label1";
			optCopyToInfoScreen.AutoSize = true;
			optCopyToInfoScreen.Checked = true;
			optCopyToInfoScreen.Location = new System.Drawing.Point(17, 44);
			optCopyToInfoScreen.Name = "optCopyToInfoScreen";
			optCopyToInfoScreen.Size = new System.Drawing.Size(70, 17);
			optCopyToInfoScreen.TabIndex = 1;
			optCopyToInfoScreen.TabStop = true;
			optCopyToInfoScreen.Text = "To Folder";
			optCopyToInfoScreen.UseVisualStyleBackColor = true;
			optCopyToFolder.AutoSize = true;
			optCopyToFolder.Location = new System.Drawing.Point(218, 45);
			optCopyToFolder.Name = "optCopyToFolder";
			optCopyToFolder.Size = new System.Drawing.Size(122, 17);
			optCopyToFolder.TabIndex = 3;
			optCopyToFolder.Text = "To Database Folder:";
			optCopyToFolder.UseVisualStyleBackColor = true;
			ExternalFilesFolder.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1]
			{
				columnHeader2
			});
			ExternalFilesFolder.FullRowSelect = true;
			ExternalFilesFolder.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			ExternalFilesFolder.HideSelection = false;
			ExternalFilesFolder.Location = new System.Drawing.Point(11, 64);
			ExternalFilesFolder.MultiSelect = false;
			ExternalFilesFolder.Name = "ExternalFilesFolder";
			ExternalFilesFolder.Size = new System.Drawing.Size(196, 117);
			ExternalFilesFolder.TabIndex = 2;
			ExternalFilesFolder.UseCompatibleStateImageBehavior = false;
			ExternalFilesFolder.View = System.Windows.Forms.View.Details;
			ExternalFilesFolder.DoubleClick += new System.EventHandler(ExternalFilesFolder_DoubleClick);
			ExternalFilesFolder.MouseUp += new System.Windows.Forms.MouseEventHandler(ExternalFilesFolder_MouseUp);
			ExternalFilesFolder.KeyUp += new System.Windows.Forms.KeyEventHandler(ExternalFilesFolder_KeyUp);
			columnHeader2.Width = 180;
			SongFolder.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1]
			{
				columnHeader1
			});
			SongFolder.FullRowSelect = true;
			SongFolder.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			SongFolder.HideSelection = false;
			SongFolder.Location = new System.Drawing.Point(213, 64);
			SongFolder.MultiSelect = false;
			SongFolder.Name = "SongFolder";
			SongFolder.Size = new System.Drawing.Size(196, 117);
			SongFolder.TabIndex = 4;
			SongFolder.UseCompatibleStateImageBehavior = false;
			SongFolder.View = System.Windows.Forms.View.Details;
			SongFolder.DoubleClick += new System.EventHandler(SongFolder_DoubleClick);
			SongFolder.MouseUp += new System.Windows.Forms.MouseEventHandler(SongFolder_MouseUp);
			SongFolder.KeyUp += new System.Windows.Forms.KeyEventHandler(SongFolder_KeyUp);
			columnHeader1.Width = 180;
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(419, 227);
			base.Controls.Add(optCopyToInfoScreen);
			base.Controls.Add(optCopyToFolder);
			base.Controls.Add(ExternalFilesFolder);
			base.Controls.Add(SongFolder);
			base.Controls.Add(Label1);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmCopyMoveExternal";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "External Files";
			base.Load += new System.EventHandler(FrmCopyMoveExternal_Load);
			ResumeLayout(false);
			PerformLayout();
		}

		public FrmCopyMoveExternal()
		{
			InitializeComponent();
		}

		private void FrmCopyMoveExternal_Load(object sender, EventArgs e)
		{
			if (gf.ExternalCopyFolder >= 1)
			{
				ActionString1 = "copying";
				ActionString2 = "copy";
				switch (gf.ExternalMoveCopyType)
				{
				case "I":
					Text = "Copy InfoScreen(s)";
					optCopyToFolder.Enabled = true;
					SongFolder.Enabled = true;
					break;
				case "P":
					Text = "Copy Powerpoint File(s)";
					optCopyToInfoScreen.Checked = true;
					optCopyToFolder.Enabled = false;
					SongFolder.Enabled = false;
					break;
				}
			}
			else
			{
				ActionString1 = "moving";
				ActionString2 = "move";
				optCopyToInfoScreen.Checked = true;
				optCopyToFolder.Enabled = false;
				SongFolder.Enabled = false;
				switch (gf.ExternalMoveCopyType)
				{
				case "I":
					Text = "Move InfoScreen(s)";
					break;
				case "P":
					Text = "Move Powerpoint File(s)";
					break;
				}
			}
			Label1.Text = "You have selected " + gf.SelectedItemsCount + " item" + ((gf.SelectedItemsCount > 1) ? "s" : "") + " for " + ActionString1 + ". Please choose a folder to " + ActionString2 + " the item" + ((gf.SelectedItemsCount > 1) ? "s" : "") + " to, and then click OK.";
			ExternalFilesFolder.Items.Clear();
			switch (gf.ExternalMoveCopyType)
			{
			case "I":
			{
				for (int i = 0; i < gf.InfoScreenFolderTotal; i++)
				{
					ExternalFilesFolder.Items.Add(gf.InfoScreenGroups[i, 0]);
				}
				break;
			}
			case "P":
			{
				for (int i = 0; i < gf.PowerpointFolderTotal; i++)
				{
					ExternalFilesFolder.Items.Add(gf.PowerpointGroups[i, 0]);
				}
				break;
			}
			}
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
			if (optCopyToInfoScreen.Checked)
			{
				if (ExternalFilesFolder.SelectedItems.Count > 0)
				{
					if (gf.ExternalCopyFolder >= 1)
					{
						gf.ExternalCopyFolder = gf.GetSelectedIndex(ExternalFilesFolder);
					}
					else
					{
						gf.ExternalMoveFolder = gf.GetSelectedIndex(ExternalFilesFolder);
					}
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Please select a folder to " + ActionString2 + " to!");
				}
			}
			else if (SongFolder.Items.Count > 0)
			{
				if (SongFolder.SelectedItems.Count > 0)
				{
					if (gf.ExternalCopyFolder >= 1)
					{
						gf.ExternalCopyFolder = -1 * gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					}
					else
					{
						gf.ExternalMoveFolder = -1 * gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					}
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Please select a folder to " + ActionString2 + " to!");
				}
			}
			else
			{
				MessageBox.Show("There are no Song Folders enabled!");
			}
		}

		private void SongFolder_DoubleClick(object sender, EventArgs e)
		{
			SelectOptCopyToFolder(0);
			BtnOK.PerformClick();
		}

		private void SongFolder_MouseUp(object sender, MouseEventArgs e)
		{
			SelectOptCopyToFolder(0);
		}

		private void SongFolder_KeyUp(object sender, KeyEventArgs e)
		{
			SelectOptCopyToFolder(0);
		}

		private void ExternalFilesFolder_DoubleClick(object sender, EventArgs e)
		{
			SelectOptCopyToFolder(1);
			BtnOK.PerformClick();
		}

		private void ExternalFilesFolder_KeyUp(object sender, KeyEventArgs e)
		{
			SelectOptCopyToFolder(1);
		}

		private void ExternalFilesFolder_MouseUp(object sender, MouseEventArgs e)
		{
			SelectOptCopyToFolder(1);
		}

		private void SelectOptCopyToFolder(int SelectedOpt)
		{
			if (SelectedOpt == 0)
			{
				optCopyToFolder.Checked = true;
			}
			else
			{
				optCopyToInfoScreen.Checked = true;
			}
		}
	}
}
