# Proposal: WPF 앱이 폴더의 정렬 방식을 지우지 않게 한다

## Why

폴더의 정렬 방식은 `FOLDER.GroupStyle`에 저장되고 WinForms 앱과 WPF 앱이 같이 쓴다. `add-song-number-sort`로 값 2(번호순)가 생겼다. WPF 앱은 이 값을 두 군데에서 잃는다.

- `AdminDatabaseRepository.NormalizeLegacyFolderGroupStyle`이 1이 아닌 값을 모두 0으로 저장한다. 2가 0이 된다.
- 폴더 편집기(`FolderEditorViewModel`)는 이름과 사용 여부만 다루는데, 저장할 때 정렬 방식의 기본값 0을 같이 써 버린다. 번호순(2)뿐 아니라 글자 수순(1)도 0으로 돌아간다. 이것은 값 2가 생기기 전부터 있던 문제다.

또 WPF 앱은 값 2인 폴더를 제목순으로 보여 준다. WPF 앱에는 이미 "곡 번호순"(`LibrarySortMode.Number`)이 있다.

## What Changes

- WPF 앱이 폴더를 저장할 때 0, 1, 2를 그대로 쓴다.
- 폴더 편집기의 저장은 정렬 방식을 건드리지 않는다.
- 값 2인 폴더를 열면 곡 번호순으로 보여 준다.

## Out of Scope

- WPF 앱에 폴더별 정렬을 고르는 새 UI를 만드는 것 (지금 있는 글자 수순 토글과 보기용 정렬 선택은 그대로).
- WinForms 앱.

## Impact

- `Easislides.Wpf/Data/AdminDatabaseRepository.cs`, `Easislides.Wpf/Library/LibraryViewModel.cs`, `FolderEditorViewModel.cs`, `Easislides.Wpf.Tests`
