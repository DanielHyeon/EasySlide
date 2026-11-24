# WinForms PPT 썸네일 번호를 WPF 위치로

## Why

사용자 보고: WinForms(`Axicultuss`)의 PPT 썸네일 번호 위치가 WPF와 다르다. WPF에 맞춰 달라.

- WPF(`Axicultus/MainWindow.xaml`, 미리보기·송출 칸 `PowerPoint.Thumbnails`/`OutputPowerPoint.Thumbnails`): 슬라이드 그림이 썸네일 폭을 채우고(테두리 5px 안), 번호는 그 그림의 **오른쪽 위 모서리에 겹쳐** 노란 바탕(#F6E600)·검정 굵은 글자로 그린다.
- WinForms(`Axicultuss/Forms/ImageCanvas.cs`): `ContentBounds`가 오른쪽에 번호 자리를 비우고 슬라이드를 그만큼 줄여 그리며, `SlideNumberBounds`가 번호를 **슬라이드 오른쪽 바깥**(틀 안)에 그린다.

## What Changes

- WinForms PPT 썸네일에서 슬라이드는 번호 자리를 비우지 않고 틀 안을 채운다(가로세로 비율 유지).
- 번호는 슬라이드 그림의 오른쪽 위 모서리에 겹쳐 그린다(그림 안쪽, 위·오른쪽 가장자리에 붙음). 노란 바탕·검정 글자, 굵게. 두 자리 이상 번호도 잘리지 않는다.
- 썸네일 폭은 그대로, 높이는 WPF처럼 슬라이드 비율에 테두리를 더한 만큼이다(슬라이드 양옆·위아래에 빈 띠가 없다). 1차 구현은 높이를 고정해 16:9 슬라이드 양옆에 빈 띠가 남았다.
- 선택 표시, 순서, 클릭 동작은 그대로다.

## 허용 한계

- 썸네일 칸은 덱을 처음 보여 줄 때 디스크에 있는 슬라이드 그림 하나에서 덱의 슬라이드 비율을 읽어 그 높이로 놓는다(덱마다 한 번). 그때 내보낸 그림이 하나도 없으면 칸은 지금처럼 4:3으로 놓였다가 그림이 그려질 때 제 높이가 된다(기존 동작). 같은 덱의 슬라이드 비율이 바뀌어 다시 내보내는 경우도 같다. 그림이 그려질 때 모든 칸을 다시 배치하는 장치는 리뷰마다 새 문제(그리기 중 스크롤, 오래된 그림 비율, 핸들 재생성)를 낳아 넣지 않는다.
- 크기 조절 중에는 저장된 썸네일 그림을 늘려 보여 주므로 테두리 안 슬라이드 비율이 1px 정도 어긋날 수 있다(다음 다시 그리기에서 맞춰진다). 번호 크기는 바로 다시 그린다.

## Non-goals

- WPF 변경.
- PPT가 아닌 썸네일(이미지 등, `DrawSlideNumber` 경로)의 번호.

## Impact

- `Axicultuss/Forms/ImageCanvas.cs`(`ContentBounds`, `SlideNumberBounds`, `DrawSlideNumberInFrame`).
- `Axicultuss.Tests`(그림 픽셀로 번호 위치 확인).

## Capabilities

### New Capabilities

- `desktop-ppt-thumbnails`: WinForms PPT 썸네일 번호.
