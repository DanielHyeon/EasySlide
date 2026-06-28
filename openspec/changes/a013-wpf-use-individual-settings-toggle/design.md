# 설계: WPF Use Individual Settings 텍스트 항목 서식 활성화

## 현재 흐름

- `MainWindow.xaml`은 미리보기 서식 패널에 `Ind_checkBox`를 렌더링한다.
- `Ind_checkBox.Command`는 `ToggleUseIndividualFormattingCommand`에 바인딩된다.
- `Ind_checkBox.IsChecked`는 `SelectedItem.UseIndividualFormatting`에 단방향 바인딩된다.
- `ToggleUseIndividualFormattingCommand`는 `MainViewModel.ToggleUseIndividualFormatting`을 호출한다.
- `ToggleUseIndividualFormatting`은 선택된 큐 항목을 record copy로 교체하면서 `UseIndividualFormatting` 플래그를 반전한다.
- 개별 텍스트/색/배경/강조 명령은 `MainViewModel.IsPerItemFormattable`을 통해 활성화 여부가 결정된다.
- 수정 전에는 이 게이트가 `Lyrics`가 있는 `Song`과 `Bible`만 허용했고, `Lyrics`가 있는 `Notice` 텍스트 파일 항목은 제외했다.

## 위험

ViewModel 체크박스 명령은 직접 단위 테스트에서 통과할 수 있다. 그러나 선택 항목 종류가 `IsPerItemFormattable`에서 제외되면 운영자는 설정 화면을 보면서도 실제 개별 설정을 사용할 수 없다.

이번 문제의 핵심은 의미 불일치다. 텍스트 파일 항목은 렌더링 경로에서 `LiveItemKinds.Notice`로 표현되지만, 기존 서식 게이트는 `Notice`를 본문 텍스트 항목으로 보지 않았다.

## 접근

- `ToggleUseIndividualFormatting`은 계속 단일 플래그 변경 경로로 유지한다.
- `Lyrics`가 비어 있지 않은 `LiveItemKinds.Notice`를 `IsPerItemFormattable`에 포함한다.
- 본문이 없는 placeholder 항목과 PowerPoint/미디어 같은 시각 전용 항목은 계속 서식 불가로 둔다.
- code-behind로 우회하지 않고, ViewModel 게이트만 최소 수정한다.

## 제약

- ViewModel 명령을 code-behind로 우회하지 않는다.
- 라이브 재송출 동작을 제거하지 않는다.
- `UseIndividualFormatting` 기본 의미를 바꾸지 않는다.
- 관련 없는 개별 서식 컨트롤은 변경하지 않는다.
- 이 변경에서 PowerPoint/미디어/이미지 항목을 텍스트 서식 대상으로 만들지 않는다.
