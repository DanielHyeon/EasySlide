using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Easislides.Analyzers;

/// <summary>
/// EasiDS003 — 매직 간격(Margin/Padding) 값 직접 사용을 잡아내는 분석기 (계획서 §4.3, §9.2).
///
/// 쉽게 말하면: 여백(Margin)·안쪽 여백(Padding)에 아무 숫자나 손으로 적으면 안 된다고 알려줘요.
/// 간격은 4-base 스케일(2,4,8,12,16,20,24,32,40,48,64) 또는 0만 쓸 수 있습니다.
/// 토큰: <c>Thickness.Sm/Md/Lg</c> 등 (계획서 §4.3).
///
/// 잡아내는 경우:
///  - <c>Margin = new Thickness(3, 5, 3, 5)</c> / <c>Padding = new Thickness(3)</c> 처럼
///    스케일에 없는 값을 직접 쓴 경우.
///
/// 허용하는 경우(오탐 방지):
///  - 스케일 위의 값(0,2,4,8,12,16,…)만으로 만든 Thickness.
///  - <c>BorderThickness = new Thickness(1)</c> 등 Margin/Padding 이 아닌 곳 — 간격이 아니므로 대상 외.
///  - 런타임 값으로 만든 Thickness — 인자가 상수가 아님.
///
/// 이 단계(스켈레톤)는 아직 아무 것도 잡지 않습니다 — TDD 빨강(red) 단계용.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EasiDsSpacingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "EasiDS003";

    private const string Category = "EasiDS";

    internal static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "매직 간격 값 직접 사용 금지",
        messageFormat: "'{0}'의 간격 값이 4-base 스케일(2,4,8,12,16,20,24,32,40,48,64) 밖입니다 — Thickness.* 토큰을 사용하세요",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Margin/Padding 은 EasiDS 4-base 간격 토큰만 사용해야 레이아웃 리듬이 일관됩니다 (계획서 §4.3, §9.2).");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    // 허용되는 4-base 간격 스케일 (+ 0). 계획서 §4.3.
    private static readonly HashSet<double> AllowedScale =
        new() { 0, 2, 4, 8, 12, 16, 20, 24, 32, 40, 48, 64 };

    // 간격을 의미하는 속성 — 이 속성에 대입될 때만 검사 (BorderThickness 등은 제외).
    private static readonly HashSet<string> SpacingProperties =
        new() { "Margin", "Padding" };

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeThicknessCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeThicknessCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;

        // Thickness 생성만 대상.
        if (context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type
            is not INamedTypeSymbol { Name: "Thickness" })
        {
            return;
        }

        // Margin/Padding 에 대입되는 경우에만 검사 (간격 의미).
        if (!IsAssignedToSpacingProperty(creation))
        {
            return;
        }

        var arguments = creation.ArgumentList?.Arguments;
        if (arguments is null || arguments.Value.Count == 0)
        {
            return;
        }

        var anyOffScale = false;
        foreach (var argument in arguments.Value)
        {
            var constant = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
            if (!constant.HasValue)
            {
                // 런타임 값이 섞이면 동적 간격으로 보고 검사하지 않음.
                return;
            }

            if (!TryToDouble(constant.Value, out var value))
            {
                return;
            }

            if (!AllowedScale.Contains(value))
            {
                anyOffScale = true;
            }
        }

        if (anyOffScale)
        {
            var label = string.Join(" ", creation.ToString().Split(
                new[] { ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries));
            context.ReportDiagnostic(Diagnostic.Create(Rule, creation.GetLocation(), label));
        }
    }

    // new Thickness(...) 가 Margin/Padding 대입의 우변인지 확인 (대입식·객체 초기자 모두 포함).
    private static bool IsAssignedToSpacingProperty(ObjectCreationExpressionSyntax creation)
    {
        if (creation.Parent is not AssignmentExpressionSyntax assignment || assignment.Right != creation)
        {
            return false;
        }

        var targetName = assignment.Left switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            _ => null,
        };

        return targetName is not null && SpacingProperties.Contains(targetName);
    }

    private static bool TryToDouble(object? value, out double result)
    {
        switch (value)
        {
            case double d: result = d; return true;
            case int i: result = i; return true;
            case float f: result = f; return true;
            case long l: result = l; return true;
            case short s: result = s; return true;
            case byte b: result = b; return true;
            default: result = 0; return false;
        }
    }
}
