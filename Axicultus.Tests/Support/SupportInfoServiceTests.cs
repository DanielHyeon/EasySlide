using System;
using System.Collections.Generic;
using System.IO;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Support;

public class SupportInfoServiceTests
{
    [Fact]
    public void GetAboutInfo_IncludesVersionWebsiteEulaAndRegistrationUser()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.RegistrationUser, "Grace Church").Succeeded.Should().BeTrue();
        var sut = new SupportInfoService(settings);

        var about = sut.GetAboutInfo();

        about.ProductName.Should().Be("Axicultus");
        about.VersionLabel.Should().StartWith(UiText.Format("SupportInfoService", "VersionLabel", ""));
        about.WebsiteUrl.Should().Be("http://www.easislides.com");
        about.RegistrationUser.Should().Be("Grace Church");
        about.EulaText.Should().Be(Axicultus.Localization.LegalText.Eula, "the shared EULA in the UI language");
        about.EulaText.Should().Contain("최종 사용자 사용권 계약"); // Korean, the tests' language
        about.EulaText.Should().Contain("EASISLIDES 소프트웨어는 '있는 그대로' 배포됩니다");
    }

    [Fact]
    public void SaveRegistrationUser_TrimsAndPersistsToSettings()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = new SupportInfoService(settings);

        var result = sut.SaveRegistrationUser("  Grace Church  ");

        result.Succeeded.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.RegistrationUser).Should().Be("Grace Church");
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

        settings.Get(AxicultusSettingKeys.RegistrationUser).Should().Be("Grace Church");
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

        registration.Title.Should().Be(UiText.Get("SupportInfoService", "RegistrationTitle"));
        registration.RegisterUrl.Should().Be("http://www.easislides.com/register");
        registration.Body.Should().Be(UiText.Get("SupportInfoService", "RegistrationBody"));
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

        help.ItemShortcuts.Should().Contain(Entry("FirstItem", "KeyHome"));
        help.ItemShortcuts.Should().Contain(Entry("LastItem", "KeyEnd"));
        help.ItemShortcuts.Should().Contain(Entry("PreviousItem", "KeyPageUp"));
        help.ItemShortcuts.Should().Contain(Entry("NextItem", "KeyPageDown"));
        help.SlideShortcuts.Should().Contain(Entry("FirstSlide", "KeyLeft"));
        help.SlideShortcuts.Should().Contain(Entry("LastSlide", "KeyRight"));
        help.SlideShortcuts.Should().Contain(Entry("PreviousSlide", "KeyUp"));
        help.SlideShortcuts.Should().Contain(Entry("NextSlide", "KeyDownSpace"));
    }

    [Fact]
    public void GetKeyboardHelp_ArrowNavigationModeMatchesLegacyKeyboardOptionOne()
    {
        using var folder = TempSettingsFolder.Create();
        var sut = new SupportInfoService(folder.CreateSettings());

        var help = sut.GetKeyboardHelp(LegacyKeyboardOption.ArrowNavigation);

        help.ItemShortcuts.Should().Contain(Entry("FirstItem", "KeyLeft"));
        help.ItemShortcuts.Should().Contain(Entry("LastItem", "KeyRight"));
        help.ItemShortcuts.Should().Contain(Entry("PreviousItem", "KeyUp"));
        help.ItemShortcuts.Should().Contain(Entry("NextItem", "KeyDown"));
        help.SlideShortcuts.Should().Contain(Entry("FirstSlide", "KeyHome"));
        help.SlideShortcuts.Should().Contain(Entry("LastSlide", "KeyEnd"));
        help.SlideShortcuts.Should().Contain(Entry("PreviousSlide", "KeyPageUp"));
        help.SlideShortcuts.Should().Contain(Entry("NextSlide", "KeyPageDownSpace"));
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

        sut.ItemShortcuts.Should().Contain(Entry("FirstItem", "KeyLeft"));
        sut.SlideShortcuts.Should().Contain(Entry("NextSlide", "KeyPageDownSpace"));
    }

    private static KeyboardHelpEntry Entry(string action, string gesture)
        => new(UiText.Get("SupportInfoService", action), UiText.Get("SupportInfoService", gesture));

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_SupportSettings_{Guid.NewGuid():N}"));

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
