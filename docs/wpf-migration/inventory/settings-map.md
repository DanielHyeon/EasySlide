# Legacy 설정 매핑 인벤토리

> 기준일: 2026-05-29
> 코드 기준: `Easislides.Wpf/Settings/LegacySettingsMap.cs`
> 범위: `FrmOptions.SaveVariables`, `gfConfig.InitEasiSlidesDir`, WPF `EasiSettingKeys`

## 1. 목적

이 문서는 WinForms `FrmOptions`와 `Gf` 전역 설정을 WPF typed settings로 옮기기 위한 매핑 인벤토리다. 자동 이식 가능한 항목은 `LegacySettingsMap`과 `SettingsService.MigrateLegacyAsync`에 연결하고, 아직 WPF key가 없는 항목은 `DocumentedOnly`로 추적한다.

## 2. 자동 이식 완료

| Legacy key | WPF key | 섹션 | 변환 |
|---|---|---|---|
| `Language` | `general.language` | 일반 | 문자열 |
| `WorkingFolder`, `RootEasiSlidesDir`, `root_directory` | `general.workingFolder` | 일반 | 경로 문자열 |
| `OnboardingCompleted` | `general.onboardingCompleted` | 일반 | bool |
| `RegistrationUser` | `general.registrationUser` | 일반 | 문자열, About dialog 등록 사용자 표시/저장 |
| `Theme` | `appearance.theme` | 화면 | enum |
| `InterfaceSize` | `appearance.interfaceSize` | 화면 | enum |
| `DefaultOutputMonitorId`, `OutputMonitorName` | `liveOutput.defaultOutputMonitorId` | 송출 | 문자열 |
| `UseSafetyConfirmations` | `liveOutput.useSafetyConfirmations` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `ShowLyricsMonitorAlertBox` | `liveOutput.showLyricsMonitorAlertBox` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `AdvanceNextItem` | `liveOutput.advanceNextItem` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `GapItemOption` | `liveOutput.gapItemOption` | 송출 | `None`/`Black`/`Default`/`User` enum 또는 legacy numeric `0..3` |
| `GapItemLogoFile` | `liveOutput.gapItemLogoFile` | 송출 | 경로 문자열 |
| `GapItemUseFade` | `liveOutput.gapItemUseFade` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `DMAlwaysUseSecondaryMonitor`, `AlwaysTryDualMonitor` | `liveOutput.displayAlwaysUseSecondaryMonitor` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `DMOption1Top`, `DualMonitorOptionCustomTop` | `liveOutput.displayCustomTop` | 송출 | 정수, -9999..9999 |
| `DMOption1Left`, `DualMonitorOptionCustomLeft` | `liveOutput.displayCustomLeft` | 송출 | 정수, -9999..9999 |
| `DMOption1Width`, `DualMonitorOptionCustomWidth` | `liveOutput.displayCustomWidth` | 송출 | 정수, 1..9999 |
| `LMTextColour`, `LyricsMonitorTextColour` | `liveOutput.lyricsMonitorTextColorArgb` | 송출 | ARGB 정수 |
| `LMBackColour`, `LyricsMonitorBackColour` | `liveOutput.lyricsMonitorBackgroundColorArgb` | 송출 | ARGB 정수 |
| `LMShowNotations`, `LyricsMonitorShowNotations` | `liveOutput.lyricsMonitorShowNotations` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `UsePowerpointTab`, `UsePowerPointTab` | `powerPoint.usePowerPointTab` | PowerPoint | bool, `true/false`, `1/0`, `yes/no` |
| `NoPowerpointPanelOverlay`, `NoPowerPointPanelOverlay` | `powerPoint.noPanelOverlay` | PowerPoint | bool, `true/false`, `1/0`, `yes/no` |
| `PowerPointRenderTimeoutSeconds` | `powerPoint.renderTimeoutSeconds` | PowerPoint | 정수, `PowerPointRenderService` 기본 timeout runtime 반영 |
| `ThumbnailCacheMegabytes` | `powerPoint.thumbnailCacheMegabytes` | PowerPoint | 정수, `PowerPointRenderService` 설정 기반 thumbnail cache 용량/runtime 재구성 반영 |
| `PowerPointMaxFiles`, `PowerpointMaxFiles`, `PP_MaxFiles` | `powerPoint.maxFiles` | PowerPoint | 정수, 1..100 |
| `UseMediaTab` | `media.useMediaTab` | 미디어 | bool, `true/false`, `1/0`, `yes/no` |
| `NoMediaPanelOverlay` | `media.noPanelOverlay` | 미디어 | bool, `true/false`, `1/0`, `yes/no` |
| `MediaDirectory`, `MediaDir`, `media_dir` | `media.directory` | 미디어 | 경로 문자열 |
| `MediaVolume`, `LiveCamVolume` | `media.volume` | 미디어 | 0..1 double, legacy 0..100 scale 자동 정규화, `MediaPlaybackService` default/runtime 반영 |
| `MediaBalance`, `LiveCamBalance` | `media.balance` | 미디어 | -1..1 double, legacy -100..100 scale 자동 정규화, `MediaPlaybackService` default/runtime 반영 |
| `MediaMuted`, `LiveCamMute` | `media.muted` | 미디어 | bool, `true/false`, `1/0`, `yes/no`, `MediaPlaybackService` default/runtime 반영 |
| `LiveCamNumber` | `media.liveCameraNumber` | 미디어 | 정수, 1..5 |
| `AdminDatabasePath`, `DBFileName` | `data.adminDatabasePath` | 데이터 | 경로 문자열 |
| `DataBackupRoot` | `data.backupRoot` | 데이터 | 경로 문자열 |
| `EnableDiagnostics` | `advanced.enableDiagnostics` | 고급 | bool |
| `KeyBoardOption` | `shortcuts` | 단축키 | `1`이면 local `Live.Previous`/`Live.Next`를 `PageUp`/`PageDown`으로 override |
| `GlobalHookKey_F7`, `GlobalHookKey_F8` | `shortcuts` | 단축키 | global `Live.Go`를 `F7` 우선, 없으면 `F8`로 override |
| `GlobalHookKey_F9`, `GlobalHookKey_F10` | `shortcuts` | 단축키 | global `Live.Black`을 `F9` 우선, 없으면 `F10`으로 override |
| `GlobalHookKey_Arrow`, `GlobalHookKey_CtrlArrow` | `shortcuts` | 단축키 | global `Live.Previous`/`Live.Next`를 `Up`/`Down` 또는 `Ctrl+Up`/`Ctrl+Down`으로 override |

