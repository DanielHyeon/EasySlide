## ADDED Requirements

### Requirement: 데스크톱 제품 식별
WinForms는 Axicultuss, WPF는 Axicultus를 앱 표시 이름과 어셈블리 이름으로 사용해야 한다(SHALL).

#### Scenario: 두 앱 배포
- **WHEN** Release 솔루션을 빌드한다.
- **THEN** WinForms는 Axicultuss.exe와 Axicultuss.dll, WPF는 Axicultus.exe와 Axicultus.dll을 생성하고 창과 제품 메타데이터에 해당 이름을 표시한다.
- **AND** WPF의 `--legacy-ui` 옵션은 같은 배포 폴더의 Axicultuss.exe를 실행한다.

#### Scenario: Windows에서 앱 식별
- **WHEN** 사용자가 실행 파일, 작업 표시줄 또는 앱 아이콘을 표시하는 창을 본다.
- **THEN** WinForms와 WPF 모두 같은 Axicultus 아이콘을 표시하고 Windows 배율에 맞는 크기를 제공한다.

### Requirement: 기존 사용자 자료 호환
앱은 이름 변경 이전의 설정 및 예배 파일을 다시 사용할 수 있어야 한다(SHALL).

#### Scenario: 이전 버전에서 업그레이드
- **WHEN** 이전 제품명으로 만든 DB와 예배 자료가 존재한다.
- **THEN** 기존 경로와 파일 형식을 읽을 수 있다.

#### Scenario: 기존 설정을 새 프로필로 복사
- **WHEN** 기존 WinForms 레지스트리 또는 WPF 프로필이 있는 PC에서 새 버전을 시작한다.
- **THEN** WinForms는 이전 WinForms 레지스트리에서, WPF는 이전 WPF 프로필 폴더에서 각 앱의 새 프로필에 없는 항목을 복사하고 원본, 값 형식, 하위 경로 및 기존 대상 값을 보존한다.
- **AND** 완료 후 재실행해도 사용자가 수정하거나 삭제한 설정이 복구되지 않는다.

#### Scenario: 서로 다른 설정으로 두 앱 사용

- **WHEN** 운영자가 WinForms와 WPF에서 서로 다른 송출 모니터와 예배 목록을 선택한 뒤 두 앱을 다시 시작한다.
- **THEN** WinForms는 `%APPDATA%\Axicultuss\settings.json`에서, WPF는 `%APPDATA%\Axicultus\settings.json`에서 각자의 선택을 복원한다.
- **AND** 처음 실행하는 WPF는 기존 WPF 프로필이 있으면 이전하고, 없으면 WPF 기본값을 사용한다.
- **AND** WinForms 초기화는 빈 JSON 프로필을 저장하고 재실행 시 이전 레지스트리 값을 복원하지 않는다.
- **AND** 두 앱은 선택한 송출 위치를 슬라이드 쇼 창에 적용하며 Office 레지스트리에 기록하지 않는다.
