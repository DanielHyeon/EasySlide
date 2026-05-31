using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Controls;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Controls;

/// <summary>
/// 외곽선 텍스트 컨트롤(OutlinedTextBlock, §7.3-A 폰트 효과)의 측정·렌더 검증.
/// WPF 컴포넌트라 STA 스레드에서 실행(PreviewCanvasTests 와 동일 패턴).
/// </summary>
public class OutlinedTextBlockTests
{
    [Fact]
    public void Measure_WithText_ReturnsNonEmptySize()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new OutlinedTextBlock
            {
                Text = "은혜로다",
                FontSize = 48,
            };

            Measure(sut, 1160, 660);

            sut.DesiredSize.Width.Should().BeGreaterThan(0);
            sut.DesiredSize.Height.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void Measure_EmptyText_ReturnsZeroSize()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new OutlinedTextBlock { Text = string.Empty, FontSize = 48 };

            Measure(sut, 1160, 660);

            sut.DesiredSize.Should().Be(new Size(0, 0));
        });
    }

    [Fact]
    public void Measure_FontSizeZero_ReturnsZeroSize()
    {
        StaHelper.RunOnSta(() =>
        {
            // FontSize<=0 가드 — 0으로 나누거나 빈 형상을 만들지 않고 0 크기로 안전 처리.
            var sut = new OutlinedTextBlock { Text = "가사", FontSize = 0 };

            Measure(sut, 1160, 660);

            sut.DesiredSize.Should().Be(new Size(0, 0));
        });
    }

    [Fact]
    public void Measure_ClampsToAvailableWidth()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new OutlinedTextBlock
            {
                Text = "아주 긴 가사 줄 아주 긴 가사 줄 아주 긴 가사 줄",
                FontSize = 64,
                MaxTextWidth = 200,
            };

            Measure(sut, 200, 660);

            sut.DesiredSize.Width.Should().BeLessThanOrEqualTo(200, "가용 폭으로 클램프");
        });
    }

    [Fact]
    public void Render_WithText_ProducesPixels()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new OutlinedTextBlock
            {
                Text = "은혜로다",
                FontSize = 64,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                // 비트맵 폭(400)에 맞춰 래핑 폭을 제한 — 기본 1160 + 가운데 정렬이면 글자가 400px 밖에 그려진다.
                MaxTextWidth = 400,
            };
            Measure(sut, 400, 200);
            sut.Arrange(new Rect(new Size(400, 200)));
            sut.UpdateLayout();

            var bitmap = new RenderTargetBitmap(400, 200, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(sut);
            var pixels = new byte[400 * 200 * 4];
            bitmap.CopyPixels(pixels, stride: 400 * 4, offset: 0);

            pixels.Should().Contain(value => value != 0, "외곽선 글자가 실제로 그려져야 한다");
        });
    }

    [Fact]
    public void Render_EmptyText_DoesNotThrow()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new OutlinedTextBlock { Text = string.Empty, FontSize = 48 };
            Measure(sut, 400, 200);
            sut.Arrange(new Rect(new Size(400, 200)));

            // 빈 텍스트 렌더가 예외 없이 안전히 넘어가야 한다.
            var bitmap = new RenderTargetBitmap(400, 200, 96, 96, PixelFormats.Pbgra32);
            var act = () => bitmap.Render(sut);

            act.Should().NotThrow();
        });
    }

    private static void Measure(FrameworkElement element, int width, int height)
        => element.Measure(new Size(width, height));
}
