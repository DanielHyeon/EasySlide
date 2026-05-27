using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmShowAlert : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private string FormRegLeft = "ShowAlertLeft";

		private string FormRegTop = "ShowAlertTop";

		public event Message OnMessage;

		public FrmShowAlert()
		{
			InitializeComponent();
		}

		private void FrmShowAlert_Load(object sender, EventArgs e)
		{
			Gf.AlertFormOpen = true;
			int num = RegUtil.GetRegValue("settings", FormRegLeft, 50);
			int num2 = RegUtil.GetRegValue("settings", FormRegTop, 100);
			if (num < 0)
			{
				num = 0;
			}
			else if (num > Screen.PrimaryScreen.Bounds.Width - base.Width)
			{
				num = Screen.PrimaryScreen.Bounds.Width - base.Width;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			else if (num2 > Screen.PrimaryScreen.Bounds.Height - base.Height)
			{
				num2 = Screen.PrimaryScreen.Bounds.Height - base.Height;
			}
			base.Top = num2;
			base.Left = num;
			LoadAlertList();
			cbParentalAlert.Text = Gf.ParentalAlertDetails;
			Parental_Flash.Checked = Gf.ParentalAlertFlash;
			Parental_Scroll.Checked = Gf.ParentalAlertScroll;
			Parental_Transparent.Checked = Gf.ParentalAlertTransparent;
			ParentalPrefix.Text = Gf.ParentalAlertHeading + " ";
			cbMessageAlert.Text = Gf.MessageAlertDetails;
			Message_Flash.Checked = Gf.MessageAlertFlash;
			Message_Scroll.Checked = Gf.MessageAlertScroll;
			Message_Transparent.Checked = Gf.MessageAlertTransparent;
			tbLyricsAlert.Text = Gf.LyricsAlertDetails;
			TimerRestoreWindow.Start();
		}

		private void BtnStop_Click(object sender, EventArgs e)
		{
			Gf.MessageAlertLive = false;
			Gf.ParentalAlertLive = false;
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void LoadAlertList()
		{
			Gf.LoadComboBoxFromTextFile(ref cbMessageAlert, Gf.AlertsDataFile);
			Gf.LoadComboBoxFromTextFile(ref cbParentalAlert, Gf.ParentalDataFile);
		}

		private void ParentalShow_Click(object sender, EventArgs e)
		{
			Gf.ParentalAlertDetails = DataUtil.Trim(cbParentalAlert.Text);
			if (!(Gf.ParentalAlertDetails == ""))
			{
				if (cbParentalAlert.Items.Count == 0 || cbParentalAlert.Text != cbParentalAlert.Items[0].ToString())
				{
					try
					{
						cbParentalAlert.Items.Insert(0, cbParentalAlert.Text);
						if (cbParentalAlert.Items.Count > 20)
						{
							for (int num = cbParentalAlert.Items.Count; num >= 21; num--)
							{
								cbParentalAlert.Items.RemoveAt(num);
							}
						}
						Gf.SaveComboBoxToTextFile(ref cbParentalAlert, Gf.ParentalDataFile);
					}
					catch
					{
					}
				}
				this.OnMessage(1, "");
			}
		}

		private void MessageShow_Click(object sender, EventArgs e)
		{
			Gf.MessageAlertDetails = DataUtil.Trim(cbMessageAlert.Text);
			if (!(Gf.MessageAlertDetails == ""))
			{
				cbMessageAlert.Text = DataUtil.Trim(cbMessageAlert.Text);
				if (cbMessageAlert.Items.Count == 0 || cbMessageAlert.Text != cbMessageAlert.Items[0].ToString())
				{
					try
					{
						cbMessageAlert.Items.Insert(0, cbMessageAlert.Text);
						if (cbMessageAlert.Items.Count > 20)
						{
							for (int num = cbMessageAlert.Items.Count; num >= 21; num--)
							{
								cbMessageAlert.Items.RemoveAt(num);
							}
						}
						Gf.SaveComboBoxToTextFile(ref cbMessageAlert, Gf.AlertsDataFile);
					}
					catch
					{
					}
				}
				this.OnMessage(0, "");
			}
		}

		private void FrmShowAlert_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveFormLocation();
			Gf.SaveOptionsData();
			TimerRestoreWindow.Stop();
			Gf.AlertFormOpen = false;
		}

		private void SaveFormLocation()
		{
			RegUtil.SaveRegValue("settings", FormRegLeft, base.Left);
			RegUtil.SaveRegValue("settings", FormRegTop, base.Top);
		}

		private void TimerRestoreWindow_Tick(object sender, EventArgs e)
		{
			if (Gf.AlertRestoreWindow)
			{
				Gf.AlertRestoreWindow = false;
				if (base.WindowState == FormWindowState.Minimized)
				{
					base.WindowState = FormWindowState.Normal;
				}
				else
				{
					Focus();
				}
				base.TopMost = true;
				base.TopMost = false;
			}
		}

		private void ScrollFlashOption_Click(object sender, EventArgs e)
		{
			Gf.ParentalAlertScroll = Parental_Scroll.Checked;
			Gf.ParentalAlertFlash = Parental_Flash.Checked;
			Gf.ParentalAlertTransparent = Parental_Transparent.Checked;
			Gf.MessageAlertScroll = Message_Scroll.Checked;
			Gf.MessageAlertFlash = Message_Flash.Checked;
			Gf.MessageAlertTransparent = Message_Transparent.Checked;
		}

		private void btnClearHistoryMessage_Click(object sender, EventArgs e)
		{
			cbMessageAlert.Items.Clear();
			cbMessageAlert.Text = "";
			Gf.MessageAlertDetails = "";
			Gf.SaveComboBoxToTextFile(ref cbMessageAlert, Gf.AlertsDataFile);
		}

		private void btnClearHistoryParental_Click(object sender, EventArgs e)
		{
			cbParentalAlert.Items.Clear();
			cbParentalAlert.Text = "";
			Gf.ParentalAlertDetails = "";
			Gf.SaveComboBoxToTextFile(ref cbParentalAlert, Gf.ParentalDataFile);
		}

		private void cbParentalAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = ParentalShow;
		}

		private void cbMessageAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = MessageShow;
		}

		private void tbLyricsAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = LyricsShow;
		}

		private void LyricsShow_Click(object sender, EventArgs e)
		{
			Gf.LyricsAlertDetails = DataUtil.Trim(tbLyricsAlert.Text);
			if (!(Gf.LyricsAlertDetails == ""))
			{
				tbLyricsAlert.Text = DataUtil.Trim(tbLyricsAlert.Text);
				this.OnMessage(2, "");
			}
		}

		private void btnClearLyrics_Click(object sender, EventArgs e)
		{
			tbLyricsAlert.Text = "";
			Gf.LyricsAlertDetails = "";
			this.OnMessage(2, "");
		}
	}
}
