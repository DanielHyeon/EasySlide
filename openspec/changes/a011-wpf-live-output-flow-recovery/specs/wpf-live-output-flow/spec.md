## ADDED Requirements

### Requirement: Preview Live shall start actual output
WPF MainWindow SHALL, Preview 항목이 선택된 상태에서 Live 버튼을 누르면 출력 창을 자동으로 준비하고 선택 항목을 실제 Output surface에 송출해야 한다.

#### Scenario: Text preview item starts fullscreen output
- **WHEN** 운영자가 텍스트/찬양 Preview 항목을 선택하고 Live 버튼을 클릭한다
- **THEN** WPF는 확인 메시지 없이 출력 창을 열고 선택 항목을 전체 화면 Output surface에 표시해야 한다

#### Scenario: Live state is not sufficient without output content
- **WHEN** Live 명령이 성공했다고 표시된다
- **THEN** 출력 모델의 현재 항목과 표시 payload도 같은 항목으로 갱신되어야 한다

### Requirement: Output Live shall follow FrmMain show start behavior
WPF MainWindow SHALL, Output 영역의 Live 시작 버튼을 누르면 WinForms `cbGoLive`/`Start_Presentation` 흐름처럼 현재 Output 항목 또는 WorshipList 첫 항목을 실제 송출해야 한다.

#### Scenario: Output live starts first worship item when output is empty
- **WHEN** Output 항목이 비어 있고 WorshipList에 항목이 있는 상태에서 운영자가 Output Live를 시작한다
- **THEN** WPF는 첫 WorshipList 항목을 Output으로 준비하고 전체 화면 송출을 시작해야 한다

### Requirement: PowerPoint Live shall start slideshow on selected display
WPF MainWindow SHALL, PPT 항목 Live 시작 시 PowerPoint SlideShow를 선택된 출력 모니터에서 시작해야 한다.

#### Scenario: PowerPoint preview item starts slideshow
- **WHEN** 운영자가 PPT Preview 항목을 선택하고 Live 버튼을 클릭한다
- **THEN** WPF는 선택된 출력 모니터 장치명을 사용해 PowerPoint SlideShow 시작을 요청해야 한다

### Requirement: Live flow shall preserve FrmMain immediate operation
WPF MainWindow Live 정상 경로 SHALL, WinForms `FrmMain`처럼 별도 확인 대화상자 없이 즉시 실행되어야 하며, Live 상태는 상단 빨간 상태 라인이 아니라 조작 아이콘/하단 상태바 맥락에서 표시되어야 한다.

#### Scenario: Live starts without modal prompt
- **WHEN** 운영자가 정상 Preview/Output 항목에 대해 Live를 시작한다
- **THEN** WPF는 메시지 박스나 확인 질문 없이 즉시 송출을 시작해야 한다
