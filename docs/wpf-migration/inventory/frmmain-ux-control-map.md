# FrmMain UX control map

작성일: 2026-06-04
근거: `FrmMain.Designer.cs`, `FrmMain.Events.cs`, `FrmMain.Logic.cs`, `KeyboardActionHandler.cs`

## 목적

WPF `MainWindow`를 새 대시보드가 아니라 기존 `FrmMain` 운영 콘솔의 연장선으로 정렬하기 위한 기준표다. 이 문서는 모든 private 컨트롤을 기계적으로 나열하기보다, 라이브 운영 중 손이 바로 가는 컨트롤과 WPF 대응 위치를 우선 기록한다.

## 라이브 운영 P0

| FrmMain 컨트롤/메뉴 | 역할 | 영역 | WPF 대응 | 판정 | 우선순위 |
| --- | --- | --- | --- | --- | --- |
| `btnToLive` / `btnToOutput` / `Menu_GoLiveWithPreview` / `Menu_StartShow` | 선택/Preview 항목을 송출 | Preview -> Output | `GoLiveCommand`, 고정 운영 바, Output 메뉴 | 유지 | P0 |
| `btnToOutputMoveNext` | 송출 후 다음 항목 선택 | Preview -> Output | `SendToOutputAndNextCommand`, 고정 운영 바 | 유지 | P0 |
| `cbOutputBlack` / `Menu_BlackScreen` / `labelBlackScreen` | 검은 화면 | Output 안전 명령 | `BlackScreenCommand`, 고정 운영 바, Output 메뉴 | 제거 금지 | P0 |
| `cbOutputClear` / `Menu_ClearScreen` | 화면 비우기 | Output 안전 명령 | `ClearOutputCommand`, 고정 운영 바, Output 메뉴 | 제거 금지 | P0 |
| `labelHideText` / hide 계열 | 가사/콘텐츠 숨김 | Output 안전 명령 | `HideOutputCommand`, 고정 운영 바 | 유지 | P0 |
| Restore 흐름 | Black/Clear/Hide 이후 복귀 | Output 안전 명령 | `RestoreOutputCommand`, 고정 운영 바 | 유지 | P0 |
| `Menu_StartShow` / live 종료 흐름 | 라이브 송출 중지 | Output 안전 명령 | `StopLiveCommand`, 고정 운영 바, Output 메뉴 | 제거 금지 | P0 |
| `Menu_RestartCurrentItem` | 현재 항목 처음으로 | Output 재시작 | `RestartCurrentItemCommand`, 고정 운영 바 | 유지 | P0 |
| `Menu_RefreshOutput` | 출력 새로고침 | Output 복구 | `RefreshOutputCommand`, 고정 운영 바 | 유지 | P0 |
| `PreviewBtnItemUp/Down`, `OutputBtnItemUp/Down` | 이전/다음 예배 항목 | 예배 순서/Preview/Output | `PreviousItemCommand`, `NextItemCommand`, 중앙 하단 항목 이동 바 | 유지 | P0 |
| `PreviewBtnSlideUp/Down`, `OutputBtnSlideUp/Down` | 이전/다음 절 또는 PPT 슬라이드 | Preview/Output | `PreviousLyricsPageCommand`, `NextLyricsPageCommand`, `PreviousSlideCommand`, `NextSlideCommand` | 유지 | P0 |

## 절/구간 직접 점프

| FrmMain 컨트롤 | 역할 | WPF 대응 | 판정 | 우선순위 |
| --- | --- | --- | --- | --- |
| `PreviewBtnVerse1`..`PreviewBtnVerse9` | Preview 절 번호 직접 이동 | `AvailableSectionLabels` + `JumpToLyricsSectionCommand` | 유지 | P0 |
| `PreviewBtnVersePreChorus*` | 전주/프리코러스 이동 | label 기반 점프, `VerseJumpKeyMap` | WPF식 개선 | P1 |
| `PreviewBtnVerseChorus*` | 후렴 이동 | label 기반 점프, `VerseJumpKeyMap` | 유지 | P0 |
| `PreviewBtnVerseBridge*` | 브릿지 이동 | label 기반 점프, `VerseJumpKeyMap` | 유지 | P0 |
| `PreviewBtnVerseEnding` | 엔딩 이동 | label 기반 점프 | 유지 | P1 |
| `OutputBtnVerse1`..`OutputBtnVerseEnding` | 송출 중 절 직접 이동 | 현재 WPF는 선택/라이브 문맥을 명령에서 처리 | WPF식 개선 | P1 |

## Preview / Output 표시 영역

