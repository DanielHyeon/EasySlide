# Function AST Summary

> 이 문서는 tree-sitter-c-sharp 로 C# 대상 메서드 내부 구조를 **기계 추출**한 결과다.
> Roslyn semantic model이 아니므로 타입 추론/symbol resolution/DI·overload 해석은 하지 않는다.

## File

`Easislides.Wpf/Shell/MainViewModel.cs`

## Method

`ResolveLiveProjection`

## Location

- Start line: `7962`
- End line: `8004`
- Body: block (`{ ... }`)

## Review Rule

- 이 요약에 나온 branch, return, assignment, conversion, mutation, fallback path를 누락한 채 구현하면 안 된다.
- 이 요약에 없는 branch를 임의로 만들어 추론하면 안 된다.
- `OUTPUT_STATE_MUTATION`(LS_*/selectScreen) 이 있으면 `selectScreen==null`/`LS_Width=0` 회귀 가드를 반드시 검증한다.
- 타입 추론·DI concrete class·extension method·overload·LINQ/async 의미는 Tree-sitter만으로 확정하지 말고 '불확실'로 표시해 CodeGraph/Roslyn 2차 분석으로 넘긴다.
- 도메인 의미 판단은 다음 단계 `function-logic-map.md`에서 수행한다.

## Parameters

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `parameter` | 7963 | `LiveQueueItem item` |
| 2 | `parameter` | 7964 | `Rendering.PowerPointPreviewViewModel? powerPoint = null` |

## Branch Conditions

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `ternary` | 7975 | `item.UseIndividualFormatting ? item.FormatData : null` |
| 2 | `if` | 7977 | `IsPowerPointItem(item) && ppt.State == Rendering.PowerPointPreviewState.Ready && ppt.PreviewImage is not null && !string.IsNullOrEmpty(item.ContentPath) && string.Equals(ppt.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIg...` |
| 3 | `logical_and` | 7977 | `IsPowerPointItem(item) && ppt.State == Rendering.PowerPointPreviewState.Ready && ppt.PreviewImage is not null && !string.IsNullOrEmpty(item.ContentPath) && string.Equals(ppt.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIg...` |
| 4 | `logical_and` | 7977 | `IsPowerPointItem(item) && ppt.State == Rendering.PowerPointPreviewState.Ready` |
| 5 | `logical_and` | 7979 | `null && !string.IsNullOrEmpty(item.ContentPath) && string.Equals(ppt.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase)` |
| 6 | `logical_and` | 7979 | `null && !string.IsNullOrEmpty(item.ContentPath)` |

## Return Statements

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `return` | 7983 | `return item with { PreviewSource = ppt.PreviewImage, PreviewFillMode = Rendering.ImageFillMode.Fit, SlideNumber = ppt.SlideNumber, PositionLabel = positionLabel, NextTitle = nextTitle, ShowNotations = showNotations, TransposeSemitones = ...` |
| 2 | `return` | 7996 | `return item with { PositionLabel = positionLabel, NextTitle = nextTitle, ShowNotations = showNotations, TransposeSemitones = LiveTransposeSemitones, FormatData = formatData, };` |

## Assignments / Variable Declarations

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 7966 | `item = PrepareLiveItemForOutput(item)` |
| 2 | `variable_declaration` | 7967 | `var ppt = powerPoint ?? PowerPoint` |
| 3 | `variable_declaration` | 7968 | `var positionLabel = ComputePositionLabel(item, ppt)` |
| 4 | `variable_declaration` | 7969 | `var nextTitle = ComputeNextTitle(item)` |
| 5 | `variable_declaration` | 7972 | `var showNotations = _settings.Get(EasiSettingKeys.LyricsMonitorShowNotations)` |
| 6 | `variable_declaration` | 7975 | `var formatData = item.UseIndividualFormatting ? item.FormatData : null` |

## Type Conversions / Object Creation

- none

## Invocations / Await / Object Creation

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `invocation_expression` | 7966 | `PrepareLiveItemForOutput(item)` |
| 2 | `invocation_expression` | 7968 | `ComputePositionLabel(item, ppt)` |
| 3 | `invocation_expression` | 7969 | `ComputeNextTitle(item)` |
| 4 | `invocation_expression` | 7972 | `_settings.Get(EasiSettingKeys.LyricsMonitorShowNotations)` |
| 5 | `invocation_expression` | 7977 | `IsPowerPointItem(item)` |
| 6 | `invocation_expression` | 7980 | `string.IsNullOrEmpty(item.ContentPath)` |
| 7 | `invocation_expression` | 7981 | `string.Equals(ppt.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase)` |

## Mutations / Possible Side Effects

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 7966 | `item = PrepareLiveItemForOutput(item)` |

## Try / Catch / Finally / Throw

- none

## Loops

- none

## Next Required Artifacts

1. `function-logic-map.md` — 위 항목별 도메인 의미 / early return 영향 / invariant / 수정 가능·금지 영역
2. `branch-test-map.md` — branch별 기존 테스트 매핑 + 추가 실패 테스트

## Implementation Gate

이 문서만으로 production code를 수정하면 안 된다.
`function-logic-map.md` 와 `branch-test-map.md` 작성 후에만 구현을 시작한다.
(CLAUDE.md `## 함수 내부 로직 분석 레이어` 7절 구현 금지 조건)
