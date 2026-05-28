# 운영 셸 및 라이브 송출 이식 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: Stage Safety, LiveBar, 키보드/리모컨 우선 원칙
- `docs/adr/0001-wpf-ui-framework.md`: WPF + WPF UI 채택
- `docs/adr/0002-fluent-icons.md`: 라이브/송출 제어 아이콘 전환
- `docs/adr/0004-hookmanager-preservation.md`: 글로벌 단축키 보존
- `docs/adr/0006-senior-mode-token-scale.md`: 큰 글자/터치 타깃
- `docs/adr/0007-legacy-ui-safety-net.md`: legacy UI fallback 유지
- `docs/ui/icon-migration-map.md`: `Icon.Live.*`, `Icon.Monitor.*` 매핑

## 1. 범위

이 문서는 기존 `Easislides/Easislides`의 메인 운영 화면과 라이브 송출 흐름을 WPF로 옮기는 계획이다.

대상 legacy 파일:

| Legacy | 역할 | WPF 목표 |
|---|---|---|
| `FrmMain.cs`, `FrmMain.Events.cs`, `FrmMain.Layout.cs`, `FrmMain.Logic.cs`, `FrmMain.Fields.cs` | 메인 운영 워크플로우, 리스트/프리뷰/송출 명령 | `MainWindow`, `MainViewModel`, 기능별 패널 |
| `FrmInfoScreen.cs` | 보조 정보/출력 화면 | `OutputInfoWindow`, `OutputScreenService` |
| `FrmLyricsScreen.cs` | 가사 송출 화면 | `LyricsOutputWindow` |
| `FrmLaunchShow.cs` | 쇼 실행/출력 선택 | `LaunchShowDialog` 또는 `ShowSetupPanel` |
| `FrmMediaPlayerControl.cs`, `FrmLaunchMediaPlayer.cs` | 미디어 재생 컨트롤 | `MediaControlPanel`, `MediaPlaybackViewModel` |
| `FrmShowAlert.cs`, `FrmSingleMonitorAlert.cs`, `FrmPopupText.cs` | 라이브 위험 알림/팝업 | `EsDialog`, `SafetyConfirm`, toast/inline alert |
| `FrmSplashScreen.cs` | 시작 상태 표시 | WPF splash/onboarding |

현재 WPF 기반:

- `Easislides.Wpf/Demo/DemoWindow.xaml`
- `Easislides.Wpf/Composites/LiveBar.xaml`
- `Easislides.Wpf/Composites/SafetyConfirm.cs`
- `Easislides.Wpf/Controls/*`
- `Easislides.Wpf/Input/ShortcutRegistry.cs`
- `Easislides.Wpf/Media/MediaPlaybackBackend.cs`

## 2. 목표 UI 구조

메인 창은 운영자가 반복 사용하는 화면이므로 마케팅형 레이아웃이 아니라 밀도 있는 작업 도구로 설계한다.

권장 레이아웃:

| 영역 | 내용 | 비고 |
|---|---|---|
| 상단 LiveBar | 라이브/대기/숨김 상태, 현재 송출 항목, 다음 항목, 단축키 힌트 | `LiveBar` 확장 |
| 좌측 탐색 | 예배 목록, 폴더, 성경, 미디어, 설정 진입 | 고정 폭 + 키보드 포커스 |
| 중앙 작업 | 선택된 목록/검색 결과/편집 대상 | 가변 폭 |
| 우측 프리뷰 | 현재/다음 슬라이드, 이미지/영상/PPT 썸네일 | 출력과 동등성 비교 가능 |
| 하단 명령 | Black, Hide, Go Live, Next/Prev, media control | 위험 명령은 `SafetyConfirm` |

## 3. 이식 단계

### 3.0 ADR 준수 체크

