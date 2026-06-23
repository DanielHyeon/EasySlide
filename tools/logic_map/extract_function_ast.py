#!/usr/bin/env python3
"""
extract_function_ast.py — C# 메서드 내부 구조 추출기 (tree-sitter-c-sharp 기반)

목적
  C# 소스에서 대상 메서드 하나의 내부 구조를 tree-sitter로 기계 추출해
  SDD Function-Level Logic Analysis용 `function-ast-summary.md`를 생성한다.
  (CLAUDE.md `## 함수 내부 로직 분석 레이어` 4절 참조)

추출 대상 (메서드 본문 한정, 중첩 람다/로컬함수/중첩 타입 제외)
  - parameters                  파라미터
  - branch conditions           if / switch / switch_section(case) / ternary / && / ||
  - return statements           return (expression-bodied `=> expr`는 암묵 return으로 합성)
  - assignments                 assignment / variable_declaration
  - type conversions            cast / as / object creation / Convert.* / *.Parse 등
  - invocations                 invocation / await / object creation
  - mutations / side effects    assignment + DB/log/COM/저장 마커 호출
  - try / catch / finally / throw
  - loops                       for / foreach / while / do

경계 (중요)
  이 스크립트는 Roslyn semantic model이 아니다. 타입 추론, symbol resolution,
  DI concrete class 추적, extension method/overload 해석, LINQ/async 실제 의미는
  하지 않는다. 그 부분은 "불확실"로 두고 CodeGraph / Roslyn 2차 분석으로 넘긴다.
  역할은 오직 "AI가 branch/return/assignment/mutation/conversion을 놓치지 않도록
  구조적 체크리스트를 만들어 주는 것"이다.

설치
  pip install tree-sitter tree-sitter-c-sharp

사용
  python tools/logic_map/extract_function_ast.py \
    --file Easislides/Global/gfDisplay.cs \
    --method FormatText \
    --out openspec/changes/<change-id>/analysis/function-ast-summary.md

  # 오버로드/동명 메서드가 여러 개면 클래스명으로 한정
  python tools/logic_map/extract_function_ast.py \
    --file Easislides/Global/gfDisplay.cs \
    --method gfDisplay.FormatText \
    --out openspec/changes/<change-id>/analysis/function-ast-summary.md

검증된 grammar: tree-sitter-c-sharp 0.23.5 / tree-sitter 0.25.2 (2026-06-23)
"""

from __future__ import annotations

import argparse
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Optional

from tree_sitter import Language, Node, Parser
import tree_sitter_c_sharp as tscsharp


# ---------------------------------------------------------------------------
# 자료구조
# ---------------------------------------------------------------------------
@dataclass
class Finding:
    kind: str
    text: str
    line: int
    column: int


# 본문을 걷다가 만나면 "그 안쪽으로는 들어가지 않는" 노드들.
# 중첩 메서드/로컬함수/람다/중첩 타입의 내부 로직을 대상 메서드 본문과 섞지 않는다.
NESTED_DEFINITION_NODES = {
    "method_declaration",
    "local_function_statement",
    "class_declaration",
    "struct_declaration",
    "record_declaration",
    "record_struct_declaration",
    "interface_declaration",
    "enum_declaration",
    "lambda_expression",
    "anonymous_method_expression",
}

CLASS_LIKE_NODES = {
    "class_declaration",
    "struct_declaration",
    "record_declaration",
    "record_struct_declaration",
    "interface_declaration",
}

METHOD_LIKE_NODES = {
    "method_declaration",
    "constructor_declaration",
    "destructor_declaration",
    "operator_declaration",
    "conversion_operator_declaration",
    "local_function_statement",
}


# ---------------------------------------------------------------------------
# tree-sitter 헬퍼
# ---------------------------------------------------------------------------
def make_csharp_parser() -> Parser:
    """C# 파서 생성 (tree-sitter 0.25 / 0.21 양쪽 API 호환)."""
    language = Language(tscsharp.language())
    try:
        return Parser(language)  # tree-sitter >= 0.22
    except TypeError:
        parser = Parser()  # 구버전 폴백
        parser.language = language
        return parser


def node_text(source: bytes, node: Node) -> str:
    return source[node.start_byte : node.end_byte].decode("utf-8", errors="replace")


