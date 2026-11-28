## Phase 0. 기준과 영향

- Goal: 옮기기 전 결과를 기록해, 옮긴 뒤 같은지 비교할 수 있게 한다.
- [x] 0.1 브랜치 `feat/a046-caption-shared-library`(지금 HEAD에서). Debug 빌드, `Axicultus.Tests` 전체와 `FullyQualifiedName~Caption` 결과(통과·건너뜀 수) 기록
- [x] 0.2 하네스 `captiontab`, `pptcaption` 기준 로그(격리 사본, 실행 규칙은 `tools\AxicultusHarness\README.md`). 실행 전 사용자에게 창이 뜬다고 알린다
- [x] 0.3 design 1의 의존 목록을 지금 코드와 대조(줄 번호 갱신, 빠진 것 추가)
- DoD: 기준 수치와 로그 이름이 이 문서 "검증 결과"에 있다.

## Phase 1. 묶음 A — 엔진

- Goal: `MainViewModel`을 모르는 자막 코드를 `Axicultus.Captions`로 옮긴다. Axicultus 동작은 그대로.
- Scope: design 3 묶음 A. `CaptionConsole`·`CaptionTabStrip`·`CaptionSettingsWindow`·`QueueRecoveryJournal`은 아직 앱에 둔다.
- [x] 1.1 프로젝트와 참조, `Axicultus.sln` 등록, `InternalsVisibleTo`
- [x] 1.2 호스트·라이브 인터페이스 초안(design 2.2, 2.3)과 라이브 상태 enum
- [x] 1.3 엔진 파일 옮기기, 앱 형식 의존을 인터페이스·라이브러리 형식으로 바꾸기 (`CaptionRecoveryStore`는 제외: 검증 결과 참조)
- [x] 1.4 자막 문구 resx와 표시 확장, 테마 기본 사전
- [x] 1.5 앱 쪽 수정(`using`·xmlns·서비스 등록), 테스트 만들기 코드 수정
- Tests: 한국어·영어 키 집합이 옮기기 전과 같음, 테마 사전이 design 1의 키를 모두 가짐(밝은·어두운), 자막 테스트 전부
- DoD: Debug·Release 빌드 오류 0, `Axicultus.Tests` 전체가 0.1과 같은 통과 수(새 테스트만큼 증가), 라이브러리가 Axicultus 앱 어셈블리를 참조하지 않음
- Constraints: 식별자·주석 영어. 규칙을 고치지 않는다. 자막과 무관한 코드를 정리하지 않는다. 사용자의 로컬 `*.csproj` 변경이 있으면 커밋에 넣지 않는다.

## Phase 2. 묶음 B — 화면

- Goal: 자막 탭 화면을 라이브러리로 옮기고 Axicultus는 연결부로 붙인다.
- [x] 2.1 `CaptionConsole`(두 partial)·`CaptionTabStrip`·`CaptionSettingsWindow` 옮기기, `MainViewModel` 사용을 호스트 인터페이스로 바꾸기 (`CaptionRecoveryStore`, `CameraOwnership`과 그 문구 영역도 옮김)
- [x] 2.2 Axicultus 연결부(`MainViewModel` 구현), `QueueRecoveryJournal`은 앱에 남김, 본창·서비스 등록 연결
- [x] 2.3 테스트 만들기 코드(`WithConsole`)와 하네스(`tools\AxicultusHarness`) 수정
- [x] 2.4 가짜 호스트로 자막 탭을 여는 테스트: 화면 글 따라가기, 페이지 단계와 페이지로 가기, 고른 항목 올리기(송출 중에는 안 함), 송출 스위치
- [x] 2.5 design 5에 확정된 인터페이스 기록
- Tests: 자막 테스트 전부(검사 내용 변경 없음), 2.4, 하네스 `captiontab`·`pptcaption`
- DoD: Debug·Release 빌드 오류 0(저장소 bin), 전체 테스트 통과, 하네스 두 단계가 0.2와 같은 결과, 실제 데이터 unchanged, `Axicultus\Captions\`에 연결부와 `QueueRecoveryJournal`만 남음
- Constraints: Phase 1과 같다. 자막 탭의 모양·키·저장 파일은 바뀌지 않는다.

## Phase 3. 회귀와 빌드

- [x] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [ ] 3.2 하네스 그림 (묶음 B 구현자: 새 그림은 만들었으나 같은 이름의 이전 그림을 따로 보관하지 않아 덮어씀. 비교는 못 함, 검증 결과 참조)(`captiontab-*.png`)을 0.2의 그림과 비교(탭 줄, 자막 탭, 라이브 탭)

## Phase 4. 리뷰와 기록

- [x] 4.1 묶음마다 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [x] 4.2 묶음마다 `/code-review`
- [x] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [x] 4.4 커밋·푸시, 아카이브(`openspec/changes/archive/<날짜>-a046-caption-shared-library`, `openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 옮기다 찾은 기존 결함 (이 변경에서 고치지 않음, 별도 fix 변경으로)

