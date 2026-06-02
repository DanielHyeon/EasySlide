# FrmMain ↔ WPF 상세 갭 분석 (UI/UX + 기능 전수)

> 작성일: 2026-06-01 · 방법: **레거시 `FrmMain.*`(21,649줄) 전체와 WPF `Easislides.Wpf` 전체를 실제 코드로 전수 인벤토리**하여 컨트롤·기능 단위로 1:1 대조. 추정/요약이 아니라 Designer 컨트롤명·핸들러 메서드명·WPF Command/Property명을 근거로 표기한다.
> 근거 파일: `FrmMain.Designer.cs`(7,318) · `FrmMain.cs`(8,822) · `FrmMain.Logic.cs`(3,115) · `FrmMain.Events/Fields/Layout.cs` ↔ `MainWindow.xaml(.cs)` · `Shell/MainViewModel.cs` · `Composites/*` · `Library/BibleWindow.xaml` 등.
>
> **재검증 이력(2026-06-01)**: 작성 후 적대적 교차검증(코드 직접 grep/read + 독립 리뷰 에이전트) 수행. 결과 — **거짓 "없음" 주장(over-statement) 0건**(모든 🔴/0% 판정이 코드로 확인됨). 발견·반영한 정정 4건: ① PraiseBook "전무" → 문서생성(RTF/HTML)은 존재·운영 UI만 부재로 정밀화, ② Region2 "0%" → 디코더는 region2 파싱·출력 렌더만 부재로 정밀화, ③ ToolStrip 수 31→30, ④ **`LyricsMonitorShowNotations` 설정이 mis-bound된 실제 결함** 발견·기록(§3.7).

---

## 0. 정직한 현황 요약 (먼저 인정할 것)

기존 `gap-analysis.md`는 "WPF는 **얇은 디스패처 셸**(FrmMain의 일부)"이라고 §7에서 이미 밝혔으나, **컨트롤·기능 단위의 정밀 대조는 없었다**. 이 문서가 그 빈자리를 채운다.

- 직전 작업(per-song 폰트/배경이미지·성경 버전 CRUD)은 **유효한 백로그 2건**이긴 하나, FrmMain의 근본 갭(아래 §5 Top 10)에는 손대지 못했다. "완료"라는 표현이 **전체 진척의 완료처럼 들리게 한 것은 과장**이었다.
- **WPF는 FrmMain의 운영 기능을 기능 면적 기준 대략 한 자릿수 %만 포팅한 상태**다(아래 정량 근거). 특히 **이중 언어(Region 1/2) 렌더링**이라는 이 프로그램의 골격이 **WPF엔 전혀 없다**(0%). 이것 하나만으로도 WPF는 한국 교회 다수의 실사용(한/영, 한/중 이중 송출)을 대체할 수 없다.

---

## 1. 정량 요약

### 1.1 UI 표면 (Designer 전수 카운트)

| 표면 | FrmMain (레거시) | WPF MainWindow | 비고 |
|---|---|---|---|
| 메뉴(MenuStrip) | 6 대메뉴 / **클릭 항목 52** (+구분선 16) | **0** (메뉴바 없음 → ⌘K 팔레트 24개로 일부 흡수) | 팔레트 24 ≪ 메뉴 52 + 툴바 |
| 툴바(ToolStrip) | **30개 스트립 / 상호작용 요소 ~172** (버튼·드롭다운·콤보·하위메뉴 포함; 최상위 버튼 ~93) | 하단 운영바 + 우측 인스펙터 합쳐 **~40개 컨트롤** | 인-셸 포맷팅 밀도 ~20% |
| ListView | **8** (곡/InfoScreen/PPT/미디어/예배순서/PraiseBook/미리보기명/출력명) | ListBox 3 (라이브러리 곡/검색결과/성경후보) + 예배순서 1 | 폴더 트리·이미지·PraiseBook 목록 없음 |
| ComboBox | 폼 8 + 툴바 8 = **16** | ~6 (출력모니터/폴더/성경버전·책/검색·매치/템플릿) | |
| NumericUpDown(여백·크기) | **8** | **5** 직접 수치 입력(가사 크기·줄 간격 + 본문 좌/우/아래 여백 TextBox, 범위 클램프·빨간 테두리 검증) + −/+ 버튼·메뉴 step | 🟢 크기·줄간격·여백 3종 모두 직접 수치 입력 완료(인스펙터 "본문 여백" 섹션) |
| ContextMenu(우클릭) | **6** (40개 항목) | **0** | 우클릭 운영 전무 |
| 비툴바 버튼 | **49** (절/슬라이드 점프 16+16, 항목·슬라이드 이동, 전송, LM 등) | 인스펙터/운영바 버튼들 | 절 라벨 점프(1~9·p/q/c/t/b/w/e) 없음 |
| 탭(TabControl/TabPage) | 3 컨트롤 / **10 탭** (Folders·InfoScr·PowerP·Bibles·Images·Media·Default·WorshipList·PraiseBook + 성경버전) | 좌 3탭(라이브러리/성경/검색) + 중앙 3탭(Preview/PPT/Media) | Images·PraiseBook·InfoScr·Default(포맷) 탭 없음 |
| SplitContainer(가변 도킹) | **5~6 중첩** | Grid 고정 + GridSplitter 1 + 인스펙터 토글 | 사용자 레이아웃 저장 없음 |

### 1.2 기능 면적 (영역별 거친 커버리지)

| 영역 | 커버리지 | 근거 |
|---|---|---|
| 라이브 송출 기본(Go Live/Black/Next·Prev/Clear/Hide/Restart/Refresh) | 🟢 ~80% | WPF 핵심 운영 명령 존재 |
| **가사 렌더링·포맷팅(Region 1/2·헤딩·정렬·폰트·효과·전환·배경)** | 🟢 **~85%** | 이중 언어(Region1/2)·표시모드·인터레이스·헤딩(절/제목·L/C/R·AsR1·AsR2)·Display Panel 완비·전환·배경모드 4종+그라데이션 방향·여백·직접수치·영역별 폰트크기/세로간격/세로위치 모두 구현. 배경 텍스처 패턴만 후속 |
| 콘텐츠 브라우징(폴더/곡/성경/PPT/미디어/이미지/InfoScreen/PraiseBook/세션) | 🟡 ~30% | 곡·성경·검색만 인라인; 나머지 미포팅/별도창 |
| 예배 순서 관리(추가/이동/제거/드래그/자동회전/검증/.esw) | 🟡 ~55% | 기본 OK, 자동회전 세분·검증·legacy v3.2 부족 |
| 편집(곡/성경/노트/InfoScreen/팝업/인라인 포맷) | 🟡 ~40% | SongEditor 있으나 InfoScreen·노트 전용·인라인 per-item 포맷 부족 |
| 설정·템플릿 | 🟡 ~35% | 출력모양 템플릿만; Default/Individual·배경패턴·Apply-to-all 없음 |
| 조옮김·코드·음악 | 🟡 ~40% | 라이브 코드 조옮김 ±반음(증분30)·코드/악상 표시 토글 구현. To-Capo-0 정규화·Capo 영속·악보 뷰만 후속 |
| 데이터 작업(Import/Export/Generate/Copy/Move/Delete/Recover/Merge/Usages/Find) | 🟢 ~75% | 별도 창으로 대부분 포팅(통합은 별개) |
| 키보드/리모컨 후킹 | 🟡 ~40% | 전역 단축키 일부; 절-라벨 점프·미디어 후킹·리모컨 버스 부족 |

