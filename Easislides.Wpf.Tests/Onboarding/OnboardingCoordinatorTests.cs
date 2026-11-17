using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Easislides.Wpf.Onboarding;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Onboarding;

public class OnboardingCoordinatorTests
{
    [Fact]
    public void RunIfNeeded_WhenOnboardingIsPending_PersistsSelectedInterfaceSizeAndCompletion()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        var dialog = new RecordingOnboardingDialogService { NextSelection = InterfaceSize.Large };
        var theme = new RecordingThemeService();
        var sut = new OnboardingCoordinator(settings, dialog, theme);

        var result = sut.RunIfNeeded();

        result.WasShown.Should().BeTrue();
        result.Completed.Should().BeTrue();
        result.SelectedSize.Should().Be(InterfaceSize.Large);
        dialog.ShowCount.Should().Be(1);
        settings.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Large);
        settings.Get(EasiSettingKeys.OnboardingCompleted).Should().BeTrue();
        theme.AppliedSizes.Should().ContainSingle().Which.Should().Be(InterfaceSize.Large);
    }

    [Fact]
    public void RunIfNeeded_WhenOnboardingAlreadyCompleted_DoesNotShowDialog()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        settings.Set(EasiSettingKeys.OnboardingCompleted, true).Succeeded.Should().BeTrue();
        var dialog = new RecordingOnboardingDialogService { NextSelection = InterfaceSize.Senior };
        var theme = new RecordingThemeService();
        var sut = new OnboardingCoordinator(settings, dialog, theme);

        var result = sut.RunIfNeeded();

        result.WasShown.Should().BeFalse();
        result.Completed.Should().BeTrue();
        dialog.ShowCount.Should().Be(0);
        settings.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Standard);
        theme.AppliedSizes.Should().BeEmpty();
    }

    [Fact]
    public void RunIfNeeded_WhenDialogIsDismissed_DoesNotPersistCompletion()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        var dialog = new RecordingOnboardingDialogService { NextSelection = null };
        var theme = new RecordingThemeService();
        var sut = new OnboardingCoordinator(settings, dialog, theme);

        var result = sut.RunIfNeeded();

        result.WasShown.Should().BeTrue();
        result.Completed.Should().BeFalse();
        result.SelectedSize.Should().BeNull();
        settings.Get(EasiSettingKeys.OnboardingCompleted).Should().BeFalse();
        settings.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Standard);
        theme.AppliedSizes.Should().BeEmpty();
    }

    private sealed class RecordingOnboardingDialogService : IOnboardingDialogService
    {
        public InterfaceSize? NextSelection { get; set; }

        public int ShowCount { get; private set; }

        public InterfaceSize? ShowInterfaceSizeOnboarding(System.Windows.Window? owner)
        {
            ShowCount++;
            return NextSelection;
        }
    }

    private sealed class RecordingThemeService : IThemeService
    {
        public ColorTheme CurrentTheme { get; private set; } = ColorTheme.Light;

        public InterfaceSize CurrentSize { get; private set; } = InterfaceSize.Standard;

        public double ScaleFactor => CurrentSize switch
        {
            InterfaceSize.Large => 1.15,
            InterfaceSize.Senior => 1.35,
            _ => 1.0,
        };

        public string LastDiagnostic { get; private set; } = "";

        public List<InterfaceSize> AppliedSizes { get; } = [];

        public event EventHandler? ThemeChanged;

        public event EventHandler? SizeChanged;

        public void ApplyTheme(ColorTheme theme)
        {
            CurrentTheme = theme;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyInterfaceSize(InterfaceSize size)
        {
            CurrentSize = size;
            AppliedSizes.Add(size);
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            SettingsPath = Path.Combine(root, "settings.json");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public string BackupRoot { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_Onboarding_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(SettingsPath, BackupRoot));

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
}
