# CodeGraph 영향

기준 브랜치는 `feat/frmmain-modern-ui`의 `3ed6a76`이다. 로컬 `.codegraph/codegraph.db`를 읽기 전용으로 조회했다. 인덱스가 일부 최신 호출과 줄 번호를 포함하지 않아 아래 경로를 기준 소스와 대조했다.

- `SaveRegValueInternal`은 `RegUtil`의 문자열, `int`, `long` 저장 오버로드에서 호출된다. 접근·로그 정책 변경은 이 경계를 통과하는 기존 호출에도 적용된다.
- 시작 시 `gfConfig`에서 호출하는 `LoadSavedData`가 `UpdateV4RegDM`과 가사 모니터 글꼴 크기 조회를 수행한다. 옵션 확인·종료·알림 창 등에서 호출하는 `SaveOptionsData`와 저장 키를 맞춘다.
- `FrmOptions.SaveVariables`는 `OfficeVersion.SetPowerPointDisplayMonitor` 뒤에 EasiSlides 설정을 저장한다. 변경 지점은 설치 조회이며 Office COM과 실제 슬라이드 실행은 그대로 사용한다.
- 출력 모니터의 좌표 계산, `LS_Width`, `selectScreen`, DB 쿼리 및 동기화와의 직접 변경 경로는 없다.
