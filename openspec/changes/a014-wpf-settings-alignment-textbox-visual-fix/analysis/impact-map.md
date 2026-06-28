# 영향 맵

## CodeGraph 증거

### `SettingsWindowViewModel`

- `codegraph_impact("SettingsWindowViewModel", depth: 2)` 결과: 설정 ViewModel의 속성·명령·섹션 생성·설정 저장 함수 등 132개 심볼이 영향권으로 표시됐다.
- 이번 변경은 ViewModel 내부 값을 변경하지 않고 XAML 배치/스타일만 다루므로 이 넓은 영향권은 주의 대상으로만 기록한다.
- `codegraph_callers("SettingsWindowViewModel")` 결과: 정적 호출자는 없음. DI/XAML 생성 경로는 CodeGraph가 정적으로 연결하지 못할 수 있다.

### `LyricsTextAlignment`

- `codegraph_impact("LyricsTextAlignment", depth: 2)` 결과: enum 및 멤버 3개가 영향권으로 표시됐다.
- 정렬 값 자체는 변경하지 않는다.

### `CreateLyricsSampleTextAlignment`

- `codegraph_callers("CreateLyricsSampleTextAlignment")` 결과: 정적 호출자 없음.
- 이번 요청의 핵심은 정렬 명령 로직이 아니라 설정 UI 컨트롤 위치 문제로 판단한다.

## 파일 영향

- `Easislides.Wpf/Controls/EsTextBox.xaml`: 공통 TextBox 템플릿. 별도 설정 창과 컴포지트 입력에 영향.
- `Easislides.Wpf/MainWindow.xaml`: 메인 설정/서식 패널의 정렬 버튼, 직접 입력 TextBox.
- `Easislides.Wpf.Tests`: XAML 구조 회귀 테스트 추가.

## 테스트 영향

- XAML 구조 테스트로 스타일 적용과 균등 배치를 검증한다.
- ViewModel/렌더링 테스트는 정렬 로직을 바꾸지 않는 한 직접 추가하지 않는다.

