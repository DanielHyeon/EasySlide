# app-lifecycle

## ADDED Requirements

### Requirement: 종료할 때 앱이 띄운 PowerPoint를 남기지 않는다

앱이 정상 종료되면, 앱이 실행 중에 띄운 PowerPoint 프로세스는 SHALL 10초 안에 모두 끝난다.
앱은 사용자가 직접 연 프레젠테이션을 SHALL NOT 닫는다.

#### Scenario: PPT 항목을 미리 본 뒤 종료

- **GIVEN** 앱을 시작하기 전에 PowerPoint가 실행 중이 아니다
- **WHEN** 예배 순서의 PPT 항목을 선택해 미리보기에 올리고, 앱을 닫는다
- **THEN** 10초 안에 `POWERPNT.EXE` 프로세스가 하나도 남지 않는다

#### Scenario: PPT 항목을 송출 단까지 보낸 뒤 종료

- **GIVEN** 앱을 시작하기 전에 PowerPoint가 실행 중이 아니다
- **WHEN** PPT 항목을 미리보기에 올리고 송출 단으로 복사한 뒤 앱을 닫는다
- **THEN** 10초 안에 `POWERPNT.EXE` 프로세스가 하나도 남지 않는다

#### Scenario: PPT 항목을 한 번도 열지 않고 종료

- **WHEN** 가사 항목만 선택한 뒤 앱을 닫는다
- **THEN** 종료 과정에서 PowerPoint를 새로 띄우지 않는다
- **AND** 종료 시간이 눈에 띄게 늘어나지 않는다

#### Scenario: 사용자가 PowerPoint로 다른 문서를 열어 둔 경우

- **GIVEN** 사용자가 앱과 별개로 PowerPoint에서 프레젠테이션을 열어 두었다
- **WHEN** 앱에서 PPT 항목을 미리 본 뒤 앱을 닫는다
- **THEN** 사용자가 연 프레젠테이션은 닫히지 않는다
- **AND** 저장하지 않은 내용을 잃지 않는다

### Requirement: 가사 목록의 글꼴 자원을 쌓지 않는다

가사 목록을 다시 만들 때 앱은 슬라이드마다 글꼴 객체를 새로 만들어 쌓아 두면 SHALL NOT 안 된다.
더 쓰지 않는 글꼴은 SHALL 해제한다. 아직 컨트롤이 쓰고 있는 글꼴은 SHALL NOT 해제하지 않는다.

#### Scenario: 항목을 여러 번 바꾼다

- **GIVEN** 글꼴 이름, 굵기, 미리보기 글자 크기가 바뀌지 않았다
- **WHEN** 가사 항목 두 개를 번갈아 50번 선택한다
- **THEN** 가사 칸에 쓸 글꼴 객체를 새로 만들지 않는다

#### Scenario: 보이는 것은 그대로다

- **WHEN** 가사 항목을 미리보기와 송출 단에 올린다
- **THEN** 글꼴 이름, 크기, 굵기가 수정 전과 같다
- **AND** 옵션에서 미리보기 글자 크기를 바꾸면 바로 반영된다

#### Scenario: 해제한 글꼴을 다시 쓰지 않는다

- **WHEN** 항목을 바꾸고, 글자 크기를 바꾸고, 창 크기를 바꾼다
- **THEN** "Parameter is not valid" 같은 그리기 예외가 나지 않는다
