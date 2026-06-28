## 1. 0단계 - 근거 확보

- [x] 1.1 코드 수정 전 프로젝트 SDD hard gate를 확인한다.
- [x] 1.2 CodeGraph로 `UseIndividualFormatting` 관련 심볼을 식별한다.
- [x] 1.3 CodeGraph `impact`/`callers`로 `ToggleUseIndividualFormatting` 영향을 확인한다.
- [x] 1.4 `impact-map.md`, `risk-pattern-report.md`, `function-ast-summary.md`, `function-logic-map.md`, `branch-test-map.md`를 작성한다.

## 2. 1단계 - 실패 테스트

- [x] 2.1 `Notice`/텍스트 파일 항목에서 개별 설정 명령이 활성화되는지 검증하는 회귀 테스트를 추가한다.
- [x] 2.2 focused 테스트를 실행하고 실패를 기록한다.

## 3. 2단계 - 최소 수정

- [x] 3.1 실패 테스트를 통과시키는 데 필요한 WPF `Use Individual Settings` ViewModel 게이트만 수정한다.
- [x] 3.2 기존 직접 명령 테스트와 라이브 재송출 동작을 보존한다.

## 4. 3단계 - 검증

- [x] 4.1 관련 WPF focused 테스트를 실행한다.
- [x] 4.2 `openspec validate a013-wpf-use-individual-settings-toggle --strict`를 실행한다.
- [x] 4.3 verification과 failure-log를 갱신한다.
