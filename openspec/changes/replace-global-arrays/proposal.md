# Proposal: 전역 배열을 이름 있는 형식으로 바꾼다

작성 2026-11-17 (2판: 2~7번을 구체화). 줄 번호와 수는 v2.6.4의 `4e18207`에서 센 것이다 (Designer 파일 제외). 줄 번호는 앞 단계를 하면 밀리므로 이름으로 다시 찾는다.

## Why

WinForms 앱에는 배열로 된 필드가 172개 있다. 81개가 전역(static)이고 56개가 2차원 이상이며, 73개가 `Global/gfConstants.cs`에 있다.

- 열 번호가 뜻을 숨긴다 (`WorshipSongs[i, 1] == "P"`).
- 크기가 고정이라 넘치면 잘리거나 예외가 난다 (`new string[1000, 2]`).
- 개수를 따로 든 변수와 배열이 어긋날 수 있다.
- 한 가지 대상의 정보가 여러 배열에 나뉘어 있다 (폴더 하나가 배열 24개).

## 원칙

1. **배열 하나에 커밋 하나.** 커밋마다 솔루션 전체가 빌드되고 앱이 뜬다. 어느 단계에서 멈춰도 된다.
2. **동작은 바꾸지 않는다.** 형식만 바꾼다. 번호가 1부터인 것은 1부터 그대로 둔다. 버그를 발견하면 따로 커밋한다.
3. **컴파일러에게 찾게 한다.** 선언의 형식을 먼저 바꾸면 쓰는 곳이 모두 오류로 뜬다. 그 목록을 하나씩 고친다.
4. **개수 변수는 마지막에 없앤다.** 형식을 바꾸는 커밋에서는 남겨 두고, 다음 커밋에서 목록의 `Count`로 바꾼다.

## 공통으로 걸리는 것

- **`ref`로 넘기는 칸이 있다.** 예: `DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[CurrentSong, 4], …)`. 속성은 `ref`로 넘길 수 없으므로 새 형식의 멤버는 **속성이 아니라 필드**로 둔다. (필드는 `ref item.Field`가 된다.)
- **`struct`를 `List`에 넣으면 제자리에서 못 고친다.** `list[i].Start = x`는 컴파일되지 않는다 (CS1612). 칸을 나중에 고쳐 쓰는 자료(구절 위치, 쇼 복사본)는 `class`로 만든다.
- **`null`과 `""`.** 배열의 빈 칸은 `null`이었다. 새 형식에서 기본값을 `""`로 두면 `== null` 검사나 `?? ""`가 뜻을 잃는다. 고치면서 그런 검사가 있으면 `string.IsNullOrEmpty`로 본다.
- **0번 칸.** 1부터 쓰는 배열은 0번 칸을 비워 두거나 개수를 넣어 둔다 (`HB_VersesLocation[0, 0]`). 새 형식에서도 번호를 그대로 쓰게 인덱서를 만든다. 번호를 0부터로 바꾸는 일은 하지 않는다.
- **배열을 받는 도우미.** 여러 배열이 같은 도우미를 쓴다 (`RemoveDuplicateEditorHistoryItems(ref string[,], ref int)`, `BuildSubFolderList(…, ref string[,], ref int)`). 도우미의 매개변수 형식을 바꾸면 그 도우미를 쓰는 배열을 같은 커밋에서 같이 바꿔야 한다.

## 순서

| 순서 | 대상 | 규모 | 위험 |
|---|---|---|---|
| 1 | `WorshipSongs` | 79줄, 7개 파일 | 낮음 |
| 2 | `DocumentSongs` | 33줄, 4개 파일 | 낮음 |
| 3 | `HB_VersesLocation` | 55줄, 5개 파일 | 낮음 |
| 4 | 편집 기록 목록 4개 | 69줄, 8개 파일 | 낮음 |
| 5 | `PowerpointList` + `TotalPowerpointItems` | 135줄, 7개 파일, OfficeLib까지 | **높음** |
| 6 | 폴더 설정 배열 24개 | 약 610줄, 25개 파일 | 중간 (양이 많다) |
| 7 | 크기 고정만 남은 것 3개 | 30줄 | 낮음 |

**오늘 안에 하기 좋은 범위는 1~4번과 7번이다.** 5번은 이번 주에 여러 번 고친 PPT 송출 경로 한가운데라 따로 하루를 잡는다. 6번은 줄 수가 가장 많고 옵션 창과 얽혀 있다.

