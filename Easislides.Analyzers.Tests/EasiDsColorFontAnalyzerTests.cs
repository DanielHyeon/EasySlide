using Easislides.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = Easislides.Analyzers.Tests.AnalyzerVerifier<Easislides.Analyzers.EasiDsColorFontAnalyzer>;

namespace Easislides.Analyzers.Tests;

/// <summary>
/// EasiDS001 분석기 명세 — 계획서 §4.1/§4.2/§9.2.
/// 스니펫은 WPF/Drawing 참조 없이 컴파일되도록 색·폰트 타입을 스텁으로 직접 선언한다.
/// (스텁 namespace 블록은 using 뒤에 와야 하므로 항상 소스 '끝'에 붙인다.)
/// </summary>
public class EasiDsColorFontAnalyzerTests
{
    // 스니펫 끝에 붙는 색·폰트 스텁 타입 (실제 WPF/Drawing 모양을 흉내).
    private const string Stubs = @"
namespace System.Windows.Media
{
    public struct Color
    {
        public static Color FromArgb(byte a, byte r, byte g, byte b) => default;
        public static Color FromRgb(byte r, byte g, byte b) => default;
    }
    public static class Colors
    {
        public static Color Navy => default;
        public static Color Red => default;
    }
    public sealed class SolidColorBrush
    {
        public SolidColorBrush(Color color) { }
    }
    public sealed class FontFamily
    {
        public FontFamily(string name) { }
    }
}
namespace System.Drawing
{
    public struct Color
    {
        public static Color Navy => default;
        public static Color FromArgb(int r, int g, int b) => default;
    }
    public static class SystemColors
    {
        public static Color Control => default;
    }
    public sealed class Font
    {
        public Font(string family, float size) { }
    }
}
namespace EasiDsDomain
{
    // 디자인 시스템이 정의할 법한 도메인 'Font' 토큰 타입 — 절대 차단되면 안 됨.
    public sealed class Font
    {
        public Font(string token, float scale) { }
    }
}
";

    // ── 위반(diagnostic 발생해야 함) ─────────────────────────────

    [Fact]
    public async Task Flags_ColorFromArgb_With_Constant_Arguments()
    {
        var source = @"
using System.Windows.Media;
class C
{
    Color M() => {|EasiDS001:Color.FromArgb(255, 20, 20, 20)|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_ColorFromRgb_With_Constant_Arguments()
    {
        var source = @"
using System.Windows.Media;
class C
{
    Color M() => {|EasiDS001:Color.FromRgb(23, 23, 23)|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_Wpf_Named_Color()
    {
        var source = @"
using System.Windows.Media;
class C
{
    Color M() => {|EasiDS001:Colors.Navy|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_Drawing_Named_Color()
    {
        var source = @"
class C
{
    System.Drawing.Color M() => {|EasiDS001:System.Drawing.Color.Navy|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_Drawing_SystemColors_Member()
    {
        // SystemColors.Control 같은 레거시 시스템 색도 매직 색으로 차단 (계획서 §4.1 취지).
        var source = @"
class C
{
    System.Drawing.Color M() => {|EasiDS001:System.Drawing.SystemColors.Control|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_FontFamily_With_Literal_Name()
    {
        var source = @"
using System.Windows.Media;
class C
{
    FontFamily M() => {|EasiDS001:new FontFamily(""Microsoft Sans Serif"")|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Flags_Drawing_Font_Construction()
    {
        var source = @"
class C
{
    System.Drawing.Font M() => {|EasiDS001:new System.Drawing.Font(""Tahoma"", 8.25f)|};
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    // ── 정상(diagnostic 없어야 함) ───────────────────────────────

    [Fact]
    public async Task Allows_ColorFromArgb_With_Dynamic_Arguments()
    {
        // OutputWindowViewModel.CreateBrush 처럼 런타임 값으로 만든 색은 허용.
        var source = @"
using System.Windows.Media;
class C
{
    Color M(uint value) => Color.FromArgb(
        (byte)(value >> 24),
        (byte)(value >> 16),
        (byte)(value >> 8),
        (byte)value);
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Allows_FontFamily_From_Pack_Uri_Resource()
    {
        // Pretendard 번들 폰트 등록(pack:// 리소스)은 토큰 정의 자체이므로 허용.
        var source = @"
using System.Windows.Media;
class C
{
    FontFamily M() => new FontFamily(""pack://application:,,,/Assets/Fonts/#Pretendard Variable"");
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Allows_Code_Without_Magic_Color_Or_Font()
    {
        var source = @"
class C
{
    int M() => 42;
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    [Fact]
    public async Task Allows_Domain_Font_Token_Type_Outside_Drawing()
    {
        // System.Drawing 밖의 도메인 'Font' 타입 생성은 차단하지 않는다 (오탐 방지).
        var source = @"
using EasiDsDomain;
class C
{
    Font M() => new Font(""Type.Body"", 1.0f);
}
" + Stubs;
        await Verify.VerifyAsync(source);
    }

    // ── 진단 메타데이터 ──────────────────────────────────────────

    [Fact]
    public async Task Diagnostic_Default_Severity_Is_Warning()
    {
        // 계획서 §9.2: 우선 경고로 시작(추후 에러 승격). 회귀로 severity가 바뀌면 잡아낸다.
        var source = @"
using System.Windows.Media;
class C
{
    Color M() => Color.FromArgb(255, 20, 20, 20);
}
" + Stubs;

        var diagnostics = await AnalyzerVerifier<EasiDsColorFontAnalyzer>.AnalyzeAsync(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == EasiDsColorFontAnalyzer.DiagnosticId);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }
}
