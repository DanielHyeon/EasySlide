using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Localization;

namespace Axicultus;

public partial class FrmHelp : Form
{

		public FrmHelp()
    {
        InitializeComponent();
        UiText.ApplyTo(this);
    }

    private void FrmHelp_Load(object sender, EventArgs e)
    {
        int keyBoardOption = Gf.KeyBoardOption;
        if (keyBoardOption == 1)
        {
            label_FirstItem.Text = UiText.Get(nameof(FrmHelp), "LeftArrow");
            label_LastItem.Text = UiText.Get(nameof(FrmHelp), "RightArrow");
            label_PreviousItem.Text = UiText.Get(nameof(FrmHelp), "UpArrow");
            label_NextItem.Text = UiText.Get(nameof(FrmHelp), "DownArrow");
            label_FirstSlide.Text = "Home"; // same in every language
            label_LastSlide.Text = "End"; // same in every language
            label_PreviousSlide.Text = UiText.Get(nameof(FrmHelp), "PageUp");
            label_NextSlide.Text = UiText.Get(nameof(FrmHelp), "PageDownSpace");
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
