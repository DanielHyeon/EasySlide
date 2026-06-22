using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmSingleMonitorAlert : Form
	{

		public FrmSingleMonitorAlert()
		{
			InitializeComponent();
		}

		private void FrmSingleMonitorAlert_Load(object sender, EventArgs e)
		{
			base.Height = 24;
			base.Top = Screen.PrimaryScreen.Bounds.Height - base.Height;
			base.Left = 0;
			LoadAlertList();
			Message_Scroll.Checked = Gf.MessageAlertScroll;
			Message_Flash.Checked = Gf.MessageAlertFlash;
			Message_Transparent.Checked = Gf.MessageAlertTransparent;
			cbMessageAlert.Text = Gf.MessageAlertDetails;
			cbMessageAlert.SelectAll();
			Parental_Scroll.Checked = Gf.ParentalAlertScroll;
			Parental_Flash.Checked = Gf.ParentalAlertFlash;
			Parental_Transparent.Checked = Gf.ParentalAlertTransparent;
			cbParentalAlert.Text = Gf.ParentalAlertDetails;
			ParentalPrefix.Text = Gf.ParentalAlertHeading + " ";
			Cursor.Position = new Point(270, base.Top + 12);
			Cursor.Show();
		}

		private void LoadAlertList()
		{
			Gf.LoadComboBoxFromTextFile(ref cbMessageAlert, Gf.AlertsDataFile);
			Gf.LoadComboBoxFromTextFile(ref cbParentalAlert, Gf.ParentalDataFile);
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
				Gf.MessageAlertScroll = Message_Scroll.Checked;
				Gf.MessageAlertFlash = Message_Flash.Checked;
				Gf.MessageAlertTransparent = Message_Transparent.Checked;
				Gf.MessageAlertDetails = cbMessageAlert.Text;
				base.DialogResult = DialogResult.OK;
				Gf.AlertSettings(AlertType.Message);
				Close();
			}
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
				Gf.ParentalAlertScroll = Parental_Scroll.Checked;
				Gf.ParentalAlertFlash = Parental_Flash.Checked;
				Gf.ParentalAlertTransparent = Parental_Transparent.Checked;
				Gf.ParentalAlertDetails = cbParentalAlert.Text;
				base.DialogResult = DialogResult.OK;
				Gf.AlertSettings(AlertType.Parental);
				Close();
			}
		}

		private void BtnDismiss_Click(object sender, EventArgs e)
		{
			Gf.ParentalAlertLive = false;
			Gf.MessageAlertLive = false;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void FrmSingleMonitorAlert_FormClosing(object sender, FormClosingEventArgs e)
		{
			Gf.SaveOptionsData();
			Cursor.Hide();
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

		private void cbMessageAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = MessageShow;
		}

		private void cbParentalAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = ParentalShow;
		}

					}
}
