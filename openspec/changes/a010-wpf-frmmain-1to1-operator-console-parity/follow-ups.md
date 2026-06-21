# Follow-Up / Explicit Defer Register

작성일: 2026-06-21

대상: `a010-wpf-frmmain-1to1-operator-console-parity`

근거: `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`, `evidence/manual-uat/2026-06-21/manual-uat-summary.md`

## 원칙

- `FAIL`은 없다.
- `PARTIAL`은 follow-up 검증/구현 과제로 남긴다.
- 로컬 legacy 데이터나 하드웨어 조건이 없어 검증할 수 없는 항목은 explicit defer로 남긴다.
- 이 register는 현재 change의 ship-readiness 판정에서 "숨겨진 실패"를 만들지 않기 위한 추적 표다.

## Follow-Up Tasks

| Group | UAT IDs | Disposition | Reason / Next Action |
| --- | --- | --- | --- |
| Source gesture parity | UAT-103, UAT-104 | Follow-up | 곡 `WL_Add`는 동작하지만 더블클릭/drag insertion gesture 증거가 부족하다. 실제 pointer drag/double-click 수동 UAT 또는 gesture command 경로 보강 필요 |
| Bible gesture parity | UAT-134, UAT-135 | Follow-up | Bible lookup/add는 PASS이나 drag insertion 및 Shift-click multi-select gesture는 별도 검증 필요 |
| Worship List operation parity | UAT-205, UAT-206, UAT-207, UAT-208 | Follow-up | delete/select는 PASS이나 exact one-position diff, drag reorder, context menu Play/Play on Output은 추가 증거 필요 |
| Visual usability parity | UAT-141, UAT-161, UAT-211 | Follow-up | Images thumbnail density, Default first-viewport order/density, Praise Book toolbar/list density가 FrmMain 첫 화면과 아직 다르다 |
| Preview named section jump | UAT-303 | Follow-up | 현재 곡/노출 조건에서 chorus/bridge/ending jump AutomationId가 확인되지 않았다 |
| Output navigation/jump | UAT-402, UAT-404 | Follow-up | Output slide button disabled 조건 및 Output section jump 노출 조건 확인 필요 |
| Output safety external monitor | UAT-406, UAT-407, UAT-408 | Follow-up | operator surface 캡처는 있으나 외부 송출 모니터의 black/clear/hide/restore 시각 결과는 독립 관찰하지 못했다 |
| Live message | UAT-411 | Follow-up | live message input/send/clear AutomationId가 run2 시점에서 탐색되지 않아 실제 send/clear 재확인 필요 |
| Keyboard external/focus parity | UAT-501, UAT-502, UAT-503, UAT-504, UAT-505, UAT-506, UAT-507 | Follow-up | key send 후 crash 없음은 확인했으나 외부 show/black/clear 및 focus-dependent next/previous/section target은 추가 수동 검증 필요 |

## Explicit Defers

| Group | UAT IDs | Deferred Until | Reason |
| --- | --- | --- | --- |
| InfoScreen source folder | UAT-111, UAT-112 | 실제 `C:\EasiSlides\InfoScreens` source 파일 확보 또는 legacy가 참조하는 다른 source 경로 확인 | 현재 직접 폴더 파일 수 0 |
| PowerPoint source config | UAT-121, UAT-122, UAT-123, UAT-124, UAT-306, UAT-405 | `UsePowerPointTab=true` 및 source 경로 재검증 | 운영 PPT는 `D:\예배자료` 링크 대상에 존재하지만 현재 `UsePowerPointTab=false`, `C:\EasiSlides\Powerpoint` 직접 폴더 파일 수 0 |
| Media source config | UAT-151, UAT-152 | `UseMediaTab=true` 및 source 경로 재검증 | 운영 Media는 `D:\예배자료` 링크 대상에 존재하지만 현재 `UseMediaTab=false`, `C:\EasiSlides\Media` 직접 폴더 파일 수 0 |
| PraiseBook selection | UAT-212, UAT-213 | `주일예배.esp` 등 실제 항목이 있는 책을 선택해 add/delete/export UAT 재수행 | `C:\EasiSlides\Admin\PraiseBooks`에 `.esp` 2개 존재. 현재 선택값 `PraiseBook 1`은 빈/헤더 중심 책으로 관찰됨 |

## Phase 9 Registry Runtime Sync Update

`HKCU\Software\EasiSlides` 재검토 결과, WPF의 기존 설정 파일이 있으면 registry-backed runtime settings가 재반영되지 않는 코드 원인이 확인되어 수정했다.

- `current_praisebook`, `current_session`, `media_dir`, `UsePowerpointTab`, `UseMediaTab`는 다음 WPF 시작부터 레지스트리 기준으로 재동기화된다.
- 현재 레지스트리 값 자체가 `UsePowerpointTab=0`, `UseMediaTab=0`이면 PowerPoint/Media tab 숨김은 FrmMain과 동일한 현재 설정 상태다.
- PraiseBook은 `current_praisebook=PraiseBook 1`이 재동기화된다. `PraiseBook 1`이 실제 항목이 없는 책이면 UAT-212/UAT-213은 `주일예배.esp` 등 항목이 있는 책으로 선택을 바꾼 뒤 재수행해야 한다.

## Phase 10 Deployment Update

WPF Release 산출물을 `C:\EasiSlides\EasislidesNext`에 배포하고 기존 WinForms `C:\EasiSlides\Easislides.exe`와 같은 운영 루트에서 실행 검증했다.

- 배포 실행 기준으로 registry/settings/data read parity는 PASS다.
- WPF는 `CurrentPraiseBookName=PraiseBook 1`을 정상 반영하고 `찬양집 열림: PraiseBook 1 (0곡)`을 표시했다.
- 따라서 UAT-212/UAT-213의 남은 조건은 WPF 설정 미반영이 아니라, 항목이 있는 PraiseBook을 선택한 뒤 add/delete/export gesture를 실제 수행하는 manual UAT다.

## Completion Mapping

- PASS 25개는 current change evidence로 수락한다.
- PARTIAL 25개는 위 Follow-Up Tasks로 전환했다.
- BLOCKED 12개는 위 Explicit Defers로 전환했다.
- FAIL 0개이므로 별도 bug task는 없다.

## Phase 8 Closure Result

2026-06-21 재확인 결과:

- 코드 보정 완료: UAT-141, UAT-161, UAT-211.
- 구현/테스트 증거로 닫음: UAT-103, UAT-104, UAT-134, UAT-135, UAT-205, UAT-206, UAT-207, UAT-208, UAT-303, UAT-402, UAT-404, UAT-411, UAT-501, UAT-502, UAT-503, UAT-504, UAT-505, UAT-506, UAT-507.
- 외부 송출 모니터 관찰 필요: UAT-406, UAT-407, UAT-408.
- source 폴더/설정/선택 상태 재검증 필요: BLOCKED 12개 유지.

상세 근거: `evidence/manual-uat/2026-06-21/phase8-partial-blocked-closure.md`.
