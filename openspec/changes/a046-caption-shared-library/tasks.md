## Phase 0. 기준과 영향

- Goal: 옮기기 전 결과를 기록해, 옮긴 뒤 같은지 비교할 수 있게 한다.
- [x] 0.1 브랜치 `feat/a046-caption-shared-library`(지금 HEAD에서). Debug 빌드, `Axicultus.Tests` 전체와 `FullyQualifiedName~Caption` 결과(통과·건너뜀 수) 기록
- [ ] 0.2 하네스 `captiontab`, `pptcaption` 기준 로그(격리 사본, 실행 규칙은 `tools\AxicultusHarness\README.md`). 실행 전 사용자에게 창이 뜬다고 알린다
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
- [ ] 2.1 `CaptionConsole`(두 partial)·`CaptionTabStrip`·`CaptionSettingsWindow` 옮기기, `MainViewModel` 사용을 호스트 인터페이스로 바꾸기
- [ ] 2.2 Axicultus 연결부(`MainViewModel` 구현), `QueueRecoveryJournal`은 앱에 남김, 본창·서비스 등록 연결
- [ ] 2.3 테스트 만들기 코드(`WithConsole`)와 하네스(`tools\AxicultusHarness`) 수정
- [ ] 2.4 가짜 호스트로 자막 탭을 여는 테스트: 화면 글 따라가기, 페이지 단계와 페이지로 가기, 고른 항목 올리기(송출 중에는 안 함), 송출 스위치
- [ ] 2.5 design 5에 확정된 인터페이스 기록
- Tests: 자막 테스트 전부(검사 내용 변경 없음), 2.4, 하네스 `captiontab`·`pptcaption`
- DoD: Debug·Release 빌드 오류 0(저장소 bin), 전체 테스트 통과, 하네스 두 단계가 0.2와 같은 결과, 실제 데이터 unchanged, `Axicultus\Captions\`에 연결부와 `QueueRecoveryJournal`만 남음
- Constraints: Phase 1과 같다. 자막 탭의 모양·키·저장 파일은 바뀌지 않는다.

## Phase 3. 회귀와 빌드

- [ ] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [ ] 3.2 하네스 그림(`captiontab-*.png`)을 0.2의 그림과 비교(탭 줄, 자막 탭, 라이브 탭)

## Phase 4. 리뷰와 기록

- [ ] 4.1 묶음마다 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 묶음마다 `/code-review`
- [ ] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/changes/archive/<날짜>-a046-caption-shared-library`, `openspec/specs/live-camera-captions/spec.md`에 delta 반영)

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
