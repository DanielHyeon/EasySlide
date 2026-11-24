## Phase 0. 기준과 영향 확인

- [x] 0.1 a035 반영 후 `dotnet test Axicultus.Tests` 기준 결과 기록
- [x] 0.2 `TextFor`/`Follow`/`MoveToSlide`/상자 목록/복구 저장 경로와 `ReadLegacyDeckAsync` 확인
- DoD: 기준 수치와 영향 목록이 "검증 결과"에 있다.

## Phase 1. 실패하는 테스트

- [x] 1.1 코드로 만든 pptx: 제목+본문 상자, 슬라이드마다 이름이 다른 TextBox, 그룹 안 상자, 표, 숨긴 상자, 슬라이드 번호·바닥글 자리표시자, 아래 가장자리 절 버튼, 레이아웃에서 위치를 상속한 자리표시자, 배경 채우기, 전체 크기 배경 그림
- [x] 1.2 전체 글자 상자의 내용과 순서, 제외 규칙
- [x] 1.3 이름이 바뀌는 상자를 고른 채 다음 슬라이드로 가도 자막이 비지 않음(전체 글자로 대체)
- [x] 1.4 배경 그림·배경 채우기만 있는 슬라이드는 `HasPicture == false`
- [x] 1.5 `CaptionStyle.Display` 저장·복원, 없는 값/모르는 값은 `auto`
- [x] 1.6 자동 표시의 슬라이드별 선택(악보 그림 → 악보, 텍스트 → 자막)을 검증할 수 있는 수준의 단위(분리한 판정 함수 등)
- [x] 1.7 `.ppt` 경로의 전체 글자 상자(공통 도우미 단위 테스트)

## Phase 2. 최소 구현

- [x] 2.1 전체 글자 상자(pptx, .ppt 공통 도우미)
- [x] 2.2 `TextFor` 대체 규칙
- [x] 2.3 `HasPicture` 판정 변경, OCR 대상 축소
- [x] 2.4 기본 상자 = 전체 글자
- [x] 2.5 `Display` 저장과 "자동" 라디오, 슬라이드별 자동 선택
- Constraints: 식별자·주석 영어. 콘솔 배치는 라디오 하나 추가 외 변경 금지.

## Phase 3. 회귀와 빌드

- [x] 3.1 Release 빌드 오류 0, 전체 테스트 통과
- [ ] 3.2 하네스(격리 사본): 실제 텍스트 PPT 하나와 배경 위 텍스트 PPT 하나를 따라가며 슬라이드별 자막 기록, 악보 PPT에서 자동 모드가 악보를 고르는지, 실제 데이터 해시 unchanged

## Phase 4. 리뷰와 QA 증거

- [x] 4.1 적대적 리뷰 지적 사항 처리 — 리뷰 1~3차(별도 에이전트), 구현 4~5차. 3차에서 큰 지적 없음, 남은 작은 지적은 6차에서 수정
- [x] 4.2 `/code-review` 지적 사항 처리 — 7건(중요 1건은 design 5 (나)로 명세 보완, a036 이전부터 있던 결함 3건 포함) 모두 6차에서 수정
- [x] 4.3 총괄 완료 검증(2026-11-24) — 전체 테스트 3446 통과 / 21 건너뜀 / 0 실패, `openspec validate --strict` 통과, 세션·덱 변경 읽기 검토. 하네스(3.2)는 화면이 1개라 미실행: a035와 같이 사용자 결정에 따라 화면이 2개일 때 `ppttext` 단계로 따로 실행

## 검증 결과

(구현 에이전트와 총괄이 채운다)

### 구현 에이전트 1차 (2026-11-24)

**Phase 0 기준**
- 변경 전(HEAD 1450e5b) `dotnet test Axicultus.Tests`: 통과 3418, 건너뜀 21, 실패 0.
- 영향 확인(저장소 검색): `CaptionSession.TextFor`는 `MoveToSlide`(따라가기 자막), 콘솔 `RefreshCue`, 하네스가 부른다. `Follow`는 `BoxKey`를 정하고 `MoveToSlide`를 부른다. 상자 목록은 콘솔 `SlideSelected`/`RefreshBoxes`/`ShowDeck`가 채우고 고른 키가 없으면 첫 상자를 고른다. 새 PPT의 기본 상자는 `PrepareDeckAsync`가 직전 PPT의 `BoxKey`를 넘기던 것. 복구 저장(`CaptionRecovery`)은 고른 슬라이드와 상자 키(`SelectedKey`)를 저장하고 `Recover`가 `ShowDeck(saved.Box)`로 되살린다(바꾸지 않음). `.ppt`는 자식 프로세스의 `OfficePptSession.ReadCaptionDeckAsync`가 상자를 만들고 부모의 `PowerPointWorker.ReadLegacyDeckAsync`가 받는다. `HasPicture`를 쓰는 곳은 `RecognizeDeckAsync`(OCR 대상) 하나와 테스트.

**Phase 1** — 테스트를 먼저 썼고, 구현 전에는 `CaptionDeck.AllTextKey`/`WithAllText`, `CaptionStyle.Display`, `CaptionSession.Display`, `CaptionConsole.AutoShow`가 없어 컴파일 실패(CS0117/CS1061)로 실패함을 확인했다.
- 새 파일 `Axicultus.Tests/Shell/CaptionAllTextTests.cs`(레이아웃·마스터가 있는 pptx를 코드로 만든다)
  - `EverySlideStartsWithItsWholeTextReadTopToBottomAndLeftToRight`: 8장. 1장 본문(레이아웃 위치)·제목(레이아웃에 위치가 없어 마스터 위치)이 쌓임 순서와 반대로 제목→본문, 슬라이드 번호·날짜·바닥글 제외, 어디에도 위치가 없는 자리표시자는 끝. 2장 이름이 다른 TextBox 둘이 같은 띠에서 왼→오른. 3장 그룹 변환(자식 단위 → 절대 좌표)으로 그룹 밖 상자 사이에 끼는 순서, 숨긴 그룹 제외. 4장 표(칸은 공백, 행은 줄, 빈 칸 생략, 칸 안 문단은 공백), 숨긴 상자 제외. 5장 아래 가장자리 작은 절 버튼 제외, 위쪽 88% 이상이어도 높이 8% 초과 상자와 위쪽 86% 본문 줄은 남김, 슬라이드 밖(왼쪽·아래) 제외, 상자 안 빈 줄 여러 개 → 하나. 6장 배경 그림 채우기 위 글, 7장 전체 크기 배경 그림 위 글, 8장 악보 그림만(전체 글자 ""). 모든 슬라이드 첫 상자가 `slide:all`/"슬라이드 전체 글자"이고 기존 상자 키는 그대로.
  - `OnlyASlideWithAContentPictureIsAPictureSlide`: 위 8장 중 악보 그림 슬라이드만 `HasPicture == true`, 모든 슬라이드에서 `HasPicture == (Picture != null)`.
  - `ABoxMissingOnTheNextSlideGivesThatSlidesWholeTextInsteadOfNothing`: 2장의 "TextBox 3"을 고르고 3장으로 가면 3장 전체 글자. 수동 문구가 먼저.
  - `TheOldDecksWholeTextJoinsItsBoxesInTheGivenOrder`: `.ppt` 공통 도우미 `CaptionDeck.WithAllText`(주어진 순서, 빈 줄 하나로, 빈 상자 생략, `HasPicture` null 유지, 상자 없는 슬라이드도 빈 전체 글자 상자).
  - `TheDisplayChoiceIsKeptAndAnOldOrUnknownOneReadsAsAuto`: a036 이전 `caption-style.json`(Display 없음) → auto이고 다른 값 유지, 저장·재열기, 모르는 값 → auto.
