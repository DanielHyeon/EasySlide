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

## 2026-06-22 후속 UAT 이슈: Live 종료 시 PPT 종료

사용자 실제 UAT에서 Live 취소/종료 시 PowerPoint SlideShow가 멈추고 PowerPoint도 종료되어야 한다는 요구가 추가되었다.

### 추가 원인

- `StopLiveAsync`는 WPF Live session과 Output PPT preview 상태만 정리하고, 실제 PowerPoint SlideShow control/COM session에는 종료 요청을 보내지 않았다.
- `IPowerPointSlideShowControl`에는 Start/TriggerNext만 있고 Stop/Close 계약이 없어 WinForms `ClearUpPowerpointWindows`/`QuitPowerPointApp`에 대응하는 종료 경로가 없었다.

### 추가 수정

- `IPowerPointSlideShowControl.StopAsync`를 추가했다.
- `OfficePowerPointSlideShowControl.StopAsync`는 생성된 `OfficePptSession.CloseAsync()`를 호출해 PowerPoint application을 종료한다.
- `StopLiveAsync`는 사용자 Stop 확인 후 `StopAsync`를 await하고, 종료 실패 시에도 Live session은 Off로 전환하되 상태바에 실패 사유를 남긴다.

### 추가 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~StopLiveCommand_PowerPointLive|FullyQualifiedName~StopLiveCommand_WhenPowerPointCloseFails|FullyQualifiedName~GoLiveCommand_PowerPoint|FullyQualifiedName~LiveItemId_ReflectsPublishedItem" -v minimal
```

- 결과: 통과
- 테스트 수: 5개

### 추가 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,437개

### 추가 OpenSpec 검증

```powershell
openspec validate a011-wpf-live-output-flow-recovery --strict
```

- 결과: 통과

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

- 1차 결과: 실패. 실행 중인 `EasislidesNext (PID 38064)`가 `C:\EasiSlides\EasislidesNext\EasislidesNext.dll`을 잠그고 있었다.
- 조치: 해당 WPF 앱 프로세스를 종료한 뒤 재시도.
- 최종 결과: 통과
- 배포 위치: `C:\EasiSlides\EasislidesNext`

## 2026-06-22 후속 UAT 이슈: Text/Bible 본문 Live 미표시

사용자 실제 UAT에서 텍스트와 성경 문구가 여전히 Live 송출 화면에 나타나지 않는 문제가 보고되어 같은 change의 Phase 7로 후속 처리했다.

### 추가 원인

- Text/Notice 항목은 자유 텍스트 본문이 비어 있으면 Live 본문도 빈 문자열이 되어 출력 렌더러가 본문 없음으로 판단할 수 있었다.
- 성경 항목 생성부 주석은 본문 확장 실패 시 제목 폴백을 의도하고 있었지만 실제 코드는 `Lyrics = body`만 저장했다.
- 성경 DB/작업 폴더/본문 확장 실패 시 `Bible.ExpandSelectionBody(...)`가 빈 문자열을 반환하면 Live session과 Output 표면 텍스트가 모두 빈 상태가 될 수 있었다.

### 추가 수정

- `LiveSessionService`의 Notice/Bible 본문 계산에 `TextOrTitle` 폴백을 적용해 빈 Live 본문을 방지했다.
- `MainViewModel.CreateBibleItem`이 성경 본문 확장 실패 시 선택 제목(예: `요한복음 3:16`)을 `Lyrics`에 저장하도록 수정했다.
- `MainViewModel.BuildOutputSurfaceText`도 Live session과 같은 폴백 규칙을 사용하게 정렬했다.

### 추가 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~GoLive_NoticeWithoutLyrics_UsesTitleFallbackInsteadOfBlankScreen|FullyQualifiedName~GoLive_WithBibleItemEmptyBody_UsesReferenceFallbackInsteadOfBlankScreen|FullyQualifiedName~GoLiveCommand_TextItem_PublishesLiteralBodyToOutput|FullyQualifiedName~GoLiveCommand_BibleWithoutExpandedBody_PublishesReferenceInsteadOfBlankOutput" -v minimal
```

- 결과: 통과
- 테스트 수: 4개

### 추가 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,441개

### 추가 OpenSpec 검증

```powershell
openspec validate a011-wpf-live-output-flow-recovery --strict
```

- 결과: 통과

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
