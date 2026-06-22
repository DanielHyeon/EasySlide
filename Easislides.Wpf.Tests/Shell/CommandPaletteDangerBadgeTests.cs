using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 명령 팔레트(⌘K) 결과에서 위험 명령(IsDangerous = 라이브 중지·검은 화면·출력 닫기·화면 비우기·출력 숨김·Go Live)을
/// 시각적으로 구별하는지 정적 XAML 구조로 검증(§7.4 후속). 한 사람이 라이브를 운영할 때 실수로 송출을 끊는
/// 사고를 줄이려, 위험 명령은 결과 목록에서 빨간 "위험" 배지로 눈에 띄게 한다.
/// (IsDangerous 플래그 자체의 데이터는 CommandCatalogTests 가 보장 — 여기선 "화면에 표시되는가"만 본다.)
/// </summary>
public class CommandPaletteDangerBadgeTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    [Fact]
    public void PaletteResultTemplate_VisuallyMarks_DangerousCommands()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        // 명령 팔레트 결과 목록(ListBox, ItemsSource=CommandPalette.Results)의 항목 템플릿을 찾는다.
        var resultsList = window.Descendants().Single(
            e => e.Name.LocalName == "ListBox"
                && Attr(e, "ItemsSource").Contains("CommandPalette.Results"));
        var template = resultsList.Descendants().Single(e => e.Name.LocalName == "DataTemplate");

        // (1) 위험 배지는 IsDangerous 일 때만 보여야 한다 — Visibility 가 IsDangerous 에 바인딩돼야 함.
        //     (단순히 IsDangerous 가 어딘가 등장하는 것보다, "위험할 때만 표시" 계약을 못 박아 항상 표시 실수를 막는다.)
        var togglesByDanger = template.Descendants()
            .Any(e => e.Attributes().Any(a => a.Name.LocalName == "Visibility" && a.Value.Contains("IsDangerous")));
        togglesByDanger.Should().BeTrue("위험 배지는 IsDangerous 일 때만 보이도록 Visibility 가 바인딩돼야 함");

        // (2) 단순 텍스트가 아니라 위험 색(Brush.Status.Danger)으로 강조해야 한다.
        var usesDangerBrush = template.Descendants()
            .Any(e => e.Attributes().Any(a => a.Value.Contains("Brush.Status.Danger")));
        usesDangerBrush.Should().BeTrue("위험 배지는 위험 색(Brush.Status.Danger)으로 강조");
    }
}
