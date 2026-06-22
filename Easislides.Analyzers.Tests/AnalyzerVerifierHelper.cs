using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Easislides.Analyzers.Tests;

/// <summary>
/// 경량 분석기 검증기.
/// roslyn-sdk 테스트 하니스(xUnit 버전 충돌)를 쓰지 않고, 직접 컴파일 후 분석기를 돌려 진단을 비교한다.
///
/// 소스에는 `{|EasiDS001:코드|}` 형태의 마크업으로 "여기서 이 진단이 나와야 한다"를 표시한다.
/// 마크업이 하나도 없으면 "진단이 전혀 없어야 한다"는 뜻이다.
/// </summary>
internal static class AnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    // 모든 프레임워크 어셈블리 참조는 한 번만 만들어 캐싱한다 (테스트마다 150+ DLL 재로딩 방지).
    private static readonly Lazy<IReadOnlyList<MetadataReference>> CachedReferences =
        new(BuildRuntimeReferences);

    /// <summary>소스를 컴파일하고 분석기를 돌려 진단 목록을 돌려준다. (마크업 없는 순수 소스)</summary>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "EasiDsAnalyzerTestAsm",
            syntaxTrees: new[] { tree },
            references: CachedReferences.Value,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // 스니펫 자체가 (분석기와 무관하게) 정상 컴파일되는지 먼저 보장 — 깨진 스니펫이 오탐을 가리지 않도록.
        var compileErrors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.True(
            compileErrors.Count == 0,
            "테스트 스니펫이 컴파일되지 않습니다:\n" + string.Join("\n", compileErrors));

        var withAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer()));
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();

        // AD0001 = 분석기가 실행 중 예외로 죽었다는 신호. 조용히 통과하지 않도록 명시적으로 실패시킨다.
        Assert.DoesNotContain(diagnostics, d => d.Id == "AD0001");

        return diagnostics;
    }

    /// <summary>마크업(`{|EasiDS001:...|}`)이 포함된 소스를 검증한다.</summary>
    public static async Task VerifyAsync(string markup)
    {
        var (source, expected) = MarkupParser.Parse(markup);
        var diagnostics = await AnalyzeAsync(source);

        var actual = diagnostics
            .Select(d => (d.Id, d.Location.SourceSpan))
            .OrderBy(d => d.SourceSpan.Start)
            .ToList();

        var expectedOrdered = expected
            .OrderBy(e => e.Span.Start)
            .Select(e => (e.Id, e.Span))
            .ToList();

        Assert.Equal(expectedOrdered, actual);
    }

    /// <summary>현재 런타임의 모든 프레임워크 어셈블리를 메타데이터 참조로 모은다 (BCL 전부 사용 가능).</summary>
    private static IReadOnlyList<MetadataReference> BuildRuntimeReferences()
    {
        var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;
        return tpa
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
    }
}

/// <summary>`{|ID:text|}` 마크업을 깨끗한 소스와 기대 진단 목록으로 분리한다.</summary>
internal static class MarkupParser
{
    public static (string Source, List<(string Id, TextSpan Span)> Expected) Parse(string markup)
    {
        var sb = new StringBuilder(markup.Length);
        var expected = new List<(string, TextSpan)>();

        var i = 0;
        while (i < markup.Length)
        {
            if (markup[i] == '{' && i + 1 < markup.Length && markup[i + 1] == '|')
            {
                var colon = markup.IndexOf(':', i + 2);
                Assert.True(colon >= 0, "마크업이 잘못되었습니다 — '{|ID:' 형태의 콜론을 찾지 못했습니다.");
                var end = markup.IndexOf("|}", colon + 1, StringComparison.Ordinal);
                Assert.True(end >= 0, "마크업이 잘못되었습니다 — 닫는 '|}'를 찾지 못했습니다.");

                var id = markup.Substring(i + 2, colon - (i + 2));
                var inner = markup.Substring(colon + 1, end - (colon + 1));

                var start = sb.Length;
                sb.Append(inner);
                expected.Add((id, TextSpan.FromBounds(start, sb.Length)));

                i = end + 2; // "|}" 건너뜀
            }
            else
            {
                sb.Append(markup[i]);
                i++;
            }
        }

        return (sb.ToString(), expected);
    }
}
