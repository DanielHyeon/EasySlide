# build-targets Specification

## Purpose

`Axicultus.sln`의 프로젝트가 어떤 대상 프레임워크로 빌드되고 빌드와 테스트에 어떤 SDK가 필요한지 정한다.

## Requirements

### Requirement: 솔루션의 프로젝트는 .NET 11을 대상으로 한다

시스템 SHALL `Axicultus.sln`의 모든 프로젝트를 .NET 11 대상으로 빌드한다. 예외는 Roslyn 분석기 프로젝트(`Axicultus.Analyzers`, `netstandard2.0`) 하나다. 공통 도메인 어셈블리(`Axicultus.Core`)는 Windows 전용이 아닌 `net11.0`을 대상으로 한다. 솔루션의 빌드와 테스트에는 .NET 11 SDK만 있으면 된다.

#### Scenario: .NET 11 SDK만 있는 PC

- **GIVEN** PC에 .NET 11 SDK와 런타임만 설치되어 있다
- **WHEN** `Axicultus.sln`을 빌드하고 솔루션의 테스트 프로젝트를 모두 실행한다
- **THEN** 빌드가 성공하고 모든 테스트 프로젝트가 실행된다

#### Scenario: 공통 도메인 어셈블리는 Windows 전용 API를 쓰지 못한다

- **GIVEN** `Axicultus.Core`의 코드가 WinForms나 `System.Drawing` 형식을 쓴다
- **WHEN** 솔루션을 빌드한다
- **THEN** `Axicultus.Core`의 컴파일이 실패한다