- `CaptionConsoleTests`: `ANewlyReadDeckStartsOnTheSlidesWholeText`(직전 PPT에서 고른 상자와 같은 키가 새 PPT에 있어도 기본은 전체 글자), `AutoSendsTheScoreOnAScoreSlideAndTheTextOnATextSlide`(자동이 기본, 1장 텍스트 → 전체 글자 자막, 2장 악보 그림 → 악보 줄(`ScoreSource == "picture"`, 글자 없음), 다시 1장 → 자막, 라디오 선택이 `Display`로 저장).
- `PowerPointWorkerTests.AnOldDeckReadByTheChildStartsEachSlideWithItsWholeText`: 대역 자식 프로세스가 돌려준 `.ppt` 상자에 `ReadLegacyDeckAsync`가 전체 글자 상자를 맨 앞에 붙인다.
- 의도한 동작 변경으로 고친 기존 테스트: `CaptionScorePictureTests`의 `HasPicture` 단언 2곳(모든 그림 → `Picture != null`과 같음, EMF 그림 → false), `CaptionIntegrationTests.DeckUsesPresentationOrderAndKeepsParagraphsAndLineBreaks`(상자가 하나 → 전체 글자 + 상자).

**Phase 2 구현**
- `CaptionDeck`: `AllTextKey = "slide:all"`. `Read`가 슬라이드마다 전체 글자 상자를 맨 앞에 넣는다(`p:sp` 글 + `p:graphicFrame`의 표, `mc:Fallback` 안 중복 개체 제외). a035의 그룹 좌표 계산을 `Place`로 꺼내 내용 그림 판정과 함께 쓴다(동작 같음, a035 테스트 통과). 자리표시자 위치 상속은 레이아웃 → 마스터 순서로 같은 idx, 없으면 같은 type(레이아웃·마스터 XML은 PPT마다 한 번만 읽음). 공통 도우미 `AllText(texts)`(빈 줄 하나로, 앞뒤 공백 정리)와 `.ppt`용 `WithAllText(slide)`. `HasPicture = Picture != null`.
- `PowerPointWorker.ReadLegacyDeckAsync`: 받은 슬라이드마다 `WithAllText`.
- `CaptionSession.TextFor`: 수동 문구 → 그 키 상자 → 그 슬라이드의 전체 글자. `CaptionStyle.Display`(기본 `auto`, `ParseDisplay`가 모르는 값·null을 auto로) 저장, `CaptionSession.Display` 속성.
- `CaptionConsole`: 라디오 줄 맨 앞에 "자동"(`AutoShow`, 기본 선택), 라디오 `Tag`로 `Display` 저장·복원(다른 배치 변경 없음). 자동일 때 현재 슬라이드에 내용 그림이 있고 준비 결과 그 그림에서 오선을 찾았으면 악보만, 아니면 자막(그림 준비 중에는 악보 경로가 기다리며 글자를 내보낸다). 새로 읽은 PPT의 기본 상자 = 전체 글자. `OfferRecognizedLyrics` 조건은 그대로.

**Phase 3.1**
- 최종 `dotnet test Axicultus.Tests`: 통과 3426, 건너뜀 21, 실패 0(기준 3418 + 새 테스트 8).
- `dotnet build Axicultus\Axicultus.csproj -c Release --no-incremental`: 오류 0. 바꾼 파일에서 나온 경고는 이전부터 있던 `CaptionSession.cs` EasiDS001 하나(줄 번호만 이동). 하네스 프로젝트 빌드 오류 0.

**Phase 3.2 하네스** — 하네스: 화면 1개라 미실행. `Run-Harness.ps1 -Phases ppttext` → `GUARD: only 1 screen(s) present; not launching`(exit 10). 새 단계 `ppttext`(`tools/AxicultusHarness/Program.cs`, README 표에 추가)를 만들어 빌드만 확인했다: 예배 순서의 PPT와 환경 변수 `AXI_A036_DECKS`(세미콜론 구분)의 PPT를 격리 작업 폴더 `a036`에 복사해 읽고, 자동 표시로 슬라이드마다 보낸 것(자막 글/악보 줄과 출처)을 기록하고, 1장의 이름 상자를 따라갈 때 그 상자가 없는 슬라이드의 빈 자막 수를 센다. 화면 2개 PC에서 텍스트 PPT, 배경 위 텍스트 PPT, 악보 PPT를 `AXI_A036_DECKS`로 주고 돌리면 3.2를 확인할 수 있다.

**실제 자료 조사(읽기 전용)** — `D:\예배자료`의 pptx 1,248개를 scratchpad 사본(`a036-round1\decks`)으로 복사해 앱 없이 `CaptionDeck.Read`와 같은 규칙으로 셌다(조사 프로그램 `a036-round1\survey*`, 결과 `survey-text.txt`, `survey-link.txt`). 읽기 실패 3개는 모두 Office 잠금 파일(`~$*.pptx`).
- 1,245개 PPT, 10,347장. 전체 글자가 있는 슬라이드 877장(133개 PPT), 비어 있는 슬라이드 9,470장(대부분 찬송가 악보).
- 남는 글이 2개 이상인 418장 중 249장에서 읽는 순서가 쌓임 순서와 다르다. 표본 25건을 봤고 모두 제목/머리말(『말씀 제목』, 『교회 표어』, "장로 임직자", "설교")이 본문 앞으로 온 경우다.
- 아래 가장자리 규칙으로 뺀 개체: 슬라이드 7,781장, 개체 27,558개. 그중 27,298개는 슬라이드 링크(절 버튼)다. 링크 없는 260개 중 242개는 찬송가 폴더(절 이름, "2/8" 같은 쪽 번호), **18개는 찬송가 밖의 실제 본문 줄**(재정보고 "잔 액 … 원" 14장, 교회 비전 "교회 공동체의 선교적 사명 강화" 2장, 재정집행계획 "※ 지출 소요 외 …" 1장, 그 밖에 바닥글 성격의 "산소망 교회, 2025년 12월" 1장). 텍스트 슬라이드 877장 중 약 18장에서 마지막 줄이 빠진다.
- 슬라이드 밖·숨김으로 뺀 개체 0, 번호·날짜·바닥글 자리표시자 88장, 표 있는 슬라이드 59장, 레이아웃·마스터에서 위치를 받은 자리표시자 2장, 위치를 모르는 자리표시자 0, `mc:Fallback` 중복 0.
- 1장의 첫 상자(이전 기본 상자)가 없는 뒤 슬라이드: 1,664장(305개 PPT). 이전에는 빈 자막, 이제 그 슬라이드의 전체 글자.
- 그림 슬라이드 판정: 이전(그림 요소가 하나라도 있음) 9,897장 → 이제(내용 그림) 9,213장. 빠진 684장 중 `p:pic`이 있는 것 499장, 그중 **전체 글자도 빈 슬라이드 151장(29개 PPT, 예: "대단한 믿음 없어도 -마커스워십", "나 주님의 기쁨되기 원하네", "세상 흔들리고 (고형원) 4_3")** — 악보 그림이 여러 조각이거나 덮여 있어 a035가 렌더 경로로 보낸 악보 슬라이드다. 이전에는 렌더 그림으로 OCR해 가사를 자막으로 썼지만 이제 OCR 대상이 아니어서 자막이 빈다(아래 "열린 문제" 1).
- 자동 표시에서 악보로 가는 슬라이드(내용 그림에서 오선 찾음): 내용 그림 9,213장 중 9,104장. 나머지 109장(오선이 없는 그림: 가사 그림, 사진)과 내용 그림이 없는 슬라이드는 자막. 배경 그림·배경 채우기 위의 텍스트 슬라이드는 내용 그림이 없으므로 자막이다.

