using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Easislides.Wpf.Startup;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Startup;

/// <summary>
/// legacy WinForms UI 롤백 런처 검증 — ADR-0007 (작업 B: --legacy-ui 안전망).
///
/// 실제 프로세스를 띄우지 않도록 파일 존재 체크·프로세스 시작을 seam(델리게이트)으로 주입한다.
/// 핵심 불변식: 시작 실패(반환 false 또는 throw)가 절대 예외로 전파되어 앱 시작을 깨면 안 된다.
/// </summary>
public class LegacyUiLauncherTests
{
    private static LegacyUiLauncher Build(
        bool exists,
        List<string> started,
        bool startSucceeds = true,
        string baseDir = @"C:\app")
        => new(baseDir, _ => exists, p => { started.Add(p); return startSucceeds; });

    [Fact]
    public void NotRequested_DoesNothing()
    {
        var started = new List<string>();
        Build(exists: true, started).LaunchIfRequested(false).Should().Be(LegacyUiLaunchOutcome.NotRequested);
        started.Should().BeEmpty("요청하지 않으면 프로세스를 시작하지 않아야 함");
    }

    [Fact]
    public void Requested_ButExecutableMissing_ReportsNotFound()
    {
        var started = new List<string>();
        Build(exists: false, started).LaunchIfRequested(true).Should().Be(LegacyUiLaunchOutcome.ExecutableNotFound);
        started.Should().BeEmpty("실행 파일이 없으면 시작하지 않아야 함");
    }

    [Fact]
    public void Requested_AndExecutableExists_LaunchesCorrectPath()
    {
        var started = new List<string>();
        Build(exists: true, started, baseDir: @"C:\app").LaunchIfRequested(true)
            .Should().Be(LegacyUiLaunchOutcome.Launched);
        started.Should().ContainSingle()
            .Which.Should().Be(Path.Combine(@"C:\app", LegacyUiLauncher.LegacyExecutableName));
    }

    [Fact]
    public void Requested_ButStartReturnsFalse_ReportsLaunchFailed()
    {
        var started = new List<string>();
        Build(exists: true, started, startSucceeds: false).LaunchIfRequested(true)
            .Should().Be(LegacyUiLaunchOutcome.LaunchFailed, "프로세스가 실제로 시작되지 않으면 Launched 가 아님");
    }

    [Fact]
    public void Requested_AndStartThrows_ReportsLaunchFailed_WithoutPropagating()
    {
        // 안전망 불변식: Process.Start throw(잠금/권한 등)가 앱 시작을 크래시시키면 안 된다.
        var sut = new LegacyUiLauncher(@"C:\app", _ => true,
            _ => throw new Win32Exception("access denied"));

        Action act = () => sut.LaunchIfRequested(true);
        act.Should().NotThrow("시작 실패는 예외로 전파되지 않고 결과로 흡수되어야 함");
        sut.LaunchIfRequested(true).Should().Be(LegacyUiLaunchOutcome.LaunchFailed);
    }

    [Fact]
    public void LegacyExecutablePath_Is_BaseDir_Plus_LegacyExeName()
    {
        Build(exists: true, new List<string>(), baseDir: @"C:\deploy")
            .LegacyExecutablePath.Should().Be(Path.Combine(@"C:\deploy", "Easislides.exe"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_RejectsEmptyBaseDirectory(string? baseDir)
    {
        Action act = () => new LegacyUiLauncher(baseDir!, _ => true, _ => true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_RejectsNullDelegates()
    {
        ((Action)(() => new LegacyUiLauncher(@"C:\app", null!, _ => true))).Should().Throw<ArgumentNullException>();
        ((Action)(() => new LegacyUiLauncher(@"C:\app", _ => true, null!))).Should().Throw<ArgumentNullException>();
    }
}
