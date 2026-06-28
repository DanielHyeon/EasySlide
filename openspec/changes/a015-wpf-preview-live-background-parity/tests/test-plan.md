# 테스트 계획

## Red

- `UseIndividualFormatting=false`인데 `FormatData`에 `61=` 배경 이미지가 있는 항목을 선택한다.
- 전역 배경 이미지 경로를 별도로 지정한다.
- Preview 샘플 배경 로더가 항목 배경 경로를 요청하면 실패로 본다.
- 기대값: 항목 배경 경로가 아니라 전역/default 경로를 사용한다.

## Green

- 샘플 서식 계산에서 `UseIndividualFormatting`을 반영한 유효 `SongFormatData` helper를 사용한다.

## Verify

- 관련 MainViewModel 테스트 통과
- LiveSessionService/OutputRenderer 기존 배경 우선순위 테스트 통과
- 실제 사도신경 앱 캡처 저장
