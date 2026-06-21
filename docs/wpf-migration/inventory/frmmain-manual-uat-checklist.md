# FrmMain 1:1 Manual UAT Checklist

작성일: 2026-06-04

목적: WPF가 "실행되는 껍데기"가 아니라 실제 예배 운영에 쓸 수 있는지 검증한다. 모든 시나리오는 가능하면 `C:\EasiSlides` 실제 데이터로 수행한다.

결과 표기:

- `PASS`: FrmMain과 같은 흐름으로 성공
- `PARTIAL`: 일부만 성공
- `FAIL`: 사용할 수 없음
- `BLOCKED`: 데이터/환경 문제로 검증 불가

## 1. Startup And Layout

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-001 | WPF 실행 | 창이 오류 없이 열리고 메뉴/툴바/상태바/좌측/Preview/Output이 보인다 | PASS - 2026-06-21 캡처 비교에서 WPF MainWindow가 오류 없이 열리고 주요 영역이 첫 화면에 표시됨 |
| UAT-002 | 1180x760 크기로 조절 | 좌측 상단, 좌측 하단, Preview, Output이 겹치지 않는다 | PASS - WPF `MinWidth=1180`, `MinHeight=760` 확인. 125% DPI 환경에서 물리 캡처는 1475x950으로 기록되며, 해당 1180x760 logical size에서 영역 겹침 없음 |
| UAT-003 | 1920x1080 최대화 | FrmMain과 같은 운영 영역 비율로 사용할 수 있다 | PASS - 2026-06-21 1920x1080 캡처에서 좌측 source/list, Preview, Output, bottom preview/output 영역 분리 유지 |
| UAT-004 | `C:\EasiSlides` working folder 확인 | 실제 legacy 데이터 경로를 사용한다 | PASS - `C:\EasiSlides` 존재 및 `Legacy worship list loaded: 1.주일예배 (23 .esw items)` 확인 |

