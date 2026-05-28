using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class OutputWindowServiceTests
{
    [Fact]
    public void Open_Fullscreen_UsesDisplayBounds()
    {
        var sut = new OutputWindowService();
        var display = new OutputDisplay("display-2", "모니터 2", 1920, 0, 1920, 1080, 1.25);

        sut.Open(display, windowed: false);

        sut.Current.IsOpen.Should().BeTrue();
        sut.Current.Display.Should().Be(display);
        sut.Current.Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1920, 1080, false));
    }

    [Fact]
    public void Open_Windowed_CentersSixteenByNinePreviewInsideDisplay()
    {
        var sut = new OutputWindowService();
        var display = new OutputDisplay("display-1", "주 모니터", 0, 0, 1920, 1080, 1.0);

        sut.Open(display, windowed: true);

        sut.Current.Placement.Width.Should().Be(1280);
        sut.Current.Placement.Height.Should().Be(720);
        sut.Current.Placement.Left.Should().Be(320);
        sut.Current.Placement.Top.Should().Be(180);
        sut.Current.Placement.IsWindowed.Should().BeTrue();
    }

    [Fact]
    public void Open_Windowed_ShrinksToFitSmallDisplay()
    {
        var sut = new OutputWindowService();
        var display = new OutputDisplay("small", "작은 모니터", 0, 0, 1024, 768, 1.0);

        sut.Open(display, windowed: true);

        sut.Current.Placement.Width.Should().BeLessThanOrEqualTo(1024);
        sut.Current.Placement.Height.Should().BeLessThanOrEqualTo(768);
        (sut.Current.Placement.Width / sut.Current.Placement.Height).Should().BeApproximately(16d / 9d, 0.01);
    }

    [Fact]
    public void MoveTo_ReplacesDisplayAndPreservesOpenState()
    {
        var sut = new OutputWindowService();
        var display1 = new OutputDisplay("display-1", "주 모니터", 0, 0, 1920, 1080, 1.0);
        var display2 = new OutputDisplay("display-2", "모니터 2", 1920, 0, 1920, 1080, 1.0);

        sut.Open(display1, windowed: true);
        sut.MoveTo(display2, windowed: false);

        sut.Current.IsOpen.Should().BeTrue();
        sut.Current.Display.Should().Be(display2);
        sut.Current.Placement.IsWindowed.Should().BeFalse();
    }

    [Fact]
    public void Close_ClearsOpenState()
    {
        var sut = new OutputWindowService();
        sut.Open(OutputDisplay.PrimaryFallback, windowed: true);

        sut.Close();

        sut.Current.IsOpen.Should().BeFalse();
        sut.Current.Display.Should().BeNull();
    }
}
