//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using Easislides.Properties;
//using Microsoft.Office.Interop.Access.Dao;
using Easislides.SQLite;
using Easislides.Util;

namespace Easislides
{
    public class FrmExport : Form
	{
		private IContainer components = null;

		private CheckedListBox FolderList;

		private Button BtnCancel;

		private Button BtnOK;

		private CheckBox cbSongsListTickAll;

		private Label tbExportTo;

		private ProgressBar ProgressBar1;

		private GroupBox groupBox5;

		private RadioButton OptExport1;

		private RadioButton OptExport0;

		private Panel panelLinkTitle2Lookup;

		private ToolStrip toolStrip2;

		private ToolStripButton Export_FileName;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private DateTimePicker CalendarFrom;

		private Label label2;

		private DateTimePicker CalendarTo;

		private Label label1;

		private CheckBox cbFolderListTickAll;

		private ListView SongsList;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader4;

		private SaveFileDialog saveFileDialog1;

		private ColumnHeader columnHeader3;

		private string SongID1;

		private string tempSequence;

		private string SongTitle;

		private int TotSongsSel;

		private bool GroupCheck = false;

		private SongSettings ExportItem = new SongSettings();

		private string Preferred_Ext = "";

		private bool GeneratingList = false;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmExport));
            FolderList = new CheckedListBox();
            BtnCancel = new Button();
            BtnOK = new Button();
            cbSongsListTickAll = new CheckBox();
            tbExportTo = new Label();
            ProgressBar1 = new ProgressBar();
            groupBox5 = new GroupBox();
            CalendarTo = new DateTimePicker();
            CalendarFrom = new DateTimePicker();
            OptExport1 = new RadioButton();
            OptExport0 = new RadioButton();
            label2 = new Label();
            label1 = new Label();
            panelLinkTitle2Lookup = new Panel();
            toolStrip2 = new ToolStrip();
            Export_FileName = new ToolStripButton();
            groupBox1 = new GroupBox();
            cbFolderListTickAll = new CheckBox();
            groupBox2 = new GroupBox();
            SongsList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            saveFileDialog1 = new SaveFileDialog();
            groupBox5.SuspendLayout();
            panelLinkTitle2Lookup.SuspendLayout();
            toolStrip2.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // FolderList
            // 
            FolderList.CheckOnClick = true;
            FolderList.FormattingEnabled = true;
            FolderList.Location = new Point(11, 28);
            FolderList.Margin = new Padding(4, 5, 4, 5);
            FolderList.Name = "FolderList";
            FolderList.Size = new Size(244, 158);
            FolderList.TabIndex = 0;
            FolderList.SelectedValueChanged += FolderList_SelectedValueChanged;
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(616, 522);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 6;
            BtnCancel.Text = "Close";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(488, 522);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 5;
            BtnOK.Text = "Export";
            BtnOK.Click += BtnOK_Click;
            // 
            // cbSongsListTickAll
            // 
            cbSongsListTickAll.AutoSize = true;
            cbSongsListTickAll.Location = new Point(19, 411);
            cbSongsListTickAll.Margin = new Padding(4, 5, 4, 5);
            cbSongsListTickAll.Name = "cbSongsListTickAll";
            cbSongsListTickAll.Size = new Size(79, 24);
            cbSongsListTickAll.TabIndex = 1;
            cbSongsListTickAll.Text = "Tick All";
            cbSongsListTickAll.ThreeState = true;
            cbSongsListTickAll.CheckedChanged += cbSongsListTickAll_CheckedChanged;
            // 
            // tbExportTo
            // 
            tbExportTo.BackColor = SystemColors.Control;
            tbExportTo.Location = new Point(25, 486);
            tbExportTo.Margin = new Padding(4, 0, 4, 0);
            tbExportTo.Name = "tbExportTo";
            tbExportTo.Size = new Size(640, 20);
            tbExportTo.TabIndex = 4;
            // 
            // ProgressBar1
            // 
            ProgressBar1.Location = new Point(17, 480);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(660, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 3;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(CalendarTo);
            groupBox5.Controls.Add(CalendarFrom);
            groupBox5.Controls.Add(OptExport1);
            groupBox5.Controls.Add(OptExport0);
            groupBox5.Controls.Add(label2);
            groupBox5.Controls.Add(label1);
            groupBox5.Location = new Point(16, 280);
            groupBox5.Margin = new Padding(4, 5, 4, 5);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 5, 4, 5);
            groupBox5.Size = new Size(264, 185);
            groupBox5.TabIndex = 1;
            groupBox5.TabStop = false;
            groupBox5.Text = "List Items from Selected Folders";
            // 
            // CalendarTo
            // 
            CalendarTo.Location = new Point(51, 140);
            CalendarTo.Margin = new Padding(4, 5, 4, 5);
            CalendarTo.Name = "CalendarTo";
            CalendarTo.Size = new Size(207, 27);
            CalendarTo.TabIndex = 5;
            CalendarTo.ValueChanged += Calendar_ValueChanged;
            // 
            // CalendarFrom
            // 
            CalendarFrom.Location = new Point(51, 98);
            CalendarFrom.Margin = new Padding(4, 5, 4, 5);
            CalendarFrom.Name = "CalendarFrom";
            CalendarFrom.Size = new Size(207, 27);
            CalendarFrom.TabIndex = 3;
            CalendarFrom.ValueChanged += Calendar_ValueChanged;
            // 
            // OptExport1
            // 
            OptExport1.AutoSize = true;
            OptExport1.Location = new Point(11, 63);
            OptExport1.Margin = new Padding(4, 5, 4, 5);
            OptExport1.Name = "OptExport1";
            OptExport1.Size = new Size(182, 24);
            OptExport1.TabIndex = 1;
            OptExport1.Text = "Items Added/Updated:";
            // 
            // OptExport0
            // 
            OptExport0.AutoSize = true;
            OptExport0.Location = new Point(11, 28);
            OptExport0.Margin = new Padding(4, 5, 4, 5);
            OptExport0.Name = "OptExport0";
            OptExport0.Size = new Size(88, 24);
            OptExport0.TabIndex = 0;
            OptExport0.Text = "All Items";
            OptExport0.CheckedChanged += OptExport_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 146);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(28, 20);
            label2.TabIndex = 4;
            label2.Text = "To:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 105);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(46, 20);
            label1.TabIndex = 2;
            label1.Text = "From:";
            // 
            // panelLinkTitle2Lookup
            // 
            panelLinkTitle2Lookup.Controls.Add(toolStrip2);
            panelLinkTitle2Lookup.Location = new Point(685, 478);
            panelLinkTitle2Lookup.Margin = new Padding(4, 5, 4, 5);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(29, 34);
            panelLinkTitle2Lookup.TabIndex = 48;
            // 
            // toolStrip2
            // 
            toolStrip2.AutoSize = false;
            toolStrip2.CanOverflow = false;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { Export_FileName });
            toolStrip2.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.RenderMode = ToolStripRenderMode.System;
            toolStrip2.Size = new Size(33, 43);
            toolStrip2.TabIndex = 0;
            // 
            // Export_FileName
            // 
            Export_FileName.AutoSize = false;
            Export_FileName.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Export_FileName.Image = Resources.folder;
            Export_FileName.ImageTransparentColor = Color.Magenta;
            Export_FileName.Name = "Export_FileName";
            Export_FileName.Size = new Size(22, 22);
            Export_FileName.Tag = "";
            Export_FileName.ToolTipText = "Export file name";
            Export_FileName.Click += Export_FileName_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cbFolderListTickAll);
            groupBox1.Controls.Add(FolderList);
            groupBox1.Location = new Point(16, 18);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(264, 252);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Select Song Folder to Export";
            // 
            // cbFolderListTickAll
            // 
            cbFolderListTickAll.AutoSize = true;
            cbFolderListTickAll.Location = new Point(12, 205);
            cbFolderListTickAll.Margin = new Padding(4, 5, 4, 5);
            cbFolderListTickAll.Name = "cbFolderListTickAll";
            cbFolderListTickAll.Size = new Size(79, 24);
            cbFolderListTickAll.TabIndex = 1;
            cbFolderListTickAll.Text = "Tick All";
            cbFolderListTickAll.ThreeState = true;
            cbFolderListTickAll.CheckedChanged += cbFolderListTickAll_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(SongsList);
            groupBox2.Controls.Add(cbSongsListTickAll);
            groupBox2.Location = new Point(288, 18);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(435, 446);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Items Found:";
            // 
            // SongsList
            // 
            SongsList.CheckBoxes = true;
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader3, columnHeader4 });
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            SongsList.Location = new Point(8, 29);
            SongsList.Margin = new Padding(4, 5, 4, 5);
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new Size(417, 370);
            SongsList.Sorting = SortOrder.Ascending;
            SongsList.TabIndex = 0;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            SongsList.ItemChecked += SongsList_ItemChecked;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Tick the items you wish to Export";
            columnHeader1.Width = 192;
            // 
            // columnHeader3
            // 
            columnHeader3.DisplayIndex = 2;
            columnHeader3.Text = "ID";
            columnHeader3.Width = 0;
            // 
            // columnHeader4
            // 
            columnHeader4.DisplayIndex = 1;
            columnHeader4.Text = "Song Folder";
            columnHeader4.Width = 96;
            // 
            // FrmExport
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(731, 580);
            Controls.Add(tbExportTo);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(panelLinkTitle2Lookup);
            Controls.Add(groupBox5);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(ProgressBar1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmExport";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Export";
            Load += FrmExport_Load;
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            panelLinkTitle2Lookup.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        public FrmExport()
		{
			InitializeComponent();
		}

		private void FrmExport_Load(object sender, EventArgs e)
		{
			OptExport0.Checked = true;
			CalendarFrom.Value = DateTime.Now.Subtract(TimeSpan.FromDays(91.0));
			BuildFolderList();
			ExportItem.Initialise();
			SongsList.Sorting = SortOrder.None;
			Preferred_Ext = ".xml";
			tbExportTo.Text = ((gf.ExportFileName != "") ? gf.ExportFileName : GetLowestFileNum(gf.DocumentsDir + "Export_" + DateTime.Today.ToString("yyyy-MM-dd"), Preferred_Ext));
		}

		private void BuildFolderList()
		{
			FolderList.Items.Clear();
			for (int i = 1; i < 41; i++)
			{
				if (gf.FolderUse[i] > 0)
				{
					FolderList.Items.Add(gf.FolderName[i]);
				}
			}
			if (FolderList.Items.Count == 0)
			{
				FolderList.Items.Add(gf.FolderName[1]);
			}
			FolderList.Text = FolderList.Items[0].ToString();
			SongsList.Items.Clear();
			DisplayCount();
		}

		private string ValidateExportExtension(string InExtension)
		{
			InExtension = InExtension.ToLower();
			return (InExtension == ".esn" || InExtension == ".xml") ? InExtension : ".xml";
		}

		private void CreateExportList()
		{
			GeneratingList = true;
			SongsList.Items.Clear();
			SongsList.Sorting = SortOrder.None;
			if (FolderList.CheckedItems.Count >= 1)
			{
				ListViewItem listViewItem = new ListViewItem();
				string str;
				if (OptExport1.Checked)
				{
					string text = CalendarFrom.Value.ToString("MM-dd-yyyy");
					string text2 = CalendarTo.Value.ToString("MM-dd-yyyy");
					str = " and LastModified >=#" + text + "# and LastModified <=#" + text2 + "#";
				}
				else
				{
					str = "";
				}
				string text3 = "";
				for (int i = 0; i < FolderList.CheckedItems.Count; i++)
				{
					text3 = ((!(text3 == "")) ? (text3 + " or FolderNo=" + Convert.ToString(gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()))) : ("select * from SONG where (FolderNo=" + Convert.ToString(gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()))));
				}
				text3 = text3 + ") " + str + " order by cjk_strokecount";
				//int num = 0;
				int num2 = 0;
				try
				{
					Cursor = Cursors.WaitCursor;
#if OleDb
					using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, text3);
#elif SQLite
					using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, text3);
