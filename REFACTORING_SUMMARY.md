# gfUtility.cs 리팩토링 요약

## 개요
gfUtility.cs 파일을 기능별로 분할하여 코드 관리를 개선하는 작업을 진행했습니다.

## 작업 완료 내용

### Step 1: 미디어 관련 메서드 이동 (완료)
- **대상 파일**: [gfMedia.cs](Easislides/Global/gfMedia.cs)
- **이동된 메서드**:
  - `LoadMusicExtArray()` - 2개 오버로드
  - `BuildMusicFilesListArray()`
  - `GetMediaFileName()` - 4개 오버로드
  - `GetMediaFileNameFromDir()` - 3개 오버로드
  - `Play_Media()`
- **추가된 using 문**:
  - `using Easislides.Module;`
  - `using Easislides.Util;`
  - `using System;`
  - `using System.IO;`
  - `using System.Linq;`
  - `using System.Windows.Forms;`
- **하위 호환성**: gfUtility.cs에 `GetMusicFileName()` → `GetMediaFileName()` 리다이렉트 메서드 추가
- **파일 크기**: 41줄 → 250줄
- **컴파일 결과**: ✅ 성공

### Step 2: 성경 관련 메서드 이동 (완료)
- **대상 파일**: [gfBible.cs](Easislides/Global/gfBible.cs)
- **이동된 메서드**:
  - `LookUpBookName(int InBibleVersion, int InBookNumber)`
  - `LoadSelectedBibleVerses(string InFullBibleString)`
  - `LoadSelectedBibleVerses_Old(string InFullBibleString)`
  - `BuildBibleSearchString()` - 2개 오버로드
  - `PartialWordSearch(int VersionIndex)`
  - `SearchBiblePassages()`
- **추가된 using 문**:
  - `using DbDataReader = System.Data.SQLite.SQLiteDataReader;`
- **파일 크기**: 381줄 → 707줄
- **gfUtility.cs에서 제거된 줄 수**: 167줄
- **컴파일 결과**: ✅ 성공

## 파일 크기 변화

### gfUtility.cs
- **원본**: 7,485줄 (257.9 KB)
- **현재**: 7,282줄
- **감소**: 203줄 제거됨
- **목표**: 더 많은 메서드를 기능별 파일로 이동하여 관리 용이성 향상

## 기존 gf*.cs 파일 현황
프로젝트에 이미 다음과 같은 기능별 파일들이 존재합니다:
- ✅ [gfBible.cs](Easislides/Global/gfBible.cs) - 성경 관련 메서드
- ✅ [gfMedia.cs](Easislides/Global/gfMedia.cs) - 미디어 관련 메서드
- ✅ [gfPowerPoint.cs](Easislides/Global/gfPowerPoint.cs) - PowerPoint 관련 메서드
- ✅ [gfLyrics.cs](Easislides/Global/gfLyrics.cs) - 가사 관련 메서드
- ✅ [gfDisplay.cs](Easislides/Global/gfDisplay.cs) - 화면 표시 관련 메서드
- ✅ [gfFileIO.cs](Easislides/Global/gfFileIO.cs) - 파일 I/O 관련 메서드
- ✅ [gfDatabase.cs](Easislides/Global/gfDatabase.cs) - 데이터베이스 관련 메서드
- ✅ [gfConstants.cs](Easislides/Global/gfConstants.cs) - 상수 및 변수 정의
- ✅ [gfConfig.cs](Easislides/Global/gfConfig.cs) - 설정 관련 메서드
- ✅ [gfFolder.cs](Easislides/Global/gfFolder.cs) - 폴더 관련 메서드
- ✅ [gfImages.cs](Easislides/Global/gfImages.cs) - 이미지 관련 메서드

## 백업 파일
모든 변경 사항 전에 백업이 생성되었습니다:
- `gfUtility.cs.bak` - Step 1 백업
- `gfUtility.cs.bak2` - Step 2 첫 번째 시도 백업
- `gfUtility.cs.bak3` - Step 2 두 번째 시도 백업
- `gfUtility.cs.bak4` - Step 2 최종 백업
- `gfMedia.cs.bak` - 미디어 파일 백업

## 다음 단계 제안

### Step 3: PowerPoint 관련 메서드 검토
- gfUtility.cs에서 PowerPoint 관련 중복 메서드가 있는지 확인
- 필요시 gfPowerPoint.cs로 이동

### Step 4: 화음/코드 표기 관련 메서드 분리
- 화음 및 코드 표기 관련 메서드들을 별도 파일로 분리
  - `BuildChordString()`
  - `GetChordPosInLine()`
  - `IsContainChord()`
  - `GetNotationString()`
  - 기타 관련 메서드들
- 새 파일명 제안: `gfNotation.cs` 또는 `gfChords.cs`

### Step 5: 유틸리티 메서드 재분류
- 남아있는 범용 유틸리티 메서드들을 검토
- 특정 기능 영역과 관련된 것들을 해당 파일로 이동
- 진정한 범용 유틸리티만 gfUtility.cs에 남김

## 검증 사항
- ✅ 모든 변경 후 컴파일 성공
- ✅ 중복 메서드 제거 확인
- ✅ 백업 파일 생성 확인
- ✅ Using 문 추가로 타입 해결
- ✅ 하위 호환성 유지 (리다이렉트 메서드)

## 주의사항
- 각 단계마다 반드시 컴파일하여 오류 확인
- 백업 파일 보관 (필요시 복원 가능)
- 메서드 이동 시 using 문 확인
- 하위 호환성 유지를 위한 리다이렉트 메서드 고려

## 생성된 스크립트
- [cleanup_bible_duplicates.ps1](cleanup_bible_duplicates.ps1) - 수동 정리 가이드
- [remove_bible_methods.ps1](remove_bible_methods.ps1) - 첫 번째 자동 제거 시도
- [remove_bible_methods_v2.ps1](remove_bible_methods_v2.ps1) - 인코딩 보존 제거 (사용됨)
- [build.ps1](build.ps1) - MSBuild 빌드 스크립트

---
**작업 완료일**: 2026-01-01
**컴파일 상태**: ✅ 성공 (경고만 있음, 오류 없음)
