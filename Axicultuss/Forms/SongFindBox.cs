using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Axicultus;

/// <summary>
/// "Find in this folder" box above the song list (design D26, task 5.2): a magnifier, a borderless text box
/// with a placeholder and a clear (x) button while it holds text, in a thin frame. It only tells its owner
/// when to filter (FilterRequested: about 200 ms after the last keystroke, at once for Enter, Esc and the clear
/// button) and when Down was pressed (DownPressed). Keys never go anywhere else: Esc empties the box, Enter and
/// Down are taken by it, and everything else (letters, arrows, Home / End, Backspace, Delete) is the text box's
/// own editing.
/// </summary>
internal sealed class SongFindBox : Panel
{
    public const int DebounceMilliseconds = 200;

    // Logical (96 dpi) sizes.
    private const int IconSize = 16;
    private const int IconPadding = 6;
    private const int ClearCross = 8;

    private readonly TextBox text;
    private readonly Panel clear;
    private readonly Timer debounce;
    private bool silent;

    public event EventHandler FilterRequested;
    public event EventHandler DownPressed;

    public SongFindBox(Font font)
    {
        Name = "panelSongFind";
        BackColor = ModernUiPalette.Surface;
        TabStop = false;
        text = new TextBox
        {
            Name = "SongFindText",
            BorderStyle = BorderStyle.None,
            Font = font,
            BackColor = ModernUiPalette.Surface,
            PlaceholderText = "Find in this folder",
            AccessibleName = "Find in this folder",
            TabIndex = 0,
        };
        clear = new Panel
        {
            Name = "SongFindClear",
            AccessibleName = "Clear",
            AccessibleRole = AccessibleRole.PushButton,
            BackColor = ModernUiPalette.Surface,
            Cursor = Cursors.Hand,
            Visible = false,
            TabStop = false,
        };
        Controls.Add(text);
        Controls.Add(clear);
        debounce = new Timer { Interval = DebounceMilliseconds };
        debounce.Tick += Debounce_Tick;
        clear.Paint += Clear_Paint;
        clear.Click += (s, e) => { Clear(); text.Focus(); };
        text.TextChanged += Text_TextChanged;
        text.KeyDown += Text_KeyDown;
        text.Enter += (s, e) => Invalidate();
        // LostFocus, not Leave: it also comes when another window (a dialog opened right after typing)
        // takes the focus, where Leave does not.
        text.LostFocus += Text_LostFocus;
        MouseDown += (s, e) => text.Focus();
    }

    /// <summary>
    /// True when a plain Down key is taken by the app's global arrow-key hook (it moves the live output
    /// to the next slide whatever has the focus). Down is then not used by the box. Read at every key.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<bool> DownIsGlobalSlideKey { get; set; }

    /// <summary>
    /// SQL text of the title condition (SQLite): the displayed title contains @title. LIKE ignores case for
    /// ASCII letters; '\' is the escape character of the pattern (see LikePattern).
    /// </summary>
    public const string TitleCondition = "Title_1 like @title escape '\\'";

    /// <summary>
    /// Value bound to @title: the text with the LIKE wildcards '%' and '_' and the escape character '\'
    /// escaped, so they match themselves, between two '%'. Quotes need nothing: the value is a bound
    /// parameter, never part of the SQL.
    /// </summary>
    public static string LikePattern(string text)
    {
        return "%" + text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
    }

    /// <summary>The filter text as typed, blanks at both ends removed.</summary>
    public string FilterText => text.Text.Trim();

    public TextBox TextBox => text;

    /// <summary>Empties the box without asking for a filter (the owner has just listed the whole folder).</summary>
    public void ResetSilently()
    {
        debounce.Stop();
        if (text.TextLength == 0)
        {
            return;
        }
        silent = true;
        try
        {
            text.Text = "";
        }
        finally
        {
            silent = false;
        }
    }

    /// <summary>Empties the box and asks for the filter at once (Esc, the clear button).</summary>
    public void Clear()
    {
        ResetSilently();
        FilterRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>A pending filter is not wanted any more.</summary>
    public void CancelPending()
    {
        debounce.Stop();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        Size client = ClientSize;
        int iconSpace = LogicalToDeviceUnits(IconSize + 2 * IconPadding);
        int clearSize = Math.Max(0, client.Height - 2);
        clear.SetBounds(client.Width - 1 - clearSize, 1, clearSize, clearSize);
        int textHeight = text.PreferredHeight;
        int right = client.Width - 1 - clearSize;
        text.SetBounds(iconSpace, Math.Max(1, (client.Height - textHeight) / 2), Math.Max(0, right - iconSpace), textHeight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Size client = ClientSize;
        using (Pen pen = new(text.Focused ? ModernUiPalette.PreviewAccent : ModernUiPalette.Border))
        {
            e.Graphics.DrawRectangle(pen, 0, 0, client.Width - 1, client.Height - 1);
        }
        int iconSize = LogicalToDeviceUnits(IconSize);
        Bitmap icon = ModernIcons.Get("search", ModernIconStyle.Regular, iconSize, Enabled ? ModernUiPalette.TextSecondary : ModernUiPalette.Border);
        if (icon != null)
        {
            e.Graphics.DrawImageUnscaled(icon, LogicalToDeviceUnits(IconPadding), (client.Height - iconSize) / 2);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            debounce.Stop();
            debounce.Dispose();
        }
        base.Dispose(disposing);
    }

    private void Debounce_Tick(object sender, EventArgs e)
    {
        debounce.Stop();
        if (IsDisposed || Disposing)
        {
            return;
        }
        FilterRequested?.Invoke(this, EventArgs.Empty);
    }

    // A filter still waiting for the debounce is applied when the box loses the focus, so the reload never
    // comes later, inside a drag started from the list or under a dialog.
    private void Text_LostFocus(object sender, EventArgs e)
    {
        Invalidate();
        if (debounce.Enabled)
        {
            debounce.Stop();
            if (!IsDisposed && !Disposing)
            {
                FilterRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Clear_Paint(object sender, PaintEventArgs e)
    {
        Size client = clear.ClientSize;
        int half = LogicalToDeviceUnits(ClearCross) / 2;
        int cx = client.Width / 2;
        int cy = client.Height / 2;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using Pen pen = new(ModernUiPalette.TextSecondary, Math.Max(1f, LogicalToDeviceUnits(1)));
        e.Graphics.DrawLine(pen, cx - half, cy - half, cx + half, cy + half);
        e.Graphics.DrawLine(pen, cx - half, cy + half, cx + half, cy - half);
    }

    private void Text_TextChanged(object sender, EventArgs e)
    {
        clear.Visible = text.TextLength > 0;
        if (silent)
        {
            return;
        }
        debounce.Stop();
        debounce.Start();
    }

    private void Text_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Escape:
                e.Handled = true;
                e.SuppressKeyPress = true;
                Clear();
                break;
            case Keys.Return:
                e.Handled = true;
                e.SuppressKeyPress = true;
                debounce.Stop();
                FilterRequested?.Invoke(this, EventArgs.Empty);
                break;
            case Keys.Down:
                if (e.Modifiers != Keys.None)
                {
                    break;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
                // With the global arrow-key hook on (Gf.GlobalHookKey_Arrow, KeyboardActionHandler.HandleGlobalArrowKey)
                // every Down press also moves the live output to the next slide, wherever the focus is. The box
                // must not invite that key: Down then does nothing here (Tab still reaches the list).
                if (DownIsGlobalSlideKey?.Invoke() == true)
                {
                    break;
                }
                debounce.Stop();
                FilterRequested?.Invoke(this, EventArgs.Empty);
                DownPressed?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
