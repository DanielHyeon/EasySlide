# Design — a045 카메라 자막 탭

## 현재 구조 (영향 근거)

- `CaptionConsole : Window`(XAML 671줄, 코드 숨김 약 1470 + 520줄). 본창 `OpenCameraCaptions_Click`이 처음 열 때 만들고 `ShowOnScreen()`으로 띄운다. 닫기는 숨기기, 예배 순서(계획)가 바뀌면 `EndSession()`으로 스스로 닫히고 본창이 참조를 버린다. 본창이 닫힐 때도 `EndSession()`.
- 창에만 있는 것: 창 위치 기억(`TrackPlacement`/`KeepPlacement`/`PlaceOnScreen`, `CaptionWindowPlacement.Read/Write/Fits/Plan`), `Closing` 숨기기, 창 수준 `PreviewKeyDown`·`PreviewMouseUp`, `FocusManager.SetFocusedElement(this, …)`, `MessageBox.Show(this, …)`, `OpenFileDialog.ShowDialog(this)`, 설정 창 `Owner = this`.
- 본창 `OnPreviewKeyDown`: Esc → 쇼 끝내기, 미디어 키, 위/아래 전역 화살표, 장면 단축키, 포커스된 미리보기/송출 칸 키, 절 점프(숫자·글자), 입력 칸 보호(`IsTextInputFocused` + `IsMenuShortcut`), 마지막에 단축키 등록부(`ShortcutRegistry`: Space = 다음 등).
- 전역 키 훅(`GlobalInputService`)은 앱 창이 앞에 있을 때 Esc·화살표를 처리하지 않는다(창이 처리). 그래서 본창 안의 칸으로 옮겨도 Esc·화살표는 본창 `OnPreviewKeyDown`만 거친다.
- 미리보기 칸: `ClassicRightConsole`의 0번 열 `PreviewSplit`(위: `ClassicPreviewPane`, 아래: `ClassicPreviewSlidePane`). `MainMenuBarTests`가 이 이름들을 검사한다.

## 결정

1. **칸으로 바꾸기**: `CaptionConsole`을 `UserControl`로 바꾼다. 클래스·생성자·DI·세션 규칙은 그대로다(바뀌는 것은 놓이는 곳과 배치). 자원(스타일)은 지금처럼 칸 안에 두어 테스트가 앱 사전 없이도 연다.
2. **탭 줄**: 새 `CaptionTabStrip`(UserControl)에 `[미리보기] [● 자막]` 라디오 탭과 켜기/끄기 스위치(`AirButton`, 지금 운영 창의 송출 상태 스타일)를 둔다. 콘솔이 만들고(`Strip`, DataContext = 콘솔 뷰모델) 본창이 미리보기 열 맨 위 줄에 놓는다. `CaptionConsole.AirButton`은 `Strip.AirButton`을 가리킨다(테스트·하네스 이름 유지).
3. **본창 배치**: `PreviewSplit`을 감싸는 열 Grid(줄: 탭 줄 Auto, 내용 *)를 두고, 내용 줄에 `PreviewSplit`과 자막 칸 자리(`CaptionHost`)를 겹쳐 둔다. 탭은 **보이기/숨기기로만** 바꾼다(내용을 갈아 끼우면 숨은 칸이 언로드되어 따라가기가 멈춘다). 기존 이름·`Tag`(splitContainerPreview 등)는 그대로 둔다.
4. **열기**: 메뉴 "카메라 하단 자막"(F2 표시)과 F2. 처음이면 콘솔을 만들어 탭 줄을 보이고 자막 탭을 고른다. 탭 줄은 그 뒤 계속 보인다(켜기/끄기 상태가 늘 보이도록). 다음 실행 때 저절로 열지는 않는다(지금 운영 창과 같음).
5. **끝내기**: `EndSession(planChanged)`이 지금 `Closed` 처리(구독 해제·복구 저장·설정 창 닫기)를 한 번만 하고 `Ended`를 알린다. 본창은 칸을 떼어 내고, 계획이 바뀐 경우면 같은 탭 상태로 새 콘솔을 만든다(지금 운영 창은 닫혔다가 메뉴로 다시 열어야 했다). 본창이 닫힐 때는 다시 만들지 않는다.
6. **키**:
   - F6(자막 켜기/끄기)·F2(탭 전환)는 `MainCommandIds`·`CommandCatalog`에 명령으로 넣고 기본 단축키로 둔다(설정의 단축키 화면·명령 팔레트에 나오고 바꿀 수 있다). 입력 칸에서도 되도록 `IsMenuShortcut`에 넣는다. F6은 자막을 연 뒤에만 실행된다.
   - 포커스가 자막 칸 안이면 본창 `OnPreviewKeyDown`은 Ctrl·Alt 없는 일반 키(기능 키 F1~F24 제외)를 처리하지 않고 넘긴다. 그러면 칸의 `PreviewKeyDown`(지금 운영 창 키 규칙)이 받는다. 그래서 자막 칸에서 Esc는 쇼를 끝내지 않고 자막만 숨기고, 숫자는 미리보기 절로 튀지 않는다. F9·F12 같은 기능 키와 Ctrl 단축키는 본창이 처리한다.
   - 자막 탭 밖에서는 지금 본창 키 그대로다.