## 3. typed key 완료, 후속 UI/운영 연결 필요

| Legacy key | 섹션 | 상태 |
|---|---|---|
| `UsePowerpointTab`, `NoPowerpointPanelOverlay`, `PP_MaxFiles` | PowerPoint | typed key, legacy migration, SettingsWindow 노출 완료. `MainViewModel` 탭 표시/PP 개수 제한, `OutputRenderer`/`OutputWindowViewModel` PowerPoint live panel overlay 숨김, `PowerPointRenderTimeoutSeconds`/`ThumbnailCacheMegabytes` runtime 소비처 연결 완료 |
| `UseMediaTab`, `NoMediaPanelOverlay`, `MediaDir`, `LiveCamNumber` | 미디어 | typed key, legacy migration, SettingsWindow 노출 완료. `MainViewModel` 탭 표시/미디어 runtime 상태, `MediaPlaybackService.LoadFromMediaDirectory`/`LoadLiveCamera`, `OutputRenderer`/`OutputWindowViewModel` media live panel overlay 숨김 runtime 소비처 연결 완료 |
| `ShowLyricsMonitorAlertBox`, `AdvanceNextItem`, `GapItemOption`, `GapItemLogoFile`, `GapItemUseFade` | 송출 | typed key, legacy migration, SettingsWindow 노출 완료. `AdvanceNextItem` 운영 selection advance, alert box visibility, gap ready scene/fade snapshot runtime 연결 완료 |
| `DMAlwaysUseSecondaryMonitor`, `DMOption1Top`, `DMOption1Left`, `DMOption1Width` | 송출 | typed key, legacy migration, SettingsWindow 노출 완료. display/window placement runtime 연결 완료 |
| `LMTextColour`, `LMBackColour`, `LMShowNotations` | 송출 | typed key, legacy migration, SettingsWindow 노출 완료. `OutputWindowViewModel` lyrics monitor foreground/background/notation visibility runtime 연결 완료 |