`/code-review`(묶음 A, 2026-11-28)가 옮긴 코드에서 찾았다. 옮기기 전부터 있던 동작이라 이 변경(동작 그대로)에서는 고치지 않고, a046이 끝난 뒤 작은 fix 변경으로 처리한다.

- `LyricsReader.ReadAsync`: 모든 장 그림을 OCR 도우미 한 번에 넘기고 전체에 2분 제한을 둔다. 장이 아주 많은 PPT(명령줄 길이 초과)나 2분을 넘는 PPT는 가사 인식 결과를 전부 잃는다.
- `CaptionStyle.Parse`: `caption-style.json`의 `Color`가 null이면 예외가 나서 자막 세션을 만들지 못한다(기본값으로 돌아가야 한다).
- `ObsOperator.StartAsync`: `obs.json`에 `Secret`이 없으면 `_savedSecret`이 null이 되어 이후 `HasSavedPassword`에서 예외가 난다.

## 검증 결과

### Phase 0 (2026-11-28, 브랜치 `feat/a046-caption-shared-library`, 문서 커밋 b8d2d42 위)

- 0.1 기준: `dotnet build Axicultus.Tests\Axicultus.Tests.csproj -c Debug` 오류 0. `dotnet test Axicultus.Tests\Axicultus.Tests.csproj -c Debug --no-build` 통과 3,732·건너뜀 22·전체 3,754. `--filter "FullyQualifiedName~Caption"` 통과 317·건너뜀 1·전체 318. `Axicultuss.Tests` 통과 478·건너뜀 2·전체 480.
- 0.2: 이 묶음에서 하지 않음(하네스 기준 로그는 이미 있음).
- 0.3: design 1에 대조 결과를 보탰다(파일별 의존, `ReferenceEquals` 규칙, 문구 영역 키 수, 테마 키의 코드 사용, OCR 도우미 경로, 테스트의 만들기 곳).

### Phase 1 묶음 A (구현 직후, 리뷰 전)

