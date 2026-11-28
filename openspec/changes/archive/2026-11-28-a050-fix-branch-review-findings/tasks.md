전제: a047 Phase 0(하네스)이 커밋되어 있다. 같은 파일을 건드리는 다른 묶음과 동시에 하지 않는다.

## Phase 0. 기준과 영향

- [x] 0.1 브랜치 `feat/a050-fix-branch-review-findings`. Debug 빌드, `Axicultus.Tests`·`Axicultuss.Tests` 전체 결과 기록
- [x] 0.2 design "현재 구조"의 항목마다 지금 코드와 대조: 줄 번호, 부르는 곳, 이미 잡는 예외, 실제 스레드

## Phase 1. 실패하는 테스트

- [x] 1.1 OBS: 읽기 실패(JSON 아님, 잠긴 파일) 뒤 자막 탭을 처음 봐도 `obs.json`이 그대로다. 운영자가 직접 연결하면 저장된다
- [x] 1.2 장 넘기기: 호스트의 넘기기가 예외를 던져도 처리되지 않는 예외가 없고, 상태 줄 안내와 오류 기록이 남는다
- [x] 1.3 라이브 알림: 다른 스레드에서 올린 알림이 세션의 UI 스레드에서 처리된다. 순서가 지켜진다
- [x] 1.4 글상자 목록: `mc:AlternateContent`의 상자가 한 번만, 숨긴 상자는 안 나온다. 기존 PPT의 상자 키는 그대로다
- [x] 1.5 곡 순서: 탭·줄바꿈으로 나눈 글, 실제 코드 열(양방향)
- [x] 1.6 순서 글: 모르는 코드가 처음·가운데·끝에 있을 때
- [x] 1.7 휴지통 되돌리기: 둘째 옮기기 실패 뒤 두 폴더가 처음 상태다. 다시 시도하면 사본 없이 되돌려진다
- DoD: 새 테스트가 고치기 전 코드에서 실패하는 것을 확인하고 출력을 기록한다. 실패하지 않는 항목은 고치지 않고 이유를 적는다.

## Phase 2. 고치기

- [x] 2.1 `ObsOperator`와 `StartWhenUsed`(design 2)
- [x] 2.2 `Step`과 같은 꼴의 처리기(design 3)
- [x] 2.3 `CaptionSession.OnLiveChanged`(design 4)
- [x] 2.4 `CaptionDeck` 상자 목록(design 5)
- [x] 2.5 `SongExchangeSequence`(design 6), `gfLyrics` 순서 글(design 7)
- [x] 2.6 `WorshipListStore.RestoreFromTrash`(design 8)
- [x] 2.7 주석의 한글(design 9)
- Constraints: 식별자·주석 영어, 문구는 리소스(한국어·영어). 항목 밖의 동작을 바꾸지 않는다.

## Phase 3. 회귀와 빌드

- [x] 3.1 Debug·Release 빌드(저장소 bin), `Axicultus.Tests`·`Axicultuss.Tests` 전체
- [x] 3.2 WPF 하네스 `captiontab`·`pptcaption`이 고치기 전과 같은 결과, Axicultuss 하네스 `smoke`

## Phase 4. 리뷰와 기록

- [x] 4.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [x] 4.2 `/code-review`
- [x] 4.3 총괄 완료 검증과 "검증 결과" 기록
- [x] 4.4 커밋·푸시, 아카이브(`openspec/specs/live-camera-captions/spec.md`에 delta 반영)

## 검증 결과

(구현자 기록, 2026-11-28. 3.2와 Phase 4는 총괄 몫)

