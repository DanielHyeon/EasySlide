using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeLib
{
    public sealed class ShowCopy          // a class: CopyName is cleared in place (ClearStaleRowsWithName)
    {
        public string SourcePath = "";    // column 0
        public string CopyName = "";      // column 1
        internal SlideImageManifest.SourceVersion OpenedVersion;
    }

    /// <summary>The copies the app opened in PowerPoint for the show. Rows are numbered from 1 and never move.</summary>
    public sealed class ShowCopyList
    {
        private readonly List<ShowCopy> rows = new List<ShowCopy> { new ShowCopy() }; // [0] unused
        public int Count => rows.Count - 1;
        public ShowCopy this[int row] => rows[row];
        public int Add(string sourcePath, string copyName) { rows.Add(new ShowCopy { SourcePath = sourcePath, CopyName = copyName }); return Count; }
        public void Clear() { rows.Clear(); rows.Add(new ShowCopy()); }
    }
}
