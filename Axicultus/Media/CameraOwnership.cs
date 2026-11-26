using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Media;

public sealed class CameraOwnership : ObservableObject
{
    private bool _obsOwnsCamera;
    private string _status = UiText.Get("CameraOwnership", "LocalCameraIdle");
    public bool ObsOwnsCamera
    {
        get => _obsOwnsCamera;
        set
        {
            if (!SetProperty(ref _obsOwnsCamera, value)) return;
            if (value)
            {
                YieldCheck();
                ReleaseLocalCamera?.Invoke();
            }
            Status = value ? UiText.Get("CameraOwnership", "ObsOwnsCamera") : UiText.Get("CameraOwnership", "LocalCameraIdle");
        }
    }
    public string Status { get => _status; internal set => SetProperty(ref _status, value); }
    private System.Windows.Media.ImageSource? _preview;
    public System.Windows.Media.ImageSource? Preview { get => _preview; internal set => SetProperty(ref _preview, value); }
    private double _scaleY = 1;
    public double ScaleY { get => _scaleY; internal set => SetProperty(ref _scaleY, value); }
    private object? _localOwner;
    private Action? _yieldCheck;
    internal bool HasLocalCapture => _localOwner != null;
    internal bool IsOwner(object owner) => ReferenceEquals(_localOwner, owner);

    internal bool TryBeginCheck(Action yield)
    {
        if (ObsOwnsCamera || HasLocalCapture || _yieldCheck != null) return false;
        _yieldCheck = yield;
        return true;
    }

    internal void EndCheck(Action yield)
    {
        if (_yieldCheck == yield) _yieldCheck = null;
    }

    private void YieldCheck()
    {
        var stop = _yieldCheck;
        _yieldCheck = null;
        stop?.Invoke();
    }
    internal bool TryAcquire(object owner)
    {
        if (ObsOwnsCamera || _localOwner != null) return false;
        _localOwner = owner;
        YieldCheck();
        OnPropertyChanged(nameof(HasLocalCapture));
        return true;
    }

    internal void Release(object owner)
    {
        if (!ReferenceEquals(owner, _localOwner)) return;
        _localOwner = null;
        OnPropertyChanged(nameof(HasLocalCapture));
        Preview = null;
        Status = ObsOwnsCamera ? UiText.Get("CameraOwnership", "ObsOwnsCamera") : UiText.Get("CameraOwnership", "LocalCameraIdle");
    }

    internal event Action? ReleaseLocalCamera;
}
