using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;
using Microsoft.Win32;

namespace Axicultus.Wpf.Support;

public enum LegacyKeyboardOption
{
    Default = 0,
    ArrowNavigation = 1,
}

public sealed record KeyboardHelpEntry(string Action, string Gesture);

public sealed record KeyboardHelpInfo(
    IReadOnlyList<KeyboardHelpEntry> ItemShortcuts,
    IReadOnlyList<KeyboardHelpEntry> SlideShortcuts);

public sealed record AboutInfo(
    string ProductName,
    string VersionLabel,
    string Copyright,
    string WebsiteUrl,
    string? SystemInfoPath,
    string RegistrationUser,
    string EulaText);

public sealed record RegistrationInfo(
    string Title,
    string RegisterUrl,
    string Body);

public interface ISupportInfoService
{
    AboutInfo GetAboutInfo();

    RegistrationInfo GetRegistrationInfo();

    KeyboardHelpInfo GetKeyboardHelp(LegacyKeyboardOption option);

    SettingsResult SaveRegistrationUser(string registrationUser);
}

public sealed class SupportInfoService(ISettingsService settings) : ISupportInfoService
{
    public const string WebsiteUrl = "http://www.easislides.com";
    public const string RegistrationUrl = "http://www.easislides.com/register";

    // The EULA in the UI language (the English original, or its Korean translation), shared with the WinForms app.
    private static string EulaText => Axicultus.Localization.LegalText.Eula;

    private readonly ISettingsService _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public AboutInfo GetAboutInfo()
        => new(
            "Axicultus",
            UiText.Format(nameof(SupportInfoService), "VersionLabel", GetProductVersion()),
            "Copyright (C) 2019 daniel park revision",
            WebsiteUrl,
            GetSystemInfoPath(),
            _settings.Get(AxicultusSettingKeys.RegistrationUser),
            EulaText);

    public RegistrationInfo GetRegistrationInfo()
        => new(
            UiText.Get(nameof(SupportInfoService), "RegistrationTitle"),
            RegistrationUrl,
            UiText.Get(nameof(SupportInfoService), "RegistrationBody"));

    public KeyboardHelpInfo GetKeyboardHelp(LegacyKeyboardOption option)
        => option == LegacyKeyboardOption.ArrowNavigation
            ? new KeyboardHelpInfo(
                [
                    new(UiText.Get(nameof(SupportInfoService), "FirstItem"), UiText.Get(nameof(SupportInfoService), "KeyLeft")),
                    new(UiText.Get(nameof(SupportInfoService), "LastItem"), UiText.Get(nameof(SupportInfoService), "KeyRight")),
                    new(UiText.Get(nameof(SupportInfoService), "PreviousItem"), UiText.Get(nameof(SupportInfoService), "KeyUp")),
                    new(UiText.Get(nameof(SupportInfoService), "NextItem"), UiText.Get(nameof(SupportInfoService), "KeyDown")),
                ],
                [
                    new(UiText.Get(nameof(SupportInfoService), "FirstSlide"), UiText.Get(nameof(SupportInfoService), "KeyHome")),
                    new(UiText.Get(nameof(SupportInfoService), "LastSlide"), UiText.Get(nameof(SupportInfoService), "KeyEnd")),
                    new(UiText.Get(nameof(SupportInfoService), "PreviousSlide"), UiText.Get(nameof(SupportInfoService), "KeyPageUp")),
                    new(UiText.Get(nameof(SupportInfoService), "NextSlide"), UiText.Get(nameof(SupportInfoService), "KeyPageDownSpace")),
                ])
            : new KeyboardHelpInfo(
                [
                    new(UiText.Get(nameof(SupportInfoService), "FirstItem"), UiText.Get(nameof(SupportInfoService), "KeyHome")),
                    new(UiText.Get(nameof(SupportInfoService), "LastItem"), UiText.Get(nameof(SupportInfoService), "KeyEnd")),
                    new(UiText.Get(nameof(SupportInfoService), "PreviousItem"), UiText.Get(nameof(SupportInfoService), "KeyPageUp")),
                    new(UiText.Get(nameof(SupportInfoService), "NextItem"), UiText.Get(nameof(SupportInfoService), "KeyPageDown")),
                ],
                [
                    new(UiText.Get(nameof(SupportInfoService), "FirstSlide"), UiText.Get(nameof(SupportInfoService), "KeyLeft")),
                    new(UiText.Get(nameof(SupportInfoService), "LastSlide"), UiText.Get(nameof(SupportInfoService), "KeyRight")),
                    new(UiText.Get(nameof(SupportInfoService), "PreviousSlide"), UiText.Get(nameof(SupportInfoService), "KeyUp")),
                    new(UiText.Get(nameof(SupportInfoService), "NextSlide"), UiText.Get(nameof(SupportInfoService), "KeyDownSpace")),
                ]);

    public SettingsResult SaveRegistrationUser(string registrationUser)
    {
        ArgumentNullException.ThrowIfNull(registrationUser);
        return _settings.Set(AxicultusSettingKeys.RegistrationUser, registrationUser.Trim());
    }

    private static string GetProductVersion()
    {
        var assembly = typeof(SupportInfoService).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            return informational;
        }

        return assembly.GetName().Version?.ToString() ?? "unknown";
    }

    private static string? GetSystemInfoPath()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\MSINFO");
            var path = key?.GetValue("Path", "") as string;
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path)
                ? path
                : null;
        }
        catch
        {
            return null;
        }
    }
}
