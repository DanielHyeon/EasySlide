# 스크린샷 회귀 자동화 (계획서 §9.1 / 작업 C)

> 상태: **PoC + 기반 구축 완료**. CI 클라우드 실행 1회차 검증은 PR 머지 후 GitHub Actions에서 확인.

## 1. 목표

라이트/다크 등 시각 상태의 의도치 않은 변화(토큰 색·레이아웃 회귀)를 자동으로 잡는다.
WinForms→WPF 이식·컴포지트 추출(작업 A) 시 "보이는 결과가 그대로인지"의 안전망이 된다.

## 2. 핵심 설계 결정

| 결정 | 이유 |
|---|---|
| **헤드리스 렌더 = `RenderTargetBitmap`** | 창을 띄우지 않고 비주얼 트리를 비트맵으로 래스터화. 이미 `PreviewCanvasTests`에서 동작 확인됨. |
| **기준 비주얼 = 텍스트 없는 색 토큰 스와치** | 폰트 힌팅·안티에일리어싱은 환경마다 달라 픽셀 비교가 불안정. 텍스트를 배제하면 결정적이 되어 환경 간 안정적이고, 토큰 색이 바뀌면 즉시 회귀로 잡힌다. |
| **DPI 96 고정 + Pbgra32** | 환경 간 동일 픽셀 치수·채널 정렬 보장. |
| **허용오차 비교** (채널 ±2, 차이 픽셀 0%) | 미세 인코딩 잡음은 흡수, 실제 색·레이아웃 변경은 감지. |
| **승인 테스트(approval) 방식** | 기준 이미지가 없으면 `EASISLIDES_APPROVE_BASELINES=1` 일 때만 생성·실패. CI에서는 미설정이라 "조용한 생성 후 통과" 위장 차단. |
| **경로 분리** | 기준 이미지는 소스 트리(`Easislides.Wpf.Tests/Rendering/baselines/`, 커밋 대상), 진단 산출물 `*.actual.png` 는 테스트 출력 폴더(`bin/.../screenshot-actuals/`, gitignore). |

## 3. 구성 요소 (테스트 인프라)

- `Rendering/VisualRenderHarness.cs` — 요소 → `RenderTargetBitmap`/PNG (DPI 96 고정).
- `Rendering/ImageComparer.cs` — 두 PNG를 Pbgra32로 디코드해 채널 허용오차 + 차이 픽셀 비율 비교(`long` 승격으로 대형 캡처 확장 대비).
- `Rendering/ScreenshotBaseline.cs` — 승인 테스트 기준 저장/비교, 불일치 시 actual 기록.
- `Rendering/ScreenshotRegressionTests.cs` — 렌더 PoC + 회귀 감지 검증 + `color-tokens-light` 기준 비교(테마/사이즈를 Light·Standard로 고정해 앰비언트 상태 의존 제거).
- `Rendering/baselines/color-tokens-light.png` — 최초 기준 이미지(EasiDS Light 토큰 스와치).

## 4. 기준 이미지 추가·갱신 방법

```powershell
# 새 기준 비주얼을 추가하거나 의도적으로 갱신할 때(로컬, 승인 모드):
$env:EASISLIDES_APPROVE_BASELINES = "1"
dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~ScreenshotRegressionTests"
# → 누락된 기준 이미지가 생성되고 테스트는 1회 실패. 생성된 PNG를 눈으로 검토 후 커밋하고 다시 실행.
```

## 5. CI 연동

`.github/workflows/ci.yml` — `windows-latest`에서 .NET 9.0+10.0 SDK 설치 후 솔루션 빌드·테스트.
스크린샷 회귀 테스트가 CI 러너에서도 헤드리스 렌더로 통과하는지 이 잡이 함께 검증한다.
실패 시 `screenshot-actuals` 아티팩트를 업로드해 원격 진단을 돕는다.

> ⚠️ 1회차 주의: GitHub 러너에서 `RenderTargetBitmap` 헤드리스 렌더 가능 여부는 첫 Actions 실행으로 확정된다.
> 만약 러너 환경에서 렌더가 불안정하면, 데스크톱 세션 보장(예: 별도 러너) 또는 소프트웨어 렌더 강제를 검토한다.

## 6. 한계·후속

- 텍스트·아이콘 폰트 렌더는 결정성이 낮아 현재 기준에서 제외. 필요 시 큰 허용오차 + 영역 마스킹으로 별도 다룬다.
- 후속: 다크 테마 스와치, 주요 컨트롤(EsButton/EsDataGrid 등) 시각 기준, 주요 창 레이아웃 기준 추가(작업 A 안전망과 연계).
