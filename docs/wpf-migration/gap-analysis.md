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
- **PPT 슬라이드 렌더**: 🟡 **부분 완료** — 단일 슬라이드는 실제 Office Interop(`OfficePptSession`, STA 워커, JPG export)로 렌더돼 운영자 PowerPoint 탭(`PowerPoint.PreviewImage`)에 표시되고, **GoLive 시 출력 창에도 송출**된다(2026-05-30, 신원 가드로 stale 슬라이드 방지). 출력 창이 열려 있으면 **출력 모니터 해상도(종횡비 보존, 1080p 상한)로 렌더**해 송출을 선명하게 한다(2026-05-30). **남은 것: (1) 썸네일 스트립(덱 전체 슬라이드) 미구현, (2) 항목 선택 *뒤* 출력 여는 경우 재렌더 트리거 후속, (3) 라이브 중 슬라이드 이동 시 출력 갱신 후속.**
- **미디어 재생 UI**: ✅ **연결 완료(2026-05-30, PR #48)** — orphaned 였던 `MediaPlaybackViewModel` 을 MainWindow Media 탭에 바인딩(G1.2)하고, 라이브 큐 선택→`MediaPlaybackService`→`AttachableMediaPlaybackBackend`(생명주기 브리지)→출력 창 `MediaElement` 로 실제 재생 체인을 연결(트랙1~3). 비-미디어 항목 전환 시 `Unload` 로 출력에서 미디어를 내려 가사 복귀(출력 패리티). **남은 것: 코덱·오디오/싱크·라이브 카메라 캡처의 라이브 패리티 미검증**(아래 출력 렌더 패리티와 함께 G1.4 스크린샷 회귀로 고정 대상).
- **출력 렌더 패리티**: `OutputWindow` 의 가사/성경/배경 실제 렌더가 레거시(`gfDisplay`/`gfLyrics` GDI+ 경로)와 동등한지 미검증.

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
4. **출력 렌더 패리티 검증**: OutputWindow 가사/성경/배경 렌더를 레거시와 스크린샷 비교로 고정.
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
- [ ] **다음 결정 대상**: PPT 썸네일 스트립/라이브 중 슬라이드 이동(G1.2 잔여) 또는 G1.4 출력 렌더 패리티(스크린샷 기준 확대) 중 착수 선택

## 7. 참조
- 도메인 계획: [01-shell-live-operations](01-shell-live-operations.md) ~ [06-verification-test-plan](06-verification-test-plan.md)
- [next-session-plan.md](next-session-plan.md) (남은 백로그 A/B/C)
- ADR-0001(WPF), ADR-0005(Options 분해), ADR-0007(안전망), ADR-0008(Core 추출)
- 산출물: `Easislides.exe`(legacy) / `EasislidesNext.exe`(WPF)
