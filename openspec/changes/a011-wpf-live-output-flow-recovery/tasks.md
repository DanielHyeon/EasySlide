## 1. Phase 0 - 기준 흐름 및 영향 분석

- [x] 1.1 CodeGraph로 WPF Live 명령, Output surface 갱신, PPT SlideShow 시작 경로의 영향 범위를 기록한다.
- [x] 1.2 WinForms `FrmMain`의 `btnToLive_Click`, `PreviewItemToLive`, `cbGoLive_Click`, `GoLive`, `Start_Presentation` 기준 흐름을 이번 change 문서에 요약한다.
- [x] 1.3 현재 WPF Live 불능의 재현 조건을 코드/테스트 관점에서 특정한다.

## 2. Phase 1 - 실패 테스트 작성

- [x] 2.1 Preview 텍스트 항목 Live 시작 시 출력 창이 열리고 Output surface 현재 항목이 갱신되는 실패 테스트를 작성한다.
- [x] 2.2 Output 항목이 비어 있을 때 Output Live가 WorshipList 첫 항목을 송출하는 실패 테스트를 작성한다.
- [x] 2.3 PPT Preview 항목 Live 시작 시 선택된 출력 모니터 장치명으로 SlideShow 시작 요청이 발생하는 실패 테스트를 작성한다.

## 3. Phase 2 - 최소 구현

- [x] 3.1 Preview Live 명령이 WinForms `PreviewItemToLive`처럼 Output 상태와 출력 surface를 함께 갱신하도록 보정한다.
- [x] 3.2 Output Live 명령이 WinForms `Start_Presentation`처럼 Output이 비어 있으면 첫 WorshipList 항목을 준비하도록 보정한다.
- [x] 3.3 PPT Live 시작 요청이 선택된 출력 모니터와 현재 PPT 항목을 사용하도록 보정한다.

## 4. Phase 3 - 회귀 검증 및 배포

- [x] 4.1 Live 관련 집중 테스트와 전체 `Easislides.Wpf.Tests`를 통과시킨다.
- [x] 4.2 `openspec validate a011-wpf-live-output-flow-recovery --strict`를 통과시킨다.
- [x] 4.3 `dotnet build Easislides\Easislides.csproj -nologo -v minimal`을 통과시킨다.
- [x] 4.4 WPF Release를 `C:\EasiSlides\EasislidesNext`에 publish한다.

## 5. Phase 4 - 증거 정리 및 전달

- [x] 5.1 CodeGraph impact와 검증 결과를 change 문서에 남긴다.
- [x] 5.2 커밋 및 푸시 전 변경 범위를 확인한다.
- [x] 5.3 수정 내용, 검증 결과, 남은 수동 UAT 항목을 보고한다.

## 6. Phase 5 - 실제 UAT 후속: Text/PPT Live 불능

- [x] 6.1 Text Live 불능 원인을 레거시 `.esw` 타입 코드와 Live 본문 계산 경로 기준으로 재점검한다.
- [x] 6.2 `LiveItemKindMatcher`가 Song/Bible/Notice 레거시 별칭까지 판별하도록 확장하고, Live 본문 계산과 Output 표면 본문 계산이 같은 판별 규칙을 쓰게 한다.
- [x] 6.3 PPT Live가 PowerPoint 편집 창만 열리고 SlideShow가 안 뜨는 경로를 WinForms `RunPowerpointSong` 기준으로 보강한다.
- [x] 6.4 Live 버튼 경로가 PPT SlideShow 시작 Task를 기다리고, 실패 시 메시지 박스 없이 상태바/텔레메트리에 실패를 남기도록 한다.
- [x] 6.5 Text 레거시 타입과 PPT SlideShow 실패 전파 회귀 테스트를 추가하고 집중 테스트를 통과시킨다.
- [x] 6.6 전체 WPF 테스트, WinForms 빌드, WPF Release 배포를 다시 수행한다.

## 7. Phase 6 - Live 종료 시 PowerPoint 종료

- [x] 7.1 Stop/Live off 경로와 PowerPoint SlideShow control 영향 범위를 확인한다.
- [x] 7.2 Live 종료 시 PowerPoint SlideShow stop/close 요청이 발생하는 실패 테스트를 추가한다.
- [x] 7.3 `IPowerPointSlideShowControl`에 종료 API를 추가하고 Office 구현이 SlideShow와 PowerPoint application을 닫게 한다.
- [x] 7.4 `StopLiveAsync`가 사용자 확인 후 PowerPoint 종료를 기다리고, 실패해도 Live 세션은 안전하게 Off로 전환하게 한다.
- [x] 7.5 집중 테스트, 전체 WPF 테스트, OpenSpec 검증, WinForms 빌드, WPF Release 배포를 수행한다.

## 8. Phase 7 - Text/Bible Live 본문 공백 폴백

- [x] 8.1 Text/Notice와 Bible Live 경로가 빈 본문을 만들 수 있는 지점을 재점검한다.
- [x] 8.2 Notice 본문 없음, Bible 본문 확장 실패, GoLiveCommand Text/Bible 송출 경로에 회귀 테스트를 추가한다.
- [x] 8.3 `LiveSessionService`와 `MainViewModel` Output 표면 본문 계산이 같은 폴백 규칙을 쓰도록 정렬한다.
- [x] 8.4 집중 테스트, 전체 WPF 테스트, OpenSpec 검증, WinForms 빌드, WPF Release 배포를 수행한다.

## 9. Phase 8 - WinForms Text/InfoScreen Live Display 동작 정합

- [x] 9.1 WinForms `PreviewItemToLive`/`CopyPreviewToOutput`/`GoLive`/`Start_Presentation` 기준 Text/InfoScreen Live 흐름을 재분석한다.
- [x] 9.2 WPF `PublishNotice`와 Text/Bible `GoLiveCommand`가 실제 `OutputWindowHost` Display VM까지 본문을 전달하는 회귀 테스트를 추가한다.
- [x] 9.3 출력 창이 닫힌 상태에서도 Text/InfoScreen Live가 WinForms처럼 Display를 열고 즉시 송출되도록 수정한다.
- [x] 9.4 집중 테스트, 전체 WPF 테스트, OpenSpec 검증, WinForms 빌드, WPF Release 배포를 수행한다.
