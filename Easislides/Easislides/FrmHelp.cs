using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    public partial class FrmHelp : Form
    {

		public FrmHelp()
        {
            InitializeComponent();
        }

        private void FrmHelp_Load(object sender, EventArgs e)
        {
            int keyBoardOption = Gf.KeyBoardOption;
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

                    }
}
