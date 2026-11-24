## Phase 0. 원인 확인

- [x] 0.1 하네스 로그 `%LOCALAPPDATA%\AxicultusHarness\logs\a035-a037-final.log`의 세 사례 원인 확인(특히 2019성경학교 광고 33장: 결정 9 대체인지, 어떤 그림·행 판정으로 "오선 있음"이 되었는지)
- [x] 0.2 기준 테스트 결과 기록

## Phase 1. 실패하는 테스트

- [x] 1.1 슬라이드 1장 PPT(입력 글 + 그림)에서 그림 속 가사 읽기 뒤 자동 전환 안 함, 모든 슬라이드 빈 글이면 전환
- [x] 1.2 결정 9 대체: 한글 4자 미만/한글 비율 60% 미만/오선 아님이면 대체 안 함, 조건을 채우면 대체
- [x] 1.3 전체 글자 줄 안 연속 공백·앞뒤 공백 정리(pptx, .ppt 공통 도우미)

## Phase 2. 구현

- [x] 2.1 `OfferRecognizedLyrics` 조건
- [x] 2.2 결정 9 대체 조건(원인이 오선 판정이면 그 판정도)
- [x] 2.3 `AllText` 공백 정리
- Constraints: 식별자·주석 영어. 다른 동작 불변.

## Phase 3. 회귀

- [x] 3.1 Release 빌드 오류 0, 전체 테스트 두 번 통과
- [x] 3.2 실제 자료 사본 전수 조사: 자동 전환 PPT 수, 결정 9 대체 슬라이드 수의 변화(전후), 공백 정리로 바뀐 슬라이드 수
- [x] 3.3 하네스 `pptcaption,ppttext` 재실행(화면 2개, 격리 사본), 세 사례 확인, 실제 데이터 unchanged

## Phase 4. 리뷰

- [x] 4.1 적대적 리뷰 지적 사항 처리 — 큰 지적 없음, NBSP 공백·주석 위치는 2차에서 수정, 나머지는 proposal의 허용 한계
- [x] 4.2 `/code-review` 지적 사항 처리 — 5건(중요 3: 줄 없음 판정 범위, 1장 PPT 짧은 제목, 문자만 세는 한글 비율) 모두 2차에서 수정
- [x] 4.3 총괄 완료 검증(2026-11-24) — 전체 테스트 3560 통과 / 22 건너뜀 / 0 실패, `openspec validate --strict` 통과, 하네스(화면 2개) 1·2차 모두 세 사례 정상·실제 데이터 unchanged

## 검증 결과

(구현 에이전트와 총괄이 채운다)

### 구현 에이전트 1차 (2026-11-24)

**Phase 0**
- 0.2 기준(HEAD 498567e): `dotnet test Axicultus.Tests` 통과 3554, 건너뜀 22, 실패 0.
- 0.1 원인:
  - 찬양대-2부/3부: 슬라이드 1장이라 `OfferRecognizedLyrics`의 "고른 상자(전체 글자)가 모든 슬라이드에서 같다"가 늘 참 → 인식 가사로 전환되어 OCR 오인식 글("산양하네 빚 되신 구주", "기on Ch0ir")이 나갔다.
  - 2019성경학교 광고 33장: 결정 9의 전체 글자 대체다. 이 슬라이드는 내용 그림(`image21.jpeg`, 정자와 잔디 사진)이 있지만 a035 판정(`ScoreLayout.Layout`)으로는 오선이 없어(줄 0) 렌더 경로로 OCR했다. `RecognizeDeckAsync`의 렌더 경로는 렌더 그림에 `ScoreLayout.FindRows`(렌더용, 덜 엄격)를 써서 줄 수를 정하는데, 정자 지붕의 가로 판자들이 오선 하나로 읽혀(사진만으로 재 봐도 `FindRows` 1줄: staff 179-290) "오선에서 읽은 가사"가 되었고, 전체 글자가 빈 글(짧음)이라 읽은 "하*날1까 `"로 바뀌었다. 전환(`OfferRecognizedLyrics`)은 이 PPT의 전체 글자가 슬라이드마다 달라 일어나지 않았다.
  - 겟세마네: 전체 글자가 상자 글의 연속 공백을 그대로 이었다.

