전제: a046이 끝나 있다(공용 라이브러리와 확정된 호스트 인터페이스).

## Phase 0. 기준, 영향, 하네스

- Goal: 실제 데이터를 건드리지 않고 Axicultuss를 실행해 볼 수단을 만든다. 제품 코드는 바꾸지 않는다.
- [x] 0.1 브랜치 `feat/a047-winforms-camera-captions`. `Axicultuss` Debug 빌드, `Axicultuss.Tests` 전체 결과 기록
- [x] 0.2 design 1의 사실을 지금 코드와 대조(줄 번호 갱신). a046의 호스트 인터페이스에 모자란 것이 있으면 목록으로 보고(고치는 것은 총괄이 결정)
- [x] 0.3 `tools\AxicultussHarness`: 격리 사본 스크립트, 실행 스크립트(보호 검사), 드라이버, `smoke` 단계, README
- Tests: 보호 검사가 조건마다 실행을 막는지(모니터 1대는 사람이 확인하기 어려우므로 스크립트 조건을 코드로 읽어 검토), `smoke` 실행 뒤 실제 폴더 해시 unchanged
- DoD: `smoke`가 격리 사본에서 통과하고 로그에 처리되지 않은 예외가 없다. 실행 전 사용자에게 창이 뜬다고 알렸다.
- Constraints: 보호 검사를 풀거나 우회하지 않는다. 실제 앱(`C:\Axicultuss\Axicultuss.exe`)을 실행하지 않는다. 실행 중인 Axicultuss가 있으면 멈추고 보고한다.

## Phase 1. 호스트 연결부 (상태 읽기)

- Goal: 송출 상태를 읽어 호스트 인터페이스로 내놓는다. 화면은 아직 없다.
- [ ] 1.1 실패하는 테스트 먼저: `SongSettings`로 만든 항목에서 상태 묶음 만들기(쇼가 돌 때는 LiveItem, 아니면 OutputItem; 종류 맞춤; 지금 글과 페이지 목록; 검은 화면; PPT 경로와 장; `Slide[,]`를 바꾸지 않음), 달라졌을 때만 알림
- [ ] 1.2 `Axicultuss.csproj`: `UseWPF`, `Microsoft.AspNetCore.App`, `Axicultus.Captions` 참조. 기존 빌드와 테스트가 그대로인지
- [ ] 1.3 연결부 구현(design 2.1~2.3, 2.9, 2.11): 상태 읽기 순수 함수, 타이머, 넘기기·올리기, 곡 목록, 작은 장 그림, 저장 폴더
- Tests: 1.1, 저장 폴더 규칙(`AXICULTUSS_SETTINGS_FILE`이 있을 때와 없을 때), 올리기는 쇼가 돌면 하지 않음
- DoD: `Axicultuss` Debug·Release 빌드 오류 0, `Axicultuss.Tests`·`Axicultus.Tests` 전체 통과, 쇼 창·송출 로직 파일에 변경 없음(`git diff --stat`로 확인)
- Constraints: 식별자·주석 영어. DB 접근은 `SQLiteController`·`gfDatabase` 경유. 새 메서드는 알맞은 `Global\gf*.cs`나 새 파일(연결부)로.

## Phase 2. 본창의 탭과 키

