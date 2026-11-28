전제: a050이 커밋되어 있다(`feat/a047-winforms-camera-captions`의 HEAD). 다른 묶음과 동시에 하지 않는다.

## Phase 0. 기준

- [x] 0.1 브랜치 `feat/a051-target-net11-everywhere`. `dotnet build Axicultus.sln -c Debug --no-incremental`의 오류·경고 수, 테스트 프로젝트 셋(`Axicultus.Tests`, `Axicultuss.Tests`, `Axicultus.Analyzers.Tests`)의 결과 기록. 분석기 테스트가 실행되지 않으면 그 출력을 기록
- [x] 0.2 design "현재 구조"를 지금 저장소와 대조(`net10` 대상이 더 있는지, `net10`·`10.0`을 가리키는 설정·스크립트·문서가 더 있는지)

## Phase 1. 올리기

- [x] 1.1 `Axicultus.Core`와 `Axicultus.Analyzers.Tests`의 대상 변경(design 1, 2)
- [x] 1.2 CI의 SDK 설치(design 3)
- [x] 1.3 `migrate-dotnet10-to-dotnet11`의 변경 목록 대조와 필요한 고치기(design 5)
- Constraints: 범위 밖의 코드를 고치거나 현대화하지 않는다. 패키지 버전은 필요할 때만. 식별자·주석 영어(기존 한글 주석은 그 줄을 고칠 때만 영어로).

## Phase 2. 회귀와 빌드

- [x] 2.1 Debug·Release `--no-incremental` 빌드(저장소 bin), 경고 수를 Phase 0과 비교(새 경고는 목록으로)
- [x] 2.2 테스트 프로젝트 셋 전체
- [x] 2.3 두 하네스 드라이버 빌드(`tools\AxicultusHarness`, `tools\AxicultussHarness`)

## Phase 3. 리뷰와 기록

- [x] 3.1 적대적 리뷰(구현자와 다른 에이전트) 지적 사항 처리
- [x] 3.2 `/code-review`
- [x] 3.3 총괄 완료 검증(하네스 포함)과 "검증 결과" 기록. 사람이 확인할 것: 첫 CI 실행
- [x] 3.4 커밋·푸시, 아카이브(새 `openspec/specs/build-targets/spec.md`)

## 검증 결과

구현자 기록 (2026-11-28, SDK 11.0.100 / 런타임 11.0.0만 설치된 PC)