**Phase 1** (실패 확인 후 구현): 새 테스트 4개.
- `CaptionConsoleTests.AOneSlideDeckWithTypedTextKeepsItsTextAfterTheLyricsAreRead`: 입력 글이 있는 1장 PPT는 읽기 뒤 전체 글자 그대로(전환 안 함), 글 없는 1장 PPT는 인식 가사로 전환. 고치기 전: 첫 PPT가 `ocr:lyrics`로 전환되어 실패.
- `CaptionConsoleTests.APhotoWhoseRenderLooksLikeAStaffDoesNotReplaceTheWholeText`: 1장 = 내용 그림이 사진(오선 없음)인데 렌더(대역)에 오선이 보임, 2장 = 두 조각 악보(내용 그림 없음). 1장 전체 글자는 빈 글 그대로이고 인식 가사 상자에는 읽은 글, 2장 전체 글자는 가사로 대체. 고치기 전: 1장 전체 글자가 "사진 속 간판"으로 바뀌어 실패(하네스 33장과 같은 경로).
- `CaptionAllTextTests.OnlyTextThatReadsAsKoreanLyricsReplacesAScoreSlidesWholeText`: "하*날1까 `"(음절 3), "주 하나님 love you all"(음절 4, 한글 29%), "주하나님 abc"(57%), 오선 아님 → 대체 안 함; "주 하나님 지으신" → 대체. 인식 가사 상자에는 읽은 그대로.
- `CaptionAllTextTests.RunsOfSpacesInsideALineOfTheWholeTextBecomeOneSpace`: pptx "겟 세    마  네  동  산" → "겟 세 마 네 동 산", 앞뒤 공백·탭 줄 정리, `.ppt` 도우미도 같음.
- 테스트 도우미: `ReadLyrics`의 사진 줄 위치를 첫 오선 아래(177)로 옮김(기존 테스트 결과 불변), 렌더 대역 `StaffRender`(불투명 악보 그림) 추가, `CaptionConsoleTests.ScoreSlide`를 internal로.

**Phase 2 구현**
- 2.1 `CaptionConsole.OfferRecognizedLyrics`: 고른 상자의 글이 모든 슬라이드에서 비었거나, 슬라이드가 2장 이상이고 모두 같을 때만 전환.
- 2.2 결정 9: `CaptionSession.AddRecognized`가 `staves`이고 `CaptionSession.Lyrical(text)`(한글 음절 4자 이상, 공백 뺀 글자의 60% 이상)일 때만 짧은 전체 글자를 대체(인식 가사 상자는 그대로 채움). 오선 판정: `RecognizeDeckAsync`의 렌더 경로에서, 내용 그림이 있는데 a035 판정으로 오선이 없던 슬라이드(사진)는 렌더의 `FindRows` 결과를 쓰지 않고 줄 없음으로 읽는다(줄 나누기 없이 모든 줄을 읽고, 오선에서 읽은 것으로 보지 않음). a035 내용 그림의 줄(`Layout`)과 내용 그림이 없는 렌더 경로 악보의 줄은 그대로다.
- 2.3 `CaptionDeck.AllText`(pptx·`.ppt` 공통): 각 줄의 스페이스·탭 연속을 하나로 줄이고 줄 앞뒤 공백을 지운 뒤 기존대로 빈 줄을 하나로.

**Phase 3**
- 3.1 `dotnet test Axicultus.Tests` 두 번: 1회 통과 3558·건너뜀 22·실패 0, 2회 통과 3558·건너뜀 22·실패 0(기준 + 새 테스트 4). `dotnet build Axicultus\Axicultus.csproj -c Release --no-incremental` 오류 0, 바꾼 파일에서 나온 경고 없음. 하네스 프로젝트 빌드 오류 0.
- 3.2 실제 자료 사본 1,245개 PPT, 10,347장(`scratchpad\a036-round1\survey4`, 읽기 전용; 결과 `survey-a038.txt`):
  - 자동 전환 조건에 걸리는 PPT(기본 상자 = 전체 글자 기준): 전 1,152개 → 후 1,119개. 빠진 33개는 모두 1장짜리 PPT에 입력 글이 있는 경우(기관찬양축제 순서 10개, 재정집행현황 10개, 찬양대-2부/3부 두 폴더 4개, 임직예배 2개, 송구영신·신년선포 3개 등). 새로 걸리는 PPT 0개.
  - 결정 9 대체: 실제 OCR 없이는 셀 수 없어 구조로 셌다. 글이 짧고 내용 그림이 있는데 a035로는 오선이 없는 사진 슬라이드 중 그림만으로도 `FindRows`가 오선을 찾는 것(고치기 전 렌더 경로에서 잘못 대체될 수 있던 슬라이드): 9장(2019성경학교 광고 8장 — 33장 포함, 2026년선교비전 1장). 이제 0장(이런 슬라이드는 오선 없음으로 읽음). 그 밖의 대체는 한글 조건이 추가로 거른다.
  - 공백 정리로 전체 글자가 바뀐 슬라이드: 402장(예: 성경 구절 제목 줄 끝 공백 "■ 1절 /…" → "■ 1절/…", 음절마다 띄운 찬양 가사). 조사 프로그램이 다시 만든 이전 전체 글자에 공백 정리를 적용하면 10,347장 모두 지금 결과와 같다(다른 변화 없음).
