# 개발 명령어 (Windows)

## 빌드
```powershell
# 솔루션 빌드 (Visual Studio 사용)
msbuild Easislides.sln /p:Configuration=Release /p:Platform=x64

# 또는 Visual Studio에서 F6 (빌드)
```

## 실행
```powershell
# Debug 모드 실행 (Visual Studio)
# F5 키 또는 Debug > Start Debugging

# 빌드된 실행 파일 직접 실행
.\Easislides\bin\Release\net10.0-windows7.0\Easislides.exe
```

## 테스트
- 현재 프로젝트에 자동화된 테스트 프로젝트는 포함되어 있지 않음
- 수동 테스트 필요

## Git 명령어
```powershell
# 상태 확인
git status

# 변경사항 확인
git diff

# 브랜치 확인
git branch

# 커밋 로그
git log --oneline -10
```

## Windows 시스템 명령어
```powershell
# 디렉토리 목록
dir
# 또는
Get-ChildItem

# 파일 내용 보기
type <파일명>
# 또는
Get-Content <파일명>

# 파일 찾기
Get-ChildItem -Path . -Filter "*.cs" -Recurse

# 텍스트 검색
Select-String -Path "*.cs" -Pattern "검색어" -Recurse

# 프로세스 확인 (좀비 프로세스 확인용)
Get-Process | Where-Object {$_.ProcessName -like "*Easislides*"}
Get-Process | Where-Object {$_.ProcessName -like "*POWERPNT*"}
```

## 데이터베이스 관련
- SQLite 데이터베이스 파일은 애플리케이션 실행 시 자동 생성
- MariaDB/MySQL은 별도 설정 필요

## 포맷팅 및 린팅
- 현재 자동화된 린팅/포맷팅 도구는 설정되어 있지 않음
- .editorconfig에 UTF-8 인코딩만 지정됨
- Visual Studio의 기본 C# 포맷팅 규칙 사용 권장

## 성능 프로파일링
- Visual Studio Performance Profiler 사용 권장
- 특히 PPT 미리보기 delay 문제 분석 시 유용
