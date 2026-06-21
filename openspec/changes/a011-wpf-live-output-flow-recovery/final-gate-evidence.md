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

## 2026-06-22 후속 UAT 이슈: Text Live 전체화면/사도신경 viewport 렌더링

사용자 실제 UAT에서 Text Live가 여전히 모니터 전체화면으로 송출되지 않고, 사도신경 문단 선택 후 렌더링이 WinForms 대비 한쪽으로 치우쳐 보이는 문제가 보고되어 같은 change의 Phase 10으로 후속 처리했다.

### 추가 원인

- WinForms는 `PreviewItemToLive` → `CopyPreviewToOutput` → `GoLive` → `Start_Presentation`에서 `DisplayInfo.SizeLaunchDisplay()`를 통해 출력 모니터의 실제 좌표/크기와 버퍼 크기를 먼저 확정한 뒤 `gfDisplay.DrawText`가 그 크기를 기준으로 본문 영역을 계산한다.
- WPF `PreviewToLiveAsync`와 `StartOutputLiveAsync` 일부 경로는 출력창이 닫혀 있을 때 `OpenOutput()`을 호출해 운영자용 windowed 출력창을 먼저 만들 수 있었다.
- WPF `OutputWindow.xaml` 본문 묶음에 `MaxWidth=1160`, `MaxHeight=980`, 본문 `MaxHeight=660` 같은 고정 제약이 남아 있어 실제 1080p/프로젝터 폭을 충분히 사용하지 못했다.
- 기존 WPF 설정 파일이 이미 있으면 WinForms 레지스트리의 `OutputmonitorName`, `AlwaysTryDualMonitor`, `LyricsMonitorFontSize`가 런타임에 다시 반영되지 않아 실제 배포 환경의 출력 모니터/텍스트 설정과 어긋날 수 있었다.

### 추가 수정

- Text/성경/Notice Live 시작 시 출력창이 닫혀 있으면 `EnsureLiveOutputDisplay()`를 사용해 WinForms처럼 실제 출력 모니터 전체화면 배치를 보장했다.
- `SettingsBootstrapMigrationService`가 기존 WPF 설정 파일이 있어도 WinForms 레지스트리의 출력 모니터, 보조 모니터 우선 정책, Live 텍스트 폰트 크기를 갱신하도록 확장했다.
- WinForms 실제 레지스트리 casing인 `OutputmonitorName`을 OpenSpec/WPF legacy alias에 추가했다.
- WinForms 960px 기준 폰트 크기 값이 WPF 검증 최소값보다 작을 수 있어 `LyricsMonitorFontSize`/`LyricsMonitorFontSize2` legacy import는 WPF 허용 범위로 정규화했다.
- `OutputWindowViewModel`에 `BodyMaxWidth`, `BodyMaxHeight`, `BodyTextMaxHeight`, `BodyText2MaxHeight`를 추가하고, `OutputWindow.xaml` 본문/외곽선/Region2/Interlace 고정 제약을 출력 viewport 기반 바인딩으로 교체했다.

