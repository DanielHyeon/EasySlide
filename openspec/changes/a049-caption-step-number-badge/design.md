# Design — a049 자막 단계의 번호

## 현재 구조 (영향 근거)

CodeGraph CLI는 이 세션에서 쓸 수 없었다. 아래는 2026-11-28에 읽은 코드다(a046 묶음 B가 이 파일들을 라이브러리로 옮긴 뒤의 위치를 구현자가 Phase 0에서 확인한다).

- 단계 카드 틀: `CaptionConsole.xaml`의 `StepStrip`(`ListBox`, `WrapPanel`). 카드는 140×62의 `Border` 안 `DockPanel`: 위에 `TextBlock Text="{Binding Label}"`(굵게), 아래에 `Image Source="{Binding Thumbnail}"`와 `TextBlock Text="{Binding Text}"`.
- 단계 만들기: `CaptionConsole.Operator.cs`의 `DeckSteps`가 악보 줄이면 `new CaptionStep(장, 줄, UiText "StepRow"(장, 줄+1), "", 그림)`, 아니면 `new CaptionStep(장, 0, "StepSlide"(장), 첫 줄, null)`. 페이지 단계는 `UpdateSteps`가 `new CaptionStep(페이지+1, 0, PageLabel(page), 첫 줄, null)`(`PageLabel`은 위치 글, 비면 "1/1").
- `CaptionStep`(`CaptionConsoleViewModel.cs`): `Slide`, `Row`, `Label`, `Text`, `Thumbnail`, 그리고 화면 읽기용 `Spoken`.
- 본창 미리보기 썸네일의 번호: `MainWindow.xaml`의 썸네일 틀에서 `TextBlock`(오른쪽 위, `MinWidth=18`, 가운데 맞춤, 굵게, 글자 검정, 바탕 `#FFF6E600`).
- 단계 이름표를 보는 테스트: `editor.Steps.Select(step => step.Label)`을 `"1/3"` 등과 비교하는 것들, `CurrentStep?.Label`.

## 결정

1. **번호 글**: `CaptionStep`에 번호 글(짧은 표시)과 말로 된 이름을 따로 둔다. 번호 글은 악보 줄 `"{장}-{줄}"`, 줄 없는 장 `"{장}"`, 페이지 단계는 위치 글. 말로 된 이름("1장 1줄", "Slide 1 row 1")은 지금 문구 그대로 화면 읽기용 이름에만 쓴다.
2. **카드 틀**: 위쪽 이름표 줄을 없애고, 카드 내용 위에 겹쳐 오른쪽 위 모서리에 번호를 둔다. 모양은 미리보기 썸네일의 번호와 같은 값(바탕 `#FFF6E600`, 글자 검정, 굵게, `MinWidth=18`, 가운데 맞춤, 좌우 여백 조금). 색은 테마를 따르지 않는다(미리보기와 같게, 밝은·어두운 테마 모두 같은 노랑).
3. **자리**: 이름표가 없어진 만큼 그림과 글이 카드 높이를 다 쓴다. 번호가 글 첫 줄의 오른쪽 끝을 가릴 수 있으므로 글에는 번호 폭만큼의 오른쪽 여백을 첫 줄에 주거나 위 여백을 준다(구현자가 그림으로 보고 정한다. 악보 줄 그림은 가려져도 된다: 미리보기 썸네일도 그림 위에 번호를 겹친다).
4. **테스트**: 번호 글을 보는 새 테스트(악보 줄 `1-1`·`1-2`·`2-1`, 줄 없는 장 `3`, 페이지 `2/3`), 화면 읽기 이름이 그대로인지. 지금 `Label`을 보는 테스트는 번호 글이 같은 값이면(페이지 단계) 그대로 통과해야 하고, 값이 바뀌는 것(악보 줄·장)만 새 값으로 고친다.
5. **그림**: 검토용 그림(`CaptionConsoleShots`)으로 밝은·어두운 테마에서 악보 줄 단계, 글 단계, 페이지 단계를 남긴다.

## 위험과 대응

- **지금 단계 테두리와 겹침**: 번호가 카드 모서리에 붙어 강조 테두리(2px)와 겹치지 않는지 그림으로 확인한다.
- **좁은 글 카드**: 글이 번호 밑으로 들어가 읽기 어려워지지 않는지 그림으로 확인한다.
