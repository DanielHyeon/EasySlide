# 렌더링, Office, 미디어 이식 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: Office Interop UI freeze, STA 스레드 리스크, 라이브 출력 안전성
- `docs/adr/0001-wpf-ui-framework.md`: WPF 기반 렌더링 UI
- `docs/adr/0002-fluent-icons.md`: PPT/미디어/문서 아이콘 전환
- `docs/adr/0007-legacy-ui-safety-net.md`: rendering 이식 중 legacy fallback 유지
- `docs/ui/icon-migration-map.md`: `Icon.PowerPoint.*`, `Icon.Media.*`, `Icon.Document.*`

## 1. 범위

이 문서는 PPT/이미지/미디어 렌더링과 출력 품질을 WPF로 옮기는 계획이다.

대상 legacy 파일:

| Legacy | 역할 | WPF 목표 |
|---|---|---|
| `ImageCanvas.cs` | 이미지 캔버스/썸네일 표시 | `PreviewCanvas`, `SlidePreviewControl` |
| `ImageTransitionControl.cs` | 이미지 전환 효과 | WPF animation/transition service |
| `gfImages.cs` | 이미지 처리 | `IImageAssetService` |
| `gfMedia.cs` | 미디어 처리 | `IMediaPlaybackService` |
| `gfPowerPoint.cs` | PowerPoint 연동 | `IPowerPointRenderService` |
| `OfficeLib/PowerPoint.cs`, `OfficeLib/WordDoc.cs` | Office Interop | STA 격리 service |
| `DirectShow/`, `DShowLib.cs` | 영상/캡처/재생 연동 | media backend adapter |
| `gfDisplay.cs` | 디스플레이/출력 | output rendering service |
| `FrmLaunchMediaPlayer.cs`, `FrmMediaPlayerControl.cs` | 미디어 제어 UI | WPF media controls |

현재 WPF 기반:

- `Easislides.Wpf/Interop/OfficePptSession.cs`
- `Easislides.Wpf/Poc/PocBComStress.xaml`
- `Easislides.Wpf/Rendering/PowerPointRenderService.cs`
- `Easislides.Wpf/Rendering/ThumbnailCache.cs`
- `Easislides.Wpf/Rendering/ImageAssetService.cs`
- `Easislides.Wpf/Rendering/PreviewCanvas.cs`
- `Easislides.Wpf/Rendering/TransitionEffectService.cs`
- `Easislides.Wpf/Rendering/OutputRenderer.cs`
- `Easislides.Wpf/Media/MediaPlaybackService.cs`
- `Easislides.Wpf/Media/MediaPlaybackViewModel.cs`

## 2. 핵심 리스크

| 리스크 | 영향 | 대응 |
|---|---|---|
| PowerPoint COM STA 위반 | UI freeze, 좀비 POWERPNT.EXE | 전용 STA 실행 경계, 명시적 release |
| 렌더링 결과 불일치 | 실제 송출 품질 저하 | WinForms/WPF 이미지 diff 검증 |
| 미디어 재생 backend 차이 | 코덱/싱크 문제 | adapter 패턴, DirectShow 유지 가능 |
| 출력 화면 DPI/스케일 차이 | 잘림/블러/좌표 오류 | per-monitor DPI 테스트 |
| 이미지 캐시 누수 | 장시간 운영 메모리 증가 | cache cap, dispose policy |

## 3. 목표 아키텍처

| 컴포넌트 | 책임 |
|---|---|
| `IPowerPointRenderService` | PPT 파일 열기, 슬라이드 export, 썸네일 생성 |
| `IOfficeStaScheduler` | Office COM 작업 직렬화, timeout, cleanup |
| `IThumbnailCache` | PPT/이미지 썸네일 캐시, 파일 stamp 기반 invalidation, LRU/용량 제한 |
| `IImageAssetService` | 이미지 로드, 리사이즈, 배경 처리 |
| `ITransitionEffectService` | legacy 58개 이미지 전환 목록, action/background/progress/frame 계약 |
| `IMediaPlaybackService` | 재생/일시정지/정지/seek/volume |
| `IOutputRenderer` | `LiveSessionSnapshot`/`OutputWindowState` 기반 scene snapshot, 이미지 배치, 전환 frame 제공 |
| `IRenderTelemetry` | 렌더 시간, 실패 파일, COM cleanup 기록 |

