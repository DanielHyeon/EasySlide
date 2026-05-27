using Easislides.Module;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
    partial class FrmLaunchMediaPlayer
    {
		private IContainer components = null;
		private System.Windows.Forms.Timer TimerAttemptConnect;
		private System.Windows.Forms.Timer TimerRefresh;

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
			TimerAttemptConnect = new System.Windows.Forms.Timer(components);
			TimerRefresh = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			TimerRefresh.Enabled = true;
			TimerRefresh.Interval = 500;
			//TimerRefresh.Tick += new System.EventHandler(TimerRefresh_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.Black;
			base.ClientSize = new System.Drawing.Size(125, 110);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Name = "FrmLaunchMediaPlayer";
			base.ShowInTaskbar = false;
			base.VisibleChanged += new System.EventHandler(FrmLaunchMediaPlayer_VisibleChanged);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLaunchMediaPlayer_FormClosing);
			base.Load += new System.EventHandler(FrmLaunchMediaPlayer_Load);
			ResumeLayout(false);
		}

        #endregion
    }
}
