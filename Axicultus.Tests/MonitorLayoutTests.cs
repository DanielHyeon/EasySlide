using System.Drawing;
using Axicultus.Util;
using Xunit;

namespace Axicultus.Tests;

public class MonitorLayoutTests
{
    private static readonly MonitorDevice Primary = new("PRIMARY", new Rectangle(0, 0, 3440, 1440), true);
    private static readonly MonitorDevice Secondary = new("SECONDARY", new Rectangle(3440, 0, 2560, 1440), false);
    private static readonly MonitorDevice Third = new("THIRD", new Rectangle(-1920, 0, 1920, 1080), false);

    [Theory]
    [InlineData(3600, 80)]
    [InlineData(-1800, 120)]
    [InlineData(100, -1000)]
    public void OperatorWindowRestoresOnTheSavedScreen(int x, int y)
    {
        Rectangle saved = new(x, y, 720, 520);
        var screens = new[] { Primary.Bounds, Secondary.Bounds, Third.Bounds, new Rectangle(0, -1080, 1920, 1080) };
        Assert.Equal(saved, MonitorLayout.RestoreOperatorBounds(saved, screens, Primary.Bounds));
        Assert.True(Primary.Bounds.Contains(MonitorLayout.RestoreOperatorBounds(saved, [Primary.Bounds], Primary.Bounds)));
    }

    [Fact]
    public void LiveMediaUsesItemThenSessionThenCustomAudienceBounds()
    {
        MonitorDevice[] devices = [Primary, Secondary, Third];
        var audience = new MonitorPlacement(Secondary.Name, new Rectangle(3500, 50, 960, 540), true, true);
        Assert.Equal(Third.Name, MonitorLayout.ResolveMedia(devices, "third", Primary.Name, audience, true).DeviceName);
        Assert.Equal(Primary.Name, MonitorLayout.ResolveMedia(devices, "missing", Primary.Name, audience, true).DeviceName);
        Assert.Equal(audience, MonitorLayout.ResolveMedia(devices, null, null, audience, true));
        Assert.Equal(audience, MonitorLayout.ResolveMedia(devices, Secondary.Name, null, audience, true));
    }

    [Theory]
    [InlineData(960, 160, 96, 720, 120, 960, 540)]
    [InlineData(-1920, -120, 96, -1440, -90, 960, 540)]
    [InlineData(-1920, -120, 144, -960, -60, 640, 360)]
    public void CustomPowerPointOriginAndSizeUsePoints(int x, int y, float dpi, int left, int top, int width, int height)
    {
        Assert.Equal(new RectangleF(left, top, width, height),
            Gf.PowerPointWindowPoints(new Rectangle(x, y, 1280, 720), dpi));
    }

    [Theory]
    [InlineData("PowerPoint 슬라이드 쇼  -  주일 예배.pptx  -  읽기 전용", "PowerPoint 슬라이드 쇼  -  주일 예배.pptx")]
    [InlineData("PowerPoint Slide Show - Worship - Read-Only.pptx – Read-Only", "PowerPoint Slide Show - Worship - Read-Only.pptx")]
    [InlineData("PowerPoint Slide Show - Another Worship.pptx", "PowerPoint Slide Show - Another Worship.pptx")]
    public void PowerPointWindowLookupPreservesTheDeckNameWhenRemovingReadOnlyStatus(string caption, string expected)
        => Assert.Equal(expected, OfficeLib.PowerPoint.WithoutReadOnlyCaption(caption));

    [Fact]
    public void ResolvesSelectionAgainAfterDisconnectAndReconnect()
    {
        var before = MonitorLayout.ResolveOutput([Primary, Secondary, Third], "third", true, false);
        var unplugged = MonitorLayout.ResolveOutput([Primary, Secondary], "third", true, false);
        var reconnected = MonitorLayout.ResolveOutput([Primary, Secondary, Third], "third", true, false);

        Assert.Equal("THIRD", before.DeviceName);
        Assert.Equal(new Rectangle(-1680, 0, 1440, 1080), before.Bounds);
        Assert.Equal("SECONDARY", unplugged.DeviceName);
        Assert.Equal(new Rectangle(3760, 0, 1920, 1440), unplugged.Bounds);
        Assert.Equal(before, reconnected);
    }

    [Fact]
    public void SingleDisplayIsModalAndNoneKeepsSafePreviewDimensions()
    {
        var single = MonitorLayout.ResolveOutput([Primary], MonitorLayout.Auto, true, false);
        Assert.True(single.Enabled);
        Assert.False(single.IsDual);
        Assert.Equal("PRIMARY", single.DeviceName);

        var disabled = MonitorLayout.ResolveOutput([Primary, Secondary], MonitorLayout.None, true, false);
        var noScreens = MonitorLayout.ResolveOutput([], MonitorLayout.Auto, true, true);
        Assert.False(disabled.Enabled);
        Assert.False(noScreens.Enabled);
        Assert.True(disabled.Bounds.Width > 0 && disabled.Bounds.Height > 0);
        Assert.True(noScreens.Bounds.Width > 0 && noScreens.Bounds.Height > 0);
    }

