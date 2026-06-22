# EasiSlides UI/UX 현대화 계획서 (v1.1 — Reviewed)

> 작성일: 2026-05-27 (v1.0) / 리뷰 반영: 2026-05-27 (v1.1) · 대상: EasiSlides v2.6.4 → v3.0 (UI 풀-리프레시)
> 입력: ui-ux-pro-max 스킬, ux-ui-design-reviewer 에이전트(현재 상태 감사), 사용자 결정 4건 + 리뷰 합의 8건(Q1~Q8)
> 상태: **결정 확정 (Ready for ADR)** — 11개 미해결 → 0개. Sprint 0 킥오프 가능.
>
> **v1.1 변경 요약**: 리뷰 의견에 따라 Q1~Q8 전부 확정(§0.5), STA 스레드 모델 리스크 추가(§10.3), 데이터 마이그레이션 도구 신설(§10.5), Sprint 0~1 PoC 우선순위 명시(§8.3).

---

## 0. 결정 사항 요약 (사용자 합의)

| 영역 | 결정 |
|---|---|
| 프레임워크 | **전체 WPF/WinUI 마이그레이션** (WinForms 단계적 폐기) |
| 비주얼 톤 | **Fluent 2 (Windows 11 네이티브)** |
| 1차 범위 | **전체 40개 폼 동시 진행** (단, 토큰·베이스 컨트롤 선행) |
| 테마/A11y | **다크모드 필수** · **라이트모드 필수** · **큰 폰트/터치 타깃** · **키보드/리모컨 우선** |

---

## 0.5 리뷰 확정 결정 (v1.1, Q1~Q8 답변 lock-in)

