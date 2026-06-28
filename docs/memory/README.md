# docs/memory — 경험 기억 로그 (사람이 읽는 층)

SDD 2층 기억 중 **경험 기억(Hindsight)** 의 사람이 읽을 수 있는 원본/후보/승격 기록.
정책은 `docs/sdd/hindsight-policy.md`, 승격은 `docs/sdd/promotion-gate.md` 참조.

| 폴더 | 내용 | 상태 |
| --- | --- | --- |
| `episodes/` | Hindsight에 retain한 경험의 원본 요약 (`<date>-<change-id>.md`) | 작업 기록 |
| `candidates/` | GBrain/ADR 승격 후보 (아직 공식 아님) | 미검증 |
| `promoted/` | 승격 완료 기록 (어디로 올렸는지 링크) | 공식화됨 |

흐름: `episodes/` → (후보 추림) `candidates/` → (게이트 통과) `promoted/` + GBrain/ADR.

retain: `./scripts/hindsight-retain-episode.ps1 -Path docs/memory/episodes/<file>.md`
