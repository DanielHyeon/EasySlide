## Phase 0. 기준

- [x] 0.1 HEAD 전체 테스트 결과 기록
- [x] 0.2 본창 예배 순서의 선택·더블클릭·LIVE 표시 경로 확인(`WorshipListPanel.xaml`, `MainViewModel.SelectedItem/LiveRow/WorshipListDoubleClickCommand`)

## Phase 1. 실패하는 테스트

- [x] 1.1 시나리오 4개(다음 곡 송출, 고르기만 한 항목, 항목을 누른 뒤의 Space, 직접 입력 중 송출)
- [x] 1.2 키보드로 고른 목록: 화살표는 목록 선택, Enter는 송출. 빈 예배 순서에서 "송출" 버튼 비활성
- [x] 1.3 본창에서 선택·순서가 바뀌면 운영 창 목록도 같다

## Phase 2. 구현

- [x] 2.1 왼쪽 칸(따라가기·직접 입력 공용)에 목록과 "송출" 버튼. 목록은 남는 높이를 쓰고 안에서 스크롤, 조작은 위에 고정, 직접 입력 칸 최소 3줄
- [x] 2.2 본창 명령·속성만 사용(새 송출 경로 없음). 기존 포커스·키 규칙(마우스 클릭 뒤 Surface로, Tab으로 고른 컨트롤은 제 키 유지)에 목록 포함
- Constraints: 식별자·주석 영어, UI 글은 한글. 운영 창 기본·최소 크기 그대로. 밝은·어두운 테마 모두 읽을 수 있게.

## Phase 3. 회귀

- [x] 3.1 Release 빌드 오류 0, 전체 테스트 두 번 통과(운영 창 화면 사진 기준 갱신 시 바뀐 이유 기록)
- [x] 3.2 하네스(화면 2개, 격리 사본): 운영 창 최대화, 따라가기에서 예배 순서 2번 항목 송출 → 송출 화면·LIVE 표시·자막이 2번 항목, 실제 데이터 unchanged

## Phase 4. 리뷰

- [x] 4.1 적대적 리뷰 지적 사항 처리 — 6건(큰 것: 항목 더블클릭 미송출·빈 곳 더블클릭 송출, 본창 예배 순서도 같은 기존 버그). 아래 기록
- [x] 4.2 `/code-review` 지적 사항 처리 — 3회(곡 목록·검색·제목 찾기 더블클릭 기존 버그, Enter 반복 송출, Shift 더블클릭 다른 항목 송출, 미리보기 상태로 송출 등). 아래 기록
- [x] 4.3 총괄 완료 검증(2026-11-25) — Release 전체 테스트 3617 통과 / 22 건너뜀 / 0 실패, `openspec validate --strict` 통과, 추가 줄의 한글은 UI 글·테스트 자료만, 하네스 consolelist 13개 PASS·실제 데이터 unchanged, 하네스 사진 확인

## 검증 결과

### 구현 에이전트 (2026-11-25)