> 단순 평균이 아니라 **운영 본질(라이브 렌더링)에 가중치를 두면** WPF의 실효 커버리지는 한 자릿수~십몇 %대다. "기능 5%에 미치지 못한다"는 사용자 지적은 **운영 렌더링·이중언어 관점에서 타당**하다.

---

## 2. UI/UX 표면 갭 (Designer 컨트롤 단위)

### 2.1 메뉴바 (FrmMain 52항목 → WPF 0)
WPF는 메뉴바가 없고 ⌘K 팔레트(24)로 일부만 흡수. **팔레트에 없는 FrmMain 메뉴 다수가 미구현 기능이라 흡수 자체가 불가**:
- File: Worship Sessions / PraiseBooks / Listing of Selected Folder / Recent Edits → PraiseBooks·Recent Edits 는 포팅(증분44/세션), Worship Sessions·Listing of Folder 만 후속
- Edit: Add/Edit/Copy/Move/Delete/Select All/**Use Song Numbering**/Re-Arrange Song Folders → Use Song Numbering·Select All 등 인-셸 없음
- View: EasiSlides Folder / **Show Notations in Preview** / Status Bar → 미포팅
- Output: Start Show(F12)/Preview Go-Live-Move-Next(F11)/Refresh/Black(F9)/**Clear(F3)**/**Restart(F5)** → 단축키 F9/F11/F12/F3/F5 매핑 미검증
- Tools: Import/ImportFolder/Export/Recover/Empty/AddToUsages/ViewUsages/SmartMerge/**Compact&Repair**/**Clear All Formatting**/Clear Registry → Compact·ClearAllFormatting·ClearRegistry **미포팅**
- Help: Contents(F1)/Help-Web/Register/About → 창은 있으나 F1 등 미검증

### 2.2 인-셸 포맷팅 툴바 (FrmMain `Def_*`/`Ind_*` ~80컨트롤 → WPF 인스펙터 ~15)
이것이 **UI/UX 최대 갭**이다. FrmMain은 운영 중 즉시 바꾸는 포맷 컨트롤이 Default 탭(`Def_*`)과 개별설정(`Ind_*`)에 양분되어 다음을 모두 노출한다. WPF 우측 인스펙터에 **대응 없는 항목**을 🔴로 표시:

