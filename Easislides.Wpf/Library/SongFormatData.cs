using System;
using System.Globalization;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

/// <summary>
/// 레거시 v32 FormatData 를 디코드한 곡-레벨 포맷(색·폰트·크기·정렬·배경). 형식은 "코드=값&gt;코드=값&gt;...".
/// 필드-코드 매핑은 레거시 gfLyrics(HeaderData[n] → 속성)의 권위를 따른다:
///   26/27 = 배경색 region1/2(ARGB int), 29/30 = 글자색 region1/2,
///   43/44 = 폰트명, 47/48 = 글자 크기(6~100), 31/32 = 정렬(1~3), 41 = 글꼴 효과 비트(0-based: region1 bit0=Bold·bit1=Italic·bit2=Underline / region2 bit3=Bold·bit4=Italic·bit5=Underline),
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
    public bool Underline1 { get; init; }
    public bool Bold2 { get; init; }
    public bool Italic2 { get; init; }
    public bool Underline2 { get; init; }
    public string BackgroundImagePath { get; init; } = "";
    public LyricsBackgroundMode? BackgroundImageMode { get; init; }
    public string MediaPath { get; init; } = "";
    public string ItemTransitionName { get; init; } = "";
    public string SlideTransitionName { get; init; } = "";

    /// <summary>FormatData 문자열을 디코드한다. 비었으면 null. 인식 못 한 키/형식은 건너뛴다.</summary>
    public static SongFormatData? Parse(string? formatData)
    {
        if (string.IsNullOrWhiteSpace(formatData))
        {
            return null;
        }

        int? textColor1 = null, textColor2 = null, backColor1 = null, backColor2 = null;
        int? fontSize1 = null, fontSize2 = null, align1 = null, align2 = null;
        string font1 = "", font2 = "", backgroundImage = "", media = "", itemTransition = "", slideTransition = "";
        LyricsBackgroundMode? backgroundImageMode = null;
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
                case 62: backgroundImageMode = ParseLegacyBackgroundImageMode(value); break;
                case 72: itemTransition = SanitizeTransitionName(value); break;
                case 73: slideTransition = SanitizeTransitionName(value); break;
            }
        }

        // 글꼴 효과 비트(레거시, 0-based): region1 bit0=Bold·bit1=Italic·bit2=Underline / region2 bit3=Bold·bit4=Italic·bit5=Underline. 범위 밖이면 무시.
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
            Bold1 = hasBits && (effectBits & 0b0000_0001) != 0,      // bit0
            Italic1 = hasBits && (effectBits & 0b0000_0010) != 0,    // bit1
            Underline1 = hasBits && (effectBits & 0b0000_0100) != 0, // bit2
            Bold2 = hasBits && (effectBits & 0b0000_1000) != 0,      // bit3
            Italic2 = hasBits && (effectBits & 0b0001_0000) != 0,    // bit4
            Underline2 = hasBits && (effectBits & 0b0010_0000) != 0, // bit5
            BackgroundImagePath = backgroundImage,
            BackgroundImageMode = backgroundImageMode,
            MediaPath = media,
            ItemTransitionName = itemTransition,
            SlideTransitionName = slideTransition,
        };
    }

    /// <summary>
    /// 이 포맷을 레거시 v32 FormatData 문자열("코드=값&gt;코드=값&gt;...")로 인코드한다 — Parse 의 역. 빈 항목은 생략한다
    /// (null 색·크기·정렬, 빈 폰트·경로, 효과 비트 0). Parse(Encode(x)) 가 x 와 같도록 코드 매핑을 정확히 맞춘다.
    /// 곡별 영역 스타일을 인스펙터에서 편집해 저장할 때 쓴다.
    /// </summary>
    public string Encode()
    {
        var parts = new System.Collections.Generic.List<string>();
        void Add(int code, string value) => parts.Add($"{code}={value}");
        string Num(int n) => n.ToString(CultureInfo.InvariantCulture);

        if (BackgroundColorArgb1 is int bc1) Add(26, Num(bc1));
        if (BackgroundColorArgb2 is int bc2) Add(27, Num(bc2));
        if (TextColorArgb1 is int tc1) Add(29, Num(tc1));
        if (TextColorArgb2 is int tc2) Add(30, Num(tc2));
        if (Alignment1 is int a1) Add(31, Num(a1));
        if (Alignment2 is int a2) Add(32, Num(a2));

        // 글꼴 효과 비트(레거시 HeaderData[41], 0-based): region1 bit0=Bold·bit1=Italic·bit2=Underline /
        // region2 bit3=Bold·bit4=Italic·bit5=Underline. (region1=하위 3비트, region2=상위 3비트로 대칭.)
        var effectBits = (Bold1 ? 0b0000_0001 : 0)
                       | (Italic1 ? 0b0000_0010 : 0)
                       | (Underline1 ? 0b0000_0100 : 0)
                       | (Bold2 ? 0b0000_1000 : 0)
                       | (Italic2 ? 0b0001_0000 : 0)
                       | (Underline2 ? 0b0010_0000 : 0);
        if (effectBits != 0) Add(41, Num(effectBits));

        if (!string.IsNullOrEmpty(FontName1)) Add(43, FontName1);
        if (!string.IsNullOrEmpty(FontName2)) Add(44, FontName2);
        if (FontSize1 is int fs1) Add(47, Num(fs1));
        if (FontSize2 is int fs2) Add(48, Num(fs2));
        if (!string.IsNullOrEmpty(MediaPath)) Add(51, MediaPath);
        if (!string.IsNullOrEmpty(BackgroundImagePath)) Add(61, BackgroundImagePath);
        if (ToLegacyBackgroundImageMode(BackgroundImageMode) is int bgMode) Add(62, Num(bgMode));
        if (!string.IsNullOrEmpty(ItemTransitionName)) Add(72, SanitizeTransitionName(ItemTransitionName));
        if (!string.IsNullOrEmpty(SlideTransitionName)) Add(73, SanitizeTransitionName(SlideTransitionName));

        return string.Join(">", parts);
    }

    /// <summary>
    /// 글꼴명을 FormatData 에 안전하게 실을 수 있게 다듬는다 — 앞뒤 공백 제거 + 구분자('>'·'=') 제거.
    /// '>'·'='는 "코드=값>코드=값" 포맷의 구분자라 글꼴명에 섞이면 인코딩이 깨져 엉뚱한 코드가 주입되므로 막는다(실제 글꼴명엔 안 쓰임).
    /// null/공백뿐이면 빈 문자열(=전역 글꼴 추종). 곡 항목 글꼴(43)·공지 글꼴이 함께 쓰는 공통 규약(중복 방지).
    /// </summary>
    public static string SanitizeFontName(string? name)
        => string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : name.Trim().Replace(">", string.Empty).Replace("=", string.Empty);

    public static string SanitizeTransitionName(string? name)
        => string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : name.Trim().Replace(">", string.Empty).Replace("=", string.Empty);

    /// <summary>부호 있는 ARGB 정수를 WPF "#AARRGGBB" 16진 문자열로. null 이면 null.</summary>
    public static string? ArgbToHex(int? argb)
        => argb is null ? null : "#" + unchecked((uint)argb.Value).ToString("X8", CultureInfo.InvariantCulture);

    /// <summary>WPF "#AARRGGBB"(또는 "#RRGGBB") 16진 문자열을 부호 있는 ARGB 정수로. 비거나 형식 오류면 null.</summary>
    public static int? HexToArgb(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        var text = hex.Trim().TrimStart('#');
        if (text.Length == 6)
        {
            text = "FF" + text; // 알파 없으면 불투명으로.
        }

        return text.Length == 8
            && uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb)
            ? unchecked((int)argb)
            : null;
    }

    private static int? ParseArgb(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static int? ParseInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static LyricsBackgroundMode? ParseLegacyBackgroundImageMode(string value)
        => ParseInt(value) switch
        {
            0 => LyricsBackgroundMode.Tile,
            1 => LyricsBackgroundMode.Center,
            2 => LyricsBackgroundMode.Fit,
            _ => null,
        };

    private static int? ToLegacyBackgroundImageMode(LyricsBackgroundMode? mode)
        => mode switch
        {
            LyricsBackgroundMode.Tile => 0,
            LyricsBackgroundMode.Center => 1,
            LyricsBackgroundMode.Fit => 2,
            _ => null,
        };

    // 폰트 크기는 레거시에서 6~100 만 유효(그 밖은 기본값으로 폴백 → 여기선 무시).
    private static int? ParseFontSize(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n is >= 6 and <= 100 ? n : null;

    // 정렬은 레거시에서 1~3 만 유효(그 밖은 무시).
    private static int? ParseAlign(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n is >= 1 and <= 3 ? n : null;
}
