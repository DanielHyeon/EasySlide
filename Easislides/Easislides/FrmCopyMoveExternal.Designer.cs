using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    partial class FrmCopyMoveExternal
    {
		private Button BtnCancel;
		private Button BtnOK;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private IContainer components = null;
		private Label Label1;
		private ListView ExternalFilesFolder;
		private ListView SongFolder;
		private RadioButton optCopyToFolder;
		private RadioButton optCopyToInfoScreen;

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

        #endregion
    }
}
