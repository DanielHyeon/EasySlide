using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Easislides
{
	public partial class FrmEditNotes : Form
	{

		public FrmEditNotes()
		{
			InitializeComponent();
		}

		private void FrmEditNotes_Load(object sender, EventArgs e)
		{
			Text = Gf.EditNotesHeading;
			tbData.Text = Gf.EditNotes;
			if (tbData.Text != "")
			{
				tbData.SelectionStart = 0;
				tbData.SelectionLength = 0;
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Gf.EditNotes = tbData.Text;
		}
	}
}