**열린 문제(총괄 판단 필요, 설계와 다르게 구현하지 않음)**
1. 결정 5(OCR은 내용 그림 슬라이드만)는 실제 자료에서 151장(29개 PPT)의 악보 슬라이드 자막을 비운다. 제안: OCR 대상 = 내용 그림이 있는 슬라이드 + (전체 글자가 비어 있고 배경이 아닌 `p:pic`이 있는 슬라이드). 배경 위 텍스트 슬라이드는 전체 글자가 있으므로 여전히 OCR하지 않는다.
2. 아래 가장자리 규칙은 찬송가 밖 18장의 본문 마지막 줄(주로 재정보고 잔액)을 뺀다. 제안: 규칙을 "슬라이드 링크(`hlinkClick`, 자기 또는 그룹)가 있는 개체"로 좁히면 찬송가 절 버튼 27,298개는 그대로 빠지고 본문 18줄은 남는다. 다만 찬송가 폴더의 링크 없는 242개(쪽 번호 "2/8" 등)가 전체 글자에 들어가 `OfferRecognizedLyrics`의 "모든 슬라이드가 같은 글" 조건이 깨질 수 있으므로, 링크 조건과 지금 규칙을 함께(둘 중 하나면 제외하되 링크 없는 것은 숫자·"n/m"·절 표기만) 쓰는 안도 있다.

### 구현 에이전트 2차 — 총괄 결정(결정 2, 5 개정) 반영 (2026-11-24)

**바꾼 것**
- 결정 2(아래 가장자리): 위쪽 88% 이상·높이 8% 이하 개체는 슬라이드 링크(개체 자신·글 안·속한 그룹, 같은 그룹 안의 링크 포함 — a035의 버튼 판정과 같은 범위, `hlinkClick`의 `action`이 `ppaction://hlinksldjump` 또는 `ppaction://hlinkshowjump`)이거나 글이 6자 이하일 때만 뺀다(`CaptionDeck.SlideLink`).
- 결정 5(그림 슬라이드): `HasPicture = Picture != null || (전체 글자가 비어 있음 && 배경이 아닌 p:pic이 있음)`. "배경이 아님" = 숨기지 않았고(숨긴 그룹 포함) 그룹 변환 후 면적이 슬라이드의 80% 미만(a035 배경 기준). 위치가 없는 그림은 배경이 아닌 것으로 본다(`CaptionDeck.ShownPicture`). `.ppt`는 그대로 null. 자동 표시는 결정 7 그대로(렌더 경로 악보 슬라이드 → 자막).

**새·바뀐 테스트**
- `CaptionAllTextTests.AScoreCutInPiecesWithoutTextIsAPictureSlideAndASmallPictureBesideTextIsNot`: 두 조각 악보만 있는 슬라이드 → `HasPicture` true(`Picture` null), 글 옆 작은 그림 → false, 배경 그림 채우기만 → false.
- `CaptionAllTextTests.AtTheSlidesFootOnlyASlideLinkOrAShortMarkIsLeftOut`: 아래 가장자리의 링크 없는 "잔 액 1,234,000 원"은 남고, 슬라이드 이동 링크가 있는 긴 절 버튼과 링크 없는 "2/8"은 빠진다.
- `OnlyASlideWithAContentPictureIsAPictureSlide` → `ABackgroundUnderTextIsNoPictureSlide`로 이름을 바꾸고 `HasPicture == (Picture != null)` 단언을 뺐다.
- `CaptionScorePictureTests`의 `HasPicture` 단언 2곳을 원래대로(모든 슬라이드 true, EMF 악보 true) 되돌렸다. 글 상자가 없으므로 두 번째 조건으로 참이다.

**결과**
- `dotnet test Axicultus.Tests`: 통과 3428, 건너뜀 21, 실패 0(1차 3426 + 새 테스트 2).
- Release 빌드(`--no-incremental`) 오류 0, 바꾼 파일의 새 경고 없음. 하네스 빌드 오류 0. 하네스는 여전히 화면 1개라 미실행.

**실제 자료 재조사(읽기 전용 사본, `a036-round1\survey2`, 결과 `survey-round2.txt`)** — 1차 규칙은 조사 프로그램 안에서 다시 만들어 2차(`CaptionDeck.Read`)와 같은 XML 읽기로 비교했다.
- 1차의 실제 본문 마지막 줄 18개: 18개 모두 남는다(재정 잔액 14, 비전 2, "※ 지출 소요 외 …" 1, "산소망 교회, 2025년 12월" 1).
- 1차의 찬송가 폴더 링크 없는 표시 242개: 218개는 여전히 빠지고 24개가 남는다. 24개는 모두 "삶의 예배 PPT" 한 PPT(8장 × 3개)의 링크 없는 절 버튼("1.보여지는 예배 아닌 마음으로" 등, 7자 이상)으로, 이 PPT의 가사 글 뒤에 매 장 붙는다.
- 1차에서 "링크"로 센 27,298개 중 27,186개가 슬라이드 이동 링크. 링크가 `ppaction://noaction`뿐인 6자 초과 절 버튼 18개(본 표본 15개: "어버이은혜(1,2)" 6장, "평안을 너희게 주노라" 두 파일 9장 — 현재 절 버튼)가 새로 전체 글자에 들어간다.
- 렌더 경로 악보 슬라이드 151장: 121장이 이제 `HasPicture`(OCR 유지). 남은 30장(7개 PPT, 예: "시편 139편" 두 파일 16장, "예수 열방의 소망 ppt" 8장, 중고등부 온라인 예배 4장)은 그림이 슬라이드의 80% 이상(86%~133%)이라 배경 기준에 걸려 여전히 OCR 대상이 아니고 자막이 빈다.
- 글이 있는 슬라이드 중 `HasPicture`가 1차와 달라진 것: 0장. 그림 슬라이드 수: 1차 9,213장 → 2차 9,334장.
- 전체 글자가 달라진 슬라이드 44장(빈 글 → 글 12장: 위 noaction 절 버튼, 글 → 빈 글 0장).
- `OfferRecognizedLyrics` 조건(모든 슬라이드 전체 글자가 같음/비어 있음)에 걸리는 PPT: 1차 1,146개, 2차 1,146개, 바뀐 PPT 0개.

