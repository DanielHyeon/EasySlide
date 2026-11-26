using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Localization;
using Axicultus.Util;

namespace Axicultus;

	public partial class FrmEditBibleItem : Form
	{

		private string InIDStringFirstPart;

		private string InIDString;

		private int InRegion1;

		private int InRegion2;

		public FrmEditBibleItem()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
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

		private static void LoadBibleList(ref ComboBox InBibleList)
		{
			InBibleList.Items.Clear();
			InBibleList.Items.Add("");
			foreach (var version in Gf.BibleVersions)
			{
				InBibleList.Items.Add(version.Name + " - " + version.Description);
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
			if (BibleVersionsRegion1.SelectedIndex <= 0)
			{
				MessageBox.Show(UiText.Get(nameof(FrmEditBibleItem), "SelectRegion1Version"));
				return false;
			}
			return true;
		}

		private void BuildNewIDString()
		{
			var primaryVersion = Gf.BibleVersions[BibleVersionsRegion1.SelectedIndex - 1];
			var secondaryVersion = BibleVersionsRegion2.SelectedIndex > 0
				? Gf.BibleVersions[BibleVersionsRegion2.SelectedIndex - 1] : null;
			Gf.EditBible_IDString = $"{InIDStringFirstPart};{primaryVersion.FileName};{secondaryVersion?.FileName};{InIDString}";

			int parenthesisIndex = Gf.EditBible_Title.IndexOf('(');
			if (parenthesisIndex > 0)
				Gf.EditBible_Title = DataUtil.Trim(DataUtil.Left(Gf.EditBible_Title, parenthesisIndex - 1));
			string versionNames = secondaryVersion == null ? primaryVersion.Name : $"{primaryVersion.Name}/{secondaryVersion.Name}";
			Gf.EditBible_Title += $" ({versionNames})";
		}

	}
