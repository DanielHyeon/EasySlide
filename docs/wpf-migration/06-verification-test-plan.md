# WPF 이식 후 검증 및 테스트 총괄 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: v3.0 UI/UX 목표와 Maturity Level 4
- `docs/adr/README.md`: Accepted ADR 0001~0007 전체
- `docs/ui/icon-migration-map.md`: 아이콘 전환 완료 검증
- `docs/ui/icon-pipeline.md`: 아이콘 추출/변환 검증
- `docs/form-designer-split-plan.md`: legacy WinForms 폼 분리 상태와 build 검증 기준

## 1. 목적

이 문서는 WPF 이식 완료 여부를 판단하기 위한 공통 검증 기준과 테스트 체계를 정의한다. 개별 부문 문서의 테스트를 하나의 release gate로 묶는다.

## 2. 현재 기준선

현재 WPF 기반 검증 결과:

| 항목 | 결과 |
|---|---|
| `dotnet build Easislides.sln -c Debug` | 성공 |
| `dotnet test Easislides.sln -c Debug` | 372개 통과 |
| `dotnet build Easislides.sln -c Release` | 성공 |
| `dotnet test Easislides.sln -c Release --no-build` | 372개 통과 |
| `gstack /qa`, `GSD verify-work` | 현재 작업 환경 PATH에 도구 없음 |
| WPF 프로젝트 | `Easislides.Wpf` |
| WPF 테스트 프로젝트 | `Easislides.Wpf.Tests` |
| 남은 주요 경고 | NetOffice 호환성, DirectShow Windows API, HookManager nullable |

이 기준선은 M0 통과 조건으로 유지한다. 이후 마일스톤마다 테스트 수와 수동 검증 시나리오를 늘린다.

## 3. 완료 여부 판정 체계

| 상태 | 의미 |
|---|---|
| 미착수 | 분석/계약/테스트가 없다 |
| 분석 완료 | legacy 기능 목록과 이식 계약이 문서화되었다 |
| 구현 중 | WPF UI 또는 service가 작성 중이다 |
| 기능 완료 | 대표 happy path가 동작한다 |
| 검증 완료 | 자동 테스트와 수동 검증이 통과했다 |
| 운영 승인 | 실제 운영 데이터와 리허설 시나리오가 통과했다 |

부문별 완료는 "기능 완료"가 아니라 "검증 완료" 이상이어야 완료로 본다.

## 4. 전체 테스트 계층

| 계층 | 목적 | 도구/방식 |
|---|---|---|
| Unit | ViewModel, service, parser, settings, state machine | xUnit, FluentAssertions |
| WPF STA unit | ResourceDictionary, control state, dispatcher 필요 로직 | xUnit collection fixture |
| Integration | DB, filesystem, Office adapter, render cache | fixture 작업 폴더 |
| Visual/manual | 다크/라이트/Senior, 출력 화면, 텍스트 잘림 | 수동 체크리스트, screenshot |
| Regression parity | WinForms와 WPF 결과 비교 | 동일 작업 폴더, 동일 명령 순서 |
| Stress | COM, PPT, media, 장시간 운영 | 반복 실행, process/memory 확인 |
| Rehearsal | 실제 예배 운영 흐름 | 60분 리허설 |

## 4.1 ADR 검증 항목

| ADR | 테스트/검증 방법 |
|---|---|
| ADR-0001 | WPF 프로젝트 build, WPF UI resource 로드, EasiDS token 사용 검사 |
| ADR-0002 | `docs/ui/icon-migration-map.md` 행별 완료 체크, 새 WPF UI의 raster 참조 검사 |
| ADR-0003 | Pretendard resource 포함 여부, 한글 긴 문장 렌더링 확인 |
| ADR-0004 | 로컬/글로벌 shortcut 자동 테스트, 실제 리모컨 수동 테스트 |
| ADR-0005 | Settings section map과 `FrmOptions` 기능 커버리지 비교 |
| ADR-0006 | Standard/Large/Senior mode resource scale 테스트와 screenshot 검수 |
| ADR-0007 | `--legacy-ui` 또는 rollback 실행 경로 smoke test |

