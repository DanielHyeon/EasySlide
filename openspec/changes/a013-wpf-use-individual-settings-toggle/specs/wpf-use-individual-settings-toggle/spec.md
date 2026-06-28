## ADDED Requirements

### Requirement: WPF Use Individual Settings 텍스트 항목 서식 활성화

WPF 미리보기 서식 패널은 화면에 보이는 `Use Individual Settings` 체크박스로 선택 항목의 개별 서식 사용 여부를 바꿀 수 있어야 한다(SHALL).

WPF 미리보기 서식 패널은 본문이 비어 있지 않은 `Notice`/텍스트 파일 항목에 대해 개별 서식 컨트롤을 활성화해야 한다(SHALL).

#### Scenario: 선택 항목 개별 서식 끄기

GIVEN `UseIndividualFormatting`이 켜진 WPF 큐 항목이 선택되어 있다
WHEN 운영자가 `Use Individual Settings`를 클릭한다
THEN 선택 항목의 `UseIndividualFormatting`은 꺼져야 한다
AND 큐 안의 항목도 동일하게 꺼진 설정으로 교체되어야 한다
AND 화면의 체크박스 상태도 꺼진 상태를 반영해야 한다.

#### Scenario: 선택 항목 개별 서식 다시 켜기

GIVEN `UseIndividualFormatting`이 꺼진 WPF 큐 항목이 선택되어 있다
WHEN 운영자가 `Use Individual Settings`를 클릭한다
THEN 선택 항목의 `UseIndividualFormatting`은 켜져야 한다
AND 화면의 체크박스 상태도 켜진 상태를 반영해야 한다.

#### Scenario: 공지 텍스트 항목 개별 설정 편집

GIVEN 선택된 WPF 큐 항목의 종류가 `Notice`이다
AND 해당 항목의 `Lyrics`가 비어 있지 않다
WHEN 미리보기 서식 패널이 개별 서식 컨트롤 상태를 평가한다
THEN 해당 항목은 서식 가능 항목으로 처리되어야 한다
AND 개별 글꼴, 색, 배경, 강조 명령은 실행 가능해야 한다.

#### Scenario: 비텍스트 시각 항목은 계속 서식 불가

GIVEN 선택된 WPF 큐 항목의 종류가 `PowerPoint`이다
WHEN 미리보기 서식 패널이 개별 서식 컨트롤 상태를 평가한다
THEN 해당 항목은 서식 가능 항목으로 처리되지 않아야 한다
AND 개별 텍스트 서식 명령은 계속 비활성화되어야 한다.
