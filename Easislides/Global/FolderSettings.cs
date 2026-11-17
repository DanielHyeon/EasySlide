using Easislides.Module;

namespace Easislides.Global
{
    /// <summary>
    /// Settings of one song folder; Gf.Folders is indexed by FolderNo (0 is the Recycle folder). Each field replaced
    /// one per-folder array of Gf (openspec change replace-global-arrays, step 6); defaults are those of the arrays.
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
        // Heading style: [0] from bits 1-3 of the folder's BIUHeading value, [1] from bits 4-6.
        public int[] HeadingFontBold = new int[2];
        public int[] HeadingFontItalic = new int[2];
        public int[] HeadingFontUnderline = new int[2];
        // [0] Region 1, [1] Region 2.
        public string[] FontName = new string[2];
        public int[] FontSize = new int[2];
        public int[] FontVPosition = new int[2];
        public int[] FontAlign = new int[2];
        public int[] FontRTL = new int[2];
        // [0], [1]: Region 1, 2 (bits 1-3 of BIU0, BIU1); [2], [3]: bits 4-6 of BIU0, BIU1.
        public int[] FontBold = new int[4];
        public int[] FontItalic = new int[4];
        public int[] FontUnderline = new int[4];
        public double[] LineSpacing = new double[2];
        public double[] FontSpacingFactor = new double[2];
        public int LeftMargin;
        public int RightMargin;
        public int BottomMargin;

        public FolderSettings Clone()
        {
            FolderSettings copy = (FolderSettings)MemberwiseClone();
            // Arrays inside are references: give the copy its own.
            copy.LyricsHeading = (string[])LyricsHeading.Clone();
            copy.HeadingFontBold = (int[])HeadingFontBold.Clone();
            copy.HeadingFontItalic = (int[])HeadingFontItalic.Clone();
            copy.HeadingFontUnderline = (int[])HeadingFontUnderline.Clone();
            copy.FontName = (string[])FontName.Clone();
            copy.FontSize = (int[])FontSize.Clone();
            copy.FontVPosition = (int[])FontVPosition.Clone();
            copy.FontAlign = (int[])FontAlign.Clone();
            copy.FontRTL = (int[])FontRTL.Clone();
            copy.FontBold = (int[])FontBold.Clone();
            copy.FontItalic = (int[])FontItalic.Clone();
            copy.FontUnderline = (int[])FontUnderline.Clone();
            copy.LineSpacing = (double[])LineSpacing.Clone();
            copy.FontSpacingFactor = (double[])FontSpacingFactor.Clone();
            return copy;
        }
    }
}
