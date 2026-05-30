using System;
using Easislides.Wpf.Media;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Media;

/// <summary>미디어 백엔드 생명주기 브리지 검증 (실제 미디어 백엔드 트랙 1단계).</summary>
public class AttachableMediaPlaybackBackendTests
{
    [Fact]
    public void BeforeAttach_CommandsAreNoOp_AndNotAttached()
    {
        var sut = new AttachableMediaPlaybackBackend();

        sut.IsAttached.Should().BeFalse();
        var act = () =>
        {
            sut.Load(MediaPlaybackSnapshot.Empty);
            sut.Play();
            sut.Pause();
            sut.Stop();
            sut.Seek(TimeSpan.Zero);
            sut.ApplySettings(MediaPlaybackSnapshot.Empty);
        };

        act.Should().NotThrow("부착 전엔 모든 호출이 안전하게 흡수돼야 함");
    }

    [Fact]
    public void AfterAttach_ForwardsCommandsToInner()
    {
        var inner = new RecordingBackend();
        var sut = new AttachableMediaPlaybackBackend();

        sut.Attach(inner);

        sut.IsAttached.Should().BeTrue();
        sut.Play();
        sut.Stop();
        inner.PlayCount.Should().Be(1);
        inner.StopCount.Should().Be(1);
    }

    [Fact]
    public void Attach_ReplaysLastLoadedMedia()
    {
        var sut = new AttachableMediaPlaybackBackend();
        var snapshot = MediaPlaybackSnapshot.Empty with { Source = "intro.mp4" };
        sut.Load(snapshot); // 부착 전 로드(출력 창이 아직 안 열림)

        var inner = new RecordingBackend();
        sut.Attach(inner);

        inner.LoadCount.Should().Be(1, "부착 시점에 진행 중 미디어를 새 백엔드에 재적재");
        inner.LastLoaded!.Source.Should().Be("intro.mp4");
    }

    [Fact]
    public void Detach_StopsForwarding()
    {
        var inner = new RecordingBackend();
        var sut = new AttachableMediaPlaybackBackend();
        sut.Attach(inner);

        sut.Detach();

        sut.IsAttached.Should().BeFalse();
        sut.Play();
        inner.PlayCount.Should().Be(0, "분리 후엔 위임하지 않음");
    }

    [Fact]
    public void Attach_RestoresPlayingIntent()
    {
        // 부착 전 Load→Play(둘 다 흡수) 후 부착 시, 미디어 재적재 + 재생 의도(Play)까지 복원.
        var sut = new AttachableMediaPlaybackBackend();
        sut.Load(MediaPlaybackSnapshot.Empty with { Source = "intro.mp4" });
        sut.Play();

        var inner = new RecordingBackend();
        sut.Attach(inner);

        inner.LoadCount.Should().Be(1);
        inner.PlayCount.Should().Be(1, "부착 시 재생 의도 복원");
    }

    [Fact]
    public void Attach_AfterStop_DoesNotReplayPlay()
    {
        // 정지된 미디어는 재적재(처음 프레임)는 하되 자동 재생하지 않음(서비스 Stopped 상태와 일치).
        var sut = new AttachableMediaPlaybackBackend();
        sut.Load(MediaPlaybackSnapshot.Empty with { Source = "intro.mp4" });
        sut.Stop();

        var inner = new RecordingBackend();
        sut.Attach(inner);

        inner.LoadCount.Should().Be(1);
        inner.PlayCount.Should().Be(0, "정지 상태는 자동 재생하지 않음");
    }

    [Fact]
    public void ReAttach_AfterDetach_ReplaysToNewBackend()
    {
        // 출력 창 재오픈 시나리오: Attach→Detach→Attach(new) 면 새 백엔드에 재적재.
        var sut = new AttachableMediaPlaybackBackend();
        sut.Load(MediaPlaybackSnapshot.Empty with { Source = "intro.mp4" });
        sut.Attach(new RecordingBackend());
        sut.Detach();

        var second = new RecordingBackend();
        sut.Attach(second);

        second.LoadCount.Should().Be(1, "재부착 시 새 백엔드에 현재 미디어 재적재");
    }

