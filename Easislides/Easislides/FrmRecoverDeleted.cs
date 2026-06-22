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
	public partial class FrmRecoverDeleted : Form
	{
		private int sortColumn = -1;

		private bool InitListItems = true;

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

				using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

				SongsList.Sorting = SortOrder.None;
				if (datatable.Rows.Count > 0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach (DataRow dr in datatable.Rows)
					{
						num = DataUtil.ObjToInt(dr["OldFolder"]);
						if ((num < 0) | (num > Gf.MAXSONGSFOLDERS))
						{
							num = 1;
						}
						DateTime dateTime = DataUtil.ObjToDate(dr["LastModified"]);
						listViewItem = SongsList.Items.Add(DataUtil.ObjToString(dr["Title_1"]));
						listViewItem.SubItems.Add(Gf.FolderName[num]);
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
				if (Gf.ReFileSelectedSongs(ref SongsList) == 0)
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
			Lv.Sort(ref SongsList, ref sortColumn, e.Column, FlipSort: true);
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

		            }
}
