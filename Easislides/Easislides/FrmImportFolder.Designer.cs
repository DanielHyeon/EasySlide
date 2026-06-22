using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
    partial class FrmImportFolder
    {
		private Button BtnCancel;
		private Button BtnOK;
		private ComboBox SongFolder;
		private FolderBrowserDialog folderBrowserDialog1;
		private GroupBox groupBox1;
		private IContainer components = null;
		private Label label1;
		private Label label3;
		private Label label4;
		private Label label5;
		private Panel panel1;
		private ProgressBar ProgressBar1;
		private TextBox tbLocation;
		private ToolStrip toolStrip3;
		private ToolStripButton LocationBtn;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmImportFolder));
            groupBox1 = new GroupBox();
            panel1 = new Panel();
            toolStrip3 = new ToolStrip();
            LocationBtn = new ToolStripButton();
            tbLocation = new TextBox();
            ProgressBar1 = new ProgressBar();
            label1 = new Label();
            SongFolder = new ComboBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            toolStrip3.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(tbLocation);
            groupBox1.Controls.Add(ProgressBar1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(SongFolder);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new Point(13, 18);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(693, 280);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Import all Text/Word Documents from a particular Windows Folder into a EasiSlides Database folder";
            //
            // panel1
            //
            panel1.Controls.Add(toolStrip3);
            panel1.Location = new Point(655, 57);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(31, 35);
            panel1.TabIndex = 67;
            //
            // toolStrip3
            //
            toolStrip3.AutoSize = false;
            toolStrip3.CanOverflow = false;
            toolStrip3.Dock = DockStyle.None;
            toolStrip3.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip3.ImageScalingSize = new Size(20, 20);
            toolStrip3.Items.AddRange(new ToolStripItem[] { LocationBtn });
            toolStrip3.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip3.Location = new Point(0, 0);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.RenderMode = ToolStripRenderMode.System;
            toolStrip3.Size = new Size(33, 46);
            toolStrip3.TabIndex = 0;
            //
            // LocationBtn
            //
            LocationBtn.AutoSize = false;
            LocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LocationBtn.Image = Resources.folder;
            LocationBtn.ImageTransparentColor = Color.Magenta;
            LocationBtn.Name = "LocationBtn";
            LocationBtn.Size = new Size(22, 22);
            LocationBtn.Tag = "";
            LocationBtn.ToolTipText = "Media File Location";
            LocationBtn.Click += LocationBtn_Click;
            //
            // tbLocation
            //
            tbLocation.Location = new Point(12, 62);
            tbLocation.Margin = new Padding(4, 5, 4, 5);
            tbLocation.Name = "tbLocation";
            tbLocation.Size = new Size(633, 27);
            tbLocation.TabIndex = 66;
            //
            // ProgressBar1
            //
            ProgressBar1.Location = new Point(12, 203);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(673, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 58;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(8, 25);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(0, 20);
            label1.TabIndex = 52;
            //
            // SongFolder
            //
            SongFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            SongFolder.FormattingEnabled = true;
            SongFolder.Location = new Point(12, 131);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.MaxDropDownItems = 12;
            SongFolder.Name = "SongFolder";
            SongFolder.Size = new Size(264, 28);
            SongFolder.TabIndex = 5;
            //
            // label5
            //
            label5.AutoSize = true;
            label5.Location = new Point(8, 178);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(580, 20);
            label5.TabIndex = 56;
            label5.Text = "C. Click Import to start the Import. Each title shall be based on the imported File name.";
            //
            // label4
            //
            label4.AutoSize = true;
            label4.Location = new Point(8, 106);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(552, 20);
            label4.TabIndex = 55;
            label4.Text = "B. Select the destination EasiSlides Folder you wish to store the imported text into:";
            //
            // label3
            //
            label3.AutoSize = true;
            label3.Location = new Point(8, 37);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(587, 20);
            label3.TabIndex = 54;
            label3.Text = "A. Select the source Windows Folder which contains the  documents you wish to import:";
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(600, 308);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 9;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(472, 308);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 8;
            BtnOK.Text = "Import";
            BtnOK.Click += BtnOK_Click;
            //
            // FrmImportFolder
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(723, 369);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmImportFolder";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Import Folder";
            FormClosing += FrmImportFolder_FormClosing;
            Load += FrmImportFolder_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel1.ResumeLayout(false);
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
