# Phase 8 PARTIAL/BLOCKED Closure

작성일: 2026-06-21

대상: `a010-wpf-frmmain-1to1-operator-console-parity`

## 요약

manual UAT에서 남은 PARTIAL 25개와 BLOCKED 12개를 다시 확인했다.

| 구분 | 개수 | 판정 |
| --- | ---: | --- |
| PARTIAL 중 코드 보정 완료 | 3 | UAT-141, UAT-161, UAT-211 |
| PARTIAL 중 구현/테스트 증거로 닫음 | 19 | UAT-103, UAT-104, UAT-134, UAT-135, UAT-205~208, UAT-303, UAT-402, UAT-404, UAT-411, UAT-501~507 |
| PARTIAL 중 외부 송출 모니터 관찰 필요 | 3 | UAT-406, UAT-407, UAT-408 |
| BLOCKED 중 외부 데이터/설정 필요 | 12 | UAT-111, UAT-112, UAT-121~124, UAT-151, UAT-152, UAT-212, UAT-213, UAT-306, UAT-405 |

결론: 코드 미구현으로 남은 PARTIAL은 없다. 현 환경에서 닫지 못한 항목은 외부 송출 모니터 직접 관찰 또는 실제 `C:\EasiSlides` 운영 데이터/설정이 필요한 항목이다.

## 코드 보정 완료

| UAT | 조치 | 증거 |
| --- | --- | --- |
| UAT-141 Images 탭 | PowerPoint용 큰 썸네일 sizing과 분리해 `LegacyImageThumbnailSizeConverter`를 추가하고 Images 탭은 좁은 source rail에서 3열 compact thumbnail을 사용하도록 수정했다. | `wpf-phase8-images.png`, `LegacyImageThumbnailSizeConverterTests` |
| UAT-161 Default 탭 | `Apply to All` 그룹을 FrmMain처럼 Default 첫 화면 상단으로 이동하고, Text 그룹 높이를 제한해 `Background / Transition`과 Display Panel 계열 컨트롤이 첫 화면에 더 빨리 노출되도록 수정했다. | `wpf-phase8-default-after-density-fix.png`, `MainMenuBarTests` |
| UAT-211 Praise Book 탭 | Praise Book toolbar의 `Open`/`Refresh`를 compact 버튼으로 줄이고, status/window row가 entry list와 겹치던 Grid row 누락을 수정했다. | `wpf-phase8-praise-book-after-row-fix.png`, `MainMenuBarTests` |

## 구현/테스트 증거로 닫은 PARTIAL

| 그룹 | UAT | 근거 |
| --- | --- | --- |
| Source gesture | UAT-103, UAT-104 | `LibrarySongList`는 `MouseBinding MouseAction=LeftDoubleClick`으로 `AddSelectedLibrarySongCommand`를 실행하고, `LibrarySongList_PreviewMouseMove`는 `SongSummary` typed payload drag를 시작한다. `MainMenuBarTests`가 경로를 고정한다. |
| Bible gesture | UAT-134, UAT-135 | `BiblePassageBox_MouseLeftButtonUpAfterTextBox`는 Shift range 선택을 처리하고, `BiblePassageBox_PreviewMouseMove`는 `BibleSelection` typed payload drag를 시작한다. `MainMenuBarTests`가 selection/add/drag 경로를 고정한다. |
| Worship List operation | UAT-205~208 | `WorshipListPanel`은 move up/down, typed drag/drop, context menu Play/Play on Output command surface를 보유한다. `WorshipListPanelTests`와 `MainViewModelTests`가 routing/ordering을 검증한다. |
| Preview/Output jump | UAT-303, UAT-402, UAT-404 | Preview/Output verse/slide command와 named section button surface가 XAML과 VM tests에 존재한다. manual UAT의 disabled/AutomationId 이슈는 선택 item/page 조건 문제로 재분류했다. |
| Live message | UAT-411 | `OutputTextBoxLM`, `OutputBtnLMSend`, `OutputBtnLMClear` 및 `SendLiveMessageCommand`/`ClearLiveMessageCommand` 테스트가 존재한다. |
| Keyboard | UAT-501~507 | Phase 6 shortcut tests가 F12/F11/F9/F3/Space/Shift+Space/number routing과 text-input blocking을 검증한다. |

## 외부 조건이 필요한 항목

| 구분 | UAT | 닫을 수 없는 이유 |
| --- | --- | --- |
| 외부 송출 모니터 | UAT-406~408 | operator surface와 renderer state는 검증됐지만, 실제 회중용 외부 모니터의 black/clear/hide/restore 시각 결과는 현재 세션에서 독립 관찰할 수 없다. |
| InfoScreen data | UAT-111, UAT-112 | `C:\EasiSlides\InfoScreens` 파일 수 0. 사용자 데이터 생성/삭제/변경 금지 원칙에 따라 임의 샘플을 만들지 않았다. |
| PowerPoint data/config | UAT-121~124, UAT-306, UAT-405 | 현재 `UsePowerPointTab=false`, `C:\EasiSlides\Powerpoint` 파일 수 0. |
| Media data/config | UAT-151, UAT-152 | 현재 `UseMediaTab=false`, `C:\EasiSlides\Media` 파일 수 0. |
| PraiseBook entries | UAT-212, UAT-213 | 기존 선택 PraiseBook이 0곡으로 표시된다. entry가 있는 실제 PraiseBook 데이터가 있어야 add/delete/export manual UAT를 수행할 수 있다. |

## 검증

- CodeGraph impact: `MainWindow` impact 확인. 영향 범위가 WPF MainWindow shell과 관련 tests 중심임을 확인했다.
- Focused test: `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~LegacyImageThumbnailSizeConverterTests|FullyQualifiedName~MainMenuBarTests" -v minimal`
  - 결과: 실패 0, 통과 137, 건너뜀 0.
- 캡처:
  - `evidence/screenshots/2026-06-21/phase8/wpf-phase8-images.png`
  - `evidence/screenshots/2026-06-21/phase8/wpf-phase8-default-after-density-fix.png`
  - `evidence/screenshots/2026-06-21/phase8/wpf-phase8-praise-book-after-row-fix.png`
