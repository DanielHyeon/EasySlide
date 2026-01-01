# gfUtility.cs 리팩토링 계획

## 현황

- **gfUtility.cs**: 6,769 라인 (매우 큼!)
- **총 함수 수**: 약 200+ 개

## 기존 gf* 파일 구조

1. **gf.cs** (51 lines) - 메인 partial class 선언
2. **gfConstants.cs** (2,062 lines) - 상수 및 필드
3. **gfConfig.cs** (855 lines) - 설정/구성 관련
4. **gfDatabase.cs** (980 lines) - 데이터베이스 관련
5. **gfBible.cs** (707 lines) - 성경 관련
6. **gfDisplay.cs** (1,655 lines) - 디스플레이/UI 관련
7. **gfFolder.cs** (617 lines) - 폴더 관련
8. **gfLyrics.cs** (579 lines) - 가사/노테이션 관련
9. **gfFileHelpers.cs** (507 lines) - 파일 헬퍼
10. **gfImages.cs** (443 lines) - 이미지 관련
11. **gfUiText.cs** (308 lines) - UI 텍스트 관련 (새로 생성됨)
12. **gfMedia.cs** (254 lines) - 미디어/음악 관련
13. **gfIO.cs** (141 lines) - 파일 I/O 관련 (새로 생성됨)
14. **gfColorsFonts.cs** (119 lines) - 색상/폰트 관련 (새로 생성됨)
15. **gfPowerPoint.cs** (28 lines) - PowerPoint 관련

## 함수 분류 및 이동 계획

### 1. gfColorsFonts.cs로 이동 (색상/폰트 관련)

- `GetNewFont` (2개 오버로드)
- `SetColoursFormat` (2개 오버로드)
- `SelectColor` 관련 함수들 (이미 존재하면 패스)

**예상 추가 라인**: ~150 lines

---

### 2. gfIO.cs로 이동 (파일/XML/Base64 I/O)

- `GetDisplayNameOnly` ← **사용자가 선택한 함수**
- `RenameExtensions`
- `RecycleBin`
- `CopyExternalFile`
- `MoveExternalFile`
- `MakeTitleValidFileName`
- `GetOfficeDocContents`
- `SupportedOpenDocFormat`
- `SaveXMLInfoScreen`
- `WriteXMLOneItem` (2개 오버로드)
- `WriteXMLSessionHeader`
- `Base64EncodeImageFile`
- `ExtractEasiSlidesXMLItem`
- `MoveToXMLItemElement`
- `GetTitle2AndFormatFromInfoFile`
- `AssignElementToItem`
- `LoadComboBoxFromTextFile`
- `SaveComboBoxToTextFile`
- `LoadListViewFromTextFile`
- `SaveListViewToTextFile`

**예상 추가 라인**: ~600 lines

---

### 3. gfConfig.cs로 이동 (설정/레지스트리/옵션)

- `LoadEulaText`
- `SetPatternPeriod`
- `SaveFoldersSettings4`
- `SaveFoldersSettings`
- `SaveOptionsData`
- `LoadSongKeyCapoTiming`
- `GenerateMusicKeysList`
- `LoadRegistryMainEditHistory`
- `LoadRegistryEditorEditHistory`
- `LoadRegistryInfoScreenEditHistory`
- `SaveMainEditHistoryToRegistry`
- `SaveEditorEditHistoryToRegistry`
- `RemoveDuplicateEditorHistoryItems`
- `SaveInfoScreenEditHistoryToRegistry`
- `ClearAllFormatting`
- `ClearRegistrySettings`
- `UpdateV4RegDM`
- `SetLiveShowScreenSaverSettings`
- `RestoreScreenSaverSettings`
- `SetScreenSaverActive`

**예상 추가 라인**: ~600 lines

---

### 4. gfDatabase.cs로 이동 (데이터베이스 관련)

- `LoadDataIntoItem` (2개 오버로드)
- `LookupDBTitle2`
- `GetItemTitle`
- `SaveFormatStringToDatabase`
- `LoadDBFormatString`
- `LoadIndividualData` (2개 오버로드)
- `LoadIndividualFormatData` (2개 오버로드)
- `ValidSongID`
- `BuildItemSearchString` (2개 오버로드)

**예상 추가 라인**: ~400 lines

---

### 5. gfLyrics.cs로 이동 (가사/노테이션/슬라이드)

