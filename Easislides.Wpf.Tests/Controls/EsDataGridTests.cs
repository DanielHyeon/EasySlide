using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Easislides.Wpf.Tests.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Controls;

/// <summary>
/// EsDataGrid 스타일 검증 — 계획서 §5.1 (Fluent 2 row hover, zebra optional).
/// EsDataGrid는 코드비하인드 없는 순수 ResourceDictionary(Style)이므로
/// 딕셔너리를 로드해 기대 키·TargetType·토큰 참조를 검증한다.
/// </summary>
[Collection("WPF Application")]
public class EsDataGridTests
{
    private static ResourceDictionary LoadDictionary()
        => new()
        {
            Source = new Uri(
                "pack://application:,,,/EasislidesNext;component/Controls/EsDataGrid.xaml",
                UriKind.Absolute),
        };

    [Fact]
    public void Defines_EsDataGrid_Style_Targeting_DataGrid()
    {
        var dict = LoadDictionary();

        dict.Contains("EsDataGrid").Should().BeTrue();
        (dict["EsDataGrid"] as Style)!.TargetType.Should().Be(typeof(DataGrid));
    }

    [Fact]
    public void Defines_Zebra_Variant_Targeting_DataGrid()
    {
        var dict = LoadDictionary();

        dict.Contains("EsDataGrid.Zebra").Should().BeTrue();
        (dict["EsDataGrid.Zebra"] as Style)!.TargetType.Should().Be(typeof(DataGrid));
    }

    [Fact]
    public void Defines_Row_And_ColumnHeader_Styles()
    {
        var dict = LoadDictionary();

        (dict["EsDataGrid.Row"] as Style)!.TargetType.Should().Be(typeof(DataGridRow));
        (dict["EsDataGrid.ColumnHeader"] as Style)!.TargetType.Should().Be(typeof(DataGridColumnHeader));
    }

    [Fact]
    public void Base_Style_Uses_Surface_Token_Background()
    {
        // 매직 색이 아닌 EasiDS 토큰을 DynamicResource로 참조해야 테마 전환에 따라간다.
        var dict = LoadDictionary();
        var style = (Style)dict["EsDataGrid"];

        var backgroundSetter = style.Setters
            .OfType<Setter>()
            .FirstOrDefault(s => s.Property == Control.BackgroundProperty);

        backgroundSetter.Should().NotBeNull("배경은 토큰으로 지정되어야 함");
        (backgroundSetter!.Value is DynamicResourceExtension)
            .Should().BeTrue("배경 토큰은 DynamicResource여야 테마 전환 시 갱신됨");
    }
}
