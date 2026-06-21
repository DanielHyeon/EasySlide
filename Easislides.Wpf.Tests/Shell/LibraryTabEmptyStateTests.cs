using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 증분157 — 좌측 "라이브러리" 탭의 곡 목록이 비었을 때(불러왔는데 0건) "곡 없음" 빈 상태 안내가
/// 곡 목록과 같은 셀에 겹쳐 뜨도록 XAML에 배선됐는지 정적 구조로 검증한다(검색 빈 상태 154/155 와 일관).
/// (빈 상태를 언제 띄울지 판단하는 로직은 LibraryViewModelTests 가 보장 — 여기선 "오버레이가 화면에 배선됐는가"만 본다.)
/// </summary>
public sealed class LibraryTabEmptyStateTests
{
    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    private static XElement LibraryTab()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var window = XDocument.Load(Path.Combine(repoRoot, "Easislides.Wpf/MainWindow.xaml"), LoadOptions.None);
        return window.Descendants().Single(
            e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "Folders");
    }

    [Fact]
    public void LibraryTab_HasEmptyStateOverlay_BoundToHasLibraryEmptyState()
    {
        var library = LibraryTab();

        // 빈 상태 TextBlock 이 Library.HasLibraryEmptyState 가시성에 바인딩되어 있어야 한다.
        var emptyState = library.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TextBlock" && Attr(e, "Visibility").Contains("HasLibraryEmptyState"));
        emptyState.Should().NotBeNull("라이브러리 빈 상태 안내가 HasLibraryEmptyState 에 바인딩돼야 함");

        // 문구는 고정 텍스트가 아니라 상황(빈 폴더 / 검색 0건)에 따라 VM이 정해 주는 동적 메시지여야 한다.
        Attr(emptyState!, "Text").Should().Contain("LibraryEmptyStateMessage", "안내 문구는 VM의 동적 메시지에 바인딩");
        // 빈 상태는 클릭을 막지 않아야 한다(아래 곡 목록·더블클릭 추가가 가려지지 않도록).
        Attr(emptyState!, "IsHitTestVisible").Should().Be("False", "안내는 클릭을 통과시켜야 함");

        // 안내와 곡 목록이 같은 셀에 겹쳐야 "오버레이"가 성립한다.
        var songList = library.Descendants().Single(
            e => e.Name.LocalName == "ListView" && Attr(e, "Name") == "LibrarySongList");
        Attr(songList, "Tag").Should().Be("SongsList", "WPF Folders list should keep the FrmMain SongsList role visible");
        Attr(emptyState!, "Grid.Row").Should().Be("2");
        Attr(songList, "Grid.Row").Should().Be("2", "안내와 곡 목록이 같은 셀에 겹쳐야 오버레이가 됨");
    }
}
