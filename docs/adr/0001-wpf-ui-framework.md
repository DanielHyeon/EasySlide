# ADR-0001: WPF + WPF UI 프레임워크 채택

* **상태**: Accepted
* **결정일**: 2026-05-27
* **결정자**: 프로젝트 메인테이너 + UI/UX 현대화 리뷰
* **태그**: framework, ui-library, fluent2, migration
* **관련**: 계획서 §0.5 Q1, §8.1 · ADR-0002 (아이콘), ADR-0003 (폰트), ADR-0006 (시니어)

## 컨텍스트

EasiSlides v2.6.4는 .NET 10 WinForms 기반(40+ `Frm*.cs` 폼, FrmMain 8,800줄)이며, UI 시각/상호작용 시스템은 XP/Vista 시대 패턴에 머물러 있다. 계획서 §0에서 다음이 결정됨:

- **전체 WPF/WinUI 마이그레이션** (WinForms 단계적 폐기)
- **Fluent 2 (Windows 11 네이티브)** 비주얼 톤
- **다크모드+라이트모드+큰 폰트+키보드** 우선 접근성

이 결정을 실현할 구체 프레임워크/라이브러리 선택이 필요. 라이브 송출 도구라는 도메인 특성상 **신뢰성·기존 도메인 자산 보존·라이선스**가 핵심 평가축.

## 고려한 대안

### A. WPF + WPF UI (lepoco/wpfui) — **채택**

WPF 위에 [WPF UI](https://github.com/lepoco/wpfui) (MIT) 라이브러리를 컨트롤 베이스로 활용하고, 자체 디자인 토큰(EasiDS)으로 색·간격·타이포 오버라이딩.

| 장점 | 단점 |
|---|---|
| .NET 10 정식 지원, 성숙한 XAML 스택 | WPF 자체는 신규 기능 투자 감소 추세 |
| WinForms 코드와 `ElementHost`/`WindowsFormsHost` 양방향 호환 → Strangler Fig 점진 이식 가능 | DPI/모니터 변경 처리는 수동 케어 필요 |
| `OfficeLib`(NetOffice) / `DirectShow.NET` Interop 검증된 호스트 환경 | 모바일/크로스플랫폼 불가 (이 프로젝트는 무관) |
| WPF UI 라이브러리: Fluent 2 컨트롤 + Mica/Acrylic Backdrop + 다크/라이트 ThemeService | Fluent 2 디자인 토큰은 라이브러리 디폴트 — EasiDS 오버라이딩 필요 |
| MIT 라이선스, 활발한 유지보수 (스타 12k+, 월별 릴리스) | — |
| 한국 개발자 커뮤니티 친숙도 높음 | — |

### B. WinUI 3 + Windows App SDK

| 장점 | 단점 |
|---|---|
| Microsoft 차세대 데스크톱 UI 스택, Fluent 2 네이티브 | Win32 데스크톱 시나리오 미성숙 (2024년 기준에서도 갭) |
| Mica/Acrylic 등 시스템 효과 1급 지원 | MSIX 패키징 강제 또는 unpackaged 모드 제약 |
| 최신 XAML 컴포지션 API | NetOffice/DirectShow Interop 호스팅 시 시나리오별 추가 검증 필요 |
| — | 3rd party 컨트롤 생태계 빈약, 한국 사용자 PC에서 Windows App SDK 런타임 사전 설치 필요 |
| — | 라이브 운영 환경(다양한 Windows 빌드)에서 호환성 리스크 |

### C. Avalonia 11

| 장점 | 단점 |
|---|---|
| 크로스 플랫폼 (Windows/Linux/macOS) | 본 프로젝트는 Windows 전용 — 가치 없음 |
| Fluent 테마 내장 | Windows "네이티브감" 약함 (커스텀 윈도우 chrome) |
| 활발한 OSS | NetOffice/DirectShow Win32 COM Interop이 추가 마샬링 레이어를 거쳐야 함 (리스크) |
| — | 한국 데스크톱 개발 풀에서 친숙도 낮음 |

### D. WinForms 유지 + 커스텀 컨트롤·테마 라이브러리 (Krypton/MaterialSkin.2 등)

| 장점 | 단점 |
|---|---|
| 마이그레이션 비용 0 | Fluent 2 / 다크 / 벡터 아이콘 등 핵심 목표 달성 불가 |
| 기존 코드 그대로 | OwnerDraw·커스텀 페인팅 누적으로 유지보수 비용 폭증 |
| — | 계획서 §0 결정과 정면 충돌 |

## 결정

**A. WPF + WPF UI 채택.**

추가 결정 세부:
- **타겟 프레임워크**: `net10.0-windows`, `UseWPF=true`, `UseWindowsForms=false` (신규 프로젝트 기준)
- **베이스 컨트롤**: WPF UI 라이브러리의 `Ui:FluentWindow`, `Ui:Button`, `Ui:NavigationView` 등 활용
- **비주얼 토큰**: EasiDS ResourceDictionary가 단일 진실 — WPF UI 디폴트 색·타이포를 오버라이딩
- **MVVM**: `CommunityToolkit.Mvvm` 9.x
- **DI**: `Microsoft.Extensions.DependencyInjection` 9.x
- **WinForms 공존 기간**: Strangler Fig 패턴으로 M3(Sprint 8 end)까지 양쪽 동시 빌드 (계획서 §8.3·§9.4)

## 결과

### 긍정적
- **신뢰성**: 성숙한 WPF 위에서 동일 호스트 프로세스에서 Office/DirectShow Interop이 검증된 방식대로 동작 → 라이브 사고 리스크 최소
- **점진 이식 가능**: ElementHost로 WPF UserControl을 WinForms에 임베드, 또는 그 반대도 가능 → 한 폼씩 교체 가능
- **Fluent 2 즉시 사용**: WPF UI의 ThemeService로 다크/라이트 런타임 전환 — Q3 결정(다크는 Solid, 라이트는 Mica)을 라이브러리 API로 직접 매핑 가능
- **라이선스 안전**: MIT (LICENSE.txt만 함께 배포)
- **유지보수 위험 분산**: EasiDS 토큰은 라이브러리 비의존 → 최악 시 WPF UI fork (MIT 허용) 또는 자체 컨트롤로 점진 대체 가능

### 부정적
- WPF는 Microsoft의 신규 기능 투자가 적음 — 5~10년 후 시점에 WinUI/MAUI로의 재마이그레이션 가능성. 단, 현재 비즈니스 가치는 충분.
- 일부 Fluent 2 최신 효과(예: 시스템 색 동기화)는 WinUI 3보다 늦게/제한적으로 들어옴. EasiDS 토큰으로 대응.

### 중립 / 리스크
- WPF UI 라이브러리의 .NET 10 지원 — 작성 시점 기준 .NET 8/9까지 공식 지원. **Sprint 0에서 .NET 10 호환성 PoC 필수** (실패 시 .NET 9 타겟 폴백 또는 fork).
- COM Interop의 STA 스레드 모델은 WPF에서도 동일하게 적용 — `OfficePptSession` 래퍼가 자체 STA 워커 스레드 보유해야 함 (ADR-0004 + 계획서 §10.3 STA 리스크 참조).

## 참조

- 계획서 §0.5 Q1, §8.1, §8.3, §10.3
- [WPF UI GitHub](https://github.com/lepoco/wpfui)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- WinUI 3 vs WPF 비교: [Microsoft 공식 가이드](https://learn.microsoft.com/windows/apps/desktop/choose-your-platform)
