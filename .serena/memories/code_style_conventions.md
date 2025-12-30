# 코드 스타일 및 규칙

## 일반 규칙
- **파일 인코딩**: UTF-8 (.editorconfig에 정의됨)
- **문자열 포맷팅**: $ 구문 사용 권장 (string interpolation)

## 네이밍 규칙

### 클래스 및 구조체
- PascalCase 사용
- 예: `FrmMain`, `ImageCanvas`, `SHFILEOPSTRUCT`

### 메서드
- PascalCase 사용
- 예: `BuildAllPowerpointScreenDumps()`, `LoadItem()`, `ShowPreviewPPThumbs()`

### 변수
- camelCase 또는 PascalCase 혼용
- 프리픽스 사용:
  - `Frm` - Form 클래스
  - `gf` - Global Functions 클래스
  - `In` - Input 파라미터 (예: `InItem`, `InCanvas`)

### 파일명
- PascalCase 사용
- Form 클래스: `Frm` 프리픽스 (예: `FrmMain.cs`, `FrmOptions.cs`)
- 전역 함수 클래스: `gf` 프리픽스 (예: `gf.cs`, `gfBible.cs`, `gfConfig.cs`)

## 프로젝트 구조 패턴
- Global functions를 기능별로 분리된 파일에 정의:
  - `gf.cs` - 메인 전역 함수
  - `gfBible.cs` - 성경 관련
  - `gfConfig.cs` - 설정 관련
  - `gfDatabase.cs` - 데이터베이스 관련
  - `gfDisplay.cs` - 디스플레이 관련
  - `gfFileIO.cs` - 파일 입출력
  - `gfImages.cs` - 이미지 처리
  - `gfLyrics.cs` - 가사 관련
  - `gfMedia.cs` - 미디어 관련
  - `gfPowerPoint.cs` - PowerPoint 관련
  - `gfUtility.cs` - 유틸리티

## 주석 및 문서화
- 한국어 주석 사용
- 복잡한 로직에 대한 설명 주석 권장
