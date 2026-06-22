using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmEditBibleItem : Form
	{

		private string InIDStringFirstPart;

		private string InIDString;

		private int InRegion1;

		private int InRegion2;

		public FrmEditBibleItem()
		{
			InitializeComponent();
		}

		private void FrmEditBibleItem_Load(object sender, EventArgs e)
		{
			Title.Text = Gf.EditBible_Title;
			LoadBibleList(ref BibleVersionsRegion1);
			LoadBibleList(ref BibleVersionsRegion2);
			InIDString = Gf.EditBible_IDString;
			Gf.EditBible_IDString = "";
			InIDStringFirstPart = DataUtil.ExtractOneInfo(ref InIDString, ';');
			InRegion1 = Gf.LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InIDString, ';')) + 1;
			InRegion2 = Gf.LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InIDString, ';')) + 1;
			BibleVersionsRegion1.SelectedIndex = InRegion1;
			BibleVersionsRegion2.SelectedIndex = InRegion2;
		}

		private void LoadBibleList(ref ComboBox InBibleList)
		{
			InBibleList.Items.Clear();
			InBibleList.Items.Add("");
			for (int i = 0; i <= Gf.HB_TotalVersions - 1; i++)
			{
				InBibleList.Items.Add(Gf.HB_Versions[i, 1] + " - " + Gf.HB_Versions[i, 2]);
			}
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			if (ValidateVersionOptions())
			{
				if (!((BibleVersionsRegion1.SelectedIndex == InRegion1) & (BibleVersionsRegion2.SelectedIndex == InRegion2)))
				{
					BuildNewIDString();
				}
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}

		private bool ValidateVersionOptions()
		{
			if (BibleVersionsRegion1.SelectedIndex == 0)
			{
				MessageBox.Show("You must select a Bible Version for Region 1");
				return false;
			}
			return true;
		}

		private void BuildNewIDString()
		{
			string text = "";
			string text2 = "";
			text = Gf.GetDisplayNameOnly(ref Gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 4], UpdateByRef: false, KeepExt: true);
			if (BibleVersionsRegion2.SelectedIndex > 0)
			{
				text2 = Gf.GetDisplayNameOnly(ref Gf.HB_Versions[BibleVersionsRegion2.SelectedIndex - 1, 4], UpdateByRef: false, KeepExt: true);
			}
			//Gf.EditBible_IDString = InIDStringFirstPart + ';' + text + ';' + text2 + ';' + InIDString;
			Gf.EditBible_IDString = $"{InIDStringFirstPart};{text};{text2};{InIDString}";

			int num = Gf.EditBible_Title.IndexOf('(');
			if (num > 0)
			{
				Gf.EditBible_Title = DataUtil.Trim(DataUtil.Left(Gf.EditBible_Title, num - 1));
			}
			if (text2 == "")
			{
				Gf.EditBible_Title = Gf.EditBible_Title + " (" + Gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 1] + ")";
				return;
			}
			string editBible_Title = Gf.EditBible_Title;
			//Gf.EditBible_Title = editBible_Title + " (" + Gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 1] + "/" + Gf.HB_Versions[BibleVersionsRegion2.SelectedIndex - 1, 1] + ")";
			Gf.EditBible_Title = $"{editBible_Title} ({Gf.HB_Versions[BibleVersionsRegion1.SelectedIndex - 1, 1]}/{Gf.HB_Versions[BibleVersionsRegion2.SelectedIndex - 1, 1]})";
		}

		private void BtnOK_Click()
		{
		}

		private void BtnCancel_Click()
		{
		}
	}
}
