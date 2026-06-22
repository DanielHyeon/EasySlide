// 라이브 큐 항목 종류(LiveQueueItem.Kind)의 정규 값 상수.
// 매직 스트링 산재를 줄이고 오타를 컴파일 단계에서 차단한다(여러 code-review 누적 제안).
// LiveQueueItem 과 동일하게 글로벌 네임스페이스 — Shell/Rendering 양쪽이 레이어링 의존 없이 참조.
// (참고: OutputRenderer/MainViewModel 의 IsPowerPointKind/IsMediaKind 는 레거시 별칭("P"/"PPT"/"M"/"Video" 등)도
//  받는 별칭 매처라 이 상수와 별개다 — 별칭 리스트는 의도적으로 유지.)
public static class LiveItemKinds
{
    public const string Item = "Item";
    public const string Song = "Song";
    public const string Bible = "Bible";
    public const string PowerPoint = "PowerPoint";
    public const string Media = "Media";
    public const string Notice = "Notice";

    /// <summary>공지(InfoScreen) 라이브 항목의 고정 ID — 큐의 어떤 항목 ID 와도 겹치지 않는 센티넬.
    /// 공지 송출 시 _liveItemId 를 이 값으로 두면 슬라이드/절 이동 가드(== 항목 ID)가 자연히 비활성화돼
    /// 공지가 떠 있는 동안 의미 없는 이동 버튼이 켜지지 않는다(레거시 FrmInfoScreen 의 "모달" 송출 의미).</summary>
    public const string NoticeLiveId = "notice:live";
}
