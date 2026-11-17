# live-show

## ADDED Requirements

### Requirement: PowerPoint가 응답하지 않아도 쇼를 끌 수 있다

쇼를 끄거나 앱을 닫을 때 PowerPoint가 응답하지 않으면, 앱은 PowerPoint 정리를 정해진 시간 안에 포기하고 나머지 쇼 끄기를 끝내야 한다(SHALL). 메인 화면은 계속 응답해야 한다(SHALL). 정리하지 못한 것이 있음을 알린다.

#### Scenario: PowerPoint가 멈춘 채 쇼를 끈다

- **GIVEN** PPT 항목이 송출 중이고 PowerPoint가 응답하지 않는다
- **WHEN** 쇼 끄기 버튼을 누른다
- **THEN** 몇 초 안에 쇼 버튼이 꺼진 상태가 되고 송출 창이 내려간다
- **AND** 알림 칸에 PowerPoint를 닫지 못했다는 글이 나온다
- **AND** 메인 화면의 다른 버튼이 계속 동작한다

#### Scenario: 그 뒤에 앱을 닫는다

- **WHEN** PowerPoint가 여전히 응답하지 않는 채로 메인 화면을 닫는다
- **THEN** 앱이 닫힌다. PowerPoint는 강제로 끝내지 않는다

#### Scenario: PowerPoint가 다시 응답한다

- **GIVEN** 정리하지 못한 채 쇼가 꺼졌고, 그 뒤 PowerPoint가 다시 응답한다
- **WHEN** 쇼를 다시 시작한다
- **THEN** 쇼가 정상으로 시작되고, 이전에 닫지 못한 쇼 복사본이 정리된다

### Requirement: PowerPoint가 정상일 때는 달라지지 않는다

PowerPoint가 응답하는 동안 쇼 끄기와 앱 닫기의 결과와 걸리는 시간은 이 변경 전과 같아야 한다(SHALL).

#### Scenario: 평소의 쇼 끄기

- **WHEN** PPT 항목 송출 중에 쇼를 끈다
- **THEN** 슬라이드 쇼 창과 쇼 복사본이 닫히고, 알림은 나오지 않는다