**열린 문제(2차, 설계대로 구현함)**
1. 결정 2의 "다른 슬라이드로 가는 링크"는 현재 절 버튼(`ppaction://noaction`) 18개를 놓친다. 제안: 아래 가장자리에서는 `hlinkClick`이 있으면 동작과 관계없이 링크로 본다(1차 조사에서 아래 가장자리의 `hlinkClick` 27,298개는 모두 절 버튼이었고, 살려야 할 본문 18줄에는 `hlinkClick`이 없다).
2. "삶의 예배 PPT"의 링크 없는 긴 절 버튼 24개는 6자 규칙으로 막히지 않는다(가사 자막 뒤에 붙음). 한 PPT뿐이라 허용 한계로 기록하거나, 규칙에 "같은 PPT의 모든 슬라이드에 같은 글로 반복되는 아래 가장자리 개체"를 더하는 안이 있다.
3. 글이 없는 슬라이드의 그림이 슬라이드 크기(80% 이상)일 때도 30장(7개 PPT)의 악보 자막이 빈다. 제안: 전체 글자가 비어 있으면 숨기지 않은 `p:pic`이 하나라도 있을 때 그림 슬라이드로 본다(글 없는 배경 사진 슬라이드는 OCR해도 읽을 글이 없어 자막은 그대로 빈다).

### 구현 에이전트 3차 — 총괄 결정(결정 2, 5 재개정) 반영, 최종 상태 (2026-11-24)

1차·2차 절은 진행 기록이다. (이 절의 "최종 동작"은 4차에서 갱신됐다. 현재 동작은 4차 절을 본다.)

**최종 동작**
- 전체 글자 상자(`CaptionDeck.AllTextKey = "slide:all"`, "슬라이드 전체 글자")가 모든 슬라이드의 첫 상자다. pptx(`CaptionDeck.Read` → `AllText`): 보이는 `p:sp` 글과 표(칸은 공백, 행은 줄). 빼는 것: 숨긴 개체(숨긴 그룹 포함), `sldNum`/`dt`/`ftr` 자리표시자, 슬라이드 밖 개체, `mc:Fallback` 안의 중복, 아래 가장자리 개체(위쪽 ≥ 88%, 높이 ≤ 8%) 중 **링크가 있거나(`CaptionDeck.HasLink`: `hlinkClick`이면 동작 무관 — `ppaction://noaction` 포함 — 개체·글 안·감싼 그룹·같은 부모 그룹 어디든) 글이 6자 이하인 것**. 순서는 5% 띠 → 왼쪽. 자리표시자 위치는 레이아웃 → 마스터(같은 idx, 없으면 같은 type), 없으면 끝. `.ppt`는 `WithAllText`로 주어진 순서. 빈 줄은 하나로.
- `HasPicture` = `Picture != null` **또는 (전체 글자가 비어 있고 숨기지 않은 `p:pic`이 하나라도 있음 — 크기 무관)**(`CaptionDeck.ShownPicture`). 배경 그림 채우기만 있거나 글 아래 배경 그림이 있는 슬라이드는 false. `.ppt`는 null. OCR(`RecognizeDeckAsync`)은 `HasPicture != false`인 슬라이드만.
- `TextFor`: 수동 문구 → 그 키 상자 → 그 슬라이드의 전체 글자. 새로 읽은 PPT의 기본 상자 = 전체 글자. `OfferRecognizedLyrics` 조건은 그대로.
- 표시 방식: `CaptionStyle.Display`(`auto`|`text`|`score`|`scoretext`, 없거나 모르는 값 → `auto`) 저장, 콘솔 라디오 맨 앞 "자동". 자동: 내용 그림이 있고 준비 결과 오선이 있으면 악보만, 아니면 자막(렌더 경로 악보 슬라이드는 자막 — design.md의 허용 한계).
- 허용 한계(design.md): "삶의 예배 PPT"의 링크 없는 긴 절 버튼(8장 × 3개)은 전체 글자에 남는다.

**3차에서 바꾼 것**
- `SlideLink`(슬라이드 이동 동작만) → `HasLink`(모든 `hlinkClick`).
- `ShownPicture`에서 80% 면적 조건을 뺐다(숨김·숨긴 그룹 조건은 유지).

**바뀐 테스트** (`CaptionAllTextTests`)
- `AtTheSlidesFootOnlyALinkOrAShortMarkIsLeftOut`(이름 변경): 링크 없는 "잔 액 1,234,000 원"은 남고, `hlinksldjump` 절 버튼, `noaction` 현재 절 버튼("3.사람의 마음속엔 온 가지 소원"), 링크 없는 "2/8"은 빠진다.
- `AShownPictureOnASlideWithoutTextMakesAPictureSlideAndAPictureUnderTextDoesNot`(이름 변경): 두 조각 악보만 → true, 글 옆 작은 그림 → false, 배경 그림 채우기만 → false, a035가 렌더로 보내는(회전) 슬라이드 크기 그림만·글 없음 → true(`Picture` null), 슬라이드 크기 배경 그림 위 글 → false, 숨긴 그림만 → false.

**결과**
- `dotnet test Axicultus.Tests`: 통과 3428, 건너뜀 21, 실패 0(a036 이전 기준 3418 + a036 테스트 10: `CaptionAllTextTests` 7, `CaptionConsoleTests` 2, `PowerPointWorkerTests` 1).
- `dotnet build Axicultus\Axicultus.csproj -c Release --no-incremental`: 오류 0. 바꾼 파일의 경고는 이전부터 있던 `CaptionSession.cs` EasiDS001 하나. 하네스 프로젝트 빌드 오류 0. 하네스(3.2)는 화면 1개라 미실행(`GUARD: only 1 screen(s) present`, exit 10).

**실제 자료 재조사(읽기 전용 사본 1,245개 PPT, 10,347장; `scratchpad\a036-round1\survey2`, 결과 `survey-round3.txt`)** — 1차 규칙은 조사 프로그램 안에서 다시 만들고 `CaptionDeck.Read`와 같은 XML 읽기로 3차 결과와 비교했다.
- 실제 본문 마지막 줄 18개(재정 잔액 14, 비전 2, "※ 지출 소요 외 …" 1, "산소망 교회, 2025년 12월" 1): 18개 모두 남는다.
- 아래 가장자리의 링크 있는 개체 27,298개: 모두 빠진다(2차에서 새로 들어가던 `noaction` 현재 절 버튼 18개 포함, 남는 것 0).
- 찬송가 폴더의 링크 없는 표시 242개: 218개 빠지고 24개("삶의 예배 PPT", 허용 한계) 남는다.
- 렌더 경로 악보 슬라이드 151장(29개 PPT, a036 이전에 렌더 OCR로 가사를 얻던 글 없는 그림 슬라이드): 151장 모두 `HasPicture`(2차에서 남았던 슬라이드 크기 그림 30장 포함).
- `HasPicture`: 1차(내용 그림만) 9,213장 → 3차 9,364장. 차이 151장은 모두 위 렌더 경로 악보 슬라이드다. 글이 있는 슬라이드 중 `HasPicture`가 바뀐 것 0장.
- 1차 대비 전체 글자가 바뀐 슬라이드 26장(실제 본문 줄 18장 + "삶의 예배" 8장), 빈 글 ↔ 글로 바뀐 슬라이드 0장.
- `OfferRecognizedLyrics`가 인식 가사로 넘어가는 PPT(모든 슬라이드 전체 글자가 같거나 비어 있음): 1차 1,146개, 3차 1,146개, 바뀐 PPT 0개.
- 1차의 다른 수치(읽는 순서가 바뀐 슬라이드 249장, 1장 상자가 없는 뒤 슬라이드 1,664장/305개 PPT, 내용 그림 중 오선 9,104장)는 3차 변경과 관계없다.

**1·2차 열린 문제 처리**: 1차 1·2, 2차 1·3은 위 규칙으로 해결, 2차 2("삶의 예배")는 허용 한계로 design.md에 기록됨. 남은 열린 문제 없음.

