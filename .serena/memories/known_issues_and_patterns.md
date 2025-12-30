# 알려진 이슈 및 디자인 패턴

## 현재 진행 중인 주요 작업

### 1. gf.cs 리팩토링
- gf.cs 파일이 너무 크고 복잡함
- 기능별로 분리 작업 진행 중 (gfBible.cs, gfConfig.cs 등)
- gf_split_plan.md 참조

### 2. 성능 최적화
- **PPT 미리보기 Delay 문제**:
  - PowerPoint Interop이 동기 방식으로 실행되어 UI 블로킹
  - 해결 방안: 비동기 처리 도입 필요
  - 관련 파일: `FrmMain.cs`, `OfficeLib/PowerPoint.cs`, `ImageCanvas.cs`
  
- **프로그램 시작/종료 지연**:
  - 초기화 과정 최적화 필요
  - 리소스 정리 개선 필요

### 3. 좀비 프로세스 문제
- **해결됨 (v2.4.3)**:
  - Live 상태에서 프로그램 종료 시 좀비 프로세스
  - PowerPoint 좀비 프로세스
- 계속 모니터링 필요

## 주요 디자인 패턴

### 전역 함수 패턴 (gf 클래스)
- 정적 메서드로 구성된 유틸리티 클래스
- 기능별로 파일 분리 (gfBible, gfConfig 등)
- 많은 폼에서 `gf.` 프리픽스로 접근

### Windows Forms 패턴
- Form 클래스는 `Frm` 프리픽스 사용
- Designer 패턴 사용 (FrmMain.Designer.cs)
- 이벤트 핸들러 기반 UI 로직

### 데이터베이스 추상화
- SQLite와 MySQL/MariaDB 모두 지원
- DefineConstants로 빌드 시 선택 (ODBC, SQLite)

## 알려진 제한사항

### PowerPoint Interop
- NetOffice 라이브러리 사용
- COM Interop 특성상 성능 제약
- 동기 방식 호출로 인한 UI 블로킹
- 슬라이드 Export가 느림

### 멀티 모니터 지원
- 복잡한 모니터 선택 로직
- 다양한 엣지 케이스 존재
- 개선사항.md의 8번 항목 참조

### 데이터베이스
- PK 설정 이슈 (MariaDB)
- 테이블 존재 여부 체크 로직 개선 필요
- 초기화 시 데이터베이스 파일 생성 확인 작업 필요

## 성능 핫스팟

### 확인된 성능 병목
1. **PowerPoint Export** (가장 큰 병목):
   - 위치: `OfficeLib/PowerPoint.cs:757-884`
   - 동기 방식 슬라이드 Export
   - 슬라이드당 수백ms~수초 소요

2. **썸네일 이미지 로딩**:
   - 위치: `ImageCanvas.cs:496-583`
   - 모든 썸네일을 순차적으로 로드
   - 고품질 리사이징으로 CPU 부하

3. **캐시 체크 로직**:
   - 위치: `FrmMain.cs:4955-4960`
   - 파일 시스템 I/O 빈번

### 권장 최적화 방향
- PowerPoint Export 비동기 처리
- 썸네일 지연 로딩 (Lazy Loading)
- 메모리 캐시 개선
- 이미지 로딩 최적화 (해상도 옵션, InterpolationMode 조정)

## 코드 작성 시 주의사항

### 리소스 정리
- PowerPoint Application 객체는 반드시 명시적으로 Close/Quit
- COM 객체는 Marshal.ReleaseComObject 사용
- using 문 적극 활용

### 스레드 안전성
- UI 스레드와 백그라운드 스레드 구분
- Invoke/BeginInvoke 사용하여 UI 업데이트

### 예외 처리
- COM Interop 호출 시 예외 처리 필수
- 데이터베이스 연결 실패 대비
- 파일 I/O 오류 처리

### 테스트 필요 영역
- 멀티 모니터 환경
- 다양한 해상도
- 대용량 PPT 파일
- 장시간 실행 시나리오
