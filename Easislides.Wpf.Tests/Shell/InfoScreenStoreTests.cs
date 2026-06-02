using System;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>정보 화면(공지) JSON 영속 스토어 검증(레거시 InfoScreen .esi 목록의 첫 슬라이스). 임시 폴더 사용.</summary>
public sealed class InfoScreenStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_IS_{Guid.NewGuid():N}");

    [Fact]
    public async Task Save_Then_Load_RoundTripsTextAndFontSize()
    {
        var store = new InfoScreenStore(_dir);

        await store.SaveAsync("환영 인사", new InfoScreenDto("예배에 오신 것을 환영합니다", 60));
        var loaded = await store.LoadAsync("환영 인사");

        loaded.Should().NotBeNull();
        loaded!.Text.Should().Be("예배에 오신 것을 환영합니다");
        loaded.FontSize.Should().Be(60);
    }

    [Fact]
    public async Task Save_Then_Load_RoundTripsBackgroundColor()
    {
        var store = new InfoScreenStore(_dir);

        await store.SaveAsync("광고", new InfoScreenDto("주보 광고", FontSize: 60, ColorArgb: -1, BackgroundColorArgb: unchecked((int)0xFF202020)));
        var loaded = await store.LoadAsync("광고");

        loaded!.BackgroundColorArgb.Should().Be(unchecked((int)0xFF202020), "배경색도 영속");
        loaded.ColorArgb.Should().Be(-1, "글자색도 함께");
    }

    [Fact]
    public async Task Save_Then_Load_RoundTripsEmphasis()
    {
        var store = new InfoScreenStore(_dir);

        await store.SaveAsync("강조", new InfoScreenDto("중요 공지", Bold: true, Underline: true));
        var loaded = await store.LoadAsync("강조");

        loaded!.Bold.Should().BeTrue("굵게도 영속");
        loaded.Italic.Should().BeFalse("켜지 않은 기울임은 false");
        loaded.Underline.Should().BeTrue("밑줄도 영속");
    }

    [Fact]
    public async Task Save_Then_Load_RoundTripsFontName()
    {
        var store = new InfoScreenStore(_dir);

        await store.SaveAsync("글꼴", new InfoScreenDto("글꼴 공지", FontName: "나눔고딕"));
        var loaded = await store.LoadAsync("글꼴");

        loaded!.FontName.Should().Be("나눔고딕", "글꼴명도 영속");
    }

    [Fact]
    public async Task Load_OldFileWithoutFontNameField_DefaultsToNull()
    {
        // 글꼴명 필드 없는 옛 파일도 역직렬화 시 기본 null(=전역 글꼴)로 안전하게 읽힌다(무회귀).
        Directory.CreateDirectory(_dir);
        var path = Path.Combine(_dir, "옛글꼴.json");
        await File.WriteAllTextAsync(path, "{\"Text\":\"옛 공지\",\"FontSize\":40}");

        var loaded = await new InfoScreenStore(_dir).LoadAsync("옛글꼴");

        loaded!.Text.Should().Be("옛 공지");
        loaded.FontName.Should().BeNull("필드 없는 옛 파일은 기본 null");
    }

    [Fact]
    public async Task Load_OldFileWithoutEmphasisField_DefaultsToFalse()
    {
        // 강조 필드(Bold/Italic/Underline) 없는 옛 파일도 역직렬화 시 기본 false 로 안전하게 읽힌다(무회귀).
        Directory.CreateDirectory(_dir);
        var path = Path.Combine(_dir, "옛강조.json");
        await File.WriteAllTextAsync(path, "{\"Text\":\"옛 공지\",\"FontSize\":40,\"BackgroundColorArgb\":-16777216}");

        var loaded = await new InfoScreenStore(_dir).LoadAsync("옛강조");

        loaded!.BackgroundColorArgb.Should().Be(-16777216, "있는 필드는 읽힘");
        loaded.Bold.Should().BeFalse("강조 필드 없는 옛 파일은 기본 false");
        loaded.Italic.Should().BeFalse();
        loaded.Underline.Should().BeFalse();
    }

    [Fact]
    public async Task Load_OldFileWithoutBackgroundField_DefaultsToZero()
    {
        // 옛 저장 파일(BackgroundColorArgb 필드 없음)도 JSON 역직렬화 시 기본 0=출력 기본 배경으로 안전하게 읽힌다(무회귀).
        Directory.CreateDirectory(_dir);
        var path = Path.Combine(_dir, "옛파일.json");
        await File.WriteAllTextAsync(path, "{\"Text\":\"옛 공지\",\"FontSize\":40,\"Alignment\":1,\"ColorArgb\":-1}");

        var loaded = await new InfoScreenStore(_dir).LoadAsync("옛파일");

        loaded!.Text.Should().Be("옛 공지");
        loaded.ColorArgb.Should().Be(-1);
        loaded.BackgroundColorArgb.Should().Be(0, "필드 없는 옛 파일은 기본 0");
    }

    [Fact]
    public async Task Load_MissingName_ReturnsNull()
    {
        var store = new InfoScreenStore(_dir);
        (await store.LoadAsync("없음")).Should().BeNull();
    }

    [Fact]
    public async Task ListNames_ReturnsSavedSortedByName()
    {
        var store = new InfoScreenStore(_dir);
        await store.SaveAsync("헌금 안내", new InfoScreenDto("헌금 시간입니다"));
        await store.SaveAsync("광고", new InfoScreenDto("이번 주 광고"));

        store.ListNames().Should().BeEquivalentTo(new[] { "광고", "헌금 안내" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task Load_CorruptFile_ReturnsNull_GracefulDegradation()
    {
        // 손상/비-JSON 파일은 null 로 우아하게 처리(호출자 크래시 방지).
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(Path.Combine(_dir, "깨짐.json"), "{not valid json");
        var store = new InfoScreenStore(_dir);

        (await store.LoadAsync("깨짐")).Should().BeNull();
    }

    [Fact]
    public async Task Delete_RemovesSavedScreen()
    {
        var store = new InfoScreenStore(_dir);
        await store.SaveAsync("임시", new InfoScreenDto("지울 것"));

        store.Delete("임시");

        store.ListNames().Should().NotContain("임시");
        (await store.LoadAsync("임시")).Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }
}
