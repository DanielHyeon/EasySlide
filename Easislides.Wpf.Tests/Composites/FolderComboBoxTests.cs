using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Controls;
using Easislides.Wpf.Composites;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// FolderComboBox 재사용 컨트롤 검증 — 계획서 §5.2 / 작업 A-4.
///
/// 의존 속성(Label/ItemsSource/SelectedItem/AutomationName)이 내부 ComboBox·캡션에
/// 올바르게 전달되고, SelectedItem 이 양방향으로 동기화되는지를 실제 인스턴스로 검증한다.
/// (이 컨트롤은 컨트롤 스타일 사전 의존이 없어 STA 인스턴스화가 가능 — 런타임 검증.)
///
/// 캡션 TextBlock 의 Type.Caption.Style(StaticResource) 해석을 위해 "WPF Application" 픽스처 필요.
/// </summary>
[Collection("WPF Application")]
public class FolderComboBoxTests
{
    private sealed record Folder(string Name);

    /// <summary>UserControl 콘텐츠 트리에서 캡션/콤보를 꺼낸다(루트 StackPanel 의 두 자식).</summary>
    private static (TextBlock Caption, ComboBox Combo) Parts(FolderComboBox sut)
    {
        var panel = (StackPanel)sut.Content;
        return ((TextBlock)panel.Children[0], (ComboBox)panel.Children[1]);
    }

    [Fact]
    public void Label_Flows_To_Caption()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new FolderComboBox { Label = "대상 폴더" };
            sut.UpdateLayout();

            var (caption, _) = Parts(sut);
            caption.Text.Should().Be("대상 폴더");
        });
    }

    [Fact]
    public void ItemsSource_Flows_To_Inner_ComboBox()
    {
        StaHelper.RunOnSta(() =>
        {
            var folders = new List<Folder> { new("A"), new("B") };
            var sut = new FolderComboBox { ItemsSource = folders };
            sut.UpdateLayout();

            var (_, combo) = Parts(sut);
            combo.ItemsSource.Should().BeSameAs(folders);
            combo.DisplayMemberPath.Should().Be("Name", "폴더는 Name 으로 표시되어야 함");
        });
    }

    [Fact]
    public void SelectedItem_Synchronizes_Both_Ways()
    {
        StaHelper.RunOnSta(() =>
        {
            var a = new Folder("A");
            var b = new Folder("B");
            var sut = new FolderComboBox { ItemsSource = new List<Folder> { a, b } };
            sut.UpdateLayout();
            var (_, combo) = Parts(sut);

            // DP → 내부 콤보
            sut.SelectedItem = a;
            combo.SelectedItem.Should().BeSameAs(a);

            // 내부 콤보 → DP (양방향)
            combo.SelectedItem = b;
            sut.SelectedItem.Should().BeSameAs(b);
        });
    }

    [Fact]
    public void AutomationName_Flows_To_Inner_ComboBox()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new FolderComboBox { AutomationName = "대상 폴더" };
            sut.UpdateLayout();

            var (_, combo) = Parts(sut);
            AutomationProperties.GetName(combo).Should().Be("대상 폴더");
        });
    }
}
