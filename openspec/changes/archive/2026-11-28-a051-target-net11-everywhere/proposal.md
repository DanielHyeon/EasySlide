# 남은 프로젝트를 .NET 11로 올리기

## Why

사용자가 .NET 11 SDK로 올릴 것을 요청했다(2026-11-28). 솔루션의 앱과 라이브러리는 이미 `net11.0-windows`이고, 개발 PC에는 SDK 11.0.100과 런타임 11.0.0만 설치되어 있다. .NET 10에 남은 것은 둘이다.

- `Axicultus.Core`(`net10.0`): 두 앱이 함께 쓰는 도메인 어셈블리. .NET 11 앱 안에서 .NET 11 런타임으로 돌지만 컴파일은 .NET 10 참조 어셈블리로 한다. `LangVersion=latest` 때문에 SDK 11에서는 C# 15로 컴파일되어, SDK 10만 있는 곳에서는 같은 소스가 다른 언어 버전으로 컴파일된다.
- `Axicultus.Analyzers.Tests`(`net10.0`): 분석기 테스트. .NET 10 런타임이 있어야 실행된다. CI는 이것 때문에 SDK 10과 11을 둘 다 설치한다.

사용자 흐름: 개발과 빌드. 예배 중 동작은 바뀌지 않는다.

## What Changes

- `Axicultus.Core`와 `Axicultus.Analyzers.Tests`의 대상을 `net11.0`으로 바꾼다(Core는 지금처럼 Windows 전용이 아닌 대상이다).
- CI는 .NET 11 SDK만 설치한다.
- .NET 10 → 11의 호환성 변경 목록(공식 마이그레이션 스킬 `migrate-dotnet10-to-dotnet11`)을 두 프로젝트에 대조해, 걸리는 것이 있으면 고친다(걸리는 것은 없었다).
- 대상을 바꾼 두 프로젝트에서 `LangVersion=latest`를 지운다(`net11.0`의 기본값 C# 15와 같다).
- 테스트를 더한다: Core의 대상이 Windows 전용이 아닌 `net11.0`임을 확인하는 테스트(다음에 대상을 올릴 때 이 테스트의 버전도 함께 고친다), 대상 변경으로 생성 코드가 달라진 찬송가 번호 정규식의 동작을 고정하는 테스트.
- 대상이 `net10.0`이라고 적은 코드 주석을 고치고, ADR 0008에 한 줄을 덧붙인다.

## Non-goals

- `Axicultus.Analyzers`(`netstandard2.0`): Roslyn 분석기는 이 대상이어야 컴파일러에 로드된다.
- 솔루션 밖의 `SQLiteTestConsole`(`net8.0`), `GetOffice`(.NET Framework 4.7.2).
- `global.json`으로 SDK 버전 고정(모든 대상이 `net11.0`이면 옛 SDK는 명확한 오류로 막힌다).
- C# 15 문법으로 코드 고쳐 쓰기, 패키지 버전 올리기(대상 변경 때문에 꼭 필요한 것만).
- 문서(`.claude/CLAUDE.md`)의 옛 구조 설명 고치기.

## Impact

- 영향: `Axicultus.Core\Axicultus.Core.csproj`, `Axicultus.Analyzers.Tests\Axicultus.Analyzers.Tests.csproj`, `.github\workflows\ci.yml`, 테스트(`Axicultus.Tests\Startup\CoreExtractionTests.cs`, 새 `Axicultus.Tests\Library\PowerPointSourceFilesTests.cs`), 주석만 바뀌는 파일(`Axicultus.Core`의 세 파일, `Axicultus.Analyzers.csproj`), `docs\adr\0008-…`.
- 컴파일 결과: Core의 참조는 그대로이고, 생성된 정규식 코드(`PowerPointSourceFiles.HymnPrefix`)만 달라진다. 결과는 같다(design 7).
- 배포: 두 앱의 실행 파일 폴더에 들어가는 `Axicultus.Core.dll`이 .NET 11 대상으로 바뀐다. 앱은 이미 .NET 11 런타임을 요구하므로 방송 PC에 더 필요한 것은 없다.
- 라이브 송출, Office 연동, DB, 모니터 배치는 바뀌지 않는다.

## Capabilities

### New Capabilities

- `build-targets`: 솔루션 프로젝트의 대상 프레임워크와 필요한 SDK.
