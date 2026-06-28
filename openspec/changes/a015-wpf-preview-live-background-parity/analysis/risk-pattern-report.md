# ast-grep 위험 패턴 보고

## 실행 명령

- `sg scan --rule ast-grep/rules/csharp-risk-patterns.yml Easislides.Wpf/Shell/MainViewModel.cs Easislides.Wpf/Shell/LiveSessionService.cs Easislides.Wpf/Rendering/OutputRenderer.cs`
- `sg scan --rule ast-grep/rules/sdd-invariants.yml Easislides.Wpf/Shell/MainViewModel.cs Easislides.Wpf/Shell/LiveSessionService.cs Easislides.Wpf/Rendering/OutputRenderer.cs`

## 결과

- `csharp-risk-patterns.yml`: 위험 패턴 출력 없음.
- `sdd-invariants.yml`: `_session.GoLive(...)`, `_session.UpdateHiddenContent(...)` 호출 지점들이 Live text monitor invariant checkpoint로 보고됨.

## 해석

이번 변경은 송출 세션 mutation 지점 자체를 넓게 바꾸지 않는다. 우선 미리보기 샘플이 읽는 `FormatData`의 유효성 판단을 Live projection과 맞추는 방식으로 위험 범위를 제한한다.