    [Fact]
    public void AutomaticOutputFitsAndCentersInsidePortraitAndUltrawideDisplays()
    {
        var portrait = new MonitorDevice("PORTRAIT", new Rectangle(-1080, 20, 1080, 1920), false);
        var standard = MonitorLayout.ResolveOutput([Primary, portrait], "PORTRAIT", true, false);
        var wide = MonitorLayout.ResolveOutput([Primary], "PRIMARY", false, true);

        Assert.Equal(new Rectangle(-1080, 575, 1080, 810), standard.Bounds);
        Assert.Equal(new Rectangle(440, 0, 2560, 1440), wide.Bounds);
    }

    [Theory]
    [InlineData(false, 1920, 1440)]
    [InlineData(true, 1920, 1080)]
    [InlineData(false, 1, 1)]
    [InlineData(true, 1, 1)]
    public void CustomModeRetainsOriginAndUsesPositiveAspectHeight(bool wide, int width, int height)
    {
        var placement = MonitorLayout.ResolveOutput([Primary, Secondary], "PRIMARY", true, wide,
            new Point(3600, 10), width, asSingle: true);

        Assert.Equal("SECONDARY", placement.DeviceName);
        Assert.Equal(new Rectangle(3600, 10, width, height), placement.Bounds);
        Assert.False(placement.IsDual);
        Assert.Equal(height, MonitorLayout.HeightForWidth(width, wide));
    }

    [Fact]
    public void PreviewUsesBothAspectsAndAvailableHeight()
    {
        Rectangle viewport = new(0, 0, 720, 540);
        Assert.Equal(new Size(720, 540), MonitorLayout.Fit(viewport, false).Size);
        Assert.Equal(new Size(720, 405), MonitorLayout.Fit(viewport, true).Size);
        Assert.Equal(new Size(592, 333), MonitorLayout.Fit(new Rectangle(0, 0, 720, 333), true).Size);
    }

    [Fact]
    public void LyricsMonitorLookupUsesNameAndExcludesAudienceDisplay()
    {
        MonitorDevice[] screens = [Primary, Secondary, Third];
        Assert.Equal(Secondary.Bounds, MonitorLayout.FindDisplay(screens, "secondary").Bounds);
        Assert.Null(MonitorLayout.FindDisplay(screens, MonitorLayout.None));
        Assert.Null(MonitorLayout.FindDisplay(screens, null));

        var output = MonitorLayout.ResolveOutput(screens, "SECONDARY", true, false);
        var lyrics = MonitorLayout.ResolveLyrics(screens, "SECONDARY", true, false, output);
        Assert.Equal("THIRD", lyrics.DeviceName);
        Assert.True(lyrics.Enabled);
        Assert.False(MonitorLayout.ResolveLyrics(screens, MonitorLayout.None, true, false, output).Enabled);
        Assert.False(MonitorLayout.ResolveLyrics([Primary, Secondary], "SECONDARY", true, false, output).Enabled);
    }

    [Fact]
    public void AutomaticLyricsFallbackSkipsTheOperatorsActualMonitor()
    {
        var output = MonitorLayout.ResolveOutput([Primary, Secondary], Primary.Name, true, true);
        Assert.False(MonitorLayout.ResolveLyrics([Primary, Secondary], Primary.Name, true, true, output,
            operatorDisplayName: Secondary.Name).Enabled);
        Assert.Equal(Third.Name, MonitorLayout.ResolveLyrics([Primary, Secondary, Third], Primary.Name, true, true,
            output, operatorDisplayName: Secondary.Name).DeviceName);
        Assert.Equal(Secondary.Name, MonitorLayout.ResolveLyrics([Primary, Secondary], Secondary.Name, true, true,
            output, operatorDisplayName: Secondary.Name).DeviceName);
    }

    [Fact]
    public void CustomLyricsWindowCannotCoverTheAudienceWindow()
    {
        var output = MonitorLayout.ResolveOutput([Primary, Secondary, Third], "SECONDARY", true, false);
        var overlap = MonitorLayout.ResolveLyrics([Primary, Secondary, Third], "THIRD", true, false, output, new Point(3800, 0), 640);
        var separate = MonitorLayout.ResolveLyrics([Primary, Secondary, Third], "THIRD", true, false, output, new Point(-1900, 0), 640);
        Assert.False(overlap.Enabled);
        Assert.Equal(new Rectangle(-1900, 0, 640, 480), separate.Bounds);
        Assert.True(separate.Enabled);
    }

    [Fact]
    public void FullyDisconnectedCustomOutputCannotStartOnTheWrongDisplay()
    {
        var output = MonitorLayout.ResolveOutput([Primary], "PRIMARY", true, true, new Point(-1900, 0), 640);
        Assert.False(output.Enabled);
        Assert.Null(output.DeviceName);
        Assert.True(output.Bounds.Width > 0 && output.Bounds.Height > 0);
    }
}
