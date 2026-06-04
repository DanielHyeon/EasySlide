# FrmMain 기준 WPF UI/UX 정렬 문서

작성일: 2026-06-04  
대상: `Easislides.Wpf/MainWindow.xaml`, `Easislides/Easislides/FrmMain.*`

## 1. 결론

`Easislides.Wpf/MainWindow.xaml`의 최종 UI/UX 기준은 새롭게 재해석한 WPF 대시보드가 아니라, 기존 WinForms `FrmMain`의 운영 방식이어야 한다.

이 판단의 이유는 단순한 취향 문제가 아니다. EasiSlides는 예배나 행사 중 실제 송출을 다루는 도구이고, 이 환경에서는 보기 좋은 화면보다 운영자가 실수 없이 빠르게 조작할 수 있는 화면이 더 중요하다. 기존 `FrmMain`은 시각적으로 낡았더라도, 운영자가 필요한 기능을 한 화면에서 바로 찾고 바로 누르는 "운영 콘솔" 구조를 갖고 있다.

따라서 WPF 전환의 목표는 다음과 같이 잡는다.

> FrmMain의 정보 구조, 조작 흐름, 단축키, 즉시 접근성을 유지하면서 WPF의 바인딩, 명령, 스타일, 접근성, 고해상도 대응으로 정돈한다.

## 2. 현재 상황 요약

최근 WPF 앱 실행 확인 중 `MainWindow.xaml`의 `IsChecked` 바인딩에서 읽기 전용 상태 속성에 기본 `TwoWay` 바인딩이 걸려 디버그 알림이 발생했다. `TransitionKindIsSlideRight`, `TransitionKindIsSlideLeft`, `BackgroundModeIsCenter` 계열이 대표 사례였다.

이 문제는 read-only 상태 표시에는 `Mode=OneWay`, 사용자가 실제로 변경하는 옵션에는 `Mode=TwoWay`를 명시하는 방식으로 정리되었다. 이후 `EasislidesNext.exe`는 메인 창까지 뜨고, 디버그 알림 없이 실행되는 상태가 확인되었다.

하지만 실행 가능 여부와 UI/UX 적합성은 별개의 문제다. 현재 WPF `MainWindow.xaml`은 메뉴, LiveBar, 상태바, 좌측 브라우저, 예배 순서, 중앙 Preview, 우측 인스펙터 등 현대적인 구조를 갖추고 있으나, 기존 `FrmMain`과 비교하면 운영자가 익숙하게 쓰던 기능 배치와 조작 밀도가 달라졌다.

사용자 관점에서는 "기능이 있다"보다 "라이브 중 손이 기억하는 위치에 있다"가 더 중요하다. 이 문서는 그 기준을 고정한다.

## 3. 근거 파일

레거시 WinForms 기준:

- `Easislides/Easislides/FrmMain.cs`
- `Easislides/Easislides/FrmMain.Designer.cs`
- `Easislides/Easislides/FrmMain.Events.cs`
- `Easislides/Easislides/FrmMain.Fields.cs`
- `Easislides/Easislides/FrmMain.Layout.cs`
- `Easislides/Easislides/FrmMain.Logic.cs`
- `Easislides/Easislides/FrmMain.resx`

WPF 대상:

- `Easislides.Wpf/MainWindow.xaml`
- `Easislides.Wpf/MainWindow.xaml.cs`
- `Easislides.Wpf/Shell/MainViewModel.cs`
- `Easislides.Wpf/Composites/*`

관련 기존 문서:

- `docs/wpf-migration/frmmain-vs-wpf-detailed-gap.md`
- `docs/wpf-migration/frmmain-port-roadmap.md`
- `docs/ui-ux-modernization-plan.md`

## 4. FrmMain 방식이 더 편한 이유

### 4.1 운영 콘솔형 화면이다

`FrmMain`은 한 화면 안에 Preview, Output, 예배 순서, 라이브 제어, 절/슬라이드 이동, 배경, 폰트, 정렬, 전환, Black/Clear/Go Live 같은 현장 조작을 촘촘하게 배치한다.