- 3.3 하네스(화면 2개, 격리 사본, `Run-Harness.ps1 -Phases 'pptcaption,ppttext' -Name a038-final`, `AXI_A036_DECKS` = 총괄과 같은 사본 4개; 로그 `%LOCALAPPDATA%\AxicultusHarness\logs\a038-final.log`):
  - 찬양대-2부: "할렐루야 찬양대 / 찬양하네 빛 되신 구주"(입력 글), 찬양대-3부: "시온 찬양대 / Zion Choir / 나는 아네 그의 계획 / 'I Know the Plans…"(입력 글). 로그 전체에 "산양하네", "기on Ch0ir" 없음.
  - 2019성경학교 광고 33장: 빈 자막(""). 로그에 "하*날1까" 없음.
  - 겟세마네 동산에서 기도 하실 때: "겟 세 마 네 동 산 에 서 기 도 하 실 - 때 / …"(연속 공백 없음).
  - 실제 데이터 세 폴더 모두 `unchanged:`(`%APPDATA%\Axicultus` 259 files, `C:\Axicultus\Admin` 93 files, `C:\EasiSlides\Admin` 92 files). `POWERPNT before=0 after=1`: 스크립트가 끝난 직후의 집계로, 곧바로 다시 확인했을 때 PowerPoint 프로세스는 남아 있지 않았다(직접 종료하지 않음). `LEFTOVER PROCESSES: Axicultus.Tests#34036`은 하네스가 아니라 같은 시각에 돌던 내 조사 프로그램(`scratchpad\a036-round1\survey4\...\Axicultus.Tests.exe`, 이름이 같음)이며 조사가 끝나 스스로 종료했다. 처음 보는 예외 없음(FIRSTCHANCE 4건은 이전 실행 로그와 같은 PowerPoint 쇼 창 찾기 경로).

### 구현 에이전트 2차 — 적대적 리뷰·`/code-review` 반영 (2026-11-24)

