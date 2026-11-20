## Why

WinForms에서 저장한 예배 자료와 운영 동작을 WPF에서 이어 쓸 때 발견된 여덟 가지 차이를 해소한다.

## What Changes

찬양집 식별자와 문서 본문, 진행 메모, 상세 곡 검색, 성경 검색 범위, 폴더 정렬 및 전역 키 처리를 보완한다.

## Capabilities

### New Capabilities

- `operator-workflow-parity` — 기존 예배 자료와 운영 명령의 호환성.

## Impact

WPF 메인 창, 라이브러리와 설정, 회귀 테스트가 대상이다. WinForms 데이터베이스 스키마와 운영 레지스트리는 변경하지 않는다.
