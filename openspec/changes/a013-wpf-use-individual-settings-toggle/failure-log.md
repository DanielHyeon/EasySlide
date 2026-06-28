# 실패 기록

## Red 테스트

명령:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CanEditSelectedItemColor_True_ForNoticeTextItemWithBody" --no-restore -v minimal
```

production 수정 전 실패:

```text
Expected sut.CanEditSelectedItemColor to be True ... but found False.
```

근본 원인:

`MainViewModel.IsPerItemFormattable`이 `LiveItemKinds.Notice`를 제외했다. 그래서 본문이 있는 텍스트 파일/공지 항목이 미리보기 패널에 표시되더라도 개별 설정 명령은 비활성화됐다.

## Green 테스트

`Lyrics`가 비어 있지 않은 경우 `LiveItemKinds.Notice`를 서식 가능 항목에 포함한 뒤, 동일 focused 테스트가 통과했다.
