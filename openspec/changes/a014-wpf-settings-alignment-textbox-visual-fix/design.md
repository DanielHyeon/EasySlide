# 설계

## 접근

문제는 두 층으로 나누어 처리한다.

1. 공통 TextBox 템플릿
   - `EsTextBox`의 `PART_ContentHost`가 내부 여백을 margin으로 받고 `VerticalAlignment=Center`와 숨김 스크롤바를 함께 쓰면 특정 글꼴에서 하단 글리프가 잘릴 수 있다.
   - 콘텐츠 호스트는 stretch 배치를 사용하고 TextBox 자체의 최소 높이/패딩으로 여백을 확보한다.

2. 메인 설정/서식 패널
   - 전역 가사 정렬과 항목별 정렬 버튼은 `StackPanel`/`WrapPanel`의 자연 폭에 의존한다.
   - 설정 패널 폭이 좁거나 버튼 텍스트 길이가 섞이면 `Left`, `Center`, `Right`가 같은 기준으로 보이지 않는다.
   - 3열 `UniformGrid` 또는 동일 폭 버튼 스타일로 정렬 버튼을 균등 배치한다.
   - 설정 패널의 직접 입력 `TextBox`에는 공통 입력 스타일을 적용해 최소 높이와 내부 여백을 통일한다.

## 변경 대상

- `Easislides.Wpf/Controls/EsTextBox.xaml`
- `Easislides.Wpf/MainWindow.xaml`
- XAML 구조 테스트 파일 1개 추가 또는 기존 XAML 테스트 보강

## 위험

- TextBox 공통 템플릿 수정은 별도 설정 창과 컴포지트 입력 상자에도 영향을 준다.
- 기존 compact 레거시 레이아웃은 높이가 제한된 영역이 있으므로, 메인 설정 패널에만 별도 스타일을 적용하고 좁은 레거시 툴바 TextBox는 필요한 곳만 선별한다.
- 정렬 명령 바인딩은 그대로 유지해야 하며, 버튼 레이아웃 변경으로 `CommandParameter`가 바뀌면 안 된다.

