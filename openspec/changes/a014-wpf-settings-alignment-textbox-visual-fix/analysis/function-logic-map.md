# 함수 로직 맵

## 대상 함수

- `MainViewModel.ApplyLyricsAlignment(LyricsTextAlignment alignment)`

## tree-sitter 요약 기준

- `function-ast-summary.md`는 `ApplyLyricsAlignment` 내부를 기계 추출했다.
- 분기: `alignment switch`로 상태 메시지 문구를 `왼쪽`/`오른쪽`/`가운데`로 선택한다.
- side effect: `_settings.Set(...)`, `ActiveLyricsAlignment = alignment`, `StatusText = ...`.

## 이번 변경의 관계

- 정렬 버튼은 이 함수를 호출하지만, 이번 변경은 버튼의 배치와 TextBox 시각 스타일만 수정한다.
- `ApplyLyricsAlignment` 내부 로직, 저장 키, 상태 메시지, enum 매핑은 수정 금지다.
- Live Show 출력 정렬과 preview/output sample 정렬 계산 함수는 수정 금지다.

## 수정 가능 영역

- XAML 컨테이너: 전역 가사 정렬 버튼의 부모 레이아웃.
- XAML 컨테이너: 항목별 정렬 버튼의 부모 레이아웃.
- XAML 스타일: 설정 패널 직접 입력 TextBox 스타일.
- XAML 스타일: `EsTextBox` 콘텐츠 호스트 배치.

## 불변식

- `LyricsTextAlignment.Left/Center/Right` CommandParameter는 그대로 유지한다.
- `SetSelectedItemAlignmentCommand`의 `"1"`, `"2"`, `"3"` 매핑은 그대로 유지한다.
- 선택 항목별 정렬과 전역 정렬은 서로 다른 명령을 계속 사용한다.
- 항목별 기본값 버튼은 유지한다.

