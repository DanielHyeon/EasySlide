# Design: 찬양 목록을 번호순으로도 정렬한다

## 확인된 것 (2026-11-17)

- 목록 채우기: `FrmMain.FillList` (`FrmMain.Logic.cs`). 정렬은 SQL의 `order by`로 한다.
  - `Gf.UseSongNumbers`(전체 옵션)가 켜져 있으면 `order by song_number, cjk_strokecount`
  - 아니면 `CurStyle`에 따라 `order by cjk_strokecount`(제목순) 또는 `order by cjk_wordcount, cjk_strokecount`(글자 수순)
  - 개수 질의는 `str.Replace("*", "Count(*)")`로 만든다. 정렬 구문에 `*`가 들어가면 안 된다.
- 정렬 방식: `SortBy { Alpha, WordCount }` (`Easislides.Core/CommonEnum.cs`). 폴더마다 `Gf.FolderGroupStyle[]`에 있고 `FOLDER.GroupStyle`에 저장된다 (`gfFolder.cs` 읽기, `gfUtility.cs` 쓰기). 폴더를 바꾸면 `CurStyle`과 버튼 상태를 맞춘다 (`FrmMain.Logic.cs` 약 520행, `SetSortButton`).
- 버튼: `Folders_WordCount` (`ToolStripButton`, `CheckOnClick`, 툴팁 "Sort by CJK Word Count", `MouseUp`에서 처리). 현대 모습에서는 아이콘을 `FrmMain.ModernUi.Icons.cs`가 바꾼다. `ApplyUseSongNumbers`가 켜고 끈다.
- 같은 열을 WPF 앱도 읽고 쓴다: `LibraryViewModel.ResolveFolderGroupStyleSortMode`(1이면 글자 수순, 아니면 제목순), `AdminDatabaseRepository.NormalizeLegacyFolderGroupStyle`(저장할 때 값을 정리한다).
- 찬양집의 정렬(`PB_CJKGroupStyle`)은 0과 1만 받는다 (`gfLyrics.cs` 약 850행).
- 찾기 상자(D26)도 `FillList`로 다시 채운다.

## Decisions

### N1. 정렬 방식 `SortBy.Number`

`SortBy`에 `Number = 2`를 더한다. 기존 값의 숫자는 바꾸지 않는다 (DB에 저장된 값이다).

`SortBy`를 쓰는 다른 곳(찬양집, `GetCJKTitle`, 옵션 창)이 새 값을 만나도 지금의 제목순처럼 동작하는지 확인한다. 찬양집에는 새 값이 들어가지 않게 한다.

### N2. 번호순의 정의

- 곡 번호(`song_number`)가 1 이상인 곡을 번호 오름차순으로 먼저, 번호가 없는 곡(0 또는 NULL)은 그 뒤에 둔다.
- 번호가 같은 곡끼리, 그리고 번호가 없는 곡끼리는 제목순(`cjk_strokecount`)이다.
- SQL의 `order by`로 한다 (예: 번호 없음 여부, `song_number`, `cjk_strokecount`). 찾기 상자로 걸러진 목록도 같은 순서다.
- 전체 옵션 `UseSongNumbers`가 켜져 있을 때의 순서는 바꾸지 않는다.

### N3. 정렬 버튼을 메뉴로

폴더 정렬 버튼을 누르면 세 항목의 메뉴가 열린다. 지금 방식에 체크 표시가 있다.

| 항목 (영어 UI) | 방식 |
|---|---|
| Sort by Title | `Alpha` |
| Sort by CJK Word Count | `WordCount` |
| Sort by Song Number | `Number` |

- 항목을 고르면 그 폴더의 방식으로 저장하고(지금의 버튼과 같은 경로) 목록을 다시 채운다. 같은 방식을 다시 고르면 아무 일도 하지 않는다.
- 버튼의 툴팁은 지금 방식을 말한다. 현대 모습과 기존 모습 양쪽에서 동작한다 (기능이므로 모습 스위치에 묶지 않는다). 현대 모습의 아이콘 규칙(D18~D24: 아이콘 묶음, `ImageScaling`, 캐시)을 따른다.
- 구현 방법(버튼을 드롭다운 버튼으로 바꿀지, 버튼에 메뉴를 붙일지)은 구현에서 정한다. 조건: 디자이너 파일의 변경을 작게, `Folders_WordCount`를 참조하는 기존 코드(`ApplyUseSongNumbers`의 켜고 끄기, 아이콘 교체)가 계속 동작할 것.
- `UseSongNumbers`가 켜져 있으면 지금처럼 버튼이 꺼진다.

