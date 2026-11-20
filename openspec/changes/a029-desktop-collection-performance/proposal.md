# 데스크톱 자료구조 및 성능 개선

## Why
고정 배열의 용량에 따라 카메라 연결과 파일 선택이 실패할 수 있고, 검색 및 문서 생성에서 입력 크기만큼 메모리를 반복 할당한다.

## What Changes
2026-11-20 전수 조사에서 확인한 포맷 선택, 썸네일, 카탈로그 검색, 문서 입출력과 코드 스타일을 개선한다.

## Capabilities

### New Capabilities
- `desktop-collection-performance`

## Impact
WinForms, WPF, OfficeLib 및 두 테스트 프로젝트. 운영 레지스트리와 파일 포맷은 보존한다.
