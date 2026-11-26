using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Settings;

public sealed partial class SettingsWindowViewModel
{
    public object this[string name]
    {
        get => ReadOperatorOption(OperatorOptionCatalog.ById["operator." + name]);
        set
        {
            var option = OperatorOptionCatalog.ById["operator." + name];
            WriteOperatorOption(option, Convert.ChangeType(value, option.Default.GetType(),
                System.Globalization.CultureInfo.CurrentCulture));
        }
    }

    internal object ReadOperatorOption(OperatorOption option) => option.Read(_settings.Current.Operator);

    internal void WriteOperatorOption(OperatorOption option, object value)
    {
        if (Equals(ReadOperatorOption(option), value)) return;
        var values = new Dictionary<string, object> { [option.Id] = value };
        if (value is true && option.Id == "operator.GlobalArrow") values["operator.GlobalCtrlArrow"] = false;
        if (value is true && option.Id == "operator.GlobalCtrlArrow") values["operator.GlobalArrow"] = false;
        var result = _settings.SetValues(values);
        ApplyResult(result, IsEditing ? UiText.Get("SettingsWindow", "SaveOnOk") : UiText.Get("SettingsWindow", "SettingsSaved"));
        if (!result.Succeeded) throw new ArgumentException(result.Issues[0].Message);
        OnPropertyChanged(nameof(OperatorPreferences));
        OnPropertyChanged(System.Windows.Data.Binding.IndexerName);
        OnPropertyChanged(nameof(KeyboardLayoutGuide));
        OnPropertyChanged(nameof(StageHeight));
    }

    internal void ImportLicenceNames(string path, IReadOnlyDictionary<int,string> names)
    {
        if (string.Equals(_settings.Current.General.LicenceNamesImportedFrom, path, StringComparison.OrdinalIgnoreCase)) return;
        foreach (var (slot, name) in names)
            if (slot is >= 4 and <= 8 && name.Length > 0 && string.IsNullOrWhiteSpace(LicenceRows[slot - 1].Name))
                LicenceRows[slot - 1].Name = name;
        _settings.Set(AxicultusSettingKeys.LicenceNamesImportedFrom, path);
    }

    public OperatorOptions OperatorPreferences => _settings.Current.Operator;
    public string KeyboardLayoutGuide => OperatorPreferences.AlternateKeyboard
        ? UiText.Get("SettingsWindow", "KeyboardGuideAlternate")
        : UiText.Get("SettingsWindow", "KeyboardGuideDefault");
    public int StageHeight => Axicultus.Core.SlideAspectRatio.HeightForWidth(OperatorPreferences.StageWidth, IsMonitorWide);
    public int AudienceHeight => Axicultus.Core.SlideAspectRatio.HeightForWidth(DisplayCustomWidth, IsMonitorWide);

    private bool IsEditing => !ReferenceEquals(_settings, _applicationSettings);

    internal void BeginOptionsEdit()
    {
        if (_applicationSettings is not SettingsService service)
            throw new InvalidOperationException("The options dialog requires the application settings store.");
        _settings = service.BeginEdit();
        RefreshFromSettings(applyAppearance: false);
        StatusMessage = "";
    }

    private void ReloadOptions()
    {
        if (IsEditing) BeginOptionsEdit();
        else RefreshFromSettings(applyAppearance: true);
    }

    internal bool ConfirmOptions()
    {
        if (!IsEditing) return true;
        var result = ((SettingsService)_settings).CommitEdit();
        ApplyResult(result, UiText.Get("SettingsWindow", "SettingsSaved"));
        if (!result.Succeeded) return false;
        if (_applicationSettings.Current.Appearance.Theme != _appearanceOnOpen?.Theme)
            _theme.ApplyTheme(_applicationSettings.Current.Appearance.Theme);
        if (_applicationSettings.Current.Appearance.InterfaceSize != _appearanceOnOpen?.InterfaceSize)
            _theme.ApplyInterfaceSize(_applicationSettings.Current.Appearance.InterfaceSize);
        return true;
    }

