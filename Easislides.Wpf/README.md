# Easislides.Wpf (Sprint 0 산출물)

> EasiSlides v3.0 UI 마이그레이션의 출발점. WinForms → WPF + Fluent 2 (ADR-0001).
> 본 프로젝트는 **Sprint 0 PoC 단계** — 정식 마이그레이션은 Sprint 1 이후 진행.

## 디렉터리 구조

```
Easislides.Wpf/
├── Easislides.Wpf.csproj   # 프로젝트 파일 (UseWPF + UseWindowsForms)
├── app.manifest            # DPI 인식 / Windows 10/11 호환 선언
├── App.xaml(.cs)           # 진입점 + DI 부트스트랩
├── Theme/
│   ├── EasiDS.xaml         # 디자인 시스템 마스터 ResourceDictionary
│   ├── Tokens/
│   │   ├── Colors.Light.xaml   # 라이트 모드 색 토큰 (Q3)
│   │   ├── Colors.Dark.xaml    # 다크 모드 — Solid 강제
│   │   ├── Typography.xaml     # 타입 스케일 (ADR-0003 폰트)
│   │   ├── Spacing.xaml        # 4-base 스페이싱
│   │   ├── Radius.xaml         # Fluent 2 반경
│   │   └── Motion.xaml         # 모션 토큰
│   └── ThemeService.cs     # 런타임 테마/시니어 모드 전환 (ADR-0006)
├── Controls/
│   └── EsButton.xaml       # Primary/Secondary/Danger 3변종 (§5.1)
├── Input/
│   ├── Shortcut.cs         # 단축키 정의 (ADR-0004)
│   └── ShortcutRegistry.cs # 글로벌+로컬 단일 소스
├── Interop/
│   └── OfficePptSession.cs # COM STA 어피니티 래퍼 (§10.3)
├── Demo/
│   ├── DemoWindow.xaml(.cs)
│   └── ...                 # 토큰·테마 시각 검증 + PoC 진입점
├── Poc/
│   ├── PocAHookTest.xaml(.cs)   # HookManager 충돌 검증
│   └── PocBComStress.xaml(.cs)  # 100회 COM stress
└── External/
    └── HookManager/        # .csproj가 ..\Easislides\HookManager\에서 file-link
```

## 빌드

```powershell
# 솔루션 전체 (Easislides + Easislides.Wpf 모두 빌드)
dotnet build Easislides.sln -nologo -v minimal

# 신규 WPF 프로젝트만
dotnet build Easislides.Wpf\Easislides.Wpf.csproj -nologo -v minimal

# 실행 (Sprint 0 데모)
dotnet run --project Easislides.Wpf\Easislides.Wpf.csproj
```

> **참고**: `dotnet`이 PATH에 없을 경우 CLAUDE.md §8의 PATH 재로딩 명령 실행 후 다시 시도.

## Sprint 0 PoC 합격 기준

### PoC-A — HookManager 호환성 (ADR-0004)
- [ ] 로컬 단축키(Ctrl+F)가 창 활성 상태에서 발화
- [ ] 글로벌 단축키(F5)가 다른 앱 활성 상태에서도 EasiSlides 명령 실행
- [ ] 글로벌 후킹 활성 시 다른 앱으로 키 전달 차단 (`SuppressKeyPress`)
- [ ] WPF `PreviewKeyDown`에서 `e.Handled=true` 처리 시 라우트 종료 확인

### PoC-B — COM STA Stress (§10.3)
- [ ] 100회 순회 모두 성공 (실패 0건)
- [ ] 종료 후 POWERPNT.EXE 좀비 프로세스 0건
- [ ] 100회 진행 중 UI 응답성 유지 (버튼 클릭·로그 갱신 즉시 반응)
- [ ] 회당 평균 시간 < 2초 (참고치)

위 항목 중 하나라도 실패 시 Sprint 1 진입 차단 — 계획서 §8.3.

## EasiDS 토큰 사용법

XAML에서:
```xml
<Border Background="{DynamicResource Brush.Surface.Card}"
        BorderBrush="{DynamicResource Brush.Border.Default}"
        CornerRadius="{StaticResource CornerRadius.Medium}"
        Padding="{StaticResource Thickness.Lg}">
    <TextBlock Text="안녕하세요"
               Style="{StaticResource Type.Body.Style}"
               Foreground="{DynamicResource Brush.Text.Primary}" />
</Border>
```

코드에서:
```csharp
var theme = App.Services.GetRequiredService<IThemeService>();
theme.ApplyTheme(ColorTheme.Dark);                  // 무대 모드
theme.ApplyInterfaceSize(InterfaceSize.Senior);     // 시니어 모드
```

**금지** (Sprint 1에 Roslyn 분석기 `EasiDS001`로 차단):
```csharp
// ❌ 매직 색 직접 사용
border.Background = new SolidColorBrush(Color.FromArgb(255, 23, 23, 23));
// ❌ 매직 폰트 직접 생성
text.FontFamily = new FontFamily("Microsoft Sans Serif");
// ❌ 비정형 패딩
panel.Margin = new Thickness(3, 5, 3, 5);
```

## 다음 단계 (Sprint 1)

1. PoC-A/B 합격 확인 (실제 실행 결과 캡처)
2. Pretendard Variable TTF를 `Assets/Fonts/`에 배치 + Typography.xaml `Font.Primary` 갱신
3. Fluent UI System Icons 빌드 파이프라인 구축
4. `EsTextBox`, `EsComboBox`, `EsToggle`, `EsTabView` 추가
5. `IThemeService.cs` 단위 테스트 (xUnit)
6. Roslyn 분석기 `EasiDS001` 작성 (매직 색·폰트 차단)
7. M1 마일스톤 — `WpfMainWindow`(FrmMain 신규) 착수

## 참조

- [계획서 (v1.1)](../docs/ui-ux-modernization-plan.md)
- [ADR-0001 ~ 0007](../docs/adr/)
