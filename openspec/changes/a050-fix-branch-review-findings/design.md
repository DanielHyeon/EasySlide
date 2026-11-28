# Design — a050 브랜치 전체 코드 리뷰가 찾은 결함

## 현재 구조 (영향 근거)

CodeGraph CLI는 이 세션에서 쓸 수 없었다. 아래는 `/code-review`(2026-11-28)가 코드를 읽어 낸 결과이고 실행으로 확인된 것은 없다. 구현자는 Phase 0에서 항목마다 줄 번호, 부르는 곳, 이미 잡는 예외를 확인한다.

- `CaptionConsole.xaml.cs` 1238줄 근처 `StartWhenUsed`: `obs.json`이 있으면 `ConnectAsync` → `ObsOperator.ConfigureAsync`(설정 창의 값으로 `obs.json`을 다시 씀). `ObsOperator.StartAsync`는 읽기 실패를 잡아 `SettingsReadFailed`만 알린다.
- `CaptionConsole.xaml.cs` 1412줄 근처 `Step`: `async void`, `await _host.StepOutputAsync(direction)`에 try/catch 없음. Axicultus 호스트의 다음·이전 명령이 예외를 스스로 잡는지는 확인되지 않았다.
- `CaptionSession.cs` 369줄 근처 `OnLiveChanged`: `_visible`·`_text`·`Published`를 바꾸고 `PropertyChanged`·`PublishedChanged`를 부른 스레드에서 올린다. `CaptionHost.cs`의 계약 글은 `ICaptionLiveSource.Changed`가 다른 스레드에서 올 수 있다고 한다. Axicultus의 `LiveSessionCaptions`가 실제로 어느 스레드에서 올리는지는 구현자가 확인한다.
- `CaptionDeck.cs` 51줄 근처: 상자 목록은 모든 `p:sp` 자손, 같은 파일의 `AllText`는 `mc:Fallback` 아래와 숨긴 도형·묶음을 뺀다.
- `Axicultus.Core\SongExchangeSequence.cs` 71줄 근처 `IsVerseCodes`, 같은 파일의 `ToLegacy`·`IsTokenText`·`ToDatabase`.
- `Axicultuss\Global\gfLyrics.cs` 3372줄 근처: 순서 글 만들기.
- `Axicultus\Shell\WorshipListStore.cs` 314줄 근처 `RestoreFromTrash`.

## 결정

1. **재현 먼저**: 항목마다 결함을 드러내는 테스트를 먼저 쓰고 고치기 전 코드에서 실패를 확인한다. 실패하지 않으면(결함이 아니면) 그 항목은 고치지 않고 이유를 tasks의 검증 결과에 적는다. 테스트로 만들 수 없는 것(주석)은 검색 결과로 대신한다.
2. **OBS 설정 보존**: `ObsOperator`가 설정 읽기가 끝나지 않았거나 실패했음을 기억한다. 그 상태에서는 운영자가 설정 창에서 직접 연결(또는 저장)을 누르기 전에는 `obs.json`을 쓰지 않는다. `StartWhenUsed`의 자동 연결은 읽기가 성공으로 끝난 뒤에만 한다. 운영자가 직접 누르면 그때의 값이 저장된다(파일을 고칠 기회). a048의 "해독되지 않는 비밀번호는 다시 써서 지키기"와 같은 방향이다.
3. **장 넘기기 실패**: `Step`이 예외를 잡아 자막 탭 상태 줄에 안내하고 `_host.WriteError`로 남긴다. 취소는 조용히 끝낸다. 같은 꼴의 다른 `async void`(자막 탭의 버튼·키 처리기 중 호스트를 기다리는 것)를 찾아 같은 처리가 필요한지 목록으로 보고하고, 호스트 호출을 기다리는 것만 같이 고친다.
4. **라이브 알림의 스레드**: 자막 세션이 만들어진 디스패처로 옮겨 처리한다(이미 그 스레드면 바로). 계약 글은 그대로 둔다(호스트에 부담을 주지 않는다). 옮겨진 뒤 순서가 뒤바뀌지 않게 한다(큐에 넣은 순서대로).
5. **글상자 목록**: `AllText`와 같은 두 거르기를 쓴다(같은 함수로 묶을 수 있으면 묶는다). 상자 키는 바뀌지 않아야 한다(운영자가 저장한 상자 선택이 그대로 맞아야 한다): 키가 바뀌는 PPT가 있는지 테스트로 확인하고, 바뀌면 총괄에게 보고한다.
6. **곡 순서의 구분자**: 탭·CR·LF 말고는 제어 문자가 없고 `IsTokenText`가 참이면 글로 읽는다. `ToDatabase`도 같은 판단을 쓴다. 절 코드로 저장된 기존 값(제어 문자가 섞인 실제 코드 열)이 글로 잘못 읽히지 않는지 반대 방향 테스트를 넣는다.
7. **순서 글의 쉼표**: 모르는 코드는 쉼표째 뺀다.
8. **휴지통 되돌리기**: 한쪽 옮기기가 실패하면 이미 옮긴 쪽을 되돌려 놓고 실패를 알린다. 되돌리기도 실패하면 그 사실을 오류에 담는다(조용히 삼키지 않는다).
9. **주석의 한글**: 브랜치에서 추가된 코드 줄(`git diff origin/main...HEAD`)의 주석에 있는 한글을 영어로 바꾼다. 글자열, 리소스, `.md`, 테스트 자료는 건드리지 않는다. 동작 변경이 없음을 빌드와 전체 테스트로 확인한다.

## 고치지 않는 것과 근거

- `ImageCanvas.cs` 324줄 근처(`OnPaint` → `RebuildForNewWidth`가 보이는 썸네일을 다시 읽고 높이를 바꿈): 창 크기를 바꿀 때 버벅일 수 있다는 성능 지적이다. 잘못된 결과가 나오는 결함이 아니고, PPT 미리보기 성능 개선(CLAUDE.md 11절 백로그)에서 다룬다.

## 위험과 대응

- **예배 중 경로**: 2·3·4는 자막의 송출 경로다. Axicultus 자막 테스트 전체와 WPF 하네스 `captiontab`·`pptcaption`으로 확인한다.
- **스레드 옮기기**: 지금 UI 스레드에서 바로 처리되던 알림이 늦어지면 자막이 한 박자 늦을 수 있다. 이미 UI 스레드면 바로 처리해 지금 동작을 지킨다.
- **곡 순서**: DB에 저장된 값의 읽기 규칙이다. 기존 코드 열·글 모두에 대한 양방향 테스트로 고정한다. DB는 고쳐 쓰지 않는다.