| # | 영역 | 결정 | 근거 요약 |
|---|---|---|---|
| **Q1** | 컨트롤 라이브러리 | **WPF UI (lepoco/wpfui)** 베이스 + EasiDS 토큰 오버레이 | Windows 11 Fluent 디자인 완전 통합. Material Design은 안드로이드/웹 멘탈모델로 데스크톱 이질감. 자체 작성은 유지보수 비용 폭증. |
| **Q2** | 시니어 모드 디폴트 | **OFF + 최초 실행 온보딩에서 선택 유도** | 파워 유저 정보 밀도 보호. 설치 후 첫 실행 웰컴 스크린에서 "운영자 연령/시력에 맞는 인터페이스 크기 선택" 명시 선택. `FrmOptions`에서 언제든 토글. |
| **Q3** | Mica/Acrylic | **라이트(사무실) ON · 다크(무대) OFF (Solid)** | 라이트는 심미적 만족, 다크는 무대 환경 인지 부하 최소화. 다크는 `Elevation.0` flat backdrop, 솔리드 단색 배경 강제. |
| **Q4** | 한국어 폰트 | **Pretendard Variable (OFL 1.1) 번들 필수** | WPF ClearType + Malgun Gothic weight 부족 + UI 스케일링 시 글자 뭉개짐. 모든 PC 동일 레이아웃 보장 위해 앱 리소스 포함 배포. |
| **Q5** | 라이브 인디케이터 위치 | **상단 LiveBar + 우측 상단 모서리 Pulse 이중 배치** | (1) 화면 최상단에 `Color.Live.Active` 라인 — 폼 전체가 라이브임을 무의식 인지. (2) 송출 중인 개별 아이템(WorshipList 행·PPT 썸네일) 우측 상단에 깜빡이는 Pulse 애니메이션 — 어떤 소스가 나가는지 직관 매핑. |
| **Q6** | Legacy 아이콘 자산 | **100% 폐기 → Fluent UI System Icons 전면 교체** | 일관성이 시각 시스템의 생명. 10% 잔존만으로도 "낡았다" 인지. 메타포 부재 시 가장 유사한 SVG 차용 또는 조합으로라도 SVG 통일. |
| **Q7** | 사용자 커스텀 자산 | **첫 실행 자동 마이그레이션 도구로 보존** | `%AppData%\Easislides` 스캔 → `%AppData%\EasislidesNext\UserAssets\`로 자동 복사 + 해상도/포맷 백그라운드 최적화. 절대 유실 금지. (§10.5 참조) |
| **Q8** | Legacy 유지보수 | **v3.0 출시 후 6개월, 크리티컬 버그/보안만** | 두 아키텍처 장기 동시 유지 = 리소스 고갈. 새 기능은 v3.0 전용. v2.6.4는 "유지보수 모드" 선언. (§10.5 참조) |

---

## 1. Executive Summary

EasiSlides는 .NET 10 WinForms로 동작하지만 시각·상호작용 시스템은 XP/Vista 시대(2005~2008년경) 패턴에 머물러 있다. 색·폰트·아이콘이 토큰화되지 않았고, 다크모드·접근성·반응형 레이아웃이 부재하다. **현재 UX Maturity = Level 1.2 / 5** (Functional이지만 Consistent에 도달 못 함).

본 계획은 **WinForms → WPF/.NET 10 (Fluent UI WPF Theme + WinUI 3 Composition 일부)** 마이그레이션과 동시에 **Fluent 2 디자인 시스템(EasiDS)** 을 신규 구축하여 다음을 달성한다:

1. **무대 운영용 다크 테마**(저조도) + **사무실 준비용 라이트 테마** 듀얼 지원
2. **시맨틱 디자인 토큰** 기반 색·타이포·간격·모션 통일 (`Color.FromArgb(20,20,20)` 매직 RGB 4회 반복 같은 잔재 제거)
3. **벡터(SVG/XAML) 아이콘 + 단일 자산 + 상태 스타일링** — `Bible.png` + `Bible - Hightlight.png` 같은 비트맵 듀얼 자산 폐기
4. **WCAG 2.1 AA** 충족 (`AccessibleName` 0건 → 100%, 액세스 키 0건 → 모든 주요 액션)
5. **라이브 운영 안전장치** — Black/Hide/Gap "Label as Button" 같은 위험 패턴 차단
6. **40개 폼 일관성** — 베이스 컨트롤 라이브러리(`EsButton`, `EsTextBox`, `EsTabView` 등)로 강제 통일

목표 Maturity: **Level 1.2 → Level 4 (Optimized UX)** within 4 quarters.

---

## 2. 현재 상태 평가 (감사 발췌)

> 전체 보고서는 §11.A 부록에 첨부. 핵심 발견만 요약.

### 2.1 시각 시스템 약점 (Designer.cs 기반)
- **매직 색**: `FrmMain.Designer.cs:1198, 1207, 1703, 1712`에 `Color.FromArgb(20, 20, 20)` 4회 반복 / `panelVideoHolder.BackColor = Color.Navy` (FrmOptions:1943), `Color.SlateBlue` (1964), `Color.LightGray` 등 동일 컴포넌트군 내 6+ 색 혼재
- **폰트 혼재**: `FrmMain.Designer.cs`에 `new Font(...)` 15회, `"Microsoft Sans Serif"`(레거시 디폴트)과 `"Tahoma"` 한 폼 안 혼재
- **간격 무토큰**: `Margin = new Padding(...)`이 `FrmMain.Designer.cs`에 **226회** 직접 호출, `Padding(3,5,3,5)` / `Padding(3)` 혼재
- **컨트롤 오용**: Black/Hide/Gap "버튼"이 실제로 `Label` + `Click` 핸들러 (`FrmMain.cs:4361~4373`) → 키보드 포커스 불가, 스크린리더 미인식

### 2.2 아이콘 자산
- 60+ raster (PNG/BMP), 벡터 0개
- 동일 의미 3상태 별파일: `BlackScreen.png` + `-Pressed.png` + `-Red.png`
- 호버 듀얼: `Bible.png` + **`Bible - Hightlight.png` (오타)**, `Media.png` + `Media-highlight.png`, 6쌍 이상
- 하이-DPI 스케일 대응 없음

### 2.3 색상/폰트 헬퍼 (`gfColorsFonts.cs`, 153줄)
- 디자인 **토큰 0개**, 시맨틱 색상명(Primary/Surface/Danger) 0개
- 다크/라이트 분기 0개
- 단순 `ColorDialog` 래퍼 + `Microsoft Sans Serif` 폴백

### 2.4 접근성
- `AccessibleName/AccessibleDescription/AccessibleRole` 사용 **0건** (`Easislides/Easislides` 전체)
- 단축키(`&` 액세스 키) **0건** — 메뉴/버튼 키 접근 불가
- `HookManager` 5파일로 전역 Win32 후킹 → 표준 키 라우팅 대체 (이식 시 보존 필요)

### 2.5 Top 5 UX 마찰
1. **동기 Office Interop UI 프리즈** — `BuildScreenPreDumps` + `DoEvents`/`Sleep`/`WaitCursor` 9회 산재
2. **거대 옵션 다이얼로그** — `FrmOptions` 491 디자이너 심볼, 한 모달에 모든 설정
3. **모달 남용** — `ShowDialog()` 63회 (`FrmMain`에서만 30회)
4. **라이브 안전장치 부재** — Label 클릭 한 번으로 즉시 라이브 토글
5. **멀티모니터 설정 복잡성** — `FrmInfoScreen` 264 심볼, `selectScreen==null` 시 과거 0-분할 버그 (CLAUDE.md §7)

### 2.6 Maturity Level
| Level | 상태 | 근거 |
|---|---|---|
| 1 Functional | ✅ | 기능 동작, 빌드 가능 |
| 2 Consistent | ❌ | 색·폰트·아이콘 토큰 없음, 매직 RGB 반복 |
| 3 Accessible | ❌ | AccessibleName 0, 액세스 키 0, 스크린리더 미지원 |
| 4 Optimized | ❌ | 비동기·적응형·다크 부재 |
| 5 Adaptive | ❌ | — |

**현재 = Level 1.2** · **목표(v3.0) = Level 4**

---

## 3. 디자인 비전 — "Stage-Ready Fluent"

**한 줄 미션**: *"무대 위 라이브 운영자가 어둠 속에서도 안심하고, 사무실 준비자가 밝은 형광등 아래서도 편안한, Windows 11 네이티브 워십 송출 도구."*

### 3.1 3대 디자인 원칙
1. **무대 안전성 (Stage Safety)** — 라이브 중인 액션은 색 + 모션 + 위치로 3중 신호. 우발 클릭 방어. 다크 베이스에서 라이브 인디케이터 즉시 인식. **무대 다크 모드는 Mica/Acrylic 등 투명 재질 금지(§Q3) — 솔리드 단색 강제**.
2. **운영자 우선 (Operator-First)** — 자주 쓰는 액션은 손가락 닿는 위치(+큰 타깃), 드물게 쓰는 설정은 패널 뒤로. 키보드/리모컨 단독 운영 가능.
3. **체계적 일관성 (Systemic Consistency)** — 디자이너 한 명이 손으로 못 박는 일관성이 아니라, 토큰·컨트롤 라이브러리·린트가 강제하는 일관성. **레거시 raster 아이콘은 100% 폐기(§Q6) — 10%만 남아도 시스템 전체가 낡아 보임**.

### 3.2 페르소나 (간략)
| 페르소나 | 환경 | 시간대 | 핵심 니즈 |
|---|---|---|---|
| **무대 운영자** | A/V 부스, 저조도, 헤드셋 | 라이브 (가장 위험) | 다크, 큰 타깃, 단축키, 라이브 인디케이터, 즉시 피드백 |
| **준비 담당자** | 사무실/PC방, 형광등 | 평일 낮 | 라이트, 검색·임포트 효율, 일괄 편집, 미리보기 속도 |
| **시니어 운영자** | 부스 양쪽 | 모두 | 16~18px 본문, 44×44px+ 타깃, 명확한 색 대비, 단순 단축키 |
| **리모컨 사용자** | 무대 뒤 | 라이브 | 키보드 후킹 호환, 시각 피드백 강조 |

---

## 4. 디자인 시스템 — "EasiDS" 토큰 명세

> 모든 토큰은 `App.xaml` `ResourceDictionary`로 라이트/다크 듀얼 정의 → 컨트롤은 `{DynamicResource}` 참조 (런타임 테마 전환).

### 4.1 색상 토큰 (시맨틱)

**라이트 모드 (사무실)**
| 토큰 | Hex | 용도 |
|---|---|---|
| `Color.Surface.Page` | `#FAFAFA` | 페이지 배경 |
| `Color.Surface.Card` | `#FFFFFF` | 카드/패널 |
| `Color.Surface.Subtle` | `#F3F4F6` | 보조 영역 |
| `Color.Text.Primary` | `#0F172A` | 본문 (대비 19:1) |
| `Color.Text.Secondary` | `#475569` | 보조 (대비 7.5:1) |
| `Color.Text.Tertiary` | `#64748B` | 캡션 (대비 5.6:1) |
| `Color.Border.Default` | `#E5E7EB` | 보더 |
| `Color.Accent.Primary` | `#0078D4` | Fluent 시스템 액센트(기본) |
| `Color.Accent.PrimaryHover` | `#106EBE` | hover |
| `Color.Accent.PrimaryPressed` | `#005A9E` | pressed |
| `Color.Live.Active` | `#DC2626` | **라이브 송출 중** (오직 이 한 가지 의미만) |
| `Color.Live.Standby` | `#F59E0B` | 대기/전환 중 |
| `Color.Live.Hidden` | `#1E293B` | Black/Hide 활성 |
| `Color.Status.Success` | `#16A34A` | 저장·완료 |
| `Color.Status.Warning` | `#D97706` | 경고 |
| `Color.Status.Danger` | `#DC2626` | 위험/삭제 |
| `Color.Status.Info` | `#0284C7` | 정보 |