## 4. 검증

- `LegacySettingsMapTests`: 현행 `EasiSettingKeys` 전체가 인벤토리에 포함되는지 검증한다.
- `LegacySettingsMapTests`: `FrmOptions.SaveVariables`의 고위험 key가 문서화되어 있는지 검증한다.
- `LegacySettingsMapTests`: `root_directory`, `RegistrationUser`, `OutputMonitorName`, `LiveCamVolume`, `LiveCamBalance`, `LiveCamMute`, `DBFileName` 별칭 migration, scale 정규화, operational setting alias migration, `KeyBoardOption`/`GlobalHookKey_*` shortcut override 변환을 검증한다.
- `RegistryLegacySettingsSourceTests`: `RegUtil` 호환 HKCU section(`config`/`options`/`monitors`)에서 문자열/DWORD 값을 읽고, 누락 값 조회 시 legacy registry key를 생성하지 않으며, registry source가 `SettingsService.MigrateLegacyAsync`에 연결되는지 검증한다.
- `FileLegacySettingsSourceTests`: legacy INI/key-value 파일, JSON, .NET `user.config`/`appSettings` XML, `Properties.Settings` 호환 `<setting><value>` 구조, missing file no-create, `CompositeLegacySettingsSource`의 registry 우선/fallback, file source가 `SettingsService.MigrateLegacyAsync`에 연결되는지 검증한다.
- `SettingsBootstrapMigrationServiceTests`: WPF production 시작 시 settings.json이 없는 경우 registry migration이 실행되고, 이미 settings.json이 있으면 기존 사용자 설정을 보존하며, migration 실패 시 settings.json을 만들지 않는지 검증한다.

## 5. 완료 여부

현재 상태는 **부분 구현**이다. 자동 이식 가능한 핵심 경로/모니터/미디어/DB 별칭, `RegistrationUser`, legacy shortcut override, WPF `OnboardingCompleted`, `FrmOptions`의 PowerPoint/media/display/alert/gap/lyrics monitor 세부 key는 WPF typed key, legacy migration, SettingsWindow 노출까지 구현됐다. 실제 legacy registry source 연결은 `RegistryLegacySettingsSource`로 완료됐고, legacy file source fallback은 `FileLegacySettingsSource`/`CompositeLegacySettingsSource`로 완료됐으며, production 시작 시 첫 실행 migration 경계는 `SettingsBootstrapMigrationService`로 연결됐다. runtime 소비처 중 `DefaultOutputMonitorId`와 `DisplayAlwaysUseSecondaryMonitor`는 `MainViewModel.RefreshOutputDisplays` 초기 선택 및 제거된 저장 모니터 fallback으로, `DisplayCustomTop`/`DisplayCustomLeft`/`DisplayCustomWidth`는 `WindowPlacementService` custom bounds로, `MediaVolume`/`MediaBalance`/`MediaMuted`/`MediaDir`/`LiveCamNumber`는 `MediaPlaybackService` load/default, media directory, live camera source 및 settings change 경계로, `UsePowerpointTab`/`UseMediaTab`/`PP_MaxFiles`는 `MainViewModel` 탭 표시와 PowerPoint queue 제한으로, `NoPowerpointPanelOverlay`/`NoMediaPanelOverlay`는 `OutputRenderer`/`OutputWindowViewModel` overlay visibility로, `ShowLyricsMonitorAlertBox`/`AdvanceNextItem`/`GapItemOption`/`GapItemLogoFile`/`GapItemUseFade`/`LMTextColour`/`LMBackColour`/`LMShowNotations`는 WPF live output renderer/ViewModel 경계로 연결됐다. 운영 데이터 dry-run 리허설은 `OperationalDataRehearsalService`와 SettingsWindow 데이터 탭 실행/요약/검증 메시지/DB table inventory 표시로 연결됐으며, first-run InterfaceSize 온보딩은 `WelcomeWindow`/`OnboardingCoordinator`로 연결됐다. 남은 범위는 실제 운영 `%AppData%` 데이터 기반 수동 리허설과 데이터 마이그레이션 온보딩 UI 연결이다.
