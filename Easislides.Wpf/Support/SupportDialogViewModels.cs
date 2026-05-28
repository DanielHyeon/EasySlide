using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Input;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Support;

public sealed partial class AboutWindowViewModel : ObservableObject
{
    private readonly ISupportInfoService _supportInfo;
    private readonly ISupportLauncher _launcher;

    [ObservableProperty] private string _registrationUser;
    [ObservableProperty] private string _statusMessage = "";

    public AboutWindowViewModel(ISupportInfoService supportInfo, ISupportLauncher launcher)
    {
        _supportInfo = supportInfo ?? throw new ArgumentNullException(nameof(supportInfo));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));

        var about = _supportInfo.GetAboutInfo();
        ProductName = about.ProductName;
        VersionLabel = about.VersionLabel;
        Copyright = about.Copyright;
        WebsiteUrl = about.WebsiteUrl;
        SystemInfoPath = about.SystemInfoPath;
        EulaText = about.EulaText;
        _registrationUser = about.RegistrationUser;

        OpenWebsiteCommand = new RelayCommand(() => Open(WebsiteUrl, "웹사이트를 열 수 없습니다."));
        OpenSystemInfoCommand = new RelayCommand(
            () => Open(SystemInfoPath!, "시스템 정보 도구를 열 수 없습니다."),
            () => !string.IsNullOrWhiteSpace(SystemInfoPath));
        SaveRegistrationUserCommand = new RelayCommand(SaveRegistrationUser);
    }

    public string ProductName { get; }

    public string VersionLabel { get; }

    public string Copyright { get; }

    public string WebsiteUrl { get; }

    public string? SystemInfoPath { get; }

    public string EulaText { get; }

    public IRelayCommand OpenWebsiteCommand { get; }

    public IRelayCommand OpenSystemInfoCommand { get; }

    public IRelayCommand SaveRegistrationUserCommand { get; }

    private void SaveRegistrationUser()
    {
        var result = _supportInfo.SaveRegistrationUser(RegistrationUser);
        StatusMessage = result.Succeeded
            ? "등록 정보가 저장되었습니다."
            : string.Join(" ", result.Issues.Select(issue => issue.Message));
    }

    private void Open(string target, string failureMessage)
    {
        StatusMessage = _launcher.TryOpen(target)
            ? ""
            : failureMessage;
    }
}

public sealed class HelpWindowViewModel
{
    public HelpWindowViewModel(ISupportInfoService supportInfo, ISettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(supportInfo);
        ArgumentNullException.ThrowIfNull(settings);

        var help = supportInfo.GetKeyboardHelp(ResolveLegacyKeyboardOption(settings));
        ItemShortcuts = help.ItemShortcuts;
        SlideShortcuts = help.SlideShortcuts;
    }

    public IReadOnlyList<KeyboardHelpEntry> ItemShortcuts { get; }

    public IReadOnlyList<KeyboardHelpEntry> SlideShortcuts { get; }

    private static LegacyKeyboardOption ResolveLegacyKeyboardOption(ISettingsService settings)
    {
        var shortcuts = settings.Current.Shortcuts;
        var previousSlot = ShortcutSettings.GetSlotId(MainCommandIds.LivePrevious, isGlobal: false);
        var nextSlot = ShortcutSettings.GetSlotId(MainCommandIds.LiveNext, isGlobal: false);

        return shortcuts.TryGetValue(previousSlot, out var previous) &&
               shortcuts.TryGetValue(nextSlot, out var next) &&
               IsSameGesture(previous, "PageUp") &&
               IsSameGesture(next, "PageDown")
            ? LegacyKeyboardOption.ArrowNavigation
            : LegacyKeyboardOption.Default;
    }

    private static bool IsSameGesture(string actual, string expected)
    {
        try
        {
            return string.Equals(
                ShortcutSettings.NormalizeGesture(actual),
                ShortcutSettings.NormalizeGesture(expected),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

public sealed partial class RegistrationWindowViewModel : ObservableObject
{
    private readonly ISupportLauncher _launcher;

    [ObservableProperty] private string _statusMessage = "";

    public RegistrationWindowViewModel(ISupportInfoService supportInfo, ISupportLauncher launcher)
    {
        ArgumentNullException.ThrowIfNull(supportInfo);
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));

        var registration = supportInfo.GetRegistrationInfo();
        Title = registration.Title;
        RegisterUrl = registration.RegisterUrl;
        Body = registration.Body;
        OpenRegistrationCommand = new RelayCommand(OpenRegistration);
    }

    public string Title { get; }

    public string RegisterUrl { get; }

    public string Body { get; }

    public IRelayCommand OpenRegistrationCommand { get; }

    private void OpenRegistration()
    {
        StatusMessage = _launcher.TryOpen(RegisterUrl)
            ? ""
            : "등록 페이지를 열 수 없습니다.";
    }
}
