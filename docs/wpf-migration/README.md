# WPF 이식 계획 문서 인덱스

> 기준일: 2026-05-29
> 참조: `.serena/memories/project_structure.md`, `docs/` 기준 문서, `Easislides.Wpf/README.md`  
> 목적: WinForms 기반 EasiSlides v2.6.4의 모든 운영 기능을 WPF/Fluent 2 기반 v3.0으로 이식하기 위한 부문별 실행 계획, 완료 기준, 검증 및 테스트 방안을 정의한다.

## 0. 기준 문서

이 문서 세트는 `cf9ceed` 커밋(`docs: UI/UX modernization plan + ADR-001~007`)에서 추가된 `docs/` 아래 산출물을 기준 결정으로 삼는다. 이후 계획 변경이 필요하면 기존 ADR을 임의 수정하지 않고 새 ADR 또는 별도 변경 이력을 추가한다.

| 기준 문서 | 반영 위치 | 핵심 반영 내용 |
|---|---|---|
| `docs/ui-ux-modernization-plan.md` | 전체 | WPF/WinUI 전환, Fluent 2, 전체 폼 동시 범위, 다크/라이트/Senior/키보드 우선 |
| `docs/adr/0001-wpf-ui-framework.md` | 전체 | WPF + WPF UI 기반, EasiDS 토큰 오버레이 |
| `docs/adr/0002-fluent-icons.md` | 셸/콘텐츠/설정 | 레거시 raster 아이콘 100% 폐기, Fluent UI System Icons 전환 |
| `docs/adr/0003-pretendard-font-bundling.md` | 전체 UI | Pretendard Variable 번들, 시스템 폰트 의존 제거 |
| `docs/adr/0004-hookmanager-preservation.md` | 셸/플랫폼 | HookManager 보존, WPF `PreviewKeyDown`과 공존 |
| `docs/adr/0005-options-decomposition.md` | 설정 | `FrmOptions` 단일 모달을 Settings 페이지로 분해 |
| `docs/adr/0006-senior-mode-token-scale.md` | 전체 UI | Standard/Large/Senior 토큰 스케일 적용 |
| `docs/adr/0007-legacy-ui-safety-net.md` | 전체 전환 | M3까지 `--legacy-ui` 안전망 유지 |
| `docs/ui/icon-migration-map.md` | 셸/콘텐츠/설정 | 현행 PNG/BMP → EasiDS 아이콘 키 매핑 |
| `docs/ui/icon-pipeline.md` | UI 자산 | WPF UI SymbolIcon 우선, SVG/XAML fallback |
| `docs/form-designer-split-plan.md` | 분석/추적 | WinForms 폼 분해 상태를 WPF 이식 인벤토리의 입력으로 사용 |
| `docs/wpf-migration/frmmain-ux-alignment-next-steps.md` | 운영 셸 UI/UX | FrmMain을 WPF MainWindow의 운영 콘솔 UX 기준으로 고정하고 다음 작업 정의 |

## 1. 현재 완료 여부