## 4. 이식 단계

### 4.0 ADR 준수 체크

- PowerPoint/미디어 관련 새 UI는 레거시 raster 아이콘을 사용하지 않는다.
- 렌더링/미디어 이식이 완료되기 전에는 WinForms rendering fallback을 제거하지 않는다.
- Office COM 호출은 WPF UI thread에서 직접 실행하지 않는다.
- 렌더 실패는 라이브 상태를 불명확하게 만들지 않고 LiveBar/toast/log에 표시한다.

### 4.1 렌더링 계약 작성

- 입력: 파일 경로, 슬라이드 번호, 출력 크기, 테마/배경 옵션
- 출력: bitmap stream, metadata, error
- 실패: unsupported, file locked, timeout, missing office, corrupt file로 분류
- 모든 렌더 결과는 cancellation token과 timeout을 지원한다.

완료 조건:

- UI thread에서 직접 Office COM 호출이 없다.
- 렌더 실패가 앱 종료로 이어지지 않는다.
- 같은 요청은 cache hit를 통해 재사용된다.

### 4.2 PPT 렌더링 이식

- `gfPowerPoint.cs`와 `OfficeLib/PowerPoint.cs`의 주요 호출 경로를 목록화한다.
- `OfficePptSession`을 운영 service로 확장한다.
- 슬라이드 export 결과를 기존 WinForms 결과와 비교한다.
- 파일 잠금, 암호, 손상 PPT, PowerPoint 미설치 상황을 분리 처리한다.

검증 기준:

- 대표 PPT 10개에서 슬라이드 수가 일치한다.
- 각 슬라이드 썸네일 크기와 비율이 일치한다.
- export 후 POWERPNT.EXE 잔존 0건.
- 같은 PPT 반복 렌더 100회에서 실패 0건.

### 4.3 이미지/전환 이식

- `ImageCanvas`의 그리기 규칙을 `PreviewCanvas`로 옮긴다.
- `ImageTransitionControl`의 효과를 WPF animation으로 재현한다.
- 전환 효과는 live output과 preview가 같은 상태 machine을 사용한다.
- 사용자 배경 이미지와 기본 이미지 자산의 DPI/비율 정책을 통일한다.

### 4.4 미디어 이식

- 기존 DirectShow 기반 기능 중 필수 기능과 대체 가능한 기능을 구분한다.
- WPF `MediaElement`로 충분한 기능과 DirectShow 유지가 필요한 기능을 나눈다.
- 재생 상태는 `MediaPlaybackViewModel`로 단일화한다.
- 라이브 중 stop/restart/seek는 SafetyConfirm 정책을 적용한다.

필수 기능:

- 재생/일시정지/정지
- seek
- volume/mute
- 현재 재생 시간/전체 시간
- 출력 화면과 control panel 상태 동기화
- 파일 오류/코덱 오류 안내

## 5. 완료 여부