**처리**
- C1(오선 없음 판정의 범위): 렌더 경로에서 줄을 쓰지 않는 것은 이 PPT의 악보 그림 준비가 끝났고(실패·취소 아님) 그 슬라이드의 내용 그림을 읽어 a035 판정이 실제로 오선을 찾지 못했을 때만이다. `PrepareScoreCoreAsync`가 성공하면 `_noStaves`(PPT, 슬라이드 집합: 내용 그림이 있고 오선 줄이 없으며 실패 목록에 없는 슬라이드)를 남긴다. 준비 실패·취소, 그림 읽기 실패(실패 목록), 준비 전(`StartScore`·준비 시작에서 비움), 오선이 있던 슬라이드의 `OcrPicture` 실패·`LoadPictureAsync` 제거는 모두 이 집합에 없으므로 a038 이전처럼 `ScoreLayout.FindRows(렌더)`를 쓴다. 테스트 `CaptionConsoleTests.APictureThatCouldNotBeMeasuredLeavesTheRenderItsStaves`(내용 그림이 깨진 PNG → 렌더 줄 유지 → 전체 글자가 가사로 대체) 추가, `APhotoWhoseRenderLooksLikeAStaffDoesNotReplaceTheWholeText`(실제로 오선 없음 → 줄 없음)는 그대로 통과.
- C2(1장 PPT의 짧은 제목): 1장 PPT는 고른 상자 글이 12자 이하이고(`CaptionDeck.ShortText`, 빈 글 아님) 그 장의 인식 가사가 `CaptionSession.Lyrical`이면 전환한다. 12자 넘는 입력 글(찬양대 사례)은 그대로. 테스트 `AOneSlideDeckWithAShortTitleSwitchesToLyricsThatReadAsLyrics`("찬양대 2부" + 한글 가사 읽음 → 전환, 같은 PPT + "하*날1까 `" → 전환 안 함, 전체 글자 "찬양대 2부" 유지).
- C3(`Lyrical` 분모): 공백 뺀 모든 글자가 아니라 문자(`char.IsLetter`)만 센다(숫자·기호·음표 기호 제외), 한글 음절 4자 이상은 그대로. 한글 음절 범위는 문자 상수 대신 `(char)0xAC00`~`(char)0xD7A3`로 썼다(코드에 한글 없음). `OnlyTextThatReadsAsKoreanLyricsReplacesAScoreSlidesWholeText`에 "1. 주 하나님 (2x)!!" → 참(문자 5 중 4), "1. 주 J 하 | 나 o 님 (2x)" → 거짓(문자 7 중 4, 57%), "하*날1까 `" → 거짓(음절 3)을 더함.
- C4(공백 정규식): `[^\S\n]+`(줄 안의 모든 공백: 줄바꿈 없는 공백(NBSP)·전각 공백 포함). `RunsOfSpacesInsideALineOfTheWholeTextBecomeOneSpace`에 U+00A0·U+3000 사례를 더함.
- C5: `Lyrical`을 `AddRecognized` 위로 옮기고 한 줄 요약을 달아 `AddRecognized`가 자기 `<summary>`를 갖게 했다.
- 고치기 전 코드로 되돌려 C1·C2 새 테스트가 실패함을 확인했다.

**결과**
- `dotnet test Axicultus.Tests` 두 번: 1회 통과 3560·건너뜀 22·실패 0, 2회 통과 3560·건너뜀 22·실패 0(1차 3558 + 새 테스트 2). `dotnet build Axicultus\Axicultus.csproj -c Release --no-incremental` 오류 0, 바꾼 파일에서 나온 경고 없음.
- 실제 자료 사본(`survey4`, 결과 `survey-a038-r2.txt`):
  - 공백 정리: 1차 규칙(스페이스·탭)으로 바뀐 슬라이드 402장, 넓힌 규칙이 더 바꾼 슬라이드 10장(모두 "성령의 불길 - 참참참 피 흘리신" 1~10장: 음절 사이 줄바꿈 없는 공백(NBSP)). 합계 412장.
  - 자동 전환: 1차 규칙 1,119개 PPT(a038 이전 1,152개)에 더해, C2로 전환될 수 있는 1장 PPT(짧은 제목 + 그림 슬라이드)는 3개 — "연합예배"("연합/예배/추수 감사의 노래"), "하나님께로 더 가까이 ", 청년부예배 "찬양대-3부"("시온/찬양대/거룩하신 하나님", 공백 뺀 12자). 실제 전환 여부는 읽은 글이 `Lyrical`인지에 달려 있다(OCR 없이는 셀 수 없음). 주일예배 찬양대-2부/3부는 12자를 넘어 전환하지 않는다.
- 하네스(화면 2개, 격리 사본, `Run-Harness.ps1 -Phases 'pptcaption,ppttext' -Name a038-r2`, `AXI_A036_DECKS` 사본 4개; 로그 `a038-r2.log`): 찬양대-2부 "할렐루야 찬양대 / 찬양하네 빛 되신 구주", 찬양대-3부 "시온 찬양대 / Zion Choir / 나는 아네 그의 계획 / 'I Know the Plans…"(입력 글), 2019성경학교 광고 33장 "", 겟세마네 연속 공백 없음. 로그에 "산양하네"·"기on Ch0ir"·"하*날1까" 없음. 실제 데이터 세 폴더 모두 `unchanged:`. `POWERPNT before=0 after=1`은 끝나는 순간의 집계이고 곧바로 다시 봤을 때 PowerPoint 프로세스는 없었다(종료하지 않음). `LEFTOVER PROCESSES: Axicultus.Tests#48084`는 하네스가 아니라 `scratchpad\ocr-pdf-experiment\lib\Axicultus.Tests.exe`(20:00:23 시작, 내가 띄운 것이 아님)이며 건드리지 않았다. FIRSTCHANCE 4건은 이전 실행과 같은 PowerPoint 쇼 창 찾기 경로.
