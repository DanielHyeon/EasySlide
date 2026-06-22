namespace Easislides.Module
{
    // 한 항목(찬양/성경/PPT)의 "포터블 도메인/런타임 상태" (§9.4 / core-extraction-plan.md / 작업 B-3 Phase 3 — 상속 분리).
    //
    // [상속 분리 — 왜 이렇게 나눴나]
    //   SongSettings 는 도메인 데이터와 함께 GUI/렌더 런타임 상태도 들고 있다(WinForms ListView, legacy SongLyrics 배열).
    //   System.Windows.Forms 와 legacy SongLyrics 결합은 포터블 Core(net10.0, 비-Windows)로 옮길 수 없다. 그래서:
    //   - System.Drawing/WinForms/Gf 비의존인 부분(텍스트·번호·플래그·배열·SongFormat·ItemSource)만 이 base 에 둔다.
    //   - ListView LyricsAndNotationsList, SongLyrics[] Lyrics, Initialise()(static Gf 호출)는 legacy 파생 SongSettings 에 남긴다.
    //   파생 클래스가 같은 네임스페이스(Easislides.Module)·같은 이름(SongSettings)을 유지하므로 사용처는 무변경.
    public class SongSettingsBase
    {
        public ItemSource Source;

        public string ItemID = "";

        public string InMainItemText = "";

        public string InSubItemItem1Text = "";

        public string Type = "";

        public string Title = "";

        public string Title2 = "";

        public int SongNumber = -1;

        public int FolderNo = 0;

        public int HBR2_FolderNo = 0;

        public string Path = "";

        public string AllLyrics = "";

        public string Notations = "";

        public string OriginalNotations = "";

        public int FontSizeFactor = 0;

        public int HBR2_FontSizeFactor = 0;

        public int Capo = -1;

        public string Copyright = "";

        public string FolderName = "";

        public string Writer = "";

        public string Category = "";

        public string Book_Reference = "";

        public string User_Reference = "";

        public string Timing = "";

        public string MusicKey = "";

        public string RotateString = "";

        public int RotateStyle = 1;

        public int RotateGap = 0;

        public int RotateTotal = 0;

        public string RotateTimings = "";

        public string RotateSequence = "";

        public string Settings = "";

        public int CurSlide = 0;

        public bool CurSlideIsVerse = false;

        public int TotalSlides = 0;

        public bool OutputStyleScreen = false;

        public bool AtLiveScreen = false;

        public int[,] Slide = new int[1001, 8];

        public bool UseDefaultFormat = true;

        public int CurItemNo = 0;

        public int TotalItems = 0;

        public string CompleteLyrics = "";

        public bool FirstShowing = true;

        public bool SplitScreens = true;

        public bool Show_BookName = true;

        public bool Show_LicAdim = true;

        public string Show_LicAdminInfo1 = "";

        public string Show_LicAdminInfo2 = "";

        public string In_LicAdminInfo1 = "";

        public string In_LicAdminInfo2 = "";

        public string SongSequence = "";

        public string SongBasicSequence = "";

        public string SongOriginalLoadedSequence = "";

        public int[] SongVerses = new int[100];

        public int[] ChorusSlides = new int[10];

        public bool Verse2Present = false;

        public bool[] VersePresent = new bool[160];

        public int[,] VerseLineLoc = new int[160, 5];

        public bool PrevItemPP = false;

        public string PrevTitle = "";

        public string NextTitle = "";

        public bool GapItemOnDisplay = false;

        public SongFormat Format = new SongFormat();

        public bool isEditable = false;

        public bool isLive = false;
    }
}
