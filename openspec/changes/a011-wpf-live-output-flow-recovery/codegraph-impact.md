# CodeGraph Impact

## Summary

CodeGraph 기준 이번 change의 핵심 영향 심볼은 `OutputWindowHost`이다.

- `OutputWindowHost`: `IOutputWindowService`와 `ILiveSessionService`를 구독해 실제 출력 surface를 만들고 세션을 반영한다.
- `IOutputSurface`: 실제 `OutputWindow`와 테스트 fake surface가 구현하는 출력 창 추상화이다.
- `OutputWindow`: WPF 회중 송출 창이며, WPF `Window.Closed` 이벤트를 통해 외부 닫힘을 알릴 수 있다.
- `OutputWindowService`: 출력 창 상태(`IsOpen`, display, placement)를 소유한다.
- `LiveSessionService`: Live payload와 상태를 알리고 host가 이를 Output surface에 반영한다.

## Root Cause

WPF 출력 창을 사용자가 창 닫기(X/Alt+F4 등)로 직접 닫으면 실제 surface는 사라지지만 `OutputWindowService.Current.IsOpen`은 계속 true로 남을 수 있었다. 이 상태에서 다시 Live를 누르면 `EnsureLiveOutputDisplay()`가 출력 창이 이미 열려 있다고 판단해 `Open()`을 호출하지 않고, host도 새 surface를 만들지 않는다.

그 결과 내부 Live 상태와 상태바는 갱신되어도 실제 회중 출력 창은 다시 열리지 않는 “Live가 안됨” 상태가 발생할 수 있다.

## Impact

- `IOutputSurface`에 `Closed` 이벤트를 추가해 실제 surface 닫힘을 host가 감지한다.
- `OutputWindowHost`는 외부 닫힘을 감지하면 내부 surface/view model 참조를 정리하고 `OutputWindowService.Close()`로 서비스 상태를 닫힘으로 동기화한다.
- 서비스가 닫힘 상태가 되면 다음 Live 시작 시 기존 `EnsureLiveOutputDisplay()`가 다시 `Open(display, windowed:false)`를 호출해 새 출력 창을 만들 수 있다.
- 서비스 주도 `Close()`와 외부 닫힘 이벤트가 재진입하지 않도록 `_closingSurfaceFromService` 가드를 둔다.

## Regression Guards

- `ExternalSurfaceClose_MarksServiceClosedAndAllowsLiveReopen`
  - 외부 surface 닫힘 후 `OutputWindowService.Current.IsOpen == false`가 되는지 확인한다.
  - 같은 display로 다시 output open/live session을 시작하면 새 surface가 만들어지고 Live payload가 반영되는지 확인한다.
- 기존 Preview Live, Output Live, PPT Live 집중 테스트를 함께 실행해 WinForms 흐름의 주요 경로를 유지한다.

## Phase 11 - Text Live Root Cause Follow-up

### WinForms Evidence

WinForms `FrmLaunchShow.Remote_SongChanged`는 `Gf.OutputItem`을 그대로 그리지 않고 `LoadWorshipListItemToLive(...)`를 호출한다. 이 함수는 `Gf.WorshipSongs[Selecteditem, 0/2/4]`의 타입/ID/제목 경로/FormatData를 기준으로 `Gf.LiveItem`을 다시 채운 뒤, 텍스트 계열(`D/B/T/I/W/M`)에서 `Gf.FormatText`, `Gf.FormatDisplayLyrics`, `Gf.DisplaySlidesFormattedLyrics`, `ShowSlide`를 거쳐 `Gf.ShowDBSlide`/`Gf.DrawText`로 송출한다.

실제 운영 `.esw` 데이터도 이 흐름을 요구한다. 예: `C:\EasiSlides\Admin\WorshipLists\1.주일예배.esw`의 사도신경 항목은 `<ItemID>T1</ItemID>`이고 실제 파일 경로는 `<Title1>D:\예배자료\주일예배\★★★사도신경★★★.txt</Title1>`에 있다. 해당 파일은 현재 PC에 존재하며 본문도 정상이다.

### WPF Gap

WPF는 Live 시점에 이미 만들어진 `LiveQueueItem.Kind`와 `Lyrics`를 신뢰했다. 따라서 레거시 Text 항목이 `Kind=Item`, `Lyrics=null`, `Id=esw:T:<path>` 또는 `ContentPath=<path>` 형태로 남으면 `LiveSessionService`가 Text/Notice로 보지 못해 본문 대신 제목/경로 또는 공백에 가까운 payload가 송출될 수 있었다.

### Fix

`MainViewModel.PrepareLiveItemForOutput`를 추가해 모든 주요 Live 경로(`PreviewToLive`, `GoLive`, Output Live, Hidden payload refresh, Output visual refresh, `ResolveLiveProjection`)가 WinForms처럼 레거시 타입/ID/ContentPath/Title 경로를 보수적으로 재해석한다.

- `T/I/W` 또는 `esw:T:`/텍스트 파일 경로는 `Notice`로 정규화하고 `.txt` 본문을 UTF-8/BOM/CP949 폴백으로 다시 읽는다.
- `B`/`bible:` 항목은 본문이 비어 있으면 `Bible.ExpandSelectionBody`로 다시 확장한다.
- PPT/Media/Song 타입 추론은 기존 경로를 깨지 않도록 `Kind=Item`일 때만 보정한다.

### Regression Guards

- `PreviewToLiveCommand_LegacyEswTextItemReloadsFileBodyLikeFrmLaunchShow`
  - 빈 `Lyrics` + `Kind=Item` + `esw:T:<path>` 항목도 Live 시점에 파일 본문을 다시 읽어 첫 문단을 송출하는지 확인한다.
- 집중 테스트: Text/Bible/ExternalText/PPT 인접 Live 테스트 7개 통과.
- 전체 WPF 테스트: 2447개 통과.
- `openspec validate a011-wpf-live-output-flow-recovery --strict` 통과.
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal` 통과.
- WPF Release `C:\EasiSlides\EasislidesNext` 배포 완료.
