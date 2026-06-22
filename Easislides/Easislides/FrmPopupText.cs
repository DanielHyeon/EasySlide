using System;
using System.ComponentModel;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmPopupText : Form
	{

		public FrmPopupText()
		{
			InitializeComponent();
		}

		private void FrmPopup_Load(object sender, EventArgs e)
		{
			tbData.MaxLength = Gf.popUpTextMaxLength;
			tbData.Text = DataUtil.Left(Gf.popUpText, Gf.popUpTextMaxLength);
		}

		private void FrmPopup_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Gf.popUpText = tbData.Text;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

					}
}
