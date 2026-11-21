# MuseScore 악보 편집

WPF와 WinForms 모두 곡 편집기의 **악보 편집… → MuseScore 악보 열기…**에서 같은 편집창을 엽니다. MuseScore Studio에서 만든 피아노 2단 악보(.mscz, .mscx)를 열고, 악보나 오른쪽 목록에서 음표를 선택해 음높이와 1절 가사를 수정할 수 있습니다. 한 화음의 가사는 화음에 속한 음표들이 공유합니다. **실행 취소**는 직전 적용 한 건을 되돌립니다.

**저장**은 .mscz 파일을 씁니다. .mscx를 처음 저장하면 새 .mscz 파일의 위치를 선택합니다. **PPTX 내보내기**는 MuseScore가 페이지별로 악보 바깥의 빈 여백을 잘라낸 뒤, 원래 비율을 유지해 16:9 슬라이드 한 장에 배치합니다. 편집창을 닫으면 임시 악보와 실행 취소 기록을 정리합니다.

## 실행 환경

Windows용 [MuseScore Studio 4.6.5](https://github.com/musescore/MuseScore/releases/tag/v4.6.5)로 검증했습니다. 기본 설치 폴더를 자동으로 찾으며, 별도 위치의 실행 파일은 두 앱을 시작하기 전에 환경 변수 `AXICULTUS_MUSESCORE_EXE`로 지정할 수 있습니다. 악보 처리에는 로컬 MuseScore 실행 파일과 해당 설치본에 포함된 글꼴·확장 런타임이 필요합니다.

## 공통 C# API

```csharp
var engine = MuseScoreEngine.Find() ?? throw new InvalidOperationException("MuseScore Studio가 필요합니다.");
await using var score = await engine.OpenAsync(inputPath, cancellationToken);
var leftHand = score.Score.Notes.First(note => note.Staff == 2);
await score.EditAsync(leftHand, 52, "평화", cancellationToken);
await score.SaveAsync(outputMscz, cancellationToken);
await score.ExportPptxAsync(outputPptx, cancellationToken);
```

`MuseScoreEngine`은 MuseScore Studio의 명령줄 변환기와 engraving 확장 API를 사용합니다. 각 편집 작업은 선택한 단·성부·시간 위치·화음 내 음표에 적용됩니다. `MuseScoreSession`은 엔진이 저장한 악보와 렌더링된 페이지를 한 작업 단위로 보관합니다. 다음 악보를 다시 읽고 렌더링하는 데 성공하면 화면을 갱신하며, `UndoAsync`는 이전 작업 단위로 복귀합니다. 사용자 파일은 저장을 선택한 시점에 교체합니다.
