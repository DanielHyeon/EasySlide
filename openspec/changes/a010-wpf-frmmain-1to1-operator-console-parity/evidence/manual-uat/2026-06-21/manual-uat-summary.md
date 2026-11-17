# Manual UAT Summary - 2026-06-21

대상: WPF `MainWindow` / `Easislides.Wpf\bin\Debug\net10.0-windows\EasislidesNext.exe`

데이터: `C:\EasiSlides` 실제 legacy 데이터

## 결과 요약

| Status | Count |
| --- | ---: |
| PASS | 25 |
| PARTIAL | 25 |
| BLOCKED | 12 |
| FAIL | 0 |

총 62개 UAT row를 실제 화면 실행, UIAutomation 조작, 스크린샷 검토로 채웠다.

## 주요 PASS

- Startup/layout/working folder: UAT-001~004.
- Folders 검색 및 실제 곡 로딩: UAT-101, UAT-102.
- Bible 조회 및 예배 순서 추가: UAT-131~133.
- Image background 적용: UAT-142.
- Worship List 선택/삭제: UAT-203, UAT-204.
- Preview 기본 이동/Go Live/Output copy next: UAT-301, UAT-302, UAT-304, UAT-305, UAT-307, UAT-308.
- Output item/restart/refresh: UAT-401, UAT-403, UAT-409, UAT-410.
- Text input focus shortcut guard: UAT-508.

## 주요 PARTIAL

- Gesture parity: song double-click, source drag, Bible drag, Worship List drag/context menu는 안정적인 gesture 증거가 부족하다.
- Visual usability parity: Images, Default, Praise Book은 기능 surface는 있으나 FrmMain 첫 화면 밀도/순서/entry population 차이가 남아 있다.
- Output safety/shortcut: operator surface command 또는 key send는 확인했으나 외부 송출 모니터의 black/clear/hide/restore 시각 결과는 독립 관찰하지 못했다.
- 일부 named section jump: `PreviewBtnVerseChorus`, `OutputBtnVerse1`은 현재 데이터/노출 조건에서 AutomationId를 찾지 못했다.

## BLOCKED

- InfoScreen: `C:\EasiSlides\InfoScreens` 파일 수 0.
- PowerPoint: `UsePowerPointTab=false`, `C:\EasiSlides\Powerpoint` 파일 수 0.
- Media: `UseMediaTab=false`, `C:\EasiSlides\Media` 파일 수 0.
- PraiseBook add/delete/export: 현재 선택된 `PraiseBook 1`이 0곡으로 표시되어 entry 대상 없음.

## Evidence Files

- `manual-uat-start.png`
- `manual-uat-song-search.png`
- `manual-uat-run2-song-search.png`
- `manual-uat-run2-song-add-button.png`
- `manual-uat-bible-lookup-add.png`
- `manual-uat-images-apply.png`
- `manual-uat-run2-images-apply.png`
- `manual-uat-preview-start-song.png`
- `manual-uat-PreviewBtnVerse1.png`
- `manual-uat-PreviewBtnSlideDown.png`
- `manual-uat-PreviewBtnItemDown.png`
- `manual-uat-preview-golive-run3.png`
- `manual-uat-preview-output-next-run3.png`
- `manual-uat-OutputBtnItemDown-run3.png`
- `manual-uat-run2-OutputBtnRestartCurrentItem.png`
- `manual-uat-run2-OutputBtnRefresh.png`
- `manual-uat-output-black.png`
- `manual-uat-output-clear.png`
- `manual-uat-output-restore.png`
- `manual-uat-run2-key-f12.png`
- `manual-uat-run2-key-f11.png`
- `manual-uat-run2-key-f9.png`
- `manual-uat-run2-key-f3.png`
- `manual-uat-run2-key-space.png`
- `manual-uat-run2-key-shift-space.png`
- `manual-uat-run2-key-1.png`
- `manual-uat-run2-text-input-f9.png`

주의: `manual-uat-results.json`의 초기 run2 raw count에는 WPF ListBox virtualization으로 인한 list item count 오판이 있었다. 최종 판정은 스크린샷과 후속 preview/output run 결과를 우선 증거로 삼아 `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`에 반영했다.
