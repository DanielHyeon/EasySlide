# Proposal: 쇼 진행 중에 PowerPoint가 멈춰도 앱이 멈추지 않게 한다

## Why

`fix-ppt-show-end-timeout`으로 쇼 끄기와 앱 닫기는 PowerPoint가 응답하지 않아도 끝난다. 그러나 쇼를 시작할 때와 쇼가 진행되는 동안의 PowerPoint 호출에는 여전히 시간 제한이 없다. 모두 메인 화면 스레드에서 부른다.

- 쇼 시작의 PPT 미리 열기 (`PreLoadPowerpointFiles`: PowerPoint에 붙기, 파일 열기)
- PPT 항목 불러오기 (송출 창과 메인 화면이 각각 슬라이드 쇼를 실행하거나 따라간다)
- 슬라이드 넘김, 슬라이드 쇼 상태 다시 묻기
- 메인 화면의 PPT 항목 썸네일과 절 정보 읽기

PowerPoint가 응답하지 않으면 이 중 어느 것에서든 앱이 그대로 멈춘다. 예배 중이면 가사 항목으로 넘어갈 수도 없다. 쇼를 껐다가 다시 켜는 경우도 같다 (앞 변경의 Known Limitation).

## What Changes

- 쇼 시작부터 쇼 끝까지 메인 화면 스레드에서 PowerPoint로 가는 호출 묶음을 모두 하나의 관문으로 보낸다. 관문은 앞 변경의 응답 확인과 시간 제한을 쓴다.
- PowerPoint가 응답하지 않는다고 판단되면 "응답 없음" 상태가 된다. 이 상태에서는 PowerPoint 호출을 기다리지 않고 바로 건너뛴다. 가사, 성경, 이미지 항목은 평소처럼 송출된다.
- 응답 없음 상태에서 PPT 항목을 불러오면 "시작하지 못한 PPT 항목"의 기존 규칙대로 기본 배경을 송출하고 알린다. 이미 송출 중인 PPT 항목에서 슬라이드를 넘기려 하면 화면은 그대로 두고 알린다.
- PowerPoint가 다시 응답하면 상태가 풀리고, PPT 항목을 다시 불러오면 정상으로 송출된다.

## Out of Scope

- 응답하지 않는 PowerPoint를 강제로 끝내는 것.
- 멈춘 PowerPoint의 슬라이드 쇼 창을 치우는 것 (다른 프로세스의 창이다). 송출 창이 불투명해지면 가려진다.
- PowerPoint 호출을 다른 스레드로 옮기는 것.

## Impact

- `OfficeLib/PowerPoint.cs`, `OfficeLib/PowerPointCallLimit.cs`, `Easislides/Global/gfPowerPoint.cs`, `gfFileHelpers.cs`, `Easislides/Easislides/FrmLaunchShow.cs`, `FrmMain.cs`, `FrmMain.Logic.cs`
- 사용자 영향: PowerPoint가 정상일 때는 달라지는 것이 없다. PowerPoint가 응답하지 않을 때 앱이 멈추지 않고, PPT 항목만 송출되지 않으며 알림이 나온다.