#endif
					if (datatable.Rows.Count > 0)
					{
						foreach (DataRow dr in datatable.Rows)
						{
							SongID1 = DataUtil.ObjToString(dr["SongID"]);
							SongTitle = DataUtil.ObjToString(dr["Title_1"]);
							num2 = DataUtil.ObjToInt(dr["FolderNo"]);
							listViewItem = SongsList.Items.Add(SongTitle);
							listViewItem.SubItems.Add(SongID1);
							listViewItem.SubItems.Add(gf.FolderName[num2]);
						}
					}
				}
				catch
				{
				}
				SongsList.Sorting = SortOrder.Ascending;
				SongsList.Sort();
				DisplayCount();
				GeneratingList = false;
				Cursor = Cursors.Default;
			}
		}

		private void DisplayCount()
		{
			if (!GroupCheck)
			{
				int count = SongsList.Items.Count;
				if (count < 1)
				{
					SongsList.Columns[0].Text = "No Items found";
					return;
				}
				int count2 = SongsList.CheckedItems.Count;
				SongsList.Columns[0].Text = " " + Convert.ToString(count) + " items found : " + Convert.ToString(count2) + " ticked.";
			}
		}

		private void FolderList_SelectedValueChanged(object sender, EventArgs e)
		{
			FolderListCheckedItems_Changed();
		}

		private void FolderListCheckedItems_Changed()
		{
			if (FolderList.CheckedItems.Count <= 0)
			{
				SongsList.Items.Clear();
				cbFolderListTickAll.CheckState = CheckState.Unchecked;
				cbSongsListTickAll.CheckState = CheckState.Unchecked;
			}
			CreateExportList();
		}

		private void SongsList_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (!GeneratingList)
			{
				DisplayCount();
			}
		}

		private string GetLowestFileNum(string InFileName, string InExt)
		{
			if (!File.Exists(InFileName + InExt))
			{
				return InFileName + InExt;
			}
			int num = 0;
			for (int num2 = 98; num2 >= 1; num2--)
			{
				if (File.Exists(InFileName + "_" + num2.ToString("00") + InExt))
				{
					num = num2;
					num2 = 0;
				}
			}
			num++;
			if ((num == 99) & !File.Exists(InFileName + "_" + num.ToString("00") + InExt))
			{
				bool flag = false;
				for (int num2 = 1; num2 < 100; num2++)
				{
					if (!File.Exists(InFileName + "_" + num2.ToString("00") + InExt))
					{
						flag = true;
						num = num2;
						num2 = 100;
					}
				}
			}
			return InFileName + "_" + num.ToString("00") + InExt;
		}

		private void cbSongsListTickAll_CheckedChanged(object sender, EventArgs e)
		{
			SetSongsListCheckBoxes();
		}

		private void SetSongsListCheckBoxes()
		{
			if (SongsList.Items.Count <= 0)
			{
				return;
			}
			GeneratingList = true;
			Cursor = Cursors.WaitCursor;
			if (cbSongsListTickAll.CheckState == CheckState.Checked)
			{
				for (int num = SongsList.Items.Count - 1; num >= 0; num--)
				{
					SongsList.Items[num].Checked = true;
				}
			}
			else if (cbSongsListTickAll.CheckState == CheckState.Unchecked)
			{
				for (int num = SongsList.Items.Count - 1; num >= 0; num--)
				{
					SongsList.Items[num].Checked = false;
				}
			}
			DisplayCount();
			GeneratingList = false;
			Cursor = Cursors.Default;
		}

		private void cbFolderListTickAll_CheckedChanged(object sender, EventArgs e)
		{
			SetFolderListCheckBoxes();
		}

		private void SetFolderListCheckBoxes()
		{
			if (FolderList.Items.Count == 0)
			{
				return;
			}
			if (cbFolderListTickAll.CheckState == CheckState.Checked)
			{
				for (int num = FolderList.Items.Count - 1; num >= 0; num--)
				{
					FolderList.SetItemChecked(num, value: true);
				}
			}
			else if (cbFolderListTickAll.CheckState == CheckState.Unchecked)
			{
				for (int num = FolderList.Items.Count - 1; num >= 0; num--)
				{
					FolderList.SetItemChecked(num, value: false);
				}
			}
			FolderListCheckedItems_Changed();
		}

		private void OptExport_CheckedChanged(object sender, EventArgs e)
		{
			CalendarFrom.Enabled = (OptExport1.Checked ? true : false);
			CalendarTo.Enabled = CalendarFrom.Enabled;
			CreateExportList();
		}

		private void Calendar_ValueChanged(object sender, EventArgs e)
		{
			CreateExportList();
		}

		private void Export_FileName_Click(object sender, EventArgs e)
		{
			saveFileDialog1.Filter = "EasiSlides XML File (*.xml)|*.xml|EasiSlides Database File (*.esf)|*.esf|EasiSlides Text File (*.esn)|*.esn";
			saveFileDialog1.InitialDirectory = gf.ExportToDir;
			string InFileName = tbExportTo.Text;
			saveFileDialog1.FileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: false);
			saveFileDialog1.OverwritePrompt = false;
			saveFileDialog1.DefaultExt = Preferred_Ext;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string extension = Path.GetExtension(saveFileDialog1.FileName);
				string str = DataUtil.Left(saveFileDialog1.FileName, saveFileDialog1.FileName.Length - extension.Length);
				tbExportTo.Text = str + extension;
				gf.ExportFileName = tbExportTo.Text;
				gf.ExportToDir = Path.GetDirectoryName(tbExportTo.Text) + "\\";
				string extension2 = Path.GetExtension(tbExportTo.Text);
				Preferred_Ext = ValidateExportExtension(extension2);
				RegUtil.SaveRegValue("settings", "export_ext", Preferred_Ext);
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Start_Export();
		}

		private void Start_Export()
		{
			if (FolderList.CheckedItems.Count == 0)
			{
				MessageBox.Show("You have not selected any folders to export!");
				return;
			}
			if (SongsList.Items.Count < 1)
			{
				MessageBox.Show("No Songs exported - the Songs List is empty!");
				return;
			}
			TotSongsSel = SongsList.CheckedItems.Count;
			if (TotSongsSel < 1)
			{
				MessageBox.Show("Please Tick the songs you wish to export");
				return;
			}
			string text = tbExportTo.Text;
			if (File.Exists(text))
			{
				if (MessageBox.Show("This will overwrite the existing document '" + text + "' - OK to Continue?", "Overwrite", MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					MessageBox.Show("Export file NOT produced.");
					return;
				}
				try
				{
					File.Delete(text);
				}
				catch
				{
					MessageBox.Show("For some reason, Windows could not create the file '" + text + "'. Export NOT done.");
					ProgressBar1.Value = 0;
					return;
				}
			}
			tbExportTo.Visible = false;
			string strExt = Path.GetExtension(text).ToLower();
			switch (strExt)
			{
				case ".esf":
					Export_DatabaseFormat(text);
					break;
				case ".esn":
					Export_TextFormat(text);
					break;
				default:
					Export_XMLFormat(text);
					break;
			}
			tbExportTo.Visible = true;
		}
#if DAO
		private void Export_DatabaseFormat(string ExportFileName)
		{
			int num = 0;
			string text = Application.StartupPath + "\\Sys\\Defdb.dat";
			if (File.Exists(text))
			{
				gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
				File.Copy(text, ExportFileName, overwrite: true);
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				Cursor = Cursors.WaitCursor;
				ProgressBar1.Visible = true;
				ProgressBar1.Value = 0;
				int num2 = 0;
				int num3 = 0;
				if (gf.DeleteAllFolders(gf.ConnectStringDef + ExportFileName))
				{
					for (int i = 0; i < FolderList.CheckedItems.Count; i++)
					{
						gf.ResetFolder(gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()), FolderList.CheckedItems[i].ToString(), gf.ConnectStringDef + ExportFileName);
					}
					for (int i = 0; i < SongsList.Items.Count; i++)
					{
						if (SongsList.Items[i].Checked)
						{
							if (gf.LoadDataIntoItem(ref ExportItem, tableRecordSet, SongsList.Items[i].SubItems[1].Text))
							{
								num3 = gf.InsertItemIntoDatabase(gf.ConnectStringDef + ExportFileName, ExportItem);
							}
							Update();
							num = (i + 1) * 100 / TotSongsSel;
							ProgressBar1.Value = ((num > 100) ? 100 : num);
							ProgressBar1.Invalidate();
							if (num3 < 1)
							{
								i = SongsList.Items.Count;
							}
						}
					}
					Cursor = Cursors.Default;
					if (num3 > 0)
					{
						MessageBox.Show("Export Completed. Total of " + Convert.ToString(SongsList.CheckedItems.Count) + " songs exported to " + ExportFileName);
					}
					else
					{
						MessageBox.Show("Error encountered when exporting to database file'" + ExportFileName + "'. Export NOT completed.");
					}
				}
				else
				{
					MessageBox.Show("Error encountered when trying to create export database file'" + ExportFileName + "'. Export NOT completed.");
				}
				if (tableRecordSet != null)
				{
					tableRecordSet = null;
				}
				ProgressBar1.Value = 0;
				Cursor = Cursors.Default;
			}
			else
			{
				MessageBox.Show("System Error: cannot create export database file. You may need to re-install EasiSlides Software.");
			}
		}

