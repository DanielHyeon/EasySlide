using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Easislides.Analyzers;

/// <summary>
/// EasiDS001 — 매직 색·폰트 직접 사용을 잡아내는 분석기 (계획서 §9.2, §4.1, §4.2).
///
/// 쉽게 말하면: 코드 안에 색깔이나 글꼴 이름을 "손으로 직접" 적으면 안 된다고 알려주는 검사기예요.
/// 색과 글꼴은 디자인 토큰(예: Brush.Accent.Primary, Font.Primary)으로만 써야 합니다.
///
/// 잡아내는 경우:
///  - <c>Color.FromArgb/FromRgb/FromScRgb(...)</c> 인자가 전부 상수(고정값)일 때 — 매직 색.
///  - <c>Colors.Navy</c>, <c>System.Drawing.Color.Red</c>, <c>SystemColors.Control</c> 같은 이름표 색(named color).
///  - <c>new FontFamily("Tahoma")</c> 처럼 글꼴 이름을 문자열로 직접 쓴 경우.
///  - <c>new System.Drawing.Font(...)</c> — WPF에서 Drawing 폰트 생성.
///
/// 허용하는 경우(오탐 방지):
///  - 런타임 값으로 만든 색(예: <c>Color.FromArgb((byte)(v>>24), ...)</c>) — 인자가 상수가 아님.
///  - <c>new FontFamily("pack://...")</c> — 번들 폰트 리소스 등록(토큰 정의 자체).
///  - <c>System.Drawing</c> 밖의 도메인 <c>Font</c> 타입 — 디자인 토큰 래퍼 등은 차단하지 않음.
///
/// 알려진 한계(후속 작업):
///  - 인자 하나라도 <c>static readonly</c> 색 값이면 동적으로 간주되어 빠져나갈 수 있음(스펙상 의도).
///  - <c>Brushes.Red</c> 등 브러시 팔레트는 아직 미대상 — PreviewCanvas 등 라이브 렌더링 폴백을
///    토큰화하는 별도 작업에서 다룬다(silent drop 아님, 명시적 보류).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EasiDsColorFontAnalyzer : DiagnosticAnalyzer
{
    /// <summary>진단 식별자 — 계획서가 명시한 규칙 번호.</summary>
    public const string DiagnosticId = "EasiDS001";

    private const string Category = "EasiDS";

    internal static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "매직 색·폰트 직접 사용 금지",
        messageFormat: "'{0}' 대신 EasiDS 토큰(예: Brush.* / Font.Primary)을 사용하세요",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "색과 글꼴은 EasiDS 디자인 토큰으로만 지정해야 라이트/다크 테마와 일관성이 유지됩니다 (계획서 §4.1, §4.2, §9.2).");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    // Color.FromArgb / FromRgb / FromScRgb 가 전부 상수 인자일 때 → 매직 색.
    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol
            is not IMethodSymbol method)
        {
            return;
        }

        if (method.ContainingType?.Name != "Color")
        {
            return;
        }

        if (method.Name is not ("FromArgb" or "FromRgb" or "FromScRgb"))
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count == 0)
        {
            return;
        }

        // 인자 중 하나라도 런타임 값이면(상수가 아니면) 동적 색 → 허용.
        foreach (var argument in arguments)
        {
            if (!context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken).HasValue)
            {
                return;
            }
        }

        Report(context, invocation);
    }

    // Colors.Navy / System.Drawing.Color.Red 처럼 이름표 색 읽기 → 매직 색.
    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;

        // 메서드 그룹(Color.FromArgb)이나 타입/네임스페이스 접근은 제외 — 오직 속성/필드 읽기만.
        ITypeSymbol? memberType = symbol switch
        {
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            _ => null,
        };

        if (memberType is null)
        {
            return;
        }

        // "색을 돌려주는" 멤버이고, 그 멤버가 색 팔레트 타입에 들어있을 때만.
        // Color/Colors(WPF·Drawing 명명색) + SystemColors(레거시 시스템 색).
        var containerName = symbol!.ContainingType?.Name;
        if (containerName is not ("Color" or "Colors" or "SystemColors"))
        {
            return;
        }

        if (memberType.Name != "Color")
        {
            return;
        }

        Report(context, memberAccess);
    }

    // new FontFamily("이름") / new System.Drawing.Font(...) → 매직 폰트.
    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type
            is not INamedTypeSymbol type)
        {
            return;
        }

        switch (type.Name)
        {
            case "FontFamily":
                // pack:// 리소스 등록(번들 폰트)은 허용. 그 외 문자열 리터럴 이름만 차단.
                var firstArgument = creation.ArgumentList?.Arguments.FirstOrDefault();
                if (firstArgument is null)
                {
                    return;
                }

                var constant = context.SemanticModel.GetConstantValue(firstArgument.Expression, context.CancellationToken);
                if (constant.Value is not string name)
                {
                    return; // 동적 글꼴 이름 → 허용
                }

                if (name.StartsWith("pack:", System.StringComparison.OrdinalIgnoreCase))
                {
                    return; // 번들 리소스 → 허용
                }

                Report(context, creation);
                break;

            case "Font":
                // WPF에는 Font 타입이 없음 — System.Drawing.Font 생성만 레거시 매직 폰트로 차단.
                // 도메인이 정의한 다른 'Font' 토큰 타입은 오탐이 되지 않도록 네임스페이스로 한정.
                if (type.ContainingNamespace?.ToDisplayString() == "System.Drawing")
                {
                    Report(context, creation);
                }
                break;
        }
    }

    private static void Report(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        // 여러 줄에 걸친 호출(예: Color.FromArgb(\n ... ))도 메시지에서는 한 줄로 보이도록 공백 정규화.
        var label = string.Join(" ", node.ToString().Split(
            new[] { ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries));
        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), label));
    }
}
