using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Easislides.Wpf.Settings;

public sealed partial class SettingsWindowViewModel
{
    internal object ReadOperatorOption(OperatorOption option) => option.Read(_settings.Current.Operator);

    internal void WriteOperatorOption(OperatorOption option, object value)
    {
        if (Equals(ReadOperatorOption(option), value)) return;
        var values = new Dictionary<string, object> { [option.Id] = value };
        if (value is true && option.Id == "operator.GlobalArrow") values["operator.GlobalCtrlArrow"] = false;
        if (value is true && option.Id == "operator.GlobalCtrlArrow") values["operator.GlobalArrow"] = false;
        var result = _settings.SetValues(values);
        ApplyResult(result, IsEditing ? "확인을 누르면 설정을 저장합니다." : "설정 저장됨");
        if (!result.Succeeded) throw new ArgumentException(result.Issues.First().Message);
        OnPropertyChanged(nameof(OperatorPreferences));
        OnPropertyChanged(nameof(KeyboardLayoutGuide));
        OnPropertyChanged(nameof(StageHeight));
    }

    internal void ImportLicenceNames(string path, IReadOnlyDictionary<int,string> names)
    {
        if (string.Equals(_settings.Current.General.LicenceNamesImportedFrom, path, StringComparison.OrdinalIgnoreCase)) return;
        foreach (var (slot, name) in names)
            if (slot is >= 4 and <= 8 && name.Length > 0 && string.IsNullOrWhiteSpace(LicenceRows[slot - 1].Name))
                LicenceRows[slot - 1].Name = name;
        _settings.Set(EasiSettingKeys.LicenceNamesImportedFrom, path);
    }

