using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easislides.Module
{
    public class SongLyrics
    {
        public Font Font;

        public Font ChorusFont;

        public Font FS_Font;

        public Font FS_ChorusFont;

        public StringAlignment TextAlign;

        public bool Shadow = true;

        public bool Transparent = true;

        public Color BackColour = default;

        public Color ForeColour = default;

        public string Text = "";

        public bool Visible = true;

        public int Left;

        public int Top;

        public int Width;

        public int Height;

        public int Height_R2Bound;

        public int FS_Left;

        public int FS_Top;

        public int FS_Width;

        public int FS_Height;

        public int FS_Height_R2Bound;

        public int FS_TopOffset;

        public int FS_ComputedHeight;

        public int FS_OneLyricAndNotationHeight;

        public int FS_InterlaceGapHeight;

        public string FS_InterlaceLinePattern = "";

        public void Initialise()
        {
            TextAlign = StringAlignment.Center;
            Shadow = false;
            ForeColour = gf.DefaultForeColour;
            Text = "";
            Visible = true;
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
            FS_Left = 0;
            FS_Top = 0;
            FS_Width = 0;
            FS_Height = 0;
            FS_TopOffset = 0;
            FS_OneLyricAndNotationHeight = 0;
        }
    }

}
