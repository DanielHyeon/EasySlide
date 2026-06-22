# ADR-0002: Fluent UI System Icons 아이콘 셋 채택

* **상태**: Accepted
* **결정일**: 2026-05-27
* **결정자**: 프로젝트 메인테이너 + UI/UX 현대화 리뷰
* **태그**: icons, vector, fluent2, assets
* **관련**: 계획서 §0.5 Q6, §4.5, §11.B · ADR-0001 (WPF UI 라이브러리 SymbolIcon 내장)

## 컨텍스트

현재 EasiSlides는 `Easislides/Resources/` + `EasislideImages/`에 60+ 개의 raster (PNG/BMP) 아이콘을 사용:
- 동일 의미 상태 3개를 별도 파일로 (`BlackScreen.png` / `BlackScreen-Pressed.png` / `BlackScreen-Red.png`)
- 호버 듀얼 (`Bible.png` / `Bible - Hightlight.png` ← 오타 자산)
- High-DPI 스케일 대응 없음, 벡터 자산 0개

Q6 결정: **레거시 raster 100% 폐기, 벡터 아이콘 셋으로 전면 교체.** 어떤 벡터 셋을 채택할 것인가?

## 고려한 대안

### A. Fluent UI System Icons (Microsoft 공식) — **채택**

[`microsoft/fluentui-system-icons`](https://github.com/microsoft/fluentui-system-icons) (MIT).

| 장점 | 단점 |
|---|---|
| **Windows 11 일관성** — OS 셸·Office·Edge가 같은 셋 사용 → 한국 사용자 시각 인지 부담 0 | 디자인 변종 제한 (Regular/Filled 2가지 weight) |
| 3,000+ 아이콘, 5개 사이즈(12/16/20/24/28/32/48) 별 최적화 | 추상적/창의적 메타포는 약함 — 명료한 비즈니스 메타포 위주 |
| MIT 라이선스 — 상업 배포·수정 자유 | — |
| **WPF UI 라이브러리에 `SymbolIcon`으로 내장** (ADR-0001) → 코드 한 줄로 사용 가능 (`<Ui:SymbolIcon Symbol="Book24" />`) | — |
| SVG·XAML·PNG·Font 4가지 포맷 동시 제공 | — |
| Regular/Filled 페어 → hover/active 상태 표현에 활용 가능 | — |

### B. Lucide

[Lucide Icons](https://lucide.dev/) (ISC) — Feather Icons fork.

| 장점 | 단점 |
|---|---|
| 모던/미니멀 스타일, 1,400+ 아이콘 | **Windows 멘탈 모델과 미세 불일치** — 사용자가 Windows 셸에서 같은 의미의 다른 모양에 익숙 |
| 일관된 stroke width | WPF UI 라이브러리 내장 아님 → SVG → XAML 빌드 변환 파이프라인 필수 |
| 웹 개발자 친숙도 높음 | — |

### C. Heroicons

| 장점 | 단점 |
|---|---|
| Tailwind 팀 제작, 깔끔 | 웹 중심 — 데스크톱 워크플로 메타포(예: 다중 모니터, 슬라이드 쇼) 빈약 |
| 2가지 스타일 (Outline/Solid) | 아이콘 수량 300+ (부족할 수 있음) |
| MIT | — |

### D. 자체 디자인 (외주)

| 장점 | 단점 |
|---|---|
| 100% 브랜드 일관성 | 60+ 아이콘 디자인 비용·시간 ($3k-10k, 4-8주) |
| EasiSlides 도메인 특화 메타포 | 사용자 익숙도 0에서 시작 |
| — | 유지보수 비용 (신규 아이콘 필요 시마다 디자이너 의뢰) |

## 결정

**A. Fluent UI System Icons 채택.**

세부:
- **우선순위**: WPF UI 라이브러리의 `SymbolIcon` 사용. 라이브러리에 없거나 다른 weight 필요 시 SVG 다운로드 후 빌드 시 XAML 변환.
- **사이즈 표준**: 컨트롤별 고정 (계획서 §4.5 — 16/20/24/32)
- **상태 표현**: 단일 아이콘 + 색·투명도 토큰으로 hover/pressed/disabled. Regular(기본) ↔ Filled(active) 페어 활용.
- **레거시 자산**: `Easislides/Resources/` + `EasislideImages/` 모든 raster 100% 폐기. 사용자 콘텐츠(예배 썸네일 등)는 자산 아니므로 분리 보존(`%AppData%\EasislidesNext\UserAssets\`, ADR 외 §10.5.1).
- **메타포 부재 처리**: Fluent 셋에 정확한 메타포가 없으면 가장 유사한 아이콘 차용. 절대 raster 잔존 금지.
- **매핑 표**: 계획서 §11.B에 초안, 전체 60+개 1:1 매핑은 별도 `docs/ui/icon-migration-map.md`로 확정.

## 결과

### 긍정적
- **사용자 인지 부담 최소** — Windows 11 셸과 동일 시각 언어
- **개발 비용 0** — WPF UI `SymbolIcon` 또는 공식 SVG/XAML 다운로드
- **자동 업데이트** — Microsoft가 셋 확장/리뉴얼 시 NuGet 업데이트로 흡수
- **시스템 색 동기화** — 액센트 색을 시스템 설정에서 가져오면 아이콘 색도 자동 매칭 (옵션)

### 부정적
- 도메인 특화 메타포(예: "워십 리스트", "예배 순서") 정확한 1:1 없음 → 조합·차용 필요
- Regular/Filled 2가지 weight만 — 시니어 모드용 굵은 두께 별도 없음 (사이즈 확대로 대응)

### 중립 / 리스크
- 매핑 단계에서 일부 아이콘이 사용자에게 이전과 다르게 인식될 수 있음 → 베타 기간(M2~M4) 사용자 피드백 수집, 필요 시 차선 매핑으로 변경 (이 ADR 자체는 변경 없음, 매핑 표만 갱신).

## 참조

- 계획서 §0.5 Q6, §4.5, §11.B
- [Fluent UI System Icons GitHub](https://github.com/microsoft/fluentui-system-icons)
- [WPF UI Icons 가이드](https://wpfui.lepo.co/documentation/icons.html)
- ADR-0001 (WPF UI 라이브러리 — SymbolIcon 내장)
