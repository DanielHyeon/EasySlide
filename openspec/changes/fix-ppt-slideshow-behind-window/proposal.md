# Proposal: PPT 슬라이드 쇼가 출력 모니터의 다른 창 뒤에 뜨는 문제 수정

## Why

출력 모니터에 다른 앱의 창이 떠 있으면, PPT 항목을 송출해도 PowerPoint 슬라이드 쇼가 그 창 뒤에 뜬다. 프로젝터에는 다른 앱의 창이 그대로 보이는데 메인 화면은 `Show: ON`이고 썸네일도 정상이라 운영자는 알 수 없다. `fix-show-window-lifecycle`의 검증 중 확인했다 (출력 모니터에 Codex 창, VS Code 창이 있을 때 각각 재현. PowerPoint의 작업 표시줄 단추만 깜빡였다).

가사 항목은 영향이 없다. 송출 창이 항상 위에 뜨는 창이기 때문이다.

원인 (design.md): 앱은 슬라이드 쇼 창을 앞으로 가져오는 일을 하지 않고 PowerPoint가 스스로 앞으로 나오기를 기대한다. PowerPoint는 별도 프로세스라서 Windows가 그 요청을 거절한다. PPT 항목일 때 송출 창은 맨 위에 있지만 투명해서, 그 아래에 있는 창이 그대로 보인다.

이번 작업 이전부터 있던 동작이다. 이 경로의 코드는 이 브랜치에서 바뀌지 않았다.

## What Changes

- 쇼에서 슬라이드 쇼를 실행한 직후, 앱이 그 슬라이드 쇼 창을 "항상 위" 창으로 만든다. 포커스는 옮기지 않는다.
- 송출 창은 지금처럼 그 위에 놓인다 (PPT 항목 위의 알림, 참조 표시가 계속 보여야 한다).
- 쇼가 끝나면 슬라이드 쇼 창이 "항상 위"로 남지 않는다.
- 창을 올리지 못하면 기록하고 알림 칸에 알린다.

## Out of Scope

- 슬라이드 쇼 실행 결과를 확인하지 않아서 이전 항목의 슬라이드가 남는 문제 (별도 과제, PowerPoint 묶음)
- 외부 미디어 플레이어 창의 같은 약점 (`gfPowerPoint.cs`의 `RunProcessOnMonitor`)
- 모니터가 하나인 방식의 쇼

## Impact

- `OfficeLib/PowerPoint.cs`, `Easislides/Global/gfPowerPoint.cs`
- 필요하면 `Easislides/Easislides/FrmLaunchShow.cs` (송출 창을 다시 위로 올리는 순서)
