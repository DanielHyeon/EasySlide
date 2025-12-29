# gf.cs 파일 분리 계획서

## 📊 현재 상태
- **원본 파일**: `gf.cs` (17,665줄, 604KB)
- **목표**: 12개 파일로 분리
- **예상 총 라인 수**: 21,689줄
- **예상 총 메서드 수**: 533개
- **예상 상수/필드 수**: 964개

## 📁 분리 대상 파일 목록

### 1. gfConstants.cs (2,014줄, 51KB)
**목적**: 상수와 정적 필드 정의
- **상수**: 246개
- **정적 필드**: 718개
- **포함 내용**:
  - `public const string` 선언들 (버전, 확장자, 심볼 등)
  - `public const int` 선언들 (모드, 제한값 등)
  - `public const char` 선언들
  - `public static` 필드들 (전역 변수, 설정값 등)
  - `[DllImport]` 선언들
  - 구조체 정의 (SHFILEOPSTRUCT 등)

**분리 기준**:
- 모든 `const` 선언
- 모든 `static` 필드 (메서드가 아닌)
- P/Invoke 선언
- 구조체/열거형 정의

---

### 2. gfUtility.cs (3,183줄, 89KB)
**목적**: 범용 유틸리티 메서드
- **메서드 수**: 122개
- **포함 내용**:
  - 문자열 처리 유틸리티
  - 형변환 메서드
  - 검증 메서드 (Validate*)
  - 배열 정렬/처리
  - 일반적인 헬퍼 메서드
  - 초기화 메서드 (Init*, Load* 중 범용)

**분리 기준**:
- 특정 도메인에 속하지 않는 범용 메서드
- 재사용 가능한 유틸리티 함수
- 예: `GetDisplayNameOnly`, `ValidateRootFolder`, `SingleArraySort` 등

---

### 3. gfDatabase.cs (1,375줄, 44KB)
**목적**: 데이터베이스 작업 메서드
- **메서드 수**: 16개
- **포함 내용**:
  - 데이터베이스 연결 관리
  - CRUD 작업 (Insert, Update, Delete, Select)
  - 데이터베이스 검증
  - 트랜잭션 처리
  - 데이터베이스 초기화/마이그레이션

**분리 기준**:
- `InsertItemIntoDatabase` (모든 오버로드)
- `UpdateDatabaseItem` (모든 오버로드)
- `DeleteAllFolders`
- `ResetFolder`
- `ValidateDB`
- `RestoreOriginalSongsDatabase`
- 데이터베이스 관련 쿼리 실행 메서드

---

### 4. gfBible.cs (974줄, 33KB)
**목적**: 성경 관련 기능
- **메서드 수**: 19개
- **포함 내용**:
  - 성경 버전 관리
  - 성경 책 목록 로드
  - 성경 구절 로드/표시
  - 성경 검색 기능
  - 성경 파일 관리

**분리 기준**:
- `LoadBibleVersions`
- `LoadBibleBooksList` (모든 오버로드)
- `LoadBiblePassages` (모든 오버로드)
- `RefreshBiblePassages` (모든 오버로드)
- `GetBibleFileName`
- `HBConvertVersion`
- `LookUpBibleName`
- 성경 관련 모든 메서드

---

### 5. gfDisplay.cs (1,785줄, 74KB)
**목적**: UI/Display 관련 메서드
- **메서드 수**: 40개
- **포함 내용**:
  - 슬라이드 표시
  - 텍스트 렌더링
  - 화면 출력
  - 폰트 조정
  - 레이아웃 계산

**분리 기준**:
- `ShowDBSlide`
- `DrawText`, `DrawOneLine`, `DrawOneRegion`
- `DisplaySlidesFormattedLyrics`
- `OutputOneLineToScreen`
- `ReduceFontToFit`, `IncreaseFontToLargest`
- `DrawDisplayPanel`
- `DataDisplaySlides`
- 모든 화면 표시 관련 메서드

---

### 6. gfMedia.cs (854줄, 25KB)
**목적**: 미디어/음악 관련 메서드
- **메서드 수**: 36개
- **포함 내용**:
  - 미디어 파일 검색
  - 음악 파일 관리
  - 미디어 확장자 검증
  - 미디어 파일 경로 처리

**분리 기준**:
- `MusicFound` (모든 오버로드)
- `GetMediaFileName` (모든 오버로드)
- `GetMediaFileNameFromDir` (모든 오버로드)
- `BuildMusicFilesListArray`
- `ValidateMusicExt`
- `GetOpenFileDialogMediaString`
- `LoadMusicExtArray`
- 미디어 관련 모든 메서드

---

