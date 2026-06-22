using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Tests.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Accessibility;

/// <summary>
/// 라이브 영역(LiveRegion) 접근성 검증 — 계획서 §7.3 (D 접근성 2차 정밀화).
///
/// 라이브 상태가 동적으로 바뀌는 컨트롤(LiveBar, EsToast)은
/// 스크린리더가 "포커스가 없어도" 변화를 즉시 읽어주도록
/// AutomationProperties.LiveSetting = Assertive 를 노출해야 한다.
///
/// WPF에서 라이브 영역은 자동화 피어(AutomationPeer)의 GetLiveSetting()으로 노출되고,
/// 내용이 바뀔 때 LiveRegionChanged 자동화 이벤트를 올려야 실제로 읽힌다.
/// (계획서는 WinUI 용어 LiveRegion="Assertive"로 적혀 있으나 WPF 대응은 LiveSetting.Assertive)
///
/// WPF UI 컴포넌트 인스턴스화는 STA 스레드 필수 → StaHelper 사용.
/// 컨트롤 XAML이 EasiDS 토큰(StaticResource)을 참조하므로 "WPF Application" 픽스처 필요.
/// </summary>
[Collection("WPF Application")]
public class LiveRegionTests
{
    [Fact]
    public void EsToast_Peer_Reports_Assertive_LiveSetting()
    {
        StaHelper.RunOnSta(() =>
        {
            var toast = new EsToast { Kind = ToastKind.Success, Title = "저장됨", Message = "예배 리스트 저장 완료" };

            var peer = UIElementAutomationPeer.CreatePeerForElement(toast);

            peer.Should().NotBeNull("토스트는 자동화 피어를 노출해야 함");
            peer!.GetLiveSetting().Should().Be(AutomationLiveSetting.Assertive,
                "토스트는 포커스 없이도 즉시 읽히는 Assertive 라이브 영역이어야 함");
        });
    }

    [Fact]
    public void EsToast_Peer_Name_Includes_Title_And_Message()
    {
        StaHelper.RunOnSta(() =>
        {
            var toast = new EsToast { Kind = ToastKind.Error, Title = "오류", Message = "파일을 찾을 수 없습니다" };

            var peer = UIElementAutomationPeer.CreatePeerForElement(toast);

            // 스크린리더가 제목과 본문을 모두 읽어주도록 이름에 둘 다 포함되어야 함
            peer!.GetName().Should().Contain("오류").And.Contain("파일을 찾을 수 없습니다");
        });
    }

    [Fact]
    public void LiveBar_Peer_Reports_Assertive_LiveSetting()
    {
        StaHelper.RunOnSta(() =>
        {
            var bar = new LiveBar
            {
                ViewModel = new LiveBarViewModel
                {
                    State = LiveState.Active,
                    CurrentItemTitle = "주일찬양 #3 은혜로다",
                },
            };

            var peer = UIElementAutomationPeer.CreatePeerForElement(bar);

            peer.Should().NotBeNull("LiveBar는 자동화 피어를 노출해야 함");
            peer!.GetLiveSetting().Should().Be(AutomationLiveSetting.Assertive,
                "라이브 송출 상태 변화는 즉시 읽혀야 하므로 Assertive 라이브 영역이어야 함");
        });
    }

    [Fact]
    public void LiveBar_Peer_Name_Reflects_State_And_Current_Item()
    {
        StaHelper.RunOnSta(() =>
        {
            var bar = new LiveBar
            {
                ViewModel = new LiveBarViewModel
                {
                    State = LiveState.Active,
                    CurrentItemTitle = "주일찬양 #3 은혜로다",
                },
            };

            var peer = UIElementAutomationPeer.CreatePeerForElement(bar);

            // 상태 라벨(LIVE)과 현재 송출 아이템 제목이 이름으로 읽혀야 함
            peer!.GetName().Should().Contain("LIVE").And.Contain("주일찬양 #3 은혜로다");
        });
    }
}
