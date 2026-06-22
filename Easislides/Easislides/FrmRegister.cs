using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public partial class FrmRegister : Form
	{

		public FrmRegister()
		{
			InitializeComponent();
		}

		private void FrmRegister_Load(object sender, EventArgs e)
		{
			lblRegister.Text = "EasiSlides Version 4.0.5 is provided free of charge and for your indefinite use provided you abide by the End User Licence Agreement (EULA).  \r\n\r\nIf you intend to use this software on an on-going basis, you are invited to register your use of the software by clicking on the 'Register' button below which will take you to the EasiSlides Registration Page at http://www.easislides.com/register \r\n\r\nRegistration is voluntary and is free of charge.  The registration information you provide will help us to monitor the spread of use of EasiSlides around the world.";
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
}
