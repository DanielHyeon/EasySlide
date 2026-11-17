using System.Linq;
using System.Windows;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Library;

/// <summary>
/// 성경 버전 관리 창(레거시 FrmBibleRename 확장) — 버전 목록을 보여 주고 추가·삭제·순서 변경·이름 변경을 한다.
/// 모든 동작은 BibleViewModel(검증 + IBibleRepository write)에 위임하고, 창은 제스처·확인 다이얼로그만 담당한다.
/// (ManageWorshipListsWindow 와 동일하게 얇은 창 — VM 의 Versions/SelectedVersion 을 그대로 쓴다.)
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

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is BibleVersion version)
        {
            _viewModel.MoveVersionUp(version);
        }
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is BibleVersion version)
        {
            _viewModel.MoveVersionDown(version);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is not BibleVersion version)
        {
            return;
        }

        // 삭제는 숨김(되돌릴 수 있음)이지만 운영 중 실수 방지를 위해 확인을 받는다.
        var confirm = MessageBox.Show(
            this,
            $"'{version.Name}' 버전을 목록에서 삭제할까요?\n(본문 파일은 보존되며 나중에 다시 추가할 수 있습니다.)",
            "성경 버전 삭제",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirm == MessageBoxResult.OK)
        {
            _viewModel.DeleteVersion(version);
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var candidates = _viewModel.GetAddableVersions();
        if (candidates.Count == 0)
        {
            MessageBox.Show(
                this,
                "추가할 수 있는 성경 파일이 없습니다.\n(HolyBibles 폴더의 성경 파일이 모두 이미 목록에 있습니다.)",
                "성경 버전 추가",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        // 추가 후보(숨김/신규 파일)에서 하나를 고르고 이름을 입력받아 VM 에 위임.
        var dialog = new BibleVersionAddWindow(candidates, _viewModel.Versions.Select(v => v.Name)) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.AddVersion(dialog.SelectedFileName, dialog.EnteredName);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
