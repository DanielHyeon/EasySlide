using Easislides.Module;

namespace Easislides.Global
{
    /// <summary>
    /// Settings of one song folder; Gf.Folders is indexed by FolderNo. The per-folder arrays of Gf move here one
    /// field at a time (openspec change replace-global-arrays, step 6): a field is added when its array is removed.
    /// </summary>
    public sealed class FolderSettings
    {
        public string Name = "";
        public int Use;
        public SortBy GroupStyle = SortBy.Alpha;
        public string RenameName;
        public bool FindSongs;
        public string[] LyricsHeading = new string[4]; // 0 PreChorus, 1 Chorus, 2 Bridge, 3 Ending
        public int HeadingPercentSize;
        public int HeadingOption;
        public int[] HeadingFontBold = new int[2];
        public int[] HeadingFontItalic = new int[2];
        public int[] HeadingFontUnderline = new int[2];
        public string[] FontName = new string[2];
        public int[] FontSize = new int[2];
        public int[] FontVPosition = new int[2];
        public int[] FontAlign = new int[2];
        public int[] FontRTL = new int[2];

        public FolderSettings Clone()
        {
            FolderSettings copy = (FolderSettings)MemberwiseClone();
            // Arrays inside are references: copy each one here when such a field is added.
            copy.LyricsHeading = (string[])LyricsHeading.Clone();
            copy.HeadingFontBold = (int[])HeadingFontBold.Clone();
            copy.HeadingFontItalic = (int[])HeadingFontItalic.Clone();
            copy.HeadingFontUnderline = (int[])HeadingFontUnderline.Clone();
            copy.FontName = (string[])FontName.Clone();
            copy.FontSize = (int[])FontSize.Clone();
            copy.FontVPosition = (int[])FontVPosition.Clone();
            copy.FontAlign = (int[])FontAlign.Clone();
            copy.FontRTL = (int[])FontRTL.Clone();
            return copy;
        }
    }
}
