전제: a046이 끝나 있다(자막 코드가 `Axicultus.Captions`에 있다). 같은 파일을 건드리므로 a046 묶음 B와 동시에 하지 않는다.

## Phase 0. 기준과 영향

- [ ] 0.1 브랜치(a046 뒤의 HEAD에서) `feat/a048-fix-caption-settings-and-large-deck`. Debug 빌드, `Axicultus.Tests` 전체와 자막 테스트 결과 기록
- [ ] 0.2 design "현재 구조"를 지금 코드와 대조(줄 번호, 부르는 곳, 잡는 예외)

## Phase 1. 실패하는 테스트

- [ ] 1.1 가사 인식: 명령줄 상한을 넘는 수의 그림, 둘째 묶음만 실패, 첫 묶음만 실패, 모든 묶음 실패, 한국어 OCR 없음(종료 코드 3), 취소
- [ ] 1.2 `caption-style.json`: `"Color": null`, 색 형식이 틀림, 숫자 값이 범위 밖, 파일이 JSON이 아님 → 세션이 기본 모양으로 만들어짐
- [ ] 1.3 `obs.json`: `Secret` 없음, `Secret: null`, `Scenes`에 null 원소 → 시작이 예외 없이 끝나고 "저장된 비밀번호 없음"
- [ ] 1.4 복구: 코드(`Notations`)와 카포(`Capo`)가 있는 곡을 복구 파일에 담았다 되돌리면 두 값이 그대로다. 그 값이 없는 옛 파일도 읽힌다
- DoD: 새 테스트가 고치기 전 코드에서 실패하는 것을 확인하고 그 출력을 기록한다.

## Phase 2. 고치기

- [ ] 2.1 `LyricsReader`: 묶음 나누기, 묶음별 시간 제한, 부분 결과(design 1, 2)
- [ ] 2.2 자막 탭의 안내와 문구(design 3)
- [ ] 2.3 `CaptionStyle.Parse`와 세션 생성(design 4)
- [ ] 2.4 `ObsOperator`(design 5)
- [ ] 2.5 복구의 코드와 카포(design 7)
- Constraints: 식별자·주석 영어, 문구는 resx(한국어·영어). 다른 동작을 바꾸지 않는다.

## Phase 3. 회귀와 빌드

- [ ] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [ ] 3.2 하네스 `pptcaption`(가사 인식 포함)이 고치기 전과 같은 결과

## Phase 4. 리뷰와 기록

- [ ] 4.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 `/code-review`
- [ ] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

(구현 뒤 기록)
