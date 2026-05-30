using System.Windows.Media;
using Easislides.Wpf.Rendering;

public sealed record LiveQueueItem(string Id, string Title, string Kind = "Item")
{
    public ImageSource? PreviewSource { get; init; }

    public ImageFillMode PreviewFillMode { get; init; } = ImageFillMode.Fit;

    public int SlideNumber { get; init; }

    /// <summary>곡 항목의 가사(추가 시점에 적재 — 선택 시 미리보기에 표시). 라이브 큐 콘텐츠 plumbing.</summary>
    public string? Lyrics { get; init; }

    /// <summary>미디어/PPT 항목의 파일 경로(선택 시 해당 VM 의 LoadAsync 디스패치에 사용).</summary>
    public string? ContentPath { get; init; }

    public override string ToString() => Title;
}
