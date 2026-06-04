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
| UAT-001 | WPF 실행 | 창이 오류 없이 열리고 메뉴/툴바/상태바/좌측/Preview/Output이 보인다 | |
| UAT-002 | 1180x760 크기로 조절 | 좌측 상단, 좌측 하단, Preview, Output이 겹치지 않는다 | |
| UAT-003 | 1920x1080 최대화 | FrmMain과 같은 운영 영역 비율로 사용할 수 있다 | |
| UAT-004 | `C:\EasiSlides` working folder 확인 | 실제 legacy 데이터 경로를 사용한다 | |

## 2. Left Top Source Browser

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-101 | Folders 탭 열기 | song folders and song list are populated | |
| UAT-102 | 곡 검색 | title/lyrics/number search works with real songs | |
| UAT-103 | 곡 더블클릭 | selected song is added to Worship List or Preview according to legacy flow | |
| UAT-104 | 곡 drag to Worship List | dropped song is inserted at drop position | |
| UAT-111 | InfoScr 탭 열기 | InfoScreen folders and list are populated | |
| UAT-112 | InfoScreen add/drag | selected InfoScreen is inserted into Worship List | |
| UAT-121 | PowerPoint 탭 열기 | PPT folders/files are populated | |
| UAT-122 | PPT preview/list style | thumbnails or list style matches selected mode | |
| UAT-123 | PPT add/drag | selected PPT is inserted into Worship List and Preview shows first slide | |
| UAT-131 | Bibles 탭 열기 | Bible versions, books, and text area are populated | |
| UAT-132 | Bible direct lookup | reference such as `사도행전 9:31` selects/shows verses | |
| UAT-133 | Bible passage add | selected verses are added to Worship List | |
| UAT-134 | Bible passage drag | selected verses can be dragged into desired Worship List position | |
| UAT-141 | Images 탭 열기 | image folders and thumbnails are populated | |
| UAT-142 | Apply image background | selected image changes item/default/output background as expected | |
| UAT-151 | Media 탭 열기 | media folders and files are populated | |
| UAT-152 | Media double-click/add/drag | selected media is inserted and can be previewed/output | |
| UAT-161 | Default 탭 열기 | default format/background/transition controls are available | |

## 3. Left Bottom Worship List And Praise Book

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-201 | Saved Worship List selector open | `.esw` lists from `C:\EasiSlides\Admin\WorshipLists` appear | |
| UAT-202 | Load saved Worship List | selected list loads real items with icons/types | |
| UAT-203 | Select Worship List item | Preview updates to selected item | |
| UAT-204 | Delete selected item | item is removed and list remains stable | |
| UAT-205 | Move item up/down buttons | selected item moves exactly one position | |
| UAT-206 | Drag reorder Worship List | item moves to dropped position | |
| UAT-207 | Context menu Play | item plays/previews according to legacy behavior | |
| UAT-208 | Context menu Play on Output | selected item goes directly to Output | |
| UAT-211 | Praise Book tab open | praise book selector and entries are populated | |
| UAT-212 | Praise Book entry add | selected entry is added to Worship List | |
| UAT-213 | Praise Book delete/export buttons | behavior matches FrmMain or is explicitly marked deferred | |

## 4. Preview Column

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-301 | Select song item | Preview header, text, and large preview update | |
| UAT-302 | Preview section jump 1-9 | Preview moves to chosen section only | |
| UAT-303 | Preview chorus/bridge/ending jump | Preview moves to named section only | |
| UAT-304 | Preview previous/next slide | Preview page/slide changes only | |
| UAT-305 | Preview previous/next item | selected/preview item changes | |
| UAT-306 | Preview PowerPoint thumbnails | selected thumbnail updates Preview slide | |
| UAT-307 | Preview Go Live | selected Preview is sent live | |
| UAT-308 | Preview send and next | selected Preview is sent live and next item is selected | |

## 5. Output Column

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-401 | Go Live from Preview | Output header and large output show live item | |
| UAT-402 | Output previous/next slide | live slide changes without changing Preview unexpectedly | |
| UAT-403 | Output previous/next item | live item changes according to Worship List order | |
| UAT-404 | Output section jump | live item jumps to selected section | |
| UAT-405 | Output PowerPoint thumbnails | live PPT slide highlight and large output match | |
| UAT-406 | Black | output turns black and state is visible | |
| UAT-407 | Clear | output clears content and state is visible | |
| UAT-408 | Hide | output hides text/content and Restore brings it back | |
| UAT-409 | Restart current item | current live item restarts from first slide/page | |
| UAT-410 | Refresh output | output window/media/slide state refreshes without crash | |
| UAT-411 | Live message send/clear | live message area behaves like FrmMain or is deferred | |

## 6. Keyboard And Focus

| ID | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| UAT-501 | F12 | Go Live/start show action works | |
| UAT-502 | F11 | send to Output and advance works | |
| UAT-503 | F9 | Black toggles/activates | |
| UAT-504 | F3 | Clear toggles/activates | |
| UAT-505 | Space | next item/slide according to active legacy focus | |
| UAT-506 | Shift+Space | previous item/slide according to active legacy focus | |
| UAT-507 | Number keys 1-9 | section jump works when not typing in text input | |
| UAT-508 | Text input focus | live shortcuts do not fire while typing in search/reference fields | |

## 7. Completion Gate

The WPF migration cannot be marked usable until:

- all P0 UAT rows are `PASS`;
- every `PARTIAL` row has a linked implementation task;
- every `FAIL` row has a bug/task and is not hidden;
- `dotnet test Easislides.Wpf.Tests` passes;
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal` passes;
- WPF is launched and manually checked with real legacy data.
