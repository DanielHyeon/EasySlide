# EasiSlides v2.6.x — 핵심 요약 + SDD 운영 정책(GBrain 통합)

## 0. 미션 / 스택

교회 예배용 워십 슬라이드 송출 프로그램. 찬양 가사·성경 구절·PPT를 멀티 모니터로 송출하고 예배 순서(WorshipList)에 따라 즉시 전환한다.

- **런타임**: C# / .NET 10 (`net10.0-windows7.0`), WinForms(`WinExe`) — WPF(`Easislides.Wpf`) 신규 포팅 진행 중
- **솔루션**: `Easislides/`(메인) · `OfficeLib/`(PPT/Word Interop) · `DirectShow/`(영상) · `Easislides.Wpf/`(WPF 포팅)
- **주요 NuGet**: MySql.Data 9.1, MySqlConnector 2.4, NetOffice.PowerPoint/Word 1.7.4.11, System.Data.SQLite.Core 1.0.119
- **DB**: 로컬 SQLite(찬양/성경) + 선택적 MariaDB/MySQL 동기화. `DefineConstants=ODBC, SQLite`

---

## 1. SDD 운영 정책 (GBrain 통합판 · 최우선 규칙)

스택은 **GBrain(기억) + OpenSpec(계약) + CodeGraph(증거) + Superpowers(TDD 실행) + gstack(게이트)**.

> OpenSpec은 계약, CodeGraph는 증거, Superpowers는 TDD 실행, gstack은 게이트며, **GBrain은 이 모든 과정을 연결하는 장기 기억(Memory)**이다.

### 핵심 철학
- AI 코딩을 무제한 코드 생성기가 아니라 **통제된 엔지니어링 워크플로**로 사용한다.
- 요구사항·영향 분석·구현·품질 게이트를 각각 **분리된 책임**으로 유지한다.
- 명시적 완료 조건(DoD)·제약·검증 증거를 갖춘 **작고 단계(phase) 기반**의 변경을 선호한다.
- 테스트·빌드·리뷰·재현 가능한 QA 증거 같은 **구체적 검증 없이 "완료"로 보고하지 않는다**.
- 모든 작업의 컨텍스트와 최종 결과물은 휘발되지 않도록 **장기 기억(GBrain/메모리)에 축적하고 다음 작업에 재활용**한다.

### 책임 분리
| 계층 | 도구 | 책임 |
| --- | --- | --- |
| 기억 | GBrain | 전역 맥락 관리, 과거 아키텍처 의사결정 회고, 변경 이력 지식 그래프 공급 |
| 계약 | OpenSpec(또는 승인된 구현 계획) | 범위·비목표(non-goals)·수용 기준·설계·승인 이력 (`openspec/changes/<id>/`) |
| 증거 | CodeGraph | 구조적 영향, 호출자/피호출자, 영향 받는 심볼·테스트 |
| 실행 | Superpowers | OpenSpec `tasks.md`의 Phase를 하나씩 TDD로 구현·디버깅·1차 리뷰 (Red→Green→Refactor→Verify) |
| 게이트 | gstack | guard · freeze · review · security(cso) · qa · ship 준비 게이트 |

**도구 상태(2026-06-04 설치·검증 / 2026-06-19 GBrain 정책 반영)**:
CodeGraph 0.9.4(`.codegraph/codegraph.db`, MCP `codegraph_*`) · OpenSpec 1.4.1(`openspec/`, 커맨드 `/opsx:*`) · gstack(`/gstack-*` 접두사 스킬). **GBrain(기억 계층)** = MCP `gbrain_*` 서버(garrytan/gbrain). **설치·등록 완료(2026-06-19)**: 로컬 PGLite 브레인 `~/.gbrain`, 소스 체크아웃 `C:\Users\Admin\.local\src\gbrain`, Claude Code MCP **user 스코프** 등록(✓connected, 새 세션부터 `gbrain_*` 로드), `gbrain` CLI는 `~/.local/bin` 셰임. 임베딩(시맨틱 검색)은 **로컬 Ollama**(`nomic-embed-text` 768d, API 키 불필요)로 활성화됨 — Ollama 서버(`localhost:11434`, 로그인 시 자동 기동)가 떠 있어야 함. LLM 쿼리 확장은 채팅 키가 없어 conservative 모드(벡터 검색엔 영향 없음). (Bun이 사내 프록시 루트 CA 미신뢰 → `NODE_EXTRA_CA_CERTS`에 Windows 루트 PEM 지정해 우회.) Superpowers도 동일 — 미설치 시 같은 TDD/리뷰 규칙을 수동 적용한다. 런타임: Node 24.16 / npm 11.13 / bun 1.3.14. Claude·Codex 양쪽 구성됨.