| 항목 | 상태 | 비고 |
|---|---|---|
| Office COM PoC | 1차 완료 | `OfficePptSession`, stress 화면 존재 |
| PPT 운영 service | 부분 구현 | `IPowerPointRenderService`, `PowerPointRenderService`, `IPowerPointRenderBackend`, `OfficePowerPointRenderBackend` 추가. 파일 검증, timeout, `IThumbnailCache` 기반 cache invalidation, 오류 분류, STA `OfficePptSession.ExportSlideAsync` 경계 구현 및 자동 검증 완료. 실제 운영 PPT fixture 10개/100회 stress 검증은 후속 필요 |
| 썸네일 캐시 | 1차 구현 완료 | `IThumbnailCache`, `ThumbnailCache`, `ThumbnailCacheKey` 추가. 파일 경로/stamp/길이/크기/variant 기반 cache key, source invalidation, entry/byte limit, LRU eviction, 통계 snapshot, `PowerPointRenderService` 공유 캐시 주입 계약 자동 검증 완료. 이미지 asset service와 실제 디스크 캐시 연동은 후속 필요 |
| 이미지 프리뷰 WPF 컨트롤 | 1차 구현 완료 | `IImageAssetService`, `ImageAssetService`, `PreviewCanvas` 추가. legacy `ImageCanvas.ResizeCanvas`와 같은 fit 중앙 정렬 계약, fill/stretch/center 배치, 이미지 메타데이터 로드, unsupported/locked/decode 오류 분류, bitmap pixel dimension 기반 DPI 안전 배치, `Source`/`FillMode`/선택 테두리/슬라이드 번호 WPF 렌더링 자동 검증 완료. 출력 renderer scene contract 연결은 완료했으며, `SlidePreviewControl` 연결과 WinForms/WPF 이미지 diff는 후속 필요 |
| 전환 효과 WPF화 | 부분 구현 | `ITransitionEffectService`, `TransitionEffectService` 추가. legacy `ImageTransitionControl`의 58개 전환 이름/순서, `TransitionAction`, background layer 정책, progress clamp, fade/slide/reveal/stretch/zoom/spin/flip frame 계약 자동 검증 완료. output renderer scene frame 연결은 완료했으며, 실제 WPF animation visual 적용은 후속 필요 |
| 미디어 playback WPF화 | 부분 구현 | `IMediaPlaybackService`, `MediaPlaybackService`, `MediaPlaybackViewModel`로 재생 상태 계약, command, 시간 표시, seek/audio setting clamp 자동 검증 완료. 실제 WPF `MediaElement`/DirectShow backend, 출력 화면 렌더 연결, 파일/코덱 오류 UI는 후속 구현 필요 |
| 출력 renderer 동등성 | 부분 구현 | `IOutputRenderer`, `OutputRenderer`, `OutputSceneSnapshot` 추가. `LiveSessionSnapshot`/`OutputWindowState`에서 Live/Hidden/Blackout/Ready/Standby 장면, 출력 모니터명, 표시 라벨, blackout flag, viewport, content placement, transition frame을 생성하고 `OutputWindowViewModel.Scene`으로 바인딩 경계를 고정했다. 실제 WinForms/WPF 이미지 diff와 full output visual renderer는 후속 필요 |

## 6. 이식 후 검증 방안

렌더링 검증:

- 샘플 PPT/이미지/영상 세트로 WinForms와 WPF 결과를 비교한다.
- 썸네일 수, 순서, 비율, 배경, 투명 영역 처리 결과를 확인한다.
- 장시간 운영 중 메모리 사용량이 계속 증가하지 않는지 확인한다.
- 썸네일 캐시는 파일 수정 시간/길이가 바뀌면 miss 처리하고 source invalidation으로 관련 variant를 제거한다.
- 이미지 preview 배치는 `ImageAssetService.CalculatePlacement`의 fit/fill/stretch/center 결과를 기준으로 WPF Control에서 동일하게 적용한다.
- 출력 scene snapshot은 `OutputRenderer`에서 Live/Hidden/Blackout/Ready/Standby 상태, content placement, transition frame을 동일 계약으로 생성한다.

Office 검증:

- PPT 100회 반복 열기/렌더/닫기.
- 실패 PPT 후 다음 정상 PPT 렌더가 가능한지 확인.
- PowerPoint 프로세스 잔존 여부 확인.
- UI 버튼 클릭과 로그 갱신이 렌더 중에도 응답하는지 확인.
- 현재 1차 자동 검증은 fake backend 기반 service contract로 수행한다. 실제 PowerPoint 설치 환경에서는 `OfficePowerPointRenderBackend`와 `OfficePptSession.ExportSlideAsync`를 사용해 JPG export를 수행한다.

