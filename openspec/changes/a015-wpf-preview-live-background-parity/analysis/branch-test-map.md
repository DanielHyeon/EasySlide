# 분기 테스트 맵

## Branch Coverage

| 분기 | 기존 테스트 | 추가 필요 | 기대 결과 |
| --- | --- | --- | --- |
| `UseIndividualFormatting=true` + `FormatData 61=` | `ToggleOutputLiveCommand_AfterCopyPreviewToOutput_PublishesSelectedPageAndFormatting` | `PreviewToLiveCommand` 직접 경로 보강 검토 | 항목 배경이 Live session에 실림 |
| `UseIndividualFormatting=false` + `FormatData 61=` | 없음 | 필요 | 미리보기 샘플이 항목 배경이 아니라 default/global 배경을 사용 |
| `FormatData` 없음 | 기존 샘플/렌더 테스트 | 불필요 | default/global 배경 사용 |
| 이미지 파일 로드 실패 | `OutputWindowViewModelTests` | 불필요 | 이미지 숨김 또는 색 배경 fallback |

## Fallback Coverage

- 전역 배경 이미지가 있으면 `UseIndividualFormatting=false` 상태의 샘플은 전역 배경을 사용해야 한다.
- 전역 배경 이미지가 없으면 색 배경 fallback을 사용해야 한다.

## Mutation Coverage

- `FormatData` 원문은 변경하지 않는다.
- `UseIndividualFormatting` 값도 변경하지 않는다.
- `OutputItem`, `_session.Current` 변경은 기존 Live 송출 테스트가 담당한다.

## Required New Tests

- `PreviewSampleAppearance_WhenIndividualFormattingOff_IgnoresItemBackgroundImage`
- 가능하면 `PreviewToLiveCommand_WithIndividualFormattingOn_CarriesPreviewBackgroundToLiveSession`

## Regression Tests

- `ToggleOutputLiveCommand_AfterCopyPreviewToOutput_PublishesSelectedPageAndFormatting`
- `SetSelectedItemBackgroundImage_WhileLive_CarriesOverridePath`
- `GoLive_WithSongFormatDataBackgroundImage_CarriesImagePath`
- `CreateScene_Active_SongBackgroundImage_WinsOverGlobal`