이 구조는 일반적인 업무 앱 기준으로는 복잡해 보일 수 있다. 하지만 라이브 송출 앱에서는 오히려 장점이다. 운영자는 메뉴를 탐색하거나 패널을 열고 닫는 시간이 아니라, 현재 상태를 보고 즉시 조작하는 시간이 필요하다.

### 4.2 Preview와 Output 조작이 분리되어 있다

`FrmMain`에는 Preview 영역과 Output 영역이 각각 독립적으로 존재한다. Verse 버튼, 슬라이드 이동, 항목 이동, 송출 버튼이 Preview/Output 맥락에서 명확하게 나뉜다.

이 방식은 "다음에 보여줄 것"과 "이미 회중에게 나가고 있는 것"을 머릿속에서 분리하게 해준다. WPF에서도 이 분리는 반드시 유지해야 한다.

### 4.3 자주 쓰는 기능이 숨지 않는다

라이브 중 자주 쓰는 기능은 다음과 같다.

- Go Live
- 송출 후 다음 항목
- 이전/다음 항목
- 이전/다음 슬라이드
- 절/후렴/브릿지/엔딩 바로 이동
- Black
- Clear
- Hide
- Restore
- Restart current item
- Output refresh
- 배경 이미지/색상/표시 모드
- 가사 표시 옵션
- 이중 언어 영역 표시
- 코드 표시/조옮김

이 기능들이 메뉴, 명령 팔레트, 접힌 인스펙터, 모달 창 안에만 있으면 현장 운영성이 떨어진다. 보조 경로로 메뉴나 명령 팔레트를 두는 것은 좋지만, 주요 기능의 기본 경로는 항상 보여야 한다.

### 4.4 기존 단축키와 근육 기억이 자산이다

`FrmMain`을 오래 사용한 운영자는 버튼 위치, 메뉴 위치, F키, 숫자/문자 점프, Space 계열 동작을 몸으로 익힌 상태다. WPF 전환이 이 기억을 깨면 기능이 더 좋아져도 실제 현장에서는 느려질 수 있다.

WPF의 단축키 체계는 새로 설계하는 것이 아니라 기존 조작을 우선 보존하고, 새 기능은 그 위에 추가하는 방식이어야 한다.

## 5. 현재 WPF MainWindow의 장점

현재 WPF 화면에도 강점은 있다.

- `Menu`가 File/Edit/View/Output/Tools/Help 구조로 복원되어 있다.
- `LiveBar`와 상태바가 있어 현재 라이브 상태를 노출한다.
- 좌측에 라이브러리/성경/검색 브라우저와 예배 순서를 함께 두려는 방향이 있다.
- 중앙 Preview와 PowerPoint/Media 탭을 분리하려는 구조가 있다.
- 우측 인스펙터로 출력 모양을 편집하려는 방향이 있다.
- WPF 바인딩, Command, ViewModel, 테스트로 기능을 검증할 수 있는 기반이 있다.
- 최근 바인딩 모드 오류를 계기로 XAML 바인딩 품질 게이트를 추가할 수 있는 상태가 되었다.

즉 WPF 작업을 버릴 이유는 없다. 다만 현재 방향은 "현대적인 WPF 앱"으로만 가면 안 되고, "FrmMain식 운영 콘솔을 WPF로 정돈"하는 방향으로 재정렬되어야 한다.

## 6. WPF에서 지켜야 할 UX 원칙

### 6.1 기능 존재보다 즉시 접근성을 우선한다

기능이 메뉴에 있거나 명령 팔레트에서 검색 가능하다는 사실만으로는 충분하지 않다. 라이브 중 자주 쓰는 기능은 첫 화면에서 보여야 한다.

### 6.2 화면 밀도를 무조건 낮추지 않는다

현대 UI라고 해서 여백을 크게 잡고 카드를 많이 쓰면 운영 효율이 떨어질 수 있다. EasiSlides의 기본 화면은 SaaS 대시보드가 아니라 현장 콘솔이다. 밀도는 유지하되, 그룹핑, 정렬, 색상, 아이콘, 접근성 이름을 정돈하는 방식이 맞다.

### 6.3 FrmMain의 영역 모델을 먼저 보존한다