- 0.1 기준(3b29310): Debug 오류 0, `Axicultus.Tests` 3821 통과/22 건너뜀/0 실패, `Axicultuss.Tests` 478/2/0.
- 0.2 대조: 줄 번호는 design과 맞음(StartWhenUsed 1233, Step 1402, OnLiveChanged 369, CaptionDeck 51, IsVerseCodes 68, gfLyrics 3372, RestoreFromTrash 305). `ConfigureAsync`를 부르는 곳은 자막 탭의 `ConnectAsync`뿐(자동 연결과 연결 버튼). Axicultus의 `LiveSessionService.SessionChanged`는 부른 스레드에서 바로 올리고, 부르는 쪽(`MainViewModel`, `OutputAlertsWindow`)은 모두 UI 스레드(`ConfigureAwait(true)`만 씀, `Task.Run` 없음): Axicultus에서는 3이 일어나지 않고 계약상 결함이다. Axicultus의 장 넘기기는 PowerPoint 네이티브 경로가 모든 예외를 잡고, 장 번호 경로는 렌더 서비스가 실패를 결과로 돌려준다(디코드 필터 밖 예외, 디스크 캐시 읽기만 남음): 실제로 던질 일은 드물다.
- 1 실패 확인: 새 테스트 19개(Axicultus) 중 11개, Axicultuss 7개 중 5개(기존 `"1,,"` 기대값 포함)가 고치기 전 코드에서 실패. 1.1 4개, 1.2 2개, 1.3 1개, 1.4 1개, 1.5 3개(뒤에 가져오기 테스트로 바꿈), 1.6 5개, 1.7 1개. 반대 방향(코드 열, 키 그대로) 테스트 8개는 고치기 전에도 통과.
- 적대적 리뷰 뒤(design 10-14): 새 테스트 28개 중 14개가 고치기 전 코드에서 실패(가져오기 5, 글상자 1, 호스트 호출 5, OBS 안내 3). 순서 테스트(14)는 1차 수정으로 이미 통과.
- 5(design 10): `SongExchangeSequence.cs`는 HEAD 그대로. 교환 파일(XML·JSON)과 글 파일 가져오기에서 순서 값을 자르고 탭·CR·LF를 쉼표로 바꾼다(`ImportExportService.SequenceText`, 두 곳). 저장된 코드 열(`e\te`, `p\te`, `1\t2` 등)은 HEAD처럼 읽고 쓴다.
- 4(design 11): 같은 키의 상자가 이미 있을 때만 `mc:Fallback` 도형을 뺀다. 숨긴 상자는 목록에 남고, 이름 없는 상자의 번호와 `AllText`는 HEAD와 같다.
- 2(design 12): `CanStepOutput`, `SendPicked`, PPT 읽기(필터 없이 모든 예외, 탭 수명의 취소만 조용히), `Capture`(실패하면 저장할 것으로 남기고 판번호를 쓰지 않음)를 잡아 안내·기록한다.
- 1(design 13): 읽지 못했거나 읽기 전에 만든 탭은 "연결을 누르면 지금 창의 값으로 저장된다"는 안내를 상태 줄과 설정 창에 보인다.
- 8(design 9): 브랜치 추가 줄의 한글 주석 57줄 중 47줄을 영어로(파일 18개), 문장이 이어지는 기존 줄 2줄도 함께. 남긴 10줄은 가사·글꼴 이름·자료 예시 인용.
- `/code-review` 뒤(design 15-22): 새 테스트 21개 중 10개가 고치기 전 코드에서 실패(Axicultus 7, Axicultuss 3).
  - 15: `ICaptionHost.SendPicked()` → `Task SendPickedAsync()`(Axicultus는 명령의 `ExecuteAsync`). `PickedSending`은 남긴다: 본창이 스스로 시작한 보내기는 탭이 기다릴 수 있는 다른 길이 없다.
  - 16·17: 세션은 만든 스레드의 디스패처를 생성자에서 만든다(`Dispatcher.CurrentDispatcher`). 즉시 처리와 줄 세우기는 한 잠금 안에서 정한다. 두 스레드가 동시에 오는 경우는 제품 코드에 고리 없이 결정적으로 시험할 수 없다. 펌프 없는 MTA 스레드에서 세션을 만들던 기존 테스트 1개는 STA에서 돌게 바꿨다.
  - 18·19: `ObsOperator.StartAsync`는 한 번의 읽기 작업을 돌려준다. 읽기 전에 만든 탭은 처음 보일 때 그 읽기를 기다려(호스트가 시작하지 않았으면 시작) 설정 창을 채운다. 읽기 실패 안내는 OBS 줄과 설정 창에만 보인다.
  - 20: 복구 중 PPT 읽기는 탭 수명의 취소 말고 어떤 예외든 PPT 없이 계속한다(`IsDeckReadError` 삭제).
  - 21: `FrmExport`의 순서 글은 `ConvertSequenceToTextString`(알려진 코드 0-103·111·112의 한두 개 조합 전부 이전 출력과 같음). 내보내기 줄 자체는 폼 밖에서 부를 수 없어 테스트하지 않았다.
  - 22: Axicultuss XML 항목 가져오기는 탭·줄바꿈 사이의 토큰을 잃었다(`1\tc\t2` → 빈 순서). `ConvertTextStringToSequence` 안에서 자르고 쉼표로 바꾼다(부르는 곳 3곳 모두 글 입력).
