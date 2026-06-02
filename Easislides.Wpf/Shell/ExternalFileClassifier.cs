using System;
using System.Linq;

namespace Easislides.Wpf.Shell;

/// <summary>외부에서 끌어다 놓은 파일이 어떤 예배 순서 항목이 되는지(확장자 기준). 인식 못 하면 Unsupported.</summary>
public enum ExternalFileKind
{
    /// <summary>지원하지 않는 확장자 — 큐에 추가하지 않는다.</summary>
    Unsupported,

    /// <summary>PowerPoint 파일(.ppt/.pptx) — PowerPoint 항목.</summary>
    PowerPoint,

    /// <summary>미디어 파일(영상/오디오) — 미디어 항목.</summary>
    Media,
}

/// <summary>
/// 탐색기 등에서 예배 순서로 끌어다 놓은 파일을 확장자로 분류한다(순수 — 드래그 제스처는 View, 분류·추가는 VM/이 헬퍼).
/// 레거시 "외부 파일 추가" 필터와 같은 확장자 집합을 쓴다(파일 추가 메뉴와 일관). 인식 못 한 확장자는 Unsupported(추가 안 함).
/// </summary>
public static class ExternalFileClassifier
{
    // 파일 추가 메뉴(AddExternalFile)의 필터와 동일한 확장자 집합 — 한 곳에서 관리해 메뉴/드래그가 어긋나지 않게.
    private static readonly string[] PowerPointExtensions = [".ppt", ".pptx"];
    private static readonly string[] MediaExtensions =
        [".mp4", ".avi", ".wmv", ".mov", ".mkv", ".mp3", ".wav", ".wma"];

    /// <summary>파일 경로의 확장자로 항목 종류를 정한다. 비었거나 모르는 확장자면 <see cref="ExternalFileKind.Unsupported"/>.</summary>
    public static ExternalFileKind Classify(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return ExternalFileKind.Unsupported;
        }

        var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
        if (PowerPointExtensions.Contains(ext))
        {
            return ExternalFileKind.PowerPoint;
        }

        if (MediaExtensions.Contains(ext))
        {
            return ExternalFileKind.Media;
        }

        return ExternalFileKind.Unsupported;
    }
}
