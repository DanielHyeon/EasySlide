# EasiSlides 프로젝트 개요

## 프로젝트 목적
EasiSlides는 교회/예배용 프레젠테이션 소프트웨어입니다. PowerPoint 파일, 가사, 성경 구절, 미디어 등을 관리하고 화면에 표시하는 Windows Forms 기반 응용 프로그램입니다.

## 주요 기능
- PowerPoint 슬라이드 미리보기 및 재생
- 성경 구절 표시 및 관리
- 가사(Lyrics) 표시 및 편집
- 멀티 모니터 지원
- 미디어 플레이어 기능
- 데이터베이스 기반 콘텐츠 관리 (SQLite, MySQL/MariaDB)

## 현재 버전
v2.6.1

## 주요 개선 사항 및 이슈
- 좀비 프로세스 문제 해결 (Live 상태 종료 시, PowerPoint 종료 시)
- 멀티 모니터 디스플레이 문제 해결
- 성능 최적화 (프로그램 시작/종료, PPT 미리보기 delay)
- OLEDB 종속성 제거 완료
- String 구문 최적화 ($구문 사용)
