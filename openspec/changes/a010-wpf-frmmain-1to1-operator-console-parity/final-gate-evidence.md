# Final Gate Evidence

작성일: 2026-06-21

대상 change: `a010-wpf-frmmain-1to1-operator-console-parity`

## Gate Summary

| Gate | Result | Evidence |
| --- | --- | --- |
| OpenSpec strict validation | PASS | `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict` -> `Change is valid` |
| WPF tests | PASS | `dotnet test Easislides.Wpf.Tests -v minimal` -> 실패 0, 통과 2417, 건너뜀 0 |
| WinForms build | PASS | `dotnet build Easislides\Easislides.csproj -nologo -v minimal` -> 오류 0, 경고 13 |
| WPF launch with legacy data | PASS | `EasislidesNext.exe` 실행 및 `C:\EasiSlides` 데이터 로딩 확인 |
| Actual manual UAT | RECORDED | 62개 row 모두 판정 기록: PASS 25, PARTIAL 25, BLOCKED 12, FAIL 0 |
| PARTIAL/BLOCKED disposition | RECORDED | `follow-ups.md`에 Follow-Up Tasks 및 Explicit Defers로 전환 |
| Phase 8 PARTIAL/BLOCKED closure | RECORDED | PARTIAL 25개 재확인: 코드 보정 3, 구현/테스트 증거로 닫음 19, 외부 송출 모니터 관찰 필요 3. BLOCKED 12개는 외부 데이터/설정 필요로 유지 |
| Phase 8 focused tests | PASS | `LegacyImageThumbnailSizeConverterTests|MainMenuBarTests`: 실패 0, 통과 137, 건너뜀 0 |
| Phase 8 full WPF tests | PASS | `dotnet test Easislides.Wpf.Tests -v minimal`: 실패 0, 통과 2421, 건너뜀 0 |
| Phase 8 OpenSpec validation | PASS | `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`: Change is valid |
| Phase 8 WinForms build | PASS | `dotnet build Easislides\Easislides.csproj -nologo -v minimal`: 오류 0, 경고 13 |

## Manual UAT Evidence

- Checklist: `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`
- Summary: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/manual-uat-summary.md`
- Screenshots: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/`
- Follow-up/defer register: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/follow-ups.md`

## Known Non-Ship Items

이 gate는 "검증 증거가 기록되었다"는 의미이며, 모든 parity gap이 닫혔다는 의미는 아니다.

- Gesture parity follow-up: song double-click/drag, Bible drag/Shift-click, Worship List drag/context menu.
- Visual usability follow-up: Images thumbnail density, Default first-viewport density/order, Praise Book toolbar/list density.
- External output hardware follow-up: black/clear/hide/restore 및 shortcut 결과의 실제 송출 모니터 확인.
- Data/config defers: InfoScreen, PowerPoint, Media, PraiseBook entries.

## Warning Notes

- `dotnet test`와 `dotnet build` 모두 기존 `NU1701` NetOffice compatibility warning을 포함한다.
- WinForms build는 기존 `NETSDK1137` WindowsDesktop SDK warning을 포함한다.
- 위 warning들은 이번 WPF MainWindow manual UAT 문서화 작업에서 새로 도입한 오류가 아니며 build/test gate를 실패시키지 않았다.