- `BuildSlides`
- `BuildSlidesReg2`
- `GetVerseScreenCount`
- `GetVerseScreenLoc`
- `GetVerseScreenEndLoc`
- `GetScreensRequired`
- `GetOneScreen` (2개 오버로드)
- `GetSlideContents`
- `OldGetSlideContents`
- `FormatNotationString`
- `GetAssociatedLyricsLineCurPosX` (3개 오버로드)
- `GetMinMaxfromTextBox`
- `GetMinMaxfromTextString`
- `GetVerseIndicator`
- `TransposeOneNotationString`
- `ExtractOneNotationsLine`
- `ChangeNotationLineNumber`
- `GetVerseNumeric` (private)
- `AddNotationLineNumber`
- `TransposeChord`
- `TransposeOneChord`
- `TransposeKey`
- `IncrementChord`
- `GetCurPosInLine`
- `MoveToPosInLine`
- `InsertChordAboveCurrentLine`
- `GetTextFromPreviousLine`
- `FormatPlainLyrics`
- `Merge_Songs`
- `ExtractLyrics` (2개 오버로드)
- `ExtractNewFormatLyrics`
- `ExtractDefaultFormatLyrics`
- `SubDivideTextAndNotations`
- `OldSubDivideTextAndNotations`
- `MapLyricsBreak`
- `ValidateVerseIndicator`
- `AddItemToScreenBreak`
- `GetBreakPosition` (2개 오버로드)
- `ScanSelectedRTB`
- `MarkSelectedRTB`
- `SubstituteDashes`
- `SubDivideOneOutputText`
- `ActionWordWrapSpacesAtStart`
- `ActionUndoWordWrapSpacesAtStart`
- `GetLinesRequiredAndAddBreakPlusFont`
- `SwitchChineseLyricsNotationListView`
- `OldSwitchChineseLyricsNotationListView`
- `SwitchChinese` (2개 오버로드)
- `getStrConv` (static private)
- `Load32HeaderData` (2개 오버로드)
- `ExtractHeaderInfo`
- `LoadHeaderData`
- `ApplyHeaderData` (2개 오버로드)
- `GapFormatString` (private)
- `IsNewR2Format`

**예상 추가 라인**: ~2,500 lines

---

### 6. gfDisplay.cs로 이동 (디스플레이/UI)

- `MessageOverSplashScreen`
- `ResetShowRunningSettings`
- `ComputeShowLineSpacing`
- `SetLyricsTopPos`
- `HighlightDisplaySlidesText` (3개 오버로드)
- `DisplaySlidesFormattedLyrics` (2개 오버로드)
- `DP_SetSlideIndicators`

**예상 추가 라인**: ~400 lines

---

### 7. gfUiText.cs로 이동 (UI 컨트롤)

- `AssignDropDownItem` (4개 오버로드)
- `SetListViewColumns`
- `UpdatePosUpDowns`
- `GetSelectedIndex` (4개 오버로드)
- `SetMenuItem`
- `FormInUse`
- `UpdateRefString`

**예상 추가 라인**: ~300 lines

---

### 8. gfImages.cs로 이동 (이미지/배경/트랜지션)

- `SetShowBackground` (2개 오버로드)
- `SetImageFormat`
- `ComputeTransition`
- `GetOneRegionHeight`
- `ReplaceFont` (private)
- `SetDefaultBackScreen`

**예상 추가 라인**: ~350 lines

---

### 9. gfMedia.cs로 이동 (미디어/음악)

- `GetMusicFileName` (4개 오버로드)
- `GetMusicFileNameFromDir` (3개 오버로드)
- `Html_MusicDisplayName`
- `LookUpMediaString` (2개 오버로드)
- `LookUpMediaInteger`
- `GetMediaLocation` (2개 오버로드)
- `GetMediaBackgroundType`
- `GetMediaType`
- `GetRotationStyle`
- `GetNextNonRotateItem` (internal)
- `GetItemRotateResult` (2개 오버로드, internal)
- `AlertSettings`
- `BuildAlertSequence`

**예상 추가 라인**: ~450 lines

---

### 10. gfPowerPoint.cs로 이동 (PowerPoint 관련)

