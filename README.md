# Axicultus

교회 예배용 슬라이드·자막 송출 애플리케이션입니다. `Axicultus.sln`에서 WinForms 실행 파일 `Axicultuss.exe`와 WPF 실행 파일 `Axicultus.exe`를 빌드합니다. 관리 어셈블리는 각각 `Axicultuss.dll`과 `Axicultus.dll`입니다. 같은 폴더에 두 앱을 게시하면 `Axicultus.exe --legacy-ui`로 WinForms를 열 수 있습니다.

기존 EasiSlides 예배 순서와 찬양 데이터베이스는 그대로 열 수 있습니다. 새 설치의 기본 작업 폴더는 `C:\Axicultus`이며, 기존 작업 폴더와 찬양 DB는 기존 경로에서 계속 엽니다. WinForms는 첫 실행 시 이전 설정을 `HKCU\Software\Axicultuss`로, WPF는 `%APPDATA%\AxicultusNext` 또는 `%APPDATA%\EasiSlidesNext`의 프로필을 `%APPDATA%\Axicultus`로 복사합니다. 새 위치에 같은 값이나 파일이 있으면 그 내용을 유지합니다.
