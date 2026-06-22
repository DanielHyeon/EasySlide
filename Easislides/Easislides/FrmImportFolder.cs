using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
    public partial class FrmImportFolder : Form
	{
		private const int MaxDocExtensions = 3000;

		private static DateTime BuildDocumentsStartTime;

		private static TimeSpan BuildDocumentsLapseTime = new TimeSpan(0L);

		private static bool BuildDocumentsContinue = true;

		private static string[] DocFilesList = new string[32000];

		private static int TotalDocFiles = -1;

		public static string[] DocFileExtension = new string[3000];

		public static int TotalDocFileExt = 0;

		private SongSettings ImportItem = new SongSettings();

		public FrmImportFolder()
		{
			InitializeComponent();
		}

		private void FrmImportFolder_Load(object sender, EventArgs e)
		{
			tbLocation.Text = Gf.ImportFolder_StartDir;
			DocFileExtension[0] = ".doc";
			DocFileExtension[1] = ".docx";
			DocFileExtension[2] = ".txt";
			TotalDocFileExt = 3;
			BuildFolderList();
		}

		private void LocationBtn_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.SelectedPath = ((tbLocation.Text != "") ? tbLocation.Text : "C:\\");
			folderBrowserDialog.Description = "Please select your Source Windows Folder.";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				tbLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (ValidateContents())
			{
				StartImport();
			}
		}

		/// <summary>
		/// daniel
		/// Ȯ���� docx �߰�
		/// </summary>
		private void StartImport()
		{
			BuildDocumentsContinue = true;
			BuildDocumentsStartTime = DateTime.Now;
			TotalDocFiles = 0;
			BuildDocumentsListArray(tbLocation.Text);
			int num2 = 0;
			ProgressBar1.Value = 0;
			Gf.InitialiseIndividualData(ref ImportItem);
			int num3 = 0;
			int num4 = 0;
			int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
			if (TotalDocFiles > 0 && folderNumber > 0)
			{
				for (int i = 0; i < TotalDocFiles; i++)
				{
					num2 = i * 100 / TotalDocFiles;
					ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
					Invalidate();
					ProgressBar1.Invalidate();
					switch (Path.GetExtension(DocFilesList[i]).ToLower())
					{
						case ".doc":
						case ".docx":
							num3++;
							break;
						case ".txt":
							num4++;
							break;
					}
					ImportItem.FolderNo = folderNumber;
					ImportItem.Title = Path.GetFileNameWithoutExtension(DocFilesList[i]);
					ImportItem.CompleteLyrics = Gf.ExtractDocTextContents(DocFilesList[i]);
					Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, ImportItem);
				}
				ProgressBar1.Value = 100;
				string text = (num3 > 0) ? (num3 + " Word Document" + ((num3 > 1) ? "s" : "")) : "";
				string text2 = (num4 > 0) ? (num4 + " Text File" + ((num4 > 1) ? "s" : "")) : "";
				string text3 = "";
				text3 = ((num3 > 0 && num4 > 0) ? (TotalDocFiles + " items (" + text + " and " + text2 + ")") : ((num3 > 0) ? text : ((num4 <= 0) ? "" : text2)));
				MessageBox.Show((text3 != "") ? ("Imported " + text3 + " into " + SongFolder.Text) : "No Word/text documents were found for import");
			}
			else
			{
				MessageBox.Show("Nothing Imported.  No Word or text Files were found in the Source Windows Folder");
			}
		}

		public static void BuildDocumentsListArray(string FolderPath)
		{
			if (FolderPath == "" || !BuildDocumentsContinue || (!Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\")))
			{
				return;
			}
			BuildDocumentsLapseTime = DateTime.Now.Subtract(BuildDocumentsStartTime);
			if (BuildDocumentsLapseTime.Seconds > 10)
			{
				BuildDocumentsContinue = false;
				return;
			}
			string[] array;
			for (int i = 0; i < TotalDocFileExt; i++)
			{
				try
				{
					string[] files = Directory.GetFiles(FolderPath, "*" + DocFileExtension[i]);
					array = files;
					foreach (string text in array)
					{
						string text2 = text;
						DocFilesList[TotalDocFiles] = text2;
						TotalDocFiles++;
					}
				}
				catch
				{
				}
			}
			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				Gf.SingleArraySort(directories, SortAscending: true);
			}
			array = directories;
			foreach (string str in array)
			{
				BuildDocumentsListArray(str + "\\");
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
				MessageBox.Show("Error - Destination EasiSlides Folder not yet selected! Please select a Destination EasiSlides Folder at B.");
				return false;
			}
			return true;
		}

		private void BuildFolderList()
		{
			string text = "";
			SongFolder.Items.Clear();
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				if (Gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(Gf.FolderName[i]);
					if (Gf.FolderName[i] == Gf.ImportFolder_FolderName)
					{
						text = Gf.ImportFolder_FolderName;
					}
				}
			}
			SongFolder.Text = text;
		}

		private void FrmImportFolder_FormClosing(object sender, FormClosingEventArgs e)
		{
			Gf.ImportFolder_FolderName = SongFolder.Text;
			Gf.ImportFolder_StartDir = tbLocation.Text;
		}

		            }
}
