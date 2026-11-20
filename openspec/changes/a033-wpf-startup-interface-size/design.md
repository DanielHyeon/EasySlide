## 시작 순서

`App.OnStartup`에서 프로필과 기존 설정을 읽은 후 `IThemeService.ApplyInterfaceSize`에 현재 인터페이스 크기를 전달한다. 메인 창을 만들기 전에 실행하므로 첫 렌더링부터 같은 크기를 사용한다.

크기의 기본값은 설정 모델의 `Standard`를 사용한다. 예전 프로필의 `OnboardingCompleted`는 계속 읽을 수 있지만 시작 흐름을 제어하지 않는다. 데모에서 수동으로 여는 샘플 창은 별도 진입점이다.
