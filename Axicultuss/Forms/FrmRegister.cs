using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Localization;

namespace Axicultus;

	public partial class FrmRegister : Form
	{

		public FrmRegister()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmRegister_Load(object sender, EventArgs e)
		{
			lblRegister.Text = UiText.Get(nameof(FrmRegister), "Info");
			lblRegister.SelectionStart = 0;
			lblRegister.SelectionLength = 0;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Gf.RunProcess("http://www.easislides.com/register");
		}

		private void lblRegister_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Gf.RunProcess(e.LinkText);
		}

		            }
