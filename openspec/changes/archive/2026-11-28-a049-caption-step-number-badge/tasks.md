전제: a046 묶음 B가 끝나 있다(자막 탭 화면이 `Axicultus.Captions`에 있다).

## Phase 0. 기준과 영향

- [x] 0.1 브랜치 `feat/a049-caption-step-number-badge`(a046 뒤의 HEAD에서). Debug 빌드, 자막 테스트 결과 기록
- [x] 0.2 design "현재 구조"를 지금 코드와 대조, 단계 이름표를 보는 테스트 목록

## Phase 1. 실패하는 테스트

- [x] 1.1 번호 글: 악보 줄 `1-1`·`1-2`·`2-1`, 줄 없는 장 `3`, 페이지 단계 `2/3`. 화면 읽기 이름은 말로 된 이름 그대로

## Phase 2. 고치기

- [x] 2.1 `CaptionStep`의 번호 글과 말로 된 이름, 단계 만들기
- [x] 2.2 단계 카드 틀: 이름표 줄 삭제, 오른쪽 위 번호(design 2, 3)
- Constraints: 식별자·주석 영어. 단계 목록의 동작과 카드 크기는 그대로.

## Phase 3. 회귀와 그림

- [x] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [x] 3.2 검토용 그림: 밝은·어두운 테마, 악보 줄·글·페이지 단계, 지금 단계와 다음 단계 테두리

## Phase 4. 리뷰와 기록

- [x] 4.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [x] 4.2 `/code-review`
- [x] 4.3 총괄 완료 검증(그림 확인)과 "검증 결과" 기록
- [x] 4.4 커밋·푸시, 아카이브(`openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

구현(Phase 0~3, 2026-11-28):

- 0.1 기준(154cd5d, 바꾸기 전): Debug 빌드 오류 0. `Axicultus.Tests` 3755 통과 / 22 건너뜀, `Axicultuss.Tests` 478 / 2.
- 0.2 design "현재 구조"는 코드와 같다(파일은 `Axicultus.Captions\`). 이름표를 보는 테스트: `CaptionConsoleTests`의 `TheNextStepIsTheNextRowThenTheNextSlideAndNothingAtTheEnd`·`WhileTheScoreIsMadeTheNextStepIsTheNextSlide`·`PickingAStepInTheStripMovesOnlyTheCaption`·`AScoreStepGoesToItsRowInOneCaption`(값이 바뀜), `WithTheLiveTabShownTheCaptionKeepsFollowingTheScreen`·`AnotherHostsPagesAreTheStepsAndAPickedStepGoesToItsPage`·자막 탭의 페이지 단계 테스트(`1/3`, `2/3`, `1/1`, 그대로).
- 1.1 새 테스트 `CaptionConsoleStepNumberTests.cs` 3개(악보 줄 `1-1`…`2-2`와 화면 읽기 "1장 2줄", 장 `1`·`2`·`3`과 글이 번호 옆, 페이지 `2/3`, 셋 다 카드 오른쪽 위 노란 번호 확인)와 값이 바뀐 기존 4개를 고친 뒤, 고치기 전 실행: 7개 실패(이름표가 "1장 1줄"·"1장", 노란 번호 없음).
- 2: `CaptionStep(Slide, Row, Label, Name, Text, Thumbnail)` — `Label`은 번호 글, `Name`은 말로 된 이름(`Spoken`이 씀). 카드: 이름표 줄 삭제, 오른쪽 위 노란 번호(`#FFF6E600`, 검은 굵은 글자, `MinWidth` 18, 모서리는 카드처럼 둥금), 글은 번호 옆에 카드 높이 전부.
- 3.1 Debug 솔루션 빌드·Release `Axicultus` 빌드 오류 0. `Axicultus.Tests` 3758 통과 / 22 건너뜀(새 테스트 3개), `Axicultuss.Tests` 478 / 2.
- 3.2 그림(`DrawTheConsoleForReview`, 밝은·어두운): `4-follow-score-auto-default`·`-dark-default`·`-max`(악보 줄 `2-1` 지금, `2-2` 다음), `3-follow-text-default`·`-dark-default`·`-max`(장 `2` 지금, `3` 다음), `9-follow-song-pages-default`·`-dark-default`(`1/3` 지금, `2/3` 다음), `11-default-look-light`·`-dark`. 번호는 테두리 안 모서리에 붙고 글은 번호 밑으로 들어가지 않는다. 기본 크기에서는 단계 목록이 한 줄 높이라 둘째 줄의 다음 단계가 잘려 보인다(스크롤, 이번 변경 전과 같음).

총괄 검증(2026-11-28):

- 적대적 리뷰(별도 에이전트): 치명·주요 0. `CaptionStep.Label`을 읽는 곳은 카드 틀뿐이고 송출·기록·저장에는 쓰이지 않음을 확인. 사소한 지적 중 주석의 한글 예시만 고쳤다.
- `/code-review`: 결함 0, 사소 3. 주석은 고쳤고, 페이지 단계의 화면 읽기 이름이 위치 글이라는 점은 스펙 문구에 적었다.
- 그대로 둔 것: 번호가 카드 오른쪽 한 칸을 차지해 글이 모든 줄에서 번호 폭만큼 좁아진다(`100/120` 같은 긴 번호에서 글 폭이 꽤 준다). 번호를 글 위에 겹치면 글이 한 줄 덜 보여 지금 방식을 택했다.
- 총괄이 직접 돌린 결과: Debug 솔루션 빌드 오류 0, `Axicultus.Tests` 3758 통과·22 건너뜀·실패 0, `Axicultuss.Tests` 478 통과·2 건너뜀. 하네스 `captiontab`(로그 `a049-verify.log`, 격리 사본) 16 PASS·0 FAIL, 처리되지 않은 예외 0, 실제 데이터 unchanged.
- 그림: 검토용 그림(악보 줄 `2-1`·`2-2`, 장 `1`~`3`, 밝은·어두운)과 실제 앱 그림(`captiontab-following.png`: 페이지 단계 `1/2`·`2/2`)을 총괄이 봤다. 번호는 카드 오른쪽 위에 노란 바탕으로 붙고 지금·다음 단계 테두리와 겹치지 않는다.
