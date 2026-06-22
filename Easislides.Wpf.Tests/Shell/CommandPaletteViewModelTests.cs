using System.Collections.Generic;
using System.Linq;
using Easislides.Wpf.Input;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 명령 팔레트(⌘K, §7.4) ViewModel 검증 — 검색 필터·선택 이동·실행·열기/닫기.
/// 제스처(키 입력)는 View 가 처리하고, 필터/실행 로직은 이 VM 이 담당한다(테스트 가능 코어).
/// </summary>
public class CommandPaletteViewModelTests
{
    [Fact]
    public void Open_PopulatesAllResults_AndSelectsFirst()
    {
        var sut = CreateSut(out _);

        sut.Open();

        sut.IsOpen.Should().BeTrue();
        sut.Query.Should().BeEmpty("열 때 검색어 초기화");
        sut.Results.Should().HaveCount(3);
        sut.SelectedCommand.Should().Be(sut.Results[0], "첫 결과 자동 선택");
    }

    [Fact]
    public void Query_FiltersByDisplayName_CaseInsensitive()
    {
        var sut = CreateSut(out _);
        sut.Open();

        sut.Query = "검은";

        sut.Results.Should().ContainSingle();
        sut.Results[0].DisplayName.Should().Be("검은 화면");
    }

    [Fact]
    public void HasNoResults_TrueOnlyWhenSearchMatchesNothing()
    {
        // "결과 없음" 안내 — 검색어가 비면(전체) false, 매칭 있으면 false, 일치하는 게 없을 때만 true.
        var sut = CreateSut(out _);
        sut.Open();
        sut.HasNoResults.Should().BeFalse("검색어 비면 전체가 보이므로 결과 있음");

        sut.Query = "검은"; // 매칭 1건
        sut.HasNoResults.Should().BeFalse("일치 명령 있으면 결과 있음");

        sut.Query = "존재하지않는명령어zzz"; // 매칭 0건
        sut.HasNoResults.Should().BeTrue("일치하는 게 없으면 결과 없음");

        sut.Query = string.Empty; // 다시 전체
        sut.HasNoResults.Should().BeFalse("검색어 지우면 다시 전체");
    }

    [Fact]
    public void HasNoResults_RaisesPropertyChanged_OnQueryChange()
    {
        var sut = CreateSut(out _);
        sut.Open();
        var notified = new List<bool>();
        sut.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CommandPaletteViewModel.HasNoResults)) notified.Add(sut.HasNoResults); };

        sut.Query = "없는명령zzz";

        notified.Should().Contain(true, "결과 없음으로 바뀔 때 통지(안내 가시성 갱신)");
    }

    [Fact]
    public void Query_FiltersByCategory()
    {
        var sut = CreateSut(out _);
        sut.Open();

        sut.Query = "Output";

        sut.Results.Should().OnlyContain(c => c.Category == "Output");
    }

    [Fact]
    public void Query_NoMatch_EmptyResults_NullSelection()
    {
        var sut = CreateSut(out _);
        sut.Open();

        sut.Query = "존재하지않는명령";

        sut.Results.Should().BeEmpty();
        sut.SelectedCommand.Should().BeNull();
    }

    [Fact]
    public void Query_KeepsSelectionWithinResults()
    {
        var sut = CreateSut(out _);
        sut.Open();

        sut.Query = "Live"; // 카테고리 Live 만 남음

        sut.SelectedCommand.Should().NotBeNull();
        sut.Results.Should().Contain(sut.SelectedCommand!);
    }

    [Fact]
    public void ExecuteSelected_InvokesByIdAndCloses()
    {
        var sut = CreateSut(out var invoked);
        sut.Open();
        sut.Query = "검은";
        var target = sut.Results[0];

        sut.ExecuteSelectedCommand();

        invoked.Should().ContainSingle().Which.Should().Be(target.Id);
        sut.IsOpen.Should().BeFalse("실행 후 팔레트 닫힘");
    }

    [Fact]
    public void ExecuteSelected_NoSelection_DoesNothing()
    {
        var sut = CreateSut(out var invoked);
        sut.Open();
        sut.Query = "존재하지않는명령"; // 결과 없음 → 선택 없음

        sut.ExecuteSelectedCommand();

        invoked.Should().BeEmpty("선택이 없으면 실행하지 않음");
    }

    [Fact]
    public void SelectNext_And_SelectPrevious_MoveWithinResults()
    {
        var sut = CreateSut(out _);
        sut.Open(); // 3개 결과, 첫 항목 선택

        sut.SelectNext();
        sut.SelectedCommand.Should().Be(sut.Results[1]);

        sut.SelectNext();
        sut.SelectedCommand.Should().Be(sut.Results[2]);

        sut.SelectNext();
        sut.SelectedCommand.Should().Be(sut.Results[2], "마지막에서 더 내려가도 마지막 유지");

        sut.SelectPrevious();
        sut.SelectedCommand.Should().Be(sut.Results[1]);
    }

    [Fact]
    public void Close_SetsIsOpenFalse()
    {
        var sut = CreateSut(out _);
        sut.Open();

        sut.Close();

        sut.IsOpen.Should().BeFalse();
    }

    private static CommandPaletteViewModel CreateSut(out List<string> invoked)
    {
        var captured = new List<string>();
        invoked = captured;
        var catalog = new CommandCatalog(new[]
        {
            Descriptor("output.open", "Output", "출력 창 열기"),
            Descriptor("live.go", "Live", "Go Live"),
            Descriptor("live.black", "Live", "검은 화면"),
        });

        return new CommandPaletteViewModel(catalog, id =>
        {
            captured.Add(id);
            return true;
        });
    }

    private static CommandDescriptor Descriptor(string id, string category, string displayName)
        => new(id, category, displayName, displayName + " 설명", IsDangerous: false, new List<Shortcut>());
}
