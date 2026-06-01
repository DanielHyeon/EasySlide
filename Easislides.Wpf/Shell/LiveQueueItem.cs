using System.Windows.Media;
using Easislides.Wpf.Rendering;

public sealed record LiveQueueItem(string Id, string Title, string Kind = LiveItemKinds.Item)
{
    public ImageSource? PreviewSource { get; init; }

    public ImageFillMode PreviewFillMode { get; init; } = ImageFillMode.Fit;

    public int SlideNumber { get; init; }

    /// <summary>곡 번호(레거시 SongNumber) — 출력 "곡 번호 표시"(Display Panel) 설정 on 일 때 회중 화면에 표시. 0이면 미표시.</summary>
    public int SongNumber { get; init; }

    /// <summary>곡 항목의 가사(추가 시점에 적재 — 선택 시 미리보기에 표시). 라이브 큐 콘텐츠 plumbing.</summary>
    public string? Lyrics { get; init; }

    /// <summary>
    /// 곡 절 순서(Sequence) — 절을 [라벨] 마커로 1회 정의하고 "1 C 2 C" 식으로 순서·반복을 지정(레거시 절 순서 모델).
    /// 토큰은 쉼표·공백으로 구분하므로 라벨은 공백 없는 한 단어여야 한다(예: [Verse 1] 은 단일 토큰으로 못 부름 → [V1] 처럼).
    /// 비었거나 라벨과 안 맞으면 가사를 선형으로 페이지네이션(기존 동작). 절 페이지 계산·라이브 투영에 쓰인다.
    /// </summary>
    public string? Sequence { get; init; }

    /// <summary>
    /// 가사 절 단위 페이지네이션의 현재 페이지 인덱스(0-based).
    /// GoLive 시 이 절만 출력 화면에 보인다(PR B 절 단위 페이지네이션).
    /// PPT 의 SlideNumber 와 대칭: 큐에는 0 으로 두고, 라이브 투영 시 MainViewModel 이 현재 페이지를 얹는다.
    /// </summary>
    public int LyricsPageIndex { get; init; }

    /// <summary>
    /// 곡별 출력 포맷(레거시 v32 FORMATDATA, "code=value>…" 사전). 글자·배경색·정렬·폰트를 담는다.
    /// GoLive 시 파싱돼 그 곡만의 색으로 송출(없으면 운영 기본색). 곡이 아니면 null.
    /// </summary>
    public string? FormatData { get; init; }

    /// <summary>미디어/PPT 항목의 파일 경로(선택 시 해당 VM 의 LoadAsync 디스패치에 사용).</summary>
    public string? ContentPath { get; init; }

    /// <summary>
    /// 라이브 투영 시 MainViewModel 이 계산해 얹는 위치 라벨(예: "2/4"). 곡=절 위치, PPT=슬라이드 위치.
    /// 단일 절/슬라이드면 빈 문자열. GoLive 가 스냅샷으로 전달, 출력은 설정 on 일 때만 표시(§7.3-A 인디케이터).
    /// </summary>
    public string PositionLabel { get; init; } = "";

    public override string ToString() => Title;
}
