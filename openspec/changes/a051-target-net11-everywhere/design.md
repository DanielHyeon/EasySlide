# Design — a051 남은 프로젝트를 .NET 11로

## 현재 구조 (영향 근거)

2026-11-28 조사(읽기만 함).

- 설치: SDK 11.0.100, 런타임 `Microsoft.NETCore.App`·`WindowsDesktop.App`·`AspNetCore.App` 11.0.0. .NET 10 런타임은 없다. `global.json`, `Directory.Build.props`는 없다.
- `net10.0`: `Axicultus.Core`(`LangVersion=latest`, `Nullable=enable`, 패키지 참조 없음, 리소스 생성 설정 있음), `Axicultus.Analyzers.Tests`(xunit 2.9.3, `Microsoft.NET.Test.Sdk` 17.13.0, `Microsoft.CodeAnalysis.CSharp` 4.8.0).
- `netstandard2.0`: `Axicultus.Analyzers`. 그 밖의 솔루션 프로젝트는 모두 `net11.0-windows…`.
- `Axicultus.Core`를 참조하는 것: 두 앱, 자막 라이브러리, 테스트, 하네스(모두 `net11.0-windows`). Core의 csproj 주석: 대상을 Windows 전용이 아닌 것으로 두어 WinForms·`System.Drawing` 결합을 컴파일 단계에서 막는다.
- CI(`.github\workflows\ci.yml`): `actions/setup-dotnet`이 `10.0.x`와 `11.0.x`를 설치한다(주석: 분석기 테스트가 .NET 10 런타임을 쓴다).
- 총괄의 검증은 지금까지 `Axicultus.Tests`와 `Axicultuss.Tests`만 돌렸다. `Axicultus.Analyzers.Tests`가 이 PC에서 지금 실행되는지는 확인되지 않았다(런타임 10이 없다).

## 결정

1. `Axicultus.Core`: `net10.0` → `net11.0`. `-windows`를 붙이지 않는다(주석의 뜻을 지킨다). 주석의 "net10.0" 글자는 고친다.
2. `Axicultus.Analyzers.Tests`: `net10.0` → `net11.0`. 패키지 버전은 빌드와 실행에 필요할 때만 올린다.
3. CI: `11.0.x`만 설치하고 주석을 고친다.
4. `LangVersion=latest`는 그대로 둔다(분석기 프로젝트는 필요하고, 나머지는 이제 기본값과 같아 해가 없다. 지우는 것은 이 변경의 일이 아니다).
5. 호환성 변경 대조: 공식 스킬 `migrate-dotnet10-to-dotnet11`(플러그인 `dotnet-upgrade`)의 변경 목록을 두 프로젝트의 소스에 대조한다. Core는 이미 런타임 11에서 돌고 있으므로 런타임 동작 변경은 새로 생기지 않는다. 볼 것은 컴파일 때 달라지는 것(참조 어셈블리의 API 변경, 새 경고, 오버로드 선택)이다. 걸리는 것이 없으면 "없음"을 근거와 함께 적는다.
6. 확인 방법: 바꾸기 전에 `Axicultus.Analyzers.Tests`가 이 PC에서 실행되는지 먼저 본다(실행되지 않으면 그 출력이 "고치기 전 실패"다). 바꾼 뒤 솔루션의 테스트 프로젝트 셋을 모두 돌린다. 경고 수는 바꾸기 전후를 `--no-incremental` 빌드로 비교한다.

## 적대적 리뷰 뒤의 결정 (2026-11-28)

7. **컴파일 결과가 다른 곳은 생성된 정규식 하나다.** 리뷰어가 같은 소스를 net10·net11로 빌드해 비교했다: 형식·멤버 참조는 같고, 메서드 IL은 `Axicultus.Core\PowerPointSourceFiles.cs`의 `[GeneratedRegex]`(`HymnPrefix`)가 만든 코드만 다르다(생성기 버전 10.0 → 11.0, 앞보기 안의 되돌리기 위치 저장이 추가됨). 4,000,719개 입력에서 net10 생성 코드·net11 생성 코드·해석기의 결과가 모두 같았다. 기존 테스트는 "N장" 꼴만 본다. 앞보기 가지("40 찬송…", "123_제목" 같은 꼴)의 테스트를 넣어 동작을 고정한다.
8. **Core의 대상은 테스트로 고정한다.** `CoreExtractionTests`에 Core 어셈블리의 대상이 `.NETCoreApp,Version=v11.0`이고 Windows 전용 표시(대상 플랫폼 특성)가 없다는 확인을 넣는다. 지금은 Core를 `net11.0-windows`로 바꿔도 실패하는 테스트가 없다.
9. **결정 4를 바꾼다**: 이번에 대상이 바뀐 두 프로젝트(`Axicultus.Core`, `Axicultus.Analyzers.Tests`)에서 `LangVersion=latest`를 지운다. `net11.0`의 기본값이 C# 15라 지금 결과는 같고, 남겨 두면 다음 SDK에서 이 둘만 다른 언어 버전으로 컴파일된다(proposal의 Why와 같은 문제). 다른 프로젝트의 것은 건드리지 않는다.
10. **이제 틀린 주석을 고친다**: 대상이 `net10.0`이라고 적은 코드 주석(`Axicultus.Core\SongFormat.cs`, `SongLyricsBase.cs`, `SongSettingsBase.cs`, `Axicultus.Analyzers\Axicultus.Analyzers.csproj`, `CoreExtractionTests.cs`)과 CI 설치 단계의 주석. 고치는 줄은 영어로 쓴다. `docs\` 아래의 결정 기록과 계획서는 그때의 기록이므로 두고, ADR 0008에 "a051에서 `net11.0`으로 올림" 한 줄만 덧붙인다.

## 위험과 대응

- **Core의 오버로드 선택이 달라짐**: 참조 어셈블리가 바뀌면 같은 호출이 새 오버로드로 묶일 수 있다. Core를 쓰는 테스트 전체(`Axicultus.Tests` 약 3,860개, `Axicultuss.Tests` 약 495개)와 두 하네스로 확인한다.
- **SDK 10만 있는 곳에서 빌드 불가**: 의도한 결과다. 이 저장소의 앱은 이미 그렇다.
- **CI**: SDK 10을 빼면 분석기 테스트가 .NET 11에서 돈다. CI는 이 세션에서 돌릴 수 없으므로 로컬에서 같은 명령(`dotnet restore`, `build`, `test`)으로 확인하고, 첫 CI 실행을 사람이 확인할 것으로 남긴다.
