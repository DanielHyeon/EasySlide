# 실패 기록

## 2026-06-23 사도신경 Live Show 배경 불일치

### 증상

왼쪽 미리보기에는 사도신경 배경이 보이지만 Live Show 출력은 default 배경을 사용한다.

### 현재 가설

미리보기 샘플은 `FormatData`를 직접 읽고, Live 송출은 `UseIndividualFormatting=false`일 때 `FormatData`를 null 처리한다. 따라서 체크박스 상태와 샘플 렌더 규칙이 서로 다르면 왼쪽/오른쪽이 다르게 보인다.

### 예방

미리보기 샘플과 Live 송출은 동일한 유효 서식 helper를 사용해야 한다. `UseIndividualFormatting=false` 분기에 대한 회귀 테스트를 추가한다.

## 2026-06-23 실제 앱 캡처 제한

### 증상

새 빌드 실행 후 UI Automation 트리에는 메뉴, 예배 순서, 사도신경 항목, `Preview Go Live` 버튼이 존재하지만, `CopyFromScreen`과 `PrintWindow` 캡처는 흰 창으로만 저장되었다.

### 확인된 사실

- 앱 프로세스는 응답 상태였다.
- UIA로 사도신경 DataItem을 찾을 수 있었다.
- UIA로 `Preview Go Live` 버튼을 Invoke할 수 있었다.
- 실행 후 UIA 트리에서 `LIVE: ★★★사도신경★★★.txt`, `LIVE 1/10`이 확인되었다.

### 영향

자동 테스트와 UIA 상태로 송출 상태 전환은 확인했지만, 사용자가 요구한 “배경이 실제 화면에서 동일하게 보이는지”에 대한 시각 캡처 검증은 이 세션에서 완료하지 못했다.

### 다음 조치

정상 렌더링 캡처가 가능한 환경에서 동일 절차를 반복하거나, WPF 렌더가 흰 창으로 보이는 별도 문제를 먼저 해결한다.
