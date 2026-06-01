using System;
using System.Collections.Generic;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Library;

/// <summary>
/// 미디어 폴더 탐색기(FrmMain Media 탭 포팅) — 폴더의 동영상·오디오 파일 경로를 이름순으로 돌려준다.
/// 예배 순서에 추가할 미디어를 운영자가 폴더에서 골라볼 수 있게 하는 데이터 소스(PowerPoint 폴더 브라우저와 동일 구조).
/// 폴더 열거는 공통 헬퍼에 위임(폴더 없음/접근 불가 시 빈 목록 — 예외 없음).
/// </summary>
public interface IMediaLibraryService
{
    IReadOnlyList<string> EnumerateMedia(string folderPath, bool includeSubfolders);
}

public sealed class MediaLibraryService : IMediaLibraryService
{
    // 동영상 + 오디오 확장자(MainWindow 파일 대화상자·InferMediaType 과 동일 어휘).
    private static readonly IReadOnlySet<string> MediaExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".avi", ".wmv", ".mov", ".mkv", ".m4v",                 // 동영상
            ".mp3", ".wav", ".wma", ".m4a", ".aac", ".flac", ".ogg",        // 오디오
        };

    public IReadOnlyList<string> EnumerateMedia(string folderPath, bool includeSubfolders)
        => FolderFileEnumerator.Enumerate(folderPath, MediaExtensions, includeSubfolders);
}
