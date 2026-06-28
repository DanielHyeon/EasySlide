## ADDED Requirements

### Requirement: 설정 정렬 컨트롤은 균등하게 배치되어야 한다

메인 설정/서식 영역의 좌·중·우 정렬 컨트롤은 같은 기준선과 같은 폭 배치를 사용해야 한다. 구현은 이 요구를 SHALL 만족해야 한다.

#### Scenario: 전역 가사 정렬 버튼 표시

- GIVEN 사용자가 메인 화면의 설정/서식 영역을 본다
- WHEN 전역 가사 정렬 `왼쪽`, `가운데`, `오른쪽` 버튼이 표시된다
- THEN 세 버튼은 같은 행에서 균등한 폭으로 배치되어야 한다
- AND 한쪽으로 치우쳐 보이지 않아야 한다

#### Scenario: 항목별 정렬 버튼 표시

- GIVEN 사용자가 선택 항목의 `Align / Size` 영역을 본다
- WHEN `Left`, `Center`, `Right` 버튼이 표시된다
- THEN 세 버튼은 같은 행에서 균등한 폭으로 배치되어야 한다
- AND `Default` 버튼은 별도 동작으로 유지되어야 한다

### Requirement: 설정 입력 TextBox는 글자를 클리핑하지 않아야 한다

설정/서식 영역의 직접 입력 `TextBox`는 한글, 영문 하강 글리프, 숫자 입력에서 글자 하단을 자르지 않아야 한다. 구현은 이 요구를 SHALL 만족해야 한다.

#### Scenario: 숫자 입력 TextBox 표시

- GIVEN 사용자가 설정/서식 영역의 숫자 입력 TextBox를 본다
- WHEN 값에 `gypq`, 한글 받침, 숫자가 표시된다
- THEN 글자 하단은 보이는 영역 안에 완전히 표시되어야 한다

#### Scenario: 공통 EsTextBox 표시

- GIVEN 앱의 공통 `EsTextBox` 스타일을 사용하는 입력 상자가 표시된다
- WHEN 사용자가 값을 입력하거나 포커스를 이동한다
- THEN 텍스트 콘텐츠 호스트는 하단 클리핑을 만들지 않는 배치를 사용해야 한다
