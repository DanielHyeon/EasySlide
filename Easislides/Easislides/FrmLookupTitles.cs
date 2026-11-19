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
	public partial class FrmLookupTitles : Form
	{
		private const string Reg_LookupIncludeBookRef = "LookupIncludeBookRef";

		private const string Reg_LookupIncludeUserRef = "LookupIncludeUserRef";

		public FrmLookupTitles()
		{
			InitializeComponent();
		}

		private void FrmLookupTitles_Load(object sender, EventArgs e)
		{
			Gf.Lookup_NameBookRef = "";
			Gf.Lookup_NameUserRef = "";
			cbBookRef.Checked = ((RegUtil.GetRegValue("options", "LookupIncludeBookRef", 1) > 0) ? true : false);
			cbUserRef.Checked = ((RegUtil.GetRegValue("options", "LookupIncludeUserRef", 0) > 0) ? true : false);
			try
			{
				ListViewItem listViewItem = new ListViewItem();
				using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB,
					"select Title_1, SongID, FolderNo, Book_Reference, User_Reference from SONG where title_1 like @title and folderno > 0 order by cjk_strokecount", ("@title", Gf.Lookup_NameSelected));
				if (datatable.Rows.Count>0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach(DataRow dr in datatable.Rows)
					{
						listViewItem = SongsList.Items.Add(DataUtil.ObjToString(dr["Title_1"]));
						listViewItem.SubItems.Add(DataUtil.ObjToString(dr["SongID"]));
						listViewItem.SubItems.Add(Gf.Folders[DataUtil.ObjToInt(dr["FolderNo"])].Name);
						listViewItem.SubItems.Add(DataUtil.ObjToString(dr["Book_Reference"]));
						listViewItem.SubItems.Add(DataUtil.ObjToString(dr["User_Reference"]));
						//recordSet.MoveNext();
					}
				}
			}
			catch
			{
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			SelectTitle_Click();
		}

		private void SongsList_DoubleClick(object sender, EventArgs e)
		{
			SelectTitle_Click();
		}

		private void SelectTitle_Click()
		{
			if (SongsList.SelectedItems.Count <= 0)
			{
				return;
			}
			int selectedIndex = Gf.GetSelectedIndex(SongsList);
			if (selectedIndex >= 0)
			{
				Gf.Lookup_NameSelected = SongsList.Items[selectedIndex].Text;
				if (cbBookRef.Checked)
				{
					Gf.Lookup_NameBookRef = SongsList.Items[selectedIndex].SubItems[3].Text;
				}
				if (cbUserRef.Checked)
				{
					Gf.Lookup_NameUserRef = SongsList.Items[selectedIndex].SubItems[4].Text;
				}
			}
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void FrmLookupTitles_FormClosing(object sender, FormClosingEventArgs e)
		{
			RegUtil.SaveRegValue("options", "LookupIncludeBookRef", cbBookRef.Checked ? 1 : 0);
			RegUtil.SaveRegValue("options", "LookupIncludeUserRef", cbUserRef.Checked ? 1 : 0);
		}
	}
}