#elif SQLite
		private void Export_DatabaseFormat(string ExportFileName)
		{
			int num = 0;
			string text = Application.StartupPath + "\\Sys\\Defdb.dat";
			if (File.Exists(text))
			{
				gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
				File.Copy(text, ExportFileName, overwrite: true);
				using DataTable dt = DbController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
				//tableRecordSet.Index = "PrimaryKey";
				Cursor = Cursors.WaitCursor;
				ProgressBar1.Visible = true;
				ProgressBar1.Value = 0;
				int num2 = 0;
				int num3 = 0;
				if (gf.DeleteAllFolders(gf.ConnectStringDef + ExportFileName))
				{
					for (int i = 0; i < FolderList.CheckedItems.Count; i++)
					{
#if OleDb
						gf.ResetFolder(gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()), FolderList.CheckedItems[i].ToString(), gf.ConnectStringDef + ExportFileName);
#elif SQLite
						gf.ResetFolder(gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()), FolderList.CheckedItems[i].ToString(), gf.ConnectSQLiteDef + ExportFileName);
#endif
					}
					for (int i = 0; i < SongsList.Items.Count; i++)
					{
						if (SongsList.Items[i].Checked)
						{
							if (gf.LoadDataIntoItem(ref ExportItem, dt, SongsList.Items[i].SubItems[1].Text))
							{
								num3 = gf.InsertItemIntoDatabase(gf.ConnectStringDef + ExportFileName, ExportItem);
							}
							Update();
							num = (i + 1) * 100 / TotSongsSel;
							ProgressBar1.Value = ((num > 100) ? 100 : num);
							ProgressBar1.Invalidate();
							if (num3 < 1)
							{
								i = SongsList.Items.Count;
							}
						}
					}
					Cursor = Cursors.Default;
					if (num3 > 0)
					{
						MessageBox.Show("Export Completed. Total of " + Convert.ToString(SongsList.CheckedItems.Count) + " songs exported to " + ExportFileName);
					}
					else
					{
						MessageBox.Show("Error encountered when exporting to database file'" + ExportFileName + "'. Export NOT completed.");
					}
				}
				else
				{
					MessageBox.Show("Error encountered when trying to create export database file'" + ExportFileName + "'. Export NOT completed.");
				}

				ProgressBar1.Value = 0;
				Cursor = Cursors.Default;
			}
			else
			{
				MessageBox.Show("System Error: cannot create export database file. You may need to re-install EasiSlides Software.");
			}
		}
