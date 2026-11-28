# Design — a046 자막 공용 라이브러리

## 1. 현재 구조 (영향 근거)

CodeGraph CLI는 이 세션에서 쓸 수 없었다. 아래는 2026-11-28 읽기 전용 조사 3건(자막 코드 의존성, Axicultuss 송출 상태, Axicultuss 본창·키·설정·테스트)의 결과다. 구현자는 Phase 0에서 줄 번호를 다시 확인한다.

- `MainViewModel`을 직접 쓰는 자막 파일은 셋이다: `CaptionConsole.xaml.cs`, `CaptionConsole.Operator.cs`(같은 partial 클래스), `QueueRecoveryJournal.cs`.
- `CaptionConsole`이 `MainViewModel`에서 쓰는 것:
  - 예배 순서: `CurrentWorshipListName`, `SelectedItem`, `Queue`, `LoadRecoveredQueue`, `QueueReplacing`, `PropertyChanged`
  - 송출 칸: `OutputItem`(Id, Title, ContentPath, SlideNumber, Kind), `OutputLyricsText`, `HasOutputLyricsText`, `OutputLyricsPages`, `OutputPowerPoint`·`PowerPoint`(State, PreviewImage, SlideNumber, LoadedContentPath)
  - 명령: `SendWorshipSelectionToOutputCommand`, `NextOutputSlideCommand`, `PreviousOutputSlideCommand`, `GoToOutputLyricsPageCommand`, `ToggleOutputCameraCommand`
  - 라이브: `Session.Current`(State, PptPath, PptSlideNumber), `Session.SessionChanged`
  - 기타: `Media.MediaType`·`Media.Unload`(라이브 카메라 내리기), `SongDatabasePath`, `Library.ReadSongsAsync`
  - `QueueRecoveryJournal`은 위의 `Queue`(`CollectionChanged`), `SelectedItem`, `CurrentWorshipListName`, `QueueReplacing` 말고도 `WorshipListHasUnsavedChanges`와 `StatusText`(저장 실패 알림)를 쓴다.
- `CaptionSession`은 `ILiveSessionService`에서 `Current.State`, `PptPath`, `PptSlideNumber`, `SessionChanged`(인자의 `Snapshot`)만 쓰고, `IPowerPointSlideShowControl`에서 `ReadOutputPositionAsync(path)`만 쓴다. 쇼의 장을 읽는 동안 송출이 바뀌었는지는 `Current`의 참조 비교(`ReferenceEquals`)로 가린다. 그래서 라이브 인터페이스의 `Current`는 송출이 바뀔 때까지 같은 객체여야 한다.
- 앱 형식에 묶인 나머지: `UiText`·`{loc:Text}`(Axicultus 어셈블리의 resx), `LiveState`(`Controls\EsLiveIndicator.cs` 안의 enum), `LiveItemKindMatcher`, `LiveQueueItem`·`WorshipListItemDto`(복구 저장), `IPowerPointRenderService`, `PowerPointWorker`(옛 .ppt 읽기, 앱 exe를 다시 실행), `CameraOwnership`, `OperationalErrorLog`, `LyricsDisplayFormatter.ReadLines`, `LyricWordWrapping`, `CaptionWindowPlacement`가 쓰는 `Platform.*`·`OutputWindowPlacement`.
  - 파일별로 보면(Phase 0.3 대조): `ScoreLayout`, `CaptionFit`, `CaptionBroadcastServer`, `CaptionConsoleViewModel`(`SameItemConverter` 포함)은 앱 형식을 쓰지 않는다. `CaptionDeck`, `LyricsReader`, `ObsConnection`, `ObsCaptureInputs`, `ObsOperator`는 `UiText`만 쓴다. `SongLyrics`는 `LyricsDisplayFormatter.ReadLines`만, `CameraCaptionView.xaml`은 `LyricWordWrapping`만 쓴다.
  - `LyricsDisplayFormatter.ReadLines`는 같은 클래스의 `NormalizeText`·`StripInlineNotation`·`IsMarkerOnlyLine`을 쓰고, 이 셋은 앱의 가사 표시 여러 곳이 쓴다.
  - `CaptionWindowPlacement`는 `Platform.SystemDisplayReader.GetDpiScale`(`Platform\DisplayService.cs`), `Platform.DesktopWindowPlacement.Apply`(`Platform\DesktopWindowPlacement.cs`, 송출·미리보기 창도 쓴다), `Shell.OutputWindowPlacement`, `Axicultus.Interop.DesktopWindows.GetWindowRect`(`Axicultus.Win32`)를 쓴다. `GetDpiScale`의 `Windows.Win32.PInvoke`는 `Axicultus.Win32`가 공개로 만든 것이다.
  - `CaptionRecoveryStore`는 복구 파일의 `Items`를 `WorshipListItemDto[]`로 저장하고, 읽을 때 각 항목의 `Id`·`Title`·`Kind`가 있는지 검사한다(없으면 `.bak`으로 간다). `Capture`·`Restore`가 `LiveQueueItem`과 바꾼다. 쓰는 곳은 `CaptionConsole`과 `QueueRecoveryJournal`이다.
  - `LyricWordWrapping`의 XAML 사용처는 `Theme\EasiDS.xaml` 하나다(그 파일의 `rendering:` 형식은 이것뿐). 코드에서는 `ForDisplay`를 `Controls\LyricParagraphSurface.cs`, `Shell\TextFilePagination.cs`가 쓴다.
  - 자막 XAML 밖에서 자막 형식을 XAML로 쓰는 곳: `OutputWindow.xaml`(`captions:CameraCaptionView`). `CaptionConsole.xaml`은 `captions:CameraCaptionView`와 `captions:SameItemConverter`를 쓴다.
