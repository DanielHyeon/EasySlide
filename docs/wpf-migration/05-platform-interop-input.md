# 플랫폼 연동, 입력, 멀티모니터 이식 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: 키보드/리모컨 우선, 멀티모니터 마찰, HookManager 보존
- `docs/adr/0001-wpf-ui-framework.md`: WPF input routing
- `docs/adr/0004-hookmanager-preservation.md`: HookManager + WPF `PreviewKeyDown` 협력
- `docs/adr/0007-legacy-ui-safety-net.md`: platform 이식 중 legacy fallback
- `docs/ui/icon-migration-map.md`: `Icon.Monitor.*`, `Icon.Shortcuts`

## 1. 범위

이 문서는 WPF 이식 중 Windows 플랫폼 연동, 전역 입력, 멀티모니터, 팝업/창 관리, 유틸리티 계층을 다룬다.

대상 legacy 파일:

| Legacy | 역할 | WPF 목표 |
|---|---|---|
| `HookManager/*` | 글로벌 키보드/마우스 훅 | 보존 후 adapter화 |
| `KeyboardActionHandler.cs`, `KeyboardMapping.cs` | 키보드 명령 매핑 | `ShortcutRegistry`와 통합 |
| `PopupWindowHelper.cs`, `PopupWindowHelperMessageFilter.cs` | 팝업/외부 클릭 처리 | WPF dialog/toast/window service |
| `gfDisplay.cs`, `Util/DisplayInfo.cs`, `Util/DiNative.cs` | 모니터/디스플레이 | `IDisplayService` |
| `Util/RegUtil.cs`, `OfficeVersion.cs`, `CommonUtil.cs`, `FileUtil.cs` | Windows/Office/파일 유틸 | platform service |
| `DShowLib.cs`, `DirectShow/` | DirectShow interop | media backend adapter |

현재 WPF 기반:

- `Shortcut.cs`
- `ShortcutRegistry.cs`
- `GlobalInputService.cs`
- `Poc/PocAHookTest.xaml`

## 2. 입력 이식 원칙

1. 로컬 WPF 단축키와 글로벌 HookManager 단축키는 같은 command id를 사용한다.
2. 라이브 명령은 전역 키로 실행되더라도 LiveBar와 로그가 즉시 갱신되어야 한다.
3. 사용자가 단축키 충돌을 설정 화면에서 확인할 수 있어야 한다.
4. 전역 훅 실패 시 앱은 로컬 단축키 모드로 degradation해야 한다.
5. 리모컨 입력은 가능한 한 키보드 단축키로 추상화한다.

## 3. 목표 아키텍처

| 컴포넌트 | 책임 |
|---|---|
| `IShortcutService` | shortcut 등록, 충돌 검사, 사용자 설정 반영 |
| `IGlobalInputService` | HookManager 연결/해제, global key event 변환 |
| `ICommandRouter` | command id를 ViewModel/service 명령으로 라우팅 |
| `IDisplayService` | 모니터 목록, DPI, 좌표, primary/output screen |
| `IWindowPlacementService` | 출력/팝업/대화상자 위치 |
| `IPlatformDiagnosticsService` | hook, monitor, registry, command catalog 진단 |

## 4. 이식 단계

### 4.0 ADR 준수 체크

- 글로벌 키는 HookManager adapter를 통해 유지하고, WPF local key는 같은 command id를 사용한다.
- WPF `InputBinding`만으로 글로벌 리모컨 동작을 대체하지 않는다.
- 멀티모니터/단축키 설정 UI는 Fluent 아이콘과 EasiDS 토큰을 사용한다.
- platform adapter 교체 전에는 WinForms 실행 경로와 기존 HookManager 동작을 유지한다.

### 4.1 HookManager adapter

- 기존 `HookManager` 파일은 초기에는 file-link 방식 유지.
- WPF 전용 `GlobalInputService`에서만 HookManager를 참조한다.
- event handler는 UI dispatcher로 marshal한다.
- dispose/unsubscribe를 명시한다.
- hook install 실패를 사용자에게 알려준다.

