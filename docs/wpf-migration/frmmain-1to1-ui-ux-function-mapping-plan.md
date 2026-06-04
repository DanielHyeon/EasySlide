# FrmMain 1:1 UI/UX and Function Mapping Plan

작성일: 2026-06-04

기준 파일:

- `Easislides/Easislides/FrmMain.Designer.cs`
- `Easislides/Easislides/FrmMain.cs`
- `Easislides/Easislides/FrmMain.Events.cs`
- `Easislides/Easislides/FrmMain.Logic.cs`
- `Easislides/Easislides/FrmMain.Layout.cs`
- `Easislides/Easislides/FrmMain.Fields.cs`
- `Easislides.Wpf/MainWindow.xaml`
- `Easislides.Wpf/MainWindow.xaml.cs`

## 1. 재수립 이유

최근 WPF 작업은 일부 영역을 FrmMain처럼 보이게 배치했지만, 실제 평가는 불합격이다. 이유는 명확하다.

- 좌측 상단 소스 브라우저가 FrmMain의 실제 탭, 툴바, 목록, 검색, 더블클릭, 드래그 동작을 1:1로 갖지 않는다.
- 좌측 하단 Worship List와 Praise Book은 보이기만 할 뿐, 저장 목록 불러오기, 예배순서 편집, 항목 이동, 컨텍스트 메뉴, 드래그 앤 드롭이 FrmMain 흐름과 동일하지 않다.
- 성경 탭은 `BibleUserLookup`, `BookLookup`, `TabBibleVersions`, `BibleText`, 선택 구절 추가 흐름을 완전히 재현하지 못한다.
- 오른쪽 영역은 FrmMain의 Preview와 Output이 분리된 운영 콘솔이 아니라, 일부 미리보기 껍데기를 붙인 상태다.
- PowerPoint, Images, InfoScreen, Media의 소스 목록과 우측 Preview/Output 썸네일 관계가 FrmMain 운영 방식과 다르다.
- 화면 UI/UX도 1:1 대상이어야 하는데, 현재는 기능 매핑과 화면 매핑이 분리되어 계획되었다.

따라서 이후 작업은 "WPF스럽게 재해석"이 아니라 먼저 "FrmMain 1:1 기준선 복구"로 진행한다. WPF 개선은 1:1 매핑이 통과한 뒤 별도 단계로 한다.

## 2. 1:1의 정의

1:1 매핑은 WinForms 코드를 그대로 복사한다는 뜻이 아니다. 운영자가 FrmMain에서 하던 동작을 같은 화면 영역, 같은 우선순위, 같은 입력 경로로 수행할 수 있어야 한다는 뜻이다.

필수 조건:

- 첫 화면의 영역 구조가 FrmMain의 `SplitContainer` 구조와 대응해야 한다.
- 각 영역 안의 주요 컨트롤은 원본 컨트롤명 기준으로 WPF 대응 항목을 가져야 한다.
- 각 버튼, 목록, 탭, 단축키, 더블클릭, 드래그 앤 드롭, 컨텍스트 메뉴는 "동일", "WPF식 대체", "보류", "제거 금지" 중 하나로 판정되어야 한다.
- `C:\EasiSlides` 기반 실제 데이터가 로드되어야 한다.
- Worship List, Praise Book, Bible, PowerPoint, Images, InfoScreen, Media는 모두 실제 항목을 추가하거나 미리보기/송출로 이어져야 한다.
- Preview와 Output은 항상 화면상으로 분리되어야 한다.
- 검증 전에는 완료라고 말하지 않는다.

## 3. FrmMain 원본 화면 계층

FrmMain 기준 계층은 다음 구조다.

| FrmMain 구조 | 원본 컨트롤 | 역할 |
| --- | --- | --- |
| 전체 운영 콘솔 | `toolStripContainerMain` | 메뉴, 툴바, 본문, 상태바의 최상위 컨테이너 |
| 좌측/우측 대분할 | `splitContainerMain` | 왼쪽 소스/예배목록, 오른쪽 Preview/Output |
| 좌측 상하 분할 | `splitContainer1` | 상단 소스 브라우저, 하단 예배 목록 |
| 좌측 상단 소스 탭 | `tabControlSource` | Folders, InfoScr, PowerPoint, Bibles, Images, Media, Default |
| 좌측 하단 목록 탭 | `tabControlLists` | Worship List, Praise Book |
| 오른쪽 Preview/Output 분할 | `splitContainer2` | 왼쪽 Preview, 오른쪽 Output |
| Preview 상하 분할 | `splitContainerPreview` | 상단 텍스트/PPT/서식 조작, 하단 실제 Preview 화면 |
| Output 상하 분할 | `splitContainerOutput` | 상단 텍스트/PPT/송출 상태, 하단 실제 Output 화면 |

