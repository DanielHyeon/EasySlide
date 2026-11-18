# CodeGraph 영향

기준은 `feat/frmmain-modern-ui`의 `a8e0c3d`이다. 로컬 CodeGraph에서 `SizeLaunchDisplay`, `ResizeSampleScreen`, `SaveVariables`, `LoadSavedData`, `LyricsMonitor_Info_Click`, `BuildNewImageThumbs`, `InitForm`, `SetShowWindow`를 조회했다. 인덱스가 최신 줄과 일부 호출을 포함하지 않아 작업 트리에서 교차 확인했다.

- 설정 읽기·쓰기와 옵션 저장은 `gfDatabase`, `gfUtility`, `FrmOptions`를 거친다. 실제 배치는 `DisplayInfo` 결과를 `FrmMain` 시작 경로와 `FrmLaunchShow`, PPT·미디어 경로가 소비한다.
- `ResizeSampleScreen`은 운영자 미리보기와 출력 패널에 공통으로 적용된다. `ImageCanvas`는 PPT 내보내기 및 외부 미리보기 캐시의 이미지 크기를 사용한다.
- 기존 가사 창의 표시·업데이트·정리 호출을 `FrmLaunchShow` 수명에 연결한다. 가사 표시를 위한 데이터 읽기는 기존 Gf 경로를 쓴다.
