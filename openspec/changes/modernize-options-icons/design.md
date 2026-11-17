# Design: 옵션 창의 아이콘을 현대 모습으로 바꾼다

## 확인된 것 (2026-11-17)

- `FrmOptions`: 탭 8개 (Main Window, Show, Monitors, Alerts, Folders, Bibles, Licence, Keyboard). 도구 모음 28개, `ToolStripButton` 42개, `ToolStripDropDownButton` 10개, `ToolStripMenuItem` 28개. 아이콘은 `FrmOptions.resx`의 비트맵 80여 개.
- 목록: `imageListSys`(옵션 창의 것)를 쓰는 `ListView` 2개 (`SongFolder`, `BibleList`. `BibleSearchList`에는 이미지 목록이 없다). 폴더 목록은 인덱스 25/26(사용/미사용), 성경 목록은 4/27. 코드에서 `ImageIndex`를 바꾼다 (`FrmOptions.cs` 약 576, 638, 1055~1294행).
- 메인 화면의 방식: `Global/ModernIcons.cs` (경로 자료, `Get` / `Render`, 캐시, 직접 그린 아이콘), `FrmMain.ModernUi.Icons.cs`와 `FrmMain.ModernUi.FormatIcons.cs` (컨트롤과 아이콘 이름의 표, 원래 이미지의 기록과 복원), `FrmMain.ModernUi.ImageList.cs` (같은 인덱스 배치의 두 번째 이미지 목록).
- 현대 모습의 스위치: `Gf.UseModernMainUi && !SystemInformation.HighContrast`.
- 옵션 창은 열 때마다 새로 만든다. 취소(`DialogResult.Cancel`)는 저장하지 않는다 (구현에서 다시 확인).

## Decisions

### O1. 메인 화면의 규칙을 그대로 쓴다

- 아이콘은 `ModernIcons`에서 얻는다. 호출 규칙(D18~D24)을 지킨다: 캐시 비트맵을 `ToolStripItem`에 넣기 전에 `ImageTransparentColor`를 비우고, `ImageScaling = None`, 32비트 이미지 목록은 `ColorDepth`를 먼저 정하고, 이미지 목록의 원본 비트맵은 목록이 살아 있는 동안 버리지 않는다.
- 같은 뜻의 단추는 메인 화면의 표에 있는 이름을 쓴다 (굵게 `text_bold`, 기울임 `text_italic`, 밑줄 `text_underline`, 그림자 `text_shadow`, 외곽선 `text_effects`, 정렬 `text_align_*`, 세로 정렬 `textbox_align_*`, 투명 `transparency_square` 등). 메인 화면에 없는 뜻(스크롤, 깜박임, 오른쪽에서 왼쪽, 위치 고르기, 정보)은 Fluent 묶음에서 고르고, 고정한 커밋의 `<Folder>/SVG/` 파일만 쓴다. 맞는 것이 없으면 메인 화면의 "직접 그린 아이콘" 방식으로 만든다.
- 메인 화면과 옵션 창이 같은 표를 두 벌 갖지 않게 한다. 공통으로 쓸 수 있는 부분(이름으로 아이콘을 얻어 항목에 넣고 원래 것을 기록하는 일)이 메인 화면에 `FrmMain` 전용으로 있으면, 옵션 창에서 다시 쓸 수 있는 가장 작은 형태로 꺼낸다. 메인 화면의 동작은 바꾸지 않는다.

### O2. 적용 시점과 되돌리기

- 창을 만들 때(또는 `Load`) 스위치를 한 번 보고 적용한다. 창이 열려 있는 동안 스위치가 바뀌는 경우는 다루지 않는다 (옵션 창은 모달이고, 스위치는 메인 화면의 것).
- 스위치가 꺼져 있으면 아무것도 건드리지 않는다. 디자이너가 넣은 비트맵이 그대로 쓰인다.
- 창이 닫히면 이 창이 만든 이미지 목록과 그 비트맵을 정리한다. `ModernIcons`의 캐시 비트맵은 버리지 않는다 (공유).

### O3. 도구 모음의 바탕

아이콘만 바꾸면 눌린 상태와 마우스를 올린 상태의 바탕이 예전 그대로다. 단색 아이콘이 그 바탕에서 잘 보이면 그대로 두고, 눌린 상태가 알아보기 어려우면 메인 화면의 평평한 렌더러(`ModernSurfaceRenderer`)를 옵션 창의 도구 모음에도 쓴다. 어느 쪽인지는 캡처로 판단하고 적는다. 그 밖의 표면(탭, 그룹 상자, 일반 단추의 모양, 글꼴)은 바꾸지 않는다.

### O4. 목록의 아이콘

`imageListSys`를 쓰는 목록에는 메인 화면의 두 번째 이미지 목록과 같은 방식을 쓴다: 같은 인덱스 배치의 새 목록을 만들어 목록 컨트롤에 넣는다. 코드가 쓰는 인덱스(4, 25, 26, 27)의 뜻은 그대로다.

## 시험 방법

