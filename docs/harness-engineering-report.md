# EasiSlides 하네스 엔지니어링 적용 보고서

작성일: 2026-06-11  
대상 저장소: `EasiSlides_v2.6.4`  
작성 목적: 교육용 분석 자료. 이 프로젝트가 어떤 하네스 엔지니어링 기법을 사용했고, 현재 어디에 적용되어 있는지 설명한다.

## 1. 결론 요약

이 프로젝트의 하네스 엔지니어링은 크게 두 층으로 나뉜다.

1. 개발 프로세스 하네스
   - OpenSpec으로 요구사항과 승인 범위를 고정한다.
   - CodeGraph로 변경 영향과 구조 증거를 먼저 확인한다.
   - Superpowers 방식의 TDD 규칙으로 실패 테스트를 먼저 만든다.
   - gstack 계열 리뷰와 QA 게이트를 완료 판단의 안전장치로 둔다.

2. 코드 및 테스트 하네스
   - WPF 포팅 프로젝트 `Easislides.Wpf`와 테스트 프로젝트 `Easislides.Wpf.Tests`에 집중되어 있다.
   - UI STA 스레드, 헤드리스 렌더링, 스크린샷 회귀, DI 서비스 검증, Office PowerPoint COM 격리, SQLite 임시 DB, 파일 시스템 임시 폴더, 레지스트리 샌드박스, 글로벌 입력 훅, 출력창 surface fake, media backend fake 등이 적용되어 있다.

현재 하네스의 핵심 특징은 "운영 중 실패하면 치명적인 영역을 직접 건드리지 않고, 인터페이스와 fixture를 통해 제어 가능한 작은 환경에서 검증한다"는 점이다. 예배 송출 프로그램 특성상 PowerPoint COM, 출력 모니터 좌표, 라이브 상태 전환, SQLite 데이터 변경, WPF UI 렌더링이 모두 위험 지점인데, WPF 포팅에서는 이 지점마다 테스트 seam이 비교적 촘촘하게 들어가 있다.

## 2. 확인 범위와 근거

확인한 주요 근거는 다음과 같다.

| 구분 | 확인 내용 |
| --- | --- |
| CodeGraph 상태 | 525개 파일, 29,517개 노드, 50,034개 엣지 색인 확인 |
| 테스트 프로젝트 구조 | `Easislides.Wpf.Tests` 아래 146개 C# 파일 확인 |
| xUnit 테스트 규모 | literal scan 기준 `[Fact]` 1,898개, `[Theory]` 103개 |
| production seam | `Easislides.Wpf`에서 `public interface I...` 58개 확인 |
| 테스트 더블 | test double 명명 패턴 기준 Fake/Stub/Recording/InMemory 계열 55개 확인 |
| fixture/harness | Fixture/Harness/Temp* 계열 클래스 36개 확인 |

주요 확인 명령은 `codegraph_status`, `codegraph_context`, `codegraph_explore`, `rg --stats`, `rg -n`이다. 이 문서는 분석 및 문서 추가 작업이므로 `dotnet test`는 실행하지 않았다.

## 3. 이 문서에서 말하는 하네스 엔지니어링

여기서 "하네스"는 테스트를 실행하는 단순 wrapper만 뜻하지 않는다. 이 프로젝트에서는 다음 네 가지 의미로 쓰는 것이 교육에 적합하다.

| 하네스 유형 | 의미 | 프로젝트 예시 |
| --- | --- | --- |
| 계약 하네스 | 변경 범위와 완료 조건을 문서로 고정 | `openspec/changes/*` |
| 구조 하네스 | 변경 전 영향 범위를 AST 그래프로 확인 | `.codegraph/codegraph.db`, `codegraph-impact.md` |
| 실행 하네스 | 위험한 외부 의존성을 fake, fixture, temp folder로 대체 | `Easislides.Wpf.Tests/*` |
| 회귀 하네스 | UI/렌더/접근성/DI drift를 자동 테스트로 잠금 | `VisualRenderHarness`, `ScreenshotBaseline`, XAML scanner tests |

핵심은 "실제 예배 송출 환경을 그대로 테스트하지 않고, 예배 송출에 필요한 결정적인 계약을 작게 잘라 반복 가능하게 만든다"는 점이다.

## 4. 개발 프로세스 하네스

### 4.1 SDD 운영 계약

위치:

- `AGENTS.md`
- `openspec/`
- `.codegraph/`
- `docs/wpf-migration/`

적용 방식:

- `AGENTS.md`는 OpenSpec, CodeGraph, Superpowers, gstack의 역할을 명시한다.
- production code 변경은 승인된 OpenSpec change를 전제로 한다.
- 공유 심볼, Interop, DB, 출력 좌표를 수정할 때는 CodeGraph impact가 필요하다.
- Phase는 `Goal`, `Scope`, `Tasks`, `DoD`, `Tests`, `Constraints`를 가져야 한다.
- 완료 보고는 빌드, 테스트, 리뷰, 수동 QA 같은 검증 증거를 요구한다.

교육 포인트:

- 하네스는 코드 안에만 있는 것이 아니라 개발 절차에도 존재한다.
- OpenSpec은 "무엇을 할 것인가"를 묶고, CodeGraph는 "어디가 영향을 받는가"를 묶고, 테스트는 "정말 작동하는가"를 묶는다.

### 4.2 OpenSpec 변경 단위 하네스

대표 위치:

- `openspec/changes/wpf-frmmain-1to1-operator-console-parity/`
- `openspec/changes/wpf-frmmain-pane-role-parity-recovery/`
- `openspec/changes/wpf-frmmain-functional-parity-recovery/`
- `openspec/changes/wpf-frmmain-shortcut-parity/`
- `openspec/changes/wpf-frmmain-visual-parity-correction/`

