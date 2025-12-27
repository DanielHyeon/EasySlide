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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmMove));
            BtnCancel = new Button();
            BtnOK = new Button();
            SongFolder = new ListView();
            columnHeader1 = new ColumnHeader();
            Label1 = new Label();
            SuspendLayout();
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(205, 275);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 2;
            BtnCancel.Text = "Cancel";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(77, 275);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 1;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            // 
            // SongFolder
            // 
            SongFolder.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            SongFolder.FullRowSelect = true;
            SongFolder.HeaderStyle = ColumnHeaderStyle.None;
            SongFolder.Location = new Point(16, 71);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.MultiSelect = false;
            SongFolder.Name = "SongFolder";
            SongFolder.Size = new Size(368, 184);
            SongFolder.TabIndex = 0;
            SongFolder.UseCompatibleStateImageBehavior = false;
            SongFolder.View = View.Details;
            SongFolder.DoubleClick += SongFolder_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 240;
            // 
            // Label1
            // 
            Label1.Location = new Point(17, 14);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(368, 52);
            Label1.TabIndex = 3;
            Label1.Text = "label1";
            // 
            // FrmMove
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(401, 331);
            Controls.Add(Label1);
            Controls.Add(SongFolder);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmMove";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Move item(s) to another folder";
            Load += FrmMove_Load;
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