- **0.1 HEAD(6548c1a) Release 전체 테스트**: Axicultus.Tests 3589 통과 · 22 건너뜀 · 실패 0, Axicultuss.Tests 438 통과 · 2 건너뜀.
- **0.2 경로**: 본창 `WorshipListPanel.xaml`의 `QueueList`는 `Queue`/`SelectedItem`(TwoWay)을 쓰고, 더블클릭은 `MouseBinding LeftDoubleClick → WorshipListDoubleClickCommand`(미디어는 재생, 그 밖은 `PreviewToLiveAsync`), LIVE는 `LiveRowBadgeConverter`([행, `LiveRow`] 참조 비교)다. 운영 창은 이것들을 그대로 쓴다(새 송출 경로 없음).
- **1.x 테스트**(`Axicultus.Tests/Shell/CaptionConsoleWorshipListTests.cs`, 14개): 다음 곡 송출(LIVE 표시·자막 따라감), 고르기만 한 항목, 항목을 누른 뒤의 Space, 직접 입력 중 송출("환영합니다" 유지, 포커스는 입력 칸, 입력 칸 Enter는 자막 송출만), Tab으로 고른 목록의 화살표·Enter, 더블클릭 바인딩, 빈 예배 순서(버튼 비활성·안내 글), 본창 선택·순서·추가 따라가기, 배치(최대화·기본·최소 × 따라가기·직접 입력). HEAD에서는 컨트롤이 없어 컴파일부터 실패한다.
- **2.x 구현**:
  - `CaptionConsole.xaml`: 왼쪽 칸을 `SourcePanel`(ScrollViewer) 안의 두 줄로 바꿨다. 위는 표시 방식별 조작(직접 입력: 입력 칸·송출·입력하는 대로·안내 / 따라가기: 표시 방식·이전/다음·카드·싱크), 아래(`WorshipPanel`, DataContext = 본창 `MainViewModel`)는 예배 순서 `WorshipList`와 "송출" `SendItemButton`. 목록 행은 제목과 LIVE 표시(본창과 같은 변환기), 비면 "예배 순서가 비어 있습니다".
  - 항목 더블클릭(행의 `MouseDoubleClick`, 왼쪽 버튼만 — 1차의 목록 전체 `MouseBinding`은 4.1에서 바꿈)·Enter(`KeyBinding`, 목록에 키보드 포커스가 있을 때만)·"송출"은 모두 `WorshipListDoubleClickCommand`다.
  - `CaptionConsole.Operator.cs` `FitSourceColumn`: 입력 칸 높이를 글자 3줄로 맞추고(인터페이스 크기 따라감), 칸의 최소 높이를 "조작 + 목록 2행 + 송출 버튼"으로 둔다. 창이 더 작으면 칸이 스크롤되어 버튼이 잘리지 않는다.
  - 포커스·키: 기존 규칙 그대로(`ClickedControl`이 이미 `ListBoxItem`/`ListBox`를 포함). 마우스로 고르거나 "송출"을 누르면 따라가기에서는 운영 창(Space·화살표는 자막), 직접 입력에서는 입력 칸으로 돌아간다. Tab으로 고른 목록은 화살표·Space를 그대로 쓴다.
- **스펙과 다른 점**:
  - 목록 최소 높이는 3행이 아니라 2행이다. 기본 크기(1040×720)에서 따라가기 + 단계 띠가 보이면 왼쪽 칸에 남는 높이가 약 2.3행뿐이라, 3행이면 기본 크기부터 스크롤된다. 최대화(1080p)에서는 목록이 약 360px(10행 이상)이다.
  - 직접 입력 칸은 이전에는 남는 높이를 모두 썼지만 이제 3줄 고정(넘치면 칸 안에서 스크롤)이다. 목록이 남는 높이를 쓴다.
  - 최소 크기(900×620)에서는 두 방식 모두 왼쪽 칸이 스크롤된다(잘리지 않음).
- **함께 고친 기존 버그(테스트)**: `ANarrowBarDropsTheDestinationsLabelAndWrapsTheObsStatus`는 다른 테스트가 Application을 먼저 만들었을 때만 통과했다(혼자 돌리면 본문 글자가 12 DIP라 줄이 바뀌지 않아 HEAD에서도 실패). 새 테스트로 실행 순서가 바뀌자 드러났다. 운영 창 리소스에 앱의 본문 크기(16)를 넣어 순서와 무관하게 했다.
- **알려진 기존 문제(고치지 않음)**: `--filter "FullyQualifiedName~CaptionConsole"`처럼 운영 창 테스트와 "WPF Application" 묶음만 함께 돌리면 시작 직후 두 STA 스레드가 동시에 BAML을 읽다가 `PackagePart.CleanUpRequestedStreamsList`의 `ArgumentOutOfRangeException`이 날 수 있다. HEAD에서도 같은 필터로 재현된다(Contrast 테스트 1건). 전체 실행에서는 나지 않았다.
- **3.1**: `Axicultus.sln` Release 빌드 오류 0.
  - Axicultus.Tests: 3603 통과 · 22 건너뜀 · 실패 0, 두 번.
  - Axicultuss.Tests: 438 통과 · 2 건너뜀, 두 번.
  - 운영 창 화면 사진 기준(픽셀 비교)은 없다. 검토용 사진 테스트(`DrawTheConsoleForReview`, `AXI_A037_SHOTS`)에 예배 순서 6항목을 넣어 새 목록이 사진에 나오게 했다. 밝은·어두운 테마 모두 확인했다.
