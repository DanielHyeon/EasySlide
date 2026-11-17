using Easislides.Wpf.Shell;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Wpf.Ui.Controls;
using Xunit;

namespace Easislides.Wpf.Tests.Theme;

/// <summary>예배 순서 항목 종류 → Fluent 아이콘 매핑(순수 함수) 검증.</summary>
public class WorshipItemIconTests
{
    [Theory]
    [InlineData(LiveItemKinds.Song, SymbolRegular.Notebook24)]        // 곡 = 노트북
    [InlineData(LiveItemKinds.Bible, SymbolRegular.Book24)]           // 성경 = 책
    [InlineData(LiveItemKinds.PowerPoint, SymbolRegular.SlideLayout24)] // PPT = 슬라이드
    [InlineData(LiveItemKinds.Media, SymbolRegular.Video24)]          // 미디어 = 비디오
    [InlineData(LiveItemKinds.Notice, SymbolRegular.Comment24)]       // 공지 = 말풍선
    public void SymbolForKind_MapsEachKindToItsIcon(string kind, SymbolRegular expected)
        => WorshipItemIcon.SymbolForKind(kind).Should().Be(expected);

    [Theory]
    [InlineData(LiveItemKinds.Item)]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("UnknownKind")]
    public void SymbolForKind_UnknownOrEmpty_FallsBackToGenericItem(string? kind)
        => WorshipItemIcon.SymbolForKind(kind).Should().Be(EsIcons.GenericItem);

    [Fact]
    public void SymbolForKind_GenericFallback_IsDocumentIcon()
        => EsIcons.GenericItem.Should().Be(SymbolRegular.Document24);

    [Theory]
    [InlineData(LiveItemKinds.Song, "곡")]
    [InlineData(LiveItemKinds.Bible, "성경")]
    [InlineData(LiveItemKinds.PowerPoint, "PowerPoint")]
    [InlineData(LiveItemKinds.Media, "미디어")]
    [InlineData(LiveItemKinds.Notice, "공지")]
    public void KindLabel_MapsEachKindToKoreanLabel(string kind, string expected)
        => WorshipItemIcon.KindLabel(kind).Should().Be(expected);

    [Theory]
    [InlineData(LiveItemKinds.Item)]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("UnknownKind")]
    public void KindLabel_UnknownOrEmpty_FallsBackToItem(string? kind)
        => WorshipItemIcon.KindLabel(kind).Should().Be("항목");
}
