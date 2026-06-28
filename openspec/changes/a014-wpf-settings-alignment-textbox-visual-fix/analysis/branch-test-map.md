# 분기-테스트 맵

## 함수 분기

| 대상 | 분기 | 기존 테스트 | 이번 변경 |
| --- | --- | --- | --- |
| `ApplyLyricsAlignment` | `Left` | `MainViewModelTests`의 정렬 적용 테스트군 | 로직 수정 없음 |
| `ApplyLyricsAlignment` | `Right` | `MainViewModelTests`의 정렬 적용 테스트군 | 로직 수정 없음 |
| `ApplyLyricsAlignment` | 기본/Center | `MainViewModelTests`의 정렬 적용 테스트군 | 로직 수정 없음 |

## 추가할 XAML 회귀 테스트

- 메인 설정 패널의 전역 가사 정렬 버튼 3개가 같은 `UniformGrid` 안에 있고 `Columns=3`인지 확인한다.
- 항목별 `Align / Size`의 `Left`, `Center`, `Right`도 같은 균등 레이아웃 안에 있는지 확인한다.
- 설정 패널 직접 입력 TextBox가 공통 스타일을 사용해 하단 글리프 잘림을 피하도록 확인한다.
- `EsTextBox` 템플릿의 `PART_ContentHost`가 하단 클리핑을 만들기 쉬운 `VerticalAlignment=Center`에 고정되지 않았는지 확인한다.

## 수동 확인

- 앱 실행 후 설정/서식 패널에서 정렬 버튼이 오른쪽으로 밀려 보이지 않는지 확인한다.
- 숫자 입력 TextBox에 `gypq`, `1234567890`, 한글 받침 포함 문자열을 입력해 하단이 잘리지 않는지 확인한다.

