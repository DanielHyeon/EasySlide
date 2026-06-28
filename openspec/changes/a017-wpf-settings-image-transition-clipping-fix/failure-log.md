# 실패 기록

## 2026-06-23 Image/Transition 설정 컨트롤 잘림

### 증상

선택 항목 설정의 Image 경로 텍스트와 Transition 영역 콤보박스 텍스트가 하단에서 잘려 보인다.

### 원인

- Image/Media 경로 텍스트 박스가 `Height=26`으로 고정되어 `ClassicCompactSettingsTextBox`보다 작다.
- Transition 콤보박스가 `Height=26` 또는 `Height=30`으로 고정되어 있고, 설정 전용 수직 패딩 스타일이 없다.

### 수정

- `ClassicSettingsComboBox` 스타일을 추가했다.
- `Def_TransItem`, `Def_TransSlides`, `Ind_TransItem`, `Ind_TransSlides`에서 고정 `Height`를 제거하고 설정 전용 콤보박스 스타일을 적용했다.
- `SelectedItemBackgroundImagePath`, `SelectedItemMediaPath` 텍스트 박스에서 고정 `Height=26`을 제거하고 `ClassicCompactSettingsTextBox`를 적용했다.

### 예방

XAML 구조 테스트와 렌더 캡쳐 테스트로 설정 패널의 compact 텍스트 박스와 콤보박스 높이 기준을 고정한다.