- **3.2 하네스**: `tools\AxicultusHarness\Run-Harness.ps1 -Phases 'consolelist' -Name 'a041-consolelist2'`(새 단계 `consolelist`).
  - 운영 창 최대화, 따라가기, 1번 "우리교회말씀"(곡) 송출 뒤 운영 창 목록에서 2번 "■산소망찬양대.txt" 고름(송출 화면은 1번 그대로) → "송출": `PASS` — 송출 화면·LIVE 행·운영 창 LIVE 표시·자막이 2번 항목.
  - 직접 입력 "환영합니다" 송출 중 1번 항목 "송출": `PASS` — 송출 화면·LIVE는 1번, 자막은 "환영합니다" 그대로.
  - 실제 데이터 unchanged(`%APPDATA%\Axicultus` F11E147ABCEF4267, `C:\Axicultus\Admin` 638DB2FF156A75CF, `C:\EasiSlides\Admin` 505B6B1528D81EAE), POWERPNT 0 → 0.

### 4.1 적대적 리뷰 반영 (구현 에이전트, 2026-11-25)

- **더블클릭(주요)**: 목록 전체에 건 `MouseBinding LeftDoubleClick`은 항목이 마우스 누름을 먼저 처리해 항목 더블클릭으로는 송출되지 않고, 빈 곳 더블클릭이 고른 항목을 송출했다. 본창 예배 순서(`WorshipListPanel.xaml`)도 같은 기존 버그였다. 두 목록 모두 `MouseBinding`을 지우고 행 스타일의 `EventSetter MouseDoubleClick`(왼쪽 버튼, `CanExecute`일 때만 `WorshipListDoubleClickCommand`)으로 바꿨다. 본창의 끌어 놓기·오른쪽 클릭·여러 항목 선택·미리보기는 그대로다. `WorshipListPanelTests`의 XAML 검사도 바꿨다.
- **목록 스크롤**: 고른 항목이 바뀌면(본창, 다음 항목 등) 운영 창 목록이 그 행으로, `LiveRow`가 바뀌면 송출 중인 행으로 스크롤한다.
- **작은 창**: 목록·입력 칸 위의 휠은 그 칸이 끝에 닿았거나 스크롤할 것이 없으면 왼쪽 칸을 스크롤한다. 직접 입력에서 "송출"을 누른 뒤 입력 칸으로 포커스가 돌아갈 때 왼쪽 칸 스크롤 위치를 그대로 둔다(전에는 맨 위로 튀어 목록·송출 버튼이 가려졌다).
- **입력 칸 높이**: `FontFamily.LineSpacing × 글자 크기 × 3`(+ 안쪽 여백·테두리·1)로 바꿨다(1.4배 계산은 약 4줄이었다). 보통 103.9px, 시니어 138.3px. 3줄은 스크롤 없이 보이고 4줄째부터 스크롤된다(하네스로 확인).
- **테스트**:
  - 이름만 확인하던 더블클릭 테스트를 바꿨다. 실제로 라우팅된 `MouseDown`(`ClickCount` 2)을 보낸다. 항목 제목 → 송출, `ItemsPresenter`(빈 곳) → 송출 없음, 오른쪽 더블클릭 → 송출 없음.
  - 키보드 테스트는 ↓가 목록 선택을 실제로 옮기고 Enter가 그 항목을 송출하는지 본다.
  - 배치 테스트에 시니어(1.4배)를 더하고, 3줄은 들어가고 4줄은 스크롤되는지 실제 줄 간격으로 본다.
  - 새 테스트: 고른 행·송출 중인 행이 목록에 보이는지, 작은 창에서 휠이 왼쪽 칸으로 넘어가는지, 송출 뒤에도 스크롤 위치가 그대로인지.
- **리뷰 요청과 다른 점**:
  - 배치 테스트에 `appLook: true`를 쓰지 않았다. 테스트마다 STA 스레드가 새로 생기므로, 한 테스트가 만든 Application의 스타일을 뒤 테스트들이 다른 스레드에서 읽다가 실패한다(1차에서 `XamlParseException`으로 확인). 대신 운영 창 리소스에 앱의 글자 크기(보통·시니어)를 넣어 앱 유무와 상관없이 같게 했다. 실제 앱 모양(WPF UI 스타일)에서의 확인은 하네스로 한다.
  - 본창 목록 더블클릭은 단위 테스트로 만들지 못했다. `WorshipListPanel`은 생성자에서 `FrmMainButton` 같은 앱 컨트롤 사전을 StaticResource로 읽는데, 테스트용 Application("WPF Application" 묶음)에는 이 사전이 없다. 전역 Application 리소스를 테스트에서 바꾸면 병렬로 도는 다른 테스트가 영향을 받는다. 그래서 XAML 검사(EventSetter가 있고 `MouseBinding`은 없음)와 하네스의 실제 더블클릭으로 확인했다.
