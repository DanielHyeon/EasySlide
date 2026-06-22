# 아이콘 파이프라인 — Microsoft Fluent UI System Icons 추출 가이드

> **결정**: 외주 없이 내부 작업으로 Microsoft 공식 Fluent UI System Icons에서 직접 추출.
> 근거: [ADR-0002](../adr/0002-fluent-icons.md) · [계획서 §4.5](../ui-ux-modernization-plan.md) · [§11.B 매핑 표](icon-migration-map.md)

## 0. 한 줄 요약

레거시 PNG/BMP raster 60+개 → Microsoft `microsoft/fluentui-system-icons` (MIT) 라이브러리의 동등한 SVG/XAML 자산으로 1:1 교체. 디자이너 외주 0원, 추출·변환은 내부 1~2일 작업.

## 1. 소스: Fluent UI System Icons

| 항목 | 값 |
|---|---|
| 공식 저장소 | <https://github.com/microsoft/fluentui-system-icons> |
| 라이선스 | MIT |
| 아이콘 수 | 3,000+ (Regular + Filled 페어) |
| 제공 사이즈 | 12 / 16 / 20 / 24 / 28 / 32 / 48 px (사이즈별 최적화) |
| 포맷 | SVG · XAML · Symbol Font · PNG (4종) |
| 명명 규칙 | `ic_fluent_<name>_<size>_<weight>.svg` (예: `ic_fluent_play_circle_24_regular.svg`) |
| 검색 UI | <https://aka.ms/fluenticons> (공식 카탈로그) — 한국어 키워드도 인덱싱됨 |
| WPF UI 라이브러리 내장 | ✅ `Wpf.Ui.Controls.SymbolIcon` enum으로 즉시 사용 가능 |

## 2. 3가지 사용 옵션 — 각각 언제 쓰나

### 옵션 A: WPF UI `SymbolIcon` (권장, 즉시 사용 가능)

WPF UI 3.x 라이브러리가 Fluent 아이콘 전체를 Symbol Font + Enum으로 내장. **추가 다운로드·변환 0**.

```xml
<Ui:SymbolIcon Symbol="Book24" FontSize="24" />
<Ui:SymbolIcon Symbol="PlayCircle24" />
<Ui:SymbolIcon Symbol="EyeOff24" Foreground="{DynamicResource Brush.Live.Active}" />
```

**언제**: 표준 Fluent 아이콘 셋에 정확히 매칭되는 메타포일 때 (대부분의 경우).

**장점**: 코드 한 줄, 의존성 0, 빌드 산출물 증가 없음.
**단점**: WPF UI 라이브러리의 Symbol enum 안에 있는 것만 사용 가능 (3,000개 전부 다 들어 있지는 않을 수 있음 — 확인 후 옵션 B 폴백).

### 옵션 B: SVG → XAML `DrawingImage` 빌드 변환

옵션 A에 없는 아이콘 또는 커스터마이즈(이중 색 등)가 필요할 때.

#### B-1. 수동 변환 (10개 미만)

