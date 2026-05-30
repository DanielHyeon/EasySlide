# `Easislides.Core.dll` 공통 도메인 추출 계획 (§9.4 / ADR-0007 / 작업 B-3)

> 상태: **설계(코드 변경 전)** · 작성 기준: `main` (B-1 `--legacy-ui` #25, B-2 산출물 분리 #26 병합 후)
> 목적: legacy WinForms(`Easislides.exe`)와 신규 WPF(`EasislidesNext.exe`)가 **공유할 도메인 어셈블리**를 안전하게 추출하는 단계 계획을 정의한다. 큰 리팩토링이므로 코드 변경 전 "무엇을·어떤 순서로" 먼저 확정한다.

## 1. 배경

- §9.4 산출물 분리 + ADR-0007 안전망: 두 빌드(legacy/신규)가 D-Day 이후까지 공존한다.
- 공통 도메인을 `Easislides.Core.dll`로 추출해 **양쪽이 참조**하면, 도메인 진실(enum·모델)의 단일 출처가 생기고 중복·드리프트를 줄인다.

## 2. 현황 분석 (조사 결과)

| 항목 | 사실 |
|---|---|
| legacy 프로젝트 | `Easislides/Easislides.csproj` — `net10.0-windows7.0`, `WinExe`, `UseWindowsForms=true` |
| 신규 WPF | `Easislides.Wpf/Easislides.Wpf.csproj` — `net10.0-windows`, 산출물 `EasislidesNext.exe` |
| 상호 참조 | **WPF는 legacy 를 ProjectReference 하지 않음** — 두 앱의 도메인 모델이 **현재 완전히 분리**(WPF는 `LegacySettingsMap` 등으로 매핑만) |
| legacy 도메인 위치 | `Easislides/Module/` — `CommonEnum.cs`(131줄), `SongFormat.cs`(97), `SongLyrics.cs`(79), `SongSettings.cs`(210) |

### 2.1 Core 후보의 결합도 분류 (using 분석)

| 파일 | 결합 | Core 적합성 |
|---|---|---|
| `CommonEnum.cs` | **없음**(순수 enum ~17종: SettingsCategory, MediaBackgroundStyle, PlayState, DatabaseType, VAlign, ImageMode, UsageMode, PraiseBookLayout, GapMedia, HeadingFormat, GapType, AlertType, SortBy, InfoType, ItemSource, KeyDirection, MPCType …) | ★★★ **최우선·최저위험** |
| `SongFormat.cs` | `System.Drawing`(Color/Font) | ★★ — Core 가 System.Drawing 의존을 가질지 결정 필요 |
| `SongLyrics.cs` | `System.Drawing` | ★★ — 동상 |
| `SongSettings.cs` | `System.Windows.Forms` + `static Easislides.Gf` | ★ **보류** — WinForms·Gf 강결합, 디커플 선행 필요 |

> WPF 측은 자체 enum(`LiveState` 등)을 별도로 갖고 있어 **개념적 중복**이 존재하나 타입은 공유하지 않는다. 통합(중복 제거)은 후순위(Phase 4)로 둔다.

## 3. 핵심 설계 결정

1. **Core TFM = 포터블** (`net10.0`, Windows 비의존). 두 앱(net10.0-windows*)이 모두 참조 가능하고, Core 에 WinForms/Drawing 결합을 끌어들이지 않는다. → `CommonEnum` 만 우선 이동(순수). System.Drawing 결합 타입은 별도 단계에서 결정.
2. **네임스페이스 유지 = 사용처 churn 0** (핵심 안전 장치). `CommonEnum` 을 Core 로 옮길 때 네임스페이스를 `Easislides.Module` 그대로 둔다. C# 은 한 네임스페이스가 여러 어셈블리에 걸쳐 존재할 수 있으므로, legacy 의 기존 `using Easislides.Module;` 코드는 **수정 없이** Core.dll 에서 enum 을 해석한다. (SongFormat 등 아직 안 옮긴 타입은 legacy 어셈블리의 같은 네임스페이스에 잔존 — 공존 가능.)
3. **단방향 의존**: `Easislides.Core` ← `Easislides`(legacy), `Easislides.Core` ← `Easislides.Wpf`(필요 시). Core 는 어떤 앱도 참조하지 않는다(순수 도메인).
4. **소비 시점 참조**: WPF 는 Core 타입을 실제로 소비할 때만 참조를 추가한다(불필요한 결합 회피).

## 4. 단계별 추출 순서 (작은 PR 단위, 각 단계 독립 검증)

### Phase 1 — `CommonEnum` → Core (최저위험, 먼저)
1. `Easislides.Core` 프로젝트 생성(`net10.0`, 클래스 라이브러리), 솔루션에 추가.
2. `Easislides/Module/CommonEnum.cs` 를 `Easislides.Core/` 로 이동(네임스페이스 `Easislides.Module` 유지).
3. `Easislides.csproj` 에 `ProjectReference Easislides.Core` 추가.
4. **검증**: legacy 빌드 0 errors(사용처 코드 무수정 컴파일), 전체 테스트 green.
5. **롤백**: 파일 되돌리기 + 참조 제거(독립적).

### Phase 2 — `SongFormat`/`SongLyrics` (System.Drawing 결정 후)
- 옵션 A: Core 에 `System.Drawing.Common`(크로스플랫폼 패키지) 추가 후 이동.
- 옵션 B: 도메인 모델에서 `System.Drawing.Color/Font` 를 원시값(ARGB int, 폰트명 string)으로 디커플 후 이동(더 깨끗·권장).
- 옵션은 Phase 1 완료 후 별도 결정(이 계획의 비범위 — 후속 ADR).

### Phase 3 — `SongSettings` 디커플 후 이동
- `System.Windows.Forms` 의존(메시지박스/스크린 등)과 `static Easislides.Gf` 호출을 인터페이스/원시값으로 분리한 뒤에만 이동. 가장 큰 작업.

### Phase 4 — WPF 가 Core 소비 / 개념 중복 제거 (선택·후순위)
- WPF 가 자체 enum 대신 Core enum 을 쓰도록 점진 통합. 회귀 위험이 있어 컴포지트·스크린샷 안전망 위에서 1종씩.

## 5. 위험과 완화

| 위험 | 완화 |
|---|---|
| 네임스페이스 분할로 혼란 | Phase 1 은 enum만, 네임스페이스 유지 → 사용처 무변경. 문서로 "Module 네임스페이스는 Core+legacy 에 걸쳐 있음" 명시 |
| legacy 빌드 깨짐 | 각 Phase 후 `dotnet build Easislides.sln` + 전체 테스트로 즉시 검증, 단계별 독립 롤백 |
| Core 에 원치 않는 결합 유입 | Core TFM 을 `net10.0`(비-windows)로 고정 → WinForms/Drawing 결합 타입은 컴파일 자체가 막혀 사전 차단 |
| 순환 참조 | Core 는 앱을 참조하지 않음(단방향 규칙) |
| 같은 네임스페이스가 두 어셈블리에 걸침 | **불변식**: 공유 네임스페이스의 동일 타입명은 반드시 **단일 어셈블리에만** 정의(이동 시 원본 삭제 동반). 양쪽에 같은 타입이 생기면 소비자에서 `CS0433` 모호성 에러. Phase 1 은 git mv 로 원본을 제거해 이 규칙을 지킴 |
| enum 정수값과 영속 데이터 | **경고**: `ItemSource`(0,2,3,…), `SettingsCategory.RotateString=10` 등 명시적 정수값 enum 은 DB/레지스트리/파일에 정수로 저장됐을 수 있다. Core 로 옮길 때 **값/순서를 재배치하지 말 것**(영속 데이터 손상). 이동은 값 무변경(0-line diff)만 허용 |

## 6. 수용 기준 (Phase 1)

- `Easislides.Core.dll`(net10.0) 생성, 솔루션 포함.
- `CommonEnum` enum 전부 Core 로 이동(네임스페이스 `Easislides.Module` 유지), legacy 사용처 코드 **무수정**.
- `dotnet build Easislides.sln` 0 errors, 전체 테스트(현재 467) green 유지.
- (가능 시) Core 어셈블리명/타깃을 고정하는 테스트 1건.

## 7. 비범위 / 후속

- Phase 2~4 의 구체 결정(System.Drawing 디커플 방식, WPF 통합 범위)은 Phase 1 완료 후 별도 계획/ADR.
- 배포 패키징(두 exe + Core.dll 동봉)과 1시간 예배 리허설 게이트(B-4)는 별도 항목.
- 이 계획이 합의되면 ADR 로 승격 검토(ADR-0008 후보).
