using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 출력 모양(글자색·배경·정렬·폰트 크기/효과·줄 간격·위치 인디케이터)을 한 묶음으로 담는 영속 템플릿.
/// 예배별/장소별 프리셋을 이름으로 저장·복원한다(레거시 Save/Load Settings Template — §7.3-A).
/// 인-셸 인스펙터가 다루는 출력 모양 설정 키들의 스냅샷이며, 적용 시 각 설정 키에 값을 되쓴다.
/// <para>
/// 스키마 진화 주의: positional record 로 역직렬화하므로 향후 필드를 추가하면 구버전 JSON 에는 그 키가 없어
/// 타입 기본값(0/false/enum 0)이 들어간다. 새 필드 추가 시 기본값이 출력을 깨지 않는지 확인하거나
/// 캡처 시점 합리적 기본값을 갖도록 한다(현재 14개 필드는 안전 — 신규 ShowTitleHeading 의 기본 false 는 기존 동작 유지).
/// </para>
/// </summary>
public sealed record LyricsAppearanceTemplate(
    int TextColorArgb,
    int BackgroundColorArgb,
    int BackgroundColor2Argb,
    bool BackgroundIsGradient,
    LyricsTextAlignment TextAlignment,
    LyricsVerticalAlignment VerticalAlignment,
    int FontSize,
    int LineSpacingPercent,
    bool Bold,
    bool Italic,
    bool Shadow,
    bool ShowNotations,
    bool ShowPositionIndicator,
    // 기본값 false 는 "구버전(이 키가 없는) JSON 역직렬화 시 헤딩 off 로 안전 복원"을 위한 안전망이다.
    // 코드에서 Capture 가 이 인자를 실수로 빠뜨려도 조용히 false 가 되지 않도록 Capture 는 명명 인자로 채운다.
    bool ShowTitleHeading = false)
{
    /// <summary>현재 설정값에서 템플릿을 캡처한다(인-셸 출력 모양 키 전체).</summary>
    public static LyricsAppearanceTemplate Capture(ISettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return new LyricsAppearanceTemplate(
            settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient),
            settings.Get(EasiSettingKeys.LyricsMonitorTextAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorVerticalAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorFontSize),
            settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent),
            settings.Get(EasiSettingKeys.LyricsMonitorBold),
            settings.Get(EasiSettingKeys.LyricsMonitorItalic),
            settings.Get(EasiSettingKeys.LyricsMonitorShadow),
            settings.Get(EasiSettingKeys.LyricsMonitorShowNotations),
            settings.Get(EasiSettingKeys.LyricsMonitorShowPositionIndicator),
            // 명명 인자 — 끝자리 기본값(false)에 묻혀 누락이 무증상 버그가 되는 것을 방지(code-review MINOR).
            ShowTitleHeading: settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading));
    }

    /// <summary>템플릿 값을 설정에 되쓴다(각 키 Set → 출력 VM 이 SettingsChanged 로 라이브 반영).</summary>
    public void ApplyTo(ISettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, TextColorArgb);
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, BackgroundColorArgb);
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, BackgroundColor2Argb);
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundIsGradient, BackgroundIsGradient);
        settings.Set(EasiSettingKeys.LyricsMonitorTextAlignment, TextAlignment);
        settings.Set(EasiSettingKeys.LyricsMonitorVerticalAlignment, VerticalAlignment);
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, FontSize);
        settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, LineSpacingPercent);
        settings.Set(EasiSettingKeys.LyricsMonitorBold, Bold);
        settings.Set(EasiSettingKeys.LyricsMonitorItalic, Italic);
        settings.Set(EasiSettingKeys.LyricsMonitorShadow, Shadow);
        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, ShowNotations);
        settings.Set(EasiSettingKeys.LyricsMonitorShowPositionIndicator, ShowPositionIndicator);
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, ShowTitleHeading);
    }
}

/// <summary>출력 모양 템플릿을 이름으로 저장/불러오기/삭제(§7.3-A Save/Load Settings Template).</summary>
public interface IAppearanceTemplateStore
{
    Task SaveAsync(string name, LyricsAppearanceTemplate template, CancellationToken cancellationToken = default);

    Task<LyricsAppearanceTemplate?> LoadAsync(string name, CancellationToken cancellationToken = default);

    IReadOnlyList<string> ListNames();

    void Delete(string name);
}

/// <summary>
/// 출력 모양 템플릿을 JSON 파일로 영속화 — `%AppData%/EasislidesNext/AppearanceTemplates/{name}.json`.
/// WorshipListStore 와 동일한 경로 안전(무효 문자/예약명/경로 탈출 차단)·원자적 쓰기 패턴.
/// </summary>
public sealed class AppearanceTemplateStore : IAppearanceTemplateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _directory;

    public AppearanceTemplateStore()
        : this(DefaultDirectory())
    {
    }

    public AppearanceTemplateStore(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    private static string DefaultDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasislidesNext",
        "AppearanceTemplates");

    public async Task SaveAsync(string name, LyricsAppearanceTemplate template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        var path = ResolvePath(name);
        Directory.CreateDirectory(_directory);

        var json = JsonSerializer.Serialize(template, JsonOptions);
        // 원자적 쓰기 — temp 에 완전히 쓴 뒤 교체(저장 중 중단돼도 기존 템플릿 보존).
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, path, overwrite: true);
    }

    public async Task<LyricsAppearanceTemplate?> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(name);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        try
        {
            return JsonSerializer.Deserialize<LyricsAppearanceTemplate>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // 손상/비-JSON 파일은 null 로 우아하게 처리(호출자 크래시 방지).
            return null;
        }
    }

    public IReadOnlyList<string> ListNames()
    {
        if (!Directory.Exists(_directory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(_directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void Delete(string name)
    {
        var path = ResolvePath(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    // 이름을 안전한 파일 경로로 해석(무효 문자/예약명/길이/경로 탈출 차단) — WorshipListStore 와 동일 방어선.
    private string ResolvePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("템플릿 이름이 비어 있습니다.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 100
            || trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.EndsWith('.')
            || IsReservedDeviceName(trimmed))
        {
            throw new ArgumentException($"템플릿 이름에 사용할 수 없는 문자/형식입니다: {name}", nameof(name));
        }

        var full = Path.GetFullPath(Path.Combine(_directory, trimmed + ".json"));
        var root = Path.GetFullPath(_directory);
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"템플릿 이름이 허용 경로를 벗어납니다: {name}", nameof(name));
        }

        return full;
    }

    private static bool IsReservedDeviceName(string name)
    {
        var upper = name.ToUpperInvariant();
        if (upper is "CON" or "PRN" or "AUX" or "NUL")
        {
            return true;
        }

        return upper.Length == 4
            && (upper.StartsWith("COM", StringComparison.Ordinal) || upper.StartsWith("LPT", StringComparison.Ordinal))
            && upper[3] is >= '1' and <= '9';
    }
}