### 7. gfPowerPoint.cs (201줄, 5.7KB)
**목적**: PowerPoint 관련 메서드
- **메서드 수**: 7개
- **포함 내용**:
  - PowerPoint 파일 처리
  - PowerPoint 슬라이드 관리
  - PowerPoint 연동

**분리 기준**:
- PowerPoint 관련 모든 메서드
- `GetOfficeDocContents` (PowerPoint 관련 부분)
- PowerPoint 객체 관리 메서드

---

### 8. gfFileIO.cs (1,350줄, 44KB)
**목적**: 파일 I/O 작업
- **메서드 수**: 43개
- **포함 내용**:
  - 파일 읽기/쓰기
  - 텍스트 파일 처리
  - 바이너리 파일 처리
  - 파일 검증
  - 파일 경로 처리

**분리 기준**:
- `LoadTextFile` (모든 오버로드)
- `LoadFileContents`
- `Load32InfoFile`, `LoadInfoFile`
- `SaveWorshipList`
- `LoadWorshipList`
- 파일 저장/로드 관련 모든 메서드
- 파일 경로 처리 메서드

---

### 9. gfImage.cs (514줄, 16KB)
**목적**: 이미지 처리 메서드
- **메서드 수**: 10개
- **포함 내용**:
  - 이미지 로드/저장
  - 이미지 크기 조정
  - 이미지 비율 계산
  - 썸네일 생성
  - 이미지 캔버스 관리

**분리 기준**:
- `FormatImageContainers`
- `ShowThumbImage`
- `SetImageRatio`
- `CalcImageToFit` (모든 오버로드)
- `dumpImageToFile`
- 이미지 처리 관련 모든 메서드

---

### 10. gfLyrics.cs (5,421줄, 185KB)
**목적**: 가사/노테이션 처리
- **메서드 수**: 142개
- **포함 내용**:
  - 가사 파싱
  - 노테이션 처리
  - 가사 포맷팅
  - 시퀀스 관리
  - 가사 표시 형식 변환

**분리 기준**:
- `ExtractLyrics` (모든 오버로드)
- `ExtractNewFormatLyrics`, `ExtractDefaultFormatLyrics`
- `FormatDisplayLyrics`
- `CombineLyricsAndNotations`
- `RTFFormatNotationString`
- `TransposeNotations`
- `ListNotationData`
- `FormatText`
- 가사/노테이션 관련 모든 메서드

---

### 11. gfFolder.cs (1,414줄, 52KB)
**목적**: 폴더 관리 메서드
- **메서드 수**: 32개
- **포함 내용**:
  - 폴더 생성/삭제
  - 폴더 검증
  - 폴더 이름 처리
  - 폴더 목록 관리

**분리 기준**:
- `LoadFolderNamesArray` (모든 오버로드)
- `GetFolderNumber` (모든 오버로드)
- `ValidateDir`, `ValidateDirNameFormat`
- `CorrectDirNameFormat`
- 폴더 관련 모든 메서드

---

### 12. gfConfig.cs (2,604줄, 113KB)
**목적**: 설정 관리 메서드
- **메서드 수**: 46개
- **포함 내용**:
  - 설정 로드/저장
  - 레지스트리 작업
  - 옵션 관리
  - 기본값 설정
  - 설정 검증

**분리 기준**:
- `LoadSavedData`
- `SaveConfigSettings`
- `SaveFoldersSettings` (모든 오버로드)
- `SaveOptionsData`
- `LoadLicAdminDetails`
- `SaveLicenceConfigSettings`
- `LoadSongKeyCapoTiming`
- `GenerateMusicKeysList`
- `ComputeShowLineSpacing`
- 설정 관련 모든 메서드

---

## 🔄 작업 순서

### Phase 1: 준비 작업
1. ✅ 원본 파일 백업 (`gf.cs.backup` 확인됨)
2. ✅ 빈 껍데기 파일 준비 (`gf.cs.empty` 확인됨)
3. 현재 `gf.cs` 파일의 전체 구조 분석

### Phase 2: 상수 및 필드 분리 (우선순위 1)
1. **gfConstants.cs** 생성
   - 모든 `const` 선언 추출
   - 모든 `static` 필드 추출
   - P/Invoke 선언 추출
   - 구조체 정의 추출

**이유**: 다른 모든 파일이 이 상수/필드에 의존하므로 먼저 분리해야 함

### Phase 3: 핵심 기능 분리 (우선순위 2)
2. **gfDatabase.cs** - 데이터베이스 메서드
3. **gfUtility.cs** - 범용 유틸리티 메서드
4. **gfConfig.cs** - 설정 관리 메서드