    [Fact]
    public void Attach_WithNoPriorLoad_DoesNotCallLoad()
    {
        var sut = new AttachableMediaPlaybackBackend();
        var inner = new RecordingBackend();

        sut.Attach(inner);

        inner.LoadCount.Should().Be(0, "로드된 미디어가 없으면 재적재하지 않음");
    }

    [Fact]
    public void Attach_Null_Throws()
    {
        var sut = new AttachableMediaPlaybackBackend();

        var act = () => sut.Attach(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Detach_WhenNotAttached_IsSafeNoOp()
    {
        var sut = new AttachableMediaPlaybackBackend();

        var act = () => sut.Detach();

        act.Should().NotThrow();
        sut.IsAttached.Should().BeFalse();
    }

    [Fact]
    public void Unload_ForwardsToInner_WhenAttached()
    {
        var inner = new RecordingBackend();
        var sut = new AttachableMediaPlaybackBackend();
        sut.Attach(inner);

        sut.Unload();

        inner.UnloadCount.Should().Be(1, "부착 상태에선 Unload 를 실제 백엔드로 위임");
    }

    [Fact]
    public void Unload_ClearsPendingMedia_SoReAttachDoesNotReplay()
    {
        // 미디어 항목 → 다른 종류 항목 전환 시나리오: 부착 전 로드 후 Unload 하면
        // 이후 출력 창이 열려 Attach 돼도 내려간 미디어를 재적재하지 않아야 한다(가사 화면 유지).
        var sut = new AttachableMediaPlaybackBackend();
        sut.Load(ReadyMedia("intro.mp4"));

        sut.Unload();

        var inner = new RecordingBackend();
        sut.Attach(inner);
        inner.LoadCount.Should().Be(0, "내려간 미디어는 재부착 시 재적재하지 않음");
    }

    [Fact]
    public void Unload_WhileAttached_PreventsReplayOnReAttach()
    {
        // 부착 상태에서 Unload → 출력 창을 닫았다(Detach) 다시 열어도(Attach) 미디어가 되살아나지 않아야 한다.
        // (Unload 가 _lastLoad 를 비웠으므로 — Stop/Detach 와 달리 재적재 대상이 없음.)
        var sut = new AttachableMediaPlaybackBackend();
        sut.Load(ReadyMedia("intro.mp4"));
        sut.Attach(new RecordingBackend());

        sut.Unload();
        sut.Detach();

        var reopened = new RecordingBackend();
        sut.Attach(reopened);
        reopened.LoadCount.Should().Be(0, "Unload 후엔 출력 창 재오픈(재부착) 시에도 재적재하지 않음");
    }

    [Fact]
    public void Unload_BeforeAttach_IsSafeNoOp()
    {
        var sut = new AttachableMediaPlaybackBackend();

        var act = () => sut.Unload();

        act.Should().NotThrow("부착 전 Unload 도 안전하게 흡수돼야 함");
        sut.IsAttached.Should().BeFalse();
    }

    // 실제 서비스(MediaPlaybackService.Load)가 브리지에 넘기는 스냅샷은 항상 State=Ready 다.
    // 테스트도 같은 형태로 만들어 실제 흐름과 일치시킨다.
    private static MediaPlaybackSnapshot ReadyMedia(string source)
        => MediaPlaybackSnapshot.Empty with { State = MediaPlaybackState.Ready, Source = source };

    private sealed class RecordingBackend : IMediaPlaybackBackend
    {
        public int LoadCount { get; private set; }
        public int PlayCount { get; private set; }
        public int StopCount { get; private set; }
        public int UnloadCount { get; private set; }
        public MediaPlaybackSnapshot? LastLoaded { get; private set; }

        public void Load(MediaPlaybackSnapshot snapshot)
        {
            LoadCount++;
            LastLoaded = snapshot;
        }

        public void Play() => PlayCount++;

        public void Pause()
        {
        }

        public void Stop() => StopCount++;

        public void Seek(TimeSpan position)
        {
        }

        public void ApplySettings(MediaPlaybackSnapshot snapshot)
        {
        }

        public void Unload() => UnloadCount++;
    }
}
