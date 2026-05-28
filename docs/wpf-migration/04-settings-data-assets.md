# 설정, 데이터, 자산 마이그레이션 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: 설정 복잡성, 사용자 자산 보존, legacy 유지보수 정책
- `docs/adr/0002-fluent-icons.md`: 설정/도움말/시스템 아이콘 전환
- `docs/adr/0003-pretendard-font-bundling.md`: UI 폰트 번들
- `docs/adr/0005-options-decomposition.md`: `FrmOptions` 페이지 분해
- `docs/adr/0006-senior-mode-token-scale.md`: Senior mode 설정
- `docs/adr/0007-legacy-ui-safety-net.md`: rollback/fallback 정책
- `docs/ui/icon-migration-map.md`: `Icon.Settings`, `Icon.Shortcuts`, `Icon.Help`, `Icon.Info`

## 1. 범위

이 문서는 환경 설정, 작업 폴더, 데이터베이스, 사용자 자산, 등록/도움말 기능을 WPF로 옮기는 계획이다.

대상 legacy 파일:

| Legacy | 역할 | WPF 목표 |
|---|---|---|
| `FrmOptions.cs` | 거대 옵션 다이얼로그 | `SettingsWindow` + section tabs |
| `FrmBackground.cs` | 배경 설정 | `AppearanceSettingsView` |
| `FrmGetWorkingFolder.cs` | 작업 폴더 선택 | `WorkingFolderSetupView` |
| `FrmAbout.cs`, `FrmHelp.cs`, `FrmRegister.cs` | 정보/도움말/등록 | `AboutView`, `HelpView`, `RegistrationView` |
| `Settings.cs`, `Properties/Settings.Designer.cs` | 앱 설정 | typed settings service |
| `gfConfig.cs`, `gfConstants.cs`, `gfColorsFonts.cs`, `gfUiText.cs` | 설정/상수/색/문구 | `ISettingsService`, EasiDS token |
| `SQLite/SQLiteController.cs`, `Util/DataUtil.cs` | DB 접근 | repository + migration guard |
| `Resources`, `EasislideImages`, `AdminDB` | 리소스/기본 데이터 | asset migration |
| `Util/FileUtil.cs`, `RegUtil.cs`, `OfficeVersion.cs` | 파일/레지스트리/Office 확인 | platform service |

## 2. 목표 설정 구조

`FrmOptions`의 모든 설정을 한 모달에 넣는 구조를 WPF에서는 영역별로 나눈다.

| Settings section | 내용 |
|---|---|
| General | 언어, 시작 동작, 작업 폴더 |
| Appearance | 라이트/다크, Senior 모드, 폰트 크기, 배경 |
| Live Output | 출력 모니터, 가사 화면, Info screen, 안전 확인 |
| PowerPoint | Office 경로, 렌더 옵션, cache |
| Media | 재생 backend, 기본 볼륨, 코덱 안내 |
| Shortcuts | 키보드/리모컨 단축키 |
| Data | SQLite/AdminDB/백업/복구 |
| Import/Export | 기본 경로, DOC/HTML 옵션 |
| Advanced | 로그, diagnostic, legacy compatibility |

## 3. 이식 단계

### 3.0 ADR 준수 체크

- `FrmOptions`의 단일 거대 모달은 Settings sidebar + section content 구조로 분해한다.
- 설정 화면은 다크/라이트/Senior 모드에서 즉시 preview 가능한 구조로 만든다.
- 사용자 자산은 Fluent 아이콘 전환 대상과 사용자 콘텐츠 보존 대상을 분리한다.
- `--legacy-ui` fallback이 필요한 설정은 M3까지 유지한다.

### 3.1 설정 목록화

- `FrmOptions.Designer.cs`의 컨트롤 목록을 섹션별로 분류한다.
- 각 설정의 저장 위치를 찾는다.
- 설정 key, type, default, valid range, legacy value conversion을 표로 정리한다.
- 제거할 설정과 유지할 설정을 구분한다.

