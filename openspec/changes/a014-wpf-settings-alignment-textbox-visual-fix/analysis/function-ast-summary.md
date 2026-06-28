# Function AST Summary

> 이 문서는 tree-sitter-c-sharp 로 C# 대상 메서드 내부 구조를 **기계 추출**한 결과다.
> Roslyn semantic model이 아니므로 타입 추론/symbol resolution/DI·overload 해석은 하지 않는다.

## File

`Easislides.Wpf/Shell/MainViewModel.cs`

## Method

`ApplyLyricsAlignment`

## Location

- Start line: `8371`
- End line: `8376`
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
| 1 | `parameter` | 8371 | `LyricsTextAlignment alignment` |

## Branch Conditions

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `switch_expression` | 8375 | `alignment switch { LyricsTextAlignment.Left => "왼쪽", LyricsTextAlignment.Right => "오른쪽", _ => "가운데" }` |
| 2 | `switch_arm` | 8375 | `LyricsTextAlignment.Left => "왼쪽"` |
| 3 | `switch_arm` | 8375 | `LyricsTextAlignment.Right => "오른쪽"` |
| 4 | `switch_arm` | 8375 | `_ => "가운데"` |

## Return Statements

- none

## Assignments / Variable Declarations

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 8374 | `ActiveLyricsAlignment = alignment` |
| 2 | `assignment` | 8375 | `StatusText = $"가사 정렬: {alignment switch { LyricsTextAlignment.Left => "왼쪽", LyricsTextAlignment.Right => "오른쪽", _ => "가운데" }}"` |

## Type Conversions / Object Creation

- none

## Invocations / Await / Object Creation

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `invocation_expression` | 8373 | `_settings.Set(EasiSettingKeys.LyricsMonitorTextAlignment, alignment)` |

## Mutations / Possible Side Effects

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 8374 | `ActiveLyricsAlignment = alignment` |
| 2 | `assignment` | 8375 | `StatusText = $"가사 정렬: {alignment switch { LyricsTextAlignment.Left => "왼쪽", LyricsTextAlignment.Right => "오른쪽", _ => "가운데" }}"` |

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