def one_line(text: str, max_len: int = 240) -> str:
    compact = " ".join(text.strip().split())
    if len(compact) > max_len:
        return compact[: max_len - 3] + "..."
    return compact


def line_col(node: Node) -> tuple[int, int]:
    return node.start_point[0] + 1, node.start_point[1] + 1


def field(node: Node, name: str) -> Optional[Node]:
    try:
        return node.child_by_field_name(name)
    except Exception:
        return None


def walk_all(node: Node) -> Iterable[Node]:
    """노드 전체를 깊이우선으로 순회 (메서드 탐색용)."""
    yield node
    for child in node.children:
        yield from walk_all(child)


def walk_body(node: Node) -> Iterable[Node]:
    """대상 메서드 본문만 순회 — 중첩 정의(람다/로컬함수/중첩타입) 안쪽은 건너뜀."""
    yield node
    for child in node.children:
        if child.type in NESTED_DEFINITION_NODES:
            continue
        yield from walk_body(child)


# ---------------------------------------------------------------------------
# 메서드 찾기
# ---------------------------------------------------------------------------
def enclosing_class_name(source: bytes, node: Node) -> Optional[str]:
    """node의 가장 가까운 상위 클래스/구조체/레코드 이름. (node.parent로 위로 탐색)"""
    current = node.parent
    while current is not None:
        if current.type in CLASS_LIKE_NODES:
            name_node = field(current, "name")
            if name_node is not None:
                return node_text(source, name_node)
        current = current.parent
    return None


def method_name_of(source: bytes, node: Node) -> Optional[str]:
    name_node = field(node, "name")
    if name_node is None:
        for child in node.children:
            if child.type == "identifier":
                name_node = child
                break
    return node_text(source, name_node) if name_node is not None else None


def find_method(source: bytes, root: Node, method_spec: str) -> Optional[Node]:
    """`Method` 또는 `Class.Method` 형식으로 단일 메서드/생성자를 찾는다."""
    wanted_class: Optional[str] = None
    wanted_method = method_spec
    if "." in method_spec:
        parts = method_spec.split(".")
        wanted_class = parts[-2]
        wanted_method = parts[-1]

    candidates: list[Node] = []
    for node in walk_all(root):
        if node.type not in METHOD_LIKE_NODES:
            continue
        if method_name_of(source, node) != wanted_method:
            continue
        if wanted_class and enclosing_class_name(source, node) != wanted_class:
            continue
        candidates.append(node)

    if len(candidates) == 1:
        return candidates[0]

    if len(candidates) > 1:
        locations = ", ".join(
            f"{(enclosing_class_name(source, c) or '?')} @L{c.start_point[0] + 1}"
            for c in candidates
        )
        print(
            f"ERROR: '{method_spec}' 가 여러 개({len(candidates)}) 발견됨: {locations}. "
            "ClassName.MethodName 으로 한정하세요.",
            file=sys.stderr,
        )
        return None

    return None


def method_body(node: Node) -> Optional[Node]:
    """본문 노드. 블록(`{...}`) 또는 expression-bodied(`=> expr`)의 arrow_expression_clause."""
    body = field(node, "body")
    if body is not None:
        return body
    # 일부 형태는 body 필드 없이 직접 block/arrow를 자식으로 가질 수 있음
    for child in node.children:
        if child.type in {"block", "arrow_expression_clause"}:
            return child
    return None


# ---------------------------------------------------------------------------
# 추출기들
# ---------------------------------------------------------------------------
def extract_parameters(source: bytes, method_node: Node) -> list[Finding]:
    results: list[Finding] = []
    params = field(method_node, "parameters")
    if params is None:
        for child in method_node.children:
            if child.type == "parameter_list":
                params = child
                break
    if params is None:
        return results
    for child in params.children:
        if child.type == "parameter":
            line, col = line_col(child)
            results.append(Finding("parameter", one_line(node_text(source, child)), line, col))
    return results


def _logical_operator(node: Node) -> Optional[str]:
    """binary_expression 의 직속 연산자 토큰이 && / || 면 반환."""
    for child in node.children:
        if not child.is_named and child.type in {"&&", "||"}:
            return child.type
    return None