### N4. 저장과 다른 앱

- `FOLDER.GroupStyle`에 2를 저장한다. 읽을 때 0, 1, 2가 아닌 값은 제목순으로 본다.
- 옵션 창(`FrmOptions`)이 폴더 설정을 저장할 때 새 값을 잃지 않는지 확인한다.
- WPF 앱은 2를 제목순으로 보여 주면 된다. WPF 앱이 폴더를 저장할 때 2를 0이나 1로 바꾸는지 확인하고, 바꾼다면 그 사실을 적는다 (WPF 쪽은 이번에 고치지 않는다. 오류가 나면 고친다).

## 시험 방법

- **실제 자료를 바꾸지 않는다.** 메뉴에서 방식을 고르면 교회의 DB(`C:\EasiSlides\Admin\Database\EasiSlidesDb.db`의 `FOLDER.GroupStyle`)에 저장된다. 그래서 실제 앱에서 메뉴 항목을 누르는 시험은 하지 않는다 (사용자의 허락이 따로 있을 때만, DB 파일을 백업하고 되돌리는 방식으로).
- 순서의 확인: 읽기 전용 연결로 실제 DB에 같은 `order by`를 돌려 결과를 본다 (새찬송가: 1, 2, 3 … 645, 번호 없는 곡이 끝).
- 화면의 확인: 임시 스위치(환경 변수)로 저장 없이 번호순을 켠 채 앱을 띄워 캡처한다. 스위치는 끝나고 지운다.
- 메뉴를 고르는 경로와 저장은 코드로 확인한다. DB의 복사본을 쓰는 작은 시험 프로그램으로 저장과 다시 읽기를 확인할 수 있으면 한다.
- 솔루션 전체 빌드, WPF 테스트 중 `GroupStyle`과 정렬에 관한 것.

## 구현하면서 정한 것 (2026-11-17)

- `SortBy`에는 쓰이지 않던 세 번째 값 `SongNumber`(2)가 이미 있었다. 이름을 `Number`로 바꾸고 값을 명시했다 (`Alpha = 0, WordCount = 1, Number = 2`). 참조하던 코드는 없었다.
- 정렬 구문은 `FrmMain.SongListOrderClause`가 만든다. 번호순: `order by (case when song_number >= 1 then 0 else 1 end), (case when song_number >= 1 then song_number else 0 end), cjk_strokecount`. 제목순, 글자 수순, "Use Song Numbering"의 구문은 이전과 글자 하나까지 같다.
- 버튼은 `ToolStripButton` 그대로 두고(`CheckOnClick` 제거, `MouseUp` 대신 `Click`), 누르면 세 항목의 `ContextMenuStrip`을 연다. 기존 참조(`ApplyUseSongNumbers`, 아이콘 교체)가 그대로 동작한다. 버튼은 제목순이 아닐 때 눌린 모습이고 툴팁이 지금 방식을 말한다. 접근성 이름("Song list sort order")을 붙였다. 메뉴는 현대 모습의 컨텍스트 메뉴 목록에 넣어 모습 스위치를 따른다.
- 읽을 때 `Gf.ToFolderSortBy`가 0, 1, 2가 아닌 값을 제목순으로 본다. 옵션 창은 값을 정수 그대로 되돌려 쓰므로 2가 유지된다.
- **같이 고친 기존 버그**: "Search Results:"가 떠 있을 때 정렬을 바꾸면 그 방식이 1번 폴더에 저장되었다. 이제는 저장하지 않고 검색 결과만 새 순서로 다시 채운다. 다시 채울 때는 그 결과를 만든 검색 질의의 사본(`searchResultsSql`)을 쓴다 (`Gf.Find_SQLString`은 Find 창이 열리면 지워진다).
- 폴더 이름이 비어 있거나 알 수 없으면 정렬을 골라도 아무것도 바뀌지 않는다.

## 확인한 것

| 항목 | 결과 |
|---|---|
| 솔루션 전체 빌드 | 오류 0. `GroupStyle` 관련 WPF 테스트 4개 통과 |
| 새찬송가 번호순 (실제 DB, 읽기 전용) | 644곡: 641곡이 1→645, 번호 없는 3곡이 끝 |
| 구찬송가 번호순 | 549곡: 538곡이 번호순, 11곡이 끝 |
| 번호 없는 폴더 (English, 복음성가) | 제목순과 같음 |
| 찾기 상자와 함께 | 새찬송가 "주": 212곡이 12, 14, 16, 20 … 순 |
| 저장과 다시 읽기 (DB 복사본) | 2가 저장되고 다시 읽힘. 다른 폴더는 그대로. 7, -1은 제목순으로 읽힘 |
| 화면 (임시 스위치, 지움) | 번호순 목록, 열린 메뉴(번호순에 체크), 찾기 상자와 번호순 |

