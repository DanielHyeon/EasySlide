using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class NoticeScreenViewModelTests
{
    // 임시 폴더 스토어로 명명 정보 화면 저장/불러오기/삭제를 격리 검증.
    private static (NoticeScreenViewModel Vm, InfoScreenStore Store, string Dir) CreateWithStore()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_ISVM_{Guid.NewGuid():N}");
        var store = new InfoScreenStore(dir);
        var vm = new NoticeScreenViewModel((_, _) => true, () => { }, store: store);
        return (vm, store, dir);
    }

    [Fact]
    public async Task SaveThenOpen_RoundTripsTextAndFontSize()
    {
        var (vm, _, dir) = CreateWithStore();
        try
        {
            vm.Text = "예배에 오신 것을 환영합니다";
            vm.FontSizePt = 60;
            vm.NewScreenName = "환영 인사";
            vm.SaveAsCommand.CanExecute(null).Should().BeTrue();

            await vm.SaveAsCommand.ExecuteAsync(null);
            vm.SavedScreens.Should().Contain("환영 인사");

            // 편집기를 바꾼 뒤 다시 불러오면 저장된 내용으로 복원.
            vm.Text = "다른 내용";
            vm.FontSizePt = 40;
            vm.SelectedScreen = "환영 인사";
            await vm.OpenCommand.ExecuteAsync(null);

            vm.Text.Should().Be("예배에 오신 것을 환영합니다");
            vm.FontSizePt.Should().Be(60);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void SaveAsCommand_DisabledWhenNameEmpty()
    {
        var (vm, _, dir) = CreateWithStore();
        try
        {
            vm.SaveAsCommand.CanExecute(null).Should().BeFalse("이름 없으면 저장 불가");
            vm.NewScreenName = "광고";
            vm.SaveAsCommand.CanExecute(null).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteCommand_RemovesScreenAndClearsSelection()
    {
        var (vm, _, dir) = CreateWithStore();
        try
        {
            vm.Text = "지울 공지";
            vm.NewScreenName = "임시";
            await vm.SaveAsCommand.ExecuteAsync(null);
            vm.SelectedScreen = "임시";

            vm.DeleteCommand.Execute(null);

            vm.SavedScreens.Should().NotContain("임시");
            vm.SelectedScreen.Should().BeNull("삭제 후 선택 해제");
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void SendCommand_DisabledWhenTextEmpty()
    {
        var sut = new NoticeScreenViewModel((_, _) => true, () => { });

        sut.SendCommand.CanExecute(null).Should().BeFalse("빈 문구는 송출 불가");

        sut.Text = "예배 후 다과가 있습니다";
        sut.SendCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void InitialText_PrefillsEditor_AndEnablesSend()
    {
        // 성경 "공지 화면으로 복사"처럼 초기 텍스트를 주고 열면, 편집기가 채워지고 바로 송출 가능.
        var sut = new NoticeScreenViewModel((_, _) => true, () => { }, initialText: "창세기 1:1 태초에...");

        sut.Text.Should().Be("창세기 1:1 태초에...");
        sut.SendCommand.CanExecute(null).Should().BeTrue("초기 텍스트가 있으면 송출 가능");
    }

    [Fact]
    public void InitialText_NullOrOmitted_StartsEmpty()
    {
        new NoticeScreenViewModel((_, _) => true, () => { }).Text.Should().BeEmpty();
        new NoticeScreenViewModel((_, _) => true, () => { }, initialText: null).Text.Should().BeEmpty();
    }

    [Theory]
    [InlineData("창 1:1 선택", "본문 전체", "창 1:1 선택")]  // 선택이 있으면 선택 우선
    [InlineData("  ", "본문 전체", "본문 전체")]              // 선택이 공백뿐이면 본문 전체
    [InlineData("", "  은혜  ", "은혜")]                       // 선택 없음 → 본문(양끝 공백 다듬음)
    [InlineData("", "", null)]                                 // 둘 다 비면 null(복사할 게 없음)
    [InlineData(null, null, null)]
    public void ResolveCopyText_PicksSelectionElseFull_TrimsAndNullsEmpty(string? selected, string? full, string? expected)
        => NoticeScreenViewModel.ResolveCopyText(selected, full).Should().Be(expected);

    [Fact]
    public void Send_InvokesPublishWithTextAndFontSize_AndReportsSuccess()
    {
        var published = new List<(string Text, int FontSize)>();
        var sut = new NoticeScreenViewModel((text, size) => { published.Add((text, size)); return true; }, () => { })
        {
            Text = "주차장 만차 안내",
            FontSizePt = 60,
        };

        sut.SendCommand.Execute(null);

        published.Should().ContainSingle();
        published[0].Text.Should().Be("주차장 만차 안내");
        published[0].FontSize.Should().Be(60);
        sut.StatusText.Should().Contain("송출");
    }

    [Fact]
    public void FontSizePt_DefaultsToNormal_AndPresetsExposed()
    {
        var sut = new NoticeScreenViewModel((_, _) => true, () => { });

        sut.FontSizePt.Should().Be(40);
        sut.FontSizePresets.Should().Equal(40, 60, 80);
    }

    [Fact]
    public void EveryFontPreset_SurvivesFormatDataDecode()
    {
        // 회귀 가드: 모든 글자 크기 프리셋이 FormatData(47=pt) 디코더 범위(6~100pt) 안에 있어야 한다.
        // 범위 밖 프리셋을 실수로 추가하면 디코더가 조용히 null 로 떨어뜨려 크기 적용이 무시되므로 여기서 막는다.
        var sut = new NoticeScreenViewModel((_, _) => true, () => { });

        foreach (var pt in sut.FontSizePresets)
        {
            Easislides.Wpf.Library.SongFormatData.Parse($"47={pt}>")!.FontSize1
                .Should().Be(pt, $"프리셋 {pt}pt 는 디코더 범위(6~100) 안이어야 함");
        }
    }

    [Fact]
    public void Send_WhenPublishReturnsFalse_ShowsOutputClosedWarning()
    {
        var sut = new NoticeScreenViewModel((_, _) => false, () => { }) { Text = "공지" };

        sut.SendCommand.Execute(null);

        sut.StatusText.Should().Contain("출력 창이 열려 있지 않");
    }

    [Fact]
    public void ClearCommand_InvokesClearCallback()
    {
        var cleared = new List<bool>();
        var sut = new NoticeScreenViewModel((_, _) => true, () => cleared.Add(true));

        sut.ClearCommand.Execute(null);

        cleared.Should().ContainSingle();
        sut.StatusText.Should().Contain("내렸습니다");
    }
}
