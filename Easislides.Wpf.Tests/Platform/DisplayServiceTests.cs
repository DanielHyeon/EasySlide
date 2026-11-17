using System;
using System.Collections.Generic;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Platform;

public class DisplayServiceTests
{
    [Fact]
    public void GetDisplays_WhenReaderReturnsNoScreens_UsesPrimaryFallback()
    {
        var sut = new DisplayService(new FakeDisplayReader(Array.Empty<OutputDisplay>()));

        var displays = sut.GetDisplays();

        displays.Should().ContainSingle().Which.Should().Be(OutputDisplay.PrimaryFallback);
        sut.GetPrimaryDisplay().Should().Be(OutputDisplay.PrimaryFallback);
        sut.GetPreferredDisplay().Should().Be(OutputDisplay.PrimaryFallback);
    }

    [Fact]
    public void GetDisplays_SortsPrimaryFirstThenByCoordinates()
    {
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var right = new OutputDisplay("right", "우측 모니터", 1920, 0, 1920, 1080, 1);
        var top = new OutputDisplay("top", "상단 모니터", 0, -1080, 1920, 1080, 1);
        var sut = new DisplayService(new FakeDisplayReader(new[] { right, top, primary }));

        sut.GetDisplays().Should().Equal(primary, top, right);
    }

    [Fact]
    public void GetPreferredDisplay_WhenPreferredIdExists_ReturnsMatchingDisplay()
    {
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var secondary = new OutputDisplay("secondary", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = new DisplayService(new FakeDisplayReader(new[] { primary, secondary }));

        sut.GetPreferredDisplay("secondary").Should().Be(secondary);
    }

    [Fact]
    public void GetPreferredDisplay_WhenNoPreference_PrefersNonPrimaryOutputDisplay()
    {
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var secondary = new OutputDisplay("secondary", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = new DisplayService(new FakeDisplayReader(new[] { primary, secondary }));

        sut.GetPreferredDisplay().Should().Be(secondary);
    }

    [Fact]
    public void GetPreferredDisplay_WhenPreferredIdIsMissing_FallsBackToOutputDisplay()
    {
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var secondary = new OutputDisplay("secondary", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = new DisplayService(new FakeDisplayReader(new[] { primary, secondary }));

        sut.GetPreferredDisplay("removed-monitor").Should().Be(secondary);
    }

    private sealed class FakeDisplayReader : IDisplayReader
    {
        private readonly IReadOnlyList<OutputDisplay> _displays;

        public FakeDisplayReader(IReadOnlyList<OutputDisplay> displays) => _displays = displays;

        public IReadOnlyList<OutputDisplay> ReadDisplays() => _displays;
    }
}
