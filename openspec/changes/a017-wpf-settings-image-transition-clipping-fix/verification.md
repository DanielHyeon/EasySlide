# 검증

## 자동 검증

- Red 확인: `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~SettingsVisualLayoutTests|FullyQualifiedName~SettingsControlRenderEvidenceTests" --no-restore`
  - 결과: 실패 2, 통과 8
  - 실패 원인: `ClassicSettingsComboBox` 미존재, `SelectedItemBackgroundImagePath` 텍스트 박스 스타일 미적용
- Green 확인: 같은 focused 테스트 재실행
  - 결과: 실패 0, 통과 10
- 통과: `dotnet build Easislides.Wpf\Easislides.Wpf.csproj --no-restore`
  - 결과: 오류 0, 기존 NuGet 호환성 경고 6개
- 통과: `openspec validate a017-wpf-settings-image-transition-clipping-fix --strict`
  - 결과: change valid
- 통과: `git diff --check -- Easislides.Wpf/MainWindow.xaml Easislides.Wpf.Tests/Shell/SettingsVisualLayoutTests.cs Easislides.Wpf.Tests/Shell/SettingsControlRenderEvidenceTests.cs openspec/changes/a017-wpf-settings-image-transition-clipping-fix`
  - 결과: 공백 오류 없음, 기존 CRLF 경고만 있음

## 캡쳐 검증

- 생성 및 직접 확인: `evidence/screenshots/2026-06-23/settings-image-transition-clipping/settings-image-transition-controls-after.png`
  - Image 경로 텍스트 박스와 Transition 콤보박스 샘플을 WPF 렌더 하네스로 캡쳐했다.
  - `image-gjpqy-sample.png`, `Slide From Bottom gjpqy`의 하단 획이 잘리지 않는 것을 확인했다.
- 추가 생성: `evidence/screenshots/2026-06-23/settings-image-transition-clipping/settings-controls-after.png`

## 실제 앱 캡쳐 시도

- Windows 앱 캡쳐 도구 연결 시도 결과: `node_repl/js` 도구가 `codex/sandbox-state-meta: missing field sandboxPolicy` 오류를 반환했다.
- 따라서 실제 창 자동 캡쳐는 확보하지 못했고, 이번 시각 확인은 WPF 렌더 하네스 캡쳐를 공식 증거로 사용한다.

## 배포 검증

- 통과: `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext --no-restore -nologo -v minimal`
  - 결과: `C:\EasiSlides\EasislidesNext`로 publish 완료, 오류 0
- 배포 파일 확인:
  - 파일: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
  - LastWriteTime: `2026-06-23 23:13:29`
- 배포본 실행 smoke:
  - 실행 파일: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
  - 결과: `MainWindowTitle=EasiSlides`, 창 핸들 생성, 프로세스 정상 실행
