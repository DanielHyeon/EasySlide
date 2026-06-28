# 영향 분석

## CodeGraph 조회

- `codegraph_context`: WPF 미리보기 항목별 배경이 Live Output으로 넘어가지 않는 결함의 송출 흐름 확인
- `codegraph_impact("PreviewToLiveAsync", depth=2)`: `MainViewModel` 송출 상태, `PreviewToLiveCommand`, `OutputItem`, 샘플 서식 속성까지 넓은 영향 확인
- `codegraph_callers("PreviewToLiveAsync")`: RelayCommand/partial 바인딩 구조 때문에 직접 caller 없음
- `codegraph_impact("PrepareOutputFromItem", depth=2)`: `OutputItem` 설정 및 PowerPoint publish 준비 경로 확인
- `codegraph_impact("PublishOutputItemAtPreviewPage", depth=2)`: Live session publish 경로 확인
- `codegraph_impact("ResolveLiveProjection", depth=2)`: `FormatData`를 Live projection에 유지하거나 null 처리하는 핵심 함수 확인
- `codegraph_impact("PrepareLiveItemForOutput", depth=2)`: 텍스트 파일/성경 항목 변환 시 `FormatData` 보존 여부 확인

## 런타임 흐름

`Preview Go Live` 버튼 → `PreviewToLiveCommand` → `PreviewToLiveAsync()` → `PrepareOutputFromPreview()` → `PrepareOutputFromItem()` → `PrepareLiveItemForOutput()` → `PublishOutputItemAtPreviewPage()` → `ResolveLiveProjection()` → `_session.GoLive(...)` → Output renderer/view model.

## 영향 심볼

- `MainViewModel.CreateLyricsSampleBackgroundBrush`
- `MainViewModel.CreateLyricsSampleForegroundBrush`
- `MainViewModel.CreateLyricsSampleFontSize`
- `MainViewModel.CreateLyricsSampleLineHeight`
- `MainViewModel.CreateLyricsSampleFontFamily`
- `MainViewModel.CreateLyricsSampleTextAlignment`
- `MainViewModel.CreateLyricsSampleHorizontalAlignment`
- `MainViewModel.CreateLyricsSampleVerticalAlignment`
- `MainViewModel.ResolveLiveProjection`
- `LiveSessionService.CreateSnapshot`
- `OutputRenderer.CreateScene`
- `OutputWindowViewModel.TryLoadBackgroundImage`

## 결론

수정 대상은 우선 `MainViewModel`의 샘플 서식 계산 계층으로 제한한다. Live 송출 세션과 렌더러에는 이미 항목별 배경 우선순위 테스트가 있으므로, 이번 결함의 1차 원인은 샘플 렌더 규칙과 Live projection 규칙의 불일치로 본다.
