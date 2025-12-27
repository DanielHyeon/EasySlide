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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmCopyMoveExternal));
            BtnCancel = new Button();
            BtnOK = new Button();
            Label1 = new Label();
            optCopyToInfoScreen = new RadioButton();
            optCopyToFolder = new RadioButton();
            ExternalFilesFolder = new ListView();
            columnHeader2 = new ColumnHeader();
            SongFolder = new ListView();
            columnHeader1 = new ColumnHeader();
            SuspendLayout();
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(291, 292);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 6;
            BtnCancel.Text = "Cancel";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(163, 292);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 5;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            // 
            // Label1
            // 
            Label1.Location = new Point(17, 12);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(527, 52);
            Label1.TabIndex = 0;
            Label1.Text = "label1";
            // 
            // optCopyToInfoScreen
            // 
            optCopyToInfoScreen.AutoSize = true;
            optCopyToInfoScreen.Checked = true;
            optCopyToInfoScreen.Location = new Point(23, 68);
            optCopyToInfoScreen.Margin = new Padding(4, 5, 4, 5);
            optCopyToInfoScreen.Name = "optCopyToInfoScreen";
            optCopyToInfoScreen.Size = new Size(92, 24);
            optCopyToInfoScreen.TabIndex = 1;
            optCopyToInfoScreen.TabStop = true;
            optCopyToInfoScreen.Text = "To Folder";
            optCopyToInfoScreen.UseVisualStyleBackColor = true;
            // 
            // optCopyToFolder
            // 
            optCopyToFolder.AutoSize = true;
            optCopyToFolder.Location = new Point(291, 69);
            optCopyToFolder.Margin = new Padding(4, 5, 4, 5);
            optCopyToFolder.Name = "optCopyToFolder";
            optCopyToFolder.Size = new Size(162, 24);
            optCopyToFolder.TabIndex = 3;
            optCopyToFolder.Text = "To Database Folder:";
            optCopyToFolder.UseVisualStyleBackColor = true;
            // 
            // ExternalFilesFolder
            // 
            ExternalFilesFolder.Columns.AddRange(new ColumnHeader[] { columnHeader2 });
            ExternalFilesFolder.FullRowSelect = true;
            ExternalFilesFolder.HeaderStyle = ColumnHeaderStyle.None;
            ExternalFilesFolder.Location = new Point(15, 98);
            ExternalFilesFolder.Margin = new Padding(4, 5, 4, 5);
            ExternalFilesFolder.MultiSelect = false;
            ExternalFilesFolder.Name = "ExternalFilesFolder";
            ExternalFilesFolder.Size = new Size(260, 178);
            ExternalFilesFolder.TabIndex = 2;
            ExternalFilesFolder.UseCompatibleStateImageBehavior = false;
            ExternalFilesFolder.View = View.Details;
            ExternalFilesFolder.DoubleClick += ExternalFilesFolder_DoubleClick;
            ExternalFilesFolder.KeyUp += ExternalFilesFolder_KeyUp;
            ExternalFilesFolder.MouseUp += ExternalFilesFolder_MouseUp;
            // 
            // columnHeader2
            // 
            columnHeader2.Width = 180;
            // 
            // SongFolder
            // 
            SongFolder.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            SongFolder.FullRowSelect = true;
            SongFolder.HeaderStyle = ColumnHeaderStyle.None;
            SongFolder.Location = new Point(284, 98);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.MultiSelect = false;
            SongFolder.Name = "SongFolder";
            SongFolder.Size = new Size(260, 178);
            SongFolder.TabIndex = 4;
            SongFolder.UseCompatibleStateImageBehavior = false;
            SongFolder.View = View.Details;
            SongFolder.DoubleClick += SongFolder_DoubleClick;
            SongFolder.KeyUp += SongFolder_KeyUp;
            SongFolder.MouseUp += SongFolder_MouseUp;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 180;
            // 
            // FrmCopyMoveExternal
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(559, 349);
            Controls.Add(optCopyToInfoScreen);
            Controls.Add(optCopyToFolder);
            Controls.Add(ExternalFilesFolder);
            Controls.Add(SongFolder);
            Controls.Add(Label1);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmCopyMoveExternal";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "External Files";
            Load += FrmCopyMoveExternal_Load;
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
