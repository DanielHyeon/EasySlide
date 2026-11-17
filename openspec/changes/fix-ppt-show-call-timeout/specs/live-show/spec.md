# live-show

## ADDED Requirements

### Requirement: 쇼 진행 중에 PowerPoint가 응답하지 않아도 앱은 응답한다

쇼를 시작할 때와 쇼가 진행되는 동안 PowerPoint가 응답하지 않으면, 앱은 PowerPoint 호출을 정해진 시간 안에 포기하고 그 뒤로는 기다리지 않고 건너뛰어야 한다(SHALL). 메인 화면과 송출 창은 계속 응답해야 하고(SHALL), PowerPoint를 쓰지 않는 항목은 평소처럼 송출되어야 한다(SHALL).

#### Scenario: 송출 중인 PPT 항목에서 슬라이드를 넘긴다

- **GIVEN** PPT 항목이 송출 중이고 PowerPoint가 응답하지 않는다
- **WHEN** 다음 슬라이드로 넘긴다
- **THEN** 몇 초 안에 앱이 다시 응답하고, 알림 칸에 PowerPoint가 응답하지 않는다는 글이 나온다
- **AND** 송출 화면은 바뀌지 않는다
- **AND** 그 뒤의 넘김은 기다리지 않고 바로 돌아온다

#### Scenario: 가사 항목으로 넘어간다

- **WHEN** PowerPoint가 응답하지 않는 채로 가사 항목으로 이동한다
- **THEN** 가사가 평소처럼 송출되고, 멈춘 슬라이드는 보이지 않는다

#### Scenario: PPT 항목으로 넘어간다

- **WHEN** PowerPoint가 응답하지 않는 채로 PPT 항목으로 이동한다
- **THEN** 기본 배경이 송출되고 (시작하지 못한 PPT 항목의 규칙), 알림이 나오고, 앱이 응답한다

#### Scenario: 쇼를 다시 켠다

- **GIVEN** PowerPoint가 응답하지 않는 채로 쇼를 껐다
- **WHEN** 쇼를 다시 켠다
- **THEN** 쇼가 시작되고 앱이 응답한다. PPT 항목은 위의 규칙을 따른다

### Requirement: PowerPoint가 다시 응답하면 PPT 항목을 다시 쓸 수 있다

PowerPoint가 다시 응답하면 앱을 다시 시작하지 않고도 PPT 항목이 송출되어야 한다(SHALL).

#### Scenario: 다시 응답한 뒤 PPT 항목을 불러온다

- **GIVEN** 응답 없음 상태였고, PowerPoint가 다시 응답한다
- **WHEN** PPT 항목을 다시 불러온다
- **THEN** 슬라이드가 송출되고 슬라이드 넘김이 동작한다

### Requirement: PowerPoint가 정상일 때는 달라지지 않는다

PowerPoint가 응답하는 동안 쇼 시작, 항목 이동, 슬라이드 넘김의 결과와 걸리는 시간은 이 변경 전과 같아야 한다(SHALL). 정상이지만 느린 호출(PowerPoint를 새로 띄우기, 큰 파일 열기)을 끊지 않아야 한다(SHALL).

#### Scenario: PowerPoint가 떠 있지 않은 상태에서 첫 쇼 시작

- **WHEN** PPT 항목이 있는 목록으로 쇼를 시작한다
- **THEN** PowerPoint가 뜨고 파일이 열리며, 알림 없이 쇼가 시작된다
