using System;
using System.Collections.Generic;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Library;

/// <summary>
/// PowerPoint 폴더 탐색기(FrmMain PowerP 탭 포팅) — 폴더의 .ppt/.pptx 파일 경로를 이름순으로 돌려준다.
/// 예배 순서에 추가할 PPT 덱을 운영자가 폴더에서 골라볼 수 있게 하는 데이터 소스.
/// 폴더 열거는 공통 헬퍼에 위임(폴더 없음/접근 불가 시 빈 목록 — 예외 없음).
/// </summary>
public interface IPowerPointLibraryService
{
    IReadOnlyList<string> EnumeratePresentations(string folderPath, bool includeSubfolders);
}

public sealed class PowerPointLibraryService : IPowerPointLibraryService
{
    private static readonly IReadOnlySet<string> PresentationExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".ppt", ".pptx" };

    public IReadOnlyList<string> EnumeratePresentations(string folderPath, bool includeSubfolders)
        => FolderFileEnumerator.Enumerate(folderPath, PresentationExtensions, includeSubfolders);
}