산출물:

- `docs/wpf-migration/inventory/settings-map.md`
- `docs/wpf-migration/inventory/legacy-resource-map.md`

### 3.2 Settings service 구현

필수 기능:

- typed get/set
- validation
- 변경 이벤트
- default restore
- import/export
- legacy setting migration
- rollback backup

규칙:

- UI가 `Properties.Settings`나 registry를 직접 읽지 않는다.
- 설정 저장 실패는 사용자에게 알려야 한다.
- 라이브 중 출력 관련 설정 변경은 preview 또는 확인 단계를 둔다.

### 3.3 사용자 자산 마이그레이션

보존 대상:

- 작업 폴더
- 사용자 배경 이미지
- 가져온 찬양/성경/미디어 파일
- DB 파일
- export 기본 경로
- 등록/라이선스 관련 값
- 최근 파일/최근 작업 목록

마이그레이션 정책:

- 원본은 절대 수정하지 않는다.
- `%AppData%\EasislidesNext\Backups\`에 설정 snapshot을 남긴다.
- 복사 후 hash/size를 확인한다.
- 실패 항목은 전체 중단이 아니라 보고서로 남긴다.
- 사용자가 "나중에"를 선택할 수 있다.

### 3.4 데이터베이스 이식

- 현재 SQLite schema와 AdminDB 구조를 문서화한다.
- 읽기 전용 호환 layer를 먼저 구현한다.
- 쓰기 기능은 backup과 transaction을 적용한 뒤 WPF에 연결한다.
- schema 변경이 필요하면 migration version을 둔다.

완료 조건:

- 기존 DB를 WPF에서 열 수 있다.
- WPF에서 저장한 DB를 WinForms v2.6.4가 읽을 수 있는지 호환성 정책을 명확히 한다.
- schema 변경 시 자동 백업과 rollback이 가능하다.

## 4. 완료 여부

| 항목 | 상태 | 비고 |
|---|---|---|
| ThemeService | 1차 완료 | 다크/라이트/Senior 테스트 있음 |
| Pretendard/EasiDS | 1차 완료 | WPF resource 반영 |
| SettingsService | 부분 구현 | `ISettingsService`, `SettingsService`, `EasiSettingsSnapshot`, typed `EasiSettingKeys` 추가. typed get/set, validation, 변경 이벤트, default restore, JSON import/export, 현재 설정 backup, legacy settings source 변환 경계를 자동 검증 완료. 실제 `FrmOptions` 전체 key inventory, SettingsWindow 바인딩, DB schema migration 연결은 후속 필요 |
| SettingsWindow | 미완료 | 신규 구현 필요 |
| legacy settings map | 미완료 | `FrmOptions` 분석 필요 |
| 자산 마이그레이션 | 부분 구현 | `IAssetMigrationService`, `AssetMigrationService` 추가. dry-run, 파일 hash 계산, 복사 후 hash 검증, backup report 작성, 목적지 충돌 시 기존 파일 보존 및 safe name 복사, source missing/not-directory 오류 분류 자동 검증 완료. AppData 실제 경로 연결, SettingsWindow/온보딩 UI, DB/설정 snapshot 통합은 후속 필요 |
| DB 마이그레이션 | 부분 구현 | `IDatabaseMigrationService`, `DatabaseMigrationService` 추가. SQLite `PRAGMA user_version` 분석, 사용자 테이블 inventory, dry-run migration path, backup, transaction, rollback, 실패 시 backup restore, source missing/source-not-file/corrupt DB 오류 분류 자동 검증 완료. 실제 AdminDB 전체 schema inventory, repository 연결, 운영 DB fixture 기반 WinForms/WPF 호환성 검증은 후속 필요 |
| 도움말/정보/등록 | 미완료 | 기존 폼 기능 확인 필요 |

## 5. 이식 후 검증 방안

설정 검증:

- 기존 사용자의 설정을 WPF가 읽고 같은 UI 상태로 표시한다.
- 각 설정 변경 후 앱 재시작 시 값이 유지된다.
- default restore가 동작한다.
- 잘못된 경로/권한 부족/읽기 전용 파일에서 명확한 오류를 표시한다.
- 현재 1차 자동 검증은 임시 설정 파일에서 `SettingsService` typed key get/set, validation failure rollback, default restore, JSON import/export, backup, legacy value conversion 계약으로 수행한다. 실제 `Properties.Settings`/`FrmOptions` 전체 key 매핑 후 호환성 검증을 추가한다.

자산 검증:

- 작업 폴더 복사 전후 파일 수와 hash가 일치한다.
- 이미지/미디어 경로가 깨지지 않는다.
- 중복 파일명 처리 규칙이 문서와 일치한다.
- 마이그레이션 취소 후 원본 앱 실행이 가능하다.
- 현재 1차 자동 검증은 임시 작업 폴더에서 `AssetMigrationService` dry-run/copy/hash/conflict/path error 계약으로 수행한다. 실제 `%AppData%`/운영 작업 폴더 연결 후 수동 리허설을 추가한다.

DB 검증:

- DB backup 생성 확인.
- 정상 DB open/save.
- 손상 DB 처리.
- schema version mismatch 안내.
- transaction 실패 시 rollback.
- 현재 1차 자동 검증은 임시 SQLite DB에서 `DatabaseMigrationService` schema version/table 분석, dry-run 무변경 보고, backup 생성, 순차 migration, transaction rollback, backup restore, missing/directory/corrupt path 오류 계약으로 수행한다. 실제 `AdminDB` schema와 운영 DB 복사본 기준 read/write/repository 호환성 검증을 추가한다.

## 6. 테스트 방안

자동 테스트:

- SettingsService default/get/set/validation tests
- legacy setting conversion tests
- migration dry-run tests
- path permission/error tests
- SQLite repository transaction tests
- asset copy hash tests

현재 자동화 완료:

- `SettingsServiceTests`: typed get/set 저장 및 변경 이벤트, invalid value rollback, default restore, JSON import/export, import 전 backup, invalid import 차단, legacy setting 변환 및 warning report 검증
- `AssetMigrationServiceTests`: dry-run 파일/sha256 report, 원본 무수정 복사, 복사 후 hash 검증, backup report 작성, 목적지 파일 충돌 safe-name 처리, source missing/source-not-directory 오류 분류 검증
- `DatabaseMigrationServiceTests`: SQLite schema version/table 분석, dry-run path 보고, backup 생성, 순차 migration, user_version 갱신, transaction rollback 및 backup restore, source missing/source-not-file/corrupt DB 오류 분류 검증

2026-05-29 검증 결과:

- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter SettingsServiceTests`: 7개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter AssetMigrationServiceTests`: 5개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter DatabaseMigrationServiceTests`: 7개 통과
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug`: 164개 통과
- `dotnet test Easislides.sln -c Debug`: 164개 통과
- `dotnet build Easislides.sln -c Release`: 성공
- `dotnet test Easislides.sln -c Release --no-build`: 164개 통과
- `gstack /qa`, `GSD verify-work`: 현재 작업 환경 PATH에 도구가 없어 실행 불가. 동일 요구사항은 xUnit/Release build/산출물 확인으로 대체 검증

수동 테스트:

1. WinForms에서 설정 변경
2. WPF 실행 후 설정 migration
3. SettingsWindow에서 값 확인
4. 테마/출력/단축키/PowerPoint 설정 변경
5. 앱 재시작
6. 작업 폴더 변경
7. 사용자 배경 이미지 적용
8. DB 백업/복구
9. WinForms rollback 실행 확인

운영 승인 기준:

- 실제 사용자 작업 폴더 3개 이상에서 migration 성공.
- migration report에 실패 항목 0개 또는 사용자가 이해 가능한 skip 항목만 존재.
- 원본 데이터 무손실.
- 설정 변경 후 라이브 출력 동작에 회귀 없음.
