using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
    partial class FrmGetWorkingFolder
    {
		private Button BtnOK;
		private GroupBox groupBox1;
		private IContainer components = null;
		private Label labelMsg;
		private Panel panel1;
		private RadioButton OptionExit;
		private RadioButton OptionNewFolder;
		private RadioButton OptionRestoreOriginalDatabase;
		private RadioButton OptionSelectLocation;
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmGetWorkingFolder));
            BtnOK = new Button();
            labelMsg = new Label();
            OptionExit = new RadioButton();
            OptionNewFolder = new RadioButton();
            OptionSelectLocation = new RadioButton();
            groupBox1 = new GroupBox();
            OptionRestoreOriginalDatabase = new RadioButton();
            panel1 = new Panel();
            toolStrip3 = new ToolStrip();
            LocationBtn = new ToolStripButton();
            tbLocation = new TextBox();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            toolStrip3.SuspendLayout();
            SuspendLayout();
            //
            // BtnOK
            //
            BtnOK.Location = new Point(216, 309);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 0;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            //
            // labelMsg
            //
            labelMsg.Location = new Point(16, 31);
            labelMsg.Margin = new Padding(4, 0, 4, 0);
            labelMsg.Name = "labelMsg";
            labelMsg.Size = new Size(475, 69);
            labelMsg.TabIndex = 9;
            labelMsg.Text = "The EasiSlides Working Folder at C:\\EasiSlides is missing.  Please select one of the following options and click OK.";
            //
            // OptionExit
            //
            OptionExit.AutoSize = true;
            OptionExit.Checked = true;
            OptionExit.Location = new Point(24, 103);
            OptionExit.Margin = new Padding(4, 5, 4, 5);
            OptionExit.Name = "OptionExit";
            OptionExit.Size = new Size(167, 24);
            OptionExit.TabIndex = 0;
            OptionExit.TabStop = true;
            OptionExit.Text = "Exit out of EasiSlides";
            OptionExit.UseVisualStyleBackColor = true;
            //
            // OptionNewFolder
            //
            OptionNewFolder.AutoSize = true;
            OptionNewFolder.Location = new Point(24, 138);
            OptionNewFolder.Margin = new Padding(4, 5, 4, 5);
            OptionNewFolder.Name = "OptionNewFolder";
            OptionNewFolder.Size = new Size(326, 24);
            OptionNewFolder.TabIndex = 1;
            OptionNewFolder.Text = "Create the Folder with a new blank Database";
            OptionNewFolder.UseVisualStyleBackColor = true;
            //
            // OptionSelectLocation
            //
            OptionSelectLocation.AutoSize = true;
            OptionSelectLocation.Location = new Point(24, 209);
            OptionSelectLocation.Margin = new Padding(4, 5, 4, 5);
            OptionSelectLocation.Name = "OptionSelectLocation";
            OptionSelectLocation.Size = new Size(197, 24);
            OptionSelectLocation.TabIndex = 3;
            OptionSelectLocation.Text = "Use the Following Folder:";
            OptionSelectLocation.UseVisualStyleBackColor = true;
            //
            // groupBox1
            //
            groupBox1.Controls.Add(OptionRestoreOriginalDatabase);
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(tbLocation);
            groupBox1.Controls.Add(OptionSelectLocation);
            groupBox1.Controls.Add(OptionExit);
            groupBox1.Controls.Add(OptionNewFolder);
            groupBox1.Controls.Add(labelMsg);
            groupBox1.Location = new Point(13, 8);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(503, 292);
            groupBox1.TabIndex = 13;
            groupBox1.TabStop = false;
            groupBox1.Text = "Working Folder Options";
            //
            // OptionRestoreOriginalDatabase
            //
            OptionRestoreOriginalDatabase.AutoSize = true;
            OptionRestoreOriginalDatabase.Location = new Point(24, 174);
            OptionRestoreOriginalDatabase.Margin = new Padding(4, 5, 4, 5);
            OptionRestoreOriginalDatabase.Name = "OptionRestoreOriginalDatabase";
            OptionRestoreOriginalDatabase.Size = new Size(442, 24);
            OptionRestoreOriginalDatabase.TabIndex = 2;
            OptionRestoreOriginalDatabase.Text = "Create the Folder and restore the originally supplied Database";
            OptionRestoreOriginalDatabase.UseVisualStyleBackColor = true;
            //
            // panel1
            //
            panel1.Controls.Add(toolStrip3);
            panel1.Location = new Point(457, 242);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(31, 35);
            panel1.TabIndex = 65;
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
            toolStrip3.Location = new Point(1, 0);
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
            tbLocation.Location = new Point(24, 245);
            tbLocation.Margin = new Padding(4, 5, 4, 5);
            tbLocation.Name = "tbLocation";
            tbLocation.Size = new Size(429, 27);
            tbLocation.TabIndex = 4;
            tbLocation.TextChanged += tbLocation_TextChanged;
            //
            // FrmGetWorkingFolder
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(531, 365);
            Controls.Add(groupBox1);
            Controls.Add(BtnOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmGetWorkingFolder";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Loading EasiSlides ... Working Folder Missing!";
            TopMost = true;
            Load += FrmGetWorkingFolder_Load;
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
