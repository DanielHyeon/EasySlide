using Easislides.Module;
using Easislides.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmManageItemLists : Form
	{
		private string InputDir;

		private string InputDefaultFile;

		private string ListText;

		private string ListTextTo;

		private string CurFile;

		private string StartingFolder;

		private string Ext_ToUse = "";

		private IContainer components = null;

		private ListView ItemList;

		private Button AddBtn;

		private Button RenameBtn;

		private Button DelBtn;

		private Button CloseBtn;

		private ColumnHeader columnHeader1;

		private Button SaveAsBtn;

		private Button SaveToBtn;

		private Button SaveTemplateBtn;

		private SaveFileDialog saveFileDialog1;

		public FrmManageItemLists()
		{
			InitializeComponent();
		}

		private void FrmManageItemLists_Load(object sender, EventArgs e)
		{
			if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				Text = "Manage Worship Lists";
				InputDir = gf.WorshipDir;
				InputDefaultFile = "Worship Service";
				CurFile = gf.CurSession;
				ListText = "WorshipList";
				ListTextTo = "PraiseBook";
			}
			else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
			{
				Text = "Manage PraiseBooks";
				InputDir = gf.PraiseBookDir;
				InputDefaultFile = "PraiseBook 1";
				CurFile = gf.CurPraiseBook;
				ListText = "PraiseBook";
				ListTextTo = "WorshipList";
			}
			SaveToBtn.Text = ListTextTo;
			UpdateLists();
		}

		private void UpdateLists()
		{
			bool flag = false;
			byte[] array = new byte[1];
			ItemList.Items.Clear();
			gf.ValidateDir(InputDir, CreateDir: true);
			Ext_ToUse = ((InputDir == gf.WorshipDir) ? ".esw" : ".esp");
			string[] files = Directory.GetFiles(InputDir, "*" + Ext_ToUse);
			string[] array2 = files;
			foreach (string text in array2)
			{
				string InFileName = text;
				InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				if (InFileName != "")
				{
					ItemList.Items.Add(InFileName);
					if (CurFile == InFileName)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				int num = 0;
				while (true)
				{
					if (num < ItemList.Items.Count)
					{
						if (CurFile == ItemList.Items[num].Text)
						{
							break;
						}
						num++;
						continue;
					}
					return;
				}
				ItemList.Items[num].Selected = true;
			}
			else if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				if (ItemList.Items.Count > 0)
				{
					gf.CurSession = ItemList.Items[0].Text;
				}
				else
				{
					gf.CurSession = "";
				}
			}
			else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
			{
				if (ItemList.Items.Count > 0)
				{
					gf.CurPraiseBook = ItemList.Items[0].Text;
				}
				else
				{
					gf.CurPraiseBook = "";
				}
			}
		}

		private void AddBtn_Click(object sender, EventArgs e)
		{
			gf.NameChangeAction = 1;
			gf.NameChangeSucceeded = false;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				UpdateLists();
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
				}
				else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void DelBtn_Click(object sender, EventArgs e)
		{
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to delete!");
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			if (MessageBox.Show("Really delete '" + text + "' " + ListText + "?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					File.Delete(InputDir + text + Ext_ToUse);
					UpdateLists();
					if (gf.EasiSlidesMode == UsageMode.Worship)
					{
						gf.WorshipListsChanged = true;
					}
					else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
					{
						gf.PraiseBooksListChanged = true;
					}
				}
				catch
				{
					MessageBox.Show("Error encountered whilst trying to delete the " + ListText + " - Please ensure you have delete access.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

		private void RenameBtn_Click(object sender, EventArgs e)
		{
			gf.NameChangeAction = 2;
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to rename!");
				return;
			}
			gf.SelectedListName = ItemList.SelectedItems[0].Text;
			bool flag = (gf.SelectedListName == CurFile) ? true : false;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
			{
				if (flag)
				{
					CurFile = gf.SelectedListName;
				}
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
					gf.CurSession = CurFile;
				}
				else
				{
					gf.PraiseBooksListChanged = true;
					gf.CurPraiseBook = CurFile;
				}
				UpdateLists();
			}
		}

		private void SaveAsBtn_Click(object sender, EventArgs e)
		{
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to Save As!");
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			if (SaveAsItem(InputDir + text + Ext_ToUse) != "")
			{
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.WorshipListsChanged = true;
				}
				else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
				{
					gf.PraiseBooksListChanged = true;
				}
			}
		}

		private void SaveToBtn_Click(object sender, EventArgs e)
		{
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to Save To " + ListTextTo);
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			gf.SelectedListName = InputDir + text + Ext_ToUse;
			FrmUpdateFileName frmUpdateFileName = new FrmUpdateFileName();
			gf.NameChangeSucceeded = false;
			if (gf.EasiSlidesMode == UsageMode.Worship)
			{
				gf.NameChangeAction = 7;
				if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
				{
					gf.PraiseBooksListChanged = true;
				}
			}
			else
			{
				gf.NameChangeAction = 8;
				if (frmUpdateFileName.ShowDialog() == DialogResult.OK)
				{
					gf.WorshipListsChanged = true;
				}
			}
		}

		private void SaveTemplateBtn_Click(object sender, EventArgs e)
		{
			if (ItemList.SelectedItems.Count < 1)
			{
				MessageBox.Show("You have not selected a " + ListText + " to Save As a Template!");
				return;
			}
			string text = ItemList.SelectedItems[0].Text;
			try
			{
				string text2 = InputDir + text + Ext_ToUse;
				string text3 = gf.WorshipTemplatesDir + text + ".est";
				if (File.Exists(text2))
				{
					DialogResult dialogResult = DialogResult.Yes;
					if (File.Exists(text3))
					{
						dialogResult = MessageBox.Show("There is already a template with the same name. Overwrite it?", "Overwrite", MessageBoxButtons.YesNo);
					}
					if (dialogResult == DialogResult.Yes)
					{
						File.Copy(text2, text3, overwrite: true);
					}
				}
				else
				{
					MessageBox.Show("Error Saving Template - cannot find the selected " + ListText + "!");
				}
			}
			catch
			{
				MessageBox.Show("Error Saving File, please make sure you have write access and try again");
			}
		}

		private string SaveAsItem(string InFileName)
		{
			saveFileDialog1.Filter = "EasiSlides " + ListText + " File (*" + Ext_ToUse + ")|*" + Ext_ToUse;
			saveFileDialog1.InitialDirectory = Path.GetDirectoryName(InFileName);
			saveFileDialog1.FileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true);
			saveFileDialog1.OverwritePrompt = true;
			saveFileDialog1.AddExtension = true;
			saveFileDialog1.DefaultExt = Ext_ToUse;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string fileName = saveFileDialog1.FileName;
				try
				{
					if (!File.Exists(fileName))
					{
						File.Copy(InFileName, fileName, overwrite: true);
						UpdateLists();
						return fileName;
					}
				}
				catch
				{
					MessageBox.Show("Error Saving File, please make sure you have write access and try again");
				}
			}
			return "";
		}

		private void CloseBtn_Click(object sender, EventArgs e)
		{
			MakeSelectedItemCurrent();
		}

		private void ItemList_DoubleClick(object sender, EventArgs e)
		{
			MakeSelectedItemCurrent();
			Close();
		}

		private void MakeSelectedItemCurrent()
		{
			if (ItemList.SelectedItems.Count > 0)
			{
				if (gf.EasiSlidesMode == UsageMode.Worship)
				{
					gf.CurSession = ItemList.SelectedItems[0].Text;
				}
				else
				{
					gf.CurPraiseBook = ItemList.SelectedItems[0].Text;
				}
			}
		}

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmManageItemLists));
			ItemList = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			AddBtn = new System.Windows.Forms.Button();
			RenameBtn = new System.Windows.Forms.Button();
			DelBtn = new System.Windows.Forms.Button();
			CloseBtn = new System.Windows.Forms.Button();
			SaveAsBtn = new System.Windows.Forms.Button();
			SaveToBtn = new System.Windows.Forms.Button();
			SaveTemplateBtn = new System.Windows.Forms.Button();
			saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			SuspendLayout();
			ItemList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1]
			{
				columnHeader1
			});
			ItemList.FullRowSelect = true;
			ItemList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			ItemList.HideSelection = false;
			ItemList.Location = new System.Drawing.Point(12, 12);
			ItemList.Name = "ItemList";
			ItemList.ShowGroups = false;
			ItemList.ShowItemToolTips = true;
			ItemList.Size = new System.Drawing.Size(181, 200);
			ItemList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			ItemList.TabIndex = 0;
			ItemList.UseCompatibleStateImageBehavior = false;
			ItemList.View = System.Windows.Forms.View.Details;
			ItemList.DoubleClick += new System.EventHandler(ItemList_DoubleClick);
			columnHeader1.Text = "";
			columnHeader1.Width = 165;
			AddBtn.Image = Resources.New;
			AddBtn.Location = new System.Drawing.Point(199, 12);
			AddBtn.Name = "AddBtn";
			AddBtn.Size = new System.Drawing.Size(88, 24);
			AddBtn.TabIndex = 1;
			AddBtn.Text = "Add New";
			AddBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			AddBtn.Click += new System.EventHandler(AddBtn_Click);
			RenameBtn.Image = Resources.editsym;
			RenameBtn.Location = new System.Drawing.Point(199, 42);
			RenameBtn.Name = "RenameBtn";
			RenameBtn.Size = new System.Drawing.Size(88, 24);
			RenameBtn.TabIndex = 2;
			RenameBtn.Text = "Rename";
			RenameBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			RenameBtn.Click += new System.EventHandler(RenameBtn_Click);
			DelBtn.Image = Resources.Delete;
			DelBtn.Location = new System.Drawing.Point(199, 72);
			DelBtn.Name = "DelBtn";
			DelBtn.Size = new System.Drawing.Size(88, 24);
			DelBtn.TabIndex = 3;
			DelBtn.Text = "Delete";
			DelBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			DelBtn.Click += new System.EventHandler(DelBtn_Click);
			CloseBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			CloseBtn.Location = new System.Drawing.Point(105, 220);
			CloseBtn.Name = "CloseBtn";
			CloseBtn.Size = new System.Drawing.Size(88, 24);
			CloseBtn.TabIndex = 4;
			CloseBtn.Text = "Close";
			CloseBtn.Click += new System.EventHandler(CloseBtn_Click);
			SaveAsBtn.Image = Resources.Save;
			SaveAsBtn.Location = new System.Drawing.Point(199, 128);
			SaveAsBtn.Name = "SaveAsBtn";
			SaveAsBtn.Size = new System.Drawing.Size(88, 24);
			SaveAsBtn.TabIndex = 5;
			SaveAsBtn.Text = "Save As";
			SaveAsBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			SaveAsBtn.Click += new System.EventHandler(SaveAsBtn_Click);
			SaveToBtn.Image = Resources.Save;
			SaveToBtn.Location = new System.Drawing.Point(199, 188);
			SaveToBtn.Name = "SaveToBtn";
			SaveToBtn.Size = new System.Drawing.Size(88, 24);
			SaveToBtn.TabIndex = 6;
			SaveToBtn.Text = "WorshipList";
			SaveToBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			SaveToBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			SaveToBtn.Click += new System.EventHandler(SaveToBtn_Click);
			SaveTemplateBtn.Image = Resources.Save;
			SaveTemplateBtn.Location = new System.Drawing.Point(199, 158);
			SaveTemplateBtn.Name = "SaveTemplateBtn";
			SaveTemplateBtn.Size = new System.Drawing.Size(88, 24);
			SaveTemplateBtn.TabIndex = 7;
			SaveTemplateBtn.Text = "Template";
			SaveTemplateBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			SaveTemplateBtn.Click += new System.EventHandler(SaveTemplateBtn_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(299, 256);
			base.Controls.Add(SaveTemplateBtn);
			base.Controls.Add(SaveToBtn);
			base.Controls.Add(SaveAsBtn);
			base.Controls.Add(CloseBtn);
			base.Controls.Add(DelBtn);
			base.Controls.Add(RenameBtn);
			base.Controls.Add(AddBtn);
			base.Controls.Add(ItemList);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmManageItemLists";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			base.Load += new System.EventHandler(FrmManageItemLists_Load);
			ResumeLayout(false);
		}
	}
}
