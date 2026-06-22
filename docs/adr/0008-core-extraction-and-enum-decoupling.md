# ADR-0008: `Easislides.Core` 공통 도메인 추출 — 상속 분리 패턴 & WPF enum 디커플 유지

* **상태**: Accepted
* **결정일**: 2026-05-31
* **결정자**: 프로젝트 메인테이너
* **태그**: architecture, core-extraction, domain, decoupling, migration
* **관련**: 계획서 §9.4 · [`core-extraction-plan.md`](../wpf-migration/core-extraction-plan.md) · ADR-0007 (안전망)

## 컨텍스트

§9.4 산출물 분리 + ADR-0007 안전망에 따라 legacy WinForms(`Easislides.exe`)와 신규 WPF(`EasislidesNext.exe`) 두 빌드가 D-Day 이후까지 공존한다. 공통 도메인을 `Easislides.Core.dll`(net10.0 포터블, System.Drawing/WinForms 비의존)로 추출해 도메인 진실의 단일 출처를 만들되, **양쪽 빌드가 깨지지 않게** 단계적으로 옮긴다.

이동 대상 도메인 타입은 결합도가 제각각이다:

| 타입 | 결합 |
|---|---|
| `CommonEnum`(enum 17종) | 없음(순수) |
| `SongFormat` | `System.Drawing.Color` |
| `SongLyrics` | `System.Drawing`(Font/Color/StringAlignment) + GDI+ 렌더 캐시 + `static Gf` |
| `SongSettings` | `System.Windows.Forms.ListView` + legacy `SongLyrics[]` + `static Gf` |

또한 신규 WPF 는 자체 enum(`GapItemMode`, `ImageFillMode`, `MediaPlaybackState`, `LiveState` 등)을 갖고 있어 일부가 Core enum 과 **개념적으로 겹친다**. 이를 통합할지가 Phase 4 의 질문이다.

## 고려한 대안

### Core 비-포터블 타입 이동 방식

**A. 전체 원시값 디커플** — `Color`→ARGB int, `Font`→디스크립터, WinForms 의존 제거 후 통째로 Core 이동.
- 장점: Core 에 도메인이 온전히 모임.
- 단점: `SongLyrics` 의 Font 는 렌더 핫패스 캐시 → 디스크립터화 시 매 그리기 Font 재생성(회귀·성능 위험). `SongSettings` 의 ListView 는 애초에 포터블화 불가. 사용처 광범위 churn.

**B. 상속 분리(채택)** — 포터블 부분만 `*Base`(Core)로 떼고, Drawing/WinForms/Gf 결합은 legacy 파생 클래스에 잔류. 파생이 같은 네임스페이스·이름을 유지 → 사용처 무변경.
- 장점: 렌더 핫패스 무변경, 사용처 churn 0(순수 relocation), 단계별 독립 검증·롤백.
- 단점: 도메인이 base/파생으로 나뉨(단, 포터블 경계가 명확해지는 이점이기도 함).

### WPF enum 개념 중복 (Phase 4)

조사 결과 Core enum 과 WPF enum 의 대응:

| Core enum | WPF enum | 일치 | 판정 |
|---|---|---|---|
| `GapType`(None=0/Black=1/Default=2/User=3) | `GapItemMode`(동일 값) | **정확 1:1** | 그러나 `SettingKey<>`·`LegacySettingsMap`·설정 UI 에 깊이 결합 |
| `ImageMode`(Tile/Centre/BestFit) | `ImageFillMode`(Fit/Fill/Stretch/Center) | 부분 | 멤버 상이(WPF 가 더 풍부) |
| `PlayState`(Stopped/Paused/Running/Init) | `MediaPlaybackState`(Empty/Ready/Playing/Paused/Stopped/Failed) | 부분 | 멤버 상이 |
| `MediaBackgroundStyle` | `TransitionBackgroundMode` | 부분 | 상이 |
| (없음) | `LiveState`, `*IssueKind`, `Import*`, `Asset*` 등 다수 | — | WPF 전용 |

