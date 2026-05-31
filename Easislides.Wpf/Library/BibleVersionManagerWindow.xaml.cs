using System.Linq;
using System.Windows;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Library;

/// <summary>
/// 성경 버전 관리 창(레거시 FrmBibleRename 대응) — 버전 목록을 보여 주고 이름을 바꾼다.
/// 이름 변경은 재사용 NameEntryWindow(고유성 검증)로 새 이름을 받아 BibleViewModel.RenameVersion 에 위임.
/// (ManageWorshipListsWindow 와 동일하게 얇은 창 — VM 의 Versions/RenameVersion 을 그대로 쓴다.)
/// </summary>
public partial class BibleVersionManagerWindow : Window
{
    private readonly BibleViewModel _viewModel;

    public BibleVersionManagerWindow(BibleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        // 버전 목록이 아직 비어 있으면(성경 탭을 한 번도 안 본 경우) 여기서 1회 불러온다.
        // 이미 로드돼 있으면 건드리지 않아 좌측 성경 탭의 선택을 보존한다(VM 공유). LoadAsync 는 예외를 내지 않는다.
        Loaded += async (_, _) =>
        {
            if (_viewModel.Versions.Count == 0)
            {
                await _viewModel.LoadAsync().ConfigureAwait(true);
            }
        };
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is not BibleVersion version)
        {
            return;
        }

        // 기존 버전 이름들을 넘겨 중복을 막는다(다이얼로그 VM 이 자기 자신은 제외하고 검사).
        var dialogViewModel = new NameEntryViewModel(
            "성경 버전 이름 변경",
            $"'{version.Name}'의 새 이름:",
            version.Name,
            _viewModel.Versions.Select(v => v.Name));
        var dialog = new NameEntryWindow(dialogViewModel) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.RenameVersion(version, dialog.EnteredName);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
