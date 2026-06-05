using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Library;

public sealed record PowerPointFolderItem(string DisplayName, string FolderPath);

/// <summary>
/// PowerPoint 폴더 탐색기(FrmMain PowerP 탭 포팅) — 폴더의 .ppt/.pptx 파일 경로를 이름순으로 돌려준다.
/// 예배 순서에 추가할 PPT 덱을 운영자가 폴더에서 골라볼 수 있게 하는 데이터 소스.
/// 폴더 열거는 공통 헬퍼에 위임(폴더 없음/접근 불가 시 빈 목록 — 예외 없음).
/// </summary>
public interface IPowerPointLibraryService
{
    IReadOnlyList<string> EnumeratePresentations(string folderPath, bool includeSubfolders);

    IReadOnlyList<PowerPointFolderItem> EnumerateFolders(string rootFolder);
}

public sealed class PowerPointLibraryService : IPowerPointLibraryService
{
    private static readonly IReadOnlySet<string> PresentationExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".ppt", ".pptx" };

    public IReadOnlyList<string> EnumeratePresentations(string folderPath, bool includeSubfolders)
        => FolderFileEnumerator.Enumerate(folderPath, PresentationExtensions, includeSubfolders);

    public IReadOnlyList<PowerPointFolderItem> EnumerateFolders(string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
        {
            return Array.Empty<PowerPointFolderItem>();
        }

        var root = Path.GetFullPath(rootFolder);
        var folders = new List<PowerPointFolderItem>
        {
            new("Powerpoint Items", root),
        };

        try
        {
            folders.AddRange(Directory
                .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                .OrderBy(path => RelativeFolderName(root, path), StringComparer.OrdinalIgnoreCase)
                .Take(254)
                .Select(path => new PowerPointFolderItem("\\" + RelativeFolderName(root, path), path)));
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