---

## 1. `WorshipSongs` (예배 목록)

지금: `public static string[,] WorshipSongs = new string[20000, 5];` (`gfConstants.cs` 1835행). 번호는 1부터, 개수는 `TotalWorshipListItems`(33줄).

| 열 | 뜻 | 쓰는 곳 |
|---|---|---|
| 0 | ID 문자열: 종류 글자 + 곡 ID 또는 경로 (`"D123"`, `"P" + 경로`) | 22 |
| 1 | 종류 글자 (`D`, `P`, `B`, `T`, `I`, `W`, `M` …) | 12 |
| 2 | 제목 (음표 표시를 뗀 것) | 17 |
| 3 | 채울 때는 `" "`. 문서 만들기에서 숫자로도 읽는다 (2번의 3번 열로 복사된다) | 8 |
| 4 | 서식 문자열 (항목별 서식) | 15 |

채우는 곳: `FrmMain.cs` 5004~5061행 근처 (종류별로 여러 군데).

```csharp
// Global/WorshipItemList.cs (new)
public sealed class WorshipItem
{
    public string IdString = "";     // column 0
    public string Type = "";         // column 1
    public string Title = "";        // column 2
    public string Group = "";        // column 3 (name it after confirming what the readers expect)
    public string FormatString = ""; // column 4
}

/// <summary>Items of the loaded worship list. The index is the item number, starting at 1, as before.</summary>
public sealed class WorshipItemList
{
    private readonly List<WorshipItem> items = new List<WorshipItem> { new WorshipItem() }; // [0] unused
    public int Count => items.Count - 1;
    public WorshipItem this[int itemNumber]
    {
        get
        {
            while (items.Count <= itemNumber) items.Add(new WorshipItem());
            return items[itemNumber];
        }
    }
    public void Clear() { items.Clear(); items.Add(new WorshipItem()); }
}
```

절차:

1. 선언을 `public static WorshipItemList WorshipSongs = new WorshipItemList();`로 바꾼다.
2. 오류가 뜨는 곳을 `Gf.WorshipSongs[i, 2]` → `Gf.WorshipSongs[i].Title` 식으로 고친다.
3. 배열을 받는 메서드를 고친다: `PreLoadPowerpointFiles(ref PowerPoint InPP, ref string[,] InSongsArray)` (`gfFileHelpers.cs`) 등. `ref`를 뗀다.
4. 빌드, 확인, 커밋.
5. 다음 커밋: 목록을 다시 채우는 곳에서 `Clear()`를 부르게 하고 `TotalWorshipListItems`를 `Count`로 바꾼다. 지금 코드가 "개수는 줄였지만 뒤 칸은 그대로 둔" 상태를 읽는 곳이 없는지 먼저 본다.

주의: 인덱서가 없는 번호를 만나면 빈 항목을 만들어 준다. 지금 코드가 채우지 않은 칸을 읽기도 하기 때문이다 (`for (i = 0; i <= Gf.TotalWorshipListItems; i++)`처럼 0번부터 도는 곳이 있다).

확인: 앱 시작, 예배 목록 표시, 항목을 미리보기로, 쇼 켜고 항목 넘기기 (가사, 성경, PPT), 가사 화면(`FrmLyricsScreen`) 열기.

## 2. `DocumentSongs` (문서 만들기)

지금: `public static string[,] DocumentSongs = new string[20000, 5];` (1837행). 번호는 1부터. 찬양집과 예배 목록을 문서(RTF, HTML)로 만들 때 쓴다.

| 열 | 뜻 | 근거 |
|---|---|---|
| 0 | ID 문자열 (종류 글자 + 곡 ID) | `FrmGenerateHtml.cs` 146행이 첫 글자를 떼고 `songid`로 쓴다 |
| 1 | 종류 글자 | `FrmGenerateDoc.cs` 938행 |
| 2 | 제목 | 945~972행 |
| 3 | 찾아보기의 묶음 값: 글자 수 (숫자 문자열) 또는 `" "`로 시작하는 글자 | 623~640행이 `Convert.ToInt32`로 읽고 16, 17과 비교한다 |
| 4 | `곡 번호>그 밖의 정보`. `>` 앞이 곡 번호 | 332, 384, 631행, `FrmGenerateHtml.cs` 310행 |

