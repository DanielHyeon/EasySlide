# Final Gate Evidence

작성일: 2026-06-21

대상 change: `a010-wpf-frmmain-1to1-operator-console-parity`

## Gate Summary

| Gate | Result | Evidence |
| --- | --- | --- |
| OpenSpec strict validation | PASS | `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict` -> `Change is valid` |
| WPF tests | PASS | `dotnet test Easislides.Wpf.Tests -v minimal` -> 실패 0, 통과 2417, 건너뜀 0 |
| WinForms build | PASS | `dotnet build Easislides\Easislides.csproj -nologo -v minimal` -> 오류 0, 경고 13 |
| WPF launch with legacy data | PASS | `EasislidesNext.exe` 실행 및 `C:\EasiSlides` 데이터 로딩 확인 |
| Actual manual UAT | RECORDED | 62개 row 모두 판정 기록: PASS 25, PARTIAL 25, BLOCKED 12, FAIL 0 |
| PARTIAL/BLOCKED disposition | RECORDED | `follow-ups.md`에 Follow-Up Tasks 및 Explicit Defers로 전환 |
| Phase 8 PARTIAL/BLOCKED closure | RECORDED | PARTIAL 25개 재확인: 코드 보정 3, 구현/테스트 증거로 닫음 19, 외부 송출 모니터 관찰 필요 3. BLOCKED 12개는 source 폴더/설정/선택 상태 재검증 필요로 유지 |
| Phase 8 focused tests | PASS | `LegacyImageThumbnailSizeConverterTests|MainMenuBarTests`: 실패 0, 통과 137, 건너뜀 0 |
| Phase 8 full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests -v minimal`: 실패 0, 통과 2421, 건너뜀 0 |
| Phase 8 OpenSpec validation | PASS | `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`: Change is valid |
| Phase 8 WinForms build | PASS | `dotnet build Easislides\Easislides.csproj -nologo -v minimal`: 오류 0, 경고 13 |

| Phase 9 registry runtime sync | PASS | `HKCU\Software\EasiSlides` 확인 후 기존 WPF `settings.json` 존재 시 `current_praisebook` 등 runtime settings가 재반영되지 않는 원인을 수정 |
| Phase 9 focused settings tests | PASS | `SettingsBootstrapMigrationServiceTests|RegistryLegacySettingsSourceTests`: 실패 0, 통과 8, 건너뜀 0 |
| Phase 9 full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests -v minimal`: 실패 0, 통과 2423, 건너뜀 0 |
| Phase 9 OpenSpec validation | PASS | `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`: Change is valid |
| Phase 10 C:\EasiSlides WPF deployment | PASS | `dotnet publish ... -o C:\EasiSlides\EasislidesNext`, deployed `EasislidesNext.exe` launch smoke 통과 |
| Phase 10 registry/settings parity | PASS | WPF deployed run synced `WorkingFolder=C:\EasiSlides\`, `CurrentWorshipListName=1.주일예배`, `CurrentPraiseBookName=PraiseBook 1`, `UsePowerPointTab=false`, `UseMediaTab=false`, `MediaDirectory=C:\EasiSlides\Media\` |
| Phase 10 WinForms comparison smoke | PASS | `C:\EasiSlides\Easislides.exe` launch smoke 통과, 동일 source list/Worship 23 items 화면 확인 |

## Manual UAT Evidence

- Checklist: `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`
- Summary: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/manual-uat-summary.md`
- Screenshots: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/`
- Deployment screenshots: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/screenshots/2026-06-21/phase10-deployed/`
- Follow-up/defer register: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/follow-ups.md`

## Known Non-Ship Items

이 gate는 "검증 증거가 기록되었다"는 의미이며, 모든 parity gap이 닫혔다는 의미는 아니다.

- Gesture parity follow-up: song double-click/drag, Bible drag/Shift-click, Worship List drag/context menu.
- Visual usability follow-up: Images thumbnail density, Default first-viewport density/order, Praise Book toolbar/list density.
- External output hardware follow-up: black/clear/hide/restore 및 shortcut 결과의 실제 송출 모니터 확인.
- Source/config defers: InfoScreen 직접 폴더, PowerPoint/Media source tab 옵션과 경로, PraiseBook 선택 책. 운영 PPT/Media는 `C:\EasiSlides\Documents` 바로가기 대상인 `D:\예배자료`에 존재함을 후속 확인했다.

## Registry Runtime Settings Correction

사용자 확인에 따라 `HKCU\Software\EasiSlides`를 재검토했다. WPF는 `RegistryLegacySettingsSource`와 legacy settings map을 이미 가지고 있었지만, `%APPDATA%\EasislidesNext\settings.json`이 존재하면 bootstrap migration이 skip되어 `current_praisebook`, `current_session`, `UsePowerpointTab`, `UseMediaTab`, `media_dir` 같은 FrmMain runtime settings가 재반영되지 않았다.

Phase 9에서 이 원인을 코드로 보정했다. 기존 WPF 작업 폴더는 반복 full migration으로 덮어쓰지 않고, MainWindow 시작 시 registry-backed runtime settings만 좁게 재동기화한다.

## Phase 11 PPT Resolution And Preview Speed Gate

| Gate | Result | Evidence |
| --- | --- | --- |
| WinForms PPT export baseline | PASS | `OfficeLib/PowerPoint.cs` uses `EXPORT_WIDTH=640`, `EXPORT_HEIGHT=480` |
| WPF PPT render size parity | PASS | `LegacyPowerPointImageSize` centralizes WPF PPT preview/source/thumbnail render requests to `640x480` |
| Focused tests | PASS | `PowerPointPreviewViewModelTests|PowerPointLibraryViewModelTests|MainViewModelTests|MainMenuBarTests`: 실패 0, 통과 791, 건너뜀 0 |
| Full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests -v minimal`: 실패 0, 통과 2423, 건너뜀 0 |
| OpenSpec strict validation | PASS | `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`: Change is valid |
| C:\EasiSlides deployed publish and launch | PASS | `dotnet publish ... -o C:\EasiSlides\EasislidesNext`; launch smoke `MainWindowTitle=EasiSlides` |