## 5. 마일스톤별 테스트 게이트

### M0 기반 안정화

명령:

```powershell
dotnet build Easislides.sln -c Debug
dotnet test Easislides.sln -c Debug --no-build
```

통과 기준:

- 오류 0개.
- WPF 테스트 219개 이상 통과.
- DemoWindow, ControlsGallery, IconGallery, LiveBarDemo 실행 가능.

### M1 운영 셸

추가 테스트:

- `MainViewModelTests`
- `LiveSessionServiceTests`
- `OutputWindowServiceTests`
- `OutputWindowHostTests`
- `OutputWindowViewModelTests`
- `SafetyConfirm` 실제 command 연결 테스트
- `MediaPlaybackServiceTests`
- `MediaPlaybackViewModelTests`

현재 상태: `MainViewModelTests`, `MainWindowCopyTests`, `LiveSessionServiceTests`, `OutputWindowServiceTests`, `OutputWindowHostTests`, `OutputWindowViewModelTests`, `MediaPlaybackServiceTests`, `MediaPlaybackViewModelTests` 추가 완료. `BlackScreenCommand`, `HideOutputCommand`, `GoLiveCommand`, `StopLiveCommand`, 라이브 중 `CloseOutputCommand`는 `ILiveSafetyPrompt`를 통해 SafetyConfirm 연결 경계를 검증한다. 라이브 중 Next/Prev는 리모컨 운영 흐름을 막지 않도록 추가 확인 없이 현재 선택 항목을 송출하며, 저장/이식된 `AdvanceNextItem`은 현재 항목을 송출한 뒤 다음 항목을 운영 selection으로 준비한다. `MainWindowCopyTests`는 운영 셸 제목/출력/설정/예배 순서/상태 패널 한국어 라벨을 회귀 방지하고, `MainViewModelTests`는 샘플 큐와 빈 큐/PowerPoint 제한 상태 메시지의 readable Korean copy를 고정한다. `OutputWindowHost`/`OutputWindowViewModel`은 settings-backed lyrics monitor alert/color/notation visibility를 출력 창에 반영한다. 미디어 컨트롤은 `IMediaPlaybackBackend` fake/NoOp 경계 기준으로 load/play/pause/stop/seek/mute/repeat/volume/balance, 저장/이식된 media audio default와 runtime 변경 반영, backend 명령 위임, 모든 주요 backend 명령 실패 상태 전환, ViewModel command를 자동 검증한다.
2026-05-29 기준 Release 산출물 `Easislides.Wpf\bin\Release\net10.0-windows\Easislides.Wpf.exe`, `MainWindow.baml`, `OutputWindow.baml` 생성 확인 완료.

수동 게이트:

- 작업 폴더 열기
- 항목 선택
- 프리뷰 갱신
- 출력 창 열기
- Go Live
- Next/Prev
- Black/Hide

### M2 콘텐츠

추가 테스트:

- repository read/write
- item editor validation
- bible parser/search
- import/export fixture
- search hit count

현재 상태: `LibraryViewModelTests`, `FolderEditorViewModelTests`, `SongEditorViewModelTests`, `SongCopyViewModelTests`, `SongMoveViewModelTests`, `SongDeleteViewModelTests`, `SongRecoveryViewModelTests`, `SongMergeViewModelTests`, `SongMergeServiceTests`, `ExternalFileOperationServiceTests`, `ExternalFileOperationViewModelTests`, `ImportExportServiceTests`, `ImportExportViewModelTests`, `SearchUsageServiceTests`, `SearchUsageViewModelTests`, `BibleRepositoryTests`, `BibleViewModelTests`, `AdminDatabaseRepositoryTests` 추가 완료.

