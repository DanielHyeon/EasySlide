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

    // 획수 콜레이션(zh-Hant)이 살아 있는지 — globalization-invariant 모드면 Ordinal 로 폴백해 획수 순서가 깨진다.
    // 그 환경(드문 CI 하드닝 플래그)에선 획수 테스트가 "조용히 뒤집혀 실패"하지 않도록 건너뛴다(운영 기본은 ICU 정상).
    private static bool StrokeCollationAvailable()
    {
        try
        {
            var comparer = System.StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("zh-Hant"), ignoreCase: false);
            return comparer.Compare("人", "三") < 0; // 획수면 人(2획)<三(3획), Ordinal 이면 三(U+4E09)<人(U+4EBA)로 역전.
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            return false;
        }
    }

    [Fact]
    public void Order_StrokeCount_SortsHanjaByStrokeOrder()
    {
        if (!StrokeCollationAvailable())
        {
            return; // invariant 모드 — 획수 콜레이션 없음. 운영(ICU) 환경에서만 검증.
        }

        // 획수 정렬: 한자를 획수 순으로 — 一(1획)·人(2획)·三(3획)·愛(13획)·龍(16획). zh-Hant 콜레이션이 획수 기반.
        var songs = new[] { Song("龍"), Song("三"), Song("一"), Song("愛"), Song("人") };

        SongOrdering.Order(songs, LibrarySortMode.StrokeCount)
            .Select(s => s.Title).Should().Equal("一", "人", "三", "愛", "龍");
    }

    [Fact]
    public void Order_StrokeCount_EmptyTitlesGoLast()
    {
        // 빈 제목을 뒤로 보내는 규칙은 콜레이션과 무관하므로 invariant 모드에서도 성립(건너뛸 필요 없음).
        var songs = new[] { Song(""), Song("一"), Song("  ") };

        SongOrdering.Order(songs, LibrarySortMode.StrokeCount)
            .Select(s => s.Title).Should().Equal("一", "", "  ");
    }

    [Fact]
    public void Order_WordCount_SortsByLegacyCjkWordCountThenStrokeCount()
    {
        // FrmMain FillList: WordCount = ORDER BY cjk_wordcount, cjk_strokecount.
        // 짧은 CJK 제목이 먼저 오고, 같은 글자 수 안에서는 획수/제목 키로 결정된다.
        var songs = new[]
        {
            Song("가나다라"),
            Song("가"),
            Song("가나"),
            Song("Amazing"),
            Song("나"),
        };

        SongOrdering.Order(songs, LibrarySortMode.WordCount)
            .Select(s => s.Title).Should().Equal("Amazing", "가", "나", "가나", "가나다라");
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