- `SetPowerpointPreviewPrefix`
- `SetPowerpointPreviewPrefix1`
- `MinimizePowerPointWindows`
- `RunPowerpointSong` (2개 오버로드)
- `ClearUpPowerpointWindows`
- `ReMapKeyBoard`
- `ImplementSlideMovement`
- `ReMapKeyDirectionToPowerpoint`
- `RunProcess`
- `FindRecentMediaPlayerWindow` (private)
- `RunProcessOnMonitor`

**예상 추가 라인**: ~450 lines

---

### 11. gfUtility.cs에 남겨둘 함수 (범용 유틸리티)

- `GetUniqueID`
- `SingleArraySort` (2개 오버로드)
- `ExtractNumericData`
- `RemoveMusicSym`
- `ReverseString`
- `RTFCheck`
- `ConvertSequenceSymbol`
- `ConvertSequenceToTextString`
- `ConvertTextStringToSequence`
- `FormatMode`
- `ExtractSettings`
- `CombineSettings`
- `StartElement`
- `ValidateTitleDetails`

**예상 남은 라인**: ~500 lines

---

## 구현 단계

### Phase 1: 분석 및 준비

1. ✅ 기존 gf* 파일 구조 파악
2. ✅ gfUtility.cs의 모든 함수 목록 추출
3. ✅ 함수를 카테고리별로 분류
4. ⬜ 함수 간 의존성 확인

### Phase 2: 파일별 이동 (작은 파일부터)

1. ⬜ **gfColorsFonts.cs** - 폰트/색상 관련 함수 이동
2. ⬜ **gfUiText.cs** - UI 컨트롤 관련 함수 이동
3. ⬜ **gfIO.cs** - 파일 I/O 관련 함수 이동 (**GetDisplayNameOnly 포함**)
4. ⬜ **gfDisplay.cs** - 디스플레이 관련 함수 이동
5. ⬜ **gfDatabase.cs** - 데이터베이스 관련 함수 이동
6.
7. ⬜ **gfConfig.cs** - 설정/레지스트리 관련 함수 이동
8. ⬜ **gfImages.cs** - 이미지/트랜지션 관련 함수 이동
9. ⬜ **gfMedia.cs** - 미디어/음악 관련 함수 이동
10. ⬜ **gfPowerPoint.cs** - PowerPoint 관련 함수 이동
11. ⬜ **gfLyrics.cs** - 가사/노테이션 관련 함수 이동 (가장 큼)

### Phase 3: 검증 및 정리

1. ⬜ 빌드 테스트
2. ⬜ 코드 검토
3. ⬜ Git 커밋

## 예상 결과


| 파일명           | 현재 라인 | 추가 예상 | 최종 예상 |
| ---------------- | --------- | --------- | --------- |
| gfUtility.cs     | 6,769     | -6,200    | ~570      |
| gfColorsFonts.cs | 119       | +150      | ~270      |
| gfIO.cs          | 141       | +600      | ~740      |
| gfConfig.cs      | 855       | +600      | ~1,450    |
| gfDatabase.cs    | 980       | +400      | ~1,380    |
| gfLyrics.cs      | 579       | +2,500    | ~3,080    |
| gfDisplay.cs     | 1,655     | +400      | ~2,050    |
| gfUiText.cs      | 308       | +300      | ~610      |
| gfImages.cs      | 443       | +350      | ~790      |
| gfMedia.cs       | 254       | +450      | ~700      |
| gfPowerPoint.cs  | 28        | +450      | ~480      |

## 주의사항

1. **점진적 이동**: 한 번에 하나의 파일만 수정하여 빌드가 깨지지 않도록 함
2. **의존성 체크**: 함수 간 호출 관계를 확인하여 순서대로 이동
3. **테스트**: 각 이동 후 빌드 및 기본 테스트 수행
4. **커밋 전략**: 파일별로 커밋하여 롤백 가능하도록 함
5. **네이밍 일관성**: 모든 함수는 `gf` 클래스의 `partial` 메서드로 유지

## 참고

- 최근 커밋 히스토리를 보면 이미 비슷한 리팩토링을 진행 중:
  - `ce3a5aa gfImages.cs 파일 리팩토링`
  - `b08ca13 gfFileHelpers.cs 리팩토링`
  - `1d22da1 gfDatabase 리팩토링`
  - `ba1aee2 gfFileIO.cs 파일 리팩토링`
  - `92738d6 gfUtility.cs 함수 분리`
