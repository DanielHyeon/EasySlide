# 쇼를 끝내도 송출 칸 PPT 썸네일 유지

## Why

사용자 보고(WPF): F11(송출 후 다음 항목)로 PPT를 송출한 뒤 Esc로 쇼를 끝내면 송출(라이브) 칸의 슬라이드 썸네일이 사라진다.

원인: `MainViewModel.EndShowAsync`가 쇼를 끝내면서 `OutputPowerPoint.Clear()`를 부른다. 송출 칸 항목(`OutputItem`)은 그대로 남는데 PPT 상태(썸네일, 미리보기 그림, 덱 경로, 장 번호)만 지워져 송출 칸이 항목과 어긋난다.

WinForms(`Axicultuss`) 비교: Esc는 쇼 창에서 `StopShowAndTellMainForm` → 본창 `Remote_ShowStopped`/`Remote_EndShow` → `GoLive(false)` → `LiveShow.StopShow()`로 끝나고, 송출 칸 PPT 썸네일 판(`flowLayoutOutputPowerPoint`)을 비우는 코드는 없다. 쇼를 끝내도 송출 칸은 같은 항목·썸네일을 보여 준다(`fix-show-stop-output-item`도 "쇼를 끝내도 송출 단에는 그 항목이 보인다"를 전제로 한다).

## What Changes

- WPF에서 쇼를 끝낼 때(Esc, 라이브 중지) 송출 칸의 PPT 상태를 지우지 않는다. 송출 칸은 같은 항목과 썸네일을 계속 보여 준다.
- 출력 창 닫기(항목도 비움), 공지 송출, PPT가 아닌 항목 준비처럼 송출 칸 항목 자체가 바뀌는 경우의 지우기는 그대로다. 다른 예배 순서를 불러와 송출 칸 항목이 새 목록에 없으면 송출 칸도 비우고, 같은 항목이 있으면 새 목록의 그 항목을 가리키게 한다(장은 유지). 예배 순서를 비우는 것만으로는 송출 칸을 비우지 않는다(FrmMain처럼, 되돌리기 후에도 송출 칸이 남는다).
- 쇼가 꺼진 동안 송출 칸에서 라이브를 다시 시작하면 송출 칸에서 고른 장으로 나간다(기존 버그: 미리보기 칸의 장이나 첫 장으로 바뀌었다). PPT 파일이 그사이 바뀌었으면 다시 읽는다. 쇼 도중 항목 이동(간격 화면에서 이전 항목, 다시 시작 등)은 지금처럼 그 항목의 첫 장부터다.
- 자막 운영 창은 쇼 도중의 송출 칸 장을 기억해, 쇼를 끝낸 것을 송출 칸 넘김으로 보지 않는다(운영자가 옮긴 자막이 Esc로 튀지 않는다).

## Non-goals

- F11의 송출·다음 항목 이동 동작 변경.
- WinForms 변경.

## Impact

- `Axicultus/Shell/MainViewModel.cs` `EndShowAsync`.
- 쇼가 꺼진 뒤 송출 칸을 읽는 곳(자막 운영 창의 "송출 화면 따라가기", 송출 칸 썸네일 키보드 이동, 다시 송출)의 동작 확인.
- 테스트, 하네스.

## Capabilities

### New Capabilities

- `wpf-live-output`: 쇼 종료 후 송출 칸.