| FrmMain 컨트롤(발췌) | 기능 | WPF |
|---|---|---|
| `Def_ShowRegion1/2/Both`, `Ind_ShowRegion*` | **Region 1만/2만/둘다** 표시 | 🟢 (증분 51) 출력 메뉴 "이중 언어 영역 표시"(둘다/Region1만/Region2만, 라이브 즉시 반영). 한쪽만 모드여도 다른 쪽이 비면 남은 본문 유지(빈 화면 방지). 전역 설정(Def_); 항목별 Ind_ 는 후속 |
| `Def_Interlace`/`Ind_Interlace` | **Region 1·2 인터레이스**(줄 교차) | 🟢 (증분 53) 출력 메뉴 "이중 언어 줄 교차" — 원문/번역 줄을 번갈아 송출(각 줄이 자기 영역 색·글꼴·효과·정렬·줄높이·그림자 유지). 두 영역이 다 보일 때만 동작 |
| `Def_R1Colour`·`Def_R2Colour`, `Ind_R1/R2Colour` | **영역별** 글자색 | 🟢 (증분 46) 곡 편집기 "영역별 스타일(Region 1/2)" 인스펙터에서 영역별 글자색 지정(FormatData 29/30 인코드)→출력 렌더 적용. Display Panel 전역 기본값 UI는 후속 |
| `Def_R1Align`·`Def_R2Align` (L/C/R) | **영역별** 정렬 | 🟢 (증분 46) 곡 편집기 인스펙터에서 영역별 정렬(상속/왼쪽/가운데/오른쪽, FormatData 31/32)→출력 렌더 적용 |
| `Ind_R1Bold/Italics(3종)/Underline`, R2 동일 | 영역별 굵게/기울임(없음/전체/후렴만)/밑줄 | 🟢 (증분 46/47) 영역별 굵게/기울임 + (증분 48~50) 밑줄(전역·영역별·외곽선) + (증분 52) **강조 후렴만**(굵게·기울임·밑줄을 후렴 절에만, [C]/[Chorus]/[후렴] 인식, 이중 언어 라벨↔페이지 정렬 가드). 레거시 비트 한계로 굵게/기울임/밑줄 "끄기"는 전역 추종 |
| `Ind_Reg1/2SizeUpDown`, `Ind_Reg1/2TopUpDown` | 영역별 폰트 크기·세로위치 | 🟢 Region1/2 크기(증분64) + 영역 간 세로 간격(증분67) + **본문 세로 위치 오프셋(증분73, Ind_Reg1TopUpDown — TranslateTransform 위/아래 이동)** 구현 |
| `Ind_Reg1/2FontsList` | 영역별 글꼴 | 🟢 인스펙터에 본문(Region1)·보조영역(Region2) **전역 글꼴 선택 콤보**(LyricsMonitorFontFamily/2). **증분103**: 추천 한/영 글꼴 + **설치된 시스템 글꼴 전체**(Fonts.SystemFontFamilies, 추천 앞·설치 정렬 뒤·중복 제거)를 콤보에 노출(편집 콤보라 직접 입력도 가능). 곡별 FormatData 글꼴(43/44)이 있으면 그 곡 우선. 항목별 인스펙터 글꼴(per-item)·한글 표시명 노출은 후속 |
| `Def_Head`/`Ind_Head` (NoTitles/All/FirstScreen) | 헤딩 표시 모드 | 🟢 제목 헤딩 on/off·첫화면만 + **절 헤딩(All — 섹션 라벨 "1절"/"후렴" 표시)** 구현 |
| `Def_HeadAlign`/`Ind_HeadAlign` (AsR1/AsR2/L/C/R) | 헤딩 정렬(영역 추종 포함) | 🟢 L/C/R + AsR1(본문 따름) + **AsR2(보조영역 Region2 따름, 우선순위 AsR2>AsR1)** — 5종 전부 구현 |
| `Def_VAlign`/`Ind_VAlign` (Top/Centre/Bottom) | 세로 정렬 | 🟢 있음 |
| `Def_Shadow`/`Def_Outline` | 그림자/외곽선 | 🟢 있음 |
| `Def_ToZero` | **To Capo 0**(조옮김 정규화) | 🟢 라이브 "원조 복귀"(TransposeLiveResetCommand, 0 으로 리셋)·새 곡 송출 시 자동 원조 초기화 |
| `Def_Notations`/`Menu_PreviewNotations` | **코드/악상 표시**(+미리보기) | 🟢 (증분69) 출력 메뉴 "코드 표시" 토글 + ExpandNotations 렌더. 미리보기 전용 토글만 후속 |
| `Ind_CapoUp`/`Ind_CapoDown` | **반음 조옮김 ↑↓** | 🟢 라이브 코드 조옮김 ▲/▼(±반음, ±11 클램프, ChordTransposer) 메뉴 |
| `Def_BackColour`/`Ind_BackColour` | 배경색+패턴(그라데이션) | 🟡 프리셋4+hex+2색 그라데이션(방향 4종: 세로/가로/대각↘/대각↗). 텍스처 패턴만 후속 |
| `Def_ImageMode`/`Ind_ImageMode` (Tile/Centre/BestFit), `Def_NoImage` | **배경 이미지 표시 모드** | 🟢 LyricsBackgroundMode 4종(Fill=UniformToFill·Fit=BestFit·Center=Centre·Tile) 인스펙터 라디오(ApplyBackgroundModeCommand)→출력 렌더 반영 |
| `Def_TransItem`/`Def_TransSlides`, `Ind_Trans*` | **항목/슬라이드 전환 효과** | 🟡 출력 메뉴 "전환 효과"(페이드 on/off + 모션 Fade/Slide 4방향 + 속도)로 노출. 항목/슬라이드 개별 전환 분리·전체 효과군은 후속 |
| `Def_AssignMedia`/`Ind_AssignMedia` (None/AsTitle/Specific/LiveFeed) | 항목 미디어 배정 | 🔴 없음 |
| `Ind_LeftUpDown`/`RightUpDown`/`BottomUpDown` | **여백 수치 입력** | 🟢 출력 본문 좌/우/아래 여백(LyricsMonitorBodyLeft/Right/BottomMargin, 0~400px) — 메뉴 "본문 여백" −/+ 로 8px 단위 조절·설정 영속·라이브 즉시 반영. 직접 수치 입력 박스만 후속 |
| `Def_Panel*` (Show/Title/Copyright/ItemNumber/Slides/PrevNext/Transparent/AsR1/색/폰트 9종+) | Display Panel 항목(곡번호·저작권·다음항목·위치·제목) + 투명 토글 + 밴드 색 + 글자 크기 | 🟢 ItemNumber/Copyright/PrevNext/Position(증분6)·Transparent(증분39)·밴드 색(증분63)·글자 크기(증분65)·**제목 표시(증분66, Def_PanelTitle)** 구현. (패널 AsR1 정렬 추종은 미세 잔여) |
| `Ind_LoadTemplate`/`SaveTemplate`, `Def_*Template` | 개별/기본 설정 템플릿 | 🟡 출력모양 템플릿 1종만 |
| `Ind_checkBox` "Use Individual Settings" | **항목별 개별 포맷 vs 기본** | 🟢 (증분 54) 예배 순서 항목 우클릭 "개별 서식 사용" 토글 — off 면 그 항목의 FormatData(색·정렬·폰트·배경) 무시하고 전역 기본으로 송출. 라이브 즉시 반영(현재 절 유지)·저장/불러오기 보존(FormatData 도 영속) |
| `DefApplyDefaultsBtn` "Apply to All Except InfoScreens" | 전 항목 기본 적용 | 🟢 (증분71) 출력 메뉴 "전 항목에 전역 서식 적용" — 모든 항목 개별서식 off→전역 통일, 라이브 즉시 반영 |

### 2.3 콘텐츠 소스 탭 (FrmMain 7 source 탭 → WPF 3 좌측 탭)
| FrmMain 탭 | 내용 | WPF |
|---|---|---|
| Folders + `SongFolder` + `SongsList` + 정렬(획/CJK단어수/곡번호)·A/B/C 점프 | 곡 폴더·목록 | 🟢 라이브러리 탭(폴더 콤보+검색+ListBox + 정렬[원래/제목/곡번호/획수순] + A/B/C 초성 점프 바). CJK 단어수 정렬만 후속 |
| InfoScr(`InfoScreenList`, .esi 편집기) | 공지 화면(NoticeScreenWindow — 텍스트 편집·송출 + **명명 정보화면 저장/불러오기/삭제**) | 🟡 텍스트 편집·송출 + 명명 저장 목록(증분76) 포팅. .esi 풀 편집기(7337줄)·레거시 포맷만 후속 |
| PowerP(`PowerpointList`, list/thumbnail) | PPT 파일 브라우징 | 🟡 중앙 PPT 탭은 미리보기만(폴더 브라우징·import 없음) |
| Bibles(인라인, §4 참조) | 인라인 성경 | 🟡 좌측 성경 탭(단, 이중영역·typed-ref·copy-to-InfoScreen 없음) |
| Images(`flowLayoutImages`, Scenery/Tiles, 배경 적용) | 이미지 갤러리(ImageLibraryWindow — 폴더 썸네일·출력 배경 적용/지우기) | 🟢 폴더 이미지를 썸네일로 보고 출력 배경으로 적용. Scenery/Tiles 카테고리 구분만 후속 |
| Media(`MediaList`) | 미디어 파일 목록 | 🟡 중앙 Media 탭(폴더 목록 브라우징 없음, NoOp 백엔드) |
| Default(포맷 탭) | 기본 포맷 전체 | 🔴 인스펙터로 일부 대체 |

### 2.4 리스트 탭 (FrmMain 2 → WPF 1)
- Worship List(`WorshipListItems` + 세션 콤보 + 노트) → 🟡 WorshipListPanel(세션 노트·세션 콤보 없음)
- **Praise Book**(`PraiseBookItems` 인터랙티브 목록 + CJK 그룹핑 + 추가/정렬/Clear/RTF·HTML 생성) → 🟢 **운영 UI 포팅 완료(증분44, 2026-06-02 재확인)**: PraiseBookIndexWindow(머리글자 CJK 그룹 색인·명명 저장/열기/삭제·HTML 내보내기·곡 더블클릭→예배순서 추가, MainWindow 에서 열림) + 문서 생성(RTF/HTML, `ImportExportService.PraiseBookExportOptions`·`WriteRtf`·`BuildPraiseBookHeading`). 즉 만들어 출력(문서)·운영 중 탐색·관리·저장 모두 가능. 책 안 개별 추가/Clear·legacy v3.2 만 후속. (※ 직전 "브라우저/관리 UI 전무" 표기는 stale 였음 — 코드/증분44 로그로 정정)

