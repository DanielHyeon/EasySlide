# 실패 기록

## 2026-06-23 설정 패널 컨트롤 내용 잘림

### 증상

설정 패널의 텍스트 박스와 체크박스 내용이 하단에서 잘려 보인다.

### 원인

- `ClassicSettingsTextBox`의 최소 높이와 세로 패딩이 실제 글꼴 렌더링보다 타이트했다.
- `ClassicCompactSettingsTextBox`는 고정 높이와 패딩이 더 작아 숫자/문자 하단 획이 잘릴 위험이 컸다.
- `Use Individual Settings` 체크박스에는 설정 패널 전용 높이/패딩/수직 정렬 스타일이 없었다.

### 수정

- 설정 텍스트 박스 기본 스타일을 `MinHeight=36`, `Padding=8,6`, `VerticalContentAlignment=Center`로 조정했다.
- compact 설정 텍스트 박스 스타일을 `Height=34`, `MinHeight=34`, `Padding=7,4`로 조정했다.
- `ClassicSettingsCheckBox` 스타일을 추가하고 `Ind_checkBox`에 적용했다.

### 검증

- 구조 테스트로 스타일 값과 `Ind_checkBox` 스타일 적용을 고정했다.
- 렌더 캡쳐 테스트로 텍스트 박스/체크박스 샘플을 PNG로 생성했다.
- 캡쳐 확인 결과 `g/j/p/q/y` 하단 획이 잘리지 않았다.

## 2026-06-23 실제 앱 창 캡쳐 제약

### 증상

`EasislidesNext.exe`는 실행되고 `EasiSlides` 창 핸들도 생성됐지만, `PrintWindow`와 화면 픽셀 복사 캡쳐 모두 타이틀바와 흰 본문만 기록했다.

### 영향

이번 변경의 설정 컨트롤 잘림 여부는 WPF 렌더 하네스 캡쳐로 확인했지만, 실제 운영 창의 설정 패널 캡쳐는 확보하지 못했다.

### 후속

실제 앱 본문이 자동 실행 환경에서 흰 화면으로 남는 원인은 이번 컨트롤 스타일 변경과 별도로 조사해야 한다.