적용 방식:

- `proposal.md`는 변경 이유와 목표를 적는다.
- `design.md`는 설계와 non-goal을 적는다.
- `tasks.md`는 phase 단위 진행 상태를 관리한다.
- `codegraph-impact.md`는 구조 영향 분석을 보관한다.

대표 예:

- `wpf-frmmain-1to1-operator-console-parity/tasks.md`는 Phase 0부터 Phase 7까지 WPF FrmMain 1:1 포팅을 쪼개고, Phase 5부터 Phase 7은 아직 미완료로 남겨 둔다.
- 이 방식은 "다 했다고 착각하는 것"을 막는 하네스다.

### 4.3 CodeGraph 구조 증거 하네스

대표 위치:

- `.codegraph/codegraph.db`
- `openspec/changes/*/codegraph-impact.md`

현재 적용:

- `wpf-frmmain-1to1-operator-console-parity/codegraph-impact.md`는 Phase 0 문서화 전에 `FrmMain`, `MainWindow`, `MainViewModel` 영향 범위를 기록한다.
- `wpf-frmmain-pane-role-parity-recovery/codegraph-impact.md`는 pane role 복구가 `MainWindow`, `PraiseBookIndexViewModel`, `ImageLibraryViewModel` 중심임을 기록한다.
- `wpf-frmmain-visual-parity-correction/codegraph-impact.md`는 `MainWindow` XAML drift test 범위로 변경을 제한한다.

교육 포인트:

- grep으로 추측하지 않고 AST 기반 구조 그래프를 먼저 본다.
- 특히 이 저장소처럼 WinForms 레거시와 WPF 포팅이 공존하면 "같은 이름의 기능"이 여러 곳에 있으므로 구조 하네스가 중요하다.

## 5. 테스트 프로젝트 하네스 구조

### 5.1 테스트 프로젝트 기본 구성

위치:

- `Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj`

확인 내용:

- `TargetFramework`: `net10.0-windows`
- `UseWPF`: `true`
- 테스트 프레임워크: xUnit
- Assertion: FluentAssertions
- 참조: `Easislides.Wpf`, `Easislides.Core`

의미:

- WPF 테스트는 일반 .NET 테스트보다 까다롭다. STA, WPF Application, ResourceDictionary, Dispatcher, RenderTargetBitmap 같은 환경이 필요하다.
- 테스트 프로젝트가 `UseWPF=true`로 설정되어 있어 WPF 컨트롤과 렌더링을 테스트 대상으로 직접 다룰 수 있다.

### 5.2 테스트 디렉터리별 역할

| 디렉터리 | 하네스 적용 주제 |
| --- | --- |
| `Accessibility` | XAML 정적 스캐너, AutomationProperties 정책, LabeledBy 검사 |
| `Binding` | XAML binding mode drift 방지 |
| `Composites` | composite control의 UI 상태와 구조 검증 |
| `Controls` | WPF custom control 인스턴스화, 렌더링, automation peer |
| `Data` | SQLite Admin DB, migration, transaction, backup/rollback 검증 |
| `Input` | shortcut registry, global key source, dispatcher fake |
| `Library` | 찬양, 성경, PPT, 미디어, 가져오기/내보내기, 검색 사용 기록 |
| `Media` | playback backend fake, service state transition |
| `Platform` | display enumeration, diagnostics, window placement |
| `Rendering` | output renderer, thumbnail cache, preview canvas, screenshot regression |
| `Settings` | settings.json, legacy registry/file migration, rehearsal |
| `Shell` | MainViewModel, live session, output/preview window, worship list |
| `Startup` | app startup, demo flag, legacy launcher, output artifact |
| `Theme` | WPF Application fixture, theme resources, icons |

교육 포인트:

- 테스트를 "파일 하나당 테스트 하나"가 아니라 "위험 영역별 하네스"로 나눴다.
- `Shell/MainViewModelTests.cs`처럼 큰 테스트 파일도 존재하지만, 그 안에는 InMemory store, recording service, PowerPoint render stub 같은 여러 작은 하네스가 들어 있다.

## 6. WPF STA 및 UI 실행 하네스

### 6.1 `StaHelper`

위치:

- `Easislides.Wpf.Tests/StaHelper.cs`

현재 적용:

- `LiveRegionTests`
- `FolderComboBoxTests`
- `OutlinedTextBlockTests`
- `PreviewCanvasTests`
- `SlidePreviewControlTests`
- `OutputRenderParityTests`
- `ScreenshotRegressionTests`
- `AppServiceRegistrationTests`
- `PaletteItemContainerStyleTests`

역할:

- WPF UI 객체는 STA 스레드에서 만들어야 한다.
- 일반 xUnit 테스트 스레드에서 WPF 컨트롤을 만들면 Dispatcher, dependency property, automation peer, rendering 관련 오류가 날 수 있다.
- `StaHelper.RunOnSta`와 `RunOnStaAsync`는 테스트 코드를 별도 STA 스레드에서 실행하게 해 WPF 제약을 하네스로 감싼다.

교육 포인트:

- UI 테스트의 첫 번째 하네스는 "올바른 스레드"다.
- 테스트가 불안정해지는 원인을 fake 이전에 실행 환경에서 제거한다.

### 6.2 `WpfApplicationFixture`

위치:

- `Easislides.Wpf.Tests/Theme/WpfApplicationFixture.cs`

현재 적용:

- `ThemeServiceTests` 등 WPF Application resource가 필요한 테스트 collection

역할:

