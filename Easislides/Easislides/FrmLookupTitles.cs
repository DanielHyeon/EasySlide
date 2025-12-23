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
	public class FrmLookupTitles : Form
	{
		private const string Reg_LookupIncludeBookRef = "LookupIncludeBookRef";

		private const string Reg_LookupIncludeUserRef = "LookupIncludeUserRef";

		private IContainer components = null;

		private ListView SongsList;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader2;

		private ColumnHeader columnHeader3;

		private ColumnHeader columnHeader4;

		private Label label1;

		private CheckBox cbBookRef;

		private CheckBox cbUserRef;

		private Button BtnCancel;

		private Button BtnOK;

		private ColumnHeader columnHeader5;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLookupTitles));
			SongsList = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			label1 = new System.Windows.Forms.Label();
			cbBookRef = new System.Windows.Forms.CheckBox();
			cbUserRef = new System.Windows.Forms.CheckBox();
			BtnCancel = new System.Windows.Forms.Button();
			BtnOK = new System.Windows.Forms.Button();
			SuspendLayout();
			SongsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5]
			{
				columnHeader1,
				columnHeader2,
				columnHeader3,
				columnHeader4,
				columnHeader5
			});
			SongsList.FullRowSelect = true;
			SongsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			SongsList.Location = new System.Drawing.Point(12, 41);
			SongsList.MultiSelect = false;
			SongsList.Name = "SongsList";
			SongsList.ShowItemToolTips = true;
			SongsList.Size = new System.Drawing.Size(451, 183);
			SongsList.TabIndex = 1;
			SongsList.UseCompatibleStateImageBehavior = false;
			SongsList.View = System.Windows.Forms.View.Details;
			SongsList.DoubleClick += new System.EventHandler(SongsList_DoubleClick);
			columnHeader1.Text = "Song Title";
			columnHeader1.Width = 214;
			columnHeader2.Text = "Song ID";
			columnHeader2.Width = 0;
			columnHeader3.Text = "Folder";
			columnHeader3.Width = 92;
			columnHeader4.Text = "Book Ref";
			columnHeader4.Width = 74;
			columnHeader5.Text = "User Ref";
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(301, 27);
			label1.TabIndex = 0;
			label1.Text = "Song titles listing based on what you have typed under Title2.  Select ONE title from the list and click OK to copy it to Title2.";
			cbBookRef.AutoSize = true;
			cbBookRef.Location = new System.Drawing.Point(13, 230);
			cbBookRef.Name = "cbBookRef";
			cbBookRef.Size = new System.Drawing.Size(154, 17);
			cbBookRef.TabIndex = 2;
			cbBookRef.Text = "Also Copy Book Reference";
			cbUserRef.AutoSize = true;
			cbUserRef.Location = new System.Drawing.Point(13, 245);
			cbUserRef.Name = "cbUserRef";
			cbUserRef.Size = new System.Drawing.Size(151, 17);
			cbUserRef.TabIndex = 3;
			cbUserRef.Text = "Also Copy User Reference";
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(382, 238);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(80, 24);
			BtnCancel.TabIndex = 5;
			BtnCancel.Text = "Cancel";
			BtnOK.Location = new System.Drawing.Point(286, 238);
			BtnOK.Name = "BtnOK";
			BtnOK.Size = new System.Drawing.Size(80, 24);
			BtnOK.TabIndex = 4;
			BtnOK.Text = "OK";
			BtnOK.Click += new System.EventHandler(BtnOK_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(474, 274);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnOK);
			base.Controls.Add(cbUserRef);
			base.Controls.Add(cbBookRef);
			base.Controls.Add(label1);
			base.Controls.Add(SongsList);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmLookupTitles";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Lookup Titles";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLookupTitles_FormClosing);
			base.Load += new System.EventHandler(FrmLookupTitles_Load);
			ResumeLayout(false);
			PerformLayout();
		}

		public FrmLookupTitles()
		{
			InitializeComponent();
		}

		private void FrmLookupTitles_Load(object sender, EventArgs e)
		{
			gf.Lookup_NameBookRef = "";
			gf.Lookup_NameUserRef = "";
			cbBookRef.Checked = ((RegUtil.GetRegValue("options", "LookupIncludeBookRef", 1) > 0) ? true : false);
			cbUserRef.Checked = ((RegUtil.GetRegValue("options", "LookupIncludeUserRef", 0) > 0) ? true : false);
			try
			{
				ListViewItem listViewItem = new ListViewItem();
				string fullSearchString = "select * from SONG where title_1 like \"" + gf.Lookup_NameSelected + "\" and folderno > 0 order by cjk_strokecount";
#if OleDb
				using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach(DataRow dr in datatable.Rows)
					{
						listViewItem = SongsList.Items.Add(DataUtil.ObjToString(dr["Title_1"]));
						listViewItem.SubItems.Add(DataUtil.ObjToString(dr["SongID"]));
						listViewItem.SubItems.Add(gf.FolderName[DataUtil.ObjToInt(dr["FolderNo"])]);
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
			int selectedIndex = gf.GetSelectedIndex(SongsList);
			if (selectedIndex >= 0)
			{
				gf.Lookup_NameSelected = SongsList.Items[selectedIndex].Text;
				if (cbBookRef.Checked)
				{
					gf.Lookup_NameBookRef = SongsList.Items[selectedIndex].SubItems[3].Text;
				}
				if (cbUserRef.Checked)
				{
					gf.Lookup_NameUserRef = SongsList.Items[selectedIndex].SubItems[4].Text;
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