채우는 곳 세 군데:

- `FrmMain.cs` 1259~1261행: 찬양집에서 (0, 2, 4번 열만 채운다. 1, 3번은 이전 값이 남는다)
- `FrmMain.cs` 2794~2798행: 예배 목록(`WorshipSongs`)에서 다섯 열을 복사
- `FrmMain.cs` 7276~7280행: 찬양집 항목에서 다섯 열

```csharp
// Global/DocumentItemList.cs (new)
public sealed class DocumentItem
{
    public string IdString = "";      // column 0
    public string Type = "";          // column 1
    public string Title = "";         // column 2
    public string IndexGroup = "";    // column 3
    public string NumberAndInfo = ""; // column 4: "<song number>><info>"; passed by ref to ExtractOneInfo
}
// DocumentItemList: the same shape as WorshipItemList (index from 1, grows on demand).
```

절차와 주의:

1. `WorshipItemList`와 모양이 같다. 두 번째로 만들 때 공통 부모(`OneBasedList<T>`)로 뽑는다. 1번을 먼저 커밋했다면 이 단계에서 `WorshipItemList`도 그 부모를 쓰게 고친다.
2. `ref Gf.DocumentSongs[CurrentSong, 4]`가 두 군데 있다 (332, 631행). `ref Gf.DocumentSongs[CurrentSong].NumberAndInfo`로 바뀐다. 그래서 필드여야 한다.
3. 1259~1261행은 세 열만 채운다. 새 형식에서도 **나머지 두 필드를 건드리지 않는다** (이전 값이 남는 것이 지금의 동작이다). 이상해 보여도 이 커밋에서는 고치지 않는다.
4. 2794~2798행은 1번을 먼저 했다면 `Gf.WorshipSongs[i + 1].Title` 식으로 이미 바뀌어 있다.
5. 개수 변수는 채우는 반복문의 상한에서 찾는다 (찬양집 항목 수, 예배 목록 항목 수).

확인: 찬양집 탭에서 문서 만들기 창(`FrmGenerateDoc`)과 HTML 만들기 창(`FrmGenerateHtml`)을 열어 만들어 본다. 결과 파일이 이전 빌드의 것과 같은지 비교하면 가장 확실하다 (같은 찬양집으로 전후 두 번 만들어 파일 비교).

## 3. `HB_VersesLocation` (성경 구절 위치)

지금: `public static int[,] HB_VersesLocation = new int[3001, 7];` (1430행). 성경 본문 상자(`BibleText`)에 보이는 구절들의 위치표다. `[0, 0]`에 개수를 넣고 1번부터 쓴다. 6번 열은 쓰는 곳이 없다.

| 열 | 뜻 |
|---|---|
| 0 | 성경 판 번호 (0번 행에서는 개수) |
| 1 | 책 번호 |
| 2 | 장 |
| 3 | 절 |
| 4 | 본문 문자열에서의 시작 위치 |
| 5 | 길이 |

채우는 곳: `gfBible.cs` 264~285행 (장 읽기), 685~693행 (검색 결과). 고쳐 쓰는 곳: 319~338행 (구절 목록을 다시 그릴 때 4, 5번 열), 362~364행 (판 번호를 일괄 변경). 읽는 곳: `FrmMain.cs` 2464~2496행, `FrmMain.Logic.cs` 1239~1364행 (23줄), `FrmMain.Layout.cs` 976행 (상태 줄의 개수).

```csharp
// Global/BibleVerseList.cs (new)
public sealed class VerseLocation   // a class: entries are changed in place after they are added
{
    public int Version, Book, Chapter, Verse, Start, Length;
}

/// <summary>Verses shown in the Bible text box, numbered from 1 as before.</summary>
public sealed class BibleVerseList
{
    private readonly List<VerseLocation> items = new List<VerseLocation> { new VerseLocation() }; // [0] unused
    public int Count => items.Count - 1;
    public VerseLocation this[int number] => items[number];
    public VerseLocation Add() { var v = new VerseLocation(); items.Add(v); return v; }
    public void Clear() { items.Clear(); items.Add(new VerseLocation()); }
}
```

절차와 주의:

