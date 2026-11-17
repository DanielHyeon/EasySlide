using System;
using System.Linq;

namespace Easislides.Wpf.Shell;

/// <summary>외부에서 끌어다 놓은 파일의 종류(확장자 기준). 소비 측이 종류에 맞게 처리한다(큐는 PPT/미디어만, 배경 드롭은 이미지). 인식 못 하면 Unsupported.</summary>
public enum ExternalFileKind
{
    /// <summary>지원하지 않는 확장자 — 어디에도 쓰지 않는다.</summary>
    Unsupported,

    /// <summary>PowerPoint 파일(.ppt/.pptx) — 예배 순서 PowerPoint 항목.</summary>
    PowerPoint,

    /// <summary>미디어 파일(영상/오디오) — 예배 순서 미디어 항목.</summary>
    Media,

    /// <summary>이미지 파일(.jpg/.png 등) — 큐 항목이 아니라 출력 배경 이미지로 쓴다(미리보기 영역에 드롭).</summary>
    Image,
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
    // 출력 배경 드롭에 쓰는 이미지 확장자 — 배경 이미지 선택 대화상자 필터와 같은 집합.
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];

    /// <summary>파일 경로의 확장자로 파일 종류를 정한다. 비었거나 모르는 확장자면 <see cref="ExternalFileKind.Unsupported"/>.</summary>
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

        if (ImageExtensions.Contains(ext))
        {
            return ExternalFileKind.Image;
        }

        return ExternalFileKind.Unsupported;
    }
}
