# FrmMain → WPF UI/UX + 기능 포팅 로드맵

> 목표(사용자 /goal): **FrmMain의 UI/UX를 완벽하게 포팅 + 아이콘·디자인을 현대적 WPF 스타일로 업그레이드, 기능까지.**
> 기준 문서: [frmmain-vs-wpf-detailed-gap.md](frmmain-vs-wpf-detailed-gap.md)(전수 갭) · 원칙: 증분마다 TDD(테스트 먼저→실패→구현→통과)→code-reviewer→빌드/심볼확인→커밋/푸시. **dead UI 금지**(백엔드 없는 컨트롤은 비활성+툴팁으로 정직 표기).
> 검증 도구 주: 요청된 `$gstack-qa`·`$gsd-verify-work`(GSD/gstack)는 환경 미설치 → 표준 검증(전수 `dotnet test` + code-reviewer + Release 빌드/심볼)으로 대체하고 그 사실을 매 증분 보고.

## 증분 계획 (의존성 순)

| # | 증분 | 성격 | 갭 근거 | 상태 |
|---|---|---|---|---|
| 1 | **현대적 메뉴바**(파일/편집/보기/출력/도구/도움말, EsIcons+wpfui) — 기존 명령·DI 창에 배선 | UI/UX + 기능(기존 배선) | §2.1 메뉴 52→0 | ✅ 완료(9e0fb74) |
| 1b | **F-키 단축키(F12/F9/F3/F1) + 메뉴 제스처 힌트** — FrmMain 라이브 운영 키 파리티 | UI/UX + 기능 | §2.1, §3.9 | ✅ 완료(c8ff422) |
| 2 | **Region 1/2 이중언어 출력 렌더 파이프라인** — 디코더(이미 region2 파싱)→스냅샷 이중영역→렌더→VM→XAML | 기능(골격) | §3.2, §5 #1 | ⬜ 차기(최대 갭) |
| 3 | **인-셸 per-region 포맷 인스펙터**(영역별 색·정렬·폰트·표시모드 R1/R2/Both·인터레이스) — 2 위에 UI | UI/UX + 기능 | §2.2 | ⬜ |
| 4 | **절 라벨 직접 점프**(1~9·후렴·브릿지 버튼 → 절 인덱스 이동) | UI/UX + 기능 | §3.1 #절점프 | ✅ 완료(119600c) |
| 5 | **코드/악상 표시 + 조옮김**(ShowNotations 정상 배선 → 코드 렌더, Transpose ↑↓, To Capo 0) | 기능 | §3.7(+mis-bind 결함) | ⬜ |
| 6 | **Display Panel**(송출 하단 정보 바: 제목/저작권/항목번호/절·슬라이드/이전·다음) | UI/UX + 기능 | §3.2, §5 #2 | ⬜ |
| 7 | **콘텐츠 브라우징 보강**(이미지 라이브러리·배경 적용, 폴더 정렬/번호, 미디어·PPT 폴더 브라우징) | UI/UX + 기능 | §2.3, §3.3 | ⬜ |
| 8 | **전환 효과 UI**(항목/슬라이드 전환 — 서비스는 이미 존재, 인스펙터 노출) | UI/UX | §3.2 #전환 | ⬜ |
| 9 | **항목 검증·실제 큐 도메인 강화**(SeedPlaceholderQueue 더미 제거, ValidateWorshipListItems 대응) | 기능(견고성) | §5 #6·#7 | ⬜ |
| 10 | **세부 정리**(우클릭 컨텍스트 메뉴, 단축키 F3/F5/F9/F11/F12 매핑, 헤딩 AsRegion·Verse 헤딩) | UI/UX | §2.1, §3.9 | ⬜ |

> 각 증분은 독립 커밋/PR. 1·3·4·6·8은 직접적 UI/UX, 2·5·9는 기능 골격, 7·10은 보강. **2가 3의 선결**(per-region UI는 이중영역 렌더가 있어야 의미). 1은 의존성 없어 먼저 착수.