1. `HB_VersesLocation[0, 0]`(개수)을 읽는 곳 9군데는 `Gf.HB_Verses.Count`가 된다. **개수를 쓰는 곳**(285, 693행의 `[0, 0] = num`)은 지운다. 그 대신 채우기 전에 `Clear()`를 부른다.
2. 채우는 곳은 `HB_VersesLocation[num2, 0] = …` 여섯 줄이 `var v = Gf.HB_Verses.Add(); v.Version = …`가 된다. `num2`를 증가시키는 줄과 순서를 맞춘다 (번호 = 추가 순서 + 1).
3. `FrmMain.Logic.cs` 1250, 1253행의 `j = Gf.HB_VersesLocation[0, 0];`은 반복문을 끝내려고 개수를 넣는 것이다. `Count`로 바꾼다 (`break`로 고치고 싶어도 이 커밋에서는 그대로).
4. 채우는 반복문에 3000 상한 검사가 있는지 본다. 있으면 그대로 둔다 (없애면 긴 검색 결과가 더 길어지는 동작 변화다. 하려면 따로 커밋).
5. 인덱서는 없는 번호에서 예외를 낸다 (1번과 다르다). 지금 코드가 `Count`보다 큰 번호를 읽지 않는지 고치면서 본다. 읽는 곳이 있으면 그것은 지금도 0을 읽는 버그다.

확인: 성경 탭에서 책과 장을 골라 본문 표시, 구절을 드래그해 선택했을 때 제목 줄이 맞게 나오는지 (`FrmMain.Logic.cs` 1293~1364행), 성경 검색, 상태 줄의 "Verses listed: n".

## 4. 편집 기록 목록

지금 (모두 `string[AbsoluteMaxHitoryItems + 1, 2]`, 최대 20, 번호는 1부터):

| 배열 | 개수 변수 | 레지스트리 키 | 쓰는 창 |
|---|---|---|---|
| `MainEditHistoryList` (21줄) | `TotalMainEditHistory` | `maineditlist` | 메인 화면 (`FrmMain.Logic.cs` 2897~2951) |
| `EditorEditHistoryList` (19줄) | `TotalEditorEditHistory` | `editoreditlist` | 편집기 (`FrmEditItem.cs` 1226~1275) |
| `InfoScreenEditHistoryList` (19줄) | `TotalInfoScreenEditHistory` | `infoscreeneditlist` | 정보 화면 (`FrmInfoScreen.cs` 1282~1331) |
| `TempEditHistoryList` (10줄) | 없음 | 없음 | 중복 제거의 임시 공간 (`gfUtility.cs` 1572~1602) |

0번 열은 항목 ID 문자열(종류 글자 + ID), 1번 열은 제목이다 (메뉴를 그릴 때 채운다). 레지스트리에는 0번 열만 번호별로 저장한다 (`gfUtility.cs` 1430~1482 읽기, 1555~1641 쓰기).

배열을 받는 도우미: `Gf.RemoveDuplicateEditorHistoryItems(ref string[,], ref int)` (`gfUtility.cs`), `ValidateHistoryItems(ref string[,], ref int, int, bool)` (`gfConfig.cs` 975~985).

```csharp
// Global/EditHistory.cs (new)
public sealed class EditHistoryEntry
{
    public string Id = "";    // column 0
    public string Title = ""; // column 1
}

/// <summary>A recent-edits list, numbered from 1; always holds Capacity entries, as the arrays did.</summary>
public sealed class EditHistoryList
{
    public const int Capacity = 20; // Gf.AbsoluteMaxHitoryItems
    private readonly EditHistoryEntry[] items = new EditHistoryEntry[Capacity + 1];
    public EditHistoryList() { for (int i = 0; i < items.Length; i++) items[i] = new EditHistoryEntry(); }
    public EditHistoryEntry this[int number] => items[number];
}
```

두 커밋으로 나눈다.

**4a. 형식만 바꾼다.** 배열 네 개를 `EditHistoryList`로 바꾸고, 도우미 두 개의 매개변수를 `EditHistoryList`로 바꾼다 (`ref`를 뗀다. 개수의 `ref int`는 남긴다). 여기서는 칸 수가 정해져 있고 뒤 칸을 `""`로 지우는 코드가 있으므로, 목록이 아니라 **고정 칸 배열을 감싼 형식**이 동작을 그대로 지킨다. `[i, 0] = [i - 1, 0]`처럼 칸을 미는 코드는 `list[i].Id = list[i - 1].Id`가 된다 (객체를 옮기지 말고 값을 복사한다. 객체를 옮기면 두 칸이 같은 객체를 가리킨다).