- 빌드: `Axicultus\Axicultus.csproj` Debug·Release 오류 0. `Axicultuss\Axicultuss.csproj` Debug 오류 0. 하네스 `tools\AxicultusHarness\Harness.csproj` Debug 빌드 오류 0(실행하지 않음).
- 테스트: `Axicultus.Tests` 통과 3,739·건너뜀 22·전체 3,761(기준 + 새 테스트 7, 실패 0). `FullyQualifiedName~Caption` 통과 324·건너뜀 1(기준 317 + 새 `CaptionLibraryTests` 7). `Axicultuss.Tests` 통과 478·건너뜀 2(기준과 같음).
- 새 테스트(`Axicultus.Tests\Shell\CaptionLibraryTests.cs`): 라이브러리가 `Axicultus`·`Axicultuss`를 참조하지 않음, 문구 키 191개가 옮기기 전(`Localization\CaptionTextKeys.txt`)과 영어·한국어 모두 같고 실행 중에 찾아짐, 기본 테마(밝은·어두운)가 design 1의 키 24개를 모두 가짐과 자막 XAML·코드가 쓰는 키가 모두 그 안에 있음, 테마 적용·교체, 라이브 연결부의 스냅샷 동일성·알림·해제, `LiveState` 대응.
- 리뷰 뒤 고침(2026-11-28):
  - 가사 줄 읽기(`ReadLines`·`NormalizeText`·`StripInlineNotation`·`IsMarkerOnlyLine`)를 `Axicultus.Core\LyricLines.cs`로 옮김. 앱의 `LyricsDisplayFormatter`와 라이브러리의 `SongLyrics`가 Core를 부름. 라이브러리의 `LyricLines.cs`는 없앰.
  - 화면 배율·창 놓기를 `Axicultus.Win32\DesktopPixels.cs`로 옮김(`Graphics` 때문에 `FrameworkReference Microsoft.WindowsDesktop.App.WindowsForms` 추가). 앱의 `DisplayService`·`DesktopWindowPlacement`와 라이브러리의 `CaptionWindowPlacement`가 그것을 부름.
  - 줄바꿈 글자열 처리를 `Axicultus.Core\LyricWordBreaks.cs`로 옮김. 앱의 `LyricWordWrapping`은 앱에 되돌림, 라이브러리는 `CaptionWordWrapping` 변환기. `EasiDS.xaml` xmlns 되돌림.
  - `UiTextCoverageTests`: `UiText`는 앱 문구, `CaptionText`는 라이브러리 문구, XAML `{loc:Text}`는 `xmlns:loc`가 가리키는 어셈블리의 문구와 대조. 잘못된 카탈로그 사용을 일부러 넣어 실패를 확인한 뒤 되돌림.
  - 라이브 연결부: 옛 스냅샷을 실은 알림이 와도 `Current`의 객체가 그대로, `Changed += null`은 구독하지 않음. 테스트 2개 추가.
  - 테스트: 한국어 키 확인은 영어로 대체되지 않음(`tryParents: false`), 기본 테마 값이 앱 토큰 값과 같음(`Font.Primary` 제외), `UiLanguageFlowTests`는 앱 문구(`SettingsWindow.Title`)와 자막 문구를 둘 다 봄.
  - 수치: `Axicultus` Debug·Release, `Axicultuss` Debug, 하네스, `Axicultus.sln` Debug 빌드 오류 0. `Axicultus.Tests` 통과 3,741·건너뜀 22·전체 3,763(기준 + 9). `FullyQualifiedName~Caption` 통과 326·건너뜀 1. `Axicultuss.Tests` 통과 478·건너뜀 2.
- 코드 리뷰 뒤 고침: `Axicultus.Core\LyricWordBreaks.cs`의 보이지 않는 글자와 한글 범위를 원래의 `\uXXXX` 표기로 되돌림(본문이 HEAD의 `LyricWordWrapping.ForDisplay`와 같음). 문구 키 테스트는 "옮기기 전 키가 모두 있음(영어·한국어)과 영어·한국어 키 집합이 같음"으로 바꿈(고정 개수 없앰). 수치: `Axicultus.sln` Debug·`Axicultus` Release 빌드 오류 0, `Axicultus.Tests` 통과 3,741·건너뜀 22, `Axicultuss.Tests` 통과 478·건너뜀 2.
- 남긴 것: `CaptionRecoveryStore`는 앱에 남겼다. 복구 파일의 `Items`가 앱 형식 `WorshipListItemDto[]`이고 읽을 때 그 필드를 검사하므로, 라이브러리로 옮기려면 항목 표현(호스트 인터페이스의 "복구")을 먼저 정해야 한다. 쓰는 곳(`CaptionConsole`, `QueueRecoveryJournal`)도 묶음 B에서 옮긴다.

### 묶음 A 총괄 검증 (2026-11-28)

- 적대적 리뷰(별도 에이전트): 치명 0, 주요 2(문구 검사가 어셈블리를 구분하지 않음, 호스트 인터페이스 초안의 빈 곳), 사소 6. 문구 검사와 사소한 것은 이 묶음에서 고쳤고, 인터페이스의 빈 곳은 design 4b로 옮겨 묶음 B가 채운다. 리뷰 뒤 design 2.7을 고쳐 함께 쓰는 도우미를 Axicultus.Core·Axicultus.Win32로 내렸다.
- /code-review: 옮기기에서 회귀 없음. 지금 고친 것 2(보이지 않는 문자 원문, 너무 엄격한 키 검사), 기존 결함 3은 위 목록으로.
- 총괄이 직접 돌린 결과: `dotnet build Axicultus.sln -c Debug` 오류 0, `Axicultus.csproj -c Release` 오류 0, `Axicultus.Tests` 3741 통과·22 건너뜀·실패 0, `Axicultuss.Tests` 478 통과·2 건너뜀. 라이브러리 참조는 Axicultus.Core·Axicultus.Win32뿐, Theme\EasiDS.xaml 변경 없음, 앱에서 라이브러리를 쓰는 파일은 원래 자막 형식을 쓰던 곳뿐.
- 하지 않은 것: 앱 실행 점검(하네스)은 묶음 B 끝에서 한다.

