using System.Windows;
using System.Windows.Controls;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 팔레트 ListBox 의 ItemContainerStyle 은 BasedOn="{StaticResource {x:Type ListBoxItem}}" 로
/// wpfui 기본 ListBoxItem 테마(선택·호버 비주얼)를 보존한다. 그 암시적 스타일이 실제로 해석되는지
/// 검증해, MainWindow 로드 시 리소스 누락(XamlParseException)으로 창이 안 뜨는 회귀를 막는다.
/// (테스트 픽스처는 wpfui 를 로드하지 않으므로 여기서 ControlsDictionary 를 직접 로드해 확인.
///  "WPF Application" 컬렉션으로 Application 을 띄워 pack:// 스킴이 등록되게 한다.)
/// </summary>
[Collection("WPF Application")]
public sealed class PaletteItemContainerStyleTests
{
    [Fact]
    public void WpfUi_Provides_ImplicitListBoxItemStyle_SoItemContainerBasedOnResolves()
    {
        StaHelper.RunOnSta(() =>
        {
            var resources = new ResourceDictionary();
            resources.MergedDictionaries.Add(new global::Wpf.Ui.Markup.ControlsDictionary());

            // 키가 없으면 인덱서가 throw → 실제 앱에서 BasedOn 도 런타임에 깨진다는 뜻.
            var style = resources[typeof(ListBoxItem)];

            style.Should().BeOfType<Style>(
                "BasedOn={x:Type ListBoxItem} 가 해석되려면 wpfui 암시적 ListBoxItem 스타일이 있어야 함");
        });
    }
}
