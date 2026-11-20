## 호출 영향

- `rg` 및 기존 Release 분석 SARIF에서 26개 위치와 호출부를 확인했다. `OfficeVersion`은 PPT 설치 조회 및 송출 모니터 설정에 사용되며 테스트에서는 별도 하이브를 사용한다.
- `FrmMain`의 파일 삭제가 `Gf.RecycleBin` 결과를 사용한다. 두 WinForms 목록의 `List_DrawSubItem`은 사용자 그리기 실패 시 기본 그리기로 복구한다.
- `OutputWindow`는 로드, 옵션 변경, 창 종료에서 전원 관리를 갱신한다. WPF·WinForms가 키보드 훅 코드를 공유한다.

로컬 CodeGraph 심볼 색인에서 RecycleBin과 SetPowerPointDisplayMonitor 정의를 확인했다. 이번 대상의 호출 간선은 조회되지 않아 실제 호출부와 이벤트 연결을 소스에서 대조했다.
