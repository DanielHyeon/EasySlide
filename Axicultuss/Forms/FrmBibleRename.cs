using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Axicultus;

public partial class FrmBibleRename : Form
{
    private readonly HashSet<string> otherNames;
    public string BibleName { get; private set; }

    public FrmBibleRename() : this("", []) { }

    internal FrmBibleRename(string currentName, IEnumerable<string> otherNames)
    {
        InitializeComponent();
        BibleName = currentName;
        this.otherNames = new HashSet<string>(otherNames, StringComparer.OrdinalIgnoreCase);
    }

    internal bool CanUseName(string name) => !string.IsNullOrWhiteSpace(name) && !otherNames.Contains(name.Trim());

    private void FrmRename_Load(object sender, EventArgs e)
    {
        label1.Text = $"Rename '{BibleName}' to:";
        textBoxNewString.Text = BibleName;
        textBoxNewString.SelectAll();
    }

    private void BtnCancel_Click(object sender, EventArgs e) => Close();

    private void BtnOK_Click(object sender, EventArgs e)
    {
        string proposed = textBoxNewString.Text.Trim();
        if (!CanUseName(proposed))
        {
            MessageBox.Show(this, proposed.Length == 0 ? "Please enter a new Bible Name."
                : "Name already exists! Please try a different Bible Name.");
            return;
        }
        if (proposed != BibleName)
        {
            BibleName = proposed;
            DialogResult = DialogResult.OK;
        }
        Close();
    }
}
