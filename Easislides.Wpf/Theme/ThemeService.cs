using System;
using System.Collections;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Easislides.Wpf.Theme;

/// <summary>인터페이스 크기 — Q2 결정. 디폴트 Standard.</summary>
public enum InterfaceSize
{
    /// <summary>표준 — 본문 14px, 타깃 44px (ScaleFactor 1.0)</summary>
    Standard,
    /// <summary>큰 글씨 — 본문 16px, 타깃 48px (ScaleFactor 1.2)</summary>
    Large,
    /// <summary>시니어 — 본문 20px, 타깃 52px+, 버튼 패딩 +50% (ScaleFactor 1.4)</summary>
    Senior,
}

/// <summary>색 테마 — Q3 결정. 다크는 Solid 강제(Mica 금지), 라이트는 Mica 허용.</summary>
public enum ColorTheme
{
    Light,
    Dark,
}

/// <summary>
/// 런타임 테마/인터페이스 크기 전환 서비스 (ADR-0006).
/// 색 사전을 미리 로드한 뒤 ApplyTheme 시 키 값을 Application.Current.Resources에 직접 등록 — DynamicResource 무효화 100% 보장.
/// </summary>
public interface IThemeService
{
    ColorTheme CurrentTheme { get; }
    InterfaceSize CurrentSize { get; }
    double ScaleFactor { get; }

    /// <summary>마지막 ApplyTheme/ApplyInterfaceSize 결과를 사용자에게 보여줄 짧은 진단 문자열.</summary>
    string LastDiagnostic { get; }

    void ApplyTheme(ColorTheme theme);
    void ApplyInterfaceSize(InterfaceSize size);

    event EventHandler? ThemeChanged;
    event EventHandler? SizeChanged;
}

/// <inheritdoc/>
public sealed partial class ThemeService : ObservableObject, IThemeService
{
    private const string ColorsLightPath = "pack://application:,,,/Easislides.Wpf;component/Theme/Tokens/Colors.Light.xaml";
    private const string ColorsDarkPath = "pack://application:,,,/Easislides.Wpf;component/Theme/Tokens/Colors.Dark.xaml";

    // 기본 폰트 크기 (Standard 기준) — Typography.xaml과 일치해야 함.
    // ADR-0006: ScaleFactor가 이 값에 곱해져 런타임에 갱신됨.
    private static readonly (string Key, double Base)[] ScaledFontSizes = new[]
    {
        ("Type.Display.FontSize", 32.0),
        ("Type.Title1.FontSize", 24.0),
        ("Type.Title2.FontSize", 20.0),
        ("Type.Title3.FontSize", 18.0),
        ("Type.BodyLarge.FontSize", 16.0),
        ("Type.Body.FontSize", 14.0),
        ("Type.Caption.FontSize", 12.0),
        ("Type.ButtonLarge.FontSize", 16.0),
        ("Type.Button.FontSize", 14.0),
    };

    // 시니어 모드에 영향받는 간격 토큰 — 보더·반경은 제외 (ADR-0006).
    private static readonly (string Key, double Base)[] ScaledSpacings = new[]
    {
        ("Spacing.TargetMin", 44.0),
        ("Spacing.ButtonHeight.Sm", 32.0),
        ("Spacing.ButtonHeight.Md", 40.0),
        ("Spacing.ButtonHeight.Lg", 48.0),
    };

    // 색 사전을 미리 로드 — ApplyTheme 호출 시 매번 로드하지 않고 메모리에서 키 값만 복사
    private readonly ResourceDictionary? _lightColors;
    private readonly ResourceDictionary? _darkColors;

    [ObservableProperty] private ColorTheme _currentTheme = ColorTheme.Light;
    [ObservableProperty] private InterfaceSize _currentSize = InterfaceSize.Standard;
    [ObservableProperty] private string _lastDiagnostic = "초기화됨";

