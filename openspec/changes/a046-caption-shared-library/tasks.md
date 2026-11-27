## Phase 0. 기준과 영향

- Goal: 옮기기 전 결과를 기록해, 옮긴 뒤 같은지 비교할 수 있게 한다.
- [ ] 0.1 브랜치 `feat/a046-caption-shared-library`(지금 HEAD에서). Debug 빌드, `Axicultus.Tests` 전체와 `FullyQualifiedName~Caption` 결과(통과·건너뜀 수) 기록
- [ ] 0.2 하네스 `captiontab`, `pptcaption` 기준 로그(격리 사본, 실행 규칙은 `tools\AxicultusHarness\README.md`). 실행 전 사용자에게 창이 뜬다고 알린다
- [ ] 0.3 design 1의 의존 목록을 지금 코드와 대조(줄 번호 갱신, 빠진 것 추가)
- DoD: 기준 수치와 로그 이름이 이 문서 "검증 결과"에 있다.

## Phase 1. 묶음 A — 엔진

- Goal: `MainViewModel`을 모르는 자막 코드를 `Axicultus.Captions`로 옮긴다. Axicultus 동작은 그대로.
- Scope: design 3 묶음 A. `CaptionConsole`·`CaptionTabStrip`·`CaptionSettingsWindow`·`QueueRecoveryJournal`은 아직 앱에 둔다.
- [ ] 1.1 프로젝트와 참조, `Axicultus.sln` 등록, `InternalsVisibleTo`
- [ ] 1.2 호스트·라이브 인터페이스 초안(design 2.2, 2.3)과 라이브 상태 enum
- [ ] 1.3 엔진 파일 옮기기, 앱 형식 의존을 인터페이스·라이브러리 형식으로 바꾸기
- [ ] 1.4 자막 문구 resx와 표시 확장, 테마 기본 사전
- [ ] 1.5 앱 쪽 수정(`using`·xmlns·서비스 등록), 테스트 만들기 코드 수정
- Tests: 한국어·영어 키 집합이 옮기기 전과 같음, 테마 사전이 design 1의 키를 모두 가짐(밝은·어두운), 자막 테스트 전부
- DoD: Debug·Release 빌드 오류 0, `Axicultus.Tests` 전체가 0.1과 같은 통과 수(새 테스트만큼 증가), 라이브러리가 Axicultus 앱 어셈블리를 참조하지 않음
- Constraints: 식별자·주석 영어. 규칙을 고치지 않는다. 자막과 무관한 코드를 정리하지 않는다. 사용자의 로컬 `*.csproj` 변경이 있으면 커밋에 넣지 않는다.

## Phase 2. 묶음 B — 화면

- Goal: 자막 탭 화면을 라이브러리로 옮기고 Axicultus는 연결부로 붙인다.
- [ ] 2.1 `CaptionConsole`(두 partial)·`CaptionTabStrip`·`CaptionSettingsWindow` 옮기기, `MainViewModel` 사용을 호스트 인터페이스로 바꾸기
- [ ] 2.2 Axicultus 연결부(`MainViewModel` 구현), `QueueRecoveryJournal`은 앱에 남김, 본창·서비스 등록 연결
- [ ] 2.3 테스트 만들기 코드(`WithConsole`)와 하네스(`tools\AxicultusHarness`) 수정
- [ ] 2.4 가짜 호스트로 자막 탭을 여는 테스트: 화면 글 따라가기, 페이지 단계와 페이지로 가기, 고른 항목 올리기(송출 중에는 안 함), 송출 스위치
- [ ] 2.5 design 5에 확정된 인터페이스 기록
- Tests: 자막 테스트 전부(검사 내용 변경 없음), 2.4, 하네스 `captiontab`·`pptcaption`
- DoD: Debug·Release 빌드 오류 0(저장소 bin), 전체 테스트 통과, 하네스 두 단계가 0.2와 같은 결과, 실제 데이터 unchanged, `Axicultus\Captions\`에 연결부와 `QueueRecoveryJournal`만 남음
- Constraints: Phase 1과 같다. 자막 탭의 모양·키·저장 파일은 바뀌지 않는다.

## Phase 3. 회귀와 빌드

- [ ] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [ ] 3.2 하네스 그림(`captiontab-*.png`)을 0.2의 그림과 비교(탭 줄, 자막 탭, 라이브 탭)

## Phase 4. 리뷰와 기록

- [ ] 4.1 묶음마다 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 묶음마다 `/code-review`
- [ ] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/changes/archive/<날짜>-a046-caption-shared-library`, `openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

(구현 뒤 기록)