미디어 검증:

- MP4, WMV, MP3 등 실제 운영 파일 재생.
- pause/resume/seek 후 출력 화면과 컨트롤 상태 동기화.
- 코덱 오류 파일에서 앱이 멈추지 않고 안내를 표시.
- 라이브 중 미디어 정지/seek가 안전 확인을 거침.
- 현재 1차 자동 검증은 backend 없는 in-memory 상태 계약으로 수행한다. 실제 playback backend 연결 후 fixture 파일 기반 통합 검증을 추가한다.

## 7. 테스트 방안

자동 테스트:

- render contract unit tests
- thumbnail cache invalidation tests
- COM scheduler timeout/cancellation tests
- PowerPoint render service cache/timeout/error classification tests
- image asset metadata/layout/error classification tests
- media playback ViewModel state tests
- media playback service state machine tests
- output snapshot state tests

현재 자동화 완료:

- `PowerPointRenderServiceTests`: backend 결과 반환, 파일 stamp 기반 cache hit, 파일 변경 invalidation, unsupported/locked file 차단, timeout, backend 오류 분류 검증
- `ThumbnailCacheTests`: cache hit, 파일 stamp 변경 miss, source invalidation, LRU entry/byte eviction, 통계 snapshot 검증
- `ImageAssetServiceTests`: fit/fill 배치, 이미지 metadata 로드, unsupported/locked/decode error 분류 검증
- `PreviewCanvasTests`: `ImageAssetService.CalculatePlacement` 계약 기반 fit/fill 배치, DPI별 bitmap pixel dimension 처리, source 없음 처리, slide number/selection 포함 WPF pixel render 검증
- `TransitionEffectServiceTests`: legacy 58개 전환 목록/표시명/해석, `AsFade` override, background layer, fade opacity, slide/reveal frame 계약 검증
- `OutputRendererTests`: Live/Blackout/Standby scene, content placement, transition frame, 표시 라벨 계약 검증
- `MediaPlaybackServiceTests`: load/play/pause/stop, seek clamp, volume/balance/mute/repeat 상태 유지 검증
- `MediaPlaybackViewModelTests`: load 표시값, play/pause command, 5초 seek command, mute/repeat toggle 검증

2026-05-29 검증 결과:

- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "MediaPlaybackServiceTests|MediaPlaybackViewModelTests"`: 8개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "ThumbnailCacheTests|PowerPointRenderServiceTests"`: 12개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter PowerPointRenderServiceTests`: 7개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter ImageAssetServiceTests`: 6개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter PreviewCanvasTests`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter TransitionEffectServiceTests`: 7개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "OutputRendererTests|OutputWindowViewModelTests"`: 6개 통과
- `dotnet test Easislides.sln -c Debug`: 140개 통과
- `dotnet build Easislides.sln -c Release`: 성공
- `dotnet test Easislides.sln -c Release --no-build`: 140개 통과
- `gstack /qa`, `GSD verify-work`: 현재 작업 환경 PATH에 도구가 없어 실행 불가. 동일 요구사항은 xUnit/Release build/산출물 확인으로 대체 검증

통합 테스트:

- PPT fixture 10개 렌더 후 metadata 비교
- 같은 PPT 100회 stress
- 이미지 fixture resize/fit/fill 비교
- cache hit/miss 성능 측정

수동 테스트:

1. PPT 파일 열기
2. 슬라이드 썸네일 전체 생성
3. 특정 슬라이드 live 전송
4. 이미지 배경 적용
5. 전환 효과 실행
6. 영상 재생
7. 라이브 중 pause/seek/stop
8. 출력 모니터에서 실제 화면 확인

성능 목표:

- 일반 PPT 첫 썸네일 2초 이내 표시
- 100장 PPT 전체 썸네일 생성 중 UI 응답 유지
- live next/prev 체감 지연 100ms 이하
- 2시간 운영 후 메모리 증가가 허용 범위 내
