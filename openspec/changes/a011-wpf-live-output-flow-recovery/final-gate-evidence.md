# Final Gate Evidence

## 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~OutputWindowHostTests|FullyQualifiedName~PreviewWindowHostTests|FullyQualifiedName~GoLiveCommand_WhenOutputClosed_OpensOutputAndStartsPreviewLikeFrmMain|FullyQualifiedName~PreviewToLiveCommand_OpensOutputAndPublishesPreviewWithoutAdvancingSelection|FullyQualifiedName~ToggleOutputLiveCommand_WhenNoPreparedOutput_StartsFirstWorshipItemLikeFrmMain|FullyQualifiedName~GoLiveCommand_PowerPoint_StartsSlideShowOnSelectedOutputMonitor" -v minimal
```

- 결과: 통과
- 테스트 수: 19개

## 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,433개

## OpenSpec 검증

```powershell
openspec validate a011-wpf-live-output-flow-recovery --strict
```

- 결과: 통과

## WinForms 빌드

```powershell
dotnet build Easislides\Easislides.csproj -nologo -v minimal
```

- 결과: 통과
- 경고: 기존 NetOffice/DirectShow/WinForms analyzer 경고 유지

## WPF 배포

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -v minimal
```

- 결과: 통과
- 배포 위치: `C:\EasiSlides\EasislidesNext`

## 남은 수동 UAT

- 실제 멀티모니터에서 Output 창을 X/Alt+F4로 닫은 뒤 Preview Live 재시도.
- 실제 PPT 항목에서 PowerPoint SlideShow가 선택 출력 모니터에 뜨는지 확인.

## 2026-06-22 후속 UAT 이슈: Text/PPT Live 불능

사용자 실제 UAT에서 Text Live가 안 되고, PPT는 PowerPoint만 실행되고 SlideShow가 시작되지 않는 문제가 보고되어 같은 change의 Phase 5로 후속 처리했다.

### 추가 원인

- Text/Notice/Bible/Song 본문 계산 일부가 `LiveItemKinds.*` 정규 문자열만 비교해 레거시 `.esw` 타입 코드(`T`, `I`, `W`, `B`, `D` 등)와 불일치할 수 있었다.
- WPF Live 버튼 경로가 PowerPoint SlideShow 시작 Task를 fire-and-forget으로 호출해 COM 실패가 `Debug.WriteLine`에만 남고 운영 상태에는 표시되지 않았다.
- COM 래퍼는 SlideShow 창 생성 실패(false)를 상위 컨트롤에서 실패로 승격하지 않았다.

### 추가 수정

- `LiveItemKindMatcher`에 `IsSong`, `IsBible`, `IsNotice`를 추가하고 Live 본문 계산/Output 표면 본문 계산이 같은 판별 규칙을 사용하도록 정렬.
- `OfficePowerPointSlideShowControl`이 SlideShow 창 미생성 결과를 예외로 승격.
- `OfficePptSession`이 WinForms 흐름처럼 SlideShow 범위/PresenterView/ShowType/AdvanceMode를 명시하고 `Run()` 후 `First()` 또는 `GotoSlide()`를 수행.
- `GoLiveCommand`, Output Live 시작, 송출 후 다음 경로는 PPT SlideShow 시작 완료를 기다리며 실패 시 메시지 박스 없이 상태바에 실패를 표시.

### 추가 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~LiveSessionServiceTests|FullyQualifiedName~GoLiveCommand_PowerPoint" -v minimal
```

- 결과: 통과
- 테스트 수: 63개

### 추가 OpenSpec 검증

```powershell
openspec validate a011-wpf-live-output-flow-recovery --strict
```

- 결과: 통과

### 추가 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,435개

### 추가 WinForms 빌드

```powershell
dotnet build Easislides\Easislides.csproj -nologo -v minimal
```

- 결과: 통과
- 경고: 기존 NetOffice/DirectShow/WinForms analyzer 경고 유지

### 추가 WPF 배포

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -v minimal
```

- 결과: 통과
- 배포 위치: `C:\EasiSlides\EasislidesNext`
