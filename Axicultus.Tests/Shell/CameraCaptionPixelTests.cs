using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Tests.Rendering;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// The output screen's camera caption, drawn off screen in fixed sizes and compared pixel for pixel with the pictures
/// kept before it became a shared control (a037 design 12: on a one-screen PC the harness cannot show it).
/// Baselines: Axicultus.Tests/Rendering/baselines/camera-caption-*.png (made with AXICULTUS_APPROVE_BASELINES=1).
/// </summary>
[Collection("WPF Application")]
public sealed class CameraCaptionPixelTests
{
    private const string Text = "주님은 나의 목자시니 내게 부족함 없으리\n푸른 풀밭에 누이시며 쉴 만한 물가로 인도하시네";

    public static IEnumerable<object[]> Cases()
    {
        foreach (var (width, height) in new[] { (1280, 720), (960, 540) })
        {
            foreach (int lines in new[] { 1, 2, 3 })
                foreach (bool box in new[] { true, false })
                    yield return [width, height, "text", lines, 1, box];
            foreach (int score in new[] { 0, 1, 2 })
                yield return [width, height, "score", 3, score, true];
            foreach (int score in new[] { 0, 1, 2 })
                foreach (int lines in new[] { 1, 2, 3 })
                    foreach (bool box in new[] { true, false })
                        yield return [width, height, "scoretext", lines, score, box];
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void TheCameraCaptionDrawsAsBefore(int width, int height, string kind, int lines, int score, bool box) => StaHelper.RunOnSta(() =>
    {
        // Other tests of the shared Application change the theme and the interface size (the output's status band).
        var theme = new Axicultus.Wpf.Theme.ThemeService();
        theme.ApplyTheme(Axicultus.Wpf.Theme.ColorTheme.Light);
        theme.ApplyInterfaceSize(Axicultus.Wpf.Theme.InterfaceSize.Standard);
        var png = Draw(width, height, kind, lines, score, box);
        ScreenshotBaseline.AssertMatches($"camera-caption-{width}x{height}-{kind}-l{lines}-s{score}-{(box ? "box" : "nobox")}", png,
            channelTolerance: 0);
    });

    // A score row picture: staff lines and notes on white, the same every time.
    internal static BitmapSource ScoreRow()
    {
        const int w = 1200, h = 260;
        var pixels = new byte[w * h];
        System.Array.Fill(pixels, (byte)255);
        void Fill(int x, int y, int fw, int fh) { for (int r = y; r < y + fh; r++) System.Array.Fill(pixels, (byte)0, r * w + x, fw); }
        for (int line = 0; line < 5; line++) Fill(40, 60 + line * 18, 1120, 3);
        for (int note = 0; note < 12; note++) Fill(90 + note * 88, 50 + note % 5 * 14, 22, 18);
        Fill(120, 190, 900, 30); // lyrics under the staff
        var image = BitmapSource.Create(w, h, 96, 96, PixelFormats.Gray8, null, pixels, w);
        image.Freeze();
        return image;
    }

    internal static byte[] Draw(int width, int height, string kind, int lines, int score, bool box)
    {
        using var session = new CaptionSession(new LiveSessionService().ForCaptions());
        session.SetStyle(session.Style with { Lines = lines, Score = score, Box = box });
        session.Publish(Text);
        if (kind != "text") session.SetImage(ScoreRow(), withText: kind == "scoretext");
        using var scene = new OutputWindowViewModel(); // a quiet output screen: nothing but the caption shows
        var window = new OutputWindow();
        try
        {
            ((FrameworkElement)window.FindName("CameraCaption")).DataContext = session;
            var root = (Grid)window.Content;
            window.Content = null; // drawn on its own, never shown
            root.DataContext = scene;
            VisualRenderHarness.RenderToBitmap(root, width, height); // first layout: the caption is sized to the screen
            root.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            return VisualRenderHarness.RenderToPng(root, width, height);
        }
        finally { window.Close(); }
    }
}
