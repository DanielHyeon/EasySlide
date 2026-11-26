using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Axicultus.Localization;
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
			UiText.ApplyTo(this);
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
			folderBrowserDialog.Description = UiText.Get(nameof(FrmImportFolder), "SelectSourceFolder");
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
					MessageBox.Show(this, ex.Message, UiText.Get(nameof(FrmImportFolder), "FolderImportCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				string wordDocumentSummary = (wordDocumentCount > 0) ? UiText.Format(nameof(FrmImportFolder), (wordDocumentCount > 1) ? "WordDocuments" : "WordDocument", wordDocumentCount) : "";
				string textFileSummary = (textFileCount > 0) ? UiText.Format(nameof(FrmImportFolder), (textFileCount > 1) ? "TextFiles" : "TextFile", textFileCount) : "";
				string importSummary = "";
				importSummary = ((wordDocumentCount > 0 && textFileCount > 0) ? UiText.Format(nameof(FrmImportFolder), "ItemsSummary", documents.Count, wordDocumentSummary, textFileSummary) : ((wordDocumentCount > 0) ? wordDocumentSummary : ((textFileCount <= 0) ? "" : textFileSummary)));
				MessageBox.Show((importSummary != "") ? UiText.Format(nameof(FrmImportFolder), "ImportedInto", importSummary, SongFolder.Text) : UiText.Get(nameof(FrmImportFolder), "NoDocumentsFound"));
			}
			else
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportFolder), "NothingImported"));
			}
		}

		private bool ValidateContents()
		{
			if (!Directory.Exists(tbLocation.Text))
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportFolder), "SourceFolderMissing"));
				return false;
			}
			if (SongFolder.Text == "")
			{
				MessageBox.Show(UiText.Get(nameof(FrmImportFolder), "DestinationFolderMissing"));
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
