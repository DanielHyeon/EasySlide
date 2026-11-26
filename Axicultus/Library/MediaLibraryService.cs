using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Rendering;

namespace Axicultus.Wpf.Library;

public sealed record MediaFolderItem(string DisplayName, string FolderPath);

/// <summary>
/// 미디어 폴더 탐색기(FrmMain Media 탭 포팅) — 폴더의 동영상·오디오 파일 경로를 이름순으로 돌려준다.
/// 예배 순서에 추가할 미디어를 운영자가 폴더에서 골라볼 수 있게 하는 데이터 소스(PowerPoint 폴더 브라우저와 동일 구조).
/// 폴더 열거는 공통 헬퍼에 위임(폴더 없음/접근 불가 시 빈 목록 — 예외 없음).
/// </summary>
public interface IMediaLibraryService
{
    IReadOnlyList<string> EnumerateMedia(string folderPath, bool includeSubfolders);

    IReadOnlyList<MediaFolderItem> EnumerateFolders(string rootFolder);
}

/// <param name="workingFolder">Where the operator's extension lists are (Admin\Database); without it the shipped lists are read.</param>
public sealed class MediaLibraryService(string? workingFolder = null) : IMediaLibraryService
{
    public IReadOnlyList<string> EnumerateMedia(string folderPath, bool includeSubfolders)
        => FolderFileEnumerator.Enumerate(folderPath, SourceFileExtensions.Media(workingFolder), includeSubfolders);

    public IReadOnlyList<MediaFolderItem> EnumerateFolders(string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
        {
            return Array.Empty<MediaFolderItem>();
        }

        var root = Path.GetFullPath(rootFolder);
        var folders = new List<MediaFolderItem>
        {
            new(UiText.Get(nameof(MediaLibraryService), "RootFolder"), root),
        };

        try
        {
            folders.AddRange(Directory
                .EnumerateDirectories(root, "*", Axicultus.Module.SourceFileFolder.SearchOptions(true))
                .OrderBy(path => RelativeFolderName(root, path), StringComparer.OrdinalIgnoreCase)
                .Take(254)
                .Select(path => new MediaFolderItem("\\" + RelativeFolderName(root, path), path)));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return folders;
    }

    private static string RelativeFolderName(string root, string path)
        => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '\\');
}
