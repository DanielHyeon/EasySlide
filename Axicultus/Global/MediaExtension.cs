namespace Axicultus.Global;

/// <summary>One entry of Gf.MediaFileExtensions (was a row of the MediaFileExtension array).</summary>
public sealed class MediaExtension
{
    public string Extension = ""; // column 0: lower case, with the dot (".mp4")
    public string TypeName = "";  // column 1: MediaBackgroundStyle.Audio or .Video, as ToString()
}
