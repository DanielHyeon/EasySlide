using Wpf.Ui.Controls;

namespace Easislides.Wpf.Theme;

/// <summary>
/// EasiDS 아이콘 매핑 — 도메인 의미가 있는 키로 WPF UI SymbolRegular을 노출.
/// 매핑 표 원본: <see href="../../docs/ui/icon-migration-map.md"/>
///
/// 사용법 (XAML):
///   <code>&lt;ui:SymbolIcon Symbol="{x:Static theme:EsIcons.Bible}" /&gt;</code>
///
/// 사용법 (C#):
///   <code>button.Icon = new SymbolIcon { Symbol = EsIcons.Bible };</code>
///
/// ADR-0002: Fluent UI System Icons 채택 + 외주 없이 내부 추출.
/// </summary>
public static class EsIcons
{
    // === §1 송출 제어 (라이브) ===

    /// <summary>Black Screen — 화면 검정으로 가림 (라이브 안전 액션)</summary>
    public const SymbolRegular LiveBlack = SymbolRegular.EyeOff24;

    /// <summary>Blue Screen / Hide — 슬라이드 일시 숨김</summary>
    public const SymbolRegular LiveHide = SymbolRegular.Pause24;

    /// <summary>Hide Text — 텍스트만 숨김</summary>
    public const SymbolRegular LiveHideText = SymbolRegular.EyeOff24;

    /// <summary>Live Toggle — 라이브 시작/중지</summary>
    public const SymbolRegular LiveToggle = SymbolRegular.VideoClip24;

    /// <summary>Send to Output — 송출 영역으로 보내기</summary>
    public const SymbolRegular LiveSendToOutput = SymbolRegular.ArrowRight24;

    /// <summary>Move to Output — 송출 영역으로 이동 (원본 제거)</summary>
    public const SymbolRegular LiveMoveToOutput = SymbolRegular.ArrowMove24;

    /// <summary>Live Camera — 실시간 카메라 입력</summary>
    public const SymbolRegular LiveCamera = SymbolRegular.VideoClip24;

    /// <summary>Camcorder — 영상 송출 토글</summary>
    public const SymbolRegular LiveCamcorder = SymbolRegular.Camera24;

    /// <summary>Send — 일반 전송</summary>
    public const SymbolRegular LiveSend = SymbolRegular.Send24;

    /// <summary>New Screen — 새 송출 창</summary>
    public const SymbolRegular LiveNewScreen = SymbolRegular.WindowNew24;

    /// <summary>Hide Display — 송출 화면 숨김</summary>
    public const SymbolRegular LiveHideDisplay = SymbolRegular.EyeOff24;

    /// <summary>Check / Tick — 확인·완료</summary>
    public const SymbolRegular StatusCheck = SymbolRegular.Checkmark24;

    // === §2 콘텐츠 (성경·찬양·미디어·문서) ===

    /// <summary>Bible — 성경 콘텐츠</summary>
    public const SymbolRegular Bible = SymbolRegular.Book24;

    /// <summary>Media Play — 미디어 재생</summary>
    public const SymbolRegular MediaPlay = SymbolRegular.PlayCircle24;

    /// <summary>Notebook — 노트북/찬양 가사</summary>
    public const SymbolRegular Notebook = SymbolRegular.Notebook24;

    /// <summary>Word Document — Word 문서</summary>
    public const SymbolRegular DocumentWord = SymbolRegular.DocumentText24;

    /// <summary>PowerPoint Slide — PPT 슬라이드</summary>
    public const SymbolRegular PowerPointSlide = SymbolRegular.SlideLayout24;

    /// <summary>HTML Document — HTML 출력</summary>
    public const SymbolRegular DocumentHtml = SymbolRegular.Code24;

    // === §3 파일·폴더·관리 ===

    /// <summary>Folder — 닫힌 폴더</summary>
    public const SymbolRegular Folder = SymbolRegular.Folder24;

    /// <summary>Folder Open — 열린 폴더</summary>
    public const SymbolRegular FolderOpen = SymbolRegular.FolderOpen24;

    /// <summary>Add — 신규 추가</summary>
    public const SymbolRegular ActionAdd = SymbolRegular.Add24;

    /// <summary>Delete — 삭제 (단일)</summary>
    public const SymbolRegular ActionDelete = SymbolRegular.Delete24;