WPF `MainWindow`는 최소한 다음 영역을 명확히 가져야 한다.

- 상단 메뉴/명령 영역
- 항상 보이는 라이브 상태 영역
- 콘텐츠 브라우저 영역
- 예배 순서 영역
- Preview 영역
- Output 또는 현재 송출 상태 영역
- 절/슬라이드 직접 이동 영역
- Black/Clear/Hide/Restore/Live 제어 영역
- 서식/배경/전환 조작 영역
- 하단 상태 영역

### 6.4 Preview와 Output을 섞지 않는다

선택한 항목을 미리 보는 조작과 현재 송출을 바꾸는 조작은 시각적으로도, 명령으로도 구분되어야 한다.

### 6.5 위험 명령은 더 눈에 띄게, 더 예측 가능하게 둔다

Black, Clear, Hide, Close Output, Stop Live 같은 명령은 색상과 위치가 일관되어야 한다. 실수 방지를 위해 확인이 필요한 명령과 즉시 실행되어야 하는 명령을 구분한다.

### 6.6 바인딩 모드는 명시한다

이번 디버그 알림의 원인은 WPF 기본 바인딩 모드와 ViewModel 속성의 의도가 어긋난 것이다. 앞으로 XAML에서는 특히 `IsChecked`, `SelectedItem`, `SelectedValue`, `Text` 등 양방향 가능성이 있는 속성의 바인딩 모드를 명시한다.

- 상태 표시 전용: `Mode=OneWay`
- 사용자 입력으로 값을 바꾸는 옵션: `Mode=TwoWay`
- 명령으로 토글하고 상태만 표시하는 메뉴/토글: `Mode=OneWay` + `Command`

## 7. 목표 화면 방향

### 7.1 기본값은 Classic Operator Layout

WPF의 기본 첫 화면은 `FrmMain` 사용자에게 가장 덜 낯선 구조여야 한다. 필요하면 나중에 Modern Layout을 옵션으로 둘 수 있지만, 기본값은 Classic Operator Layout이 맞다.

Classic Operator Layout은 다음 특징을 가진다.

- 예배 순서가 항상 보인다.
- 라이브 제어 버튼이 항상 보인다.
- 절/후렴/브릿지/엔딩 점프가 항상 보인다.
- Preview와 Output 상태가 동시에 보인다.
- 콘텐츠 탐색과 송출 제어가 탭 전환 때문에 서로 가려지지 않는다.
- 자주 쓰는 출력 옵션은 인스펙터가 접혀도 접근 가능하다.

### 7.2 WPF 스타일은 보조 수단이다

WPF에서 개선할 대상은 다음이다.

- 레이아웃 정렬
- 고해상도 대응
- Fluent 아이콘
- 색상 토큰
- 접근성 이름
- 키보드 탐색
- 상태 표시
- 테스트 가능한 Command 구조

반대로 다음은 함부로 바꾸면 안 된다.

- 핵심 버튼의 위치 의미
- Preview/Output 분리
- 라이브 제어 흐름
- 기존 단축키
- 운영자가 한 화면에서 보는 정보량

## 8. 다음 할 일

### P0. FrmMain UI/UX 인벤토리 작성

목표: `FrmMain`의 실제 컨트롤, 메뉴, 버튼, 이벤트, 단축키를 빠짐없이 표로 만든다.

작업:

- `FrmMain.Designer.cs`의 주요 컨트롤을 영역별로 분류한다.
- `FrmMain.Events.cs`와 `FrmMain.Logic.cs`에서 각 컨트롤의 동작을 연결한다.
- Preview 전용, Output 전용, 공통, 설정, 라이브러리, 예배 순서, 미디어, 성경, InfoScreen으로 나눈다.
- 각 항목을 `유지`, `WPF식 개선`, `보조 패널 이동`, `보류`, `제거 금지`로 분류한다.

산출물:

- `docs/wpf-migration/inventory/frmmain-ux-control-map.md`

완료 기준:

- `FrmMain`의 주요 버튼/메뉴/단축키가 "WPF에서 어디에 대응되는지" 또는 "아직 없음"으로 명확히 표시된다.
- 라이브 중 사용하는 기능은 모두 P0/P1 우선순위를 받는다.

