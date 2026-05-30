using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Media;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_UsesReadableKoreanDefaultStatusAndSampleQueue()
    {
        var sut = CreateSut();

        sut.StatusText.Should().Be("3개 항목 로드됨");
        sut.Queue.Select(item => item.Title)
            .Should()
            .Contain(["예배 시작 안내", "주일 찬양 #1", "말씀 본문"]);
    }

    [Fact]
    public void LoadQueue_WhenEmpty_UsesReadableKoreanStatus()
    {
        var sut = CreateSut();

        sut.LoadQueue([]);

        sut.StatusText.Should().Be("송출할 항목이 없습니다");
    }

    [Fact]
    public async Task GoLiveCommand_RequiresSelectionAndOpenOutput()
    {
        var sut = CreateSut();
        var item = new LiveQueueItem("song-1", "입례 찬양");
        sut.LoadQueue(new[] { item });

        sut.GoLiveCommand.CanExecute(null).Should().BeFalse("출력 창이 열리기 전에는 라이브 시작을 막는다");

        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = item;

        sut.GoLiveCommand.CanExecute(null).Should().BeTrue();
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Active);
        sut.LiveBar.CurrentItemTitle.Should().Be("입례 찬양");
        sut.StatusText.Should().Contain("LIVE");
    }

    [Fact]
    public async Task GoLiveCommand_WhenSafetyPromptDeclines_DoesNotChangeState()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];

        await sut.GoLiveCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle(request => request.ActionName == MainCommandIds.LiveGo);
        sut.LiveBar.State.Should().Be(LiveState.Off);
        sut.Session.Current.State.Should().Be(LiveState.Off);
        sut.StatusText.Should().Be("라이브 안전 확인 취소");
    }

    [Fact]
    public async Task GoLiveCommand_WhenAdvanceNextItemEnabled_PublishesCurrentAndSelectsNext()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("입례 찬양");
        sut.LiveBar.CurrentItemTitle.Should().Be("입례 찬양");
        sut.SelectedItem.Should().Be(second);
    }

    [Fact]
    public void Constructor_UsesOperationalPowerPointAndMediaSettings()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.UsePowerPointTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoPowerPointPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 12).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.UseMediaTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoMediaPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaDirectory, @"C:\EasiSlides\Media").Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LiveCameraNumber, 4).Succeeded.Should().BeTrue();

        var sut = CreateSut(settings: settings);

        sut.IsPowerPointTabVisible.Should().BeTrue();
        sut.IsPowerPointPanelOverlayEnabled.Should().BeFalse();
        sut.PowerPointMaxFiles.Should().Be(12);
        sut.IsMediaTabVisible.Should().BeTrue();
        sut.IsMediaPanelOverlayEnabled.Should().BeFalse();
        sut.MediaDirectory.Should().Be(@"C:\EasiSlides\Media");
        sut.LiveCameraNumber.Should().Be(4);
        sut.LiveCameraSource.Should().Be("<<Capture>>4");
    }

    [Fact]
    public void SettingsChanged_RefreshesOperationalPowerPointAndMediaSettings()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.UsePowerPointTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoPowerPointPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 7).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.UseMediaTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoMediaPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaDirectory, @"D:\Media").Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LiveCameraNumber, 5).Succeeded.Should().BeTrue();

        sut.IsPowerPointTabVisible.Should().BeTrue();
        sut.IsPowerPointPanelOverlayEnabled.Should().BeFalse();
        sut.PowerPointMaxFiles.Should().Be(7);
        sut.IsMediaTabVisible.Should().BeTrue();
        sut.IsMediaPanelOverlayEnabled.Should().BeFalse();
        sut.MediaDirectory.Should().Be(@"D:\Media");
        sut.LiveCameraNumber.Should().Be(5);
        sut.LiveCameraSource.Should().Be("<<Capture>>5");
    }

    [Fact]
    public void LoadQueue_WhenPowerPointItemsExceedConfiguredLimit_DisablesGoLiveUntilLimitIncreases()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 1).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("ppt-1", "Deck 1", "P"),
            new LiveQueueItem("ppt-2", "Deck 2", "PowerPoint"),
        });
        sut.StatusText.Should().Be("PowerPoint 제한 초과: 2/1");
        sut.OpenOutputCommand.Execute(null);

        sut.HasPowerPointLimitViolation.Should().BeTrue();
        sut.GoLiveCommand.CanExecute(null).Should().BeFalse();

        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 2).Succeeded.Should().BeTrue();

        sut.HasPowerPointLimitViolation.Should().BeFalse();
        sut.GoLiveCommand.CanExecute(null).Should().BeTrue();
        sut.StatusText.Should().Be("2개 항목 로드됨");
    }

    [Fact]
    public void AddBibleSelection_InsertsAfterCurrentSelectionAndSelectsInsertedItem()
    {
        var sut = CreateSut();
        var opener = new LiveQueueItem("song-1", "Opening Song", "Song");
        var sermon = new LiveQueueItem("sermon", "Sermon", "Message");
        sut.LoadQueue([opener, sermon]);
        sut.SelectedItem = opener;
        var selection = new BibleSelection("0;kjv.db;niv.db;1;1;1;1;1;", "Genesis 1:1 (KJV/NIV)");

        var inserted = sut.AddBibleSelection(selection);

        inserted.Should().NotBeNull();
        inserted!.Id.Should().Be(selection.IdString);
        inserted.Title.Should().Be(selection.Title);
        inserted.Kind.Should().Be("Bible");
        sut.Queue.Should().Equal(opener, inserted, sermon);
        sut.SelectedItem.Should().Be(inserted);
        sut.StatusText.Should().Contain("성경 구절 추가됨");
        sut.StatusText.Should().Contain("Genesis 1:1");
    }

    [Fact]
    public void AddBibleSelection_WhenSelectionIsEmpty_DoesNotChangeQueue()
    {
        var sut = CreateSut();
        var original = sut.Queue.ToArray();

        var inserted = sut.AddBibleSelection(new BibleSelection("", ""));

        inserted.Should().BeNull();
        sut.Queue.Should().Equal(original);
        sut.StatusText.Should().Be("선택된 성경 구절이 없습니다.");
    }

    [Fact]
    public void OpenOutputCommand_UsesPreferredDisplayFromDisplayService()
    {
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay));

        sut.OutputDisplays.Should().Equal(primary, outputDisplay);
        sut.SelectedOutputDisplay.Should().Be(outputDisplay);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("송출 모니터");
        sut.StatusText.Should().Contain("송출 모니터");
    }

    [Fact]
    public void OpenOutputCommand_UsesDefaultOutputMonitorFromSettings()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DefaultOutputMonitorId, "primary");
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay), settings: settings);

        sut.SelectedOutputDisplay.Should().Be(primary);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("주 모니터");
        sut.StatusText.Should().Contain("주 모니터");
    }

    [Fact]
    public void OpenOutputCommand_WhenAlwaysUseSecondaryMonitorDisabledWithoutDefault_SelectsPrimary()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor, false);
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay), settings: settings);

        sut.SelectedOutputDisplay.Should().Be(primary);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("주 모니터");
        sut.StatusText.Should().Contain("주 모니터");
    }

    [Fact]
    public void OpenOutputCommand_WhenDefaultMonitorMissingAndAlwaysUseSecondaryDisabled_FallsBackToPrimary()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DefaultOutputMonitorId, "removed-monitor");
        settings.Set(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor, false);
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay), settings: settings);

        sut.SelectedOutputDisplay.Should().Be(primary);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("주 모니터");
        sut.StatusText.Should().Contain("주 모니터");
    }

    [Fact]
    public async Task NextItemCommand_WhenLive_AdvancesSelectionAndLiveSession()
    {
        var prompt = new RecordingSafetyPrompt(allow: true);
        var sut = CreateSut(prompt);
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = first;
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();

        sut.NextItemCommand.Execute(null);

        sut.SelectedItem.Should().Be(second);
        sut.LiveBar.CurrentItemTitle.Should().Be("봉헌 찬양");
        prompt.Requests.Should().BeEmpty("라이브 중 Next/Prev 리모컨 흐름은 추가 확인으로 막지 않는다");
    }

    [Fact]
    public async Task BlackScreenCommand_WhenLive_AsksSafetyPromptBeforeChangingState()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        prompt.Allow = true;
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();
        prompt.Allow = false;

        await sut.BlackScreenCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle();
        sut.LiveBar.State.Should().Be(LiveState.Active, "사용자가 취소하면 live 상태가 보존되어야 한다");

        prompt.Allow = true;
        await sut.BlackScreenCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Hidden);
        sut.Session.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public async Task StopLiveCommand_WhenLive_AsksSafetyPromptBeforeStopping()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        prompt.Allow = true;
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();
        prompt.Allow = false;

        await sut.StopLiveCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle(request => request.ActionName == MainCommandIds.LiveStop);
        sut.LiveBar.State.Should().Be(LiveState.Active);

        prompt.Allow = true;
        await sut.StopLiveCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Off);
    }

    [Fact]
    public async Task CloseOutputCommand_WhenLive_AsksSafetyPromptBeforeClosing()
    {
        var prompt = new RecordingSafetyPrompt(allow: true);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();
        prompt.Allow = false;

        await sut.CloseOutputCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle(request => request.ActionName == MainCommandIds.OutputClose);
        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.CloseOutputCommand.CanExecute(null).Should().BeTrue();

        prompt.Allow = true;
        await sut.CloseOutputCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Off);
        sut.CloseOutputCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void BindShortcuts_RegistersLocalAndGlobalLiveNextCommands()
    {
        var catalog = new CommandCatalog();
        var sut = CreateSut(commandCatalog: catalog);
        var registry = new ShortcutRegistry();

        sut.BindShortcuts(registry);

        registry.All.Should().BeEquivalentTo(catalog.GetDefaultShortcuts());
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveGo && !s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveNext && s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveNext && !s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LivePrevious && !s.IsGlobal);
    }

    [Fact]
    public void BindShortcuts_AppliesSavedShortcutOverrides()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var catalog = new CommandCatalog();
        var defaultShortcut = catalog.GetDefaultShortcuts()
            .Single(shortcut => shortcut.CommandName == MainCommandIds.LiveNext && shortcut.IsGlobal);
        settings.SetShortcutOverride(ShortcutSettings.GetSlotId(defaultShortcut), "F8");
        var sut = CreateSut(commandCatalog: catalog, settings: settings);
        var registry = new ShortcutRegistry();

        sut.BindShortcuts(registry);

        registry.All.Should().Contain(shortcut =>
            shortcut.CommandName == MainCommandIds.LiveNext &&
            shortcut.IsGlobal &&
            shortcut.Key == Key.F8);
        registry.All.Should().NotContain(shortcut =>
            shortcut.CommandName == MainCommandIds.LiveNext &&
            shortcut.IsGlobal &&
            shortcut.Key == Key.F5);
    }

    [Fact]
    public void Exposes_Media_PlaybackViewModel()
    {
        // G1.2(gap-analysis §4 G-α): orphaned MediaPlaybackViewModel 이 MainViewModel(→ MainWindow Media 탭)에
        // 연결됐는지 고정. 미디어 미적재 초기엔 Empty 상태(컨트롤 비활성).
        var sut = CreateSut();

        sut.Media.Should().NotBeNull("Media 탭이 바인딩할 재생 컨트롤 VM 이 노출돼야 함");
        sut.Media.State.Should().Be(MediaPlaybackState.Empty, "미디어 미적재 초기 상태");
    }

    [Fact]
    public void Exposes_PowerPoint_PreviewViewModel()
    {
        // G1(gap-analysis §4 G-α): orphaned PowerPoint 렌더 서비스가 MainViewModel(→ MainWindow PowerPoint 탭)에 연결됐는지 고정.
        var sut = CreateSut();

        sut.PowerPoint.Should().NotBeNull("PowerPoint 탭이 바인딩할 PPT 미리보기 VM 이 노출돼야 함");
        sut.PowerPoint.State.Should().Be(PowerPointPreviewState.Idle, "PPT 미적재 초기 상태");
    }

    [Fact]
    public void Dispose_Is_Idempotent_With_Media_ViewModel()
    {
        // MainViewModel 과 DI 컨테이너가 모두 Media VM 해제를 시도할 수 있어(이중 dispose),
        // 반복 호출이 예외 없이 안전해야 한다(MediaPlaybackViewModel.Dispose 멱등 가드 검증).
        var sut = CreateSut();

        var disposeTwice = () =>
        {
            sut.Dispose();
            sut.Dispose();
        };
        disposeTwice.Should().NotThrow();
    }

    private static MainViewModel CreateSut(
        ILiveSafetyPrompt? prompt = null,
        IDisplayService? display = null,
        ICommandCatalog? commandCatalog = null,
        ISettingsService? settings = null)
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var telemetry = new InMemoryCommandTelemetry();
        var media = new MediaPlaybackViewModel(new MediaPlaybackService());
        var powerPoint = new PowerPointPreviewViewModel(new StubPowerPointRenderService());
        return new MainViewModel(
            session,
            output,
            prompt ?? new RecordingSafetyPrompt(allow: true),
            telemetry,
            display ?? new FixedDisplayService(OutputDisplay.PrimaryFallback),
            commandCatalog ?? new CommandCatalog(),
            settings ?? TempSettingsFolder.CreateDetachedSettings(),
            media,
            powerPoint);
    }

    // MainViewModel 은 PowerPoint VM 을 노출만 하고 렌더를 호출하지 않으므로, 실패 결과만 내는 스텁이면 충분.
    private sealed class StubPowerPointRenderService : IPowerPointRenderService
    {
        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PowerPointRenderResult(
                PowerPointRenderErrorKind.MissingOffice, Slide: null, ErrorMessage: "stub", FromCache: false, Elapsed: TimeSpan.Zero));

        public void ClearCache()
        {
        }
    }

    private sealed class RecordingSafetyPrompt : ILiveSafetyPrompt
    {
        public RecordingSafetyPrompt(bool allow) => Allow = allow;

        public bool Allow { get; set; }
        public List<LiveSafetyRequest> Requests { get; } = new();

        public Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(Allow);
        }
    }

    private sealed class FixedDisplayService : IDisplayService
    {
        private readonly IReadOnlyList<OutputDisplay> _displays;

        public FixedDisplayService(params OutputDisplay[] displays) => _displays = displays;

        public IReadOnlyList<OutputDisplay> GetDisplays() => _displays;

        public OutputDisplay GetPrimaryDisplay()
            => _displays.FirstOrDefault(display => display.IsPrimary) ?? _displays[0];

        public OutputDisplay GetPreferredDisplay(string? preferredDisplayId = null)
        {
            if (!string.IsNullOrWhiteSpace(preferredDisplayId))
            {
                var preferred = _displays.FirstOrDefault(display => display.Id == preferredDisplayId);
                if (preferred is not null)
                {
                    return preferred;
                }
            }

            return _displays.FirstOrDefault(display => !display.IsPrimary) ?? GetPrimaryDisplay();
        }
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
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_MainSettings_{Guid.NewGuid():N}"));

        public static ISettingsService CreateDetachedSettings()
        {
            var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_MainSettings_{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            return new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups")));
        }

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
}