1. <https://aka.ms/fluenticons>에서 검색 → SVG 다운로드
2. Visual Studio Blend의 "이미지 → XAML 변환" 또는 온라인 변환기 (<https://github.com/BerndK/SvgToXaml>)
3. `Easislides.Wpf/Assets/Icons/<Name>.xaml`에 `DrawingImage` 또는 `Geometry` 리소스로 저장
4. `Theme/Icons.xaml`에 머지

예시:
```xml
<!-- Assets/Icons/EsIconCustom.xaml -->
<DrawingImage x:Key="Icon.Custom">
    <DrawingImage.Drawing>
        <GeometryDrawing Brush="{DynamicResource Brush.Text.Primary}">
            <GeometryDrawing.Geometry>
                <PathGeometry Figures="M 4 4 L 20 4 ..." />
            </GeometryDrawing.Geometry>
        </GeometryDrawing>
    </DrawingImage.Drawing>
</DrawingImage>
```

사용:
```xml
<Image Source="{StaticResource Icon.Custom}" Width="24" Height="24" />
```

#### B-2. 빌드 자동화 (수십 개 이상)

`Easislides.Wpf.csproj`에 빌드 타깃 추가:

```xml
<Target Name="ConvertSvgToXaml" BeforeTargets="BeforeBuild">
    <Exec Command="inkscape --export-type=xaml --export-filename=&quot;%(SvgIcon.RelativeDir)%(SvgIcon.Filename).xaml&quot; &quot;%(SvgIcon.FullPath)&quot;"
          Condition="!Exists('%(SvgIcon.RelativeDir)%(SvgIcon.Filename).xaml')" />
</Target>
<ItemGroup>
    <SvgIcon Include="Assets\Icons\Source\*.svg" />
</ItemGroup>
```

또는 [SvgToXaml CLI](https://github.com/BerndK/SvgToXaml) 사용:
```powershell
SvgToXaml.exe BuildDict /inputdir Assets/Icons/Source /outputname EsIcons.xaml
```

### 옵션 C: PNG 직접 사용 (비권장)

PNG는 하이-DPI에서 흐릿. 부득이한 경우만 — 예: 일러스트레이션·로고처럼 벡터화 어려운 자산.

## 3. 작업 흐름 (Sprint 1 Day 1~2)

1. **매핑 표 확정** — [icon-migration-map.md](icon-migration-map.md)의 모든 행을 검토하여 신규 Fluent 아이콘 이름이 실제 카탈로그에 존재하는지 <https://aka.ms/fluenticons>에서 확인.
2. **옵션 A 우선** — WPF UI `SymbolIcon` enum (`Symbol="Book24"` 등)으로 사용 가능한 항목 표시.
3. **옵션 B로 보충** — WPF UI에 없는 항목은 SVG 다운로드 → `Easislides.Wpf/Assets/Icons/Source/`에 저장 → 변환.
4. **`Theme/Icons.xaml` 정리** — 모든 도메인-특화 아이콘 키를 한 곳에 모아 명명 표준화:
   ```xml
   <Ui:SymbolIcon x:Key="Icon.Bible" Symbol="Book24" />
   <Ui:SymbolIcon x:Key="Icon.Media.Play" Symbol="PlayCircle24" />
   <DrawingImage x:Key="Icon.Worship" ... /> <!-- 커스텀 -->
   ```
5. **레거시 자산 삭제** — `Easislides/Resources/*.png`, `EasislideImages/*.png` 모두 제거 (Q6 100% 폐기).
6. **빌드 검증** — `dotnet build`로 미사용 리소스 참조 0건, 신규 아이콘 표시 확인.

## 4. 색상·크기 정책 (계획서 §4.5)

- **사이즈**: 컨트롤별 고정 — 버튼 인라인 16, 툴바 24, 카드 32 (시니어 모드 시 ScaleFactor 적용)
- **색**: 모두 EasiDS 토큰 참조 (`Brush.Text.Primary` / `Brush.Accent.Primary` / `Brush.Live.Active`)
- **상태**: 베이스 1개 SVG + 색·투명도 토큰으로 hover/pressed/disabled 표현 — 별파일 금지

## 5. 검증 체크리스트

- [ ] [icon-migration-map.md](icon-migration-map.md)의 모든 행이 옵션 A 또는 B에 매핑됨
- [ ] 매핑 표에 ❌ 표시된 항목(예: 사용자 콘텐츠로 분리) 처리됨
- [ ] `Easislides.Wpf/Assets/Icons/` 또는 WPF UI Symbol enum만 사용, 외부 raster 0건
- [ ] `Theme/Icons.xaml`에 도메인-특화 키(`Icon.Bible`, `Icon.WorshipList` 등) 정리됨
- [ ] 다크/라이트 모드 양쪽에서 아이콘 색 토큰이 정상 전환되는지 시각 확인
- [ ] `Easislides/Resources/`, `EasislideImages/` 폴더 전체 삭제됨

## 6. 참고 자료

- ADR-0002 — Fluent UI System Icons 채택 사유 + 대안 비교
- 계획서 §4.5 — 아이콘 시스템 정책
- 계획서 §11.B — 핵심 매핑 (현행 → 신규) 요약
- <https://github.com/microsoft/fluentui-system-icons> — 공식 저장소
- <https://aka.ms/fluenticons> — 검색 가능한 카탈로그
- <https://wpfui.lepo.co/documentation/icons.html> — WPF UI SymbolIcon 사용법