**다크 모드 (무대)**
| 토큰 | Hex | 용도 |
|---|---|---|
| `Color.Surface.Page` | `#0A0A0A` | 페이지 (거의 검정, 무대 산란광 최소) |
| `Color.Surface.Card` | `#171717` | 카드 |
| `Color.Surface.Subtle` | `#262626` | 보조 |
| `Color.Text.Primary` | `#F5F5F5` | 본문 (대비 17:1) |
| `Color.Text.Secondary` | `#A3A3A3` | 보조 (대비 7.1:1) |
| `Color.Text.Tertiary` | `#737373` | 캡션 |
| `Color.Border.Default` | `#404040` | 보더 (다크에서 보이도록) |
| `Color.Accent.Primary` | `#60A5FA` | 다크용 액센트 (대비 7.2:1) |
| `Color.Live.Active` | `#EF4444` | 라이브 (어둠 속 가독성 강화) |
| `Color.Live.Standby` | `#FBBF24` | 대기 |
| `Color.Status.*` | 동일 의미 + 다크 변형 | |

**금지 규칙**
- `Color.FromArgb`/`Color.Navy` 등 매직 색상 직접 사용 금지 → 빌드 시 Roslyn 분석기로 차단 (§9.4)
- "Live" 색은 라이브 송출 중 외 어디에도 쓰지 않음 (의미 보호)

### 4.2 타이포그래피 토큰

**폰트 패밀리** (Q4 확정: Pretendard Variable 번들 필수)
- **UI 기본**: `Segoe UI Variable` (영문/Windows 11 디폴트), 폴백 `Segoe UI`
- **한글 기본**: **`Pretendard Variable` (앱 리소스 번들, OFL 1.1)** — 시스템 폰트 의존 금지. WPF ClearType 렌더링 한계 + Malgun Gothic weight 부족 + UI 스케일링 시 글자 뭉개짐 회피. 모든 사용자 PC에서 100% 동일 레이아웃 보장.
- **번들 위치**: `Easislides.Wpf/Assets/Fonts/PretendardVariable.ttf` + `App.xaml`에 `pack://application:,,,/Assets/Fonts/#Pretendard Variable` 등록. OFL 1.1 라이선스 파일 함께 동봉.
- **폴백 체인**: `Pretendard Variable, Segoe UI Variable, Segoe UI, Malgun Gothic, sans-serif`
- **모노**: `Cascadia Code` (가사 코드 표기 등)
- **금지**: `Microsoft Sans Serif`, `Tahoma` (현재 잔재) — 마이그레이션 시 제거. `Malgun Gothic` 직접 지정도 금지(폴백으로만).

**타입 스케일** (시니어 운영자 대응 — 기본을 14→16으로 상향)
| 토큰 | 크기 | line-height | weight | 용도 |
|---|---|---|---|---|
| `Type.Display` | 32px | 1.25 | 600 | 스플래시 타이틀 |
| `Type.Title1` | 24px | 1.3 | 600 | 폼 헤더 |
| `Type.Title2` | 20px | 1.35 | 600 | 섹션 헤더 |
| `Type.Title3` | 18px | 1.4 | 600 | 카드 헤더 |
| `Type.BodyLarge` | 16px | 1.5 | 400 | 본문 기본 (시니어 친화) |
| `Type.Body` | 14px | 1.5 | 400 | 보조 본문 |
| `Type.Caption` | 12px | 1.4 | 400 | 캡션 |
| `Type.ButtonLarge` | 16px | 1 | 600 | 주요 액션 버튼 |
| `Type.Button` | 14px | 1 | 600 | 기본 버튼 |

**시니어 모드 (옵션)**: 모든 본문/버튼 +20% 스케일, 한 토글로 전체 적용.

### 4.3 간격·반경·고도

**스페이싱 스케일 (4 base)**: `2, 4, 8, 12, 16, 20, 24, 32, 40, 48, 64`
→ `Padding(3,5,3,5)` 같은 비정형 값 금지. 헬퍼 `EsSpacing.Sm` (8), `.Md` (16), `.Lg` (24).

**반경(Corner Radius)**: Fluent 2 표준 — `Radius.Small=4`, `Radius.Medium=6`, `Radius.Large=8`, `Radius.Pill=∞`. 무대 다크에서 6px 통일(과도한 둥글기 자제).

**고도(Elevation/Shadow)**:
- `Elevation.0` — flat (다크 베이스에 추천)
- `Elevation.4` — dropdown, popup
- `Elevation.16` — modal, dialog
- `Elevation.32` — context menu, command bar

### 4.4 모션 토큰
- `Motion.Fast` 150ms — 호버·포커스 색 전환
- `Motion.Normal` 250ms — 패널·플라이아웃 등장
- `Motion.Slow` 400ms — 폼 전환
- **Easing**: `CircleEaseOut`(등장), `CircleEaseIn`(퇴장), `QuadraticEaseInOut`(상태 전환)
- **prefers-reduced-motion 대응**: Windows 설정 "애니메이션 표시" 끔 감지 → 모션 즉시 0ms (`SystemParameters.ClientAreaAnimation`)

### 4.5 아이콘 시스템 (Q6 확정: Legacy 100% 폐기)
- **소스**: **Fluent UI System Icons** (Microsoft 공식 MIT, SVG) **확정**. Lucide는 대안에서 제외 — Windows 11 일관성 우선.
- **포맷**: WPF UI 라이브러리의 `SymbolIcon` 우선(Fluent 시스템 아이콘 내장) → 부족분만 SVG → XAML `DrawingImage` 빌드 변환 (Inkscape CLI 또는 SvgToXaml 빌드 태스크)
- **사이즈**: 16, 20, 24, 32 — 컨트롤별 고정
- **상태 표현**: 베이스 1개 SVG + 색·투명도 토큰으로 hover/pressed/disabled → **상태별 별파일 제거**
- **레거시 자산 정책 (Q6)**: `Easislides/Resources/` + `EasislideImages/` 모든 raster(60+ PNG/BMP) **100% 폐기**. Fluent 셋에 메타포가 정확히 일치하지 않으면 가장 유사한 SVG 차용 또는 여러 아이콘 조합으로라도 SVG 통일. **부분 잔존 금지** — 10%만 남아도 시스템 전체가 낡아 보임.
- **자산 매핑 (현재 → 신규)** — 부록 §11.B 참조 (전체 60+개), 1:1 매핑 미정인 항목은 ADR-002 작성 시 확정

---

## 5. 컴포넌트 라이브러리 — "EasiControls"

`Easislides/UI/Controls/` 신규 디렉터리. 모든 컨트롤은 EasiDS 토큰 강제 참조.

### 5.1 베이스 컨트롤