### Phase 4: 도메인별 기능 분리 (우선순위 3)
5. **gfBible.cs** - 성경 관련
6. **gfLyrics.cs** - 가사/노테이션 (가장 큰 파일)
7. **gfDisplay.cs** - UI/Display
8. **gfMedia.cs** - 미디어/음악
9. **gfFileIO.cs** - 파일 I/O
10. **gfImage.cs** - 이미지 처리
11. **gfFolder.cs** - 폴더 관리
12. **gfPowerPoint.cs** - PowerPoint

### Phase 5: 정리 작업
13. 원본 `gf.cs`를 빈 껍데기로 교체
14. 모든 파일 컴파일 테스트
15. 의존성 확인 및 수정

---

## 📝 각 파일 생성 규칙

### 공통 헤더 구조
```csharp
//using JRO;
using Easislides.SQLite;
//using Easislides.Model.EasiSlidesDbDataSetTableAdapters;
using Easislides.Util;
//using Microsoft.Office.Interop.Access.Dao;
using Microsoft.Win32;
//using NetOffice.PowerPointApi;
using OfficeLib;
using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using System.Threading;

//using NetOffice.DAOApi;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Easislides
{
    internal unsafe partial class gf
    {
        // 각 파일별 내용
    }
}
```

### 공통 푸터 구조
```csharp
    }
}
```

---

## ⚠️ 주의사항

1. **Partial Class**: 모든 파일은 `partial class gf`로 선언되어야 함
2. **의존성 순서**: 
   - Constants → Utility → Database → Config → 나머지
3. **메서드 오버로드**: 같은 이름의 모든 오버로드를 같은 파일에 포함
4. **private 메서드**: 관련 public 메서드와 같은 파일에 배치
5. **컴파일 테스트**: 각 파일 분리 후 즉시 컴파일 확인
6. **네임스페이스**: 모든 파일은 `namespace Easislides` 내부

---

## 🔍 분류 기준 요약

### 메서드 분류 키워드

**gfConstants.cs**:
- `const`, `static` (필드), `[DllImport]`, `struct`, `enum`

**gfUtility.cs**:
- `Get*`, `Validate*` (범용), `Convert*`, `Format*` (범용), `Init*` (범용)

**gfDatabase.cs**:
- `Insert*`, `Update*`, `Delete*`, `Select*`, `*Database*`, `*DB*`

**gfBible.cs**:
- `*Bible*`, `*HB*`, `LoadBible*`, `GetBible*`

**gfDisplay.cs**:
- `Show*`, `Draw*`, `Display*`, `Output*`, `*Screen*`, `*Panel*`

**gfMedia.cs**:
- `*Music*`, `*Media*`, `GetMedia*`, `ValidateMusic*`

**gfPowerPoint.cs**:
- `*PowerPoint*`, `*PPT*`, `*PP*`

**gfFileIO.cs**:
- `Load*File*`, `Save*File*`, `*TextFile*`, `*InfoFile*`

**gfImage.cs**:
- `*Image*`, `*Thumb*`, `*Canvas*`, `*Ratio*`

**gfLyrics.cs**:
- `*Lyrics*`, `*Notation*`, `Extract*`, `Format*Lyrics*`, `*Sequence*`

**gfFolder.cs**:
- `*Folder*`, `*Dir*`, `GetFolder*`, `ValidateDir*`

**gfConfig.cs**:
- `Load*`, `Save*`, `*Config*`, `*Settings*`, `*Options*`

---

## ✅ 검증 체크리스트

각 파일 분리 후 확인:
- [ ] 파일이 올바른 헤더/푸터를 가지고 있는가?
- [ ] `partial class gf`로 선언되어 있는가?
- [ ] 필요한 using 문이 모두 포함되어 있는가?
- [ ] 컴파일 오류가 없는가?
- [ ] 메서드가 올바른 카테고리로 분류되었는가?
- [ ] 오버로드가 모두 포함되었는가?
- [ ] private 메서드가 관련 public 메서드와 함께 있는가?

---

## 📅 예상 작업 시간

- **Phase 1**: 30분 (준비 및 분석)
- **Phase 2**: 1시간 (Constants 분리)
- **Phase 3**: 2시간 (핵심 기능 분리)
- **Phase 4**: 4시간 (도메인별 기능 분리)
- **Phase 5**: 1시간 (정리 및 테스트)

**총 예상 시간**: 약 8-10시간

---

## 🚀 시작 방법

1. 현재 `gf.cs` 파일의 전체 내용을 분석
2. 각 메서드/필드를 카테고리별로 분류
3. Phase 2부터 순차적으로 진행
4. 각 단계마다 컴파일 테스트 수행



