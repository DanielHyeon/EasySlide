using System.Windows;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Controls;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// LiveBarViewModel 단위 테스트 — §6.2.1 LiveBar 동작 검증.
/// </summary>
public class LiveBarViewModelTests
{
    [Fact]
    public void NewViewModel_DefaultsToOff_AndCollapsed()
    {
        var sut = new LiveBarViewModel();

        sut.State.Should().Be(LiveState.Off);
        sut.BarVisibility.Should().Be(Visibility.Collapsed);
        sut.StateLabel.Should().BeEmpty();
    }

    [Theory]
    [InlineData(LiveState.Active, "LIVE")]
    [InlineData(LiveState.Standby, "STANDBY")]
    [InlineData(LiveState.Hidden, "HIDDEN")]
    [InlineData(LiveState.Off, "")]
    public void StateLabel_MapsCorrectly(LiveState state, string expected)
    {
        var sut = new LiveBarViewModel { State = state };
        sut.StateLabel.Should().Be(expected);
    }

    [Fact]
    public void StateChangeToActive_MakesBarVisible()
    {
        var sut = new LiveBarViewModel();
        sut.BarVisibility.Should().Be(Visibility.Collapsed);

        sut.State = LiveState.Active;

        sut.BarVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void StateChangeToOff_HidesBar()
    {
        var sut = new LiveBarViewModel { State = LiveState.Active };
        sut.BarVisibility.Should().Be(Visibility.Visible);

        sut.State = LiveState.Off;

        sut.BarVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void StateChange_RaisesPropertyChangedForBarVisibility()
    {
        var sut = new LiveBarViewModel();
        var changes = new System.Collections.Generic.List<string>();
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is not null) changes.Add(e.PropertyName);
        };

        sut.State = LiveState.Active;

        changes.Should().Contain("State");
        changes.Should().Contain("BarVisibility");
    }

    [Fact]
    public void StateChange_RaisesPropertyChangedForStateLabel()
    {
        // 리뷰 #3 회귀 방지 — State 변경 시 StateLabel도 통지해야 XAML이 자동 갱신
        var sut = new LiveBarViewModel();
        var changes = new System.Collections.Generic.List<string>();
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is not null) changes.Add(e.PropertyName);
        };

        sut.State = LiveState.Active;

        changes.Should().Contain("StateLabel",
            "State 변경 시 StateLabel 파생 속성도 PropertyChanged 발생해야 XAML 바인딩이 갱신됨");
    }

    [Fact]
    public void CurrentItemTitle_SupportsMultilineFormat()
    {
        var sut = new LiveBarViewModel
        {
            State = LiveState.Active,
            CurrentItemTitle = "주일찬양 #3 \"은혜로다\"",
        };

        sut.CurrentItemTitle.Should().Contain("은혜로다");
    }

    [Fact]
    public void ShortcutHint_DefaultIsNonEmpty()
    {
        var sut = new LiveBarViewModel();
        sut.ShortcutHint.Should().NotBeNullOrEmpty("사용자에게 항상 단축키 안내가 보여야 함");
    }

    [Fact]
    public void PositionLabel_DefaultsEmpty_AndSettable()
    {
        // 라이브 위치 라벨(곡 절·PPT 슬라이드) — 기본 빈 문자열(위치 없음→알약 숨김), 설정하면 반영.
        var sut = new LiveBarViewModel();
        sut.PositionLabel.Should().BeEmpty("기본은 빈 문자열");

        sut.PositionLabel = "3/12";
        sut.PositionLabel.Should().Be("3/12");
    }
}
