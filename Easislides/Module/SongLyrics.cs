using System.Drawing;

namespace Easislides.Module
{
    // 포터블 도메인(SongLyricsBase) + GDI+ 렌더 캐시(Font/StringAlignment) (§9.4 / Phase 2b — 상속 분리).
    //
    // 포터블 상태(텍스트·좌표·레이아웃 캐시·색상 ARGB int)는 Easislides.Core 의 SongLyricsBase 에 있고,
    // 이 파생 클래스는 System.Drawing 에 결합된 렌더 캐시만 추가한다.
    //   - Font 4종: SongFormat 설정으로부터 빌드해 캐싱하는 GDI+ 폰트(렌더 핫패스에서 .Name/.Style/.Size 로 재사용).
    //   - TextAlign: StringAlignment(가로 정렬).
    // 네임스페이스(Easislides.Module)·이름(SongLyrics) 유지 → 기존 사용처 코드 무변경.
    public class SongLyrics : SongLyricsBase
    {
        public Font Font;

        public Font ChorusFont;

        public Font FS_Font;

        public Font FS_ChorusFont;

        public StringAlignment TextAlign;

        public void Initialise()
        {
            TextAlign = StringAlignment.Center;
            Shadow = false;
            // ForeColour 는 base 의 ARGB int 필드 — Color 를 ToArgb() 로 변환해 저장.
            ForeColour = Gf.DefaultForeColour.ToArgb();
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