## 2. Left Top Source Browser

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-101 | Folders 탭 열기 | song folders and song list are populated | PASS - `tabs/source-01-folders-after-initial-strip-fix.png`; 1개 폴더, 6444개 곡 표시. 2026-06-21 추가 확인: FrmMain에 없는 WPF-only 머리글자 점프 바를 제거해 `SongFolder`/`SongsList` 첫 화면 구조에 맞춤 |
| UAT-102 | 곡 검색 | title/lyrics/number search works with real songs | PASS - `manual-uat-run2-song-search.png`; 실제 곡 검색어 `감사` 입력 후 목록이 감사 관련 곡으로 필터링되고 Preview/목록 상태가 유지됨 |
| UAT-103 | 곡 더블클릭 | selected song is added to Worship List or Preview according to legacy flow | PARTIAL - `manual-uat-run2-song-add-button.png`; 선택 곡은 `WL_Add` 경로로 예배 순서에 추가되고 Preview가 갱신됨. 단, 더블클릭 gesture 자체의 목록 증가 증거는 확보하지 못해 follow-up 필요 |
| UAT-104 | 곡 drag to Worship List | dropped song is inserted at drop position | PARTIAL - source 선택 및 add 경로는 검증했으나 UIAutomation drag gesture 증거가 안정적으로 확보되지 않음 |
| UAT-111 | InfoScr 탭 열기 | InfoScreen folders and list are populated | BLOCKED - `tabs/source-02-infoscr.png`; 탭은 열리지만 `C:\EasiSlides\InfoScreens` 파일 수 0으로 실제 데이터 population 검증 불가 |
| UAT-112 | InfoScreen add/drag | selected InfoScreen is inserted into Worship List | BLOCKED - `C:\EasiSlides\InfoScreens` 파일 수 0으로 실제 add/drag 대상 없음 |
| UAT-121 | PowerPoint 탭 열기 | PPT folders/files are populated | BLOCKED - 현재 `UsePowerPointTab=false` 설정으로 탭 숨김, `C:\EasiSlides\Powerpoint` 파일 수 0. XAML에는 `PowerPointSourceTab` 존재 |
| UAT-122 | PPT preview/list style | thumbnails or list style matches selected mode | BLOCKED - `UsePowerPointTab=false` 및 `C:\EasiSlides\Powerpoint` 파일 수 0 |
| UAT-123 | PPT add/drag | selected PPT is inserted into Worship List and Preview shows first slide | BLOCKED - 실제 PPT 데이터 없음 |
| UAT-124 | PPT thumbnail sharpness | left source, Preview, and Output PPT thumbnails render as clear 4:3 images, not blurry low-resolution exports | BLOCKED - 실제 PPT 데이터 없음 |
| UAT-131 | Bibles 탭 열기 | Bible versions, books, and text area are populated | PASS - `tabs/source-03-bibles-after-frmmain-bottom-fix.png`; `C:\EasiSlides\HolyBibles` DB 2개, 책/본문/버전 UI 표시. 2026-06-21 지적 반영: non-legacy 하단 추가 버튼 제거, `BibleText` 바로 아래 `TabBibleVersions` 배치 확인 |
| UAT-132 | Bible direct lookup | reference such as `사도행전 9:31` selects/shows verses | PASS - `manual-uat-bible-lookup-add.png`, `manual-uat-run2-bible-lookup.png`; `사도행전 9:31` 조회 후 본문과 선택 범위가 표시됨 |
| UAT-133 | Bible passage add | selected verses are added to Worship List | PASS - `manual-uat-bible-lookup-add.png`; `사도행전 9:31-32 (개역개정)` 항목이 예배 순서에 추가됨 |
| UAT-134 | Bible passage drag | selected verses can be dragged into desired Worship List position | PARTIAL - lookup/add 경로는 검증했으나 Bible drag insertion 증거는 안정적으로 확보하지 못함 |
| UAT-135 | Bible click and Shift-click selection | clicking a verse selects that verse, Shift+click extends multiple verses, and context/Enter/source-add paths insert the selected range into Worship List | PARTIAL - 단일 조회 및 2절 범위 추가는 확인했으나 Shift-click multi-select gesture 자체는 별도 follow-up 필요 |
| UAT-141 | Images 탭 열기 | image folders and thumbnails are populated | PARTIAL - `tabs/source-04-images.png`; `C:\EasiSlides\Images` 파일 117개와 썸네일 표시. 단, `tabs-compare/compare-04-images.png` 기준 WPF 첫 화면 썸네일 밀도/가시 개수가 FrmMain보다 낮아 visual usability parity 보정 필요 |
| UAT-142 | Apply image background | selected image changes item/default/output background as expected | PASS - `manual-uat-images-apply.png`, `manual-uat-run2-images-apply.png`; `cross1.jpg` 선택 후 Apply 실행, 상태바에 항목 배경 이미지 적용 메시지 표시 |
| UAT-151 | Media 탭 열기 | media folders and files are populated | BLOCKED - 현재 `UseMediaTab=false` 설정으로 탭 숨김, `C:\EasiSlides\Media` 파일 수 0. XAML에는 `MediaSourceTab` 존재 |
| UAT-152 | Media double-click/add/drag | selected media is inserted and can be previewed/output | BLOCKED - `UseMediaTab=false` 및 `C:\EasiSlides\Media` 파일 수 0 |
| UAT-161 | Default 탭 열기 | default format/background/transition controls are available | PARTIAL - `tabs/source-05-default.png`; `Def_*` 컨트롤은 XAML/스크롤 영역에 존재하지만, `tabs-compare/compare-05-default.png` 기준 FrmMain 첫 화면의 `Apply to All Except InfoScreens`, `Default Background`, `Display Panel` 밀도와 순서가 WPF 첫 화면에서 동일하지 않음 |

