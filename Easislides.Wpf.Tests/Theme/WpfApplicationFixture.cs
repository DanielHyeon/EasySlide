using System;
using System.Threading;
using System.Windows;

namespace Easislides.Wpf.Tests.Theme;

/// <summary>
/// WPF Application 인스턴스를 한 번만 생성하는 xUnit fixture.
/// ThemeService가 Application.Current.Resources에 의존하므로 모든 테스트가 공유.
///
/// xUnit collection으로 직렬 실행 — Application은 프로세스당 단 하나만 가능.
/// </summary>
public sealed class WpfApplicationFixture : IDisposable
{
    public WpfApplicationFixture()
    {
        if (Application.Current is null)
        {
            // STA 스레드에서 Application 인스턴스 생성 + EasiDS 리소스 로드
            var app = new Application();

            // EasiDS 리소스 사전을 코드로 머지 (App.xaml 부트스트랩을 흉내냄)
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/EasislidesNext;component/Theme/EasiDS.xaml", UriKind.Absolute),
            });
        }
    }

    public void Dispose()
    {
        // Application은 프로세스 lifetime — Dispose 시점에 정리 안 함.
    }
}

[Xunit.CollectionDefinition("WPF Application")]
public class WpfApplicationCollection : Xunit.ICollectionFixture<WpfApplicationFixture>
{
    // xUnit이 같은 collection의 테스트를 직렬 실행하도록 표시.
}