- 0.1 기준(d5ce5aa, 바꾸기 전): Debug `--no-incremental` 오류 0, 경고 1805(중복 제거 기준, Core·분석기 테스트에서 나온 경고 0). `Axicultus.Tests` 3863 통과 / 22 건너뜀 / 0 실패, `Axicultuss.Tests` 495 / 2 / 0. `Axicultus.Analyzers.Tests`는 실행 안 됨: testhost가 `Microsoft.NETCore.App 10.0.0`을 찾지 못함("You must install or update .NET", 11.0.0만 있음) → "Test Run Aborted", 종료 코드 1.
- 0.2 대조: 솔루션에서 `net10.0` 대상은 design대로 둘뿐. `global.json`·`Directory.Build.*`·`Directory.Packages.props` 없음. 빌드 설정·스크립트(ps1)·하네스 csproj에 `net10` 출력 폴더를 가리키는 것 없음. 남은 `net10.0` 글자는 주석뿐(`Axicultus.Analyzers.csproj` 6행, `Axicultus.Core\SongFormat.cs`·`SongLyricsBase.cs`·`SongSettingsBase.cs`, `Axicultus.Tests\Startup\CoreExtractionTests.cs` 99행)과 옛 문서(`docs\…`, 보관된 openspec) — 범위 밖이라 그대로 둠.
- 1.1/1.2 변경: Core `net11.0`(주석 한 줄 영어로, `System.Drawing.Primitives`는 기본 프레임워크에 있어 컴파일로 막히지 않고 `CoreExtractionTests`가 막는다는 점을 적음), 분석기 테스트 `net11.0`, CI는 `11.0.x`만. 패키지 버전 변경 없음(빌드·실행에 필요 없었음).
- 1.3 호환성 대조: 스킬 참조 목록 csharp-compiler·core-libraries·sdk-msbuild(항상), runtime-jit·cryptography(확인용). 두 프로젝트 소스 24개에서 Span 컬렉션 식, `ref readonly`, `nameof(this.)`, 컬렉션 안 `with(`, `dynamic`, switch 팔의 `) when`, NamedPipe, 압축·TAR·MemoryStream·TickCount·Calendar·BackgroundService·암호화 검색 → 해당 없음(Span은 문자열에서 받는 매개변수·지역변수, `when`은 catch 필터). 오버로드 선택: 같은 소스로 net10.0/net11.0 빌드한 `Axicultus.Core.dll`의 외부 참조(메서드 360, 필드 48, 형식 157, 제네릭 인스턴스 40)는 같음. 분석기 테스트 dll도 298개 같음. 다만 IL은 같지 않다: 생성된 정규식 `HymnPrefix`(`PowerPointSourceFiles.cs`)만 다름(생성기 10.0.15.3008 → 11.0.15.3018, 리뷰어가 찾음, design 7). 참조 비교로는 IL 차이를 볼 수 없었음. 고칠 것 없음.
- 2.1 바꾼 뒤: Debug·Release `--no-incremental` 오류 0, 경고 1805, 기준 대비 새 경고 0 / 사라진 경고 0. 바꾼 뒤 첫 Debug 빌드 한 번은 `Axicultus\Library\SongDeleteWindow.xaml.cs(12,9) CS0103 InitializeComponent`(wpftmp 프로젝트)로 실패했으나 그 뒤 깨끗한 빌드 6번(net10↔net11 되돌렸다 다시 올리기 포함)에서 재현되지 않음 — 일시적 XAML 컴파일 경쟁으로 봄.
- 2.2 바꾼 뒤 테스트: `Axicultus.Tests` 3863 / 22 / 0, `Axicultuss.Tests` 495 / 2 / 0, `Axicultus.Analyzers.Tests` 20 / 0 / 0(net11.0에서 처음 실행됨).
- 2.3 하네스 드라이버 빌드: 둘 다 오류 0(경고 126, 8; Core에서 나온 경고 없음). 실행은 안 함.
- 산출물: 두 앱·두 테스트·두 하네스 출력 폴더의 `Axicultus.Core.dll`이 모두 `.NETCoreApp,Version=v11.0`. 옛 `bin\…\net10.0`·`obj\…\net10.0` 폴더(Core, 분석기 테스트, `artifacts\legacy-build-cache`)는 디스크에 남아 있으나 참조하는 프로젝트 없음. 지우지 않음.
- 적대적 리뷰 뒤(design 7-10) 구현자 기록:
  - 7(코드 리뷰 뒤 27개로 늘림: 접두어 5종마다 `장` 줄과 구분자 줄, 이름이 `장`·구분자로 끝나는 "40장"·"40_". "찬5장"처럼 접두어+번호+`장`으로 끝나는 이름은 대조 이름 "찬5장가"가 곡 번호로 읽혀 표현할 수 없어 뺌, 테스트 주석에 적음): 새 `Axicultus.Tests\Library\PowerPointSourceFilesTests.cs` 테스트 처음 21개(공개 `PowerPointSourceFiles.Sort`로 번호와 제목을 확인). 끝(`$`), 공백, U+3000(`\s`에 포함), `_`, `.`, `-`, 앞 공백, 구분자 여러 개, `장`(앞 공백 있음·없음), 접두어 5종(새찬송가·통일찬송가·찬송가·찬송·찬), 숫자 뒤 바로 글자(번호 아님), 숫자 없는 접두어. Core를 net10.0(생성기 10.0)으로 빌드해도 21개 통과, net11.0에서도 통과. 앞보기에서 `_`를 뺀 변형은 `_` 세 줄이 잡음(되돌림).
  - 8: `CoreExtractionTests.Core_Targets_Portable_Net11` — `TargetFrameworkAttribute`가 `.NETCoreApp,Version=v11.0`, `TargetPlatform`·`SupportedOSPlatform` 특성 없음. Core를 `net11.0-windows7.0`으로 바꾸면 `TargetPlatformAttribute`가 있어 실패, net10.0이면 "v10.0"으로 실패. 되돌린 뒤 `--no-incremental`로 다시 빌드.
  - 9: Core·분석기 테스트의 `LangVersion=latest` 지움. `#error version` 임시 파일로 확인: 전 "latest (15.0)", 후 "15.0"(컴파일러 5.12.0). 임시 파일은 지움.
  - 10: `SongFormat.cs` 6-7행, `SongLyricsBase.cs` 6-7행(문장이 두 줄에 걸쳐 함께 고침), `SongSettingsBase.cs` 7행, `Axicultus.Analyzers.csproj` 6행, `CoreExtractionTests.cs` 99-100행, CI 설치 단계 주석을 영어로. ADR 0008 끝에 한 줄 덧붙임.
  - 다시 확인: Debug·Release `--no-incremental` 오류 0, 경고 1805(기준 대비 새 0 / 사라짐 0). `Axicultus.Tests` 3885 / 22 / 0(새 22개; 코드 리뷰 뒤 줄 6개 더해 Debug `--no-incremental` 경고 1805·새 0, 3891 / 22 / 0), `Axicultuss.Tests` 495 / 2 / 0, `Axicultus.Analyzers.Tests` 20 / 0 / 0. 하네스 드라이버 둘 다 오류 0. 출력 폴더의 Core는 `v11.0`, 정규식 생성기 11.0.15.3018.
