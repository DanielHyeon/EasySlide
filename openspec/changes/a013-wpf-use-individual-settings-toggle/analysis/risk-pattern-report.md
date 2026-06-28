# 위험 패턴 보고서

## ast-grep

명령:

```powershell
ast-grep scan --rule ast-grep\rules\csharp-risk-patterns.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf.Tests\Shell\MainViewModelTests.cs
```

결과: 통과, 발견 항목 없음.

## SDD 불변식

이번 변경은 다음 영역을 수정하지 않아야 한다.

- `LiveSessionService.GoLive(...)`
- 출력 위치 계산
- 모니터 선택
- DB 접근
- COM interop
- 예배 목록 저장 스키마

## UI/ViewModel 게이트 위험

- `IsChecked`는 단방향 바인딩이므로, 화면 체크박스가 실제 운영자 변경을 반영하는지 별도 확인이 필요하다.
- `ToggleUseIndividualFormattingCommand.Execute(...)` 직접 테스트만으로는 XAML 컨트롤 경로가 실제로 상호작용 가능한지 증명할 수 없다.
- 선택 항목은 record이며, 토글 시 큐 항목을 교체한다. 따라서 `SelectedItem` 변경 후 UI 바인딩이 갱신되어야 한다.
- 개별 설정 컨트롤은 `IsPerItemFormattable`로도 게이트된다. 화면 패널이 보여도 렌더링되는 텍스트 항목 종류가 이 함수에서 제외되면 설정은 사용할 수 없다.
- 본문이 있는 `Notice`/텍스트 파일 항목은 서식 가능해야 하고, PowerPoint/미디어 같은 시각 항목은 계속 서식 불가여야 한다.
