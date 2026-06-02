using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 좌측 "검색" 탭이 SearchUsageWindow 의 곡/제목/사용을 서브탭으로 인라인 흡수했는지 정적 XAML 구조로 검증(§7.4 후속).
/// (검색·추가 동작 로직은 MainViewModelTests 가 보장 — 여기선 "서브탭과 바인딩이 화면에 배선됐는가"만 본다.)
/// </summary>
public sealed class SearchTabModesTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    private static XElement SearchModeTabs()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");
        return window.Descendants().Single(
            e => e.Name.LocalName == "TabControl" && Attr(e, "Name") == "SearchModeTabs");
    }

    [Fact]
    public void SearchTab_Hosts_ThreeSubTabs_Songs_Titles_Usage()
    {
        var modeTabs = SearchModeTabs();
        var tags = modeTabs.Elements().Where(e => e.Name.LocalName == "TabItem").Select(t => Attr(t, "Tag")).ToArray();

        tags.Should().Contain(new[] { "SearchSongs", "SearchTitles", "SearchUsage" },
            "검색 탭은 곡/제목/사용 서브탭으로 SearchUsageWindow 를 인라인 흡수");
    }

    [Fact]
    public void TitlesSubTab_BindsLookup_AndAddToQueue()
    {
        var titles = SearchModeTabs().Elements()
            .Single(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "SearchTitles");

        var list = titles.Descendants().Single(e => e.Name.LocalName == "ListBox");
        Attr(list, "ItemsSource").Should().Contain("LookupCandidates", "제목 조회 결과 목록");
        Attr(list, "SelectedItem").Should().Contain("SelectedTitleCandidate").And.Contain("TwoWay");

        // 조회 버튼/추가 버튼이 알맞은 커맨드에 바인딩.
        var commands = titles.Descendants()
            .Where(e => e.Name.LocalName == "Button")
            .Select(b => Attr(b, "Command"))
            .ToArray();
        commands.Should().Contain(c => c.Contains("LookupTitlesCommand"), "제목 조회 커맨드");
        commands.Should().Contain(c => c.Contains("AddLookupTitleCommand"), "선택 제목 추가 커맨드");
    }

    [Fact]
    public void SearchTab_SurfacesValidationMessage_NotJustStatus()
    {
        // 가드 메시지(DB 경로 미준비 등)는 ValidationMessage 에 쓰이므로, 인라인 검색 탭이 이를 표시해야
        // 곡/제목/사용 버튼이 "침묵"하지 않는다(인라인 흡수 시 피드백 사각지대 제거).
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");
        var searchTab = window.Descendants().Single(
            e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "Search");

        searchTab.Descendants().Any(
            e => e.Name.LocalName == "TextBlock" && Attr(e, "Text").Contains("Search.ValidationMessage"))
            .Should().BeTrue("검색 탭은 StatusMessage 뿐 아니라 ValidationMessage(가드 경고)도 표시해야 함");
    }

    [Fact]
    public void SongsSubTab_HasEmptyStateOverlay_BoundToHasNoSearchResults()
    {
        // 증분154 — 곡 검색 결과가 0건일 때(검색 실행 후) "결과 없음" 빈 상태 안내가 같은 셀에 겹쳐 뜨도록 배선됐는지 검증(모던 빈 상태 UX).
        var songs = SearchModeTabs().Elements()
            .Single(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "SearchSongs");

        // 빈 상태 TextBlock 이 Search.HasNoSearchResults 가시성에 바인딩되어 있어야 한다.
        var emptyState = songs.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TextBlock" && Attr(e, "Visibility").Contains("HasNoSearchResults"));
        emptyState.Should().NotBeNull("검색 결과 빈 상태 안내가 HasNoSearchResults 에 바인딩돼야 함");
        Attr(emptyState!, "Text").Should().Contain("결과가 없습니다", "운영자에게 결과 없음을 안내");
        // 빈 상태는 클릭을 막지 않아야 한다(아래 목록·더블클릭 추가가 가려지지 않도록).
        Attr(emptyState!, "IsHitTestVisible").Should().Be("False", "안내는 클릭을 통과시켜야 함");
        // 안내와 결과 목록이 같은 셀(Row 3)에 겹쳐야 "오버레이"가 성립한다 — 둘 다 Grid.Row=3 인지 고정.
        var resultList = songs.Descendants().Single(
            e => e.Name.LocalName == "ListBox" && Attr(e, "ItemsSource").Contains("SearchResults"));
        Attr(emptyState!, "Grid.Row").Should().Be("3");
        Attr(resultList, "Grid.Row").Should().Be("3", "안내와 결과 목록이 같은 셀에 겹쳐야 오버레이가 됨");
    }

    [Fact]
    public void TitlesSubTab_HasEmptyStateOverlay_BoundToHasNoLookupResults()
    {
        // 증분155 — 제목 조회 후보가 0건일 때(조회 실행 후) "결과 없음" 안내가 같은 셀에 겹쳐 뜨도록 배선됐는지(곡 검색 빈 상태와 일관).
        var titles = SearchModeTabs().Elements()
            .Single(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "SearchTitles");

        var emptyState = titles.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TextBlock" && Attr(e, "Visibility").Contains("HasNoLookupResults"));
        emptyState.Should().NotBeNull("제목 조회 빈 상태 안내가 HasNoLookupResults 에 바인딩돼야 함");
        Attr(emptyState!, "Text").Should().Contain("없습니다", "운영자에게 결과 없음을 안내");
        Attr(emptyState!, "IsHitTestVisible").Should().Be("False", "안내는 클릭을 통과시켜야 함");
        // 안내와 후보 목록이 같은 셀(Row 2)에 겹쳐야 오버레이가 성립한다.
        var lookupList = titles.Descendants().Single(
            e => e.Name.LocalName == "ListBox" && Attr(e, "ItemsSource").Contains("LookupCandidates"));
        Attr(emptyState!, "Grid.Row").Should().Be("2");
        Attr(lookupList, "Grid.Row").Should().Be("2", "안내와 후보 목록이 같은 셀에 겹쳐야 오버레이가 됨");
    }

    [Fact]
    public void UsageSubTab_BindsUsageRecords_ReadOnly()
    {
        var usage = SearchModeTabs().Elements()
            .Single(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "SearchUsage");

        var list = usage.Descendants().Single(e => e.Name.LocalName == "ListBox");
        Attr(list, "ItemsSource").Should().Contain("UsageRecords", "송출 이력 목록");

        usage.Descendants().Any(e => e.Name.LocalName == "Button" && Attr(e, "Command").Contains("RefreshUsageCommand"))
            .Should().BeTrue("이력 불러오기 커맨드 배선");

        // 사용 서브탭은 읽기전용 — 삭제/내보내기 같은 관리 커맨드는 인라인에 두지 않는다(기존 창에 남김).
        var commands = usage.Descendants()
            .Where(e => e.Name.LocalName == "Button")
            .Select(b => Attr(b, "Command"))
            .ToArray();
        commands.Should().NotContain(c => c.Contains("DeleteSelectedUsageCommand"), "삭제는 인라인에 두지 않음");
        commands.Should().NotContain(c => c.Contains("ExportUsageReportCommand"), "RTF 내보내기는 인라인에 두지 않음");
    }
}