---

## 3. 기능 갭 매트릭스 (동작 단위)

범례: 🟢 대응 / 🟡 부분 / 🔴 없음

### 3.1 라이브 송출 제어
| FrmMain 기능 (구현 메서드) | WPF (Command/메서드) | 상태 |
|---|---|---|
| Go Live (`GoLive`,`Start_Presentation`,`FrmLaunchShow`) | `GoLiveCommand` + OutputWindow | 🟢 |
| Black (`LiveBlack`/F9) | `BlackScreenCommand` | 🟢 |
| Clear/Hide text (`LiveClear`/F3) | `ClearOutputCommand`/`HideOutputCommand` | 🟢 |
| Next/Prev/First/Last/Refresh (`MoveToItem` KeyDirection) | Next/Previous + First/Last Item·Slide·LyricsPage | 🟢 (2026-06-02 증분 37) First/Last 항목 이동 추가(처음/마지막 버튼·메뉴, 라이브면 송출) |
| Restart Current Item (F5) | `RestartCurrentItemCommand` | 🟢 |
| 절/슬라이드 라벨 점프(`PreviewBtnVerse_Click` 1~9·p·q·c·t·b·w·e) | 절 라벨 점프 바(AvailableSectionLabels·JumpToLyricsSectionCommand) | 🟢 (이전 증분) 곡에 [라벨]이 있으면 절 점프 버튼이 생겨 그 절로 즉시 이동(순차 이전/다음도 유지) |
| Preview→Output 전송 분리(`btnToOutput`/`btnToOutputMoveNext`/`btnToLive`) | Go LIVE(=btnToOutput/btnToLive) + (증분 55) **송출 후 다음 항목**(btnToOutputMoveNext) | 🟡 송출·송출후다음 2단 전송 명령 제공(자동 다음 설정과 무관). 별도 Preview 모니터(≠출력)는 후속 |
| **이중 모니터 Preview+Output 동시 송출**(`ShowDualMonitorPP_Preview/_Output`) | — | 🔴 미리보기 ≠ 출력 분리 송출 없음 |
| 리모컨/런치스크린 명령 버스(`RemoteControlLiveShow` ~25 액션) | 직접 SessionChanged | 🟡 단순화(리모컨/별도 런치윈도우 없음) |
| 출력 모니터에 미디어 재생 분리창(`PlayMediaOnOutputMonitor`,`FrmLaunchMediaPlayer`) | OutputWindow MediaElement | 🟡(전용 풀스크린 런처·top-most 제어 없음) |
| 알림 오버레이(`FrmShowAlert`,message/parental/reference/lyrics) | EsToast/LyricsAlert | 🟡 조작 UI 없음 |
| Lyrics Monitor 메시지 전송(`OutputTextBoxLM`,`SendLyricsMonitorMessage`) | — | 🔴 없음 |
| 중/번체 전환(`OutputChineseSwitch`) | — | 🔴 없음 |
| 런타임 출력 모니터 이동(MoveTo) | 출력 모니터 선택 변경 시 열린 창을 즉시 이동(OnSelectedOutputDisplayChanged→MoveTo) | 🟢 (증분68) 출력 열린 상태에서 모니터 바꾸면 그 모니터로 즉시 이동, 새 해상도로 PPT 재렌더 |

