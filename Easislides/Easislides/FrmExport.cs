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
				if (Gf.Folders[i].Use > 0)
				{
					FolderList.Items.Add(Gf.Folders[i].Name);
				}
			}
			if (FolderList.Items.Count == 0)
			{
				FolderList.Items.Add(Gf.Folders[1].Name);
			}
			FolderList.Text = FolderList.Items[0].ToString();
			SongsList.Items.Clear();
			DisplayCount();
		}

		private string ValidateExportExtension(string InExtension)
		{
			InExtension = InExtension.ToLower();
			return (InExtension == ".esn" || InExtension == ".json" || InExtension == ".xml") ? InExtension : ".xml";
		}

		private void CreateExportList()
		{
			GeneratingList = true;
			SongsList.Items.Clear();
			SongsList.Sorting = SortOrder.None;
			if (FolderList.CheckedItems.Count >= 1)
			{
				ListViewItem listViewItem = new ListViewItem();
				var folders = new List<int>();
				foreach (object item in FolderList.CheckedItems)
					folders.Add(Gf.GetFolderNumber(item.ToString()));
				var dates = OptExport1.Checked ? Gf.LastModifiedBetween(CalendarFrom.Value, CalendarTo.Value) : null;
				string exportQuery = $"select * from SONG where FolderNo in ({string.Join(",", folders)})"
					+ (dates == null ? "" : " and " + dates.Text) + " order by cjk_strokecount";
				int folderNumber = 0;
				try
				{
					Cursor = Cursors.WaitCursor;

					using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, exportQuery, dates?.Parameters ?? []);

					if (datatable.Rows.Count > 0)
					{
						foreach (DataRow dr in datatable.Rows)
						{
							SongID1 = DataUtil.ObjToString(dr["SongID"]);
							SongTitle = DataUtil.ObjToString(dr["Title_1"]);
							folderNumber = DataUtil.ObjToInt(dr["FolderNo"]);
							listViewItem = SongsList.Items.Add(SongTitle);
							listViewItem.SubItems.Add(SongID1);
							listViewItem.SubItems.Add(Gf.Folders[folderNumber].Name);
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
			int fileSuffix = 0;
			for (int candidateSuffix = 98; candidateSuffix >= 1; candidateSuffix--)
			{
				if (File.Exists(InFileName + "_" + candidateSuffix.ToString("00") + InExt))
				{
					fileSuffix = candidateSuffix;
					candidateSuffix = 0;
				}
			}
			fileSuffix++;
			if ((fileSuffix == 99) & !File.Exists(InFileName + "_" + fileSuffix.ToString("00") + InExt))
			{
				for (int candidateSuffix = 1; candidateSuffix < 100; candidateSuffix++)
				{
					if (!File.Exists(InFileName + "_" + candidateSuffix.ToString("00") + InExt))
					{
						fileSuffix = candidateSuffix;
						candidateSuffix = 100;
					}
				}
			}
			return InFileName + "_" + fileSuffix.ToString("00") + InExt;
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
				for (int songIndex = SongsList.Items.Count - 1; songIndex >= 0; songIndex--)
				{
					SongsList.Items[songIndex].Checked = true;
				}
			}
			else if (cbSongsListTickAll.CheckState == CheckState.Unchecked)
			{
				for (int songIndex = SongsList.Items.Count - 1; songIndex >= 0; songIndex--)
				{
					SongsList.Items[songIndex].Checked = false;
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
				for (int folderIndex = FolderList.Items.Count - 1; folderIndex >= 0; folderIndex--)
				{
					FolderList.SetItemChecked(folderIndex, value: true);
				}
			}
			else if (cbFolderListTickAll.CheckState == CheckState.Unchecked)
			{
				for (int folderIndex = FolderList.Items.Count - 1; folderIndex >= 0; folderIndex--)
				{
					FolderList.SetItemChecked(folderIndex, value: false);
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
			saveFileDialog1.Filter = "EasiSlides XML File (*.xml)|*.xml|EasiSlides Database File (*.esf)|*.esf|EasiSlides Text File (*.esn)|*.esn|EasiSlides JSON File (*.json)|*.json";
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
			string outputPath = tbExportTo.Text;
			if (File.Exists(outputPath))
			{
				if (Gf.IsMainDatabaseFile(outputPath))
				{
					MessageBox.Show("Choose an export file other than the active song database.");
					return;
				}
				if (MessageBox.Show("This will overwrite the existing document '" + outputPath + "' - OK to Continue?", "Overwrite", MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					MessageBox.Show("Export file NOT produced.");
					return;
				}
				try
				{
					if (!SongExchangeFile.IsJson(outputPath)) File.Delete(outputPath);
				}
				catch
				{
					MessageBox.Show("For some reason, Windows could not create the file '" + outputPath + "'. Export NOT done.");
					ProgressBar1.Value = 0;
					return;
				}
			}
			tbExportTo.Visible = false;
			string strExt = Path.GetExtension(outputPath).ToLower();
			switch (strExt)
			{
				case ".esf":
					Export_DatabaseFormat(outputPath);
					break;
				case ".json":
					ExportJson(outputPath);
					break;
				case ".esn":
					Export_TextFormat(outputPath);
					break;
				default:
					Export_XMLFormat(outputPath);
					break;
			}
			tbExportTo.Visible = true;
		}

		private void Export_DatabaseFormat(string ExportFileName)
		{
			string exportConnectionString = DbController.ConnectionStringForFile(ExportFileName);
			int progressPercent = 0;
			string databaseTemplatePath = Application.StartupPath + "\\Sys\\Defdb.dat";
			if (File.Exists(databaseTemplatePath))
			{
				Gf.ValidateDir(Path.GetDirectoryName(ExportFileName) + "\\", CreateDir: true);
				File.Copy(databaseTemplatePath, ExportFileName, overwrite: true);
				List<ListViewItem> checkedItems = GetCheckedSongItems();
				using DataTable dt = GetSongsForExport(checkedItems);
				Dictionary<int, DataRow> rowMap = BuildSongRowMap(dt);
				//tableRecordSet.Index = "PrimaryKey";
				Cursor = Cursors.WaitCursor;
				ProgressBar1.Visible = true;
				ProgressBar1.Value = 0;
				int exportedSongId = 0;
				if (Gf.DeleteAllFolders(exportConnectionString))
				{
					for (int i = 0; i < FolderList.CheckedItems.Count; i++)
					{
						Gf.ResetFolder(Gf.GetFolderNumber(FolderList.CheckedItems[i].ToString()), FolderList.CheckedItems[i].ToString(), exportConnectionString);
					}
					for (int i = 0; i < checkedItems.Count; i++)
					{
						int songId = DataUtil.StringToInt(checkedItems[i].SubItems[1].Text);
						if (rowMap.TryGetValue(songId, out DataRow row) && Gf.LoadDataIntoItem(ref ExportItem, row))
						{
							exportedSongId = Gf.InsertItemIntoDatabase(exportConnectionString, ExportItem);
						}
						Update();
						progressPercent = (i + 1) * 100 / TotSongsSel;
						ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
						ProgressBar1.Invalidate();
						if (exportedSongId < 1)
						{
							i = checkedItems.Count;
						}
					}
					Cursor = Cursors.Default;
					if (exportedSongId > 0)
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
			int exportedSongCount = 0;
			Gf.ValidateDir(Path.GetDirectoryName(ExportFileName), CreateDir: true);
			int progressPercent = 0;
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
					stringBuilder.Append(">f" + Gf.Folders[ExportItem.FolderNo].Name);
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
						int verseIndicator = DataUtil.StringToInt(ExportItem.SongSequence[j]);
						if (verseIndicator > 0 && verseIndicator < 13)
						{
							tempSequence += Convert.ToString(verseIndicator);
						}
						else
						{
							tempSequence += Gf.SequenceSymbol[verseIndicator];
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
				AppendLegacyTextLyrics(stringBuilder, ExportItem.CompleteLyrics);
				exportedSongCount++;
				Update();
				progressPercent = (i + 1) * 100 / TotSongsSel;
				ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
				ProgressBar1.Invalidate();
			}

			if (FileUtil.CreateNewFile(ExportFileName, FileUtil.FileContentsType.DoubleByte, stringBuilder.ToString()))
			{
				MessageBox.Show("Export Completed. Total of " + Convert.ToString(exportedSongCount) + " songs exported to " + ExportFileName);
			}
			else
			{
				MessageBox.Show("Error encountered when trying to create export file'" + ExportFileName + "'. Export NOT completed.");
			}
			Cursor = Cursors.Default;
			ProgressBar1.Visible = false;
		}

		internal static void AppendLegacyTextLyrics(StringBuilder output, string lyrics)
		{
			output.Append("\r\n");
			ReadOnlySpan<char> remaining = lyrics.AsSpan();
			int lineEnd;
			while ((lineEnd = remaining.IndexOfAny('\r', '\n')) >= 0)
			{
				output.Append(remaining[..lineEnd]).Append("\r\n");
				int next = lineEnd + 1;
				if (remaining[lineEnd] == '\r' && next < remaining.Length && remaining[next] == '\n')
					next++;
				remaining = remaining[next..];
			}
			output.Append(remaining);
		}

		private void ExportJson(string path)
		{
			Cursor = Cursors.WaitCursor;
			ProgressBar1.Value = 0;
			ProgressBar1.Visible = true;
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
				var checkedItems = GetCheckedSongItems();
				using var table = GetSongsForExport(checkedItems);
				var rows = BuildSongRowMap(table);
				int exported = 0;
				IEnumerable<ExchangeSong> SelectedSongs()
				{
					foreach (var item in checkedItems)
					{
						int id = DataUtil.StringToInt(item.SubItems[1].Text);
						if (!rows.TryGetValue(id, out var row) || !Gf.LoadDataIntoItem(ref ExportItem, row))
							throw new InvalidDataException("Could not read selected song " + id);
						yield return Gf.ToExchangeSong(ExportItem, Gf.Folders[ExportItem.FolderNo].Name);
						ProgressBar1.Value = (int)(++exported * 100L / checkedItems.Count);
					}
				}
				SongExchangeFile.WriteJson(path, SelectedSongs());
				MessageBox.Show($"Exported {exported} songs to {path}");
			}
			catch (Exception ex) when (SongExchangeFile.IsReadError(ex))
			{
				MessageBox.Show("Could not export the song file. " + ex.Message);
			}
			finally
			{
				ProgressBar1.Visible = false;
				Cursor = Cursors.Default;
			}
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
            int exportedSongCount = 0;
			int progressPercent = 0;
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
							exportedSongCount++;
						}
						Update();
						progressPercent = (i + 1) * 100 / TotSongsSel;
						ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
						ProgressBar1.Invalidate();
					}
				}
				xtw.WriteEndDocument();
				xtw.Flush();
				xtw.Dispose();
				Cursor = Cursors.Default;
				MessageBox.Show("Export Completed. Total of " + Convert.ToString(exportedSongCount) + " songs exported to " + ExportFileName);
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
