using System;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 스테이지(Preview) 모니터 창 호스트 — 출력 창 호스트(OutputWindowHost)의 병렬 미러.
/// <see cref="IPreviewWindowService"/>(창 상태)와 <see cref="ILiveSessionService"/>(라이브 콘텐츠)를 구독해,
/// 선택한 모니터에 스테이지 창을 띄우고 라이브 가사를 그린다.
///
/// 출력과의 핵심 차이 단 하나: 세션을 <see cref="StageSessionNormalizer"/> 로 정규화해(Hidden→Active)
/// 회중 화면이 검정/비움/숨김이어도 스테이지는 가사를 계속 보여 준다. 렌더·VM 은 출력과 똑같이
/// <see cref="OutputWindowViewModel"/> 를 재사용한다(별도 스테이지 VM 불필요 — 정규화만으로 충분).
///
/// 주의: blackout(완전 검정)/clear 는 회중 화면 전용이라 스테이지엔 **의도적으로 전파하지 않는다**(풀-스테이지 MVP 약속).
/// 나중에 "스테이지도 같이 꺼달라"는 요구가 오면 이 정규화를 조건부로 끄는 설정을 더해야 한다(현재는 항상 무시).
/// </summary>
public sealed class PreviewWindowHost : IDisposable
{
    private readonly IPreviewWindowService _preview;
    private readonly ILiveSessionService _session;
    private readonly OutputSurfaceFactory _surfaceFactory;
    private readonly IOutputRenderer _renderer;
    private readonly ISettingsService? _settings;
    private IOutputSurface? _surface;
    private OutputWindowViewModel? _viewModel;
    private LiveSessionSnapshot _lastSession;
    private bool _disposed;

    public PreviewWindowHost(
        IPreviewWindowService preview,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory)
        : this(
            preview,
            session,
            surfaceFactory,
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings: null)
    {
    }

    public PreviewWindowHost(
        IPreviewWindowService preview,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory,
        IOutputRenderer renderer)
        : this(preview, session, surfaceFactory, renderer, settings: null)
    {
    }

    public PreviewWindowHost(
        IPreviewWindowService preview,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory,
        IOutputRenderer renderer,
        ISettingsService? settings)
    {
        _preview = preview ?? throw new ArgumentNullException(nameof(preview));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _surfaceFactory = surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _settings = settings;
        _lastSession = session.Current;

        _preview.PreviewChanged += OnPreviewChanged;
        _session.SessionChanged += OnSessionChanged;

        ApplyPreview(_preview.Current);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _preview.PreviewChanged -= OnPreviewChanged;
        _session.SessionChanged -= OnSessionChanged;
        CloseSurface();
    }

    private void OnPreviewChanged(object? sender, PreviewWindowChangedEventArgs e)
        => ApplyPreview(e.State);

    private void OnSessionChanged(object? sender, LiveSessionChangedEventArgs e)
    {
        _lastSession = e.Snapshot;
        // 스테이지는 회중 화면이 가려져도 가사를 계속 보여 준다 → 세션을 정규화(Hidden→Active)해 VM 에 적용.
        _viewModel?.ApplySession(StageSessionNormalizer.Normalize(e.Snapshot));
    }

    private void ApplyPreview(PreviewWindowState state)
    {
        if (!state.IsOpen)
        {
            _viewModel?.ApplyOutput(ToOutputState(state));
            CloseSurface();
            return;
        }

        EnsureSurface();
        _viewModel!.ApplyOutput(ToOutputState(state));
        _surface!.ApplyPlacement(state.Placement);
        _surface.Show();
    }

    private void EnsureSurface()
    {
        if (_surface is not null)
        {
            return;
        }

        _viewModel = new OutputWindowViewModel(_renderer, _settings);
        // 창을 처음 열 때 직전 세션을 정규화해 적용 — 이미 검정/비움 중이어도 스테이지엔 가사가 떠 있게.
        _viewModel.ApplySession(StageSessionNormalizer.Normalize(_lastSession));

        _surface = _surfaceFactory();
        _surface.Bind(_viewModel);
    }

    private void CloseSurface()
    {
        _surface?.Close();
        _viewModel?.Dispose();
        _surface = null;
        _viewModel = null;
    }

    // 스테이지 창의 지오메트리(IsOpen/Display/Placement)를 출력 VM 이 이해하는 OutputWindowState 로 옮긴다.
    // VM 은 이 값으로 뷰포트 크기·모니터 이름·열림 여부를 잡는다(같은 출력 VM 을 스테이지 창 크기로 쓰기 위함).
    private static OutputWindowState ToOutputState(PreviewWindowState state)
        => new(state.IsOpen, state.Display, state.Placement);
}
