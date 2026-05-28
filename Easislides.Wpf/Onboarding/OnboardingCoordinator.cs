using System;
using System.Collections.Generic;
using System.Windows;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Theme;

namespace Easislides.Wpf.Onboarding;

public interface IOnboardingCoordinator
{
    OnboardingRunResult RunIfNeeded(Window? owner = null);
}

public interface IOnboardingDialogService
{
    InterfaceSize? ShowInterfaceSizeOnboarding(Window? owner);
}

public sealed record OnboardingRunResult(
    bool WasShown,
    bool Completed,
    InterfaceSize? SelectedSize,
    IReadOnlyList<SettingsIssue> Issues)
{
    public static OnboardingRunResult Skipped()
        => new(WasShown: false, Completed: true, SelectedSize: null, Array.Empty<SettingsIssue>());

    public static OnboardingRunResult Dismissed()
        => new(WasShown: true, Completed: false, SelectedSize: null, Array.Empty<SettingsIssue>());
}

public sealed class OnboardingCoordinator : IOnboardingCoordinator
{
    private readonly ISettingsService _settings;
    private readonly IOnboardingDialogService _dialogService;
    private readonly IThemeService _theme;

    public OnboardingCoordinator(
        ISettingsService settings,
        IOnboardingDialogService dialogService,
        IThemeService theme)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public OnboardingRunResult RunIfNeeded(Window? owner = null)
    {
        if (_settings.Get(EasiSettingKeys.OnboardingCompleted))
        {
            return OnboardingRunResult.Skipped();
        }

        var selectedSize = _dialogService.ShowInterfaceSizeOnboarding(owner);
        if (selectedSize is null)
        {
            return OnboardingRunResult.Dismissed();
        }

        var sizeResult = _settings.Set(EasiSettingKeys.InterfaceSize, selectedSize.Value);
        if (!sizeResult.Succeeded)
        {
            return new OnboardingRunResult(true, false, selectedSize, sizeResult.Issues);
        }

        _theme.ApplyInterfaceSize(selectedSize.Value);

        var completionResult = _settings.Set(EasiSettingKeys.OnboardingCompleted, true);
        return new OnboardingRunResult(
            WasShown: true,
            Completed: completionResult.Succeeded,
            SelectedSize: selectedSize,
            Issues: completionResult.Issues);
    }
}

public sealed class WelcomeWindowDialogService : IOnboardingDialogService
{
    public InterfaceSize? ShowInterfaceSizeOnboarding(Window? owner)
    {
        var welcome = new WelcomeWindow();
        if (owner is not null)
        {
            welcome.Owner = owner;
        }

        return welcome.ShowDialog() == true
            ? welcome.SelectedSize
            : null;
    }
}