### 3.2 가사 렌더링·포맷팅 — ★ 최대 갭
| FrmMain 기능 | WPF | 상태 |
|---|---|---|
| **Region 1/2 이중 언어**(영역별 색·정렬·폰트·크기·세로위치·굵게·기울임·밑줄) | `[region 2]` 마커 파서(GetRegionPages) + 영역별 송출(Region1 위 Region2) + 영역별 색/정렬/글꼴/크기/굵게/기울임/밑줄·간격·인터레이스·표시모드 + **Region2 전역 글꼴(80)·색(82)·정렬(83)·굵게(85)·기울임(86)·밑줄(87)** | 🟢 이중 언어 출력 렌더 동작(이번 다증분 슬라이스의 최대 갭 해소). Region2 전역 글꼴·색·정렬·굵게·기울임·밑줄 모두 설정 가능(0/빈값/FollowRegion1=본문 추종, 곡별 우선). 굵게·기울임·밑줄은 3-상태(추종/켬/끔)로 번역만 분리 가능. **영역별 세로위치만 본문 전체 오프셋(증분73)으로 근사** — Region2 외형 전역 셋 완비 |
| Region 인터레이스(줄 교차 송출) | 원문/번역 줄 교차(LyricsMonitorInterlace, 인스펙터 토글) | 🟢 이중언어 Region1/2 줄을 교차 배치해 송출(InterlacedLines) |
| Region 표시 모드(1만/2만/둘다) | LyricsRegionDisplay(Both/Region1Only/Region2Only, ApplyRegionDisplayCommand) | 🟢 영역 표시 모드 3종 인스펙터 라디오→출력 반영 |
| Display Panel(송출 하단 정보 바: 제목/저작권/항목번호/절·슬라이드/이전·다음, 색·폰트·투명) | 정보 항목(항목번호·저작권·다음항목·위치 N/M·제목) + 투명 토글 + 밴드 색 + 글자 크기 | 🟢 정보 항목·제목·투명·밴드 색·글자 크기 비율 모두 렌더링(증분6/39/63/65/66) |
| 헤딩(제목/**절 헤딩**, 모드 3종, 정렬 AsR1/AsR2/L/C/R) | 🟢 제목 on/off·L/C/R·첫화면 + 절 헤딩 + AsR1 + AsR2 — 헤딩 표시/정렬 전부 구현 |
| 코드/악상 표시(`ShowNotations`, 미리보기 토글) | 코드 표시(LyricsDisplayFormatter.ExpandNotations) | 🟢 (증분 27) "코드 표시" on 이면 가사 위에 코드 줄 송출 + (증분 30) 라이브 조옮김 ±반음 |
| 조옮김(Capo ↑/↓, To Capo 0) | `LiveTranspose*Command`(출력 메뉴 "코드 조옮김 ▲/▼/원조") + ExpandNotations transpose | 🟡 (2026-06-01 증분 30) — **라이브 코드 조옮김 ±반음**(코드 표시 on 일 때 송출 코드 이동, 가사 불변). 새 곡 송출 시 원조 초기화. ChordTransposer 재사용 |
| 항목/슬라이드 전환 효과 선택 | 출력 메뉴 "전환 효과"(페이드 on/off·모션 32종·속도) + **항목 전환 vs 슬라이드/절 전환 분리(증분92)** | 🟢 항목 전환(곡→곡)과 슬라이드 전환(같은 곡 절·슬라이드 이동)을 각각 다른 효과로 지정 — 스냅샷 CurrentItemId 로 변경 종류 판별. 전체 32 효과군 노출 |
| 배경 색+패턴(그라데이션 2색) | 🟡 프리셋/hex/2색 그라데이션(방향 4종) | 🟡(텍스처 패턴만 후속) |
| 배경 이미지 + 표시 모드(Tile/Centre/BestFit/No) | 곡별 FormatData 이미지 + 전역 배경(ImageLibrary 적용) + 표시 모드 4종 | 🟢 모드 선택(Fill/Fit/Center/Tile)·이미지 갤러리 적용·해제 가능 |
| 정렬 가로 L/C/R · 세로 T/C/B | 🟢 | 🟢 |
| 폰트 크기/줄간격/Bold/Italic/Shadow/Outline | 🟢(단일 영역) | 🟢 |
| 글꼴명(per-song FormatData 43/44 + 전역 Def_FontName/Ind_Reg2Font) | 곡별 글꼴(43/44) + **전역 본문·보조영역 글꼴 설정**(LyricsMonitorFontFamily/FontFamily2, 인스펙터 편집 콤보 2개) | 🟢 본문: 곡별(43) > 전역 > 테마 기본. 보조영역(Region2): 곡별(44) > 전역 > 본문 추종. 비우면 상속(무회귀), "전체 복원"이 글꼴까지 되돌림 |
| 위치 인디케이터 N/M | 🟢 | 🟢 |
| 절 단위 페이지네이션 + Sequence 절 순서 | 🟢 | 🟢 |

### 3.3 콘텐츠 브라우징
| FrmMain | WPF | 상태 |
|---|---|---|
| 폴더+곡 목록, 정렬(획수/CJK단어수/곡번호), A/B/C 점프, Use Song Numbering | 라이브러리 탭(폴더+검색+초성 점프+정렬(원래/제목/곡번호/획수)+곡번호 표시) | 🟢 (증분 34) **A/B/C 점프** + (증분 35) **정렬(원래순서/제목/곡번호)** + (증분 36) **Use Song Numbering** + (증분 45) **획수순(한자) 정렬**(zh-Hant ICU 콜레이션=획수 기반, 별도 데이터 불필요). CJK 단어수 정렬만 후속 |
| **PraiseBooks**(관리/추가/정렬/RTF·HTML/legacy v3.2) | 찬양집 색인 창(PraiseBookIndexWindow — 머리글자 CJK 그룹 색인·명명 저장/열기/삭제·HTML 내보내기·곡 더블클릭→예배순서, RTF 는 ImportExportService) | 🟢 (증분44) 운영 UI 포팅 완료(MainWindow 에서 열림). 책 안 개별 추가/Clear·legacy v3.2 만 후속 |
| **Worship Sessions / Recent Edits / Session Notes** | 최근 예배 순서(IRecentWorshipLists) + 세션 메모(WorshipSessionNotes — 예배 순서 이름별 키) | 🟢 Recent Edits=최근 예배 순서·Session Notes=세션 메모(예배 순서 이름별 저장) 포팅 완료. Worship Sessions 풀 번들(순서+설정 묶음)만 후속 |
| Images 라이브러리(Scenery/Tiles, 우클릭 배경 적용) | 이미지 갤러리(폴더 썸네일·하위폴더 포함·카테고리[Scenery/Tiles] 필터·배경 적용/해제·더블클릭·**우클릭 메뉴**) | 🟢 (증분74·75) 카테고리 필터 + 우클릭 "배경으로 적용/해제"(우클릭 항목 선택). Images 트랙 완비 |
| Media 목록 브라우징 | 중앙 Media 탭 + `MediaLibraryWindow`(폴더 브라우저) | 🟢 (2026-06-01 증분 33) 미디어 폴더 브라우저(동영상·오디오 13확장자, 하위폴더·더블클릭 추가) — PowerPoint 브라우저와 동일 구조 |
| PowerPoint 목록/썸네일/Import | 중앙 PPT 미리보기 | 🟡 브라우징/Import 없음 |
| InfoScreens(.esi) 목록·편집 | 명명 정보화면 목록(InfoScreenStore 저장/불러오기/삭제) + 본문·글자크기·가로 정렬·**글자색**(증분76~78) | 🟡 명명 저장·텍스트/크기/정렬/색 편집(곡 FormatData 오버라이드 재사용). 풀 편집기(7337줄)·.esi 레거시 포맷만 후속 |
| 인라인 성경(§4) | 좌측 성경 탭 + BibleWindow | 🟡 |

### 3.4 예배 순서 관리
| FrmMain | WPF | 상태 |
|---|---|---|
| 항목 추가(곡/PPT/성경/텍스트/InfoScreen/Word/미디어/외부파일/.esw 병합) | AddSong/Bible/PowerPoint/Media + 파일추가 + 병합(93) + 텍스트/공지(94) + **Word 문서(증분100)** | 🟡 곡/성경/PPT/미디어/파일/텍스트(공지)/**Word 문서** 추가 + 병합. Word 는 OfficeLib.WordDoc 으로 **본문 텍스트만 추출**해 Notice 항목으로 추가(이미지 페이지 렌더 아님 — 긴 문서는 한 화면, 페이지네이션 후속). legacy .esw(XML) 가져오기는 증분101·102 에서 구현(아래 행 참조) |
| 이동 ↑↓ / 드래그 재정렬 / 제거 / 전체 비우기 | Move·드래그 + **맨위/맨아래 이동(증분96)** + Remove + 전체 비우기(84)·되돌리기 + 항목 복제(95) | 🟢 이동 ↑↓·**맨 위로/맨 아래로 한 번에**·드래그 재정렬 + 전체 비우기/1단계 되돌리기 + 항목 복제. 모던 재정렬 UX 보강 |
| 드래그 소스 다양(곡/Info/PPT/미디어/이미지→배경/성경구절) | 큐 내부 재정렬 + 성경 본문→큐(43) + **라이브러리 곡→큐(증분99)** | 🟡 큐 재정렬 + 성경 구절 드래그(43) + 라이브러리 곡 목록 드래그→예배 순서 드롭(드롭 위치 앞에 추가). PPT/미디어/이미지 외부 드래그만 후속 |
| 자동 회전(One/One-Repeat/Group/Group-Repeat 4종 + Rotate Style) | `ToggleAutoRotate`(간격) + **4종 모드**(AutoRotateMode 콤보: 현재항목반복/한항목만/그룹/그룹반복) | 🟢 (증분81) 끝 절·슬라이드 도달 시 모드별 동작(반복/정지/다음 항목/첫 항목 순환). Rotate Style 세부만 후속 |
| Gap Item 처리 | GapItem 렌더 | 🟡 조작 UI 약함 |
| 항목 검증(`ValidateWorshipListItems`: DB존재/PPT설치/삭제됨) | `WorshipListValidator`(파일) + **곡 DB 존재 검증(증분98)** + 도구 메뉴 "예배 순서 검증" + 좌측 경고 패널 | 🟢 파일 차원(깨진/이동·삭제된 PPT·미디어) + **곡 DB 존재 검증**(song:{id} 항목이 가사 DB 에 있는지 비동기 점검, 없으면 SongNotInDatabase 경고; DB 경로 없으면 생략하고 상태바에 명시). |
| .esw 저장/로드 + **legacy v3.2**(`Load32WorshipList`) | Save/Load(.esw 신규=JSON) + **legacy .esw 가져오기(증분101 파서 + 증분102 매핑·UI)** | 🟡 레거시 .esw 는 바이너리가 아니라 **XML**(FrmMain.LoadIndexFile=XmlTextReader)임을 확인 — 순수 파서 `EswWorshipListParser`(종류코드 D/P/B/M/T/I/W·식별자·제목·폴더·FormatData 추출) + `ImportEswWorshipList`(종류코드→WPF 항목 매핑: **D→곡**(라이브러리에 같은 SongId 있으면 가사·번호·저작권까지, 없으면 제목만)·**P→PowerPoint**·**M→미디어**(둘 다 파일 참조 ContentPath 보존)·**B→성경 참조**·**T/I/W→텍스트(공지)**) + 파일 메뉴 "레거시 예배 순서(.esw) 가져오기..."(파일 선택→파싱→가져오기, 빈/손상 파일은 현재 큐 보존). **최선 노력**: 항목 순서·제목·종류·곡 가사(라이브러리)는 복원하되, PPT/미디어 경로 재해석·성경 본문 확장·라이브러리에 없는 곡 가사는 가져오기 후 운영자 보정(원본 환경 의존). 신규 .esw=JSON 저장/로드는 별개로 존재 |
| 세션 노트 편집 | 세션 메모 창(WorshipSessionNotesWindow — 예배 순서 이름별 메모 편집·저장) | 🟢 메뉴 "세션 메모..." → 자유 메모 편집, 닫을 때 자동 저장 |

### 3.5 편집
| FrmMain | WPF | 상태 |
|---|---|---|
| 곡 편집(`FrmEditItem`) | SongEditorWindow | 🟢 |
| 성경 항목 편집(`FrmEditBibleItem`) | — | 🟡 BibleWindow는 선택만 |
| 노트/코드 편집(`FrmEditNotes`) | SongEditor notation 일부 | 🟡 |
| InfoScreen 편집(`FrmInfoScreen`, 7,337줄 가사편집기) | — | 🔴 |
| 팝업 텍스트 편집(`FrmPopupText`) | SongEditor 인라인으로 obsolete | ⚪ 불요 |
| 항목별 인라인 포맷(`Ind_*` 즉시 반영) | **증분106~113 절 순서 + 글자색/정렬/크기/글꼴/배경색/배경이미지/강조** + 우클릭 개별서식 토글 | 🟢 선택 곡의 **절 순서**(입력칸 "1 2 C 3 C C") + 우클릭 **글자색**(29)·**정렬**(31)·**글자 크기**(47)·**글꼴명**(43)·**배경색**(26)·**배경 이미지**(코드61)·**강조 굵게/기울임/밑줄**(코드41 비트 토글) — **8종 인라인 서식**을 공통 헬퍼(Parse→한 필드만 바꿔 Encode, 나머지 보존)로 FormatData 편집, 주면 개별서식 자동 켬, 즉시 라이브 재송출, 곡만 활성 + "개별 서식 사용" 토글. **이중 언어(보조 영역 Region2)**: 우클릭 "이중 언어(보조 영역)" 하위 메뉴에 **글자색**(코드30)·**정렬**(코드32) — 비우면 본문 추종, 이중언어 곡만 보임. Region2 크기·글꼴·강조는 후속. 항목 미디어 배정(코드51)은 출력 미소비라 미노출(정직) |
| 더블클릭 미리보기→편집 | — | 🔴 |

### 3.6 설정·템플릿
| FrmMain | WPF | 상태 |
|---|---|---|
| Options 다이얼로그(`FrmOptions`: 폴더목록/미디어dir·ext/성경목록/히스토리/이중모니터/탭표시) | SettingsWindow | 🟢 |
| 개별/기본 설정 템플릿(.est) | 출력모양 템플릿 1종 | 🟡 |
| Use Individual Settings / Apply to All Except InfoScreens / Default Layout | 개별 서식 토글 + 전 항목 전역 적용 + **출력 모양 기본값 복원(증분72)** | 🟢 개별 서식 토글·전 항목 전역 적용·기본값 복원(Default Layout, 출력 모양 전체 리셋) 모두 구현 |
| 배경색+패턴/picture mode/transition 저장 | 🟡 일부 | 🟡 |
| 창 상태(splitter/bounds) 레지스트리 저장 | **메인 창 크기·위치·최대화(88)** + **인스펙터 접힘(89)** + **좌측 패널 splitter 비율(90)** 저장/복원 — 화면 밖/모니터 분리/해상도 변경 보정 순수 헬퍼 | 🟢 메인 창 bounds + 인스펙터 펼침/접힘 + 브라우저/예배순서 패널 높이 비율 저장·복원 모두 구현(닫을 때 저장, 열 때 보정). 창 상태 저장 트랙 완비 |

### 3.7 조옮김·코드·음악 — 🟢 핵심 운영 토글 구현(증분27/30/69)
Transpose Up/Down semitone(▲/▼), To Capo 0(원조 복귀), Show Notations(코드 표시 토글, 증분69) → **출력 메뉴에 운영 토글 모두 노출**. Key/Capo/Timing 표시는 SongEditor 미리보기에 일부; Capo는 곡 메타데이터 필드로 import/export 통과(라이브 조옮김은 코드 표시 on 일 때 ±반음 연산).

> ✅ **해결됨(2026-06-01, 증분 27)**: 위 mis-bound 결함을 근본 수정하고 실제 코드 렌더(옵션 ①)를 구현했다.
> - **mis-bind 제거**: 상태 라벨("LIVE")이 더 이상 `LyricsMonitorShowNotations`(NotationVisibility)에 묶이지 않는다 — 라벨은 밴드 가시성(PanelOverlayVisibility)에 위임. 죽은 `NotationVisibility` 속성·씬 필드(`OutputSceneSnapshot`/`LiveOutputRenderSettings`)도 제거(이름·동작 불일치 해소).
> - **실제 코드 렌더**: `LyricsDisplayFormatter.ExpandNotations` 가 "코드 표시" on 일 때 각 가사 줄의 '»' 뒤 코드를 가사 줄 "위"에 끼워 송출한다(레거시 ShowNotations 대응). 설정은 `LiveQueueItem.ShowNotations`→`ComputeBodyText` 경로로 본문에 반영되고, 운영 중 토글하면 MainViewModel 이 라이브 곡을 같은 절로 재송출한다(라이브 즉시 반영).
> - **기본 off**(회중 화면은 예부터 코드를 숨김) → 끄면 본문 비트 동일(무회귀). 절(페이지) 수는 켜고/끄고에 무관(코드 줄은 빈 줄·마커가 아니라 절 경계 불변). 단위 테스트로 패리티 고정.
> - ⚠️ **남은 한계(정직)**: 코드는 가사와 같은 글꼴·문자열로 "윗줄"에 송출된다 — 음절 위 정확한 모노스페이스 정렬(레거시의 픽셀 측정 배치)은 아직 아니다(코드 데이터의 작성 공백에 의존). 정밀 정렬은 후속.
> - 〔조옮김(Capo ↑/↓·To Capo 0)은 SongEditor 미리보기에 이미 있고(증분 5), 라이브 출력 조옮김 연산은 별도 후속.〕

### 3.8 데이터 작업 (별도 창으로 대부분 포팅)
Import/ImportFolder/Export/Generate RTF·HTML/Copy/Move/Delete/Recover/Empty/SmartMerge/Usages/Find → 🟢 WPF 창 존재. **Compact&Repair ✅(2026-06-02 증분 40 — 백업 후 VACUUM, 도구 메뉴 "데이터베이스 압축·정리").** 단 Clear All Formatting, Clear Registry, Listing of Folder는 🔴 미포팅. (통합=별도창이라 "흩어짐"은 별개 과제.)

### 3.9 키보드/후킹/단축키
| FrmMain | WPF | 상태 |
|---|---|---|
| 전역 후킹 Black(F9/F10), Slide ↑↓(Arrow/Ctrl+Arrow) | ShortcutRegistry 일부 | 🟡 |
| 미디어플레이어 전역키(Esc/Space/Enter/S/M) | **증분104**: 순수 MediaPlayerKeyMap+MediaPlayerKeyRouter + RestartCommand | 🟢 라이브로 미디어 재생 중일 때 Space=재생/정지·Enter=처음부터·S/Esc=정지·M=음소거(레거시 FrmMain.HandleMediaPlayerKey 충실). 미디어 없으면 Space=다음 항목 등 평소 단축키 유지(CanExecute 게이팅), 버튼 포커스 시 Space/Enter 는 그 버튼 우선(라우터 가드). 레거시 Esc 의 "영상 숨김→가사 복귀"(Unload)는 후속 |
| 절 라벨/숫자 키 매핑(`KeyboardMapping`) | **숫자(1~9)·문자(c·b·t·w·e·p·q) 키 → 절 점프(증분91)** — 순수 VerseJumpKeyMap + 포커스 가드 | 🟢 라이브 중 숫자/문자 키로 해당 절·후렴 라벨로 즉시 점프(JumpToLyricsSection 재사용). 텍스트 입력·수식 키·없는 라벨이면 가로채지 않음 |
| ⌘K 명령 팔레트 | 25개(+항목 복제 Ctrl+D, 증분97) | 🟢(신규, FrmMain엔 없던 개선) — 항목 복제도 Ctrl+D 단축키·팔레트 검색 |

---

## 4. BibleWindow ↔ FrmMain 인라인 성경 (상세 — 사용자 지정 대조)

FrmMain의 성경은 **메인 창에 인라인**(Bibles 탭)이고, 운영 중 즉시 쓰는 풍부한 기능을 가진다. WPF는 `BibleWindow`(별도 모달) + 좌측 성경 탭으로 나뉘며 **기능이 크게 축소**됐다.

| FrmMain 인라인 성경 기능(메서드) | WPF BibleWindow / 성경 탭 | 상태 |
|---|---|---|
| 버전 탭 + 책 콤보 선택(`TabBibleVersions`,`BookLookup`) | 버전·책 ComboBox(`BibleVerseFinder`) | 🟢 |
| **Typed reference 입력**("창 1:1-2:3", `BibleUserLookup`+`BibleUserLookupValidation`+Go) | `BibleReferenceParser`+`BibleViewModel.JumpToReference`(성경 탭 입력창+이동/Enter) | ✅ (2026-06-01 증분 28) — "창 1:1-2:3"·"John 3:16"·"1요한 4:7-8" 파싱→책 해석→절 범위 선택→예배순서 추가(드래그 선택과 동일 BuildSelection 경로 재사용) |
| 구절 검색(phrase, `Bibles_Go`/`BibleVerseSearch`/매치모드) | 매치모드 ComboBox + 검색 | 🟢 |
| 본문에서 드래그 선택→passage 빌드(`HB_BuildSelectionString`, 순차/임의, max verses) | `PassageText` 드래그 선택→`BibleSelection` | 🟢 |
| **Add Region 2**(이중언어 번역 합치기, `CMenuBible_AddRegion2`,`AddRegion2ToBiblePassage`,`BuildBibleTextR2SubMenus`) | Region2 미리보기 콤보 + **실제 이중언어 송출**(`BibleRepository.ExpandSelection`→`[region 2]` 본문 조립→Region1/Region2 동시 밴드) | 🟢 (2026-06-02 증분 41) **회중 화면에 한/영 두 본문 동시 송출**. 절 단위 페이지(절 이동·위치 라벨)·번역 인용부호 »…« 보존 |
| **Copy to InfoScreen**(`CMenuBible_CopyInfoScreen`) | 본문 우클릭 "선택 구절 공지 화면으로" (NoticeScreen 편집기 프리필) | 🟢 (2026-06-02 증분 38) |
| 우클릭 메뉴(SelectAll/Unselect/Add&Show/Copy/AddRegion2/CopyInfoScreen) | 본문 우클릭(전체 선택·복사·예배 순서 추가·**Region 2와 함께 추가 서브메뉴**·공지 화면으로) | 🟢 (2026-06-02 증분 38·42) 핵심 + **AddRegion2 서브메뉴**(보조 버전 목록에서 골라 이중 언어로 추가, 증분 41 본문 송출과 연동) |
| 본문 드래그→예배순서 드롭(`BibleText_MouseDown` DragDrop) | Insert 버튼 + **본문 선택 드래그→예배 순서 목록 드롭** | 🟢 (2026-06-02 증분 43) 선택 글자 위를 끌어 큐의 원하는 위치(타깃 항목 앞)에 추가. 참조 일치 삽입(중복 항목 안전) |
| 구절 표시 토글(`Bibles_ShowVerses`) | `ShowVerses` CheckBox | 🟢 |
| 버전 추가/삭제/순서변경/이름변경 | BibleVersionManagerWindow(직전 구현) | 🟢(신규) |

**핵심**(2026-06-02 갱신): typed-reference 입력(증분 28)·copy-to-InfoScreen(증분 38)에 이어 **Region 2 실제 이중언어 송출(증분 41)** 이 동작한다 — 성경 항목이 IdString 만 들고 제목만 송출하던 과거 한계를 넘어, `BibleRepository.ExpandSelection` 이 구절 본문을 절 단위로 펼치고 보조 버전을 `[region 2]` 로 합쳐 **회중 화면에 한/영 두 본문을 동시 송출**한다(곡 이중 언어 렌더 재사용 — 영역별 폰트·정렬·색). 절 이동·위치 라벨·번역 인용부호(»…«) 보존까지 포함. 남은 성경 갭: 본문 드래그→예배순서 드롭, AddRegion2 우클릭 서브메뉴 UI.

---

## 5. production을 막는 치명적 갭 Top 10

1. **이중 언어(Region 1/2) 출력 렌더링 전무** — 한/영·한/중 동시 송출이 불가. 다국어 회중에 사실상 사용 불가. (FrmMain의 `ShowRegion*`,`Interlace`,`R1/R2 Colour/Align/Font/Size`) — 단 `SongFormatData` 디코더가 region2 필드를 이미 파싱하므로 **렌더 경로만 구축**하면 되는 상태(완전 백지는 아님).
2. **Display Panel(송출 하단 정보 바) 부재** — 제목/저작권/절·슬라이드/이전·다음 송출 정보 바 개념 자체 없음.
3. **코드/악상·조옮김(Transpose/Capo/Notations) 부재** — 찬양 연주팀 운영 핵심.
4. **절 라벨 직접 점프 부재** — 운영 중 "후렴으로", "3절로" 즉시 이동(1~9/c/b 버튼)이 없음. 순차 이동만.
5. **Preview↔Output 2단 운영 부재** — FrmMain은 미리보기에서 준비→출력 전송이 분리. WPF는 단일 라이브라 "다음 곡 미리 준비" 워크플로 약함.
6. **항목 검증(ValidateWorshipListItems) 부재** — 깨진 PPT/삭제된 곡을 라이브 직전에 거르지 못함(예배 중 사고 위험).
7. ~~**placeholder 더미 큐** — 시작 시 가짜 항목 3개 시드(`SeedPlaceholderQueue`).~~ ✅ (2026-06-01 증분 31) 더미 시드 제거 → 빈 큐로 시작 + 좌측 패널 빈 상태 안내(`IsQueueEmpty`). 실제 큐 도메인 plumbing(곡·성경·파일 추가, 최근 예배 순서 불러오기)은 이미 동작.
8. **PraiseBook 운영 UI ✅(2026-06-02 증분 44 — 머리글자 색인·명명 저장/열기/삭제·HTML 내보내기 + 곡 더블클릭→예배 순서 추가, SongId 정확 해석) / Worship Sessions·Session Notes·Recent Edits 부재** — PraiseBook 인터랙티브 목록은 포팅 완료. 나머지 곡 라이브러리 운영 보조(세션/노트/최근 편집)는 후속.
9. **이미지/배경 라이브러리** — 배경 운영이 곡별 FormatData 이미지뿐. ~~배경 표시 모드 부재(UniformToFill 고정)~~ ✅ (2026-06-01 증분 32) 배경 표시 모드(채움/맞춤/가운데/타일) 추가 — `LyricsBackgroundMode` 설정+ImageBrush. (이미지 라이브러리 브라우저는 증분 8b 에 존재.)
10. **전환 효과·중번체·Lyrics Monitor 메시지·미디어 전역키** 등 라이브 보조 기능 다수 부재.

---

## 6. 정직한 커버리지 결론

- **UI 표면**: 인터랙티브 컨트롤 수 기준 WPF ≈ FrmMain의 **15~20%**, 그나마 핵심인 **인-셸 포맷팅(Region/헤딩/전환/배경/Display Panel/조옮김)은 ~10% 미만**.
- **운영 렌더링 기능**: 이중언어·Display Panel·코드·조옮김·절점프·전환이 빠져 있어 **실효 ~10%대**. 사용자가 말한 "5%"는 *이 영역* 기준으로 과장이 아니다.
- **데이터 관리 창**(Import/Export/Copy/Move/검색 등)은 별도 창으로 ~75% 포팅됐으나, **운영 셸의 본질(라이브 다국어 송출)은 거의 비어 있다**.
- 직전 per-song 폰트/배경·성경 CRUD 작업은 이 그림에서 **소수점 단위 기여**였고, 골격 갭을 줄이지 못했다.

---

## 7. 권장 우선순위 (운영 가치 기준 — 골격부터)

1. **P0 — 큐 도메인 plumbing 실체화**: `SeedPlaceholderQueue` 더미 제거 → 실제 항목 로드/검증(`ValidateWorshipListItems` 대응). 모든 운영 폼의 공통 선결.
2. **P0 — 이중 언어(Region 1/2) 렌더 파이프라인**: `OutputSceneSnapshot`을 단일→이중 영역으로 확장(영역별 텍스트/색/정렬/폰트), `[region 2]` 마커 파서, 인터레이스. **이 프로그램을 "쓸 수 있게" 만드는 단일 최대 항목.**
3. **P1 — 인-셸 포맷팅 인스펙터 확장**: 절 라벨 점프, Display Panel, 전환 효과 UI, 배경 이미지 모드, Use Individual Settings.
4. **P1 — 코드/조옮김**: Transpose ↑↓, To Capo 0, Show Notations(+preview).
5. **P2 — 콘텐츠 브라우징 보강**: 이미지 라이브러리, PraiseBooks, 세션/노트, InfoScreen 편집기 트랙.
6. **P2 — 운영 견고성**: 항목 검증, legacy v3.2 로더, 리모컨/런치스크린 버스, 미디어 전역키.

> 결론: WPF는 "현대적 단일 콘솔"의 **뼈대와 일부 P0 인프라**는 갖췄지만, **다국어 라이브 송출이라는 EasiSlides의 본질 기능은 아직 비어 있다**. production 전환을 말하려면 최소 위 P0 2건(큐 실체화 + 이중언어 렌더)이 선결되어야 한다.
