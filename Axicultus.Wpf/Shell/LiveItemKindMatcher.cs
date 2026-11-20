using System;
using System.IO;

namespace Axicultus.Wpf.Shell;

/// <summary>
/// 라이브 큐 항목 종류(Kind) 판별을 한 곳에 모은 공용 매처. 레거시/별칭("P"·"PPT"·"M"·"Video" 등)을 포함해
/// PowerPoint·미디어 항목을 알아본다. MainViewModel·예배 순서 검증 등 여러 곳이 같은 어휘를 쓰도록 단일화한다
/// (이전엔 MainViewModel 안에 같은 별칭 목록이 흩어져 있었음).
/// </summary>
public static class LiveItemKindMatcher
{
    /// <summary>찬양/가사 항목인지 — LiveItemKinds.Song + 레거시 ESW 타입(D/S/Song/Lyrics).</summary>
    public static bool IsSong(string? kind)
        => Eq(kind, "D")
           || Eq(kind, "S")
           || Eq(kind, LiveItemKinds.Song)
           || Eq(kind, "Lyrics");

    /// <summary>성경 항목인지 — LiveItemKinds.Bible + 레거시 ESW 타입(B/Bible).</summary>
    public static bool IsBible(string? kind)
        => Eq(kind, "B")
           || Eq(kind, LiveItemKinds.Bible);

    /// <summary>공지/텍스트 항목인지 — LiveItemKinds.Notice + 레거시 InfoScreen/Text 타입(T/I/W).</summary>
    public static bool IsNotice(string? kind)
        => Eq(kind, "T")
           || Eq(kind, "I")
           || Eq(kind, "W")
           || Eq(kind, LiveItemKinds.Notice)
           || Eq(kind, "Info")
           || Eq(kind, "InfoScreen")
           || Eq(kind, "Text");

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

    internal static bool IsExternalTextFileItem(LiveQueueItem item)
        => item.Id.StartsWith("esw:T:", StringComparison.OrdinalIgnoreCase)
            || item.Id.StartsWith("text-file:", StringComparison.OrdinalIgnoreCase)
            || (IsNotice(InferLegacyKind(item)) && ResolveExternalTextContentPath(item) != null);

    internal static string InferLegacyKind(LiveQueueItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Kind)
            && !string.Equals(item.Kind, LiveItemKinds.Item, StringComparison.OrdinalIgnoreCase))
        {
            return item.Kind;
        }

        var typeCode = ResolveLegacyTypeCode(item);
        return typeCode switch
        {
            "D" => LiveItemKinds.Song,
            "S" => LiveItemKinds.Song,
            "B" => LiveItemKinds.Bible,
            "T" => LiveItemKinds.Notice,
            "I" => LiveItemKinds.Notice,
            "W" => LiveItemKinds.Notice,
            "P" => LiveItemKinds.PowerPoint,
            "M" => LiveItemKinds.Media,
            _ when item.Id.StartsWith("song:", StringComparison.OrdinalIgnoreCase) => LiveItemKinds.Song,
            _ when item.Id.StartsWith("bible:", StringComparison.OrdinalIgnoreCase) => LiveItemKinds.Bible,
            _ when item.Id.StartsWith("powerpoint:", StringComparison.OrdinalIgnoreCase) => LiveItemKinds.PowerPoint,
            _ when item.Id.StartsWith("media:", StringComparison.OrdinalIgnoreCase) => LiveItemKinds.Media,
            _ when item.Id.StartsWith("text-file:", StringComparison.OrdinalIgnoreCase) => LiveItemKinds.Notice,
            _ => item.Kind,
        };
    }

    private static string ResolveLegacyTypeCode(LiveQueueItem item)
    {
        if (item.Id.StartsWith("esw:", StringComparison.OrdinalIgnoreCase))
        {
            var start = "esw:".Length;
            var end = item.Id.IndexOf(':', start);
            return end > start
                ? item.Id.Substring(start, end - start).ToUpperInvariant()
                : string.Empty;
        }

        if (item.Id.Length >= 2 && char.IsLetter(item.Id[0]) && char.IsDigit(item.Id[1]))
        {
            return char.ToUpperInvariant(item.Id[0]).ToString();
        }

        return string.Empty;
    }

    internal static string? ResolveExternalTextContentPath(LiveQueueItem item)
    {
        if (IsTextContentPath(item.ContentPath))
        {
            return item.ContentPath!.Trim();
        }

        if (item.Id.StartsWith("esw:T:", StringComparison.OrdinalIgnoreCase))
        {
            var path = item.Id.Substring("esw:T:".Length).Trim();
            if (IsTextContentPath(path))
            {
                return path;
            }
        }

        if (IsTextContentPath(item.Title))
        {
            return item.Title.Trim();
        }

        return null;
    }

    private static bool IsTextContentPath(string? path)
        => !string.IsNullOrWhiteSpace(path)
            && string.Equals(Path.GetExtension(path.Trim()), ".txt", StringComparison.OrdinalIgnoreCase);

    private static bool Eq(string? kind, string value)
        => string.Equals(kind, value, StringComparison.OrdinalIgnoreCase);
}
