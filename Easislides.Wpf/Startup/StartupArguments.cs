using System;
using System.Collections.Generic;
using System.Linq;

namespace Easislides.Wpf.Startup;

/// <summary>
/// 앱 시작 CLI 인자 파서 — 계획서 §9.4 / ADR-0007.
///
/// 인식하는 플래그:
///  - --demo      : 데모 갤러리 창 표시(개발용).
///  - --legacy-ui : 신규 WPF 빌드에서 legacy WinForms UI 로 롤백(ADR-0007 안전망, M3까지 활성).
/// 알 수 없는 인자는 무시한다.
/// </summary>
public sealed class StartupArguments
{
    public bool UseDemo { get; }

    public bool UseLegacyUi { get; }

    public StartupArguments(IEnumerable<string>? args)
    {
        var list = args?.ToList() ?? new List<string>();
        UseDemo = HasFlag(list, "--demo");
        UseLegacyUi = HasFlag(list, "--legacy-ui");
    }

    private static bool HasFlag(IEnumerable<string> args, string flag)
        => args.Any(a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));
}
