# Axicultuss 실행 검증 하네스

WinForms 앱 Axicultuss(`Axicultuss/`)를 **격리한 설정과 데이터 사본**으로 실행해 본창을 점검한다. 단계, 처리되지 않은 예외, 첫 예외(first-chance), 앱의 콘솔·Trace 출력, 앱 자신의 `Axicultus_Debug.log`에 이번 실행이 더한 줄을 모두 한 로그에 남긴다. WPF 앱용 하네스(`tools\AxicultusHarness`)와 같은 짜임이다.

실제 교회 데이터가 있는 PC에서 쓰도록 만들었다. 실제 설정과 데이터는 읽기만 한다. **PowerShell 7.4 이상(`pwsh`)이 필요하다.**

## 안전 규칙

### 실행을 막는 검사

`Run-Harness.ps1`은 아래 검사를 이 순서로 하고, 하나라도 걸리면 아무것도 실행하지 않는다. 이 검사를 풀거나 우회하지 않는다.

| 순서 | 조건 | 종료 코드 |
|---|---|---|
| 1 | 모니터가 2대 미만 | 10 |
| 2 | `Axicultuss`, `Axicultus`, `Easislides`, 또는 이 하네스나 WPF 하네스의 드라이버(`Axicultuss.Tests`, `Axicultus.Tests`)가 실행 중 | 11 |
| 3 | PowerPoint(`POWERPNT`)가 실행 중. 앱은 시작할 때 PowerPoint에 붙으므로, 운영자의 PowerPoint에 붙지 않게 한다 | 15 |
| 4 | 격리 사본이 없음(`iso.pristine\profile\.axicultuss-isolated` 표식 기준) | 12 |
| 5 | 드라이버 빌드 실패, 또는 빌드 뒤 출력 폴더에 앱의 `Axicultuss.exe`나 `Axicultuss.runtimeconfig*.json`이 남음 | 13 |
| 6 | 반드시 있어야 할 실제 폴더(`%APPDATA%\Axicultuss`, 실제 데이터 루트)나 시스템 폴더가 없음 | 16 |

드라이버도 본창을 만들기 **전에** 스스로 확인하고, 하나라도 어긋나면 `[ABORT]`를 남기고 종료 코드 2로 끝난다.

