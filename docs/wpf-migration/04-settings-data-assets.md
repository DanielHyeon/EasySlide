# 설정, 데이터, 자산 마이그레이션 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: 설정 복잡성, 사용자 자산 보존, legacy 유지보수 정책
- `docs/adr/0002-fluent-icons.md`: 설정/도움말/시스템 아이콘 전환
- `docs/adr/0003-pretendard-font-bundling.md`: UI 폰트 번들
- `docs/adr/0005-options-decomposition.md`: `FrmOptions` 페이지 분해
- `docs/adr/0006-senior-mode-token-scale.md`: Senior mode 설정
- `docs/adr/0007-legacy-ui-safety-net.md`: rollback/fallback 정책
- `docs/ui/icon-migration-map.md`: `Icon.Settings`, `Icon.Shortcuts`, `Icon.Help`, `Icon.Info`

## 1. 범위

이 문서는 환경 설정, 작업 폴더, 데이터베이스, 사용자 자산, 등록/도움말 기능을 WPF로 옮기는 계획이다.

대상 legacy 파일:

| Legacy | 역할 | WPF 목표 |
|---|---|---|
| `FrmOptions.cs` | 거대 옵션 다이얼로그 | `SettingsWindow` + section tabs |
| `FrmBackground.cs` | 배경 설정 | `AppearanceSettingsView` |
| `FrmGetWorkingFolder.cs` | 작업 폴더 선택 | `WorkingFolderSetupView` |
| `FrmAbout.cs`, `FrmHelp.cs`, `FrmRegister.cs` | 정보/도움말/등록 | `AboutView`, `HelpView`, `RegistrationView` |
| `Settings.cs`, `Properties/Settings.Designer.cs` | 앱 설정 | typed settings service |
| `gfConfig.cs`, `gfConstants.cs`, `gfColorsFonts.cs`, `gfUiText.cs` | 설정/상수/색/문구 | `ISettingsService`, EasiDS token |
| `SQLite/SQLiteController.cs`, `Util/DataUtil.cs` | DB 접근 | repository + migration guard |
| `Resources`, `EasislideImages`, `AdminDB` | 리소스/기본 데이터 | asset migration |
| `Util/FileUtil.cs`, `RegUtil.cs`, `OfficeVersion.cs` | 파일/레지스트리/Office 확인 | platform service |

## 2. 목표 설정 구조

`FrmOptions`의 모든 설정을 한 모달에 넣는 구조를 WPF에서는 영역별로 나눈다.

| Settings section | 내용 |
|---|---|
| General | 언어, 시작 동작, 작업 폴더 |
| Appearance | 라이트/다크, Senior 모드, 폰트 크기, 배경 |
| Live Output | 출력 모니터, 가사 화면, Info screen, 안전 확인 |
| PowerPoint | Office 경로, 렌더 옵션, cache |
| Media | 재생 backend, 기본 볼륨, 코덱 안내 |
| Shortcuts | 키보드/리모컨 단축키 |
| Data | SQLite/AdminDB/백업/복구 |
| Import/Export | 기본 경로, DOC/HTML 옵션 |
| Advanced | 로그, diagnostic, legacy compatibility |

## 3. 이식 단계

### 3.0 ADR 준수 체크

- `FrmOptions`의 단일 거대 모달은 Settings sidebar + section content 구조로 분해한다.
- 설정 화면은 다크/라이트/Senior 모드에서 즉시 preview 가능한 구조로 만든다.
- 사용자 자산은 Fluent 아이콘 전환 대상과 사용자 콘텐츠 보존 대상을 분리한다.
- `--legacy-ui` fallback이 필요한 설정은 M3까지 유지한다.

### 3.1 설정 목록화

- `FrmOptions.Designer.cs`의 컨트롤 목록을 섹션별로 분류한다.
- 각 설정의 저장 위치를 찾는다.
- 설정 key, type, default, valid range, legacy value conversion을 표로 정리한다.
- 제거할 설정과 유지할 설정을 구분한다.