## Phase 12 Worship List Toolbar Gate

| Gate | Result | Evidence |
| --- | --- | --- |
| WinForms toolbar baseline | PASS | `panelWorshipList2`/`toolStripWorshipList2` 기준: 폭 33px, `VerticalStackWithOverflow`, `WL_Up` 22x22 |
| WPF toolbar layout parity | PASS | `ClassicWorshipListToolStrip1` 상단 수평 스트립과 `ClassicWorshipListToolStrip2` 오른쪽 33px 세로 레일로 분리 |
| WPF toolbar composition parity | PASS | 세로 레일은 WinForms `toolStripWorshipList2.Items`와 동일하게 `WL_Up`, `WL_Down`, `WL_Delete`, separator, `WL_Word`, `WL_Notes`만 포함 |
| Button size parity | PASS | 레일 버튼 `MinWidth=0`, `MinHeight=0`, `Width=22`, `Height=22` 고정 |
| Focused tests | PASS | `WorshipListPanelTests`: 실패 0, 통과 22, 건너뜀 0 |
| Full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`: 실패 0, 통과 2422, 건너뜀 0 |
| Screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase12-worship-toolbar/wpf-worship-list-toolbar-after.png` |
| C:\EasiSlides deployment | PASS | `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal` |
| Deployed launch smoke | PASS | `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`, `MainWindowTitle=EasiSlides`, `LastWriteTime=2026-06-21 22:19:38` |
| Deployed screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase12-worship-toolbar/wpf-deployed-worship-list-toolbar-after-exact-buttons.png` |

## Phase 13 Main Top ToolStrip Single-Row Gate

| Gate | Result | Evidence |
| --- | --- | --- |
| WinForms toolbar baseline | PASS | `FrmMain.Designer.cs` 기준 `toolStripMain.Items`는 `Main_New`부터 `Main_JumpC`까지 한 줄 `ToolStrip`, `Size=632x31` |
| WPF top toolbar non-wrap parity | PASS | `toolStripMain`을 `StackPanel Orientation=Horizontal`로 고정해 `WrapPanel` 줄바꿈 가능성을 제거 |
| WPF second-row removal | PASS | WPF 전용 `ClassicOperatorBar`는 `Visibility=Collapsed`; 상단 visible toolbar row를 만들지 않음 |
| Command retention | PASS | 라이브/출력 명령은 메뉴 및 Preview/Output 패널 경로에 유지되고, 숨겨진 OperatorBar XAML은 command regression guard로 남김 |
| Focused tests | PASS | `MainMenuBarTests`: 실패 0, 통과 134, 건너뜀 0 |
| Full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`: 실패 0, 통과 2423, 건너뜀 0 |
| C:\EasiSlides deployment | PASS | `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal` |
| Deployed launch smoke | PASS | `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`, `MainWindowTitle=EasiSlides`, `LastWriteTime=2026-06-21 22:25:30` |
| Deployed screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase13-top-toolbar/wpf-deployed-top-toolbar-single-row.png` |

## Phase 14 Worship List Horizontal ToolStrip Runtime Gate

| Gate | Result | Evidence |
| --- | --- | --- |
| WinForms runtime baseline | PASS | `ResizeComboAndToolBar`가 `SessionList`를 남는 폭으로 늘리고 `panelWorshipList1`을 그 오른쪽에 배치 |
| WPF horizontal toolbar row parity | PASS | `ClassicWorshipListToolStrip1`은 별도 row가 아니라 `ClassicWorshipListSessionStrip` 오른쪽 94px column에 위치 |
| WPF-only load button removal | PASS | WinForms에 없는 `LoadSelectedWorshipListCommand` 버튼을 해당 band에서 제거 |
| Button composition/size parity | PASS | `WL_Manage`, `WL_Add`, `WL_Open` 순서 유지, 각 29px 폭, WinForms tooltip 문구 반영 |
| Focused tests | PASS | `WorshipListPanelTests`: 실패 0, 통과 22, 건너뜀 0 |
| Full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`: 실패 0, 통과 2423, 건너뜀 0 |
| C:\EasiSlides deployment | PASS | `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal` |
| Deployed launch smoke | PASS | `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`, `MainWindowTitle=EasiSlides`, `LastWriteTime=2026-06-21 22:34:18` |
| WinForms screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase14-worship-horizontal-toolbar/winforms-worship-horizontal-toolbar-crop.png` |
| WPF screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase14-worship-horizontal-toolbar/wpf-deployed-worship-horizontal-toolbar-crop-final.png` |

