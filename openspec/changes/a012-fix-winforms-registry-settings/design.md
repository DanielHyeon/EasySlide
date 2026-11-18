# Design

## Context

`RegUtil`은 WinForms의 기존 설정 호출을 유지한다. 읽기, 쓰기, 삭제의 결과는 내부에서 구분하며 레지스트리 접근 경계에는 실패를 재현할 수 있는 테스트 대역을 둔다.

## Goals / Non-Goals

- 읽기 전용 키에서 기존 값을 복원하고, 실패 시 작업과 키 이름을 기존 진단 로그에 남긴다.
- 이전 실패와 재시도에서 원본과 이미 저장된 새 값을 보호한다.
- 찬양·성경·예배 순서의 동작, 출력 좌표 계산, PPT COM 및 DB 접근은 범위 밖이다.

## Decisions

- 조회는 `OpenSubKey`를 읽기 전용으로 호출한다. 키나 값이 없는 조회와 예외가 난 조회를 구분한다. 기존 기본값 및 숫자 변환 규칙은 유지한다.
- 다섯 모니터 값 중 비어 있는 대상에 존재하는 이전 값만 기록한다. 접근 실패 시 원본 삭제를 시작하지 않는다. 이때 새 값이 없으면 이전 설정으로 시작한다. 정리할 때 실제로 값이 사라졌는지 확인하고, 재시도를 위한 기준 값 `AlwaysTryDualMonitor`는 마지막에 지운다. [.NET RegistryKey 구현](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Win32.Registry/src/Microsoft/Win32/RegistryKey.cs#L346-L370)
- 글꼴 크기는 `monitors`의 값을 우선하고, 값이 없거나 읽기 실패 시 `options`를 조회한다. 누락 기본값과 범위 검증은 유지한다.
- PowerPoint 등록 버전이 없으면 PowerPoint 설정 쓰기를 건너뛰고 옵션 저장을 계속한다.

## Risks / Trade-offs

이전 중 대상 쓰기가 일부 성공한 상태에서도 원본은 유지된다. 재시도 시 대상에 이미 존재하는 값은 보존한다. 기존 설정 창과 시작 경로는 같은 설정 구현을 사용한다.
