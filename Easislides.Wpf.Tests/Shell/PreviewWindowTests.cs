using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 증분160-E — 스테이지(Preview) 모니터 창이 같은 OutputWindowViewModel 의 가사(Region1/2)·다음 항목을 바인딩하고
/// 시계 자리(ClockText)를 갖췄는지 정적 XAML 구조로 검증한다. (시계 갱신·창 생명주기는 코드비하인드 — 런타임.)
/// </summary>
public sealed class PreviewWindowTests
{
    private static XDocument LoadXaml()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, "Easislides.Wpf/PreviewWindow.xaml"), LoadOptions.None);
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    [Fact]
    public void PreviewWindow_BindsLyricsBothRegions_NextItem_AndHasClock()
    {
        var window = LoadXaml();
        var textBlocks = window.Descendants().Where(e => e.Name.LocalName == "TextBlock").ToArray();

        // 가사 Region1·Region2 본문 바인딩(이중 언어 곡도 스테이지에 보이도록).
        var region1 = textBlocks.Single(
            t => Attr(t, "Text").Contains("BodyText") && !Attr(t, "Text").Contains("BodyText2"));
        var region2 = textBlocks.Single(t => Attr(t, "Text").Contains("BodyText2"));

        // 본문 Visibility 는 효과 게이트(BodyTextVisibility — 외곽선/인터레이스 시 Collapsed)가 아니라 **본문 문자열 유무**로
        // 걸려야 한다 — 그래야 효과 켠 곡에서도 스테이지에 평문 가사가 계속 보인다(회귀 방지).
        Attr(region1, "Visibility").Should().Contain("BodyText").And.NotContain("BodyTextVisibility",
            "Region1 표시는 효과 게이트가 아닌 본문 문자열 유무로 걸어야 함");
        Attr(region2, "Visibility").Should().Contain("BodyText2").And.NotContain("BodyText2Visibility");
        // 다음 항목 바인딩(리더가 다음 순서를 미리 봄).
        textBlocks.Should().Contain(t => Attr(t, "Text").Contains("NextItemText"), "다음 항목 바인딩");
        // 시계 자리 — 코드비하인드 타이머가 채우는 ClockText.
        textBlocks.Should().Contain(t => Attr(t, "Name") == "ClockText", "시계 TextBlock(ClockText) 존재");
        // 배경은 출력과 같은 SceneBackgroundBrush(회중 화면과 같은 톤).
        Attr(window.Root!, "Background").Should().Contain("SceneBackgroundBrush");
    }
}
