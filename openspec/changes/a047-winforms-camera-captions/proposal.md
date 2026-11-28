# Axicultuss(WinForms)에 카메라 하단 자막 탭

## Why

카메라 하단 자막은 WPF 앱 Axicultus에만 있다. 지금 예배에 주로 쓰는 앱은 WinForms 앱 Axicultuss다(사용자 확인, 2026-11-28). 방송 운영자가 Axicultuss로 송출하면서 OBS 자막을 함께 운영할 수 있어야 한다.

a046이 자막 코드를 공용 라이브러리(`Axicultus.Captions`)로 분리했다. 이 변경은 Axicultuss를 그 라이브러리의 두 번째 호스트로 만든다.

사용자 흐름: 예배 중 OBS 자막 운영(Axicultuss로 송출하면서).

## What Changes

- Axicultuss 본창의 송출 칸(오른쪽 열) 맨 위에 `[라이브] [● 자막]` 탭 줄을 둔다. 앱이 뜰 때부터 보이고 라이브 탭이 골라져 있다. 자막 탭은 Axicultus의 자막 탭과 같은 화면이다(공용 라이브러리).
- 자막은 회중 화면에 실제로 나가는 것을 따라간다: 쇼가 돌고 있으면 쇼 창의 항목과 페이지, 아니면 본창 송출 칸의 항목과 페이지. 찬양·성경·글은 지금 페이지의 글, PPT는 파일을 읽어 장별 글과 악보 줄.
- 출력 메뉴에 "카메라 하단 자막"(F2: 라이브·자막 탭 전환)과 "카메라 자막 켜기/끄기"(F6)를 넣는다. 두 키는 지금 Axicultuss 본창에서 비어 있다.
- 자막 탭이 보이고 쇼가 돌고 있지 않으면, 예배 순서에서 고른 항목이 송출 칸에 올라가 그 자막이 바로 보인다(Axicultus와 같은 규칙). 쇼가 도는 중에는 고르기만으로 바뀌지 않는다.
- 자막은 OBS 브라우저 소스로 나간다(자막 서버, OBS 연결·씬 전환·카메라 미리보기는 Axicultus와 같다).
- 자막은 회중 화면에도 나간다(내보낼 곳 "카메라", 사용자 요청 2026-11-28). 쇼가 도는 동안 회중 화면 아래쪽에 Axicultus의 카메라 자막과 같은 모양(폭·줄 수·글자 크기 비율·배경 상자·악보 줄)으로 보인다. 쇼 창 위에 겹쳐 놓는 투명한 자막 창으로 그리며, 쇼 창의 그리기 코드는 바꾸지 않는다.
- 자막의 선택(모양, 내보낼 곳, 미리보기 높이)과 OBS 설정은 Axicultuss 프로필 폴더에 저장한다.
- Axicultuss를 격리한 프로필과 데이터 사본으로 실행해 점검하는 하네스(`tools\AxicultussHarness`)를 만든다. 지금은 WinForms 앱용 점검 수단이 없다.

## Non-goals

- 라이브 카메라 항목(카메라 영상을 회중 화면에 띄우는 것). Axicultuss에는 그 항목이 없고 이 변경도 넣지 않는다. 회중 화면의 자막은 무엇이 나가고 있든 그 위에 겹쳐진다.
- 옛 PowerPoint 형식(.ppt·.pps)의 장별 글 읽기. 그런 파일은 자막 탭에 읽을 수 없다는 안내가 보인다(.pptx·.ppsx 등은 된다).
- 큰 장 렌더로 만드는 악보 줄. Axicultuss에서는 PPT 안의 악보 그림과 송출 칸의 장 그림으로 만든다.
- 자막 복구의 예배 순서 되돌리기(Axicultuss는 예배 순서를 스스로 저장한다). 초안·PPT·고친 문구의 복구는 된다.
- 단일 모니터 쇼(쇼 창이 본창을 가리는 모드)에서의 자막 조작. 그 모드에서도 켜 둔 자막은 화면을 따라가지만 조작은 쇼를 끝낸 뒤에 한다.
- ATEM Mini Pro 카메라 전환(별도 변경).
- Axicultuss의 기존 키, 클릭, 더블클릭, 끌어 놓기 동작 변경.

## Impact

- 영향: `Axicultuss\Axicultuss.csproj`(`UseWPF`, `Microsoft.AspNetCore.App`, `Axicultus.Captions` 참조, OCR 도우미 복사), 새 `Axicultuss\Forms\FrmMain.Captions.cs`(탭 줄·자막 칸 놓기, 메뉴·키)와 호스트 연결부, `FrmMain.MenuText.cs`(메뉴 글), `FrmMain.Events.cs`(닫을 때 자막 끝내기), `Axicultus.Core\Localization\MenuText*.resx`(켜기/끄기 메뉴 글), 새 `tools\AxicultussHarness\`, `Axicultuss.Tests`.
- 라이브 송출: 쇼 창과 송출 로직의 코드는 바꾸지 않는다. 자막은 상태를 읽기만 한다(design 2.2). 송출 칸 넘기기와 "송출 칸에 올리기"는 본창의 기존 메서드를 부른다. **회중 화면에는 자막 창이 하나 더 겹쳐진다**(자막이 "카메라"로 나가는 동안만 보임). 자막을 끄거나 "카메라"를 끄면 회중 화면은 지금과 똑같다.
- 미리보기·Office 연동·DB·모니터 배치: 바뀌지 않는다. PPT 글은 파일에서 직접 읽는다(PowerPoint를 거치지 않음).
- 화면: 송출 칸 위에 탭 줄 높이(약 36px)만큼 자리가 준다. 본창 최소 크기 계산(`SizeMainColumns`)에 탭 줄이 들어간다.
- 배포: Axicultuss 배포 폴더에 `Axicultus.Captions.dll`, WPF·CommunityToolkit 관련 파일, `ocr\` 폴더가 늘어난다. **자막 서버 때문에 실행 PC에 ASP.NET Core 런타임(.NET 11)이 있어야 한다.** 없으면 앱이 시작되지 않으므로 방송 PC에 먼저 설치해야 한다.
- 저장: `%APPDATA%\Axicultuss\`(또는 `AXICULTUSS_SETTINGS_FILE`이 가리키는 파일의 폴더)에 `caption-style.json`, `obs.json`, 자막 복구 파일이 생긴다.

## Capabilities

### New Capabilities

- `winforms-camera-captions`: Axicultuss 본창의 자막 탭, 화면 따라가기, 키, 저장 위치, 하네스.
