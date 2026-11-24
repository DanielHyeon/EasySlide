## Phase 0. 기준과 영향 확인

- [x] 0.1 a035·a036 반영 후 전체 테스트 기준 결과 기록
- [x] 0.2 하네스 `pptcaption` 실행, 변경 전 그림 보관(콘솔, 회중 창 `caption-*`, `score-size-*`, `score-only-size-*`). 화면이 1개라 하네스가 거부하면 design 12대로 테스트 안의 회중 창 카메라 자막 기준 그림을 변경 전에 만든다
- [x] 0.3 콘솔 컨트롤 이름을 쓰는 테스트·하네스 위치 목록

## Phase 1. 실패하는 테스트

- [x] 1.1 창 크기 조절·최소 크기, 위치 기억 파일 저장·복원·화면 밖 무시
- [x] 1.2 키보드: Space 전환, 입력 칸·IME 조합 중 무시, 1~4 표시 방식
- [x] 1.3 송출/숨김 상태 표시가 세션 상태를 따른다
- [x] 1.4 다음 단계 계산(같은 장 다음 줄, 다음 장, 끝)
- [x] 1.5 단계 띠 선택 시 자막만 이동(송출 화면 명령 호출 없음)
- [x] 1.6 설정 창에 OBS·카메라·씬·복구·세부 조정 컨트롤이 있고 콘솔 본문에는 없다
- [x] 1.7 접근성: 버튼·입력 이름, 레이블 중복 없음
- [x] 1.8 a036 콘솔 동작(고른 상자 유지, 선택 표시 없음, 저장 안 한 문구 유지, 상자 선택 시 줄·보는 슬라이드 유지)이 설정 창 배치에서도 통과
- [x] 1.9 회중 창 카메라 자막 변경 전후 픽셀 비교 테스트(design 12)

## Phase 2. 구현

- [x] 2.1 뷰모델 속성/명령 추가(필요한 것만)
- [x] 2.2 새 콘솔 배치(상단 상태줄, 왼쪽 조작, 오른쪽 미리보기·다음, 아래 단계 띠·모양 조절)
- [x] 2.3 미리보기 일치(권장: 회중 창 카메라 자막 공유 컨트롤)
- [x] 2.4 설정 창 분리
- [x] 2.5 키보드, 창 위치 기억
- [x] 2.6 테스트·하네스 컨트롤 이름 갱신
- Constraints: 테마 토큰만, 식별자·주석 영어, 자막 송출 규칙 불변.

## Phase 3. 회귀와 빌드

- [x] 3.1 Release 빌드 오류 0, 새 파일 EasiDS001 경고 0, 전체 테스트 통과
- [x] 3.3 화면에 띄우지 않고 그린 콘솔·설정 창 그림(design 12)을 작업 폴더에 남김
- [ ] 3.2 하네스(격리 사본): 새 콘솔 그림(직접 입력, 따라가기-자막, 따라가기-악보, 숨김 시작, 설정 창), 회중 창 그림이 변경 전과 같은지 비교, 실제 데이터 해시 unchanged

## Phase 4. 리뷰와 QA 증거

- [x] 4.1 적대적 리뷰 지적 사항 처리 — 리뷰 1~3차(별도 에이전트), 구현 3·5차. 2·3차에서 큰 지적 없음
- [x] 4.2 `/code-review` 지적 사항 처리 — 7건(중요 5건: Space 포커스, 따라가기 복귀, 혼합 배율, 첫 열기 화면, 설정 창 화면) 모두 구현 4차에서 수정, 리뷰 3차에서 재검증
- [x] 4.3 총괄 화면 캡처 검사와 완료 검증(2026-11-24) — 화면에 띄우지 않고 그린 그림(기본·최소·작은 화면, 어두운 테마, 화면 꺼짐, 설정 창)을 1~3차마다 직접 확인. 전체 테스트 3554 통과 / 22 건너뜀 / 0 실패, 회중 창 카메라 자막 픽셀 54/54, `openspec validate --strict` 통과, 추가 주석에 한글 없음. 하네스(3.2)는 화면이 1개라 미실행 — a035·a036과 같이 화면이 2개일 때 따로 실행

