# 함수 AST 요약

> 이 문서는 `tree-sitter-c-sharp` 기반 C# 메서드 구조 추출 결과를 사람이 검토할 수 있도록 정리한 것이다.  
> tree-sitter 결과는 구문 구조 근거이며, overload/DI/의미 해석은 CodeGraph와 테스트 결과로 보완한다.

## 파일

`Easislides.Wpf/Shell/MainViewModel.cs`

## 메서드

`MainViewModel.IsPerItemFormattable`

## 역할

선택 항목이 개별 서식 명령을 받을 수 있는지 판단한다.

## 매개변수

| 이름 | 형식 | 의미 |
| --- | --- | --- |
| `item` | `LiveQueueItem` | 검사 대상 예배 순서 항목 |

## 수정 후 반환식

```csharp
(item.Kind is LiveItemKinds.Song or LiveItemKinds.Bible or LiveItemKinds.Notice)
    && !string.IsNullOrWhiteSpace(item.Lyrics)
```

## 주요 조건

| 조건 | 의미 |
| --- | --- |
| `item.Kind is LiveItemKinds.Song` | 곡 항목은 본문이 있으면 개별 서식 가능 |
| `item.Kind is LiveItemKinds.Bible` | 성경 항목은 본문이 있으면 개별 서식 가능 |
| `item.Kind is LiveItemKinds.Notice` | 텍스트 파일/공지 항목은 본문이 있으면 개별 서식 가능 |
| `!string.IsNullOrWhiteSpace(item.Lyrics)` | 본문 없는 항목은 개별 서식 불가 |

## 상태 변경

- 없음. 순수 판정 함수다.

## 검토 메모

- 이번 production 수정 대상이다.
- `Notice`를 추가하되, `Lyrics` 조건을 유지해 빈 placeholder 항목은 계속 막는다.
- PowerPoint/미디어 같은 비텍스트 항목은 포함하지 않았다.
