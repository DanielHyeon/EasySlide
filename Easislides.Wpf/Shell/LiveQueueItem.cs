using System.Windows.Media;
using Easislides.Wpf.Rendering;

public sealed record LiveQueueItem(string Id, string Title, string Kind = LiveItemKinds.Item)
{
    public ImageSource? PreviewSource { get; init; }

    public ImageFillMode PreviewFillMode { get; init; } = ImageFillMode.Fit;

    public int SlideNumber { get; init; }

    /// <summary>곡 항목의 가사(추가 시점에 적재 — 선택 시 미리보기에 표시). 라이브 큐 콘텐츠 plumbing.</summary>
    public string? Lyrics { get; init; }

    /// <summary>
    /// 가사 절 단위 페이지네이션의 현재 페이지 인덱스(0-based).
    /// GoLive 시 이 절만 출력 화면에 보인다(PR B 절 단위 페이지네이션).
    /// PPT 의 SlideNumber 와 대칭: 큐에는 0 으로 두고, 라이브 투영 시 MainViewModel 이 현재 페이지를 얹는다.
    /// </summary>
    public int LyricsPageIndex { get; init; }

    /// <summary>미디어/PPT 항목의 파일 경로(선택 시 해당 VM 의 LoadAsync 디스패치에 사용).</summary>
    public string? ContentPath { get; init; }

    public override string ToString() => Title;
}