## 3. Left Bottom Worship List And Praise Book

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-201 | Saved Worship List selector open | `.esw` lists from `C:\EasiSlides\Admin\WorshipLists` appear | PASS - `tabs/list-01-worship-list.png`; `C:\EasiSlides\Admin\WorshipLists` 파일 35개, selector 표시 |
| UAT-202 | Load saved Worship List | selected list loads real items with icons/types | PASS - `tabs/list-01-worship-list.png`; `1.주일예배` 23개 항목 로드 및 아이콘/타입 표시 |
| UAT-203 | Select Worship List item | Preview updates to selected item | PASS - `manual-uat-run2-worship-select.png`; 실제 예배 순서 항목 선택 시 Preview 선택 상태/본문이 갱신됨 |
| UAT-204 | Delete selected item | item is removed and list remains stable | PASS - `manual-uat-run2-worship-delete.png`; 세션 내 항목 제거 버튼 실행 후 UI 안정 유지. 파일 저장은 수행하지 않음 |
| UAT-205 | Move item up/down buttons | selected item moves exactly one position | PARTIAL - `manual-uat-run2-worship-move-down.png`; 이동 버튼은 실행되었으나 전후 순서 diff 증거가 부족해 exact one-position parity는 follow-up 필요 |
| UAT-206 | Drag reorder Worship List | item moves to dropped position | PARTIAL - 버튼 이동 경로는 확인했으나 drag reorder gesture 증거는 안정적으로 확보하지 못함 |
| UAT-207 | Context menu Play | item plays/previews according to legacy behavior | PARTIAL - Preview/Go Live 경로는 검증했으나 context menu Play gesture 자체는 별도 확인 필요 |
| UAT-208 | Context menu Play on Output | selected item goes directly to Output | PARTIAL - Go Live/Output 복사 경로는 검증했으나 context menu Play on Output gesture 자체는 별도 확인 필요 |
| UAT-211 | Praise Book tab open | praise book selector and entries are populated | PARTIAL - `tabs/list-02-praise-book.png`; Praise Book tab과 selector/entry surface는 표시됨. 단, `tabs-compare/compare-07-praise-book.png` 기준 WPF toolbar가 가로 확장되어 FrmMain의 compact top/vertical toolstrip 밀도와 다르고, 선택된 실제 legacy book의 entry population은 추가 데이터 확인 필요 |
| UAT-212 | Praise Book entry add | selected entry is added to Worship List | BLOCKED - `manual-uat-run2-praisebook.png`; 현재 선택된 PraiseBook 1이 0곡으로 표시되어 add 대상 없음 |
| UAT-213 | Praise Book delete/export buttons | behavior matches FrmMain or is explicitly marked deferred | BLOCKED - PraiseBook entry population이 0곡이라 delete/export 동작 검증 불가 |

## 4. Preview Column

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-301 | Select song item | Preview header, text, and large preview update | PASS - `manual-uat-preview-start-song.png`; 실제 곡 추가/선택 후 Preview header/text/large preview가 갱신됨 |
| UAT-302 | Preview section jump 1-9 | Preview moves to chosen section only | PASS - `manual-uat-PreviewBtnVerse1.png`; Preview 절 버튼 1 실행 성공 |
| UAT-303 | Preview chorus/bridge/ending jump | Preview moves to named section only | PARTIAL - `PreviewBtnVerseChorus` AutomationId를 찾지 못함. 현재 곡/데이터에서 chorus named jump 노출 조건 확인 필요 |
| UAT-304 | Preview previous/next slide | Preview page/slide changes only | PASS - `manual-uat-PreviewBtnSlideDown.png`; Preview 다음 슬라이드 버튼 실행 성공 |
| UAT-305 | Preview previous/next item | selected/preview item changes | PASS - `manual-uat-PreviewBtnItemDown.png`; Preview 다음 항목 버튼 실행 성공 |
| UAT-306 | Preview PowerPoint thumbnails | selected thumbnail updates Preview slide | BLOCKED - PowerPoint 탭/실제 PPT 데이터가 없어 검증 불가 |
| UAT-307 | Preview Go Live | selected Preview is sent live | PASS - `wpf-before-preview-go-live.png`, `wpf-after-preview-go-live.png`; Preview Go Live 호출 후 live banner, Worship List LIVE badge, Output 본문/큰 화면 갱신 |
| UAT-308 | Preview send and next | selected Preview is sent live and next item is selected | PASS - `manual-uat-preview-output-next-run3.png`; Preview Output 복사 후 다음 실행 성공 |

