using System;
using System.Globalization;

namespace Axicultus.Wpf.Data;

// Column names are the fixed FOLDER schema used by both applications. Older databases may omit columns.
public sealed record FolderFormatField(string Column, string Label, double? Minimum = null, double? Maximum = null,
    bool Decimal = false, string[]? Flags = null)
{
    private static readonly string[] BodyFlags = ["굵게", "기울임", "밑줄", "후렴 굵게", "후렴 기울임", "후렴 밑줄", "오른쪽에서 왼쪽"];
    public static readonly FolderFormatField[] All = [
        new("PreChorusHeading", "프리코러스 제목"), new("ChorusHeading", "후렴 제목"), new("BridgeHeading", "브리지 제목"), new("EndingHeading", "끝 제목"),
        new("HeadingSize", "제목 크기 (%)", 0, 150), new("HeadingOption", "제목 서식 (0 직접 지정 · 1 영역 1과 결합 · 2 영역 1)", 0, 2),
        new("BIUHeading", "제목 서식", Flags: ["굵게", "기울임", "밑줄", "후렴 굵게", "후렴 기울임", "후렴 밑줄"]),
        new("FontName0", "영역 1 글꼴"), new("Size0", "영역 1 글자 크기", 6, 100), new("Vpos0", "영역 1 세로 위치 (%)", 0, 100),
        new("BIU0", "영역 1 서식", Flags: BodyFlags), new("LineSpacing", "영역 1 줄 간격", 0.5, 2, true),
        new("FontName1", "영역 2 글꼴"), new("Size1", "영역 2 글자 크기", 6, 100), new("Vpos1", "영역 2 세로 위치 (%)", 0, 100),
        new("BIU1", "영역 2 서식", Flags: BodyFlags), new("LineSpacing2", "영역 2 줄 간격", 0.5, 2, true),
        new("LMargin", "왼쪽 여백 (%)", 0, 99), new("RMargin", "오른쪽 여백 (%)", 0, 99), new("BMargin", "아래 여백 (%)", 0, 99)
    ];

    public bool IsValid(string text)
    {
        if (Flags is not null) return int.TryParse(text, out var flags) && flags >= 0;
        if (Minimum is null) return text.Length <= 255;
        return double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            && value >= Minimum && value <= Maximum && (Decimal || value == Math.Truncate(value));
    }
}
