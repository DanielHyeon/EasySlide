# ADR-0005: FrmOptions 단일 모달 → Settings 페이지 분해

* **상태**: Accepted
* **결정일**: 2026-05-27
* **결정자**: 프로젝트 메인테이너 + UI/UX 현대화 리뷰
* **태그**: ux, settings, modal, decomposition, navigation
* **관련**: 계획서 §6.4 · ADR-0001 (WPF UI 라이브러리 NavigationView)

## 컨텍스트

현재 `Easislides/Easislides/FrmOptions.cs` + `FrmOptions.Designer.cs`:
- **491 디자이너 심볼** — `FrmMain` 다음으로 큰 폼
- 단일 거대 모달 다이얼로그 (`ShowDialog()`)
- 다중 `TabPage` 안에 모니터·송출, 폰트·색, 데이터베이스, 단축키, 미디어, 임포트/익스포트 등 8+개 카테고리 혼재
- 변경하려면 다른 모든 작업 중단 후 모달 → 변경 → 닫기 (작업 흐름 단절)
- 같은 다이얼로그에 매우 다른 성격의 설정 (시각 디자인 ↔ 시스템 경로) 공존 → 인지 부하

UX 마찰 (계획서 §2.5):
- 모달 남용 — `ShowDialog()` 63회 중 FrmOptions가 하나
- 정보 밀도 — 한 화면에 8 카테고리 × 평균 10+ 설정 = 80+ 컨트롤
- 검색 불가 — 어디에 무슨 옵션이 있는지 사용자가 매번 탭 클릭하며 찾음

## 고려한 대안

### A. Settings 페이지 패턴 (Sidebar + 콘텐츠, Windows 11 설정 스타일) — **채택**

WPF UI의 `Ui:NavigationView` 활용. 좌측 사이드바(8 카테고리) + 우측 콘텐츠 영역.

```
┌────────────────────────────────────────────────────┐
│ 설정                                  [검색...]    │
├─────────────┬──────────────────────────────────────┤
│ ● 일반       │  일반 설정                          │
│   외관       │  ──────────────────                 │
│   모니터 송출│  □ 시작 시 마지막 예배 자동 열기    │
│   단축키     │  □ 자동 백업 (매 5분)               │
│   데이터     │  언어: [한국어 ▼]                   │
│   미디어     │  인터페이스 크기: [표준 ▼]          │
│   가져오기   │     ↑ ADR-0006 시니어 모드 토글     │
│   고급       │  ...                                │
└─────────────┴──────────────────────────────────────┘
```

| 장점 | 단점 |
|---|---|
| **Windows 11 시스템 설정과 동일 멘탈 모델** — 사용자 학습 비용 0 | 마이그레이션 복잡 — 단일 폼 → 8 페이지로 분해 |
| **검색** — 카테고리 가로지르는 즉시 검색 | — |
| **딥링크** — 다른 폼에서 "단축키 설정 열기"가 특정 페이지 직접 호출 가능 | — |
| **모달 아님** — `Ui:NavigationView`는 별도 창 또는 인라인 호스트, 다른 작업 병행 가능 | — |
| 페이지별 ViewModel 분리 → 단위 테스트 용이 | — |

### B. Tab 다이얼로그 유지 + 정리

| 장점 | 단점 |
|---|---|
| 변경 폭 최소 — 기존 구조 유지 | 491심볼 → 어떻게든 줄어도 거대 모달 본질 유지 |
| 마이그레이션 비용 낮음 | UX 마찰 5개 중 "한 화면 정보 밀도", "검색 불가", "딥링크 불가" 해결 안 됨 |
| — | Windows 11 표준 패턴에서 벗어남 |

### C. 마법사(Wizard) 분리

| 장점 | 단점 |
|---|---|
| 카테고리별 별도 마법사 — 단순 | 자주 쓰는 설정 변경(예: 모니터 토글)에도 마법사 진입 단계 거쳐야 함 |
| — | 설정은 "흐름"이 아니라 "검색" — 마법사는 부적합 |

## 결정

**A. Settings 페이지 패턴.**

세부 구조 (계획서 §6.4 확정):

| # | 카테고리 | 흡수 대상 (Legacy) |
|---|---|---|
| 1 | 일반 (General) | 시작 동작, 언어, 인터페이스 크기 (ADR-0006) |
| 2 | 외관 (Appearance) | 폰트·색·테마(라이트/다크), Mica(Q3) |
| 3 | 모니터·송출 (Display & Output) | `FrmInfoScreen` 264심볼 일부 흡수 — 단순 송출 옵션만, 복잡한 멀티모니터 매핑은 별도 `FrmInfoScreen` (`MonitorMapper`) 유지 |
| 4 | 단축키 (Shortcuts) | 후킹 매핑 시각화 + 충돌 검사 (ADR-0004 ShortcutRegistry UI) |
| 5 | 데이터 (Data) | SQLite/MariaDB 경로, 백업, 마이그레이션 도구(§10.5) 진입점 |
| 6 | 미디어 (Media) | 미디어 폴더 경로, 코덱 옵션, 미리보기 캐시 |
| 7 | 가져오기/내보내기 (Import/Export) | 외부 형식 매핑 |
| 8 | 고급 (Advanced) | 진단 로그, 개발자 옵션, 레지스트리 정리 |

**구현 패턴**:
- WPF UI `Ui:NavigationView` (PaneDisplayMode = LeftCompact)
- 페이지별 `SettingsPage{Name}.xaml` + `SettingsPage{Name}ViewModel.cs`
- 검색: 모든 페이지의 설정 항목을 `ISettingsCatalog` 인덱스에 등록, 상단 검색 박스가 즉시 필터
- 딥링크: `INavigationService.NavigateTo("Settings/Shortcuts/Section/Live")` 같은 경로

**모달 → 비모달**:
- 신규 Settings는 별도 `Ui:FluentWindow` (모달 아님) — 사용자가 동시에 메인 창에서 작업 가능
- 변경은 즉시 적용 (Apply 버튼 없음, OK/Cancel 없음) — Windows 11 설정 패턴

## 결과

### 긍정적
- **사용자 학습 비용 0** — Windows 11 사용 경험 그대로
- **검색 가능** — 491 설정을 카테고리 가로질러 찾기
- **딥링크** — 다른 곳(예: 메인 폼의 "단축키 보기" 버튼)에서 특정 설정 페이지 직접 진입
- **단위 테스트 용이** — 페이지별 ViewModel 독립
- **인지 부하 분산** — 한 페이지 평균 10~15개 설정으로 분산

### 부정적
- **마이그레이션 비용 큼** — Sprint 6 (계획서 §10.1) 통째로 할당
- 기존 사용자가 "Apply 버튼이 없네?" 잠시 혼란 가능 → 온보딩 카피 또는 첫 진입 시 1회 안내

### 중립 / 리스크
- 일부 설정은 즉시 적용이 위험할 수 있음 (예: DB 경로 변경) — 그런 항목은 명시적 "변경" 확인 인라인 다이얼로그(`SafetyConfirm`) 사용
- 검색 인덱스 유지보수 — 신규 설정 추가 시 `ISettingsCatalog` 등록 규칙 필수 (코드 리뷰 체크리스트 항목)

## 참조

- 계획서 §6.4, §10.1 Sprint 6
- [WPF UI NavigationView 가이드](https://wpfui.lepo.co/documentation/navigation.html)
- [Windows 11 Settings 디자인 패턴](https://learn.microsoft.com/windows/apps/design/controls/navigationview)
- ADR-0001 (WPF UI), ADR-0006 (시니어 모드)
