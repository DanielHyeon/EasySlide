using System.Windows.Media;
using Easislides.Wpf;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class OutputWindowClipGuardTests
{
    [Fact]
    public void ShouldClearClip_OnlyWhenActiveClipIsMyGeometry()
    {
        // 빠른 장면 전환 경쟁 가드: 전환 완료 시 "내가 건 도형"이 아직 활성일 때만 클립을 지운다.
        var mine = new RectangleGeometry();
        var other = new RectangleGeometry();

        OutputWindow.ShouldClearClip(mine, mine).Should().BeTrue("내 클립이 활성이면 제거");
        OutputWindow.ShouldClearClip(other, mine).Should().BeFalse("다음 장면이 새 클립을 걸었으면 건드리지 않음");
        OutputWindow.ShouldClearClip(null, mine).Should().BeFalse("이미 클립이 없으면(다른 경로가 지움) 건드리지 않음");
    }
}
