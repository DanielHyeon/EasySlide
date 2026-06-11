# SDD 방법론 적용 검토 보고서 — EasiSlides v2.6.4 WPF 포팅

> 작성일: 2026-06-11 · 기준 브랜치: `feat/per-song-font-bg-bible-version-crud`
> 검토 방법: 저장소 산출물(openspec/·docs/·.claude/·.codex/), git 이력(총 783커밋), 세션 핸드오프 메모리 기반의 **증거 중심 검토**.
> 한계: 세션 트랜스크립트 자체는 검토 대상이 아니므로, 스킬/에이전트의 "실제 호출 여부"는 저장소에 남은 산출물·로그·메모리로부터 추정한 것이다.

---

## 1. 요약 (Executive Summary)

이 프로젝트의 SDD(Spec-Driven Development)는 **"도구를 먼저 깔고 방법론을 따른" 것이 아니라, 작동하는 루프를 먼저 만들고 나중에 도구로 제도화한 사례**다.

1. **실질적 성공의 핵심은 4계층 스택이 아니라 "증분 루프"였다.** 조사(이미 포팅됐는지 먼저 확인) → 실패 테스트 → 최소 구현 → `code-reviewer` 에이전트 게이트(0 critical/major 목표) → 로드맵 한 줄 기록 → 커밋/푸시. 이 루프가 2026-06-01~04 나흘간 **증분 160여 개, 테스트 919 → 2,017 green(+1,098)** 이라는 처리량을 만들었다.
2. **가장 효과가 검증된 단일 도구는 `code-reviewer` 에이전트**다. 로드맵 진행 로그에 증분별 평결이 전수 기록되어 있고, MAJOR급 결함(경쟁 조건, 잠복 설정 버그, 송출 가시성 회귀 등)을 머지 전 반복 적발했다.
3. **OpenSpec·CodeGraph·gstack·GSD로 구성된 공식 스택은 2026-06-04에야 설치**됐고, 이후 운영은 "OpenSpec change 10건 + codegraph-impact 7건"까지는 정착했으나 **라이프사이클 후반부(`/opsx:sync`·`/opsx:archive`, specs 동기화)는 한 번도 완결되지 않았다** (archive 0건, `openspec/specs/` 빈 상태).
4. **설치 대비 활용률 격차가 크다.** gstack 52스킬 중 실효 확인된 것은 방법론 스킬 일부(브라우저 계열은 Windows에서 미검증), 에이전트 32종 중 실사용 증거가 뚜렷한 것은 사실상 1종(code-reviewer)이다. GSD는 "참고용"으로 강등됐지만 훅 8종은 모든 도구 호출에 상시 작동 중이다.
5. **이 프로젝트가 잘한 것의 본질**은 도구 선택이 아니라 운영 원칙이다 — *거짓 완료 금지, 검증 증거 없는 완료 보고 금지, 작은 외과 수술 우선, 재사용 우선(422줄 XAML 무복제 결정), 송출 안정성 > 구조적 아름다움.* 도구는 이 원칙을 싸게 강제하는 수단으로 쓰였을 때만 효과를 냈다.

**종합 평가: 실행 루프 A / 계약 관리 B- / 도구 포트폴리오 관리 C+.** 권고는 §8 참조.

---

## 2. 방법론 도입 타임라인 — 4단계 진화

