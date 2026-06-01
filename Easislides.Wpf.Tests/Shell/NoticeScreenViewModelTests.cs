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
        var sut = new NoticeScreenViewModel(_ => true, () => { });

        sut.SendCommand.CanExecute(null).Should().BeFalse("빈 문구는 송출 불가");

        sut.Text = "예배 후 다과가 있습니다";
        sut.SendCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void Send_InvokesPublishWithText_AndReportsSuccess()
    {
        var published = new List<string>();
        var sut = new NoticeScreenViewModel(text => { published.Add(text); return true; }, () => { })
        {
            Text = "주차장 만차 안내",
        };

        sut.SendCommand.Execute(null);

        published.Should().ContainSingle().Which.Should().Be("주차장 만차 안내");
        sut.StatusText.Should().Contain("송출");
    }

    [Fact]
    public void Send_WhenPublishReturnsFalse_ShowsOutputClosedWarning()
    {
        var sut = new NoticeScreenViewModel(_ => false, () => { }) { Text = "공지" };

        sut.SendCommand.Execute(null);

        sut.StatusText.Should().Contain("출력 창이 열려 있지 않");
    }

    [Fact]
    public void ClearCommand_InvokesClearCallback()
    {
        var cleared = new List<bool>();
        var sut = new NoticeScreenViewModel(_ => true, () => cleared.Add(true));

        sut.ClearCommand.Execute(null);

        cleared.Should().ContainSingle();
        sut.StatusText.Should().Contain("내렸습니다");
    }
}
