전제: a046이 끝나 있다(공용 라이브러리와 확정된 호스트 인터페이스).

## Phase 0. 기준, 영향, 하네스

- Goal: 실제 데이터를 건드리지 않고 Axicultuss를 실행해 볼 수단을 만든다. 제품 코드는 바꾸지 않는다.
- [ ] 0.1 브랜치 `feat/a047-winforms-camera-captions`. `Axicultuss` Debug 빌드, `Axicultuss.Tests` 전체 결과 기록
- [ ] 0.2 design 1의 사실을 지금 코드와 대조(줄 번호 갱신). a046의 호스트 인터페이스에 모자란 것이 있으면 목록으로 보고(고치는 것은 총괄이 결정)
- [ ] 0.3 `tools\AxicultussHarness`: 격리 사본 스크립트, 실행 스크립트(보호 검사), 드라이버, `smoke` 단계, README
- Tests: 보호 검사가 조건마다 실행을 막는지(모니터 1대는 사람이 확인하기 어려우므로 스크립트 조건을 코드로 읽어 검토), `smoke` 실행 뒤 실제 폴더 해시 unchanged
- DoD: `smoke`가 격리 사본에서 통과하고 로그에 처리되지 않은 예외가 없다. 실행 전 사용자에게 창이 뜬다고 알렸다.
- Constraints: 보호 검사를 풀거나 우회하지 않는다. 실제 앱(`C:\Axicultuss\Axicultuss.exe`)을 실행하지 않는다. 실행 중인 Axicultuss가 있으면 멈추고 보고한다.

## Phase 1. 호스트 연결부 (상태 읽기)

- Goal: 송출 상태를 읽어 호스트 인터페이스로 내놓는다. 화면은 아직 없다.
- [ ] 1.1 실패하는 테스트 먼저: `SongSettings`로 만든 항목에서 상태 묶음 만들기(쇼가 돌 때는 LiveItem, 아니면 OutputItem; 종류 맞춤; 지금 글과 페이지 목록; 검은 화면; PPT 경로와 장; `Slide[,]`를 바꾸지 않음), 달라졌을 때만 알림
- [ ] 1.2 `Axicultuss.csproj`: `UseWPF`, `Microsoft.AspNetCore.App`, `Axicultus.Captions` 참조. 기존 빌드와 테스트가 그대로인지
- [ ] 1.3 연결부 구현(design 2.1~2.3, 2.9, 2.11): 상태 읽기 순수 함수, 타이머, 넘기기·올리기, 곡 목록, 작은 장 그림, 저장 폴더
- Tests: 1.1, 저장 폴더 규칙(`AXICULTUSS_SETTINGS_FILE`이 있을 때와 없을 때), 올리기는 쇼가 돌면 하지 않음
- DoD: `Axicultuss` Debug·Release 빌드 오류 0, `Axicultuss.Tests`·`Axicultus.Tests` 전체 통과, 쇼 창·송출 로직 파일에 변경 없음(`git diff --stat`로 확인)
- Constraints: 식별자·주석 영어. DB 접근은 `SQLiteController`·`gfDatabase` 경유. 새 메서드는 알맞은 `Global\gf*.cs`나 새 파일(연결부)로.

## Phase 2. 본창의 탭과 키

- Goal: 자막 탭이 본창에 보이고 조작된다.
- [ ] 2.1 `FrmMain.Captions.cs`: 탭 줄과 자막 칸 놓기, 탭 전환, 새 UI 층과 맞추기(design 2.4, 2.5, 2.8)
- [ ] 2.2 출력 메뉴 항목 둘과 F2·F6, 메뉴 글(한국어·영어)(design 2.6)
- [ ] 2.3 설정 창 키보드 연동, 닫을 때 끝내기, 예배 순서가 바뀌면 다시 만들기, 실패해도 본창은 뜸(design 2.7, 2.12, 5)
- [ ] 2.4 OCR 도우미 복사, "카메라" 내보낼 곳 숨기기, 옛 .ppt 안내
- [ ] 2.5 하네스 `captiontab` 단계와 그림
- Tests: 하네스 `captiontab`(탭 줄이 시작부터 보임, F2 전환, 고른 항목이 송출 칸에 올라가고 자막이 준비됨, F6으로 자막이 나감, 페이지 단계, 다른 예배 순서, 닫기), 본창 최소 크기와 저장된 분할 값에서의 그림, 기존 키(F3·F5·F9·F11·F12, 송출 칸의 Space·화살표)가 그대로인지
- DoD: 하네스 통과, 실제 폴더 unchanged, Debug·Release 빌드(저장소 bin), 전체 테스트 통과, `FrmMain.Designer.cs`에 변경 없음
- Constraints: 기존 클릭·더블클릭·끌어 놓기·단축키를 바꾸지 않는다.

## Phase 3. 회귀와 배포 준비

- [ ] 3.1 Debug·Release 빌드, `Axicultuss.Tests`·`Axicultus.Tests` 전체, 하네스 `smoke`·`captiontab`, WPF 하네스 `captiontab`
- [ ] 3.2 긴 본문(성경 여러 장)에서 상태 읽기 비용 재기(하네스 로그)
- [ ] 3.3 배포 안내: 늘어나는 파일, ASP.NET Core 런타임 확인 방법, 되돌리는 방법. 방송 PC 배포는 사용자가 한다

## Phase 4. 리뷰와 기록

- [ ] 4.1 묶음(Phase 0, 1, 2)마다 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [ ] 4.2 묶음마다 `/code-review`
- [ ] 4.3 총괄 완료 검증(그림 포함)과 "검증 결과" 기록. 사람이 확인할 것 목록: 한글 조합 입력, 실제 OBS 연결, 듀얼 모니터 쇼에서의 따라가기
- [ ] 4.4 커밋·푸시, 아카이브(`openspec/changes/archive/<날짜>-a047-winforms-camera-captions`, 새 `openspec/specs/winforms-camera-captions/spec.md`)

## 검증 결과

(구현 뒤 기록)
