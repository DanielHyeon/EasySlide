## Context

WinForms `FrmMain`에서 Live는 예배 운영자가 가장 자주 쓰는 즉시 송출 흐름이다. `btnToLive_Click`은 Preview 항목을 Output으로 복사한 뒤 쇼가 꺼져 있으면 `GoLive(true)`를 호출하고, `cbGoLive_Click`은 `GoLive(cbGoLive.Checked)`로 현재 Output/첫 WorshipList 항목을 실제 송출한다.

WPF MainWindow는 최근 상태 표시와 전체 화면/PPT 경로를 보정했지만, 사용자 확인 결과 Live 버튼을 눌러도 실제 송출이 시작되지 않는다. 이번 change는 이전 파리티 change와 분리해 실제 Live 불능 원인을 좁히고, 텍스트와 PPT 모두에서 WinForms와 같은 결과를 보장한다.

## Goals / Non-Goals

**Goals:**

- Preview 항목 선택 후 Live 버튼을 누르면 출력 창이 자동 준비되고 실제 텍스트/PPT 송출이 시작된다.
- Output 영역의 Live 버튼도 WinForms `cbGoLive`처럼 현재 Output 항목 또는 WorshipList 첫 항목을 실제 송출한다.
- Live 시작 경로에서 사용자 확인 메시지나 빨간 상단 상태 라인을 만들지 않는다.
- PPT 항목은 PowerPoint SlideShow가 선택된 출력 모니터에 시작되도록 기존 Phase 21 경로를 재검증한다.
- 회귀 테스트와 실제 배포 검증으로 “버튼 상태만 Live”가 아니라 “출력 surface가 갱신됨”을 확인한다.

**Non-Goals:**

- PowerPoint 썸네일 생성 성능 개선은 이번 범위가 아니다.
- WinForms 코드 리팩터링, `gf` 명명 변경, DB 스키마 변경은 포함하지 않는다.
- 전체 MainWindow UI 파리티 재정렬은 기존 `a010` 범위에 남긴다.

## Decisions

- Live 불능은 독립 change `a011`에서 다룬다.
  - 이유: `a010`은 UI/UX 1:1 파리티의 큰 change이고 이미 많은 Phase가 누적되어 있다. Live 송출 불능은 예배 운영 핵심 결함이므로 별도 계약과 검증 증거가 필요하다.

- WinForms 흐름을 제품 기준으로 둔다.
  - 이유: 사용자는 실제 WinForms와 WPF를 같은 데이터/설정으로 비교하고 있다. 따라서 WPF 내부 구조보다 `FrmMain`의 버튼 클릭 결과가 우선이다.

- 먼저 실패 테스트로 “출력 surface 갱신”을 고정한다.
  - 이유: 이전 수정은 명령 실행 가능 여부와 상태바 표시를 보정했지만, 사용자는 실제 Live가 되지 않는다고 보고했다. 이번 테스트는 상태 텍스트가 아니라 `OutputWindowViewModel`/PPT 시작 요청까지 확인해야 한다.

- PPT 경로는 새 Interop 추상화를 만들지 않고 기존 `OfficePptSession`/PowerPoint service를 보정한다.
  - 이유: Office COM 경로는 회귀 위험이 높다. WinForms와 동일한 모니터 지정/슬라이드쇼 실행 결과만 최소 수정한다.

## Risks / Trade-offs

- [Risk] 테스트 환경에서 실제 PowerPoint/멀티모니터를 완전히 재현하기 어렵다. → `IPowerPointShowService` 호출 테스트와 실제 `C:\EasiSlides` 배포 후 수동 Live/PPT UAT를 함께 남긴다.
- [Risk] 출력 창이 열려도 내용 바인딩이 갱신되지 않으면 버튼 상태만 Live가 될 수 있다. → Live 시작 테스트에서 `OutputWindowViewModel.CurrentItem` 및 rendered payload 갱신을 확인한다.
- [Risk] PPT 시작 요청이 발생해도 PowerPoint 창이 잘못된 모니터에 뜰 수 있다. → `OutputDisplay.Id` 기반 모니터 전달 테스트와 수동 모니터 검증을 분리한다.
- [Risk] 메시지 박스 제거가 실패 상황 진단을 어렵게 만들 수 있다. → 실패는 상태바/로그 경로로 남기되 Live 정상 경로에는 확인 대화상자를 두지 않는다.