7. **포커스**: 칸 안을 마우스로 누르면(빈 곳 포함) 포커스가 칸으로 온다. 버튼·라디오·단계를 마우스로 누른 뒤는 지금처럼 직접 입력이면 입력 칸, 따라가기면 칸 바탕(`Surface`)이다. 탭 줄을 누르면 고른 탭의 칸으로 포커스를 돌린다(미리보기면 본창 미리보기 표면). 그래서 탭 줄 버튼에 포커스가 남아 Space가 스위치를 누르는 일이 없다. 포커스 범위는 칸이 놓인 창(`FocusManager.GetFocusScope`)이다.
8. **배치(한 열)**: 줄 0 원본 선택·OBS 상태·설정, 줄 1(남는 높이) 따라가기면 표시 방식 + 단계 목록(목록이 남는 높이를 쓰고 안에서 스크롤, 최소 한 줄), 직접 입력이면 입력 칸(최소 3줄) + 보내기 + 입력하는 대로, 줄 2 안내 한 줄, 줄 3(따라가기만) 이전·다음·현재 위치(장·줄과 PPT 이름)·싱크, 줄 4 카메라 화면(칸 폭의 16:9, 지금 미리보기 그리기 그대로), 줄 5 자막 모양과 내보낼 곳(줄바꿈). 칸이 너무 낮으면 본문이 스크롤된다(최소 높이 = 고정 줄들 + 줄 1의 최소). "자막 모양" 제목 줄과 지금 위치 카드는 줄인다(위치는 줄 3으로).
9. **지우는 것**: 운영 창 예배 순서(목록·클릭 송출·Enter·스크롤 맞추기·휠·칸 높이 맞추기), 창 위치 기억과 `CaptionWindowPlacement`의 창 위치 계산(설정 창 배치에 쓰는 `Screens/At/Inside/Minimum/Bounds/Apply`만 남김), 좁은 맨 위 줄에서 "내보낼 곳"을 빼던 처리. 관련 테스트도 지운다.
10. **문구**: 스위치 글의 "(Space)"를 "(F6)"으로 바꾼다(자막 탭 밖에서도 되는 키). 메뉴 글의 "…"를 뺀다(창을 열지 않음). 새 글: 탭 이름 "자막", 싱크 줄임 "싱크", 명령 이름·설명 2쌍. 한국어·영어 모두.
11. **설정 창**: 소유자는 본창, 열 때마다 본창이 있는 화면의 작업 영역에 맞춘다(지금 규칙, 기준 창만 바뀜).

## 위험과 대응

- 본창 키 순서: 자막 칸 안의 일반 키를 본창이 먼저 처리하면 Esc가 쇼를 끝낸다 → 본창 `OnPreviewKeyDown` 맨 앞(명령 팔레트 검사 다음)에서 넘긴다. 테스트로 고정한다.
- 숨은 탭: 내용 교체가 아니라 보이기/숨기기라 콘솔은 로드된 채로 따라가기를 계속한다. 카메라 미리보기 폴링은 칸이 보일 때만 돈다(지금 `IsVisible` 규칙).
- 테스트 대량 수정: 콘솔 테스트는 탭 줄 + 칸을 담은 시험용 창에서 돈다(`WithConsole`). 창 위치·예배 순서 목록 테스트는 지운다.
- 허용 한계: 본창 최소 크기(780×540)에서는 자막 칸이 스크롤된다. 미리보기 칸 폭이 아주 좁으면 상태 글은 말줄임된다(툴팁에 전체).