| 컨트롤 | WPF 베이스 | 변종 (Style) | 핵심 속성 |
|---|---|---|---|
| `EsButton` | `Button` | `Primary`, `Secondary`, `Subtle`, `Danger`, `Ghost`, `Live`, `IconOnly` | `Size`(Sm/Md/Lg), `IsLoading`, `Icon` |
| `EsTextBox` | `TextBox` | `Default`, `Search`, `Numeric` | `Label`, `HelperText`, `ErrorText`, `Prefix/Suffix` |
| `EsComboBox` | `ComboBox` | `Default`, `Combo`, `Autocomplete` | |
| `EsToggle` | `ToggleButton` | `Switch`, `Checkbox`, `Radio` | |
| `EsTabView` | `TabControl` | `Underlined`, `Pills`, `Sidebar` | |
| `EsCard` | `Border` | `Flat`, `Elevated`, `Interactive` | |
| `EsDataGrid` | `DataGrid` | Fluent 2 row hover, zebra optional | |
| `EsCommandBar` | `ToolBar` | Top, Floating | overflow auto |
| `EsDialog` | `Window`/`ContentDialog` | `Modal`, `Drawer`, `Sheet` | |
| `EsToast` | — | Success/Warning/Error/Info | auto-dismiss |
| `EsLiveIndicator` | — | 라이브/대기/숨김 3상태 | 깜빡임 옵션 |

### 5.2 컴포지트 (도메인 특화)

| 컴포넌트 | 역할 | 대체 대상 |
|---|---|---|
| `WorshipListPanel` | 좌측 예배 순서 패널 (드래그 정렬, 컨텍스트 메뉴) | `FrmMain` 좌측 `lvWorshipList` |
| `LyricsCanvas` | 가사 미리보기 (다크 베이스, 안전 영역 가이드) | `ImageCanvas` 일부 |
| `PptThumbStrip` | PPT 썸네일 가상화 리스트 (lazy load, async) | `FrmMain` PPT 영역 |
| `MonitorMapper` | 멀티모니터 시각화 선택기 (실제 모니터 배치 미리보기) | `FrmInfoScreen` 264심볼 단순화 |
| `BibleVerseFinder` | 책·장·절 calc + 검색 통합 | `FrmFind` 일부 + 성경 UI |
| `MediaInsert` | 이미지/영상 드롭+미리보기+포맷 검증 | `FrmLaunchMediaPlayer` 일부 |
| `LiveBar` | 상단 라이브 상태바 (현재 송출 항목·모니터·전환 단축키) | 현재 분산 |
| `SafetyConfirm` | "라이브 중 ○○ 진행?" 작은 인라인 확인 (모달 X) | Label-as-Button 직접 토글 대체 |

---

## 6. 폼별 리디자인 명세 (40개)

> 각 폼 = (목적, 핵심 사용자, 현재 문제, 신규 레이아웃, 신규 컴포넌트, 단축키, 우선순위)
> 본 절은 요약. 상세는 폼별 ADR로 분리 (`docs/ui/forms/Frm{Name}-redesign.md`).

### 6.1 카테고리 & 우선순위

| Tier | 폼 | 사용 빈도 | 1차 작업 |
|---|---|---|---|
| **T0 Critical (라이브)** | `FrmMain`, `FrmLyricsScreen`, `FrmInfoScreen`, `FrmShowAlert` | 매 예배 | Sprint 1~4 |
| **T1 Frequent (편집)** | `FrmEditItem`, `FrmEditBibleItem`, `FrmEditNotes`, `FrmFind`, `FrmLaunchShow`, `FrmBackground` | 주간 | Sprint 3~6 |
| **T2 Setup** | `FrmOptions`, `FrmImport`, `FrmImportFolder`, `FrmImportAccessHelper`, `FrmExport`, `FrmGenerateDoc`, `FrmGenerateHtml`, `FrmCopyMoveExternal` | 월간 | Sprint 5~8 |
| **T3 Utility** | 나머지 21개 (FrmCopy/Move/Usages/SmartMerge/Manage*/Recover/Rearrange/Popup/Lookup/Update/SingleMonitorAlert/Splash/Help/About/Register/MediaPlayer*) | 비정기 | Sprint 7~10 |

### 6.2 T0 핵심 리디자인 (상세)

#### 6.2.1 FrmMain (현재 579 designer symbols, 8800줄 로직)
**현재 문제**: 좌측 WorshipList + 중앙 미리보기 + 우측 PPT 썸네일 + 상단 메뉴 + 하단 라이브 컨트롤이 모두 같은 레벨에 평면 배치, 라이브 상태가 시각적으로 도드라지지 않음, 모달 30회.

**신규 레이아웃** (3-zone)
```
┌─ LiveBar (상단 56px) ─────────────────────────────────────┐
│ ● LIVE  주일찬양 #3 "은혜로다"  → 모니터 2  [Space=Next]  │
├──────────┬──────────────────────────┬──────────────────────┤
│          │                          │                      │
│ Worship  │   Preview / Lyrics       │  PPT Thumbs          │
│ List     │   (LyricsCanvas)         │  (PptThumbStrip,     │
│ Panel    │                          │   가상화·lazy)        │
│ (좌 280) │   (중앙 flex)             │   (우 320)            │
│          │                          │                      │
├──────────┴──────────────────────────┴──────────────────────┤
│ CommandBar: [Bible] [Add] [Edit] [Black] [Hide] [Gap]      │
│ ─ Live 상태일 때 위험 버튼은 SafetyConfirm 인라인 확인       │
└────────────────────────────────────────────────────────────┘
```

**신규 컴포넌트**: `LiveBar`, `WorshipListPanel`, `LyricsCanvas`, `PptThumbStrip`, `EsCommandBar`, `SafetyConfirm`.

**라이브 인디케이터 이중 배치 (Q5 확정)**:
1. **상단 LiveBar** — 화면 최상단 56px 영역에 `Color.Live.Active` 라인(2px) + 텍스트(`● LIVE  주일찬양 #3...`). 라이브 중일 때 폼 전체가 라이브 상태임을 무의식 인지.
2. **개별 아이템 우측 상단 모서리 Pulse** — `WorshipListPanel`의 송출 중 행, `PptThumbStrip`의 송출 중 썸네일 우측 상단에 8px 점등 + Pulse 애니메이션(1.2초 주기, `prefers-reduced-motion` 끔 시 정적). 어떤 소스가 나가는지 직관 매핑.

두 인디케이터는 같은 ViewModel 상태(`IsLive`)에 바인딩 — 깜빡임 동기화.

**단축키** (HookManager와 호환, 후킹 우선순위 보존):
- `Space` = 다음 슬라이드, `Shift+Space` = 이전
- `B` = Black, `H` = Hide, `G` = Gap (현재 Label과 동일 키)
- `Ctrl+L` = 라이브 토글, `Ctrl+1~9` = 모니터 전환
- `F1` = 도움말 플라이아웃 (큰 단축키 치트시트)
- `Esc` = 위험 액션 취소

**우선순위**: P0. Sprint 1 시작.

#### 6.2.2 FrmInfoScreen (현재 264 symbols)
**현재 문제**: 멀티모니터 설정이 숫자/콤보 입력 + 텍스트 라벨로 구성 → `selectScreen==null` 시 0-분할 버그 이력. 직관적 모니터 배치 미리보기 없음.

**신규 레이아웃**: `MonitorMapper` 컴포넌트 — 실제 모니터 배치를 SVG로 시각화, 마우스로 해당 모니터 클릭해 송출 대상 지정. 수동 영역은 드래그·리사이즈 가능한 빨간 외곽선. 우측에 폼-숫자 미러링(고급 사용자용).

