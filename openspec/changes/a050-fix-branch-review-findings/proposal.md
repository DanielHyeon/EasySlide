# 브랜치 전체 코드 리뷰가 찾은 결함 고치기

## Why

a047 Phase 0의 `/code-review`(2026-11-28)가 작업 트리 대신 브랜치 전체(`origin/main` 대비 약 900개 파일)를 읽고, 이번 묶음과 무관한 기존 결함을 찾았다. 리뷰는 코드를 읽기만 했고 실행해 확인하지 않았다. 작업 중 찾은 결함은 기록만 하지 않고 고친다는 사용자 규칙에 따라 별도 변경으로 처리한다. 각 항목은 먼저 실패하는 테스트로 재현하고, 재현되지 않는 항목은 근거를 적고 뺀다.

자막(두 앱 공용 라이브러리, 예배 중에 쓰는 경로):

1. **OBS 설정을 읽지 못한 채 자막 탭을 처음 보면 저장된 OBS 설정이 기본값으로 덮어써진다.** `ObsOperator.StartAsync`가 `obs.json`을 읽다 실패하면(파일이 잠겨 있거나 JSON이 아님) 안내만 하고 값은 기본값으로 남는다. 그 뒤 `StartWhenUsed`가 연결을 시도하면서 `ConfigureAsync`가 그 기본값을 파일에 쓴다. 저장된 주소·비밀번호·씬 단축키가 사라진다. a048은 "항목 하나가 틀린 파일"과 "해독되지 않는 비밀번호"만 고쳤다.
2. **자막 탭에서 장을 넘기다 호스트가 예외를 던지면 앱이 끝난다.** `CaptionConsole`의 `Step`이 `async void`이고 `_host.StepOutputAsync`를 잡지 않고 기다린다. PowerPoint가 응답하지 않을 때의 `TimeoutException` 같은 것이 그대로 디스패처로 나간다.
3. **라이브 상태 알림이 다른 스레드에서 오면 자막 세션이 UI 스레드 밖에서 바뀐다.** 호스트 계약은 `ICaptionLiveSource.Changed`만은 다른 스레드에서 올 수 있다고 적는데, `CaptionSession.OnLiveChanged`는 받은 스레드에서 상태를 바꾸고 알린다.
4. **슬라이드의 글상자 목록이 같은 상자를 두 번 내놓거나 숨긴 상자를 내놓을 수 있다.** `CaptionDeck`의 상자 목록은 `mc:Fallback` 아래와 숨긴 도형을 거르지 않는다(같은 파일의 전체 글 읽기는 거른다).

곡과 예배 순서:

5. **탭이나 줄바꿈으로 나눈 곡 순서 글이 절 코드로 잘못 읽힌다.** `SongExchangeSequence.IsVerseCodes`가 32 미만 글자가 하나라도 있으면 코드로 보는데, 같은 파일의 글 읽기는 탭·CR·LF를 구분자로 받는다.
6. **모르는 코드가 섞인 곡 순서가 쉼표가 겹친 글로 보인다**(`1,,2`). `gfLyrics.cs`의 순서 글 만들기가 모르는 코드의 글자는 건너뛰면서 쉼표는 붙인다.
7. **휴지통에서 예배 순서를 되돌리다 한쪽 폴더에서 실패하면 반만 되돌려진다.** `WorshipListStore.RestoreFromTrash`가 두 폴더의 파일을 차례로 옮기고 되돌리기가 없다. 다시 시도하면 이름이 겹쳐 사본이 하나 더 생긴다.

규칙:

8. **추가된 코드 주석에 한글이 있다**(`MainViewModel.cs`, `CaptionSession.cs`, `OfficePptSession.cs`, `LyricsDisplayFormatter.cs` 등). 저장소 규칙은 식별자와 주석을 영어로 쓴다.

사용자 흐름: 예배 중 자막 운영(OBS 연결, 장 넘기기), 곡 편집·가져오기, 예배 순서 휴지통.

## What Changes

- OBS 설정을 읽지 못했으면, 운영자가 직접 연결을 누르기 전에는 설정 파일을 쓰지 않는다.
- 자막 탭의 장 넘기기 실패는 자막 탭에 안내하고 오류 기록에 남긴다. 앱은 계속 돈다.
- 라이브 상태 알림은 어느 스레드에서 와도 자막 세션의 UI 스레드에서 처리된다.
- 글상자 목록은 같은 상자를 한 번만 내놓는다(숨긴 상자는 지금처럼 둔다: design 11).
- 탭·줄바꿈이 섞인 곡 순서 글은 가져올 때 쉼표로 고쳐 저장한다. 저장된 값을 읽는 규칙은 바꾸지 않는다(design 10).
- 모르는 코드는 순서 글에서 쉼표째 빠진다.
- 휴지통 되돌리기는 모두 되돌리거나 아무것도 바꾸지 않는다.
- 주석의 한글을 영어로 바꾼다(동작 변경 없음).

## Non-goals

- `ImageCanvas`가 폭이 바뀔 때 `OnPaint` 안에서 썸네일을 다시 읽는 것(같은 리뷰의 지적). 결함이 아니라 성능 문제이고 PPT 미리보기 성능 개선 백로그에 속한다. 여기서는 고치지 않고 design에 근거를 남긴다.
- 위 항목 밖의 동작 변경, 리팩터링.

## Impact

- 영향: `Axicultus.Captions`(`ObsOperator.cs`, `CaptionConsole.xaml.cs`, `CaptionSession.cs`, `CaptionDeck.cs`), `Axicultus.Core\SongExchangeSequence.cs`, `Axicultuss\Global\gfLyrics.cs`, `Axicultus\Shell\WorshipListStore.cs`, 주석만 바뀌는 파일들, 테스트.
- 자막 항목은 두 앱 공통이다. 5·6은 두 앱의 곡 순서, 7은 Axicultus의 예배 순서 휴지통이다.
- 라이브 송출, Office 연동, DB 스키마, 모니터 배치는 바뀌지 않는다.

## Capabilities

### Modified Capabilities

- `live-camera-captions`: OBS 설정을 읽지 못했을 때의 보존, 장 넘기기 실패 처리, 글상자 목록.
