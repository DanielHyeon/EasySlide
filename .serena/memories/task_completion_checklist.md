# 작업 완료 시 체크리스트

## 코드 변경 후 수행할 작업

### 1. 빌드 확인
```powershell
# Visual Studio에서 솔루션 빌드
# F6 또는 Build > Build Solution

# 빌드 오류 확인 및 해결
```

### 2. 실행 테스트
```powershell
# Debug 모드로 실행하여 동작 확인
# F5 또는 Debug > Start Debugging

# 주요 확인 사항:
# - 프로그램 정상 시작/종료
# - PowerPoint 미리보기 정상 작동
# - 메모리 누수 없음
# - 좀비 프로세스 발생 안함
```

### 3. 성능 테스트 (성능 관련 변경 시)
- Visual Studio Performance Profiler로 성능 확인
- 특히 다음 영역 주의:
  - PPT 미리보기 로딩 시간
  - 프로그램 시작/종료 시간
  - UI 응답성

### 4. Git 커밋
```powershell
# 변경사항 확인
git status
git diff

# 스테이징
git add .

# 커밋 (한국어 메시지 사용)
git commit -m "설명적인 커밋 메시지"
```

### 5. 문서 업데이트 (필요 시)
- `버그수정이력.MD` 업데이트 (버그 수정 시)
- `개선사항.md` 업데이트 (기능 개선 시)

## 주요 확인 사항

### 좀비 프로세스 체크
```powershell
# 프로그램 종료 후 프로세스 확인
Get-Process | Where-Object {$_.ProcessName -like "*Easislides*"}
Get-Process | Where-Object {$_.ProcessName -like "*POWERPNT*"}
```

### 멀티 모니터 테스트 (디스플레이 관련 변경 시)
- 싱글 모니터 환경 테스트
- 멀티 모니터 환경 테스트
- 모니터 선택 옵션 테스트

### 데이터베이스 관련 변경 시
- SQLite 데이터베이스 정상 동작 확인
- MariaDB/MySQL 연결 테스트 (해당하는 경우)
- 데이터 무결성 확인

## 릴리스 전 체크리스트
1. Release 빌드 생성 및 테스트
2. 모든 기능 수동 테스트
3. 버전 번호 업데이트
4. 변경 이력 문서 업데이트
5. 설치 패키지 생성 (필요 시)
