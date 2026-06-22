# ADR-0006: 시니어 모드 토큰 스케일 함수 (vs 별도 테마)

* **상태**: Accepted
* **결정일**: 2026-05-27
* **결정자**: 프로젝트 메인테이너 + UI/UX 현대화 리뷰
* **태그**: accessibility, senior, scaling, tokens
* **관련**: 계획서 §0.5 Q2, §4.2, §7.4 · ADR-0001 (WPF), ADR-0003 (Pretendard Variable Font)

## 컨텍스트

Q2 결정: 시니어 모드(큰 폰트/큰 타깃)는 **디폴트 OFF + 최초 실행 온보딩에서 3단계 선택**:
- **표준 (Standard)**: 14px 본문, 44×44px 타깃 (기본)
- **큰 글씨 (Large)**: 16px 본문, 48×48px 타깃 — 본문 +14%, 타깃 +9%
- **시니어 (Senior)**: 20px 본문, 52×52px 타깃, 버튼 패딩 +50% — 본문 +43%, 타깃 +18%

EasiDS 디자인 토큰 시스템(§4)은 색·타이포·간격·반경·모션을 ResourceDictionary로 정의. 이 시스템에 "사용자 선택 크기"라는 동적 축을 어떻게 추가할 것인가?

## 고려한 대안

### A. 토큰 스케일 함수 (런타임 곱하기) — **채택**

EasiDS 기본 토큰 + `ScaleFactor` 멀티플라이어. 사용자 선택에 따라 1.0/1.2/1.4 곱.

```csharp
// IThemeService
public double ScaleFactor { get; private set; } = 1.0;

public void SetSize(InterfaceSize size) {
    ScaleFactor = size switch {
        InterfaceSize.Standard => 1.0,
        InterfaceSize.Large => 1.2,
        InterfaceSize.Senior => 1.4,
        _ => 1.0
    };
    UpdateScaledResources();
}

private void UpdateScaledResources() {
    // 런타임에 ResourceDictionary 갱신
    Application.Current.Resources["Type.Body.FontSize"] = 14.0 * ScaleFactor;
    Application.Current.Resources["Spacing.TargetMin"] = 44.0 * ScaleFactor;
    // ...
}
```

| 장점 | 단점 |
|---|---|
| **유지보수 비용 1×** — 기본 토큰만 관리, 스케일 자동 파생 | 일부 픽셀 단위 미세 조정 불가 (예: 1px 보더 = 1.4px 가능 vs 1px 유지) |
| **런타임 토글 즉시 반영** — 사용자가 옵션에서 변경 → 모든 UI 0.2s 안에 재렌더 | — |
| **Variable Font와 시너지** (ADR-0003) — Pretendard Variable은 무단계 weight/스케일 가능 | — |
| **테마 직교** — 시니어 모드는 색 테마(라이트/다크)와 독립 → 4가지 조합 (S/L/Sr × Light/Dark) 자연스럽게 지원 | — |
| 토큰 시스템 일관성 보존 | — |

### B. 별도 ResourceDictionary (Standard.xaml / Large.xaml / Senior.xaml)

| 장점 | 단점 |
|---|---|
| 픽셀 단위 정밀 제어 — 디자이너가 각 크기마다 미세 조정 가능 | **유지보수 비용 3×** — 한 토큰 변경 시 3 파일 모두 갱신 |
| | 색 테마와 곱하면 6 파일 (Standard.Light, Standard.Dark, Large.Light, ...) |
| | 일관성 검증 비용 — 3 파일 비례가 어긋났는지 자동 검출 필요 |

### C. WPF 시스템 DPI 스케일링 활용

WPF는 OS DPI 설정(125%/150%/175%)을 자동 반영. 사용자가 OS 설정 변경하도록 안내.