**안전장치**: `LS_Width=0` 같은 잘못된 상태는 토큰·뷰모델 레벨에서 차단 (model invariant). 0이면 UI가 경고 표시 + 송출 비활성화.

**우선순위**: P0 (라이브 안정성).

#### 6.2.3 FrmLyricsScreen
- 다크 캔버스 베이스, Safe Area 가이드 (모니터 가장자리 5% 마진 점선) 토글
- 폰트 미리보기 실시간 (현재 hardcoded `Tahoma` 8.25pt 잔재 제거)
- `FormatText` 0-분할 가드 유지

#### 6.2.4 FrmShowAlert
- Toast 패턴 (`EsToast`) — 더 이상 모달 아님
- 다크모드 대비 강한 액센트, 5초 자동 닫힘 + 마우스 호버 시 일시 정지

### 6.3 T1 편집 폼 (간략)

| 폼 | 핵심 변경 |
|---|---|
| `FrmEditItem` (255) | 좌 트리 + 우 폼 split. 가사 에디터 모노폰트, 코드 자동 정렬 |
| `FrmEditBibleItem` | Bible Verse Finder 통합 |
| `FrmEditNotes` | 미니 마크다운 에디터 |
| `FrmFind` | 즉시 검색(타이핑→결과), 결과에 다크모드 하이라이트 |
| `FrmLaunchShow` | 카드형 송출 후보 + 큰 "Go Live" 버튼 |
| `FrmBackground` | 배경 갤러리 (그리드 썸네일, 드래그 업로드) |

### 6.4 T2 설정 폼

**`FrmOptions` 491 symbols → 분해 전략**: 단일 거대 다이얼로그 폐기. 대신 **세팅스 페이지** 패턴 (`EsTabView` Sidebar + 우측 콘텐츠). 검색 박스 상단. 카테고리:
1. 일반(General)
2. 모니터·송출(Display & Output) — `FrmInfoScreen` 일부 흡수
3. 폰트·색(Appearance) — 라이트/다크 토글 포함
4. 단축키(Shortcuts) — 후킹 매핑 시각화 + 충돌 검사
5. 데이터베이스(Data) — SQLite/MariaDB 경로 + 백업
6. 미디어(Media)
7. 가져오기/내보내기(Import/Export)
8. 고급(Advanced)

(상세 ADR은 별도)

### 6.5 T3 유틸 폼
- 공통 컴포넌트로 자동 적용, 별도 디자인 비용 최소.
- Splash 2개(`FrmSplashScreen`, `FrmSplashScreenOld`) — `Old` 제거 후 신규 단일 스플래시.

---

## 7. 접근성 표준 (WCAG 2.1 AA + 시니어 모드)

### 7.1 색·대비
- 본문 텍스트 ≥ 4.5:1 (AA) — 토큰 자체가 7:1 이상 (AAA) 목표
- 큰 텍스트 ≥ 3:1
- 상호작용 요소(버튼·링크·아이콘) 배경 대비 ≥ 3:1
- **색만으로 의미 전달 금지** — 라이브 상태는 색 + 텍스트(`● LIVE`) + 위치 3중

### 7.2 키보드
- 모든 액션이 키보드로 접근 가능
- `Tab` 순서가 시각 순서와 일치 — 토큰 `TabIndex` 자동 부여 헬퍼 작성
- **포커스 링** 항상 보이게 — Fluent 2 `FocusVisualKind="Reveal"`, 색 토큰 `Color.Accent.Primary`, 두께 2px
- 액세스 키 `&` 모든 주요 버튼·메뉴에 부여 — 현재 0건 → 100%

### 7.3 스크린리더
- 모든 컨트롤 `AutomationProperties.Name` + `HelpText`
- 아이콘-온리 버튼 `AutomationProperties.Name="설명"` 필수
- 라이브 토글 등 동적 상태는 `LiveRegion="Assertive"`

### 7.4 시니어 모드 (Q2 확정: 디폴트 OFF + 온보딩 선택)
- **디폴트 OFF** — 정보 밀도를 중시하는 파워 유저 보호
- **최초 실행 온보딩(Welcome Screen)** — 설치 후 첫 구동 시 "운영자의 연령대나 시력에 맞춘 인터페이스 크기를 선택해 주세요" 화면 표시. 3가지 옵션 큰 카드:
  - 표준 (Standard) — 14px 본문, 44×44px 타깃 (기본)
  - 큰 글씨 (Large) — 16px 본문, 48×48px 타깃
  - 시니어 (Senior) — 20px 본문, 52×52px 타깃, 버튼 패딩 +50%
- **언제든 변경 가능** — `FrmOptions` → 일반(General) 카테고리 상단에 동일 토글 노출, 변경 즉시 전체 UI 재렌더(테마 전환과 동일 메커니즘)
- 다크 베이스에서도 본문 굵기 400 유지 (불필요한 굵게 자제 — 가독성)
- 토큰 스케일 곱하기 1.2/1.4 (Large/Senior) 단순 적용 — 별도 테마가 아닌 토큰 변환 함수

### 7.5 HookManager 호환
- 기존 전역 Win32 후킹(`HookManager.cs` 외 4파일)은 보존
- 신규 WPF `RoutedEvent` 시스템과 충돌 방지 — `PreviewKeyDown`에서 후킹 결과 우선 처리, 처리됨 표시 후 라우트 종료

---

## 8. 마이그레이션 전략 (WinForms → WPF)

