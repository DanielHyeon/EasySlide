# 검증

## 실행 명령

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CanEditSelectedItemColor_True_ForNoticeTextItemWithBody" --no-restore -v minimal
```

결과: production 수정 전에는 실패했고, `IsPerItemFormattable`이 본문 있는 `Notice` 텍스트 항목을 포함하도록 수정한 뒤 통과했다.

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CanEditSelectedItemColor|FullyQualifiedName~SetSelectedItemTextColor|FullyQualifiedName~SetSelectedItemAlignment|FullyQualifiedName~SetSelectedItemFontSize|FullyQualifiedName~SetSelectedItemFontName|FullyQualifiedName~SetSelectedItemBackgroundColor|FullyQualifiedName~ApplySelectedItemFormatDataTemplate|FullyQualifiedName~SetSelectedItemBackgroundImage|FullyQualifiedName~ToggleSelectedItemEmphasis" --no-restore -v minimal
```

결과: 82개 테스트 통과.

```powershell
ast-grep scan --rule ast-grep\rules\csharp-risk-patterns.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf.Tests\Shell\MainViewModelTests.cs
```

결과: 통과, 발견 항목 없음.

```powershell
openspec validate a013-wpf-use-individual-settings-toggle --strict
```

결과: 통과.

## 수동 배포/화면 확인

- Release 배포 경로: `C:\EasiSlides\EasislidesNext`
- 실행 파일: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
- 사도신경 텍스트 항목에서 `Set` 모드 진입, `Use Individual Settings` 체크 상태, `Font / Back` 배경 설정 버튼 표시를 캡쳐로 확인했다.
- `Blue` 배경 설정 클릭 후 상태줄에 `항목 배경색: #FF0B5B7A`가 표시됨을 확인했다.
- `Go Live` 후 Preview/Output 양쪽에서 항목 배경 이미지가 변경된 화면을 캡쳐로 확인했다.

## 알려진 경고

- 기존 NetOffice 패키지의 `NU1701` 경고.
- 기존 `Easislides/HookManager/*` nullable 경고.
- 기존 `WFO0003` manifest 경고와 `EasiDS001` 디자인 토큰 경고.
