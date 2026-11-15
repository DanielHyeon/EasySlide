# ppt-items

## ADDED Requirements

### Requirement: 쇼에서 PowerPoint 발표자 도구가 뜨지 않는다

앱이 PowerPoint 슬라이드 쇼를 시작할 때 발표자 도구 창은 뜨지 않아야 한다(SHALL NOT). PowerPoint의 "발표자 도구 사용" 설정이 켜져 있어도 마찬가지다.

#### Scenario: 모니터가 둘이고 발표자 도구 설정이 켜져 있다

- **GIVEN** 모니터가 둘이고 PowerPoint의 발표자 도구 설정이 켜져 있다
- **WHEN** PPT 항목이 있는 쇼를 시작한다
- **THEN** 출력 모니터에 슬라이드 쇼 창만 뜨고, 발표자 도구 창은 어느 모니터에도 뜨지 않는다

#### Scenario: 쇼 도중 다른 PPT 항목으로 넘어간다

- **WHEN** 쇼 진행 중에 다른 PPT 항목을 송출한다
- **THEN** 발표자 도구 창이 뜨지 않는다

#### Scenario: 속성을 지원하지 않는 PowerPoint

- **WHEN** 발표자 도구 속성이 없는 PowerPoint에서 쇼를 시작한다
- **THEN** 오류 없이 슬라이드 쇼가 시작된다
