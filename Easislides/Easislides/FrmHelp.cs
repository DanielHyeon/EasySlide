using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    public class FrmHelp : Form
    {
        private IContainer components = null;

        private Button CloseBtn;

        private Label label_FirstItem;

        private GroupBox groupBox1;

        private Label label_LastItem;

        private Label label21;

        private Label label22;

        private Label label23;

        private Label label24;

        private Label label25;

        private Label label26;

        private Label label27;

        private Label label28;

        private Label label30;

        private Label label31;

        private Label label32;

        private Label label33;

        private Label label34;

        private Label label35;

        private Label label36;

        private Label label37;

        private Label label38;

        private Label label39;

        private Label label40;

        private Label label20;

        private Label label9;

        private Label label17;

        private Label label18;

        private Label label19;

        private Label label13;

        private Label label14;

        private Label label15;

        private Label label10;

        private Label label11;

        private Label label12;

        private Label label_NextSlide;

        private Label label_PreviousSlide;

        private Label label_LastSlide;

        private Label label_FirstSlide;

        private Label label_NextItem;

        private Label label_PreviousItem;

        private Label label41;

        private Label label42;

        private Label label43;

        private Label label44;

        private Label label45;

        private Label label46;

        private Label label47;

        private Label label48;

        private Label label49;

        private Label label50;

        private Label label1;

        private Label label2;

        private Label label3;

        private Label label4;

        private Label label5;

        private Label label6;

        private Label label7;

        private Label label8;

        private Label label51;

        private Label label52;

        private Label label53;
        private Label label56;
        private Label label55;
        private Label label54;

        public FrmHelp()
        {
            InitializeComponent();
        }

        private void FrmHelp_Load(object sender, EventArgs e)
        {
            int keyBoardOption = gf.KeyBoardOption;
            if (keyBoardOption == 1)
            {
                label_FirstItem.Text = "Left Arrow";
                label_LastItem.Text = "Right Arrow";
                label_PreviousItem.Text = "Up Arrow";
                label_NextItem.Text = "Down Arrow";
                label_FirstSlide.Text = "Home";
                label_LastSlide.Text = "End";
                label_PreviousSlide.Text = "Page Up";
                label_NextSlide.Text = "Page Down, Space";
            }
            Cursor.Position = new Point(base.Left + CloseBtn.Left + 50, base.Top + CloseBtn.Top + 40);
            Cursor.Current = Cursors.Default;
            Cursor.Show();
        }

        private void FrmHelp_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Current = Cursors.Default;
            Cursor.Hide();
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmHelp));
            CloseBtn = new Button();
            label_FirstItem = new Label();
            groupBox1 = new GroupBox();
            label53 = new Label();
            label56 = new Label();
            label55 = new Label();
            label54 = new Label();
            label51 = new Label();
            label52 = new Label();
            label7 = new Label();
            label8 = new Label();
            label5 = new Label();
            label6 = new Label();
            label3 = new Label();
            label4 = new Label();
            label1 = new Label();
            label2 = new Label();
            label41 = new Label();
            label42 = new Label();
            label43 = new Label();
            label44 = new Label();
            label45 = new Label();
            label46 = new Label();
            label47 = new Label();
            label48 = new Label();
            label49 = new Label();
            label50 = new Label();
            label21 = new Label();
            label22 = new Label();
            label23 = new Label();
            label24 = new Label();
            label25 = new Label();
            label26 = new Label();
            label27 = new Label();
            label28 = new Label();
            label30 = new Label();
            label31 = new Label();
            label32 = new Label();
            label33 = new Label();
            label34 = new Label();
            label35 = new Label();
            label36 = new Label();
            label20 = new Label();
            label9 = new Label();
            label17 = new Label();
            label18 = new Label();
            label19 = new Label();
            label13 = new Label();
            label14 = new Label();
            label15 = new Label();
            label10 = new Label();
            label11 = new Label();
            label12 = new Label();
            label_PreviousSlide = new Label();
            label_LastSlide = new Label();
            label_FirstSlide = new Label();
            label_NextItem = new Label();
            label_PreviousItem = new Label();
            label_LastItem = new Label();
            label_NextSlide = new Label();
            label37 = new Label();
            label38 = new Label();
            label39 = new Label();
            label40 = new Label();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // CloseBtn
            // 
            CloseBtn.Location = new Point(181, 599);
            CloseBtn.Margin = new Padding(5, 4, 5, 4);
            CloseBtn.Name = "CloseBtn";
            CloseBtn.Size = new Size(106, 37);
            CloseBtn.TabIndex = 2;
            CloseBtn.Text = "Close";
            CloseBtn.Click += CloseBtn_Click;
            // 
            // label_FirstItem
            // 
            label_FirstItem.Location = new Point(14, 32);
            label_FirstItem.Margin = new Padding(5, 0, 5, 0);
            label_FirstItem.Name = "label_FirstItem";
            label_FirstItem.Size = new Size(95, 20);
            label_FirstItem.TabIndex = 3;
            label_FirstItem.Text = "Home";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label53);
            groupBox1.Controls.Add(label56);
            groupBox1.Controls.Add(label55);
            groupBox1.Controls.Add(label54);
            groupBox1.Controls.Add(label51);
            groupBox1.Controls.Add(label52);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label41);
            groupBox1.Controls.Add(label42);
            groupBox1.Controls.Add(label43);
            groupBox1.Controls.Add(label44);
            groupBox1.Controls.Add(label45);
            groupBox1.Controls.Add(label46);
            groupBox1.Controls.Add(label47);
            groupBox1.Controls.Add(label48);
            groupBox1.Controls.Add(label49);
            groupBox1.Controls.Add(label50);
            groupBox1.Controls.Add(label21);
            groupBox1.Controls.Add(label22);
            groupBox1.Controls.Add(label23);
            groupBox1.Controls.Add(label24);
            groupBox1.Controls.Add(label25);
            groupBox1.Controls.Add(label26);
            groupBox1.Controls.Add(label27);
            groupBox1.Controls.Add(label28);
            groupBox1.Controls.Add(label30);
            groupBox1.Controls.Add(label31);
            groupBox1.Controls.Add(label32);
            groupBox1.Controls.Add(label33);
            groupBox1.Controls.Add(label34);
            groupBox1.Controls.Add(label35);
            groupBox1.Controls.Add(label36);
            groupBox1.Controls.Add(label20);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label17);
            groupBox1.Controls.Add(label18);
            groupBox1.Controls.Add(label19);
            groupBox1.Controls.Add(label13);
            groupBox1.Controls.Add(label14);
            groupBox1.Controls.Add(label15);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(label_PreviousSlide);
            groupBox1.Controls.Add(label_LastSlide);
            groupBox1.Controls.Add(label_FirstSlide);
            groupBox1.Controls.Add(label_NextItem);
            groupBox1.Controls.Add(label_PreviousItem);
            groupBox1.Controls.Add(label_LastItem);
            groupBox1.Controls.Add(label_FirstItem);
            groupBox1.Controls.Add(label_NextSlide);
            groupBox1.Controls.Add(label37);
            groupBox1.Controls.Add(label38);
            groupBox1.Controls.Add(label39);
            groupBox1.Controls.Add(label40);
            groupBox1.Location = new Point(10, 4);
            groupBox1.Margin = new Padding(5, 4, 5, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(5, 4, 5, 4);
            groupBox1.Size = new Size(449, 584);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            // 
            // label53
            // 
            label53.Location = new Point(142, 256);
            label53.Margin = new Padding(5, 0, 5, 0);
            label53.Name = "label53";
            label53.Size = new Size(185, 20);
            label53.TabIndex = 65;
            label53.Text = "Restart Current Item";
            // 
            // label56
            // 
            label56.Location = new Point(14, 305);
            label56.Margin = new Padding(5, 0, 5, 0);
            label56.Name = "label56";
            label56.Size = new Size(118, 20);
            label56.TabIndex = 64;
            label56.Text = "F10";
            // 
            // label55
            // 
            label55.Location = new Point(14, 281);
            label55.Margin = new Padding(5, 0, 5, 0);
            label55.Name = "label55";
            label55.Size = new Size(118, 20);
            label55.TabIndex = 64;
            label55.Text = "F9";
            // 
            // label54
            // 
            label54.Location = new Point(14, 256);
            label54.Margin = new Padding(5, 0, 5, 0);
            label54.Name = "label54";
            label54.Size = new Size(118, 20);
            label54.TabIndex = 64;
            label54.Text = "F5";
            // 
            // label51
            // 
            label51.AutoSize = true;
            label51.Location = new Point(272, 367);
            label51.Margin = new Padding(5, 0, 5, 0);
            label51.Name = "label51";
            label51.Size = new Size(160, 20);
            label51.TabIndex = 63;
            label51.Text = "Media - Pause/Resume";
            // 
            // label52
            // 
            label52.Location = new Point(231, 364);
            label52.Margin = new Padding(5, 0, 5, 0);
            label52.Name = "label52";
            label52.Size = new Size(42, 20);
            label52.TabIndex = 62;
            label52.Text = "M";
            // 
            // label7
            // 
            label7.Location = new Point(55, 488);
            label7.Margin = new Padding(5, 0, 5, 0);
            label7.Name = "label7";
            label7.Size = new Size(139, 53);
            label7.TabIndex = 61;
            label7.Text = "Jump To Next Non-Rotating Item";
            // 
            // label8
            // 
            label8.Location = new Point(14, 488);
            label8.Margin = new Padding(5, 0, 5, 0);
            label8.Name = "label8";
            label8.Size = new Size(33, 20);
            label8.TabIndex = 60;
            label8.Text = "J";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(55, 416);
            label5.Margin = new Padding(5, 0, 5, 0);
            label5.Name = "label5";
            label5.Size = new Size(120, 20);
            label5.TabIndex = 59;
            label5.Text = "Gap item On/Off";
            // 
            // label6
            // 
            label6.Location = new Point(14, 416);
            label6.Margin = new Padding(5, 0, 5, 0);
            label6.Name = "label6";
            label6.Size = new Size(33, 20);
            label6.TabIndex = 58;
            label6.Text = "G";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(272, 509);
            label3.Margin = new Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Size = new Size(125, 20);
            label3.TabIndex = 57;
            label3.Text = "Reference On/Off";
            // 
            // label4
            // 
            label4.Location = new Point(231, 509);
            label4.Margin = new Padding(5, 0, 5, 0);
            label4.Name = "label4";
            label4.Size = new Size(42, 20);
            label4.TabIndex = 56;
            label4.Text = "Z";
            // 
            // label1
            // 
            label1.Location = new Point(142, 231);
            label1.Margin = new Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new Size(185, 20);
            label1.TabIndex = 55;
            label1.Text = "Live Cam On/Off";
            // 
            // label2
            // 
            label2.Location = new Point(14, 231);
            label2.Margin = new Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new Size(118, 20);
            label2.TabIndex = 54;
            label2.Text = "F4";
            // 
            // label41
            // 
            label41.Location = new Point(235, 548);
            label41.Margin = new Padding(5, 0, 5, 0);
            label41.Name = "label41";
            label41.Size = new Size(85, 20);
            label41.TabIndex = 53;
            label41.Text = "End Show";
            // 
            // label42
            // 
            label42.AutoSize = true;
            label42.Location = new Point(272, 488);
            label42.Margin = new Padding(5, 0, 5, 0);
            label42.Name = "label42";
            label42.Size = new Size(131, 20);
            label42.TabIndex = 52;
            label42.Text = "Vertical Alignment";
            // 
            // label43
            // 
            label43.AutoSize = true;
            label43.Location = new Point(272, 416);
            label43.Margin = new Padding(5, 0, 5, 0);
            label43.Name = "label43";
            label43.Size = new Size(140, 20);
            label43.TabIndex = 51;
            label43.Text = "Outline Font On/Off";
            // 
            // label44
            // 
            label44.AutoSize = true;
            label44.Location = new Point(272, 464);
            label44.Margin = new Padding(5, 0, 5, 0);
            label44.Name = "label44";
            label44.Size = new Size(145, 20);
            label44.TabIndex = 50;
            label44.Text = "Shadow Font On/Off";
            // 
            // label45
            // 
            label45.AutoSize = true;
            label45.Location = new Point(272, 440);
            label45.Margin = new Padding(5, 0, 5, 0);
            label45.Name = "label45";
            label45.Size = new Size(122, 20);
            label45.TabIndex = 49;
            label45.Text = "Region 1, 2 Lyrics";
            // 
            // label46
            // 
            label46.Location = new Point(138, 548);
            label46.Margin = new Padding(5, 0, 5, 0);
            label46.Name = "label46";
            label46.Size = new Size(89, 20);
            label46.TabIndex = 48;
            label46.Text = "Esc, F12, '-'";
            // 
            // label47
            // 
            label47.Location = new Point(231, 488);
            label47.Margin = new Padding(5, 0, 5, 0);
            label47.Name = "label47";
            label47.Size = new Size(42, 20);
            label47.TabIndex = 47;
            label47.Text = "V";
            // 
            // label48
            // 
            label48.Location = new Point(231, 416);
            label48.Margin = new Padding(5, 0, 5, 0);
            label48.Name = "label48";
            label48.Size = new Size(42, 20);
            label48.TabIndex = 46;
            label48.Text = "O";
            // 
            // label49
            // 
            label49.Location = new Point(231, 464);
            label49.Margin = new Padding(5, 0, 5, 0);
            label49.Name = "label49";
            label49.Size = new Size(42, 20);
            label49.TabIndex = 45;
            label49.Text = "S";
            // 
            // label50
            // 
            label50.Location = new Point(231, 440);
            label50.Margin = new Padding(5, 0, 5, 0);
            label50.Name = "label50";
            label50.Size = new Size(42, 20);
            label50.TabIndex = 44;
            label50.Text = "R";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(272, 391);
            label21.Margin = new Padding(5, 0, 5, 0);
            label21.Name = "label21";
            label21.Size = new Size(124, 20);
            label21.TabIndex = 43;
            label21.Text = "Notations On/Off";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(55, 464);
            label22.Margin = new Padding(5, 0, 5, 0);
            label22.Name = "label22";
            label22.Size = new Size(116, 20);
            label22.TabIndex = 42;
            label22.Text = "Interlace On/Off";
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(55, 440);
            label23.Margin = new Padding(5, 0, 5, 0);
            label23.Name = "label23";
            label23.Size = new Size(108, 20);
            label23.TabIndex = 41;
            label23.Text = "Headings Style";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(55, 391);
            label24.Margin = new Padding(5, 0, 5, 0);
            label24.Name = "label24";
            label24.Size = new Size(151, 20);
            label24.TabIndex = 40;
            label24.Text = "Display Panel  On/Off";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(55, 367);
            label25.Margin = new Padding(5, 0, 5, 0);
            label25.Name = "label25";
            label25.Size = new Size(139, 20);
            label25.TabIndex = 39;
            label25.Text = "Auto Rotate On/Off";
            // 
            // label26
            // 
            label26.Location = new Point(142, 305);
            label26.Margin = new Padding(5, 0, 5, 0);
            label26.Name = "label26";
            label26.Size = new Size(226, 20);
            label26.TabIndex = 38;
            label26.Text = "Black Screen On/Off (global)";
            // 
            // label27
            // 
            label27.Location = new Point(142, 281);
            label27.Margin = new Padding(5, 0, 5, 0);
            label27.Name = "label27";
            label27.Size = new Size(185, 24);
            label27.TabIndex = 37;
            label27.Text = "Black Screen On/Off (global)";
            // 
            // label28
            // 
            label28.Location = new Point(142, 204);
            label28.Margin = new Padding(5, 0, 5, 0);
            label28.Name = "label28";
            label28.Size = new Size(185, 20);
            label28.TabIndex = 36;
            label28.Text = "Clear Text On/Off";
            // 
            // label30
            // 
            label30.Location = new Point(142, 180);
            label30.Margin = new Padding(5, 0, 5, 0);
            label30.Name = "label30";
            label30.Size = new Size(299, 20);
            label30.TabIndex = 34;
            label30.Text = "PreChorus1/2,  Bridge1/2,  Chorus2 ,  Ending";
            // 
            // label31
            // 
            label31.Location = new Point(330, 156);
            label31.Margin = new Padding(5, 0, 5, 0);
            label31.Name = "label31";
            label31.Size = new Size(82, 20);
            label31.TabIndex = 33;
            label31.Text = "Chorus";
            // 
            // label32
            // 
            label32.Location = new Point(113, 156);
            label32.Margin = new Padding(5, 0, 5, 0);
            label32.Name = "label32";
            label32.Size = new Size(88, 20);
            label32.TabIndex = 32;
            label32.Text = "Verse 1 .. 9";
            // 
            // label33
            // 
            label33.Location = new Point(330, 107);
            label33.Margin = new Padding(5, 0, 5, 0);
            label33.Name = "label33";
            label33.Size = new Size(104, 20);
            label33.TabIndex = 31;
            label33.Text = "Next Slide";
            // 
            // label34
            // 
            label34.Location = new Point(330, 81);
            label34.Margin = new Padding(5, 0, 5, 0);
            label34.Name = "label34";
            label34.Size = new Size(104, 20);
            label34.TabIndex = 30;
            label34.Text = "Previous Slide";
            // 
            // label35
            // 
            label35.Location = new Point(330, 57);
            label35.Margin = new Padding(5, 0, 5, 0);
            label35.Name = "label35";
            label35.Size = new Size(104, 20);
            label35.TabIndex = 29;
            label35.Text = "Last Slide";
            // 
            // label36
            // 
            label36.Location = new Point(330, 32);
            label36.Margin = new Padding(5, 0, 5, 0);
            label36.Name = "label36";
            label36.Size = new Size(104, 20);
            label36.TabIndex = 28;
            label36.Text = "First Slide";
            // 
            // label20
            // 
            label20.Location = new Point(231, 389);
            label20.Margin = new Padding(5, 0, 5, 0);
            label20.Name = "label20";
            label20.Size = new Size(42, 20);
            label20.TabIndex = 23;
            label20.Text = "N";
            // 
            // label9
            // 
            label9.Location = new Point(14, 464);
            label9.Margin = new Padding(5, 0, 5, 0);
            label9.Name = "label9";
            label9.Size = new Size(33, 20);
            label9.TabIndex = 22;
            label9.Text = "I";
            // 
            // label17
            // 
            label17.Location = new Point(14, 440);
            label17.Margin = new Padding(5, 0, 5, 0);
            label17.Name = "label17";
            label17.Size = new Size(33, 20);
            label17.TabIndex = 21;
            label17.Text = "H";
            // 
            // label18
            // 
            label18.Location = new Point(14, 391);
            label18.Margin = new Padding(5, 0, 5, 0);
            label18.Name = "label18";
            label18.Size = new Size(33, 20);
            label18.TabIndex = 20;
            label18.Text = "D";
            // 
            // label19
            // 
            label19.Location = new Point(14, 367);
            label19.Margin = new Padding(5, 0, 5, 0);
            label19.Name = "label19";
            label19.Size = new Size(33, 20);
            label19.TabIndex = 19;
            label19.Text = "A";
            // 
            // label13
            // 
            label13.Location = new Point(0, 0);
            label13.Margin = new Padding(5, 0, 5, 0);
            label13.Name = "label13";
            label13.Size = new Size(134, 36);
            label13.TabIndex = 66;
            // 
            // label14
            // 
            label14.Location = new Point(0, 0);
            label14.Margin = new Padding(5, 0, 5, 0);
            label14.Name = "label14";
            label14.Size = new Size(134, 36);
            label14.TabIndex = 67;
            // 
            // label15
            // 
            label15.Location = new Point(14, 204);
            label15.Margin = new Padding(5, 0, 5, 0);
            label15.Name = "label15";
            label15.Size = new Size(118, 20);
            label15.TabIndex = 16;
            label15.Text = "F3";
            // 
            // label10
            // 
            label10.Location = new Point(14, 180);
            label10.Margin = new Padding(5, 0, 5, 0);
            label10.Name = "label10";
            label10.Size = new Size(118, 20);
            label10.TabIndex = 13;
            label10.Text = "P, Q, B, W, T, E";
            // 
            // label11
            // 
            label11.Location = new Point(234, 156);
            label11.Margin = new Padding(5, 0, 5, 0);
            label11.Name = "label11";
            label11.Size = new Size(73, 20);
            label11.TabIndex = 12;
            label11.Text = "0, C";
            // 
            // label12
            // 
            label12.Location = new Point(14, 156);
            label12.Margin = new Padding(5, 0, 5, 0);
            label12.Name = "label12";
            label12.Size = new Size(118, 20);
            label12.TabIndex = 11;
            label12.Text = "1 .. 9";
            // 
            // label_PreviousSlide
            // 
            label_PreviousSlide.Location = new Point(234, 81);
            label_PreviousSlide.Margin = new Padding(5, 0, 5, 0);
            label_PreviousSlide.Name = "label_PreviousSlide";
            label_PreviousSlide.Size = new Size(91, 20);
            label_PreviousSlide.TabIndex = 9;
            label_PreviousSlide.Text = "Up Arrow";
            // 
            // label_LastSlide
            // 
            label_LastSlide.Location = new Point(234, 57);
            label_LastSlide.Margin = new Padding(5, 0, 5, 0);
            label_LastSlide.Name = "label_LastSlide";
            label_LastSlide.Size = new Size(91, 20);
            label_LastSlide.TabIndex = 8;
            label_LastSlide.Text = "Right Arrow";
            // 
            // label_FirstSlide
            // 
            label_FirstSlide.Location = new Point(234, 32);
            label_FirstSlide.Margin = new Padding(5, 0, 5, 0);
            label_FirstSlide.Name = "label_FirstSlide";
            label_FirstSlide.Size = new Size(91, 20);
            label_FirstSlide.TabIndex = 7;
            label_FirstSlide.Text = "Left Arrow";
            // 
            // label_NextItem
            // 
            label_NextItem.Location = new Point(14, 107);
            label_NextItem.Margin = new Padding(5, 0, 5, 0);
            label_NextItem.Name = "label_NextItem";
            label_NextItem.Size = new Size(95, 20);
            label_NextItem.TabIndex = 6;
            label_NextItem.Text = "Page Down";
            // 
            // label_PreviousItem
            // 
            label_PreviousItem.Location = new Point(14, 81);
            label_PreviousItem.Margin = new Padding(5, 0, 5, 0);
            label_PreviousItem.Name = "label_PreviousItem";
            label_PreviousItem.Size = new Size(95, 20);
            label_PreviousItem.TabIndex = 5;
            label_PreviousItem.Text = "Page Up";
            // 
            // label_LastItem
            // 
            label_LastItem.Location = new Point(14, 57);
            label_LastItem.Margin = new Padding(5, 0, 5, 0);
            label_LastItem.Name = "label_LastItem";
            label_LastItem.Size = new Size(95, 20);
            label_LastItem.TabIndex = 4;
            label_LastItem.Text = "End";
            // 
            // label_NextSlide
            // 
            label_NextSlide.Location = new Point(234, 107);
            label_NextSlide.Margin = new Padding(5, 0, 5, 0);
            label_NextSlide.Name = "label_NextSlide";
            label_NextSlide.Size = new Size(91, 43);
            label_NextSlide.TabIndex = 10;
            label_NextSlide.Text = "Down Arrow, Space";
            // 
            // label37
            // 
            label37.Location = new Point(113, 107);
            label37.Margin = new Padding(5, 0, 5, 0);
            label37.Name = "label37";
            label37.Size = new Size(105, 20);
            label37.TabIndex = 27;
            label37.Text = "Next item";
            // 
            // label38
            // 
            label38.Location = new Point(113, 81);
            label38.Margin = new Padding(5, 0, 5, 0);
            label38.Name = "label38";
            label38.Size = new Size(105, 20);
            label38.TabIndex = 26;
            label38.Text = "Previous Item";
            // 
            // label39
            // 
            label39.Location = new Point(113, 57);
            label39.Margin = new Padding(5, 0, 5, 0);
            label39.Name = "label39";
            label39.Size = new Size(105, 20);
            label39.TabIndex = 25;
            label39.Text = "Last Item";
            // 
            // label40
            // 
            label40.Location = new Point(113, 32);
            label40.Margin = new Padding(5, 0, 5, 0);
            label40.Name = "label40";
            label40.Size = new Size(105, 20);
            label40.TabIndex = 24;
            label40.Text = "First Item";
            // 
            // FrmHelp
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(474, 651);
            Controls.Add(groupBox1);
            Controls.Add(CloseBtn);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(5, 4, 5, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmHelp";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Keyboard Keys";
            TopMost = true;
            FormClosing += FrmHelp_FormClosing;
            Load += FrmHelp_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }
    }
}
