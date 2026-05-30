using System;
using System.IO;
using System.Windows.Controls;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

/// <summary>
/// 출력 창 렌더 패리티(G1.4 / 계획서 §9.1) — 스크린샷 회귀 안전망을 실제 출력 표면으로 확대.
///
/// 출력 배경(솔리드 / 세로 그라데이션)은 텍스트가 없어 결정적이므로, 실제
/// <see cref="OutputWindowViewModel.SceneBackgroundBrush"/>(앱이 설정에서 만드는 그 브러시)를
/// 렌더해 기준 이미지로 고정한다. 토큰 색이나 그라데이션 방향이 의도치 않게 바뀌면 회귀로 잡힌다.
///
/// 가사·타이틀 텍스트는 폰트 힌팅/안티에일리어싱이 환경마다 달라 픽셀 비교에 부적합하므로
/// (screenshot-regression.md §6) 여기서 다루지 않는다 — 텍스트·레이아웃 패리티는
/// OutputWindowViewModelTests 의 속성 검증(브러시 색·표시 여부·콘텐츠 배치)이 담당한다.
/// </summary>
[Collection("WPF Application")]
public class OutputRenderParityTests
{
    private const int Width = 320;
    private const int Height = 180;

    [Fact]
    public void OutputBackground_VerticalGradient_MatchesBaseline()
    {
        StaHelper.RunOnSta(() =>
        {
            using var temp = TempSettings.Create();
            temp.Service.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFF1B2A4A));
            temp.Service.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, unchecked((int)0xFF0A1020));
            temp.Service.Set(EasiSettingKeys.LyricsMonitorBackgroundIsGradient, true);

            var png = RenderSceneBackground(temp.Service);
            // 그라데이션은 솔리드보다 래스터화 변동 여지가 있어 소폭의 안전 여유를 둔다
            // (실제 색·방향 회귀는 이보다 훨씬 큰 차이를 내므로 여전히 감지됨).
            ScreenshotBaseline.AssertMatches("output-bg-gradient", png, channelTolerance: 3, maxDifferingPercent: 0.5);
        });
    }

    [Fact]
    public void OutputBackground_Solid_MatchesBaseline()
    {
        StaHelper.RunOnSta(() =>
        {
            using var temp = TempSettings.Create();
            // 그라데이션 미설정(기본 false) → 솔리드 배경.
            temp.Service.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFF202830));

            var png = RenderSceneBackground(temp.Service);
            ScreenshotBaseline.AssertMatches("output-bg-solid", png);
        });
    }

    // 실제 출력 VM 이 설정에서 만든 SceneBackgroundBrush 로 채운 영역을 렌더한다.
    // VM/SettingsService 는 테스트마다 임시 폴더로 격리돼 곧 GC 대상이라 명시 해제하지 않는다
    // (sibling OutputWindowViewModelTests 와 동일 패턴 — 교차 테스트 누수 없음).
    private static byte[] RenderSceneBackground(SettingsService settings)
    {
        var vm = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings);
        // ApplySession 必須: 생성자는 LiveOutputRenderSettings.Default 로 배경 브러시를 만들고,
        // 주입한 설정(배경색/그라데이션)의 반영은 ApplySession→CreateScene→ApplyScene 경로에서만 일어난다.
        // 이 호출을 빼면 두 테스트가 기본 브러시를 검증하게 돼 의도가 조용히 무력화된다.
        vm.ApplySession(new LiveSessionSnapshot(LiveState.Active, "Parity", "Display 1", IsBlackout: false));

        var border = new Border { Background = vm.SceneBackgroundBrush };
        return VisualRenderHarness.RenderToPng(border, Width, Height);
    }

    // 디스크 영향 없이 SettingsService 를 격리(임시 폴더, Dispose 시 정리).
    private sealed class TempSettings : IDisposable
    {
        private readonly string _root;

        private TempSettings(string root, SettingsService service)
        {
            _root = root;
            Service = service;
        }

        public SettingsService Service { get; }

        public static TempSettings Create()
        {
            var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_OutputParity_{Guid.NewGuid():N}");
            var service = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups")));
            return new TempSettings(root, service);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
