using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Easislides.Wpf.Composites;

/// <summary>
/// FolderComboBox — 계획서 §5.2 재사용 컴포지트(라벨 + 폴더 선택 콤보박스).
///
/// SongCopy/SongMove/SongMerge 등 여러 창에 반복되던 "캡션 + 폴더 ComboBox(DisplayMemberPath=Name)"
/// 패턴을 하나의 재사용 컨트롤로 통합한다. 호스트마다 바인딩 대상이 다르므로(원본 인라인은
/// DataContext 상속), 단순 View 분리가 아니라 의존 속성(DependencyProperty)으로 노출한다.
///
/// 사용 예:
///   <composites:FolderComboBox Label="대상 폴더" AutomationName="대상 폴더"
///                              ItemsSource="{Binding TargetFolders}"
///                              SelectedItem="{Binding SelectedTargetFolder, Mode=TwoWay}" />
/// </summary>
public partial class FolderComboBox : UserControl
{
    public FolderComboBox()
    {
        InitializeComponent();
    }

    /// <summary>콤보박스 위에 표시할 캡션.</summary>
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FolderComboBox),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>폴더 목록(각 항목은 Name 속성을 가진다).</summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FolderComboBox),
            new PropertyMetadata(null));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// 선택된 폴더 — 기본 양방향 바인딩(원본 인라인 ComboBox 의 Mode=TwoWay 동등).
    /// 주의: 이 DP 에는 CoerceValueCallback/PropertyChangedCallback 을 두지 말 것.
    /// 외부 호스트 ↔ DP ↔ 내부 콤보가 모두 TwoWay 라, 콜백에서 값을 재설정하면
    /// 양방향 핑퐁(재진입) 위험이 생긴다. (현재는 콜백 없음 — 안전)
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FolderComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// 콤보박스의 접근성 이름(캡션과 다를 수 있어 별도 노출 — 예: 캡션 "Source A" + 이름 "Source A 폴더").
    /// 호스트가 반드시 지정해야 한다(사실상 필수). XAML 의 정적 접근성 가드 스캐너가
    /// AutomationProperties.Name 특성 바인딩을 요구하므로, 빈 값일 때 Label 로 폴백시키는
    /// 코드/MultiBinding 방식은 그 특성을 없애 가드를 깨뜨린다 → 직접 바인딩을 유지한다.
    /// </summary>
    public static readonly DependencyProperty AutomationNameProperty =
        DependencyProperty.Register(nameof(AutomationName), typeof(string), typeof(FolderComboBox),
            new PropertyMetadata(string.Empty));

    public string AutomationName
    {
        get => (string)GetValue(AutomationNameProperty);
        set => SetValue(AutomationNameProperty, value);
    }
}
