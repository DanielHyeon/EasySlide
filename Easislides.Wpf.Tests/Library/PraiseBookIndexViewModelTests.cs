using System.Linq;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PraiseBookIndexViewModelTests
{
    [Fact]
    public void Constructor_BuildsGroupsFromSongs()
    {
        var sut = new PraiseBookIndexViewModel(
            new PraiseBookIndexService(),
            new[]
            {
                new PraiseBookIndexEntry("가나", 1),
                new PraiseBookIndexEntry("하늘", 2),
            });

        sut.Groups.Select(g => g.Key).Should().Equal("ㄱ", "ㅎ");
        sut.StatusText.Should().Contain("2곡");
    }

    [Fact]
    public void Constructor_EmptySongs_SetsEmptyStatus()
    {
        var sut = new PraiseBookIndexViewModel(
            new PraiseBookIndexService(),
            System.Array.Empty<PraiseBookIndexEntry>());

        sut.Groups.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }
}
