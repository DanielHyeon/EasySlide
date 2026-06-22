using System.Windows;
using System.Windows.Controls;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Theme;

/// <summary>
/// ThemeService 단위 테스트 — ADR-0006 시니어 모드 스케일 + Q3 다크/라이트 전환 검증.
///
/// [Collection("WPF Application")]만 사용 — ICollectionFixture가 WpfApplicationFixture 인스턴스를 공유해줌.
/// IClassFixture 중복 선언하면 fixture가 두 번 생성됨 (구버전 실수, 리뷰 #1 수정).
/// </summary>
[Collection("WPF Application")]
public class ThemeServiceTests
{

    [Fact]
    public void NewService_DefaultsTo_StandardLight()
    {
        var sut = new ThemeService();

        sut.CurrentTheme.Should().Be(ColorTheme.Light);
        sut.CurrentSize.Should().Be(InterfaceSize.Standard);
        sut.ScaleFactor.Should().Be(1.0);
    }

    [Theory]
    [InlineData(InterfaceSize.Standard, 1.0)]
    [InlineData(InterfaceSize.Large, 1.2)]
    [InlineData(InterfaceSize.Senior, 1.4)]
    public void ScaleFactor_MapsCorrectly(InterfaceSize size, double expectedFactor)
    {
        var sut = new ThemeService();
        sut.ApplyInterfaceSize(size);

        sut.ScaleFactor.Should().Be(expectedFactor);
        sut.CurrentSize.Should().Be(size);
    }

    [Fact]
    public void ApplyInterfaceSize_RaisesSizeChangedEvent()
    {
        var sut = new ThemeService();
        var raised = false;
        sut.SizeChanged += (_, _) => raised = true;

        sut.ApplyInterfaceSize(InterfaceSize.Senior);

        raised.Should().BeTrue();
    }

    [Fact]
    public void TypographyResources_ExposeCaptionStrongStyleUsedByMainWindow()
    {
        var style = Application.Current.Resources["Type.Caption.StrongStyle"];

        style.Should().BeOfType<Style>();
        ((Style)style).TargetType.Should().Be(typeof(TextBlock));
    }

    [Fact]
    public void ApplyInterfaceSize_UpdatesFontSizeResources()
    {
        var sut = new ThemeService();
        var bodyKey = "Type.Body.FontSize";

        sut.ApplyInterfaceSize(InterfaceSize.Standard);
        var standardSize = Application.Current.Resources[bodyKey];

        sut.ApplyInterfaceSize(InterfaceSize.Senior);
        var seniorSize = Application.Current.Resources[bodyKey];

        standardSize.Should().Be(14.0); // Typography.xaml 기본값
        seniorSize.Should().Be(14.0 * 1.4); // ScaleFactor 1.4 적용
    }

    [Fact]
    public void ApplyInterfaceSize_UpdatesSpacingResources()
    {
        var sut = new ThemeService();
        var targetKey = "Spacing.TargetMin";

        sut.ApplyInterfaceSize(InterfaceSize.Standard);
        var standardTarget = Application.Current.Resources[targetKey];

        sut.ApplyInterfaceSize(InterfaceSize.Large);
        var largeTarget = Application.Current.Resources[targetKey];

        standardTarget.Should().Be(44.0);
        largeTarget.Should().Be(44.0 * 1.2);
    }

    [Theory]
    [InlineData(ColorTheme.Light)]
    [InlineData(ColorTheme.Dark)]
    public void ApplyTheme_UpdatesCurrentTheme(ColorTheme theme)
    {
        var sut = new ThemeService();
        sut.ApplyTheme(theme);
        sut.CurrentTheme.Should().Be(theme);
    }

    [Fact]
    public void ApplyTheme_RaisesThemeChangedEvent()
    {
        var sut = new ThemeService();
        var raised = false;
        sut.ThemeChanged += (_, _) => raised = true;

        sut.ApplyTheme(ColorTheme.Dark);

        raised.Should().BeTrue();
    }

    [Fact]
    public void ApplyTheme_Dark_UpdatesSurfaceToBlackish()
    {
        var sut = new ThemeService();
        sut.ApplyTheme(ColorTheme.Dark);

        // Colors.Dark.xaml의 Surface.Page는 #0A0A0A
        var pageBrush = Application.Current.Resources["Brush.Surface.Page"] as System.Windows.Media.SolidColorBrush;
        pageBrush.Should().NotBeNull();
        pageBrush!.Color.R.Should().BeLessThan(0x30, "다크 모드 페이지 배경은 어두워야 함");
    }

    [Fact]
    public void ApplyTheme_Light_UpdatesSurfaceToWhitish()
    {
        var sut = new ThemeService();
        sut.ApplyTheme(ColorTheme.Light);

        // Colors.Light.xaml의 Surface.Page는 #FAFAFA
        var pageBrush = Application.Current.Resources["Brush.Surface.Page"] as System.Windows.Media.SolidColorBrush;
        pageBrush.Should().NotBeNull();
        pageBrush!.Color.R.Should().BeGreaterThan(0xE0, "라이트 모드 페이지 배경은 밝아야 함");
    }

    [Fact]
    public void ApplyTheme_LightAfterDark_RevertsColorsCorrectly()
    {
        var sut = new ThemeService();

        sut.ApplyTheme(ColorTheme.Dark);
        var darkText = (Application.Current.Resources["Brush.Text.Primary"] as System.Windows.Media.SolidColorBrush)!.Color.R;

        sut.ApplyTheme(ColorTheme.Light);
        var lightText = (Application.Current.Resources["Brush.Text.Primary"] as System.Windows.Media.SolidColorBrush)!.Color.R;

        // 다크 모드 본문은 밝은색(F5), 라이트 모드 본문은 어두운색(0F)
        darkText.Should().BeGreaterThan(0xE0);
        lightText.Should().BeLessThan(0x30);
    }
}
