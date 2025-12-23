using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easislides.Module
{
    public class SongFormat
    {
        public string FormatString = "";

        public string DBStoredFormat = "";

        public string[] HeaderData = new string[255];

        public int ShowSongHeadings = 1;

        public int ShowSongHeadingsAlign = 0;

        public int ShowLyrics = 1;

        public ImageMode BackgroundMode = ImageMode.BestFit;

        public Color[] ShowScreenColour = new Color[2];

        public Color[] ShowFontColour = new Color[2];

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

        public string BackgroundPicture;

        public int MediaOption;

        public string MediaLocation;

        public int MediaCaptureDeviceNumber;

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