#endif



#if DAO
		private void Export_TextFormat(string ExportFileName)
		{
			int num = 0;
			gf.ValidateDir(Path.GetDirectoryName(ExportFileName), CreateDir: true);
			int num2 = 0;
			int num3 = 0;
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[est3.1]");
			Recordset tableRecordSet = DbDaoController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
			tableRecordSet.Index = "PrimaryKey";
			Cursor = Cursors.WaitCursor;
			for (int i = 0; i < SongsList.Items.Count; i++)
			{
				if (!SongsList.Items[i].Checked || !gf.LoadDataIntoItem(ref ExportItem, tableRecordSet, SongsList.Items[i].SubItems[1].Text))
				{
					continue;
				}
				stringBuilder.Append("\r\n[>" + ExportItem.Title);
				if (ExportItem.Title2 != "")
				{
					stringBuilder.Append(">>" + ExportItem.Title2);
				}
				if (ExportItem.FolderNo > 0)
				{
					stringBuilder.Append(">f" + gf.FolderName[ExportItem.FolderNo]);
				}
				if (ExportItem.SongNumber > 0)
				{
					stringBuilder.Append(">n" + ExportItem.SongNumber);
				}
				if (ExportItem.Book_Reference != "")
				{
					stringBuilder.Append(">r" + ExportItem.Book_Reference);
				}
				if (ExportItem.User_Reference != "")
				{
					stringBuilder.Append(">u" + ExportItem.User_Reference);
				}
				if (ExportItem.Copyright != "")
				{
					stringBuilder.Append(">c" + ExportItem.Copyright);
				}
				if (ExportItem.Writer != "")
				{
					stringBuilder.Append(">w" + ExportItem.Writer);
				}
				if (ExportItem.MusicKey != "")
				{
					stringBuilder.Append(">k" + ExportItem.MusicKey);
				}
				if (ExportItem.Timing != "")
				{
					stringBuilder.Append(">t" + ExportItem.Timing);
				}
				if (ExportItem.Capo >= 0)
				{
					stringBuilder.Append(">0" + ExportItem.Capo);
				}
				if (ExportItem.Show_LicAdminInfo1 != "")
				{
					stringBuilder.Append(">a" + ExportItem.Show_LicAdminInfo1);
				}
				if (ExportItem.Show_LicAdminInfo2 != "")
				{
					stringBuilder.Append(">b" + ExportItem.Show_LicAdminInfo2);
				}
				if (ExportItem.SongSequence.Length > 0)
				{
					tempSequence = "";
					for (int j = 0; j < ExportItem.SongSequence.Length; j++)
					{
						int num4 = DataUtil.StringToInt(ExportItem.SongSequence[j]);
						if (num4 > 0 && num4 < 13)
						{
							tempSequence += Convert.ToString(num4);
						}
						else
						{
							tempSequence += gf.SequenceSymbol[num4];
						}
						if (j < ExportItem.SongSequence.Length - 1)
						{
							tempSequence += ",";
						}
					}
					stringBuilder.Append(">@" + tempSequence);
				}
				stringBuilder.Append("]");
				if (ExportItem.Notations != "")
				{
					string str = "[~" + ExportItem.Notations + "]";
					stringBuilder.Append("\r\n" + str);
				}
				ExportItem.CompleteLyrics.Replace("\r\n", "\n");
				stringBuilder.Append("\r\n" + ExportItem.CompleteLyrics.Replace("\n", "\r\n"));
				num++;
				Update();
				num2 = num * 100 / TotSongsSel;
				ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
				ProgressBar1.Invalidate();
			}
			tableRecordSet = null;
			if (tableRecordSet != null)
			{
				tableRecordSet = null;
			}
			if (FileUtil.CreateNewFile(ExportFileName, FileUtil.FileContentsType.DoubleByte, stringBuilder.ToString()))
			{
				MessageBox.Show("Export Completed. Total of " + Convert.ToString(num) + " songs exported to " + ExportFileName);
			}
			else
			{
				MessageBox.Show("Error encountered when trying to create export file'" + ExportFileName + "'. Export NOT completed.");
			}
			Cursor = Cursors.Default;
			ProgressBar1.Visible = false;
		}
