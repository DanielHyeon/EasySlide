# Proposal: 쇼를 시작할 때 PowerPoint 발표자 도구가 뜨지 않게 한다

## Why

모니터가 둘인 환경에서 PPT 항목이 있는 쇼를 시작하면, 출력 모니터의 슬라이드 쇼 창과 함께 PowerPoint 발표자 도구가 조작용 모니터 전체 크기로 뜬다. 보통은 EasiSlides 창 뒤에 깔리지만, EasiSlides를 최소화하거나 창을 바꾸면 앞으로 나온다. 운영자가 요구한 것은 "발표자 도구가 나오면 안 된다"이다.

원인: 슬라이드 쇼를 시작하는 유일한 코드(`OfficeLib/PowerPoint.cs`의 `Run(string, …)`)가 `SlideShowSettings.Run()`을 부르기 전에 `ShowPresenterView`를 끄지 않는다. 이 값은 파일이 아니라 PowerPoint 사용자 설정("발표자 도구 사용")을 따른다. 코드에 끄는 줄이 하나 있지만(`Keys.Left` 처리) 쇼가 시작된 뒤에 실행돼서 효과가 없다.

## What Changes

- 쇼를 시작하기 직전에 매번 `ShowPresenterView`를 끈다. PowerPoint 2013 이전처럼 이 속성이 없는 경우에는 무시하고 쇼를 시작한다.

## 알아야 할 부작용

PowerPoint는 이 값을 사용자 기본값으로 저장한다 (`HKCU\Software\Microsoft\Office\16.0\PowerPoint\Options\UseMonMgr = 0`). 그래서 EasiSlides로 쇼를 한 번 시작하면, 그 PC에서 PowerPoint를 따로 쓸 때도 "발표자 도구 사용"이 꺼진 상태가 된다. PowerPoint의 슬라이드 쇼 탭에서 다시 켤 수 있지만, EasiSlides로 쇼를 시작하면 또 꺼진다.

## Out of Scope

- 쇼의 종류(`ShowType`)를 바꾸는 것. 키오스크 방식도 발표자 도구를 없애지만 넘기기와 Esc 동작이 달라진다.

## Impact

- `OfficeLib/PowerPoint.cs` (`Run`)
