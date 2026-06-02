namespace Easislides.Wpf.Shell;

public static class MainCommandIds
{
    public const string OutputOpen = "Output.Open";
    public const string OutputClose = "Output.Close";
    public const string LiveGo = "Live.Go";
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
}
