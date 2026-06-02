using System;
using System.Linq;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class SystemFontCatalogTests
{
    private static readonly string[] Favorites = ["Malgun Gothic", "Arial"];

    [Fact]
    public void Build_PutsFavoritesFirst_ThenInstalledRestSorted()
    {
        // 추천 글꼴이 맨 앞(순서 유지), 그 뒤에 설치된 나머지가 가나다·ABC 순.
        var installed = new[] { "Verdana", "Calibri", "Arial", "Malgun Gothic" };

        var result = SystemFontCatalog.BuildFontFamilyList(installed, Favorites);

        result.Should().Equal("Malgun Gothic", "Arial", "Calibri", "Verdana");
    }

    [Fact]
    public void Build_DeduplicatesCaseInsensitively()
    {
        // 추천·설치에 같은 글꼴이 대소문자만 다르게 있어도 한 번만(추천 표기 유지).
        var installed = new[] { "ARIAL", "malgun gothic", "Tahoma" };

        var result = SystemFontCatalog.BuildFontFamilyList(installed, Favorites);

        result.Should().Equal("Malgun Gothic", "Arial", "Tahoma");
        result.Count(n => string.Equals(n, "Arial", StringComparison.OrdinalIgnoreCase))
            .Should().Be(1, "대소문자만 다른 중복은 합쳐야 함");
    }

    [Fact]
    public void Build_NullOrEmptyInstalled_ReturnsFavoritesOnly()
    {
        // 설치 글꼴 열거가 비거나 없으면(헤드리스·열거 실패) 추천 목록만(무회귀).
        SystemFontCatalog.BuildFontFamilyList(null, Favorites).Should().Equal("Malgun Gothic", "Arial");
        SystemFontCatalog.BuildFontFamilyList(Array.Empty<string>(), Favorites).Should().Equal("Malgun Gothic", "Arial");
    }

    [Fact]
    public void Build_KeepsFavoritesEvenWhenNotInstalled()
    {
        // 추천 글꼴은 설치 여부와 무관하게 제안으로 남긴다(편집 콤보라 직접 입력도 되니까).
        var installed = new[] { "Tahoma" };

        var result = SystemFontCatalog.BuildFontFamilyList(installed, Favorites);

        result.Should().Equal("Malgun Gothic", "Arial", "Tahoma");
    }

    [Fact]
    public void Build_SkipsBlankNames_AndTrims()
    {
        // 빈/공백 이름은 버리고, 앞뒤 공백은 다듬는다(콤보에 빈 줄·중복 공백 글꼴이 안 생기게).
        var installed = new[] { "  ", "", "  Georgia  ", "Tahoma" };
        var favorites = new[] { " Arial ", "" };

        var result = SystemFontCatalog.BuildFontFamilyList(installed, favorites);

        result.Should().Equal("Arial", "Georgia", "Tahoma");
    }

    [Fact]
    public void Build_PreservesFavoriteOrder_NotAlphabetical()
    {
        // 추천 글꼴은 알파벳이 아니라 준 순서를 그대로(운영자가 대표 글꼴을 정한 순서대로 보이게).
        var favorites = new[] { "Segoe UI", "맑은 고딕", "Arial" };
        var installed = new[] { "Arial", "맑은 고딕", "Segoe UI" };

        var result = SystemFontCatalog.BuildFontFamilyList(installed, favorites);

        result.Should().Equal("Segoe UI", "맑은 고딕", "Arial");
    }
}
