using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Composites;

/// <summary>
/// LIVE badge of a worship-list row: inputs [the row, MainViewModel.LiveRow]. The row itself is compared, not its
/// id, so that only one of two rows of the same song carries the badge.
/// </summary>
public sealed class LiveRowBadgeConverter : System.Windows.Data.IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => values is { Length: >= 2 } && values[0] is LiveQueueItem row && ReferenceEquals(row, values[1])
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}

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
    private bool _refreshingSessionCombo;

    public event RoutedEventHandler? AddSelectedSourceRequested;
    public event RoutedEventHandler? ManageWorshipListsRequested;
    public event RoutedEventHandler? OpenSessionNotesRequested;
    public event RoutedEventHandler? EditSelectedItemRequested;

    public WorshipListPanel()
    {
        InitializeComponent();
        // The list is multi-select; removal acts on every selected row, as FrmMain does.
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is MainViewModel viewModel)
                viewModel.SelectedQueueRows = () => [.. QueueList.SelectedItems.OfType<LiveQueueItem>()];
        };
    }

    private void WL_Add_Click(object sender, RoutedEventArgs e)
    {
        AddSelectedSourceRequested?.Invoke(this, e);
        e.Handled = true;
    }

    private void WL_Manage_Click(object sender, RoutedEventArgs e)
    {
        ManageWorshipListsRequested?.Invoke(this, e);
        e.Handled = true;
    }

    private void WL_Notes_Click(object sender, RoutedEventArgs e)
    {
        OpenSessionNotesRequested?.Invoke(this, e);
        e.Handled = true;
    }

    private void WL_Word_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        if (viewModel.Queue.Count == 0)
        {
            MessageBox.Show(
                Window.GetWindow(this),
                "Can't produce lyrics document because the Worship List is empty!",
                "Generate RTF Document",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            e.Handled = true;
            return;
        }

        var documentName = string.IsNullOrWhiteSpace(viewModel.CurrentWorshipListName)
            ? "Worship List"
            : viewModel.CurrentWorshipListName;
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Generate RTF Document",
            DefaultExt = ".rtf",
            Filter = "Rich Text Format (*.rtf)|*.rtf|모든 파일 (*.*)|*.*",
            FileName = BuildWorshipListExportFileName(documentName),
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            var rtf = WorshipListRtfExporter.BuildRtf(documentName, viewModel.Queue.ToArray());
            try
            {
                File.WriteAllText(dialog.FileName, rtf, new UTF8Encoding(false));
                viewModel.StatusText = $"Worship List RTF 저장: {dialog.FileName}";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The target is often still open in Word.
                viewModel.StatusText = $"Worship List RTF 저장 실패: {ex.Message}";
            }
        }

        e.Handled = true;
    }

    private static string BuildWorshipListExportFileName(string name)
    {
        var safe = string.IsNullOrWhiteSpace(name) ? "Worship List" : name.Trim();
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            safe = safe.Replace(invalid, '_');
        }

        return safe.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) ? safe : $"{safe}.rtf";
    }

    private async void WL_Open_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "예배 순서에 추가할 외부 파일 선택",
            Filter = "Valid External Files (*.ppt;*.pptx;*.doc;*.docx;*.txt;*.esi;*.esw;*.json)|*.ppt;*.pptx;*.doc;*.docx;*.txt;*.esi;*.esw;*.json"
                + "|" + SourceFileExtensions.DialogFilter("PowerPoint", SourceFileExtensions.PowerPoint)
                + "|Word 문서 (*.doc;*.docx)|*.doc;*.docx"
                + "|Text/InfoScreen (*.txt;*.esi)|*.txt;*.esi"
                + "|Worship Lists (*.esw;*.json)|*.esw;*.json"
                + "|" + viewModel.MediaDialogFilter("미디어")
                + "|모든 파일 (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = true,
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        await AddExternalFilesFromToolbarAsync(viewModel, dialog.FileNames).ConfigureAwait(true);
        e.Handled = true;
    }

    private static async Task AddExternalFilesFromToolbarAsync(MainViewModel viewModel, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (extension is ".doc" or ".docx")
            {
                await AddWordDocumentAsync(viewModel, fileName).ConfigureAwait(true);
            }
            else if (extension is ".txt" or ".esi")
            {
                await AddTextFileAsNoticeAsync(viewModel, fileName).ConfigureAwait(true);
            }
            else
            {
                viewModel.AddExternalFiles(new[] { fileName });
            }
        }
    }

    private static async Task AddWordDocumentAsync(MainViewModel viewModel, string fileName)
    {
        viewModel.StatusText = "Word 문서를 읽는 중...";
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);

        var documentText = new OfficeLib.WordDoc().GetContents(fileName);
        viewModel.AddWordTextItem(documentText);
    }

    private static async Task AddTextFileAsNoticeAsync(MainViewModel viewModel, string fileName)
    {
        try
        {
            var fileContents = await File.ReadAllTextAsync(fileName).ConfigureAwait(true);
            viewModel.AddTextFileItem(fileName, ExtractLegacyInfoScreenText(fileContents));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            viewModel.StatusText = $"텍스트 파일을 읽을 수 없습니다: {ex.Message}";
        }
    }

    private static string ExtractLegacyInfoScreenText(string text)
    {
        try
        {
            var document = XDocument.Parse(text, LoadOptions.PreserveWhitespace);
            var item = document.Descendants().FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, "Item", StringComparison.OrdinalIgnoreCase));
            var contents = item?.Elements().FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, "Contents", StringComparison.OrdinalIgnoreCase))?.Value.Trim();
            if (!string.IsNullOrWhiteSpace(contents))
            {
                return NormalizeLegacyInfoScreenText(contents);
            }
        }
        catch (System.Xml.XmlException)
        {
        }

        return NormalizeLegacyInfoScreenText(text);
    }

    private static string NormalizeLegacyInfoScreenText(string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (!normalized.StartsWith('['))
        {
            return normalized;
        }

        var close = normalized.IndexOf(']', StringComparison.Ordinal);
        return close >= 0 && close < normalized.Length - 1
            ? normalized[(close + 1)..].TrimStart('\n')
            : normalized;
    }

    // 예배 순서 목록에 포커스가 있을 때 Delete 키 = 선택 항목 제거(목록 삭제 표준 키). 판단은 순수 WorshipListKeyMap,
    // 실제 제거는 검증된 VM.RemoveSelectedItemCommand 에 위임. 컨트롤 레벨 KeyDown 이라 검색창 등 다른 입력의 Delete 는 안 가로챈다.
    private void QueueList_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        if (Axicultus.Wpf.Input.WorshipListKeyMap.IsRemoveSelectedItem(e.Key, Keyboard.Modifiers)
            && viewModel.RemoveSelectedItemCommand.CanExecute(null))
        {
            viewModel.RemoveSelectedItemCommand.Execute(null);
            e.Handled = true;
        }
    }

    // 세션 콤보를 열 때 저장된 예배 순서 목록을 새로고침한다(저장·삭제가 다른 창에서 일어나도 항상 최신 목록을 보여 주기 위해).
    // 새로고침 로직은 검증된 VM.RefreshSavedWorshipListNames 가, 여기선 "콤보 열림" 제스처만 감지한다.
    private void SessionCombo_DropDownOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            _refreshingSessionCombo = true;
            try
            {
                viewModel.RefreshSavedWorshipListNames();
            }
            finally
            {
                _refreshingSessionCombo = false;
            }
        }
    }

    private void SessionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingSessionCombo
            || !ReferenceEquals(e.OriginalSource, sender)
            || e.AddedItems.Count == 0
            || sender is not ComboBox { IsKeyboardFocusWithin: true })
        {
            return;
        }

        if (TryLoadSelectedWorshipList())
        {
            e.Handled = true;
        }
    }

    private void SessionCombo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return && TryLoadSelectedWorshipList())
        {
            e.Handled = true;
        }
    }

    private void SessionCombo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TryLoadSelectedWorshipList())
        {
            e.Handled = true;
        }
    }

    private bool TryLoadSelectedWorshipList()
    {
        if (DataContext is not MainViewModel viewModel
            || !viewModel.LoadSelectedWorshipListCommand.CanExecute(null))
        {
            return false;
        }

        viewModel.LoadSelectedWorshipListCommand.Execute(null);
        return true;
    }

    private void CMenuWorship_Opened(object sender, RoutedEventArgs e)
    {
        var selected = QueueList.SelectedItem as LiveQueueItem;
        CMenuWorship_Edit.Header = MainWindow.EditItemMenuHeader(selected?.Title);
        CMenuWorship_Edit.IsEnabled = !string.IsNullOrEmpty(selected?.Title);
        bool mediaSelected = QueueList.SelectedItems.Count == 1 && selected != null && LiveItemKindMatcher.IsMedia(selected.Kind);
        CMenuWorship_Play.IsEnabled = mediaSelected;
        CMenuWorship_PlayOnOutput.IsEnabled = mediaSelected;
    }

    private void CMenuWorship_Clear_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var owner = Window.GetWindow(this);
        if (vm.Queue.Count == 0 || string.IsNullOrWhiteSpace(vm.CurrentWorshipListName))
        {
            MessageBox.Show(owner, vm.Queue.Count == 0 ? "Worship list is already empty." : "No worship list is currently loaded.", "Clear Worship List");
            return;
        }
        if (MessageBox.Show(owner, $"Really clear the worship list '{vm.CurrentWorshipListName}'?\n\nThe current worship list will be moved to trash.",
                "Clear Worship List", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            vm.ArchiveAndClearCurrentWorshipList();
    }

    private void CMenuWorship_SelectAll_Click(object sender, RoutedEventArgs e)
    {
        QueueList.SelectAll();
        QueueList.Focus();
        e.Handled = true;
    }

    private void CMenuWorship_UnselectAll_Click(object sender, RoutedEventArgs e)
    {
        QueueList.SelectedItems.Clear();
        QueueList.Focus();
        e.Handled = true;
    }

    private void CMenuWorship_Edit_Click(object sender, RoutedEventArgs e)
    {
        EditSelectedItemRequested?.Invoke(this, e);
        e.Handled = true;
    }

    private async void CMenuWorship_AddUsages_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.AddWorshipListSongsToUsagesAsync().ConfigureAwait(true);
        }

        e.Handled = true;
    }

    // 왼쪽 버튼 누름: 어느 항목 위인지 기억하되, 아직 드래그는 시작하지 않는다(클릭/선택과 공존).
    private void QueueList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _dragCandidate = ItemFromPoint(e.GetPosition(QueueList));
        // FrmMain WorshipList_MouseUp reloads the preview on every click. Clicking the row that is already
        // selected changes no selection here, so a library or praise-book preview is dropped explicitly.
        if (_dragCandidate is not null && DataContext is MainViewModel viewModel && ReferenceEquals(_dragCandidate, viewModel.SelectedItem))
        {
            viewModel.ShowWorshipSelectionPreview();
        }
    }

    // 우클릭 메뉴 대상도 FrmMain처럼 "마우스 아래 항목"으로 맞춘다. 기존 선택과 다른 항목을 우클릭해도 Edit/Play가 그 행에 적용된다.
    private void QueueList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = ItemFromPoint(e.GetPosition(QueueList));
        // The context menu acts on the preview item: a right-click on the selected row shows that row again.
        if (item is not null && DataContext is MainViewModel current && ReferenceEquals(item, current.SelectedItem))
        {
            current.ShowWorshipSelectionPreview();
        }

        if (item is null || QueueList.SelectedItems.Contains(item))
        {
            return;
        }

        QueueList.SelectedItem = item;
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.SelectedItem = item;
        }
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
            || e.Data.GetDataPresent(typeof(Axicultus.Wpf.Data.SongSummary))
            || e.Data.GetDataPresent(typeof(InfoScreenSelection))
            || e.Data.GetDataPresent(typeof(InfoScreenSelection[]))
            || e.Data.GetDataPresent(typeof(PraiseBookIndexEntry))
            || e.Data.GetDataPresent(typeof(PraiseBookIndexEntry[]))
            || e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // 성경 본문 선택·라이브러리 곡·InfoScreen·PraiseBook·탐색기 외부 파일(PPT/미디어) = 큐에 추가(Copy).
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
    private async void QueueList_Drop(object sender, DragEventArgs e)
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
        else if (e.Data.GetData(typeof(Axicultus.Wpf.Data.SongSummary)) is Axicultus.Wpf.Data.SongSummary song)
        {
            // 라이브러리 곡 목록에서 끌어다 놓은 곡 — 드롭 위치 앞에 추가(레거시 외부 소스 드래그).
            viewModel.AddSongRelativeTo(song, targetItem);
        }
        else if (e.Data.GetData(typeof(InfoScreenSelection)) is InfoScreenSelection infoScreen)
        {
            // InfoScr 소스 탭에서 끌어다 놓은 저장 공지 — 드롭 위치 앞에 추가(레거시 InfoScreenList DragDrop).
            viewModel.AddTextItemRelativeTo(infoScreen.Text, infoScreen.Options, targetItem);
        }
        else if (e.Data.GetData(typeof(InfoScreenSelection[])) is InfoScreenSelection[] infoScreens)
        {
            // InfoScr 다중 선택 드래그 — FrmMain ListView.SelectedItems 처럼 선택된 저장 공지를 순서대로 같은 타깃 앞에 묶음 삽입.
            LiveQueueItem? firstAdded = null;
            foreach (var selectedInfoScreen in infoScreens)
            {
                var added = viewModel.AddTextItemRelativeTo(
                    selectedInfoScreen.Text,
                    selectedInfoScreen.Options,
                    targetItem);
                firstAdded ??= added;
            }

            if (firstAdded is not null)
            {
                viewModel.SelectedItem = firstAdded;
            }
        }
        else if (e.Data.GetData(typeof(PraiseBookIndexEntry)) is PraiseBookIndexEntry praiseBookEntry)
        {
            // 하단 PraiseBook 항목을 예배 순서에 드롭 — 더블클릭 추가와 같은 곡 해석을 쓰되 드롭 위치를 보존한다.
            await viewModel.AddPraiseBookSongRelativeToAsync(praiseBookEntry, targetItem).ConfigureAwait(true);
        }
        else if (e.Data.GetData(typeof(PraiseBookIndexEntry[])) is PraiseBookIndexEntry[] praiseBookEntries)
        {
            LiveQueueItem? firstAdded = null;
            foreach (var selectedPraiseBookEntry in praiseBookEntries)
            {
                var added = await viewModel.AddPraiseBookSongRelativeToAsync(selectedPraiseBookEntry, targetItem).ConfigureAwait(true);
                firstAdded ??= added;
            }

            if (firstAdded is not null)
            {
                viewModel.SelectedItem = firstAdded;
            }
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
        while (element is not null and not ListBoxItem)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return (element as ListBoxItem)?.DataContext as LiveQueueItem;
    }
}