완료 조건:

- 앱 종료 후 hook이 남지 않는다.
- 다른 앱 포커스 상태에서도 허용된 global shortcut만 처리한다.
- 텍스트 입력 중 불필요하게 키가 먹히지 않는다.

### 4.2 Shortcut 통합

- legacy `KeyboardMapping`과 WPF `Shortcut`를 하나의 command catalog로 합친다.
- command id naming 규칙을 정의한다. 예: `Live.Next`, `Live.Black`, `Library.Search`.
- 설정 화면에서 command, key, scope, description을 표시한다.
- 중복 shortcut 등록은 저장 전에 막는다.

### 4.3 멀티모니터 및 DPI

- `DisplayInfo`와 `DiNative` 기능을 WPF service로 감싼다.
- WPF window 좌표와 Win32 monitor 좌표 변환을 한 곳에서 처리한다.
- 출력 창은 monitor id가 사라졌을 때 primary monitor로 fallback한다.
- DPI 변경 이벤트를 처리한다.

검증 대상:

- 단일 모니터
- 듀얼 모니터 좌우 배치
- 듀얼 모니터 상하 배치
- 서로 다른 DPI 배율
- 출력 모니터 제거/재연결

### 4.4 팝업/대화상자

- 기존 `PopupWindowHelper` 패턴은 WPF `Popup`, `Window`, `Adorner`, `DialogService` 중 기능에 맞게 대체한다.
- 라이브 위험 확인은 `SafetyConfirm`.
- 일반 알림은 `EsToast`.
- 설정/편집은 modal보다 side panel 또는 document-like dialog를 우선한다.

## 5. 완료 여부

| 항목 | 상태 | 비고 |
|---|---|---|
| ShortcutRegistry | 1차 완료 | thread-safe 구현, 테스트 있음 |
| HookManager adapter | 1차 운영 연결 완료 | `GlobalInputService`, `HookManagerGlobalKeySource`, `WpfGlobalInputDispatcher` 추가. 앱 시작 시 HookManager 전역 키를 `ShortcutRegistry.TryHandleGlobal`로 라우팅하고 종료 시 unsubscribe/dispose |
| Command catalog | 1차 구현 완료 | `CommandCatalog`, `CommandDescriptor` 추가. 기본 shortcut, command category, 위험 명령 메타데이터를 중앙화하고 `MainViewModel.BindShortcuts`가 catalog 기본값을 등록 |
| Display service | 1차 구현 완료 | `IDisplayService`, `DisplayService`, `SystemDisplayReader` 추가. WinForms `Screen.AllScreens` 기반 모니터 열거, primary fallback, 보조 출력 모니터 선호 정책 구현 |
| Window placement | 1차 구현 완료 | `IWindowPlacementService`, `WindowPlacementService` 추가. 출력 창 fullscreen/windowed 배치 정책을 중앙화하고 `OutputWindowService`가 주입된 배치 서비스를 사용 |
| Platform diagnostics | 1차 구현 완료 | `IPlatformDiagnosticsService`, `PlatformDiagnosticsService`, `PlatformDiagnosticsSnapshot` 추가. display/global input/command catalog 상태와 중복 command/shortcut 경고를 snapshot으로 수집 |

## 6. 이식 후 검증 방안

입력 검증:

- 로컬 단축키가 WPF 포커스 상태에서 동작한다.
- 글로벌 단축키가 다른 앱 포커스 상태에서 동작한다.
- 텍스트박스 입력 중 문자 입력과 command shortcut이 충돌하지 않는다.
- 중복 shortcut 저장이 차단된다.
- hook on/off 전환 후 이벤트 중복 발생이 없다.

디스플레이 검증:

- 모든 모니터가 이름/해상도/DPI와 함께 표시된다.
- 선택한 출력 모니터에 borderless window가 정확히 뜬다.
- 모니터 제거 시 앱이 죽지 않고 fallback 안내를 표시한다.
- 앱 재시작 후 마지막 출력 모니터 설정이 복원된다.