`LibraryViewModel`은 설정의 명시 AdminDB 경로 또는 기존 작업 폴더 파생 경로를 해석하고, `IAdminDatabaseRepository`를 통해 폴더 목록/선택 폴더 곡 목록을 로드하며, 제목/대체 제목/분류/key/가사 검색, AdminDB 경로 누락 상태 메시지, 폴더 삭제/복구 command와 reload/선택 복원, 폴더/곡 순서 변경 요청 매핑, drag/drop용 target index reorder와 reload/선택 복원을 자동 검증한다. `FolderEditorViewModel`은 기존 폴더 로드/dirty state, 새 폴더 번호 산정, 이름 validation, `SongFolderWriteModel` 매핑, configured/default backup root를 자동 검증한다. `SongEditorViewModel`은 기존 곡 필드 로드/dirty state, `IAdminSongDetailRepository` 기반 legacy metadata 보존, preview title/lyrics/metadata/색상/font/format 상태, 라이브 중 저장 SafetyConfirm 승인/취소 경계, 제목 validation, `SongWriteModel` 매핑, configured/default backup root, 신규 곡 저장 후 id 반영을 자동 검증한다.

`SongCopyViewModel`, `SongMoveViewModel`, `SongDeleteViewModel`, `SongRecoveryViewModel`은 복사/이동/삭제/복구 요청 매핑, configured/default backup root, 대상/선택 validation을 검증한다. `SongMergeViewModel`과 `SongMergeService`는 Source A/B 폴더와 기본/대체 제목 매칭, 후보 선택 validation, legacy `FrmSmartMerge` metadata fallback, `[region 2]` 기반 가사 병합, notation remap, 저장 후 reload 신호를 자동 검증한다. `ExternalFileOperationService`와 `ExternalFileOperationViewModel`은 legacy 외부 폴더 목록, InfoScreen/PowerPoint 외부 폴더 copy/move, ` - Copy (n)` 충돌 파일명, InfoScreen `.esi` 곡 폴더 import, source/destination validation과 완료 이벤트를 자동 검증한다. `ImportExportService`와 `ImportExportViewModel`은 legacy ESN/EST header/body/notation/metadata preview/import, XML replace import, ESF SQLite import/export, TXT/DOC/DOCX 폴더 import, Word COM `.doc` 추출 seam, XML/ESN/HTML/RTF export 산출물, praise-book metadata/style/index/page 옵션, 중복 정책, source folder 및 export candidate 선택을 자동 검증한다. `SearchUsageService`와 `SearchUsageViewModel`은 legacy `FrmFind`의 제목/가사/번호/참조/관리자/저작권/작가 검색 필드, 폴더/key/timing/notation/수정일 필터, `FrmLookupTitles` Title2 후보, `FrmUsages`의 `EsUsage.db` 기간/세션별 사용 기록, 발생 횟수 집계, 선택 record 삭제 확인 승인/취소 경계, 삭제 후 usage refresh, RTF 보고서 산출물을 자동 검증한다. `BibleRepository`와 `BibleViewModel`은 legacy Biblefolder/성경 DB 로드, 책 목록, 본문 로드, 전체/일부/구문 검색, 선택 구절 ID/title 생성, Region 1/2 버전 변경, dual-region preview 갱신과 완료 이벤트를 자동 검증한다. `MainViewModelTests`는 Bible selection을 현재 예배 순서 다음에 삽입하고 선택 상태/상태 메시지를 갱신하는 경계를 고정한다. `AdminDatabaseRepositoryTests`는 song detail 조회, 삭제 곡 조회, folder soft delete/recover `Use` 토글과 곡 `FOLDERNO` 보존, 일반 이동의 `LastModified` 보존, song soft delete/recover backup/transaction, folder reorder의 `FOLDER`/`SONG.FOLDERNO` staging 갱신, song reorder의 `SONG_NUMBER` 재시퀀싱과 rollback을 검증한다.

