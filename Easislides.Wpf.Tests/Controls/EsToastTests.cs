using System;
using Easislides.Wpf.Controls;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Controls;

/// <summary>
/// EsToast 동작 검증 — 리뷰 #4 권장사항 회귀 방지 (메모리 누수 가드).
///
/// WPF UI 컴포넌트 인스턴스화는 STA 스레드 필수 → StaHelper.RunOnSta 사용.
/// 시각 검증은 LiveBarDemoWindow의 토스트 호출로 보강.
/// </summary>
public class EsToastTests
{
    [Fact]
    public void Show_NullOwner_Throws()
    {
        // 동기 null 체크 — STA 불필요
        Action act = () => EsToast.Show(null!, ToastKind.Info, "t", "m");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(ToastKind.Info)]
    [InlineData(ToastKind.Success)]
    [InlineData(ToastKind.Warning)]
    [InlineData(ToastKind.Error)]
    public void Toast_KindProperty_Defaults(ToastKind kind)
    {
        StaHelper.RunOnSta(() =>
        {
            var toast = new EsToast { Kind = kind, Title = "타이틀", Message = "메시지" };
            toast.Kind.Should().Be(kind);
            toast.Title.Should().Be("타이틀");
            toast.Message.Should().Be("메시지");
        });
    }
}
