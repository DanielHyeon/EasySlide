using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Localization;

namespace Axicultus;

	public partial class FrmMove : Form
	{

		public FrmMove()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmMove_Load(object sender, EventArgs e)
		{
			Label1.Text = UiText.Format(nameof(FrmMove), Gf.SelectedItemsCount > 1 ? "SelectedItems" : "SelectedItem", Gf.SelectedItemsCount);
			Gf.MoveToFolder = -1;
			SongFolder.Items.Clear();
			for (int i = 1; i <= 40; i++)
			{
				if (Gf.Folders[i].Use > 0)
				{
					SongFolder.Items.Add(Gf.Folders[i].Name);
				}
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (SongFolder.Items.Count > 0)
			{
				if (SongFolder.SelectedItems.Count > 0)
				{
					Gf.MoveToFolder = Gf.GetFolderNumber(SongFolder.SelectedItems[0].Text);
					base.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					MessageBox.Show(UiText.Get(nameof(FrmMove), "SelectFolder"));
				}
			}
			else
			{
				MessageBox.Show(UiText.Get(nameof(FrmMove), "NoFolders"));
			}
		}

		private void SongFolder_DoubleClick(object sender, EventArgs e)
		{
			BtnOK.PerformClick();
		}
	}
