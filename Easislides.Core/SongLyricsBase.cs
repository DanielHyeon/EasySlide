namespace Easislides.Module
{
    // SongLyrics 의 "포터블 도메인/레이아웃 상태" (§9.4 / core-extraction-plan.md / 작업 B-3 Phase 2b — 상속 분리).
    //
    // [상속 분리 — 왜 이렇게 나눴나]
    //   SongLyrics 는 순수 도메인이 아니라 GDI+ 렌더 상태 홀더다. Font 4종·StringAlignment 같은
    //   System.Drawing 캐시는 포터블 Core(net10.0, 비-Windows)로 옮길 수 없다. 그래서:
    //   - System.Drawing 비의존인 부분(텍스트·표시여부·좌표·레이아웃 캐시 int·색상)만 이 base 에 둔다.
    //   - Font/StringAlignment 등 GDI+ 캐시는 legacy 파생 클래스 Easislides.Module.SongLyrics 에 남긴다.
    //   파생 클래스가 같은 네임스페이스(Easislides.Module)·같은 이름(SongLyrics)을 유지하므로 사용처는 무변경.
    //
    // [색상 디커플 — SongFormat 과 동일 규칙(옵션 B)]
    //   BackColour/ForeColour 는 System.Drawing.Color 가 아니라 ARGB int 로 보관한다.
    //   경계(legacy)에서 Color.FromArgb(int)/Color.ToArgb() 로 변환. 기본값 0 == 과거 default(Color).ToArgb().
    public class SongLyricsBase
    {
        public bool Shadow = true;

        public bool Transparent = true;

        // 배경/글자 색 — ARGB int. 과거 Color BackColour = default 의 ToArgb() 는 0 이므로 기본값 동일.
        public int BackColour = 0;

        public int ForeColour = 0;

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
    }
}
