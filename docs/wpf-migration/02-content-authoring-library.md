# 콘텐츠 편집 및 라이브러리 이식 계획

## 기준 문서

- `docs/ui-ux-modernization-plan.md`: 40개 폼 일관성, 모달 남용 감소, 검색/임포트 효율
- `docs/adr/0001-wpf-ui-framework.md`: WPF 화면 체계
- `docs/adr/0002-fluent-icons.md`: 콘텐츠/파일 관리 아이콘 전환
- `docs/adr/0003-pretendard-font-bundling.md`: 한글 가사/성경 가독성
- `docs/adr/0006-senior-mode-token-scale.md`: 편집 필드와 목록 타깃 확대
- `docs/ui/icon-migration-map.md`: `Icon.Bible`, `Icon.WorshipList`, `Icon.Action.*` 매핑

## 1. 범위

이 문서는 찬양/성경/아이템/폴더/검색/가져오기/내보내기 기능을 WPF로 이식하는 계획이다.

대상 legacy 파일:

| Legacy | 역할 | WPF 목표 |
|---|---|---|
| `FrmEditItem.cs` | 일반 아이템 편집 | `ItemEditorView` |
| `FrmEditBibleItem.cs`, `FrmBibleRename.cs` | 성경 아이템 편집/이름 변경 | `BibleItemEditorView` |
| `FrmEditNotes.cs` | 노트 편집 | `NotesEditorDialog` |
| `FrmImport.cs`, `FrmImportFolder.cs`, `FrmImportAccessHelper.cs` | 가져오기 | `ImportWizard` |
| `FrmExport.cs`, `FrmGenerateDoc.cs`, `FrmGenerateHtml.cs` | 내보내기/문서 생성 | `ExportWizard` |
| `FrmFind.cs`, `FrmLookupTitles.cs`, `FrmUsages.cs` | 검색/참조 확인 | `SearchPanel`, `UsageInspector` |
| `FrmManageItemLists.cs`, `FrmRearrangeFolderPositions.cs` | 목록/폴더 관리 | `LibraryManagementView` |
| `FrmCopy.cs`, `FrmMove.cs`, `FrmCopyMoveExternal.cs`, `FrmUpdateFileName.cs`, `FrmRecoverDeleted.cs`, `FrmSmartMerge.cs` | 파일/목록 조작 | `LibraryCommandDialogs` |
| `gfBible.cs`, `gfLyrics.cs`, `gfFolder.cs`, `gfDatabase.cs`, `gfFileHelpers.cs`, `gfIO.cs`, `gfUtility.cs` | 콘텐츠 도메인 로직 | service 계층으로 추출 |

## 2. 목표 모델

WinForms의 폼 단위 기능을 WPF에서는 다음 도메인 단위로 묶는다.

| 도메인 | 주요 객체 | 주요 기능 |
|---|---|---|
| Worship list | `WorshipList`, `WorshipItem` | 순서 편집, 복사/이동, 삭제 복구, 사용처 확인 |
| Song/Lyrics | `SongLyrics`, `SongFormat`, `SongSettings` | 가사 편집, 포맷 적용, 미리보기 |
| Bible | `BibleReference`, `BibleItem` | 구절 검색, 이름 변경, 성경 아이템 편집 |
| Import/Export | `ImportJob`, `ExportJob` | 폴더/Access/HTML/DOC 변환 |
| Search | `SearchQuery`, `SearchResult` | 제목/본문/성경/파일명 검색 |

## 3. 이식 단계

### 3.0 ADR 준수 체크

- 찬양/성경/미디어/문서/폴더/관리 명령은 `docs/ui/icon-migration-map.md`의 EasiDS 키를 우선 사용한다.
- 긴 한글 가사와 성경 본문은 Pretendard 타입 토큰을 사용하고 시스템 폰트 직접 지정은 금지한다.
- 가져오기/내보내기 wizard의 버튼과 목록은 Senior mode scale을 적용한다.
- 기존 WinForms 모달을 그대로 복제하지 않고, 빈번한 편집은 패널/문서형 화면으로 옮긴다.

### 3.1 데이터 계약 정리

- `Module/SongLyrics.cs`, `SongFormat.cs`, `SongSettings.cs`를 WPF에서도 참조 가능한 순수 도메인 모델로 정리한다.
- `gfLyrics`, `gfBible`, `gfFolder`의 public 기능을 인터페이스로 감싼다.
- 파일 경로, DB id, 표시 제목, 정렬 순서의 canonical format을 문서화한다.