- `Application.Current`는 프로세스당 하나만 존재한다.
- ThemeService는 `Application.Current.Resources`에 의존한다.
- fixture는 Application을 한 번만 만들고, `Theme/EasiDS.xaml` ResourceDictionary를 병합한다.
- xUnit collection fixture로 직렬 실행을 강제해 Application 중복 생성 문제를 피한다.

교육 포인트:

- singleton 성격의 런타임 객체는 테스트마다 새로 만들면 안 된다.
- fixture는 "재사용"뿐 아니라 "실행 순서와 lifetime을 통제하는 장치"다.

## 7. 헤드리스 렌더링 및 스크린샷 회귀 하네스

### 7.1 `VisualRenderHarness`

위치:

- `Easislides.Wpf.Tests/Rendering/VisualRenderHarness.cs`

현재 적용:

- `ScreenshotRegressionTests`
- `OutputRenderParityTests`

역할:

- WPF visual을 실제 창 없이 `RenderTargetBitmap`으로 렌더한다.
- DPI를 96으로 고정한다.
- 픽셀 포맷을 `Pbgra32`로 고정한다.
- 요소의 `Measure`, `Arrange`, `UpdateLayout`을 명시적으로 호출해 결정적인 크기에서 렌더링한다.

교육 포인트:

- UI를 화면에 띄우지 않고도 시각 결과를 검증할 수 있다.
- 렌더링 하네스에서는 DPI, 크기, 픽셀 포맷, 레이아웃 pass를 고정해야 한다.

### 7.2 `ImageComparer`

위치:

- `Easislides.Wpf.Tests/Rendering/ImageComparer.cs`

현재 적용:

- `ScreenshotBaseline.AssertMatches`
- `ScreenshotRegressionTests`

역할:

- PNG를 디코딩해 픽셀 단위로 비교한다.
- channel tolerance와 differing percent를 둬 환경 차이를 일부 흡수한다.
- 치수 불일치와 픽셀 불일치를 구분한다.

교육 포인트:

- 스크린샷 테스트는 단순 byte compare보다 픽셀 비교와 허용오차가 실용적이다.
- 하지만 폰트 힌팅, 안티앨리어싱이 환경별로 달라질 수 있어 텍스트가 많은 화면에는 신중해야 한다.

### 7.3 `ScreenshotBaseline`

위치:

- `Easislides.Wpf.Tests/Rendering/ScreenshotBaseline.cs`

현재 적용:

- `ScreenshotRegressionTests`
- `OutputRenderParityTests`

역할:

- 기준 이미지는 `Easislides.Wpf.Tests/Rendering/baselines/`에 둔다.
- 실제 결과가 다르면 테스트 출력 폴더의 `screenshot-actuals`에 `*.actual.png`를 남긴다.
- 기준 이미지가 없을 때는 `EASISLIDES_APPROVE_BASELINES=1`인 경우에만 생성하고, 그래도 테스트는 실패시켜 사람이 검토하게 한다.

교육 포인트:

- approval test는 "자동 통과"가 아니라 "사람이 승인할 artifact를 만드는 하네스"다.
- 기준 이미지와 진단 이미지를 분리해 소스 트리를 더럽히지 않는다.

## 8. DI 및 서비스 경계 하네스

### 8.1 인터페이스 기반 seam

대표 위치:

- `Easislides.Wpf/App.xaml.cs`
- `Easislides.Wpf/Rendering/PowerPointRenderService.cs`
- `Easislides.Wpf/Media/MediaPlaybackService.cs`
- `Easislides.Wpf/Input/GlobalInputService.cs`
- `Easislides.Wpf/Data/AdminDatabaseRepository.cs`
- `Easislides.Wpf/Library/BibleRepository.cs`
- `Easislides.Wpf/Shell/OutputWindowHost.cs`

현재 확인된 production interface:

- literal scan 기준 `Easislides.Wpf`에 `public interface I...` 58개
- 예: `IPowerPointRenderService`, `IPowerPointRenderBackend`, `IMediaPlaybackBackend`, `IGlobalKeySource`, `IGlobalInputDispatcher`, `IOutputWindowService`, `IOutputSurface`, `IBibleRepository`, `IAdminDatabaseRepository`, `ISettingsService`

역할:

- Office, Media, global hook, DB, settings, output window처럼 테스트에서 직접 다루기 어려운 의존성을 인터페이스 뒤로 숨긴다.
- 테스트에서는 fake, stub, recording implementation을 주입한다.

교육 포인트:

- 좋은 하네스는 test project에서 억지로 private 상태를 만지는 것이 아니라 production code의 경계를 작게 설계한다.
- 이 프로젝트는 "service interface + real implementation + test double" 패턴이 WPF 쪽에 넓게 적용되어 있다.

### 8.2 `App.ConfigureServices` 등록 검증

위치:

- production: `Easislides.Wpf/App.xaml.cs`
- test: `Easislides.Wpf.Tests/AppServiceRegistrationTests.cs`

현재 적용:

- `ISettingsService`
- `ILegacySettingsSource`
- `IPowerPointRenderService`
- `IPowerPointSlideShowControl`
- `IMediaPlaybackService`
- `IWindowPlacementService`
- `IAdminDatabaseRepository`
- `IAdminSongDetailRepository`
- `IExternalFileOperationService`
- `IBibleRepository`
- `IOutputWindowHost`
- `IPreviewWindowService`
- `PreviewWindowHost`
- 각종 ViewModel과 Window

역할:

- DI 등록 누락을 실행 시점이 아니라 테스트에서 잡는다.
- singleton이어야 하는 서비스가 transient로 바뀌는 drift도 잡는다.
- `PreviewWindowHost`가 `ILiveSessionService`와 `IOutputRenderer` singleton을 공유하는지 reflection으로 확인한다.