| FrmMain 컨트롤 | 역할 | WPF 대응 | 판정 | 우선순위 |
| --- | --- | --- | --- | --- |
| `PreviewHolder`, `PreviewBack`, `flowLayoutPreviewLyrics` | 다음 송출 후보 미리보기 | 중앙 Preview 탭, `SlidePreviewControl` | 유지 | P0 |
| `OutputHolder`, `OutputBack`, `flowLayoutOutputLyrics` | 현재 송출 상태 | `LiveBar`, 중앙 Output 상태 헤더, 출력 창 | WPF식 개선 | P0 |
| `flowLayoutPreviewPowerPoint` | Preview PPT 썸네일 | PowerPoint 탭 thumbnail strip | 유지 | P1 |
| `flowLayoutOutputPowerPoint` | Output PPT 썸네일 | PowerPoint 탭 + live slide commands | WPF식 개선 | P1 |
| `PreviewInfo`, `OutputInfo` | 항목/상태 정보 | `StatusText`, 중앙 Preview/Output 헤더 | 유지 | P0 |
| `PreviewNotes`, `IndcbPreviewNotes` | 세션/Preview notes | 별도 세션 notes 창/후속 | 보류 | P2 |

## 콘텐츠 브라우저

| FrmMain 컨트롤/메뉴 | 역할 | WPF 대응 | 판정 | 우선순위 |
| --- | --- | --- | --- | --- |
| `Main_QuickFind`, `Main_Find`, `Menu_Find` | 빠른 곡 검색 | `toolStripMain`의 `Main_QuickFind`/`Main_Find`가 inline `Search` 탭의 `SearchUsageViewModel.SearchSongsCommand`를 실행 | 유지 | P0 |
| `BibleUserLookup`, `Bibles_Go`, `BookLookup`, `BibleText` | 성경 검색/본문 | 좌측 성경 탭, Bible 창 | 유지 | P1 |
| `PowerpointFolder`, `PP_OpenFolder`, `PP_Import` | PPT 탐색/추가 | 파일 추가 버튼, PowerPoint library/window | WPF식 개선 | P1 |
| `MediaFolder`, `Media_Import`, `MediaList` | 미디어 탐색/추가 | 파일 추가 버튼, Media tab/window | WPF식 개선 | P1 |
| `ImagesFolder`, `Image_Import`, `Image_OpenFolder` | 배경 이미지 관리 | 우측 인스펙터/이미지 라이브러리 | 보조 패널 이동 | P1 |

## 서식/배경/표시 옵션

| FrmMain 컨트롤 | 역할 | WPF 대응 | 판정 | 우선순위 |
| --- | --- | --- | --- | --- |
| `Ind_R1Align*`, `Ind_R2Align*`, `Ind_VAlign*` | 가사/언어 영역 정렬 | 인스펙터 정렬 버튼, Output 메뉴 | 유지 | P1 |
| `Ind_R1Colour`, `Ind_R2Colour`, `Ind_BackColour` | 글자/배경색 | 인스펙터 색 직접 지정/프리셋 | 유지 | P1 |
| `Ind_ImageTile`, `Ind_ImageCentre`, `Ind_ImageBestFit`, `Ind_NoImage` | 배경 이미지 표시 모드 | Output 메뉴 배경 표시 모드, 인스펙터 후속 | 유지 | P1 |
| `Ind_Notations`, `Ind_CapoUp`, `Ind_CapoDown` | 코드 표시/조옮김 | Output 메뉴, `LiveTranspose*Command` | WPF식 개선 | P1 |
| `Ind_Outline`, `Ind_Shadow`, `Ind_Interlace` | 가독성/이중언어 효과 | 인스펙터 토글, Output 메뉴 | 유지 | P1 |

## 단축키 기준

| FrmMain 기준 | 역할 | WPF 현재 | 판정 |
| --- | --- | --- | --- |
| `F12` | Go Live / Start show | `CommandCatalog.LiveGo` | 유지 |
| `F11` | 송출 후 다음 항목 | `CommandCatalog.LiveGoAndNext` | 유지 |
| `F9` | Black screen | `CommandCatalog.LiveBlack` | 유지 |
| `F3` | Clear screen | `CommandCatalog.LiveClear` | 유지 |
| `Space` | 다음 항목 | `CommandCatalog.LiveNext` | 유지 |
| `Shift+Space` | 이전 항목 | `CommandCatalog.LivePrevious` | 유지 |
| `Ctrl+R` | 현재 항목 처음으로 | `CommandCatalog.LiveRestart` | WPF식 개선 |
| `Ctrl+F5` | 출력 새로고침 | `CommandCatalog.LiveRefresh` | WPF식 개선 |
| 숫자 1-9 | 절 번호 점프 | `VerseJumpKeyMap` | 유지 |
| `C/B/P/Q/T/W/E` | 후렴/브릿지 등 라벨 점프 | `VerseJumpKeyMap` | WPF식 개선 |

## 제거 금지

라이브 안전 명령(Black/Clear/Hide/Restore/Close Output/Stop Live), Go Live, 송출 후 다음, 절/슬라이드 이동, 예배 순서 이동은 메뉴나 명령 팔레트에만 남기면 안 된다. 첫 화면에 항상 보이는 경로가 필요하다.
