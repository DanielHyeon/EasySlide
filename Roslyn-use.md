**Roslyn(Microsoft.CodeAnalysis)**은 Microsoft가 공식 제공하는 .NET 컴파일러 플랫폼입니다. 단순한 텍스트 파싱을 넘어, 소스 코드의 **실제 타입 시스템, 의존성 관계, 메서드 오버로딩 등 실제 의미(Semantics)**를 분석할 수 있어 Codex의 디버깅 능력을 극대화할 수 있습니다. [[1](https://www.youtube.com/watch?v=_5fliQotx50), [2](https://learn.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview?view=visualstudio)]

Codex가 유독 C#의 버그 원인을 찾지 못할 때, **Roslyn을 활용해 핵심 '컴파일러 지식'을 추출한 뒤 Codex 프롬프트에 RAG(검색 증강 생성) 형태로 주입하는 방법**을 안내합니다.

---

1. Roslyn의 핵심 구조 개념

Roslyn은 분석을 위해 크게 두 가지 모델을 제공합니다: [[1](https://learn.microsoft.com/ko-kr/dotnet/csharp/roslyn-sdk/get-started/semantic-analysis)]

1. **구문 모델 (Syntax Tree)**: 코드의 구조(클래스 이름, `if`문 위치, 괄호 등)를 분석합니다. (ANTLR과 유사)
2. **의미 체계 모델 (Semantic Model)**: 변수의 **진짜 타입**, 특정 인터페이스를 누가 구현했는지, 메서드가 어디서 호출되는지 등의 **의미와 연결 구조**를 파악합니다. [[1](https://www.youtube.com/watch?v=_5fliQotx50), [2](https://learn.microsoft.com/ko-kr/dotnet/csharp/roslyn-sdk/get-started/semantic-analysis), [3](https://github.com/xamarin/Workbooks/blob/master/csharp/roslyn/roslyn-syntax-trees.workbook/index.workbook), [4](https://www.youtube.com/watch?v=-bBA8WvH-BQ), [5](https://github.com/PositiveTechnologies/PT.Doc/blob/master/Articles/Theory-and-Practice-of-source-code-parsing-with-ANTLR-and-Roslyn/English.md)]

> 💡 **Codex 디버깅의 치트키는 '의미 체계 모델'입니다.**  
> "이 `var data`가 정확히 어떤 타입 객체인가?"를 Roslyn이 완벽히 찾아내어 Codex에게 전달하면 환각 현상이 사라집니다.

---

2. [실전 예제] .NET에서 Roslyn을 활용한 코드 컨텍스트 추출기

이 C# 콘솔 앱 코드는 특정 C# 파일을 로드한 뒤, **"의존성 주입 관계"와 "변수의 실제 컴파일 타입"**을 정밀하게 추출하여 요약본을 만들어 냅니다. [[1](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-analyzers-and-code-aware-library-for-immutablearrays?view=visualstudio)]

1) NuGet 패키지 설치

프로젝트에 Roslyn 분석 패키지를 설치합니다. [[1](https://stackoverflow.com/questions/30721461/process-encountered-symbols-while-also-having-access-to-a-semanticmodel-in-rosly)]

bash

```
dotnet add package Microsoft.CodeAnalysis.CSharp
```

Use code with caution.

2) C# 소스 코드 분석 스크립트 작성

csharp

```
using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class RoslynAnalyzer
{
    static void Main(string[] args)
    {
        string code = @"
            using System;
            public interface IUserService { void Login(); }
            public class UserService : IUserService { public void Login() { } }

            public class Controller 
            {
                private readonly IUserService _service;
                public Controller(IUserService service) { _service = service; }

                public void Execute() 
                {
                    var user = _service; // <-- 이 변수의 '진짜 타입'은 무엇일까?
                    user.Login();
                }
            }";

        // 1. 구문 트리 생성
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();

        // 2. 가상 컴파일 환경 구성 (가장 중요: 시스템 라이브러리 참조 연동)
        var compilation = CSharpCompilation.Create("AnalysisComponent")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        // 3. 의미 체계 모델 획득
        SemanticModel semanticModel = compilation.GetSemanticModel(tree);

        Console.WriteLine("=== [Roslyn이 추출한 Codex용 프롬프트 메타데이터] ===");

        // 예시 A: 클래스 및 상속 관계 추적
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classDecl in classDeclarations)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (classSymbol != null)
            {
                var interfaces = string.Join(", ", classSymbol.AllInterfaces.Select(i => i.Name));
                Console.WriteLine($"[구조] 클래스: {classSymbol.Name} | 구현 인터페이스: {interfaces}");
            }
        }

        // 예시 B: 로컬 변수의 '실제 타입(Semantic Type)' 확인 (var 추론)
        var localVariables = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();
        foreach (var varDecl in localVariables)
        {
            // var user = _service; 에서 'user' 변수 기호 가져오기
            var localSymbol = semanticModel.GetDeclaredSymbol(varDecl) as ILocalSymbol;
            if (localSymbol != null)
            {
                // 'var'로 선언되었지만 Roslyn은 'IUserService'임을 명확히 알고 있음
                Console.WriteLine($"[타입 정보] 변수명: {localSymbol.Name} | 실제 컴파일 타입: {localSymbol.Type}");
            }
        }
    }
}
```

Use code with caution.

3) 출력 결과 (Codex에게 주입할 핵심 힌트가 됨)

text

```
=== [Roslyn이 추출한 Codex용 프롬프트 메타데이터] ===
[구조] 클래스: UserService | 구현 인터페이스: IUserService
[구조] 클래스: Controller | 구현 인터페이스: 
[타입 정보] 변수명: user | 실제 컴파일 타입: IUserService
```

Use code with caution.

---

3. Codex와의 연동 및 활용 시나리오

위 스크립트를 빌드하여 자동화 도구(CLI)로 만든 뒤, 분석할 C# 파일들을 먼저 통과시킵니다. 이후 Codex에 질문할 때 다음과 같이 구조화된 프롬프트를 전달합니다.

> **[Codex에게 줄 RAG 프롬프트 예시]**
> 
> **1. Roslyn이 분석한 컴파일러 정보 (Context)**
> 
> - `Controller` 클래스는 생성자를 통해 `IUserService`를 주입받고 있습니다.
> - `Execute()` 메서드 내부의 `var user` 변수의 실제 추론 타입은 `IUserService` 인터페이스입니다.
> - 현재 솔루션 내에서 `IUserService`를 구현한 구체 클래스는 `UserService`가 유일합니다.
> 
> **2. 대상 소스 코드 (Source Code)**
> 
> csharp
> 
> ```
> // 실제 오류가 나는 코드 영역 부착
> public void Execute() {
>     var user = _service;
>     user.Login(); // 여기서 예외가 발생합니다.
> }
> ```
> 
> Use code with caution.
> 
> **3. 질문 (Instruction)**  
> "Roslyn 컨텍스트를 참고하여, `user.Login()` 호출 시 발생할 수 있는 잠재적 위험 요소를 진단하고 해결책을 제시해 주세요."

이러한 방식으로 진행하면 Codex는 타입 추론에 실패해 헤매는 일 없이, **"의존성 주입 단계에서 인터페이스에 구체 클래스가 바인딩되지 않아** `_service`**가 null일 가능성"**을 정확하게 짚어내게 됩니다.

---
**WinForms에서 WPF로의 1:1 전환** 과정에서 Claude Code나 Codex가 원인을 찾지 못하고 요구사항을 놓치는 이유는 매우 명확합니다.

WinForms는 **‘이벤트 기반(Event-driven)’** 구조로 비하인드 코드(`.cs`)에 UI 제어 로직이 끈적하게 얽혀 있는 반면, WPF는 **‘데이터 바인딩 기반(MVVM 아키텍처)’** 구조를 사용합니다. AI 입장에서는 단순히 텍스트만 읽어서는 WinForms의 델리게이트, 화면 갱신 주기, 마샬링 코드가 WPF의 XAML 가상 트리와 어떻게 매핑되어야 하는지 **구조적 추론**을 하지 못하기 때문입니다. [[1](https://www.reddit.com/r/ClaudeCode/comments/1qobg1g/how_to_refactor_50k_lines_of_legacy_code_without/), [2](https://stackoverflow.com/questions/322612/what-are-the-most-common-mistakes-made-in-wpf-development)]

이 문제를 해결하고 AI의 분석 및 코드 수정 능력을 강제로 끌어올리기 위한 **실전 3단계 대응 전략**을 제시합니다.

---

1단계: 컨텍스트 주입 - 단일 파일이 아닌 ‘관계성’ 주입하기

현재 AI에게 "이 WinForms 코드를 WPF로 바꿔줘"라며 소스 코드만 던져주고 계신다면 AI는 환각(Hallucination)을 일으킵니다. 앞서 논의한 **Code Graph** 도구를 활용해 전체 연결 지도를 먼저 심어주어야 합니다. [[1](https://www.reddit.com/r/ClaudeCode/comments/1qobg1g/how_to_refactor_50k_lines_of_legacy_code_without/)]

- **해결 행동**: 터미널(CLI)에 아래 명령어를 입력하여 Claude Code나 Codex 환경에 즉시 로컬 코드 그래프 엔진을 설치합니다.
    
    bash
    
    ```
    npx @colbymchenry/codegraph
    cd [당신의_C#_프로젝트_폴더]
    codegraph init
    ```
    
    Use code with caution.
    
    [[1](https://discuss.pytorch.kr/t/codegraph-ai/10308)]
- **효과**: 이 도구는 백그라운드에서 SQLite와 트리시터 파서를 사용해 C# 소스 코드의 기호, 메서드 호출 흐름, 클래스 계층 구조를 지도로 굽습니다. 이제 Claude Code가 파일을 검색(Grep/Find)하느라 토큰을 낭비하지 않고, **"이 WinForms 이벤트가 어떤 비즈니스 로직 클래스를 호출하는지"**의 전체 맥락을 파악한 상태에서 WPF 전환을 시작합니다. [[1](https://github.com/colbymchenry/codegraph), [2](https://github.com/colbymchenry/codegraph/blob/main/CLAUDE.md), [3](https://github.com/colbymchenry/codegraph?ref=aitoolnet.com), [4](https://github.com/colbymchenry/codegraph?ref=creativeainews.com)]

---

2단계: 프롬프트 엔지니어링 - ‘WinForms 식 코드 작성’ 금지 규정 선언

AI가 WPF 코드를 짤 때 가장 자주 하는 실수는 **"WPF XAML 비하인드 코드에 WinForms 스타일로 변수를 직접 대입하거나 컨트롤을 조작하는 코드를 짜는 것"**입니다. 이를 프롬프트 수준에서 완전히 차단해야 합니다. [[1](https://stackoverflow.com/questions/322612/what-are-the-most-common-mistakes-made-in-wpf-development), [2](https://stackify.com/7-steps-to-improve-code-quality/)]

질문하실 때 아래의 **제약 조건 템플릿**을 프롬프트 상단에 강제로 주입하세요. [[1](https://www.reddit.com/r/ClaudeAI/comments/1szn9b0/how_to_be_better_than_99_of_claude_code_users/)]

> **[WPF 전환 전용 시스템 프롬프트 가이드]**
> 
> **1. 아키텍처 원칙**
> 
> - 제공된 WinForms의 `Form.cs` 코드를 WPF로 전환할 때, 모든 UI 컨트롤 조작 로직은 비하인드 코드(`.xaml.cs`)에 작성하지 마십시오.
> - 반드시 **MVVM 아키텍처**를 준수하여, 데이터와 상태 관리는 `ViewModel` 클래스로 분리하고 XAML에서 `Binding` 하도록 설계하세요.
> 
> **2. 1:1 매핑 변환 규칙 지정**
> 
> - `MessageBox.Show` ──> WPF 스타일 메세지 서비스로 추상화
> - `Control.Invalidate()` 또는 `Refresh()` ──> `INotifyPropertyChanged` 인터페이스 구현을 통한 데이터 바인딩 트리거로 변환
> - `BackgroundWorker` ──> `async / await (Task)` 기반 비동기 패턴으로 현대화
> 
> **3. 미션**  
> "현재 [특정 안 되는 부분의 함수명] 부분에서 컴파일 에러 혹은 데이터 갱신이 누락되는 문제가 있습니다. 위 규칙을 기반으로 WinForms와 WPF의 생명주기(Lifecycle) 차이를 고려하여 원인을 진단하고 수정된 XAML과 C# 코드를 분리해 제시해 주세요."
> 
> [[1](https://stackoverflow.com/questions/322612/what-are-the-most-common-mistakes-made-in-wpf-development)]

---

3단계: 로슬린(Roslyn) MCP 서버로 타입 추론 강제 연동

만약 1, 2단계를 거쳐도 복잡한 제네릭(Generics), 델리게이트 인터페이스, 혹은 타사(3rd-party) 라이브러리가 얽혀 있어 AI가 엉뚱한 컴파일 에러 코드를 뱉는다면, 컴파일러 엔진인 **Roslyn MCP**를 Claude Code에 마운트해야 합니다. [[1](https://github.com/JoshuaRamirez/RoslynMcpServer)]

- **해결 행동**: Claude Code 및 에이전트의 설정 파일(`claude_desktop_config.json` 등)에 공식 C# 리팩토링 MCP인 [**JoshuaRamirez/RoslynMcpServer**](https://github.com/JoshuaRamirez/RoslynMcpServer)를 연동합니다.
- **효과**: AI가 단순 텍스트가 아니라, Visual Studio 컴파일러가 바라보는 **"실제 데이터 타입 정의"**와 **"종속성 파이프라인"** 정보를 MCP 툴을 통해 실시간으로 조해합니다. "이 인터페이스가 왜 WPF 런타임에서 바인딩 오류(Binding Error)를 내는지" 그 원인을 정확히 컴파일러 레벨에서 짚어낼 수 있게 됩니다. [[1](https://github.com/JoshuaRamirez/RoslynMcpServer)]

---

💡 지금 바로 시작해야 할 액션 플랜

1. 지금 즉시 작업 단위를 거대하게 잡지 마시고, **"단 하나의 화면(Form) 또는 단 하나의 복잡한 메서드"** 단위로 잘라서 세션을 새로 시작하세요. (컨텍스트 누적 오류 방지) [[1](https://www.reddit.com/r/ClaudeCode/comments/1qobg1g/how_to_refactor_50k_lines_of_legacy_code_without/), [2](https://www.reddit.com/r/ClaudeCode/comments/1ol1pjp/looking_for_best_practices_working_with_a_larger/)]
2. 터미널에 `npx @colbymchenry/codegraph`를 실행해 AI에게 프로젝트 전반의 길잡이를 제공해 줍니다. [[1](https://github.com/colbymchenry/codegraph), [2](https://discuss.pytorch.kr/t/codegraph-ai/10308)]
3. AI에게 코드를 수정하라고 지시하기 전에, **"이 WinForms 코드 로직이 WPF에서 왜 비정상 동작하는지 원인부터 추론해서 보고서로 제출해줘"**라고 분석 작업만 먼저 명하시기 바랍니다.

현재 전환 과정에서 **가장 골칫거리가 되고 있는 특정 WinForms 컴포넌트나 코드 패턴**(예: `Grid` 데이터 바인딩, 멀티스레딩 UI 갱신 등)이 무엇인지 구체적으로 말씀해 주시면, 그 로직을 WPF(MVVM)로 매끄럽게 컴파일하기 위해 AI에게 내려야 할 맞춤형 지시문을 직접 작성해 드리겠습니다.