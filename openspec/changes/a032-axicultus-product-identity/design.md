## 구현
WinForms는 Axicultuss, WPF는 Axicultus를 실행 파일명, CLR 어셈블리명, Windows 제품명, 앱 표시 이름으로 사용한다. 테스트 어셈블리는 각각 Axicultuss.Tests와 Axicultus.Tests다. 소스 네임스페이스와 공통 라이브러리 Axicultus.Core는 기존 참조를 유지한다. 저장 경로와 파일 식별자는 표시 이름과 독립적으로 관리한다.

## 이전 설치에서의 동작

- WinForms는 이전 HKCU 프로필을 읽어 새 프로필에 누락된 값만 복사한다. 완료 표시가 있으면 재실행하거나 새 설정을 초기화해도 이전 값을 덮어쓰지 않는다.
- 작업 폴더, 찬양 DB, 오프라인 도움말은 새 이름을 우선하고 이전 이름으로 설치된 리소스도 인식한다. 기존 예배 자료의 배경 이미지가 이전 기본 경로에서 사라진 경우 현재 작업 폴더에서 같은 상대 경로를 찾는다.
- 찬양 교환 XML과 PPT 이미지 캐시는 두 제품 식별자로 읽으며 새 파일은 Axicultus 식별자로 기록한다.

- WinForms는 HKCU의 Software\Axicultuss에 저장한다. 이전 Axicultus 설정을 먼저 가져오고, 이전 세대에서 초기화했거나 이전을 마친 경우를 제외하고 EasiSlides 설정으로 빈 값을 채운다.
- WPF는 %APPDATA%\Axicultus에 저장한다. AxicultusNext, EasiSlidesNext 순으로 기존 프로필을 읽어 상대 경로별 누락 파일을 복사한다. 모든 파일을 복사한 뒤 완료를 기록하고 기존 데이터 파일의 경로를 보존한다.
- 복사한 WPF 설정으로 첫 시작 시 WinForms의 송출 모니터·예배 순서 등 현재 선택을 기준값으로 저장한다. 다음 시작부터 WinForms에서 달라진 선택만 동기화해 기존 WPF 선택도 보존한다.
