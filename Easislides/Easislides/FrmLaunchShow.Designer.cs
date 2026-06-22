using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;
using OfficeLib;

namespace Easislides
{
    partial class FrmLaunchShow
    {
		private IContainer components = null;
		private ImageList imageList1;
		private Timer TimerMouseDown;
		private Timer TimerOpacity;
		private Timer TimerRemote;
		private Timer TimerRotate;
		private Timer TimerSingleScreen;
		private Timer TimerToFront;

protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code

private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLaunchShow));
			TimerSingleScreen = new System.Windows.Forms.Timer(components);
			TimerRemote = new System.Windows.Forms.Timer(components);
			TimerMouseDown = new System.Windows.Forms.Timer(components);
			TimerOpacity = new System.Windows.Forms.Timer(components);
			TimerRotate = new System.Windows.Forms.Timer(components);
			imageList1 = new System.Windows.Forms.ImageList(components);
			TimerToFront = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			TimerSingleScreen.Tick += new System.EventHandler(TimerSingleScreen_Tick);
			TimerRemote.Tick += new System.EventHandler(TimerRemote_Tick);
			TimerMouseDown.Enabled = true;
			TimerMouseDown.Interval = 200;
			TimerMouseDown.Tick += new System.EventHandler(TimerMouseDown_Tick);
			TimerOpacity.Tick += new System.EventHandler(TimerOpacity_Tick);
			TimerRotate.Interval = 500;
			TimerRotate.Tick += new System.EventHandler(TimerRotate_Tick);
			imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
			imageList1.TransparentColor = System.Drawing.Color.Transparent;
			imageList1.Images.SetKeyName(0, "Blank.gif");
			TimerToFront.Enabled = true;
			TimerToFront.Tick += new System.EventHandler(TimerToFront_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.Black;
			base.ClientSize = new System.Drawing.Size(102, 72);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.KeyPreview = true;
			base.Name = "FrmLaunchShow";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			Text = "Live";
			base.TopMost = true;
			base.TransparencyKey = System.Drawing.Color.FromArgb(128, 64, 192);
			base.Load += new System.EventHandler(FrmLaunchShow_Load);
			base.Enter += new System.EventHandler(FrmLaunchShow_Enter);
			base.VisibleChanged += new System.EventHandler(FrmLaunchShow_VisibleChanged);
			base.Leave += new System.EventHandler(FrmLaunchShow_Leave);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLaunchShow_FormClosing);
			ResumeLayout(false);
		}

        #endregion
    }
}