교육 포인트:

- DI는 "편하게 new 안 하려고" 쓰는 것이 아니라 "운영 wiring을 테스트 가능한 계약으로 만드는 장치"다.

## 9. Office PowerPoint COM 하네스

### 9.1 `OfficePptSession`

위치:

- `Easislides.Wpf/Interop/OfficePptSession.cs`

역할:

- PowerPoint COM은 STA affinity가 중요하다.
- `OfficePptSession`은 별도 STA worker thread를 만들고, 작업 queue로 COM 호출을 직렬 실행한다.
- `OpenAsync`, `PingAsync`, `ExportSlideAsync`, `TriggerSlideShowNextAsync`, `CloseAsync`를 제공한다.
- export 시 임시 PNG 파일을 만들고 finally에서 presentation close/dispose와 temp file delete를 수행한다.

교육 포인트:

- COM은 "테스트하기 어려운 외부 시스템"의 대표 사례다.
- 이 프로젝트는 COM 호출을 직접 ViewModel에서 하지 않고 session/service/backend 뒤로 격리한다.

### 9.2 `PowerPointRenderService`와 backend seam

위치:

- production: `Easislides.Wpf/Rendering/PowerPointRenderService.cs`
- tests: `Easislides.Wpf.Tests/Rendering/PowerPointRenderServiceTests.cs`
- shell tests: `Easislides.Wpf.Tests/Shell/MainViewModelTests.cs`

현재 적용:

- `IPowerPointRenderService`
- `IPowerPointRenderBackend`
- `OfficePowerPointRenderBackend`
- `FakePowerPointRenderBackend`
- `StubPowerPointRenderService`
- `SuccessPowerPointRenderService`
- `FixedSlideCountPowerPointRenderService`
- `GatedPowerPointRenderService`
- `RecordingPowerPointRenderService`

검증하는 것:

- 파일 timestamp 기반 cache hit
- 파일 변경 시 cache invalidation
- pixel size가 다르면 별도 cache entry
- timeout/cancellation/error classification
- settings 기반 thumbnail cache option
- PowerPoint slide count와 slide snapshot 전달
- MainViewModel의 preview/output 독립 PowerPoint 상태
- slide show next request 기록

교육 포인트:

- 실제 PowerPoint 설치 여부에 기대지 않고도 대부분의 render orchestration을 검증한다.
- "real backend는 얇게, service는 fake backend로 두껍게 테스트"하는 구조다.

## 10. 출력 및 Preview window 하네스

### 10.1 `IOutputSurface`와 surface factory

위치:

- production: `Easislides.Wpf/Shell/OutputWindowHost.cs`
- production: `Easislides.Wpf/Shell/PreviewWindowHost.cs`
- tests: `Easislides.Wpf.Tests/Shell/OutputWindowHostTests.cs`
- tests: `Easislides.Wpf.Tests/Shell/PreviewWindowHostTests.cs`

현재 적용:

- `OutputSurfaceFactory`
- `IOutputSurface`
- `FakeOutputSurface`

검증하는 것:

- 출력 open 시 surface 생성
- monitor placement 적용
- live session snapshot이 ViewModel에 반영
- close 시 surface close 및 ViewModel dispose
- reopen 시 surface 재생성
- dispose 시 이벤트 구독 해제
- preview monitor도 같은 패턴으로 stage surface를 생성/이동/닫기

교육 포인트:

- 실제 WPF Window를 띄우지 않고도 출력창 host의 lifecycle을 검증한다.
- 멀티 모니터/출력창 같은 외부 UI 의존성은 `surface`라는 작은 인터페이스로 줄일 수 있다.

### 10.2 출력 렌더러와 ViewModel 하네스

위치:

- production: `Easislides.Wpf/Rendering/OutputRenderer.cs`
- production: `Easislides.Wpf/Shell/OutputWindowViewModel.cs`
- tests: `Easislides.Wpf.Tests/Rendering/OutputRendererTests.cs`
- tests: `Easislides.Wpf.Tests/Shell/OutputWindowViewModelTests.cs`
- tests: `Easislides.Wpf.Tests/Shell/OutputWindowTileClipTests.cs`
- tests: `Easislides.Wpf.Tests/Shell/OutputWindowClipGuardTests.cs`

검증하는 것:

- live/hidden/blackout/standby 상태
- lyrics monitor 색상, alert, notation visibility
- image/background placement
- content asset pixel size와 fill mode
- output tile clipping
- output scene snapshot
- transition frame contract

교육 포인트:

- 실제 송출 창이 없어도 "송출될 장면의 데이터 계약"을 테스트한다.
- 시각적 결과는 일부 screenshot regression으로, 상태와 배치는 ViewModel/renderer 단위 테스트로 나눠 검증한다.

## 11. Media playback 하네스

위치:

- production: `Easislides.Wpf/Media/MediaPlaybackBackend.cs`
- production: `Easislides.Wpf/Media/MediaPlaybackService.cs`
- tests: `Easislides.Wpf.Tests/Media/MediaPlaybackServiceTests.cs`
- tests: `Easislides.Wpf.Tests/Media/AttachableMediaPlaybackBackendTests.cs`
- tests: `Easislides.Wpf.Tests/Media/MediaPlaybackViewModelTests.cs`

현재 적용:

- `IMediaPlaybackBackend`
- `IMediaPlaybackService`
- `FakeMediaPlaybackBackend`
- `RecordingBackend`

검증하는 것:

- media load/play/pause/stop/seek
- mute, repeat, volume, balance clamp
- backend command delegation
- unload failure 처리
- settings 기반 audio default/runtime 변경 반영
- ViewModel command와 표시 상태

교육 포인트:

- 미디어 재생은 실제 codec, 장치, WPF MediaElement 상태에 좌우되기 쉽다.
- service layer에서 playback state machine을 fake backend로 검증하면 테스트가 안정된다.

## 12. 글로벌 입력 및 단축키 하네스

위치:

- production: `Easislides.Wpf/Input/GlobalInputService.cs`
- production: `Easislides.Wpf/Input/ShortcutRegistry.cs`
- production: `Easislides.Wpf/Input/CommandCatalog.cs`
- tests: `Easislides.Wpf.Tests/Input/GlobalInputServiceTests.cs`
- tests: `Easislides.Wpf.Tests/Input/ShortcutRegistryTests.cs`
- tests: `Easislides.Wpf.Tests/Input/CommandCatalogTests.cs`
- docs: `docs/wpf-migration/inventory/frmmain-shortcut-parity-map.md`

현재 적용:

- `IGlobalKeySource`
- `IGlobalInputDispatcher`
- `FakeGlobalKeySource`
- `RecordingGlobalInputDispatcher`

검증하는 것:

- global hook adapter start/stop
- key event를 registry로 routing
- local/global shortcut 충돌 방지
- command id 중복 방지
- FrmMain live operation key parity
- menu hint drift 방지

교육 포인트:

- 키보드 입력은 운영자의 muscle memory와 연결된다.
- 이 프로젝트는 shortcut을 문서 map, command catalog, registry test, menu hint test로 여러 겹 잠근다.

## 13. SQLite, 파일 시스템, 설정 migration 하네스

### 13.1 임시 SQLite DB fixture

대표 위치:

- `Easislides.Wpf.Tests/Data/AdminDatabaseRepositoryTests.cs`
- `Easislides.Wpf.Tests/Data/DatabaseMigrationServiceTests.cs`
- `Easislides.Wpf.Tests/Library/BibleRepositoryTests.cs`
- `Easislides.Wpf.Tests/Settings/OperationalDataRehearsalServiceTests.cs`

현재 적용:

- `AdminDatabaseFixture`
- `TempDatabaseFolder`
- `BibleDatabaseFixture`
- `OperationalDataFixture`

검증하는 것:

- legacy AdminDB schema/table/column 호환
- folder/song read/write
- soft delete/recover
- song move transaction rollback
- backup restore
- Bible list DB와 Bible content DB load/search
- corrupt DB와 missing DB 처리

교육 포인트:

- DB 하네스는 mock만으로 부족하다.
- 이 프로젝트는 실제 SQLite file을 임시 폴더에 만들고, 실제 SQL path를 검증한다.
- transaction/backup/rollback처럼 운영 데이터 안전성과 직결된 부분은 fake repository보다 실제 DB fixture가 교육 가치가 높다.

### 13.2 파일 시스템 temp folder 하네스

대표 위치:

- `Easislides.Wpf.Tests/Library/ImportExportServiceTests.cs`
- `Easislides.Wpf.Tests/Library/ExternalFileOperationServiceTests.cs`
- `Easislides.Wpf.Tests/Settings/AssetMigrationServiceTests.cs`
- `Easislides.Wpf.Tests/Settings/SettingsServiceTests.cs`
- `Easislides.Wpf.Tests/Shell/WorshipListStoreTests.cs`
- `Easislides.Wpf.Tests/Rendering/ThumbnailCacheTests.cs`

현재 적용:

- `Path.GetTempPath()`와 `Guid.NewGuid()`로 격리된 폴더 생성
- `IDisposable` fixture로 cleanup
- copy/move conflict 파일명 검증
- settings import/export
- worship list JSON save/load/corruption handling
- thumbnail cache file invalidation

교육 포인트:

- 파일 시스템은 fake로 지나치게 단순화하면 path, encoding, collision, cleanup 버그를 놓친다.
- temp folder fixture는 실제 파일 동작을 재현하면서도 사용자 데이터를 건드리지 않는다.

### 13.3 레지스트리 및 legacy settings 하네스

위치:

- production: `Easislides.Wpf/Settings/SettingsService.cs`
- production: `Easislides.Wpf/Settings/RegistryLegacySettingsSource.cs`
- production: `Easislides.Wpf/Settings/FileLegacySettingsSource.cs`
- tests: `Easislides.Wpf.Tests/Settings/RegistryLegacySettingsSourceTests.cs`
- tests: `Easislides.Wpf.Tests/Settings/FileLegacySettingsSourceTests.cs`
- tests: `Easislides.Wpf.Tests/Settings/LegacySettingsMapTests.cs`
- tests: `Easislides.Wpf.Tests/Settings/SettingsBootstrapMigrationServiceTests.cs`

현재 적용:

- `RegistryFixture`
- `TempSettingsFolder`
- `CompositeLegacySettingsSource`

검증하는 것:

- legacy registry key read
- missing registry value가 새 key를 만들지 않는지
- INI/key-value/JSON/user.config/appSettings XML parse
- registry 우선, file fallback
- WPF 최초 실행 settings bootstrap migration
- 기존 settings.json 보존

교육 포인트:

- migration 하네스는 "깨끗한 새 설치"와 "기존 사용자 데이터"를 모두 재현해야 한다.
- 이 프로젝트는 WPF 기본 경로와 legacy `C:\EasiSlides` 경로 차이를 OpenSpec과 tests로 다룬다.

## 14. 접근성 및 XAML 정적 분석 하네스

위치:

- `Easislides.Wpf.Tests/Accessibility/XamlAccessibilityScanner.cs`
- `Easislides.Wpf.Tests/Accessibility/LabeledByScanner.cs`
- `Easislides.Wpf.Tests/Accessibility/XamlAccessibilityTests.cs`
- `Easislides.Wpf.Tests/Accessibility/LabeledByTests.cs`
- `Easislides.Wpf.Tests/Accessibility/TemplatePartsTests.cs`
- `Easislides.Wpf.Tests/Accessibility/LiveRegionTests.cs`
- `Easislides.Wpf.Tests/Binding/XamlBindingModeTests.cs`

현재 적용:

- production XAML files를 찾아 스캔한다.
- `bin`, `obj`, `Demo`, `Poc`, `App.xaml`은 제외한다.
- interactive control의 accessible name을 검사한다.
- `AutomationProperties.LabeledBy`가 깨진 ElementName을 참조하지 않는지 검사한다.
- template part는 `PART_*`나 `Focusable=False`일 때만 면제한다.
- binding mode drift를 별도 검사한다.

교육 포인트:

- 접근성은 사람이 눈으로 매번 보기 어렵다.
- XAML scanner는 완벽한 컴파일러가 아니라 "반복 가능한 정책 하네스"다.
- false positive를 줄이기 위해 제외 규칙과 heuristic을 문서화한 점이 중요하다.

## 15. ViewModel 및 live workflow 하네스

### 15.1 `MainViewModelTests`의 테스트 더블 묶음

위치:

- `Easislides.Wpf.Tests/Shell/MainViewModelTests.cs`

현재 적용:

- `InMemoryWorshipListStore`
- `InMemoryAppearanceTemplateStore`
- `InMemoryRecentWorshipLists`
- `RecordingPowerPointSlideShowControl`
- `StubPowerPointRenderService`
- `SuccessPowerPointRenderService`
- `FixedSlideCountPowerPointRenderService`
- `GatedPowerPointRenderService`
- `RecordingPowerPointRenderService`
- `RecordingSafetyPrompt`
- `RecordingSearchUsageService`
- `StubSongDetailRepository`

검증하는 것:

- Worship List 추가/삭제/이동/저장
- Preview와 Output의 독립 상태
- PowerPoint slide 이동, replay, rerender
- live/black/hidden/restore 상태
- output item navigation
- 안전 확인 prompt 승인/취소
- 최근 예배 순서와 appearance template 저장
- Bible/song/detail lookup

교육 포인트:

- 거대한 운영 ViewModel은 실제 Window 없이도 거의 모든 operator workflow를 재현할 수 있다.
- 단, 테스트 파일이 커질수록 공통 fixture 추출과 naming consistency가 중요해진다.

### 15.2 Live session/output state 하네스

위치:

- production: `Easislides.Wpf/Shell/LiveSessionService.cs`
- production: `Easislides.Wpf/Shell/OutputWindowService.cs`
- production: `Easislides.Wpf/Shell/PreviewWindowService.cs`
- tests: `LiveSessionServiceTests`
- tests: `OutputWindowServiceTests`
- tests: `PreviewWindowServiceTests`
- tests: `StageSessionNormalizerTests`

검증하는 것:

- live item publish
- hidden/blackout/standby 상태
- output monitor open/move/close
- preview stage monitor open/move/close
- stage session normalization

교육 포인트:

- live state는 실제 화면보다 먼저 순수 service state로 모델링해야 한다.
- 이 상태 모델이 있어야 출력 Window와 operator UI를 분리해 테스트할 수 있다.

## 16. Legacy FrmMain parity 하네스

위치:

- `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md`
- `docs/wpf-migration/inventory/frmmain-shortcut-parity-map.md`
- `docs/wpf-migration/frmmain-1to1-ui-ux-function-mapping-plan.md`
- `Easislides.Wpf.Tests/Composites/WorshipListPanelTests.cs`
- `Easislides.Wpf.Tests/Shell/MainMenuBarTests.cs`
- `Easislides.Wpf.Tests/Shell/MainWindowCopyTests.cs`
- `Easislides.Wpf.Tests/Startup/AppStartupTests.cs`

현재 적용:

- legacy control name과 WPF target control을 mapping table로 기록한다.
- `FrmMain`의 source tabs, preview/output panes, shortcut, menu hint를 WPF XAML/test가 따라가게 한다.
- XAML text scan으로 "Preview/PowerPoint/Media tab header가 first-screen에 잘못 노출되는지" 같은 drift를 잡는다.
- `MainMenuBarTests`는 output menu gesture와 command id/hint drift를 막는다.

교육 포인트:

- 레거시 포팅에서 하네스는 "새 코드가 예쁘냐"보다 "기존 operator workflow를 배신하지 않느냐"를 검증해야 한다.
- 문서 inventory와 테스트를 같이 두면 parity 기준이 살아 있는 계약이 된다.

## 17. Startup 및 demo 하네스

위치:

- production: `Easislides.Wpf/App.xaml.cs`
- production: `Easislides.Wpf/Startup/StartupArguments.cs`
- tests: `Easislides.Wpf.Tests/Startup/StartupArgumentsTests.cs`
- tests: `Easislides.Wpf.Tests/Startup/AppStartupTests.cs`
- tests: `Easislides.Wpf.Tests/AppServiceRegistrationTests.cs`

현재 적용:

- `--demo` flag로 `DemoWindow`와 `MainWindow`를 분기한다.
- startup 초기에 `ShutdownMode.OnExplicitShutdown`을 사용하고, MainWindow 할당 후 `OnMainWindowClose`로 전환하는 순서를 테스트한다.
- output/preview host를 startup에서 resolve해 host가 이벤트를 구독하게 한다.

교육 포인트:

- WPF startup은 transient window가 먼저 생성/닫힐 수 있어 shutdown mode가 중요하다.
- startup test는 실제 UI launch를 하지 않고도 lifetime contract를 고정한다.