- 실제 자료를 바꾸지 않는다. 옵션 창은 메인 화면의 Options 단추로 열고, **OK를 누르지 않는다.** 탭을 넘기며 캡처한 뒤 Cancel로 닫는다. 창 안의 다른 컨트롤(단추, 목록, 입력 칸)은 건드리지 않는다. 드롭다운을 펼쳐 캡처하는 것은 괜찮다 (항목을 고르지 않고 Esc로 닫는다).
- 실행 전후에 레지스트리, 예배 목록 32개, DB 파일이 같은지 확인한다 (레지스트리는 내보냈다가 되돌린다).
- 현대 모습을 끈 상태는 코드로 확인한다 (스위치를 바꾸는 것은 설정 변경이다).
- 탭 8개의 전후 캡처를 나란히 둔다.

## 구현하면서 정한 것 (2026-11-17)

- **대상**: 디자이너의 이미지 80개 전부 (단추 42, 드롭다운 단추 10, 메뉴 항목 28). 드롭다운 단추는 고른 메뉴 항목의 그림을 그대로 보여 준다 (`Gf.AssignDropDownItem`이 복사한다).
- **아이콘 표** (`FrmOptions.ModernUi.cs`):

  | 뜻 | 아이콘 |
  |---|---|
  | 굵게 / 기울임 / 밑줄 | `text_bold` / `text_italic` / `text_underline` |
  | 그림자 / 외곽선 | `text_shadow` / `text_effects` |
  | 투명 | `transparency_square` |
  | 가로 정렬, 세로 정렬 | `text_align_*`, `textbox_align_*` |
  | 기울임 없음 / 전체 / 후렴만 | `italic_off` / `text_italic` / `italic_chorus` |
  | 스크롤 (새로 넣음) | `text_direction_horizontal_left` |
  | 깜박임 (새로 넣음) | `flash` |
  | 오른쪽에서 왼쪽 (새로 넣음) | `text_paragraph_direction_left` |
  | 위치 고르기 | `folder_open` |
  | 정보 | `info` |
  | 성경 목록의 위로 / 아래로 | `arrow_up` / `arrow_down` |
  | 목록: 성경 있음 / 폴더 사용 / 미사용 / 성경 없음 | `bible` / `checkbox_checked`(채움, 새로 넣음) / `checkbox_unchecked`(새로 넣음) / `warning` |

  새로 넣은 다섯 개는 고정한 커밋의 `<Folder>/SVG/` 파일을 그대로 옮겼다. `ModernIcons.cs`의 원본 파일 수는 262다. 폴더 사용 표시는 16px에서 얇은 체크가 흐려 채운 체크 상자를 썼다.
- **메인 화면과 같이 쓰는 부분** (`Global/ModernItemIcons.cs`): 아이콘을 항목에 넣는 일(`TryApply`, `SetSharedImage`), 드롭다운 단추 준비와 맞춤, 크기 계산, 이미지 목록의 칸 만들기를 `FrmMain`에서 꺼냈다. 원래 것을 기억하고 되돌리는 일은 메인 화면에 남아 있고 콜백으로 넘긴다. 메인 화면의 캡처는 전후가 픽셀 단위로 같다.
- **O3**: 도구 모음의 바탕은 바꾸지 않았다. 눌린 단추의 연한 파란 바탕과 테두리가 단색 아이콘 뒤에서 잘 보인다.
- **O2**: 모습이 꺼져 있으면 `Load` 처리도 붙이지 않는다. 창이 닫히면 목록을 `imageListSys`로 되돌리고 이 창의 이미지 목록과 칸 28개를 정리한다. 정리가 닫힘보다 먼저 와도 컨트롤은 건드리지 않는다.
- **같이 고친 기존 버그**:
  - 옵션 창을 닫아도 정리(`Dispose`)하지 않아 열 때마다 창 하나가 남았다. `FrmMain`에서 `using`으로 연다.
  - Bibles 탭 정보 단추(`Bibles_Info`)의 툴팁이 "Delete"였다. 단추는 성경 정보를 보여 준다. "Bible Info"로 고쳤다 (두 모습 모두).

## 확인한 것

- 솔루션 전체 빌드 오류 0.
- 탭 8개의 전후 캡처, 펼친 드롭다운, 눌린 단추와 아닌 단추의 확대.
- 한 번의 실행에서 옵션 창을 두 번 열고 닫음: 첫 창이 정리된 뒤에도 두 번째 창의 아이콘이 정상이다 (공유 캐시가 상하지 않았다).
- 메인 화면: 수정 전후 캡처가 같다.
- 매 실행에서 DB, 예배 목록 32개, 레지스트리가 그대로였다. OK는 누르지 않았다.

## Known Limitations

- 창이 열려 있는 동안 고대비 모드를 바꾸는 경우는 다루지 않는다 (O2).
- 드롭다운의 메뉴 항목 중 일부만 새 아이콘을 얻지 못한 경우, 그 항목을 고르면 단추에 예전 그림의 자홍색 바탕이 보일 수 있다. 메인 화면과 같은 동작이고, 쓰는 아이콘 이름이 모두 있어 실제로는 일어나지 않는다.

## 확인하지 못한 것 (코드로만 확인)

- 현대 모습이 꺼진 상태와 고대비 모드에서의 화면
- 100 %, 150 % 배율 (이 PC는 125 %)
- `Bibles_Info`의 새 툴팁이 뜨는 모습 (UI 자동화가 툴팁 글을 돌려주지 않는다)