산출물:

- `docs/wpf-migration/inventory/settings-map.md`
- `docs/wpf-migration/inventory/legacy-resource-map.md`

### 3.2 Settings service 구현

필수 기능:

- typed get/set
- validation
- 변경 이벤트
- default restore
- import/export
- legacy setting migration
- rollback backup

규칙:

- UI가 `Properties.Settings`나 registry를 직접 읽지 않는다.
- 설정 저장 실패는 사용자에게 알려야 한다.
- 라이브 중 출력 관련 설정 변경은 preview 또는 확인 단계를 둔다.

### 3.3 사용자 자산 마이그레이션

보존 대상:

- 작업 폴더
- 사용자 배경 이미지
- 가져온 찬양/성경/미디어 파일
- DB 파일
- export 기본 경로
- 등록/라이선스 관련 값
- 최근 파일/최근 작업 목록

마이그레이션 정책:

- 원본은 절대 수정하지 않는다.
- `%AppData%\EasislidesNext\Backups\`에 설정 snapshot을 남긴다.
- 복사 후 hash/size를 확인한다.
- 실패 항목은 전체 중단이 아니라 보고서로 남긴다.
- 사용자가 "나중에"를 선택할 수 있다.

### 3.4 데이터베이스 이식

- 현재 SQLite schema와 AdminDB 구조를 문서화한다.
- 읽기 전용 호환 layer를 먼저 구현한다.
- 쓰기 기능은 backup과 transaction을 적용한 뒤 WPF에 연결한다.
- schema 변경이 필요하면 migration version을 둔다.

완료 조건:

- 기존 DB를 WPF에서 열 수 있다.
- WPF에서 저장한 DB를 WinForms v2.6.4가 읽을 수 있는지 호환성 정책을 명확히 한다.
- schema 변경 시 자동 백업과 rollback이 가능하다.

## 4. 완료 여부

| 항목 | 상태 | 비고 |
|---|---|---|
| ThemeService | 1차 완료 | 다크/라이트/Senior 테스트 있음 |
| Pretendard/EasiDS | 1차 완료 | WPF resource 반영 |
| SettingsService | 부분 구현 | `ISettingsService`, `SettingsService`, `EasiSettingsSnapshot`, typed `EasiSettingKeys`, `RegistryLegacySettingsSource`, `FileLegacySettingsSource`, `CompositeLegacySettingsSource`, `SettingsBootstrapMigrationService` 추가. typed get/set, validation, 변경 이벤트, default restore, JSON import/export, 현재 설정 backup, legacy settings source 변환 경계를 자동 검증 완료. `LegacySettingsMap` 별칭 기반으로 `root_directory`/`RootEasiSlidesDir`/`RegistrationUser`/`OutputMonitorName`/`LiveCamVolume`/`LiveCamBalance`/`LiveCamMute`/`DBFileName` 이식, 미디어 0..100 scale 정규화, legacy `KeyBoardOption`/`GlobalHookKey_*` shortcut override 변환, PowerPoint/media/display/alert/gap/lyrics monitor operational setting migration을 구현했다. 실제 HKCU legacy registry section(`config`/`options`/`monitors`) 읽기와 누락 key 생성 방지 검증 완료. WPF production 시작 시 settings.json이 없을 때 registry 우선, 파일 fallback composite migration을 1회 적용하고 기존 settings.json이 있으면 사용자 값을 보존하도록 연결했다. legacy file source는 INI/key-value/XML `.config`/`user.config`/appSettings/JSON 경계와 `Properties.Settings` 호환 XML 읽기를 검증 완료. `general.onboardingCompleted`, `general.registrationUser`와 운영 데이터 dry-run 리허설 서비스 연결 완료. 실제 운영 수동 리허설은 후속 필요 |
| SettingsWindow | 부분 구현 | `SettingsWindow`, `SettingsWindowViewModel` 추가. ADR-0005 기준 9개 섹션(일반/화면/송출/PowerPoint/미디어/단축키/데이터/가져오기·내보내기/고급), typed setting 즉시 저장, 다크/라이트 및 Standard/Large/Senior 적용, default restore, import/export, AdminDB 분석, 검증 메시지 표시, MainWindow 설정 진입 버튼 자동 검증 완료. 작업 폴더/AdminDB/백업 루트/설정 가져오기/내보내기 파일 선택 dialog, shortcut editor 목록/저장/복원/충돌 검증, runtime shortcut override 반영, media volume/balance/mute/directory/live camera runtime 반영, PowerPoint render timeout/thumbnail cache/tab/max file/panel overlay runtime 반영, display/window placement runtime 반영, alert/gap/lyrics monitor output runtime 반영, PowerPoint/media/display/alert/gap/lyrics monitor operational setting 로드/저장/rollback 및 XAML 노출 검증 완료. 데이터 탭에서 운영 데이터 리허설 실행 버튼, 요약 상태, DB table inventory 표시, warning/error 검증 메시지 연결을 완료했다. WPF focus/screenshot 회귀와 데이터 마이그레이션 온보딩 UI는 후속 필요 |
| 최초 실행 온보딩 | 1차 구현 완료 | `WelcomeWindow`, `IOnboardingCoordinator`, `IOnboardingDialogService` 추가. 첫 실행 시 Standard/Large/Senior 크기 선택 화면을 표시하고, 선택한 `InterfaceSize`를 저장/적용한 뒤 `general.onboardingCompleted`로 재표시를 차단한다. 취소 시 완료 플래그를 저장하지 않는 경계를 자동 검증했다. 실제 설치/업그레이드 패키지 첫 실행 수동 확인은 후속 필요 |
| legacy settings map | 부분 구현 | `docs/wpf-migration/inventory/settings-map.md`와 `LegacySettingsMap` 추가. 현행 `EasiSettingKeys` 전체 inventory, `FrmOptions.SaveVariables` 고위험 key 문서화, `RegistrationUser` 포함 핵심 legacy alias 자동 이식, bool 호환 parsing, media scale 정규화, legacy shortcut 자동 이식, PowerPoint/media/display/alert/gap/lyrics monitor 세부 typed key 자동 이식 검증 완료. media audio/default output monitor/PowerPoint render timeout/thumbnail cache/display window placement/alert/gap/lyrics monitor/PowerPoint tab/max file/media tab/media directory/live camera/panel overlay runtime 소비처 연결 완료 |
| 자산 마이그레이션 | 부분 구현 | `IAssetMigrationService`, `AssetMigrationService` 추가. dry-run, 파일 hash 계산, 복사 후 hash 검증, backup report 작성, 목적지 충돌 시 기존 파일 보존 및 safe name 복사, source missing/not-directory 오류 분류 자동 검증 완료. `IOperationalDataRehearsalService`가 설정의 `WorkingFolder` 또는 `%AppData%\Easislides` 기본 경로를 자산 dry-run scan에 연결하고, 목적지/백업 폴더를 만들지 않는 리허설 경계를 검증했다. SettingsWindow 데이터 탭에서 dry-run 리허설 실행과 결과 요약 표시를 연결했다. 데이터 마이그레이션 온보딩 UI와 실제 운영 수동 리허설은 후속 필요 |
| DB 마이그레이션 | 부분 구현 | `IDatabaseMigrationService`, `DatabaseMigrationService`, `IAdminDatabaseRepository`, `AdminDatabaseRepository` 추가. SQLite `PRAGMA user_version` 분석, 사용자 테이블 inventory, dry-run migration path, backup, transaction, rollback, 실패 시 backup restore, source missing/source-not-file/corrupt DB 오류 분류 자동 검증 완료. 실제 bundled `AdminDB/Database/EasiSlidesDb.db` schema inventory, `FOLDER`/`SONG` 필수 table/column 호환성 진단, read-only song folder/song summary repository, folder/song write repository backup/transaction/rollback, 운영 DI 등록 검증 완료. `IOperationalDataRehearsalService`가 `WorkingFolder\Admin\Database\EasiSlidesDb.db` 파생 경로와 명시 AdminDB 경로의 schema inventory를 dry-run 리허설에 통합했다. 실제 운영 DB 복사본 수동 리허설은 후속 필요 |
| 운영 데이터 리허설 | 1차 구현 완료 | `IOperationalDataRehearsalService`, `OperationalDataRehearsalService`, `OperationalDataRehearsalReport` 추가. 설정 snapshot에서 작업 폴더/백업 루트/AdminDB 경로를 해석하고, 누락 작업 폴더는 error로 중단, 누락 AdminDB는 warning으로 계속 진행, 자산 dry-run과 AdminDB schema inventory를 통합한다. dry-run 중 destination/backup directory를 생성하지 않으며 운영 DI 등록을 검증했다. SettingsWindow 데이터 탭에서 현재 경로 값을 리허설 요청으로 전달하고 파일/테이블/error/warning 요약을 표시한다. 실제 `%AppData%` 운영 데이터와 60분 리허설은 후속 필요 |
| 도움말/정보/등록 | 1차 구현 완료 | `ISupportInfoService`, `SupportInfoService`, `ISupportLauncher`, `AboutWindow`, `HelpWindow`, `RegistrationWindow` 추가. About 창은 legacy `RegistrationUser` 표시/저장, version/copyright/EULA/웹사이트/System Info 실행 경계를 제공한다. Help 창은 legacy 기본 단축키와 `KeyBoardOption=1` 이식 후 arrow navigation 도움말을 표시한다. Registration 창은 무료/자발 등록 안내와 등록 페이지 실행을 제공하며 MainWindow에서 도움말/등록/정보 진입 버튼을 연결했다. 운영 DI와 ViewModel 계약 자동 검증 완료 |

## 5. 이식 후 검증 방안

설정 검증:

- 기존 사용자의 설정을 WPF가 읽고 같은 UI 상태로 표시한다.
- 각 설정 변경 후 앱 재시작 시 값이 유지된다.
- default restore가 동작한다.
- 잘못된 경로/권한 부족/읽기 전용 파일에서 명확한 오류를 표시한다.
- 현재 1차 자동 검증은 임시 설정 파일에서 `SettingsService` typed key get/set, validation failure rollback, default restore, JSON import/export, backup, legacy value conversion 계약으로 수행한다. `LegacySettingsMapTests`로 현행 typed key 전체 inventory 포함, `FrmOptions.SaveVariables` 고위험 key 문서화, legacy alias migration, bool 호환 parsing, media scale 정규화, PowerPoint/media/display/alert/gap/lyrics monitor operational setting migration, `KeyBoardOption`/`GlobalHookKey_*` shortcut override 변환을 검증한다. `RegistryLegacySettingsSourceTests`로 실제 HKCU registry source가 legacy section의 문자열/DWORD 값을 읽고 누락 key를 생성하지 않으며 `MigrateLegacyAsync`에 연결되는지 검증한다. `FileLegacySettingsSourceTests`로 legacy INI/key-value 파일, JSON, .NET `user.config`/`appSettings` XML, `Properties.Settings` 호환 `<setting><value>` 구조, missing file no-create, registry 우선 composite fallback, `MigrateLegacyAsync` 연결을 검증한다. `SettingsBootstrapMigrationServiceTests`로 첫 실행 migration, 기존 settings.json 보존, 실패 시 settings.json 미생성을 검증한다.
- SettingsWindow 1차 자동 검증은 `SettingsWindowViewModel` 섹션 구성, 설정값 로드, 테마/크기 즉시 적용, invalid setting rollback, default restore, import 후 화면 갱신, DB 분석 결과 표시, 경로 선택 dialog command, picker 취소 처리, shortcut editor 항목 생성/저장/복원/충돌, operational setting 로드/저장/rollback 검증을 계약으로 수행한다. 실제 WPF 창의 keyboard focus order와 스크린샷 회귀 검증은 후속으로 추가한다.

자산 검증:

- 작업 폴더 복사 전후 파일 수와 hash가 일치한다.
- 이미지/미디어 경로가 깨지지 않는다.
- 중복 파일명 처리 규칙이 문서와 일치한다.
- 마이그레이션 취소 후 원본 앱 실행이 가능하다.
- 현재 1차 자동 검증은 임시 작업 폴더에서 `AssetMigrationService` dry-run/copy/hash/conflict/path error 계약으로 수행한다. `OperationalDataRehearsalServiceTests`로 설정의 `WorkingFolder`, `%AppData%\EasislidesNext\UserAssets` 계열 destination override, 백업 루트, AdminDB 파생 경로를 묶은 dry-run orchestration을 검증한다. 실제 `%AppData%`/운영 작업 폴더 수동 리허설은 후속으로 추가한다.

DB 검증:

- DB backup 생성 확인.
- 정상 DB open/save.
- 손상 DB 처리.
- schema version mismatch 안내.
- transaction 실패 시 rollback.
- 현재 1차 자동 검증은 임시 SQLite DB에서 `DatabaseMigrationService` schema version/table 분석, dry-run 무변경 보고, backup 생성, 순차 migration, transaction rollback, backup restore, missing/directory/corrupt path 오류 계약으로 수행한다. `AdminDatabaseRepositoryTests`로 임시 legacy AdminDB schema inventory, missing table/column 호환성 진단, bundled `AdminDB/Database/EasiSlidesDb.db` 실제 schema inventory, read-only folder song count, folder별 song summary 조회, 삭제 곡 조회, folder upsert backup, legacy SONG 필드 insert/update, 일반 song move의 `LastModified` 보존, soft delete/recover backup/transaction, folder reorder 시 `FOLDER.FolderNo`와 `SONG.FOLDERNO` 동시 갱신, song reorder 시 `SONG_NUMBER` 재시퀀싱과 rollback, song 이동 실패 시 transaction rollback 및 backup restore, 운영 DI 등록을 검증한다. `OperationalDataRehearsalServiceTests`로 운영 작업 폴더 기준 AdminDB 파생 경로 inventory와 AdminDB missing warning 경계를 검증한다. 운영 DB 복사본 기준 수동 리허설은 후속으로 추가한다.

## 6. 테스트 방안

자동 테스트:

- SettingsService default/get/set/validation tests
- legacy setting conversion tests
- migration dry-run tests
- path permission/error tests
- SQLite repository transaction tests
- asset copy hash tests

현재 자동화 완료:

- `SettingsServiceTests`: typed get/set 저장 및 변경 이벤트, invalid value rollback, default restore, JSON import/export, import 전 backup, invalid import 차단, legacy setting 변환 및 warning report 검증
- `LegacySettingsMapTests`: 현행 typed setting key 전체 인벤토리 포함, `FrmOptions.SaveVariables` 고위험 key 문서화, legacy alias 자동 이식, bool 호환 parsing, media scale 정규화, PowerPoint/media/display/alert/gap/lyrics monitor operational setting migration, legacy shortcut override 변환 검증
- `RegistryLegacySettingsSourceTests`: HKCU legacy registry section(`config`/`options`/`monitors`) 문자열/DWORD 읽기, 누락 값 조회 시 registry key 미생성, registry source 기반 `SettingsService.MigrateLegacyAsync` 검증
- `FileLegacySettingsSourceTests`: legacy INI/key-value 파일, JSON, .NET `user.config`/`appSettings` XML, `Properties.Settings` 호환 `<setting><value>` 구조, missing file no-create, `CompositeLegacySettingsSource`의 registry 우선/fallback, 파일 source 기반 `SettingsService.MigrateLegacyAsync` 검증
- `SettingsBootstrapMigrationServiceTests`: settings.json 최초 미생성 시 legacy migration 실행, 기존 settings.json 존재 시 migration skip, migration 실패 시 settings.json 미생성 검증
- `SettingsWindowViewModelTests`: 9개 설정 섹션 구성, 현재 설정 로드, 테마/크기 변경 저장 및 `IThemeService` 적용, invalid setting rollback, default restore, DB 분석 결과 표시, import 후 화면 갱신, 작업 폴더/AdminDB/백업 루트/설정 transfer path picker command, shortcut editor 항목 생성/override 저장/충돌 차단/기본값 복원, operational setting 로드/저장/rollback 검증
- `MainViewModelTests.OpenOutputCommand_UsesDefaultOutputMonitorFromSettings`: 저장/이식된 `DefaultOutputMonitorId`가 WPF 운영 셸의 초기 출력 모니터 선택에 적용되는지 검증
- `MainViewModelTests.OpenOutputCommand_WhenAlwaysUseSecondaryMonitorDisabledWithoutDefault_SelectsPrimary`, `MainViewModelTests.OpenOutputCommand_WhenDefaultMonitorMissingAndAlwaysUseSecondaryDisabled_FallsBackToPrimary`, `WindowPlacementServiceTests.CreateOutputPlacement_Fullscreen_UsesSettingsCustomBoundsWhenConfigured`, `WindowPlacementServiceTests.CreateOutputPlacement_Windowed_CentersPreviewInsideSettingsCustomBounds`: 저장/이식된 display/window placement 정책과 제거된 저장 모니터 fallback이 출력 모니터 선택과 출력 창 geometry에 반영되는지 검증
- `MediaPlaybackServiceTests.Load_WithSettingsService_UsesPersistedMediaDefaults`, `MediaPlaybackServiceTests.SettingsChanged_WhenMediaLoaded_AppliesPersistedMediaDefaults`: 저장/이식된 media volume/balance/mute 기본값과 runtime 변경 이벤트가 재생 스냅샷/백엔드에 반영되는지 검증
- `PowerPointRenderServiceTests.RenderSlideAsync_WhenRequestTimeoutIsZero_UsesSettingsTimeout`, `PowerPointRenderServiceTests.RenderSlideAsync_UsesSettingsBackedThumbnailCacheMegabytes`, `PowerPointRenderServiceTests.RenderSlideAsync_WhenThumbnailCacheSettingChanges_ReconfiguresOwnedCache`: 저장/이식된 PowerPoint render timeout과 thumbnail cache 용량이 렌더 서비스 기본값 및 runtime 변경에 반영되는지 검증
- `AppServiceRegistrationTests.ConfigureServices_ResolvesPowerPointRenderServiceWithSettingsBackedConstructor`: 운영 DI 등록이 PowerPoint 설정 소비 생성자와 Library/FolderEditor/SongEditor/SongCopy/SongMove ViewModel/Window 등록을 사용하도록 고정
- `SupportInfoServiceTests`: About/Registration/Help legacy copy, `RegistrationUser` 저장, 웹사이트/등록 페이지 launcher, `KeyBoardOption=1` shortcut override 기반 도움말 전환을 검증
- `AssetMigrationServiceTests`: dry-run 파일/sha256 report, 원본 무수정 복사, 복사 후 hash 검증, backup report 작성, 목적지 파일 충돌 safe-name 처리, source missing/source-not-directory 오류 분류 검증
- `DatabaseMigrationServiceTests`: SQLite schema version/table 분석, dry-run path 보고, backup 생성, 순차 migration, user_version 갱신, transaction rollback 및 backup restore, source missing/source-not-file/corrupt DB 오류 분류 검증
- `AdminDatabaseRepositoryTests`: 임시 legacy AdminDB 및 bundled `AdminDB/Database/EasiSlidesDb.db` schema/table/column inventory, `FOLDER`/`SONG` 필수 table/column 호환성 진단, read-only folder song count, folder별 song summary 조회, deleted song summary 조회, folder upsert backup, legacy SONG 필드 insert/update, update 시 `OldFolder` 보존, 일반 song 이동 시 `LastModified` 보존, soft delete/recover backup/transaction, folder reorder의 `FOLDER`/`SONG.FOLDERNO` staging 갱신, song reorder의 `SONG_NUMBER` 재시퀀싱과 rollback, song 이동 transaction rollback/backup restore, 운영 DI 등록 검증
- `OperationalDataRehearsalServiceTests`: 설정 기반 작업 폴더/백업 루트/AdminDB 파생 경로 해석, destination/backup directory 미생성 dry-run, 자산 scan과 DB inventory 통합, 작업 폴더 누락 error, AdminDB 누락 warning 검증
- `SettingsWindowViewModelTests.RunOperationalDataRehearsalAsync_*`: SettingsWindow 데이터 탭에서 현재 작업 폴더/AdminDB/백업 루트 값을 리허설 서비스에 전달하고, 파일/테이블/error/warning 요약과 검증 메시지, DB table inventory를 표시하는지 검증
- `OnboardingCoordinatorTests`: 최초 실행 온보딩 표시, InterfaceSize 저장/테마 적용, 완료 플래그 저장, 완료 후 재표시 방지, 취소 시 미완료 유지 검증

2026-05-29 검증 결과:

- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter SettingsServiceTests`: 7개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter LegacySettingsMapTests`: 9개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter RegistryLegacySettingsSourceTests`: 3개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "FileLegacySettingsSourceTests|ConfigureServices_ResolvesPowerPointRenderServiceWithSettingsBackedConstructor"`: 6개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter SettingsBootstrapMigrationServiceTests`: 3개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter SettingsWindowViewModelTests`: 20개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter AssetMigrationServiceTests`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter DatabaseMigrationServiceTests`: 7개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter OperationalDataRehearsalServiceTests`: 3개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SettingsWindowViewModelTests|AppServiceRegistrationTests|ThemeServiceTests"`: 36개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "OnboardingCoordinatorTests|AppServiceRegistrationTests|LegacySettingsMapTests"`: 13개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SupportInfoServiceTests|AppServiceRegistrationTests|LegacySettingsMapTests|MigrateLegacyAsync_ImportsRegistrationUser"`: 20개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "AdminDatabaseRepositoryTests|LibraryViewModelTests"`: 22개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "FolderEditorViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SongEditorViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SongCopyViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SongMoveViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "AdminDatabaseRepositoryTests|SongDeleteViewModelTests|SongRecoveryViewModelTests|AppServiceRegistrationTests"`: 25개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug`: 303개 통과
- `dotnet test Easislides.sln -c Debug`: 303개 통과
- `dotnet build Easislides.sln -c Release`: 성공
- `dotnet test Easislides.sln -c Release --no-build`: 303개 통과
- `gstack /qa`, `GSD verify-work`: 현재 작업 환경 PATH에 도구가 없어 실행 불가. 동일 요구사항은 xUnit/Release build/산출물 확인으로 대체 검증

수동 테스트:

1. WinForms에서 설정 변경
2. WPF 실행 후 설정 migration
3. SettingsWindow에서 값 확인
4. 테마/출력/단축키/PowerPoint 설정 변경
5. 앱 재시작
6. 작업 폴더 변경
7. 사용자 배경 이미지 적용
8. DB 백업/복구
9. WinForms rollback 실행 확인

운영 승인 기준:

- 실제 사용자 작업 폴더 3개 이상에서 migration 성공.
- migration report에 실패 항목 0개 또는 사용자가 이해 가능한 skip 항목만 존재.
- 원본 데이터 무손실.
- 설정 변경 후 라이브 출력 동작에 회귀 없음.
