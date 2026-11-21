using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Axicultus.Util;
#if SQLite
using Axicultus.SQLite;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
#endif

namespace Axicultus;

/// <summary>
/// Song list of the Folders tab in the new look (openspec change modernize-main-form-ui, design D26,
/// tasks 5.1 and 5.2): title and trailing number drawn apart (SongListPainter), and a "find in this
/// folder" box (SongFindBox) between the folder combo and the list. Both exist only while IsModernUiActive;
/// with the gate closed the list is drawn by the system and has the place and size it has without them.
/// </summary>
public partial class FrmMain
{
    // ----- 5.1 Title and number apart -----

    private SongListPainter songListPainter;

    private void AttachSongListPainter()
    {
        songListPainter = new SongListPainter(SongsList, () => IsModernUiActive);
        songListPainter.Attach();
        Disposed += (s, e) => songListPainter?.Dispose();
    }

    // ----- 5.2 Find in this folder -----

    private const string SongSearchResultsFolder = "Search Results:";

    private SongFindBox songFind;

    // The box is meant to be shown (new look). Control.Visible cannot tell: it is false while the
    // Folders tab is not the selected tab.
    private bool songFindShown;

    // Filter of the rows now in the list ("" = the whole folder).
    private string songFindApplied = "";

    // While filtered: songs in the whole folder and rows listed, both at the last filtered load. Rows leave
    // the list afterwards only through Move / Delete, so total - (listed - now) stays right.
    private int songFindFolderTotal = -1;
    private int songFindListedAtLoad = -1;

    // The list's top and tab index without the box, the designer's gap between combo and list, and what
    // the box takes from the list's height now.
    private int songsListBaseTop = -1;
    private int songsListBaseTabIndex;
    private int songFindGap;
    private int songFindHeightTaken;

    /// <summary>
    /// Creates the find box (new look only, from ApplyModernUi; not in the designer file, design D2). The list
    /// moves down by the box's height plus the gap the designer left between the combo and the list, and gets
    /// shorter by the same amount; its left and width do not change.
    /// </summary>
    private void CreateSongFind()
    {
        songsListBaseTop = SongsList.Top;
        songsListBaseTabIndex = SongsList.TabIndex;
        songFindGap = Math.Max(0, songsListBaseTop - SongFolder.Bottom);

        songFind = new SongFindBox(SongFolder.Font) { TabIndex = songsListBaseTabIndex };
        songFind.FilterRequested += (s, e) => ApplySongFind();
        songFind.DownPressed += (s, e) => FocusSongsListFromFind();
        songFind.DownIsGlobalSlideKey = () => Gf.GlobalHookKey_Arrow;
        // No filter reload once the form is closing (Form1_FormClosing never cancels the close).
        FormClosing += (s, e) => songFind?.CancelPending();
        // After the designer's handler (added first), which loads the folder.
        SongFolder.SelectedIndexChanged += SongFolder_SelectedIndexChangedForFind;

        tabFolders.Controls.Add(songFind);
        songFindShown = true;
        // Tab order: folder combo, find box, list.
        SongsList.TabIndex = songsListBaseTabIndex + 1;
        UpdateSongFindEnabled();
        LayoutSongFind(listHeightIsBase: false);
    }

