using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

/// <summary>
/// SearchUsageWindow 한글화 잠금(§7.4 정리) — 자체 크롬(타이틀·탭·버튼)과 DataGrid 헤더를
/// 곡 편집기·검색 패널과 동일 한글 용어로 통일했다. 영어 라벨이 다시 새는 회귀를 차단한다.
/// (컨트롤 스타일 사전 미로드로 런타임 인스턴스화 불가 → XAML 구조로 검증, 인접 테스트와 동일 방식.)
/// </summary>
public class SearchUsageWindowLocalizationTests
{
    private static XDocument LoadWindow()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, "Easislides.Wpf/Library/SearchUsageWindow.xaml"), LoadOptions.None);
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    private static string[] AttrValues(XDocument doc, string localName)
        => doc.Descendants()
            .Select(e => Attr(e, localName))
            .Where(v => v.Length > 0)
            .ToArray();

    [Fact]
    public void DataGridHeaders_AreKorean_NoEnglishRemains()
    {
        var window = LoadWindow();
        var headers = AttrValues(window, "Header");

        // 곡 편집기·검색 패널과 동일 용어로 통일된 한글 헤더가 존재해야 한다.
        headers.Should().Contain(new[] { "제목", "폴더", "번호", "키", "일치 항목", "발췌", "다른 제목", "교단 참조", "사용자 참조", "날짜", "예배 순서", "횟수" });
        // 영어 헤더가 남으면 안 된다(용어 통일 회귀 방지).
        headers.Should().NotContain(new[] { "Title", "Folder", "No.", "Key", "Match", "Snippet", "Alt", "Book", "User", "Date", "List", "Count" });
    }

    [Fact]
    public void TabHeaders_AndWindowTitle_AreKorean()
    {
        var window = LoadWindow();
        var headers = AttrValues(window, "Header");

        // 탭 머리글(곡/제목/사용 현황)이 한글이어야 한다.
        headers.Should().Contain(new[] { "곡", "사용 현황" });
        headers.Should().NotContain(new[] { "Songs", "Titles", "Usage" });

        // 창 제목도 한글.
        var title = window.Root!.Attributes().First(a => a.Name.LocalName == "Title").Value;
        title.Should().Be("검색 및 사용 현황");
    }

    [Fact]
    public void ButtonLabels_AreKorean_NoEnglishRemains()
    {
        var window = LoadWindow();
        var texts = AttrValues(window, "Text");

        // 버튼 라벨(새로고침/조회/불러오기/삭제/RTF 내보내기/닫기)이 한글이어야 한다.
        texts.Should().Contain(new[] { "새로고침", "조회", "불러오기", "삭제", "RTF 내보내기", "닫기" });
        // 영어 버튼 라벨이 남으면 안 된다.
        texts.Should().NotContain(new[] { "Refresh", "Lookup", "Load", "Delete", "Close" });
    }
}
