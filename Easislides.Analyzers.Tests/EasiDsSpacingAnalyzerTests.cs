using Easislides.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = Easislides.Analyzers.Tests.AnalyzerVerifier<Easislides.Analyzers.EasiDsSpacingAnalyzer>;

namespace Easislides.Analyzers.Tests;

/// <summary>
/// EasiDS003 분석기 명세 — 계획서 §4.3/§9.2.
/// Margin/Padding 에 4-base 스케일 밖 매직 값을 직접 쓰면 경고.
/// 스니펫은 WPF 참조 없이 컴파일되도록 Thickness/Control 스텁을 직접 선언한다.
/// </summary>
public class EasiDsSpacingAnalyzerTests
{
    private const string Stubs = @"
namespace System.Windows
{
    public struct Thickness
    {
        public Thickness(double uniform) { }
        public Thickness(double left, double top, double right, double bottom) { }
    }
}
namespace System.Windows.Controls
{
    using System.Windows;
    public class Control
    {
        public Thickness Margin { get; set; }
        public Thickness Padding { get; set; }
        public Thickness BorderThickness { get; set; }
    }
}
";

    // ── 위반(diagnostic 발생해야 함) ─────────────────────────────

    [Fact]
    public async Task Flags_Margin_With_Offscale_Values()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M() { Margin = {|EasiDS003:new Thickness(3, 5, 3, 5)|}; }
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_Padding_With_Offscale_Uniform_Value()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M() { Padding = {|EasiDS003:new Thickness(3)|}; }
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_Margin_In_Object_Initializer()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C
{
    Control M() => new Control { Margin = {|EasiDS003:new Thickness(10)|} };
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    // ── 정상(diagnostic 없어야 함) ───────────────────────────────

    [Fact]
    public async Task Allows_Margin_On_Scale()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M() { Margin = new Thickness(0, 4, 8, 16); }
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Allows_Padding_Uniform_On_Scale()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M() { Padding = new Thickness(12); }
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Allows_BorderThickness_Not_A_Spacing_Property()
    {
        // 1px 보더는 간격이 아니므로 대상 외 — 오탐 방지.
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M() { BorderThickness = new Thickness(1); }
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Allows_Margin_With_Dynamic_Value()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M(double gap) { Margin = new Thickness(gap); }
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    // ── 진단 메타데이터 ──────────────────────────────────────────

    [Fact]
    public async Task Diagnostic_Default_Severity_Is_Warning()
    {
        var source = @"
using System.Windows;
using System.Windows.Controls;
class C : Control
{
    void M() { Padding = new Thickness(3); }
}
" + Stubs;

        var diagnostics = await AnalyzerVerifier<EasiDsSpacingAnalyzer>.AnalyzeAsync(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == EasiDsSpacingAnalyzer.DiagnosticId);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }
}
