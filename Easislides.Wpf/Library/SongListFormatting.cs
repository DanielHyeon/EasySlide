namespace Easislides.Wpf.Library;

/// <summary>
/// 라이브러리 곡 목록 표시 문자열을 만드는 순수 헬퍼(레거시 Use Song Numbering).
/// 곡 번호 표시가 켜져 있고 번호가 1 이상이면 "번호. 제목", 아니면 제목만 돌려준다.
/// </summary>
public static class SongListFormatting
{
    public static string FormatRow(string? title, int songNumber, bool useNumbering)
    {
        var name = title ?? string.Empty;
        return useNumbering && songNumber > 0 ? $"{songNumber}. {name}" : name;
    }
}
