using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class PowerPointRenderServiceTests
{
    [Fact]
    public async Task RenderSlideAsync_ReturnsBackendResultAndCachesByFileStamp()
    {
        using var fixture = TempPowerPointFile.Create();
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend);
        var request = DefaultRequest(fixture.Path);

        var first = await sut.RenderSlideAsync(request);
        var second = await sut.RenderSlideAsync(request);

        first.Succeeded.Should().BeTrue();
        first.FromCache.Should().BeFalse();
        second.Succeeded.Should().BeTrue();
        second.FromCache.Should().BeTrue();
        second.Slide.Should().BeSameAs(first.Slide);
        backend.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenFileChanges_InvalidatesCache()
    {
        using var fixture = TempPowerPointFile.Create();
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend);
        var request = DefaultRequest(fixture.Path);

        await sut.RenderSlideAsync(request);
        fixture.TouchWithNewContent();
        await sut.RenderSlideAsync(request);

        backend.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenPixelSizeDiffers_InvalidatesCache()
    {
        // 출력 해상도 렌더(G1.2 후속)의 전제: 같은 파일/슬라이드라도 픽셀 크기가 다르면
        // 별도 캐시 엔트리로 재렌더돼야 한다(캐시 키에 PixelWidth/Height 포함). 이 불변식 회귀 가드.
        using var fixture = TempPowerPointFile.Create();
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend);

        await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { PixelWidth = 960, PixelHeight = 540 });
        await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { PixelWidth = 1920, PixelHeight = 1080 });

        backend.CallCount.Should().Be(2, "픽셀 크기가 다르면 캐시 미스로 재렌더");
    }

    [Fact]
    public async Task RenderSlideAsync_UsesInjectedThumbnailCacheAcrossServiceInstances()
    {
        using var fixture = TempPowerPointFile.Create();
        var cache = new ThumbnailCache();
        var firstBackend = new FakePowerPointRenderBackend();
        var secondBackend = new FakePowerPointRenderBackend();
        var firstService = new PowerPointRenderService(firstBackend, cache);
        var secondService = new PowerPointRenderService(secondBackend, cache);
        var request = DefaultRequest(fixture.Path);

        var first = await firstService.RenderSlideAsync(request);
        var second = await secondService.RenderSlideAsync(request);

        first.FromCache.Should().BeFalse();
        second.FromCache.Should().BeTrue();
        second.Slide.Should().BeSameAs(first.Slide);
        firstBackend.CallCount.Should().Be(1);
        secondBackend.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenRequestTimeoutIsZero_UsesSettingsTimeout()
    {
        using var fixture = TempPowerPointFile.Create();
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateService();
        settings.Set(EasiSettingKeys.PowerPointRenderTimeoutSeconds, 42).Succeeded.Should().BeTrue();
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend, settings);

        var result = await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { Timeout = TimeSpan.Zero });

        result.Succeeded.Should().BeTrue();
        backend.LastRequest!.Timeout.Should().Be(TimeSpan.FromSeconds(42));
    }

    [Fact]
    public async Task RenderSlideAsync_UsesSettingsBackedThumbnailCacheMegabytes()
    {
        using var fixture = TempPowerPointFile.Create();
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateService();
        settings.Set(EasiSettingKeys.ThumbnailCacheMegabytes, 0).Succeeded.Should().BeTrue();
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend, settings);

        var first = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));
        var second = await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { SlideNumber = 2 });
        var third = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));

        first.FromCache.Should().BeFalse();
        second.FromCache.Should().BeFalse();
        third.FromCache.Should().BeFalse();
        backend.CallCount.Should().Be(3);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenThumbnailCacheSettingChanges_ReconfiguresOwnedCache()
    {
        using var fixture = TempPowerPointFile.Create();
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateService();
        settings.Set(EasiSettingKeys.ThumbnailCacheMegabytes, 1).Succeeded.Should().BeTrue();
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend, settings);

        await sut.RenderSlideAsync(DefaultRequest(fixture.Path));
        await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { SlideNumber = 2 });
        var cachedBeforeResize = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));
        settings.Set(EasiSettingKeys.ThumbnailCacheMegabytes, 0).Succeeded.Should().BeTrue();
        var secondAfterResize = await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { SlideNumber = 2 });
        var firstAfterSecondStore = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));

        cachedBeforeResize.FromCache.Should().BeTrue();
        secondAfterResize.FromCache.Should().BeFalse();
        firstAfterSecondStore.FromCache.Should().BeFalse();
        backend.CallCount.Should().Be(4);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenExtensionIsUnsupported_ReturnsUnsupportedWithoutCallingBackend()
    {
        using var fixture = TempPowerPointFile.Create(extension: ".txt");
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend);

        var result = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(PowerPointRenderErrorKind.UnsupportedFile);
        backend.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenFileIsLocked_ReturnsFileLockedWithoutCallingBackend()
    {
        using var fixture = TempPowerPointFile.Create();
        await using var lockStream = new FileStream(fixture.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var backend = new FakePowerPointRenderBackend();
        var sut = new PowerPointRenderService(backend);

        var result = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(PowerPointRenderErrorKind.FileLocked);
        backend.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenBackendExceedsTimeout_ReturnsTimeout()
    {
        using var fixture = TempPowerPointFile.Create();
        var backend = new FakePowerPointRenderBackend
        {
            Delay = TimeSpan.FromSeconds(5)
        };
        var sut = new PowerPointRenderService(backend);

        var result = await sut.RenderSlideAsync(DefaultRequest(fixture.Path) with { Timeout = TimeSpan.FromMilliseconds(25) });

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(PowerPointRenderErrorKind.Timeout);
        backend.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task RenderSlideAsync_WhenBackendThrowsClassifiedException_ReturnsErrorKind()
    {
        using var fixture = TempPowerPointFile.Create();
        var backend = new FakePowerPointRenderBackend
        {
            Exception = new PowerPointRenderException(PowerPointRenderErrorKind.CorruptFile, "Cannot open presentation.")
        };
        var sut = new PowerPointRenderService(backend);

        var result = await sut.RenderSlideAsync(DefaultRequest(fixture.Path));

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(PowerPointRenderErrorKind.CorruptFile);
        result.ErrorMessage.Should().Contain("Cannot open presentation.");
    }

    private static PowerPointRenderRequest DefaultRequest(string filePath)
        => new(filePath, SlideNumber: 1, PixelWidth: 1280, PixelHeight: 720, Timeout: TimeSpan.FromSeconds(1));

    private sealed class FakePowerPointRenderBackend : IPowerPointRenderBackend
    {
        public int CallCount { get; private set; }
        public PowerPointRenderRequest? LastRequest { get; private set; }
        public TimeSpan Delay { get; init; }
        public Exception? Exception { get; init; }

        public async Task<PowerPointSlideSnapshot> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;

            if (Delay > TimeSpan.Zero)
            {
                await Task.Delay(Delay, cancellationToken);
            }

            if (Exception is not null)
            {
                throw Exception;
            }

            return new PowerPointSlideSnapshot(
                request.FilePath,
                request.SlideNumber,
                SlideCount: 3,
                request.PixelWidth,
                request.PixelHeight,
                ImageBytes: new byte[] { 1, 2, 3, (byte)CallCount },
                ContentType: "image/jpeg",
                GeneratedAtUtc: DateTimeOffset.UtcNow);
        }
    }

    private sealed class TempPowerPointFile : IDisposable
    {
        private TempPowerPointFile(string path) => Path = path;

        public string Path { get; }

        public static TempPowerPointFile Create(string extension = ".pptx")
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_{Guid.NewGuid():N}{extension}");
            File.WriteAllText(path, "ppt fixture");
            return new TempPowerPointFile(path);
        }

        public void TouchWithNewContent()
        {
            File.AppendAllText(Path, " updated");
            File.SetLastWriteTimeUtc(Path, DateTime.UtcNow.AddMinutes(1));
        }

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch
            {
            }
        }
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            SettingsPath = Path.Combine(root, "settings.json");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public string BackupRoot { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_PowerPointSettings_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(SettingsPath, BackupRoot));

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
}
