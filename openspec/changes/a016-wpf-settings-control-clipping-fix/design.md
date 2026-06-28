# 설계

## 변경 방향

- `ClassicSettingsTextBox`의 `MinHeight`를 올리고 세로 `Padding`을 늘린다.
- `ClassicCompactSettingsTextBox`도 기존보다 높이를 키워 숫자 입력 글자 하단이 잘리지 않게 한다.
- 설정 패널용 `ClassicSettingsCheckBox` 스타일을 추가해 `MinHeight`, `Padding`, `VerticalContentAlignment`를 명시한다.
- `Ind_checkBox`에 새 체크박스 스타일을 적용한다.

## 테스트

- `SettingsVisualLayoutTests`에 텍스트 박스 스타일의 최소 높이/패딩 회귀 테스트를 추가한다.
- `Ind_checkBox`가 설정 체크박스 스타일을 사용하는지 테스트한다.

## 위험

- 컨트롤 높이가 커져 패널 안에 보이는 항목 수가 줄 수 있다.
- 그러나 잘림 없는 운영 가독성이 더 중요하므로 최소 수정으로 처리한다.
