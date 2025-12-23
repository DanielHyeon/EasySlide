using Easislides.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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

		private int[] OriginalFolderPosition = new int[41];

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRearrangeFolderPositions));
			SongFolder = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			OKBtn = new System.Windows.Forms.Button();
			CancelBtn = new System.Windows.Forms.Button();
			toolStripRearrangeFolders = new System.Windows.Forms.ToolStrip();
			SF_Up = new System.Windows.Forms.ToolStripButton();
			SF_Down = new System.Windows.Forms.ToolStripButton();
			panel1 = new System.Windows.Forms.Panel();
			toolStripRearrangeFolders.SuspendLayout();
			panel1.SuspendLayout();
			SuspendLayout();
			SongFolder.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2]
			{
				columnHeader1,
				columnHeader2
			});
			SongFolder.FullRowSelect = true;
			SongFolder.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			SongFolder.HideSelection = false;
			SongFolder.Location = new System.Drawing.Point(12, 12);
			SongFolder.Name = "SongFolder";
			SongFolder.ShowGroups = false;
			SongFolder.ShowItemToolTips = true;
			SongFolder.Size = new System.Drawing.Size(156, 209);
			SongFolder.TabIndex = 0;
			SongFolder.UseCompatibleStateImageBehavior = false;
			SongFolder.View = System.Windows.Forms.View.Details;
			columnHeader1.Text = "";
			columnHeader1.Width = 120;
			columnHeader2.Width = 0;
			OKBtn.Location = new System.Drawing.Point(199, 21);
			OKBtn.Name = "OKBtn";
			OKBtn.Size = new System.Drawing.Size(80, 24);
			OKBtn.TabIndex = 1;
			OKBtn.Text = "Apply";
			OKBtn.Click += new System.EventHandler(OKBtn_Click);
			CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			CancelBtn.Location = new System.Drawing.Point(199, 51);
			CancelBtn.Name = "CancelBtn";
			CancelBtn.Size = new System.Drawing.Size(80, 24);
			CancelBtn.TabIndex = 2;
			CancelBtn.Text = "Cancel";
			toolStripRearrangeFolders.AutoSize = false;
			toolStripRearrangeFolders.CanOverflow = false;
			toolStripRearrangeFolders.Dock = System.Windows.Forms.DockStyle.None;
			toolStripRearrangeFolders.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripRearrangeFolders.Items.AddRange(new System.Windows.Forms.ToolStripItem[2]
			{
				SF_Up,
				SF_Down
			});
			toolStripRearrangeFolders.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripRearrangeFolders.Location = new System.Drawing.Point(0, 0);
			toolStripRearrangeFolders.Name = "toolStripRearrangeFolders";
			toolStripRearrangeFolders.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripRearrangeFolders.Size = new System.Drawing.Size(25, 57);
			toolStripRearrangeFolders.TabIndex = 0;
			SF_Up.AutoSize = false;
			SF_Up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			SF_Up.Image = Resources.handup;
			SF_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
			SF_Up.Name = "SF_Up";
			SF_Up.Size = new System.Drawing.Size(22, 22);
			SF_Up.Tag = "up";
			SF_Up.ToolTipText = "Move Item Up";
			SF_Up.MouseUp += new System.Windows.Forms.MouseEventHandler(SF_UpDown_MouseUp);
			SF_Down.AutoSize = false;
			SF_Down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			SF_Down.Image = Resources.handdown;
			SF_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
			SF_Down.Name = "SF_Down";
			SF_Down.Size = new System.Drawing.Size(22, 22);
			SF_Down.Tag = "down";
			SF_Down.ToolTipText = "Move Item Down";
			SF_Down.MouseUp += new System.Windows.Forms.MouseEventHandler(SF_UpDown_MouseUp);
			panel1.Controls.Add(toolStripRearrangeFolders);
			panel1.Location = new System.Drawing.Point(170, 18);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(25, 53);
			panel1.TabIndex = 7;
			base.AcceptButton = OKBtn;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(293, 233);
			base.Controls.Add(panel1);
			base.Controls.Add(CancelBtn);
			base.Controls.Add(OKBtn);
			base.Controls.Add(SongFolder);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmRearrangeFolderPositions";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Re-Arrange Song Folders";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmRearrangeFolderPositions_FormClosing);
			base.Load += new System.EventHandler(FrmRearrangeFolderPositions_Load);
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
			for (int i = 1; i < 41; i++)
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
