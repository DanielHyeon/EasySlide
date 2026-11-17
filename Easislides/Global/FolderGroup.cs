namespace Easislides.Global
{
    /// <summary>
    /// One entry of a folder list of the main form's Images, InfoScreen, Powerpoint or Media tab (Gf.PictureGroups,
    /// Gf.InfoScreenGroups, Gf.PowerpointGroups, Gf.MediaGroups). The list index is the folder combo box's SelectedIndex.
    /// </summary>
    public sealed class FolderGroup
    {
        public string Name = ""; // column 0: shown in the folder combo box
        public string Path = ""; // column 1: folder path, ending with a backslash
    }
}
