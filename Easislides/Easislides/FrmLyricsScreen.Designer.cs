using Easislides.Module;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
    partial class FrmLyricsScreen
    {
		private ColumnHeader columnHeader10;
		private ColumnHeader columnHeader11;
		private ColumnHeader columnHeader12;
		private ColumnHeader columnHeader13;
		private ColumnHeader columnHeader14;
		private ColumnHeader columnHeader8;
		private ColumnHeader columnHeader9;
		private IContainer components = null;
		private ImageList imageListSys;
		private ListView WorshipListItems;
		private Panel panelBottom;
		private Panel panelLeft;
		private Panel panelRight;
		private Panel panelTop;
		private RichTextBox LyricsAlertTextBox;
		private RichTextBox OutputLyrics;
		private RichTextBox PreviewLyrics;
		private SplitContainer splitContainer1;
		private SplitContainer splitContainer2;
		private SplitContainer splitContainer3;
		private System.Windows.Forms.Timer timerLyricsAlert;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLyricsScreen));
			OutputLyrics = new System.Windows.Forms.RichTextBox();
			panelTop = new System.Windows.Forms.Panel();
			panelBottom = new System.Windows.Forms.Panel();
			splitContainer1 = new System.Windows.Forms.SplitContainer();
			splitContainer2 = new System.Windows.Forms.SplitContainer();
			WorshipListItems = new System.Windows.Forms.ListView();
			columnHeader8 = new System.Windows.Forms.ColumnHeader();
			columnHeader9 = new System.Windows.Forms.ColumnHeader();
			columnHeader10 = new System.Windows.Forms.ColumnHeader();
			columnHeader11 = new System.Windows.Forms.ColumnHeader();
			columnHeader12 = new System.Windows.Forms.ColumnHeader();
			columnHeader13 = new System.Windows.Forms.ColumnHeader();
			columnHeader14 = new System.Windows.Forms.ColumnHeader();
			imageListSys = new System.Windows.Forms.ImageList(components);
			splitContainer3 = new System.Windows.Forms.SplitContainer();
			PreviewLyrics = new System.Windows.Forms.RichTextBox();
			LyricsAlertTextBox = new System.Windows.Forms.RichTextBox();
			panelLeft = new System.Windows.Forms.Panel();
			panelRight = new System.Windows.Forms.Panel();
			timerLyricsAlert = new System.Windows.Forms.Timer(components);
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			splitContainer2.Panel1.SuspendLayout();
			splitContainer2.Panel2.SuspendLayout();
			splitContainer2.SuspendLayout();
			splitContainer3.Panel1.SuspendLayout();
			splitContainer3.Panel2.SuspendLayout();
			splitContainer3.SuspendLayout();
			SuspendLayout();
			OutputLyrics.BackColor = System.Drawing.SystemColors.Window;
			OutputLyrics.Dock = System.Windows.Forms.DockStyle.Fill;
			OutputLyrics.Location = new System.Drawing.Point(0, 0);
			OutputLyrics.Name = "OutputLyrics";
			OutputLyrics.ReadOnly = true;
			OutputLyrics.ShowSelectionMargin = true;
			OutputLyrics.Size = new System.Drawing.Size(148, 162);
			OutputLyrics.TabIndex = 1;
			OutputLyrics.Text = "";
			panelTop.Dock = System.Windows.Forms.DockStyle.Top;
			panelTop.Location = new System.Drawing.Point(0, 0);
			panelTop.Name = "panelTop";
			panelTop.Size = new System.Drawing.Size(220, 3);
			panelTop.TabIndex = 6;
			panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			panelBottom.Location = new System.Drawing.Point(0, 165);
			panelBottom.Name = "panelBottom";
			panelBottom.Size = new System.Drawing.Size(220, 3);
			panelBottom.TabIndex = 7;
			splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer1.Location = new System.Drawing.Point(3, 3);
			splitContainer1.Name = "splitContainer1";
			splitContainer1.Panel1.Controls.Add(splitContainer2);
			splitContainer1.Panel2.Controls.Add(OutputLyrics);
			splitContainer1.Size = new System.Drawing.Size(214, 162);
			splitContainer1.SplitterDistance = 63;
			splitContainer1.SplitterWidth = 3;
			splitContainer1.TabIndex = 8;
			splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer1_SplitterMoved);
			splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer2.Location = new System.Drawing.Point(0, 0);
			splitContainer2.Name = "splitContainer2";
			splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainer2.Panel1.Controls.Add(WorshipListItems);
			splitContainer2.Panel2.Controls.Add(splitContainer3);
			splitContainer2.Size = new System.Drawing.Size(63, 162);
			splitContainer2.SplitterDistance = 45;
			splitContainer2.SplitterWidth = 3;
			splitContainer2.TabIndex = 9;
			splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer2_SplitterMoved);
			WorshipListItems.AllowDrop = true;
			WorshipListItems.BackColor = System.Drawing.SystemColors.Window;
			WorshipListItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[7]
			{
				columnHeader8,
				columnHeader9,
				columnHeader10,
				columnHeader11,
				columnHeader12,
				columnHeader13,
				columnHeader14
			});
			WorshipListItems.Dock = System.Windows.Forms.DockStyle.Fill;
			WorshipListItems.FullRowSelect = true;
			WorshipListItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			WorshipListItems.HideSelection = false;
			WorshipListItems.LabelWrap = false;
			WorshipListItems.Location = new System.Drawing.Point(0, 0);
			WorshipListItems.MultiSelect = false;
			WorshipListItems.Name = "WorshipListItems";
			WorshipListItems.Size = new System.Drawing.Size(63, 45);
			WorshipListItems.SmallImageList = imageListSys;
			WorshipListItems.TabIndex = 2;
			WorshipListItems.UseCompatibleStateImageBehavior = false;
			WorshipListItems.View = System.Windows.Forms.View.Details;
			WorshipListItems.Resize += new System.EventHandler(WorshipListItems_Resize);
			columnHeader9.Width = 0;
			columnHeader10.Width = 0;
			columnHeader11.Width = 0;
			columnHeader12.Width = 0;
			columnHeader13.Width = 0;
			columnHeader14.Width = 0;
			imageListSys.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListSys.ImageStream");
			imageListSys.TransparentColor = System.Drawing.Color.Transparent;
			imageListSys.Images.SetKeyName(0, "ES Icon 32 Blue.ico");
			imageListSys.Images.SetKeyName(1, "ES Icon 32 Blue - Highlight.ico");
			imageListSys.Images.SetKeyName(2, "PPImg.gif");
			imageListSys.Images.SetKeyName(3, "PPImg - Highlight.gif");
			imageListSys.Images.SetKeyName(4, "Bible.gif");
			imageListSys.Images.SetKeyName(5, "Bible - Hightlight.gif");
			imageListSys.Images.SetKeyName(6, "notebook.gif");
			imageListSys.Images.SetKeyName(7, "notebook-highlight.gif");
			imageListSys.Images.SetKeyName(8, "Info_Sym.gif");
			imageListSys.Images.SetKeyName(9, "Info_Sym highlight.gif");
			imageListSys.Images.SetKeyName(10, "word.gif");
			imageListSys.Images.SetKeyName(11, "word-highlight.gif");
			imageListSys.Images.SetKeyName(12, "singlescreen.gif");
			imageListSys.Images.SetKeyName(13, "dualscreens.gif");
			imageListSys.Images.SetKeyName(14, "keyboard.gif");
			imageListSys.Images.SetKeyName(15, "BlackScreen-Pressed.gif");
			imageListSys.Images.SetKeyName(16, "BlackScreen-Red.gif");
			imageListSys.Images.SetKeyName(17, "BlueScreen-Pressed.gif");
			imageListSys.Images.SetKeyName(18, "BlueScreen-Red.gif");
			imageListSys.Images.SetKeyName(19, "folder.gif");
			imageListSys.Images.SetKeyName(20, "pic-bestfit.gif");
			imageListSys.Images.SetKeyName(21, "Bible.gif");
			imageListSys.Images.SetKeyName(22, "options.gif");
			imageListSys.Images.SetKeyName(23, "Info_Sym.gif");
			imageListSys.Images.SetKeyName(24, "PPImg.gif");
			imageListSys.Images.SetKeyName(25, "Tick.gif");
			imageListSys.Images.SetKeyName(26, "NumNewScreen.gif");
			imageListSys.Images.SetKeyName(27, "ques.gif");
			imageListSys.Images.SetKeyName(28, "Media.gif");
			imageListSys.Images.SetKeyName(29, "Media-highlight.gif");
			splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer3.Location = new System.Drawing.Point(0, 0);
			splitContainer3.Name = "splitContainer3";
			splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainer3.Panel1.Controls.Add(PreviewLyrics);
			splitContainer3.Panel2.Controls.Add(LyricsAlertTextBox);
			splitContainer3.Size = new System.Drawing.Size(63, 114);
			splitContainer3.SplitterDistance = 72;
			splitContainer3.SplitterWidth = 3;
			splitContainer3.TabIndex = 11;
			splitContainer3.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer3_SplitterMoved);
			PreviewLyrics.BackColor = System.Drawing.SystemColors.Window;
			PreviewLyrics.Dock = System.Windows.Forms.DockStyle.Fill;
			PreviewLyrics.Location = new System.Drawing.Point(0, 0);
			PreviewLyrics.Name = "PreviewLyrics";
			PreviewLyrics.ReadOnly = true;
			PreviewLyrics.Size = new System.Drawing.Size(63, 72);
			PreviewLyrics.TabIndex = 2;
			PreviewLyrics.Text = "";
			LyricsAlertTextBox.BackColor = System.Drawing.SystemColors.Window;
			LyricsAlertTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			LyricsAlertTextBox.Location = new System.Drawing.Point(0, 0);
			LyricsAlertTextBox.Name = "LyricsAlertTextBox";
			LyricsAlertTextBox.ReadOnly = true;
			LyricsAlertTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			LyricsAlertTextBox.ShowSelectionMargin = true;
			LyricsAlertTextBox.Size = new System.Drawing.Size(63, 39);
			LyricsAlertTextBox.TabIndex = 2;
			LyricsAlertTextBox.Text = "";
			panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
			panelLeft.Location = new System.Drawing.Point(0, 3);
			panelLeft.Name = "panelLeft";
			panelLeft.Size = new System.Drawing.Size(3, 162);
			panelLeft.TabIndex = 9;
			panelRight.Dock = System.Windows.Forms.DockStyle.Right;
			panelRight.Location = new System.Drawing.Point(217, 3);
			panelRight.Name = "panelRight";
			panelRight.Size = new System.Drawing.Size(3, 162);
			panelRight.TabIndex = 10;
			timerLyricsAlert.Interval = 500;
			timerLyricsAlert.Tick += new System.EventHandler(timerLyricsAlert_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(220, 168);
			base.ControlBox = false;
			base.Controls.Add(splitContainer1);
			base.Controls.Add(panelLeft);
			base.Controls.Add(panelRight);
			base.Controls.Add(panelBottom);
			base.Controls.Add(panelTop);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmLyricsScreen";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			Text = "Lyrics Monitor";
			base.Load += new System.EventHandler(FrmLyricsScreen_Load);
			base.VisibleChanged += new System.EventHandler(FrmLyricsScreen_VisibleChanged);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLyricsScreen_FormClosing);
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			splitContainer1.ResumeLayout(false);
			splitContainer2.Panel1.ResumeLayout(false);
			splitContainer2.Panel2.ResumeLayout(false);
			splitContainer2.ResumeLayout(false);
			splitContainer3.Panel1.ResumeLayout(false);
			splitContainer3.Panel2.ResumeLayout(false);
			splitContainer3.ResumeLayout(false);
			ResumeLayout(false);
		}

        #endregion
    }
}
