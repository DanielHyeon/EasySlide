# 함수 AST 요약

> 이 문서는 `tree-sitter-c-sharp` 기반 C# 메서드 구조 추출 결과를 사람이 검토할 수 있도록 정리한 것이다.  
> tree-sitter 결과는 구문 구조 근거이며, overload/DI/의미 해석은 CodeGraph와 테스트 결과로 보완한다.

## 파일

`Easislides.Wpf/Shell/MainViewModel.cs`

## 메서드

`MainViewModel.ToggleUseIndividualFormatting`

## 역할

선택된 예배 순서 항목의 `UseIndividualFormatting` 값을 반전하고, 큐와 선택 항목을 같은 record 값으로 갱신한다.

## 매개변수

- 없음

## 주요 분기

| 분기 | 조건 | 의미 |
| --- | --- | --- |
| B1 | `SelectedItem is not { } item` | 선택 항목이 없으면 아무 작업도 하지 않는다. |
| B2 | `index < 0` | 선택 항목이 큐에서 참조로 찾히지 않으면 아무 작업도 하지 않는다. |
| B3 | `updated.UseIndividualFormatting` | 상태 문구를 “개별 서식 사용” 또는 “전역 기본 서식 사용”으로 나눈다. |

## 주요 호출

- `IndexOfReference(item)`
- `RepublishLiveSongForBodyChange()`
- `NotifyCommandStates()`

## 상태 변경

- `Queue[index] = updated`
- `SelectedItem = updated`
- `StatusText = ...`

## 검토 메모

- 이 함수 자체는 수정하지 않았다.
- 기존 직접 명령 테스트는 유지했다.
- 실제 문제는 이 함수가 아니라 개별 설정 명령의 활성화 게이트인 `IsPerItemFormattable`에 있었다.
