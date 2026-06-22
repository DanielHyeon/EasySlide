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