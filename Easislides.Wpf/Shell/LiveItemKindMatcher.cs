using System;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 라이브 큐 항목 종류(Kind) 판별을 한 곳에 모은 공용 매처. 레거시/별칭("P"·"PPT"·"M"·"Video" 등)을 포함해
/// PowerPoint·미디어 항목을 알아본다. MainViewModel·예배 순서 검증 등 여러 곳이 같은 어휘를 쓰도록 단일화한다
/// (이전엔 MainViewModel 안에 같은 별칭 목록이 흩어져 있었음).
/// </summary>
public static class LiveItemKindMatcher
{
    /// <summary>PowerPoint 항목인지 — LiveItemKinds.PowerPoint + 레거시 별칭(P/PPT/Presentation).</summary>
    public static bool IsPowerPoint(string? kind)
        => Eq(kind, "P")
           || Eq(kind, "PPT")
           || Eq(kind, LiveItemKinds.PowerPoint)
           || Eq(kind, "Presentation");

    /// <summary>미디어 항목인지 — LiveItemKinds.Media + 레거시/별칭(M/Video/Audio/LiveCamera/CaptureDevice).</summary>
    public static bool IsMedia(string? kind)
        => Eq(kind, "M")
           || Eq(kind, LiveItemKinds.Media)
           || Eq(kind, "Video")
           || Eq(kind, "Audio")
           || Eq(kind, "LiveCamera")
           || Eq(kind, "Live Camera")
           || Eq(kind, "CaptureDevice");

    /// <summary>
    /// 입력 장치(카메라·캡처) 미디어인지 — 파일이 아니라 장치라 "파일 존재" 검증 대상이 아니다.
    /// </summary>
    public static bool IsDeviceMedia(string? kind)
        => Eq(kind, "LiveCamera")
           || Eq(kind, "Live Camera")
           || Eq(kind, "CaptureDevice");

    private static bool Eq(string? kind, string value)
        => string.Equals(kind, value, StringComparison.OrdinalIgnoreCase);
}
