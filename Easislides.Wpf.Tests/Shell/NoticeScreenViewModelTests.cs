using System.Collections.Generic;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class NoticeScreenViewModelTests
{
    [Fact]
    public void SendCommand_DisabledWhenTextEmpty()
    {
        var sut = new NoticeScreenViewModel((_, _) => true, () => { });

        sut.SendCommand.CanExecute(null).Should().BeFalse("빈 문구는 송출 불가");

        sut.Text = "예배 후 다과가 있습니다";
        sut.SendCommand.CanExecute(null).Should().BeTrue();
    }

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