## 18. 교육용 핵심 사례 6개

### 사례 1: WPF 컨트롤을 안정적으로 테스트하기

추천 파일:

- `Easislides.Wpf.Tests/StaHelper.cs`
- `Easislides.Wpf.Tests/Rendering/SlidePreviewControlTests.cs`
- `Easislides.Wpf.Tests/Rendering/PreviewCanvasTests.cs`

강의 흐름:

1. WPF는 STA가 필요하다는 문제 제기
2. `StaHelper.RunOnSta`로 실행 환경 고정
3. control instance 생성
4. `Measure/Arrange/UpdateLayout`
5. dependency property와 visibility/assertion 검증

### 사례 2: 스크린샷 회귀 하네스 만들기

추천 파일:

- `VisualRenderHarness.cs`
- `ImageComparer.cs`
- `ScreenshotBaseline.cs`
- `ScreenshotRegressionTests.cs`

강의 흐름:

1. Headless render
2. PNG byte 생성
3. baseline 저장 정책
4. approval env var
5. tolerance와 actual output artifact

### 사례 3: Office COM을 서비스 뒤로 격리하기

추천 파일:

- `OfficePptSession.cs`
- `PowerPointRenderService.cs`
- `PowerPointRenderServiceTests.cs`

강의 흐름:

1. COM STA 문제
2. session worker thread
3. backend interface
4. fake backend로 cache/error/timeout 검증
5. real Office 의존성은 얇은 adapter로 제한

### 사례 4: 출력창을 실제로 띄우지 않고 lifecycle 검증하기

추천 파일:

- `OutputWindowHost.cs`
- `PreviewWindowHost.cs`
- `OutputWindowHostTests.cs`
- `PreviewWindowHostTests.cs`

강의 흐름:

1. Window 대신 `IOutputSurface`
2. factory injection
3. fake surface에 Bind, ApplyPlacement, Show, Close 기록
4. output state change로 surface lifecycle 검증

### 사례 5: SQLite migration을 실제 파일 fixture로 검증하기

추천 파일:

- `AdminDatabaseRepositoryTests.cs`
- `BibleRepositoryTests.cs`
- `DatabaseMigrationServiceTests.cs`

강의 흐름:

1. mock DB의 한계
2. temp folder와 실제 SQLiteConnection
3. schema 생성
4. transaction rollback과 backup restore
5. legacy DB 호환성 검증

### 사례 6: 레거시 포팅에서 parity를 하네스로 고정하기

추천 파일:

- `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md`
- `docs/wpf-migration/inventory/frmmain-shortcut-parity-map.md`
- `MainMenuBarTests.cs`
- `CommandCatalogTests.cs`
- `WorshipListPanelTests.cs`

강의 흐름:

1. legacy workflow가 contract임을 선언
2. inventory 문서화
3. command id와 shortcut map 테스트
4. XAML drift 검사
5. OpenSpec tasks와 manual UAT checklist 연결

## 19. 현재 하네스의 강점

1. 위험 외부 의존성이 잘 격리되어 있다.
   - Office COM, global hook, media backend, output window, display reader가 interface 뒤에 있다.

2. WPF 특유의 불안정성을 인식하고 있다.
   - STA helper, WPF Application fixture, ResourceDictionary fixture, RenderTargetBitmap 하네스가 있다.

3. 데이터 안전성 검증이 mock에만 의존하지 않는다.
   - Admin DB, Bible DB, migration은 실제 SQLite file fixture로 검증한다.

4. 문서와 테스트가 같이 parity를 잠근다.
   - `docs/wpf-migration/inventory/*`와 테스트가 서로 보완한다.

5. approval-style screenshot regression이 있다.
   - 기준 이미지 자동 생성이 무조건 통과하지 않도록 실패시키는 정책이 좋다.

6. DI registration test가 넓다.
   - 운영 wiring drift를 조기에 잡을 수 있다.

## 20. 현재 한계와 보강 제안

### 20.1 테스트 더블이 여러 파일에 중첩되어 있다

현재:

- fake/stub/recording class가 테스트 파일 내부 private sealed class로 많이 존재한다.

장점:

- 테스트 의도가 가까운 곳에 있어 읽기 쉽다.
- 불필요한 공유 abstraction이 적다.

한계:

- `TempSettingsFolder`, fake repository, fake service 패턴이 반복된다.
- 교육 자료에서는 같은 이름의 하네스가 여러 변형으로 보일 수 있다.

보강 제안:

- 교육용으로 `docs/testing-harness-patterns.md` 같은 "패턴 사전"을 만들면 좋다.
- 공용화는 신중히 하되, 반복되는 temp settings fixture 정도는 shared test utility 후보로 볼 수 있다.

### 20.2 스크린샷 회귀 범위가 아직 제한적이다

현재:

- color token swatch, output background, 일부 deterministic surface 중심이다.

한계:

- 실제 전체 MainWindow, 가사 텍스트, 폰트 렌더링, 다국어 조합은 픽셀 비교가 불안정할 수 있다.

보강 제안:

- 텍스트 없는 layout/token 회귀는 screenshot으로 확대한다.
- 텍스트와 레이아웃 의미는 ViewModel property, AutomationProperties, XAML scanner로 계속 분리 검증한다.

### 20.3 실제 Office 통합 테스트는 제한적이다

현재:

- PowerPoint orchestration은 fake backend로 잘 검증한다.
- `OfficePptSession` 실제 Office 경로는 환경 의존성이 커 자동화 범위가 좁다.

보강 제안:

- Office 설치 환경이 있는 전용 machine에서 opt-in integration test profile을 분리한다.
- 기본 CI에서는 fake backend 단위 테스트를 유지한다.

