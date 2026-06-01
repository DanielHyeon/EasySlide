using System.Linq;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class SongOrderingTests
{
    private static SongSummary Song(string title, int number = 0)
        => new(number == 0 ? 1 : number, title, "", 1, number, "", "", "");

    [Fact]
    public void Order_Original_PreservesInputOrder()
    {
        var songs = new[] { Song("다"), Song("가"), Song("나") };

        SongOrdering.Order(songs, LibrarySortMode.Original)
            .Select(s => s.Title).Should().Equal("다", "가", "나");
    }

    [Fact]
    public void Order_Title_SortsByTitleAscending()
    {
        var songs = new[] { Song("다윗"), Song("가나안"), Song("Amazing"), Song("나의") };

        SongOrdering.Order(songs, LibrarySortMode.Title)
            .Select(s => s.Title).Should().Equal("Amazing", "가나안", "나의", "다윗");
    }

    [Fact]
    public void Order_Title_EmptyTitlesGoLast()
    {
        var songs = new[] { Song(""), Song("가"), Song("  ") };

        SongOrdering.Order(songs, LibrarySortMode.Title)
            .Select(s => s.Title).Should().Equal("가", "", "  ");
    }

    [Fact]
    public void Order_Number_SortsByNumberThenTitle_ZeroLast()
    {
        var songs = new[] { Song("을", 5), Song("번호없음", 0), Song("갑", 5), Song("처음", 1) };

        SongOrdering.Order(songs, LibrarySortMode.Number)
            .Select(s => s.Title).Should().Equal("처음", "갑", "을", "번호없음");
    }

    [Fact]
    public void Order_IsStable_ForEqualKeys()
    {
        // 같은 번호(5)의 두 곡은 제목 순(갑→을) — ThenBy(Title) 로 결정적.
        var songs = new[] { Song("을", 5), Song("갑", 5) };

        SongOrdering.Order(songs, LibrarySortMode.Number)
            .Select(s => s.Title).Should().Equal("갑", "을");
    }
}
