# 분기-테스트 맵

## 분기 커버리지

| 분기 | 기존 테스트 | 공백 | 필요한 결과 |
| --- | --- | --- | --- |
| 선택 항목 null | ViewModel setup의 Command CanExecute 경계 | 이번 버그에는 별도 UI 테스트 불필요 | 선택 항목이 없으면 명령 비활성 |
| 선택 항목이 큐 안에 있음 | `ToggleUseIndividualFormatting_FlipsFlagOnSelectedItem` | 직접 명령 실행만 커버 | 큐 항목과 선택 항목의 플래그가 함께 토글 |
| 라이브 중 다중 절 항목 | `ToggleUseIndividualFormatting_LiveMultiVerse_PreservesCurrentVerse` | 커버됨 | 라이브 절 위치가 튀지 않음 |
| 화면 체크박스 클릭 | 일부 XAML literal 테스트만 존재 | 실제 상호작용 경로는 별도 수동 확인 필요 | 화면 체크박스로 켜기/끄기가 가능 |
| XAML checked 상태 동기화 | `MainMenuBarTests`의 일부 literal 확인 | 상호작용 assertion 부족 | `Ind_checkBox`는 선택 항목 상태와 동기화 |
| 본문 있는 `Notice`/텍스트 항목 | 수정 전 없음 | 핵심 공백 | `CanEditSelectedItemColor`가 true이고 개별 설정 컨트롤이 실제 사용 가능 |
| 본문 없는 `Notice`/텍스트 항목 | 수정 전 없음 | 후속 추가 가능 | 별도 설계 전까지 false 유지 |
| PowerPoint 같은 비텍스트 항목 | 기존에는 `Notice`를 음수 fixture로 사용 | fixture 의미가 부정확 | `PowerPoint`로 음수 테스트를 유지 |

## 새로 필요한 테스트

- 본문이 있는 `Notice`/텍스트 파일 항목에서 `CanEditSelectedItemColor`와 대표 개별 설정 명령이 활성화되는 ViewModel 회귀 테스트.
- 비텍스트 시각 항목이 계속 서식 불가임을 보장하는 기존 음수 테스트 fixture 정리.

## 유지할 회귀 테스트

- `ToggleUseIndividualFormatting_FlipsFlagOnSelectedItem`
- `ToggleUseIndividualFormatting_LiveMultiVerse_PreservesCurrentVerse`
- `UseIndividualFormattingOff_LiveProjectionDropsPerSongColor`