**X. WPF 가 Core enum 소비(통합)** — `Easislides.Wpf` 에 Core 참조 추가, `GapItemMode`→`GapType` 교체.
- 단점: WPF→Core **신규 교차 결합**(WPF 는 의도적으로 Core 비참조). `GapItemMode` 는 영속화 키·레거시 마이그레이션·설정 UI 등 ~30곳 + 테스트에 박혀 있어 중간 규모 리팩토링 + 회귀 위험. 4값 enum 중복 제거 대비 이득 작음. 나머지 후보는 멤버가 달라 통합 불가.

**Y. WPF enum 분리 유지(채택)** — WPF 는 `LegacySettingsMap` 으로 레거시와 경계를 긋는 자체 도메인을 유지. 중복은 우발적이 아니라 **의도적 디커플**로 본다.

## 결정

1. **비-포터블 도메인 타입은 상속 분리(대안 B)로 이동한다.**
   - `SongFormat` 은 색상 결합만 있어 ARGB `int` 로 디커플 후 통째로 Core 이동(Phase 2a).
   - `SongLyrics`/`SongSettings` 는 `SongLyricsBase`/`SongSettingsBase`(Core) + legacy 파생으로 분리(Phase 2b/3).
   - **네임스페이스 `Easislides.Module` 유지** — 같은 네임스페이스가 Core+legacy 두 어셈블리에 걸친다(불변식: 동일 타입명은 단일 어셈블리에만).
   - 색상은 Core 에서 ARGB `int` 로 보관, 경계에서 `Color.FromArgb`/`.ToArgb` 변환(영속 정수 포맷 무변경).

2. **WPF enum 은 Core enum 과 통합하지 않고 분리 유지한다(대안 Y).**
   - WPF 의 enum 중복은 `LegacySettingsMap` 기반 의도적 경계의 산물이다. WPF→Core 결합을 추가하지 않는다.
   - 향후 신규 공유 개념이 생기면 그때 Core 에 정의하고 양쪽이 소비한다(소비 시점 참조 원칙).

### 진행 상태 (2026-05-31 기준)

| Phase | 내용 | 상태 |
|---|---|---|
| 1 | `CommonEnum` → Core | ✅ main 머지(#28) |
| 2a | `SongFormat` → Core + 색상 int | ✅ main 머지(#30 경유) |
| 2b | `SongLyrics` 상속 분리 | ✅ main 머지(#30) |
| 3 | `SongSettings` 상속 분리 | ✅ main 머지(#31) |
| 4 | WPF enum 통합 | ✅ **분리 유지로 종결**(이 ADR) |

## 결과

### 긍정적
- 렌더 핫패스(Font/GDI+) 무변경 → 라이브 송출 회귀 위험 최소화.
- 사용처 churn 0(상속·네임스페이스 유지) → 단계별 독립 검증·롤백 가능, 전체 테스트 green 유지.
- Core 가 System.Drawing/WinForms 비참조(테스트 `Core_Does_Not_Reference_WindowsForms_Or_Drawing` 가 가드) → 포터블성 보장.
- WPF 의 깨끗한 자체 도메인 경계 보존(레거시 enum 정수 quirk 가 신규 빌드로 새지 않음).

### 부정적 / 리스크
- 도메인이 `*Base`(Core) + 파생(legacy)로 물리적으로 나뉘어, 한 타입을 보려면 두 파일을 봐야 함(주석으로 상호 안내).
- Core enum ↔ WPF enum 개념 중복이 남음 — 의도적이나, 추후 동기화 누락(예: 한쪽에만 새 값 추가) 가능성. 공유가 필요해지는 시점에 재검토.
- `SongSettings.Initialise()` 의 local `songFormat` 미대입(기존 죽은 코드)은 relocation 계약 보존 위해 미수정 — relocation 종료 후 별도 정리 대상.

## 참조

- 계획서 §9.4 · [`core-extraction-plan.md`](../wpf-migration/core-extraction-plan.md)
- ADR-0007 (`--legacy-ui` 안전망)
- PR #28(Phase 1), #29/#30(Phase 2), #31(Phase 3)
- 가드 테스트: `Easislides.Wpf.Tests/Startup/CoreExtractionTests.cs`
