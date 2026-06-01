using System;
using System.Collections.Generic;

namespace Easislides.Wpf.Rendering;

/// <summary>
/// 이미지 폴더 탐색기(FrmMain Images 탭 포팅) — 지정한 폴더의 이미지 파일 경로 목록을 돌려준다.
/// 출력 배경으로 쓸 이미지를 운영자가 갤러리에서 고를 수 있게 하는 데이터 소스.
/// 순수 함수형(파일 시스템 열거만) — 폴더가 없거나 접근 불가면 빈 목록(예외를 던지지 않는다).
/// </summary>
public interface IImageLibraryService
{
    /// <summary>폴더 안 이미지 파일 경로를 이름순으로 돌려준다. 폴더가 없으면 빈 목록.</summary>
    IReadOnlyList<string> EnumerateImages(string folderPath, bool includeSubfolders);
}

public sealed class ImageLibraryService : IImageLibraryService
{
    // 출력 배경으로 쓸 수 있는 이미지 확장자(ImageAssetService 의 디코드 가능 형식과 동일).
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp", ".gif", ".jpg", ".jpeg", ".png", ".tif", ".tiff",
    };

    public IReadOnlyList<string> EnumerateImages(string folderPath, bool includeSubfolders)
        => FolderFileEnumerator.Enumerate(folderPath, ImageExtensions, includeSubfolders);
}
