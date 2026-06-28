## MODIFIED Requirements

### Requirement: 설정 패널 컨트롤은 텍스트를 잘라내지 않아야 한다

WPF 설정 패널은 텍스트 박스, 체크박스, 콤보박스의 표시 텍스트를 하단에서 잘라내지 않을 만큼의 높이와 세로 패딩을 제공해야 하며, 각 설정 컨트롤은 표시 텍스트를 자르지 않아야 한다(SHALL).

#### Scenario: Image 경로와 Transition 콤보박스 텍스트가 잘리지 않는다

- **GIVEN** 운영자가 설정 패널에서 선택 항목의 Image 또는 Transition 영역을 본다
- **WHEN** Image 경로 텍스트 박스와 Transition 콤보박스가 렌더링된다
- **THEN** 각 컨트롤은 하단 획이 있는 문자도 잘리지 않도록 설정 전용 높이와 패딩을 사용한다
