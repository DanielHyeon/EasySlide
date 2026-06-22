namespace Easislides.Wpf.Shell;

public static class MainCommandIds
{
    public const string OutputOpen = "Output.Open";
    public const string OutputClose = "Output.Close";
    public const string LiveGo = "Live.Go";
    // 레거시 btnToOutputMoveNext(F11) — 선택 항목을 송출하고 곧바로 다음 항목으로 넘어간다(자동 다음 설정과 무관).
    public const string LiveGoAndNext = "Live.GoAndNext";
    // 레거시 글로벌 F8 — PreviewItem 을 OutputItem 으로 복사한다. 라이브 중이면 현재 Preview 를 Output 으로 재송출한다.
    public const string LivePreviewToOutput = "Live.PreviewToOutput";
    // 레거시 글로벌 F7 — PreviewItem 을 OutputItem 으로 복사하고 Black 화면을 해제한다.
    public const string LivePreviewToOutputClearBlack = "Live.PreviewToOutputClearBlack";
    public const string LiveStop = "Live.Stop";
    public const string LiveNext = "Live.Next";
    public const string LivePrevious = "Live.Previous";
    public const string LiveFirst = "Live.First";
    public const string LiveLast = "Live.Last";
    public const string LiveHide = "Live.Hide";
    public const string LiveBlack = "Live.Black";
    // 화면 제어 보강(§7.3-B): 비우기·처음으로·새로고침·복귀·자동회전
    public const string LiveClear = "Live.Clear";
    public const string LiveRestart = "Live.Restart";
    public const string LiveRefresh = "Live.Refresh";
    public const string LiveRestore = "Live.Restore";
    public const string LiveAutoRotate = "Live.AutoRotate";

    // 창 런처(§7.4 — 명령 팔레트로 분리창을 흡수). 실행은 View(MainWindow)가 레지스트리에 등록.
    public const string WindowLibrary = "Window.Library";
    public const string WindowBible = "Window.Bible";
    public const string WindowManageBibleVersions = "Window.ManageBibleVersions";
    public const string WindowSearch = "Window.Search";
    public const string WindowImportExport = "Window.ImportExport";
    public const string WindowExternalFiles = "Window.ExternalFiles";
    public const string WindowManageLists = "Window.ManageLists";
    public const string WindowSettings = "Window.Settings";
    public const string WindowHelp = "Window.Help";
    public const string WindowRegistration = "Window.Registration";
    public const string WindowAbout = "Window.About";
    public const string AddExternalFile = "Item.AddExternalFile";
    public const string WorshipListValidate = "WorshipList.Validate";
    public const string WorshipDuplicateItem = "WorshipList.Duplicate";
    // 예배 순서 항목 재정렬(레거시 위/아래/맨위/맨아래 버튼) — 팔레트·단축키로도 실행. 위/아래는 Ctrl+Shift+Up/Down.
    public const string WorshipMoveItemUp = "WorshipList.MoveUp";
    public const string WorshipMoveItemDown = "WorshipList.MoveDown";
    public const string WorshipMoveItemToTop = "WorshipList.MoveToTop";
    public const string WorshipMoveItemToBottom = "WorshipList.MoveToBottom";
    // 빠른 저장(Ctrl+S) — 현재 세션 이름으로 덮어 저장(미저장 변경 ● 수정됨 과 짝).
    public const string WorshipQuickSave = "WorshipList.QuickSave";
    // 현재 송출(LIVE) 항목 선택 — 미리보기로 앞서 본 뒤 라이브 항목으로 선택 되돌리기.
    public const string WorshipSelectLiveItem = "WorshipList.SelectLiveItem";
    // 레거시 .esw(EasiSlides v3.2) 예배 순서 파일 가져오기(§3.4) — 파일 선택 → 파서 → ImportEswWorshipList.
    public const string ImportLegacyWorshipList = "WorshipList.ImportLegacyEsw";
}
