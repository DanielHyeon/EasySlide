# 영향 맵

## 구현 영향

- `IsPerItemFormattable`은 이제 `Lyrics`가 비어 있지 않은 경우 `LiveItemKinds.Notice`도 포함한다.
- 기존 음수 명령 테스트에서 `Notice`를 일반적인 “비곡” fixture로 쓰던 부분은 실제 비텍스트 시각 항목인 `PowerPoint`로 바꿨다.
- 이로써 사용자 문제가 발생한 텍스트 파일 경로는 활성화하면서, PowerPoint/미디어 항목은 텍스트 서식 게이트 밖에 유지한다.

## CodeGraph 근거

- CodeGraph 상태: 초기화됨, 536개 파일 인덱싱.
- `codegraph_search UseIndividual`에서 다음 관련 심볼을 확인했다.
  - `MainViewModel.ApplyGlobalFormatToAll`
  - `MainViewModel.ClearSelectedItemFormatting`
  - `MainViewModel.ClearAllItemsFormatting`
  - 기존 회귀 테스트 `UseIndividualFormattingOff_LiveProjectionDropsPerSongColor`
- `codegraph_search ToggleUseIndividualFormatting`에서 다음 심볼을 확인했다.
  - `MainViewModel.ToggleUseIndividualFormatting`
  - `MainViewModel.ToggleUseIndividualFormattingCommand`
  - `ToggleUseIndividualFormatting_FlipsFlagOnSelectedItem`
  - `ToggleUseIndividualFormatting_LiveMultiVerse_PreservesCurrentVerse`
- `codegraph_impact ToggleUseIndividualFormatting` 결과, 변경 영향은 `MainViewModel` 내부의 큐 선택 상태, 미리보기/출력 서식 속성, 명령 상태 알림, 라이브 재송출 경로에 걸친다.
- `codegraph_callers ToggleUseIndividualFormatting`은 직접 호출자를 찾지 못했다. 해당 함수는 `RelayCommand`를 통해 호출된다.

## XAML 바인딩 근거

- `MainWindow.xaml`에는 `Ind_checkBox`가 있다.
- `Ind_checkBox.Command`는 `ToggleUseIndividualFormattingCommand`에 바인딩된다.
- `Ind_checkBox.IsChecked`는 `SelectedItem.UseIndividualFormatting`에 `Mode=OneWay`로 바인딩된다.
- `WorshipListPanel.xaml` 컨텍스트 메뉴도 `ToggleUseIndividualFormattingCommand`에 바인딩된다.

## 영향 경계

- 주요 production 대상: `Easislides.Wpf/Shell/MainViewModel.cs`
- 관련 UI 화면: `Easislides.Wpf/MainWindow.xaml`
- 테스트 대상: `Easislides.Wpf.Tests/Shell/MainViewModelTests.cs`

## 초기 발견

ViewModel의 직접 명령 경로는 기존 테스트에서 이미 플래그를 정상 반전했다. 따라서 처음에는 WPF UI 바인딩/상호작용 계약 문제 가능성이 높다고 보았다.

## 정제된 발견

개별 설정 컨트롤은 `CanEditSelectedItemColor`로 활성화 여부를 판단하고, 이 속성은 `IsPerItemFormattable`에 위임한다. 기존 함수는 `Lyrics`가 있는 `Song` 또는 `Bible`만 허용했다. 외부 `.txt` 항목, 예를 들어 사도신경 텍스트 파일은 `Notice` 항목으로 표현되므로 화면에는 보이지만 실제 설정 명령은 비활성화됐다.
