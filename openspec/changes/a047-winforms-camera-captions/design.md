# Design — a047 Axicultuss 카메라 자막 탭

## 1. 현재 구조 (영향 근거)

CodeGraph CLI는 이 세션에서 쓸 수 없었다. 아래는 2026-11-28 읽기 전용 조사의 결과다. 구현자는 Phase 0에서 줄 번호를 다시 확인한다.

**회중 화면에 나가는 것**
- 회중 화면은 `Gf.LiveItem`(쇼 창 `FrmLaunchShow`의 항목)으로 그린다. 본창 송출 칸은 `Gf.OutputItem`이다. 둘 다 `SongSettings`(`Type`, `ItemID`, `Title`, `Path`, `CurSlide`, `TotalSlides`, `Slide[,]`, `LyricsAndNotationsList`).
- 듀얼 모니터: 본창이 `OutputItem`을 바꾸고 `RemoteControlLiveShow(...)`로 쇼 창에 알린다. 단일 모니터: 쇼가 모달이고 쇼 창이 `LiveItem`을 스스로 움직인다. 자동 넘김도 쇼 창이 움직인다. 그래서 쇼가 도는 동안은 `LiveItem`을 봐야 한다.
- 종류는 `Type` 한 글자: D 곡, P PowerPoint, B 성경, T 글 파일, I 정보 화면, W Word, M 미디어, G 간격 항목.
- 페이지 글: `Gf.GetSlideContents(item, slide, region, font, notations)`(`gfLyrics.cs`)가 한 페이지 한 영역의 글을 준다. `Gf.BuildLyricsSlideTexts`는 `Slide[i,5..7]`을 덮어쓰므로 자막에서 쓰지 않는다.
- 상태: `Gf.ShowRunning`, `Gf.ShowLiveBlack`, `Gf.ShowLiveClear`, `Gf.LivePowerPointRunState`, `Gf.LiveLoadSerial`. "숨김" 상태는 따로 없다(글 숨김 = clear).
- PPT: 회중 화면은 PowerPoint의 쇼 창이다. 지금 장은 `LiveItem.CurSlide`·`OutputItem.CurSlide`에 있고, 듀얼 모니터에서는 `TimerFlasher`(600ms)의 `SyncNativePowerPointSlide`가 PowerPoint 안에서 넘긴 것을 반영한다. 파일 경로는 `Path`. 송출 칸의 장 그림은 `Gf.OUTPPFullPath + 장 + ".jpg"`.
- 변경을 알리는 단일 지점은 없다. 기존 본보기 둘: 무대 모니터 창(`FrmLyricsScreen`, 쇼 창이 더러움 표시만 하고 250ms 타이머가 전역 상태를 읽음)과 `LiveIndicatorTimer_Tick`(`FrmMain.ModernUi.cs`, 600ms마다 상태 묶음을 이전 값과 비교).

**본창**
- 오른쪽 열: `splitContainer2.Panel2` 안에 `splitContainerOutput`(Dock Fill). 그 안이 송출 칸 머리(`panel10`), 페이지·썸네일·글(`panelOutputTop`), 단추 줄(`panel2`), 상태 줄(`panel8`), 화면 그림(`panelOutputBottom`).
- 새 UI(`FrmMain.ModernUi*.cs`)가 색과 최소 폭을 정한다: `SizeMainColumns`, `ApplyHeaderMinimumWidths`, `ApplySurface`(구조 컨트롤 목록과 제외 목록), `ChangeKeepingSplitterDistances`.
- 키: 본창에는 `KeyPreview`·`ProcessCmdKey`가 없다. 메뉴 `ShortcutKeys`(본창 전체), 포커스된 칸의 `KeyUp` → `ItemKeyPressed` → `KeyboardActionHandler`, 전역 훅(옵션이 켜져 있으면 실행 내내). F2와 F6은 본창에서 비어 있다(F2는 옵션 창 안에서만 쓴다).
- 송출 칸을 움직이는 본창 메서드: `CopyPreviewToOutput()`(미리보기 → 송출 칸, 쇼가 돌면 회중 화면까지), `MoveToSlide(item, direction)`, `OutputRichTextBox_ShowSlide(n)`.
- WPF: Axicultuss에는 `ElementHost`가 없다. 모달 WPF 창 하나(악보 편집기, `Axicultus.ScoreEditor`)는 이미 연다.

