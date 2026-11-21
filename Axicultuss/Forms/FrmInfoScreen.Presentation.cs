namespace Axicultus;

public partial class FrmInfoScreen
{
    private void ConfigureEditorLayout()
    {
        SuspendLayout();
        try
        {
            Text = "Edit InfoScreen";
            LyricsEditorLayout.FileActions(Main_New, Main_Save, Main_SaveExit, Main_Import, Menu_Save);
            LyricsEditorLayout.AddFormattingRow(this, splitContainerMain, ComboFontName, ComboMainFontSize,
                ComboNotationFontSize, toolStripSeparator4, Main_WordWrap, Main_ChordsMenu, toolStripSeparator2,
                Main_TransposeDown, Main_TransposeUp);
            LyricsEditorLayout.SequenceActions(Verses_Add, Verses_SmartAdd, OrderList_Up, OrderList_Down, OrderList_Delete);
            LyricsEditorLayout.SequenceActions(Rotate_Verses_Add, Rotate_Verses_SmartAdd, Rotate_OrderList_Up,
                Rotate_OrderList_Down, Rotate_OrderList_Delete);
            LyricsEditorLayout.ConfigureAction(Title2_LookUp, "Look up second title", "search");
            LyricsEditorLayout.ConfigureAction(Rotate_LocationBtn, "Choose media file", "folder_open");
            LyricsEditorLayout.AddDetailsTabs(splitContainerMain.Panel1, "Notice", groupBox1, groupBox2,
                panel7, panel8, ResizeTitleFields);
        }
        finally
        {
            ResumeLayout(true);
        }
    }
}
