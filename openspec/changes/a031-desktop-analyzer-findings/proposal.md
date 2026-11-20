## Why

두 데스크톱 앱의 Release 분석에서 171개 소스 위치가 남았고, 운영 경로 네 곳과 조건부 경로 두 곳에서 오류를 재현했다.

## What Changes

- 탭 선택·복원, 성경·폴더 이름, PPT 파일 연결, SQLite 스키마 동작을 수정한다.
- 분석 결과에 따라 문자열 비교와 내부 자료형, 인스턴스 의존성 및 네이티브 호출 경계를 명확히 한다.

## Capabilities

### New Capabilities
- `desktop-analyzer-findings`: 탭, 이름 변경, Unicode 파일 연결과 스키마 식별자의 동작을 보장한다.

## Impact

WinForms 옵션·목록과 WPF 내부 렌더링·조회 경로, 공유 HookManager, 회귀 테스트에 적용한다.
