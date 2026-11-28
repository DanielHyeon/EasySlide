전제: a046 묶음 B가 끝나 있다(자막 탭 화면이 `Axicultus.Captions`에 있다).

## Phase 0. 기준과 영향

- [ ] 0.1 브랜치 `feat/a049-caption-step-number-badge`(a046 뒤의 HEAD에서). Debug 빌드, 자막 테스트 결과 기록
- [ ] 0.2 design "현재 구조"를 지금 코드와 대조, 단계 이름표를 보는 테스트 목록

## Phase 1. 실패하는 테스트

- [ ] 1.1 번호 글: 악보 줄 `1-1`·`1-2`·`2-1`, 줄 없는 장 `3`, 페이지 단계 `2/3`. 화면 읽기 이름은 말로 된 이름 그대로

## Phase 2. 고치기

- [ ] 2.1 `CaptionStep`의 번호 글과 말로 된 이름, 단계 만들기
- [ ] 2.2 단계 카드 틀: 이름표 줄 삭제, 오른쪽 위 번호(design 2, 3)
- Constraints: 식별자·주석 영어. 단계 목록의 동작과 카드 크기는 그대로.

## Phase 3. 회귀와 그림

- [ ] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [ ] 3.2 검토용 그림: 밝은·어두운 테마, 악보 줄·글·페이지 단계, 지금 단계와 다음 단계 테두리

## Phase 4. 리뷰와 기록

- [ ] 4.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 `/code-review`
- [ ] 4.3 총괄 완료 검증(그림 확인)과 "검증 결과" 기록
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

(구현 뒤 기록)