필수 인터페이스:

| 인터페이스 | 책임 |
|---|---|
| `IWorshipListRepository` | 예배 목록 조회/저장/정렬 |
| `ISongRepository` | 찬양 가사 조회/저장 |
| `IBibleService` | 성경 검색/구절 파싱 |
| `IImportService` | 외부 데이터 가져오기 |
| `IExportService` | DOC/HTML/파일 내보내기 |
| `ISearchService` | 통합 검색 |

### 3.2 라이브러리 화면 이식

권장 화면:

- 좌측: 폴더/목록 트리
- 중앙: 아이템 목록
- 우측: 선택 항목 편집/프리뷰
- 상단: 검색, 필터, 정렬
- 하단: 복사, 이동, 삭제, 복구, 병합 등 명령

완료 조건:

- 기존 목록 순서가 WPF에서 동일하게 보인다.
- 드래그/키보드로 순서 변경 가능하다.
- 삭제는 soft delete와 복구 경로를 보존한다.
- 파일명 변경과 외부 복사는 충돌/중복 확인을 제공한다.

### 3.3 편집 화면 이식

`FrmEditItem`은 심볼 수가 많아 한 번에 옮기지 않는다.

단계:

1. 읽기 전용 상세 패널
2. 제목/메모/분류 편집
3. 가사/본문 편집
4. 배경/폰트/포맷 미리보기
5. 저장 전 diff/validation

저장 규칙:

- 저장 전 원본 백업 생성
- validation 실패 시 파일에 쓰지 않음
- 저장 성공 후 프리뷰와 검색 인덱스 갱신
- 라이브 중 현재 송출 항목 편집은 명시적 확인 필요

### 3.4 Import/Export 이식

Import Wizard:

- Step 1: 원본 유형 선택
- Step 2: 파일/폴더/Access DB 선택
- Step 3: 항목 preview
- Step 4: 충돌 해결
- Step 5: 실행 및 결과

Export Wizard:

- Step 1: 대상 목록 선택
- Step 2: 형식 선택(DOC/HTML/폴더)
- Step 3: 파일명/경로/옵션
- Step 4: 실행
- Step 5: 결과 열기/로그 확인

완료 조건:

- 기존 `FrmImport*`, `FrmExport`, `FrmGenerateDoc`, `FrmGenerateHtml`의 주요 옵션이 WPF에 존재한다.
- 같은 입력 데이터에서 WinForms와 WPF의 결과 파일 수, 제목, 순서가 일치한다.
- 실패 항목은 건너뛰기/재시도/로그 저장이 가능하다.

## 4. 완료 여부