### 구현 에이전트 4차 — 적대적 리뷰 1차 반영, 현재 동작 (2026-11-24)

1~3차 절은 진행 기록이다. 리뷰 기준이 되는 현재 동작은 이 절에 적고, 5차에서 바뀐 점은 5차 절에 적는다.

**현재 동작**
- 전체 글자 상자(`CaptionDeck.AllTextKey = "slide:all"`, "슬라이드 전체 글자")가 모든 슬라이드의 첫 상자다.
  - pptx(`CaptionDeck.Read` → `AllText`): 보이는 `p:sp` 글과 표(칸은 공백, 행은 줄). 빼는 것: 숨긴 개체, 숨긴 그룹 안 개체(위치가 없어도), `sldNum`/`dt`/`ftr` 자리표시자, 슬라이드 밖 개체, `mc:Fallback` 안의 중복, 아래 가장자리 개체(위쪽 ≥ 88%, 높이 ≤ 8%) 중 `hlinkClick`이 있거나(동작 무관; 개체·글·감싼 그룹·같은 부모 그룹, `CaptionDeck.HasLink`) 글이 6자 이하인 것.
  - 순서: 5% 띠 → 왼쪽. 위치 없는 자리표시자는 PowerPoint처럼 상속(`CaptionDeck.Inherited`): 레이아웃에서 같은 idx, 없으면 같은 type; 그래도 위치가 없으면 마스터에서 type으로만(`ctrTitle`→`title`, `subTitle`/`obj`/type 없음→`body`). 못 찾거나 레이아웃·마스터를 읽다 어떤 예외가 나도 그 상자는 끝에 두고 PPT 읽기는 계속한다.
  - 공통 도우미 `AllText(texts)`(pptx·`.ppt` 모두): 글 전체가 번호 표시(`^\(?\d{1,2}(\.|\)|/\d{1,2})?$`: "1", "2.", "(3)", "4)", "2/4", "12/16")인 상자는 뺀다. 빈 줄은 하나로. `.ppt`는 `WithAllText`로 PowerPoint가 준 순서.
- `HasPicture` = `Picture != null` 또는 (전체 글자가 공백을 뺀 12자 이하(`CaptionDeck.ShortText`, 빈 글 포함)이고, 숨기지 않은 `p:pic`(숨긴 그룹 제외, 크기 무관)이 있거나 슬라이드 자신의 `p:bg/p:bgPr/a:blipFill`이 있음). 레이아웃·마스터 배경은 보지 않는다. `.ppt`는 null. OCR은 `HasPicture != false`인 슬라이드만.
- 그림 속 가사 읽기 뒤(`RecognizeDeckAsync`): 슬라이드마다 "그림 속 가사" 상자를 넣고(기존), `OfferRecognizedLyrics`(조건 그대로)를 판단한 다음, 오선을 찾은 그림(읽은 줄이 1개 이상)에서 읽은 가사가 있으면 그 슬라이드의 전체 글자가 12자 이하일 때 전체 글자를 그 가사로 바꾼다(`CaptionSession.AddRecognized(slide, text, staves: true)`). 오선이 없는 그림(사진)의 글은 바꾸지 않는다. 전환 판단을 먼저 하므로 악보 PPT가 인식 가사 상자로 넘어가는 동작(줄별 가사 포함)은 그대로다.
- `TextFor(slide, key)`: 수동 문구 → 그 키 상자 → 그 슬라이드의 전체 글자. 키가 "그림 속 가사"인데 그 슬라이드에서 읽은 가사가 비었으면 전체 글자.
- 고른 상자(`CaptionConsole._chosenKey`): 운영자가 목록에서 고를 때(`BoxSelected` → `Choose`)만 바뀐다. 슬라이드가 바뀌어 목록을 다시 채울 때(`ListBoxes`, `_listing` 동안)는 고른 상자가 있으면 그것을, 없으면 전체 글자를 목록에서 가리키지만 세션의 `BoxKey`와 복구 저장(`SaveAsync`가 `_chosenKey` 저장)은 바뀌지 않는다. 새로 읽은 PPT는 전체 글자, 복구는 저장한 상자, 인식 가사 전환은 `Choose(RecognizedKey)`.
- 표시 방식: `CaptionStyle.Display`(`auto`|`text`|`score`|`scoretext`, 없거나 모르는 값 → `auto`) 저장·복원, 콘솔 라디오 맨 앞 "자동". 자동: 내용 그림이 있고 준비 결과 오선이 있으면 악보만, 아니면 자막. 다음·이전(↓↑, 버튼)은 그 슬라이드의 악보 줄이 실제로 송출 중일 때(`_rowShown`: `UpdateScore`가 줄을 내보냈을 때)만 줄을 옮기고, 아니면 자막 모드처럼 슬라이드를 넘긴다.
- 허용 한계(design.md): "삶의 예배 PPT"의 링크 없는 긴 절 버튼, `.ppt`의 숨긴 개체·번호·절 버튼, 렌더 경로 악보 슬라이드의 자동 표시(자막).

**리뷰 지적별 처리**
- R1(따라가는 중 고른 상자가 덮어써짐): 고침. 목록 선택을 운영자 선택(`_chosenKey`)과 분리했다. 테스트 `CaptionConsoleTests.TheChosenBoxStaysChosenOverASlideWithoutIt`(리뷰어 재현과 같은 3장: 1장 TextBox 3 선택 → 2장 "이장"·목록은 전체 글자·`BoxKey` 유지 → 3장 "삼장 상자", 복구 파일의 상자도 TextBox 3). 수정 전 코드로 되돌리면 실패함을 확인.
- R2(자동, 악보 준비 중 다음·이전이 안 보이는 줄을 옮김): 고침. 줄 이동 조건을 `ScoreOn`에서 `_rowShown`으로. 테스트 `WhileTheScoreIsMadeNextTurnsTheSlideInAuto`(준비 중 상태에서 2장 → 다음 3장, 이전 2장). 수정 전으로 되돌리면 실패함을 확인.
- R3(자리표시자 상속): 고침. 테스트 `CaptionAllTextTests.APlaceholderTakesItsPlaceAsPowerPointInheritsIt`의 A(레이아웃 idx 2 위치 없음, 마스터 idx 2는 날짜 → 마스터 body를 써서 "제목\n아멘", 이전 코드는 날짜 위치를 받아 "아멘"을 절 버튼으로 뺐다), A2(ctrTitle/subTitle → 마스터 title/body, "큰제목\n부제").
- R4(레이아웃·마스터 읽기 실패): 고침. `Inherited` 전체를 예외 처리. 같은 테스트의 깨진 레이아웃 XML: PPT를 읽고 제목은 끝("본문\n제목").
- R5(숨긴 그룹 안 위치 없는 개체): 고침. 테스트 `AShapeInAHiddenGroupAndTheFallbackCopyOfAShapeAreLeftOut`(같은 테스트에서 `mc:Fallback` 중복 제거도 확인).
- S6+S7(그림 슬라이드 = 내용 그림, 또는 12자 이하 + 보이는 그림/자기 그림 배경): 구현. 테스트 `AShownPictureOnASlideWithoutTextMakesAPictureSlideAndAPictureUnderTextDoesNot`(9장: 두 조각 악보, 글 옆 작은 그림+긴 글 → 아님, 배경 그림 채우기만 → 그림 슬라이드, 회전한 전체 크기 그림, 배경 그림 위 긴 글 → 아님, 숨긴 그림만 → 아님, 두 조각 악보 위 "에게", 배경 악보 위 "3/8", 배경 위 13자 → 아님), `ABackgroundUnderTextIsNoPictureSlide`(배경 위 긴 글 → 아님). a035 테스트의 `HasPicture` 단언(모든 슬라이드·EMF true)은 그대로 통과.
- S8(번호 표시 상자 빼기): 구현. 테스트 `ANumberMarkBoxIsNoPartOfTheWholeTextButAnAmountOrAHymnNumberIs`("1", "2.", "(3)", "4)", "12/16" 제외, "1,234,000"·"123" 유지, `.ppt` 도우미에서 "2/4" 제외).
- S9(오선을 찾은 그림의 가사로 짧은 전체 글자 대체): 구현. 테스트 `LyricsReadUnderStavesBecomeAScoreSlidesWholeTextWhenItSaysLittle`("3/8" → 가사, 오선 없음 → 그대로, 긴 글 → 그대로, 인식 가사 상자는 그대로).
- S11(인식 가사가 빈 슬라이드에서 인식 가사 키 → 전체 글자): 구현. 테스트 `FollowingTheRecognizedLyricsATextSlideGivesItsWholeText`.
- 리뷰어가 빠졌다고 한 테스트: 아래 가장자리의 글(run) 링크·감싼 그룹 링크·같은 그룹 그림 링크(`AtTheSlidesFootOnlyALinkOrAShortMarkIsLeftOut` 2장), `mc:Fallback` 중복(위 R5 테스트), 저장된 자동 아닌 표시 방식 복원(`CaptionConsoleTests.AKeptDisplayChoiceIsCheckedWhenTheConsoleOpens`: "score" → "악보" 라디오만 선택).