## 검증 결과


### 구현 1차 (2026-11-24, 구현 에이전트)

**Phase 0**
- 0.1 기준: `dotnet test Axicultus.Tests` 통과 3446 / 건너뜀 21 / 실패 0 (HEAD 6e9fb7a).
- 0.2 하네스: 화면 1개라 `Run-Harness.ps1 -Phases pptcaption`이 `GUARD: only 1 screen(s) present; not launching`(exit 10)으로 거부. 가드는 그대로 둠. design 12대로 회중 창을 바꾸기 **전에** `CameraCaptionPixelTests`를 쓰고 기준 그림 54장을 `Axicultus.Tests/Rendering/baselines/camera-caption-*.png`에 만듦(`AXICULTUS_APPROVE_BASELINES=1`, 변경 전 코드에서 54/54 통과 확인). 조합: 1280×720·960×540 × (자막: 줄 1~3 × 상자 켬/끔, 악보만: 크기 0~2, 악보+자막: 크기 0~2 × 줄 1~3 × 상자 켬/끔).
- 0.3 콘솔 컨트롤 이름을 쓰는 곳
  - `Axicultus.Tests/Shell/CaptionConsoleTests.cs`: Draft, Instant, TakeButton, ShowButton, HideButton, Status, ScreenMode, AutoShow, TextOnly, ScoreOnly, ScoreAndText, ScoreRowLabel, Slides, Boxes, Cue, 메서드 Next/Previous/SaveOverride/RefreshBoxes/Changed/SaveAsync/UpdateScore/LoadPictureAsync, 필드 _scoreDeck/_preparingScore/_pictures/_store/_plan/_main/_scorePrepared.
  - `tools/AxicultusHarness/Program.cs` `pptcaption`/`ppttext`: ScreenMode, DirectMode, HiddenBadge, ShowButton, HideButton, Status, DeckLabel, SyncShow, ScoreOnly, TextOnly, ScoreAndText, AutoShow, ScoreRowLabel, SettingsToggle, ScoreSizeBox, LinesBox, Draft, Boxes, 메서드 OpenLiveDeck/Next/Previous/SetScoreRow/PickColor, 회중 창 FindName("LowerThirdImage"/"LowerThirdArea"/"LowerThirdText").
  - 접근성 스캐너(`Axicultus.Tests/Accessibility`)는 모든 제품 XAML을 자동으로 검사(이름 목록 없음).

**이름 변경**: ShowButton·HideButton → `AirButton`(한 개의 송출/숨김 전환), SettingsToggle → `SettingsButton` + `Settings`(설정 창). Slides·Boxes·Cue·Host·Port·Password·AutoObs·AddObsSource·CameraSource·PreviewObsCamera·Scenes·SourceName·EnableSource·Scene1~4·RecoverButton·ServerLabel은 같은 이름으로 설정 창(`CaptionSettingsWindow`)에 있다(콘솔 코드 숨김은 같은 이름의 전달 속성으로 그대로 씀). 회중 창 LowerThird*는 `CameraCaption`(공유 컨트롤 `CameraCaptionView`) 안의 같은 이름. 테스트와 하네스는 같은 변경에서 고침.

