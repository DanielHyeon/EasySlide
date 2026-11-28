## ADDED Requirements

### Requirement: 솔루션의 프로젝트는 .NET 11을 대상으로 한다

시스템 SHALL `Axicultus.sln`의 모든 프로젝트를 .NET 11 대상으로 빌드한다. 예외는 Roslyn 분석기 프로젝트(`Axicultus.Analyzers`, `netstandard2.0`) 하나다. 공통 도메인 어셈블리(`Axicultus.Core`)는 Windows 전용이 아닌 `net11.0`을 대상으로 한다. 솔루션의 빌드와 테스트에는 .NET 11 SDK만 있으면 된다.

#### Scenario: .NET 11 SDK만 있는 PC

- **GIVEN** PC에 .NET 11 SDK와 런타임만 설치되어 있다
- **WHEN** `Axicultus.sln`을 빌드하고 솔루션의 테스트 프로젝트를 모두 실행한다
- **THEN** 빌드가 성공하고 모든 테스트 프로젝트가 실행된다

#### Scenario: 공통 도메인 어셈블리는 Windows 전용 API를 쓰지 못한다

- **GIVEN** `Axicultus.Core`의 코드가 WinForms 형식이나 `System.Drawing.Common`의 형식(예: `Bitmap`, `Font`)을 쓴다
- **WHEN** 솔루션을 빌드한다
- **THEN** `Axicultus.Core`의 컴파일이 실패한다

#### Scenario: 기본 프레임워크에 있는 그리기 형식

- **GIVEN** `Axicultus.Core`가 `System.Drawing.Primitives`의 형식(예: `Color`, `Point`)을 참조한다
- **WHEN** 솔루션의 테스트를 실행한다
- **THEN** 공통 도메인 어셈블리의 참조를 검사하는 테스트가 실패한다
