# 기억 승격 게이트 (Memory Promotion Gate)

경험 기억(Hindsight)에서 **검증된 교훈만** 공식 기억(GBrain canonical / ADR / OpenSpec
archive)으로 올리는 심사대. 이 게이트가 없으면 Hindsight 경험이 쌓이며 노이즈가 되고,
미검증 경험이 공식 규칙으로 새어 들어간다.

## 1. 승격 흐름

```text
Hindsight episode / reflection
    ↓  (작업 종료 시 후보 추림)
docs/memory/candidates/<change-id>.md
    ↓  (사람 또는 gstack 검토)
GBrain canonical note  /  docs/adr/ADR-*  /  openspec archive 요약
    ↓
docs/memory/promoted/<change-id>.md   (승격 기록 보관)
```

- `docs/memory/episodes/` — Hindsight에 넣은 경험의 원본(사람이 읽는 로그).
- `docs/memory/candidates/` — 승격 후보(아직 공식 아님).
- `docs/memory/promoted/` — 승격 완료 기록(어디로 올렸는지 링크 포함).

## 2. 승격 조건 (모두 충족해야 canonical)

`.claude/CLAUDE.md` 의 write-back 규칙과 동일 기준을 적용한다.

1. OpenSpec 변경 승인됨.
2. Superpowers Verify 통과 (RED→GREEN→refactor 증거).
3. 영향 테스트 통과 (`dotnet build` + `dotnet test` green).
4. gstack review / security 통과.
5. ship/merge 됨.
6. 교훈이 **재현 가능한 사실**로 정리됨 (단발 추측 아님).

위를 모두 만족하지 못하면 candidate에 머문다. 미검증 메모는 절대 canonical로 올리지 않는다.

## 3. 무엇을 어디로 승격하나

| 교훈 유형 | 승격 대상 |
| --- | --- |
| 반복 금지 코딩 패턴 · 함수 불변식 · 장애 판례 | GBrain canonical note (`gbrain put` / `mcp__gbrain__put_page`) |
| 장기 아키텍처 결정 | `docs/adr/ADR-*.md` |
| 변경 계약·수용 기준의 결론 | OpenSpec archive 요약 |
| 새로 막아야 할 위험 구조 | ast-grep 룰 후보 (`ast-grep/rules/`) + GBrain note |

## 4. 승격하지 않는 것

- 미검증 추측 · 단발성 시행착오(다음에도 반복될지 불확실).
- 코드/대화 전문 · 비밀(키·토큰·개인정보).
- OpenSpec과 충돌하는 임시 판단.
- 이미 코드/CLAUDE.md/ADR에 있는 사실(중복).

## 5. 충돌 시

승격된 canonical과 현재 코드/spec이 다르면 **GBrain 요약이 아니라 인용 원본**(spec/ADR/
코드/테스트)을 따른다. 과거 경험은 commit SHA·시점이 붙은 historical evidence로만 쓴다.