WPF 기준선은 이 계층을 직접 반영해야 한다.

## 4. 화면 UI/UX 1:1 매핑

### 4.1 전체 화면

| FrmMain | WPF 목표 | 상태 기준 |
| --- | --- | --- |
| `menuStripMain` | File, Edit, View, Output, Tools, Help 유지 | 보조 경로로 유지 |
| `toolStripMain` | 상단 아이콘 툴바 유지 | 새 항목, 편집, 복사, 이동, 삭제, 미디어, 새로고침, 옵션, 회전, 알림, 검색, 점프 노출 |
| `splitContainerMain.Panel1` | WPF 좌측 고정 영역 | 소스 탭과 예배 목록이 동시에 보임 |
| `splitContainerMain.Panel2` | WPF 오른쪽 Preview/Output 영역 | Preview와 Output이 나란히 보임 |
| status strip | WPF 하단 상태바 | 폴더 항목 수, Worship 항목 수, 현재 송출, 모니터 상태 노출 |

### 4.2 좌측 상단 소스 브라우저

| FrmMain 탭/컨트롤 | WPF 1:1 목표 | 필수 동작 |
| --- | --- | --- |
| `tabFolders` | `Folders` 탭 | `SongFolder`, `SongsList`, 빠른 검색, 더블클릭 추가, 드래그로 Worship List 추가 |
| `tabFiles` | `InfoScr` 탭 | `InfoScreenFolder`, `InfoScreenList`, New/Edit/Copy/Move/Delete, Import, 드래그 추가 |
| `tabPowerpoint` | `PowerPoint` 탭 | `PowerpointFolder`, `PowerpointList`, List/Preview 스타일, Import, 썸네일, 더블클릭 추가 |
| `tabBibles` | `Bibles` 탭 | `BookLookup`, `BibleUserLookup`, `Bibles_Go`, `TabBibleVersions`, `BibleText`, 선택 구절 추가 |
| `tabImages` | `Images` 탭 | `ImagesFolder`, `flowLayoutImages`, 배경으로 적용, 기본 배경 지정, 새로고침 |
| `tabMedia` | `Media` 탭 | `MediaFolder`, `MediaList`, Import, 더블클릭 추가, 드래그 추가 |
| `tabDefault` | `Default` 탭 | `DefPanel`의 기본 서식, 배경, 전환, 표시 패널 옵션 |

UI/UX 규칙:

- 탭 순서는 FrmMain `tabControlSource.Controls.Add(...)` 순서를 기본값으로 한다.
- 탭 헤더 위치는 FrmMain처럼 아래쪽 배치가 기본이다.
- 상단 툴바, 폴더 콤보, 목록은 한 화면 안에 있어야 하며 모달로 숨기지 않는다.
- 목록 더블클릭, Enter, 컨텍스트 메뉴, 드래그 앤 드롭은 동일한 진입 경로를 가져야 한다.

### 4.3 좌측 하단 Worship List와 Praise Book

| FrmMain 컨트롤 | WPF 1:1 목표 | 필수 동작 |
| --- | --- | --- |
| `tabWorshipList` | `Worship List` 탭 | 항상 좌측 하단에 표시 |
| `SessionList` | 저장된 예배순서 콤보 | `C:\EasiSlides\Admin\WorshipLists`의 실제 `.esw` 목록 로드 |
| `WorshipListItems` | 예배순서 목록 | 선택, 더블클릭 Preview, Play, Play on Output, 재정렬 |
| `WL_Manage` | 관리 | 기존 세션 관리 흐름 또는 WPF 대체 창 |
| `WL_Add` | 선택 소스 추가 | 현재 선택 소스에 따라 곡, 성경, PPT, InfoScreen, Media 추가 |
| `WL_Open` | 예배순서 열기 | 저장 목록 또는 파일 열기 |
| `WL_Up`, `WL_Down`, `WL_Delete` | 항목 이동/삭제 | 키보드와 버튼 모두 동작 |
| `WL_Word`, `WL_Notes` | Word/Notes | 보류하더라도 위치와 상태 표시 필요 |
| `tabPraiseBook` | `Praise Book` 탭 | Worship List 옆 하단 탭 |
| `PraiseBook` | 찬양집 선택 콤보 | 실제 찬양집 목록 로드 |
| `PraiseBookItems` | 찬양집 항목 | 더블클릭/추가/삭제/Word/HTML 동작 |