- 라이브/송출 명령 버튼은 `Icon.Live.*` 키 또는 WPF UI `SymbolIcon`을 사용한다.
- 기존 Label-click 기반 위험 명령은 실제 Button/Command로 바꾼다.
- Black/Hide/Go Live는 EasiDS `Brush.Live.*`, `Brush.Danger.*` 토큰으로 상태를 표현한다.
- Senior 모드에서 LiveBar, 하단 명령, 출력 선택 UI가 최소 44px 타깃을 유지한다.
- M3까지 WinForms 메인 화면으로 되돌아갈 수 있는 실행 경로를 유지한다.

### 3.1 기능 인벤토리 고정

- `FrmMain`의 메뉴, 버튼, 라벨 클릭 핸들러를 전부 목록화한다.
- 라이브 상태를 바꾸는 명령과 단순 편집 명령을 분리한다.
- `FrmMain.Fields.cs`의 전역 상태를 `LiveSessionState`, `SelectionState`, `PreviewState`로 분류한다.
- `FrmMain.Logic.cs`의 동기 작업 중 UI freeze 가능 구간을 표시한다.

산출물:

- `docs/wpf-migration/inventory/frm-main-command-map.md`
- `docs/wpf-migration/inventory/live-state-map.md`

### 3.2 ViewModel/Service 경계 작성

| 계약 | 책임 |
|---|---|
| `ILiveSessionService` | 현재 송출 상태, 라이브 시작/중지, 숨김/검은 화면 |
| `IPreviewService` | 선택 항목의 프리뷰 이미지/메타데이터 제공 |
| `IOutputWindowService` | 출력 모니터 창 열기/닫기/위치 갱신 |
| `IAlertService` | 위험 명령 확인, toast, inline alert |
| `ICommandTelemetry` | 라이브 중 명령 기록, 실패 로그 |

완료 조건:

- WPF ViewModel에서 WinForms Form 인스턴스를 직접 참조하지 않는다.
- `gf*` 호출은 서비스 구현 내부로만 제한한다.
- 라이브 상태 변경은 단일 service를 통해서만 일어난다.

### 3.3 메인 셸 구현

- `MainWindow.xaml` 추가
- `MainViewModel` 추가
- 기존 `DemoWindow`는 컨트롤 갤러리/개발 검증용으로 유지
- `App.xaml.cs` 시작 창을 설정으로 분기: 개발 모드는 Demo, 운영 모드는 Main
- `LiveBar`를 실제 `ILiveSessionService` 상태에 바인딩
- `ShortcutRegistry`를 MainWindow `PreviewKeyDown`에 연결

### 3.4 라이브 위험 액션 보호

보호 대상:

- Go Live
- Black screen
- Hide output
- Media stop/restart
- Output monitor 변경
- PPT 파일 재로드
- 현재 송출 항목 삭제/이동

규칙:

- 라이브 중 destructive 명령은 `SafetyConfirm`을 통과해야 한다.
- 확인 문구에는 대상과 결과가 들어가야 한다.
- 취소 후 라이브 상태는 변하지 않아야 한다.
- 모든 위험 액션은 `ICommandTelemetry`에 기록한다.

### 3.5 출력 화면 이식

- `FrmInfoScreen`과 `FrmLyricsScreen`을 각각 WPF `Window`로 대체한다.
- 출력 화면은 borderless/fullscreen, DPI aware, 모니터 좌표 고정이 필요하다.
- 테스트를 위해 "창 모드 출력"을 지원한다.
- 출력 화면의 내용은 메인 프리뷰와 같은 ViewModel snapshot을 사용한다.

## 4. 완료 여부

