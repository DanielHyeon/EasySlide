# Axicultus

교회 예배용 슬라이드·자막 송출 애플리케이션입니다. `Axicultus.sln`에서 WinForms 실행 파일 `Axicultuss.exe`와 WPF 실행 파일 `Axicultus.exe`를 빌드합니다. 관리 어셈블리는 각각 `Axicultuss.dll`과 `Axicultus.dll`입니다. 같은 폴더에 두 앱을 게시하면 `Axicultus.exe --legacy-ui`로 WinForms를 열 수 있습니다.

| 앱 | 프로젝트 | 사용자 설정 |
|---|---|---|
| WinForms | `Axicultuss/Axicultuss.csproj` | `%APPDATA%\Axicultuss\settings.json` |
| WPF | `Axicultus/Axicultus.csproj` | `%APPDATA%\Axicultus\settings.json` |

WinForms는 JSON 파일이 없는 첫 실행에 현재 사용자 레지스트리의 Axicultuss, Axicultus, EasiSlides 설정을 최신 프로필 우선으로 읽습니다. 이후 저장과 앱 메뉴를 통한 초기화는 WinForms JSON에만 적용됩니다. WPF는 자체 JSON과 이전 WPF 프로필을 사용합니다.

새 설치의 기본 작업 폴더는 WinForms `C:\Axicultuss`, WPF `C:\Axicultus`입니다. 기존에 선택한 작업 폴더와 찬양 데이터베이스는 원래 경로에서 계속 열 수 있습니다.