#elif SQLite
		private void Export_TextFormat(string ExportFileName)
		{
			int num = 0;
			gf.ValidateDir(Path.GetDirectoryName(ExportFileName), CreateDir: true);
			int num2 = 0;
			int num3 = 0;
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[est3.1]");
			using DataTable dt = DbController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
			//tableRecordSet.Index = "PrimaryKey";
			Cursor = Cursors.WaitCursor;
			for (int i = 0; i < SongsList.Items.Count; i++)
			{
				if (!SongsList.Items[i].Checked || !gf.LoadDataIntoItem(ref ExportItem, dt, SongsList.Items[i].SubItems[1].Text))
				{
					continue;
				}
				stringBuilder.Append("\r\n[>" + ExportItem.Title);
				if (ExportItem.Title2 != "")
				{
					stringBuilder.Append(">>" + ExportItem.Title2);
				}
				if (ExportItem.FolderNo > 0)
				{
					stringBuilder.Append(">f" + gf.FolderName[ExportItem.FolderNo]);
				}
				if (ExportItem.SongNumber > 0)
				{
					stringBuilder.Append(">n" + ExportItem.SongNumber);
				}
				if (ExportItem.Book_Reference != "")
				{
					stringBuilder.Append(">r" + ExportItem.Book_Reference);
				}
				if (ExportItem.User_Reference != "")
				{
					stringBuilder.Append(">u" + ExportItem.User_Reference);
				}
				if (ExportItem.Copyright != "")
				{
					stringBuilder.Append(">c" + ExportItem.Copyright);
				}
				if (ExportItem.Writer != "")
				{
					stringBuilder.Append(">w" + ExportItem.Writer);
				}
				if (ExportItem.MusicKey != "")
				{
					stringBuilder.Append(">k" + ExportItem.MusicKey);
				}
				if (ExportItem.Timing != "")
				{
					stringBuilder.Append(">t" + ExportItem.Timing);
				}
				if (ExportItem.Capo >= 0)
				{
					stringBuilder.Append(">0" + ExportItem.Capo);
				}
				if (ExportItem.Show_LicAdminInfo1 != "")
				{
					stringBuilder.Append(">a" + ExportItem.Show_LicAdminInfo1);
				}
				if (ExportItem.Show_LicAdminInfo2 != "")
				{
					stringBuilder.Append(">b" + ExportItem.Show_LicAdminInfo2);
				}
				if (ExportItem.SongSequence.Length > 0)
				{
					tempSequence = "";
					for (int j = 0; j < ExportItem.SongSequence.Length; j++)
					{
						int num4 = DataUtil.StringToInt(ExportItem.SongSequence[j]);
						if (num4 > 0 && num4 < 13)
						{
							tempSequence += Convert.ToString(num4);
						}
						else
						{
							tempSequence += gf.SequenceSymbol[num4];
						}
						if (j < ExportItem.SongSequence.Length - 1)
						{
							tempSequence += ",";
						}
					}
					stringBuilder.Append(">@" + tempSequence);
				}
				stringBuilder.Append("]");
				if (ExportItem.Notations != "")
				{
					string str = "[~" + ExportItem.Notations + "]";
					stringBuilder.Append("\r\n" + str);
				}
				ExportItem.CompleteLyrics.Replace("\r\n", "\n");
				stringBuilder.Append("\r\n" + ExportItem.CompleteLyrics.Replace("\n", "\r\n"));
				num++;
				Update();
				num2 = num * 100 / TotSongsSel;
				ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
				ProgressBar1.Invalidate();
			}

			if (FileUtil.CreateNewFile(ExportFileName, FileUtil.FileContentsType.DoubleByte, stringBuilder.ToString()))
			{
				MessageBox.Show("Export Completed. Total of " + Convert.ToString(num) + " songs exported to " + ExportFileName);
			}
			else
			{
				MessageBox.Show("Error encountered when trying to create export file'" + ExportFileName + "'. Export NOT completed.");
			}
			Cursor = Cursors.Default;
			ProgressBar1.Visible = false;
		}
