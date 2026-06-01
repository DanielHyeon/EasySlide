using System;
using System.IO;
using System.Linq;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class RecentWorshipListsServiceTests : IDisposable
{
    private readonly string _file = Path.Combine(Path.GetTempPath(), $"EasiSlides_Recent_{Guid.NewGuid():N}.json");

    [Fact]
    public void Record_AddsMostRecentFirst()
    {
        var sut = new RecentWorshipListsService(_file);

        sut.Record("A");
        sut.Record("B");

        sut.GetRecent().Should().Equal("B", "A");
    }

    [Fact]
    public void Record_DuplicateName_MovesToFront_NoDup()
    {
        var sut = new RecentWorshipListsService(_file);
        sut.Record("A");
        sut.Record("B");

        sut.Record("a"); // 대소문자 무시 중복 → 앞으로 이동, 중복 없음

        sut.GetRecent().Should().Equal("a", "B");
    }

    [Fact]
    public void Record_BlankName_Ignored()
    {
        var sut = new RecentWorshipListsService(_file);

        sut.Record("   ");
        sut.Record("");

        sut.GetRecent().Should().BeEmpty();
    }

    [Fact]
    public void Record_CapsAtEightMostRecent()
    {
        var sut = new RecentWorshipListsService(_file);
        for (var i = 1; i <= 10; i++)
        {
            sut.Record($"L{i}");
        }

        var recent = sut.GetRecent();
        recent.Should().HaveCount(8);
        recent.First().Should().Be("L10");
        recent.Should().NotContain("L1").And.NotContain("L2");
    }

    [Fact]
    public void Record_PersistsAcrossInstances()
    {
        new RecentWorshipListsService(_file).Record("주일오전");

        var reopened = new RecentWorshipListsService(_file);

        reopened.GetRecent().Should().Equal("주일오전");
    }

    [Fact]
    public void Load_CorruptFile_ReturnsEmpty()
    {
        File.WriteAllText(_file, "{ not json");

        var sut = new RecentWorshipListsService(_file);

        sut.GetRecent().Should().BeEmpty();
    }

    public void Dispose()
    {
        try { File.Delete(_file); } catch { }
    }
}