- 로그 파일이 하네스 `logs` 폴더 안이다(아니면 로그도 쓰지 않고 끝난다).
- 설정 파일이 `%LOCALAPPDATA%\AxicultussHarness\iso\profile` 안에 있고, `AXICULTUSS_SETTINGS_FILE`이 그 파일이며, 앱의 설정 저장소(`RegUtil.Profile`)가 실제로 그 파일로 만들어졌다.
- 설정 파일 옆에 표식 파일이 있다.
- 앱이 읽는 `config\root_directory`가 `iso\work` 안에 있고, 그 폴더와 곡 DB가 있다(없으면 앱이 작업 폴더를 묻거나 DB 설치를 묻는다).
- 설정의 어떤 문자열도 실제 데이터 루트나 실제 프로필로 풀리지 않는다. 경로는 앱처럼 풀어서 비교한다: 환경 변수(`%SystemDrive%`), `/`, `\\?\`, `..`, 8.3 짧은 이름(`C:\AXICUL~1`). `%` 변수가 든 값은 펼친 값 전체도 경로로 읽는다(`%AXI_MEDIA%` 하나만 있는 값이 `C:\Axicultuss\Media`를 가리켜도 걸린다).
- 임시 폴더(`%TEMP%`)가 하네스 폴더 안이다(`Run-Harness.ps1`이 드라이버 프로세스에만 바꾼다).
- 앱 어셈블리를 드라이버 폴더에서 읽었다. 앱은 `Axicultus_Debug.log`를 자기 어셈블리 옆에 쓰므로 이 로그는 드라이버의 `bin` 폴더에 생기고 `C:\Axicultuss`에는 생기지 않는다.

### 실행 중: 손대지 않는다

실행하는 동안 키보드와 마우스로 앱 창을 건드리지 않는다. 탐색기에서 앱 창으로 파일을 끌어 놓지 않고, UI 자동화 도구(agent-desktop, 화면 읽기 프로그램 등)를 하네스 창에 쓰지 않는다. 드라이버는 끌어 놓기를 막으려고 본창과 그 안의 모든 컨트롤의 `AllowDrop`을 끈다(만든 뒤, 뜬 뒤, 그리고 기다리는 동안 0.5초마다 다시). 끌어 놓기는 OLE로 오므로 아래의 입력 버리기에 걸리지 않고, 예배 순서에 놓으면 파일이 추가되고 PPT가 원래 자리에서 열리기 때문이다. UI 자동화는 메시지를 거치지 않고 컨트롤을 직접 누를 수 있어 막을 수 없다. 드라이버는 사용자의 키와 마우스 단추 입력(휠, 제목 줄 클릭, 작업 표시줄의 "창 닫기" 포함)을 앱 창에 전하지 않고 버린다(종류마다 한 번 `[INPUT-DROPPED]` 줄). 예: PowerPoint 목록에서 Delete를 누르면 기본값이 "예"인 확인 창 뒤에 실제 PPT 파일이 휴지통으로 간다. 버리는 것은 앱의 메시지 루프를 거치는 입력뿐이고 마우스 이동은 그대로다.

대화상자가 뜨면(앱의 `MessageBox`, 파일 대화상자 같은 Windows 대화상자, 본창을 막는 WinForms 모달 창) 드라이버는 0.1초 안에 그것을 찾아(본창을 만드는 동안에는 프로세스의 어떤 창이든 보이면 대화상자로 본다. 예: 작업 폴더를 묻는 창. 다른 창 없이 본창만 꺼진 경우는 1초 뒤) 비활성화하고(입력으로 답할 수 없게), 제목과 글을 `[MODAL]` 줄로 남긴 뒤, **답하지 않고** 프로세스를 끝낸다(종료 코드 6). 답이 주어지지 않으므로 대화상자 뒤의 동작(삭제, DB 교체 등)은 일어나지 않는다. `smoke`에서 대화상자가 뜨는 것은 실패다. 이렇게 끝나면 앱의 닫기 처리(창 위치 저장, PowerPoint 정리)는 돌지 않는다. 사본의 설정만 덜 저장되고, 숨은 PowerPoint가 남을 수 있다(아래 `LEFTOVER POWERPNT`).

### 실행 전후 비교

실행 전후로 아래를 해시해 항목마다 `unchanged:`, `REAL DATA CHANGED:`, `UNREADABLE`을 출력한다. 실행 전에 `check:` 줄로 목록과 방식을 먼저 보여 준다.

- **내용(content)**: 폴더와 파일 목록, 크기, 수정 시각, 속성, 그리고 파일마다 SHA-256. 대상: `%APPDATA%\Axicultuss`, `%APPDATA%\Axicultus`, 실제 데이터 루트의 `Admin`(설정 템플릿, DB, 예배 순서. 이 PC에서 약 74MB).
- **목록(metadata)**: 폴더와 파일 목록, 크기, 수정 시각, 속성. 대상:
  - 실제 데이터 루트의 데이터 폴더 각각과 실제 데이터 루트 전체(실행 파일 옆의 파일 포함)
  - 있는 보호 폴더 전부: `C:\Axicultuss`, `C:\Axicultus`, `C:\EasiSlides`, `D:\예배자료`(이 PC에서 21GB, 3242개), `D:\OneDrive - 산소망교회`(이 PC에는 없음)
  - 사본이 가리키는 사본 밖의 실제 폴더 전부. 사본의 설정, JSON 파일(예배 순서, 찬양집), `*.esw`·`*.est`·`*.esp`·`*.esi`·`*.xml`·`*.txt`·`*.ini`에 있는 경로를 실행할 때마다 모아 맨 위 폴더(드라이브 + 첫 폴더, 사용자 프로필 아래면 그 아래 첫 폴더, 네트워크 경로면 `\\서버\공유`)로 줄인다. 웹 주소(`http://…`)는 경로로 보지 않고, `file:///C:/…`는 `C:\…`로 읽는다. 이 PC에서는 `D:\예배자료`, `D:\OneDrive - 산소망교회`, `C:\EasiSlides`, `C:\Users\Admin\Desktop`, `X:\...pptx`, `\\Mac\Home`이 나온다.
  - 맨 위 폴더가 하네스 자신(저장소, 드라이버의 `bin`에서 앱의 `Axicultus_Debug.log`가 실행마다 자란다)을 품으면 그 폴더 전체는 비교하지 않고, 대신 그 경로가 든 폴더를 비교한다. 그 폴더도 저장소와 겹치면 `not checked (it holds the harness's own files …)`로 보여 준다.
  - 비교하지 않고 `not checked (…)`로 보여 주는 것: 시스템이 관리하는 폴더(Windows, Program Files, ProgramData, 임시 폴더, `AppData\Local\Microsoft`, `AppData\Local\Temp`, `AppData\Local\Packages`, `AppData\Roaming\Microsoft`. 저절로 바뀌고 교회 데이터가 없다)와 네트워크 경로·네트워크 드라이브(닿지 않는 서버가 실행을 멈추게 할 수 있다. 이 PC의 `\\Mac\Home`). 네트워크 경로의 파일은 비교하지 않는다는 뜻이다.