### 추가 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~SettingsBootstrapMigrationServiceTests|FullyQualifiedName~PreviewToLiveCommand_TextItem_OpensSelectedOutputMonitorFullScreen|FullyQualifiedName~ApplyOutputAndSession_ActiveText_UsesOutputViewportForBodyBoundsLikeFrmMain" -v minimal
```

- 결과: 통과
- 테스트 수: 7개

### 추가 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,446개

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

- 1차 결과: 실패. 실행 중인 `EasislidesNext (PID 41092)`가 `C:\EasiSlides\EasislidesNext\EasislidesNext.dll`을 잠그고 있었다.
- 조치: 해당 WPF 앱 프로세스를 종료한 뒤 재시도.
- 최종 결과: 통과
- 배포 위치: `C:\EasiSlides\EasislidesNext`

## 2026-06-22 후속 UAT 이슈: 사도신경 외부 Text 파일 단락 선택 불일치

사용자 실제 UAT에서 사도신경 같은 외부 `.txt` 항목이 WinForms처럼 단락 단위로 선택되지 않고 전체 본문으로 취급되는 문제가 보고되어, 같은 change의 Phase 9로 처리했다.

### 추가 원인

- WinForms는 `.txt` 외부 파일을 WorshipList에 `T + 파일경로`로 저장하고, `Gf.LoadIndividualData`에서 파일 내용을 `CompleteLyrics`로 읽어 가사/구절과 같은 빈 줄 기준 페이지 선택 흐름을 사용한다.
- WPF는 `.txt` 파일 추가 시 `AddTextItem`으로 일반 Notice 항목을 만들고, ESW `T` 항목도 Notice 전체 본문으로 계산해 사도신경 전체가 한 번에 Preview/Live 대상이 되었다.
- 단순히 `Kind == "T"` 전체를 단락형으로 바꾸면 파일 경로가 없는 레거시 텍스트 `T` 항목의 `[공지]` 같은 첫 줄이 가사 포맷터에 의해 빠지는 회귀가 발생한다.

### 추가 수정

- `WorshipListPanel.AddTextFileAsNoticeAsync`가 외부 `.txt`를 `MainViewModel.AddTextFileItem`으로 추가하도록 변경했다.
- `AddTextFileItem`은 파일명을 제목으로 유지하고 `ContentPath`와 본문을 보존하되, 항목 ID를 `text-file:`로 구분해 외부 파일 텍스트임을 명확히 했다.
- `MainViewModel`과 `LiveSessionService`는 `esw:T:` 또는 `text-file:` 항목만 빈 줄 기준 단락 페이지로 계산한다.
- 일반 Notice와 파일 경로 없는 레거시 `Kind == "T"` 텍스트는 기존처럼 본문을 그대로 송출하도록 보존했다.

### 추가 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~GoLive_LegacyTextKind_ProjectsLiteralText|FullyQualifiedName~ImportEswWorshipList_TextFile_UsesParagraphPagesLikeFrmMain|FullyQualifiedName~AddTextFileItem_UsesFileNameTitleAndParagraphPages|FullyQualifiedName~GoLive_NoticeItem_RendersTextVerbatim_NotThroughLyricsFormatter" -v minimal
```

- 결과: 통과
- 테스트 수: 4개
- 사전 Red 확인: `ImportEswWorshipList_TextFile_UsesParagraphPagesLikeFrmMain` 추가 직후 `LyricsPageCount`가 0으로 실패함을 확인했다.

### 추가 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,444개

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

## 2026-06-22 후속 UAT 이슈: Text/InfoScreen Live Display 미오픈

사용자 실제 UAT에서 PPT는 Live되지만 Text/성경 같은 텍스트 계열 항목이 Display에 Live되지 않는 문제가 다시 보고되어 같은 change의 Phase 8로 후속 처리했다.

### 추가 원인

- WinForms `btnToLive_Click` 흐름은 `PreviewItemToLive` → `CopyPreviewToOutput` → `GoLive(true)` → `Start_Presentation`으로 이어지며, 텍스트/공지/성경도 출력 쇼가 닫혀 있으면 먼저 Display를 열고 즉시 송출한다.
- WPF `PublishNotice`는 본문이 있어도 `_output.Current.IsOpen == false`이면 즉시 `false`를 반환했다.
- 이 때문에 Text/InfoScreen 계열 Live가 실제 Display 창을 열 기회를 얻지 못했고, PPT는 별도의 PowerPoint SlideShow 경로가 있어 상대적으로 동작하는 것처럼 보였다.

### 추가 수정

- `MainViewModel.PublishNotice`가 출력 창 닫힘을 실패 조건으로 보지 않고 `EnsureLiveOutputDisplay()`를 호출해 WinForms처럼 전체화면 Display를 먼저 준비하도록 수정했다.
- `PublishNotice_WhenOutputClosed_OpensDisplayAndSendsNoticeBodyLiveLikeFrmMain` 회귀 테스트를 추가해 Text/InfoScreen Live가 닫힌 Display를 열고 `OutputWindowHost`가 바인딩한 VM까지 본문을 표시하는지 검증했다.
- `GoLiveCommand_BibleItem_PublishesBodyToDisplayHost` 회귀 테스트를 추가해 성경 본문이 PowerPoint 경로 없이 WPF Display VM의 텍스트 레이어까지 도달하는지 검증했다.

### 추가 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~PublishNotice_WhenOutputClosed_OpensDisplayAndSendsNoticeBodyLiveLikeFrmMain|FullyQualifiedName~GoLiveCommand_BibleItem_PublishesBodyToDisplayHost|FullyQualifiedName~PublishNotice_WhenOutputOpen_SendsNoticeBodyLive" -v minimal
```

- 결과: 통과
- 테스트 수: 3개

### 추가 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,442개

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
