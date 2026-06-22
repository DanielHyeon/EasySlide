//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
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
    public partial class FrmExport : Form
	{

		private string SongID1;

		private string tempSequence;

		private string SongTitle;

		private int TotSongsSel;

		private bool GroupCheck = false;

		private SongSettings ExportItem = new SongSettings();

		private string Preferred_Ext = "";

		private bool GeneratingList = false;

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
			tbExportTo.Text = ((Gf.ExportFileName != "") ? Gf.ExportFileName : GetLowestFileNum(Gf.DocumentsDir + "Export_" + DateTime.Today.ToString("yyyy-MM-dd"), Preferred_Ext));
		}

		private void BuildFolderList()
		{
			FolderList.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (Gf.FolderUse[i] > 0)
				{
					FolderList.Items.Add(Gf.FolderName[i]);
				}
			}
			if (FolderList.Items.Count == 0)
			{
				FolderList.Items.Add(Gf.FolderName[1]);
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
					text3 = ((!(text3 == "")) ? (text3 + " or FolderNo=" + Convert.ToString(Gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()))) : ("select * from SONG where (FolderNo=" + Convert.ToString(Gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()))));
				}
				text3 = text3 + ") " + str + " order by cjk_strokecount";
				//int num = 0;
				int num2 = 0;
				try
				{
					Cursor = Cursors.WaitCursor;

					using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, text3);

					if (datatable.Rows.Count > 0)
					{
						foreach (DataRow dr in datatable.Rows)
						{
							SongID1 = DataUtil.ObjToString(dr["SongID"]);
							SongTitle = DataUtil.ObjToString(dr["Title_1"]);
							num2 = DataUtil.ObjToInt(dr["FolderNo"]);
							listViewItem = SongsList.Items.Add(SongTitle);
							listViewItem.SubItems.Add(SongID1);
							listViewItem.SubItems.Add(Gf.FolderName[num2]);
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
				for (int num2 = 1; num2 < 100; num2++)
				{
					if (!File.Exists(InFileName + "_" + num2.ToString("00") + InExt))
					{
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
			saveFileDialog1.InitialDirectory = Gf.ExportToDir;
			string InFileName = tbExportTo.Text;
			saveFileDialog1.FileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: false);
			saveFileDialog1.OverwritePrompt = false;
			saveFileDialog1.DefaultExt = Preferred_Ext;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string extension = Path.GetExtension(saveFileDialog1.FileName);
				string str = DataUtil.Left(saveFileDialog1.FileName, saveFileDialog1.FileName.Length - extension.Length);
				tbExportTo.Text = str + extension;
				Gf.ExportFileName = tbExportTo.Text;
				Gf.ExportToDir = Path.GetDirectoryName(tbExportTo.Text) + "\\";
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

		private void Export_DatabaseFormat(string ExportFileName)
		{
			int num = 0;
			string text = Application.StartupPath + "\\Sys\\Defdb.dat";
			if (File.Exists(text))
			{
				Gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
				File.Copy(text, ExportFileName, overwrite: true);
				List<ListViewItem> checkedItems = GetCheckedSongItems();
				using DataTable dt = GetSongsForExport(checkedItems);
				Dictionary<int, DataRow> rowMap = BuildSongRowMap(dt);
				//tableRecordSet.Index = "PrimaryKey";
				Cursor = Cursors.WaitCursor;
				ProgressBar1.Visible = true;
				ProgressBar1.Value = 0;
				int num3 = 0;
				if (Gf.DeleteAllFolders(Gf.ConnectStringDef + ExportFileName))
				{
					for (int i = 0; i < FolderList.CheckedItems.Count; i++)
					{
						Gf.ResetFolder(Gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()), FolderList.CheckedItems[i].ToString(), Gf.ConnectSQLiteDef + ExportFileName);
					}
					for (int i = 0; i < checkedItems.Count; i++)
					{
						int songId = DataUtil.StringToInt(checkedItems[i].SubItems[1].Text);
						if (rowMap.TryGetValue(songId, out DataRow row) && Gf.LoadDataIntoItem(ref ExportItem, row))
						{
							num3 = Gf.InsertItemIntoDatabase(Gf.ConnectStringDef + ExportFileName, ExportItem);
						}
						Update();
						num = (i + 1) * 100 / TotSongsSel;
						ProgressBar1.Value = ((num > 100) ? 100 : num);
						ProgressBar1.Invalidate();
						if (num3 < 1)
						{
							i = checkedItems.Count;
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

		private void Export_TextFormat(string ExportFileName)
		{
			int num = 0;
			Gf.ValidateDir(Path.GetDirectoryName(ExportFileName), CreateDir: true);
			int num2 = 0;
			ProgressBar1.Visible = true;
			ProgressBar1.Value = 0;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[est3.1]");
			List<ListViewItem> checkedItems = GetCheckedSongItems();
			using DataTable dt = GetSongsForExport(checkedItems);
			Dictionary<int, DataRow> rowMap = BuildSongRowMap(dt);
			//tableRecordSet.Index = "PrimaryKey";
			Cursor = Cursors.WaitCursor;
			for (int i = 0; i < checkedItems.Count; i++)
			{
				int songId = DataUtil.StringToInt(checkedItems[i].SubItems[1].Text);
				if (!rowMap.TryGetValue(songId, out DataRow row) || !Gf.LoadDataIntoItem(ref ExportItem, row))
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
					stringBuilder.Append(">f" + Gf.FolderName[ExportItem.FolderNo]);
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
							tempSequence += Gf.SequenceSymbol[num4];
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
				num2 = (i + 1) * 100 / TotSongsSel;
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

		private void Export_XMLFormat(string ExportFileName)
		{
			Gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
			List<ListViewItem> checkedItems = GetCheckedSongItems();
			using DataTable dt = GetSongsForExport(checkedItems);
			Dictionary<int, DataRow> rowMap = BuildSongRowMap(dt);
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
				for (int i = 0; i < checkedItems.Count; i++)
				{
					int songId = DataUtil.StringToInt(checkedItems[i].SubItems[1].Text);
					if (rowMap.TryGetValue(songId, out DataRow row))
					{
						if (Gf.LoadDataIntoItem(ref ExportItem, row))
						{
							Gf.WriteXMLOneItem(ref xtw, ExportItem, null, ReloadImageData: false);
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

		private List<ListViewItem> GetCheckedSongItems()
		{
			List<ListViewItem> items = new List<ListViewItem>(SongsList.CheckedItems.Count);
			foreach (ListViewItem item in SongsList.Items)
			{
				if (item.Checked)
				{
					items.Add(item);
				}
			}
			return items;
		}

		private DataTable GetSongsForExport(List<ListViewItem> checkedItems)
		{
			if (checkedItems.Count == 0)
			{
				return new DataTable();
			}
			List<int> ids = new List<int>(checkedItems.Count);
			for (int i = 0; i < checkedItems.Count; i++)
			{
				int id = DataUtil.StringToInt(checkedItems[i].SubItems[1].Text);
				if (id > 0)
				{
					ids.Add(id);
				}
			}
			if (ids.Count == 0)
			{
				return new DataTable();
			}
			string query = "select * from SONG where SONGID in (" + string.Join(",", ids) + ")";
			return DbController.GetDataTable(Gf.ConnectStringMainDB, query);
		}

		private Dictionary<int, DataRow> BuildSongRowMap(DataTable dt)
		{
			Dictionary<int, DataRow> map = new Dictionary<int, DataRow>(dt?.Rows.Count ?? 0);
			if (dt == null)
			{
				return map;
			}
			foreach (DataRow row in dt.Rows)
			{
				int id = DataUtil.ObjToInt(row["SongID"]);
				if (id > 0)
				{
					map[id] = row;
				}
			}
			return map;
		}

	}
}