### P0. 현재 WPF MainWindow 대응표 작성

목표: `MainWindow.xaml`이 이미 제공하는 기능과 부족한 기능을 한눈에 본다.

작업:

- `MainWindow.xaml`의 메뉴, LiveBar, 상태바, 좌측 브라우저, 예배 순서, Preview 탭, Output 조작, 우측 인스펙터를 영역별로 표기한다.
- `FrmMain` 인벤토리와 1:1로 비교한다.
- "기능은 있지만 숨겨짐", "기능은 있으나 위치가 다름", "기능 없음", "기능은 있으나 조작 흐름이 다름"을 구분한다.

산출물:

- `docs/wpf-migration/inventory/mainwindow-ux-parity-map.md`

완료 기준:

- 사용자가 불편하다고 느끼는 차이가 감각적 표현이 아니라 구체적인 컨트롤/흐름 차이로 기록된다.

### P0. Classic Operator Layout 설계안 작성

목표: WPF 첫 화면을 어떤 구조로 재배치할지 결정한다.

작업:

- 1180x760 최소 창 크기 기준으로 첫 화면 영역을 정의한다.
- `FrmMain`의 Preview/Output/예배 순서/라이브 제어/절 점프 구조를 WPF 영역에 재배치한다.
- 우측 인스펙터에 둘 기능과 첫 화면에 고정할 기능을 구분한다.
- 메뉴/명령 팔레트에만 있어도 되는 기능과 절대 숨기면 안 되는 기능을 구분한다.

산출물:

- `docs/wpf-migration/classic-operator-layout-spec.md`

완료 기준:

- 구현자가 XAML을 열고 바로 레이아웃 작업을 시작할 수 있을 정도로 영역, 우선순위, 고정/접힘 정책이 정리된다.

### P1. WPF MainWindow를 Classic Operator Layout으로 재배치

목표: 기존 `MainWindow.xaml`을 FrmMain 방식의 운영 콘솔에 가깝게 만든다.

작업:

- 라이브 제어 버튼을 항상 보이는 고정 영역으로 정리한다.
- Preview와 Output의 상태/조작을 시각적으로 분리한다.
- 절/후렴/브릿지/엔딩 직접 이동 버튼을 첫 화면에서 더 명확하게 노출한다.
- 예배 순서 조작과 현재 Preview/Output 조작의 연결을 명확히 한다.
- 우측 인스펙터가 접혀도 라이브 핵심 조작은 사라지지 않게 한다.
- 명령 팔레트는 보조 탐색 수단으로 유지하되 핵심 운영 기능의 유일한 경로가 되지 않게 한다.

완료 기준:

- `FrmMain` 사용자 기준으로 기본 라이브 송출 흐름을 메뉴/모달 없이 수행할 수 있다.
- Go Live, 송출 후 다음, Black, Clear, Hide, Restore, Restart, Refresh, 절 점프가 첫 화면에서 보인다.
- 1180x760에서 텍스트/버튼이 겹치지 않는다.

### P1. 단축키/키보드 조작 파리티 확인

목표: 기존 운영자의 근육 기억을 보존한다.

작업:

- F12, F11, F9, F3, Space, Shift+Space, Ctrl 계열 단축키를 `FrmMain` 기준으로 점검한다.
- 숫자/문자 절 점프 동작을 Preview/Live 맥락별로 정리한다.
- 텍스트 입력 중에는 라이브 단축키가 오작동하지 않도록 포커스 예외를 둔다.

완료 기준:

- 단축키 표가 문서화된다.
- WPF 테스트 또는 수동 스모크 체크로 주요 단축키가 확인된다.

### P1. XAML 바인딩 모드 품질 게이트 유지

목표: 이번 디버그 알림 같은 바인딩 모드 오류가 재발하지 않게 한다.

작업:

- `IsChecked` 바인딩은 모두 `Mode`를 명시한다.
- 명령으로 토글되는 상태 메뉴는 `OneWay + Command` 패턴을 사용한다.
- 사용자가 직접 값을 바꾸는 체크박스/토글은 `TwoWay`를 사용한다.
- XAML 검사 테스트를 유지하고, 새 XAML 파일도 검사 대상에 포함한다.