- Goal: 자막 탭이 본창에 보이고 조작된다.
- [ ] 2.1 `FrmMain.Captions.cs`: 탭 줄과 자막 칸 놓기, 탭 전환, 새 UI 층과 맞추기(design 2.4, 2.5, 2.8)
- [ ] 2.2 출력 메뉴 항목 둘과 F2·F6, 메뉴 글(한국어·영어)(design 2.6)
- [ ] 2.3 설정 창 키보드 연동, 닫을 때 끝내기, 예배 순서가 바뀌면 다시 만들기, 실패해도 본창은 뜸(design 2.7, 2.12, 5)
- [ ] 2.4 OCR 도우미 복사, 옛 .ppt 안내 (내보낼 곳 "카메라"는 Phase 2b가 끝날 때까지 꺼 둔다)
- [ ] 2.5 하네스 `captiontab` 단계와 그림
- Tests: 하네스 `captiontab`(탭 줄이 시작부터 보임, F2 전환, 고른 항목이 송출 칸에 올라가고 자막이 준비됨, F6으로 자막이 나감, 페이지 단계, 다른 예배 순서, 닫기), 본창 최소 크기와 저장된 분할 값에서의 그림, 기존 키(F3·F5·F9·F11·F12, 송출 칸의 Space·화살표)가 그대로인지
- DoD: 하네스 통과, 실제 폴더 unchanged, Debug·Release 빌드(저장소 bin), 전체 테스트 통과, `FrmMain.Designer.cs`에 변경 없음
- Constraints: 기존 클릭·더블클릭·끌어 놓기·단축키를 바꾸지 않는다.

## Phase 2b. 회중 화면의 카메라 자막

- Goal: 쇼가 도는 동안 자막이 회중 화면 아래쪽에 보인다(내보낼 곳 "카메라").
- Scope: design 2.13. 쇼 창의 그리기 코드는 바꾸지 않는다.
- [ ] 2b.1 실패하는 테스트 먼저: 자막 창이 보이는 조건(쇼·송출·"카메라"·검은 화면), 영역 맞추기 계산
- [ ] 2b.2 자막 창(투명, 클릭 통과, 활성화되지 않음)과 열기·닫기·영역 맞추기, 쇼 창을 만들고 끝내는 곳의 호출
- [ ] 2b.3 내보낼 곳 "카메라" 선택을 Axicultuss에서 켜기
- [ ] 2b.4 하네스 `captionshow` 단계: 찬양 항목으로 쇼를 잠깐 띄워 자막 창이 회중 화면 영역에 보이는지, 자막을 끄면 숨는지, 검은 화면에서 숨는지, 쇼가 끝나면 사라지는지, 본창의 키보드 포커스가 그대로인지. 그림 저장
- Tests: 2b.1, 하네스 `captionshow`, 자막을 끈 상태의 회중 화면이 변경 전과 같은지(자막 창이 숨겨져 있음)
- DoD: 하네스 통과, 실제 폴더 unchanged, 쇼 창 그리기 파일(`ImageTransitionControl*.cs`, `gfDisplay.cs`)에 변경 없음, 사람이 확인할 것에 "PowerPoint 쇼 위의 자막"과 "수동 영역 지정" 기록
- Constraints: 쇼를 띄우는 하네스 단계는 짧게 하고 끝나면 반드시 쇼를 멈춘다. PowerPoint를 띄우는 단계는 넣지 않는다.

## Phase 3. 회귀와 배포 준비

- [ ] 3.1 Debug·Release 빌드, `Axicultuss.Tests`·`Axicultus.Tests` 전체, 하네스 `smoke`·`captiontab`, WPF 하네스 `captiontab`
- [ ] 3.2 긴 본문(성경 여러 장)에서 상태 읽기 비용 재기(하네스 로그)
- [ ] 3.3 배포 안내: 늘어나는 파일, ASP.NET Core 런타임 확인 방법, 되돌리는 방법. 방송 PC 배포는 사용자가 한다

## Phase 4. 리뷰와 기록

