using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Easislides.Wpf.Library;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Composites;

/// <summary>
/// WorshipListPanel — 계획서 §5.2 재사용 컴포지트(예배 순서 목록 패널).
///
/// View만 분리한 UserControl이다. ViewModel은 호스트에서 상속되는 DataContext
/// (MainViewModel)를 그대로 사용하므로, 바인딩은 MainWindow 인라인 시절과 100% 동일하다.
/// 드래그-드롭 재정렬(§7.5 P1)만 코드비하인드에서 처리한다 — 드래그는 STA 입력 제스처라
/// 단위 테스트가 어렵고, 실제 순서 변경 로직은 테스트된 <see cref="MainViewModel.MoveQueueItem"/>에 위임한다
/// (자동회전 DispatcherTimer 와 동일한 "View=제스처, VM=로직" 분리).
/// </summary>
public partial class WorshipListPanel : UserControl
{
    // 드래그 시작 후보 지점(왼쪽 버튼 누른 위치)과 드래그 대상 항목. 임계 거리 이전엔 단순 클릭으로 둔다.
    private Point _dragStartPoint;
    private LiveQueueItem? _dragCandidate;

    public WorshipListPanel()
    {
        InitializeComponent();
    }

    // 세션 콤보를 열 때 저장된 예배 순서 목록을 새로고침한다(저장·삭제가 다른 창에서 일어나도 항상 최신 목록을 보여 주기 위해).
    // 새로고침 로직은 검증된 VM.RefreshSavedWorshipListNames 가, 여기선 "콤보 열림" 제스처만 감지한다.
    private void SessionCombo_DropDownOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.RefreshSavedWorshipListNames();
        }
    }

    // 우클릭 "이 항목 배경 이미지 → 이미지 선택..." — 파일 선택은 View(코드비하인드)에서, 경로 기록·서식 편집은 검증된 VM 이 맡는다.
    private void SelectItemBackgroundImage_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || !viewModel.CanEditSelectedItemColor)
        {
            return; // 곡 항목이 아니면(메뉴는 비활성이지만 방어적으로) 무시.
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "이 항목의 배경 이미지 선택",
            Filter = "이미지 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
                + "|모든 파일 (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog() == true)
        {
            viewModel.SetSelectedItemBackgroundImage(dialog.FileName);
        }
    }

    // 왼쪽 버튼 누름: 어느 항목 위인지 기억하되, 아직 드래그는 시작하지 않는다(클릭/선택과 공존).
    private void QueueList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _dragCandidate = ItemFromPoint(e.GetPosition(QueueList));
    }

    // 임계 거리 이상 움직이면 드래그를 시작 — 항목 인스턴스를 데이터로 실어 보낸다.
    private void QueueList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragCandidate is null)
        {
            return;
        }

        var moved = e.GetPosition(null) - _dragStartPoint;
        if (Math.Abs(moved.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(moved.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var data = new DataObject(typeof(LiveQueueItem), _dragCandidate);
        try
        {
            DragDrop.DoDragDrop(QueueList, data, DragDropEffects.Move);
        }
        finally
        {
            _dragCandidate = null; // 드래그가 끝나면(드롭/취소) 후보 해제
        }
    }

    // 드래그가 목록 위를 지날 때 커서로 드롭 가능함을 알린다(운영자 확신·UX).
    //  - 큐 항목(LiveQueueItem) = 재정렬(Move), 성경 본문 선택(BibleSelection) = 추가(Copy).
    private void QueueList_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(LiveQueueItem)))
        {
            e.Effects = DragDropEffects.Move;
        }
        else if (e.Data.GetDataPresent(typeof(BibleSelection))
            || e.Data.GetDataPresent(typeof(Easislides.Wpf.Data.SongSummary))
            || e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // 성경 본문 선택·라이브러리 곡·탐색기 외부 파일(PPT/미디어) = 큐에 추가(Copy).
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    // 드롭: 떨어뜨린 위치의 "타깃 항목"을 그대로 VM에 넘긴다(인덱스 계산은 참조-안전한 VM이 전담).
    // 빈 공간(마지막 항목 아래)에 드롭하면 타깃이 null → VM 이 맨 끝으로.
    //  - 큐 항목이면 그 위치로 재정렬, 성경 본문 선택이면 그 위치 앞에 추가(레거시 BibleText DragDrop).
    private void QueueList_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var targetItem = ItemFromPoint(e.GetPosition(QueueList));

        if (e.Data.GetData(typeof(LiveQueueItem)) is LiveQueueItem dragged)
        {
            viewModel.MoveQueueItemRelativeTo(dragged, targetItem);
        }
        else if (e.Data.GetData(typeof(BibleSelection)) is BibleSelection selection)
        {
            viewModel.AddBibleSelectionRelativeTo(selection, targetItem);
        }
        else if (e.Data.GetData(typeof(Easislides.Wpf.Data.SongSummary)) is Easislides.Wpf.Data.SongSummary song)
        {
            // 라이브러리 곡 목록에서 끌어다 놓은 곡 — 드롭 위치 앞에 추가(레거시 외부 소스 드래그).
            viewModel.AddSongRelativeTo(song, targetItem);
        }
        else if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            // 탐색기 등에서 끌어다 놓은 외부 파일(PPT/미디어) — 떨어뜨린 위치(타깃 항목) 앞에 묶음으로 추가
            //  (성경/곡 드래그와 동일한 드롭-위치 삽입). 분류·삽입은 참조-안전한 검증된 VM 이 맡는다.
            viewModel.AddExternalFilesRelativeTo(files, targetItem);
        }
    }

    // 주어진 좌표 아래에 있는 ListBoxItem 의 데이터(LiveQueueItem)를 찾는다(없으면 null).
    private LiveQueueItem? ItemFromPoint(Point point)
    {
        var element = QueueList.InputHitTest(point) as DependencyObject;
        while (element is not null && element is not ListBoxItem)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return (element as ListBoxItem)?.DataContext as LiveQueueItem;
    }
}