완료 기준:

- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj -c Debug --no-restore` 통과.
- WPF 앱 실행 시 바인딩 관련 디버그 알림이 뜨지 않는다.

### P2. 시각 디자인 정돈

목표: FrmMain식 밀도는 유지하되, 낡은 느낌은 WPF 스타일로 정돈한다.

작업:

- 아이콘, 버튼 크기, 간격, 색상 토큰을 일관화한다.
- 위험 명령과 일반 명령의 색상 체계를 명확히 한다.
- 폰트와 줄 높이를 정리한다.
- 고해상도/다중 모니터 환경에서 레이아웃이 무너지지 않게 한다.

완료 기준:

- 화면이 단순히 "낡은 WinForms 복제"가 아니라, 같은 조작성을 가진 WPF 운영 콘솔로 보인다.

### P2. 사용자 검증 루프

목표: 실제 사용자가 느끼는 편의성을 기준으로 마무리한다.

작업:

- 사용자가 자주 하는 라이브 시나리오를 5개 이상 적는다.
- 각 시나리오를 `FrmMain`과 WPF에서 비교한다.
- 클릭 수, 화면 전환 수, 모달/메뉴 진입 수, 실수 가능 지점을 기록한다.

예시 시나리오:

- 예배 순서에서 다음 찬양을 Preview로 확인한 뒤 Go Live.
- 현재 찬양 후렴으로 즉시 점프.
- 송출 중 Black 전환 후 Restore.
- 성경 구절을 찾아 예배 순서에 추가하고 송출.
- 배경 이미지를 바꾸고 현재 항목에 적용.
- 코드 표시를 켜고 조옮김 후 송출.

완료 기준:

- WPF가 `FrmMain`보다 느리거나 불안한 흐름이 남아 있으면 개선 backlog로 등록한다.

## 9. 우선순위 요약

가장 먼저 할 일:

1. `FrmMain` 컨트롤/이벤트/단축키 인벤토리 작성.
2. `MainWindow.xaml` 대응표 작성.
3. Classic Operator Layout 설계안 작성.
4. 첫 화면에서 숨기면 안 되는 라이브 조작 목록 확정.
5. `MainWindow.xaml`을 설계안에 맞춰 단계적으로 재배치.

그 다음 할 일:

1. 단축키 파리티 검증.
2. Preview/Output 조작 분리 강화.
3. 우측 인스펙터 의존도 축소.
4. 컨텍스트 메뉴와 드래그 앤 드롭 파리티 확인.
5. 화면 밀도와 접근성 균형 조정.

마지막에 할 일:

1. 시각 디자인 polish.
2. 고해상도/다중 모니터 확인.
3. 실제 예배 운영 시나리오 기반 사용자 검증.

## 10. 작업 원칙

- `FrmMain`을 낡은 코드가 아니라 검증된 운영 흐름으로 취급한다.
- WPF는 기능 재배치보다 운영 안정성을 먼저 달성한다.
- 자주 쓰는 기능은 숨기지 않는다.
- 모달, 명령 팔레트, 접힌 인스펙터는 보조 경로로 둔다.
- 사용자가 "기존보다 불편하다"고 느끼는 지점은 미감 문제가 아니라 요구사항으로 기록한다.
- 기능 구현이 끝났다는 말은 실제 실행, 테스트, 주요 시나리오 확인 후에만 한다.

## 11. 완료 정의

이 UX 정렬 작업은 다음 조건을 만족해야 완료로 본다.

- WPF `MainWindow`가 디버그 알림 없이 실행된다.
- `FrmMain`의 주요 라이브 운영 기능이 WPF 첫 화면 또는 명확한 동일 경로에 매핑된다.
- `FrmMain` 사용자가 기본 송출 흐름을 새로 배우지 않고 수행할 수 있다.
- Preview와 Output 조작이 명확히 분리되어 있다.
- 단축키 파리티가 문서화되고 검증된다.
- 전체 WPF 테스트가 통과한다.
- 사용자 확인에서 "FrmMain 방식이 더 편하다"는 핵심 불만이 해소되었다고 판단된다.

