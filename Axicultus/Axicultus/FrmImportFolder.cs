using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

public partial class FrmImportFolder : Form
	{
		private CancellationTokenSource documentScan;

		private SongSettings ImportItem = new();

		public FrmImportFolder()
		{
			InitializeComponent();
		}

		private void FrmImportFolder_Load(object sender, EventArgs e)
		{
			tbLocation.Text = Gf.ImportFolder_StartDir;
			BuildFolderList();
		}

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new();
			folderBrowserDialog.SelectedPath = ((tbLocation.Text != "") ? tbLocation.Text : "C:\\");
			folderBrowserDialog.Description = "Please select your Source Windows Folder.";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				tbLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private async void BtnOK_Click(object sender, EventArgs e)
		{
			if (documentScan != null || !ValidateContents())
				return;
			string sourceFolder = tbLocation.Text;
			using var scan = new CancellationTokenSource();
			documentScan = scan;
			BtnOK.Enabled = false;
			groupBox1.Enabled = false;
			ProgressBar1.Style = ProgressBarStyle.Marquee;
			try
			{
				var documents = await Task.Run(() => DocumentImportFiles.Find(sourceFolder, scan.Token), scan.Token);
				if (!scan.IsCancellationRequested && !IsDisposed)
				{
					ProgressBar1.Style = ProgressBarStyle.Blocks;
					ImportDocuments(documents);
				}
			}
			catch (OperationCanceledException) when (scan.IsCancellationRequested) { }
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				if (!scan.IsCancellationRequested && !IsDisposed)
					MessageBox.Show(this, ex.Message, "Folder import", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				documentScan = null;
				if (!IsDisposed)
				{
					ProgressBar1.Style = ProgressBarStyle.Blocks;
					groupBox1.Enabled = true;
					BtnOK.Enabled = true;
				}
			}
		}

		private void ImportDocuments(List<string> documents)
		{
			ProgressBar1.Value = 0;
			Gf.InitialiseIndividualData(ref ImportItem);
			int wordDocumentCount = 0;
			int textFileCount = 0;
			int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
			if (documents.Count > 0 && folderNumber > 0)
			{
				for (int i = 0; i < documents.Count; i++)
				{
					string path = documents[i];
					ProgressBar1.Value = (int)((long)i * 100 / documents.Count);
					Invalidate();
					ProgressBar1.Invalidate();
					if (Path.GetExtension(path.AsSpan()).Equals(".txt", StringComparison.OrdinalIgnoreCase))
						textFileCount++;
					else
						wordDocumentCount++;
					ImportItem.FolderNo = folderNumber;
					ImportItem.Title = Path.GetFileNameWithoutExtension(path);
					ImportItem.CompleteLyrics = Gf.ExtractDocTextContents(path);
					Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, ImportItem);
				}
				ProgressBar1.Value = 100;
				string wordDocumentSummary = (wordDocumentCount > 0) ? (wordDocumentCount + " Word Document" + ((wordDocumentCount > 1) ? "s" : "")) : "";
				string textFileSummary = (textFileCount > 0) ? (textFileCount + " Text File" + ((textFileCount > 1) ? "s" : "")) : "";
				string importSummary = "";
				importSummary = ((wordDocumentCount > 0 && textFileCount > 0) ? (documents.Count + " items (" + wordDocumentSummary + " and " + textFileSummary + ")") : ((wordDocumentCount > 0) ? wordDocumentSummary : ((textFileCount <= 0) ? "" : textFileSummary)));
				MessageBox.Show((importSummary != "") ? ("Imported " + importSummary + " into " + SongFolder.Text) : "No Word/text documents were found for import");
			}
			else
			{
				MessageBox.Show("Nothing Imported.  No Word or text Files were found in the Source Windows Folder");
			}
		}

		private bool ValidateContents()
		{
			if (!Directory.Exists(tbLocation.Text))
			{
				MessageBox.Show("Error - Source Windows Folder does not exist! Please enter a valid Source Windows Folder at A.");
				return false;
			}
			if (SongFolder.Text == "")
			{
				MessageBox.Show("Error - Destination Axicultus Folder not yet selected! Please select a Destination Axicultus Folder at B.");
				return false;
			}
			return true;
		}

		private void BuildFolderList()
		{
			string selectedFolderName = "";
			SongFolder.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (Gf.Folders[i].Use > 0)
				{
					SongFolder.Items.Add(Gf.Folders[i].Name);
					if (Gf.Folders[i].Name == Gf.ImportFolder_FolderName)
					{
						selectedFolderName = Gf.ImportFolder_FolderName;
					}
				}
			}
			SongFolder.Text = selectedFolderName;
		}

		private void FrmImportFolder_FormClosing(object sender, FormClosingEventArgs e)
		{
			documentScan?.Cancel();
			Gf.ImportFolder_FolderName = SongFolder.Text;
			Gf.ImportFolder_StartDir = tbLocation.Text;
		}

		            }
