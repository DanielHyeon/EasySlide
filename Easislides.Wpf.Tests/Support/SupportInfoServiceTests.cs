using System;
using System.Collections.Generic;
using System.IO;
using Easislides.Wpf.Input;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Support;

public class SupportInfoServiceTests
{
    [Fact]
    public void GetAboutInfo_IncludesVersionWebsiteEulaAndRegistrationUser()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.RegistrationUser, "Grace Church").Succeeded.Should().BeTrue();
        var sut = new SupportInfoService(settings);

        var about = sut.GetAboutInfo();

        about.ProductName.Should().Be("EasiSlides");
        about.VersionLabel.Should().StartWith("Software Version:");
        about.WebsiteUrl.Should().Be("http://www.easislides.com");
        about.RegistrationUser.Should().Be("Grace Church");
        about.EulaText.Should().Contain("end user licence agreement");
        about.EulaText.Should().Contain("EASISLIDES SOFTWARE IS DISTRIBUTED 'AS IS'");
    }

    [Fact]
    public void SaveRegistrationUser_TrimsAndPersistsToSettings()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = new SupportInfoService(settings);

        var result = sut.SaveRegistrationUser("  Grace Church  ");

        result.Succeeded.Should().BeTrue();
        settings.Get(EasiSettingKeys.RegistrationUser).Should().Be("Grace Church");
    }

    [Fact]
    public void AboutWindowViewModel_SaveRegistrationUser_TrimsAndPersistsToSettings()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = new AboutWindowViewModel(new SupportInfoService(settings), new FakeSupportLauncher())
        {
            RegistrationUser = "  Grace Church  ",
        };

        sut.SaveRegistrationUserCommand.Execute(null);

        settings.Get(EasiSettingKeys.RegistrationUser).Should().Be("Grace Church");
        sut.StatusMessage.Should().Be("등록 정보가 저장되었습니다.");
    }

    [Fact]
    public void AboutWindowViewModel_OpenWebsiteCommand_UsesLauncher()
    {
        using var folder = TempSettingsFolder.Create();
        var launcher = new FakeSupportLauncher();
        var sut = new AboutWindowViewModel(new SupportInfoService(folder.CreateSettings()), launcher);

        sut.OpenWebsiteCommand.Execute(null);

        launcher.OpenedTargets.Should().Contain("http://www.easislides.com");
    }

    [Fact]
    public void GetRegistrationInfo_UsesVoluntaryFreeRegistrationCopyAndUrl()
    {
        using var folder = TempSettingsFolder.Create();
        var sut = new SupportInfoService(folder.CreateSettings());

        var registration = sut.GetRegistrationInfo();

        registration.Title.Should().Be("Register Use of EasiSlides");
        registration.RegisterUrl.Should().Be("http://www.easislides.com/register");
        registration.Body.Should().Contain("provided free of charge");
        registration.Body.Should().Contain("Registration is voluntary");
    }

    [Fact]
    public void RegistrationWindowViewModel_OpenRegistrationCommand_UsesLauncher()
    {
        using var folder = TempSettingsFolder.Create();
        var launcher = new FakeSupportLauncher();
        var sut = new RegistrationWindowViewModel(new SupportInfoService(folder.CreateSettings()), launcher);

        sut.OpenRegistrationCommand.Execute(null);

        launcher.OpenedTargets.Should().Contain("http://www.easislides.com/register");
    }

    [Fact]
    public void GetKeyboardHelp_DefaultModeMatchesLegacyHelpLayout()
    {
        using var folder = TempSettingsFolder.Create();
        var sut = new SupportInfoService(folder.CreateSettings());

        var help = sut.GetKeyboardHelp(LegacyKeyboardOption.Default);

        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("First item", "Home"));
        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("Last item", "End"));
        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("Previous item", "Page Up"));
        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("Next item", "Page Down"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("First slide", "Left Arrow"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Last slide", "Right Arrow"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Previous slide", "Up Arrow"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Next slide", "Down Arrow, Space"));
    }

    [Fact]
    public void GetKeyboardHelp_ArrowNavigationModeMatchesLegacyKeyboardOptionOne()
    {
        using var folder = TempSettingsFolder.Create();
        var sut = new SupportInfoService(folder.CreateSettings());

        var help = sut.GetKeyboardHelp(LegacyKeyboardOption.ArrowNavigation);

        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("First item", "Left Arrow"));
        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("Last item", "Right Arrow"));
        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("Previous item", "Up Arrow"));
        help.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("Next item", "Down Arrow"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("First slide", "Home"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Last slide", "End"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Previous slide", "Page Up"));
        help.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Next slide", "Page Down, Space"));
    }

    [Fact]
    public void HelpWindowViewModel_UsesLegacyArrowNavigationWhenShortcutOverridesWereMigrated()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.SetShortcutOverride(
            ShortcutSettings.GetSlotId(MainCommandIds.LivePrevious, isGlobal: false),
            "PageUp").Succeeded.Should().BeTrue();
        settings.SetShortcutOverride(
            ShortcutSettings.GetSlotId(MainCommandIds.LiveNext, isGlobal: false),
            "PageDown").Succeeded.Should().BeTrue();

        var sut = new HelpWindowViewModel(new SupportInfoService(settings), settings);

        sut.ItemShortcuts.Should().Contain(new KeyboardHelpEntry("First item", "Left Arrow"));
        sut.SlideShortcuts.Should().Contain(new KeyboardHelpEntry("Next slide", "Page Down, Space"));
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_SupportSettings_{Guid.NewGuid():N}"));

        public ISettingsService CreateSettings()
            => new SettingsService(new SettingsServiceOptions(
                Path.Combine(Root, "settings.json"),
                Path.Combine(Root, "Backups")));

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed class FakeSupportLauncher : ISupportLauncher
    {
        public List<string> OpenedTargets { get; } = [];

        public bool TryOpen(string target)
        {
            OpenedTargets.Add(target);
            return true;
        }
    }
}
