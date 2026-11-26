using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Axicultus.Localization;
using Axicultus.Util;
using Microsoft.Win32;

namespace Axicultus;

	public partial class FrmAbout : Form
	{

		public FrmAbout()
		{
			InitializeComponent();
			UiText.ApplyTo(this);
		}

		private void FrmAbout_Load(object sender, EventArgs e)
		{
			lblRegDetails.Text = RegUtil.GetRegValue("config", "RegistrationUser", "");
			lblVersion.Text = UiText.Format(nameof(FrmAbout), "SoftwareVersion", "5.0.0");
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
			string systemInfoPath = (string)Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\MSINFO").GetValue("Path", "");
			if (File.Exists(systemInfoPath))
			{
				Gf.RunProcess(systemInfoPath);
			}
		}
	}
