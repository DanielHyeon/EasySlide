using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static Easislides.gf;

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

    public class SongSettings
    {

        public ItemSource Source;

        public ListView LyricsAndNotationsList = new ListView();

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

        public SongLyrics[] Lyrics = new SongLyrics[4];

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
