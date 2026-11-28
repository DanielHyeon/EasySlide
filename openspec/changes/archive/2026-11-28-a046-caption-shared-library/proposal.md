# 카메라 자막 코드를 공용 라이브러리로 분리

## Why

카메라 하단 자막(자막 탭, 화면 따라가기, OBS 송출)은 WPF 앱 `Axicultus`에만 있다. 지금 예배에 주로 쓰는 앱은 WinForms 앱 `Axicultuss`다(사용자 확인, 2026-11-28). 같은 기능을 Axicultuss에 넣으려면 자막 코드를 두 앱이 함께 쓸 수 있어야 한다.

지금 자막 코드(`Axicultus\Captions\`, 23개 파일, 약 6,200줄)는 Axicultus 프로젝트 안에 있고 다음에 묶여 있다.

- 자막 탭 화면(`CaptionConsole`)과 `QueueRecoveryJournal`이 `MainViewModel`을 직접 쓴다.
- 문구는 Axicultus 어셈블리의 resx와 `{loc:Text}`를 쓴다.
- 라이브 상태(`ILiveSessionService`, `LiveState`), 예배 순서 항목(`LiveQueueItem`), PPT 렌더, 곡 읽기, 오류 기록이 Axicultus 프로젝트의 형식이다.
- 색과 글꼴은 Axicultus 앱의 테마 사전에 있다.

사용자 흐름: 예배 중 카메라·OBS 자막 운영(두 앱 공통). 이 변경은 Axicultuss 이식(a047)의 준비 단계다.

## What Changes

- 새 프로젝트 `Axicultus.Captions`(WPF 클래스 라이브러리)를 만들고 자막 코드를 옮긴다. 네임스페이스 `Axicultus.Wpf.Captions`는 그대로 둔다.
- 자막 코드가 앱에서 받는 것을 인터페이스로 정리한다(design 2). 라이브러리는 Axicultus 앱 어셈블리의 형식을 쓰지 않는다.
- Axicultus 앱에는 그 인터페이스를 `MainViewModel`로 구현하는 연결부만 둔다. `QueueRecoveryJournal`은 앱에 남긴다.
- 자막 문구(resx, 한국어·영어)와 `{loc:Text}`에 해당하는 표시 확장을 라이브러리로 옮긴다.
- 앱 테마가 없는 호스트를 위해 자막 화면이 쓰는 색·글꼴 키의 기본 사전(밝은·어두운)을 라이브러리에 둔다. Axicultus에서는 지금처럼 앱 테마를 쓴다.
- Axicultus의 자막 동작, 화면, 저장 파일, 단축키는 바뀌지 않는다.

## Non-goals

- Axicultuss에 자막을 넣는 일(a047).
- 자막 기능의 동작 변경, 새 기능, 화면 손질.
- 자막과 무관한 Axicultus 코드의 정리. 옮기는 데 꼭 필요한 것만 고친다.
- ATEM Mini Pro 카메라 전환(이식 뒤 별도 변경).

## Impact

- 영향: `Axicultus\Captions\*`(이동), 새 `Axicultus.Captions\`, `Axicultus\Axicultus.csproj`, `Axicultus\App.xaml.cs`(서비스 등록), `Axicultus\MainWindow.xaml.cs`(자막 탭 만들기), `Axicultus\OutputWindow.xaml`(카메라 자막 보기의 xmlns), 자막 형식을 쓰는 앱 파일(`MainWindow.LiveDevices.cs`, `Media\LiveMicrophoneRouter.cs`, `Composites\DeviceCheckPanel.xaml.cs`, `SettingsWindow.xaml.cs`, `Interop\OfficePptSession.cs`, `Rendering\PowerPointWorker.cs`), `Axicultus.Tests`(자막 테스트의 만들기 코드), `tools\AxicultusHarness`, `Axicultus.sln`.
- 라이브 송출, Office 연동, DB, 모니터 배치는 바뀌지 않는다. 미리보기와 송출 화면의 모양도 바뀌지 않는다.
- 저장: 프로필의 `caption-style.json`, `obs.json`, 복구 파일의 위치와 형식은 그대로다.
- 배포: Axicultus 배포 폴더에 `Axicultus.Captions.dll`이 하나 늘어난다.

## Capabilities

### Modified Capabilities

- `live-camera-captions`: 자막 기능이 호스트 앱과 분리된 공용 라이브러리로 제공된다는 요구 추가(동작은 그대로).
