# Axicultus 실행 검증 하네스

WPF 운영 콘솔(`Axicultus/`)을 **격리한 프로필과 데이터 사본**으로 실행해 창, 메뉴, 명령, 단축키, 라이브 송출, PowerPoint 연동을 자동으로 점검한다. 예외와 바인딩 오류를 모두 로그에 남긴다.

실제 교회 데이터가 있는 PC에서 쓰도록 만들었다. 실제 프로필과 데이터는 읽기만 한다.

## 안전 규칙

`Run-Harness.ps1`은 다음 경우 앱을 실행하지 않는다. 이 검사를 풀거나 우회하지 않는다.

- 모니터가 2대 미만이다.
- Axicultus, Axicultuss, Easislides가 이미 실행 중이다.
- 격리 사본이 없다(`.axicultus-isolated` 표식 파일 기준).
- 하네스 빌드가 실패했다.

`Run-OwnPowerPoint.ps1`은 PowerPoint가 이미 실행 중이면 실행하지 않는다. `ppt2` 단계는 PowerPoint 프로세스를 강제 종료하므로, 작업 중인 PowerPoint가 있으면 PowerPoint 단계를 돌리지 않는다.

실행 전후로 `%APPDATA%\Axicultus`, `C:\Axicultus\Admin`, `C:\EasiSlides\Admin`의 파일 목록 해시를 비교해 출력한다. `REAL DATA CHANGED`가 나오면 원인을 찾기 전에는 다시 실행하지 않는다.

## 사용법

```powershell
# 1. 격리 사본 만들기 (실제 데이터가 바뀌면 다시 실행)
tools\AxicultusHarness\New-IsolatedCopy.ps1

# 2. 단계를 골라 실행
tools\AxicultusHarness\Run-Harness.ps1 -Phases 'tabs,windows,queue' -Name 'check1'

# 3. 전체 회귀 (약 25분)
tools\AxicultusHarness\Run-All.ps1
```

사본, 실행 상태, 로그는 `%LOCALAPPDATA%\AxicultusHarness`에 둔다(`-Root`로 변경). 로그는 `logs\<Name>.log`다. 매 실행은 `iso.pristine`을 `iso`로 복사한 뒤 시작하므로 이전 실행의 흔적이 남지 않는다.

로그에서 먼저 볼 것: `[UNHANDLED-UI]`, `[UNHANDLED-DOMAIN]`, `[SCENARIO-ABORT]`, `[HANG]`, `[BINDING]`, `[FIRSTCHANCE]`.

## 단계

| 단계 | 내용 |
|---|---|
| `tabs`, `windows` | 모든 탭과 창 열기 |
| `queue` | 예배 순서의 모든 항목 선택 |
| `menu`, `commands`, `shortcuts` | 주 메뉴 전체, 뷰모델 명령 전체, 단축키 |
| `empty` | 빈 예배 순서에서 명령 실행 |
| `live`, `liveppt`, `live2` | 송출, PPT 송출, 블랙·지우기·포커스·모니터 없음 |
| `gap`, `trash`, `wlist` | 간격 화면, 휴지통, 예배 순서 저장과 복원 |
| `chords` | 저장된 코드(MSC) 표시 |
| `bible2` | 성경 본문 흐름(1개 버전, 2개 버전, 기본 글꼴과 작은 글꼴), 높은 절의 글꼴 축소 |
| `rows` | 실제 곡의 송출 줄 높이, 2개 언어 곡의 화면 분할(처음 세 화면) |
| `ppt2`, `ppt2hang`, `ppt3` | 실제 PowerPoint: 블랙·숨김, 강제 종료, 멈춤, 블랙 중 이동 |
| `pptown` | 운영자가 먼저 연 PowerPoint 보호 (`Run-OwnPowerPoint.ps1`로 실행) |
| `pptcaption` | 카메라 하단 자막 창: 송출 PPT 찾기, 송출 화면 따라가기, 악보 상단·하단, 글자색. 창 그림을 `logs\pptcaption-*.png`로 저장 |
| `consolelist` | 자막 창 최대화, 송출 화면 따라가기에서 창 안의 예배 순서로 1번 항목 송출 뒤 2번 항목을 골라 "송출": 송출 화면·LIVE 표시(본창·자막 창)·자막이 2번 항목인지, 자막 창·본창 예배 순서에서 항목 더블클릭은 그 항목을 송출하고 빈 곳 더블클릭은 아무것도 송출하지 않는지, 고른 항목과 다른 항목의 더블클릭(Shift·Ctrl)은 더블클릭한 항목을 송출하는지, 본창 곡 목록(라이브러리)에서 항목 더블클릭은 그 곡을 예배 순서에 추가하고 빈 곳 더블클릭은 추가하지 않는지, 직접 입력("환영합니다" 송출 중)에서 2번 항목을 "송출": 송출 화면은 2번, 자막은 그대로인지, 최소 크기 창에서 "송출" 뒤 왼쪽 칸 스크롤 위치가 그대로이고 입력 칸에 포커스가 가는지, 입력 칸이 3줄을 보이고 4줄째는 스크롤하는지(보통·시니어 크기). 결과는 `[CONSOLELIST] PASS/FAIL`, 창 그림 `logs\consolelist-*.png`. 예배 순서가 5항목 미만이면 FAIL로 끝낸다(송출 화면을 열지 않음) |
| `ppttext` | 자막 창 자동 표시: 예배 순서의 PPT와 `AXI_A036_DECKS`(세미콜론으로 구분한 경로, 격리 작업 폴더 `a036`에 복사해 읽음)를 슬라이드별로 따라가며 전체 글자·악보 줄 기록, 1장 상자가 없는 슬라이드의 빈 자막 수 |
| `editorscan` | 모든 곡을 편집기로 열어 수정 없는 저장 결과 비교 |
| `bodyoffset`, `splitter` | 본문 세로 위치, 왼쪽 열 너비 |

## 구조

어셈블리 이름이 `Axicultus.Tests`다. 제품의 `InternalsVisibleTo`를 쓰기 위해서이며, 같은 이름의 테스트 프로젝트와 함께 빌드할 수 없어 `Axicultus.sln`에 넣지 않았다. 제품의 `Startup` 처리기를 거치지 않고 `App.xaml` 리소스만 읽은 뒤 격리 경로로 `App.ConfigureServices`를 호출한다. 전역 키 후킹은 시작하지 않는다.