`LibraryWindow`는 MainWindow 라이브러리 버튼으로 진입해 browse/search, 폴더 추가/편집 저장 후 reload, 폴더 위아래 및 직접 drag gesture 순서 변경, 새 곡/편집 저장 후 reload, 선택 곡 복사/이동/삭제 후 reload, 곡 위아래 및 직접 drag gesture 순서 변경, 삭제 곡 복구 후 대상 폴더 reload와 곡 선택, 스마트 병합 후 대상 폴더 reload까지 연결되었다. `ExternalFileOperationWindow`는 MainWindow 외부 파일 버튼에서 진입해 InfoScreen/PowerPoint 파일 추가, 외부 폴더 복사/이동, InfoScreen 곡 폴더 가져오기를 연결한다. `ImportExportWindow`는 MainWindow I/O 버튼에서 진입해 ESN/EST/XML/ESF/Access MDB/TXT/DOC/DOCX import preview/import, Word COM `.doc` 추출 경계, MDB table/column helper mapping, 중복 정책, praise-book 번호/메타데이터/chord/index/page 옵션, XML/ESN/ESF/HTML/RTF export를 연결한다. `SearchUsageWindow`는 MainWindow Search 버튼에서 진입해 곡 상세 검색, Title2 후보 조회, usage 기간/세션 조회, 집계/삭제/RTF 보고서 생성을 연결한다. `BibleWindow`는 MainWindow 성경 버튼에서 진입해 성경 버전/책 로드, 본문/검색, 선택 구절 ID/title 생성, Region 1/2 preview, 현재 예배 순서 다음 삽입을 연결한다.

수동 게이트:

- 예배 목록 편집
- 항목 복사/이동/삭제/복구
- 성경 아이템 생성
- 폴더 가져오기
- DOC/HTML 내보내기

### M3 렌더링/미디어

추가 테스트:

- PPT 10개 fixture metadata 비교
- PPT 100회 stress
- thumbnail cache key/invalidation/LRU tests
- PowerPoint render service cache/timeout/error classification tests
- image asset metadata/layout/error classification tests
- image resize/fill/fit tests
- PreviewCanvas WPF placement/render tests
- transition effect list/action/frame contract tests
- output renderer scene snapshot tests
- media playback state tests
- media playback backend delegation/failure tests

현재 상태: `PowerPointRenderServiceTests`, `ThumbnailCacheTests`, `ImageAssetServiceTests`, `PreviewCanvasTests`, `TransitionEffectServiceTests`, `OutputRendererTests`, `OutputWindowViewModelTests`, `MediaPlaybackServiceTests`, `MediaPlaybackViewModelTests` 추가 완료. PPT는 `IPowerPointRenderService`/`PowerPointRenderService`로 파일 검증, 요청 timeout 및 설정 기반 `PowerPointRenderTimeoutSeconds`, 설정 기반 `ThumbnailCacheMegabytes`, `IThumbnailCache` 기반 cache invalidation/runtime 재구성, 오류 분류 계약을 고정했고 `OfficePowerPointRenderBackend`가 `OfficePptSession.ExportSlideAsync`를 통해 STA 경계 안에서 JPG export를 수행한다. 썸네일 캐시는 파일 경로/stamp/길이/크기/variant 기반 key, source invalidation, LRU entry/byte eviction, 통계 snapshot, PowerPoint render service 공유/설정 기반 캐시 주입을 고정했다. 이미지는 `IImageAssetService`/`ImageAssetService`로 메타데이터 로드, legacy `ImageCanvas.ResizeCanvas`와 같은 fit 배치, fill/stretch/center 배치, unsupported/locked/decode 오류 분류를 고정했고 `PreviewCanvas`가 bitmap pixel dimension 기준으로 같은 배치 계약과 WPF pixel render를 수행함을 검증했다. 전환 효과는 `ITransitionEffectService`/`TransitionEffectService`로 legacy 58개 전환 목록, `AsFade` action, background layer, progress/frame 계약을 고정했다. 출력 renderer는 `IOutputRenderer`/`OutputRenderer`로 Live/Hidden/Blackout/Ready/Standby scene snapshot, content placement, transition frame, gap item ready scene/fade snapshot, lyrics monitor appearance snapshot, PowerPoint/media panel overlay visibility, `OutputWindowViewModel.Scene` 바인딩 경계를 고정했다. 출력 ViewModel은 GAP(User) 로고 파일 로더 주입, 로고 표시 시 제목 숨김, 로더 실패 시 제목 fallback, 기본 GAP 모드/라이브 상태 미표시, 동일 경로 캐시 경계를 고정했다. 미디어는 `IMediaPlaybackService`, `IMediaPlaybackBackend`, `NoOpMediaPlaybackBackend`, `WpfMediaElementPlaybackBackend`로 상태 machine, settings 기반 volume/balance/mute default 및 변경 이벤트 반영, `MediaDir` 상대 파일 source 해석, `LiveCamNumber` capture source 및 settings change 재로드, backend 명령 위임, 모든 주요 backend 명령 실패 시 `Failed` snapshot 전환, WPF `MediaElement` 파일 adapter 경계와 `MediaPlaybackViewModel` command/표시 계약을 고정했다. 후속으로 실제 PPT fixture 10개/100회 stress, WPF `MediaElement` visual host 또는 DirectShow adapter 기반 통합 테스트, WinForms/WPF 출력 이미지 diff 테스트가 필요하다.

