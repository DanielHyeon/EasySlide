using System;
using System.Globalization;

namespace Easislides.Wpf.Library;

/// <summary>
/// 레거시 v32 FormatData 를 디코드한 곡-레벨 포맷(색·폰트·크기·정렬·배경). 형식은 "코드=값&gt;코드=값&gt;...".
/// 필드-코드 매핑은 레거시 gfLyrics(HeaderData[n] → 속성)의 권위를 따른다:
///   26/27 = 배경색 region1/2(ARGB int), 29/30 = 글자색 region1/2,
///   43/44 = 폰트명, 47/48 = 글자 크기(6~100), 31/32 = 정렬(1~3), 41 = 글꼴 효과 비트(region1 bit1=Bold·bit2=Italic / region2 bit4·5),
///   61 = 배경 이미지 경로, 51 = 미디어 경로.
/// 곡마다 두 영역(region1/region2 — 이중 언어 등)을 가질 수 있고, 가사의 [region 2] 마커로 줄을 region2 에 배정한다.
/// 인식 못 한 키/형식은 무시한다(레거시 데이터 견고성). 비었으면 null.
/// </summary>
public sealed record SongFormatData
{
    public int? TextColorArgb1 { get; init; }
    public int? TextColorArgb2 { get; init; }
    public int? BackgroundColorArgb1 { get; init; }
    public int? BackgroundColorArgb2 { get; init; }
    public string FontName1 { get; init; } = "";
    public string FontName2 { get; init; } = "";
    public int? FontSize1 { get; init; }
    public int? FontSize2 { get; init; }
    public int? Alignment1 { get; init; }
    public int? Alignment2 { get; init; }
    public bool Bold1 { get; init; }
    public bool Italic1 { get; init; }
    public bool Bold2 { get; init; }
    public bool Italic2 { get; init; }
    public string BackgroundImagePath { get; init; } = "";
    public string MediaPath { get; init; } = "";

    /// <summary>FormatData 문자열을 디코드한다. 비었으면 null. 인식 못 한 키/형식은 건너뛴다.</summary>
    public static SongFormatData? Parse(string? formatData)
    {
        if (string.IsNullOrWhiteSpace(formatData))
        {
            return null;
        }

        int? textColor1 = null, textColor2 = null, backColor1 = null, backColor2 = null;
        int? fontSize1 = null, fontSize2 = null, align1 = null, align2 = null;
        string font1 = "", font2 = "", backgroundImage = "", media = "";
        var effectBits = 0;

        foreach (var entry in formatData.Split('>'))
        {
            var eq = entry.IndexOf('=');
            if (eq <= 0)
            {
                continue; // 키 없는 토큰(머리/꼬리 공백 등)은 무시.
            }

            if (!int.TryParse(entry[..eq], NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
            {
                continue; // 숫자 키가 아니면 무시.
            }

            var value = entry[(eq + 1)..];
            switch (code)
            {
                case 26: backColor1 = ParseArgb(value); break;
                case 27: backColor2 = ParseArgb(value); break;
                case 29: textColor1 = ParseArgb(value); break;
                case 30: textColor2 = ParseArgb(value); break;
                case 31: align1 = ParseAlign(value); break;
                case 32: align2 = ParseAlign(value); break;
                case 41: effectBits = ParseInt(value) ?? 0; break; // 글꼴 효과 비트(레거시 HeaderData[41]).
                case 43: font1 = value; break;
                case 44: font2 = value; break;
                case 47: fontSize1 = ParseFontSize(value); break;
                case 48: fontSize2 = ParseFontSize(value); break;
                case 51: media = value; break;
                case 61: backgroundImage = value; break;
            }
        }

        // 글꼴 효과 비트(레거시): region1 bit1=Bold, bit2=Italic / region2 bit4=Bold, bit5=Italic. 범위 밖이면 무시.
        var hasBits = effectBits is >= 0 and <= 127;

        return new SongFormatData
        {
            TextColorArgb1 = textColor1,
            TextColorArgb2 = textColor2,
            BackgroundColorArgb1 = backColor1,
            BackgroundColorArgb2 = backColor2,
            FontName1 = font1,
            FontName2 = font2,
            FontSize1 = fontSize1,
            FontSize2 = fontSize2,
            Alignment1 = align1,
            Alignment2 = align2,
            Bold1 = hasBits && (effectBits & 0b0000_0001) != 0,
            Italic1 = hasBits && (effectBits & 0b0000_0010) != 0,
            Bold2 = hasBits && (effectBits & 0b0000_1000) != 0,
            Italic2 = hasBits && (effectBits & 0b0001_0000) != 0,
            BackgroundImagePath = backgroundImage,
            MediaPath = media,
        };
    }

    /// <summary>부호 있는 ARGB 정수를 WPF "#AARRGGBB" 16진 문자열로. null 이면 null.</summary>
    public static string? ArgbToHex(int? argb)
        => argb is null ? null : "#" + unchecked((uint)argb.Value).ToString("X8", CultureInfo.InvariantCulture);

    private static int? ParseArgb(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static int? ParseInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    // 폰트 크기는 레거시에서 6~100 만 유효(그 밖은 기본값으로 폴백 → 여기선 무시).
    private static int? ParseFontSize(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n is >= 6 and <= 100 ? n : null;

    // 정렬은 레거시에서 1~3 만 유효(그 밖은 무시).
    private static int? ParseAlign(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n is >= 1 and <= 3 ? n : null;
}
