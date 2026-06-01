using System;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class AppearanceTemplateStoreTests
{
    [Fact]
    public async Task SaveThenLoad_RoundTripsAllFields()
    {
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        var template = new LyricsAppearanceTemplate(
            TextColorArgb: unchecked((int)0xFFFFFFFF),
            BackgroundColorArgb: unchecked((int)0xFF101830),
            BackgroundColor2Argb: unchecked((int)0xFF0A1020),
            BackgroundIsGradient: true,
            TextAlignment: LyricsTextAlignment.Left,
            VerticalAlignment: LyricsVerticalAlignment.Bottom,
            FontSize: 64,
            LineSpacingPercent: 150,
            Bold: true,
            Italic: false,
            Shadow: true,
            ShowNotations: false,
            ShowPositionIndicator: true,
            ShowTitleHeading: true,
            Outline: true,
            TitleHeadingAlignment: LyricsTextAlignment.Right,
            TitleHeadingFirstScreenOnly: true,
            BodyLeftMargin: 40,
            BodyRightMargin: 56,
            BodyBottomMargin: 72);

        await store.SaveAsync("주일예배", template);
        var loaded = await store.LoadAsync("주일예배");

        loaded.Should().Be(template, "모든 필드가 디스크 round-trip 으로 보존");
    }

    [Fact]
    public async Task ListNames_ReturnsSavedTemplatesSorted()
    {
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        await store.SaveAsync("저녁", LyricsAppearanceTemplate.Capture(NewSettings()));
        await store.SaveAsync("새벽", LyricsAppearanceTemplate.Capture(NewSettings()));

        store.ListNames().Should().Equal("새벽", "저녁");
    }

    [Fact]
    public async Task Delete_RemovesTemplate()
    {
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        await store.SaveAsync("임시", LyricsAppearanceTemplate.Capture(NewSettings()));

        store.Delete("임시");

        store.ListNames().Should().BeEmpty();
        (await store.LoadAsync("임시")).Should().BeNull();
    }

    [Fact]
    public async Task LoadMissing_ReturnsNull()
    {
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);

        (await store.LoadAsync("없는템플릿")).Should().BeNull();
    }

    [Theory]
    [InlineData("..\\escape")]
    [InlineData("a/b")]
    [InlineData("CON")]
    public async Task Save_RejectsUnsafeNames(string name)
    {
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);

        var act = async () => await store.SaveAsync(name, LyricsAppearanceTemplate.Capture(NewSettings()));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LoadCorruptJson_ReturnsNull()
    {
        // 손상/비-JSON 파일은 null 로 우아하게 처리(호출자 크래시 방지).
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        Directory.CreateDirectory(dir.Path);
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "깨진.json"), "{ this is not valid json ");

        (await store.LoadAsync("깨진")).Should().BeNull();
    }

    [Fact]
    public async Task Save_SameName_Overwrites()
    {
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        var first = LyricsAppearanceTemplate.Capture(NewSettings()) with { FontSize = 40 };
        var second = first with { FontSize = 90 };

        await store.SaveAsync("예배", first);
        await store.SaveAsync("예배", second);

        store.ListNames().Should().ContainSingle();
        (await store.LoadAsync("예배"))!.FontSize.Should().Be(90, "같은 이름은 덮어씀");
    }

    [Fact]
    public void CaptureThenApply_RestoresSettings()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 72);
        settings.Set(EasiSettingKeys.LyricsMonitorBold, true);
        var captured = LyricsAppearanceTemplate.Capture(settings);

        // 값을 바꾼 뒤 템플릿을 되적용하면 원래 값으로 복원된다.
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 40);
        settings.Set(EasiSettingKeys.LyricsMonitorBold, false);
        captured.ApplyTo(settings);

        settings.Get(EasiSettingKeys.LyricsMonitorFontSize).Should().Be(72);
        settings.Get(EasiSettingKeys.LyricsMonitorBold).Should().BeTrue();
    }

    [Fact]
    public void CaptureThenApply_RestoresUnderline()
    {
        // 밑줄도 모양 템플릿에 캡처·복원된다(굵게/기울임/그림자와 동일).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorUnderline, true);
        var captured = LyricsAppearanceTemplate.Capture(settings);

        settings.Set(EasiSettingKeys.LyricsMonitorUnderline, false);
        captured.ApplyTo(settings);

        settings.Get(EasiSettingKeys.LyricsMonitorUnderline).Should().BeTrue();
    }

    [Fact]
    public async Task Load_OldSchemaJsonWithoutTitleHeading_DefaultsFalse()
    {
        // 스키마 진화 안전망(code-review MINOR): ShowTitleHeading 키가 없는 구버전(13필드) JSON 을
        // 불러오면 기본 false 로 채워져 기존 동작(헤딩 off)을 보존해야 한다.
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        Directory.CreateDirectory(dir.Path);
        // 구버전 스토어와 동일한 직렬화 형식(enum=숫자, Center=1) — ShowTitleHeading 키만 없는 13필드.
        var json13 = """
            {
              "TextColorArgb": -1,
              "BackgroundColorArgb": -16777216,
              "BackgroundColor2Argb": -16777216,
              "BackgroundIsGradient": false,
              "TextAlignment": 1,
              "VerticalAlignment": 1,
              "FontSize": 48,
              "LineSpacingPercent": 125,
              "Bold": false,
              "Italic": false,
              "Shadow": false,
              "ShowNotations": true,
              "ShowPositionIndicator": false
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "구버전.json"), json13);

        var loaded = await store.LoadAsync("구버전");

        loaded.Should().NotBeNull();
        loaded!.ShowTitleHeading.Should().BeFalse("구버전 템플릿은 헤딩 off 로 복원돼 기존 동작 보존");
    }

    [Fact]
    public async Task Load_OldSchemaJsonWithoutTitleHeadingAlignment_DefaultsCenter()
    {
        // 스키마 진화 안전망(code-review SUGGESTION): TitleHeadingAlignment 키가 없는 구버전 JSON 을
        // 불러오면 enum 타입 기본값 0(=Left)이 아니라 record 의 명시 기본값 Center 로 복원돼야 한다.
        // (이 키만 빠진 15필드 — ShowTitleHeading/Outline 은 있고 정렬만 없음)
        using var dir = new TempDir();
        var store = new AppearanceTemplateStore(dir.Path);
        Directory.CreateDirectory(dir.Path);
        var json15 = """
            {
              "TextColorArgb": -1,
              "BackgroundColorArgb": -16777216,
              "BackgroundColor2Argb": -16777216,
              "BackgroundIsGradient": false,
              "TextAlignment": 0,
              "VerticalAlignment": 1,
              "FontSize": 48,
              "LineSpacingPercent": 125,
              "Bold": false,
              "Italic": false,
              "Shadow": false,
              "ShowNotations": true,
              "ShowPositionIndicator": false,
              "ShowTitleHeading": true,
              "Outline": false
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "구버전2.json"), json15);

        var loaded = await store.LoadAsync("구버전2");

        loaded.Should().NotBeNull();
        loaded!.TitleHeadingAlignment.Should().Be(
            LyricsTextAlignment.Center, "키가 없으면 enum 0(Left)이 아니라 명시 기본값 Center 로 복원");
        loaded.TextAlignment.Should().Be(LyricsTextAlignment.Left, "본문 정렬은 JSON 값(0=Left) 그대로 — 헤딩 정렬과 독립");
    }

    [Fact]
    public void CaptureThenApply_RestoresTitleHeading()
    {
        // 출력 모양 템플릿이 제목 헤딩 표시 설정까지 캡처·복원하는지(§7.3-A 신규 필드).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, true);
        var captured = LyricsAppearanceTemplate.Capture(settings);

        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, false);
        captured.ApplyTo(settings);

        settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading).Should().BeTrue();
    }

    [Fact]
    public void CaptureThenApply_RestoresOutline()
    {
        // 출력 모양 템플릿이 외곽선 효과 설정까지 캡처·복원하는지(§7.3-A 신규 필드).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorOutline, true);
        var captured = LyricsAppearanceTemplate.Capture(settings);

        settings.Set(EasiSettingKeys.LyricsMonitorOutline, false);
        captured.ApplyTo(settings);

        settings.Get(EasiSettingKeys.LyricsMonitorOutline).Should().BeTrue();
    }

    [Fact]
    public void CaptureThenApply_RestoresTitleHeadingAlignment()
    {
        // 출력 모양 템플릿이 제목 헤딩 정렬까지 캡처·복원하는지(§7.3-A 신규 필드).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment, LyricsTextAlignment.Right);
        var captured = LyricsAppearanceTemplate.Capture(settings);

        settings.Set(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment, LyricsTextAlignment.Center);
        captured.ApplyTo(settings);

        settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment).Should().Be(LyricsTextAlignment.Right);
    }

    [Fact]
    public void CaptureThenApply_RestoresTitleHeadingFirstScreenOnly()
    {
        // 출력 모양 템플릿이 "At First Screen Only" 설정까지 캡처·복원하는지(§7.3-A 신규 필드).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly, true);
        var captured = LyricsAppearanceTemplate.Capture(settings);

        settings.Set(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly, false);
        captured.ApplyTo(settings);

        settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly).Should().BeTrue();
    }

    private static ISettingsService NewSettings()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_TplSettings_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return new SettingsService(new SettingsServiceOptions(
            Path.Combine(root, "settings.json"),
            Path.Combine(root, "Backups")));
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_TplSettings_{Guid.NewGuid():N}"));

        public ISettingsService CreateSettings()
            => new SettingsService(new SettingsServiceOptions(
                Path.Combine(Root, "settings.json"),
                Path.Combine(Root, "Backups")));

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "EasiTpl_" + Guid.NewGuid().ToString("N"));
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch (IOException)
            {
                // 정리 실패는 테스트 결과에 영향 주지 않음.
            }
        }
    }
}