## Known Limitations

- **WPF 앱이 값을 되돌린다.** WPF 앱은 값 2를 제목순으로 보여 주고 오류는 없다. 그러나 WPF 앱에서 폴더를 저장하면(`AdminDatabaseRepository.NormalizeLegacyFolderGroupStyle`) 1이 아닌 값은 0이 되어 번호순 설정이 사라진다.
- 번호는 `SONG.song_number`를 쓴다. 제목 끝의 번호와 다르면 화면의 번호 열과 순서가 어긋나 보인다. 2026-11-17에 어긋나던 자료를 고쳤다 (아래 "자료 수정").
- 곡 번호가 없는 폴더에서는 번호순이 제목순과 같다.

## 자료 수정 (2026-11-17, 사용자 요청)

교회의 실제 DB(`C:\EasiSlides\Admin\Database\EasiSlidesDb.db`)의 `SONG` 13행을 고쳤다. 코드 변경이 아니며 저장소에는 이 기록만 남는다. 앱을 끈 상태에서 한 트랜잭션으로 적용했고, 적용 전후를 표 전체로 비교해 13행만 바뀐 것을 확인했다 (`pragma integrity_check`: ok). 바뀐 행의 `LASTMODIFIED`는 `2026-11-17`이다.

| SongID | 폴더 | 이전 | 이후 |
|---|---|---|---|
| 3718 | 구찬송가 | 제목 `천부여의지없어서38` (곡 번호 338) | 제목 `천부여의지없어서338` |
| 4244 | 구찬송가 | 제목 `주믿는형제들524` (곡 번호 525) | 제목 `주믿는형제들525` |
| 10161 | 일한찬양 | 곡 번호 0 (제목은 `…/1218`) | 곡 번호 1218 |
| 9398 | 새찬송가 | 제목 `주기도송` (곡 번호 635) | 제목 `주기도송635(새)` |
| 10061, 10062, 10064, 10066, 10065, 10067, 10068, 10071, 10072 | 중한찬양 | 제목 끝에 번호 없음 (곡 번호 560, 561, 568, 575, 577, 579, 592, 604, 606) | 제목 끝에 `/곡 번호` 추가 |

- 제목을 바꾼 행 중 정렬 키(`CJK_STROKECOUNT`)가 `000` + 제목이던 10행은 키도 새 제목으로 맞췄다 (9398은 `CJK_WORDCOUNT`도 4 → 6). 3718과 4244는 키가 원래 제목과 달라(옛 제목에서 만든 값) 그대로 두었다.
- 예배 목록 32개 중 이 곡들을 쓰는 것은 없다.
- 고친 뒤 찬송가 폴더(구찬송가, 새찬송가, 개역개정)와 일한찬양, 중한찬양에서 제목 끝 번호와 곡 번호가 다른 곡은 0이다.
- 고치지 않은 것: 번호가 원래 없는 항목(사도신경 등), DB에 없는 장(새찬송가 122, 292, 338, 412장, 구찬송가 21개 장), 시와찬미7의 `7001`식 표기.

같은 날 한 건을 더 고쳤다 (사용자 결정). 구찬송가에 221번이 두 곡 있었다: `나가난복지221`(SongID 4027)과 `나가난복지귀한성에221`(4126). 같은 찬송이고 가사 배치만 다르며, 둘 다 예배 목록에서 쓰이지 않았다. 제목이 온전하고 후렴 표시(`[chorus]`)가 있는 4126을 남기고, 4027은 앱의 삭제와 같은 방식으로 Recycle 폴더로 옮겼다 (`FOLDERNO` 1 → 0, `OLDFOLDER` 1, `LASTMODIFIED` 2026-11-17). 지워진 것이 아니라 앱에서 되살릴 수 있다. 구찬송가는 549곡에서 548곡이 되었다.

## 확인하지 못한 것 (코드로만 확인)

- 실제 앱에서 메뉴 항목을 고르는 동작과 그 저장 (실제 DB에 쓰므로 하지 않았다)
- 검색 결과를 새 순서로 다시 채우는 경로, Find 창을 열었다 닫은 뒤의 경우
- 기존 모습에서의 화면, 버튼이 넘침 영역에 있을 때의 메뉴 위치