| 항목 | 상태 | 비고 |
|---|---|---|
| WPF 앱 골격 | 완료 | `Easislides.Wpf` 존재 |
| LiveBar 컴포지트 | 1차 구현 완료 | `LiveSessionService` snapshot과 `MainViewModel.ApplyLiveSnapshot`으로 상태/현재 항목/출력 모니터 연결 완료 |
| SafetyConfirm | 1차 운영 연결 완료 | Go Live, Stop Live, 라이브 중 출력 닫기, Black/Hide 명령이 `ILiveSafetyPrompt` 확인 경계를 통과 |
| MainWindow | 1차 구현 완료 | `MainWindow`, `MainViewModel`, 기본 운영 셸 및 단축키 연결 |
| 출력 화면 WPF 대체 | 1차 구현 완료 | `OutputWindow`, `OutputWindowHost`, `OutputWindowViewModel`, `IDisplayService` 연결. borderless/fullscreen 및 창 모드 배치, 출력 모니터 선택, 라이브 snapshot 반영 완료. 실제 PPT/가사 렌더링 동등성은 M3에서 검증 |
| 미디어 컨트롤 WPF 대체 | 1차 구현 완료 | `IMediaPlaybackService`, `MediaPlaybackService`, `MediaPlaybackViewModel`, `IMediaPlaybackBackend`, `NoOpMediaPlaybackBackend`, `WpfMediaElementPlaybackBackend` 추가. 재생/일시정지/정지/seek/mute/repeat/volume/balance 상태 계약, backend 명령 위임, 모든 주요 backend 명령 실패 시 `Failed` snapshot 전환, WPF `MediaElement` 파일 adapter 경계와 command 기반 ViewModel 검증 완료. DirectShow adapter 및 출력 화면 visual host 연결은 M3에서 계속 진행 |
| 키보드/리모컨 연결 | 1차 구현 완료 | `ShortcutRegistry`, `MainViewModel.BindShortcuts`, `GlobalInputService`, HookManager adapter 운영 연결 완료. 실제 리모컨/타 앱 포커스 수동 검증은 M5에서 계속 수행 |

## 5. 이식 후 검증 방안

기능 검증:

- 앱 시작 후 메인 창이 3초 이내 표시된다.
- 예배 목록을 선택하면 중앙 목록과 프리뷰가 갱신된다.
- Go Live 실행 시 LiveBar가 Active로 바뀐다.
- Next/Prev 명령이 현재 출력과 다음 프리뷰를 정확히 갱신한다.
- Black/Hide 상태는 LiveBar와 출력 창에 동시에 반영된다.
- 출력 모니터 변경 후 기존 출력 창이 잘못된 모니터에 남지 않는다.
- 허용된 글로벌 단축키가 HookManager adapter를 통해 `ShortcutRegistry`의 동일 command id로 라우팅된다.

라이브 안전 검증:

- 라이브 중 Go Live, Stop Live, 출력 닫기, Black/Hide 명령은 확인 없이 실행되지 않는다.
- 취소 시 상태가 보존된다.
- 실패 시 toast와 로그가 남는다.

UX 검증:

- 다크 모드에서 LiveBar와 위험 버튼이 즉시 구분된다.
- 라이트 모드에서 장시간 준비 작업 시 대비가 충분하다.
- Senior 모드에서 상단/하단 명령이 잘리지 않는다.
- 키보드 포커스 순서가 좌측 탐색 -> 중앙 목록 -> 프리뷰 -> 주요 명령 순으로 자연스럽다.

## 6. 테스트 방안

자동 테스트:

