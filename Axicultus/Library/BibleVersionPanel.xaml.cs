using System.Linq;
using System.Windows;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Library;

public partial class BibleVersionPanel : System.Windows.Controls.UserControl
{
    private BibleViewModel Model => (BibleViewModel)DataContext;

    public BibleVersionPanel()
    {
        InitializeComponent();
        // 버전 목록이 아직 비어 있으면(성경 탭을 한 번도 안 본 경우) 여기서 1회 불러온다.
        // 이미 로드돼 있으면 건드리지 않아 좌측 성경 탭의 선택을 보존한다(VM 공유). LoadAsync 는 예외를 내지 않는다.
        Loaded += async (_, _) =>
        {
            if (DataContext is BibleViewModel model && model.Versions.Count == 0)
            {
                await Model.LoadAsync().ConfigureAwait(true);
            }
        };
    }

    private void SaveLayout_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is not BibleVersion version) return;
        if (!int.TryParse(LinkedFolder.Text, out var folder) || !int.TryParse(FontPoints.Text, out var size))
        {
            MessageBox.Show(Window.GetWindow(this), UiText.Get("BibleVersionPanel", "EnterNumbers"), UiText.Get("BibleVersionPanel", "LayoutTitle"));
            return;
        }
        Model.SetVersionLayout(version, folder, size);
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is not BibleVersion version)
        {
            return;
        }

        // 기존 버전 이름들을 넘겨 중복을 막는다(다이얼로그 VM 이 자기 자신은 제외하고 검사).
        var dialogViewModel = new NameEntryViewModel(
            UiText.Get("BibleVersionPanel", "RenameTitle"),
            UiText.Format("BibleVersionPanel", "RenamePrompt", version.Name),
            version.Name,
            Model.Versions.Select(v => v.Name));
        var dialog = new NameEntryWindow(dialogViewModel) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            Model.RenameVersion(version, dialog.EnteredName);
        }
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is BibleVersion version)
        {
            Model.MoveVersionUp(version);
        }
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        if (VersionList.SelectedItem is BibleVersion version)
        {
            Model.MoveVersionDown(version);
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
            Window.GetWindow(this),
            UiText.Format("BibleVersionPanel", "DeleteConfirm", version.Name),
            UiText.Get("BibleVersionPanel", "DeleteTitle"),
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirm == MessageBoxResult.OK)
        {
            Model.DeleteVersion(version);
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var candidates = Model.GetAddableVersions();
        if (candidates.Count == 0)
        {
            MessageBox.Show(
                Window.GetWindow(this),
                UiText.Get("BibleVersionPanel", "NothingToAdd"),
                UiText.Get("BibleVersionPanel", "AddTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        // 추가 후보(숨김/신규 파일)에서 하나를 고르고 이름을 입력받아 VM 에 위임.
        var dialog = new BibleVersionAddWindow(candidates, Model.Versions.Select(v => v.Name)) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            Model.AddVersion(dialog.SelectedFileName, dialog.EnteredName);
        }
    }

}