플랫폼 검증:

- Office 설치 확인이 정확하다.
- registry 권한 부족 상황에서 앱이 멈추지 않는다.
- 파일 경로가 UNC/network drive에서도 동작한다.

## 7. 테스트 방안

자동 테스트:

- Shortcut conflict tests
- `CommandCatalogTests`: command id 중복 방지, 기본 shortcut의 command 참조 검증, 기본 shortcut 충돌 방지, Live 위험 명령 메타데이터 검증
- display coordinate conversion tests
- `WindowPlacementServiceTests`: fullscreen 모니터 bounds 적용, settings 기반 custom bounds, windowed 16:9 중앙 배치, 짧은 모니터 height 기준 축소 검증
- `PlatformDiagnosticsServiceTests`: display/input/catalog snapshot, global input 오류 경고, display 진단 실패 graceful fallback, command/shortcut 중복 경고 검증
- `GlobalInputServiceTests`: 전역 입력 시작 1회 보장, global scope 단축키 라우팅, local-only 단축키 차단, stop/unsubscribe, 시작 실패 cleanup
- `DisplayServiceTests`: 빈 모니터 목록 fallback, primary 우선 정렬, preferred id 선택, 제거된 모니터 fallback
- `MainViewModelTests.OpenOutputCommand_UsesPreferredDisplayFromDisplayService`: 운영 셸 출력 열기 명령이 선택/선호 모니터를 사용
- `MainViewModelTests.OpenOutputCommand_UsesDefaultOutputMonitorFromSettings`, `MainViewModelTests.OpenOutputCommand_WhenAlwaysUseSecondaryMonitorDisabledWithoutDefault_SelectsPrimary`, `MainViewModelTests.OpenOutputCommand_WhenDefaultMonitorMissingAndAlwaysUseSecondaryDisabled_FallsBackToPrimary`: 저장/이식된 기본 출력 모니터와 보조 모니터 우선 정책, 제거된 저장 모니터 fallback이 운영 셸 초기 선택에 적용

수동 테스트:

1. WPF 앱 실행
2. 단축키 설정 열기
3. F5/Ctrl+F/Space 등 주요 단축키 실행
4. 다른 앱 포커스 상태에서 global shortcut 실행
5. 텍스트 입력 중 shortcut 충돌 확인
6. 출력 모니터 선택
7. 출력 창 이동/닫기/재열기
8. 모니터 연결 해제 후 fallback 확인
9. 앱 종료 후 hook 잔존 여부 확인

리허설 테스트:

- 리모컨 장비를 연결하고 30분 동안 Next/Prev/Black/Hide를 반복한다.
- 입력 누락, 중복 실행, 포커스 손실, LiveBar 상태 불일치를 기록한다.

## 8. 현재 검증 결과

2026-05-29 기준:

- `dotnet test Easislides.sln -c Debug`: 375개 통과
- `dotnet build Easislides.sln -c Release`: 성공
- `dotnet test Easislides.sln -c Release --no-build`: 375개 통과
- CodeGraph 동기화 완료: `Easislides.Wpf/Input/GlobalInputService.cs`, `Easislides.Wpf/Input/CommandCatalog.cs`, `Easislides.Wpf/Platform/DisplayService.cs`, `Easislides.Wpf/Platform/WindowPlacementService.cs`, `Easislides.Wpf/Platform/PlatformDiagnosticsService.cs`, `Easislides.Wpf/Shell/MainViewModel.cs` 인식 확인
- Release 산출물 확인: `Easislides.Wpf\bin\Release\net10.0-windows\EasislidesNext.exe`, `MainWindow.baml`, `OutputWindow.baml` (산출물 분리 §9.4 — exe 명 EasislidesNext)
- 남은 수동 검증: 실제 듀얼 모니터 좌우/상하 배치, 혼합 DPI, 모니터 제거/재연결, 실제 리모컨/타 앱 포커스 글로벌 단축키, 텍스트 입력 중 shortcut 충돌
