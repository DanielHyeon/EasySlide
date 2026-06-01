using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Easislides.Wpf.Library;

/// <summary>
/// 찬양집 색인 뷰모델(FrmMain PraiseBook/Listing of Selected Folder 포팅) —
/// 곡 목록을 받아 머리글자(초성/영문/숫자/기타)별로 묶은 색인을 보여 준다.
/// 그룹화는 IPraiseBookIndexService 에 위임하고, VM 은 표시용 컬렉션과 요약 텍스트만 만든다.
/// </summary>
public sealed partial class PraiseBookIndexViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusText = string.Empty;

    public PraiseBookIndexViewModel(
        IPraiseBookIndexService service,
        IEnumerable<PraiseBookIndexEntry> songs)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(songs);

        var groups = service.BuildIndex(songs);
        foreach (var group in groups)
        {
            Groups.Add(group);
        }

        var total = groups.Sum(group => group.Entries.Count);
        StatusText = total == 0
            ? "색인할 곡이 없습니다(곡 폴더를 먼저 선택하세요)."
            : $"{total}곡 · {groups.Count}개 머리글자 그룹";
    }

    public ObservableCollection<PraiseBookIndexGroup> Groups { get; } = new();
}
