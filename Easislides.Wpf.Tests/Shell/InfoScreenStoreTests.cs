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
