# 함수 로직 맵

## 대상 함수 1

- 파일: `Easislides.Wpf/Shell/MainViewModel.cs`
- 함수: `MainViewModel.ToggleUseIndividualFormatting`
- 호출 경로: `RelayCommand`를 통한 `ToggleUseIndividualFormattingCommand`
- 관련 UI: `MainWindow.xaml`의 `Ind_checkBox`, `WorshipListPanel.xaml`의 컨텍스트 메뉴 항목
- 호출 함수:
  - `IndexOfReference(item)`
  - `RepublishLiveSongForBodyChange()`
  - `NotifyCommandStates()`

## 함수 책임

선택된 큐 항목의 `UseIndividualFormatting` 플래그를 토글한다. 큐 항목 record를 교체하고, 선택 항목과 상태 문구를 갱신하며, 현재 라이브 중인 항목이면 출력 재송출 경로도 갱신한다.

## 입력

- `SelectedItem`: 현재 미리보기/큐 선택 항목. null일 수 있다.
- `Queue`: 선택 항목 인스턴스 또는 참조 기준으로 찾을 수 있는 항목을 포함한 observable 큐.

## 출력

- 반환값 없음.
- `Queue[index]`, `SelectedItem`, `StatusText`를 갱신한다.

## 분기표

| 분기 | 조건 | 참 경로 | 거짓 경로 | 위험 |
| --- | --- | --- | --- | --- |
| B1 | `SelectedItem is not { } item` | 변경 없이 반환 | 계속 진행 | 선택 항목이 없는데 UI가 보이면 클릭이 무시된다. 일반적으로 Command CanExecute가 막아야 한다. |
| B2 | `index < 0` | 변경 없이 반환 | 계속 진행 | 선택 항목이 큐 안에 없으면 화면 클릭이 동작하지 않는 것처럼 보인다. |
| B3 | `updated.UseIndividualFormatting` | 상태 문구를 개별 서식 사용으로 표시 | 상태 문구를 전역 기본 서식 사용으로 표시 | 상태 문구 전용 분기이며 실제 동작을 주도하면 안 된다. |

## 상태 변경

- `Queue[index] = updated`: 플래그가 반전된 record로 큐 항목을 교체한다.
- `SelectedItem = updated`: 교체된 항목을 다시 선택하고 의존 UI 속성을 갱신한다.
- `StatusText = ...`: 켜짐/꺼짐 상태를 보고한다.

## 외부 부작용

- `RepublishLiveSongForBodyChange()`는 현재 항목이 라이브 중이면 출력 화면을 다시 발행할 수 있다.
- `NotifyCommandStates()`는 관련 명령 활성 상태를 갱신한다.

## 불변식

- 플래그 토글은 계속 이 명령 경로를 통해 수행한다.
- 토글은 `FormatData`를 삭제하지 않아야 한다.
- 라이브 중 다중 절 항목에서 현재 절 위치가 0절로 튀지 않아야 한다.
- XAML UI는 교체된 `SelectedItem.UseIndividualFormatting` 값을 반영해야 한다.

## 대상 함수 2

- 파일: `Easislides.Wpf/Shell/MainViewModel.cs`
- 함수: `MainViewModel.IsPerItemFormattable`
- 호출 경로:
  - `CanEditSelectedItemColor`
  - `CanPasteSelectedItemFormatting`
  - `CanApplyCopiedFormatToAll`
  - `CanClearAllItemsFormatting`
  - 일괄 서식 적용/삭제 루프

## 함수 책임

선택된 큐 항목 종류가 개별 서식 명령을 받을 수 있는지 판단한다.

## 분기표

| 분기 | 조건 | 수정 전 동작 | 수정 후 동작 | 위험 |
| --- | --- | --- | --- | --- |
| B1 | `item.Kind is Song or Bible` | 본문이 있으면 허용 | 유지 | 기존 곡/성경 동작은 유지되어야 한다. |
| B2 | `item.Kind is Notice` | 항상 제외 | 본문이 있으면 허용 | 텍스트 파일/공지 항목이 실제 렌더링되는 경로와 일치해야 한다. |
| B3 | `!string.IsNullOrWhiteSpace(item.Lyrics)` | 본문 없는 항목 제외 | 유지 | 빈 placeholder 항목이 편집 가능해지면 안 된다. |
| B4 | PowerPoint/Media 등 | 제외 | 제외 | 비텍스트 시각 항목이 텍스트 서식 대상으로 잘못 열리면 안 된다. |

## 필요한 변경

WPF 텍스트 모니터 출력 경로는 `Notice`/텍스트 파일 항목에도 `FormatData`를 적용할 수 있다. 따라서 `Lyrics`가 있는 `Notice` 항목을 서식 가능 항목에 포함해야 한다.

## 불변식

- `Lyrics`가 비어 있는 항목은 계속 편집 불가다.
- PowerPoint/미디어/이미지 같은 시각 항목은 별도 설계 전까지 텍스트 서식 명령 대상이 아니다.
- `IsPerItemFormattable`을 쓰는 일괄 명령도 같은 기준을 공유한다.