### 우선순위 (충돌 시 위가 이김)
1. 현재 사용자 지시
2. GBrain(또는 파일 메모리)이 공급한 장기 기억 컨텍스트
3. 프로젝트 로컬 계약 — `openspec/changes/<change>/{proposal,design,tasks,codegraph-impact}.md` → `openspec/specs/` · `openspec/config.yaml`
4. CodeGraph impact 결과
5. Superpowers TDD 실행 결과 / phase별 검증 증거
6. gstack 리뷰 결과 / 이 SDD 정책

### Hard Rules
- **production code 변경 전 기억 회고**: `gbrain_recall`(미구성 시 파일 메모리·`MEMORY.md`)로 관련 과거 히스토리·선호 아키텍처 패턴·장애 이력을 먼저 확인하고 계약(Spec)에 사전 반영한다.
- 승인된 OpenSpec change 없이 production code를 바꾸지 않는다.
- CodeGraph impact 없이 공유 심볼(헬퍼·Interop·DB)을 고치지 않는다.
- public API/스펙 변경엔 spec delta, DB 변경엔 rollback 계획을 동반한다.
- Phase 계획은 OpenSpec `tasks.md`가 소유한다. 각 Phase는 merge 가능한 작은 단위여야 하며 UI·Interop·DB·리팩터를 한 Phase에 섞지 않는다.
- Superpowers는 승인된 Phase 하나만 TDD로 실행한다. Phase를 임의로 추가하거나 OpenSpec의 scope, non-goals, acceptance criteria를 바꾸지 않는다.
- **작업 종료 시 기억 저장**: `/opsx:archive` 후 이번 사이클의 교훈·변경된 구조·핵심 의사결정 요약을 `gbrain_store`(미구성 시 파일 메모리)로 영구 동기화한다.
- 검증 증거 없는 "완료" 보고 금지. 다 안 했으면 다 했다고 하지 않는다.
- Claude와 Codex가 같은 브랜치의 같은 파일을 동시에 수정하지 않는다(Single Writer).

### 표준 워크플로우 (기억 → 계약 → 증거 → 실행 → 게이트 → 기억 아카이브)
```
gbrain_recall  (과거 작업·유저 선호·장애 디버깅 이력 회고)
→ /opsx:explore → /opsx:propose <id>  →  CodeGraph impact 작성  →  사람 승인
→ OpenSpec tasks.md에 Phase plan 작성 (Goal / Scope / Tasks / DoD / Tests / Constraints)
→ /gstack-guard → /gstack-freeze <허용 경로> → Superpowers로 Phase <N>만 TDD 실행
→ /gstack-review → /gstack-cso → /gstack-qa → OpenSpec DoD 확인
→ /opsx:sync → /opsx:archive → gbrain_store(교훈·구조·의사결정 영구 저장) → /gstack-ship
```
구현 세션과 리뷰 세션을 분리하고, 긴 작업 뒤 `/clear` 후 review-only로 검증한다. (gstack 명령은 모두 `/gstack-` 접두사 — flat `/review` 등 내장 스킬과 충돌 회피)
`/gstack-ship`은 자동 push/deploy가 아니라 ship 가능 여부 확인 게이트로 다룬다. 실제 push/deploy는 사람이 diff·테스트 결과 승인 후 수행한다.

