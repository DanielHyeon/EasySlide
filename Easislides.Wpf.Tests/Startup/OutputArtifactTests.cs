using Easislides.Wpf;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Startup;

/// <summary>
/// 산출물 분리 검증 — 계획서 §9.4 / ADR-0007 (작업 B: 신규 빌드 = EasislidesNext.exe).
///
/// legacy WinForms 빌드(Easislides.exe)와 구분되도록 신규 WPF 빌드의 어셈블리(=실행 파일)
/// 이름이 EasislidesNext 여야 한다. (RootNamespace 는 Easislides.Wpf 로 유지 — 코드 네임스페이스 불변.)
/// </summary>
public class OutputArtifactTests
{
    [Fact]
    public void Wpf_Assembly_Is_Named_EasislidesNext()
    {
        typeof(App).Assembly.GetName().Name.Should().Be("EasislidesNext",
            "신규 WPF 산출물은 legacy Easislides.exe 와 구분되는 EasislidesNext.exe 여야 함 (§9.4)");
    }

    [Fact]
    public void Wpf_Namespace_Stays_EasislidesWpf()
    {
        // AssemblyName 변경이 코드 네임스페이스까지 바꾸지 않았는지 확인.
        typeof(App).Namespace.Should().Be("Easislides.Wpf");
    }
}