    private AppearanceSettings? _appearanceOnOpen;
    private void RefreshAdditionalOptions()
    {
        _appearanceOnOpen ??= _applicationSettings.Current.Appearance;
        LicenceRows = [
            new(this, "None"), new(this, "Public Domain"),
            new(this, "CCLI", number: AxicultusSettingKeys.LicenceNumber3),
            new(this, "", AxicultusSettingKeys.LicenceName4, AxicultusSettingKeys.LicenceNumber4),
            new(this, "", AxicultusSettingKeys.LicenceName5, AxicultusSettingKeys.LicenceNumber5),
            new(this, "", AxicultusSettingKeys.LicenceName6, AxicultusSettingKeys.LicenceNumber6),
            new(this, "", AxicultusSettingKeys.LicenceName7, AxicultusSettingKeys.LicenceNumber7),
            new(this, "", AxicultusSettingKeys.LicenceName8, AxicultusSettingKeys.LicenceNumber8),
        ];
        OnPropertyChanged(string.Empty);
    }

    private void ChangeOption<T>(SettingKey<T> key, T value, [CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(_settings.Get(key), value)) return;
        var result = _settings.Set(key, value);
        ApplyResult(result, IsEditing ? UiText.Get("SettingsWindow", "SaveOnOk") : UiText.Get("SettingsWindow", "SettingsSaved"));
        OnPropertyChanged(property);
    }

