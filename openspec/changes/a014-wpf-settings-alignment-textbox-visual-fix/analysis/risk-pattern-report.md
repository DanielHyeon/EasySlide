# 위험 패턴 보고서

## ast-grep 실행

```text
sg scan --rule ast-grep/rules/csharp-risk-patterns.yml Easislides.Wpf/MainWindow.xaml.cs Easislides.Wpf/Shell/MainViewModel.cs
```

결과: 위험 패턴 발견 없음.

```text
sg scan --rule ast-grep/rules/sdd-invariants.yml Easislides.Wpf/MainWindow.xaml.cs Easislides.Wpf/Shell/MainViewModel.cs
```

결과: live text monitor 세션 변경 지점이 기존 invariant checkpoint로 보고됐다. 이번 변경은 해당 함수들을 수정하지 않으므로 직접 영향 없음.

## XAML 위험

- 공통 TextBox 템플릿 수정은 앱 전체 입력 상자에 영향을 줄 수 있다.
- 설정 패널 TextBox를 너무 크게 만들면 레거시 패널 밀도가 낮아질 수 있다.
- `CommandParameter`와 `AutomationProperties.Name`은 유지해야 한다.