- **검증**:
  - Release 빌드 오류 0.
  - Axicultus.Tests: 3609 통과 · 22 건너뜀 · 실패 0, 두 번.
  - 하네스 `Run-Harness.ps1 -Phases 'consolelist' -Name 'a041-r2-consolelist2'`: `[CONSOLELIST]` 9건 모두 PASS.
    - 2번 송출
    - 운영 창 빈 곳 더블클릭(송출 없음)·5번 항목 더블클릭(송출)
    - 본창 빈 곳 더블클릭(송출 없음)·1번 항목 더블클릭(송출)
    - 직접 입력 중 2번 송출(자막 "환영합니다" 그대로)
    - 최소 창·시니어에서 송출 뒤 칸 스크롤 84 → 84, 입력 칸 키보드 포커스
    - 입력 칸 3줄 확인(보통·시니어)
  - 실제 데이터 unchanged: `%APPDATA%\Axicultus` B746FDC762C708AA(260), `C:\Axicultus\Admin` 638DB2FF156A75CF(93), `C:\EasiSlides\Admin` 505B6B1528D81EAE(92). POWERPNT 0 → 0.
  - `%APPDATA%\Axicultus` 해시가 1차(F11E147ABCEF4267)와 다르다. 두 실행 사이에 바뀌었고, 이번 실행 전후로는 같다.

### 4.2 `/code-review` 반영 (구현 에이전트, 2026-11-25)

- **송출 중인 행으로의 스크롤**: `MainViewModel`은 예배 순서가 바뀔 때마다, 송출 항목이 바뀔 때마다 `LiveRow` 변경을 알린다. 그래서 운영자가 목록을 스크롤하거나 타자하는 중에도 목록(작은 창이면 왼쪽 칸 전체)이 송출 중인 행으로 튀었다.
  - 이제 `LiveRow`가 실제로 다른 행으로 바뀔 때만 스크롤한다.
  - 목록의 행 보이기(BringIntoView)는 목록만 움직이고 왼쪽 칸은 그대로 둔다. 목록에 키보드 포커스가 있을 때(Tab·화살표)는 왼쪽 칸도 따라간다.
- **Enter 누르고 있기**: `KeyBinding`은 자동 반복마다 다시 송출했다. 목록의 `KeyDown` 처리기로 바꾸고 `IsRepeat`는 무시한다.
- **더블클릭한 행**: 처리기가 더블클릭한 행이 아니라 `SelectedItem`을 송출해서, Shift·Ctrl 더블클릭이면 기준 항목(앵커)이 나갔다.
  - 새 `Axicultus/Input/RowDoubleClick.cs`(행 스타일에 거는 연결 속성 `RowDoubleClick.Command`)로 바꿨다. 더블클릭한 행 하나만 선택하고, 명령을 실행할 수 있을 때만 실행한다. 왼쪽 버튼만 받는다.
  - 운영 창 목록, 본창 예배 순서가 함께 쓴다. 처리기 두 개(`SendDoubleClicked`, `QueueItem_MouseDoubleClick`)는 지웠다.
- **왼쪽 칸 맞춤**: `FitSourceColumn`이 조작 크기가 바뀔 때만 돌아서, 첫 배치 때 예배 순서가 비어 있으면 행 높이 추정이 고쳐지지 않았다. 입력 칸 글꼴이 바뀌어도 다시 맞추지 않았다.
  - 이제 행이 만들어질 때(`ItemContainerGenerator.StatusChanged`, 배치 뒤)와 입력 칸의 글자 크기·글꼴이 바뀔 때도 다시 맞춘다. 창을 닫으면 구독을 푼다.
  - 3줄 높이는 첫 글꼴의 `LineSpacing`이 아니라 한글 세 줄의 `FormattedText` 높이로 잰다(한글이 대체 글꼴로 그려지는 경우 포함).
