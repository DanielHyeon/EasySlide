# 설계

## 범위

이번 변경은 WPF `MainWindow.xaml`의 설정 패널 시각 스타일에 한정한다.

- 설정 전용 콤보박스 스타일을 추가한다.
- `Def_TransItem`, `Def_TransSlides`, `Ind_TransItem`, `Ind_TransSlides`에 설정 전용 콤보박스 스타일을 적용한다.
- `Image` 및 같은 행의 `Media` 경로 텍스트 박스에 `ClassicCompactSettingsTextBox`를 적용해 같은 글꼴 높이 기준을 사용한다.

## 위험

- 콤보박스 높이를 키우면 설정 패널의 세로 밀도가 조금 낮아질 수 있다.
- 설정 패널은 스크롤 가능한 영역이므로 잘림 방지가 밀도보다 우선이다.

## 검증

- XAML 구조 테스트로 스타일 값과 적용 대상을 고정한다.
- WPF 렌더 하네스로 Image/Transition 샘플을 캡쳐한다.
- focused 테스트, WPF 빌드, OpenSpec strict 검증, Release publish, 배포본 실행 smoke를 수행한다.
