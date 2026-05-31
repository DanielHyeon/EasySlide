# WinForms → WPF 마이그레이션 갭 분석 및 구현 계획

> 작성일: 2026-05-31 · 기준: `main` (Core 추출 B-3 #28~33 머지 후)
> **G0 정밀 검증 완료(2026-05-31)**: 초판의 ❓ 항목을 각 WPF 창/ViewModel/enum 직접 확인으로 확정. 여러 "갭"이 실은 통합 창에 포함됨이 밝혀져 갭이 초판보다 **작아졌다**(아래 §2 evidence 열·§4 반영).
> 목적: "어디까지 됐나"를 **실제 코드 근거**로 확정하고(레거시 38폼 ↔ WPF 창 커버리지), 남은 갭을 분류·우선순위화한 뒤 안전한 구현 계획을 수립한다.
> 방법: WPF 창은 매핑 주석이 없어, 매핑은 **창 Title + 기능 + enum/서비스 + 도메인 문서(01~06)** 로 확인. 잔여 추정은 ❓로 표시.

## 0. 한눈에 보기 (정량 현황)

| 지표 | 값 | 근거 |
|---|---|---|
| 레거시 폼 | **38개** (`Frm*.cs`, partial 제외) | `Easislides/Easislides/Frm*.cs` |
| WPF 실제 앱 창 | **19개** (+데모/갤러리 5) | `Easislides.Wpf/**/*Window.xaml` |
| 빌드/테스트 | 0 errors · WPF 514 green (2026-05-30, 미디어 트랙3 포함) + 분석기 20 | — |
| production entrypoint | **레거시(`Easislides.exe`)가 주력** | next-session-plan §B "WPF 아직 production 아님" |
| 마일스톤(기능 기준) | **M1 도달 / M2 부분 / M3·M4 미달** | 아래 §3·§4 |
| 알려진 문서 부채 | `Easislides.Wpf/README.md` 가 "Sprint 0 PoC" 로 **stale** | 실제 90 커밋·~6,800줄 XAML |

## 1. 상태 범례

| 표기 | 의미 |
|---|---|
| ✅ 포팅 | 전용 WPF 창 존재 + 기능 대응 |
| ➕ 통합 | 다중 레거시 폼이 한 WPF 창으로 합쳐짐 |
| 🟡 부분 | 창/서비스는 있으나 충실도·UI 연결 미완 |
| 🔴 미포팅 | WPF 대응 없음 |
| ❓ 확인필요 | title/기능 기반 추정(Phase G0 에서 정밀 검증) |

## 2. 커버리지 매트릭스 (도메인별)

### A. 운영 셸 / 라이브 송출 (도메인문서 01)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmMain` | `MainWindow` | 🟡 | 운영 셸·예배순서·Preview·LIVE/Black·모니터선택 O / **PPT·미디어 탭 placeholder** |
| `FrmLyricsScreen` | `OutputWindow` | 🟡 | Scene 렌더(전경/배경 브러시) O, 레거시 GDI+ 패리티 미검증 |
| `FrmInfoScreen` | — | 🔴 | **분류 정정(G2 조사)**: "정보화면"이 아니라 `tbLyrics1/2`+드래그드롭+코드 인디케이터를 가진 **가사 편집기(7,337줄, ~FrmEditItem 급 대형)**. 도메인 B(편집)로 재분류 대상 |
| `FrmLaunchShow` | `MainWindow` LIVE 흐름 | 🟡 | 풀스크린 launch 통합 추정 ❓ |
| `FrmShowAlert` | `OutputWindow`(LyricsAlertVisibility) + `EsToast` | 🟡 | 출력 화면 경고 오버레이 존재, 패리티 미검증 |
| `FrmSingleMonitorAlert` | — | 🔴 | 단일 모니터 경고 — **레거시에서 주석 처리(미사용)**, 저가치 |
| `FrmPopupText` | — (포팅 불요) | ⚪ | **분류 정정(#3 조사)**: 출력 송출이 아니라 **필드 텍스트 편집 팝업**. `FrmEditItem`/`FrmInfoScreen`/`FrmMain` 이 작은 단일행 필드값을 `Gf.popUpText` 에 넣고 큰 박스에서 편집 후 되돌려받는 WinForms 워크어라운드. WPF `SongEditorWindow`(18개 TextBox 인라인 편집)로 obsolete → **포팅 불요** |

### B. 콘텐츠 편집 / 라이브러리 (도메인문서 02)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmEditItem` | `SongEditorWindow` | ✅ | |
| `FrmEditBibleItem` | `BibleWindow` | 🟡 | BibleWindow 는 성경 검색/버전/미리보기(`MatchMode`/`PreviewRegionXVersion`) — **본문 편집 대응 미확인** |
| `FrmEditNotes` | `SongEditorWindow`(notation 부분) | 🟡 | 코드/노트 편집 전용 창 없음, SongEditor notation 부분 통합 |
| `FrmFind` | `SearchUsageWindow` | ➕✅ | `SongSearchFields`(Title/Lyrics/…) |
| `FrmUsages` | `SearchUsageWindow` | ➕✅ | UsageReport(rtf) |
| `FrmLookupTitles` | `SearchUsageWindow` | ➕✅ | `SongSearchFields.Title` — **G0 확정** |
| `FrmSmartMerge` | `SongMergeWindow` | ✅ | |
| `FrmManageItemLists` | — | 🔴 | 예배 리스트 관리 대응 없음 |
| (신규 개념) | `LibraryWindow` | ✅ | 라이브러리 허브(신규) |

### C. 곡/파일 관리

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmCopy` | `SongCopyWindow` | ✅ | |
| `FrmMove` | `SongMoveWindow` | ✅ | |
| `FrmCopyMoveExternal` | `ExternalFileOperationWindow` | ✅ | |
| `FrmRecoverDeleted` | `SongRecoveryWindow` | ✅ | |
| (곡 삭제) | `SongDeleteWindow` | ✅ | |
| `FrmRearrangeFolderPositions` | `FolderEditorWindow` | 🟡 | FolderEditor=폴더 생성/이름/번호 — 위치 정렬 전용 기능 미확인 |
| `FrmBibleRename` | — | 🔴 | 성경 이름변경 대응 없음 — **G0 확정** |
| `FrmUpdateFileName` | — | 🔴 | 파일명 갱신 대응 없음 — **G0 확정** |

### D. Import / Export / Generate (도메인문서 02·04)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmImport` | `ImportExportWindow` | ➕✅ | `ImportSourceKind`(Text/Xml/Database) |
| `FrmExport` | `ImportExportWindow` | ➕✅ | `ExportFormat`(Xml/Text/Database) |
| `FrmGenerateDoc` | `ImportExportWindow` | ➕✅ | `ExportFormat.Rtf` (문서=RTF) — **G0 확정** |
| `FrmGenerateHtml` | `ImportExportWindow` | ➕✅ | `ExportFormat.Html` — **G0 확정** |
| `FrmImportFolder` | `ImportExportWindow` | ➕✅ | `ImportSourceKind.DocumentFolder` — **G0 확정** |
| `FrmImportAccessHelper` | `ImportExportWindow` | ➕✅ | `ImportSourceKind.AccessDatabase` — **G0 확정**(초판 🔴 정정) |

### E. 렌더링 / Office / 미디어 (도메인문서 03)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmMediaPlayerControl` | `Media/MediaPlaybackService`+`ViewModel` | 🟡 | **연결 완료(2026-05-30)**: VM→MainWindow Media 탭 바인딩(G1.2) + 라이브 큐→서비스→브리지→출력 창 `MediaElement` 재생(트랙1~3, PR #48). 비-미디어 전환 시 `Unload`로 가사 복귀. **남은 것: 코덱·오디오/싱크 라이브 패리티 미검증** |
| `FrmLaunchMediaPlayer` | `Media/*` | 🟡 | 동상 — 위 체인으로 출력 재생 연결됨. 풀스크린 전용 launch UI/라이브 카메라 캡처 경로 패리티 미검증 |
| `FrmBackground` | — | 🔴 | **배경 설정 대응 없음(확인됨)** |

### F. 설정 / 데이터 / 자산 (도메인문서 04)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmOptions` | `SettingsWindow`(분해) | ✅ | ADR-0005 단일모달→Settings 페이지, WPF 31참조 |
| `FrmGetWorkingFolder` | `SettingsWindow` | ✅ | 작업 폴더 + `BrowseWorkingFolderCommand` — **G0 확정**(WelcomeWindow 아님) |

### G. 시작 / 정보 / 기타

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmAbout` | `AboutWindow` | ✅ | |
| `FrmHelp` | `HelpWindow` | ✅ | |
| `FrmRegister` | `RegistrationWindow` | ✅ | |
| `FrmSplashScreen` | `WelcomeWindow`(개념 상이) | 🟡 | WelcomeWindow=시니어 온보딩(§7.4), 스플래시(로딩) 별개 — 포팅 불요 후보 |
| `FrmSplashScreenOld` | — | 🔴 | 구 스플래시(레거시 잔재) — **포팅 불요** |

### 집계 (G0 검증 후)

| 상태 | 개수 |
|---|---|
| ✅ 포팅/➕통합 | **20** |
| 🟡 부분 | **10** |
| 🔴 미포팅 | **6** |
| ⚪ 포팅 불요 | **2** (`FrmSplashScreenOld`, `FrmPopupText`) |
| **합계** | 38 |

> 초판(추정) 14/12/9 → G0 검증 후 20/10/8 → G2 조사 후 **20/10/6 + 포팅불요 2**. `FrmPopupText` 가 출력 송출이 아니라 WPF 에서 obsolete 한 필드 편집 팝업으로 확인되어 포팅 불요로 재분류.

## 3. 마일스톤 평가 (기능 기준)

| 마일스톤 | 정의 | 평가 |
|---|---|---|
| **M1** | 신규 메인 폼으로 일반 송출 | **기능상 도달** — MainWindow + OutputWindow + LIVE/Black/모니터선택/예배순서/Preview |
| **M2** | 모든 T0/T1 신규(운영자 베타) | **부분** — 핵심 창은 있으나 렌더 충실도·미디어 UI 미완, T0/T1 전수 미검증 |
| **M3** | 모든 폼 신규(일반 베타) | **미달** — 미포팅 9 + 부분 12 |
| **M4** | v3.0 정식 출시 | **미달** — WPF 미전환(레거시 주력) |

## 4. 갭 분류 (성격별)

### G-α. 렌더링 충실도 갭 (라이브 핵심 · 최우선)
- **PPT 슬라이드 렌더**: 🟡 **부분 완료** — 단일 슬라이드는 실제 Office Interop(`OfficePptSession`, STA 워커, JPG export)로 렌더돼 운영자 PowerPoint 탭(`PowerPoint.PreviewImage`)에 표시되고, **GoLive 시 출력 창에도 송출**된다(2026-05-30, 신원 가드로 stale 슬라이드 방지). 출력 창이 열려 있으면 **출력 모니터 해상도(종횡비 보존, 1080p 상한)로 렌더**해 송출을 선명하게 하며, 항목을 먼저 고르고 **출력을 나중에 열어도 그 시점에 재렌더**해 선명함을 보장한다(2026-05-31). **라이브 중 PPT 슬라이드 이동(이전/다음 버튼)** 으로 덱 내 슬라이드를 넘기면 출력도 즉시 갱신된다(2026-05-31: PPT 덱은 자동 다음-항목 이동에서 제외해 선택이 라이브 덱에 머묾, 블랙아웃 중 이동 후 재개 시에도 이동한 슬라이드 송출). **덱 전체 슬라이드 썸네일 스트립**(PowerPoint 탭, 클릭으로 해당 슬라이드 이동·라이브 중이면 출력 갱신, 현재 슬라이드 강조)도 백그라운드 로딩으로 구현됨(2026-05-31). **남은 것(백로그): (1) 썸네일 렌더의 `CancellationToken` 을 `OfficePptSession.ExportSlideAsync` 까지 관통(덱 빠른 전환 시 큐된 stale export 스킵), (2) `PowerPointPreviewViewModel.LoadAsync` 동시 호출 순서 보장(generation/취소), (3) 썸네일·메인 미리보기가 같은 슬라이드를 다른 크기로 이중 렌더(효율).**
- **미디어 재생 UI**: ✅ **연결 완료(2026-05-30, PR #48)** — orphaned 였던 `MediaPlaybackViewModel` 을 MainWindow Media 탭에 바인딩(G1.2)하고, 라이브 큐 선택→`MediaPlaybackService`→`AttachableMediaPlaybackBackend`(생명주기 브리지)→출력 창 `MediaElement` 로 실제 재생 체인을 연결(트랙1~3). 비-미디어 항목 전환 시 `Unload` 로 출력에서 미디어를 내려 가사 복귀(출력 패리티). **남은 것: 코덱·오디오/싱크·라이브 카메라 캡처의 라이브 패리티 미검증**(아래 출력 렌더 패리티와 함께 G1.4 스크린샷 회귀로 고정 대상).
- **출력 렌더 패리티**: 🟡 **부분 검증(G1.4, 2026-05-31)** — 출력 배경(솔리드/세로 그라데이션)은 실제 `OutputWindowViewModel.SceneBackgroundBrush` 를 렌더해 스크린샷 기준(`output-bg-solid`/`output-bg-gradient`)으로 고정(텍스트 없는 결정적 표면). 텍스트·레이아웃 패리티(가사/타이틀·표시 여부·콘텐츠 배치)는 `OutputWindowViewModelTests` 속성 검증이 담당. **남은 것: 레거시 GDI+ 경로(`gfDisplay`/`gfLyrics`)와의 1:1 시각 대조는 미수행(폰트 렌더 비결정성으로 픽셀 비교 부적합), 블랙아웃 등 추가 표면은 후속.**

### G-β. 미포팅 폼 갭 (기능 부재 — G0 확정)
- 🔴 **확정 미포팅(7)**: `FrmBackground`(배경 설정), `FrmInfoScreen`(보조 모니터 정보화면), `FrmManageItemLists`(예배 리스트 관리), `FrmPopupText`(팝업 송출), `FrmSingleMonitorAlert`(단일 모니터 경고), `FrmBibleRename`(성경 이름변경), `FrmUpdateFileName`(파일명 갱신).
- 🟡 부분(전용 창 없음, 통합 일부): `FrmEditNotes`(SongEditor notation 부분), `FrmEditBibleItem`(BibleWindow=선택/미리보기, 본문 편집 미확인).
- ✅ **초판 갭에서 정정**: `FrmImportAccessHelper`(→ `ImportSourceKind.AccessDatabase`), `FrmGenerateDoc/Html`(→ `ExportFormat.Rtf/Html`), `FrmImportFolder`(→ `DocumentFolder`), `FrmLookupTitles`(→ `SongSearchFields.Title`), `FrmGetWorkingFolder`(→ Settings) 는 **통합 창에 포함됨**.

### G-γ. 횡단/인프라 갭
- ✅ **스크린샷 회귀 자동화 구축 완료**(§9.1, [screenshot-regression.md](screenshot-regression.md)) — 헤드리스 렌더 하니스(`VisualRenderHarness`)·허용오차 비교기(`ImageComparer`)·승인 기준(`ScreenshotBaseline`)·light/dark 토큰 스와치 기준 + CI 워크플로우. 렌더 충실도/리팩토링 안전망의 전제 충족. **남은 것: 주요 컨트롤·창 레이아웃 기준 확대(후속), CI 1회차 헤드리스 렌더 확정.**
- **문서 부채**: `Easislides.Wpf/README.md` "Sprint 0 PoC" stale → 실제 진척 반영 필요.
- **운영 전환 게이트 미통과**(B-4): 두 exe + Core.dll 동봉 패키징 + 1시간 예배 리허설.

## 5. 구현 계획

> 원칙: ADR-0007 안전망(`--legacy-ui` + legacy 별도 다운로드) 위에서, **라이브 위험이 큰 렌더링부터** 안전망(스크린샷 회귀)을 깔고 1건씩. TDD·작은 PR·code-reviewer 검증·문서 동기화(CLAUDE.md §9).

### Phase G0 — 정밀 검증 & 문서 정합 (저위험, 즉시)
1. **커버리지 정밀 검증**: 본 매트릭스의 ❓ 항목을 각 WPF 창/ViewModel 직접 확인으로 확정(특히 OutputWindow=가사/성경/정보 분담, SongEditor=노트 통합 여부, FolderEditor·ImportExport 통합 범위).
2. **WPF 창 헤더에 `// 대체: FrmX` 매핑 주석** 추가 → 향후 추적 비용 0.
3. **README 상태 갱신**(stale "Sprint 0 PoC" → 실제 진척 + 본 갭 문서 링크).
- 산출: 확정된 커버리지 매트릭스(이 문서 갱신), 매핑 주석 PR.

### Phase G1 — 렌더링 충실도 안전망 + 핵심 렌더 (중·고위험)
1. ✅ **스크린샷 회귀 PoC**(완료, [screenshot-regression.md](screenshot-regression.md)): 헤드리스 `RenderTargetBitmap` 렌더 + 허용오차 비교 + 승인 기준(light/dark 토큰 스와치) + CI. **G1 이후 모든 렌더 작업의 안전망 확보**. (단 CI 1회차 헤드리스 렌더 가능 여부는 GitHub Actions 실행으로 확정.)
2. **PPT 썸네일/슬라이드 렌더**: `gf.PreviewPPT.BuildScreenPreDumps`(OfficeLib) 산출을 WPF Preview/썸네일 스트립에 연결. MainWindow PowerPoint 탭 placeholder 대체.
3. ✅ **미디어 재생 UI 연결**(완료 2026-05-30, PR #48): orphaned `MediaPlaybackViewModel` → MainWindow Media 탭 바인딩(G1.2) + 라이브 큐→서비스→브리지→출력 `MediaElement` 재생 체인(트랙1~3) + 비-미디어 전환 시 `Unload` 가사 복귀. 라이브 코덱/싱크 패리티는 4(스크린샷 회귀)와 함께 검증 예정.
4. ✅ **출력 렌더 패리티 검증**(부분, 2026-05-31): 출력 배경(솔리드/그라데이션)을 스크린샷 기준으로 고정(G1.4). 텍스트는 비결정성으로 제외 — 속성 검증이 담당. 레거시 GDI+ 1:1 대조는 미수행.
- 게이트: 각 항목 스크린샷 회귀 통과 + `--legacy-ui` 롤백 유지.

### Phase G2 — 미포팅 폼 (중위험, 1건씩)
- ✅ **완료**: `FrmBackground` → 가사 배경 세로 그라데이션(#38).
- **G2 조사 발견(선결 인프라 게이트)**: 남은 폼 대부분이 깔끔한 단일 PR 이 아니라 **선결 인프라**를 요구한다.
  - `FrmInfoScreen`: 분류 정정 — 7,337줄 **가사 편집기**(FrmEditItem 급). 단일 PR 부적합 → 대형 편집기 마이그레이션으로 별도 트랙.
  - `FrmManageItemLists`·`FrmPopupText`: WPF 라이브 셸이 **placeholder 큐**(`SeedPlaceholderQueue`)라 실제 항목/큐 도메인 plumbing 선결 필요.
  - `FrmBibleRename`/`FrmUpdateFileName`: `IBibleRepository`/곡 데이터가 **읽기 전용** → 쓰기(rename) 경로 선결 필요.
  - `FrmSingleMonitorAlert`: 레거시에서 주석 처리(미사용) — 저가치.
- **권장 다음 트랙**: (a) 실제 라이브 큐 도메인 plumbing(LiveQueueItem 확장 + 항목 로드) — 다수 라이브-운영 폼의 공통 선결, 또는 (b) 데이터 계층 쓰기 경로(rename/update) — 유틸 폼들의 공통 선결, 또는 (c) 대형 편집기(EditItem/InfoScreen) 마이그레이션 트랙.
- 각 폼: ViewModel 단위 테스트 우선 → View 구현 → 동작 동등성 확인.

### Phase G3 — 운영 전환 게이트 (B-4)
- 두 exe + `Core.dll` 동봉 패키징, 1시간 예배 리허설 시나리오 통과를 production 전환 게이트로(ADR-0007 sunset 일정과 연동).

### 권장 착수 순서
```
G0 (즉시·저위험) → G1.1 스크린샷 PoC → G1.2~G1.4 렌더 → G2 (1건씩) → G3 게이트
```
- next-session-plan §2(C 스크린샷 → A 컴포지트 → B 전환)와 정합: **C = G1.1**, **A(컴포지트) = G1 진행 중 병행**, **B = G3**.

## 6. 즉시 액션
- [x] ❓ 항목 정밀 검증으로 매트릭스 확정(G0-1) — **완료**(본 문서 §2 evidence·집계 갱신)
- [x] WPF 창 헤더에 `// 대체: FrmX` 매핑 주석(G0-2) — **완료**
- [x] README stale 갱신(G0-3) — **완료**
- [x] 스크린샷 회귀 PoC(G1.1) — **완료**(인프라·light/dark 기준·CI 구축, [screenshot-regression.md](screenshot-regression.md))
- [x] 미디어 재생 UI 연결(G1.3) — **완료**(트랙1~3, PR #48: 라이브 큐→서비스→브리지→출력 `MediaElement` + 비-미디어 전환 시 `Unload`)
- [x] G1.2 PPT 슬라이드 출력 송출 — **완료**(2026-05-30: 단일 슬라이드 렌더는 기존 완료, GoLive 시 출력 창 송출 추가 + 신원 가드로 stale 방지)
- [x] G1.2 후속 PPT 출력 해상도 렌더 — **완료**(2026-05-30: 출력 창 열림 시 출력 모니터 해상도(종횡비 보존·1080p 상한)로 렌더 → 송출 선명)
- [x] G1.2 후속 선택-후-출력열기 재렌더 — **완료**(2026-05-31: 항목을 먼저 고르고 출력을 나중에 열어도 그 시점에 출력 해상도로 재렌더)
- [x] G1.2 라이브 중 PPT 슬라이드 이동 — **완료**(2026-05-31: 이전/다음 버튼으로 덱 내 슬라이드 이동 + 라이브 출력 즉시 갱신, PPT 덱은 자동 advance 제외)
- [x] G1.2 PPT 덱 썸네일 스트립 — **완료**(2026-05-31: PowerPoint 탭에 덱 전체 슬라이드 썸네일·클릭 이동·현재 강조, 백그라운드 로딩). **G1.2 PPT 트랙 사실상 완결**(잔여는 백로그 §G-α)
- [x] G1.4 출력 렌더 패리티(부분) — **완료**(2026-05-31: 출력 배경 솔리드/그라데이션 스크린샷 기준 고정. 텍스트는 비결정성으로 제외, 속성 검증이 담당)
- [x] G1.4 출력 가사 송출 1차 — **완료**(2026-05-31, PR #63: `LyricsDisplayFormatter.ToDisplayText`로 마커 제거 + `OutputWindow.BodyText` TextBlock 렌더. 곡 항목 GoLive 시 회중 화면에 가사 텍스트 송출, 본문 보일 때 타이틀 자동 숨김.)
- [x] G1.4 절 단위 페이지네이션 + Next/Prev (PR B) — **완료**(2026-05-31: `LyricsDisplayFormatter.ToVersePages`/`GetVersePage`로 절 단위 분할, `LiveQueueItem.LyricsPageIndex`, `MainViewModel.NextLyricsPageCommand`/`PreviousLyricsPageCommand` + `LyricsPageLabel`. GoLive 시 현재 절만 출력, 라이브 중 이전/다음 절 이동 → 출력 즉시 갱신. MainWindow Preview 탭에 "이전 절/다음 절" 버튼 + 페이지 표시(N/M절). 테스트 591개 Green.)
- [x] UI/UX 갭 분석(FrmMain ↔ MainWindow, 미이식 폼) — **완료**(2026-05-31, 아래 §7) — 단일 콘솔 통합 설계 제안
- [x] §7.5 P0 — 인-셸 **출력 모양 인스펙터**(1차) — **완료**(2026-05-31: MainWindow 우측에서 글자색·배경 프리셋을 모달 없이 즉시 적용→라이브 반영). 단일 콘솔 통합 첫걸음.
- [x] §7.5 P0 — 인라인 **콘텐츠 브라우저(곡)** 1차 — **완료**(2026-05-31: 좌측 "예배 순서"·"라이브러리" 탭, 폴더·곡 검색·더블클릭 추가. 별도 LibraryWindow 없이 셸에서 곡 추가). 모달 LibraryWindow 진입점은 한동안 병존(백로그: 제거/대체).
- [x] §7.5 P0 — 인라인 **성경 브라우저** 1차 — **완료**(2026-05-31: 좌측 "성경" 탭, BibleVerseFinder 재사용 + 본문 구절 드래그 선택→예배 순서 추가. 별도 BibleWindow 없이 셸에서 성경 추가). → **좌측 단일 콘솔(예배 순서/라이브러리/성경) 완성**
- [x] §7.3-B 라이브 화면 제어(숨김·복귀) — **완료**(2026-05-31: 운영바에 "숨김"(HideOutput)·"복귀"(Restore, Hidden→Active 콘텐츠 보존) 노출)
- [x] §7.3-B 예배 순서 항목 이동(↑/↓)·제거 — **완료**(2026-05-31, PR #61: WorshipListPanel ↑/↓/제거 버튼. 값 동등성 record 중복은 ReferenceEquals 기반으로 정확한 인스턴스 이동/제거, 라이브 항목 제거 시 _liveItemId 고아 정리)
- [x] §7.3-B 화면 제어 보강(비우기·처음으로·새로고침) — **완료**(2026-05-31: 비우기(Clear)=콘텐츠 감추고 배경 유지(`OutputSceneKind.Cleared`/`IsCleared`, Black과 상호배타·복귀 가능), 처음으로(Restart)=라이브 곡 첫 절/PPT 첫 슬라이드 재송출(이미 1슬라이드면 Refresh 폴백), 새로고침(Refresh)=출력 강제 재렌더(SessionChanged 재발생). 테스트 612 green, code-reviewer 3건 반영)
- [x] §7.3-A 인-셸 가사 정렬(가로 좌/중/우 + 세로 상/중/하) — **완료**(2026-05-31: `LyricsTextAlignment`/`LyricsVerticalAlignment` 설정 키 + 렌더 스레딩 + `OutputWindowViewModel` enum→WPF(TextAlignment/HorizontalAlignment/VerticalAlignment) 매핑 + 우측 인스펙터 버튼. SettingsChanged 화이트리스트로 라이브 즉시 반영, 기본 Center 로 기존 동작 보존. 테스트 636 green, 두 증분 모두 code-reviewer APPROVE. 가로 증분에서 발견한 영속화 타입스위치 누락 회귀는 세로 증분에 선제 반영해 무회귀)
- [x] §7.3-A 인-셸 가사 폰트 크기(A−/A+, 24~120px) — **완료**(2026-05-31: `LyricsMonitorFontSize` int 설정→렌더→출력, `BodyFontSize`/`BodyLineHeight`(×1.25) 매핑, 우측 인스펙터 A−/A+ + 현재 px 표시. ±4 step·경계 비활성·Math.Clamp 삼중 가드. 테스트 645 green, code-reviewer Approve + SUGGESTION 2건(비배수 클램프 테스트·배율 상수화) 반영)
- [x] §7.3-A 인-셸 가사 폰트 효과(굵게·기울임·그림자) — **완료**(2026-05-31: `LyricsMonitorBold/Italic/Shadow` bool 설정→렌더→출력, `BodyFontWeight`(off=SemiBold 보존)/`BodyFontStyle`/`BodyHasShadow`(DropShadowEffect) 매핑, 우측 인스펙터 토글 버튼. 테스트 657 green, code-reviewer Approve + MINOR 2건 반영. 기본 전부 off 로 기존 출력 무변화)
- [x] §7.3-A 인-셸 가사 줄 간격(−/＋, 100~220%) — **완료**(2026-05-31: `LyricsMonitorLineSpacingPercent` int 설정→렌더→출력, 기존 하드코딩 줄높이(폰트×1.25)를 설정 기반(폰트×%)으로 전환, 기본 125% 로 동작 보존. 우측 인스펙터 −/＋ + 현재 % 표시. 테스트 664 green, code-reviewer Approve)
- [x] §7.3-A 세분 색 직접 지정(글자/배경 hex #RRGGBB) — **완료**(2026-05-31: 기존 색 설정 재사용(신규 키 없음), `ApplyTextColorHexCommand`/`ApplyBackgroundColorHexCommand` + hex 파싱(6자리 불투명, 표시 대칭) + 우측 인스펙터 입력칸·Enter·적용 버튼. 배경은 솔리드 전환. 테스트 672 green, code-reviewer Approve + SUGGESTION(8자리 거부·전환/추종 테스트) 반영)
- [ ] **다음 착수(권장)**: 헤딩(제목/절 표시) / Outline 효과 / 모달 Library·Bible 진입점 정리 / 자동 회전(Group/One·Repeat·Stop)
- [ ] **G1.4 백로그(선택)**: 썸네일 렌더 `CancellationToken` 관통 / `LoadAsync` 동시 호출 순서 보장 / 이중 렌더 효율화 (§G-α 잔여)

## 7. UI/UX 갭 분석 — FrmMain ↔ MainWindow (단일 콘솔 통합)

> 작성일 2026-05-31 · 방법: `FrmMain.Designer.cs`(레거시 단일 운영창)와 `MainWindow.xaml`(WPF 셸)의 실제 UI 표면을 추출·대조하고, UI/UX 스킬(ui-ux-pro-max)의 **Data-Dense Dashboard** 패턴·키보드 우선 접근성 가이드를 적용해 현대적 재해석을 제안한다.
> 사용자 요구: **"흩어진 별도 창이 아니라 FrmMain 처럼 MainWindow 한 곳에서 모든 동작"** + FrmMain 을 현대적으로 더 편리하게.

### 7.1 정량 표면 대조

| 표면 | FrmMain (레거시) | MainWindow (WPF) | 격차 |
|---|---|---|---|
| 메뉴 항목 | **136** (MenuStrip 6대 메뉴 File/Edit/View/Output/Tools/Help) | **0** (메뉴바 없음) | 🔴 전면 부재 |
| 툴바 버튼 | **93** (ToolStrip 30개, 포맷/정렬/색/전환 등) | ~16 하단 액션(대부분 별도 창 오픈) | 🔴 인-셸 포맷팅 0 |
| 패널/탭 | Panel 57, TabPage 10, TabControl 3, SplitContainer 5(가변 레이아웃) | 3열 고정 그리드 + 탭 3(Preview/PowerPoint/Media) | 🟡 고정·저밀도 |
| 목록 뷰 | ListView 8 (폴더·곡목록·PPT·이미지·미디어·사용처…) | WorshipListPanel 1 (예배 순서만) | 🔴 인라인 브라우징 부재 |
| 수치 입력 | NumericUpDown 8 (여백·크기·전환…) | 0 (Settings 창에 분산) | 🔴 |

핵심: MainWindow 는 **얇은 디스패처 셸**이다 — 좌(예배순서)·중(미리보기 탭)·우(운영상태) 3열 + 하단 버튼이 **라이브러리/성경/Search/I-O/외부파일/설정/편집기**를 각각 **모달/별도 창**으로 띄운다. FrmMain 은 그 전부를 **한 창의 패널·탭·메뉴**로 처리했다.

### 7.2 핵심 UX 문제 — 분산 창 vs 단일 콘솔

1. **컨텍스트 단절**: 라이브 송출 중 곡을 고르려면 별도 라이브러리 창을 띄우고(미리보기 가림), 포맷을 바꾸려면 Settings 모달을 연다 — 예배 진행 중 치명적. UX 가이드 *"100 tabs to reach content"* anti-pattern 의 데스크톱판.
2. **인-셸 렌더링 제어 전무**: 운영자가 라이브 중 즉시 바꾸던 정렬·폰트효과·리전·헤딩·배경·전환·코드/조옮김이 WPF 셸엔 **없다**(일부만 Settings 모달). 워십 운영의 본질 기능.
3. **저밀도·키보드 미흡**: 3열 고정·여백 큼(데스크톱 운영 콘솔엔 부적합) + 라이브 단축키는 next/prev/black 정도. FrmMain 은 메뉴 가속키·툴바로 키보드 운영이 촘촘했다.

### 7.3 기능 갭 매트릭스 (FrmMain 실제 라벨 기준)

#### A. 인-셸 렌더링/포맷팅 (🔴 거의 전무 — 최우선 통합 대상)

| 기능군 | FrmMain 항목(발췌) | WPF 현황 |
|---|---|---|
| 텍스트 정렬·크기·줄간격 | Align Left/Right/Centre/Top/Bottom, Vertical Alignment, (글자 크기·줄 간격) | ✅ 가로(좌/중/우)+세로(상/중/하) 정렬 + **폰트 크기 A−/A+(24~120px)** + **줄 간격 −/＋(100~220%)** 인-셸 인스펙터 완료(2026-05-31, `LyricsTextAlignment`/`LyricsVerticalAlignment`/`LyricsMonitorFontSize`/`LyricsMonitorLineSpacingPercent` 설정→렌더→출력, 우측 패널·라이브 즉시 반영) |
| 폰트 효과 | Shadow Font, Outline Font, Italics / No Italics / Chorus Italics Only | 🟡 **굵게·기울임·그림자 토글 완료**(2026-05-31, `LyricsMonitorBold/Italic/Shadow` 설정→렌더→출력, 우측 인스펙터 토글·라이브 즉시 반영, 그림자=DropShadowEffect). Outline·Chorus-only-italics 는 후속 |
| 리전(2단) | Region 1/2 Only, Regions 1&2, Interlace, R1/R2 Colour, Region n Align L/R/C, Text Colour As Region 1 | 🔴 없음(렌더는 전경/배경 브러시만) |
| 헤딩(제목/절) | Show All/No Headings, Heading Align L/R/C/As Region, Heading At First Screen Only, Display Title/Verse Headings | 🔴 없음 |
| 배경 | Background Colours and Patterns, Background Picture Format(Best Fit/Centre/Tile/Size), No/Transparent/Default Background, Back Colour | 🟡 솔리드·세로 그라데이션 + **글자/배경색 hex 직접 지정**(2026-05-31, 프리셋 4종 + 임의 #RRGGBB 입력) 완료. 이미지/패턴/타일은 후속 |
| 전환 효과 | Slide Transition, Item Transition | 🟡 TransitionEffectService 있으나 UI 노출 미약 |
| 코드/악상 | Show Notations(+in Preview), Transpose Up/Down Semi-Tone, To Capo 0 | 🔴 없음 |
| 인디케이터 | Show Verse/Slide Indicators, Show Item Number, Show Title, Use Song Numbering | 🔴 없음 |
| 설정 템플릿 | Save/Load Settings Template, Use Individual Settings, Apply to All Except InfoScreens, Default Layout | 🔴 없음 |

#### B. 라이브 운영 제어 (🟡 핵심 일부만)

| 기능 | FrmMain | WPF |
|---|---|---|
| Go LIVE / Black / Next·Prev | Start Show-Go LIVE, Black Screen, Move Next | ✅ 있음 |
| 화면 제어 | Clear Screen, Hide Text, Refresh Output, Restart Current Item | ✅ Black + 숨김(Hide)·복귀(Restore) + **비우기(Clear)·처음으로(Restart)·새로고침(Refresh)** 운영바 노출(2026-05-31). 비우기=배경유지(Black과 구별, `OutputSceneKind.Cleared`), 처음으로=라이브 곡 첫 절/PPT 첫 슬라이드 재송출, 새로고침=출력 강제 재렌더 |
| 자동 회전 | Auto Rotate Group/One Item(+Repeat), Stop Auto Rotate, Rotate Style | 🔴 없음 |
| Gap/안내 | Gap Item, Alerts(경고 오버레이) | 🟡 출력측 오버레이만, 조작 UI 없음 |
| 보조 화면 | InfoScr, Copy to InfoScreen, Apply to All Except InfoScreens | 🔴 없음(FrmInfoScreen 미이식) |
| 미디어 출력 | Play Media (on Output Monitor) | ✅ G1.2 트랙으로 연결 |
| 항목 이동 | Move Item Up/Down | ✅ 예배 순서 패널 ↑/↓ 이동·제거(2026-05-31, PR #61). 값 동등성 중복은 참조 기반으로 안전 처리 |

#### C. 인라인 콘텐츠 브라우징 (🔴 별도 창으로 분산)

| 기능 | FrmMain | WPF |
|---|---|---|
| 폴더 트리 + 곡 목록 | Folders + Listing of Selected Folder(2-pane) | 🔴 별도 `LibraryWindow` 모달 |
| PraiseBook | Add/Manage/Clear PraiseBooks | 🔴 없음 |
| 워십 세션/최근 | Worship Sessions, Recent Edits, Edit Session Notes | 🔴 없음 |
| 성경 인라인 | Bibles, Select typed-in reference, Search Phrase, Add Region 2 | 🔴 별도 `BibleWindow` |
| 이미지/미디어 목록 | Images, Media, Refresh Images Lists, Powerpoint Listing/Preview | 🟡 미디어/PPT 탭은 있으나 폴더 브라우징 없음 |

#### D. 데이터/관리 (✅ 통합 창에 포함 — 단 별도 창)
Import/Export·Generate RTF/HTML·Copy/Move/Delete·Recover·Smart Merge·Search/Usages 는 §2 매트릭스대로 **WPF 창으로 포팅됨**(미이식 아님). 다만 모두 **별도 다이얼로그** — 7.4 통합 대상.

### 7.4 현대적 재해석 — 단일 콘솔 통합 설계

> 원칙(ui-ux 스킬): **Data-Dense Dashboard**(고밀도·최소 패딩·그리드·최대 가시성) + 키보드 우선 + "색상 단독 의존 금지"(LIVE 상태=아이콘+텍스트) + 비동기 콘텐츠 공간 예약(미리보기/썸네일 레이아웃 점프 방지).

제안 레이아웃 — **3-구역 도킹 콘솔 + 컨텍스트 인스펙터**(별도 창 제거):

```
┌───────────────────────────────────────── Command/Top bar (LIVE 상태·전역 검색·⌘K 명령 팔레트) ─┐
│ ┌─ 좌: 콘텐츠 브라우저(도킹) ─┐ ┌─ 중앙: 미리보기/송출 ─┐ ┌─ 우: 컨텍스트 인스펙터 ─┐ │
│ │ 폴더 트리 + 곡/성경/PPT/   │ │ 라이브 미리보기        │ │ (선택 항목 종류별 탭)    │ │
│ │ 미디어/이미지 목록 탭       │ │ + PPT 썸네일 스트립     │ │ · 가사: 정렬·폰트·리전·  │ │
│ │ (인라인 검색·필터)          │ │ + Next/Prev/Go Live    │ │   헤딩·배경·전환·코드    │ │
│ └────────────────────────────┘ │                        │ │ · PPT: 슬라이드·전환     │ │
│ ┌─ 하단 좌: 예배 순서 큐(도킹) ─┐ │                        │ │ · 미디어: 재생·볼륨      │ │
│ │ 드래그 재정렬·자동회전·Gap   │ │                        │ │ · 성경: 버전·구절        │ │
│ └────────────────────────────┘ └────────────────────────┘ └──────────────────────────┘ │
└─ 하단: 운영 바(출력 모니터·LIVE/Black/Clear/Hide/Restart·자동회전·InfoScreen) ───────────────┘
```

핵심 패턴:
1. **컨텍스트 인스펙터**(우측 도킹): 선택 항목 종류(곡/성경/PPT/미디어)에 따라 **인-셸 포맷팅 컨트롤**을 표시 — §7.3-A 의 정렬·폰트·리전·헤딩·배경·전환·코드/조옮김을 모달 없이 즉시 조작. Settings 모달은 "전역 기본값"만 남기고 **항목별 라이브 조정은 인스펙터**로 이동.
2. **인라인 콘텐츠 브라우저**(좌측 도킹): `LibraryWindow`/`BibleWindow` 의 폴더-트리+목록을 셸에 도킹(탭: 곡/성경/PPT/미디어/이미지). 더블클릭/Enter 로 큐 추가. 별도 창 제거(검색·필터 인라인).
3. **명령 팔레트(⌘K/Ctrl+K)**: 136개 메뉴를 단축키+검색 가능한 명령 팔레트로 흡수 — "100탭" 문제 해소, 키보드 우선 운영. 자주 쓰는 라이브 동작은 전역 단축키(이미 `ShortcutRegistry` 존재) 확장.
4. **도킹/레이아웃 저장**: SplitContainer 의 현대판 — 도킹 패널(접기/크기조절) + "Default Layout"/사용자 레이아웃 저장(FrmMain 의 Default Layout 계승).
5. **운영 바 강화**: Black 외 Clear/Hide/Restart/Refresh + 자동회전(시작/정지/Repeat) + InfoScreen 토글. 상태는 아이콘+텍스트(색상 단독 금지).
6. **밀도 상향**: 3열 고정·큰 여백 → 도킹 그리드·조밀 간격(데스크톱 운영 콘솔). 큐는 드래그 재정렬.

### 7.5 우선순위 로드맵 (라이브 운영 가치 기준)

- **P0 (운영 본질·최우선)**
  - (a) **컨텍스트 인스펙터 + 인-셸 가사 포맷팅**: 🟡 진행 중 — **가로·세로 정렬 완료**(2026-05-31, 우측 인스펙터 버튼 → 라이브 즉시 반영). 배경색/그라데이션은 출력 모양 프리셋으로 기완료. **잔여: 폰트효과·리전·헤딩·세분 색 ColorPicker·폰트 크기**.
  - (b) **인라인 콘텐츠 브라우저**: LibraryWindow/BibleWindow 를 좌측 도킹 패널로 흡수(별도 창 제거).
  - (c) **화면 제어 보강**: ✅ Clear/Hide/Restart/Refresh 완료(2026-05-31). InfoScreen 토글은 FrmInfoScreen 미이식이라 후속.
- **P1 (운영 편의)**
  - 명령 팔레트(⌘K) + 단축키 확장 / 자동 회전(Group/One, Repeat, Stop) / 코드 표기·조옮김 인스펙터 / 큐 드래그 재정렬.
- **P2 (구조·완성)**
  - 도킹 레이아웃 + 저장(Default/사용자 레이아웃) / 설정 템플릿(Save/Load, Apply to All Except InfoScreens) / PraiseBook·워십 세션·세션 노트 / 배경 이미지·패턴·전환 UI / 미이식 폼(FrmInfoScreen=가사편집기, FrmManageItemLists, FrmBibleRename, FrmUpdateFileName) 통합.

### 7.6 "미이식" 오해 정정

사용자 인상("다른 폼들이 전혀 이식 안 됨")과 달리, §2 매트릭스 기준 **20개가 포팅/통합**됨 — 다만 대부분 **별도 다이얼로그**라 "흩어져" 보인다(통합이 진짜 과제). **실제 미이식(🔴)** 은 6개: `FrmInfoScreen`(실은 7,337줄 가사 편집기), `FrmManageItemLists`, `FrmBibleRename`, `FrmUpdateFileName`, `FrmSingleMonitorAlert`(레거시 미사용), 배경 설정 일부. → 통합(7.4) + 미이식 6 처리가 M3 의 핵심.

## 8. 참조
- 도메인 계획: [01-shell-live-operations](01-shell-live-operations.md) ~ [06-verification-test-plan](06-verification-test-plan.md)
- [next-session-plan.md](next-session-plan.md) (남은 백로그 A/B/C)
- ADR-0001(WPF), ADR-0005(Options 분해), ADR-0007(안전망), ADR-0008(Core 추출)
- 산출물: `Easislides.exe`(legacy) / `EasislidesNext.exe`(WPF)