수동 게이트:

- 실제 PPT 송출
- 이미지 배경 송출
- 영상 재생/정지/seek
- 출력 화면에서 결과 확인

### M4 설정/데이터

추가 테스트:

- legacy setting migration
- legacy settings map inventory/alias coverage
- legacy registry source coverage
- startup legacy migration coverage
- settings validation
- settings path picker dialog commands
- shortcut editor override/collision/runtime binding
- legacy shortcut migration from `KeyBoardOption`/`GlobalHookKey_*`
- `PowerPointRenderTimeoutSeconds`/`ThumbnailCacheMegabytes` runtime reflection tests
- `App.ConfigureServices` production DI resolution tests
- asset copy hash
- SQLite transaction/rollback

현재 상태: `SettingsServiceTests`, `LegacySettingsMapTests`, `RegistryLegacySettingsSourceTests`, `FileLegacySettingsSourceTests`, `SettingsBootstrapMigrationServiceTests`, `SettingsWindowViewModelTests`, `OnboardingCoordinatorTests`, `SupportInfoServiceTests`, `AssetMigrationServiceTests`, `DatabaseMigrationServiceTests`, `AdminDatabaseRepositoryTests`, `OperationalDataRehearsalServiceTests` 추가 완료. `ISettingsService`/`SettingsService`로 typed setting get/set, validation failure rollback, default restore, JSON import/export, backup, legacy settings source 변환, shortcut override 저장/초기화를 자동 검증한다. `RegistryLegacySettingsSource`는 HKCU legacy registry section(`config`/`options`/`monitors`)의 문자열/DWORD 값을 읽고 누락 값 조회 시 registry key를 생성하지 않으며 `MigrateLegacyAsync`에 직접 투입되는 경계를 고정했다. `FileLegacySettingsSource`는 legacy INI/key-value 파일, JSON, .NET `user.config`/`appSettings` XML, `Properties.Settings` 호환 `<setting><value>` 구조를 읽고 missing file을 생성하지 않으며, `CompositeLegacySettingsSource`는 registry 값을 우선하고 file source로 fallback하는 production migration 경계를 고정했다. `SettingsBootstrapMigrationService`는 production 시작 시 settings.json이 없을 때 legacy migration을 1회 실행하고, 기존 settings.json이 있으면 사용자 값을 보존하며, migration 실패 시 새 settings.json을 만들지 않는 경계를 고정했다. `LegacySettingsMap`과 `docs/wpf-migration/inventory/settings-map.md`로 현행 typed key 전체 inventory, `FrmOptions.SaveVariables` 고위험 key 문서화, `root_directory`/`RegistrationUser`/`OutputMonitorName`/`LiveCamVolume`/`LiveCamBalance`/`LiveCamMute`/`DBFileName` alias migration, bool 호환 parsing, media scale 정규화, PowerPoint/media/display/alert/gap/lyrics monitor operational setting migration, legacy `KeyBoardOption`/`GlobalHookKey_*` shortcut override migration과 WPF `OnboardingCompleted` 완료 플래그를 고정했다. `SettingsWindow`/`SettingsWindowViewModel`로 ADR-0005 9개 섹션, typed setting 즉시 저장, 다크/라이트 및 Standard/Large/Senior 적용, invalid setting rollback, default restore, import/export, AdminDB 분석 결과 표시, MainWindow 설정 진입 버튼, 작업 폴더/AdminDB/백업 루트/설정 transfer path picker command, shortcut editor 목록/저장/복원/충돌 검증, PowerPoint/media/display/alert/gap/lyrics monitor operational setting 로드/저장/rollback 및 XAML 노출, 운영 데이터 리허설 실행/요약/검증 메시지/DB table inventory 표시를 구현했다. `WelcomeWindow`/`OnboardingCoordinator`는 첫 실행 InterfaceSize 선택, 선택값 저장/테마 적용/완료 플래그 저장/취소 시 미완료 유지 경계를 고정했다. `SupportInfoService`와 `AboutWindow`/`HelpWindow`/`RegistrationWindow`는 legacy About/Register/Help copy, `RegistrationUser` 표시/저장, 웹사이트/System Info/등록 페이지 launcher, `KeyBoardOption=1` 이식 후 도움말 단축키 전환을 고정했다. `ShortcutSettings`와 `MainViewModel.BindShortcuts`는 저장된 override를 runtime `ShortcutRegistry` 등록에 반영하고, `UsePowerpointTab`/`UseMediaTab`/`PP_MaxFiles`는 `MainViewModel` module tab visibility와 PowerPoint queue 제한에 반영하며, `AdvanceNextItem`은 `MainViewModel` 송출 후 selection advance에 반영한다. `NoPowerpointPanelOverlay`/`NoMediaPanelOverlay`는 `LiveOutputRenderSettings`, `OutputRenderer`, `OutputWindowViewModel` panel overlay visibility에 반영하고, `ShowLyricsMonitorAlertBox`/`GapItemOption`/`GapItemLogoFile`/`GapItemUseFade`/`LMTextColour`/`LMBackColour`/`LMShowNotations`는 `LiveOutputRenderSettings`, `OutputRenderer`, `OutputWindowViewModel` 출력 경계에 반영한다. 특히 `GapItemOption=User`와 `GapItemLogoFile`은 로고 이미지 source/visibility, 제목 숨김, 로더 실패 시 텍스트 fallback, 동일 경로 캐시까지 반영한다. `MediaVolume`/`MediaBalance`/`MediaMuted`/`MediaDir`/`LiveCamNumber`는 `MediaPlaybackService` load/default, media directory, live camera source 및 settings change 경계에 반영한다. `IAssetMigrationService`/`AssetMigrationService`로 사용자 자산 migration dry-run, 파일 sha256 report, 원본 무수정 복사, 복사 후 hash 검증, backup report 작성, 목적지 파일 충돌 시 safe-name 복사, source missing/source-not-directory 오류 분류를 자동 검증한다. `IDatabaseMigrationService`/`DatabaseMigrationService`로 SQLite `user_version`/table 분석, dry-run, backup, transaction migration, rollback, backup restore, source missing/source-not-file/corrupt DB 오류 분류를 자동 검증한다. `IAdminDatabaseRepository`/`AdminDatabaseRepository`로 실제 bundled `AdminDB/Database/EasiSlidesDb.db` schema inventory, `FOLDER`/`SONG` 필수 table/column 호환성 진단, read-only folder song count, folder별 song summary 조회, deleted song summary 조회, folder upsert backup, legacy SONG 필드 insert/update, update 시 `OldFolder` 보존, 일반 song move의 `LastModified` 보존, soft delete/recover backup/transaction, folder reorder의 `FOLDER`/`SONG.FOLDERNO` staging 갱신, song reorder의 `SONG_NUMBER` 재시퀀싱과 rollback, song 이동 실패 시 transaction rollback 및 backup restore, 운영 DI 등록을 자동 검증한다. `IOperationalDataRehearsalService`/`OperationalDataRehearsalService`로 설정 기반 작업 폴더/백업 루트/AdminDB 경로 해석, destination/backup directory 미생성 dry-run, 자산 scan과 DB inventory 통합, 작업 폴더 누락 error, AdminDB 누락 warning을 자동 검증한다. DI 테스트 중 드러난 `ThemeService`의 `Application.Current` 미초기화 환경 NRE도 상태 갱신/이벤트 유지 방식으로 수정했다. 남은 범위는 실제 `%AppData%` 운영 데이터로 수행하는 수동 60분 리허설과 데이터 마이그레이션 온보딩 UI 연결이다.