    public OperatorOptions OperatorPreferences => _settings.Current.Operator;
    public string KeyboardLayoutGuide => OperatorPreferences.AlternateKeyboard
        ? "이전·다음 슬라이드  Page Up / Page Down　 첫·끝 슬라이드  Home / End\n이전·다음 항목  ↑ / ↓　 첫·끝 항목  ← / →"
        : "이전·다음 슬라이드  ↑ / ↓　 첫·끝 슬라이드  ← / →\n이전·다음 항목  Page Up / Page Down　 첫·끝 항목  Home / End";
    public int StageHeight => Easislides.Core.SlideAspectRatio.HeightForWidth(OperatorPreferences.StageWidth, IsMonitorWide);
    public int AudienceHeight => Easislides.Core.SlideAspectRatio.HeightForWidth(DisplayCustomWidth, IsMonitorWide);

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
        ApplyResult(result, "설정 저장됨");
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
            new(this, "CCLI", number: EasiSettingKeys.LicenceNumber3),
            new(this, "", EasiSettingKeys.LicenceName4, EasiSettingKeys.LicenceNumber4),
            new(this, "", EasiSettingKeys.LicenceName5, EasiSettingKeys.LicenceNumber5),
            new(this, "", EasiSettingKeys.LicenceName6, EasiSettingKeys.LicenceNumber6),
            new(this, "", EasiSettingKeys.LicenceName7, EasiSettingKeys.LicenceNumber7),
            new(this, "", EasiSettingKeys.LicenceName8, EasiSettingKeys.LicenceNumber8),
        ];
        OnPropertyChanged(string.Empty);
    }

    private void ChangeOption<T>(SettingKey<T> key, T value, [CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(_settings.Get(key), value)) return;
        var result = _settings.Set(key, value);
        ApplyResult(result, IsEditing ? "확인을 누르면 설정을 저장합니다." : "설정 저장됨");
        OnPropertyChanged(property);
    }

    public int BibleSequentialLimit { get => _settings.Get(EasiSettingKeys.BibleSequentialLimit); set => ChangeOption(EasiSettingKeys.BibleSequentialLimit, value); }
    public int BibleAdHocLimit { get => _settings.Get(EasiSettingKeys.BibleAdHocLimit); set => ChangeOption(EasiSettingKeys.BibleAdHocLimit, value); }
    public bool MessageAlertFlash { get => _settings.Get(EasiSettingKeys.MessageAlertFlash); set => ChangeOption(EasiSettingKeys.MessageAlertFlash, value); }
    public bool MessageAlertScroll { get => _settings.Get(EasiSettingKeys.MessageAlertScroll); set => ChangeOption(EasiSettingKeys.MessageAlertScroll, value); }
    public bool MessageAlertTransparent { get => _settings.Get(EasiSettingKeys.MessageAlertTransparent); set => ChangeOption(EasiSettingKeys.MessageAlertTransparent, value); }
    public int MessageAlertDuration { get => _settings.Get(EasiSettingKeys.MessageAlertDuration); set => ChangeOption(EasiSettingKeys.MessageAlertDuration, value); }
    public bool ParentalAlertFlash { get => _settings.Get(EasiSettingKeys.ParentalAlertFlash); set => ChangeOption(EasiSettingKeys.ParentalAlertFlash, value); }
    public bool ParentalAlertScroll { get => _settings.Get(EasiSettingKeys.ParentalAlertScroll); set => ChangeOption(EasiSettingKeys.ParentalAlertScroll, value); }
    public bool ParentalAlertTransparent { get => _settings.Get(EasiSettingKeys.ParentalAlertTransparent); set => ChangeOption(EasiSettingKeys.ParentalAlertTransparent, value); }
    public int ParentalAlertDuration { get => _settings.Get(EasiSettingKeys.ParentalAlertDuration); set => ChangeOption(EasiSettingKeys.ParentalAlertDuration, value); }
    public string ParentalAlertHeading { get => _settings.Get(EasiSettingKeys.ParentalAlertHeading); set => ChangeOption(EasiSettingKeys.ParentalAlertHeading, value); }
    public bool PreviewAreaShowNotations { get => _settings.Get(EasiSettingKeys.PreviewAreaShowNotations); set => ChangeOption(EasiSettingKeys.PreviewAreaShowNotations, value); }
    public int JumpToA { get => _settings.Get(EasiSettingKeys.JumpToA); set => ChangeOption(EasiSettingKeys.JumpToA, value); }
    public int JumpToB { get => _settings.Get(EasiSettingKeys.JumpToB); set => ChangeOption(EasiSettingKeys.JumpToB, value); }
    public int JumpToC { get => _settings.Get(EasiSettingKeys.JumpToC); set => ChangeOption(EasiSettingKeys.JumpToC, value); }
    public int LyricsMonitorFontSize { get => _settings.Get(EasiSettingKeys.LyricsMonitorFontSize); set => ChangeOption(EasiSettingKeys.LyricsMonitorFontSize, value); }
    public int LyricsMonitorHighlightColorArgb { get => _settings.Get(EasiSettingKeys.LyricsMonitorHighlightColorArgb); set => ChangeOption(EasiSettingKeys.LyricsMonitorHighlightColorArgb, value); }
    public int ReferenceAlertSource { get => _settings.Get(EasiSettingKeys.ReferenceAlertSource); set => ChangeOption(EasiSettingKeys.ReferenceAlertSource, value); }
    public int ReferenceAlertDurationSeconds { get => _settings.Get(EasiSettingKeys.ReferenceAlertDurationSeconds); set => ChangeOption(EasiSettingKeys.ReferenceAlertDurationSeconds, value); }
    public bool LyricsMonitorBold { get => _settings.Get(EasiSettingKeys.LyricsMonitorBold); set => ChangeOption(EasiSettingKeys.LyricsMonitorBold, value); }
    public bool LyricsMonitorItalic { get => _settings.Get(EasiSettingKeys.LyricsMonitorItalic); set => ChangeOption(EasiSettingKeys.LyricsMonitorItalic, value); }
    public bool LyricsMonitorUnderline { get => _settings.Get(EasiSettingKeys.LyricsMonitorUnderline); set => ChangeOption(EasiSettingKeys.LyricsMonitorUnderline, value); }
    public bool ReferenceAlertUsePick { get => _settings.Get(EasiSettingKeys.ReferenceAlertUsePick); set => ChangeOption(EasiSettingKeys.ReferenceAlertUsePick, value); }
    public bool ReferenceAlertBlankIfPickNotFound { get => _settings.Get(EasiSettingKeys.ReferenceAlertBlankIfPickNotFound); set => ChangeOption(EasiSettingKeys.ReferenceAlertBlankIfPickNotFound, value); }
    public bool ReferenceAlertFlash { get => _settings.Get(EasiSettingKeys.ReferenceAlertFlash); set => ChangeOption(EasiSettingKeys.ReferenceAlertFlash, value); }
    public bool ReferenceAlertScroll { get => _settings.Get(EasiSettingKeys.ReferenceAlertScroll); set => ChangeOption(EasiSettingKeys.ReferenceAlertScroll, value); }
    public bool ReferenceAlertTransparent { get => _settings.Get(EasiSettingKeys.ReferenceAlertTransparent); set => ChangeOption(EasiSettingKeys.ReferenceAlertTransparent, value); }
    public string ReferenceAlertPickName { get => _settings.Get(EasiSettingKeys.ReferenceAlertPickName); set => ChangeOption(EasiSettingKeys.ReferenceAlertPickName, value); }
    public string ReferenceAlertPickSubstitute { get => _settings.Get(EasiSettingKeys.ReferenceAlertPickSubstitute); set => ChangeOption(EasiSettingKeys.ReferenceAlertPickSubstitute, value); }
    public string ReferenceAlertPickSeparator { get => _settings.Get(EasiSettingKeys.ReferenceAlertPickSeparator); set => ChangeOption(EasiSettingKeys.ReferenceAlertPickSeparator, value); }
    public string PreviewMonitorId { get => _settings.Get(EasiSettingKeys.PreviewMonitorId); set => ChangeOption(EasiSettingKeys.PreviewMonitorId, value); }

    public IReadOnlyList<string> ReferenceSources { get; } = ["표시 안 함", "기본 참조", "곡 번호", "책 참조", "사용자 참조"];
    public IReadOnlyList<LicenceRow> LicenceRows { get; private set; } = [];

    public bool LicenceOnFirstPage { get => _settings.Get(EasiSettingKeys.LicenceOnFirstPage); set => ChangeOption(EasiSettingKeys.LicenceOnFirstPage, value); }

    public string LicenceNumberSymbol
    {
        get => _settings.Get(EasiSettingKeys.LicenceNumberSymbol);
        set
        {
            ChangeOption(EasiSettingKeys.LicenceNumberSymbol, value);
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
        public string Preview => fixedName == "Public Domain" ? fixedName
            : string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Number) ? ""
            : string.Concat(Name.Trim(), editor.LicenceNumberSymbol, Number.Trim());
        internal void RefreshPreview() => OnPropertyChanged(nameof(Preview));
    }
}
