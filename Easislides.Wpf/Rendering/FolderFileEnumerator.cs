using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Easislides.Wpf.Rendering;

/// <summary>
/// 폴더의 파일을 확장자로 걸러 이름순으로 돌려주는 공통 헬퍼(이미지 갤러리·PPT 브라우저 공유).
/// 폴더가 없거나 접근 불가/읽기 중 사라지면 빈 목록(예외 없이 운영 중 안전 강등).
/// </summary>
internal static class FolderFileEnumerator
{
    public static IReadOnlyList<string> Enumerate(
        string folderPath,
        IReadOnlySet<string> extensions,
        bool includeSubfolders)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return Array.Empty<string>();
        }

        try
        {
            var option = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory
                .EnumerateFiles(folderPath, "*", option)
                .Where(path => extensions.Contains(Path.GetExtension(path)))
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }
}
