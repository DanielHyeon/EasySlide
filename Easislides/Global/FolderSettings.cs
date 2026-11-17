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

        public FolderSettings Clone()
        {
            FolderSettings copy = (FolderSettings)MemberwiseClone();
            // Arrays inside are references: copy each one here when such a field is added.
            return copy;
        }
    }
}
