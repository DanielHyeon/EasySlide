전제: a047 Phase 0(하네스)이 커밋되어 있다. 같은 파일을 건드리는 다른 묶음과 동시에 하지 않는다.

## Phase 0. 기준과 영향

- [ ] 0.1 브랜치 `feat/a050-fix-branch-review-findings`. Debug 빌드, `Axicultus.Tests`·`Axicultuss.Tests` 전체 결과 기록
- [ ] 0.2 design "현재 구조"의 항목마다 지금 코드와 대조: 줄 번호, 부르는 곳, 이미 잡는 예외, 실제 스레드

## Phase 1. 실패하는 테스트

- [ ] 1.1 OBS: 읽기 실패(JSON 아님, 잠긴 파일) 뒤 자막 탭을 처음 봐도 `obs.json`이 그대로다. 운영자가 직접 연결하면 저장된다
- [ ] 1.2 장 넘기기: 호스트의 넘기기가 예외를 던져도 처리되지 않는 예외가 없고, 상태 줄 안내와 오류 기록이 남는다
- [ ] 1.3 라이브 알림: 다른 스레드에서 올린 알림이 세션의 UI 스레드에서 처리된다. 순서가 지켜진다
- [ ] 1.4 글상자 목록: `mc:AlternateContent`의 상자가 한 번만, 숨긴 상자는 안 나온다. 기존 PPT의 상자 키는 그대로다
- [ ] 1.5 곡 순서: 탭·줄바꿈으로 나눈 글, 실제 코드 열(양방향)
- [ ] 1.6 순서 글: 모르는 코드가 처음·가운데·끝에 있을 때
- [ ] 1.7 휴지통 되돌리기: 둘째 옮기기 실패 뒤 두 폴더가 처음 상태다. 다시 시도하면 사본 없이 되돌려진다
- DoD: 새 테스트가 고치기 전 코드에서 실패하는 것을 확인하고 출력을 기록한다. 실패하지 않는 항목은 고치지 않고 이유를 적는다.

## Phase 2. 고치기

- [ ] 2.1 `ObsOperator`와 `StartWhenUsed`(design 2)
- [ ] 2.2 `Step`과 같은 꼴의 처리기(design 3)
- [ ] 2.3 `CaptionSession.OnLiveChanged`(design 4)
- [ ] 2.4 `CaptionDeck` 상자 목록(design 5)
- [ ] 2.5 `SongExchangeSequence`(design 6), `gfLyrics` 순서 글(design 7)
- [ ] 2.6 `WorshipListStore.RestoreFromTrash`(design 8)
- [ ] 2.7 주석의 한글(design 9)
- Constraints: 식별자·주석 영어, 문구는 리소스(한국어·영어). 항목 밖의 동작을 바꾸지 않는다.

## Phase 3. 회귀와 빌드

- [ ] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [ ] 3.2 WPF 하네스 `captiontab`·`pptcaption`이 고치기 전과 같은 결과, Axicultuss 하네스 `smoke`

## Phase 4. 리뷰와 기록

- [ ] 4.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 `/code-review`
- [ ] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

(구현 뒤 기록)