    public double ScaleFactor => CurrentSize switch
    {
        InterfaceSize.Standard => 1.0,
        InterfaceSize.Large => 1.2,
        InterfaceSize.Senior => 1.4,
        _ => 1.0,
    };

    public event EventHandler? ThemeChanged;
    public event EventHandler? SizeChanged;

    public ThemeService()
    {
        // 부팅 시 두 색 사전 모두 미리 로드 — 토글 시 즉시 키 갱신만 수행
        _lightColors = TryLoadDictionary(ColorsLightPath);
        _darkColors = TryLoadDictionary(ColorsDarkPath);

        LastDiagnostic = $"색 사전 로드 — Light={_lightColors?.Count ?? -1}, Dark={_darkColors?.Count ?? -1}";
        System.Diagnostics.Debug.WriteLine($"[ThemeService] {LastDiagnostic}");
    }

    private static ResourceDictionary? TryLoadDictionary(string packUri)
    {
        try
        {
            return new ResourceDictionary { Source = new Uri(packUri, UriKind.Absolute) };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] 사전 로드 실패 ({packUri}): {ex}");
            return null;
        }
    }

    public void ApplyTheme(ColorTheme theme)
    {
        if (Application.Current is null) return;

        // 1. WPF UI 라이브러리 테마 — 실패해도 EasiDS 색 갱신은 계속 진행
        //
        // Sprint 0 주의사항: Mica는 ui:FluentWindow(WPF UI 자체 chrome)에서만 정확히 작동한다.
        // 현재 DemoWindow는 표준 <Window>이므로 Mica를 적용하면 라이트 모드 배경이 시스템 Mica
        // 레이어(데스크톱 색)로 덮여버리는 현상 발생 → 양쪽 모두 None 적용.
        // Q3 Mica 정책은 Sprint 1에서 DemoWindow를 ui:FluentWindow로 전환하면서 재활성화 예정.
        try
        {
            var wpfUiTheme = theme == ColorTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
            ApplicationThemeManager.Apply(wpfUiTheme, WindowBackdropType.None, updateAccent: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] WPF UI Apply 실패 (계속 진행): {ex}");
        }

        // 2. EasiDS 색 토큰을 Application.Current.Resources 최상위에 직접 갱신
        //    이 방식이 ResourceDictionary swap보다 DynamicResource 무효화가 확실하게 발생함.
        var source = theme == ColorTheme.Dark ? _darkColors : _lightColors;
        if (source == null)
        {
            LastDiagnostic = $"⚠ {theme} 색 사전이 로드되지 않음";
            System.Diagnostics.Debug.WriteLine($"[ThemeService] {LastDiagnostic}");
            return;
        }

        int updated = 0;
        foreach (DictionaryEntry entry in source)
        {
            Application.Current.Resources[entry.Key] = entry.Value;
            updated++;
        }

        CurrentTheme = theme;
        LastDiagnostic = $"✓ {theme} 적용 — 색 토큰 {updated}개 갱신";
        System.Diagnostics.Debug.WriteLine($"[ThemeService] {LastDiagnostic}");
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyInterfaceSize(InterfaceSize size)
    {
        CurrentSize = size;
        var factor = ScaleFactor;

        // FontSize 토큰 갱신 — DynamicResource 참조 컨트롤이 자동 재렌더
        foreach (var (key, baseValue) in ScaledFontSizes)
        {
            Application.Current.Resources[key] = baseValue * factor;
        }

        foreach (var (key, baseValue) in ScaledSpacings)
        {
            Application.Current.Resources[key] = baseValue * factor;
        }

        LastDiagnostic = $"✓ 크기 {size} (×{factor:F1}) — 폰트 {ScaledFontSizes.Length}개 / 간격 {ScaledSpacings.Length}개 갱신";
        System.Diagnostics.Debug.WriteLine($"[ThemeService] {LastDiagnostic}");
        SizeChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(ScaleFactor));
    }
}