| 구분 | 현재 상태 | 완료 판단 |
|---|---|---|
| WPF 프로젝트 골격 | `Easislides.Wpf` 솔루션 포함, DI, App.xaml, 테마 토큰 구성 | 완료 |
| 디자인 시스템 | EasiDS 색/타입/간격/반경/모션 토큰, Pretendard 번들, 기본 컨트롤 일부 | 1차 완료, 추가 컨트롤 확장 필요 |
| PoC | HookManager 호환 PoC, Office COM STA stress PoC 화면 존재 | 구현 완료, 실제 운영 데이터 검증 필요 |
| 디자인 시스템 베이스 컨트롤 | §5.1 11종 완성 (`EsButton`~`EsLiveIndicator` + `EsDataGrid`) | 완료 |
| 테스트 인프라 | `Easislides.Wpf.Tests` 398 + `Easislides.Analyzers.Tests` 20 = 418개 테스트 | 완료 |
| 거버넌스 자동화 | `Easislides.Analyzers` EasiDS001(매직 색·폰트) + EasiDS003(매직 간격) + 20개 분석기 테스트, WPF에 Analyzer 참조 | 1차 완료 |
| 접근성(a11y) | `XamlAccessibilityScanner` 가드 + 프로덕션 XAML 인터랙티브 컨트롤 이름 부여(위반 0). 2차 정밀화: 입력 필드 `LabeledBy` 라벨 연결(PR #16), `LiveRegion`(LiveSetting=Assertive) 동적 알림 — LiveBar·EsToast(PR #15), 템플릿 파트 노출 정책 정형화(PR #17). a11y 가드 +11 테스트(세 PR 병합 시) | 1·2차 완료(3 PR 리뷰 대기). 잔여: NVDA/Narrator 수동 검증 |
| 실제 기능 이식 | 운영 셸, 출력, 렌더링/미디어, 플랫폼 일부와 SettingsWindow/SettingsService/LegacySettingsMap/RegistryLegacySettingsSource/FileLegacySettingsSource/CompositeLegacySettingsSource/SettingsBootstrapMigrationService/최초 실행 온보딩/도움말·정보·등록 창/LibraryWindow/LibraryViewModel AdminDB browse/search/folder soft delete recover/folder·song reorder 및 drag reorder/FolderEditorWindow folder add edit/SongEditorWindow song edit legacy detail 보존/preview/live safety 저장/SongCopyWindow song copy/SongMoveWindow song move/SongDeleteWindow song soft delete/SongRecoveryWindow song recover/SongMergeWindow smart merge/ExternalFileOperationWindow external file copy move 및 InfoScreen import/ImportExportWindow text XML ESF Access MDB helper document-folder import(.txt/.doc/.docx, Word COM `.doc` 추출 포함) 및 praise-book 옵션형 XML ESN ESF HTML RTF export/SearchUsageWindow song search title lookup usage inspector/delete confirmation/BibleWindow bible browse search selection/dual-region preview/live queue insert/자산/DB 마이그레이션 service/AdminDatabaseRepository read/write/detail/folder and song soft delete/recover/reorder backup transaction/운영 데이터 dry-run 리허설 및 SettingsWindow 데이터 탭 실행/요약 표시, 기본 출력 모니터, display/window placement, GAP(User) 로고 이미지 송출, media audio/directory/live camera, PowerPoint render/cache/module runtime 소비, 운영 셸 한국어 UI copy 회귀 방지가 WPF 경계로 이동 중 | 부분 구현 |
| 운영 앱 전환 | WPF 앱은 데모/기반 단계, production entrypoint 아님 | 미완료 |

최근 검증 결과:

```powershell
dotnet build Easislides.sln -c Debug
dotnet test Easislides.sln -c Debug
```

결과: 빌드 성공, 테스트 418개 통과(WPF 398 + 분석기 20). 단, 기존 NetOffice/DirectShow/HookManager 경고는 남아 있다.

> 다음 세션 실행 계획(남은 고위험·광범위 항목)은 [next-session-plan.md](next-session-plan.md) 참조.

## 2. 구조 기준 분류

`.serena/memories/project_structure.md`의 기존 구조는 다음 책임으로 나뉜다.

| 기존 구조 | WPF 이식 분류 | 계획 문서 |
|---|---|---|
| `Easislides/Easislides/FrmMain*.cs`, `FrmInfoScreen`, `FrmLyricsScreen`, `FrmLaunchShow`, alert/media control forms | 운영 셸 및 라이브 송출 | `01-shell-live-operations.md` |
| `FrmEditItem`, `FrmEditBibleItem`, `FrmImport*`, `FrmExport`, `FrmGenerate*`, `FrmFind`, `FrmManageItemLists`, `gfBible`, `gfLyrics`, `gfFolder` | 콘텐츠 편집/라이브러리 | `02-content-authoring-library.md` |
| `ImageCanvas`, `ImageTransitionControl`, `gfImages`, `gfMedia`, `gfPowerPoint`, `OfficeLib`, `DirectShow` | 렌더링, Office, 미디어 | `03-rendering-office-media.md` |
| `FrmOptions`, `FrmBackground`, `FrmGetWorkingFolder`, `FrmRegister`, `Settings`, `SQLite`, `AdminDB`, `Resources` | 설정, 데이터, 자산 마이그레이션 | `04-settings-data-assets.md` |
| `HookManager`, `KeyboardActionHandler`, `KeyboardMapping`, `Util`, `gfDisplay`, `PopupWindowHelper` | 플랫폼 연동, 입력, 멀티모니터 | `05-platform-interop-input.md` |
| 전체 부문 공통 | 이식 후 검증 및 테스트 전략 | `06-verification-test-plan.md` |

## 2.1 ADR 준수 추적

| ADR | 이식 중 확인할 사항 | 완료 증거 |
|---|---|---|
| ADR-0001 | WPF 화면은 WPF UI와 EasiDS 토큰을 사용한다 | XAML resource reference, 컨트롤 갤러리 |
| ADR-0002 | 새 UI에 레거시 PNG/BMP 상태 아이콘을 쓰지 않는다 | `Icon.*` 키 또는 `SymbolIcon` 사용 목록 |
| ADR-0003 | 모든 주요 화면이 Pretendard 기반 타입 토큰을 사용한다 | `Typography.xaml`, 시각 검증 |
| ADR-0004 | 글로벌 단축키는 HookManager adapter를 통해 유지한다 | PoC-A, shortcut integration tests |
| ADR-0005 | 설정은 sidebar 기반 페이지로 분해한다 | `SettingsWindow` section map |
| ADR-0006 | Senior 모드는 별도 테마가 아니라 token scale로 처리한다 | ThemeService tests, Senior mode screenshot |
| ADR-0007 | M3 전까지 legacy fallback을 제거하지 않는다 | launch option/rollback 문서 |

## 3. 공통 이식 원칙

1. 기능 동등성 우선: WPF 화면은 기존 WinForms 기능의 동작 결과를 먼저 동일하게 만든 뒤 UX를 개선한다.
2. Strangler 방식: legacy 로직을 즉시 삭제하지 않고 서비스/어댑터로 감싼 뒤 화면 단위로 교체한다.
3. 라이브 안전성 우선: 송출 중 상태 변경, 화면 숨김, 검은 화면, 미디어 재생, 슬라이드 전환은 확인/되돌리기/상태 표시가 있어야 한다.
4. 테스트 가능한 경계: `gf*` 전역 함수 호출은 WPF ViewModel에서 직접 호출하지 않고 서비스 인터페이스를 통해 호출한다.
5. STA/COM 격리: PowerPoint/Word/DirectShow 연동은 UI 스레드를 막지 않는 전용 실행 경계와 정리 규칙을 둔다.
6. 디자인 토큰 강제: 색, 폰트, 간격, 반경, 모션은 EasiDS 리소스만 사용한다.
7. 접근성 기준: 키보드 단독 조작, 포커스 표시, 스크린리더 이름, 44px 이상 타깃, 다크/라이트 대비를 완료 기준에 포함한다.

## 4. 전체 마일스톤

| 마일스톤 | 목표 | 완료 조건 |
|---|---|---|
| M0 기반 안정화 | 현재 WPF 기반과 테스트 인프라를 고정 | 빌드/테스트 통과, 컨트롤 갤러리 정상 실행 |
| M1 운영 셸 | 메인 창, 라이브 상태, 출력 화면 골격 이식 | 예배 리스트 선택, 라이브 상태 표시, 출력 화면 열기 가능 |
| M2 콘텐츠 | 찬양/성경/목록/검색/가져오기/내보내기 이식 | 대표 운영 데이터로 CRUD와 검색/정렬 검증 |
| M3 렌더링/미디어 | PPT 썸네일, 이미지 전환, 미디어 재생 이식 | 기존 샘플 PPT/영상/이미지와 결과 동등성 확보 |
| M4 설정/데이터 | 옵션, 작업 폴더, 사용자 자산, DB 설정 이식 | 기존 설정 자동 마이그레이션과 롤백 검증 |
| M5 플랫폼 완성 | HookManager, 멀티모니터, 리모컨, 배포 안정화 | 라이브 리허설 체크리스트 통과 |
| M6 전환 | WPF를 기본 앱으로 전환, WinForms 유지보수 모드 | 운영 리그레션 0건, v2.6.4 롤백 경로 확인 |

## 5. 전역 완료 기준

WPF 이식 완료는 다음을 모두 만족할 때만 선언한다.

- 기존 WinForms의 주요 운영 기능을 WPF에서 수행할 수 있다.
- 1시간 예배 리허설 시나리오가 WPF 앱만으로 통과한다.
- `dotnet build Easislides.sln -c Release`와 `dotnet test Easislides.sln -c Release`가 통과한다.
- PowerPoint/Word/DirectShow 사용 후 좀비 프로세스와 파일 잠금이 남지 않는다.
- 설정/사용자 자산/작업 폴더 마이그레이션 후 원본 데이터가 보존된다.
- 다크/라이트/큰 글자 모드에서 주요 화면의 텍스트 잘림과 겹침이 없다.
- 키보드/리모컨만으로 라이브 운영 핵심 흐름을 수행할 수 있다.
- 장애 발생 시 v2.6.4 WinForms 앱으로 되돌릴 수 있는 배포 절차가 문서화되어 있다.
