# 다음 세션 실행 계획 — 남은 고위험·광범위 백로그

> 작성일: 2026-05-30 · 기준: `main` (PR #9/#13/#10/#12 머지 완료)
> 목적: 저위험·범위 명확한 항목을 모두 처리한 뒤 남은 **고위험·광범위** 항목의 안전한 실행 방안을 정의한다.

## 0. 직전 세션 완료 (참고)

| 항목 | PR | 상태 |
|---|---|---|
| EasiDS001 매직 색·폰트 분석기 (§9.2) | #9 | ✅ merged |
| EasiDS003 매직 간격 분석기 (§4.3) | #13 | ✅ merged |
| EsDataGrid 베이스 컨트롤 (§5.1 마지막) | #10 | ✅ merged |
| 접근성 전면 스윕 55건 + 가드 (§7.3) | #12 | ✅ merged |

통합 검증: `dotnet build Easislides.sln` 0 errors, 테스트 418개 통과(WPF 398 + 분석기 20), EasiDS00x 위반 0.

## 1. 남은 백로그 (모두 고위험 — 별도 세션·세심한 리뷰 필요)

### A. 컴포지트 독립 추출 (§5.2) — **고위험 (라이브 운영)**
현재 `WorshipListPanel`/`PptThumbStrip`/`MonitorMapper`/`BibleVerseFinder`/`MediaInsert`는
각 Window 안에 인라인되어 있다. §5.2는 재사용 컴포넌트로 분리를 요구.

- **위험**: MainWindow/OutputWindow 등 라이브 송출 경로 리팩토링 → 회귀 시 예배 중 사고.
- **권장 접근**:
  1. 가장 위험이 낮은 1개(`PptThumbStrip` 썸네일 가상화 리스트)부터 시범 추출.
  2. ViewModel은 그대로 두고 View만 `UserControl`로 분리(동작 동등성 우선).
  3. 추출 전후 동일 ViewModel 단위 테스트로 동등성 고정 → 그 다음 View 교체.
  4. MainWindow 핵심 라이브 경로는 마지막에, `--legacy-ui`/스크린샷 회귀 갖춘 뒤 착수.

### B. 운영 앱 전환 / `--legacy-ui` 안전망 (M6, ADR-0007) — **고위험 (아키텍처)**
WPF 앱은 아직 production entrypoint가 아니다(데모/기반 단계).

- **권장 접근**:
  1. 산출물 분리: `Easislides.exe`(legacy) / `EasislidesNext.exe`(신규) — §9.4.
  2. `--legacy-ui` CLI 안전망 구현(롤백 경로) — ADR-0007, M3까지 유지.
  3. 공통 도메인 어셈블리 `Easislides.Core.dll` 추출 검토(양쪽 참조).
  4. 1시간 예배 리허설 시나리오 통과를 전환 게이트로.

### C. 스크린샷 회귀 자동화 (§9.1, Sprint 4+) — **중위험 (CI 인프라)**
라이트/다크 양쪽 모드 시각 회귀 자동 캡처가 없다.

- **권장 접근**: 헤드리스 렌더 + 기준 이미지 비교(예: 컨트롤 갤러리/주요 창 N개)부터 시작.
  CI 환경에서 WPF 렌더 가능 여부 PoC 선행.

### D. 접근성 2차 정밀화 (§7.3 보강) — **✅ 완료 (PR #15/#16/#17 리뷰 대기)**
1차 스윕은 `AutomationProperties.Name`을 부여했다. 2차로 다음을 완료:
- ✅ 입력 필드를 인접 라벨에 `AutomationProperties.LabeledBy`로 연결(중복 Name 21개 전환 + 무결성 가드) — **PR #16**.
- ✅ 라이브 상태 동적 영역 적용 — WPF 대응 `AutomationProperties.LiveSetting=Assertive` + `LiveRegionChanged` 이벤트, LiveBar·EsToast 커스텀 자동화 피어 — **PR #15**.
- ✅ `Controls/` 템플릿 파트 자동화 트리 노출 정책 정형화(블랭킷 제외 → PART_*/Focusable=False 면제 규칙 + 가드) — **PR #17**.
- ⏳ 잔여(수동): NVDA/Narrator 실제 스크린리더로 LiveRegion·ControlType 동작 확인.

## 2. 권장 착수 순서

1. ~~**D(접근성 2차)**~~ — ✅ 완료 (PR #15/#16/#17). 잔여는 NVDA/Narrator 수동 검증뿐.
2. **C(스크린샷 회귀)** — 이후 리팩토링 안전망이 되므로 먼저 구축. ← **다음 착수**
3. **A(컴포지트 추출)** — C 안전망 위에서 1개씩.
4. **B(운영 앱 전환)** — 가장 마지막, 리허설 게이트 통과 후.

## 3. 세션 시작 체크리스트

```powershell
# PATH 재로딩(필요 시)
$env:Path = [Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')
# 기준선 green 확인
dotnet build Easislides.sln -c Debug -nologo -v minimal
dotnet test  Easislides.sln -c Debug -nologo -v minimal
```

- 기준선: 빌드 0 errors, 테스트 418개 통과(작업 D 3 PR 병합 후 429개 — a11y 가드 +11).
- 작업 원칙: TDD(실패 테스트 우선) → 구현 → 리뷰(`dotnet-csharp-quality-reviewer`) → 검증(build/test) → 작은 단위 커밋 + PR.
- 디자인 토큰/접근성은 EasiDS001/003 분석기 + 접근성 가드 테스트가 자동 강제하므로, 신규 코드가 위반하면 빌드/테스트가 잡아낸다.
