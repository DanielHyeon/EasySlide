using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Properties;

namespace Easislides
{
	public class FrmRearrangeFolderPositions : Form
	{
		private IContainer components = null;

		private ListView SongFolder;

		private Button OKBtn;

		private Button CancelBtn;

		private ColumnHeader columnHeader1;

		private ToolStrip toolStripRearrangeFolders;

		private ToolStripButton SF_Up;

		private ToolStripButton SF_Down;

		private ColumnHeader columnHeader2;

		private Panel panel1;

		private int[] OriginalFolderPosition = new int[gf.MAXSONGSFOLDERS];

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmRearrangeFolderPositions));
            SongFolder = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            OKBtn = new Button();
            CancelBtn = new Button();
            toolStripRearrangeFolders = new ToolStrip();
            SF_Up = new ToolStripButton();
            SF_Down = new ToolStripButton();
            panel1 = new Panel();
            toolStripRearrangeFolders.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // SongFolder
            // 
            SongFolder.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            SongFolder.FullRowSelect = true;
            SongFolder.HeaderStyle = ColumnHeaderStyle.None;
            SongFolder.Location = new Point(16, 18);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.Name = "SongFolder";
            SongFolder.ShowGroups = false;
            SongFolder.ShowItemToolTips = true;
            SongFolder.Size = new Size(207, 319);
            SongFolder.TabIndex = 0;
            SongFolder.UseCompatibleStateImageBehavior = false;
            SongFolder.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "";
            columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            columnHeader2.Width = 0;
            // 
            // OKBtn
            // 
            OKBtn.Location = new Point(265, 32);
            OKBtn.Margin = new Padding(4, 5, 4, 5);
            OKBtn.Name = "OKBtn";
            OKBtn.Size = new Size(107, 37);
            OKBtn.TabIndex = 1;
            OKBtn.Text = "Apply";
            OKBtn.Click += OKBtn_Click;
            // 
            // CancelBtn
            // 
            CancelBtn.DialogResult = DialogResult.Cancel;
            CancelBtn.Location = new Point(265, 78);
            CancelBtn.Margin = new Padding(4, 5, 4, 5);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(107, 37);
            CancelBtn.TabIndex = 2;
            CancelBtn.Text = "Cancel";
            // 
            // toolStripRearrangeFolders
            // 
            toolStripRearrangeFolders.AutoSize = false;
            toolStripRearrangeFolders.CanOverflow = false;
            toolStripRearrangeFolders.Dock = DockStyle.None;
            toolStripRearrangeFolders.GripStyle = ToolStripGripStyle.Hidden;
            toolStripRearrangeFolders.ImageScalingSize = new Size(20, 20);
            toolStripRearrangeFolders.Items.AddRange(new ToolStripItem[] { SF_Up, SF_Down });
            toolStripRearrangeFolders.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripRearrangeFolders.Location = new Point(0, 0);
            toolStripRearrangeFolders.Name = "toolStripRearrangeFolders";
            toolStripRearrangeFolders.RenderMode = ToolStripRenderMode.System;
            toolStripRearrangeFolders.Size = new Size(33, 88);
            toolStripRearrangeFolders.TabIndex = 0;
            // 
            // SF_Up
            // 
            SF_Up.AutoSize = false;
            SF_Up.DisplayStyle = ToolStripItemDisplayStyle.Image;
            SF_Up.Image = Resources.handup;
            SF_Up.ImageTransparentColor = Color.Magenta;
            SF_Up.Name = "SF_Up";
            SF_Up.Size = new Size(22, 22);
            SF_Up.Tag = "up";
            SF_Up.ToolTipText = "Move Item Up";
            SF_Up.MouseUp += SF_UpDown_MouseUp;
            // 
            // SF_Down
            // 
            SF_Down.AutoSize = false;
            SF_Down.DisplayStyle = ToolStripItemDisplayStyle.Image;
            SF_Down.Image = Resources.handdown;
            SF_Down.ImageTransparentColor = Color.Magenta;
            SF_Down.Name = "SF_Down";
            SF_Down.Size = new Size(22, 22);
            SF_Down.Tag = "down";
            SF_Down.ToolTipText = "Move Item Down";
            SF_Down.MouseUp += SF_UpDown_MouseUp;
            // 
            // panel1
            // 
            panel1.Controls.Add(toolStripRearrangeFolders);
            panel1.Location = new Point(227, 28);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(33, 82);
            panel1.TabIndex = 7;
            // 
            // FrmRearrangeFolderPositions
            // 
            AcceptButton = OKBtn;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(391, 358);
            Controls.Add(panel1);
            Controls.Add(CancelBtn);
            Controls.Add(OKBtn);
            Controls.Add(SongFolder);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmRearrangeFolderPositions";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Re-Arrange Song Folders";
            FormClosing += FrmRearrangeFolderPositions_FormClosing;
            Load += FrmRearrangeFolderPositions_Load;
            toolStripRearrangeFolders.ResumeLayout(false);
            toolStripRearrangeFolders.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        public FrmRearrangeFolderPositions()
		{
			InitializeComponent();
		}

		private void FrmRearrangeFolderPositions_Load(object sender, EventArgs e)
		{
			BuildFolderList();
		}

		private void BuildFolderList()
		{
			ListViewItem listViewItem = new ListViewItem();
			SongFolder.Items.Clear();
			for (int i = 1; i < gf.MAXSONGSFOLDERS; i++)
			{
				listViewItem = SongFolder.Items.Add(gf.FolderName[i]);
				listViewItem.SubItems.Add(i.ToString());
			}
		}

		private void SF_UpDown_MouseUp(object sender, MouseEventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string name = toolStripButton.Name;
			if (name == "SF_Up")
			{
				MoveFolderUp();
			}
			else
			{
				MoveFolderDown();
			}
		}

		private void MoveFolderUp()
		{
			int count = SongFolder.Items.Count;
			if (count < 1)
			{
				return;
			}
			int selectedIndex = gf.GetSelectedIndex(SongFolder);
			if (selectedIndex >= 1)
			{
				for (int i = 0; i <= 1; i++)
				{
					string text = SongFolder.Items[selectedIndex].SubItems[i].Text;
					SongFolder.Items[selectedIndex].SubItems[i].Text = SongFolder.Items[selectedIndex - 1].SubItems[i].Text;
					SongFolder.Items[selectedIndex - 1].SubItems[i].Text = text;
				}
				SongFolder.Items[selectedIndex].Selected = false;
				SongFolder.Items[selectedIndex - 1].Selected = true;
				SongFolder.EnsureVisible(selectedIndex - 1);
			}
		}

		private void MoveFolderDown()
		{
			int count = SongFolder.Items.Count;
			if (count <= 1)
			{
				return;
			}
			int selectedIndex = gf.GetSelectedIndex(SongFolder);
			if (!((selectedIndex < 0) | (selectedIndex == count - 1)))
			{
				for (int i = 0; i <= 1; i++)
				{
					string text = SongFolder.Items[selectedIndex].SubItems[i].Text;
					SongFolder.Items[selectedIndex].SubItems[i].Text = SongFolder.Items[selectedIndex + 1].SubItems[i].Text;
					SongFolder.Items[selectedIndex + 1].SubItems[i].Text = text;
				}
				SongFolder.Items[selectedIndex].Selected = false;
				SongFolder.Items[selectedIndex + 1].Selected = true;
				SongFolder.EnsureVisible(selectedIndex + 1);
			}
		}

		private void OKBtn_Click(object sender, EventArgs e)
		{
			bool flag = false;
			Cursor = Cursors.WaitCursor;
			string inName = gf.FolderName[gf.JumpToA];
			string inName2 = gf.FolderName[gf.JumpToB];
			string inName3 = gf.FolderName[gf.JumpToC];
			if (gf.SwapFolderNumbers(SongFolder))
			{
				gf.LoadFolderNamesArray();
				gf.JumpToA = gf.GetFolderNumber(inName);
				gf.JumpToB = gf.GetFolderNumber(inName2);
				gf.JumpToC = gf.GetFolderNumber(inName3);
				base.DialogResult = DialogResult.OK;
			}
			else
			{
				MessageBox.Show("Error encountered whilst re-arranging some of the folders. Please restart EasiSlides and try again.");
				base.DialogResult = DialogResult.Cancel;
			}
			Cursor = Cursors.Default;
			Close();
		}

		private void FrmRearrangeFolderPositions_FormClosing(object sender, FormClosingEventArgs e)
		{
			Cursor = Cursors.Default;
		}
	}
}
