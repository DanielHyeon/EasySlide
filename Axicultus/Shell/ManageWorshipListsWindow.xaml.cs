using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Axicultus.Wpf.Localization;

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
            if (MessageBox.Show(this, UiText.Format("ManageWorshipListsWindow", "DeleteConfirm", name), UiText.Get("ManageWorshipListsWindow", "DeleteTitle"),
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
            MessageBox.Show(this, UiText.Get("ManageWorshipListsWindow", "RestoreNone"), UiText.Get("ManageWorshipListsWindow", "Restore"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var question = names.Length == 1 ? UiText.Format("ManageWorshipListsWindow", "RestoreOne", names[0]) : UiText.Format("ManageWorshipListsWindow", "RestoreMany", names.Length);
        if (MessageBox.Show(this, question, UiText.Get("ManageWorshipListsWindow", "Restore"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        ReportTrashResult(_viewModel.RestoreWorshipLists(names), names.Length, restore: true);
    }

    private void DeletePermanently_Click(object sender, RoutedEventArgs e)
    {
        var names = SelectedTrashedLists();
        if (names.Length == 0)
        {
            MessageBox.Show(this, UiText.Get("ManageWorshipListsWindow", "DeleteNone"), UiText.Get("ManageWorshipListsWindow", "DeletePermanently"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var question = names.Length == 1
            ? UiText.Format("ManageWorshipListsWindow", "DeleteOne", names[0])
            : UiText.Format("ManageWorshipListsWindow", "DeleteMany", names.Length);
        if (MessageBox.Show(this, question, UiText.Get("ManageWorshipListsWindow", "DeletePermanently"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        ReportTrashResult(_viewModel.DeleteTrashedWorshipLists(names), names.Length, restore: false);
    }

    private void EmptyTrash_Click(object sender, RoutedEventArgs e)
    {
        RefreshTrashedLists();
        if (TrashedLists.Count == 0)
        {
            MessageBox.Show(this, UiText.Get("ManageWorshipListsWindow", "TrashEmpty"), UiText.Get("ManageWorshipListsWindow", "EmptyTrash"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show(this, UiText.Get("ManageWorshipListsWindow", "EmptyTrashConfirm"), UiText.Get("ManageWorshipListsWindow", "EmptyTrash"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        string[] names = [.. TrashedLists];
        ReportTrashResult(_viewModel.DeleteTrashedWorshipLists(names), names.Length, restore: false);
    }

    // FrmManageItemLists reports the files it could not move or delete; the reason is in the status line.
    private void ReportTrashResult(int done, int requested, bool restore)
    {
        RefreshTrashedLists();
        if (done < requested)
        {
            MessageBox.Show(this, UiText.Format("ManageWorshipListsWindow", restore ? "RestoreIncomplete" : "DeleteIncomplete", requested - done, done, _viewModel.StatusText),
                UiText.Get("ManageWorshipListsWindow", restore ? "Restore" : "DeletePermanently"),
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
            UiText.Get("ManageWorshipListsWindow", "RenameTitle"),
            UiText.Format("ManageWorshipListsWindow", "RenamePrompt", oldName),
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
