using System.Linq;
using System.Reflection;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Wpf.Ui.Controls;
using Xunit;

namespace Easislides.Wpf.Tests.Theme;

/// <summary>
/// EsIcons 매핑 검증 — docs/ui/icon-migration-map.md의 모든 키가 유효한 SymbolRegular 값에 매핑되는지.
///
/// 현재는 Application.Current 의존성이 없지만, 미래 회귀 방지를 위해 동일 collection에 소속.
/// </summary>
[Collection("WPF Application")]
public class EsIconsTests
{
    private static readonly FieldInfo[] AllFields = typeof(EsIcons)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(SymbolRegular))
        .ToArray();

    [Fact]
    public void EsIcons_ExposesAtLeast50Mappings()
    {
        AllFields.Should().HaveCountGreaterThanOrEqualTo(50,
            "매핑 표 §1~§5 합계 50+ 항목이 EsIcons에 정의돼야 함");
    }

    [Fact]
    public void EsIcons_AllValuesAreValidSymbolRegular()
    {
        var allEnumValues = System.Enum.GetValues<SymbolRegular>();

        foreach (var field in AllFields)
        {
            var value = (SymbolRegular)field.GetValue(null)!;
            allEnumValues.Should().Contain(value,
                $"{field.Name}이 가리키는 SymbolRegular 값이 WPF UI 라이브러리에 존재해야 함");
        }
    }

    [Theory]
    [InlineData(nameof(EsIcons.Bible), SymbolRegular.Book24)]
    [InlineData(nameof(EsIcons.WorshipList), SymbolRegular.TaskListLtr24)]
    [InlineData(nameof(EsIcons.LiveBlack), SymbolRegular.EyeOff24)]
    [InlineData(nameof(EsIcons.LiveToggle), SymbolRegular.VideoClip24)]
    [InlineData(nameof(EsIcons.StatusCheck), SymbolRegular.Checkmark24)]
    [InlineData(nameof(EsIcons.Settings), SymbolRegular.Settings24)]
    [InlineData(nameof(EsIcons.MonitorDual), SymbolRegular.DualScreen24)]
    public void EsIcons_KeyMappings_MatchExpected(string keyName, SymbolRegular expected)
    {
        var field = typeof(EsIcons).GetField(keyName, BindingFlags.Public | BindingFlags.Static);
        field.Should().NotBeNull($"EsIcons.{keyName} 정의돼야 함");

        var value = (SymbolRegular)field!.GetValue(null)!;
        value.Should().Be(expected, $"매핑 표(docs/ui/icon-migration-map.md)와 일치");
    }

    [Fact]
    public void EsIcons_LiveCategoryHasAllRequiredKeys()
    {
        var requiredLiveKeys = new[]
        {
            "LiveBlack", "LiveHide", "LiveHideText", "LiveToggle",
            "LiveSendToOutput", "LiveMoveToOutput", "LiveCamera",
            "LiveCamcorder", "LiveSend", "LiveNewScreen", "LiveHideDisplay",
        };

        var actualNames = AllFields.Select(f => f.Name).ToList();
        foreach (var key in requiredLiveKeys)
        {
            actualNames.Should().Contain(key, $"§1 송출 제어 카테고리에 {key} 필요");
        }
    }

    [Fact]
    public void EsIcons_DomainCategoryHasCoreKeys()
    {
        var requiredDomainKeys = new[]
        {
            "Bible", "Notebook", "WorshipList", "PowerPointSlide",
            "MediaPlay", "Settings", "Help", "Folder",
        };

        var actualNames = AllFields.Select(f => f.Name).ToList();
        foreach (var key in requiredDomainKeys)
        {
            actualNames.Should().Contain(key, $"도메인 핵심 키 {key} 필요");
        }
    }
}