- **본창의 같은 기존 버그**(목록 전체 `MouseAction="LeftDoubleClick"`: 행 더블클릭은 추가하지 않고, 빈 곳 더블클릭은 고른 곡을 추가):
  - `MainWindow.xaml` 곡 목록 `LibrarySongList`(`AddSelectedLibrarySongCommand`)
  - 검색 결과 `SearchResultList`(`AddSearchedSongCommand`)
  - 제목 조회 결과(`AddLookupTitleCommand`)
  - 셋 모두 `RowDoubleClick.Command`로 바꿨다.
  - 프로젝트 전체를 찾아본 결과 `MouseAction="LeftDoubleClick"`이 남은 곳은 PPT 썸네일 단추 두 곳(`ReplayPreviewPowerPointSlideCommand`, `ReplayOutputPowerPointSlideCommand`, 단추 하나하나에 건 바인딩)뿐이다. 실험(단추 글자에서 라우팅한 `ClickCount` 2 누름)으로 이 둘은 제대로 동작함을 확인해 그대로 두었다.
- **테스트**:
  - 새 `Axicultus.Tests/Input/RowDoubleClickTests.cs`(ListBox, 여러 항목 선택 ListView): 행 더블클릭은 그 행 하나를 선택해 명령 실행, 빈 곳·오른쪽 버튼·실행할 수 없는 명령은 아무것도 안 함, Shift 상태(앵커가 첫 선택)에서 더블클릭한 행이 실행.
  - `WorshipListPanelTests`·`MainMenuBarTests` XAML 검사를 바꿨다(`GoLiveCommand`가 아님도 확인).
  - 운영 창 테스트 추가:
    - Enter 자동 반복 무시
    - Shift 더블클릭
    - 같은 송출 행에서 목록이 바뀌어도 스크롤 유지
    - 작은 창에서 다른 곳에서 고른 행이 왼쪽 칸을 움직이지 않음
    - 빈 목록 뒤 행이 생기면 실제 행 높이로 다시 맞춤
    - 입력 칸 글꼴이 바뀌면 다시 맞춤
  - 테스트 주석의 한글을 영어로 바꿨다.
- **검증**:
  - Release 빌드 오류 0.
  - Axicultus.Tests: 3613 통과 · 22 건너뜀 · 실패 0, 두 번.
  - 하네스 `Run-Harness.ps1 -Phases 'consolelist' -Name 'a041-r3-consolelist4'`: `[CONSOLELIST]` 12건 모두 PASS. 새로 더한 것:
    - Shift 더블클릭: 1번을 고른 채 5번 더블클릭 → 5번 송출
    - 곡 목록: 빈 곳 더블클릭 추가 0, "가난한자돌봐주며517(새)" 더블클릭 추가 1
  - 실제 데이터 unchanged: `%APPDATA%\Axicultus` B746FDC762C708AA(260), `C:\Axicultus\Admin` 638DB2FF156A75CF(93), `C:\EasiSlides\Admin` 505B6B1528D81EAE(92). POWERPNT 0 → 0.
  - 하네스 작은 창 검사에서 숫자로 스크롤한다. `ScrollToBottom`은 나중에 늘어난 칸의 맨 아래를 따라가므로 위치 유지 검사가 거짓 실패한다.

### 4.2 두 번째 `/code-review` 반영 (구현 에이전트, 2026-11-25)