- 문구: 자막 코드가 쓰는 영역은 `CaptionConsole`(151개 키)과 `CaptionSettingsWindow`(40개 키) 둘뿐이고, 영어·한국어 키 집합이 같다. 자막 코드 밖에서는 `Axicultus.Tests\Localization\UiLanguageFlowTests.cs`가 `CaptionConsole.Tab`을 읽는다. `UiTextCoverageTests`는 `Axicultus` 폴더의 소스와 resx만 본다.
- 테마 키(자막 XAML 3개가 쓰는 것): `Brush.Surface.Page|Subtle|Card`, `Brush.Text.Primary|Secondary|Tertiary|Inverse`, `Brush.Border.Default|Strong|Focus`, `Brush.Accent.Primary|PrimaryHover|PrimaryPressed`, `Brush.Live.Active|Standby|Hidden|OnActive|OnStandby|OnHidden`, `Font.Primary`, `Type.Body|BodyLarge|Button|Title3.FontSize`. 정의는 `Axicultus\Theme\Tokens\`. 코드에서는 `CaptionConsole.xaml.cs`가 `Brush.Accent.Primary`·`Brush.Border.Strong`을 `SetResourceReference`로 쓴다(목록 안에 있음). `CameraCaptionView.xaml`은 테마 키를 쓰지 않는다. `Font.Primary`는 `pack://application:,,,/Axicultus;component/Assets/Fonts/#Pretendard Variable, Segoe UI Variable, Segoe UI, Malgun Gothic, sans-serif`(`Tokens\Typography.xaml`)다.
- OCR 도우미: `LyricsReader.HelperPath`는 `AppContext.BaseDirectory\ocr\Axicultus.Ocr.exe`다. 실행 파일 폴더 기준이라 호스트에 묶이지 않는다. 복사는 `Axicultus.csproj`의 `BuildOcrHelper` 대상이 `$(OutDir)ocr\`로 한다.
- 테스트: `CaptionConsoleTests.WithConsole`이 앱의 실제 DI 컨테이너와 실제 `MainViewModel`로 자막 탭을 만든다. 자막 테스트 파일은 `Axicultus.Tests\Shell\Caption*.cs`, `CameraCaption*.cs`, `ObsCaptionConnectionTests.cs` 등이다. `CaptionSession`을 `LiveSessionService`로 직접 만드는 곳이 5개 파일에 22곳 있다(`CameraCaptionPixelTests`, `CameraCaptionTests`, `CaptionAllTextTests`, `CaptionConsoleTests`, `CaptionIntegrationTests`). 하네스(`tools\AxicultusHarness`)의 어셈블리 이름은 `Axicultus.Tests`라 같은 `InternalsVisibleTo`를 쓴다.

## 2. 결정

1. **프로젝트**: `Axicultus.Captions`(`net11.0-windows7.0`, `UseWPF`, `UseWindowsForms`(색 대화상자·화면 목록), `FrameworkReference Microsoft.AspNetCore.App`, CommunityToolkit.Mvvm). `Axicultus.Core`와 `Axicultus.Win32`를 참조한다. Axicultus 앱이나 Axicultuss를 참조하지 않는다. 내부 형식은 `InternalsVisibleTo`로 `Axicultus`, `Axicultus.Tests`, `Axicultuss`, `Axicultuss.Tests`에 연다(하네스는 `Axicultus.Tests`·`Axicultuss.Tests` 이름을 쓴다).
2. **호스트 인터페이스**: 자막 탭이 앱에서 받는 것을 라이브러리의 인터페이스로 둔다. 이름은 구현자가 정하되 아래 책임을 모두 담고, 앱 어셈블리의 형식이 드러나지 않아야 한다. 값은 BCL·WPF·라이브러리 형식만 쓴다.
   - **계획**: 지금 예배 순서 이름과 그 변경 알림.
   - **송출 화면 상태**: 라이브 상태(꺼짐·대기·송출·검은 화면), 송출 칸 항목(식별자, 제목, 종류: 없음·글·PPT·미디어), PPT면 파일 경로와 지금 장, 지금 화면의 글, 페이지 목록(번호 표시, 글, 지금 페이지 여부), 그리고 이것들의 변경 알림. "라이브가 아니어도 송출 칸을 따라간다"는 지금 규칙을 그대로 표현할 수 있어야 한다(라이브 PPT와 송출 칸 PPT, 송출 칸 그림이 그려졌는지).
   - **넘기기**: 송출 화면의 다음·이전(할 수 있는지와 실행), 페이지로 가기.
   - **고른 항목**: 예배 순서에서 고른 항목(식별자, 미디어 여부, PPT 경로와 장), 변경 알림, 송출 칸에 올리기(할 수 있는지, 진행 중인지, 실행).
   - **PPT 그림**: 송출 칸·미리보기 칸이 가진 작은 장 그림(악보 대신 쓰는 그림), 큰 장 렌더(없을 수 있음), 옛 형식(.ppt·.pps) 읽기(없을 수 있음).
   - **곡**: 가사 고치기에 쓰는 곡 목록 읽기(버전 키 포함: 지금의 "같은 DB 버전은 한 번만 읽고, 못 쓰는 버전은 다시 읽지 않고, 잠긴 경우는 다시 시도" 규칙을 유지할 수 있어야 한다).
   - **카메라**: 로컬 라이브 카메라 내리기, 송출 카메라 켜기/끄기(없을 수 있음).
   - **복구**: 예배 순서 내용의 저장·되돌리기(없을 수 있음. 라이브러리는 그 내용을 해석하지 않는다).
   - **오류 기록**: 지금 `OperationalErrorLog.Write`에 해당하는 것.
3. **라이브 상태**: `CaptionSession`은 라이브러리의 작은 인터페이스(상태, PPT 경로, PPT 장, 변경 알림)와 "네이티브 쇼의 지금 장 읽기"만 받는다. 라이브 상태 종류는 라이브러리의 enum으로 두고, Axicultus 연결부가 `LiveState`와 맞춘다.
4. **Axicultus 연결부**: `Axicultus\Captions\`에 호스트 인터페이스의 `MainViewModel` 구현과 `QueueRecoveryJournal`만 남긴다. 지금 `CaptionConsole` 안에 있는 `MainViewModel` 다루기(고른 항목 올리기의 조건, 곡 읽기, PPT 찾기에 쓰는 송출 칸 값)는 규칙은 라이브러리에, 값 읽기는 연결부에 둔다. 규칙이 연결부로 새어 나가 두 앱이 따로 구현하게 되면 안 된다.
5. **문구**: 자막 문구 resx(`CaptionConsole`, `CaptionSettingsWindow`와 자막 코드가 쓰는 다른 영역의 키)를 라이브러리로 옮긴다. 라이브러리는 `Axicultus.Core`의 `UiTextCatalog`·`UiLanguage`로 한국어·영어를 고른다(앱과 같은 언어 선택을 따른다). 자막 XAML의 `{loc:Text …}` 글은 그대로 두고 `xmlns:loc`만 라이브러리의 확장을 가리키게 한다. 앱의 `UiText`는 그대로 둔다.
6. **테마**: 라이브러리에 1의 키를 모두 담은 기본 사전(밝은·어두운)과 그것을 요소에 넣는 한 가지 방법을 둔다. Axicultus는 그것을 부르지 않는다(앱 테마가 그대로 적용). `Font.Primary`의 기본값은 어셈블리 이름에 기대지 않는 글꼴 목록이다.
7. **자막과 앱이 함께 쓰는 도우미는 아래층에 둔다**(묶음 A 리뷰 뒤 고침, 2026-11-28). 앱의 핵심 기능(가사 표시, 송출·미리보기 창 배치, 앱 테마)이 자막 라이브러리에 기대면 안 된다. 의존은 앱 → 자막 라이브러리 → 아래층 한 방향이다.
   - 가사 줄 읽기(`ReadLines`, `NormalizeText`, `StripInlineNotation`, `IsMarkerOnlyLine`): 글자열만 쓰므로 `Axicultus.Core`에 둔다. 앱의 `LyricsDisplayFormatter`와 라이브러리의 `SongLyrics`가 그것을 부른다.
   - 화면 배율과 창 놓기(`GetDpiScale`, 창 좌표 적용): Win32 호출이므로 `Axicultus.Win32`에 둔다. 앱의 `Platform.*`과 라이브러리의 `CaptionWindowPlacement`가 그것을 부른다.
   - 줄바꿈(`LyricWordWrapping.ForDisplay`): 글자열 처리는 `Axicultus.Core`에 두고, WPF 변환기는 앱과 라이브러리가 각자 얇게 가진다(앱의 `LyricWordWrapping`은 앱에 남고, 라이브러리의 카메라 자막 보기는 자기 변환기를 쓴다).
   - `CameraOwnership`은 자막 탭·설정 창과 함께 묶음 B에서 옮기고, 그 문구 영역(`CameraOwnership`)도 같이 옮긴다.
   - 통째로 옮기기 어려운 것은 인터페이스 뒤로 돌린다(2의 "없을 수 있음").
8. **테스트**: 자막 테스트는 `Axicultus.Tests`에 남고 실제 `MainViewModel` + 연결부로 자막 탭을 만든다. 테스트 본문의 검사 내용은 바꾸지 않는다. 바뀌는 것은 만들기 코드와 `MainViewModel`을 꺼내는 도우미뿐이다. 호스트 인터페이스의 가짜 구현으로 자막 탭을 여는 테스트를 최소 하나 넣는다(a047이 같은 방식으로 붙는다는 증거).

## 3. 묶음

- **묶음 A (엔진)**: 프로젝트 만들기, 호스트·라이브 인터페이스, `MainViewModel`을 모르는 파일 옮기기(`CaptionSession`, `CaptionDeck`, `ScoreLayout`, `SongLyrics`, `LyricsReader`, `CaptionFit`, `ObsConnection`, `ObsCaptureInputs`, `ObsOperator`, `CaptionBroadcastServer`, `CaptionRecoveryStore`, `CaptionConsoleViewModel`, `CameraCaptionView`, `CaptionWindowPlacement`), 문구와 테마 사전, OCR 도우미 찾기.
- **묶음 B (화면)**: `CaptionConsole`·`CaptionTabStrip`·`CaptionSettingsWindow` 옮기기, `MainViewModel` 연결부, 본창·하네스 연결, 가짜 호스트 테스트.

두 묶음은 차례로 한다(같은 파일을 건드린다). 묶음마다 구현 → 적대적 리뷰 → 수정 → `/code-review` → 총괄 검증 → 커밋.

## 4. 위험과 대응

- **동작이 조용히 바뀜**: 옮기면서 규칙을 고치지 않는다. 자막 테스트 전부와 하네스 `captiontab`·`pptcaption`이 옮기기 전과 같은 결과여야 한다(Phase 0에서 기준 기록).
- **문구 누락**: 영역을 옮기다 키가 빠지면 화면에 키 이름이 보인다. 한국어·영어 키 집합이 옮기기 전과 같은지 테스트로 고정한다.
- **XAML 리소스 경로**: `InitializeComponent`의 pack URI와 `Font.Primary`가 어셈블리 이름에 묶여 있다. 라이브러리에서 자막 탭을 여는 테스트와 하네스 그림으로 확인한다.
- **인터페이스가 a047에 모자람**: 묶음 B의 가짜 호스트 테스트가 "MainViewModel 없이 화면 따라가기·단계·송출 스위치가 된다"를 보인다. a047 Phase 0에서 모자란 것이 나오면 이 인터페이스를 고치는 작은 묶음을 a047에 둔다.
- **빌드 순서**: OCR 도우미 복사(`BuildOcrHelper`)는 Axicultus에 그대로 둔다. a047이 Axicultuss에도 같은 복사를 넣는다.

## 4b. 묶음 B가 인터페이스에서 채울 것 (묶음 A 리뷰, 2026-11-28)

묶음 A의 호스트 인터페이스는 초안이고 아직 쓰는 곳이 없다. 묶음 B는 아래를 모두 라이브러리 형식으로 표현해, 규칙은 라이브러리에 두고 연결부는 값만 읽게 한다.

- **송출 칸과 송출 항목의 구분**: 항목 자신의 장, 송출 칸이 지금 그리고 있는 PPT의 경로와 장("칸이 아직 앞 PPT를 들고 있다" 규칙, "라이브가 아닐 때 칸을 넘기면 자막이 따라간다" 규칙). 칸이 따로 없는 호스트(Axicultuss)는 그 값을 주지 않을 수 있어야 한다.
- **고른 PPT의 장**: 미리보기 칸의 장을 쓴다는 것을 형식에서 분명히 한다.
- **고른 항목의 동일성**: 기다린 뒤 "아직 같은 항목인가"를 볼 수 있게, 고른 항목이 바뀌기 전까지 같은 객체를 준다는 약속(라이브 스냅샷과 같은 방식)을 문서와 테스트로 고정한다. 값을 읽어 만드는 호스트는 비교 값이 같으면 같은 객체를 돌려준다.
- **올리기 진행 기다리기**: 진행 중인 올리기가 끝날 때까지 기다릴 수 있어야 한다(지금은 실행 작업을 기다린다).
- **렌더**: 실패 이유(화면에 보이는 글), 인코딩된 바이트와 형식(가사 인식에 파일로 쓴다).
- **복구**: 고른 행 번호의 되돌리기, 항목 유효성 검사(망가진 항목이면 `.bak`으로 돌아가는 규칙). 복구 파일의 위치와 JSON 모양은 지금과 같아야 한다. `QueueRecoveryJournal`(앱에 남음)이 쓰는 저장 호출도 그대로 동작해야 한다.
- **페이지**: 페이지 번호(0부터), 위치 글이 비었을 때의 "1/1" 규칙은 라이브러리에.
- **카메라**: 자막 탭과 설정 창이 묶는 카메라 상태(미리보기 그림, 상태 글, 위아래 뒤집기, OBS가 카메라를 쓰는지, 로컬 카메라 내리기 알림). 카메라가 없는 호스트에서도 자막 탭이 열려야 한다.
- **곡**: "폴더 번호가 0보다 큰 곡만" 같은 데이터 규칙을 호스트마다 되풀이하지 않게 한다.
- **계약**: 알림이 오는 스레드(UI 스레드), 호스트의 송출 상태와 `ICaptionLiveSource` 스냅샷의 관계.
- **기본 컨트롤 모양**: 자막 XAML이 앱의 암시적 스타일(Button, CheckBox, ComboBox, TextBox)에 기대는 곳이 있다. 앱 테마가 없는 호스트에서 어떻게 보이는지 그림으로 확인하고, 쓸 수 없는 모양이면 라이브러리 기본 사전에 최소한의 모양을 넣는다.
- **하네스**: `Windows()` 단계가 앱 어셈블리만 훑는다. 설정 창이 라이브러리로 가도 그 단계에 남게 한다.
- **문구 검사**: a047 뒤에는 `UiTextCoverageTests`가 Axicultuss에서 쓰는 라이브러리 문구도 본다.

## 5. 확정된 인터페이스 (구현 뒤 기록)

묶음 B가 끝나면 구현자가 호스트 인터페이스의 이름과 멤버 목록을 여기에 적는다. a047의 연결부는 이 절을 기준으로 한다.