## 5. Output Column

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-401 | Go Live from Preview | Output header and large output show live item | PASS - `wpf-after-preview-go-live.png`; Output header와 large output에 선택 항목 본문 표시 |
| UAT-402 | Output previous/next slide | live slide changes without changing Preview unexpectedly | PARTIAL - `manual-uat-OutputBtnSlideDown-run3.png`; live 상태 후 버튼이 disabled로 관찰됨. 출력 slide 이동 가능 조건 확인 필요 |
| UAT-403 | Output previous/next item | live item changes according to Worship List order | PASS - `manual-uat-OutputBtnItemDown-run3.png`; Output 다음 항목 실행 성공 |
| UAT-404 | Output section jump | live item jumps to selected section | PARTIAL - `OutputBtnVerse1` AutomationId를 찾지 못함. Output 절 점프 노출 조건 확인 필요 |
| UAT-405 | Output PowerPoint thumbnails | live PPT slide highlight and large output match | BLOCKED - PowerPoint 탭/실제 PPT 데이터가 없어 검증 불가 |
| UAT-406 | Black | output turns black and state is visible | PARTIAL - `manual-uat-output-black.png`, `manual-uat-run2-output-black.png`; operator surface에서 명령/상태 캡처는 확보했으나 외부 송출 모니터 black 전환은 독립 관찰하지 못함 |
| UAT-407 | Clear | output clears content and state is visible | PARTIAL - `manual-uat-output-clear.png`, `manual-uat-run2-output-clear.png`; operator surface에서 명령/상태 캡처는 확보했으나 외부 송출 모니터 clear 전환은 독립 관찰하지 못함 |
| UAT-408 | Hide | output hides text/content and Restore brings it back | PARTIAL - `manual-uat-output-restore.png`, `manual-uat-run2-output-restore.png`; Restore 명령 캡처는 확보했으나 외부 송출 모니터 hide/restore 시각 결과는 독립 관찰하지 못함 |
| UAT-409 | Restart current item | current live item restarts from first slide/page | PASS - `manual-uat-run2-OutputBtnRestartCurrentItem.png`; Output 현재 항목 처음으로 실행 성공 |
| UAT-410 | Refresh output | output window/media/slide state refreshes without crash | PASS - `manual-uat-run2-OutputBtnRefresh.png`, `manual-uat-preview-output-live.png`; Output 새로고침 실행 후 화면 안정 유지 |
| UAT-411 | Live message send/clear | live message area behaves like FrmMain or is deferred | PARTIAL - `manual-uat-run2-live-message.png`; run2 시점에서 live message 입력/버튼 AutomationId가 탐색되지 않아 실제 send/clear는 follow-up 필요 |

## 6. Keyboard And Focus

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-501 | F12 | Go Live/start show action works | PARTIAL - `manual-uat-run2-key-f12.png`; WPF window에 F12 송신 후 crash 없음. 외부 show action은 독립 관찰하지 못함 |
| UAT-502 | F11 | send to Output and advance works | PARTIAL - `manual-uat-run2-key-f11.png`; F11 송신 후 crash 없음. send/advance target의 실제 전환은 follow-up 필요 |
| UAT-503 | F9 | Black toggles/activates | PARTIAL - `manual-uat-run2-key-f9.png`, `manual-uat-key-f9-black.png`; F9 송신 후 crash 없음. 외부 black state는 독립 관찰하지 못함 |
| UAT-504 | F3 | Clear toggles/activates | PARTIAL - `manual-uat-run2-key-f3.png`, `manual-uat-key-f3-clear.png`; F3 송신 후 crash 없음. 외부 clear state는 독립 관찰하지 못함 |
| UAT-505 | Space | next item/slide according to active legacy focus | PARTIAL - `manual-uat-run2-key-space.png`; Space 송신 후 crash 없음. focus-dependent legacy target의 정확한 결과는 follow-up 필요 |
| UAT-506 | Shift+Space | previous item/slide according to active legacy focus | PARTIAL - `manual-uat-run2-key-shift-space.png`; Shift+Space 송신 후 crash 없음. focus-dependent legacy target의 정확한 결과는 follow-up 필요 |
| UAT-507 | Number keys 1-9 | section jump works when not typing in text input | PARTIAL - `manual-uat-run2-key-1.png`; 숫자 1 송신 후 crash 없음. 현재 live item별 section jump 결과는 follow-up 필요 |
| UAT-508 | Text input focus | live shortcuts do not fire while typing in search/reference fields | PASS - `manual-uat-run2-text-input-f9.png`; 곡 검색 textbox focus 상태에서 F9 송신 시 live shortcut이 가시적으로 text-entry context를 깨지 않음 |

## Phase 7 Smoke Evidence (2026-06-21)

자동 smoke 검증은 manual UAT 전체 PASS를 대체하지 않는다. 다만 실제 `C:\EasiSlides` 데이터가 있는 환경에서 WPF MainWindow가 시작되고, 주요 first-screen 영역과 레거시 데이터 로딩 신호가 노출되는지 확인하는 gate evidence로 기록한다.

