# Proposal: 남은 자원 정리 문제 수정

## Why

`modernize-main-form-ui` 작업 1을 검증하면서 화면 표현과 무관한 기존 문제 두 가지를 확인했다.

1. **앱을 닫아도 숨은 PowerPoint가 남는다.** PPT 항목을 한 번이라도 미리보기에 올린 뒤 앱을 닫으면 창이 없는 `POWERPNT.EXE`가 남는다. 작업 1 검증에서 세 번 실행해 세 번 모두 재현됐다. 2.4.3에서 고쳤다고 기록된 문제가 다시 나타난 것이다.
2. **가사 목록을 만들 때마다 글꼴 객체가 새로 만들어지고 해제되지 않는다.** `FrmMain.FormatLyricsContainers`가 슬라이드 수만큼 `Font`를 만들어 각 `RichTextBox`에 넣고, 이전 글꼴은 그대로 버린다. 예배 한 번에 항목을 수십 번 바꾸므로 그때마다 글꼴 객체가 쌓였다가 가비지 수집 때에야 회수된다.

## What Changes

- 앱이 종료될 때, 앱이 띄운 PowerPoint가 모두 끝나게 한다.
- 가사 목록의 글꼴을 슬라이드마다 만들지 않고 함께 쓰며, 더 쓰지 않는 글꼴은 해제한다.

## Out of Scope

- PPT 미리보기의 속도 개선(비동기화, 지연 로딩)
- `OfficeLib`의 구조 변경. 종료 경로에 필요한 최소한만 고친다.
- 화면 모양. 이 변경으로 보이는 것이 달라지면 안 된다.

## Impact

- 관련 코드 (구현 전에 다시 확인할 것):
  - 종료 경로: `FrmMain.Events.cs`의 `Form1_FormClosing` → `Gf.ClearUpPowerpointWindows`, `gfFileHelpers.ReleasePowerpointObjectsForCleanup`, `OfficeLib/PowerPoint.cs`의 `QuitPowerPointApp`
  - PowerPoint 래퍼 인스턴스: `Gf.LivePP`, `Gf.PreviewPPT`, `Gf.OutputPPT`, `Gf.ExternalPPT` (`gfConstants.cs`), `FrmMain.MainPPT` (`FrmMain.Fields.cs`)
  - 글꼴: `FrmMain.cs`의 `FormatLyricsContainers`, `ApplyLyricsRichTextBoxFont` 주변 (`new Font(` 사용처)
