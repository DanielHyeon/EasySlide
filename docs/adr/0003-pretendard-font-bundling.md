# ADR-0003: Pretendard Variable 한국어 폰트 번들링

* **상태**: Accepted
* **결정일**: 2026-05-27
* **결정자**: 프로젝트 메인테이너 + UI/UX 현대화 리뷰
* **태그**: typography, korean, font, licensing, ofl
* **관련**: 계획서 §0.5 Q4, §4.2 · ADR-0001 (WPF), ADR-0006 (시니어 모드 스케일)

## 컨텍스트

EasiSlides 사용자 90%+ 가 한국 교회 운영자. 현재 폰트 사용 현황:
- `gfColorsFonts.cs:148` 폴백: `Microsoft Sans Serif` (1992년 도입, 한글 미지원 — 시스템 한글 폰트로 자동 대체)
- `FrmMain.Designer.cs` 등에 `Tahoma` 8.25pt 하드코딩
- 시스템 한글: 사용자 PC의 `Malgun Gothic` (Windows 8+) 또는 `Gulim` (구버전)

문제:
- **Malgun Gothic 한계**: weight가 Regular/Bold 2가지뿐 — Fluent 2 타입 스케일(weight 400/500/600/700)의 풍부함 활용 불가
- **WPF ClearType 렌더링 + Malgun Gothic 조합**: UI 스케일링 시 한글 글자 흐려짐·뭉개짐
- **PC 환경 차이**: Windows 7/8 일부 PC에 Malgun Gothic 미설치 — 폴백이 `Gulim`/`Batang` 같은 비트맵 폰트로 떨어져 가독성 급락
- **레이아웃 일관성**: 같은 폼이 PC마다 폰트가 다르게 잡혀 텍스트 잘림·줄바꿈 차이

Q4 결정: **앱이 폰트를 번들로 제공해 시스템 의존 제거.** 어느 폰트를 어떻게 번들할 것인가?

## 고려한 대안

### A. Pretendard Variable 번들 (OFL 1.1) — **채택**

[Pretendard](https://github.com/orioncactus/pretendard) by orioncactus. OFL 1.1 라이선스.

| 장점 | 단점 |
|---|---|
| **9개 weight (100~900) Variable Font** — Fluent 2 타입 스케일 완전 활용 | 앱 크기 +~1.5MB |
| **한·영 통합 디자인** — 영문 폴백 별도 불필요 | OFL 1.1 라이선스 파일 동봉 + 폰트 이름 변경 금지 의무 |
| 한국 디자인 커뮤니티 표준 (토스/카카오/네이버 등 채택) | — |
| WPF `pack://` URI로 임베드 가능 | — |
| 최신 한글 자형(KS X 1001 + Adobe-KR 1) 완비 | — |
| 활발한 유지보수 (2024~ 정기 업데이트) | — |

### B. 시스템 Malgun Gothic 의존

| 장점 | 단점 |
|---|---|
| 앱 크기 증가 0 | weight 부족 (Regular/Bold) |
| 번들 라이선스 고려 불필요 | UI 스케일링 시 흐림 |
| — | Windows 7/8 PC 일부 미설치 |
| — | 사용자 PC마다 레이아웃 차이 가능성 |

### C. Noto Sans KR 번들

[Noto Sans KR](https://fonts.google.com/noto/specimen/Noto+Sans+KR) by Google. OFL 1.1.

| 장점 | 단점 |
|---|---|
| Google 메인테너 — 글로벌 표준 | **파일 크기 큼** — 한글 풀셋 + Variable 시 ~6MB |
| 한·중·일 통합 패밀리 — 다국어 확장 시 유리 | 한·영 분리(영문은 Roboto와 결합 필요) → EasiDS 폰트 폴백 체인 복잡화 |
| 충분한 weight | 한국어 디자인 특성보다 글로벌 통일성 우선 → 한글 자형이 Pretendard보다 덜 친숙 |

### D. 사용자 폰트 선택 (현재 방식 유지 + 옵션 노출)

| 장점 | 단점 |
|---|---|
| 사용자 자유도 | UI 일관성 보장 불가 |
| 앱 크기 증가 0 | 폰트별 메트릭 차이로 폼 깨짐 |
| — | 디자인 토큰 시스템과 정면 충돌 |

## 결정

**A. Pretendard Variable 번들 (OFL 1.1).**

세부:
- **파일**: `Easislides.Wpf/Assets/Fonts/PretendardVariable.ttf` (Variable Font 단일 파일, ~1.5MB)
- **WPF 등록**: `App.xaml`
  ```xml
  <FontFamily x:Key="Font.Primary">pack://application:,,,/Assets/Fonts/#Pretendard Variable</FontFamily>
  ```
- **폴백 체인** (`Type.Body` 등 모든 토큰에 적용):
  `Pretendard Variable, Segoe UI Variable, Segoe UI, Malgun Gothic, sans-serif`
- **라이선스 동봉**: `Easislides.Wpf/Assets/Fonts/PretendardVariable-OFL-1.1-LICENSE.txt` (원본 LICENSE 그대로 복사)
- **금지**: `Malgun Gothic` 직접 지정 금지(폴백으로만), `Microsoft Sans Serif`/`Tahoma` 사용 금지 (마이그레이션 시 모두 제거 — 계획서 §4.2)
- **모노스페이스**: `Cascadia Code` 시스템 폰트 사용 (Windows 11 기본). 미설치 PC를 위해 폴백 `Consolas` 지정.

## 결과

### 긍정적
- **PC 무관 동일 레이아웃** — 모든 사용자 PC에서 100% 동일 픽셀 결과
- **Fluent 2 타입 스케일 활용** — Display(600)/Title(600)/Body(400)/Caption(400) 등 weight 분기 풍부
- **한글 가독성 향상** — Malgun Gothic 대비 한글 자형 명료·균형
- **시니어 모드와 시너지** — Variable Font의 무단계 스케일링이 토큰 스케일 함수(ADR-0006)와 매끄럽게 작동
- **상업 자유** — OFL 1.1, 라이선스 파일만 동봉

### 부정적
- 앱 설치 크기 +1.5MB (현재 EXE ~30MB 기준 5% 증가, 무시 가능)
- 폰트 이름 변경·재배포 시 제약 (OFL 1.1 — 단, 단순 번들은 문제 없음)

### 중립 / 리스크
- Variable Font를 일부 구버전 WPF 렌더링 파이프라인이 완전 지원하지 않을 가능성 → **Sprint 1에서 .NET 10 + Pretendard Variable 렌더링 PoC** (모든 weight가 화면에 올바르게 출력되는지)
- 사용자 PC에 같은 이름 폰트가 미리 설치돼 있으면 충돌 가능성 → `pack://` URI 임베드 폰트는 시스템 폰트와 별도 네임스페이스로 동작하나, 일부 텍스트 박스(예: PowerPoint Interop 결과)에서는 시스템 폰트가 우선 — 본 폰트는 EasiSlides UI 내부에만 적용, 송출 슬라이드 내 폰트는 별도 처리.

## 참조

- 계획서 §0.5 Q4, §4.2
- [Pretendard GitHub](https://github.com/orioncactus/pretendard)
- [SIL Open Font License 1.1](https://scripts.sil.org/OFL)
- [WPF Application Resources — 폰트 임베드](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/packaging-fonts-with-applications)
- ADR-0001 (WPF), ADR-0006 (시니어 모드 토큰 스케일)