#endif

#if DAO
		private void Export_XMLFormat(string ExportFileName)
		{
			gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
			Recordset tableRecordSet = DbDaoController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
			tableRecordSet.Index = "PrimaryKey";
			Cursor = Cursors.WaitCursor;
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			int num = 0;
			int num2 = 0;
			try
			{
				XmlTextWriter xtw = new XmlTextWriter(ExportFileName, Encoding.UTF8);
				xtw.Formatting = Formatting.Indented;
				xtw.WriteStartDocument();
				xtw.WriteStartElement("EasiSlides");
				for (int i = 0; i < SongsList.Items.Count; i++)
				{
					if (SongsList.Items[i].Checked)
					{
						if (gf.LoadDataIntoItem(ref ExportItem, tableRecordSet, SongsList.Items[i].SubItems[1].Text))
						{
							gf.WriteXMLOneItem(ref xtw, ExportItem, null, ReloadImageData: false);
							num++;
						}
						Update();
						num2 = (i + 1) * 100 / TotSongsSel;
						ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
						ProgressBar1.Invalidate();
					}
				}
				xtw.WriteEndDocument();
				xtw.Flush();
				xtw.Close();
				Cursor = Cursors.Default;
				MessageBox.Show("Export Completed. Total of " + Convert.ToString(num) + " songs exported to " + ExportFileName);
			}
			catch
			{
				Cursor = Cursors.Default;
				MessageBox.Show("Error encountered when trying to create export file'" + ExportFileName + "'. Export NOT completed.");
			}
			ProgressBar1.Value = 0;
		}
