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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmBackground));
            OkBtn = new Button();
            Colour1Btn = new Button();
            Colour2Btn = new Button();
            CancelBtn = new Button();
            groupBox1 = new GroupBox();
            groupBox4 = new GroupBox();
            SwapBtn = new Button();
            checkBoxColour2 = new CheckBox();
            groupBox3 = new GroupBox();
            groupBox2 = new GroupBox();
            panel11 = new Panel();
            pictureBox11 = new PictureBox();
            panel9 = new Panel();
            pictureBox9 = new PictureBox();
            panel10 = new Panel();
            pictureBox10 = new PictureBox();
            panel8 = new Panel();
            pictureBox8 = new PictureBox();
            panel7 = new Panel();
            pictureBox7 = new PictureBox();
            panel6 = new Panel();
            pictureBox6 = new PictureBox();
            panel5 = new Panel();
            pictureBox5 = new PictureBox();
            panel4 = new Panel();
            pictureBox4 = new PictureBox();
            panel3 = new Panel();
            pictureBox3 = new PictureBox();
            panel2 = new Panel();
            pictureBox2 = new PictureBox();
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            panel0 = new Panel();
            pictureBox0 = new PictureBox();
            toolTip1 = new ToolTip(components);
            groupBox1.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            panel11.SuspendLayout();
            ((ISupportInitialize)pictureBox11).BeginInit();
            panel9.SuspendLayout();
            ((ISupportInitialize)pictureBox9).BeginInit();
            panel10.SuspendLayout();
            ((ISupportInitialize)pictureBox10).BeginInit();
            panel8.SuspendLayout();
            ((ISupportInitialize)pictureBox8).BeginInit();
            panel7.SuspendLayout();
            ((ISupportInitialize)pictureBox7).BeginInit();
            panel6.SuspendLayout();
            ((ISupportInitialize)pictureBox6).BeginInit();
            panel5.SuspendLayout();
            ((ISupportInitialize)pictureBox5).BeginInit();
            panel4.SuspendLayout();
            ((ISupportInitialize)pictureBox4).BeginInit();
            panel3.SuspendLayout();
            ((ISupportInitialize)pictureBox3).BeginInit();
            panel2.SuspendLayout();
            ((ISupportInitialize)pictureBox2).BeginInit();
            panel1.SuspendLayout();
            ((ISupportInitialize)pictureBox1).BeginInit();
            panel0.SuspendLayout();
            ((ISupportInitialize)pictureBox0).BeginInit();
            SuspendLayout();
            // 
            // OkBtn
            // 
            OkBtn.Location = new Point(283, 23);
            OkBtn.Margin = new Padding(4, 5, 4, 5);
            OkBtn.Name = "OkBtn";
            OkBtn.Size = new Size(101, 38);
            OkBtn.TabIndex = 0;
            OkBtn.Text = "OK";
            OkBtn.Click += OkBtn_Click;
            // 
            // Colour1Btn
            // 
            Colour1Btn.FlatStyle = FlatStyle.Flat;
            Colour1Btn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Colour1Btn.Location = new Point(9, 14);
            Colour1Btn.Margin = new Padding(4, 5, 4, 5);
            Colour1Btn.Name = "Colour1Btn";
            Colour1Btn.Size = new Size(81, 34);
            Colour1Btn.TabIndex = 0;
            Colour1Btn.Text = "Colour 1";
            Colour1Btn.MouseUp += Colour1_MouseUp;
            // 
            // Colour2Btn
            // 
            Colour2Btn.FlatStyle = FlatStyle.Flat;
            Colour2Btn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Colour2Btn.Location = new Point(29, 14);
            Colour2Btn.Margin = new Padding(4, 5, 4, 5);
            Colour2Btn.Name = "Colour2Btn";
            Colour2Btn.Size = new Size(81, 34);
            Colour2Btn.TabIndex = 1;
            Colour2Btn.Text = "Colour 2";
            Colour2Btn.MouseUp += Colour2_MouseUp;
            // 
            // CancelBtn
            // 
            CancelBtn.DialogResult = DialogResult.Cancel;
            CancelBtn.Location = new Point(392, 23);
            CancelBtn.Margin = new Padding(4, 5, 4, 5);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(101, 38);
            CancelBtn.TabIndex = 1;
            CancelBtn.Text = "Cancel";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(groupBox4);
            groupBox1.Controls.Add(groupBox3);
            groupBox1.Controls.Add(OkBtn);
            groupBox1.Controls.Add(CancelBtn);
            groupBox1.Location = new Point(12, 371);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(512, 72);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(SwapBtn);
            groupBox4.Controls.Add(checkBoxColour2);
            groupBox4.Controls.Add(Colour2Btn);
            groupBox4.Location = new Point(115, 11);
            groupBox4.Margin = new Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 5, 4, 5);
            groupBox4.Size = new Size(160, 52);
            groupBox4.TabIndex = 1;
            groupBox4.TabStop = false;
            // 
            // SwapBtn
            // 
            SwapBtn.FlatStyle = FlatStyle.Flat;
            SwapBtn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            SwapBtn.Image = Resources.SwapColours;
            SwapBtn.Location = new Point(119, 14);
            SwapBtn.Margin = new Padding(4, 5, 4, 5);
            SwapBtn.Name = "SwapBtn";
            SwapBtn.Size = new Size(32, 34);
            SwapBtn.TabIndex = 2;
            toolTip1.SetToolTip(SwapBtn, "Swap Colour 1 with Colour 2");
            SwapBtn.MouseUp += SwapBtn_MouseUp;
            // 
            // checkBoxColour2
            // 
            checkBoxColour2.AutoSize = true;
            checkBoxColour2.Location = new Point(5, 20);
            checkBoxColour2.Margin = new Padding(4, 5, 4, 5);
            checkBoxColour2.Name = "checkBoxColour2";
            checkBoxColour2.Size = new Size(18, 17);
            checkBoxColour2.TabIndex = 0;
            toolTip1.SetToolTip(checkBoxColour2, "Use Colour 2");
            checkBoxColour2.Click += checkBoxColour2_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(Colour1Btn);
            groupBox3.Location = new Point(8, 11);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(99, 52);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            // 
            // groupBox2
            // 
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
            groupBox2.Location = new Point(12, 6);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(512, 358);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Patterns";
            // 
            // panel11
            // 
            panel11.BackColor = Color.Red;
            panel11.Controls.Add(pictureBox11);
            panel11.Location = new Point(373, 242);
            panel11.Margin = new Padding(4, 5, 4, 5);
            panel11.Name = "panel11";
            panel11.Size = new Size(115, 102);
            panel11.TabIndex = 11;
            // 
            // pictureBox11
            // 
            pictureBox11.BackColor = Color.White;
            pictureBox11.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox11.BorderStyle = BorderStyle.Fixed3D;
            pictureBox11.Cursor = Cursors.Hand;
            pictureBox11.Location = new Point(4, 5);
            pictureBox11.Margin = new Padding(4, 5, 4, 5);
            pictureBox11.Name = "pictureBox11";
            pictureBox11.Size = new Size(105, 90);
            pictureBox11.TabIndex = 4;
            pictureBox11.TabStop = false;
            pictureBox11.Tag = "11";
            pictureBox11.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox11.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel9
            // 
            panel9.BackColor = Color.Red;
            panel9.Controls.Add(pictureBox9);
            panel9.Location = new Point(136, 242);
            panel9.Margin = new Padding(4, 5, 4, 5);
            panel9.Name = "panel9";
            panel9.Size = new Size(115, 102);
            panel9.TabIndex = 9;
            // 
            // pictureBox9
            // 
            pictureBox9.BackColor = Color.White;
            pictureBox9.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox9.BorderStyle = BorderStyle.Fixed3D;
            pictureBox9.Cursor = Cursors.Hand;
            pictureBox9.Location = new Point(4, 5);
            pictureBox9.Margin = new Padding(4, 5, 4, 5);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(105, 90);
            pictureBox9.TabIndex = 4;
            pictureBox9.TabStop = false;
            pictureBox9.Tag = "9";
            pictureBox9.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox9.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel10
            // 
            panel10.BackColor = Color.Red;
            panel10.Controls.Add(pictureBox10);
            panel10.Location = new Point(255, 242);
            panel10.Margin = new Padding(4, 5, 4, 5);
            panel10.Name = "panel10";
            panel10.Size = new Size(115, 102);
            panel10.TabIndex = 10;
            // 
            // pictureBox10
            // 
            pictureBox10.BackColor = Color.White;
            pictureBox10.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox10.BorderStyle = BorderStyle.Fixed3D;
            pictureBox10.Cursor = Cursors.Hand;
            pictureBox10.Location = new Point(4, 5);
            pictureBox10.Margin = new Padding(4, 5, 4, 5);
            pictureBox10.Name = "pictureBox10";
            pictureBox10.Size = new Size(105, 90);
            pictureBox10.TabIndex = 4;
            pictureBox10.TabStop = false;
            pictureBox10.Tag = "10";
            pictureBox10.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox10.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel8
            // 
            panel8.BackColor = Color.Red;
            panel8.Controls.Add(pictureBox8);
            panel8.Location = new Point(17, 242);
            panel8.Margin = new Padding(4, 5, 4, 5);
            panel8.Name = "panel8";
            panel8.Size = new Size(115, 102);
            panel8.TabIndex = 8;
            // 
            // pictureBox8
            // 
            pictureBox8.BackColor = Color.White;
            pictureBox8.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox8.BorderStyle = BorderStyle.Fixed3D;
            pictureBox8.Cursor = Cursors.Hand;
            pictureBox8.Location = new Point(4, 5);
            pictureBox8.Margin = new Padding(4, 5, 4, 5);
            pictureBox8.Name = "pictureBox8";
            pictureBox8.Size = new Size(105, 90);
            pictureBox8.TabIndex = 4;
            pictureBox8.TabStop = false;
            pictureBox8.Tag = "8";
            pictureBox8.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox8.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel7
            // 
            panel7.BackColor = Color.Red;
            panel7.Controls.Add(pictureBox7);
            panel7.Location = new Point(373, 135);
            panel7.Margin = new Padding(4, 5, 4, 5);
            panel7.Name = "panel7";
            panel7.Size = new Size(115, 102);
            panel7.TabIndex = 7;
            // 
            // pictureBox7
            // 
            pictureBox7.BackColor = Color.White;
            pictureBox7.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox7.BorderStyle = BorderStyle.Fixed3D;
            pictureBox7.Cursor = Cursors.Hand;
            pictureBox7.Location = new Point(4, 5);
            pictureBox7.Margin = new Padding(4, 5, 4, 5);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(105, 90);
            pictureBox7.TabIndex = 4;
            pictureBox7.TabStop = false;
            pictureBox7.Tag = "7";
            pictureBox7.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox7.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel6
            // 
            panel6.BackColor = Color.Red;
            panel6.Controls.Add(pictureBox6);
            panel6.Location = new Point(255, 135);
            panel6.Margin = new Padding(4, 5, 4, 5);
            panel6.Name = "panel6";
            panel6.Size = new Size(115, 102);
            panel6.TabIndex = 6;
            // 
            // pictureBox6
            // 
            pictureBox6.BackColor = Color.White;
            pictureBox6.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox6.BorderStyle = BorderStyle.Fixed3D;
            pictureBox6.Cursor = Cursors.Hand;
            pictureBox6.Location = new Point(4, 5);
            pictureBox6.Margin = new Padding(4, 5, 4, 5);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(105, 90);
            pictureBox6.TabIndex = 4;
            pictureBox6.TabStop = false;
            pictureBox6.Tag = "6";
            pictureBox6.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox6.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel5
            // 
            panel5.BackColor = Color.Red;
            panel5.Controls.Add(pictureBox5);
            panel5.Location = new Point(136, 135);
            panel5.Margin = new Padding(4, 5, 4, 5);
            panel5.Name = "panel5";
            panel5.Size = new Size(115, 102);
            panel5.TabIndex = 5;
            // 
            // pictureBox5
            // 
            pictureBox5.BackColor = Color.White;
            pictureBox5.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox5.BorderStyle = BorderStyle.Fixed3D;
            pictureBox5.Cursor = Cursors.Hand;
            pictureBox5.Location = new Point(4, 5);
            pictureBox5.Margin = new Padding(4, 5, 4, 5);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(105, 90);
            pictureBox5.TabIndex = 4;
            pictureBox5.TabStop = false;
            pictureBox5.Tag = "5";
            pictureBox5.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox5.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel4
            // 
            panel4.BackColor = Color.Red;
            panel4.Controls.Add(pictureBox4);
            panel4.Location = new Point(17, 135);
            panel4.Margin = new Padding(4, 5, 4, 5);
            panel4.Name = "panel4";
            panel4.Size = new Size(115, 102);
            panel4.TabIndex = 4;
            // 
            // pictureBox4
            // 
            pictureBox4.BackColor = Color.White;
            pictureBox4.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox4.BorderStyle = BorderStyle.Fixed3D;
            pictureBox4.Cursor = Cursors.Hand;
            pictureBox4.Location = new Point(4, 5);
            pictureBox4.Margin = new Padding(4, 5, 4, 5);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(105, 90);
            pictureBox4.TabIndex = 4;
            pictureBox4.TabStop = false;
            pictureBox4.Tag = "4";
            pictureBox4.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox4.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Red;
            panel3.Controls.Add(pictureBox3);
            panel3.Location = new Point(373, 29);
            panel3.Margin = new Padding(4, 5, 4, 5);
            panel3.Name = "panel3";
            panel3.Size = new Size(115, 102);
            panel3.TabIndex = 3;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.White;
            pictureBox3.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox3.BorderStyle = BorderStyle.Fixed3D;
            pictureBox3.Cursor = Cursors.Hand;
            pictureBox3.Location = new Point(4, 5);
            pictureBox3.Margin = new Padding(4, 5, 4, 5);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(105, 90);
            pictureBox3.TabIndex = 4;
            pictureBox3.TabStop = false;
            pictureBox3.Tag = "3";
            pictureBox3.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox3.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Red;
            panel2.Controls.Add(pictureBox2);
            panel2.Location = new Point(255, 29);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new Size(115, 102);
            panel2.TabIndex = 2;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.White;
            pictureBox2.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox2.BorderStyle = BorderStyle.Fixed3D;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Location = new Point(4, 5);
            pictureBox2.Margin = new Padding(4, 5, 4, 5);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(105, 90);
            pictureBox2.TabIndex = 4;
            pictureBox2.TabStop = false;
            pictureBox2.Tag = "2";
            pictureBox2.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox2.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Red;
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(136, 29);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(115, 102);
            panel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.White;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.BorderStyle = BorderStyle.Fixed3D;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Location = new Point(4, 5);
            pictureBox1.Margin = new Padding(4, 5, 4, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(105, 90);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            pictureBox1.Tag = "1";
            pictureBox1.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox1.MouseDown += pictureBoxAll_MouseDown;
            // 
            // panel0
            // 
            panel0.BackColor = Color.Red;
            panel0.Controls.Add(pictureBox0);
            panel0.Location = new Point(17, 29);
            panel0.Margin = new Padding(4, 5, 4, 5);
            panel0.Name = "panel0";
            panel0.Size = new Size(115, 102);
            panel0.TabIndex = 0;
            // 
            // pictureBox0
            // 
            pictureBox0.BackColor = Color.White;
            pictureBox0.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox0.BorderStyle = BorderStyle.Fixed3D;
            pictureBox0.Cursor = Cursors.Hand;
            pictureBox0.Location = new Point(4, 5);
            pictureBox0.Margin = new Padding(4, 5, 4, 5);
            pictureBox0.Name = "pictureBox0";
            pictureBox0.Size = new Size(105, 90);
            pictureBox0.TabIndex = 4;
            pictureBox0.TabStop = false;
            pictureBox0.Tag = "0";
            pictureBox0.DoubleClick += pictureBoxAll_DoubleClick;
            pictureBox0.MouseDown += pictureBoxAll_MouseDown;
            // 
            // FrmBackground
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(536, 458);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmBackground";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Background Colours and Patterns";
            groupBox1.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            panel11.ResumeLayout(false);
            ((ISupportInitialize)pictureBox11).EndInit();
            panel9.ResumeLayout(false);
            ((ISupportInitialize)pictureBox9).EndInit();
            panel10.ResumeLayout(false);
            ((ISupportInitialize)pictureBox10).EndInit();
            panel8.ResumeLayout(false);
            ((ISupportInitialize)pictureBox8).EndInit();
            panel7.ResumeLayout(false);
            ((ISupportInitialize)pictureBox7).EndInit();
            panel6.ResumeLayout(false);
            ((ISupportInitialize)pictureBox6).EndInit();
            panel5.ResumeLayout(false);
            ((ISupportInitialize)pictureBox5).EndInit();
            panel4.ResumeLayout(false);
            ((ISupportInitialize)pictureBox4).EndInit();
            panel3.ResumeLayout(false);
            ((ISupportInitialize)pictureBox3).EndInit();
            panel2.ResumeLayout(false);
            ((ISupportInitialize)pictureBox2).EndInit();
            panel1.ResumeLayout(false);
            ((ISupportInitialize)pictureBox1).EndInit();
            panel0.ResumeLayout(false);
            ((ISupportInitialize)pictureBox0).EndInit();
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
