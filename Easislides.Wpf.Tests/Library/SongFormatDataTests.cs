using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

/// <summary>
/// 레거시 v32 FormatData 디코더 검증 — 실제 운영 DB(C:\EasiSlides) 샘플로 필드-코드 매핑을 고정.
/// 형식: "코드=값>코드=값>..." (코드별 곡-레벨 포맷). 매핑은 레거시 gfLyrics(HeaderData[n]) 권위에 따른다:
///   29/30=글자색 region1/2, 26/27=배경색 region1/2(ARGB int), 43/44=폰트명, 47/48=크기(6~100),
///   31/32=정렬(1~3), 41=글꼴효과 비트(r1 bit1=Bold·bit2=Italic, r2 bit4·5), 61=배경이미지, 51=미디어.
/// </summary>
public sealed class SongFormatDataTests
{
    // 실제 DB SONGID 454 "Jesus loves me" 의 FORMATDATA (이중 언어: region1 영어 / region2 한글).
    private const string Sample454 =
        "21=0>23=0>22=32>25=2>26=-16777056>27=-16777056>28=0>29=-16777216>30=-16744193>31=2>32=1>" +
        "41=16>42=16>43=Microsoft Sans Serif>44=HY견고딕>45=0>46=50>47=55>48=44>50=2>51=>52=50>53=-1>" +
        "54=2>55=1>61=C:\\EasiSlides\\Images\\성경\\bible-sermon.jpg>62=2>63=1>64=2>65=2>66=0>71=0>72=None>73=None>";

    [Fact]
    public void Parse_RealSample_DecodesColorsFontsSizesAlignmentBackground()
    {
        var f = SongFormatData.Parse(Sample454);

        f.Should().NotBeNull();
        f!.TextColorArgb1.Should().Be(-16777216, "29 = region1 글자색(검정)");
        f.TextColorArgb2.Should().Be(-16744193, "30 = region2 글자색");
        f.BackgroundColorArgb1.Should().Be(-16777056, "26 = region1 배경색");
        f.FontName1.Should().Be("Microsoft Sans Serif", "43 = region1 폰트");
        f.FontName2.Should().Be("HY견고딕", "44 = region2 폰트");
        f.FontSize1.Should().Be(55, "47 = region1 크기");
        f.FontSize2.Should().Be(44, "48 = region2 크기");
        f.Alignment1.Should().Be(2, "31 = region1 정렬");
        f.Alignment2.Should().Be(1, "32 = region2 정렬");
        f.BackgroundImagePath.Should().Be(@"C:\EasiSlides\Images\성경\bible-sermon.jpg", "61 = 배경 이미지");
        // 실 샘플 41=16=0b10000=bit4 → region2 Italic 만 true(0-based; 레거시 HeaderData[41] 권위).
        f.Bold1.Should().BeFalse();
        f.Italic1.Should().BeFalse();
        f.Bold2.Should().BeFalse();
        f.Italic2.Should().BeTrue("41=16 의 bit4 = region2 Italic");
    }

    [Fact]
    public void Parse_BoldItalicBits_FromField41()
    {
        // 41 = 글꼴 효과 비트(레거시 HeaderData[41]): region1 bit1=Bold, bit2=Italic. 3 = Bold+Italic.
        var f = SongFormatData.Parse("29=-1>41=3>");

        f.Should().NotBeNull();
        f!.Bold1.Should().BeTrue();
        f.Italic1.Should().BeTrue();
    }

    [Fact]
    public void ArgbToHex_ConvertsSignedArgbInt_ToHashHex()
    {
        SongFormatData.ArgbToHex(-16777216).Should().Be("#FF000000", "검정");
        SongFormatData.ArgbToHex(-1).Should().Be("#FFFFFFFF", "흰색");
        SongFormatData.ArgbToHex(-256).Should().Be("#FFFFFF00", "노랑");
        SongFormatData.ArgbToHex(null).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyOrWhitespace_ReturnsNull(string? raw)
    {
        SongFormatData.Parse(raw).Should().BeNull();
    }

    [Fact]
    public void Parse_GarbledOrUnknownKeys_IgnoredGracefully()
    {
        // 알 수 없는 키/형식은 무시하고, 인식 가능한 색만 채운다(레거시 데이터 견고성).
        var f = SongFormatData.Parse("999=xyz>29=-1>nonsense>43=Arial>");

        f.Should().NotBeNull();
        f!.TextColorArgb1.Should().Be(-1);
        f.FontName1.Should().Be("Arial");
    }

    [Fact]
    public void Encode_ThenParse_RoundTripsAllFields()
    {
        // 인스펙터에서 만든 영역별 스타일이 인코드→디코드로 비트 동일하게 보존돼야 한다.
        var original = new SongFormatData
        {
            TextColorArgb1 = -16777216,
            TextColorArgb2 = -16744193,
            BackgroundColorArgb1 = -16777056,
            BackgroundColorArgb2 = 100,
            FontName1 = "Microsoft Sans Serif",
            FontName2 = "HY견고딕",
            FontSize1 = 55,
            FontSize2 = 44,
            Alignment1 = 2,
            Alignment2 = 1,
            Bold1 = true,
            Italic1 = false,
            Bold2 = false,
            Italic2 = true,
            BackgroundImagePath = @"C:\EasiSlides\Images\성경\bible.jpg",
            MediaPath = "clip.mp4",
        };

        var round = SongFormatData.Parse(original.Encode());

        round.Should().Be(original);
    }

    [Fact]
    public void Encode_OmitsEmptyFields()
    {
        // 비어 있는(상속) 항목은 인코드 결과에서 빠진다 — region1 글자색만 설정한 경우.
        var encoded = new SongFormatData { TextColorArgb1 = -1 }.Encode();

        encoded.Should().Be("29=-1");
        SongFormatData.Parse(encoded)!.TextColorArgb1.Should().Be(-1);
    }

    [Fact]
    public void Encode_BoldItalicBits_RoundTrip()
    {
        // 효과 비트: region1 Bold+Italic, region2 Bold → 1|2|8 = 11.
        var encoded = new SongFormatData { Bold1 = true, Italic1 = true, Bold2 = true }.Encode();

        encoded.Should().Contain("41=11");
        var f = SongFormatData.Parse(encoded)!;
        f.Bold1.Should().BeTrue();
        f.Italic1.Should().BeTrue();
        f.Bold2.Should().BeTrue();
        f.Italic2.Should().BeFalse();
    }

    [Theory]
    [InlineData("#FF000000", -16777216)]
    [InlineData("#FFFFFFFF", -1)]
    [InlineData("FFFF00", -256)]     // 알파 생략 → 불투명 노랑(0xFFFFFF00).
    [InlineData("#80FF0000", 2164195328 - 4294967296)] // 반투명 빨강(부호 변환).
    public void HexToArgb_ParsesHashAndAlphalessHex(string hex, long expected)
        => SongFormatData.HexToArgb(hex).Should().Be((int)expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xyz")]
    [InlineData("#12")]
    public void HexToArgb_InvalidOrEmpty_ReturnsNull(string? hex)
        => SongFormatData.HexToArgb(hex).Should().BeNull();
}