### 빠른 명령 레퍼런스
```
기억   gbrain_recall · gbrain_store · gbrain_search_graph   (미구성 시 파일 메모리 MEMORY.md)
계약   /opsx:explore · /opsx:propose "<의도>" · /opsx:apply · /opsx:sync · /opsx:archive · openspec list
증거   codegraph_search/callers/callees/impact/context/trace · codegraph sync
실행   Superpowers (brainstorming · TDD · systematic-debugging) — Red→Green→Refactor→Verify
게이트  /gstack-guard · /gstack-freeze · /gstack-review · /gstack-cso · /gstack-qa · /gstack-ship
검증   dotnet build / dotnet test (green 유지) + 수동 송출 QA
```

### Codex SDD 실행 가이드
Codex는 구현 에이전트로 동작한다. 기본 순서는 다음과 같다.

1. **기억 회고**: production code 변경 전 `gbrain_recall`(미구성 시 `MEMORY.md`)로 관련 과거 히스토리·유저 선호 패턴·장애 이력을 확인하고 계약에 반영한다.
2. **계약 확인**: `openspec/changes/<change-id>/proposal.md`, `design.md`, `tasks.md`, `codegraph-impact.md`를 먼저 확인한다.
3. **영향 증거**: 구조 질문과 공유 심볼 영향은 CodeGraph(`codegraph_context`, `codegraph_impact`, `codegraph_callers`)로 확인한다.
4. **TDD 실행**: 새 동작·버그 수정은 실패 테스트를 먼저 만들고 expected fail을 확인한 뒤 최소 구현한다.
5. **게이트**: `openspec validate --all --no-interactive`, 관련 `dotnet build/test`, code-review/gstack, 수동 송출 QA 증거를 남긴다.
6. **종결 + 기억 저장**: 완료 change는 `/opsx:sync`·`/opsx:archive` 대상인지 확인하고, 이번 사이클 교훈·구조·의사결정을 `gbrain_store`(미구성 시 `MEMORY.md`)로 영구 저장한다.

### Phase 기반 실행 원칙
변경은 작은 Phase 단위로 실행하며, Phase 계획은 OpenSpec `tasks.md`가 소유한다.

각 OpenSpec change는 Phase 단위 implementation plan을 포함해야 한다.
- Phase 0: baseline·impact 확인 (+ 기억 회고 `gbrain_recall`)
- Phase 1: acceptance criteria별 실패 테스트 작성 및 expected fail 확인
- Phase 2: 테스트를 통과시키는 최소 production code 구현
- Phase 3: CodeGraph 영향 테스트·통합·회귀 검증
- Phase 4: Superpowers 1차 리뷰 + gstack review/cso/qa/ship gate (+ 기억 저장 `gbrain_store`)

각 Phase는 `Goal`, `Scope`, `Tasks`, `DoD`, `Tests`, `Constraints`를 명시한다. Superpowers에는 "OpenSpec `tasks.md`의 Phase N만 실행하라"고 지시하고, Phase DoD 충족 후 멈춘다.

### 변경 위험도별 강도
| 유형 | GBrain | OpenSpec | CodeGraph | Superpowers / Phase | gstack |
| --- | --- | --- | --- | --- | --- |
| Small(문구·단일 검증) | recall(선택) | light | search/affected | 생략/light | 생략 |
| Normal(서비스 일부) | recall+store | full | impact/context | Phase plan + TDD exec + verify | review |
| High-risk(DB·송출 좌표·Interop) | recall+store(필수) | full | impact+callers+affected | full TDD + Phase stop point | guard+review+cso+qa |
| Hotfix | store(필수) | 사후 sync 필수 | impact 최소 | verify 중심 | review |

### EasiSlides OpenSpec change에 반드시 답할 것
사용자 흐름(찬양/성경/PPT/순서 중 무엇) · 송출 영향(미리보기만? 실제 송출도?) · 멀티모니터 케이스(PRIMARY/선택/수동좌표/None) · Office Interop COM 생성·해제 영향 · DB 경유 원칙 준수 · 책임 파일(FrmMain partial / gf*.cs) · 검증 방법.

