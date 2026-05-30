using System.Collections.ObjectModel;
using System.Windows;

namespace Easislides.Wpf.Shell;

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
    public ObservableCollection<string> SavedLists { get; } = new();

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

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SavedListBox.SelectedItem is string name)
        {
            _viewModel.DeleteWorshipList(name);
            RefreshSavedLists();
        }
    }
}
