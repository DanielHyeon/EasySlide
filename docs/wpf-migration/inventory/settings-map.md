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
| `Theme` | `appearance.theme` | 화면 | enum |
| `InterfaceSize` | `appearance.interfaceSize` | 화면 | enum |
| `DefaultOutputMonitorId`, `OutputMonitorName` | `liveOutput.defaultOutputMonitorId` | 송출 | 문자열 |
| `UseSafetyConfirmations` | `liveOutput.useSafetyConfirmations` | 송출 | bool, `true/false`, `1/0`, `yes/no` |
| `PowerPointRenderTimeoutSeconds` | `powerPoint.renderTimeoutSeconds` | PowerPoint | 정수 |
| `ThumbnailCacheMegabytes` | `powerPoint.thumbnailCacheMegabytes` | PowerPoint | 정수 |
| `MediaVolume`, `LiveCamVolume` | `media.volume` | 미디어 | 0..1 double, legacy 0..100 scale 자동 정규화 |
| `MediaBalance`, `LiveCamBalance` | `media.balance` | 미디어 | -1..1 double, legacy -100..100 scale 자동 정규화 |
| `MediaMuted`, `LiveCamMute` | `media.muted` | 미디어 | bool, `true/false`, `1/0`, `yes/no` |
| `AdminDatabasePath`, `DBFileName` | `data.adminDatabasePath` | 데이터 | 경로 문자열 |
| `DataBackupRoot` | `data.backupRoot` | 데이터 | 경로 문자열 |
| `EnableDiagnostics` | `advanced.enableDiagnostics` | 고급 | bool |
| `KeyBoardOption` | `shortcuts` | 단축키 | `1`이면 local `Live.Previous`/`Live.Next`를 `PageUp`/`PageDown`으로 override |
| `GlobalHookKey_F7`, `GlobalHookKey_F8` | `shortcuts` | 단축키 | global `Live.Go`를 `F7` 우선, 없으면 `F8`로 override |
| `GlobalHookKey_F9`, `GlobalHookKey_F10` | `shortcuts` | 단축키 | global `Live.Black`을 `F9` 우선, 없으면 `F10`으로 override |
| `GlobalHookKey_Arrow`, `GlobalHookKey_CtrlArrow` | `shortcuts` | 단축키 | global `Live.Previous`/`Live.Next`를 `Up`/`Down` 또는 `Ctrl+Up`/`Ctrl+Down`으로 override |

## 3. 문서화 완료, 후속 key 필요

| Legacy key | 섹션 | 상태 |
|---|---|---|
| `UsePowerpointTab`, `NoPowerpointPanelOverlay`, `PP_MaxFiles` | PowerPoint | WPF key 필요 |
| `UseMediaTab`, `NoMediaPanelOverlay`, `MediaDir`, `LiveCamNumber` | 미디어 | WPF key 필요 |
| `ShowLyricsMonitorAlertBox`, `AdvanceNextItem`, `GapItemOption`, `GapItemLogoFile`, `GapItemUseFade` | 송출 | WPF key 필요 |
| `DMAlwaysUseSecondaryMonitor`, `DMOption1Top`, `DMOption1Left`, `DMOption1Width` | 송출 | display/window placement 설정으로 확장 필요 |
| `LMTextColour`, `LMBackColour`, `LMShowNotations` | 송출 | lyrics monitor appearance 설정으로 확장 필요 |

## 4. 검증

- `LegacySettingsMapTests`: 현행 `EasiSettingKeys` 전체가 인벤토리에 포함되는지 검증한다.
- `LegacySettingsMapTests`: `FrmOptions.SaveVariables`의 고위험 key가 문서화되어 있는지 검증한다.
- `LegacySettingsMapTests`: `root_directory`, `OutputMonitorName`, `LiveCamVolume`, `LiveCamBalance`, `LiveCamMute`, `DBFileName` 별칭 migration, scale 정규화, `KeyBoardOption`/`GlobalHookKey_*` shortcut override 변환을 검증한다.

## 5. 완료 여부

현재 상태는 **부분 구현**이다. 자동 이식 가능한 핵심 경로/모니터/미디어/DB 별칭과 legacy shortcut override는 구현됐고, `FrmOptions`의 세부 display, alert, folder formatting key는 문서화됐지만 아직 WPF typed key와 UI가 필요하다.
