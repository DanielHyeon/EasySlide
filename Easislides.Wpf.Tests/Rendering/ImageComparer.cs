using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Easislides.Wpf.Tests.Rendering;

/// <summary>
/// 스크린샷 회귀용 이미지 비교 — 계획서 §9.1 (작업 C).
///
/// 두 PNG 를 Pbgra32 로 디코드해 픽셀 단위로 비교한다. 정확 일치가 아니라
/// "채널 허용오차 + 차이 픽셀 비율 허용치"로 비교해, 결정적 비주얼의 미세한 인코딩
/// 잡음은 흡수하되 실제 시각 회귀(색·레이아웃 변경)는 잡아낸다.
/// </summary>
internal static class ImageComparer
{
    public readonly record struct Result(
        bool IsMatch,
        bool DimensionsMatch,
        int ExpectedWidth,
        int ExpectedHeight,
        int ActualWidth,
        int ActualHeight,
        long DifferingPixels,
        long TotalPixels,
        int MaxChannelDelta)
    {
        public double DifferingPercent =>
            TotalPixels <= 0 || DifferingPixels < 0 ? 0d : 100d * DifferingPixels / TotalPixels;
    }

    /// <summary>
    /// PNG 바이트 두 개를 비교한다.
    /// </summary>
    /// <param name="channelTolerance">채널(B/G/R/A)별 허용 절대 차이. 이보다 크면 "다른 픽셀"로 카운트.</param>
    /// <param name="maxDifferingPercent">허용하는 "다른 픽셀" 비율(%) 상한.</param>
    public static Result Compare(
        byte[] expectedPng,
        byte[] actualPng,
        int channelTolerance = 2,
        double maxDifferingPercent = 0.0)
    {
        var (ew, eh, ePixels) = DecodeToBgra(expectedPng);
        var (aw, ah, aPixels) = DecodeToBgra(actualPng);

        if (ew != aw || eh != ah)
        {
            return new Result(false, DimensionsMatch: false,
                ExpectedWidth: ew, ExpectedHeight: eh, ActualWidth: aw, ActualHeight: ah,
                DifferingPixels: -1, TotalPixels: (long)aw * ah, MaxChannelDelta: -1);
        }

        // long 승격 — 향후 전체 화면 캡처(고해상도) 비교로 재사용해도 오버플로 없도록.
        var total = (long)ew * eh;
        var differing = 0L;
        var maxDelta = 0;

        for (var i = 0; i < ePixels.Length; i += 4)
        {
            var dB = Math.Abs(ePixels[i] - aPixels[i]);
            var dG = Math.Abs(ePixels[i + 1] - aPixels[i + 1]);
            var dR = Math.Abs(ePixels[i + 2] - aPixels[i + 2]);
            var dA = Math.Abs(ePixels[i + 3] - aPixels[i + 3]);
            var pixelMax = Math.Max(Math.Max(dB, dG), Math.Max(dR, dA));

            if (pixelMax > maxDelta) maxDelta = pixelMax;
            if (pixelMax > channelTolerance) differing++;
        }

        var percent = total == 0 ? 0d : 100d * differing / total;
        var isMatch = percent <= maxDifferingPercent;

        return new Result(isMatch, DimensionsMatch: true,
            ExpectedWidth: ew, ExpectedHeight: eh, ActualWidth: aw, ActualHeight: ah,
            differing, total, maxDelta);
    }

    private static (int Width, int Height, byte[] Bgra) DecodeToBgra(byte[] png)
    {
        using var stream = new MemoryStream(png);
        // IgnoreColorProfile — WIC 색 관리 개입 배제로 환경 간 결정성 강화.
        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
            BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];

        // Pbgra32 로 통일해 채널 정렬 보장.
        var converted = new FormatConvertedBitmap(frame, PixelFormats.Pbgra32, destinationPalette: null, alphaThreshold: 0);
        var w = converted.PixelWidth;
        var h = converted.PixelHeight;
        var stride = w * 4;
        var pixels = new byte[stride * h];
        converted.CopyPixels(pixels, stride, offset: 0);
        return (w, h, pixels);
    }
}
