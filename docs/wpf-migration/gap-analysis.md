# WinForms → WPF 마이그레이션 갭 분석 및 구현 계획

> 작성일: 2026-05-31 · 기준: `main` (Core 추출 B-3 #28~33 머지 후)
> 목적: "어디까지 됐나"를 **실제 코드 근거**로 확정하고(레거시 38폼 ↔ WPF 창 커버리지), 남은 갭을 분류·우선순위화한 뒤 안전한 구현 계획을 수립한다.
> 방법: WPF 창은 매핑 주석이 없어, 매핑은 **창 Title + 기능 + 도메인 문서(01~06) 의도** 기반이다. 추정 항목은 ❓로 표시했고, 정밀 검증은 Phase G0 에 포함한다.

## 0. 한눈에 보기 (정량 현황)

| 지표 | 값 | 근거 |
|---|---|---|
| 레거시 폼 | **38개** (`Frm*.cs`, partial 제외) | `Easislides/Easislides/Frm*.cs` |
| WPF 실제 앱 창 | **19개** (+데모/갤러리 5) | `Easislides.Wpf/**/*Window.xaml` |
| 빌드/테스트 | 0 errors · 474 green (WPF 454 + 분석기 20) | — |
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
| `FrmLyricsScreen` | `OutputWindow` | 🟡 | 출력 창 존재, 가사 렌더 충실도 ❓ |
| `FrmInfoScreen` | `OutputWindow`(정보 출력) | 🟡❓ | 별도 정보화면 대응 ❓ |
| `FrmLaunchShow` | `MainWindow` LIVE 흐름 | 🟡 | 풀스크린 launch 통합 추정 ❓ |
| `FrmShowAlert` | `Controls/EsToast` | 🟡 | 토스트 알림(개념 상이) — 화면 송출 경고 대응 ❓ |
| `FrmSingleMonitorAlert` | — | 🔴 | 단일 모니터 경고 대응 없음 ❓ |
| `FrmPopupText` | — | 🔴 | 팝업 텍스트 송출 대응 없음 |

### B. 콘텐츠 편집 / 라이브러리 (도메인문서 02)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmEditItem` | `SongEditorWindow` | ✅ | |
| `FrmEditBibleItem` | `BibleWindow` / `SongEditorWindow` | 🟡❓ | 성경 항목 편집 분담 ❓ |
| `FrmEditNotes` | — | 🔴 | 코드/노트 편집 전용 대응 없음(SongEditor 에 notation 5참조 — 부분 통합 ❓) |
| `FrmFind` | `SearchUsageWindow` | ➕✅ | 검색 통합 |
| `FrmUsages` | `SearchUsageWindow` | ➕✅ | 사용현황 통합 |
| `FrmLookupTitles` | `SearchUsageWindow` | ➕❓ | 제목 조회 통합 추정 |
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
| `FrmRearrangeFolderPositions` | `FolderEditorWindow` | 🟡❓ | 폴더 정렬 통합 추정 |
| `FrmBibleRename` | — | 🔴❓ | 성경 이름변경 대응 없음 |
| `FrmUpdateFileName` | — | 🔴❓ | 파일명 갱신 대응 없음 |

### D. Import / Export / Generate (도메인문서 02·04)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmImport` | `ImportExportWindow` | ➕✅ | |
| `FrmExport` | `ImportExportWindow` | ➕✅ | |
| `FrmGenerateDoc` | `ImportExportWindow` | ➕🟡 | 문서 생성 충실도 ❓ |
| `FrmGenerateHtml` | `ImportExportWindow` | ➕🟡 | HTML 생성 충실도 ❓ |
| `FrmImportFolder` | `ImportExportWindow` | 🟡❓ | 폴더 임포트 통합 추정 |
| `FrmImportAccessHelper` | — | 🔴❓ | Access 임포트 보조 대응 없음 |

### E. 렌더링 / Office / 미디어 (도메인문서 03)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmMediaPlayerControl` | `Media/MediaPlaybackService`+`ViewModel` | 🟡 | **서비스/VM 존재하나 어떤 창에도 미바인딩(orphaned)** |
| `FrmLaunchMediaPlayer` | `Media/*` | 🟡 | 동상 — 재생 UI 미연결 |
| `FrmBackground` | — | 🔴 | **배경 설정 대응 없음(확인됨)** |

### F. 설정 / 데이터 / 자산 (도메인문서 04)

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmOptions` | `SettingsWindow`(분해) | ✅ | ADR-0005 단일모달→Settings 페이지, WPF 31참조 |
| `FrmGetWorkingFolder` | `WelcomeWindow` | 🟡❓ | 초기 폴더 설정 통합 추정 |

### G. 시작 / 정보 / 기타

| 레거시 폼 | WPF 대응 | 상태 | 비고 |
|---|---|---|---|
| `FrmAbout` | `AboutWindow` | ✅ | |
| `FrmHelp` | `HelpWindow` | ✅ | |
| `FrmRegister` | `RegistrationWindow` | ✅ | |
| `FrmSplashScreen` | `WelcomeWindow` | 🟡❓ | 스플래시 vs 온보딩(개념 상이) |
| `FrmSplashScreenOld` | — | 🔴 | 구 스플래시(레거시 잔재, 포팅 불요 가능) |

### 집계 (추정 포함)

| 상태 | 개수(대략) |
|---|---|
| ✅ 포팅/➕통합 | 14 |
| 🟡 부분 | 12 |
| 🔴 미포팅 | 9 |
| (포팅 불요 후보) | `FrmSplashScreenOld` 등 |

## 3. 마일스톤 평가 (기능 기준)

| 마일스톤 | 정의 | 평가 |
|---|---|---|
| **M1** | 신규 메인 폼으로 일반 송출 | **기능상 도달** — MainWindow + OutputWindow + LIVE/Black/모니터선택/예배순서/Preview |
| **M2** | 모든 T0/T1 신규(운영자 베타) | **부분** — 핵심 창은 있으나 렌더 충실도·미디어 UI 미완, T0/T1 전수 미검증 |
| **M3** | 모든 폼 신규(일반 베타) | **미달** — 미포팅 9 + 부분 12 |
| **M4** | v3.0 정식 출시 | **미달** — WPF 미전환(레거시 주력) |

## 4. 갭 분류 (성격별)

### G-α. 렌더링 충실도 갭 (라이브 핵심 · 최우선)
- **PPT 썸네일/슬라이드 렌더**: `MainWindow` PowerPoint 탭이 `"Decks: {N} / Limit: {M}"` 텍스트 placeholder([MainWindow.xaml:220-236](../../Easislides.Wpf/MainWindow.xaml#L220)). 실제 썸네일 스트립·슬라이드 출력 미구현.
- **미디어 재생 UI**: `MediaPlaybackService`/`MediaPlaybackViewModel` 는 있으나 **어떤 창에도 바인딩 안 됨**(App.xaml.cs DI 등록만). Media 탭은 디렉터리/카메라 텍스트뿐.
- **출력 렌더 패리티**: `OutputWindow` 의 가사/성경/배경 실제 렌더가 레거시(`gfDisplay`/`gfLyrics` GDI+ 경로)와 동등한지 미검증.

### G-β. 미포팅 폼 갭 (기능 부재)
- 🔴 확정: `FrmBackground`(배경 설정), `FrmEditNotes`(노트/코드 편집), `FrmManageItemLists`(예배 리스트 관리), `FrmPopupText`(팝업 송출).
- 🔴 추정(검증 필요): `FrmSingleMonitorAlert`, `FrmBibleRename`, `FrmUpdateFileName`, `FrmImportAccessHelper`.

### G-γ. 횡단/인프라 갭
- **스크린샷 회귀 자동화 부재**(§9.1) — 렌더 충실도/리팩토링 안전망의 전제.
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
1. **스크린샷 회귀 PoC**(§9.1, next-session-plan C): 헤드리스 WPF 렌더 가능성 PoC → 기준 이미지 비교 하니스. **이게 G1 이후 모든 렌더 작업의 안전망**.
2. **PPT 썸네일/슬라이드 렌더**: `gf.PreviewPPT.BuildScreenPreDumps`(OfficeLib) 산출을 WPF Preview/썸네일 스트립에 연결. MainWindow PowerPoint 탭 placeholder 대체.
3. **미디어 재생 UI 연결**: orphaned `MediaPlaybackViewModel` 을 MainWindow Media 탭/전용 컨트롤에 바인딩.
4. **출력 렌더 패리티 검증**: OutputWindow 가사/성경/배경 렌더를 레거시와 스크린샷 비교로 고정.
- 게이트: 각 항목 스크린샷 회귀 통과 + `--legacy-ui` 롤백 유지.

### Phase G2 — 미포팅 폼 (중위험, 1건씩)
- 우선순위: 라이브 사용 빈도 高 → 低.
  1. `FrmBackground`(배경 설정 — 송출 직접 영향) → Settings 또는 전용 창.
  2. `FrmEditNotes`(노트/코드 편집) → SongEditor 통합 또는 전용.
  3. `FrmManageItemLists`(예배 리스트 관리).
  4. 경고/알림(`FrmSingleMonitorAlert`, `FrmPopupText`) → 통합 알림 체계.
  5. 유틸(`FrmBibleRename`, `FrmUpdateFileName`, `FrmImportAccessHelper`) — 일괄.
- 각 폼: ViewModel 단위 테스트 우선 → View 구현 → 동작 동등성 확인.

### Phase G3 — 운영 전환 게이트 (B-4)
- 두 exe + `Core.dll` 동봉 패키징, 1시간 예배 리허설 시나리오 통과를 production 전환 게이트로(ADR-0007 sunset 일정과 연동).

### 권장 착수 순서
```
G0 (즉시·저위험) → G1.1 스크린샷 PoC → G1.2~G1.4 렌더 → G2 (1건씩) → G3 게이트
```
- next-session-plan §2(C 스크린샷 → A 컴포지트 → B 전환)와 정합: **C = G1.1**, **A(컴포지트) = G1 진행 중 병행**, **B = G3**.

## 6. 즉시 액션 (이 PR 범위 외 후속)
- [ ] README stale 갱신(G0-3)
- [ ] ❓ 항목 정밀 검증으로 매트릭스 확정(G0-1)
- [ ] 스크린샷 회귀 PoC 착수 여부 결정(G1.1)

## 7. 참조
- 도메인 계획: [01-shell-live-operations](01-shell-live-operations.md) ~ [06-verification-test-plan](06-verification-test-plan.md)
- [next-session-plan.md](next-session-plan.md) (남은 백로그 A/B/C)
- ADR-0001(WPF), ADR-0005(Options 분해), ADR-0007(안전망), ADR-0008(Core 추출)
- 산출물: `Easislides.exe`(legacy) / `EasislidesNext.exe`(WPF)