수동 게이트:

- WinForms 설정을 WPF로 migration
- 작업 폴더 변경
- 사용자 자산 보존 확인
- 앱 재시작 후 설정 유지

2026-05-29 추가 검증: `IAdminDatabaseRepository.SoftDeleteFoldersAsync`/`RecoverFoldersAsync`는 `FOLDER.Use`만 전환하고 곡 `FOLDERNO`를 보존하며 backup/transaction/restore 경계를 검증했다. `LibraryViewModel.DeleteSelectedFolderCommand`/`RecoverSelectedFolderCommand`는 폴더 상태 전환 후 reload와 선택 복원을 검증했다.

2026-05-29 추가 검증: `SongEditorViewModel.LoadAsync`는 기존 곡 상세 필드를 로드해 writer/copyright/capo/timing/licence/book/user/sequence/notation/settings/format data를 저장 시 보존한다. Preview는 lyrics-monitor 색상, preview font/size, 첫 screen lyrics, metadata, format/notation/sequence 상태를 갱신하고, 라이브 송출 중 저장은 `ILiveSafetyPrompt` 확인 취소 시 repository write를 수행하지 않는다.

2026-05-29 추가 검증: `SearchUsageViewModel.DeleteSelectedUsageAsync`는 `IUsageDeleteConfirmation` 확인 요청을 만들고 취소 시 repository delete를 수행하지 않는다. 승인 시 선택 usage record 삭제 후 `RefreshUsageAsync`를 다시 호출해 목록과 집계를 최신 상태로 갱신하며, `WpfUsageDeleteConfirmation`은 `SafetyConfirm` 기반 WPF 확인 UX로 운영 DI에 등록되었다.

