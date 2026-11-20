## Why

첫 실행과 이전 프로필 복사 후에 화면 크기를 묻는 창이 운영 콘솔을 가린다. 운영자는 앱을 열고 곧바로 예배 순서를 준비할 수 있어야 한다.

## What Changes

WPF는 첫 실행에서 표준 크기를 사용한다. 저장한 크기가 있으면 메인 창 생성 전에 적용한다. 크기는 설정에서 변경한다.

## Capabilities

### New Capabilities

- `wpf-startup-appearance` 운영 콘솔의 시작 화면과 인터페이스 크기 선택을 정의한다.

## Impact

범위는 WPF 운영 콘솔 시작과 설정 읽기다. 송출, 모니터 배치, DB, Office 연동은 범위 밖이다.
