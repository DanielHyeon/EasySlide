# Architecture Decision Records (ADR)

> EasiSlides v3.0 UI/UX 현대화 관련 아키텍처 결정 기록.
> 본 ADR들은 [`docs/ui-ux-modernization-plan.md`](../ui-ux-modernization-plan.md) v1.1 §0.5 (Q1~Q8 결정)에 근거.

## ADR 인덱스

| ID | 제목 | 상태 | 관련 결정 |
|---|---|---|---|
| [ADR-0001](0001-wpf-ui-framework.md) | WPF + WPF UI 프레임워크 채택 | Accepted | Q1 |
| [ADR-0002](0002-fluent-icons.md) | Fluent UI System Icons 아이콘 셋 채택 | Accepted | Q6 |
| [ADR-0003](0003-pretendard-font-bundling.md) | Pretendard Variable 한국어 폰트 번들링 | Accepted | Q4 |
| [ADR-0004](0004-hookmanager-preservation.md) | HookManager 전역 후킹 보존 (vs WPF InputBinding) | Accepted | (Q5 키보드 안전성) |
| [ADR-0005](0005-options-decomposition.md) | FrmOptions 단일 모달 → Settings 페이지 분해 | Accepted | §6.4 |
| [ADR-0006](0006-senior-mode-token-scale.md) | 시니어 모드 토큰 스케일 함수 (vs 별도 테마) | Accepted | Q2 |
| [ADR-0007](0007-legacy-ui-safety-net.md) | `--legacy-ui` 안전망 유지 기간 (M3까지) | Accepted | Q8 |

## ADR 작성 규칙

- **포맷**: [MADR (Markdown ADR)](https://adr.github.io/madr/) 1.x 변형
- **번호**: 4자리 zero-pad (`0001`~`9999`)
- **상태**: `Proposed` → `Accepted` → `Deprecated` → `Superseded by ADR-XXXX`
- **불변성**: 한 번 Accepted 된 ADR은 수정하지 않음. 결정 변경 시 새 ADR 작성 + 기존 ADR을 `Superseded` 표시.
- **언어**: 한국어 본문, 영문 고유명·인용은 그대로 (CLAUDE.md §9 규정 — 문서는 한글 허용).

## 결정 그래프

```
계획서 §0/§0.5 (Q1~Q8)
    │
    ├── ADR-0001 (WPF UI) ─── 영향 ───→ ADR-0002, ADR-0003, ADR-0006
    ├── ADR-0002 (아이콘)
    ├── ADR-0003 (폰트)
    ├── ADR-0004 (HookManager) ─── 영향 ───→ Sprint 0 PoC-A
    ├── ADR-0005 (Options 분해)
    ├── ADR-0006 (시니어 모드)
    └── ADR-0007 (Legacy 안전망) ─── 영향 ───→ §10.5.2
```

## 변경 이력

- 2026-05-27: 초기 7개 ADR 작성 (v1.1 리뷰 합의 lock-in).