- 사람이 확인할 것: 첫 CI 실행에서 setup-dotnet 단계가 11.0.x만 설치하는지, `dotnet test Axicultus.sln` 출력에 `Axicultus.Analyzers.Tests.dll (net11.0)` 20개 통과가 있는지.

총괄 검증(2026-11-28):

- 적대적 리뷰(별도 에이전트): 치명·주요 0. .NET 10에 기대는 것이 더 없음, 두 앱 출력 폴더의 파일 목록·deps.json·runtimeconfig가 바꾸기 전후 같음, CI와 같은 순서가 이 PC에서 깨끗이 도는 것을 확인. 구현자의 "컴파일 결과가 같다"는 틀렸다: 생성된 정규식 코드(`HymnPrefix`)의 IL이 다르다. 4,000,719개 입력에서 결과는 같았고, 그 가지를 고정하는 테스트를 넣었다(design 7).
- `/code-review`: 결함 0, 사소 4. 테스트 행 보강과 제안서 문구는 처리. `.serena/`를 `.gitignore`에 넣자는 지적은 저장소 설정 변경이라 사용자에게 물었다.
- 총괄이 직접 돌린 결과: Debug·Release `--no-incremental` 오류 0, 경고 1805(바꾸기 전과 같음). `dotnet test Axicultus.sln`(CI와 같은 방식): `Axicultus.Analyzers.Tests` 20 통과(net11.0, 이 PC에서 처음 실행됨), `Axicultuss.Tests` 495 통과·2 건너뜀, `Axicultus.Tests` 3891 통과·22 건너뜀·실패 0. 두 앱의 Debug·Release `Axicultus.Core.dll`이 `.NETCoreApp,Version=v11.0`. `openspec validate --all`(CLI 1.4.1, CI와 같은 버전) 63개 통과.
- 하네스: Axicultuss `smoke`(`a051-smoke.log`) 종료 코드 0, 실제 폴더 unchanged. WPF `pptcaption,captiontab` 두 번. 첫 실행(`a051-verify.log`)은 자막 출력 크기를 재는 11줄이 a050 실행과 달랐다(가로 1152 → 461, 비율은 같음): 실행 도중 출력 모니터가 1024 폭으로 바뀐 것으로, 화면 끄기 시간(15분) 동안 입력이 없어 모니터가 절전에 들어간 것으로 본다. 다시 돌린 실행(`a051-verify2.log`)은 크기 줄이 a050과 모두 같고(/1152 6줄, /461 0줄), 실제 데이터 unchanged, `captiontab` 16 PASS·0 FAIL, `[AIR]` 67줄 동일.
- 하네스에서 본 것(이 변경과 무관, 남은 결함): 자막 탭을 처음 열 때의 복구 안내가 실행마다 보였다 안 보였다 한다. 앞선 실행의 복구가 있는데도 첫 화면 맞춤이 상태 줄을 지우면 안내가 사라진다(a050 design 23의 조사에서 본 경쟁. 복구 버튼은 켜져 있다). 뒤의 변경에서 고친다.
- 다른 PC에서 이 커밋을 받아 처음 빌드할 때 `InitializeComponent`를 찾지 못한다는 오류가 한 번 나면 다시 빌드한다(구현 중 한 번 봤고 깨끗한 빌드 16번에서는 재현되지 않았다).
- 사람이 확인할 것: 푸시 뒤 첫 CI 실행(SDK 11.0.x만 설치, 분석기 테스트 20개가 net11.0에서 통과).