**Phase 1** (새 테스트, 구현 전 컴파일 실패로 빨강 확인)
- 1.1 `TheWindowIsLargeResizableAndRemembersItsPlace`, `TheConsoleOpensLargeInTheMiddleTheFirstTime`, `TheKeptPlaceIsWhereTheConsoleOpens`, `AKeptPlaceOffEveryScreenOpensInTheMiddleAtTheDefaultSize`(화면 밖, 깨진 파일), `AKeptPlaceFitsWhenItsTitleBarIsOnAScreen`
- 1.2 `SpaceShowsAndHidesTheCaptionOutsideTheTextBoxes`, `NumberKeysChooseTheDisplayOutsideTheTextBoxes`, `ArrowKeysStepTheFollowedDeck`
- 1.3 `TheAirButtonShowsWhetherTheCaptionIsOnAir`
- 1.4 `TheNextPreviewIsTheNextRowThenTheNextSlideAndNothingAtTheEnd`, `TheNextTextIsTheChosenBoxOfTheNextSlide`, `WhileTheScoreIsMadeTheNextPreviewIsTheNextSlide`
- 1.5 `PickingAStepInTheStripMovesOnlyTheCaption`(싱크 켠 상태에서도 송출 화면·세션 변화 0), `AScoreStepGoesToItsRowInOneCaption`(악보 줄 썸네일, 한 번만 송출)
- 1.6 `ConnectionAndDeviceSettingsAreInTheirOwnWindow`
- 1.7 `EveryControlOfTheConsoleAndItsSettingsHasItsOwnName`(실행 중 자동화 이름, 창마다 중복 없음) + 기존 XAML 접근성·LabeledBy 스캐너
- 1.8 a036 테스트 4개를 설정 창이 열린 경우까지 이론 테스트로: `TheChosenBoxStaysChosenOverASlideWithoutIt`, `NewLyricsForTheSlideOnAirChangeOnlyItsCaptionText`, `AnUnsavedCaptionEditStaysWhileTheSlideOnAirMoves`, `PickingABoxOnAnotherSlideKeepsTheViewAndTheRowOnAir` (각 false/true)
- 1.9 `CameraCaptionPixelTests.TheCameraCaptionDrawsAsBefore` 54건(채널 허용오차 0)

**Phase 2**
- 새 파일: `Axicultus/Captions/CameraCaptionView.xaml(.cs)`(회중 창과 미리보기가 같이 쓰는 카메라 자막, 화면 크기에서 폭 88%·아래 5%·글자 5%·악보 높이 비율), `CaptionSettingsWindow.xaml(.cs)`, `CaptionConsole.Operator.cs`(키, 창 위치, 설정 창 연결, 단계 띠, 다음 미리보기), `CaptionWindowPlacement.cs`(`caption-window.json`).
- 콘솔 미리보기는 1920×1080 카메라 화면에 `CameraCaptionView`를 그리고 Viewbox로 축소(16:9). 다음 미리보기도 같은 컨트롤.
- 송출 규칙(숨김 시작, 싱크, 줄 수, 악보 크기, OBS 페이지, a036 상자·문구 규칙)은 바꾸지 않음. `UpdateScore`의 줄 가사 계산은 같은 식을 `RowLyrics`로 옮겨 다음 미리보기와 함께 씀.

**Phase 3**
- 3.1 Release 빌드(`dotnet build Axicultus -c Release --no-incremental`): 오류 0, 경고 156. EasiDS 경고는 모두 이번에 손대지 않은 파일(CaptionSession.cs:96 등 기존) — 새/바뀐 파일 0.
- 전체 테스트 1회차: 통과 3521 / 건너뜀 22 / 실패 0. 2회차: 통과 3521 / 건너뜀 22 / 실패 0. (+75 = 새 콘솔 테스트 21 + 픽셀 54; 건너뜀 +1 = 그림 그리기 테스트, 환경 변수 없으면 건너뜀)
- 픽셀 비교: 공유 컨트롤로 바꾼 뒤 54/54 일치(허용오차 0). 단, 전체 실행에서 다른 테스트가 공유 Application의 화면 크기 배율을 바꿔 두면 회중 창 아래 상태 띠 높이가 달라져 처음엔 여러 건이 달랐음(다른 픽셀 최대 약 9%) → 테스트 시작에 Light·Standard로 고정(ScreenshotRegressionTests와 같은 방식) 후 두 번 모두 통과.
- 3.3 그림: `CaptionConsoleTests.DrawTheConsoleForReview`(`AXI_A037_SHOTS=<폴더>`일 때만 실행). 임시 프로필, 코드로 만든 PPT(텍스트 3장, 텍스트+악보 2장, 가사 읽기는 가짜), 창을 화면에 띄우지 않고 내용만 그림. 기본(1040×720, 내용 1024×681)과 최소(900×620, 내용 884×581) 각 7장: 1-hidden-start, 2-direct-on-air, 3-follow-text, 4-follow-score-auto, 5-follow-score-text, 6-follow-score-hidden, 7-settings.
- 3.2 하네스: 화면 1개라 미실행. 하네스 코드는 새 이름으로 고쳤고(AirButton, Settings, CameraCaption 안의 LowerThird*, 설정 창 그림 `pptcaption-settings.png` 추가) 빌드 오류 0.

