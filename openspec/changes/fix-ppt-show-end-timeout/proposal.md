# Proposal: PowerPoint가 멈춰 있어도 쇼를 끌 수 있게 한다

## Why

쇼를 끌 때 앱은 PowerPoint에 여러 요청을 보낸다 (프레젠테이션 목록 읽기, 쇼 복사본 닫기, 필요하면 PowerPoint 종료). 이 요청들은 메인 화면의 스레드에서 보내고, 답을 기다리는 시간에 제한이 없다. PowerPoint가 답하지 않으면(숨은 대화 상자, 추가 기능, 전환 도중의 멈춤 등) 앱도 그대로 멈춘다. 예배 중에 생기면 앱을 강제로 끝내는 수밖에 없다.

2026-11-17의 조사에서 확인했다. PowerPoint 프로세스를 일부러 멈춰 놓고 쇼를 끄면, 쇼 끄기 호출이 26초 뒤 시간 초과로 실패하고 그 뒤로 앱이 응답하지 않는다. (같은 날 검증 중에 앱이 닫히지 않은 일이 한 번 있었지만, 그것이 이 때문이었다는 증거는 없다. 36번의 쇼 끄기에서 재현되지 않았다.)

앱을 닫을 때의 PowerPoint 정리도 같은 호출을 쓴다.

## What Changes

- 쇼를 끝낼 때와 앱을 닫을 때, PowerPoint에 요청을 보내기 전에 PowerPoint가 응답하는지 먼저 확인한다. 응답이 없으면 정리를 건너뛰고 알림 칸에 알린다.
- 응답을 확인한 뒤의 요청에도 시간 제한을 둔다. 넘으면 그 정리를 포기하고 알린다.
- 건너뛰거나 포기한 정리는 다음 쇼를 시작할 때와 앱을 닫을 때 다시 시도한다.
- 송출 창을 숨기고 메인 화면의 쇼 상태를 "꺼짐"으로 바꾸는 일이 PowerPoint 정리에 막히지 않게 한다.

## Out of Scope

- 쇼 진행 중의 슬라이드 이동, 항목 불러오기에서 PowerPoint가 멈추는 경우 (같은 종류의 약점이다. 호출하는 곳의 목록만 남긴다).
- 응답하지 않는 PowerPoint를 강제로 끝내는 것. 그 PowerPoint에 사용자의 문서가 열려 있을 수 있다 (`fix-ppt-small-items` S6의 결정을 유지한다).

## Impact

- `OfficeLib/PowerPoint.cs`, `Easislides/Global/gfPowerPoint.cs`, `Easislides/Easislides/FrmLaunchShow.cs`, `FrmMain.cs`
- 사용자 영향: PowerPoint가 정상일 때는 달라지는 것이 없다. PowerPoint가 멈춰 있을 때 쇼 끄기와 앱 닫기가 몇 초 안에 끝나고, 닫지 못한 것이 있다는 알림이 나온다.
