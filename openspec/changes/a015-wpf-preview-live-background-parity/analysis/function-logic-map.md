# 함수 내부 로직 맵

## Target Function

- 파일: `Easislides.Wpf/Shell/MainViewModel.cs`
- 주요 대상:
  - `ResolveLiveProjection`
  - `CreateLyricsSampleBackgroundBrush`
  - `CreateLyricsSampleForegroundBrush`
  - `CreateLyricsSampleFontSize`
  - `CreateLyricsSampleLineHeight`
  - `CreateLyricsSampleFontFamily`
  - `CreateLyricsSampleTextAlignment`
  - `CreateLyricsSampleHorizontalAlignment`
  - `CreateLyricsSampleVerticalAlignment`

## Function Responsibility

- `ResolveLiveProjection`: Live session에 넘길 `LiveQueueItem` projection을 만든다. 항목별 서식 사용 여부에 따라 `FormatData`를 보존하거나 null 처리한다.
- `CreateLyricsSample*`: 메인 화면의 Preview/Output 샘플 패널에서 배경·글자색·폰트·정렬을 계산한다.

## Inputs

| 입력 | 출처 | 의미 | 주의 |
| --- | --- | --- | --- |
| `LiveQueueItem item` | 선택/미리보기/출력 항목 | 가사, 제목, `FormatData`, `UseIndividualFormatting` 보유 | record라 `with` 복사 시 기존 값 보존 |
| `item.FormatData` | 레거시 서식 문자열 | `61=` 배경 이미지, `62=` 이미지 모드, `29=` 글자색 등 | `UseIndividualFormatting=false`면 렌더 입력으로 쓰면 안 됨 |
| `item.UseIndividualFormatting` | 예배 순서 항목 설정 | 항목별 서식 사용 여부 | Live projection은 이미 이 값을 적용함 |
| `ActiveOutputBackgroundImagePath` | 전역 설정 | default/global 배경 이미지 | 항목별 배경이 없거나 비활성일 때 fallback |

## Outputs

| 함수 | 출력 | 조건 |
| --- | --- | --- |
| `ResolveLiveProjection` | `LiveQueueItem` | `FormatData = item.UseIndividualFormatting ? item.FormatData : null` |
| `CreateLyricsSampleBackgroundBrush` | `Brush` | 현재는 `FormatData`를 직접 parse함 |
| `CreateLyricsSampleForegroundBrush` 등 | 색/크기/정렬 값 | 현재는 `FormatData`를 직접 parse함 |

## Branch Table

| 분기 | 현재 동작 | 도메인 의미 | 위험 |
| --- | --- | --- | --- |
| `item.UseIndividualFormatting ? item.FormatData : null` | Live projection에서만 적용 | 개별 서식 꺼짐이면 Live는 default | Preview 샘플이 같은 분기를 적용하지 않으면 좌/우 불일치 |
| `format.BackgroundImagePath` 있음 | Preview 샘플은 항목 배경 표시 | 항목별 배경 미리보기 | UseIndividualFormatting=false에서도 표시되는 위험 |
| 이미지 로드 실패 | 색 배경 fallback | 파일 없음/권한 오류 안전 | 이번 변경 범위 아님 |
| PPT ready branch | PPT 이미지 projection | PPT 송출 전용 | 이번 변경 범위 아님 |

## Early Returns

- `ResolveLiveProjection`은 PPT ready branch에서 조기 `return`한다.
- 두 return 모두 `FormatData = formatData`를 포함하므로, `UseIndividualFormatting` 규칙은 PPT/non-PPT 양쪽에 적용된다.

## State Mutations

- `ResolveLiveProjection` 내부에서 `item = PrepareLiveItemForOutput(item)` 재할당이 있다.
- `CreateLyricsSample*` 함수는 상태 변경 없이 화면 표시 값을 계산한다.

## External Side Effects

- `CreateLyricsSampleBackgroundBrush`는 이미지 파일 존재 확인 및 `BitmapImage` 로딩을 수행한다.
- 송출 좌표/모니터 상태 변경 없음.
- DB/COM 변경 없음.

## Exception/Fallback Path

- 샘플 이미지 로딩 중 `IOException`, `UnauthorizedAccessException`, `NotSupportedException`, `UriFormatException`은 null fallback으로 처리된다.
- 이번 변경은 예외 처리 구조를 바꾸지 않는다.

## Invariants

- `UseIndividualFormatting=false`이면 Live 송출에 항목 `FormatData`가 적용되면 안 된다.
- 미리보기 샘플은 Live 송출과 같은 유효 서식 규칙을 따라야 한다.
- `FormatData` 원문은 보존되어야 하며, 체크박스를 다시 켰을 때 항목별 서식을 되살릴 수 있어야 한다.
- `LS_Width`, `LS_Height`, `selectScreen`은 변경하지 않는다.

## Suspicious Logic

- `CreateLyricsSample*`는 현재 `UseIndividualFormatting`을 확인하지 않아 Live projection과 다르게 보일 수 있다.
- 사용자는 왼쪽 Preview를 Live 결과의 근거로 보므로, 샘플과 송출이 다른 규칙을 쓰면 “배경이 넘어가지 않음”으로 인식된다.

## Implementation Boundary

- 수정 가능: `MainViewModel`의 샘플 서식 계산 helper와 해당 단위 테스트.
- 수정 금지: `LiveSessionService`, `OutputRenderer`, Output monitor 좌표 계산, DB/COM 경로.

## 분석 결론

수정 가능. 단, 먼저 `UseIndividualFormatting=false` 미리보기 배경 불일치를 실패 테스트로 고정한다.