### M5 플랫폼/운영

추가 테스트:

- command catalog/default shortcut metadata
- global shortcut adapter
- display coordinate conversion
- window placement policy
- hook lifecycle

현재 상태: `DisplayServiceTests`, `GlobalInputServiceTests`, `CommandCatalogTests`, `WindowPlacementServiceTests`, `PlatformDiagnosticsServiceTests`, `MainViewModelTests.OpenOutputCommand_UsesPreferredDisplayFromDisplayService`, `MainViewModelTests.OpenOutputCommand_UsesDefaultOutputMonitorFromSettings`, `MainViewModelTests.OpenOutputCommand_WhenAlwaysUseSecondaryMonitorDisabledWithoutDefault_SelectsPrimary`, `MainViewModelTests.OpenOutputCommand_WhenDefaultMonitorMissingAndAlwaysUseSecondaryDisabled_FallsBackToPrimary` 추가 완료. 모니터 열거 fallback, primary/secondary 선택 정책, 선택된 출력 모니터와 저장/이식된 기본 출력 모니터 및 보조 모니터 우선 정책으로 WPF 출력 창을 여는 경계, 저장된 모니터가 제거된 경우의 primary fallback, HookManager adapter 시작/중지/실패 cleanup, global scope 단축키 라우팅, command id 중복/shortcut 충돌 방지, Live 위험 명령 메타데이터, fullscreen/windowed 출력 창 배치 정책, settings 기반 legacy custom bounds, platform diagnostics snapshot과 경고 수집을 자동 검증한다. `App.xaml.cs`는 운영 MainWindow 시작 시 `IGlobalInputService.Start()`를 호출하고 앱 종료 시 DI provider dispose로 hook 구독을 해제한다.

수동 게이트:

- 듀얼 모니터 출력
- DPI 배율 혼합
- 리모컨 Next/Prev
- 다른 앱 포커스 상태에서 global shortcut
- 앱 종료 후 hook/process 잔존 0건

### M6 전환 승인

통과 기준:

- Debug/Release build/test 통과.
- 실제 운영 작업 폴더 3개 이상에서 migration 성공.
- WinForms/WPF parity checklist 100% 완료.
- 60분 리허설 중 blocker 0건.
- rollback 절차 검증.

