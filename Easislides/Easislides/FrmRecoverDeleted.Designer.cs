//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Util;

namespace Easislides
{
    partial class FrmRecoverDeleted
    {
		private Button BtnCancel;
		private Button BtnOK;
		private CheckBox cbTickAll;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private ColumnHeader columnHeader5;
		private IContainer components = null;
		private Label label1;
		private ListView SongsList;
		private ToolTip toolTip1;

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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmRecoverDeleted));
            BtnCancel = new Button();
            BtnOK = new Button();
            SongsList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            cbTickAll = new CheckBox();
            toolTip1 = new ToolTip(components);
            label1 = new Label();
            SuspendLayout();
            //
            // BtnCancel
            //
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(505, 392);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 3;
            BtnCancel.Text = "Close";
            //
            // BtnOK
            //
            BtnOK.Location = new Point(379, 392);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 2;
            BtnOK.Text = "Recover";
            BtnOK.Click += BtnOK_Click;
            //
            // SongsList
            //
            SongsList.CheckBoxes = true;
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            SongsList.FullRowSelect = true;
            SongsList.Location = new Point(16, 40);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new Size(595, 342);
            SongsList.Sorting = SortOrder.Ascending;
            SongsList.TabIndex = 0;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            SongsList.ColumnClick += SongsList_ColumnClick;
            SongsList.ItemChecked += SongsList_ItemChecked;
            //
            // columnHeader1
            //
            columnHeader1.Text = "";
            columnHeader1.Width = 239;
            //
            // columnHeader2
            //
            columnHeader2.Text = "Restore to Folder";
            columnHeader2.Width = 106;
            //
            // columnHeader3
            //
            columnHeader3.Text = "Deleted (Y-M-D)";
            columnHeader3.Width = 97;
            //
            // columnHeader4
            //
            columnHeader4.Text = "Song ID";
            columnHeader4.Width = 0;
            //
            // columnHeader5
            //
            columnHeader5.Text = "FolderNo";
            columnHeader5.Width = 0;
            //
            // cbTickAll
            //
            cbTickAll.AutoSize = true;
            cbTickAll.Location = new Point(28, 392);
            cbTickAll.Margin = new Padding(4, 5, 4, 5);
            cbTickAll.Name = "cbTickAll";
            cbTickAll.Size = new Size(79, 24);
            cbTickAll.TabIndex = 1;
            cbTickAll.Text = "Tick All";
            cbTickAll.ThreeState = true;
            cbTickAll.CheckedChanged += cbTickAll_CheckedChanged;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(25, 12);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(390, 20);
            label1.TabIndex = 4;
            label1.Text = "Tick the items you wish to restore and then click 'Recover':";
            //
            // FrmRecoverDeleted
            //
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(628, 449);
            Controls.Add(label1);
            Controls.Add(cbTickAll);
            Controls.Add(SongsList);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmRecoverDeleted";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Recover Deleted Songs";
            Load += FrmRecoverDeleted_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
