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
    public void Send_PassesBackgroundColor_InNoticeOptions()
    {
        // 공지 배경색(코드26)이 송출 옵션에 실려 출력으로 전달되는지 — 콜백이 받은 NoticeOptions 를 잡아 확인.
        NoticeOptions? captured = null;
        var vm = new NoticeScreenViewModel((_, opts) => { captured = opts; return true; }, () => { });
        vm.Text = "광고";
        vm.ColorArgb = unchecked((int)0xFFFFFFFF);
        vm.BackgroundColorArgb = unchecked((int)0xFF000000); // 검정 배경.

        vm.SendCommand.Execute(null);

        captured.Should().NotBeNull();
        captured!.BackgroundColorArgb.Should().Be(unchecked((int)0xFF000000), "배경색이 송출 옵션에 실림");
        captured.ColorArgb.Should().Be(unchecked((int)0xFFFFFFFF), "글자색도 함께");
    }

    [Fact]
    public async Task SaveThenOpen_RoundTripsBackgroundColor()
    {
        var (vm, _, dir) = CreateWithStore();
        try
        {
            vm.Text = "광고";
            vm.BackgroundColorArgb = unchecked((int)0xFF202020);
            vm.NewScreenName = "주보 광고";
            await vm.SaveAsCommand.ExecuteAsync(null);

            vm.BackgroundColorArgb = 0; // 편집기 변경 후 불러오면 복원.
            vm.SelectedScreen = "주보 광고";
            await vm.OpenCommand.ExecuteAsync(null);

            vm.BackgroundColorArgb.Should().Be(unchecked((int)0xFF202020), "배경색도 저장/복원");
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Send_PassesEmphasis_InNoticeOptions()
    {
        // 공지 강조(굵게·기울임·밑줄, 코드41)가 송출 옵션에 실려 출력으로 전달되는지 — 콜백이 받은 NoticeOptions 확인.
        NoticeOptions? captured = null;
        var vm = new NoticeScreenViewModel((_, opts) => { captured = opts; return true; }, () => { });
        vm.Text = "중요 공지";
        vm.Bold = true;
        vm.Underline = true; // 기울임은 일부러 끔 → 비트 조합도 정확히 실리는지.

        vm.SendCommand.Execute(null);

        captured.Should().NotBeNull();
        captured!.Bold.Should().BeTrue("굵게가 옵션에 실림");
        captured.Italic.Should().BeFalse("끈 기울임은 false 그대로");
        captured.Underline.Should().BeTrue("밑줄이 옵션에 실림");
    }

    [Fact]
    public async Task SaveThenOpen_RoundTripsEmphasis()
    {
        var (vm, _, dir) = CreateWithStore();
        try
        {
            vm.Text = "굵게 기울임 공지";
            vm.Bold = true;
            vm.Italic = true;
            vm.NewScreenName = "강조테스트";
            await vm.SaveAsCommand.ExecuteAsync(null);

            vm.Bold = false; // 편집기 변경 후 불러오면 복원.
            vm.Italic = false;
            vm.SelectedScreen = "강조테스트";
            await vm.OpenCommand.ExecuteAsync(null);

            vm.Bold.Should().BeTrue("저장된 굵게 복원");
            vm.Italic.Should().BeTrue("저장된 기울임 복원");
            vm.Underline.Should().BeFalse("켜지 않은 밑줄은 false");
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
    public void Send_InvokesPublishWithTextAndOptions_AndReportsSuccess()
    {
        var published = new List<(string Text, NoticeOptions Options)>();
        var sut = new NoticeScreenViewModel((text, options) => { published.Add((text, options)); return true; }, () => { })
        {
            Text = "주차장 만차 안내",
            FontSizePt = 60,
            Alignment = 1, // 왼쪽
            ColorArgb = unchecked((int)0xFFFFE066), // 노랑
        };

        sut.SendCommand.Execute(null);

        published.Should().ContainSingle();
        published[0].Text.Should().Be("주차장 만차 안내");
        published[0].Options.FontSizePt.Should().Be(60);
        published[0].Options.Alignment.Should().Be(1, "정렬도 옵션에 전달");
        published[0].Options.ColorArgb.Should().Be(unchecked((int)0xFFFFE066), "색도 옵션에 전달");
        sut.StatusText.Should().Contain("송출");
    }

    [Theory]
    [InlineData(0, 0, 0, null)]                          // 모두 미지정 → FormatData 없음
    [InlineData(60, 0, 0, "47=60>")]                     // 크기만
    [InlineData(0, 1, 0, "31=1>")]                       // 정렬만(왼쪽)
    [InlineData(60, 2, 0, "47=60>31=2>")]                // 크기+정렬(가운데)
    [InlineData(40, 9, 0, "47=40>")]                     // 정렬 범위 밖(9)은 무시 → 크기만
    [InlineData(0, 0, -1, "29=-1>")]                     // 색만(흰색 0xFFFFFFFF = -1)
    [InlineData(60, 2, -1, "47=60>31=2>29=-1>")]         // 크기+정렬+색
    public void BuildNoticeFormatData_ComposesSizeAlignmentColor(int size, int align, int color, string? expected)
        => MainViewModel.BuildNoticeFormatData(new NoticeOptions(size, align, color)).Should().Be(expected);

    [Fact]
    public async Task SaveThenOpen_RoundTripsAlignmentAndColor()
    {
        var (vm, _, dir) = CreateWithStore();
        try
        {
            vm.Text = "왼쪽 정렬 노랑 공지";
            vm.Alignment = 1;
            vm.ColorArgb = unchecked((int)0xFFFFE066);
            vm.NewScreenName = "서식테스트";
            await vm.SaveAsCommand.ExecuteAsync(null);

            vm.Alignment = 0; // 바꿔 둠
            vm.ColorArgb = 0;
            vm.SelectedScreen = "서식테스트";
            await vm.OpenCommand.ExecuteAsync(null);

            vm.Alignment.Should().Be(1, "저장된 정렬 복원");
            vm.ColorArgb.Should().Be(unchecked((int)0xFFFFE066), "저장된 색 복원");
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
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
    public void AddToQueueCommand_InvokesCallback_WithCurrentText()
    {
        // "순서에 추가" — 현재 텍스트를 예배 순서 추가 콜백으로 넘긴다.
        var added = new List<string>();
        var sut = new NoticeScreenViewModel(
            (_, _) => true, () => { }, addToWorshipQueue: text => { added.Add(text); return true; })
        {
            Text = "주차 안내",
        };

        sut.AddToQueueCommand.CanExecute(null).Should().BeTrue("콜백 있고 텍스트 있으면 활성");
        sut.AddToQueueCommand.Execute(null);

        added.Should().ContainSingle().Which.Should().Be("주차 안내");
        sut.StatusText.Should().Contain("예배 순서");
    }

    [Fact]
    public void AddToQueueCommand_DisabledWhenNoCallback()
    {
        // 콜백을 주입하지 않으면(즉시 송출 전용으로 열린 경우) "순서에 추가"는 비활성.
        var sut = new NoticeScreenViewModel((_, _) => true, () => { }) { Text = "공지" };

        sut.AddToQueueCommand.CanExecute(null).Should().BeFalse("콜백 없으면 비활성");
    }

    [Fact]
    public void AddToQueueCommand_DisabledWhenTextEmpty()
    {
        var sut = new NoticeScreenViewModel((_, _) => true, () => { }, addToWorshipQueue: _ => true);

        sut.AddToQueueCommand.CanExecute(null).Should().BeFalse("빈 텍스트는 추가 불가");
        sut.Text = "안내";
        sut.AddToQueueCommand.CanExecute(null).Should().BeTrue("텍스트가 생기면 활성");
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
