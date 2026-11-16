# Proposal: 화면 그리기 쪽에 남은 문제들

## Why

앞선 변경들의 조사와 리뷰에서 확인됐지만 범위 밖으로 미뤄 둔 그리기 쪽 문제들이다. 모두 이번 작업 이전부터 있었다.

1. **사각형 전환 효과가 새 그림을 그리지 않는다.** `Rectangle`, `RectangleIn` 전환은 잘라낼 영역만 정하고, 그 안에 그릴 그림을 그리는 줄이 주석으로 막혀 있다. 전환 시간 동안 한쪽 그림만 보이다가 끝에서 한 번에 바뀐다.
2. **전환 프레임이 실패하면 화면 크기의 작업용 비트맵이 해제되지 않는다.** 여러 전환 효과가 프레임마다 화면 크기의 비트맵을 만들고, 그리다 예외가 나면 해제하는 줄을 건너뛴다 (문 열림/닫힘 전환은 `fix-show-window-lifecycle`에서 고쳤다).
3. **알림과 참조 표시를 켤 때마다 글꼴 객체가 해제되지 않는다.** `StartAlert`, `LoadRef`가 부를 때마다 `Font`를 만들고 버린다. 가사를 그릴 때는 `FontFamily`를 만들고 해제하지 않는다.
4. **부를 때마다 배경 그림을 복제하고 해제하지 않는 곳이 있다.** `SetColoursFormat`, 그리고 PPT 항목의 슬라이드를 바꿀 때마다 약 10MB가 GC 전까지 쌓이는 경로 (`fix-show-window-lifecycle` 검증 중 측정).
5. **화면이 폐기될 때 처음 만든 복제본이 해제되지 않는다.** `EnsureImageBuffers`와 `gfUtility`가 만든 복제본을 화면이 자기 것인지 구분하지 못해 종료자에 맡긴다.
6. **송출 단 썸네일을 불러올 때 스크롤 호출이 예외 처리 밖에 있다.** (`LoadThumbOutImages`의 `ScrollControlIntoView`)
7. **쇼가 진행 중일 때 앱을 닫으면 송출 창의 정리가 실행되지 않는다.** 작업 표시줄과 화면 보호기 설정 복원, 올려 둔 슬라이드 쇼 내리기가 빠진다 (`fix-show-window-lifecycle`의 알려진 한계).

## What Changes

- 사각형 전환이 다른 모양 전환(원, 별 등)과 같은 방식으로 새 그림을 그린다.
- 전환 프레임의 작업용 비트맵과 Graphics를 실패해도 해제한다.
- 글꼴 객체를 다시 쓰거나 해제한다.
- 배경 그림 복제본을 화면이 소유하고, 바꿀 때와 폐기될 때 해제한다. 다른 코드가 넣은 그림은 해제하지 않는다.
- 썸네일 스크롤 호출을 예외 처리 안으로 넣는다.
- 쇼가 진행 중일 때 앱을 닫으면 쇼를 먼저 정상 경로로 끝낸다.
- (리뷰에서 추가) 닫기가 취소됐거나 Windows 종료가 아직 묻는 단계이면 아무것도 닫지 않는다. 지금은 그때도 PowerPoint를 놓고 후킹을 풀고, 송출 창은 쇼 정리를 실행한다.

## Out of Scope

- 출력 영역이 모니터보다 좁은 설정에서 PPT 슬라이드 쇼가 모니터 전체를 덮는 것 (설정과 PowerPoint의 동작)
- 전환 효과의 종류나 시간 변경
- NetOffice 내부의 첫 예외(first-chance) 소음

## Impact

- `Easislides/Easislides/ImageTransitionControl.cs`, `FrmMain.cs`, `FrmMain.Logic.cs`, `FrmMain.Layout.cs`
- `Easislides/Global/gfDisplay.cs`, `gfUtility.cs`, `gfColorsFonts.cs`