### 검증 = 실제 게이트 (도구 + 빌드/테스트)
이 저장소에서 "완료" 증거 = **빌드 + 테스트 + 리뷰 + 수동 QA**.
```powershell
dotnet build Easislides\Easislides.csproj -nologo -v minimal   # dotnet 없으면 PATH 재로딩 후 재실행
dotnet test Easislides.Wpf.Tests                                # 전체 green 유지
```
- 구현 후 `code-reviewer` 에이전트 또는 `/gstack-review`·`/gstack-cso`. Release 빌드 시 DLL/BAML 심볼 반영 확인.
- 수동 송출 QA: 찬양·성경 검색 / WorshipList 전환 / PPT 썸네일 생성 / 싱글·멀티·None·수동좌표 송출 / PowerPoint·Word 좀비 프로세스 미발생 / SQLite·MariaDB 동기화 회귀 없음.

### Superpowers TDD 게이트 (완화 — 2026-06-04, 기본 정책)
테스트/마커 요구는 **새로운 동작 또는 기존 동작의 변경에만** 적용한다(휴리스틱). 사소한 리팩터까지 전부 테스트를 요구하던 엄격 모드를 완화함.
- **면제(테스트 불요)**: 순수 리네임·서식·주석·코드 이동·import 정리 등 **동작 불변 리팩터링**, 설정/문서 변경.
- **요구**: 새 기능, 버그 수정, 로직·계약·경계 조건이 바뀌는 변경.
- **Phase 실행 규칙**: 요구 대상 변경은 Superpowers가 먼저 실패 테스트를 작성하고 expected fail을 확인한 뒤 최소 구현으로 통과시킨다.
- **비상 탈출구**: 면제 대상인데 도구/리뷰가 테스트를 요구하면 커밋에 `Test-Needed: no` 트레일러 + 한 줄 사유로 통과.
- 경계가 모호하면 테스트 추가 쪽으로. **불변 규칙은 유지** — 빌드 + `dotnet test` green 유지, "검증 증거 없는 완료 보고 금지".

---

## 2. 프로젝트 구조 (압축)

```
Easislides/Easislides/   FrmMain + 모든 다이얼로그(Frm*.cs ~40개)
Easislides/Global/       기능별 정적 헬퍼 gf*.cs (gf→Gf 리네이밍 진행 중)
Easislides/Module/       도메인 모델(CommonEnum, SongFormat, SongLyrics, SongSettings)
Easislides/Util/         공통 유틸(CommonUtil, DisplayInfo, FileUtil, RegUtil 등)
Easislides/SQLite/       SQLiteController (로컬 DB 접근)
Easislides/HookManager/  전역 키보드/마우스 후킹
OfficeLib/               PPT/Word Interop 래퍼(BuildScreenPreDumps 등)
Easislides.Wpf/          WPF 포팅(신규) + Easislides.Wpf.Tests
docs/adr/, docs/wpf-migration/   ADR·갭분석·로드맵(포팅 설계 산출물)
openspec/                SDD 산출물(OpenSpec change · proposal/design/tasks/codegraph-impact)
```

**FrmMain** = 거대 partial: `FrmMain.cs`(~8.8k줄, 핸들러) · `.Designer.cs` · `.Fields.cs` · `.Events.cs` · `.Layout.cs` · `.Logic.cs`(~3.1k줄, 비즈니스 로직).

**gf*.cs 책임**: gfBible(성경) · gfLyrics(가사) · gfMedia(미디어) · gfPowerPoint(PPT, PreviewPPT) · gfDisplay(렌더링 FormatText) · gfImages · gfFileIO/gfIO/gfFileHelpers(파일) · gfDatabase(DB) · gfConfig(설정) · gfConstants(상수·LS_Width 등 송출 변수) · gfFolder(경로) · gfColorsFonts · gfUiText · gfUtility(잔여) · gf.cs(진입점) · PerfLog.

---

## 3. 핵심 송출 흐름 + 멀티모니터

```
WorshipList 선택 → WorshipListIndexChanged → LoadItem
  → BuildAllPowerpointScreenDumps → gf.PreviewPPT.BuildScreenPreDumps  (동기 Export = 병목)
  → ShowPreviewPPThumbs → LoadThumbPreviewImages
  → ImageCanvas.BuildNewImageThumbs → Calc_Image + Invalidate
```
병목: PowerPoint Interop **동기 Export**(슬라이드당 수백ms~수초) + 썸네일 동기 로딩 → 비동기화·지연 로딩 권장.