**4b. (선택) 세 창의 같은 코드를 형식 안으로 옮긴다.** `FrmMain.Logic.cs`, `FrmEditItem.cs`, `FrmInfoScreen.cs`에 같은 코드가 세 벌 있다 (맨 앞에 넣기, 중복 제거, 제목 채우기, 메뉴 그리기). `Push(id)`, `RefreshTitles(Func<string, string> getTitle)`로 옮기면 `TempEditHistoryList`와 개수 변수 세 개가 없어진다. 세 벌이 정말 같은지 줄 단위로 비교한 뒤에 한다 (`FrmEditItem.cs` 1273행은 `i < Max`, `FrmInfoScreen.cs` 1329행은 `i <= Max`로 다르다. 이런 차이는 동작이므로 그대로 옮기거나, 고친다면 따로 커밋한다).

주의: `FrmMain.ModernUi.FormatIcons.cs` 235행이 `Gf.MainEditHistoryList[i, 0]`을 읽는다 (최근 편집 메뉴의 아이콘).

확인: 곡을 편집기로 열었다 닫으면 Edit 메뉴의 최근 편집 목록 맨 위에 오는지, 같은 곡을 다시 열면 중복되지 않는지, 앱을 껐다 켜도 목록이 남는지. 편집기와 정보 화면의 목록도 같은 식으로. **레지스트리에 저장되는 설정이니**, 시험 전에 `HKCU\Software\EasiSlides`를 내보내 두었다가 끝나고 되돌린다.

## 5. `PowerpointList` + `TotalPowerpointItems` (쇼의 PPT 복사본)

지금: `public static string[,] PowerpointList = new string[1000, 2];`, `public static int TotalPowerpointItems;` (`gfConstants.cs` 1947~1949행). 번호는 1부터. 0번 열은 원본 파일 경로, 1번 열은 PowerPoint 안의 복사본 이름(`FullName`). 같은 모양이 하나 더 있다: `gfPowerPoint.cs`의 `pendingShowCopies` / `pendingShowCopyCount` (쇼 끝에서 닫지 못한 복사본).

`OfficeLib/PowerPoint.cs`에서 이 쌍을 받는 메서드 10개:

| 메서드 | 받는 방식 | 하는 일 |
|---|---|---|
| `Open` (85행) | `ref`, `ref` | 복사본을 열고 행을 더한다 (140~144행). 109행에서 개수를 0으로 하는 경우가 있다 |
| `Run` (183행) | `ref`, `ref` | 경로로 복사본 이름을 찾아 슬라이드 쇼를 실행 |
| `GetPresentationName` (1015행) | 값 | 경로 → 복사본 이름 |
| `FindListedEntry` (1038행) | 값 | 경로 → 행 번호 |
| `GetShowCopyPath` (1091행) | 값 | 열린 프레젠테이션이 목록의 복사본인지 (이름과 표식) |
| `ClearStaleRowsWithName` (1131행) | 값 | 같은 이름의 옛 행의 이름을 `""`로 (1138행) |
| `FollowRunOf` (1355행) | 값 | 다른 래퍼의 실행을 따라간다 |
| `ResSetAllShowWindows` (1890행) | 값 | 쇼 창을 줄인다 |
| `ClearUpPowerpointWindows` (1943, 1953행) | `ref`, `ref` | 복사본을 닫고, 포기되지 않았으면 개수를 0으로 (2060행) |

```csharp
// OfficeLib/ShowCopyList.cs (new)
public sealed class ShowCopy          // a class: CopyName is cleared in place (ClearStaleRowsWithName)
{
    public string SourcePath = "";    // column 0
    public string CopyName = "";      // column 1
}

/// <summary>The copies the app opened in PowerPoint for the show. Rows are numbered from 1 and never move.</summary>
public sealed class ShowCopyList
{
    private readonly List<ShowCopy> rows = new List<ShowCopy> { new ShowCopy() }; // [0] unused
    public int Count => rows.Count - 1;
    public ShowCopy this[int row] => rows[row];
    public int Add(string sourcePath, string copyName) { rows.Add(new ShowCopy { SourcePath = sourcePath, CopyName = copyName }); return Count; }
    public void Clear() { rows.Clear(); rows.Add(new ShowCopy()); }
}
```

