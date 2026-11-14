# Design: 메인 화면 표현 현대화

## Context

`FrmMain`은 3단 구조다. 왼쪽은 자료 목록과 예배 순서, 가운데는 미리보기, 오른쪽은 송출이다.
가운데와 오른쪽 단은 각각 머리줄, 슬라이드 목록(가사 또는 PPT 썸네일), 조작 줄, 큰 화면으로 이뤄진다.

코드 분석에서 확인한 관련 컨트롤 (구현 전에 CodeGraph로 다시 확인할 것):

| 영역 | 미리보기 | 송출 |
| --- | --- | --- |
| 머리줄 패널 | `panel9` (`PreviewPanelDisplayName`, `btnToLive`, `btnToOutputMoveNext`, `btnToOutput`) | `panel10` (`OutputPanelDisplayName`, `cbGoLive`, `cbOutputClear`, `cbOutputBlack`) |
| 가사 목록 | `flowLayoutPreviewLyrics` (슬라이드마다 RichTextBox) | `flowLayoutOutputLyrics` |
| PPT 썸네일 | `flowLayoutPreviewPowerPoint` (`ImageCanvas`) | `flowLayoutOutputPowerPoint` |
| 조작 줄 | `panel1` (`panel3`, `flowLayoutPanel1`) | `panel2` (`flowLayoutPanel2`, `panel6`) |
| 상태 라벨 | 없음 | `panel8` (`labelBlackScreen`, `labelHideText`, `labelGapItem`) |
| 큰 화면 | `panelPreviewBottom` → `PreviewHolder` → `PreviewScreen` | `panelOutputBottom` → `OutputHolder` → `OutputScreen` |

## Goals / Non-Goals

- Goals: 미리보기와 송출을 한눈에 구분한다. 송출 상태를 색이 아닌 글자로도 알 수 있게 한다. 잘못 누르는 실수를 줄인다.
- Non-Goals: 배치 변경, 조작 방법 변경, 새 UI 프레임워크 도입.

## Decisions

### D1. 색은 한곳에 모은다

색 값을 여러 파일에 흩어 쓰지 않는다. 팔레트를 한 클래스에 상수로 둔다. 위치는 `Global/gfColorsFonts.cs`를 먼저 검토하고, 맞지 않으면 새 파일을 만든다.

| 이름 | 값 | 쓰는 곳 |
| --- | --- | --- |
| PreviewAccent | `#005FB8` | 미리보기 선택 테두리 |
| PreviewTint | `#E5F1FB` | 미리보기 머리줄 바탕 |
| LiveAccent | `#C4500A` | 송출 선택 테두리, 켜진 송출 버튼 |
| LiveTint | `#FDEDE3` | 송출 머리줄 바탕, 송출 중인 목록 줄 |
| NextSlide | `#8A8A8A` | 다음 슬라이드 점선 테두리 |
| Surface | `#FFFFFF` | 목록 바탕 |
| SurfaceAlt | `#F3F3F3` | 창 바탕 |
| TextPrimary | `#1B1B1B` | 본문 글자 |
| TextSecondary | `#5C5C5C` | 보조 글자 |
| OnAccent | `#FFFFFF` | PreviewAccent, LiveAccent 바탕 위의 글자 |

글자와 바탕의 명도 대비는 4.5:1 이상이어야 한다. 파랑과 주황은 색각 이상이 있어도 구분되도록 명도가 다르다.

PreviewAccent와 LiveAccent는 PPT 썸네일에서는 선택 테두리로, 가사 목록에서는 선택된 슬라이드의 바탕으로 쓴다. 가사 목록에서 연한 바탕(Tint)을 쓰지 않는 이유는 포커스를 가진 목록의 바탕색과 거의 구분되지 않기 때문이다. 그 위의 글자는 OnAccent를 쓴다. OnAccent와 LiveAccent의 대비는 약 4.67:1로 기준을 겨우 넘으므로, LiveAccent를 더 밝게 바꾸면 안 된다.

