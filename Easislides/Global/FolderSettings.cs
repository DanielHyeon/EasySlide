using Easislides.Module;
using System.Runtime.CompilerServices;

namespace Easislides.Global
{
    /// <summary>
    /// Settings of one song folder; Gf.Folders is indexed by FolderNo (0 is the Recycle folder). Each field replaced
    /// one per-folder array of Gf (openspec change replace-global-arrays, step 6); defaults are those of the arrays.
    /// </summary>
    public sealed class FolderSettings
    {
        [InlineArray(2)]
        public struct RegionPair<T>
        {
            private T first;
        }

        [InlineArray(4)]
        public struct FourSlots<T>
        {
            private T first;
        }

        public string Name = "";
        public int Use;
        public SortBy GroupStyle = SortBy.Alpha;
        public string RenameName;
        public bool FindSongs;
        public FourSlots<string> LyricsHeading; // 0 PreChorus, 1 Chorus, 2 Bridge, 3 Ending
        public int HeadingPercentSize;
        public int HeadingOption;
        // Heading style: [0] from bits 1-3 of the folder's BIUHeading value, [1] from bits 4-6.
        public RegionPair<int> HeadingFontBold;
        public RegionPair<int> HeadingFontItalic;
        public RegionPair<int> HeadingFontUnderline;
        // [0] Region 1, [1] Region 2.
        public RegionPair<string> FontName;
        public RegionPair<int> FontSize;
        public RegionPair<int> FontVPosition;
        public RegionPair<int> FontAlign;
        public RegionPair<int> FontRTL;
        // [0], [1]: Region 1, 2 (bits 1-3 of BIU0, BIU1); [2], [3]: bits 4-6 of BIU0, BIU1.
        public FourSlots<int> FontBold;
        public FourSlots<int> FontItalic;
        public FourSlots<int> FontUnderline;
        public RegionPair<double> LineSpacing;
        public RegionPair<double> FontSpacingFactor;
        public int LeftMargin;
        public int RightMargin;
        public int BottomMargin;

        public FolderSettings Clone() => (FolderSettings)MemberwiseClone();
    }
}
