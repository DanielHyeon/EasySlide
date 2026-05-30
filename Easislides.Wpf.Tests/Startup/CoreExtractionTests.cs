using Easislides.Module;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Startup;

/// <summary>
/// 공통 도메인 추출 검증 — §9.4 / core-extraction-plan.md (작업 B-3 Phase 1).
///
/// CommonEnum 의 도메인 enum 들이 공통 어셈블리 Easislides.Core 로 이동했는지,
/// 그리고 네임스페이스는 Easislides.Module 로 유지되어 사용처 코드가 무수정 컴파일되는지 검증.
/// (네임스페이스를 바꾸지 않는 것이 Phase 1 의 최저위험 핵심 — 이 테스트로 회귀를 고정한다.)
/// </summary>
public class CoreExtractionTests
{
    [Fact]
    public void CommonEnums_Live_In_Easislides_Core_Assembly()
    {
        typeof(SettingsCategory).Assembly.GetName().Name.Should().Be("Easislides.Core",
            "CommonEnum 도메인 enum 은 공통 어셈블리 Easislides.Core 에 있어야 함 (§9.4)");
        typeof(DatabaseType).Assembly.GetName().Name.Should().Be("Easislides.Core");
        typeof(ItemSource).Assembly.GetName().Name.Should().Be("Easislides.Core");
    }

    [Fact]
    public void Domain_Enums_Keep_Easislides_Module_Namespace()
    {
        // 네임스페이스 유지 = legacy 사용처 무수정 컴파일(Phase 1 안전 장치).
        typeof(SettingsCategory).Namespace.Should().Be("Easislides.Module");
        typeof(MediaBackgroundStyle).Namespace.Should().Be("Easislides.Module");
    }

    [Fact]
    public void SongFormat_Lives_In_Easislides_Core_Assembly()
    {
        // Phase 2: SongFormat 표시형식 도메인 모델도 공통 어셈블리 Easislides.Core 로 이동(§9.4).
        typeof(SongFormat).Assembly.GetName().Name.Should().Be("Easislides.Core",
            "SongFormat 은 Phase 2 에서 Easislides.Core 로 이동해야 함 (§9.4)");
        typeof(SongFormat).Namespace.Should().Be("Easislides.Module",
            "네임스페이스 유지 = legacy 사용처 무수정 컴파일");
    }

    [Fact]
    public void SongFormat_Colours_Are_Decoupled_From_System_Drawing()
    {
        // 옵션 B: 색상은 System.Drawing.Color 가 아니라 ARGB int 로 보관 → Core 포터블 유지.
        // (Color.ToArgb()/FromArgb(int) 로 경계에서 변환. 영속 데이터(헤더 정수)와도 호환.)
        typeof(SongFormat).GetField(nameof(SongFormat.ShowScreenColour))!.FieldType
            .Should().Be(typeof(int[]), "배경색은 ARGB int[] 로 디커플");
        typeof(SongFormat).GetField(nameof(SongFormat.ShowFontColour))!.FieldType
            .Should().Be(typeof(int[]), "글자색은 ARGB int[] 로 디커플");
    }

    [Fact]
    public void Core_Does_Not_Reference_WindowsForms_Or_Drawing()
    {
        // Core 는 포터블 도메인이어야 한다(net10.0). WinForms/System.Drawing 결합이 새어들면
        // 두 앱 공유·포터블성이 깨지므로, Core 어셈블리가 그것들을 참조하지 않음을 고정한다.
        // (향후 Core 에 그런 결합 타입이 추가되면 이 테스트가 즉시 잡는다.)
        var referenced = typeof(SettingsCategory).Assembly.GetReferencedAssemblies();
        referenced.Should().NotContain(a => a.Name == "System.Windows.Forms",
            "Core 는 WinForms 에 의존하면 안 됨");
        referenced.Should().NotContain(a => a.Name != null && a.Name.StartsWith("System.Drawing"),
            "Core 는 System.Drawing 에 의존하면 안 됨(포터블 도메인 유지)");
    }
}