절차:

1. `ShowCopyList`를 OfficeLib에 만든다 (`PowerPoint.cs`가 쓰므로 Easislides 쪽이 아니다).
2. `Gf.PowerpointList`의 형식을 바꾸고 `Gf.TotalPowerpointItems`를 지운다. 이번에는 **개수 변수를 같은 커밋에서 없앤다**: 메서드 10개가 둘을 쌍으로 받기 때문에 따로 떼면 중간 상태가 더 위험하다.
3. 메서드 10개의 매개변수 두 개를 `ShowCopyList list` 하나로 바꾼다. 본문에서 `TotalPowerpointItems`는 `list.Count`, `PowerpointList[i, 0]`은 `list[i].SourcePath`.
4. `TotalPowerpointItems = 0` 세 군데(109, 2060행, `gfPowerPoint.cs` 861행)는 `list.Clear()`가 된다. **109행은 조건을 그대로 옮긴다** (어떤 경우에 0으로 하는지 읽고).
5. `TotalPowerpointItems++`(144행)는 `list.Add(…)`가 한다. 행 번호(`row`)를 미리 계산해 쓰는 코드가 있으면 `Add`의 반환값과 같은지 확인한다.
6. `pendingShowCopies`도 `ShowCopyList`로 바꾼다. `KeepShowCopiesForLater`(`gfPowerPoint.cs` 약 840~861행)는 행을 옮긴 뒤 원래 목록을 비운다. 1000행 상한 검사(`pendingShowCopyCount < GetUpperBound(0)`)는 없어진다.

절대 바꾸지 않을 규칙 (형식만 바꾸면서 실수하기 쉬운 곳):

- `ClearUpPowerpointWindows`: 호출 묶음이 **포기되면 목록을 그대로 둔다.** 아니면 비운다.
- `ClearStaleRowsWithName`: 행을 지우지 않고 이름만 비운다. 행 번호가 밀리면 안 된다 (`RememberRunPresentation(fullName, i, …)`이 행 번호를 기억한다).
- `GetShowCopyPath`: 이름 **그리고** 표식이 모두 맞아야 복사본이다.
- 목록의 행 순서 = 연 순서. 정렬하거나 중복을 합치지 않는다.

확인 (반드시 전부): 쇼 시작 → PPT 항목 → 슬라이드 넘김 → 다른 PPT 항목 → 가사 항목 → 처음 PPT 항목으로 돌아감 → 쇼 끄기 → 쇼 다시 켜기 → 앱 닫기. 앱을 닫은 뒤 작업 관리자에 POWERPNT가 남지 않는지. 쇼 도중에 예배 목록을 바꾸는 시험은 실제 목록이라 하지 않는다.

## 6. 폴더 설정 배열 24개

지금: 폴더 번호(0~40, `MAXSONGSFOLDERS = 41`)로 찾는 배열이 24개다. 폴더 하나의 설정이 24군데에 나뉘어 있다.

| 묶음 | 배열 (줄 수) | 두 번째 번호의 뜻 |
|---|---|---|
| 기본 | `FolderName`(88), `FolderUse`(24), `FolderGroupStyle`(9), `FolderRenameName`(7), `FindSongsFolder`(7) | 없음 |
| 머리글 | `FolderLyricsHeading[,4]`(44) | 0 PreChorus, 1 Chorus, 2 Bridge, 3 Ending |
| | `FolderHeadingPercentSize`(11), `FolderHeadingOption`(11) | 없음 |
| | `FolderHeadingFontBold[,2]`(13), `…Italic[,2]`(14), `…Underline[,2]`(13) | 0과 1. DB의 한 값에서 비트 1~3과 4~6으로 읽는다 (`gfFolder.cs` 약 220행). 뜻은 그 줄 주변에서 확인 |
| 영역 글꼴 | `ShowFontName[,2]`(37), `ShowFontSize[,2]`(34), `ShowFontVPosition[,2]`(37), `ShowFontAlign[,2]`(21), `ShowFontRTL[,2]`(20), `ShowLineSpacing[,2]`(20), `MainFontSpacingFactor[,2]`(6) | 0 Region 1, 1 Region 2 |
| | `ShowFontBold[,4]`(32), `ShowFontItalic[,4]`(42), `ShowFontUnderline[,4]`(32) | 0, 1은 Region 1, 2. 2, 3은 네 값을 모두 쓰므로 읽는 곳에서 뜻을 확인 (후렴용으로 보인다) |
| 여백 | `ShowLeftMargin`(21), `ShowRightMargin`(21), `ShowBottomMargin`(26) | 없음 |

