# Design — a048 자막 설정 파일과 장이 많은 PPT

## 현재 구조 (영향 근거)

CodeGraph CLI는 이 세션에서 쓸 수 없었다. 아래는 a046 묶음 A의 `/code-review`(2026-11-28) 결과다. 구현자는 Phase 0에서 줄 번호와 호출하는 곳을 다시 확인한다.

- `LyricsReader.ReadAsync(images, token)`(`Axicultus.Captions\LyricsReader.cs`): `ocr\Axicultus.Ocr.exe`를 `"ko"`와 모든 그림 경로를 인자로 한 번 실행하고 2분 뒤 프로세스를 끝낸다. 종료 코드 3은 한국어 OCR 없음, 그 밖의 0 아닌 값은 오류다. 결과는 표준 출력의 JSON 배열(`OcrPage`: `File`, `Error`, `Lines`).
- 부르는 곳: 자막 탭의 `RecognizeDeckAsync`가 PPT의 그림 있는 장을 모두 임시 폴더(`%TEMP%\Axicultus\ocr\<guid>\`)에 쓰고 한 번에 넘긴다. 결과에서 `Error == null`인 장만 쓴다. 예외(`Win32Exception`, `TimeoutException` 등)가 나면 "가사를 읽지 못했다"는 안내만 남고 결과는 없다. 테스트는 `ReadLyrics` 대리자를 바꿔 끼운다.
- `CaptionSession` 생성자: `caption-style.json`을 `JsonSerializer.Deserialize<CaptionStyle>`로 읽고 `CaptionStyle.Parse(saved.Color, …)`를 부른다. `catch`는 `IOException`, `JsonException`, `UnauthorizedAccessException`만 잡는다. `Parse`는 `Regex.IsMatch(color, …)`를 null 검사 없이 부른다.
- `ObsOperator.StartAsync`: `obs.json`을 `Profile` 레코드로 읽고 `_savedSecret = profile.Secret`을 그대로 넣는다. `HasSavedPassword`와 `ReadSavedPassword`가 `_savedSecret.Length`를 쓴다. `Scenes`는 길이 4인지만 보고 원소가 null인지는 보지 않는다.

## 결정

1. **가사 인식은 묶음으로**: `ReadAsync`가 그림 목록을 묶음으로 나눠 도우미를 차례로 실행한다. 묶음 크기는 "그림 수 상한"과 "명령줄 길이 상한" 둘 다로 정한다(값은 구현자가 정하고 상수에 이유를 적는다. 명령줄은 Windows 제한 32,767자보다 충분히 작게). 시간 제한은 묶음마다 따로 건다.
2. **부분 결과를 남긴다**: 한 묶음이 실패(실행 실패, 시간 초과, 0 아닌 종료 코드, 깨진 출력)하면 그 묶음의 그림은 `Error`가 있는 장으로 돌려주고 다음 묶음을 계속한다. 종료 코드 3(한국어 OCR 없음)은 어느 묶음에서 나와도 전체를 끝내고 지금과 같은 안내를 한다(다음 묶음도 같은 결과이므로). 취소(`CancellationToken`)는 지금처럼 바로 끝낸다.
3. **안내**: 읽지 못한 장이 있으면 자막 탭에 "N장 중 M장을 읽지 못했다"는 한 줄을 보인다(문구 한국어·영어). 읽은 장의 가사는 지금처럼 쓴다. 모든 묶음이 실패하면 지금의 "읽지 못했다" 안내와 같다.
4. **자막 설정 파일**: `CaptionStyle.Parse`는 색이 null이거나 형식에 맞지 않으면 기본 색을 쓴다(다른 값의 범위 맞추기는 지금 그대로). 세션 생성자는 설정 파일을 읽는 중의 어떤 예외에도 기본 모양으로 시작하고, 원인을 디버그 출력에 남긴다. 파일은 고쳐 쓰지 않는다(다음에 운영자가 무언가 바꾸면 그때 올바른 값으로 저장된다).
5. **OBS 설정 파일**: `Secret`이 없거나 null이면 빈 글자열(저장된 비밀번호 없음), `Scenes`의 null 원소는 빈 글자열, `Host`·`Camera`는 지금처럼. 암호 해독이 실패하는 값(다른 PC에서 가져온 파일)은 지금 동작을 확인해, 예외로 앱이 멈추는 경로가 있으면 "저장된 비밀번호 없음"으로 처리하고 연결 안내를 보인다.
7. **복구의 코드와 카포**: `MainViewModelCaptionHost.ToRecovery`/`FromRecovery`(Axicultus 연결부, 예전 `CaptionRecoveryStore.Capture`/`Restore`)가 `WorshipListItemDto`의 13개 값 중 `Capo`와 `Notations`를 옮기지 않는다. 둘 다 옮긴다. 복구 파일은 값이 늘어날 뿐이고(저장소는 항목을 해석하지 않는다), 그 값이 없는 옛 파일은 기본값(카포 0, 코드 없음)으로 읽힌다. 테스트: 코드와 카포가 있는 곡을 담았다 되돌리면 그대로, 옛 모양 파일도 읽힘.
6. **실패하는 테스트 먼저**: 각 결함을 그대로 드러내는 테스트를 먼저 넣고 실패를 확인한 뒤 고친다. 도우미 실행은 테스트에서 바꿔 끼울 수 있게 한다(지금 `ReadLyrics` 대리자는 자막 탭 쪽에만 있다. `LyricsReader` 안의 "한 묶음 실행"을 대리자나 내부 메서드로 분리해 묶음 나누기와 부분 결과 규칙을 OCR 없이 검사한다).

## 위험과 대응

- **묶음 사이의 순서와 파일 짝**: 결과의 `File`로 장을 찾으므로 순서에 기대지 않는다. 테스트로 고정한다(둘째 묶음만 실패, 첫 묶음만 실패).
- **시간이 더 오래 걸림**: 묶음마다 도우미를 다시 띄운다. 장이 적은 PPT(대부분)는 한 묶음이라 지금과 같다.
- **두 앱 공통 코드**: Axicultus 자막 테스트 전체와 하네스 `pptcaption`(가사 인식 포함)으로 확인한다.