## 6. 기능 동등성 체크리스트

| 부문 | 동등성 기준 |
|---|---|
| 운영 셸 | 같은 예배 목록에서 같은 순서로 송출 가능 |
| 라이브 상태 | Active/Standby/Hidden/Off 상태가 출력과 UI에 일치 |
| 콘텐츠 | CRUD, 검색, 가져오기/내보내기 결과 일치 |
| 렌더링 | PPT/이미지 썸네일 수와 비율 일치 |
| 미디어 | 재생 상태와 시간 표시 일치 |
| 설정 | 기존 설정 migration 후 같은 동작 |
| 단축키 | 기존 주요 키와 리모컨 흐름 유지 |
| 멀티모니터 | 선택한 출력 화면 위치와 크기 일치 |

## 7. 수동 리허설 시나리오

준비:

- 실제 운영 작업 폴더 복사본
- PPT 3개 이상
- 이미지 배경 3개 이상
- 영상/음원 각 2개 이상
- 듀얼 모니터 또는 모니터 emulator
- 리모컨/키보드

시나리오:

1. 앱 실행
2. 작업 폴더 migration
3. 테마를 다크로 전환
4. Senior 모드 켜기/끄기
5. 예배 목록 열기
6. 항목 검색
7. 찬양 가사 편집
8. 성경 항목 추가
9. PPT 열기 및 썸네일 생성
10. 출력 화면 열기
11. Go Live
12. Next/Prev 30회
13. Black/Hide 각각 3회
14. 영상 재생/정지
15. 라이브 중 위험 액션 취소/확인
16. DOC/HTML 내보내기
17. 설정 변경 후 앱 재시작
18. 종료 후 process/hook/file lock 확인

기록 항목:

- 실패 명령
- 예상과 다른 출력
- UI freeze 시간
- 텍스트 잘림/겹침
- 메모리 증가
- PowerPoint/DirectShow 잔존 프로세스
- 파일 잠금

## 8. 자동 테스트 확장 계획

새 테스트 프로젝트/폴더:

| 위치 | 목적 |
|---|---|
| `Easislides.Wpf.Tests/Shell` | MainViewModel, LiveSession |
| `Easislides.Wpf.Tests/Library` | 목록/검색/편집 |
| `Easislides.Wpf.Tests/Rendering` | PPT/image/media service |
| `Easislides.Wpf.Tests/Settings` | 설정/migration |
| `Easislides.Wpf.Tests/Platform` | shortcut/display/window placement |
| `Easislides.Wpf.Tests/Fixtures` | 샘플 작업 폴더와 작은 테스트 자산 |

테스트 작성 규칙:

- ViewModel은 UI 없이 테스트 가능해야 한다.
- WPF resource/control은 STA fixture를 사용한다.
- Office/DirectShow 테스트는 category를 붙여 일반 unit test와 분리한다.
- 실제 파일을 바꾸는 테스트는 임시 디렉터리 복사본에서만 실행한다.
- 네트워크/UNC 경로 테스트는 환경 변수로 opt-in한다.

## 9. Release 전 최종 명령

```powershell
dotnet restore Easislides.sln
dotnet build Easislides.sln -c Release
dotnet test Easislides.sln -c Release --no-build
dotnet run --project Easislides.Wpf\Easislides.Wpf.csproj -c Release
```

추가 수동 확인:

- 출력 화면 실제 모니터 확인.
- PowerPoint 프로세스 잔존 확인.
- 작업 폴더 원본과 migration 복사본 비교.
- rollback으로 WinForms 실행 확인.

## 10. 승인 조건

WPF 이식은 다음 조건을 모두 만족하면 완료로 표시한다.

- 부문별 계획 문서의 완료 여부가 모두 "검증 완료" 이상.
- 자동 테스트가 Release에서 통과.
- 실제 운영 리허설 blocker 0건.
- 데이터/자산 손실 0건.
- 라이브 송출 안전 액션 누락 0건.
- 사용자에게 남길 known issue가 문서화되어 있고 운영을 막지 않는다.