#elif SQLite
		private void Export_XMLFormat(string ExportFileName)
		{
			gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
			using DataTable dt = DbController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
			//tableRecordSet.Index = "PrimaryKey";
			Cursor = Cursors.WaitCursor;
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
            XmlTextWriter xtw = null;
            int num = 0;
			int num2 = 0;
			try
			{
				xtw = new XmlTextWriter(ExportFileName, Encoding.UTF8);
				xtw.Formatting = Formatting.Indented;
				xtw.WriteStartDocument();
				xtw.WriteStartElement("EasiSlides");
				for (int i = 0; i < SongsList.Items.Count; i++)
				{
					if (SongsList.Items[i].Checked)
					{
						if (gf.LoadDataIntoItem(ref ExportItem, dt, SongsList.Items[i].SubItems[1].Text))
						{
							gf.WriteXMLOneItem(ref xtw, ExportItem, null, ReloadImageData: false);
							num++;
						}
						Update();
						num2 = (i + 1) * 100 / TotSongsSel;
						ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
						ProgressBar1.Invalidate();
					}
				}
				xtw.WriteEndDocument();
				xtw.Flush();
				xtw.Dispose();
				Cursor = Cursors.Default;
				MessageBox.Show("Export Completed. Total of " + Convert.ToString(num) + " songs exported to " + ExportFileName);
			}
			catch
			{
				if(xtw!= null)
					xtw.Dispose();

				Cursor = Cursors.Default;
				MessageBox.Show("Error encountered when trying to create export file'" + ExportFileName + "'. Export NOT completed.");
			}
			ProgressBar1.Value = 0;
		}
#endif
	}
}
