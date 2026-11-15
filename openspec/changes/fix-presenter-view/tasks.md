# Tasks: 쇼를 시작할 때 PowerPoint 발표자 도구가 뜨지 않게 한다

구현 → 적대적 리뷰 → 내장 코드 리뷰 → 총괄 검증 → 커밋.

- [x] 1 `OfficeLib/PowerPoint.cs` `Run(string, ref string[,], ref int)`: `SlideShowSettings.Run()` 직전에 `ShowPresenterView = msoFalse`를 넣는다. 이 대입이 실패해도 쇼는 시작한다 (try/catch, 기록)
- [x] 2 호출하는 곳이 없는 `Run(int)`에도 같은 처리를 넣는다
- [x] 3 `Keys.Left` 처리에 있는 효과 없는 `ShowPresenterView` 대입을 없앤다
- [x] 4 빌드 확인
- [ ] 5 실행 확인: PowerPoint의 발표자 도구 설정이 켜진 상태에서 쇼를 시작하고, 창 목록에 발표자 도구 창(`PodiumParent`)이 없는지 본다. 쇼 도중 다른 PPT 항목으로 넘어가서도 본다
  - 저장소 빌드로는 하지 못했다. 조사 때 설정이 한 번 꺼진 뒤로는 수정 전 빌드에서도 발표자 도구가 뜨지 않고, 레지스트리 값(`UseMonMgr`)을 지우거나 1로 바꿔도 다시 켜지지 않았다. 설정이 꺼진 상태에서는 쇼가 정상으로 시작되고 발표자 도구 창이 없는 것을 확인했다.
  - 근거는 조사 때의 실행이다. 설정이 켜진 상태에서 수정 전에는 발표자 도구 창이 떴고, 같은 방식의 수정을 넣은 빌드에서는 뜨지 않았다.
  - PowerPoint에서 "발표자 도구 사용"을 직접 켠 뒤 다시 확인하면 이 항목을 닫을 수 있다.

## Known Limitations

- 주석은 "2013 이전 버전에는 속성이 없다"고 적었지만, NetOffice 문서로는 PowerPoint 2010부터 있다. 동작에는 차이가 없다 (없는 경우의 예외를 잡는다).
- `GetOffice/GetOffice/PP.cs`에도 슬라이드 쇼를 시작하는 코드가 있다. 앱이 참조하지 않는 보조 프로젝트라 고치지 않았다.
