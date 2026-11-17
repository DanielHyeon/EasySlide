using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// FolderComboBox 호스트 배선 동등성 가드 — 계획서 §5.2 / 작업 A-4.
///
/// SongCopy/SongMove/SongMerge 의 인라인 폴더 ComboBox 가 재사용 FolderComboBox 로
/// 교체되었고, 각 호스트가 올바른 Label/AutomationName/ItemsSource/SelectedItem 으로
/// 배선되었는지(그리고 더 이상 인라인 ComboBox 로 같은 폴더를 선택하지 않는지) 검증한다.
///
/// ViewModel(SongCopy/Move/MergeViewModel)은 변경하지 않았으므로 동작 동등성은 기존
/// 각 ViewModel 테스트가 계속 보장한다(이 테스트는 View 배선 면만 고정).
/// 컨트롤 자체 동작(DP 전달·양방향)은 FolderComboBoxTests 가 런타임으로 검증한다.
/// </summary>
public class FolderComboBoxHostsTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    /// <summary>윈도우가 (Label, ItemsSource 경로, SelectedItem 경로)로 배선된 FolderComboBox 를 가지는지.</summary>
    private static bool HasFolderComboBox(XDocument window, string label, string itemsPath, string selectedPath)
        => window.Descendants()
            .Where(e => e.Name.LocalName == "FolderComboBox")
            .Any(e => Attr(e, "Label") == label
                   && Attr(e, "ItemsSource").Contains(itemsPath)
                   && Attr(e, "SelectedItem").Contains(selectedPath)
                   && Attr(e, "SelectedItem").Contains("TwoWay")
                   && !string.IsNullOrEmpty(Attr(e, "AutomationName")));

    /// <summary>윈도우에 지정 SelectedItem 경로로 바인딩된 인라인 ComboBox 가 남아있지 않은지.</summary>
    private static bool NoInlineComboBoxBoundTo(XDocument window, string selectedPath)
        => !window.Descendants()
            .Where(e => e.Name.LocalName == "ComboBox")
            .Any(e => Attr(e, "SelectedItem").Contains(selectedPath));

    [Fact]
    public void SongCopy_Uses_FolderComboBox()
    {
        var window = LoadXaml("Easislides.Wpf/Library/SongCopyWindow.xaml");

        HasFolderComboBox(window, "대상 폴더", "TargetFolders", "SelectedTargetFolder").Should().BeTrue();
        NoInlineComboBoxBoundTo(window, "SelectedTargetFolder").Should().BeTrue("인라인 폴더 콤보는 컴포지트로 이동");
    }

    [Fact]
    public void SongMove_Uses_FolderComboBox()
    {
        var window = LoadXaml("Easislides.Wpf/Library/SongMoveWindow.xaml");

        HasFolderComboBox(window, "대상 폴더", "TargetFolders", "SelectedTargetFolder").Should().BeTrue();
        NoInlineComboBoxBoundTo(window, "SelectedTargetFolder").Should().BeTrue("인라인 폴더 콤보는 컴포지트로 이동");
    }

    [Fact]
    public void SongMerge_Uses_Three_FolderComboBoxes()
    {
        var window = LoadXaml("Easislides.Wpf/Library/SongMergeWindow.xaml");

        HasFolderComboBox(window, "Source A", "Folders", "SelectedSourceFolderA").Should().BeTrue();
        HasFolderComboBox(window, "Source B", "Folders", "SelectedSourceFolderB").Should().BeTrue();
        HasFolderComboBox(window, "대상 폴더", "Folders", "SelectedTargetFolder").Should().BeTrue();

        window.Descendants().Count(e => e.Name.LocalName == "FolderComboBox").Should().Be(3);

        NoInlineComboBoxBoundTo(window, "SelectedSourceFolderA").Should().BeTrue();
        NoInlineComboBoxBoundTo(window, "SelectedSourceFolderB").Should().BeTrue();
        NoInlineComboBoxBoundTo(window, "SelectedTargetFolder").Should().BeTrue();
    }
}