### D2. 꾸미는 코드는 Designer 파일 밖에 둔다

`FrmMain.Designer.cs`는 Visual Studio 디자이너가 다시 쓰는 파일이고, 원격 저장소의 WPF 작업과 합칠 때 충돌이 가장 나기 쉽다.
그래서 색과 테두리를 적용하는 코드는 새 partial 파일(예: `FrmMain.ModernUi.cs`)에 모으고, 폼이 뜰 때 한 번 호출한다.
Designer 파일은 새 컨트롤을 추가해야 할 때만 고친다.

### D3. 이전 모양으로 되돌리는 설정을 둔다

예배 중에 문제가 생기면 바로 되돌릴 수 있어야 한다. 설정 하나(기본값: 새 모양 사용)를 두고, 끄면 이번 변경이 적용한 색과 테두리가 전혀 적용되지 않게 한다.
설정은 다른 옵션과 같은 곳(`gfDatabase.LoadSavedData`)에서 읽는다. 옵션 대화상자에 항목을 추가하지는 않는다. 레지스트리 값으로만 바꾼다.

- 위치: `HKEY_CURRENT_USER\Software\EasiSlides\options`, 값 이름 `ModernMainUi`
- 값이 없으면 새 모양이다. 값이 0이거나 숫자로 읽을 수 없으면 이전 모양이다. 읽을 수 없는 값일 때 이전 모양으로 가는 것은 의도한 동작이다. 문제가 있을 때 안전한 쪽으로 간다.
- 시작할 때 한 번만 읽는다. 바꾼 뒤에는 앱을 다시 시작해야 한다.

### D4. 동작을 바꾸지 않는다

클릭, 더블클릭, 끌어 놓기, 키 입력 처리 코드는 건드리지 않는다. 목록을 직접 그리도록 바꾸는 작업(4, 5)에서도 선택과 끌어 놓기 동작이 그대로여야 한다.

### D5. 상태 표시는 색에만 의존하지 않는다

켜짐/꺼짐, 송출 중/미리보기, 다음 슬라이드는 색과 함께 글자나 선 모양(실선/점선)으로도 구분한다.

### D6. 사용자가 고른 선택 색이 우선한다

옵션 창의 "Selected Colour / Selected Back"은 가사 목록에서 선택된 슬라이드의 색을 정한다. 새 모양이 이 설정을 무조건 덮어쓰면 옵션 창의 버튼이 아무 효과 없이 남는다.

- 두 값이 **기본값 그대로**(빨간 글자, 노란 바탕)이면 새 모양의 색(미리보기 파랑, 송출 주황)을 쓴다.
- 사용자가 둘 중 하나라도 **바꿨으면** 사용자의 색을 두 단 모두에 그대로 쓴다. 이때 미리보기와 송출의 구분은 머리줄 바탕색과 썸네일 테두리로만 한다.
- 판단은 색을 칠하는 시점에 한다. 옵션 창에서 색을 바꾸면 앱을 다시 시작하지 않아도 반영돼야 한다.

적대적 리뷰(작업 1, 지적 1)에서 나온 결정이다.

알려진 한계: "기본값 그대로"와 "기본값을 일부러 골랐다"를 구분하지 못한다. 새 모양에서 빨간 글자와 노란 바탕을 그대로 쓰고 싶은 사용자는 되돌리기 설정을 꺼야 한다. 또 두 색 중 하나만 바꿔도 두 단의 가사 목록 색 구분이 사라진다. 옵션 창에 설명을 넣는 일은 이번 범위에 넣지 않는다.

### D7. 고대비 테마에서는 새 모양을 적용하지 않는다

Windows 고대비 테마(`SystemInformation.HighContrast`)가 켜져 있으면 되돌리기 설정을 끈 것과 같게 동작한다. 사용자가 고른 접근성 테마를 밝은 색으로 덮지 않기 위해서다.