- 캡처 위치: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/screenshots/2026-06-21/`.
- FrmMain/WPF 비교 캡처: `frmmain-vs-wpf-printwindow-side-by-side.png`.
- 원본 캡처: `winforms-frmmain-printwindow-1440x900.png`, `wpf-mainwindow-printwindow-1440x900.png`, `wpf-mainwindow-printwindow-1180x760.png`, `wpf-mainwindow-printwindow-1920x1080.png`.
- 탭별 캡처: `tabs/wpf-source-tabs-contact-sheet.png`, `tabs/source-01-folders.png`, `tabs/source-01-folders-after-initial-strip-fix.png`, `tabs/source-02-infoscr.png`, `tabs/source-03-bibles.png`, `tabs/source-03-bibles-after-frmmain-bottom-fix.png`, `tabs/source-04-images.png`, `tabs/source-05-default.png`, `tabs/list-01-worship-list.png`, `tabs/list-02-praise-book.png`.
- 탭별 FrmMain/WPF 비교 캡처: `tabs-compare/compare-01-folders.png`, `tabs-compare/compare-01-folders-after-initial-strip-fix.png`, `tabs-compare/compare-02-infoscr.png`, `tabs-compare/compare-03-bibles.png`, `tabs-compare/compare-04-images.png`, `tabs-compare/compare-05-default.png`, `tabs-compare/compare-06-worship-list.png`, `tabs-compare/compare-07-praise-book.png`.
- Folders UI 수정 후 캡처: `tabs/source-01-folders-after-initial-strip-fix.png`, `tabs-compare/compare-01-folders-after-initial-strip-fix.png`. FrmMain `tabFolders`는 `SongFolder`, `SongsList`, `panelFolders/Folders_WordCount`만 가지므로 WPF-only 머리글자 점프 바를 제거함. 재캡처는 최대화 상태라 기존 1440x900 비교와 배율은 다르며, 추가 UI 제거 확인 증거로 사용한다.
- Bibles 하단 UI 수정 후 캡처: `tabs/source-03-bibles-after-frmmain-bottom-fix.png`. 사용자 지적에 따라 WPF-only 하단 추가 버튼/상태 줄을 제거하고 FrmMain처럼 `BibleText` 바로 아래 `TabBibleVersions`가 오도록 맞춤.
- Live/Output 캡처: `wpf-before-preview-go-live.png`, `wpf-after-preview-go-live.png`, `wpf-live-safety-contact-sheet.png`, `wpf-live-before-safety-actions.png`, `wpf-output-black.png`, `wpf-output-clear.png`, `wpf-output-restore.png`.
- `C:\EasiSlides` 존재 확인: `Admin`, `Backgrounds`, `HolyBibles`, `Images`, `InfoScreens`, `Media`, `Powerpoint` 폴더 확인.
- `Easislides.Wpf\bin\Debug\net10.0-windows\EasislidesNext.exe` 실행 확인: `MainWindowTitle=EasiSlides`, `AutomationName=EasiSlides`, `AutomationClassName=Window`, UIAutomation descendant 355개.
- 실제 데이터 로딩 신호 확인: `Legacy worship list loaded: 1.주일예배 (23 .esw items)`.
- 주요 UI 영역 노출 확인: `Folders`, `InfoScr`, `Bibles`, `Images`, `Default`, `Worship List`, `Praise Book`, `Preview`, `Output`, `Go Live`.
- 주요 Output 안전 제어 노출 확인: `Output 검은 화면`, `Output 화면 비우기`, `Output 복귀`, `Output 새로고침`.
- 시각 비교 결과: WPF는 FrmMain처럼 좌상단 source browser, 좌하단 Worship List/Praise Book, 오른쪽 Preview/Output column, bottom preview/output surfaces를 첫 화면에 유지한다. 다만 탭별 세부 UI는 추가 보정이 필요하다. 확인된 남은 visual usability gap은 Images 첫 화면 썸네일 밀도, Default 첫 화면 `Default Background`/`Display Panel` 노출 순서와 밀도, Praise Book toolbar/list 밀도, Worship List toolbar density, InfoScr/PowerPoint/Media 실제 데이터 부족 검증이다.
- DPI 참고: 1180x760 resize 요청의 캡처 크기는 1475x950이었다. 이는 125% DPI에서 WPF logical size 1180x760이 물리 픽셀로 변환된 값이며, 결함으로 보지 않는다.
- 종료 확인: `CloseMainWindow=True`, 최종 프로세스 종료 확인.

## Actual Manual UAT Evidence (2026-06-21)

실제 `C:\EasiSlides` 데이터로 WPF MainWindow를 실행하고 UAT 표의 모든 row에 PASS/PARTIAL/BLOCKED/FAIL 판정을 채웠다.

- 상세 요약: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/manual-uat-summary.md`.
- 결과 집계: PASS 25, PARTIAL 25, BLOCKED 12, FAIL 0.
- raw 조작/캡처 위치: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/`.
- `manual-uat-results.json`의 초기 run2 raw count에는 WPF ListBox virtualization으로 인한 list item count 오판이 있어 최종 판정에는 스크린샷과 후속 preview/output run 결과를 우선 적용했다.
- smoke evidence만으로 PASS 처리하지 않고, 외부 송출 모니터 미관찰/drag gesture 미확인/PPT·Media·InfoScreen 데이터 부재 항목은 PARTIAL 또는 BLOCKED로 남겼다.

## Phase 8 PARTIAL/BLOCKED 재확인 (2026-06-21)

OpenSpec `a010-wpf-frmmain-1to1-operator-console-parity` Phase 8에서 PARTIAL 25개와 BLOCKED 12개를 재확인했다.

- 코드 보정 완료: UAT-141 Images, UAT-161 Default, UAT-211 Praise Book visual usability gap.
- 구현/테스트 증거로 닫음: UAT-103, UAT-104, UAT-134, UAT-135, UAT-205~208, UAT-303, UAT-402, UAT-404, UAT-411, UAT-501~507.
- 외부 송출 모니터 직접 관찰 필요: UAT-406~408.
- source 폴더/설정/선택 상태 재검증 필요로 BLOCKED 유지: UAT-111, UAT-112, UAT-121~124, UAT-151, UAT-152, UAT-212, UAT-213, UAT-306, UAT-405.
- 후속 확인: `C:\EasiSlides` 운영 데이터 루트는 존재한다. PPT/Media 운영 자료는 `C:\EasiSlides\Documents\*.lnk`가 가리키는 `D:\예배자료`에 존재한다. 다만 현재 `UsePowerPointTab=0`, `UseMediaTab=0`이고 `C:\EasiSlides\Powerpoint`, `C:\EasiSlides\Media`, `C:\EasiSlides\InfoScreens` 직접 source 폴더는 비어 있어 source tab UAT는 설정/경로 재확인 후 재수행해야 한다.

상세 근거: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/phase8-partial-blocked-closure.md`.

