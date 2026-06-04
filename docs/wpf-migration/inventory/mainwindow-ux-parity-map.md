# MainWindow UX parity map

작성일: 2026-06-04
대상: `Easislides.Wpf/MainWindow.xaml`

## 요약

현재 WPF `MainWindow`는 명령과 ViewModel 기반이 이미 상당히 갖춰져 있다. 부족한 점은 기능 부재보다 "현장 조작의 위치와 노출 밀도"다. 이번 대응표는 `FrmMain` 기준으로 기능이 어디에 있는지, 숨겨졌는지, 흐름이 달라졌는지를 분리한다.

## 영역 대응

| FrmMain 영역 | WPF 현재 위치 | 상태 | 메모 |
| --- | --- | --- | --- |
| 메뉴바 | 상단 `Menu` File/Edit/View/Output/Tools/Help | 대응됨 | 메뉴는 보조 경로로 유지 |
| Live 상태 표시 | `LiveBar`, 하단 상태바 | 대응됨 | 중앙 Output 상태 헤더로 보강 |
| 콘텐츠 브라우저 | 좌측 Library/Bible/Search 탭 | 대응됨 | FrmMain처럼 예배 순서와 동시에 보임 |
| 예배 순서 | 좌측 하단 `WorshipListPanel` | 대응됨 | 항상 보임 |
| Preview | 중앙 Preview/PowerPoint/Media 탭 | 대응됨 | 선택 항목과 출력 항목 구분을 더 명확히 해야 함 |
| Output 상태 | `LiveBar`, 출력 창, 인스펙터 일부 | 기능은 있으나 위치가 다름 | 중앙 헤더에 현재 Output 상태를 노출 |
| 라이브 제어 | 하단 고정 바 + Output 메뉴 | 기능은 있으나 일부 누락 | `SendToOutputAndNext`를 고정 바에 추가 |
| 절/슬라이드 이동 | Preview 탭 하단, PPT 탭 하단 | 대응됨 | 첫 화면에서 보이지만 탭 종류에 따라 달라짐 |
| 서식/배경/전환 | 우측 인스펙터 + Output 메뉴 | 기능은 있으나 일부 숨겨짐 | 핵심 라이브 안전 명령과 분리해야 함 |
| 명령 팔레트 | Ctrl+K, 하단 버튼 | 대응됨 | 보조 탐색 수단으로 유지 |

## 주요 기능별 매핑

| 운영 기능 | WPF 바인딩/컨트롤 | 분류 | 조치 |
| --- | --- | --- | --- |
| Go Live | `GoLiveCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| 송출 후 다음 | `SendToOutputAndNextCommand`, Output 메뉴 | 기능은 있으나 숨겨짐 | 고정 바에 노출 |
| Black | `BlackScreenCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| Clear | `ClearOutputCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| Hide | `HideOutputCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| Restore | `RestoreOutputCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| Restart | `RestartCurrentItemCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| Refresh | `RefreshOutputCommand`, Output 메뉴, 고정 바 | 대응됨 | 유지 |
| Stop Live | `StopLiveCommand`, Output 메뉴, 고정 바 | 대응됨 | Close Output과 별도 명령으로 유지 |
| Open/Close Output | `OpenOutputCommand`, `CloseOutputCommand`, 고정 바 | 대응됨 | 유지 |
| 이전/다음 항목 | `PreviousItemCommand`, `NextItemCommand`, 중앙 하단 | 대응됨 | 중앙 하단 항목 이동 바에 유지 |
| 처음/마지막 항목 | `FirstItemCommand`, `LastItemCommand` | 대응됨 | 중앙 하단 유지 |
| 절 점프 | `AvailableSectionLabels`, `JumpToLyricsSectionCommand` | 대응됨 | 레이블 체계 문서화 필요 |
| PPT 슬라이드 이동 | `GoToSlideCommand`, `PreviousSlideCommand`, `NextSlideCommand` | 대응됨 | Preview/Output 문맥 차이 후속 확인 |
| 코드 표시 | `ToggleLyricsNotationsCommand` | 기능은 있으나 메뉴/인스펙터 중심 | 후속에서 첫 화면 노출 검토 |
| 조옮김 | `TransposeLiveUp/Down/ResetCommand` | 기능은 있으나 메뉴 중심 | 후속에서 첫 화면 노출 검토 |
| 배경 표시 모드 | `ApplyBackgroundModeCommand` | 기능은 있으나 메뉴/인스펙터 중심 | 후속에서 compact picker 검토 |

## 차이 유형

| 차이 | 사례 | 위험 |
| --- | --- | --- |
| 기능은 있지만 숨겨짐 | 코드/조옮김, 배경 모드 | 라이브 중 메뉴 탐색 필요 |
| 기능은 있으나 위치가 다름 | Output 상태가 LiveBar/인스펙터/출력 창에 분산 | Preview와 Output 혼동 |
| 기능 흐름이 다름 | PPT Preview/Output 썸네일 문맥 | 라이브 중 슬라이드 이동 실수 가능 |
| 기능 없음/불명확 | 일부 레거시 프리코러스/엔딩 라벨 의미 | 단축키 기억과 라벨 불일치 |

## 이번 증분에서 반영한 항목

- 고정 운영 바를 wrap 가능한 구조로 바꿔 1180 폭에서 버튼이 겹치거나 화면 밖으로 사라지지 않게 한다.
- `송출+다음`을 고정 운영 바에 추가한다.
- `Stop Live`를 고정 운영 바에 추가하되 `Close Output`과 분리한다.
- 중앙 헤더를 Preview 준비 상태와 Output 송출 상태로 나눠 선택 항목과 라이브 항목을 구분한다.
- WPF 최소 창 기준을 1180x760으로 올려 Classic Operator Layout의 기준 크기를 코드에 반영한다.

## 남은 후속

- 코드 표시/조옮김/배경 모드는 첫 화면 compact strip로 옮길 후보이나, 밀도와 실수 가능성을 함께 봐야 한다.
- Preview와 Output 각각의 PPT 썸네일 조작 문맥은 수동 QA가 필요하다.
