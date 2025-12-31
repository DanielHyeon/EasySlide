using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public class FrmSingleMonitorAlert : Form
	{
		private IContainer components = null;

		private Button MessageShow;

		private Button BtnDismiss;

		private Button BtnCancel;

		private Panel panelMessageBtn;

		private ToolStrip ToolBarMessageFormat;

		private ToolStripButton Message_Scroll;

		private ToolStripButton Message_Flash;

		private ToolStripButton Message_Transparent;

		private Panel panelParentalBtn;

		private ToolStrip ToolBarParentalFormat;

		private ToolStripButton Parental_Scroll;

		private ToolStripButton Parental_Flash;

		private ToolStripButton Parental_Transparent;

		private Button ParentalShow;

		private TextBox ParentalPrefix;

		private ComboBox cbMessageAlert;

		private ComboBox cbParentalAlert;

		private Button btnClearHistoryMessage;

		private Button btnClearHistoryParental;

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
			Message_Scroll.Checked = gf.MessageAlertScroll;
			Message_Flash.Checked = gf.MessageAlertFlash;
			Message_Transparent.Checked = gf.MessageAlertTransparent;
			cbMessageAlert.Text = gf.MessageAlertDetails;
			cbMessageAlert.SelectAll();
			Parental_Scroll.Checked = gf.ParentalAlertScroll;
			Parental_Flash.Checked = gf.ParentalAlertFlash;
			Parental_Transparent.Checked = gf.ParentalAlertTransparent;
			cbParentalAlert.Text = gf.ParentalAlertDetails;
			ParentalPrefix.Text = gf.ParentalAlertHeading + " ";
			Cursor.Position = new Point(270, base.Top + 12);
			Cursor.Show();
		}

		private void LoadAlertList()
		{
			gf.LoadComboBoxFromTextFile(ref cbMessageAlert, gf.AlertsDataFile);
			gf.LoadComboBoxFromTextFile(ref cbParentalAlert, gf.ParentalDataFile);
		}

		private void MessageShow_Click(object sender, EventArgs e)
		{
			gf.MessageAlertDetails = DataUtil.Trim(cbMessageAlert.Text);
			if (!(gf.MessageAlertDetails == ""))
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
						gf.SaveComboBoxToTextFile(ref cbMessageAlert, gf.AlertsDataFile);
					}
					catch
					{
					}
				}
				gf.MessageAlertScroll = Message_Scroll.Checked;
				gf.MessageAlertFlash = Message_Flash.Checked;
				gf.MessageAlertTransparent = Message_Transparent.Checked;
				gf.MessageAlertDetails = cbMessageAlert.Text;
				base.DialogResult = DialogResult.OK;
				gf.AlertSettings(AlertType.Message);
				Close();
			}
		}

		private void ParentalShow_Click(object sender, EventArgs e)
		{
			gf.ParentalAlertDetails = DataUtil.Trim(cbParentalAlert.Text);
			if (!(gf.ParentalAlertDetails == ""))
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
						gf.SaveComboBoxToTextFile(ref cbParentalAlert, gf.ParentalDataFile);
					}
					catch
					{
					}
				}
				gf.ParentalAlertScroll = Parental_Scroll.Checked;
				gf.ParentalAlertFlash = Parental_Flash.Checked;
				gf.ParentalAlertTransparent = Parental_Transparent.Checked;
				gf.ParentalAlertDetails = cbParentalAlert.Text;
				base.DialogResult = DialogResult.OK;
				gf.AlertSettings(AlertType.Parental);
				Close();
			}
		}

		private void BtnDismiss_Click(object sender, EventArgs e)
		{
			gf.ParentalAlertLive = false;
			gf.MessageAlertLive = false;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void FrmSingleMonitorAlert_FormClosing(object sender, FormClosingEventArgs e)
		{
			gf.SaveOptionsData();
			Cursor.Hide();
		}

		private void btnClearHistoryMessage_Click(object sender, EventArgs e)
		{
			cbMessageAlert.Items.Clear();
			cbMessageAlert.Text = "";
			gf.MessageAlertDetails = "";
			gf.SaveComboBoxToTextFile(ref cbMessageAlert, gf.AlertsDataFile);
		}

		private void btnClearHistoryParental_Click(object sender, EventArgs e)
		{
			cbParentalAlert.Items.Clear();
			cbParentalAlert.Text = "";
			gf.ParentalAlertDetails = "";
			gf.SaveComboBoxToTextFile(ref cbParentalAlert, gf.ParentalDataFile);
		}

		private void cbMessageAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = MessageShow;
		}

		private void cbParentalAlert_Enter(object sender, EventArgs e)
		{
			base.AcceptButton = ParentalShow;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			MessageShow = new System.Windows.Forms.Button();
			BtnDismiss = new System.Windows.Forms.Button();
			BtnCancel = new System.Windows.Forms.Button();
			panelMessageBtn = new System.Windows.Forms.Panel();
			ToolBarMessageFormat = new System.Windows.Forms.ToolStrip();
			Message_Scroll = new System.Windows.Forms.ToolStripButton();
			Message_Flash = new System.Windows.Forms.ToolStripButton();
			Message_Transparent = new System.Windows.Forms.ToolStripButton();
			panelParentalBtn = new System.Windows.Forms.Panel();
			ToolBarParentalFormat = new System.Windows.Forms.ToolStrip();
			Parental_Scroll = new System.Windows.Forms.ToolStripButton();
			Parental_Flash = new System.Windows.Forms.ToolStripButton();
			Parental_Transparent = new System.Windows.Forms.ToolStripButton();
			ParentalShow = new System.Windows.Forms.Button();
			ParentalPrefix = new System.Windows.Forms.TextBox();
			cbMessageAlert = new System.Windows.Forms.ComboBox();
			cbParentalAlert = new System.Windows.Forms.ComboBox();
			btnClearHistoryMessage = new System.Windows.Forms.Button();
			btnClearHistoryParental = new System.Windows.Forms.Button();
			panelMessageBtn.SuspendLayout();
			ToolBarMessageFormat.SuspendLayout();
			panelParentalBtn.SuspendLayout();
			ToolBarParentalFormat.SuspendLayout();
			SuspendLayout();
			MessageShow.Location = new System.Drawing.Point(264, 2);
			MessageShow.Name = "MessageShow";
			MessageShow.Size = new System.Drawing.Size(44, 22);
			MessageShow.TabIndex = 1;
			MessageShow.Text = "Show";
			MessageShow.Click += new System.EventHandler(MessageShow_Click);
			BtnDismiss.Location = new System.Drawing.Point(320, 2);
			BtnDismiss.Name = "BtnDismiss";
			BtnDismiss.Size = new System.Drawing.Size(37, 22);
			BtnDismiss.TabIndex = 2;
			BtnDismiss.Text = "Stop";
			BtnDismiss.Click += new System.EventHandler(BtnDismiss_Click);
			BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnCancel.Location = new System.Drawing.Point(358, 2);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new System.Drawing.Size(37, 22);
			BtnCancel.TabIndex = 3;
			BtnCancel.Text = "Exit";
			BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
			panelMessageBtn.Controls.Add(ToolBarMessageFormat);
			panelMessageBtn.Location = new System.Drawing.Point(191, 3);
			panelMessageBtn.Name = "panelMessageBtn";
			panelMessageBtn.Size = new System.Drawing.Size(71, 20);
			panelMessageBtn.TabIndex = 22;
			ToolBarMessageFormat.AutoSize = false;
			ToolBarMessageFormat.CanOverflow = false;
			ToolBarMessageFormat.Dock = System.Windows.Forms.DockStyle.None;
			ToolBarMessageFormat.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			ToolBarMessageFormat.Items.AddRange(new System.Windows.Forms.ToolStripItem[3]
			{
				Message_Scroll,
				Message_Flash,
				Message_Transparent
			});
			ToolBarMessageFormat.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			ToolBarMessageFormat.Location = new System.Drawing.Point(0, 0);
			ToolBarMessageFormat.Name = "ToolBarMessageFormat";
			ToolBarMessageFormat.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			ToolBarMessageFormat.Size = new System.Drawing.Size(71, 22);
			ToolBarMessageFormat.TabIndex = 0;
			Message_Scroll.CheckOnClick = true;
			Message_Scroll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Message_Scroll.Image = Resources.Scroll;
			Message_Scroll.ImageTransparentColor = System.Drawing.Color.Magenta;
			Message_Scroll.Name = "Message_Scroll";
			Message_Scroll.Size = new System.Drawing.Size(23, 19);
			Message_Scroll.ToolTipText = "Scroll";
			Message_Flash.CheckOnClick = true;
			Message_Flash.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Message_Flash.Image = Resources.Flash;
			Message_Flash.ImageTransparentColor = System.Drawing.Color.Magenta;
			Message_Flash.Name = "Message_Flash";
			Message_Flash.Size = new System.Drawing.Size(23, 19);
			Message_Flash.ToolTipText = "Flash";
			Message_Transparent.CheckOnClick = true;
			Message_Transparent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Message_Transparent.Image = Resources.Transparent;
			Message_Transparent.ImageTransparentColor = System.Drawing.Color.Magenta;
			Message_Transparent.Name = "Message_Transparent";
			Message_Transparent.Size = new System.Drawing.Size(23, 19);
			Message_Transparent.Text = "Transparent";
			panelParentalBtn.Controls.Add(ToolBarParentalFormat);
			panelParentalBtn.Location = new System.Drawing.Point(629, 3);
			panelParentalBtn.Name = "panelParentalBtn";
			panelParentalBtn.Size = new System.Drawing.Size(71, 20);
			panelParentalBtn.TabIndex = 25;
			ToolBarParentalFormat.AutoSize = false;
			ToolBarParentalFormat.CanOverflow = false;
			ToolBarParentalFormat.Dock = System.Windows.Forms.DockStyle.None;
			ToolBarParentalFormat.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			ToolBarParentalFormat.Items.AddRange(new System.Windows.Forms.ToolStripItem[3]
			{
				Parental_Scroll,
				Parental_Flash,
				Parental_Transparent
			});
			ToolBarParentalFormat.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			ToolBarParentalFormat.Location = new System.Drawing.Point(0, 0);
			ToolBarParentalFormat.Name = "ToolBarParentalFormat";
			ToolBarParentalFormat.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			ToolBarParentalFormat.Size = new System.Drawing.Size(71, 22);
			ToolBarParentalFormat.TabIndex = 0;
			Parental_Scroll.CheckOnClick = true;
			Parental_Scroll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Parental_Scroll.Image = Resources.Scroll;
			Parental_Scroll.ImageTransparentColor = System.Drawing.Color.Magenta;
			Parental_Scroll.Name = "Parental_Scroll";
			Parental_Scroll.Size = new System.Drawing.Size(23, 19);
			Parental_Scroll.ToolTipText = "Scroll";
			Parental_Flash.CheckOnClick = true;
			Parental_Flash.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Parental_Flash.Image = Resources.Flash;
			Parental_Flash.ImageTransparentColor = System.Drawing.Color.Magenta;
			Parental_Flash.Name = "Parental_Flash";
			Parental_Flash.Size = new System.Drawing.Size(23, 19);
			Parental_Flash.ToolTipText = "Flash";
			Parental_Transparent.CheckOnClick = true;
			Parental_Transparent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Parental_Transparent.Image = Resources.Transparent;
			Parental_Transparent.ImageTransparentColor = System.Drawing.Color.Magenta;
			Parental_Transparent.Name = "Parental_Transparent";
			Parental_Transparent.Size = new System.Drawing.Size(23, 19);
			Parental_Transparent.ToolTipText = "Transparent";
			ParentalShow.Location = new System.Drawing.Point(702, 2);
			ParentalShow.Name = "ParentalShow";
			ParentalShow.Size = new System.Drawing.Size(44, 22);
			ParentalShow.TabIndex = 24;
			ParentalShow.Text = "Show";
			ParentalShow.Click += new System.EventHandler(ParentalShow_Click);
			ParentalPrefix.Location = new System.Drawing.Point(409, 3);
			ParentalPrefix.Name = "ParentalPrefix";
			ParentalPrefix.ReadOnly = true;
			ParentalPrefix.Size = new System.Drawing.Size(114, 20);
			ParentalPrefix.TabIndex = 26;
			ParentalPrefix.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			cbMessageAlert.FormattingEnabled = true;
			cbMessageAlert.Location = new System.Drawing.Point(3, 2);
			cbMessageAlert.MaxDropDownItems = 4;
			cbMessageAlert.Name = "cbMessageAlert";
			cbMessageAlert.Size = new System.Drawing.Size(164, 21);
			cbMessageAlert.TabIndex = 27;
			cbMessageAlert.Enter += new System.EventHandler(cbMessageAlert_Enter);
			cbParentalAlert.FormattingEnabled = true;
			cbParentalAlert.Location = new System.Drawing.Point(524, 2);
			cbParentalAlert.MaxDropDownItems = 4;
			cbParentalAlert.Name = "cbParentalAlert";
			cbParentalAlert.Size = new System.Drawing.Size(80, 21);
			cbParentalAlert.TabIndex = 28;
			cbParentalAlert.Enter += new System.EventHandler(cbParentalAlert_Enter);
			btnClearHistoryMessage.FlatAppearance.BorderSize = 0;
			btnClearHistoryMessage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			btnClearHistoryMessage.Image = Resources.Delete;
			btnClearHistoryMessage.Location = new System.Drawing.Point(167, 2);
			btnClearHistoryMessage.Name = "btnClearHistoryMessage";
			btnClearHistoryMessage.Size = new System.Drawing.Size(23, 22);
			btnClearHistoryMessage.TabIndex = 29;
			btnClearHistoryMessage.Click += new System.EventHandler(btnClearHistoryMessage_Click);
			btnClearHistoryParental.FlatAppearance.BorderSize = 0;
			btnClearHistoryParental.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			btnClearHistoryParental.Image = Resources.Delete;
			btnClearHistoryParental.Location = new System.Drawing.Point(604, 1);
			btnClearHistoryParental.Name = "btnClearHistoryParental";
			btnClearHistoryParental.Size = new System.Drawing.Size(23, 22);
			btnClearHistoryParental.TabIndex = 30;
			btnClearHistoryParental.Click += new System.EventHandler(btnClearHistoryParental_Click);
			base.AcceptButton = MessageShow;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = BtnCancel;
			base.ClientSize = new System.Drawing.Size(750, 29);
			base.Controls.Add(cbParentalAlert);
			base.Controls.Add(cbMessageAlert);
			base.Controls.Add(ParentalPrefix);
			base.Controls.Add(panelParentalBtn);
			base.Controls.Add(ParentalShow);
			base.Controls.Add(panelMessageBtn);
			base.Controls.Add(BtnCancel);
			base.Controls.Add(BtnDismiss);
			base.Controls.Add(MessageShow);
			base.Controls.Add(btnClearHistoryMessage);
			base.Controls.Add(btnClearHistoryParental);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Name = "FrmSingleMonitorAlert";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmSingleMonitorAlert_FormClosing);
			base.Load += new System.EventHandler(FrmSingleMonitorAlert_Load);
			panelMessageBtn.ResumeLayout(false);
			ToolBarMessageFormat.ResumeLayout(false);
			ToolBarMessageFormat.PerformLayout();
			panelParentalBtn.ResumeLayout(false);
			ToolBarParentalFormat.ResumeLayout(false);
			ToolBarParentalFormat.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