알려진 한계: 머리줄 색은 시작할 때, 썸네일 테두리는 썸네일을 처음 만들 때, 가사 목록 색은 칠할 때마다 판단한다. 그래서 앱을 실행한 채로 고대비 테마를 켜거나 끄면 세 부분이 서로 다른 모양이 될 수 있다. 테마를 바꾼 뒤에는 앱을 다시 시작해야 한다.

### D8. 미리보기와 송출은 패널 자체로 구분한다

어느 단인지 판단할 때 컨트롤 이름 문자열을 비교하지 않고 패널 참조를 비교한다. 미리보기 패널도 송출 패널도 아니면 새 모양을 적용하지 않는다. 가사 목록의 선택 색은 전역 표에 저장하지 않고 `FrmMain`이 색을 칠하는 함수에 직접 넘긴다.

## Known Leftovers

- 왼쪽 자료 탭의 PowerPoint 썸네일은 선택 테두리가 빨강 그대로다. 미리보기/송출 구분과 무관하므로 이번 범위에서 고치지 않는다.
- Text / Set / Info 전환 버튼의 눌린 색도 빨강 그대로다.
- 쇼가 꺼져 있어도 송출 단의 현재 슬라이드가 주황으로 표시된다. 작업 2에서 다룬다.
- 송출 단에 포커스가 없을 때는 간격 항목이 나가는 중에도 현재 슬라이드가 선택 색으로 칠해진다 (`gfUiText.cs`의 `HighlightRichTextBox`, 포커스가 없는 쪽 분기). 작업 2에서 함께 다룬다.
- `ImageCanvas.SelectedBorderColor`는 값을 바꿔도 썸네일을 다시 그리지 않는다. 지금은 처음 그리기 전에 한 번만 정하므로 문제가 없다. 작업 2에서 다음 슬라이드 표시를 넣을 때 다시 그리도록 고친다.
- `FormatLyricsContainers`는 호출될 때마다 슬라이드 수만큼 `Font`를 새로 만들고 이전 것을 해제하지 않는다. 이번 변경 이전부터 있던 문제다. 화면 표현과 무관하므로 별도 수정으로 다룬다.
- `FormatLyricsContainers`는 미리보기와 송출을 패널 이름 문자열로 구분한다. 이번 변경 이전부터 있던 코드라 그대로 둔다.

## Verification (작업 묶음마다)

1. `dotnet build Easislides\Easislides.csproj -nologo -v minimal`이 오류 없이 끝난다. 이번 변경으로 새 경고가 생기지 않는다.
2. 앱을 실행해 메인 화면을 캡처하고 사양의 시나리오와 눈으로 대조한다. 캡처는 미리보기만 채운 상태와, 미리보기를 송출로 복사한 상태 두 가지를 찍는다.
3. 되돌리기 설정을 끈 상태로도 캡처해 이전 모양과 같은지 본다.
4. 적대적 리뷰와 내장 코드 리뷰의 지적 사항이 모두 처리됐거나, 처리하지 않은 이유가 기록돼 있다.

## Risks / Trade-offs

- **목록 직접 그리기(작업 4, 5)**: 선택, 끌어 놓기, 키보드 이동이 깨질 수 있다. 이 두 작업은 리뷰에서 동작 확인을 따로 요구한다.
- **PPT 항목 캡처**: PPT 항목을 고르면 PowerPoint가 뜬다. 캡처 뒤 PowerPoint 프로세스가 남지 않는지 확인한다.
- **원격과의 충돌**: 원격의 `FrmMain.Events.cs`, `FrmMain.Layout.cs`에 작은 변경이 있다. 작업 7이 `FrmMain.Layout.cs`를 크게 고치므로 합칠 때 충돌할 수 있다.
- **WinForms의 한계**: 둥근 모서리와 부드러운 글자 표현은 시안보다 거칠다. 시안과 똑같이 만드는 것은 목표가 아니다.

## Open Questions

- 검은 화면이 켜진 채로 다음 항목을 송출하면 자동으로 풀지 여부. 이번 변경에서는 지금 동작을 유지한다.
- 찬양집 모드에서 송출 단의 표시. 이번 변경에서는 지금 동작을 유지한다.
