# Design: WPF 앱이 폴더의 정렬 방식을 지우지 않게 한다

## 확인된 것 (2026-11-17)

- 쓰기: `AdminDatabaseRepository.SaveFolder`가 `SongFolderWriteModel(FolderNo, Name, IsEnabled, GroupStyle = 0)`을 받아 `GroupStyle = NormalizeLegacyFolderGroupStyle(folder.GroupStyle)`로 쓴다 (1이면 1, 아니면 0). 있는 폴더는 UPDATE, 없는 폴더는 INSERT.
- 폴더 편집기: `FolderEditorViewModel`이 `new SongFolderWriteModel(FolderNo, Name.Trim(), IsEnabled)`으로 저장한다. 정렬 방식은 기본값 0이 들어간다.
- 글자 수순 토글: `LibraryViewModel.PersistSelectedFolderGroupStyleAsync`가 0 또는 1을 저장한다.
- 읽기: `LibraryViewModel.ResolveFolderGroupStyleSortMode`는 1이면 `WordCount`, 아니면 `StrokeCount`. 전체 옵션 `UseSongNumbering`이 켜져 있으면 `Number`.
- WinForms 쪽의 뜻: 0 제목순, 1 글자 수순, 2 번호순, 그 밖의 값은 제목순 (`Gf.ToFolderSortBy`).
- 테스트: `Easislides.Wpf.Tests`의 `FolderEditorViewModelTests`(쓰기 모델의 동등 비교), `AdminDatabaseRepositoryTests`(임시 DB), `LibraryViewModel` 관련 테스트.

## Decisions

### W1. 저장할 때 0, 1, 2를 지킨다

`NormalizeLegacyFolderGroupStyle`은 0, 1, 2를 그대로 돌려주고 그 밖의 값만 0으로 한다.

### W2. 폴더 편집기는 정렬 방식을 건드리지 않는다

쓰기 모델에서 "정렬 방식을 바꾸지 않는다"를 표현할 수 있게 한다 (예: `GroupStyle`을 `int?`로 하고 기본값 `null`). 있는 폴더를 `null`로 저장하면 저장된 값을 그대로 둔다 (UPDATE에서 그 열을 빼거나 저장된 값을 다시 쓴다). 새 폴더를 `null`로 저장하면 0이다. 글자 수순 토글처럼 값을 주는 호출은 그 값을 쓴다.

기존 테스트의 `new SongFolderWriteModel(3, "Evening Revised", false)` 같은 비교가 그대로 통과하는 형태를 고른다.

### W3. 값 2는 곡 번호순으로 보여 준다

`ResolveFolderGroupStyleSortMode`가 2를 `LibrarySortMode.Number`로 돌려준다. 글자 수순 토글은 번호순 폴더에서 꺼진 것으로 보이고, 켜면 1, 다시 끄면 0이 저장된다 (지금의 규칙 그대로).

WPF의 번호순(`SongOrdering`의 `Number`)이 번호 없는 곡을 어디에 두는지 확인한다. WinForms와 다르면(번호 없는 곡이 앞에 오는 등) 그 차이를 적고, 폴더의 번호순(값 2)일 때는 WinForms와 같은 순서(번호 있는 곡 먼저, 없는 곡은 뒤에 제목순)가 되게 할 수 있는지 본다. 전체 옵션 `UseSongNumbering`의 순서는 바꾸지 않는다.

## 시험 방법

- 단위 테스트를 더한다: 저장 뒤 0/1/2가 그대로인지, 폴더 편집기로 이름을 바꿔도 1과 2가 남는지, 새 폴더는 0인지, 값 2인 폴더가 번호순으로 열리는지.
- 테스트는 임시 폴더의 DB만 쓴다. `C:\EasiSlides`나 레지스트리를 건드리는 테스트는 돌리지 않는다 (돌리기 전에 읽어서 확인한다).
- WPF 앱 자체는 띄우지 않는다 (실제 자료를 쓴다).

## 구현하면서 정한 것 (2026-11-17)

- `SongFolderWriteModel.GroupStyle`은 `int?`이고 기본값이 `null`이다. `null`은 "정렬 방식을 바꾸지 않는다"는 뜻이다.
  - 있는 폴더를 `null`로 저장하면 UPDATE 문에서 `GroupStyle` 열을 뺀다. 저장된 값이 그대로 남는다.
  - 새 폴더를 `null`로 저장하면 0을 쓴다.
  - 값을 주면 정리해서 쓴다 (0, 1, 2는 그대로, 그 밖의 값은 0).
- 폴더 편집기의 코드는 바뀌지 않았다 (주석만). 이제 `null`을 보내게 되어 정렬 방식을 건드리지 않는다.
- `ResolveFolderGroupStyleSortMode`: 1은 `WordCount`, 2는 `Number`, 그 밖은 `StrokeCount`.
- 번호 없는 곡의 자리는 WPF의 `Number` 순서가 이미 WinForms의 번호순과 같았다 (번호 있는 곡 먼저, 없는 곡은 뒤).
- 폴더를 고르는 것만으로는 아무것도 저장하지 않는다. 글자 수순 토글은 번호순 폴더에서 꺼진 것으로 보이고, 누르면 1, 다시 끄면 0이 저장된다.

## 확인한 것

- 솔루션 전체 빌드 오류 0.
- 테스트 143개 통과 (`FolderEditor`, `AdminDatabaseRepository`, `LibraryViewModel`, `SongOrdering` 관련. 새 테스트 14개 포함). 저장소 테스트는 임시 폴더의 실제 SQLite DB를 쓴다.
  - 저장 뒤 0, 1, 2가 그대로인지, 범위 밖의 값(3, -1, 99)이 0이 되는지
  - 값을 주지 않은 저장이 1과 2를 남기는지, 새 폴더가 0인지
  - 실제 폴더 편집기로 이름을 바꾸고 끄기를 해도 1과 2가 남는지
  - 값 2인 폴더가 번호순으로 열리고 번호 없는 곡이 뒤에 오는지

## Known Limitations

- **번호가 같은 곡과 번호 없는 곡의 순서가 두 앱에서 조금 다르다.** WPF는 제목의 글자 코드순, WinForms는 `cjk_strokecount`순이다. WPF 앱이 저장한 곡은 그 값이 `"000"` + 제목이라 대체로 같지만, WinForms가 예전에 넣은 값이나 비어 있는 값이 있으면 달라진다. 맞추려면 WPF의 정렬에 새 방식을 더해야 해서 하지 않았다.
- **전체 옵션 "Use Song Numbering"이 켜져 있을 때의 순서도 두 앱이 다르다** (이 변경 전부터). WinForms는 번호 없는 곡(0)이 앞에 오고, WPF는 뒤에 온다.
- 글자 수순 토글을 끄면 0(제목순)이 저장된다. 그때의 상태 문구는 예전 그대로 "CJK Stroke Count"다.

## 확인하지 못한 것

- WPF 앱을 띄워서 본 동작 (실제 자료를 쓰므로 띄우지 않았다). 폴더를 바꿀 때 글자 수순 체크 상자의 양방향 바인딩이 값을 되써 넣지 않는지는 코드로만 확인했다.
- `GroupStyle` 열이 없는 DB에 새 폴더를 만드는 경우 (그 경로의 코드는 바뀌지 않았다).