DB에서 읽는 곳: `gfFolder.cs` 약 205~250행. 쓰는 곳: `gfUtility.cs` 약 260~335행 (`SaveFoldersSettings`). 옵션 창(`FrmOptions.cs`)은 같은 모양의 임시 배열 16개(`tempFolderName`, `tempShowFontSize` …)에 복사해 두고 OK에서 되돌려 쓴다 (약 100행, 217~222행).

```csharp
// Global/FolderSettings.cs (new)
public sealed class FolderSettings
{
    public string Name = "";
    public int Use;                 // stays int in the first pass (FrmOptions keeps a bool copy)
    public SortBy GroupStyle = SortBy.Alpha;
    // … one field per array of the table above; arrays of 2 or 4 stay small arrays inside the object:
    public string[] LyricsHeading = new string[4];
    public int[] FontSize = new int[2];
    // …
    public FolderSettings Clone() { /* copies the inner arrays too */ }
}

// gfConstants.cs
public static readonly FolderSettings[] Folders = CreateFolders(); // MAXSONGSFOLDERS entries, none null
```

여기서는 바깥 배열(폴더 번호로 찾기)을 그대로 둔다. 폴더 번호는 DB의 `FolderNo`이고 범위가 정해져 있어 배열이 맞다. 얻는 것은 "폴더 하나 = 객체 하나"다.

절차 (배열 하나에 커밋 하나, 24번):

1. `FolderSettings`와 `Gf.Folders`를 만든다 (필드는 아직 없다). 빌드, 커밋.
2. `FolderName`부터: `FolderSettings`에 `Name`을 더하고, `Gf.FolderName` 선언을 지운다. 오류 88군데를 `Gf.FolderName[i]` → `Gf.Folders[i].Name`으로 고친다. 빌드, 확인, 커밋.
3. 같은 식으로 줄 수가 적은 것부터 하나씩. 2차원 배열은 `Gf.ShowFontSize[f, r]` → `Gf.Folders[f].FontSize[r]`.
4. 메서드에 배열째 넘기는 곳이 있으면 (`ref Gf.FolderName` 등) 그 메서드의 매개변수를 `FolderSettings[]`로 바꾼다.
5. 마지막에 옵션 창: 임시 배열 16개를 `FolderSettings[] tempFolders`(각 원소는 `Clone()`)로 바꾼다. 옵션 창은 임시 값의 형식이 다른 것이 있다 (`tempFolderUse`는 `bool[]`, `tempShowFontBold`는 `bool[,]`, `tempFolderGroupStyle`은 `int[]`). 변환하는 줄을 그대로 옮기고, 형식을 통일하는 일은 따로 커밋한다. `tempShowFontVPositionMax` / `Min`은 `Gf`에 짝이 없는 옵션 창만의 값이니 그대로 둔다.

주의:

- **먼저 `feat/frmmain-modern-ui`의 최신을 받아 온다.** 옵션 창 아이콘(`7b902f9`)이 `FrmOptions.cs`와 `FrmMain.cs`를 고쳤다. 리팩토링 폴더에서 `git merge feat/frmmain-modern-ui`.
- `FolderGroupStyle`은 오늘 번호순(`SortBy.Number`)이 생겼고 읽을 때 `Gf.ToFolderSortBy`를 거친다. 그 호출을 지우지 않는다.
- `FolderName`은 이름 → 번호 색인(`RebuildFolderNameIndex`, `gfFolder.cs` 약 317행)이 따로 있다. 이름을 바꾸는 곳 뒤에 색인을 다시 만드는 호출이 그대로 있는지 본다.
- 0번 폴더는 Recycle 폴더다. 반복문이 0부터인지 1부터인지 바꾸지 않는다.

확인: 폴더 바꾸기, 곡 미리보기의 글꼴과 위치가 이전과 같은지 (폴더마다 다른 설정이 있는 폴더로), 옵션 창의 Folders 탭을 열어 값이 맞게 보이는지. **옵션 창에서 OK를 누르면 실제 DB에 저장된다.** 값을 바꾸는 시험은 DB를 백업하고 한다.

