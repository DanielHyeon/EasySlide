//using NetOffice.DAOApi;
using Easislides.SQLite;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

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
#if DAO
				using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif
				SongsList.Sorting = SortOrder.None;
				if (datatable.Rows.Count > 0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach (DataRow dr in datatable.Rows)
					{
						num = DataUtil.ObjToInt(dr["OldFolder"]);
						if ((num < 0) | (num > 41))
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRecoverDeleted));
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			SongsList = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			cbTickAll = new System.Windows.Forms.CheckBox();
			toolTip1 = new System.Windows.Forms.ToolTip(components);
			label1 = new System.Windows.Forms.Label();
			SuspendLayout();
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(379, 255);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 3;
			BtnCancel.Text = "Close";
			BtnOK.Location = new System.Drawing.Point(284, 255);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 2;
			BtnOK.Text = "Recover";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			SongsList.CheckBoxes = true;
			SongsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5]
			{
				columnHeader1,
				columnHeader2,
				columnHeader3,
				columnHeader4,
				columnHeader5
			});
			SongsList.FullRowSelect = true;
			SongsList.Location = new System.Drawing.Point(12, 26);
			SongsList.Name = "SongsList";
			SongsList.ShowItemToolTips = true;
			SongsList.Size = new System.Drawing.Size(447, 224);
			SongsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			SongsList.TabIndex = 0;
			SongsList.UseCompatibleStateImageBehavior = false;
			SongsList.View = System.Windows.Forms.View.Details;
			SongsList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(SongsList_ItemChecked);
			SongsList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(SongsList_ColumnClick);
			columnHeader1.Text = "";
			columnHeader1.Width = 239;
			columnHeader2.Text = "Restore to Folder";
			columnHeader2.Width = 106;
			columnHeader3.Text = "Deleted (Y-M-D)";
			columnHeader3.Width = 97;
			columnHeader4.Text = "Song ID";
			columnHeader4.Width = 0;
			columnHeader5.Text = "FolderNo";
			columnHeader5.Width = 0;
			cbTickAll.AutoSize = true;
			cbTickAll.Location = new System.Drawing.Point(21, 255);
			cbTickAll.Name = "cbTickAll";
			cbTickAll.Size = new System.Drawing.Size(61, 17);
			cbTickAll.TabIndex = 1;
			cbTickAll.Text = "Tick All";
			cbTickAll.ThreeState = true;
			cbTickAll.CheckedChanged += new System.EventHandler(cbTickAll_CheckedChanged);
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(19, 8);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(285, 13);
			label1.TabIndex = 4;
			label1.Text = "Tick the items you wish to restore and then click 'Recover':";
			base.AcceptButton = BtnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(471, 292);
			base.Controls.Add(label1);
			base.Controls.Add(cbTickAll);
			base.Controls.Add(SongsList);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmRecoverDeleted";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Recover Deleted Songs";
			base.Load += new System.EventHandler(FrmRecoverDeleted_Load);
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