    /// <summary>Delete List — 목록 삭제</summary>
    public const SymbolRegular ActionDeleteList = SymbolRegular.Delete24;

    /// <summary>Clear / Eraser — 내용 지움</summary>
    public const SymbolRegular ActionClear = SymbolRegular.Eraser24;

    /// <summary>Edit — 편집</summary>
    public const SymbolRegular ActionEdit = SymbolRegular.Edit24;

    /// <summary>New Item — 신규 아이템 생성</summary>
    public const SymbolRegular ActionNewItem = SymbolRegular.DocumentAdd24;

    /// <summary>Copy — 복사</summary>
    public const SymbolRegular ActionCopy = SymbolRegular.Copy24;

    /// <summary>Move — 이동</summary>
    public const SymbolRegular ActionMove = SymbolRegular.ArrowMove24;

    /// <summary>Move Up — 위로 이동</summary>
    public const SymbolRegular ActionMoveUp = SymbolRegular.ArrowUp24;

    /// <summary>Move Down — 아래로 이동</summary>
    public const SymbolRegular ActionMoveDown = SymbolRegular.ArrowDown24;

    /// <summary>Refresh / Sync — 새로고침</summary>
    public const SymbolRegular ActionRefresh = SymbolRegular.ArrowSync24;

    /// <summary>Find / Search — 검색</summary>
    public const SymbolRegular ActionFind = SymbolRegular.Search24;

    /// <summary>Save — 저장</summary>
    public const SymbolRegular ActionSave = SymbolRegular.Save24;

    // === §4 설정·시스템·도움말 ===

    /// <summary>Settings / Options — 설정</summary>
    public const SymbolRegular Settings = SymbolRegular.Settings24;

    /// <summary>Keyboard / Shortcuts — 단축키</summary>
    public const SymbolRegular Shortcuts = SymbolRegular.Keyboard24;

    /// <summary>Help — 도움말</summary>
    public const SymbolRegular Help = SymbolRegular.QuestionCircle24;

    /// <summary>Question — 질문 표시</summary>
    public const SymbolRegular Question = SymbolRegular.Question24;

    /// <summary>Info — 정보 표시</summary>
    public const SymbolRegular Info = SymbolRegular.Info24;

    /// <summary>Alert / Warning — 경고</summary>
    public const SymbolRegular Alert = SymbolRegular.Warning24;

    /// <summary>Template — 템플릿</summary>
    public const SymbolRegular Template = SymbolRegular.Layer24;

    /// <summary>Contents / List — 목차/리스트</summary>
    public const SymbolRegular Contents = SymbolRegular.TextBulletList24;

    /// <summary>Worship List — 예배 순서 (EasiSlides 도메인 핵심)</summary>
    public const SymbolRegular WorshipList = SymbolRegular.TaskListLtr24;

    /// <summary>Session Note / Notepad — 예배 메모</summary>
    public const SymbolRegular SessionNote = SymbolRegular.Notepad24;

    /// <summary>PPT List — PPT 목록</summary>
    public const SymbolRegular PptList = SymbolRegular.SlideMultiple24;

    /// <summary>PPT Preview — PPT 미리보기 스타일</summary>
    public const SymbolRegular PptPreview = SymbolRegular.SlideContent24;

    /// <summary>Media File — 미디어 파일</summary>
    public const SymbolRegular MediaFile = SymbolRegular.Video24;

    /// <summary>No Rotate / Lock — 회전 잠금</summary>
    public const SymbolRegular NoRotate = SymbolRegular.LockClosed24;

    // === §5 모니터·디스플레이 ===

    /// <summary>Single Screen — 단일 모니터</summary>
    public const SymbolRegular MonitorSingle = SymbolRegular.Desktop24;

    /// <summary>Dual Screens — 듀얼 모니터</summary>
    public const SymbolRegular MonitorDual = SymbolRegular.DualScreen24;

    // === §7 폐기 대체 (단순 작업) ===

    /// <summary>No Image Placeholder — 이미지 없음</summary>
    public const SymbolRegular PlaceholderNoImage = SymbolRegular.ImageMultiple24;

    /// <summary>Bible Go / Play — 성경 송출 시작</summary>
    public const SymbolRegular BibleGo = SymbolRegular.Play24;

    /// <summary>RTF Export — RTF 내보내기</summary>
    public const SymbolRegular ExportRtf = SymbolRegular.DocumentArrowDown24;
}
