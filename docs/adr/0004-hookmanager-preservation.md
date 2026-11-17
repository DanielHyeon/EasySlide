# ADR-0004: HookManager 전역 후킹 보존 (vs WPF InputBinding)

* **상태**: Accepted
* **결정일**: 2026-05-27
* **결정자**: 프로젝트 메인테이너 + UI/UX 현대화 리뷰
* **태그**: input, keyboard, global-hook, win32, live-safety
* **관련**: 계획서 §7.5, §8.3 (Sprint 0 PoC-A), §10.3 · ADR-0001 (WPF)

## 컨텍스트

`Easislides/HookManager/` 에 5파일로 구성된 전역 Win32 키보드/마우스 후킹 시스템:
- `HookManager.cs` — 진입점
- `HookManager.Callbacks.cs` — 후킹 콜백
- `HookManager.Windows.cs` — Win32 API 바인딩 (`SetWindowsHookEx`, `WH_KEYBOARD_LL`, `WH_MOUSE_LL`)
- `HookManager.Structures.cs` — Win32 구조체
- `GlobalEventProvider.cs` — .NET 이벤트 어댑터

이 시스템이 처리하는 것:
- **무선 리모컨 입력** — 라이브 운영자가 무대 뒤에서 PgUp/PgDn/F5/Esc 등으로 슬라이드 전환 (앱이 포커스를 갖지 않을 때도 작동)
- **글로벌 단축키** — 다른 앱이 활성일 때도 EasiSlides에 입력 전달
- **마우스 후킹** — 송출 화면 영역 클릭 차단 등

WPF는 **표준 입력 라우팅**으로 `InputBinding`/`KeyBinding`/`PreviewKeyDown` 라우티드 이벤트를 제공하지만, 이는 **앱이 포커스를 가질 때만** 작동.

**Q4 결정 (계획서 §7.5)**: HookManager 보존. 하지만 어떻게 WPF 라우팅과 공존시킬 것인가?

## 고려한 대안

### A. HookManager 그대로 보존 + WPF `PreviewKeyDown` 협력 — **채택**

| 장점 | 단점 |
|---|---|
| **검증된 라이브 동작** — 5년+ 운영 환경에서 안정 | 코드 두 시스템 공존 (HookManager + WPF InputBinding 양쪽 이해 필요) |
| **무선 리모컨 호환** — 앱 비포커스 시에도 작동 | 단축키 매핑 검색이 두 곳에서 일어남 (`HookManager.Callbacks.cs` + XAML `KeyBinding`) |
| **Win32 레벨 우선순위** — OS 단축키와 충돌 최소 | — |
| 변경 위험 없음 — 이 부분이 깨지면 라이브 운영 마비 직결 | — |

### B. WPF `InputBinding`/`KeyBinding`으로 완전 대체

| 장점 | 단점 |
|---|---|
| 코드 일관성 — XAML 한 곳에서 단축키 선언 | **글로벌 후킹 미지원** — 앱 포커스 잃으면 작동 안 함 |
| MVVM `Command` 자연스러운 통합 | **무선 리모컨 동작 불확실** — OS가 어느 앱에 입력을 보내는지 불명확 |
| — | 마이그레이션 위험: 라이브 단축키 누락 = 운영 마비 |

### C. 점진 전환 — UI 단축키는 WPF InputBinding, 글로벌은 HookManager

| 장점 | 단점 |
|---|---|
| 각 시스템의 강점 활용 | 두 시스템 책임 경계 모호 — "이 단축키는 어느 쪽?" 매번 판단 |
| 마이그레이션 가능 | 충돌 시 디버깅 비용 폭증 |
| — | 같은 키 (예: `Space`)가 두 곳에 등록되면 우선순위 혼란 |

## 결정

**A. HookManager 보존 + WPF `PreviewKeyDown` 협력.**

세부 — **충돌 회피 프로토콜**:

1. **단축키 등록 단일 소스**: `Easislides.Wpf/Input/ShortcutRegistry.cs` 신설.
   - 단축키 정의를 `Shortcut` 레코드(키 조합 + Command + 글로벌 여부)로 단일 선언
   - HookManager는 `IsGlobal == true`인 항목만 처리
   - WPF `PreviewKeyDown`은 모든 항목 처리 (포커스 있을 때)

2. **우선순위**: 같은 키 입력이 양쪽에 도달할 수 있을 때 — HookManager가 먼저 Win32 메시지 펌프에서 가로채고, 처리됨 표시. WPF 라우티드 이벤트는 받지 못함.

3. **WPF 측 구현**:
   ```csharp
   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
       if (_shortcutRegistry.TryHandle(e.Key, Keyboard.Modifiers))
       {
           e.Handled = true; // 라우트 종료
           return;
       }
       base.OnPreviewKeyDown(e);
   }
   ```

4. **검증 — Sprint 0 PoC-A**: 다음 시나리오를 자동 테스트.
   - 글로벌 키(예: `F5`) → 다른 앱 활성 상태에서 EasiSlides 명령 실행 확인
   - 로컬 키(예: `Ctrl+S` in 편집 폼) → 글로벌 등록 없을 때 폼별 작동 확인
   - 충돌 키 → 글로벌 핸들러가 항상 우선 작동 확인
   - 리모컨 시뮬레이션 — `PgUp`/`PgDn`이 앱 비포커스 시에도 슬라이드 전환

PoC-A 실패 시 Sprint 1 진입 차단 (계획서 §8.3).

## 결과

### 긍정적
- **라이브 안정성 보존** — 5년+ 검증된 동작 그대로
- **리모컨·글로벌 단축키 보장** — 운영자가 부스 뒤·다른 앱 활성 상태에서도 EasiSlides 제어 가능
- **마이그레이션 위험 최소** — HookManager 코드는 0줄 변경

### 부정적
- **두 시스템 공존 인지 비용** — 신규 개발자가 단축키 추가 시 ShortcutRegistry 중앙 등록 규칙을 학습해야 함
- WPF 표준에서 벗어남 — 코드 리뷰어 입장에서 "왜 KeyBinding 안 쓰나?" 의문 발생 가능. 본 ADR을 코드 헤더 주석에 인용.

### 중립 / 리스크
- **HookManager가 다른 보안 소프트웨어(백신·EDR)에 의해 차단될 가능성** — 일부 기업/교회 보안 정책에서 LL 후킹을 위험으로 분류. 현재 운영 중에 발생한 사례는 없으나, 신규 환경에서는 추적 필요. 해결책: 사용자 안내 + 화이트리스트 가이드.
- **`SetWindowsHookEx` 콜백 실행 시간 제한** (15ms LowLevel 후킹) — 현재 코드는 이 제한 안에서 동작하나, MVVM Command 호출이 비동기로 갈 때 콜백 내에서 직접 호출하면 위반 가능. ShortcutRegistry는 콜백에서 큐잉만 하고 실제 처리는 UI Dispatcher로 위임하는 패턴 적용.

## 참조

- 계획서 §7.5, §8.3 (Sprint 0 PoC-A), §10.3
- [Win32 SetWindowsHookEx 문서](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowshookex)
- [WPF Input Routing](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/input-overview)
- `Easislides/HookManager/HookManager.cs` (보존 대상)
- ADR-0001 (WPF UI)
