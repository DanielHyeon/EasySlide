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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmLookupTitles));
            SongsList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            label1 = new Label();
            cbBookRef = new CheckBox();
            cbUserRef = new CheckBox();
            BtnCancel = new Button();
            BtnOK = new Button();
            SuspendLayout();
            // 
            // SongsList
            // 
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            SongsList.Location = new Point(16, 63);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.MultiSelect = false;
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new Size(600, 279);
            SongsList.TabIndex = 1;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            SongsList.DoubleClick += SongsList_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Song Title";
            columnHeader1.Width = 214;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Song ID";
            columnHeader2.Width = 0;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Folder";
            columnHeader3.Width = 92;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Book Ref";
            columnHeader4.Width = 74;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "User Ref";
            // 
            // label1
            // 
            label1.Location = new Point(16, 14);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(401, 42);
            label1.TabIndex = 0;
            label1.Text = "Song titles listing based on what you have typed under Title2.  Select ONE title from the list and click OK to copy it to Title2.";
            // 
            // cbBookRef
            // 
            cbBookRef.AutoSize = true;
            cbBookRef.Location = new Point(17, 354);
            cbBookRef.Margin = new Padding(4, 5, 4, 5);
            cbBookRef.Name = "cbBookRef";
            cbBookRef.Size = new Size(206, 24);
            cbBookRef.TabIndex = 2;
            cbBookRef.Text = "Also Copy Book Reference";
            // 
            // cbUserRef
            // 
            cbUserRef.AutoSize = true;
            cbUserRef.Location = new Point(17, 377);
            cbUserRef.Margin = new Padding(4, 5, 4, 5);
            cbUserRef.Name = "cbUserRef";
            cbUserRef.Size = new Size(201, 24);
            cbUserRef.TabIndex = 3;
            cbUserRef.Text = "Also Copy User Reference";
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(509, 366);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "Cancel";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(381, 366);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 4;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            // 
            // FrmLookupTitles
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(632, 422);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(cbUserRef);
            Controls.Add(cbBookRef);
            Controls.Add(label1);
            Controls.Add(SongsList);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmLookupTitles";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Lookup Titles";
            FormClosing += FrmLookupTitles_FormClosing;
            Load += FrmLookupTitles_Load;
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
