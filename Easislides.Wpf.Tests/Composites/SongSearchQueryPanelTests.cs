using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// SongSearchQueryPanel 컴포지트 추출 동등성 가드 — 계획서 §5.2 / 작업 A-3.
///
/// SearchUsageWindow "Songs" 탭의 검색 쿼리 폼 카드를 재사용 UserControl로 분리했다.
/// "View만 분리, ViewModel·바인딩 동일" 원칙.
///
///  1) 분리된 컴포지트가 원래 인라인 바인딩 면(검색어·필드 토글·조/타이밍·플래그·
///     수정일·폴더·검색 명령)을 그대로 보유한다.
///  2) SearchUsageWindow 는 더 이상 그 바인딩을 직접 인라인하지 않고 컴포지트를 호스트한다.
///
/// ViewModel(SearchUsageViewModel)은 변경하지 않았으므로 동작 동등성은 기존
/// SearchUsageViewModelTests 가 계속 보장한다(이 테스트는 View 이동 면만 고정).
/// (컨트롤 스타일 사전 미로드로 런타임 인스턴스화가 불가하므로 XAML 구조로 검증.)
/// </summary>
public class SongSearchQueryPanelTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static XElement ElementByAutomationName(XDocument doc, string name)
        => doc.Descendants().Single(e => e.Attributes()
            .Any(a => a.Name.LocalName == "AutomationProperties.Name" && a.Value == name));

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    private static bool HasAttributeValueContaining(XDocument doc, string localName, string needle)
        => doc.Descendants().Any(e => e.Attributes()
            .Any(a => a.Name.LocalName == localName && a.Value.Contains(needle)));

    [Fact]
    public void Composite_Retains_Key_Controls()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/SongSearchQueryPanel.xaml");

        // 검색어 TextBox — 경로 + 즉시 반영 트리거 보존.
        var query = ElementByAutomationName(composite, "검색어");
        query.Name.LocalName.Should().Be("TextBox");
        Attr(query, "Text").Should().Contain("SearchText").And.Contain("UpdateSourceTrigger=PropertyChanged");

        // 조/타이밍 검색 TextBox
        Attr(ElementByAutomationName(composite, "조(Key) 검색"), "Text")
            .Should().Contain("SearchKey").And.Contain("UpdateSourceTrigger=PropertyChanged");
        Attr(ElementByAutomationName(composite, "타이밍 검색"), "Text")
            .Should().Contain("SearchTiming").And.Contain("UpdateSourceTrigger=PropertyChanged");

        // 검색 실행 명령 버튼
        composite.Descendants().Any(e => e.Name.LocalName == "Button"
                && Attr(e, "Command").Contains("SearchSongsCommand"))
            .Should().BeTrue("검색 실행 명령 보존");

        // 폴더 목록
        HasAttributeValueContaining(composite, "ItemsSource", "SearchFolders").Should().BeTrue("폴더 목록 바인딩 보존");
    }

    [Fact]
    public void Composite_Retains_All_Search_Field_Bindings()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/SongSearchQueryPanel.xaml");

        // 검색 필드 토글 8종 + 플래그 3종 + 수정일 2종 — 하나라도 누락되면 검색 동작이 달라짐.
        var isCheckedBindings = new[]
        {
            "SearchTitle", "SearchLyrics", "SearchSongNumber", "SearchBookReference",
            "SearchUserReference", "SearchLicenceAdmin", "SearchWriter", "SearchCopyright",
            "NotationsOnly", "MediaOnly", "UseModifiedDates",
        };
        foreach (var path in isCheckedBindings)
        {
            HasAttributeValueContaining(composite, "IsChecked", path).Should().BeTrue($"{path} 토글 보존");
        }

        HasAttributeValueContaining(composite, "SelectedDate", "ModifiedFrom").Should().BeTrue("수정일 From 보존");
        HasAttributeValueContaining(composite, "SelectedDate", "ModifiedTo").Should().BeTrue("수정일 To 보존");

        // 폴더 항목 체크박스의 선택 토글(IsSelected) — 검색 범위를 결정하는 핵심 바인딩.
        // ItemTemplate 내부 바인딩이라 ItemsSource 존재만으로는 가드되지 않으므로 명시 검사.
        HasAttributeValueContaining(composite, "IsChecked", "IsSelected").Should().BeTrue("폴더 선택 토글 보존");
    }

    [Fact]
    public void Composite_FieldLabels_AreKorean_UnifiedWithEditor()
    {
        // §7.4 후속: 검색 필드 라벨을 곡 편집기(SongEditorWindow)와 동일 한글 용어로 통일.
        // 이전엔 "Book"/"Title" 등 영어였고 편집기는 "교단 참조"/"제목"이라 같은 필드가 달리 보였다.
        var composite = LoadXaml("Easislides.Wpf/Composites/SongSearchQueryPanel.xaml");

        var checkboxContents = composite.Descendants()
            .Where(e => e.Name.LocalName == "CheckBox")
            .Select(e => Attr(e, "Content"))
            .Where(content => content.Length > 0)
            .ToArray();

        // BookReference/UserReference 필드는 편집기와 동일 용어로 통일.
        checkboxContents.Should().Contain("교단 참조").And.Contain("사용자 참조");
        // 영어 필드 라벨이 남지 않아야 한다(용어 통일 회귀 방지).
        checkboxContents.Should().NotContain(new[] { "Book", "User", "Title", "Lyrics", "Writer", "Copyright", "Admin" });
    }

    [Fact]
    public void Window_Hosts_Composite_And_No_Longer_Inlines_Query_Form()
    {
        var window = LoadXaml("Easislides.Wpf/Library/SearchUsageWindow.xaml");

        var host = window.Descendants().SingleOrDefault(e => e.Name.LocalName == "SongSearchQueryPanel");
        host.Should().NotBeNull("SearchUsageWindow 는 SongSearchQueryPanel 컴포지트를 호스트해야 함");

        // 원본 Border는 Grid.Column 미지정(=column 0)이었다. 컴포지트도 동일해야 카드 위치가 보존된다
        // (column 1은 spacer, column 2는 결과 DataGrid). 누군가 column을 옮기는 회귀를 차단.
        host!.Attributes().Any(a => a.Name.LocalName == "Grid.Column")
            .Should().BeFalse("컴포지트는 Grid.Column 미지정(=column 0) 원래 위치를 유지해야 함");

        // Songs 패널 고유 바인딩은 윈도우에 더 이상 인라인되지 않는다.
        // (SearchText 는 Titles 탭에도 쓰이므로 부재 단언 대상이 아님 — 고유 토큰만 검사.)
        HasAttributeValueContaining(window, "Command", "SearchSongsCommand").Should().BeFalse("검색 실행 명령은 컴포지트로 이동");
        HasAttributeValueContaining(window, "ItemsSource", "SearchFolders").Should().BeFalse("폴더 목록은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Text", "SearchKey").Should().BeFalse("조 검색은 컴포지트로 이동");
        HasAttributeValueContaining(window, "IsChecked", "SearchTitle").Should().BeFalse("필드 토글은 컴포지트로 이동");
    }
}