- **송출 버튼·전환 버튼 뒤의 스크롤**: 입력 중에 자막 송출 버튼이나 송출/숨김 전환 버튼을 누르면 왼쪽 칸이 입력 칸 쪽으로 튀었다. 이제 `ReleaseFocus`와 `BackToDraft`가 같은 도우미 `FocusKeepingColumn`으로 왼쪽 칸 스크롤 위치를 지킨다.
- **휠 한 칸**: Windows 설정이 "한 번에 한 화면"이면 `WheelScrollLines`가 -1이라, 방향이 뒤집히고 16 DIP만 움직였다. `WheelStep`을 두어 0 이하일 때는 한 화면만큼 움직인다.
- **휠 건너뛰기**: 열린 드롭다운이나 팝업에서 온 휠은 왼쪽 칸으로 넘기지 않는다.
- **송출 중인 행이 안 보이던 경우**: 운영 창이 숨겨져 있거나(창을 닫으면 숨는다) 목록이 아직 그 행을 받지 못했으면, 그 행을 "보였다"고 기록하지 않는다. 운영 창이 다시 보일 때(`IsVisibleChanged`)와 목록이 바뀐 뒤(`Loaded` 우선순위)에 다시 시도한다.
- **행 높이**: 첫 행이 아니라 만들어진 아무 행의 높이를 쓰고, 마지막으로 잰 값을 기억한다. 긴 목록을 스크롤해도 칸의 최소 높이가 흔들리지 않는다.
- **하네스 곡 목록 검사**: 예배 순서에 더한 곡을 `finally`에서 지우고, 미뤄 둔 저장을 바로 실행해 격리 사본의 예배 순서 파일도 원래대로 둔다.
- **테스트 추가**:
  - 숨긴 동안 송출한 행이 다시 보이면 목록에 나타남
  - 휠 한 칸 크기(3줄·-1·0)
  - 작은 창에서 자막 송출 버튼을 눌러도 칸 위치 유지
  - 긴 목록을 끝까지 스크롤한 뒤에도 행 높이로 최소 높이 유지
  - 끝까지 스크롤한 뒤 첫 행이 정말 사라졌는지는 확인하지 못한다. 150항목에서도 첫 행이 남아 있었다.
- **검증**:
  - Release 빌드 오류 0.
  - Axicultus.Tests: 3616 통과 · 22 건너뜀 · 실패 0, 두 번.
  - 하네스 `-Name 'a041-r4-consolelist'`: `[CONSOLELIST]` 12건 모두 PASS. 예배 순서는 21항목으로 원래와 같고, 저장도 했으며 남은 저장이 없다.
  - 실제 데이터 unchanged: B746FDC762C708AA / 638DB2FF156A75CF / 505B6B1528D81EAE. POWERPNT 0 → 0.

### 4.2 세 번째 `/code-review` 반영 (구현 에이전트, 2026-11-25)

- **송출 페이지(기존 동작의 빈틈)**: 본창이 라이브러리 곡·성경·찬송가 같은 원본 미리보기를 보여 주는 동안 운영 창에서 고른 예배 순서 항목을 송출하면, 그 항목이 미리보기의 페이지 상태로 나갔다(예: 3번째 절). 본창 목록은 고른 행을 다시 누르면 미리보기를 되돌리지만, 운영 창은 누르지 않고 송출하기 때문이다.
  - `MainViewModel.WorshipListDoubleClickAsync`가 송출 전에 원본 미리보기를 고른 항목으로 되돌리고, 내용 적재를 기다린다(`ShowWorshipSelectionPreviewAsync`).
  - 원본 미리보기가 없으면 바로 끝나므로, 본창 더블클릭(첫 누름에서 이미 되돌림)은 그대로다.
  - 테스트: 라이브러리 곡 3번째 절을 보는 중에 운영 창에서 송출하면 고른 곡의 1번째 절이 나간다. 수정 전에는 실패했다(페이지 2).
- **스크롤마다 다시 맞추던 것**: 행이 만들어지거나 사라질 때(긴 목록을 스크롤할 때마다)는 행 높이가 바뀐 경우에만 왼쪽 칸을 다시 맞춘다.
- **여러 항목 선택의 정리**: `RowDoubleClick`이 다른 선택을 하나씩 지우지 않고 `ListBox.SetSelectedItems`로 한 번에 바꾼다. 선택 변경 알림이 한 번만 가서, 고를 때마다 미리보기를 다시 그리는 목록도 한 번만 그린다. 테스트로 확인했다.
- **하네스 곡 목록 검사**: 검사 뒤 왼쪽 탭을 원래대로 되돌린다. 예배 순서 복원이 저장되지 않았거나 저장이 남아 있으면 FAIL로 적는다.
- **검증**:
  - Release 빌드 오류 0.
  - Axicultus.Tests: 3617 통과 · 22 건너뜀 · 실패 0, 두 번.
  - 하네스 `-Name 'a041-r5-consolelist'`: `[CONSOLELIST]` 13건 모두 PASS(복원 검사 포함). 끝난 뒤 예배 순서는 21항목이다.
  - 실제 데이터 unchanged: B746FDC762C708AA / 638DB2FF156A75CF / 505B6B1528D81EAE. POWERPNT 0 → 0.
