## Why

정적 분석에서 확인한 반환값 미확인 26곳에서 Windows 호출 실패와 구형 데이터 기본값을 구분해야 한다.

## What Changes

- Office 환경 조회, 휴지통, 송출 전원 관리와 입력 결과 처리를 보완한다.
- 목록 그리기 실패 시 Windows 기본 그리기를 사용하고, 손상된 숫자 필드의 기존 기본값을 보존한다.

## Capabilities

### New Capabilities
- `desktop-call-results` — 두 앱의 네이티브 호출 및 파싱 결과 처리.

## Impact

WinForms 목록 그리기, 파일·레지스트리 조회와 WPF 송출 창 및 단축키 처리에 적용한다.