| 장점 | 단점 |
|---|---|
| 코드 변경 0 | **앱 외부 의존** — Windows 설정 메뉴 진입 부담 |
| | 모니터별 DPI 분리 — 같은 PC 다중 모니터에서 다르게 작동 (라이브 운영 부스에서 혼란) |
| | **EasiSlides 단독으로 시니어 모드 켜고 끄기 불가** — 다른 앱까지 영향 |

## 결정

**A. 토큰 스케일 함수.**

세부 — **스케일 적용 토큰 vs 비적용 토큰**:

| 카테고리 | 스케일 적용? | 사유 |
|---|---|---|
| 폰트 크기 (Type.*.FontSize) | ✅ | 본문 16→20px 등 핵심 |
| 간격 (Spacing.*) | ✅ | 패딩·마진 비례 확대 |
| 타깃 크기 (TargetMin, ButtonHeight) | ✅ | 44→52px 등 핵심 |
| 보더 두께 (Border.Thickness) | ❌ | 1px 보더 = 1px 유지 (스케일 시 흐림 방지) |
| 반경 (Radius.*) | ❌ | Fluent 2 표준 — 시니어도 동일 |
| 색 (Color.*) | ❌ | 색은 크기와 무관 |
| 모션 시간 (Motion.*) | ❌ | 시간은 크기와 무관, 단 시니어 모드에서 자동 +50% 천천히 옵션 고려 가능(별도 ADR) |
| 아이콘 사이즈 | ✅ | 16→24, 24→32 등 한 단계 위로 자동 매핑 |

**구현 — `IThemeService.SetSize(InterfaceSize)`**:
1. `ScaleFactor` 갱신
2. 스케일 적용 토큰을 `Application.Current.Resources`에서 갱신 (`Type.Body.FontSize` = 14.0 × factor)
3. `OnPropertyChanged("ScaleFactor")` 발생
4. WPF DataBinding이 모든 `{DynamicResource}` 참조 컨트롤 재렌더
5. 설정 저장 (`%AppData%\EasislidesNext\settings.json`)

**온보딩 (계획서 §7.4)**:
- 최초 실행 1회 표시, 큰 카드 3개 — "표준" / "큰 글씨" / "시니어"
- 각 카드에 미리보기 미니 UI 포함 (실제 토큰으로 렌더)
- 선택 후 즉시 적용, "FrmOptions → 일반"에서 언제든 변경 가능

## 결과

### 긍정적
- **유지보수 비용 1×** — 기본 토큰만 변경하면 3 크기 자동 파생
- **런타임 토글** — 사용자가 옵션에서 바꿔도 앱 재시작 불필요
- **테마 직교** — 라이트/다크와 독립
- **Variable Font 시너지** — Pretendard Variable이 무단계 스케일이므로 1.2/1.4 곱이 자연스럽게 동작
- **단위 테스트 용이** — `ScaleFactor`를 ViewModel에서 mock 가능

### 부정적
- 보더·반경 등 일부 토큰은 스케일 제외 → 디자이너 관점에서 "왜 마진은 커지는데 반경은 안 커지지?" 의문 가능. 본 ADR을 디자인 시스템 문서에 인용.
- 픽셀 단위 정밀 조정이 필요한 케이스가 발생하면 별도 토큰 분기 필요 (예: `Type.Caption.FontSize.Senior = 14` 명시) — 토큰 폭증 우려

### 중립 / 리스크
- **3가지 스케일 × 2가지 테마 = 6가지 조합** 자동 스크린샷 회귀 테스트 필수 (Sprint 4부터)
- 시니어 모드에서 일부 폼 레이아웃이 잘리거나 가로 스크롤 발생 가능 → 모든 폼이 시니어 모드(1.4×)에서 1024×768 해상도에 들어맞는지 QA 체크리스트 (계획서 §9.1 추가 항목)

## 참조

- 계획서 §0.5 Q2, §4.2, §7.4, §9.1
- [WPF DynamicResource](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/resources-overview)
- ADR-0001 (WPF UI), ADR-0003 (Pretendard Variable Font)
