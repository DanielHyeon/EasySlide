using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static Easislides.Gf;

namespace Easislides.Module
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
    public struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;

        public uint wFunc;

        public string pFrom;

        public string pTo;

        public ushort fFlags;

        public int fAnyOperationsAborted;

        public IntPtr hNameMappings;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszProgressTitle;
    }

    // 포터블 도메인(SongSettingsBase) + GUI/렌더 런타임 상태(WinForms ListView, legacy SongLyrics 배열) (§9.4 / Phase 3 — 상속 분리).
    //
    // 포터블 상태(텍스트·번호·플래그·배열·SongFormat·ItemSource)는 Easislides.Core 의 SongSettingsBase 에 있고,
    // 이 파생 클래스는 Core 로 옮길 수 없는 결합만 추가한다.
    //   - LyricsAndNotationsList: System.Windows.Forms.ListView(가사/코드 측정용 런타임 컨트롤).
    //   - Lyrics: legacy SongLyrics(System.Drawing Font 캐시를 가진 렌더 상태) 4개 배열.
    //   - Initialise(): static Easislides.Gf.SetListViewColumns 호출 + SongLyrics 생성.
    // 네임스페이스(Easislides.Module)·이름(SongSettings) 유지 → 기존 사용처 코드 무변경.
    public class SongSettings : SongSettingsBase
    {
        public ListView LyricsAndNotationsList = new ListView();

        public SongLyrics[] Lyrics = new SongLyrics[4];

        public void Initialise()
        {
            SetListViewColumns(LyricsAndNotationsList, 6);
            Title = "";
            Copyright = "";
            TotalSlides = 0;
            CurSlide = 1;
            SongNumber = 0;
            Book_Reference = "";
            User_Reference = "";
            Capo = -1;
            FirstShowing = true;
            SongFormat songFormat = new SongFormat();
            songFormat.ShowSongHeadings = 1;
            songFormat.ShowVerticalAlign = 1;
            songFormat.ShowLeftMargin = 2;
            songFormat.ShowRightMargin = 2;
            songFormat.ShowBottomMargin = 0;
            songFormat.ShowLyrics = 2;
            songFormat.BackgroundMode = ImageMode.BestFit;
            RotateString = "";
            songFormat.ShowItemTransition = 15;
            songFormat.ShowSlideTransition = 0;
            UseDefaultFormat = true;
            CompleteLyrics = "";
            FontSizeFactor = 100;
            Verse2Present = false;
            OutputStyleScreen = false;
            Show_BookName = false;
            Show_LicAdim = true;
            Show_LicAdminInfo1 = "";
            Show_LicAdminInfo2 = "";
            In_LicAdminInfo1 = "";
            In_LicAdminInfo2 = "";
            Lyrics[0] = new SongLyrics();
            Lyrics[1] = new SongLyrics();
            Lyrics[2] = new SongLyrics();
            Lyrics[3] = new SongLyrics();
            Lyrics[0].Initialise();
            Lyrics[1].Initialise();
            Lyrics[2].Initialise();
            Lyrics[3].Initialise();
        }
    }
}