- `LiveBarViewModelTests`: 상태/라벨/표시 여부
- `SafetyConfirmTests`: 위험 명령 확인/취소/중복 실행 방지
- `ShortcutRegistryTests`: 로컬/글로벌 단축키 매핑
- `MainViewModelTests`: 선택 항목 변경, 라이브 상태 전이, 명령 enable/disable, 위험 명령 SafetyConfirm 경계
- `OutputWindowServiceTests`: 주입된 window placement 정책 사용, 모니터 좌표 계산, 창 재배치 정책
- `OutputWindowHostTests`: 출력 창 생성/재배치/닫기, 라이브 세션 snapshot 바인딩, 이벤트 구독 해제
- `OutputWindowViewModelTests`: Active/Hidden/Blackout/Standby 표시 라벨
- `DisplayServiceTests`: 출력 모니터 열거 fallback 및 선호 모니터 선택 정책
- `GlobalInputServiceTests`: HookManager adapter 시작/중지, 글로벌 단축키 라우팅, 로컬 단축키 차단, 시작 실패 cleanup
- `CommandCatalogTests`: command id 중복 방지, 기본 shortcut의 command 참조 검증, 기본 shortcut 충돌 방지, Live 위험 명령 메타데이터 검증
- `MediaPlaybackServiceTests`: media request load, playback state 전이, seek/audio setting clamp, backend 명령 위임, load/play/pause/stop/seek/settings 실패 상태 전환 검증
- `MediaPlaybackViewModelTests`: play/pause, stop, seek, mute/repeat command와 시간/상태 표시 검증
- `PreviewCanvasTests`: WPF preview placement/render/DPI contract 검증
- `TransitionEffectServiceTests`: WPF transition effect list/action/frame contract 검증
- `OutputRendererTests`: 출력 scene snapshot, 표시 라벨, content placement, transition frame contract 검증
- `ThumbnailCacheTests`: 썸네일 cache key/invalidation/LRU contract 검증

현재 자동화 완료:

- `LiveSessionServiceTests`
- `OutputWindowServiceTests`
- `MainViewModelTests`
- `OutputWindowHostTests`
- `OutputWindowViewModelTests`
- `DisplayServiceTests`
- `GlobalInputServiceTests`
- `CommandCatalogTests`
- `WindowPlacementServiceTests`
- `PlatformDiagnosticsServiceTests`
- `MediaPlaybackServiceTests`
- `MediaPlaybackViewModelTests`
- `OutputRendererTests`
- `ThumbnailCacheTests`

2026-05-29 검증 결과:

- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "MediaPlaybackServiceTests|MediaPlaybackViewModelTests"`: 13개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug`: 145개 통과
- `dotnet test Easislides.sln -c Debug`: 145개 통과
- `dotnet build Easislides.sln -c Release`: 성공
- `dotnet test Easislides.sln -c Release --no-build`: 145개 통과
- CodeGraph 동기화 및 `GlobalInputService`, `CommandCatalog`, `WindowPlacementService`, `PlatformDiagnosticsService`, `IMediaPlaybackService`, `IMediaPlaybackBackend`, `NoOpMediaPlaybackBackend`, `WpfMediaElementPlaybackBackend`, `MediaPlaybackService`, `MediaPlaybackViewModel`, `IPowerPointRenderService`, `PowerPointRenderService`, `IThumbnailCache`, `ThumbnailCache`, `ThumbnailCacheTests`, `IImageAssetService`, `ImageAssetService`, `PreviewCanvas`, `PreviewCanvasTests`, `ITransitionEffectService`, `TransitionEffectService`, `TransitionEffectServiceTests`, `IOutputRenderer`, `OutputRenderer`, `OutputRendererTests`, `OfficePptSession.ExportSlideAsync` 인식 확인 완료
- Release 산출물 확인: `Easislides.Wpf\bin\Release\net10.0-windows\Easislides.Wpf.exe`, `MainWindow.baml`, `OutputWindow.baml`
- `gstack /qa`, `GSD verify-work`: 현재 작업 환경 PATH에 도구가 없어 실행 불가. 동일 요구사항은 xUnit/Release build/산출물 확인으로 대체 검증

수동 테스트:

1. WPF 앱 실행
2. 작업 폴더 선택
3. 예배 목록 열기
4. 항목 선택
5. 출력 창 열기
6. Go Live
7. Next/Prev 20회
8. Black/Hide 토글
9. 미디어 항목 재생/일시정지/정지
10. 라이브 중 삭제/이동 시도 후 취소/확인 각각 검증

회귀 테스트:

- 동일 시나리오를 WinForms v2.6.4와 WPF에서 수행하고 출력 결과를 비교한다.
- 프리뷰 이미지, 현재 항목 제목, 출력 화면 상태, 로그 메시지를 비교 항목으로 둔다.
