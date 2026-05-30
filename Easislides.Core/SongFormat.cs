namespace Easislides.Module
{
    // 찬양/슬라이드 한 항목의 "표시 형식" 도메인 모델 (§9.4 / core-extraction-plan.md / 작업 B-3 Phase 2).
    //
    // [Core 이동 + System.Drawing 디커플 — 옵션 B]
    //   원래 이 클래스는 색상을 System.Drawing.Color 로 들고 있었지만, 포터블 Core(net10.0, 비-Windows)
    //   에는 System.Drawing 결합을 끌어들이지 않기로 했다. 그래서 색상은 ARGB 정수(int)로 보관한다.
    //   - ARGB int 는 Color.ToArgb() 가 만들어 내는 값과 동일하고, Color.FromArgb(int) 로 되돌릴 수 있다.
    //   - 화면 헤더 직렬화 코드가 이미 ToArgb()/FromArgb() 로 정수를 주고받고 있어 영속 데이터와도 호환된다.
    //   legacy WinForms 사용처는 경계에서 Color.FromArgb / .ToArgb 로 변환해 쓴다.
    public class SongFormat
    {
        public string FormatString = "";

        public string DBStoredFormat = "";

        public string[] HeaderData = new string[255];

        public int ShowSongHeadings = 1;

        public int ShowSongHeadingsAlign = 0;

        public int ShowLyrics = 1;

        public ImageMode BackgroundMode = ImageMode.BestFit;

        // 배경 색상 2개(그라데이션 시작/끝) — ARGB int. Color.FromArgb/ToArgb 로 변환.
        public int[] ShowScreenColour = new int[2];

        // 글자 색상 2개(1행/2행) — ARGB int. Color.FromArgb/ToArgb 로 변환.
        public int[] ShowFontColour = new int[2];

        public int ShowScreenStyle = 0;

        public int UseShadowFont = 1;

        public int UseOutlineFont = 1;

        public int ShowInterlace = 0;

        public int ShowVerticalAlign = 1;

        public int ShowLeftMargin;

        public int ShowRightMargin;

        public int ShowBottomMargin;

        public int ShowNotations = 0;

        public int HideDisplayPanel = 0;

        public int[] ShowFontBold = new int[4];

        public int[] ShowFontItalic = new int[4];

        public int[] ShowFontUnderline = new int[4];

        public int[] ShowFontRTL = new int[2];

        public int[] ShowFontAlign = new int[2];

        public string[] ShowFontName = new string[2];

        public int[] ShowFontVPosition = new int[2];

        public int[] ShowFontSize = new int[2];

        public int[] ShowBibleFontSize = new int[2];

        public int TransposeOffset;

        public int PreviousTransposeOffset;

        // 미초기화(기본 null) 동작 보존을 위해 nullable 로 표기 — legacy 는 Nullable disable 이라 영향 없음.
        public string? BackgroundPicture;

        public int MediaOption;

        public string? MediaLocation;

        public int MediaCaptureDeviceNumber;

        public string MediaOutputMonitorName = "";

        public int MediaVolume = 50;

        public int MediaBalance = 0;

        public int MediaMute = 0;

        public int MediaRepeat = 0;

        public int MediaWidescreen = 0;

        public bool MediaTransparent = false;

        public string ImageString = "";

        public string TempImageFileName = "";

        public int ShowItemTransition;

        public int ShowSlideTransition;
    }
}