    public int BibleSequentialLimit { get => _settings.Get(AxicultusSettingKeys.BibleSequentialLimit); set => ChangeOption(AxicultusSettingKeys.BibleSequentialLimit, value); }
    public int BibleAdHocLimit { get => _settings.Get(AxicultusSettingKeys.BibleAdHocLimit); set => ChangeOption(AxicultusSettingKeys.BibleAdHocLimit, value); }
    public bool MessageAlertFlash { get => _settings.Get(AxicultusSettingKeys.MessageAlertFlash); set => ChangeOption(AxicultusSettingKeys.MessageAlertFlash, value); }
    public bool MessageAlertScroll { get => _settings.Get(AxicultusSettingKeys.MessageAlertScroll); set => ChangeOption(AxicultusSettingKeys.MessageAlertScroll, value); }
    public bool MessageAlertTransparent { get => _settings.Get(AxicultusSettingKeys.MessageAlertTransparent); set => ChangeOption(AxicultusSettingKeys.MessageAlertTransparent, value); }
    public int MessageAlertDuration { get => _settings.Get(AxicultusSettingKeys.MessageAlertDuration); set => ChangeOption(AxicultusSettingKeys.MessageAlertDuration, value); }
    public bool ParentalAlertFlash { get => _settings.Get(AxicultusSettingKeys.ParentalAlertFlash); set => ChangeOption(AxicultusSettingKeys.ParentalAlertFlash, value); }
    public bool ParentalAlertScroll { get => _settings.Get(AxicultusSettingKeys.ParentalAlertScroll); set => ChangeOption(AxicultusSettingKeys.ParentalAlertScroll, value); }
    public bool ParentalAlertTransparent { get => _settings.Get(AxicultusSettingKeys.ParentalAlertTransparent); set => ChangeOption(AxicultusSettingKeys.ParentalAlertTransparent, value); }
    public int ParentalAlertDuration { get => _settings.Get(AxicultusSettingKeys.ParentalAlertDuration); set => ChangeOption(AxicultusSettingKeys.ParentalAlertDuration, value); }
    public string ParentalAlertHeading { get => _settings.Get(AxicultusSettingKeys.ParentalAlertHeading); set => ChangeOption(AxicultusSettingKeys.ParentalAlertHeading, value); }
    public bool PreviewAreaShowNotations { get => _settings.Get(AxicultusSettingKeys.PreviewAreaShowNotations); set => ChangeOption(AxicultusSettingKeys.PreviewAreaShowNotations, value); }
    public int JumpToA { get => _settings.Get(AxicultusSettingKeys.JumpToA); set => ChangeOption(AxicultusSettingKeys.JumpToA, value); }
    public int JumpToB { get => _settings.Get(AxicultusSettingKeys.JumpToB); set => ChangeOption(AxicultusSettingKeys.JumpToB, value); }
    public int JumpToC { get => _settings.Get(AxicultusSettingKeys.JumpToC); set => ChangeOption(AxicultusSettingKeys.JumpToC, value); }
    public int LyricsMonitorFontSize { get => _settings.Get(AxicultusSettingKeys.LyricsMonitorFontSize); set => ChangeOption(AxicultusSettingKeys.LyricsMonitorFontSize, value); }
    public int LyricsMonitorHighlightColorArgb { get => _settings.Get(AxicultusSettingKeys.LyricsMonitorHighlightColorArgb); set => ChangeOption(AxicultusSettingKeys.LyricsMonitorHighlightColorArgb, value); }
    public int ReferenceAlertSource { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertSource); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertSource, value); }
    public int ReferenceAlertDurationSeconds { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertDurationSeconds); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertDurationSeconds, value); }
    public bool LyricsMonitorBold { get => _settings.Get(AxicultusSettingKeys.LyricsMonitorBold); set => ChangeOption(AxicultusSettingKeys.LyricsMonitorBold, value); }
    public bool LyricsMonitorItalic { get => _settings.Get(AxicultusSettingKeys.LyricsMonitorItalic); set => ChangeOption(AxicultusSettingKeys.LyricsMonitorItalic, value); }
    public bool LyricsMonitorUnderline { get => _settings.Get(AxicultusSettingKeys.LyricsMonitorUnderline); set => ChangeOption(AxicultusSettingKeys.LyricsMonitorUnderline, value); }
    public bool ReferenceAlertUsePick { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertUsePick); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertUsePick, value); }
    public bool ReferenceAlertBlankIfPickNotFound { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertBlankIfPickNotFound); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertBlankIfPickNotFound, value); }
    public bool ReferenceAlertFlash { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertFlash); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertFlash, value); }
    public bool ReferenceAlertScroll { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertScroll); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertScroll, value); }
    public bool ReferenceAlertTransparent { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertTransparent); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertTransparent, value); }
    public string ReferenceAlertPickName { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertPickName); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertPickName, value); }
    public string ReferenceAlertPickSubstitute { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertPickSubstitute); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertPickSubstitute, value); }
    public string ReferenceAlertPickSeparator { get => _settings.Get(AxicultusSettingKeys.ReferenceAlertPickSeparator); set => ChangeOption(AxicultusSettingKeys.ReferenceAlertPickSeparator, value); }
    public string PreviewMonitorId { get => _settings.Get(AxicultusSettingKeys.PreviewMonitorId); set => ChangeOption(AxicultusSettingKeys.PreviewMonitorId, value); }

    public IReadOnlyList<string> ReferenceSources { get; } = [UiText.Get("SettingsWindow", "RefNone"), UiText.Get("SettingsWindow", "RefDefault"), UiText.Get("SettingsWindow", "SongNumber"), UiText.Get("SettingsWindow", "BookReference"), UiText.Get("SettingsWindow", "UserReference")];
    public IReadOnlyList<LicenceRow> LicenceRows { get; private set; } = [];

    public bool LicenceOnFirstPage { get => _settings.Get(AxicultusSettingKeys.LicenceOnFirstPage); set => ChangeOption(AxicultusSettingKeys.LicenceOnFirstPage, value); }

    public string LicenceNumberSymbol
    {
        get => _settings.Get(AxicultusSettingKeys.LicenceNumberSymbol);
        set
        {
            ChangeOption(AxicultusSettingKeys.LicenceNumberSymbol, value);
            foreach (var row in LicenceRows) row.RefreshPreview();
        }
    }

    public sealed class LicenceRow(SettingsWindowViewModel editor, string fixedName, SettingKey<string>? name = null,
        SettingKey<string>? number = null) : ObservableObject
    {
        public bool CanEditName => name is not null;
        public bool CanEditNumber => number is not null;
        public string Name
        {
            get => name is null ? fixedName : editor._settings.Get(name);
            set { if (name is not null) editor.ChangeOption(name, value); RefreshPreview(); }
        }
        public string Number
        {
            get => number is null ? "" : editor._settings.Get(number);
            set { if (number is not null) editor.ChangeOption(number, value); RefreshPreview(); }
        }
        // (a public-domain song shows no licence line: GeneralSettings.FormatLicence)
        public string Preview => fixedName == "Public Domain" ? ""
            : string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Number) ? ""
            : string.Concat(Name.Trim(), editor.LicenceNumberSymbol, Number.Trim());
        internal void RefreshPreview() => OnPropertyChanged(nameof(Preview));
    }
}
