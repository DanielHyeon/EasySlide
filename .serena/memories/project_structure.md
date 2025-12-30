# 프로젝트 구조

## 최상위 디렉토리
```
EasiSlides_v2.6.1/
├── .claude/              - Claude 설정
├── .git/                 - Git 저장소
├── .serena/              - Serena 설정
├── .vs/                  - Visual Studio 설정
├── .vscode/              - VSCode 설정
├── Admin/                - 관리자 도구
├── AdminDB/              - 데이터베이스 관리
├── DirectShow/           - DirectShow 라이브러리
├── Dpendency/            - 의존성 파일
├── EasislideImages/      - 이미지 리소스
├── Easislides/           - 메인 애플리케이션 (핵심)
├── GetOffice/            - Office 버전 확인 도구
├── OfficeLib/            - Office Interop 라이브러리
├── packages/             - NuGet 패키지
├── sqlite-netStandard21-binary-1.0.115.0/
├── SQLiteTestConsole/    - SQLite 테스트 콘솔
├── Easislides.sln        - 솔루션 파일
├── .editorconfig         - 편집기 설정
├── .gitignore
├── README.md
├── 개선사항.md           - 개선 사항 목록
└── 버그수정이력.MD       - 버그 수정 이력
```

## Easislides 메인 프로젝트 구조
```
Easislides/
├── Easislides/           - 핵심 소스 코드
│   ├── FrmMain.cs        - 메인 폼
│   ├── Program.cs        - 진입점
│   ├── gf.cs             - 전역 함수 (메인)
│   ├── gfBible.cs        - 성경 관련 함수
│   ├── gfConfig.cs       - 설정 함수
│   ├── gfDatabase.cs     - 데이터베이스 함수
│   ├── gfDisplay.cs      - 디스플레이 함수
│   ├── gfFileIO.cs       - 파일 입출력
│   ├── gfImages.cs       - 이미지 처리
│   ├── gfLyrics.cs       - 가사 처리
│   ├── gfMedia.cs        - 미디어 처리
│   ├── gfPowerPoint.cs   - PowerPoint 처리
│   ├── gfUtility.cs      - 유틸리티
│   ├── ImageCanvas.cs    - 이미지 캔버스 컨트롤
│   ├── ImageTransitionControl.cs - 이미지 전환 효과
│   └── Frm*.cs           - 각종 폼 파일들
├── HookManager/          - 키보드/마우스 훅
├── MariaDB/              - MariaDB 관련
├── Module/               - 모듈
├── Properties/           - 프로젝트 속성
├── Resources/            - 리소스 파일
├── SQLite/               - SQLite 관련
├── Util/                 - 유틸리티
└── Easislides.csproj     - 프로젝트 파일
```

## 주요 파일 설명

### 핵심 파일
- **FrmMain.cs**: 메인 UI 및 워크플로우 관리
- **gf.cs**: 전역 함수 및 유틸리티 (현재 리팩토링 중)
- **ImageCanvas.cs**: PPT 썸네일 이미지 렌더링
- **ImageTransitionControl.cs**: 이미지 전환 애니메이션

### Office Interop
- **OfficeLib/PowerPoint.cs**: PowerPoint 파일 처리 및 슬라이드 Export