**멀티모니터 규칙**: 싱글=해당 모니터 / 멀티=선택 모니터(선택값 저장) / 멀티+싱글모드=PRIMARY / 수동영역=좌표·크기 / None=송출 안 함. 변수 `LS_Width LS_Height Buffer_LS_Width selectScreen`. ⚠️ 과거 회귀: `selectScreen==null`일 때 `LS_Width=0` → `FormatText` 0 나눗셈 (가드 코드 존재, 반드시 유지).

---

## 4. CodeGraph 우선 규칙

`.codegraph/codegraph.db` (AST 심볼 그래프, MCP `codegraph_*`). 구조 질문은 파일을 직접 읽기 전 **먼저 조회**. 단, 문자열·문구·로그·주석 같은 literal 확인이나 이미 특정 파일이 열린 경우는 `rg`/파일 읽기가 우선이다.

| 의도 | 도구 |
| --- | --- |
| 심볼 정의 | `codegraph_search "심볼"` |
| 호출자/피호출자 | `codegraph_callers` / `codegraph_callees` |
| 변경 영향 범위 | `codegraph_impact "심볼"` |
| 태스크 컨텍스트 | `codegraph_context "작업 설명"` |
| 흐름 추적(A→B) | `codegraph_trace` |
| 여러 심볼 본문 일괄 | `codegraph_explore` |

**금지**: 심볼 찾기용 `grep -r "함수명"`(→ search), 파일 전체 읽어 import 추적(→ callers), 관련 파일 수동 탐색(→ context). 결과는 강력한 힌트이나 dynamic/DI/reflection은 테스트로 보완. 인덱스는 수정 후 자동 sync(PostToolUse 훅), 수동 시 `codegraph sync`.

---

## 5. 코딩 / 금지 규칙

- **한글 식별자·주석** — 초등학생도 이해할 수준. 단, 기존 스타일 일관성 우선.
- **작은 단위 커밋 + PR** (예: `feat(wpf): … (증분160-E)`). 문서·코드·테스트 일치, 문서 없이 기능 추가 지양(TDD 우선).
- **백업 파일(`.bak`, `.bak2`…) 임의 삭제 금지** (리팩토링 중 의도적 생성).
- **거대 파일은 partial/기능별 분할** 우선. 새 메서드는 적합한 `gf*.cs`를 먼저 찾고 없을 때만 새 파일.
- **gf → Gf 리네이밍은 별도 refactor phase로 분리** (기능 변경과 섞지 않음).
- **NetOffice/DirectShow 리소스 명시적 해제** — `Marshal.ReleaseComObject` / `using` 유지 (좀비 프로세스 방지, 2.4.3에서 수정 이력).
- **DB 접근은 `SQLiteController.cs` / `gfDatabase.cs` 경유** — 임의 위치에 새 connection 만들지 말 것.
- **송출 크기/좌표 변경 시** `LS_Width LS_Height Buffer_LS_Width selectScreen` 영향 + `selectScreen null`/`LS_Width=0` 회귀 검증.
- **최우선 원칙**: 예배 중 **송출 안정성 > 구조적 아름다움**, **작은 외과 수술 > 대규모 리팩터링**, **검증 증거 > 말뿐인 완료**.

---

## 6. 자주 보는 파일

| 무엇 | 어디 |
| --- | --- |
| 앱 진입점 | `Easislides/Easislides/Program.cs` |
| 메인 폼 | `Easislides/Easislides/FrmMain.cs` |
| 송출 캔버스 | `Easislides/Easislides/ImageCanvas.cs` |
| PPT Interop | `OfficeLib/PowerPoint.cs` (BuildScreenPreDumps, IsBuildedFileCheck) |
| DB 접근 | `SQLite/SQLiteController.cs`, `Global/gfDatabase.cs` |
| 전역 키보드 후킹 | `HookManager/HookManager.cs` |
| 다이얼로그 | `Easislides/Easislides/Frm*.cs` (Bible/Find/Import/Export/Options 등) |
| 포팅 설계 | `docs/adr/*`, `docs/wpf-migration/*` |
| SDD 산출물 | `openspec/changes/*` (proposal/design/tasks/codegraph-impact) |
