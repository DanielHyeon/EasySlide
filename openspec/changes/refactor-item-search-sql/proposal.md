# 곡 검색 SQL 생성 함수 정리 (refactor-item-search-sql)

## 왜
`Gf.BuildItemSearchString`(찾기 창과 빠른 검색이 쓰는 SONG 검색 SQL 생성)은 매개변수 16개, `text`~`text16` 같은 디컴파일 이름, 손으로 이어 붙이는 `" OR "`/`" AND "` 때문에 읽기 어려웠고, 다음 버그가 있었다.

1. 곡 번호 검색이 아무것도 찾지 못함: `"%221%"`를 숫자로 바꾸려 해서 항상 -1이 되고, 조건이 `song_number >= -1 and song_number <= -1`이 됨.
2. 날짜 검색이 SQL 오류: Access 문법 `#01-01-2020#`은 SQLite에서 `near "#01": syntax error`.
3. 검색 항목을 하나도 체크하지 않고 조·박자·악보만 등을 고르면 조건 사이 `AND`가 빠져 SQL 오류. 이때 폴더 조건도 빠짐.
4. 검색어에 `"`가 있으면 SQL 오류(따옴표 처리 없음).

## 무엇을
- `Easislides/Global/ItemSearchOptions.cs` 추가: 검색 옵션 묶음(기본값 = 빠른 검색).
- `BuildItemSearchString(string, ItemSearchOptions, CheckedListBox)`로 바꾸고 `gfUtility.cs`에서 `gfDatabase.cs`로 옮김. 1인자 판(빠른 검색)은 그대로.
- 조건을 리스트에 모아 `string.Join`으로 연결. 폴더 조건은 `FolderNo in (...)`로 한 번만, 항상 적용.
- 문자열은 작은따옴표 리터럴, 안의 따옴표는 두 번.
- 곡 번호: 양수만 `song_number = N`. "0"은 번호 조건을 만들지 않음(이전과 같은 결과).
- 날짜: `substr(LastModified, 1, 10) between 'yyyy-MM-dd' and 'yyyy-MM-dd'`(불변 문화권). 저장된 값이 `yyyy-MM-dd`, `yyyy-MM-dd HH:mm:ss`, ko-KR DataRow 경로의 `yyyy-MM-dd 오전 12:00:00` 중 무엇이어도 동작.
- 검색 항목이 있는데 조건이 하나도 안 나오면(곡 번호만 체크 + 숫자가 아닌 검색어) 결과 없음. 검색 항목 없이 결과를 내는 경우는 이전처럼 "악보만"뿐.

## 검증
- 수정 전·후 DLL로 같은 검색 34개의 SQL을 만들어 DB 사본에서 실행해 찾은 SongID를 비교: 27개 동일. 다른 7개는 위 버그 수정(번호·날짜·`"`·검색 항목 없음)과, 앱에서는 생기지 않는 앞뒤 공백 검색어 1개.
- 새 결과 수를 독립 쿼리로 대조: 번호 221 = 2, 2018–2019 날짜 = 85, 악보만 = 27.
- 앱 실행(찾기 창): 수정 전·후 빌드 모두 `0` 641곡, `사랑` 1132곡, `a"b` 0곡. DB·예배 목록·레지스트리 변화 없음.
- 별도 리뷰어 검토. 지적 4건(날짜 형식, "0" 처리, 곡 번호+악보만, 문화권 날짜)은 반영함.

## 이번에 고치지 않은 기존 버그 (별도 수정 대상)
- 새 곡 추가·폴더 이동 시 `LastModified`를 DataRow에 `DateTime`으로 넣어 문화권 문자열(`2026-11-18 오전 12:00:00`)로 저장됨: `gfDatabase.cs` AddDatabaseItem, `gfFolder.cs` 두 곳.
- `FrmExport.cs`의 날짜 조건도 Access `#...#` 문법.
- `FrmMain.Logic.cs`의 개수 쿼리가 `Replace("*", "Count(*)")`라서 검색어에 `*`가 있으면 깨짐.
- `FrmFind.cs` 시작 시 저장된 날짜를 `ToShortTimeString()`으로 복원해 날짜가 항상 오늘이 됨.
