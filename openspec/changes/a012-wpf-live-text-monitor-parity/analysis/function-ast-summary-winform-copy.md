# Function AST Summary

> 이 문서는 tree-sitter-c-sharp 로 C# 대상 메서드 내부 구조를 **기계 추출**한 결과다.
> Roslyn semantic model이 아니므로 타입 추론/symbol resolution/DI·overload 해석은 하지 않는다.

## File

`Easislides/Easislides/FrmMain.Logic.cs`

## Method

`FrmMain.CopyPreviewToOutput`

## Location

- Start line: `3052`
- End line: `3085`
- Body: block (`{ ... }`)

## Review Rule

- 이 요약에 나온 branch, return, assignment, conversion, mutation, fallback path를 누락한 채 구현하면 안 된다.
- 이 요약에 없는 branch를 임의로 만들어 추론하면 안 된다.
- `OUTPUT_STATE_MUTATION`(LS_*/selectScreen) 이 있으면 `selectScreen==null`/`LS_Width=0` 회귀 가드를 반드시 검증한다.
- 타입 추론·DI concrete class·extension method·overload·LINQ/async 의미는 Tree-sitter만으로 확정하지 말고 '불확실'로 표시해 CodeGraph/Roslyn 2차 분석으로 넘긴다.
- 도메인 의미 판단은 다음 단계 `function-logic-map.md`에서 수행한다.

## Parameters

- none

## Branch Conditions

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `ternary` | 3058 | `(Gf.OutputItem.CurItemNo > 0) ? Gf.OutputItem.CurItemNo : Gf.StartPresAt` |
| 2 | `if` | 3060 | `Gf.ShowRunning` |
| 3 | `if` | 3062 | `Gf.OutputItem.CurItemNo == 0` |
| 4 | `if` | 3070 | `Gf.PreviewItem.Type == "P" && Gf.PreviewItem.Source != ItemSource.WorshipList` |
| 5 | `logical_and` | 3070 | `Gf.PreviewItem.Type == "P" && Gf.PreviewItem.Source != ItemSource.WorshipList` |
| 6 | `if` | 3079 | `Gf.ShowRunning` |

## Return Statements

- none

## Assignments / Variable Declarations

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 3054 | `Gf.OutputItem.InMainItemText = Gf.PreviewItem.InMainItemText` |
| 2 | `assignment` | 3055 | `Gf.OutputItem.InSubItemItem1Text = Gf.PreviewItem.InSubItemItem1Text` |
| 3 | `assignment` | 3056 | `Gf.OutputItem.Source = Gf.PreviewItem.Source` |
| 4 | `assignment` | 3057 | `Gf.OutputItem.CurItemNo = Gf.PreviewItem.CurItemNo` |
| 5 | `assignment` | 3058 | `Gf.StartPresAt = ((Gf.OutputItem.CurItemNo > 0) ? Gf.OutputItem.CurItemNo : Gf.StartPresAt)` |
| 6 | `assignment` | 3059 | `Gf.OutputItem.OutputStyleScreen = true` |
| 7 | `assignment` | 3064 | `Gf.WorshipSongs[0, 2] = Gf.OutputItem.InMainItemText` |
| 8 | `assignment` | 3065 | `Gf.WorshipSongs[0, 0] = Gf.OutputItem.InSubItemItem1Text` |
| 9 | `assignment` | 3066 | `Gf.WorshipSongs[0, 1] = DataUtil.Left(Gf.WorshipSongs[0, 0], 1)` |
| 10 | `assignment` | 3067 | `Gf.WorshipSongs[0, 4] = Gf.PreviewItem.Format.FormatString` |
| 11 | `assignment` | 3068 | `Gf.AdHocItemPresent = true` |
| 12 | `assignment` | 3075 | `LoadThumbOutlockkey = 0` |
| 13 | `assignment` | 3076 | `previousOutSelectedSlide = 1` |
| 14 | `assignment` | 3081 | `Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStoredItem` |

## Type Conversions / Object Creation

- none

## Invocations / Await / Object Creation

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `invocation_expression` | 3066 | `DataUtil.Left(Gf.WorshipSongs[0, 0], 1)` |
| 2 | `invocation_expression` | 3072 | `gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs)` |
| 3 | `invocation_expression` | 3077 | `LoadItem(ref Gf.OutputItem, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString, Gf.PreviewItem.CurSlide, ref Gf.PreviewItem.Title, ScrollToCaret: true)` |
| 4 | `invocation_expression` | 3078 | `UpdateWorshipShowIcons()` |
| 5 | `invocation_expression` | 3082 | `RemoteControlLiveShow(LiveShowAction.Remote_SongChanged)` |
| 6 | `invocation_expression` | 3084 | `FocusOutputArea()` |

## Mutations / Possible Side Effects

| ID | Kind | Line | Expression |
|---:|---|---:|---|
| 1 | `assignment` | 3054 | `Gf.OutputItem.InMainItemText = Gf.PreviewItem.InMainItemText` |
| 2 | `assignment` | 3055 | `Gf.OutputItem.InSubItemItem1Text = Gf.PreviewItem.InSubItemItem1Text` |
| 3 | `assignment` | 3056 | `Gf.OutputItem.Source = Gf.PreviewItem.Source` |
| 4 | `assignment` | 3057 | `Gf.OutputItem.CurItemNo = Gf.PreviewItem.CurItemNo` |
| 5 | `assignment` | 3058 | `Gf.StartPresAt = ((Gf.OutputItem.CurItemNo > 0) ? Gf.OutputItem.CurItemNo : Gf.StartPresAt)` |
| 6 | `assignment` | 3059 | `Gf.OutputItem.OutputStyleScreen = true` |
| 7 | `assignment` | 3064 | `Gf.WorshipSongs[0, 2] = Gf.OutputItem.InMainItemText` |
| 8 | `assignment` | 3065 | `Gf.WorshipSongs[0, 0] = Gf.OutputItem.InSubItemItem1Text` |
| 9 | `assignment` | 3066 | `Gf.WorshipSongs[0, 1] = DataUtil.Left(Gf.WorshipSongs[0, 0], 1)` |
| 10 | `assignment` | 3067 | `Gf.WorshipSongs[0, 4] = Gf.PreviewItem.Format.FormatString` |
| 11 | `assignment` | 3068 | `Gf.AdHocItemPresent = true` |
| 12 | `assignment` | 3075 | `LoadThumbOutlockkey = 0` |
| 13 | `assignment` | 3076 | `previousOutSelectedSlide = 1` |
| 14 | `assignment` | 3081 | `Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStoredItem` |

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