**결과**
- `dotnet test Axicultus.Tests`: 통과 3436, 건너뜀 21, 실패 0(a036 이전 기준 3418 + a036 테스트 18: `CaptionAllTextTests` 12, `CaptionConsoleTests` 5, `PowerPointWorkerTests` 1).
- `dotnet build Axicultus\Axicultus.csproj -c Release --no-incremental`: 오류 0. 바꾼 파일의 경고는 이전부터 있던 `CaptionSession.cs` EasiDS001 하나. 하네스 프로젝트 빌드 오류 0. 하네스(3.2)는 화면 1개라 미실행.

**실제 자료 재조사(읽기 전용 사본 1,245개 PPT, 10,347장; `scratchpad\a036-round1\survey3`, 결과 `survey-round4.txt`)** — HEAD 1450e5b의 동작(그림 슬라이드 = 슬라이드 XML에 그림 데이터가 하나라도 있음; 기본 자막 = 1장의 첫 텍스트 상자, 그 상자가 없는 슬라이드는 빈 자막)과 3차 규칙을 조사 프로그램 안에서 다시 만들어 4차(`CaptionDeck.Read`)와 비교했다.
- 그림 슬라이드(OCR 대상): HEAD 9,897장, 3차 9,364장, 4차 9,587장. HEAD에서 아니던 슬라이드가 4차에서 그림 슬라이드가 된 것 0장.
- 리뷰어의 배경 악보 166장(HEAD 그림 슬라이드, 3차 아님, 전체 글자 없음): 166장 모두 슬라이드 자신의 배경 그림이고, 4차에서 166장 모두 그림 슬라이드(OCR 복구).
- 리뷰어의 짧은 글이 얹힌 213장(찬송가 폴더, HEAD 그림 슬라이드, 3차 아님, 전체 글자 있음): 4차에서 41장이 그림 슬라이드. 남은 172장은 가사를 글 상자로 친 PPT(예: "충만 - 손경민", "세상에서방황할때", "감사해")라 전체 글자가 12자를 넘고 그 글이 곧 가사 자막이다. (찬송가 밖까지 합한 같은 조건 367장 중 57장 복구.)
- 3차에서 아니던 슬라이드가 4차에서 그림 슬라이드가 된 것 223장, 그중 전체 글자가 있는 것 33장. 찬송가 밖의 PPT: 장로 임직자 소개(2), 서약(4), 임직자 소개(1), 임직예배 장로 임직자 소개(2), 금요연합예배설교(1), 추수감사절_게임_PPT(1), 설교-금요기도회(1), 연합예배(1), 중고등부 온라인 예배 두 파일(각 1), 성탄축하잔치(1) — 모두 HEAD에서도 그림 슬라이드였던 짧은 글 + 그림 슬라이드다. 오선이 없는 그림이면 결정 9에 따라 전체 글자는 바뀌지 않는다.
- 레이아웃·마스터 배경: 166장은 모두 슬라이드 자신의 배경이라 상속 배경과 관계없다. 짧은 글이고 자기 그림이 없는데 레이아웃·마스터에 그림 배경이 있는 슬라이드는 전체에서 1장뿐이다(HEAD도 OCR하지 않던 슬라이드).
- 번호 표시 상자 빼기: 찬송가 폴더 454개, 그 밖 19개. 그 밖 19개는 "2019성경학교 광고"의 카운트다운 "5"~"1"(5장)과 "2026년 교회 비젼" 두 파일의 쪽 표시 "2/7"~"8/9"(14개)로, 자막에 필요한 본문은 없었다.
- 아래 가장자리의 링크 없는 본문 줄 18개(찬송가 밖): 18개 모두 남는다. 링크 있는 아래 가장자리 개체는 모두 빠진다(2·3차와 같음).
- `OfferRecognizedLyrics`가 인식 가사로 넘어가는 PPT(모든 슬라이드의 기본 자막이 같거나 비어 있음): HEAD 908개, 4차 1,152개(3차 1,146개). HEAD는 넘어가고 4차는 안 넘어가는 16개는 1장 첫 상자(제목)가 모든 슬라이드에 같았지만 가사가 글 상자로 들어 있어 전체 글자가 슬라이드마다 다른 PPT로, 4차에서는 그 가사 글이 자막이 된다(예: "평안을 너희게 주노라", "괴로울 때 주님의 얼굴 보라", 성탄 "6.합심기도"). 4차만 넘어가는 260개는 HEAD의 기본 상자(1장 첫 상자) 글이 슬라이드마다 달라 넘어가지 않던 PPT로, 기록한 표본 71개는 모두 찬송가 폴더의 악보 PPT(모든 슬라이드 전체 글자가 빔)였다.
- 기본 자막이 빈 슬라이드: HEAD 3,458장, 4차 9,505장. 4차에서 빈 슬라이드는 대부분 악보 슬라이드로, 그림 속 가사 읽기 뒤 결정 9(짧은 전체 글자 → 읽은 가사) 또는 인식 가사 전환으로 채워진다. HEAD에서 빈 자막이 아니던 악보 슬라이드의 기본 자막은 절 버튼·제목 글이었다.

**남은 열린 문제**: 없음(위 수치는 설계 결정 2·4·5·7·9의 범위 안이다).

### 구현 에이전트 5차 — 적대적 리뷰 2차 반영 (2026-11-24)

4차 절의 "현재 동작"에서 아래가 바뀌었다(나머지는 4차 그대로).

