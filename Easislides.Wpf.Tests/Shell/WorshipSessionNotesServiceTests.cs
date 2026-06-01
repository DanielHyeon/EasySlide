using System;
using System.IO;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class WorshipSessionNotesServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_Notes_{Guid.NewGuid():N}");

    [Fact]
    public void SetThenGet_RoundTripsNotes()
    {
        var sut = new WorshipSessionNotesService(_dir);

        sut.SetNotes("주일오전", "순서: 찬양 3곡 → 말씀 → 봉헌");

        sut.GetNotes("주일오전").Should().Be("순서: 찬양 3곡 → 말씀 → 봉헌");
    }

    [Fact]
    public void GetNotes_MissingSession_ReturnsEmpty()
    {
        var sut = new WorshipSessionNotesService(_dir);

        sut.GetNotes("없는세션").Should().BeEmpty();
    }

    [Fact]
    public void SetNotes_Empty_DeletesNotes()
    {
        var sut = new WorshipSessionNotesService(_dir);
        sut.SetNotes("세션", "내용 있음");

        sut.SetNotes("세션", "");

        sut.GetNotes("세션").Should().BeEmpty();
    }

    [Fact]
    public void SetNotes_WhitespaceOnly_DeletesNotes()
    {
        // 공백뿐인 메모는 빈 것으로 취급해 파일 삭제(뷰모델의 "비웠습니다" 판정과 일치).
        var sut = new WorshipSessionNotesService(_dir);
        sut.SetNotes("세션", "내용 있음");

        sut.SetNotes("세션", "   \n  ").Should().BeTrue();

        sut.GetNotes("세션").Should().BeEmpty();
    }

    [Fact]
    public void SetNotes_Success_ReturnsTrue()
    {
        var sut = new WorshipSessionNotesService(_dir);

        sut.SetNotes("세션", "메모").Should().BeTrue();
    }

    [Fact]
    public void Notes_AreKeyedPerSession()
    {
        var sut = new WorshipSessionNotesService(_dir);

        sut.SetNotes("A", "에이 메모");
        sut.SetNotes("B", "비 메모");

        sut.GetNotes("A").Should().Be("에이 메모");
        sut.GetNotes("B").Should().Be("비 메모");
    }

    [Fact]
    public void SetNotes_PersistsAcrossInstances()
    {
        new WorshipSessionNotesService(_dir).SetNotes("세션", "기억할 메모");

        new WorshipSessionNotesService(_dir).GetNotes("세션").Should().Be("기억할 메모");
    }

    [Theory]
    [InlineData("..\\escape")]
    [InlineData("sub/dir")]
    [InlineData("")]
    public void SetNotes_UnsafeKey_Throws(string key)
    {
        var sut = new WorshipSessionNotesService(_dir);

        var act = () => sut.SetNotes(key, "x");

        act.Should().Throw<ArgumentException>();
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }
}
