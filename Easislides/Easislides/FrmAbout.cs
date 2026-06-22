using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Util;
using Microsoft.Win32;

namespace Easislides
{
	public partial class FrmAbout : Form
	{

		public FrmAbout()
		{
			InitializeComponent();
		}

		private void FrmAbout_Load(object sender, EventArgs e)
		{
			lblRegDetails.Text = RegUtil.GetRegValue("config", "RegistrationUser", "");
			lblVersion.Text = "Software Version: 5.0.0";
			lblCopyright.Text = "Copyright " + '©' + " 2019 daniel park revision";
			lbleula.Text = Gf.EULA;
			lbleula.SelectionStart = 0;
			lbleula.SelectionLength = 0;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Gf.UserString = DataUtil.Trim(lblRegDetails.Text);
			RegUtil.SaveRegValue("config", "RegistrationUser", Gf.UserString);
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Gf.RunProcess(linkLabel1.Text);
		}

		private void lbleula_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Gf.RunProcess(e.LinkText);
		}

		private void BtnSysInfo_Click(object sender, EventArgs e)
		{
			string text = (string)Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\MSINFO").GetValue("Path", "");
			if (File.Exists(text))
			{
				Gf.RunProcess(text);
			}
		}
	}
}
