# 검증

## 자동 검증

- 통과: `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~SettingsVisualLayoutTests" --no-restore`
  - 결과: 실패 0, 통과 6
- 통과: `dotnet build Easislides.Wpf\Easislides.Wpf.csproj --no-restore`
  - 결과: 오류 0, 기존 NuGet 호환성 경고 6개
- 통과: `openspec validate a016-wpf-settings-control-clipping-fix --strict`
  - 결과: change valid
- 통과: `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~SettingsVisualLayoutTests|FullyQualifiedName~SettingsControlRenderEvidenceTests" --no-restore`
  - 결과: 실패 0, 통과 7

## 캡쳐 검증

- 생성 및 확인: `evidence/screenshots/2026-06-23/settings-control-clipping/settings-controls-after.png`
  - `ClassicSettingsTextBox`와 같은 높이/패딩 조건을 적용한 텍스트 박스 2개와 `ClassicSettingsCheckBox` 조건을 적용한 체크박스를 렌더했다.
  - `g/j/p/q/y`처럼 하단 획이 있는 글자가 텍스트 박스와 체크박스에서 잘리지 않는 것을 이미지로 확인했다.

## 실제 앱 창 캡쳐 시도

- 실행 파일: `Easislides.Wpf\bin\Debug\net10.0-windows\EasislidesNext.exe`
- 확인: 프로세스가 실행되고 `MainWindowTitle=EasiSlides`, 창 핸들이 생성됐다.
- 생성 파일:
  - `evidence/screenshots/2026-06-23/settings-control-clipping/easislidesnext-mainwindow-after.png`
  - `evidence/screenshots/2026-06-23/settings-control-clipping/easislidesnext-mainwindow-screen-after.png`
  - `evidence/screenshots/2026-06-23/settings-control-clipping/easislidesnext-mainwindow-screen-after-10s.png`
- 결과: 실제 앱 창 캡쳐는 타이틀바와 흰 본문만 표시되어 설정 패널까지는 캡쳐되지 않았다.
- 판단: 이번 변경의 시각 확인은 WPF 렌더 하네스 기반 캡쳐로 완료했고, 실제 앱 창의 흰 본문 캡쳐 문제는 별도 런타임 캡쳐/초기화 이슈로 기록한다.

## 배포 검증

- 통과: `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext --no-restore -nologo -v minimal`
  - 결과: `C:\EasiSlides\EasislidesNext`로 publish 완료, 오류 0
  - 기존 NuGet/nullable/WinForms DPI/EasiDS analyzer 경고는 남아 있으나 publish는 성공했다.
- 배포 파일 확인:
  - 파일: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
  - LastWriteTime: `2026-06-23 23:06:03`
- 배포본 실행 smoke:
  - 실행 파일: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
  - 결과: `MainWindowTitle=EasiSlides`, 창 핸들 생성, 프로세스 정상 실행
