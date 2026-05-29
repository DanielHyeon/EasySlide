using System.Windows.Media;
using Easislides.Wpf.Rendering;

public sealed record LiveQueueItem(string Id, string Title, string Kind = "Item")
{
    public ImageSource? PreviewSource { get; init; }

    public ImageFillMode PreviewFillMode { get; init; } = ImageFillMode.Fit;

    public int SlideNumber { get; init; }

    public override string ToString() => Title;
}