## Phase 9 레지스트리 런타임 설정 보정 (2026-06-21)

사용자 확인으로 `HKCU\Software\EasiSlides` 아래에 FrmMain 관련 설정이 있음을 재확인했다. WPF는 레지스트리 소스를 가지고 있었지만, 기존 `%APPDATA%\EasislidesNext\settings.json`이 있으면 bootstrap migration을 건너뛰어 `current_praisebook`, `current_session`, `UsePowerpointTab`, `UseMediaTab`, `media_dir`가 재반영되지 않았다.

Phase 9에서 `SettingsBootstrapMigrationService`를 수정해 기존 WPF 작업 폴더는 덮어쓰지 않고 위 runtime registry settings만 시작 시 재동기화하도록 했다.

- 코드 보정 완료: `SettingsBootstrapMigrationService`.
- 테스트 완료: `SettingsBootstrapMigrationServiceTests|RegistryLegacySettingsSourceTests` 실패 0, 통과 8, 건너뜀 0.
- UAT 영향: `current_praisebook`/`current_session` 선택 상태와 PowerPoint/Media tab 표시 설정은 다음 WPF 시작부터 레지스트리 기준으로 재동기화된다. 단, `UsePowerpointTab=0`, `UseMediaTab=0` 값 자체가 legacy 현재 상태이면 해당 탭은 FrmMain과 동일하게 숨김 상태가 맞다.

## 7. Completion Gate

The WPF migration cannot be marked usable until:

- all P0 UAT rows are `PASS`;
- every `PARTIAL` row has a linked implementation task;
- every `FAIL` row has a bug/task and is not hidden;
- `dotnet test Easislides.Wpf.Tests` passes;
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal` passes;
- WPF is launched and manually checked with real legacy data.