### 20.4 WinForms legacy 쪽은 하네스가 상대적으로 약하다

현재:

- legacy `FrmMain`은 behavior oracle이자 reference로 쓰인다.
- 자동화 하네스는 WPF 포팅 쪽에 더 집중되어 있다.

보강 제안:

- legacy behavior를 직접 수정하는 경우에는 최소한 characterization tests 또는 문서화된 manual UAT checklist를 더 강하게 연결해야 한다.

### 20.5 gstack/수동 QA 증거는 환경 의존적이다

현재:

- 문서와 AGENTS 정책에는 gstack review/cso/qa/ship gate가 명시되어 있다.
- 일부 마이그레이션 문서에는 환경 미설치로 대체 검증했다는 기록이 있다.

보강 제안:

- 교육에서는 "자동 테스트 green"과 "수동 송출 QA"를 분리해 설명해야 한다.
- 특히 멀티 모니터, 실제 PowerPoint, 실제 예배 데이터는 별도 UAT checklist가 필요하다.

## 21. 교육 진행 추천 순서

1. 프로젝트 위험 모델 설명
   - 예배 송출 안정성
   - Office COM
   - SQLite 데이터
   - 멀티 모니터 출력
   - WPF STA/UI

2. SDD 프로세스 하네스 설명
   - OpenSpec
   - CodeGraph
   - TDD
   - gate

3. DI seam 둘러보기
   - `App.xaml.cs`
   - `AppServiceRegistrationTests`

4. UI 하네스 실습
   - `StaHelper`
   - `VisualRenderHarness`
   - `ScreenshotBaseline`

5. 외부 의존성 하네스 실습
   - PowerPoint fake backend
   - Media fake backend
   - Global input fake source

6. 데이터 하네스 실습
   - SQLite temp fixture
   - file temp folder
   - settings/registry migration

7. 레거시 parity 하네스 실습
   - mapping inventory
   - shortcut parity
   - XAML drift tests

8. 한계와 개선 토론
   - 어떤 하네스는 공용화할 것인가
   - 어떤 것은 테스트 파일 내부에 둘 것인가
   - 픽셀 테스트와 의미 테스트를 어디서 나눌 것인가

## 22. 적용 위치 빠른 참조표

| 하네스 기법 | 주요 파일 | 현재 적용 대상 |
| --- | --- | --- |
| OpenSpec contract | `openspec/changes/*` | WPF FrmMain parity, shortcut parity, visual parity |
| CodeGraph impact | `openspec/changes/*/codegraph-impact.md` | MainWindow, MainViewModel, FrmMain reference 영향 분석 |
| STA 실행 | `Easislides.Wpf.Tests/StaHelper.cs` | WPF control, automation peer, render tests |
| WPF Application fixture | `Theme/WpfApplicationFixture.cs` | theme resource, Application.Current 의존 테스트 |
| Headless render | `Rendering/VisualRenderHarness.cs` | screenshot regression, output render parity |
| Image compare | `Rendering/ImageComparer.cs` | PNG pixel tolerance 비교 |
| Approval baseline | `Rendering/ScreenshotBaseline.cs` | source baseline과 actual diagnostic 분리 |
| DI registration guard | `AppServiceRegistrationTests.cs` | production service wiring drift 방지 |
| PowerPoint COM seam | `OfficePptSession.cs`, `PowerPointRenderService.cs` | PPT export, slide show, cache, timeout |
| PPT fake backend | `PowerPointRenderServiceTests.cs`, `MainViewModelTests.cs` | Office 없이 PPT workflow 검증 |
| Output surface fake | `OutputWindowHostTests.cs`, `PreviewWindowHostTests.cs` | Window 없이 output/preview lifecycle 검증 |
| Media backend fake | `MediaPlaybackServiceTests.cs` | media state machine 검증 |
| Global key fake | `GlobalInputServiceTests.cs` | HookManager 없이 shortcut routing 검증 |
| SQLite fixture | `AdminDatabaseRepositoryTests.cs`, `BibleRepositoryTests.cs` | legacy DB 호환, transaction, backup |
| File temp fixture | `ImportExportServiceTests.cs`, `SettingsServiceTests.cs` | import/export, settings, worship list |
| Registry fixture | `RegistryLegacySettingsSourceTests.cs` | legacy registry migration |
| XAML scanner | `Accessibility/*Scanner.cs` | accessible name, LabeledBy, template policy |
| Shortcut parity map | `docs/wpf-migration/inventory/frmmain-shortcut-parity-map.md` | operator key drift 방지 |
| FrmMain mapping | `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md` | legacy workflow parity |

## 23. 최종 판단

EasiSlides WPF 포팅은 하네스 엔지니어링 교육에 좋은 사례다. 이유는 단순히 테스트가 많아서가 아니라, 위험한 운영 환경을 작은 계약으로 나누는 방식이 여러 층에 나타나기 때문이다.

가장 강한 교육 메시지는 다음이다.

> 예배 송출 안정성을 지키려면 "실제 환경을 매번 띄워 본다"만으로는 부족하다. Office, DB, 출력창, WPF UI, 단축키, 레거시 parity를 각각 작게 격리하고, 그 격리된 환경을 반복 실행 가능한 하네스로 만들어야 한다.

현재 이 원칙은 특히 `Easislides.Wpf`와 `Easislides.Wpf.Tests`에 잘 적용되어 있다. 반대로 legacy WinForms 영역은 주로 reference oracle로 남아 있어, 실제 legacy production code를 수정할 때는 OpenSpec, CodeGraph impact, manual UAT checklist, focused characterization test를 더 강하게 붙이는 것이 좋다.
