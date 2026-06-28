# 검증 계획

## 자동 검증

- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~MainViewModelTests" --no-restore`
- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~OutputRendererTests|FullyQualifiedName~OutputWindowViewModelTests" --no-restore`
- `dotnet build Easislides.Wpf/Easislides.Wpf.csproj --no-restore`
- `openspec validate a015-wpf-preview-live-background-parity --strict`

## 수동 검증

- 앱 실행 후 사도신경 항목 선택
- `Use Individual Settings` 켬/끔 상태 각각 확인
- 미리보기 배경과 Live Show 배경 캡처 저장
- 캡처 위치: `evidence/screenshots/2026-06-23/a015-wpf-preview-live-background-parity/`

## 실행 결과

- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~PreviewSampleAppearance_WhenIndividualFormattingOff_IgnoresItemBackgroundImage" --no-restore`
  - Red: 실패 확인. 기대 default 배경 대신 item 배경을 읽음.
  - Green: 구현 후 통과.
- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~PreviewSampleAppearance_WhenIndividualFormattingOff_IgnoresItemBackgroundImage|FullyQualifiedName~PreviewToLiveCommand_WithIndividualFormattingOn_CarriesPreviewBackgroundToLiveSession" --no-restore`: 통과, 2개 테스트
- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~ToggleOutputLiveCommand_AfterCopyPreviewToOutput_PublishesSelectedPageAndFormatting|FullyQualifiedName~SetSelectedItemBackgroundImage_WhileLive_CarriesOverridePath|FullyQualifiedName~GoLive_WithSongFormatDataBackgroundImage_CarriesImagePath|FullyQualifiedName~CreateScene_Active_SongBackgroundImage_WinsOverGlobal" --no-restore`: 통과, 4개 테스트
- `dotnet build Easislides.Wpf/Easislides.Wpf.csproj --no-restore`: 통과, 오류 0개, 기존 NetOffice 호환성 경고만 출력
- `openspec validate a015-wpf-preview-live-background-parity --strict`: 통과

## 실제 앱 캡처

- 저장 위치: `evidence/screenshots/2026-06-23/a015-wpf-preview-live-background-parity/`
- `01-apostles-creed-selected-preview.png`: 앱 시작 직후 로딩 진행 창이 캡처됨.
- `03-main-window-restarted.png`, `04-virtual-screen-main-loaded.png`, `05-window-moved-restored.png`, `06-printwindow-after-preview-go-live.png`: UIA 트리에는 메인 메뉴, 예배 순서, 사도신경, `Preview Go Live` 버튼이 존재했지만 화면 캡처는 흰 창으로만 저장됨.
- UIA 상태 확인: `Preview Go Live` 실행 후 트리에서 `LIVE: ★★★사도신경★★★.txt`, `LIVE 1/10`, 사도신경 본문 카드가 확인됨.

## 남은 확인 필요

- 현재 세션의 화면 캡처 방식에서는 WPF 시각 레이어가 흰 창으로만 저장되어, 실제 배경 이미지의 눈 검증을 완료하지 못했다.
- 사용자 화면 또는 정상 렌더링되는 환경에서 사도신경 `Use Individual Settings=true` 상태의 왼쪽 미리보기와 오른쪽 Live Show 배경을 다시 캡처해야 한다.
