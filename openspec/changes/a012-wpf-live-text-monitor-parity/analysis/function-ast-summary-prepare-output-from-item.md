# Function AST Summary

> 이 문서는 tree-sitter-c-sharp 로 C# 대상 메서드 내부 구조를 **기계 추출**한 결과다.
> Roslyn semantic model이 아니므로 타입 추론/symbol resolution/DI·overload 해석은 하지 않는다.

## File

`Easislides.Wpf/Shell/MainViewModel.cs`

## Method

`MainViewModel.PrepareOutputFromItem`

## Location

- Start line: `6034`
- End line: `6046`
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
| 1 | `parameter` | 6034 | `LiveQueueItem item` |

## Branch Conditions

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `ternary` | 6037 | `IsLyricsPaginated(prepared) ? GetPreviewLyricsPageIndex(item) : prepared.LyricsPageIndex` |
| 2 | `ternary` | 6040 | `prepared.LyricsPageIndex == lyricsPageIndex ? prepared : prepared with { LyricsPageIndex = lyricsPageIndex }` |

## Return Statements

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `return` | 6045 | `return output;` |

## Assignments / Variable Declarations

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `variable_declaration` | 6036 | `var prepared = PrepareLiveItemForOutput(item)` |
| 2 | `variable_declaration` | 6037 | `var lyricsPageIndex = IsLyricsPaginated(prepared) ? GetPreviewLyricsPageIndex(item) : prepared.LyricsPageIndex` |
| 3 | `variable_declaration` | 6040 | `var output = prepared.LyricsPageIndex == lyricsPageIndex ? prepared : prepared with { LyricsPageIndex = lyricsPageIndex }` |
| 4 | `assignment` | 6043 | `OutputItem = output` |

## Type Conversions / Object Creation

- none

## Invocations / Await / Object Creation

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `invocation_expression` | 6036 | `PrepareLiveItemForOutput(item)` |
| 2 | `invocation_expression` | 6037 | `IsLyricsPaginated(prepared)` |
| 3 | `invocation_expression` | 6038 | `GetPreviewLyricsPageIndex(item)` |
| 4 | `invocation_expression` | 6044 | `PrepareOutputPowerPointForPublish(output)` |

## Mutations / Possible Side Effects

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 6043 | `OutputItem = output` |

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