- **레지스트리**: `HKCU\Software\Axicultuss`, `HKCU\Software\Axicultus`, `HKCU\Software\EasiSlides`. 앱은 레지스트리에 쓰지 않는다. 설정 파일이 없을 때만 이 키들을 읽어 설정을 옮긴다.

없는 폴더는 `MISSING`으로 나와 빈 폴더와 구별된다(없다가 생기면 바뀐 것). 읽을 수 없는 폴더나 파일이 하나라도 있으면 `UNREADABLE`이고 실패로 다룬다(종료 코드 31). `unchanged`로 보지 않는다. 실행 전에 이미 읽을 수 없으면 실행하지 않는다(31).

앱·드라이버와 PowerPoint 검사(11, 15)는 드라이버를 띄우기 직전에 한 번 더 한다(빌드, 해시, 미러가 시간이 걸리므로). 실행 뒤 `LEFTOVER POWERPNT`는 그때 없던 PowerPoint만 보여 준다. 드라이버를 띄운 뒤에는 무슨 일이 있어도(스크립트 오류, Ctrl+C 포함) 실행 뒤 비교, `LEFTOVER PROCESSES`, `LEFTOVER POWERPNT`를 출력한다. 비교 자체가 실패하면 `NOT VERIFIED`(종료 코드 32)다. 스크립트가 중단돼 드라이버가 아직 돌고 있으면 `DRIVER STILL RUNNING #<pid>`를 출력한다(끝내지 않는다).

격리 사본의 표식은 `New-IsolatedCopy.ps1`이 마지막에, 복사 전후 비교가 통과한 뒤에만 쓴다. 이전 표식은 맨 먼저 지운다. 그래서 중간에 실패한 사본에는 표식이 없고 `Run-Harness.ps1`이 거부한다(12).

**비교하지 않는 것**: PowerPoint 자신의 레지스트리와 최근 파일 기록(`HKCU\Software\Microsoft\Office`, 최근 문서 목록), 사용자 임시 폴더, 목록 방식 폴더에서 크기와 수정 시각을 그대로 둔 채 내용만 바뀐 경우. PowerPoint는 예배 순서의 PPT를 원본 그대로 읽기 전용으로 연다(그 파일이 이미 PowerPoint에 열려 있을 때만 임시 사본을 연다). 그래서 `D:\예배자료`에 Office 소유자 파일(`~$...`)이 잠깐 생기면 목록 비교에 걸린다.

`REAL DATA CHANGED`가 나오면 원인을 찾기 전에는 다시 실행하지 않는다.

### 실제 폴더가 쓰기 대상이 되지 않게 하는 방법

모든 쓰기는 `HarnessSafety.ps1`의 `Assert-HarnessTarget`을 거친다. 대상이 하네스 폴더(`%LOCALAPPDATA%\AxicultussHarness`, 앱처럼 `GetFolderPath`로 찾는다) 아래가 아니거나, 대상이나 그 위의 어느 폴더가 연결 지점(junction, symlink)이거나, 보호 폴더(`%APPDATA%\Axicultuss`, `%APPDATA%\Axicultus`, `C:\Axicultuss`, `C:\Axicultus`, `C:\EasiSlides`, `D:\예배자료`, `D:\OneDrive - 산소망교회`, 설정이 가리키는 실제 데이터 루트)와 겹치면 멈춘다. 실제 경로는 복사의 원본으로만 쓴다. 격리 사본은 비어 있는 새 폴더에 복사하므로 `/MIR`·`/PURGE`가 없다. 설정은 경로를 바꾼 글만 새 파일로 쓴다(실제 루트를 가리키는 원본 그대로의 사본은 하네스 폴더에 한 번도 생기지 않는다). 지우는 것은 검사를 거친 하네스 폴더뿐이고, 그 안에 연결 지점이 하나라도 있으면 지우지 않으며, 지우기 전에 그 안의 읽기 전용 속성만 푼다. 복사는 `/XJ`로 연결 지점을 따라가지 않는다.

