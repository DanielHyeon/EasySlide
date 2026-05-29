using System.Windows.Controls;

namespace Easislides.Wpf.Composites;

/// <summary>
/// WorshipListPanel — 계획서 §5.2 재사용 컴포지트(예배 순서 목록 패널).
///
/// View만 분리한 UserControl이다. ViewModel은 호스트에서 상속되는 DataContext
/// (MainViewModel)를 그대로 사용하므로, 코드비하인드는 InitializeComponent 외에 없다.
/// 라이브 송출 경로의 일부이므로 동작/바인딩은 MainWindow 인라인 시절과 100% 동일해야 한다.
/// </summary>
public partial class WorshipListPanel : UserControl
{
    public WorshipListPanel()
    {
        InitializeComponent();
    }
}