### Phase 2·3 묶음 B (구현 직후, 리뷰 전, 2026-11-28)

- 옮김(네임스페이스 그대로): `CaptionConsole`(xaml과 두 partial), `CaptionTabStrip`, `CaptionSettingsWindow`, `CaptionRecoveryStore`, `CameraOwnership`(`Axicultus.Wpf.Media`)과 문구 영역 `CameraOwnership`. `Axicultus\Captions\`에는 `MainViewModelCaptionHost`(연결부), `LiveSessionCaptions`(묶음 A의 라이브 연결부), `QueueRecoveryJournal`만 남음. 인터페이스는 design 5.
- 복구 파일: 위치(프로필 `CaptionRecovery\`, 이름은 예배 순서 이름의 SHA-256)와 JSON 모양 그대로. `CaptionRecovery.Items`는 `JsonElement[]`, Axicultus는 항목을 예전처럼 `WorshipListItemDto`로 쓰고 읽으며 `IsRecoverableItem`으로 망가진 항목을 가린다(`.bak`으로). 684e93d의 저장 코드로 만든 파일을 읽고 같은 모양으로 다시 쓰는 테스트를 넣음.
- 새 문구 1개: `CaptionConsole.LegacyDeckUnsupported`(영어·한국어, 옛 형식 읽기가 없는 호스트에서만 보임).
- 기본 모양: 앱 테마가 없는 호스트에서 어두운 모양의 설정 창 체크 상자 글자가 검게 나와 읽을 수 없었다(시스템 컨트롤 모양). `CaptionTheme.Apply(CaptionConsole, dark)`가 설정 창의 체크 상자 글자를 기본 모양의 글자색으로 한다(Axicultus는 이것을 부르지 않으므로 모양 변화 없음). 그 밖의 시스템 모양 단추·입력 칸·목록은 밝은·어두운 모두 읽을 수 있어 그대로 둠.
- 빌드: `dotnet build Axicultus.sln -c Debug`, `Axicultus\Axicultus.csproj -c Release`, `-c Debug`, `Axicultuss\Axicultuss.csproj -c Debug`, 하네스 `Harness.csproj` 모두 오류 0.
- 테스트: `Axicultus.Tests` 통과 3,749·건너뜀 22·실패 0(3,741 + 새 8: 가짜 호스트 4, 연결부 `Picked` 동일성 1, 옛 모양 복구 파일 1, 기본 모양 밝은·어두운 2). 바꾸기 전 `FullyQualifiedName~Caption` 통과 326·건너뜀 1, 바꾼 뒤 같은 필터에서 기존 326 모두 통과. `Axicultuss.Tests` 통과 478·건너뜀 2.
- 기존 테스트 수정: 만들기 코드(`WithConsole`, `Main(console)`, 테스트 본문에서 `_main` 필드를 꺼내던 4곳을 `Main(console)`로)만. 예외: `CaptionIntegrationTests`의 복구 저장소 테스트 3개는 항목이 `JsonElement`가 되어 항목 만들기와 항목 비교 두 곳의 식이 바뀜(같은 값 비교: `Items[1].Sequence` → `Items[1].Deserialize<WorshipListItemDto>().Sequence`, 항목 동등 → 역직렬화한 DTO 동등), 저장소는 앱의 항목 검사로 만듦.
- 기본 모양 그림(`AXI_A037_SHOTS`, `DrawTheConsoleForReview` 한 번 실행): `11-default-look-light.png`, `11-default-look-dark.png`, `12-default-look-settings-light.png`, `12-default-look-settings-dark.png`. 그리는 동안 Application 리소스를 비워 앱 테마·컨트롤 모양이 섞이지 않게 함. `CaptionDefaultLookTests`를 따로 돌리면 프로세스에 Application이 없음(테스트 출력으로 확인).
- 하네스 `Run-Harness.ps1 -Phases 'captiontab,pptcaption' -Name 'a046-b'`: 보호 검사 통과, `[CAPTIONTAB]` PASS 16·FAIL 0이고 줄 전체(시각 제외)가 `previewkeep.log`(d4e2b97)와 같음. `[PPT]` 69줄 중 68줄이 `livetab.log`(67a195a)와 같고 한 줄 다름: `after GoLive: console=` 기준은 "", 이번은 "그림 속 가사를 자막으로 씁니다…". 기준 실행에만 그 사이 직접 입력 글이 송출된 `[AIR]` 4건(rev 14~17, "나의" → "나의 전부이신 주여 오셔서")이 있고, 같은 날 03:32 `tabstart-pptcaption.log`, `a045-pptcaption*.log`, a035~a040 실행이 모두 이번과 같은 문구다(한국어 실행은 같은 키의 한국어 글). 실제 데이터 unchanged(3곳). `FIRSTCHANCE` 3건은 기준과 같음(OfficePptSession). 끝에 `POWERPNT before=0 after=1`(끝난 뒤 곧 사라짐, 기준 실행의 값은 로그에 없음).
- 하지 않은 것: 3.2(그림 비교) — 하네스가 같은 이름으로 그림을 덮어써 0.2의 그림이 남아 있지 않았다.

### 묶음 B 리뷰 뒤 고침 (2026-11-28)

- M1: `CaptionDefaultLookTests`가 `WPF Application` 모음과 함께 돌며 공유 리소스 사전을 건드렸다(리뷰어 5회 중 5회 실패). 새 모음 `Caption default look`(`DisableParallelization = true`)에 넣어 다른 테스트가 모두 끝난 뒤 혼자 돌게 했고, 테스트 동안 Application 리소스를 빈 사전으로 바꿨다가 되돌린다(앱 테마·컨트롤 모양이 없음을 테스트 안에서 단언). 가짜 호스트의 다른 테스트는 `CaptionConsoleTests`(이미 `WPF Application` 모음)에 있다. 리뷰어의 필터(`CaptionDefaultLookTests|AnotherHost|AHostWithNone|ARecoveryFileOfTheShape|TheAppsPickedRow`)를 5회 연속 실행: 5회 모두 통과 11·실패 0.
- M2: 가짜 호스트 테스트가 패널 없는 호스트의 길을 지나게 함. `AHostWithNoneOfTheOptionalPartsHasAWorkingTab`에서 `ScreenDeck` 대역을 없앰: `OutputPanel` 없이 PPT 항목 `PptSlide` 2 → 자막 2장, `Output`으로 3 → 3장(라이브 아님, `OutputPanel()` 대체), 라이브 Active에서 라이브 쪽 장 1 → 1, 2 → 2, 라이브 중 송출 칸 3은 따라가지 않음. 새 테스트 3개: 미리보기 칸 없이 고른 PPT 다시 읽기(1장으로), 올리기 진행 중의 두 번 고르기(끝난 뒤 마지막 행만 올림), 렌더 없음(글로 나가고 "악보 만드는 중")·곡 DB 없음(인식한 가사 그대로, 오류 기록 없음). 자막 탭의 결함은 나오지 않음(코드 수정 없음).
- 문서: `CaptionItem.PptSlide`(패널 없는 호스트에서는 지금 보이는 장, 넘길 때마다 `Output`), 한 번만 읽는 멤버(`Live`, `Recovery`, `Renderer`, `Camera`, `ToggleOutputCamera`가 null인지), 예배 순서가 바뀐 뒤 호스트가 새 탭에 다시 할 일(design 5).
- 수치: `Axicultus.sln -c Debug`, `Axicultus\Axicultus.csproj -c Release` 빌드 오류 0. `Axicultus.Tests` 두 번: 통과 3,752·건너뜀 22·실패 0(두 번 같음, 3,741 + 새 11). `Axicultuss.Tests` 통과 478·건너뜀 2.

### 묶음 B 코드 리뷰 뒤 고침 (2026-11-28)

- 라이브 연결부(`LiveSessionCaptions.LiveSource`): 캐시한 (스냅샷, 보기) 짝을 불변 객체 하나로 두고 `Volatile.Read`·`Interlocked.CompareExchange`로 통째로 바꿈(다른 스레드의 알림과 읽기가 섞여도 보기가 다른 스냅샷과 짝지어지지 않음). 테스트: 두 스레드가 `Current`를 읽는 동안 다른 스레드가 송출을 바꾸고 알림(늦은 알림 포함)을 올려도 돌려받은 보기가 늘 한 스냅샷의 온전한 보기이고 읽기 전 송출보다 오래되지 않음.
- 복구 되돌리기: 호스트의 `Restore`가 던지면 탭이 잡아 `WriteError`로 기록하고 상태 줄에 알리며(새 문구 `CaptionConsole.RecoverFailed`, 영어·한국어) 초안과 즉시 보내기를 그대로 두고 복구를 그만둔다. 설정 창의 복구 단추 처리기(`async void`)도 그 밖의 실패를 잡아 같은 문구로 알린다. 테스트: `Restore`가 던지는 가짜 호스트.
- 복구 행 직렬화: `MainViewModelCaptionHost.ToRecovery`가 행 객체마다 직렬화한 JSON을 `ConditionalWeakTable`에 보관(바뀌지 않은 행은 한 번만 직렬화, 쓰는 바이트는 같음, 옛 모양 복구 파일 테스트 통과). 테스트: 같은 순서를 두 번 캡처해도 행마다 보관한 상자가 같고, 바뀐 행은 새로 직렬화.
- 수치: `Axicultus.sln -c Debug`, `Axicultus\Axicultus.csproj -c Release` 빌드 오류 0. `Axicultus.Tests` 통과 3,755·건너뜀 22·실패 0(3,752 + 새 3). `Axicultuss.Tests` 통과 478·건너뜀 2.

### 묶음 B 총괄 검증 (2026-11-28)

- 적대적 리뷰(별도 에이전트): 치명 0. 옮긴 화면 코드는 옛 코드와 줄 단위로 대조해 동작 차이 없음, 복구 파일은 옛 코드가 쓴 것과 바이트까지 같음. 주요 2건은 새 테스트의 문제(다른 WPF 테스트와 동시에 돌면 실패, 가짜 호스트 테스트가 송출 칸 없는 호스트의 경로를 건너뜀)라 고쳤다. a047에 남긴 것은 a047 design 2b.
- `/code-review`: 회귀 없음. 고친 것 3(라이브 연결부의 엇갈린 읽기, 복구 되돌리기 실패가 앱을 멈출 수 있음, 복구 저장 때 UI 스레드에서 예배 순서 전체를 다시 변환). 기존 결함 1(복구가 코드와 카포를 잃음)은 a048로.
- 총괄이 직접 돌린 결과: `dotnet build Axicultus.sln -c Debug` 오류 0, `Axicultus.csproj -c Release` 오류 0, `Axicultus.Tests` 3755 통과·22 건너뜀·실패 0, `Axicultuss.Tests` 478 통과·2 건너뜀. 라이브러리에 앱 형식 없음(검색 0건), `Axicultus\Captions\`에는 연결부 둘과 `QueueRecoveryJournal`만.
- 하네스(총괄 실행, 로그 `a046-b-verify.log`, 격리 사본): `captiontab` 16 PASS·0 FAIL, `pptcaption` 69줄이 구현자 실행(`a046-b.log`)과 모두 같음, 처리되지 않은 예외 0, 실제 데이터 unchanged. 0.2의 기준은 `previewkeep.log`(captiontab)와 `tabstart-pptcaption.log`·`a045-pptcaption-watch.log`(pptcaption)이다.
- 그림: 앱 테마 없는 기본 모양(밝은·어두운, 자막 탭과 설정 창)을 총괄이 봤다. 밝은 쪽은 문제없고, 어두운 쪽 설정 창은 입력 칸·버튼이 시스템 기본 모양(밝음)으로 남지만 읽고 쓸 수 있다. 본창 그림(`captiontab-pick.png`)은 옮기기 전과 같은 모양이다. 3.2의 옮기기 전후 그림 맞비교는 기준 그림을 남기지 않아 하지 못했다.
