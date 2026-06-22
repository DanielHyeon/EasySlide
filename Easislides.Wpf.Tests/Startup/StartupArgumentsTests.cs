using Easislides.Wpf.Startup;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Startup;

/// <summary>
/// 시작 인자 파서 검증 — 계획서 §9.4 / ADR-0007 (작업 B: --legacy-ui 안전망).
/// </summary>
public class StartupArgumentsTests
{
    [Fact]
    public void NoArgs_AllFlagsFalse()
    {
        var args = new StartupArguments(System.Array.Empty<string>());
        args.UseDemo.Should().BeFalse();
        args.UseLegacyUi.Should().BeFalse();
    }

    [Fact]
    public void Null_AllFlagsFalse()
    {
        var args = new StartupArguments(null);
        args.UseDemo.Should().BeFalse();
        args.UseLegacyUi.Should().BeFalse();
    }

    [Fact]
    public void LegacyUiFlag_Recognized()
    {
        new StartupArguments(new[] { "--legacy-ui" }).UseLegacyUi.Should().BeTrue();
    }

    [Fact]
    public void DemoFlag_Recognized()
    {
        new StartupArguments(new[] { "--demo" }).UseDemo.Should().BeTrue();
    }

    [Theory]
    [InlineData("--LEGACY-UI")]
    [InlineData("--Legacy-Ui")]
    public void LegacyUiFlag_IsCaseInsensitive(string flag)
    {
        new StartupArguments(new[] { flag }).UseLegacyUi.Should().BeTrue();
    }

    [Fact]
    public void Mixed_Args_SetBothFlags_AndIgnoreUnknown()
    {
        var args = new StartupArguments(new[] { "--demo", "/unknown", "--legacy-ui" });
        args.UseDemo.Should().BeTrue();
        args.UseLegacyUi.Should().BeTrue();
    }
}