## 7. 크기 고정만 남은 것

**`LyricsArray = new string[10000, 3]`** (`gfConstants.cs` 719행): 쓰는 곳이 한 줄뿐이고 (`gfLyrics.cs` 1741행의 대입) 읽는 곳이 없다. 선언과 그 대입 줄을 지운다. 그 줄의 색인(`LyricsIndex`)이 10000을 넘으면 지금은 예외가 나는데, 지우면 그 예외도 없어진다.

**`MediaFileExtension = new string[3000, 2]`** (750행, 16줄): 0번 열은 확장자(`.mp4`, 소문자), 1번 열은 종류의 이름(`MediaBackgroundStyle.Video.ToString()`). 개수는 `TotalMediaFileExt`. 채우는 곳은 `gfMedia.cs` 85~86행.

```csharp
public sealed class MediaExtension { public string Extension = ""; public string TypeName = ""; }
public static readonly List<MediaExtension> MediaFileExtensions = new List<MediaExtension>();
```

번호가 0부터다 (`for (i = 0; i < TotalMediaFileExt; i++)`). `List`를 그대로 쓰고 `TotalMediaFileExt`는 `Count`로 바꾼다. 1번 열은 문자열 비교를 그대로 둔다 (`== InMediaType.ToString()`). 열거형으로 바꾸는 일은 따로.

**`PictureGroups = new string[255, 3]`** (1923행, 12줄): 0번 열은 묶음 이름, 1번 열은 폴더 경로. 2번 열은 쓰는 곳을 찾지 못했다. 개수는 `PicFolderTotal`. 번호는 0부터이고 **`ImagesFolder` 콤보 상자의 `SelectedIndex`와 같다** (`FrmMain.cs` 8826, 8835행). 순서를 바꾸면 안 된다. `Gf.BuildSubFolderList(…, ref Gf.PictureGroups, ref Gf.PicFolderTotal)`(`FrmMain.Logic.cs` 441행)이 배열을 받는다. 이 도우미를 다른 배열도 쓰는지 먼저 본다. 쓰면 그 배열과 같이 바꾼다.

확인: 미디어가 있는 곡의 재생, Images 탭의 폴더 목록과 썸네일.

---

## 하지 않는 것

- **`SongSettings`의 배열** (`SongVerses`, `Slide` 등): `Easislides.Core`에 있고 WPF 앱도 쓴다. 범위가 다르다.
- **창 안의 지역 배열** (`FrmInfoScreen` 15개, `FrmEditItem` 14개): 전역이 아니라 급하지 않다.
- **`StrokeCount = new int[65536]`** 같은 표: 배열이 맞는 자료다.

## 작업 폴더

`E:\WorkSpace\wp-ms\dev\EasiSlides_arrays` (브랜치 `refactor/typed-collections`, `182d97d`에서 시작). v2.6.4 폴더에서는 다른 작업이 진행되므로 여기서 한다.

- 1~4번, 7번은 v2.6.4의 새 커밋과 겹치는 파일이 없다.
- 5번을 하기 전에: 겹치는 것은 없지만 `gfPowerPoint.cs`와 `PowerPoint.cs`가 최근에 많이 바뀌었으니 최신에서 시작한다.
- 6번을 하기 전에: `git merge feat/frmmain-modern-ui` (옵션 창 아이콘 `7b902f9`).

## 단계마다 확인할 것

자동 테스트가 없으므로 손으로 본다. 이 PC에는 실제 예배 자료가 있고, 리팩토링 폴더에서 빌드한 앱도 같은 자료와 레지스트리를 쓴다.

1. 솔루션 전체 빌드 오류 0.
2. 앱 시작, 예배 목록이 이전과 같이 보임.
3. 가사 항목, 성경 항목, PPT 항목을 미리보기로 불러옴.
4. 쇼 켜기, 항목 넘기기, 슬라이드 넘기기, 쇼 끄기.
5. 각 단계의 "확인"에 적은 것.

하지 않을 것: 예배 목록의 항목 추가, 삭제, 순서 바꾸기. 찬양 목록의 더블 클릭(예배 목록에 추가된다), 삭제. 옵션 창의 OK (6번의 시험은 DB를 백업하고).