- [ ] 4.1 묶음(Phase 0, 1, 2, 2b)마다 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 묶음마다 `/code-review`
- [ ] 4.3 총괄 완료 검증(그림 포함)과 "검증 결과" 기록. 사람이 확인할 것 목록: 한글 조합 입력, 실제 OBS 연결, 듀얼 모니터 쇼에서의 따라가기
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/changes/archive/<날짜>-a047-winforms-camera-captions`, 새 `openspec/specs/winforms-camera-captions/spec.md`)

## 검증 결과

### Phase 0 (구현자, 2026-11-28)

- 기준: `dotnet build Axicultus.sln -c Debug` 오류 0(경고 1805). `Axicultuss.Tests` 478 통과, 2 건너뜀, 실패 0.
- 하네스: `tools\AxicultussHarness` 드라이버 빌드 오류 0. 스크립트 셋은 PowerShell 파서로 구문 오류 0.
- 실행하지 않고 확인한 것: `HarnessSafety.ps1`의 함수를 임시 폴더(스크래치)에만 넣어 시험했다. 하네스 폴더 밖·하네스 폴더 자체·보호 폴더와 겹침·연결 지점(junction)을 지나는 쓰기 대상, 원본과 대상 겹침, 하네스 안의 원본, 이미 있는 대상, 연결 지점이 든 폴더 지우기·미러는 모두 종료 코드 20으로 거부됐고 연결 지점 너머의 파일은 그대로였다. 원본의 연결 지점은 복사되지 않았다(`/XJ`). 설정 경로 바꾸기는 경로 경계(`C:\Axicultuss2`는 그대로), 대소문자, 배열 값, 종류 앞 글자가 붙은 값(`PC:\...`), 한글, DWord·Binary 값 보존을 확인했다. 실제 `settings.json`은 메모리에서만 바꿔 보았다(파일 수정 시각 그대로): 바뀌는 값 5개(`root_directory`, `export_dir`, `import_dir`, `praiseoutput_dir`, `media_dir`), 남는 실제 루트 경로 0.
- 아직 확인하지 못한 것(첫 실행에서 총괄이 확인): 보호 검사 1~4의 실제 동작, `smoke` 통과와 실행 전후 해시 unchanged, 드라이버의 격리 확인, 로그 내용.

### Phase 0 리뷰 반영 (구현자, 2026-11-28)

- 결정 a: `Run-Harness.ps1`은 PowerPoint가 돌고 있으면 실행하지 않는다(15). 실행 뒤 남은 PowerPoint는 `LEFTOVER POWERPNT #pid started 시각`으로 출력만 한다.
- 결정 b: `Harness.csproj`의 빌드 뒤 단계가 출력 폴더에서 `Axicultuss.exe`, `Axicultuss.runtimeconfig.json`, `Axicultuss.runtimeconfig.dev.json`을 지운다. 빌드 뒤 확인: 출력 폴더의 실행 파일은 `Axicultuss.Tests.exe`, runtimeconfig는 드라이버 것뿐이다. `Run-Harness.ps1`도 남아 있으면 13으로 거부한다.
- B1: 비교 대상은 있는 보호 폴더 전부(`D:\OneDrive - 산소망교회` 추가)와, 사본의 설정·JSON·`*.esw` 등이 가리키는 사본 밖 폴더(실행할 때 사본에서 모아 맨 위 폴더로 줄이고 `check:` 줄로 출력). README의 "PPT는 임시 사본을 연다"는 틀린 문장을 고쳤다(원본을 읽기 전용으로 연다).
- M1: `New-IsolatedCopy.ps1`도 앱이 돌고 있으면 거부하고(11), 원본을 복사 전후로 해시해 바뀌면 30, 읽을 수 없으면 31로 끝난다.
- M2: 해시는 폴더도 포함하고 속성까지 본다. `%APPDATA%\Axicultuss`, `%APPDATA%\Axicultus`, 실제 루트의 `Admin`은 파일 내용(SHA-256)까지, 나머지는 목록 방식. 어느 방식인지 출력한다. 읽을 수 없는 것이 있으면 `UNREADABLE`이고 실패(31)다.
- M3: 드라이버가 사용자 키·마우스 단추 입력을 앱 창에 전하지 않는다(`IMessageFilter`, `[INPUT-DROPPED]`). 대화상자(`#32770` 창, 또는 본창이 1초 넘게 비활성)가 뜨면 0.1초 안에 그것을 비활성화하고 제목과 글을 `[MODAL]`로 남긴 뒤 답하지 않고 끝낸다(6).
- 작은 것 1~10: 경로를 앱처럼 풀어 비교(변수, `/`, `\\?\`, `..`, 8.3 이름). 드라이버 로그 경로 확인. 종료 코드를 나누고(40 시간 초과, 41 스크립트 오류) README에 전부 적음. `GetFolderPath` 사용과 없는 필수 폴더 거부(16). 하네스 루트의 위쪽 폴더까지 연결 지점 확인. 지우기·미러 전에 하네스 폴더 안의 읽기 전용 속성 해제. `EnableUnsafeBinaryFormatterSerialization`. 바꾼 설정만 새로 씀. 레지스트리 키 셋. README 보강.
- 실행하지 않고 확인한 것(스크래치 폴더만): 쓰기 거부 13가지(하네스 루트 위쪽의 연결 지점, 두 번 쓰기 포함)가 모두 거부됐고 연결 지점 너머 파일은 그대로였다. 해시: 새 빈 폴더, 속성만 바꾼 것, 크기와 시각을 되돌린 내용 변경(내용 방식만 잡는다. 목록 방식의 알려진 한계), 읽기 거부 폴더 → `UNREADABLE`. 경로 표기 `C:/…`, `%SystemDrive%\…`, `C:\AXICUL~1\…`(이 PC에 있는 짧은 이름), 스크래치 폴더의 짧은 이름, `\\?\`, `..`, 앞 글자가 붙은 값(`PC:/…`)이 PowerShell 함수와 드라이버의 C# 규칙(`Guards.cs`를 스크래치 콘솔 프로젝트로 컴파일해 시험) 양쪽에서 같게 풀렸다. 스크래치 사본에서 찾은 폴더: `D:\예배자료`, `D:\OneDrive - 산소망교회`, `C:\EasiSlides`, `C:\Axicultus`, 변수 경로, 다른 프로필(`C:\Users` 전체가 아니라 그 아래 폴더). 읽기 전용 파일이 든 사본 지우기·미러. 실제 폴더는 읽기만 해 보았다: 실제 설정의 경로 바꾸기(5개, 남는 실제 경로 0), 실제 `Admin`의 파일에서 찾은 폴더, 실제 폴더 해시(내용 방식 `Admin` 0.2초, `%APPDATA%\Axicultus` 2초, `D:\예배자료` 목록 0.2초, 모두 읽기 성공).
- 드라이버 빌드 오류 0, 스크립트 셋 구문 오류 0.

### Phase 0 2차 리뷰 반영 (구현자, 2026-11-28)

- N1: `New-IsolatedCopy.ps1`은 이전 표식을 맨 먼저 지우고, 새 표식은 복사 전후 비교가 통과한 뒤 마지막에 쓴다(30·31·41로 끝난 사본에는 표식이 없다).
- N2: `Run-Harness.ps1`은 드라이버를 띄운 뒤의 처리를 try/finally로 묶었다. 오류·Ctrl+C에도 비교, `LEFTOVER PROCESSES`, `LEFTOVER POWERPNT`가 나오고, 비교 자체가 실패하면 `NOT VERIFIED`(32), 드라이버가 살아 있으면 `DRIVER STILL RUNNING #pid`. 실행 전 해시가 이미 `UNREADABLE`이면 실행하지 않는다(31).
- N3: `*.esp`·`*.esi`도 읽고, 네트워크 경로(`\\서버\공유`)를 찾는다. 네트워크 경로와 네트워크 드라이브는 파일 시스템을 건드리지 않고 `not checked (network path)`로 보여 준다. 실제 `Admin`에서 `\\Mac\Home`이 새로 나왔다.
- N4: `AppData\Local\Microsoft`·`Temp`·`Packages`, `AppData\Roaming\Microsoft`는 시스템 폴더처럼 `not checked`.
- N5: 드라이버가 본창과 모든 컨트롤의 `AllowDrop`을 끈다(만든 뒤, 뜬 뒤, 기다리는 동안 0.5초마다). README에 끌어 놓기와 UI 자동화 도구 금지를 적었다.
- 작은 것: 단어 안의 드라이브 글자는 경로로 보지 않는다(`http://`는 경로 아님, `file:///C:/…`는 `C:\…`, 한 글자 종류 앞 글자 `PD:\`는 글 첫머리나 단어가 아닌 글자 뒤에서 허용). `Axicultuss.runtimeconfig*.json`이 남아도 13. `dotnet build`에만 Path를 바꾸고 되돌린다.
- 확인(스크래치만): 위 경로 경우들을 PowerShell 함수와 드라이버의 C# 규칙 양쪽에서 시험, 네트워크 경로의 맨 위 폴더와 건너뜀, 시스템 폴더 건너뜀, `.esp`·`.esi` 읽기, 이전 쓰기 거부 13가지 재시험 통과. 표식 순서는 스크립트 줄 순서로 확인(함수가 아니라 스크립트 본문이라 실행 없이는 시험할 수 없다). 드라이버 빌드 오류 0, 출력 폴더에 앱의 exe·runtimeconfig 없음.

### 첫 실행 (총괄 실행, 2026-11-28)

- `New-IsolatedCopy.ps1` 종료 0, 모든 원본 unchanged, 출력은 예상대로.
- `Run-Harness.ps1 -Phases smoke -Name smoke1` 종료 0: 드라이버 exit=0(pass), 모든 폴더와 레지스트리 키 unchanged. `LEFTOVER POWERPNT #16392 started 14:01:24`(몇 초 뒤 스스로 끝남). FIRSTCHANCE 1건(닫을 때 `NetOffice.Core.TryLoadAssembly`의 `NullReferenceException`, 알려진 무해한 것). 로그: 루트가 `iso\work` 아래, 예배 순서 '01.주일예배' 23개 항목, 시작할 때의 `SaveWorshipList`가 사본에 씀, 본창 dispose, 처리되지 않은 예외 0. 드라이버 시간 약 9초.

### `/code-review` 반영 (구현자, 2026-11-28)

- 1: 앱·드라이버와 PowerPoint 검사를 `Process.Start` 바로 앞에서 다시 한다(11, 15). 그때의 POWERPNT PID를 기록하고, 실행 뒤에는 그때 없던 것만 `LEFTOVER POWERPNT`로 보여 준다.
- 2: 사본이 가리키는 폴더의 맨 위 폴더가 저장소(드라이버의 bin)를 품으면 그 전체를 비교하지 않고, 그 경로가 든 폴더를 비교한다. 그 폴더도 저장소와 겹치면 `not checked (it holds the harness's own files …)`. 스크래치 시험: 저장소 밖 덱 폴더는 비교, 저장소 안 덱은 이유와 함께 제외, `D:\예배자료`는 그대로 비교.
- 3: smoke의 대기 단계에서 무엇이 실패해도 본창을 닫는다. 실패는 `[HARNESS-ERROR]`로 남기고 종료 코드 1. 뜬 직후의 끌어 놓기 끄기도 같은 방식.
- 4: `%` 변수가 든 값은 펼친 값 전체를 경로로 읽고(다시 쓰기는 값 전체를 바꿈), 펼친 글 안의 경로도 찾아 거부한다. PowerShell과 드라이버 양쪽. 스크래치 시험: `%AXI_MEDIA%` → `C:\Axicultuss\Media`를 찾고, 다시 쓰고, 거부함. 목록 변수 안의 8.3 경로도 거부.
- 5: 드라이버의 경로 정리에서 드라이브 루트는 `C:\`로 남는다(스크래치 C# 시험).
- 6: 멈춤 감시도 끝내기 전에 앱 로그를 하네스 로그로 옮긴다.
- 7: `Stop-Guard`는 `Write-Host`로 쓴다. 스크래치 시험: 값으로 받는 호출 안에서도 메시지가 보이고 종료 코드가 맞다.
- 8: 프로세스 검사에 WPF 하네스의 드라이버 `Axicultus.Tests`를 넣었다.
- 9: 본창을 만드는 동안(본창 창이 아직 없을 때) 프로세스의 어떤 창이든 보이면 대화상자로 보고 0.1초 안에 끝낸다. 실행 중에는 본창이 꺼지고 다른 창이 보이면 0.1초, 다른 창 없이 본창만 꺼지면 1초. 닫는 동안은 Windows 대화상자만. README의 시간을 이것과 맞췄다.
- 10: 스크립트 주석의 한글 폴더 이름을 영어 설명으로 바꿨다(코드의 문자열은 그대로).
- 드라이버 빌드 오류 0, 스크립트 구문 오류 0, 이전 스크래치 시험(쓰기 거부 13가지, 해시, 복사·미러·지우기, 경로 표기) 모두 다시 통과.

총괄 검증 — Phase 0 (2026-11-28):

- 적대적 리뷰(별도 에이전트) 두 번. 1차 판정은 "실행하지 말 것": 실행 전후 비교가 사본이 가리키는 실제 폴더(`D:\예배자료`, `C:\EasiSlides`, `C:\Axicultus`)를 보지 않음, 비교가 빈 폴더·속성·내용 변경을 놓치고 읽지 못한 폴더를 건너뜀, 격리 사본 스크립트에 실행 중인 앱 검사 없음, 실행 중 사용자 입력(Delete → 예 → 실제 PPT가 휴지통으로)이 닿을 수 있음. 모두 고쳤다. 2차 판정은 "실행해도 됨"(치명·주요 0), 사소 6건도 고쳤다(표식은 검사 통과 뒤에만, 드라이버가 뜬 뒤에는 비교가 항상 출력, 끌어 놓기 끔 등).
- `/code-review`(하네스 폴더): 10건 모두 고쳤다(실행 직전 보호 검사 다시, 하네스 자신의 출력이 든 폴더는 좁혀서 검사, 보고 중 예외에도 본창 닫기, 환경 변수만 있는 설정 값, 드라이브 루트, 멈춤 때 앱 로그, 보호 검사 글이 삼켜지던 것, WPF 하네스 드라이버 이름, 본창 만들기 중의 대화상자, 주석의 한글).
- 총괄이 스크립트 세 개를 직접 읽었다: 쓰기·삭제·미러 대상은 모두 `Assert-HarnessTarget`을 거친 하네스 폴더이고 실제 폴더는 복사 원본으로만 쓰인다.
- 총괄이 직접 실행(모니터 2대, PowerPoint 없음, 실행 전 사용자에게 알림): `New-IsolatedCopy.ps1` 두 번 모두 종료 코드 0, 실제 폴더 unchanged, 87.7MB·325개 파일 복사. `Run-Harness.ps1 -Phases smoke` 두 번(`smoke1.log`, 수정 뒤 `smoke2.log`) 모두 종료 코드 0: 드라이버 pass, 실제 폴더 20곳과 레지스트리 키 3개 unchanged, 처리되지 않은 예외 0. 로그로 사본에서 뜬 것을 확인했다(루트 `iso\work`, 예배 순서 01.주일예배 23개 항목, 시작할 때의 예배 순서 저장이 사본에 쓰임).
- 남는 것: 실행 뒤 숨은 PowerPoint 하나가 `LEFTOVER POWERPNT`로 보고되고 몇 초 뒤 스스로 끝난다(두 번 모두). 끝나기 전에 다시 실행하면 보호 검사 15가 거부한다. first-chance 1줄은 닫을 때의 `NetOffice.Core.TryLoadAssembly` NullReferenceException으로 WPF 하네스에서도 보던 것이다.
- 이 PC의 사본 설정에서 회중 화면은 주 모니터(`\\.\DISPLAY2`, 3440×1440) 쪽으로 잡힌다(설정의 모니터 이름이 이 PC의 번호와 다르다). 쇼를 띄우는 Phase 2b 하네스 단계는 이것을 감안한다.