UI/UX 규칙:

- Worship List는 FrmMain처럼 소스 탐색과 동시에 보여야 한다.
- Praise Book은 별도 팝업만으로 대체하지 않는다.
- 드래그 앤 드롭은 `DragDropSource` 기준으로 모두 매핑한다: `WorshipList`, `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `BiblePassage`.

### 4.4 오른쪽 Preview 영역

| FrmMain 컨트롤 | WPF 1:1 목표 | 필수 동작 |
| --- | --- | --- |
| `splitContainerPreview.Panel1` | Preview 상단 조작 영역 | 선택 항목 제목, Go Live, 송출 후 다음, 항목 이동, 절/슬라이드 이동 |
| `PreviewPanelDisplayName` | 선택 Preview 제목/상태 | 현재 선택 항목과 소스 표시 |
| `PreviewInfo` | Preview 텍스트 정보 | 가사/성경/InfoScreen 정보 표시 |
| `flowLayoutPreviewLyrics` | Preview 가사 화면 | 절 이동, 클릭, 키보드 이동 |
| `flowLayoutPreviewPowerPoint` | Preview PPT 썸네일 | 선택 슬라이드 강조, 키보드 이동 |
| `PreviewBtnVerse1..Ending` | Preview 절 점프 버튼 | 1-9, PreChorus, Chorus, Bridge, Ending 직접 이동 |
| `PreviewBtnItemUp/Down` | Preview 항목 이동 | 이전/다음 예배 항목 |
| `PreviewBtnSlideUp/Down` | Preview 슬라이드/절 이동 | 이전/다음 절 또는 PPT 슬라이드 |
| `btnToLive`, `btnToOutput`, `btnToOutputMoveNext` | 송출 명령 | Preview에서 Output으로 전송 |
| `IndPanel` | 개별 서식 패널 | 현재 Preview 항목의 서식 편집 |
| `splitContainerPreview.Panel2` | 실제 Preview 화면 | FrmMain처럼 큰 미리보기 화면 유지 |

UI/UX 규칙:

- Preview는 "다음에 보낼 항목"이다.
- Output과 같은 데이터를 보더라도 시각적으로 별도 영역이어야 한다.
- Preview용 절/슬라이드 버튼과 Output용 절/슬라이드 버튼을 섞지 않는다.

### 4.5 오른쪽 Output 영역

| FrmMain 컨트롤 | WPF 1:1 목표 | 필수 동작 |
| --- | --- | --- |
| `splitContainerOutput.Panel1` | Output 상단 조작/상태 영역 | 현재 송출 항목, Black, Clear, live 상태 |
| `OutputPanelDisplayName` | 현재 Output 제목/상태 | 송출 중인 항목과 모니터 정보 |
| `OutputInfo` | Output 텍스트 정보 | 현재 송출 내용 상태 |
| `flowLayoutOutputLyrics` | Output 가사 상태 | 송출 중 절 표시와 직접 이동 |
| `flowLayoutOutputPowerPoint` | Output PPT 썸네일 | 송출 슬라이드 강조, 키보드 이동 |
| `OutputBtnVerse1..Ending` | Output 절 점프 버튼 | live 상태 직접 점프 |
| `OutputBtnItemUp/Down` | Output 항목 이동 | 송출 항목 이전/다음 |
| `OutputBtnSlideUp/Down` | Output 슬라이드 이동 | 송출 슬라이드 이전/다음 |
| `cbOutputBlack`, `cbOutputClear` | 위험 명령 | 눈에 띄는 고정 위치 |
| `OutputBtnRefAlert`, `OutputBtnMedia`, `OutputTextBoxLM` | 알림/미디어/라이브 메시지 | 별도 P1/P2로 명확히 이식 |
| `splitContainerOutput.Panel2` | 실제 Output 화면 | 큰 송출 화면 상태 유지 |

UI/UX 규칙:

- Output은 "회중에게 지금 나가는 항목"이다.
- Output 하단 큰 화면은 Preview 하단 큰 화면과 독립적으로 보여야 한다.
- Black, Clear, Restore, Stop Live는 오른쪽 Output 영역에서도 즉시 접근 가능해야 한다.

### 4.6 서식, 배경, 전환 UI

| FrmMain 그룹 | 원본 컨트롤 | WPF 1:1 목표 |
| --- | --- | --- |
| 개별 서식 | `Ind_Region`, `Ind_VAlign`, `Ind_R1Align`, `Ind_R2Align`, `Ind_Head` | Preview 선택 항목에 적용되는 고정 패널 |
| 개별 글꼴 | `Ind_Reg1FontsList`, `Ind_Reg2FontsList`, size/top/bottom/left/right numeric | 현재 항목 개별 서식 편집 |
| 개별 효과 | `Ind_Shadow`, `Ind_Outline`, `Ind_Interlace`, `Ind_Notations` | 토글 상태가 Preview에 즉시 반영 |
| 개별 배경 | `Ind_ImageMode`, `Ind_NoImage`, `Ind_BackColour`, `Ind_AssignMedia` | 배경 이미지/색상/미디어 연결 |
| 기본 서식 | `DefPanel`, `Def_*` | Default 탭에서 전역 기본값 편집 |
| 전환 | `Ind_TransItem`, `Ind_TransSlides`, `Def_TransItem`, `Def_TransSlides` | 항목/슬라이드 전환 구분 유지 |

UI/UX 규칙:

- 서식 패널은 보조 패널로 밀어낼 수 있지만, FrmMain에서 한 화면에 보이던 핵심 토글은 숨기지 않는다.
- 개별 서식과 기본 서식은 명확히 분리한다.
- Preview 항목 편집과 Output live 상태 변경은 분리한다.

## 5. 기능 1:1 매핑

### 5.1 데이터 로딩

| FrmMain 초기화 | WPF 필수 대응 |
| --- | --- |
| `Gf.InitAppData()` | `C:\EasiSlides` 실제 루트 초기화와 설정 반영 |
| `BuildFolderList()` | 곡 폴더와 `SongsList` 실제 로딩 |
| `PopulateWorshipList()` | 저장된 예배순서 목록 실제 로딩 |
| `LoadWorshipList(0/1/2)` | `.esw`, 템플릿, 세션 열기 구분 |
| `PopulatePraiseBooksList()` | Praise Book 목록 실제 로딩 |
| `BuildPicturesFolderList()` | Images 폴더 목록 실제 로딩 |
| `BuildInfoScreenFolderList()` | InfoScreen 폴더 목록 실제 로딩 |
| `BuildMediaFolderList()` | Media 폴더 목록 실제 로딩 |
| `Gf.LoadBibleVersions()` | Bible version 탭 실제 로딩 |
| `TabBibleVersionsChanged()` | Book list, passages, selection 상태 로딩 |

### 5.2 입력 동작

| 입력 종류 | FrmMain 기준 | WPF 목표 |
| --- | --- | --- |
| 목록 더블클릭 | 곡/PPT/Media/InfoScreen을 예배순서에 추가 또는 Preview | 동일 동작 |
| Enter | 검색/선택 목록에서 실행 | 동일 동작 |
| Delete | Worship List 선택 항목 삭제 | 동일 동작 |
| DragDrop | `DragDropSource` enum 기준 | 동일 소스 타입과 드롭 위치 삽입 |
| Context menu | Songs, Worship, PraiseBook, Bible, Images | 메뉴 항목별 대응표 필요 |
| Keyboard | `KeyboardActionHandler`, `ItemKeyPressed` | 동일 단축키와 포커스 예외 |

### 5.3 송출 흐름

| FrmMain 흐름 | WPF 목표 |
| --- | --- |
| 선택 소스 -> Preview | 선택 즉시 Preview 영역 갱신 |
| Preview -> Output | `btnToOutput`, `btnToLive` 동작 유지 |
| Preview -> Output -> Next | `btnToOutputMoveNext` 동작 유지 |
| Output slide move | live 항목만 이동, Preview와 분리 |
| Output item move | `LoadWorshipListItemToOutput` 대응 명령 |
| Black/Clear/Hide/Restore | Output 상태에 즉시 반영 |
| Refresh Output | Media/PPT/lyrics output 창 새로고침 |

## 6. 구현 단계

### Phase 0: 실측 인벤토리 재작성

산출물:

- `docs/wpf-migration/inventory/frmmain-control-event-inventory.md`
- `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md`

작업:

- `FrmMain.Designer.cs`의 private control 선언을 영역별로 분류한다.
- `+=` 이벤트 연결을 컨트롤별로 기록한다.
- `FrmMain.cs`, `Events.cs`, `Logic.cs`, `Layout.cs`의 이벤트 핸들러를 기능별로 묶는다.
- 각 행에 WPF 대응 위치, 누락 여부, 우선순위, 테스트 방법을 적는다.

완료 기준:

- 좌측 상단, 좌측 하단, Preview, Output, 메뉴/툴바, 단축키, 드래그 앤 드롭, 데이터 로딩이 모두 표로 존재한다.

### Phase 1: WPF Classic FrmMain Shell 재배치

작업:

- WPF `MainWindow`를 FrmMain split 계층과 동일하게 재정렬한다.
- 좌측 상단 `tabControlSource` 대응 탭 순서를 원본 순서로 맞춘다.
- 좌측 하단 `tabControlLists`를 원본과 동일한 운영 영역으로 고정한다.
- Preview와 Output의 상단/하단 영역을 원본처럼 분리한다.
- 오른쪽 큰 화면은 Preview 큰 화면과 Output 큰 화면을 독립적으로 표시한다.

완료 기준:

- 1920x1080과 1180x760에서 FrmMain 스크린샷과 영역 역할이 일치한다.
- "좌측 상단이 다르다", "좌측 하단이 다르다", "오른쪽이 다르다"가 컨트롤명 기준으로 해소된다.

### Phase 2: 실제 데이터 로딩 복구

작업:

- `C:\EasiSlides` 기준 WorkingFolder를 명확히 적용한다.
- Worship List `.esw` 로딩과 저장 목록 콤보를 FrmMain처럼 동작시킨다.
- Praise Book 목록과 항목을 실제 데이터에서 로드한다.
- Bible versions, books, passage lookup을 실제 DB/API 경로로 복구한다.
- Images, PowerPoint, InfoScreen, Media 폴더 목록을 실제 파일에서 로드한다.

완료 기준:

- 빈 껍데기 목록이 없어야 한다.
- 사용자가 제공한 `C:\EasiSlides` 데이터로 찬양, 예배순서, 성경, PPT, 이미지, 미디어가 보인다.

### Phase 3: 소스별 추가, Preview, Worship List 삽입

작업:

- `SongsList`, `BibleText`, `InfoScreenList`, `PowerpointList`, `MediaList`에서 Worship List로 추가한다.
- 더블클릭, Add 버튼, Enter, 컨텍스트 메뉴, 드래그 앤 드롭을 동일하게 구현한다.
- 드롭 위치 앞 삽입과 맨 끝 삽입을 FrmMain 기준으로 맞춘다.

완료 기준:

- 모든 소스에서 Worship List로 항목이 들어간다.
- 항목 선택 시 Preview가 바뀐다.
- 삽입 위치가 마우스 드롭 위치와 일치한다.

### Phase 4: Preview와 Output 조작 1:1 복구

작업:

- Preview 절 점프 버튼과 Output 절 점프 버튼을 각각 구현한다.
- Preview slide/item up/down과 Output slide/item up/down을 분리한다.
- `Go Live`, `To Output`, `To Output and Next`, `Restart`, `Refresh`, `Stop Live`를 원본 위치와 동작으로 맞춘다.
- PPT Preview 썸네일과 Output 썸네일 강조 상태를 각각 구현한다.

완료 기준:

- Preview 조작이 Output을 의도치 않게 바꾸지 않는다.
- Output 조작이 Preview 선택을 의도치 않게 바꾸지 않는다.
- PPT 1/4, 2/4 같은 상태가 오른쪽 영역에 정확히 표시된다.

### Phase 5: 서식/배경/전환/라이브 안전 명령

작업:

- `IndPanel`과 `DefPanel` 대응 UI를 완성한다.
- 배경 이미지, 색상, 이미지 모드, 미디어 연결을 원본 경로와 연결한다.
- Black, Clear, Hide, Restore, Close Output, Stop Live를 위험 명령으로 시각 구분한다.
- chord notation, capo, heading, region, alignment, shadow, outline, interlace를 반영한다.

완료 기준:

- 라이브 중 자주 쓰는 명령이 첫 화면에서 접근 가능하다.
- 세부 서식은 숨겨져 있더라도 원본 위치와 대응이 명확하다.

### Phase 6: 단축키와 포커스 규칙

작업:

- `KeyboardActionHandler` 기준 단축키 표를 WPF에 반영한다.
- 텍스트 입력 중에는 live 단축키가 오작동하지 않도록 포커스 예외를 둔다.
- 숫자/문자 절 점프, Space/Shift+Space, F12/F11/F9/F3, Ctrl 계열을 검증한다.

완료 기준:

- FrmMain 운영자가 키보드 습관을 바꾸지 않고 사용할 수 있다.

### Phase 7: 수동 UAT와 회귀 테스트

필수 시나리오:

1. Worship List 저장 목록을 열고 실제 `.esw` 항목이 표시되는지 확인한다.
2. Folders에서 찬양을 더블클릭해 Worship List에 추가하고 Preview로 본다.
3. Bible에서 구절을 조회하고 선택 구절을 Worship List에 추가한다.
4. PowerPoint를 추가하고 오른쪽 Preview/Output 썸네일과 큰 화면을 확인한다.
5. Images에서 배경을 적용하고 Preview/Output 반영을 확인한다.
6. Media를 추가하고 Preview/Output 제어를 확인한다.
7. Worship List 항목을 드래그로 재정렬한다.
8. Bible/PPT/Media/InfoScreen을 드래그로 Worship List 원하는 위치에 삽입한다.
9. Go Live, To Output and Next, Black, Clear, Hide, Restore, Restart, Refresh를 실행한다.
10. F12/F11/F9/F3/Space/Shift+Space/숫자 절 점프를 확인한다.

## 7. 우선순위

| 우선순위 | 범위 | 이유 |
| --- | --- | --- |
| P0 | 화면 계층, 실제 데이터 로딩, Worship List, Bible, Preview/Output 분리 | 지금 사용 불가라고 느끼는 원인 |
| P1 | 드래그 앤 드롭, 더블클릭, 컨텍스트 메뉴, PPT/Images/Media/InfoScreen | 예배 준비와 실시간 운영 필수 |
| P2 | 서식, 배경, 전환, chord/capo, 위험 명령 시각 정리 | 라이브 품질과 사고 방지 |
| P3 | Word/HTML export, 관리 창, 고급 편집, 시각 polish | 1:1 이후 안정화 |

## 8. 작업 원칙

- 한 번에 "전체를 예쁘게" 바꾸지 않는다.
- 각 Phase는 FrmMain 컨트롤명 기준 매핑표와 함께 시작한다.
- WPF 구현은 `MainWindow.xaml`에 무작정 추가하지 않고, 필요한 경우 composite 또는 ViewModel로 분리한다.
- 단, 사용자가 보는 위치와 즉시 접근성은 FrmMain을 우선한다.
- 각 Phase 완료 시 `dotnet test Easislides.Wpf.Tests`와 수동 실행 증거를 남긴다.
- "껍데기" 상태의 화면만 만든 커밋은 완료로 보지 않는다.

## 9. 다음 작업

가장 먼저 해야 할 일은 Phase 0이다. 기존 WPF 코드를 더 만지기 전에 다음 표를 완성한다.

| 산출물 | 포함해야 할 것 |
| --- | --- |
| `frmmain-control-event-inventory.md` | 모든 주요 컨트롤, 이벤트 핸들러, 원본 영역 |
| `frmmain-to-wpf-1to1-map.md` | 원본 컨트롤, WPF 위치, 상태, 누락 기능, 구현 Phase |
| `frmmain-manual-uat-checklist.md` | 사용자 시나리오별 수동 검증 절차 |

그 다음 Phase 1부터 구현한다. Phase 1은 레이아웃만 끝내는 것이 아니라, 각 영역이 실제 데이터와 다음 Phase 작업을 받을 수 있는 WPF 컴포넌트 경계까지 고정해야 한다.
