using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Input;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class MainViewModelTests
{
    [Fact]
    public void GoLiveCommand_RequiresSelectionAndOpenOutput()
    {
        var sut = CreateSut();
        var item = new LiveQueueItem("song-1", "입례 찬양");
        sut.LoadQueue(new[] { item });

        sut.GoLiveCommand.CanExecute(null).Should().BeFalse("출력 창이 열리기 전에는 라이브 시작을 막는다");

        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = item;

        sut.GoLiveCommand.CanExecute(null).Should().BeTrue();
        sut.GoLiveCommand.Execute(null);

        sut.LiveBar.State.Should().Be(LiveState.Active);
        sut.LiveBar.CurrentItemTitle.Should().Be("입례 찬양");
        sut.StatusText.Should().Contain("LIVE");
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
    public void NextItemCommand_WhenLive_AdvancesSelectionAndLiveSession()
    {
        var sut = CreateSut();
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = first;
        sut.GoLiveCommand.Execute(null);

        sut.NextItemCommand.Execute(null);

        sut.SelectedItem.Should().Be(second);
        sut.LiveBar.CurrentItemTitle.Should().Be("봉헌 찬양");
    }

    [Fact]
    public async Task BlackScreenCommand_WhenLive_AsksSafetyPromptBeforeChangingState()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        sut.GoLiveCommand.Execute(null);

        await sut.BlackScreenCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle();
        sut.LiveBar.State.Should().Be(LiveState.Active, "사용자가 취소하면 live 상태가 보존되어야 한다");

        prompt.Allow = true;
        await sut.BlackScreenCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Hidden);
        sut.Session.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public void BindShortcuts_RegistersLocalAndGlobalLiveNextCommands()
    {
        var sut = CreateSut();
        var registry = new ShortcutRegistry();

        sut.BindShortcuts(registry);

        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveNext && s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveNext && !s.IsGlobal);
    }

    private static MainViewModel CreateSut(ILiveSafetyPrompt? prompt = null, IDisplayService? display = null)
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var telemetry = new InMemoryCommandTelemetry();
        return new MainViewModel(
            session,
            output,
            prompt ?? new RecordingSafetyPrompt(allow: true),
            telemetry,
            display ?? new FixedDisplayService(OutputDisplay.PrimaryFallback));
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
}
