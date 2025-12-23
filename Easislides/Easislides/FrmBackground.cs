using Easislides.Properties;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace Easislides
{
	public class FrmBackground : Form
	{
		private IContainer components = null;

		private Button OkBtn;

		private Button Colour1Btn;

		private Button Colour2Btn;

		private Button CancelBtn;

		private PictureBox pictureBox0;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private Panel panel0;

		private Panel panel1;

		private PictureBox pictureBox1;

		private Panel panel2;

		private PictureBox pictureBox2;

		private Panel panel7;

		private PictureBox pictureBox7;

		private Panel panel6;

		private PictureBox pictureBox6;

		private Panel panel5;

		private PictureBox pictureBox5;

		private Panel panel4;

		private PictureBox pictureBox4;

		private Panel panel3;

		private PictureBox pictureBox3;

		private Panel panel8;

		private PictureBox pictureBox8;

		private CheckBox checkBoxColour2;

		private GroupBox groupBox4;

		private GroupBox groupBox3;

		private Panel panel9;

		private PictureBox pictureBox9;

		private Panel panel10;

		private PictureBox pictureBox10;

		private Button SwapBtn;

		private Panel panel11;

		private PictureBox pictureBox11;

		private ToolTip toolTip1;

		private Color InColour1;

		private Color InColour2;

		private int SelectedStyle;

		private int PicWidth = 80;

		private int PicHeight = 60;

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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBackground));
			OkBtn = new System.Windows.Forms.Button();
			Colour1Btn = new System.Windows.Forms.Button();
			Colour2Btn = new System.Windows.Forms.Button();
			CancelBtn = new System.Windows.Forms.Button();
			groupBox1 = new System.Windows.Forms.GroupBox();
			groupBox4 = new System.Windows.Forms.GroupBox();
			SwapBtn = new System.Windows.Forms.Button();
			checkBoxColour2 = new System.Windows.Forms.CheckBox();
			groupBox3 = new System.Windows.Forms.GroupBox();
			groupBox2 = new System.Windows.Forms.GroupBox();
			panel11 = new System.Windows.Forms.Panel();
			pictureBox11 = new System.Windows.Forms.PictureBox();
			panel9 = new System.Windows.Forms.Panel();
			pictureBox9 = new System.Windows.Forms.PictureBox();
			panel10 = new System.Windows.Forms.Panel();
			pictureBox10 = new System.Windows.Forms.PictureBox();
			panel8 = new System.Windows.Forms.Panel();
			pictureBox8 = new System.Windows.Forms.PictureBox();
			panel7 = new System.Windows.Forms.Panel();
			pictureBox7 = new System.Windows.Forms.PictureBox();
			panel6 = new System.Windows.Forms.Panel();
			pictureBox6 = new System.Windows.Forms.PictureBox();
			panel5 = new System.Windows.Forms.Panel();
			pictureBox5 = new System.Windows.Forms.PictureBox();
			panel4 = new System.Windows.Forms.Panel();
			pictureBox4 = new System.Windows.Forms.PictureBox();
			panel3 = new System.Windows.Forms.Panel();
			pictureBox3 = new System.Windows.Forms.PictureBox();
			panel2 = new System.Windows.Forms.Panel();
			pictureBox2 = new System.Windows.Forms.PictureBox();
			panel1 = new System.Windows.Forms.Panel();
			pictureBox1 = new System.Windows.Forms.PictureBox();
			panel0 = new System.Windows.Forms.Panel();
			pictureBox0 = new System.Windows.Forms.PictureBox();
			toolTip1 = new System.Windows.Forms.ToolTip(components);
			groupBox1.SuspendLayout();
			groupBox4.SuspendLayout();
			groupBox3.SuspendLayout();
			groupBox2.SuspendLayout();
			panel11.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox11).BeginInit();
			panel9.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox9).BeginInit();
			panel10.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox10).BeginInit();
			panel8.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox8).BeginInit();
			panel7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
			panel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
			panel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
			panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
			panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
			panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
			panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			panel0.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox0).BeginInit();
			SuspendLayout();
			OkBtn.Location = new System.Drawing.Point(212, 15);
			OkBtn.Name = "OkBtn";
			OkBtn.Size = new System.Drawing.Size(76, 25);
			OkBtn.TabIndex = 0;
			OkBtn.Text = "OK";
			OkBtn.Click += new System.EventHandler(OkBtn_Click);
			Colour1Btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Colour1Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			Colour1Btn.Location = new System.Drawing.Point(7, 9);
			Colour1Btn.Name = "Colour1Btn";
			Colour1Btn.Size = new System.Drawing.Size(61, 22);
			Colour1Btn.TabIndex = 0;
			Colour1Btn.Text = "Colour 1";
			Colour1Btn.MouseUp += new System.Windows.Forms.MouseEventHandler(Colour1_MouseUp);
			Colour2Btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Colour2Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			Colour2Btn.Location = new System.Drawing.Point(22, 9);
			Colour2Btn.Name = "Colour2Btn";
			Colour2Btn.Size = new System.Drawing.Size(61, 22);
			Colour2Btn.TabIndex = 1;
			Colour2Btn.Text = "Colour 2";
			Colour2Btn.MouseUp += new System.Windows.Forms.MouseEventHandler(Colour2_MouseUp);
			CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			CancelBtn.Location = new System.Drawing.Point(294, 15);
			CancelBtn.Name = "CancelBtn";
			CancelBtn.Size = new System.Drawing.Size(76, 25);
			CancelBtn.TabIndex = 1;
			CancelBtn.Text = "Cancel";
			groupBox1.Controls.Add(groupBox4);
			groupBox1.Controls.Add(groupBox3);
			groupBox1.Controls.Add(OkBtn);
			groupBox1.Controls.Add(CancelBtn);
			groupBox1.Location = new System.Drawing.Point(9, 241);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(384, 47);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox4.Controls.Add(SwapBtn);
			groupBox4.Controls.Add(checkBoxColour2);
			groupBox4.Controls.Add(Colour2Btn);
			groupBox4.Location = new System.Drawing.Point(86, 7);
			groupBox4.Name = "groupBox4";
			groupBox4.Size = new System.Drawing.Size(120, 34);
			groupBox4.TabIndex = 1;
			groupBox4.TabStop = false;
			SwapBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			SwapBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			SwapBtn.Image = Resources.SwapColours;
			SwapBtn.Location = new System.Drawing.Point(89, 9);
			SwapBtn.Name = "SwapBtn";
			SwapBtn.Size = new System.Drawing.Size(24, 22);
			SwapBtn.TabIndex = 2;
			toolTip1.SetToolTip(SwapBtn, "Swap Colour 1 with Colour 2");
			SwapBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(SwapBtn_MouseUp);
			checkBoxColour2.AutoSize = true;
			checkBoxColour2.Location = new System.Drawing.Point(4, 13);
			checkBoxColour2.Name = "checkBoxColour2";
			checkBoxColour2.Size = new System.Drawing.Size(15, 14);
			checkBoxColour2.TabIndex = 0;
			toolTip1.SetToolTip(checkBoxColour2, "Use Colour 2");
			checkBoxColour2.Click += new System.EventHandler(checkBoxColour2_Click);
			groupBox3.Controls.Add(Colour1Btn);
			groupBox3.Location = new System.Drawing.Point(6, 7);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new System.Drawing.Size(74, 34);
			groupBox3.TabIndex = 0;
			groupBox3.TabStop = false;
			groupBox2.Controls.Add(panel11);
			groupBox2.Controls.Add(panel9);
			groupBox2.Controls.Add(panel10);
			groupBox2.Controls.Add(panel8);
			groupBox2.Controls.Add(panel7);
			groupBox2.Controls.Add(panel6);
			groupBox2.Controls.Add(panel5);
			groupBox2.Controls.Add(panel4);
			groupBox2.Controls.Add(panel3);
			groupBox2.Controls.Add(panel2);
			groupBox2.Controls.Add(panel1);
			groupBox2.Controls.Add(panel0);
			groupBox2.Location = new System.Drawing.Point(9, 4);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new System.Drawing.Size(384, 233);
			groupBox2.TabIndex = 1;
			groupBox2.TabStop = false;
			groupBox2.Text = "Patterns";
			panel11.BackColor = System.Drawing.Color.Red;
			panel11.Controls.Add(pictureBox11);
			panel11.Location = new System.Drawing.Point(280, 157);
			panel11.Name = "panel11";
			panel11.Size = new System.Drawing.Size(86, 66);
			panel11.TabIndex = 11;
			pictureBox11.BackColor = System.Drawing.Color.White;
			pictureBox11.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox11.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox11.Location = new System.Drawing.Point(3, 3);
			pictureBox11.Name = "pictureBox11";
			pictureBox11.Size = new System.Drawing.Size(80, 60);
			pictureBox11.TabIndex = 4;
			pictureBox11.TabStop = false;
			pictureBox11.Tag = "11";
			pictureBox11.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox11.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel9.BackColor = System.Drawing.Color.Red;
			panel9.Controls.Add(pictureBox9);
			panel9.Location = new System.Drawing.Point(102, 157);
			panel9.Name = "panel9";
			panel9.Size = new System.Drawing.Size(86, 66);
			panel9.TabIndex = 9;
			pictureBox9.BackColor = System.Drawing.Color.White;
			pictureBox9.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox9.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox9.Location = new System.Drawing.Point(3, 3);
			pictureBox9.Name = "pictureBox9";
			pictureBox9.Size = new System.Drawing.Size(80, 60);
			pictureBox9.TabIndex = 4;
			pictureBox9.TabStop = false;
			pictureBox9.Tag = "9";
			pictureBox9.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox9.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel10.BackColor = System.Drawing.Color.Red;
			panel10.Controls.Add(pictureBox10);
			panel10.Location = new System.Drawing.Point(191, 157);
			panel10.Name = "panel10";
			panel10.Size = new System.Drawing.Size(86, 66);
			panel10.TabIndex = 10;
			pictureBox10.BackColor = System.Drawing.Color.White;
			pictureBox10.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox10.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox10.Location = new System.Drawing.Point(3, 3);
			pictureBox10.Name = "pictureBox10";
			pictureBox10.Size = new System.Drawing.Size(80, 60);
			pictureBox10.TabIndex = 4;
			pictureBox10.TabStop = false;
			pictureBox10.Tag = "10";
			pictureBox10.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox10.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel8.BackColor = System.Drawing.Color.Red;
			panel8.Controls.Add(pictureBox8);
			panel8.Location = new System.Drawing.Point(13, 157);
			panel8.Name = "panel8";
			panel8.Size = new System.Drawing.Size(86, 66);
			panel8.TabIndex = 8;
			pictureBox8.BackColor = System.Drawing.Color.White;
			pictureBox8.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox8.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox8.Location = new System.Drawing.Point(3, 3);
			pictureBox8.Name = "pictureBox8";
			pictureBox8.Size = new System.Drawing.Size(80, 60);
			pictureBox8.TabIndex = 4;
			pictureBox8.TabStop = false;
			pictureBox8.Tag = "8";
			pictureBox8.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox8.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel7.BackColor = System.Drawing.Color.Red;
			panel7.Controls.Add(pictureBox7);
			panel7.Location = new System.Drawing.Point(280, 88);
			panel7.Name = "panel7";
			panel7.Size = new System.Drawing.Size(86, 66);
			panel7.TabIndex = 7;
			pictureBox7.BackColor = System.Drawing.Color.White;
			pictureBox7.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox7.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox7.Location = new System.Drawing.Point(3, 3);
			pictureBox7.Name = "pictureBox7";
			pictureBox7.Size = new System.Drawing.Size(80, 60);
			pictureBox7.TabIndex = 4;
			pictureBox7.TabStop = false;
			pictureBox7.Tag = "7";
			pictureBox7.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox7.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel6.BackColor = System.Drawing.Color.Red;
			panel6.Controls.Add(pictureBox6);
			panel6.Location = new System.Drawing.Point(191, 88);
			panel6.Name = "panel6";
			panel6.Size = new System.Drawing.Size(86, 66);
			panel6.TabIndex = 6;
			pictureBox6.BackColor = System.Drawing.Color.White;
			pictureBox6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox6.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox6.Location = new System.Drawing.Point(3, 3);
			pictureBox6.Name = "pictureBox6";
			pictureBox6.Size = new System.Drawing.Size(80, 60);
			pictureBox6.TabIndex = 4;
			pictureBox6.TabStop = false;
			pictureBox6.Tag = "6";
			pictureBox6.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox6.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel5.BackColor = System.Drawing.Color.Red;
			panel5.Controls.Add(pictureBox5);
			panel5.Location = new System.Drawing.Point(102, 88);
			panel5.Name = "panel5";
			panel5.Size = new System.Drawing.Size(86, 66);
			panel5.TabIndex = 5;
			pictureBox5.BackColor = System.Drawing.Color.White;
			pictureBox5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox5.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox5.Location = new System.Drawing.Point(3, 3);
			pictureBox5.Name = "pictureBox5";
			pictureBox5.Size = new System.Drawing.Size(80, 60);
			pictureBox5.TabIndex = 4;
			pictureBox5.TabStop = false;
			pictureBox5.Tag = "5";
			pictureBox5.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox5.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel4.BackColor = System.Drawing.Color.Red;
			panel4.Controls.Add(pictureBox4);
			panel4.Location = new System.Drawing.Point(13, 88);
			panel4.Name = "panel4";
			panel4.Size = new System.Drawing.Size(86, 66);
			panel4.TabIndex = 4;
			pictureBox4.BackColor = System.Drawing.Color.White;
			pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox4.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox4.Location = new System.Drawing.Point(3, 3);
			pictureBox4.Name = "pictureBox4";
			pictureBox4.Size = new System.Drawing.Size(80, 60);
			pictureBox4.TabIndex = 4;
			pictureBox4.TabStop = false;
			pictureBox4.Tag = "4";
			pictureBox4.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox4.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel3.BackColor = System.Drawing.Color.Red;
			panel3.Controls.Add(pictureBox3);
			panel3.Location = new System.Drawing.Point(280, 19);
			panel3.Name = "panel3";
			panel3.Size = new System.Drawing.Size(86, 66);
			panel3.TabIndex = 3;
			pictureBox3.BackColor = System.Drawing.Color.White;
			pictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox3.Location = new System.Drawing.Point(3, 3);
			pictureBox3.Name = "pictureBox3";
			pictureBox3.Size = new System.Drawing.Size(80, 60);
			pictureBox3.TabIndex = 4;
			pictureBox3.TabStop = false;
			pictureBox3.Tag = "3";
			pictureBox3.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox3.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel2.BackColor = System.Drawing.Color.Red;
			panel2.Controls.Add(pictureBox2);
			panel2.Location = new System.Drawing.Point(191, 19);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(86, 66);
			panel2.TabIndex = 2;
			pictureBox2.BackColor = System.Drawing.Color.White;
			pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox2.Location = new System.Drawing.Point(3, 3);
			pictureBox2.Name = "pictureBox2";
			pictureBox2.Size = new System.Drawing.Size(80, 60);
			pictureBox2.TabIndex = 4;
			pictureBox2.TabStop = false;
			pictureBox2.Tag = "2";
			pictureBox2.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox2.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel1.BackColor = System.Drawing.Color.Red;
			panel1.Controls.Add(pictureBox1);
			panel1.Location = new System.Drawing.Point(102, 19);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(86, 66);
			panel1.TabIndex = 1;
			pictureBox1.BackColor = System.Drawing.Color.White;
			pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox1.Location = new System.Drawing.Point(3, 3);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(80, 60);
			pictureBox1.TabIndex = 4;
			pictureBox1.TabStop = false;
			pictureBox1.Tag = "1";
			pictureBox1.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			panel0.BackColor = System.Drawing.Color.Red;
			panel0.Controls.Add(pictureBox0);
			panel0.Location = new System.Drawing.Point(13, 19);
			panel0.Name = "panel0";
			panel0.Size = new System.Drawing.Size(86, 66);
			panel0.TabIndex = 0;
			pictureBox0.BackColor = System.Drawing.Color.White;
			pictureBox0.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			pictureBox0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			pictureBox0.Cursor = System.Windows.Forms.Cursors.Hand;
			pictureBox0.Location = new System.Drawing.Point(3, 3);
			pictureBox0.Name = "pictureBox0";
			pictureBox0.Size = new System.Drawing.Size(80, 60);
			pictureBox0.TabIndex = 4;
			pictureBox0.TabStop = false;
			pictureBox0.Tag = "0";
			pictureBox0.DoubleClick += new System.EventHandler(pictureBoxAll_DoubleClick);
			pictureBox0.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBoxAll_MouseDown);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(402, 298);
			base.Controls.Add(groupBox2);
			base.Controls.Add(groupBox1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmBackground";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Background Colours and Patterns";
			groupBox1.ResumeLayout(false);
			groupBox4.ResumeLayout(false);
			groupBox4.PerformLayout();
			groupBox3.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			panel11.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox11).EndInit();
			panel9.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox9).EndInit();
			panel10.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox10).EndInit();
			panel8.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox8).EndInit();
			panel7.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
			panel6.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
			panel5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
			panel4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
			panel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
			panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
			panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			panel0.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox0).EndInit();
			ResumeLayout(false);
		}

		public FrmBackground()
		{
			InitializeComponent();
			LoadData();
		}

		private void LoadData()
		{
			Text = (gf.ChangedIsDefault ? "Select Default Background Colours and Pattern" : "Background and Pattern for selected item");
			InColour1 = gf.ChangedBackColour1;
			InColour2 = gf.ChangedBackColour2;
			SelectedStyle = gf.ChangedBackStyle;
			PicWidth = pictureBox0.Width;
			PicHeight = pictureBox0.Height;
			Colour1Btn.ForeColor = InColour1;
			Colour2Btn.ForeColor = InColour2;
			checkBoxColour2.Checked = ((!(InColour1 == InColour2)) ? true : false);
			Colour2Btn.Enabled = checkBoxColour2.Checked;
			SwapBtn.Enabled = Colour2Btn.Enabled;
			DrawAllPatterns();
		}

		private void DrawAllPatterns()
		{
			DrawPattern(pictureBox0, panel0);
			DrawPattern(pictureBox1, panel1);
			DrawPattern(pictureBox2, panel2);
			DrawPattern(pictureBox3, panel3);
			DrawPattern(pictureBox4, panel4);
			DrawPattern(pictureBox5, panel5);
			DrawPattern(pictureBox6, panel6);
			DrawPattern(pictureBox7, panel7);
			DrawPattern(pictureBox8, panel8);
			DrawPattern(pictureBox9, panel9);
			DrawPattern(pictureBox10, panel10);
			DrawPattern(pictureBox11, panel11);
			SetBorder(SelectedStyle);
		}

		private void DrawPattern(PictureBox InPictureBox, Panel InPanel)
		{
			string BackgroundID = "";
			int inStyle = DataUtil.StringToInt((string)InPictureBox.Tag);
			Image image = new Bitmap(PicWidth, PicHeight);
			Graphics g = Graphics.FromImage(image);
			gf.BackPattern.Fill(ref g, InColour1, InColour2, inStyle, PicWidth, PicHeight, ref BackgroundID);
			InPictureBox.BackgroundImage = image;
			InPanel.BackColor = BackColor;
			//image.Dispose();
			//g.Dispose();
		}

		private void SetBorder(int InStyle)
		{
			panel0.BackColor = ((DataUtil.StringToInt((string)pictureBox0.Tag) == InStyle) ? Color.Red : BackColor);
			panel1.BackColor = ((DataUtil.StringToInt((string)pictureBox1.Tag) == InStyle) ? Color.Red : BackColor);
			panel2.BackColor = ((DataUtil.StringToInt((string)pictureBox2.Tag) == InStyle) ? Color.Red : BackColor);
			panel3.BackColor = ((DataUtil.StringToInt((string)pictureBox3.Tag) == InStyle) ? Color.Red : BackColor);
			panel4.BackColor = ((DataUtil.StringToInt((string)pictureBox4.Tag) == InStyle) ? Color.Red : BackColor);
			panel5.BackColor = ((DataUtil.StringToInt((string)pictureBox5.Tag) == InStyle) ? Color.Red : BackColor);
			panel6.BackColor = ((DataUtil.StringToInt((string)pictureBox6.Tag) == InStyle) ? Color.Red : BackColor);
			panel7.BackColor = ((DataUtil.StringToInt((string)pictureBox7.Tag) == InStyle) ? Color.Red : BackColor);
			panel8.BackColor = ((DataUtil.StringToInt((string)pictureBox8.Tag) == InStyle) ? Color.Red : BackColor);
			panel9.BackColor = ((DataUtil.StringToInt((string)pictureBox9.Tag) == InStyle) ? Color.Red : BackColor);
			panel10.BackColor = ((DataUtil.StringToInt((string)pictureBox10.Tag) == InStyle) ? Color.Red : BackColor);
			panel11.BackColor = ((DataUtil.StringToInt((string)pictureBox11.Tag) == InStyle) ? Color.Red : BackColor);
		}

		private void Colour1_MouseUp(object sender, MouseEventArgs e)
		{
			if (gf.SelectColorFromBtn(ref Colour1Btn, ref InColour1))
			{
				if (!checkBoxColour2.Checked)
				{
					SetColour2As1();
				}
				DrawAllPatterns();
			}
		}

		private void Colour2_MouseUp(object sender, MouseEventArgs e)
		{
			if (gf.SelectColorFromBtn(ref Colour2Btn, ref InColour2))
			{
				DrawAllPatterns();
			}
		}

		private void pictureBoxAll_MouseDown(object sender, MouseEventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			SelectedStyle = DataUtil.StringToInt((string)pictureBox.Tag);
			SetBorder(SelectedStyle);
		}

		private void checkBoxColour2_Click(object sender, EventArgs e)
		{
			SetColour2As1();
			DrawAllPatterns();
		}

		private void SetColour2As1()
		{
			Colour2Btn.Enabled = checkBoxColour2.Checked;
			Colour2Btn.ForeColor = Colour1Btn.ForeColor;
			InColour2 = Colour2Btn.ForeColor;
			SwapBtn.Enabled = Colour2Btn.Enabled;
		}

		private void pictureBoxAll_DoubleClick(object sender, EventArgs e)
		{
			UseNewSettingsAndExit();
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			UseNewSettingsAndExit();
		}

		private void UseNewSettingsAndExit()
		{
			gf.ChangedBackColour1 = InColour1;
			gf.ChangedBackColour2 = InColour2;
			gf.ChangedBackStyle = SelectedStyle;
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void SwapBtn_MouseUp(object sender, MouseEventArgs e)
		{
			if (Colour2Btn.Enabled)
			{
				InColour1 = Colour2Btn.ForeColor;
				InColour2 = Colour1Btn.ForeColor;
				Colour1Btn.ForeColor = InColour1;
				Colour2Btn.ForeColor = InColour2;
				DrawAllPatterns();
			}
		}
	}
}