    /// <summary>
    /// Places the box and the list. listHeightIsBase: ResizeTabList has just set the list's height (its height
    /// without the box); otherwise the list still has the height this method gave it last time. With the box
    /// hidden (gate closed) the list gets back exactly the top and height it has without the box.
    /// </summary>
    private void LayoutSongFind(bool listHeightIsBase)
    {
        if (songFind == null)
        {
            return;
        }
        try
        {
            int baseHeight = listHeightIsBase ? SongsList.Height : SongsList.Height + songFindHeightTaken;
            int shift = 0;
            if (songFindShown)
            {
                int boxHeight = SongFolder.Height;
                songFind.SetBounds(SongsList.Left, SongFolder.Bottom + songFindGap, SongsList.Width, boxHeight);
                shift = boxHeight + songFindGap;
            }
            int top = songsListBaseTop + shift;
            int height = Math.Max(0, baseHeight - shift);
            songFindHeightTaken = baseHeight - height;
            if (SongsList.Top != top || SongsList.Height != height)
            {
                SongsList.SetBounds(SongsList.Left, top, SongsList.Width, height);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"LayoutSongFind failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Down in the find box: focus to the list, on the selected row when there is one, else on the first row,
    /// which gets selected. The key's release then reaches the list, whose KeyUp handler loads that row's
    /// preview, as after any key pressed in the list.
    /// </summary>
    private void FocusSongsListFromFind()
    {
        if (SongsList.Items.Count == 0)
        {
            return;
        }
        if (SongsList.SelectedItems.Count == 0)
        {
            SongsList.Items[0].Selected = true;
            SongsList.Items[0].Focused = true;
            SongsList.EnsureVisible(0);
        }
        else if (SongsList.FocusedItem == null || !SongsList.FocusedItem.Selected)
        {
            SongsList.SelectedItems[0].Focused = true;
        }
        SongsList.Focus();
    }

    /// <summary>
    /// Lists the current folder's songs whose title contains the box's text (design D26), through FillList
    /// with a title condition: no copy of the rows is kept, so rows moved or deleted meanwhile cannot come
    /// back. The Media folder scan of the folder load is kept. Not for "Search Results:" (the box is disabled).
    /// </summary>
    private void ApplySongFind()
    {
        if (songFind == null || !songFindShown || InitFormLoad || MainFormClosing || songFind.IsDisposed)
        {
            return;
        }
        string filter = songFind.FilterText;
        if (filter == songFindApplied || SongFolder.Text == "" || SongFolder.Text == SongSearchResultsFolder)
        {
            return;
        }
        int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
        if (folderNumber > 0)
        {
            ReloadSongFolderRows(folderNumber, filter);
        }
    }

    /// <summary>
    /// Reloads the folder's rows with the filter. Selected songs that are still listed stay selected (by
    /// song id, SubItems[1]) and the focused one stays focused; no selection event of the app runs, so the
    /// preview is left as it is (also when the selected song is filtered out).
    /// </summary>
    private void ReloadSongFolderRows(int folderNumber, string filter)
    {
        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (ListViewItem row in SongsList.SelectedItems)
        {
            selectedIds.Add(SongRowId(row));
        }
        string focusedId = SongsList.FocusedItem != null ? SongRowId(SongsList.FocusedItem) : null;

        Cursor = Cursors.WaitCursor;
        SongsList.BeginUpdate();
        try
        {
            songFindApplied = filter;
            songFindFolderTotal = filter == "" ? -1 : CountSongFolderRows(folderNumber);
            songFindListedAtLoad = -1;
            SongsList.Items.Clear();
            FillList(folderNumber, filter, InItemMusicOnly: false, rescanMedia: false);
            songFindListedAtLoad = SongsList.Items.Count;

            int firstSelected = -1;
            for (int i = 0; i < SongsList.Items.Count; i++)
            {
                ListViewItem row = SongsList.Items[i];
                string id = SongRowId(row);
                if (selectedIds.Contains(id))
                {
                    row.Selected = true;
                    if (firstSelected < 0)
                    {
                        firstSelected = i;
                    }
                }
                if (focusedId != null && id == focusedId)
                {
                    row.Focused = true;
                }
            }
            if (firstSelected >= 0)
            {
                SongsList.EnsureVisible(firstSelected);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ReloadSongFolderRows failed: {ex.Message}");
        }
        finally
        {
            SongsList.EndUpdate();
            Cursor = Cursors.Default;
        }
        ShowStatusBarSummary();
    }

    /// <summary>
    /// HighlightNewFolderItem rewrote a row's title in place (the song stayed in its folder): with a filter
    /// active it is applied again, so a song that no longer matches leaves the list; one that still matches
    /// stays selected (by id). Nothing happens without a filter or after a full folder load (that emptied the box).
    /// </summary>
    private void ReapplySongFindAfterEdit()
    {
        if (songFind == null || !songFindShown || songFindApplied == "" || MainFormClosing)
        {
            return;
        }
        int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
        if (folderNumber > 0)
        {
            ReloadSongFolderRows(folderNumber, songFindApplied);
        }
    }

    /// <summary>
    /// The folder listing reports list the whole folder, as before the find box: with a filter active the box
    /// is emptied and the whole folder loaded first, through the full load path (SongFolder_Change).
    /// </summary>
    private void ShowWholeSongFolderForReport()
    {
        if (songFind == null || (songFindApplied == "" && songFind.FilterText == ""))
        {
            return;
        }
        songFind.CancelPending();
        ImplementFolderChange = true;
        SongFolder_Change();
    }

    private static string SongRowId(ListViewItem row) => row.SubItems.Count > 1 ? row.SubItems[1].Text : "";

    /// <summary>Songs of the folder, counted as FillList lists them without a filter; -1 when unknown.</summary>
    private static int CountSongFolderRows(int folderNumber)
    {
        const string query = "select count(*) from SONG where FolderNo=@folder and (Title_1 is not null or Title_2 is not null)";
        try
        {
            DbConnection connection;
            DbDataReader reader;
            (connection, reader) = DbController.GetDataReader(Gf.ConnectStringMainDB, query, ("@folder", folderNumber));
            using (connection)
            using (reader)
            {
                if (reader != null && reader.Read())
                {
                    return DataUtil.ObjToInt(reader[0]);
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"CountSongFolderRows failed: {ex.Message}");
        }
        return -1;
    }

    /// <summary>
    /// Count part of the status bar's "Folder: " text: the rows listed, exactly as before; while the find box
    /// filters, "n of total items".
    /// </summary>
    private string SongFolderCountText()
    {
        int count = SongsList.Items.Count;
        if (songFindApplied != "" && songFindFolderTotal >= 0)
        {
            int removed = songFindListedAtLoad >= 0 ? Math.Max(0, songFindListedAtLoad - count) : 0;
            int total = Math.Max(count, songFindFolderTotal - removed);
            return count + " of " + total + ((total == 1) ? " item / " : " items / ");
        }
        return count + ((count < 1) ? " item. /" : " items / ");
    }

    /// <summary>
    /// Called by SongFolder_Change before it lists a whole folder: the box is emptied without a reload of its
    /// own. Every full load (another folder, sort order, song numbering, copy / move / edit / recover) shows the
    /// whole folder, so the box never shows a filter the list does not have.
    /// </summary>
    private void ResetSongFindForFolderLoad()
    {
        if (songFind == null)
        {
            return;
        }
        songFind.ResetSilently();
        songFindApplied = "";
        songFindFolderTotal = -1;
        songFindListedAtLoad = -1;
        UpdateSongFindEnabled();
    }

    // Find (toolbar box, Find dialog) shows "Search Results:" without SongFolder_Change: the box is emptied
    // and disabled there.
    private void SongFolder_SelectedIndexChangedForFind(object sender, EventArgs e)
    {
        if (SongFolder.Text == SongSearchResultsFolder)
        {
            ResetSongFindForFolderLoad();
        }
        UpdateSongFindEnabled();
    }

    private void UpdateSongFindEnabled()
    {
        if (songFind == null)
        {
            return;
        }
        bool enabled = SongFolder.Text != SongSearchResultsFolder;
        if (songFind.Enabled != enabled)
        {
            songFind.Enabled = enabled;
        }
    }

    /// <summary>
    /// Gate changed while the app runs (high contrast): the box goes away and the list gets back its place,
    /// size, tab index and whole folder; when the gate opens again the box comes back empty. The painter reads
    /// the gate at every paint; the list is repainted here.
    /// </summary>
    private void RefreshSongListForGate()
    {
        if (songFind == null)
        {
            return;
        }
        bool active = IsModernUiActive;
        if (active == songFindShown)
        {
            return;
        }
        // The painter: attached (OwnerDraw on, erase filter) only while the gate is open; with it closed the
        // list is drawn by the system exactly as the designer sets it up (OwnerDraw false).
        if (active)
        {
            songListPainter?.Attach();
        }
        else
        {
            songListPainter?.Detach();
        }
        SongsList.Invalidate();
        if (!active)
        {
            songFind.CancelPending();
            if (songFindApplied != "")
            {
                int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
                if (folderNumber > 0)
                {
                    ReloadSongFolderRows(folderNumber, "");
                }
            }
            ResetSongFindForFolderLoad();
            songFindShown = false;
            songFind.Visible = false;
            // Designer order again (combo, list, toolbar panel); the hidden box goes after the list.
            SongsList.TabIndex = songsListBaseTabIndex;
            songFind.TabIndex = songsListBaseTabIndex + 1;
        }
        else
        {
            songFindShown = true;
            songFind.Visible = true;
            songFind.TabIndex = songsListBaseTabIndex;
            SongsList.TabIndex = songsListBaseTabIndex + 1;
            UpdateSongFindEnabled();
        }
        LayoutSongFind(listHeightIsBase: false);
    }
}
