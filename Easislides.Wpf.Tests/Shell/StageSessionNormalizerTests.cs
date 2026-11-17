using System;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 증분160-B — 스테이지(Preview) 세션 정규화. 회중 화면이 검정/비움/숨김(Hidden)이어도 스테이지는 가사를
/// 계속 보여 주도록 Hidden→Active 로 되돌린다(콘텐츠 필드는 Hidden 스냅샷에 보존돼 있어 가사가 살아남는다).
/// </summary>
public class StageSessionNormalizerTests
{
    private static LiveSessionSnapshot Hidden(bool blackout, bool cleared, string body = "은혜로다 가사")
        => new(
            LiveState.Hidden,
            "은혜로다",
            "Primary",
            IsBlackout: blackout,
            CurrentItemBodyText: body,
            IsCleared: cleared,
            CurrentItemId: "song-1");

    [Fact]
    public void Active_ReturnsUnchanged()
    {
        var active = new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Primary", IsBlackout: false, CurrentItemBodyText: "은혜로다 가사");

        StageSessionNormalizer.Normalize(active).Should().Be(active);
    }

    [Fact]
    public void Off_ReturnsUnchanged()
    {
        StageSessionNormalizer.Normalize(LiveSessionSnapshot.Off).Should().Be(LiveSessionSnapshot.Off);
    }

    [Fact]
    public void HiddenBlackout_BecomesActive_KeepsLyrics()
    {
        // 회중 화면이 검정(Black)이어도 스테이지는 Active 로 되돌아가 같은 가사를 계속 보여 줘야 한다.
        var result = StageSessionNormalizer.Normalize(Hidden(blackout: true, cleared: false));

        result.State.Should().Be(LiveState.Active);
        result.IsBlackout.Should().BeFalse();
        result.IsCleared.Should().BeFalse();
        result.CurrentItemBodyText.Should().Be("은혜로다 가사", "Hidden 스냅샷이 보존한 가사가 스테이지에 그대로 살아 있어야 함");
        result.CurrentItemTitle.Should().Be("은혜로다");
    }

    [Fact]
    public void HiddenCleared_BecomesActive_KeepsLyrics()
    {
        // 화면 비우기(Clear)도 마찬가지 — 스테이지는 가사를 계속 보여 준다.
        var result = StageSessionNormalizer.Normalize(Hidden(blackout: false, cleared: true));

        result.State.Should().Be(LiveState.Active);
        result.IsCleared.Should().BeFalse();
        result.CurrentItemBodyText.Should().Be("은혜로다 가사");
    }

    [Fact]
    public void Null_Throws()
    {
        var act = () => StageSessionNormalizer.Normalize(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
