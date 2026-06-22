using System;
using System.IO;
using Easislides.Wpf.Tests.Accessibility;
using Xunit.Sdk;

namespace Easislides.Wpf.Tests.Rendering;

/// <summary>
/// 스크린샷 회귀 기준 이미지 저장·비교 — 계획서 §9.1 (작업 C).
///
/// 승인 테스트(approval test) 방식:
///  - 기준 이미지가 없으면, 환경변수 EASISLIDES_APPROVE_BASELINES 가 설정된 경우에만
///    현재 렌더를 기준으로 "생성"하고 실패시킨다(사람이 검토 후 커밋 → 다음 실행부터 비교).
///    CI 등에서는 이 변수를 켜지 않아, 기준 누락이 "조용한 생성 후 통과"로 위장되지 않는다.
///  - 기준이 있으면 허용오차 비교. 불일치 시 실제 이미지를 *.actual.png 로 남겨 진단을 돕는다.
///
/// 경로 정책:
///  - 기준 이미지: 저장소 소스 트리(커밋 대상) Easislides.Wpf.Tests/Rendering/baselines/.
///  - 진단 산출물(*.actual.png): 테스트 출력 폴더(AppContext.BaseDirectory) — 소스 트리 오염·병렬 경합 방지.
/// </summary>
internal static class ScreenshotBaseline
{
    private const string ApproveEnvVar = "EASISLIDES_APPROVE_BASELINES";

    /// <summary>기준 이미지(커밋 대상) 디렉터리 — 소스 트리.</summary>
    public static string BaselineDir()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return Path.Combine(repoRoot, "Easislides.Wpf.Tests", "Rendering", "baselines");
    }

    /// <summary>진단 산출물 디렉터리 — 테스트 출력 폴더(소스 트리 밖).</summary>
    private static string DiagnosticsDir()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "screenshot-actuals");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>현재 렌더(PNG)를 같은 이름의 기준 이미지와 비교(허용오차)하고, 불일치면 실패시킨다.</summary>
    public static void AssertMatches(
        string name,
        byte[] actualPng,
        int channelTolerance = 2,
        double maxDifferingPercent = 0.0)
    {
        var baselineDir = BaselineDir();
        var baselinePath = Path.Combine(baselineDir, name + ".png");

        if (!File.Exists(baselinePath))
        {
            HandleMissingBaseline(name, baselinePath, baselineDir, actualPng);
            return; // (도달하지 않음 — HandleMissingBaseline 은 항상 throw)
        }

        var expected = File.ReadAllBytes(baselinePath);
        var result = ImageComparer.Compare(expected, actualPng, channelTolerance, maxDifferingPercent);

        if (!result.IsMatch)
        {
            var actualPath = Path.Combine(DiagnosticsDir(), name + ".actual.png");
            File.WriteAllBytes(actualPath, actualPng);

            var detail = result.DimensionsMatch
                ? $"다른 픽셀 {result.DifferingPixels}/{result.TotalPixels} ({result.DifferingPercent:F3}%), 최대 채널 차 {result.MaxChannelDelta}"
                : $"치수 불일치 — 기준 {result.ExpectedWidth}x{result.ExpectedHeight}, 실제 {result.ActualWidth}x{result.ActualHeight}";

            throw new XunitException(
                $"스크린샷 회귀 감지 — '{name}'\n  {detail}\n  실제 이미지: {actualPath}\n" +
                "의도된 변경이면 기준 이미지를 갱신하세요.");
        }
    }

    private static void HandleMissingBaseline(string name, string baselinePath, string baselineDir, byte[] actualPng)
    {
        var approve = Environment.GetEnvironmentVariable(ApproveEnvVar);
        var approveEnabled = !string.IsNullOrEmpty(approve)
            && (approve == "1" || string.Equals(approve, "true", StringComparison.OrdinalIgnoreCase));

        if (approveEnabled)
        {
            Directory.CreateDirectory(baselineDir);
            File.WriteAllBytes(baselinePath, actualPng);
            throw new XunitException(
                $"기준 이미지가 없어 새로 생성했습니다(승인 모드):\n  {baselinePath}\n" +
                "내용을 검토 후 커밋하고 테스트를 다시 실행하세요.");
        }

        // CI 등 비승인 환경: 조용한 생성 금지 — 명확히 실패시킨다.
        var actualPath = Path.Combine(DiagnosticsDir(), name + ".actual.png");
        File.WriteAllBytes(actualPath, actualPng);
        throw new XunitException(
            $"기준 이미지가 없습니다: {baselinePath}\n" +
            $"  현재 렌더는 {actualPath} 에 남겼습니다.\n" +
            $"  로컬에서 기준을 생성하려면 환경변수 {ApproveEnvVar}=1 로 다시 실행하세요.");
    }
}