### 구현 2차 (2026-11-24, 구현 에이전트 · 총괄 1차 검토 반영)

- design 13 숨김 중 미리보기: `CaptionSession`에 읽기 전용 `ReadyText`/`ReadyImage`(공개하면 나갈 글·악보 줄; Emit에서 계산만 하고 송출 규칙은 그대로)를 두고, 콘솔 미리보기에 `HeldCaption`(같은 `CameraCaptionView`, 불투명도 0.5)을 숨김일 때만 그린다. "숨김 중 · 내보내지 않습니다" 표시 유지. 테스트 `WhileHiddenThePreviewShowsTheTypedCaptionDimAndNothingGoesOut`(직접 입력: 미리보기에 흐리게, `Published`·`ForObs` 빈 값, 카메라 자막 없음, 송출 후 흐린 미리보기 사라짐), `WhileHiddenThePreviewShowsTheFollowedScoreRow`(따라가는 악보 줄이 흐린 미리보기에, 송출 이미지 없음).
- design 14 작은 화면: 창이 놓일 화면 작업 영역(기억한 위치의 화면, 없으면 주 화면, 창이 만들어질 때 주 창의 화면)으로 MinWidth/MinHeight와 크기를 줄인다. 본문(맨 위 송출/숨김 줄 아래 전부)은 `Body` ScrollViewer 안이고 900×620 설계 최소보다 작으면 스크롤된다(잘리지 않음). 화면 목록은 생성자에서 주입(테스트), 기본은 `CaptionWindowPlacement.Screens()`. 테스트 `OnASmallScreenTheConsoleFitsAndItsBodyScrolls`(작업 영역 819×566: 창이 들어가고, 송출/숨김 버튼이 보이며 Body 밖, 미리보기·단계 띠·줄 수는 Body 안, 가로·세로 스크롤 가능). 콘솔 테스트는 기본으로 1920×1040 작업 영역을 준다(이 PC의 실제 화면 819×614와 무관하게 같은 결과).
- (a) 설정 창 아래 상태 줄은 콘솔 상태를 그대로 비추던 것이라 없앴다. 이제 설정 창에서 한 일(연결 포트 오류, 주소 복사, 자막 소스 추가, 씬 전환, PPT 다시 읽기 실패, 문구 저장, 복구)의 결과만 설정 창과 콘솔에 함께 쓴다(`Told`). 자막 소스 추가 안내의 "표시를 눌러야" → "송출을 눌러야".
- (b) 설정 창: 카메라 카드를 오른쪽 열(PPT 세부 조정 아래, 복구 위)로 옮기고 기본 크기 900×820, 문구 칸 높이 100. 기본 크기에서 모든 카드가 스크롤 없이 보임(그림 7-settings-default). 작으면 기존대로 스크롤.
- 콘솔 맨 위 OBS 상태는 좁으면 줄여 보이므로 글 전체를 툴팁으로도 보여 준다.
- Release 빌드 오류 0, 경고 156(1차와 같음). EasiDS 경고는 기존 줄뿐(CaptionSession.cs:99 `Color.FromArgb`는 1차의 96행이 3줄 밀린 것, 이번에 바꾼 줄 아님).
- 전체 테스트 1회차: 통과 3524 / 건너뜀 22 / 실패 0. 2회차: 통과 3524 / 건너뜀 22 / 실패 0. 픽셀 비교 54/54 유지.
- 그림 다시 그림(같은 폴더): 1~7 기본·최소 전부, 그리고 `1-hidden-start-small.png`, `4-follow-score-auto-small.png`(창 819×566 = 1024×768 125%의 작업 영역; 819×614는 작업 표시줄 때문에 창이 가질 수 없는 높이라 작업 영역으로 그림). `7-settings-default/min`은 설정 창 자신의 기본(900×820)·최소(680×480) 크기.
- 하네스: 화면 1개라 미실행(가드 그대로). 하네스 빌드 오류 0.

