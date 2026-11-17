using System.Collections.Generic;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class WorshipSessionNotesViewModelTests
{
    private sealed class FakeNotes : IWorshipSessionNotes
    {
        public Dictionary<string, string> Store { get; } = new();

        // 저장 실패를 모사하려면 true 로 둔다(거짓 성공 방지 테스트용).
        public bool FailSave { get; set; }

        public string GetNotes(string sessionKey) => Store.TryGetValue(sessionKey, out var v) ? v : string.Empty;

        public bool SetNotes(string sessionKey, string notes)
        {
            if (FailSave)
            {
                return false;
            }

            Store[sessionKey] = notes;
            return true;
        }
    }

    [Fact]
    public void Constructor_LoadsExistingNotesForSession()
    {
        var notes = new FakeNotes();
        notes.Store["주일오전"] = "기존 메모";

        var sut = new WorshipSessionNotesViewModel(notes, "주일오전");

        sut.NotesText.Should().Be("기존 메모");
        sut.SessionKey.Should().Be("주일오전");
    }

    [Fact]
    public void Constructor_BlankSessionKey_FallsBackToGeneral()
    {
        var sut = new WorshipSessionNotesViewModel(new FakeNotes(), "   ");

        sut.SessionKey.Should().Be("일반");
    }

    [Fact]
    public void Save_PersistsNotesForSession()
    {
        var notes = new FakeNotes();
        var sut = new WorshipSessionNotesViewModel(notes, "저녁예배") { NotesText = "새 메모" };

        sut.SaveCommand.Execute(null);

        notes.GetNotes("저녁예배").Should().Be("새 메모");
        sut.StatusText.Should().Contain("저장");
    }

    [Fact]
    public void Save_EmptyNotes_ReportsCleared()
    {
        var notes = new FakeNotes();
        var sut = new WorshipSessionNotesViewModel(notes, "세션") { NotesText = "" };

        sut.SaveCommand.Execute(null);

        sut.StatusText.Should().Contain("비웠");
    }

    [Fact]
    public void Save_WhenServiceFails_ReportsFailure_NotFalseSuccess()
    {
        // 디스크 오류 등으로 저장이 실패하면 거짓 성공("저장했습니다")이 아니라 실패를 알려야 한다.
        var notes = new FakeNotes { FailSave = true };
        var sut = new WorshipSessionNotesViewModel(notes, "세션") { NotesText = "내용" };

        sut.SaveCommand.Execute(null);

        sut.StatusText.Should().Contain("실패");
    }
}
