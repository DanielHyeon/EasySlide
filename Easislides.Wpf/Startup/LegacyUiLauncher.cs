using System;
using System.Diagnostics;
using System.IO;

namespace Easislides.Wpf.Startup;

/// <summary>--legacy-ui 롤백 시도 결과.</summary>
public enum LegacyUiLaunchOutcome
{
    /// <summary>요청되지 않음(플래그 없음).</summary>
    NotRequested,

    /// <summary>요청됐으나 legacy 실행 파일을 찾지 못함.</summary>
    ExecutableNotFound,

    /// <summary>legacy 실행 파일을 시작함.</summary>
    Launched,

    /// <summary>실행 파일은 있으나 시작 실패(잠금/권한 등). 신규 UI 로 우아하게 계속해야 함.</summary>
    LaunchFailed,
}

/// <summary>legacy WinForms UI 롤백 런처 (ADR-0007).</summary>
public interface ILegacyUiLauncher
{
    /// <summary>요청 시 legacy 실행 파일을 시작한다. 결과를 반환(예외 없이 진단 가능).</summary>
    LegacyUiLaunchOutcome LaunchIfRequested(bool requested);
}

/// <summary>
/// --legacy-ui 안전망 런처 — ADR-0007.
///
/// 신규 WPF 빌드 옆에 함께 배포된 legacy WinForms 실행 파일(Easislides.exe)을 별도 프로세스로
/// 시작한다(§9.4 산출물 분리: Easislides.exe + EasislidesNext.exe). 신규 앱은 이 호출 후
/// 자신의 WPF UI 를 띄우지 않고 종료해, 사용자가 라이브 운영 중 즉시 legacy 로 롤백할 수 있게 한다.
///
/// 파일 존재 확인·프로세스 시작을 델리게이트로 주입받아 단위 테스트가 실제 프로세스를 띄우지 않게 한다.
/// </summary>
public sealed class LegacyUiLauncher : ILegacyUiLauncher
{
    /// <summary>legacy WinForms 실행 파일 이름(신규 빌드와 동일 폴더에 배포 — §9.4).</summary>
    public const string LegacyExecutableName = "Easislides.exe";

    private readonly Func<string, bool> _fileExists;

    // 시작 델리게이트는 "프로세스가 실제로 시작됐는가"를 bool 로 반환한다
    // (Process.Start 가 null 을 반환하는 드문 경우를 Launched 로 오보하지 않도록).
    private readonly Func<string, bool> _startProcess;

    public LegacyUiLauncher(string baseDirectory, Func<string, bool> fileExists, Func<string, bool> startProcess)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        ArgumentNullException.ThrowIfNull(fileExists);
        ArgumentNullException.ThrowIfNull(startProcess);

        LegacyExecutablePath = Path.Combine(baseDirectory, LegacyExecutableName);
        _fileExists = fileExists;
        _startProcess = startProcess;
    }

    /// <summary>현재 실행 폴더 기준 기본 런처(실제 File.Exists + Process.Start).</summary>
    public static LegacyUiLauncher CreateDefault()
        => new(
            AppContext.BaseDirectory,
            File.Exists,
            path => Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }) is not null);

    /// <summary>배포된 legacy 실행 파일의 전체 경로.</summary>
    public string LegacyExecutablePath { get; }

    public LegacyUiLaunchOutcome LaunchIfRequested(bool requested)
    {
        if (!requested)
        {
            return LegacyUiLaunchOutcome.NotRequested;
        }

        if (!_fileExists(LegacyExecutablePath))
        {
            return LegacyUiLaunchOutcome.ExecutableNotFound;
        }

        // 안전망 불변식: 시작 실패가 절대 앱 시작을 크래시시키면 안 된다.
        // Process.Start 는 잠금/권한/UAC 거부 등으로 throw 할 수 있으므로 결과 enum 으로 흡수한다.
        try
        {
            return _startProcess(LegacyExecutablePath)
                ? LegacyUiLaunchOutcome.Launched
                : LegacyUiLaunchOutcome.LaunchFailed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LEGACY-UI] legacy 실행 파일 시작 실패: {ex}");
            return LegacyUiLaunchOutcome.LaunchFailed;
        }
    }
}