### 구현 3차 (2026-11-24, 구현 에이전트 · 적대적 리뷰 1차 반영, design 16–24)

- R1 대비(결정 16): 새 토큰 `Brush.Live.OnActive`/`OnStandby`/`OnHidden`(밝은: 흰/#0F172A/흰, 어두운: #0A0A0A/#0A0A0A/#FAFAFA)을 두 색 사전에 추가하고 전환 버튼·표시 배지가 씀. 테스트 `CaptionConsoleContrastTests.TheOnAirStatesReadInBothThemes`(Light·Dark × 숨김·송출·화면 꺼짐, 실제로 적용된 브러시로 4.5:1 이상).
- R2 세 상태(결정 16): `CaptionSession.ScreenOff`(라이브 Hidden/Standby, 읽기 전용, 송출 규칙 그대로) — 송출을 골랐지만 화면이 꺼지면 버튼 "송출 대기 · 화면 꺼짐 (Space)"(Live.Standby), 배지 "화면 꺼짐 · 내보내지 않습니다", 흐린 미리보기. 테스트 `WhenTheOutputScreenIsOffTheCaptionWaitsAndSaysSo`(리뷰 P1).
- R3 쇼 따라가기(결정 17): `SyncNativeSlideAsync`는 쇼의 장이 마지막으로 본 장(`_showSlide`, 따라가기 시작·라이브 장 변경 때 기록)에서 바뀌었을 때만 자막을 옮긴다. 라이브가 아닌 송출 칸 따라가기도 같은 규칙(`_outputSlide`). 테스트 `AStepPickedWhileANativeShowRunsStaysUntilTheShowTurns`(리뷰 P2: 단계 띠·◀ 이후 1초 넘게 유지, 쇼가 넘어가면 따라감).
- R4 키(결정 18): 1~4는 따라가기 모드에서만, 직접 입력에서 송출·전환 버튼을 누르면 입력 칸으로 포커스, 설정 창에서 Esc 숨김(열린 목록이 있으면 목록만 닫힘; 콘솔의 줄 수·악보 크기 목록도 같음), 싱크 켜짐에서 단계 키 자동 반복 무시. 테스트 `NumberKeysChooseTheDisplayOutsideTheTextBoxes`(직접 입력에서 무시로 고침), `ClickingTakeOrTheSwitchWhileTypingGoesBackToTheBox`(리뷰 P10), `EscHidesFromTheSettingsWindowButFirstClosesAnOpenList`(리뷰 P6), `WithSyncOnAHeldArrowDoesNotRaceTheScreen`.
- R5 고치던 문구(결정 19, a037 이전 결함): `Step()`이 보는 슬라이드를 직접 바꾸지 않고, 슬라이드 이동 처리(`CaptionChanged`)가 저장 안 한 문구가 있으면 보는 슬라이드를 그대로 둔다. 테스트 `AnUnsavedCaptionEditStaysWhenTheConsoleMovesTheCaption`(리뷰 P12: 단계 띠, 다음 버튼, ↑).
- R6 다음 미리보기(결정 20): 자막 모양이 바뀌면 다음 미리보기·단계 띠를 다시 만들고, 다음 장의 악보 그림(내용 그림 또는 렌더)이 준비되면 다시 그린다. `CaptionSession`은 모양마다 브러시를 한 번만 만들고(같은 객체), 흐린 미리보기·다음 미리보기는 내용이 같으면 바꾸지 않는다. 단계 띠는 현재 단계가 바뀔 때만 스크롤. 테스트 `TheNextPreviewFollowsTheLookAndIsNotRebuiltForNothing`(리뷰 P3).
- R7 접근성(결정 24): 단계 띠 항목 자동화 이름 "N장 · 첫 줄" / "N장 M줄". 테스트 `TheStripsStepsHaveNames`(리뷰 P4), `EveryControlOfTheConsoleAndItsSettingsHasItsOwnName`에 단계 띠 항목 컨테이너 포함.
- R8 창 위치(결정 21): 작업 영역에 맞춰 크기와 함께 위치도 안으로 옮김; `CaptionWindowPlacement.Screens()`는 모니터마다 자기 DPI(`GetDpiForMonitor`)로 환산; 주 창의 화면은 실제 창 사각형(`GetWindowRect`, 최대화도)으로 찾음; 설정 창도 콘솔이 있는 화면의 작업 영역에 맞추고 그 안에 둠(스크롤). 테스트 `AKeptPlaceIsMovedIntoTheWorkArea`(리뷰 P7 3건), `ScreensAreInEachScreensOwnScale`(섞인 배율), `AMaximizedWindowsMiddleIsWhereItIsNotWhereItWouldBeRestored`, `TheSettingsWindowFitsASmallLaptopScreen`(리뷰 P11).
- R9 좁은 맨 위 줄(결정 22): 폭 900 미만이면 "내보낼 곳" 글을 빼고 OBS 상태는 두 줄까지 줄바꿈(툴팁에 전체). 맨 위 줄 간격도 줄임. 테스트 `ANarrowBarDropsTheDestinationsLabelAndWrapsTheObsStatus`. 그림 `*-small` 다시 그림. 허용: 카메라 상태(둘째 줄)는 좁으면 말줄임.
- R10 안내 글(결정 23): "카메라 미리보기는 설정에서 켭니다"는 자막·악보 줄(흐린 미리보기 포함)이 있는 동안 숨김. `WhenTheOutputScreenIsOff…`에서 확인, 그림 6 다시 그림.
- 기타: 쓰지 않는 `ShowCommand` 삭제; 단계 띠 글자 15 → `Type.BodyLarge.FontSize` 토큰(칸 높이 44, 띠 92). S3(입력하는 대로 송출 끔): 숨김 중 미리보기는 마지막으로 송출한 글 — 테스트 `WhileHiddenWithoutTypingAsItGoesThePreviewShowsTheLastTake`. 자막 배경 상자 색은 `Color.FromArgb` 상수 대신 `CaptionStyle.BoxColor`("#CC101010", OBS 페이지와 같은 값) 문자열 상수(기존 `DefaultColor`와 같은 방식) — 픽셀 비교 그대로 54/54.
- Release 빌드 오류 0, 경고 154(2차 156; CaptionSession의 EasiDS001이 없어짐). 새/바뀐 파일의 EasiDS 경고 0.
- 전체 테스트 1회차: 통과 3542 / 건너뜀 22 / 실패 0. 2회차: 통과 3542 / 건너뜀 22 / 실패 0.
- 그림(같은 폴더, 21장): 1~8 기본·최소, `1-hidden-start-small`, `4-follow-score-auto-small`, 어두운 테마 `1-hidden-start-dark-default`, `4-follow-score-auto-dark-default`, `8-screen-off-dark-default`, 화면 꺼짐 `8-screen-off-default/min`, 설정 창 `7-settings-default/min`.
- 하네스: 화면 1개라 미실행(가드 그대로). 하네스 빌드 오류 0.

### 구현 4차 (2026-11-24, 구현 에이전트 · 적대적 리뷰 2차·`/code-review` 반영, design 17·18(마)·20·21·25)

- V1 쇼의 마지막 장(결정 17): 따라가기를 시작할 때 화면 상태와 상관없이 쇼의 장을 기록하고, 기록이 없으면 첫 쇼 확인은 기록만 한다. 라이브 장 변경도 마지막으로 본 장과 다를 때만 자막을 옮긴다(같은 장에서 다시 시작한 쇼는 넘어감이 아님). 테스트 `FollowedWhileTheScreenIsOffAPickStaysWhenItComesBack`(리뷰 P13b), `AShowRestartedOnItsLastSlideLeavesTheCaptionWhereItIs`.
- V2 화면 다시 읽기(결정 21): 운영 창은 보여 줄 때마다(`ShowOnScreen`/`PlaceOnScreen`, 주 창 메뉴가 부름) 화면 목록을 다시 읽어 그 작업 영역에 맞추고, 설정 창은 열 때마다 운영 창이 있는 화면에 맞춰 그 위에 둔다. 테스트 `TheConsoleAndItsSettingsFitTheScreensAsTheyAreEachTimeTheyOpen`(1366×728 작업 영역 안, 화면 목록 4번 읽음).
- CR1 포커스(결정 18 (마)): Tab으로 고른 버튼·체크 상자·라디오·단계 띠 항목·목록에서는 Space·화살표가 그 컨트롤의 기본 동작. 운영 창의 버튼·체크 상자·라디오·단계 띠를 마우스로 누르면 포커스를 직접 입력이면 입력 칸, 따라가기면 운영 창 본문(`Surface`, 포커스 표시 없음)으로 돌린다. 테스트 `ACheckBoxChosenWithTabKeepsItsSpace`(Space로 싱크 켜짐·송출 상태 그대로, 라디오 화살표), `AStepChosenWithTheKeyboardKeepsItsArrows`, `AMouseClickLeavesSpaceToTheOnAirSwitch`(다음 ▶ 클릭 뒤 Space = 송출, 직접 입력에서는 입력 칸). 키 테스트는 창(본문)에서 누른 키로 바꿈.
- CR2 따라가기로 돌아올 때(결정 25): 아직 따라가지 않을 때 송출 화면의 장(`FindCurrentDeck`)을 따라간다(설정 창의 오래된 선택값이 아님). 테스트 `BackToFollowingTheCaptionGoesToTheSlideOnTheScreenNow`(5장 → 직접 입력 중 송출 칸 2장 → 따라가기 = 2장; 송출 화면 PPT는 테스트용 `ScreenDeck` 자리에 둠).
- CR3 물리 픽셀(결정 21): `caption-window.json`은 물리 픽셀과 최대화 여부(`Version: 2`)를 저장하고, 이전 형식(WPF 단위)은 쓰지 않아 기본 크기로 주 창 화면 가운데에 연다. 화면 목록(`CaptionScreen`: 경계·작업 영역·배율·주 화면, 픽셀)과 화면 찾기·작업 영역 안으로 옮기기를 픽셀로 하고, 창은 보이기 전에 `SetWindowPos`(기존 `DesktopWindowPlacement.Apply`, 배율이 다른 화면으로 옮길 때 한 번 더 맞춤)로 둔다. 닫을 때의 보통 크기 사각형은 보통 상태일 때 `GetWindowRect`로 계속 기록한 값(최대화·최소화 중에도 유지). 화면 목록은 주입 가능(테스트). 테스트 `APlaceOnTheSecondScreenOfAnotherScaleStaysThere`(100% + 오른쪽 150%), `AnEarlierFormPlaceOpensAtTheDefaultSizeOnTheMainWindowsScreen`, `AKeptPlaceIsMovedIntoTheWorkArea`(4건), `AKeptPlaceFitsWhenItsTitleBarIsOnAScreen`, `OnASmallScreenTheMinimumIsTheWorkArea`, `TheWindowIsLargeResizableAndRemembersItsPlace`(픽셀로 저장).
  - 설계와 다른 점: 저장할 보통 사각형은 `GetWindowPlacement`의 작업 공간 좌표 대신 보통 상태에서 기록한 화면 좌표 픽셀을 쓰고, 복원은 `SetWindowPlacement` 대신 보이기 전 `SetWindowPos` + 최대화 상태 지정(출력 창과 같은 방식). 픽셀로 한 공간이라는 목적은 같다.
- CR4 처음 열 때(결정 21): 주 창이 있으면 주 창의 실제 사각형(최대화도)의 화면, 없으면 `Primary` 화면(목록의 첫 번째가 아님). 진짜 화면을 알기 전(생성자)에는 크기를 줄이지 않는다. 테스트 `TheFirstOpenIsOnTheMainScreenNotTheFirstListed`(주 화면이 둘째, 작업 표시줄 위쪽), `AMaximizedWindowsRectangleIsWhereItIsNotWhereItWouldBeRestored`.
- CR6 최대화 기억: 최소화 전 상태를 기록해, 최대화 → 최소화 → 닫기여도 `Maximized=true`. 테스트 `AConsoleClosedMinimizedFromMaximizedIsKeptMaximized`.
- CR7 다음 미리보기 그림 출처(결정 20): 다음 장의 첫 줄을 송출과 같은 순서(내용 그림 → 렌더 → 운영자 PowerPoint 미리보기 그림)로 찾는다. 테스트 `TheNextPreviewTakesItsRowFromTheSamePicturesAsTheRowOnAir`(렌더 없이 악보만, 운영자 미리보기 그림 → "2장 1줄").
- Release 빌드 오류 0, 경고 154(3차와 같음), 새/바뀐 파일의 EasiDS 경고 0.
- 전체 테스트 1회차: 통과 3551 / 건너뜀 22 / 실패 0. 2회차: 통과 3551 / 건너뜀 22 / 실패 0. 픽셀 비교 54/54 유지.
- 그림 21장 다시 그림(같은 폴더, 3차와 같은 목록).
- 하네스: 화면 1개라 미실행(가드 그대로). 하네스 빌드 오류 0.

### 구현 5차 (2026-11-24, 구현 에이전트 · 리뷰 3차 반영, design 18 (마)·21)

- W1 모양 목록(결정 18 (마)): 줄 수·악보 크기 목록을 마우스로 열고 고른 뒤 목록이 닫히면 포커스를 다른 클릭과 같은 곳(직접 입력 = 입력 칸, 따라가기 = 운영 창 본문)으로 돌린다. 키보드로 연 목록은 그대로 키를 가진다. 테스트 `AMousePickInALookListLeavesTheArrowsToTheSteps`(리뷰 F2: 마우스로 2줄 고른 뒤 ↓ = 다음 장, 줄 수 그대로, Space = 송출).
- W2 숨긴 최대화·최소화 창(결정 21): 다시 열 때 기억한 보통 크기 사각형이 지금 화면 어디에도 없으면(모니터 분리) 주 창 화면(없으면 주 화면) 작업 영역 가운데로 옮긴 뒤 원래 상태를 다시 준다. 테스트 `AConsoleHiddenMaximizedComesBackOnAScreenThatIsStillThere`(오른쪽 화면에서 최대화 → 화면 제거 → 주 화면 작업 영역 안, 최대화 유지, 숨김 유지).
- W3 본문 탭 정지 없음: `Surface`에 `KeyboardNavigation.IsTabStop="False"`(코드로는 포커스를 줄 수 있음). 테스트 `TabNeverStopsOnTheConsoleBodyAndStartsAtTheOnAirSwitch`(첫 탭 정지 = 송출/숨김 버튼, 한 바퀴에 본문 없음, 다음 ▶ 포함).
- W4 하네스: 콘솔을 앱과 같이 `ShowOnScreen()`으로 열고 설정 창은 콘솔의 여는 경로(`OpenSettings`)로 연다. 하네스 빌드 오류 0.
- Release 빌드 오류 0, 경고 154, 새/바뀐 파일의 EasiDS 경고 0.
- 전체 테스트 1회차: 통과 3554 / 건너뜀 22 / 실패 0. 2회차: 통과 3554 / 건너뜀 22 / 실패 0.
- 하네스: 화면 1개라 미실행(가드 그대로).
