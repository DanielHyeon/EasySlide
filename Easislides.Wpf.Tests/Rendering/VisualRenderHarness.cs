using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Easislides.Wpf.Tests.Rendering;

/// <summary>
/// 스크린샷 회귀용 헤드리스 렌더 하니스 — 계획서 §9.1 (작업 C).
///
/// WPF 비주얼을 창 없이 RenderTargetBitmap 으로 렌더한다(헤드리스 렌더는 이미
/// PreviewCanvasTests 에서 동작 확인됨). DPI/픽셀 포맷을 고정해 환경 간 결과를 안정화한다.
///
/// 주의: 텍스트는 폰트 힌팅/안티에일리어싱이 환경마다 달라 비교가 불안정하므로,
/// 시각 회귀 기준 비주얼은 가급적 텍스트 없는 결정적 요소(색·레이아웃·도형)로 구성한다.
///
/// STA 스레드에서 호출해야 한다(StaHelper).
/// </summary>
internal static class VisualRenderHarness
{
    /// <summary>고정 DPI — 환경 간 동일 픽셀 크기 보장.</summary>
    public const double Dpi = 96d;

    /// <summary>
    /// 지정 크기로 요소를 측정·배치한 뒤 Pbgra32 RenderTargetBitmap 으로 렌더한다.
    /// 콘텐츠가 지정 크기보다 크면 지정 크기로 클립된다(결정성 위해 가용 공간 고정).
    /// </summary>
    public static RenderTargetBitmap RenderToBitmap(FrameworkElement element, int width, int height)
    {
        if (element is null) throw new ArgumentNullException(nameof(element));
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        var size = new Size(width, height);
        element.Measure(size);
        element.Arrange(new Rect(size));
        element.UpdateLayout();

        var rtb = new RenderTargetBitmap(width, height, Dpi, Dpi, PixelFormats.Pbgra32);
        rtb.Render(element);
        return rtb;
    }

    /// <summary>요소를 렌더해 PNG 바이트로 인코딩한다.</summary>
    public static byte[] RenderToPng(FrameworkElement element, int width, int height)
    {
        var rtb = RenderToBitmap(element, width, height);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