def _switch_section_label(source: bytes, section: Node) -> str:
    """switch_section 의 case 라벨/패턴 텍스트. default 면 'default'."""
    patterns: list[str] = []
    has_default = False
    for child in section.children:
        if child.type == "default_switch_label" or (not child.is_named and child.type == "default"):
            has_default = True
        elif child.type.endswith("_pattern") or child.type == "when_clause":
            patterns.append(one_line(node_text(source, child)))
    if patterns:
        return "case " + " | ".join(patterns)
    return "default" if has_default else "case"


def extract_branches(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        if node.type == "if_statement":
            cond = field(node, "condition")
            target = cond if cond is not None else node
            line, col = line_col(target)
            results.append(Finding("if", one_line(node_text(source, target)), line, col))
        elif node.type == "switch_statement":
            value = field(node, "value")
            target = value if value is not None else node
            line, col = line_col(node)
            results.append(Finding("switch", one_line(node_text(source, target)), line, col))
        elif node.type == "switch_section":
            line, col = line_col(node)
            results.append(Finding("switch_case", _switch_section_label(source, node), line, col))
        elif node.type == "switch_expression":
            line, col = line_col(node)
            results.append(Finding("switch_expression", one_line(node_text(source, node)), line, col))
        elif node.type == "switch_expression_arm":
            line, col = line_col(node)
            results.append(Finding("switch_arm", one_line(node_text(source, node)), line, col))
        elif node.type == "conditional_expression":
            line, col = line_col(node)
            results.append(Finding("ternary", one_line(node_text(source, node)), line, col))
        elif node.type == "binary_expression":
            op = _logical_operator(node)
            if op:
                kind = "logical_and" if op == "&&" else "logical_or"
                line, col = line_col(node)
                results.append(Finding(kind, one_line(node_text(source, node)), line, col))
    return results


def extract_returns(source: bytes, method_node: Node, body: Node) -> list[Finding]:
    results: list[Finding] = []
    # expression-bodied (`=> expr`) 는 return 문이 없으므로 암묵 return으로 합성
    if body.type == "arrow_expression_clause":
        for child in body.children:
            if child.is_named:
                line, col = line_col(child)
                results.append(
                    Finding("return (expression-bodied)", one_line(node_text(source, child)), line, col)
                )
                break
        return results
    for node in walk_body(body):
        if node.type == "return_statement":
            line, col = line_col(node)
            results.append(Finding("return", one_line(node_text(source, node)), line, col))
        elif node.type == "yield_statement":
            line, col = line_col(node)
            results.append(Finding("yield", one_line(node_text(source, node)), line, col))
    return results


def extract_assignments(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        if node.type == "assignment_expression":
            line, col = line_col(node)
            results.append(Finding("assignment", one_line(node_text(source, node)), line, col))
        elif node.type == "variable_declaration":
            line, col = line_col(node)
            results.append(Finding("variable_declaration", one_line(node_text(source, node)), line, col))
    return results


CONVERSION_NODES = {
    "cast_expression",
    "as_expression",
    "object_creation_expression",
    "implicit_object_creation_expression",
}

CONVERSION_CALL_MARKERS = (
    "Convert.To",
    "int.Parse", "Int32.Parse", "long.Parse", "Int64.Parse",
    "decimal.Parse", "Decimal.Parse",
    "double.Parse", "Double.Parse", "float.Parse", "Single.Parse",
    "bool.Parse", "Boolean.Parse",
    "DateTime.Parse", "DateTime.ParseExact", "Guid.Parse",
    ".Parse(", ".TryParse(",
    ".ToString(", ".ToDecimal(", ".ToDouble(", ".ToInt32(", ".ToInt64(",
    ".ToInt16(", ".ToSingle(", ".ToBoolean(",
)


def extract_conversions(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        text = one_line(node_text(source, node))
        if node.type in CONVERSION_NODES:
            line, col = line_col(node)
            results.append(Finding(node.type, text, line, col))
        elif node.type == "invocation_expression" and any(m in text for m in CONVERSION_CALL_MARKERS):
            line, col = line_col(node)
            results.append(Finding("conversion_call", text, line, col))
    return results


INVOCATION_NODES = {
    "invocation_expression",
    "object_creation_expression",
    "implicit_object_creation_expression",
    "await_expression",
}


def extract_invocations(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        if node.type in INVOCATION_NODES:
            line, col = line_col(node)
            results.append(Finding(node.type, one_line(node_text(source, node)), line, col))
    return results


# DB write / 외부 호출 / COM / 로그 / 저장 / 컬렉션 변경 마커.
# EasiSlides 도메인(송출 좌표·COM Interop·SQLite/MariaDB)을 추가로 반영.
SIDE_EFFECT_MARKERS = (
    ".Add(", ".AddRange(", ".Remove(", ".RemoveAt(", ".RemoveAll(", ".Clear(",
    ".Insert(", ".Update(", ".Save", ".SaveChanges", ".SaveChangesAsync",
    ".Execute", ".ExecuteAsync", ".ExecuteNonQuery", ".ExecuteReader", ".ExecuteScalar",
    ".Commit", ".Rollback", ".Publish", ".Send", ".SendAsync",
    ".Log", ".LogInformation", ".LogWarning", ".LogError", ".LogDebug",
    ".Enqueue", ".Dequeue", ".Push", ".Pop",
    ".Invalidate(", ".Refresh(", ".Show(", ".Close(", ".Dispose(",
    "Marshal.ReleaseComObject", ".ReleaseComObject",
    "_repository.", "Repository.", "_repo.", "_db.", "_context.",
    "DbContext", "SQLiteController", "MySqlCommand", "SQLiteCommand",
    "File.Write", "File.Delete", "File.Move", "File.Copy", "File.Create",
    "Registry.", "RegUtil.",
)

# EasiSlides 송출 좌표/상태 변수 변경 (CLAUDE.md 민감 경로)
OUTPUT_STATE_MARKERS = (
    "LS_Width", "LS_Height", "Buffer_LS_Width", "Buffer_LS_Height", "selectScreen",
)


def extract_mutations(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        if node.type == "assignment_expression":
            text = one_line(node_text(source, node))
            kind = "assignment"
            if any(m in text for m in OUTPUT_STATE_MARKERS):
                kind = "OUTPUT_STATE_MUTATION"  # 송출 좌표/화면선택 변경 — 회귀 가드 필수
            line, col = line_col(node)
            results.append(Finding(kind, text, line, col))
        elif node.type in {"postfix_unary_expression", "prefix_unary_expression"}:
            text = one_line(node_text(source, node))
            if text.endswith("++") or text.endswith("--") or text.startswith("++") or text.startswith("--"):
                line, col = line_col(node)
                results.append(Finding("inc_dec", text, line, col))
        elif node.type in {"invocation_expression", "await_expression"}:
            text = one_line(node_text(source, node))
            if any(m in text for m in SIDE_EFFECT_MARKERS):
                line, col = line_col(node)
                results.append(Finding("side_effect_call", text, line, col))
    return results


def extract_try_catch(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        if node.type in {"try_statement", "catch_clause", "finally_clause", "throw_statement", "throw_expression"}:
            line, col = line_col(node)
            results.append(Finding(node.type, one_line(node_text(source, node)), line, col))
    return results


def extract_loops(source: bytes, body: Node) -> list[Finding]:
    results: list[Finding] = []
    for node in walk_body(body):
        if node.type in {"for_statement", "foreach_statement", "while_statement", "do_statement"}:
            line, col = line_col(node)
            results.append(Finding(node.type, one_line(node_text(source, node)), line, col))
    return results


# ---------------------------------------------------------------------------
# 렌더링
# ---------------------------------------------------------------------------
def render_table(findings: list[Finding], title: str) -> str:
    lines = [f"## {title}", ""]
    if not findings:
        lines.append("- none")
        lines.append("")
        return "\n".join(lines)
    lines.append("| ID | Kind | Line | Expression |")
    lines.append("|---:|---|---:|---|")
    for idx, item in enumerate(findings, start=1):
        expr = item.text.replace("|", "\\|").replace("`", "'")
        lines.append(f"| {idx} | `{item.kind}` | {item.line} | `{expr}` |")
    lines.append("")
    return "\n".join(lines)


def render_markdown(source_path: Path, method_spec: str, method_node: Node, body: Node, sections: list[tuple[list[Finding], str]]) -> str:
    start_line = method_node.start_point[0] + 1
    end_line = method_node.end_point[0] + 1
    body_kind = "expression-bodied (`=> expr`)" if body.type == "arrow_expression_clause" else "block (`{ ... }`)"

    lines = [
        "# Function AST Summary",
        "",
        "> 이 문서는 tree-sitter-c-sharp 로 C# 대상 메서드 내부 구조를 **기계 추출**한 결과다.",
        "> Roslyn semantic model이 아니므로 타입 추론/symbol resolution/DI·overload 해석은 하지 않는다.",
        "",
        "## File",
        "",
        f"`{source_path.as_posix()}`",
        "",
        "## Method",
        "",
        f"`{method_spec}`",
        "",
        "## Location",
        "",
        f"- Start line: `{start_line}`",
        f"- End line: `{end_line}`",
        f"- Body: {body_kind}",
        "",
        "## Review Rule",
        "",
        "- 이 요약에 나온 branch, return, assignment, conversion, mutation, fallback path를 누락한 채 구현하면 안 된다.",
        "- 이 요약에 없는 branch를 임의로 만들어 추론하면 안 된다.",
        "- `OUTPUT_STATE_MUTATION`(LS_*/selectScreen) 이 있으면 `selectScreen==null`/`LS_Width=0` 회귀 가드를 반드시 검증한다.",
        "- 타입 추론·DI concrete class·extension method·overload·LINQ/async 의미는 Tree-sitter만으로 확정하지 말고 '불확실'로 표시해 CodeGraph/Roslyn 2차 분석으로 넘긴다.",
        "- 도메인 의미 판단은 다음 단계 `function-logic-map.md`에서 수행한다.",
        "",
    ]
    for findings, title in sections:
        lines.append(render_table(findings, title))
    lines.extend(
        [
            "## Next Required Artifacts",
            "",
            "1. `function-logic-map.md` — 위 항목별 도메인 의미 / early return 영향 / invariant / 수정 가능·금지 영역",
            "2. `branch-test-map.md` — branch별 기존 테스트 매핑 + 추가 실패 테스트",
            "",
            "## Implementation Gate",
            "",
            "이 문서만으로 production code를 수정하면 안 된다.",
            "`function-logic-map.md` 와 `branch-test-map.md` 작성 후에만 구현을 시작한다.",
            "(CLAUDE.md `## 함수 내부 로직 분석 레이어` 7절 구현 금지 조건)",
            "",
        ]
    )
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# main
# ---------------------------------------------------------------------------
def main() -> int:
    cli = argparse.ArgumentParser(description="Extract C# method-level AST summary using tree-sitter-c-sharp.")
    cli.add_argument("--file", required=True, help="C# 소스 파일 경로")
    cli.add_argument("--method", required=True, help="대상 메서드명 (예: FormatText 또는 gfDisplay.FormatText)")
    cli.add_argument("--out", required=True, help="출력 markdown 경로")
    args = cli.parse_args()

    source_path = Path(args.file)
    out_path = Path(args.out)

    if not source_path.exists():
        print(f"ERROR: 소스 파일 없음: {source_path}", file=sys.stderr)
        return 2

    source = source_path.read_bytes()
    parser = make_csharp_parser()
    root = parser.parse(source).root_node

    method_node = find_method(source, root, args.method)
    if method_node is None:
        print(f"ERROR: 메서드를 찾지 못했거나 모호함: {args.method} in {source_path}", file=sys.stderr)
        return 3

    body = method_body(method_node)
    if body is None:
        print(f"ERROR: 메서드 본문(body)을 찾지 못함: {args.method} (abstract/partial 선언일 수 있음)", file=sys.stderr)
        return 4

    sections = [
        (extract_parameters(source, method_node), "Parameters"),
        (extract_branches(source, body), "Branch Conditions"),
        (extract_returns(source, method_node, body), "Return Statements"),
        (extract_assignments(source, body), "Assignments / Variable Declarations"),
        (extract_conversions(source, body), "Type Conversions / Object Creation"),
        (extract_invocations(source, body), "Invocations / Await / Object Creation"),
        (extract_mutations(source, body), "Mutations / Possible Side Effects"),
        (extract_try_catch(source, body), "Try / Catch / Finally / Throw"),
        (extract_loops(source, body), "Loops"),
    ]

    markdown = render_markdown(source_path, args.method, method_node, body, sections)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(markdown, encoding="utf-8")
    print(f"Wrote: {out_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