- 총괄 검증 중(design 23): 하네스의 복구 안내 차이는 a050 탓이 아니다. 탭은 `Loaded`를 두 번 받고(접힌 칸에서 만들 때, 처음 보일 때) `LoadRecovery`가 두 번 읽으며, 안내가 보이는지는 첫 화면 맞춤의 상태 줄 지우기와 몇 ms 차이로 갈린다(HEAD와 지금의 사건 순서가 같음을 같은 실험으로 확인). 고침: 탭이 스스로 복구를 저장한 뒤의 `LoadRecovery`는 파일을 다시 읽지 않고 앞서 읽은 것(앞선 실행의 것, 없으면 없음)을 그대로 쓴다. 새 테스트 3개 중 2개가 고치기 전에 실패.
- 2차 적대적 리뷰 뒤(design 24-28, 고치기 전 확인은 `--no-incremental` 빌드): 새·바뀐 테스트 중 5개가 고치기 전에 실패(Axicultuss 1, Axicultus 4: OBS 안내 지우기 2, 복구 읽기 대기 1, 올리기 취소 1).
  - 24: `ConvertTextStringToSequence`는 1~99절을 받는다. 9의 한계는 원래 EasiSlides(디컴파일 원본 `num > 9 || num < 0`)에서 그대로 온 것이고, 글 형식은 쉼표로 나누므로 한 글자 가정이 없다. 1~99 왕복, 0=후렴, 음수·100 이상은 버림.
  - 25: 실제 호스트 테스트 2개(출력 칸 렌더를 붙잡으면 작업이 끝나지 않음, 렌더가 던지면 그 예외로 끝남). 올리기를 `Execute`+완료된 작업으로 바꾸면 둘 다 실패함을 확인. 통과만 하던 테스트는 지움.
  - 26: `CaptionHost.cs` 계약 글에 세션을 메시지를 돌리는 UI 스레드에서 만든다는 줄.
  - 27: 탭의 저장은 진행 중인 복구 읽기를 기다린다(`_recoveryRead`). 저장소의 항목 확인으로 첫 읽기를 붙잡아 시험.
  - 28: 올리기 취소는 조용히, 연결 성공 뒤 "읽지 못함" 안내를 지움.
- 3.1 Debug·Release 오류 0, `Axicultus.Tests` 3863/22/0, `Axicultuss.Tests` 495/2/0.

총괄 검증(2026-11-28):

- 적대적 리뷰(별도 에이전트) 두 번. 1차: 치명 0, 곡 순서 규칙이 저장된 값의 뜻을 바꾼다는 주요 지적(길이 5까지의 저장 값 3,368,420개 중 840개가 다르게 읽히고 쓰임)을 받아 읽기 규칙을 HEAD로 되돌리고 가져오기 쪽에서 고쳤다(design 10). 2차: 치명 0, 핵심 수정 다섯 가지를 임시로 무력화하면 테스트가 실패함을 확인. Axicultuss가 순서 글을 읽을 때 10절 이상을 버리는 기존 결함을 새로 찾아 고쳤다(design 24).
- `/code-review`: 7건 모두 처리(design 15-22). 호스트 인터페이스가 하나 바뀌었다: `SendPicked()` → `Task SendPickedAsync()`.
- 총괄 결정으로 원래 지적과 다르게 한 것: 숨긴 글상자는 목록에 그대로 둔다(design 11). 저장된 곡 순서를 읽는 규칙은 건드리지 않는다(design 10).
- 테스트가 없는 것: 두 스레드가 동시에 알릴 때의 순서(design 17, 리뷰어가 읽어서 교착·누락 없음 확인), 휴지통 되돌리기의 '되돌리기도 실패' 가지, `FrmExport`의 내보내기 줄 자체(폼 안), 실제 곡 DB에 대한 순서 읽기 비교(`RealDatabasesFact`는 실제 DB가 있어야 돈다).
- 총괄이 직접 돌린 결과(`--no-incremental`로 전체 다시 빌드): Debug·Release 오류 0. `Axicultus.Tests` 3863 통과·22 건너뜀·실패 0, `Axicultuss.Tests` 495 통과·2 건너뜀. `SongExchangeSequence.cs`는 HEAD와 내용이 같다. 변경 40개 파일, 공백 무시 비교와 3줄 차이(통째로 바뀐 파일 없음).
- WPF 하네스 `pptcaption,captiontab`(로그 `a050-verify2.log`, 격리 사본): 실제 데이터 unchanged, 처리되지 않은 예외·중단·멈춤 0, `captiontab` 16 PASS·0 FAIL, `[AIR]` 67줄과 first-chance 3줄이 a048 실행과 같다. `[PPT]` 69줄 중 1줄만 다르다: 자막 탭을 처음 열 때 상태 줄이 a048 실행에서는 비어 있었고 지금은 복구 안내가 보인다. 격리 사본의 프로필에는 2026-11-21 실행이 남긴 복구 파일이 실제로 있어(`CaptionRecovery` 폴더) 안내가 맞다. 전에는 첫 화면 맞춤의 상태 줄 지우기와의 경쟁으로 보였다 안 보였다 했다(design 23의 조사).
- Axicultuss 하네스 `smoke`(로그 `a050-smoke2.log`): 종료 코드 0, 실제 폴더와 레지스트리 unchanged. 실행 뒤의 숨은 PowerPoint는 몇 초 뒤 스스로 끝났다.