| 항목 | 상태 | 비고 |
|---|---|---|
| 도메인 모델 파악 | 부분 완료 | `Module` 구조 확인 |
| Repository/service 추출 | 부분 구현 | `IAdminDatabaseRepository`/`AdminDatabaseRepository`로 AdminDB schema inventory, `FOLDER`/`SONG` read-only folder/song summary 조회, bundled AdminDB 호환성 검증, folder/song 저장 및 song 이동 write repository backup/transaction/rollback 검증 완료. `LibraryViewModel`이 명시 AdminDB 경로 또는 기존 작업 폴더의 `Admin\Database\EasiSlidesDb.db`를 읽는 UI 경계까지 연결했다. `gf*` 의존 분리와 전체 콘텐츠 CRUD service 계약은 후속 필요 |
| Library WPF 화면 | 1차 구현 완료 | `LibraryWindow`, `LibraryViewModel` 추가. MainWindow에서 라이브러리 버튼으로 진입하고, AdminDB 폴더 목록, 폴더 추가/편집 진입, 선택 폴더의 곡 목록, 제목/대체 제목/분류/key/가사 검색, 선택 곡 가사 프리뷰, 새 곡/편집/선택 곡 복사/이동/삭제 진입, 삭제 곡 복구 진입, 폴더/곡 위아래 및 직접 drag gesture 순서 변경, 경로 누락/파일 누락 상태 메시지를 제공한다. 현재는 AdminDB folder add/edit/reorder와 song browse/search/edit/copy/move/soft delete/recover/reorder 중심이며 병합 명령은 후속 구현 |
| Library management | 부분 구현 | `FolderEditorWindow`, `FolderEditorViewModel` 추가. 기존 폴더 편집과 새 폴더 번호 산정, 이름/사용 여부 저장, `IAdminDatabaseRepository.SaveFolderAsync` 기반 backup/transaction 저장, 저장 후 폴더 reload와 선택 복원을 구현했다. 폴더 순서 변경은 `ReorderFoldersAsync`로 `FOLDER.FolderNo`와 해당 `SONG.FOLDERNO`를 함께 staging 업데이트하도록 구현했다. 폴더 삭제/복구, 병합, 외부 복사는 후속 필요 |
| Item editor | 부분 구현 | `SongEditorWindow`, `SongEditorViewModel` 추가. 선택 폴더에서 새 곡 생성 또는 선택 곡 편집을 열고 제목/대체 제목/번호/분류/key/가사를 편집한 뒤 `IAdminDatabaseRepository.SaveSongAsync`로 저장한다. 저장 전 제목/AdminDB/폴더 검증, 설정 `DataBackupRoot` 또는 DB sibling `Backups` fallback, 저장 성공 후 목록 reload/선택 복원을 구현했다. `FrmEditItem`의 배경/폰트/포맷 미리보기와 라이브 중 편집 안전 확인은 후속 필요 |
| Library command dialogs | 부분 구현 | `SongCopyWindow`/`SongCopyViewModel`, `SongMoveWindow`/`SongMoveViewModel`, `SongDeleteWindow`/`SongDeleteViewModel`, `SongRecoveryWindow`/`SongRecoveryViewModel` 추가. 복사는 대상 폴더/복사 제목/곡 번호를 편집하고 원곡 메타데이터와 가사를 `SaveSongAsync`로 새 곡 저장한다. 이동은 선택 곡과 현재 폴더를 기준으로 source 폴더를 제외한 대상 폴더를 선택하고 `MoveSongsAsync`로 backup/transaction 기반 이동을 수행한다. 삭제는 `SoftDeleteSongsAsync`로 `FOLDERNO=0`, `OldFolder=원래 폴더`, `LastModified=삭제일`을 보존하고, 복구는 `GetDeletedSongsAsync` 목록에서 선택한 곡을 `RecoverSongsAsync`로 원래 폴더에 되돌리며 `OldFolder=0`으로 정리한다. 일반 이동은 legacy와 맞게 `LastModified`를 갱신하지 않고, 삭제/복구만 날짜를 갱신하도록 분리했다. AdminDB 경로/파일, 선택 곡, 대상 폴더, configured/default backup root validation을 자동 검증했다. 병합, 외부 복사는 후속 필요 |
| Bible editor | 미완료 | `gfBible` 계약화 필요 |
| Import/Export wizard | 미완료 | 기존 옵션 목록화 필요 |
| Search/usage inspector | 미완료 | 검색 결과 동등성 테스트 필요 |

## 5. 이식 후 검증 방안

데이터 동등성:

- 동일 작업 폴더를 WinForms와 WPF에서 열었을 때 목록 수, 항목 수, 제목, 순서가 일치한다.
- 저장 후 파일 diff에서 의도하지 않은 메타데이터 변경이 없다.
- 가져오기 후 새 항목 수와 중복 처리 결과가 일치한다.
- 내보내기 결과 파일명이 기존 규칙과 호환된다.

편집 안정성:

- 저장 실패 시 원본 파일이 보존된다.
- 편집 후 프리뷰가 갱신된다.
- 라이브 중인 항목 편집 시 SafetyConfirm이 표시된다.
- 삭제/복구/스마트 병합 후 사용처 정보가 갱신된다.

UX 검증:

- 검색 결과에서 키보드 Enter로 선택/편집 가능하다.
- 긴 제목, 한글/영문 혼합, 특수문자 파일명이 잘리지 않는다.
- Senior 모드에서 편집 필드와 버튼의 최소 높이가 유지된다.

## 6. 테스트 방안

자동 테스트:

- Repository 테스트: 샘플 작업 폴더 fixture로 목록 읽기/저장
- 현재 자동화: `AdminDatabaseRepositoryTests`로 임시 legacy AdminDB와 bundled `AdminDB/Database/EasiSlidesDb.db` schema/table/column inventory, `FOLDER`/`SONG` 필수 table/column 호환성 진단, read-only folder song count, folder별 song summary 조회, 삭제 곡 조회, folder upsert, legacy SONG 필드 insert/update, update 시 `OldFolder` 보존, 일반 song 이동 시 `LastModified` 보존, song 이동 transaction rollback 및 backup restore, soft delete/recover backup/transaction, folder reorder 시 `FOLDER.FolderNo`와 `SONG.FOLDERNO` 동시 갱신, song reorder 시 `SONG_NUMBER` 재시퀀싱과 folder boundary rollback을 검증한다. `LibraryViewModelTests`로 명시 AdminDB 경로 로드, 폴더 선택 시 곡 재조회, 제목/대체 제목/분류/가사 검색, AdminDB 경로 누락 상태 메시지, 폴더/곡 순서 변경 요청 매핑, drag/drop용 target index reorder와 reload/선택 복원을 검증한다. `FolderEditorViewModelTests`로 기존 폴더 로드/dirty state, folder save mapping, configured/default backup root, 새 폴더 번호 산정, 이름 누락 validation을 검증한다. `SongEditorViewModelTests`로 기존 곡 편집 필드 로드/dirty state, 저장 시 `SongWriteModel` 매핑과 configured/default backup root, 제목 누락 validation, 신규 곡 insert 후 song id 반영을 검증한다. `SongCopyViewModelTests`로 원본 폴더 포함 대상 목록, 기본 복사 제목, 대상 폴더별 곡 번호 산정, `SongWriteModel` 매핑, configured/default backup root, 제목 누락 validation을 검증한다. `SongMoveViewModelTests`로 source 폴더 제외 대상 목록, configured/default backup root, `SongMoveRequest` 매핑, 대상 폴더 누락 validation을 검증한다. `SongDeleteViewModelTests`로 삭제 대상 로드, `SongDeleteRequest` 매핑, configured/default backup root, 선택 곡 누락 validation을 검증한다. `SongRecoveryViewModelTests`로 삭제 곡 목록 로드, 선택 곡 recover request 매핑, configured/default backup root, 미선택 validation을 검증한다. `AppServiceRegistrationTests`는 Library/FolderEditor/SongEditor/SongCopy/SongMove/SongDelete/SongRecovery ViewModel/Window 운영 DI 등록을 포함한다.
- Parser 테스트: 성경 참조, 가사 구분자, 파일명 규칙
- Import 테스트: 정상/중복/깨진 파일/빈 폴더
- Export 테스트: DOC/HTML 결과 메타데이터와 파일 생성 여부
- Editor ViewModel 테스트: validation, dirty state, save/cancel
- Search 테스트: 제목/본문/성경 키워드 hit count

2026-05-29 현재 추가 검증:

- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "AdminDatabaseRepositoryTests|LibraryViewModelTests"`: 24개 통과
- `LibraryViewModelTests`: AdminDB 경로 해석, 폴더/곡 목록 로드, 선택 폴더 재조회, 제목/대체 제목/분류/가사 검색, 경로 누락 메시지, 폴더/곡 순서 변경 요청 매핑, target index reorder와 reload/선택 복원 검증
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "FolderEditorViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `FolderEditorViewModelTests`: 기존 폴더 로드/dirty state, 저장 매핑/백업 루트, 새 폴더 번호 산정, 이름 validation 검증
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SongEditorViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `SongEditorViewModelTests`: 기존 곡 로드/dirty state, 저장 매핑/백업 루트, 제목 validation, 신규 곡 id 반영 검증
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SongCopyViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `SongCopyViewModelTests`: 기본 복사 제목/대상 폴더/곡 번호 산정, 저장 매핑/백업 루트, 제목 validation, 신규 곡 id 반영 검증
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "SongMoveViewModelTests|AppServiceRegistrationTests"`: 5개 통과
- `SongMoveViewModelTests`: 대상 폴더 목록, 이동 요청 매핑, 백업 루트 fallback, 대상 폴더 validation 검증
- `dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj -c Debug --filter "AdminDatabaseRepositoryTests|SongDeleteViewModelTests|SongRecoveryViewModelTests|AppServiceRegistrationTests"`: 25개 통과
- `SongDeleteViewModelTests`/`SongRecoveryViewModelTests`: soft delete/recover request 매핑, 백업 루트 fallback, validation 검증
- `dotnet test Easislides.sln -c Debug`: 305개 통과

수동 테스트:

1. 실제 운영 작업 폴더 복사본 준비
2. 목록 열기
3. 항목 5개 편집 후 저장
4. 항목 복사/이동/삭제/복구
5. 성경 아이템 생성/이름 변경
6. 폴더 가져오기
7. DOC/HTML 내보내기
8. 검색/사용처 확인
9. WinForms 결과와 WPF 결과 비교

테스트 데이터:

- 한글 제목 긴 곡
- 영어/숫자/특수문자 제목
- 이미지 배경 포함 항목
- 성경 구절 범위
- 빈 가사/깨진 파일/중복 파일명
