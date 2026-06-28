# 제안: WPF Use Individual Settings 텍스트 항목 서식 활성화

## 문제

WPF 미리보기 서식 패널에는 사도신경 같은 텍스트 파일/공지 항목에도 `Use Individual Settings` 관련 화면이 표시된다. 그러나 실제로는 개별 설정을 바꿀 수 없었다.

CodeGraph와 회귀 테스트 확인 결과, 체크박스 명령 자체는 `UseIndividualFormatting` 값을 토글할 수 있었다. 실제 원인은 항목별 서식 가능 여부를 판단하는 `MainViewModel.IsPerItemFormattable`이 곡과 성경만 허용하고, 본문이 있는 `Notice` 텍스트 항목을 제외한 데 있었다.

## 범위

- 본문이 있는 `Notice`/텍스트 파일 항목도 개별 서식 설정을 사용할 수 있도록 WPF ViewModel 게이트를 수정한다.
- 기존 ViewModel 동작인 큐 항목 교체, 선택 항목 갱신, 상태 문구 갱신, 라이브 재송출 경로는 유지한다.
- 선택된 텍스트/공지 항목에서 개별 설정 명령이 실제로 활성화되는 회귀 테스트를 추가한다.

## 제외 범위

- WinForms 코드는 변경하지 않는다.
- 예배 목록 저장 스키마는 변경하지 않는다.
- 항목별 `FormatData` 인코딩 형식은 변경하지 않는다.
- 서식 패널 UI를 재설계하지 않는다.

## 인수 기준

- `MainWindow.xaml`의 `Use Individual Settings` 제어가 선택 항목의 `UseIndividualFormatting` 값을 토글할 수 있다.
- 본문이 비어 있지 않은 `Notice`/텍스트 파일 항목은 개별 글꼴, 색, 배경, 강조 설정을 사용할 수 있다.
- PowerPoint/미디어 같은 비텍스트 시각 항목은 텍스트 서식 게이트 밖에 남아야 한다.
- UI 바인딩은 `SelectedItem.UseIndividualFormatting`과 동기화되어야 한다.
- 기존 직접 명령 테스트는 계속 통과해야 한다.
- production 코드 수정 전에 관련 SDD Step 2 산출물이 존재해야 한다.
