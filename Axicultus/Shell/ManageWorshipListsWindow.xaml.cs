using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Axicultus.Wpf.Shell;

// 레거시 대체: FrmManageItemLists(예배 순서 저장/불러오기/삭제) — G2 / 라이브 큐 plumbing.
// MainViewModel 의 워십 리스트 명령을 호출하는 얇은 창(피커). 저장 목록은 로컬 ObservableCollection 으로 갱신.
public partial class ManageWorshipListsWindow : Window
{
    private readonly MainViewModel _viewModel;

    public ManageWorshipListsWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = this;
        RefreshSavedLists();
    }

    /// <summary>저장된 예배 순서 이름 목록(바인딩 대상).</summary>
    public ObservableCollection<string> SavedLists { get; } = [];

    private void RefreshSavedLists()
    {
        SavedLists.Clear();
        foreach (var name in _viewModel.GetSavedWorshipLists())
        {
            SavedLists.Add(name);
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveWorshipListAsync(NameBox.Text);
        RefreshSavedLists();
    }

    private async void Load_Click(object sender, RoutedEventArgs e)
    {
        if (SavedListBox.SelectedItem is string name)
        {
            await _viewModel.LoadWorshipListAsync(name);
            Close();
        }
    }

    // 선택 순서를 현재 큐 뒤에 이어 붙인다(불러오기=대체와 달리 병합=추가, 레거시 .esw 병합).
    private async void Merge_Click(object sender, RoutedEventArgs e)
    {
        if (SavedListBox.SelectedItem is string name)
        {
            await _viewModel.MergeWorshipListAsync(name);
            Close();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SavedListBox.SelectedItem is string name)
        {
            // FrmManageItemLists asks before deleting.
            if (MessageBox.Show(this, $"'{name}' 예배 순서를 휴지통으로 옮기시겠습니까?", "예배 순서 삭제",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;
            _viewModel.DeleteWorshipList(name);
            RefreshSavedLists();
        }
    }

    /// <summary>Lists in Trash (FrmManageItemLists TrashList).</summary>
    public ObservableCollection<string> TrashedLists { get; } = [];

    private void RefreshTrashedLists()
    {
        TrashedLists.Clear();
        foreach (var name in _viewModel.GetTrashedWorshipLists())
        {
            TrashedLists.Add(name);
        }
    }

    // FrmManageItemLists.MainTabControl_SelectedIndexChanged: each tab refreshes its list and shows its own buttons.
    private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, MainTabControl) || TrashButtons is null || ListButtons is null)
        {
            return; // a list selection bubbling up, or the first selection during InitializeComponent
        }

        var trash = ReferenceEquals(MainTabControl.SelectedItem, TrashTab);
        TrashButtons.Visibility = trash ? Visibility.Visible : Visibility.Collapsed;
        ListButtons.Visibility = trash ? Visibility.Collapsed : Visibility.Visible;
        if (trash) RefreshTrashedLists();
        else RefreshSavedLists();
    }

    private string[] SelectedTrashedLists() => [.. TrashListBox.SelectedItems.OfType<string>()];

    private void Restore_Click(object sender, RoutedEventArgs e)
    {
        var names = SelectedTrashedLists();
        if (names.Length == 0)
        {
            MessageBox.Show(this, "복원할 예배 순서를 선택하세요.", "복원", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var question = names.Length == 1 ? $"'{names[0]}' 예배 순서를 복원하시겠습니까?" : $"선택한 예배 순서 {names.Length}개를 복원하시겠습니까?";
        if (MessageBox.Show(this, question, "복원", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        ReportTrashResult(_viewModel.RestoreWorshipLists(names), names.Length, "복원");
    }

    private void DeletePermanently_Click(object sender, RoutedEventArgs e)
    {
        var names = SelectedTrashedLists();
        if (names.Length == 0)
        {
            MessageBox.Show(this, "영구 삭제할 예배 순서를 선택하세요.", "영구 삭제", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var question = names.Length == 1
            ? $"'{names[0]}' 예배 순서를 영구 삭제하시겠습니까? 되돌릴 수 없습니다."
            : $"선택한 예배 순서 {names.Length}개를 영구 삭제하시겠습니까? 되돌릴 수 없습니다.";
        if (MessageBox.Show(this, question, "영구 삭제", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        ReportTrashResult(_viewModel.DeleteTrashedWorshipLists(names), names.Length, "영구 삭제");
    }

    private void EmptyTrash_Click(object sender, RoutedEventArgs e)
    {
        RefreshTrashedLists();
        if (TrashedLists.Count == 0)
        {
            MessageBox.Show(this, "휴지통이 이미 비어 있습니다.", "휴지통 비우기", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show(this, "휴지통을 비우시겠습니까? 휴지통의 모든 예배 순서가 영구 삭제되며 되돌릴 수 없습니다.", "휴지통 비우기",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        string[] names = [.. TrashedLists];
        ReportTrashResult(_viewModel.DeleteTrashedWorshipLists(names), names.Length, "영구 삭제");
    }

    // FrmManageItemLists reports the files it could not move or delete; the reason is in the status line.
    private void ReportTrashResult(int done, int requested, string verb)
    {
        RefreshTrashedLists();
        if (done < requested)
        {
            MessageBox.Show(this, $"{requested - done}개는 {verb}하지 못했습니다. {done}개는 처리했습니다.\n\n{_viewModel.StatusText}", verb,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // 선택한 예배 순서의 이름을 바꾼다 — 재사용 이름 입력 다이얼로그(고유성 검증)를 띄우고, 확인 시 VM 에 위임.
    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (SavedListBox.SelectedItem is not string oldName)
        {
            return;
        }

        // 기존 이름 목록을 넘겨 중복을 막는다(다이얼로그 VM 이 자기 자신은 제외하고 검사).
        var dialogViewModel = new NameEntryViewModel(
            "예배 순서 이름 변경",
            $"'{oldName}'의 새 이름:",
            oldName,
            _viewModel.GetSavedWorshipLists());
        var dialog = new NameEntryWindow(dialogViewModel) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.RenameWorshipList(oldName, dialog.EnteredName);
            RefreshSavedLists();
        }
    }
}
