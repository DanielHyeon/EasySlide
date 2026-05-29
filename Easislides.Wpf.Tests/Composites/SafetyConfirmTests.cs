using System;
using System.Threading.Tasks;
using Easislides.Wpf.Composites;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// SafetyConfirm 동작 검증 — 리뷰 #1 회귀 방지 (Esc 동작 + 타임아웃 정확성).
/// UI 인스턴스화는 StaHelper로 격리. 풀 시나리오(외부 클릭/Esc) 검증은 LiveBarDemoWindow 시각 데모로 보강.
/// </summary>
public class SafetyConfirmTests
{
    [Fact]
    public void AskAsync_NullAnchor_Throws()
    {
        // 동기 throw — 진입 즉시 ArgumentNullException
        Action act = () => _ = SafetyConfirm.AskAsync(null!, "test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AskAsync_TimeoutReached_ReturnsFalse()
    {
        // STA 스레드 + Dispatcher 펌프에서 비동기 흐름 검증
        await StaHelper.RunOnStaAsync(async () =>
        {
            var anchor = new System.Windows.Controls.Button();
            var task = SafetyConfirm.AskAsync(anchor, "타임아웃 테스트", null, TimeSpan.FromMilliseconds(300));
            var result = await task.WaitAsync(TimeSpan.FromSeconds(2));
            result.Should().BeFalse("타임아웃 시 false를 반환해야 함 (실수 방지)");
        });
    }
}
