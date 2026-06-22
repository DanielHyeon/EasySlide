using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

/// <summary>
/// 스크린샷 회귀 자동화 PoC + 기반 — 계획서 §9.1 (작업 C).
///
/// 헤드리스 렌더(RenderTargetBitmap)는 이미 PreviewCanvasTests 에서 동작 확인됨.
/// 여기서는 (1) PNG 인코딩 가능 여부, (2) 허용오차 비교기가 실제 시각 변화를 잡는지,
/// (3) EasiDS 색 토큰 스와치의 시각 회귀 가드를 검증한다.
///
/// 기준 비주얼은 "텍스트 없는 색 토큰 스와치"다 — 폰트 렌더 차이에 흔들리지 않는
/// 결정적 비주얼이라 환경 간 안정적이며, 토큰 색이 바뀌면 즉시 회귀로 잡힌다.
///
/// EasiDS 리소스가 필요하므로 "WPF Application" 픽스처 사용, STA 스레드에서 렌더.
/// </summary>
[Collection("WPF Application")]
public class ScreenshotRegressionTests
{
    // 결정적 색 스와치를 구성할 EasiDS 토큰 키(순서 고정 — 기준 이미지와 1:1 대응).
    private static readonly string[] SwatchTokens =
    {
        "Brush.Accent.Primary",
        "Brush.Surface.Page",
        "Brush.Surface.Card",
        "Brush.Text.Primary",
        "Brush.Status.Success",
        "Brush.Status.Warning",
        "Brush.Status.Danger",
        "Brush.Status.Info",
    };

    private const int SwatchWidth = 320;
    private const int SwatchHeight = 40;

    [Fact]
    public void RenderHarness_Produces_Valid_Png()
    {
        StaHelper.RunOnSta(() =>
        {
            var png = VisualRenderHarness.RenderToPng(BuildTokenSwatch(SwatchTokens), SwatchWidth, SwatchHeight);

            png.Length.Should().BeGreaterThan(0);
            // PNG 시그니처: 89 50 4E 47 0D 0A 1A 0A
            png.Take(8).Should().Equal(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);
        });
    }

    [Fact]
    public void Comparer_Detects_Token_Color_Change()
    {
        StaHelper.RunOnSta(() =>
        {
            // 같은 스와치 두 번 렌더 → 동일(결정적).
            var baseline = VisualRenderHarness.RenderToPng(BuildTokenSwatch(SwatchTokens), SwatchWidth, SwatchHeight);
            var same = VisualRenderHarness.RenderToPng(BuildTokenSwatch(SwatchTokens), SwatchWidth, SwatchHeight);
            ImageComparer.Compare(baseline, same).IsMatch.Should().BeTrue("동일 토큰 스와치는 결정적으로 같아야 함");

            // 첫 칸 색을 강제로 바꾼 스와치 → 회귀로 감지되어야 함.
            var changed = VisualRenderHarness.RenderToPng(BuildSwatchWithOverride(0, Colors.Magenta), SwatchWidth, SwatchHeight);
            var result = ImageComparer.Compare(baseline, changed);
            result.IsMatch.Should().BeFalse("토큰 색이 바뀌면 시각 회귀로 감지되어야 함");
            result.DifferingPixels.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void ColorTokens_Light_Match_Baseline()
    {
        StaHelper.RunOnSta(() =>
        {
            // 같은 "WPF Application" 컬렉션의 다른 테스트가 테마/사이즈를 바꿔둘 수 있으므로,
            // 기준(Light·Standard)과 비교하기 전에 명시적으로 고정한다(앰비언트 상태 의존 제거).
            var theme = new ThemeService();
            theme.ApplyTheme(ColorTheme.Light);
            theme.ApplyInterfaceSize(InterfaceSize.Standard);

            var png = VisualRenderHarness.RenderToPng(BuildTokenSwatch(SwatchTokens), SwatchWidth, SwatchHeight);
            ScreenshotBaseline.AssertMatches("color-tokens-light", png);
        });
    }

    [Fact]
    public void ColorTokens_Dark_Match_Baseline()
    {
        StaHelper.RunOnSta(() =>
        {
            // 라이트/다크 양쪽 시각 회귀(계획서 §9.1) — 다크는 별도 기준 이미지로 고정.
            // 앰비언트 상태(같은 컬렉션 다른 테스트가 바꿔둔 테마)에 의존하지 않도록 명시 고정.
            var theme = new ThemeService();
            theme.ApplyTheme(ColorTheme.Dark);
            theme.ApplyInterfaceSize(InterfaceSize.Standard);
            try
            {
                var png = VisualRenderHarness.RenderToPng(BuildTokenSwatch(SwatchTokens), SwatchWidth, SwatchHeight);
                ScreenshotBaseline.AssertMatches("color-tokens-dark", png);
            }
            finally
            {
                // 예외 경로에서도 공유 Application 리소스를 라이트로 복원(후속 테스트 보호).
                // (각 스크린샷 테스트는 시작 시 자기 테마를 고정하므로 주 방어선은 아니나, 의도-구현 일치.)
                theme.ApplyTheme(ColorTheme.Light);
            }
        });
    }

    /// <summary>토큰 브러시로 채운 가로 균등 스와치(텍스트 없음 → 결정적).</summary>
    private static FrameworkElement BuildTokenSwatch(string[] tokenKeys)
    {
        var grid = new UniformGrid
        {
            Rows = 1,
            Columns = tokenKeys.Length,
            Background = Brushes.White,
        };

        foreach (var key in tokenKeys)
        {
            grid.Children.Add(new Border { Background = ResolveBrush(key) });
        }

        return grid;
    }

    /// <summary>한 칸의 색을 지정 색으로 덮어쓴 스와치(회귀 감지 검증용).</summary>
    private static FrameworkElement BuildSwatchWithOverride(int index, Color overrideColor)
    {
        var grid = new UniformGrid
        {
            Rows = 1,
            Columns = SwatchTokens.Length,
            Background = Brushes.White,
        };

        for (var i = 0; i < SwatchTokens.Length; i++)
        {
            var brush = i == index ? new SolidColorBrush(overrideColor) : ResolveBrush(SwatchTokens[i]);
            grid.Children.Add(new Border { Background = brush });
        }

        return grid;
    }

    private static Brush ResolveBrush(string key)
    {
        Application.Current.Should().NotBeNull("WPF Application 픽스처가 EasiDS 리소스를 로드해야 함");
        return Application.Current!.TryFindResource(key) as Brush
               ?? throw new InvalidOperationException($"EasiDS 토큰 브러시를 찾을 수 없음: {key}");
    }
}
