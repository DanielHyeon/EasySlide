using System;

namespace Easislides.Wpf.Media;

/// <summary>
/// 미디어 백엔드 생명주기 브리지 (실제 미디어 백엔드 트랙 1단계).
///
/// 문제: <see cref="IMediaPlaybackService"/> 는 DI 시점에 생성되는 싱글톤이라 백엔드를 즉시 요구하지만,
/// 실제 재생 백엔드(<see cref="WpfMediaElementPlaybackBackend"/>)는 출력 창의 MediaElement 가 만들어진
/// 뒤에야 구성 가능하다(지연 생성). 이 불일치를 브리지가 해소한다.
///
/// 동작: 실제 백엔드가 <see cref="Attach"/> 되기 전엔 모든 호출을 no-op(크래시 없음)으로 흡수하고,
/// Attach 후엔 그대로 위임한다. Attach 시점에 이미 로드된 미디어가 있으면 새 백엔드에 재적재하고
/// 마지막 재생 의도(재생/일시정지)도 복원해, 출력 창이 미디어 선택·재생 이후에 열려도 현재 미디어가
/// 이어지도록 한다. 단, 정확한 재생 위치는 브리지가 추적하지 못하므로 0(처음)부터 재생된다.
///
/// 스레딩 계약: 실제 백엔드(WpfMediaElementPlaybackBackend → MediaElement)는 WPF UI(디스패처)
/// 스레드 전용이므로 이 브리지의 모든 멤버도 UI 스레드에서 호출돼야 한다(브리지는 마샬링하지 않음).
/// 그래서 _inner/_lastLoad/_intent 는 동기화 없이 단일 스레드 접근을 전제한다.
/// </summary>
public sealed class AttachableMediaPlaybackBackend : IMediaPlaybackBackend
{
    private IMediaPlaybackBackend? _inner;
    private MediaPlaybackSnapshot? _lastLoad;
    private MediaPlaybackState _intent = MediaPlaybackState.Empty;

    /// <summary>실제 백엔드가 부착됐는지.</summary>
    public bool IsAttached => _inner is not null;

    /// <summary>실제 재생 백엔드를 부착한다. 진행 중 미디어가 있으면 재적재 + 재생 의도 복원.</summary>
    public void Attach(IMediaPlaybackBackend backend)
    {
        _inner = backend ?? throw new ArgumentNullException(nameof(backend));
        if (_lastLoad is { } snapshot)
        {
            _inner.Load(snapshot);
            // 부착 시점의 재생 의도 복원(위치는 추적 불가 → 처음부터). Stopped/Ready 는 Load 만으로 충분.
            if (_intent == MediaPlaybackState.Playing)
            {
                _inner.Play();
            }
            else if (_intent == MediaPlaybackState.Paused)
            {
                _inner.Pause();
            }
        }
    }

    /// <summary>실제 백엔드를 분리한다(출력 창 닫힘 등). 이후 호출은 다시 no-op.</summary>
    public void Detach() => _inner = null;

    public void Load(MediaPlaybackSnapshot snapshot)
    {
        _lastLoad = snapshot;
        _intent = MediaPlaybackState.Ready;
        _inner?.Load(snapshot);
    }

    public void Play()
    {
        _intent = MediaPlaybackState.Playing;
        _inner?.Play();
    }

    public void Pause()
    {
        _intent = MediaPlaybackState.Paused;
        _inner?.Pause();
    }

    public void Stop()
    {
        _intent = MediaPlaybackState.Stopped;
        _inner?.Stop();
    }

    public void Seek(TimeSpan position) => _inner?.Seek(position);

    public void ApplySettings(MediaPlaybackSnapshot snapshot) => _inner?.ApplySettings(snapshot);

    public void Unload()
    {
        // 진행 중 미디어 기록을 함께 비운다 — 이후 출력 창이 다시 열려 Attach 되더라도
        // (Stop 과 달리) 내려간 미디어를 재적재하지 않도록. 출력 패리티: 가사 화면으로 복귀.
        _lastLoad = null;
        _intent = MediaPlaybackState.Empty;
        _inner?.Unload();
    }
}