빌드할 때마다 `Harness.csproj`가 출력 폴더에서 참조 때문에 따라온 앱의 `Axicultuss.exe`, `Axicultuss.runtimeconfig.json`, `Axicultuss.runtimeconfig.dev.json`을 지운다. 그 파일들로는 앱이 실제 설정으로 뜨기 때문이다. 출력 폴더의 실행 파일은 드라이버(`Axicultuss.Tests.exe`)뿐이다.

## 사용법

```powershell
# 1. 격리 사본 만들기 (실제 데이터가 바뀌면 다시 실행). 앱이 돌고 있으면 거부한다.
pwsh -NoProfile -File tools\AxicultussHarness\New-IsolatedCopy.ps1

# 2. 실행 (창이 뜬다는 것을 사용자에게 먼저 알린다. 실행 중에는 손대지 않는다)
pwsh -NoProfile -File tools\AxicultussHarness\Run-Harness.ps1 -Phases smoke -Name smoke1
```

사본, 임시 폴더, 로그는 `%LOCALAPPDATA%\AxicultussHarness`에 둔다(바꿀 수 없다).

- `iso.pristine\profile\settings.json`: 실제 설정의 사본. 실제 데이터 루트로 풀리는 모든 경로(`root_directory`, `export_dir`, `import_dir`, `praiseoutput_dir`, `media_dir`, 어떤 표기든)를 `iso\work` 아래로 바꾼다. `D:\예배자료` 같은 루트 밖 경로(PowerPoint 원본 폴더, 최근 편집 목록)는 그대로 둔다. 앱은 그 파일을 읽기만 한다. 예배 순서 파일 안의 경로도 그대로다(위의 비교 대상).
- `iso.pristine\work\<폴더>`: 데이터 폴더 `Admin`, `Backgrounds`, `Documents`, `HolyBibles`, `Html`, `Images`, `InfoScreens`, `Media`, `Music`, `Powerpoint`, `User Images` 중 실제로 있는 것. 실행 파일, dll, `ko`, `runtimes`, `win-x86`은 복사하지 않는다. 이 PC에서 약 88MB(`Admin` 74MB, `HolyBibles` 10MB, 나머지 그림).
- `iso`: 실행마다 `iso.pristine`을 미러링한 것. 이전 실행의 흔적이 남지 않는다.
- `temp`: 드라이버 프로세스의 `%TEMP%`·`%TMP%`. 앱의 세션 폴더(PPT 장 그림), 외부 PPT 미리보기 캐시, PPT 임시 사본이 실제 앱과 함께 쓰는 사용자 임시 폴더 대신 여기에 생긴다. 실행마다 비운다.
- `logs\<Name>.log`: 드라이버 로그.

로그에서 먼저 볼 것: `[ABORT]`, `[MODAL]`, `[HARNESS-ERROR]`, `[UNHANDLED-UI]`, `[UNHANDLED-DOMAIN]`, `[UNOBSERVED-TASK]`, `[HANG]`, `[INPUT-DROPPED]`, `[FIRSTCHANCE]`. `[GUARD]` 줄은 드라이버가 확인한 경로, `[SMOKE]` 줄은 본창이 보여 준 것(제목, 예배 순서 이름과 항목 수, 모니터, 데이터 루트), `[APPLOG]` 줄은 앱 자신의 `Axicultus_Debug.log`에 이번 실행이 더한 줄이다. `[CONSOLE]`·`[TRACE]`는 앱 자신의 진단 출력이다.

실행 뒤 `LEFTOVER POWERPNT #<pid> started <시각>`이 나오면 이번 실행이 띄운 숨은 PowerPoint가 남은 것이다(실행 전에 PowerPoint가 없었으므로). 하네스는 끝내지 않는다. 확인한 뒤 작업 관리자로 끝낸다.

## 종료 코드

`Run-Harness.ps1`:

