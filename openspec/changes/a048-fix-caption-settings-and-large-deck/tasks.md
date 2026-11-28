전제: a046이 끝나 있다(자막 코드가 `Axicultus.Captions`에 있다). 같은 파일을 건드리므로 a046 묶음 B와 동시에 하지 않는다.

## Phase 0. 기준과 영향

- [x] 0.1 브랜치(a046 뒤의 HEAD에서) `feat/a048-fix-caption-settings-and-large-deck`. Debug 빌드, `Axicultus.Tests` 전체와 자막 테스트 결과 기록
- [x] 0.2 design "현재 구조"를 지금 코드와 대조(줄 번호, 부르는 곳, 잡는 예외)

## Phase 1. 실패하는 테스트

- [x] 1.1 가사 인식: 명령줄 상한을 넘는 수의 그림, 둘째 묶음만 실패, 첫 묶음만 실패, 모든 묶음 실패, 한국어 OCR 없음(종료 코드 3), 취소
- [x] 1.2 `caption-style.json`: `"Color": null`, 색 형식이 틀림, 숫자 값이 범위 밖, 파일이 JSON이 아님 → 세션이 기본 모양으로 만들어짐
- [x] 1.3 `obs.json`: `Secret` 없음, `Secret: null`, `Scenes`에 null 원소 → 시작이 예외 없이 끝나고 "저장된 비밀번호 없음"
- [x] 1.4 복구: 코드(`Notations`)와 카포(`Capo`)가 있는 곡을 복구 파일에 담았다 되돌리면 두 값이 그대로다. 그 값이 없는 옛 파일도 읽힌다
- DoD: 새 테스트가 고치기 전 코드에서 실패하는 것을 확인하고 그 출력을 기록한다.

## Phase 2. 고치기

- [x] 2.1 `LyricsReader`: 묶음 나누기, 묶음별 시간 제한, 부분 결과(design 1, 2)
- [x] 2.2 자막 탭의 안내와 문구(design 3)
- [x] 2.3 `CaptionStyle.Parse`와 세션 생성(design 4)
- [x] 2.4 `ObsOperator`(design 5)
- [x] 2.5 복구의 코드와 카포(design 7)
- Constraints: 식별자·주석 영어, 문구는 resx(한국어·영어). 다른 동작을 바꾸지 않는다.

## Phase 3. 회귀와 빌드

- [x] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [x] 3.2 하네스 `pptcaption`(가사 인식 포함)이 고치기 전과 같은 결과

## Phase 4. 리뷰와 기록