### 8.1 기술 결정 (Q1 확정: WPF UI 베이스)
- **타겟**: `.NET 10` 유지, `net10.0-windows`, `UseWPF=true`
- **테마/스타일**: **[WPF UI](https://github.com/lepoco/wpfui) 확정** (Fluent 2 컨트롤 라이브러리, MIT). 자체 `EasiDS` 토큰 ResourceDictionary로 색·간격·타이포 **오버라이딩(Overriding)** — WPF UI 컨트롤은 베이스로만 활용, 비주얼은 EasiDS가 단일 진실. Material Design In XAML과 자체 작성안은 기각 (각각 데스크톱 이질감·유지보수 비용 사유).
- **MVVM**: `CommunityToolkit.Mvvm` (소스 제너레이터 기반 ObservableObject/RelayCommand)
- **DI**: `Microsoft.Extensions.DependencyInjection`
- **차트/그리드** (필요 시): 표준 WPF 충분, 부족 시 [LiveChartsCore](https://github.com/beto-rodriguez/LiveCharts2) (오픈소스)
- **벡터 아이콘 파이프라인**: `WPF UI`의 `SymbolIcon` (Fluent 시스템 아이콘 내장) 우선. 부족분만 SVG → XAML 변환 [Inkscape CLI](https://inkscape.org/) 또는 [SvgToXaml](https://github.com/BerndK/SvgToXaml) 빌드 태스크.
- **번들 폰트**: `Pretendard Variable` (Q4) — `Assets/Fonts/`에 동봉, OFL 1.1 LICENSE.txt 포함

### 8.2 도메인 로직 보존 (변경 최소)
유지 (수정 거의 없음):
- `OfficeLib/` PowerPoint·Word interop
- `DirectShow/` 비디오
- `Easislides/SQLite/SQLiteController.cs` DB
- `Easislides/Global/gf*.cs` 도메인 헬퍼 (Bible/Lyrics/Media/Database/Config 등)
- `Easislides/HookManager/` 전역 후킹
- `Easislides/Module/` 도메인 모델 (`SongFormat`, `SongLyrics`, `SongSettings`, `CommonEnum`)
- `Easislides/Util/` 공통 유틸

대체:
- `Easislides/Easislides/Frm*.cs` 및 `Frm*.Designer.cs` (40개) → WPF `Window`/`Page` + `ViewModel`
- `Easislides/Global/gfColorsFonts.cs` → `EasiDS` 토큰 + `IThemeService`
- `EasislideImages/` raster + `Resources/` 이미지 일부 → SVG/XAML 아이콘 + 단일 자산

### 8.3 단계적 이식 (Strangler Fig)
**바로 전체 재작성은 비추**. 다음 순서로 점진 이식 — 사용자 결정이 "동시 진행"이라도 **빌드/배포는 점진**:

1. **Sprint 0 (1주) — 기술 PoC 최우선**:
   - 새 프로젝트 `Easislides.Wpf/` 생성, WPF UI + Toolkit MVVM 설치
   - `EasiDS` 토큰 ResourceDictionary 작성, 첫 컨트롤 `EsButton` 3변종
   - **PoC-A (필수)**: HookManager 전역 후킹 ↔ WPF `PreviewKeyDown` 충돌 검증. 후킹 결과를 `e.Handled = true`로 처리하면 라우티드 이벤트가 차단되는지 확인. 실패 시 InputBinding 라우팅 우선순위 재설계 — 라이브 단축키 누락 = 운영 마비 직결.
   - **PoC-B (필수)**: PowerPoint COM 객체 수명주기 래퍼(`OfficePptSession`) — `Task.Run` 백그라운드에서 COM 생성/사용/`Marshal.ReleaseComObject` 호출이 좀비 프로세스 없이 종료되는지 stress 테스트 (100회 순회). STA 스레드 어피니티 보장 패턴 확정 (§10.3 신규 리스크 참조).
2. **Sprint 1 (2주)**: `EasiControls` 베이스 컨트롤 라이브러리 1차 완성, `IThemeService`(다크/라이트), 아이콘 파이프라인. PoC-A·B 결과를 베이스 인프라에 흡수.
3. **Sprint 2 (2주)**: T0 `FrmMain`을 새 `WpfMainWindow`로 — 다만 기능은 도메인 헬퍼를 그대로 호출. 기존 `FrmMain`은 백업 유지.
4. **Sprint 3~4 (4주)**: 나머지 T0 (`FrmLyricsScreen`, `FrmInfoScreen`, `FrmShowAlert`) + T1 일부
5. **Sprint 5~8 (8주)**: T2 (옵션 분해 포함)
6. **Sprint 9~10 (4주)**: T3 + QA + 시니어 모드 + 한국어 리소스 검증

**총 약 5~6개월** (1인 풀타임 기준, 단계별 출시 가능). **Sprint 0의 PoC-A/B 중 하나라도 실패하면 Sprint 1 진입 전에 아키텍처 재논의** — 기술적 불확실성을 초기에 해소하지 않고 진행하면 후반에 회귀.

### 8.4 데이터 흐름·테스트 보존
- 기존 통합 테스트(`SQLiteTestConsole/`) 그대로
- 신규 ViewModel은 단위 테스트 추가 (xUnit + Moq)
- `Marshal.ReleaseComObject` / `using` 패턴 보존 (CLAUDE.md §9, 좀비 프로세스 재발 방지)

---

## 9. 거버넌스·품질 게이트

### 9.1 Pre-Delivery 체크리스트 (모든 PR)
(ui-ux-pro-max 스킬 체크리스트 + 본 프로젝트 추가)

**시각 품질**
- [ ] 이모지를 아이콘으로 쓰지 않음 — Fluent UI Icons SVG 사용
- [ ] 동일 아이콘 셋 일관 사용
- [ ] hover/pressed 상태에서 레이아웃 시프트 없음
- [ ] 색 토큰 직접 참조 (`{DynamicResource Color.Accent.Primary}`) — 매직 색 금지

**상호작용**
- [ ] 모든 클릭 가능 요소에 `Cursor="Hand"`
- [ ] hover 시 시각 피드백 (색·외곽선·그림자)
- [ ] 트랜지션 150~300ms
- [ ] 포커스 링 키보드로 보임

**테마**
- [ ] 라이트 모드 대비 4.5:1 이상
- [ ] 다크 모드 대비 4.5:1 이상
- [ ] 보더 양쪽 모드 모두 보임
- [ ] 양쪽 모드 빌드 시 자동 스크린샷 회귀 (Sprint 4부터)

**접근성**
- [ ] 모든 이미지/아이콘 `AutomationProperties.Name`
- [ ] 입력 필드 `Label` 연결
- [ ] 색만으로 의미 전달 없음
- [ ] Windows 애니메이션 끔 설정 존중
- [ ] 키보드 단독 운영으로 모든 라이브 액션 가능

**라이브 안전**
- [ ] 위험 액션은 `SafetyConfirm` 인라인 또는 Undo 5초 토스트
- [ ] 라이브 상태 시각 인디케이터 항상 표시
- [ ] HookManager 단축키와 충돌 없음

### 9.2 자동화 (린트·분석기)
- Roslyn 분석기 `EasiDS001` — `Color.FromArgb`/`new Font(...)` 직접 사용 시 경고 (목표: 에러)
- StyleCop 규칙: `AutomationProperties.Name` 누락 컨트롤 경고
- PR CI에 위 체크리스트 일부 자동 검증

### 9.3 디자인 토큰 단일 소스
`Easislides.Wpf/Theme/` 디렉터리:
```
Theme/
  Tokens/
    Colors.Light.xaml
    Colors.Dark.xaml
    Typography.xaml
    Spacing.xaml
    Motion.xaml
    Radius.xaml
  EasiDS.xaml       ← 통합 ResourceDictionary
  IThemeService.cs  ← 런타임 전환
```

### 9.4 마이그레이션 가드 (병행 기간)
구버전 WinForms와 신버전 WPF가 한 솔루션에 공존하는 동안:
- 빌드 산출물 분리: `Easislides.exe` (legacy) / `EasislidesNext.exe` (신규)
- 사용자 토글: `--legacy-ui` CLI 인자로 기존 UI 호출 가능 (롤백 안전망)
- 공통 도메인 어셈블리 `Easislides.Core.dll` 추출 → 양쪽 참조

---

## 10. 로드맵 · 일정 · 리스크

### 10.1 Sprint 일정 (2주 단위)

| Sprint | 기간 | 산출물 | Maturity |
|---|---|---|---|
| 0 | 1주 | Wpf 프로젝트, 토큰, EsButton | 1.5 |
| 1 | 2주 | EasiControls 베이스 + ThemeService + 아이콘 파이프라인 | 2 |
| 2 | 2주 | WpfMainWindow (FrmMain 신규) | 2.3 |
| 3 | 2주 | FrmLyricsScreen·FrmInfoScreen·FrmShowAlert 신규 | 2.6 |
| 4 | 2주 | FrmEditItem·FrmEditBibleItem·FrmFind 신규 + 다크 회귀 자동화 | 3 |
| 5 | 2주 | FrmLaunchShow·FrmBackground·FrmEditNotes | 3 |
| 6 | 2주 | FrmOptions 분해 (Settings 페이지 패턴) | 3.2 |
| 7 | 2주 | Import/Export/Generate 5폼 | 3.5 |
| 8 | 2주 | 미디어/Copy/Move/Manage 폼 | 3.7 |
| 9 | 2주 | 유틸 폼 일괄 + 시니어 모드 + a11y 감사 | 4 |
| 10 | 2주 | 통합 QA + 한국어 리소스 + 베타 출시 | 4 |

**총 21주 (~5개월)**, 1인 풀타임 기준. 2인 시 ~3개월 가능.

### 10.2 마일스톤
- **M1 (Sprint 2 end)**: 신규 메인 폼으로 일반 송출 가능 — 내부 베타
- **M2 (Sprint 5 end)**: 모든 T0/T1 신규 — 운영자 베타
- **M3 (Sprint 8 end)**: 모든 폼 신규 — 일반 베타
- **M4 (Sprint 10 end)**: v3.0 정식 출시

### 10.3 리스크 & 완화

| 리스크 | 영향 | 확률 | 완화 |
|---|---|---|---|
| HookManager·WPF 키 이벤트 충돌로 단축키 누락 | 라이브 운영 마비 | 중 | **Sprint 0 PoC-A에서 선행 검증** (§8.3), `PreviewKeyDown` 우선순위 테스트, 실패 시 Sprint 1 진입 차단 |
| Office Interop이 WPF UI 스레드 멈춤 | 라이브 프리즈 | 고 | `Task.Run` + `IProgress<T>` 패턴, `BuildScreenPreDumps` 비동기화 |
| **STA 스레드 모델 위반(COM 객체 크로스-스레드 호출)** | 좀비 프로세스·간헐 크래시 | **고** | **Sprint 0 PoC-B에서 선행 검증**. COM 객체는 생성 STA 스레드에서만 호출/소멸. `Task.Run` 결과를 `Dispatcher.Invoke`로 UI에 넘기는 패턴 엄격 적용. `OfficePptSession` 래퍼가 자체 STA 워커 스레드 보유(`Thread.SetApartmentState(ApartmentState.STA)`) + 100회 stress 테스트로 좀비 0 검증. |
| 병행 기간 중 COM 객체 해제 지연 | 좀비 프로세스 재발 | 중 | `Marshal.ReleaseComObject` / `using` 패턴 + `OfficePptSession` IDisposable + WeakReference 감시 + 종료 시 잔여 COM 카운트 로깅 (CLAUDE.md §9 보강) |
| Mica/Acrylic이 무대 환경에서 산만 | 사용성 저하 | 저 | **Q3 확정 — 다크 무대는 Solid 강제**, 라이트만 Mica 허용 |
| 폼 40개 일괄 마이그레이션 도중 회귀 | 라이브 사고 | 고 | 점진 배포 (§8.3 Strangler), `--legacy-ui` 안전망 |
| 한국어 폰트 변경으로 폼 잘림 | 가독성 | 저 | **Q4 확정 — Pretendard Variable 번들**로 시스템 폰트 의존 제거, 모든 폼 한·영 자동 스크린샷 회귀 |
| 시니어 모드 토글이 토큰 시스템 깨뜨림 | 일관성 | 저 | 시니어 모드는 토큰 스케일 곱하기 1.2/1.4 단순 적용 (별도 테마 X) |
| WPF UI 라이브러리 유지보수 중단 | 장기 유지 | 중 | EasiDS 토큰은 라이브러리 비의존, 컨트롤만 일부 활용. 최악의 경우 fork 가능 (MIT) |
| 첫 실행 시 사용자 자산 마이그레이션 실패 | 데이터 유실 | 중 | §10.5 마이그레이션 도구 — 복사 전 백업 zip 생성, 실패 시 롤백, 사용자에게 명시 로그 |

### 10.4 일정 가속 옵션 (선택)
- 디자인 시스템 컨설팅 1주 → 토큰 합의 가속
- (참고: **아이콘은 외주 없음** — Microsoft Fluent UI System Icons 라이브러리에서 직접 추출. 추출·변환·사용 가이드는 [docs/ui/icon-pipeline.md](ui/icon-pipeline.md), 자산 1:1 매핑은 [docs/ui/icon-migration-map.md](ui/icon-migration-map.md))

### 10.5 데이터 마이그레이션 & Legacy 유지보수 (Q7·Q8 확정)

#### 10.5.1 자동 마이그레이션 도구 (`AssetMigrator`)
v3.0 첫 구동 시 자동 실행:

1. **탐지**: `%AppData%\Easislides\` (또는 사용자 설정 `WorkingFolder`)를 스캔 — 커스텀 썸네일/배경 이미지/사용자 폰트 등
2. **백업**: 마이그레이션 직전 원본을 `%AppData%\EasislidesNext\Backup\YYYYMMDD-HHmmss\` zip으로 압축 (실패 롤백 안전망)
3. **변환·복사**: 신규 규격 폴더로 이동
   - `UserAssets/Thumbs/` — 썸네일 (해상도 정규화: 긴 변 1920px 이상이면 리사이즈)
   - `UserAssets/Backgrounds/` — 배경 (포맷 검증, 손상 파일 격리)
   - `UserAssets/CustomFonts/` — 사용자 폰트
4. **인덱스 재생성**: 신규 SQLite 스키마에 자산 경로 등록
5. **결과 리포트**: 사용자에게 "X개 이동, Y개 변환, Z개 실패(목록)" 명시 다이얼로그 + 백업 zip 경로 안내
6. **실패 처리**: 한 자산이라도 실패하면 전체 롤백 옵션 제공 — 데이터 절대 유실 금지

DB 마이그레이션은 별도 `DatabaseMigrator` (스키마 버전 비교 후 ALTER, 백업 동시 수행).

#### 10.5.2 Legacy v2.6.4 유지보수 정책
v3.0 정식 출시일(M4)을 D-Day로 하여:

| 기간 | 지원 범위 | 주체 |
|---|---|---|
| **D-Day ~ +6개월** | 크리티컬 버그(데이터 유실·라이브 마비·보안) **만** 패치 | 동일 메인테이너 |
| **D-Day +6개월 이후** | EOL(End of Life) 선언, 코드만 보존 | — |

- 신기능 추가는 v3.0 전용 — v2.6.4에 backport 금지
- v2.6.4의 GitHub 이슈는 라벨 `legacy-maintenance`로 분류, 비크리티컬은 즉시 close + v3.0 마이그레이션 안내
- D-Day +3개월 시점에 사용 잔존율 모니터링 (자동 업데이트 통계로) → 50% 이상 잔존 시 EOL 연장 재논의
- `--legacy-ui` CLI 안전망은 M3까지 유지(ADR-007), M4 출시와 함께 제거 (legacy WinForms 빌드 자체는 별도 .exe로 D-Day +6개월까지 다운로드 가능)

---

## 11. 부록

### 11.A 현재 상태 감사 보고서 (ux-ui-design-reviewer 에이전트 산출)
→ §2 본문에 핵심 인용, 전체 원본은 PR 첨부 (코드 라인 참조 보존)

### 11.B 아이콘 자산 매핑 (현행 → Fluent UI System Icons)

| 현행 raster | 신규 Fluent Icon | 비고 |
|---|---|---|
| `BlackScreen.png` / `-Pressed.png` / `-Red.png` | `ic_fluent_eye_off_24_regular` + 색 토큰 | 3상태 → 1자산 |
| `BlueScreen-*` | `ic_fluent_pause_24_regular` (또는 Stop) | |
| `Bible.png` / `Bible - Hightlight.png` | `ic_fluent_book_24_regular` | 오타 자산 폐기 |
| `Media.png` / `Media-highlight.png` | `ic_fluent_play_circle_24_regular` | |
| `notebook.png` / `notebook-highlight.png` | `ic_fluent_notebook_24_regular` | |
| `word.png` / `word-highlight.png` | `ic_fluent_document_24_regular` | |
| `PPImg.png` / `PPImg - Highlight.png` | `ic_fluent_slide_layout_24_regular` | |
| `camcorder.png` / `camcorder-Red.png` | `ic_fluent_video_24_regular` | |
| `btnToOutput.png` / `btnToOutputMove.png` | `ic_fluent_arrow_right_24_regular` + variant | |
| `Add.png` | `ic_fluent_add_24_regular` | |
| `DeleteFile.png` / `DeleteList.png` | `ic_fluent_delete_24_regular` | |
| `Clear.png` | `ic_fluent_eraser_24_regular` | |
| `Send.png` | `ic_fluent_send_24_regular` | |
| `Help.png` | `ic_fluent_question_circle_24_regular` | |
| `Option.png` / `options.png` | `ic_fluent_settings_24_regular` | |
| `keyboard.png` | `ic_fluent_keyboard_24_regular` | |
| `LiveCam.png` / `btnLive.png` | `ic_fluent_video_clip_24_regular` | |
| `Tick.png` | `ic_fluent_checkmark_24_regular` | |
| `folder.png` / `folderOpen.png` | `ic_fluent_folder_24_regular` + `_open_` | |
| `dualscreens.png` / `singlescreen.png` | `ic_fluent_dual_screen_24_regular` / `ic_fluent_screen_24_regular` | |
| `주일예배 썸네일.png` / `수요예배 썸네일.png` | 사용자 콘텐츠로 분리 (`%AppData%/EasiSlides/Thumbs/`) | 자산 아님 |

**전체 매핑 표 + 추출/변환 가이드는 별도 문서로 분리됨**:
- [docs/ui/icon-pipeline.md](ui/icon-pipeline.md) — Microsoft Fluent UI System Icons에서 직접 추출, SVG→XAML 변환, WPF UI `SymbolIcon` 사용법
- [docs/ui/icon-migration-map.md](ui/icon-migration-map.md) — 60+ 자산 전체 1:1 매핑

> **외주 불가**: 아이콘 자산은 별도 디자이너에게 발주하지 않고, MIT 라이선스의 공식 Fluent UI System Icons 라이브러리에서 직접 추출하여 사용 (ADR-0002).

### 11.C 결정 사항 ADR 후보
다음 결정은 본 계획서 승인 후 개별 ADR로 분리하여 `docs/adr/`에 기록:

1. ADR-001: WPF + WPF UI 라이브러리 선정 사유 (vs WinUI 3 vs Avalonia)
2. ADR-002: Fluent UI System Icons vs Lucide
3. ADR-003: 한글 폰트 Pretendard Variable 번들링 (라이선스 OFL 1.1)
4. ADR-004: HookManager 보존 vs WPF 표준 InputBinding 통합
5. ADR-005: `FrmOptions` 단일 모달 분해 (Settings 페이지) 패턴
6. ADR-006: 시니어 모드 토큰 스케일 vs 별도 테마
7. ADR-007: `--legacy-ui` 안전망 유지 기간 (제안: M3까지)

### 11.D 결정 완료 사항 (v1.1 리뷰 합의)

> v1.0의 미해결 질문 Q1~Q8 모두 확정. 상세 답변과 근거는 §0.5 표 참조.

- [x] **Q1**: 컨트롤 라이브러리 → **WPF UI (lepoco/wpfui)** 베이스 + EasiDS 토큰 오버레이
- [x] **Q2**: 시니어 모드 디폴트 → **OFF + 최초 실행 온보딩 선택**
- [x] **Q3**: Mica/Acrylic → **라이트 ON · 다크 OFF (Solid)**
- [x] **Q4**: 한국어 폰트 → **Pretendard Variable (OFL 1.1) 번들 필수**
- [x] **Q5**: 라이브 인디케이터 → **상단 LiveBar + 우측 모서리 Pulse 이중 배치**
- [x] **Q6**: Legacy raster 아이콘 → **100% 폐기, Fluent UI System Icons 전면 교체**
- [x] **Q7**: 사용자 자산 마이그레이션 → **첫 실행 자동 도구 (`AssetMigrator`, §10.5.1)**
- [x] **Q8**: Legacy 유지보수 → **D-Day +6개월, 크리티컬만 (§10.5.2)**

### 11.E 다음 단계 (v1.1 승인 후 즉시)

1. ~~본 계획서 합의 / Q1~Q8 답변~~ ✅ 완료 (v1.1)
2. **ADR-001~007 작성** (다음 마일스톤) — 결정의 배경·대안·결과를 영구 기록
3. **Sprint 0 킥오프** — `Easislides.Wpf` 프로젝트 생성 + `EasiDS` 토큰 ResourceDictionary + **PoC-A(HookManager), PoC-B(COM STA) 최우선 검증** (§8.3)
4. **아이콘 추출 (내부 작업, 외주 없음)** — [docs/ui/icon-pipeline.md](ui/icon-pipeline.md) 가이드 따라 Microsoft Fluent UI System Icons에서 60+ 자산 SVG 추출 + WPF 자원 등록. 매핑 표는 [docs/ui/icon-migration-map.md](ui/icon-migration-map.md). Sprint 1 Day 1~2에 일괄 작업.
5. 베타 테스터 그룹(2~3명 운영자) 모집 — 시니어 모드 온보딩 카피 검증 포함
6. `AssetMigrator` 명세서 + 백업/롤백 시퀀스 다이어그램 별도 작성 (§10.5.1)
