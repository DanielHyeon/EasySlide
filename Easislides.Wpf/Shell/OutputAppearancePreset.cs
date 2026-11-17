using System.Windows.Media;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 출력 화면 모양 프리셋 — 가사/타이틀 글자색 + 배경(단색/세로 그라데이션)의 완성된 한 벌.
/// 운영자가 MainWindow 인-셸 인스펙터에서 한 번에 적용한다(별도 Settings 모달 없이 라이브 반영).
/// 색은 ARGB 정수(설정 키와 동일 형식). 미리보기 스와치 브러시는 바인딩용으로 계산해 노출한다.
/// </summary>
public sealed record OutputAppearancePreset(
    string Name,
    int TextArgb,
    int Background1Argb,
    int Background2Argb,
    bool IsGradient)
{
    private Brush? _textBrush;
    private Brush? _backgroundBrush;

    /// <summary>글자색 미리보기 브러시(스와치). frozen 이라 한 번만 만들어 캐시.</summary>
    public Brush TextBrush => _textBrush ??= FrozenSolid(TextArgb);

    /// <summary>배경 미리보기 브러시 — 그라데이션이면 세로 그라데이션, 아니면 단색. 한 번만 만들어 캐시.</summary>
    public Brush BackgroundBrush => _backgroundBrush ??= IsGradient && Background1Argb != Background2Argb
        ? FrozenGradient(Background1Argb, Background2Argb)
        : FrozenSolid(Background1Argb);

    private static Color ColorFromArgb(int argb)
    {
        var value = unchecked((uint)argb);
        return Color.FromArgb(
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value);
    }

    private static Brush FrozenSolid(int argb)
    {
        var brush = new SolidColorBrush(ColorFromArgb(argb));
        brush.Freeze();
        return brush;
    }

    private static Brush FrozenGradient(int topArgb, int bottomArgb)
    {
        var brush = new LinearGradientBrush(
            ColorFromArgb(topArgb),
            ColorFromArgb(bottomArgb),
            new System.Windows.Point(0.5, 0),
            new System.Windows.Point(0.5, 1));
        brush.Freeze();
        return brush;
    }
}