**리뷰 지적별 처리**
- F1(가사 읽기가 끝나면 송출 중 악보 줄이 1줄로 돌아감, 보던 슬라이드·고치던 문구가 덮임): 고침. `CaptionSession.AddRecognized`는 송출 중인 슬라이드의 자막 글이 바뀌었을 때만 그 글을 다시 보낸다(`Resend`: `CurrentSlide`를 알리지 않으므로 콘솔이 줄을 0으로 돌리거나 슬라이드 목록을 되돌리지 않는다; 같은 글이면 아무것도 보내지 않는다). 인식 가사 전환(`OfferRecognizedLyrics` → `RefreshBoxes`)도 `Follow` 대신 새 `CaptionSession.ChangeBox`로 키만 바꿔 같은 방식으로 글만 다시 보낸다. `RecognizeDeckAsync` 끝의 무조건 `RefreshCue()`는 `UpdateScore()`(줄은 그대로, 그 줄의 가사 갱신)와 `ListBoxes(keepEdit: true)`로 바꿨다. 문구 칸은 콘솔이 마지막으로 넣은 글(`_cueShown`)과 다르면(저장 안 한 고침) 가사 읽기 쪽 갱신에서 덮지 않는다. 테스트 `NewLyricsForTheSlideOnAirChangeOnlyItsCaptionText`(악보+자막 2/2줄에서 `AddRecognized` → 여전히 2/2줄, 자막 글 갱신 1회, 같은 글이면 0회; 2장을 보며 고치던 문구는 `AddRecognized`와 인식 가사 전환 뒤에도 그대로, 보던 슬라이드 2장 유지)과 `LyricsReadAfterTheScoreRowWentOnAirKeepTheRowAndThenReplaceShortWholeText`(가짜 OCR이 2/2줄 송출 중에 끝남 → 2/2줄 유지). 4차 동작(`MoveToSlide`)으로 되돌리면 두 테스트가 실패함을 확인.
- F2(결정 9로 바뀐 전체 글자의 악보+자막 줄별 가사): 고침. 줄별 가사 조건을 "고른 키가 인식 가사"에서 "보내는 자막 글이 그 슬라이드의 인식 가사와 같음"으로 바꿨다(수동 문구는 `TextFor`가 먼저라 그대로 제외). 테스트 `AWholeTextReplacedByLyricsSendsEachRowItsOwnLineUnderTheScore`(악보 아래 "에게" → 가사로 바뀌고, 전체 글자를 따라가는 악보+자막에서 1줄 "1장 첫 줄", 2줄 "1장 둘째 줄").
- F3(고른 상자가 없는 슬라이드에서 전체 글자를 고를 수 없음): 고침. 그 슬라이드에서는 목록에 선택이 없다(`ListBoxes`). 문구 칸과 자막은 그 슬라이드의 전체 글자(`RefreshCue`가 `_chosenKey`로 `TextFor`). 거기서 "슬라이드 전체 글자"를 고르면 새 선택이 된다. `TheChosenBoxStaysChosenOverASlideWithoutIt`에 리뷰어 U1b 경우를 더함(2장에서 선택 없음·문구 "이장" → 전체 글자 고름 → `BoxKey` 전체 글자 → 3장 "삼장 상자\n삼장 다른"). 콘솔을 다시 열 때 세션 키가 비어 있으면 전체 글자로 본다.
- F4: 고침. 읽지 못한 레이아웃·마스터는 PPT를 읽는 동안 "없음"(null)으로 기억해 다시 읽지 않는다(`parsed`가 `Dictionary<Uri, XDocument?>`). 별도 테스트 없음(동작은 깨진 레이아웃 테스트로 확인).
- F5(복구 뒤 그림 속 가사가 없음, a036 이전부터): 고침. 복구 확인 뒤 처리를 `CaptionConsole.RestoreAsync`로 떼고, 복구한 PPT에 대해 악보 그림 준비(`StartScore`, `PrepareDeckAsync`와 같은 도우미)와 그림 속 가사 읽기를 다시 한다. 이때 `RecognizeDeckAsync(deck, offer: false)`라 `OfferRecognizedLyrics`는 하지 않고 복구한 선택을 지킨다(결정 9의 전체 글자 대체는 한다). 테스트 `ARestoredDeckHasItsLyricsReadAgainAndKeepsTheRestoredChoice`(전체 글자로 복구 → 모든 전체 글자가 비어도 인식 가사로 넘어가지 않고 인식 가사는 다시 생김; 인식 가사로 복구 → 문구 칸이 그 가사).
- a(번호 표시 정규식): `^\(?[0-9]{1,2}(\.|\)|[/-][0-9]{1,2})?$`(ASCII 숫자만, "3-4" 추가). 테스트 `ANumberMarkBoxIsNoPartOfTheWholeTextButAnAmountOrAHymnNumberIs`에 "3-4" 제외, 전각 "１"·아랍 "٣" 유지를 더함.
- 테스트 빈틈: `LyricsReadUnderStavesBecomeAScoreSlidesWholeTextWhenItSaysLittle`는 "3/8" 대신 번호 표시가 아닌 "에게"를 쓴다. 결정 9의 콘솔 연결 테스트 `LyricsReadAfterTheScoreRowWentOnAirKeepTheRowAndThenReplaceShortWholeText`: 악보 2장(전체 글자 빔)과 사진 1장(오선 없음, 렌더 대역)을 가짜 OCR로 읽음 → 인식 가사로 전환됨(전환 판단이 대체보다 먼저: 대체가 먼저였다면 전체 글자가 서로 달라 전환되지 않음), 그 뒤 악보 슬라이드 전체 글자는 가사로 바뀌고 사진 슬라이드는 그대로(줄을 찾은 경우에만 대체).
- OCR 대역: 테스트를 위해 `CaptionConsole.ReadLyrics`(기본값 null이면 OCR 도우미), 기다릴 수 있는 `CaptionConsole.LyricsReading`을 더했다. `WithConsole`에 렌더 서비스 인자를 더했다.

**결과**
- `dotnet test Axicultus.Tests` 두 번: 1회 통과 3440·건너뜀 21·실패 0, 2회 통과 3440·건너뜀 21·실패 0(4차 3436 + 새 테스트 4). `PowerPointWorkerTests`만 따로: 7개 통과. a036이 `PowerPointWorker.cs`에서 바꾼 것은 `ReadLegacyDeckAsync`가 받은 슬라이드에 `WithAllText`를 붙이는 한 줄뿐이고, 리뷰어가 한 번 본 `ExportOverItsTimeLimitStillKillsTheChild`(렌더 시간 초과 시 자식 종료, `WaitForExit(2000)` 타이밍)의 경로는 건드리지 않았다.
- a036 테스트: 22개(`CaptionAllTextTests` 12, `CaptionConsoleTests` 9, `PowerPointWorkerTests` 1).
- `dotnet build Axicultus\Axicultus.csproj -c Release --no-incremental`: 오류 0, 바꾼 파일의 경고는 이전부터 있던 `CaptionSession.cs` EasiDS001 하나. 하네스 빌드 오류 0, 하네스(3.2)는 화면 1개라 미실행.