**설정과 데이터**
- 설정은 `%APPDATA%\Axicultuss\settings.json`. 환경 변수 `AXICULTUSS_SETTINGS_FILE`이 있으면 그 파일을 쓴다. 데이터 폴더는 그 파일의 `config\root_directory`(이 PC는 `C:\Axicultuss\`, 데이터 폴더가 실행 파일과 같은 곳에 있다).
- 시작할 때: 같은 이름의 다른 프로세스를 모두 끝낸다(`OverlapProcessKill`), 설정을 쓰고, 폴더를 만들고, 지금 예배 순서를 다시 저장한다. 끝날 때 창 위치와 분할 값을 쓴다. 실행 파일 폴더에 `Axicultus_Debug.log`를 쓴다.
- 테스트: `Axicultuss.Tests`(xUnit, 44개 파일)는 정적 도우미와 일부 대화상자를 본다. `FrmMain`을 만드는 테스트와 WinForms용 하네스는 없다. 지금 하네스(`tools\AxicultusHarness`)는 WPF 앱만 돌리고, 그 보호 검사는 `%APPDATA%\Axicultuss`와 `C:\Axicultuss`를 보지 않는다.

**Phase 0 대조에서 고친 사실 (2026-11-28)**
- `SongSettings`의 값들은 `Axicultus.Core\SongSettingsBase.cs`에 있다. `LyricsAndNotationsList`는 WinForms `ListView`다.
- `Gf.BuildLyricsSlideTexts`는 `gfUiText.cs`에 있고 `Slide[i,5..7]` 말고 `CurSlide`·`FolderNo`도 고친다.
- `Gf.GetSlideContents`의 영역 값은 0이 영역 1, 그 밖은 영역 2다. 공용 정적 글상자(`tbWorkspace`·`tbTempSpace`)를 쓰고 `WordWrap`을 바꾸므로 UI 스레드에서만 부른다.
- `splitContainerOutput`은 두 칸이다: Panel1에 `panel10`·`panelOutputTop`, Panel2에 `panel2`·`panel8`·`panelOutputBottom`.
- `LiveIndicatorTimer_Tick`은 따로 타이머가 없고 `TimerFlasher.Tick`에 붙어 있다. 무대 모니터 창의 250ms 타이머는 `UpdateProgress`를 부른다("더러움 표시"는 확인되지 않았다).
- `OutputRichTextBox_ShowSlide`는 private, `RemoteControlLiveShow`는 `string`을 돌려준다. `LivePowerPointRunState`는 internal enum이다.
- 장 그림은 세션 임시 폴더에 있다(`OUTPPFullPath + 장 + ".jpg"`).
- `Axicultus_Debug.log`는 `Axicultuss.dll` 옆에 쓴다. 레지스트리에는 쓰지 않는다(설정 파일이 없을 때 읽어 오기만 한다).
- 시작할 때 설정 파일은 값이 달라질 때만 쓴다. 시작 경로에서 확인된 예배 순서 쓰기는 "하나도 없을 때 빈 예배 순서 만들기"뿐이다(나머지 `SaveWorshipList` 호출은 하네스 로그로 확인한다).
- 시작할 때 앱이 PowerPoint에 붙는다(`Gf.InitAppData`의 작업 스레드 넷, `Form1_Load`의 `MainPPT.Init()`). 숨은 PowerPoint가 뜨거나, 열려 있는 PowerPoint에 붙는다.

## 2. 결정

1. **호스트 연결부는 읽기만 한다.** `Axicultuss`에 a046의 호스트 인터페이스 구현을 둔다. 송출 상태를 한 묶음(snapshot)으로 읽는 순수 함수와, 그것을 주기적으로 읽어 달라졌을 때만 알리는 타이머로 이룬다. 쇼 창과 송출 로직(`FrmLaunchShow`, `MoveToSlide*`, `LoadItem`, `Gf.DrawText*`)에는 알림 코드를 넣지 않는다. 예배 중에 쓰는 경로를 건드리지 않기 위해서다.
2. **상태 읽기 규칙**
   - 대상 항목: `Gf.ShowRunning`이면 `Gf.LiveItem`, 아니면 `Gf.OutputItem`.
   - 라이브 상태: 쇼가 돌지 않으면 꺼짐(`Off`). 돌고 있고 `ShowLiveBlack`이면 `Hidden`(회중 화면이 꺼진 것으로 다룬다. `Standby`로 두면 자막 탭이 쇼가 도는 중에도 고른 항목을 올리므로 쓰지 않는다), 아니면 송출(`Active`). `ShowLiveClear`(글 숨김)는 Axicultus가 "지우기"를 다루는 것과 같게 한다(구현자가 Axicultus의 규칙을 확인해 맞추고 design 4에 적는다).
   - 종류: P는 PPT(경로 `Path`, 장 `CurSlide`), M은 미디어, G와 빈 항목은 없음, 나머지는 글.
   - 지금 글과 페이지 목록: `GetSlideContents`로 영역 1과 2를 읽어 만든다(항목의 `Slide[,]`를 바꾸지 않는다). 페이지 표시는 "지금/전체".
   - 주기: 200ms(무대 모니터 창과 같은 급). 비교하는 값: 대상 항목의 `ItemID`·`Type`·`Path`·`CurSlide`·`TotalSlides`, `Gf.LiveLoadSerial`, `ShowRunning`, `ShowLiveBlack`, `ShowLiveClear`, `LivePowerPointRunState`, 예배 순서 이름, 고른 행. 글은 위 값이 달라졌을 때만 다시 만든다.
3. **넘기기와 올리기는 본창의 기존 메서드를 부른다.** 다음·이전은 `MoveToSlide(Gf.OutputItem, …)`, 페이지로 가기는 송출 칸의 페이지 클릭과 같은 경로, 고른 항목 올리기는 `CopyPreviewToOutput()`. 올리기는 쇼가 돌고 있으면 하지 않는다(돌고 있을 때 그 메서드는 회중 화면까지 바꾼다). 미디어 항목은 올리지 않는다.
4. **화면 놓기**: 새 partial `FrmMain.Captions.cs`에서 코드로 놓는다(`FrmMain.Designer.cs`는 고치지 않는다). `splitContainer2.Panel2`에 탭 줄(`ElementHost`, 위쪽 고정)과 자막 칸(`ElementHost`, 채움)을 넣고, 탭은 `splitContainerOutput`과 자막 칸의 보이기/숨기기로 바꾼다(자막 칸은 숨어도 계속 따라간다). 탭 줄과 자막 칸은 본창의 `Load`에서 만든다.
5. **새 UI 층과 맞추기**: 본창 최소 크기와 송출 칸 최소 폭 계산에 탭 줄과 자막 칸의 최소 폭(300)을 넣는다. `ApplySurface`의 제외 목록에 두 `ElementHost`를 넣는다. 고대비 모드처럼 새 UI가 꺼진 경우에도 탭 줄은 보인다.
6. **키**: 출력 메뉴에 항목 둘을 넣고 `ShortcutKeys`로 F2·F6을 건다. 메뉴 단축키는 포커스가 자막 칸(WPF)에 있어도 본창이 먼저 받는다(기능 키는 본창, 일반 키는 자막 칸이라는 Axicultus 규칙과 같다). 본창의 Space·화살표·글자 키는 포커스된 칸의 `KeyUp`으로만 오므로 자막 칸에 포커스가 있으면 자막 칸의 키가 된다. 전역 화살표 훅(옵션, 기본 꺼짐)이 켜져 있으면 화살표가 송출 칸도 넘긴다는 것은 그대로 두고 문서에 적는다.
7. **WPF 창과 키보드**: 자막 설정 창(WPF `Window`)은 소유자를 본창으로 하고 `ElementHost.EnableModelessKeyboardInterop`을 부른다(안 부르면 WinForms 앱에서 그 창에 글자가 들어가지 않는다). 한글 조합 입력(직접 입력 칸, 설정 창의 칸)은 하네스가 아니라 사람 손으로 확인하는 항목으로 둔다.
8. **테마**: a046의 기본 사전을 쓴다. 밝은·어두운은 Axicultuss 새 UI의 지금 모드를 따른다.
9. **저장 위치**: 자막 파일(`caption-style.json`, `obs.json`, 복구)은 설정 파일이 있는 폴더에 둔다. `AXICULTUSS_SETTINGS_FILE`이 있으면 그 파일의 폴더다. 그래야 격리 실행이 실제 프로필을 건드리지 않는다.
10. **빌드와 배포**: `Axicultuss.csproj`에 `UseWPF`, `FrameworkReference Microsoft.AspNetCore.App`, `Axicultus.Captions` 참조, OCR 도우미 복사(Axicultus의 `BuildOcrHelper`와 같은 방식)를 넣는다. SDK 이름(`Microsoft.NET.Sdk.WindowsDesktop`)은 `UseWPF`가 동작하면 그대로 둔다.
13. **회중 화면의 카메라 자막**: 쇼 창(`FrmLaunchShow`) 위에 겹치는 자막 창을 둔다. 테두리 없는 투명 WPF 창에 공용 라이브러리의 카메라 자막 보기(`CameraCaptionView`)를 놓고 자막 세션에 묶는다. Axicultus의 송출 창과 같은 보기라서 모양 규칙(폭 88%, 아래 여백 5%, 글자 5%, 줄 수, 상자, 악보 높이)이 저절로 같다.
    - 쇼 창의 그리기 코드(`ImageTransitionControl`, `Gf.DrawText*`)는 바꾸지 않는다. 쇼 창을 만들고 끝내는 곳에서 자막 창을 열고 닫는 호출만 넣는다.
    - 자막 창은 쇼 창을 소유자로 하고 쇼 창이 차지하는 회중 화면 영역(수동 영역 지정 포함)에 맞춘다. 마우스와 키보드를 받지 않고(클릭 통과, 활성화되지 않음) 포커스를 가져가지 않는다.
    - 보이는 때: 쇼가 돌고 있고, 자막이 나가는 중이고, 내보낼 곳 "카메라"가 켜져 있고, 검은 화면이 아닐 때. 그 밖에는 숨긴다(창을 없애지 않고 숨겨 깜빡임을 줄인다).
    - PowerPoint 쇼 위: PowerPoint의 슬라이드 쇼 창 위에도 자막이 보여야 한다. `NoPowerpointPanelOverlay`로 쇼 창이 0×0이 되는 경우에도 자막 창은 회중 화면 영역을 유지한다. PowerPoint 쇼 창과의 앞뒤 순서는 구현자가 확인해 design 4에 적는다(쇼 창이 지금 PowerPoint 위에 놓이는 방식과 같게 한다).
    - 대안(자막 창으로 안 되는 것이 확인될 때만): 쇼 창의 알림 띠(`TextOverlay`)를 본뜬 GDI+ 자막 띠. 이 경우 총괄에게 먼저 보고한다(쇼 창 그리기 코드를 고치게 되므로).
11. **호스트가 주지 않는 것**: 큰 장 렌더, 옛 .ppt 읽기, 라이브 카메라 항목 켜기/끄기, 예배 순서 복구는 "없음"으로 둔다. 곡 목록은 Axicultuss의 DB 접근(`SQLiteController`·`gfDatabase` 경유)으로 읽는다. 작은 장 그림은 송출 칸의 장 그림 파일에서 읽는다.
12. **끝내기**: 본창이 닫힐 때 자막을 끝낸다(복구 저장, 설정 창 닫기, 자막 서버와 OBS 연결 정리). 예배 순서가 바뀌면 자막 탭을 새로 만든다(Axicultus와 같다).

## 2b. a046 리뷰가 a047에 남긴 것 (2026-11-28)

a046 묶음 B의 적대적 리뷰가 Axicultuss 호스트에서 걸릴 점을 찾았다. Phase 1·2가 처리한다.

- **설정 창의 주인과 위치**: 자막 탭이 `ElementHost` 안에 있으면 `Window.GetWindow(tab)`이 null이라 설정 창에 주인이 없고(본창 뒤로 숨을 수 있다) 본창이 있는 화면의 작업 영역에 맞추는 계산도 건너뛴다. 라이브러리가 탭이 놓인 최상위 창의 핸들(`PresentationSource` → 최상위 HWND)로 주인과 영역을 잡게 한다. WPF 창 안에 있을 때(Axicultus)의 동작은 그대로여야 한다.
- **고른 항목의 동일성**: 같은 행이면 같은 객체를 준다(행마다 간직, 주기마다 새로 만들지 않는다).
- **송출 항목은 그때그때 읽는다**: 호스트의 `OutputItem`은 200ms 묶음이 아니라 읽는 순간의 값이어야 한다. 아니면 "송출 칸에 올리기" 직후의 두 번째 고르기 알림이 옛 값을 보고 한 번 더 올린다.
- **송출 칸이 따로 없는 호스트**: `OutputPanel`은 없음(null)으로 두고, 송출 항목의 `PptSlide`에 지금 보이는 장을 넣고 장이 바뀔 때마다 `Output` 변경을 알린다(라이브가 아닐 때 자막이 장을 따라가는 신호다).
- **새 자막 탭**: 예배 순서가 바뀌어 탭을 다시 만들면 새 탭의 탭 줄·설정 창을 다시 놓고 테마와 키보드 연동을 다시 건다.
- **복구 저장소의 항목 검사**: 예배 순서 복구를 주지 않으므로(없음) 검사 함수는 필요 없다. 복구를 주게 되면 검사 함수도 같이 준다.
- **탭 줄과 자막 칸이 다른 `ElementHost`**: 포커스가 두 창 사이를 오간다(스위치를 누른 뒤 입력 칸으로 돌아가기). 하네스와 사람 손으로 확인한다.
- **어두운 모드의 기본 컨트롤**: 앱 테마가 없으면 설정 창의 입력 칸·버튼·목록은 시스템 기본 모양(밝음)이다. 읽고 쓰는 데 문제는 없다. Axicultuss 새 UI의 모드에 맞춰 보기 싫으면 Phase 2에서 기본 사전에 최소 모양을 더한다.

## 3. 하네스 (`tools\AxicultussHarness`)

WPF 하네스와 같은 짜임이다. 실제 데이터가 있는 PC에서 쓰므로 보호 검사가 먼저다.

- **격리 사본**: `%APPDATA%\Axicultuss\settings.json`과 데이터 폴더(`config\root_directory`가 가리키는 곳의 Admin, HolyBibles, InfoScreens, Powerpoint 등 데이터 폴더만, 실행 파일 제외)를 `%LOCALAPPDATA%\AxicultussHarness\iso.pristine`에 복사하고, 사본 설정의 `root_directory`를 사본으로 바꾼다. 표식 파일을 둔다.
- **실행**: 사본을 `iso`로 다시 복사한 뒤 `AXICULTUSS_SETTINGS_FILE`을 사본 설정으로 두고 드라이버를 실행한다. 드라이버는 `Program.Main`을 거치지 않고(다른 프로세스 끝내기와 PowerPoint 정리를 하지 않는다) 필요한 초기화만 한 뒤 `FrmMain`을 만든다. 어셈블리 이름은 `Axicultuss.Tests`(내부 형식 접근)이고 `Axicultus.sln`에 넣지 않는다.
- **보호 검사**(풀거나 우회하지 않는다): 모니터 2대, `Axicultuss`·`Axicultus`·`Easislides` 프로세스가 없음, 격리 표식이 있음, 드라이버가 읽은 `root_directory`가 사본 안임(아니면 본창을 만들기 전에 끝낸다). 실행 전후로 `%APPDATA%\Axicultuss`, 실제 데이터 폴더, `%APPDATA%\Axicultus`의 파일 목록 해시를 비교해 출력한다.
- **PowerPoint**: 앱이 시작할 때 PowerPoint에 붙으므로, PowerPoint가 이미 실행 중이면 하네스를 실행하지 않는다(운영자의 PowerPoint에 붙지 않게). 실행 뒤 남은 숨은 PowerPoint는 수를 출력한다.
- **하네스 출력 폴더의 `Axicultuss.exe`**: 프로젝트 참조로 따라오는 실행 파일은 직접 실행하면 실제 설정으로 뜬다. 빌드 뒤 출력 폴더에서 지운다(드라이버는 dll만 쓴다).
- **자막 서버 포트**: 자막 탭을 여는 단계(`captiontab` 등)는 드라이버가 자막 서버를 다른 포트로 만든다. 고정 포트(17948)는 실제 OBS 브라우저 소스가 쓰는 주소다.
- **단계**: `smoke`(본창이 뜨고 닫힘, 예외·로그 기록), `captiontab`(탭 줄, F2, 화면 따라가기, 고른 항목, F6, 페이지 단계, 다른 예배 순서, 닫기). PowerPoint 쇼를 띄우는 단계는 이 변경에 넣지 않는다.

## 4. 구현 뒤 기록

구현자가 적는다: 지우기(clear)의 자막 규칙, 연결부가 본창에서 부르는 메서드 이름, 하네스 사용법.

**하네스 사용법 (Phase 0)** — 자세한 것은 `tools\AxicultussHarness\README.md`.

```powershell
pwsh -NoProfile -File tools\AxicultussHarness\New-IsolatedCopy.ps1                     # 격리 사본 (PowerShell 7.4+)
pwsh -NoProfile -File tools\AxicultussHarness\Run-Harness.ps1 -Phases smoke -Name smoke1
```

- 사본: `%LOCALAPPDATA%\AxicultussHarness\iso.pristine`(설정 파일과 데이터 폴더 `Admin`, `Backgrounds`, `Documents`, `HolyBibles`, `Html`, `Images`, `InfoScreens`, `Media`, `Music`, `Powerpoint`, `User Images` 중 있는 것, 이 PC에서 약 88MB). 설정에서 실제 루트로 풀리는 경로(어떤 표기든)는 모두 `iso\work`로 바뀐다. 실행마다 `iso`로 미러링한다. 복사도 앱이 돌면 거부하고 원본을 앞뒤로 해시한다.
- `Run-Harness.ps1`의 보호 검사(순서와 종료 코드): 모니터 2대 미만 10, 앱·드라이버 실행 중 11, PowerPoint 실행 중 15, 표식 없음 12, 드라이버 빌드 실패나 출력 폴더의 앱 exe 13, 필수 실제 폴더 없음 16. 실제 데이터가 바뀌면 30, 읽지 못하면 31, 시간 초과 40, 스크립트 오류 41. 전체 목록은 README.
- 실행 전후 비교: 내용 해시(`%APPDATA%\Axicultuss`, `%APPDATA%\Axicultus`, 실제 루트의 `Admin`), 목록 해시(실제 루트와 데이터 폴더, 있는 보호 폴더 전부, 사본의 설정과 예배 순서가 가리키는 사본 밖 폴더), 레지스트리 키 셋. PowerPoint 자신의 레지스트리와 최근 파일 기록은 비교하지 않는다.
- 드라이버는 `AXICULTUSS_SETTINGS_FILE`, `TEMP`, `TMP`를 자기 프로세스에만 받는다. 본창을 만들기 전에 로그 경로, 설정 저장소의 파일, 데이터 루트, 표식, 설정의 모든 경로(앱처럼 풀어서), 임시 폴더, 앱 어셈블리 위치를 확인하고 아니면 종료 코드 2로 끝난다. `Program.Main`의 프로세스 끝내기·PowerPoint 정리·시작 화면은 하지 않고 전역 키 후킹 옵션을 끈다.
- 실행 중에는 사용자 키·마우스 단추 입력을 앱 창에 전하지 않는다. 대화상자가 뜨면 답하지 않고 끝낸다(6). 이후 단계도 앱을 입력 주입이 아니라 메서드 호출로 움직인다.
- 앱의 `Axicultus_Debug.log`는 앱 어셈블리 옆(드라이버의 `bin`)에 생기고, 이번 실행분이 하네스 로그의 `[APPLOG]` 줄로 들어간다. 출력 폴더에는 앱의 exe와 runtimeconfig가 없다(빌드 뒤 지움).

## 5. 위험과 대응

- **예배 중 안정성**: 자막 코드의 예외가 본창을 끝내면 안 된다. 연결부의 타이머 본문과 자막 탭 만들기는 예외를 잡아 기록하고 자막만 멈춘다. 자막 탭 만들기가 실패하면 탭 줄 없이 본창은 평소대로 뜬다.
- **회중 화면의 자막 창**: 포커스를 가져가거나 쇼 창·PowerPoint 쇼 창 뒤로 숨거나, 쇼가 끝난 뒤 남으면 예배에 바로 보인다. 하네스 `captionshow` 단계가 찬양 항목으로 "보임·숨김·쇼 끝에 사라짐·포커스 그대로"를 검사하고, PowerPoint 쇼 위는 사람이 확인할 항목으로 둔다. 자막 창을 만들다 실패하면 자막만 회중 화면에 나가지 않고 쇼는 평소대로 돈다.
- **런타임 누락**: ASP.NET Core 런타임이 없는 PC에서는 앱이 뜨지 않는다. 배포 안내에 적고, 방송 PC 배포 전에 확인한다.
- **최소 폭과 분할 값**: 탭 줄과 자막 칸이 새 UI의 폭 계산과 `ChangeKeepingSplitterDistances`에 걸린다. 본창 최소 크기와 저장된 분할 값으로 다시 열었을 때의 그림을 하네스로 확인한다.
- **상태 읽기의 비용**: 200ms마다 값 비교만 하고, 글 만들기는 달라졌을 때만 한다. 긴 성경 본문(수백 페이지)에서도 송출이 끊기지 않는지 하네스로 잰다.
- **실제 데이터**: 하네스 보호 검사와 저장 위치 규칙(2.9)으로 막는다. 구현 에이전트는 실제 앱을 직접 실행하지 않는다(하네스만 쓴다).
