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
	public class FrmRecoverDeleted : Form
	{
		private int sortColumn = -1;

		private bool InitListItems = true;

		private IContainer components = null;

		private Button BtnCancel;

		private Button BtnOK;

		private ListView SongsList;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader2;

		private ColumnHeader columnHeader3;

		private CheckBox cbTickAll;

		private ColumnHeader columnHeader4;

		private ToolTip toolTip1;

		private Label label1;

		private ColumnHeader columnHeader5;

		public FrmRecoverDeleted()
		{
			InitializeComponent();
		}

		private void FrmRecoverDeleted_Load(object sender, EventArgs e)
		{
			try
			{
				InitListItems = true;
				int num = 0;
				ListViewItem listViewItem = new ListViewItem();
				string fullSearchString = "select * from SONG where FolderNo=" + 0 + " order by LastModified";

				using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);

				SongsList.Sorting = SortOrder.None;
				if (datatable.Rows.Count > 0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach (DataRow dr in datatable.Rows)
					{
						num = DataUtil.ObjToInt(dr["OldFolder"]);
						if ((num < 0) | (num > gf.MAXSONGSFOLDERS))
						{
							num = 1;
						}
						DateTime dateTime = DataUtil.ObjToDate(dr["LastModified"]);
						listViewItem = SongsList.Items.Add(DataUtil.ObjToString(dr["Title_1"]));
						listViewItem.SubItems.Add(gf.FolderName[num]);
						listViewItem.SubItems.Add(dateTime.ToString("yyyy-MM-dd"));
						listViewItem.SubItems.Add(DataUtil.ObjToString(dr["SongID"]));
						listViewItem.SubItems.Add(num.ToString());
					}
				}

				SongsList.Sorting = SortOrder.Ascending;
				SongsList.Sort();
				SongsList.Sorting = SortOrder.None;
			}
			catch
			{
			}
			InitListItems = false;
			SetButtons();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (SongsList.CheckedItems.Count <= 0)
			{
				return;
			}
			Cursor = Cursors.WaitCursor;
			if (MessageBox.Show("Recover the Ticked song(s) to their Original Folders?", "Recover Song(s)", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (gf.ReFileSelectedSongs(ref SongsList) == 0)
				{
					Cursor = Cursors.Default;
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show("Not all the ticked songs were recovered, please try again");
					SetButtons();
				}
			}
			else
			{
				SetButtons();
			}
			Cursor = Cursors.Default;
		}

		private void SetButtons()
		{
			if (SongsList.Items.Count == 0)
			{
				return;
			}
			InitListItems = true;
			if (cbTickAll.CheckState == CheckState.Checked)
			{
				for (int num = SongsList.Items.Count - 1; num >= 0; num--)
				{
					SongsList.Items[num].Checked = true;
				}
			}
			else if (cbTickAll.CheckState == CheckState.Unchecked)
			{
				for (int num = SongsList.Items.Count - 1; num >= 0; num--)
				{
					SongsList.Items[num].Checked = false;
				}
			}
			SongsList.Columns[0].Text = SongsList.Items.Count + " items listed / " + SongsList.CheckedItems.Count + " ticked for recovery.";
			BtnOK.Enabled = ((SongsList.CheckedItems.Count > 0) ? true : false);
			InitListItems = false;
		}

		private void SongsList_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (!InitListItems)
			{
				SongsListItemTicked();
			}
		}

		private void SongsListItemTicked()
		{
			if (SongsList.Items.Count == SongsList.CheckedItems.Count)
			{
				cbTickAll.CheckState = CheckState.Checked;
			}
			else if (SongsList.CheckedItems.Count == 0)
			{
				cbTickAll.CheckState = CheckState.Unchecked;
			}
			else
			{
				cbTickAll.CheckState = CheckState.Indeterminate;
			}
			SetButtons();
		}

		private void cbTickAll_CheckedChanged(object sender, EventArgs e)
		{
			SetButtons();
		}

		private void SongsList_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			lv.Sort(ref SongsList, ref sortColumn, e.Column, FlipSort: true);
		}

		private void oldSongsList_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (e.Column != sortColumn)
			{
				sortColumn = e.Column;
				SongsList.Sorting = SortOrder.Ascending;
			}
			else if (SongsList.Sorting == SortOrder.Ascending)
			{
				SongsList.Sorting = SortOrder.Descending;
			}
			else
			{
				SongsList.Sorting = SortOrder.Ascending;
			}
			SongsList.Sort();
			SongsList.ListViewItemSorter = new ListViewItemComparer(e.Column, SongsList.Sorting);
		}

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
    }
}