| 시기 | 커밋 | 운영 방식 | 계약(스펙)의 실체 |
| --- | --- | --- | --- |
| **1기** 2025-12 ~ 2026-02 | 73 | 레거시 유지보수·탐색, 소규모 | 없음(커밋 메시지 수준) |
| **2기** 2026-05 | 230 | **gap-analysis 주도 PR 루프** — 기능별 브랜치(`feat/inshell-lyrics-*` 등) → PR(#61~69+) 머지. 커밋이 gap 문서 §번호를 직접 인용 (예: `feat(wpf): 예배 순서 항목 이동 … (§7.3-B)`) | `docs/wpf-migration/gap-analysis.md`(72KB)·상세 갭(98KB)이 사실상의 스펙 |
| **3기** 2026-06-01 ~ 06-03 | (6월 480 중 전반) | **로드맵 증분 루프(자율 연속 실행)** — `frmmain-port-roadmap.md` 수립 후 "완료 주장 시 남은 빨강/노랑 백로그를 근거로 미완이면 계속"이라는 지속 지시 하에 증분 1→160-E를 연속 처리. 매 증분 TDD→code-reviewer→로드맵 기록→한글 커밋→푸시 | 로드맵(225KB) + 갭 문서. **이때는 OpenSpec/gstack/GSD 미설치** — 로드맵 머리말에 "요청된 gstack-qa·gsd-verify-work는 환경 미설치 → 표준 검증(전수 dotnet test + code-reviewer + Release 빌드/심볼)으로 대체"라고 명시 |
| **4기** 2026-06-04 ~ 현재 | 214 | **공식 SDD 스택 제도화** — OpenSpec 1.4.1 + CodeGraph 0.9.4(MCP+자동 sync 훅) + gstack 52스킬(`--prefix`) + GSD 1.2.0(강등). `.claude/CLAUDE.md`에 4계층 계약·우선순위·Hard Rules·위험도 매트릭스 성문화. 06-05 운영 정책 보강(GSD 강등 확정, gstack 계획 스킬 용도 제한), TDD 게이트 완화 결정 | `openspec/changes/<id>/{proposal,design,tasks,codegraph-impact}.md` 10건 |

**핵심 관찰**: 처리량 정점(3기)은 공식 스택 설치 *이전*에 달성됐다. 4기의 스택은 3기 루프가 이미 증명한 관행(작은 단위·리뷰 게이트·증거 기록)을 **문서 계약과 구조 증거로 제도화**한 것이며, 처리량 자체를 만든 원동력은 아니다. 이는 "방법론이 먼저, 도구는 나중"이라는 올바른 순서의 드문 실증 사례다.

---

## 3. SDD 스택 구조 (성문화된 계약)

`.claude/CLAUDE.md` §1 (루트 `AGENTS.md`는 Codex용 미러):

> OpenSpec은 계약, CodeGraph는 증거, Superpowers는 TDD 실행, gstack은 차단 게이트다.

| 계층 | 도구 | 실측 상태 (2026-06-11) |
| --- | --- | --- |
| 계약 | OpenSpec (`/opsx:*` 5커맨드) | change 10건 활성 / archive 0 / specs 0 |
| 구조 | CodeGraph (MCP 10도구 + PostToolUse 자동 sync 훅) | `.codegraph/codegraph.db` 운영 중, impact 문서 7/10 change |
| 실행 | Superpowers — **의도적 미설치, 규칙만 수동 적용** | 완화된 TDD 게이트(06-04 결정)로 운영 |
| 품질 | gstack (`/gstack-*` 52스킬) | 방법론 스킬 가용, 브라우저 스킬(qa/browse) Windows 미검증 |
| (강등) | GSD 1.2.0 (`/gsd:*` ~20커맨드 + 훅 8종 + 전용 에이전트 6종) | "Phase 문서화 참고용"으로 강등, 단 훅은 상시 작동 |

부속 장치: 우선순위 5단계(충돌 시 OpenSpec 산출물이 최상위), Hard Rules 8개(무계약 production 변경 금지·Single Writer 등), 위험도 4등급별 도구 강도 매트릭스, 도메인 체크리스트(송출 영향·멀티모니터 케이스·COM 해제·DB 경유), 검증 게이트 정의(빌드+테스트+리뷰+수동 QA).

---

## 4. 계층·도구별 세밀 검토 — 장단점

### 4.1 OpenSpec (`/opsx:explore·propose·apply·sync·archive`)

**실사용 증거**: change 10건(`wpf-frmmain-*` 9건 + `wpf-legacy-working-folder-autodetect`), 전건 proposal/design/tasks 보유, 7건 codegraph-impact 동반. 최초 커밋 2026-06-04(설치 당일 5건 일괄 생성 — 기존 작업의 소급 계약화 포함).

| 장점 (이 프로젝트 실증) | 단점·리스크 (이 프로젝트 실증) |
| --- | --- |
| **Non-goals 절이 실질 효과를 냄** — "layout-only 작업으로 완료 주장 금지", "패리티 전 재설계 금지" 같은 부정 조항이 거짓-완료 방지 원칙과 직결 | **라이프사이클 미완결** — sync/archive 0건, `openspec/specs/` 빈 폴더. "계약→승인→구현→스펙 반영→아카이브" 중 후반 40%가 작동하지 않아, change가 쌓일수록 "현재 유효한 스펙이 무엇인가"가 다시 모호해짐 |
| change 단위가 작고 목적이 명확(visual-parity, shortcut-parity 등 관심사별 분리) — 거대 단일 스펙의 경직성 회피 | **tasks.md 양식이 계약 위반** — CLAUDE.md는 Phase별 `Goal/Scope/Tasks/DoD/Tests/Constraints`를 요구하나 실제 파일은 평면 체크박스 목록. 정책-실행 갭 |
| design.md가 "무엇을 안 할지"까지 기록(예: visual-parity-correction — "새 라이브 렌더 모델 없음, 좌표 동작 무변경") | **미완 DoD 방치** — 1to1-operator-console-parity의 Phase 5~7 미체크 상태에서 후속 change들이 파생. change 간 의존·승계 관계가 어디에도 추적되지 않음 |
| 커맨드 5종이 단순해 학습 비용 낮음, Claude/Codex 양쪽 동일 스캐폴드 | **커밋 ↔ change 추적성 부재** — 06-04 이후 214커밋 중 change-id를 참조하는 커밋 0건. 어떤 커밋이 어떤 계약의 이행인지 git만으로 복원 불가 |

**판정**: 계약의 "앞 절반"(제안·설계·비목표)은 정착, "뒤 절반"(동기화·아카이브·추적성)은 미정착. 도구 문제가 아니라 운영 루틴에 sync/archive 단계가 편입되지 않은 문제.

### 4.2 CodeGraph (MCP `codegraph_*` 10도구 + 자동 sync 훅)

**실사용 증거**: codegraph-impact.md 7건(전부 "구현 전 작성" 명시, 고영향 영역과 "공유 로직 수정 전 targeted impact 재실행" 가드 조항 포함). `.claude/settings.json`의 PostToolUse 훅이 Write/Edit마다 `codegraph sync` 자동 실행. CLAUDE.md에 의도별 도구 매핑표 + 금지 패턴(심볼 찾기에 grep 금지 등) 성문화.

| 장점 | 단점·리스크 |
| --- | --- |
| **impact 문서가 "구현 전 의례"로 정착** — FrmMain(8.8k줄 partial) 같은 거대 레거시에서 영향 범위를 선언하고 들어가는 것 자체가 회귀 방지 장치 | **훅 부작용 실측** — 백그라운드 sync가 git 작업과 겹치며 **stale `index.lock` 충돌** 유발(핸드오프 메모리에 복구 절차 기록), 커밋 직후 `sleep 4` 후 푸시라는 우회 관행 발생 |
| 자동 sync 훅으로 인덱스 신선도를 무비용 유지(수동 sync 망각 문제 제거) | impact 문서 3/10 change 누락 — "공유 심볼은 impact 없이 수정 금지" Hard Rule이 100% 집행되지는 않음 |
| AST 기반이라 WinForms partial 클래스(FrmMain 6분할)·gf* 정적 헬퍼의 호출 관계 추적에 grep보다 구조적으로 우월 | dynamic/DI/reflection 한계는 스스로 인지하고 "테스트로 보완" 명시 — 단 WPF의 XAML 바인딩·DI 컨테이너 경유 흐름은 그래프 사각지대로 남음 |
| 의도별 매핑표·금지 패턴이 탐색 비용을 체계적으로 절감(search/callers/impact/context/trace/explore 역할 분리) | 인덱스 ~500ms 지연을 모르면 "수정 직후 조회"에서 stale 결과 — 문서화는 되어 있으나 운영자가 기억해야 하는 암묵지 |

**판정**: 4계층 중 비용 대비 효과가 가장 좋은 층. 훅-git 충돌만 해소하면 순효익.

### 4.3 Superpowers (의도적 미설치) + 완화된 TDD 게이트

**실사용 증거**: 메모리에 "Superpowers intentionally excluded per the SDD guide" — 계약상 "실행 계층"이지만 실체는 **플러그인 없이 규칙만 수동 적용**. 2026-06-04 사용자 결정으로 게이트 완화: 테스트는 *새 동작/동작 변경*에만 요구, 동작 불변 리팩터·문서·설정은 면제, 비상 탈출구 `Test-Needed: no` 트레일러.

| 장점 | 단점·리스크 |
| --- | --- |
| **TDD가 구호가 아니라 실측됨** — 로드맵 증분마다 "신규 테스트 N + 누적 green 수" 기록(919→2,017). 현재 테스트 파일 148개, [Fact]/[Theory] 마커 2,001개 | "Superpowers"라는 계층명이 실체(수동 규칙)와 불일치 — 신규 참여자가 설치된 도구로 오인할 수 있는 명명 부채 |
| 완화 결정이 **명시적·문서화된 트레이드오프** — 엄격 모드의 마찰(리네임에도 테스트 요구)을 겪고 나서 경계 휴리스틱+탈출구+["모호하면 테스트 쪽으로" 기울기]로 조정. 방법론을 현실에 맞춰 튜닝한 모범 사례 | 완화 게이트를 **집행하는 훅이 없음**(메모리에 자인: "no installed hook enforcing it") — 순수 자율 규범이라 세션·에이전트가 바뀌면 침식 가능 |
| Release 구성 테스트 실행 규칙(Debug DLL 잠금 회피) 같은 환경 특이사항까지 운영 지식으로 축적 | 테스트의 무게중심이 WPF 신규 코드에 편중 — 레거시 WinForms 쪽은 여전히 수동 QA 의존(이는 의도된 범위이긴 함) |

**판정**: "도구 없이 규율만 차용"이 이 프로젝트에서는 작동했다. 단, 작동 이유는 로드맵 기록+reviewer 게이트라는 **외부 검증 루프**가 떠받쳤기 때문이며, 그 루프가 사라지면 게이트도 같이 무너질 구조다.

### 4.4 gstack (`/gstack-*` 52스킬, `--prefix` 설치)

**실사용 증거**: 설치·검증 기록(메모리)과 CLAUDE.md 역할 정의는 충실. 단 **Windows에서 setup이 Playwright/Chromium 검증 단계에서 행** → 수동 링크로 우회, 브라우저 의존 스킬(`/gstack-qa`, `/gstack-browse`)은 미검증 상태. 저장소 산출물에서 gstack 게이트 실행 흔적(리뷰 리포트 등)은 확인되지 않음.

| 장점 | 단점·리스크 |
| --- | --- |
| `--prefix` 설치로 내장 `/review` 등과의 네임스페이스 충돌을 사전 회피 — 다중 스킬 생태계 운영의 좋은 선례 | **52스킬 중 실효 확인 소수** — 설치 풋프린트·스킬 목록 인지 부담 대비 가동률이 낮음. "차단 게이트" 계층이 실제로 차단한 기록이 저장소에 없음 |
| guard/freeze(편집 경로 제한)는 자율 루프의 폭주 방지 장치로 설계상 적합 — 표준 워크플로우에 `/gstack-freeze <허용 경로>`로 편입 | 핵심 차별화 기능(브라우저 QA)이 이 프로젝트 환경(Windows + WPF 데스크톱)과 **이중으로 부적합** — Chromium 행 + 애초에 웹앱 대상 QA라 WPF 송출 검증에 못 씀 |
| `/gstack-ship`을 "자동 배포가 아니라 ship 가능성 확인 게이트"로 재정의해 수용 — 도구를 정책에 종속시킨 올바른 방향 | 계획 스킬(`/gstack-spec`·`/gstack-autoplan`)이 OpenSpec과 역할 중복 → "아이디어 검토용만"으로 제한하는 교통정리 비용 발생(06-05 정책 보강) |

**판정**: "품질 게이트 계층"의 실질은 현재 code-reviewer 에이전트 + dotnet test가 대행 중. gstack은 잠재 가치(guard/freeze/cso) 대비 검증 부족 상태로, 유지하려면 가동 증거를 만들고 아니면 축소가 합리적.

### 4.5 GSD (강등 운영 + 상시 훅 인프라)

**실사용 증거**: `.claude/get-shit-done/` 약 190파일(워크플로우 80+, 레퍼런스 60+, 템플릿 40+) 설치. `.planning/` 미생성(= `/gsd:new-project` 미실행, 정식 GSD 플로우 미가동). 반면 **훅 8종은 settings.local.json에 등록되어 모든 세션에서 작동**: SessionStart(업데이트 확인·세션 상태), PostToolUse(context-monitor, read-injection-scanner, graphify-update, phase-boundary), PreToolUse(prompt-guard, read-guard, workflow-guard, validate-commit).

| 장점 | 단점·리스크 |
| --- | --- |
| **강등 결정 자체가 잘한 일** — OpenSpec과 계약 소유권이 충돌하는 것을 "phase-based execution pattern만 OpenSpec tasks.md로 흡수"라는 원칙으로 정리. 도구 중복 시 책임 단일화의 교과서적 처리 | **"참고용" 도구가 실행 비용은 전액 지불** — 훅 8종이 Read/Write/Edit/Bash 전 호출에 개입(각 timeout 5~10s 한도). 강등됐는데 런타임 오버헤드는 정식 계층과 동일 |
| 부수 훅의 독립 가치 — read-injection-scanner(파일 경유 프롬프트 인젝션 방어)는 자율 루프 안전망으로 유의미, context-monitor도 장기 세션 운영에 부합 | 1.5MB+ 문서 풋프린트와 `/gsd:*` 20개 커맨드가 스킬 목록을 점유 — 세션마다 "이건 안 쓰는 것"이라는 메타 지식을 요구(CLAUDE.md·메모리 양쪽에서 반복 단속 중인 것이 그 방증) |
| GSD 전용 에이전트 6종(gsd-planner/executor 등)은 비활성이라 해는 없음 | validate-commit 훅은 opt-in/off 상태로 사실상 장식 — 켜져 있는 것과 작동하는 것의 구분이 설정 파일을 읽어야만 보임 |

**판정**: "흡수 원칙"은 모범적이나, 흡수가 끝났다면 본체는 정리 대상. 남길 가치가 있는 것은 injection-scanner 등 훅 2~3종이며, 이는 GSD 전체 없이도 독립 유지 가능.

### 4.6 에이전트 — Claude측 32종 / Codex측 미러 26종

**구성**: `.claude/agents/` 32정의(도메인 26 + gsd 전용 6), `.codex/agents/` TOML 26(현재 untracked — 커밋 대기). 도메인 적합도를 보면 C#/WPF 데스크톱 프로젝트에 직접 유효한 것은 약 10종(code-reviewer, dotnet-csharp-quality-reviewer, code-debugger, code-refactor 계열, code-inspector-tester, code-implementation-planner, technical-doc-writer, ux-ui-design-reviewer, code-security-auditor, database-designer), 나머지 절반 이상(php/ios/mobile/frontend/javascript/typescript/python/api/backend/msa/rag 등)은 **이 프로젝트 도메인과 무관한 카탈로그 적재물**이다.

**실사용 증거가 뚜렷한 것은 사실상 1종 — `code-reviewer`.** 로드맵 진행 로그에 증분별 평결이 전수 기록:

| 증분 | reviewer가 잡은 것 (머지 전 적발) | 등급 |
| --- | --- | --- |
| 6-슬라이스2 | 저작권 오버레이가 기존 코너 UI와 충돌 → 위치 변경 | MAJOR |
| 7 | **잠복 버그 발굴** — `EasiSettingKeys.All` 누락으로 토글 3종이 라이브 즉시 반영 안 되던 기존 결함, 회귀 가드까지 추가 | 근본수정 |
| 10 | 썸네일 비동기 디코딩 재진입 경쟁 | MAJOR |
| 11 | 공지 텍스트가 가사 포맷터를 타며 마커 손상 + `_liveItemId=null` 와일드카드로 공지 중 이동 버튼 오활성 — 2건 근본수정 후 **재검증 에이전트로 종결 확인** | MAJOR×2 |
| 18 | 빠른 전환 시 이전 애니메이션 Completed가 다음 클립을 지우는 경쟁 → 참조 가드 + InternalsVisibleTo 테스트 | MAJOR |
| 160-D2 | 시작 시 Refresh가 저장된 모니터 선택을 fallback으로 덮어쓰는 영속성 버그 | major |
| 160-E | 출력 VM `BodyTextVisibility`(외곽선 효과 게이트)를 그대로 바인딩하면 스테이지 모니터에서 가사가 사라지는 가시성 회귀 → 전용 컨버터 신설 | MAJOR |

리뷰 품질의 또 다른 증거: 증분 146·147에서는 APPROVE 사유가 "구조적으로 왜 안전한가"(키 버블링 경로, LiveSetting 의미론)까지 검증한 기록이고, 제안 중 일부는 **사유를 달아 기각**(147: 선언적 LiveSetting이 정답이라 코드비하인드 이벤트 발화 제안 미적용)했다 — 고무도장 리뷰가 아니라는 뜻.

| 장점 | 단점·리스크 |
| --- | --- |
| **리뷰 게이트의 ROI가 정량 입증** — 증분당 1회 리뷰로 송출 가시성·경쟁 조건급 결함을 수십 건 차단. "0 critical/major 통과" 기준이 명확해 게이트가 객관화됨 | **단일 에이전트 의존** — 보안(code-security-auditor)·UX(ux-ui-design-reviewer)·.NET 특화(dotnet-csharp-quality-reviewer) 관점은 카탈로그에 있으나 가동 증거 없음. code-reviewer 1종의 시야가 곧 품질 천장 |
| 구현-검증 분리 원칙(리뷰 세션 분리, 재검증 에이전트로 종결 확인)이 자기 채점 오류를 구조적으로 차단 | **카탈로그 비대** — 32종 중 ~60%가 도메인 무관. 선택 비용·혼선(예: WPF UI 작업에 frontend-developer가 웹 전제로 개입할 위험)만 남김 |
| Claude/Codex 양측 에이전트 정의 미러로 듀얼 에이전트 체제 대비 | 미러 유지비 — Codex TOML 26종이 untracked로 방치 중이고, 루트 AGENTS.md에는 일괄 치환 부작용("Claude와 Codex" → **"Codex와 Codex가 같은 파일을 동시에 수정하지 않는다"**)으로 Single Writer 조항이 무의미한 문장으로 변질된 결함이 실존 |

### 4.7 기타 보조 도구

- **agent-browser / ui-ux-pro-max**: 설치됨, 사용 흔적 없음. 전자는 웹 자동화라 WPF에 부적합, 후자는 웹 스택 편향(React/Vue/...)이라 XAML 작업과 접점 약함.
- **로드맵/갭 문서 자체가 사실상의 '스킬'**: `frmmain-port-roadmap.md`(225KB)·`frmmain-vs-wpf-detailed-gap.md`(98KB)·`gap-analysis.md`(72KB)·1:1 매핑표(59KB)가 계약-증거-진행기록을 겸했다. 효과는 §5, 비용은 §6 참조.
- **ADR 8건**(`docs/adr/`): WPF 프레임워크 선정, HookManager 보존, 레거시 UI 안전망 등 — 포팅 초기의 구조 결정이 추적 가능. SDD 도입 전부터 결정 기록 문화가 있었음을 보여준다.
- **메모리(자동 메모리 + 핸드오프 문서)**: 세션 간 연속성의 실질 담당. "Stop-hook 루프 현재 상태·다음 증분 후보·금지사항(반쪽 구현 금지, OpenCC 사전 없는 중문 변환 금지)"까지 인계 — 자율 루프 운영의 숨은 핵심 인프라.

---

## 5. 효과적·효율적이었던 적용 방식 (What worked)

### 5.1 "조사 먼저" 단계가 중복 작업을 구조적으로 차단
증분 158·159가 대표 사례: 착수 전 "이미 포팅됐는지"를 먼저 조사해 — 158은 Listing 기능 대부분이 기존 증분 44에 이미 존재(갭 문서가 stale)함을 발견하고 **실제 차이(RTF 출력)만** 추가, 159는 핵심(기본값 복원)이 이미 `ISettingsService.RestoreDefaults()`로 존재함을 확인하고 메뉴 진입점+재시작 경로만 신설. 갭 문서는 그 자리에서 정정. → 스펙 문서를 "읽는 것"이 아니라 **검증하며 소비**하는 패턴이며, 대규모 포팅에서 가장 흔한 낭비(이중 구현)를 막았다.

### 5.2 거짓 완료 금지 + 백로그 소진 루프 = 자율 실행의 안전한 형태
지속 지시("완료 주장은 남은 빨강/노랑 백로그로 반증되면 기각")와 Hard Rule("검증 증거 없는 완료 보고 금지")의 결합이, 자율 루프의 고질병인 **조기 완료 선언**을 막았다. 결과적으로 나흘간 160증분이 멈춤 없이, 그러나 매 증분 게이트를 통과하며 진행됐다. "자율성 = 게이트 제거"가 아니라 "자율성 = 게이트 통과의 자동 반복"으로 설계한 점이 핵심.

### 5.3 증분 입도(granularity)의 규율
한 증분 = 한 기능 슬라이스(UI·Interop·DB 혼합 금지) + 신규 테스트 + 리뷰 + 독립 커밋. 큰 기능은 명시적으로 분해(증분 2를 슬라이스 1~6으로, 160을 A~E 체인으로 — 체인 각 단계가 "상태머신→정규화→DI→UI→오버레이"라는 의존 순서). 거대 갭(§3.1 별도 Preview 모니터)도 5커밋으로 나뉘어 각각 reviewer를 통과했다.

### 5.4 재사용-우선 설계 결정의 명문화
160 체인의 결정 기록이 모범적: OutputWindowViewModel(1,381줄)·OutputWindow(422줄 XAML+848줄 전환엔진)을 **복제하지 않고** 정규화 순수함수(`StageSessionNormalizer`)+병렬 Host 미러+전용 surface로 해결, "출력 경로 무변경(외과 0)"을 매 단계 검증. "송출 안정성 > 구조적 아름다움" 원칙이 실제 설계 선택으로 번역된 사례.

### 5.5 진행 로그 = 증거 데이터베이스
로드맵 진행 로그가 증분마다 {무엇을·왜 그렇게 설계·reviewer 평결과 적발 내용·신규 테스트 수·누적 green·커밋 해시}를 한 줄로 압축 기록. 이 보고서가 가능한 것 자체가 그 증거 체계의 효용이다. 또한 "다음=160-C" 식 포인터로 세션 단절에도 재개 지점이 명확했다.

### 5.6 위험도별 강도 차등 (비용 효율의 핵심 장치)
Small(문구) → 게이트 생략, High-risk(DB·송출 좌표·Interop) → full TDD+guard+cso+qa. 모든 변경에 동일 의례를 요구하지 않음으로써 의례 피로를 막았고, 06-04의 TDD 게이트 완화도 같은 철학의 연장(엄격 모드 마찰의 실측 후 조정). **방법론을 고정 교리가 아니라 튜닝 대상으로 다룬 것**이 지속 가능성의 비결.

### 5.7 자동화는 "잊으면 안 되는 것"에만 투입
codegraph 자동 sync(PostToolUse), 세션 시작 상태 점검, injection 스캐너 — 사람이 망각하는 유지 작업만 훅으로 자동화하고, 판단이 필요한 게이트(리뷰·승인·푸시)는 명시적 단계로 남겼다. `/gstack-ship`을 자동 배포가 아닌 확인 게이트로 재정의한 것도 동일 원칙.

### 5.8 듀얼 에이전트(Claude+Codex) 거버넌스
같은 계약(CLAUDE.md ≡ AGENTS.md 미러)·같은 스킬 세트를 양측에 배치하고 Single Writer 규칙으로 충돌을 차단. 멀티 에이전트 환경의 계약 일원화라는 어려운 문제에 단순하고 실효적인 답을 택했다 (단, 미러 유지 결함은 §6.7).

---

## 6. 비효율·갭·리스크 (What needs improvement)

### 6.1 OpenSpec 라이프사이클 미완결 (가장 큰 구조적 갭)
sync/archive 0건, specs/ 빈 폴더 → change 디렉터리가 **단방향으로만 쌓이는 제안서 보관함**이 되어 간다. 10건 중 일부는 사실상 완료됐는데도 활성 상태라, "지금 유효한 계약 집합"을 알려면 사람이 10개 폴더를 읽고 교차 판단해야 한다. 이는 OpenSpec이 해결하려던 문제(스펙의 단일 진실원) 그 자체의 재발이다.

### 6.2 정책-실행 갭: tasks.md 양식
계약(CLAUDE.md)이 요구하는 Phase별 Goal/Scope/DoD/Tests/Constraints를 어떤 tasks.md도 갖추지 않았다. 양식이 과한 것인지(→ 계약을 현실에 맞게 완화), 실행이 게으른 것인지(→ 템플릿 자동 생성으로 비용 제거) 어느 쪽이든 **계약과 실제의 불일치는 계약 전체의 권위를 갉아먹는다.**

### 6.3 추적성 단절
커밋 메시지에 change-id·증분 번호 부재(4기 들어 영어 한 줄 제목으로 단순화되며 2~3기에 있던 §참조·증분 표기 관행이 소실). 로드맵 로그가 커밋 해시를 기록해 한 방향 추적은 되지만, 역방향(git → 계약)은 끊겼다.

### 6.4 문서 비대화와 컨텍스트 비용
로드맵 225KB·갭 문서 98KB+72KB·1:1 맵 59KB. 진행 로그의 가치는 §5.5대로 크지만, **단일 파일 무한 append 구조**라 이제 읽기 자체가 비용(이번 검토에서도 부분 읽기 강제). stale 정보 문제도 이미 실증됐다(158·159·§2.1 — 갭 문서가 완료 항목을 빨강으로 유지). 완료 증분의 아카이브 분리, 갭 문서의 "최종 검증일" 메타데이터가 필요한 시점.

### 6.5 설치 대비 활용률 — 도구 포트폴리오의 과적
gstack 52스킬(브라우저 계열 미검증), GSD ~190파일(본 플로우 미가동), 에이전트 32종(실사용 1종 집중), agent-browser/ui-ux-pro-max(도메인 부적합). **"언젠가 쓸 것"의 누적이 세션 인지 부하와 정책 단속 비용(메모리·CLAUDE.md에서 반복적으로 "GSD는 참고용"을 명시해야 하는 것 자체)으로 전가**되고 있다. 도구 도입의 기준이 "설치 가능한가"에서 "가동 증거를 만들 수 있는가"로 바뀌어야 한다.

### 6.6 훅 충돌·오버헤드
codegraph sync ↔ git index.lock 충돌(실측, 우회 관행 `sleep 4` 존재), GSD 훅 8종의 전 호출 개입. 개별로는 작아도 증분당 수십 회 도구 호출 × 나흘 160증분 규모에서는 누적 마찰이 된다. sync 훅에 git 작업 감지/지연 로직, GSD 훅 중 가치 있는 2~3종만 선별 유지가 필요.

### 6.7 규약 침식 (방치 시 계약 신뢰도 문제로 전이)
- 커밋 언어/형식 드리프트: 한글+컨벤셔널(`feat(wpf): … (§7.3-B)`) → 영어 한 줄 제목. CLAUDE.md §5의 커밋 예시와 현재 관행이 불일치.
- 루트 AGENTS.md의 치환 사고: "Codex와 Codex가 같은 파일을 동시에 수정하지 않는다" — **Single Writer 핵심 조항이 무의미해진 채 미커밋 상태로 존재.** 미러를 일괄 치환으로 만들면 안 된다는 교훈.
- `.claude/` gitignore로 Claude측 계약·스킬이 로컬 전용 → 머신 교체·협업자 합류 시 재현 불가(이미 메모리에 "per-machine reinstall needed" 자인).

### 6.8 검증 체인의 마지막 구간이 수동
빌드+테스트+리뷰는 자동화·게이트화됐지만, 송출 QA(멀티모니터 4케이스·좀비 프로세스·DB 동기화)는 체크리스트 기반 수동이며 **수행 기록이 저장소에 남지 않는다**(UAT 체크리스트 문서는 있으나 회차별 결과 로그 부재). gstack-qa가 이 구간을 메우지 못하는 환경 제약(§4.4)까지 겹쳐, "검증 증거" 원칙이 유일하게 빈 곳.

---

## 7. 종합 스코어카드

| 도구/스킬/에이전트 | 역할 | 실가동 | 효과(실증) | 비용/리스크 | 판정 |
| --- | --- | --- | --- | --- | --- |
| 증분 루프(조사→TDD→리뷰→기록→커밋) | 실행 방법론 | ●●● | 160증분/4일, +1,098 테스트 | 로그 비대화 | **A** — 이 프로젝트의 진짜 엔진 |
| code-reviewer 에이전트 | 품질 게이트 | ●●● | MAJOR급 수십 건 머지 전 차단, 기각 사유까지 기록 | 단일 관점 의존 | **A** |
| CodeGraph (+자동 sync 훅) | 구조 증거 | ●●○ | impact 7건, 탐색 규율 정착 | index.lock 충돌 | **B+** |
| OpenSpec (/opsx) | 계약 | ●●○ | change 10건, Non-goals 실효 | 후반 라이프사이클 0, 추적성 부재 | **B-** |
| Superpowers(개념)+완화 TDD 게이트 | 실행 규율 | ●●○ | 테스트 수 실측 증가, 명시적 튜닝 | 집행 장치 없는 자율 규범 | **B** |
| 로드맵·갭 문서 체계 | 사실상의 스펙+증거 DB | ●●● | stale 정정 루프 포함 작동 | 225KB 단일 파일 | **B+** |
| 메모리/핸드오프 | 세션 연속성 | ●●● | 재개 지점·금지사항 인계 | 인코딩 깨짐(가독성) | **B+** |
| gstack 52스킬 | 차단 게이트 | ●○○ | 가동 증거 빈약 | 브라우저 계열 미검증, 역할 중복 정리 비용 | **C** |
| GSD (강등+훅 8종) | 참고+가드 | ●○○(훅만 ●●) | injection-scanner 등 부수 가치 | 풋프린트·전 호출 오버헤드 | **C+** (흡수 원칙 자체는 A) |
| 도메인 에이전트 카탈로그 31종 | 전문 보조 | ○○○ | 미사용 | 선택 비용·도메인 불일치 | **C-** |
| agent-browser / ui-ux-pro-max | 웹 QA/디자인 | ○○○ | 미사용 | 도메인 부적합 | **D** |

---

## 8. 권고사항 (우선순위순)

1. **OpenSpec 후반부를 루틴에 편입** — 완료 change부터 `/opsx:sync` → `/opsx:archive` 1회전을 실제로 돌려 specs/를 단일 진실원으로 만들 것. 이후 "증분 완료 정의"에 archive 단계를 포함.
2. **커밋 ↔ 계약 추적성 복원** — 커밋 제목 또는 트레일러에 change-id/증분 번호 표기 재도입(2~3기 관행 복구). 비용은 0에 가깝고 감사 가능성은 크게 회복된다.
3. **tasks.md 양식 결단** — Phase 메타(Goal/DoD/Tests/Constraints)를 propose 시 자동 스캐폴드하거나, 계약 쪽을 "체크박스+커밋 해시"로 완화. 어느 쪽이든 계약=실행이 되게.
4. **도구 다이어트** — (a) 도메인 무관 에이전트 ~20종 제거 또는 별도 카탈로그로 격리, (b) gstack은 guard/freeze/review/cso 등 가동 증거를 만들 4~6종만 유지하고 브라우저 계열은 Chromium 검증 전까지 비활성 명시, (c) GSD는 injection-scanner 등 유효 훅만 독립 추출 후 본체 제거 검토.
5. **codegraph 훅의 git 안전화** — sync 실행 전 `.git/index.lock` 존재 시 대기/스킵 로직 추가로 `sleep 4` 관행 제거.
6. **로드맵 분권** — 완료 증분 로그를 연/월별 아카이브 파일로 분리, 갭 문서 항목에 "최종 검증일" 부여로 stale 탐지를 구조화.
7. **수동 QA의 증거화** — UAT 체크리스트 수행 결과를 회차별 기록 파일(`docs/wpf-migration/uat-runs/<date>.md`)로 남겨 "검증 증거" 원칙을 마지막 구간까지 관철.
8. **AGENTS.md 미러 결함 수정** — "Codex와 Codex" 치환 사고를 바로잡고, 미러는 치환이 아니라 생성 스크립트 또는 단일 원본+include 방식으로 전환.
9. **리뷰 관점 다변화(선택)** — High-risk 변경(송출 좌표·Interop·DB)에 한해 dotnet-csharp-quality-reviewer 또는 code-security-auditor를 2차 게이트로 추가해 단일 리뷰어 시야 한계를 보완.

---

## 9. 부록 — 증거 인덱스

| 주장 | 근거 |
| --- | --- |
| 커밋 분포 783건(12월 38·1월 34·2월 1·5월 230·6월 480), 06-04 이후 214건 | `git log` 월별 집계 |
| 증분 루프·리뷰 평결·테스트 수 추이(919→2,017) | `docs/wpf-migration/frmmain-port-roadmap.md` 진행 로그(증분 1~160-E, 커밋 해시 병기) |
| 3기 당시 gstack/GSD 미설치 | 같은 파일 머리말 "환경 미설치 → 표준 검증으로 대체" |
| 테스트 현황 148파일·마커 2,001개 | `Easislides.Wpf.Tests` 파일/어트리뷰트 집계 |
| OpenSpec change 10건·archive 0·specs 0·impact 7건 | `openspec/changes/*` 디렉터리 실사 |
| 4계층 계약·Hard Rules·위험도 매트릭스·TDD 완화 | `.claude/CLAUDE.md` §1, 루트 `AGENTS.md` |
| codegraph 자동 sync 훅 | `.claude/settings.json` PostToolUse |
| GSD 훅 8종 상시 등록 | `.claude/settings.local.json` hooks 절 |
| gstack Windows 셋업 행·우회, Superpowers 의도적 제외, `.claude/` 로컬 전용 | 메모리 `sdd-stack-installed` |
| TDD 게이트 완화 결정·집행 훅 부재 | 메모리 `tdd-gate-relaxed` |
| Stop-hook 루프 지시·index.lock 충돌·sleep 4 관행·증분 152~160-E 상세 | 메모리 `frmmain-port-session-handoff` |
| 2기 PR 루프·§참조 커밋 | `git log` 2026-05 (PR #61~69, `feat(wpf): … (§7.3-B)` 등) |
| AGENTS.md 치환 결함 | 루트 `AGENTS.md` "Codex와 Codex가 같은 …" (Hard Rules 절) |
| 에이전트 32종/Codex TOML 26종(untracked) | `.claude/agents/`, `.codex/agents/`, `git status` |
