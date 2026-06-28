# 설계: 유효 서식 기준 단일화

## 핵심 결정

미리보기 샘플도 실제 Live 송출과 같은 기준으로 항목별 서식을 판단한다.

- `UseIndividualFormatting=true`: `FormatData`를 유효 서식으로 사용한다.
- `UseIndividualFormatting=false`: `FormatData`는 보존하되 렌더 입력으로 사용하지 않는다.
- `FormatData`가 null/공백이면 기존처럼 전역/default 설정을 사용한다.

## 변경 지점

- `MainViewModel`의 미리보기/출력 샘플 서식 계산 함수에서 `SongFormatData.Parse(item?.FormatData)`를 직접 호출하는 경로를 유효 서식 helper로 모은다.
- Live 송출 경로 `ResolveLiveProjection`의 existing rule과 동일하게 미리보기 샘플도 `UseIndividualFormatting`을 반영한다.

## 테스트 전략

- `UseIndividualFormatting=false`인 항목의 미리보기 배경이 항목별 `61=` 경로를 사용하지 않는 실패 테스트를 먼저 추가한다.
- 기존 `ToggleOutputLiveCommand_AfterCopyPreviewToOutput_PublishesSelectedPageAndFormatting` 테스트로 `UseIndividualFormatting=true` 송출 회귀를 유지한다.
- 필요 시 `PreviewToLiveCommand` 직접 경로 테스트를 추가해 사도신경 항목 배경이 Live 세션에 실리는지 검증한다.

## 위험과 완화

- 위험: 미리보기 샘플의 글자색·폰트·정렬도 같이 바뀌어 기존 UI와 다르게 보일 수 있다.
- 완화: 이것은 Live 송출과 맞추기 위한 의도된 변경이며, `Use Individual Settings`가 꺼진 상태에서만 적용된다.
- 위험: 항목별 서식 편집 중 체크박스가 꺼진 상태에서 입력값이 보이지 않아 혼란이 있을 수 있다.
- 완화: 입력값 표시 속성은 보존하고, 실제 샘플 렌더만 유효 서식 기준을 따른다.

## 제외 영향

- DB 스키마 변경 없음.
- COM/Office Interop 영향 없음.
- 송출 모니터 좌표(`LS_Width`, `LS_Height`, `selectScreen`) 변경 없음.
