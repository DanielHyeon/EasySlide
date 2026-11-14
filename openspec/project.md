# Project Context

## Purpose

EasiSlides는 교회 예배용 프레젠테이션 프로그램이다. 가사, 성경 구절, PowerPoint 슬라이드를 멀티 모니터로 송출한다.
자세한 구조와 규칙은 `.claude/CLAUDE.md`를 따른다. 이 문서는 OpenSpec 변경 제안에 공통으로 적용되는 조건만 적는다.

## Tech Stack

- C# / .NET (`net11.0-windows7.0`), Windows Forms, `WinExe`
- 메인 폼 `FrmMain`은 partial 파일로 나뉜다: `FrmMain.cs`, `.Designer.cs`, `.Fields.cs`, `.Events.cs`, `.Layout.cs`, `.Logic.cs`
- 빌드: `dotnet build Easislides\Easislides.csproj -nologo -v minimal`
- git 실행 파일은 PATH에 없다. Visual Studio에 포함된 git을 전체 경로로 호출한다.
  `C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe`

## Conventions (요약, 원문은 `.claude/CLAUDE.md` §9)

- 식별자와 코드 주석은 영어로 쓴다. 문서(*.md)와 사용자에게 보이는 문구는 한글을 쓸 수 있다.
- 작은 단위로 커밋한다. `.bak` 파일은 지우지 않는다.
- 새 메서드는 알맞은 `Global/gf*.cs`를 먼저 찾아 넣고, 없을 때만 새 파일을 만든다.
- NetOffice, DirectShow 리소스는 명시적으로 해제한다.
- 심볼 탐색은 CodeGraph를 먼저 쓴다 (`.claude/CLAUDE.md` §12).

## Constraints

- 이 프로그램은 예배 중에 실시간으로 쓰인다. 화면 모양을 바꾸는 변경은 기존 동작(클릭, 더블클릭, 끌어 놓기, 단축키)을 바꾸지 않아야 한다.
- 원격 저장소(`origin/main`)에는 WPF 전환 작업이 따로 진행돼 있다. 이 저장소 폴더의 변경은 WinForms `FrmMain`에 한정하고, 나중에 합칠 때 충돌을 줄이도록 `FrmMain.Designer.cs`의 수정을 최소화한다.
- 사용자의 로컬 수정(`*.csproj` 3개의 대상 프레임워크 변경)은 커밋에 포함하지 않는다.

## Working Agreement (역할)

| 역할 | 담당 | 하는 일 |
| --- | --- | --- |
| 총괄 아키텍트 | Fable | 작업 분할, OpenSpec 작성과 검토, 완료 검증. 코드를 직접 고치지 않는다 |
| 구현 에이전트 | Opus | 작업 묶음 하나의 코딩과 빌드 확인 |
| 적대적 리뷰 에이전트 | Opus, 구현 에이전트와 별개 | 구현 결과에서 결함을 찾는다. 구현자의 설명을 믿지 않고 코드와 실행 결과로 판단한다 |
| 내장 코드 리뷰 | `/code-review` | 적대적 리뷰가 끝난 뒤 한 번 더 본다 (gstack 리뷰 대체) |

작업 묶음 하나의 절차: 구현 → 적대적 리뷰 → 지적 사항 수정 → 내장 코드 리뷰 → 총괄의 화면 캡처 검사와 완료 검증 → 커밋.
한 번에 한 묶음만 코드를 고친다. `FrmMain` 파일들을 여러 묶음이 함께 건드리기 때문이다.
