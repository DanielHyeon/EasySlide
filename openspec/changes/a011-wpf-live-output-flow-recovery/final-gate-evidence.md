# Final Gate Evidence

## 집중 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~OutputWindowHostTests|FullyQualifiedName~PreviewWindowHostTests|FullyQualifiedName~GoLiveCommand_WhenOutputClosed_OpensOutputAndStartsPreviewLikeFrmMain|FullyQualifiedName~PreviewToLiveCommand_OpensOutputAndPublishesPreviewWithoutAdvancingSelection|FullyQualifiedName~ToggleOutputLiveCommand_WhenNoPreparedOutput_StartsFirstWorshipItemLikeFrmMain|FullyQualifiedName~GoLiveCommand_PowerPoint_StartsSlideShowOnSelectedOutputMonitor" -v minimal
```

- 결과: 통과
- 테스트 수: 19개

## 전체 WPF 테스트

```powershell
dotnet test Easislides.Wpf.Tests --no-restore -v minimal
```

- 결과: 통과
- 테스트 수: 2,433개

## OpenSpec 검증

```powershell
openspec validate a011-wpf-live-output-flow-recovery --strict
```

- 결과: 통과

## WinForms 빌드

```powershell
dotnet build Easislides\Easislides.csproj -nologo -v minimal
```

- 결과: 통과
- 경고: 기존 NetOffice/DirectShow/WinForms analyzer 경고 유지

## WPF 배포

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -v minimal
```

- 결과: 통과
- 배포 위치: `C:\EasiSlides\EasislidesNext`

## 남은 수동 UAT

- 실제 멀티모니터에서 Output 창을 X/Alt+F4로 닫은 뒤 Preview Live 재시도.
- 실제 PPT 항목에서 PowerPoint SlideShow가 선택 출력 모니터에 뜨는지 확인.
