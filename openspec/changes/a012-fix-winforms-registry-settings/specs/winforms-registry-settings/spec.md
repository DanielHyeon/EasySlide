# Spec Delta

## Purpose

WinForms의 설정 조회와 저장이 레지스트리 권한 제한과 버전 이전 실패에도 저장된 값을 보존하도록 한다.

## ADDED Requirements

### Requirement: 읽기 권한과 접근 실패 진단

WinForms는 설정을 읽기 전용으로 조회하고 접근 실패 시 작업과 키 이름을 기록해야 한다(SHALL).

#### Scenario: 읽기 전용으로 기존 설정 조회

- **GIVEN** 현재 사용자가 설정 키를 읽을 수 있다.
- **WHEN** WinForms가 기존 값을 조회한다.
- **THEN** 키를 생성하거나 쓰기 권한을 요청하지 않고 저장된 값을 사용한다.

#### Scenario: 접근 실패

- **WHEN** 레지스트리 읽기, 쓰기 또는 삭제 도중 접근 오류가 발생한다.
- **THEN** 실패한 작업과 키를 로그에 남기고 원본을 제거할지 결정하는 호출자에게 실패를 알린다.

### Requirement: 구버전 모니터 설정 이전

WinForms는 새 위치에 필요한 값이 모두 준비된 후에 기존 설정을 삭제해야 한다(SHALL).

#### Scenario: 대상 쓰기 실패 후 재시도

- **GIVEN** 구버전 모니터 설정 값이 있다.
- **WHEN** 대상 쓰기 중 하나가 실패한다.
- **THEN** 원본 설정을 모두 보존한다.
- **AND** 새 설정이 비어 있는 동안에는 이전 설정을 읽는다.
- **AND** 다시 시도할 때 기존 대상 값을 덮어쓰지 않는다.

### Requirement: 가사 모니터 악보 글꼴 크기 복원

WinForms는 가사 모니터 악보 글꼴 크기를 저장한 위치에서 복원해야 한다(SHALL).

#### Scenario: 가사 모니터 옵션을 저장하고 다시 실행

- **WHEN** 옵션에서 유효한 악보 글꼴 크기를 저장하고 WinForms를 다시 시작한다.
- **THEN** `monitors`에 저장한 크기가 적용된다.
- **AND** 해당 값이 없으면 기존 `options`의 값으로 복원할 수 있다.

### Requirement: PowerPoint 등록이 없는 PC의 옵션 저장

WinForms는 PowerPoint의 설치 등록을 찾지 못해도 EasiSlides 옵션을 저장해야 한다(SHALL).

#### Scenario: PowerPoint 미설치 상태에서 출력 모니터 선택

- **GIVEN** 출력 모니터가 선택되어 있고 PowerPoint 설치 등록을 찾을 수 없다.
- **WHEN** 사용자가 옵션의 확인을 누른다.
- **THEN** PowerPoint 설정 쓰기를 건너뛰고 EasiSlides 옵션 저장을 계속한다.
