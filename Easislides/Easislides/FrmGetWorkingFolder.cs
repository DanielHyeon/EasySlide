using Easislides.Properties;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmGetWorkingFolder : Form
	{
		private IContainer components = null;

		private Button BtnOK;

		private Label labelMsg;

		private RadioButton OptionExit;

		private RadioButton OptionNewFolder;

		private RadioButton OptionSelectLocation;

		private GroupBox groupBox1;

		private Panel panel1;

		private ToolStrip toolStrip3;

		private ToolStripButton LocationBtn;

		private TextBox tbLocation;

		private RadioButton OptionRestoreOriginalDatabase;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGetWorkingFolder));
			BtnOK = new System.Windows.Forms.Button();
			labelMsg = new System.Windows.Forms.Label();
			OptionExit = new System.Windows.Forms.RadioButton();
			OptionNewFolder = new System.Windows.Forms.RadioButton();
			OptionSelectLocation = new System.Windows.Forms.RadioButton();
			groupBox1 = new System.Windows.Forms.GroupBox();
			OptionRestoreOriginalDatabase = new System.Windows.Forms.RadioButton();
			panel1 = new System.Windows.Forms.Panel();
			toolStrip3 = new System.Windows.Forms.ToolStrip();
			LocationBtn = new System.Windows.Forms.ToolStripButton();
			tbLocation = new System.Windows.Forms.TextBox();
			groupBox1.SuspendLayout();
			panel1.SuspendLayout();
			toolStrip3.SuspendLayout();
			SuspendLayout();
			BtnOK.Location = new System.Drawing.Point(162, 201);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 0;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			labelMsg.Location = new System.Drawing.Point(12, 20);
			labelMsg.Name = "labelMsg";
			labelMsg.Size = new System.Drawing.Size(356, 45);
			labelMsg.TabIndex = 9;
			labelMsg.Text = "The EasiSlides Working Folder at C:\\EasiSlides is missing.  Please select one of the following options and click OK.";
			OptionExit.AutoSize = true;
			OptionExit.Checked = true;
			OptionExit.Location = new System.Drawing.Point(18, 67);
			OptionExit.Name = "OptionExit";
			OptionExit.Size = new System.Drawing.Size(123, 17);
			OptionExit.TabIndex = 0;
			OptionExit.TabStop = true;
			OptionExit.Text = "Exit out of EasiSlides";
			OptionExit.UseVisualStyleBackColor = true;
			OptionNewFolder.AutoSize = true;
			OptionNewFolder.Location = new System.Drawing.Point(18, 90);
			OptionNewFolder.Name = "OptionNewFolder";
			OptionNewFolder.Size = new System.Drawing.Size(238, 17);
			OptionNewFolder.TabIndex = 1;
			OptionNewFolder.Text = "Create the Folder with a new blank Database";
			OptionNewFolder.UseVisualStyleBackColor = true;
			OptionSelectLocation.AutoSize = true;
			OptionSelectLocation.Location = new System.Drawing.Point(18, 136);
			OptionSelectLocation.Name = "OptionSelectLocation";
			OptionSelectLocation.Size = new System.Drawing.Size(144, 17);
			OptionSelectLocation.TabIndex = 3;
			OptionSelectLocation.Text = "Use the Following Folder:";
			OptionSelectLocation.UseVisualStyleBackColor = true;
			groupBox1.Controls.Add(OptionRestoreOriginalDatabase);
			groupBox1.Controls.Add(panel1);
			groupBox1.Controls.Add(tbLocation);
			groupBox1.Controls.Add(OptionSelectLocation);
			groupBox1.Controls.Add(OptionExit);
			groupBox1.Controls.Add(OptionNewFolder);
			groupBox1.Controls.Add(labelMsg);
			groupBox1.Location = new System.Drawing.Point(10, 5);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(377, 190);
			groupBox1.TabIndex = 13;
			groupBox1.TabStop = false;
			groupBox1.Text = "Working Folder Options";
			OptionRestoreOriginalDatabase.AutoSize = true;
			OptionRestoreOriginalDatabase.Location = new System.Drawing.Point(18, 113);
			OptionRestoreOriginalDatabase.Name = "OptionRestoreOriginalDatabase";
			OptionRestoreOriginalDatabase.Size = new System.Drawing.Size(314, 17);
			OptionRestoreOriginalDatabase.TabIndex = 2;
			OptionRestoreOriginalDatabase.Text = "Create the Folder and restore the originally supplied Database";
			OptionRestoreOriginalDatabase.UseVisualStyleBackColor = true;
			panel1.Controls.Add(toolStrip3);
			panel1.Location = new System.Drawing.Point(343, 157);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(23, 23);
			panel1.TabIndex = 65;
			toolStrip3.AutoSize = false;
			toolStrip3.CanOverflow = false;
			toolStrip3.Dock = System.Windows.Forms.DockStyle.None;
			toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				LocationBtn
			});
			toolStrip3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			toolStrip3.Location = new System.Drawing.Point(1, 0);
			toolStrip3.Name = "toolStrip3";
			toolStrip3.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStrip3.Size = new System.Drawing.Size(25, 30);
			toolStrip3.TabIndex = 0;
			LocationBtn.AutoSize = false;
			LocationBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			LocationBtn.Image = Resources.folder;
			LocationBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
			LocationBtn.Name = "LocationBtn";
			LocationBtn.Size = new System.Drawing.Size(22, 22);
			LocationBtn.Tag = "";
			LocationBtn.ToolTipText = "Media File Location";
			LocationBtn.Click += new System.EventHandler(LocationBtn_Click);
			tbLocation.Location = new System.Drawing.Point(18, 159);
			tbLocation.Name = "tbLocation";
			tbLocation.Size = new System.Drawing.Size(323, 20);
			tbLocation.TabIndex = 4;
			tbLocation.TextChanged += new System.EventHandler(tbLocation_TextChanged);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(398, 237);
			base.Controls.Add(groupBox1);
			base.Controls.Add(BtnOK);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmGetWorkingFolder";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Loading EasiSlides ... Working Folder Missing!";
			base.TopMost = true;
			base.Load += new System.EventHandler(FrmGetWorkingFolder_Load);
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			panel1.ResumeLayout(false);
			toolStrip3.ResumeLayout(false);
			toolStrip3.PerformLayout();
			ResumeLayout(false);
		}

		public FrmGetWorkingFolder()
		{
			InitializeComponent();
		}

		private void FrmGetWorkingFolder_Load(object sender, EventArgs e)
		{
			labelMsg.Text = "The EasiSlides Working Folder at " + gf.RootEasiSlidesDir + " is missing. Please select one of the following options and click OK.";
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (OptionExit.Checked)
			{
				base.DialogResult = DialogResult.Cancel;
				Close();
				return;
			}
			if (OptionNewFolder.Checked)
			{
				if (CreateFolder(gf.RootEasiSlidesDir))
				{
					base.DialogResult = DialogResult.OK;
					Close();
				}
				return;
			}
			if (OptionRestoreOriginalDatabase.Checked)
			{
				if (CreateFolder(gf.RootEasiSlidesDir))
				{
					gf.RestoreSongsDatabase = true;
					base.DialogResult = DialogResult.OK;
					Close();
				}
				return;
			}
			string text = tbLocation.Text.Trim();
			if (text == "")
			{
				MessageBox.Show("Please select a valid folder location.");
				return;
			}
			if (DataUtil.Right(text, 1) != "\\")
			{
				text += "\\";
			}
			if (Directory.Exists(text))
			{
				gf.RootEasiSlidesDir = text;
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else if (MessageBox.Show("Folder " + text + " doesn't exist, do you want EasiSlides to create it?", "Create Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (CreateFolder(text))
				{
					gf.RootEasiSlidesDir = text;
					base.DialogResult = DialogResult.OK;
					Close();
				}
			}
			else
			{
				MessageBox.Show("Folder NOT created as instructed - Please select another option.");
			}
		}

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			string text = "";
			folderBrowserDialog.SelectedPath = "C:\\";
			folderBrowserDialog.Description = "Please select a Folder from below to be the EasiSlides Working Folder.";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				tbLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private bool CreateFolder(string NewLocation)
		{
			if (FileUtil.MakeDir(NewLocation))
			{
				return true;
			}
			MessageBox.Show("Error encountered whilst creating folder: " + gf.RootEasiSlidesDir + ". Make sure have write access to the area and try again");
			return false;
		}

		private void tbLocation_TextChanged(object sender, EventArgs e)
		{
			OptionSelectLocation.Checked = true;
		}
	}
}