## Phase 15 Worship List Toolbar Icon Spacing Gate

| Gate | Result | Evidence |
| --- | --- | --- |
| Horizontal icon spacing | PASS | `WL_Manage`와 `WL_Add`에 오른쪽 3px 간격을 추가하고 `WL_Open`은 마지막 0px margin으로 유지하여 94px band 안의 한 줄 배치를 보존 |
| Vertical icon spacing | PASS | `WL_Up/WL_Down/WL_Delete/WL_Word`는 22x22 크기 유지 + 하단 3px 간격, side rail style은 `HorizontalAlignment=Center` |
| Layout parity guard | PASS | 가로 toolbar wrap 없음, 세로 rail width 33px 유지, WinForms 기준 버튼 수와 순서 유지 |
| Focused tests | PASS | `WorshipListPanelTests`: 실패 0, 통과 22, 건너뜀 0 |
| Full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`: 실패 0, 통과 2423, 건너뜀 0 |
| C:\EasiSlides deployment | PASS | `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal` |
| WPF deployed screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase15-worship-toolbar-spacing/wpf-deployed-worship-toolbar-spacing-crop.png`, LastWriteTime=`2026-06-21 22:41:24` |

## Phase 16 Worship List RTF Icon And Session Combo Text Gate

| Gate | Result | Evidence |
| --- | --- | --- |
| RTF icon clarity | PASS | `WL_Word` icon changed from `DocumentArrowDown24` to `DocumentText24` so it reads as text document generation rather than download |
| Session combo text clipping | PASS | `SessionCombo` keeps 28px height but uses `Padding="8,2,4,2"` and `VerticalContentAlignment="Center"` |
| Layout parity guard | PASS | Worship List toolbar button count, order, 22x22 side rail, 94px horizontal band remain unchanged |
| Focused tests | PASS | `WorshipListPanelTests`: 실패 0, 통과 22, 건너뜀 0 |
| Full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`: 실패 0, 통과 2423, 건너뜀 0 |
| C:\EasiSlides deployment | PASS | First publish was blocked by running `EasislidesNext (17456)` DLL lock; after closing that deployed process, `dotnet publish ... -o C:\EasiSlides\EasislidesNext` passed |
| WPF deployed screenshot evidence | PASS | `evidence/screenshots/2026-06-21/phase16-worship-rtf-combo-polish/wpf-deployed-worship-rtf-combo-polish-crop.png`, LastWriteTime=`2026-06-21 22:49:27` |

## C:\EasiSlides Deployment Verification

WPF Release 산출물을 `C:\EasiSlides\EasislidesNext`에 배포하고, 기존 WinForms `C:\EasiSlides\Easislides.exe`와 같은 운영 루트/레지스트리 기준으로 실행 검증했다.

- WPF 배포본: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
- WinForms 배포본: `C:\EasiSlides\Easislides.exe`
- WPF launch result: `MainWindowTitle=EasiSlides`, descendants 358개, `Legacy worship list loaded: 1.주일예배 (23 .esw items)`.
- WPF settings result: `WorkingFolder=C:\EasiSlides\`, `CurrentWorshipListName=1.주일예배`, `CurrentPraiseBookName=PraiseBook 1`, `UsePowerPointTab=false`, `UseMediaTab=false`, `MediaDirectory=C:\EasiSlides\Media\`.
- Praise Book tab result: `찬양집 열림: PraiseBook 1 (0곡)`.
- WinForms comparison result: `MainWindowTitle=EasiSlides`, same Folders song list and Worship 23 items screen observed.

## Warning Notes

- `dotnet test`와 `dotnet build` 모두 기존 `NU1701` NetOffice compatibility warning을 포함한다.
- WinForms build는 기존 `NETSDK1137` WindowsDesktop SDK warning을 포함한다.
- 위 warning들은 이번 WPF MainWindow manual UAT 문서화 작업에서 새로 도입한 오류가 아니며 build/test gate를 실패시키지 않았다.
