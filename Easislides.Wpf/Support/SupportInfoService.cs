using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Easislides.Wpf.Settings;
using Microsoft.Win32;

namespace Easislides.Wpf.Support;

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

public sealed class SupportInfoService : ISupportInfoService
{
    public const string WebsiteUrl = "http://www.easislides.com";
    public const string RegistrationUrl = "http://www.easislides.com/register";

    private const string EulaText =
        "EasiSlides Software\r\n\r\n" +
        "IMPORTANT: This software end user licence agreement ('EULA') is a legal agreement between you and EasiSlides. " +
        "Read it carefully before completing the installation process and using the software. It provides a licence to use the software. " +
        "By installing and using the software, you are confirming your acceptance of the software and agreeing to become bound by the terms of this agreement. " +
        "If you do not agree with the terms of this licence you must remove EasiSlides Software files from your storage devices and cease to use the product.\r\n\r\n" +
        "All copyrights to 'EasiSlides Software', hereafter shall be referred to as 'the software', are exclusively owned by EasiSlides. " +
        "Your licence confers no title or ownership in the software and should not be construed as a sale of any right in the software.\r\n\r\n" +
        "You MUST NOT use this software for purposes which are unlawful, including, but not limited to, the transmission of obscene or offensive content, " +
        "or contents which may harass or cause distress to any person.\r\n\r\n" +
        "You may use this software for any length of time.\r\n\r\n" +
        "You are hereby licenced to make any number of backup copies of this software and documentation. " +
        "You can give the copy of the software to anyone or distribute the software provided you abide by the following Licence restrictions:\r\n" +
        "(a) You may not reproduce or distribute the software for the purpose of promoting other non-EasiSlides products or organisations unless specific permission to do so have been obtained from the EasiSlides Copyright holder.\r\n" +
        "(b) You may not alter, merge, modify, adapt or translate the software, or decompile, reverse engineer, disassemble, or otherwise reduce the Software to a human-perceivable form.\r\n" +
        "(c) You may not rent, lease, or sublicence the Software.\r\n" +
        "(d) Where the software is placed on a network for distribution, you must place alongside the distributed software a fully functional and visible hyperlink to the official EasiSlides website at http://www.EasiSlides.com.\r\n" +
        "(e) No fee is charged for the software.\r\n\r\n" +
        "EASISLIDES SOFTWARE IS DISTRIBUTED 'AS IS'. NO WARRANTY OF ANY KIND IS EXPRESSED OR IMPLIED. " +
        "YOU USE IT AT YOUR OWN RISK. EASISLIDES WILL NOT BE LIABLE FOR DATA LOSS, DAMAGES, LOSS OF PROFITS OR ANY OTHER KIND OF LOSS WHILE USING OR MISUSING THIS SOFTWARE.\r\n\r\n" +
        "This EULA shall be governed by and construed in accordance with the laws of Northern Ireland. " +
        "Any dispute arising under this EULA shall be subject to the exclusive jurisdiction of the courts of Northern Ireland.\r\n\r\n" +
        "Copyright (C) 2007 EasiSlides, All rights reserved.\r\n" +
        "Internet: http://www.EasiSlides.com";

    private const string RegistrationBody =
        "EasiSlides is provided free of charge and for your indefinite use provided you abide by the End User Licence Agreement (EULA).\r\n\r\n" +
        "If you intend to use this software on an on-going basis, you are invited to register your use of the software. " +
        "Registration is voluntary and is free of charge. The registration information you provide helps monitor the spread of EasiSlides around the world.";

    private readonly ISettingsService _settings;

    public SupportInfoService(ISettingsService settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public AboutInfo GetAboutInfo()
        => new(
            "EasiSlides",
            $"Software Version: {GetProductVersion()}",
            "Copyright (C) 2019 daniel park revision",
            WebsiteUrl,
            GetSystemInfoPath(),
            _settings.Get(EasiSettingKeys.RegistrationUser),
            EulaText);

    public RegistrationInfo GetRegistrationInfo()
        => new(
            "Register Use of EasiSlides",
            RegistrationUrl,
            RegistrationBody);

    public KeyboardHelpInfo GetKeyboardHelp(LegacyKeyboardOption option)
        => option == LegacyKeyboardOption.ArrowNavigation
            ? new KeyboardHelpInfo(
                [
                    new("First item", "Left Arrow"),
                    new("Last item", "Right Arrow"),
                    new("Previous item", "Up Arrow"),
                    new("Next item", "Down Arrow"),
                ],
                [
                    new("First slide", "Home"),
                    new("Last slide", "End"),
                    new("Previous slide", "Page Up"),
                    new("Next slide", "Page Down, Space"),
                ])
            : new KeyboardHelpInfo(
                [
                    new("First item", "Home"),
                    new("Last item", "End"),
                    new("Previous item", "Page Up"),
                    new("Next item", "Page Down"),
                ],
                [
                    new("First slide", "Left Arrow"),
                    new("Last slide", "Right Arrow"),
                    new("Previous slide", "Up Arrow"),
                    new("Next slide", "Down Arrow, Space"),
                ]);

    public SettingsResult SaveRegistrationUser(string registrationUser)
    {
        ArgumentNullException.ThrowIfNull(registrationUser);
        return _settings.Set(EasiSettingKeys.RegistrationUser, registrationUser.Trim());
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
