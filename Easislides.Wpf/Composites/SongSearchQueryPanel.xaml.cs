using System.Windows.Controls;

namespace Easislides.Wpf.Composites;

/// <summary>
/// SongSearchQueryPanel — 계획서 §5.2 재사용 컴포지트(곡 검색 쿼리 폼 카드).
///
/// View만 분리한 UserControl이다. ViewModel은 호스트에서 상속되는 DataContext
/// (SearchUsageViewModel)를 그대로 사용하므로, 코드비하인드는 InitializeComponent 외에 없다.
/// 동작/바인딩은 SearchUsageWindow 인라인 시절과 100% 동일(동작 동등성 우선).
/// </summary>
public partial class SongSearchQueryPanel : UserControl
{
    public SongSearchQueryPanel()
    {
        InitializeComponent();
    }
}