**실제 자료(읽기 전용 사본, `survey3`, 결과 `survey-round5.txt`)** — 새 정규식으로 다시 셌다.
- 번호 표시 상자 빼기: 찬송가 폴더 470개(4차 454개 + "1-4"·"2-4" 같은 줄표 표시 16개), 그 밖 19개(4차와 같음: 성경학교 광고 카운트다운 5개, 교회 비젼 두 파일 쪽 표시 14개).
- 나머지 수치는 4차와 같다: 그림 슬라이드 HEAD 9,897 / 4차 9,587(HEAD에서 아니던 것 0), 배경 악보 166장 모두 복구, 짧은 글 213장 중 41장, 아래 가장자리 본문 줄 18개 유지, `OfferRecognizedLyrics` 전환 PPT HEAD 908 / a036 1,152.

**남은 열린 문제**: 없음.

### 구현 에이전트 6차 — 적대적 리뷰 3차·`/code-review` 반영 (2026-11-24)

4·5차 절의 "현재 동작"에서 아래가 바뀌었다(나머지는 그대로).

**처리**
- CR1(그림 슬라이드 판정, 결정 5 개정): `HasPicture` = (가) `Picture != null` 또는 (나) 숨기지 않은 `p:pic`(숨긴 그룹 제외) 중 그룹 변환 뒤 넓이가 슬라이드의 80% 미만인 것(위치가 없는 그림 포함)이 있음 — 글 길이 무관 — 또는 (다) 전체 글자 12자 이하이고 보이는 그림(크기 무관)이나 자기 `p:bg` 그림 배경이 있음(`CaptionDeck.ShownPictures`가 그림마다 넓이를 준다). 테스트 `AShownPictureOnASlideWithoutTextMakesAPictureSlideAndAPictureUnderTextDoesNot`: 글 옆 작은 그림 → 참으로 바뀜, "79장 주 하나님 지으신 모든 세계" 제목 아래 두 조각 악보 → 참(새 10번째 장), 전체 크기 배경 그림 위 긴 글 → 거짓, 그림 배경 위 13자 → 거짓. `ABackgroundUnderTextIsNoPictureSlide`(배경 채우기·전체 크기 그림 위 긴 글 → 거짓)는 그대로 통과.
- CR2: 콘솔 생성자의 라디오 선택은 `DisplayRadio()`(맞는 것이 없으면 "자동")로, 예외가 나지 않는다.
- CR3: `CaptionSession.SetStyle`이 `Display`를 `ParseDisplay`로 정리하고 `PropertyChanged(Display)`를 알린다. 콘솔은 그것을 받아 맞는 라디오를 선택한다(선택이 같은 값을 다시 쓰므로 아무것도 바뀌지 않음). 테스트 `TheDisplayChosenElsewhereChecksItsRadio`("scoretext" → "악보 + 자막" 선택, "fancy" → auto·"자동").
- CR4: `RestoreAsync`도 저장된 상자가 비었으면 전체 글자(생성자와 같음). `ARestoredDeckHasItsLyricsReadAgainAndKeepsTheRestoredChoice`에 추가.
- CR5(a036 이전부터): `UpdateScore`가 그 슬라이드의 줄을 만들 수 없을 때(렌더 그림 없음, `CropRow` 실패) 이전 줄(다른 슬라이드의 줄일 수 있음)을 내린다(`SetImage(null)`). 택한 쪽: 그림을 내림 — 송출은 그 슬라이드의 글, `_rowShown`은 거짓이라 화살표는 슬라이드를 넘긴다(송출 내용과 화살표 동작이 일치; a035가 내용 그림을 다시 읽는 동안 줄을 내리는 것과 같은 방식). 테스트 `WithoutARowOfTheSlideTheLastRowIsTakenDownAndTheArrowsTurnSlides`.
- CR6(a036 이전부터): 문구 칸에 저장 안 한 고침이 있으면(`CueEdited`: 콘솔이 넣은 글과 다름) 송출 슬라이드가 바뀌어도 보던 슬라이드를 옮기지 않고 고침을 덮지 않는다. 저장(`SaveOverride`)하거나 운영자가 다른 슬라이드를 보면(고침 버림) 다시 따라간다. 테스트 `AnUnsavedCaptionEditStaysWhileTheSlideOnAirMoves`.
- CR7(a036 이전부터): 운영자가 상자를 고를 때 `Follow` 대신 `ChangeBox`(글만 다시 보냄) — 보던 슬라이드와 송출 중 악보 줄이 그대로. 테스트 `PickingABoxOnAnotherSlideKeepsTheViewAndTheRowOnAir`(송출 1장 2/2줄, 5장을 보며 상자 선택 → 5장 유지, 1장 2/2줄 유지, 키 바뀜).
- RV1: 그림 속 가사 읽기 뒤의 처리(인식 가사 넣기, 전환, 전체 글자 대체, `UpdateScore`, 목록)를 `CaptionSession.Hold()` 안에서 해 한 번만 내보낸다(악보+자막에서 가사 전체가 잠깐 나갔다가 줄 가사로 바뀌던 깜빡임 없음). 테스트 `SwitchingToTheLyricsUnderAScoreRowGoesOutOnce`(2줄 송출 중 읽기 완료 → 내보낸 글은 "1장 둘째 줄" 한 번).
- RV3(a036 이전부터): 복구할 PPT 파일이 없으면 경고만 하고 초안·예배 순서·PPT 어느 것도 바꾸지 않는다. 테스트 `ARecoveryWhoseDeckIsGoneChangesNothing`.
- 위 CR6·CR7·RV1 테스트는 고치기 전 코드로 되돌려 실패함을 확인했다.

**결과**
- `dotnet test Axicultus.Tests` 두 번: 통과 3446·건너뜀 21·실패 0, 두 번 모두 같음(5차 3440 + 새 테스트 6). a036 테스트 28개(`CaptionAllTextTests` 12, `CaptionConsoleTests` 15, `PowerPointWorkerTests` 1).
- Release 빌드(`--no-incremental`) 오류 0, 바꾼 파일의 경고는 이전부터 있던 `CaptionSession.cs` EasiDS001 하나. 하네스 빌드 오류 0, 하네스(3.2)는 화면 1개라 미실행.

**실제 자료(읽기 전용 사본 1,245개 PPT, 10,347장; `survey3`, 결과 `survey-round6.txt`)**
- 그림 슬라이드: HEAD 9,897장, 5차 규칙 9,587장, 6차 9,777장. HEAD에서 아니던 슬라이드가 6차에서 그림 슬라이드가 된 것 0장. 5차보다 빠진 것 0장.
- (나)가 더한 190장(모두 전체 글자 12자 초과): 찬송가 폴더 150장, 그 밖 40장(교회 비젼 두 파일 13장, 성전 이전 추진 자료 7장, 성탄축하잔치 3장, 성탄예배-1부 2장, 설교·순서·송구영신 등 15개 PPT 각 1장). 모두 HEAD에서도 그림 속 가사를 읽던 슬라이드다.
- 리뷰 1차의 짧은 글이 얹힌 찬송가 213장 중 191장이 이제 그림 슬라이드(5차 41장). 같은 조건 전체 367장 중 247장.
- 결정 9는 전체 글자가 12자 이하일 때만 바꾸므로(`AddRecognized`의 `ShortText` 조건) (나)로 더해진 190장의 긴 전체 글자는 바뀌지 않는다(대체 0장). `OfferRecognizedLyrics` 전환 PPT: HEAD 908개, a036 1,152개(5차와 같음 — `HasPicture`는 전체 글자를 바꾸지 않는다).
- 나머지(번호 표시 470/19, 아래 가장자리 본문 줄 18개 유지, 배경 악보 166장)는 5차와 같다.

**남은 열린 문제**: 없음.
