# 별도 Preview(스테이지) 모니터 — 설계 + 증분 계획 (gap §3.1)

> 2026-06-03 착수. 레거시 §5 "이중 모니터 Preview+Output 동시 송출"(🔴) 포팅.
> 회중용 **출력 모니터**와 별개로, 워십 리더·밴드가 보는 **스테이지/확인 모니터**를 띄운다.

## 1. 결정된 범위 (사용자 합의)

- **MVP = 풀 스테이지 디스플레이**: 현재 가사 + **다음 항목** + **시계** 오버레이.
- **스테이지 관행**: 회중 화면이 꺼져도(blackout/clear) **스테이지 모니터는 가사를 계속** 보여 준다(리더가 계속 인도하도록). 즉 Preview 는 출력 body 를 미러하되 blackout/clear/hide 게이트를 **무시**한다.
- 송출 모니터 선택과 **독립**된 Preview 모니터 선택(다른 모니터).

## 2. 아키텍처 — 출력 스택의 병렬 미러

기존 출력 스택이 깔끔히 계층화돼 있어 그대로 병렬 복제한다(검증된 구조·테스트 패턴 재사용). 표면 지오메트리 레코드(`OutputDisplay`, `OutputWindowPlacement`)와 렌더러(`IOutputRenderer`), 디스플레이 서비스(`IDisplayService`), 배치(`IWindowPlacementService`)는 **공유**한다.

| 출력(기존) | Preview(신규) | 역할 |
| --- | --- | --- |
| `IOutputWindowService` / `OutputWindowService` | `IPreviewWindowService` / `PreviewWindowService` | 상태머신(Open/MoveTo/Close·상태 이벤트) |
| `OutputWindowHost` | `PreviewWindowHost` | SessionChanged 구독 → Surface 구동 |
| `IOutputSurface`(OutputWindow) | `PreviewWindow`(IOutputsurface 재사용 또는 전용) | 모니터 풀스크린 창 |
| `OutputWindowViewModel` | `StageDisplayViewModel` | **스테이지 콘텐츠**(가사 미러+blackout 무시, 다음 항목, 시계) |
| `OutputDisplays`/`DefaultOutputMonitorId` | `PreviewDisplays`/`PreviewMonitorId`(신규 설정) | 모니터 선택 |

핵심: 신규 코드는 **얇은 병렬 계층 + 스테이지 VM**뿐. 출력 경로는 **건드리지 않는다**(회귀 0).

## 3. 증분 분해 (작은 단위·TDD, 각 독립 출하)

- **160-A** ✅ `PreviewWindowService` + `PreviewWindowState`/`PreviewWindowChangedEventArgs` — 순수 상태머신(UI 0). `OutputWindowService` 미러, 지오메트리 레코드 공유. 완전 단위 테스트(open/move/close/동일상태 no-op).
- **160-B** `PreviewWindowHost` + `StageDisplayViewModel` — SessionChanged→Surface 구동, FakeSurface 테스트. 가사 미러 + **blackout/clear 무시**(스테이지 관행)가 이 단계 핵심.
- **160-C** 실제 `PreviewWindow` 창(풀스크린 Surface) + App DI 배선. XAML 구조 테스트.
- **160-D** Preview 모니터 선택 + 명령 + 메뉴 — `PreviewMonitorId` 설정·picker(`OutputDisplays` 미러)·"Preview 모니터 열기/이동/닫기"(출력 메뉴), VM 테스트.
- **160-E** 스테이지 오버레이 — **다음 항목** 제목(큐에서 계산) + **시계**. `StageDisplayViewModel` 에 next/clock 추가, 다음-항목 플러밍(MainViewModel 큐→Preview).

## 4. 주의·리스크

- **다음 항목 플러밍**(E): `LiveSessionService` 는 현재 라이브만 안다 → 다음 항목 제목은 MainViewModel 큐/인덱스에서 와야 함. 세션 스냅샷 확장 또는 별도 신호 경로 설계 필요(E 착수 시).
- **배치 공유 주의**(C/D): `WindowPlacementService(ISettingsService)` 는 `DisplayCustomLeft/Top/Width` 설정을 읽어 출력용 커스텀 영역으로 배치를 덮어쓴다(`WindowPlacementService.cs:59-87`). Preview 가 같은 settings-backed 인스턴스를 공유하면 Preview 창도 그 영역으로 끌려간다 → Preview 는 default(설정 무시) 배치 인스턴스를 쓰거나 `PreviewMonitorId` 기반 풀스크린 전용 경로를 둘지 D 착수 시 결정(160-A 는 default-ctor 라 무관).
- 출력 스택 무변경 원칙. 공유 레코드(`OutputWindowPlacement` 등)에 Preview 전용 필드 추가 금지(필요하면 Preview 전용 타입).
- 시계는 타이머(DispatcherTimer) — 테스트는 VM 의 시각 포맷 순수 함수로 분리해 검증.
