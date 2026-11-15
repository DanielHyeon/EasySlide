# Proposal: PPT 항목의 절 버튼에 이전 항목의 값이 남는 문제 수정

## Why

미리보기 단과 송출 단의 조작 줄에는 항목의 절로 바로 가는 작은 버튼(`1`, `2`, `c` 등)이 나온다. PPT 항목에서는 이 버튼이 그 파일의 슬라이드 노트가 아니라 **직전에 올렸던 항목의 값**으로 만들어지는 경우가 있다. 같은 PPT 항목인데 실행마다 `1 c`, `c`, 버튼 없음으로 다르게 보였던 이유다.

원인은 두 가지다 (조사에서 배열 값을 기록해 확인했다).

1. **송출 단은 절 정보를 지우지 못한다.** `OfficeLib/PowerPoint.cs`의 `BuildScreenOutDumps`는 `TotalSlides`까지만 지우는데, 들어올 때 `TotalSlides`는 항상 0이다. 그래서 아무것도 지워지지 않는다. 미리보기 쪽 `BuildScreenDumps`는 전체를 지운다.
2. **이미지가 이미 만들어져 있으면 노트를 읽지 않는다.** `FrmMain.Logic.cs`의 `BuildAllPowerpointScreenDumps`는 같은 PPT를 같은 단에 다시 올릴 때 바로 돌아간다. 그 사이에 다른 항목이 절 정보를 덮어썼어도 다시 읽지 않는다. 두 단 모두 해당한다.

`c` 버튼은 "슬라이드 종류 값이 0인 슬라이드가 있다"는 뜻인데, 새 배열의 기본값이 0이라서 지우지 않은 칸이 후렴으로 읽힌다.

## What Changes

- 송출 단도 PPT를 읽기 전에 절 정보 전체를 지운다.
- 이미지를 다시 만들지 않는 경우에도 그 파일의 노트에서 절 정보를 다시 읽는다.

## Out of Scope

- 노트에서 절 표시를 해석하는 규칙 (`GetVerseIndicator`)
- 가사 항목의 절 버튼
- PPT 미리보기 속도 개선. 다만 이 수정은 이미지가 이미 있는 PPT를 다시 올릴 때 파일을 한 번 더 연다 (조사에서 0.1~0.3초).

## Known Limitations

적대적 리뷰에서 나왔고 고치지 않은 것들이다.

- 기억해 두는 절 정보의 수정 시각을 노트를 읽은 뒤에 기록한다. 썸네일을 만드는 몇 초 사이에 파일이 저장되면, 옛 내용의 절 정보가 새 시각과 함께 기억된다. 다른 항목을 올렸다가 다시 올리면 새로 읽는다. 이미지 캐시(`UpdateImageBuildCache`)에도 같은 약점이 원래 있다.
- 노트를 읽는 도중 PowerPoint가 바빠서 일부 슬라이드를 건너뛰면, 그 불완전한 결과가 기억된다. 같은 항목을 연달아 다시 올리는 동안만 유지되고, 썸네일을 새로 만드는 경로를 타면 다시 읽는다.
- 쇼 도중 `Run`이 열어 둔 프레젠테이션을 찾지 못하면 `LoadVersesAndSlides`가 그때 활성인 프레젠테이션에서 절 정보를 읽는다 (원래 있던 동작). 이 경우에는 이전 항목의 버튼이 나올 수 있다.
- `BuildScreenOutDumps`, `BuildScreenDumps`의 기존 호출은 여전히 숫자 9, 1000을 직접 쓴다. 값은 상수와 같다.

## Impact

- `OfficeLib/PowerPoint.cs` (`BuildScreenOutDumps`, 새 도우미)
- `Easislides/Easislides/FrmMain.Logic.cs` (`BuildAllPowerpointScreenDumps`의 두 분기)