| 코드 | 뜻 |
|---|---|
| 0 | 통과 |
| 1 | 처리되지 않은 UI 예외나 하네스 자체의 오류(`[HARNESS-ERROR]`)가 로그에 있음(드라이버. 하네스 오류가 나도 본창은 닫는다) |
| 2 | 드라이버의 격리 확인이 거부함 |
| 3 | 멈춤: 120초 동안 단계가 없음(드라이버) |
| 4 | 본창이 시작하지 못함(드라이버) |
| 5 | 모르는 단계(드라이버) |
| 6 | 대화상자가 떠서 답하지 않고 끝냄(드라이버) |
| 10 | 모니터 2대 미만 |
| 11 | 앱이나 드라이버가 실행 중 |
| 12 | 격리 사본(표식)이 없음 |
| 13 | 드라이버 빌드 실패, 또는 출력 폴더에 앱의 exe나 runtimeconfig가 남음 |
| 14 | `iso.pristine` → `iso` 미러 실패 |
| 15 | PowerPoint가 실행 중 |
| 16 | 있어야 할 실제 폴더나 시스템 폴더가 없음 |
| 20 | 쓰기 대상 경로 검사가 거부함 |
| 30 | REAL DATA CHANGED |
| 31 | 실제 폴더를 읽을 수 없어 비교하지 못함(실행 전이면 실행하지 않음) |
| 32 | NOT VERIFIED: 실행 뒤 비교 자체가 실패함 |
| 40 | 드라이버가 제한 시간(`-TimeoutSeconds`, 기본 300초) 안에 끝나지 않아 끝냄 |
| 41 | 스크립트 오류(`SCRIPT ERROR:` 줄) |

`New-IsolatedCopy.ps1`: 0 준비됨, 11 앱이 실행 중, 16 시스템 폴더를 모름, 20 쓰기 대상 경로 검사 거부, 21 실제 설정 파일이 없음, 22 쓸 수 있는 `root_directory`가 없음, 23 복사 실패, 24 바꾼 설정이 아직 실제 폴더를 가리킴, 30 복사하는 동안 실제 데이터가 바뀜, 31 실제 폴더를 읽을 수 없음, 41 스크립트 오류.

## 단계

| 단계 | 내용 |
|---|---|
| `smoke` | 본창을 만들고 띄운다. 최소 5초, 그리고 시작할 때의 PowerPoint 연결(작업 스레드)이 끝날 때까지(최대 30초) 아무것도 하지 않고 기다린 뒤, 본 것을 기록하고 본창을 닫는다. 쇼 시작, PowerPoint 조작, 입력 주입, 목록 편집은 하지 않는다. 대화상자가 뜨면 실패(6) |

## 드라이버

`Harness.csproj` + `Program.cs`(단계) + `Guards.cs`(경로 풀기, 입력 버리기, 대화상자 감시). 어셈블리 이름이 `Axicultuss.Tests`다. 제품의 `InternalsVisibleTo("Axicultuss.Tests")`를 쓰기 위해서이며, 같은 이름의 테스트 프로젝트와 함께 빌드할 수 없어 `Axicultus.sln`에 넣지 않았다.

앱의 `Program.Main`을 거치지 않는다. 건너뛰는 것: 같은 이름의 다른 프로세스 끝내기(`OverlapProcessKill`), 창 없는 PowerPoint 끝내기, 맨 위에 남은 PowerPoint 쇼 창 내리기, 시작 화면. 똑같이 하는 것: 코드 페이지 등록, UI 스레드 예외 모드, 설정 파일 읽기(`RegUtil.Profile.Initialize`), 메뉴 언어, DPI 모드, 글자 렌더링, 시각 스타일. 그리고 전역 키 후킹 옵션(F7~F10, 화살표)을 이 실행 동안 끈다. 하네스가 도는 동안에도 PC를 쓰기 때문이다.

본창은 시작할 때 PowerPoint에 연결한다(`Gf.InitAppData`의 작업 스레드 넷, 본창의 `MainPPT.Init`). 이것은 앱 동작이라 드라이버가 막지 않는다. 그래서 PowerPoint가 이미 돌고 있으면 실행하지 않는다(검사 3). 앱이 띄운 숨은 PowerPoint는 닫을 때 앱이 정리한다.

## 구조

| 파일 | 내용 |
|---|---|
| `HarnessSafety.ps1` | 경로 검사와 풀기, 복사, 미러, 지우기, 설정 경로 바꾸기, 사본이 가리키는 실제 폴더 찾기, 해시. 두 스크립트가 함께 쓴다 |
| `New-IsolatedCopy.ps1` | 격리 사본 만들기(앞뒤로 원본 해시 비교) |
| `Run-Harness.ps1` | 보호 검사, 해시 비교, 드라이버 실행 |
| `Harness.csproj`, `Program.cs`, `Guards.cs` | 드라이버 |