- [x] 4.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [x] 4.2 `/code-review`
- [x] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [x] 4.4 커밋·푸시, 아카이브(`openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

구현자 기록(2026-11-28, 커밋 전):

- 기준: 브랜치 `feat/a048-fix-caption-settings-and-large-deck`(b4344f2에서). 기준 수치는 총괄이 준 값(`Axicultus.Tests` 통과 3,758·건너뜀 22, `Axicultuss.Tests` 통과 478·건너뜀 2).
- 0.2 대조: design "현재 구조"와 코드가 같다. 다른 점 하나: `Secret`이 null일 때 처리되지 않는 예외가 나는 곳은 `ReadSavedPassword`가 아니라(자동 연결이면 `StartAsync`의 catch가 잡아 "설정 읽기 실패"로 보인다) 자막 탭 생성자의 `obs.HasSavedPassword`다(`MainWindow.StartObsControl` → `OpenCaptions`, async void). `RecognizeDeckAsync`는 `CaptionConsole.xaml.cs`에 있다.
- 실패하는 테스트(고치기 전 19개 실패, 12개 통과·통과한 것은 기존 동작 고정용): `CaptionLyricsBatchTests`(묶음 나누기 2, 묶음 하나 실패 6, 모두 실패, 한국어 OCR 없음 2, 취소), `CaptionSettingsFileTests`(자막 설정 10+1, obs.json 4, 복구 2), `CaptionConsoleTests`의 읽지 못한 장 안내 2와 복구 1(고친 뒤 추가, 고치기 전 실패 확인).
- 결정: 묶음 크기 그림 40장, 명령줄 인자 16,000자(경로마다 +3), 묶음마다 2분. 모든 묶음이 실패하면 첫 실패를 지금처럼 던진다(안내 "읽지 못했습니다"). 새 문구 `CaptionConsole.LyricsPartlyRead`(2차에서 바꿈, 아래).
- 해독할 수 없는 비밀번호: 앱이 멈추는 경로 없음(`StartAsync`, 자막 탭 연결, 장치 점검이 모두 예외를 잡아 안내). 2차에서 design 13으로 바꿈(아래).
- 빌드: `Axicultus.sln` Debug·Release 오류 0.
- 테스트(1차): `Axicultus.Tests` 통과 3,787·건너뜀 22·실패 3(전체 3,812, 새 테스트 32). 실패 3개(`InALowColumnTheTabScrollsAndKeepsARowOfSteps` 2, `TheCameraPreviewIsResizedWithItsSplitter`)는 고치기 전 코드에서도 똑같이 실패한다(이 PC의 화면 작업 영역 1024×708이 창 높이보다 작음). `Axicultuss.Tests` 통과 478·건너뜀 2.
- 3.2 하네스는 총괄이 돌린다(구현자는 실행하지 않음).

구현자 기록 2차(적대적 리뷰 뒤, design 8-13):

- 고치기 전 실패 21개(1차 코드에서 실행): 악보 줄 송출 중 안내(`SlidesWhoseLyricsCouldNotBeReadStayToldWhileAScoreRowIsOnAir`), 세는 법(`TheSlidesToldUnreadAreCountedFromTheSlidesToBeRead`), 떠난 PPT(`TheReadingOfALeftDeckIsEnded`), 도우미 계속 멈춤 2(`Expected 2 Actual 10`, `Expected 3 Actual 5`), `caption-style.json` 항목별 8, `obs.json` 항목별 5와 자막 탭 첫 연결 1(`Actual ("127.0.0.1", 4455, False, "")`), 해독 불가 2.
- design 10: 안내를 `Status`에서 PPT 상태 줄(`DeckLabel`)로 옮김("파일 · 그림 속 가사 N장 읽음 · M장 중 K장 읽지 못함"). 악보 갱신은 이 줄을 쓰지 않고, 다른 PPT를 열면 `ShowDeck`이 지운다. 문구: "{0}장 중 {1}장 읽지 못함" / "{1} of {0} slides not read".
- design 11: 전체 = 그림을 읽으려던 장 모두, 읽은 장 = 파일마다 한 번. 그림을 하나도 만들지 못하면 "N장 중 N장 읽지 못함"(전에는 "읽는 중" 표시가 남았다).
- design 8: 잇달아 두 번 시간 초과면 남은 묶음을 읽지 않은 장으로 돌려주고 끝냄.
- design 9: 읽기마다 취소 토큰(`_lyricsStop`), 새 PPT의 `StartScore`가 끊음. 끊긴 읽기는 아무것도 안내·기록하지 않는다.
- design 12: 새 `Axicultus.Captions\SettingsJson.cs`(`Read<T>`): 항목마다 따로 읽어 보고 못 읽는 항목만 빼고 읽는다. `CaptionSession`과 `ObsOperator.StartAsync`가 쓴다. obs.json: `Profile`의 모든 항목에 기본값, 범위 밖 포트는 기본 포트, 씬은 길이 4가 아니면 빈 이름 4개.
- design 13: 시작할 때 해독해 보고 실패하면 저장된 비밀번호 없음, 자동 연결 안 함, 안내는 "연결 안 됨".
- resx 줄 끝: HEAD의 blob은 모두 LF이고, 작업 사본은 CRLF에 마지막 줄만 LF다(git diff는 더한 한 줄뿐).
- 빌드: Debug·Release 오류 0. 테스트: `Axicultus.Tests` 통과 3,807·건너뜀 22·실패 3(전체 3,832, 새 테스트 52, 실패 3개는 위의 화면 크기 탓), `Axicultuss.Tests` 통과 478·건너뜀 2.

구현자 기록 3차(2차 리뷰 뒤, design 9·14·15):

- 고치기 전 실패 5개(2차 코드에서 실행): `WhenNoSlideCouldBeReadAllAreToldUnreadWhileAScoreRowIsOnAir`(`Expected "글자.pptx · 3장 중 3장 읽지 못함" Actual "글자.pptx"`), 같은 이름 두 번: `caption-style.json` 2개, `obs.json` 2개(`Actual ("127.0.0.1", 4455, "", False, False)`).
- design 14: 모두 읽지 못한 때도 `DeckLabel`에 "N장 중 N장 읽지 못함"(전체는 읽으려던 장 모두). `Status`의 자세한 글은 그대로. 한국어 OCR 없음은 제외: `LyricsReader.NoKoreanOcrException`(IOException, 글은 같음)으로 가려 낸다. 조용한 취소는 원래 따로 끝난다.
- design 15: `SettingsJson.Read`가 `JsonDocument`로 읽고 항목을 이름별로 모아(뒤의 값이 이김) 항목마다 읽어 본다.
- design 9 고정 테스트 `TheScreenGoingToASongsTextDoesNotEndTheDecksReading`(고치기 전에도 통과): 화면이 찬양 가사로 갔다 같은 PPT로 돌아와도 읽기 토큰은 끊기지 않고, 같은 읽기가 끝나 두 장 모두 가사를 가진다.
- 빌드: Debug·Release 오류 0. 테스트: `Axicultus.Tests` 통과 3,817·건너뜀 22·실패 0(전체 3,839, 새 테스트 59), `Axicultuss.Tests` 통과 478·건너뜀 2.

구현자 기록 4차(`/code-review` 뒤, design 13·16-19):

- 고치기 전 실패 4개(3차 코드에서 실행): `AReadingThatCannotStartLeavesTheNextDeckToOpen`(`ObjectDisposedException : The CancellationTokenSource has been disposed.`), `TheCaptionTabsFirstConnectionKeepsASecretThisPcCannotDecrypt`(`Expected "AAAAAAAA" Actual ""`), `AFailureOfALeftDecksReadingDoesNotWriteOverTheNextDeck`(`Actual ("글자.pptx · 2장 중 2장 읽지 못함", "그림 속 가사를 읽지 못했습니다 · picture not written")`), `TheReasonsSomeSlidesCouldNotBeReadAreLoggedOnce`(`Expected 1 Actual 0`).
- design 16: 임시 폴더 만들기를 `try` 안으로 옮김. 실패하면 다른 읽기 실패와 같이 끝나고(기록, `Status` "읽지 못했습니다", `DeckLabel` "N장 중 N장"), `finally`가 `_lyricsStop`을 비운다. 테스트용 자리: `CaptionConsole.OcrFolder`(기본은 지금과 같은 `%TEMP%\Axicultus\ocr`).
- design 13: 해독 안 되는 `Secret`을 `_unreadableSecret`에 두고, 비밀번호 없이 저장할 때 그대로 다시 쓴다. 새 비밀번호를 저장하면 지운다. `HasSavedPassword`는 false.
- design 17: 일반 catch는 기록한 뒤 그 PPT가 지금 PPT가 아니면 `DeckLabel`·`Status`를 쓰지 않는다.
- design 18: `RunHelperAsync`의 `Kill`이 던지는 것은 모두 삼키고(디버그 출력) 원래 예외를 다시 던진다. 테스트 없음: `Kill`은 실제 프로세스 경로에만 있고 대리자 자리 밖이다.
- design 19: 읽은 결과의 `page.Error` 값마다(같은 값은 한 번) `_host.WriteError`. 묶음 실패의 `Error`는 종료 코드와 stderr(`OcrError`), 시간 초과 메시지, 실행 실패 메시지를 담는다.
- 규칙: 새 테스트 주석의 한글 시나리오 이름을 영어로 바꿈. 변경 전체(`git diff HEAD`의 더한 줄과 새 파일 셋)에서 문자열 밖 한글 없음.
- 빌드: Debug·Release 오류 0. 테스트: `Axicultus.Tests` 통과 3,821·건너뜀 22·실패 0(전체 3,843, 새 테스트 63), `Axicultuss.Tests` 통과 478·건너뜀 2.

총괄 검증(2026-11-28):

- 적대적 리뷰(별도 에이전트) 두 번. 1차: 주요 2(악보 줄 송출 중 안내가 지워짐, 한 항목 때문에 설정 전체가 기본값이 되고 다음 저장에서 사라짐), 사소 4. 2차: 1차 지적이 모두 고쳐진 것을 확인, 새 지적 3. 그중 "PPT에서 찬양 가사로 옮겨도 인식이 계속된다"는 고치지 않기로 정했다(design 9: 세션이 그 PPT를 들고 있다가 돌아오면 다시 읽지 않으므로 멈추면 반만 읽힌 채 남는다. 테스트로 고정). 나머지 2건(모두 읽지 못한 때의 안내, 같은 이름이 두 번 나오는 설정 파일)은 고쳤다(design 14·15).
- `/code-review`: 6건 모두 고쳤다(design 13 확장, 16-19). 인식 시작 실패 뒤 다음 PPT가 열리지 않던 결함(이번 변경에서 생김), 해독되지 않는 OBS 비밀번호가 탭을 처음 볼 때 파일에서 지워지던 결함, 떠난 PPT의 실패가 지금 PPT 표시를 덮던 결함, 도우미 종료 오류가 시간 초과를 가리던 결함, 일부 실패의 이유가 오류 기록에 남지 않던 것, 테스트 주석의 한글.
- 테스트가 없는 것: 도우미를 끝낼 때의 오류 삼키기(design 18)는 실제 프로세스 경로에만 있어 테스트하지 않았다. 실제 도우미의 묶음 실행은 하네스가 한 번 지난다(10장, 한 묶음). 여러 묶음과 실제 도우미의 멈춤은 사람 확인 대상이다.
- 총괄이 직접 돌린 결과: Debug·Release 솔루션 빌드 오류 0. `Axicultus.Tests` 3821 통과·22 건너뜀·실패 0(a048 테스트 파일 둘 50 통과), `Axicultuss.Tests` 478 통과·2 건너뜀. 코드와 주석의 한글 0(글자열 안의 테스트 자료만), 지워진 파일 0.
- 하네스 `pptcaption,captiontab`(로그 `a048-verify.log`, 격리 사본, 모니터 2대): 실제 데이터 unchanged, 처리되지 않은 예외·중단·멈춤 0. `[PPT]` 69줄과 `[AIR]` 60줄(순번 제외)이 a046 검증 실행과 모두 같다. 가사 인식은 실제 도우미로 10장을 읽었다(`lyrics read on 10 slides`). `captiontab` PASS 17·FAIL 0. `[BINDING]` 7줄은 앞 실행과 같은 줄이다.
- 화면이 한 대(1024×768)로 잡혀 있던 동안 배치 테스트 3개가 실패했다. 이 변경과 무관하며(리뷰어가 창 높이 제한으로 확인) 모니터 2대에서는 통과한다.
