using System;
using System.Collections.Generic;

namespace Easislides.Wpf.Shell;

/// <summary>예배 순서 항목에서 발견된 문제의 종류.</summary>
public enum WorshipItemProblemKind
{
    /// <summary>PowerPoint 항목인데 연결된 파일 경로가 비어 있다(재생할 파일 없음).</summary>
    MissingFilePath,

    /// <summary>연결된 파일(PPT·미디어)이 디스크에 없다 — 이동·삭제·이름변경됐을 가능성.</summary>
    FileNotFound,

    /// <summary>곡 항목인데 가사 DB 에 해당 곡이 없다 — 삭제·이동·DB 교체로 사라졌을 가능성(비동기·DB 의존 검사).</summary>
    SongNotInDatabase,
}

/// <summary>예배 순서 검증에서 발견한 문제 하나(어느 항목이 왜 문제인지).</summary>
public sealed record WorshipItemProblem(LiveQueueItem Item, WorshipItemProblemKind Kind, string Message);

/// <summary>
/// 예배 순서(큐)를 라이브 송출 전에 점검한다(레거시 FrmMain ValidateWorshipListItems 대응).
/// 예배 도중 "깨진 PPT/이동·삭제된 미디어"로 화면이 안 뜨는 사고를 미리 거른다.
/// <para>
/// 현재 검사 범위는 <b>파일 존재</b> 차원이다: ContentPath 가 가리키는 PPT·미디어 파일이 실제로 있는지,
/// PowerPoint 항목에 파일 경로가 비어 있지 않은지. 파일이 아닌 입력 장치(카메라/캡처) 미디어와
/// 곡·성경·공지처럼 파일이 없는 항목은 건너뛴다. (곡의 DB 존재 검증은 후속 — 비동기·DB 의존이라 분리.)
/// </para>
/// <para>파일 존재 판정은 주입 가능(<c>fileExists</c>)하여 단위 테스트가 디스크 없이 동작한다.</para>
/// </summary>
public sealed class WorshipListValidator
{
    private readonly Func<string, bool> _fileExists;

    public WorshipListValidator(Func<string, bool>? fileExists = null)
        => _fileExists = fileExists ?? System.IO.File.Exists;

    /// <summary>
    /// 큐 항목들을 점검해 문제 목록을 돌려준다(문제가 없으면 빈 목록). 입력 순서를 유지한다.
    /// </summary>
    public IReadOnlyList<WorshipItemProblem> Validate(IEnumerable<LiveQueueItem>? items)
    {
        var problems = new List<WorshipItemProblem>();
        if (items is null)
        {
            return problems;
        }

        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var hasPath = !string.IsNullOrWhiteSpace(item.ContentPath);

            // PowerPoint 항목은 반드시 파일이 있어야 한다 — 경로가 비면 명백한 결함.
            if (!hasPath)
            {
                if (LiveItemKindMatcher.IsPowerPoint(item.Kind))
                {
                    problems.Add(new WorshipItemProblem(
                        item, WorshipItemProblemKind.MissingFilePath, $"{Label(item)}: 연결된 파일이 없습니다."));
                }

                continue; // 경로 없는 곡·성경·공지·입력 장치 등은 파일 검사 대상이 아님.
            }

            // 입력 장치(카메라/캡처) 미디어는 파일이 아니므로 존재 검사 제외.
            if (LiveItemKindMatcher.IsDeviceMedia(item.Kind))
            {
                continue;
            }

            // 경로가 있으면(PPT·미디어 파일·외부 파일) 실제로 존재해야 한다.
            if (!_fileExists(item.ContentPath!))
            {
                problems.Add(new WorshipItemProblem(
                    item, WorshipItemProblemKind.FileNotFound, $"{Label(item)}: 파일을 찾을 수 없습니다 ({item.ContentPath})."));
            }
        }

        return problems;
    }

    // 사람이 읽기 좋은 항목 이름 — 제목이 비면 ID 로 대체.
    private static string Label(LiveQueueItem item)
        => string.IsNullOrWhiteSpace(item.Title) ? item.Id : item.Title;
}