## 진행 로그
- (2026-06-01) 로드맵 수립.
- (2026-06-01) ✅ 증분 1 — 현대적 메뉴바(6대메뉴, EsIcons+wpfui, 컨텍스트 주입). 919 green, code-reviewer Approve, 커밋 `9e0fb74`.
- (2026-06-01) ✅ 증분 4 — 절 라벨 직접 점프(GetSectionLabels + JumpToLyricsSectionCommand + Preview SectionJumpBar). 934 green, code-reviewer Approve, 커밋 `119600c`.
- (2026-06-01) ✅ 증분 1b — F-키 단축키(F12/F9/F3/F1) + 메뉴 제스처 힌트. 942 green, 커밋 `c8ff422`.
- (2026-06-01) ✅ 증분 (상태바) — 하단 상태바(예배순서 수·절위치·상태·모니터, VM 무변경). 947 green, 커밋 `eb0aab2`.
- (2026-06-01) ✅ 증분 2 슬라이스 1 — **Region 1/2 region-aware 파서**(GetRegionPages, [region 2] 분리, 단일영역 무회귀). 956 green, 커밋 `c08d5ff`.
- (2026-06-01) ✅ 증분 2 슬라이스 2 — **이중언어 라이브 송출 배선**(GoLive 결합 송출 + VM 영역-인식 페이지수). [region 2] 곡이 두 언어를 회중 화면에 동시 송출(동작·가시). 966 green, code-reviewer Approve, 커밋 `57ae22f`.
- (2026-06-01) ✅ 증분 2 슬라이스 3 — **영역별 독립 렌더**(Region1/Region2 별도 본문 + 영역별 색(코드 30), 출력 XAML StackPanel 두 본문). [region 2] 곡이 두 언어를 독립 색으로 동시 송출. 단일 영역 무회귀(외곽선 정렬·높이 상한 code-review 반영). 969 green, 커밋 `191e022`.
- (2026-06-01) ✅ 증분 2 슬라이스 4 — **이중언어 Sequence 지원**("1 C 2 C"가 이중언어에도 적용). 974 green, 커밋 `dd80e16`.
- (2026-06-01) ✅ 증분 2 슬라이스 5 — **Region2 독립 정렬**(코드 32, R1 왼쪽·R2 오른쪽 등). 977 green, 커밋 `668e9aa`.
- (2026-06-01) ✅ 증분 2 슬라이스 6 — **Region2 독립 폰트**(코드 44/48). 980 green, 커밋 `6c672ae`.
- ✅ **이중 언어(Region 1/2) 트랙 영역별 완전 독립 완성**: 파서·송출·영역별 **색·정렬·폰트(이름+크기)**·Sequence. 잔여 minor: 이중언어 절점프 라벨(편의 기능, 추후).
- (2026-06-01) ✅ 증분 5(조옮김) 슬라이스 1 — **ChordTransposer 핵심 로직**(루트·접미사·슬래시·±반음). 1001 green, 커밋 `ad17650`.
- (2026-06-01) ✅ 증분 5(조옮김) 슬라이스 2 — **SongEditor 미리보기 조옮김 UI**(♯+/♭−/원조, 코드만 이동·저장 가사 불변). 1002 green, 커밋 `557ae6a`.
- (2026-06-01) ✅ 증분 6(Display Panel) 슬라이스 1 — **출력 곡 번호 표시**(FrmMain "Show Item Number"). 설정·렌더·데이터·VM·XAML·메뉴 토글 종단(15편집, 기본 off 무회귀). 1021 green, code-reviewer Approve, 커밋 `c37d61e`.
- (2026-06-01) ✅ 증분 6(Display Panel) 슬라이스 2 — **출력 저작권 표시**(FrmMain "Show Copyright Information"). SongSummary.Copyright(GetSongs COPYRIGHT 컬럼) → LiveQueueItem → GoLive → CurrentItemCopyright → ShowsCopyright → OutputWindow 하단 중앙 Border(좌/우 코너 비충돌). 설정·렌더·VM·메뉴 토글 종단(12파일, 기본 off 무회귀). 1025 green, code-reviewer 코너 충돌 MAJOR 수정(우하단→중앙), 커밋 `b0ca00a`.
- (2026-06-01) ✅ 증분 6(Display Panel) 슬라이스 3 — **출력 다음 항목 표시**(Display Panel PrevNext). MainViewModel.ComputeNextTitle(큐 Id 매칭, 마지막=빈문자열) → ResolveLiveProjection.NextTitle → GoLive → CurrentItemNextTitle → ShowsNextItem → OutputWindow 우측 상단 Border("다음 ▶ ..."). 설정·렌더·VM·메뉴 토글 종단(기본 off 무회귀). 1031 green, code-reviewer Approve(0 critical/major), 커밋 `6efcd90`.
- (2026-06-01) ✅ 증분 7(전환 효과) 슬라이스 1 — **출력 페이드 전환 UI**(FrmMain 전환 효과, 페이드만 구현). 설정 UseFadeTransition(기본 true=기존 250ms 보존)·TransitionDurationMs(0~2000) + 메뉴 "전환 효과 ▸ 페이드 사용/빠르게·보통·느리게". **근본 버그 수정**: `EasiSettingKeys.All` 누락으로 곡번호·저작권·다음항목 토글이 라이브 즉시 반영 안 되던 잠복 버그(5키 All+LegacyMap 등록, 회귀 가드 추가). 1037 green, code-reviewer Approve(0 critical), 커밋 `94793e0`.
- (2026-06-01) ✅ 증분 8(이미지) 슬라이스 1 — **출력 전역 배경 이미지**(FrmMain Images 탭 핵심 기능, 배경 적용). 설정 LyricsMonitorBackgroundImagePath(기본 빈값=무회귀) + 렌더 우선순위(곡별 61 우선, 없으면 전역) + 메뉴 "배경 이미지 ▸ 선택.../지우기". 1041 green, code-reviewer Approve(0 critical), 커밋 `edae914`.
- (2026-06-01) ✅ 증분 8(이미지) 슬라이스 2 — **이미지 썸네일 갤러리 브라우저**(FrmMain Images 탭). ImageLibraryService(폴더 열거)·ImageLibraryViewModel(Load·적용·하위폴더 토글)·ImageLibraryWindow(WrapPanel 썸네일·폴더 선택·더블클릭 적용). 메뉴 "배경 이미지 ▸ 갤러리에서 선택...". 1053 green, code-reviewer Approve(0 critical, includeSubfolders 체크박스 연결로 해소), 커밋 `76796d6`. **백로그(P2)**: 대용량 폴더 썸네일 비동기/지연 디코딩(현재 동기).
- (2026-06-01) ✅ 증분 9(PraiseBook) 슬라이스 1 — **찬양집 색인 브라우저**(FrmMain PraiseBook/Listing 초성 그룹핑). PraiseBookIndexService(한글 초성·영문·숫자·기타 그룹화, 가나다 순)·VM·읽기전용 중첩 ItemsControl 창 + 메뉴 "도구 ▸ 찬양집 색인...". 1066 green, code-reviewer Approve(0 critical), 커밋 `96af0d4`.
- (2026-06-01) ✅ 증분 9(PraiseBook) 슬라이스 2 — **명명 찬양집 저장/관리 영속화**(FrmMain PraiseBookDir). StoreFileNaming(경로 안전 공통 헬퍼 추출, WorshipListStore 도 위임)·PraiseBookStore(JSON 저장/불러오기/목록/삭제/이름변경)·VM SaveAs/Open/Delete + 창 콤보·이름 다이얼로그. 다른 찬양집 이름 충돌 시 실수 덮어쓰기 방지. 1082 green, code-reviewer Approve(0 critical), 커밋 `9fb2376`.
- (2026-06-01) ✅ 증분 9(PraiseBook) 슬라이스 3 — **찬양집 색인 HTML 내보내기**(FrmMain GenerateIndexReport). PraiseBookIndexExporter(머리글자 그룹·제목·번호 표 HTML, 특수문자 이스케이프=주입 방지)·VM BuildIndexHtml·창 SaveFileDialog 저장(try/catch 안내). 1088 green, code-reviewer Approve(0 critical), 커밋 `21a5c57`.
- ✅ **절 라벨 직접 점프** — `JumpToLyricsSectionCommand` + SectionJumpBar 로 이미 구현됨(FrmMain PreviewBtnVerse 1~9·c·b 대응). 갭 해소 확인.
- (2026-06-01) ✅ 증분 10(이미지 perf) — **이미지 갤러리 썸네일 비동기 디코딩**. 목록 즉시 표시 + 백그라운드 디코딩(Task.Run·Freeze) + 재진입 취소 가드(AsyncRelayCommand CancellationToken). 대용량 폴더 UI 멈춤(개선사항.md 병목) 해소. 1090 green, code-reviewer MAJOR(재진입 경쟁) 반영, 커밋 `18c54ce`.
- (2026-06-01) ✅ 증분 11(InfoScreen) — **공지 화면(자유 텍스트 출력 송출)**(FrmInfoScreen). MainViewModel.PublishNotice/ClearNotice + NoticeScreen VM/Window + 메뉴 "도구 ▸ 공지 화면". code-reviewer MAJOR 2건 근본수정: ①공지 텍스트가 가사 포맷터를 타 마커 손상되던 문제→Notice kind 면 포맷터 우회(verbatim), ②_liveItemId=null 와일드카드로 공지 중 이동 버튼 활성되던 문제→NoticeLiveId 센티넬. 재검증 에이전트로 종결 확인. 1100 green, 커밋 `156c25b`.
- (2026-06-01) ✅ 증분 12(PowerP) — **PowerPoint 폴더 브라우저**(FrmMain PowerP 탭). FolderFileEnumerator(폴더 열거 공통화, ImageLibraryService 위임)·PowerPointLibraryService(.ppt/.pptx)·VM/Window + 메뉴 "파일 ▸ PowerPoint 폴더 찾아보기". 선택·더블클릭으로 예배 순서 추가. 1109 green, code-reviewer Approve(0 issue), 커밋 `95e2089`.
- (2026-06-01) ✅ 증분 13(전환 모션) — **출력 전환 슬라이드 4방향**(페이드+좌/우/상/하). enum LyricsTransitionKind 설정 전배선 + OutputWindow TranslateTransform 슬라이드(CubicEase)+ClipToBounds + 메뉴 "전환 효과 ▸ 모션". 기본 Fade=무회귀. 1113 green, code-reviewer Approve(0 critical), 커밋 `5df4e5f`. 셰이프/타일 등 나머지 전환은 P2 백로그.
- (2026-06-01) ✅ 증분 14(Recent) — **최근 예배 순서**(FrmMain Recent Edits). RecentWorshipListsService(JSON 영속·최신순·중복제거·최대8) + MainViewModel 통합(Save/Load 시 Record)·동적 서브메뉴 "파일 ▸ 최근 예배 순서"·재오픈 명령. DI 등록. 1123 green, code-reviewer Approve(0 critical), 커밋 `2fac483`.
- (2026-06-01) ✅ 증분 15(InfoScreen 글자) — **공지 화면 글자 크기 선택**(보통40/크게60/아주크게80). 새 plumbing 없이 기존 곡별 폰트 오버라이드(FormatData 47=pt) 재사용 — PublishNotice(text,pt)→OverrideFontSizePx. 프리셋 디코더 범위 회귀 가드. 1127 green, code-reviewer Approve(0 critical), 커밋 `450b0db`.
- (2026-06-01) ✅ 증분 16(세션 메모) — **세션 노트**(FrmMain Session Notes). WorshipSessionNotesService(세션 키별 텍스트 영속, StoreFileNaming 재사용)·MainViewModel.CurrentWorshipListName(메모 키, DI 무변경)·VM/Window + 메뉴 "도구 ▸ 세션 메모". code-reviewer MINOR 2건 반영(IO 실패 거짓성공 방지·공백 판정 일치). 1143 green, 커밋 `c02f744`.
- (2026-06-01) ✅ 증분 17(전환 확장) — **줌/회전/뒤집기 전환**(트랜스폼 기반). LyricsTransitionKind += ZoomIn/ZoomOut/Spin/FlipH/FlipV. OutputWindow OnSceneChanged 를 TransformGroup{Translate,Scale,Rotate}+중앙 원점으로 재작성(Fade/Slide 무회귀). 메뉴 모션 9종. 1144 green, code-reviewer Clean(무회귀 전수 확인), 커밋 `f653ed5`.
- (2026-06-01) ✅ 증분 18(셰이프/마스크 전환) — **원형·사각 리빌 + 와이프 4방향**. 2-레이어 엔진 없이 단일 레이어 WPF Clip 지오메트리 애니메이션(EllipseGeometry 반지름·RectAnimation)으로 구현. enum += RevealCircle/RevealRectangle/Wipe×4, 메뉴 모션 15종. code-reviewer MAJOR(빠른 전환 시 이전 Completed 가 다음 클립 지우던 경쟁) 근본수정(ShouldClearClip 참조 가드 + InternalsVisibleTo 단위 테스트). 1146 green, 커밋 `f472d81`.
- (2026-06-01) ✅ 증분 19(다중 타일 마스크) — **블라인드 가로/세로 + 체커보드**. 단일 레이어 GeometryGroup(여러 RectangleGeometry) 클립 애니메이션 — BuildBlinds(8띠)·BuildCheckerboard(10×6 셀 2단계) + remaining 카운트다운 클립 정리. 빌더 internal static 단위 테스트(개수·커버리지·타이밍). enum += Blinds×2/Checkerboard, 메뉴 모션 18종. 1149 green, code-reviewer Approve(0 critical). 커밋 `ebd8c0a`.
- (2026-06-01) ✅ 증분 20(셰이프 전환 마무리) — **다이아몬드 + 양문 열기/닫기**. Diamond=마름모 PathGeometry 중심 배율 확대(AnimateScaledShapeClip 일반 메커니즘, (w+h)/2+1 오버스캔으로 전체 덮음)·Doors=2분할 클립(AnimateTileClip 재사용). 끝에 화면 전체를 덮어 잔여 마스크 없음. enum += Diamond/Doors×2, 메뉴 모션 21종. Diamond FillContains·Doors 커버리지 단위 테스트. 1152 green, code-reviewer Approve(MAJOR AA 슬라이버 반영). 커밋 `3d0e5e9`.
- (2026-06-01) ✅ 증분 21(별 전환) — **별(Star) 모션**. 별의 안쪽 골 반지름을 모서리 거리+1 로 잡으면 단일 레이어로도 끝에 전체를 덮음(골이 경계 전역 최소점이라 노치 없음 — 해석·수치 증명, code-reviewer 검증). 기존 AnimateScaledShapeClip 재사용. enum += Star, 메뉴 모션 22종. FillContains(모서리·골방위) 단위 테스트. 1153 green, code-reviewer Approve(0 critical). 커밋 `e9d1f2f`.
- (2026-06-01) ✅ 증분 22(2-레이어 엔진 + 십자) — **오목 도형 전환용 2-레이어 출력 엔진**(범위 한정). SceneChanging 이벤트(새 콘텐츠 전 발화)→뷰가 옛 프레임 RTB 스냅샷→PreviousLayer(ContentArea 뒤). 십자(Cross)가 코너에 옛 화면을 남기고 완료 시 새 화면 전환. **기존 22 단일레이어 전환 무회귀**(PreviousLayer 기본 Collapsed, Cross+duration>0 만 스냅샷). code-reviewer MAJOR 2건 근본수정(UpdateLayout 결정적 캡처·영상 송출 시 단일레이어 강등). enum += Cross, 메뉴 모션 23종. 1155 green, 커밋 `efbe6aad`.
- (2026-06-01) ✅ 증분 23(엔진 일반화) — **나비넥타이(BowTie)**. 2-레이어 엔진 위에 오목 도형 추가(좌/우 삼각형, 상/하 중앙 오므라듦). RequiresPreviousLayer 에 BowTie 추가·BuildBowTie·AnimateScaledShapeClip 재사용. 메뉴 **모션 24종**. FillContains 단위 테스트. 1156 green. 커밋 `1f7648d`. **남은 도형(P2, 장식)**: Heart(베지어)·Spiral·WindMill·FanUp 등 동일 엔진 위 도형 추가.
- ✅ **출력 메뉴 서식 파리티 완비** — 표시 토글 7(위치·곡번호·제목헤딩·외곽선·그림자·굵게·기울임) + 저작권 + 다음 항목 + 전환 효과(페이드/속도) + 배경 이미지 + 가로/세로 정렬 + 글자 크기 + 줄 간격(전부 인스펙터와 동일 설정 공유, 마우스 운영자도 메뉴로 전부 제어).
- **차기(후속 슬라이스): PraiseBook 명명 저장·관리 영속화 · 이미지 썸네일 비동기 디코딩(대용량) · 50여 종 셰이프/타일 전환 · InfoScreen 편집기 · 절 라벨 점프(P2 백로그).** 각 슬라이스를 단계별 빌드+code-review로 무회귀 검증하며 진행.

> 현황(정직): FrmMain 전체 포팅은 다증분 장기 과제다. UI/UX 표면(메뉴·단축키·상태바) + 운영 기능(절 점프) + **최대 갭이던 이중언어 송출이 이제 동작**한다(슬라이스 2). 남은 본질 갭: 영역별 스타일 분리·Display Panel·조옮김·전환 UI·이미지/PraiseBook 브라우징. "완벽 포팅"은 증분 3·5~10 완료 시 달성.
